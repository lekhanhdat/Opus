/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Collector;
using RAFileSystem.FileSystem.DataSync.V2;
using RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies;
using RAFileSystemCore.Utils;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services
{
    /// <summary>
    /// Analyzes files against disposal rules and handles manual approval logic.
    /// </summary>
    public class DisposalFileAnalyzer
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(DisposalFileAnalyzer));

        private readonly DisposalDtoFactory _dtoFactory;
        private readonly DisposalReportService _reportService;
        private readonly MemoryListCacheService<FSAzureTableEntityDto> _deleteManualItemCache;
        private readonly MemoryListCacheService<FSAzureTableEntityDto> _rejectItemCache;

        private readonly FSDisposalChannelProvider _channel;

        public DisposalFileAnalyzer(
            DisposalDtoFactory dtoFactory,
            DisposalReportService reportService,
            FSDisposalChannelProvider channel)
        {
            _deleteManualItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            _rejectItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            _dtoFactory = dtoFactory;
            _reportService = reportService;
            _channel = channel;
        }
        
        public async Task ProcessFilesBatchAsync(List<FSAzureTableEntityDto> tempRecords)
        {
            if (tempRecords == null || tempRecords.Count == 0) return;

            var fileIds = tempRecords.Select(r => r.FilePathMd5).ToList();
            Dictionary<Guid, FileSystemRecordDto> azureRecords;
            using (new AgentPerformanceScope("FSDicover.GetFSManualRecords.Channel", addToStatistics: true))
            {
                azureRecords = JobContext.Current.ApiClient.GetFSManualRecords(fileIds).ToDictionary(r => r.NodeId);
            }

            foreach (var dto in tempRecords)
            {
                try
                {
                    if (string.IsNullOrEmpty(dto.RuleId) || !FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId)))
                    {
                        _reportService.AddSkipReport(dto);
                        dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                        dto.ScanTime = DateTime.UtcNow;
                        await _channel.WriteToCosmosAsync(dto).ConfigureAwait(false);
                        continue;
                    }

                    var rule = FSJobCache.Instance.Rules[new Guid(dto.RuleId)];
                    var id = dto.FilePathMd5;
                    

                    if (azureRecords.TryGetValue(id, out var azureRecord))
                    {
                        dto.MovedToApprovalTable = true;
                        dto.Status = azureRecord.ManualApprovedStatus;
                        dto.ScanTime = DateTime.UtcNow;

                        if (azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
                        {
                            await HandleSameRuleApprovalStatus(dto, azureRecord, rule);
                        }
                        else
                        {
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.MovedToApprovalTable = false;
                            dto.ScanTime = DateTime.UtcNow;
                            await _channel.WriteToCosmosAsync(dto).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        _reportService.AddSkipReport(dto);
                        dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                        dto.ScanTime = DateTime.UtcNow;
                        await _channel.WriteToCosmosAsync(dto).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error in reconcile for {0}: {1}", dto?.FullPath, e.ToString());
                    _reportService.ReportFailedFile(dto, e);
                }
            }
        }


        /// <summary>
        /// Analyzes a batch of files against disposal rules and manual approval status.
        /// </summary>
        public async Task AnalyzeFiles(
            FileSystemCollectionFolder folder,
            List<FSFileStub> files,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            foreach (var fileStub in files)
            {
                using (new AgentPerformanceScope("FSDocumentDisposal.AnalyzeFile", addToStatistics: true))
                {
                    await AnalyzeSingleFile(folder,fileStub, azureCache);
                }
            }
        }

        /// <summary>
        /// Analyzes files inherited from folder term assignment and pushes matched results to channels.
        /// </summary>
        public async Task AnalyzeFilesFromFolderAsync(
            List<FSFileStub> files,
            FSFolderStub folder,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            var termId = DisposalExecutionStrategyV3.CurrentSetting.DefaultTermId;
            var isHold = false;
            string termName;

            using (new AgentPerformanceScope("FSDiscover.Process.AssignFolderTermId", addToStatistics: true))
            {
                var existingFolder = FSJobCache.Instance.DisposalDifferentFolderCache
                    .FirstOrDefault(a => a.NodeId == folder.SelfId);

                if (existingFolder != null)
                {
                    termId = existingFolder.TermId;
                    _logger.Info("Different term {0} on folder {1}", termId, folder.FullPath.LogBase64());
                    isHold = existingFolder.HoldStatus && existingFolder.HoldReleaseTime > DateTime.UtcNow.Ticks;
                }

                if (termId == Guid.Empty)
                {
                    _logger.Warn("Term is empty on folder {0}", folder.FullPath.LogBase64());
                    return;
                }

                termName = FSJobCache.Instance.Terms.ContainsKey(termId)
                    ? FSJobCache.Instance.Terms[termId].Name
                    : null;
            }

            foreach (var file in files)
            {
                _logger.Debug("Start to process file.id :{0}", file.FullPath.ToLowerInvariant().ToMd5());

                azureCache.TryGetValue(file.SelfId, out var fileRecord);
                if (fileRecord == null)
                {
                    _logger.Debug("File record not found for file id: {0}", file.SelfId);
                }

                using (new AgentPerformanceScope("FSDocumentDisposal.AnalyzeFileFromFolder", addToStatistics: true))
                {
                    await AnalyzeSingleFileFromFolderAsync(folder, file, fileRecord, termId, termName, isHold);
                }
            }
        }
        

        private async Task AnalyzeSingleFileFromFolderAsync(
            FSFolderStub folder,
            FSFileStub fileStub,
            FileSystemRecordDto fileRecord,
            Guid termId,
            string termName,
            bool isHold)
        {
            _logger.Debug("Start to analyze file from folder, file id:{0}, folder {1}", fileStub?.SelfId, folder.SelfId);
            FSAzureTableEntityDto dto = null;

            try
            {
                if (!FSJobCache.Instance.TermRuleMapping.TryGetValue(termId, out var rules))
                {
                    _logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", termId);
                    return;
                }

                var filteredRules = RuleUtil
                    .FilterMoveRules(rules, Path.GetDirectoryName(fileStub.FullPath))
                    .Where(x => x.FSRule != null)
                    .ToList();

                if (filteredRules.Count == 0)
                {
                    _logger.Info("Current Term[{0}] doesn't have FS rule so skip check rule. FSPath:{1}.",
                        termId,
                        fileStub.FullPath.LogBase64());
                    return;
                }

                var hasOwnerRule = HasOwnerRule(filteredRules);
                var engine = new DisposalRuleEngine(filteredRules);
                var filterObject = ObjectConverter.ConvertXObject2FilterObject(
                    new XFileInfoEx(fileStub.MediaObj),
                    FSJobCache.Instance.RootPath,
                    hasOwnerRule);

                var matchedRule = engine.MatchPotentialRule(filterObject);
                if (matchedRule?.Item1 == null || string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                {
                    _logger.Info("Current file not match rule. FSPath:{0}.", fileStub.FullPath.LogBase64());
                    return;
                }

                var rule = matchedRule.Item1;
                if (fileRecord != null)
                {
                    fileRecord.RuleName = rule.Name;
                }

                if (DisposalRuleUtility.IsRemoveRule(rule) && isHold)
                {
                    _logger.Info("This file is on hold and current rule is remove rule, will be skipped.");
                    _reportService.AddSkipReportForHold(fileStub);
                    return;
                }

                dto = _dtoFactory.CreateDto(fileStub, rule, folder, fileRecord,termId,termName);

                if (!rule.FSRule.IsManualApproval)
                {
                    dto.ScanTime = DateTime.UtcNow;
                    await _channel.WriteToWorkerAsync((dto, fileRecord));
                    return;
                }

                await _channel.ManualInFolderToCosmoChannel.Writer.WriteAsync(dto);
            }
            catch (Exception e)
            {
                _logger.Error("Error occurred while disposal file:{0} Error:{1}", fileStub.FullPath.LogBase64(), e);
                _reportService.ReportFailedFile(dto, fileStub, folder, e);
            }
        }

        private static bool HasOwnerRule(List<Rule> rules)
        {
            var fsRules = rules.Where(r => r.FSRule != null).Select(r => r.FSRule).ToList();
            if (!fsRules.Any())
            {
                return false;
            }

            return fsRules.Any(r => r.Filters.Any(f => f.Rule is AvePoint.GCommon.Contract.CommonFilter.OwnerRule));
        }

        private async Task AnalyzeSingleFile(
            FileSystemCollectionFolder folder,
            FSFileStub fileStub,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            _logger.Debug("Start to analyze file, file id:{0}", fileStub.SelfId);
            var id = fileStub.SelfId;

            if (!azureCache.TryGetValue(id, out var dbRecord))
            {
                _logger.Debug("No db record found:{0}", id);
                return;
            }

            try
            {
                await ProcessFileWithDbRecord(fileStub, dbRecord, azureCache);
            }
            catch (Exception e)
            {
                _logger.Error("Error occurred while disposal file:{0} Error:{1}",
                    ExternalUtil.CombinePath(dbRecord.DirPath.LogBase64(), dbRecord.LeafName.LogBase64()),
                    e.ToString());
                _reportService.ReportFailedFile(folder,fileStub, dbRecord, e);
            }
        }

        private async Task ProcessFileWithDbRecord(
            FSFileStub fileStub,
            FileSystemRecordDto dbRecord,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            if (dbRecord.TermId == null || dbRecord.TermId.Equals(Guid.Empty))
            {
                return;
            }

            if (!FSJobCache.Instance.TermRuleMapping.ContainsKey(dbRecord.TermId))
            {
                _logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", dbRecord?.TermId);
                return;
            }

            var matchedRule = MatchRule(fileStub, dbRecord);
            if (matchedRule == null)
            {
                HandleUnmatchedFile(fileStub, dbRecord, azureCache);
                return;
            }

            await ProcessMatchedRule(fileStub, matchedRule, dbRecord, azureCache);
        }

        private Rule MatchRule(FSFileStub fileStub, FileSystemRecordDto dbRecord)
        {
            var rules = FSJobCache.Instance.TermRuleMapping[dbRecord.TermId];
            var filteredRules = RuleUtil
                .FilterMoveRules(rules, Path.GetDirectoryName(fileStub.FullPath))
                .Where(x => x.FSRule != null)
                .ToList();

            if (filteredRules.Count == 0)
            {
                _logger.Debug("Current Term[{0}] doesn't have FS rule so skip check rule. FSPath:{1}",
                    dbRecord.TermId, fileStub.FullPath.LogBase64());
                return null;
            }

            var engine = new DisposalRuleEngine(filteredRules);
            var filterObject = ObjectConverter.ConvertXObject2FilterObjectV2(
                new XFileInfoEx(fileStub.MediaObj), dbRecord, FSJobCache.Instance.RootPath);

            var result = engine.MatchPotentialRule(filterObject);
            if (result?.Item1 != null && !string.IsNullOrWhiteSpace(result.Item1.Id))
            {
                return result.Item1;
            }

            return null;
        }

        private void HandleUnmatchedFile(
            FSFileStub fileStub,
            FileSystemRecordDto dbRecord,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            if (azureCache.ContainsKey(fileStub.SelfId))
            {
                _logger.Debug("Current file not match rule and has manual record. FSPath:{0}",
                    fileStub.FullPath.LogBase64());
                var manualData = _dtoFactory.CreateDto(fileStub, null, dbRecord);
                RemoveManualData(manualData);
            }
            else
            {
                _logger.Debug("Current file not match rule. FSPath:{0}", fileStub.FullPath.LogBase64());
            }
        }

        private async Task ProcessMatchedRule(
            FSFileStub fileStub,
            Rule rule,
            FileSystemRecordDto dbRecord,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            if (DisposalRuleUtility.IsRemoveRule(rule) && IsFileOnHold(dbRecord))
            {
                _logger.Info("This file is on hold and current rule is remove rule, will be skipped.");
                _reportService.AddSkipReportForHold(fileStub);
                return;
            }

            var dto = _dtoFactory.CreateDto(fileStub, rule, dbRecord);

            if (rule.FSRule.IsManualApproval)
            {
                await HandleManualApproval(fileStub, dto, rule, dbRecord, azureCache);
            }
            else
            {
                _logger.Info("Not manual rule, file id:{0}", fileStub?.SelfId);
                dto.ScanTime = DateTime.UtcNow;
                await _channel.WriteToWorkerAsync((dto, dbRecord));
            }
        }

        private static bool IsFileOnHold(FileSystemRecordDto dbRecord)
        {
            return dbRecord.HoldStatus && dbRecord.HoldReleaseTime > DateTime.UtcNow.Ticks;
        }

        private async Task HandleManualApproval(
            FSFileStub fileStub,
            FSAzureTableEntityDto dto,
            Rule rule,
            FileSystemRecordDto dbRecord,
            Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            if (!azureCache.TryGetValue(fileStub.SelfId, out var azureRecord))
            {
                await HandleNoAzureRecord(fileStub, dto);
                return;
            }

            dto.Status = azureRecord.ManualApprovedStatus;
            dto.ScanTime = DateTime.UtcNow;

            if (azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
            {
                await HandleSameRuleApprovalStatus(fileStub, dto, azureRecord, dbRecord);
            }
            else
            {
                await HandleRuleChanged(fileStub, dto);
            }
        }

        private async Task HandleNoAzureRecord(FSFileStub fileStub, FSAzureTableEntityDto dto)
        {
            _logger.Debug("Azure table not exist, file path:{0}", fileStub?.SelfId);
            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
            dto.ScanTime = DateTime.UtcNow;
            await _channel.WriteToCosmosAsync(dto);
        }

        private async Task HandleSameRuleApprovalStatus(
            FSFileStub fileStub,
            FSAzureTableEntityDto dto,
            FileSystemRecordDto azureRecord,
            FileSystemRecordDto dbRecord)
        {
            var status = (SOApproveDBStatus)azureRecord.ManualApprovedStatus;

            switch (status)
            {
                case SOApproveDBStatus.None:
                    _logger.Debug("cosmos record not exist, file id:{0}", fileStub?.SelfId);
                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                    dto.ScanTime = DateTime.UtcNow;
                    await _channel.WriteToCosmosAsync(dto);
                    break;

                case SOApproveDBStatus.Approved:
                    _logger.Debug("File approved, file id:{0}", fileStub?.SelfId);
                    await _channel.WriteToWorkerAsync((dto,dbRecord)).ConfigureAwait(false);
                    break;

                case SOApproveDBStatus.KeepData:
                case SOApproveDBStatus.CheckOption:
                case SOApproveDBStatus.WaitingApprove:
                    _logger.Debug("File status is {0}, file id:{1}", azureRecord.ManualApprovedStatus, fileStub?.SelfId);
                    _reportService.AddSkipReportForApproval(dto);
                    break;

                case SOApproveDBStatus.Rejected:
                    await HandleRejectedStatus(fileStub, dto, dbRecord);
                    break;

                default:
                    _logger.Warn("Invalid status. File id:{0}", fileStub?.SelfId);
                    break;
            }
        }

        private async Task HandleSameRuleApprovalStatus(
            FSAzureTableEntityDto dto,
            FileSystemRecordDto azureRecord,
            Rule rule)
        {
            switch (azureRecord.ManualApprovedStatus)
            {
                case (int)SOApproveDBStatus.None:
                    _reportService.AddSkipReport(dto);
                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                    dto.ScanTime = DateTime.UtcNow;
                    await _channel.WriteToCosmosAsync(dto);
                    break;
                case (int)SOApproveDBStatus.Approved:
                    if (string.IsNullOrEmpty(azureRecord.RuleName))
                    {
                        azureRecord.RuleName = string.IsNullOrEmpty(azureRecord.ManualRuleName)
                            ? rule.Name
                            : azureRecord.ManualRuleName;
                    }

                    await _channel.WriteToWorkerAsync((dto, azureRecord));
                    break;
                case (int)SOApproveDBStatus.KeepData:
                case (int)SOApproveDBStatus.CheckOption:
                case (int)SOApproveDBStatus.WaitingApprove:
                    _reportService.AddSkipReport(dto);
                    break;
                case (int)SOApproveDBStatus.Rejected:
                    if (azureRecord.IsManualSynced && azureRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                    {
                        break;
                    }

                    AddRejectFileToAzureTable(dto);
                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                    dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
                    await _channel.WriteToCosmosAsync(dto);
                    break;
                default:
                    _logger.Warn("Invalid status. File id:{0}", dto.FilePathMd5);
                    break;
            }
        }

        private async Task HandleRejectedStatus(
            FSFileStub fileStub,
            FSAzureTableEntityDto dto,
            FileSystemRecordDto dbRecord)
        {
            if (dbRecord.IsManualSynced && dbRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
            {
                _logger.Debug("Item is manualsync and its extend");
                return;
            }

            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
            dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
            await _channel.WriteToCosmosAsync(dto);
            _logger.Debug("File status is Rejected, file id:{0}", fileStub?.SelfId);
        }

        private async Task HandleRuleChanged(FSFileStub fileStub, FSAzureTableEntityDto dto)
        {
            _logger.Debug("File rule id changed, file id:{0}", fileStub?.SelfId);
            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
            dto.MovedToApprovalTable = false;
            dto.ScanTime = DateTime.UtcNow;
            await _channel.WriteToCosmosAsync(dto);
        }

        private void RemoveManualData(FSAzureTableEntityDto dto)
        {
            _deleteManualItemCache.Add(dto);
            if (_deleteManualItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = _deleteManualItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("FSDicover.RemoveManualData", $"FSDicover.RemoveManualData.Count:{tempEntities.Count}", true))
                    {
                        JobContext.Current.ApiClient.RemoveManualData(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("An error occurred while removing manual data. Error:{0}", e.ToString());
                }
            }
        }
        
        private void AddRejectFileToAzureTable(FSAzureTableEntityDto dto)
        {
            _rejectItemCache.Add(dto);
            if (_rejectItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = _rejectItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("FSDicover.AddRejectScanData", $"FSDicover.AddRejectScanData.Count:{tempEntities.Count}", true))
                    {
                        List<Guid> failedGuids = JobContext.Current.ApiClient.AddRejectScanData(tempEntities);
                        _reportService.AddRejectToReports(tempEntities, failedGuids);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("An error occured while adding reject data. Error:{0}", e.ToString());
                    _reportService.AddRejectToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
                }
            }
        }
        
        public void FinalAddRejectFileToAzureTable()
        {
            var tempEntities = _rejectItemCache.TakeAll().ToList();
            if (tempEntities.Count > 0)
            {
                try
                {
                    using (new AgentPerformanceScope("FSDicover.AddRejectScanData.Final", "FSDicover.AddRejectScanData.Count:" + tempEntities.Count, true))
                    {
                        List<Guid> failedGuids = JobContext.Current.ApiClient.AddRejectScanData(tempEntities);
                        _reportService.AddRejectToReports(tempEntities, failedGuids);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("An error occurred while final adding reject data. Error:{0}", e.ToString());
                    _reportService.AddRejectToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
                }
            }
        }
        
        public void FinalRemoveManualData()
        {
            var tempEntities = _deleteManualItemCache.TakeAll().ToList();
            if (tempEntities.Count > 0)
            {
                try
                {
                    using (new AgentPerformanceScope("FSDicover.RemoveManualData.Final", "FSDicover.RemoveManualData.Count:" + tempEntities.Count, true))
                    {
                        JobContext.Current.ApiClient.RemoveManualData(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("An error occurred while final remove manual data. Error:{0}", e.ToString());
                }
            }
        }

        public void Finish()
        {
            _channel.ManualInFolderToCosmoChannel.Writer.TryComplete();
        }
    }
}

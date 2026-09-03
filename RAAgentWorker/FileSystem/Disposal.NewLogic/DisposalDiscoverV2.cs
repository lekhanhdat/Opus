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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystem.Disposal;
using RAFileSystem.Disposal.NewLogic;
using RAFileSystem.FileSystem.Collector;
using RAFileSystem.FileSystem.FileSystem.Collector.FilterImplement;
using RAFileSystem.Utils;
using RAFileSystemCore.Utils;

namespace AvePoint.RA.FileSystem.Collect.NewLogic
{
    internal class DisposalDiscoverV2
    {
        private readonly AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem _system;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private readonly MemoryListCacheService<FSAzureTableEntityDto> DeleteManualItemCache;
        private readonly MemoryListCacheService<FSAzureTableEntityDto> RejectItemCache;

        // Channel outputs
        private readonly ChannelWriter<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> workerWriter;
        private readonly ChannelReader<FSAzureTableEntityDto> cosmosReader;
        private readonly ChannelWriter<FSAzureTableEntityDto> cosmosWriter;
        private readonly ChannelReader<FSAzureTableEntityDto> manualReader;
        private readonly ChannelWriter<FSAzureTableEntityDto> manualWriter;

        public DisposalDiscoverV2()
        {
            DeleteManualItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            RejectItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);

            workerWriter = FSJobCache.Instance.DiscoveryToWorker.Writer;
            cosmosReader = FSJobCache.Instance.DiscoveryToCosmos.Reader;
            cosmosWriter = FSJobCache.Instance.DiscoveryToCosmos.Writer;
            manualReader = FSJobCache.Instance.ManualInFolderToCosmos.Reader;
            manualWriter = FSJobCache.Instance.ManualInFolderToCosmos.Writer;
        }

        public async Task Run()
        {
            try
            {
                logger.Info("Classification Level {0}", FSDataDisposalV2.ClassificationLevel);
                if (FSDataDisposalV2.ClassificationLevel != NodeLevel.FSFolder)
                {
                    await RunFromFileLevelClassify();
                }
                else
                {
                    await RunFromFolderLevelClassify();
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to discover the files. Exception:{0}", ex.ToString());
            }
            finally
            {
                try
                {
                    FinalAddRejectFileToAzureTable();
                    FinalRemoveManualData();

                    manualWriter?.TryComplete();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final update. Error:{0}", e.ToString());
                }
            }
        }

        private async Task RunFromFolderLevelClassify()
        {
            logger.Info("Run from folder level");

            var filter = new FileSystemFolderDisposalFilter(FSJobCache.Instance.BreakNodeUrls, FSJobCache.Instance.RunningJobNodeUrls, FSJobCache.Instance.ScopeSettingCache);

            var collector = new FileSystemCollector(FSJobCache.Instance.RootPath, filter);

            _ = collector.StartAsync(FSJobCache.Instance.RunJobScopePath).ConfigureAwait(false);

            foreach (var folder in collector.CollectAsync())
            {
                string fullPath = folder.FullPath;
                var folderId = fullPath.ToLowerInvariant().ToMd5();
                try
                {
                    if (folder.ChildrenFilesCount > 0)
                    {
                        FSFolderStub rootStub = new FSFolderStub()
                        {
                            MediaObj = folder.CurrentItem,
                            FullPath = fullPath,
                            SelfId = fullPath.ToLowerInvariant().ToMd5(),
                            //ParentId = parentId, 
                            ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId
                        };
                        var azureRecords = GetDBRecordsByFolder(folderId.ToString());
                        await AnalyzeFileFromFolder(folder.ChildrenFiles, rootStub, azureRecords);
                    }
                }
                catch (Exception itemex)
                {
                    logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FullPath, itemex.ToString());
                    ProgressService.Increase();
                    JobDetailService.Commit(new JMFSDisposalJobDetailV2
                    {
                        ObjectName = Path.GetFileName(folder.FullPath),
                        SourceLocation = folder.FullPath,
                        FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JM_FSFailedToDiscoverFolder",
                        Type = "RM_JS_Rule_ObjectLevel_FSFolder",
                        Depth = folder.Depth,
                        DirPath = folder.FullPath,
                        DetailAction = (int)DetailAction.Scan,
                    });
                    FSJobCache.Instance.FailedCount++;
                }
            }

            collector.Dispose();
        }

        private async Task RunFromFileLevelClassify()
        {
            var validFolderPaths = FSJobCache.Instance.DisposalFolderCache.TakeAll().Select(i => i.FolderPath);
            var filter = new FileSystemFileDisposalFilter(validFolderPaths);

            var collector = new FileSystemCollector(FSJobCache.Instance.RootPath, filter);

            _ = collector.StartAsync(FSJobCache.Instance.RunJobScopePath).ConfigureAwait(false);

            foreach (var folder in collector.CollectAsync())
            {
                try
                {
                    if (folder.ChildrenFilesCount > 0)
                    {
                        ProgressService.IncreaseBase(folder.ChildrenFilesCount);
                        string fullPath = folder.FullPath;
                        var folderId = fullPath.ToLowerInvariant().ToMd5();
                        var azureRecords = GetDBRecordsByFolder(folderId.ToString());
                        await AnalyzeFile(folder, azureRecords, folderId);
                    }
                }
                catch (Exception itemex)
                {
                    logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FullPath, itemex.ToString());
                    ProgressService.Increase();
                    JobDetailService.Commit(new JMFSDisposalJobDetailV2
                    {
                        ObjectName = Path.GetFileName(folder.FullPath),
                        SourceLocation = folder.FullPath,
                        FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JM_FSFailedToDiscoverFolder",
                        Type = "RM_JS_Rule_ObjectLevel_FSFolder",
                        Depth = folder.Depth,
                        DirPath = folder.FullPath,
                        DetailAction = (int)DetailAction.Scan,
                    });
                    FSJobCache.Instance.FailedCount++;
                }
            }

            collector.Dispose();
        }

        private Dictionary<Guid, FileSystemRecordDto> GetDueRecordsInFolder(Guid folderId)
        {
            using (new AgentPerformanceScope("FSDisposal.GetDueRecordsInFolder", addToStatistics: true))
            {
                FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                Dictionary<Guid, FileSystemRecordDto> folderRecords = new Dictionary<Guid, FileSystemRecordDto>();
                int index = 0;
                int pageSize = 1000;
                bool hasMore = true;
                List<FileSystemRecordDto> records = null;
                do
                {
                    using (new AgentPerformanceScope("FSDiscover.QueryAllByPage", addToStatistics: true))
                    {
                        records = fileSystemSqliteWrapper.QueryAllByPage(index, pageSize, folderId);
                    }

                    if (records != null && records.Count > 0)
                    {
                        index++;
                        foreach (var item in records)
                        {
                            if (folderRecords.ContainsKey(item.NodeId))
                            {
                                logger.Warn("Duplicate record in due db. NodeId:{0}, LeafName: {1}", item.NodeId, item.LeafName.LogBase64());
                                continue;
                            }
                            folderRecords[item.NodeId] = item;
                        }
                        hasMore = true;
                    }
                    else
                    {
                        hasMore = false;
                    }
                } while (hasMore);
                logger.Info($"Get due records in folder:{folderId} finished. Count:{folderRecords.Count}");
                return folderRecords;
            }
        }

        public async Task RunSendRecordsToCosmos()
        {
            var buffer = new List<FSAzureTableEntityDto>(ExternalUtil.TransferDataCount);

            while (await cosmosReader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (cosmosReader.TryRead(out var dto))
                {
                    buffer.Add(dto);
                    if (buffer.Count >= ExternalUtil.TransferDataCount)
                    {
                        await FlushCosmosAsync(buffer).ConfigureAwait(false);
                        buffer.Clear();
                    }
                }
            }

            if (buffer.Count > 0)
            {
                await FlushCosmosAsync(buffer).ConfigureAwait(false);
            }

            JobContext.Current.SendDataToCosmosFinish = true;
            JobContext.Current.SendDataToAzureTableFinish = true;
        }

        public async Task RunGetRecordsFromCosmos()
        {
            var buffer = new List<FSAzureTableEntityDto>(ExternalUtil.TransferDataCount);

            while (await manualReader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (manualReader.TryRead(out var dto))
                {
                    buffer.Add(dto);
                    if (buffer.Count >= ExternalUtil.TransferDataCount)
                    {
                        await ProcessFilesBatchAsync(buffer).ConfigureAwait(false);
                        buffer.Clear();
                    }
                }
            }

            if (buffer.Count > 0)
            {
                await ProcessFilesBatchAsync(buffer).ConfigureAwait(false);
            }

            workerWriter?.TryComplete();
        }

        private async Task FlushCosmosAsync(List<FSAzureTableEntityDto> batch)
        {
            if (batch == null || batch.Count == 0) return;

            FSAzureTableEntityDtoWithJobId dtoInfo = new FSAzureTableEntityDtoWithJobId
            {
                JobId = JobContext.Current.JobId,
                EntityDtos = batch,
                IsFSHighPerformanceMode = true,
            };

            List<Guid> failedIds;
            using (new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{batch.Count}", true))
            {
                failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);
                //FSJobCache.Instance.DataIngestionDataCollector.WriteData

                var archived = batch.Where(a => a.Status == (int)SOApproveDBStatus.Archived).ToList();
                if (archived.Count > 0)
                {
                    HybridApiClient.Instance.AddScanData(archived);
                }
            }

            var sendable = batch.Where(e => !e.NoNeedSendReport).ToList();
            if (sendable.Count > 0)
            {
                var details = new List<JMFSDisposalJobDetailV2>(sendable.Count);
                foreach (var entity in sendable)
                {
                    var detail = new JMFSDisposalJobDetailV2
                    {
                        ObjectName = entity.LowName,
                        Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                        FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                        Action = GetActionString(entity.RuleAction),
                        SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                        RuleName = !string.IsNullOrEmpty(entity.RuleId) && FSJobCache.Instance.Rules.ContainsKey(new Guid(entity.RuleId))
                            ? FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name
                            : "",
                        Type = "RM_JS_Rule_ObjectLevel_FSFile",
                        AgentName = OSInformation.HostName,
                        Status = failedIds.Contains(entity.FilePathMd5) ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                        Comment = failedIds.Contains(entity.FilePathMd5)
                            ? "RM_JM_FSFailedAddToArchiverTable"
                            : "RM_JM_FSFileWaitingForApproval",
                        Depth = entity.Depth,
                        DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName),
                        DetailAction = (int)DetailAction.UpdateManual,
                    };
                    details.Add(detail);
                }
                JobContext.Current.JobDetailManager.Create().CommitBatch(details);
            }

            if (failedIds.Count > 0)
            {
                AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
                         .Warn("Failed to add fs archived data to cosmos. File ids:{0}", string.Join(",", failedIds));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task ProcessFilesBatchAsync(List<FSAzureTableEntityDto> tempRecords)
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
                        AddSkipReport(dto);
                        dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                        dto.ScanTime = DateTime.UtcNow;
                        await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                        continue;
                    }

                    var rule = FSJobCache.Instance.Rules[new Guid(dto.RuleId)];
                    //fileRecord.RuleName = rule.Name;
                    var id = dto.FilePathMd5;

                    //if (!rule.FSRule.IsManualApproval)
                    //{
                    //    dto.ScanTime = DateTime.UtcNow;
                    //    workerWriter?.TryWrite((dto, fileRecord));
                    //    continue;
                    //}

                    if (azureRecords.ContainsKey(id))
                    {
                        var azureRecord = azureRecords[id];
                        dto.MovedToApprovalTable = true;
                        dto.Status = azureRecord.ManualApprovedStatus;
                        dto.ScanTime = DateTime.UtcNow;

                        if (azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
                        {
                            switch (azureRecord.ManualApprovedStatus)
                            {
                                case (int)SOApproveDBStatus.None:
                                    AddSkipReport(dto);
                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                    dto.ScanTime = DateTime.UtcNow;
                                    await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                                    break;
                                case (int)SOApproveDBStatus.Approved:
                                    if (string.IsNullOrEmpty(azureRecord.RuleName))
                                    {
                                        azureRecord.RuleName = string.IsNullOrEmpty(azureRecord.ManualRuleName)
                                        ? rule.Name
                                        : azureRecord.ManualRuleName;
                                    }
                                    await workerWriter.WriteAsync((dto, azureRecord)).ConfigureAwait(false);
                                    break;
                                case (int)SOApproveDBStatus.KeepData:
                                case (int)SOApproveDBStatus.CheckOption:
                                case (int)SOApproveDBStatus.WaitingApprove:
                                    AddSkipReport(dto);
                                    break;
                                case (int)SOApproveDBStatus.Rejected:
                                    if (azureRecord.IsManualSynced && azureRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                                    {
                                        break;
                                    }
                                    AddRejectFileToAzureTable(dto);
                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                    dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
                                    await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                                    break;
                                default:
                                    AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
                                             .Warn("Invalid status. File id:{0}", id);
                                    break;
                            }
                        }
                        else
                        {
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.MovedToApprovalTable = false;
                            dto.ScanTime = DateTime.UtcNow;
                            await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        AddSkipReport(dto);
                        dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                        dto.ScanTime = DateTime.UtcNow;
                        await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
                             .Error("Error in reconcile for {0}: {1}", dto?.FullPath, e.ToString());

                    var detail = new JMFSDisposalJobDetailV2
                    {
                        ObjectName = dto.LowName,
                        SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName, dto.LowName),
                        Size = ExternalUtil.ConvertToFormatSize(dto.Size),
                        FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                        Action = GetActionString(dto.RuleAction),
                        RuleName = !string.IsNullOrEmpty(dto.RuleId) && FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId))
                            ? FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name
                            : "",
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                        Type = "RM_JS_Rule_ObjectLevel_FSFile",
                        AgentName = OSInformation.HostName,
                        Depth = dto.Depth,
                        DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName),
                        DetailAction = (int)DetailAction.Scan,
                    };
                    JobDetailService.Commit(detail);
                    FSJobCache.Instance.FailedCount++;
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private Dictionary<Guid, FileSystemRecordDto> GetDBRecordsByFolder(string folderId)
        {
            using (new AgentPerformanceScope("FSDiscover.GetAzureDataByFolder", addToStatistics: true))
            {
                Dictionary<Guid, FileSystemRecordDto> folderRecords = new Dictionary<Guid, FileSystemRecordDto>();
                long sortTicks = DateTime.MinValue.Ticks;
                while (true)
                {
                    var data = JobContext.Current.ApiClient.GetDBRecordsByFolderAndFilterByEndTime(folderId, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(), sortTicks, ExternalUtil.TransferDataCount);
                    if (data != null && data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            if (folderRecords.ContainsKey(item.NodeId))
                            {
                                logger.Warn("Duplicate record in due db. NodeId:{0}, LeafName: {1}", item.NodeId, item.LeafName.LogBase64());
                                continue;
                            }
                            folderRecords[item.NodeId] = item;
                        }
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                }
                return folderRecords;
            }
        }

        private async Task AnalyzeFileFromFolder(List<StorageInfo> files, FSFolderStub folder, Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            Guid termId = Guid.Empty;
            bool isHold = false;
            string termName = string.Empty;
            FileSystemRecordDto existFolder;

            using (new AgentPerformanceScope("FSDiscover.Process.AssignFolderTermId", addToStatistics: true))
            {
                termId = FSDataDisposalV2.currentSetting.DefaultTermId;
                existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
                if (existFolder != null)
                {
                    termId = existFolder.TermId;
                    logger.Info($"Different term {termId} on folder {folder.FullPath.LogBase64()}");
                    if (existFolder.HoldStatus && existFolder.HoldReleaseTime > DateTime.UtcNow.Ticks)
                    {
                        isHold = true;
                    }
                }
                if (termId == Guid.Empty)
                {
                    logger.Warn("Term is empty on folder {0}", folder.FullPath.LogBase64());
                    return;
                }
                termName = FSJobCache.Instance.Terms.ContainsKey(termId) ? FSJobCache.Instance.Terms[termId].Name : null;
            }

            foreach (var file in files)
            {
                string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName);
                logger.Debug("Start to process file.id :{0}", fullPath.ToLowerInvariant().ToMd5());
                FSFileStub fileStub = new FSFileStub()
                {
                    FullPath = fullPath,
                    MediaObj = file,
                    SelfId = fullPath.ToLowerInvariant().ToMd5(),
                    ParentId = folder.SelfId,
                    ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId,
                    Depth = folder.Depth + 1,
                };

                if (!azureCache.TryGetValue(fileStub.SelfId, out var fileRecord))
                {
                    logger.Debug("File record not found for file id: {0}", fileStub.SelfId);
                    //fileRecord = AssembleFileBasicInfo(fileStub);
                }

                using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.AnalyzeFileFromFolder", addToStatistics: true))
                {
                    logger.Debug("Start to analyze file from folder, file id:{0}, folder {1}", fileStub?.SelfId, folder.SelfId);
                    var id = fileStub.SelfId;
                    FSAzureTableEntityDto dto = null;
                    try
                    {
                        if (FSJobCache.Instance.TermRuleMapping.ContainsKey(termId))
                        {
                            var rules = FSJobCache.Instance.TermRuleMapping[termId];
                            var filteredRules = RuleUtil.FilterMoveRules(rules, Path.GetDirectoryName(fileStub.FullPath)).Where(x => x.FSRule != null).ToList();
                            if (filteredRules.Count > 0)
                            {
                                DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                                var hasOwnerRule = HasOwnerRule(filteredRules);
                                ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(fileStub.MediaObj), FSJobCache.Instance.RootPath, hasOwnerRule);
                                var matchedRule = engine.MatchPotentialRule(filterObject);
                                if (matchedRule != null && matchedRule.Item1 != null && !string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                                {
                                    var rule = matchedRule.Item1;
                                    if (fileRecord != null)
                                    {
                                        fileRecord.RuleName = rule.Name;
                                    }
                                    if (IsRemoveRule(rule) && isHold)
                                    {
                                        logger.Info("This file in on hold and current rule is remove rule, will be skipped.");
                                        AddSkipReport(fileStub);
                                        continue;
                                    }

                                    dto = CreateAzureEntityDto(fileStub, rule, folder, existFolder, termId, termName);

                                    if (!rule.FSRule.IsManualApproval)
                                    {
                                        dto.ScanTime = DateTime.UtcNow;
                                        await workerWriter.WriteAsync((dto, fileRecord)).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await manualWriter.WriteAsync(dto).ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    logger.Info($"Current file not match rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                                }
                            }
                            else
                            {
                                logger.Info($"Current Term[{termId}] doesn't have FS rule so skip check rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                            }
                        }
                        else
                        {
                            logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", termId);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while disposal file:{fileStub.FullPath.LogBase64()} Error:{e}");
                        var detail = new JMFSDisposalJobDetailV2()
                        {
                            ObjectName = dto != null ? dto.HighName : Path.GetFileName(fileStub.FullPath),
                            SourceLocation = fileStub.FullPath,
                            Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                            FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                            Action = dto != null && dto.RuleId != null && FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId))
                                ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[new Guid(dto.RuleId)]))
                                : "",
                            RuleName = dto != null && dto.RuleId != null && FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId))
                                ? FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name
                                : "",
                            Status = JobDetailsStatus.Failed,
                            Comment = e.Message,
                            Type = "RM_JS_Rule_ObjectLevel_FSFile",
                            AgentName = OSInformation.HostName,
                            Depth = fileStub.Depth,
                            DirPath = folder.FullPath,
                            DetailAction = (int)DetailAction.Scan,
                        };
                        JobDetailService.Commit(detail);
                        FSJobCache.Instance.FailedCount++;
                    }
                }
            }
        }

        private bool HasOwnerRule(List<Rule> rules)
        {
            var hasOwnerRule = false;
            var fsRules = rules.Where(r => r.FSRule != null).Select(r => r.FSRule).ToList();
            if (fsRules != null && fsRules.Any())
            {
                hasOwnerRule = fsRules.Any(r => r.Filters.Any(f => f.Rule is AvePoint.GCommon.Contract.CommonFilter.OwnerRule));
            }
            return hasOwnerRule;
        }

        private async Task AnalyzeFile(FileSystemCollectionFolder folder, Dictionary<Guid, FileSystemRecordDto> azureCache, Guid folderId)
        {
            var depth = folder.Depth + 1;
            foreach (var file in folder.ChildrenFiles)
            {
                using (new AgentPerformanceScope("FSDocumentDisposal.AnalyzeFile", addToStatistics: true))
                {
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName);
                    logger.Debug("Start to process file.id :{0}", fullPath.ToLowerInvariant().ToMd5());
                    FSFileStub fileStub = new FSFileStub()
                    {
                        FullPath = fullPath,
                        MediaObj = file,
                        SelfId = fullPath.ToLowerInvariant().ToMd5(),
                        ParentId = folderId,
                        ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId,
                        Depth = depth
                    };

                    logger.Debug("Start to analyze file, file id:{0}", fileStub?.SelfId);

                    var id = fileStub.SelfId;
                    if (!azureCache.ContainsKey(id))
                    {
                        logger.Debug("No db record found:{0}", id);
                        continue;
                    }

                    var dbRecord = azureCache[id];
                    try
                    {
                        if (dbRecord.TermId == Guid.Empty)
                        {
                            continue;
                        }

                        if (!FSJobCache.Instance.TermRuleMapping.ContainsKey(dbRecord.TermId))
                        {
                            logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", dbRecord?.TermId);
                            continue;
                        }

                        var rules = FSJobCache.Instance.TermRuleMapping[dbRecord.TermId];
                        var filteredRules = RuleUtil.FilterMoveRules(rules, Path.GetDirectoryName(fileStub.FullPath)).Where(x => x.FSRule != null).ToList();

                        if (filteredRules.Count == 0)
                        {
                            logger.Debug("Current Term[{0}] doesn't have FS rule so skip check rule.FSPath:{1}.", dbRecord.TermId, fileStub.FullPath.LogBase64());
                            continue;
                        }

                        DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                        ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObjectV2(new XFileInfoEx(fileStub.MediaObj), dbRecord, FSJobCache.Instance.RootPath);
                        var matchedRule = engine.MatchPotentialRule(filterObject);
                        if (matchedRule == null || matchedRule.Item1 == null || string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                        {
                            if (azureCache.ContainsKey(id))
                            {
                                var manualData = CreateAzureEntityDto(fileStub, null, dbRecord);
                                RemoveManualData(manualData);
                            }
                            else
                            {
                                logger.Debug("Current file not match rule.FSPath:{0}.", fileStub.FullPath.LogBase64());
                            }

                            continue;
                        }

                        var rule = matchedRule.Item1;
                        if (IsRemoveRule(rule) && dbRecord.HoldStatus && dbRecord.HoldReleaseTime > DateTime.UtcNow.Ticks)
                        {
                            logger.Info("This file in on hold and current rule is remove rule, will be skipped.");
                            AddSkipReport(fileStub);
                            continue;
                        }

                        var dto = CreateAzureEntityDto(fileStub, rule, dbRecord);

                        if (!rule.FSRule.IsManualApproval)
                        {
                            dto.ScanTime = DateTime.UtcNow;
                            await workerWriter.WriteAsync((dto, dbRecord)).ConfigureAwait(false);
                            continue;
                        }

                        if (!azureCache.ContainsKey(id))
                        {
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.ScanTime = DateTime.UtcNow;
                            await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                            continue;
                        }

                        var azureRecord = azureCache[id];
                        dto.Status = azureRecord.ManualApprovedStatus;
                        dto.ScanTime = DateTime.UtcNow;

                        if (!azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
                        {
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.MovedToApprovalTable = false;
                            dto.ScanTime = DateTime.UtcNow;
                            await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                            continue;
                        }

                        switch (azureRecord.ManualApprovedStatus)
                        {
                            case (int)SOApproveDBStatus.None:
                                dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                dto.ScanTime = DateTime.UtcNow;
                                await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                                break;
                            case (int)SOApproveDBStatus.Approved:
                                await workerWriter.WriteAsync((dto, dbRecord)).ConfigureAwait(false);
                                break;
                            case (int)SOApproveDBStatus.KeepData:
                            case (int)SOApproveDBStatus.CheckOption:
                            case (int)SOApproveDBStatus.WaitingApprove:
                                AddSkipReport(dto);
                                break;
                            case (int)SOApproveDBStatus.Rejected:
                                if (dbRecord.IsManualSynced && dbRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                                {
                                    logger.Debug("item is manualsync and its extend");
                                    continue;
                                }

                                dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
                                await cosmosWriter.WriteAsync(dto).ConfigureAwait(false);
                                break;
                            default:
                                logger.Warn("Invalid status. File id:{0}", fileStub?.SelfId);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("Error occurred while disposal file:{0} Error:{1}", ExternalUtil.CombinePath(dbRecord.DirPath, dbRecord.LeafName), e.ToString());
                        JMFSDisposalJobDetailV2 detail = new JMFSDisposalJobDetailV2()
                        {
                            ObjectName = dbRecord.LeafName,
                            SourceLocation = ExternalUtil.CombinePath(dbRecord.DirPath, dbRecord.LeafName),
                            Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                            FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                            Action = FSJobCache.Instance.Rules.ContainsKey(dbRecord.RuleId) ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[dbRecord.RuleId])) : "",
                            RuleName = FSJobCache.Instance.Rules.ContainsKey(dbRecord.RuleId) ? FSJobCache.Instance.Rules[dbRecord.RuleId].Name : "",
                            Status = JobDetailsStatus.Failed,
                            Comment = e.Message,
                            Type = "RM_JS_Rule_ObjectLevel_FSFile",
                            AgentName = OSInformation.HostName,
                            Depth = depth,
                            DirPath = folder.FullPath,
                            DetailAction = (int)DetailAction.Scan,
                        };
                        JobDetailService.Commit(detail);
                        FSJobCache.Instance.FailedCount++;
                    }

                }
            }
        }

        private bool IsRemoveRule(Rule rule)
        {
            if (rule != null && rule.FSRule != null && (rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveSetting != null && rule.FSRule.spMoveOption.MoveDestination != null))
            {
                return false; // move to
            }
            return true; // remove
        }

        private void AddSkipReport(FSFileStub file)
        {
            XFileInfoEx xObj = new XFileInfoEx(file.MediaObj);
            var detail = new JMFSDisposalJobDetailV2()
            {
                ObjectName = xObj.LowName,
                Size = ExternalUtil.ConvertToFormatSize(xObj.FileSize),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName, xObj.LowName),
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileOnHold",
                Depth = file.Depth,
                DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName),
                DetailAction = (int)DetailAction.Scan,
            };
            JobDetailService.Commit(detail);
        }

        private void AddSkipReport(FSAzureTableEntityDto entity)
        {
            var detail = new JMFSDisposalJobDetailV2()
            {
                ObjectName = entity.LowName,
                Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = GetActionString(entity.RuleAction),
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                RuleName = !string.IsNullOrEmpty(entity.RuleId) && FSJobCache.Instance.Rules.ContainsKey(new Guid(entity.RuleId))
                    ? FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name
                    : "",
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileWaitingForApproval",
                Depth = entity.Depth,
                DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName),
                DetailAction = (int)DetailAction.Scan,
            };
            JobDetailService.Commit(detail);
            entity.NoNeedSendReport = true;
        }

        private RuleAction GetRuleAction(Rule currentRule)
        {
            RuleAction action = new RuleAction();
            if (currentRule.FSRule != null && currentRule.FSRule.spMoveOption != null &&
                currentRule.FSRule.spMoveOption.MoveSetting != null)
            {
                action = RuleAction.MoveAndDeclare;
            }
            else
            {
                action = RuleAction.ArchiveAndRemove;
            }
            return action;
        }

        private void RemoveManualData(FSAzureTableEntityDto dto)
        {
            DeleteManualItemCache.Add(dto);
            if (DeleteManualItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = DeleteManualItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("FSDicover.RemoveManualData", $"FSDicover.RemoveManualData.Count:{tempEntities.Count}", true))
                    {
                        JobContext.Current.ApiClient.RemoveManualData(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while removing manual data. Error:{0}", e.ToString());
                }
            }
        }

        private void AddRejectFileToAzureTable(FSAzureTableEntityDto dto)
        {
            RejectItemCache.Add(dto);
            if (RejectItemCache.Count > ExternalUtil.TransferDataCount)
            {
                var tempEntities = RejectItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    using (new AgentPerformanceScope("FSDicover.AddRejectScanData", $"FSDicover.AddRejectScanData.Count:{tempEntities.Count}", true))
                    {
                        List<Guid> failedGuids = JobContext.Current.ApiClient.AddRejectScanData(tempEntities);
                        AddRejectToReports(tempEntities, failedGuids);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occured while adding reject data. Error:{0}", e.ToString());
                    AddRejectToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
                }
            }
        }

        private static string GetActionString(int action)
        {
            string actionStr = string.Empty;
            switch (action)
            {
                case (int)RuleAction.ArchiveAndRemove:
                    actionStr = "RM_FS_DisposalAction_Remove";
                    break;
                case (int)RuleAction.MoveAndDeclare:
                    actionStr = "RM_FS_DisposalAction_Move";
                    break;
                case (int)RuleAction.None:
                case (int)RuleAction.ArchiveAndKeep:
                case (int)RuleAction.ExportOnly:
                default:
                    break;
            }
            return actionStr;
        }

        private void FinalRemoveManualData()
        {
            var tempEntities = DeleteManualItemCache.TakeAll().ToList();
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
                    logger.Error("An error occurred while final remove manual data. Error:{0}", e.ToString());
                }
            }
        }

        private void FinalAddRejectFileToAzureTable()
        {
            var tempEntities = RejectItemCache.TakeAll().ToList();
            if (tempEntities.Count > 0)
            {
                try
                {
                    using (new AgentPerformanceScope("FSDicover.AddRejectScanData.Final", "FSDicover.AddRejectScanData.Count:" + tempEntities.Count, true))
                    {
                        List<Guid> failedGuids = JobContext.Current.ApiClient.AddRejectScanData(tempEntities);
                        AddRejectToReports(tempEntities, failedGuids);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final adding reject data. Error:{0}", e.ToString());
                    AddRejectToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
                }
            }
        }

        private void AddRejectToReports(List<FSAzureTableEntityDto> tempEntities, List<Guid> failedGuids)
        {
            if (failedGuids.Count > 0)
            {
                logger.Debug("Failed to add reject data. Ids:{0}", string.Join(",", failedGuids));
            }
            var details = new List<JMFSDisposalJobDetailV2>();
            foreach (var entity in tempEntities)
            {
                var detail = new JMFSDisposalJobDetailV2()
                {
                    ObjectName = entity.LowName,
                    Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                    Action = GetActionString(entity.RuleAction),
                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                    RuleName = !string.IsNullOrEmpty(entity.RuleId) && FSJobCache.Instance.Rules.ContainsKey(new Guid(entity.RuleId))
                        ? FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name
                        : "",
                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                    AgentName = OSInformation.HostName,
                    Depth = entity.Depth,
                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName),
                    DetailAction = (int)DetailAction.UpdateManual,
                };
                if (failedGuids.Contains(entity.FilePathMd5))
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_JM_FSFailedUpdateRejectFile";
                }
                else
                {
                    detail.Status = JobDetailsStatus.Successful;
                    detail.Comment = "RM_JM_FSFileWaitingForApproval";
                }
                details.Add(detail);
            }
            JobDetailService.CommitBatch(details);
        }

        private FSAzureTableEntityDto CreateAzureEntityDto(FSFileStub stub, Rule rule, FileSystemRecordDto dbRecord)
        {
            XFileInfoEx xObj = new XFileInfoEx(stub.MediaObj);
            FSAzureTableEntityDto dto = new FSAzureTableEntityDto()
            {
                ConnectionId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5(),
                ScopeID = FSJobCache.Instance.ScopeSettingCache[stub.ScopeSettingId].ScopeId,
                CreateTime = xObj.CreationTimeUtc,
                NodeLevel = (int)NodeLevel.FSFile,
                LastModifiedTme = xObj.LastWriteTimeUtc,
                RuleId = rule != null ? rule.Id : String.Empty,
                ParentID = stub.ParentId,
                LowName = xObj.LowName,
                HighName = xObj.HighName,
                FullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName, xObj.LowName),
                MovedToApprovalTable = false,
                ScanTime = DateTime.UtcNow,
                FilePathMd5 = stub.FullPath.ToLowerInvariant().ToMd5(),
                KeepDataOption = rule != null ? rule.KeepDataOption : 0,
                Status = (int)SOApproveDBStatus.None,
                SortTicks = Snowflake.Instance().GetTicks(),
                RuleAction = rule != null ? (int)GetRuleAction(rule) : 0,
                Size = xObj.FileSize,
                TermId = dbRecord.TermId,
                TermName = dbRecord.TermName,
                Property = GetMetaData(xObj),
                InternalConnectionId = FSJobCache.Instance.AveConnectionId
            };
            dto.CurrentSettingId = GetCurrentSettingId(stub);
            dto.HoldStatus = dbRecord.HoldStatus;
            dto.HoldReleaseTime = dbRecord.HoldReleaseTime;
            dto.HoldBy = dbRecord.HoldBy;
            dto.HoldId = dbRecord.HoldId;
            dto.HoldType = dbRecord.HoldType;
            dto.HoldByUsers = dbRecord.HoldByUsers;
            dto.HoldUntilTimes = dbRecord.HoldUntilTimes;
            dto.AppendHolds_Array = dbRecord.AppendHolds_Array;
            dto.ManualApprovedBy = dbRecord.ManualApprovedBy;
            dto.ManualEscalateFrom = dbRecord.ManualEscalateFrom;
            dto.Depth = stub.Depth;
            return dto;
        }

        private FSAzureTableEntityDto CreateAzureEntityDto(FSFileStub stub, Rule rule, FSFolderStub folder, FileSystemRecordDto holdFolder, Guid termId, string termName)
        {
            XFileInfoEx xObj = new XFileInfoEx(stub.MediaObj);
            FSAzureTableEntityDto dto = new FSAzureTableEntityDto()
            {
                ConnectionId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5(),
                ScopeID = FSJobCache.Instance.ScopeSettingCache[stub.ScopeSettingId].ScopeId,
                CreateTime = xObj.CreationTimeUtc,
                NodeLevel = (int)NodeLevel.FSFile,
                LastModifiedTme = xObj.LastWriteTimeUtc,
                RuleId = rule != null ? rule.Id : String.Empty,
                ParentID = stub.ParentId,
                LowName = xObj.LowName,
                HighName = xObj.HighName,
                FullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName, xObj.LowName),
                MovedToApprovalTable = false,
                ScanTime = DateTime.UtcNow,
                FilePathMd5 = stub.FullPath.ToLowerInvariant().ToMd5(),
                KeepDataOption = rule != null ? rule.KeepDataOption : 0,
                Status = (int)SOApproveDBStatus.None,
                SortTicks = Snowflake.Instance().GetTicks(),
                RuleAction = rule != null ? (int)GetRuleAction(rule) : 0,
                Size = xObj.FileSize,
                TermId = termId,
                TermName = termName,
                Property = GetMetaData(xObj),
                InternalConnectionId = FSJobCache.Instance.AveConnectionId,
                Depth = stub.Depth
            };
            dto.CurrentSettingId = GetCurrentSettingId(stub);
            if (holdFolder != null && holdFolder.HoldStatus)
            {
                dto.HoldStatus = holdFolder.HoldStatus;
                dto.HoldReleaseTime = holdFolder.HoldReleaseTime;
                dto.HoldBy = holdFolder.HoldBy;
                dto.HoldId = holdFolder.HoldId;
                dto.HoldType = holdFolder.HoldType;
                dto.HoldByUsers = holdFolder.HoldByUsers;
                dto.HoldUntilTimes = holdFolder.HoldUntilTimes;
                dto.AppendHolds_Array = holdFolder.AppendHolds_Array;
            }
            return dto;
        }

        private Guid GetCurrentSettingId(FSFileStub stub)
        {
            var settings = FSJobCache.Instance.GroupSettingCache.Where(c => stub.FullPath.StartsWith(c.Key));
            if (settings.Count() > 0)
            {
                var currentSetting = settings.OrderByDescending(c => c.Key).FirstOrDefault();
                return currentSetting.Value.ScopeId;
            }
            else
            {
                return FSJobCache.Instance.ScopeSettingCache[stub.ScopeSettingId].ScopeId;
            }
        }

        private string GetMetaData(XFileInfoEx xObj)
        {
            string propertyValue = string.Empty;
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement xe = doc.CreateElement("Property");

                XmlElement createdByElement = doc.CreateElement("Column");
                createdByElement.SetAttribute("Name", "CreatedBy");
                createdByElement.SetAttribute("Value", xObj.Owner);
                xe.AppendChild(createdByElement);

                XmlElement modifiedByElement = doc.CreateElement("Column");
                modifiedByElement.SetAttribute("Name", "ModifiedBy");
                modifiedByElement.SetAttribute("Value", GetOfficeLastModifiedBy(xObj));
                xe.AppendChild(modifiedByElement);

                propertyValue = xe.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Debug("GetMetaData failed, path: {0}, details: {1}", xObj.Name, ex.ToString());
            }
            return propertyValue;
        }

        private string GetOfficeLastModifiedBy(XFileInfoEx xObj)
        {
            string lastModifiedBy = string.Empty;
            if (IsOffice07(xObj.Name))
            {
                // intentionally not reading package metadata here
            }
            return lastModifiedBy;
        }

        private bool IsOffice07(string fileName)
        {
            try
            {
                string fileExtension = Path.GetExtension(fileName).Substring(1).ToLower(CultureInfo.InvariantCulture);
                List<string> officeCollection = new List<string>() { "docx", "docm", "dotx", "dotm", "xlsx", "xlsm", "xltx", "xltm", "xlsb", "xlam", "pptx", "pptm", "ppsx", "ppsm", "potx", "potm", "ppam" };
                if (officeCollection.Contains(fileExtension))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("Check office file type failed, details: {0}", ex.ToString());
            }
            return false;
        }
    }
}
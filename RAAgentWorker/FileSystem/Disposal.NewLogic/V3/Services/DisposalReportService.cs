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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.FileSystem.Stubs;
using RAFileSystem.FileSystem.Collector;
using RAFileSystem.Utils;
using FSTreeNodeDto = AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services
{
    /// <summary>
    /// Responsible for generating disposal job detail reports (skip, fail, etc.).
    /// </summary>
    public class DisposalReportService
    {
        private readonly IReportService<JMJobDetails> _jobDetailService;
        private readonly IProgressService _progressService;
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(DisposalReportService));

        public DisposalReportService(
            IReportService<JMJobDetails> jobDetailService,
            IProgressService progressService)
        {
            _jobDetailService = jobDetailService;
            _progressService = progressService;
        }

        public void AddConnectionSkipReport(FSTreeNodeDto dto) {
            var detail = new JMFSDisposalJobDetailV2()
            {
                ObjectName = dto.Name,
                Type = "RM_FS_Register_Connections",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileWaitingForApproval_Pause",
                Depth =0,
                DirPath = dto.FullPath,
                DetailAction = (int)DetailAction.Scan,
            };
            _jobDetailService.Commit(detail);
        }
        
        public void AddSkipReport(FSAzureTableEntityDto entity)
        {
            var detail = new JMFSDisposalJobDetailV2()
            {
                ObjectName = entity.LowName,
                Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = DisposalRuleUtility.GetActionString(entity.RuleAction),
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
            _jobDetailService.Commit(detail);
            entity.NoNeedSendReport = true;
        }

        /// <summary>
        /// Reports a skipped file that is waiting for manual approval.
        /// </summary>
        public void AddSkipReportForApproval(FSAzureTableEntityDto entity)
        {
            var detail = new JMFSDisposalJobDetailV2
            {
                ObjectName = entity.LowName,
                Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = DisposalRuleUtility.GetActionString(entity.RuleAction),
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                RuleName = ResolveRuleName(entity.RuleId),
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileWaitingForApproval",
                Depth = entity.Depth,
                DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName),
                DetailAction = (int)DetailAction.Scan,
            };
            _jobDetailService.Commit(detail);
            entity.NoNeedSendReport = true;
        }

        /// <summary>
        /// Reports a skipped file that is on hold.
        /// </summary>
        public void AddSkipReportForHold(FSFileStub file)
        {
            var xObj = new XFileInfoEx(file.MediaObj);
            var detail = new JMFSDisposalJobDetailV2
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
            _jobDetailService.Commit(detail);
        }

        /// <summary>
        /// Reports a failed file during disposal analysis.
        /// </summary>
        public void ReportFailedFile(FileSystemCollectionFolder folder,FSFileStub fileStub, FileSystemRecordDto dbRecord, Exception error)
        {
            var detail = new JMFSDisposalJobDetailV2
            {
                ObjectName = dbRecord.LeafName,
                SourceLocation = ExternalUtil.CombinePath(dbRecord.DirPath, dbRecord.LeafName),
                Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = ResolveActionString(dbRecord.RuleId),
                RuleName = ResolveRuleNameByGuid(dbRecord.RuleId),
                Status = JobDetailsStatus.Failed,
                Comment = error.Message,
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                DetailAction = (int)DetailAction.Scan,
                DirPath = folder.FullPath,
                Depth = folder.Depth
            };
            _jobDetailService.Commit(detail);
            FSJobCache.Instance.FailedCount++;
        }

        public void ReportFailedFile(FSAzureTableEntityDto dto, Exception e)
        {
            var detail = new JMFSDisposalJobDetailV2
            {
                ObjectName = dto.LowName,
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName, dto.LowName),
                Size = ExternalUtil.ConvertToFormatSize(dto.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = DisposalRuleUtility.GetActionString(dto.RuleAction),
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
            _jobDetailService.Commit(detail);
            FSJobCache.Instance.FailedCount++;
        }
        
        public void ReportFailedFile(FSAzureTableEntityDto dto,FSFileStub fileStub, FSFolderStub folder, Exception error)
        {
            var detail = new JMFSDisposalJobDetailV2
            {
                ObjectName = dto != null ? dto.HighName : Path.GetFileName(fileStub.FullPath),
                SourceLocation = fileStub.FullPath,
                Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = ResolveActionString(dto != null && dto.RuleId != null ? new Guid(dto.RuleId) : Guid.Empty),
                RuleName = ResolveRuleNameByGuid(dto != null && dto.RuleId != null ? new Guid(dto.RuleId) : Guid.Empty),
                Status = JobDetailsStatus.Failed,
                Comment = error.Message,
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                DetailAction = (int)DetailAction.Scan,
                DirPath = folder.FullPath,
                Depth = fileStub.Depth 
            };
            _jobDetailService.Commit(detail);
            FSJobCache.Instance.FailedCount++;
        }

        /// <summary>
        /// Reports a failed folder during discovery.
        /// </summary>
        public void ReportFailedFolder(FileSystemCollectionFolder folder)
        {
            var detail = new JMFSDisposalJobDetailV2
            {
                ObjectName = Path.GetFileName(folder.FullPath),
                SourceLocation = folder.FullPath,
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JM_FSFailedToDiscoverFolder",
                Type = "RM_JS_Rule_ObjectLevel_FSFolder",
                Depth = folder.Depth,
                DirPath = folder.FullPath,
                DetailAction = (int)DetailAction.Scan
            };
            _progressService.Increase();
            _jobDetailService.Commit(detail);
            FSJobCache.Instance.FailedCount++;
        }
        
        public void AddRejectToReports(List<FSAzureTableEntityDto> tempEntities, List<Guid> failedGuids)
        {
            if (failedGuids.Count > 0)
            {
                _logger.Debug("Failed to add reject data. Ids:{0}", string.Join(",", failedGuids));
            }
            var details = new List<JMFSDisposalJobDetailV2>();
            foreach (var entity in tempEntities)
            {
                var detail = new JMFSDisposalJobDetailV2()
                {
                    ObjectName = entity.LowName,
                    Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                    Action = DisposalRuleUtility.GetActionString(entity.RuleAction),
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
            _jobDetailService.CommitBatch(details);
        }

        public void CommitCosmosSenderDetails(
            List<FSAzureTableEntityDto> sendable,
            List<Guid> failedIds)
        {
            if (sendable == null || sendable.Count == 0)
            {
                return;
            }

            var details = new List<JMFSDisposalJobDetailV2>(sendable.Count);
            foreach (var entity in sendable)
            {
                var isFailed = failedIds.Contains(entity.FilePathMd5);
                var detail = new JMFSDisposalJobDetailV2
                {
                    ObjectName = entity.LowName,
                    Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                    Action = DisposalRuleUtility.GetActionString(entity.RuleAction),
                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                    RuleName = !string.IsNullOrEmpty(entity.RuleId) && FSJobCache.Instance.Rules.ContainsKey(new Guid(entity.RuleId))
                        ? FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name
                        : string.Empty,
                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                    AgentName = OSInformation.HostName,
                    Status = isFailed ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                    Comment = isFailed ? "RM_JM_FSFailedAddToArchiverTable" : "RM_JM_FSFileWaitingForApproval",
                    Depth = entity.Depth,
                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName),
                    DetailAction = (int)DetailAction.UpdateManual,
                };
                details.Add(detail);
            }

            JobContext.Current.JobDetailManager.Create().CommitBatch(details);
        }

        private string ResolveRuleName(string ruleId)
        {
            if (!string.IsNullOrEmpty(ruleId) && Guid.TryParse(ruleId, out var guid)
                && FSJobCache.Instance.Rules.ContainsKey(guid))
            {
                return FSJobCache.Instance.Rules[guid].Name;
            }

            return string.Empty;
        }

        private string ResolveRuleNameByGuid(Guid ruleId)
        {
            if (FSJobCache.Instance.Rules.ContainsKey(ruleId))
            {
                return FSJobCache.Instance.Rules[ruleId].Name;
            }

            return string.Empty;
        }

        private string ResolveActionString(Guid ruleId)
        {
            if (FSJobCache.Instance.Rules.ContainsKey(ruleId))
            {
                return DisposalRuleUtility.GetActionString((int)DisposalRuleUtility.GetRuleAction(FSJobCache.Instance.Rules[ruleId]));
            }
            return string.Empty;
        }

        public void IncreaseProgressBase(int count)
        {
            _progressService.IncreaseBase(count);
        }

        public void IncreaseProgress()
        {
            _progressService.Increase();
        }
    }
}

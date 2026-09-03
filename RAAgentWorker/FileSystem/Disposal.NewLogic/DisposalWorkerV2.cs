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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.StorageOptimization.Schedule.Archiver;
using RAFileSystem.FileSystem.Backup;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.Common.Extension;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services;
using RAFileSystem.Utils;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.Disposal.NewLogic
{
    public class DisposalWorkerV2
    {
        private readonly AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DisposalReportService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private IXSystem Device { get; set; }
        private IXSystem TempDevice { get; set; }

        private readonly ChannelReader<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> discoveryReader;
        private readonly ChannelWriter<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> updaterWriter;
        private readonly FolderSizeUpdateTracker _folderSizeTracker;
        public DisposalWorkerV2(DisposalReportService reportService = null)
        {
            Device = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);

            JobDetailService = JobContext.Current.JobDetailManager.Create();
            ProgressService = reportService ?? new DisposalReportService(JobDetailService, JobContext.Current.mProgressManager.Create());
            discoveryReader = FSJobCache.Instance.DiscoveryToWorker.Reader;
            updaterWriter = FSJobCache.Instance.WorkerToUpdater.Writer;
            _folderSizeTracker = new FolderSizeUpdateTracker(FSJobCache.Instance.RootPath);
        }

        public async Task Run()
        {
            try
            {
                using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.TotalArchiveJob", addToStatistics: true))
                {
                    // Consume items from the channel without async streams
                    while (discoveryReader != null && await discoveryReader.WaitToReadAsync().ConfigureAwait(false))
                    {
                        while (discoveryReader.TryRead(out var channelItem))
                        {
                            var file = channelItem.FileDto;
                            var fsRecord = channelItem.FSRecordDto;
                            try
                            {
                                if (file == null)
                                {
                                    continue;
                                }

                                if (file.Size <= 0)
                                {
                                    logger.Warn("File size is less than 0. Skip this file. Scope id:{0}", file?.ScopeID);
                                    ProgressService.IncreaseProgress();
                                    continue;
                                }

                                if (fsRecord != null)
                                {
                                    file.CreateDate = fsRecord.CreateDate;
                                    fsRecord.RuleAction = file.RuleAction;
                                    fsRecord.FileSize = file.Size;
                                }

                                var backup = GetBackupObject(new Guid(file.RuleId));
                                var status = backup.Backup(file, fsRecord);

                                if (status == (int)BackupRestoreStatus.Succeed)
                                {
                                    file.AchiveTime = DateTime.UtcNow;
                                    file.Status = (int)SOApproveDBStatus.Archived;
                                    FSJobCache.Instance.SuccessCount++;
                                    await updaterWriter.WriteAsync((file, fsRecord)).ConfigureAwait(false);
                                    logger.Debug("File processed successfully. Scope id:{0} NodeId:{1} HighName {2}", file?.ScopeID, file?.FilePathMd5, file?.HighName);
                                    //await _folderSizeTracker.RecordDeletedFile(file.HighName, file.Size);
                                }
                                else if (status == (int)BackupRestoreStatus.Failed)
                                {
                                    FSJobCache.Instance.FailedCount++;
                                }

                                logger.Debug("Disposal file finished. Scope id:{0} Status:{1} NodeId:{2}", file?.ScopeID, status.ToString(), file?.FilePathMd5);
                                ProgressService.IncreaseProgress();
                            }
                            catch (IndexDeviceNotSurpportException e)
                            {
                                FSJobCache.Instance.SuccessCount = 0;
                                FSJobCache.Instance.FailedCount++;
                                JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, TenantAgentInfo.JobId, true, e.Message);
                                // fatal, break out to finally
                                return;
                            }
                            catch (DataDeviceNotSurpportException e)
                            {
                                FSJobCache.Instance.FailedCount++;
                                JobDetailService.Commit(new JMFSDisposalJobDetailV2
                                {
                                    ObjectName = file.LowName,
                                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    Size = ExternalUtil.ConvertToFormatSize(file.Size),
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    Action = GetActionString(file.RuleAction),
                                    RuleName = FSJobCache.Instance.Rules[new Guid(file.RuleId)].Name,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = e.Message,
                                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                                    Depth = file.Depth,
                                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    DetailAction = (int)DetailAction.ArchiveAndMove,
                                });
                            }
                            catch (Exception itemex)
                            {
                                logger.Error("Failed to process item. Object:{0}, Exception:{1}",
                                    ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName),
                                    itemex.ToString());
                                ProgressService.IncreaseProgress();
                                FSJobCache.Instance.FailedCount++;

                                var errorComment = itemex.Message == "Incorrect action."
                                    ? "RM_JM_FSFailedToDisposalFileActionIsIncorrect"
                                    : "RM_JM_FSFailedToDisposalFile";

                                JobDetailService.Commit(new JMFSDisposalJobDetailV2
                                {
                                    ObjectName = file.LowName,
                                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    Size = ExternalUtil.ConvertToFormatSize(file.Size),
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    Action = GetActionString(file.RuleAction),
                                    RuleName = FSJobCache.Instance.Rules[new Guid(file.RuleId)].Name,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = errorComment,
                                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                                    Depth = file.Depth,
                                    DirPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    DetailAction = (int)DetailAction.ArchiveAndMove,
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                ClearBackUpSenderAndRemoveFile();
                //await FlushFolderSizeUpdates();
                updaterWriter.TryComplete();
            }
        }

        private void ClearBackUpSenderAndRemoveFile()
        {
            foreach (var backupAction in FSJobCache.Instance.RuleActionCache)
            {
                backupAction.Value.ClearBackupSender();
            }
            foreach (var backupAction in FSJobCache.Instance.RuleActionCache)
            {
                backupAction.Value.RemoveArchivedFiles();
                backupAction.Value.MergeIndex();
            }
            try
            {
                TempDevice = ExternalUtil.OpenXSystem(AppDomain.CurrentDomain.BaseDirectory);
                TempDevice.DeleteDirectory(new StorageInfo() { HighName = BackgroundSettings.GetInstance().InternalArchiveCache });
            }
            catch (Exception e)
            {
                logger.Error($"Failed to delete cache folder.{e}");
            }
        }

        private async Task FlushFolderSizeUpdates()
        {
            try
            {
                await _folderSizeTracker.FlushUpdates();
            }
            catch (Exception e)
            {
                logger.Error("Failed to flush folder size updates. Error: {0}", e);
            }
        }

        private string GetActionString(int action)
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

        private FSObjectBackup GetBackupObject(Guid ruleId)
        {
            if (FSJobCache.Instance.RuleActionCache.ContainsKey(ruleId))
            {
                return FSJobCache.Instance.RuleActionCache[ruleId];
            }
            else
            {
                if (FSJobCache.Instance.Rules.ContainsKey(ruleId))
                {
                    var rule = FSJobCache.Instance.Rules[ruleId];
                    if (rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveSetting != null && rule.FSRule.spMoveOption.MoveDestination != null)
                    {
                        FSToFSDocumentMoveTo imps = new FSToFSDocumentMoveTo(Device, rule.FSRule.spMoveOption, JobDetailService);
                        FSJobCache.Instance.RuleActionCache.TryAdd(ruleId, imps);
                        return imps;
                    }
                    else if (rule.FSRule.KeepDataOption == (int)KeepDataOption.LinkDocument || rule.FSRule.KeepDataOption == (int)KeepDataOption.Delete)
                    {
                        FSDocumentBackupImps imps = new FSDocumentBackupImps(Device,
                            rule.FSRule.KeepDataOption == (int)KeepDataOption.LinkDocument ? true : false,
                            rule.FSRule.RelatedRecordOption == RelatedRecordOption.Both ? true : false,
                            JobDetailService);
                        FSJobCache.Instance.RuleActionCache.TryAdd(ruleId, imps);
                        return imps;
                    }
                    else if ((rule.FSRule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemove) == (int)KeepDataOption.ArchiveBackupAndRemove)
                    {
                        if (!FSJobCache.Instance.RuleActionCache.Keys.Contains(ruleId))
                        {
                            FSDocumentBackup imps = new FSDocumentBackup(Device, JobDetailService);
                            FSJobCache.Instance.ConnectionPath = Device.SystemLocation.TrimEnd('\\');
                            FSJobCache.Instance.RuleActionCache.TryAdd(ruleId, imps);
                            return imps;
                        }
                        else
                        {
                            return FSJobCache.Instance.RuleActionCache[ruleId];
                        }
                    }
                    else
                    {
                        throw new Exception("Incorrect action.");
                    }
                }
                else
                {
                    throw new Exception("Cannot find rule in cache. Rule Id: " + ruleId);
                }
            }
        }
    }
}

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
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Backup;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.StorageOptimization.Schedule.Archiver;
using System.IO;
using RAFileSystem.FileSystem.Common;
using Alphaleonis.Win32.Filesystem;
using AvePoint.GCommon.Contract.Media.Object;
using RAFileSystem.FileSystem.Common.Extension;
using AvePoint.RA.Contract.Tenant;

namespace RAFileSystem.Disposal.Archive
{
    public class DisposalWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private IXSystem Device { get; set; }
        private IXSystem tempDevice { get; set; }
        public DisposalWorker()
        {
            Device = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
        }
        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("WorkerThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.AnalyzerThreadMonitor.Increment();
                using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.TotalArchiveJob", addToStatistics: true))
                {
                    while (true)
                    {
                        if (FSJobCache.Instance.DisposalScanCache.Count == 0
                            && FSJobCache.Instance.DiscoveryCache.Count == 0
                            && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                            && JobContext.Current.GetCosmosDBDataFinish)
                        {
                            logger.Info("There is no FILE/Folder to be analyzed,nor any discovery thread running, analyzer thread [{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                            break;
                        }
                        //if (FSDataDisposal.ClassificationLevel == AvePoint.GCommon.Contract.Tree.Object.NodeLevel.FSFolder
                        //    && FSJobCache.Instance.DisposalFolderCache.Count == 0
                        //    && FSJobCache.Instance.DisposalScanCache.Count == 0
                        //    && FSJobCache.Instance.DiscoveryCache.Count == 0
                        //    && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0)
                        //{
                        //    logger.Info("There is no FOLDER to be analyzed,nor any discovery thread running, analyzer thread [{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                        //    break;
                        //}
                        //discoveyr thread is still running.Wait 1 sec for new objects.
                        if (FSJobCache.Instance.AnalyzerCache.Count == 0)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        IEnumerable<FSAzureTableEntityDto> files = FSJobCache.Instance.DisposalScanCache.Take(100);
                        if (files.Count() == 0)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        logger.Info("Worker got {0} files. ", files.Count());
                        foreach (FSAzureTableEntityDto file in files)
                        {
                            try
                            {
                                if (file.Size <= 0)
                                {
                                    logger.Warn("File size is less than 0. Skip this file. Scope id:{0}", file?.ScopeID);
                                    ProgressService.Increase();
                                    continue;
                                }
                                FSObjectBackup backup = GetBackupObject(new Guid(file.RuleId));
                                var status = backup.Backup(file);
                                //only process document
                                if (status == (int)BackupRestoreStatus.Succeed)
                                {
                                    //delete/move successfully, add to archiver table
                                    file.AchiveTime = DateTime.UtcNow;
                                    file.Status = (int)SOApproveDBStatus.Archived;
                                    FSJobCache.Instance.SuccessCount++;
                                    FSJobCache.Instance.DisposalArchiveCache.Add(file);
                                }
                                if (status == (int)BackupRestoreStatus.Failed)
                                {
                                    FSJobCache.Instance.FailedCount++;
                                }
                                logger.Debug("Disposal file finished. Scope id:{0} Status:{1}", file?.ScopeID, status.ToString());
                                ProgressService.Increase();
                            }
                            catch (IndexDeviceNotSurpportException e)
                            {
                                FSJobCache.Instance.SuccessCount = 0;
                                FSJobCache.Instance.FailedCount++;
                                JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, TenantAgentInfo.JobId,true, e.Message);
                                break;
                            }
                            catch (DataDeviceNotSurpportException e)
                            {
                                FSJobCache.Instance.FailedCount++;
                                JobDetailService.Commit(new JMFSDisposalJobDetails
                                {
                                    ObjectName = file.LowName,
                                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    Size = ExternalUtil.ConvertToFormatSize(file.Size),
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    Action = GetActionString(file.RuleAction),
                                    RuleName = FSJobCache.Instance.Rules[new Guid(file.RuleId)].Name,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = e.Message,
                                    Type = "RM_JS_Rule_ObjectLevel_FSFile"
                                });
                            }
                            catch (Exception itemex)
                            {
                                logger.Error("Failed to process item. Object:{0}, Exception:{1}", ExternalUtil.CombinePath(FSJobCache.Instance.RootPath.LogBase64(), file.HighName.LogBase64(), file.LowName.LogBase64()), itemex.ToString());
                                ProgressService.Increase();
                                FSJobCache.Instance.FailedCount++;
                                string errorCommont = string.Empty;
                                if (itemex.Message == "Incorrect action.")
                                {
                                    errorCommont = "RM_JM_FSFailedToDisposalFileActionIsIncorrect";
                                }
                                else
                                {
                                    errorCommont = "RM_JM_FSFailedToDisposalFile";
                                }
                                JobDetailService.Commit(new JMFSDisposalJobDetails
                                {
                                    ObjectName = file.LowName,
                                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName),
                                    Size = ExternalUtil.ConvertToFormatSize(file.Size),
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    Action = GetActionString(file.RuleAction),
                                    RuleName = FSJobCache.Instance.Rules[new Guid(file.RuleId)].Name,
                                    //DetailTab = DetailTab.Deletion.ToString(),
                                    Status = JobDetailsStatus.Failed,
                                    Comment = errorCommont,
                                    Type = "RM_JS_Rule_ObjectLevel_FSFile"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Analyzer thread occurs an unexpected Error. Exception:{0}", ex.ToString());
                //JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, ex.Message);
            }
            finally
            {
                ClearBackUpSenderAndRemoveFile();
                FSJobCache.Instance.AnalyzerThreadMonitor.Decrement();
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
                tempDevice = ExternalUtil.OpenXSystem(AppDomain.CurrentDomain.BaseDirectory);
                tempDevice.DeleteDirectory(new StorageInfo() { HighName = BackgroundSettings.GetInstance().InternalArchiveCache });
            }
            catch (Exception e)
            {
                logger.Error($"Failed to delete cache folder.{e}");
            }
        }

        private void AddReport(FSAzureTableEntityDto dto, int status)
        {
            JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
            {
                ObjectName = dto.LowName,
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dto.HighName),
                Size = ExternalUtil.ConvertToFormatSize(dto.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = GetActionString(dto.RuleAction),
                RuleName = FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name,
                //DetailTab = DetailTab.Deletion.ToString(),
                Status = status >= 0 ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                Comment = status >= 0 ? ".File has been deleted" : ".Failed to delete this file.",
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName
            };
            JobDetailService.Commit(detail);
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

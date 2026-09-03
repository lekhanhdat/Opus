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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using static AvePoint.RA.FileSystem.Stubs.Stub;
using System.Net.Http;

namespace AvePoint.RA.FileSystem.Collect
{
    public class DataSyncAnalyzerThread
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DataSyncAnalyzerThread()
        {
            FileAnalyzer = new FSObjectAnalyzer();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            JobContext.Current.Count = 0;
        }
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private FSObjectAnalyzer FileAnalyzer { get; set; }
       
        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("AnalyzerThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.AnalyzerThreadMonitor.Increment();
                while (true)
                {
                    if (FSJobCache.Instance.AnalyzerCache.Count == 0 && FSJobCache.Instance.DiscoveryCache.Count == 0 && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0)
                    {
                        logger.Info("There is no obj to be analyzed,nor any discovery thread running, analyzer thread [{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                        break;
                    }
                    //discoveyr thread is still running.Wait 1 sec for new objects.
                    if (FSJobCache.Instance.AnalyzerCache.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    IEnumerable<Stub> stubs = FSJobCache.Instance.AnalyzerCache.Take(100);
                    logger.Info("Analyzer got {1} stubs. There are {0} records to be analyzed left in the cache.", FSJobCache.Instance.AnalyzerCache.Count, stubs.Count());
                    foreach (Stub stub in stubs)
                    {
                        JobContext.Current.Count++;
                        try
                        {
                            if (FSJobCache.Instance.JobController.JobType == FSJobType.RematchRuleFullJob && stub.Type == StubType.File)
                            {
                                if (NeedSkipForRematchRule(stub))
                                {
                                    logger.Debug($"This file [{stub.FullPath.LogBase64()}] will be skipped, term or file is not changed.");
                                    ProgressService.Increase();
                                    continue;
                                }
                            }
                            FileSystemRecordDto record = FileAnalyzer.Analyze(stub, (int)FSDataCollector.ClassificationLevel);
                            if ((stub.Type == Stub.StubType.File && record.FileSize != 0)
                                || (stub.Type == Stub.StubType.Folder && (stub.failedInPreJob || FilterdIn(new XDirectoryInfoEx(stub.MediaObj))))
                                || stub.Type == Stub.StubType.ConnectionGroup
                                || stub.Type == Stub.StubType.ConnectionGroups
                                )
                            {
                                FSJobCache.Instance.RecordCache.Add(record);
                            }
                            else
                            {
                                logger.Debug("Skip record {0}", record.LeafName.LogBase64());
                                ProgressService.Increase();
                            }
                            if (FSJobCache.Instance.LastJobFailedItemIds.Contains(stub.SelfId)
                                && !FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Contains(stub.SelfId))
                            {
                                FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Add(stub.SelfId);
                            }
                        }
                        catch (HttpRequestException ex) 
                        {
                            logger.Error("An error occurred while sending the crop：{0}", ex.Message.ToString());
                            JobContext.Current.Count--;
                        }
                        catch (Exception itemex)
                        {
                            logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath.LogBase64(), itemex.ToString());
                            ProgressService.Increase();
                            FSJobCache.Instance.FailedCount++;
                            string comment = itemex.Message.StartsWith("RM_FS_DisposalDetail_TermIsInvalid", StringComparison.OrdinalIgnoreCase) ?
                                itemex.Message : "RM_JM_FSFailedAddToExplorer";
                            JobDetailService.Commit(
                                 new FSDataSyncJobReportDetail()
                                 {
                                     AgentName = OSInformation.HostName,
                                     ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                                     FullPath = stub.FullPath,
                                     Status = JobDetailsStatus.Failed,
                                     Comment = comment
                                 }
                               );
                            Add2FailedItemCache(stub, itemex);
                            //2 是因为它会提前扫描File System Connection Groups 和FS Group
                            if (JobContext.Current.Count == 2 + FSJobCache.Instance.FailedCount)
                            {
                                JobContext.Current.AllErrorNode = true;
                            }
                            else
                            {
                                JobContext.Current.AllErrorNode = false;
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
                FSJobCache.Instance.AnalyzerThreadMonitor.Decrement();
            }
        }

        private void Add2FailedItemCache(Stub stub, Exception e)
        {
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling && !FSJobCache.Instance.LastJobFailedItemIds.Contains(stub.SelfId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = stub.Type == StubType.File? stub.FullPath.Substring(FSJobCache.Instance.RootPath.Length + 1): stub.FullPath,
                    SortTicks = Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = stub.MediaObj.Name,
                    Message = GetExceptionMessage(e)
                };
                item.NodeId = stub.Type == StubType.Folder 
                    ? stub.FullPath.ToLowerInvariant().ToMd5().ToString() 
                    : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath,stub.MediaObj.HighName, stub.MediaObj.LowName).Substring(FSJobCache.Instance.RootPath.Length + 1).ToLowerInvariant().ToMd5().ToString();
                FSJobCache.Instance.FailedItems.Add(item);
            }
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }

        private bool NeedSkipForRematchRule(Stub stub)
        {
            var dbRecord = stub.DBRecord;
            XFileInfoEx xObj = new XFileInfoEx(stub.MediaObj);
            if (xObj.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                || xObj.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime)
            {
                return false;
            }

            if (dbRecord != null)
            {
                if (FSJobCache.Instance.ChangedTermIds != null && dbRecord.TermId != null && dbRecord.TermId != Guid.Empty && FSJobCache.Instance.ChangedTermIds.Contains(dbRecord.TermId))
                {
                    return false;
                }
            }

            return true;
        }
        private bool FilterdIn(XDirectoryInfoEx t)
        {
            if (t.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0) { return false; }
            switch (FSJobCache.Instance.JobController.JobType)
            {
                case FSJobType.UserFullJob:
                case FSJobType.RematchRuleFullJob:
                    return true;
                case FSJobType.IncrementalJob:
                    return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime);
                default:
                    logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }

    }

}

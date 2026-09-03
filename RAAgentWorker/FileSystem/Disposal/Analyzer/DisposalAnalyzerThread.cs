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
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.Disposal.Analyzer
{
    public class DisposalAnalyzerThread
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public DisposalAnalyzerThread()
        {
            FileAnalyzer = new FSObjectAnalyzer();
            ProgressService = JobContext.Current.ProgressManager.Create();
            ReportService = JobContext.Current.JobDetailManager.Create();
        }
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> ReportService { get; set; }
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
                    try
                    {
                        if (stubs.Count() > 0)
                        {
                            FileAnalyzer.AssembleDBRecords(stubs);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while getting records from explorer db. Error: " + e.ToString());
                        continue;
                    }

                    logger.Info("Analyzer got {1} stubs. There are {0} records to be analyzed left in the cache.", FSJobCache.Instance.AnalyzerCache.Count, stubs.Count());
                    foreach (Stub stub in stubs)
                    {
                        try
                        {
                            FileSystemRecordDto record = FileAnalyzer.Analyze(stub);
                            if (stub.Type == Stub.StubType.File
                                || (stub.Type == Stub.StubType.Folder && FilterdIn(stub.MediaObj as XDirectoryInfo))
                                || stub.Type == Stub.StubType.ConnectionGroup
                                || stub.Type == Stub.StubType.ConnectionGroups
                                )
                            {
                                FSJobCache.Instance.RecordCache.Add(record);
                            }
                            else
                            {
                                ProgressService.Increase();
                            }
                        }
                        catch (Exception itemex)
                        {
                            logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath, itemex.ToString());
                            ProgressService.Increase();
                            FSJobCache.Instance.FailedCount++;
                            ReportService.Commit(new FSCollectJobReportEntry(Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                                stub.FullPath,
                                stub.Type.ToString(),
                                itemex.Message));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("Analyzer thread occurs an unexpected Error. Exception:{0}", ex.ToString());
                JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, ex.Message);
            }
            finally
            {
                FSJobCache.Instance.AnalyzerThreadMonitor.Decrement();
            }
        }
        private bool FilterdIn(XDirectoryInfo t)
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

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
using AvePoint.Hybrid.Utility.AveCommonLogger;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AvePoint.RA.FileSystem.Collect
{
    internal class FSDiscover
    {
        private AveRALogger logger = AveRALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem _system;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        public FSDiscover()
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
        }

        public void Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("DiscoveryThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                while (true)
                {
                    //there is no file/folder to be processed and also there is no discovery thread working on..   thread exit..
                    if (FSJobCache.Instance.DiscoveryCache.Count == 0 && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0)
                    {
                        logger.Info("There is no more task. Discovery thread[{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                        break;
                    }
                    //someone is till working. wait 1 sec for new objects.
                    if (FSJobCache.Instance.DiscoveryCache.Count == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    // try to get new file/folder..
                    try
                    {
                        FSJobCache.Instance.DiscoverThreadMonitor.Increment();
                        IEnumerable<Stub> stubs = FSJobCache.Instance.DiscoveryCache.Take(30).Where(t => t.Type == Stub.StubType.Folder);
                        logger.Debug("FSDiscover got {1} folders. There are {0} folders to be discovered left in the cache.", FSJobCache.Instance.DiscoveryCache.Count, stubs.Count());
                        //using (new RAPerformanceScope(string.Format("FSDiscover--process {0} folders", stubs.Count())))
                        {
                            foreach (Stub stub in stubs)
                            {
                                try
                                {
                                    logger.Debug("Begin to query folder:{0}", stub.FullPath);
                                    QueryFiles(stub);
                                    QuerySubFolders(stub);
                                }
                                catch (Exception itemex)
                                {
                                    logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath, itemex.ToString());
                                    ProgressService.Increase();
                                    FSJobCache.Instance.FailedCount++;
                                    JobDetailService.Commit(
                                           new FSCollectJobReportEntry()
                                           {
                                               ObjName = Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                                               Url = stub.FullPath,
                                               Status = JobDetailsStatus.Failed,
                                               Comment = itemex.Message
                                           }
                                    );
                                }
                            }
                        }
                    }
                    finally
                    {
                        FSJobCache.Instance.DiscoverThreadMonitor.Decrement();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to discover the files. Exception:{0}", ex.ToString());
                //JobContext.Current.JobSummaryService.NotifyManager((int)AvePoint.Common.JobState.Failed, ex.Message);
            }
        }


        private void QuerySubFolders(Stub stub)
        {
            //using (new RAPerformanceScope("FSDiscover--QuerySubFolders"))
            {
                //List Dirs and add them to cache
                List<XDirectoryInfo> dirs = _system.ListDirectories(stub.MediaObj);
                List<Stub> dirStubs = new List<Stub>();
                foreach (XDirectoryInfo dir in dirs)
                {
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    Guid id = fullPath.ToLowerInvariant().ToMd5();
                    Guid termSettingId = stub.ScopeSettingId;
                    if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                    {
                        logger.Debug("The folder node {0}  has unique setting.", fullPath);
                        continue;
                    }
                    dirStubs.Add(new FSFolderStub
                    {
                        FullPath = fullPath,
                        MediaObj = dir,
                        ScopeSettingId = termSettingId,
                        SelfId = fullPath.ToLowerInvariant().ToMd5(),
                        ParentId = stub.SelfId
                    });
                }
                FSJobCache.Instance.AnalyzerCache.AddBatch(dirStubs);
                FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                logger.Info("Found {0} new folders", dirs.Count);
                ProgressService.IncreaseBase(dirStubs.Count);
            }
        }

        private void QueryFiles(Stub stub)
        {
            //using (new RAPerformanceScope("FSDiscover--QueryFiles"))
            {
                //List Files and add them to cache
                List<XFileInfo> files = _system.ListFiles(stub.MediaObj);
                List<Stub> fileStubs = new List<Stub>();
                files.ForEach(t =>
                {
                    if (FilterdIn(t))
                    {
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                        fileStubs.Add(new FSFileStub
                        {
                            FullPath = fullPath,
                            MediaObj = t,
                            SelfId = fullPath.ToLowerInvariant().ToMd5(),
                            ParentId = stub.SelfId,
                            ScopeSettingId = stub.ScopeSettingId,
                        });
                    }
                });
                FSJobCache.Instance.AnalyzerCache.AddBatch(fileStubs);
                logger.Info("Found {0} files and {1} files filtered in", files.Count, fileStubs.Count);
                ProgressService.IncreaseBase(fileStubs.Count);
            }
        }

        private bool FilterdIn(XFileInfo t)
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

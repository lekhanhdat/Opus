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
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvePoint.GCommon;

namespace AvePoint.RA.FileSystem.Collect
{
    internal class FSDiscover
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem _system;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private const int BATCH_SIZE = 100;
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
                var currentConnectionSettings = GetCurrentConnectionAllSettings();
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
                        logger.Info("FSDiscover got {1} folders. There are {0} folders to be discovered left in the cache.", FSJobCache.Instance.DiscoveryCache.Count, stubs.Count());
                        using (new AgentPerformanceScope("FSDiscover.ProcessFolders", string.Format("FSDiscover.Process {0} Folders", stubs.Count()), true))
                        {
                            foreach (Stub stub in stubs)
                            {
                                if(!stub.failedInPreJob)
                                    stub.failedInPreJob = FailedInLastJob(stub.SelfId);
                                try
                                {
                                    if (FSDataCollector.ClassificationLevel == GCommon.Contract.Tree.Object.NodeLevel.FSFile)
                                    {
                                        logger.Debug("Begin to query files in folder:{0}", stub.FullPath.LogBase64());
                                        bool addedFolder = false;
                                        foreach (var files in QueryFilesInBatch(stub))
                                        {
                                            if (files.Count > 0)
                                            {
                                                var fileIds = files.Select(f => f.SelfId).ToList();
                                                if (!addedFolder)
                                                {
                                                    fileIds.Add(stub.SelfId);
                                                }
                                                var explorerRecords = GetFSDBRecords(fileIds).GroupBy(n => n.NodeId).ToDictionary(k => k.Key, v => v.ToList());
                                                if (!addedFolder && explorerRecords.ContainsKey(stub.SelfId))
                                                {
                                                    stub.DBRecord = explorerRecords[stub.SelfId].FirstOrDefault();
                                                }

                                                if (!addedFolder)
                                                {
                                                    FSJobCache.Instance.AnalyzerCache.Add(stub);
                                                    addedFolder = true;
                                                }
                                                files.ForEach(f =>
                                                {
                                                    if (explorerRecords.ContainsKey(f.SelfId))
                                                    {
                                                        f.DBRecord = explorerRecords[f.SelfId].FirstOrDefault();
                                                    }
                                                });
                                                FSJobCache.Instance.AnalyzerCache.AddBatch(files);
                                            }
                                        }
                                        QuerySubFoldersFileLevelInBatch(stub);
                                    }
                                    else
                                    {
                                        //下一行 改在获取子路径之后， 可以保证对Folder是有权限的
                                        //与Parent Folder Setting不一致的Folder必定存在自己的Setting，获取自己的setting中的Term信息即可
                                        QuerySubFoldersInBatch(stub, currentConnectionSettings);
                                    }
                                }
                                catch (Exception itemex)
                                {
                                    logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath.LogBase64(), itemex.ToString());
                                    ProgressService.Increase();
                                    FSJobCache.Instance.FailedCount++;
                                    JobDetailService.Commit(
                                           new FSDataSyncJobReportDetail()
                                           {
                                               AgentName = OSInformation.HostName,
                                               ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                                               FullPath = stub.FullPath,
                                               Status = JobDetailsStatus.Failed,
                                               Comment = "RM_JM_FSFailedToDiscoverFolder"
                                           }
                                    );
                                    Add2FailedItemCache(stub); //失败的Folder加入failed table 
                                    JobContext.Current.HasErrorNode = true;
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

        private List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId)
        {
            List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
            try
            {

                long sortTicks = DateTime.MinValue.Ticks;
                while (true)
                {
                    List<FileSystemRecordDto> data = new List<FileSystemRecordDto>();
                    using (new AgentPerformanceScope("FSDicover.GetDBRecordsByFolder", addToStatistics: true))
                    {
                        data = JobContext.Current.ApiClient.GetDBRecordsByFolder(folderId, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(), sortTicks, ExternalUtil.TransferDataCount);
                    }
                    if (data != null && data.Count > 0)
                    {
                        folderRecords.AddRange(data);
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetDBRecordsByFolder. Folder Id:{0} Error:{1}", folderId, e.ToString());
            }
            return folderRecords;
        }



        private List<FileSystemRecordDto> GetFSDBRecords(List<Guid> ids)
        {
            List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
            try
            {
                List<FileSystemRecordDto> data = new List<FileSystemRecordDto>();
                for (int i = 0; i < ids.Count; i += 100)
                {
                    using (new AgentPerformanceScope("FSDicover.GetFSDBRecords", addToStatistics: true))
                    {
                        var tempIds = ids.Skip(i).Take(100).ToList();
                        data = JobContext.Current.ApiClient.GetFSDBRecords(tempIds);
                        if (data != null && data.Count > 0)
                        {
                            folderRecords.AddRange(data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetFSDBRecords. Error:{0}", e.ToString());
            }
            return folderRecords;
        }
        /// <summary>
        /// 下一层folder term与Parent不同的数据
        /// </summary>  
        private List<FSFolderCacheDto> GetFSFolderWithoutInheritTerm(Stub stub)
        {
            if (FSJobCache.Instance.JobController.JobType != FSJobType.UserFullJob)
            {
                //Full JOb不获取
                using (new AgentPerformanceScope("FSDicover.GetFSFolderWithoutInheritTerm", addToStatistics: true))
                {
                    var parentId = stub.SelfId.ToString();
                    var termId = stub.TermId4Folder.ToString();
                    var data = JobContext.Current.ApiClient.GetFSFolderWithoutInheritTerm(parentId, termId);
                    return data == null ? new List<FSFolderCacheDto>() : data;
                } 
            }
            return new List<FSFolderCacheDto>();
        }

        private List<FSFolderCacheDto> GetCurrentConnectionAllSettings()
        {
            using (new AgentPerformanceScope("FSDicover.GetCurrentConnectionAllSettings", addToStatistics: true))
            {                   
                var data = JobContext.Current.ApiClient.GetCurrentConnectionAllSettings(FSJobCache.Instance.RootPath);
                return data == null ? new List<FSFolderCacheDto>() : data;
            }
        }

        private void QuerySubFoldersFileLevel(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
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
                        logger.Debug("The folder node {0}  has unique setting.", fullPath.LogBase64());
                        continue;
                    }
                    if (HasRunningJob(fullPath.ToLowerInvariant()))
                    {
                        logger.Debug("There is already a job running on this node.id:{0}",id);
                        continue;
                    }
                    dirStubs.Add(new FSFolderStub
                    {
                        FullPath = fullPath,
                        MediaObj = dir,
                        ScopeSettingId = termSettingId,
                        SelfId = fullPath.ToLowerInvariant().ToMd5(),
                        ParentId = stub.SelfId,
                        failedInPreJob = stub.failedInPreJob,
                    });
                }
                //FSJobCache.Instance.AnalyzerCache.AddBatch(dirStubs);
                FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                logger.Info("Found {0} new folders", dirs.Count);
                ProgressService.IncreaseBase(dirStubs.Count);
            }
        }
        
        private void QuerySubFoldersFileLevelInBatch(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
            {
                //List Dirs and add them to cache
                var dirsCollection = _system.GetDirectoriesInBatch(stub.MediaObj, 100);
                foreach (var dirs in dirsCollection)
                {
                    List<Stub> dirStubs = new List<Stub>();
                    foreach (XDirectoryInfo dir in dirs)
                    {
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                        Guid id = fullPath.ToLowerInvariant().ToMd5();
                        Guid termSettingId = stub.ScopeSettingId;
                        if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                        {
                            logger.Debug("The folder node {0}  has unique setting.", fullPath.LogBase64());
                            continue;
                        }
                        if (HasRunningJob(fullPath.ToLowerInvariant()))
                        {
                            logger.Debug("There is already a job running on this node.id:{0}",id);
                            continue;
                        }
                        dirStubs.Add(new FSFolderStub
                        {
                            FullPath = fullPath,
                            MediaObj = dir,
                            ScopeSettingId = termSettingId,
                            SelfId = fullPath.ToLowerInvariant().ToMd5(),
                            ParentId = stub.SelfId,
                            failedInPreJob = stub.failedInPreJob,
                        });
                    }
                    FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                    logger.Info("Found {0} new folders", dirs.Count);
                    ProgressService.IncreaseBase(dirStubs.Count);
                }
            }
        }

        private void QuerySubFolders(Stub stub, List<FSFolderCacheDto> differentTermSubFolders)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
            {
                //List Dirs and add them to cache
                List<XDirectoryInfo> dirs = _system.ListDirectories(stub.MediaObj);

                if (FilterdIn(new XDirectoryInfoEx(stub.MediaObj), stub.SelfId, stub.failedInPreJob))
                {
                    FSJobCache.Instance.AnalyzerCache.Add(stub);    //父路径加入Analyzer Cache
                }

                List<Stub> dirStubs = new List<Stub>();
                foreach (XDirectoryInfo dir in dirs)
                {
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    Guid id = fullPath.ToLowerInvariant().ToMd5();
                    Guid termSettingId = stub.ScopeSettingId;
                    if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                    {
                        logger.Debug("The folder node {0}  has unique setting.", fullPath.LogBase64());
                        continue;
                    }
                    if (HasRunningJob(fullPath.ToLowerInvariant()))
                    {
                        logger.Debug("There is already a job running on this node.id:{0}", id);
                        continue;
                    }
                    var newStub = new FSFolderStub
                    {
                        FullPath = fullPath,
                        MediaObj = dir,
                        ScopeSettingId = termSettingId,
                        SelfId = fullPath.ToLowerInvariant().ToMd5(),
                        ParentId = stub.SelfId, 
                        failedInPreJob = stub.failedInPreJob,
                        TermId4Folder = stub.TermId4Folder,
                        TermName4Folder = stub.TermName4Folder
                    };
                    dirStubs.Add(newStub);
                }
                //FSJobCache.Instance.AnalyzerCache.AddBatch(dirStubs);
                FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                logger.Info("Found {0} new folders", dirs.Count);
                ProgressService.IncreaseBase(dirStubs.Count);
            }
        }
        
        private void QuerySubFoldersInBatch(Stub stub, List<FSFolderCacheDto> differentTermSubFolders)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
            {
                //List Dirs and add them to cache
                var dirsCollection = _system.GetDirectoriesInBatch(stub.MediaObj, BATCH_SIZE);

                if (FilterdIn(new XDirectoryInfoEx(stub.MediaObj), stub.SelfId, stub.failedInPreJob))
                {
                    FSJobCache.Instance.AnalyzerCache.Add(stub);    //父路径加入Analyzer Cache
                }
                
                foreach (var dirs in dirsCollection)
                {
                    List<Stub> dirStubs = new List<Stub>();
                    foreach (XDirectoryInfo dir in dirs)
                    {
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                        Guid id = fullPath.ToLowerInvariant().ToMd5();
                        Guid termSettingId = stub.ScopeSettingId;
                        if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                        {
                            logger.Debug("The folder node {0}  has unique setting.", fullPath.LogBase64());
                            continue;
                        }
                        if (HasRunningJob(fullPath.ToLowerInvariant()))
                        {
                            logger.Debug("There is already a job running on this node.id:{0}", id);
                            continue;
                        }
                        var newStub = new FSFolderStub
                        {
                            FullPath = fullPath,
                            MediaObj = dir,
                            ScopeSettingId = termSettingId,
                            SelfId = fullPath.ToLowerInvariant().ToMd5(),
                            ParentId = stub.SelfId, 
                            failedInPreJob = stub.failedInPreJob,
                            TermId4Folder = stub.TermId4Folder,
                            TermName4Folder = stub.TermName4Folder
                        };
                        dirStubs.Add(newStub);
                    }
                    //FSJobCache.Instance.AnalyzerCache.AddBatch(dirStubs);
                    FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                    logger.Info("Found {0} new folders", dirs.Count);
                    ProgressService.IncreaseBase(dirStubs.Count);
                }
            }
        }

        private bool HasRunningJob(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCache.Instance.RunningJobNodeUrls != null && FSJobCache.Instance.RunningJobNodeUrls.Contains(sha1Url))
            {
                return true;
            }
            return false;
        }

        private List<Stub> QueryFiles(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QueryFiles", addToStatistics: true))
            {
                //List Files and add them to cache              
                List<XFileInfo> files = _system.ListFiles(stub.MediaObj);
                List<Stub> fileStubs = new List<Stub>();
                files.ForEach(t =>
                {
                    if (FilterdIn(new XFileInfoEx(t), stub.failedInPreJob))
                    {
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                        var fileId = fullPath.ToLowerInvariant().ToMd5();
                        fileStubs.Add(new FSFileStub
                        {
                            FullPath = fullPath,
                            MediaObj = t,
                            SelfId = fileId,
                            ParentId = stub.SelfId,
                            ScopeSettingId = stub.ScopeSettingId,
                            failedInPreJob = stub.failedInPreJob,
                        });
                    }
                });
                // FSJobCache.Instance.AnalyzerCache.AddBatch(fileStubs);
                logger.Debug("Found {0} files and {1} files filtered in", files.Count, fileStubs.Count);
                ProgressService.IncreaseBase(fileStubs.Count);
                return fileStubs;
            }
        }
        
        private IEnumerable<List<Stub>> QueryFilesInBatch(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QueryFiles", addToStatistics: true))
            {
                var files = _system.GetFilesInBatch(stub.MediaObj,BATCH_SIZE);
                var filesCount = 0;
                foreach (var batchFiles in files)
                {
                    var fileStubs = new List<Stub>(BATCH_SIZE);
                    batchFiles.ForEach(t =>
                    {
                        if (FilterdIn(new XFileInfoEx(t), stub.failedInPreJob))
                        {
                            string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                            var fileId = fullPath.ToLowerInvariant().ToMd5();
                            fileStubs.Add(new FSFileStub
                            {
                                FullPath = fullPath,
                                MediaObj = t,
                                SelfId = fileId,
                                ParentId = stub.SelfId,
                                ScopeSettingId = stub.ScopeSettingId,
                                failedInPreJob = stub.failedInPreJob,
                            });
                        }
                    });
                    yield return fileStubs;
                    ProgressService.IncreaseBase(fileStubs.Count);
                    filesCount += fileStubs.Count;
                }
                logger.Debug("Found {0} files filtered in {1}", filesCount, stub.MediaObj.HighName);
            }
        }


        private bool FilterdIn(XFileInfoEx t, bool folderFailedInLastJob)
        {
            if (t.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0) { return false; }
            //Filter hidden files
            if (t.IsHidden) { return false; }
            switch (FSJobCache.Instance.JobController.JobType)
            {
                case FSJobType.UserFullJob:
                case FSJobType.RematchRuleFullJob:
                    return true;
                case FSJobType.IncrementalJob:
                    return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.LastAccessTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime) 
                        || folderFailedInLastJob || FailedInLastJob(t);
                default:
                    logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }
        private bool FilterdIn(XDirectoryInfoEx t, Guid selfId, bool parentFailedInLastJob)
        {
            //Filter hidden folders
            if (t.IsHidden) { return false; }
            switch (FSJobCache.Instance.JobController.JobType)
            {
                case FSJobType.UserFullJob:
                case FSJobType.RematchRuleFullJob:
                    return true;
                case FSJobType.IncrementalJob:
                    return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.LastAccessTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime) || parentFailedInLastJob || FailedInLastJob(selfId);
                default:
                    logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }
        private bool FailedInLastJob(XFileInfoEx t)
        {
            var fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
            var nodeId = fullPath.Substring(FSJobCache.Instance.RootPath.Length + 1).ToLowerInvariant().ToMd5();
            if (FSJobCache.Instance.LastJobFailedItemIds.Contains(nodeId))
            { 
                return true;
            }
            else 
            { 
                return false; 
            }
        }
        private bool FailedInLastJob(Guid selfId)
        {
            //Guid fileId = ExternalUtil.CombinePath(t.HighName, t.LowName).ToMd5();
            if (FSJobCache.Instance.LastJobFailedItemIds.Contains(selfId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void Add2FailedItemCache(Stub stub)
        {
            var fullPath = stub.FullPath;
            Guid nodeId = stub.SelfId; 
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling && !FSJobCache.Instance.LastJobFailedItemIds.Contains(nodeId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = fullPath,
                    SortTicks = RAFileSystem.Utils.Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    NodeId = nodeId.ToString(),
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = stub.MediaObj.Name,
                    Message = "RM_JM_FSFailedAddToExplorer"
                };
                FSJobCache.Instance.FailedItems.Add(item);
            }
        }
    }
}

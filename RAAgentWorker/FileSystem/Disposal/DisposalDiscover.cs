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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystem.Disposal;
using RAFileSystem.Utils;
using RAFileSystemCore.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Common.Hybrid;
using AvePoint.GCommon;

namespace AvePoint.RA.FileSystem.Collect
{
    internal class DisposalDiscover
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IXSystem _system;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private MemoryListCacheService<FSAzureTableEntityDto> WaitingApprovalItemCache;
        private MemoryListCacheService<FSAzureTableEntityDto> DeleteManualItemCache;
        private MemoryListCacheService<FSAzureTableEntityDto> RejectItemCache;
        private MemoryListCacheService<FSAzureTableEntityDto> mCachedRecords;
        private MemoryListCacheService<FSAzureTableEntityDto> mMatchRuleData;
        public DisposalDiscover()
        {
            WaitingApprovalItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            DeleteManualItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            RejectItemCache = new MemoryListCacheService<FSAzureTableEntityDto>();
            mCachedRecords = new MemoryListCacheService<FSAzureTableEntityDto>();
            mMatchRuleData = new MemoryListCacheService<FSAzureTableEntityDto>();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
        }

        public void Run()
        {
            try
            {
                logger.Info("Classification Level {0}", FSDataDisposal.ClassificationLevel);
                Thread.CurrentThread.Name = string.Format("DiscoveryThread[{0}]", Thread.CurrentThread.ManagedThreadId);
                FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Increment();
                Thread t = new Thread(new ThreadStart(SendRecordsToCosmos));
                t.Start();
                Thread t1 = new Thread(new ThreadStart(GetRecordsFromCosmos));
                t1.Start();
                if (FSDataDisposal.ClassificationLevel != NodeLevel.FSFolder)
                {
                    RunFromFileLevelClassify();
                }
                else
                {
                    RunFromFolderLevelClassify();
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
                    JobContext.Current.DisposalScanFinish = true;
                    if (FSDataDisposal.ClassificationLevel == NodeLevel.FSFolder)
                    {
                        FSJobCache.Instance.DiscoverThreadMonitor.Decrement();
                    }
                    WaitForComplete();
                    FinalAddRejectFileToAzureTable();
                    FinalRemoveManualData();
                    FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Decrement();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while final update. Error:{0}", e.ToString());
                }
            }
        }

        private void WaitForComplete()
        {
            while (!JobContext.Current.SendDataToAzureTableFinish)
            {
                Thread.Sleep(5000);
                logger.Info("Waiting disposal scan finish.");
            }
        }


        private void RunFromFolderLevelClassify()
        {
            logger.Info("Run from folder level");
            FSJobCache.Instance.DiscoverThreadMonitor.Increment();
            while (true)
            {
                try
                {
                    if (FSJobCache.Instance.DisposalFSFolderCache.Count == 0)
                    {
                        logger.Info("There is no more task. Discovery Folder thread[{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                        break;
                    }
                    IEnumerable<FSFolderStub> folders = FSJobCache.Instance.DisposalFSFolderCache.Take(1);
                    logger.Debug("FSDiscover got [{1}] folders. There are [{0}] folders to be discovered left in the cache.", FSJobCache.Instance.DisposalFSFolderCache.Count, folders.Count());
                    using (new AgentPerformanceScope("FSDiscover.Process.Folders", string.Format("FSDiscover.Process {0} folders", folders.Count()), true))
                    {
                        foreach (var folder in folders)
                        {
                            try
                            {
                                logger.Debug("Begin to query folder id:{0}", folder?.SelfId);
                                var files = QueryFiles(folder);
                                if (files.Count > 0)
                                {
                                    //List<Guid> fileIds;
                                    //using (new AgentPerformanceScope("FSDiscover.Process.GetFileIds", addToStatistics: true))
                                    //{
                                    //    fileIds = files.Select(f => f.SelfId).ToList();
                                    //}
                                    //Dictionary<Guid, FileSystemRecordDto> azureRecords;
                                    //using (new AgentPerformanceScope("FSDiscover.Process.GetManualDictionary", addToStatistics: true))
                                    //{
                                    //    azureRecords = GetFSManualRecords(fileIds).ToDictionary(r => r.NodeId);
                                    //}
                                    AnalyzeFileFromFolder(files,/* azureRecords,*/ folder);
                                }
                                QuerySubFolders(folder);
                            }
                            catch (Exception itemex)
                            {
                                //FIXED
                                logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FullPath.LogBase64(), itemex.ToString());
                                ProgressService.Increase();
                                JobDetailService.Commit(new JMFSDisposalJobDetails
                                {
                                    ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(folder.FullPath),
                                    SourceLocation = folder.FullPath,
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    //DetailTab = DetailTab.Deletion.ToString(),
                                    Status = JobDetailsStatus.Failed,
                                    Comment = "RM_JM_FSFailedToDiscoverFolder",
                                    Type = "RM_JS_Rule_ObjectLevel_FSFolder"
                                });
                                FSJobCache.Instance.FailedCount++;
                            }
                        }
                    }
                }
                finally
                {
                    //FSJobCache.Instance.DiscoverThreadMonitor.Decrement();
                }
            }
        }

        /// <summary>
        /// new logic for folder level classification
        /// </summary>
        private void RunFromFileLevelClassify()
        {
            while (true)
            {
                //there is no file/folder to be processed and also there is no discovery thread working on..   thread exit..
                if (FSJobCache.Instance.DisposalFolderCache.Count == 0)
                {
                    logger.Info("There is no more task. Discovery thread[{0}] exiting....", Thread.CurrentThread.ManagedThreadId);
                    break;
                }
                //someone is till working. wait 1 sec for new objects.
                //if (FSJobCache.Instance.DiscoveryCache.Count == 0)
                //{
                //    Thread.Sleep(1000);
                //    continue;
                //}
                // try to get new file/folder..
                try
                {
                    FSJobCache.Instance.DiscoverThreadMonitor.Increment();
                    IEnumerable<FSDisposalDiscoverFolder> folders = FSJobCache.Instance.DisposalFolderCache.Take(5);
                    logger.Debug("FSDiscover got {1} folders. There are {0} folders to be discovered left in the cache.", FSJobCache.Instance.DisposalFolderCache.Count, folders.Count());
                    using (new AgentPerformanceScope("FSDiscover.Process.Folders", string.Format("FSDiscover.Process {0} folders", folders.Count()), true))
                    {
                        foreach (FSDisposalDiscoverFolder folder in folders)
                        {
                            try
                            {
                                logger.Debug("Begin to query folder id:{0}", folder?.FolderId);
                                var files = QueryFiles(folder);
                                if (files.Count > 0)
                                {
                                    var explorerRecords = GetDueRecordsInFolder(folder.FolderId).ToDictionary(r => r.NodeId);
                                    var azureRecords = GetDBRecordsByFolder(folder.FolderId.ToString()).ToDictionary(r => r.NodeId);
                                    AnalyzeFile(files, explorerRecords, azureRecords);
                                }
                                //QuerySubFolders(folder);
                            }
                            catch (Exception itemex)
                            {
                                logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FolderPath.LogBase64(), itemex.ToString());
                                ProgressService.Increase();
                                JobDetailService.Commit(new JMFSDisposalJobDetails
                                {
                                    ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(folder.FolderPath),
                                    SourceLocation = folder.FolderPath,
                                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                    //DetailTab = DetailTab.Deletion.ToString(),
                                    Status = JobDetailsStatus.Failed,
                                    Comment = "RM_JM_FSFailedToDiscoverFolder",
                                    Type = "RM_JS_Rule_ObjectLevel_FSFolder"
                                });
                                FSJobCache.Instance.FailedCount++;
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
        private List<FileSystemRecordDto> GetDueRecordsInFolder(Guid folderId)
        {
            using (new AgentPerformanceScope("FSDisposal.GetDueRecordsInFolder", addToStatistics: true))
            {
                FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
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
                        index++; ;
                        folderRecords.AddRange(records);
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

        private List<FileSystemRecordDto> GetFSManualRecords(List<Guid> ids)
        {
            List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
            try
            {
                List<FileSystemRecordDto> data = new List<FileSystemRecordDto>();
                for (int i = 0; i < ids.Count; i += 100)
                {
                    using (new AgentPerformanceScope("FSDicover.GetFSManualRecords", addToStatistics: true))
                    {
                        var tempIds = ids.Skip(i).Take(100).ToList();
                        data = JobContext.Current.ApiClient.GetFSManualRecords(tempIds);
                        if (data != null && data.Count > 0)
                        {
                            folderRecords.AddRange(data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while GetFSManualRecords. Error:{0}", e.ToString());
            }
            return folderRecords;
        }

        private List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId)
        {
            using (new AgentPerformanceScope("FSDiscover.GetAzureDataByFolder", addToStatistics: true))
            {
                List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
                long sortTicks = DateTime.MinValue.Ticks;
                while (true)
                {
                    var data = JobContext.Current.ApiClient.GetDBRecordsByFolder(folderId, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(), sortTicks, ExternalUtil.TransferDataCount);
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
                return folderRecords;
            }
        }
        #region used code
        //private List<FileSystemRecordDto> GetExplorerDataByFolder(string folderId)
        //{
        //    using (new AgentPerformanceScope("FSDiscover.GetExplorerDataByFolder", addToStatistics: true))
        //    {
        //        List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
        //        long sortTicks = DateTime.MinValue.Ticks;
        //        while (true)
        //        {
        //            var data = JobContext.Current.ApiClient.GetDBRecordsByFolder(folderId, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(), sortTicks, ExternalUtil.TransferDataCount);
        //            if (data != null && data.Count > 0)
        //            {
        //                folderRecords.AddRange(data);
        //            }
        //            if (data == null || data.Count < ExternalUtil.TransferDataCount)
        //            {
        //                break;
        //            }
        //            sortTicks = data[data.Count - 1].SortTicks;
        //        }
        //        return folderRecords;
        //    }
        //}
        private void QuerySubFolders(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
            {
                //List Dirs and add them to cache
                List<XDirectoryInfo> dirs = _system.ListDirectories(stub.MediaObj);
                List<FSFolderStub> dirStubs = new List<FSFolderStub>();
                foreach (XDirectoryInfo dir in dirs)
                {
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    Guid id = fullPath.ToLowerInvariant().ToMd5();
                    Guid termSettingId = stub.ScopeSettingId;
                    if (IsBreakInheritNode(fullPath.ToLowerInvariant()))
                    {
                        logger.Debug("The folder node {0} has unique setting.", fullPath.LogBase64());
                        continue;
                    }
                    if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                    {
                        if (!FSJobCache.Instance.ScopeSettingCache[id].IsActive)
                        {
                            logger.Debug("The folder node {0}  has been deactived.", fullPath.LogBase64());
                            continue;
                        }
                    }
                    if (HasRunningJob(fullPath.ToLowerInvariant()))
                    {
                        logger.Debug("There is already a job running on this node. Path:{0}", fullPath.LogBase64());
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
                //FSJobCache.Instance.DiscoveryCache.AddBatch(dirStubs);
                FSJobCache.Instance.DisposalFSFolderCache.AddBatch(dirStubs);
                logger.Info("Found {0} new folders", dirs.Count);
                ProgressService.IncreaseBase(dirStubs.Count);
            }
        }

        private bool IsBreakInheritNode(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCache.Instance.BreakNodeUrls != null && FSJobCache.Instance.BreakNodeUrls.Contains(sha1Url))
            {
                return true;
            }
            return false;
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
        #endregion

        private List<FSFileStub> QueryFiles(FSDisposalDiscoverFolder stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QueryFiles", addToStatistics: true))
            {
                //folder no longer exist
                using (var _system = ExternalUtil.OpenXSystem(stub.FolderPath))
                {
                    List<XFileInfo> files = _system.ListFiles(new StorageInfo());
                    List<FSFileStub> fileStubs = new List<FSFileStub>();
                    if (files.Count > 0)
                    {
                        bool resetHighName = !FSJobCache.Instance.RootPath.Equals(stub.FolderPath, StringComparison.OrdinalIgnoreCase);
                        string tempPath = stub.FolderPath.Substring(FSJobCache.Instance.RootPath.Length, stub.FolderPath.Length - FSJobCache.Instance.RootPath.Length).Trim('\\');
                        foreach (var t in files)
                        {
                            if (FilterdIn(new XFileInfoEx(t)))
                            {
                                if (resetHighName)
                                {
                                    t.HighName = tempPath;
                                }
                                string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                                logger.Debug("Start to process file.id :{0}", fullPath.ToLowerInvariant().ToMd5());
                                FSFileStub fileStub = new FSFileStub()
                                {
                                    FullPath = fullPath,
                                    MediaObj = t,
                                    SelfId = fullPath.ToLowerInvariant().ToMd5(),
                                    ParentId = stub.FolderId,
                                    ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId
                                };
                                fileStubs.Add(fileStub);
                            }
                        }
                    }
                    return fileStubs;
                }
            }
        }
        private List<FSFileStub> QueryFiles(FSFolderStub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QueryFiles", addToStatistics: true))
            {
                //folder no longer exist
                using (var _system = ExternalUtil.OpenXSystem(stub.FullPath))
                {
                    List<XFileInfo> files = _system.ListFiles(new StorageInfo());
                    List<FSFileStub> fileStubs = new List<FSFileStub>();
                    if (files.Count > 0)
                    {
                        bool resetHighName = !FSJobCache.Instance.RootPath.Equals(stub.FullPath, StringComparison.OrdinalIgnoreCase);
                        string tempPath = stub.FullPath.Substring(FSJobCache.Instance.RootPath.Length, stub.FullPath.Length - FSJobCache.Instance.RootPath.Length).Trim('\\');
                        foreach (var t in files)
                        {
                            if (FilterdIn(new XFileInfoEx(t)))
                            {
                                if (resetHighName)
                                {
                                    t.HighName = tempPath;
                                }
                                string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                                logger.Debug("Start to process file.id :{0}", fullPath.ToLowerInvariant().ToMd5());
                                FSFileStub fileStub = new FSFileStub()
                                {
                                    FullPath = fullPath,
                                    MediaObj = t,
                                    SelfId = fullPath.ToLowerInvariant().ToMd5(),
                                    ParentId = stub.SelfId,
                                    ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId
                                };
                                fileStubs.Add(fileStub);
                            }
                        }
                    }
                    return fileStubs;
                }
            }
        }
        private void AnalyzeFileFromFolder(List<FSFileStub> files, /*Dictionary<Guid, FileSystemRecordDto> azureCache,*/ FSFolderStub folder)
        {
            Guid termId = Guid.Empty;
            bool isHold = false;
            string termName = String.Empty;
            FileSystemRecordDto existFolder;
            using (new AgentPerformanceScope("FSDiscover.Process.AssignFolderTermId", addToStatistics: true))
            {
                termId = FSDataDisposal.currentSetting.DefaultTermId;
                existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
                if (existFolder != null)
                {
                    //如果当前Folder的Term与Default Value不同， 使用当前值
                    termId = existFolder.TermId;
                    //FIXED
                    logger.Info($"Different term {termId} on folder {folder.FullPath.LogBase64()}");
                    if (existFolder.HoldStatus && existFolder.HoldReleaseTime > DateTime.UtcNow.Ticks)
                    {
                        //在Folder level判断是否HOld
                        isHold = true;
                    }
                }
                if (termId == null || termId.Equals(Guid.Empty))
                {
                    //如果Term Id是空 不处理 也不报错
                    logger.Warn("Term is empty on folder {0}", folder.FullPath.LogBase64());
                    return;
                }
                //获取Term Name用于生成AzureTable数据
                termName = FSJobCache.Instance.Terms.ContainsKey(termId) ? FSJobCache.Instance.Terms[termId].Name : null;
            }
            foreach (var fileStub in files)
            {
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
                            var filteredRules = RuleUtil.FilterMoveRules(rules, Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileStub.FullPath)).Where(x => x.FSRule != null).ToList();
                            if (filteredRules.Count > 0)
                            {
                                DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                                var hasOwnerRule = HasOwnerRule(filteredRules);
                                ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(fileStub.MediaObj), FSJobCache.Instance.RootPath, hasOwnerRule);
                                var matchedRule = engine.MatchPotentialRule(filterObject);
                                if (matchedRule != null && matchedRule.Item1 != null && !string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                                {
                                    var rule = matchedRule.Item1;
                                    if (IsRemoveRule(rule))
                                    {
                                        if (isHold)
                                        {
                                            //结合Rule 类型和HOld值 判断是否Skip
                                            logger.Info("This file in on hold and current rule is remove rule, will be skipped.");
                                            AddSkipReport(fileStub, rule);
                                            continue;
                                        }
                                    }
                                    dto = CreateAzureEntityDto(fileStub, rule, folder, existFolder, termId, termName);

                                    if (!rule.FSRule.IsManualApproval)
                                    {
                                        logger.Debug("Not manual rule, file id:{0}", id);
                                        dto.ScanTime = DateTime.UtcNow;
                                        FSJobCache.Instance.DisposalScanCache.Add(dto);
                                    }
                                    else
                                    {
                                        ProcessMatchRuleData(dto);
                                    }
                                }
                                else
                                {
                                    //if (azureCache.ContainsKey(id))
                                    //{
                                    //    logger.Info($"Current file not match rule and has manual reocrd.FSPath:{fileStub.FullPath}.");
                                    //    var manualData = CreateAzureEntityDto(fileStub, null, folder, existFolder, termId, termName);
                                    //    RemoveManualData(manualData);
                                    //}
                                    //else
                                    {
                                        //FIXED
                                        logger.Info($"Current file not match rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                                    }
                                }
                            }
                            else
                            {
                                //FIXED
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
                        //FIXED
                        logger.Error($"Error occurred while disposal file:{fileStub.FullPath.LogBase64()} Error:{e.ToString()}");
                        JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
                        {
                            ObjectName = dto.HighName,
                            SourceLocation = fileStub.FullPath,
                            Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                            FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                            Action = dto.RuleId == null ? "" : FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId)) ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[new Guid(dto.RuleId)])) : "",
                            RuleName = dto.RuleId == null ? "" : FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId)) ? FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name : "",
                            //DetailTab = DetailTab.Deletion.ToString(),
                            Status = JobDetailsStatus.Failed,
                            Comment = e.Message,
                            Type = "RM_JS_Rule_ObjectLevel_FSFile",
                            AgentName = OSInformation.HostName
                        };
                        JobDetailService.Commit(detail);
                        FSJobCache.Instance.FailedCount++;
                    }

                }
            }
        }

        private void ProcessMatchRuleData(FSAzureTableEntityDto entity)
        {
            FSJobCache.Instance.DisposalCosmosDBData.Add(entity);
        }

        private void RealProcess(FSAzureTableEntityDto dto, Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            using (new AgentPerformanceScope("FSDicover.RealProcess", addToStatistics: true))
            {
                try
                {
                    var rule = FSJobCache.Instance.Rules.FirstOrDefault(r => r.Key == new Guid(dto.RuleId)).Value;
                    var id = dto.FilePathMd5;
                    if (rule.FSRule.IsManualApproval)
                    {
                        if (azureCache.ContainsKey(id))
                        {
                            var azureRecord = azureCache[id];
                            dto.MovedToApprovalTable = true;
                            dto.Status = azureRecord.ManualApprovedStatus;
                            dto.ScanTime = DateTime.UtcNow;
                            if (azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                                {
                                    logger.Debug("cosmos record not exist, file path:{0}", dto?.FullPath.LogBase64());
                                    AddSkipReport(dto);
                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                    dto.ScanTime = DateTime.UtcNow;
                                    OnlyUpdateAzureTable(dto);
                                }
                                else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                                {
                                    logger.Debug("File approved, file id:{0}", id);
                                    //AddToAzureTableAndArchive(dto);
                                    FSJobCache.Instance.DisposalScanCache.Add(dto);
                                }
                                else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.KeepData
                                    || azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.CheckOption
                                    || azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                                {
                                    logger.Debug("File status is {0}, file id:{1}", azureRecord.ManualApprovedStatus, id);
                                    AddSkipReport(dto);
                                }
                                else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                                {
                                    logger.Debug("File status is Rejected, file id:{0}", id);
                                    //XFileInfoEx xFile = new XFileInfoEx(fileStub.MediaObj);
                                    //if (azureRecord.ManualCollectionTime < xFile.LastWriteTimeUtc.Ticks)
                                    //{
                                    if (azureRecord.IsManualSynced && azureRecord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                                    {
                                        logger.Debug("item is manualsync and its extend");
                                        return;
                                    }
                                    AddRejectFileToAzureTable(dto);
                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                    dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
                                    UpdateRejectDto(dto);
                                }
                                else
                                {
                                    logger.Warn("Invalid status. File id:" + id);
                                }
                            }
                            else
                            {
                                logger.Debug("File rule id chaned, file id:{0}", id);
                                //only update ruleId in azure table
                                dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                dto.MovedToApprovalTable = false;
                                dto.ScanTime = DateTime.UtcNow;
                                OnlyUpdateAzureTable(dto);
                            }
                        }
                        else
                        {
                            logger.Debug("Azure table not exist, file path:{0}", id);
                            AddSkipReport(dto);
                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                            dto.ScanTime = DateTime.UtcNow;
                            OnlyUpdateAzureTable(dto);
                        }
                    }

                }
                catch (Exception e)
                {
                    //FIXED
                    logger.Error($"Error occurred while disposal file:{dto.FullPath.LogBase64()} Error:{e.ToString()}");
                    JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
                    {
                        ObjectName = dto.HighName,
                        SourceLocation = dto.FullPath,
                        Size = ExternalUtil.ConvertToFormatSize(dto.Size),
                        FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                        Action = dto.RuleId == null ? "" : FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId)) ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[new Guid(dto.RuleId)])) : "",
                        RuleName = dto.RuleId == null ? "" : FSJobCache.Instance.Rules.ContainsKey(new Guid(dto.RuleId)) ? FSJobCache.Instance.Rules[new Guid(dto.RuleId)].Name : "",
                        //DetailTab = DetailTab.Deletion.ToString(),
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                        Type = "RM_JS_Rule_ObjectLevel_FSFile",
                        AgentName = OSInformation.HostName
                    };
                    JobDetailService.Commit(detail);
                    FSJobCache.Instance.FailedCount++;
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

        private void AnalyzeFile(List<FSFileStub> files, Dictionary<Guid, FileSystemRecordDto> explorerCache, Dictionary<Guid, FileSystemRecordDto> azureCache)
        {
            foreach (var fileStub in files)
            {
                using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.AnalyzeFile", addToStatistics: true))
                {
                    logger.Debug("Start to analyze file, file id:{0}", fileStub?.SelfId);
                    var id = fileStub.SelfId;
                    if (explorerCache.ContainsKey(id))
                    {
                        var dbReord = explorerCache[id];
                        try
                        {
                            if (dbReord.TermId == null || dbReord.TermId.Equals(Guid.Empty))
                            {
                                continue;
                            }
                            else
                            {
                                if (FSJobCache.Instance.TermRuleMapping.ContainsKey(dbReord.TermId))
                                {
                                    var rules = FSJobCache.Instance.TermRuleMapping[dbReord.TermId];
                                    var filteredRules = RuleUtil.FilterMoveRules(rules, Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileStub.FullPath)).Where(x => x.FSRule != null).ToList();
                                    if (filteredRules.Count > 0)
                                    {
                                        DisposalRuleEngine engine = new DisposalRuleEngine(filteredRules);
                                        ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(fileStub.MediaObj), FSJobCache.Instance.RootPath);
                                        var matchedRule = engine.MatchPotentialRule(filterObject);
                                        if (matchedRule != null && matchedRule.Item1 != null && !string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                                        {
                                            var rule = matchedRule.Item1;
                                            if (IsRemoveRule(rule))
                                            {
                                                if (dbReord.HoldStatus && dbReord.HoldReleaseTime > DateTime.UtcNow.Ticks)
                                                {
                                                    logger.Info("This file in on hold and current rule is remove rule, will be skipped.");
                                                    AddSkipReport(fileStub, rule);
                                                    continue;
                                                }
                                            }
                                            var dto = CreateAzureEntityDto(fileStub, rule, dbReord);
                                            if (rule.FSRule.IsManualApproval)
                                            {
                                                if (azureCache.ContainsKey(id))
                                                {
                                                    var azureRecord = azureCache[id];
                                                    dto.Status = azureRecord.ManualApprovedStatus;
                                                    dto.ScanTime = DateTime.UtcNow;
                                                    if (azureRecord.RuleId.ToString().Equals(rule.Id, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.None)
                                                        {
                                                            logger.Debug("cosmos record not exist, file path:{0}", fileStub?.SelfId);
                                                            //AddSkipReport(dto);
                                                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                                            dto.ScanTime = DateTime.UtcNow;
                                                            OnlyUpdateAzureTable(dto);
                                                        }
                                                        else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                                                        {
                                                            logger.Debug("File approved, file id:{0}", fileStub?.SelfId);
                                                            //AddToAzureTableAndArchive(dto);
                                                            FSJobCache.Instance.DisposalScanCache.Add(dto);
                                                        }
                                                        else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.KeepData
                                                            || azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.CheckOption
                                                            || azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                                                        {
                                                            logger.Debug("File status is {0}, file id:{1}", azureRecord.ManualApprovedStatus, fileStub?.SelfId);
                                                            AddSkipReport(dto);
                                                        }
                                                        else if (azureRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected)
                                                        {
                                                            if (dbReord.IsManualSynced && dbReord.ManualExtendTime >= DateTime.UtcNow.Ticks)
                                                            {
                                                                logger.Debug("item is manualsync and its extend");
                                                                continue;
                                                            }
                                                            dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                                            dto.InternalApprovedStatus = (int)SOApproveDBStatus.Rejected;
                                                            logger.Debug("File status is Rejected, file id:{0}", fileStub?.SelfId);
                                                            //if (FileIsModified(fileStub, dbReord))
                                                            //{
                                                            //AddRejectFileToAzureTable(dto);
                                                            //}
                                                            UpdateRejectDto(dto);
                                                        }
                                                        else
                                                        {
                                                            logger.Warn("Invalid status. File id:" + fileStub?.SelfId);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        logger.Debug("File rule id chaned, file id:{0}", fileStub?.SelfId);
                                                        //only update ruleId in azure table
                                                        dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                                        dto.MovedToApprovalTable = false;
                                                        dto.ScanTime = DateTime.UtcNow;
                                                        OnlyUpdateAzureTable(dto);
                                                    }
                                                }
                                                else
                                                {
                                                    logger.Debug("Azure table not exist, file path:{0}", fileStub?.SelfId);
                                                    dto.Status = (int)SOApproveDBStatus.WaitingApprove;
                                                    dto.ScanTime = DateTime.UtcNow;
                                                    OnlyUpdateAzureTable(dto);
                                                }
                                            }
                                            else
                                            {
                                                logger.Info("Not manual rule, file id:{0}", fileStub?.SelfId);
                                                dto.ScanTime = DateTime.UtcNow;
                                                FSJobCache.Instance.DisposalScanCache.Add(dto);
                                            }
                                        }
                                        else
                                        {
                                            if (azureCache.ContainsKey(id))
                                            {
                                                logger.Debug($"Current file not match rule and has manual reocrd.FSPath:{fileStub.FullPath.LogBase64()}.");
                                                var manualData = CreateAzureEntityDto(fileStub, null, dbReord);
                                                RemoveManualData(manualData);
                                            }
                                            else
                                            {
                                                logger.Debug($"Current file not match rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Debug($"Current Term[{dbReord.TermId}] doesn't have FS rule so skip check rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                                    }
                                }
                                else
                                {
                                    logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", dbReord?.TermId);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //FIXED
                            logger.Error($"Error occurred while disposal file:{ExternalUtil.CombinePath(dbReord.DirPath.LogBase64(), dbReord.LeafName.LogBase64())} Error:{e.ToString()}");
                            JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
                            {
                                ObjectName = dbReord.LeafName,
                                SourceLocation = ExternalUtil.CombinePath(dbReord.DirPath, dbReord.LeafName),
                                Size = ExternalUtil.ConvertToFormatSize(new XFileInfoEx(fileStub.MediaObj).FileSize),
                                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                                Action = FSJobCache.Instance.Rules.ContainsKey(dbReord.RuleId) ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[dbReord.RuleId])) : "",
                                RuleName = FSJobCache.Instance.Rules.ContainsKey(dbReord.RuleId) ? FSJobCache.Instance.Rules[dbReord.RuleId].Name : "",
                                //DetailTab = DetailTab.Deletion.ToString(),
                                Status = JobDetailsStatus.Failed,
                                Comment = e.Message,
                                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                                AgentName = OSInformation.HostName
                            };
                            JobDetailService.Commit(detail);
                            FSJobCache.Instance.FailedCount++;
                        }
                    }
                    else
                    {
                        logger.Debug($"No db record found:{id}");
                    }
                }
            }
        }

        private bool IsRemoveRule(Rule rule)
        {
            if (rule != null && rule.FSRule != null && (rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveSetting != null && rule.FSRule.spMoveOption.MoveDestination != null))
            {
                //move to
                return false;
            }
            return true;
        }

        private void AddSkipReport(FSFileStub file, Rule rule)
        {
            XFileInfoEx xObj = new XFileInfoEx(file.MediaObj);
            JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
            {
                ObjectName = xObj.LowName,
                Size = ExternalUtil.ConvertToFormatSize(xObj.FileSize),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, xObj.HighName, xObj.LowName),
                //DetailTab = DetailTab.Deletion.ToString(),
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileOnHold",
                Action = FSJobCache.Instance.Rules.ContainsKey(new Guid(rule.Id)) ? GetActionString((int)GetRuleAction(FSJobCache.Instance.Rules[new Guid(rule.Id)])) : "",
                RuleName = FSJobCache.Instance.Rules.ContainsKey(new Guid(rule.Id)) ? FSJobCache.Instance.Rules[new Guid(rule.Id)].Name : "",
            };
            JobDetailService.Commit(detail);
        }

        private void AddSkipReport(FSAzureTableEntityDto entity)
        {
            JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
            {
                ObjectName = entity.LowName,
                Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                Action = GetActionString(entity.RuleAction),
                SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                RuleName = FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name,
                //DetailTab = DetailTab.Deletion.ToString(),
                Type = "RM_JS_Rule_ObjectLevel_FSFile",
                AgentName = OSInformation.HostName,
                Status = JobDetailsStatus.Skipped,
                Comment = "RM_JM_FSFileWaitingForApproval"
            };
            JobDetailService.Commit(detail);
            entity.NoNeedSendReport = true;
        }

        private bool FileIsModified(FSFileStub file, FileSystemRecordDto dbDto)
        {
            XFileInfoEx xObj = new XFileInfoEx(file.MediaObj);
            var lastAccess = GetAccessTime(dbDto.MetaInfo);
            if (xObj.LastWriteTimeUtc > new DateTime(dbDto.TimeLastModified) || xObj.LastAccessTimeUtc > new DateTime(lastAccess))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private long GetAccessTime(string metainfo)
        {
            var meta = JsonConvert.DeserializeObject<RecordMetaInfo>(metainfo);
            return meta.LastAccessTime;
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
            //else if (!(currentRule.KeepDataOption.Equals((int)KeepDataOption.Delete) ||
            //    currentRule.KeepDataOption.Equals((int)KeepDataOption.LinkDocument) ||
            //    currentRule.KeepDataOption.Equals((int)KeepDataOption.Remove)))
            //{
            //    action = RuleAction.ArchiveAndKeep;
            //}
            //else if (currentRule.KeepDataOption.Equals((int)KeepDataOption.Delete)
            //    || currentRule.KeepDataOption.Equals((int)KeepDataOption.LinkDocument))

            return action;
        }

        //private void AddToAzureTableAndArchive(FSAzureTableEntityDto dto)
        //{
        //    addToAzureTableCache.Add(dto);
        //    if (addToAzureTableCache.Count > 30)
        //    {
        //        var tempEntities = addToAzureTableCache.Take(30).ToList();
        //        List<Guid> failedGuids = JobContext.Current.ApiClient.AddScanData(tempEntities);
        //        var successFiles = tempEntities.Where(r => !failedGuids.Contains(r.FilePathMd5)).ToList();
        //        //process files that added to azure table successfully
        //        FSJobCache.Instance.DisposalScanCache.AddBatch(successFiles);
        //        var failedFiles = tempEntities.Except(successFiles);
        //        AddToReports(failedFiles.ToList(), "Failed to add this file to azure table.");
        //    }
        //}

        //private void FinalAddToAzureTableAndArchive()
        //{
        //    var tempEntities = addToAzureTableCache.TakeAll().ToList();
        //    List<Guid> failedGuids = JobContext.Current.ApiClient.AddScanData(tempEntities);
        //    var successFiles = tempEntities.Where(r => !failedGuids.Contains(r.FilePathMd5)).ToList();
        //    //process files that added to azure table successfully
        //    if (successFiles.Count > 0)
        //    {
        //        FSJobCache.Instance.DisposalScanCache.AddBatch(successFiles);
        //    }
        //    var failedFiles = tempEntities.Except(successFiles);
        //    AddToReports(failedFiles.ToList(), "Failed to add this file to azure table.");
        //}

        private void OnlyUpdateAzureTable(FSAzureTableEntityDto dto)
        {
            FSJobCache.Instance.DisposalAzureData.Add(dto);
        }
        private void SendRecordsToCosmos()
        {
            try
            {
                while (true)
                {
                    if (JobContext.Current.DisposalScanFinish && JobContext.Current.DisposalArchiveFinish && JobContext.Current.GetCosmosDBDataFinish
                        && FSJobCache.Instance.DisposalAzureData.Count == 0)
                    {
                        break;
                    }

                    if (FSJobCache.Instance.DisposalAzureData.Count == 0)
                    {
                        if (JobContext.Current.DisposalScanFinish == true)
                        {
                            break;
                        }
                        else
                        {
                            Thread.Sleep(3000);
                            continue;
                        }
                    }
                    try
                    {
                        var records = FSJobCache.Instance.DisposalAzureData.Take(100).ToList();
                        if (records != null && records.Count > 0)
                        {
                            mCachedRecords.AddBatch(records);
                            if (mCachedRecords.Count > ExternalUtil.TransferDataCount)
                            {
                                var mSendRecords = mCachedRecords.Take(ExternalUtil.TransferDataCount).ToList();
                                logger.Info("Send file to cosmos count:{0}", mSendRecords.Count);
                                List<Guid> failedIds;
                                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{mSendRecords.Count}", true))
                                {
                                    FSAzureTableEntityDtoWithJobId dtoInfo = new FSAzureTableEntityDtoWithJobId()
                                    {
                                        JobId = JobContext.Current.JobId,
                                        EntityDtos = mSendRecords
                                    };
                                    failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);
                                    HybridApiClient.Instance.AddScanData(mSendRecords?.Where(a => a.Status == (int)SOApproveDBStatus.Archived).ToList());
                                }
                                AddToReports(mSendRecords, failedIds);
                                if (failedIds.Count > 0)
                                {
                                    logger.Warn("Failed to add fs archived data to cosmos. File ids:" + string.Join(",", failedIds));
                                }
                                //AddReport(result?.FailedGuids, result?.SkippedGuids, mSendRecords);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while send sp data to explorer. Error:{0}", e.ToString());
                    }
                }
                FinialSendDataToCosmos();
            }
            catch (Exception e)
            {
                logger.Error("Error in send data. Error:{0}", e.ToString());
            }
            finally
            {
                JobContext.Current.SendDataToCosmosFinish = true;
                JobContext.Current.SendDataToAzureTableFinish = true;
            }
        }

        private void SendRecordsToAzureTable()
        {
            try
            {
                while (true)
                {
                    if (JobContext.Current.DisposalScanFinish && JobContext.Current.DisposalArchiveFinish && JobContext.Current.GetCosmosDBDataFinish
                        && FSJobCache.Instance.DisposalAzureData.Count == 0)
                    {
                        break;
                    }

                    if (FSJobCache.Instance.DisposalAzureData.Count == 0)
                    {
                        Thread.Sleep(3000);
                        continue;
                    }
                    try
                    {
                        var records = FSJobCache.Instance.DisposalAzureData.Take(100).ToList();
                        if (records != null && records.Count > 0)
                        {
                            mCachedRecords.AddBatch(records);
                            if (mCachedRecords.Count > ExternalUtil.TransferDataCount)
                            {
                                var mSendRecords = mCachedRecords.Take(ExternalUtil.TransferDataCount).ToList();
                                logger.Info("Send file to azure table count:{0}", mSendRecords.Count);
                                List<Guid> failedIds;
                                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{mSendRecords.Count}", true))
                                {
                                    failedIds = HybridApiClient.Instance.AddScanData(mSendRecords);
                                }
                                AddToReports(mSendRecords, failedIds);
                                if (failedIds.Count > 0)
                                {
                                    logger.Warn("Failed to add fs archived data to azure table. File ids:" + string.Join(",", failedIds));
                                }
                                //AddReport(result?.FailedGuids, result?.SkippedGuids, mSendRecords);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while send sp data to explorer. Error:{0}", e.ToString());
                    }
                }
                FinialSendData();
            }
            catch (Exception e)
            {
                logger.Error("Error in send data. Error:{0}", e.ToString());
            }
            finally
            {
                JobContext.Current.SendDataToAzureTableFinish = true;
            }
        }
        private void FinialSendDataToCosmos()
        {
            if (mCachedRecords.Count > 0)
            {
                var records = mCachedRecords.TakeAll().ToList();
                logger.Info("Send file to cosmos count:{0}", records.Count);
                List<Guid> failedIds;
                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{records.Count}", true))
                {
                    FSAzureTableEntityDtoWithJobId dtoInfo = new FSAzureTableEntityDtoWithJobId()
                    {
                        JobId = JobContext.Current.JobId,
                        EntityDtos = records
                    };
                    failedIds = HybridApiClient.Instance.AddScanDataToCosmos(dtoInfo);
                    HybridApiClient.Instance.AddScanData(records?.Where(a => a.Status == (int)SOApproveDBStatus.Archived).ToList());
                }
                AddToReports(records, failedIds);
                if (failedIds.Count > 0)
                {
                    logger.Warn("Failed to add fs archived data to cosmos. File ids:" + string.Join(",", failedIds));
                }
                //AddReport(result?.FailedGuids, result?.SkippedGuids, records);
            }
        }
        private void FinialSendData()
        {
            if (mCachedRecords.Count > 0)
            {
                var records = mCachedRecords.TakeAll().ToList();
                logger.Info("Send file to azure table count:{0}", records.Count);
                List<Guid> failedIds;
                using (var performance = new AgentPerformanceScope("DisposalDiscover.AddScanData", $"DisposalDiscover.AddScanData.Count:{records.Count}", true))
                {
                    failedIds = HybridApiClient.Instance.AddScanData(records);
                }
                AddToReports(records, failedIds);
                if (failedIds.Count > 0)
                {
                    logger.Warn("Failed to add fs archived data to azure table. File ids:" + string.Join(",", failedIds));
                }
                //AddReport(result?.FailedGuids, result?.SkippedGuids, records);
            }
        }

        private void GetRecordsFromCosmos()
        {
            try
            {
                while (true)
                {
                    if (JobContext.Current.DisposalScanFinish && FSJobCache.Instance.DisposalCosmosDBData.Count == 0 && JobContext.Current.SendDataToCosmosFinish)
                    {
                        break;
                    }

                    if (FSJobCache.Instance.DisposalCosmosDBData.Count == 0)
                    {
                        logger.Info($"FSJobCache.Instance.DisposalCosmosDBData.Count is:{FSJobCache.Instance.DisposalCosmosDBData.Count},JobContext.Current.SendDataToCosmosFinish:{JobContext.Current.SendDataToCosmosFinish}");
                        Thread.Sleep(3000);
                        continue;
                    }
                    try
                    {
                        var records = FSJobCache.Instance.DisposalCosmosDBData.Take(100).ToList();
                        if (records != null && records.Count > 0)
                        {
                            mMatchRuleData.AddBatch(records);
                            if (mMatchRuleData.Count > ExternalUtil.TransferDataCount)
                            {
                                var tempRecords = mMatchRuleData.Take(ExternalUtil.TransferDataCount).ToList();
                                var fileIds = tempRecords.Select(r => r.FilePathMd5).ToList();
                                //start new thread to get data from cosmos db may improve performance, too many theads in job hard to control
                                var azureRecords = GetFSManualRecords(fileIds).ToDictionary(r => r.NodeId);
                                foreach (var dto in tempRecords)
                                {
                                    RealProcess(dto, azureRecords);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while send sp data to explorer. Error:{0}", e.ToString());
                    }
                }
                FinalGetRecordsFromCosmos();
            }
            catch (Exception e)
            {
                logger.Error("Error in send data. Error:{0}", e.ToString());
            }
            finally
            {
                JobContext.Current.GetCosmosDBDataFinish = true;
            }
        }
        private void FinalGetRecordsFromCosmos()
        {
            var tempRecords = mMatchRuleData.TakeAll().ToList();
            if (tempRecords != null && tempRecords.Any())
            {
                //logger.Info("Send file to azure table count:{0}", tempRecords.Count);
                var fileIds = tempRecords.Select(r => r.FilePathMd5).ToList();
                //start new thread to get data from cosmos db may improve performance, too many theads in job hard to control
                var azureRecords = GetFSManualRecords(fileIds).ToDictionary(r => r.NodeId);
                foreach (var dto in tempRecords)
                {
                    RealProcess(dto, azureRecords);
                }
            }
        }
        private void RemoveManualData(FSAzureTableEntityDto dto)
        {
            DeleteManualItemCache.Add(dto);
            if (DeleteManualItemCache.Count > ExternalUtil.TransferDataCount)
            {

                var tempEntities = DeleteManualItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                try
                {
                    List<Guid> failedGuids = new List<Guid>();
                    using (new AgentPerformanceScope("FSDicover.RemoveManualData", $"FSDicover.RemoveManualData.Count:{tempEntities.Count}", true))
                    {
                        failedGuids = JobContext.Current.ApiClient.RemoveManualData(tempEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while removing manual data. Error:{0}", e.ToString());
                    // AddToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
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
                        //JMFSDisposalJobDetails
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
        private void UpdateRejectDto(FSAzureTableEntityDto dto)
        {
            FSJobCache.Instance.DisposalAzureData.Add(dto);
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

        //private void FinalUpdateAzureTable()
        //{
        //    var tempEntities = WaitingApprovalItemCache.TakeAll().ToList();
        //    if (tempEntities.Count > 0)
        //    {

        //        try
        //        {
        //            List<Guid> failedGuids = new List<Guid>();
        //            using (new AgentPerformanceScope("FSDicover.AddScanData", "FSDicover.AddScanData.Count:" + tempEntities.Count, true))
        //            {
        //                failedGuids = JobContext.Current.ApiClient.AddScanData(tempEntities);
        //            }
        //            AddToReports(tempEntities, failedGuids);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Error("An error occurred while final updating azure table. Error:{0}", e.ToString());
        //            AddToReports(tempEntities, tempEntities.Select(r => r.FilePathMd5).ToList());
        //        }
        //    }
        //}

        private void FinalRemoveManualData()
        {
            var tempEntities = DeleteManualItemCache.TakeAll().ToList();
            if (tempEntities.Count > 0)
            {

                try
                {
                    List<Guid> failedGuids = new List<Guid>();
                    using (new AgentPerformanceScope("FSDicover.RemoveManualData", "FSDicover.RemoveManualData.Count:" + tempEntities.Count, true))
                    {
                        failedGuids = JobContext.Current.ApiClient.RemoveManualData(tempEntities);
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
                    using (new AgentPerformanceScope("FSDicover.AddRejectScanData", "FSDicover.AddRejectScanData.Count:" + tempEntities.Count, true))
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
            List<JMFSDisposalJobDetails> details = new List<JMFSDisposalJobDetails>();
            foreach (var entity in tempEntities)
            {
                JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
                {
                    ObjectName = entity.LowName,
                    Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                    Action = GetActionString(entity.RuleAction),
                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                    RuleName = FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name,
                    //DetailTab = DetailTab.Deletion.ToString(),
                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                    AgentName = OSInformation.HostName
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

        private void AddToReports(List<FSAzureTableEntityDto> tempEntities, List<Guid> failedGuids)
        {
            if (failedGuids.Count > 0)
            {
                logger.Debug("Failed to add scan data. Ids:{0}", string.Join(",", failedGuids));
            }
            List<JMFSDisposalJobDetails> details = new List<JMFSDisposalJobDetails>();
            tempEntities = tempEntities.Where(e => !e.NoNeedSendReport).ToList();
            foreach (var entity in tempEntities)
            {
                JMFSDisposalJobDetails detail = new JMFSDisposalJobDetails()
                {
                    ObjectName = entity.LowName,
                    Size = ExternalUtil.ConvertToFormatSize(entity.Size),
                    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                    Action = GetActionString(entity.RuleAction),
                    SourceLocation = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, entity.HighName, entity.LowName),
                    RuleName = FSJobCache.Instance.Rules[new Guid(entity.RuleId)].Name,
                    //DetailTab = DetailTab.Deletion.ToString(),
                    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                    AgentName = OSInformation.HostName
                };
                if (failedGuids.Contains(entity.FilePathMd5))
                {
                    detail.Status = JobDetailsStatus.Failed;
                    detail.Comment = "RM_JM_FSFailedAddToArchiverTable";
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
                InternalConnectionId = FSJobCache.Instance.AveConnectionId
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
                //if (FSDataDisposal.ClassificationLevel != NodeLevel.FSFolder)
                {
                    createdByElement.SetAttribute("Value", xObj.Owner);
                }
                xe.AppendChild(createdByElement);

                XmlElement modifiedByElement = doc.CreateElement("Column");
                modifiedByElement.SetAttribute("Name", "ModifiedBy");
                modifiedByElement.SetAttribute("Value", GetOfficeLastModifiedBy(xObj));
                xe.AppendChild(modifiedByElement);
                propertyValue = xe.OuterXml;
            }
            catch (Exception ex)
            {
                logger.Debug("GetMetaData failed, path: {0}, details: {1}", xObj.Name.LogBase64(), ex.ToString());
            }
            return propertyValue;
        }

        private string GetOfficeLastModifiedBy(XFileInfoEx xObj)
        {
            string lastModifiedBy = string.Empty;
            if (IsOffice07(xObj.Name))
            {
                //Package package = null;
                //try
                //{
                //    package = Package.Open(fileInfo.FullPath);
                //    lastModifiedBy = string.IsNullOrEmpty(package.PackageProperties.LastModifiedBy) ? string.Empty : package.PackageProperties.LastModifiedBy;
                //}
                //catch (Exception e)
                //{
                //    logger.Debug(e.ToString());
                //}
                //finally
                //{
                //    if (package != null)
                //    {
                //        package.Close();
                //        package = null;
                //    }
                //}
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

        private bool FilterdIn(XFileInfoEx t)
        {
            if (t.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }
            else
            {
                return true;
            }
            //switch (FSJobCache.Instance.JobController.JobType)
            //{
            //    case FSJobType.UserFullJob:
            //    case FSJobType.RematchRuleFullJob:
            //        return true;
            //    case FSJobType.IncrementalJob:
            //        return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
            //            || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime);
            //    default:
            //        logger.Warn("The code shouldnt go this approach.");
            //        return false;
            //}
        }

    }
}

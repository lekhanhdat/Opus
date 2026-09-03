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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Disposal.Archive;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace RAFileSystem.Disposal
{
    public class FSDataDisposal : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static NodeLevel ClassificationLevel;
        internal static AvePoint.RA.Contract.FileSystem.FSSettingDto currentSetting;
        internal static Stub _rootStub;
        public void Bind(string msgStr)
        {
            try
            {
                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                JobContext.Current.JobMessage = msgStr;
                //FSCollectJobMessage msg = (jobMsg as FSCollectJobMessage);
                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]);  //for now, the sub job can only process one connection.
                System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]));
              /*
                var scopeSet = msg.RMScopeSettings.FirstOrDefault(item => item.ScopeId.ToString().Equals(top3Nodes.Item3.ID));
                var isTermHasRule = msg.TermRuleMapping.Any(items => items.Key.Equals(scopeSet?.DefaultTermId));

                if (!isTermHasRule) 
                {
                    logger.Error("FSDataDisposal Error,No Term,There may be no rules");
                    throw new Exception("No Term,There may be no rules,Please Check");
                }
               */
                ClassificationLevel = (NodeLevel)msg.ClassificationLevel;
                string path = top3Nodes.Item3.FullPath;
                logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID);
                FSJobCache.Instance.RootPath = path.TrimEnd('\\');
                //FSJobCache.Instance.UserName = node.Username;
                //FSJobCache.Instance.SecPwd = node.EncryptedPassword;
                FSJobCache.Instance.AveConnectionId = new Guid(top3Nodes.Item3.ID);
                JobContext.Current.BulkImportEnabled = msg.BulkImportEnabled;
                JobContext.Current.BulkSize = msg.BulkSize;
                IXSystem _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
                //Add connection Groups to analyzer cache.
                FSJobCache.Instance.AnalyzerCache.Add(new FSConnectionGroupsStub { FullPath = top3Nodes.Item1.Name, SelfId = new Guid(top3Nodes.Item1.ID) });
                //Add Connection Group to analyzer cache.
                FSJobCache.Instance.AnalyzerCache.Add(new FSConnectionGroupStub { FullPath = top3Nodes.Item2.Name, SelfId = new Guid(top3Nodes.Item2.ID), ParentId = new Guid(top3Nodes.Item1.ID) });
                FSJobCache.Instance.RunJobScopePath = node.FullPath;
                string highName = node.FullPath.Substring(path.Length).Trim('\\');
                StorageInfo dirInfo = new StorageInfo() { HighName = highName };
                Guid settingScopeId = QueryScopeTermIdSetting(node);
                FSJobCache.Instance.DispoalSettingScopeId = settingScopeId;
                var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];  //跑Job的节点的Setting
                currentSetting = setting;
                if (_system.DirectoryExists(dirInfo))
                {
                    XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
                    // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine
                    Guid parentId = string.IsNullOrEmpty(dirInfo.HighName) ?
                        new Guid(top3Nodes.Item2.ID)  //level3  connection
                        : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, Path.GetDirectoryName(ExternalUtil.CombinePath(dir.HighName, dir.LowName))).ToLowerInvariant().ToMd5();  //sub folder
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    FSFolderStub rootStub = new FSFolderStub() { MediaObj = dir, FullPath = fullPath, SelfId = fullPath.ToLowerInvariant().ToMd5(), ParentId = parentId, ScopeSettingId = settingScopeId };
                    _rootStub = rootStub;
                    FSJobCache.Instance.DisposalFSFolderCache.Add(rootStub);
                    FSJobCache.Instance.AnalyzerCache.Add(rootStub);
                    //FSJobCache.Instance.DiscoveryCache.Add(rootStub);
                    FSJobCache.Instance.JobController.InitJob(setting, rootStub.FullPath.ToLowerInvariant().ToMd5(), rootStub.FullPath, msg,dir.Name);
                    JobContext.Current.mProgressManager.Create().IncreaseBase(3);
                }
                else
                {
                    JobContext.Current.JobDetailManager.Create().Commit(new JMFSDisposalJobDetails()
                    {
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine
                        ObjectName = Path.GetFileName(node.FullPath),
                        SourceLocation = node.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_JMD_FS_PathCanNotAccess"
                    });
                    throw new FileNotFoundException("We cannot open the Dir" + node.FullPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to initialize the file system from the tree node dto.  Exception:{0}", ex.ToString());
                throw;
            }
        }
        public void Run()
        {
            try
            {
                if(ClassificationLevel == NodeLevel.FSFile)
                {

                    GetAllRecords();
                    var allFolderCache = GetDisposalDiscoverFolders();
                    if (allFolderCache != null && allFolderCache.Count > 0)
                    {
                        FSJobCache.Instance.DisposalFolderCache.AddBatch(allFolderCache.AsEnumerable());
                        StartSubThreads();
                    }
                    else
                    {
                        logger.Warn("No available folder path, skip running job.");
                    }
                }
                else
                {
                    var allExceptFolderCache = this.GetAllFolders();
                    //缓存与default term不一样的数据，  或Hold的数据。
                    FSJobCache.Instance.DisposalDifferentFolderCache.AddRange(allExceptFolderCache.AsEnumerable());
                    StartSubThreads();
                }
            }
            catch (Exception e)
            {
                FSJobCache.Instance.FailedCount++;
                logger.Error($"Error occurred while running disposal job. Error:{e.ToString()}");
            }
            finally
            {
                //FSJobCache.Instance.JobController.StoreJobTime();
                //FSJobCache.Instance.JobController.UpdateScopeSettingProfile();
                try
                {
                    FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                    fileSystemSqliteWrapper.Dispose();
                    fileSystemSqliteWrapper.DeleteDBFile();
                }
                catch(Exception e)
                {
                    logger.Warn("An error occurred while deleting db file. Error:" + e.ToString());
                }
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                    FSJobCache.Instance.FailedCount++;
                }
                if (FSJobCache.Instance.FailedCount > 0)
                {
                    if (FSJobCache.Instance.SuccessCount > 0)
                    {
                        JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
                    }
                    else
                    {
                        JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Failed, JobContext.Current.JobId);
                    }
                }
                else
                {
                    JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
                }
                logger.Info("Enforce retention job finished.");
            }
        }

        private void StartSubThreads()
        {
            StartDiscoveryThread();
            Thread.Sleep(2000);
            StartWorkerThread();
            Thread.Sleep(2000);
            StartReportThread();
            Thread.Sleep(2000);
            WaitForDiscoveryThreadExit();
            WaitForAnalyzerThreadExit();
            WaitForPersistThreadExit();
            RunSendEmailJob();

        }
        private void RunSendEmailJob()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0
                        && FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count == 0 && FSJobCache.Instance.DisposalScanCache.Count == 0
                        && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                        && FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count == 0 && FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                {
                    logger.Info("There is no send email serializer thread running now...");
                    JobContext.Current.ApiClient.RunSendEmailJob(JobContext.Current.JobId);
                    break;
                }
            }
        }
        private bool IsBreakInheritNode(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCache.Instance.BreakNodeUrls != null && FSJobCache.Instance.BreakNodeUrls.Contains(sha1Url))
            {
                if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    return false;
                }
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

        private List<Guid> GetAllDefaultTermId()
        {
            List<Guid> result = new List<Guid>() { currentSetting.DefaultTermId };
            var subSettings = FSJobCache.Instance.ScopeSettingCache.Values.Where(a=>a.FullPath.StartsWith(_rootStub.FullPath, StringComparison.InvariantCultureIgnoreCase));
            result.AddRange(subSettings.Select(a => a.DefaultTermId));
            var temp = result.Where(a => a != Guid.Empty).Distinct().ToList();
            logger.Info("Break inherit default term count {0}", temp.Count);
            return temp;
        }

        private List<FileSystemRecordDto> GetAllFolders()
        {
            using (new AgentPerformanceScope("FSDisposal.GetAllDifferentTermFolders", addToStatistics: true))
            {

                AvePoint.RA.Contract.Explorer.SearchFilterParam searchFilterParam = new AvePoint.RA.Contract.Explorer.SearchFilterParam()
                {
                    TermId = currentSetting.DefaultTermId,
                    DataSource = (int)AvePoint.RA.Contract.Explorer.SourceFlag.FileSystem,
                    ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(),
                    PageInfo = new AvePoint.RA.Contract.Explorer.SearchPageInfo()
                    {
                        PageIndex = "",
                        PageSize = 100
                    }
                };

                searchFilterParam.Filter = new AvePoint.RA.Contract.Explorer.SearchFilterInfo()
                {
                    NodeTypes = new System.Collections.Generic.List<int> { (int)NodeLevel.FSFolder }
                };
                if (!FSJobCache.Instance.RunJobScopePath.Equals(FSJobCache.Instance.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobScopePath;
                    searchFilterParam.FolderId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5();
                }
                List<FileSystemRecordDto> ret = new List<FileSystemRecordDto>();
                int index = 0;
                int totalCount = 0;
                do
                {
                    using (new AgentPerformanceScope("FSDisposal.QuerybyPage", addToStatistics: true))
                    {
                        var result = JobContext.Current.ApiClient.GetFSDueRecords(searchFilterParam);
                        if (result != null)
                        {
                            searchFilterParam.PageInfo.HasNextPage = !string.IsNullOrEmpty(result?.PageInfo?.PageIndex);
                            searchFilterParam.PageInfo.PageIndex = result?.PageInfo?.PageIndex;
                            int resultCount = result.Records != null ? result.Records.Count : 0;
                            totalCount += resultCount;
                            index++;
                            logger.Info($"query for {index} times, result count:{resultCount}, has next page:{searchFilterParam.PageInfo.HasNextPage}");
                            //SavePagingResult(result.Records);
                            ret.AddRange(result.Records);
                        }
                        else
                        {
                            logger.Warn($"Query result is null");
                            break;
                        }
                    }
                }
                while (searchFilterParam.PageInfo.HasNextPage);
                logger.Info("finish searching, total result count {0}", totalCount);
                return ret;
            }
        }

        private void GetAllRecords()
        {
            using (new AgentPerformanceScope("FSDisposal.GetAllRecords", addToStatistics: true))
            {
                AvePoint.RA.Contract.Explorer.SearchFilterParam searchFilterParam = null;
                using (new AgentPerformanceScope("FSDisposal.Init", addToStatistics: true))
                {
                    searchFilterParam = AssembleQueryDto();
                }
                int index = 0;
                int totalCount = 0;
                do
                {
                    using (new AgentPerformanceScope("FSDisposal.QuerybyPage", addToStatistics: true))
                    {
                        var result = JobContext.Current.ApiClient.GetFSDueRecords(searchFilterParam);
                        if (result != null)
                        {
                            searchFilterParam.PageInfo.HasNextPage = !string.IsNullOrEmpty(result?.PageInfo?.PageIndex);
                            searchFilterParam.PageInfo.PageIndex = result?.PageInfo?.PageIndex;
                            int resultCount = result.Records != null ? result.Records.Count : 0;
                            totalCount += resultCount;
                            index++;
                            logger.Info($"query for {index} times, result count:{resultCount}, has next page:{searchFilterParam.PageInfo.HasNextPage}");
                            SavePagingResult(result.Records);
                        }
                        else
                        {
                            logger.Warn($"Query result is null");
                            break;
                        }
                    }
                }
                while (searchFilterParam.PageInfo.HasNextPage);
                logger.Info("finish searching, total result count {0}", totalCount);
            }
        }

        private List<FSDisposalDiscoverFolder> GetDisposalDiscoverFolders()
        {
            FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
            if (fileSystemSqliteWrapper != null)
            {
                using (new AgentPerformanceScope("FSDisposal.GetDisposalDiscoverFolders", addToStatistics: true))
                {
                    var folders = fileSystemSqliteWrapper.GetDisposalDiscoverFolders();
                    var _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RunJobScopePath);
                    List<FSDisposalDiscoverFolder> validFolders = new List<FSDisposalDiscoverFolder>();
                    foreach (var folder in folders)
                    {
                        if (!folder.FolderPath.StartsWith(FSJobCache.Instance.RunJobScopePath, StringComparison.OrdinalIgnoreCase))
                        {
                            //FIXED
                            logger.Info($"Folder is not under run job scope. id:{folder?.FolderId} Run job scope:{FSJobCache.Instance.RunJobScopePath.LogBase64()}");
                            continue;
                        }
                        if (IsBreakInheritNode(folder.FolderPath.ToLowerInvariant()))
                        {
                            logger.Debug("The folder node {0} has unique setting.", folder?.FolderId);
                            continue;
                        }
                        if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(folder.FolderId))
                        {
                            if (!FSJobCache.Instance.ScopeSettingCache[folder.FolderId].IsActive)
                            {
                                logger.Debug("The folder node {0}  has been deactived.", folder?.FolderId);
                                continue;
                            }
                        }
                        if (HasRunningJob(folder.FolderPath.ToLowerInvariant()))
                        {
                            logger.Debug("There is already a job running on this node. id:{0}", folder?.FolderId);
                            continue;
                        }

                        //check sub folder still exist
                        if (!folder.FolderPath.Equals(FSJobCache.Instance.RunJobScopePath, StringComparison.OrdinalIgnoreCase))
                        {
                            StorageInfo info = new StorageInfo()
                            {
                                HighName = folder.FolderPath.Substring(FSJobCache.Instance.RunJobScopePath.Length, folder.FolderPath.Length - FSJobCache.Instance.RunJobScopePath.Length)
                            };
                            if (!_system.DirectoryExists(info))
                            {
                                logger.Info($"Folder no longer exist. id:{folder?.FolderId}");
                                continue;
                            }
                        }
                        validFolders.Add(folder);
                    }
                    return validFolders;
                }
            }
            return null;
        }



        private void SavePagingResult(List<FileSystemRecordDto> result)
        {
            if (result == null)
            {
                return;
            }
            using (new AgentPerformanceScope("FSDisposal.SavePagingResult", addToStatistics: true))
            {
                FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                fileSystemSqliteWrapper.Insert(result);
            }
        }

        private AvePoint.RA.Contract.Explorer.SearchFilterParam AssembleQueryDto()
        {
            AvePoint.RA.Contract.Explorer.SearchFilterParam searchFilterParam = new AvePoint.RA.Contract.Explorer.SearchFilterParam()
            {
                DataSource = (int)AvePoint.RA.Contract.Explorer.SourceFlag.FileSystem,
                ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(),
                DueDate = JobContext.Current.JobStartTime.Ticks,
                PageInfo = new AvePoint.RA.Contract.Explorer.SearchPageInfo()
                {
                    PageIndex = "",
                    PageSize = 100
                }
            };

            searchFilterParam.Filter = new AvePoint.RA.Contract.Explorer.SearchFilterInfo()
            {
                NodeTypes = new System.Collections.Generic.List<int> { (int)NodeLevel.FSFile }
            };
            if (!FSJobCache.Instance.RunJobScopePath.Equals(FSJobCache.Instance.RootPath, StringComparison.OrdinalIgnoreCase))
            {
                searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobScopePath;
            }

            return searchFilterParam;
        }

        private void WaitForDiscoveryThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                logger.Debug("{0}, {1}, {2}", FSJobCache.Instance.DisposalScanCache.Count, FSJobCache.Instance.DiscoverThreadMonitor.Count, FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count);
                if (FSJobCache.Instance.DisposalScanCache.Count == 0
                    && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0
                    && FSJobCache.Instance.WaitingApprovalReportThreadMonitor.Count == 0)
                {
                    logger.Info("There is no discovery thread running now..");
                    break;
                }
            }
        }

        private void WaitForAnalyzerThreadExit()
        {
            while (true)
            {
                logger.Debug("{0},", FSJobCache.Instance.AnalyzerThreadMonitor.Count);
                if (FSJobCache.Instance.AnalyzerThreadMonitor.Count == 0)
                {
                    break;
                }
                Thread.Sleep(3000);
            }
        }

        private void WaitForPersistThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                logger.Debug("{0}, {1}", FSJobCache.Instance.SerializerThreadMonitor.Count, FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count);
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0
                    && FSJobCache.Instance.DisposalDataUpdaterThreadMonitor.Count == 0)
                {
                    logger.Info("There is no serializer thread running now...");
                    break;
                }
            }
        }

        private void StartReportThread()
        {
            int serializerThreadCount = 1;
            //FSJobCache.Instance.Config.PersistThreadCount;
            for (int i = 0; i < serializerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalDataUpdater serializer = new DisposalDataUpdater();
                    serializer.Run();
                });
            }
        }

        private void StartWorkerThread()
        {
            int analyzerThreadCount = 1;
            //try
            //{
            //    analyzerThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.DisposalAnalyzerThreadCount));
            //    logger.Info("analyzerThreadCount is " + analyzerThreadCount);
            //}
            //catch (Exception e)
            //{
            //    logger.Error("An error occurred while gettting analyzerThreadCount.Error:{0}", e.ToString());
            //}
            //FSJobCache.Instance.Config.AnalyzerThreadCount;
            for (int i = 0; i < analyzerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalWorker analyzer = new DisposalWorker();
                    analyzer.Run();
                });
            }
        }

        private void StartDiscoveryThread()
        {
            int discoveryThreadCount = 1;
            //try
            //{
            //    discoveryThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.DisposalDiscoveryThreadCount));
            //    logger.Info("discoveryThreadCount is " + discoveryThreadCount);
            //}
            //catch (Exception e)
            //{
            //    logger.Error("An error occurred while gettting discoveryThreadCount.Error:{0}", e.ToString());
            //}
            for (int i = 0; i < discoveryThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DisposalDiscover discovery = new DisposalDiscover();
                    discovery.Run();
                });
            }
        }



        private static Guid QueryScopeTermIdSetting(AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node)
        {
            Guid id = node.Level == NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : new Guid(node.ID);
            //Guid id = node.FullPath.ToLowerInvariant().ToMd5();
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
            {
                return id;
            }
            else if (node.Parent != null)
            {
                return QueryScopeTermIdSetting(node.Parent);
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}

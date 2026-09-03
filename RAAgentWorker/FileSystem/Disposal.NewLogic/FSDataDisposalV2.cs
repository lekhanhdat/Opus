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
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Collect.NewLogic;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.Wrapper.Common;
using RAFileSystem.Disposal.Archive;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataIngestion;
using RAFileSystem.Utils;
using AvePoint.RA.Contract.Services;
using Task = System.Threading.Tasks.Task;
using AvePoint.RA.Common.Utils.ProtoBuf;

namespace RAFileSystem.Disposal.NewLogic
{
    public class FSDataDisposalV2 : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static NodeLevel ClassificationLevel;
        internal static FSSettingDto currentSetting;
        internal static RMDataIngestionOperationType OperationType = RMDataIngestionOperationType.FileSystemEnforceRunAction;


        private static Channel<T> CreateBounded<T>(int capacity, bool singleWriter = false, bool singleReader = false) =>
            Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = singleWriter,
                SingleReader = singleReader
            });

        public void Bind(string msgStr)
        {
            try
            {
                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                JobContext.Current.JobMessage = msgStr;
                JobContext.Current.EnableFSHighPerformanceMode = true;
                //FSCollectJobMessage msg = (jobMsg as FSCollectJobMessage);
                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]);  //for now, the sub job can only process one connection.
                System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]));
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
                    FSJobCache.Instance.JobController.InitJob(setting, fullPath.ToLowerInvariant().ToMd5(), fullPath, msg, dir.Name);
                    JobContext.Current.mProgressManager.Create().IncreaseBase(3);

                    ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
                    var batchCapacity = Math.Max(100, ExternalUtil.TransferDataCount);
                    FSJobCache.Instance.DiscoveryToWorker = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
                    FSJobCache.Instance.WorkerToUpdater = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
                    FSJobCache.Instance.DiscoveryToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
                    FSJobCache.Instance.ManualInFolderToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
                    //FSJobCache.Instance.WorkerMoveToUpdater = CreateBounded<FileSystemRecordDto>(batchCapacity);
                    var ingestionPersistor = new RMDataIngestionPersistor(JobContext.Current.JobId);
                    FSJobCache.Instance.DataIngestionResultCollector = new RMDataIngestionExecutionResultCollector(JobContext.Current.JobId, OperationType, ingestionPersistor);
                    FSJobCache.Instance.DataIngestMessageExtensionManager = new RMDataIngestMessageExtensionManager();
                    FSJobCache.Instance.DataIngestionDataCollector = new RMDataIngestionDataCollector(FSJobCache.Instance.DataIngestionResultCollector, FSJobCache.Instance.DataIngestMessageExtensionManager, ingestionPersistor);
                }
                else
                {
                    JobContext.Current.JobDetailManager.Create().Commit(new JMFSDisposalJobDetailV2()
                    {
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine
                        ObjectName = Path.GetFileName(node.FullPath),
                        SourceLocation = node.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_JMD_FS_PathCanNotAccess",
                        DirPath = node.FullPath,
                        Depth = 0,
                        DetailAction = 0,
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
                if (ClassificationLevel == NodeLevel.FSFile)
                {

                    GetAllRecords();
                    var allFolderCache = GetDisposalDiscoverFolders();
                    if (allFolderCache != null && allFolderCache.Count > 0)
                    {
                        FSJobCache.Instance.DisposalFolderCache.AddBatch(allFolderCache.AsEnumerable());
                        StartSubThreads().GetAwaiter().GetResult();
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
                    StartSubThreads().GetAwaiter().GetResult();
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
                catch (Exception e)
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

        private async Task StartSubThreads()
        {
            try
            {
                var discovery = new DisposalDiscoverV2();
                var worker = new DisposalWorkerV2();
                var updater = new DisposalDataUpdaterV2();

                var runningTasks = new List<Task>
                {
                    ExecuteTask("DiscoveryRun", () => discovery.Run()),
                    ExecuteTask("WorkerRun", () => worker.Run()),
                    ExecuteTask("DataUpdaterRun", () => updater.Run()),
                    ExecuteTask("CosmosSend", () => discovery.RunSendRecordsToCosmos()),
                    ExecuteTask("CosmosReceive", () => discovery.RunGetRecordsFromCosmos()),
                };

                var reportTask = ExecuteTask("ReportCollector", () => Task.Run(() => ProcessExecutionResults()));

                await Task.WhenAll(runningTasks).ConfigureAwait(false);

                FSJobCache.Instance.DataIngestionDataCollector.Complete();

                await reportTask.ConfigureAwait(false); // wait for report after copmplete data collector

                RunSendEmailJob();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while running sub threads. Error:" + e.ToString());
                throw;
            }
        }

        private void ProcessExecutionResults()
        {
            //var jobDetailService = JobContext.Current.JobDetailManager.Create();
            foreach (var item in FSJobCache.Instance.DataIngestionResultCollector.ReadItemExecutionResults())
            {
                //var detail = new JMFSDisposalJobDetails
                //{
                //    ObjectName = item.LeafName,
                //    Size = ExternalUtil.ConvertToFormatSize(item.Size),
                //    FinishTime = TimeSettingUtil.GetFinishTime(DateTime.UtcNow),
                //    Action = GetActionString(item.RuleAction),
                //    SourceLocation = ExternalUtil.CombinePath(item.DirPath, item.LeafName),
                //    RuleName = item.RuleName,
                //    Type = "RM_JS_Rule_ObjectLevel_FSFile",
                //    AgentName = OSInformation.HostName,
                //    Status = (JobDetailsStatus)item.Status,
                //    Comment = item.Message
                //};
                //jobDetailService.Commit(detail);
                logger.Debug($"Receive ingestion result for item id: {item.Id}, " +
                    $"name: {ExternalUtil.CombinePath(item.DirPath, item.LeafName).LogBase64()}, " +
                    $"ruleName: {item.RuleName}, " +
                    $"ruleAction: {GetActionString(item.RuleAction)}, " +
                    $"isSucceed: {item.Succeed}, " +
                    $"{(!item.Succeed ? $"ErrorMessage: {item.Message}" : "")}");
            }
        }

        private Task ExecuteTask(string taskName, Func<Task> action)
        {
            return Task.Run(async () =>
            {
                try
                {
                    logger.Info($"[Task Start] {taskName}");
                    await action();
                    logger.Info($"[Task Success] {taskName}");
                }
                catch (Exception ex)
                {
                    logger.Error($"[Task Failed] {taskName}. Error: {ex}");
                    throw;
                }
            });
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

        private void RunSendEmailJob()
        {
            logger.Info("There is no send email serializer thread running now...");
            JobContext.Current.ApiClient.RunSendEmailJob(JobContext.Current.JobId);
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
                            logger.Info($"Folder is not under run job scope. id:{folder?.FolderId} Run job scope:{FSJobCache.Instance.RunJobScopePath}");
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

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
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.GCommon;
using System.Runtime.Remoting.Lifetime;

namespace AvePoint.RA.FileSystem.Collect
{
    public class FSDataCollector : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IReportService<JMJobDetails> JobDetailService { get; set; }

        internal static NodeLevel ClassificationLevel = 0;
        public void Bind(string msgStr)
        {
            try
            {
                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                JobContext.Current.JobMessage = msgStr;
                ClassificationLevel = (NodeLevel)msg.ClassificationLevel;
                logger.Info("Init classification level:{0}", ClassificationLevel);
                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]);  //for now, the sub job can only process one connection.
                System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]));
                string path = top3Nodes.Item3.FullPath;
                logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID.ToString());
                FSJobCache.Instance.RootPath = path.TrimEnd('\\');
                FSJobCache.Instance.RecordOwner = msg.RecordOwner;
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
                var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
                if (_system.DirectoryExists(dirInfo))
                {
                    // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine
                    XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
                    Guid parentId = string.IsNullOrEmpty(dirInfo.HighName) ?
                        new Guid(top3Nodes.Item2.ID)  //level3  connection
                        : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, Path.GetDirectoryName(ExternalUtil.CombinePath(dir.HighName, dir.LowName))).ToLowerInvariant().ToMd5();  //sub folder
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);

                    string termName = FSJobCache.Instance.Terms.ContainsKey(setting.DefaultTermId) ? FSJobCache.Instance.Terms[setting.DefaultTermId].Name : null;
                    Stub rootStub = new FSFolderStub() { MediaObj = dir, FullPath = fullPath, SelfId = fullPath.ToLowerInvariant().ToMd5(), ParentId = parentId, ScopeSettingId = settingScopeId, TermId4Folder = GetTermId4Folder(msg, setting), TermName4Folder = termName };
                    //FSJobCache.Instance.AnalyzerCache.Add(rootStub);
                    FSJobCache.Instance.DiscoveryCache.Add(rootStub);
                    FSJobCache.Instance.JobController.InitJob(setting, rootStub.FullPath.ToLowerInvariant().ToMd5(), rootStub.FullPath, msg, dir.Name);
                    JobContext.Current.mProgressManager.Create().IncreaseBase(3);
                }
                else
                {
                    JobContext.Current.JobDetailManager.Create().Commit(new FSDataSyncJobReportDetail()
                    {
                        AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                        // NET6Upgrade Alphaleonis.Win32.Filesystem.Path.Combine => Path.Combine
                        ObjectName = Path.GetFileName(node.FullPath),
                        FullPath = node.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_JMD_FS_PathCanNotAccess"
                    });
                    JobContext.Current.HasErrorNode = true;
                    FSJobCache.Instance.FailedCount++;
                    throw new FileNotFoundException("We cannot open the Dir" + node.FullPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to initialize the file system from the tree node dto.  Exception:{0}", ex.ToString());
                throw;
            }
        }

        private Guid GetTermId4Folder(FSJobMessage msg, Contract.FileSystem.FSSettingDto setting)
        {
            Guid folderTermId = Guid.Empty;
            if (setting.NeedCheckDefaultValue || string.IsNullOrWhiteSpace(msg.FolderTermId))
            {
                folderTermId = setting.DefaultTermId;
            }
            else
            {
                if (Guid.TryParse(msg.FolderTermId, out Guid termid))
                {
                    folderTermId = termid;
                }
                else
                {
                    folderTermId = setting.DefaultTermId;
                }
            }
            //NOTSURE
            logger.Info($"Get folder termId:{folderTermId}");
            return folderTermId;
        }
        public void Run()
        {
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            GetFailedItemIds();
            //TODO  CHANGE TO SIGNAL
            StartDiscoveryThread();
            Thread.Sleep(1000);
            StartAnalyzerThread();
            Thread.Sleep(1000);
            StartPersistThread();
            Thread.Sleep(1000);

            WaitForDiscoveryThreadExit();
            WaitForAnalyzerThreadExit();
            WaitForPersistThreadExit();
            RemoveFailedItemInAzure();
            if (FSJobCache.Instance.FailedItems.Count > FSJobCache.Instance.FailedItemThrottling - 1)
            {
                JobContext.Current.HasErrorNode = true;
                logger.Warn($"Has more than {FSJobCache.Instance.FailedItemThrottling} failed item in job, will not update last job time.");
            }
            else
            {
                if (FSJobCache.Instance.FailedItems.Count > 0)
                {
                    AddFailedItems2Azure();
                    JobContext.Current.HasErrorNode = true;
                }
                FSJobCache.Instance.JobController.StoreJobTime();
            }

            //FSJobCache.Instance.JobController.UpdateScopeSettingProfile();
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                FSJobCache.Instance.FailedCount++;
                JobContext.Current.HasErrorNode = true;
            }
            if (JobContext.Current.AllErrorNode) 
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Failed, JobContext.Current.JobId);
            }
            else if (JobContext.Current.HasErrorNode && !JobContext.Current.AllErrorNode)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
            }
            else if (FSJobCache.Instance.SuccessCount > 0 && FSJobCache.Instance.FailedCount > 0)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
            }
            else if (FSJobCache.Instance.SuccessCount == 0 && FSJobCache.Instance.FailedCount > 0)
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Failed, JobContext.Current.JobId);
            }
            else
            {
                JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
            }
            logger.Info("Collect job finished.");
        }
        private void WaitForDiscoveryThreadExit()
        {
            while (true)
            {
                Thread.Sleep(3 * 1000);
                if (FSJobCache.Instance.AnalyzerCache.Count == 0 && FSJobCache.Instance.DiscoverThreadMonitor.Count == 0)
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
                if (FSJobCache.Instance.SerializerThreadMonitor.Count == 0)
                {
                    logger.Info("There is no serializer thread running now...");
                    break;
                }
            }
        }

        private void StartPersistThread()
        {
            int serializerThreadCount = 1;
            try
            {
                serializerThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.PersistThreadCount));
                logger.Info("serializerThreadCount is " + serializerThreadCount);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while gettting serializerThreadCount.Error:{0}", e.ToString());
            }
            //FSJobCache.Instance.Config.PersistThreadCount;
            for (int i = 0; i < serializerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DataSyncRecordsSerializer serializer = new DataSyncRecordsSerializer();
                    serializer.Run();
                });
            }
        }

        private void StartAnalyzerThread()
        {
            int analyzerThreadCount = 1;
            try
            {
                analyzerThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.AnalyzerThreadCount));
                logger.Info("analyzerThreadCount is " + analyzerThreadCount);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while gettting analyzerThreadCount.Error:{0}", e.ToString());
            }
            for (int i = 0; i < analyzerThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    DataSyncAnalyzerThread analyzer = new DataSyncAnalyzerThread();
                    analyzer.Run();
                });
            }
        }

        private void StartDiscoveryThread()
        {
            int discoveryThreadCount = 1;
            try
            {
                discoveryThreadCount = int.Parse(CommonConfiguration.getConfig(HybridAppSettingKey.DiscoveryThreadCount));
                logger.Info("discoveryThreadCount is " + discoveryThreadCount);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while gettting discoveryThreadCount.Error:{0}", e.ToString());
            }
            for (int i = 0; i < discoveryThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    FSDiscover discovery = new FSDiscover();
                    discovery.Run();
                });
            }
        }

        private void GetFailedItemIds()
        {
            long sortTicks = 0;
            int pageSize = ExternalUtil.TransferDataCount;
            string scopeId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString();
            List<Guid> failedItemIds = new List<Guid>();
            try
            {
                do
                {
                    List<RMAgentSyncFailureItem> data = new List<RMAgentSyncFailureItem>();
                    using (new AgentPerformanceScope("FSDicover.FindSyncFailedItems", addToStatistics: true))
                    {
                        data = JobContext.Current.ApiClient.FindSyncFailedItems((int)SourceFlag.FileSystem, scopeId, sortTicks, pageSize);
                    }
                    if (data != null && data.Count > 0)
                    {
                        FSJobCache.Instance.LastJobFailedItems.AddRange(data);
                        var itemIds = data.Select(d => d.NodeId).ToList();
                        foreach (var id in itemIds)
                        {
                            Guid tempId;
                            if (Guid.TryParse(id, out tempId))
                            {
                                if (!failedItemIds.Contains(tempId))
                                {
                                    failedItemIds.Add(tempId);
                                }
                            }
                        }
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                } while (true);

                FSJobCache.Instance.LastJobFailedItemIds = failedItemIds;
                logger.Info($"Get failed items in last job, count:{failedItemIds.Count}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while FindSyncFailedItems, error:{e.ToString()}");
                //JobDetailService.Commit(new FSDataSyncJobReportDetail()
                //{
                //    Status = JobDetailsStatus.Failed,
                //    Comment = "RM_JM_GetFailedItemFromAzureFailed",
                //    AgentName = OSInformation.HostName
                //});
            }
            //FindSyncFailedItems(scopeId,sortTicks,pageSize);
        }

        private void RemoveFailedItemInAzure()
        {
            try
            {
                var notExistItemIds = GetNotExistItemIds();
                //移除不存在和本次job处理成功的数据
                notExistItemIds = notExistItemIds.Concat(FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems).ToList();
                var successItems = FSJobCache.Instance.LastJobFailedItems.Where(i => notExistItemIds.Contains(new Guid(i.NodeId))).ToList();
                if (successItems.Count > 0)
                {
                    for (int i = 0; i < successItems.Count; i += ExternalUtil.TransferDataCount)
                    {
                        var temp = successItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                        using (new AgentPerformanceScope("FSDicover.RemoveSuccessItemsInAzure", addToStatistics: true))
                        {
                            JobContext.Current.ApiClient.RemoveSuccessItemsInAzure(temp);
                        }
                    }
                }
                logger.Info($"Remove success items in azure, count:{successItems.Count}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while RemoveFailedItemInAzure, error:{e.ToString()}");
                //JobContext.Current.HasErrorNode = true; 
                //JobDetailService.Commit(
                //                      new FSDataSyncJobReportDetail()
                //                      {
                //                          AgentName = OSInformation.HostName,
                //                          Status = JobDetailsStatus.Failed,
                //                          Comment = "RM_JM_RemoveFailedItemInAzureFailed"
                //                      });
            }
        }

        private List<Guid> GetNotExistItemIds()
        {
            List<Guid> notExistItemIds = new List<Guid>();
            try
            {
                var system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
                foreach (var item in FSJobCache.Instance.LastJobFailedItems)
                {
                    if (!system.FileExists(new StorageInfo("", item.URL)))
                    {
                        Guid nodeId;
                        if (Guid.TryParse(item.NodeId, out nodeId))
                        {
                            if (!notExistItemIds.Contains(nodeId))
                            {
                                notExistItemIds.Add(nodeId);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting not exist items, error:{e.ToString()}");
            }
            return notExistItemIds;
        }

        private void AddFailedItems2Azure()
        {
            try
            {
                for (int i = 0; i < FSJobCache.Instance.FailedItems.Count; i += ExternalUtil.TransferDataCount)
                {
                    var temp = FSJobCache.Instance.FailedItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                    using (new AgentPerformanceScope("FSDicover.AddSyncFailedItems", addToStatistics: true))
                    {
                        JobContext.Current.ApiClient.AddSyncFailedItems(temp);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while AddSyncFailedItems, error:{e.ToString()}");
                //JobContext.Current.HasErrorNode = true;
                //JobDetailService.Commit(
                //                       new FSDataSyncJobReportDetail()
                //                       {
                //                           AgentName = OSInformation.HostName,
                //                           Status = JobDetailsStatus.Failed,
                //                           Comment = "RM_JM_AddFailedItemToAzureFailed"
                //                       });
            }
        }

        private static Guid QueryScopeTermIdSetting(AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node)
        {
            Guid scopeId = node.Level == NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : new Guid(node.ID);
            //Guid id = node.FullPath.ToLowerInvariant().ToMd5();
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(scopeId))
            {
                return scopeId;
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

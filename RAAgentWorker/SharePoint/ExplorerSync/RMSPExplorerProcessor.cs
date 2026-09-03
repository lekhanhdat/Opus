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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.RA.SharePoint.SPObjDiscover.DiscoverImpl;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using RAFileSystem.SharePoint.Util;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.ExplorerSync
{
    public class RMSPExplorerProcessor : IScheduleJobWorker
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(RMSPExplorerProcessor));
        private Dictionary<Guid, string> columnInternalNameMap = null;
        private Contract.Global.JobMessage.DataSyncJobMessage mJobMessage;
        private List<Contract.Global.Object.NodeFlag> siteNodeFlags = null;
        private RMSPExplorerDataCache ExplorerCache = null;
        public IProgressService ProgressService { get; set; }
        public IReportService<JMJobDetails> JobDetailService { get; set; }
        private MemoryListCacheService<RecordDto> mCachedRecords;

        public RMSPExplorerProcessor()
        {
            columnInternalNameMap = new Dictionary<Guid, string>();
            siteNodeFlags = new List<Contract.Global.Object.NodeFlag>();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            mCachedRecords = new MemoryListCacheService<RecordDto>();
        }


        public void Run()
        {
            // using (var performance = new AgentPerformanceScope("RMSPExplorerProcesser.RunNow"))
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcesser.RunNow", addToStatistics: true))
            {
                //以site collection分的sub job，所以获取到的都是site collecttion节点
                List<SPTreeNodeDto> siteNodes = new List<SPTreeNodeDto>();
                Thread t = new Thread(new ThreadStart(SendRecordsToExploer));
                t.Start();
                SPTreeNodeDto groupNode = null;
                IAveSite aveSite = null;
                try
                {

                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        //ThrowUtil.ThrowIfNullOrEmpty(jobContext.JobContextSetting, "job context info empty.");

                        List<Contract.Global.Object.RMSPTreeNode> tempList = mJobMessage.TreeNodes;
                        siteNodes = tempList.ConvertAll(node => RMDtoConverter.ConvertRMTree2SPTree(node));
                        foreach (var site in siteNodes)
                        {
                            string siteId = string.Empty;
                            try
                            {
                                logger.Info($"Scan node:{site.FullPath.LogBase64()}, Id:{site.SPObjectId}");
                                groupNode = SPTreeNodeManagement.GetGroupNode(site);
                                ThrowUtil.ThrowIfNull(groupNode, "group node info empty.");

                                long lastScanTime = mJobMessage.SiteInformationDic[site.FullPath].LastScanTime;
                                //GetLastScanTimeFromDB(groupNode.SPObjectId, site.SPObjectId);

                                using (var discoverSite = GetDiscoverSite(site, mJobMessage.MainJobStartTime, lastScanTime, out aveSite))
                                {
                                    siteId = discoverSite.SiteID.ToString();
                                    using (aveSite)
                                    {
                                        Guid bcsColumnID = Guid.Empty;
                                        var internalName = GetBCSColumnInternalName(site, aveSite, mJobMessage.SiteInformationDic, ref bcsColumnID);
                                        if (string.IsNullOrEmpty(internalName))
                                        {
                                            logger.Warn($"site doesn't have bcs column:{site.FullPath.LogBase64()}");
                                            continue;
                                        }

                                        RMSPExplorerDataCache.Instance.InitSiteLevelCache(siteId, new RMSPExplorerSiteLevelCache
                                        {
                                            AveSiteId = site.ID,
                                            BCSColumnInternalName = internalName,
                                            BCSColumnID = bcsColumnID,
                                            HasErrorNode = false,
                                            SPSiteId = aveSite.ID
                                        });
                                        RMSPExplorerDataCache.Instance.LoadFailedItems(siteId);

                                        RMSPExplorerBase jobWorker = null;
                                        if (lastScanTime == DateTime.MinValue.Ticks)
                                        {
                                            logger.Info($"site:{site.FullPath.LogBase64()}, full scan.");
                                            jobWorker = BuildJobWorker(SPDiscoverType.Full, discoverSite, site, lastScanTime, mJobMessage.MainJobStartTime);
                                            jobWorker.RunNow();
                                        }
                                        else if (NeedRunSearchDiscover(lastScanTime))
                                        {
                                            logger.Info($"site:{site.FullPath.LogBase64()}, search scan. Last job time:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()}");
                                            jobWorker = BuildJobWorker(SPDiscoverType.CAMLSearch, discoverSite, site, lastScanTime, mJobMessage.MainJobStartTime);
                                            jobWorker.RunNow();
                                            WaitAndSendAllRecords();
                                            jobWorker.ProcessTermChangedItems(lastScanTime, mJobMessage.SiteInformationDic[site.FullPath].ChangedTermIds, mJobMessage.MainJobStartTime);
                                        }
                                        else
                                        {
                                            logger.Info($"site:{site.FullPath.LogBase64()}, sp inc date from:{new DateTime(lastScanTime, DateTimeKind.Utc).ToString()} to {new DateTime(mJobMessage.MainJobStartTime, DateTimeKind.Utc).ToString()}");
                                            jobWorker = BuildJobWorker(SPDiscoverType.Incremental, discoverSite, site, lastScanTime, mJobMessage.MainJobStartTime);
                                            jobWorker.RunNow();
                                            WaitAndSendAllRecords();
                                            // process records by term rule changed.
                                            jobWorker.ProcessTermChangedItems(lastScanTime, mJobMessage.SiteInformationDic[site.FullPath].ChangedTermIds, mJobMessage.MainJobStartTime);
                                        }
                                    }
                                    RemoveFailedItemInAzure();
                                    if (!ExplorerCache.SiteLevelCache[siteId].HasErrorNode)
                                    {
                                        //需要插入Flag 或者更新Flag中的时间
                                        if (RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count > 999)
                                        {
                                            logger.Info("More than 1000 failed items in site {0}, count {2}", aveSite.Url.LogBase64(), RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count);
                                            //failure 数量大于 1000， 不插入Azure Table， 
                                            JobContext.Current.HasErrorNode = true;
                                            ExplorerCache.SiteLevelCache[siteId].HasErrorNode = true;
                                        }
                                        else
                                        {
                                            logger.Info("Failed items count{0}, in site {1}", RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count, aveSite.Url.LogBase64());
                                            //将失败的Item插入Azure Table， 下次Job再处理
                                            AddFailedItems2Azure();
                                            //如果存在失败数据， Job状态不能是Finish
                                            if (RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count > 0)
                                            {
                                                JobContext.Current.HasErrorNode = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //HasErrorNode， 不会更新Flag， 也不需要单独处理此次失败的Item。
                                        logger.Info("Has error container in site {0}, ignore the fail items, count {1}", aveSite.Url.LogBase64(), RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count);
                                    }

                                    if (!ExplorerCache.SiteLevelCache[siteId].HasErrorNode)
                                    {
                                        logger.Info($"update the site node flag info.");
                                        siteNodeFlags.Add(new Contract.Global.Object.NodeFlag()
                                        {
                                            NodeId = new Guid(site.SPObjectId),
                                            Title = site.Name,
                                            FullPath = site.FullPath,
                                            CollectionTime = mJobMessage.MainJobStartTime,
                                            GroupId = new Guid(groupNode.SPObjectId),
                                            IsRemoved = false,
                                            NodeFlagType = 3
                                        });
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Process Site error, Path:{site?.FullPath.LogBase64()}, ERROR:{ex.ToString()}");
                                JobContext.Current.HasErrorNode = true;
                                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                                {
                                    ObjectName = site?.Name,
                                    FullPath = site?.FullPath,
                                    Status = JobDetailsStatus.Failed,
                                    Comment = ex.Message,
                                    AgentName = OSInformation.HostName
                                });
                            }
                            finally
                            {
                                RMSPExplorerDataCache.Instance.ResetFailedItemCache();
                                if (ExplorerCache.SiteLevelCache.ContainsKey(siteId))
                                {
                                    ExplorerCache.SiteLevelCache[siteId].Dispose();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    JobContext.Current.HasErrorNode = true;
                    logger.Error($"error occurred while Process Sync Job, ERROR:{e.ToString()}");
                    JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                    {
                        ObjectName = string.Empty,
                        FullPath = string.Empty,
                        Status = JobDetailsStatus.Failed,
                        Comment = e.Message,
                        AgentName = OSInformation.HostName
                    });
                }
                finally
                {
                    SendSiteNodeFlag();
                    ExplorerCache.JobIsFinish = true;
                    WaitForComplete();

                    try
                    {
                        JobContext.Current.Cleanup();
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                    }

                    if (JobContext.Current.HasErrorNode)
                    {
                        HybridApiClient.Instance.UpdateJobState(JobContext.Current.JobId, (int)JobStatus.FinishWithException, "");
                    }
                    else
                    {
                        HybridApiClient.Instance.UpdateJobState(JobContext.Current.JobId, (int)JobStatus.Finished, "");
                    }
                }
            }
        }

        private void RemoveFailedItemInAzure()
        {
            try
            {
                //移除不存在和本次job处理成功的数据
                List<RMAgentSyncFailureItem> successItems = new List<RMAgentSyncFailureItem>();
                foreach (var kv in RMSPExplorerDataCache.Instance.LastJobFailedItems)
                {
                    successItems.AddRange(kv.Value.Where(v => RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Contains(new Guid(v.ItemId))).ToList());
                }
                if (successItems.Count > 0)
                {
                    for (int i = 0; i < successItems.Count; i += ExternalUtil.TransferDataCount)
                    {
                        var temp = successItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                        using (new AgentPerformanceScope("RMSPExplorerProcessor.RemoveSuccessItemsInAzure", addToStatistics: true))
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
                //                     new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                //                     {
                //                         AgentName = OSInformation.HostName,
                //                         Status = JobDetailsStatus.Failed,
                //                         Comment = "RM_JM_RemoveFailedItemInAzureFailed"
                //                     });
            }
        }

        private void AddFailedItems2Azure()
        {
            try
            {
                for (int i = 0; i < RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count; i += ExternalUtil.TransferDataCount)
                {
                    var temp = RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                    using (new AgentPerformanceScope("RMSPExplorerProcessor.AddSyncFailedItems", addToStatistics: true))
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
                //                       new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                //                       {
                //                           AgentName = OSInformation.HostName,
                //                           Status = JobDetailsStatus.Failed,
                //                           Comment = "RM_JM_AddFailedItemToAzureFailed"
                //                       });
            }
        }

        //上次运行job是在59天以前，本次Job采用CAML Query方式，防止由于change log被冲掉了导致少查数据
        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }

        private void WaitForComplete()
        {
            while (!ExplorerCache.SendDataFinish)
            {
                Thread.Sleep(5000);
            }
        }

        private void WaitAndSendAllRecords()
        {
            while (true)
            {
                if (ExplorerCache.NeedSyncDataCache.Count > 100 || mCachedRecords.Count > ExternalUtil.TransferDataCount)
                {
                    logger.Info($"Waiting for send remaining records. NeedSyncDataCache count:{ExplorerCache.NeedSyncDataCache.Count} Cached records count:{ mCachedRecords.Count}");
                    Thread.Sleep(3000);
                    continue;
                }

                try
                {
                    if (ExplorerCache.NeedSyncDataCache.Count > 0)
                    {
                        var tempData = ExplorerCache.NeedSyncDataCache.TakeAll();
                        mCachedRecords.AddBatch(tempData);
                    }
                    var records = mCachedRecords.TakeAll().ToList();
                    if (records != null && records.Count > 0)
                    {
                        logger.Info("Send file count:{0}", records.Count);
                        Contract.FileSystem.AgentSyncDataResultDto result;
                        using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.AddSPDataToExplorer", $"RMSPExplorerProcessor.AddSPDataToExplorer.Count:{records.Count}", true))
                        {
                            result = HybridApiClient.Instance.AddSPDataToExplorer(records);
                        }
                        AddReport(result?.FailedGuids, result?.SkippedGuids, records);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while send all sp data to explorer. Error:{0}", e.ToString());
                }
                break;
            }

            logger.Info("All records has been sent to explorer.");
        }

        private void SendRecordsToExploer()
        {
            try
            {
                while (true)
                {
                    if (ExplorerCache.JobIsFinish && ExplorerCache.NeedSyncDataCache.Count == 0)
                    {
                        break;
                    }

                    if (ExplorerCache.NeedSyncDataCache.Count == 0)
                    {
                        Thread.Sleep(3000);
                        continue;
                    }
                    try
                    {
                        var records = ExplorerCache.NeedSyncDataCache.Take(100).ToList();
                        if (records != null && records.Count > 0)
                        {
                            mCachedRecords.AddBatch(records);
                            if (mCachedRecords.Count > ExternalUtil.TransferDataCount)
                            {
                                var mSendRecords = mCachedRecords.Take(ExternalUtil.TransferDataCount).ToList();
                                logger.Info("Send file count:{0}", mSendRecords.Count);
                                Contract.FileSystem.AgentSyncDataResultDto result;
                                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.AddSPDataToExplorer", $"RMSPExplorerProcessor.AddSPDataToExplorer.Count:{mSendRecords.Count}", true))
                                {
                                    result = HybridApiClient.Instance.AddSPDataToExplorer(mSendRecords);
                                }
                                AddReport(result?.FailedGuids, result?.SkippedGuids, mSendRecords);
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
                ExplorerCache.SendDataFinish = true;
            }
        }

        private void FinialSendData()
        {
            if (mCachedRecords.Count > 0)
            {
                var records = mCachedRecords.TakeAll().ToList();
                logger.Info("Send file count:{0}", records.Count);
                Contract.FileSystem.AgentSyncDataResultDto result;
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.AddSPDataToExplorer", $"RMSPExplorerProcessor.AddSPDataToExplorer.Count:{records.Count}", true))
                {
                    result = HybridApiClient.Instance.AddSPDataToExplorer(records);
                }
                AddReport(result?.FailedGuids, result?.SkippedGuids, records);
            }
        }

        private void AddReport(List<Guid> failedIds, List<Guid> skipIds, List<RecordDto> records)
        {
            //if (failedIds != null && failedIds.Count > 0)
            //{
            //    JobContext.Current.HasErrorNode = true;
            //}
            foreach (var record in records)
            {
                if (record.NodeType != (int)NodeLevel.Item)
                {
                    continue;
                }
                JobDetailsStatus status = JobDetailsStatus.Successful;
                string comment = string.Empty;
                if (failedIds != null && failedIds.Contains(record.Id))
                {
                    Add2FailedItemCache(record);
                    status = JobDetailsStatus.Failed;
                    comment = "RM_JM_FSFailedAddToExplorer";
                }
                else if (skipIds != null && skipIds.Contains(record.Id))
                {
                    status = JobDetailsStatus.Skipped;
                    comment = "RM_JM_FSSkipAddToExplorer";
                }
                else
                {
                    if (Add2SuccessItemCache(record))
                    {
                       // comment = "RM_JM_SyncFailedItemSuccess";
                    }
                }
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = record.LeafName,
                    FullPath = record.FullPath,
                    Status = status,
                    Comment = comment,
                    AgentName = OSInformation.HostName
                });
            }
        }

        private void Add2FailedItemCache(RecordDto dto)
        {
            if (RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Count <= 1000
                && dto.NodeType == (int)NodeLevel.Item && !RMSPExplorerDataCache.Instance.LastJobFailedItemIds.Contains(dto.ItemId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = dto.ScopeId.ToString(),
                    ItemId = dto.ItemId.ToString(),
                    IntemIntId = dto.ItemRowId,
                    ListId = dto.ListId.ToString(),
                    WebId = dto.WebId.ToString(),
                    URL = dto.DirPath,
                    SortTicks = Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    SourceFlag = (int)SourceFlag.SharePointOnPrem,
                    ParentId = dto.FolderId.ToString(),
                    ObjectName = dto.LeafName,
                    Message = "RM_JM_FSFailedAddToExplorer"
                };
                RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Add(item);
            }
        }

        private bool Add2SuccessItemCache(RecordDto dto)
        {
            bool success = false;
            if (RMSPExplorerDataCache.Instance.LastJobFailedItemIds.Contains(dto.ItemId)
                && !RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Contains(dto.ItemId))
            {
                RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Add(dto.ItemId);
                success = true;
            }
            return success;
        }

        private void SendSiteNodeFlag()
        {
            if (siteNodeFlags.Count > 0)
            {
                for (int i = 0; i < siteNodeFlags.Count; i += 100)
                {
                    try
                    {
                        var nodeFlags = siteNodeFlags.Skip(i).Take(100).ToList();
                        HybridApiClient.Instance.AddSiteFlagInfos(nodeFlags);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while sending site node flags. Error:{0}", e.ToString());
                    }
                }
            }
        }
        private RMSPExplorerBase BuildJobWorker(SPDiscoverType discoverType, AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, long lastJobTicks, long mainJobTicks, List<AveCamlQuery> camlQueries = null)
        {
            //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.BuildJobWorker"))
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.BuildJobWorker", addToStatistics: true))
            {
                RMSPDiscoverHelper discoverHelper = null;
                ISPDiscover sPDiscover = null;

                discoverHelper = new RMSPDiscoverHelper();
                sPDiscover = RMSPDiscoverFactory.CreateFactory(discoverHelper, discoverType);
                var retentionWorker = new RMSPExplorerBase(discoverSite, treeNode);

                retentionWorker.Init(sPDiscover, discoverType, lastJobTicks, mainJobTicks);
                return retentionWorker;
            }

        }


        private AveDiscoverSite GetDiscoverSite(SPTreeNodeDto site, long mainJobStartTime, long lastScanTime, out IAveSite aveSite)
        {
            //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.initSite"))
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.InitSite", addToStatistics: true))
            {
                //var remoteSite = new SharePointSettingUtility().GetRemoteSiteCollection(site.SPObjectId.ToString());
                var bposInfo = GetBPOSInfo(site.FullPath);
                var mfactory = AveObjectModelFactory.CreateObjectModelFactory(site.FullPath, bposInfo, AveContextKind.ClientObjectModel);
                aveSite = mfactory.CreateSite(site.FullPath);

                return lastScanTime == DateTime.MinValue.Ticks ? new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive) : new AveDiscoverSite(aveSite, bposInfo, AveDiscoveryKind.API, DiscoverModule.Archive, new DateTime(lastScanTime, DateTimeKind.Utc), new DateTime(mainJobStartTime, DateTimeKind.Utc));
            }

        }

        private AveBPOSAccountInfo GetBPOSInfo(string siteUrl)
        {
            var account = AgentAccountUtil.Get();
            AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
            {
                Domain = account.Domain,
                UserName = account.UserName,
                Password = account.Password
            };

            return aveBPOSAccountInfo;

        }

        private string GetBCSColumnInternalName(SPTreeNodeDto treeNode, IAveSite aveSite, Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo> siteInfos, ref Guid bcsColumnID)
        {
            //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor..GetSPSetting"))
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor..GetSPSetting", addToStatistics: true))
            {
                var internalName = string.Empty;

                var scopeId = Guid.Parse(treeNode.SPObjectId);
                var groupId = Guid.Parse(SPTreeNodeManagement.GetGroupNode(treeNode).SPObjectId);

                if (columnInternalNameMap.ContainsKey(scopeId))
                {
                    return columnInternalNameMap[scopeId];
                }

                //get from message
                var columnName = siteInfos[treeNode.FullPath].BCSColumnName;
                //new SharePointSettingUtility().GetMedataColumn(groupId);
                if (!(columnName == null || columnName == string.Empty))
                {
                    logger.Info("Column name on group:{0}, groupId {1}", columnName.LogBase64(), groupId);
                    var field = GetTaxonomyField(aveSite.RootWeb.Fields, columnName);
                    if (field != null)
                    {
                        internalName = field.InternalName;
                        if (!columnInternalNameMap.ContainsKey(scopeId))
                        {
                            columnInternalNameMap.Add(scopeId, internalName);
                        }
                        bcsColumnID = field.ID;
                    }

                }
                return internalName;
            }


        }

        protected IAveTaxonomyField GetTaxonomyField(IAveFieldCollection fields, string rmFieldTitle)
        {
            //var field = fields.GetField(rmFieldTitle);
            var field = fields.AsQueryable().Where(f => f.Title.Equals(rmFieldTitle, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return field as IAveTaxonomyField;
        }

        public void Bind(string msg)
        {
            mJobMessage = SerializerHelper.DeserializeByDataContractSerializer<Contract.Global.JobMessage.DataSyncJobMessage>(msg);
            JobContext.Current.JobMessage = msg;
            JobContext.Current.JobStartTime = DateTime.UtcNow;
            JobContext.Current.BulkImportEnabled = mJobMessage.BulkImportEnabled;
            JobContext.Current.BulkSize = mJobMessage.BulkSize;
            ExplorerCache = RMSPExplorerDataCache.Instance;
        }
    }
}

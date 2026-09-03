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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.ExplorerSync.Cache
{
    public class RMSPExplorerDataCache : IDisposable
    {
        public IReportService<JMJobDetails> JobDetailService { get; set; }
        private AveLogger logger = AveLogger.GetInstance(typeof(RMSPExplorerDataCache));
        private static object locker = new object();
        static RMSPExplorerDataCache _instance;
        public static RMSPExplorerDataCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMSPExplorerDataCache();
                            _instance.Initialize(JobContext.Current.JobMessage);
                        }
                    }
                }
                return _instance;
            }
        }

        public Dictionary<string, RMSPExplorerSiteLevelCache> SiteLevelCache = null;
        public Dictionary<Guid, RMRuleItemCollection> TermRuleMapping { get; private set; }

        public Dictionary<Guid, Contract.Global.Object.RMTermInfo> Terms { get; private set; }

        public Dictionary<Guid, Rule> Rules { get; private set; }

        public Contract.Global.Object.SOArchiverSettings ArchiverSettings { get; private set; }

        public ICacheService<RecordDto> NeedSyncDataCache { get; set; }

        //key is list id
        public Dictionary<Guid, List<RMAgentSyncFailureItem>> LastJobFailedItems = new Dictionary<Guid, List<RMAgentSyncFailureItem>>();

        public List<Guid> LastJobFailedItemIds = new List<Guid>();

        public List<Guid> SuccessSyncedFailedItemIds = new List<Guid>();

        public List<RMAgentSyncFailureItem> CurrentJobFailedItems = new List<RMAgentSyncFailureItem>();

        public bool JobIsFinish { get; set; }

        public bool SendDataFinish { get; set; }

        private void Initialize(object msgStr)
        {
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage>(msgStr.ToString());
            SiteLevelCache = new Dictionary<string, RMSPExplorerSiteLevelCache>();
            LoadRules(msg);
            LoadTerms(msg);
            AssembleTermRuleMapping(msg);
            AssembleArchiverSettings(msg);
            NeedSyncDataCache = new MemoryListCacheService<RecordDto>();
            NeedSyncDataCache.SetThrottling(5000);
        }

        public void InitSiteLevelCache(string key, RMSPExplorerSiteLevelCache value)
        {
            if (!SiteLevelCache.ContainsKey(key))
            {
                SiteLevelCache.Add(key, value);
            }
        }

        public void AssembleArchiverSettings(AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage msg)
        {
            logger.Debug("Begin to assemble archiver setting to cache.");
            ArchiverSettings = msg.ArchiverSetting;
        }

        private void AssembleTermRuleMapping(AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage msg)
        {
            logger.Debug("Begin to assemble term rules mappings to cache.");
            TermRuleMapping = DtoConverter.ConvertGlobalRuleTermMappingToAgentRuleTermMapping(msg.TermAndRulesMapping);
        }

        private void LoadTerms(AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage msg)
        {
            logger.Debug("Begin to load terms to cache.");
            Terms = msg.Terms;
            logger.Info("Loaded {0} terms to memory cache.", Terms.Count);
        }

        public void LoadFailedItems(string siteId)
        {
            long sortTicks = 0;
            int pageSize = ExternalUtil.TransferDataCount;
            List<Guid> failedItemIds = new List<Guid>();
            List<RMAgentSyncFailureItem> failedItems = new List<RMAgentSyncFailureItem>();
            try
            {
                do
                {
                    List<RMAgentSyncFailureItem> data = new List<RMAgentSyncFailureItem>();
                    using (new AgentPerformanceScope("RMSPExplorerDataCache.FindSyncFailedItems", addToStatistics: true))
                    {
                        data = JobContext.Current.ApiClient.FindSyncFailedItems((int)SourceFlag.SharePointOnPrem, siteId, sortTicks, pageSize);
                    }
                    if (data != null && data.Count > 0)
                    {
                        failedItems.AddRange(data);
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
                failedItems.ForEach(i =>
                {
                    var itemId = new Guid(i.ItemId);
                    if (!LastJobFailedItemIds.Contains(itemId))
                    {
                        LastJobFailedItemIds.Add(itemId);
                    }
                });
                LastJobFailedItems = failedItems.GroupBy(i => i.ListId).ToDictionary(k => new Guid(k.Key), v => v.ToList());

                logger.Info($"Get failed items in last job, count:{failedItems.Count}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while FindSyncFailedItems, error:{e.ToString()}");
                //JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                //{
                //    Status = JobDetailsStatus.Failed,
                //    Comment = "RM_JM_GetFailedItemFromAzureFailed",
                //    AgentName = OSInformation.HostName
                //});
            }
        }

        public void ResetFailedItemCache()
        {
            LastJobFailedItems.Clear();
            LastJobFailedItemIds.Clear();
            SuccessSyncedFailedItemIds.Clear();
            CurrentJobFailedItems.Clear();
        }

        private void LoadRules(AvePoint.RA.Contract.Global.JobMessage.DataSyncJobMessage msg)
        {
            logger.Debug("Begin to Load rules to cache.");
            Dictionary<Guid, Rule> rules = new Dictionary<Guid, Rule>();
            foreach (var ruleInfo in msg.Rules)
            {
                rules.Add(ruleInfo.Key, DtoConverter.ConvertGlobalRule2Rule(ruleInfo.Value));
            }
            Rules = rules;
            logger.Debug("End to load Rules to cache");
        }

        private Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class RMSPExplorerSiteLevelCache : IDisposable
    {
        public RMSPExplorerSiteLevelCache()
        {
        }

        public string BCSColumnInternalName { get; set; }
        public Guid BCSColumnID { get; set; }
        public bool HasErrorNode { get; set; } = false;
        public string AveSiteId { get; set; }
        public Guid SPSiteId { get; set; }
        public void Dispose()
        {
            BCSColumnInternalName = null;
            AveSiteId = null;
            HasErrorNode = false;
        }
    }

    public class RMSPExplorerListLevelCache : IDisposable
    {
        private static object locker = new object();
        public RMSPExplorerListLevelCache()
        {
        }

        static RMSPExplorerListLevelCache _instance;
        public static RMSPExplorerListLevelCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new RMSPExplorerListLevelCache();
                        }
                    }
                }
                return _instance;
            }
        }

        public void Add(string key, SyncItemRuleInfo rule)
        {
            lock (locker)
            {
                if (FolderRule.ContainsKey(key))
                {
                    FolderRule[key] = rule;
                }
                else
                {
                    FolderRule.Add(key, rule);
                }
            }

        }

        public Dictionary<string, SyncItemRuleInfo> FolderRule { get; private set; } = new Dictionary<string, SyncItemRuleInfo>();

        public void Dispose()
        {
            if (FolderRule != null)
            {
                FolderRule.Clear();
            }

        }
    }

    //internal static class IEnumerableExtensions
    //{
    //    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    //    {
    //        foreach (var item in source)
    //        {
    //            action(item);
    //        }
    //    }
    //}
}

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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Common.ApprovalService4JPMC;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Interface;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Implement
{
    public class JPMCScanDiscovrerNodeWorker : IDiscoverNodeWorker, ISiteMetricsDeletionHandler
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal IBackwardDependencyNodeCache<ArchiveApproveReport4JPMC> mApprovalReportProxy;
        internal ScheduleConfiguration config = null;
        internal ScanJobSettings mJobSettings = null;
        private JPMCTenantConfig mJPMCTenantConfig = null;
        internal List<int> systemListTable = new List<int>();
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal RuleManagement mRuleEngine;
        private static long _processedCount = 0;

        public JPMCScanDiscovrerNodeWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs, JPMCTenantConfig jpmcTenantConfig)
        {
            mJobSettings = jobSettings;
            config = paraConfig;
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport4JPMC>(new ApprovalReportService4JPMC(config));
            mDependencyObjs = dependencyObjs;
            systemListTable = ScheduleConfiguration.ListTemplate;
            mJPMCTenantConfig = jpmcTenantConfig;
        }

        public void Dispose()
        {
            using (mApprovalReportProxy) { }
        }

        public void Init(object obj)
        {
            RuleNodeContract nodeContract = obj as RuleNodeContract;
            RuleEngine = new RuleManagement(config.RuleCollection);
        }
        public RuleManagement RuleEngine
        {
            get
            {
                return mRuleEngine;
            }
            set
            {
                mRuleEngine = value;
            }
        }

        public virtual bool IsRuleBreakInheritNode(string sha1URL)
        {
            return false;
        }

        public virtual async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainer"))
            {
                ProcessResult result = ProcessResult.Default;
                mLog.Info(string.Format("begin to scan container. Type:{0}, Name:{1} ", item.Cache_NodeType.ToString(), item.Name));
                if (item.Cache_NodeType == (int)CacheNodeType.WebApplication)
                {
                    //Do not scan webapplication
                }
                else
                {
                    TransmitToNextLayer(item);
                }
                mLog.Info(string.Format("finish to scan container. Type:{0}, Name:{1}, result:{2} ", item.Cache_NodeType.ToString(), item.Name, result.ToString()));
                return result;
            }
        }

        public virtual Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            return RealProcessItemAsync(item, parent);
        }

        public virtual void Flush()
        {
            mApprovalReportProxy.Flush();
        }

        public Task RemoveWebDataAsync(Guid webId, string webUrl)
        {
            if (webId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            try
            {
                mLog.Info($"Change log indicates deleted web {webUrl} ({webId}). Purging cached scan data.");
                ExecuteApprovalReportProxyAction(service => service.DeleteWebData(webId), $"deleted web {webUrl} ({webId})");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to purge cached scan data for deleted web {webUrl} ({webId}). Error: {ex}");
            }

            return Task.CompletedTask;
        }

        public Task RemoveListDataAsync(Guid webId, Guid listId, string listUrl)
        {
            if (listId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            try
            {
                var listIdentifier = string.IsNullOrWhiteSpace(listUrl) ? listId.ToString("D") : listUrl;
                mLog.Info($"Change log indicates deleted list {listIdentifier} under web {webId}. Purging cached scan data.");
                ExecuteApprovalReportProxyAction(service => service.DeleteListData(webId, listId), $"deleted list {listIdentifier} (web:{webId}, list:{listId})");
            }
            catch (Exception ex)
            {
                var listIdentifier = string.IsNullOrWhiteSpace(listUrl) ? listId.ToString("D") : listUrl;
                mLog.Warn($"Failed to purge cached scan data for deleted list {listIdentifier} (web:{webId}, list:{listId}). Error: {ex}");
            }

            return Task.CompletedTask;
        }

        public Task RemoveFolderDataAsync(Guid webId, Guid listId, IEnumerable<Guid> folderIds, string listUrl)
        {
            if (listId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            var ids = folderIds?.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids == null || ids.Count == 0)
            {
                return Task.CompletedTask;
            }

            try
            {
                var idListText = string.Join(",", ids);
                mLog.Info($"Change log indicates deleted folders under list {listUrl} ({listId}); folder count: {ids.Count}; folder IDs: {idListText}.");
                ExecuteApprovalReportProxyAction(service => service.DeleteFolderData(ids), $"deleted folders for list {listUrl} ({listId})");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to purge cached scan data for deleted folders under list {listUrl} ({listId}). Error: {ex}");
            }

            return Task.CompletedTask;
        }

        public Task RemoveItemDataAsync(Guid webId, Guid listId, IEnumerable<Guid> itemIds, string listUrl)
        {
            if (listId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            var ids = itemIds?.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids == null || ids.Count == 0)
            {
                return Task.CompletedTask;
            }

            try
            {
                var idListText = string.Join(",", ids);
                mLog.Info($"Change log indicates deleted items under list {listUrl} ({listId}); item count: {ids.Count}; item IDs: {idListText}.");
                ExecuteApprovalReportProxyAction(service => service.DeleteItemData(ids), $"deleted items for list {listUrl} ({listId})");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to purge cached scan data for deleted items under list {listUrl} ({listId}). Error: {ex}");
            }

            return Task.CompletedTask;
        }

        public async Task<ProcessResult> RealProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.RealProcessItem"))
            {
                long currentCount = Interlocked.Increment(ref _processedCount); // global processed count
                mLog.Info(string.Format("begin to scan item, ID:{0}, ProcessedCount:{1}.", item.ID, currentCount));
                ProcessResult result = ProcessResult.Default;
                Rule resultRule = null;
                //System Item not to check rule
                if (item.Parent != null && !string.IsNullOrEmpty(item.Parent.RuleId) && item.Parent.DoDelete)
                {
                    item.RuleId = item.Parent.RuleId;
                    item.DoDelete = item.Parent.DoDelete;
                    item.ShouldDoArchive = true;
                    item.ArchiveLevel = true;
                    item.RuleName = item.Parent.RuleName;
                    var rule = config.RuleCollection.Values.Where(r => r.Id.Equals(item.RuleId))?.FirstOrDefault();
                    if (rule != null && (rule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document || rule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Item))
                    {
                        item.ForcedReport = true;
                    }
                    TransmitToNextLayer(item);
                    return ProcessResult.FitParentRule;
                }
                else if (item.IsSystemObject)
                {
                    return ProcessResult.SkipCurrentNode;
                }
                resultRule = await CheckItemRuleAsync(item);
                ProcessItemCheckResultNode(resultRule, ref item, parent);
                TransmitToNextLayer(item);

                mLog.Info(string.Format("finish to scan item, id:{0}.", item.ID));
                return result;
            }
        }

        private void ProcessItemCheckResultNode(Rule rule, ref ArchiverNodeItem item, ArchiverNodeItem parent)// to do unit test
        {
            if (rule != null)
            {
                item.DoDelete = true;
                item.ShouldDoArchive = item.ArchiveLevel = true;
                item.RuleId = rule.Id;
                item.RuleName = rule.Name;
            }
            else if (parent.ShouldDoArchive)
            {
                item.DoDelete = true;
                item.ShouldDoArchive = true;
                item.ArchiveLevel = true;
                item.RuleId = item.Parent.RuleId;
                item.RuleName = item.Parent.RuleName;
            }
            else
            {
                item.ShouldDoArchive = false;
            }
        }

        internal virtual void TransmitToNextLayer(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.TransmitToNextLayer"))
            {
                mApprovalReportProxy.PutIn(ConvertToArchiveApproveReport4JPMC(item), item.Cache_NodeType, item.ShouldDoArchive);
            }
        }

        private ArchiveApproveReport4JPMC ConvertToArchiveApproveReport4JPMC(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.NodeItem.ConvertToArchiveApproveReport4JPMC"))
            {
                IAveListItem itemObject = null;
                ArchiveApproveReport4JPMC result = new ArchiveApproveReport4JPMC();
                result.ScanTime = DateTime.UtcNow.Ticks;//arthur: maybe need pass this value from outside
                result.ScanJobID = mJobSettings.SubJobId;
                result.FullPath = item.FullPath;
                result.LeafName = item.Name == null ? "null" : item.Name;
                result.LibRowId = item.LibRowID;
                result.NodeId = item.ID.ToString();
                result.NodeType = item.Cache_NodeType >= 10000 ? (int)item.ItemType : (int)item.NodeType;

                //区分item,document及它们的version，SPNodeLevel：item为500，document为505，item version为550，document version为555.
                switch (item.ItemType)
                {
                    case ArchiverCommon.ItemType.ITEM_TYPE:
                        {
                            result.SPNodeLevel = 500;
                            if (item.DiscoverSPObject is AveDiscoverItem)
                            {
                                itemObject = (item.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                            }
                            if (item.DiscoverSPObject is IAveListItem)
                            {
                                itemObject = item.DiscoverSPObject as IAveListItem;
                            }
                            break;
                        }
                    case ArchiverCommon.ItemType.ITEM_VERSION:
                        {
                            result.SPNodeLevel = 550;
                            break;
                        }
                    case ArchiverCommon.ItemType.DOCUMENT:
                        {
                            result.SPNodeLevel = 505;
                            if (item.DiscoverSPObject is AveDiscoverItem)
                            {
                                itemObject = (item.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                            }
                            if (item.DiscoverSPObject is IAveListItem)
                            {
                                itemObject = item.DiscoverSPObject as IAveListItem;
                            }
                            break;
                        }
                    case ArchiverCommon.ItemType.DOCUMENT_VER:
                        {
                            result.SPNodeLevel = 555;
                            break;
                        }
                    default:
                        {
                            result.SPNodeLevel = (int)item.SPNodeLevel;
                            break;
                        }
                }
                result.CacheNodeType = item.Cache_NodeType;
                result.ParentId = item.Parent == null ? Guid.Empty.ToString() : item.Parent.ID.ToString();

                result.RuleId = item.RuleId == null ? null : item.RuleId;
                result.RuleName = item.RuleName == null ? null : item.RuleName;

                result.DocumentSize = item.DocumentSize;
                result.Created = item.Created;
                result.CreatedBy = item.CreatedBy;
                //result.Modified = item.Modified;
                result.ModifiedBy = item.ModifiedBy;
                result.LastModifiedTime = item.Modified;
                result.ActionTaken = item.ActionTaken;
                result.SiteUrl = item.SiteUrl == null ? string.Empty : item.SiteUrl;
                result.WebID = item.WebId.ToString();
                result.ListID = item.ListId.ToString();
                if (itemObject != null)
                {
                    result.ClassCode = itemObject.FieldValues.GetValue(mJPMCTenantConfig.CustomColumns.ClassCode)?.ToString();
                    result.CountryCode = itemObject.FieldValues.GetValue(mJPMCTenantConfig.CustomColumns.CountryCode)?.ToString();
                    result.RecordStatus = itemObject.FieldValues.GetValue(mJPMCTenantConfig.CustomColumns.RecordStatus)?.ToString();
                }
                return result;
            }
        }

        internal virtual async Task<Rule> CheckItemRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                Rule result = null;
                switch (item.ItemType)
                {
                    case ArchiverCommon.ItemType.DOCUMENT:
                        if (mRuleEngine.HasDocumentCondition)
                        {
                            result = mRuleEngine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        break;

                    case ArchiverCommon.ItemType.ITEM_TYPE:
                        if (mRuleEngine.HasItemCondition)
                        {
                            result = mRuleEngine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        break;
                    case ArchiverCommon.ItemType.DOCUMENT_VER:
                        if (mRuleEngine.HasDocVersionCondition)
                        {
                            result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;

                    case ArchiverCommon.ItemType.ITEM_VERSION:
                        if (mRuleEngine.HasItemVersionCondition)
                        {
                            result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;
                    case ArchiverCommon.ItemType.ATTACHMENT:
                        if (mRuleEngine.HasAttachmentCondition)
                        {
                            result = mRuleEngine.CheckAttachmentCriteria(item.Parent.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;
                    default:
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule);
                }
                return result;
            }
        }

        private void ExecuteApprovalReportProxyAction(Action<ApprovalReportService4JPMC> action, string operationContext)
        {
            if (action == null)
            {
                return;
            }

            mApprovalReportProxy.ExecuteContainerAction(container =>
            {
                if (container is ApprovalReportService4JPMC service)
                {
                    action(service);
                }
                else
                {
                    mLog.Warn($"Approval report proxy container does not support operation for {operationContext ?? "unknown context"}.");
                }
            });
        }
    }
}

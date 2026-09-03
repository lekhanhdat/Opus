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
using System.Linq;
using System.Text;
using AvePoint.RA.I18N.Core;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Common;
using System.Reflection;
//using Microsoft.SharePoint;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
//using AvePoint.Adonis.StorageOptimization.Common.Object;
//using AvePoint.Wrapper.Contract;
using AvePoint.Wrapper.Discovery;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.RA.SharePoint.ExplorerSync.Utils;
using System.Collections;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMReport;
using AvePoint.GCommon.Contract.CommonFilter;
using HtmlAgilityPack;
using RazorEngine;
using Microsoft.SharePoint.Client;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.Common.Util;
using System.Globalization;
using AvePoint.RA.Common.RMRuleManagement;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Implement
{
    public class RecordsSharePointScanDiscoverNodeWorker : DiscoverNodeWorkerBase
    {
        #region Private fields
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ManualUtil manualUtil;
        #endregion

        #region Properties
        private RMSPExplorerSiteLevelCache mSiteLevelCache = null;

        public RMSPExplorerSiteLevelCache SiteLevelCache
        {
            get
            {
                if (null == mSiteLevelCache)
                {
                    mSiteLevelCache = ScanDataCache.Instance.SiteLevelCache;
                }
                return mSiteLevelCache;
            }
        }
        #endregion

        #region Public Methods
        public RecordsSharePointScanDiscoverNodeWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs, bool justEstimateListCount) : base(jobSettings, paraConfig, dependencyObjs)
        {
            manualUtil = new ManualUtil(paraConfig);
        }

        public override bool NeedSkipCurrentRule(Rule rule)
        {
            bool needSkipCurrentRule = false;
            if (
                rule != null
                && (mJobSettings.TreeNode.SkipRemoveContentAndDestroyAction 
                    || (mJobSettings.TreeNode.GetTeamsNode()?.SkipRemoveContentAndDestroyAction ?? false) 
                    || mJobSettings.TreeNode.GetGroupNode().SkipRemoveContentAndDestroyAction)
                && !(rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(rule.spMoveOption.MoveDestination.SPUrl))//Move Rule
                && !(rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)//Export Only Rule
                &&
                (rule.KeepDataOption == (int)KeepDataOption.Delete
                || (rule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument
                || (rule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (rule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                || (rule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                || (rule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub) == (int)KeepDataOption.ArchiveAndLeaveStub)
                )
            {
                needSkipCurrentRule = true;
            }
            return needSkipCurrentRule;
        }

        internal override async Task<(Rule, bool)> CheckContainerRuleAsync(ArchiverNodeItem item)
        {
            Rule result = null;
            RMTermInfo termInfo = null;
            switch (item.Cache_NodeType)
            {
                case (int)CacheNodeType.List:
                    {
                        if (mRuleEngine.HasListCondition)
                        {
                            var discoverList = item.DiscoverSPObject as AveDiscoverList;
                            var list = discoverList.GetListObject();
                            termInfo = GetTermInfo(list.RootFolder.Properties);
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = engine.CheckListCriteria(list);
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the list {list.RootFolder.ServerRelativeUrl}");
                            }
                        }
                        break;
                    }

                case (int)CacheNodeType.SiteCollection:
                    {
                        if (mRuleEngine.HasSiteCollectionCondition)
                        {
                            var discoverSite = item.DiscoverSPObject as AveDiscoverSite;
                            var site = discoverSite.Site;
                            termInfo = GetTermInfo(site.RootWeb.Properties);
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = engine.CheckSiteCollectionCriteria(site);
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the site {site.Url}");
                            }
                        }
                        break;
                    }
                default:
                    {
                        //Container Web:
                        if (mRuleEngine.HasSiteCondition && item.Cache_NodeType > (int)CacheNodeType.SiteCollection && item.Cache_NodeType < (int)CacheNodeType.List)
                        {
                            var discoverWeb = item.DiscoverSPObject as AveDiscoverWeb;
                            var tmpWeb = discoverWeb.AveWeb;
                            if (!tmpWeb.IsRootWeb)
                            {
                                termInfo = GetTermInfo(tmpWeb.Properties);
                                var engine = BuildRuleManagementByTerm(termInfo);
                                if (engine != null)
                                {
                                    result = engine.CheckSiteCriteria(tmpWeb);
                                }
                                else
                                {
                                    mLog.Info($"No SP rules realted to the web {tmpWeb.ServerRelativeUrl}");
                                }
                            }
                        }
                        //add for RevIM folder rule
                        if (mRuleEngine.HasFolderCondition && item.Cache_NodeType > (int)CacheNodeType.List && item.Cache_NodeType < (int)CacheNodeType.Item)
                        {
                            if (item.SPNodeLevel == NodeLevel.RootFolder)
                            {
                                mLog.Info("Skip root Folder : " + item.FullPath);
                            }
                            else if (item.LibRowID == -1 || item.LibRowID == 0)
                            {
                                mLog.Info("Skip system folder : " + item.FullPath);
                            }
                            else
                            {
                                var discoverFolder = item.DiscoverSPObject as AveDiscoverFolder;
                                var tmpFolder = discoverFolder.AveFolder;
                                var aveItem = tmpFolder.Item;
                                termInfo = GetTermInfo(aveItem, aveItem.Fields);
                                var engine = BuildRuleManagementByTerm(termInfo);
                                if (engine != null)
                                {
                                    result = engine.CheckFolderCriteria(tmpFolder, false);
                                }
                                else
                                {
                                    mLog.Info($"No SP rules realted to the folder {item.FullPath}");
                                }
                            }
                        }
                        break;
                    }
            }
            var needProcessActionManualData = false;
            mLog.Info("Current container object:{0} fit rule name:{1}.", item.FullPath, result == null ? string.Empty : result.Name);
            if (result != null && NeedSkipCurrentRule(result))
            {
                mLog.Info("Current object:{0} fit rule:{1} and SkipRemoveContentAndDestroyAction is true.", item.FullPath, result.Name);
                try
                {
                    SendScanDetail(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, result.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "StorageOptimization_SkipRemoveContentAndDestroyAction");
                }
                catch (Exception e)
                {
                    mLog.Info($"Add details failed error {e}");
                }
                result = null;
            }
            else
            {
                needProcessActionManualData = await manualUtil.IsNeedProcessDataActionForManualAsync(config, result, item, termInfo);
                if (needProcessActionManualData && WrapperConfiguration.IsProcessApprovalDatasOnly)
                {
                    ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Success);
                }
                else
                {
                    ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Failed);
                }
            }
            return (result, !needProcessActionManualData);
        }

        public override async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RecordsSharePointScanDiscoverNodeWorker.ProcessContainer"))
            {
                mLog.Info(string.Format("begin to scan container. Type:{0}, Name:{1} ", item.Cache_NodeType.ToString(), item.Name));
                ProcessResult result = ProcessResult.Default;
                if (type == ProcessType.NoNeedProcess)
                {
                    TransmitToNextLayer(item);
                }
                else if (item.Parent != null && !string.IsNullOrEmpty(item.Parent.RuleId) && item.Parent.DoDelete)
                {
                    item.RuleId = item.Parent.RuleId;
                    item.DoDelete = item.Parent.DoDelete;
                    item.RulePolicyLevel = item.Parent.RulePolicyLevel;
                    TransmitToNextLayer(item);
                }
                else
                {
                    switch (item.Cache_NodeType)
                    {
                        case (int)CacheNodeType.List:
                            {
                                //1.List本身符合Rule，不处理List节点以下数据
                                if (IsSystemList(item))
                                {
                                    mLog.Info("This List is System List or it not base list which will be skip,list Name:{0},list Title:{1}.", item.Name, item.Title);
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                else
                                {
                                    var ruleResult = await ProcessContainerLevelNodeFitRuleAsync(item);
                                    var rule = ruleResult.Item1;
                                    if (rule != null)
                                    {
                                        if (ruleResult.Item2)
                                        {
                                            result = ProcessResult.SkipCurrentNode;
                                        }
                                        else
                                        {
                                            result = ProcessResult.FitRule;
                                        }
                                    }
                                    //List不符合Rule，且当前job没有low level rule，不处理List节点以下数据
                                    else if (!HasLowLevelRule(item))
                                    {
                                        result = ProcessResult.SkipCurrentNode;
                                    }
                                    //List不符合List Type Rule,不处理List节点以下数据
                                    else if (!ProcessListTypeRule(item))
                                    {
                                        result = ProcessResult.SkipCurrentNode;
                                    }
                                }
                            }
                            break;
                        case (int)CacheNodeType.SiteCollection:
                            {
                                //1.Site Collection本身符合Rule，不处理Site Collection节点以下数据
                                var ruleResult = await ProcessContainerLevelNodeFitRuleAsync(item);
                                var rule = ruleResult.Item1;
                                if (rule != null)
                                {
                                    if (ruleResult.Item2)
                                    {
                                        result = ProcessResult.SkipCurrentNode;
                                    }
                                    else
                                    {
                                        result = ProcessResult.FitRule;
                                    }
                                }
                                //2.Site Collection不符合Rule，且当前job没有low level rule，不处理Site Collection节点以下数据
                                else if (!HasLowLevelRule(item))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                break;
                            }
                        case (int)CacheNodeType.Web:
                            {
                                //1.Site本身符合Rule，不处理Site节点以下数据
                                var ruleResult = await ProcessContainerLevelNodeFitRuleAsync(item);
                                var rule = ruleResult.Item1;
                                if (rule != null)
                                {
                                    if (ruleResult.Item2)
                                    {
                                        result = ProcessResult.SkipCurrentNode;
                                    }
                                    else
                                    {
                                        result = ProcessResult.FitRule;
                                    }
                                }
                                //2.Site Collection Level Rule，且Site Collection不符合rule，且没有Low Level Rule,初步判断进不来，先保留此方法
                                else if (!(HasCurrentLevelRule(item) || HasLowLevelRule(item)))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                //3.只有Site Rule，且Site不符合Rule，不处理Site下List
                                else if (!HasLowLevelRule(item) && HasCurrentLevelRule(item))
                                {
                                    //The Lowest level rule is : web level
                                    result = ProcessResult.SkipListNode;
                                }
                            }
                            break;
                        case (int)CacheNodeType.WebApplication:
                            {
                                //TODO:Skip scan webapplication, maybe need to do is in Server-Side.
                                break;
                            }
                        default:
                            {
                                //1.SubSite/Folder本身符合Rule，不处理SubSite/Folder节点以下数据
                                var ruleResult = await ProcessContainerLevelNodeFitRuleAsync(item);
                                var rule = ruleResult.Item1;
                                if (rule != null)
                                {
                                    if (ruleResult.Item2)
                                    {
                                        result = ProcessResult.SkipCurrentNode;
                                    }
                                    else
                                    {
                                        result = ProcessResult.FitRule;
                                    }
                                }
                                //2.(初步判断进不来，先保留此方法)举例说明：
                                //2a.当前节点是Folder，进入判断逻辑：没有Folder Level Rule & 没有低级别Rule，只有List及以上Rule才能走到此逻辑.
                                //2b.当前节点是Site，进入判断逻辑：没有Site Level Rule & 没有低级别Rule，只有Site Collection Rule才能走到此逻辑.
                                else if (!(HasCurrentLevelRule(item) || HasLowLevelRule(item)))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                //3.
                                //3a.只有SiteRule，且Sub Site不符合Rule，不处理SubSite下List
                                //3b.只有FolderRule，且Folder不符合Rule，不处理Folder以下数据
                                else if (!HasLowLevelRule(item) && HasCurrentLevelRule(item))
                                {
                                    result = ProcessResult.Continue;
                                }
                            }
                            break;
                    }
                }
                mLog.Info(string.Format("finish to scan container. Type:{0}, Name:{1}, result:{2} ", item.Cache_NodeType.ToString(), item.Name, result.ToString()));
                return result;
            }
        }

        internal override async Task<Rule> CheckItemRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                Rule result = null;
                RMTermInfo termInfo = null;
                var actionDueDate = string.Empty;
                if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                {
                    var recordItem = ApprovedDatasSqliteHelper.GetByItemId(item.ID);
                    mLog.Info($"this item will not check rule {item.ID}");
                    if (recordItem != null && recordItem.Count > 0)
                    {
                        var tempResult = recordItem.FirstOrDefault();
                        mLog.Info($"this item will not check rule {item.ID},and the record not null,termid:{tempResult.TermId},ruleid:{tempResult.RuleId}");
                        var engine = BuildRuleManagementByTerm(new RMTermInfo() {UniqueId = tempResult.TermId });
                        result = engine.GetRuleFromRuleCollectionByRuleId(tempResult.RuleId.ToString());
                        if (result != null)
                        {
                            ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Success);
                        }
                        else
                        {
                            mLog.Info($"this item will not check rule {item.ID},ruleid:{tempResult.RuleId},can not find the rule in the rule result by rule id");
                        }
                    }
                    return result;
                }
                switch (item.ItemType)
                {
                    case ArchiverCommon.ItemType.DOCUMENT:
                        if (mRuleEngine.HasDocumentCondition)
                        {
                            var listItem = GetIAveListItem(item.DiscoverSPObject);
                            termInfo = GetTermInfo(listItem, listItem.Fields);
                            if ((termInfo == null || termInfo.UniqueId == Guid.Empty) && item.IsInheritContainerTerm)
                            {
                                mLog.Info($"No term was found for doc {item.ID}, try get term from container levels");
                                termInfo = GetTermInfoFromContainer(item);
                            }
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = engine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                                if(result != null)
                                {
                                    actionDueDate = engine.GetDueDisposalTime(listItem, result, item.DiscoverSPObject);
                                }
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the item {item.FullPath}");
                            }
                        }
                        break;

                    case ArchiverCommon.ItemType.ITEM_TYPE:
                        if (mRuleEngine.HasItemCondition)
                        {
                            var listItem = GetIAveListItem(item.DiscoverSPObject);
                            termInfo = GetTermInfo(listItem, listItem.Fields);
                            if ((termInfo == null || termInfo.UniqueId == Guid.Empty) && item.IsInheritContainerTerm)
                            {
                                mLog.Info($"No term was found for item {item.ID}, try get term from container levels");
                                termInfo = GetTermInfoFromContainer(item);
                            }
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = engine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the item {item.FullPath}");
                            }
                        }
                        break;
                    case ArchiverCommon.ItemType.DOCUMENT_VER:
                        if (mRuleEngine.HasDocVersionCondition)
                        {
                            var listItem = GetIAveListItem(item.DiscoverSPObject);
                            termInfo = GetTermInfo(listItem, listItem.Fields);
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the item {item.FullPath}");
                            }
                        }
                        break;

                    case ArchiverCommon.ItemType.ITEM_VERSION:
                        if (mRuleEngine.HasItemVersionCondition)
                        {
                            var listItem = GetIAveListItem(item.DiscoverSPObject);
                            termInfo = GetTermInfo(listItem, listItem.Fields);
                            var engine = BuildRuleManagementByTerm(termInfo);
                            if (engine != null)
                            {
                                result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                            }
                            else
                            {
                                mLog.Info($"No SP rules realted to the item {item.FullPath}");
                            }
                        }
                        break;
                    case ArchiverCommon.ItemType.ATTACHMENT:
                        if (mRuleEngine.HasAttachmentCondition)
                        {
                            //result = mRuleEngine.CheckAttachmentCriteria(item.Parent.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;
                    default:
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule);
                }
                if (result != null)
                {
                    string fitRuleName = result.Name;
                    result = CheckHoldOnlyOrRecord(item, result);
                }
                if (result != null && NeedSkipCurrentRule(result))
                {
                    mLog.Info("Current object:{0} fit rule:{1} and SkipRemoveContentAndDestroyAction is true.", item.ID, result.Name);
                    SendScanDetail(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, result.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, "StorageOptimization_SkipRemoveContentAndDestroyAction");
                    result = null;
                }
                else if (IrmLeaveStubListSkipHelper.ShouldSkipItem(config, item.SPList ?? mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList, result))
                {
                    mLog.Info(
                        "Skip item for leave-stub IRM restriction. ItemId:{0}, RuleId:{1}, RuleName:{2}, KeepDataOption:{3}, PolicyLevel:{4}.",
                        item.ID,
                        result.Id,
                        result.Name,
                        result.KeepDataOption,
                        result.PolicyLevel);
                    SendScanDetail(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, result.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Skipped, IrmLeaveStubListSkipHelper.SkipReportMessageKey);
                    result = null;
                }
                else if (item.ItemType == ArchiverCommon.ItemType.DOCUMENT || item.ItemType == ArchiverCommon.ItemType.ITEM_TYPE)
                {
                    if (config.SkipCheckManualWhenObjectNotMatchRule)
                    {
                        if ((result != null && result.IsManualApproval) || WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            var needProcessAction = await manualUtil.IsNeedProcessDataActionForManualAsync(config, result, item, termInfo, actionDueDate);
                            if (needProcessAction && WrapperConfiguration.IsProcessApprovalDatasOnly)
                            {
                                ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Success);
                            }
                            if (!needProcessAction)
                            {
                                result = null;
                                ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Failed);
                            }
                        }
                        else
                        {
                            //skip
                        }
                    }
                    else
                    {
                        var needProcessAction = await manualUtil.IsNeedProcessDataActionForManualAsync(config, result, item, termInfo, actionDueDate);
                        if (needProcessAction && WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Success);
                        }
                        if (!needProcessAction)
                        {
                            result = null;
                            ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Failed);
                        }
                    }
                }
                //1.Explore Hold文件默认可以执行Disposal Move
                //2.Explore Hold文件除Disposal Move外，其它Action都Skip处理
                //2019-10-30 RECO-3524 & RECO-3852 确定目前版本hold 只skip 删除行为，所以目前不在scan level控制，因为可能出现Export before remove
                //if (result != null)
                //{
                //    Guid recordID = ScheduleConfiguration.GetRecordId(new Guid(config.archiverMessage.ScheduledConfigs[0].SiteId), item.ID);
                //    //Guid itemPathMD5 = new Guid(HashCodeHelper.ToMD5HashCode((new Uri(item.SiteUrl).Scheme + @"://" + new Uri(item.SiteUrl).Authority + item.FullPath.Replace('\\', '/')).ToLowerInvariant()));
                //    if (
                //        config.isRAJob
                //        && config.explorerDao != null
                //        && config.explorerDao.ReadById(new Guid(config.archiverMessage.ScheduledConfigs[0].SiteId), recordID) != null
                //        && config.explorerDao.ReadById(new Guid(config.archiverMessage.ScheduledConfigs[0].SiteId), recordID).HoldStatus == true
                //        && config.Procedure == ScheduleProcedure.Scan
                //        && !(result.spMoveOption != null && result.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(result.spMoveOption.MoveDestination.SPUrl))
                //        )
                //    {
                //        if (string.IsNullOrEmpty(config.ScanReportDto.partSiteCollectionUrl))
                //        {
                //            config.ScanReportDto.partSiteCollectionUrl = new Uri(item.SiteUrl).Scheme + @"://" + new Uri(item.SiteUrl).Authority;
                //        }
                //        SendScanDetail("StorageOptimization_EXOExploreHoldFile", item.FullPath, config.JobId, item.Cache_NodeType, BackupRestoreStatus.Skipped);
                //        mLog.Info(string.Format("item is hold in record, skip this file: {0}", item.Name));
                //        result = null;
                //    }
                //}
                return result;
            }
        }

        #endregion

        #region Private Methods
        internal Rule CheckHoldOnlyOrRecord(ArchiverNodeItem item, Rule result)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckHoldOnlyOrRecord"))
            {
                string fitRuleName = result.Name;
                try
                {
                    //IAveList tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                    //1.Archiver配置文件能控制是否删除Declare文件
                    //2.Records Rule页面选项能控制是否删除Declare文件
                    //3.Records  Move Rule默认Move Declare文件
                    IAveListItem listItem = null;
                    if (item.DiscoverSPObject is AveDiscoverItem)
                    {
                        listItem = (item.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                    }
                    else if (item.Parent.DiscoverSPObject is AveDiscoverItem)
                    {
                        listItem = (item.Parent.DiscoverSPObject as AveDiscoverItem).CurrentItem;
                    }

                    if (ArchiverCommonStaticMethod.CheckIsHoldOnly(listItem))
                    {
                        mLog.Info($"Item {item.ID} is Hold Only, fit rule:{fitRuleName} but it is Hold Only, so skip it.");
                        return null;
                    }

                    if ((result.spMoveOption != null && result.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(result.spMoveOption.MoveDestination.SPUrl))
                        || RuleHelper.CheckArchiveOnlyRule(result))
                    {
                        mLog.Info($"Item {item.ID} is fit move or archive only rule");
                    }
                    else
                    {
                        bool includeDeclaredRecord = ScheduleConfiguration.IsDeleteRecord || result.DeleteRecords;
                        bool includeRecordLabel = result.IncludeDeleteRecordLabel;
                        if (includeDeclaredRecord && includeRecordLabel)
                        {
                            // Records Rule with option "Include Declared Records" and "Include Items with Locked Record Label".
                        }
                        else
                        {
                            if (!includeRecordLabel)
                            {
                                if (ArchiverCommonStaticMethod.IsHaveRecordLabel(listItem))
                                {
                                    mLog.Warn($"Item {item.ID} with record label, fit rule:{fitRuleName} with option \"Include Declared Records\", but not with option \"Include Items with Locked Record Label\"");
                                    result = null;
                                }
                            }
                            if (!includeDeclaredRecord)
                            {
                                if (ArchiverCommonStaticMethod.CheckisRecord(listItem))
                                {
                                    mLog.Warn($"Item {item.ID} is Record, fit rule:{fitRuleName} with option \"Include Items with Locked Record Label\", but not with option \"Include Declared Records\"");
                                    result = ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem) ? result : null;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("Check Record Error {0}", ex.ToString());
                    throw;
                }
                mLog.Info(string.Format("item {0} is fit rule:{1} after CheckHoldOnlyOrRecord result is:{2}.", item.ID, fitRuleName, result != null));
                return result;
            }
        }



        private RuleManagement BuildRuleManagementByTerm(RMTermInfo mTermInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.BuildRuleManagementByTerm"))
            {
                RuleManagement engine = null;
                RMRuleItemCollection rules = null;
                if (ScanDataCache.Instance.TermRuleMapping.TryGetValue(mTermInfo.UniqueId, out rules))
                {
                    var newRuleCollection = RebuldSPRules(rules);
                    if (newRuleCollection.Rules.Count == 0)
                    {
                        mLog.Info($"No SP rules realted to the term {mTermInfo.UniqueId}");
                    }
                    else
                    {
                        engine = new RuleManagement(newRuleCollection);
                    }
                }
                else
                {
                    mLog.Info($"BuildRuleManagementByTerm.TermRuleMapping does not contains Term UniqueId:{mTermInfo.UniqueId}.");
                }
                return engine;
            }
        }

        private RMTermInfo GetTermInfo(IAvePropertyBag properties)
        {
            var termInfo = new RMTermInfo();

            if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        private RMTermInfo GetTermInfo(Hashtable properties)
        {
            var termInfo = new RMTermInfo();

            if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        internal RMTermInfo GetTermInfo(IAveListItem item, IAveFieldCollection fields)
        {
            var termInfo = new RMTermInfo();
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.GetTermInfo"))
            {
                string bcsColumnName = SiteLevelCache.BCSColumnInternalName;
                if (string.IsNullOrWhiteSpace(bcsColumnName) && !string.IsNullOrWhiteSpace(SiteLevelCache.BCSColumnDisplayName))
                {
                    bcsColumnName = SiteLevelCache.BCSColumnDisplayName;
                }

                if (fields.ContainsField(bcsColumnName))
                {
                    var termObj = item[bcsColumnName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termInfo.UniqueId = new Guid(valueString[1]);
                            termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                        }
                        else
                        {
                            mLog.Info($"{item.Url} invalid term format:{valueString}");
                        }

                    }
                    else
                    {
                        mLog.Info($"GetTermInfo:{item.Url} contains BCSColumnInternalName:{bcsColumnName} but column value IsNullOrEmpty.");
                        var itemTaxonomyColumns = GetItemTaxonomyColumns(item, fields);
                        if (itemTaxonomyColumns.ContainsKey(bcsColumnName))
                        {
                            var termId = itemTaxonomyColumns[bcsColumnName].ToString();
                            mLog.Info($"The term uniqueId is {termId}");
                            termInfo.UniqueId = new Guid(termId);
                        }
                    }
                }
                else
                {
                    mLog.Info($"GetTermInfo:{item.Url} does not contains BCSColumnInternalName:{SiteLevelCache.BCSColumnInternalName}.");
                }
                return termInfo;
            }
        }

        internal RMTermInfo GetTermInfoFromContainer(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.LATPerformance.GetTermInfoFromContainer"))
            {
                var termInfo = new RMTermInfo();
                if (item.IsInheritContainerTerm && item.ContainerLevelTermId != Guid.Empty)
                {
                    termInfo.UniqueId = item.ContainerLevelTermId;
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.TryGetValue(item.ContainerLevelTermId, out var term)
                        ? term.Name
                        : string.Empty;
                }

                mLog.Info($"Item [{item.ID}] uses classification from parent container. termId: {termInfo.UniqueId}, termName: {termInfo.Name}");

                return termInfo;
            }
        }

        internal IAveListItem GetIAveListItem(object info)
        {
            if (info is IAveListItem)
            {
                return (IAveListItem)info;
            }
            else if (info is AveDiscoverItem)
            {
                return ((AveDiscoverItem)info).CurrentItem;
            }
            return null;
        }
        internal RuleCollection RebuldSPRules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                {
                    reOrder++;
                    var rule = rules.CommonRules.Rules[order];
                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        private static Hashtable GetItemTaxonomyColumns(IAveListItem item, IAveFieldCollection fields)
        {
            Hashtable columnCollectionOfInternalName = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (item != null)
            {
                foreach (var field in fields)
                {
                    try
                    {
                        string fieldTitle = field.Title.ToLower(CultureInfo.CurrentCulture);
                        switch (field.Type)
                        {
                            case AveFieldType.Invalid:
                                if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                {
                                    IAveTaxonomyField taxnomyField = field as IAveTaxonomyField;

                                    string internalName = field?.StaticName;
                                    mLog.Info($"The field internalName is {internalName}");

                                    //Get Term Path Method
                                    //RECO-11440
                                    object fieldValue = null;
                                    try
                                    {
                                        fieldValue = item[field.ID];
                                    }
                                    catch (Exception ie)
                                    {
                                        mLog.Warn(ie.ToString());
                                    }
                                    if (fieldValue == null)
                                    {
                                        string textFieldName = null;
                                        //Sometimes the TaxonomyField column has no value, and its associated hidden field needs to be used to get the value.
                                        try
                                        {
                                            if (string.Equals(field.TypeAsString, "TaxonomyFieldType", StringComparison.OrdinalIgnoreCase))
                                            {
                                                textFieldName = item.Fields.GetById((field as IAveTaxonomyField).TextField).InternalName;
                                                mLog.Info("Will get field value by TextField, textFieldName is :{0}", textFieldName);
                                                fieldValue = item[textFieldName];
                                            }
                                            else if (string.Equals(field.TypeAsString, "TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                            {
                                                //Since Record does not fully support multi-value TaxonomyFieldType, special handling is currently skipped.
                                                mLog.Warn("Skip special handling for TaxonomyFieldTypeMulti data.");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            mLog.Warn("get TaxonomyField column associated hidden column error: {0}", e.ToString());
                                        }
                                        if (fieldValue == null)
                                        {
                                            continue;
                                        }

                                    }
                                    if (!string.IsNullOrEmpty(internalName))
                                    { 
                                        columnCollectionOfInternalName[internalName] = Trim(GetFieldTermIdValue(fieldValue));
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(string.Format("Get the taxnomy metadata of item error.Field Name:{0} Field.ID:{1}.Exception:{2}", field.Title, field.ID, ex));
                    }
                }
            }
            return columnCollectionOfInternalName;
        }

        private static string GetFieldTermIdValue(object value)
        {
            try
            {
                if (value is Dictionary<string, object> || value.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
                {
                    try
                    {
                        var dic = ((Dictionary<string, object>)value);
                        if (dic != null && dic.ContainsKey("TermGuid"))
                        {
                            var termId = new Guid(dic["TermGuid"].ToString());
                            return termId.ToString();
                        }
                        else
                        {
                            mLog.Warn("Current FieldTermIdValue:{0} is null or does not ContainsKey TermGuid.", value.ToString());
                            return string.Empty;
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("Get Taxnomy Filed Value by Dictionary Error, {0}", e.ToString());
                    }
                }
                else if (value is IAveTaxonomyFieldValue)
                {
                    var taxValue = value as IAveTaxonomyFieldValue;
                    var termId = new Guid(taxValue.TermGuid);
                    return termId.ToString();
                }
                else if (!(value is string))
                {
                    mLog.Info("Get Taxnomy Filed Value Error, the value is :{0}", value.ToString());
                }
            }
            catch (Exception e)
            {
                mLog.Warn("Get Taxnomy Filed Value:{0} Error:{1}.", value == null ? string.Empty : value.ToString(), e.ToString());
            }
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] values = stringValue.Split(';');
                foreach (string key in values)
                {
                    var index = key.IndexOf('|');
                    if (index == 0)
                    {
                        continue;
                    }
                    if (index < 0)
                    {
                        continue;
                    }
                    else
                    {
                        return key.Substring(index + 1);
                    }
                }
            }
            else
            {
                mLog.Warn("Current FieldTermIdValue IsNullOrEmpty.");
                return string.Empty;
            }
            return string.Empty;
        }

        private static string Trim(string str, params char[] trimchars)
        {
            return string.IsNullOrEmpty(str) ? str : str.Trim(trimchars);
        }

        #endregion
    }
}

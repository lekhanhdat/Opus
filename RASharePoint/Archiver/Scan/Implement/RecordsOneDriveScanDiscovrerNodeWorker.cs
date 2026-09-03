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
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
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
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Implement
{
    public class RecordsOneDriveScanDiscovrerNodeWorker : RecordsSharePointScanDiscoverNodeWorker
    {
        #region Private fields
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ManualUtil manualUtil;
        public List<Record> OneDriveExplorerCache = new List<Record>();
        private ExplorerDao explorerDao = null;
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

        public RecordsOneDriveScanDiscovrerNodeWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs) : base(jobSettings, paraConfig, dependencyObjs, false)
        {
            manualUtil = new ManualUtil(paraConfig);
            explorerDao = new ExplorerDao(true);
        }

        public override bool NeedSkipCurrentRule(Rule rule)
        {
            bool needSkipCurrentRule = false;
            if (
                rule != null
                &&( mJobSettings.TreeNode.SkipRemoveContentAndDestroyAction || mJobSettings.TreeNode.GetGroupNode().SkipRemoveContentAndDestroyAction)
                && !(rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null && !string.IsNullOrEmpty(rule.spMoveOption.MoveDestination.SPUrl))//Move Rule
                && !(rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)//Export Only Rule
                &&
                (rule.KeepDataOption == (int)KeepDataOption.Delete
                || (rule.KeepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument
                || (rule.KeepDataOption & (int)KeepDataOption.Remove) == (int)KeepDataOption.Remove
                || (rule.KeepDataOption & (int)KeepDataOption.NotBackup) == (int)KeepDataOption.NotBackup
                || (rule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive
                || (rule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub)== (int)KeepDataOption.ArchiveAndLeaveStub))
            {
                needSkipCurrentRule = true;
            }
            return needSkipCurrentRule;
        }

        public void InitOneDriveItemTermInfoByListId(Guid siteId, Guid listId)
        {
            if (!config.OneDriveNullClassification)
            {
                if (OneDriveExplorerCache.Count == 0 || OneDriveExplorerCache.FirstOrDefault()?.ListId != listId)
                {
                    mLog.Info("Begin InitOneDriveItemTermInfoByListId::{0}.", listId);
                    OneDriveExplorerCache.Clear();

                    bool hasNext = false;
                    string currentContinuation = string.Empty;
                    int pageSize = 500;
                    do
                    {
                        var result = explorerDao.QueryDataWithoutTotal(currentContinuation, pageSize, out hasNext,
                                          s => s.SourceFlag == (int)SourceFlag.OneDrive
                                          && s.ScopeId == siteId
                                          && s.ListId == listId
                                          && (s.RecordStatus == 1 || s.RecordStatus == 4 || s.RecordStatus == 5 || s.RecordStatus == 9)
                                          );
                        foreach (var record in result.Item1)
                        {
                            OneDriveExplorerCache.Add(record);
                            if (OneDriveExplorerCache.Count >= 10000)
                            {
                                break;
                            }
                        }
                        currentContinuation = result.Item2;
                        hasNext = !string.IsNullOrEmpty(result.Item2);
                    } while (hasNext);
                    mLog.Info("End InitOneDriveItemTermInfoByListId::{0}.OneDriveExplorerCache:{1}.", listId, OneDriveExplorerCache.Count);
                }
                else
                {
                    mLog.Info("Current list already InitOneDriveItemTermInfoByListId:{0}.OneDriveExplorerCache:{1}.", listId, OneDriveExplorerCache.Count);
                }
            }
            else
            {
                mLog.Info("OneDriveNullClassification is true.");
            }
        }

        internal override async Task<(Rule, bool)> CheckContainerRuleAsync(ArchiverNodeItem item)
        {
            Rule result = null;
            return (result, false);
        }

        internal override async Task<Rule> CheckItemRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                Rule result = null;
                if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                {
                    var recordItem = ApprovedDatasSqliteHelper.GetByItemId(item.ID);
                    mLog.Info($"this item will not check rule {item.ID}");
                    if (recordItem != null && recordItem.Count > 0)
                    {
                        var tempResult = recordItem.FirstOrDefault();
                        mLog.Info($"this item will not check rule {item.ID},and the record not null,termid:{tempResult.TermId},ruleid:{tempResult.RuleId}");
                        if (tempResult.TermId == Guid.Empty)
                        {
                            mLog.Info($"this item will not check rule {item.ID},and the record not null,term id is null,will use mRuleEngine to get the rule,ruleid:{tempResult.RuleId}");
                            result = mRuleEngine.GetRuleFromRuleCollectionByRuleId(tempResult.RuleId.ToString());
                        }
                        else
                        {
                            var engine = BuildRuleManagementByTerm(tempResult.TermId);
                            result = engine.GetRuleFromRuleCollectionByRuleId(tempResult.RuleId.ToString());
                        }
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
                if (!config.OneDriveNullClassification)
                {
                    var explorerItem = OneDriveExplorerCache.Where(x => x.NodeId == item.ID).FirstOrDefault();
                    if (explorerItem == null && OneDriveExplorerCache.Count >= 10000)
                    {
                        explorerItem = explorerDao.QueryAll(r => r.NodeId == item.ID).FirstOrDefault();
                    }
                    switch (item.ItemType)
                    {
                        case ArchiverCommon.ItemType.DOCUMENT:
                            if (mRuleEngine.HasDocumentCondition && explorerItem != null && explorerItem.TermId != Guid.Empty)
                            {
                                var engine = BuildRuleManagementByTerm(explorerItem.TermId);
                                if (engine != null)
                                {
                                    result = engine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                                }
                                else
                                {
                                    mLog.Info($"No SP rules realted to the item.ItemFullPath:{item.FullPath}.ItemTermId:{explorerItem.TermId}.");
                                }
                            }
                            else
                            {
                                mLog.Info($"Current DOCUMENT TermId is Guid.Empty.ItemFullPath:{item.FullPath}.");
                            }
                            break;

                        case ArchiverCommon.ItemType.ITEM_TYPE:
                            if (mRuleEngine.HasItemCondition && explorerItem != null && explorerItem.TermId != Guid.Empty)
                            {
                                var engine = BuildRuleManagementByTerm(explorerItem.TermId);
                                if (engine != null)
                                {
                                    result = engine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                                }
                                else
                                {
                                    mLog.Info($"No SP rules realted to the item.ItemFullPath:{item.FullPath}.ItemTermId:{explorerItem.TermId}.");
                                }
                            }
                            else
                            {
                                mLog.Info($"Current Item TermId is Guid.Empty.ItemFullPath:{item.FullPath}.");
                            }
                            break;
                        case ArchiverCommon.ItemType.DOCUMENT_VER:
                            if (mRuleEngine.HasDocVersionCondition)
                            {
                                // var listItem = GetIAveListItem(item.DiscoverSPObject);
                                // var termInfo = GetTermInfo(listItem, listItem.Fields);
                                // var engine = BuildRuleManagementByTerm(termInfo);
                                // if (engine != null)
                                // {
                                //     result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                                // }
                                // else
                                // {
                                //     mLog.Info($"No SP rules realted to the item {item.FullPath}");
                                // }
                            }
                            break;

                        case ArchiverCommon.ItemType.ITEM_VERSION:
                            if (mRuleEngine.HasItemVersionCondition)
                            {
                                // var listItem = GetIAveListItem(item.DiscoverSPObject);
                                // var termInfo = GetTermInfo(listItem, listItem.Fields);
                                // var engine = BuildRuleManagementByTerm(termInfo);
                                // if (engine != null)
                                // {
                                //     result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                                // }
                                // else
                                // {
                                //     mLog.Info($"No SP rules realted to the item {item.FullPath}");
                                // }
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
                }
                else
                {
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
                                // var listItem = GetIAveListItem(item.DiscoverSPObject);
                                // var termInfo = GetTermInfo(listItem, listItem.Fields);
                                // var engine = BuildRuleManagementByTerm(termInfo);
                                // if (engine != null)
                                // {
                                //     result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                                // }
                                // else
                                // {
                                //     mLog.Info($"No SP rules realted to the item {item.FullPath}");
                                // }
                            }
                            break;

                        case ArchiverCommon.ItemType.ITEM_VERSION:
                            if (mRuleEngine.HasItemVersionCondition)
                            {
                                // var listItem = GetIAveListItem(item.DiscoverSPObject);
                                // var termInfo = GetTermInfo(listItem, listItem.Fields);
                                // var engine = BuildRuleManagementByTerm(termInfo);
                                // if (engine != null)
                                // {
                                //     result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                                // }
                                // else
                                // {
                                //     mLog.Info($"No SP rules realted to the item {item.FullPath}");
                                // }
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
                    try
                    {
                        var needProcessAction = await manualUtil.IsNeedProcessDataActionForManualAsync(config, result, item, null);
                        if (needProcessAction && WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Success);
                        }
                        if (!needProcessAction)
                        {
                            ApprovedDatasSqliteHelper.UpdateStatus(item.ID, (int)ProcessedStatus.Failed);
                            result = null;
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("RM_MA_SiteOwner_NoSiteOwner"))
                        {
                            SendScanDetail(config.GetNodeFullPath(item.FullPath), 0, item.Cache_NodeType, result?.Name, Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed, "RM_MA_SiteOwner_NoSiteOwner");
                        }
                        throw;
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

        private RuleManagement BuildRuleManagementByTerm(Guid mTermUniqueId)
        {
            RuleManagement engine = null;
            RMRuleItemCollection rules = null;
            if (ScanDataCache.Instance.TermRuleMapping.TryGetValue(mTermUniqueId, out rules))
            {
                var newRuleCollection = RebuldSPRules(rules);
                if (newRuleCollection.Rules.Count == 0)
                {
                    mLog.Info($"No SP rules realted to the term {mTermUniqueId}");
                }
                else
                {
                    engine = new RuleManagement(newRuleCollection);
                }
            }
            return engine;
        }
    }
}

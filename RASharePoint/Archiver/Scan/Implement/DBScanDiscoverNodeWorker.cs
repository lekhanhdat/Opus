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
using AvePoint.GCommon.Contract.Tree.Object;
//using Microsoft.SharePoint;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Wrapper.Discovery;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Discover;
using Azure.ResourceManager.Resources;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.Common.RMRuleManagement;

namespace AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan
{
    public class DBScanDiscoverNodeWorker : IDiscoverNodeWorker
    {
        #region Private Fields
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IBackwardDependencyNodeCache<ArchiveApproveReport> backupNodeCache;
        private IBackwardDependencyNodeCache<object> mDependencyObjs;


        private List<int> systemListTable = new List<int>();
        private RuleManagement mRuleEngine;
        internal IBackwardDependencyNodeCache<ArchiveApproveReport> mApprovalReportProxy;
        internal ScheduleConfiguration mConfig = null;
        internal ScanJobSettings mJobSettings = null;
        private List<FilterPolicy> FilterPolicyCollection { get; set; }
        private Dictionary<string, RuleNodeContract> breakInheritNodes;
        private string siteUrl = string.Empty;
        private Guid webId = Guid.Empty;
        private Guid listId = Guid.Empty;
        #endregion

        #region Public properties
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
        public int DiscoverCacheNodeType { get; set; }
        #endregion

        #region Constructor
        public DBScanDiscoverNodeWorker(IBackwardDependencyNodeCache<ArchiveApproveReport> mBackupNodeCache,ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs)
        {
            backupNodeCache = mBackupNodeCache;
            mJobSettings = jobSettings;
            mConfig = paraConfig;
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(
                new ApprovalReportService(mConfig));
            mDependencyObjs = dependencyObjs;
            systemListTable = ScheduleConfiguration.ListTemplate;
        }

        #endregion

        #region Public Functions

        public void Init(object obj)
        {
            RuleNodeContract nodeContract = obj as RuleNodeContract;
            this.breakInheritNodes = nodeContract.BreakInheritNodesEncryptBySha1;
            this.RuleEngine = new RuleManagement(mConfig.RuleCollection);
            if (nodeContract.NodeLevel == NodeLevel.SiteCollection)
            {
                this.RuleEngine.IsCGArchiver = true;
            }
        }

        public bool IsRuleBreakInheritNode(string md5URL)
        {
            return false;
        }

        public void SendScanDetail(string errorMessage, string srcURL, string subJobId, int cacheNodeType, BackupRestoreStatus status)
        {

        }
        private Rule CheckContainerRule(ArchiverNodeItem item)
        {
            Rule result = null;
            switch (item.Cache_NodeType)
            {
                case (int)CacheNodeType.List:
                    {
                        if (mRuleEngine.HasListCondition)
                        {
                            result = mRuleEngine.CheckListCriteria(item.DiscoverSPObject);
                        }
                        break;
                    }

                case (int)CacheNodeType.SiteCollection:
                    {
                        if (mRuleEngine.HasSiteCollectionCondition)
                        {
                            result = mRuleEngine.CheckSiteCollectionCriteria(item.DiscoverSPObject);
                        }
                        break;
                    }
                default:
                    {
                        //Container Web:
                        if (mRuleEngine.HasSiteCondition && item.Cache_NodeType > (int)CacheNodeType.SiteCollection && item.Cache_NodeType < (int)CacheNodeType.List)
                        {
                            IAveWeb tmpWeb = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.Web) as IAveWeb;
                            if (!tmpWeb.IsRootWeb)
                            {
                                result = mRuleEngine.CheckSiteCriteria(item.DiscoverSPObject);
                            }
                            else
                            {
                                result = null;
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
                                result = mRuleEngine.CheckFolderCriteria(item.DiscoverSPObject, false);
                            }
                        }
                        break;
                    }
            }
            mLog.Info("Current container object:{0} fit rule name:{1}.", item.FullPath, result == null ? string.Empty : result.Name);
            return result;
        }

        #endregion 
        private bool IsSystemList(ArchiverNodeItem item)
        {
            bool result = false;
            if (item.Cache_NodeType == (int)CacheNodeType.List)
            {
                result = item.IsSystemObject;
                if (!result)
                {
                    IAveList tmpList = mDependencyObjs.ValueInCacheOfLevel((int)CacheNodeType.List) as IAveList;
                    if (tmpList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase))
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARScanDiscoverNodeWorkerIsSystemListWarn);
                        result = true;
                    }
                    else
                    {
                        if (tmpList.BaseTemplate == AveListTemplateType.PreservationHoldLibrary)
                        {
                            mLog.Info("This List is PreservationHoldLibrary,Hidden:{0},list Title:{1},list Template:{2}.", tmpList.Hidden.ToString(), tmpList.Title, tmpList.BaseTemplate.ToString());
                            return false;
                        }

                        result = (tmpList.Hidden || tmpList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase)) || (!tmpList.AllowDeletion && !systemListTable.Contains((int)tmpList.BaseTemplate));
                        if (result == true)
                        {
                            mLog.Info("This List may be Hidden or System Folder or not in BaseTemplate,Hidden:{0},list Title:{1},list Template:{2}.", tmpList.Hidden.ToString(), tmpList.Title, tmpList.BaseTemplate.ToString());
                        }
                    }
                }
            }
            return result;
        }
        private void ProcessContainerCheckResultNode(Rule result, ref ArchiverNodeItem item)
        {
            if (result != null)
            {
                item.ShouldDoArchive = item.ArchiveLevel = true;
                item.ApproveStatus = true;
                item.RuleId = result.Id;
                item.RuleName = result.Name;
            }
        }
        private void UpdateProgress(ArchiverNodeItem item)
        {
            if (item.Cache_NodeType >= (int)CacheNodeType.List && item.Cache_NodeType < (int)CacheNodeType.Item)
            {
                mConfig.ProgressDto.UpdateProgress();
            }
        }
        private bool ProcessContainerLevelNodeWithRule(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainerLevelNodeWithRule"))
            {
                Rule rule = null;
                ///*system list not to check rule*/            
                if (!IsSystemList(item))
                {
                    rule = CheckContainerRule(item);
                    if ((item.Parent != null && item.Parent.ShouldDoArchive) || (rule != null && rule.Id == mConfig.currentRule.Id))
                    {
                        ProcessContainerCheckResultNode(rule, ref item);
                        UpdateProgress(item);
                        var containerApprove = item.ConvertToArchiveApproveReport();
                        containerApprove.DoDelete = true;
                        backupNodeCache.PutIn(containerApprove, item.Cache_NodeType, item.ShouldDoArchive);
                    }
                    else
                    {
                        rule = null;
                        UpdateProgress(item);
                        backupNodeCache.PutIn(item.ConvertToArchiveApproveReport(), item.Cache_NodeType, item.ShouldDoArchive);
                    }
                    return rule != null;
                }
                else
                {
                    //System List must return true. Because we will not scan items under system list.
                    mLog.Info("This List is System List or it not base list which will be skip,list Name:{0},list Title:{1}.", item.Name, item.Title);
                    return true;
                }
            }
        }

        private Rule CheckHoldOnlyOrRecord(ArchiverNodeItem item, Rule result)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RecordsOneDriveScanDiscovrerNodeWorker.CheckHoldOnlyOrRecord"))
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
                                    result = null;
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
        private async Task<Rule> CheckItemRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                mRuleEngine.CurrentRuleId = mConfig.currentRule.Id;
                Rule result = null;
                switch (item.ItemType)
                {
                    case RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT:
                        if (mRuleEngine.HasDocumentCondition)
                        {
                            result = mRuleEngine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        break;

                    case RA.SharePoint.ArchiverCommon.ItemType.ITEM_TYPE:
                        if (mRuleEngine.HasItemCondition)
                        {
                            result = mRuleEngine.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        break;
                    case RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER:
                        if (mRuleEngine.HasDocVersionCondition)
                        {
                            result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;

                    case RA.SharePoint.ArchiverCommon.ItemType.ITEM_VERSION:
                        if (mRuleEngine.HasItemVersionCondition)
                        {
                            result = mRuleEngine.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        break;
                    case RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT:
                        if (mRuleEngine.HasAttachmentCondition)
                        {
                            result = mRuleEngine.CheckAttachmentCriteria(item.Parent.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
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
                return result;
            }
        }
        private void ProcessItemCheckResultNode(Rule rule, ref ArchiverNodeItem item, ArchiverNodeItem parent)// to do unit test
        {
            if (rule != null)
            {
                item.ShouldDoArchive = item.ArchiveLevel = true;
                item.RuleId = rule.Id;
                item.RuleName = rule.Name;
            }
            else if (parent.ShouldDoArchive)
            {
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
        private Rule CheckRealFitRule(Rule resultRule, ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("RecordsOneDriveScanDiscovrerNodeWorker.ProcessRegularItem.CheckRealFitRule"))
            {
                Rule realFitRule = resultRule;
                //For version and Attachment, we will check parent rule.
                if (item.ShouldDoArchive && (item.Cache_NodeType.Equals(10001) || item.Cache_NodeType.Equals(20000)))
                {
                    Rule parentLevelRule = null;
                    //ADO-155745 Discussion contenttype attachment's parent object is folder. 
                    if (item.Parent.Cache_NodeType >= (int)CacheNodeType.Item)
                    {
                        parentLevelRule = mConfig.RuleCollection.Values.Where(x => x.Id == item.Parent.RuleId).FirstOrDefault();
                    }
                    //if current level do not meet rule, we will set parentLevelRule to it.
                    realFitRule = resultRule ?? parentLevelRule;
                    //ADO-162640 order1 document version manual approve ,order2 document archive rule ,会导致version 不备份直接删除丢数据的情况。此种情况，让Version 符合Item rule 即可
                    if ((parentLevelRule != null && (!parentLevelRule.IsManualApproval || mConfig.AutoApproval)) && (resultRule != null && (resultRule.IsManualApproval || !mConfig.AutoApproval)))
                    {
                        mLog.Info("Parent rule is not Manual approve but Current rule is Manual approve,So current rule will be the same as parent rule.item Name: {0},UIVersion:{1}", item.Name, item.UIVersion);
                        realFitRule = parentLevelRule;
                        ProcessItemCheckResultNode(resultRule, ref item, parent);
                    }
                }
                return realFitRule;
            }
        }

        public async Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RecordsOneDriveScanDiscovrerNodeWorker.RealProcessItem"))
            {
                mLog.Info(string.Format("begin to scan item, id :{0}.UIVersion:{1}.", item.LibRowID, item.UIVersion));
                ProcessResult result = ProcessResult.Default;
                Rule resultRule = null;
                //System Item not to check rule
                if (item.IsSystemObject)
                {
                    return ProcessResult.SkipCurrentNode;
                }

                resultRule = await CheckItemRuleAsync(item);

                ProcessItemCheckResultNode(resultRule, ref item, parent);
                Rule realFitRule = CheckRealFitRule(resultRule, item, parent);
                //修改为最初的方法，来保证Test run Version rule 能统计出Current
                //TransmitToNextLayer(item);
                //if ((realFitRule == null&&item.ShouldDoArchive)||realFitRule.Id == mConfig.currentRule.Id)
                if (parent.ShouldDoArchive || (realFitRule != null && realFitRule.Id == mConfig.currentRule.Id))
                {
                    var itemApprove = item.ConvertToArchiveApproveReport();
                    if (parent.ShouldDoArchive)
                    {
                        itemApprove.ArchiveLevel = (int)SPNodeLevel.FitParentRule;
                    }
                    itemApprove.DoDelete = true;
                    backupNodeCache.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                    JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig?.currentRule);
                }
                else
                {
                    //不符合Rule或者符合其他rule时，更新数据的Status为Skip
                    //CG DB只有Current，没有Version记录，因此此处只需要更新Current
                    if (item.Cache_NodeType == (int)CacheNodeType.Item)
                    {
                        CGDBReader dbReader = CGDBReader.GetInstance(mConfig.ArchiverExtendSetting, mConfig.SiteCollectionID.ToString(), mConfig.SiteCollectionUrl);
                        dbReader.UpdateStatus(mConfig.SiteCollectionID.ToString(), item.ID, BackupRestoreStatus.Skipped);
                        mLog.Error($"DBScan RealProcessItem item not fit rule ID:{item.ID}.Path:{item.FullPath} and will update CGDB Skipped status.");
                    }
                    result = ProcessResult.SkipCurrentNode;
                }
                mLog.Info(string.Format("finish to scan item, Id:{0}.UIVersion:{1}.", item.LibRowID, item.UIVersion));
                return result;
            }
        }
        
        private void TransmitToNextLayer(ArchiverNodeItem item)
        {
            var containerApprove = item.ConvertToArchiveApproveReport();
            containerApprove.DoDelete = true;
            backupNodeCache.PutIn(containerApprove, item.Cache_NodeType, item.ShouldDoArchive);
        }
        public bool HasCurrentLevelRule(ArchiverNodeItem item)// to do unit test
        {
            return mRuleEngine.HaveCurrentLevelRule((int)item.Cache_NodeType);
        }
        private bool ProcessListTypeRule(ArchiverNodeItem item)
        {
            return mRuleEngine.CheckListType(item.DiscoverSPObject);
        }
        private bool HasLowLevelRule(ArchiverNodeItem item)// to do unit test
        {
            return mRuleEngine.HasLowerLevelRule((int)item.Cache_NodeType);
        }
        public async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainer"))
            {
                mLog.Info(string.Format("begin to scan container. Type:{0}, Name:{1} ", item.Cache_NodeType.ToString(), item.Name));
                ProcessResult result = ProcessResult.Default;
                if (type == ProcessType.NoNeedProcess)
                {
                    TransmitToNextLayer(item);
                }
                else
                {
                    switch (item.Cache_NodeType)
                    {
                        case (int)CacheNodeType.List:
                            {
                                listId = item.ID;
                                if (!ProcessContainerLevelNodeWithRule(item) && !HasLowLevelRule(item) && !item.Parent.ShouldDoArchive && !ProcessListTypeRule(item))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                            }
                            break;
                        case (int)CacheNodeType.SiteCollection:
                            {
                                siteUrl = item.FullPath;
                                if (!ProcessContainerLevelNodeWithRule(item) && !HasLowLevelRule(item))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                break;
                            }
                        case (int)CacheNodeType.Web:
                            {
                                webId = item.ID;
                                if (!ProcessContainerLevelNodeWithRule(item) && !item.Parent.ShouldDoArchive && !(HasCurrentLevelRule(item) || HasLowLevelRule(item)))
                                {
                                    //Fit With Rule
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                else if (!HasLowLevelRule(item) && HasCurrentLevelRule(item))
                                {
                                    //The Lowest level rule is : web level
                                    break;
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
                                if (!ProcessContainerLevelNodeWithRule(item) && !item.Parent.ShouldDoArchive && !(HasCurrentLevelRule(item) || HasLowLevelRule(item)))
                                {
                                    result = ProcessResult.SkipCurrentNode;
                                }
                                else if (!HasLowLevelRule(item) || HasCurrentLevelRule(item))
                                {
                                    break;
                                }
                            }
                            break;
                    }
                }
                mLog.Info(string.Format("finish to scan container. Type:{0}, Name:{1}, result:{2} ", item.Cache_NodeType.ToString(), item.Name, result.ToString()));
                return result;
            }
        }
        public void Dispose()
        {
            Console.WriteLine("the method is not implemented");
        }

        public void Flush()
        {
            backupNodeCache.Flush();
        }
    }
}
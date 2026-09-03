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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using Azure.ResourceManager.Resources;
using FluentFTP.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LOGRESOURCE = Merged18NResources.Archive.Archive;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Implement
{
    public class DiscoverNodeWorkerBase : IDiscoverNodeWorker
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, RuleNodeContract> breakInheritNodes;
        internal IBackwardDependencyNodeCache<ArchiveApproveReport> mApprovalReportProxy;
        internal ScheduleConfiguration config = null;
        internal ScanJobSettings mJobSettings = null;        
        internal List<int> systemListTable = new List<int>();
        internal IBackwardDependencyNodeCache<object> mDependencyObjs;
        internal RuleManagement mRuleEngine;
        internal List<Contract.JobMonitor.JobType> needCheckKeepVersionJobTypes = 
            [
                Contract.JobMonitor.JobType.RMArchiverBackup,
                Contract.JobMonitor.JobType.TeamsArchiverBackup
            ];
        public Dictionary<string, RuleNodeContract> BreakInheritNodes
        {
            get { return breakInheritNodes; }
            set { breakInheritNodes = value; }
        }

        public DiscoverNodeWorkerBase(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs)
        {
            mJobSettings = jobSettings;
            config = paraConfig;
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(
                new ApprovalReportService(config));
            mDependencyObjs = dependencyObjs;
            systemListTable = ScheduleConfiguration.ListTemplate;
        }

        public void Dispose()
        {
            using (mApprovalReportProxy) { }
        }

        public void Init(object obj)
        {
            RuleNodeContract nodeContract = obj as RuleNodeContract;
            this.breakInheritNodes = nodeContract?.BreakInheritNodesEncryptBySha1;
            this.RuleEngine = new RuleManagement(config.RuleCollection);
            this.RuleEngine.ForceFitTeamsRuleID = config.ForceFitTeamsRuleID;
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                this.breakInheritNodes?.Clear();
            }
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
            return breakInheritNodes != null && breakInheritNodes.ContainsKey(sha1URL);
        }

        public virtual async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainer"))
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
                                else if (await ProcessContainerLevelNodeReportSizeAsync(item))
                                {
                                    result = ProcessResult.FitRule;
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
                            break;
                        case (int)CacheNodeType.SiteCollection:
                            {
                                //1.Site Collection本身符合Rule，不处理Site Collection节点以下数据
                                if (await ProcessContainerLevelNodeReportSizeAsync(item))
                                {
                                    result = ProcessResult.FitRule;
                                    if (IsStoreInM365SCScanAction(item))
                                    {
                                        result = ProcessResult.SkipCurrentNode;
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
                                if (await ProcessContainerLevelNodeReportSizeAsync(item))
                                {
                                    result = ProcessResult.FitRule;
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
                                if (await ProcessContainerLevelNodeReportSizeAsync(item))
                                {
                                    result = ProcessResult.FitRule;
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
                                    result = ProcessResult.SkipListNode;
                                }
                            }
                            break;
                    }
                }
                mLog.Info(string.Format("finish to scan container. Type:{0}, Name:{1}, result:{2} ", item.Cache_NodeType.ToString(), item.Name, result.ToString()));
                return result;
            }
        }

        private bool IsStoreInM365SCScanAction(ArchiverNodeItem item)
        {
            if (item.Cache_NodeType != (int)CacheNodeType.SiteCollection)
                return false;

            var rule = config.RuleCollection.Values
                .FirstOrDefault(r => r.Id == item.RuleId);

            if (rule == null)
                return false;

            return (rule.KeepDataOption & (int)KeepDataOption.TriggerMicrosoft365Archiving) == (int)KeepDataOption.TriggerMicrosoft365Archiving
                   && rule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.SiteCollection;
        }

        public virtual Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            return RealProcessItemAsync(item, parent);
        }

        public void SendScanDetail(string srcURL, long nodeSize, int level, string rulename, JobDetailsStatus status, string errorMessage = "")
        {
            config.JobReportDto.AddScanReport(srcURL, nodeSize, level, rulename, status, errorMessage);
            //config.ScanReportDto.AddScanReport(0, 0, srcURL, status, cacheNodeType, subJobId, string.Empty, errorMessage);
        }

        public void Flush()
        {
            mApprovalReportProxy.Flush();
        }

        public virtual bool NeedSkipCurrentRule(Rule rule)
        {
            return false;
        }

        public virtual bool IsSystemList(ArchiverNodeItem item)
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

        public async Task<ProcessResult> RealProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.RealProcessItem"))
            {
                mLog.Info(string.Format("begin to scan item, ID:{0}.", item.ID));
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
                        item.RuleArchiverAction = config.GetRuleArchiverActionString(rule);
                        item.ForcedReport = true;
                    }
                    if (TransmitToNextLayer(item))
                    {
                        var effectiveRule = config.IsOneDriverSite && rule.OneDriveRule is not null ? rule.OneDriveRule : rule;
                        JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, item.ConvertToArchiveApproveReport(), effectiveRule);
                    }
                    return ProcessResult.FitParentRule;
                }
                else if (item.IsSystemObject)
                {
                    return ProcessResult.SkipCurrentNode;
                }

                resultRule = await CheckItemRuleAsync(item);
                ProcessItemCheckResultNode(resultRule, ref item, parent);
                Rule realFitRule = CheckRealFitRule(resultRule, item, parent);

                if(!CheckWhetherSkipDocumentForPreScanJob(item, realFitRule))
                {
                    if (TransmitToNextLayer(item))
                    {
                        var effectiveRule = config.IsOneDriverSite && realFitRule.OneDriveRule is not null ? realFitRule.OneDriveRule : realFitRule;
                        JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, item.ConvertToArchiveApproveReport(), effectiveRule);
                    }
                }

                mLog.Info(string.Format("finish to scan item, id:{0}.", item.ID));
                return result;
            }
        }

        private bool CheckWhetherSkipDocumentForPreScanJob(ArchiverNodeItem item, Rule fitRule)
        {
            if ((this.config.JobId.StartsWith("SAN") || this.config.jobtype == Contract.JobMonitor.JobType.TeamsPreScan)
                && fitRule != null
                && HasKeepLatestVersionAndArhiveOthersOption(fitRule)
                && item.ItemType == ArchiverCommon.ItemType.DOCUMENT)
            {
                mLog.Info($"Skip report document current version, because has KeepLatestVersionAndArhiveOthers Option.");
                return true;
            }
            return false;
        }
        private bool HasKeepLatestVersionAndArhiveOthersOption(Rule fitRule)
        {
            if (this.config.IsOneDriverSite)
            {
                fitRule = fitRule.OneDriveRule;
            }
            return (fitRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersionAndArhiveOthers) == (int)KeepDataOption.KeepLatestVersionAndArhiveOthers
                || (fitRule.KeepDataOption & (int)KeepDataOption.KeepLatestVersion) == (int)KeepDataOption.KeepLatestVersion;
        }

        internal bool TransmitToNextLayer(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.TransmitToNextLayer"))
            {
                mApprovalReportProxy.PutIn(item.ConvertToArchiveApproveReport(), item.Cache_NodeType, item.ShouldDoArchive);
                return item.ShouldDoArchive;
            }
        }
        

        internal async Task<bool> ProcessContainerLevelNodeWithRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainerLevelNodeWithRule"))
            {
                var fitRule = false;
                Rule rule = null;

                var result = await CheckContainerRuleAsync(item);
                rule = result.Item1;
                string oldRuleID = string.Empty;
                fitRule = rule != null;
                if (fitRule)
                {
                    item.RulePolicyLevel = (int)rule.PolicyLevel;
                    item.DoDelete = true;
                }
                ProcessContainerCheckResultNode(rule, result.Item2, ref item);
                TransmitToNextLayer(item);
                return fitRule;
            }
        }

        internal async Task<bool> ProcessContainerLevelNodeReportSizeAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainerLevelNodeWithRule"))
            {
                var fitRule = false;
                Rule rule = null;
                if (mJobSettings.Configuration.jobtype == Contract.JobMonitor.JobType.ApprovalProcessArchive)
                {
                    ManualUtil util = new ManualUtil(mJobSettings.Configuration);
                    var record = util.ApprovaledRecord(item);
                    if (record != null)
                    {
                        AnalyseContainerSize(item);
                        item.DoDelete = true;
                        item.RuleId = record.RuleId.ToString();
                        item.RuleName = record.ManualRuleName;
                        //item.RulePolicyLevel = (int)rule.PolicyLevel;
                        item.ShouldDoArchive = item.ArchiveLevel = true;
                        item.ApproveStatus = true;
                        fitRule = true;
                    }
                    else
                    {
                        item.ShouldDoArchive = false;
                    }
                }
                else
                {
                    var result = await CheckContainerRuleAsync(item);
                    rule = result.Item1;
                    string oldRuleID = string.Empty;
                    fitRule = rule != null;
                    if (fitRule)
                    {
                        item.RulePolicyLevel = (int)rule.PolicyLevel;
                        item.DoDelete = true;
                        AnalyseContainerSize(item);
                    }
                    ProcessContainerCheckResultNode(rule, result.Item2, ref item);
                }
                TransmitToNextLayer(item);
                return fitRule;
            }
        }

        /// <summary>
        /// Process Container Level Node Fit Rule
        /// </summary>
        /// <param name="item"></param>
        /// <returns>(Fit Rule,Skip Manual Data)</returns>
        internal async Task<(Rule, bool)> ProcessContainerLevelNodeFitRuleAsync(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.ProcessContainerLevelNodeFitRuleAsync"))
            {
                var fitRule = false;
                Rule rule = null;

                var result = await CheckContainerRuleAsync(item);
                rule = result.Item1;
                string oldRuleID = string.Empty;
                fitRule = rule != null;
                if (fitRule)
                {
                    item.RulePolicyLevel = (int)rule.PolicyLevel;
                    item.DoDelete = true;
                    AnalyseContainerSize(item);
                }
                ProcessContainerCheckResultNode(rule, result.Item2, ref item);
                TransmitToNextLayer(item);
                return (rule, result.Item2);
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

                if (result != null)
                {
                    result = CheckHoldOnlyOrRecord(item, result);
                }
                if (result != null && NeedSkipCurrentRule(result))
                {
                    mLog.Info("Current object:{0} fit rule:{1} and SkipRemoveContentAndDestroyAction is true.", item.ID, result.Name);
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

                // validate version count
                if (result != null && NeedSkipItemByVersion(item, result))
                {
                    result = null;
                }

                return result;
            }
        }

        public bool NeedSkipItemByVersion(ArchiverNodeItem item, Rule result)
        {
            // only SO job
            if (!needCheckKeepVersionJobTypes.Contains(config.jobtype))
            {
                return false;
            }

            // only check document
            if (item.ItemType != ArchiverCommon.ItemType.DOCUMENT)
            {
                return false;
            }

            // only check versions if option KeepLatestMajorAndMinorVersion/KeepLatestMajorAndMinorVersionAndArchiveOthers is selected
            if (!result.KeepDataOption.HasFlags(JoinTypes.Or, mRuleEngine.KeepLatestVersionsOptions))
            {
                return false;
            }

            IAveListItem listItem = null;
            if (item.DiscoverSPObject is IAveListItem)
            {
                listItem = item.DiscoverSPObject as IAveListItem;
            }
            else if (item.DiscoverSPObject is AveDiscoverItem)
            {
                listItem = (item.DiscoverSPObject as AveDiscoverItem).CurrentItem;
            }
            else if (item.Parent.DiscoverSPObject is AveDiscoverItem)
            {
                listItem = (item.Parent.DiscoverSPObject as AveDiscoverItem).CurrentItem;
            }

            var allVersions = listItem.Versions;
            var versionCount = allVersions.Count;
            var currentVersions = allVersions.Count(v => v.IsCurrentVersion);
            // if IncludeVersionForPerformance = false, listItem.Versions only return latest version, need get all previous version from listItem.File.Versions
            if (!WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance)
            {
                var allFileVersion = listItem.File.Versions; // it's not include latest version
                mLog.Debug($"Versions info of file:{item.ID}, count:{versionCount}. {string.Join(";", allFileVersion.Select(v => $"\nLabel:{v.VersionLabel}, VersionId:{v.ID}, IsCurrent:{v.IsCurrentVersion}"))}");
                versionCount += allFileVersion.Count;
                currentVersions += allFileVersion.Count(v => v.IsCurrentVersion);
            }

            var rule = config.IsOneDriverSite ? result.OneDriveRule : result;

            mLog.Debug($"Versions info of listItem:{item.ID}, count:{versionCount}, isOneDrive: {config.IsOneDriverSite}. {string.Join(";", allVersions.Select(v => $"\nLabel:{v.VersionLabel}, VersionId:{v.VersionId}, IsCurrent:{v.IsCurrentVersion}, Level: {v.Level}"))}");

            var keepVersionCount = rule.KeepLatestMajorAndMinorVersion == 0
                        ? rule.KeepLatestMajorAndMinorVersionAndArchiveOthers
                        : rule.KeepLatestMajorAndMinorVersion;

            var previousVersionsCount = versionCount - currentVersions;

            if (previousVersionsCount <= keepVersionCount)
            {
                mLog.Info($"Previous versions count <= configed keep version count, skip it. previousVersionsCount:{previousVersionsCount}, currentVersions:{currentVersions}, keepVersionCount:{keepVersionCount}, Item id:{item.ID}.");
                return true;
            }

            mLog.Info($"Previous versions count > configed keep version count, process it. previousVersionsCount:{previousVersionsCount}, currentVersions:{currentVersions}, keepVersionCount:{keepVersionCount}, Item id:{item.ID}.");
            return false;
        }

        internal void ProcessContainerCheckResultNode(Rule result, bool skipManualData, ref ArchiverNodeItem item)
        {
            if (result != null && !skipManualData)
            {
                item.ShouldDoArchive = item.ArchiveLevel = true;
                item.ApproveStatus = true;
                item.RuleId = result.Id;
                item.RuleName = result.Name;
                item.RuleArchiverAction = config.GetRuleArchiverActionString(result);
            }
        }

        /// <summary>
        /// Check Container Rule
        /// </summary>
        /// <param name="item"></param>
        /// <returns>(Fit Rule,Skip Manual Data)</returns>
        internal virtual async Task<(Rule, bool)> CheckContainerRuleAsync(ArchiverNodeItem item)
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
            return (result, false);
        }

        private Rule CheckRealFitRule(Rule resultRule, ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            //Due to the fixed Order based on Level,
            //The following cases will not occur,
            //order1 document version manual approve, order2 document archive rule
            using (AvePerformanceScope pc = new AvePerformanceScope("ProcessRegularItem.CheckRealFitRule"))
            {
                Rule realFitRule = resultRule;
                //For version and Attachment, we will check parent rule.
                if (item.ShouldDoArchive && (item.Cache_NodeType.Equals(10001) || item.Cache_NodeType.Equals(20000)))
                {
                    Rule parentLevelRule = null;
                    //ADO-155745 Discussion contenttype attachment's parent object is folder. 
                    if (item.Parent.Cache_NodeType >= (int)CacheNodeType.Item)
                    {
                        parentLevelRule = config.RuleCollection.Values.Where(x => x.Id == item.Parent.RuleId).FirstOrDefault();
                    }
                    //if current level do not meet rule, we will set parentLevelRule to it.
                    realFitRule = resultRule ?? parentLevelRule;
                    //ADO-162640 order1 document version manual approve ,order2 document archive rule ,会导致version 不备份直接删除丢数据的情况。此种情况，让Version 符合Item rule 即可
                    if ((parentLevelRule != null && (!parentLevelRule.IsManualApproval || config.AutoApproval)) && (resultRule != null && (resultRule.IsManualApproval || !config.AutoApproval)))
                    {
                        mLog.Info("Parent rule is not Manual approve but Current rule is Manual approve,So current rule will be the same as parent rule.item Name: {0},UIVersion:{1}", item.ID, item.UIVersion);
                        realFitRule = parentLevelRule;
                        ProcessItemCheckResultNode(resultRule, ref item, parent);
                    }
                }
                return realFitRule;
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
                item.RuleArchiverAction = config.GetRuleArchiverActionString(rule);
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

        public virtual bool ProcessListTypeRule(ArchiverNodeItem item)
        {
            return mRuleEngine.CheckListType(item.DiscoverSPObject);
        }

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
                    if (item.DiscoverSPObject is IAveListItem)
                    {
                        listItem = item.DiscoverSPObject as IAveListItem;
                    }
                    else if (item.DiscoverSPObject is AveDiscoverItem)
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

        //判断一个container下面是否有低级别rule
        internal bool HasCurrentLevelRule(ArchiverNodeItem item)// to do unit test
        {
            return mRuleEngine.HaveCurrentLevelRule((int)item.Cache_NodeType);
        }

        internal bool HasLowLevelRule(ArchiverNodeItem item)// to do unit test
        {
            return mRuleEngine.HasLowerLevelRule((int)item.Cache_NodeType);
        }

        internal void AnalyseContainerSize(ArchiverNodeItem item)
        {
            if (item.DiscoverSPObject is AveDiscoverSite)
            {
                StatisticSiteSize(item);
            }
            else if (item.DiscoverSPObject is AveDiscoverWeb)
            {
                StatisticWebSize(item);
            }
            else if (item.DiscoverSPObject is AveDiscoverList)
            {
                StatisticListSize(item);
            }
            else if (item.DiscoverSPObject is AveDiscoverFolder)
            {
                StatisticFolderSize(item);
            }
            else
            {
                mLog.Warn($"AnalyseContainerSize error, DiscoverSPObject {item.DiscoverSPObject.GetType().FullName}");
            }
        }

        private void StatisticSiteSize(ArchiverNodeItem site)
        {
            try
            {
                AveDiscoverSite dSite = site.DiscoverSPObject as AveDiscoverSite;
                config.JobReportDto.CalculateSize += dSite.Site.Size;
                site.DocumentSize = dSite.Site.Size;
                mLog.Info($"StorageMetrics TotalSize : {dSite.Site.Size} , TotleCalculateSize : {config.JobReportDto.CalculateSize}");
            }
            catch (Exception e)
            {
                mLog.Warn($"Get storage metrics failed:{e.ToString()}");
            }
        }

        private void StatisticWebSize(ArchiverNodeItem web)
        {
            try
            {
                var cWeb = web.DiscoverSPObject as AveDiscoverWeb;
                var cFolder = cWeb.AveWeb.RootFolder;
                long cFolderSize = 0;
                if (cFolder != null && cFolder.StorageMetrics != null)
                {
                    cFolderSize = cFolder.StorageMetrics.TotalSize;
                    web.DocumentSize = cFolderSize;
                    config.JobReportDto.CalculateSize += cFolderSize;
                }
                mLog.Info($"StorageMetrics TotalSize : {cFolderSize} , TotleCalculateSize : {config.JobReportDto.CalculateSize}");
            }
            catch (Exception e)
            {
                mLog.Warn($"Get storage metrics failed:{e.ToString()}");
            }
        }

        private void StatisticListSize(ArchiverNodeItem list)
        {
            try
            {
                var cList = list.DiscoverSPObject as AveDiscoverList;
                if (
                        list.SPList.BaseTemplate.ToString().Equals("ExternalList", StringComparison.OrdinalIgnoreCase)
                        || (list.SPList.Hidden
                        || list.SPList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase))
                        || (!list.SPList.AllowDeletion && !config.BackgroundSettings.ListTemplateTable.Contains((int)list.SPList.BaseTemplate))
                   )
                {
                    mLog.Info($"StatisticListSize:Skip External/Hidden/System Folder/NotAllowDeletion/NonBaseTemplate list.");
                    return;
                }
                else if (list.SPList != null && list.SPList.RootFolder != null && list.SPList.RootFolder.StorageMetrics != null)
                {
                    config.JobReportDto.CalculateSize += list.SPList.RootFolder.StorageMetrics.TotalSize;
                    list.DocumentSize = list.SPList.RootFolder.StorageMetrics.TotalSize;
                    mLog.Info($"StorageMetrics TotalSize : {list.SPList.RootFolder.StorageMetrics.TotalSize} , TotleCalculateSize : {config.JobReportDto.CalculateSize}");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Get storage metrics failed:{e.ToString()}");
            }
        }

        private void StatisticFolderSize(ArchiverNodeItem folder)
        {
            try
            {
                var cFolder = folder.DiscoverSPObject as AveDiscoverFolder;
                if (cFolder.AveFolder != null && cFolder.AveFolder.StorageMetrics != null)
                {
                    config.JobReportDto.CalculateSize += cFolder.AveFolder.StorageMetrics.TotalSize;
                    folder.DocumentSize = cFolder.AveFolder.StorageMetrics.TotalSize;
                    mLog.Info($"StorageMetrics TotalSize : {cFolder.AveFolder.StorageMetrics.TotalSize} , TotleCalculateSize : {config.JobReportDto.CalculateSize}");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Get storage metrics failed:{e.ToString()}");
            }
        }
    }
}

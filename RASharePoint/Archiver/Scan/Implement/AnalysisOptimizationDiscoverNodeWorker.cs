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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.GCommon;
using AvePoint.RA.Contract;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.Archiver.Common.RuleConverter;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;

namespace AvePoint.RA.SharePoint.Archiver.Scan.Implement
{
    public class AnalysisOptimizationDiscoverNodeWorker : DiscoverNodeWorkerBase
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal ScheduleConfiguration mConfig = null;
        internal ScanJobSettings mJobSettings = null;
        public PCContainer<ArchiveApproveReport> pcContainer = new PCContainer<ArchiveApproveReport>(1000);
        public IScheduleContainer<ArchiveApproveReport> ScheduleContainer = null;

        private DO2SORuleConverter mDO2SORuleConverter;
        private RuleManagement mRuleEngineForScope;
        private RuleManagement mRuleEngineForDocumentTag;
        private RuleManagement mRuleEngineForVersionTag;

        private RuleManagement RuleEngineForScope
        {
            get
            {
                if (mRuleEngineForScope == null)
                {
                    var rule = mDO2SORuleConverter.GetScopeDocumentRule();
                    Dictionary<int, Rule> Rules = new Dictionary<int, Rule>();
                    Rules.Add(1, rule);
                    mRuleEngineForScope = new RuleManagement(Rules);
                }
                return mRuleEngineForScope;
            }
        }

        private RuleManagement RuleEngineForDocumentTag
        {
            get
            {
                if (mRuleEngineForDocumentTag == null)
                {
                    var rules = mDO2SORuleConverter.GetDocumentTagRules();
                    Dictionary<int, Rule> Rules = new Dictionary<int, Rule>();
                    int i = 1;
                    foreach (var rule in rules)
                    {
                        Rules.Add(i, rule);
                        i++;
                    }
                    mRuleEngineForDocumentTag = new RuleManagement(Rules);
                }
                return mRuleEngineForDocumentTag;
            }
        }

        private RuleManagement RuleEngineForDocumentVersionTag
        {
            get
            {
                if (mRuleEngineForVersionTag == null)
                {
                    var rules = mDO2SORuleConverter.GetDocumentVersionTagRules();
                    Dictionary<int, Rule> Rules = new Dictionary<int, Rule>();
                    int i = 1;
                    foreach (var rule in rules)
                    {
                        Rules.Add(i, rule);
                        i++;
                    }
                    mRuleEngineForVersionTag = new RuleManagement(Rules);
                }
                return mRuleEngineForVersionTag;
            }
        }

        public AnalysisOptimizationDiscoverNodeWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs) : base(jobSettings, paraConfig, dependencyObjs)
        {
            ScheduleContainer = new BackupNodeCache(pcContainer);
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(ScheduleContainer);
            mConfig = paraConfig;
            mDO2SORuleConverter = new DO2SORuleConverter(mConfig.RMDiscoveryOptimizationSetting);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void Init(object obj)
        {
            throw new NotImplementedException();
        }

        public bool IsRuleBreakInheritNode(string sha1URL)
        {
            throw new NotImplementedException();
        }

        public override async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType withType)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.AnalysisOptimizationDiscoverNodeWorker.ProcessContainer"))
            {
                mLog.Info(string.Format("begin to scan container. Type:{0}, Name:{1} ", item.Cache_NodeType.ToString(), item.Name));
                ProcessResult result = ProcessResult.Default;
                ProcessContainerLevelNodeReportSizeAsync(item);
                mLog.Info(string.Format("finish to scan container. Type:{0}, Name:{1}, result:{2} ", item.Cache_NodeType.ToString(), item.Name, result.ToString()));
                return result;
            }
        }

        public override async Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.AnalysisOptimizationDiscoverNodeWorker.RealProcessItem"))
            {
                mLog.Info(string.Format("begin to scan item, id :{0}.UIVersion:{1}.", item.LibRowID, item.UIVersion));
                ProcessResult result = ProcessResult.Default;
                //System Item not to check rule


                if (item.IsSystemObject)
                {
                    return ProcessResult.SkipCurrentNode;
                }
                else
                {
                    var itemApprove = item.ConvertToArchiveApproveReport();
                    if (mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document)
                    {
                        if (item.ItemType == ArchiverCommon.ItemType.DOCUMENT)
                        {
                            if (mConfig.IsProcessDuplicateDatas)
                            {
                                item.ShouldDoArchive = true;
                                itemApprove.DoDelete = true;
                                mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                                JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
                            }
                            else
                            {
                                var scopeRule = CheckItemRuleForScope(item);
                                if (scopeRule != null)
                                {
                                    var resultRule = CheckItemRuleForDocumentTagRule(item);
                                    if (resultRule != null)
                                    {
                                        item.ShouldDoArchive = true;
                                        itemApprove.DoDelete = true;
                                        mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                                        JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
                                    }
                                    else
                                    {
                                        return ProcessResult.SkipCurrentNode;
                                    }
                                }
                                else
                                {
                                    return ProcessResult.SkipCurrentNode;
                                }
                            }
                        }
                        else
                        {
                            item.ShouldDoArchive = true;
                            itemApprove.DoDelete = true;
                            mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                            JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
                        }
                    }
                    else if (mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion)
                    {
                        if (item.ItemType == ArchiverCommon.ItemType.DOCUMENT)
                        {
                            var scopeRule = CheckItemRuleForScope(item);
                            if (scopeRule != null)
                            {
                                mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, false);
                                //JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove);
                            }
                            else
                            {
                                return ProcessResult.SkipCurrentNode;
                            }
                        }
                        else
                        {
                            var resultRule = CheckItemVersionRuleForROTAndInactive(item);
                            if (resultRule != null)
                            {
                                item.ShouldDoArchive = true;
                                itemApprove.DoDelete = true;
                                mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                                JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
                            }
                            else
                            {
                                return ProcessResult.SkipCurrentNode;
                            }
                        }
                    }
                }

                mLog.Info(string.Format("finish to scan item, Id:{0}.UIVersion:{1},should do arhiver:{2}.", item.LibRowID, item.UIVersion, item.ShouldDoArchive));
                return result;
            }
        }

        internal override async Task<(Rule, bool)> CheckContainerRuleAsync(ArchiverNodeItem item)
        {
            Rule result = null;
            return (result, false);
        }

        private Rule CheckItemRuleForScope(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                RuleEngineForScope.CurrentRuleId = mConfig.currentRule.Id;
                Rule result = null;
                switch (item.ItemType)
                {
                    case RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT:
                        if (RuleEngineForScope.HasDocumentCondition)
                        {
                            result = RuleEngineForScope.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        else
                        {
                            //means all scope
                            result = new Rule() { Name= "build in all scope" };
                        }
                        break;
                    default:
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule);
                }
                if (result != null)
                {
                    result.DeleteRecords = mConfig.currentRule.DeleteRecords;
                    string fitRuleName = result.Name;
                    result = CheckHoldOnlyOrRecord(item, result);
                }
                return result;
            }
        }

        private Rule CheckItemRuleForDocumentTagRule(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                RuleEngineForDocumentTag.CurrentRuleId = mConfig.currentRule.Id;
                Rule result = null;
                switch (item.ItemType)
                {
                    case RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT:
                        if (RuleEngineForDocumentTag.HasDocumentCondition)
                        {
                            result = RuleEngineForDocumentTag.CheckItemCriteria(item.ID, item.DiscoverSPObject);
                        }
                        else
                        {
                            result = new Rule() { Name = "build in all document" };
                        }
                        break;
                    default:
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule);
                }
                if (result != null)
                {
                    result.DeleteRecords = mConfig.currentRule.DeleteRecords;
                    string fitRuleName = result.Name;
                    result = CheckHoldOnlyOrRecord(item, result);
                }
                return result;
            }
        }

        private Rule CheckItemVersionRuleForROTAndInactive(ArchiverNodeItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ScanDiscovrerNodeWorker.CheckItemRule"))
            {
                RuleEngineForDocumentVersionTag.CurrentRuleId = mConfig.currentRule.Id;
                Rule result = null;
                switch (item.ItemType)
                {
                    case RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER:
                        if (RuleEngineForDocumentVersionTag.HasDocVersionCondition)
                        {
                            result = RuleEngineForDocumentVersionTag.CheckItemVersionCriteria(item.ID, item.Parent.DiscoverSPObject, item.DiscoverSPObject);
                        }
                        else
                        {
                            result = new Rule() { Name = "build in all document version" };
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
    }
}

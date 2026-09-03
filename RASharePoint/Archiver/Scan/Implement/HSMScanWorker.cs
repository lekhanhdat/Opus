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
    public class HSMScanWorker:DiscoverNodeWorkerBase
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        internal ScheduleConfiguration mConfig = null;
        internal ScanJobSettings mJobSettings = null;
        public PCContainer<ArchiveApproveReport> pcContainer = new PCContainer<ArchiveApproveReport>(1000);
        public IScheduleContainer<ArchiveApproveReport> ScheduleContainer = null;


        public HSMScanWorker(ScanJobSettings jobSettings, ScheduleConfiguration paraConfig, IBackwardDependencyNodeCache<object> dependencyObjs) : base(jobSettings, paraConfig, dependencyObjs)
        {
            //ScheduleContainer = new BackupNodeCache(pcContainer);
            //mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(new ApprovalReportService(paraConfig));
            mJobSettings = jobSettings;
            mConfig = paraConfig;
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
                    item.RuleId = mConfig.currentRule.Id;
                    var itemApprove = item.ConvertToArchiveApproveReport();
                    if (mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.Document)
                    {
                        item.ShouldDoArchive = true;
                        itemApprove.DoDelete = true;
                        mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                        JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
                    }
                    else if (mConfig.currentRule.PolicyLevel == GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion)
                    {
                        if (item.ItemType == ArchiverCommon.ItemType.DOCUMENT)
                        {
                            mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, false);
                        }
                        else
                        {
                            item.ShouldDoArchive = true;
                            itemApprove.DoDelete = true;
                            mApprovalReportProxy.PutIn(itemApprove, item.Cache_NodeType, item.ShouldDoArchive);
                            JobExecutionProcessStatisticExecutor.Instance.CalculateRuleAndScanSummary(result, itemApprove, mConfig.currentRule);
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
}
}

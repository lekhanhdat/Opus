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
using AvePoint.Hybrid.Contract;
using AvePoint.RA.FileSystem.Utils;
using AvePoint.RA.SharePoint.EnforceRuleAction;
using AvePoint.RA.SharePoint.ExplorerSync;
using AvePoint.RA.SharePoint.GlobalSearch;
using AvePoint.RA.SharePoint.RecordsUniqueIdSetting;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.RMSharePointTaxnomy;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.DataSync;
using RAFileSystem.FileSystem.DataSync.DataSyncExecutionStrategies;
using RAFileSystem.FileSystem.Discovery;
using RAFileSystem.FileSystem.Disposal;
using RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies;
using RAFileSystem.FileSystem.FileSystem.Restore;
using RAFileSystem.FileSystem.FileSystem.Retain;
using RAFileSystem.FileSystem.Jpmc.DataSync;
using RAFileSystem.FileSystem.Report;
using RAFileSystem.SharePoint.ScanLocalNode;

namespace AvePoint.RA.FileSystem
{
    public class FSServiceLocator
    {
        public IScheduleJobWorker Lookup(JobType action, string additional = null)
        {
            IScheduleJobWorker worker = default(IScheduleJobWorker);
            var isEnableJPMCFeature = ExternalUtil.CheckEnableFSJPMCFeature(additional);
            IFSExecutionStrategy execution = null;
            switch (action)
            {
                case JobType.FSDataSync:
                    if (isEnableJPMCFeature)
                    {
                        worker = new RMFileSystemDataSyncEngineRunner();
                    }
                    else
                    {
                        execution = new DataSyncExecutionStrategyV1();
                        worker = new FSDataSyncProcessorWorker(execution);
                    }
                    break;
                case JobType.FSDisposal:
                case JobType.FSDisposalByClassCode:
                    switch (isEnableJPMCFeature)
                    {
                        case true:
                            execution = new DisposalExecutionStrategyV3();
                            break;
                        case false:
                            execution = new DisposalExecutionStrategyV1();
                            break;
                    }
                    worker = new FSDisposalProcessorWorker(execution);
                    break;
                case JobType.FSArchiverRestore:
                    worker = new FSRestoreMain();
                    break;
                case JobType.FSRetain:
                case JobType.FSRetainSimulate:
                    worker = new FSRetainMain();
                    break;
                case JobType.FSCreationAndDestructionReport:
                    worker = new FSCreationReportWorker();
                    break;
                case JobType.FSContentDueReport:
                    worker = new FSContentDueReportWorker();
                    break;
                case JobType.SharePointOnPremApplySetting:
                    worker = new RMSettingProcessor();
                    break;
                case JobType.SPOnPremTermSynchronization:
                    worker = new RMSyncTermProcessor();
                    break;
                case JobType.SharePointOnPremEnforceRuleAction:
                    worker = new SPEnforceRuleActionWorker();
                    break;
                case JobType.SharePointOnPremDataSync:
                    worker = new RMSPExplorerProcessor();
                    break;
                case JobType.SPOnPremUniqueIDSetting:
                    worker = new SPUniqueIdSettingWorker();
                    break;
                case JobType.SPOnPremGlobalSearch:
                    worker = new GlobalSearch();
                    break;
                case JobType.SPOnPremScanNode:
                    worker = new ScanLocalNode();
                    break;
                case JobType.FSDiscovery:
                    worker = new FSDiscoveryProcessor();
                    break;
                default:
                    break;
            }
            return worker;
        }
    }
}

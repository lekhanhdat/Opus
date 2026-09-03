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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Task.DataIngestion;
using AvePoint.RA.Contract.Task.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Task
{
    public class RMTaskFactory
    {
        public static TaskBase GetDefaultTask(TaskType type)
        {
            TaskBase task = null;
            switch (type)
            {
                case TaskType.ScheduleJob:
                    task = new ScheduleJobTask();
                    break;
                case TaskType.RealTimeJob:
                    task = new RealTimeJobTask();
                    break;
                case TaskType.SubJob:
                    task = new SubJobTask();
                    break;
                case TaskType.ChangeWaitingSubJob2CanRun:
                    task = new ChangeWaitingSubJobTask();
                    break;
                case TaskType.JobTimeout:
                    task = new JobTimeoutTask();
                    break;
                case TaskType.LicenseExpried:
                    task = new LicenseExpriedTask();
                    break;
                case TaskType.EnforceRuleActionJob:
                    task = new EnforceRuleActionJobTask();
                    break;
                case TaskType.SyncAOSUser:
                    task = new SyncAOSUserTask();
                    break;
                case TaskType.UpdateCosmosDBIndexPolicy:
                    task = new UpdateCosmosIndexPolicyTask();
                    break;
                case TaskType.APIStatus:
                    task = new APIStatusTask();
                    break;
                case TaskType.COPMessage:
                    task = new COPMessageTask();
                    break;
                case TaskType.UpgradeDB:
                    task = new UpgradeDBTask();
                    break;
                case TaskType.TimerLocker:
                    task = new TimerLocker();
                    break;
                case TaskType.CheckAndUpdateAgentStatus:
                    task = new CheckAndUpdateAgentStatusTask();
                    break;
                case TaskType.InitNodesFromAOS:
                    task = new InitNodesFromAOSTask();
                    break;
                case TaskType.ObserveAOSNotification:
                    task = new ObserveAOSNotificationTask();
                    break;
                case TaskType.ProcessAOSNotification:
                    task = new ProcessAOSNotificationTask();
                    break;
                //case TaskType.DeleteArchivedSiteCollectionNodes:
                //    task = new DeleteArchivedSiteCollectionNodesTask();
                //    break;
                case TaskType.UpgradePersonalHoldData:
                    task = new UpgradePersonalHoldDataTask();
                    break;
                case TaskType.Monitor:
                    task = new MonitorTask();
                    break;
                case TaskType.UpgradeManualData:
                    task = new UpgradeManualDataTask();
                    break;
                case TaskType.TenantUpgrade:
                    task = new TenantUpgradeTask();
                    break;
                case TaskType.SyncDownloadArchivedContentJobStatus:
                    task = new SyncArchivedContentJobStatusTask();
                    break;
                case TaskType.DeleteExpiredArchivedContent:
                    task = new DeleteExpiredArchivedContentTask();
                    break;
                case TaskType.UpdateAosAppProfile:
                    task = new UpdateAosAppProfileTask();
                    break;
                case TaskType.ManualSepCIUpgrade:
                    task = new ManualSepCIUpgradeTask();
                    break;
                case TaskType.ManualSettingSepCIUpgrade:
                    task = new ManualSettingSepCIUpgradeTask();
                    break;
                case TaskType.ManualSepCIUpgradeOnlyForCLP:
                    task = new ManualSepCIUpgradeOnlyForCLPTask();
                    break;
                case TaskType.ManualHistoriesUpgrade:
                    task = new ManualHistoriesTask();
                    break;
                case TaskType.MachineLearningModelStatus:
                    task = new MachineLearningModelStatusTask();
                    break;
                case TaskType.SharePointOnlineDeletionSyncUpgrade:
                    task = new SharePointOnlineDeletionSyncUpgradeTask();
                    break;
                case TaskType.RuleStubAndStoragePolicyUpgrade:
                    task = new RuleStubAndStoragePolicyUpgrade();
                    break;
                case TaskType.ManualFileSystemUpgrade:
                    task = new ManualFileSystemUpgradeTask();
                    break;
                case TaskType.CosmosDBDirtyDataDeletion:
                    task = new CosmosDBDirtyDataDeleteUpgradeTask();
                    break;
                case TaskType.ExecuteHighPrioritySubJobs:
                    task = new ExecuteHighPrioritySubJobTask();
                    break;
                case TaskType.CheckCOPDeletion:
                    task = new CheckCOPDeletionTask();
                    break;
                case TaskType.DiscoveryTriggerTimer:
                    task = new RMDiscoveryTriggerTask();
                    break;
                case TaskType.DiscoveryMonitorTimer:
                    task = new RMDiscoveryMonitorTask();
                    break;
                case TaskType.DiscoveryStarterTimer:
                    task = new RMDiscoveryStarterTask();
                    break;
                case TaskType.DiscoveryClearLicenseUsage:
                    task = new RMDiscoveryClearLicenseUsageTask();
                    break;
                case TaskType.AosStatisticsSizeUpdate:
                    task = new UpdateAosSOJobSizeTask();
                    break;
                case TaskType.DiscoveryOptimizationTimer:
                    task = new RMDiscoveryOptimizationTask();
                    break;
                case TaskType.CorrectOldReocrdsRuleAfterMigration:
                    task = new CorrectOldReocrdsRuleAfterMigrationTask();
                    break;
                case TaskType.EnsureSecurityProfileAfterMigration:
                    task = new EnsureSecurityProfileAfterMigrationTask();
                    break;
                case TaskType.AddDiscoveryDBToFailoverGroup:
                    task = new AddDiscoveryDBToFailoverGroupTask();
                    break;
                case TaskType.StatisticSOCustomerAndMigrationJob:
                    task = new StatisticSOCustomerAndMigrationJob();
                    break;
                case TaskType.CheckBackupFailedSubjob:
                    task = new CheckBackupFailedSubjobTask();
                    break;
                case TaskType.UpdateSubJobIdToArchiverIndexSubInfoTable:
                    task = new UpdateSubJobIdToArchiverIndexSubInfoTableTask();
                    break;
                case TaskType.ArchivedSiteInfoTenantIdUpgrade:
                    task = new ArchivedSiteInfoTenantIdUpgradeTask();
                    break;
                case TaskType.AddDBToElasticDBPool:
                    task = new ElasticDBPoolTask();
                    break;
                case TaskType.ThrottlingStatistic:
                    task = new ThrottlingStatisticTask();
                    break;
                case TaskType.FeatureUsageLimit:
                    task = new FeatureUsageLimitTask();
                    break;
                case TaskType.ZeroshotCheckLicense:
                    task = new ZeroshotCheckLicenseTask();
                    break;
                case TaskType.UpgradeAvePointStorageForMigrated21VTenants:
                    task = new UpgradeAvePointStorageForMigrated21VTenantsTask();
                    break;
                case TaskType.MigratePhysicalLocationPermission:
                    task = new MigratePhysicalLocationPermissionTask();
                    break;
                case TaskType.DataIngestion:
                    task = new RMDataIngestionTask();
                    break;
                case TaskType.MultiGeoSyncCommonData:
                    task = new MultiGeoSyncCommonDataTask();
                    break;
                case TaskType.CheckTimeoutJobQueueMessage:
                    task = new CheckTimeoutJobQueueMessageTask();
                    break;
                case TaskType.APStorageCostEvaluation:
                    task = new APStorageCostEvaluationTask();
                    break;
                case TaskType.DiscoveryDalJobMonitorTimer:
                    task = new DiscoveryDalJobMonitorTimerTask();
                    break;
                default:
                    throw new NotSupportedException($"not supported task type:{type}");
            }
            return task.AssembleDefaultTask();
        }
    }
}

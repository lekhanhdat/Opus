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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Service.RMTasks.DataIngestion;
using AvePoint.RA.Service.RMTasks.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class TaskExecutorFactory
    {
        public static ITaskExecutor GetTaskExecutor(TaskType type)
        {
            ITaskExecutor taskExecutor = null;
            switch (type)
            {
                case TaskType.ScheduleJob:
                    taskExecutor = new ScheduleJobTaskExecutor();
                    break;
                case TaskType.RealTimeJob:
                    taskExecutor = new RealTimeJobTaskExecutor();
                    break;
                case TaskType.SubJob:
                    taskExecutor = new SubJobTaskExecutor();
                    break;
                case TaskType.ChangeWaitingSubJob2CanRun:
                    taskExecutor = new WaitingSubJobTaskExecutor();
                    break;
                case TaskType.JobTimeout:
                    taskExecutor = new JobTimeoutTaskExecutor();
                    break;
                case TaskType.LicenseExpried:
                    taskExecutor = new LicenseCheckTaskExecutor();
                    break;
                case TaskType.EnforceRuleActionJob:
                    taskExecutor = new EnforceRuleActionJobTaskExecutor();
                    break;
                case TaskType.SyncAOSUser:
                    taskExecutor = new SyncAOSUserTaskExecutor();
                    break;
                case TaskType.UpdateCosmosDBIndexPolicy:
                    taskExecutor = new UpdateCosmosIndexPolicyTaskExecutor();
                    break;
                case TaskType.APIStatus:
                    taskExecutor = new APIStatusTaskExecutor();
                    break;
                //case TaskType.UpgradeDB:
                //    taskExecutor = new COPMessageTaskExecutor();
                //    break;
                case TaskType.CheckAndUpdateAgentStatus:
                    taskExecutor = new AgentStatusTaskExecutor();
                        break;
                case TaskType.InitNodesFromAOS:
                    taskExecutor = new InitNodesFromAOSTaskExecutor();
                    break;
                case TaskType.ObserveAOSNotification:
                    taskExecutor = new ObserveAOSNotificationTaskExecutor();
                    break;
                case TaskType.ProcessAOSNotification:
                    taskExecutor = new ProcessAOSNotificationTaskExecutor();
                    break;
                //case TaskType.DeleteArchivedSiteCollectionNodes:
                //    taskExecutor = new DeleteArchivedSiteCollectionNodesTaskExecutor();
                //    break;
                case TaskType.UpgradePersonalHoldData:
                    taskExecutor = new UpgradePersonalHoldDataTaskExecutor();
                    break;
                case TaskType.Monitor:
                    taskExecutor = new MonitorTaskExecutor();
                    break;
                case TaskType.UpgradeManualData:
                    taskExecutor = new UpgradeManualDataTaskExecutor();
                    break;
                case TaskType.SyncDownloadArchivedContentJobStatus:
                    taskExecutor = new SyncArchivedContentJobStatusTaskExecutor();
                    break;
                case TaskType.DeleteExpiredArchivedContent:
                    taskExecutor = new DeleteExpiredArchivedContentTaskExecutor();
                    break;
                case TaskType.TenantUpgrade:
                    taskExecutor = new TenantUpgradeTaskExecutor();
                    break;
                case TaskType.UpdateAosAppProfile:
                    taskExecutor = new UpdateAosAppProfileTaskExecutor();
                    break;
                case TaskType.ManualSepCIUpgrade:
                    taskExecutor = new ManualSepCIUpgradeTaskExecutor();
                    break;
                case TaskType.ManualSettingSepCIUpgrade:
                    taskExecutor = new ManualSettingSepCIUpgradeTaskExecutor();
                    break;
                case TaskType.ManualSepCIUpgradeOnlyForCLP:
                    taskExecutor = new ManualSepCIOnlyForCLPUpgradeTaskExecutor();
                    break;
                case TaskType.ManualHistoriesUpgrade:
                    taskExecutor = new ManualHistoriesTaskExecutor();
                    break;
                case TaskType.MachineLearningModelStatus:
                    taskExecutor = new MachineLearningModelStatusTaskExecutor();
                    break;
                case TaskType.SharePointOnlineDeletionSyncUpgrade:
                    taskExecutor = new SharePointOnlineDeletionSyncUpgradeExecutor();
                    break;
                case TaskType.RuleStubAndStoragePolicyUpgrade:
                    taskExecutor = new UpgradeRuleStubAndStoragePolicy();
                    break;
                //case TaskType.CorrectOldReocrdsRuleAfterMigration:
                //    taskExecutor = new CorrectOldReocrdsRuleAfterMigrationExecutor();
                //    break;
                case TaskType.CosmosDBDirtyDataDeletion:
                    taskExecutor = new CosmosDBDirtyDataDeleteUpgradeExecutor();
                    break;
                case TaskType.ManualFileSystemUpgrade:
                    taskExecutor = new ManualFileSystemUpgradeExecutor();
                    break;
                case TaskType.ExecuteHighPrioritySubJobs:
                    taskExecutor = new ExecuteHighPrioritySubJobExecutor();
                    break;
                case TaskType.CheckCOPDeletion:
                    taskExecutor = new CheckCOPDeletionTaskExecutor();
                    break;
                case TaskType.DiscoveryTriggerTimer:
                    taskExecutor = new DiscoveryTriggerTaskExecutor();
                    break;
                case TaskType.DiscoveryMonitorTimer:
                    taskExecutor = new DiscoveryMonitorTaskExecutor();
                    break;
                case TaskType.DiscoveryStarterTimer:
                    taskExecutor = new DiscoveryStarterTaskExecutor();
                    break;
                case TaskType.DiscoveryClearLicenseUsage:
                    taskExecutor = new DiscoveryClearLicenseUsageTaskExecutor();
                    break;
                case TaskType.AosStatisticsSizeUpdate:
                    taskExecutor = new UpdateAosStatisticsSizeExecutor();
                    break;
                case TaskType.DiscoveryOptimizationTimer:
                    taskExecutor = new DiscoverOptimizationTaskExecutor();
                    break;
                case TaskType.EnsureSecurityProfileAfterMigration:
                    taskExecutor = new EnsureSecurityProfileAfterMigrationExecutor();
                    break;
                case TaskType.AddDiscoveryDBToFailoverGroup:
                    taskExecutor = new AddDiscoveryDBToFailoverGroupExecutor();
                    break;
                case TaskType.StatisticSOCustomerAndMigrationJob:
                    taskExecutor = new StatisticSOCustomerAndMigrationJobExecutor();
                    break;
                case TaskType.CheckBackupFailedSubjob:
                    taskExecutor = new CheckBackupFailedSubjobExecutor();
                    break;
                case TaskType.UpdateSubJobIdToArchiverIndexSubInfoTable:
                    taskExecutor = new UpdateSubJobIdToArchiverIndexSubInfoTableTaskExecutor();
                    break;
                case TaskType.ArchivedSiteInfoTenantIdUpgrade:
                    taskExecutor = new ArchivedSiteInfoTenantIdUpgradeExecutor();
                    break;
                case TaskType.AddDBToElasticDBPool:
                    taskExecutor = new DBPoolTaskExecutor();
                    break;
                case TaskType.ThrottlingStatistic:
                    taskExecutor = new ThrottlingStatisticExecutor();
                    break;
                case TaskType.FeatureUsageLimit:
                    taskExecutor = new FeatureUsageLimitTaskExcutor();
                    break;
                case TaskType.ZeroshotCheckLicense:
                    taskExecutor = new ZeroshotCheckLicenseTaskExcutor();
                    break;
                case TaskType.UpgradeAvePointStorageForMigrated21VTenants:
                    taskExecutor = new UpgradeAvePointStorageForMigrated21VTenantsTaskExcutor();
                    break;
                case TaskType.MigratePhysicalLocationPermission:
                    taskExecutor = new MigratePhysicalLocationPermissionTaskExecutor();
                    break;
                case TaskType.DataIngestion:
                    taskExecutor = new DataIngestionTaskExecutor();
                    break;
                case TaskType.MultiGeoSyncCommonData:
                    taskExecutor = new MultiGeoSyncCommonDataTaskExecutor();
                    break;
                case TaskType.CheckTimeoutJobQueueMessage:
                    taskExecutor = new CheckTimeoutJobQueueMessageTaskExecutor();
                    break;
                case TaskType.APStorageCostEvaluation:
                    taskExecutor = new APStorageCostEvaluationTaskExecutor();
                    break;
                case TaskType.DiscoveryDalJobMonitorTimer:
                    taskExecutor = new DiscoveryDalJobMonitorTimerExecutor();
                    break;
                default:
                    throw new NotSupportedException($"Not supported task: {type.ToString()}");
            }
            return taskExecutor;
        }
    }
}

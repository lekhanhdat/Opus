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

namespace AvePoint.RA.Contract.Task
{
    public enum TaskType
    {
        ScheduleJob = 1,
        RealTimeJob = 2,
        SubJob = 3,
        JobTimeout = 4,
        LicenseExpried = 5,
        EnforceRuleActionJob = 6,
        SyncAOSUser = 7,
        APIStatus = 8,
        COPMessage = 9,
        UpgradeDB = 10,
        TimerLocker = 11,
        CheckAndUpdateAgentStatus = 12,
        InitNodesFromAOS = 13,
        ObserveAOSNotification = 14,
        //DeleteArchivedSiteCollectionNodes = 15,
        UpgradePersonalHoldData = 16,
        ChangeWaitingSubJob2CanRun = 17,
        UpdateCosmosDBIndexPolicy = 18,
        Monitor = 19,
        ProcessAOSNotification = 20,
        UpgradeManualData = 21,
        SyncDownloadArchivedContentJobStatus = 22,
        DeleteExpiredArchivedContent = 23,
        TenantUpgrade = 24,
        ManualSepCIUpgrade = 25,
        ManualSettingSepCIUpgrade = 26,
        UpdateAosAppProfile = 27,
        ManualSepCIUpgradeOnlyForCLP = 28,
        ManualHistoriesUpgrade = 29,
        MachineLearningModelStatus = 30,
        SharePointOnlineDeletionSyncUpgrade = 31,
        RuleStubAndStoragePolicyUpgrade = 33,
        CosmosDBDirtyDataDeletion = 32,
        ManualFileSystemUpgrade = 34,
        ExecuteHighPrioritySubJobs = 35,
        CheckCOPDeletion = 36,
        #region discovery
        DiscoveryTriggerTimer = 37,
        DiscoveryMonitorTimer = 38,
        DiscoveryStarterTimer = 39,
        AosStatisticsSizeUpdate = 40,
        DiscoveryOptimizationTimer = 41,
        //CorrectOldReocrdsRuleAfterMigration = 42, //这个task内部逻辑更改 42被废弃
        CorrectOldReocrdsRuleAfterMigration = 43, //这个task 也已经不需要
        EnsureSecurityProfileAfterMigration = 44,
        AddDiscoveryDBToFailoverGroup = 45,
        #endregion
        StatisticSOCustomerAndMigrationJob = 46,
        CheckBackupFailedSubjob = 47,
        UpdateSubJobIdToArchiverIndexSubInfoTable = 48,
        //ClearLoginAudit = 49,
        //ClearInactiveTenantLoginAudit = 50, used in release brance, next task should be '51',
        DiscoveryClearLicenseUsage = 52,
        ArchivedSiteInfoTenantIdUpgrade = 53,
        AddDBToElasticDBPool = 54,
        ThrottlingStatistic = 55,
        FeatureUsageLimit = 56,
        ZeroshotCheckLicense = 57,
        UpgradeAvePointStorageForMigrated21VTenants = 58,
        MigratePhysicalLocationPermission = 59,
        DataIngestion = 60,
        MultiGeoSyncCommonData = 61,
        CheckTimeoutJobQueueMessage = 62,
        APStorageCostEvaluation = 63,
        DiscoveryDalJobMonitorTimer = 64,
    }

    public enum RMTaskStatus
    {
        //不使用Status状态.
        Completed = 0,
        Processing = 1,
    }
}

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





using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    public enum JobTypes : int
    {
        BackupJob = 1,
        CASearchJob = 2,
        CAJob = 3,
        BackupJobFB = 4,
        BackupJobIB = 5,
        BackupJobDB = 6,
        ContentManagerJob = 7,
        RestoreJob = 8,
        Replicator = 9,
        PRBackupJobFB = 10,
        PRBackupJobIB = 11,
        PRBackupJobDB = 12,
        PRRestoreJob = 13,
        SOStubRetentionExtender = 15,
        SOConvertStubToContent = 16,
        DesignManagerJob = 17,
        SOConfigStubDB = 18,
        SOExtenderScheduled = 19,
        RCCollectorJob = 20,
        DeploymentManagerJob = 21,
        FrontendDeployment = 22,
        SoluctionCenter = 23,
        ArchiverScan = 24,
        LogManager = 25,
        ContentManagerImportJob = 26,
        ContentManagerExportJob = 27,
        ArchiverRestore = 28,
        ArchiverBackup = 29,
        ArchiverMergeIndex = 30,
        PRMaintenanceJob = 31,
        MetadataService = 32,
        RPRealTime = 33,
        RPConflict = 34,
        ArchiverRetention = 35,
        JobPruning = 36,
        ConnectorSync = 37,
        PRJobRentention = 38,
        LanguageTranslater = 39,
        FileMigrationJob = 40,
        SPMigration07_10 = 41,
        EndUserArchiverBackup = 42,
        eRoomMigrationJob = 43,
        LivelinkMigrationJob = 44,
        NotesMigrationJob = 45,
        EndUserMergeIndex = 46,
        ExtenderDataUpgrade = 47,
        GranularRetention = 48,
        UpgradeImportData = 50,
        ArchiverUpgradeData = 51,
        VaultScanJob = 52,
        ReplicatorImportPlan = 55,
        PublicFolderMigration = 56,

        FileMigrationGenerateExcelFile = 57,
        UpgradeSolutionData = 59,
        SPMigration07_10_Export = 80,

        SPMigration07_10_Import = 90,
        #region 70--79 eDiscovery占用
        EDContentSourceJob = 70,
        EDSearchJob = 71,
        EDExtention5 = 72,
        EDHoldJob = 73,
        EDReleaseJob = 74,
        EDRealTimeJob = 75,
        EDSyncJob = 76,
        EDExportJob = 77,
        EDExtention6 = 78,
        EDExtention7 = 79,
        #endregion
        DeploymentManagerUpload = 81,
        ArchiverFullTextIndexJob = 58,
        EndUserRestore = 60,
        PRNAMigrationDbAndIndex = 62,
        FarmRebuildJob = 63,
        PRJobRetentionForSN = 64,
        VaultExportJob = 65,
        EBSStubUpgrade = 99,

        PRNAMigrationDb = 67,
        PRNAMigrationIndex = 68,
        PRDataManagerIndex = 100,
        #region Other module run Granular Backup&Restore job type
        CMBackupJob = 85,
        ReplicatorBackupJob = 86,
        DPMBackupJob = 87,
        CMRestoreJob = 84,
        ReplicatorRestoreJob = 88,
        DPMRestoreJob = 89,
        #endregion
        ExchangeOnlineBackupJobAdhoc = 110,
        ExchangeOnlineBackupJobFB = 111,
        ExchangeOnlineBackupJobIB = 112,
        ExchangeOnlineBackupJobDB = 113,
        ExchangeOnlienRestoreJob = 114,
        DataTransferJob = 115,
        DTDeleteJob = 116,
        ExchangeOnlineRetention = 117,
        ExportReport = 118,
        DeleteGroup = 119,
        Defragmenter = 120,
        ArchiverMoveIndex = 121,
        DeploymentManagerCompare = 122,
        ArchiverVEOMergeJob = 123,
        ExchangeArchiverScan = 124,
        ExchangeArchiverBackup = 125,
        PhysicalRecords = 4000,
        #region 300-400 CA Used

        CAProfileJob = 373,
        /// <summary>
        /// PE Job中只有Auditor类型的Rule
        /// </summary>
        CAOnlyAuditorRulePEJob = 374,
        CAOnlyScanRulePEJob = 375,

        #endregion

        CloudAppAdminJob = 401,
        CloudAppAdminPEJob = 402,

        #region media

        MediaGranularAdvancedSearch = 500,
        MediaExchangeAdvancedSearch = 501,
        MediaGranularRetention = 502,
        MediaArchiverRetention = 503,
        MediaExchangeRetention = 504,
        MediaGranularDataTransfer = 505,
        MediaExchangeDataTransfer = 506
        
        #endregion
    }

    public class JobTypeAgentTypeMappings
    {
        private static Dictionary<JobTypes, List<string>> Mappings { get; set; }

        private static readonly object Loker = new object();

        public static void InitMappings()
        {
            lock (Loker)
            {
                if (Mappings == null)
                {
                    Mappings = new Dictionary<JobTypes, List<string>>();

                    Mappings[JobTypes.ArchiverBackup] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverMergeIndex] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverRestore] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverScan] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserArchiverBackup] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserMergeIndex] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserRestore] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.BackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobDB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    //Mappings[JobTypes.ExchangeOnlineBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.ExchangeOnlineBackupJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.ExchangeOnlineBackupJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.ExchangeOnlienRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.CAJob] = new List<string>() { AgentTypes.AGENT_TYPE_SMS };
                    Mappings[JobTypes.CASearchJob] = new List<string>() { AgentTypes.AGENT_TYPE_SMS };
                    Mappings[JobTypes.ConnectorSync] = new List<string>() { AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.ContentManagerExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ContentManagerImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ContentManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.DeploymentManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.DesignManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.FrontendDeployment] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.MetadataService] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.PRBackupJobDB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRBackupJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRBackupJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRJobRentention] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRMaintenanceJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.RCCollectorJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPORT_CENTER };
                    Mappings[JobTypes.Replicator] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    Mappings[JobTypes.RestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.RPConflict] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.RPRealTime] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SOConvertStubToContent] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.SOExtenderScheduled] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE };
                    Mappings[JobTypes.SoluctionCenter] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.SOStubRetentionExtender] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.SPMigration07_10] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_10_Export] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_10_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.ExtenderDataUpgrade] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.EBSStubUpgrade] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.EDContentSourceJob] = new List<string>() { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.PRNAMigrationDbAndIndex] = new List<string> { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.VaultScanJob] = new List<string>() { AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT };
                    Mappings[JobTypes.VaultExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT };
                    #region Other module run Granular Backup&Restore job type
                    Mappings[JobTypes.CMBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ReplicatorBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    Mappings[JobTypes.DPMBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.CMRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ReplicatorRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    Mappings[JobTypes.DPMRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    #endregion
                    #region - eDiscovery job type -
                    Mappings[JobTypes.EDHoldJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDExportJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDReleaseJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDSearchJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDSyncJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    //                    Mappings[JobTypes.EDSearchArchiveJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    #endregion
                }

            }

        }

        public static List<string> GetAgentTypes(int jobType)
        {
            InitMappings();

            return Mappings[(JobTypes)jobType];
        }
    }
}
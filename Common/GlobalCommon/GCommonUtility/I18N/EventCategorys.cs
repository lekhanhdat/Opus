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
using AvePoint.GCommon.Contract.AveModuleContract;

namespace AvePoint.GCommon.Utility.I18N
{
    //1-2000	 
    public class EventCategorys
    {
        //DocAve Control Service    101-300	 
        public class DocAveControlService
        {
            //Common    1-20
            public const ushort Common_ControlService = 1;
            public const ushort Common_JobMonitor = 2;
            public const ushort Common_PlanGroup = 3;


            //Migration 21-40
            public const ushort Migration = 21;
            public const ushort Migration_SP2007ToSP2010Migration = 22;
            public const ushort Migration_FileSystemMigration = 23;
            public const ushort Migration_eRoomMigration = 24;
            public const ushort Migration_LotusNotesMigration = 25;
            public const ushort Migration_LiveLinkMigration = 26;
            public const ushort Migration_ExchangePublicFolderMigration = 27;


            //Data Protection 41-60
            //            public const ushort DataProtection = 41;
            public const ushort DataProtection_GranularBackup = 42;
            public const ushort DataProtection_GranularRestore = 43;
            public const ushort DataProtection_PlatformBackup = 44;
            public const ushort DataProtection_PlatformRestore = 45;
            public const ushort DataProtection_PlatformSQLRestore = 46;
            public const ushort DataProtection_ExchangeOnlineBackup = 47;
            public const ushort DataProtection_ExchangeOnlineRestore = 48;


            //Administration 61-80
            //            public const ushort Administration = 61;
            public const ushort Administration_Administrator = 62;
            public const ushort Administration_ContentManager = 63;
            public const ushort Administration_DeploymentManager = 64;
            public const ushort Administration_Replicator = 65;


            //Compliance 81-100
            //            public const ushort Compliance = 81;
            public const ushort Compliance_eDiscovery = 82;
            public const ushort Compliance_Vault = 83;


            //Report Center 101-120
            public const ushort ReportCenter = 101;
            public const ushort ReportCenter_UsageReports = 102;
            public const ushort ReportCenter_InfrastructureReports = 103;
            public const ushort ReportCenter_AdministrationReports = 104;
            public const ushort ReportCenter_ComplianceReports = 105;
            public const ushort ReportCenter_DocAveReports = 106;
            public const ushort ReportCenter_Settings = 107;


            //Storage Optimization 121-140
            public const ushort StorageOptimization = 121;
            public const ushort StorageOptimization_RealtimeStorageManager = 122;
            public const ushort StorageOptimization_ScheduledStorageManager = 123;
            public const ushort StorageOptimization_Connector = 124;
            public const ushort StorageOptimization_Archiver = 125;


            //Control Panel 201-300
            public const ushort ControlPanel_Monitor_ManagerMonitor = 201;
            public const ushort ControlPanel_Monitor_AgentMonitor = 202;

            public const ushort ControlPanel_SystemOptions_GeneralSettings = 203;
            public const ushort ControlPanel_SystemOptions_SecuritySettings = 204;
            public const ushort ControlPanel_SystemOptions_AdvancedSettings = 205;

            public const ushort ControlPanel_AuthenticationManager = 206;

            public const ushort ControlPanel_AccountManager = 207;

            public const ushort ControlPanel_LicenseManager = 208;

            public const ushort ControlPanel_UpdateManager = 209;

            public const ushort ControlPanel_AgentGroups = 210;

            public const ushort ControlPanel_UserNotificationSettings = 211;

            public const ushort ControlPanel_JobPruning = 212;

            public const ushort ControlPanel_LogManager = 213;
            public const ushort ControlPanel_LogManager_AutoSupportSettings = 214;

            public const ushort ControlPanel_SharePointSites = 215;

            public const ushort ControlPanel_ProfileManager_SecurityProfile = 216;

            public const ushort ControlPanel_SolutionManager = 217;

            public const ushort ControlPanel_StorageConfiguration_PhysicalDevice = 218;
            public const ushort ControlPanel_StorageConfiguration_LogicalDevice = 219;
            public const ushort ControlPanel_StorageConfiguration_StoragePolicy = 220;

            public const ushort ControlPanel_DataManager = 221;
            public const ushort ControlPanel_DataManager_IndexManager = 222;

            public const ushort ControlPanel_ExportLocation = 223;

            public const ushort ControlPanel_FilterPolicy = 224;

            public const ushort ControlPanel_MappingManager_DomainMapping = 225;
            public const ushort ControlPanel_MappingManager_UserMapping = 226;
            public const ushort ControlPanel_MappingManager_LanguageMapping = 227;
            public const ushort ControlPanel_MappingManager_ColumnMapping = 228;
            public const ushort ControlPanel_MappingManager_ContentTypeMapping = 229;
            public const ushort ControlPanel_MappingManager_TemplateMapping = 230;
            public const ushort ControlPanel_MappingManager_GroupMapping = 231;

            public const ushort ControlPanel_DataManager_DataTransfer = 232;
        }

        //DocAve Agent Service 301-700 		
        public class DocAveAgentService
        {
            //Common    301-320
            public const ushort Common_AgentService = 301;
            public const ushort Common_Wrapper = 302;
            public const ushort Common_Office365 = 303;


            //Migration 321-360
            public const ushort Migration = 321;
            public const ushort Migration_SP2007ToSP2010Migration = 322;
            public const ushort Migration_FileSystemMigration = 323;
            public const ushort Migration_eRoomMigration = 324;
            public const ushort Migration_LotusNotesMigration = 325;
            public const ushort Migration_LiveLinkMigration = 326;
            public const ushort Migration_ExchangePublicFolderMigration = 327;


            //Data Protection   361-400
            //            public const ushort DataProtection = 361;
            public const ushort DataProtection_SP2010_GranularBackup_SiteCollectionLevel = 362;
            public const ushort DataProtection_SP2010_GranularBackup_SiteLevel = 363;
            public const ushort DataProtection_SP2010_GranularBackup_ItemLevel = 364;

            public const ushort DataProtection_SP2010_GranularRestore_SiteCollectionLevel = 365;
            public const ushort DataProtection_SP2010_GranularRestore_SiteLevel = 366;
            public const ushort DataProtection_SP2010_GranularRestore_ItemLevel = 367;

            public const ushort DataProtection_SP2010_PlatformBackup = 368;

            public const ushort DataProtection_SP2010_PlatformRestore = 369;

            public const ushort DataProtection_SP2007_GranularBackup_SiteCollectionLevel = 370;
            public const ushort DataProtection_SP2007_GranularBackup_SiteLevel = 371;
            public const ushort DataProtection_SP2007_GranularBackup_ItemLevel = 372;

            public const ushort DataProtection_PlatformBackupAndRestore = 373;


            //Administration    401-440
            //            public const ushort Administration = 401;
            public const ushort Administration_SP2010_Administrator_Search = 402;
            public const ushort Administration_SP2010_Administrator_Permissions = 403;
            public const ushort Administration_SP2010_Administrator_Actions = 404;
            //            public const ushort Administration_SP2010_Administrator_UniqueFeature = 405;

            public const ushort Administration_SP2010_ContentManager_Primary = 406;
            public const ushort Administration_SP2010_ContentManager_Secondary = 407;

            public const ushort Administration_SP2010_DeploymentManager_WebApplications_Primary = 408;
            public const ushort Administration_SP2010_DeploymentManager_WebApplications_Secondary = 409;
            public const ushort Administration_SP2010_DeploymentManager_WebFrontEnd_Primary = 410;
            public const ushort Administration_SP2010_DeploymentManager_WebFrontEnd_Secondary = 411;
            public const ushort Administration_SP2010_DeploymentManager_FarmSolutions_Primary = 412;
            public const ushort Administration_SP2010_DeploymentManager_FarmSolutions_Secondary = 413;
            public const ushort Administration_SP2010_DeploymentManager_SharedServices_Primary = 414;
            public const ushort Administration_SP2010_DeploymentManager_SharedServices_Secondary = 415;


            public const ushort Administration_SP2010_Replicator_Primary = 416;
            public const ushort Administration_SP2010_Replicator_Secondary = 417;
            public const ushort Administration_SP2010_Replicator_Offline = 418;
            public const ushort Administration_SP2010_Replicator_Realtime = 419;
            public const ushort Administration_SP2010_Replicator_Analyzer = 420;

            public const ushort Administration_Replicator = 421;

            public const ushort Administration_SP2010_Administrator_Deletion = 422;

            //Compliance    441-480
            //            public const ushort Compliance = 441;
            public const ushort Compliance_SP2010_eDiscovery = 442;
            public const ushort Compliance_SP2010_Hold = 443;
            public const ushort Compliance_SP2010_Vault = 444;


            //Report Center 481-520
            public const ushort ReportCenter = 481;
            public const ushort ReportCenter_SP2010_UsageReports = 482;
            public const ushort ReportCenter_SP2010_InfrastructureReports = 483;
            public const ushort ReportCenter_SP2010_AdministrationReports = 484;
            public const ushort ReportCenter_SP2010_ComplianceReports = 485;
            public const ushort ReportCenter_SP2010_Settings = 486;
            public const ushort ReportCenter_DocAveReports = 487;


            //Storage Optimization 521-560
            public const ushort StorageOptimization = 521;
            public const ushort StorageOptimization_Archiver_Backup = 522;
            //public const ushort StorageOptimization_Archiver_Restore = 523;
            public const ushort StorageOptimization_SP2010_Archiver_Restore = 523;
            public const ushort StorageOptimization_Archiver_EndUserArchiving = 524;

            public const ushort StorageOptimization_StorageManager_Realtime = 525;
            public const ushort StorageOptimization_StorageManager_Scheduled = 526;
            public const ushort StorageOptimization_StorageManager_Restore = 527;
            public const ushort StorageOptimization_StorageManager_CleanUpOrphanBlobs = 528;

            public const ushort StorageOptimization_Connector = 529;
        }

        //DocAve Media Service      701-750
        public class DocAveMediaService
        {
            public const ushort Common_MediaService = 701;
        }

        //DocAve Report Service     751-800	
        public class DocAveReportService
        {
            public const ushort Common_ReportService = 751;
        }

        //DocAve Package Service    801-850
        public class DocAvePackageService
        {
            public const ushort PatchInstallation = 801;
            public const ushort PatchUninstallation = 802;
            public const ushort CIPatchInstallation = 803;
            public const ushort CIPatchUninstallation = 804;
            public const ushort PackageInstallation = 805;
            public const ushort PackageUninstallation = 806;
        }

        //DocAve Storage            851-900
        public class DocAveStorageAPIService
        {
            //            public const ushort Common = 851;
            public const ushort NetShare = 852;
            public const ushort FTP = 853;
            public const ushort TSM = 854;
            public const ushort EMC_Centera = 855;
            public const ushort DELL_DX_Storage = 856;
            public const ushort Caringo_Storage = 857;
            public const ushort HDS_HCP = 858;
            public const ushort NetApp_ONTAP = 859;

            //            public const ushort Cloud = 860;
            public const ushort Cloud_Rackspace = 861;
            public const ushort Cloud_Windows_Azure = 862;
            public const ushort Cloud_Amazon_S3 = 863;
            public const ushort Cloud_EMC_Atmos = 864;
            public const ushort Cloud_ATT_Synaptic = 865;

            public const ushort SFTP = 871;
        }

        //        public class DocAveToolService
        //        {
        //            //DocAve Tool Service		501-700
        //            public const ushort ToolService = 501;
        //        }

        //        public class DocAveCLIService
        //        {
        //            //DocAve CLI Service		701-800
        //            public const ushort CLIService = 701;
        //        }

        //        public class DocAveAPIService
        //        {
        //            //DocAve API Service		801-900
        //            public const ushort APIService = 801;
        //        }

        public class JobIdCategory
        {
            public static IDictionary<int, ushort> JobCategory = new Dictionary<int, ushort>()
            {
                //Item
                {GranularBackup.BACKUP_JOB_DTO_TYPE, DocAveControlService.DataProtection_GranularBackup},
                {ModuleContract.DocAvePlatform.Administration.CentralAdmin.CA_SEARCH_JOB_DTO_TYPE, DocAveControlService.Administration_Administrator},
                {ModuleContract.DocAvePlatform.Administration.CentralAdmin.CA_JOB_DTO_TYPE, DocAveControlService.Administration_Administrator},
                {GranularBackup.BACKUP_JOB_DTO_TYPE_FB, DocAveControlService.DataProtection_GranularBackup},
                {GranularBackup.BACKUP_JOB_DTO_TYPE_IB, DocAveControlService.DataProtection_GranularBackup},
                {GranularBackup.BACKUP_JOB_DTO_TYPE_DB, DocAveControlService.DataProtection_GranularBackup},
                {GranularBackup.RESTORE_JOB_DTO_TYPE, DocAveControlService.DataProtection_GranularBackup},
                 //PR
                {PlatformBackup.PR_BACKUP_JOB_DTO_TYPE_FB, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_BACKUP_JOB_DTO_TYPE_IB, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_BACKUP_JOB_DTO_TYPE_DB, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_MAINTENANCE_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_RETENTION_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_RETENTION_JOB_DTO_FOR_NETAPP_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_MIGRATION_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_MIGRATION_DATABASE_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_MIGRATION_INDEX_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE, DocAveControlService.DataProtection_PlatformBackup},
                {PlatformBackup.PR_RESTORE_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformRestore},
                {PlatformBackup.FARM_REBUILD_JOB_DTO_TYPE, DocAveControlService.DataProtection_PlatformRestore},
                //Migration
                {ModuleContract.DocAvePlatform.Migration.eRoomMigration.EROOMMIGRATION_JOB_DTO_TYPE, DocAveControlService.Migration_eRoomMigration},
                {ModuleContract.DocAvePlatform.Migration.FileMigration.FILEMIGRATION_GENERATE_EXCEL_JOB_TYPE, DocAveControlService.Migration_FileSystemMigration},
                {ModuleContract.DocAvePlatform.Migration.FileMigration.FILEMIGRATION_JOB_DTO_TYPE, DocAveControlService.Migration_FileSystemMigration},
                {ModuleContract.DocAvePlatform.Migration.LivelinkMigration.LIVELINKMIGRATION_JOB_DTO_TYPE, DocAveControlService.Migration_LiveLinkMigration},
                {ModuleContract.DocAvePlatform.Migration.NotesMigration.NOTESMIGRATION_JOB_DTO_TYPE, DocAveControlService.Migration_LotusNotesMigration},
                {ModuleContract.DocAvePlatform.Migration.PublicFolderMigration.PFMIGRATION_JOB_DTO_TYPE, DocAveControlService.Migration_ExchangePublicFolderMigration},
                {ModuleContract.DocAvePlatform.Migration.SPMigration.SPMIGRATION_07_10_EXPORT_JOB_DTO_TYPE, DocAveControlService.Migration_SP2007ToSP2010Migration},
                {ModuleContract.DocAvePlatform.Migration.SPMigration.SPMIGRATION_07_10_Import_JOB_DTO_TYPE, DocAveControlService.Migration_SP2007ToSP2010Migration},
                {ModuleContract.DocAvePlatform.Migration.SPMigration.SPMIGRATION_07_10_JOB_DTO_TYPE, DocAveControlService.Migration_SP2007ToSP2010Migration},
                //Vault
                {ModuleContract.DocAvePlatform.Compliance.Vault.VAULT_EXPORT_JOB_TYPE, DocAveControlService.Compliance_Vault},
                //eDiscovery
                {EDiscovery.ED_EXPORT_JOB_TYPE, DocAveControlService.Compliance_eDiscovery},
                {EDiscovery.ED_HOLD_JOB_TYPE, DocAveControlService.Compliance_eDiscovery},
                {EDiscovery.ED_RELEASE_JOB_TYPE, DocAveControlService.Compliance_eDiscovery},
                {EDiscovery.ED_SEARCH_JOB_TYPE, DocAveControlService.Compliance_eDiscovery},
                {EDiscovery.ED_SYNC_JOB_TYPE, DocAveControlService.Compliance_eDiscovery},
                //Archiver
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_SCAN_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_BACKUP_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_MERGEINDEX_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_RESTORE_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_RETENSION_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_FULL_TEXT_INDEX_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_END_USER_ARCHIVER_BACKUP_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_END_USER_MERGE_INDEX_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_END_USER_RESTORE_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                {ModuleContract.DocAvePlatform.StorageOptimization.Archiver.SO_ARCHIVER_DATA_IMPORT_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_Archiver},
                //Storage Manager
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_EXTENDER_SCHEDULED_JOB_DTO_TYPE, DocAveControlService.StorageOptimization_ScheduledStorageManager},
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_STUB_RESTORE_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_STUB_RETENTION_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_EXTENDER_DATAUPGRADE_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_EBS_STUB_UPGRADE_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                {ModuleContract.DocAvePlatform.StorageOptimization.Extender.SO_STUB_DB_CONFIG_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                //Connector
                {ModuleContract.DocAvePlatform.StorageOptimization.Connector.CONNECTOR_SYNC_JOB_DTO_TYPE, DocAveControlService.StorageOptimization},
                //ADM-Replicator
                {Replicator.replicator_job_dto_type ,DocAveControlService.Administration_Replicator },
                {Replicator .replicator_import_job_type ,DocAveControlService.Administration_Replicator },
                {GranularBackup.BACKUP_JOB_DTO_TYPE_Replicator ,DocAveControlService .Administration_Replicator },
                {GranularBackup .RESTORE_JOB_DTO_TYPE_Replicator ,DocAveControlService .Administration_Replicator },
                //Deployment Manager
                {DeploymentManager.JOB_TYPE_DEPLOYMENT_MANAGER ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_DESIGN_MANAGE ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_FRONTEND_DEPLOYMENT ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_SOLUTIONCENTER ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_METADATASERVICE ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_EXCEL_UPLOAD ,DocAveControlService.Administration_DeploymentManager },
                {DeploymentManager.JOB_TYPE_DEPLOYMENT_MANAGERBACKUP ,DocAveControlService.Administration_DeploymentManager },
                //Content Manager
                {ModuleContract.DocAvePlatform.Administration.ContentManager.CONTENTMANAGER_JOB_DTO_TYPE, DocAveControlService.Administration_ContentManager },
                {ModuleContract.DocAvePlatform.Administration.ContentManager.CONTENTMANAGER_EXPORT_JOB_DTO_TYPE, DocAveControlService.Administration_ContentManager },
                {ModuleContract.DocAvePlatform.Administration.ContentManager.CONTENTMANAGER_IMPORT_JOB_DTO_TYPE, DocAveControlService.Administration_ContentManager },
                {ModuleContract.DocAvePlatform.Administration.ContentManager.CONTENTMANAGER_BACKUP_JOB_DTO_TYPE, DocAveControlService.Administration_ContentManager },
                {ModuleContract.DocAvePlatform.Administration.ContentManager.CONTENTMANAGER_RESTORE_JOB_DTO_TYPE, DocAveControlService.Administration_ContentManager },
                //Log Manager
                {ModuleContract.DocAvePlatform.ControlPanel.LogManager.JOB_TYPE_LOG_MANAGER ,DocAveControlService.ControlPanel_LogManager },
                //Job Pruning
                {ModuleContract.DocAvePlatform.ControlPanel.JobPruning.JOB_TYPE_JOB_PRUNING ,DocAveControlService.ControlPanel_JobPruning },
                //report center
                {(int)JobTypes.RCCollectorJob,DocAveControlService.ReportCenter},
            };
        }
    }
}

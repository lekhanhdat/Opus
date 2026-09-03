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
using System.Text;

namespace AvePoint.Common
{
    public class AgentConstants
    {
        public class AgentBinaryName
        {
            public static readonly string SERVICE_NAME = "DocAve 6 Agent Service";
            public static readonly string SERVICE_DISPLAY_NAME = "DocAve 6 Agent Service";
            public static readonly string SERVICE_DESCRIPTION = "DocAve 6 Manager and Agent Communication Interface";
            public static readonly string SERVICE_DISPLAY_NAME_SMSP = "SMSP 7 Agent Service";
            public static readonly string SERVICE_DESCRIPTION_SMSP = "SMSP 7 Manager and Agent Communication Interface";
            public static readonly string SERVICE_EXE_NAME = "AgentService.exe";
            public static readonly string POSTINSTALL_EXE_NAME = "AgentCommonPostInstall.exe";
            public static readonly string GET_FARM_ID_2003_EXE_NAME = "DocAve.SP2003.GetFarmId.exe";
            public static readonly string COMMON_GET_FARM_ID_EXE_NAME = "AgentCommonGetFarmID.exe";
            //public static readonly string RESTART_SERVICE_EXE_NAME = "AgentCommonRestartService.exe";
            public static readonly string COMMON_BROWSER_NAME = "AgentCommonBrowser";
            public static readonly string RETENTION_Name = "AgentCommonRetention";
            public static readonly string MEDIAMANAGEMENT_Name = "AgentCommonMediaManagement";
            public static readonly string MIGRATION_BROWSER_NAME = "AgentCommonMigrationBrowser";
            public static readonly string MIGRATION_FM_BACKUP_2010 = "FileSystemMigrationWorker.exe";
            public static readonly string MIGRATION_FM_RESTORE_2010 = "FileSystemMigrationRestore.exe";
            public static readonly string MIGRATION_FM_EXCELBUILDER_2010 = "FileSystemMigrationExcelBuilder.exe";
            public static readonly string CA_Worker_NAME = "SP2010CentralAdminWorker";
            public static readonly string CA_JobWorker_NAME = "SP2010CentralAdminJobWorker";
            public static readonly string CONFIG_FILE_SYNC_NAME = "AgentToolSyncConfigFile.exe";
            public static readonly string REPORT_CENTER_EXE_NAME = "SP2010ReportCenter";
            public static readonly string File_Uploader_EXE_NAME = "AgentCommonFileUploader";
            public static readonly string ContentManager_PRIMARY_2010 = "SP2010CMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_2010 = "SP2010CMAppHostSecondary.exe";
            public static readonly string AgentHotfixMaintenanceService = "AgentHotfixMaintenanceService.exe";
            //public static readonly string DesignManager_PRIMARY_2010 = "SP2010DMAppHostPrimary.exe";
            //public static readonly string DesignManager_SECONDARY_2010 = "SP2010DMAppHostSecondary.exe";
            //public static readonly string WFEDeployManager_PRIMARY_2010 = "SP2010WFEDMAppHostPrimary.exe";
            //public static readonly string WFEDeployManager_SECONDARY_2010 = "SP2010WFEDMAppHostSecondary.exe";
            //public static readonly string SCDeployManager_PRIMARY_2010 = "SP2010SCDMAppHostPrimary.exe";
            //public static readonly string SCDeployManager_SECONDARY_2010 = "SP2010SCDMAppHostSecondary.exe";
            //public static readonly string ManagedMetadataService_PRIMARY_2010 = "SP2010MMSAppHostPrimary.exe";
            //public static readonly string ManagedMetadataService_SECONDARY_2010 = "SP2010MMSAppHostSecondary.exe";
            public static readonly string COMMON_MIGRATION_BROWSER_NAME = "AgentCommonMigrationBrowser.exe";
            public static readonly string OD4B_Backup_NAME = "OneDriveForBusinessBackup.exe";
            public static readonly string Item_Backup_NAME = "SP2010GranularBackup.exe";
            public static readonly string Item_Restore_NAME = "SP2010GranularRestore.exe";
            public static readonly string RP_PRIMARY_2010 = "SP2010ReplicatorPrimary.exe";
            public static readonly string RP_SECONDARY_2010 = "SP2010ReplicatorSecondary.exe";
            public static readonly string RP_OFFLINE_2010 = "SP2010ReplicatorOffline.exe";
            public static readonly string RP_LISTENER_2010 = "SP2010ReplicatorListener.exe";
            public static readonly string RP_ANALYZER_2010 = "SP2010ReplicatorAnalyzer.exe";
            public static readonly string RP_EVENTHANDLER_2010 = "SP2010ReplicatorEventHandler.dll";
            public static readonly string RP_TOOL_2010 = "SP2010ReplicatorTool.exe";
            public static readonly string RP_PRIMARY_2007 = "SP2007ReplicatorPrimary.exe";
            public static readonly string RP_SECONDARY_2007 = "SP2007ReplicatorSecondary.exe";
            public static readonly string RP_OFFLINE_2007 = "SP2007ReplicatorOffline.exe";
            public static readonly string RP_LISTENER_2007 = "SP2007ReplicatorListener.exe";
            public static readonly string RP_EVENTHANDLER_2007 = "SP2007ReplicatorEventHandler.dll";
            public static readonly string RP_TOOL_2007 = "SP2007ReplicatorTool.exe";
            public static readonly string OFFICE365_SERVER_EXE_NAME = "SP2010Office365Service";
            public static readonly string SO_SERVICE_2010 = "SP2010StorageOptimizationService";
            public static readonly string SO_PROCESSOR_2010 = "SP2010StorageProcessor";
            public static readonly string SOProcessingPool = "AgentCommonProcessingPool.exe";
            public static readonly string SO_MessageCenter = "SPStorageOptimizationMessageCenter.exe";
            public static readonly string PR_BROWSER_NAME = "AgentCommonPRBrowser.exe";
            public static readonly string PR_LIVEMODE_BROWSER_NAME = "AgentCommonPRLiveModeBrowser.exe";
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME = "SP2010PRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME = "SP2010PRDisasterRecoveryMember.exe";
            public static readonly string PR_COMMONUTILITY = "AgentCommonPRCommonUtility.dll";
            public static readonly string PR_COMMON_2010 = "SP2010PRCommon.dll";
            public static readonly string PR_COMMON_2007 = "SP2007PRCommon.dll";
            public static readonly string PR_SPUTILITY_2010 = "SP2010PRSPUtility.dll";
            public static readonly string PR_SPUTILITY_2007 = "SP2007PRSPUtility.dll";
            public static readonly string PR_CONTROLBACKUP_2010 = "SP2010PRControlBackup.exe";
            public static readonly string PR_CONTROLBACKUP_2007 = "SP2007PRControlBackup.exe";
            public static readonly string PR_CONTROLRESTORE_2010 = "SP2010PRControlRestore.exe";
            public static readonly string PR_CONTROLRESTORE_2007 = "SP2007PRControlRestore.exe";
            public static readonly string PR_INDEX_CONTROL_2010 = "SP2010PRIndexControl.dll";
            public static readonly string PR_INDEX_BACKUP_2010 = "SP2010PRIndexBackup.exe";
            public static readonly string PR_INDEX_BACKUP_2007 = "SP2007PRIndexBackup.exe";
            public static readonly string PR_INDEX_RESTORE_2010 = "SP2010PRIndexRestore.exe";
            public static readonly string PR_INDEX_RESTORE_2007 = "SP2007PRIndexRestore.exe";
            public static readonly string PR_WFE_BACKUP_2010 = "SP2010PRWFEBackup.exe";
            public static readonly string PR_WFE_BACKUP_2007 = "SP2007PRWFEBackup.exe";
            public static readonly string PR_WFE_RESTORE_2010 = "SP2010PRWFERestore.exe";
            public static readonly string PR_WFE_RESTORE_2007 = "SP2007PRWFERestore.exe";
            public static readonly string PR_WFE_BROWSER_2010 = "SP2010PRWFEBrowser.exe";
            public static readonly string PR_WFE_BROWSER_2007 = "SP2007PRWFEBrowser.exe";
            public static readonly string PR_VDIDBCONTROL = "AgentCommonPRVDIDBControl.dll";
            public static readonly string PR_VDIDBBACKUP = "AgentCommonPRVDIDBBackup.exe";
            public static readonly string PR_VDIDBRESTORE = "AgentCommonPRVDIDBRestore.exe";
            public static readonly string PR_VSSCONTROL = "AgentCommonPRVSSControl.dll";
            public static readonly string PR_VSSBACKUP = "AgentCommonPRVSSBackup.exe";
            public static readonly string PR_VSSESTORE = "AgentCommonPRVSSRestore.exe";
            public static readonly string PR_VSSDRIVER = "AgentCommonPRVSSDriver.dll";
            public static readonly string PR_NETAPPCONTROL = "AgentCommonPRNativeControl.dll";
            public static readonly string PR_NETAPPLUNCHECKER = "AgentCommonPRNativeLunChecker.exe";
            public static readonly string PR_NETAPPBACKUP = "AgentCommonPRNativeBackup.exe";
            public static readonly string PR_NETAPPRESTORE = "AgentCommonPRNativeRestore.exe";
            public static readonly string PR_ITEM_RESTORE_2010 = "SP2010PlatformItemRestore.exe";
            public static readonly string PR_MULTIPLE_CONTROL = "AgentCommonPRMultipleControl.exe";
            public static readonly string PR_JOB_STOP_FLAG = "PRJobStop.cmd";
            public static readonly string RC_Auditor_2010 = "SP2010RCAuditor.exe";
            public static readonly string CONNECTOR_PROCESSOR_2010 = "SP2010ConnectorProcessor.exe";
            public static readonly string PR_MULTIPLE_MEMBER_2010 = "AgentCommonPRMultipleMember.exe";
            public static readonly string CP_SolutionManager_2010 = "SP2010SolutionManager.exe";
            public static readonly string CP_SolutionManager_2007 = "SP2007SolutionManager.exe";
            public static readonly string CPL_EDSEARCH_2010_EXE_NAME = "SP2010eDiscoverySearch";
            public static readonly string CPL_EDHOLD_2010_EXE_NAME = "SP2010eDiscoveryHold";
            public static readonly string CPL_EDEXPORT_2010_EXE_NAME = "SP2010eDiscoveryExport";
            public static readonly string CPL_EDOFFLINESEARCH_2010_EXE_NAME = "SP2010eDiscoveryOfflineSearch";
            public static readonly string SP_07To10Migration_2010 = "SP2007To2010Migration.exe";
            public static readonly string COMMONLUMMONITOR_EXE_NAME = "AgentCommonLunMonitor.exe";
            public static readonly string CloudAppAdminWCFWorker = "AzureADWCFManagement.exe";


            #region << Deployment manager >>
            public static readonly string DeploymentManager_PRIMARY = "DeploymentManagerPrimary.exe";
            public static readonly string DeploymentManager_SECONDARY = "DeploymentManagerSecondary.exe";
            //[Obsolete]
            //public static readonly string DesignManager_PRIMARY_COMPARE_2010 = "AgentCommon2010ComparePrimary.exe";
            //public static readonly string DesignManager_SECONDARY_COMPARE_2010 = "AgentCommon2010CompareSecondary.exe";
            //public static readonly string DesignManager_PRIMARY_COMPARE_2013 = "AgentCommon2013ComparePrimary.exe";
            //public static readonly string DesignManager_SECONDARY_COMPARE_2013 = "AgentCommon2013CompareSecondary.exe";
            //public static readonly string DesignManager_AppUpdate2013 = "SP2013AppUpdateProcessor.exe";
            //public static readonly string DesignManager_PRIMARY_2010 = "SP2010DMAppHostPrimary.exe";
            //public static readonly string DesignManager_PRIMARY_2013 = "SP2013DMAppHostPrimary.exe";
            //public static readonly string DesignManager_SECONDARY_2010 = "SP2010DMAppHostSecondary.exe";
            //public static readonly string DesignManager_SECONDARY_2013 = "SP2013DMAppHostSecondary.exe";
            //public static readonly string WFEDeployManager_PRIMARY_2010 = "SP2010WFEDMAppHostPrimary.exe";
            //public static readonly string WFEDeployManager_PRIMARY_2013 = "SP2013WFEDMAppHostPrimary.exe";
            //public static readonly string WFEDeployManager_SECONDARY_2010 = "SP2010WFEDMAppHostSecondary.exe";
            //public static readonly string WFEDeployManager_SECONDARY_2013 = "SP2013WFEDMAppHostSecondary.exe";
            //public static readonly string SCDeployManager_PRIMARY_2010 = "SP2010SCDMAppHostPrimary.exe";
            //public static readonly string SCDeployManager_PRIMARY_2013 = "SP2013SCDMAppHostPrimary.exe";
            //public static readonly string SCDeployManager_SECONDARY_2010 = "SP2010SCDMAppHostSecondary.exe";
            //public static readonly string SCDeployManager_SECONDARY_2013 = "SP2013SCDMAppHostSecondary.exe";
            //public static readonly string ManagedMetadataService_PRIMARY_2010 = "SP2010MMSAppHostPrimary.exe";
            //public static readonly string ManagedMetadataService_PRIMARY_2013 = "SP2013MMSAppHostPrimary.exe";
            //public static readonly string ManagedMetadataService_SECONDARY_2010 = "SP2010MMSAppHostSecondary.exe";
            //public static readonly string ManagedMetadataService_SECONDARY_2013 = "SP2013MMSAppHostSecondary.exe";
            #endregion

            #region eRoom migration
            public static readonly string MIGRATION_EROOM_BACKUP = "eRoomMigrationWorker.exe";
            public static readonly string MIGRATION_EROOM_RESTORE_2010 = "eRoomMigrationRestore.exe";
            #endregion

            #region Livelink migration
            public static readonly string MIGRATION_LIVELINK_BACKUP = "LivelinkMigrationWorker.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_2010 = "LivelinkMigrationRestore.exe";
            #endregion

            #region EMC Documentum migration
            public static readonly string MIGRATION_DOCUMENTUM_BACKUP = "DocumentumMigrationWorker.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_2010 = "DocumentumMigrationRestore.exe";
            #endregion

            #region Notes Migration
            public static readonly string MIGRATION_Notes_BACKUP = "NotesMigrationWorker";
            public static readonly string MIGRATION_Notes_RESTORE_2010 = "NotesMigrationRestore.exe";
            #endregion

            #region Public Folder
            public static readonly string MIGRATION_PFBACKUP_2010 = "PublicFolderMigrationBackup.exe";
            public static readonly string MIGRATION_PFRESTORE_2010 = "PublicFolderMigrationRestore.exe";
            public static readonly string MIGRATION_PFRESETSOURCE = "PublicFolderMigrationPostWorker.exe";
            #endregion

            #region -- Common Binary--
            public static readonly string GCOMMON_CONTRACT = "CommonContract.dll";
            public static readonly string GCOMMON_UTILITY = "CommonUtility.dll";
            public static readonly string GCOMMON_MICROKERNEL = "CommonMicroKernel.dll";
            public static readonly string GCOMMON_NETWORK = "CommonNetwork.dll";
            public static readonly string GCOMMON_FILESENDER = "CommonFileSender.dll";
            public static readonly string GCOMMON_FILERECEIVER = "CommonFileReceiver.dll";
            public static readonly string COMMON_UTILITY = "AgentCommonUtility.dll";
            public static readonly string LOG4NET = "log4net.dll";
            public static readonly string MINI_COMMON = "AgentCommonMiniUtility.dll";
            #endregion

            #region Exchange online backup & restore
            public static readonly string OFFICE365EXCHANGEBROWSER = "ExchangeCommonBrowser";
            public static readonly string OFFICE365EXCHANGEBACKUP = "ExchangeOnlineBackup.exe";
            public static readonly string OFFICE365EXCHANGERESTORE = "ExchangeOnlineRestore.exe";
            #endregion

            #region
            public static readonly string AccountChangedFlagFile = "CurrentAccount.dat";
            #endregion
        }

        public class AgentConfigurationFileName
        {
            public static readonly string AgentConfigFile_VCEnvConfig = "AgentCommonVCEnv.config";
            public static readonly string AgentConfigFile_ServiceVersionConfig = "ServiceVersion.config";
            public static readonly string AgentCommonLanguageMappingFile = @"\data\SP2010\WrapperCommon\SP2010WrapperLanguageMapping.xml";
            public static readonly string AgentImportTreeFile = "AgentImportTree.xml";
            public static readonly string AgentConfigFile_Log4netConfig = "AgentLog4net.config";
            public static readonly string AgentConfigFile_SP2010CentralAdminWorker = "SP2010CentralAdminWorker.exe.config";
        }

        public class AgentFolderName
        {
            public static readonly string AgentReplicator2010ExportFolder = "Replicator2010";
            public static readonly string AgentContentManager2010ExportFolder = "ContentManager2010";
            public static readonly string AgentDesignManager2010ExportFolder = "DesignManager2010";
            public static readonly string AgentLiveLinkExportFolder = "LiveLink Exported Data";
            public static readonly string AgenteRoomExportFolder = "eRoom Exported Data";
            public static readonly string AgentLotusNotesExportFolder = "Lotus Notes Exported Data";
            public static readonly string AgentEmcDocumentumExportFolder = "EMC Documentum Exported Data";
            public static readonly string AgentHSMExportDocAveDedicatedFolder = "docavededicated";
        }

        public class AgentJobType
        {
            public const int BACKUP_JOB_DTO_TYPE = 1;
            public const int CA_SEARCH_JOB_DTO_TYPE = 2;
            public const int CA_JOB_DTO_TYPE = 3;
            public const int CONTENTMANAGER_JOB_DTO_TYPE = 7;

            public static IDictionary<int, string> JobIdPrefixes = new Dictionary<int, string>()
            {
                {BACKUP_JOB_DTO_TYPE, "BK"},
                {CA_SEARCH_JOB_DTO_TYPE, "AS"},
                {CA_JOB_DTO_TYPE, "CA"}
            };
        }

    }

    public class GlobalFlags
    {
        public static bool ServiceFullyStarted = false;
    }
}
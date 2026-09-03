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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AvePoint.Common
{
    public class AgentConstants
    {
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Existed DLL Name.")]
        public class AgentBinaryName
        {
            public static readonly string SERVICE_NAME = "DocAve 6 Agent Service";
            public static readonly string SERVICE_DISPLAY_NAME = "DocAve 6 Agent Service";
            public static readonly string SERVICE_DESCRIPTION = "DocAve 6 Manager and Agent Communication Interface";
            public static readonly string SERVICE_DISPLAY_NAME_SMSP = "SMSP 8 Agent Service";
            public static readonly string SERVICE_DESCRIPTION_SMSP = "SMSP 8 Manager and Agent Communication Interface";
            public static readonly string SERVICE_EXE_NAME = "AgentService.exe";
            public static readonly string POSTINSTALL_EXE_NAME = "AgentCommonPostInstall.exe";
            public static readonly string GET_FARM_ID_2003_EXE_NAME = "DocAve.SP2003.GetFarmId.exe";
            public static readonly string COMMON_GET_FARM_ID_EXE_NAME = "AgentCommonGetFarmID.exe";
            //public static readonly string RESTART_SERVICE_EXE_NAME = "AgentCommonRestartService.exe";
            public static readonly string COMMON_ROLECHECKER_2010 = "SP2010AgentCommonRoleChecker.exe";
            public static readonly string COMMON_ROLECHECKER_2013 = "AgentCommonSPRoleChecker.exe";//SP2013 &SP2016 use same one
            public static readonly string COMMON_BROWSER_NAME = "AgentCommonBrowser";
            public static readonly string COMMON_BROWSER_NAME2013 = "SP2013AgentCommonBrowser";
            public static readonly string COMMON_BROWSER_NAME2016 = "SP2016AgentCommonBrowser";
            public static readonly string COMMON_BROWSER_NAME2019 = "SP2019AgentCommonBrowser";
            public static readonly string COMMON_BROWSER_NAMESE = "SPSEAgentCommonBrowser";
            public static readonly string COMMON_AUTOSCAN_NAME = "AgentCommonAutoScan.exe";
            public static readonly string COMMON_USERSEAT_NAME = "AgentCommonUserSeat.exe";
            public static readonly string COMMON_USERSEAT_NAME2013 = "AgentCommonUserSeat2013.exe";
            public static readonly string COMMON_USERSEAT_NAMESE = "AgentCommonUserSeatSE.exe";
            public static readonly string COMMON_APIUtility_Name = "AgentCommonAPIUtility";
            public static readonly string COMMON_APIUtility_Name2013 = "SP2013AgentCommonAPIUtility";
            public static readonly string MIGRATION_BROWSER_NAME = "AgentCommonMigrationBrowser";
            public static readonly string CA_Worker_NAME = "SP2010CentralAdminWorker";
            public static readonly string CA_Worker_NAME_2013 = "SP2013CentralAdminWorker";
            public static readonly string CA_Worker_NAME_2016 = "SP2016CentralAdminWorker";
            public static readonly string CA_Worker_NAME_2019 = "SP2019CentralAdminWorker";
            public static readonly string CA_Worker_NAME_SE = "SPSECentralAdminWorker";
            public static readonly string CONFIG_FILE_SYNC_NAME = "AgentToolSyncConfigFile.exe";
            public static readonly string REPORT_CENTER_EXE_NAME = "SP2010ReportCenter";
            public static readonly string REPORT_CENTER_EXE_NAME_2013 = "SP2013ReportCenter";
            public static readonly string REPORT_CENTER_EXE_NAME_2016 = "SP2016ReportCenter";
            public static readonly string REPORT_CENTER_EXE_NAME_2019 = "SP2019ReportCenter";
            public static readonly string REPORT_CENTER_EXE_NAME_SE = "SPSEReportCenter";
            public static readonly string REPORT_CENTER_USAGE_LISTENER_SE = "SPSEReportCenterUsagePatternListener";
            public static readonly string REPORT_CENTER_USAGE_LISTENER_2019 = "SP2019ReportCenterUsagePatternListener";
            public static readonly string REPORT_CENTER_USAGE_LISTENER_2016 = "SP2016ReportCenterUsagePatternListener";
            public static readonly string REPORT_CENTER_USAGE_LISTENER_2013 = "SP2013ReportCenterUsagePatternListener";
            public static readonly string REPORT_CENTER_USAGE_LISTENER_2010 = "SP2010ReportCenterUsagePatternListener";
            public static readonly string REPORT_CENTER_Auditor_2010 = "SP2010RCAuditor";
            public static readonly string REPORT_CENTER_Auditor_2013 = "SP2013RCAuditor";
            public static readonly string REPORT_CENTER_Auditor_2016 = "SP2016RCAuditor";
            public static readonly string REPORT_CENTER_Auditor_2019 = "SP2019RCAuditor";
            public static readonly string REPORT_CENTER_Auditor_SE = "SPSERCAuditor";
            public static readonly string ContentManager_PRIMARY_2010 = "SP2010CMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_2010 = "SP2010CMAppHostSecondary.exe";
            public static readonly string ContentManager_PRIMARY_2013 = "SP2013CMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_2013 = "SP2013CMAppHostSecondary.exe";
            public static readonly string ContentManager_PRIMARY_2016 = "SP2016CMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_2016 = "SP2016CMAppHostSecondary.exe";
            public static readonly string ContentManager_PRIMARY_2019 = "SP2019CMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_2019 = "SP2019CMAppHostSecondary.exe";
            public static readonly string ContentManager_PRIMARY_SE = "SPSECMAppHostPrimary.exe";
            public static readonly string ContentManager_SECONDARY_SE = "SPSECMAppHostSecondary.exe";

            #region << Deployment manager >>
            public static readonly string DesignManager_PRIMARY_COMPARE_2010 = "AgentCommon2010ComparePrimary.exe";
            public static readonly string DesignManager_SECONDARY_COMPARE_2010 = "AgentCommon2010CompareSecondary.exe";
            public static readonly string DesignManager_PRIMARY_COMPARE_2013 = "AgentCommon2013ComparePrimary.exe";
            public static readonly string DesignManager_SECONDARY_COMPARE_2013 = "AgentCommon2013CompareSecondary.exe";
            public static readonly string DesignManager_PRIMARY_COMPARE_2016 = "AgentCommon2016ComparePrimary.exe";
            public static readonly string DesignManager_SECONDARY_COMPARE_2016 = "AgentCommon2016CompareSecondary.exe";
            public static readonly string DesignManager_PRIMARY_COMPARE_2019 = "AgentCommon2019ComparePrimary.exe";
            public static readonly string DesignManager_SECONDARY_COMPARE_2019 = "AgentCommon2019CompareSecondary.exe";
            public static readonly string DesignManager_PRIMARY_COMPARE_SE = "AgentCommonSPSEComparePrimary.exe";
            public static readonly string DesignManager_SECONDARY_COMPARE_SE = "AgentCommonSPSECompareSecondary.exe";
            public static readonly string DesignManager_AppUpdate2013 = "SP2013AppUpdateProcessor.exe";
            public static readonly string DesignManager_AppUpdate2016 = "SP2016AppUpdateProcessor.exe";
            public static readonly string DesignManager_AppUpdate2019 = "SP2019AppUpdateProcessor.exe";
            public static readonly string DesignManager_AppUpdateSE = "SPSEAppUpdateProcessor.exe";
            public static readonly string DesignManager_PRIMARY_2010 = "SP2010DMAppHostPrimary.exe";
            public static readonly string DesignManager_SECONDARY_2010 = "SP2010DMAppHostSecondary.exe";
            public static readonly string DesignManager_PRIMARY_2013 = "SP2013DMAppHostPrimary.exe";
            public static readonly string DesignManager_SECONDARY_2013 = "SP2013DMAppHostSecondary.exe";
            public static readonly string DesignManager_PRIMARY_2016 = "SP2016DMAppHostPrimary.exe";
            public static readonly string DesignManager_SECONDARY_2016 = "SP2016DMAppHostSecondary.exe";
            public static readonly string DesignManager_PRIMARY_2019 = "SP2019DMAppHostPrimary.exe";
            public static readonly string DesignManager_SECONDARY_2019 = "SP2019DMAppHostSecondary.exe";
            public static readonly string DesignManager_PRIMARY_SE = "SPSEDMAppHostPrimary.exe";
            public static readonly string DesignManager_SECONDARY_SE = "SPSEDMAppHostSecondary.exe";
            public static readonly string WFEDeployManager_PRIMARY_2010 = "SP2010WFEDMAppHostPrimary.exe";
            public static readonly string WFEDeployManager_SECONDARY_2010 = "SP2010WFEDMAppHostSecondary.exe";
            public static readonly string WFEDeployManager_PRIMARY_2013 = "SP2013WFEDMAppHostPrimary.exe";
            public static readonly string WFEDeployManager_SECONDARY_2013 = "SP2013WFEDMAppHostSecondary.exe";
            public static readonly string WFEDeployManager_PRIMARY_2016 = "SP2016WFEDMAppHostPrimary.exe";
            public static readonly string WFEDeployManager_SECONDARY_2016 = "SP2016WFEDMAppHostSecondary.exe";
            public static readonly string WFEDeployManager_PRIMARY_2019 = "SP2019WFEDMAppHostPrimary.exe";
            public static readonly string WFEDeployManager_SECONDARY_2019 = "SP2019WFEDMAppHostSecondary.exe";
            public static readonly string WFEDeployManager_PRIMARY_SE = "SPSEWFEDMAppHostPrimary.exe";
            public static readonly string WFEDeployManager_SECONDARY_SE = "SPSEWFEDMAppHostSecondary.exe";
            public static readonly string SCDeployManager_PRIMARY_2010 = "SP2010SCDMAppHostPrimary.exe";
            public static readonly string SCDeployManager_SECONDARY_2010 = "SP2010SCDMAppHostSecondary.exe";
            public static readonly string SCDeployManager_PRIMARY_2013 = "SP2013SCDMAppHostPrimary.exe";
            public static readonly string SCDeployManager_SECONDARY_2013 = "SP2013SCDMAppHostSecondary.exe";
            public static readonly string SCDeployManager_PRIMARY_2016 = "SP2016SCDMAppHostPrimary.exe";
            public static readonly string SCDeployManager_SECONDARY_2016 = "SP2016SCDMAppHostSecondary.exe";
            public static readonly string SCDeployManager_PRIMARY_2019 = "SP2019SCDMAppHostPrimary.exe";
            public static readonly string SCDeployManager_SECONDARY_2019 = "SP2019SCDMAppHostSecondary.exe";
            public static readonly string SCDeployManager_PRIMARY_SE = "SPSESCDMAppHostPrimary.exe";
            public static readonly string SCDeployManager_SECONDARY_SE = "SPSESCDMAppHostSecondary.exe";
            public static readonly string ManagedMetadataService_PRIMARY_2010 = "SP2010MMSAppHostPrimary.exe";
            public static readonly string ManagedMetadataService_SECONDARY_2010 = "SP2010MMSAppHostSecondary.exe";
            public static readonly string ManagedMetadataService_PRIMARY_2013 = "SP2013MMSAppHostPrimary.exe";
            public static readonly string ManagedMetadataService_SECONDARY_2013 = "SP2013MMSAppHostSecondary.exe";
            public static readonly string ManagedMetadataService_PRIMARY_2016 = "SP2016MMSAppHostPrimary.exe";
            public static readonly string ManagedMetadataService_SECONDARY_2016 = "SP2016MMSAppHostSecondary.exe";
            public static readonly string ManagedMetadataService_PRIMARY_2019 = "SP2019MMSAppHostPrimary.exe";
            public static readonly string ManagedMetadataService_SECONDARY_2019 = "SP2019MMSAppHostSecondary.exe";
            public static readonly string ManagedMetadataService_PRIMARY_SE = "SPSEMMSAppHostPrimary.exe";
            public static readonly string ManagedMetadataService_SECONDARY_SE = "SPSEMMSAppHostSecondary.exe";
            #endregion
            #region Granular Backup & Restore
            public static readonly string Item_Backup_NAME2007 = "SP2007GranularBackup.exe";
            public static readonly string Item_Backup_NAME = "SP2010GranularBackup.exe";
            public static readonly string Item_Restore_NAME = "SP2010GranularRestore.exe";
            public static readonly string Item_Backup_NAME2013 = "SP2013GranularBackup.exe";
            public static readonly string Item_Restore_NAME2013 = "SP2013GranularRestore.exe";
            public static readonly string Item_Backup_NAME2016 = "SP2016GranularBackup.exe";
            public static readonly string Item_Restore_NAME2016 = "SP2016GranularRestore.exe";
            public static readonly string Item_Backup_NAME2019 = "SP2019GranularBackup.exe";
            public static readonly string Item_Restore_NAME2019 = "SP2019GranularRestore.exe";
            public static readonly string Item_Backup_NAMESPSE = "SPSEGranularBackup.exe";
            public static readonly string Item_Restore_NAMESPSE = "SPSEGranularRestore.exe";
            #endregion
            public static readonly string COMMON_MIGRATION_BROWSER_NAME = "AgentCommonMigrationBrowser.exe";
            public static readonly string RP_PRIMARY_2010 = "SP2010ReplicatorPrimary.exe";
            public static readonly string RP_PRIMARY_2013 = "AgentCommonReplicatorPrimary.exe";
            public static readonly string RP_SECONDARY_2010 = "SP2010ReplicatorSecondary.exe";
            public static readonly string RP_SECONDARY_2013 = "AgentCommonReplicatorSecondary.exe";
            public static readonly string RP_OFFLINE_2010 = "SP2010ReplicatorOffline.exe";
            public static readonly string RP_OFFLINE_2013 = "AgentCommonReplicatorOffline.exe";
            public static readonly string RP_LISTENER_2010 = "SP2010ReplicatorListener.exe";
            public static readonly string RP_LISTENER_2013 = "SP2013ReplicatorListener.exe";
            public static readonly string RP_ANALYZER_2010 = "SP2010ReplicatorAnalyzer.exe";
            public static readonly string RP_ANALYZER_2013 = "AgentCommonReplicatorAnalyzer.exe";
            public static readonly string RP_REPLICATOR_SERVICE = "AgentCommonReplicatorService";
            public static readonly string RP_REPLICATOR_WORKER_2010 = "SP2010ReplicatorWorker";
            public static readonly string RP_REPLICATOR_WORKER_2013 = "AgentCommonReplicatorWorker";
            public static readonly string RP_EVENTHANDLER_2010 = "SP2010ReplicatorEventHandler.dll";
            public static readonly string RP_EVENTHANDLER_2013 = "SP2013ReplicatorEventHandler.dll";
            public static readonly string RP_EVENTHANDLER_2016 = "SP2016ReplicatorEventHandler.dll";
            public static readonly string RP_EVENTHANDLER_2019 = "SP2019ReplicatorEventHandler.dll";
            public static readonly string RP_EVENTHANDLER_SE = "SPSEReplicatorEventHandler.dll";
            public static readonly string RP_TOOL_2010 = "SP2010ReplicatorTool.exe";
            public static readonly string RP_PRIMARY_2007 = "SP2007ReplicatorPrimary.exe";
            public static readonly string RP_SECONDARY_2007 = "SP2007ReplicatorSecondary.exe";
            public static readonly string RP_OFFLINE_2007 = "SP2007ReplicatorOffline.exe";
            public static readonly string RP_LISTENER_2007 = "SP2007ReplicatorListener.exe";
            public static readonly string RP_EVENTHANDLER_2007 = "SP2007ReplicatorEventHandler.dll";
            public static readonly string RP_TOOL_2007 = "SP2007ReplicatorTool.exe";
            public static readonly string OFFICE365_SERVER_EXE_NAME = "SP2010Office365Service";
            public static readonly string SO_SERVICE_2010 = "SP2010StorageOptimizationService";
            public static readonly string SO_SERVICE_2013 = "SP2013StorageOptimizationService";
            public static readonly string SO_SERVICE_2016 = "SP2016StorageOptimizationService";
            public static readonly string SO_SERVICE_2019 = "SP2019StorageOptimizationService";
            public static readonly string SO_SERVICE_SE = "SPSEStorageOptimizationService";
            public static readonly string SO_PROCESSOR_2010 = "SP2010StorageProcessor";
            public static readonly string SO_PROCESSOR_2013 = "SP2013StorageProcessor";
            public static readonly string SO_PROCESSOR_2016 = "SP2016StorageProcessor";
            public static readonly string SO_PROCESSOR_2019 = "SP2019StorageProcessor";
            public static readonly string SO_PROCESSOR_SE = "SPSEStorageProcessor";
            public static readonly string SOProcessingPool = "AgentCommonProcessingPool.exe";
            public static readonly string PREVIEW_SERVICE_2010 = "SP2010PreviewService";
            public static readonly string PREVIEW_SERVICE_2013 = "SP2013PreviewService";

            #region Platform Backup & Restore
            //public static readonly string PR_BROWSER_2016 = "SP2016PRBrowser.exe";
            //public static readonly string PR_BROWSER_2013 = "SP2013PRBrowser.exe";
            public static readonly string PR_Common_BROWSER = "AgentCommonPRBrowser.exe";
            public static readonly string PR_BROWSER_2010 = "SP2010PRBrowser.exe";
            public static readonly string PR_LIVEMODE_BROWSER_NAME = "AgentCommonPRLiveModeBrowser.exe";
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME_2010 = "SP2010PRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME_2010 = "SP2010PRDisasterRecoveryMember.exe";
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME_2013 = "SP2013PRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME_2013 = "SP2013PRDisasterRecoveryMember.exe";
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME_2016 = "SP2016PRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME_2016 = "SP2016PRDisasterRecoveryMember.exe";
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME_2019 = "SP2019PRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME_2019 = "SP2019PRDisasterRecoveryMember.exe";
            public static readonly string PR_COMMONUTILITY = "AgentCommonPRCommonUtility.dll";
            public static readonly string PRWFE_COMMONCONTROL = "AgentCommonWFEControl.dll";
            //public static readonly string PR_COMMON_2016 = "SP2016PRCommon.dll";
            //public static readonly string PR_COMMON_2013 = "SP2013PRCommon.dll";
            //public static readonly string PR_COMMON_2010 = "SP2010PRCommon.dll";
            //public static readonly string PR_COMMON = "PRCommon.dll";
            public static readonly string PR_COMMON_2007 = "SP2007PRCommon.dll";
            public static readonly string PR_SPUTILITY_2019 = "SP2019DPCommonSPUtility.dll";
            public static readonly string PR_SPUTILITY_2016 = "SP2016DPCommonSPUtility.dll";
            public static readonly string PR_SPUTILITY_2013 = "SP2013PRSPUtility.dll";
            public static readonly string PR_SPUTILITY_2010 = "SP2010PRSPUtility.dll";
            public static readonly string PR_SPUTILITY_2007 = "SP2007PRSPUtility.dll";
            public static readonly string PR_CONTROLBACKUP_2019 = "SP2019PRControlBackup.exe";
            public static readonly string PR_CONTROLBACKUP_2016 = "SP2016PRControlBackup.exe";
            public static readonly string PR_CONTROLBACKUP_2013 = "SP2013PRControlBackup.exe";
            public static readonly string PR_CONTROLBACKUP_2010 = "SP2010PRControlBackup.exe";
            public static readonly string PR_CONTROLBACKUP_2007 = "SP2007PRControlBackup.exe";
            public static readonly string PR_CONTROLRESTORE_2019 = "SP2019PRControlRestore.exe";
            public static readonly string PR_CONTROLRESTORE_2016 = "SP2016PRControlRestore.exe";
            public static readonly string PR_CONTROLRESTORE_2013 = "SP2013PRControlRestore.exe";
            public static readonly string PR_CONTROLRESTORE_2010 = "SP2010PRControlRestore.exe";
            public static readonly string PR_CONTROLRESTORE_2007 = "SP2007PRControlRestore.exe";
            //public static readonly string PR_INDEX_CONTROL_2016 = "SP2016PRIndexControl.dll";
            public static readonly string PR_INDEX_CONTROL_2013 = "SP2013PRIndexControl.dll";
            public static readonly string PR_INDEX_CONTROL_2010 = "SP2010PRIndexControl.dll";
            //public static readonly string PR_INDEX_BACKUP_2016 = "SP2016PRIndexBackup.exe";
            public static readonly string PR_INDEX_BACKUP_2013 = "SP2013PRIndexBackup.exe";
            public static readonly string PR_INDEX_BACKUP_2010 = "SP2010PRIndexBackup.exe";
            public static readonly string PR_INDEX_BACKUP_2007 = "SP2007PRIndexBackup.exe";
            //public static readonly string PR_INDEX_RESTORE_2016 = "SP2016PRIndexRestore.exe";
            public static readonly string PR_INDEX_RESTORE_2013 = "SP2013PRIndexRestore.exe";
            public static readonly string PR_INDEX_RESTORE_2010 = "SP2010PRIndexRestore.exe";
            public static readonly string PR_INDEX_RESTORE_2007 = "SP2007PRIndexRestore.exe";
            public static readonly string PR_WFE_BACKUP_2019 = "SP2019PRWFEBackup.exe";
            public static readonly string PR_WFE_BACKUP_2016 = "SP2016PRWFEBackup.exe";
            public static readonly string PR_WFE_BACKUP_2013 = "SP2013PRWFEBackup.exe";
            public static readonly string PR_WFE_BACKUP_2010 = "SP2010PRWFEBackup.exe";
            public static readonly string PR_WFE_BACKUP_2007 = "SP2007PRWFEBackup.exe";

            public static readonly string PR_WFE_RESTORE_2019 = "SP2019PRWFERestore.exe";
            public static readonly string PR_WFE_RESTORE_2016 = "SP2016PRWFERestore.exe";
            public static readonly string PR_WFE_RESTORE_2013 = "SP2013PRWFERestore.exe";
            public static readonly string PR_WFE_RESTORE_2010 = "SP2010PRWFERestore.exe";
            public static readonly string PR_WFE_RESTORE_2007 = "SP2007PRWFERestore.exe";
            //public static readonly string PR_WFE_BROWSER_2016 = "SP2016PRWFEBrowser.exe";
            //public static readonly string PR_WFE_BROWSER_2013 = "SP2013PRWFEBrowser.exe";
            //public static readonly string PR_WFE_BROWSER_2010 = "SP2010PRWFEBrowser.exe";
            // public static readonly string PR_WFE_BROWSER_2007 = "SP2007PRWFEBrowser.exe";
            public static readonly string PR_VDIDBCONTROL = "AgentCommonPRVDIDBControl.dll";
            public static readonly string PR_VDIDBBACKUP = "AgentCommonPRVDIDBBackup.exe";
            public static readonly string PR_VDIDBRESTORE = "AgentCommonPRVDIDBRestore.exe";
            public static readonly string PR_VSSCONTROL = "AgentCommonPRVSSControl.dll";
            public static readonly string PR_VSSBACKUP = "AgentCommonPRVSSBackup.exe";
            public static readonly string PR_VSSESTORE = "AgentCommonPRVSSRestore.exe";
            public static readonly string PR_VSSDRIVER = "AgentCommonPRVSSDriver.dll";
            public static readonly string PR_VSSMEMBER = "AgentCommonPRVSSMember.dll";
            /// <summary>
            /// Not use after SMSP 7.1.1
            /// </summary>
            [Obsolete]
            public static readonly string PR_NETAPPCONTROL_2010 = "SP2010PRNativeControl.dll";
            /// <summary>
            /// Not use after SMSP 7.1.1
            /// </summary>
            [Obsolete]
            public static readonly string PR_NETAPPCONTROL_2013 = "SP2013PRNativeControl.dll";
            public static readonly string PR_NETAPPCONTROL = "AgentCommonPRNativeControl.dll";
            public static readonly string PR_NETAPPLUNCHECKER = "AgentCommonPRNativeLunChecker.exe";
            public static readonly string PR_NETAPPBACKUP = "AgentCommonPRNativeBackup.exe";
            public static readonly string PR_NETAPPRESTORE = "AgentCommonPRNativeRestore.exe";
            public static readonly string PR_ITEM_RESTORE_SE = "SPSEPlatformItemRestore.exe";
            public static readonly string PR_ITEM_RESTORE_2019 = "SP2019PlatformItemRestore.exe";
            public static readonly string PR_ITEM_RESTORE_2016 = "SP2016PlatformItemRestore.exe";
            public static readonly string PR_ITEM_RESTORE_2013 = "SP2013PlatformItemRestore.exe";
            public static readonly string PR_ITEM_RESTORE_2010 = "SP2010PlatformItemRestore.exe";
            public static readonly string PR_MULTIPLE_CONTROL_2010 = "SP2010PRMultipleControl.exe";
            public static readonly string PR_MULTIPLE_CONTROL_2013 = "SP2013PRMultipleControl.exe";
            public static readonly string PR_MULTIPLE_CONTROL_2016 = "SP2016PRMultipleControl.exe";
            public static readonly string PR_MULTIPLE_CONTROL_2019 = "SP2019PRMultipleControl.exe";
            public static readonly string PR_MULTIPLE_MEMBER_2010 = "AgentCommonPRMultipleMember.exe";
            public static readonly string PR_JOB_STOP_FLAG = "PRJobStop.pjs";
            public static readonly string PR_JOB_FORCE_STOP_FLAG = "PRJobForceStop.pjs";
            public static readonly string PR_NATIVEBROWSE_NAME = "AgentCommonSDMBrowser.exe";
            public static readonly string PR_MultipleSPUtility_2010 = "SP2010PRMultipleSPUtility.dll";
            public static readonly string PR_MultipleSPUtility_2013 = "SP2013PRMultipleSPUtility.dll";
            //public static readonly string PR_MultipleSPUtility_2016 = "SP2016PRMultipleSPUtility.dll";
            public static readonly string PR_NETAPPSPUTILITY_2010 = "SP2010PRNativeSPUtility.DLL";
            public static readonly string PR_NETAPPSPUTILITY_2013 = "SP2013PRNativeSPUtility.DLL";
            //public static readonly string PR_NETAPPSPUTILITY_2016 = "SP2016sPRNativeSPUtility.DLL";
            public static readonly string COMMONLUMMONITOR_EXE_NAME = "AgentCommonLunMonitor.exe";
            public static readonly string PR_SQLRECOVERY_MEMBERCONTROL = "AgentCommonSDMRestoreMember.exe";
            public static readonly string PR_SRMControlItemRestore_2010 = "SP2010SDMControlItemRestore.exe";
            public static readonly string PR_SRMControlItemRestore_2013 = "SP2013SDMControlItemRestore.exe";
            public static readonly string PR_SRMControlItemRestore_2016 = "SP2016SDMControlItemRestore.exe";
            public static readonly string PR_SRMControlItemRestore_2019 = "SP2019SDMControlItemRestore.exe";
            public static readonly string SDMRetentionControl = "AgentCommonSDMRetentionControl.exe";
            public static readonly string PR_VSSALTERNATE_RUNNING = "PRAlternateRunning.cmd";
            public static readonly string PR_VSSALTERNATE_FINISH = "PRAlternateFinish.cmd";

            //add from SPSE 6.13
            public static readonly string PR_DISASTERRECOVERYCONTROL_NAME_SE = "SPSEPRDisasterRecoveryRestore.exe";
            public static readonly string PR_DISASTERRECOVERYMEMBERL_NAME_SE = "SPSEPRDisasterRecoveryMember.exe";
            public static readonly string PR_SPUTILITY_SE = "SPSEDPCommonSPUtility.dll";
            public static readonly string PR_CONTROLBACKUP_SE = "SPSEPRControlBackup.exe";
            public static readonly string PR_CONTROLRESTORE_SE = "SPSEPRControlRestore.exe";
            public static readonly string PR_WFE_BACKUP_SE = "SPSEPRWFEBackup.exe";
            public static readonly string PR_WFE_RESTORE_SE = "SPSEPRWFERestore.exe";
            public static readonly string PR_MULTIPLE_CONTROL_SE = "SPSEPRMultipleControl.exe";

            #endregion Platform Backup & Restore

            public static readonly string RC_Auditor_2010 = "SP2010RCAuditor.exe";
            public static readonly string RC_Auditor_2013 = "SP2013RCAuditor.exe";
            public static readonly string RC_Auditor_2016 = "SP2016RCAuditor.exe";
            public static readonly string RC_Auditor_2019 = "SP2019RCAuditor.exe";
            public static readonly string RC_Auditor_SPSE = "SPSERCAuditor.exe";
            public static readonly string CONNECTOR_PROCESSOR_2010 = "SP2010ConnectorProcessor.exe";
            public static readonly string CONNECTOR_PROCESSOR_2013 = "SP2013ConnectorProcessor.exe";
            public static readonly string CONNECTOR_PROCESSOR_2016 = "SP2016ConnectorProcessor.exe";
            public static readonly string CONNECTOR_PROCESSOR_2019 = "SP2019ConnectorProcessor.exe";
            public static readonly string CONNECTOR_PROCESSOR_SE = "SPSEConnectorProcessor.exe";
            public static readonly string CP_SolutionManager_2010 = "SP2010SolutionManager";
            public static readonly string CP_SolutionManager_2007 = "SP2007SolutionManager";
            public static readonly string CP_SolutionManager_2013 = "SP2013SolutionManager";
            public static readonly string CP_SolutionManager_2016 = "SP2016SolutionManager";
            public static readonly string CP_SolutionManager_2019 = "SP2019SolutionManager";
            public static readonly string CP_SolutionManager_SE = "SPSESolutionManager";
            public static readonly string CPL_EDSEARCH_2010_EXE_NAME = "SP2010eDiscoverySearch";
            public static readonly string CPL_EDSEARCH_2013_EXE_NAME = "SP2013eDiscoverySearch";

            public static readonly string CPL_EDHOLD_2010_EXE_NAME = "SP2010eDiscoveryHold";
            public static readonly string CPL_EDHOLD_2013_EXE_NAME = "SP2013eDiscoveryHold";

            public static readonly string CPL_EDEXPORT_2010_EXE_NAME = "SP2010eDiscoveryExport";
            public static readonly string CPL_EDEXPORT_2013_EXE_NAME = "SP2013eDiscoveryExport";

            public static readonly string CPL_EDOFFLINESEARCH_2010_EXE_NAME = "SP2010eDiscoveryOfflineSearch";
            public static readonly string CPL_EDOFFLINESEARCH_2013_EXE_NAME = "SP2013eDiscoveryOfflineSearch";


            #region SharePoint Migration
            public static readonly string SPMigrationExport_2007 = "SP2007SPMigrationExport.exe";
            public static readonly string SPMigrationExport_2010 = "SP2010SPMigrationExport.exe";
            public static readonly string SPMigrationExport_2013 = "SP2013SPMigrationExport.exe";
            public static readonly string SPMigrationExport_2016 = "SP2016SPMigrationExport.exe";
            public static readonly string SP_07To10Migration_2010 = "SP2007To2010Migration.exe";
            public static readonly string SP_07To13Migration_2013 = "SP2007To2013Migration.exe";
            public static readonly string SP_07To16Migration_2016 = "SP2007To2016Migration.exe";
            public static readonly string SP_07To19Migration_2019 = "SP2007To2019Migration.exe";
            public static readonly string SP_10To13Migration_2013 = "SP2010To2013Migration.exe";
            public static readonly string SP_10To16Migration_2016 = "SP2010To2016Migration.exe";
            public static readonly string SP_10To19Migration_2019 = "SP2010To2019Migration.exe";
            public static readonly string SP_13To16Migration_2016 = "SP2013To2016Migration.exe";
            public static readonly string SP_13To19Migration_2019 = "SP2013To2019Migration.exe";
            public static readonly string SP_16To19Migration_2019 = "SP2016To2019Migration.exe";
            public static readonly string SP_07ToSPOnlineMigration = "SP2007ToSPOnlineMigration.exe";
            public static readonly string SP_10ToSPOnlineMigration = "SP2010ToSPOnlineMigration.exe";
            public static readonly string SP_13ToSPOnlineMigration = "SP2013ToSPOnlineMigration.exe";
            public static readonly string SP_16ToSPOnlineMigration = "SP2016ToSPOnlineMigration.exe";


            public static readonly string SPMigrationHSExport_2007 = "SP2007SPMigrationHSExport.exe";
            public static readonly string SPMigrationHSExport_2010 = "SP2010SPMigrationHSExport.exe";
            public static readonly string SPMigrationHSExport_2013 = "SP2013SPMigrationHSExport.exe";
            public static readonly string SPMigrationHSExport_2016 = "SP2016SPMigrationHSExport.exe";
            public static readonly string SP_07ToSPOnlineHSMigration = "SP2007ToSPOnlineHSMigration.exe";
            public static readonly string SP_10ToSPOnlineHSMigration = "SP2010ToSPOnlineHSMigration.exe";
            public static readonly string SP_13ToSPOnlineHSMigration = "SP2013ToSPOnlineHSMigration.exe";
            public static readonly string SP_16ToSPOnlineHSMigration = "SP2016ToSPOnlineHSMigration.exe";
            #endregion

            #region File Migration
            public static readonly string MIGRATION_FM_AZURE_BACKUP_2010 = "FileSystemMigrationAzureWorker.exe";
            public static readonly string MIGRATION_FM_BACKUP_2010 = "FileSystemMigrationWorker.exe";
            public static readonly string MIGRATION_FM_RESTORE_2010 = "FileSystemMigrationRestore.exe";
            public static readonly string MIGRATION_FM_EXCELBUILDER_2010 = "FileSystemMigrationExcelBuilder.exe";
            public static readonly string MIGRATION_FM_RESTORE_2013 = "SP2013FileSystemMigrationRestore.exe";
            public static readonly string MIGRATION_FM_AZURE_RESTORE_2013 = "SP2013FileSystemMigrationAzureRestore.exe";
            public static readonly string MIGRATION_FM_RESTORE_2016 = "SP2016FileSystemMigrationRestore.exe";
            public static readonly string MIGRATION_FM_RESTORE_2019 = "SP2019FileSystemMigrationRestore.exe";
            #endregion

            #region eRoom migration
            public static readonly string MIGRATION_EROOM_AZURE_BACKUP = "eRoomMigrationAzureWorker.exe";
            public static readonly string MIGRATION_EROOM_AZURE_RESTORE = "eRoomMigrationAzureRestore.exe";
            public static readonly string MIGRATION_EROOM_BACKUP = "eRoomMigrationWorker.exe";
            public static readonly string MIGRATION_EROOM_RESTORE_2010 = "eRoomMigrationRestore.exe";
            public static readonly string MIGRATION_EROOM_RESTORE_2013 = "SP2013eRoomMigrationRestore.exe";
            public static readonly string MIGRATION_EROOM_RESTORE_2016 = "SP2016eRoomMigrationRestore.exe";
            public static readonly string MIGRATION_EROOM_RESTORE_2019 = "SP2019eRoomMigrationRestore.exe";
            #endregion

            #region Livelink migration
            public static readonly string MIGRATION_LIVELINK_BACKUP = "LivelinkMigrationWorker.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_2010 = "LivelinkMigrationRestore.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_2013 = "SP2013LivelinkMigrationRestore.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_2016 = "SP2016LivelinkMigrationRestore.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_2019 = "SP2019LivelinkMigrationRestore.exe";
            public static readonly string MIGRATION_LIVELINK_BACKUP_Azure = "LivelinkMigrationAzureWorker.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_Azure_2010 = "LivelinkMigrationAzureRestore.exe";
            public static readonly string MIGRATION_LIVELINK_RESTORE_Azure_2013 = "SP2013LivelinkAzureMigrationRestore.exe";
            #endregion

            #region EMC Documentum migration
            public static readonly string MIGRATION_DOCUMENTUM_AZURE_BACKUP = "DocumentumMigrationAzureWorker.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_AZURE_2013 = "SP2013DocumentumMigrationAzureRestore.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_2010 = "DocumentumMigrationRestore.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_2013 = "SP2013DocumentumMigrationRestore.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_2016 = "SP2016DocumentumMigrationRestore.exe";
            public static readonly string MIGRATION_DOCUMENTUM_RESTORE_2019 = "SP2019DocumentumMigrationRestore.exe";
            public static readonly string MIGRATION_DOCUMENTUM_BACKUP = "DocumentumMigrationWorker.exe";
            #endregion

            #region Notes Migration
            public static readonly string MIGRATION_Notes_BACKUP_AZURE = "NotesMigrationAzureExport";
            public static readonly string MIGRATION_Notes_BACKUPSTA_AZURE = "NotesMigrationAzureExportSTA";
            public static readonly string MIGRATION_Notes_BACKUP = "NotesMigrationWorker";
            public static readonly string MIGRATION_Notes_BACKUPSTA = "NotesMigrationWorkerSTA";
            public static readonly string MIGRATION_Notes_RESTORE_2010 = "NotesMigrationRestore.exe";
            public static readonly string MIGRATION_Notes_RESTORE_2013 = "SP2013NotesMigrationRestore.exe";
            public static readonly string MIGRATION_Notes_RESTORE_2016 = "SP2016NotesMigrationRestore.exe";
            public static readonly string MIGRATION_Notes_RESTORE_2019 = "SP2019NotesMigrationRestore.exe";
            public static readonly string MIGRATION_Notes_RESTORE_AZURE = "SP2013NotesAzureMigrationRestore.exe";
            #endregion

            #region QuickPlace Migration
            public static readonly string MIGRATION_QuickPlace_BACKUP = "QuickrMigrationWorker";
            public static readonly string MIGRATION_QuickPlace_RESTORE_2010 = "QuickrMigrationRestore.exe";
            public static readonly string MIGRATION_QuickPlace_RESTORE_2013 = "SP2013QuickrMigrationRestore.exe";
            public static readonly string MIGRATION_QuickPlace_RESTORE_2016 = "SP2016QuickrMigrationRestore.exe";
            public static readonly string MIGRATION_QuickPlace_RESTORE_2019 = "SP2019QuickrMigrationRestore.exe";
            #endregion

            #region Governance Automation
            public static readonly string GA_AGENT_NAME_2007 = "SP2007GovernanceAutomation";
            public static readonly string GA_AGENT_NAME_2010 = "SP2010GovernanceAutomation";
            public static readonly string GA_AGENT_NAME_2013 = "SP2013GovernanceAutomation";
            public static readonly string GA_AGENT_NAME_2016 = "SP2016GovernanceAutomation";
            public static readonly string GA_AGENT_NAME_2019 = "SP2019GovernanceAutomation";
            public static readonly string GA_AGENT_NAME_SE = "SPSEGovernanceAutomation";
            #endregion

            #region Public Folder
            public static readonly string MIGRATION_PFBACKUP_2010 = "PublicFolderMigrationBackup.exe";
            public static readonly string MIGRATION_PFRESTORE_2010 = "PublicFolderMigrationRestore.exe";
            public static readonly string MIGRATION_PFRESTORE_2013 = "SP2013PublicFolderMigrationRestore.exe";
            public static readonly string MIGRATION_PFRESTORE_2016 = "SP2016PublicFolderMigrationRestore.exe";
            public static readonly string MIGRATION_PFRESTORE_2019 = "SP2019PublicFolderMigrationRestore.exe";
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

            #region High Availability
            public static readonly string HA_AGENTCOMMON_SYNCWORKER_EXE_NAME = "AgentCommonHASyncWorker.exe";
            public static readonly string HA_AGENTCOMMON_DATATRANSFER_EXE_NAME = "AgentCommonHADataTransferServices.exe";
            public static readonly string HA_SP2010_SYNCCONTROLLER_EXE_NAME = "SP2010HASyncController.exe";
            public static readonly string HA_SP2010_FAILOVERCONTROLLER_EXE_NAME = "SP2010HAFailoverController.exe";
            public static readonly string HA_SP2010_MULTIPLEMEMBER_EXE_NAME = "SP2010HAMultipleMember.exe";
            public static readonly string HA_JOB_STOP_FLAG = "HAJobStop.cmd";
            public static readonly string HA_SP2010_ComponentProvider_DLL_NAME = "SP2010HAComponentProvider.dll";
            public static readonly string HA_SP2013_ComponentProvider_DLL_NAME = "SP2013HAComponentProvider.dll";
            public static readonly string HA_SP2013_SYNCCONTROLLER_EXE_NAME = "SP2013HASyncController.exe";
            public static readonly string HA_SP2013_FAILOVERCONTROLLER_EXE_NAME = "SP2013HAFailoverController.exe";
            public static readonly string HA_SP2013_MULTIPLEMEMBER_EXE_NAME = "SP2013HAMultipleMember.exe";
            public static readonly string HA_SP2016_SYNCCONTROLLER_EXE_NAME = "SP2016HASyncController.exe";
            public static readonly string HA_SP2016_FAILOVERCONTROLLER_EXE_NAME = "SP2016HAFailoverController.exe";
            public static readonly string HA_SP2016_MULTIPLEMEMBER_EXE_NAME = "SP2016HAMultipleMember.exe";
            public static readonly string HA_SP2016_ComponentProvider_DLL_NAME = "SP2016HAComponentProvider.dll";
            public static readonly string HA_SP2019_SYNCCONTROLLER_EXE_NAME = "SP2019HASyncController.exe";
            public static readonly string HA_SP2019_FAILOVERCONTROLLER_EXE_NAME = "SP2019HAFailoverController.exe";
            public static readonly string HA_SP2019_MULTIPLEMEMBER_EXE_NAME = "SP2019HAMultipleMember.exe";
            public static readonly string HA_SP2019_ComponentProvider_DLL_NAME = "SP2019HAComponentProvider.dll";
            #endregion High Availability

            #region Health Analyzer
            public static readonly string HealthAnalyzer_SP2010HEALTHANALYZER_EXE_NAME = "SP2010HealthAnalyzer.exe";
            public static readonly string HealthAnalyzer_SP2013HEALTHANALYZER_EXE_NAME = "AgentCommonHealthAnalyzer.exe";
            #endregion

            #region VM
            public static readonly string VM_BROWSER_EXE = "AgentCommonVMBrowser.exe";
            public static readonly string VM_BACKUP_EXE = "AgentCommonVMBackupWorker.exe";
            public static readonly string VM_RESTORE_EXE = "AgentCommonVMRestoreWorker.exe";
            public static readonly string VM_CONTROLLER_EXE = "AgentCommonVMController.exe";
            public static readonly string VM_FIleRESTORE_EXE = "AgentCommonVMFileRestoreWorker";

            public static readonly string VM_INSTAMOUNT_EXE = "AgentCommonVMInstaMountFileServer.exe";
            #endregion

            #region
            public static readonly string AccountChangedFlagFile = "CurrentAccount.dat";
            #endregion

            #region    ########## Records #######
            public static readonly string RecordsScheduleJob = "RecordsScheduleJob.exe";


            #endregion   ########## Records #######



        }

        public class AgentConfigurationFileName
        {
            public static readonly string AgentConfigFile_VCEnvConfig = "AgentCommonVCEnv.config";
            public static readonly string AgentConfigFile_ServiceVersionConfig = "ServiceVersion.config";
            public static readonly string AgentCommonLanguageMappingFile = @"\data\WrapperCommon\SP2010WrapperLanguageMapping.xml";
            public static readonly string AgentCommon2013LanguageMappingFile = @"\data\WrapperCommon\SP2013WrapperLanguageMapping.xml";
            public static readonly string AgentCommonOffice365LanguageMappingFile = @"\data\WrapperCommon\AgentCommonOffice365WrapperLanguageMapping.xml";
            public static readonly string AgentImportTreeFile = "AgentImportTree.xml";
            public static readonly string AgentConfigFile_Log4netConfig = "AgentLog4net.config";
            public static readonly string AgentImportReplicatorCDFile = "AgentImportTreeCDInfo.xml";
            public static readonly string AgentConfigFile_SP2010CentralAdminWorker = "SP2010CentralAdminWorker.exe.config";
            public static readonly string AgentConfigFile_SP2013CentralAdminWorker = "SP2013CentralAdminWorker.exe.config";
            public static readonly string AgentConfigFile_SP2016CentralAdminWorker = "SP2016CentralAdminWorker.exe.config";
            public static readonly string AgentConfigFile_SP2019CentralAdminWorker = "SP2019CentralAdminWorker.exe.config";
            public static readonly string AgentConfigFile_SPSECentralAdminWorker = "SPSECentralAdminWorker.exe.config";
            public static readonly string AgentConfigFile_AgentCommonIocConfigurations = "AgentCommonIocConfigurations.config";
            public static readonly string AgentConfigFile_AgentCommonIocPropertiesConfigurations = "AgentCommonIocPropertiesConfigurations.config";
        }

        public class AgentFolderName
        {
            public static readonly string AgentReplicator2010ExportFolder = "Replicator";
            public static readonly string AgentContentManager2010ExportFolder = "ContentManager";
            public static readonly string AgentDesignManager2010ExportFolder = "DesignManager2010";
            public static readonly string AgentDesignManager2013ExportFolder = "DesignManager2013";
            public static readonly string AgentDesignManager2016ExportFolder = "DesignManager2016";
            public static readonly string AgentDesignManager2019ExportFolder = "DesignManager2019";
            public static readonly string AgentDesignManagerSPSEExportFolder = "DesignManagerSPSE";
            public static readonly string AgentLiveLinkExportFolder = "LiveLink Exported Data";
            public static readonly string AgentLiveLinkHighSpeedExportFolder = "Livelink High Speed Migration Exported Data";
            public static readonly string AgenteRoomExportFolder = "eRoom Exported Data";
            public static readonly string AgenteRoomHighSpeedExportFolder = "eRoom High Speed Migration Exported Data";
            public static readonly string AgentLotusNotesExportFolder = "Lotus Notes Exported Data";
            public static readonly string AgentEmcDocumentumExportFolder = "EMC Documentum Exported Data";

            [SuppressMessage("FxCopCustomRules", "C100007: SpellCheckStringValues", Justification = "DLL Name")]
            public static readonly string AgentLivelinkHighSpeedExportSubFolder = "docavededicated";
            public static readonly string AgentHSMOfflineModuleTempFolder = "MMDFolder";
            public static readonly string AgentHSMExportDocAveDedicatedFolder = "docavededicated";
            public static readonly string AgentHSMExportFolder = "Office365Migration";
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

        public const int OFFICE365_SPVERSION = 8;
    }

    public class GlobalFlags
    {
        public static bool ServiceFullyStarted = false;
    }
}
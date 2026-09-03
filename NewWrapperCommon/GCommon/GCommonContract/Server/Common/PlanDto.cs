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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using System.ComponentModel;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int PlanType { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        /// <summary>
        /// Delete: -1
        /// Active:  0
        /// Temp:    1
        /// </summary>
        [DataMember]
        public int Active { get; set; }

        [DataMember]
        public long CreateTime { get; set; }

        [DataMember]
        public long UpdateTime { get; set; }

        [DataMember]
        public PlanCategory Category { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Extension { get; set; }

        [DataMember]
        public NotificationDto Notification { get; set; }

        [DataMember]
        public LogicalDeviceDto LogicalDrive { get; set; }

        [DataMember]
        public ServiceGroupDto SrcAgentGroup { get; set; }

        [DataMember]
        public ServiceGroupDto DestAgentGroup { get; set; }

        [DataMember]
        public List<ProfileDto> Profiles { get; set; }

        [DataMember]
        public List<ScheduleDto> Schedules { get; set; }

        [DataMember]
        public List<ContentDto> Settings { get; set; }

        [DataMember]
        public List<ContentDto> TreeContents { get; set; }

        [DataMember]
        public List<PlanGroupDto> PlanGroupDtos { get; set; }

        [DataMember]
        public List<string> SiteCollectionIds { get; set; }

        public bool IsShared
        {
            get
            {
                if (ObjectInfo == null || ObjectInfo.ObjectPermissions == null)
                {
                    throw new Exception("Object permission info is null");
                }
                var planPermissions = ObjectInfo.ObjectPermissions;
                int sharedCount = 0;
                foreach (var permission in planPermissions)
                {
                    if (permission.PermissionScope == ObjectPermissionScopeType.User && permission.Permission > 0)
                    {
                        sharedCount++;
                    }
                }
                var isPlanShared = sharedCount > 1;
                return isPlanShared;
            }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanCategory
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        [Description("Administrator")]
        CentralAdmin = 2,

        [EnumMember]
        [Description("Content Manager")]
        ContentManager = 3,

        [EnumMember]
        [Description("Granular Restore")]
        GranularRestore = 4,

        [EnumMember]
        Replicator = 5,

        [EnumMember]
        [Description("Platform Backup")]
        PlatformRecoveryBackup = 6,

        [EnumMember]
        [Description("Platform Restore")]
        PlatformRecoveryRestore = 7,

        [EnumMember]
        [Description("Convert Stub To Content")]
        ConvertStubToContent = 8,

        [EnumMember]
        [Description("Scheduled Storage Manager")]
        ExtenderScheduled = 9,

        [EnumMember]
        StorageOptimizationConfig = 10,

        [EnumMember]
        [Description("Clean Up Orphan BLOBs")]
        StubRetention = 11,

        [EnumMember]
        CASTSAdmSchedule = 12,

        [EnumMember]
        Auditor = 13,

        [EnumMember]
        [Description("Deployment Manager")]
        DeploymentManager = 14,

        [EnumMember]
        [Description("Report Center")]
        ReportCenter = 15,

        [EnumMember]
        [Description("Report Center Usage Pattern Alerting")]
        ReportCenterUsagePatternAlerting = 105,


        [EnumMember]
        Archiver = 16,

        [EnumMember]
        [Description("Granular Backup")]
        GranularBackup = 18,

        [EnumMember]
        Connector = 19,

        [EnumMember]
        [Description("Archiver Restore")]
        ArchiverRestore = 20,

        [EnumMember]
        [Description("Log Manager")]
        LogManager = 21,

        [EnumMember]
        [Description("Archiver Retention")]
        ArchiverRetention = 22,

        [EnumMember]
        [Description("Job Pruning")]
        JobPruning = 23,

        [EnumMember]
        [Description("Platform Maintenance Manager")]
        PlatformRecoveryMaintenance = 24,

        [EnumMember]
        [Description("License Manager")]
        LicenseManager = 25,

        [EnumMember]
        [Description("Language Translater")]
        LanguageTranslater = 26,

        [EnumMember]
        [Description("Automatically Download Update")]
        AutomaticDownloadPatch = 27,

        [EnumMember]
        [Description("Automatically Notify New Update")]
        AutomaticNotifyNewPatch = 28,

        [EnumMember]
        [Description("File System Migration")]
        FileMigration = 29,

        [EnumMember]
        [Description("SharePoint Migration")]
        SPMigration07To10 = 30,

        [EnumMember]
        [Description("eRoom Migration")]
        eRoomMigration = 31,

        [EnumMember]
        [Description("Livelink Migration")]
        LivelinkMigration = 32,

        [EnumMember]
        [Description("Lotus Notes Migration")]
        NotesMigration = 33,

        [EnumMember]
        [Description("End User Archiver")]
        EndUserArchiver = 34,

        [EnumMember]
        Retention = 35,

        [EnumMember]
        ReplicatorImport = 36,

        //[EnumMember]
        //[Description("Extender Data Upgrade")]
        //ExtenderDataUpgrade = 37,

        [EnumMember]
        [Description("Vault")]
        Vault = 37,

        [EnumMember]
        [Description("Content Source")]
        EDContentSource = 38,

        [EnumMember]
        [Description("Data Manager")]
        DataManager = 39,

        [EnumMember]
        [Description("Plan Group")]
        PlanGroup = 40,

        [EnumMember]
        [Description("Archiver Pre-Scan Retention")]
        ArchiverDeleteDataColection = 41,

        [EnumMember]
        [Description("Archiver Full Text Index")]
        ArchiverFullTextIndex = 44,

        [EnumMember]
        [Description("Exchange Public Folder Migration")]
        PublicFolderMigration = 43,

        [EnumMember]
        [Description("End User Restore")]
        EndUserRestore = 45,

        [EnumMember]
        [Description("File System Archiver Full Text Index")]
        FSArchiverFullTextIndex = 46,

        [EnumMember]
        [Description("Farm Rebuild & Repair")]
        FarmRebuild = 53,

        [EnumMember]
        [Description("eDiscovery")]
        EDiscovery = 70,
        [EnumMember]
        [Description("Platform DB Migration")]
        PlatformRecoveryNetAppDBMigration = 54,
        [EnumMember]
        [Description("Platform Index Migration")]
        PlatformRecoveryNetAppIndexMigration = 55,

        [EnumMember]
        [Description("Start Patch Installer")]
        StartPatchInstaller = 56,

        [EnumMember]
        [Description("Deployment Manager Backup")]
        DeploymentManagerBackup = 71,

        [EnumMember]
        [Description("Quickr Migration")]
        QuickPlaceMigration = 72,

        [EnumMember]
        [Description("EMC Documentum Migration")]
        DocumentumMigration = 73,

        #region High Speed Migration takes 74~80
        [EnumMember]
        [Description("File System High Speed Migration")]
        FileSystemHighSpeedMigration = 74,

        [EnumMember]
        [Description("Livelink High Speed Migration")]
        LivelinkHighSpeedMigration = 75,

        [EnumMember]
        [Description("Lotus Notes High Speed Migration")]
        LotusNotesHighSpeedMigration = 76,

        [EnumMember]
        [Description("EMC Documentum High Speed Migration")]
        DocumentumHighSpeedMigration = 77,

        [EnumMember]
        [Description("eRoom High Speed Migration")]
        eRoomHighSpeedMigration = 78,
        #endregion

        [EnumMember]
        [Description("Analyze Sql Backup")]
        SRMAnalyzeSqlBackup = 81,

        [EnumMember]
        [Description("Restore SQL Server Data")]
        SRMRestoreFromSQLBackup = 82,

        [EnumMember]
        [Description("High Availability Sync")]
        HASync = 83,

        [EnumMember]
        [Description("High Availability Failover")]
        HAFailover = 84,

        [EnumMember]
        [Description("End User Granular Restore")]
        EndUserItemRestore = 85,

        [EnumMember]
        [Description("Storage Report")]
        StorageReport = 86,

        [EnumMember]
        [Description("High Availability Fallback")]
        HAFallback = 87,

        [EnumMember]
        [Description("High Availability PreScan")]
        HAPreScan = 88,

        [EnumMember]
        [Description("Report Collector")]
        ReportCollector = 89,

        [EnumMember]
        [Description("Data Sync")]
        SyncData = 90,

        /// <summary>
        /// Administrator新增加的Policy Enforcer功能, 有独立License, 且Job较多
        /// </summary>
        [EnumMember]
        [Description("Administrator Policy Enforcer")]
        CAPolicyEnforcer = 91,

        [EnumMember]
        [Description("Health Analyzer")]
        HealthAnalyzer = 92,

        [EnumMember]
        [Description("Farm Clone")]
        FarmClone = 93,

        [EnumMember]
        [Description("VM Management Backup")]
        VMBackup = 94,

        [EnumMember]
        [Description("VM Management Restore")]
        VMRestore = 95,

        [EnumMember]
        [Description("Report Center Usage Report")]
        ReportCenterUsageReport = 96,

        [EnumMember]
        [Description("Report Center Infrastructure Report")]
        ReportCenterInfrastructureReport = 97,

        [EnumMember]
        [Description("Report Center Administration Report")]
        ReportCenterAdministrationReport = 98,

        [EnumMember]
        [Description("Report Center Compliance Report")]
        ReportCenterComplianceReport = 99,

        [EnumMember]
        [Description("Report Center DocAve Report")]
        ReportCenterDocAveReport = 100,

        [EnumMember]
        [Description("Report Center History Activity Pruning")]
        ReportCenterHistoryActivityPruning = 101,

        [EnumMember]
        [Description("Report Center Audit Controller")]
        ReportCenterHistoryAuditController = 102,

        [EnumMember]
        [Description("Report Center Audit Pruning")]
        ReportCenterAuditPruning = 103,

        [EnumMember]
        [Description("Report Center Web Part Collector")]
        ReportCenterWebpartCollector = 104,

        [EnumMember]
        [Description("Platform Storage Provision")]
        PlatformRecoveryStorageProvision = 106,

        [EnumMember]
        [Description("Platform SnapMirror Provision")]
        PlatformRecoverySnapMirrorProvision = 107,

        [EnumMember]
        [Description("Platform SnapMirror Discover")]
        PlatformRecoverySnapMirrorDiscover = 108,

        [EnumMember]
        [Description("Report Center Common Collector")]
        ReportCenterCommonCollector = 109,

        [EnumMember]
        [Description("SharePoint High Speed Migration")]
        SPHSMigration = 110,

        [EnumMember]
        [Description("Platform Backup for NetApp Systems")]
        PlatformRecoveryBackupforSMSP = 116,

        [EnumMember]
        [Description("Platform Restore for NetApp Systems")]
        PlatformRecoveryRestoreforSMSP = 117,

        [EnumMember]
        [Description("Platform Maintenance Manager for NetApp Systems")]
        PlatformRecoveryMaintenanceforSMSP = 118,

        [EnumMember]
        [Description("Platform DB Migration for NetApp Systems")]
        PlatformRecoveryNetAppDBMigrationforSMSP = 119,

        [EnumMember]
        [Description("Platform Index Migration for NetApp Systems")]
        PlatformRecoveryNetAppIndexMigrationforSMSP = 120,

        [EnumMember]
        [Description("Farm Rebuild & Repair for NetApp Systems")]
        FarmRebuildforSMSP = 121,

        [EnumMember]
        [Description("Farm Clone for NetApp Systems")]
        FarmCloneforSMSP = 122,

        [EnumMember]
        [Description("Report Center Management Activity API Collector")]
        ReportCenterManagementActivityAPICollector = 123,

        [EnumMember]
        [Description("Microsoft 365 Auto Scan")]
        Office365AutoScan = 124,

        [EnumMember]
        [Description("File System Archiver")]
        FSArchiver = 125,

        [EnumMember]
        [Description("Report Center File Share Server Collector")]
        ReportCenterFileShareServerCollector = 126,

        [EnumMember]
        [Description("Records")]
        Records = 127,

        [EnumMember]
        [Description("Physical Archiver")]
        PhysicalArchiver = 128,

    }
}
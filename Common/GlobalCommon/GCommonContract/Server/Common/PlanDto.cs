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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

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

        public bool IsShared()
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
        Archiver = 16,

        [EnumMember]
        ExchangeArchiver = 17,

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
        [Description("SharePoint Migration 2007 to 2010")]
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

        [EnumMember]
        [Description("Vault")]
        Vault = 37,
        
        //[EnumMember]
        //[Description("Extender Data Upgrade")]
        //ExtenderDataUpgrade = 37,

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
        [Description("Exchange Public Folder Migration")]
        PublicFolderMigration = 43,

        [EnumMember]
        [Description("Archiver Full Text Index")]
        ArchiverFullTextIndex = 44,


        [EnumMember]
        [Description("End User Restore")]
        EndUserRestore = 45,

        [EnumMember]
        [Description("Farm Rebuild")]
        FarmRebuild = 53,

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
        [Description("eDiscovery")]
        EDiscovery = 70,

        [EnumMember]
        [Description("Deployment Manager Backup")]
        DeploymentManagerBackup = 71,

        /// <summary>
        /// Administrator新增加的Policy Enforcer功能, 有独立License, 且Job较多
        /// </summary>
        [EnumMember]
        [Description("Administrator Policy Enforcer")]
        CAPolicyEnforcer = 91,

        [EnumMember]
        [Description("Hidden Plan, each user have one")]
        UniqueHidden = 99,

        [EnumMember]
        [Description("Exchange Online Backup")]
        ExchangeOnlineBackup = 100,

        [EnumMember]
        [Description("Exchange Online Restore")]
        ExchangeOnlineRestore = 101,

        [EnumMember]
        [Description("Data Transfer")]
        DataTransfer = 102,

        [EnumMember]
        [Description("Export Report")]
        ExportReport = 103,

        [EnumMember]
        [Description("Delete Group")]
        DeleteGroup = 104,

        [EnumMember]
        [Description("Defragmenter")]
        Defragmenter = 105,

        [EnumMember]
        [Description("Cloud App Admin")]
        CloudAppAdmin = 106,

        [EnumMember]
        [Description("Cloud App Admin Policy Enforcer")]
        CloudAppAdminPE = 107,

        [EnumMember]
        [Description("Exchange Online Locate")]
        ExchangeOnlineLocate = 108,

        [EnumMember]
        [Description("Sync AOS Objects")]
        SyncAOSObjects = 109,

        [EnumMember]
        PhysicalRecords = 110,

        [EnumMember]
        [Description("Archiver Deduplicate")]
        Deduplicate = 111,

        [EnumMember]
        [Description("Archiver Deduplicate Report")]
        DeduplicateReport = 112,

    }
}
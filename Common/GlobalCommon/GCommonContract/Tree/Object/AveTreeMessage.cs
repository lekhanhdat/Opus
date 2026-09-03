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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [KnownType(typeof(DeploymentManagerFSTreeMessage))]
    [KnownType(typeof(EITreeMessage))]
    [KnownType(typeof(FSTreeMessage))]
    [KnownType(typeof(SPTreeMessage))]
    [KnownType(typeof(SecuritySearchTreeMessage))]
    [KnownType(typeof(PlatformBackupTreeMessage))]
    [KnownType(typeof(CAUserTreeMessage))]
    [KnownType(typeof(PreviewTreeMessage))]
    [KnownType(typeof(GRPreviewInitMessage))]
    [KnownType(typeof(ComparisonTreeMessage))]
    [KnownType(typeof(ArchiverIndexDeviceTreeMessage))]
    [KnownType(typeof(SOBlobProviderTreeMessage))]
    [KnownType(typeof(EndUserArchiverSettingTreeMessage))]
    [KnownType(typeof(ConnectorTreeMessage))]
    [KnownType(typeof(SOOrphanBLOBRetentionTreeMessage))]
    [KnownType(typeof(EDCrawlSettingsTreeMessage))]
    [KnownType(typeof(ExchangeOnlineTreeMessage))]
    [DataContract]
    public class AveTreeMessage : AveMessage
    {
        [DataMember]
        public string FarmID { get; set; }

        /// <summary>
        /// Need Move to SecuritySearchTreeMessage
        /// </summary>
        [DataMember]
        public string JobID { get; set; }

        [DataMember]
        public string PlanID { get; set; }

        [DataMember]
        public int StartIndex { get; set; }

        [DataMember]
        public int Length { get; set; }

        [DataMember]
        public string PageInfo { get; set; }

        [DataMember]
        public bool HasNextPage { get; set; }

        [DataMember]
        public int ChildrenCount { get; set; }

        [DataMember]
        public TreeType TreeType { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public bool IsAPI { get; set; }

        public AveTreeMessage()
        {
        }
    }

    [DataContract]
    public enum TreeType
    {
        [EnumMember]
        Undefined = 0,

        [EnumMember]
        CentralAdminScopeTree = 1,

        [EnumMember]
        CentralAdminSecuritySearchTree = 2,

        [EnumMember]
        ContentManagerSrcTree = 3,

        [EnumMember]
        ContentManagerDestTree = 4,

        [EnumMember]
        ContentManagerFilterPreviewTree = 5,

        [EnumMember]
        ContentManagerFSTree = 6,

        [EnumMember]
        GranularBackupTree = 7,

        [EnumMember]
        GranularRestoreTree = 8,

        [EnumMember]
        ContentManagerDestOverviewTree = 9,

        [EnumMember]
        StorageOptimizationTree = 10,

        [EnumMember]
        GranularRestoreOutOfPlaceTree = 11,

        [EnumMember]
        ReplicatorSrcTree = 12,

        [EnumMember]
        ReplicatorDestTree = 13,

        [EnumMember]
        PlatformBackupTree = 14,

        [EnumMember]
        PlatformRestoreTree = 15,

        [EnumMember]
        PlatformRestoreOutOfPlactTree = 16,

        [EnumMember]
        ContentManagerImportTree = 17,

        [EnumMember]
        CMDeleteContentTree = 18,

        [EnumMember]
        CAUserTree = 19,

        [EnumMember]
        CAPermissionTree = 20,

        [EnumMember]
        PlatformItemRestoreTree = 21,

        [EnumMember]
        RCTree = 22,

        [EnumMember]
        PreviewTree = 23,

        [EnumMember]
        GRPreviewTree = 24,

        [EnumMember]
        ReplicatorFSTree = 25,

        [EnumMember]
        ReplicatorImportTree = 26,

        [EnumMember]
        DMSrcTree = 27,

        [EnumMember]
        DMDestTree = 28,

        [EnumMember]
        SOConvertStubToContentTree = 29,

        [EnumMember]
        ReplicatorExportSrcTree = 30,

        [EnumMember]
        ReplicatorImportDetailTree = 31,

        [EnumMember]
        ComparisonTree = 32,

        [EnumMember]
        SOArchiverRestoreTree = 33,

        [EnumMember]
        SCFileSystem = 34,

        [EnumMember]
        SolutionStoreTree = 35,

        [EnumMember]
        PROutOfPlaceWFETree = 36,

        [EnumMember]
        PRCustomDatabaseTree = 37,

        [EnumMember]
        ConnectorFeatureTree = 38,

        [EnumMember]
        RCAuditReportTree = 39,

        [EnumMember]
        PRSSASettingTree = 40,

        [EnumMember]
        PRSSASettingOOPTree = 41,

        [EnumMember]
        SOArchiverIndexDeviceTree = 42,

        [EnumMember]
        SOOrphanBLOBRetentionTree = 43,

        [EnumMember]
        CentralAdminSearchTree = 44,

        [EnumMember]
        ConnectorEnableLibraryTree = 45,

        [EnumMember]
        RCDBContainedTree = 46,

        [EnumMember]
        BPOSTree = 47,

        [EnumMember]
        SOBlobProviderTree = 48,

        [EnumMember]
        SOConfigStubTree = 49,

        [EnumMember]
        FileMigrationSrcTree = 50,

        [EnumMember]
        FileMigrationDestTree = 51,

        [EnumMember]
        eRoomMigrationSrcTree = 52,

        [EnumMember]
        eRoomMigrationDestTree = 53,

        [EnumMember]
        LivelinkMigrationSrcTree = 54,

        [EnumMember]
        LivelinkMigrationDestTree = 55,

        [EnumMember]
        SPMigration07To10SrcTree = 56,

        [EnumMember]
        SPMigration07To10DestTree = 57,

        [EnumMember]
        NotesMigrationSrcTree = 58,

        [EnumMember]
        NotesMigrationDestTree = 59,

        [EnumMember]
        EndUserArchiverSettingTree = 60,

        [EnumMember]
        ComplianceSharePointTree = 61,

        [EnumMember]
        ComplianceArchiverTree = 62,

        [EnumMember]
        EDCrawlSettingTree = 63,

        [EnumMember]
        GranularDataImportTree = 64,

        [EnumMember]
        GranularDataExportTree = 65,

        [EnumMember]
        SPMigration07To10PreviewTree = 66,

        [EnumMember]
        SOExtenderDataUpgradeTree = 67,

        [EnumMember]
        DesignManagerFSTree = 68,

        [EnumMember]
        SOArchiverDataUpgradeTree = 69,

        [EnumMember]
        SP07To10ViewTree = 70,

        [EnumMember]
        SP07To10DestViewTree = 71,

        [EnumMember]
        SP07To10FSTree = 72,

        [EnumMember]
        GranularRestoreOBTree = 73,

        [EnumMember]
        PRDataImportTree = 75,

        [EnumMember]
        PRDataExportTree = 76,

        [EnumMember]
        PublicFolderMigrationSrcTree = 77,

        [EnumMember]
        PublicFolderMigrationDestTree = 78,

        [EnumMember]
        PRRestoreBlobTreeTemplate = 79,

        [EnumMember]
        PRFarmRebuildTreeTemplate = 80,

        [EnumMember]
        ComplianceVaultSettingTree = 81,

        [EnumMember]
        NotesMigrationImportSrcTree = 82,

        [EnumMember]
        NotesMigrationImportDetailTree = 83,

        [EnumMember]
        eRoomMigrationImportSrcTree = 84,

        [EnumMember]
        eRoomMigrationImportDetailTree = 85,

        [EnumMember]
        LivelinkMigrationImportSrcTree = 86,

        [EnumMember]
        LivelinkMigrationImportDetailTree = 87,

        [EnumMember]
        RCMetadataTree = 88,

        [EnumMember]
        RCDocumentAuditingTree = 89,

        [EnumMember]
        RCAdminReportTree = 90,

        [EnumMember]
        RCAuditControllerTree = 91,

        [EnumMember]
        RCUsageTree = 92,

        [EnumMember]
        SPMigration07To10ImportPreviewTree = 95,

        [EnumMember]
        SPMigration07To10ImportDetailTree = 96,

        [EnumMember]
        SOArchiverRestoreSearchTree = 100,

        [EnumMember]
        DMThirdPartyToolTree = 101,

        [EnumMember]
        ArchiverDBTree = 102,

        [EnumMember]
        ContentManagerEditTreeType = 105,

        [EnumMember]
        RCContentTypeUsageTree = 106,

        [EnumMember]
        SOArchiverTree = 108,

        [EnumMember]
        ExchangeOnlineBackupTree = 110,

        [EnumMember]
        ExchangeOnlineRestoreTree = 111,

        [EnumMember]
        ExchangeOnlineRestoreOutofplaceTree = 112,

        [EnumMember]
        RCExportLocationTree = 113,

        [EnumMember]
        ExchangeOnlineRestoreOBTree = 114,

        [EnumMember]
        ExchangeOnlineArchiverTree = 115,

        [EnumMember]
        GoogleDriveArchiverTree = 116
    }
}
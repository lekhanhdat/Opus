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

namespace AvePoint.RA.Contract.RMRuleManageMent
{
    public enum RMContentDisposalAction
    {
        #region old enum, some enum not use bit express, New option Try not to use this
        Remove = 0,
        KeepData = 1,
        LeaveStub = 2,
        Move = 3,
        Archive = 5,
        None = 99,
        MoveDeclare = 4,
        RelatedRecords = 8,
        DeclaredRecords = 16,
        //for physical
        DeleteParentBox = 32,
        ExportOnly = 64,
        MoveWithDeleteSource = 9,
        MoveWithKeepClassfication = 19,
        MoveDeclareWithKeepClassfication = 20,
        ArchiveToStorage = 25,
        ArchiveToStorageAndLeaveStub = 28,
        MoveDeclareWithAllVersions = 40,
        MoveWithAllVersions = 41,
        MoveDeclareStructureWithAllVersions = 42,
        MoveDeclareWithStructure = 43,
        MoveStructureWithAllVersions = 44,
        MoveWithStructure = 45,
        DeclareLinkFile = 128,
        BackupAndRemove = 4096,
        ArchiveBackupAndRemoveLeaveStub = 8192,
        Remove_Declared_LeaveStub_MakeStubImmutable = 146,
        #endregion

        #region in new retion, value must bigger 16884 and is bit express
        IsEnableRemoveRetentionLabel = 16384,
        NewLogicArchvie = 32768,
        NewDeclaredRecords = 65536,
        ArchiverOnly = 524288,
        ArchiveOnlyLastestVersion = 1048576,
        CalculationDisposalDate = 2097152,
        TriggerMicrosoft365ArchivingData = 4194304
        #endregion
    }

    public enum KeepDataStatus
    {
        Delete = 0,
        TagContent = 1,
        LeaveOnlyStub = 2,
        DeclareRecord = 4,
        LockConversation = 8,
        Keep = 16,
        Remove = 32,
        Vault = 64,
        LinkToDocument = 128,
        NotBackup = 256,
        Undeclare = 512,
        Archive = 1024,
        ArchiveAndLeaveStub = 2048,
        ArchiveBackupAndRemove = 4096,
        ArchiveBackupAndRemoveLeaveStub = 8192,
        DeleteOnly = 16384,
        KeepLatestVersion = 32768,
        ArchiveLatestVersion = 65536,
        KeepLatestVersionAndArhiveOthers = 131072,
        IsEnableRemoveRetentionLabel = 262144,
        ArchiverOnly = 524288,
        ArchiveOnlyLastestVersion = 1048576,
        TriggerMicrosoft365Archiving = 2097152
    }

    public enum ExportTypeValue
    {
        None = -1,
        Autonomy = 0,
        Concordance = 1,
        EDRM = 2,
        VEO = 3,
        NAA = 4,
        NARA = 5
    }

    public enum RelatedRecordOption
    {
        None = 0,
        Both = 1
    }
}

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




namespace AvePoint.GCommon.Contract.PlatformRecovery
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IAPRBlobRecoveryContract : IDisposable
    {
        [OperationContract]
        string BackupBlobWithSnapshot(SystemProfileDto filerInfo, string shareName, string jobId);

        [OperationContract]
        List<FolderInfo> BackupConnectorWithStorageApi(LogicalDeviceDto ld, string destPath, ConnectorInfo connectorInfo);

        [OperationContract]
        string RestoreBlobWithSnapshot(string sourcePath, string destPath, PhysicalDeviceDto physicalDevice);

        [OperationContract]
        string RestoreBlobToTempFolder(string sourcePath, string destPath, PhysicalDeviceDto physicalDevice);

        [OperationContract]
        string RenameDirectory(string directoryName);

        [OperationContract]
        bool CheckVolumeSpace(PhysicalDeviceDto physicalDevice, string destPath, string sourcePath, double spareSpace);

        [OperationContract]
        bool DeleteSnapshot(SystemProfileDto filerInfo, string snapshotName, string shareName);

        [OperationContract]
        bool CheckVolumeSnapshot(SystemProfileDto filerInfo, string snapshotName, string shareName);

        [OperationContract]
        int UpdateSnapMirror(string shareName, SystemProfileDto filer, List<SystemProfileDto> filerInfoS);

        [OperationContract]
        string GetVolumeNameByShareName(SystemProfileDto filer, string shareName);

        [OperationContract]
        void RestoreConnectorWithStorageApi(PhysicalDeviceDto destDeivce, LogicalDeviceDto logicalDevice, List<FolderInfo> folderInfoList, bool forceOverWrite, ulong spareSpace);

        [OperationContract]
        bool TestBlobServiceavailability();

        [OperationContract]
        int UpdateSnapVault(string volumeName, SystemProfileDto filerinfo, List<SystemProfileDto> filerInfoS);

        [OperationContract]
        long GetDirectorySize(DirectoryInfo directory);

        [OperationContract]
        string UpdateLunSnapMirror(string mountPoint);

        [OperationContract]
        string UpdateLunSnapVault(string mountPoint, string snapShotName);

        [OperationContract]
        string BackupLun(string lunName, string snapShotName);

        [OperationContract]
        long GetBlobSize(PlatformBlobDBStoreInfo storeInfo);

        [OperationContract]
        bool DeleteBlobData(PlatformBlobDBStoreInfo storeInfo);

        [OperationContract]
        bool MoveBlobData(PlatformBlobDBStoreInfo storeInfo, LogicalDeviceDto destinationDevice);
    }
}
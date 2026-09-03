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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMStoragePolicyService
    {
        [OperationContract]
        string CreateStoragePolicy(StoragePolicyDto dto);

        string CreateStoragePolicyOnGroup(StoragePolicyDto dto, EntityObjectPermissionType permission);

        string CreateStoragePolicyOnAssignedGroup(StoragePolicyDto dto, string groupId, EntityObjectPermissionType permission);

        string CreateStoragePolicy(string userId, StoragePolicyDto dto);
        [OperationContract]
        string UpdateStoragePolicy(StoragePolicyDto dto);
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicy();
        [OperationContract]
        StoragePolicyDto GetStoragePolicyById(string id);
        [OperationContract]
        void DeleteStoragePolicy(string id);
        [OperationContract]
        void DeleteStoragePolicys(List<string> ids);
        //[OperationContract]
        //bool IsDuplicateStoragePolicyName(StoragePolicyDto dto);
        //[OperationContract]
        //List<string> validateStoragePolicy(StoragePolicyDto dto);
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDevices();
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="isOldRecord"></param>
        /// <returns></returns>
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyByIsOldRecord(int isOldRecord);
        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyByRetentionType(int isOldRecord, StoragePolicyType retentionType);
        [OperationContract]
        List<StoragePolicyDetailDto> GetStoragePolicyAssoclatedPlans(StoragePolicyDto dto);
        [OperationContract]
        StoragePolicyDto GetStoragePolicyFreeSpaceById(string id);
        [OperationContract]
        StoragePolicyDto GetStoragePolicyFreeSpaceByDto(StoragePolicyDto dto);
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalFreeSpaceByStorage(StoragePolicyDto dto);
        [OperationContract]
        StoragePolicyDto GetLogicalDeviceName(string id);
        [OperationContract]
        List<ServiceDto> GetMediaServicesByStoragePolicyId(string id);
        [OperationContract]
        List<StoragePolicyDto> GetStoragePolicyByLogicalDeviceId(string logicalDeviceId);
        [OperationContract]
        string TestLogicalDevice(List<ServiceDto> mediaServices, LogicalDeviceDto logical);
        /// <summary>
        /// 这个方法是为Archiver提供，主要是为了Archive Retention中修改Schedule的NextTime提供
        /// </summary>
        /// <param name="dto"></param>
        [OperationContract]
        void UpdateArchiveRetentionNextTime(StoragePolicyDto dto);

        [OperationContract]
        List<StoragePolicyDto> GetStoragePolicyByStateAndLicense(int state);

        [OperationContract]
        List<StoragePolicyDto> GetAllStoragePolicyByRetentionTypeAndLicense(int state, StoragePolicyType retentionType);

        [OperationContract]
        List<StoragePolicyDto> GetStoragePolicyByPlatformType(StoragePolicyLicenseType platformType);
    }
}

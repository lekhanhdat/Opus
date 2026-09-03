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



using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMLogicalDeviceService
    {
        [OperationContract]
        string CreateLogicalDevice(LogicalDeviceDto dto);
        [OperationContract]
        string CreateLogicalDeviceOnGroup(LogicalDeviceDto dto, EntityObjectPermissionType permission);
        [OperationContract]
        string CreateLogicalDeviceOnAssignedGroup(LogicalDeviceDto dto, string groupId, EntityObjectPermissionType permission);
        [OperationContract]
        string UpdateLogicalDevice(LogicalDeviceDto dto);
        [OperationContract]
        void Delete(string id);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="ids"></param>
        [OperationContract]
        void DeleteLogicalDevices(List<string> ids);
        [OperationContract]
        LogicalDeviceDto GetLogicalDeviceById(string id);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OperationContract]
        LogicalDeviceDto GetLogicalDeviceByIdForRaidDisabled(string id);
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDevice();
        [OperationContract]
        bool IsDuplicateLogicalDeviceName(LogicalDeviceDto dto);
        [OperationContract]
        List<ServiceDto> GetMediaService(string type);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="storageType"></param>
        /// <returns></returns>
        [OperationContract]
        List<PhysicalDeviceDto> GetPhysicalDeviceByDeviceType(int storageType);
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDeviceByStorageTypes(List<int> storageTypes, int isOldRecord);
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDeviceByIds(List<string> idList);
        [OperationContract]
        List<LogicalDeviceDto> GetUnfilteredRedundantDevices();
        /// <summary>
        ///<remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> ValidateLogicalDevice(LogicalDeviceDto dto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="isOldRecord"></param>
        /// <returns></returns>
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDeviceByIsOldRecord(int isOldRecord);
        [OperationContract]
        List<LogicalDeviceDto> GetAllLogicalDeviceByStorageType(int storageType, int isOldRecord);
        /// <summary>
        /// </summary>
        /// <param name="storageTypes">type,isOldRecord</param>
        /// <returns></returns>
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDeviceByStorageTypeAndIsOldRecords(Dictionary<int, int> storageTypes);
        [OperationContract]
        LogicalDeviceDto GetPhysicalDeviceFreeSpaceAvailable(LogicalDeviceDto dto);
        [OperationContract]
        List<StoragePolicyDto> GetStoragePolicyByLogicalDevice(LogicalDeviceDto dto);
        [OperationContract]
        Dictionary<int, DeviceRelationContract> GetAllianceDictionary();
        [OperationContract]
        List<LogicalDeviceDto> SetDefaultLogicalDevice(LogicalDeviceDto dto);
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDeviceByStateAndLicense(int state);

        /// <summary>
        /// 根据模块获取logicalDevice
        /// </summary>
        /// <param name="type">Media.Storage.Util.JobType</param>
        /// <param name="isOldRecord">isOldRecord : 判断当前数据是否为已经删除的数据</param>
        /// <returns></returns>
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDevicesByModule(int type, int isOldRecord);
        /// <summary>
        /// 通过media ID获取所有该media关联的storage policy中的logical device信息.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        [Obsolete]
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDevicesByMediaId(string mediaId);

        /// <summary>
        /// 通过media ID获取所有该media关联的storage policy中的logical device信息.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="isFilterOldRecord">isFilterOldRecord : 判断是否过滤已经删除或者是不用的device</param>
        /// <returns></returns>
        [OperationContract]
        List<LogicalDeviceDto> GetLogicalDevicesByMediaId(string mediaId, bool isFilterOldRecord);
        [OperationContract]
        Dictionary<string, string> GetLogicalNameByIDs(List<string> Ids);
    }
}

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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMPhysicalDeviceService
    {
        [OperationContract]
        string CreatePhysicalDevice(PhysicalDeviceDto dto);

        string CreatePhysicalDeviceOnGroup(PhysicalDeviceDto dto, EntityObjectPermissionType permission);

        string CreatePhysicalDeviceOnAssignedGroup(PhysicalDeviceDto dto, string groupId, EntityObjectPermissionType permission);

        string CreatePhysicalDevice(AccountDto account, PhysicalDeviceDto dto);
        /// <summary>
        /// 为其他功能提供的创建Physical Device的方法。
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        string CreatePhysicalDeviceForOther(PhysicalDeviceDto dto);

        [OperationContract]
        string UpdatePhysicalDevice(PhysicalDeviceDto dto);

        [OperationContract]
        bool IsDuplicatePhysicalDeviceName(PhysicalDeviceDto dto, bool isType = false);

        [OperationContract]
        void DeletePhysicalDevice(string id);
        /// <summary>
        ///<remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="ids"></param>
        [OperationContract]
        void DeletePhysicalDevices(List<string> ids);

        [OperationContract]
        List<PhysicalDeviceDto> GetAllPhysicalDevice();

        [OperationContract]
        List<PhysicalDeviceDto> GetAllPhysicalDeviceByType(int type);

        [OperationContract]
        PhysicalDeviceDto GetPhysicalDeviceById(string id);

        [OperationContract]
        PhysicalDeviceDto GetGlobalDefaultPhysicalDevice(string userName);

        [OperationContract]
        PhysicalDeviceDto GetPhysicalDeviceWithDecryptPassword(string id);

        [OperationContract]
        int BringOnlineOrOfflines(List<string> ids, int deviceMode);

        [OperationContract]
        int BringOnlineOrOffline(string id, int deviceMode);

        [OperationContract]
        int testPath(PhysicalDeviceDto dto);

        [OperationContract]
        string validateDiskSpace(PhysicalDeviceDto dto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> validatePhysicalDevice(PhysicalDeviceDto dto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="isCreateFolder"></param>
        /// <returns></returns>
        [OperationContract]
        List<string> ValidatePhysicalDevicePathInfo(PhysicalDeviceDto dto, bool isCreateFolder = false);

        [OperationContract]
        List<string> ValidatePhysicalDeviceSpace(PhysicalDeviceDto dto);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <param name="isOldRecord"></param>
        /// <param name="needToUpdateData"></param>
        /// <returns></returns>
        [OperationContract]
        List<PhysicalDeviceDto> GetAllPhysicalByIsOldRecord(int isOldRecord, bool needToUpdateData = false);

        [OperationContract]
        List<LogicalDeviceDto> getLogicalDeviceByPhysicalDevice(PhysicalDeviceDto dto);

        [OperationContract]
        List<StoragePolicyDetailDto> GetPhysicalDeviceDocaveData(PhysicalDeviceDto dto);

        [OperationContract]
        List<string> GetAllFeatures(int guiType);

        [OperationContract]
        void UpdatePhysicalDeviceSpaceTask();

        [OperationContract]
        Dictionary<int, PhysicalDeviceDataInfoDto> GetDeviceUsedMapping();

        [OperationContract]
        List<PhysicalDeviceDto> GetPhysicalDeviceByLicenseAndState(int state);

        [OperationContract]
        PhysicalDeviceLicenseResult GetPhysicalDeviceLicense();

        [OperationContract]
        PhysicalDeviceDto GetCurrentGlobalDefaultPhysicalDevice(bool needCheckStorage);
    }
}

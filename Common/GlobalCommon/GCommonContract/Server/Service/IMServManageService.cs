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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Service
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMServManageService
    {
        [OperationContract]
        ServiceDto GetCurrentControlDto();

        [OperationContract]
        RegisterResult Register(ServiceDto service);

        [OperationContract]
        string UpdateService(ServiceDto service);
        /// <summary>
        /// <remarks>Invoked by CLI</remarks>
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IList<ServiceDto> GetAllServices();

        [OperationContract]
        IList<ServiceDto> GetInstalledServices();

        [OperationContract]
        IList<ServiceDto> GetServices(ServiceState state, ServiceActive active);

        [OperationContract]
        IList<ServiceDto> GetServicesByType(ServiceType type);

        //[OperationContract]
        //void EditService(ServiceDto service);

        [OperationContract]
        IList<ServiceDto> GetAvailableServicesByType(ServiceType type);

        [OperationContract]
        IList<ServiceDto> GetServicesByTypeAndState(ServiceState state, ServiceActive active, ServiceType type);

        [OperationContract]
        ServiceDto GetServiceById(string id);

        //[OperationContract]
        //string ControlServices(string serviceId, AgentControlOperations operation);

        [OperationContract]
        List<ServiceDto> GetServicesByHost(string host);

        /// <summary> 根据host,获取media service信息 。注释：此方法不检查media service的状态 </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceDto GetMediaServiceByHost(string host);

        //[OperationContract]
        //TestResult TestPath(CacheSettingDto cacheSettingDto, DiskInfoDto dto, string serviceDtoId);

        //[OperationContract]
        //void Save(CacheSettingDto dto);

        //[OperationContract]
        //List<DiskInfoDto> GetSpaceSize(List<DiskInfoDto> diskInfos, string cacheSettingsId, string serviceDtoId);

        //[OperationContract]
        //CacheSettingDto Load(string id);

        //[OperationContract]
        //TestResult TestLocalPath(string path);

        //[OperationContract]
        //bool ComparePath(List<DiskInfoDto> diskInfos, DiskInfoDto newDiskInfo, string cacheSettingsId);

        [OperationContract]
        bool IsCacheSettingInUse(string serviceDtoId);

        [OperationContract]
        string ValidatePassphrase(byte[] passphraseHash);

        //[OperationContract]
        //string ShowTimerServiceMessage();

        void ChangeLicenseStatus(IEnumerable<string> ids, LicenseStatus licenseStatus);

        
    }
}

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
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ITenantInfoDao
    {
        bool CheckIfExistTenantInfo(string tenantId);
        Task<bool> CheckIfExistTenantInfoAsync(string tenantId);
        bool CheckIfExistAOSPTenantInfo(string tenantId);
        void UpdateAOSPToOpusTenantInfo(string tenantId);
        int CheckIfExplorerDataMoved(string tenantId);
        bool CheckTenantIsAvailable(string tenantId);
        void CreateTenantInfo(TenantInfoDto tenantInfo);
        bool IsUserNameExist(string tenantGroupId, string finallyOwnerName);
        void DeleteTenantInfo(string tenantId);
        string GetAvailableTenantDB(int requiredSize);
        void CreateTenantDB(string dbName);
        //void InitTenantDBSchema(string dbName, string userName,string schemaName);
        //string GetEncryptionKey(string tenantId);
        TenantInfoDto GetTenantInfo(string tenantId);
        Task<TenantStatus?> TryGetTenantStatusAsync(string tenantId);
        List<TenantInfoDto> GetAllAvailableTenantInfo();

        bool NeedUpgradeRemoteNodeForAosId(string tenantId);

        bool NeedUpgradeManualData(string tenantId);

        void UpdateContainersUpgradeStatusToSuccessful(string tenantId);

        void UpdateManualDataUpgradeStatusToSuccessful(string tenantId);

        List<TenantInfoDto> GetAllTenantInfo(List<string> tenantIds);
        List<TenantInfoDto> GetAllTenantInfo();
        List<TenantInfoDto> GetTenantInfoByTenantStatusAndMultiGeoStatus(int tenantStatus, int MultiGeoStatus);
        List<TenantInfoDto> GetPenddingForSyncNodesTenants();
        List<TenantInfoDto> GetSyncingNodesTenants();
        int GetTenantInitNodeState(string tenantId);
        int GetTenantDBCount();
        void UpdateStorageInfo(string tenantId, string storageAccountName);
        void UpdateTenantOwner(string tenantId, string owner);
        void UpdateStorageSetting(string tenantId, TenantStorageSetting storageSetting);
        void UpdateStatus(string tenantId, TenantStatus status);

        /// <summary>
        /// 更新Tenant状态
        /// 
        /// 过期的Tenant
        ///     TenantStatus.Provisioning->TenantStatus.Provisioning //正在初始化的tenant不处理
        ///     TenantStatus.Normal->TenantStatus.Disabled    //用户从正常到过期
        ///     TenantStatus.Disabled->TenantStatus.Disabled  
        ///     TenantStatus.Locked->TenantStatus.Disabled    //被锁定的用户变为过期
        ///     
        /// 
        /// 正常的Tenant
        ///     TenantStatus.Provisioning->TenantStatus.Provisioning //正在初始化的tenant不处理
        ///     TenantStatus.Normal->TenantStatus.Normal
        ///     TenantStatus.Disabled->TenantStatus.Normal   //从过期到正常，一般case是过期后续费
        ///     TenantStatus.Locked->TenantStatus.Locked     //被锁定的账户状态不变 
        ///     
        /// </summary>
        /// <param name="tenantIds">available tenants</param>
        void UpdateStatus(List<string> availableTenantIds);
        void UpdateSyncNodeState(string tenantId, RMInitNodeState state);
        void UpdateSyncSAState(string tenantId, RMInitNodeState state);
        void UpdateMultiGeoStatus(string tenantId, MultiGeoStatus status);
        void UpdateTenantDBInfo(string tenantId, string tenantDB, string userName, string schemaName);
        void ChangeAccountStatus(string tenantId, TenantStatus status);
        void DeleteTenantDBSchema(string dbName, string userName, string schemaName);

        bool IsEnableCSD(string tenantId);
        Task<List<T>> CalcPermissionsWithModuleAsync<T>(string customerId, List<T> permissionsForUser);
        void AddOrUpdateTenantLinkedModules(string customerId, RMAosLicenseInfo licenseInfo);
        bool CheckAdditionalDataSource(string customerId, long mAdditionalDataSource);
        bool EnableAdditionalDataSource(string customerId);

        bool CheckAdditionalProduct(string customerId, long mAdditionalProduct);

        Task<bool> IsEnableIntelligent(string tenantId);
        
        Task<bool> UpdateInitStatusForGControlPlatform(string tenantId);
        
        Task<bool> GetGControlPlatformInitStatus(string tenantId);

        string GetRegisterEmailByTenantId(string tenantId);

        Task<bool> UpdateMultiGeoTenantInitStatus(string tenantId, MultiGeoStatus multiGeoStatus);
    }
}

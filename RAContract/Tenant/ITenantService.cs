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
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Tenant
{
    public interface ITenantService
    {
        Task<bool> InitTenantAsync();

        Task<bool> InitAOSPTenantAsync(string logonUserName);

        Task<bool> InitMultiGeoTenantAsync(MultiGeoStatus multiGeoStatus);

        System.Threading.Tasks.Task CheckAndUpdateAOSPTenantAsync();

        List<TenantInfoDto> GetAllAvailableTenantInfo();
        List<TenantInfoDto> GetAllTenantInfo();

        List<TenantInfoDto> GetTenantInfoByTenantStatusAndMultiGeoStatus(int tenantStatus,int MultiGeoStatus);

        string GetRegisterEmailByTenantId(string tenantId);
        bool NeedUpgradeRemoteNodeForAosId(string tenantId);

        System.Threading.Tasks.Task InitKeyForMultiGeoTenant(InitMultiGeoTenantInfo tenantInfo);

        void UpdateContainersUpgradeStatusToSuccessful(string tenantId);
        List<TenantInfoDto> GetPenddingForSyncNodesTenants();
        List<TenantInfoDto> GetSyncingNodesTenants();
        RMInitNodeState GetTenantInitNodeState(string tenantId);
        RMInitNodeState GetTenantInitNodeState(int initState);
        RMInitNodeState GetTenantInitNodeState(string tenantId, out RMDependTypeForInitNode dependType);
        bool CheckTenantExist(string tenantId);
        int IsExplorerDataMoved(string tenantId);
        bool CheckTenantIsAvailable(string tenantId);

        System.Threading.Tasks.Task UpdateAllTenantLicenseInfoAsync();

        void ChangeAccountStatus(string tenantId, TenantStatus status);
        string GetEncryptionKey(string tenantGroupId);
        bool IsOldEncryption(string tenantId);
        //TenantInfoDto GetDefaultEncryptionInfoByGroupId(string tenantGroupId);
        TenantInfoDto GetTenantInfo(string tenantGroupId);
        Task<TenantStatus?> TryGetTenantStatusAsync(string tenantId);
        void UpdateEncryptionInfoByGroupId(string groupId, string key);
        //void InitDefaultKey(string groupId);
        void SyncTenantOwner(string groupId);

        Boolean ValidateAccountByEmail(string email, ref string tenantId, ref string ownerEmail);
        Boolean ValidateAccountByTenantId(string tenantId, ref string ownerEmail);
        /// <summary>
        /// 这个接口不要随意调用, 这个方法会删除Tenant的所有数据,例如DB表,Explorer数据等.
        /// 目前只有COP通过Service Bus发送消息确认之后才会调用此方法来删除过期的Tenant
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task<Boolean> DeleteExpiredTenantAsync(string tenantId);

        bool IsCSDTenant();

        bool IsCustomizationAppTenant();

        void AddOrUpdateTenantLinkedModules(string customerId, RMAosLicenseInfo licenseInfo);

        void UpdateSyncNodeState(string tenantId, RMInitNodeState state);
        void UpdateSyncSAState(string tenantId, RMInitNodeState state);
        void UpdateMultiGeoStatus(string tenantId, MultiGeoStatus status);
        bool CheckLicenseWithAdditionalDataSource(string customerId, PaidForModule module);

        bool CheckLicenseWithAdditionalDataSource(string customerId, PreviewFeature previewFeature);

        bool IsNewOpusTenant();

        bool CheckLicenseWithAdditionalProduct(string customerId, PaidForProduct product);

        long GetUpgradeOpusTimeTicks();

        FileExtentionsConfig GetFileExtentionsConfig();

        int GetExportResultLimit();

        int GetTimeoutPeriodForWaitingJob();
        
        Task<bool> UpdateInitGControlPlatformStatus();
        
        Task<bool> HasInitGControlPlatForm();
        MultiGeoStatus IsMultiGeoTenantInitialized();
        Task<bool> UpdateMultiGeoTenantInitStatus(MultiGeoStatus multiGeoStatus);
    }
}

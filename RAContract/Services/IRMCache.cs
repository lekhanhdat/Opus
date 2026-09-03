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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.Server.UserRegister;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Services
{
    public interface IRMCache
    {
        System.Threading.Tasks.Task<bool> SetAsync<T>(string key, T value, TimeSpan duration = default, bool BuildTenantKey = true);
        Task<T> GetAsync<T>(string key, bool BuildTenantKey = true);
        Task<List<T>> GetListAsync<T>(string key);
        Task<bool> RemoveAsync(string key, bool BuildTenantKey = true);
        Task<long> RemoveAsync(string[] key);

        Task<bool> KeyExpiredAsync(string key, int second);

        Task<bool> ExistAsync(string key);
        System.Threading.Tasks.Task ListAddAsync<T>(string key, T value);
        System.Threading.Tasks.Task SetListAsync<T>(string key, IEnumerable<T> value);
        Task<bool> RenewAsync(string key, TimeSpan duration, bool BuildTenantKey = true);
        //if the redis is down, return value is null.
        Task<T> TryGetAsync<T>(string key, Func<Task<T>> dataProvider, TimeSpan duration = default, bool BuildTenantKey = true);
        Task<bool> CheckRedisAvailable();
        bool GetCachedRedisAvailability();
        public class Keys
        {
            /// <summary>
            /// should invalidate when any simple info add, update or delete
            /// </summary>ManualApprovalQuerier_GetAllSimpleInfoes
            public const string ManualApprovalQuerier_GetAllSimpleInfoes = "ManualApprovalQuerier_GetAllSimpleInfoes";

            /// <summary>
            /// should invalidate when general setting add or update.
            /// </summary>
            public const string GeneralSettingService_GetGeneralSettingAsync = "GeneralSettingService_GetGeneralSettingAsync";
            /// <summary>
            /// should invalidate when isRemove of user with {id} is updated [0=>1, 1=>0], and when user is removed (if can)
            /// </summary>
            public const string AccountDao_GetUserById = "AccountDao_GetUserById";
            /// <summary>
            /// should invalidate when LnkUserGroup records is Added or deleted
            /// </summary>
            public const string LnkUserGroupDao_GetAllGroupIdsAsync = "LnkUserGroupDao_GetAllGroupIdsAsync";
            /// <summary>
            /// should invalidate when isRemove of user with {id} is updated [0=>1, 1=>0], and when user is removed (if can)
            /// </summary>
            //public const string AccountDao_GetIdsOfUserByUserIdsAsync = "AccountDao_GetIdsOfUserByUserIdsAsync";

            public const string DAOAPIClientV1_GetArchiverDataBaseConfig = "DAOAPIClientV1_GetArchiverDataBaseConfig";

            public const string Office365_Tenant_Info = "Office365_Tenant_Info";

            public const string License_Tenant_Info = "License_Tenant_Info";
            public const string EnableMaestroAI = "EnableMaestroAI";
            public const string Tenant_IsNewOpus = "Tenant_IsNewOpus";
            public const string Tenant_JobQueueCount = "Tenant_JobQueueCount";
            public const string ForceEnableSO = "ForceEnableSO";

            public const string Discovery_Query = "Discovery_Query";
            public const string Tenant_UpgradeOpusTime = "UpgradeOpusTime";

            //11111
            public const string Progress_Query = "ProgressQuery";

            public const string Job_ArchivedDataTier = "ArchivedDataTier";

            public const string EnableFullTextIndexSearch = "EnableFullTextIndexSearch";

            public const string PreviewRestoreResult = "PreviewRestoreResult";

            public const string PreviewRestorePerMinuteCount = "PreviewRestorePerMinuteCount";
        }

    }
    
    /// <summary>
    /// 
    /// </summary>
    public enum KeyType
    {
        _Default,
        User_UserId,
        User_Id,
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyType">indicate sub type, this is must be provided becasue cache may use different key for same table, for example ,both userid or id in user table may be used as identity of cache key, when the key is invalided, the key type need to be provide.</param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public delegate System.Threading.Tasks.Task CacheInvalidateHandler(KeyType keyType = KeyType._Default, params string[] keys);
    public interface IRMCacheManager
    {
        IRMCache Cache { get; }

        public CacheInvalidateHandler SimpleInfoAdded { get; }
        public CacheInvalidateHandler SimpleInfoUpdated { get; }
        public CacheInvalidateHandler SimpleInfoDeleted { get; }
        public CacheInvalidateHandler GeneralSetingAdded { get; }
        public CacheInvalidateHandler GeneralSettingUpdated { get; }
        public CacheInvalidateHandler UserUpdated { get; }
        /// <summary>
        /// Should invoke when user records is deleted
        /// </summary>
        public CacheInvalidateHandler UserDeleted { get; }
        public CacheInvalidateHandler UserRemovedStatusChanged { get; }
        public CacheInvalidateHandler LnkUserGroupDeleted { get; }
        public CacheInvalidateHandler LnkUserGroupUpdated { get; }
        public CacheInvalidateHandler LnkUserGroupAdded { get; }
        public CacheInvalidateHandler ArchiverDatabaseConfigUpdated { get; }

    }
}

   

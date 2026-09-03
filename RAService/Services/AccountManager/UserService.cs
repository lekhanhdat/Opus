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
using AngleSharp.Text;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.Multi_Geo;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Online.SharePoint.TenantManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AccountManager
{
    public class UserService : RMServiceBase, IUserService
    {
        private RALogger logger = RALogger.GetInstance(typeof(UserService));

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private ILnkUserGroupDao LnkUserGroupDao => PlatformWindsorManager.GetService<ILnkUserGroupDao>();

        private IPoolUserDao PoolUserDao => PlatformWindsorManager.GetService<IPoolUserDao>();

        private ILnkUserRoleDao LnkUserRoleDao => PlatformWindsorManager.GetService<ILnkUserRoleDao>();

        private IRoleDao RoleDao => PlatformWindsorManager.GetService<IRoleDao>();

        private IRMSecurityGroupMembershipDao SecurityGroupMembershipDao => PlatformWindsorManager.GetService<IRMSecurityGroupMembershipDao>();

        public ITenantInfoDao TenantDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        private IRMSecurityGroupDao RMSecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        private IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();
        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        private Dictionary<string, AccountDto> _needToSyncGoogleUser = [];


        //private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        //private static IRMCacheManager _CacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();


        public void UpdatePoolUserUsage(string userName, string tenantId, bool isAdd)
        {
            PoolUserDao.UpdatePoolUserUsage(userName, tenantId, isAdd);
        }

        public void AddPoolUser(PoolUserDto user)
        {
            PoolUserDao.AddPoolUser(ConvertToRMPoolUser(user));
        }

        public PoolUserDto GetAvailableUser(string tenantId)
        {
            PoolUserDto realUser = null;
            try
            {
                var user = PoolUserDao.GetAvailableUser(tenantId);
                if (user != null)
                {
                    logger.Info("get pool user from db success.");
                    realUser = RMAosApiClient.GetPoolUserByName(TenantLocalValue.LogonGroupId, tenantId, user.UserName);
                    if (realUser != null)
                    {
                        PoolUserDao.UpdatePoolUserUsage(realUser.UserName, tenantId, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get available pool user,ERROR:{2}", ex.ToString());
            }

            return realUser;

        }

        public PoolUserDto GetPoolUserByName(string tenantId, string userName)
        {
            return ConvertToPoolUserDto(PoolUserDao.GetPoolUserByName(tenantId, userName));
        }

        public List<PoolUserDto> GetPoolUsers()
        {
            return PoolUserDao.FindAll().ConvertAll(o => ConvertToPoolUserDto(o));
        }

        public void UpdatePoolUserStatus(string userName, string tenantId, int status)
        {

            PoolUserDao.UpdatePoolUserStatus(userName, tenantId, status);
        }

        public async Task<List<AccountDto>> GetApplicationAdminsAsync()
        {
            var applicationAdminRoleId = RoleDao.Find(item => item.RoleType == RMRoleType.ApplicationAdmin).RoleId;
            var adminUserIds = (await LnkUserRoleDao.FindListAsync(item => item.RoleId == applicationAdminRoleId)).Select(item => item.UserId).ToList();
            if(adminUserIds.Count == 0)
            {
                return null;
            }
            var admins = await AccountDao.FindListAsync(item => item.IsRemoved == 0 &&
                (item.ObjectType == RMActiveDirectoryObjectType.User || item.ObjectType == RMActiveDirectoryObjectType.UserInGroup) &&
                adminUserIds.Contains(item.UserId));
            return admins.ConvertAll(o => ConvertToAccountDtoWithId(o));
        }

        public async Task<List<AccountDto>> GetManagementUsersAsync()
        {
            return (await AccountDao.FindListAsync(t => t.IsRemoved == 0)).ConvertAll(o => ConvertToAccountDtoWithId(o));
        }

        public async Task<List<AOSUserDto>> GetManagementUsersForAosDtoAsync()
        {
            return (await AccountDao.FindListAsync(t => t.IsRemoved == 0)).ConvertAll(o => ConvertRMAccountToAOSUserDto(o));
        }

        private PoolUserDto ConvertToPoolUserDto(RMPoolUser user)
        {
            if (user == null) return null;
            return new PoolUserDto()
            {
                Id = user.Id,
                TenantId = user.TenantId,
                Password = user.Password,
                Status = user.Status,
                UserName = user.UserName,
                Usage = user.Usage,
                AdminUrl = user.AdminUrl,
                RowVersion = user.RowVersion
            };
        }

        private RMPoolUser ConvertToRMPoolUser(PoolUserDto dto)
        {
            if (dto == null) return null;
            return new RMPoolUser()
            {
                Id = dto.Id,
                TenantId = dto.TenantId,
                Password = dto.Password,
                Status = dto.Status,
                UserName = dto.UserName,
                Usage = dto.Usage,
                AdminUrl = dto.AdminUrl,
                RowVersion = dto.RowVersion
            };
        }

        private AccountDto ConvertToAccountDto(RMAccount account)
        {
            if (account == null) { return null; }
            return new AccountDto()
            {
                UserId = account.UserId,
                DisplayName = account.DisplayName,
                UserPrincipalName = account.UserPrincipalName,
                ObjectType = account.ObjectType,
                IsRemoved = account.IsRemoved,
                FirstName = account.FirstName,
                LastName = account.LastName,
                AADId = account.AADId
            };
        }

        private AccountDto ConvertToAccountDtoWithId(RMAccount account)
        {
            if(account == null) 
            { 
                return null; 
            }
            var dto = ConvertToAccountDto(account);
            dto.Id = account.Id;
            return dto;
        }

        private RMAccount ConvertToRMAccount(AccountDto account)
        {
            if (account == null) { return null; }
            return new RMAccount()
            {
                UserId = account.UserId,
                DisplayName = account.DisplayName,
                UserPrincipalName = account.UserPrincipalName,
                ObjectType = account.ObjectType,
                FirstName = account.FirstName,
                LastName = account.LastName,
                AADId = account.AADId
            };
        }

        public System.Threading.Tasks.Task CreateMUserAsync(AccountDto account)
        {
            var ac = ConvertToRMAccount(account);

            return AccountDao.CreateAsync(ac);
        }

        public async System.Threading.Tasks.Task SyncAosUsersAsync()
        {
            try
            {
                var customerId = TenantLocalValue.LogonGroupId;
                var accounts = RMAosApiClient.GetGroupAndUsers(customerId);
                if (accounts != null)
                {
                    //Group下的User只有登录的时候同步.
                    var users = accounts.Where(a => a.ObjectType != RMActiveDirectoryObjectType.UserInGroup);
                    foreach (var ac in users)
                    {
                        await SyncAOSUserGroupAsync(ac);
                    }

                    await SyncUsersRemovedAsync(accounts);
                    foreach (var ac in _needToSyncGoogleUser)
                    {
                        await SyncAOSUserGroupAsync(ac.Value);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while upgrade account info,ERROR:{0}", ex.ToString());
            }
        }
        #region 获取AOS中不存在且在Opus中存在的用户,并将其删除
        /// <summary>
        /// 获取AOS中不存在且在Opus中存在的用户,并将其删除
        /// </summary>
        /// <param name="listAosUsers"></param>
        public async System.Threading.Tasks.Task SyncUsersRemovedAsync(List<AccountDto> listAosUsers)
        {
            //思路: 先AOS中找当前用户下 所有的List<UserPrincipalName>
            //再到Accounts表中找到 在AOS中不存在 的用户,
            //对上述用户进行标记

            List<RMAccount> allAccounts = await AccountDao.FindListAsync(el => el.IsRemoved == 0);

            await RemoveGControlAccounts(allAccounts,listAosUsers);
            
            foreach (var account in allAccounts)
            {
                var userId = account.UserId;
                if (!listAosUsers.Any(l => l.UserId == userId))
                {

                    if (account.ObjectType == RMActiveDirectoryObjectType.Group)
                    {
                        //标记删除Group以及Group下User
                        await FlagGroupUsersAsync(userId, allAccounts);
                    }
                    else if (account.ObjectType == RMActiveDirectoryObjectType.User || (int)account.ObjectType == 3 || (int)account.ObjectType == 4)    //3,4 是AOS里的新枚举 Portal Support Account， Product Support Account
                    {
                        //标记删除User
                        account.IsRemoved = 1;

                    }
                }
            }
            var lstRemoveAccounts = allAccounts.Where(u => u.IsRemoved == 1).ToList();
            if (lstRemoveAccounts.Count > 0)
            {
                var userIds = lstRemoveAccounts.Select(l => l.UserId).ToList();
                AccountDao.DeleteUserMapping(userIds);
                int affectRows = AccountDao.BatchUpdate(lstRemoveAccounts);
                var cachekeys = lstRemoveAccounts.Select(ac => ac.Id.ToString()).ToArray();
                await RMCacheManager.UserRemovedStatusChanged(KeyType.User_Id, cachekeys);
                cachekeys = lstRemoveAccounts.Select(ac => ac.UserId.ToString()).ToArray();
                await RMCacheManager.UserRemovedStatusChanged(KeyType.User_UserId, cachekeys);
                //await RMCacheManager.Cache.RemoveAsync(cachekeys.ToArray());
                //cachekeys = lstRemoveAccounts.Select(ac => IRMCache.Keys.AccountDao_GetIdsOfUserByUserIdsAsync + ac.Id);
                //await RMCacheManager.Cache.RemoveAsync(cachekeys.ToArray());
            }

        }
        
        /// <summary>
        /// Removes Google accounts from a list if they do not have a corresponding
        /// standard (GUID-based UserId) account sharing the same AADId.
        /// </summary>
        /// <param name="accounts">The list of accounts to process.</param>
        private async System.Threading.Tasks.Task RemoveGControlAccounts(List<RMAccount> accounts, List<AccountDto> listAosUsers)
        {
            var tempAccounts = accounts.Concat(listAosUsers.Select(ConvertToRMAccount)).DistinctBy(account => account.UserId).ToList();
            var linkedAosIds = tempAccounts
                .Where(acc => Guid.TryParse(acc.UserId, out _) && acc.AADId != null)
                .Select(acc => acc.AADId)
                .ToHashSet();

            var groupUserByUserId = tempAccounts.Where(acc => acc.AADId.IsNotNullOrEmpty()).GroupBy(acc => acc.AADId).Where(group => group.Count() > 1).ToDictionary(item => item.Key, item => item.ToList());
            foreach (var (groupKey, _) in groupUserByUserId)
            {
                var googleLinkedUser = listAosUsers.FirstOrDefault(acc => acc.AADId == groupKey);
                if (googleLinkedUser != null)
                {
                    _needToSyncGoogleUser.Add(groupKey, googleLinkedUser);
                }
            }
            accounts.RemoveAll(acc =>
                IsGoogleAccount(acc) && !linkedAosIds.Contains(acc.AADId)
            );
        }

        /// <summary>
        /// Determines if an account is a "Google account" based on the established convention.
        /// </summary>
        private bool IsGoogleAccount(RMAccount account)
        {
            return account.UserId == account.AADId && !Guid.TryParse(account.AADId, out _);
        }

        private async System.Threading.Tasks.Task FlagGroupUsersAsync(string groupId, List<RMAccount> allAccount)
        {

            var userIds = (await LnkUserGroupDao.FindListAsync(u => groupId.Equals(u.GroupId))).Select(u => u.UserId).ToList();
            var removedUser = allAccount.Where(a => userIds.Contains(a.UserId) && a.IsRemoved == 0 && a.ObjectType == RMActiveDirectoryObjectType.UserInGroup).ToList();
            var users = allAccount.Where(a => a.UserId == groupId).Union(removedUser).ToList();
            users.ForEach(u => u.IsRemoved = 1);
        }
        #endregion


        /// <summary>
        /// 批量注册accounts.
        /// 由于调用此方法前，需要向AOS注册user，然后等待返回结果，为了速度方面考虑，没有处理role。 role会在user登录时更新
        /// </summary>
        /// <param name="dtos"></param>
        public async System.Threading.Tasks.Task BatchAddAccountsAsync(List<AccountDto> dtos)
        {
            var userIds = dtos.Select(o => o.UserId).ToList();
            var existUserIds = await AccountDao.GetExistUserIdsAsync(userIds);
            var nonExistUserIds = userIds.Except(existUserIds);
            var accountDtos = dtos.Where(o => nonExistUserIds.Contains(o.UserId));
            var accountEntitys = accountDtos.Select(dto => new DB.Model.RMAccount()
            {
                AADId = dto.AADId,
                UserId = dto.UserId,
                ObjectType = dto.ObjectType,
                UserPrincipalName = dto.UserPrincipalName,
                IsRemoved = 0,
                DisplayName = dto.DisplayName,
                CreateTime = DateTime.UtcNow.Ticks,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            }).ToList();

            AccountDao.BatchCreate(accountEntitys);
            dtos.ForEach(o =>
            {
                if (o.Id == 0)
                {
                    var account = accountEntitys.Where(u => u.UserId.Equals(o.UserId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (account != null)
                    {
                        o.Id = account.Id;
                    }
                }
            });
        }

        public async System.Threading.Tasks.Task SyncAdminAccountForMultiGeoTenantOtherDCAsync(List<AccountDto> accounts)
        {
            await BatchAddAccountsAsync(accounts);
            foreach (var account in accounts)
            {
                AddRoleType(account.UserId, RMRoleType.ApplicationAdmin);
            }
        }

        private async System.Threading.Tasks.Task SyncAOSUserGroupAsync(AccountDto dto, bool isFromLogon = false)
        {
            if (dto.ObjectType == RMActiveDirectoryObjectType.Group)
            {
                await SyncSingleAOSGroupAsync(dto);
            }
            else
            {
                await SyncSingleAosUserAsync(dto, !isFromLogon);
            }
        }

        private async System.Threading.Tasks.Task SyncSingleAOSGroupAsync(AccountDto dto)
        {
            try
            {
                var groupFromDB = FindUserByNameOrId(dto.UserId, dto.UserPrincipalName);
                if (groupFromDB == null)
                {
                    await AccountDao.CreateAsync(new DB.Model.RMAccount()
                    {
                        UserId = dto.UserId,
                        ObjectType = dto.ObjectType,
                        UserPrincipalName = dto.UserPrincipalName,
                        IsRemoved = 0,
                        DisplayName = dto.DisplayName,
                        CreateTime = DateTime.UtcNow.Ticks,
                        LastUpdateTime = dto.LastModifiedTime,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        AADId = dto.AADId
                    });
                    var roleType = GetRMAccountType(dto.AccountType);
                    AddRoleType(dto.UserId, roleType);
                    AddUserToGroupMemberShips(roleType, dto.UserId);
                }
                else if (groupFromDB.LastUpdateTime != dto.LastModifiedTime)
                {
                    if (!string.IsNullOrEmpty(groupFromDB.AADId) && groupFromDB.AADId == groupFromDB.UserId)
                    {
                        //old data wrong ,just keep relation ship.
                    }
                    else
                    {
                        groupFromDB.UserId = dto.UserId;
                    }
                    groupFromDB.ObjectType = dto.ObjectType;
                    if (!string.IsNullOrEmpty(dto.UserPrincipalName))
                    {
                        groupFromDB.UserPrincipalName = dto.UserPrincipalName;
                    }
                    groupFromDB.IsRemoved = 0;
                    if (!string.IsNullOrEmpty(dto.DisplayName))
                    {
                        groupFromDB.DisplayName = dto.DisplayName; ;
                    }
                    groupFromDB.LastUpdateTime = dto.LastModifiedTime;
                    groupFromDB.FirstName = dto.FirstName;
                    groupFromDB.LastName = dto.LastName;
                    if (!Guid.TryParse(dto.AADId, out _)) // Only update AAD for google user
                    {
                        groupFromDB.AADId = dto.AADId;
                    }
                    await AccountDao.UpdateAsync(groupFromDB);
                    await RMCacheManager.UserUpdated(KeyType.User_Id, groupFromDB.Id.ToString());
                    //await RMCacheManager.Cache.RemoveAsync(IRMCache.Keys.AccountDao_GetUserById + groupFromDB.Id);
                    //AccountDao.Update(new DB.Model.RMAccount()
                    //{
                    //    Id = groupFromDB.Id,
                    //    UserId = dto.UserId,
                    //    ObjectType = dto.ObjectType,
                    //    UserPrincipalName = dto.UserPrincipalName,
                    //    IsRemoved = 0,
                    //    DisplayName = dto.DisplayName,
                    //    CreateTime = groupFromDB.CreateTime,
                    //    LastUpdateTime = dto.LastModifiedTime
                    //});

                    await UpsertRoleTypeAsync(dto);
                    var roleType = GetRMAccountType(dto.AccountType);
                    AddUserToGroupMemberShips(roleType, dto.UserId);
                }
                else
                {
                    //User什么都不变，检查User是否是Application Admin，如果是放到GroupMemberShips
                    //此处主要是为了AOS存在的旧Application Admin User，升级前后没有改动，需要放到GroupMemberShips
                    var roleType = GetRMAccountType(dto.AccountType);
                    AddUserToGroupMemberShips(roleType, dto.UserId);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Sync group user error:{ex.ToString()}");
            }
        }

        /// <summary>
        /// 1.把Application Admin的AOS User/Group添加到Records Admin Security Group里(此处只需要判断表中是否有Admin User Mapping关系，没有添加一条)
        /// 2.把Standard的AOS User/Group添加到Records Standard Group里(此处需要判断表中是否有Admin User Mapping关系，如果有需要把Admin权限User改成Standard User)
        /// </summary>
        /// <param name="roleType"></param>
        /// <param name="userId"></param>
        private void AddUserToGroupMemberShips(RMRoleType roleType, string userId)
        {
            if (roleType == RMRoleType.ApplicationAdmin)
            {
                SecurityGroupMembershipDao.AddUserToGroupMemberShips((int)RMAccountType.ApplicationAdmin, userId);
            }
            else
            {
                //SecurityGroupMembershipDao.AddUserToGroupMemberShips((int)RMAccountType.RegisteredUser, userId);
                SecurityGroupMembershipDao.RemoveUserGroupMemeberships((int)RMAccountType.ApplicationAdmin, userId);
            }
        }

        public bool IsMemberOfSecurityGroup(int groupId, string userId)
        {
            return SecurityGroupMembershipDao.IsUserInGroup(groupId, userId);
        }

        public List<int> GetAllGroupIds(List<string> userAndGroupIds)
        {
            return SecurityGroupMembershipDao.GetAllGroupIds(userAndGroupIds);
        }

        private RMAccount FindUserByNameOrId(string userId, string userName)
        {
            return AccountDao.Find(u => (u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)
            || userName.Equals(u.UserPrincipalName, StringComparison.OrdinalIgnoreCase)
            || userId.Equals(u.AADId, StringComparison.OrdinalIgnoreCase)) && u.IsRemoved == 0);

        }

        private string GetUserDisplaynameFromO365(string userIdOrUPN)
        {
            string name = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(userIdOrUPN)) return null;
                name = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, userIdOrUPN)?.DisplayName;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while get user display name with {userIdOrUPN}, message:{ex}");
            }

            return name;
        }

        /// <summary>
        /// update user to DB. it will connect to O365 to get the latest display name.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="updateDisplayNameSynchrous">if false, will update the display name asynchrously</param>
        private async System.Threading.Tasks.Task SyncSingleAosUserAsync(AccountDto dto, bool updateDisplayNameSynchrous)
        {
            try
            {
                var userId = dto.UserId;
                var userType = dto.AccountType;
                var userName = dto.UserPrincipalName;
                var displayName = dto.DisplayName;
                var firstName = dto.FirstName;
                var lastName = dto.LastName;
                if (updateDisplayNameSynchrous)
                {
                    var displayNameInO365 = GetUserDisplaynameFromO365(userName);
                    displayName = displayNameInO365 ?? displayName;
                }

                //UserId或Email只要有一个存在,则更新;如果两者都不存在,则增加 
                var dbUser = FindUserByNameOrId(userId, userName);

                if (dbUser == null)
                {
                    await AccountDao.CreateAsync(new DB.Model.RMAccount()
                    {
                        UserId = userId,
                        ObjectType = dto.ObjectType,
                        UserPrincipalName = userName,
                        DisplayName = displayName,
                        CreateTime = DateTime.UtcNow.Ticks,
                        LastUpdateTime = dto.LastModifiedTime,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        AADId = dto.AADId
                    });
                    var roleType = GetUserRoleType(userId, dto.AccountType);
                    AddRoleType(userId, roleType);
                    AddUserToGroupMemberShips(roleType, userId);
                }
                else if (displayName != dbUser.DisplayName || dbUser.LastUpdateTime != dto.LastModifiedTime)
                {
                    dbUser.ObjectType = dto.ObjectType;
                    dbUser.UserPrincipalName = userName;
                    dbUser.IsRemoved = 0;
                    dbUser.DisplayName = displayName;
                    dbUser.LastUpdateTime = dto.LastModifiedTime;
                    dbUser.FirstName = firstName;
                    dbUser.LastName = lastName;
                    if (!Guid.TryParse(dto.AADId, out _)) // Only update AAD for google user
                    {
                        dbUser.AADId = dto.AADId;
                    }
                    await AccountDao.UpdateAsync(dbUser);
                    //await RMCacheManager.UserUpdated(dbUser.Id.ToString());
                    await RMCacheManager.UserRemovedStatusChanged(KeyType.User_Id, dbUser.Id.ToString());
                    await RMCacheManager.UserRemovedStatusChanged(KeyType.User_UserId, userId);
                    //await RMCacheManager.Cache.RemoveAsync(IRMCache.Keys.AccountDao_GetUserById + dbUser.Id);
                    //await RMCacheManager.Cache.RemoveAsync(IRMCache.Keys.AccountDao_GetIdsOfUserByUserIdsAsync + dbUser.Id);
                    //AccountDao.Update(new DB.Model.RMAccount()
                    //{
                    //    Id = dbUser.Id,
                    //    UserId = userId,
                    //    ObjectType = dto.ObjectType,
                    //    UserPrincipalName = userName,
                    //    IsRemoved = 0,
                    //    DisplayName = displayName,
                    //    CreateTime = dbUser.CreateTime,
                    //    LastUpdateTime = dto.LastModifiedTime
                    //});
                    //如果Role Type改变则修改RoleType
                    await UpsertRoleTypeAsync(dto);
                    var roleType = GetUserRoleType(userId, dto.AccountType);
                    AddUserToGroupMemberShips(roleType, userId);
                }
                else
                {
                    //User什么都不变，检查User是否是Application Admin，如果是放到GroupMemberShips
                    //此处主要是为了AOS存在的旧Application Admin User，升级前后没有改动，需要放到GroupMemberShips
                    var roleType = GetUserRoleType(userId, dto.AccountType);
                    AddUserToGroupMemberShips(roleType, userId);
                }

                if (!updateDisplayNameSynchrous)
                {
                    UpdateDisplayNameAsynchronous(userId, userName);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("Invalid object name"))
                {
                    logger.Warn("customer need upgrade db, customerId:{0}, ERROR:{1}", TenantLocalValue.LogonGroupId, ex.InnerException.Message);
                }
                else
                {
                    logger.Error("error occurred while sync aos signal user info,ERROR:{0}", ex.ToString());
                }

            }
        }

        /// <summary>
        /// update the user display name asynchronously
        /// </summary>
        /// <param name="userId">user id of Account table</param>
        /// <param name="upnOrId">upn or user id in O365</param>
        private void UpdateDisplayNameAsynchronous(string userId, string upnOrId)
        {
            AveTenantThread t = new AveTenantThread(new ThreadStart(() =>
            {
                var dbUser = FindUserByNameOrId(userId, upnOrId);
                if (dbUser != null)
                {
                    var displayNameInO365 = GetUserDisplaynameFromO365(upnOrId);
                    if (!string.IsNullOrEmpty(displayNameInO365) && displayNameInO365 != dbUser.DisplayName)
                    {
                        dbUser.DisplayName = displayNameInO365;
                        _=AccountDao.UpdateAsync(dbUser).Result;
                    }
                }
            }));
            t.IsBackground = true;
            t.Start();
        }

        private RMRoleType GetUserRoleType(string userId, RMAccountType type)
        {
            var roleType = GetRMAccountType(type);
            return roleType;
        }

        private async Task<bool> UpsertRoleTypeAsync(AccountDto dto)
        {
            var result = false;
            var roleType = GetUserRoleType(dto.UserId, dto.AccountType);
            var lnkRole = LnkUserRoleDao.Find(u => u.UserId.Equals(dto.UserId));
            if (lnkRole == null)
            {
                AddRoleType(dto.UserId, roleType);
            }
            else
            {
                var role = RoleDao.Find(r => r.RoleId == lnkRole.RoleId);
                if (role != null)
                {
                    if (role.RoleType != roleType)
                    {
                        logger.Info($"user role change:{role.RoleType} 2 {roleType}.");
                        var curRole = RoleDao.Find(r => r.RoleType == roleType);
                        lnkRole.RoleId = curRole.RoleId;
                        await LnkUserRoleDao.UpdateAsync(lnkRole);
                        result = true;
                    }
                }
            }
            return result;
        }

        private Contract.RoleAssignments.RMRoleType GetRMAccountType(RMAccountType type)
        {
            return (Contract.RoleAssignments.RMRoleType)(type == RMAccountType.RegisteredUser ? RMAccountType.ApplicationAdmin : type);
        }

        /// <summary>
        /// 只有登录时可调用. 因为TenantLocalValue.UserGroups是登录时传过来的.
        /// </summary>
        /// <param name="userId"></param>
        public async System.Threading.Tasks.Task SyncLogonUserGroupAsync(string userId)
        {
            try
            {
                using (new PerformanceScope("Sync User Group"))
                {
                    var tenantId = TenantLocalValue.LogonGroupId;
                    List<AccountDto> userAndGroups = new List<AccountDto>();
                    var currentUser = RMAosApiClient.GetUserByUserId(tenantId, userId);
                    if(currentUser == null)
                    {
                        logger.Warn($"user not exists in opus. userId: {userId}");
                        return;
                    }
                    userAndGroups.Add(currentUser);
                    if (TenantLocalValue.UserGroups != null && TenantLocalValue.UserGroups.Count > 0)
                    {
                        var groupIds = TenantLocalValue.UserGroups.Select(u => u.ObjectId).ToList();
                        var groups = RMAosApiClient.GetGroupByIds(tenantId, groupIds);
                        await AccountDao.AddUserGroupMappingAsync(userId, groupIds);
                        userAndGroups.AddRange(groups);
                    }
                    else if (TenantLocalValue.UserGroups == null || TenantLocalValue.UserGroups.Count == 0)
                    {
                        await LnkUserGroupDao.RemoveUserNotInGroupAsync(userId);
                    }
                    foreach (var user in userAndGroups)
                    {
                        await SyncAOSUserGroupAsync(user, false);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while sync aos user group,ERROR:{0}", ex.ToString());
            }

        }


        public async System.Threading.Tasks.Task SyncTenantOnwerAsync()
        {
            try
            {
                var owner = RMAosApiClient.GetTenantInfo(TenantLocalValue.LogonGroupId);
                await SyncAOSUserGroupAsync(owner, false);
            }
            catch (Exception e)
            {
                logger.Error("error occurred while sync aos owner,ERROR:{0}", e.ToString());
            }
        }

        private void AddRoleType(string userId, Contract.RoleAssignments.RMRoleType type)
        {
            var role = RoleDao.GetRoleByAccountType(type);
            if (role != null)
            {
                LnkUserRoleDao.Create(new DB.Model.RMLnkUserRole()
                {
                    RoleId = role.RoleId,
                    UserId = userId
                });
            }
        }

        public AccountDto GetUserOrGroup(string id)
        {
            var user = AccountDao.Find(s => s.UserId == id && s.IsRemoved == 0);
            return ConvertToAccountDtoWithId(user);
        }

        public async Task<List<AccountDto>> GetUserGroupsAsync(string userId)
        {
            var userGroupIds = (await LnkUserGroupDao.FindListAsync(l => l.UserId == userId)).Select(l => l.GroupId);
            if (userGroupIds.Any())
            {
                var groups = await AccountDao.FindListAsync(g => userGroupIds.Contains(g.UserId));
                return groups.Select(g => ConvertToAccountDtoWithId(g)).ToList();
            }
            return null;
        }

        public async Task<List<AOSUserDto>> SearchUsersAsync(string groupId, string searchKey)
        {
            //原来从AOS筛选,有Status( u.Status != 1)判断.修改成从DB读,不加这个逻辑.
            List<AOSUserDto> lstDto = null;
            var lst = (await AccountDao.FindListAsync(u => (u.UserPrincipalName != null && u.UserPrincipalName.ToLower().Contains(searchKey)
                || u.DisplayName.ToLower().Contains(searchKey)) && u.IsRemoved == 0)).ToList();

            if (lst != null && lst.Count > 0)
            {
                lstDto = lst.Select(u => new AOSUserDto()
                {
                    DisplayName = u.DisplayName,
                    UserId = u.UserId,
                    RMUserId = u.Id,
                    UserPrincipalName = u.UserPrincipalName,
                    Id = u.AADId,
                    InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                }).ToList();
            }
            return lstDto ?? new List<AOSUserDto>();
        }        
        
        public async Task<List<ManualApprovalAOPUserInfo>> ManualSearchUsersAsync(string groupId, string searchKey)
        {
            //原来从AOS筛选,有Status( u.Status != 1)判断.修改成从DB读,不加这个逻辑.
            List<ManualApprovalAOPUserInfo> lstDto = null;
            var lst = (await AccountDao.FindListAsync(u => (u.UserPrincipalName != null && u.UserPrincipalName.ToLower().Contains(searchKey)
                || u.DisplayName.ToLower().Contains(searchKey)) && u.IsRemoved == 0)).ToList();

            if (lst != null && lst.Count > 0)
            {
                lstDto = lst.Select(u => new ManualApprovalAOPUserInfo()
                {
                    DisplayName = u.DisplayName,
                    UserId = u.UserId,
                    RMUserId = u.Id,
                    UserPrincipalName = u.UserPrincipalName,
                    Id = u.AADId,
                    InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                }).ToList();
            }
            return lstDto ?? new List<ManualApprovalAOPUserInfo>();
        }

        public async Task<List<AOSUserDto>> SearchUsersAsync(List<string> principalNames)
        {
            List<AOSUserDto> lstDto = null;
            List<RMAccount> lst = (await AccountDao.FindListAsync(u => (principalNames.Contains(u.UserPrincipalName) && u.IsRemoved == 0))).ToList();

            if (lst != null && lst.Count > 0)
            {
                lstDto = lst.Select(u => new AOSUserDto()
                {
                    DisplayName = u.DisplayName,
                    UserId = u.UserId,
                    RMUserId = u.Id,
                    UserPrincipalName = u.UserPrincipalName,
                    Id = u.AADId,
                    InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                }).ToList();
            }
            return lstDto ?? new List<AOSUserDto>();
        }

        public async Task<List<ManualApprovalAOPUserInfo>> ManualSearchUsersAsync(List<string> principalNames)
        {
            List<ManualApprovalAOPUserInfo> lstDto = null;
            List<RMAccount> lst = (await AccountDao.FindListAsync(u => (principalNames.Contains(u.UserPrincipalName) && u.IsRemoved == 0))).ToList();

            if (lst != null && lst.Count > 0)
            {
                lstDto = lst.Select(u => new ManualApprovalAOPUserInfo()
                {
                    DisplayName = u.DisplayName,
                    UserId = u.UserId,
                    RMUserId = u.Id,
                    UserPrincipalName = u.UserPrincipalName,
                    Id = u.AADId,
                    InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                }).ToList();
            }
            return lstDto ?? new List<ManualApprovalAOPUserInfo>();
        }

        public async Task<List<AOSUserDto>> SearchUsersWithoutDisplayNameAsync(string groupId, string searchKey)
        {
            //原来从AOS筛选,有Status( u.Status != 1)判断.修改成从DB读,不加这个逻辑.
            List<AOSUserDto> lstDto = null;
            var lst = (await AccountDao.FindListAsync(u => (u.UserPrincipalName.ToLower().Equals(searchKey)) && u.IsRemoved == 0)).ToList();

            if (lst != null && lst.Count > 0)
            {
                lstDto = lst.Select(u => new AOSUserDto()
                {
                    DisplayName = u.DisplayName,
                    UserId = u.UserId,
                    UserPrincipalName = u.UserPrincipalName,
                    InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                }).ToList();
            }
            return lstDto ?? new List<AOSUserDto>();
        }


        //查找删除的用户
        public async Task<AccountDto> GetGoogleUserAsync(string userId)
        {
            var googleUser = (await AccountDao.GetGoogleUserByUserIdsAsync([userId])).FirstOrDefault();
            if (googleUser != null)
            {
                return ConvertToAccountDto(googleUser);
            }

            return null;
        }

        public async Task<List<string>> SearchUsersRemovedAsync(List<string> lstUsers)
        {
            List<string> lst = new List<string>();
            var lstNotRemoved = (await AccountDao.FindListAsync(u => u.IsRemoved == 0 && lstUsers.Contains(u.UserPrincipalName))).Select(p => p.UserPrincipalName).ToList();
            foreach (string notremove in lstNotRemoved)
            {
                if (lstUsers.Contains(notremove))
                {
                    lstUsers.Remove(notremove);
                }
            }
            var lstRemoved = (await AccountDao.FindListAsync(u => u.IsRemoved == 1)).Select(p => p.UserPrincipalName).ToList();
            lst = lstUsers.Where(p => lstRemoved.Contains(p)).ToList();
            return lst;
        }

        public async Task<UserQueryResult> QueryUsersAsync(UserQueryParams queryDto)
        {
            var result = new UserQueryResult();
            var usersInfo = new List<SecurityUserDto>();
            var dbUsers = AccountDao.QueryUsers(queryDto, out int totalCount);
            if (dbUsers != null && dbUsers.Count > 0)
            {
                foreach (var user in dbUsers)
                {
                    var userAndGroupIds = await GetUserAndGroupUserIdsAsync(user.UserId);
                    var groupNames = RMSecurityGroupDao.GetGroupNames(userAndGroupIds);
                    usersInfo.Add(new SecurityUserDto
                    {
                        UserId = user.UserId,
                        DisplayName = user.DisplayName,
                        UserPrincipalName = user.UserPrincipalName,
                        SecurityGroupNames = groupNames
                    });
                }
            }
            result.TotalCount = totalCount;
            result.Users = usersInfo;
            return result;
        }

        public AOSUserDto ConvertRMAccountToAOSUserDto(RMAccount u)
        {
            return new AOSUserDto()
            {
                DisplayName = u.DisplayName,
                UserId = u.UserId,
                RMUserId = u.Id,
                UserPrincipalName = u.UserPrincipalName,
                InviteType = u.ObjectType == RMActiveDirectoryObjectType.User || u.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
            };
        }

        public async Task<List<AOSUserDto>> GetUsersByIdsAsync(List<int> ids)
        {
            var usersDto = new List<AOSUserDto>();
            var accounts = (await AccountDao.FindListAsync(o => ids.Contains(o.Id) && o.IsRemoved == 0)).ToList();
            if (accounts != null && accounts.Count > 0)
            {
                accounts.ForEach(o =>
                {
                    usersDto.Add(ConvertRMAccountToAOSUserDto(o));
                });
            }
            return usersDto;
        }
        /// <summary>
        /// Did not show User in ad group in User Management
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<AOSUserDto>> GetUsersByIdsAsync(List<string> ids)
        {
            var usersDto = new List<AOSUserDto>();
            var accounts = (await AccountDao.FindListAsync(o => ids.Contains(o.UserId) && o.IsRemoved == 0 && o.ObjectType != RMActiveDirectoryObjectType.UserInGroup)).ToList();//RECO-11692
            if (accounts != null && accounts.Count > 0)
            {
                accounts.ForEach(o =>
                {
                    usersDto.Add(ConvertRMAccountToAOSUserDto(o));
                });
            }
            return usersDto;
        }

        public AOSUserDto GetUserByUserId(string userId)
        {
            var account = AccountDao.Find(o => userId == o.UserId && o.IsRemoved == 0);
            return account != null ? ConvertRMAccountToAOSUserDto(account) : null;
        }

        public Task<List<string>> GetGroupIdsAsync(string userId)
        {
            return LnkUserGroupDao.GetAllGroupIdsAsync(userId);
        }

 public List<int> GetUserWithRemovedAndGroupIds(string userId)
        {
            try
            {
                var groupUniqueIds = LnkUserGroupDao.GetAllGroupIdsAsync(userId).GetAwaiter().GetResult();
                var userAndGroupIds = new List<string>(groupUniqueIds)
                {
                    userId
                };

                var accounts = AccountDao.GetUserByUserIdsAsync(userAndGroupIds).GetAwaiter().GetResult();
                var accountPrincipalName = accounts.First(item => item.UserId == userId).UserPrincipalName;
                var principalNameQueriedAccounts = AccountDao.GetUserWithRemovedByPrincipalNames(new List<string> { accountPrincipalName });
                accounts.AddRange(principalNameQueriedAccounts);
                return accounts.Select(item => item.Id).ToHashSet().ToList();
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while get user [{userId}] with removed and group id. Error: {e}");
                throw;
            }
        }
        public IEnumerable<int> GetUserWithIdWithOutRemoved(string userId)
        {
            try
            {
                var accountsId = AccountDao.GetUserByUserIdsAsync(new List<string> { userId }).GetAwaiter().GetResult().Select(item => item.Id);
                return accountsId;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting user [{userId}] with removed and group id. Error: {e}");
                throw;
            }
        }

        public async Task<List<int>> GetUserAndGroupIdsAsync(string userId)
        {
            try
            {
                var userAndGroupIds = new List<string>();
                userAndGroupIds.Add(userId);
                var groupUniqueIds = await LnkUserGroupDao.GetAllGroupIdsAsync(userId);
                if (groupUniqueIds.Count > 0)
                {
                    userAndGroupIds.AddRange(groupUniqueIds);
                }
                return await AccountDao.GetIdsOfUserByUserIdsAsync(userAndGroupIds);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when GetAccountAndGroupIds, message:{ex.ToString()}");
                return null;
            }
        }
        /// <summary>
        /// Need add to Redis or memory cache To do important.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUserAndGroupUserIdsAsync(string userId)
        {
            try
            {
                var userAndGroupIds = new List<string>();
                userAndGroupIds.Add(userId);
                var groupUniqueIds = await LnkUserGroupDao.GetAllGroupIdsAsync(userId);
                if (groupUniqueIds.Count > 0)
                {
                    userAndGroupIds.AddRange(groupUniqueIds);
                }
                var accounts = await AccountDao.GetUserByUserIdsAsync(userAndGroupIds);
                return accounts.Select(o => o.UserId).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when GetAccountAndGroupIds, message:{ex.ToString()}");
                return null;
            }
        }

        public async System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<AOSUserDto> users, bool needAddUserToSecurityGroupMembership = true, DefaultAddedSecurityGroupType groupType = DefaultAddedSecurityGroupType.BuiltInReviewUserGroup)
        {
            try
            {
                if (await MultiGEOSettingService.IsEnableMultiGeoFeature())
                {
                    await SyncAosUsersMultiGeoAsync(tenantId, users);
                }
                else
                {

                    var needRegisterUsers = await GetNeedSyncADUsersAsync(users, needAddUserToSecurityGroupMembership);
                    if (needRegisterUsers.Count != 0)
                    {
                        var adAccounts = AccountWrapperService.Regester2AOS(tenantId, needRegisterUsers);
                        CheckRegisterUsersResult(adAccounts);
                        UpdateUserIdForUi(users, adAccounts);
                        var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                        await BatchAddAccountsAsync(accountsDto);
                        UpdateRMUserIdForUi(users, accountsDto);
                        //把没同步到Records的AADAccount放到SecurityGroupMembership中
                    }
                }

                if (needAddUserToSecurityGroupMembership && users.Count > 0)
                {
                    SaveUsersToBuiltInGroup(users.Select(u => u.UserId).ToList(), groupType);
                }

            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when SyncADUsers,{ex.ToString()}");
                throw;
            }
        }

        public void SaveUsersToBuiltInGroup(List<string> userIds, DefaultAddedSecurityGroupType groupType = DefaultAddedSecurityGroupType.BuiltInReviewUserGroup)
        {
            logger.Info($"Add user to {groupType}, userIs: {userIds}");
            if (userIds != null && userIds.Count > 0)
            {
                var action = GetSaveBuiltInGroupAction(groupType);
                action?.Invoke(userIds);
            }
        }

        private Action<List<string>> GetSaveBuiltInGroupAction(DefaultAddedSecurityGroupType groupType)
        {
            Dictionary<DefaultAddedSecurityGroupType, Action<List<string>>> actions = new()
            {
                { DefaultAddedSecurityGroupType.BuiltInEndUserGroup, (userIds) => { SaveToBuiltInEndUserGroup(userIds); } },
                { DefaultAddedSecurityGroupType.BuiltInReviewUserGroup, (userIds) => { SaveToBuiltInReviewUserGroup(userIds); } }
            };
            if (actions.ContainsKey(groupType))
            {
                return actions[groupType];
            }
            return null;
        }

        private void SaveToBuiltInEndUserGroup(List<string> userIds)
        {
            logger.Info($"Add user to built-in end user group, userIs: {userIds}");
            SecurityGroupMembershipDao.AddOrUpdateUserToGroupMemberShips((int)RMAccountType.RegisteredUser, userIds);
        }

        private void SaveToBuiltInReviewUserGroup(List<string> userIds)
        {
            int groupId = RMSecurityGroupDao.GetBuitInReviewUserGroupId();
            if (groupId > 0)
            {
                logger.Info($"Add user to built-in review user group, userIs: {userIds}");
                SecurityGroupMembershipDao.AddOrUpdateUserToGroupMemberShips(groupId, userIds);
            }
        }

        private void UpdateUserIdForUi(List<AOSUserDto> users, List<AADAccount> adAccounts)
        {
            users.ForEach(u =>
            {
                if (string.IsNullOrEmpty(u.UserId) && adAccounts.Any(o => o.Id.Equals(u.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    var adAccount = adAccounts.Where(o => o.Id.Equals(u.Id)).FirstOrDefault();
                    u.UserId = adAccount?.AccountId;//从AOS返回的string类型userId
                }
            });
        }

        private void UpdateRMUserIdForUi(List<AOSUserDto> users, List<AccountDto> dbAccounts)
        {
            users.ForEach(u =>
            {
                if (u.RMUserId == 0 && dbAccounts.Any(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    var dbAccount = dbAccounts.Where(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)).First();
                    u.RMUserId = dbAccount.Id;//在RMAccount表添加user后返回的int类型id
                }
            });
        }

        private async Task<List<AADAccount>> GetNeedSyncADUsersAsync(List<AOSUserDto> users, bool needAddUserToSecurityGroupMembership = true)
        {
            var aadAccounts = new List<AADAccount>();
            var userAADIds = users.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id).Distinct().ToList();
            var userPrincipalNames = users.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName).Distinct().ToList();
            //已经注册过的的AADAcount
            var existAccounts = await GetExistAccountsAsync(userAADIds, userPrincipalNames);
            var existAADIds = existAccounts.Item1;
            var existUserPrincipalNames = existAccounts.Item2;
            //检查已经注册AOS的AADAcount，查看是否已同步到SecurityGroupMembership
            //ID不为空，UserID为空：AOS未注册过此AADAcount，此处不需要处理同步SecurityGroupMembership
            //ID为空，UserID不为空：AOS注册过此AADAcount，此处需要处理同步SecurityGroupMembership
            if (needAddUserToSecurityGroupMembership)
            {
                //SaveUsersToBuiltInGroup(users.Where(x => !string.IsNullOrEmpty(x.UserId)).Select(x => x.UserId).ToList());
            }
            //需要注册的user
            var matchUsers = users.Where(o => !existAADIds.Contains(o.Id) && !existUserPrincipalNames.Contains(o.UserPrincipalName)).ToList();
            aadAccounts = matchUsers.Select(o => AADAccount.Convert2AADAccountDto(o)).ToList();
            return aadAccounts;
        }

        /// <summary>
        /// 返回已经存在的AAD list和UserPrincipalName List.
        /// 返回结果 : Item1: 已经存在的AAD list ; Item2: UserPrincipalName List
        /// </summary>
        /// <param name="userAADIds"></param>
        /// <param name="userPrincipalNames"></param>
        /// <returns>Item1: 已经存在的AAD list ; Item2: UserPrincipalName List</returns>
        private async Task<Tuple<List<string>, List<string>>> GetExistAccountsAsync(List<string> userAADIds, List<string> userPrincipalNames)
        {
            //已经注册过的的AADAcount
            var existAccounts = (await AccountDao.FindListAsync(o => (userAADIds.Contains(o.UserId) || userAADIds.Contains(o.AADId) || userPrincipalNames.Contains(o.UserPrincipalName)) && o.IsRemoved == 0))
                .Select(o => new { o.UserId, o.AADId, o.UserPrincipalName })
                .ToList();
            var existAADIds = new List<string>();

            existAccounts.ForEach(account =>
            {
                if (!string.IsNullOrEmpty(account.AADId) && userAADIds.Contains(account.AADId))
                {
                    existAADIds.Add(account.AADId);
                }
                else if (userAADIds.Contains(account.UserId))
                {
                    existAADIds.Add(account.UserId);
                }
            }
            );

            //existAADIds = existAccounts.Where(o => !string.IsNullOrEmpty(o.AADId)).Select(o => o.AADId).ToList();
            var existUserPrincipalNames = existAccounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName).ToList();
            var tup = Tuple.Create(existAADIds, existUserPrincipalNames);

            return tup;
        }

        //ToUserInfo
        public async System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<ToUserInfo> users)
        {
            try
            {
                if (await MultiGEOSettingService.IsEnableMultiGeoFeature())
                {
                    await SyncToUsersMultiGeoAsync(tenantId, users);
                }
                else
                {
                    var needRegisterUsers = await GetNeedSyncADUsersAsync(users);
                    if (needRegisterUsers.Count > 0)
                    {
                        var adAccounts = AccountWrapperService.Regester2AOS(tenantId, needRegisterUsers);
                        CheckRegisterUsersResult(adAccounts);
                        UpdateUserIdForUi(users, adAccounts);
                        var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                        await BatchAddAccountsAsync(accountsDto);
                        UpdateRMUserIdForUi(users, accountsDto);
                    }
                    //把没同步到Records的AADAccount放到SecurityGroupMembership中
                    SaveUsersToBuiltInGroup(users.Select(u => u.UserId).ToList());
                }     
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when SyncADUsers,{ex.ToString()}");
                throw;
            }
        }

        #region MultiGeo
        public async System.Threading.Tasks.Task SyncCommonUsersInfoToMainDCAsync(SyncCommonDataUserInfo commonDataUser)
        {
            try
            {
                var adAccounts = AccountWrapperService.Regester2AOS(commonDataUser.TenantId, commonDataUser.UsersInfo);
                CheckRegisterUsersResult(adAccounts);
                var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                await BatchAddAccountsAsync(accountsDto);
                await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                              accountsDto,
                              MultiGeoOperationType.SyncUsers);
            }
            catch (Exception ex)
            {
                MultiGeoReplicaFailureLogWriter.WriteForSync(MultiGeoOperationType.SyncUsers.ToString(), TenantLocalValue.LogonUserId);

                logger.Error($"An error occured when SyncCommonToUsersInfoToMainDC,{ex.ToString()}");
                throw;
            }
        }

        private async System.Threading.Tasks.Task SyncToUsersMultiGeoAsync(string tenantId, List<ToUserInfo> users)
        {
            try
            {
                var needRegisterUsers = await GetNeedSyncADUsersAsync(users);
                if (needRegisterUsers.Count > 0)
                {
                    if (await RAMultiGeoClient.ShouldPostToMainDcAsync())
                    {
                        var success = await TrySyncUserInfoToMainDCAsync(needRegisterUsers, tenantId);
                        if (!success)
                        {
                            logger.Error($"Sync ToUserInfo to Main DC failed, tenantId:{tenantId}, users count:{users.Count}");
                            throw new Exception($"SyncUsers to Main DC failed, tenantId:{tenantId}, users count:{users.Count}");
                        }
                        else
                        {
                            var userResult = AccountWrapperService.GetAADAccounts(needRegisterUsers, TenantLocalValue.LogonGroupId);
                            UpdateUserIdForUi(users, userResult);

                            var accountsDto = userResult.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                            UpdateRMUserIdForUi(users, accountsDto);
                        }
                    }
                    else
                    {
                        var adAccounts = AccountWrapperService.Regester2AOS(tenantId, needRegisterUsers);
                        CheckRegisterUsersResult(adAccounts);
                        UpdateUserIdForUi(users, adAccounts);

                        var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                        await BatchAddAccountsAsync(accountsDto);
                        UpdateRMUserIdForUi(users, accountsDto);

                        await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                              accountsDto,
                              MultiGeoOperationType.SyncUsers);

                    }
                }
                SaveUsersToBuiltInGroup(users.Select(u => u.UserId).ToList());
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when SyncToUsersMultiGeo,{ex.ToString()}");
                throw;
            }
        }

        private async System.Threading.Tasks.Task SyncAosUsersMultiGeoAsync(string tenantId, List<AOSUserDto> users)
        {
            try
            {
                var needRegisterUsers = await GetNeedSyncADUsersAsync(users);
                if (needRegisterUsers.Count > 0)
                {
                    if (await RAMultiGeoClient.ShouldPostToMainDcAsync())
                    {
                        var success = await TrySyncUserInfoToMainDCAsync(needRegisterUsers, tenantId);
                        if (!success)
                        {
                            logger.Error($"Sync AosUsers to Main DC failed, tenantId:{tenantId}, users count:{users.Count}");
                            throw new Exception($"Sync AosUsers to Main DC failed, tenantId:{tenantId}, users count:{users.Count}");
                        }
                        else
                        {
                            var userResult = AccountWrapperService.GetAADAccounts(needRegisterUsers,TenantLocalValue.LogonGroupId);
                            UpdateUserIdForUi(users, userResult);

                            var accountsDto = userResult.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                            UpdateRMUserIdForUi(users, accountsDto);
                        }
                    }
                    else
                    {
                        var adAccounts = AccountWrapperService.Regester2AOS(tenantId, needRegisterUsers);
                        CheckRegisterUsersResult(adAccounts);
                        UpdateUserIdForUi(users, adAccounts);

                        var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                        await BatchAddAccountsAsync(accountsDto);
                        UpdateRMUserIdForUi(users, accountsDto);

                        await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                               accountsDto,
                               MultiGeoOperationType.SyncUsers);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when SyncAosUsersMultiGeo,{ex.ToString()}");
                throw;
            }
        }

        private async Task<bool> TrySyncUserInfoToMainDCAsync(List<AADAccount> users, string tenantId)
        {
            try
            {
                var accounts = new SyncCommonDataUserInfo
                {
                    TenantId = tenantId,
                    UsersInfo = users
                };
                var synced = await RAMultiGeoClient.PostToMainDcAsync<SyncCommonDataUserInfo, bool>(
                    accounts,
                    MultiGeoOperationType.SyncUsersToMainDC);

                return synced;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
        private void UpdateUserIdForUi(List<ToUserInfo> users, List<AADAccount> adAccounts)
        {
            users.ForEach(u =>
            {
                if (string.IsNullOrEmpty(u.UserId) && adAccounts.Any(o => o.Id.Equals(u.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    var adAccount = adAccounts.Where(o => o.Id.Equals(u.Id)).First();
                    if (adAccount?.InviteType == AccountType.Group)
                    {
                        u.UserId = adAccount.Id;
                    }
                    else
                    {
                        u.UserId = adAccount?.AccountId;//从AOS返回的string类型userId
                    }
                }
            });
        }

        private void UpdateRMUserIdForUi(List<ToUserInfo> users, List<AccountDto> dbAccounts)
        {
            users.ForEach(u =>
            {
                if (u.RMUserId == 0 && dbAccounts.Any(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    var dbAccount = dbAccounts.Where(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)).First();
                    u.RMUserId = dbAccount.Id;//在RMAccount表添加user后返回的int类型id
                }
            });
        }

        private async Task<List<AADAccount>> GetNeedSyncADUsersAsync(List<ToUserInfo> users)
        {
            var userAADIds = users.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id).Distinct().ToList();
            var userPrincipalNames = users.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName).Distinct().ToList();
            //已经注册过的的AADAcount
            var existAccounts = await GetExistAccountsAsync(userAADIds, userPrincipalNames);
            var existAADIds = existAccounts.Item1;
            var existUserPrincipalNames = existAccounts.Item2;
            //检查已经注册AOS的AADAcount，查看是否已同步到SecurityGroupMembership
            //ID不为空，UserID为空：AOS未注册过此AADAcount，此处不需要处理同步SecurityGroupMembership
            //ID为空，UserID不为空：AOS注册过此AADAcount，此处需要处理同步SecurityGroupMembership
            //SaveUsersToBuiltInGroup(users.Where(x => !string.IsNullOrEmpty(x.UserId)).Select(x => x.UserId).ToList());
            //需要注册的user
            var matchUsers = users.Where(o => !existAADIds.Contains(o.Id) && !existUserPrincipalNames.Contains(o.UserPrincipalName)).ToList();
            var aadAccounts = matchUsers.Select(o => AADAccount.Convert2AADAccountDto(o)).ToList();
            return aadAccounts;
        }

        public async System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<AADAccount> users, string office365TenantId)
        {
            try
            {
                var needRegisterUsers = await GetNeedSyncADUsersAsync(users);
                if (needRegisterUsers.Count == 0)
                {
                    return;
                }
                var adAccounts = AccountWrapperService.Regester2AOS(tenantId, office365TenantId, needRegisterUsers).ToList();
                CheckRegisterUsersResult(adAccounts);
                var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                await BatchAddAccountsAsync(accountsDto);
                SaveUsersToBuiltInGroup(accountsDto.Select(u => u.UserId).ToList());
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while sync users. Error: {e}");
                throw;
            }
        }

        public async Task<bool> SyncUsersAsync(string tenantId, List<AADAccount> users)
        {
            try
            {
                var adAccounts = AccountWrapperService.Regester2AOS(tenantId, users);
                CheckRegisterUsersResult(adAccounts);
                var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                await BatchAddAccountsAsync(accountsDto);
                SaveUsersToBuiltInGroup(accountsDto.Select(u => u.UserId).ToList());
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while sync users. Error: {e}");
                return false;
            }
        }

        //ReviewerUser
        public async System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<ReviewerUser> users)
        {
            try
            {
                var needRegisterUsers = await GetNeedSyncADUsersAsync(users);
                if (needRegisterUsers.Count > 0)
                {
                    var adAccounts = AccountWrapperService.Regester2AOS(tenantId, needRegisterUsers.DistinctBy(a => a.Id).ToList());
                    CheckRegisterUsersResult(adAccounts);
                    UpdateUserIdForUi(users, adAccounts);
                    var accountsDto = adAccounts.Select(o => AADAccount.Convert2AccountDto(o)).ToList();
                    await BatchAddAccountsAsync(accountsDto);
                    UpdateRMUserIdForUi(users, accountsDto);
                    if (await MultiGEOSettingService.IsEnableMultiGeoFeature())
                    {
                        await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                        accountsDto,
                        MultiGeoOperationType.SyncUsers);
                    }
                }

                //把没同步到Records的AADAccount放到SecurityGroupMembership中
                SaveUsersToBuiltInGroup(users.Select(u => u.UserId).ToList());
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured when SyncADUsers,{ex.ToString()}");
                throw;
            }
        }

        private void UpdateUserIdForUi(List<ReviewerUser> users, List<AADAccount> adAccounts)
        {
            users.ForEach(u =>
            {
                if (string.IsNullOrEmpty(u.UserId) && adAccounts.Any(o => o.Id.Equals(u.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    var adAccount = adAccounts.Where(o => o.Id.Equals(u.Id)).FirstOrDefault();
                    u.UserId = adAccount.AccountId;//从AOS返回的string类型userId
                }
            });
        }

        private void UpdateRMUserIdForUi(List<ReviewerUser> users, List<AccountDto> dbAccounts)
        {
            users.ForEach(u =>
            {
                if (u.RMUserId == 0 && dbAccounts.Any(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)))
                {
                    var dbAccount = dbAccounts.Where(o => o.UserId.Equals(u.UserId, StringComparison.OrdinalIgnoreCase)).First();
                    u.RMUserId = dbAccount.Id;//在RMAccount表添加user后返回的int类型id
                }
            });
        }

        private async Task<List<AADAccount>> GetNeedSyncADUsersAsync(List<AADAccount> users)
        {
            var userPrincipalNames = users.Select(item => item.UserPrincipalName);
            var existUsers = await AccountDao.FindListAsync(item => userPrincipalNames.Contains(item.UserPrincipalName) && item.IsRemoved == 0);
            existUsers.ForEach(item =>
            {
                var aadUser = users.Find(user => user.UserPrincipalName == item.UserPrincipalName);
                if (aadUser != null)
                {
                    aadUser.AccountId = item.UserId;
                }
            });

            var existUserPrincipalNames = existUsers.Select(item => item.UserPrincipalName);
            var needSyncUsers = users.FindAll(item => !existUserPrincipalNames.Contains(item.UserPrincipalName));
            return needSyncUsers;
        }

        private async Task<List<AADAccount>> GetNeedSyncADUsersAsync(List<ReviewerUser> users)
        {
            var userAADIds = users.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id).Distinct().ToList();
            var userPrincipalNames = users.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName).Distinct().ToList();
            //已经注册过的的AADAcount
            var existAccounts = await GetExistAccountsAsync(userAADIds, userPrincipalNames);
            var existAADIds = existAccounts.Item1;
            var existUserPrincipalNames = existAccounts.Item2;
            //检查已经注册AOS的AADAcount，查看是否已同步到SecurityGroupMembership
            //ID不为空，UserID为空：AOS未注册过此AADAcount，此处不需要处理同步SecurityGroupMembership
            //ID为空，UserID不为空：AOS注册过此AADAcount，此处需要处理同步SecurityGroupMembership
            //SaveUsersToBuiltInGroup(users.Where(x => !string.IsNullOrEmpty(x.UserId)).Select(x => x.UserId).ToList());
            //需要注册的user
            var matchUsers = users.Where(o => !existAADIds.Contains(o.Id) && !existUserPrincipalNames.Contains(o.UserPrincipalName)).ToList();
            var aadAccounts = matchUsers.Select(o => AADAccount.Convert2AADAccountDto(o)).ToList();
            return aadAccounts;
        }

        private void CheckRegisterUsersResult(List<AADAccount> accounts)
        {
            //注册失败AccountId没有值
            if (accounts.Any(o => string.IsNullOrEmpty(o.AccountId)))
            {
                var failedAccounts = accounts.Where(o => string.IsNullOrEmpty(o.AccountId)).Select(o => new { o.Id, o.DisplayName }).ToDictionary(o => o.Id, o => o.DisplayName);
                logger.Warn($"Failed to sync aos, AADAccount names:[{string.Join(",", failedAccounts.Values)}],ids:[{string.Join(",", failedAccounts.Keys)}].");
                throw new Exception("There are users who failed to register.");
            }
        }

        public async Task<AccountDto> GetUserByNameAsync(string name)
        {
            return ConvertToAccountDtoWithId(await AccountDao.GetActiveUserByNameAsync(name));
        }

        public string GetReviewerFirstName(string userId)
        {
            //思路：item.ObjectType.ToString().Equals("User") ? (item.FirstName ?? "aos里面找name" ?? item.DisplayName) : item.DisplayName,
            var accountDb = AccountDao.GetUserByUserIdsAsync(new List<string> { userId }).GetAwaiter().GetResult().FirstOrDefault();
            if(accountDb == null)
            {
                logger.Info($"Can not find account by user id : {userId}");
                return " ";
            }
            try
            {
                if (accountDb.ObjectType != RMActiveDirectoryObjectType.User && accountDb.ObjectType != RMActiveDirectoryObjectType.UserInGroup)
                {
                  return accountDb.DisplayName;
                }
                if (string.IsNullOrEmpty(accountDb.FirstName))
                {
                    var account = RMAosApiClient.GetUserByPrincipalName(accountDb.UserPrincipalName).GetAwaiter().GetResult();
                    if (account == null)
                    {
                        return accountDb.DisplayName;
                    }
                    AccountDao.UpdateByUserId(account.FirstName, account.LastName, DateTime.UtcNow.Ticks, userId);
                    return account.FirstName;
                }
                return accountDb.FirstName;
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while get user: [{userId}] first name. Error: {e}");
            }

            return accountDb.DisplayName;
        }

        public string GetRequesterFirstName(string loginUserId) 
        {
            var tenantDao = TenantDao.GetTenantInfo(loginUserId);
            if (tenantDao == null)
            {
                logger.Info($"Can not find TenantInfo by LoginUser id : {loginUserId}");
                return string.Empty;
            }
           
            var accountDb = AccountDao.GetUserWithRemovedByPrincipalNames(new List<string>() { tenantDao.RegisterEmail }).FirstOrDefault();

            if (accountDb == null)
            {
                logger.Info($"Can not find TenantInfo by LoginUser id : {loginUserId}");
                return string.Empty;
            }

            return GetReviewerFirstName(accountDb.UserId);

        }
        public string GetReviewerFirstNameForExportZip(string userId, string toUseremail)
        {
            var accountDb = AccountDao.GetUserByUserIdsAsync(new List<string> { userId }).GetAwaiter().GetResult().FirstOrDefault();

            if(accountDb != null) 
            {
                if (accountDb.FirstName != null)
                {
                    return accountDb.FirstName;
                }
                GetReviewerFirstName(userId);
            }

            try
            {
                var aADAcount = AccountWrapperService.GetAccount(TenantLocalValue.LogonGroupId, toUseremail);
                if (aADAcount != null )
                {
                    if (aADAcount.GivenName !=null)
                    {
                        return aADAcount.GivenName;
                    }
                    else if (aADAcount.DisplayName != null)
                    {
                        return aADAcount.DisplayName;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Failed to find user in DB and AD. Error: {e}");
            }

            return " ";
        }
    }
}

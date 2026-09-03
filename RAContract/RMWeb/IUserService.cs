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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IUserService
    {
        #region Account Pool user
        List<PoolUserDto> GetPoolUsers();
        void UpdatePoolUserUsage(string userName, string tenantId, bool isAdd);
        void AddPoolUser(PoolUserDto user);
        PoolUserDto GetAvailableUser(string tenantId);
        PoolUserDto GetPoolUserByName(string tenantId, string userName);
        void UpdatePoolUserStatus(string userName, string tenantId, int status);
        #endregion

        #region User Management

        Task<List<AccountDto>> GetManagementUsersAsync();

        Task<List<AOSUserDto>> GetManagementUsersForAosDtoAsync();

        Task<List<AccountDto>> GetApplicationAdminsAsync();

        System.Threading.Tasks.Task CreateMUserAsync(AccountDto account);
        System.Threading.Tasks.Task SyncAosUsersAsync();
        System.Threading.Tasks.Task SyncLogonUserGroupAsync(string userId);
        System.Threading.Tasks.Task SyncTenantOnwerAsync();
        Task<List<AOSUserDto>> SearchUsersAsync(string groupId, string searchKey);
        Task<List<ManualApprovalAOPUserInfo>> ManualSearchUsersAsync(string groupId, string searchKey);
        Task<List<AOSUserDto>> SearchUsersAsync(List<string> principalNames);
        Task<List<ManualApprovalAOPUserInfo>> ManualSearchUsersAsync(List<string> principalNames);
        Task<List<AOSUserDto>> SearchUsersWithoutDisplayNameAsync(string groupId, string searchKey);
        AccountDto GetUserOrGroup(string id);
        Task<AccountDto> GetUserByNameAsync(string name);
        Task<AccountDto> GetGoogleUserAsync(string userId);
        Task<List<string>> SearchUsersRemovedAsync(List<string> users);
        Task<List<AccountDto>> GetUserGroupsAsync(string userId);
        
        Task<UserQueryResult> QueryUsersAsync(UserQueryParams queryDto);

        Task<List<int>> GetUserAndGroupIdsAsync(string userId);
        List<int> GetUserWithRemovedAndGroupIds(string userId);
        Task<List<string>> GetUserAndGroupUserIdsAsync(string userId);
        Task<List<AOSUserDto>> GetUsersByIdsAsync(List<int> ids);
        Task<List<AOSUserDto>> GetUsersByIdsAsync(List<string> ids);

        /// <summary>
        /// get active user by userid
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        AOSUserDto GetUserByUserId(string userId);
        /// <summary>
        /// check if user is the member of the security group
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool IsMemberOfSecurityGroup(int groupId, string userId);
        List<int> GetAllGroupIds(List<string> userAndGroupIds);
        /// <summary>
        /// 批量注册accounts
        /// </summary>
        /// <param name="dtos"></param>
        System.Threading.Tasks.Task BatchAddAccountsAsync(List<AccountDto> dtos);
        System.Threading.Tasks.Task SyncAdminAccountForMultiGeoTenantOtherDCAsync(List<AccountDto> accounts);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="users"></param>
        /// <param name="needAddUserToSecurityGroupMembership">Security Group 界面add user外围会处理SecurityGroupMembership，不需要SyncADUsers处理</param>
        System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<AOSUserDto> users, bool needAddUserToSecurityGroupMembership = true, DefaultAddedSecurityGroupType groupType = DefaultAddedSecurityGroupType.BuiltInReviewUserGroup);
        System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<ToUserInfo> users);
        System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<ReviewerUser> users);
        System.Threading.Tasks.Task SyncCommonUsersInfoToMainDCAsync(SyncCommonDataUserInfo commonDataUser);

        Task<bool> SyncUsersAsync(string tenantId, List<AADAccount> users);

        System.Threading.Tasks.Task SyncUsersAsync(string tenantId, List<AADAccount> users, string office365TenantId);

        //string CalcUserPermission(string tenantId, string userId);
        //RMPermissionMasks CalcUserPermissionMasks(string tenantId, string userId);
        //bool EqualsThisPermission(string customerId, string userId, RMPermissionMasks mask, bool userCache = false);
        /// <summary>
        /// Check User权限
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="userId"></param>
        /// <param name="mask"></param>
        /// <param name="userCache"></param>
        /// <param name="andOr">
        /// default true, 
        /// true: mask中的权限是and关系, 需要都满足条件
        /// or:  mask中的权限是or关系, 满足一个条件即可
        /// </param>
        /// <returns></returns>
        [Obsolete("replace by IRMSecurityTrimmingHelper")]
        //bool DoesUserHasThisPermission(string customerId, string userId, RMPermissionMasks mask, bool userCache = true, bool andOr = true);
        void SaveUsersToBuiltInGroup(List<string> userIds, DefaultAddedSecurityGroupType groupType = DefaultAddedSecurityGroupType.BuiltInReviewUserGroup);
        //string CalcUserSubPermission(string userId);
        //bool DoesUserHasThisSubPermission(string customerId, string userId, RMSubPermissionMasks mask, bool userCache = false);
        #endregion
        Task<List<string>> GetGroupIdsAsync(string userId);
        string GetReviewerFirstName(string userId);
        string GetRequesterFirstName(string loginUserId);
        string GetReviewerFirstNameForExportZip(string userId,string toUseremail);

        IEnumerable<int> GetUserWithIdWithOutRemoved(string userId);
    }
}

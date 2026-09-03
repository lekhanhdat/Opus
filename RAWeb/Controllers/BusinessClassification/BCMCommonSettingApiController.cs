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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser, preferred: false)]
    public class BCMCommonSettingApiController : BaseApiController
    {
        #region interface
        public IGlobalSettingService _GlobalSettingService;
        public IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService(ref _GlobalSettingService);
        public ITaxonomyService _TaxonomyService;
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        public IUserService _UserSerive;
        public IUserService UserSerive => PlatformWindsorManager.GetService(ref _UserSerive);
        public IManualApprovalService _ManualApprovalService;
        public IManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);
        public IAccountWrapperService _AccountWrapperService;
        public IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService(ref _AccountWrapperService);
        public ISecurityGroupManagementService _SecurityGroupManagementService;
        public ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService(ref _SecurityGroupManagementService);
        #endregion

        #region Load Term
        /// <summary>
        /// 设置term default value时，获取tree的root节点
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser | RMPermissionMasks.PhysicalEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [HttpPost]
        public async Task<string> GetRootNodeOfDefaultTermTree([FromBody] TreePage tree)
        {
            if (string.IsNullOrEmpty(tree.NodeId))
            {
                Logger.Warn("NodeId is null or empty");
                throw new ArgumentNullException("NodeId");
            }
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = tree.ForPhysicalView
            };
            if (tree.NodeType.Equals("TermSet", StringComparison.OrdinalIgnoreCase))
            {
                if (Guid.TryParse(tree.NodeId, out Guid termSetId) && !(await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, Contract.RMWeb.CP.SecurityTermLevel.TermSet, new List<Guid> { termSetId }, filterOption)))
                {
                    return "";
                }
                return TaxonomyService.GetTermSetByTermSetId(tree.NodeId);
            }
            else
            {
                if (Guid.TryParse(tree.NodeId, out Guid termId) && !(await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, Contract.RMWeb.CP.SecurityTermLevel.Term, new List<Guid> {termId }, filterOption)))
                {
                    return "";
                }
                return TaxonomyService.GetTermByTermId(tree.NodeId);
            }
        }

        /// <summary>
        /// 获取setting term tree 子节点
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManualReviewEnduser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [HttpPost]
        [ValidTermTreeParameterFilter("GetChildrenTreeNodes")]
        public Task<string> GetChildrenTreeNodes([FromBody] TreePage tree)
        {
            int pIndex = tree.PageIndex ?? 0;
            int pSize = tree.PageSize ?? 0;

            //调整一下index，和前台匹配
            if (pIndex > 0)
            {
                pIndex -= 1;
            }

            string nodeId = tree.NodeId ?? string.Empty;
            string nodeType = tree.NodeType ?? string.Empty;
            int SettingType = tree.SettingType != null ? Convert.ToInt32(tree.SettingType) : 0;
            var filterOption = new FilterTermObjOption
            {
                NeedCheckPermission = true,
                FilterByContentSource = true,
                ExcludeBuiltIn = tree.ExcludeBuiltIn,
                SourceFlag = tree.SourceFlag,
                ContainerId = tree.ContainerId,
                ForPhysicalView = tree.ForPhysicalView
            };

            return TaxonomyService.GetTaxonomyTreeDataAsync(nodeType, nodeId, pIndex, pSize, tree.SPTreeNodes, SettingType, filterOption);
        }

        //[HttpPost]
        //public string GetTermTree([FromBody] CurrentSettingsInfo settingInfo)
        //{
        //    return TaxonomyService.GetTermTree(settingInfo);
        //}
        #endregion

        #region Validate DA
        [RACodeReview("Allen Yin")]
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyEnduser)]
        public ValidationMessage ValidateDAConnectionSetting()
        {
            return GlobalSettingService.CheckDocAveConnectionSetting();
        }

        #endregion

        #region Search User
        ///// <summary>
        ///// 搜索框匹配用户时调用此方法
        ///// </summary>
        ///// <param name="key">搜索框内的待匹配关键字</param>
        ///// <returns></returns>
        //[HttpGet]
        //[RACodeReview("Allen Yin")]
        //public AddUserPageInfo SearchUsers(string tenantId, string key)
        //{
        //    //List<AOSUserDto> usersInfo = UserWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, 30);
        //    List<AOSUserDto> usersInfo = UserSerive.SearchUsers(TenantLocalValue.LogonGroupId, key);
        //    AddUserPageInfo info = new AddUserPageInfo
        //    {
        //        Users = usersInfo,
        //        StatusMsg = usersInfo.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), usersInfo.Count)
        //        : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
        //    };
        //    return info;
        //}

        /// <summary>
        /// 先从数据库search，不够再从Azure AD search
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        //TODO cyrus & PeoplePicker
        [HttpGet]
        public async Task<AddUserPageInfo> SearchAADUsers([FromQuery]string tenantId, [FromQuery]string key, [FromQuery]bool onlyFromRecord = false, [FromQuery]bool onlyIncludeAAdUser = false)
        {
            Logger.Info("Start to search users.");
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
            {
                return null;
            }
            int total = 20;
            var accounts = new List<AOSUserDto>();
            if (!onlyIncludeAAdUser)
            {
                var usersFromDB = await UserSerive.SearchUsersAsync(TenantLocalValue.LogonGroupId, key);
                Logger.Info($"Search user from DB. Count:{usersFromDB.Count}.");
                accounts.AddRange(usersFromDB);
            }
            if (!onlyFromRecord && total > accounts.Count)
            {
                var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
                var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
                var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);
                var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, 20, onlyIncludeAAdUser);
                //var excludeAccounts = accountsFromAD.Where(a => existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id))
                //    .ToList();
                //var includeAccounts = accountsFromAD.Except(excludeAccounts).ToList();
                var includeAccounts = accountsFromAD.Where(a => !(existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id) || existUserIds.Contains(a.Id)))
                    .ToList();
                if (includeAccounts.Count > 0)
                {
                    var offset = includeAccounts.Count > (total - accounts.Count) ? total - accounts.Count : includeAccounts.Count;
                    var actualAccounts = includeAccounts.GetRange(0, offset);
                    //var usersInfo = UserSerive.Convert2AOSUserDtos(actualAccounts);
                    var usersInfo = actualAccounts.Select(o => AADAccount.Convert2AOSUserDto(o)).ToList();
                    accounts.AddRange(usersInfo);
                }
            }

            var finalAccounts = new List<AOSUserDto>();
            //UserId is registered in AOS id.
            var searchInAAdUserPrincipalNames = accounts.Where(a => string.IsNullOrEmpty(a.UserId)).Select(a => a.UserPrincipalName).ToList();
            var searchInAAdAccounts = (await UserSerive.SearchUsersAsync(searchInAAdUserPrincipalNames)).ToDictionary(k => k.UserPrincipalName, v => v);
            if (searchInAAdAccounts.Keys.Count > 0)
            {
                foreach (var account in accounts)
                {
                    if (!string.IsNullOrEmpty(account.UserId))
                    {
                        finalAccounts.Add(account);
                    }
                    else
                    {
                        if (searchInAAdAccounts.ContainsKey(account.UserPrincipalName))
                        {
                            finalAccounts.Add(searchInAAdAccounts[account.UserPrincipalName]);
                        }
                    }
                }
            }
            else
            {
                finalAccounts.AddRange(accounts);
            }
            Logger.Info($"The final accounts of the search:{finalAccounts.Count}.");
            
            AddUserPageInfo info = new AddUserPageInfo
            {
                Users = finalAccounts,
                StatusMsg = finalAccounts.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), finalAccounts.Count)
                : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
            };
            Logger.Info("End to search users.");
            return info;
        }
        [RMApiAuthorize(RMPermissionMasks.FSAdmin)]
        [HttpGet]
        public async Task<AddUserPageInfo> SearchAADUsers4FSConnection([FromQuery] string tenantId, [FromQuery] string key)
        {
            Logger.Info("Start to search users.");
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
            {
                return null;
            }
            int total = 20;
            var accounts = new List<AOSUserDto>();

            var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
            var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
            var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);
            var accountsFromAD = AccountWrapperService.SearchAccounts4FSConnection(TenantLocalValue.LogonGroupId, key, 20);

            var includeAccounts = accountsFromAD.Where(a => !(existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id) || existUserIds.Contains(a.Id)))
                .ToList();
            if (includeAccounts.Count > 0)
            {
                var offset = includeAccounts.Count > (total - accounts.Count) ? total - accounts.Count : includeAccounts.Count;
                var actualAccounts = includeAccounts.GetRange(0, offset);
                var usersInfo = actualAccounts.Select(o => AADAccount.Convert2AOSUserDto(o)).ToList();
                accounts.AddRange(usersInfo);
            }

            Logger.Info($"The final accounts of the search:{accounts.Count}.");

            AddUserPageInfo info = new AddUserPageInfo
            {
                Users = accounts,
                StatusMsg = accounts.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), accounts.Count)
                : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
            };
            Logger.Info("End to search users.");
            return info;
        }

        [HttpGet]
        public AddUserPageInfo SearchAADUsersByApp([FromQuery] string key, [FromQuery] string appProfileId)
        {
            var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, appProfileId, 20);
            var res = accountsFromAD.Select(o => AADAccount.Convert2AOSUserDto(o)).ToList();
            return new AddUserPageInfo
            {
                Users = res,
                StatusMsg = res.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), res.Count)
                : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
            };
        }

        /// <summary>
        /// 查找删除的用户
        /// </summary>
        /// <param name="lstUsers">待验证的用户列表</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> SearchUsersRemoved([FromBody] List<string> lstUsers)
        {
            if (lstUsers == null || lstUsers.Count == 0)
            {
                return new List<string>();
            }
            List<string> userRemoved = await UserSerive.SearchUsersRemovedAsync(lstUsers);
            return userRemoved;
        }

        #endregion

        #region Load app profile

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public Task<List<Cloud.Sdk.Data.AosModern.AppProfileInfo>> LoadAppProfiles()
        {
            var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
            profiles = profiles.Where(item =>
            item.Type == Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomAzureApp ||
            item.Type == Cloud.Sdk.Data.AosModern.IdentityProviderType.CustomDelegateApp).ToList();
            return Task.FromResult(profiles);
        }

        #endregion
    }
}
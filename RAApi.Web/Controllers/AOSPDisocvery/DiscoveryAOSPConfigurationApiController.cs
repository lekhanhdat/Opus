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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{
    public class AddUserPageInfo
    {
        public List<AOSUserDto> Users { get; set; }

        public string StatusMsg { get; set; }

        public bool Success { get; set; }
    }

    [Route("api/discoveryConfiguration/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPConfigurationApiController : RAWebApiBase
    {
        private static readonly IRMDiscoveryAOSPConfigurationService s_configurationService = new RMDiscoveryAOSPConfigurationService();
        private static IUserService UserSerive => PlatformWindsorManager.GetService<IUserService>();
        private static IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        [HttpPost]
        public Task<RMDiscoveryReturnMessage> AddOrUpdateConfigurationInfo([FromBody] RMDiscoveryAOSPConfigurationInfo discoveryConfigurationInfo)
        {
            return s_configurationService.AddOrUpdateAOSPConfigurationInfoAsync(discoveryConfigurationInfo);
        }

        [HttpPost]
        public Task<RMDiscoveryReturnMessage> RunDiscoveryJob([FromBody] RMDiscoveryAOSPJobParameter jobParameter)
        {
            return s_configurationService.RunDiscoveryJob(jobParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryReturnMessage> RunRescanJob([FromBody] RMDiscoveryAOSPRescanJobParameter jobParameter)
        {
            return s_configurationService.RunDiscoveryJob(jobParameter);
        }

        [HttpPost]
        public async Task<string> DeleteAOSPDatabase()
        {
            return await s_configurationService.DeleteDiscoveryDBAsync();
        }

        [HttpGet]
        public async Task<AddUserPageInfo> SearchAADUsers([FromQuery] string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            int total = 20;
            var accounts = new List<AOSUserDto>();
            var usersFromDB = await UserSerive.SearchUsersAsync(TenantLocalValue.LogonGroupId, key);
            accounts.AddRange(usersFromDB);

            if (total > accounts.Count)
            {
                var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
                var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
                var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);

                TenantLocalValue.CallerType = "PartnerPortal";
                var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, 20, false);
                var includeAccounts = accountsFromAD.Where(a => !(existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id) || existUserIds.Contains(a.Id)))
                    .ToList();

                if (includeAccounts.Count > 0)
                {
                    var offset = Math.Min(total - accounts.Count, includeAccounts.Count);
                    var actualAccounts = includeAccounts.GetRange(0, offset);
                    var usersInfo = actualAccounts.Select(AADAccount.Convert2AOSUserDto).ToList();
                    accounts.AddRange(usersInfo);
                }
            }

            var finalAccounts = new List<AOSUserDto>();
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
                    else if (searchInAAdAccounts.ContainsKey(account.UserPrincipalName))
                    {
                        finalAccounts.Add(searchInAAdAccounts[account.UserPrincipalName]);
                    }
                }
            }
            else
            {
                finalAccounts.AddRange(accounts);
            }

            return new AddUserPageInfo
            {
                Users = finalAccounts,
                StatusMsg = finalAccounts.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), finalAccounts.Count)
                    : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
            };
        }
    }
}

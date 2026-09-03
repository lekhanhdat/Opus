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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.ManualApproval
{
    [Route("api/googleone/manualapproval")]
    public class GoogleOneManualApprovalApiController : GoogleOneApiBaseController
    {
        private static IUserService UserSerive => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        private static IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();
        
        private readonly IRALogger _logger = new RALogger(typeof(GoogleOneManualApprovalApiController));

        [HttpGet("search")]
        public async Task<ManualApprovalAADUserInfo> SearchAADUsers([FromQuery] string key, [FromQuery] bool onlySearchFromDatabase = true, [FromQuery] bool onlySearchInTenant = false)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
                {
                    return null;
                }
                int total = 20;
                var accounts = new List<ManualApprovalAOPUserInfo>();
                if (!onlySearchInTenant)
                {
                    var usersFromDB = await UserSerive.ManualSearchUsersAsync(TenantLocalValue.LogonGroupId, key);
                    accounts.AddRange(usersFromDB);
                }
                if (!onlySearchFromDatabase && total > accounts.Count)
                {
                    var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
                    var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
                    var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);
                    var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, 20, onlySearchInTenant);
                    var includeAccounts = accountsFromAD.Where(a => !(existUserPrincipalNames.Contains(a.UserPrincipalName) || existAADIds.Contains(a.Id) || existUserIds.Contains(a.Id)))
                        .ToList();
                    if (includeAccounts.Count > 0)
                    {
                        var offset = includeAccounts.Count > (total - accounts.Count) ? total - accounts.Count : includeAccounts.Count;
                        var actualAccounts = includeAccounts.GetRange(0, offset);
                        //var usersInfo = UserSerive.Convert2AOSUserDtos(actualAccounts);
                        var usersInfo = actualAccounts.Select(o => AADAccount.Convert2ManualAOSUserDto(o)).ToList();
                        accounts.AddRange(usersInfo);
                    }
                }
                var finalAccounts = new List<ManualApprovalAOPUserInfo>();
                var searchInAAdUserPrincipalNames = accounts.Where(a => string.IsNullOrEmpty(a.UserId)).Select(a => a.UserPrincipalName).ToList();
                var searchInAAdAccounts = (await UserSerive.ManualSearchUsersAsync(searchInAAdUserPrincipalNames)).ToDictionary(k => k.UserPrincipalName, v => v);
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

                var info = new ManualApprovalAADUserInfo
                {
                    Users = finalAccounts,
                    StatusMsg = finalAccounts.Count > 0 ? string.Format(I18NEntity.GetString("RM_CP_AM_AddUser_UsersCount"), finalAccounts.Count)
                    : I18NEntity.GetString("RM_CP_AM_AddUser_NoUserFound")
                };
                return info;
            }
            catch
            {
                return new();
            }
        }

        [HttpPost("underreviewquery")]
        public async Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            if (queryDefinition.Filters.All(filter => filter.FilterOption != ManualApprovalFilterOptions.GControlTaskId))
            {
                _logger.Error("UnderReviewQuery: GControlTaskId filter is required for under review query.");
                return new ManualApprovalPaginateResult();
            }
            var timeZoneId = Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-TIMEZONE");
            _ = bool.TryParse(Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"), out var isDaylight);
            queryDefinition.FromGControl = true;
            return await ManualApprovalService.UnderReviewFolderViewQueryAsync(queryDefinition, timeZoneId, isDaylight);
        }

        [HttpPost("approve")]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
        public Task<ManualApprovalActionResult> Approve([FromBody] ManualApprovalActionParams approveParameters)
        {
            using var performance = new PerformanceScope("GoogleOneManualApprovalApiController.Approve");
            approveParameters.FromGControl = true;
            return ManualApprovalService.ApproveAsync(approveParameters);
        }

        [HttpPost("reject")]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
        public Task<ManualApprovalActionResult> Reject([FromBody] ManualApprovalActionParams rejectParameters)
        {
            using var performance = new PerformanceScope("GoogleOneManualApprovalApiController.Reject");
            rejectParameters.FromGControl = true;
            return ManualApprovalService.RejectAsync(rejectParameters);
        }
        [HttpGet("getsettinginfo")]
        public Task<ManualApprovalSettings> GetSettingInfo()
        {
            return ManualApprovalService.GetManualApprovalSettingsAsync();
        }
        [HttpGet("getapprovalcommentoption")]
        public Task<ManualApprovalCommentInfos> GetApprovalCommentOption()
        {
            return ManualApprovalService.GetApprovalCommentOptionAsync();
        }
        [HttpPost("runbulkactionjob")]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.RunBulkActionJob)]
        public string RunBulkActionJob(ManualApprovalJobParam param)
        {
            param.IsFromMyhub = true;
            param.QueryDefintion.FromGControl = true;
            return JsonConvert.SerializeObject(ManualApprovalService.RunBulkActionJob(param));
        }
    }
}

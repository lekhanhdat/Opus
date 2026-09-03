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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.CustomizeConnector.Api;
using AvePoint.RA.Service.Services.Explorer;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/manualApproval/[action]")]
    [ApiController]
    public class ManualApprovalController : RAWebApiBase
    {
        private static IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private static IUserService UserSerive => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountWrapperService AccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();

        private static IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private IRMKeyValueDao _RMKeyValueDao;

        private IRMKeyValueDao RMKeyValueDao => (IRMKeyValueDao)PlatformWindsorManager.GetService(ref _RMKeyValueDao);

        private static ITenantService _tenantService;

        private static ITenantService TenantService => PlatformWindsorManager.GetService(ref _tenantService);

        private static IAccountDao _AccountDao;
        private static IAccountDao AccountDao => PlatformWindsorManager.GetService(ref _AccountDao);

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.UnderReviewQuery)]
        public async Task<ManualApprovalPaginateResult> UnderReviewQuery([FromBody] ManualApprovalQueryDefinition queryDefinition)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryDefinition.PartitionKeyId,
                queryDefinition,
                MultiGeoOperationType.ManualApprovalUnderReviewQuery,
                async (request) =>
                {
                    var timeZoneId = Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-TIMEZONE");
                    _ = bool.TryParse(Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"), out var isDaylight);
                    if (ManualApprovalService.IsJpmc(queryDefinition.IsJpmc))
                    {
                        var sourceFilter = queryDefinition.Filters.FirstOrDefault(item => ManualApprovalFilterOptions.Source == item.FilterOption);
                        if (sourceFilter == null)
                        {
                            var removeFilter = queryDefinition.Filters.FirstOrDefault(item => item.FilterOption == ManualApprovalFilterOptions.MyhubFolderNodeId);
                            if (removeFilter != null)
                            {
                                queryDefinition.Filters.Remove(removeFilter);
                            }
                            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                            {
                                FilterOption = ManualApprovalFilterOptions.Source,
                                Value = JsonConvert.SerializeObject(new List<int> { (int)SourceFlag.FileSystem })
                            });
                        }
                        else
                        {
                            sourceFilter.Value = JsonConvert.SerializeObject(new List<int> { (int)SourceFlag.FileSystem });
                        }
                        return await ManualApprovalService.UnderReviewFolderViewQueryAsync(request, timeZoneId, isDaylight);
                    }

                    var enableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                    if (!enableJPMCFileSystemFeature)
                    {
                        return await ManualApprovalService.UnderReviewFolderViewQueryAsync(request, timeZoneId, isDaylight);
                    }

                    var jpmcSourceFilter = queryDefinition.Filters.FirstOrDefault(item => ManualApprovalFilterOptions.Source == item.FilterOption);
                    if (jpmcSourceFilter != null)
                    {
                        var sourceList = JsonConvert.DeserializeObject<List<int>>(jpmcSourceFilter.Value);
                        if (sourceList != null && !sourceList.Contains((int)SourceFlag.FileSystem))
                        {
                            return await ManualApprovalService.UnderReviewFolderViewQueryAsync(request, timeZoneId, isDaylight);
                        }

                        jpmcSourceFilter.Value = JsonConvert.SerializeObject(sourceList.Where(s => s != (int)SourceFlag.FileSystem));
                        return await ManualApprovalService.UnderReviewFolderViewQueryAsync(request, timeZoneId, isDaylight);
                    }

                    var sourceFlags = Enum.GetValues(typeof(SourceFlag))
                        .Cast<SourceFlag>()
                        .Where(flag => flag != SourceFlag.FileSystem)
                        .Select(flag => (int)flag)
                        .ToList();

                    queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.Source,
                        Value = JsonConvert.SerializeObject(sourceFlags)
                    });

                    return await ManualApprovalService.UnderReviewFolderViewQueryAsync(request, timeZoneId, isDaylight);
                });
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Approve)]
        public async Task<ManualApprovalActionResult> Approve([FromBody] ManualApprovalActionParams approveParameters)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(approveParameters.PartitionKeyId,
                approveParameters,
                MultiGeoOperationType.ManualApprovalApprove,
                (request) =>
                {
                    return ManualApprovalService.ApproveAsync(request, true);
                });
        }

        [HttpPost]
        [ValidManualApprovalParameterFilter(ManualApprovalActionType.Reject)]
        public async Task<ManualApprovalActionResult> Reject([FromBody] ManualApprovalActionParams rejectParameters)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(rejectParameters.PartitionKeyId,
                rejectParameters,
                MultiGeoOperationType.ManualApprovalReject,
                (request) =>
                {
                    return ManualApprovalService.RejectAsync(request, true);
                });
        }

        [HttpPost]
        public async Task<MAReturnMessage> RunFolderViewActionJob([FromBody] ManualApprovalActionParams folderViewParameters)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(folderViewParameters.PartitionKeyId,
                folderViewParameters,
                MultiGeoOperationType.ManualApprovalRunFolderViewActionJob,
                (request) =>
                {
                    return Task.FromResult(ManualApprovalService.RunFolderViewActionJob(request));
                });
        }

        [HttpGet]
        public async Task<ManualApprovalCommentInfos> GetApprovalCommentOption([FromQuery] string partitionKeyId)
        {
            if (string.IsNullOrEmpty(partitionKeyId))
            {
                return await ManualApprovalService.GetApprovalCommentOptionAsync();
            }
            return await RouteMultiGeoApiActionByConnectionIdAsync(partitionKeyId,
                MultiGeoOperationType.ManualApprovalGetApprovalCommentOption,
                ManualApprovalService.GetApprovalCommentOptionAsync);
        }

        [HttpPost]
        public async Task<MAReturnMessage> RunBulkActionJob([FromBody] ManualApprovalJobParam param)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(param.PartitionKeyId,
                param,
                MultiGeoOperationType.ManualApprovalRunBulkActionJob,
                (request) =>
                {
                    request.IsFromMyhub = true;
                    request.IsJpmc = param.IsJpmc;
                    return Task.FromResult(ManualApprovalService.RunBulkActionJob(request));
                });
        }

        [HttpGet]
        public async Task<List<ManualApprovalDefaultOptionDefinition>> GetFilterDefaultOptions()
        {
            var result = await ManualApprovalService.GetFilterDefaultOptionsAsync();
            var enableJPMCFileSystemFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if (!enableJPMCFileSystemFeature)
            {
                return result;
            }

            var sourceDefaultOption = result?.FirstOrDefault(item => item.DefaultOption == ManualApprovalDefaultOptions.Source);
            if (sourceDefaultOption == null)
            {
                return result;
            }

            var sourceValues = sourceDefaultOption.Value as List<KeyValuePair<int, string>>
                ?? (sourceDefaultOption.Value as IEnumerable<KeyValuePair<int, string>>)?.ToList()
                ?? JsonConvert.DeserializeObject<List<KeyValuePair<int, string>>>(JsonConvert.SerializeObject(sourceDefaultOption.Value));

            if (sourceValues != null)
            {
                sourceDefaultOption.Value = sourceValues.Where(item => item.Key != (int)SourceFlag.FileSystem).ToList();
            }

            return result;
        }

        [HttpPost]
        public Task<ManualApprovalWorkspacePaginateResult> QueryWorkspaces([FromBody] ManualApprovalWorkspaceQueryDefinition queryDefinition)
        {
            if (ManualApprovalService.IsJpmc(queryDefinition.IsJpmc))
            {
                queryDefinition.ContentSource = SourceFlag.FileSystem;
            }
            return ManualApprovalService.QueryWorkspacesAsync(queryDefinition);
        }


        [HttpPost]
        public async Task<ManualApprovalFilterFolderPathResult> QueryFolderPath([FromBody] ManualApprovalFolderPathQueryDefinition queryDefinition)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(queryDefinition.PartitionKeyId,
                queryDefinition,
                MultiGeoOperationType.ManualApprovalQueryFolderPath,
                (request) =>
                {
                    return ManualApprovalService.QueryFolderPathAsync(request);
                });
        }

        [HttpGet]
        public async Task<string> GetRealTimeJobStatusInfo([FromQuery] string jobId, [FromQuery] string partitionKeyId)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(partitionKeyId,
                jobId,
                MultiGeoOperationType.ManualApprovalGetRealTimeJobStatusInfo,
                (request) =>
                {
                    return Task.FromResult(JsonConvert.SerializeObject(ExplorerService.GetRealTimeJobStatusInfo(request)));
                });
        }

        [HttpPost]
        public async Task<RAReturnMessage> DoAction([FromBody] GlobalSearchActionDto actionDto)
        {
            return await RouteMultiGeoApiActionByConnectionIdAsync(actionDto.PartitionKeyId,
                actionDto,
                MultiGeoOperationType.ManualApprovalDoAction,
                async (request) =>
                {
                    if (ManualApprovalService.IsJpmc(actionDto.IsJpmc))
                    {
                        actionDto.SourceFlag = (int)SourceFlag.FileSystem;
                    }
                    RAReturnMessage message = await ExplorerService.ValidateParameterAsync(actionDto, ChangeTermPage.MyHub);
                    if (message.MessageType == RAMessageType.Successful)
                    {
                        if (actionDto.IsRealTimeAction)
                        {
                            message = ExplorerService.DoGlobalSearchRealTimeAction(actionDto);
                        }
                        else
                        {
                            message = ExplorerService.StartGlobalSearchActionJob(actionDto);
                        }
                    }
                    return message;
                });
        }

        [HttpGet]
        public Task<ManualApprovalSpecialReviewerResult> SpecialReviewerResult()
        {
            return ManualApprovalService.SpecialReviewerResult();
        }

        [HttpGet]
        public async Task<ManualApprovalAADUserInfo> SearchAADUsers([FromQuery] string tenantId, [FromQuery] string key, [FromQuery] bool onlyFromRecord = false, [FromQuery] bool onlyIncludeAAdUser = false)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key.Trim()))
                {
                    return null;
                }
                int total = 20;
                var accounts = new List<ManualApprovalAOPUserInfo>();
                if (!onlyIncludeAAdUser)
                {
                    var usersFromDB = await UserSerive.ManualSearchUsersAsync(TenantLocalValue.LogonGroupId, key);
                    accounts.AddRange(usersFromDB);
                }
                if (!onlyFromRecord && total > accounts.Count)
                {
                    var existUserPrincipalNames = accounts.Where(o => !string.IsNullOrEmpty(o.UserPrincipalName)).Select(o => o.UserPrincipalName);
                    var existAADIds = accounts.Where(o => !string.IsNullOrEmpty(o.Id)).Select(o => o.Id);
                    var existUserIds = accounts.Where(o => !string.IsNullOrEmpty(o.UserId)).Select(o => o.UserId);
                    var accountsFromAD = AccountWrapperService.SearchAccounts(TenantLocalValue.LogonGroupId, key, 20, onlyIncludeAAdUser);
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

        [HttpGet]
        public Task<ManualApprovalTaskInfos> GetTaskDueDate()
        {
            var timeZoneId = Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-TIMEZONE");
            _ = bool.TryParse(Request.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-ISDAYLIGHTSAVINGTIME"), out var isDaylight);
            return ManualApprovalService.GetManualApprovalTaskInfo(timeZoneId, isDaylight);
        }

        [HttpGet]
        public async Task<ManualApprovalSettings> GetSettingInfo([FromQuery] string partitionKeyId)
        {
            if (string.IsNullOrEmpty(partitionKeyId))
            {
                return await ManualApprovalService.GetManualApprovalSettingsAsync();
            }
            return await RouteMultiGeoApiActionByConnectionIdAsync(partitionKeyId,
                MultiGeoOperationType.ManualApprovalGetSettingInfo,
                ManualApprovalService.GetManualApprovalSettingsAsync);
        }

        [HttpGet]
        public Task<bool> IsHideReclassifyBtnInManualApproval()
        {
            return ManualApprovalService.IsHideReclassifyBtnInManualApproval();
        }

        [HttpGet]
        public bool IsNewLogicalAccount()
        {
            return TenantService.IsNewOpusTenant();
        }
    }
}

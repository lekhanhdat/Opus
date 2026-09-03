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

using System.Collections.Concurrent;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Google.Apis.Admin.Directory.directory_v1.Data;
using RAGoogle.Services;

namespace RAGoogle.Common;

public class PeoplePickerService : IPeoplePickerService
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(PeoplePickerService));
    private ConcurrentDictionary<string, (AccountDto,List<AccountDto>)> _userCache = new();
    private ConcurrentDictionary<string, List<string>> _googleUserInGroupCache = new();
    public PeoplePickerService()
    {
    }

    public async Task<AccountDto> ListMultiTenantsDirectoryObjectsAsync(string keyword)
    {
        try
        {
            var googleTenantAppProfiles = RMAosApiClient.GetAllAppProfilesGoogleTenants(TenantLocalValue.LogonGroupId);

            if (googleTenantAppProfiles == null) return default;
            var directoryTasks = new List<Task<AccountDto>>();

            foreach (var profile in googleTenantAppProfiles)
            {
                directoryTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        return await GetDirectoryByTenantId(profile, keyword);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while fetching directory objects for tenant {profile.TenantId}. Exception: {ex}");
                        return new AccountDto();
                    }
                }));
            }
            await Task.WhenAll(directoryTasks);
            var directoryObjects = directoryTasks.Select(t => t.Result ?? new AccountDto()).Where(t => t.UserId.EqualsIgnoreCase(keyword));

            return directoryObjects.FirstOrDefault();
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while fetching multi tenant directory objects. Exception: {ex}");
            //throw;
        }
        return null;
    }

    public async Task<(AccountDto, List<AccountDto>)> GetDirectoryAndUsersInGroupTypeDirectoryAsync(string keyword)
    {
        using var performance = new PerformanceScope($"PeoplePickerService.GetDirectoryAndUsersInGroupTypeDirectoryAsync-{keyword}");
        try
        {
            var googleTenantAppProfiles = RMAosApiClient.GetAllAppProfilesGoogleTenants(TenantLocalValue.LogonGroupId);

            if (googleTenantAppProfiles == null)
            {
                return (null, []);
            }
            var directoryTasks = googleTenantAppProfiles.Select(profile => Task.Run(async () =>
                {
                    try
                    {
                        return await GetDirectoryAndUsersInGroupTypeDirectoryByTenantId(profile, keyword);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while fetching directory objects for tenant {profile.TenantId}. Exception: {ex}");
                        return default;
                    }
                }))
                .ToList();

            await Task.WhenAll(directoryTasks);
            var (account, members) = directoryTasks.Select(t => t.Result).FirstOrDefault(t => t.Item1.UserId.EqualsIgnoreCase(keyword));
            
            return (account, members ?? []);
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while fetching multi tenant directory objects. Exception: {ex}");
            //throw;
        }
        return (null, []);
    }

    public async Task<List<string>> GetGroupUserIdsAsync(string keyword)
    {
        try
        {
            var googleTenantAppProfiles = RMAosApiClient.GetAllAppProfilesGoogleTenants(TenantLocalValue.LogonGroupId);

            if (googleTenantAppProfiles == null)
            {
                return [];
            }
            var directoryTasks = googleTenantAppProfiles.Select(profile => Task.Run(async () =>
                {
                    try
                    {
                        return await GetGroupUsersByTenantId(profile, keyword);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while fetching directory objects for tenant {profile.TenantId}. Exception: {ex}");
                        return [];
                    }
                }))
                .ToList();

            await Task.WhenAll(directoryTasks);
            return directoryTasks.SelectMany(t => t.Result).ToList();
            
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while fetching multi tenant directory objects. Exception: {ex}");
            //throw;
        }
        return [];
    }

    private async Task<AccountDto> GetDirectoryByTenantId(RMAosGoogleAppProfile profile, string keyword)
    {
        using (var directoryService = new GoogleDirectoryService(profile))
        {
            var usersResult = await directoryService.GetUserById(keyword);
            var groupsResult = await directoryService.GetGroupById(keyword);
            
            if (usersResult != null)
            {
                return new AccountDto()
                {
                    UserId = usersResult.Id,
                    UserPrincipalName = usersResult.PrimaryEmail,
                    DisplayName = usersResult.Name?.FullName,
                    ObjectType = RMActiveDirectoryObjectType.User,
                };
            }
            if (groupsResult != null)
            {
                return new AccountDto()
                {
                    UserId = groupsResult.Id,
                    UserPrincipalName = groupsResult.Email,
                    DisplayName = groupsResult.Name,
                    ObjectType = RMActiveDirectoryObjectType.Group,
                };
            }
            return null;
        }
    }
    
    private async Task<(AccountDto, List<AccountDto>)> GetDirectoryAndUsersInGroupTypeDirectoryByTenantId(RMAosGoogleAppProfile profile, string keyword)
    {
        if(_userCache.TryGetValue(keyword, out var cachedResult))
        {
            return cachedResult;
        }
        using var directoryService = new GoogleDirectoryService(profile);
        var usersResult = await directoryService.GetUserById(keyword);
        var groupsResult = await directoryService.GetGroupById(keyword);

        AccountDto directory = new();
        List<AccountDto> usersInGroup = [];
            
        if (usersResult != null)
        {
            directory = CreateNewAccountDto(usersResult.Id, usersResult.PrimaryEmail, usersResult.Name?.FullName,
                RMActiveDirectoryObjectType.User);
        }
        if (groupsResult != null)
        {
            directory = CreateNewAccountDto(groupsResult.Id, groupsResult.Email, groupsResult.Name,
                RMActiveDirectoryObjectType.Group);
            var members = await directoryService.GetUsersInGroupById(groupsResult.Id);
            foreach (var member in members)
            {
                var userName = await directoryService.GetUserNameById(member.Id);

                usersInGroup.Add(CreateNewAccountDto(member.Id, member.Email, userName,
                    RMActiveDirectoryObjectType.UserInGroup));
            }
        }
        _userCache.TryAdd(keyword, (directory, usersInGroup));
        return (directory, usersInGroup);
    }
    
    private async Task<List<string>> GetGroupUsersByTenantId(RMAosGoogleAppProfile profile, string keyword)
    {
        if(_googleUserInGroupCache.TryGetValue(keyword, out var cachedResult))
        {
            return cachedResult;
        }
        using var directoryService = new GoogleDirectoryService(profile);
        var members = await directoryService.GetUsersInGroupById(keyword);
        var memberIds = members.Select(member => member.Id).ToList();
        _googleUserInGroupCache.TryAdd(keyword, memberIds);
        return memberIds;
    }

    private AccountDto CreateNewAccountDto(string id, string upn, string displayName, RMActiveDirectoryObjectType type)
    {
        return new AccountDto()
        {
            UserId = id,
            UserPrincipalName = upn,
            DisplayName = displayName,
            ObjectType = type,
            AADId = id
        };
    }

    
}

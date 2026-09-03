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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Services;
using Google.Apis.Admin.Directory.directory_v1.Data;
using RAGoogle.GoogleObjDiscover.Services;
using RAGoogle.Models;

namespace RAGoogle.Services;

public class GoogleDirectoryService : BaseService, IDisposable
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(GoogleDirectoryService));
    private DirectoryApi _directoryApi;

    public GoogleDirectoryService(RMAosGoogleAppProfile app) : base(app, string.Empty, GoogleScopeType.Admin)
    {
        _directoryApi = new(app, initializer);
    }

    public async Task<GoogleUserData?> GetUserAsync(string key)
    {
        var user = await _directoryApi.GetUserByIdAsync(key);
        if (user != null)
        {
            return ConvertUser2Dto(user);
        }
        return null;

    }

    private GoogleUserData ConvertUser2Dto(User user)
    {
        string photo = GetImageAsBase64Async(user.PrimaryEmail, user.ThumbnailPhotoUrl).Result;
        if (photo.IsNotNullOrEmpty())
        {
            photo = $"data:image/png;base64,{photo}";
        }
        return new GoogleUserData()
        {
            UserId = user.Id,
            Name = user.Name?.FullName ?? user.PrimaryEmail,
            PrimaryEmail = user.PrimaryEmail,
            Archived = user.Archived,
            Suspended = user.Suspended,
            Photo = photo,
            CreateTime = user.CreationTimeDateTimeOffset.Value.DateTime
        };
    }

    public async Task<List<Domains>> GetAllDomainsAsync()
    {
        try
        {
            var domains = await _directoryApi.ListDomainsAsync();
            return domains;
        }
        catch (Exception ex)
        {
            logger.Error($"Get domains failed, Exception: {ex}.");
        }
        return null;
    }

    public async Task<Member> GetGroupFirstUserAsync(string key, string tenantId)
    {
        try
        {
            var members = await _directoryApi.GetGroupFirstUserAsync(key);
            return members.Where(x => x.Type != "external").First();
        }
        catch (Exception ex)
        {
            logger.Error($"Get group first user failed, Exception: {ex}.");
        }
        return null;
    }
    public async Task<List<Member>> GetGroupAllUsersAsync(string key)
    {
        try
        {
            var members = await _directoryApi.ListGroupMembersAsync(key);
            return members.Where(x => x.Type != "external").ToList();
        }
        catch (Exception ex)
        {
            logger.Error($"Get group first user failed, Exception: {ex}.");
        }
        return null;
    }
    private async Task<string> GetImageAsBase64Async(string email, string imageUrl)
    {
        if (imageUrl.IsNullOrEmpty())
        {
            return null;
        }
        try
        {
            var photoBytes = await _directoryApi.GetUserPhotoThumbnail(imageUrl);
            if (photoBytes != null && photoBytes.Length > 0)
            {
                return Convert.ToBase64String(photoBytes);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Get user {email} photo failed, Exception: {ex}.");
        }
        return null;
    }
    public async Task<User> GetUserById(string userId)
    {
        try
        {
            var user = await _directoryApi.GetUserByIdAsync(userId,"*");
            return user;
        }
        catch (Exception ex)
        {
            logger.Error($"Get user by id {userId} failed, Exception: {ex}.");
        }
        return null;
    }
    
    public async Task<string> GetUserNameById(string userId)
    {
        try
        {
            var user = await _directoryApi.GetUserByIdAsync(userId,"name");
            return user.Name?.FullName;
        }
        catch (Exception ex)
        {
            logger.Error($"Get user by id {userId} failed, Exception: {ex}.");
        }
        return null;
    }
    public async Task<Group> GetGroupById(string groupId)
    {
        try
        {
            var group = await _directoryApi.GetGroupByIdAsync(groupId, "*");
            return group;
        }
        catch (Exception ex)
        {
            logger.Error($"Get user by id {groupId} failed, Exception: {ex}.");
        }
        return null;
    }
    
    public async Task<IEnumerable<Member>> GetUsersInGroupById(string groupId)
    {
        try
        {
            var members = await _directoryApi.GetUsersInGroupByGroupIdAsync(groupId, "*");
            return members.MembersValue;
        }
        catch (Exception ex)
        {
            logger.Error($"Get users by id {groupId} failed, Exception: {ex}.");
        }
        return [];
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _directoryApi?.Dispose();
        _directoryApi = null;
    }

    ~GoogleDirectoryService()
    {
        Dispose(false);
    }
}

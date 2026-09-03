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
using AvePoint.RA.Contract.Services;
using Google;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Services;
using System.Net;
using Util;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.GoogleObjDiscover.Impl;

public class RMGoogleFullDiscover : RMGoogleDiscoverBase
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleFullDiscover));

    public RMGoogleFullDiscover(DataQueue<GoogleItemData> itemQueue) : base(itemQueue)
    {
    }

    public async Task DiscoverAsync(GoogleDriveData gDrive, CancellationToken token = default)
    {
        using (GoogleDriveService service = new GoogleDriveService(appInfo))
        using (directoryService = new(appInfo))
        {
            if (gDrive.Shared)
            {
                await InitDomainsAsync();
                List<Permission> members = await service.GetPermissionsByIdAsync(gDrive.Id, true);
                string firstMemberEmail = await GetFirstDelegateMemberAsync(members);
                members.ForEach(member => PermissionIdWithUserEmail.TryAdd(member.Id, member.EmailAddress));
                using (GoogleDriveService sharedDriveService = new GoogleDriveService(appInfo, firstMemberEmail))
                {
                    sharedDriveService.SetIncludeLabels(AllLabelsId);
                    await DiscoverySharedDriveAsync(gDrive, sharedDriveService, firstMemberEmail, token);
                }
            }
            else
            {
                using (GoogleDriveService myDriveService = new GoogleDriveService(appInfo, gDrive.DriveName))
                {
                    myDriveService.SetIncludeLabels(AllLabelsId);
                    RootFolder = await myDriveService.GetFileByIdAsync("root");
                    await DiscoveryMyDriveAsync(gDrive, myDriveService, token);
                }
            }
        }
    }


    #region My Drive
    private async Task DiscoveryMyDriveAsync(GoogleDriveData gDrive, GoogleDriveService service, CancellationToken token = default)
    {
        logger.Info($"Begin to discover my drive {gDrive.Id}.");
        GoogleUserData user = null;
        using (GoogleDirectoryService directoryService = new(appInfo))
        {
            user = await directoryService.GetUserAsync(gDrive.DriveName);
        }

        user = user ?? throw new GoogleApiException("User not found") { HttpStatusCode = HttpStatusCode.NotFound };
        gDrive.Name = user.Name ?? gDrive.Name;
        await DiscoveryMyDriveFolderAsync(gDrive, "root", gDrive.DriveName, service, token, "root");
        logger.Info($"The my drive {gDrive.Id} discovery finish.");
    }

    #endregion

    #region Shared Drive
    private async Task DiscoverySharedDriveAsync(GoogleDriveData gDrive, GoogleDriveService service, string memberEmail, CancellationToken token = default)
    {
        logger.Info($"Begin to discover shared drive {gDrive.Id}.");
        await DiscoverySharedDriveFolderAsync(gDrive, gDrive.Id, memberEmail, $"{gDrive.Name}", service, token, gDrive.Id);
        logger.Info($"The shared drive {gDrive.Id} discovery finish.");
    }



    #endregion

    #region Folder

    public async Task<List<GoogleItemData>> DiscoveryFolderAsync(GoogleItemData parentFolder, GoogleDriveData gDrive, CancellationToken token = default)
    {
        logger.Info($"Begin to discover folder {parentFolder.Id}.");
        List<GoogleItemData> gItems = [];
        using (GoogleDriveService service = new(appInfo))
        {
            service.SetIncludeLabels(AllLabelsId);
            await DiscoveryFolderSubItemAsync(parentFolder, gItems, parentFolder.RelativePath, service, gDrive, token);
        }
        logger.Info($"The folder {parentFolder.Id} discovery finish.");
        return gItems;
    }

    private async Task DiscoveryFolderSubItemAsync(GoogleItemData parentFolder, List<GoogleItemData> gItems, string parentPath, GoogleDriveService service, GoogleDriveData gDrive, CancellationToken token)
    {
        logger.Info($"Discovery {parentFolder.Id} subitems");
        List<GoogleItemData> folders = [];
        string? nextToken = null;
        do
        {
            (List<File> files, nextToken) = await service.PageFilesByFolderIdAsync(parentFolder.Id, nextToken);
            foreach (File item in files)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var gItem = item.ConvertToDto(gDrive, parentPath);
                if (IncludeLabel)
                {
                    gItem.LableIds = item.LabelInfo?.Labels?.Select(x => x.Id).ToList() ?? new(); //await service.GetLabelsIdOnFileAsync(item.Id);
                }
                await ItemQueue.WriteAsync(gItem);
                if (item.IsFolder())
                {
                    folders.Add(gItem);
                }
            }
        } while (nextToken.IsNotNullOrEmpty());
        foreach (GoogleItemData item in folders)
        {
            await DiscoveryFolderSubItemAsync(item, gItems, $"{parentPath}/{item.Name}", service, gDrive, token);
        }
    }

    #endregion


}

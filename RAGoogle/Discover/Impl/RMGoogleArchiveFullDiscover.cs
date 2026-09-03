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
using RAGoogle.Models;
using RAGoogle.Services;
using System.Net;
using Util;

namespace RAGoogle.GoogleObjDiscover.Impl;

public class RMGoogleArchiveFullDiscover : RMGoogleDiscoverBase
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleFullDiscover));
    private GoogleDriveService _driveService { get; set; }
    private GoogleDriveData _driveData { get; set; }
    private CancellationToken _cancelToken { get; set; }
    private string _delegateUser { get; set; }
    private string _myDriveRootName => "root";
    public RMGoogleArchiveFullDiscover(DataQueue<GoogleItemData> itemQueue, GoogleDriveData gDrive, CancellationToken token = default) : base(itemQueue)
    {
        _driveData = gDrive;
    }

    public async Task InitDiscoverAsync()
    {
        using (GoogleDriveService service = new GoogleDriveService(appInfo))
        using (directoryService = new(appInfo))
        {
            if (_driveData.Shared)
            {
                await InitDomainsAsync();
                List<Permission> members = await service.GetPermissionsByIdAsync(_driveData.Id, true);
                members.ForEach(member => PermissionIdWithUserEmail.TryAdd(member.Id, member.EmailAddress));
                _delegateUser = await GetFirstDelegateMemberAsync(members);
                _driveService = new GoogleDriveService(appInfo, _delegateUser);
            }
            else
            {
                _driveService = new GoogleDriveService(appInfo, _driveData.DriveName);
                RootFolder = await _driveService.GetFileByIdAsync("root");
                var user = await directoryService.GetUserAsync(_driveData.DriveName);
                if (user == null)
                {
                    throw new GoogleApiException("User not found") { HttpStatusCode = HttpStatusCode.NotFound };
                }
            }
            _driveService.SetIncludeLabels(AllLabelsId);
        }
    }


    #region Drive Root
    public async Task QueryFilesInDriveRootAsync(DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Begin to discover drive root {_driveData.Id}.");
        try
        {
            if (!_driveData.Shared)
            {
                await QuerySubFilesAsync(_myDriveRootName, _driveData.DriveName, _myDriveRootName, itemQueue);
            }
            else
            {
                await QuerySubFilesAsync(_driveData.Id, _driveData.Name, _driveData.Id, itemQueue);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Faield to query files, exception:{ex}");
        }
        finally
        {
            itemQueue.Complete();
        }

        logger.Info($"The drive {_driveData.Id} discovery finish.");
    }
    public async Task QueryFolderInDriveRootAsync(DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Begin to discover drive root {_driveData.Id}.");

        if (!_driveData.Shared)
        {
            await QuerySubFoldersAsync(_myDriveRootName, _driveData.DriveName, _myDriveRootName, itemQueue);

        }
        else
        {
            await QuerySubFoldersAsync(_driveData.Id, _driveData.Name, _driveData.Id, itemQueue);
        }

    }
    public async Task QuerySubFilesAsync(string folderId, string parentPath, string parentIds, DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Begin to discover sub files in folder {folderId}.");
        try
        {
            if (!_driveData.Shared)
            {
                await DiscoveryMyDriveFilesAsync(_driveData, folderId, parentPath, _driveService, _cancelToken, parentIds, QueryType.File, itemQueue);

            }
            else
            {
                await DiscoverySharedDriveFilesAsync(_driveData, folderId, _delegateUser, parentPath, _driveService, _cancelToken, parentIds, QueryType.File, itemQueue);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Faield to query files, exception:{ex}");
        }
        finally
        {
            itemQueue.Complete();
        }
        logger.Info($"The drive {folderId} discovery finish.");
    }
    public async Task QuerySubFoldersAsync(string folderId, string parentPath, string parentIds, DataQueue<GoogleItemData> itemQueue)
    {
        logger.Info($"Begin to discover sub folder in folder {folderId}.");
        try
        {
            if (!_driveData.Shared)
            {
                await DiscoveryMyDriveFilesAsync(_driveData, folderId, parentPath, _driveService, _cancelToken, parentIds, QueryType.Folder, itemQueue);

            }
            else
            {
                await DiscoverySharedDriveFilesAsync(_driveData, folderId, _delegateUser, parentPath, _driveService, _cancelToken, parentIds, QueryType.Folder, itemQueue);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Faield to query files, exception:{ex}");
        }
        finally
        {
            itemQueue.Complete();
        }
        logger.Info($"The drive {folderId} discovery finish.");
    }
    #endregion



}

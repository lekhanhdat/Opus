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
using AvePoint.RA.DB.Model;
using Google.Apis.Admin.Reports.reports_v1.Data;
using Google.Apis.Drive.v3.Data;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Models.Enums;
using RAGoogle.Services;
using RAGoogle.Util;
using Util;
using File = Google.Apis.Drive.v3.Data.File;

namespace RAGoogle.Discover.Impl;

public class RMGoogleIncrDiscover : RMGoogleDiscoverBase
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleIncrDiscover));
    private List<string> FailedItemIds { get; set; } = new();
    public RMGoogleIncrDiscover(DataQueue<GoogleItemData> itemQueue) : base(itemQueue)
    {
    }
    #region Drive
    public void SetFailedItemIds(List<SyncFailureItemEntity> failedItems)
    {
        FailedItemIds = failedItems.Select(x => x.DocId).ToList();
        logger.Info($"The failed items count:{FailedItemIds?.Count}");
    }

    public async Task DiscoverAsync(GoogleDriveData gDrive, bool isSync, CancellationToken token = default)
    {
        using (GoogleDriveService service = new GoogleDriveService(appInfo))
        using (directoryService = new(appInfo))
        {
            if (gDrive.Shared)
            {
                await InitDomainsAsync();
                List<Permission> members = await service.GetPermissionsByIdAsync(gDrive.Id, true);
                members.ForEach(member => PermissionIdWithUserEmail.TryAdd(member.Id, member.EmailAddress));
                gDrive.MemberEmail = await GetFirstDelegateMemberAsync(members);
            }
            if (isSync)
            {
                await IncrementalDiscoveryAsync(gDrive, token);
            }
            else
            {
                await IncrementalDiscoveryCreatedItemsAsync(gDrive, token);
            }
        }
    }

    public async Task IncrementalDiscoveryDeletedItemsAsync(GoogleDriveData gDrive, CancellationToken token = default)
    {
        logger.Info($"Begin to discover deleted items in drive {gDrive.Id}");
        using (GoogleActivityService service = new(appInfo))
        {
            DateTime startTime = DiscoverStartTime;
            DateTime endTime = DiscoverEndTime;
            List<Activity> activities = await service.GetDriveActivitiesAsync(gDrive.DriveName, startTime, endTime, gDrive.Id, gDrive.Shared, token);
            await GetDeletedItems(activities, gDrive);
        }
    }

    public async Task IncrementalDiscoveryCreatedItemsAsync(GoogleDriveData gDrive, CancellationToken token = default)
    {
        logger.Info($"Begin to discover new created items in drive {gDrive.Id}");
        using (GoogleActivityService service = new(appInfo))
        {
            DateTime startTime = DiscoverStartTime;
            DateTime endTime = DiscoverEndTime;

            List<Activity> activities = await service.GetDriveActivitiesAsync(gDrive.DriveName, startTime, endTime, gDrive.Id, gDrive.Shared, token);
            await GetCreateItems(activities, gDrive);
        }
    }

    public async Task IncrementalDiscoveryAsync(GoogleDriveData gDrive, CancellationToken token = default)
    {
        logger.Info($"Begin to incremental discover drive {gDrive.Id}.");
        using (GoogleActivityService service = new(appInfo))
        {
            DateTime startTime = DiscoverStartTime;
            DateTime endTime = DiscoverEndTime;
            logger.Info($"Begin to process failed items {gDrive.Id}.");
            await ProcessFailedItemsAsync(gDrive);
            logger.Info($"Begin to process activity {gDrive.Id}.");
            await ProcessActivitiesAsync(service, gDrive, startTime, endTime, token);
        }
    }
    private async Task ProcessActivitiesAsync(GoogleActivityService service, GoogleDriveData gDrive, DateTime startTime, DateTime endTime, CancellationToken token)
    {
        List<Activity> activities = await service.GetDriveActivitiesAsync(gDrive.DriveName, startTime, endTime, gDrive.Id, gDrive.Shared, token);
        List<FeedItemInfo> gItems = [];
        foreach (Activity activity in activities)
        {
            var events = activity.Events.Where(t => HandleActivityFamily.Contains(t.Name)).ToList();
            if (events.IsNullOrEmpty())
            {
                continue;
            }
            var t = events.Last();
            var eventTime = activity.Id.TimeDateTimeOffset.Value.UtcDateTime;
            string itemId = t.TryGetValueByName(GoogleConstant.Parameter_doc_id);
            if (!gDrive.Shared)
            {
                var ownerIsSharedDrive = t.TryGetBoolValueByName(GoogleConstant.Parameter_owner_is_shared_drive);
                if (ownerIsSharedDrive.HasValue && ownerIsSharedDrive.Value)
                {
                    continue;
                }
            }
            if (string.IsNullOrEmpty(itemId))
            {
                continue;
            }
            logger.Info($"Begin to process activity {itemId},time:{eventTime}.");
            if (CacheItemIds.Contains(itemId))
            {//has been processed in the failed items
                continue;
            }
            if (!gItems.Exists(x => x.ItemId == itemId) && itemId != gDrive.Id)
            {
                gItems.Add(new FeedItemInfo()
                {
                    ItemId = itemId,
                    EventTime = eventTime,
                    Activity = activity
                });
                logger.Info($"Add to activity collection {itemId},time:{eventTime}.");
            }
            else
            {
                var item = gItems.FirstOrDefault(x => x.ItemId == itemId);
                if (item != null && item.EventTime < eventTime)
                {
                    item.EventTime = eventTime;
                    item.Activity = activity;
                    logger.Info($"Update to activity {itemId},time:{eventTime}.");
                }
            }
            if (gItems.Count >= 1000)
            {
                await ProcessFeedItemsAsync(gItems, gDrive);
                gItems.Clear();
            }
        }
        if (gItems.Count >= 0)
        {
            await ProcessFeedItemsAsync(gItems, gDrive);
            gItems.Clear();
        }

    }
    private async Task ProcessFailedItemsAsync(GoogleDriveData gDrive)
    {
        if (FailedItemIds.IsNullOrEmpty())
        {
            return;
        }
        logger.Info($"Begin to process failed items.");
        using (var service = await GetDriveService(gDrive.DriveName))
            foreach (var itemId in FailedItemIds)
            {
                logger.Info($"Start to handle failed item :{itemId}.");
                var item = await service.GetFileByIdAsync(itemId);
                if (item is not null)
                {
                    if (!DoesItemNeedCollect(item))
                    {
                        continue;
                    }
                    string fullPath = await GeneratePath(item, gDrive, service);
                    string parentPath = string.IsNullOrEmpty(fullPath) ? gDrive.Name : fullPath;
                    string workspace = gDrive.Shared ? gDrive.Name : gDrive.DriveName;
                    logger.Info($"failed item :{itemId}, full path:{fullPath.Replace(workspace, gDrive.Id)}, parent path:{parentPath.Replace(workspace, gDrive.Id)}");
                    if (item.IsFolder())
                    {
                        if (gDrive.Shared)
                        {
                            await DiscoverySharedDriveFolderAsync(gDrive, item.Id, parentPath, gDrive.Name, service, CancellationToken.None, item.Id);
                        }
                        else
                        {
                            await DiscoveryMyDriveFolderAsync(gDrive, item.Id, parentPath, service, CancellationToken.None, item.Id);
                        }
                    }
                    else
                    {
                        var itemData = item.ConvertToDto(gDrive, string.Empty, parentPath).CheckModifiedByEmail(PermissionIdWithUserEmail, item);
                        GetParentId(item, itemData);
                        if (IncludeLabel)
                        {
                            List<Label> labels = item.LabelInfo?.Labels?.ToList() ?? new();//await service.GetLabelsAppliedOnFileAsync(item.Id);
                            itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                            itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                        }
                        CacheItemIds.Add(itemData.Id);
                        await ItemQueue.WriteAsync(itemData);
                    }
                }
                else
                {
                    logger.Info($"Not found failed item :{itemId}.");
                    await ItemQueue.WriteAsync(new GoogleItemData()
                    {
                        Id = itemId,
                        IsDeleted = true,
                        DriveName = gDrive.Name,
                        DriveId = gDrive.Id,
                    });
                }

            }
    }

    public async Task ProcessFeedItemsAsync(List<FeedItemInfo> gItems, GoogleDriveData gDrive)
    {
        foreach (var feed in gItems)
        {
            string actor = feed.Activity.Actor.Email;
            string itemId = feed.ItemId;

            using (GoogleDriveService service = new(appInfo, actor))
            {
                var item = await service.GetFileByIdAsync(itemId, AllLabelsId);
                var driveName = gDrive.Name;
                if (item is not null)
                {
                    if (!DoesItemNeedCollect(item))
                    {
                        continue;
                    }
                    var rootItem = item;
                    string fullPath = await GeneratePath(rootItem, gDrive, service);
                    if (gDrive.Shared && actor.IsExternalUser(Domains))
                    {
                        actor = gDrive.MemberEmail;
                    }

                    var itemData = item.ConvertToDto(gDrive, string.Empty, fullPath, memberEmail: actor).CheckModifiedByEmail(PermissionIdWithUserEmail, item);
                    GetParentId(item, itemData);
                    if (IncludeLabel)
                    {
                        List<Label> labels = item.LabelInfo?.Labels?.ToList() ?? new();//await service.GetLabelsAppliedOnFileAsync(item.Id);
                        itemData.LableIds = labels.ConvertAll(x => x.Id).ToList();
                        itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                    }
                    if (item.Trashed ?? false)
                    {
                        itemData.IsDeleted = true;
                    }
                    HandleSpecialEventType(itemData, rootItem, gDrive, feed.Activity);
                    await ItemQueue.WriteAsync(itemData);
                }
                else
                {
                    await ItemQueue.WriteAsync(new GoogleItemData()
                    {
                        Id = itemId,
                        IsDeleted = true,
                        DriveName = driveName,
                        DriveId = gDrive.Id,
                    });
                }
            }
        }
    }
    public void HandleSpecialEventType(GoogleItemData itemData, File rootItem, GoogleDriveData workspace, Activity act)
    {
        if (act.Events.Any(t => DeleteActivityFamily.Contains(t.Name)))
        {

            itemData.IsDeleted = true;
            return;
        }
        if (act.Events.Any(t => t.Name.Equals(ActivityType.change_owner.ToString())))
        {
            var newOwner = act.Events.Last().TryGetValueByName(GoogleConstant.Parameter_new_owner);
            string driveName = workspace.Shared ? workspace.Name : workspace.DriveName;

            if (!newOwner.IsNullOrEmpty() && !newOwner.Equals(driveName))
            {
                itemData.IsDeleted = true;
            }
            return;
        }
    }
    #endregion

    #region get specific activities 
    private async Task GetCreateItems(List<Activity> activities, GoogleDriveData gDrive)
    {
        List<GoogleItemData> processedItems = [];
        foreach (Activity activity in activities)
        {
            var events = activity.Events;
            await events.Where(t => CreateActivityFamily.Contains(t.Name)).ForEachAsync(async t =>
            {
                string itemId = t.TryGetValueByName(GoogleConstant.Parameter_doc_id);
                string actor = activity.Actor.Email;
                if (gDrive.Shared && actor.IsExternalUser(Domains))
                {
                    actor = gDrive.MemberEmail;
                }
                using (GoogleDriveService service = new(appInfo, actor))
                {
                    var item = await service.GetFileByIdAsync(itemId, AllLabelsId);
                    if (item is not null && !processedItems.Any(x => x.Id == item.Id))
                    {
                        string fullPath = await GeneratePath(item, gDrive, service);
                        var itemData = item.ConvertToDto(gDrive, string.Empty, fullPath, memberEmail: actor).CheckModifiedByEmail(PermissionIdWithUserEmail, item);

                        if (IncludeLabel)
                        {
                            var labels = item.LabelInfo?.Labels?.ToList() ?? new();
                            itemData.LableIds = labels.Select(x => x.Id).ToList();
                            itemData.MetaInfo = GenerateMetaInfo(itemData, labels);
                        }
                        processedItems.Add(itemData);
                        await ItemQueue.WriteAsync(itemData);
                    }
                }
            });
        }
    }

    private async Task GetDeletedItems(List<Activity> activities, GoogleDriveData gDrive)
    {
        List<GoogleItemData> processedItems = [];
        foreach (Activity activity in activities)
        {
            var events = activity.Events;
            await events.Where(t => DeleteActivityFamily.Contains(t.Name)).ForEachAsync(async t =>
            {
                string itemId = t.TryGetValueByName(GoogleConstant.Parameter_doc_id);
                await ItemQueue.WriteAsync(new GoogleItemData()
                {
                    Id = itemId,
                    IsDeleted = true,
                    DriveName = gDrive.Name,
                    DriveId = gDrive.Id,
                    TenantId = gDrive.TenantId
                });
            });
        }
    }

    private async Task<string> GeneratePath(File item, GoogleDriveData gDrive, GoogleDriveService service)
    {
        string workspace = gDrive.Shared ? gDrive.Name : gDrive.DriveName;

        string fullPath = string.Empty;
        while (item.Parents != null && item.Parents.Count > 0)
        {
            string parentId = item.Parents[0];
            var parentFile = await service.GetFileByIdAsync(parentId);
            if (parentFile.Parents.IsNotNullOrEmpty())
            {
                fullPath = parentFile.Name + "/" + fullPath;
            }
            item = parentFile;
        }
        int lastIndexSlash = fullPath.LastIndexOf('/');
        if (lastIndexSlash >= 0)
        {
            fullPath = $"{workspace}/" + fullPath.Remove(fullPath.LastIndexOf('/'), 1);
        }
        else fullPath = workspace;

        RootFolder = item;
        return fullPath;
    }
    #endregion
}

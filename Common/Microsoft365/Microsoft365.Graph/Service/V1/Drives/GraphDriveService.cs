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

using Microsoft.Graph.Drives.Item.Items.Item.CreateLink;
using System.Linq;
namespace Microsoft365.Graph.Service;
public partial class GraphDriveService
{
    internal readonly GraphServiceClient client;
    internal DownloadService DownloadService { get; set; }
    internal GraphDriveService(GraphServiceClient client, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(httpClient);
        this.client = client;
        this.DownloadService = new DownloadService(httpClient, new GraphErrorResponseHandler());
    }

    #region Get Drive(s)
    /// <summary>
    /// Get the default document library for user <paramref name="userIdOrPrincipalName"/>
    /// </summary>
    /// <param name="userIdOrPrincipalName">User id or user principal name, note email address does not work</param>
    /// <param name="maxRetries">8 times for important api, max retry 20 mins</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [GraphAPI("/users/{idOrUserPrincipalName}/drive")]
    public async Task<Drive?> GetDefaultDriveAsync(
        string userIdOrPrincipalName,
        CancellationToken cancellationToken = default)
    {
        userIdOrPrincipalName.ThrowIfNullOrEmpty();
        return await client.
            Users[userIdOrPrincipalName].
            Drive.
            GetAsync(
                requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = PARA_DRIVE_SELECT;
                    requestConfiguration.QueryParameters.Expand = PARA_DRIVE_EXPAND;
                }, cancellationToken);
    }

    [GraphAPI("/drives/{drive-id}")]
    public async Task<Drive?> GetDriveAsync(string driveId, CancellationToken cancellationToken = default)
    {
        driveId.ThrowIfNullOrEmpty();

        return await client.Drives[driveId].GetAsync(
            requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = PARA_DRIVE_SELECT;
                requestConfiguration.QueryParameters.Expand = PARA_DRIVE_EXPAND;
            },
            cancellationToken);
    }

    [GraphAPI("/sites/{siteId}/lists/{listidortitle}/drive")]
    public async Task<Drive?> GetDriveAsync(
        string siteId,
        string listId,
        CancellationToken cancellationToken = default)
    {
        siteId.ThrowIfNullOrEmpty();
        listId.ThrowIfNullOrEmpty();

        return await client
                .Sites[siteId]
                .Lists[listId]
                .Drive
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = PARA_DRIVE_SELECT;
                    requestConfiguration.QueryParameters.Expand = PARA_DRIVE_EXPAND;
                }, cancellationToken)
                .ConfigureAwait(false);
    }

    /// <summary>
    /// List all the document libraries for user <paramref name="userIdOrPrincipalName"/>
    /// </summary>
    /// <param name="userIdOrPrincipalName">User id or user principal name, note email address does not work</param>
    /// <param name="includeSystem">True to include system library, otherwise false, default value is false</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    /// stodo
    //[GraphAPI("/users/{idOrUserPrincipalName}/drives")]
    //public async Task<List<Drive>> ListDrivesAsync(
    //    string userIdOrPrincipalName,
    //    bool includeSystem = false,
    //    CancellationToken cancellationToken = default)
    //{
    //    userIdOrPrincipalName.ThrowIfNullOrEmpty();
    //    return await client.
    //        GetAllAsync<Drive, DriveCollectionResponse>(async () =>
    //        {
    //            return (await client.
    //            Users[userIdOrPrincipalName].
    //            Drives.GetAsync(requestConfiguration =>
    //            {
    //                requestConfiguration.QueryParameters.Select = includeSystem ? PARA_DRIVE_SELECT.Union(new string[] { "system" }).ToArray() : PARA_DRIVE_SELECT;
    //                requestConfiguration.QueryParameters.Expand = PARA_DRIVE_EXPAND;
    //            }))!;
    //        }, cancellationToken).
    //        ToListAsync(cancellationToken).
    //        ConfigureAwait(false);
    //}

    [GraphAPI("/sites/{siteId}/lists/{listId}/items/{itemIdOrRowId}/driveItem")]
    public async Task<DriveItem?> GetDriveItemAsync(
        string siteId,
        string listId,
        string itemIdOrRowId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(siteId);
        ArgumentNullException.ThrowIfNullOrEmpty(listId);
        ArgumentNullException.ThrowIfNullOrEmpty(itemIdOrRowId);

        return await client
                .Sites[siteId]
                .Lists[listId]
                .Items[itemIdOrRowId]
                .DriveItem
                .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT, cancellationToken)
                .ConfigureAwait(false);
    }

    [GraphAPI("/drives/{drive-id}/items/{item-id}")]
    public Task<DriveItem?> GetDriveItemAsync(
        string driveId,
        string itemId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(driveId);
        ArgumentNullException.ThrowIfNullOrEmpty(itemId);

        return client
                .Drives[driveId]
                .Items[itemId]
                .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT, cancellationToken);
    }

    [GraphAPI("/drives/{drive-id}/special/recordings")]
    public async Task<DriveItem?> GetRecordingFolderAsync(string driveId, CancellationToken cancellationToken = default)
    {
        return await client.
            Drives[driveId].
            Special["recordings"].
            GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT;
            }, cancellationToken).
            ConfigureAwait(false);
    }

    #region shared query parameters
    private static readonly string[] PARA_DRIVE_EXPAND = ["list($select=id,name,displayName,description,list,parentReference)", "root($select=id)"];
    private static readonly string[] PARA_DRIVE_SELECT = ["id", "createdDateTime", "lastModifiedDateTime", "createdBy", "webUrl", "owner", "description", "sharePointIds", "etag", "quota"];
    private static readonly string[] PARA_DRIVEITEM_DELTA_SELECT = new string[] { "id", "name", "createdDateTime", "lastModifiedDateTime", "createdBy", "lastModifiedBy", "size", "parentReference", "sharePointIds", "root", "file", "folder", "package", "specialFolder", "shared", "deleted", "webDavUrl", "etag", "ctag", "content.downloadUrl" };//publication is not support in delta

    #endregion
    #endregion

    /// <summary>
    /// use the Delta result to query permissions, could return all items, if item has unique permission, the permissions on return result include item's all permissions,
    /// or null if item is inherited permission
    /// </summary>
    /// <param name="drive"></param>
    /// <param name="driveItems"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [GraphAPI("/drives/{drive-id}/items/{item-id}/permissions")]
    public async IAsyncEnumerable<(DriveItem Item, IEnumerable<Permission>? Permissions, Exception? Error)> BatchPermissionAsync(Drive drive, IAsyncEnumerable<DriveItem> driveItems, int size = 20, Func<Task>? batchProcessor = null)
    {
        if (size > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "The bucket size cannot greater than 20.");
        }
        await foreach (var batch in driveItems.BatchAsync(size))
        {
            var bucket = batch.ToList().FindAll(item => item.IsShared() && item.Deleted is null);
            var batchRequestResult = await BatchPermissionsAsync(drive.Id!, bucket);
            foreach (var item in batch)
            {
                IEnumerable<Permission>? permissions = default;
                Exception? error = default;
                if (item.IsShared())
                {
                    if (batchRequestResult.TryGetValue(item.Id!, out var permissionInfo))
                    {
                        permissions = permissionInfo.permissions;
                        error = permissionInfo.Error;
                    }
                    else
                    {
                        error = new InvalidOperationException($"Permissions of shared item is not requested, item id: {item.Id}");
                    }
                }
                yield return (item, permissions, error);
            }
            if (batchProcessor is not null)
            {
                await batchProcessor();
            }
        }
    }

    [GraphAPI("/drives/{drive-id}/items/{item-id}")]
    public async IAsyncEnumerable<(string Id, DriveItem? Item, Exception? error)> BatchItemsAsync(string driveId, IEnumerable<string> batchItemIds, [EnumeratorCancellation] CancellationToken token = default)
    {
        List<BatchRequestStep> batchRequests = new();
        foreach (var itemId in batchItemIds)
        {
            batchRequests.Add(await client.CreateBatchStepAsync(itemId, client.Drives[driveId].Items[itemId].ToGetRequestInformation(requestConfiguration => requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT)));
        }
        await foreach (var (requestId, item, error) in client.SendBatchRequestAsync<DriveItem>(batchRequests.ToArray(), token))
        {
            yield return (requestId, item, error);
        }
    }

    /// <summary>
    /// Loads additional information for a batch of DriveItems, including ListItem and Permissions.
    /// </summary>
    /// <param name="driveId">The ID of the drive.</param>
    /// <param name="items">A list of DriveItems to load additional information for.</param>
    /// <param name="token">A cancellation token.</param>
    /// <returns>An IAsyncEnumerable of tuples containing the DriveItem and any error that occurred while loading information.</returns>
    [GraphAPI("/drives/{drive-id}/items/{item-id}/listitem")]
    [GraphAPI("/drives/{drive-id}/items/{item-id}/permissions")]
    public async IAsyncEnumerable<(DriveItem Item, Exception? Error)> LoadItemsAsync(string driveId, List<DriveItem> items, [EnumeratorCancellation] CancellationToken token = default)
    {
        var items2 = await BatchListItemsAsync();
        var permissions = await BatchPermissionsAsync(driveId, items.Where(v => v.IsShared()).ToList());

        foreach (var item in items)
        {
            if (items2.TryGetValue(item.Id!, out var item2))
            {
                item2.Result.SetIfNotNull(v => item.ListItem = v);
            }
            if (permissions.TryGetValue(item.Id!, out var permission))
            {
                permission.permissions.SetIfNotNull(v => item.Permissions = v);
            }
            yield return (item, Aggregate(item2.Error, permission.Error));
        }

        Exception? Aggregate(params Exception?[] errors)
        {
            errors = errors.Where(e => e is not null).ToArray();
            return errors.Length switch
            {
                0 => null,
                1 => errors[0],
                _ => new AggregateException(errors!),
            };
        }

        async Task<Dictionary<string, (string RequestId, ListItem? Result, ApiException? Error)>> BatchListItemsAsync()
        {
            List<BatchRequestStep> batchRequests = new();
            foreach (var item in items)
            {
                var id = item.Id!;
                batchRequests.Add(await client.CreateBatchStepAsync(
                    requestId: id,
                    requestInformation: client.Drives[driveId].Items[id].ListItem.ToGetRequestInformation(
                        config =>
                        {
                            config.QueryParameters.Select = GraphListService.PARA_LISTITEM_SELECT;
                            config.QueryParameters.Expand = GraphListService.PARA_LISTITEM_EXPAND;
                        })));
            }
            return new Dictionary<string, (string RequestId, ListItem? Result, ApiException? Error)> ();
            //stodo::return batchRequests.Count > 0 ? await client.SendBatchRequestAsync<ListItem>(batchRequests.ToArray(), token).ToDictionaryAsync(t => t.RequestId!) : [];
        }
    }

    private async Task<Dictionary<string, (List<Permission>? permissions, ApiException? Error)>> BatchPermissionsAsync(string driveId, List<DriveItem> items)
    {
        if (items.Count == 0) return [];
        List<BatchRequestStep> batchSteps = new();
        foreach (var item in items)
        {
            batchSteps.Add(await client.CreateBatchStepAsync(item.Id!, client.Drives[driveId].Items[item.Id].Permissions.ToGetRequestInformation()));
        }

        Dictionary<string, (List<Permission>? permissions, ApiException? Error)> bucketPermissionResult = new();
        await foreach (var (requestId, batchPermissions, error) in client.SendBatchRequestAsync<PermissionCollectionResponse>(batchSteps.ToArray()))
        {
            if (!bucketPermissionResult.ContainsKey(requestId))
            {
                bucketPermissionResult.Add(requestId, (batchPermissions?.Value, error));
            }
        }
        return bucketPermissionResult;
    }

    [GraphAPI("/drives/{driveId}/items/{driveItemId}/permissions")]
    public IAsyncEnumerable<Permission> GetItemPermissionsAsync(string driveId, string driveItemId, CancellationToken cancellationToken = default)
    {
        driveId.ThrowIfNullOrEmpty();
        driveItemId.ThrowIfNullOrEmpty();
        return client.GetAllAsync<Permission, PermissionCollectionResponse>(
            () => client.Drives[driveId].Items[driveItemId].Permissions.GetAsync()!, cancellationToken);
    }

    [GraphAPI("/drives/{driveId}/items/{driveItemId}/createLink", Method = "POST")]
    public Task<Permission?> CreateSharingLinkAsync(
        string driveId,
        string driveItemId,
        string linkType,
        string linkScope,
        DateTimeOffset? expirationDateTime,
        CancellationToken cancellationToken = default)
    {
        driveId.ThrowIfNullOrEmpty();
        driveItemId.ThrowIfNullOrEmpty();
        linkType.ThrowIfNullOrEmpty();
        linkScope.ThrowIfNullOrEmpty();

        return client
                .Drives[driveId]
                .Items[driveItemId]
                .CreateLink.PostAsync(new CreateLinkPostRequestBody
                {
                    Type = linkType,
                    Scope = linkScope,
                    ExpirationDateTime = expirationDateTime,
                    Password = null,
                }, null, cancellationToken);
    }

    [GraphAPI("/shares/{sharedDriveItemId}/permission/grant", Method = "POST")]
    public async Task<List<Permission>?> GrantAccessToSharingLinkAsync(
        string sharedDriveItemId,
        IEnumerable<DriveRecipient> recipients,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default)
    {
        sharedDriveItemId.ThrowIfNullOrEmpty();
        ArgumentNullException.ThrowIfNull(recipients);
        ArgumentNullException.ThrowIfNull(roles);

        return (await client
            .Shares[sharedDriveItemId]
            .Permission
            .Grant.PostAsGrantPostResponseAsync(new Microsoft.Graph.Shares.Item.Permission.Grant.GrantPostRequestBody
            {
                Roles = CorrectRoles(roles)?.ToList(),
                Recipients = recipients.ToList()
            }, null, cancellationToken))?.Value;
    }

    [GraphAPI("/drives/{driveId}/items/{driveItemId}/invite", Method = "POST")]
    public async Task<List<Permission>?> InviteUserAsync(
        string driveId,
        string driveItemId,
        IEnumerable<DriveRecipient> recipients,
        DateTimeOffset? expirationDateTime,
        IEnumerable<string> roles,
        bool sendInvitation = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(driveId);
        ArgumentNullException.ThrowIfNullOrEmpty(driveItemId);
        ArgumentNullException.ThrowIfNull(recipients);

        return (await client
            .Drives[driveId]
            .Items[driveItemId]
            .Invite.PostAsInvitePostResponseAsync(new Microsoft.Graph.Drives.Item.Items.Item.Invite.InvitePostRequestBody
            {
                Recipients = recipients.ToList(),
                RequireSignIn = true,
                Roles = CorrectRoles(roles),
                SendInvitation = sendInvitation,
                ExpirationDateTime = expirationDateTime?.ToString(),
                Password = null
            }, null, cancellationToken))?.Value;
    }

    [GraphAPI("/drives/{driveId}/items/{driveItemId}/permissions/{permissionId}", Method = "DELETE")]
    public Task DeletePermissionAsync(
        string driveId,
        string driveItemId,
        string permissionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(driveId);
        ArgumentNullException.ThrowIfNullOrEmpty(driveItemId);
        ArgumentNullException.ThrowIfNullOrEmpty(permissionId);

        return client
            .Drives[driveId]
            .Items[driveItemId]
            .Permissions[permissionId]
            .DeleteAsync(null, cancellationToken);
    }

    private static List<string>? CorrectRoles(IEnumerable<string>? roles)
    {
        if (roles is not null && roles.Contains("owner"))
        {
            var rolesList = roles.ToList();
            rolesList.Remove("owner");
            rolesList.Add("sp.full control");
            return rolesList;
        }
        return roles?.ToList();
    }
}
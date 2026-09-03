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

namespace Microsoft365.Graph.Service;

public class GraphGroupService
{
    private readonly GraphServiceClient client;

    private static readonly string[] PARA_GROUP_SELECT = new string[] { "id", "displayName", "mail" };

    internal GraphGroupService(GraphServiceClient client)
    {
        this.client = client;
    }

    /// <summary>
    /// Get group
    /// </summary>
    /// <param name="id">Group ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [GraphAPI("/groups/{id}")]
    public async Task<Group?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        id.ThrowIfNullOrEmpty();

        return await client
            .Groups[id]
            .GetAsync(request =>
            {
                request.QueryParameters.Select = PARA_GROUP_SELECT;
            }, cancellationToken);
    }

    [GraphAPI("/groups")]
    public IAsyncEnumerable<Group> ListGroupsAsync(CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<Group, GroupCollectionResponse>(() => client.Groups.GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/groups?$filter=mail eq ''")]
    public async ValueTask<Group?> FindGroupByMailAsync(string address, CancellationToken cancellationToken = default)
    {
        address.ThrowIfNullOrEmpty();
        var response = await client.Groups.GetAsync((request) =>
        {
            request.QueryParameters.Filter = $"mail eq '{QueryFormat.FormatMail(address)}'";
            request.QueryParameters.Top = 1;
        }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }

    [GraphAPI("/groups/{id}/sites/root")]
    public Task<Site?> GetGroupSiteRootAsync(string id, CancellationToken cancellationToken = default)
    {
        id.ThrowIfNullOrEmpty();
        return client.Groups[id].Sites["root"].GetAsync();
    }

    [GraphAPI("/groups/{id}/sites")]
    public IAsyncEnumerable<Site> ListGroupSitesAsync(string id, CancellationToken cancellationToken = default)
    {
        id.ThrowIfNullOrEmpty();
        return client.GetAllAsync<Site, SiteCollectionResponse>(() => client.Groups[id].Sites.GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/groups/{id}/owners")]
    public IAsyncEnumerable<User> ListOwnersAsync(string id, CancellationToken cancellationToken = default)
    {
        id.ThrowIfNullOrEmpty();
        return client.GetAllAsync<User, UserCollectionResponse>(() => client
        .Groups[id].Owners.GraphUser
        .GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/groups/{id}/members")]
    public IAsyncEnumerable<User> ListMembersAsync(string id, CancellationToken cancellationToken = default)
    {
        id.ThrowIfNullOrEmpty();
        return client.GetAllAsync<User, UserCollectionResponse>(() => client
        .Groups[id].Members.GraphUser
        .GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/groups?$top=1")]
    public async Task<Group?> GetOneGroupAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.Groups.GetAsync((request) =>
        {
            request.QueryParameters.Top = 1;
        }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }
}
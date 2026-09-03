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

public class GraphChatService
{
    private readonly GraphServiceClient client;
    internal GraphChatService(GraphServiceClient client)
    {
        this.client = client;
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/graph/api/chat-list?view=graph-rest-1.0&tabs=http#example-1-list-all-chats
    /// Permissions
    /// Chat.ReadBasic.All, Chat.Read.All, Chat.ReadWrite.All
    /// </summary>
    /// <param name="userIdOrUpn"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [GraphAPI("/users/{idOrUserPrincipalName}/chats")]
    public IAsyncEnumerable<Chat> ListAllChatsAsync(
        string userIdOrUpn,
        CancellationToken cancellationToken = default)
    {
        userIdOrUpn.ThrowIfNullOrEmpty();
        return client.GetAllAsync<Chat, ChatCollectionResponse>(
            () => client.Users[userIdOrUpn].Chats.GetAsync(null, cancellationToken),
            cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/chats?$top=1")]
    public async Task<Chat?> GetOneChatAsync(
    string userIdOrUpn,
    CancellationToken cancellationToken = default)
    {
        userIdOrUpn.ThrowIfNullOrEmpty();
        var response = await client.Users[userIdOrUpn].Chats.GetAsync((request) =>
        {
            request.QueryParameters.Top = 1;
        }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }

}

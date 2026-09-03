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

public partial class GraphUserService
{
    private readonly GraphServiceClient client;
    internal GraphUserService(GraphServiceClient client, GraphBeta.GraphServiceClient betaClient)
    {
        this.client = client;
        InitBetaClient(betaClient);
    }

    private static readonly string[] PARA_USER_SELECT = new string[] { "id", "mail", "userPrincipalName", "displayName", "userType", "preferredLanguage" };
    private static readonly string[] PARA_DIRECTORY_ROLE_SELECT = new string[] { "id", "displayName", "roleTemplateId", "description" };
    /// <summary>
    /// Get user
    /// https://learn.microsoft.com/en-us/graph/api/user-get?view=graph-rest-1.0&tabs=http
    /// Permissions
    /// Application	User.Read.All, User.ReadWrite.All, Directory.Read.All, Directory.ReadWrite.All
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [GraphAPI("/users/{idOrUserPrincipalName}")]
    public async Task<User?> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();

        return await client.
            Users[userId].
            GetAsync(request =>
            {
                request.QueryParameters.Select = PARA_USER_SELECT;
            }, cancellationToken).
            ConfigureAwait(false);
    }

    /// <summary>
    /// Get user
    /// https://learn.microsoft.com/en-us/graph/api/user-list-memberof?view=graph-rest-1.0&tabs=http
    /// Permissions
    /// Application	Directory.Read.All, Directory.ReadWrite.All
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    /// stodo
    //[GraphAPI("/users/{idOrUserPrincipalName}/membersof")]
    //public IAsyncEnumerable<DirectoryRole> GetUserRolesAsync(
    //    string userId,
    //    CancellationToken cancellationToken = default)
    //{
    //    userId.ThrowIfNullOrEmpty();
    //    return client.GetAllAsync<DirectoryObject, DirectoryObjectCollectionResponse>(
    //      () => client.Users[userId].MemberOf.GetAsync(request =>
    //      {
    //          request.QueryParameters.Select = PARA_DIRECTORY_ROLE_SELECT;
    //      }, cancellationToken),
    //      cancellationToken
    //      ).
    //      Where(t => t is DirectoryRole).
    //      Cast<DirectoryRole>();
    //}

    /// <summary>
    /// Get user license
    /// https://learn.microsoft.com/en-us/graph/api/user-list-licensedetails?view=graph-rest-1.0&tabs=http
    /// Permissions
    /// Application	User.Read.All, User.ReadWrite.All, Directory.Read.All, Directory.ReadWrite.All
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [GraphAPI("/users/{idOrUserPrincipalName}/licenseDetail")]
    public IAsyncEnumerable<LicenseDetails> GetUserLicenseAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        return client.GetAllAsync<LicenseDetails, LicenseDetailsCollectionResponse>(
            () => client.Users[userId].LicenseDetails.GetAsync(null, cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/graph/api/user-list?view=graph-rest-1.0&tabs=http
    /// Permissions
    /// User.Read.All, User.ReadWrite.All, Directory.Read.All, Directory.ReadWrite.All
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [GraphAPI("/users?$filter=mail eq '' or userPrincipalName eq ''")]
    public async ValueTask<User?> GetUserByMailOrUpnAsync(
        string mail,
        CancellationToken cancellationToken = default)
    {
        mail.ThrowIfNullOrEmpty();
        return (await client.Users.GetAsync(request =>
        {
            request.QueryParameters.Filter = $"mail eq '{QueryFormat.FormatMail(mail)}' or userPrincipalName eq '{QueryFormat.FormatMail(mail)}'";
            request.QueryParameters.Select = PARA_USER_SELECT;
            request.QueryParameters.Top = 1;
        }, cancellationToken))?.Value?.FirstOrDefault();
    }


    /// <summary>
    /// https://learn.microsoft.com/en-us/graph/api/user-list?view=graph-rest-1.0&tabs=http#request
    /// Permissions
    /// User.Read.All, User.ReadWrite.All, Directory.Read.All, Directory.ReadWrite.All
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [GraphAPI("/users")]
    public IAsyncEnumerable<User> ListAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<User, UserCollectionResponse>(() => client.Users.GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/users?$top=1")]
    public async Task<User?> GetOneUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.Users.GetAsync((request) =>
         {
             request.QueryParameters.Top = 1;
         }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }

    [GraphAPI("/$batch")]
    public async IAsyncEnumerable<BatchResponseResult> ProcessBatchAsync(Dictionary<string, Dictionary<string, string?>> requestInfo, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<BatchRequestStep> batchRequestSteps = [];
        foreach (var item in requestInfo)
        {
            var mappingId = item.Key;
            var itemInfo = item.Value;
            var mailboxId = itemInfo["mailboxId"]!;
            var folderId = itemInfo["folderId"]!;
            var itemId = itemInfo["itemId"]!;
            var itemType = itemInfo["itemType"]!;
            var extendedProps = itemInfo
                .Where(kv => kv.Key != "mailboxId" && kv.Key != "folderId" && kv.Key != "itemId" && kv.Key != "itemType")
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var request = CreateUpdateItemRequest(mailboxId, folderId, itemId, itemType, extendedProps, client);
            if (request is not null)
                batchRequestSteps.Add(await client.CreateBatchStepAsync(mappingId, request));
        }

        await foreach (var sub in client.SendBatchRequestAsync(batchRequestSteps.ToArray(), cancellationToken))
        {
            yield return sub;
        }
    }

    private RequestInformation? CreateUpdateItemRequest(string mailboxId, string folderId, string itemId, string itemType, Dictionary<string, string?> extendedProps, GraphServiceClient client)
    {
        RequestInformation? request = null;

        if (itemType.Contains("IPM.Note")) // For IPM.Note and REPORT.IPM.Note.NDR
        {
            var message = new Message
            {
                SingleValueExtendedProperties = extendedProps
                    .Select(prop => new SingleValueLegacyExtendedProperty
                    {
                        Id = prop.Key,
                        Value = prop.Value
                    }).ToList(),
            };
            request = client.Users[mailboxId].MailFolders[folderId].Messages[itemId].ToPatchRequestInformation(message);
        }

        return request;
    }
}
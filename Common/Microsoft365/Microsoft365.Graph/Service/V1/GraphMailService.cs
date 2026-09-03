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

using Microsoft.Graph.Beta.Models.ODataErrors;
using Microsoft.Graph.Users.Item.TranslateExchangeIds;

namespace Microsoft365.Graph.Service;

public partial class GraphMailService
{
    private readonly GraphServiceClient client;

    internal GraphMailService(GraphServiceClient client, GraphBeta.GraphServiceClient betaClient)
    {
        this.client = client;
        InitBetaClient(betaClient);
    }
    #region Folder

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders")]
    public IAsyncEnumerable<MailFolder> GetFoldersAsync(string userId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        return client.GetAllAsync<MailFolder, MailFolderCollectionResponse>(
            () => client.Users[userId].MailFolders.GetAsync(null, cancellationToken),
            cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}")]
    public async ValueTask<MailFolder> GetFolderAsync(string userId, string folderId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        return (await client.Users[userId].MailFolders[folderId].GetAsync(null, cancellationToken))!;
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}", Method = "DELETE")]
    public async Task DeleteFolderAsync(string userId, string folderId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        await client.Users[userId].MailFolders[folderId].DeleteAsync(null, cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}/childFolders")]
    public IAsyncEnumerable<MailFolder> GetChildFoldersAsync(string userId, string folderId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNullOrEmpty(folderId);
        return client.GetAllAsync<MailFolder, MailFolderCollectionResponse>(
           () => client.Users[userId].MailFolders[folderId].ChildFolders.GetAsync(null, cancellationToken),
           cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders")]
    public async ValueTask<MailFolder?> CreateFoldersAsync(string userId, MailFolder folder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(folder);
        return await client.Users[userId].MailFolders.PostAsync(folder, null, cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/delta")]
    public IAsyncEnumerable<MailFolder> GetMailFolderDeltaAsync(string userId, string queryLink, Action<RequestConfiguration<MailFoldersDeltaRequestBuilder.DeltaRequestBuilderGetQueryParameters>>? requestConfig = default, PagingCallback? callback = default, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        var fuc = () => client.Users[userId].MailFolders.Delta.WithOptionalUrl(queryLink).GetAsDeltaGetResponseAsync(requestConfig, cancellationToken);
        return client.GetAllAsync<MailFolder, MailFoldersDeltaGetResponse>(
            fuc,
            null,
            callback,
            cancellationToken);
    }

    #endregion

    #region Messages

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}/messages/delta")]
    public IAsyncEnumerable<Message> GetMessageDeltaAsync(string userId, string folderId, string? queryLink = null,
        Action<RequestConfiguration<MailFolderMessagesDeltaRequestBuilder.DeltaRequestBuilderGetQueryParameters>>? requestConfig = default,
        PagingCallback? callback = default,
        CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        var fuc = () => client.Users[userId].MailFolders[folderId].Messages.Delta.WithOptionalUrl(queryLink).GetAsDeltaGetResponseAsync(requestConfig, cancellationToken);
        return client.GetAllAsync<Message, MailFolderMessagesDetaGetResponse>(
            fuc,
            null,
            callback,
            cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}/attachments")]
    public Task<Microsoft.Graph.Models.AttachmentCollectionResponse?> GetMessageAttachmentsAsync(string userId, string folderId, string messageId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();
        var result = client.Users[userId].MailFolders[folderId].Messages[messageId].Attachments.GetAsync(cancellationToken: cancellationToken);
        return result;
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}")]
    public Task<Message?> GetMessageByIdAsync(string userId, string folderId, string messageId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        return client.Users[userId].MailFolders[folderId].Messages[messageId].GetAsync(cancellationToken: cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/messages/{message-id}")]
    public Task<Message?> GetMessageByIdAsync(string userId, string messageId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        return client.Users[userId].Messages[messageId].GetAsync(cancellationToken: cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/messages/{message-id}")]
    public Task<Message?> GetToRecipientsMessageByIdAsync(string userId, string messageId, CancellationToken cancellationToken = default)
    {
        return client.Users[userId].Messages[messageId]
            .GetAsync(r =>
            {
                r.QueryParameters.Select = new[] { "toRecipients" };
            }, cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}")]
    public Task<Message?> UpdateMessageAsync(string userId, string folderId, string messageId, Message message, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        return client.Users[userId].MailFolders[folderId].Messages[messageId].PatchAsync(message, cancellationToken: cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}")]
    public async Task DeleteMessageAsync(string userId, string folderId, string messageId, bool isHardDelete, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        if (isHardDelete)
            await client.Users[userId].MailFolders[folderId].Messages[messageId].PermanentDelete.PostAsync(cancellationToken: cancellationToken);
        else
            await client.Users[userId].MailFolders[folderId].Messages[messageId].DeleteAsync(cancellationToken: cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}/$value")]
    public Task<Stream?> GetMessageMimeContentAsync(string userId, string folderId, string messageId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        return client.Users[userId].MailFolders[folderId].Messages[messageId].Content.GetAsync(cancellationToken: cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}/microsoft.graph.move")]
    public Task<Message?> MoveMessageAsync(string userId, string folderId, string messageId, string destinationId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        messageId.ThrowIfNullOrEmpty();

        var body = new Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Item.Move.MovePostRequestBody
        {
            DestinationId = destinationId
        };
        return client.Users[userId].MailFolders[folderId].Messages[messageId].Move.PostAsync(body, cancellationToken: cancellationToken);
    }

    #endregion

    #region Message rule

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}/messageRules")]
    public IAsyncEnumerable<MessageRule> GetMessageRulesAsync(string userId, string folderId, CancellationToken cancellationToken = default)
    {
        userId.ThrowIfNullOrEmpty();
        folderId.ThrowIfNullOrEmpty();
        return client.GetAllAsync<MessageRule, MessageRuleCollectionResponse>(
           () => client.Users[userId].MailFolders[folderId].MessageRules.GetAsync(null, cancellationToken),
           cancellationToken);
    }

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{id}/messageRules")]
    public async ValueTask<MessageRule?> CreateMessageRuleAsync(string userId, string folderId, MessageRule messageRule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNullOrEmpty(folderId);
        ArgumentNullException.ThrowIfNull(messageRule);
        return await client.Users[userId].MailFolders[folderId].MessageRules.PostAsync(messageRule, null, cancellationToken);
    }

    /// <summary>
    /// <summary>
    /// Translates identifiers of Outlook items from one format to another.
    /// </summary>
    /// <param name="userId">The ID or user principal name of the user.</param>
    /// <param name="exchangeIds">List of Exchange item IDs to translate. Maximum of 1000 IDs are allowed.</param>
    /// <param name="source">The source format of the identifiers.</param>
    /// <param name="target">The target format the identifiers should be translated to.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A response containing the translated IDs.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of exchangeIds exceeds 1000.</exception>
    /// <remarks>
    /// This method allows conversion between different identifier formats for Outlook items.
    /// Supported format types include EntryId, EwsId, ImmutableEntryId, RestId, and RestImmutableEntryId.
    /// The API has a limit of 1000 IDs per request.
    /// Application permission is not supported based on the document at May 2025, but it actually works
    //  https://learn.microsoft.com/en-us/graph/api/user-translateexchangeids?view=graph-rest-1.0&tabs=http"
    /// </remarks>
    [GraphAPI("/users/{idoruserprincipalname}/translateexchangeids", Method = "POST")]
    public async Task<TranslateExchangeIdsPostResponse> TranslateExchangeIdsAsync(
        string userId,
        IEnumerable<string> exchangeIds,
        ExchangeIdFormat source,
        ExchangeIdFormat target,
        CancellationToken cancellationToken = default)
    {
        userId.EnsureIfNotNullOrEmpty();
        exchangeIds.EnsureIfNotNull();
        var inputIds = exchangeIds as List<string> ?? exchangeIds.ToList();
        if (inputIds.Count > 1000) throw new ArgumentException("The number of exchangeIds cannot exceed 1000.", nameof(exchangeIds));

        var request = new TranslateExchangeIdsPostRequestBody
        {
            InputIds = inputIds,
            SourceIdType = source,
            TargetIdType = target
        };
        return (await client.Users[userId].TranslateExchangeIds.PostAsTranslateExchangeIdsPostResponseAsync(request, null, cancellationToken))!;
    }



    #endregion

    [GraphAPI("/users/{idOrUserPrincipalName}/mailFolders/{mailFolder-id}/messages/{message-id}")]
    public async Task<Dictionary<string, Message>> BatchUpdateMessagesAsync(
       string userId,
       string folderId,
       Dictionary<string, Message> itemsToUpdate,
       CancellationToken cancellationToken = default)
    {
        var requestIds = itemsToUpdate.Keys.ToDictionary(id => id, id => Guid.NewGuid().ToString());
        var finalResults = new Dictionary<string, Message>();
        List<BatchRequestStep> batchRequestSteps = [];
        foreach (var item in itemsToUpdate)
        {
            var requestInfo = client.Users[userId].MailFolders[folderId].Messages[item.Key].ToPatchRequestInformation(item.Value);
            batchRequestSteps.Add(await client.CreateBatchStepAsync(requestIds[item.Key], requestInfo)); 
        }
        await foreach (var (RequestId, Result, Error) in client.SendBatchRequestV2Async<Message>(batchRequestSteps.ToArray(), cancellationToken))
        {
            if (Result is null)
            {
                var itemId = requestIds.FirstOrDefault(x => x.Value == RequestId).Key;
                var updatedItem = itemsToUpdate[itemId];
                ProcessBatchStepResponse(updatedItem, Error);
                finalResults[itemId] = updatedItem;
            }
            else
            {
                ProcessBatchStepResponse(Result, Error);
                finalResults[Result.Id!] = Result;
            }
        }
        return finalResults;
    }

    private Message ProcessBatchStepResponse(Message updatedMessage, ApiException? exception)
    {
        bool isSuccess = exception is null;
        updatedMessage.AdditionalData["Code"] = isSuccess ? HttpStatusCode.OK : exception!.ResponseStatusCode;
        updatedMessage.AdditionalData["Result"] = isSuccess ? ExchangeGraphServiceResult.Success : ExchangeGraphServiceResult.Error;
        updatedMessage.AdditionalData["ErrorCode"] = isSuccess ? HttpStatusCode.OK : exception!.ResponseStatusCode;
        updatedMessage.AdditionalData["ErrorMessage"] = isSuccess ? string.Empty : exception!.Message;
        return updatedMessage;
    }


    [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta")]
    public Task<GraphV1ItemsDelta.DeltaGetResponse?> DeltaItemsAsync(string mailboxId, string folderId, string? deltaLink, int pageSize, bool useImmutableId, CancellationToken cancellationToken = default)
    {
        mailboxId.EnsureIfNotNullOrEmpty();
        folderId.EnsureIfNotNullOrEmpty();

        var r = client.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.Delta
              .WithOptionalUrl(deltaLink)
              .GetAsDeltaGetResponseAsync(config => SetHeader(config.Headers, pageSize, useImmutableId), cancellationToken);
        return r;
        static void SetHeader(RequestHeaders headers, int pageSize, bool useImmutableId)
        {
            if (useImmutableId)
                headers.Add("Prefer", $"odata.maxpagesize={pageSize}, IdType=\"ImmutableId\"");
            else
                headers.Add("Prefer", $"odata.maxpagesize={pageSize}");
            //https://learn.microsoft.com/en-us/graph/outlook-immutable-id
        }
    }
    [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta")]
    public IAsyncEnumerable<MailboxItem> DeltaItemsAsync(string mailboxId, string folderId, string? deltaLink, PagingCallback? callback = default, CancellationToken cancellationToken = default)
    {
        mailboxId.EnsureIfNotNullOrEmpty();
        folderId.EnsureIfNotNullOrEmpty();
        return client.GetAllAsync<MailboxItem, GraphV1ItemsDelta.DeltaGetResponse>(
                        initialCollection: () => client.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.Delta
                                                .WithOptionalUrl(deltaLink)
                                                .GetAsDeltaGetResponseAsync(config => SetHeader(config.Headers), cancellationToken),
                        requestConfigurator: request => { SetHeader(request.Headers); return request; },
                        callback: callback,
                        token: cancellationToken);
        static void SetHeader(RequestHeaders headers)
        {
            headers.Add("Prefer", $"odata.maxpagesize={PER_PAGE}, IdType=\"ImmutableId\"");
            //https://learn.microsoft.com/en-us/graph/outlook-immutable-id
        }
    }
}

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
#pragma warning disable CS0618 // Type or member is obsolete

namespace Microsoft365.Graph.Util;

/// <summary>
/// Handles uploading mailbox items to Exchange Online using Microsoft Graph API
/// Based on: https://learn.microsoft.com/en-us/graph/import-exchange-mailbox-item
/// and https://outlook.office365.com/api/gbeta/$metadata
/// </summary>
public class MailItemUploader
{
    private readonly GraphBeta.GraphServiceClient betaClient;
    private readonly string mailboxId;
    private GraphBetaModels.MailboxItemImportSession? session;

    public MailItemUploader(Service.GraphMailService service, string mailboxId)
        : this(service.ExportImport.betaClient, mailboxId)
    {
    }

    internal MailItemUploader(GraphBeta.GraphServiceClient graphClient, string mailboxId)
    {
        this.betaClient = graphClient.EnsureIfNotNull();
        this.mailboxId = mailboxId.EnsureIfNotNullOrEmpty();
    }

    /// <summary>
    /// Imports a new item into the specified mailbox folder
    /// </summary>
    /// <param name="folderId">The ID of the folder to import the item into</param>
    /// <param name="data">The binary data of the item to import</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The response from the import operation</returns>
    public async Task<ImportItemResponse> ImportItemAsync(string folderId, Stream data, CancellationToken cancellationToken = default)
    {
        var body = new ImportItemPostRequestBody
        {
            DataStream = data,
            Mode = MailboxItemImportMode.Create,
            FolderId = folderId
        };
        return await ImportItemAsync(body, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates an existing item in the specified mailbox folder
    /// </summary>
    /// <param name="folderId">The ID of the folder containing the item to update</param>
    /// <param name="itemId">The ID of the item to update</param>
    /// <param name="changeKey">The change key of the item to update</param>
    /// <param name="data">The binary data of the updated item</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The response from the update operation</returns>
    public async Task<ImportItemResponse> UpdateItemAsync(string folderId, string itemId, string changeKey, Stream data, CancellationToken cancellationToken = default)
    {
        var body = new ImportItemPostRequestBody
        {
            DataStream = data,
            ItemId = itemId,
            ChangeKey = changeKey,
            Mode = MailboxItemImportMode.Update,
            FolderId = folderId
        };
        return await ImportItemAsync(body, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates an import session for a specific mailbox
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A MailboxItemImportSession with the import URL and expiration date/time</returns>
    private async Task<GraphBetaModels.MailboxItemImportSession> CreateImportSessionAsync(CancellationToken cancellationToken = default)
    {
        //No not lock for session creation, as it is not a critical operation
        if (session == null || (session.ExpirationDateTime ?? DateTimeOffset.MinValue) <= DateTimeOffset.UtcNow.AddMinutes(10))
        {
            // Use the Beta SDK to create the import session
            session = await betaClient.Admin.Exchange.Mailboxes[mailboxId].CreateImportSession.PostAsync(null, cancellationToken)
                ?? throw new InvalidOperationException("Failed to create import session, received null response");
        }
        return session;
    }

    /// <summary>session.ExpirationDateTime
    /// Performs the actual import operation using the specified import URL and request body
    /// </summary>
    /// <param name="body">The request body containing the item data and import parameters</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The response from the import operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="importUrl"/> is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the import operation returns a null response</exception>
    internal async Task<ImportItemResponse> ImportItemAsync(
        ImportItemPostRequestBody body,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(body);

        var session = await CreateImportSessionAsync(cancellationToken);
        var builder = new ImportItemRequestBuilder(session.ImportUrl.EnsureIfNotNull(), betaClient.RequestAdapter);
        return await builder.PostAsImportItemResponseAsync(body, config => config.WithAnonymousAuthentication(), cancellationToken)
                ?? throw new InvalidOperationException("Failed to import item, received null response");
    }
}

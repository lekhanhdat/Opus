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

using Azure;
using log4net.Repository.Hierarchy;
using Microsoft.Graph.Beta.Models;

namespace Microsoft365.Graph.Service;

public partial class GraphMailService
{
    private GraphBeta.GraphServiceClient betaClient;
    public ExportImportService ExportImport { get; private set; }
    
    internal readonly static int PER_PAGE = GetPER_PAGE();

    private static int GetPER_PAGE()
    {
        if (int.TryParse(Environment.GetEnvironmentVariable("GRAPH_MAIL_PER_PAGE"), out int perPage))
        {
            return perPage;
        }
        return 50;
    }    
    

    [MemberNotNull(nameof(betaClient))]
    [MemberNotNull(nameof(ExportImport))]
    private void InitBetaClient(GraphBeta.GraphServiceClient betaClient)
    {
        this.betaClient = betaClient;
        ExportImport = new ExportImportService(betaClient);
    }

    [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders", IsBeta = true)]
    public async Task<GraphBetaModels.MailboxFolder?> GetRootFolderByPrimaryBoxId(string mailboxId, CancellationToken cancellationToken = default)
    {
        var result = await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders.GetAsync(null, cancellationToken)!;
        var firstItem = result?.Value?[0] ?? null;
        if (firstItem is null)
        {
            return null;
        }

        var parentFolderId = firstItem.ParentFolderId;
        var parentFolder = await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[parentFolderId].GetAsync(null, cancellationToken)!;
        return parentFolder;
    }

    [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{mailboxFolder-id}/items", IsBeta = true)]
    public Task<GraphBetaModels.MailboxItemCollectionResponse?> GetItemsByFolderId(string mailboxId, string folderId, int pageSize, int offset, string[]? select = null, string filter = "", CancellationToken cancellationToken = default)
    {
        return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.GetAsync(config =>
        {
            config.QueryParameters.Top = pageSize;
            config.QueryParameters.Skip = offset;
            config.QueryParameters.Select = select;
            if (!string.IsNullOrEmpty(filter))
            {
                config.QueryParameters.Filter = filter;
            }
        }, cancellationToken);
    }
    
    [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items", IsBeta = true)]
    public IAsyncEnumerable<GraphBetaModels.MailboxItem> GetAllItemAsync(string mailboxId, string folderId, string includedProperties = "", CancellationToken cancellationToken = default)
    {
        mailboxId.EnsureIfNotNullOrEmpty();
        folderId.EnsureIfNotNullOrEmpty();

        return betaClient.GetAllAsync<GraphBetaModels.MailboxItem, GraphBetaModels.MailboxItemCollectionResponse>(
            initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.GetAsync(
                config =>
                {
                    config.QueryParameters.Expand = [includedProperties];
                    SetPageSize(config.Headers);
                }, cancellationToken),
            requestConfigurator: request => { SetPageSize(request.Headers); return request; },
            callback: null,
            token: cancellationToken);
    }
    
    private static void SetPageSize(RequestHeaders headers)
    {
        headers.Add("Prefer", $"odata.maxpagesize={PER_PAGE}");
    }

    [GraphAPI("/admin/exchange/mailboxes/{mailbox-id}/folders/{mailboxFolder-id}/items/{mailboxItem-id}", IsBeta = true)]
    public async Task<IEnumerable<GraphBetaModels.MailboxItem>> BatchGetItemsInfo(string mailboxId, string folderId, IEnumerable<string> itemIds, string includeProperties = "", CancellationToken cancellationToken = default)
    {
        var batchRequestSteps = itemIds.Select(itemId =>
        {
            var request = betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items[itemId]
                .ToGetRequestInformation(config =>
                {
                    config.QueryParameters.Expand = [includeProperties];
                });
            return betaClient.CreateBatchStepAsync(Guid.NewGuid().ToString(), request).ExecuteAsyncTask();
        });
        var itemList = await betaClient.SendBatchRequestV2Async<GraphBetaModels.MailboxItem>(batchRequestSteps.ToArray(), cancellationToken).ToListAsync();
        return itemList.Where(i => i.Result is not null).Select(i => i.Result!);
    }

    [GraphAPI("/admin/exchange/mailboxes/{mailbox-id}/folders/{mailboxFolder-id}/items/{mailboxItem-id}", IsBeta = true)]
    public async Task<GraphBetaModels.MailboxItem?> GetItemById(string mailboxId, string folderId, string itemId, string includeProperties = "", CancellationToken cancellationToken = default)
    {
        return await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items[itemId]
                .GetAsync(config =>
                {
                    config.QueryParameters.Expand = [includeProperties];
                }, cancellationToken);

    }

    [GraphAPI("/admin/exchange/mailboxes/{mailbox-id}/folders/{mailboxFolder-id}/items/{mailboxItem-id}", IsBeta = true)]
    public async Task<BatchResponseContentCollection> LoadExtendPropertiesAsync(string mailboxId, string folderId, IEnumerable<string> itemIds, string includeProperties = "", CancellationToken cancellationToken = default)
    {
        var batchRequestContent = new BatchRequestContentCollection(betaClient);

        foreach (var itemId in itemIds)
        {
            var request = betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items[itemId]
                .ToGetRequestInformation(config =>
                {
                    config.QueryParameters.Select = ["id"];
                    config.QueryParameters.Expand = [includeProperties];
                });

            await batchRequestContent.AddBatchRequestStepAsync(request, requestId: itemId);
        }

        return await betaClient.Batch.PostAsync(batchRequestContent, cancellationToken);
    }

    public class ExportImportService(GraphBeta.GraphServiceClient betaClient)
    {
        internal readonly static int PER_PAGE = GetPER_PAGE();

        private static int GetPER_PAGE()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("GRAPH_MAIL_PER_PAGE"), out int perPage))
            {
                return perPage;
            }
            return 50;
        }

        internal readonly GraphBeta.GraphServiceClient betaClient = betaClient;
        //https://learn.microsoft.com/en-us/graph/api/resources/extended-properties-overview?view=graph-rest-1.0#id-formats
        //https://learn.microsoft.com/en-us/office/client-developer/outlook/mapi/mapi-property-tags
        //https://learn.microsoft.com/en-us/openspecs/exchange_server_protocols/ms-oxprops/f6ab1613-aefe-447d-a49c-18217230b148
        //PR_SUBJECT 0x0037
        //for email address, not sure if various mail client use different property tags
        //PR_SENDER_EMAIL_ADDRESS 0x0C1F "/O=EXCHANGELABS/OU=EXCHANGE ADMINISTRATIVE GROUP (FYDIBOHF23SPDLT)/CN=RECIPIENTS/CN=218061180FEC4DBAAF358F40D2992F78-93FD615A-0A"
        //PR_SENDER_SMTP_ADDRESS 0x5D01 "Laifu.Luo@wrxhq.onmicrosoft.com"
        //private static readonly string[]? PARA_MAILITEM_DELTA_EXPAND = ["singleValueExtendedProperties($filter=id eq 'string 0x0037' or id eq 'string 0x5D01')"];

        private static readonly string[] PARA_MAILITEM_GET_EXPAND =
        [
            $"singleValueExtendedProperties($filter=id eq {string.Join(" or id eq ", GraphCommonUtil.CommonExtendedProperties)})"
        ];

        private static readonly string[] PARA_MAILITEM_LIST_EXPAND = [$"singleValueExtendedProperties($filter=id eq {OutlookExtendedProperties.CustomPidRestoreItemId})"];

        private static readonly string[] PARA_MAILFOLDER_EXPAND = [$"singleValueExtendedProperties($filter=id eq {OutlookExtendedProperties.OtherPidFolderPathName})"];

        /// <summary>
        /// Exports items from a specified mailbox  
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox to export items from</param>
        /// <param name="itemIds">List of item IDs to be exported</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
        /// <returns>Export items response containing the exported content</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/exportitems", Method = "POST", IsBeta = true)]
        public async Task<GraphBetaExportItems.ExportItemsPostResponse> ExportItemsAsync(string mailboxId, List<string> itemIds, CancellationToken cancellationToken = default)
        {
            return (await betaClient.Admin.Exchange.Mailboxes[mailboxId].ExportItems.PostAsExportItemsPostResponseAsync(
                body: new()
                {
                    ItemIds = itemIds
                },
                cancellationToken: cancellationToken))!;
        }

        /// <summary>
        /// Exports a single mailbox item as a stream  
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox to export the item from</param>
        /// <param name="itemId">The ID of the item to be exported</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
        /// <returns>A stream containing the exported item data in decoded format. stream.Read may throw </returns>
        /// <exception cref="ODataError">Thrown when the request fails with an error. HTTP 200 with error in content</exception>
        /// <exception cref="InvalidDataException">Thrown when the response is not a valid json</exception>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/exportitems", Method = "POST", IsBeta = true)]
        public async Task<Stream> ExportItemAsStreamAsync(string mailboxId, string itemId, CancellationToken cancellationToken = default)
        {
            var stream = await betaClient.RequestAdapter.SendPrimitiveAsync<Stream>(
                    requestInfo: betaClient.Admin.Exchange.Mailboxes[mailboxId].ExportItems.ToPostRequestInformation(new() { ItemIds = [itemId] }),
                    errorMapping: new() { { "XXX", GraphBetaODataErrors.ODataError.CreateFromDiscriminatorValue } },
                    cancellationToken: cancellationToken);

            return new CryptoStream(stream: new ExportItemsStream(stream.EnsureIfNotNull()),
                                    transform: new FromBase64Transform(),
                                    mode: CryptoStreamMode.Read);
        }

        /// <summary>
        /// Imports an item into a specified folder in a mailbox, if you want to reuse the import session, pleas use <see cref="MailItemUploader"/> instead.  
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox to import the item into</param>
        /// <param name="folderId">The ID of the folder to import the item into</param>
        /// <param name="data">The stream containing the item data to be imported</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
        /// <returns>Import item response containing information about the imported item</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/createimportsession", Method = "POST", IsBeta = true)]
        public async Task<ImportItemResponse> ImportItemAsync(string mailboxId, string folderId, Stream data, CancellationToken cancellationToken = default)
        {
            return await new MailItemUploader(betaClient, mailboxId).ImportItemAsync(folderId, data, cancellationToken);
        }

        /// <summary>
        /// Updates an existing item in a specified folder in a mailbox, if you want to reuse the import session, pleas use <see cref="MailItemUploader"/> instead.  
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the item to update</param>
        /// <param name="folderId">The ID of the folder containing the item to update</param>
        /// <param name="itemId">The ID of the item to update</param>
        /// <param name="changeKey">The change key for the item to update, used for concurrency control</param>
        /// <param name="data">The stream containing the updated item data</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
        /// <returns>Import item response containing information about the updated item</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/createimportsession", Method = "POST", IsBeta = true)]
        public async Task<ImportItemResponse> UpdateItemAsync(string mailboxId,
                                                              string folderId,
                                                              string itemId,
                                                              string changeKey,
                                                              Stream data,
                                                              CancellationToken cancellationToken = default)
        {
            return await new MailItemUploader(betaClient, mailboxId).UpdateItemAsync(folderId, itemId, changeKey, data, cancellationToken);
        }


        /// <summary>
        /// Gets a specific mail folder by its ID from the specified mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the folder.</param>
        /// <param name="folderId">The ID of the folder to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The retrieved MailboxFolder object.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}", IsBeta = true)]
        public async Task<GraphBetaModels.MailboxFolder> GetFolderByIdAsync(string mailboxId, string folderId, CancellationToken cancellationToken = default)
        {
            return (await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].GetAsync(
                    requestConfiguration: config =>
                    {
                        config.QueryParameters.Expand = PARA_MAILFOLDER_EXPAND;
                    }
                    , cancellationToken: cancellationToken))!;
        }

        /// <summary>
        /// Gets a mail folder by display name from the specified mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="displayName">Display name of the folder to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The retrieved MailboxFolder object matching the display name.</returns>
        /// <exception cref="GraphBetaODataErrors.ODataError">Thrown when no folder with the specified display name is found.</exception>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders?$filter=displayName eq '{displayname}'", IsBeta = true)]
        public async Task<GraphBetaModels.MailboxFolder> GetFolderByNameAsync(string mailboxId, string displayName, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            displayName.EnsureIfNotNullOrEmpty();

            var folders = await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"displayName eq '{displayName.Replace("'", "''")}'";
                config.QueryParameters.Expand = PARA_MAILFOLDER_EXPAND;
            }, cancellationToken);
            return FirstOrThrow(folders);
        }

        private static GraphBetaModels.MailboxFolder FirstOrThrow(GraphBetaModels.MailboxFolderCollectionResponse? folders)
        {
            return folders?.Value?.FirstOrDefault() ?? throw new GraphBetaODataErrors.ODataError() { Error = new() { Code = "ErrorItemNotFound", Message = "The specified object was not found in the store." } };
        }

        /// <summary>
        /// Gets a child folder by display name from the specified parent folder in a mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="parentFolderId">The ID of the parent folder to search within.</param>
        /// <param name="displayName">Display name of the child folder to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The retrieved MailboxFolder object matching the display name within the parent folder.</returns>
        /// <exception cref="GraphBetaODataErrors.ODataError">Thrown when no child folder with the specified display name is found in the parent folder.</exception>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders?$filter=displayName eq '{displayname}'", IsBeta = true)]
        public async Task<GraphBetaModels.MailboxFolder> GetFolderByNameAsync(string mailboxId, string parentFolderId, string displayName, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            parentFolderId.EnsureIfNotNullOrEmpty();
            displayName.EnsureIfNotNullOrEmpty();

            var folders = await betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[parentFolderId].ChildFolders.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"displayName eq '{displayName.Replace("'", "''")}'";
                config.QueryParameters.Expand = PARA_MAILFOLDER_EXPAND;
            }, cancellationToken);

            return FirstOrThrow(folders);
        }



        /// <summary>
        /// Creates a new mail folder in the specified mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox where the folder will be created.</param>
        /// <param name="parentFolderId">The ID of the parent folder. If null, the folder will be created at the root.</param>
        /// <param name="folderName">The name of the new folder.</param>
        /// <param name="type">The type of the folder. e.g. IPF.Note, IPF.Appointment</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The created MailboxFolder object.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders", Method = "POST", IsBeta = true)]
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{parentid}/childfolders", Method = "POST", IsBeta = true)]
        public Task<GraphBetaModels.MailboxFolder> CreateFolderAsync(string mailboxId,
                                                     string? parentFolderId,
                                                     string folderName,
                                                     string type,
                                                     CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderName.EnsureIfNotNullOrEmpty();
            type.EnsureIfNotNullOrEmpty();

            if (string.IsNullOrEmpty(parentFolderId))
            {
                // Create at root
                return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders.ToV2(betaClient).PostAsync(
                    body: new GraphBetaModels.MailboxFolder
                    {
                        DisplayName = folderName,
                        Type = type
                    },
                    cancellationToken: cancellationToken)!;
            }
            else
            {
                // Create in subfolder
                return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[parentFolderId].ChildFolders.ToV2(betaClient).PostAsync(
                    body: new GraphBetaModels.MailboxFolder
                    {
                        DisplayName = folderName,
                        Type = type
                    },
                    cancellationToken: cancellationToken)!;
            }
        }
        /// <summary>
        /// Deletes a folder from a specified mailbox using the Microsoft Graph Beta API.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the folder to delete.</param>
        /// <param name="folderId">The ID of the folder to delete.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/$ref", Method = "DELETE", IsBeta = true)]
        public Task DeleteFolderAsync(string mailboxId, string folderId, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();
            return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].ToV2(betaClient).DeleteAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Deletes a folder from a specified mailbox using the Microsoft Graph Beta API.
        /// </summary>
        /// <param name="folder">The folder to delete.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation. The default value is None.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method deletes the specified folder using the Microsoft Graph Beta API.
        /// </remarks>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/$ref", Method = "DELETE", IsBeta = true)]
        public Task DeleteFolderAsync(GraphBetaModels.MailboxFolder folder, CancellationToken cancellationToken = default)
        {
            folder.EnsureIfNotNull();
            return DeleteFolderAsync(folder.GetMailboxId(), folder.Id!, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Gets a paged collection of mail folders that have been added, deleted, or updated in the root of a specified mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="deltaLink">A delta link from a previous call to get subsequent changes. If null or empty, a new delta query is initiated.</param>
        /// <param name="propertySet">Specifies which properties to include in the response. Default includes all properties, IdOnly includes only the ID.</param>
        /// <param name="callback">An optional callback that is invoked when the enumeration completes, providing the next link and delta link.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous enumerable of MailboxFolder objects representing the changes.</returns>
        /// <remarks>
        /// This method uses delta queries to track changes in the mailbox's root folder hierarchy. Nested folders and their changes are also returned.
        /// - Folder changes(include rename, move) are returned, only items changed in the folder are not returned.
        /// - Folder delete, will only return id and a @removed facet.
        /// </remarks>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxFolder> DeltaFoldersAsync(string mailboxId, string? deltaLink, PropertySet propertySet = default, PagingCallback? callback = default, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            return betaClient.GetAllAsync<GraphBetaModels.MailboxFolder, GraphBetaRootFolderDelta.DeltaGetResponse>(
                initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders.Delta.WithOptionalUrl(deltaLink).GetAsDeltaGetResponseAsync(
                                        config =>
                                        {
                                            if (propertySet == PropertySet.IdOnly)
                                            {
                                                config.QueryParameters.Select = ["id"];// parentMailboxUrl will always be included
                                            }
                                            SetPageSize(config.Headers);
                                        }, cancellationToken),
                requestConfigurator: request => { SetPageSize(request.Headers); return request; },
                callback: callback,
                token: cancellationToken);
        }

        /// <summary>
        /// Gets a paged collection of mail folders that have been added, deleted, or updated in a specified subfolder of a mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="folderId">The ID of the folder to get delta changes for. This is mandatory.</param>
        /// <param name="deltaLink">A delta link from a previous call to get subsequent changes. If null or empty, a new delta query is initiated.</param>
        /// <param name="propertySet">Specifies which properties to include in the response. Default includes all properties, IdOnly includes only the ID.</param>
        /// <param name="callback">An optional callback that is invoked when the enumeration completes, providing the next link and delta link.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous enumerable of MailboxFolder objects representing the changes.</returns>
        /// <remarks>
        /// This method uses delta queries to track changes in the specified subfolder's hierarchy. Nested folders and their changes are also returned.
        /// - Folder changes(include rename, move) are returned, only items changed in the folder are not returned.
        /// - Folder delete, will only return id and a @removed facet.
        /// </remarks>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxFolder> DeltaFoldersAsync(string mailboxId, string folderId, string? deltaLink, PropertySet propertySet = default, PagingCallback? callback = default, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();
            return betaClient.GetAllAsync<GraphBetaModels.MailboxFolder, GraphBetaSubFolderDelta.DeltaGetResponse>(
                initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].ChildFolders.Delta.WithOptionalUrl(deltaLink).GetAsDeltaGetResponseAsync(
                                        config =>
                                        {
                                            if (propertySet == PropertySet.IdOnly)
                                            {
                                                config.QueryParameters.Select = ["id"];// parentMailboxUrl will always be included
                                            }
                                            SetPageSize(config.Headers);
                                        }, cancellationToken),
                requestConfigurator: request => { SetPageSize(request.Headers); return request; },
                callback: callback,
                token: cancellationToken);

        }

        /// <summary>
        /// Lists all mail folders in the specified mailbox. This method retrieves the complete folder hierarchy
        /// starting from the root level of the mailbox.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox to retrieve folders from.</param>
        /// <param name="propertySet">Specifies which properties to include in the response. Default includes all properties, IdOnly includes only the ID.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An async enumerable of all mail folders in the specified mailbox.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxFolder> ListAllFoldersAsync(string mailboxId, PropertySet propertySet = default, CancellationToken cancellationToken = default)
        {
            return DeltaFoldersAsync(mailboxId: mailboxId,
                                     deltaLink: null,
                                     propertySet: propertySet,
                                     cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Lists all mail folders within the specified parent folder in a mailbox. This method retrieves
        /// all subfolders under the given folder ID, including nested folder hierarchies.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the folder.</param>
        /// <param name="folderId">The ID of the parent folder to retrieve subfolders from.</param>
        /// <param name="propertySet">Specifies which properties to include in the response. Default includes all properties, IdOnly includes only the ID.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An async enumerable of all mail folders within the specified parent folder.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxFolder> ListAllFoldersAsync(string mailboxId, string folderId, PropertySet propertySet = default, CancellationToken cancellationToken = default)
        {
            return DeltaFoldersAsync(mailboxId: mailboxId,
                                     folderId: folderId,
                                     deltaLink: null,
                                     propertySet: propertySet,
                                     cancellationToken: cancellationToken);
        }
        
        /// <summary>
        /// Lists direct child folders for the specified parent folder without traversing nested hierarchies.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the folder.</param>
        /// <param name="folderId">The ID of the parent folder whose direct children are retrieved.</param>
        /// <param name="propertySet">Specifies which properties to include in the response. Default includes all properties, IdOnly includes only the ID.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An async enumerable of direct child folders.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxFolder> ListChildFoldersAsync(string mailboxId,
            string folderId,
            PropertySet propertySet = default,
            CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();

            return betaClient.GetAllAsync<GraphBetaModels.MailboxFolder, GraphBetaModels.MailboxFolderCollectionResponse>(
                initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].ChildFolders
                .WithUrl($"https://graph.microsoft.com/beta/admin/exchange/mailboxes/{mailboxId}/folders/{folderId}/childFolders?includeHiddenFolders=true")
                .GetAsync(
                    config =>
                    {
                        if (propertySet == PropertySet.IdOnly)
                        {
                            config.QueryParameters.Select = ["id"];// parentMailboxUrl will always be included
                        }
                        SetPageSize(config.Headers);
                    }, cancellationToken),
                requestConfigurator: request => { SetPageSize(request.Headers); return request; },
                callback: null,
                token: cancellationToken);
        }

        private static void SetPageSize(RequestHeaders headers)
        {
            headers.Add("Prefer", $"odata.maxpagesize={PER_PAGE}");
        }

        /// <summary>
        /// Gets a paged collection of mail items from a specified folder.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="folderId">The ID of the folder to list items from.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous enumerable of MailboxItem objects from the specified folder.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxItem> ListItemsAsync(string mailboxId, string folderId, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();

            return betaClient.GetAllAsync<GraphBetaModels.MailboxItem, GraphBetaModels.MailboxItemCollectionResponse>(
                initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.GetAsync(config =>
                {
                    config.QueryParameters.Expand = PARA_MAILITEM_LIST_EXPAND;
                }, cancellationToken),
                token: cancellationToken);
        }


        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta", IsBeta = true)]
        public Task<GraphBetaItemsDelta.DeltaGetResponse?> DeltaItemsAsync(string mailboxId, string folderId, string? deltaLink, int pageSize, bool useImmutableId, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();

            var r = betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.Delta
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

        /// <summary>
        /// Gets a paged collection of mail items that have been added, deleted, or updated in a specified folder.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox.</param>
        /// <param name="folderId">The ID of the folder to get delta changes for. This is mandatory.</param>
        /// <param name="deltaLink">A delta link from a previous call to get subsequent changes. If null or empty, a new delta query is initiated.</param>
        /// <param name="callback">An optional callback that is invoked when the enumeration completes, providing the next link and delta link.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous enumerable of MailboxItem objects representing the changes.</returns>
        /// <remarks>
        /// This method uses delta queries to track changes in the folder's items.
        /// - Item changes (create, update, delete) are returned. Move is considered as delete and create.
        /// - Deleted items will only return id and a @removed facet.
        /// </remarks>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxItem> DeltaItemsAsync(string mailboxId, string folderId, string? deltaLink, PagingCallback? callback = default, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();
            return betaClient.GetAllAsync<GraphBetaModels.MailboxItem, GraphBetaItemsDelta.DeltaGetResponse>(
                            initialCollection: () => betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items.Delta
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

        /// <summary>
        /// Gets a paged collection of mail items that have been added, deleted, or updated in the specified folder.
        /// </summary>
        /// <param name="folder">The MailboxFolder object to get delta changes for.</param>
        /// <param name="deltaLink">A delta link from a previous call to get subsequent changes. If null or empty, a new delta query is initiated.</param>
        /// <param name="callback">An optional callback that is invoked when the enumeration completes, providing the next link and delta link.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous enumerable of MailboxItem objects representing the changes.</returns>
        /// <remarks>
        /// This method uses delta queries to track changes in the folder's items.
        /// - Item changes (create, update, delete) are returned. Move is considered as delete and create.
        /// - Deleted items will only return id and a @removed facet.
        /// </remarks>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/delta", IsBeta = true)]
        public IAsyncEnumerable<GraphBetaModels.MailboxItem> DeltaItemsAsync(GraphBetaModels.MailboxFolder folder, string? deltaLink, PagingCallback? callback = default, CancellationToken cancellationToken = default)
        {
            folder.EnsureIfNotNull();
            return DeltaItemsAsync(folder.GetMailboxId(), folder.Id!, deltaLink, callback, cancellationToken);
        }

        /// <summary>
        /// Gets a specific mail item by its ID from the specified mailbox folder.
        /// </summary>
        /// <param name="mailboxId">The ID of the mailbox containing the item.</param>
        /// <param name="folderId">The ID of the folder containing the item.</param>
        /// <param name="itemId">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The retrieved MailboxItem object with extended properties for subject and sender SMTP address.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}", IsBeta = true)]
        public Task<GraphBetaModels.MailboxItem> GetItemAsync(string mailboxId, string folderId, string itemId, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();
            folderId.EnsureIfNotNullOrEmpty();
            itemId.EnsureIfNotNullOrEmpty();

            return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items[itemId].GetAsync(
                    requestConfiguration: config =>
                    {
                        config.QueryParameters.Expand = PARA_MAILITEM_GET_EXPAND;
                    },
                    cancellationToken: cancellationToken)!;
        }

        /// <summary>
        /// Gets a specific mail item by its ID from the specified folder.
        /// </summary>
        /// <param name="folder">The MailboxFolder object containing the item.</param>
        /// <param name="itemId">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The retrieved MailboxItem object with extended properties for subject and sender SMTP address.</returns>
        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}", IsBeta = true)]
        public Task<GraphBetaModels.MailboxItem> GetItemAsync(GraphBetaModels.MailboxFolder folder, string itemId, CancellationToken cancellationToken = default)
        {
            folder.EnsureIfNotNull();
            return GetItemAsync(folder.GetMailboxId(), folder.Id!, itemId, cancellationToken);
        }

        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}", IsBeta = true)]
        public IAsyncEnumerable<(string Id, GraphBetaModels.MailboxItem? Item, ApiException? Error)> GetItemsAsync(GraphBetaModels.MailboxFolder folder, List<string> ids, CancellationToken cancellationToken = default) => GetItemsAsync(folder.GetMailboxId(), folder.Id!, ids, cancellationToken);


        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}", IsBeta = true)]
        public async IAsyncEnumerable<(string Id, GraphBetaModels.MailboxItem? Item, ApiException? Error)> GetItemsAsync(String mailboxId, String folderId, List<string> ids, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ids.EnsureIfNotNull();
            foreach (var idBlock in ids.Chunk(20))
            {
                List<BatchRequestStep> batchRequestSteps = new();

                foreach (var id in idBlock)
                {
                    var requestInfo = CreateGetItemRequestInfo(mailboxId, folderId, id);
                    batchRequestSteps.Add(await betaClient.CreateBatchStepAsync(id, requestInfo));
                }

                await foreach (var sub in betaClient.SendBatchRequestAsync<GraphBetaModels.MailboxItem>(batchRequestSteps.ToArray(), cancellationToken))
                {
                    yield return sub;
                }
            }
        }

        private RequestInformation CreateGetItemRequestInfo(String mailboxId, String folderId, String itemId)
        {
            return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[folderId].Items[itemId].ToGetRequestInformation(
               requestConfiguration: config =>
               {
                   config.QueryParameters.Expand = PARA_MAILITEM_GET_EXPAND;
               });
        }

        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/items/{itemid}", IsBeta = true)]
        public async IAsyncEnumerable<(string Id, GraphBetaModels.MailboxItem? Item, ApiException? Error)> BatchGetItemAsync(List<(string id, GraphBetaModels.MailboxFolder folder)> items, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            items.EnsureIfNotNull();
            ArgumentOutOfRangeException.ThrowIfGreaterThan(items.Count, 20);

            List<BatchRequestStep> batchRequestSteps = new();

            var requestIdToItemId = new Dictionary<string, string>(items.Count);

            foreach (var (id, folder) in items)
            {
                var requestInfo = CreateGetItemRequestInfo(folder.GetMailboxId(), folder.Id!, id);

                var requestId = Guid.NewGuid().ToString();

                requestIdToItemId[requestId] = id;

                batchRequestSteps.Add(await betaClient.CreateBatchStepAsync(requestId, requestInfo));
            }

            await foreach (var sub in betaClient.SendBatchRequestAsync<GraphBetaModels.MailboxItem>(batchRequestSteps.ToArray(), cancellationToken))
            {
                yield return (
                    requestIdToItemId[sub.RequestId],
                    sub.Result,
                    sub.Error
                    );
            }
        }

        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{folderid}/childfolders", IsBeta = true)]
        public Task<GraphBetaModels.MailboxFolderCollectionResponse> ListFoldersAsync(GraphBetaModels.MailboxFolder folder, int? top, int? skip, CancellationToken cancellationToken = default)
        {
            return betaClient.Admin.Exchange.Mailboxes[folder.GetMailboxId()].Folders[folder.Id!].ChildFolders.GetAsync(
                config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Skip = skip;
                }, cancellationToken: cancellationToken)!;
        }

        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders", IsBeta = true)]
        public Task<GraphBetaModels.MailboxFolderCollectionResponse?> ListAllFoldersByMailboxIdAsync(string mailboxId, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();

            return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders.GetAsync(cancellationToken: cancellationToken);
        }

        [GraphAPI("/admin/exchange/mailboxes/{mailboxid}/folders/{mailboxFolder-id}", IsBeta = true)]
        public Task<GraphBetaModels.MailboxFolder?> GetFolderByWellKnownName(string mailboxId, string wellKnownName, CancellationToken cancellationToken = default)
        {
            mailboxId.EnsureIfNotNullOrEmpty();

            return betaClient.Admin.Exchange.Mailboxes[mailboxId].Folders[wellKnownName].GetAsync(cancellationToken: cancellationToken);
        }
    }

    public enum PropertySet
    {
        /// <summary>
        /// Default property set
        /// </summary>
        Default = 0,

        /// <summary>
        /// ID Only
        /// </summary>
        IdOnly = 1
    }
}

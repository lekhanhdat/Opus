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
using Microsoft.Graph.Sites.Item.Lists.Item.Items.Delta;
using Microsoft.Graph.Sites.Item.Lists.Item.ContentTypes.GetCompatibleHubContentTypes;
using System.Text.RegularExpressions;

namespace Microsoft365.Graph.Service;

/// <summary>
/// Service class for interacting with Microsoft Graph Lists.
/// </summary>
public partial class GraphListService
{
    private static readonly string[] PARA_LIST_SELECT = ["id", "displayName", "name", "description", "list", "sharepointIds", "webUrl", "parentReference"];
    private static readonly string[] PARA_LIST_EXPAND = ["drive", "columns", "contentTypes"];
    internal static readonly string[] PARA_LISTITEM_SELECT = ["id", "createdBy", "createdDateTime", "lastModifiedBy", "lastModifiedDateTime", "contentType", "parentReference", "sharepointIds", "webUrl", "eTag"];
    internal static readonly string[] PARA_LISTITEM_EXPAND = ["fields"];


    internal readonly GraphServiceClient client;
    public GraphListItemService Items { get; private set; }
    public GraphColumnService Columns { get; private set; }
    public GraphContentTypeService ContentTypes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphListService"/> class.
    /// </summary>
    /// <param name="client">The GraphServiceClient instance.</param>
    internal GraphListService(GraphServiceClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.client = client;
        Columns = new GraphColumnService(client);
        ContentTypes = new GraphContentTypeService(client);
        Items = new GraphListItemService(client);
    }

    /// <summary>
    /// Gets all lists for a specified site.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of lists.</returns>
    [GraphAPI("/sites/{siteId}/lists")]
    public IAsyncEnumerable<List> GetAsync(string siteId, CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<List, ListCollectionResponse>(
            async () => (await client.Sites[siteId].Lists.GetAsync(config => config.QueryParameters.Select = PARA_LIST_SELECT, cancellationToken))!,
                     cancellationToken);
    }

    /// <summary>
    /// Gets a specific list by ID or title.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="listIdOrTitle">The ID or title of the list.</param>
    /// <param name="throwIfNotFound">Whether to throw an exception if the list is not found.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list if found, otherwise null.</returns>
    [GraphAPI("/sites/{siteId}/lists/{listIdOrTitle}")]
    public async Task<List?> GetAsync(string siteId, string listIdOrTitle, bool throwIfNotFound = true, CancellationToken cancellationToken = default)
    {
        // Note that if list title contains colon followed by digits(e.g. :0), it will throw http 404 error, try to use get lists with filter instead
        if (ColonFollowedByDigits().IsMatch(listIdOrTitle))
        {
            var list = await GetListByFilterAsync();

            if (list == null && throwIfNotFound)
            {
                //throw ODataError directly to make it same as the case of list not found
                throw new ODataError { ResponseStatusCode = 400, Error = new MainError { Code = "itemNotFound", Message = $"The specified list was not found" } };
            }
            return list;
        }
        else
        {
            return await ExceptionHandler.ItemNotFound(
                async () =>
                {
                    var list = await client.Sites[siteId].Lists[listIdOrTitle].GetAsync(
                        config =>
                        {
                            config.QueryParameters.Select = PARA_LIST_SELECT;
                            config.QueryParameters.Expand = PARA_LIST_EXPAND;
                        }, cancellationToken);
                    return list!;
                }, throwIfNotFound);
        }

        async Task<List?> GetListByFilterAsync()
        {
            var lists = await client.Sites[siteId].Lists.GetAsync(config =>
            {
                config.QueryParameters.Select = PARA_LIST_SELECT;
                config.QueryParameters.Expand = PARA_LIST_EXPAND;
                config.QueryParameters.Filter = $"displayName eq '{listIdOrTitle}'";
            }, cancellationToken);
            return lists?.Value?.FirstOrDefault();
        }
    }

    /// <summary>
    /// Updates a list.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="listId">The ID of the list.</param>
    /// <param name="list">The list object with updated values.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated list.</returns>
    [GraphAPI("/sites/{siteId}/lists/{listId}", Method = "PATCH")]
    public async Task<List> UpdateAsync(string siteId, string listId, List list, CancellationToken cancellationToken = default)
    {
        return (await client.Sites[siteId].Lists[listId].PatchAsync(list, null, cancellationToken))!;
    }

    /// <summary>
    /// Deletes a list.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="listId">The ID of the list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [GraphAPI("/sites/{siteId}/lists/{listId}", Method = "DELETE")]
    public Task DeleteAsync(string siteId, string listId, CancellationToken cancellationToken = default)
    {
        return client.Sites[siteId].Lists[listId].DeleteAsync(null, cancellationToken);
    }

    /// <summary>
    /// Creates a new list.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="list">The list object to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created list.</returns>
    [GraphAPI("/sites/{siteId}/lists", Method = "POST")]
    public async Task<List> CreateAsync(string siteId, List list, CancellationToken cancellationToken = default)
    {
        return (await client.Sites[siteId].Lists.PostAsync(
            body: ToBody(list),
            cancellationToken: cancellationToken))!;

        static List ToBody(List list)
        {
            var list2 = new List()
            {
                DisplayName = list.DisplayName.EnsureIfNotNullOrEmpty(),
                Description = list.Description,
                ListProp = new ListInfo
                {
                    Template = list.ListProp?.Template.EnsureIfNotNullOrEmpty(),
                    ContentTypesEnabled = list.ListProp?.ContentTypesEnabled ?? false
                }
            };
            if (list.Columns==null||list.Columns.IsNotNullOrEmpty())
            {
                list2.Columns = list.Columns;//TODO:
            }
            return list2;
        }
    }

    /// <summary>
    /// Gets the delta changes for a list.
    /// </summary>
    /// <param name="siteId">The ID of the site.</param>
    /// <param name="listId">The ID of the list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous enumerable of list items.</returns>
    [GraphAPI("/sites/{siteId}/lists/{listId}/items/delta")]
    public IAsyncEnumerable<ListItem> DeltaAsync(
        string siteId,
        string listId,
        string? deltaToken,
        PagingCallback? callback,
        CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<ListItem, DeltaGetResponse>(
            initialCollection: async () => (await DeltaBuilder().GetAsDeltaGetResponseAsync(RequestConfiguration, cancellationToken))!,
            requestConfigurator: null,
            callback: callback,
            token: cancellationToken);


        DeltaRequestBuilder DeltaBuilder()
        {
            var builder = client.Sites[siteId].Lists[listId].Items.Delta;
            if (string.IsNullOrEmpty(deltaToken))
            {
                return builder;
            }

            //DeltaWithToken throw http 404 due to metadata bugs
            //https://github.com/microsoftgraph/msgraph-metadata/issues/431
            var rawUri = builder.ToGetRequestInformation(RequestConfiguration).URI.AppendQueryParameter("token", deltaToken);
            return builder.WithUrl(rawUri.ToString());
        }

        static void RequestConfiguration(RequestConfiguration<DeltaRequestBuilder.DeltaRequestBuilderGetQueryParameters> config)
        {
            config.QueryParameters.Select = PARA_LISTITEM_SELECT;
            config.QueryParameters.Expand = PARA_LISTITEM_EXPAND;
        }
    }

    [GeneratedRegex(@":[0-9]+")]
    private static partial Regex ColonFollowedByDigits();

    /// <summary>
    /// Service class for interacting with Microsoft Graph Columns.
    /// </summary>
    public class GraphColumnService
    {
        internal readonly GraphServiceClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphColumnService"/> class.
        /// </summary>
        /// <param name="client">The GraphServiceClient instance.</param>
        internal GraphColumnService(GraphServiceClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        /// <summary>
        /// Creates a new column.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="column">The column definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created column definition.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/columns", Method = "POST")]
        public async Task<ColumnDefinition> CreateAsync(string siteId, string listId, ColumnDefinition column, CancellationToken cancellationToken = default)
        {
            return (await client.Sites[siteId].Lists[listId].Columns.PostAsync(column, null, cancellationToken))!;
        }

        /// <summary>
        /// Creates a new column by ID.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="columnId">The ID of the column.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created column definition.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/columns", Method = "POST")]
        public async Task<ColumnDefinition> CreateAsync(string siteId, string listId, string columnId, CancellationToken cancellationToken = default)
        {
            return (await client.Sites[siteId].Lists[listId].Columns.PostAsync(new() { Id = columnId }, null, cancellationToken))!;
        }

        /// <summary>
        /// Deletes a column.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="id">The ID of the column.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/columns/{id}", Method = "DELETE")]
        public Task DeleteAsync(string siteId, string listId, string id, CancellationToken cancellationToken = default)
        {
            return ExceptionHandler.ItemNotFound(
                func: async () => await client.Sites[siteId].Lists[listId].Columns[id].DeleteAsync(null, cancellationToken),
                throwIfNotFound: false);
        }

        /// <summary>
        /// Lists all columns for a specified list.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous enumerable of column definitions.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/columns", Method = "GET")]
        public IAsyncEnumerable<ColumnDefinition> ListAsync(string siteId, string listId, CancellationToken cancellationToken = default)
        {
            return client.GetAllAsync<ColumnDefinition, ColumnDefinitionCollectionResponse>(
                async () => (await client.Sites[siteId].Lists[listId].Columns.GetAsync(cancellationToken: cancellationToken))!,
                cancellationToken);
        }
    }

    /// <summary>
    /// Service class for interacting with Microsoft Graph Content Types.
    /// </summary>
    public class GraphContentTypeService
    {
        internal readonly GraphServiceClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContentTypeService"/> class.
        /// </summary>
        /// <param name="client">The GraphServiceClient instance.</param>
        internal GraphContentTypeService(GraphServiceClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        private static readonly string[] CONTENT_TYPE_SELECT = ["id", "name", "description", "group", "parentId", "order", "propagateChanges", "hidden", "readOnly", "sealed", "columnPositions", "baseTypes"];
        private static readonly string[] CONTENT_TYPE_EXPAND = ["columnPositions,columnLinks", "baseTypes($expand=columnPositions,columnLinks)"];

        /// <summary>
        /// Gets a specific content type by ID.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="throwIfNotFound">Indicates whether to throw an exception if the content type is not found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content type if found, otherwise null.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}", Method = "GET")]
        public async Task<ContentType?> GetAsync(string siteId, string listId, string contentTypeId, bool throwIfNotFound = true, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            ArgumentNullException.ThrowIfNull(contentTypeId);

            return await ExceptionHandler.ItemNotFound(
                async () =>
                {
                    var contentType = await client.Sites[siteId].Lists[listId].ContentTypes[contentTypeId].GetAsync(
                    config =>
                    {
                        config.QueryParameters.Select = CONTENT_TYPE_SELECT;
                        config.QueryParameters.Expand = CONTENT_TYPE_EXPAND;
                    }, cancellationToken);
                    return contentType!;
                }, throwIfNotFound);
        }

        /// <summary>
        /// Lists all content types for a specified list.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous enumerable of content types.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes", Method = "GET")]
        public IAsyncEnumerable<ContentType> ListAsync(string siteId, string listId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);

            return client.GetAllAsync<ContentType, ContentTypeCollectionResponse>(
                async () => (await client.Sites[siteId].Lists[listId].ContentTypes.GetAsync(
                    config =>
                    {
                        config.QueryParameters.Select = CONTENT_TYPE_SELECT;
                        config.QueryParameters.Expand = CONTENT_TYPE_EXPAND;
                    },
                        cancellationToken))!,
                    cancellationToken);
        }

        /// <summary>
        /// Lists all compatible hub content types for a specified list.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An asynchronous enumerable of content types.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/contenttypes/getCompatibleHubContentTypes", Method = "GET")]
        public IAsyncEnumerable<ContentType> ListCompatibleHubContentTypesAsync(string siteId, string listId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            return client.GetAllAsync<ContentType, GetCompatibleHubContentTypesGetResponse>(
                async () => (await client.Sites[siteId].Lists[listId].ContentTypes.GetCompatibleHubContentTypes.GetAsGetCompatibleHubContentTypesGetResponseAsync(null, cancellationToken))!,
                cancellationToken);
        }

        /// <summary>
        /// Adds a copy of a content type to a list.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added content type.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes", Method = "POST")]
        public async Task<ContentType> AddCopyAsync(string siteId, string listId, string contentTypeId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentTypeId);

            var url = client.Sites[siteId].ContentTypes[contentTypeId].ToGetRequestInformation().URI.ToString();

            return (await client.Sites[siteId].Lists[listId].ContentTypes.AddCopy.PostAsync(new() { ContentType = url }, null, cancellationToken))!;
        }

        /// <summary>
        /// Adds a copy of a content type from the content type hub to a list.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added content type.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/contenttypes/addCopyFromContentTypeHub", Method = "POST")]
        public async Task<ContentType> AddCopyFromContentTypeHubAsync(string siteId, string listId, string contentTypeId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentTypeId);

            return (await client.Sites[siteId].Lists[listId].ContentTypes.AddCopyFromContentTypeHub.PostAsync(new() { ContentTypeId = contentTypeId }, null, cancellationToken))!;
        }

        /// <summary>
        /// Updates a content type.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="contentType">The content type object with updated values.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated content type.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}", Method = "PATCH")]
        public async Task<ContentType> UpdateAsync(string siteId, string listId, string contentTypeId, ContentType contentType, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            ArgumentNullException.ThrowIfNull(contentTypeId);
            ArgumentNullException.ThrowIfNull(contentType);

            return (await client.Sites[siteId].Lists[listId].ContentTypes[contentTypeId].PatchAsync(contentType, null, cancellationToken))!;
        }

        /// <summary>
        /// Adds a column to a list content type.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="column">The column definition to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added column definition.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}/columns", Method = "POST")]
        public async Task<ColumnDefinition> AddColumnAsync(string siteId, string listId, string contentTypeId, string columnId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            ArgumentNullException.ThrowIfNull(contentTypeId);
            ArgumentNullException.ThrowIfNull(columnId);

            return (await client.Sites[siteId].Lists[listId].ContentTypes[contentTypeId].Columns.PostAsync(ToColumn(), null, cancellationToken))!;

            ColumnDefinition ToColumn()
            {
                return new()
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "sourceColumn@odata.bind", client.Sites[siteId].Lists[listId].Columns[columnId].ToGetRequestInformation().URI.ToString() }
                    }
                };
            }
        }

        /// <summary>
        /// Deletes a column from a list content type.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="columnId">The ID of the column.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/contenttypes/{contenttypeid}/columns/{columnid}", Method = "DELETE")]
        public Task DeleteColumnAsync(string siteId, string listId, string contentTypeId, string columnId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            ArgumentNullException.ThrowIfNull(contentTypeId);
            ArgumentNullException.ThrowIfNull(columnId);

            return client.Sites[siteId].Lists[listId].ContentTypes[contentTypeId].Columns[columnId].DeleteAsync(null, cancellationToken);
        }

        /// <summary>
        /// Updates a column in a list content type.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="contentTypeId">The ID of the content type.</param>
        /// <param name="columnId">The ID of the column.</param>
        /// <param name="column">The column definition with updated values.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated column definition.</returns>
        [GraphAPI("/sites/{siteId}/lists/{listId}/contenttypes/{contentTypeId}/columns/{columnId}", Method = "PATCH")]
        public async Task<ColumnDefinition> UpdateColumnAsync(string siteId, string listId, string contentTypeId, string columnId, ColumnDefinition column, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(listId);
            ArgumentNullException.ThrowIfNull(contentTypeId);
            ArgumentNullException.ThrowIfNull(columnId);
            ArgumentNullException.ThrowIfNull(column);

            return (await client.Sites[siteId].Lists[listId].ContentTypes[contentTypeId].Columns[columnId].PatchAsync(column, null, cancellationToken))!;
        }
    }

    public class GraphListItemService
    {

        internal readonly GraphServiceClient client;

        public GraphListItemService(GraphServiceClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        /// <summary>
        /// Gets a specific list item by ID.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="itemId">The ID of the list item.</param>
        /// <param name="throwIfNotFound">Whether to throw an exception if the list item is not found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list item if found, otherwise null.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/items/{itemid}")]
        public async Task<ListItem?> GetAsync(string siteId, string listId, string itemId, bool throwIfNotFound = true, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ItemNotFound(
                async () =>
                {
                    var item = await client.Sites[siteId].Lists[listId].Items[itemId].GetAsync(
                        config =>
                        {
                            config.QueryParameters.Select = PARA_LISTITEM_SELECT;
                            config.QueryParameters.Expand = PARA_LISTITEM_EXPAND;
                        }, cancellationToken);
                    return item!;
                }, throwIfNotFound);
        }

        /// <summary>
        /// Creates a new list item.
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <param name="listId">The ID of the list.</param>
        /// <param name="item">The list item object to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created list item.</returns>
        [GraphAPI("/sites/{siteid}/lists/{listid}/items", Method = "POST")]
        public async Task<ListItem> CreateAsync(string siteId, string listId, ListItem item, CancellationToken cancellationToken = default)
        {
            return (await client.Sites[siteId].Lists[listId].Items.PostAsync(item, null, cancellationToken))!;
        }
    }

}
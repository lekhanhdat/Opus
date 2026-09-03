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

using Microsoft.Graph.Sites.Item.ContentTypes.GetCompatibleHubContentTypes;
namespace Microsoft365.Graph.Service;

/// <summary>
/// Service for interacting with Microsoft Graph Site resources.
/// </summary>
public class GraphSiteService
{
    private readonly GraphServiceClient client;

    /// <summary>
    /// Gets the service for interacting with site columns.
    /// </summary>
    public GraphColumnService Columns { get; private set; }

    /// <summary>
    /// Gets the service for interacting with site content types.
    /// </summary>
    public GraphContentTypeService ContentTypes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphSiteService"/> class.
    /// </summary>
    /// <param name="client">The Graph service client.</param>
    internal GraphSiteService(GraphServiceClient client)
    {
        this.client = client;
        this.ContentTypes = new GraphContentTypeService(client);
        this.Columns = new GraphColumnService(client);
    }

    /// <summary>
    /// Service for interacting with Microsoft Graph Content Type resources.
    /// </summary>
    public class GraphContentTypeService
    {
        internal readonly GraphServiceClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContentTypeService"/> class.
        /// </summary>
        /// <param name="client">The Graph service client.</param>
        internal GraphContentTypeService(GraphServiceClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        /// <summary>
        /// Gets a content type by ID.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        /// <param name="contentTypeId">The content type ID.</param>
        /// <param name="throwIfNotFound">Whether to throw an exception if the content type is not found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The content type, if found.</returns>
        [GraphAPI("/sites/{siteid}/contenttypes/{contenttypeid}")]
        public async Task<ContentType?> GetAsync(string siteId, string contentTypeId, bool throwIfNotFound = true, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            ArgumentNullException.ThrowIfNull(contentTypeId);
            return await ExceptionHandler.ItemNotFound(
                async () => await client.Sites[siteId].ContentTypes[contentTypeId].GetAsync(null, cancellationToken),
                throwIfNotFound);
        }

        /// <summary>
        /// Lists all content types for a site.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async enumerable of content types.</returns>
        [GraphAPI("/sites/{siteid}/contenttypes")]
        public IAsyncEnumerable<ContentType> ListAsync(string siteId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            return this.client.GetAllAsync<ContentType, ContentTypeCollectionResponse>(
                initialCollection: async () => (await client.Sites[siteId].ContentTypes.GetAsync(null, cancellationToken))!,
                token: cancellationToken);
        }

        /// <summary>
        /// Lists all compatible hub content types for a site.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async enumerable of compatible hub content types.</returns>
        [GraphAPI("/sites/{siteid}/contenttypes/getcompatiblehubcontenttypes")]
        public IAsyncEnumerable<ContentType> ListCompatibleHubContentTypesAsync(string siteId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            return this.client.GetAllAsync<ContentType, GetCompatibleHubContentTypesGetResponse>(
                async () => (await client.Sites[siteId].ContentTypes.GetCompatibleHubContentTypes.GetAsGetCompatibleHubContentTypesGetResponseAsync(null, cancellationToken))!,
                cancellationToken);
        }
    }

    /// <summary>
    /// Service for interacting with Microsoft Graph Column resources.
    /// </summary>
    public class GraphColumnService
    {
        internal readonly GraphServiceClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphColumnService"/> class.
        /// </summary>
        /// <param name="client">The Graph service client.</param>
        internal GraphColumnService(GraphServiceClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        /// <summary>
        /// Lists all columns for a site.
        /// </summary>
        /// <param name="siteId">The site ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async enumerable of column definitions.</returns>
        [GraphAPI("/sites/{siteid}/columns")]
        public IAsyncEnumerable<ColumnDefinition> ListAsync(string siteId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(siteId);
            return this.client.GetAllAsync<ColumnDefinition, ColumnDefinitionCollectionResponse>(
                initialCollection: async () => (await client.Sites[siteId].Columns.GetAsync(null, cancellationToken))!,
                token: cancellationToken);
        }
    }

    [GraphAPI("/sites")]
    public IAsyncEnumerable<Site> ListSitesAsync(CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<Site, SiteCollectionResponse>(() => client.Sites.GetAsync(null, cancellationToken));
    }

    [GraphAPI("/sites?$top=1")]
    public async Task<Site?> GetOneSiteAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.Sites.GetAsync((request) =>
        {
            request.QueryParameters.Top = 1;
        }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }

}
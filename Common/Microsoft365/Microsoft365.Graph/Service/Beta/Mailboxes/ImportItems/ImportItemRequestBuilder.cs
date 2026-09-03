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
 
namespace Microsoft365.Graph.Service.ImportItems;
/// <summary>
/// Provides operations to manage the import items process for a mailbox.
/// </summary>
internal class ImportItemRequestBuilder : BaseRequestBuilder
{
    /// <summary>
    /// Configuration for the post request of an import item operation.
    /// </summary>
    public class ImportItemRequestBuilderPostRequestConfiguration : RequestConfiguration<DefaultQueryParameters>
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportItemRequestBuilder"/> class.
    /// </summary>
    /// <param name="rawUrl">The raw URL to use for the request builder.</param>
    /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
    public ImportItemRequestBuilder(string rawUrl, IRequestAdapter requestAdapter)
        : base(requestAdapter, "", rawUrl)
    {
    }

    /// <summary>
    /// Executes a POST request to import items to a mailbox.
    /// </summary>
    /// <param name="body">The request body containing import item information.</param>
    /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>An ImportItemResponse object if successful, or null if an error occurred.</returns>
    public async Task<ImportItemResponse?> PostAsImportItemResponseAsync(ImportItemPostRequestBody body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(body);
        RequestInformation requestInfo = ToPostRequestInformation(body, requestConfiguration);
        Dictionary<string, ParsableFactory<IParsable>> errorMapping = new(){
        {
            "XXX",
             GraphBetaODataErrors.ODataError.CreateFromDiscriminatorValue
        } };
        return await base.RequestAdapter.SendAsync(requestInfo, ImportItemResponse.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    /// <summary>
    /// Creates a request information object for a POST operation to import items.
    /// </summary>
    /// <param name="body">The request body containing import item information.</param>
    /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
    /// <returns>A RequestInformation object configured for the import items operation.</returns>
    public RequestInformation ToPostRequestInformation(ImportItemPostRequestBody body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(body);
        RequestInformation requestInformation = new(Method.POST, base.UrlTemplate, base.PathParameters);
        requestInformation.Configure(requestConfiguration);
        requestInformation.Headers.TryAdd("Accept", "application/json");
        if (body.DataStream != null)
        {
            requestInformation.SetStreamContent(body.ToStream(), "application/json");
        }
        else
        {
            requestInformation.SetContentFromParsable(base.RequestAdapter, "application/json", body);
        }
        return requestInformation;
    }
}

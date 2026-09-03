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
namespace Microsoft365.Graph.Service.Mailboxes;
internal class MailboxFoldersRequestBuilderV2 : BaseRequestBuilder
{
    internal MailboxFoldersRequestBuilderV2(IRequestAdapter requestAdapter, string urlTemplate, Dictionary<string, object> pathParameters)
        : base(requestAdapter, urlTemplate, pathParameters)
    {
    }

    internal async Task<GraphBetaModels.MailboxFolder?> PostAsync(GraphBetaModels.MailboxFolder body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null, CancellationToken cancellationToken = default)
    {
        RequestInformation requestInfo = ToPostRequestInformation(body, requestConfiguration);
        Dictionary<string, ParsableFactory<IParsable>> errorMapping = new Dictionary<string, ParsableFactory<IParsable>> {
        {
            "XXX",
            ODataError.CreateFromDiscriminatorValue
        } };
        return await base.RequestAdapter.SendAsync(requestInfo, GraphBetaModels.MailboxFolder.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }

    internal RequestInformation ToPostRequestInformation(GraphBetaModels.MailboxFolder body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null)
    {
        if (body == null)
        {
            throw new ArgumentNullException(nameof(body));
        }
        RequestInformation requestInformation = new RequestInformation(Method.POST, base.UrlTemplate, base.PathParameters);
        requestInformation.Configure(requestConfiguration);
        requestInformation.Headers.TryAdd("Accept", "application/json");
        requestInformation.SetContentFromParsable(base.RequestAdapter, "application/json", body);
        return requestInformation;
    }
    
    internal async Task DeleteAsync(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null, CancellationToken cancellationToken = default)
    {
        RequestInformation requestInfo = ToDeleteRequestInformation(requestConfiguration);
        Dictionary<string, ParsableFactory<IParsable>> errorMapping = new Dictionary<string, ParsableFactory<IParsable>> {
        {
            "XXX",
            ODataError.CreateFromDiscriminatorValue
        } };
        await base.RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
    }
    internal RequestInformation ToDeleteRequestInformation(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = null)
    {
        RequestInformation requestInformation = new RequestInformation(Method.DELETE, base.UrlTemplate, base.PathParameters);
        requestInformation.Configure(requestConfiguration);
        return requestInformation;
    }
}

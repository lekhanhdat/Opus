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
using GraphBetaMailboxFoldersRequestBuilder = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.FoldersRequestBuilder;
using GraphBetaMailboxChildFoldersRequestBuilder = Microsoft.Graph.Beta.Admin.Exchange.Mailboxes.Item.Folders.Item.ChildFolders.ChildFoldersRequestBuilder;
namespace Microsoft365.Graph.Service.Mailboxes;
internal static class MailboxFolderRequestBuilderExtensions
{
    internal static MailboxFoldersRequestBuilderV2 ToV2(this GraphBetaMailboxFoldersRequestBuilder builder, GraphBeta.GraphServiceClient betaClient)
    {
        return new MailboxFoldersRequestBuilderV2(
            requestAdapter: betaClient.RequestAdapter,
            urlTemplate: builder.ToGetRequestInformation().UrlTemplate!,
            pathParameters: new Dictionary<string, object>(builder.ToGetRequestInformation().PathParameters));
    }

    internal static MailboxFoldersRequestBuilderV2 ToV2(this GraphBetaMailboxChildFoldersRequestBuilder builder, GraphBeta.GraphServiceClient betaClient)
    {
        return new MailboxFoldersRequestBuilderV2(
            requestAdapter: betaClient.RequestAdapter,
            urlTemplate: builder.ToGetRequestInformation().UrlTemplate!,
            pathParameters: new Dictionary<string, object>(builder.ToGetRequestInformation().PathParameters));
    }
    internal static MailboxFoldersRequestBuilderV2 ToV2(this GraphBetaMailboxFolderItemRequestBuilder builder, GraphBeta.GraphServiceClient betaClient)
    {
        return new MailboxFoldersRequestBuilderV2(
            requestAdapter: betaClient.RequestAdapter,
            urlTemplate: builder.ToGetRequestInformation().UrlTemplate!,
            pathParameters: new Dictionary<string, object>(builder.ToGetRequestInformation().PathParameters));
    }
}

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

using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta.Users.Item.TranslateExchangeIds;

namespace Microsoft365.Graph.Service;
public partial class GraphUserService
{
    private GraphBeta.GraphServiceClient betaClient;
    
    private static readonly string[] PARA_MAILITEM_GET_EXPAND =
    [
        $"singleValueExtendedProperties($filter=id eq {string.Join(" or id eq ",
            OutlookExtendedProperties.CustomPidItemTermId,
            OutlookExtendedProperties.PidNameMSIPLabels)})"
    ];

    [MemberNotNull(nameof(betaClient))]
    private void InitBetaClient(GraphBeta.GraphServiceClient betaClient)
    {
        this.betaClient = betaClient;
    }

    /// <summary>
    /// Get primaryMailboxId and inPlaceArchiveMailboxId
    /// Microsoft has not released a v1.0, but it is currently available via beta request
    /// Application: supported
    /// Delegated: only support login user
    /// </summary>
    /// <param name="userId">idOrUserPrincipalName</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
    /// <returns>Exchange settings containing primaryMailboxId and inPlaceArchiveMailboxId</returns>
    [GraphAPI("/users/{idoruserprincipalname}/settings/exchange", IsBeta = true)]
    public async Task<GraphBetaModels.ExchangeSettings> GetExchangeSettingsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return (await betaClient.Users[userId].Settings.Exchange.GetAsync(null, cancellationToken))!;
    }

    [GraphAPI("/users/{id|userPrincipalName}/translateExchangeIds", IsBeta = true)]
    public Task<TranslateExchangeIdsPostResponse?> ConvertExchangeIds(string userId, GraphBetaModels.ExchangeIdFormat sourceType, GraphBetaModels.ExchangeIdFormat targetType, params string[] inputIds)
    {
        var requestBody = new TranslateExchangeIdsPostRequestBody
        {
            InputIds = inputIds.ToList(),
            SourceIdType = sourceType,
            TargetIdType = targetType,
        };

        return betaClient
                .Users[userId]
                .TranslateExchangeIds
                .PostAsTranslateExchangeIdsPostResponseAsync(requestBody);
    }
    
    [GraphAPI("/users/{user-id}/messages/{message-id}", IsBeta = true)]
    public async Task<GraphBetaModels.Message> GetMessageByIdAsync(string userId, string messageId, CancellationToken cancellationToken = default)
    {
        return (await betaClient.Users[userId].Messages[messageId].GetAsync(requestConfiguration: config =>
        {
            config.QueryParameters.Expand = PARA_MAILITEM_GET_EXPAND;
        }, cancellationToken))!;
    }

    [GraphAPI("/users/{user-id}/messages/{message-id}", IsBeta = true)]
    public async IAsyncEnumerable<(string Id, GraphBetaModels.Message? Item, ApiException? Error)> BatchGetItemsInfo(
      string userId,
      IEnumerable<string> itemIds,
      [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<BatchRequestStep> batchRequestSteps = new();

        var requestIdToItemId = new Dictionary<string, string>();
        foreach (var itemId in itemIds)
        {
            var request = betaClient.Users[userId].Messages[itemId]
               .ToGetRequestInformation(config =>
               {
                   config.QueryParameters.Expand = PARA_MAILITEM_GET_EXPAND;
               });
            var requestId = Guid.NewGuid().ToString();

            requestIdToItemId[requestId] = itemId;

            batchRequestSteps.Add(await betaClient.CreateBatchStepAsync(requestId, request));
        }

        await foreach (var sub in betaClient.SendBatchRequestV2Async<GraphBetaModels.Message>(batchRequestSteps.ToArray(), cancellationToken))
        {
            yield return (
                requestIdToItemId[sub.RequestId],
                sub.Result,
                sub.Error
                );
        }
    }
}

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
#nullable enable
using AvePoint.RA.CommonUtil;
using Microsoft.Graph.Beta.Models.ODataErrors;
using Microsoft365.Graph.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ExchangeBackupUtility.Graph;

/// <summary>
/// Helper class for bulk export operations of Exchange items using Microsoft Graph API.
/// Implements retry logic with exponential backoff for failed exports.
/// </summary>
public class ExchangeGraphItemBulkExporter : IExchangeItemExporter
{
    private const int MAX_RETRY = 3;
    
    
    private static readonly RALogger logger = RALogger.GetInstance(typeof(ExchangeGraphItemBulkExporter));
    private readonly GraphService service;
    private readonly string mailboxId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExchangeGraphItemBulkExporter"/> class.
    /// </summary>
    /// <param name="mailboxId">The mailbox ID to export items from.</param>
    /// <param name="service">The Graph service instance for API calls.</param>
    public ExchangeGraphItemBulkExporter(string mailboxId, GraphService service)
    {
        this.service = service;
        this.mailboxId = mailboxId;
    }

    /// <summary>
    /// Synchronously exports multiple items from the mailbox.
    /// </summary>
    /// <param name="ids">List of item EWS IDs to export.</param>
    /// <returns>Dictionary of export results keyed by item ID.</returns>
    public Dictionary<string, ExportItemResult> ExportItems(List<string> ids)
    {
        return ExportItemsAsync(ids).ExecuteAsyncTask();
    }
    /// <summary>
    /// Batch export items with Wait-Retry logic.
    /// </summary>
    /// <param name="ids">List of item EWS IDs to export.</param>
    /// <returns>Dictionary of export results keyed by item ID.</returns>
    public async Task<Dictionary<string, ExportItemResult>> ExportItemsAsync(List<string> ids)
    {
        var remainingIds = ids.Select(id => id.ToRestId()).ToList();
        Dictionary<string, ExportItemResult> failedItems;
        Dictionary<string, ExportItemResult>? items = null;
        int retry = 0;

        do
        {
            if (retry > 0) logger.Info("Retry export items, retry count: {0}, remain item count: {1}", retry, remainingIds.Count);

            try
            {
                var temp = await ExportItemsInternalAsync(remainingIds);
                items = items.Merge(temp.Where(kv => !kv.Value.Error));
                failedItems = temp.Where(kv => kv.Value.Error).ToDictionary();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to export items, retry: {0}, error: {1}", retry, ex);
                failedItems = AssembleFailedIdCollection(remainingIds, ex);
                //don't retry here since graph middleware alreday handle retry for transient error
                break;
            }
            await WaitForNextRequestAsync(failedItems, retry);
            ++retry;
            remainingIds = [.. failedItems.Keys];
        }
        while (ShouldRetry(failedItems) && retry < MAX_RETRY);

        return ConvertToEwsId(items.Merge(failedItems));
    }

    private static Dictionary<string, ExportItemResult> ConvertToEwsId(Dictionary<string, ExportItemResult> items)
    {
        return items.ToDictionary(kv => kv.Key, kv => kv.Value);
    }
    

    /// <summary>
    /// Waits before the next retry attempt using exponential backoff with jitter.
    /// </summary>
    /// <param name="failedItems">Dictionary of failed export items.</param>
    private static async Task WaitForNextRequestAsync(Dictionary<string, ExportItemResult> failedItems, int retry)
    {
        if (ShouldRetry(failedItems))
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, retry)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100));
            await Task.Delay(delay);
        }
    }
    /// <summary>
    /// Determines whether a retry should be attempted based on failed items.
    /// </summary>
    /// <param name="failedItems">Dictionary of failed export items.</param>
    /// <returns>True if retry should be attempted, false otherwise.</returns>    
    private static bool ShouldRetry(Dictionary<string, ExportItemResult> failedItems)
    {
        return !failedItems.IsEmpty() && !failedItems.Values.All(i => i.ItemNotFound);
    }

    /// <summary>
    /// Assembles a collection of failed export results from a list of IDs and an exception.
    /// </summary>
    /// <param name="ids">List of item IDs that failed.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    /// <returns>Dictionary of failed export results.</returns>
    private static Dictionary<string, ExportItemResult> AssembleFailedIdCollection(List<string> ids, Exception ex)
    {

        return ids.ToDictionary(id => id, id => ToResult(ex, id));

        static ExportItemResult ToResult(Exception ex, string id)
        {
            if (ex is ODataError odataError)
            {
                var error = odataError.Error;
                var message = error?.Message ?? odataError.Message;
                var code = error?.Code;
                return ExportItemResult.CreateFailedResult(id, message, code);
            }
            return ExportItemResult.CreateFailedResult(id, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <summary>
    /// Internal method to perform the actual export operation via Graph API.
    /// </summary>
    /// <param name="ids">List of item IDs to export.</param>
    /// <returns>Dictionary of export results keyed by item ID.</returns>
    private async Task<Dictionary<string, ExportItemResult>> ExportItemsInternalAsync(List<string> ids)
    {
        var exportResponse = await service.Mails.ExportImport.ExportItemsAsync(mailboxId, ids);
        return (exportResponse.Value ?? [])
            .Select(item => AssembleOneItem(item))
            .ToDictionary(result => result.Id, result => result);
    }

    /// <summary>
    /// Assembles an export result from a single Graph API export item response.
    /// </summary>
    /// <param name="item">The export item response from Graph API.</param>
    /// <returns>An <see cref="ExportItemResult"/> representing the export outcome.</returns>
    private static ExportItemResult AssembleOneItem(Microsoft.Graph.Beta.Models.ExportItemResponse item)
    {
        var id = item.ItemId.EnsureIfNotNullOrEmpty();
        if (item.Error == null && item.Data != null)
        {
            return ExportItemResult.CreateSuccessfulResult(id, new MemoryStream(item.Data));
        }
        else
        {
            var message = item.Error?.Message ?? "NULL";
            var code = item.Error?.Code ?? "NULL";
            logger.Warn("Failed to export item, id: {0}, error: {1}, error code: {2}.", id, message, code);
            return ExportItemResult.CreateFailedResult(id, message, code);
        }
    }
}

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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExplorerSyncNew;

public class RMSPCosmosDBProcessor
{
    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSPCosmosDBProcessor));

    private RMAzureCosmosDBDelayConcurrentAction _concurrentAction;

    private RMAzureCosmosDBContainer _container;

    public async Task<bool> PrepareAsync()
    {
        if (!await RMAzureCosmosDBContext.ExistsContainer())
        {
            s_logger.Warn($"The current tenant has not created a COSMOS DB, skipped sync.");
            return false;
        }
        _container = await RMAzureCosmosDBContext.GetContainerAsync(false);
        _concurrentAction = _container.UseConcurrentAction().ToDelay();
        await _concurrentAction.StartAsync(NotificationCallbackAsync);
        return true;
    }

    public async IAsyncEnumerable<Record> SearchItemsAsync(Expression<Func<Record, bool>> predicate)
    {

        const int searchLimit = 1000;

        var resultSet = _container.UseLinqQuery().Where(predicate).AsResultSet();

        string continuationToken = null;
        do
        {
            var result = await resultSet.PaginateAsync(continuationToken, searchLimit);
            continuationToken = result.ContinuationToken;

            foreach (var value in result.Values)
            {
                yield return value;
            }
        } while (!string.IsNullOrEmpty(continuationToken));
    }

    public async Task AddItemAsync(Record record)
    {
        s_logger.Info($"Add item [{record.Id}] to process action queue.");
        await _concurrentAction.Replace(record); 
    }

    public async Task WaitFinishAsync()
    {
        _concurrentAction.SetCompleteAdding();
        s_logger.Info("Production task completed.");

        await _concurrentAction.WaitCompletedAsync();
        s_logger.Info("Consumption task completed.");
    }

    private Task NotificationCallbackAsync(RMAzureCosmosDBDelayConcurrentActionResult actionResult)
    {

        const int maxRetriedCount = 3;

        if (actionResult.IsSucceed)
        {
            return Task.CompletedTask;
        }

        var item = actionResult.Item;

        if (actionResult.CanContinueRetry && item.RetriedCount < maxRetriedCount)
        {
            s_logger.Error($"Item [{item.Id}] can continue retry, current retried count [{item.RetriedCount}]. Error: {actionResult.Exception}");
            item.RetriedCount++;
            _concurrentAction.Action(actionResult.ActionType, item).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }

        s_logger.Error($"An error occurred while process item [{item}]. Error: {actionResult.Exception}");

        return Task.CompletedTask;
    }
}

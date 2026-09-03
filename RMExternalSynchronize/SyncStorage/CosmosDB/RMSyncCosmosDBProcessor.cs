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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Microsoft.Azure.Cosmos.Linq;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncStorage.CosmosDB
{
    public class RMSyncCosmosDBProcessor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncCosmosDBProcessor));

        private static readonly Dictionary<SourceFlag, RMContentSourceSyncCosmosDBOperator> s_operators = new()
        {
            { SourceFlag.SharePoint, new RMSharePointSyncCosmosDBOperator() },
            { SourceFlag.OneDrive, new RMOneDriveSyncCosmosDBOperator() },
            { SourceFlag.Exchange, new RMExchangeOnlineSyncCosmosDBOperator() },
            { SourceFlag.Google, new RMGoogleSyncCosmosDBOperator() },
            { SourceFlag.Teams, new RMTeamsSyncCosmosDBOperator() }
        };

        protected RMAzureCosmosDBContainer _container;

        protected RMAzureCosmosDBDelayConcurrentAction _concurrentAction;

        public async Task<bool> PrepareAsync()
        {
            if(!await RMAzureCosmosDBContext.ExistsContainer())
            {
                s_logger.Warn($"The current tenant has not created a COSMOS DB, skipped sync.");
                return false;
            }
            _container = RMAzureCosmosDBContext.GetContainerAsync(false).GetAwaiter().GetResult();
            _concurrentAction = _container.UseConcurrentAction().ToDelay();
            await _concurrentAction.StartAsync(NotificationCallbackAsync);
            return true;
        }

        public async Task<bool> AddAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var contentSourceOperator = s_operators[changeInfo.ContentSource];
                var items = SearchItemsAsync(contentSourceOperator.AddPredicate(changeInfo));
                await foreach(var item in items)
                {
                    var processedItem = contentSourceOperator.ProcessAdd(item, changeInfo);
                    _concurrentAction.Replace(processedItem);
                }

                return true;
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while process add operation of Cosmos DB. While process [{changeInfo.ContentSource}] [{SerializerHelper.SerializeByDataContractSerializer(changeInfo)}]. Error: {e}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var contentSourceOperator = s_operators[changeInfo.ContentSource];
                var items = SearchItemsAsync(contentSourceOperator.DeletePredicate(changeInfo));
                await foreach (var item in items)
                {
                    item.PreviousRecordStatus = item.RecordStatus;
                    item.RecordStatus = (int)RMRecordStatus.Hidden;
                    _concurrentAction.Replace(item);

                }

                return true;
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while process delete operation of Cosmos DB. While process [{changeInfo.ContentSource}] [{SerializerHelper.SerializeByDataContractSerializer(changeInfo)}]. Error: {e}");
                return false;
            }
        }

        public async Task<bool> MoveContainerAsync(RMSyncNodeChangeInfo changeInfo)
        {
            try
            {
                var contentSourceOperator = s_operators[changeInfo.ContentSource];
                var items = SearchItemsAsync(contentSourceOperator.MovePredicate(changeInfo));
                await foreach (var item in items)
                {
                    var processedItem = contentSourceOperator.ProcessMove(item, changeInfo);
                    _concurrentAction.Replace(processedItem);
                }

                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process move operation of Cosmos DB. While process [{changeInfo.ContentSource}] [{SerializerHelper.SerializeByDataContractSerializer(changeInfo)}]. Error: {e}");
                return false;
            }
        }

        public async Task<bool> ChangeSourceFlagAsync(RMRemoteNode changeInfo, SourceFlag sourceFlag = SourceFlag.Teams)
        {
            try
            {
                var contentSourceOperator = s_operators[sourceFlag];
                var items = SearchItemsAsync(contentSourceOperator.ChangeSourceFlagPredicate(changeInfo));
                await foreach (var item in items)
                {
                    var processedItem = contentSourceOperator.ProcessChangeSourceFlag(item, changeInfo);
                    _concurrentAction.Replace(processedItem);
                }

                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process change source flag operation of Cosmos DB. While process [{sourceFlag}] [{changeInfo.Id}]. Error: {e}");
                return false;
            }
        }

        public async Task WaitFinishAsync()
        {
            _concurrentAction.SetCompleteAdding();
            s_logger.Info("Production task completed.");

            await _concurrentAction.WaitCompletedAsync();
            s_logger.Info("Consumption task completed.");
        }

        private async IAsyncEnumerable<Record> SearchItemsAsync(Expression<Func<Record, bool>> predicate)
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

        private Task NotificationCallbackAsync(RMAzureCosmosDBDelayConcurrentActionResult actionResult)
        {

            const int maxRetriedCount = 3;

            if(actionResult.IsSucceed)
            {
                return Task.CompletedTask;
            }

            var item = actionResult.Item;

            if(actionResult.CanContinueRetry && item.RetriedCount < maxRetriedCount)
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
}

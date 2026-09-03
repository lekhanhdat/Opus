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
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.DataIngestion.DataStorage;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.Processor.AgentWork.Ingestor
{
    public abstract class RMDataIngestionAgentWorkIngestor
    {
        private const int DEFAULT_CHANNEL_CAPACITY = 1_000;

        protected readonly RALogger _logger;

        protected readonly RMDataIngestionBlobDataReader _dataReader;

        private readonly RMAzureCosmosDBDelayConcurrentAction _concurrentAction;

        /// <summary>
        /// failed result channel
        /// </summary>
        protected readonly Channel<RMDataIngestionAgentWorkItemExecutionResult> _resultChannel;

        public abstract RMDataIngestionOperationType OperationType { get; }

        public RMDataIngestionAgentWorkIngestor(RMDataIngestionBlobDataReader dataReader, Type loggerType)
        {
            _logger = RALogger.GetInstance(loggerType ?? typeof(RMDataIngestionAgentWorkIngestor));
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            _resultChannel = Channel.CreateBounded<RMDataIngestionAgentWorkItemExecutionResult>(new BoundedChannelOptions(DEFAULT_CHANNEL_CAPACITY)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = false,
                SingleReader = false
            });
            var container = RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();
            _concurrentAction = container
                .UseConcurrentAction()
                .WithMaxDegreeOfParallelism(10)
                .WithRetryTimes(3)
                .WithInitialRetryDelayTime(500)
                .ToDelay();
        }

        protected abstract IAsyncEnumerable<Record> ReadItemsAsync();

        public async Task IngestAsync()
        {
            await _concurrentAction.StartAsync(OnNotifyAsync).ConfigureAwait(false);
            int counter = 0;
            try
            {
                await foreach(var item in ReadItemsAsync().ConfigureAwait(false))
                {
                    counter++;
                    await _concurrentAction.Upsert(item).ConfigureAwait(false);
                }

                _concurrentAction.SetCompleteAdding();
                await _concurrentAction.WaitCompletedAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred during ingestion. Error: {0}", ex);
            }
            finally
            {
                await _concurrentAction.DisposeAsync().ConfigureAwait(false);
                _logger.Info($"IngestAsync Complete with total records {counter}");
                _resultChannel.Writer.Complete();
            }
        }

        public IAsyncEnumerable<RMDataIngestionAgentWorkItemExecutionResult> ReadItemExecutionResultsAsync()
        {
            return _resultChannel.Reader.ReadAllAsync();
        }

        protected abstract Task OnNotifyAsync(RMAzureCosmosDBDelayConcurrentActionResult result);
    }
}

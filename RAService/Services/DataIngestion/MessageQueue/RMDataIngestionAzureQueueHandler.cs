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
using AvePoint.RA.DB.AzureQueue;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DataIngestion.MessageQueue
{
    public abstract class RMDataIngestionAzureQueueHandler
    {

        private static readonly TimeSpan DEFAULT_VISIBILITY_TIMEOUT = TimeSpan.FromMinutes(30);

        private readonly RMAzureQueueDataSet _queue;

        private readonly RALogger _logger;

        public abstract RMDataIngestionType IngestionType { get; }

        protected RMDataIngestionAzureQueueHandler(RMAzureQueueDataSet queue, Type loggerType)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _logger = RALogger.GetInstance(loggerType ?? typeof(RMDataIngestionAzureQueueHandler));
        }

        public Task<string> EnqueueAsync(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ExecuteAsync(() => _queue.Enqueue(message), "Enqueue message failed");
        }

        public Task<QueueMessage> DequeueAsync()
        {
            return ExecuteAsync(() => _queue.Dequeue(DEFAULT_VISIBILITY_TIMEOUT), "Dequeue messages failed");
        }

        public Task DeleteAsync(QueueMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ExecuteAsync(() => _queue.Delete(message.MessageId, message.PopReceipt), "Delete message failed");
        }

        public Task<UpdateReceipt> RenewVisibilityAsync(QueueMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return ExecuteAsync(() => _queue.RenewVisibility(message.MessageId, message.PopReceipt, DEFAULT_VISIBILITY_TIMEOUT), "Renew message visibility failed");
        }

        public async Task<bool> HasMessagesAsync()
        {
            var message = await ExecuteAsync(() => _queue.Peek(), "Check queue messages failed");
            return message != null;
        }

        private async Task ExecuteAsync(Func<Task> action, string operation)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("{0}. Error: {1}", operation, ex);
                throw;
            }
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, string operation)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("{0}. Error: {1}", operation, ex);
                throw;
            }
        }
    }
}

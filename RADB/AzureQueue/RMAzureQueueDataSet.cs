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
using AvePoint.RA.Contract.Tenant;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureQueue
{
    public class RMAzureQueueDataSet
    {
        public RMAzureQueueContext Context { get; }

        public string QueueName { get; }

        public bool EnableMultipleTenant { get; }

        public RMAzureQueueDataSet(RMAzureQueueContext context, string queueName)
            : this(context, queueName, false)
        { }

        public RMAzureQueueDataSet(RMAzureQueueContext context, string queueName, bool enableMultipleTenant)
        {
            Context = context;
            QueueName = queueName;
            EnableMultipleTenant = enableMultipleTenant;
        }

        public async Task CreateIfNotExists()
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            await client.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        public async Task<bool> Exists()
        {
            var client = await GetQueueClient(false).ConfigureAwait(false);
            var response = await client.ExistsAsync().ConfigureAwait(false);
            return response.Value;
        }

        public async Task<string> Enqueue(string messageText, TimeSpan? visibilityTimeout = null, TimeSpan? timeToLive = null)
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.SendMessageAsync(messageText, visibilityTimeout, timeToLive).ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureQueueException(response.GetRawResponse().ReasonPhrase);
            }

            return response.Value.MessageId;
        }

        public async Task<PeekedMessage> Peek()
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.PeekMessagesAsync(1).ConfigureAwait(false);
            return response.Value.FirstOrDefault();
        }

        public async Task<QueueMessage> Dequeue(TimeSpan? visibilityTimeout = null)
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.ReceiveMessagesAsync(1, visibilityTimeout).ConfigureAwait(false);
            return response.Value.FirstOrDefault();
        }

        public async Task<UpdateReceipt> Update(string messageId, string popReceipt, string messageText, TimeSpan? visibilityTimeout = null)
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var timeout = visibilityTimeout ?? TimeSpan.Zero;
            var response = await client.UpdateMessageAsync(messageId, popReceipt, messageText, timeout).ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureQueueException(response.GetRawResponse().ReasonPhrase);
            }

            return response.Value;
        }

        public Task<UpdateReceipt> Update(QueueMessage message, string messageText, TimeSpan? visibilityTimeout = null)
        {
            return Update(message.MessageId, message.PopReceipt, messageText, visibilityTimeout);
        }

        public async Task<UpdateReceipt> RenewVisibility(string messageId, string popReceipt, TimeSpan visibilityTimeout)
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.UpdateMessageAsync(messageId, popReceipt, messageText: string.Empty, visibilityTimeout).ConfigureAwait(false);
            if (response.GetRawResponse().IsError)
            {
                throw new RMAzureQueueException(response.GetRawResponse().ReasonPhrase);
            }

            return response.Value;
        }

        public Task<UpdateReceipt> RenewVisibility(QueueMessage message, TimeSpan visibilityTimeout)
        {
            return RenewVisibility(message.MessageId, message.PopReceipt, visibilityTimeout);
        }

        public async Task Delete(string messageId, string popReceipt)
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.DeleteMessageAsync(messageId, popReceipt).ConfigureAwait(false);
            if (response.IsError)
            {
                throw new RMAzureQueueException(response.ReasonPhrase);
            }
        }

        public Task Delete(QueueMessage message)
        {
            return Delete(message.MessageId, message.PopReceipt);
        }

        public async Task Clear()
        {
            var client = await GetQueueClient(true).ConfigureAwait(false);
            var response = await client.ClearMessagesAsync().ConfigureAwait(false);
            if (response.IsError)
            {
                throw new RMAzureQueueException(response.ReasonPhrase);
            }
        }

        private async Task<QueueClient> GetQueueClient(bool createIfNotExists)
        {
            var queueName = QueueName;
            if (EnableMultipleTenant)
            {
                queueName = QueueName + TenantLocalValue.LogonGroupId.Replace("-", "").ToLower();
            }

            return await Context.GetQueueClientAsync(queueName, createIfNotExists).ConfigureAwait(false);
        }
    }
}

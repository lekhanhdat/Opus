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
using Azure.Storage.Queues;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.DB.AzureQueue
{
    public class RMAzureQueueContext
    {
        public string ConnectionString { get; }

        public QueueServiceClient ServiceClient { get; }

        private readonly ConcurrentDictionary<string, QueueClient> QueueClients = new();

        private readonly SemaphoreSlim AsyncLock = new(1);

        public RMAzureQueueContext(string connectionString)
        {
            ConnectionString = connectionString;
            ServiceClient = GetServiceClient(ConnectionString);
        }

        public Task<QueueClient> GetQueueClientAsync(string queueName)
        {
            return GetQueueClientAsync(queueName, false);
        }

        public async Task<QueueClient> GetQueueClientAsync(string queueName, bool createIfNotExists)
        {
            if (!QueueClients.ContainsKey(queueName))
            {
                try
                {
                    await AsyncLock.WaitAsync().ConfigureAwait(false);
                    if (!QueueClients.ContainsKey(queueName))
                    {
                        var queueClient = ServiceClient.GetQueueClient(queueName);
                        if (createIfNotExists)
                        {
                            await queueClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                        }
                        QueueClients[queueName] = queueClient;
                    }
                }
                finally
                {
                    AsyncLock.Release();
                }
            }

            return QueueClients[queueName];
        }

        public static QueueServiceClient GetServiceClient(string connectionString)
        {
            connectionString = connectionString.Replace(".blob.", ".queue.");
            return IsConnectionString(connectionString) ? new QueueServiceClient(connectionString) : new QueueServiceClient(new Uri("https://" + connectionString), IdentityUtil.GetCredential());
        }

        internal static bool IsConnectionString(string connectionString)
        {
            return connectionString.StartsWith("DefaultEndpointsProtocol=");
        }
    }
}

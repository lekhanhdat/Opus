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
using AvePoint.RA.Common.Util;
using Azure.Storage.Blobs;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureStorageBlob
{
    public class RMAzureStorageBlobContext
    {
        public string ConnectionString { get; }

        public BlobServiceClient ServiceClient { get; }

        private readonly ConcurrentDictionary<string, BlobContainerClient> ContainerClients = new();

        private readonly SemaphoreSlim AsyncLock = new(1);

        public RMAzureStorageBlobContext(string connectionString)
        {
            ConnectionString = connectionString;
            ServiceClient = AzureUtil.GetBlobServiceClient(ConnectionString);
        }

        public Task<BlobContainerClient> GetContainerClientAsync(string containerName)
        {
            return GetContainerClientAsync(containerName, false);
        }

        public async Task<BlobContainerClient> GetContainerClientAsync(string containerName, bool createIfNotExists)
        {
            if (!ContainerClients.ContainsKey(containerName))
            {
                try
                {
                    await AsyncLock.WaitAsync().ConfigureAwait(false);
                    if (!ContainerClients.ContainsKey(containerName))
                    {
                        var containerClient = ServiceClient.GetBlobContainerClient(containerName);
                        if (createIfNotExists)
                        {
                            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
                        }

                        ContainerClients[containerName] = containerClient;
                    }
                }
                finally
                {
                    AsyncLock.Release();
                }
            }

            return ContainerClients[containerName];
        }
    }
}

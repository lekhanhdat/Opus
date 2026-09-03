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
using Azure.Data.Tables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util.MSAzure;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.DB.AzureTable
{
    public class RMAzureTableContext
    {
        public string ConnectionString { get; private set; }

        public TableServiceClient ServiceClient { get; private set; }

        private readonly Dictionary<string, TableClient> TableClients = new();

        private readonly SemaphoreSlim AsyncLock = new(1);

        public RMAzureTableContext(string connectionString)
        {
            ConnectionString = connectionString;
            ServiceClient = AzureUtil.GetServiceClient(ConnectionString);
        }

        public Task<TableClient> GetTableClientAsync(string tableName)
        {
            return GetTableClientAsync(tableName, false);
        }

        public async Task<TableClient> GetTableClientAsync(string tableName, bool createIfNotExists)
        {
            if (!TableClients.ContainsKey(tableName))
            {
                try
                {
                    await AsyncLock.WaitAsync();
                    if (!TableClients.ContainsKey(tableName))
                    {
                        var tableClient = ServiceClient.GetTableClient(tableName);
                        if (createIfNotExists)
                        {
                            await tableClient.CreateIfNotExistsAsync();
                        }
                        TableClients[tableName] = tableClient;
                    }
                }
                finally
                {
                    AsyncLock.Release();
                }
            }

            return TableClients[tableName];
        }
    }
}

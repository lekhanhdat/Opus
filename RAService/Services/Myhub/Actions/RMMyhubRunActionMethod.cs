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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MyHub.Actions
{
    public class RMMyhubRunActionMethod
    {
        private readonly Lazy<Task<Container>> _containerFactory;

        public RMMyhubRunActionMethod()
        {
            _containerFactory = new Lazy<Task<Container>>(CreateContainerAsync);
        }
        private static async Task<Container> CreateContainerAsync()
        {
            var connectionInfo = await RMDBContextManager.GetExplorerDBConnectionInfoAsync();
            var client = new CosmosClientManager(TenantLocalValue.LogonGroupId).Client;
            return client.GetDatabase(connectionInfo.DatabaseId).GetContainer(connectionInfo.CollectionId);
        }
        internal async Task<Container> GetContainerAsync()
        {
            return await _containerFactory.Value;
        }
        public (string sql, List<SqlParameter> parameter) BuildQueryAsync(RMMyhubActionInfo actionQueryInfo)
        {
            if (actionQueryInfo?.Id == null || actionQueryInfo.Id.Length == 0)
            {
                return (null, null);
            }

            var ids = actionQueryInfo.Id.Distinct().ToList();

            return BuildBatchQuery(ids);
        }
        //构建分区键，用于定位到需要更新的record
        internal static PartitionKey BuildPartitionKey(RMMyhubActionTarget target)
        {
            return new PartitionKeyBuilder().Add(target.L1PartitionKey).Add(target.L2PartitionKey).Add(target.L3PartitionKey).Build();
        }

        internal (string sql, List<SqlParameter> parameters) BuildBatchQuery(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return (null, null);

            // 去重
            var distinctIds = ids.Distinct().ToList();

            var sql = @"
        SELECT VALUE {
            ""SelectId"": c.nodeId,
            ""Name"": c.leafName,
            ""DirPath"": c.dirPath,
            ""TimeModified"": c.timeModified,
            ""L1PartitionKey"": c.l1PartitionKey,
            ""L2PartitionKey"": c.l2PartitionKey,
            ""L3PartitionKey"": c.l3PartitionKey
        }
        FROM c
        WHERE c.sourceFlag = @sourceFlag
        AND ARRAY_CONTAINS(@ids, c.nodeId)
        AND c.recordStatus = @statuses
        AND IS_DEFINED(c.recordsId)
        AND NOT IS_NULL(c.recordsId)";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
        new SqlParameter("@ids", distinctIds), 
        new SqlParameter("@statuses", (int)RMRecordStatus.Active)
    };

            return (sql, parameters);
        }
    }
}

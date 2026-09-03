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
using AvePoint.RA.DB.Core.Discovery;
using Newtonsoft.Json;
using RADiscovery.Query.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.SourceQuerier
{
    public class DiscoverySizeRangeQuerier : DiscoverySourceQuerier
    {
        public DiscoverySizeRangeQuerier(DiscoveryQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public async Task<List<DiscoverySizeRangeDataInfo>> QueryInactiveDataInfo()
        {
            try
            {
                var dataTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? GetInactiveDataTableName() : GetRotDataTableName();
                var sumQuerySql = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "SUM(data.Size)" : "SUM(FileTotalSize)";

                var sql = $@"SELECT sizeRange.Id AS Id, sizeRange.DisplayName AS Name, sizeRange.[Order] AS [Order], {sumQuerySql} AS TotalSize FROM [{_schemaName}].[{dataTable}] AS data
JOIN [dbo].[RMSizeRange] AS sizeRange ON data.SizeRange = sizeRange.Id ";

                var sqlDefinitions = new List<DiscoverySqlDefinition>();

                var (needJoinNode, tableName, joinColumn, alias) = _queryParameter.NodeQueryParameter.NeedJoin();
                if (needJoinNode)
                {
                    sql += $"JOIN [{_schemaName}].[{tableName}] AS {alias} ON data.{joinColumn} = {alias}.Id ";
                }

                var getRes = _queryParameter.NodeQueryParameter.TryGetSqlDefinition(needJoinNode ? alias : "data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                var whereSql = string.Join(" AND ", sqlDefinitions.Select(item => item.Sql));
                sql += $"WHERE {whereSql}";
                sql += $" GROUP BY sizeRange.Id, sizeRange.DisplayName, sizeRange.[Order]";

                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var dataCollection = await context.ExecuteQueryAsync(sql, sqlDefinitions.SelectMany(item => item.Parameters).ToArray());
                return dataCollection.ToList<DiscoverySizeRangeDataInfo>().OrderByDescending(item => item.Order).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while query size range of inactive data ({_queryParameter.GetJsonInfo()}). Error: {ex}");
                return new List<DiscoverySizeRangeDataInfo>();
            }
        }
    }

    public class DiscoverySizeRangeDataInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }
}

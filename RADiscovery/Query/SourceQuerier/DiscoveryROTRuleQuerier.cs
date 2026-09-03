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
    public class DiscoveryROTRuleQuerier : DiscoverySourceQuerier
    {
        public DiscoveryROTRuleQuerier(DiscoveryQueryParameter queryParameter) : base(queryParameter)
        {

        }

        public async Task<List<DiscoveryROTRuleDataInfo>> QueryROTDataInfo()
        {
            try
            {
                var dataTable = GetRotDataTableName();
                var sql = $@"SELECT rotRule.Id AS Id, rotRule.Name AS Name, SUM(data.FileTotalSize) AS TotalSize, SUM(data.FileCount) AS TotalCount FROM [{_schemaName}].[{dataTable}] AS data
JOIN [dbo].[RMRuleInfo] AS rotRule ON data.Rule = rotRule.Id ";

                var sqlDefinitions = new List<DiscoverySqlDefinition>();

                var (needJoinNode, tableName, joinColumn, alias) = _queryParameter.NodeQueryParameter.NeedJoin();
                if (needJoinNode)
                {
                    sql += $"JOIN [{_schemaName}].[{tableName}] AS {alias} ON data.{joinColumn} = {alias}.Id ";
                }

                var getRes = _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.NodeQueryParameter.TryGetSqlDefinition(needJoinNode ? alias : "data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.SizeRangeQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                getRes = _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                var whereSql = string.Join(" AND ", sqlDefinitions.Select(item => item.Sql));
                sql += $"WHERE {whereSql}";
                sql += $" GROUP BY rotRule.Id, rotRule.Name ";

                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var dataCollection = await context.ExecuteQueryAsync(sql, sqlDefinitions.SelectMany(item => item.Parameters).ToArray());
                return dataCollection.ToList<DiscoveryROTRuleDataInfo>().OrderByDescending(item => item.TotalSize).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while query file types of rot data ({_queryParameter.GetJsonInfo()}). Error: {ex}");
                return new List<DiscoveryROTRuleDataInfo>();

            }
        }
    }

    public class DiscoveryROTRuleDataInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("totalCount")]
        public long TotalCount { get; set; }
    }
}

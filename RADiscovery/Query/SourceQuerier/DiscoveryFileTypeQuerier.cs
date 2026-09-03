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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.DB.Core.Discovery;
using Newtonsoft.Json;
using RADiscovery.Query.Parameter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.SourceQuerier
{
    public class DiscoveryFileTypeQuerier : DiscoverySourceQuerier
    {
        public DiscoveryFileTypeQuerier(DiscoveryQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public async Task<List<DiscoveryFileTypeDataInfo>> QueryInactiveDataInfo()
        {
            try
            {
                var dataTable = GetInactiveDataTableName();
                var sql = $@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(data.Size) AS TotalSize FROM [{_schemaName}].[{dataTable}] AS data
JOIN [{_schemaName}].[RMFileTypes] AS fileType ON data.FileType = fileType.Id ";

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
                sql += $" GROUP BY fileType.Id, fileType.Name";

                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var dataCollection = await context.ExecuteQueryAsync(sql, sqlDefinitions.SelectMany(item => item.Parameters).ToArray());
                return dataCollection.ToList<DiscoveryFileTypeDataInfo>().OrderByDescending(item => item.TotalSize).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while query file types of inactive data ({_queryParameter.GetJsonInfo()}). Error: {ex}");
                return new List<DiscoveryFileTypeDataInfo>();
            }
        }

        public async Task<List<DiscoveryFileTypeDataInfo>> QueryROTDataInfo()
        {
            try
            {
                var dataTable = GetRotDataTableName();
                var sql = $@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(FileTotalSize.Size) AS TotalSize FROM [{_schemaName}].[{dataTable}] AS data
JOIN [{_schemaName}].[RMFileTypes] AS fileType ON data.FileType = fileType.Id ";

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

                getRes = _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("data");
                if (getRes.has)
                {
                    sqlDefinitions.Add(getRes.sqlDefinition);
                }

                var whereSql = string.Join(" AND ", sqlDefinitions.Select(item => item.Sql));
                sql += $"WHERE {whereSql}";
                sql += $" GROUP BY fileType.Id, fileType.Name";

                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var dataCollection = await context.ExecuteQueryAsync(sql, sqlDefinitions.SelectMany(item => item.Parameters).ToArray());
                return dataCollection.ToList<DiscoveryFileTypeDataInfo>().OrderByDescending(item => item.TotalSize).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while query file types of ROT data ({_queryParameter.GetJsonInfo()}). Error: {ex}");
                return new List<DiscoveryFileTypeDataInfo>();
            }
        }
    }

    public class DiscoveryFileTypeDataInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }
    }
}

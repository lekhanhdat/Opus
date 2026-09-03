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
using AvePoint.RA.DB.Model.Discovery;
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
    public class DiscoveryNodeQuerier : DiscoverySourceQuerier
    {
        public DiscoveryNodeQuerier(DiscoveryQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public async Task<DiscoveryNodeInfo> QueryInactiveDataInfo()
        {
            try
            {
                await using var context = await RMDiscoveryDBManager.GetContextAsync();
                var sql = @"SELECT UniqueId FROM [dbo].[RMRuleInfo] WHERE DefinitionKind = 1 AND IsEnable = 1";
                var dataCollection = await context.ExecuteQueryAsync(sql);
                var inactiveRules = dataCollection.ToList<RMDiscoveryRuleInfo>();
                var needSumColumns = inactiveRules.Select(item => "c" + item.UniqueId.ToString().Replace("-", "")).ToList();

                return await (_queryParameter.NodeQueryParameter.ViewMode switch
                {
                    DiscoveryNodeViewMode.Container => QueryContainerViewInactiveDataInfo(needSumColumns),
                    DiscoveryNodeViewMode.Site => QuerySiteViewInactiveDataInfo(needSumColumns),
                    DiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewInactiveDataInfo(needSumColumns),
                    _ => throw new Exception()
                });
            }
            catch(Exception ex)
            {
                _logger.Error($"An error occurred while query node of inactive data ({_queryParameter.GetJsonInfo()}). Error: {ex}");
                return new ();
            }
        }

        private async Task<DiscoveryNodeInfo> QueryContainerViewInactiveDataInfo(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sumQuerySql = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ?
                @"SUM(data.Size) AS inactiveFileTotalSize,
                SUM(data.Count) AS inactiveFileCount, " :
                @"SUM(data.FileTotalSize) AS rotFileTotalSize,
                SUM(data.FileCount) AS rotFileCount,";
            var containerTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMContainerInactiveData]" : "[RMContainerRotData]";

            var sql = $@"SELECT 
container.Id AS id, 
container.Name AS name, 
container.SiteCount AS siteCount,
{sumQuerySql} ";

            foreach(var needSumColumn in needSumColumns)
            {
                sql += $"SUM(data.{needSumColumn}) AS {needSumColumn},";
            }
            sql = sql.TrimEnd(',');
            sql += $@" FROM [{_schemaName}].{containerTable} AS data
JOIN [{_schemaName}].[RMContainerInfoes] AS container ON data.containerId = container.Id
WHERE container.Id IN (
    SELECT innerTable.Id AS Id FROM [{_schemaName}].[RMContainerInfoes] AS innerTable 
    [innerWhere]
) ";
            var sqlParameters = new List<SqlParameter>();
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("innerTable");
            if(getRes.has)
            {
                sql = sql.Replace("[innerWhere]", " WHERE " + getRes.sqlDefinition.Sql);
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }
            else
            {
                sql = sql.Replace("[innerWhere]", "");
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if(otherDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
            }

            sql += @$" GROUP BY container.Id, container.Name, container.SiteCount, container.FileTotalSize, container.FileCount 
ORDER BY container.FileTotalSize DESC
OFFSET {nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize} ROWS FETCH NEXT {nodeQueryParameter.PageSize} ROWS ONLY";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            var totalContainerCount = await GetContainerViewTotalCount();
            var result = new DiscoveryNodeInfo { TotalNodeCount = totalContainerCount, Items = dataCollection.ToDictionary() };
            result.TotalNodeCount = totalContainerCount;
            result.Items = dataCollection.ToDictionary();
            return result;
        }

        private async Task<DiscoveryNodeInfo> QuerySiteViewInactiveDataInfo(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var siteTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMSiteInactiveData]" : "[RMSiteRotData]";
            var sumQuerySql = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ?
               @"SUM(data.Size) AS inactiveFileTotalSize,
                SUM(data.Count) AS inactiveFileCount, " :
               @"SUM(data.FileTotalSize) AS rotFileTotalSize,
                SUM(data.FileCount) AS rotFileCount, ";

            var sql = $@"SELECT 
site.Id AS id, 
site.Url AS url, 
site.FileTotalSize AS fileTotalSize,
site.FileCount AS fileCount,
{sumQuerySql}
";
            foreach (var needSumColumn in needSumColumns)
            {
                sql += $"SUM(data.{needSumColumn}) AS {needSumColumn},";
            }
            sql = sql.TrimEnd(',');
            sql += $@" FROM [{_schemaName}].{siteTable} AS data
JOIN [{_schemaName}].[RMSiteInfoes] AS site ON data.siteid = site.Id
WHERE site.Id IN (
    SELECT innerTable.Id AS Id FROM [{_schemaName}].[RMSiteInfoes] AS innerTable [innerWhere] 
) ";
            var sqlParameters = new List<SqlParameter>();
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("innerTable");
            if (getRes.has)
            {
                sql = sql.Replace("[innerWhere]", " WHERE " + getRes.sqlDefinition.Sql);
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }
            else
            {
                sql = sql.Replace("[innerWhere]", "");
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if (otherDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
            }

            sql += @$" GROUP BY site.Id, site.Url, site.FileTotalSize, site.FileCount
ORDER BY site.FileTotalSize DESC
OFFSET {nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize} ROWS FETCH NEXT {nodeQueryParameter.PageSize} ROWS ONLY";

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            var totalSiteCount = await GetSiteViewTotalCount();
            var result = new DiscoveryNodeInfo();
            result.TotalNodeCount = totalSiteCount;
            result.Items = dataCollection.ToDictionary();
            return result;
        }

        private async Task<DiscoveryNodeInfo> QuerySiteInContainerViewInactiveDataInfo(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var siteTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMSiteInactiveData]" : "[RMSiteRotData]";
            var sumQuerySql = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ?
               @"SUM(data.Size) AS inactiveFileTotalSize,
                SUM(data.Count) AS inactiveFileCount, " :
               @"SUM(data.FileTotalSize) AS rotFileTotalSize,
                SUM(data.FileCount) AS rotFileCount, ";

            var sql = $@"SELECT 
site.Id AS id, 
site.Url AS url, 
site.FileTotalSize AS fileTotalSize,
site.FileCount AS fileCount,
{sumQuerySql}
";
            foreach (var needSumColumn in needSumColumns)
            {
                sql += $"SUM(data.{needSumColumn}) AS {needSumColumn},";
            }
            sql = sql.TrimEnd(',');
            sql += $@" FROM [{_schemaName}].{siteTable} AS data
JOIN [{_schemaName}].[RMSiteInfoes] AS site ON data.siteid = site.Id
WHERE site.Id IN (
    SELECT innerTable.Id AS Id FROM [{_schemaName}].[RMSiteInfoes] AS innerTable
    WHERE
    innerTable.ContainerId = @ContainerId
    [innerWhere] 
) ";
            var sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId)
            };
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("innerTable");
            if (getRes.has)
            {
                sql = sql.Replace("[innerWhere]", " AND " + getRes.sqlDefinition.Sql);
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }
            else
            {
                sql = sql.Replace("[innerWhere]", "");
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if (otherDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
            }

            sql += @$" GROUP BY site.Id, site.Url, site.FileTotalSize, site.FileCount 
ORDER BY site.FileTotalSize DESC
OFFSET {nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize} ROWS FETCH NEXT {nodeQueryParameter.PageSize} ROWS ONLY";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            var totalSiteInContainerCount = await GetSiteInContainerTotalCount();
            var result = new DiscoveryNodeInfo();
            result.TotalNodeCount = totalSiteInContainerCount;
            result.Items = dataCollection.ToDictionary();
            return result;
        }

        private List<DiscoverySqlDefinition> GetOtherInactiveConditionSqlDefinitions()
        {
            var sqlDefinitions = new List<DiscoverySqlDefinition>();

            var getRes = _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition("data");
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

            return sqlDefinitions;
        }

        private async Task<int> GetContainerViewTotalCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var containerTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMContainerInactiveData]" : "[RMContainerRotData]";

            var sql = $@"SELECT COUNT(1) FROM (SELECT container.Id AS id ";

            sql += $@" FROM [{_schemaName}].[RMContainerInfoes] AS container
JOIN [{_schemaName}].{containerTable} AS data ON data.containerId = container.Id ";

            var sqlParameters = new List<SqlParameter>();
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("container");
            if (getRes.has)
            {
                sql += " WHERE " + getRes.sqlDefinition.Sql;
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if (otherDefinitions.Any())
            {
                if (getRes.has)
                {
                    sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
                }
                else
                {
                    sql += " WHERE " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
                }
            }

            sql += " GROUP BY container.Id) AS totalCount";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            return dataCollection.ToList<int>().FirstOrDefault();
        }

        private async Task<int> GetSiteViewTotalCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var siteTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMSiteInactiveData]" : "[RMSiteRotData]";

            var sql = $@"SELECT COUNT(1) FROM (SELECT site.Id AS id ";
            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
JOIN [{_schemaName}].{siteTable} AS data ON data.siteid = site.Id ";
            var sqlParameters = new List<SqlParameter>();
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("site");
            if (getRes.has)
            {
                sql += " WHERE " + getRes.sqlDefinition.Sql;
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if (otherDefinitions.Any())
            {
                if (getRes.has)
                {
                    sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
                }
                else
                {
                    sql += " WHERE " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
                }
            }

            sql += " GROUP BY site.Id) AS totalCount";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            return dataCollection.ToList<int>().FirstOrDefault();
        }

        private async Task<int> GetSiteInContainerTotalCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var siteTable = _queryParameter.DataType == DiscoveryQueryDataType.Inactive ? "[RMSiteInactiveData]" : "[RMSiteRotData]";


            var sql = $@"SELECT COUNT(1) FROM (SELECT 
site.Id AS id ";

            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
JOIN [{_schemaName}].{siteTable} AS data ON data.siteid = site.Id 
WHERE site.ContainerId = @ContainerId";

            var sqlParameters = new List<SqlParameter>()
            {
                new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId)
            };
            var getRes = nodeQueryParameter.TryGetSearchKeySqlDefinition("site");
            if (getRes.has)
            {
                sql += " AND " + getRes.sqlDefinition.Sql;
                sqlParameters.AddRange(getRes.sqlDefinition.Parameters);
            }

            var otherDefinitions = GetOtherInactiveConditionSqlDefinitions();
            if (otherDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", otherDefinitions.Select(item => item.Sql));
            }

            sql += " GROUP BY site.Id) AS totalCount";
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var dataCollection = await context.ExecuteQueryAsync(sql, otherDefinitions.SelectMany(item => item.Parameters).Concat(sqlParameters).ToArray());
            return dataCollection.ToList<int>().FirstOrDefault();
        }
    }

    public class DiscoveryNodeInfo
    {
        [JsonProperty("totalNodeCount")]
        public int TotalNodeCount { get; set; }

        [JsonProperty("items")]
        public List<Dictionary<string, object>> Items { get; set; }
    }
}

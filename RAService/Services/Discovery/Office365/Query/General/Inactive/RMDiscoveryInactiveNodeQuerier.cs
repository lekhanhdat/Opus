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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Inactive
{
    public class RMDiscoveryInactiveNodeQuerier : RMDiscoveryInactiveDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryInactiveNodeQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();

            var items = await (_queryParameter.NodeQueryParameter.ViewMode switch
            {
                RMDiscoveryNodeViewMode.Container => QueryContainerViewItems(needSumColumns),
                RMDiscoveryNodeViewMode.Site => QuerySiteViewItems(needSumColumns),
                RMDiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewItems(needSumColumns),
                _ => throw new Exception()
            });

            var count = 0;
            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await (_queryParameter.NodeQueryParameter.ViewMode switch
                {
                    RMDiscoveryNodeViewMode.Container => QueryContainerViewCount(),
                    RMDiscoveryNodeViewMode.Site => QuerySiteViewCount(),
                    RMDiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewCount(),
                    _ => throw new Exception()
                });
            }

            return new RMDiscoveryNodeDataInfo
            {
                Count = count,
                Items = items,
            };
        }

        private async Task<List<Dictionary<string, object>>> QueryContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var containersSql = $@"SELECT Id, Name, ContentSource, FileTotalSize, FileSumCount, SiteCount
FROM [{_schemaName}].[RMContainerInfoes] AS container ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                containersSql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            containersSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var containers = await _queryDao.GetDataListAsync<RMDiscoveryOffice365ContainerInfo>(containersSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            var sql = $@"SELECT 
container.Id AS id,
container.Name AS name,
container.ContentSource AS contentSource,
container.SiteCount AS siteCount,
container.FileTotalSize AS fileTotalSize,
container.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            var inClauseParamName = DatabaseUtility.BuildInClause(containers.Select(item => item.Id), out var paramList);

            sql += $@" FROM [{_schemaName}].[RMContainerInfoes] AS container
LEFT JOIN [{_schemaName}].[RMContainerInactiveData] AS data
ON container.Id = data.ContainerId
WHERE container.Id IN {inClauseParamName}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY container.Id, container.Name, container.ContentSource, container.SiteCount, container.FileTotalSize, container.FileSumCount";

            paramList.AddRange(conditionSqlDefinitions.SelectMany(item => item.Parameters));

            var items = await _queryDao.GetDataDictionaryListAsync(sql, paramList.ToArray());

            foreach (var container in containers)
            {
                if (!items.Any(item => container.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = container.Id,
                        ["name"] = container.Name,
                        ["contentSource"] = container.ContentSource,
                        ["siteCount"] = container.SiteCount,
                        ["fileTotalSize"] = container.FileTotalSize,
                        ["fileSumCount"] = container.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sitesSql = $@"SELECT Id, Url, ContentSource, FileTotalSize, FileSumCount
FROM [{_schemaName}].[RMSiteInfoes] AS site WHERE site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sitesSql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            sitesSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var sites = await _queryDao.GetDataListAsync<RMDiscoveryOffice365SiteInfo>(sitesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            var sql = $@"SELECT 
site.Id AS id,
site.Url AS url,
site.ContentSource AS contentSource,
site.FileTotalSize AS fileTotalSize,
site.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
LEFT JOIN [{_schemaName}].[RMSiteInactiveData] AS data
ON site.Id = data.SiteId
WHERE site.Id IN {DatabaseUtility.BuildInClause(sites.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY site.Id, site.Url, site.ContentSource, site.FileTotalSize, site.FileSumCount";
            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray());

            foreach (var site in sites)
            {
                if (!items.Any(item => site.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = site.Id,
                        ["url"] = site.Url,
                        ["contentSource"] = site.ContentSource,
                        ["fileTotalSize"] = site.FileTotalSize,
                        ["fileSumCount"] = site.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteInContainerViewItems(List<string> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sitesSql = $@"SELECT Id, Url, ContentSource, FileTotalSize, FileSumCount
FROM [{_schemaName}].[RMSiteInfoes] AS site WHERE site.ContainerId = @ContainerId AND site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sitesSql += " And " + searchKeySqlDefinition.ConditionSql;
            }

            sitesSql += $@"order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var sites = await _queryDao.GetDataListAsync<RMDiscoveryOffice365SiteInfo>(sitesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize),
                    new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId)
                }).ToArray());

            var sql = $@"SELECT 
site.Id AS id,
site.Url AS url,
site.ContentSource AS contentSource,
site.FileTotalSize AS fileTotalSize,
site.FileSumCount AS fileSumCount,
SUM(data.FileTotalSize) AS inactiveFileTotalSize,
SUM(data.FileSumCount) AS inactiveFileSumCount
{(needSumColumns.Any() ? "," + string.Join(",", needSumColumns.ConvertAll(item => $"SUM(data.{item}) AS {item}")) : "")}";

            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
LEFT JOIN [{_schemaName}].[RMSiteInactiveData] AS data
ON site.Id = data.SiteId
WHERE site.Id IN {DatabaseUtility.BuildInClause(sites.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            sql += " GROUP BY site.Id, site.Url, site.ContentSource, site.FileTotalSize, site.FileSumCount";
            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray());

            foreach (var site in sites)
            {
                if (!items.Any(item => site.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = site.Id,
                        ["url"] = site.Url,
                        ["contentSource"] = site.ContentSource,
                        ["fileTotalSize"] = site.FileTotalSize,
                        ["fileSumCount"] = site.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                    };

                    needSumColumns.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<int> QueryContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMContainerInfoes] AS container";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                sql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
        }

        private async Task<int> QuerySiteViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMSiteInfoes] AS site WHERE site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
        }

        private async Task<int> QuerySiteInContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMSiteInfoes] AS site
WHERE site.ContainerId = @ContainerId AND site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters
                .Concat(new List<SqlParameter> { new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId) }).ToArray());
        }

        protected List<RMDiscoverySqlDefinition> GetConditionSqlDefinitionWithoutNode(string tableAlias)
        {
            var res = new List<RMDiscoverySqlDefinition>();

            if (_queryParameter.FileExtensionQueryParameter != null &&
                _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition(tableAlias, out var sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.SizeRangeQueryParameter != null &&
                _queryParameter.SizeRangeQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.WithoutDateQueryParameter != null &&
                _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            return res;
        }
    }
}

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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot
{
    public class RMDiscoveryRotOptmizationNodeQuerier : RMDiscoveryRotDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryRotOptmizationNodeQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }


        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {

            var items = await (_queryParameter.NodeQueryParameter.ViewMode switch
            {
                RMDiscoveryNodeViewMode.Container => QueryContainerViewItems(),
                RMDiscoveryNodeViewMode.Site => QuerySiteViewItems(),
                RMDiscoveryNodeViewMode.SiteInContainer => QuerySiteInContainerViewItems(),
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

            return new()
            {
                Items = items,
                Count = count
            };
        }


        private async Task<List<Dictionary<string, object>>> QueryContainerViewItems()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var containersSql = $@"SELECT Id, Name, ContentSource, FileTotalSize, SiteCount
FROM [{_schemaName}].[RMContainerInfoes] AS container ";

            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                containersSql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            containersSql += $@" order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var containers = await _queryDao.GetDataListAsync<RMDiscoveryOffice365ContainerInfo>(containersSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            RMDiscoverySqlDefinition rotRuleSqlDefinition = null;
            var hasRotRuleDefinition = _queryParameter.ROTRuleQueryParameter != null &&
                _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("dbo", "data", out rotRuleSqlDefinition);

            var sql = $@"SELECT 
  container.Id AS id,
  container.Name AS name,
  container.ContentSource AS contentSource,
  container.SiteCount AS siteCount,
  container.FileTotalSize AS fileTotalSize,
  container.FileSumCount AS fileSumCount,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Redundant} THEN data.FileTotalsize ELSE 0 END) AS redundant,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Obsolete} THEN data.FileTotalsize ELSE 0 END) AS obsolete,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Trivial} THEN data.FileTotalsize ELSE 0 END) AS trivial
";

            sql += $@" FROM [{_schemaName}].[RMContainerInfoes] AS container
LEFT JOIN [{_schemaName}].[RMContainerRotData] AS data
ON container.Id = data.ContainerId ";

            if (hasRotRuleDefinition)
            {
                sql += rotRuleSqlDefinition.JoinOnSqls.First().FullSql;
            }

            sql += @$" WHERE container.Id IN {DatabaseUtility.BuildInClause(containers.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");

            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            if (hasRotRuleDefinition)
            {
                sql += " AND " + rotRuleSqlDefinition.ConditionSql;
            }

            sql += " GROUP BY container.Id, container.Name, container.ContentSource, container.SiteCount, container.FileTotalSize, container.FileSumCount ";

            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).Concat(rotRuleSqlDefinition?.Parameters).ToArray());

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
                        ["redundant"] = 0,
                        ["obsolete"] = 0,
                        ["trivial"] = 0,
                    };

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteViewItems()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sitesSql = $@"SELECT Id, Url, ContentSource, FileTotalSize
FROM [{_schemaName}].[RMSiteInfoes] AS site WHERE site.Hidden = 0 ";

            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sitesSql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            sitesSql += $@" order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var sites = await _queryDao.GetDataListAsync<RMDiscoveryOffice365SiteInfo>(sitesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            RMDiscoverySqlDefinition rotRuleSqlDefinition = null;
            var hasRotRuleDefinition = _queryParameter.ROTRuleQueryParameter != null &&
                _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("dbo", "data", out rotRuleSqlDefinition);

            var sql = $@"SELECT 
  site.Id AS id,
  site.Url AS url,
  site.ContentSource AS contentSource,
  site.FileTotalSize AS fileTotalSize,
  site.FileSumCount AS fileSumCount,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Redundant} THEN data.FileTotalsize ELSE 0 END) AS redundant,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Obsolete} THEN data.FileTotalsize ELSE 0 END) AS obsolete,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Trivial} THEN data.FileTotalsize ELSE 0 END) AS trivial ";

            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
LEFT JOIN [{_schemaName}].[RMSiteRotData] AS data
ON site.Id = data.siteId ";

            if (hasRotRuleDefinition)
            {
                sql += rotRuleSqlDefinition.JoinOnSqls.First().FullSql;
            }

            sql += @$" WHERE site.Id IN {DatabaseUtility.BuildInClause(sites.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");

            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            if (hasRotRuleDefinition)
            {
                sql += " AND " + rotRuleSqlDefinition.ConditionSql;
            }

            sql += " GROUP BY site.Id, site.Url, site.ContentSource, site.FileTotalSize, site.FileSumCount ";

            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).Concat(rotRuleSqlDefinition?.Parameters).ToArray());

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
                        ["redundant"] = 0,
                        ["obsolete"] = 0,
                        ["trivial"] = 0,
                    };

                    items.Add(itemDic);
                }
            }

            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteInContainerViewItems()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sitesSql = $@"SELECT Id, Url, ContentSource, FileTotalSize
FROM [{_schemaName}].[RMSiteInfoes] AS site WHERE site.ContainerId = @ContainerId AND site.Hidden = 0 ";

            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sitesSql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            sitesSql += $@" order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var sites = await _queryDao.GetDataListAsync<RMDiscoveryOffice365SiteInfo>(sitesSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize),
                    new SqlParameter("@ContainerId", nodeQueryParameter.JoinedContainerId)
                }).ToArray());

            RMDiscoverySqlDefinition rotRuleSqlDefinition = null;
            var hasRotRuleDefinition = _queryParameter.ROTRuleQueryParameter != null &&
                _queryParameter.ROTRuleQueryParameter.TryGetSqlDefinition("dbo", "data", out rotRuleSqlDefinition);

            var sql = $@"SELECT 
  site.Id AS id,
  site.Url AS url,
  site.ContentSource AS contentSource,
  site.FileTotalSize AS fileTotalSize,
  site.FileSumCount AS fileSumCount,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Redundant} THEN data.FileTotalsize ELSE 0 END) AS redundant,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Obsolete} THEN data.FileTotalsize ELSE 0 END) AS obsolete,
  SUM(case [Rule].Category WHEN {(int)RMDiscoveryRuleCategory.Trivial} THEN data.FileTotalsize ELSE 0 END) AS trivial ";

            sql += $@" FROM [{_schemaName}].[RMSiteInfoes] AS site
LEFT JOIN [{_schemaName}].[RMSiteRotData] AS data
ON site.Id = data.siteId ";

            if (hasRotRuleDefinition)
            {
                sql += rotRuleSqlDefinition.JoinOnSqls.First().FullSql;
            }

            sql += @$" WHERE site.Id IN {DatabaseUtility.BuildInClause(sites.Select(item => item.Id))}";

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");

            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
            }

            if (hasRotRuleDefinition)
            {
                sql += " AND " + rotRuleSqlDefinition.ConditionSql;
            }

            sql += " GROUP BY site.Id, site.Url, site.ContentSource, site.FileTotalSize, site.FileSumCount ";

            var items = await _queryDao.GetDataDictionaryListAsync(sql, conditionSqlDefinitions.SelectMany(item => item.Parameters).Concat(rotRuleSqlDefinition?.Parameters).ToArray());

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
                        ["redundant"] = 0,
                        ["obsolete"] = 0,
                        ["trivial"] = 0,
                    };

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

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
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Inactive
{
    public class RMDiscoveryAOSPInactiveNodeQuerier : RMDiscoveryAOSPInactiveDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryAOSPInactiveNodeQuerier(RMDiscoveryAOSPQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var inactiveRules = await _ruleInfoDao.GetRuleInfoesAsync(true, _queryParameter.O365TenantId.ToString(), RMDiscoveryRuleDefinitionKind.Inactive);
            var needSumColumnNames = inactiveRules.Select(item => item.ToCustomColumn().Name).ToList();
            var needSumColumns = inactiveRules.Select(item => item.ToCustomColumn()).ToList();

            var items = await QuerySiteViewItems(needSumColumnNames, needSumColumns);
            var count = 0;
            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await QuerySiteViewCount();
            }

            return new RMDiscoveryNodeDataInfo
            {
                Count = count,
                Items = items,
            };
        }

        private async Task<List<Dictionary<string, object>>> QuerySiteViewItems(List<string> needSumColumnNames, List<RMDiscoveryCustomColumn> needSumColumns)
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;

            var sitesSql = $@"SELECT Id, Url, SiteId, ContentSource, FileTotalSize, FileSumCount
FROM [{_schemaName}].[RMAOSPSiteInfoes] AS site WHERE site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sitesSql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            if (nodeQueryParameter.SiteUniqueIds != null && nodeQueryParameter.SiteUniqueIds.Count > 0)
            {
                sitesSql += $" AND site.SiteId IN {DatabaseUtility.BuildInClause(nodeQueryParameter.SiteUniqueIds)} ";
            }


            sitesSql += $@" order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var sites = await _queryDao.GetDataListAsync<RMDiscoveryAOSPSiteInfo>(sitesSql,
                [
                    .. searchKeySqlDefinition.Parameters,
                    .. new List<SqlParameter>
                        {
                            new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                            new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                        },
                ]);

            var sql = $@"SELECT 
site.Id AS id,
site.Url AS url,
site.SiteId AS siteId,
site.ContentSource AS contentSource,
site.FileTotalSize AS fileTotalSize,
site.FileSumCount AS fileSumCount ";

            sql += $@" FROM [{_schemaName}].[RMAOSPSiteInfoes] AS site
WHERE site.Id IN {DatabaseUtility.BuildInClause(sites.Select(item => item.Id))}";

            sql += " GROUP BY site.Id, site.Url, site.SiteId, site.ContentSource, site.FileTotalSize, site.FileSumCount";
            var items = await _queryDao.GetDataDictionaryListAsync(sql);//conditionSqlDefinitions.SelectMany(item => item.Parameters).ToArray()

            var allInactiveSites = await _dataDao.GetSiteInactiveDataBySqlConditionalExpressionAsync(
                _queryParameter.O365TenantId, sites.Select(site => site.Id).ToList(), needSumColumns).ToListAsync();

            var allRotSites = await _dataDao.GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(
                _queryParameter.O365TenantId, sites.Select(site => site.Id).ToList()).ToListAsync();

            foreach (var item in items)
            {
                var inactiveSites = allInactiveSites.Where(site => site.SiteId.ToString() == item["id"].ToString()).ToList();
                if (inactiveSites.Count > 0)
                {
                    item["inactiveFileTotalSize"] = inactiveSites.Sum(_ => _.FileTotalSize);
                    item["inactiveFileSumCount"] = inactiveSites.Sum(_ => _.FileSumCount);
                    needSumColumnNames.ForEach(column => item[column] = inactiveSites.Sum(_ => _.CustomColumns.Where(c => c.Name == column).Select(c => (long)c.Value).First())); 
                }

                var rotSites = allRotSites.Where(site => site.SiteId.ToString() == item["id"].ToString()).ToList();
                if(rotSites.Count > 0)
                {
                    item["rotFileTotalSize"] = rotSites.Sum(_ => _.FileTotalSize);
                    item["redundant"] = rotSites.Where(_ => _.Category == RMDiscoveryRuleCategory.Redundant).Sum(_ => _.FileTotalSize);
                    item["obsolete"] = rotSites.Where(_ => _.Category == RMDiscoveryRuleCategory.Obsolete).Sum(_ => _.FileTotalSize);
                    item["trivial"] = rotSites.Where(_ => _.Category == RMDiscoveryRuleCategory.Trivial).Sum(_ => _.FileTotalSize);
                }
            }

            foreach (var site in sites)
            {
                if (!items.Any(item => site.Id.ToString() == item["id"].ToString()))
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = site.Id,
                        ["url"] = site.Url,
                        ["siteId"] = site.SiteId,
                        ["contentSource"] = site.ContentSource,
                        ["fileTotalSize"] = site.FileTotalSize,
                        ["fileSumCount"] = site.FileSumCount,
                        ["inactiveFileTotalSize"] = 0,
                        ["inactiveFileSumCount"] = 0,
                        ["rotFileTotalSize"] = 0,
                        ["redundant"] = 0,
                        ["obsolete"] = 0,
                        ["trivial"] = 0,
                    };

                    needSumColumnNames.ForEach(column => itemDic[column] = 0);

                    items.Add(itemDic);
                }
            }

            RMDiscoveryAOSPSQLiteDBManager.DeleteDatabase();
            return items.OrderByDescending(item => Convert.ToInt64(item["fileTotalSize"])).ToList();
        }

        private async Task<int> QuerySiteViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMAOSPSiteInfoes] AS site WHERE site.Hidden = 0 ";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("site", out var searchKeySqlDefinition))
            {
                sql += " AND " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
        }
    }
}

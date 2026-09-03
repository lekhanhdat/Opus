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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Rot
{
    internal class RMDiscoveryGoogleRotNodeDataQuerier : RMDiscoveryGoogleRotDataQuerier<RMDiscoveryNodeDataInfo>
    {
        public RMDiscoveryGoogleRotNodeDataQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryNodeDataInfo> QueryAsync()
        {
            var tableName = "RMGoogleContainerRootLevelRotData";
            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            if (selectedRuleIds.Any())
            {
                tableName = "RMGoogleContainerRuleLevelRotData";
            }

            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var containersSql = $@"SELECT Id, Name 
FROM [{_schemaName}].[RMGoogleContainerInfoes] AS container ";

            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                containersSql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            containersSql += $@" order BY FileTotalSize DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
";
            var containers = await _queryDao.GetDataListAsync<RMDiscoveryGoogleContainerInfo>(containersSql,
                searchKeySqlDefinition.Parameters.Concat(new List<SqlParameter>
                {
                    new SqlParameter("@Offset", nodeQueryParameter.PageIndex * nodeQueryParameter.PageSize),
                    new SqlParameter("@PageSize", nodeQueryParameter.PageSize)
                }).ToArray());

            if (containers.Count == 0)
            {
                return new();
            }

            var sqlParameters = new List<SqlParameter>();

            var containerInSql = DatabaseUtility.BuildInClause(containers.Select(item => item.Id), out var containerInParameters);
            sqlParameters.AddRange(containerInParameters);

            var sql = $@"SELECT data.ContainerId AS id, SUM(data.FileTotalSize) AS rotFileTotalSize FROM [{_schemaName}].[{tableName}] AS data
WHERE data.ContainerId IN {containerInSql}";

            if (selectedRuleIds.Any())
            {
                var ruleId = selectedRuleIds.First();
                sql += $" AND [Rule] = @RuleId ";

                sqlParameters.Add(new("@RuleId", ruleId));
            }

            var conditionSqlDefinitions = GetConditionSqlDefinitionWithoutNode("data");
            if (conditionSqlDefinitions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditionSqlDefinitions.Select(item => item.ConditionSql));
                sqlParameters.AddRange(conditionSqlDefinitions.Select(item => item.Parameters).SelectMany(item => item));
            }

            sql += " GROUP BY data.ContainerId";
            var dataDic = await _queryDao.GetDataDictionaryListAsync(sql, [.. sqlParameters]);

            var items = new List<Dictionary<string, object>>();
            foreach (var container in containers)
            {
                var matchedData = dataDic.FirstOrDefault(item => item["id"].ToString() == container.Id.ToString());
                if (matchedData != null)
                {
                    items.Add(new()
                    {
                        ["id"] = container.Id,
                        ["name"] = container.Name,
                        ["rotFileTotalSize"] = matchedData["rotFileTotalSize"]
                    });
                }
                else
                {
                    var itemDic = new Dictionary<string, object>
                    {
                        ["id"] = container.Id,
                        ["name"] = container.Name,
                        ["rotFileTotalSize"] = 0,
                    };
                    items.Add(itemDic);
                }
            }

            var count = 0;

            if (_queryParameter.NodeQueryParameter.PageIndex == 0)
            {
                count = await QueryContainerViewCount();
            }

            return new()
            {
                Items = items,
                Count = count
            };
        }

        private async Task<int> QueryContainerViewCount()
        {
            var nodeQueryParameter = _queryParameter.NodeQueryParameter;
            var sql = $@"SELECT COUNT(1) FROM [{_schemaName}].[RMGoogleContainerInfoes] AS container";
            if (nodeQueryParameter.TryGetSearchKeySqlDefinition("container", out var searchKeySqlDefinition))
            {
                sql += " WHERE " + searchKeySqlDefinition.ConditionSql;
            }

            return await _queryDao.GetDataAsync<int>(sql, searchKeySqlDefinition.Parameters.ToArray());
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

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
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Extensions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Rot
{
    public class RMDiscoveryAOSPRotAggregateStatisticQuerier : RMDiscoveryAOSPRotDataQuerier<RMDiscoveryAOSPAggregateStatisticDataInfo>
    {
        public RMDiscoveryAOSPRotAggregateStatisticQuerier(RMDiscoveryAOSPQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryAOSPAggregateStatisticDataInfo> QueryAsync()
        {
            var tableName = GetTableName();
            var sql = $@"SELECT SUM(data.FileTotalSize) AS FileTotalSize, SUM(data.FileSumCount) AS FileSumCount 
FROM [{_schemaName}].[{tableName}] AS data ";

            var definitions = GetConditionSqlDefinition("data");
            var sqlParams = new List<SqlParameter>();
            if (definitions.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", definitions.Select(item => item.ConditionSql))}";
                sqlParams = definitions.Select(item => item.Parameters).SelectMany(item => item).ToList();
            }

            return await _queryDao.GetDataAsync<RMDiscoveryAOSPAggregateStatisticDataInfo>(sql, sqlParams.ToArray());
        }

        private string GetTableName()
        {
            var tableName = "RMAOSPBasicRootLevelRotData";
            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            var containerIds = _queryParameter.NodeQueryParameter.ContainerIds;
            if (selectedRuleIds.Count > 0 && containerIds.Count > 0)
            {
                return "RMAOSPContainerRuleLevelRotData";
            }

            if (selectedRuleIds.Count > 0)
            {
                return "RMAOSPBasicRuleLevelRotData";
            }

            if (containerIds.Count > 0)
            {
                return "RMAOSPContainerRootLevelRotData";
            }

            return tableName;
        }

        private List<RMDiscoverySqlDefinition> GetConditionSqlDefinition(string tableAlias)
        {
            var res = new List<RMDiscoverySqlDefinition>();

            if (_queryParameter.SizeRangeQueryParameter != null &&
                _queryParameter.SizeRangeQueryParameter.TryGetSqlDefinition(tableAlias, out var sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.FileExtensionQueryParameter != null &&
                _queryParameter.FileExtensionQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            if (_queryParameter.WithoutDateQueryParameter != null &&
                _queryParameter.WithoutDateQueryParameter.TryGetSqlDefinition(tableAlias, out sqlDefinition))
            {
                res.Add(sqlDefinition);
            }

            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            if (selectedRuleIds.Any())
            {
                res.Add(new RMDiscoverySqlDefinition
                {
                    ConditionSql = $"{tableAlias}.[Rule] = @RuleId",
                    Parameters = new List<SqlParameter>
                    {
                        new("@RuleId", selectedRuleIds.First())
                    }
                });
            }

            if (_queryParameter.NodeQueryParameter.ContainerIds.Count > 0)
            {
                var containerInSql = DatabaseUtility.BuildInClause(_queryParameter.NodeQueryParameter.ContainerIds, out var containerInParameters);
                res.Add(new RMDiscoverySqlDefinition
                {
                    ConditionSql = $"{tableAlias}.ContainerId IN {containerInSql}",
                    Parameters = containerInParameters
                });
            }

            return res;
        }
    }
}

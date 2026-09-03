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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Extensions;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Query.General.Rot;

namespace AvePoint.RA.Service.Services.Discovery.FS.Query.General.Rot
{
    public class RMDiscoveryFSRotNodeTotalAggregateDataQuerier : RMDiscoveryFSRotDataQuerier<Dictionary<string, object>>
    {
        public RMDiscoveryFSRotNodeTotalAggregateDataQuerier(RMDiscoveryFSQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<Dictionary<string, object>> QueryAsync()
        {
            var tableName = "RMFSBasicRootLevelRotData";
            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            if (selectedRuleIds.Any())
            {
                tableName = "RMFSBasicRuleLevelRotData";
            }
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var sql = $@"SELECT SUM(FileTotalSize) AS rotFileTotalSize FROM [{_schemaName}].[{tableName}] AS data";
            var definitions = GetConditionSqlDefinitionWithoutNode("data");
            var sqlParams = new List<SqlParameter>();
            if (definitions.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", definitions.Select(item => item.ConditionSql))}";
                sqlParams = definitions.Select(item => item.Parameters).SelectMany(item => item).ToList();
            }

            var list = await _queryDao.GetDataDictionaryListAsync(sql, sqlParams.ToArray());
            return list.FirstOrDefault() ?? new();
        }

        private List<RMDiscoverySqlDefinition> GetConditionSqlDefinitionWithoutNode(string tableAlias)
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

            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            if (selectedRuleIds.Any())
            {
                res.Add(new RMDiscoverySqlDefinition
                {
                    ConditionSql = "[Rule] = @RuleId",
                    Parameters = new List<SqlParameter>
                    {
                        new("@RuleId", selectedRuleIds.First())
                    }
                });
            }

            return res;
        }
    }
}

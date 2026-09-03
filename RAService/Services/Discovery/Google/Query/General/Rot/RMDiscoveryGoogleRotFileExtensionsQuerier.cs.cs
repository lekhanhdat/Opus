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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Rot
{
    public class RMDiscoveryGoogleRotFileExtensionsQuerier : RMDiscoveryGoogleRotDataQuerier<List<RMDiscoveryFileExtensionDataInfo>>
    {
        public RMDiscoveryGoogleRotFileExtensionsQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryAsync()
        {
            var tableName = GetTableName();
            var sql =
$@"SELECT fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize 
FROM [{_schemaName}].[{tableName}] AS data 
JOIN [{_schemaName}].[RMGoogleFileExtensions] AS fileType ON data.FileExtension = fileType.Id ";

            var definitions = GetConditionSqlDefinition("data");
            var sqlParams = new List<SqlParameter>();
            if (definitions.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", definitions.Select(item => item.ConditionSql))}";
                sqlParams = definitions.Select(item => item.Parameters).SelectMany(item => item).ToList();
            }

            sql += $" GROUP BY fileType.Id, fileType.Name";
            var dataList = await _queryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql, [.. sqlParams]);
            dataList.ForEach(item => item.Name = I18NEntity.GetString(item.Name));
            return [.. dataList.OrderByDescending(item => item.FileTotalSize)];
        }


        private string GetTableName()
        {
            var tableName = "RMGoogleBasicRootLevelRotData";
            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            var containerIds = _queryParameter.NodeQueryParameter.ContainerIds;
            if (selectedRuleIds.Count > 0 && containerIds.Count > 0)
            {
                return "RMGoogleContainerRuleLevelRotData";
            }

            if (selectedRuleIds.Count > 0)
            {
                return "RMGoogleBasicRuleLevelRotData";
            }

            if (containerIds.Count > 0)
            {
                return "RMGoogleContainerRootLevelRotData";
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

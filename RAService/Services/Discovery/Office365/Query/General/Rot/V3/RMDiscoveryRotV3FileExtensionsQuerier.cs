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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Query.General.Rot.V3
{
    public class RMDiscoveryRotV3FileExtensionsQuerier : RMDiscoveryRotV3DataQuerier<List<RMDiscoveryFileExtensionDataInfo>>
    {
        public RMDiscoveryRotV3FileExtensionsQuerier(RMDiscoveryOffice365QueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryAsync()
        {
            using var performance = new PerformanceScope("QueryRotV3FileExtensionDataAsync");
            var tableName = GetTableName();
            var sql =
$@"SELECT TOP 20 fileType.Id AS Id, fileType.Name AS Name, SUM(data.FileTotalSize) AS FileTotalSize 
FROM [{_schemaName}].[{tableName}] AS data 
JOIN [{_schemaName}].[RMFileExtensions] AS fileType ON data.FileExtension = fileType.Id ";

            var definitions = GetConditionSqlDefinition("data");
            var sqlParams = new List<SqlParameter>();
            if (definitions.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", definitions.Select(item => item.ConditionSql))}";
                sqlParams = definitions.Select(item => item.Parameters).SelectMany(item => item).ToList();
            }

            sql += $" GROUP BY fileType.Id, fileType.Name ORDER BY FileTotalSize DESC";
            var dataList = await _queryDao.GetDataListAsync<RMDiscoveryFileExtensionDataInfo>(sql, [.. sqlParams]);
            dataList.ForEach(item => item.Name = I18NEntity.GetString(item.Name));
            return dataList;
        }

        private string GetTableName()
        {
            var tableName = "RMBasicRootLevelRotData";
            var selectedRuleIds = _queryParameter.ROTRuleQueryParameter.RuleCategories.Select(item => item.RuleIds).SelectMany(item => item).ToList();
            var containerIds = _queryParameter.NodeQueryParameter.ContainerIds;
            if (selectedRuleIds.Count > 0 && containerIds.Count > 0)
            {
                return "RMContainerRuleLevelRotData";
            }

            if (selectedRuleIds.Count > 0)
            {
                return "RMBasicRuleLevelRotData";
            }

            if (containerIds.Count > 0)
            {
                return "RMContainerRootLevelRotData";
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

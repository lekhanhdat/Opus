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
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Rot
{
    public class RMDiscoveryGoogleRotRuleDataQuerier : RMDiscoveryGoogleRotDataQuerier<RMDiscoveryRotRuleDataInfo>
    {
        public RMDiscoveryGoogleRotRuleDataQuerier(RMDiscoveryGoogleQueryParameter queryParameter) : base(queryParameter)
        {
        }

        public override async Task<RMDiscoveryRotRuleDataInfo> QueryAsync()
        {
            var ruleInfoes = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
            ruleInfoes = ruleInfoes.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();

            var rootLevelDataSize = await QueryRootLevelDataAsync();
            var categoryLevelDataSizeDic = await QueryCategoryLevelDataAsync();
            var ruleLevelDataSizeDic = await QueryRuleLevelDataAsync();

            var res = new RMDiscoveryRotRuleDataInfo
            {
                Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_RootNode"),
                FileTotalSize = rootLevelDataSize,
                Expand = true
            };

            var ruleCategories = ruleInfoes.Select(item => item.Category).ToHashSet();
            foreach (var ruleCategory in ruleCategories)
            {
                var underCategoryRuleInfoes = ruleInfoes.Where(item => item.Category == ruleCategory).ToDictionary(item => item.Id, item => item.Name);
                var categoryRuleDataInfo = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString(GetCategoryDisplayName(ruleCategory)),
                    FileTotalSize = categoryLevelDataSizeDic.ContainsKey((int)ruleCategory) ? categoryLevelDataSizeDic[(int)ruleCategory] : 0L,
                    Category = ruleCategory,
                    Expand = true,
                    Children = []
                };

                foreach (var underCategoryRuleEntry in underCategoryRuleInfoes)
                {
                    categoryRuleDataInfo.Children.Add(new()
                    {
                        Id = underCategoryRuleEntry.Key,
                        Label = underCategoryRuleEntry.Value,
                        FileTotalSize = ruleLevelDataSizeDic.ContainsKey(underCategoryRuleEntry.Key) ? ruleLevelDataSizeDic[underCategoryRuleEntry.Key] : 0L,
                        Category = ruleCategory
                    });
                }

                res.Children.Add(categoryRuleDataInfo);
            }

            return res;
        }

        private async Task<Dictionary<int, long>> QueryRuleLevelDataAsync()
        {
            var res = new Dictionary<int, long>();
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var tableName = "RMGoogleBasicRuleLevelRotData";
            if (_queryParameter.NodeQueryParameter.ContainerIds.Count > 0)
            {
                tableName = "RMGoogleContainerRuleLevelRotData";
            }

            var sourceSql = $"SELECT data.[Rule] AS ruleId, SUM(data.FileTotalSize) AS fileTotalSize FROM [{_schemaName}].[{tableName}] AS data";
            var (sql, parameters) = BuildSqlAndParameters(sourceSql);
            sql += " GROUP BY data.[Rule]";
            var dataDicList = await _queryDao.GetDataDictionaryListAsync(sql, [.. parameters]);

            foreach (var dataDic in dataDicList)
            {
                res[Convert.ToInt32(dataDic["ruleId"])] = Convert.ToInt64(dataDic["fileTotalSize"]);
            }

            return res;
        }

        private async Task<Dictionary<int, long>> QueryCategoryLevelDataAsync()
        {
            var res = new Dictionary<int, long>();
            SecurityUtils.SanitizeSQLSchemaName(_schemaName);
            var tableName = "RMGoogleBasicCategoryLevelRotData";
            if (_queryParameter.NodeQueryParameter.ContainerIds.Count > 0)
            {
                tableName = "RMGoogleContainerCategoryLevelRotData";
            }

            var sourceSql = $"SELECT data.[Category] AS category, SUM(data.FileTotalSize) AS fileTotalSize FROM [{_schemaName}].[{tableName}] AS data";
            var (sql, parameters) = BuildSqlAndParameters(sourceSql);
            sql += " GROUP BY data.[Category]";
            var dataDicList = await _queryDao.GetDataDictionaryListAsync(sql, [.. parameters]);

            foreach (var dataDic in dataDicList)
            {
                res[Convert.ToInt32(dataDic["category"])] = Convert.ToInt64(dataDic["fileTotalSize"]);
            }

            return res;
        }

        private async Task<long> QueryRootLevelDataAsync()
        {
            var tableName = "RMGoogleBasicRootLevelRotData";
            if (_queryParameter.NodeQueryParameter.ContainerIds.Count > 0)
            {
                tableName = "RMGoogleContainerRootLevelRotData";
            }

            var sourceSql = $"SELECT SUM(data.FileTotalSize) AS fileTotalSize FROM [{_schemaName}].[{tableName}] AS data";
            var (sql, parameters) = BuildSqlAndParameters(sourceSql);
            return await _queryDao.GetDataAsync<long>(sql, [.. parameters]);
        }

        private (string sql, List<SqlParameter> parameters) BuildSqlAndParameters(string sql)
        {
            var definitions = GetConditionSqlDefinition("data");
            var sqlParams = new List<SqlParameter>();
            if (definitions.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", definitions.Select(item => item.ConditionSql))}";
                sqlParams = definitions.Select(item => item.Parameters).SelectMany(item => item).ToList();
            }

            return (sql, sqlParams);
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

        private string GetCategoryDisplayName(RMDiscoveryRuleCategory category)
        {
            return category switch
            {
                RMDiscoveryRuleCategory.Redundant => "RM_FA_ROTRule_TreeNode_Redundant",
                RMDiscoveryRuleCategory.Obsolete => "RM_FA_ROTRule_TreeNode_Obsolete",
                RMDiscoveryRuleCategory.Trivial => "RM_FA_ROTRule_TreeNode_Trivial",
                _ => "",
            };
        }
    }

}

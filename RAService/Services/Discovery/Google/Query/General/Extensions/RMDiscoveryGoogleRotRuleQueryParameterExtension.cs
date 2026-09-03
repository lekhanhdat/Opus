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
using System.Linq;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;

namespace AvePoint.RA.Service.Services.Discovery.Google.Query.General.Extensions
{
    public static class RMDiscoveryGoogleRotRuleQueryParameterExtension
    {
        public static bool TryGetSqlDefinition(this RMDiscoveryOffice365ROTRuleQueryParameter parameter, string dbSchemaName, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = new();
            if (parameter == null || parameter.RuleCategories == null || parameter.RuleCategories.Count == 0)
            {
                return false;
            }

            sqlDefinition = new RMDiscoverySqlDefinition();

            var queryCategories = parameter.RuleCategories
                .Where(c => c.RuleIds == null || !c.RuleIds.Any())
                .Select(i => i.RuleCategory.ToString());

            var queryRules = parameter.RuleCategories
                .Where(c => c.RuleIds != null && c.RuleIds.Any())
                .SelectMany(i => i.RuleIds)
                .Select(r => r.ToString());

            var ruleTableName = "RMGoogleRuleInfo";
            var ruleTableAlias = "rule";
            sqlDefinition.JoinOnSqls = new List<RMDiscoveryJoinOnSqlDefinition>()
            {
                new RMDiscoveryJoinOnSqlDefinition()
                {
                    FullSql = $"JOIN [{dbSchemaName}].[{ruleTableName}] AS [{ruleTableAlias}] ON [{dataTableAlias}].[Rule] = [{ruleTableAlias}].Id ",
                    TableName = ruleTableName,
                    TableAlias = ruleTableAlias,
                    JoinToTableAlias = dataTableAlias
                }
            };

            // use Category Query, Join RMRuleInfo table
            if (queryRules.Any())
            {
                string sql = queryRules.BuildFindInSqlCondition("Rule", $"[{dataTableAlias}].[Rule]", 3, out var sqlParameters);
                sqlDefinition.Parameters = sqlParameters;
                sqlDefinition.ConditionSql = sql;
            }
            else
            {
                string sql = queryCategories.BuildFindInSqlCondition("Category", $"[{ruleTableAlias}].Category", 3, out var sqlParameters);
                sqlDefinition.Parameters = sqlParameters;
                sqlDefinition.ConditionSql = sql;
            }

            return true;
        }
    }
}

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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.Parameter
{
    public class DiscoveryROTRuleQueryParameter
    {
        [JsonProperty("rotRuleIds")]
        public List<int> ROTRuleIds { get; set; }

        [JsonProperty("ruleCategory")]
        public int RuleCategory { get; set; }

        public (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSqlDefinition(string tableAlias)
        {

            if ((ROTRuleIds == null || !ROTRuleIds.Any()))
            {

                if(RuleCategory != 0)
                {
                    var categorySql = $"{tableAlias}.[Category] = {RuleCategory}";
                    return (true, new DiscoverySqlDefinition { Sql = categorySql });
                }
                return (false, new());
            }

            if (ROTRuleIds.Count <= 3)
            {
                var parameters = new List<SqlParameter>();
                var conditions = new List<string>();
                for (var i = 0; i < ROTRuleIds.Count; i++)
                {
                    var placeholder = "@Id" + i;
                    conditions.Add($"{tableAlias}.Id = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, ROTRuleIds[i]));
                }

                if(RuleCategory != 0)
                {
                    var placeholder = "@Category1" ;
                    conditions.Add($"{tableAlias}.Category = {placeholder}");
                    parameters.Add(new SqlParameter(placeholder, RuleCategory));
                }

                return (true, new DiscoverySqlDefinition
                {
                    Sql = $"({string.Join(" AND ", conditions)})",
                    Parameters = parameters,
                });
            }
            var categoryAndIdSql = string.Empty;
            if(RuleCategory != 0)
            {
                categoryAndIdSql = @$" AND {tableAlias}.Category = {RuleCategory}";
            }
            var sql = $"{tableAlias}.Id IN {DatabaseUtility.BuildInClause(ROTRuleIds)}" + categoryAndIdSql;
            return (true, new DiscoverySqlDefinition { Sql = sql });
        }
    }
}

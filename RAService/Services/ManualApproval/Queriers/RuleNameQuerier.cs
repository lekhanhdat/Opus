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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class RuleNameQuerier : IFilterWithHistory, ISorter, IDefaultValue
    {

        private static IRMRuleDao RuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public ManualApprovalOrderOptions OrderOption => ManualApprovalOrderOptions.RuleName;

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.RuleName;

        public ManualApprovalDefaultOptions DefaultValueOption => ManualApprovalDefaultOptions.RuleName;

        public Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var ruleNames = JsonConvert.DeserializeObject<List<string>>(value);
            return System.Threading.Tasks.Task.FromResult<Expression<Func<ManualApprovalRecord, bool>>>((record) => ruleNames.Contains(record.ManualRuleName));
        }

        public System.Linq.Expressions.Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression()
        {
            return (record) => record.ManualRuleName;
        }

        public async Task<object> GetDefaultValueAsync()
        {
            return TenantLocalValue.RequesterType switch
            {
                RequesterTypeEnum.OpusControlPlus => await GetControlPlusRuleNameAsync(),
                _ => GetAvailableRecordRuleNames()
            };
        }
        
        public List<string> GetAvailableRecordRuleNames()
        {
            return RuleDao.GetRecordsAvailableRules().ConvertAll(item => item.RuleName);
        }

        public async Task<object> GetControlPlusRuleNameAsync()
        {
            var rules = await RuleDao.GetGoogleRulesWithoutRemovedAsync();
            var ruleNames = rules.ConvertAll(item => item.RuleName);
            return ruleNames;
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var ruleNames = JsonConvert.DeserializeObject<List<string>>(value);
            var result = new ManualApprovalSqlDefintion();
            var sql = $"RuleName IN {DatabaseUtility.BuildInClause(ruleNames)}";
            result.Sql = sql;

            return result;
        }
    }
}

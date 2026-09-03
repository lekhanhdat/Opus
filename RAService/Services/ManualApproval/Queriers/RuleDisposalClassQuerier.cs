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
using AvePoint.RA.I18N.Core;
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
    public class RuleDisposalClassQuerier : IFilterWithHistory, IDefaultValue
    {
        private static IRMRuleDao RuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.RuleDisposalClass;

        public ManualApprovalDefaultOptions DefaultValueOption => ManualApprovalDefaultOptions.RuleDisposalClass;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var filterDisposalClass = JsonConvert.DeserializeObject<List<string>>(value);
            if (filterDisposalClass.Contains(I18NEntity.GetString("RM_JS_DN_DisposalNull")))
            {
                return (record) => filterDisposalClass.Contains(record.ManualRuleDisposalClass) || record.ManualRuleDisposalClass == string.Empty || record.ManualRuleDisposalClass == null;
            }
            else 
            {
                return (record) => filterDisposalClass.Contains(record.ManualRuleDisposalClass);
            }
            
        }

        public async Task<object> GetDefaultValueAsync()
        {
            return TenantLocalValue.RequesterType switch
            {
                RequesterTypeEnum.OpusControlPlus => await GetControlPlusDisposalClassesAsync(),
                _ => await GetRMDisposalClassesAsync()
            };
        }
        
        private async Task<object> GetRMDisposalClassesAsync()
        {
            var rules = await RuleDao.GetRulesWithoutRemovedAsync();
            var disposalClass = rules.ConvertAll(item => item.DisposalClass);
            disposalClass = disposalClass.Where(item => !string.IsNullOrEmpty(item)).Distinct().ToList();
            disposalClass.Add(I18NEntity.GetString("RM_JS_DN_DisposalNull"));
            return disposalClass;
        }

        private async Task<object> GetControlPlusDisposalClassesAsync()
        {
            var rules = await RuleDao.GetGoogleRulesWithoutRemovedAsync();
            var disposalClass = rules.ConvertAll(item => item.DisposalClass);
            disposalClass = disposalClass.Where(item => !string.IsNullOrEmpty(item)).Distinct().ToList();
            disposalClass.Add(I18NEntity.GetString("RM_JS_DN_DisposalNull"));
            return disposalClass;
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var filterDisposalClass = JsonConvert.DeserializeObject<List<string>>(value);
            var result = new ManualApprovalSqlDefintion();
            var sql = $"RuleDisposalClass IN {DatabaseUtility.BuildInClause(filterDisposalClass)}";
            result.Sql = sql;
            return result;
        }
    }
}

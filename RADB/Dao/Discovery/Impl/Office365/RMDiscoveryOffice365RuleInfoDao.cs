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
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365RuleInfoDao : IRMDiscoveryOffice365RuleInfoDao
    {
        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleCategory[] categories)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && (!categories.Any() || Enumerable.Contains(categories, item.Category)) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && !item.IsRemoved && (!methods.Any() || Enumerable.Contains(methods, item.AnalyseMethod))).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesByCategoriesAsync(bool enabled, List<int> ruleCategories, RMDiscoveryRuleDefinitionKind kind)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && kind == item.DefinitionKind && !item.IsRemoved && ruleCategories.Contains((int)item.Category)).ToListAsync();
        }

        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryOffice365RuleInfo> updateRuleInfo, RMDiscoveryDBEFContext context)
        {
            context.Office365RuleInfoes.AddOrUpdate(updateRuleInfo.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetByIdsAsync(params int[] ruleIds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => Enumerable.Contains(ruleIds, item.Id)).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365RuleInfo>> GetRuleInfoesAsyncOrderByCategory(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved)
                                                    .Where(r => r.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument)
                                                    .OrderBy(item => item.Category)
                                                    .ThenBy(item => item.Order)
                                                    .ToListAsync();
        }

        public async Task<bool> CheckExistingRuleByAnalyzeMethodsAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.Office365RuleInfoes.Where(item => item.IsEnable == enabled && !item.IsRemoved && Enumerable.Contains(methods, item.AnalyseMethod)).AnyAsync();
        }
    }
}

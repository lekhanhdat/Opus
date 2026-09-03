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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPRuleInfoDao : IRMDiscoveryAOSPRuleInfoDao
    {
        public async Task<List<RMDiscoveryAOSPRuleInfo>> GetRuleInfoesAsync(string o365TenantId, params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.AOSPRuleInfoes.Where(item => Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved && item.O365TenantId == o365TenantId).ToListAsync();
        }

        public async Task<List<RMDiscoveryAOSPRuleInfo>> GetRuleInfoesAsync(bool enabled, string o365TenantId, params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.AOSPRuleInfoes.Where(item => item.IsEnable == enabled && Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved && item.O365TenantId == o365TenantId).ToListAsync();
        }

        public async Task<List<RMDiscoveryAOSPRuleInfo>> GetRuleInfoesAsync(bool enabled, string o365TenantId, params RMDiscoveryRuleAnalyseMethod[] methods)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.AOSPRuleInfoes.Where(item => item.IsEnable == enabled && !item.IsRemoved && item.O365TenantId == o365TenantId && (!methods.Any() || Enumerable.Contains(methods, item.AnalyseMethod))).ToListAsync();
        }

        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryAOSPRuleInfo> updateRuleInfo, RMDiscoveryDBEFContext context)
        {
            context.AOSPRuleInfoes.AddOrUpdate(updateRuleInfo.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task DeleteRuleInfoByO365TenantIdAsync(RMDiscoveryDBEFContext context, string O365TenantId)
        {
            context.AOSPRuleInfoes.RemoveRange(context.AOSPRuleInfoes.Where(item => item.O365TenantId == O365TenantId).ToArray());
            await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryAOSPRuleInfo>> GetByIdsAsync(params int[] ruleIds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.AOSPRuleInfoes.Where(item => Enumerable.Contains(ruleIds, item.Id)).ToListAsync();
        }

        public async Task<List<RMDiscoveryAOSPRuleInfo>> GetRuleInfoesByCategoriesAsync(bool enabled, string o365TenantId, List<int> ruleCategories, RMDiscoveryRuleDefinitionKind kind)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.AOSPRuleInfoes.Where(item => item.IsEnable == enabled && kind == item.DefinitionKind && item.O365TenantId == o365TenantId && !item.IsRemoved && ruleCategories.Contains((int)item.Category)).ToListAsync();
        }
    }
}

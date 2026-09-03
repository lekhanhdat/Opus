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
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSRuleInfoDao : IRMDiscoveryFSRuleInfoDao
    {
        public async Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleDefinitionKind[] kinds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => item.IsEnable == enabled && Enumerable.Contains(kinds, item.DefinitionKind) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleCategory[] categories)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => item.IsEnable == enabled && (!categories.Any() || Enumerable.Contains(categories, item.Category)) && !item.IsRemoved).ToListAsync();
        }

        public async Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesAsync(bool enabled, params RMDiscoveryRuleAnalyseMethod[] methods)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => item.IsEnable == enabled && !item.IsRemoved && (!methods.Any() || Enumerable.Contains(methods, item.AnalyseMethod))).ToListAsync();
        }

        public async Task<List<RMDiscoveryFSRuleInfo>> GetRuleInfoesByCategoriesAsync(bool enabled, List<int> ruleCategories, RMDiscoveryRuleDefinitionKind kind)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => item.IsEnable == enabled && kind == item.DefinitionKind && !item.IsRemoved && ruleCategories.Contains((int)item.Category)).ToListAsync();
        }

        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryFSRuleInfo> updateRuleInfo, RMDiscoveryDBEFContext context)
        {
            context.FSRuleInfoes.AddOrUpdate(updateRuleInfo.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryFSRuleInfo>> GetByIdsAsync(params int[] ruleIds)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            return await context.FSRuleInfoes.Where(item => Enumerable.Contains(ruleIds, item.Id)).ToListAsync();
        }
    }

}

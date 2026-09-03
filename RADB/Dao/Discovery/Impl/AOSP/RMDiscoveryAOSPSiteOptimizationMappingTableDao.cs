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
    public class RMDiscoveryAOSPSiteOptimizationMappingTableDao : IRMDiscoveryAOSPSiteOptimizationMappingTableDao
    {
        public async Task<List<RMDiscoveryAOSPSiteOptimizationMappingInfo>> GetAllMappingInfoBySettingIdsAsync(Guid O365TenantId, IEnumerable<Guid> settingIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(O365TenantId);
            var ids = settingIds.ToHashSet();
            return await efContext.AOSPSiteOptimizationMappingInfos.Where(item => ids.Contains(item.SettingId)).ToListAsync();
        }


        public async Task<int> AddOrUpdateAsync(List<RMDiscoveryAOSPSiteOptimizationMappingInfo> updateRuleInfos, Guid O365TenantId)
        {
            using var context = await RMDiscoveryDBManager.GetAOSPEFContextAsync(O365TenantId);
            context.AOSPSiteOptimizationMappingInfos.AddOrUpdate(updateRuleInfos.ToArray());
            return await context.SaveChangesAsync();
        }

        public async Task removeMappingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId)
        {
            var mappings = context.AOSPSiteOptimizationMappingInfos.Where(item => item.SettingId == settingId);
            foreach (var mapping in mappings)
            {
                context.AOSPSiteOptimizationMappingInfos.Remove(mapping);
            }
            await context.SaveChangesAsync();
        }
    }
}

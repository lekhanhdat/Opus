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
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile
{
    public class RMDiscoveryPlanSiteMappingDao : IRMDiscoveryPlanSiteMappingDao
    {
        public async Task<List<string>> GetNodeIdsByPlanProfileIdAsync(int planProfileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            return await efContext.Set<RMDiscoveryPlanSiteMapping>()
                                  .AsNoTracking()
                                  .Where(x => x.PlanProfileId == planProfileId)
                                  .Select(x => x.NodeId) 
                                  .ToListAsync();
        }

        public async Task<int> GetSiteMappingTypeAsync(int planProfileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var mapping = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.PlanProfileId == planProfileId);

            return mapping != null ? (int)mapping.Type : 0;
        }
        public async Task<int> GetTotalMappingSitesAsync(int planProfileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            return await efContext.Set<RMDiscoveryPlanSiteMapping>()
                                  .AsNoTracking()
                                  .CountAsync(x => x.PlanProfileId == planProfileId);
        }

        public async Task DeleteByPlanProfileIdAsync(int planProfileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var oldMappings = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                                             .Where(x => x.PlanProfileId == planProfileId)
                                             .ToListAsync();

            if (oldMappings.Any())
            {
                efContext.Set<RMDiscoveryPlanSiteMapping>().RemoveRange(oldMappings);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByPlanProfileIdsAsync(List<int> planProfileIds)
        {
            if (planProfileIds == null || !planProfileIds.Any()) return;

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var oldMappings = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                                             .Where(x => planProfileIds.Contains(x.PlanProfileId))
                                             .ToListAsync();

            if (oldMappings.Any())
            {
                efContext.Set<RMDiscoveryPlanSiteMapping>().RemoveRange(oldMappings);
                await efContext.SaveChangesAsync();
            }
        }

        public async Task InsertMappingsAsync(int planProfileId, List<string> nodeIds, RMDiscoveryPlanSiteType siteType = RMDiscoveryPlanSiteType.None)
        {
            if (nodeIds == null || !nodeIds.Any()) return;

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var newMappings = nodeIds.Select(nodeId => new RMDiscoveryPlanSiteMapping
            {
                PlanProfileId = planProfileId,
                NodeId = nodeId,
                Type = siteType 
            }).ToList();

            efContext.Set<RMDiscoveryPlanSiteMapping>().AddRange(newMappings);
            await efContext.SaveChangesAsync();
        }

        public async Task UpdateMappingsAsync(int planProfileId, List<SiteMappingRequest> siteMappings)
        {
            if (siteMappings == null || !siteMappings.Any()) return;

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var sitesToAdd = siteMappings.Where(x => x.IsAdd).Select(x => x.SiteId).ToList();
            var sitesToRemove = siteMappings.Where(x => !x.IsAdd).Select(x => x.SiteId).ToList();

            bool isChanged = false;

            if (sitesToRemove.Any())
            {
                var oldMappings = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                    .Where(x => x.PlanProfileId == planProfileId && sitesToRemove.Contains(x.NodeId))
                    .ToListAsync();

                if (oldMappings.Any())
                {
                    efContext.Set<RMDiscoveryPlanSiteMapping>().RemoveRange(oldMappings);
                    isChanged = true;
                }
            }

            if (sitesToAdd.Any())
            {
                var existingNodes = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                    .Where(x => x.PlanProfileId == planProfileId && sitesToAdd.Contains(x.NodeId))
                    .Select(x => x.NodeId)
                    .ToListAsync();

                var newNodes = sitesToAdd.Except(existingNodes).ToList();

                if (newNodes.Any())
                {
                    var newMappings = newNodes.Select(nodeId => new RMDiscoveryPlanSiteMapping
                    {
                        PlanProfileId = planProfileId,
                        NodeId = nodeId,
                        Type = RMDiscoveryPlanSiteType.None
                    }).ToList();

                    efContext.Set<RMDiscoveryPlanSiteMapping>().AddRange(newMappings);
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                await efContext.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetNodeIdsByProfileId(int planProfileId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var nodeIds = new List<string>();

            nodeIds = await efContext.Set<RMDiscoveryPlanSiteMapping>()
                             .AsNoTracking()
                             .Where(x => x.PlanProfileId == planProfileId)
                             .Select(x => x.NodeId)
                             .ToListAsync();

            return nodeIds;
        }
    }
}
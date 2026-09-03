﻿/********************************************************************
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
﻿using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile
{
    public class RMDiscoveryPlanProfileDao : IRMDiscoveryPlanProfileDao
    {
        private static readonly Dictionary<SourceFlag, List<int>> S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL = new()
        {
            {SourceFlag.SharePoint, [(int)NodeLevel.WebApplication, (int)NodeLevel.O365GroupSitesGroup]},
            {SourceFlag.OneDrive, [(int)NodeLevel.SkyDriveProGroup]},
        };

        private static readonly Dictionary<SourceFlag, List<int>> S_CONTENT_SOURCE_DISABLE_TEAMS_AVAILABLE_CONTAINER_LEVEL = new()
        {
            {SourceFlag.SharePoint, [(int)NodeLevel.PrivateChannelGroup]},
            {SourceFlag.OneDrive, []},
        };
        public async Task<RMDiscoveryPlanProfile> GetByIdAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Set<RMDiscoveryPlanProfile>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(int TotalCount, List<RMDiscoveryPlanProfile> Items)> GetPagedAsync(RMDiscoveryPlanProfilePageRequest request)
        {
            int pageSize = request.PageSize < 1 ? 50 : request.PageSize;
            int skip = Math.Max(0, request.PageIndex - 1) * pageSize;

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            IQueryable<RMDiscoveryPlanProfile> query = efContext.Set<RMDiscoveryPlanProfile>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchName))
            {
                string keyword = request.SearchName.Trim();
                query = query.Where(x => x.Name.Contains(keyword));
            }

            int totalCount = await query.CountAsync();

            bool isDesc = request.IsDesc;

            query = request.SortBy?.ToLower() switch
            {
                "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "action" => isDesc ? query.OrderByDescending(x => x.Action) : query.OrderBy(x => x.Action),
                "scope" => isDesc
                    ? query.OrderByDescending(x => efContext.Set<RMDiscoveryPlanSiteMapping>().Count(m => m.PlanProfileId == x.Id))
                    : query.OrderBy(x => efContext.Set<RMDiscoveryPlanSiteMapping>().Count(m => m.PlanProfileId == x.Id)),
                "rule" => isDesc ? query.OrderByDescending(x => x.Rules) : query.OrderBy(x => x.Rules),
                _ => isDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
            };

            var items = await query.Skip(skip).Take(pageSize).ToListAsync();

            return (totalCount, items);
        }

        public async Task<bool> ExistsByNameAsync(string name, int excludeId = 0)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Set<RMDiscoveryPlanProfile>()
                                  .AsNoTracking()
                                  .AnyAsync(x => x.Name == name && x.Id != excludeId);
        }

        public async Task<int> InsertAsync(RMDiscoveryPlanProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            using var transaction = efContext.Database.BeginTransaction();
            try
            {
                efContext.Set<RMDiscoveryPlanProfile>().Add(profile);
                await efContext.SaveChangesAsync();

                transaction.Commit();
                return profile.Id;
            }
            catch
            {
                transaction.Rollback();
                efContext.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RMDiscoveryPlanProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            using var transaction = efContext.Database.BeginTransaction();
            try
            {
                var existProfile = await efContext.Set<RMDiscoveryPlanProfile>().FirstOrDefaultAsync(x => x.Id == profile.Id);
                if (existProfile == null) return false;

                existProfile.Name = profile.Name;
                existProfile.Rules = profile.Rules;
                existProfile.Action = profile.Action;
                existProfile.ActionOptions = profile.ActionOptions;
                existProfile.PreviousVersion = profile.PreviousVersion;
                existProfile.StorageLocationId = profile.StorageLocationId;
                existProfile.StubSettingId = profile.StubSettingId;

                await efContext.SaveChangesAsync();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                efContext.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            using var transaction = efContext.Database.BeginTransaction();
            try
            {
                var profile = await efContext.Set<RMDiscoveryPlanProfile>().FirstOrDefaultAsync(x => x.Id == id);
                if (profile == null) return false;

                efContext.Set<RMDiscoveryPlanProfile>().Remove(profile);

                var result = await efContext.SaveChangesAsync() > 0;
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<RMRemoteNode>> GetOpusContainersAsync(IEnumerable<Guid> ids)
        {
            var containerIds = ids.ConvertAll(item => item.ToString()).ToList();
            using var context = RMDBContextManager.GetNewDBContext();
            return (await context.RMRemoteNodes.Where(item => containerIds.Contains(item.Id))
                .Select(item => new
                {
                    item.Id,
                    item.Url,
                    item.NodeLevel
                }).ToListAsync()).ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Url,
                    NodeLevel = item.NodeLevel
                });
        }

        public async Task<List<RMRemoteNode>> GetOpusContainersAsync(params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive };
            var availableNodeLevels = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL[item]).SelectMany(item => item).ToList();
            if (!await IsTeamsAvailableAsync())
            {
                var disableTeamsAvaliableNodeLevels = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_DISABLE_TEAMS_AVAILABLE_CONTAINER_LEVEL[item]).SelectMany(item => item).ToList();
                availableNodeLevels.AddRange(disableTeamsAvaliableNodeLevels);
            }
            using var context = RMDBContextManager.GetNewDBContext();
            return (await context.RMRemoteNodes.Where(item => availableNodeLevels.Contains(item.NodeLevel))
                .OrderBy(item => item.Url)
                .Select(item => new
                {
                    item.Id,
                    item.Url,
                    item.NodeLevel
                }).ToListAsync()).ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Url,
                    NodeLevel = item.NodeLevel
                });
        }

        public async IAsyncEnumerable<RMRemoteNode> GetOpusSitesAsync(Guid containerId)
        {
            var isTeamsAvailable = await IsTeamsAvailableAsync();
            using var context = RMDBContextManager.GetNewDBContext();
            var strContainerId = containerId.ToString();
            var batchCount = 1000;
            for (var i = 0; ; i += batchCount)
            {
                var batchSites = (await context.RMRemoteNodes
                    .Where(item => item.ParentId == strContainerId)
                    .OrderBy(item => item.Id)
                    .Skip(i)
                    .Take(batchCount)
                    .Select(item => new
                    {
                        item.Id,
                        item.Url,
                        item.TenantId,
                        item.NodeLevel,
                        item.ObjectId,
                        item.TeamId
                    })
                    .ToListAsync()).ConvertAll(item => new RMRemoteNode
                    {
                        Id = item.Id,
                        Url = item.Url,
                        TenantId = item.TenantId,
                        ObjectId = item.ObjectId,
                        NodeLevel = item.NodeLevel,
                        TeamId = item.TeamId,
                    });
                foreach (var site in batchSites)
                {
                    if (!string.IsNullOrWhiteSpace(site.TeamId) && isTeamsAvailable)
                    {
                        var teamSites = (await context.RMRemoteNodes
                            .Where(item => item.TeamId == site.TeamId)
                            .Select(item => new
                            {
                                item.Id,
                                item.Url,
                                item.TenantId,
                                item.NodeLevel,
                                item.ObjectId,
                                item.TeamId
                            })
                            .ToListAsync()).ConvertAll(item => new RMRemoteNode
                            {
                                Id = item.Id,
                                Url = item.Url,
                                TenantId = item.TenantId,
                                ObjectId = item.ObjectId,
                                NodeLevel = item.NodeLevel,
                                TeamId = item.TeamId,
                            });
                        foreach (var teamSite in teamSites)
                        {
                            yield return teamSite;
                        }
                    }
                    else
                    {
                        yield return site;
                    }
                }

                if (batchSites.Count < batchCount)
                {
                    yield break;
                }
            }
        }

        private static async Task<bool> IsTeamsAvailableAsync()
        {
            const string enableTeamsFeatureKey = "EnableTeamsFeature";
            const string hasUpgradeTeamsKey = "HasUpgradeTeams";
            using var context = RMDBContextManager.GetNewDBContext();
            var keys = new List<string> { enableTeamsFeatureKey, hasUpgradeTeamsKey };
            var kvs = await context.RMKeyValue.Where(k => keys.Contains(k.Key)).ToListAsync();
            var enableKv = kvs.FirstOrDefault(k => k.Key == enableTeamsFeatureKey);
            if (enableKv != null)
            {
                if (!bool.TryParse(enableKv.Value, out var enableParsed) || !enableParsed)
                {
                    return false;
                }
            }

            var upgradeKv = kvs.FirstOrDefault(k => k.Key == hasUpgradeTeamsKey);
            if (upgradeKv != null && bool.TryParse(upgradeKv.Value, out var upgraded) && upgraded)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> DeleteByIdsAsync(List<int> ids)
        {
            if (ids == null || !ids.Any()) return false;

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            using var transaction = efContext.Database.BeginTransaction();
            try
            {
                var profiles = await efContext.Set<RMDiscoveryPlanProfile>()
                                              .Where(x => ids.Contains(x.Id))
                                              .ToListAsync();

                if (!profiles.Any()) return false;

                efContext.Set<RMDiscoveryPlanProfile>().RemoveRange(profiles);

                var result = await efContext.SaveChangesAsync() > 0;
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao;
using RADiscovery.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Contract.Tree.Object;
using Cloud.Sdk.Data.IE;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Core;
using System.Data.Entity;
using AvePoint.GCommon.Utility;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using AvePoint.RA.Contract.Discovery.Model;

namespace RADiscovery.Work
{
    public class RMDiscoveryJobAllocator : RMDiscoveryWorker
    {

        private readonly IRMSyncNodeDao _syncNodeDao = new RMSyncNodeDao();

        private readonly AosModernApiTenantClient _aosApiClient;

        public RMDiscoveryJobAllocator() : base()
        {
            _aosApiClient = AosApiUtility.GetAosModerClient();
        }

        public async Task AllocateAsync()
        {
            var scopeInfo = new RMDiscoveryScopeInfo();

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var job = efContext.MainJobs.First(item => item.Status == RMDiscoveryJobStatus.Preparing);
            job.Status = RMDiscoveryJobStatus.Pending;
            efContext.Entry(job).Property(item => item.Status).IsModified = true;
            await efContext.SaveChangesAsync();

            var effectO365TenantIds = await AllocateAsync(scopeInfo, job);

            await InitTablesAsync(scopeInfo, job.HasRuleChange, effectO365TenantIds);

            job.Status = RMDiscoveryJobStatus.Running;
            efContext.Entry(job).Property(item => item.Status).IsModified = true;
            await efContext.SaveChangesAsync();
        }

        private async Task InitTablesAsync(RMDiscoveryScopeInfo scopeInfo, bool hasRuleChange, HashSet<Guid> o365TenantIds)
        {
            var aosTenantInfoes = await _aosApiClient.TenantManagementService.GetByTypeAsync(PlatformType.Office365);

            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var tenantInfoes = await efContext.O365TenantInfoes.ToListAsync();
            var existsTenantIds = tenantInfoes.Select(item => item.UniqueId).ToHashSet();

            var needDropTenantIds = new HashSet<Guid>();
            var needDeleteTenants = existsTenantIds.Except(o365TenantIds).ConvertAll(tenantId => tenantInfoes.First(item => item.UniqueId == tenantId));
            if (needDeleteTenants.Any())
            {
                efContext.O365TenantInfoes.RemoveRange(needDeleteTenants);
                await efContext.SaveChangesAsync();
                await tenantInfoes.ToAsyncEnumerable().ForEachAwaitAsync(async tenantInfo =>
                {
                    await RMDiscoveryDBManager.DropTables(tenantInfo.UniqueId);
                });
                needDropTenantIds.UnionWith(needDeleteTenants.Select(item => item.UniqueId));
            }
            if (scopeInfo.ScopeType == RMDiscoveryScopeType.All || hasRuleChange)
            {
                needDropTenantIds.UnionWith(existsTenantIds);
            }
            await needDropTenantIds.ToAsyncEnumerable().ForEachAwaitAsync(async tenantId =>
            {
                await RMDiscoveryDBManager.DropTables(tenantId);
            });

            var needUpdateTenants = existsTenantIds.Intersect(o365TenantIds).ConvertAll(tenantId =>
            {
                var existsTenantInfo = tenantInfoes.First(item => item.UniqueId == tenantId);
                var aosTenantInfo = aosTenantInfoes.First(item => item.Id.Equals(tenantId.ToString(), StringComparison.OrdinalIgnoreCase));
                var hasChange = !existsTenantInfo.Name.Equals(aosTenantInfo.Name, StringComparison.OrdinalIgnoreCase)
                    || !existsTenantInfo.AdminUrl.Equals(aosTenantInfo.AdminUrl, StringComparison.OrdinalIgnoreCase);
                existsTenantInfo.Name = aosTenantInfo.Name;
                existsTenantInfo.AdminUrl = aosTenantInfo.AdminUrl;
                if (hasChange)
                {
                    existsTenantInfo.ModifiedTime = DateTime.UtcNow.Ticks;
                }

                efContext.Entry(existsTenantInfo).State = hasChange ? EntityState.Modified : EntityState.Unchanged;

                return existsTenantInfo;
            });
            if (needUpdateTenants.Any())
            {
                await efContext.SaveChangesAsync();
            }

            var needAddTenant = o365TenantIds.Except(existsTenantIds).ConvertAll(tenantId =>
            {
                var aosTenantInfo = aosTenantInfoes.First(item => item.Id.Equals(tenantId.ToString(), StringComparison.OrdinalIgnoreCase));

                return new RMDiscoveryO365TenantInfo
                {
                    UniqueId = tenantId,
                    Name = aosTenantInfo.Name,
                    AdminUrl = aosTenantInfo.AdminUrl,
                    Environment = (RMAADEnvironment)aosTenantInfo.AadEnvironment,
                    CreatedTime = DateTime.UtcNow.Ticks,
                    ModifiedTime = DateTime.UtcNow.Ticks,
                };
            }).ToList();
            if(needAddTenant.Any())
            {
                efContext.O365TenantInfoes.AddRange(needAddTenant);
                await efContext.SaveChangesAsync();
            }

            var needInitTableTenants = needUpdateTenants.Union(needAddTenant).DistinctBy(item => item.UniqueId).ToAsyncEnumerable();
            await needInitTableTenants.ForEachAwaitAsync(async item =>
            {
                await RMDiscoveryDBManager.InitO365TablesAsync(item.UniqueId);
            });
        }

        private async Task<HashSet<Guid>> AllocateAsync(RMDiscoveryScopeInfo scopeInfo, RMDiscoveryMainJob job)
        {
            var o365TenantIds = new HashSet<Guid>();
            if (scopeInfo.ScopeType == RMDiscoveryScopeType.All)
            {
                var containers = await _syncNodeDao.GetContainersAsync();
                foreach (var container in containers)
                {
                    var allocateInfoes = AllocateContainer(container);
                    var tenantIds = await RunJobAsync(job.Id, allocateInfoes, job.HasRuleChange);
                    o365TenantIds.UnionWith(tenantIds);
                }
            }
            else
            {
                foreach (var containerId in scopeInfo.SpecifyContainerIds)
                {
                    var container = await _syncNodeDao.GetContainerAsync(containerId);
                    var allocateInfoes = AllocateContainer(container);
                    var tenantIds = await RunJobAsync(job.Id, allocateInfoes, job.HasRuleChange);
                    o365TenantIds.UnionWith(tenantIds);
                }
            }
            return o365TenantIds;
        }

        private async Task<HashSet<Guid>> RunJobAsync(Guid mainJobId, IAsyncEnumerable<(RMRemoteNode container, JobInitiationModel initiationMode)> allocateInfoes, bool hasRulChange)
        {
            var o365TenantIds = new HashSet<Guid>();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            await foreach ((RMRemoteNode container, JobInitiationModel initiationMode) in allocateInfoes)
            {
                initiationMode.EnforceTagRuleCheck = hasRulChange;
                var triggerTime = DateTime.UtcNow.Ticks;
                // need add roll-back logic
                var discoveryJobInfo = await _ieApiClient.JobService.TriggerAsync(initiationMode);
                efContext.DiscoveryJobs.Add(new RMDiscoveryJob
                {
                    Id = discoveryJobInfo.Id,
                    MainJobId = mainJobId,
                    O365TenantId = new Guid(initiationMode.AzureTenantId),
                    ContainerId = new Guid(container.Id),
                    ContainerName = container.Name,
                    SiteCount = initiationMode.SiteInfos.Count,
                    Status = RMDiscoveryJobStatus.Pending,
                    StartTime = DateTime.UtcNow.Ticks,
                    LastCheckedTime = triggerTime,
                });

                await efContext.SaveChangesAsync();
                await PreparerAnalysisJobsAsync(mainJobId, discoveryJobInfo.Id, container, initiationMode);
                o365TenantIds.Add(new Guid(initiationMode.AzureTenantId));
            }

            return o365TenantIds;
        }

        private async Task PreparerAnalysisJobsAsync(Guid mainJobId, Guid discoveryJobId, RMRemoteNode container, JobInitiationModel initiationMode)
        {
            var analysisJobs = initiationMode.SiteInfos.ConvertAll(item => new RMDiscoveryAnalysisJob
            {
                Id = Guid.NewGuid(),
                MainJobId = mainJobId,
                DiscoveryJobId = discoveryJobId,
                O365TenantId = new Guid(initiationMode.AzureTenantId),
                ContainerId = new Guid(container.Id),
                SiteId = new Guid(item.SiteId),
                Status = RMDiscoveryJobStatus.Preparing,
                StartTime = DateTime.UtcNow.Ticks,
            });
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(analysisJobs);
        }

        private async IAsyncEnumerable<(RMRemoteNode container, JobInitiationModel initiationMode)> AllocateContainer(RMRemoteNode container)
        {
            var tenantSites = new Dictionary<Guid, List<SiteInfoModel>>();
            var sitesAsyncEnumerable = _syncNodeDao.GetSitesAsync(new Guid(container.Id));
            await foreach (var site in sitesAsyncEnumerable)
            {
                if (!tenantSites.TryGetValue(new Guid(site.TenantId), out var sites))
                {
                    sites = new List<SiteInfoModel>();
                    tenantSites[new Guid(site.TenantId)] = sites;
                }

                if (site.NodeLevel == (int)NodeLevel.O365GroupSites)
                {
                    // get real site id
                }

                sites.Add(new SiteInfoModel(site.ObjectId, site.Url));
            }

            foreach (var tenant in tenantSites.Keys)
            {
                yield return (container, new JobInitiationModel
                {
                    Type = DataType.SPDocument,
                    AzureTenantId = tenant.ToString(),
                    SiteInfos = tenantSites[tenant]
                });
            }
        }
    }
}

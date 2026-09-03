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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365NodeDao : IRMDiscoveryOffice365NodeDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365NodeDao));

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

        private static readonly Dictionary<SourceFlag, List<int>> S_CONTENT_SOURCE_AVAILABLE_SITE_LEVEL = new()
        {
            {SourceFlag.SharePoint, [(int)NodeLevel.SiteCollection, (int)NodeLevel.O365GroupSites, (int)NodeLevel.PrivateChannel, (int)NodeLevel.SharedChannel]},
            {SourceFlag.OneDrive, [(int)NodeLevel.SkyDrivePro]},
        };

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

        public async Task AddOrUpdateDiscoveryContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerInfo[] containers)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            await AddOrUpdateDiscoveryContainerAsync(efContext, containers);
        }

        public async Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryOffice365ContainerInfo[] containers)
        {
            if (!containers.Any())
            {
                return;
            }
            efContext.Office365ContainerInfoes.AddOrUpdate(containers);
            await efContext.SaveChangesAsync();
        }

        public async Task<(bool has, RMDiscoveryOffice365ContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(Guid o365TenantId, Guid opusId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var res = await efContext.Office365ContainerInfoes.FirstOrDefaultAsync(item => item.OpusId == opusId);
            return (res != null, res);
        }

        public async Task<List<string>> GetContainerNamesByIds(Guid o365TenantId, IEnumerable<int> ids)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerInfoes.Where(info => ids.Contains(info.Id)).Select(info => info.Name).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365ContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerInfoes.ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365ContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365ContainerInfoes.Where(item => item.ContentSource == contentSource).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365ContainerInfo>> GetDiscoveryContainerInfoesAsync(Guid o365TenantId, IEnumerable<int> containerIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var ids = containerIds.ToHashSet();
            return await efContext.Office365ContainerInfoes.Where(item => ids.Contains(item.Id)).ToListAsync();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, params int[] containerIds)
        {
            const int pageSize = 1000;
            var hasAny = containerIds.Any();

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var sites = await efContext.Office365SiteInfoes.Where(item => !hasAny || Enumerable.Contains(containerIds, item.ContainerId))
                    .OrderBy(item => item.Id)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var site in sites)
                {
                    yield return site;
                }

                if (sites.Count < pageSize)
                {
                    yield break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, SourceFlag contentSource, params int[] containerIds)
        {
            const int pageSize = 1000;
            var lastId = 0;

            var hasAny = containerIds.Any();

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var sites = await efContext.Office365SiteInfoes.Where(item => (!hasAny || Enumerable.Contains(containerIds, item.ContainerId)) && item.ContentSource == contentSource && item.Id > lastId)
                    .OrderBy(item => item.Id)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var site in sites)
                {
                    yield return site;
                }

                if (sites.Count < pageSize)
                {
                    yield break;
                }

                lastId = sites.Last().Id;
            }
        }

        public async Task<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, Guid siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteInfoes.FirstOrDefaultAsync(item => item.SiteId == siteId);
        }

        public async Task<RMDiscoveryOffice365SiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteInfoes.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<int> CountDiscoverySiteAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            return await efContext.Office365SiteInfoes.CountAsync();
        }

        public async Task<int> CountDiscoverySiteAsync(List<Guid> o365TenantIds)
        {
            var totalCount = 0;
            foreach (var o365TenantId in o365TenantIds)
            {
                using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
                totalCount += await efContext.Office365SiteInfoes.CountAsync();
            }
            return totalCount;
        }

        public async Task AddOrUpdateDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInfo[] sites)
        {
            if (!sites.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            efContext.Office365SiteInfoes.AddOrUpdate(sites);
            await efContext.SaveChangesAsync();
        }

        public async Task DeleteDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInfo[] sites)
        {
            if (!sites.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            sites.ForEach(item => efContext.Entry(item).State = EntityState.Deleted);
            efContext.Office365SiteInfoes.RemoveRange(sites);
            await efContext.SaveChangesAsync();
        }

        public async Task<RMRemoteNode> GetOpusContainerById(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.FirstAsync(item => item.Id == id.ToString());
        }

        public async Task<int> CountOpusContainersAsync(params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableNodeLevels = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL[item]).SelectMany(item => item).ToList();
            if(!await IsTeamsAvailableAsync())
            {
                var disableTeamsAvaliableNodeLevels = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_DISABLE_TEAMS_AVAILABLE_CONTAINER_LEVEL[item]).SelectMany(item => item).ToList();
                availableNodeLevels.AddRange(disableTeamsAvaliableNodeLevels);
            }
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.Where(item => availableNodeLevels.Contains(item.NodeLevel)).CountAsync();
        }

        public async Task<int> CountOpusSitesAsync(params SourceFlag[] contentSources)
        {
            var hasAny = contentSources.Any();
            var availableNodeLevels = contentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_SITE_LEVEL[item]).SelectMany(item => item).ToList();
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.Where(item => !hasAny || availableNodeLevels.Contains(item.NodeLevel)).CountAsync();
        }

        public async Task<int> CountOpusSitesAsync(IEnumerable<Guid> containerIds)
        {
            var isTeamsAvailable = await IsTeamsAvailableAsync();
            var containerIdStrs = containerIds.Select(id => id.ToString()).ToList();
            if (containerIdStrs.Count == 0)
                return 0;
            
            const int batchSize = 100; 
            var allIds = new HashSet<string>();

            using var performance = new PerformanceScope("CountOpusSitesByContainerIds");
            for (int i = 0; i < containerIdStrs.Count; i += batchSize)
            {
                using var context = RMDBContextManager.GetNewDBContext();
                SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                var batch = containerIdStrs.Skip(i).Take(batchSize).ToList();
                if (!isTeamsAvailable)
                {
                    var ids = await context.RMRemoteNodes
                        .Where(item => batch.Contains(item.ParentId))
                        .Select(item => item.Id)
                        .Distinct()
                        .ToListAsync();
                    foreach (var id in ids)
                        allIds.Add(id);
                }
                else
                {
                    var inClause = DatabaseUtility.BuildInClause(batch, out var paras);
                    var sql = $@"
SELECT Id FROM {context.SchemaName}.RMRemoteNodes WHERE ParentId IN {inClause} 
UNION
SELECT Id FROM {context.SchemaName}.RMRemoteNodes WHERE TeamId IN (
    SELECT DISTINCT TeamId FROM {context.SchemaName}.RMRemoteNodes WHERE ParentId IN {inClause} AND TeamId IS NOT NULL AND TeamId <> ''
)";
                    
                    var ids = await context.Database.SqlQuery<string>(sql, paras.ToArray()).ToListAsync();
                    foreach (var id in ids)
                        allIds.Add(id);
                }
            }

            return allIds.Count;
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
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
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

        public async Task<List<Guid>> GetOpusO365TenantIdsByContainerAsync(List<NodeLevel> supportNodeLevels, params Guid[] containerIds)
        {
            var containerIdList = containerIds.ConvertAll(item => item.ToString()).ToList();
            var hasAny = containerIdList.Any();
            var supportIntNodeLevels = supportNodeLevels.ConvertAll(item => (int)item);
            using var context = RMDBContextManager.GetNewDBContext();
            var o365TenantIds = await context.RMRemoteNodes.Where(item => (!hasAny || containerIdList.Contains(item.ParentId)) && supportIntNodeLevels.Contains(item.NodeLevel) && !string.IsNullOrEmpty(item.ParentId)).Select(item => item.TenantId).Distinct().ToListAsync();
            return o365TenantIds.ConvertAll(item => new Guid(item));
        }

        public async IAsyncEnumerable<RMRemoteNode> GetOpusSitesAsync(Guid containerId)
        {
            var isTeamsAvailable = await IsTeamsAvailableAsync();
            using var context = RMDBContextManager.GetNewDBContext();
            _logger.Info($"The sql timeout for discovery get sites is {context?.Database?.CommandTimeout}s");
            if (context == null)
            {
                _logger.Error("Can not get DB context");
                throw new Exception("Can not get DB context,context is null");
            }
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

        public async Task<List<RMRemoteNode>> GetOpusTopSitesAsync(int top, params Guid[] containerIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var availableNodeLevels = new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive }.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_SITE_LEVEL[item]).SelectMany(item => item).ToList();
            _logger.Info($"The sql timeout for discovery get sites is {context?.Database?.CommandTimeout}s");
            var strContainerIds = containerIds.ConvertAll(item => item.ToString()).ToList();
            var hasAny = strContainerIds.Any();
            return await context?.RMRemoteNodes.Where(item => (!hasAny || strContainerIds.Contains(item.ParentId)) &&
                availableNodeLevels.Contains(item.NodeLevel))?
                .OrderBy(item => item.Url)
                .Take(top)?.ToListAsync();
        }

        public async Task<List<RMRemoteNode>> GetOpusTopSitesAsync(int top, params SourceFlag[] contentSources)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableNodeLevels = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_SITE_LEVEL[item]).SelectMany(item => item).ToList();
            return await context.RMRemoteNodes.Where(item =>
                availableNodeLevels.Contains(item.NodeLevel))
                .OrderBy(item => item.Url)
                .Take(top)?.ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosByContainerIds(Guid o365TenantId, IEnumerable<int> containerIds)
        {
            const int pageSize = 1000;
            var result = new List<RMDiscoveryOffice365SiteInfo>();
            if (containerIds == null)
            {
                return result;
            }

            var idList = containerIds.Distinct().ToList();
            if (idList.Count == 0)
            {
                return result;
            }

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var skip = 0;
            var pageIndex = 0;
            while (true)
            {
                var batch = await efContext.Office365SiteInfoes
                    .Where(item => idList.Contains(item.ContainerId))
                    .OrderBy(item => item.Id)
                    .Skip(skip)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.Info($"GetSiteInfosByContainerIds page {pageIndex} fetched {batch?.Count} records for tenant {o365TenantId}.");

                if (batch.Count == 0)
                {
                    _logger.Info($"GetSiteInfosByContainerIds reached end at page {pageIndex} for tenant {o365TenantId}.");
                    break;
                }

                result.AddRange(batch);

                if (batch.Count < pageSize)
                {
                    _logger.Info($"GetSiteInfosByContainerIds completed at page {pageIndex} for tenant {o365TenantId} with {result.Count} records.");
                    break;
                }

                skip += batch.Count;
                pageIndex++;
            }

            return result;
        }

        public async Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<long> siteIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var ids = siteIds.ToHashSet();
            return await efContext.Office365SiteInfoes.Where(item => ids.Contains(item.Id)).ToListAsync();
        }
        public async Task<List<RMDiscoveryOffice365SiteInfo>> GetSiteInfosBySiteUrl(Guid o365TenantId, IEnumerable<string> siteUrls)
        {
            var result = new List<RMDiscoveryOffice365SiteInfo>();
            if (siteUrls == null) return result;

            var urlsList = siteUrls.Distinct().ToList();
            if (urlsList.Count == 0) return result;

            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);

            const int batchSize = 200;
            for (int i = 0; i < urlsList.Count; i += batchSize)
            {
                var batchUrls = urlsList.Skip(i).Take(batchSize).ToHashSet();
                var batchResult = await efContext.Office365SiteInfoes.Where(item => batchUrls.Contains(item.Url)).ToListAsync();
                result.AddRange(batchResult);
            }

            return result;
        }
        public async Task<List<string>> GetSiteUrlBySiteIds(Guid o365TenantId, IEnumerable<int> siteIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);
            var ids = siteIds.ToHashSet();
            return await efContext.Office365SiteInfoes.Where(item => ids.Contains(item.Id)).Select(item => item.Url).ToListAsync();
        }

        public async Task<int> DeleteSiteDataBySiteIdAsync(Guid o365TenantId, Guid siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetOffice365EFContextAsync(o365TenantId);

            // Find the site info record by SiteId (Guid)
            var siteInfo = await efContext.Office365SiteInfoes.FirstOrDefaultAsync(item => item.SiteId == siteId);
            if (siteInfo == null)
            {
                _logger.Info($"No site info found for site [{siteId}] in tenant [{o365TenantId}]. No cleanup needed.");
                return -1;
            }

            // Remove the site info record
            efContext.Office365SiteInfoes.Remove(siteInfo);
            await efContext.SaveChangesAsync();
            _logger.Info($"Successfully deleted site info for site [{siteId}] (Id: {siteInfo.Id}) from tenant [{o365TenantId}].");
            return siteInfo.Id;

        }
    }
}

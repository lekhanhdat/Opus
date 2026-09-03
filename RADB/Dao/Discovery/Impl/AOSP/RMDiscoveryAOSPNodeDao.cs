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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.Aos.Tenant;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using RemoteNodeType = Cloud.Sdk.Data.AosModern.RemoteNodeType;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPNodeDao : IRMDiscoveryAOSPNodeDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPNodeDao));

        private static readonly Dictionary<SourceFlag, List<RemoteNodeType>> S_CONTENT_SOURCE_AVAILABLE_CONTAINER_TYPE = new()
        {
            {SourceFlag.SharePoint, [RemoteNodeType.SiteCollection, RemoteNodeType.Office365Group]},
            {SourceFlag.OneDrive, [RemoteNodeType.OneDrive]},
        };

        private static readonly Dictionary<SourceFlag, List<int>> S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL = new()
        {
            {SourceFlag.SharePoint, [(int)NodeLevel.WebApplication, (int)NodeLevel.O365GroupSitesGroup]},
            {SourceFlag.OneDrive, [(int)NodeLevel.SkyDriveProGroup]},
            {SourceFlag.Google, [(int)NodeLevel.GoogleMyDriveContainer, (int)NodeLevel.GoogleSharedDriveContainer] }
        };

        private static readonly Dictionary<SourceFlag, List<int>> S_CONTENT_SOURCE_AVAILABLE_SITE_LEVEL = new()
        {
            {SourceFlag.SharePoint, [(int)NodeLevel.SiteCollection, (int)NodeLevel.O365GroupSites]},
            {SourceFlag.OneDrive, [(int)NodeLevel.SkyDrivePro]},
            {SourceFlag.Google, [(int)NodeLevel.GoogleMyDrive, (int)NodeLevel.GoogleSharedDrive] }
        };


        public async Task<List<RMRemoteNode>> GetAOSContainersAsync(string o365TenantId, params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableAOSNodeTypes = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_TYPE[item]).SelectMany(item => item).ToList();
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var containerInfoes = new List<RMRemoteNode>();
            var currentContainerInfoes = await client.ContainerService.GetByTenantIdAsync(o365TenantId, availableAOSNodeTypes);
            if (currentContainerInfoes.Count != 0)
            {
                containerInfoes.AddRange(currentContainerInfoes.ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Name,
                    NodeLevel = GetContainerNodeLevel(item.NodeType)
                }));
            }

            return containerInfoes;
        }

        public async Task<List<RMRemoteNode>> GetAOSContainersForAOSPAsync(string o365TenantId, params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableAOSNodeTypes = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_TYPE[item]).SelectMany(item => item).ToList();
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);

            var currentContainerInfoes = await client.ImpersonateCallerInvoke<AosModernApiTenantClient, List<ContainerInfo>?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
            {
                var currentContainerInfoes = await client.ContainerService.GetByTenantIdAsync(o365TenantId, availableAOSNodeTypes);
                return currentContainerInfoes;
            });

            var containerInfoes = new List<RMRemoteNode>();
            if (currentContainerInfoes.Count != 0)
            {
                containerInfoes.AddRange(currentContainerInfoes.ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Name,
                    NodeLevel = GetContainerNodeLevel(item.NodeType)
                }));
            }
            return containerInfoes;
        }

        public async Task<List<RMRemoteNode>> GetAOSSitesAsync(string o365TenantId, string containerId, int nodeLevel)
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var remoteNodeType = GetRemoteNodeType(nodeLevel);
            var remoteNodes = GetRemoteNodes(await client.RemoteNodeService.GetNodesByContainerIdAsync(containerId, remoteNodeType), remoteNodeType);
            return remoteNodes.Where(node => o365TenantId == node.TenantId).ToList();
        }

        public async Task<int> CountAOSSitesAsync(string o365TenantId, params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableAOSNodeTypes = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_TYPE[item]).SelectMany(item => item).ToList();
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var count = 0;
            foreach (var aosNodeType in availableAOSNodeTypes)
            {
                var queryResult = await client.RemoteNodeService.GetEachContainerNodeCountAsync(aosNodeType, o365TenantId);
                queryResult.ForEach(result => count += result.Value);
            }
            return count;
        }

        public async Task<int> CountAOSContainersAsync(string o365TenantId, params SourceFlag[] contentSources)
        {
            var availableContentSources = contentSources.Any() ? new List<SourceFlag>(contentSources) : [SourceFlag.SharePoint, SourceFlag.OneDrive];
            var availableAOSNodeTypes = availableContentSources.ConvertAll(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_TYPE[item]).SelectMany(item => item).ToList();
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var containerInfoes = new List<ContainerInfo>();
            var queryResult = await client.ContainerService.GetByTenantIdAsync(o365TenantId, availableAOSNodeTypes);
            containerInfoes.AddRange(queryResult);
            return containerInfoes.DistinctBy(container => container.Id).Count();
        }

        public async Task<(bool has, RMDiscoveryAOSPContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(Guid o365TenantId, Guid opusId)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            var res = await efContext.AOSPContainerInfoes.FirstOrDefaultAsync(item => item.OpusId == opusId);
            return (res != null, res);
        }

        public async Task<RMRemoteNode> GetOpusContainerById(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.FirstAsync(item => item.Id == id.ToString());
        }

        public async Task AddOrUpdateDiscoveryContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerInfo[] containers)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            await AddOrUpdateDiscoveryContainerAsync(efContext, containers);
        }

        public async Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryAOSPContainerInfo[] containers)
        {
            if (!containers.Any())
            {
                return;
            }
            efContext.AOSPContainerInfoes.AddOrUpdate(containers);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryAOSPContainerInfo>> GetDiscoveryContainersAsync(Guid o365TenantId, IEnumerable<int> ids)
        {
            var idsSet = ids.ToHashSet();
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPContainerInfoes.Where(item => idsSet.Contains(item.Id)).ToListAsync();
        }

        public async Task<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, Guid siteId)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPSiteInfoes.FirstOrDefaultAsync(item => item.SiteId == siteId);
        }

        public async Task<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoAsync(Guid o365TenantId, int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPSiteInfoes.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task AddOrUpdateDiscoverySiteAsync(Guid o365TenantId, params RMDiscoveryAOSPSiteInfo[] sites)
        {
            if (!sites.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            efContext.AOSPSiteInfoes.AddOrUpdate(sites);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryAOSPContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPContainerInfoes.ToListAsync();
        }

        public async Task<List<RMDiscoveryAOSPContainerInfo>> GetAllDiscoveryContainersAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            return await efContext.AOSPContainerInfoes.Where(item => item.ContentSource == contentSource).ToListAsync();
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, params int[] containerIds)
        {
            const int pageSize = 1000;
            var hasAny = containerIds.Any();

            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var sites = await efContext.AOSPSiteInfoes.Where(item => !hasAny || Enumerable.Contains(containerIds, item.ContainerId))
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

        public async IAsyncEnumerable<RMDiscoveryAOSPSiteInfo> GetDiscoverySiteInfoesAsync(Guid o365TenantId, SourceFlag contentSource, params int[] containerIds)
        {
            const int pageSize = 1000;
            var lastId = 0;

            var hasAny = containerIds.Any();

            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            for (var i = 0; ; i++)
            {
                var sites = await efContext.AOSPSiteInfoes.Where(item => (!hasAny || Enumerable.Contains(containerIds, item.ContainerId)) && item.ContentSource == contentSource && item.Id > lastId)
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

        public async Task<List<RMDiscoveryAOSPSiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<long> siteIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            var ids = siteIds.ToHashSet();
            return await efContext.AOSPSiteInfoes.Where(item => ids.Contains(item.Id)).ToListAsync();
        }

        public async Task<List<RMDiscoveryAOSPSiteInfo>> GetSiteInfosBySiteIds(Guid o365TenantId, IEnumerable<Guid> siteUniqueIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetAOSPEFContextAsync(o365TenantId);
            var ids = siteUniqueIds.ToHashSet();
            return await efContext.AOSPSiteInfoes.Where(item => ids.Contains(item.SiteId)).ToListAsync();
        }

        private List<RMRemoteNode> GetRemoteNodes(RemoteNodesResult remoteNodesResult, RemoteNodeType aosNodeType)
        {
            var nodeLevel = GetSiteNodeLevel(aosNodeType);
            return aosNodeType switch
            {
                RemoteNodeType.SiteCollection => remoteNodesResult.SPSites.ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Url,
                    TenantId = item.TenantId,
                    ObjectId = item.ObjectId,
                    NodeLevel = nodeLevel,
                    AdminUrl = item.AdminUrl,
                }),
                RemoteNodeType.Office365Group => remoteNodesResult.O365Groups.ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.SiteUrl,
                    TenantId = item.TenantId,
                    ObjectId = item.SiteId,
                    NodeLevel = nodeLevel,
                    AdminUrl = item.AdminUrl,
                }),
                RemoteNodeType.OneDrive => remoteNodesResult.OneDrives.ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Url = item.Url,
                    TenantId = item.TenantId,
                    ObjectId = item.ObjectId,
                    NodeLevel = nodeLevel,
                    AdminUrl = item.AdminUrl,
                }),
                _ => throw new Exception()
            };
        }

        private static int GetContainerNodeLevel(RemoteNodeType aosNodeType)
        {
            return aosNodeType switch
            {
                RemoteNodeType.SiteCollection => (int)NodeLevel.WebApplication,
                RemoteNodeType.Office365Group => (int)NodeLevel.O365GroupSitesGroup,
                RemoteNodeType.OneDrive => (int)NodeLevel.SkyDriveProGroup,
                _ => 0,
            };
        }

        private static int GetSiteNodeLevel(RemoteNodeType aosNodeType)
        {
            return aosNodeType switch
            {
                RemoteNodeType.SiteCollection => (int)NodeLevel.SiteCollection,
                RemoteNodeType.Office365Group => (int)NodeLevel.O365GroupSites,
                RemoteNodeType.OneDrive => (int)NodeLevel.SkyDrivePro,
                _ => 0,
            };
        }

        private static RemoteNodeType GetRemoteNodeType(int nodeLevel)
        {
            return nodeLevel switch
            {
                (int)NodeLevel.WebApplication => RemoteNodeType.SiteCollection,
                (int)NodeLevel.O365GroupSitesGroup => RemoteNodeType.Office365Group,
                (int)NodeLevel.SkyDriveProGroup => RemoteNodeType.OneDrive,
                _ => throw new Exception(),
            };
        }
    }
}

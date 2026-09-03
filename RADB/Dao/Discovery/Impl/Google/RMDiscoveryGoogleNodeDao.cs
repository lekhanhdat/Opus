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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleNodeDao : IRMDiscoveryGoogleNodeDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleNodeDao));

        private static readonly List<int> S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL = new()
        {
            (int)NodeLevel.GoogleMyDriveContainer,
            (int)NodeLevel.GoogleSharedDriveContainer
        };

        private static readonly List<int> S_CONTENT_SOURCE_AVAILABLE_DRIVE_LEVEL = new()
        {
            (int)NodeLevel.GoogleMyDrive,
            (int)NodeLevel.GoogleSharedDrive
        };

        public async Task AddOrUpdateDiscoveryContainerAsync(string googleOrganizationId, params RMDiscoveryGoogleContainerInfo[] containers)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            await AddOrUpdateDiscoveryContainerAsync(efContext, containers);
        }

        public async Task AddOrUpdateDiscoveryContainerAsync(RMDiscoveryDBEFContext efContext, params RMDiscoveryGoogleContainerInfo[] containers)
        {
            if (!containers.Any())
            {
                return;
            }
            efContext.GoogleContainerInfoes.AddOrUpdate(containers);
            await efContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateDiscoveryGoogleDriveAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInfo[] drives)
        {
            if (!drives.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            efContext.GoogleDriveInfoes.AddOrUpdate(drives);
            await efContext.SaveChangesAsync();
        }

        public async Task<int> CountDiscoveryGoogleDriveAsync(string googleOrganizationId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleDriveInfoes.CountAsync();
        }

        public async Task<int> CountOpusGoogleContainersAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.Where(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL.Contains(item.NodeLevel)).CountAsync();
        }

        public async Task<int> CountOpusGoogleDrivesAsync(IEnumerable<Guid> containerIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var count = 0;
            var containers = containerIds.ToList();
            for (var i = 0; i < containers.Count; i += 100)
            {
                var batch = containerIds.Skip(i).Take(100).ConvertAll(item => item.ToString()).ToHashSet();
                var driveCount = await context.RMRemoteNodes
                    .Where(item => batch.Contains(item.ParentId) && item.Name != null && S_CONTENT_SOURCE_AVAILABLE_DRIVE_LEVEL.Contains(item.NodeLevel))
                    .CountAsync();
                count += driveCount;
            }
            return count;
        }

        public async Task<int> CountOpusGoogleDrivesAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.Where(item => S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL.Contains(item.NodeLevel)).CountAsync();
        }

        public async Task DeleteDiscoveryGoogleDrivesAsync(string googleOrganizationId, params RMDiscoveryGoogleDriveInfo[] drives)
        {
            if (!drives.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            drives.ForEach(item => efContext.Entry(item).State = EntityState.Deleted);
            efContext.GoogleDriveInfoes.RemoveRange(drives);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryGoogleContainerInfo>> GetAllDiscoveryGoogleContainersAsync(string googleOrganizationId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleContainerInfoes.ToListAsync();
        }

        public async Task<List<RMDiscoveryGoogleContainerInfo>> GetDiscoveryGoogleContainerInfoesAsync(string googleOrganizationId, IEnumerable<int> containerIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            var ids = containerIds.ToHashSet();
            return await efContext.GoogleContainerInfoes.Where(item => ids.Contains(item.Id)).ToListAsync();
        }

        public async Task<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoAsync(string googleOrganizationId, string driveId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleDriveInfoes.FirstOrDefaultAsync(item => item.DriveId == driveId);
        }

        public async Task<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoAsync(string googleOrganizationId, int id)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            return await efContext.GoogleDriveInfoes.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleDriveInfo> GetDiscoveryGoogleDriveInfoesAsync(string googleOrganizationId, params int[] containerIds)
        {
            const int pageSize = 1000;
            var hasAny = containerIds.Any();

            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            for (var i = 0; ; i++)
            {
                var drives = await efContext.GoogleDriveInfoes.Where(item => (!hasAny || Enumerable.Contains(containerIds, item.ContainerId)))
                    .OrderBy(item => item.Id)
                    .Skip(i * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                foreach (var drive in drives)
                {
                    yield return drive;
                }

                if (drives.Count < pageSize)
                {
                    yield break;
                }
            }
        }

        public async Task<List<RMDiscoveryGoogleDriveInfo>> GetDriveInfoesByContainerIds(string googleOrganizationId, IEnumerable<int> containerIds)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            var ids = containerIds.ToHashSet();
            return await efContext.GoogleDriveInfoes.Where(item => ids.Contains(item.ContainerId)).ToListAsync();
        }

        public Task<List<RMDiscoveryGoogleDriveInfo>> GetDriveInfoesByDriveIds(string googleOrganizationId, IEnumerable<long> driveIds)
        {
            throw new NotImplementedException();
        }

        public async Task<RMRemoteNode> GetOpusGoogleContainerById(Guid id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes.FirstAsync(item => item.Id == id.ToString() && S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL.Contains(item.NodeLevel));
        }

        public async Task<List<RMRemoteNode>> GetOpusGoogleContainersAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext(); ;
            return await context.RMRemoteNodes
                .Where(i => i.Name != null && S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL.Contains(i.NodeLevel))
                .OrderBy(i => i.Url)
                .ToListAsync();
        }

        public async Task<List<RMRemoteNode>> GetOpusGoogleContainersAsync(IEnumerable<Guid> ids)
        {
            var containerIds = ids.ConvertAll(item => item.ToString()).ToList();
            using var context = RMDBContextManager.GetNewDBContext();
            return (await context.RMRemoteNodes
                .Where(item => containerIds.Contains(item.Id) && S_CONTENT_SOURCE_AVAILABLE_CONTAINER_LEVEL.Contains(item.NodeLevel))
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

        public async IAsyncEnumerable<RMRemoteNode> GetOpusGoogleDrivesAsync(Guid containerId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            _logger.Info($"The sql timeout for discovery get drives is {context?.Database?.CommandTimeout}s");
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
                    .Where(item => item.ParentId == strContainerId && (item.Name != null && S_CONTENT_SOURCE_AVAILABLE_DRIVE_LEVEL.Contains(item.NodeLevel)))
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
                    })
                    .ToListAsync()).ConvertAll(item => new RMRemoteNode
                    {
                        Id = item.Id,
                        Url = item.Url,
                        TenantId = item.TenantId,
                        ObjectId = item.ObjectId,
                        NodeLevel = item.NodeLevel
                    });
                foreach (var site in batchSites)
                {
                    yield return site;
                }

                if (batchSites.Count < batchCount)
                {
                    yield break;
                }
            }
        }

        public async Task<List<string>> GetOpusGoogleTenantIdsByContainerAsync(List<NodeLevel> supportNodeLevels, params Guid[] containerIds)
        {
            var containerIdList = containerIds.ConvertAll(item => item.ToString()).ToList();
            var hasAny = containerIdList.Any();
            var supportIntNodeLevels = supportNodeLevels.ConvertAll(item => (int)item);
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes
                .Where(item => (!hasAny || containerIdList.Contains(item.ParentId))
                    && supportIntNodeLevels.Contains(item.NodeLevel)
                    && !string.IsNullOrEmpty(item.ParentId)
                )
                .Select(item => item.TenantId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<RMRemoteNode>> GetOpusTopGoogleDrivesAsync(int top)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.RMRemoteNodes
                .Where(item => item.Name != null && S_CONTENT_SOURCE_AVAILABLE_DRIVE_LEVEL.Contains(item.NodeLevel))
                .OrderBy(item => item.Url)
                .Take(top)?.ToListAsync();
        }

        public async Task<List<RMRemoteNode>> GetOpusTopGoogleDrviesAsync(int top, params Guid[] containerIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            _logger.Info($"The sql timeout for discovery get drive is {context?.Database?.CommandTimeout}s");
            var strContainerIds = containerIds.ConvertAll(item => item.ToString()).ToList();
            var hasAny = strContainerIds.Any();
            return await context?.RMRemoteNodes
                .Where(item => (!hasAny || strContainerIds.Contains(item.ParentId))
                    && item.Name != null
                    && S_CONTENT_SOURCE_AVAILABLE_DRIVE_LEVEL.Contains(item.NodeLevel)
                )
                ?.OrderBy(item => item.Url)
                .Take(top)?.ToListAsync();
        }

        public async Task<(bool has, RMDiscoveryGoogleContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(string googleOrganizationId, Guid opusId)
        {
            using var efContext = await RMDiscoveryDBManager.GetGoogleEFContextAsync(googleOrganizationId);
            var res = await efContext.GoogleContainerInfoes.FirstOrDefaultAsync(item => item.OpusId == opusId);
            return (res != null, res);
        }
    }
}

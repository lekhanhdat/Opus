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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Utility;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeDao.Imp;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSyncNodeDao : BaseDao<RMRemoteNode>, IRMSyncNodeDao
    {

        private const int S_OPERATION_BATCH_COUNT = 1000;

        private readonly IRemoteNodeSynchronizeDao _remoteNodeSynchronizeDao = new SqliteRemoteNodeSynchronizeDao();
        private IRMRemoteNodeDao s_RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRemoteNodeEvent _remoteNodeEvent;
        public async Task<List<string>> GetTenantIdListFromDB()
        {
            var tenantIds = new List<string>();
            using var context = GetNewContext();
            while (true)
            {
                var res = await context.RMRemoteNodes
                    .Where(n => !string.IsNullOrEmpty(n.TenantId) && !tenantIds.Contains(n.TenantId))
                    .Select(n => n.TenantId)
                    .FirstOrDefaultAsync();

                if (res == null)
                    break;
                tenantIds.Add(res);
            }
            return tenantIds;
        }
        public async Task<List<RMContainerInfoAdaption>> GetSiteContainersAsync(NodeLevel nodeLevel)
        {
            var containers = await _remoteNodeSynchronizeDao.GetContainerNodesAsync(nodeLevel);

            return containers.ToList().ConvertAll(item => new RMContainerInfoAdaption
            {
                Id = item.Id,
                AosId = item.AosId,
                Name = item.Url,
                NodeLevel = (NodeLevel)item.NodeLevel
            });
        }

        public async Task AddSiteContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needAddContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    Name = item.Name,
                    Url = item.Name,
                    NodeLevel = (int)item.NodeLevel,
                    AosId = item.AosId,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedDate = DateTime.UtcNow.Ticks
                });

                await _remoteNodeEvent.NotifyAddAsync(needAddContainers);
            }
        }

        public async Task DeleteSiteContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needDeleteContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => item.Id);

                await _remoteNodeEvent.NotifyDeleteContainerAsync(needDeleteContainers);

            }
        }

        public async Task UpdateSiteContainerAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needUpdateContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT)
                    .ToDictionary(item => item.Id, item => item);
                var ids = needUpdateContainers.Keys.ToHashSet();

                var existContainers = await _remoteNodeSynchronizeDao.GetRemoteNodesAsync(ids);

                foreach (var existContainer in existContainers)
                {
                    var needUpdateContainer = needUpdateContainers[existContainer.Id];

                    existContainer.Name = needUpdateContainer.Name;
                    existContainer.Url = needUpdateContainer.Name;
                }

                await _remoteNodeEvent.NotifyAddAsync(existContainers);
            }
        }

        public async Task<List<RMContainerInfoAdaption>> GetExchangeContainersAsync()
        {
            using var context = GetDbContext();

            var containers = await context.RMMailboxes.Where(item => string.IsNullOrEmpty(item.ParentId)).ToListAsync();

            return containers.ConvertAll(item => new RMContainerInfoAdaption
            {
                Id = item.Id,
                AosId = item.AosId,
                Name = item.Name,
                NodeLevel = (NodeLevel)item.NodeLevel
            });
        }

        public async Task AddExchangeContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            using var context = GetDbContext();

            context.Database.CommandTimeout = 600;

            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needAddContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMMailbox
                {
                    Id = item.Id,
                    Name = item.Name,
                    NodeLevel = (int)item.NodeLevel,
                    AosId = item.AosId,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedDate = DateTime.UtcNow.Ticks
                });

                context.RMMailboxes.AddRange(needAddContainers);

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteExchangeContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            using var context = GetDbContext();

            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needDeleteContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMMailbox
                {
                    Id = item.Id
                });

                var inClauseParamName = DatabaseUtility.BuildInClause(needDeleteContainers.Select(item => item.Id), out var paramList);

                var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].RMMailboxes WHERE ParentId IN {inClauseParamName}";

                await context.Database.ExecuteSqlCommandAsync(sql, paramList.ToArray());

                needDeleteContainers.ForEach(item => context.Entry<RMMailbox>(item).State = EntityState.Deleted);

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateExchangeContainerAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            using var context = GetDbContext();

            for (var i = 0; i < containerInfoes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needUpdateContainers = containerInfoes.Skip(i).Take(S_OPERATION_BATCH_COUNT)
                    .ToDictionary(item => item.Id, item => item);
                var ids = needUpdateContainers.Keys.ToHashSet();

                var existContainers = context.RMMailboxes.Where(item => string.IsNullOrEmpty(item.ParentId) && ids.Contains(item.Id)).ToList();

                foreach (var existContainer in existContainers)
                {
                    var needUpdateContainer = needUpdateContainers[existContainer.Id];

                    existContainer.Name = needUpdateContainer.Name;
                }

                context.RMMailboxes.AddOrUpdate(existContainers.ToArray());

                await context.SaveChangesAsync();
            }
        }

        public async IAsyncEnumerable<RMSiteNodeAdaption> GetSiteNodesAsync(string containerId, string tenantId)
        {
            await foreach (var item in _remoteNodeSynchronizeDao.GetRemoteNodesAsync(containerId, tenantId))
            {
                yield return new RMSiteNodeAdaption
                {
                    Id = item.Id,
                    ObjectId = item.ObjectId,
                    TenantId = item.TenantId,
                    ContainerId = item.ParentId,
                    NodeLevel = (NodeLevel)item.NodeLevel,
                    AppType = (AppType)item.AppType,
                    ConnectionType = (BposConnectionType)item.AuthType,
                    Url = item.Url,
                    Name = item.Name,
                    TeamId = item.TeamId,
                    UserName = string.IsNullOrWhiteSpace(item.UserName)
                        ? string.Empty
                        : RMDatabaseDefaultEncryptor.DecryptToString(item.UserName),
                    DisplayName = item.DisplayName,
                    SiteCollectionType = (SiteCollectionType)item.SiteCollectionType
                };
            }
            ;

        }

        public async Task AddSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes)
        {
            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needAddNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMRemoteNode
                {
                    Id = item.Id,
                    ObjectId = item.ObjectId,
                    TenantId = item.TenantId,
                    ParentId = item.ContainerId,
                    NodeLevel = (int)item.NodeLevel,
                    AppType = (int)item.AppType,
                    AuthType = (int)item.ConnectionType,
                    TemplateName = item.TemplateName,
                    TemplateTitle = item.TemplateTitle,
                    IsPublicWebSite = item.IsPublicWebSite,
                    Url = item.Url,
                    AdminUrl = item.AdminUrl,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    TeamId = item.TeamId,
                    SiteCollectionType = (int)item.SiteCollectionType,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedDate = DateTime.UtcNow.Ticks,
                    UserName = string.IsNullOrWhiteSpace(item.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(item.UserName),
                });

                await _remoteNodeEvent.NotifyAddAsync(needAddNodes);
            }
        }

        public async Task DeleteSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes)
        {
            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needDeleteNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => item.Id);

                await _remoteNodeEvent.NotifyDeleteAsync(needDeleteNodes);
            }
        }

        public async Task UpdateSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes)
        {
            var nodesDic = nodes.ToDictionary(item => item.Id, item => item);

            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needUpdateIds = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).Select(item => item.Id);

                var needUpdateNodes = await _remoteNodeSynchronizeDao.GetRemoteNodesAsync(needUpdateIds);

                foreach (var needUpdateNode in needUpdateNodes)
                {
                    if (nodesDic.TryGetValue(needUpdateNode.Id, out var node))
                    {
                        needUpdateNode.ObjectId = node.ObjectId;
                        needUpdateNode.Name = node.Name;
                        needUpdateNode.Url = node.Url;
                        needUpdateNode.AppType = (int)node.AppType;
                        needUpdateNode.AuthType = (int)node.ConnectionType;
                        needUpdateNode.ModifiedDate = DateTime.UtcNow.Ticks;
                        needUpdateNode.UserName = string.IsNullOrWhiteSpace(node.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(node.UserName);
                        needUpdateNode.TeamId = node.TeamId;
                        needUpdateNode.SiteCollectionType = (int)node.SiteCollectionType;
                        needUpdateNode.DisplayName = node.DisplayName;
                    }
                }

                await _remoteNodeEvent.NotifyUpdateAsync(needUpdateNodes);
            }
        }

        public async Task<List<RMExchangeNodeAdaption>> GetExchangeNodesAsync(string containerId, string tenantId)
        {
            using var context = GetDbContext();

            var nodes = await context.RMMailboxes.Where(item => item.ParentId == containerId && item.TenantId == tenantId).ToListAsync();

            return nodes.ConvertAll(item => new RMExchangeNodeAdaption
            {
                Id = item.Id,
                ObjectId = item.ObjectId,
                TenantId = item.TenantId,
                ContainerId = item.ParentId,
                NodeLevel = (NodeLevel)item.NodeLevel,
                AppType = (AppType)item.AppType,
                ConnectionType = (BposConnectionType)item.AuthType,
                EmailAddress = item.Name,
                UserName = string.IsNullOrWhiteSpace(item.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.DecryptToString(item.UserName),
            });
        }

        public async Task AddExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes)
        {
            using var context = GetDbContext();

            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                using var transaction = context.Database.BeginTransaction();
                try
                {
                    var needDeleteNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).Select(item => item.Id);
                    /* Fortify Issue Type: SQL Injection
                     */
                    var inClauseParamName = DatabaseUtility.BuildInClause(needDeleteNodes, out var paramList);
                    var sql = $"DELETE FROM [{SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}].[RMMailboxes] WHERE Id IN {inClauseParamName}";

                    await context.Database.ExecuteSqlCommandAsync(sql, paramList.ToArray());

                    var needAddNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMMailbox
                    {
                        Id = item.Id,
                        ObjectId = item.ObjectId,
                        TenantId = item.TenantId,
                        ParentId = item.ContainerId,
                        NodeLevel = (int)item.NodeLevel,
                        AppType = (int)item.AppType,
                        AuthType = (int)item.ConnectionType,
                        Name = item.EmailAddress,
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedDate = DateTime.UtcNow.Ticks,
                        UserName = string.IsNullOrWhiteSpace(item.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(item.UserName),
                    });
                    context.RMMailboxes.AddRange(needAddNodes);
                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task DeleteExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes)
        {
            using var context = GetDbContext();

            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needDeleteNodes = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).ConvertAll(item => new RMMailbox
                {
                    Id = item.Id
                });

                needDeleteNodes.ForEach(item => context.Entry<RMMailbox>(item).State = EntityState.Deleted);

                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes)
        {
            using var context = GetDbContext();

            var nodesDic = nodes.ToDictionary(item => item.Id, item => item);

            for (var i = 0; i < nodes.Count(); i += S_OPERATION_BATCH_COUNT)
            {
                var needUpdateIds = nodes.Skip(i).Take(S_OPERATION_BATCH_COUNT).Select(item => item.Id);

                var needUpdateNodes = await context.RMMailboxes.Where(item => needUpdateIds.Contains(item.Id)).ToListAsync();

                foreach (var needUpdateNode in needUpdateNodes)
                {
                    if (nodesDic.TryGetValue(needUpdateNode.Id, out var node))
                    {
                        needUpdateNode.Name = node.EmailAddress;
                        needUpdateNode.AppType = (int)node.AppType;
                        needUpdateNode.AuthType = (int)node.ConnectionType;
                        needUpdateNode.ModifiedDate = DateTime.UtcNow.Ticks;
                        needUpdateNode.UserName = string.IsNullOrWhiteSpace(node.UserName) ? string.Empty : RMDatabaseDefaultEncryptor.EncryptToString(node.UserName);
                    }
                }

                context.RMMailboxes.AddOrUpdate(needUpdateNodes.ToArray());

                await context.SaveChangesAsync();
            }
        }

        public async Task<int> CountContainerAsync()
        {
            using var context = GetDbContext();
            return await context.RMRemoteNodes.Where(item => item.NodeLevel == (int)NodeLevel.WebApplication)
                .CountAsync();
        }

        public async Task<int> CountSiteAsync()
        {
            using var context = GetDbContext();
            return await context.RMRemoteNodes.Where(item => item.NodeLevel == (int)NodeLevel.SiteCollection).CountAsync();
        }

        public async Task<int> CountSiteAsync(IEnumerable<Guid> containerIds)
        {
            var count = 0;
            var containers = containerIds.ToList();
            using var context = GetDbContext();
            for (var i = 0; i < containers.Count; i += 100)
            {
                var batch = containerIds.Skip(i).Take(100).ConvertAll(item => item.ToString()).ToHashSet();
                var siteCount = await context.RMRemoteNodes.Where(item =>
                    batch.Contains(item.ParentId) &&
                    item.NodeLevel == (int)NodeLevel.SiteCollection
                ).CountAsync();
                count += siteCount;
            }

            return count;
        }

        public async Task<List<RMRemoteNode>> GetContainersAsync()
        {
            using var context = GetDbContext();
            return await context.RMRemoteNodes.Where(item => item.NodeLevel == (int)NodeLevel.WebApplication)
                .ToListAsync();
        }

        public async Task<RMRemoteNode> GetContainerAsync(Guid containerId)
        {
            using var context = GetDbContext();
            return await context.RMRemoteNodes.FirstAsync(item => item.Id == containerId.ToString());
        }

        public async IAsyncEnumerable<RMRemoteNode> GetSitesAsync(Guid containerId)
        {
            var batchCount = 1000;

            using var context = GetDbContext();
            for (var i = 0; ; i += batchCount)
            {
                var sites = await context.RMRemoteNodes.Where(item => item.ParentId == containerId.ToString()
                && item.NodeLevel == (int)NodeLevel.SiteCollection)
                    .OrderBy(item => item.Id)
                    .Skip(i).Take(batchCount)
                .ToListAsync();
                foreach (var site in sites)
                {
                    yield return site;
                }
                if (sites.Count < batchCount)
                {
                    yield break;
                }
            }
        }

        public async Task<bool> HasAnySites()
        {
            return await _remoteNodeSynchronizeDao.HasAnySites();
        }

        public void InjectRemoteNodeSynchronizeEvent(IRemoteNodeEvent remoteNodeEvent)
        {
            _remoteNodeEvent = remoteNodeEvent;
        }

        private static RMDbContext GetDbContext()
        {
            var context = RMDBContextManager.GetNewDBContext();
            context.Database.CommandTimeout = 600;

            return context;
        }
    }
}

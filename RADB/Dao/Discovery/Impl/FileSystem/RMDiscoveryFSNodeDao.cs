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
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem
{
    public class RMDiscoveryFSNodeDao : IRMDiscoveryFSNodeDao
    {
        public async Task AddOrUpdateDiscoveryConnectionAsync(params RMDiscoveryFSConnectionInfo[] connections)
        {
            if (!connections.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            efContext.FSConnectionInfoes.AddOrUpdate(connections);
            await efContext.SaveChangesAsync();
        }

        public async Task<int> CalculateConnectionCount(List<Guid> groupIds)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var count = 0;
            var groups = groupIds.ToList();
            for (var i = 0; i < groups.Count; i += 100)
            {
                var batch = groupIds.Skip(i).Take(100).ToHashSet();
                var connectionCount = await context.FSConnection
                    .Where(item => batch.Contains(item.GroupId) && item.Name != null)
                    .CountAsync();
                count += connectionCount;
            }
            return count;
        }

        public async Task<List<RMDiscoveryFSContainerInfo>> GetAllDiscoveryContainersAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            return await efContext.FSContainerInfoes.ToListAsync();
        }

        public Task<IAsyncEnumerable<FSConnection>> GetConnectionByGroupIdAsync(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<FSConnectionGroup> GetConnectionGroupsById(Guid groupId)
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             where g.Id == groupId
                             orderby g.LastModifiedTime descending
                             //from gc in GroupAndConnection.DefaultIfEmpty()
                             select new
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from memebership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on memebership.AgentId equals agent.Id
                                          where memebership.ConnectionGroupId == g.Id
                                          select agent
                             };

                var item = groups.First();
                var group = new FSConnectionGroup();
                group.FSConnections = new List<FSConnection>();
                group.Agents = new List<RMAgent>();
                group.Id = item.Group.Id;
                group.Name = item.Group.Name;
                group.Description = item.Group.Description;
                group.LastModifiedTime = item.Group.LastModifiedTime;
                group.AccessConnectionType = item.Group.AccessConnectionType;
                foreach (var conn in item.FSConnections)
                {
                    group.FSConnections.Add(new FSConnection()
                    {
                        Id = conn.Id,
                        Name = conn.Name,
                        Description = conn.Description,
                        LastModifiedTime = conn.LastModifiedTime,
                        GroupId = conn.GroupId,
                        UNCPath = conn.UNCPath,
                        AgentId = conn.AgentId
                    });
                }

                foreach (var agent in item.Agents)
                {
                    group.Agents.Add(new RMAgent
                    {
                        Id = agent.Id,
                        Name = agent.Name,
                        SourceType = agent.SourceType,
                        Status = agent.Status
                    });
                }
                return group;
            }
        }

        public async Task<List<FSConnectionGroup>> GetConnectionGroupsByIds(List<Guid> groupIds)
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                return await ctx.FSConnectionGroup.Where(i => groupIds.Contains(i.Id)).ToListAsync();
            }
        }

        public async Task<RMDiscoveryFSConnectionInfo> GetDiscoveryConnectionInfoAsync(Guid connectionId)
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            return await efContext.FSConnectionInfoes.FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }

        public async Task<List<FSConnectionGroup>> LoadAllGroupsWithConnection()
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var result = new List<FSConnectionGroup>();
                var groups = from g in ctx.FSConnectionGroup
                                 join c in ctx.FSConnection
                                 on g.Id equals c.GroupId
                                 into GroupAndConnection
                                 orderby g.LastModifiedTime descending
                                 select new
                                 {
                                     Group = g,
                                     FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                     Agents = from memebership in ctx.FSConnectionGroupWithAgentMembership
                                              join agent in ctx.RMAgent
                                              on memebership.AgentId equals agent.Id
                                              where memebership.ConnectionGroupId == g.Id
                                              select agent
                                 };
                    foreach (var item in await groups.ToListAsync())
                    {
                        var group = new FSConnectionGroup();
                        group.FSConnections = new List<FSConnection>();
                        group.Agents = new List<RMAgent>();
                        group.Id = item.Group.Id;
                        group.Name = item.Group.Name;
                        group.Description = item.Group.Description;
                        group.LastModifiedTime = item.Group.LastModifiedTime;
                        group.AccessConnectionType = item.Group.AccessConnectionType;
                        foreach (var conn in item.FSConnections)
                        {
                            group.FSConnections.Add(new FSConnection()
                            {
                                Id = conn.Id,
                                Name = conn.Name,
                                Description = conn.Description,
                                LastModifiedTime = conn.LastModifiedTime,
                                GroupId = conn.GroupId,
                                UNCPath = conn.UNCPath,
                                AgentId = conn.AgentId
                            });
                        }

                        foreach (var agent in item.Agents)
                        {
                            group.Agents.Add(new RMAgent
                            {
                                Id = agent.Id,
                                Name = agent.Name,
                                SourceType = agent.SourceType,
                                Status = agent.Status
                            });
                        }
                        result.Add(group);
                    }
                return result;
            }
        }

        public List<FSConnectionGroup> LoadAllGroupsWithoutConnection()
        {
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                int pageSize = 2000;
                int pageIndex = 0;
                List<FSConnectionGroup> allData = new();
                List<FSConnectionGroup> batchData;
                do
                {
                    batchData = ctx.FSConnectionGroup
                                 .AsNoTracking()
                                 .OrderBy(d => d.Name)
                                 .Skip(pageIndex * pageSize)
                                 .Take(pageSize)
                                 .ToList();
                    allData.AddRange(batchData);
                    pageIndex++;
                } while (batchData.Count == pageSize); 
                return allData;
            }
        }

        public async Task<(bool has, RMDiscoveryFSContainerInfo containerInfo)> TryGetDiscoveryContainerByOpusIdAsync(Guid opusId)
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            var res = await efContext.FSContainerInfoes.FirstOrDefaultAsync(item => item.OpusId == opusId);
            return (res != null, res);
        }

        public async Task<List<FSConnectionGroup>> LoadGroupsWithConnectionByIds(List<Guid> groupIds, int batchSize = 100)
        {
            var batches = groupIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();
            using (var ctx = RMDBContextManager.GetNewDBContext())
            {
                var result = new List<FSConnectionGroup>();
                foreach (var batch in batches)
                {
                    var groups = from g in ctx.FSConnectionGroup
                                 where batch.Contains(g.Id)
                                 join c in ctx.FSConnection
                                 on g.Id equals c.GroupId
                                 into GroupAndConnection
                                 orderby g.LastModifiedTime descending
                                 select new
                                 {
                                     Group = g,
                                     FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                     Agents = from memebership in ctx.FSConnectionGroupWithAgentMembership
                                              join agent in ctx.RMAgent
                                              on memebership.AgentId equals agent.Id
                                              where memebership.ConnectionGroupId == g.Id
                                              select agent
                                 };

                    var groupWithConnection = await groups.ToListAsync();
                    foreach (var item in groupWithConnection)
                    {
                        var group = new FSConnectionGroup();
                        group.FSConnections = new List<FSConnection>();
                        group.Agents = new List<RMAgent>();
                        group.Id = item.Group.Id;
                        group.Name = item.Group.Name;
                        group.Description = item.Group.Description;
                        group.LastModifiedTime = item.Group.LastModifiedTime;
                        group.AccessConnectionType = item.Group.AccessConnectionType;
                        foreach (var conn in item.FSConnections)
                        {
                            group.FSConnections.Add(new FSConnection()
                            {
                                Id = conn.Id,
                                Name = conn.Name,
                                Description = conn.Description,
                                LastModifiedTime = conn.LastModifiedTime,
                                GroupId = conn.GroupId,
                                UNCPath = conn.UNCPath,
                                AgentId = conn.AgentId
                            });
                        }

                        foreach (var agent in item.Agents)
                        {
                            group.Agents.Add(new RMAgent
                            {
                                Id = agent.Id,
                                Name = agent.Name,
                                SourceType = agent.SourceType,
                                Status = agent.Status
                            });
                        }
                        result.Add(group);
                    }
                }
                return result;
            }
        }

        public async Task AddOrUpdateDiscoveryContainerAsync(params RMDiscoveryFSContainerInfo[] containers)
        {
            if (!containers.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            efContext.FSContainerInfoes.AddOrUpdate(containers);
            await efContext.SaveChangesAsync();
        }

        public async Task<int> CountDiscoveryConnectionAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetFileSystemEFContextAsync();
            return await efContext.FSConnectionInfoes.CountAsync();
        }
    }
}

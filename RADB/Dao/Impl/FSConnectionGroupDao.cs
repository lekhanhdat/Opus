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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSConnectionGroupDao : BaseDao<FSConnectionGroup>, IFSConnectionGroupDao
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(FSConnectionGroupDao));
        private List<FSConnectionGroup> LoadAllGroupsForJPMC()
        {
            using var ctx = GetNewContext();
            var groups = ctx.FSConnectionGroup
                .GroupJoin(
                    ctx.FSConnection,
                    g => g.Id,
                    c => c.GroupId,
                    (g, groupAndConnection) => new GroupProjection
                    {
                        Group = g,
                        FSConnections = groupAndConnection
                            .Where(conn => conn.GroupId == g.Id),
                        Agents = ctx.FSConnectionGroupWithAgentMembership
                            .Join(
                                ctx.RMAgent,
                                membership => membership.AgentId,
                                agent => agent.Id,
                                (membership, agent) => new { membership, agent })
                            .Where(x => x.membership.ConnectionGroupId == g.Id)
                            .Select(x => x.agent)
                    })
                .OrderByDescending(x => x.Group.LastModifiedTime);
            return MapToFSConnectionGroups(groups.ToList());
        }

        private List<FSConnectionGroup> LoadAllGroupsByDCInternalNameForJPMC(string DCInternalName)
        {
            using var ctx = GetNewContext();
            var groups = ctx.FSConnectionGroup.AsNoTracking().Where(g => DCInternalName.Equals(g.DCInternalName))
                .GroupJoin(
                    ctx.FSConnection,
                    g => g.Id,
                    c => c.GroupId,
                    (g, groupAndConnection) => new GroupProjection
                    {
                        Group = g,
                        FSConnections = groupAndConnection
                            .Where(conn => conn.GroupId == g.Id),
                        Agents = ctx.FSConnectionGroupWithAgentMembership
                            .Join(
                                ctx.RMAgent,
                                membership => membership.AgentId,
                                agent => agent.Id,
                                (membership, agent) => new { membership, agent })
                            .Where(x => x.membership.ConnectionGroupId == g.Id)
                            .Select(x => x.agent)
                    })
                .OrderByDescending(x => x.Group.LastModifiedTime);
            return MapToFSConnectionGroups(groups.ToList());
        }

        private List<FSConnectionGroup> LoadAllGroupsOfMainDCForJPMC()
        {
            using var ctx = GetNewContext();
            var groups = ctx.FSConnectionGroup.AsNoTracking().Where(g => string.IsNullOrEmpty(g.DCInternalName))
                .GroupJoin(
                    ctx.FSConnection,
                    g => g.Id,
                    c => c.GroupId,
                    (g, groupAndConnection) => new GroupProjection
                    {
                        Group = g,
                        FSConnections = groupAndConnection
                            .Where(conn => conn.GroupId == g.Id)
                            .Take(1),
                        Agents = ctx.FSConnectionGroupWithAgentMembership
                            .Join(
                                ctx.RMAgent,
                                membership => membership.AgentId,
                                agent => agent.Id,
                                (membership, agent) => new { membership, agent })
                            .Where(x => x.membership.ConnectionGroupId == g.Id)
                            .Select(x => x.agent)
                    })
                .OrderByDescending(x => x.Group.LastModifiedTime);
            return MapToFSConnectionGroups(groups.ToList());
        }

        public FSConnectionGroup GetGroup(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             where g.Id == groupId
                             orderby g.LastModifiedTime descending
                             select new GroupProjection
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from membership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on membership.AgentId equals agent.Id
                                          where membership.ConnectionGroupId == g.Id
                                          select agent
                             };

                return MapToFSConnectionGroup(groups.First());
            }
        }

        public FSConnectionGroup GetGroupOrNull(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             where g.Id == groupId
                             orderby g.LastModifiedTime descending
                             select new GroupProjection
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from membership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on membership.AgentId equals agent.Id
                                          where membership.ConnectionGroupId == g.Id
                                          select agent
                             };

                var group = groups.FirstOrDefault();
                return group == null ? null : MapToFSConnectionGroup(group);
            }
        }

        public async Task<List<FSConnectionGroup>> FsConnectionGroupWithSearchKey(string searchKey)
        {
            var result = new List<FSConnectionGroup>();
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             where g.Name.Contains(searchKey)
                             orderby g.LastModifiedTime descending
                             select g;
                foreach (var item in groups)
                {
                    result.Add(new FSConnectionGroup()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        LastModifiedTime = item.LastModifiedTime,
                        AccessConnectionType = item.AccessConnectionType
                    });
                }
                return result;
            }
        }

        public async Task<List<FSConnectionGroup>> FsConnectionGroupWithSearchKeyAndId(string searchKey, IEnumerable<Guid> groupIds)
        {
            var result = new List<FSConnectionGroup>();
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             where g.Name.Contains(searchKey) && groupIds.Contains(g.Id)
                             orderby g.LastModifiedTime descending
                             select g;
                foreach (var item in groups)
                {
                    result.Add(new FSConnectionGroup()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        LastModifiedTime = item.LastModifiedTime,
                        AccessConnectionType = item.AccessConnectionType
                    });
                }
                return result;
            }
        }

        public List<FSConnectionGroup> LoadAllGroups()
        {
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return LoadAllGroupsForJPMC();
            }
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             orderby g.LastModifiedTime descending
                             select new GroupProjection
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from membership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on membership.AgentId equals agent.Id
                                          where membership.ConnectionGroupId == g.Id
                                          select agent
                             };

                return MapToFSConnectionGroups(groups.ToList());
            }
        }

        public List<FSConnectionGroup> LoadAllGroupsByDCInternalName(string DCInternalName)
        {
            if (DCInternalName == null) return new();
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return LoadAllGroupsByDCInternalNameForJPMC(DCInternalName);
            }
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             where DCInternalName.Equals(g.DCInternalName)
                             orderby g.LastModifiedTime descending
                             select new GroupProjection
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from membership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on membership.AgentId equals agent.Id
                                          where membership.ConnectionGroupId == g.Id
                                          select agent
                             };

                return MapToFSConnectionGroups(groups.ToList());
            }
        }

        public List<FSConnectionGroup> LoadAllGroupsOfMainDC()
        {
            if (RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return LoadAllGroupsOfMainDCForJPMC();
            }
            using (var ctx = GetNewContext())
            {
                var groups = from g in ctx.FSConnectionGroup
                             join c in ctx.FSConnection
                             on g.Id equals c.GroupId
                             into GroupAndConnection
                             where string.IsNullOrEmpty(g.DCInternalName)
                             orderby g.LastModifiedTime descending
                             select new GroupProjection
                             {
                                 Group = g,
                                 FSConnections = from conn in GroupAndConnection where conn.GroupId == g.Id select conn,
                                 Agents = from membership in ctx.FSConnectionGroupWithAgentMembership
                                          join agent in ctx.RMAgent
                                          on membership.AgentId equals agent.Id
                                          where membership.ConnectionGroupId == g.Id
                                          select agent
                             };

                return MapToFSConnectionGroups(groups.ToList());
            }
        }

        public List<string> LoadAllConnectionGroupNames()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroup.AsNoTracking().Select(g => g.Name).ToList();
            }
        }

        public List<string> LoadAllConnectionGroupNamesByDCInternalName(string DCInternalName)
        {
            using var ctx = GetNewContext();
            if (DCInternalName == null) return new();
            return ctx.FSConnectionGroup.AsNoTracking().Where(g => DCInternalName.Equals(g.DCInternalName)).Select(g => g.Name).ToList();
        }

        public IEnumerable<Guid> LoadAllConnectionGroupIdByDCInternalName(string DCInternalName)
        {
            using var ctx = GetNewContext();
            if (DCInternalName == null) return Enumerable.Empty<Guid>();
            return ctx.FSConnectionGroup.AsNoTracking().Where(g => DCInternalName.Equals(g.DCInternalName)).Select(g => g.Id).ToList();
        }

        public IEnumerable<Guid> LoadAllConnectionGroupIdOfMainDC()
        {
            using var ctx = GetNewContext();
            return ctx.FSConnectionGroup.AsNoTracking().Where(g => string.IsNullOrEmpty(g.DCInternalName)).Select(g => g.Id).ToList();
        }

        public FSConnectionGroup GetGroupById(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroup.FirstOrDefault(g => g.Id == groupId);
            }
        }
        public FSConnectionGroup GetGroupByName(string groupName)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroup.FirstOrDefault(g => g.Name == groupName);
            }
        }

        public List<FSConnectionGroup> GetGroupByIds(List<Guid> groupIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroup.Where(g => groupIds.Contains(g.Id)).ToList();
            }
        }

        public async Task<bool> SaveConnectionGroupAsync(FSConnectionGroup connectionGroup)
        {
            using (var ctx = GetNewContext())
            {
                if (ctx.FSConnectionGroup.Any(g => g.Id != connectionGroup.Id && g.Name == connectionGroup.Name))
                {
                    throw new Exception(I18NEntity.GetString("RM_FS_Register_SameGroupNameErrorMessage"));
                }
                var exist = ctx.FSConnectionGroup.Where(g => g.Id == connectionGroup.Id).FirstOrDefault();
                if (exist == null)
                {
                    ctx.FSConnectionGroup.Add(connectionGroup);
                    return ctx.SaveChanges() > 0;
                }
                else
                {
                    connectionGroup.DCInternalName = exist.DCInternalName;
                    return await this.UpdateAsync(connectionGroup);
                }
            }
        }

        public void DeleteGroupConnectoin(Guid groupId)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    base.DeleteByKey(groupId);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AccessConnectionType GetTypeByGroupId(Guid groupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.FSConnectionGroup.Where(g => g.Id == groupId).Select(f => f.AccessConnectionType).FirstOrDefault();
            }
        }

        private List<FSConnectionGroup> MapToFSConnectionGroups(List<GroupProjection> projections)
        {
            return projections.Select(MapToFSConnectionGroup).ToList();
        }

        private FSConnectionGroup MapToFSConnectionGroup(GroupProjection item)
        {
            var group = new FSConnectionGroup
            {
                Id = item.Group.Id,
                Name = item.Group.Name,
                Description = item.Group.Description,
                LastModifiedTime = item.Group.LastModifiedTime,
                AccessConnectionType = item.Group.AccessConnectionType,
                DCInternalName = item.Group.DCInternalName,
                FSConnections = MapConnections(item.FSConnections),
                Agents = MapAgents(item.Agents)
            };

            return group;
        }

        private List<FSConnection> MapConnections(IEnumerable<FSConnection> connections)
        {
            if (connections == null || !connections.Any())
            {
                return new List<FSConnection>();
            }

            using (var ctx = GetNewContext())
            {
                var connIds = connections.Select(c => c.Id).ToList();

                var failureJobCounts = ctx.FSConnectionRelatedJobInfoes
                    .Where(r => r.EndTime > 0
                        && connIds.Contains(r.ConnectionId)
                        && (r.Status == (int)JobStatus.Failed || r.Status == (int)JobStatus.FinishWithException)
                        && ctx.JobMonitors.Any(j => j.Id == r.JobId))
                    .GroupBy(r => r.ConnectionId)
                    .Select(g => new { ConnectionId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.ConnectionId, x => x.Count);

                var connectionList = connections.ToList();

                foreach (var conn in connectionList)
                {
                    conn.FailureJobCount = failureJobCounts.TryGetValue(conn.Id, out var count) ? count : 0;
                }

                return connectionList;
            }
        }

        private static List<RMAgent> MapAgents(IEnumerable<RMAgent> agents)
        {
            return agents.Select(agent => new RMAgent
            {
                Id = agent.Id,
                Name = agent.Name,
                SourceType = agent.SourceType,
                Status = agent.Status,
                DCInternalName = agent.DCInternalName
            }).ToList();
        }

        private class GroupProjection
        {
            public FSConnectionGroup Group { get; set; }
            public IEnumerable<FSConnection> FSConnections { get; set; }
            public IEnumerable<RMAgent> Agents { get; set; }
        }

        public async Task<IEnumerable<FSConnectionGroup>> LoadByPager(int pageIndex, int pageSize)
        {
            using var ctx = GetNewContext();
            return await ctx.FSConnectionGroup.AsNoTracking().OrderBy(group => group.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<long> MultiGeoInsertFSConnectionGroupTableAsync(IEnumerable<FSConnectionGroup> fSConnectionGroups)
        {
            using var context = GetNewContext();
            try
            {
                context.FSConnectionGroup.AddRange(fSConnectionGroups);
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Insert FSConnectionGroups data has error: {ex}");
                return 0;
            }
        }

        public async Task<long> MultiGeoDeleteAllFSConnectionGroupAsync()
        {
            return await TruncateAllDataInTableAsync("FSConnectionGroups");
        }

        public async Task<string> GetGroupDCInternalNameByConnectionId(Guid connectionId)
        {
            using var ctx = GetNewContext();
            return await ctx.FSConnection
                    .AsNoTracking()
                    .Where(c => c.Id == connectionId)
                    .Join(ctx.FSConnectionGroup,
                          c => c.GroupId,
                          g => g.Id,
                          (c, g) => g.DCInternalName)
                    .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, IEnumerable<string>>> GetGroupDCInternalNameByConnectionIdsAsync(IEnumerable<string> connectionIds)
        {
            if (connectionIds == null) return new Dictionary<string, IEnumerable<string>>();

            var connectionIdGuids = connectionIds
                .Where(id => !string.IsNullOrEmpty(id))
                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                .Where(guid => guid != Guid.Empty)
                .Distinct()
                .ToList();

            if (connectionIdGuids.Count == 0) return new Dictionary<string, IEnumerable<string>>();

            using var ctx = GetNewContext();

            var queryResult = await ctx.FSConnection
                .AsNoTracking()
                .Where(c => connectionIdGuids.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    DCInternalName = ctx.FSConnectionGroup.Where(g => g.Id == c.GroupId).Select(g => g.DCInternalName).FirstOrDefault()
                })
                .ToListAsync();

            return queryResult
                .GroupBy(x => string.IsNullOrEmpty(x.DCInternalName) ? string.Empty : x.DCInternalName) 
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(y => y.Id.ToString()).AsEnumerable()
                );
        }

        public async Task<(bool, string)> CheckPathAndGetDCInternalName(string fullPath)
        {
            using var ctx = GetNewContext();
            var group = ctx.FSConnectionGroup.AsNoTracking().FirstOrDefault(g => g.Name == fullPath);
            if (group != null)
            {
                return (true, group.DCInternalName);
            }
            var connection = await ctx.FSConnection
                    .AsNoTracking()
                    .Where(g => g.UNCPath.Equals(fullPath) || fullPath.StartsWith(g.UNCPath + "\\")).FirstOrDefaultAsync();
            if(connection != null)
            {
                var connectionGroup = await ctx.FSConnectionGroup
                    .AsNoTracking()
                    .Where(g => g.Id == connection.GroupId)
                    .FirstOrDefaultAsync();
                if(connectionGroup != null)
                {
                    return (true, connectionGroup.DCInternalName);
                }
            }
            return (false, string.Empty);

        }

    }
}

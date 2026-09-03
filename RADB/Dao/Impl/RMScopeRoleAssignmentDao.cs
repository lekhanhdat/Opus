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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMScopeRoleAssignmentDao : BaseDao<RMScopeRoleAssignment>, IRMScopeRoleAssignmentDao
    {
        //private RALogger logger = RALogger.GetInstance(typeof(RMScopeRoleAssignmentDao));
        public void CreateOrUpdateScopePermission(int groupId, Dictionary<int, List<Guid>> scopePermissions)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var allContainers = context.RMScopeRoleAssignment.RemoveRange(context.RMScopeRoleAssignment.Where(g => g.GroupId == groupId).ToList());

                    context.SaveChanges();
                    var allAddScopes = new List<RMScopeRoleAssignment>();
                    foreach (var source in scopePermissions.Keys)
                    {
                        foreach (var scopeid in scopePermissions[source])
                        {
                            allAddScopes.Add(new RMScopeRoleAssignment()
                            {
                                DataSourceType = source,
                                GroupId = groupId,
                                ScopeId = scopeid,
                            });
                        }
                    }
                    context.RMScopeRoleAssignment.AddRange(allAddScopes);
                    context.SaveChanges();
                    tran.Commit();
                }
            }

        }

        public void AddScopePermission(int groupId, List<Guid> scopeIds, SourceFlag source)
        {
            using var context = GetNewContext();
            var allAddScopes = new List<RMScopeRoleAssignment>();
            foreach(var scopeId in scopeIds)
            {
                allAddScopes.Add(new RMScopeRoleAssignment()
                {
                    DataSourceType = (int)source,
                    GroupId = groupId,
                    ScopeId = scopeId,
                });
            }
            context.RMScopeRoleAssignment.AddRange(allAddScopes);
            context.SaveChanges();
        }

        public List<Guid> GetAllContainersByGroupDataSource(List<int> groupIds, int dataSource)
        {
            using (var context = GetNewContext())
            {
                var scopeids = context.RMScopeRoleAssignment.Where(t => groupIds.Contains(t.GroupId) && dataSource == t.DataSourceType).Select(t => t.ScopeId).Distinct().ToList();
                return scopeids;
            }
        }

        public List<int> GetAllGroupsByContainerId(List<Guid> containerIds, int dataSource)
        {
            List<int> groupIds = new List<int>();
            using (var context = GetNewContext())
            {
                var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
                if (defaultContianerIdSources.Contains((SourceFlag)dataSource))
                {
                    if (HasUpgradeTeams(context) && dataSource == (int)SourceFlag.SharePoint)
                    {
                        var realSPScopeIds = GetRemoteSPNodeByScopeIds(context).Select(_ => _.Id).ToList();
                        groupIds = context.RMScopeRoleAssignment.Where(t => containerIds.Contains(t.ScopeId) && dataSource == t.DataSourceType && realSPScopeIds.Contains(t.ScopeId.ToString())).Select(t => t.GroupId).Distinct().ToList();
                    }
                    else
                    {
                        groupIds = context.RMScopeRoleAssignment.Where(t => containerIds.Contains(t.ScopeId) && dataSource == t.DataSourceType).Select(t => t.GroupId).Distinct().ToList();
                    }
                }
            }
            return groupIds;
        }

        public Dictionary<Guid, IGrouping<Guid, RMScopeRoleAssignment>> GetAllScopeRoleByContainerId(List<Guid> containerIds, int dataSource)
        {
            var scopeRole = new Dictionary<Guid, IGrouping<Guid, RMScopeRoleAssignment>>();
            using (var context = GetNewContext())
            {
                var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
                if (defaultContianerIdSources.Contains((SourceFlag)dataSource))
                {
                    if(HasUpgradeTeams(context) && dataSource == (int)SourceFlag.SharePoint)
                    {
                        var realSPScopeIds = GetRemoteSPNodeByScopeIds(context).Select(_ => _.Id).ToList();
                        scopeRole = context.RMScopeRoleAssignment.Where(t => containerIds.Contains(t.ScopeId) && dataSource == t.DataSourceType && realSPScopeIds.Contains(t.ScopeId.ToString())).GroupBy(t => t.ScopeId).ToDictionary(t => t.Key);
                    }
                    else
                    {
                        scopeRole = context.RMScopeRoleAssignment.Where(t => containerIds.Contains(t.ScopeId) && dataSource == t.DataSourceType).GroupBy(t => t.ScopeId).ToDictionary(t => t.Key);
                    }
                }
            }
            return scopeRole;
        }
        public List<RMScopeRoleAssignment> GetAllScopeRoleByContainerIds(List<Guid> containerIds)
        {
            using (var context = GetNewContext())
            {
                return context.RMScopeRoleAssignment.Where(t => containerIds.Contains(t.ScopeId)).ToList();
            }
        }

        public async Task<Dictionary<int, List<Guid>>> GetAllContainersByUsersAsync(List<string> users)
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(users, out userParas);

                string getAllUserContainersQuery = string.Format(@"select p.DataSourceType,p.ScopeId from {0}.RMScopeRoleAssignments as p where p.GroupId in (select s.GroupId from {0}.RMSecurityGroupMemberships as s where s.UserId in {1})", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);
                var queryResult = await context.Database.SqlQuery<SourceScopeId>(getAllUserContainersQuery, userParas.ToArray()).ToListAsync();
                var result = new Dictionary<int, List<Guid>>();
                if(HasUpgradeTeams(context))
                {
                    var realSPScopeIds = GetRemoteSPNodeByScopeIds(context).Select(_ => _.Id).ToList();
                    var filterRealSPQuery = queryResult.Where(_ => _.DataSourceType != (int)SourceFlag.SharePoint || realSPScopeIds.Contains(_.ScopeId.ToString()));
                    foreach (SourceScopeId p in filterRealSPQuery)
                    {
                        if (result.Keys.Contains(p.DataSourceType))
                        {
                            result[p.DataSourceType].Add(p.ScopeId);
                        }
                        else
                        {
                            result.Add(p.DataSourceType, new List<Guid>() { p.ScopeId });
                        }
                    }
                }
                else
                {
                    foreach (SourceScopeId p in queryResult)
                    {
                        if (result.Keys.Contains(p.DataSourceType))
                        {
                            result[p.DataSourceType].Add(p.ScopeId);
                        }
                        else
                        {
                            result.Add(p.DataSourceType, new List<Guid>() { p.ScopeId });
                        }
                    }
                }
                return result;
            }
        }

        public List<Guid> GetContainersByUsers(List<string> users, SourceFlag sourceType = SourceFlag.All)
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(users, out userParas);

                string getAllUserContainersQuery = string.Format(
@"select p.ScopeId from {0}.RMScopeRoleAssignments as p 
  where p.GroupId in (select s.GroupId from {0}.RMSecurityGroupMemberships as s where s.UserId in {1})",
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);
                if(sourceType != SourceFlag.All)
                {
                    getAllUserContainersQuery += $" and p.DataSourceType={(int)sourceType}";
                }
                return context.Database.SqlQuery<Guid>(getAllUserContainersQuery, userParas.ToArray()).ToList();
            }
        }

        public List<int> GetSourceFlagsByUser(List<string> users)
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(users, out userParas);

                string getAllUserContainersQuery = string.Format(
                    @"select p.DataSourceType from {0}.RMScopeRoleAssignments as p 
                      where p.GroupId in (select s.GroupId from {0}.RMSecurityGroupMemberships as s where s.UserId in {1})",
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);
                return context.Database.SqlQuery<int>(getAllUserContainersQuery, userParas.ToArray()).ToList();
            }
        }


        public bool HavePermissionOnContainerId(Guid containerId, List<string> users)
        {
            bool result = false;
            using (var context = GetNewContext())
            {
                List<SqlParameter> userParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(users, out userParas);
                SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                string getAllUserContainersQuery = string.Format(@"select p.DataSourceType,p.ScopeId from {0}.RMScopeRoleAssignments as p where p.ScopeId = @containerId and p.GroupId in (select s.GroupId from {0}.RMSecurityGroupMemberships as s where s.UserId in {1})", context.SchemaName, userParameterizedStatement);
                var queryResult = context.Database.SqlQuery<SourceScopeId>(getAllUserContainersQuery, new SqlParameter[] { new SqlParameter("containerId", containerId) }.Concat(userParas).ToArray()).ToList();
                if(HasUpgradeTeams(context))
                {
                    var realSPScopeIds = GetRemoteSPNodeByScopeIds(context).Select(_ => _.Id).ToList();
                    var filterRealSPQuery = queryResult.Where(_ => _.DataSourceType != (int)SourceFlag.SharePoint || realSPScopeIds.Contains(_.ScopeId.ToString()));
                    result = filterRealSPQuery.Count() > 0;
                }
                else
                {
                    result = queryResult.Count() > 0;
                }
            }
            return result;
        }

        public void RemoveAllPermisionsByDataSource(int groupId, List<int> dataSource)
        {
            using (var context = GetNewContext())
            {
                var removeObjs = context.RMScopeRoleAssignment.Where(t => t.GroupId.Equals(groupId) && dataSource.Contains(t.DataSourceType)).ToList();
                context.RMScopeRoleAssignment.RemoveRange(removeObjs);
                context.SaveChanges();
            }
        }

        public IList<SourceScopeId> QueryAllScopes()
        {
            using (var ctx = GetNewContext())
            {
                var query = from g in ctx.RMSecurityGroup.Where(o => o.IsRemoved == false).Select(o => o.Id)
                            join m in ctx.RMScopeRoleAssignment.Select(o => o)
                            on g equals m.GroupId
                            select new SourceScopeId { DataSourceType = m.DataSourceType, ScopeId = m.ScopeId };
                if(HasUpgradeTeams(ctx))
                {
                    var realSPScopeIds = GetRemoteSPNodeByScopeIds(ctx).Select(_ => _.Id).ToList();
                    var filterRealSPQuery = query.Where(_ => _.DataSourceType != (int)SourceFlag.SharePoint || realSPScopeIds.Contains(_.ScopeId.ToString()));
                    return filterRealSPQuery.ToList();
                }
                return query.ToList();
            }
        }

        public bool ValidateContainerIdPermission(List<string> containerIds, List<string> user)
        {
            bool result = true;
            using (var context = GetNewContext())
            {
                List<SqlParameter> containerIdsParas = null;
                var containerParameterizedStatement = DatabaseUtility.BuildInClause(containerIds, out containerIdsParas, 100);

                List<SqlParameter> userParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(user, out userParas, 200);

                string query = string.Format("select distinct R.ScopeId from {0}.RMScopeRoleAssignments as R where R.ScopeId in {1} and R.GroupId  in (select s.GroupId from {0}.RMSecurityGroupMemberships as s where s.UserId in {2})", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), containerParameterizedStatement, userParameterizedStatement);
                var queryResult = context.Database.SqlQuery<List<Guid>>(query, containerIdsParas.Concat(userParas).ToArray()).ToList();
                result = queryResult.Count() == containerIds.Count;
                return result;
            }
        }

        //public void RemoveScopePermission(int groupId, List<Guid> scopeids)
        //{
        //    throw new NotImplementedException();
        //}

        public int RemoveContainers(List<Guid> scopeIds)
        {
            using (var context = GetNewContext())
            {
                var entities = context.RMScopeRoleAssignment.Where(o => scopeIds.Contains(o.ScopeId)).ToList();
                context.RMScopeRoleAssignment.RemoveRange(entities);
                return context.SaveChanges();
            }
        }

        public void RemoveContainers(List<RMScopeRoleAssignment> scopeRoleAssignments)
        {
            using (var context = GetNewContext())
            {
                var entities = new List<RMScopeRoleAssignment>();
                foreach (var item in scopeRoleAssignments)
                {
                    entities.AddRange(context.RMScopeRoleAssignment.Where(ass => ass.GroupId == item.GroupId && ass.ScopeId == item.ScopeId));
                }
                context.RMScopeRoleAssignment.RemoveRange(entities);
                context.SaveChanges();
            }
        }

        private bool HasUpgradeTeams(Core.RMDbContext ctx)
        {
            var result = false;
            if (!EnableTeamsFeature(ctx)) return result;
            var key = KeyNameCollection.HasUpgradeTeams;
            var setting = ctx.RMKeyValue.FirstOrDefault(k => k.Key.Equals(key));
            if (setting == null) return result;
            bool.TryParse(setting.Value, out result);

            return result;
        }

        private bool EnableTeamsFeature(Core.RMDbContext ctx)
        {
            var key = KeyNameCollection.EnableTeamsFeature;
            var setting = ctx.RMKeyValue.FirstOrDefault(k => k.Key.Equals(key));
            if (setting == null) return true;

            bool.TryParse(setting.Value, out var result);
            return result;
        }

        private List<RMRemoteNode> GetRemoteSPNodeByScopeIds(Core.RMDbContext ctx)
        {
            return ctx.RMRemoteNodes.AsNoTracking().Where(_ => _.NodeLevel == 2).ToList();
        }

        public List<string> GetUserIdsByScopeIds(List<Guid> scopeIds, int dataSource)
        {
            if (scopeIds == null || scopeIds.Count == 0)
            {
                return new List<string>();
            }
            using (var context = GetNewContext())
            {
                var groupIds = context.RMScopeRoleAssignment.AsNoTracking().Where(x => x.DataSourceType == dataSource && scopeIds.Contains(x.ScopeId)).Select(x => x.GroupId).Distinct();

                return context.RMSecurityGroupMembership.AsNoTracking().Where(x => groupIds.Contains(x.GroupId)).Select(x => x.UserId).Distinct().ToList();
            }
        }
    }
    public class SourceScopeId
    {
        public int DataSourceType { get; set; }
        public Guid ScopeId { get; set; }
    }

}

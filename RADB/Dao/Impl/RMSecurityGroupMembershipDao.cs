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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSecurityGroupMembershipDao : BaseDao<RMSecurityGroupMembership>, IRMSecurityGroupMembershipDao
    {
       // private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMSecurityGroupMembershipDao));
        /// <summary>
        /// overwrite group user membership settings.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public List<RMSecurityGroupMembership> CreateOrUpdateGroupMemberShips(int groupId, List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var allUserMemberships = context.RMSecurityGroupMembership.Where(g => g.GroupId == groupId).ToList();
                    context.RMSecurityGroupMembership.RemoveRange(allUserMemberships);
                    context.SaveChanges();
                    var allAddUsers = new List<RMSecurityGroupMembership>();
                    if (userIds != null)
                    {
                        foreach (var user in userIds)
                        {
                            allAddUsers.Add(new RMSecurityGroupMembership()
                            {
                                GroupId = groupId,
                                UserId = user
                            });
                        }
                        context.RMSecurityGroupMembership.AddRange(allAddUsers);
                    }
                    context.SaveChanges();
                    tran.Commit();
                }
                return context.RMSecurityGroupMembership.Where(g => g.GroupId == groupId).ToList();
            }
        }

        public void AddUsersToGroupMemberShips(int groupId, List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var allUserMemberships = context.RMSecurityGroupMembership.Where(g => g.GroupId == groupId).ToList();
                    List<string> dbUserIds = allUserMemberships.Select(x => x.UserId).ToList();
                    var allAddUsers = new List<RMSecurityGroupMembership>();
                    foreach (var user in userIds)
                    {
                        if (!dbUserIds.Contains(user))
                        {
                            allAddUsers.Add(new RMSecurityGroupMembership()
                            {
                                GroupId = groupId,
                                UserId = user
                            });
                        }
                    }
                    context.RMSecurityGroupMembership.AddRange(allAddUsers);
                    context.SaveChanges();
                    tran.Commit();
                }
            }
        }

        public bool IsUserInGroup(int groupId, string userId)
        {
            using (var context = GetNewContext())
            {
                return context.RMSecurityGroupMembership.Any(o => o.GroupId == groupId && o.UserId == userId);
            }
        }
        public void AddUserToGroupMemberShips(int groupId, string userId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var exists = context.RMSecurityGroupMembership.Any(g => g.GroupId == groupId && g.UserId == userId);
                    if (!exists)
                    {
                        context.RMSecurityGroupMembership.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = groupId,
                            UserId = userId
                        });
                    }
                    context.SaveChanges();
                    tran.Commit();
                }
            }
        }

        public void AddOrUpdateAllSameUserToGroupMemberShips(int groupId, string userId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var userMemberships = context.RMSecurityGroupMembership.Where(g => g.UserId == userId).ToList();
                    if (!userMemberships.Any())
                    {
                        context.RMSecurityGroupMembership.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = groupId,
                            UserId = userId
                        });
                    }
                    else
                    {
                        foreach (var userMemberShip in userMemberships)
                        {
                            if (userMemberShip.GroupId != groupId)
                            {
                                userMemberShip.GroupId = groupId;
                                userMemberShip.UserId = userId;
                            }
                        }
                    }
                    context.SaveChanges();
                    tran.Commit();
                }
            }
        }
        public void AddOrUpdateUserToGroupMemberShips(int groupId, List<string> userIds)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var userMemberships = context.RMSecurityGroupMembership.Where(g => userIds.Contains(g.UserId) && g.GroupId == groupId).ToList();
                    List<string> dbUserIds = userMemberships.Select(x => x.UserId).ToList();
                    var needSyncUserIds = userIds.Where(u => !dbUserIds.Contains(u)).ToList();
                    foreach (var userId in needSyncUserIds)
                    {
                        context.RMSecurityGroupMembership.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = groupId,
                            UserId = userId
                        });
                    }
                    
                    context.SaveChanges();
                    tran.Commit();
                }
            }
        }
        //public void DeleteGroupMemberShips(int groupId)
        //{
        //    throw new NotImplementedException();
        //}
        /// <summary>
        /// get user belong to which groups, get all permissionmasks 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<long> GetAllRolesByUser(List<string> userIds)//change to user && group infos.
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userIdsParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(userIds, out userIdsParas, 100);
                string getAllUserPermissionQuery = string.Format(@"select r.PermissionMasks from {0}.RMRoles as r where r.RoleId in 
                                                 (select s.RoleId from {0}.RMSecurityGroups as s where s.Id in 
                                           (select m.GroupId from {0}.RMSecurityGroupMemberships as m where m.UserId in {1}))", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);

                var result = context.Database.SqlQuery<long>(getAllUserPermissionQuery, userIdsParas.ToArray()).ToList();
                //logger.Info($"get all user permission:{getAllUserPermissionQuery}, result:{result.Count}");
                return result;
            }

        }

        public List<PermissionMask> GetAllPermissoinsByUser(List<string> userIds)//change to user && group infos.
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userIdsParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(userIds, out userIdsParas, 100);
                string getAllUserPermissionQuery = string.Format(@"select r.PermissionMasks,r.SubPermission1,r.PermissionExtensionMasks,r.SOPermissionMasks, r.DiscoveryPermissionMasks, r.SalesforceDiscoveryPermissionMasks, r.GoogleROTDiscoveryPermissionMasks, r.FSDiscoveryPermissionMasks, r.ReportingPermission from {0}.RMRoles as r where r.RoleId in 
                                                 (select s.RoleId from {0}.RMSecurityGroups as s where s.Id in 
                                           (select m.GroupId from {0}.RMSecurityGroupMemberships as m where m.UserId in {1}))", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);

                var result = context.Database.SqlQuery<PermissionMask>(getAllUserPermissionQuery, userIdsParas.ToArray()).ToList();
                //logger.Info($"get all user permission:{getAllUserPermissionQuery}, result:{result.Count}");
                return result;
            }

        }
        public List<bool> GetAllGroupStatusByUser(List<string> userIds)//change to user && group infos.
        {
            using (var context = GetNewContext())
            {
                List<SqlParameter> userIdsParas = null;
                var userParameterizedStatement = DatabaseUtility.BuildInClause(userIds, out userIdsParas, 100);
                string getAllUserPermissionQuery = string.Format(@"select r.IsNewGroup from {0}.RMRoles as r where r.RoleId in 
                                                 (select s.RoleId from {0}.RMSecurityGroups as s where s.Id in 
                                           (select m.GroupId from {0}.RMSecurityGroupMemberships as m where m.UserId in {1}))", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);

                var result = context.Database.SqlQuery<bool>(getAllUserPermissionQuery, userIdsParas.ToArray()).ToList();
                //logger.Info($"get all user permission:{getAllUserPermissionQuery}, result:{result.Count}");
                return result;
            }

        }
        public List<long> GetSubPermissionMasksByUser(List<string> userIds)
        {
            //string nodeIdInClause = DatabaseUtility.BuildInClause(userIds);
            List<SqlParameter> userIdsParas = null;
            var userParameterizedStatement = DatabaseUtility.BuildInClause(userIds, out userIdsParas, 100);
            using (var context = GetNewContext())
            {
                string getAllUserSubPermissionQuery = string.Format(@"select r.SubPermission1 from {0}.RMRoles as r where r.RoleId in 
                                                 (select s.RoleId from {0}.RMSecurityGroups as s where s.Id in 
                                           (select m.GroupId from {0}.RMSecurityGroupMemberships as m where m.UserId in {1}))", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), userParameterizedStatement);

                var result = context.Database.SqlQuery<long>(getAllUserSubPermissionQuery, userIdsParas.ToArray()).ToList();
                return result;
            }

        }
        public void RemoveUserGroupMemeberships(int groupId, string userId)
        {
            using (var context = GetNewContext())
            {
                var memberships = context.RMSecurityGroupMembership.Where(m => m.GroupId == groupId && m.UserId.Equals(userId)).ToList();
                context.RMSecurityGroupMembership.RemoveRange(memberships);
                context.SaveChanges();
            }
        }

        public List<int> GetAllGroupIds(List<string> userAndGroupIds)
        {
            using (var context = GetNewContext())
            {
                var memberships = context.RMSecurityGroupMembership.Where(m => userAndGroupIds.Contains(m.UserId)).Select(a => a.GroupId).ToList();
                return memberships;
            }
        }

    }
}

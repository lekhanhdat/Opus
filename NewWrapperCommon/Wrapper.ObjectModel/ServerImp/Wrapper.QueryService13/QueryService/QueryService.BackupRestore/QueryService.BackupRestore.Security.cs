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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.QueryService
{
    internal partial class AveQueryService
    {
        /// <summary>
        /// Gets the collection of AveUserInfo objects that all the users are explicitly assigned permissions in siteCollection
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="site"></param>
        /// <param name="allAvailableUser"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public List<AveUserInfo> GetSiteUsers(Guid siteID, AveUserBackupOption option)
        {
            string cmdText = string.Empty;
            if (option.UserQueryOption != AveSiteUsersQueryOption.AllUsers)
            {
                cmdText = @" 
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
        Order by tp_ID";
            }
            else
            {
                cmdText = @"
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
        FROM UserInfo WITH(NOLOCK)       
        WHERE tp_SiteID=@SiteId
        Order by tp_ID";
            }
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteID);
            List<AveUserInfo> list = AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, cmdText, "tp_");
            if (list != null)
            {
                SetHasPermission(list, siteID, option.UserQueryOption == AveSiteUsersQueryOption.OnlyHaveSecurityUsers);// !allAvailableUser);
            }
            return list == null ? new List<AveUserInfo>() : list;
        }
        //修改site user 效率问题 分开查询
        /*public List<AveUserInfo> GetSiteUsers(IAveSite site, bool allAvailableUser)
        {
            string cmdText = string.Empty;
            if (!allAvailableUser)
            {
                cmdText = @" 
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                        AND ( tp_ID in (
                               SELECT DISTINCT(PrincipalId) FROM RoleAssignment WITH(NOLOCK)
                               WHERE SiteId=@SiteId
                        UNION
                        SELECT Distinct(MemberId) FROM GroupMembership WITH(NOLOCK)
                               WHERE SiteId=@SiteId
                                AND GroupId in
                                     (SELECT DISTINCT(PrincipalId) FROM Roleassignment WITH(NOLOCK) WHERE  SiteId=@SiteId))
                              OR tp_SiteAdmin = 1)
        Order by tp_ID";
            }
            else
            {
                cmdText = @"
        SELECT distinct(tp_ID),tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags,
               Convert(bit,isnull(Flag,0)) as tp_HasPermission
        FROM UserInfo WITH(NOLOCK)
        Left Join (SELECT SiteId,PrincipalId,1 as Flag FROM  RoleAssignment WHERE SiteId=@SiteId) as Tmp
                 on (PrincipalId=tp_ID or PrincipalId = (SELECT top 1(GroupId) FROM GroupMembership WHERE SiteId=tp_SiteID
                               and MemberId=tp_ID and GroupId in (SELECT PrincipalId FROM RoleAssignment WHERE SiteId=tp_SiteID )))
        WHERE tp_SiteID=@SiteId
        Order by tp_ID";
            }
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", site.ID);

            List<AveUserInfo> list = AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, cmdText, "tp_");

            return list;
        }*/
        #region SetUserPermission
        public void SetHasPermission(List<AveUserInfo> users, Guid siteId, bool deleteNoPermissionUser)
        {
            var principalIds = new List<int>();
            var hasPermissionUsersInGroup = new List<int>();
            principalIds = GetPrincipalIds(siteId);
            var groupMembership = GetGroupMembershipInfos(siteId);
            foreach (var groupId in groupMembership.Keys)
            {
                if (principalIds.Contains(groupId))
                {
                    foreach (var memberId in groupMembership[groupId])
                    {
                        if (!hasPermissionUsersInGroup.Contains(memberId))
                        {
                            hasPermissionUsersInGroup.Add(memberId);
                        }
                    }
                }
            }
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                user.HasPermission = users[i].SiteAdmin || principalIds.Contains(user.ID) || hasPermissionUsersInGroup.Contains(user.ID);
                //没有权限的不备份
                if (deleteNoPermissionUser && !user.HasPermission.Value)
                {
                    users.RemoveAt(i);
                    i--;
                }
            }
        }
        public List<int> GetPrincipalIds(Guid siteId)
        {
            var temp = new Dictionary<int, int>();
            var cmdText = @"SELECT PrincipalId FROM  RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId and RoleId != 1073741825";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    temp[dr.GetInt32(0)] = 0;
                }
            }
            return temp.Keys.ToList();
        }

        public Dictionary<int, List<int>> GetGroupMembershipInfos(Guid siteId)
        {
            var result = new Dictionary<int, List<int>>();
            var cmdText = @"SELECT GroupId,MemberId FROM GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    var groupId = dr.GetInt32(0);
                    if (!result.ContainsKey(groupId) || result[groupId] == null)
                    {
                        result[groupId] = new List<int>();
                    }
                    result[groupId].Add(dr.GetInt32(1));
                }
            }
            return result;
        }

        #endregion
        /// <summary>
        /// Gets the collection of AveUserInfo objects that all the users are explicitly assigned permissions in the Web site.
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="web"></param>
        /// <param name="allAvailableUser"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public List<AveUserInfo> GetWebUsers(IAveWeb web, bool allAvailableUser)
        {
            if (!web.HasUniqueRoleAssignments)
            {
                return null;
            }

            if (allAvailableUser)
            {
                return GetWebAllUsers(web);
            }
            else
            {
                return GetWebUsers(web);
            }
        }

        /// <summary>
        /// Get web all users.
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        private List<AveUserInfo> GetWebAllUsers(IAveWeb web)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", web.Site.ID);
            string cmdText = @"
        SELECT tp_ID
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId
        Order by tp_ID";
            return AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, cmdText, "tp_");
        }

        /// <summary>
        /// Get only have permission users.
        /// </summary>
        /// <param name="web"></param>
        /// <returns></returns>
        private List<AveUserInfo> GetWebUsers(IAveWeb web)
        {
            List<AveUserInfo> list = new List<AveUserInfo>();

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", web.Site.ID);
            mQueryWorker.AddParameter("@ScopeId", web.RoleAssignments.ID.ToString());
            string commandFirst = "SELECT distinct(principalId) as tp_ID FROM RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId And Scopeid=@ScopeId And RoleId != 1073741825";
            var tempUsers = AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, commandFirst, "tp_");
            if (tempUsers == null)
            {
                return null;
            }
            list.AddRange(tempUsers);

            string commandSecond = "SELECT Distinct(MemberId),GroupId FROM GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId";
            using (var reader = mQueryWorker.ExecuteReader(commandSecond))
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var groupId = reader.GetInt32(1);
                    //User所在SP Group必须在RoleAssignment表中存在。
                    if ((tempUsers.FindIndex(u => u.ID == groupId) != -1) && (list.FindIndex(u => u.ID == id) == -1))
                    {
                        list.Add(new AveUserInfo { ID = id });
                    }
                }
            }
            list.Sort((u1, u2) => u1.ID - u2.ID);
            return list;
        }

        /// <summary>
        /// allGroups:true-获取web下的所有Groups，false-获取Web下所有有权限的Groups
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="web"></param>
        /// <param name="allGroups"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han", true, "Use a simple query to improve the performance and be brief to read")]
        public List<AveGroupInfo> GetGroups(IAveWeb web, bool allGroups)
        {
            if (!web.HasUniqueRoleAssignments)
            {
                return null;
            }

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", web.Site.ID);
            mQueryWorker.AddParameter("@ScopeId", web.RoleAssignments.ID);
            string cmdText = string.Empty;
            if (allGroups)
            {
                //                cmdText = @"
                //        SELECT distinct(ID),Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags,Convert(bit,isnull(Flag,0)) as HasPermission
                //        From Groups WITH(NOLOCK)
                //        Left Join (select SiteId,PrincipalId,1 as Flag from  RoleAssignment WITH(NOLOCK) Where SiteId=@SiteId) as Tmp on ID=PrincipalId
                //        WHERE Groups.SiteId=@SiteId ORDER BY ID";
                cmdText = @"SELECT distinct(ID),Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags,Convert(bit,isnull(r.RoleId,0)) as HasPermission
        FROM Groups g WITH(NOLOCK)
        LEFT JOIN RoleAssignment r WITH(NOLOCK) ON g.SiteId = r.SiteId AND g.ID=r.PrincipalId AND r.ScopeId=@ScopeId
        WHERE g.SiteId=@SiteId";
            }
            else
            {
                //                cmdText = @"
                //        SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags
                //        From Groups WITH(NOLOCK) WHERE SiteId=@SiteId AND ID in(SELECT Id FROM Groups WITH(NOLOCK) WHERE Id in
                //        (SELECT PrincipalId FROM Roleassignment WITH(NOLOCK) WHERE  SiteId=@SiteId)) ORDER BY ID";
                cmdText = @"
        SELECT distinct(ID),Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags
        From Groups g WITH(NOLOCK) INNER JOIN RoleAssignment r WITH(NOLOCK) ON g.SiteId=r.SiteId AND g.ID=r.PrincipalId
        WHERE g.SiteId=@SiteId AND r.ScopeId=@ScopeId";
            }
            List<AveGroupInfo> groupInfos = AveQueryUtility.GetDBRows<AveGroupInfo>(mQueryWorker, cmdText);

            if (groupInfos == null || groupInfos.Count == 0)
            {
                return groupInfos;
            }
            groupInfos.Sort();
            cmdText = "SELECT GroupId,MemberId From GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId ORDER BY GroupId,MemberId";

            int groupIndex = 0;
            int badGroupId = -1;
            AveGroupInfo group = groupInfos[groupIndex];
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    int groupId;
                    int memberId;
                    while (dr.Read())
                    {
                        groupId = dr.GetInt32(0);
                        memberId = dr.GetInt32(1);
                        if (badGroupId == groupId)
                        {
                            continue;
                        }
                        if (groupId != group.ID)
                        {
                            int i = groupIndex + 1;
                            while (i < groupInfos.Count)
                            {
                                if (groupInfos[i].ID == groupId)
                                {
                                    groupIndex = i;
                                    break;
                                }
                                ++i;
                            }
                            if (i == groupInfos.Count)
                            {
                                badGroupId = groupId;
                                continue;
                            }
                            else
                            {
                                group = groupInfos[i];
                                groupIndex = i;
                                badGroupId = -1;
                            }
                        }
                        if (group.Memberships == null)
                        {
                            group.Memberships = new List<int>();
                        }
                        group.Memberships.Add(memberId);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return groupInfos;
        }

        /// <summary>
        /// 根据ScopeId获取List的权限分配
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="ScopeId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public List<AveRoleAssignmentInfo> GetListRoleAssignments(string SiteId, string ScopeId)
        {

            string cmdText = @"
                   SELECT RoleId,PrincipalId FROM RoleAssignment r WITH(NOLOCK) 
                   Inner Join UserInfo u WITH(NOLOCK)
                   on u.tp_SiteID=r.SiteId AND u.tp_ID = r.PrincipalId 
                   WHERE r.SiteId = @SiteId AND r.ScopeId= @ScopeId  AND u.tp_Deleted = 0
                   UNION ALL
                   SELECT RoleId, PrincipalId  from RoleAssignment r WITH(NOLOCK)
                   Inner Join Groups g WITH(NOLOCK) 
                   on g.SiteId = r.SiteId AND g.ID = r.PrincipalId
                   WHERE  r.SiteId = @SiteId AND r.ScopeId = @ScopeId 
                   order by PrincipalId ASC";


            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", SiteId);
            mQueryWorker.AddParameter("@ScopeId", ScopeId);
            return AveQueryUtility.GetDBRows<AveRoleAssignmentInfo>(mQueryWorker, cmdText);

        }

        /// <summary>
        /// 查询Web下的权限分配
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="ScopeId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/13", "Austin Han")]
        public List<AveRoleAssignmentInfo> GetObjectRoleAssignments(Guid SiteId, Guid ScopeId)
        {

            string cmdText = @"
                   SELECT RoleId,PrincipalId FROM RoleAssignment r WITH(NOLOCK) 
                   Inner Join UserInfo u WITH(NOLOCK)
                   on u.tp_SiteID=r.SiteId AND u.tp_ID = r.PrincipalId 
                   WHERE r.SiteId = @SiteId AND r.ScopeId= @ScopeId  AND u.tp_Deleted = 0
                   UNION ALL
                   SELECT RoleId, PrincipalId  from RoleAssignment r WITH(NOLOCK)
                   Inner Join Groups g WITH(NOLOCK) 
                   on g.SiteId = r.SiteId AND g.ID = r.PrincipalId
                   WHERE  r.SiteId = @SiteId AND r.ScopeId = @ScopeId 
                   order by PrincipalId ASC";


            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", SiteId);
            mQueryWorker.AddParameter("@ScopeId", ScopeId);
            return AveQueryUtility.GetDBRows<AveRoleAssignmentInfo>(mQueryWorker, cmdText);
        }

        /// <summary>
        /// 获取Web下的所有Roles
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="FirstUniqueRoleDefinitionWebId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public List<AveRoleInfo> GetWebRoles(Guid siteId, Guid FirstUniqueRoleDefinitionWebId)
        {
            string cmdText = @"
                            SELECT RoleId,Title,Description,PermMask,PermMaskDeny,Hidden,Type,WebGroupId,RoleOrder 
                            FROM Roles WITH(NOLOCK) WHERE SiteId=@SiteId and WebId=@WebId";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@WebId", FirstUniqueRoleDefinitionWebId);
            return AveQueryUtility.GetDBRows<AveRoleInfo>(mQueryWorker, cmdText);

        }


        /// <summary>
        /// 获取Site下第一个有独立权限的Web Id
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public Guid GetFirstUniqueRoleDefinitionWebGuid(Guid siteId, Guid scopeId)
        {
            string cmdTxt = @"select RoleDefWebId from Perms WITH(NOLOCK) where SiteId=@SiteId AND ScopeId=@ScopeId AND DelTransId=0x";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ScopeId", scopeId);
            return (Guid)mQueryWorker.ExecuteScalar(cmdTxt);
        }
        /// <summary>
        /// 查询某User在RoleId下的所具有的全部权限
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="scopeId"></param>
        /// <param name="roleId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public int GetRoleAssignmentCount(Guid siteId, Guid scopeId, int roleId, int principalId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@ScopeId", scopeId);
            mQueryWorker.AddParameter("@RoleId", roleId);
            mQueryWorker.AddParameter("@PrincipalId", principalId);
            return (int)mQueryWorker.ExecuteScalar("SELECT COUNT(*) from RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId and ScopeId=@ScopeId and RoleId=@RoleId and PrincipalId=@PrincipalId");
        }

        /// <summary>
        /// 查询Group及Group下的Members信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public AveGroupInfo GetGroupInfo(Guid siteId, int principalId)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", principalId);
            string cmdText = @"
        SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
        From Groups WITH(NOLOCK) WHERE SiteId=@SiteId AND ID=@Id";

            List<AveGroupInfo> groupList = AveQueryUtility.GetDBRows<AveGroupInfo>(mQueryWorker, cmdText);
            if (groupList == null || groupList.Count == 0)
            {
                return null;
            }
            AveGroupInfo groupInfo = groupList[0];

            cmdText = "SELECT MemberId From GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId AND GroupId=@Id";
            try
            {
                using (SqlDataReader dr = mQueryWorker.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        int memberId = dr.GetInt32(0);
                        if (groupInfo.Memberships == null)
                        {
                            groupInfo.Memberships = new List<int>();
                        }
                        groupInfo.Memberships.Add(memberId);
                    }
                }
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
            return groupInfo;

            #region client Method
            //IAveGroup group = aveSite.SPSite.RootWeb.Groups.GetByID(principalId);
            //AveGroupInfo groupInfo = new AveGroupInfo();
            //groupInfo.ID = group.ID;
            //groupInfo.Title = group.Name;
            //foreach (IAveUser user in group.Users)
            //{
            //    groupInfo.Memberships.Add(user.ID);
            //}
            //groupInfo.RequestEmail = group.RequestToJoinLeaveEmailSetting;
            ////do something
            //return groupInfo;
            #endregion
        }

        /// <summary>
        /// 查询某User的所有信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        public AveUserInfo GetUserInfo(Guid siteId, int principalId)
        {
            return GetUserInfo(siteId, principalId, true);
        }

        /// <summary>
        /// 将User Info更新到数据库(UserInfo，AllUserData
        /// API只能实现部分字段的更新(如Name)
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="userId"></param>
        /// <param name="old"></param>
        /// <param name="displayField"></param>
        /// <param name="nameField"></param>
        /// <param name="eMailField"></param>
        [QueryReview("2012/12/17", "Austin Han")]
        [QueryReview("Security-001")]
        public void UpdateUserInfo(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField, string eMailField)
        {
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UserListId", listId);
            mQueryWorker.AddParameter("@LoginName", old.Login);
            mQueryWorker.AddParameter("@Id", userId);
            mQueryWorker.AddParameter("@SystemId", old.SystemID);
            mQueryWorker.AddParameter("@Title", old.Title);
            mQueryWorker.AddParameter("@EMail", old.Email);
            mQueryWorker.ExecuteNonQuery("UPDATE UserInfo SET tp_SystemId=@SystemId,tp_Login=@LoginName,tp_Title=@Title,tp_Email=@EMail WHERE tp_SiteId=@SiteId AND tp_Id=@Id ", VersionOption.OneItemOrVersion, RowOrdinalOption.None);
            string cmdText = string.Format("UPDATE AllUserData SET {0}=@Title, {1}=@LoginName, {2}=@EMail WHERE tp_ListId=@UserListId AND tp_Id=@Id AND tp_RowOrdinal=0", displayField, nameField, eMailField);
            mQueryWorker.ExecuteNonQuery(cmdText, VersionOption.AllVersions, RowOrdinalOption.AllUserDataOneRow);
        }

        /// <summary>
        /// 查询某User的所有信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="principalId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public AveUserInfo GetUserInfo(Guid siteId, int principalId, bool checkDeleted)
        {
            AveUserInfo userInfo = null;
            string cmdText = @"
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_ID=@Id";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@Id", principalId);

            List<AveUserInfo> Users = AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, cmdText, "tp_");
            if (Users != null)
            {
                userInfo = GetActiveUser(siteId, Users[0]);
                userInfo.ID = Users[0].ID;
                if (checkDeleted && userInfo.Deleted != 0)//由于UserInfo表不可能存在ID相同的两条记录,所以直接判断Deleted属性即可,不需要调用CheckUserIfDeleted再做一遍查询。
                {
                    userInfo = null;
                }
            }
            return userInfo;

            #region client method
            //IAveUser user = aveSite.SPSite.RootWeb.AllUsers.GetByID(principalId);
            //AveUserInfo userInfo = new AveUserInfo();
            //userInfo.ID = user.ID;
            //userInfo.Login = user.LoginName;
            //userInfo.Title = user.Name;
            //userInfo.Email = user.Email;
            //userInfo.Notes = user.Notes;
            //if (user.RegionalSettings != null)
            //{
            //    userInfo.WorkDays = user.RegionalSettings.WorkDays;
            //    userInfo.WorkDayStartHour = user.RegionalSettings.WorkDayStartHour;
            //    userInfo.WorkDayEndHour = user.RegionalSettings.WorkDayEndHour;
            //    userInfo.CalendarType = user.RegionalSettings.CalendarType;
            //    userInfo.AdjustHijriDays = user.RegionalSettings.AdjustHijriDays;
            //    userInfo.AltCalendarType = (byte?)user.RegionalSettings.AlternateCalendarType;
            //    userInfo.Time24 = user.RegionalSettings.Time24;
            //}
            //return userInfo;
            #endregion
        }

        public AveUserInfo GetActiveUser(Guid siteId, AveUserInfo user)
        {
            if (user.Deleted != user.ID)
            {
                return user;
            }
            string cmdText = @"
        SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
               tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
               tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_Login=@LoginName order by tp_ID";

            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@LoginName", user.Login);

            List<AveUserInfo> Users = AveQueryUtility.GetDBRows<AveUserInfo>(mQueryWorker, cmdText, "tp_");
            return Users != null ? Users[Users.Count - 1] : user;
        }

        /// <summary>
        /// 查询给定的User是否是可用的用户(Active，未删除，有权限)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool CheckUserIfAvailable(Guid siteId, int userId)
        {
            string cmdText = @"
        SELECT COUNT(*)
        FROM UserInfo WITH(NOLOCK)
        WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                        AND tp_ID in (
                        SELECT DISTINCT(PrincipalId) FROM RoleAssignment WITH(NOLOCK) WHERE SiteId=@SiteId And PrincipalId=@UserId
                        UNION
                        SELECT DISTINCT(MemberId) FROM GroupMembership WITH(NOLOCK) WHERE SiteId=@SiteId AND MemberId=@UserId
        )";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UserId", userId);

            return (int)mQueryWorker.ExecuteScalar(cmdText) > 0;
        }

        [QueryReview("2012/12/17", "Austin Han")]
        public string GetScopeUrl(Guid siteId, Guid scopeId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("QueryService.CheckContentIfAveStub"))
            {
                string cmdText = @"SELECT ScopeUrl FROM Perms WITH(NOLOCK) WHERE SiteId =@SiteId AND ScopeId =@ScopeId";
                try
                {
                    mQueryWorker.ClearParameters();
                    mQueryWorker.AddParameter("@SiteId", siteId);
                    mQueryWorker.AddParameter("@ScopeId", scopeId);
                    return mQueryWorker.ExecuteScalar(cmdText).ToString();
                }
                catch (SqlException ex)
                {
                    throw new AveQueryException(ex);
                }
                catch (AveQueryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new AveQueryException(e.Message, e);
                }
            }
        }

        /// <summary>
        /// 查询给定的User是否被删除(只考虑是否存在在这个site上)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [QueryReview("2012/12/17", "Austin Han")]
        public bool CheckUserIfDeleted(Guid siteId, int userId)
        {
            string cmdText = @"SELECT COUNT(*) FROM UserInfo WITH(NOLOCK) WHERE tp_SiteID=@SiteId AND tp_ID=@UserId AND tp_Deleted<>0";
            mQueryWorker.ClearParameters();
            mQueryWorker.AddParameter("@SiteId", siteId);
            mQueryWorker.AddParameter("@UserId", userId);

            return (int)mQueryWorker.ExecuteScalar(cmdText) > 0;
        }

        [QueryReview("2012/12/18", "Austin Han")]
        public string GetUserLoginBySystemId(Guid siteId, byte[] systemId)
        {
            try
            {
                const string cmdText = "select tp_Login from UserInfo WITH(NOLOCK) where tp_SiteId=@SiteId and tp_SystemId=@SystemId";
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@SystemId", systemId);
                return mQueryWorker.ExecuteScalar(cmdText) as string;
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }

        [QueryReview("2013/05/29", "Sid You")]
        [QueryReview("Security-002")]
        public void ActiveDeletedUserBySystemId(Guid siteId, byte[] systemId)
        {
            try
            {
                const string cmdText = "update UserInfo set tp_Deleted=0 where tp_SiteId=@SiteId and tp_SystemId=@SystemId";
                mQueryWorker.AddParameter("@SiteId", siteId);
                mQueryWorker.AddParameter("@SystemId", systemId);
                mQueryWorker.ExecuteNonQuery(cmdText);
            }
            catch (SqlException queryException)
            {
                throw new AveQueryException(queryException);
            }
            catch (AveQueryException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AveQueryException(e.Message, e);
            }
        }
    }
}

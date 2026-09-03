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



namespace AvePoint.ObjectModel.Server13
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Common;
    using Microsoft.SharePoint;
    using AvePoint.Wrapper.Common;
    using System.Data.SqlClient;
    using System.Reflection;
    #endregion

    internal class AveDBServiceBase : IDisposable
    {
        protected AveSqlConnection SqlConn = new AveSqlConnection();
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> mFieldMaps = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        internal void Initialize(string connectionString)
        {
            if (SqlConn == null)
            {
                SqlConn = new AveSqlConnection();
            }
            SqlConn.Open(connectionString);
            if (WrapperConfiguration.IsMonitorEnable && SqlConn != null && SqlConn.Command != null)
            {
                AveQueryMonitor.RegisterConnection(SqlConn);
            }
        }

        internal AveSiteSettingInfo GetSiteSettingFromSites(SPSite site)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetSiteSettingFromSites"))
            {
#endif
                AveSiteSettingInfo info = new AveSiteSettingInfo();

                string cmdText = @"
SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,TimeCreated,UsersCount,
       BWUsed,DiskUsed,SecondStageDiskUsed,QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
       CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
       SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,LastContentChange,
       LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,UserIsActiveFieldRowOrdinal,
       UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,DomainGroupMapVersion,
       DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
FROM Sites WHERE Id=@SiteId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", site.ID);
                GetDBRow(info, SqlConn, cmdText, null, 0);
                return info;
#if PerformanceLog
            }
#endif
        }

        internal SqlDataReader GetALLWebTemplates(Guid siteId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetALLWebTemplates"))
            {
#endif
                string cmdText = @"SELECT Id,WebTemplate,ProvisionConfig FROM Webs WHERE SiteId=@SiteId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                return SqlConn.ExecuteReader(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal static Dictionary<string, FieldInfo> GetFieldMap(Type type, string prefix)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetFieldMap"))
            {
#endif
                if (!mFieldMaps.ContainsKey(type))
                {
                    Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();
                    foreach (FieldInfo fieldInfo in type.GetFields())
                    {
                        if (string.IsNullOrEmpty(prefix))
                        {
                            fieldMap[fieldInfo.Name] = fieldInfo;
                        }
                        else
                        {
                            fieldMap[prefix + fieldInfo.Name] = fieldInfo;
                        }
                    }
                    lock (mFieldMaps)
                    {
                        if (!mFieldMaps.ContainsKey(type))
                        {
                            mFieldMaps[type] = fieldMap;
                        }
                    }
                }
                return mFieldMaps[type];
#if PerformanceLog
            }
#endif
        }

        internal static void GetDBRow(object data, AveSqlConnection sqlConn, string cmdText, string prefix, int startIndex)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetDBRow"))
            {
#endif
                Dictionary<string, FieldInfo> fieldMap = GetFieldMap(data.GetType(), prefix);
                using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
                {
                    if (!dr.Read())
                    {
                        throw new Exception("Cannot find data.");
                    }
                    GetDBRow(data, dr, fieldMap, startIndex);
                }
#if PerformanceLog
            }
#endif
        }

        internal static void GetDBRow(object data, SqlDataReader sqlReader, Dictionary<string, FieldInfo> fieldMap, int startIndex)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetDBRow"))
            {
#endif
                if (data == null)
                {
                    return;
                }
                int fieldCount = sqlReader.FieldCount;

                for (int i = startIndex; i < fieldCount; i++)
                {
                    if (sqlReader.IsDBNull(i))
                    {
                        continue;
                    }
                    string name = sqlReader.GetName(i);
                    object value = sqlReader.GetValue(i);
                    if (fieldMap.ContainsKey(name))
                    {
                        Type fieldType = fieldMap[name].FieldType;
                        if (sqlReader.GetFieldType(i).IsAssignableFrom(fieldType))
                        {
                            fieldMap[name].SetValue(data, value);
                        }
                        else
                        {
                            fieldMap[name].SetValue(data, Activator.CreateInstance(fieldType, value));
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal AveGroupInfo GetGroupInfo(Guid siteId, int principalId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetGroupInfo"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", principalId);
                string cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId AND ID=@Id";

                List<AveGroupInfo> groupList = GetDBRows<AveGroupInfo>(SqlConn, cmdText);
                if (groupList == null || groupList.Count == 0)
                {
                    return null;
                }
                AveGroupInfo groupInfo = groupList[0];

                cmdText = "SELECT MemberId From GroupMembership WHERE SiteId=@SiteId AND GroupId=@Id";

                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
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
#if PerformanceLog
            }
#endif
        }

        internal AveUserInfo GetUserInfo(Guid siteId, int principalId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetUserInfo"))
            {
#endif
                AveUserInfo userInfo = null;
                string cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId AND tp_ID=@Id";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", principalId);

                List<AveUserInfo> Users = GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");
                if (Users != null)
                {
                    userInfo = Users[0];
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
#if PerformanceLog
            }
#endif
        }

        internal List<AveUserInfo> GetSiteUsers(SPSite site, bool allAvailableUser)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetSiteUsers"))
            {
#endif
                string cmdText = string.Empty;
                if (!allAvailableUser)
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                AND ( tp_ID in (SELECT DISTINCT(tp_ID) FROM UserInfo WHERE tp_ID in (
                       SELECT PrincipalId FROM RoleAssignment 
                       WHERE SiteId=@SiteId And PrincipalId not in (SELECT Id FROM Groups WHERE Siteid=@SiteId))
                UNION
                SELECT Distinct(MemberId) FROM GroupMembership 
                       WHERE SiteId=@SiteId
                        AND GroupId in(
                             SELECT Id FROM Groups WHERE Id in
                             (SELECT PrincipalId FROM Roleassignment WHERE  SiteId=@SiteId)))
                      OR tp_SiteAdmin = 1)
Order by tp_ID";
                }
                else
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId 
Order by tp_ID";
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", site.ID);

                List<AveUserInfo> list = GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");

                return list;
#if PerformanceLog
            }
#endif
        }

        internal static List<T> GetDBRows<T>(AveSqlConnection sqlConn, string cmdText)
        {
            return GetDBRows<T>(sqlConn, cmdText, null);
        }


        internal static List<T> GetDBRows<T>(AveSqlConnection sqlConn, string cmdText, string prefix)
        {
            List<T> values = null;
            GetDBRows<T>(ref values, sqlConn, cmdText, prefix);
            return values;
        }

        internal static void GetDBRows<T>(ref List<T> values, AveSqlConnection sqlConn, string cmdText, string prefix)
        {
            Type type = typeof(T);
            Dictionary<string, FieldInfo> fieldMap = GetFieldMap(type, prefix);
            using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    T value = (T)AveAssemblyUtility.CreateInstanceByType(type);
                    GetDBRow(value, dr, fieldMap, 0);
                    if (values == null)
                    {
                        values = new List<T>();
                    }
                    values.Add(value);
                }
            }
        }

        internal AveWebSettingInfo GetWebSettingFromWebs(SPWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetWebSettingFromWebs"))
            {
#endif
                AveWebSettingInfo info = new AveWebSettingInfo();

                string cmdText = @"
SELECT Author, Title, TimeCreated, Description, SecurityProvider, MetaInfo, MetaInfoVersion, LastMetadataChange, NavStructNextEid, 
       NextWebGroupId, DefTheme, AlternateCSSUrl, CustomizedCss, CustomJSUrl, AlternateHeaderUrl, DailyUsageData, DailyUsageDataVersion, 
       MonthlyUsageData, MonthlyUsageDataVersion, DayLastAccessed, Language, Locale, TimeZone, Time24, CalendarType, AdjustHijriDays, 
       ProvisionConfig, Flags,MasterUrl,CustomMasterUrl, Collation, RequestAccessEmail, SiteLogoUrl, SiteLogoDescription, AuditFlags, 
       InheritAuditFlags, Ancestry, AltCalendarType, CalendarViewOptions, WorkDayStartHour, WorkDayEndHour,WorkDays 
FROM Webs WHERE Id=@WebId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@WebId", web.ID);
                GetDBRow(info, SqlConn, cmdText, null, 0);
                return info;
#if PerformanceLog
            }
#endif
        }

        internal string GetLeafNameFromAllDocs(string cmdText, Dictionary<string, object> parameters)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetLeafNameFromAllDocs"))
            {
#endif
                string leafName = string.Empty;

                SqlConn.ClearParameters();

                foreach (string key in parameters.Keys)
                {
                    SqlConn.AddParameter(key, parameters[key]);
                }

                leafName = (string)SqlConn.ExecuteScalar(cmdText);

                return leafName;
#if PerformanceLog
            }
#endif
        }

        internal List<AveUserInfo> GetWebUsers(SPWeb web, bool allAvailableUser)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetWebUsers"))
            {
#endif
                if (!web.HasUniqueRoleAssignments)
                {
                    return null;
                }

                string cmdText = string.Empty;
                if (!allAvailableUser)
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
   tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
   tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
FROM UserInfo WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                AND ( tp_Id in(
                SELECT distinct(tp_id) FROM UserInfo WHERE tp_id in(
                       SELECT principalId FROM RoleAssignment 
                       WHERE Scopeid=@ScopeId And SiteId=@SiteId And PrincipalId not in(SELECT Id FROM Groups WHERE Siteid=@SiteId))
                UNION
                SELECT Distinct(MemberId) FROM GroupMembership 
                       WHERE SiteId=@SiteId AND GroupId in(
                             SELECT Id FROM Groups WHERE Id in(SELECT PrincipalId FROM Roleassignment WHERE Scopeid=@ScopeId and SiteId=@SiteId)))
                OR tp_SiteAdmin = 1)
ORDER BY tp_Id";
                }
                else
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId 
Order by tp_ID";
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", web.Site.ID);
                SqlConn.AddParameter("@ScopeId", web.RoleAssignments.Id.ToString());

                List<AveUserInfo> list = GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");

                return list;
#if PerformanceLog
            }
#endif
        }

        internal List<AveGroupInfo> GetGroups(SPWeb web, bool allGroups)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetGroups"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", web.Site.ID);
                string cmdText = string.Empty;
                if (allGroups)
                {
                    cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId ORDER BY ID";
                }
                else
                {
                    cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId AND ID in(SELECT Id FROM Groups WHERE Id in
(SELECT PrincipalId FROM Roleassignment WHERE  SiteId=@SiteId)) ORDER BY ID";
                }
                List<AveGroupInfo> groupInfos = GetDBRows<AveGroupInfo>(SqlConn, cmdText);

                if (groupInfos == null || groupInfos.Count == 0)
                {
                    return groupInfos;
                }
                groupInfos.Sort();
                cmdText = "SELECT GroupId,MemberId From GroupMembership WHERE SiteId=@SiteId ORDER BY GroupId,MemberId";

                int groupIndex = 0;
                int badGroupId = -1;
                AveGroupInfo group = groupInfos[groupIndex];
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
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
                return groupInfos;
#if PerformanceLog
            }
#endif
        }

        internal List<AveRoleAssignmentInfo> GetWebRoleAssignments(Guid SiteId, Guid ScopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetWebRoleAssignments"))
            {
#endif
                string cmdText = "SELECT RoleId,PrincipalId FROM RoleAssignment WHERE SiteId=@SiteId AND ScopeId=@ScopeId order by PrincipalId ASC";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", SiteId);
                SqlConn.AddParameter("@ScopeId", ScopeId);
                return GetDBRows<AveRoleAssignmentInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal List<AveRoleInfo> GetWebRoles(Guid siteId, Guid FirstUniqueRoleDefinitionWebId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetWebRoles"))
            {
#endif
                string cmdText = @"
                    SELECT RoleId,Title,Description,PermMask,PermMaskDeny,Hidden,Type,WebGroupId,RoleOrder 
                    FROM Roles WHERE SiteId=@SiteId and WebId=@WebId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@WebId", FirstUniqueRoleDefinitionWebId);
                return GetDBRows<AveRoleInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal Guid GetFirstUniqueRoleDefinitionWebGuid(Guid siteId, Guid scopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBService.GetFirstUniqueRoleDefinitionWebGuid"))
            {
#endif
                string cmdTxt = @"select RoleDefWebId from Perms where SiteId=@SiteId AND ScopeId=@ScopeId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ScopeId", scopeId);
                return (Guid)SqlConn.ExecuteScalar(cmdTxt);
#if PerformanceLog
            }
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (SqlConn != null)
            {
                SqlConn.Dispose();
                SqlConn = null;
            }
        }

        #endregion
    }
}

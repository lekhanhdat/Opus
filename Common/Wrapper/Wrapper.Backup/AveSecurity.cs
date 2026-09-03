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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace AvePoint.Wrapper.Backup
{
    public class AveRoles : IDisposable
    {
        private AveSPWeb mAveSPWeb = null;

        public AveRoles(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public static AveRoles CreateInstance(object obj)
        {
            return new AveRoles((AveSPWeb)obj);
        }

        public List<AveRoleInfo> GetRoles()
        {
            Guid gd = mAveSPWeb.SPWeb.FirstUniqueRoleDefinitionWeb.ID;
            if (!mAveSPWeb.ParentSite.ScopeIdsProcessed.Contains(gd))
            {
                mAveSPWeb.ParentSite.ScopeIdsProcessed.Add(gd);
            }
            return mAveSPWeb.SPWeb.RolesSerializer.GetObjectData();
        }

        public void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.Roles"))
            {
                output.WriteMetadata(AveMetadataType.Roles, GetRoles());
            }
        }

        public virtual void Dispose()
        {
        }
    }

    #region GROUP

    public abstract class AveGroup : IDisposable
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPSite mAveSPSite = null;
        protected AveSPWeb mAveWeb = null;

        public AveGroup(AveSPSite site)
        {
            this.mAveSPSite = site;
        }

        public virtual void Export(IAveBackupStream output)
        {
            List<AveGroupInfo> groups = GetGroupsWithAllMembers();
            if (groups != null && groups.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.Groups, groups);
                mLog.Info($"Groups count: {groups.Count}");
            }
        }

        public virtual void Export(IAveBackupStream output, bool allGroups)
        {
            List<AveGroupInfo> groups = GetGroupsWithAllMembers(allGroups);
            if (groups != null && groups.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.Groups, groups);
                mLog.Info($"Groups count: {groups.Count}");
            }
        }

        public virtual string ExportAsXml()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.Groups.ToString(), GetGroupsWithAllMembers());
        }

        public virtual List<AveGroupInfo> GetGroups()
        {
            return GetGroups(false);
        }

        public virtual List<AveGroupInfo> GetGroupsWithAllMembers()
        {
            return GetGroupsWithAllMembers(true);
        }

        public abstract List<AveGroupInfo> GetGroups(bool allGroups);

        public abstract List<AveGroupInfo> GetGroupsWithAllMembers(bool allGroups);

        public static AveGroup CreateInstatnce(object obj)
        {
            switch (obj.GetType().Name)
            {
                case "AveSPSite":
                    return new AveSiteGroup((AveSPSite)obj);
                case "AveSPWeb":
                    return new AveWebGroup(((AveSPWeb)obj));
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }
        }

        protected void AddToSiteDataCache(List<AveGroupInfo> list)
        {
            foreach (AveGroupInfo groupInfo in list)
            {
                int id = groupInfo.ID;
                mAveSPSite.DataCache.GroupCache.Add(id, groupInfo);
            }
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveSiteGroup : AveGroup
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSiteGroup(AveSPSite aveSite)
            : base(aveSite)
        { }

        public override List<AveGroupInfo> GetGroups(bool allGroups)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSiteGroup.GetGroups"))
            {
                List<AveGroupInfo> list = mAveSPSite.SPSite.RootWeb.GroupsSerializer.GetObjectData(allGroups) as List<AveGroupInfo>;
                if (list != null)
                {
                    AddToSiteDataCache(list);
                }
                return list;
            }
        }

        public override List<AveGroupInfo> GetGroupsWithAllMembers(bool allGroups)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPSite.Groups"))
            {
                List<AveGroupInfo> list = GetGroups(allGroups);
                if (list != null)
                {
                    foreach (AveGroupInfo info in list)
                    {
                        if (info.OwnerIsUser && info.Owner > 0)
                        {
                            info.OwnerInfo = (AveUserInfo)mAveSPSite.DataCache.GetPrincipalInfo(info.Owner);
                        }
                        if (info.Members.Count == 0)
                        {
                            foreach (int i in info.Memberships)
                            {
                                AveUserInfo userInfo = (AveUserInfo)mAveSPSite.DataCache.GetPrincipalInfo(i);
                                info.Members.Add(userInfo);
                            }
                        }
                    }
                }
                return list;
            }
        }
    }

    public class AveWebGroup : AveGroup
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPWeb mAveSPWeb = null;

        public AveWebGroup(AveSPWeb aveWeb)
            : base(aveWeb.ParentSite)
        {
            mAveSPWeb = aveWeb;
        }

        public override List<AveGroupInfo> GetGroups(bool allGroups)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveWebGroup.GetGroups"))
            {
                List<AveGroupInfo> list = mAveSPWeb.SPWeb.GroupsSerializer.GetObjectData(allGroups) as List<AveGroupInfo>;
                if (list != null)
                {
                    AddToSiteDataCache(list);
                }
                return list;
            }
        }

        public override List<AveGroupInfo> GetGroupsWithAllMembers(bool allGroups)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.Groups"))
            {
                List<AveGroupInfo> list = GetGroups(allGroups);

                if (list != null)
                {
                    foreach (AveGroupInfo info in list)
                    {
                        if (info.OwnerIsUser && info.Owner > 0)
                        {
                            info.OwnerInfo = (AveUserInfo)mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(info.Owner);
                        }
                        if (info.Members.Count == 0)
                        {
                            foreach (int i in info.Memberships)
                            {
                                AveUserInfo userInfo = (AveUserInfo)mAveSPWeb.ParentSite.DataCache.GetPrincipalInfo(i);
                                info.Members.Add(userInfo);
                            }
                        }
                    }
                }
                return list;
            }
        }
    }

    #endregion GROUP

    #region USER

    public abstract class AveUser : IDisposable
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveUser(AveSPSite site)
        {
            mAveParentSite = site;
        }

        public virtual string ExportAsXml()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.Users.ToString(), GetUsers());
        }

        public virtual void Export(IAveBackupStream output)
        {
            Export(output, false);
        }

        public virtual void Export(IAveBackupStream output, bool allAvailableUser)
        {
            List<AveUserInfo> users = GetUsers(allAvailableUser);
            if (users != null && users.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.Users, users);
                mLog.Info($"User count: {users.Count}");
            }
        }

        public abstract List<AveUserInfo> GetUsers();

        public abstract List<AveUserInfo> GetUsers(bool allAvailableUser);

        public static AveUser CreateInstance(object obj)
        {
            AveUser instance = null;

            string type = obj.GetType().Name;
            switch (type)
            {
                case "AveSPSite":
                    instance = new AveSiteUser((AveSPSite)obj);
                    break;
                case "AveSPWeb":
                    instance = new AveWebUser((AveSPWeb)obj);
                    break;
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }
            return instance;
        }

        //domain group在fba环境下存储的格式为c:0+.w|Sid,通过ConvertDomainGroupSidToAccount将Sid转化成account
        public List<AveUserInfo> ConvertDomainGroupSidToAccount(List<AveUserInfo> dataCache)
        {
            if (dataCache != null && dataCache.Count > 0)
            {
                for (int i = 0; i < dataCache.Count; i++)
                {
                    if (dataCache[i].DomainGroup)
                    {
                        if (dataCache[i].Login.IndexOf('|') > 0)
                        {
                            string temp = dataCache[i].Login.Substring(dataCache[i].Login.IndexOf('|') + 1);
                            if (AveDirectoryServiceUtility.IsStringSid(temp))
                            {
                                temp = AveDirectoryServiceUtility.GetAccountFromSid(temp, ParentSite.ObjectModelFactory);
                                if (!string.IsNullOrEmpty(temp))
                                {
                                    dataCache[i].Login = dataCache[i].Login.Substring(0, dataCache[i].Login.IndexOf('|') + 1) + temp;
                                }
                            }
                        }
                    }
                }
            }
            return dataCache;
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveSiteUser : AveUser
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSiteUser(AveSPSite aveSPSite)
            : base(aveSPSite)
        { }

        private void AddToSiteDataCache(List<AveUserInfo> list)
        {
            mLog.Info($"start Add to site data cache");
            foreach (AveUserInfo userInfo in list)
            {
                int id = userInfo.ID;
                mAveParentSite.DataCache.UserCache.Add(id, userInfo);
                mLog.Info($"Add to site data cache success,user id:{id}");
            }
        }

        public override List<AveUserInfo> GetUsers()
        {
            List<AveUserInfo> list = GetUsers(false);
            return list;
        }

        public override List<AveUserInfo> GetUsers(bool allAvailableUser)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPSite.Users"))
            {
                if (mAveParentSite.SiteUserInfoCache != null)
                {
                    return mAveParentSite.SiteUserInfoCache;
                }

                List<AveUserInfo> list = mAveParentSite.SPSite.SiteUsersSerializer.GetObjectData(allAvailableUser) as List<AveUserInfo>;

                mAveParentSite.SiteUserInfoCache = list;
                //处理FBA环境下的domain group
                ConvertDomainGroupSidToAccount(list);
                if (list != null)
                {
                    AddToSiteDataCache(list);
                }

                return list;
            }
        }

        //public override string ExportAsXml()
        //{
        //    return base.ExportAsXml();
        //}

        //public override void Export(IAveBackupStream output)
        //{
        //    base.Export(output);
        //}
    }

    public class AveWebUser : AveUser
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPWeb mAveSPWeb = null;

        public AveWebUser(AveSPWeb aveSPWeb)
            : base(aveSPWeb.ParentSite)
        {
            mAveSPWeb = aveSPWeb;
        }

        private void AddToSiteDataCache(List<AveUserInfo> list)
        {
            foreach (AveUserInfo userInfo in list)
            {
                int id = userInfo.ID;
                mAveSPWeb.ParentSite.DataCache.UserCache.Add(id, userInfo);
            }
        }

        public override List<AveUserInfo> GetUsers()
        {
            List<AveUserInfo> list = GetUsers(false);
            return list;
        }

        public override List<AveUserInfo> GetUsers(bool allAvailableUser)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPWeb.Users"))
            {
                if (mAveParentSite != null && mAveParentSite.SiteUserInfoCache == null)
                {
                    mAveParentSite.SiteUserInfoCache = mAveParentSite.SPSite.SiteUsersSerializer.GetObjectData(allAvailableUser) as List<AveUserInfo>;
                }

                List<AveUserInfo> tmpList = mAveSPWeb.SPWeb.WebUsersSerializer.GetObjectData(allAvailableUser) as List<AveUserInfo>;
                List<AveUserInfo> list = new List<AveUserInfo>();

                //处理FBA环境下的domain group

                #region For performance

                if (mAveParentSite != null && tmpList != null)
                {
                    foreach (var user in tmpList)
                    {
                        bool foundInCache = false;
                        foreach (var cache in mAveParentSite.SiteUserInfoCache)
                        {
                            if (user.ID == cache.ID)
                            {
                                list.Add(cache);
                                foundInCache = true;
                                break;
                            }
                        }
                        if (!foundInCache)
                        {
                            mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBSomeUserNotFind, user.ID);
                        }
                    }
                }

                #endregion For performance

                if (list.Count > 0)
                {
                    ConvertDomainGroupSidToAccount(list);
                    AddToSiteDataCache(list);
                }
                mLog.Info($"after add user to list ,list count is {list.Count}");
                return list;
            }
        }

        //public override string ExportAsXml()
        //{
        //    return AveConvert.ConvertAveObjToAveXml(AveMetadataType.Users.ToString(), GetUsers());
        //}

        //public override void Export(IAveBackupStream output)
        //{
        //    base.Export(output);
        //}
    }

    #endregion USER

    #region RoleAssignments

    public abstract class AveRoleAssignments : IDisposable
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract List<AveRoleAssignmentInfo> GetRoleAssignments();

        public abstract void Export(IAveBackupStream output);

        public static AveRoleAssignments CreateInstance(object obj)
        {
            switch (obj.GetType().Name)
            {
                case "AveSPWeb":
                    return new AveWebRoleAssignments((AveSPWeb)obj);
                case "AveSPList":
                    return new AveListRoleAssignments((AveSPList)obj);
                case "AveSPItem":
                    return new AveItemRoleAssignments((AveSPItem)obj);
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
            }
        }

        protected List<AveRoleAssignmentInfo> GetRoleAssignments(IAveRoleAssignmentCollection roleAssignments)
        {
            List<AveRoleAssignmentInfo> roleAssignmentsInfo = null;

            if (roleAssignments.Count > 0)
            {
                roleAssignmentsInfo = new List<AveRoleAssignmentInfo>(roleAssignments.Count);
            }

            foreach (IAveRoleAssignment roleAssignment in roleAssignments)
            {
                IAveRoleDefinitionBindingCollection roleDefinitonBindingCol = roleAssignment.RoleDefinitionBindings;
                foreach (IAveRoleDefinition roleDef in roleDefinitonBindingCol)
                {
                    AveRoleAssignmentInfo roleAssignmentInfo = new AveRoleAssignmentInfo();
                    roleAssignmentInfo.PrincipalId = roleAssignment.Member.ID;
                    roleAssignmentInfo.RoleId = roleDef.ID;
                    roleAssignmentsInfo?.Add(roleAssignmentInfo);
                }
            }

            return roleAssignmentsInfo;
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveItemRoleAssignments : AveRoleAssignments
    {
        private AveSPItem mAveSPItem = null;

        public AveItemRoleAssignments(AveSPItem aveItem)
        {
            mAveSPItem = aveItem;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments()
        {
            if (!this.mAveSPItem.HasUniqueRoleAssignments)
            {
                return new List<AveRoleAssignmentInfo>();
            }
            List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPItem.Item.GetItemRoleAssignments(mAveSPItem.SiteId, mAveSPItem.ScopeId) ?? new List<AveRoleAssignmentInfo>();
            if (roleAssignmentInfos != null)
            {
                foreach (AveRoleAssignmentInfo roleAssignmentInfo in roleAssignmentInfos)
                {
                    try
                    {                        
                        IAveRoleDefinition roleDefinition = mAveSPItem.AveSPList.ParentWeb.SPWeb.RoleDefinitions.GetById(roleAssignmentInfo.RoleId);                        
                        roleAssignmentInfo.RoleName = roleDefinition.Name;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperBackupResource.AWBRoleDefinitionNotFind, roleAssignmentInfo.RoleId, ex);
                        continue;
                    }
                }
            }
            return roleAssignmentInfos;
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.RoleAssignments"))
            {
                var dataCache = mAveSPItem.RoleAssignmentCache ?? GetRoleAssignments();
                if (dataCache != null)
                {
                    logger.Info($"AveItemRoleAssignments.Export RoleAssignment Count:{dataCache.Count}.");
                    output.WriteMetadata(AveMetadataType.RoleAssignment, dataCache);
                }
            }
        }
    }

    public class AveListRoleAssignments : AveRoleAssignments
    {
        private AveSPList mAveSPList = null;

        public AveListRoleAssignments(AveSPList aveSPList)
        {
            mAveSPList = aveSPList;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments()
        {
            if (!mAveSPList.SPList.HasUniqueRoleAssignments)
            {
                return new List<AveRoleAssignmentInfo>();
            }
            List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPList.SPList.RoleAssignments.GetRoleAssignments(mAveSPList.ParentWeb.ParentSite.SPSite.ID) ?? new List<AveRoleAssignmentInfo>();
            if (roleAssignmentInfos != null)
            {
                foreach (AveRoleAssignmentInfo roleAssignmentInfo in roleAssignmentInfos)
                {
                    try
                    {
                        IAveRoleDefinition roleDefinition = mAveSPList.ParentWeb.SPWeb.RoleDefinitions.GetById(roleAssignmentInfo.RoleId);
                        roleAssignmentInfo.RoleName = roleDefinition.Name;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperBackupResource.AWBRoleDefinitionNotFind, roleAssignmentInfo.RoleId, ex);
                        continue;
                    }
                }
            }
            return roleAssignmentInfos;
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.RoleAssignments"))
            {
                var dataCache = mAveSPList.RoleAssignmentCache ?? GetRoleAssignments();
                if (dataCache != null)
                {
                    logger.Info($"AveListRoleAssignments.Export RoleAssignment Count:{dataCache.Count}.");
                    output.WriteMetadata(AveMetadataType.RoleAssignment, dataCache);
                }
            }
        }
    }

    public class AveWebRoleAssignments : AveRoleAssignments
    {
        private AveSPWeb mAveSPWeb = null;

        public AveWebRoleAssignments(AveSPWeb aveSPWeb)
        {
            mAveSPWeb = aveSPWeb;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments()
        {
            List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPWeb.SPWeb.RoleAssignments.GetRoleAssignments(mAveSPWeb.ParentSite.SPSite.ID) ?? new List<AveRoleAssignmentInfo>();
            if (roleAssignmentInfos != null)
            {
                foreach (AveRoleAssignmentInfo roleAssignmentInfo in roleAssignmentInfos)
                {
                    try
                    {
                        IAveRoleDefinition roleDefinition = mAveSPWeb.SPWeb.RoleDefinitions.GetById(roleAssignmentInfo.RoleId);
                        roleAssignmentInfo.RoleName = roleDefinition.Name;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, WrapperBackupResource.AWBRoleDefinitionNotFind, roleAssignmentInfo.RoleId, ex);
                        continue;
                    }
                }
            }
            Guid roleAssignmentsID = mAveSPWeb.SPWeb.RoleAssignments.ID;
            if (!mAveSPWeb.ParentSite.ScopeIdsProcessed.Contains(roleAssignmentsID))
            {
                mAveSPWeb.ParentSite.ScopeIdsProcessed.Add(roleAssignmentsID);
            }
            return roleAssignmentInfos;
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.RoleAssignments"))
            {
                List<AveRoleAssignmentInfo> roleAssignments = GetRoleAssignments();
                if (roleAssignments != null && roleAssignments.Count > 0)
                {
                    logger.Info($"AveWebRoleAssignments.Export RoleAssignment Count:{roleAssignments.Count}.");
                    output.WriteMetadata(AveMetadataType.RoleAssignment, roleAssignments);
                }
            }
        }
    }

    #endregion RoleAssignments
}
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
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Threading;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    #region ROLE

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

    #endregion

    #region GROUP

    public abstract class AveGroup : IDisposable
    {
        public static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        protected AveSPSite mAveSPSite = null;

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
            }
        }

        public virtual void Export(IAveBackupStream output, bool allGroups)
        {
            List<AveGroupInfo> groups = GetGroupsWithAllMembers(allGroups);
            if (groups != null && groups.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.Groups, groups);
            }
        }

        public virtual string ExportAsXml()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.Groups.ToString(), GetGroupsWithAllMembers());
        }

        public virtual List<AveGroupInfo> GetGroups()
        {
            List<AveGroupInfo> list = GetGroups(false);

            return list;
        }

        public abstract List<AveGroupInfo> GetGroups(bool allGroups);

        public virtual List<AveGroupInfo> GetGroupsWithAllMembers()
        {
            List<AveGroupInfo> list = GetGroupsWithAllMembers(true);

            return list;
        }

        public abstract List<AveGroupInfo> GetGroupsWithAllMembers(bool allGroups);

        public static AveGroup CreateInstatnce(object obj)
        {
            if (obj is AveSPSite)
            {
                return new AveSiteGroup((AveSPSite)obj);
            }
            else if (obj is AveSPWeb)
            {
                return new AveWebGroup((AveSPWeb)obj);
            }
            else
            {
                throw new Exception(string.Format("The object type:{0} is undefined.", obj.GetType().ToString()));
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

        public static void SetAboutMeToGroupInfo(AveGroupInfo groupInfo, IAveWeb web)
        {
            if (groupInfo != null && web != null)
            {
                try
                {
                    var groupInfoItem = web.SiteUserInfoList.GetItemById(groupInfo.ID);
                    if (groupInfoItem.Fields.ContainsField("Notes"))
                    {
                        groupInfo.AboutMe = (string)groupInfoItem["Notes"];
                    }
                }
                catch(Exception ex)
                {
                    mLog.Warn("Failed to set about me value to groupInfo, message: {0}", ex.ToString());
                }
            }
        }

        public static void SetAboutMeToGroupInfos(List<AveGroupInfo> groupInfos, IAveWeb web)
        {
            if (groupInfos != null)
            {
                foreach (var groupInfo in groupInfos)
                {
                    SetAboutMeToGroupInfo(groupInfo, web);
                }
            }
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
                SetAboutMeToGroupInfos(list, mAveSPSite.SPSite.RootWeb);
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
                            info.OwnerInfo = mAveSPSite.DataCache.GetUserInfo(info.Owner);
                            if (info.OwnerInfo == null)
                            {
                                mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.GroupOwnerTypeInvalidate, info.Owner);
                            }
                        }
                        if (info.Members.Count == 0)
                        {
                            foreach (int i in info.Memberships)
                            {
                                AveUserInfo userInfo = mAveSPSite.DataCache.GetUserInfo(i);
                                if (userInfo == null)
                                {
                                    mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.GroupMemberTypeInvalidate, i);
                                    continue;
                                }
                                info.Members.Add(userInfo);
                            }
                        }
                        AveUserUtility.ConvertDomainGroupSidToAccount(info.Members, this.mAveSPSite.ObjectModelFactory);
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
                SetAboutMeToGroupInfos(list, mAveSPWeb.SPWeb);
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
                            info.OwnerInfo = mAveSPWeb.ParentSite.DataCache.GetUserInfo(info.Owner);
                            if (info.OwnerInfo == null)
                            {
                                mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.GroupOwnerTypeInvalidate, info.Owner);
                            }
                        }
                        if (info.Members.Count == 0)
                        {
                            foreach (int i in info.Memberships)
                            {
                                AveUserInfo userInfo = mAveSPWeb.ParentSite.DataCache.GetUserInfo(i);
                                if (userInfo == null)
                                {
                                    mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.GroupOwnerTypeInvalidate, i);
                                    continue;
                                }
                                info.Members.Add(userInfo);
                            }
                        }
                        AveUserUtility.ConvertDomainGroupSidToAccount(info.Members, this.mAveSPSite.ObjectModelFactory);
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
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.Users.ToString(), GetUsers(new AveUserBackupOption(){ UserQueryOption=AveSiteUsersQueryOption.OnlyHaveSecurityUsers}));
        }


        public virtual void Export(IAveBackupStream output, AveUserBackupOption option)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveUser.Export"))
            {
                List<AveUserInfo> users = GetUsers(option);
                if (users != null && users.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.Users, users);
                }
            }
        }


        public abstract List<AveUserInfo> GetUsers(AveUserBackupOption option);

        public static AveUser CreateInstance(object obj)
        {
            AveUser instance = null;

            if (obj is AveSPSite)
            {
                instance = new AveSiteUser((AveSPSite) obj);
            }
            else if (obj is AveSPWeb)
            {
                instance = new AveWebUser((AveSPWeb) obj);
            }
            else
            {
                throw new Exception(string.Format("The object type:{0} is undefined.", obj.GetType().ToString()));
            }
            return instance;
        }
        
        protected virtual void AddToSiteDataCache(List<AveUserInfo> list)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveUser.AddToSiteDataCache"))
            {
                foreach (AveUserInfo userInfo in list)
                {
                    int id = userInfo.ID;
                    mAveParentSite.DataCache.UserCache.Add(id, userInfo);
                }
            }
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

        public override List<AveUserInfo> GetUsers(AveUserBackupOption option)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPSite.Users"))
            {
                if (mAveParentSite.SiteUserInfoCache != null)
                {
                    return mAveParentSite.SiteUserInfoCache;
                }

                List<AveUserInfo> list = mAveParentSite.SPSite.SiteUsersSerializer.GetObjectData(option) as List<AveUserInfo>;

                mAveParentSite.SiteUserInfoCache = list;
                //处理FBA环境下的domain group
                if (list != null)
                {
                    list = AveUserUtility.ConvertDomainGroupSidToAccount(list, this.ParentSite.ObjectModelFactory);
                    AddToSiteDataCache(list);
                }

                return list;
            }
        }
        
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

        public override List<AveUserInfo> GetUsers(AveUserBackupOption option)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPWeb.Users"))
            {
                if (mAveParentSite != null && mAveParentSite.SiteUserInfoCache == null)
                {
                    mAveParentSite.SiteUserInfoCache = mAveParentSite.SPSite.SiteUsersSerializer.GetObjectData(option) as List<AveUserInfo>;
                }

                List<AveUserInfo> tmpList = mAveSPWeb.SPWeb.WebUsersSerializer.GetObjectData(option) as List<AveUserInfo>;
                List<AveUserInfo> list = new List<AveUserInfo>();             

                #region For performance

                if (mAveParentSite != null && tmpList != null)
                {
                    mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBGetWebUserCount, tmpList.Count);
                    foreach (var user in tmpList)
                    {
                        var addReport = true;
                        if(mAveParentSite.SiteUserCache.ContainsKey(user.ID))
                        {
                            list.Add(mAveParentSite.SiteUserCache[user.ID]);
                            addReport = false;
                        }
                        if (addReport)
                        {
                            mLog.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBSomeUserNotFind, user.ID);
                        }
                    }
                }

                #endregion For performance

                //处理FBA环境下的domain group
                if (list.Count > 0)
                {
                    list = AveUserUtility.ConvertDomainGroupSidToAccount(list, this.ParentSite.ObjectModelFactory);
                    AddToSiteDataCache(list);
                }

                return list;
            }
        }


    }

    #endregion USER

    #region RoleAssignments

    public abstract class AveRoleAssignments : IDisposable
    {
        protected readonly AveSPSite spSite;

        protected AveRoleAssignments(AveSPSite spSite)
        {
            this.spSite = spSite;
        }

        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected abstract bool HasUniqueRoleAssignments { get; }

        public virtual List<AveRoleAssignmentInfo> GetRoleAssignments()
        {
            return GetRoleAssignments(false);
        }

        public abstract List<AveRoleAssignmentInfo> GetRoleAssignments(bool includeInheritanceObj);

        public abstract void Export(IAveBackupStream output);

        public void ExportInheritStatus(IAveBackupStream output, bool isInherit)
        {
            output.WriteMetadata(AveMetadataType.RoleAssignmentInheritStatus, isInherit);
        }

        /// <summary>
        /// Get Role Assignments Dto
        /// 无论是否有独立权限，都会备份RoleAssignments，如果需要过滤，在外层判断
        /// </summary>
        /// <param name="includeUsers"></param>
        /// <param name="includeGroups"></param>
        /// <returns></returns>
        internal SPRoleAssignmentsDto GetRoleAssignmentsDto(bool includeUsers, bool includeGroups)
        {
            var dto = new SPRoleAssignmentsDto();

            dto.RoleAssignmentInfos = GetRoleAssignments(true);

            if (includeGroups || includeUsers)
            {
                if (dto.RoleAssignmentInfos != null && dto.RoleAssignmentInfos.Count > 0)
                {
                    var cache = GetPrincipalInfo(dto.RoleAssignmentInfos);
                    if (cache != null)
                    {
                        if (includeUsers)
                        {
                            dto.UserCache = cache.GetUsersForExport();
                        }
                        if (includeGroups)
                        {
                            dto.GroupCache = cache.GetGroupsForExport();
                        }
                    }
                }
            }

            return dto;
        }

        private AveDataCache GetPrincipalInfo(List<AveRoleAssignmentInfo> roleAssignments)
        {
            AveDataCache dataCache = null;

            if (roleAssignments != null)
            {
                dataCache = new AveDataCache();
                for (int i = 0; i < roleAssignments.Count; ++i)
                {
                    try
                    {
                        int principalId = roleAssignments[i].PrincipalId;
                        if (!dataCache.PrincipalIdAlreadyExists(principalId))
                        {
                            var user = spSite.DataCache.GetUserInfo(principalId);
                            if (user !=null)
                            {
                                dataCache.AddToCache(principalId, user);
                                continue;
                            }
                            var group = spSite.DataCache.GetGroupInfo(principalId);
                            if (group != null)
                            {
                                dataCache.AddToCache(principalId, group);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN,
                                string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
                    }
                }
            }

            return dataCache;
        }
        
        protected bool IncludeRoleAssignments(bool includeInheritanceObj)
        {
            return HasUniqueRoleAssignments || includeInheritanceObj;
        }

        public static AveRoleAssignments CreateInstance(object obj)
        {
            if (obj is AveSPWeb)
            {
                return new AveWebRoleAssignments((AveSPWeb) obj);
            }
            else if (obj is AveSPList)
            {
                return new AveListRoleAssignments((AveSPList) obj);
            }
            else if (obj is AveSPItem)
            {
                return new AveItemRoleAssignments((AveSPItem) obj);
            }
            else
            {
                throw new Exception(string.Format("The object type:{0} is undefined.", obj.GetType().ToString()));
            }
        }

        protected List<AveRoleAssignmentInfo> GetRoleAssignments(IAveRoleAssignmentCollection roleAssignments)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveRoleAssignments.GetRoleAssignments"))
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
                        roleAssignmentsInfo.Add(roleAssignmentInfo);
                    }
                }

                return roleAssignmentsInfo;
            }
        }

        public virtual void Dispose()
        {
        }
    }

    public class AveItemRoleAssignments : AveRoleAssignments
    {
        private AveSPItem mAveSPItem = null;

        public AveItemRoleAssignments(AveSPItem aveItem)
            : base(aveItem.ParentSite)
        {
            mAveSPItem = aveItem;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments(bool includeInheritanceObj)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ItemRoleAssignments"))
            {
                if (!IncludeRoleAssignments(includeInheritanceObj))
                {
                    return new List<AveRoleAssignmentInfo>();
                }
                List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPItem.Item.GetItemRoleAssignments(mAveSPItem.SiteId, mAveSPItem.ScopeId) ?? new List<AveRoleAssignmentInfo>();
                if (roleAssignmentInfos != null)
                {
                    CultureInfo currentCultureInfo = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = mAveSPItem.AveSPList.ParentWeb.SPWeb.UICulture;
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
                    Thread.CurrentThread.CurrentUICulture = currentCultureInfo;
                }
                return roleAssignmentInfos;
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.RoleAssignments"))
            {
                var dataCache = mAveSPItem.RoleAssignmentCache ?? GetRoleAssignments();
                if (dataCache != null)
                {
                    output.WriteMetadata(AveMetadataType.RoleAssignment, dataCache);
                }
            }
        }

        protected override bool HasUniqueRoleAssignments
        {
            get { return mAveSPItem.HasUniqueRoleAssignments; }
        }
    }

    public class AveListRoleAssignments : AveRoleAssignments
    {
        private AveSPList mAveSPList = null;

        public AveListRoleAssignments(AveSPList aveSPList)
            : base(aveSPList.ParentSite)
        {
            mAveSPList = aveSPList;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments(bool includeInheritanceObj)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.ListRoleAssignments"))
            {
                if (!IncludeRoleAssignments(includeInheritanceObj))
                {
                    return new List<AveRoleAssignmentInfo>();
                }
                List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPList.SPList.RoleAssignments.GetRoleAssignments(mAveSPList.ParentWeb.ParentSite.SPSite.ID) ?? new List<AveRoleAssignmentInfo>();
                if (roleAssignmentInfos != null)
                {
                    CultureInfo currentCultureInfo = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = mAveSPList.ParentWeb.SPWeb.UICulture;     
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
                    Thread.CurrentThread.CurrentUICulture = currentCultureInfo;
                }
                return roleAssignmentInfos;
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPList.RoleAssignments"))
            {
                var dataCache = mAveSPList.RoleAssignmentCache ?? GetRoleAssignments();
                if (dataCache != null)
                {
                    output.WriteMetadata(AveMetadataType.RoleAssignment, dataCache);
                }
            }
        }

        protected override bool HasUniqueRoleAssignments
        {
            get { return mAveSPList.HasUniqueRoleAssignments; }
        }
    }

    public class AveWebRoleAssignments : AveRoleAssignments
    {
        private AveSPWeb mAveSPWeb = null;

        public AveWebRoleAssignments(AveSPWeb aveSPWeb) : base(aveSPWeb.ParentSite)
        {
            mAveSPWeb = aveSPWeb;
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments()
        {
            return GetRoleAssignments(true);
        }

        public override List<AveRoleAssignmentInfo> GetRoleAssignments(bool includeInheritanceObj)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.WebRoleAssignments"))
            {
                if (!IncludeRoleAssignments(includeInheritanceObj))
                {
                    return new List<AveRoleAssignmentInfo>();
                }
                List<AveRoleAssignmentInfo> roleAssignmentInfos = mAveSPWeb.SPWeb.RoleAssignments.GetRoleAssignments(mAveSPWeb.ParentSite.SPSite.ID) ?? new List<AveRoleAssignmentInfo>();
                if (roleAssignmentInfos != null)
                {
                    CultureInfo currentCultureInfo = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = mAveSPWeb.SPWeb.UICulture; 
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
                    Thread.CurrentThread.CurrentUICulture = currentCultureInfo;
                }
                return roleAssignmentInfos;
            }
        }

        public override void Export(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPWeb.RoleAssignments"))
            {
                List<AveRoleAssignmentInfo> roleAssignments = GetRoleAssignments();
                if (roleAssignments != null)
                {
                    output.WriteMetadata(AveMetadataType.RoleAssignment, roleAssignments);
                }
            }
        }

        protected override bool HasUniqueRoleAssignments
        {
            get { return mAveSPWeb.HasUniqueRoleAssignments; }
        }
    }

    #endregion RoleAssignments
}
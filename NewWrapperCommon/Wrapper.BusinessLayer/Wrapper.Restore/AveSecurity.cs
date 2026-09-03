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
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using System.Threading;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveSecurity
    {
        AveObjectSecurity mObj;
        public AveSecurity(object obj)
        {
            mObj = AveObjectSecurity.CreateInstance(obj);
        }

        public void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
            mObj.Restore(securityInfo, restoreOption);
        }
    }

    public abstract class AveObjectSecurity : AvePoint.Wrapper.Restore.IAveObjectSecurity, IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSite ParentSite { get; protected set; }

        private IAveWeb mWeb;

        public bool SourceHasUniqueRoleAssignment { set; get; }

        protected IAveSecurableObject securableObject { set; get; }

        private static List<int> builtinRole = new List<int>() {
            1073741825,//Limited Access
            1073741826,//Read
            1073741827,//Contribute
            1073741828,//Design
            1073741829,//Full Control
            1073741830,//Edit
            1073741924 //View Only
        };

        protected bool IsNewCreatedObject { set; get; }

        protected IReport report = new AveWrapperReport();

        public AveObjectSecurity()
        {
            SourceHasUniqueRoleAssignment = true;
        }

        public AveObjectSecurity(IAveSecurableObject securableObject, AveSPSite aveSite, IAveWeb web)
            : this()
        {
            this.securableObject = securableObject;
            mWeb = web;
            ParentSite = aveSite;
        }

        public IReport GetReport()
        {
            return report;
        }

        public static AveObjectSecurity CreateInstance(object obj)
        {
            AveObjectSecurity instance = null;

            if (obj is AveSPSite)
            {
                instance = new AveSiteSecurity((AveSPSite)obj);
            }
            else if (obj is AveSPWeb)
            {
                instance = new AveWebSecurity((AveSPWeb)obj);
            }
            else if (obj is AveSPList)
            {
                instance = new AveListSecurity((AveSPList)obj);
            }
            else if (obj is AveSPItem)
            {
                instance = new AveItemSecurity((AveSPItem)obj);
            }
            else
            {
                throw new Exception("Cannot construct an instance for this object type: " + obj.GetType().ToString());
            }

            return instance;
        }

        public IAveUser GetSPUser(int principalId, AveSPWeb aveSPWeb)
        {
            IAveUser user = null;
            try
            {
                int newPrincipalId = aveSPWeb.ParentSite.SPMembers.FindMemberId(principalId);
                user = aveSPWeb.ParentSite.SPSite.RootWeb.AllUsers.GetByID(newPrincipalId);
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetSPUserByIdError, e.ToString());
                //Do Nothing
            }

            return user;
        }

        protected void RestoreInheritanceState(SecurityRestoreOption restoreOption, bool sourceIsUniqueRoleAssignments, out bool restorePermissonComplete)
        {
            if (sourceIsUniqueRoleAssignments)
            {//源端为独立权限
                if (!securableObject.HasUniqueRoleAssignments)
                {
                    //ADO-61698 
                    //当打破继承时，如果不为新创建的并且冲突处理为merge，则保留parent 权限设置
                    if (!this.IsNewCreatedObject && restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.Merge)
                    {
                        securableObject.BreakRoleInheritance(true);
                    }
                    else
                    {
                        securableObject.BreakRoleInheritance(false, false);
                    }
                    //bool copyRoleAssignments = restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.Merge && !IsNewCreatedObject;
                    //if (WrapperConfiguration.RemoveParentLimitedAccess)
                    //{
                    //    //it will assign current user permissions to securableobject and its parent object when set copyroleassignments as false, so always set copyroleassignments as false,
                    //    securableObject.BreakRoleInheritance(true);
                    //    if (!copyRoleAssignments)
                    //    {
                    //        ClearRoleAssignments();
                    //    }
                    //}
                    //else
                    //{
                    //    securableObject.BreakRoleInheritance(copyRoleAssignments);
                    //}
                    RestoreAnonymousPermSetting();
                }
                restorePermissonComplete = false;
            }
            else
            {//源端为继承权限
                if (!securableObject.HasUniqueRoleAssignments)
                {//目的端为继承权限
                    restorePermissonComplete = true;
                }
                else
                {//目的端为独立权限
                    restorePermissonComplete = RestoreInheritanceInternal(securableObject, restoreOption);
                }
            }
        }

        protected virtual bool RestoreInheritanceInternal(IAveSecurableObject securableObject, SecurityRestoreOption restoreOption)
        {
            if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
            {
                securableObject.ResetRoleInheritance();
            }
            return true;        //源端继承时不再需要还原权限      
        }

        protected virtual void RestoreAnonymousPermSetting()
        {
        }

        protected virtual void ClearRoleAssignments()
        {
            for (int i = securableObject.RoleAssignments.Count - 1; i >= 0; i--)
            {
                securableObject.RoleAssignments.Remove(i);
            }
        }

        public abstract void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption);

        protected virtual void RestoreMemberAndMemberShip(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
        }

        public virtual void RestoreRoles(List<AveRoleInfo> roleInfos, SecurityRestoreOption restoreOption)
        {
        }

        public int RestoreRole(AveRoleInfo roleInfo, AveSPWeb aveWeb)
        {
            return RestoreRole(roleInfo, aveWeb, true);
        }

        //useCurrentWeb for list restore role.
        //aveWeb.RestorePermissionLevel   out side give us value.
        public int RestoreRole(AveRoleInfo roleInfo, AveSPWeb aveWeb, bool useCurrentWeb)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRole"))
            {
                IAveWeb web = aveWeb.SPWeb;
                bool needDispose = false;
                ThreadCultureProcesser cultureProcesser = null;
                try
                {
                    if (aveWeb.WebInfo.HasUniqueRoleDefinitions && aveWeb.RestorePermissionLevel && useCurrentWeb)
                    {
                        if (!aveWeb.SPWeb.HasUniqueRoleDefinitions)
                        {
                            if (!aveWeb.SPWeb.HasUniqueRoleAssignments)
                            {
                                aveWeb.SPWeb.BreakRoleInheritance(false);
                            }
                            aveWeb.SPWeb.RoleDefinitions.BreakInheritance(false, false);
                        }
                    }
                    else
                    {
                        web = aveWeb.SPWeb.FirstUniqueRoleDefinitionWeb;
                        needDispose = true;
                        if (web.ID != aveWeb.SPWeb.ID && aveWeb.ParentSite.AveLanguageProcesser != null)//切换web，需要切换Thread culture
                        {
                            cultureProcesser = new ThreadCultureProcesser();
                            cultureProcesser.ChangeThreadCulture(web.UICulture, web.UICulture);
                        }
                    }
                    if (aveWeb.ParentSite.AveLanguageProcesser != null)
                    {
                        uint realSourceLanguage = RetrieveSourceWebLanguage(aveWeb, roleInfo);
                        if (aveWeb.WebSrcLanguageId != realSourceLanguage
                            || aveWeb.SPWeb.Language != web.Language)
                        {
                            var tempProcesser = AveLanguageProcesser.CreateTempLanguageProcesser();
                            tempProcesser.LoadMapping(string.Empty, realSourceLanguage, web.Language, string.Empty);
                            string mappedValue;
                            if (tempProcesser.PermissionMapping.TryGetValue(roleInfo.Title, out mappedValue))
                            {
                                log.Debug("Mapping name by language mapping in temp language processer. Mapping from [{0}]  to [{1}],Type:[{2}]", roleInfo.Title, mappedValue, "PermissionMapping");
                                roleInfo.Title = mappedValue;
                            }
                        }
                        else
                        {
                            roleInfo.Title = aveWeb.ParentSite.GetNameByLanguageMapping(roleInfo.Title, AveLanguageMappingType.PermissionMapping);
                        }
                    }
                    bool needUpdate = false;
                    bool hasSamePermissions = false;
                    IAveRoleDefinition role = null;
                    try
                    {
                        try
                        {
                            role = web.RoleDefinitions[roleInfo.Title];
                        }
                        catch (Exception)
                        {
                            if (builtinRole.Contains(roleInfo.RoleId))
                            {
                                try
                                {
                                    role = web.RoleDefinitions.GetById(roleInfo.RoleId);
                                }
                                catch (Exception ex)
                                {
                                    log.Debug("Can not get builtin role by id {0}, title: {1}, error: {2}", roleInfo.RoleId, roleInfo.Title, ex);
                                }
                            }
                            if (role == null)
                            {
                                throw;
                            }
                        }
                        needUpdate = ((long)role.BasePermissions != roleInfo.PermMask) || (!role.Description.Equals(roleInfo.Description)) || (role.Order != roleInfo.RoleOrder);
                        if (needUpdate)
                        {
                            hasSamePermissions = ((long)role.BasePermissions == roleInfo.PermMask);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetRolePermissionError, ex.ToString());
                        try
                        {
                            role = ParentSite.ObjectModelFactory.CreateRoleDefinition();
                            role.Name = roleInfo.Title;
                            role.Description = roleInfo.Description;
                            role.Order = roleInfo.RoleOrder;
                            role.BasePermissions = (AveBasePermissions)roleInfo.PermMask;
                            web.RoleDefinitions.Add(role);
                            role = web.RoleDefinitions[roleInfo.Title];
                            needUpdate = true;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while adding role to web. WebUrl:{0}, RoleTitle:{1}, error:{2}", web.Url, roleInfo.Title, e.ToString());
                            return -1;
                        }
                    }
                    if (needUpdate)
                    {
                        if (!hasSamePermissions)
                        {
                            AveBasePermissions mRolePermissions = AveBasePermissions.EmptyMask;
                            if (roleInfo.PermMask == (long)AveBasePermissions.FullMask)
                            {
                                mRolePermissions = AveBasePermissions.FullMask;
                            }
                            else
                            {
                                foreach (AveBasePermissions perm in Enum.GetValues(typeof(AveBasePermissions)))
                                {
                                    if ((perm != AveBasePermissions.FullMask) && (((long)perm & roleInfo.PermMask) != 0))
                                    {
                                        mRolePermissions |= perm;
                                    }
                                }
                            }

                            //role.Hidden can't update by API
                            //role.Type  can't update by API  = (SPRoleType)Enum.ToObject(typeof(SPRoleType), roleInfo.Type);

                            role.BasePermissions = mRolePermissions;
                        }
                        role.Description = roleInfo.Description;
                        role.Order = roleInfo.RoleOrder;
                        role.Update();
                        aveWeb.ReloadWeb();
                        //TODO API update
                        //aveWeb.SqlConn.ClearParameters();
                        //aveWeb.SqlConn.AddParameter("@SiteId", aveWeb.ParentSite.SPSite.ID);
                        //aveWeb.SqlConn.AddParameter("@RoleId", role.Id);
                        //aveWeb.SqlConn.AddParameter("@WebId", web.ID);
                        //aveWeb.SqlConn.UpdateTableRow(dic, AveSP14DBTable.Roles.ToString(), ",RoleId,", " WHERE SiteId=@SiteId and WebId=@WebId and RoleId=@RoleId");
                    }
                    return role.ID;
                }
                finally
                {
                    if (needDispose && web != null)
                    {
                        web.Dispose();
                    }
                    if(cultureProcesser != null)
                    {
                        cultureProcesser.RestoreThreadCulture();
                    }
                }

            }

        }

        private class ThreadCultureProcesser
        {
            private CultureInfo threadUICulture = null;
            private CultureInfo threadCurrentCulture = null;

            public void ChangeThreadCulture(CultureInfo threadUICulture, CultureInfo threadCurrentCulture)
            {
                try
                {
                    if (Thread.CurrentThread.CurrentUICulture != threadUICulture)
                    {
                        threadUICulture = Thread.CurrentThread.CurrentUICulture;
                        Thread.CurrentThread.CurrentUICulture = threadUICulture;
                    }
                    if (Thread.CurrentThread.CurrentCulture != threadCurrentCulture)
                    {
                        threadCurrentCulture = Thread.CurrentThread.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = threadCurrentCulture;
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while set CurrentUICulture of current thread. Error: {0}", e));
                }
            }

            public void RestoreThreadCulture()
            {
                try
                {
                    if (threadUICulture != null)
                    {
                        Thread.CurrentThread.CurrentUICulture = threadUICulture;
                    }
                    if (threadCurrentCulture != null)
                    {
                        Thread.CurrentThread.CurrentCulture = threadCurrentCulture;
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while setting thread culture back. Error: {0}", e);
                }
            }
        }

        private uint RetrieveSourceWebLanguage(AveSPWeb web, AveRoleInfo role)
        {
            List<string> EnglishRoleNames = new List<string> { "Full Control", "Design", "Contribute", "Read", "Limited Access", "View Only" };
            List<string> JapanRoleNames = new List<string> { "フル コントロール", "デザイン" , "投稿", "閲覧", "制限付きアクセス", "表示のみ" };
            List<string> FrenchRoleNames = new List<string> { "Affichage seul", "Accès limité", "Lecture", "Collaboration", "Conception", "Contrôle total" };
            List<string> GermanRoleNames = new List<string> { "Nur anzeigen", "Beschränkter Zugriff", "Lesen", "Mitwirken", "Entwerfen", "Vollzugriff" };
            var processer = web.ParentSite.AveLanguageProcesser;
            //当源端是SP10或SP07时，Permission Level的Title不一定是web当前语言，需要重新获取，否则出现双份。
            if (processer.IsMigration && (processer.SourcePlatForm == AveSourceLanguagePlatForm.Sharepoint10 || processer.SourcePlatForm == AveSourceLanguagePlatForm.Sharepoint07)
                || (web.ParentSite.SPContextKind == AveContextKind.Server10ObjectModel || web.ParentSite.SPContextKind == AveContextKind.ServerObjectModel))
            {
                if(CheckIfElementAtList(EnglishRoleNames,role.Title))
                {
                    return 1033;
                }
                if(CheckIfElementAtList(JapanRoleNames,role.Title))
                {
                    return 1041;
                }
                if (CheckIfElementAtList(FrenchRoleNames, role.Title))
                {
                    return 1036;
                }
                if (CheckIfElementAtList(GermanRoleNames, role.Title))
                {
                    return 1031;
                }
            }
            return web.WebSrcLanguageId;
        }
        private bool CheckIfElementAtList(List<string> names, string roleTitle )
        {
            return names.Exists(name => name.Equals(roleTitle, StringComparison.OrdinalIgnoreCase));
        }

        public IAveRoleDefinition GetRoleWithCache(int oldId, AveSPWeb aveWeb)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.GetRoleWithCache"))
            {

                object x;
                if (!aveWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache.TryGetValue(oldId, out x))
                {
                    return null;
                }
                int newId;
                if (x is AveRoleInfo)
                {
                    newId = RestoreRole((AveRoleInfo)x, aveWeb);
                    aveWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(oldId, newId);
                }
                else if (x is int)
                {
                    newId = (int)x;
                }
                else
                {
                    return null;
                }
                IAveRoleDefinition roleDefinition = null;
                try
                {
                    roleDefinition = aveWeb.SPWeb.RoleDefinitions.GetById(newId);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.CannotGetRoleDefinition, newId, e);
                }
                return roleDefinition;

            }

        }

        public virtual void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {
        }

        protected virtual void RestoreRoleAssignment(AveRoleAssignmentInfo roleAssignmentInfo, AveSecurityParameters securityParam)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignment"))
            {

                IAveRoleDefinition spRoleDefinition = GetRoleWithCache(roleAssignmentInfo.RoleId, securityParam.aveSPWeb);
                if (spRoleDefinition == null)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(roleAssignmentInfo.RoleName))
                        {
                            roleAssignmentInfo.RoleName = securityParam.aveSPWeb.ParentSite.GetNameByLanguageMapping(roleAssignmentInfo.RoleName, AveLanguageMappingType.PermissionMapping);
                            spRoleDefinition = securityParam.aveSPWeb.SPWeb.RoleDefinitions[roleAssignmentInfo.RoleName];
                        }
                        else
                        {
                            spRoleDefinition = securityParam.aveSPWeb.SPWeb.RoleDefinitions.GetById(roleAssignmentInfo.RoleId);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.WARN, WrapperRestoreResource.CannotFindRole, roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId, e);
                        return;
                    }
                }
                IAvePrincipal member = securityParam.aveSPWeb.ParentSite.SPMembers.FindMember(roleAssignmentInfo.PrincipalId, true);
                if (member == null)
                {
                    return;
                }

                IAveRoleAssignment spRoleAssignment = ParentSite.ObjectModelFactory.CreateRoleAssignment(member);
                int count = securityParam.roleAssignments.GetRoleAssignmentCount(securityParam.roleAssignments.ID, roleAssignmentInfo.RoleId, member.ID);
                //allowed to add limited access role
                if (count == 0)
                {
                    IAveRoleAssignmentCollection roleAssginmens = securityParam.roleAssignments;
                    spRoleAssignment.RoleDefinitionBindings.Add(spRoleDefinition);
                    roleAssginmens.Add(spRoleAssignment);
                }

            }

        }

        protected virtual void RestoreRoleAssignment(int pricipalId, List<AveRoleAssignmentInfo> roleAssignmentInfos, AveSecurityParameters securityParam, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignment"))
            {

                //if (pricipalId == AveConstants.SYSTEM_ACCOUNT_ID)
                //{
                //    return;
                //}
                IAvePrincipal member = securityParam.aveSPWeb.ParentSite.SPMembers.FindMember(pricipalId, true);
                if (member == null)
                {
                    log.Warn("Cannot find one user/group with principal id. PrincipalId:{0}", pricipalId);
                    return;
                }
                bool isNewCreated = true;
                IAveRoleAssignment spRoleAssignment = null;
                if (restoreOption.ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite)//OverWriteItemPermission为true时，已经把所有的RoleAssignment全部Remove掉了
                {
                    try
                    {
                        spRoleAssignment = securityParam.roleAssignments.GetAssignmentByPrincipal(member);
                        if (spRoleAssignment != null)
                        {
                            List<int> roleIds = new List<int>();
                            foreach (AveRoleAssignmentInfo info in roleAssignmentInfos)
                            {
                                IAveRoleDefinition roleDefinition = GetRoleWithCache(info.RoleId, securityParam.aveSPWeb);
                                if (roleDefinition != null)
                                {
                                    roleIds.Add(roleDefinition.ID);
                                }
                            }
                            bool changed = false;
                            for (int i = spRoleAssignment.RoleDefinitionBindings.Count - 1; i >= 0; i--)
                            {
                                var roleDefinition = spRoleAssignment.RoleDefinitionBindings[i];
                                if (!roleIds.Contains(roleDefinition.ID) && !IsLimitAccessRole(roleDefinition))
                                {
                                    spRoleAssignment.RoleDefinitionBindings.Remove(i);
                                    changed = true;
                                }
                            }
                            isNewCreated = false;
                            if (changed && this.ParentSite != null && this.ParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                            {
                                spRoleAssignment.Update();
                            }
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                        //report.AddDetail(new AveWrapperReportDto("RoleAssignments", "RoleAssignments", AveReportObjectType.ListRoleAssignments, AveStatus.Skipped, "You don't have permission to restore list roleassignments. " + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Info("An error occurred while remove permission. error:{0}", e.ToString());
                    }
                }
                if (spRoleAssignment == null)
                {
                    //spRoleAssignment = mAveParentSite.ObjectModelFactory.CreateRoleAssignment(member);
                    spRoleAssignment = securityParam.roleAssignments.CreateRoleAssignment(member);
                }
                IAveRoleDefinitionBindingCollection roleAssignmentBindingCol = spRoleAssignment.RoleDefinitionBindings;
                foreach (AveRoleAssignmentInfo info in roleAssignmentInfos)
                {
                    try
                    {
                        IAveRoleDefinition spRoleDefinition = GetRoleWithCache(info.RoleId, securityParam.aveSPWeb);
                        if (spRoleDefinition == null)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(info.RoleName))
                                {
                                    info.RoleName = securityParam.aveSPWeb.ParentSite.GetNameByLanguageMapping(info.RoleName, AveLanguageMappingType.PermissionMapping);
                                    spRoleDefinition = securityParam.aveSPWeb.SPWeb.RoleDefinitions[info.RoleName];
                                }
                                else
                                {
                                    spRoleDefinition = securityParam.aveSPWeb.SPWeb.RoleDefinitions.GetById(info.RoleId);
                                }
                            }
                            catch (Exception e)
                            {//反插role，需要外围去load roleInfo
                                log.Info(WrapperRestoreResource.NeedRestoreRolesHere, e);
                                var roleInfo = securityParam.aveSPWeb.GetRoleByName(info.RoleName);
                                var newId = RestoreRole(roleInfo, securityParam.aveSPWeb, false);
                                if (roleInfo != null && newId > 0)
                                {
                                    securityParam.aveSPWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(securityParam.aveSPWeb.GetRoleByName(info.RoleName).RoleId, newId);
                                    spRoleDefinition = securityParam.aveSPWeb.SPWeb.FirstUniqueRoleDefinitionWeb.RoleDefinitions[info.RoleName];
                                }
                                else
                                {
                                    log.Info(WrapperRestoreResource.RestoreRolesWhenRestoreRoleAssignmentFailed, securableObject);
                                    continue;
                                }
                            }
                        }
                        if (spRoleDefinition != null)
                        {
                            int count = securityParam.roleAssignments.GetRoleAssignmentCount(securityParam.roleAssignments.ID, spRoleDefinition.ID, member.ID);

                            if (count == 0 && spRoleDefinition.ID != 1073741825)
                            {
                                roleAssignmentBindingCol.Add(spRoleDefinition);
                            }
                        }
                        else
                        {
                            log.Warn("Cannot find role in role definition cache. RoleId:{0}, PrincipalId:{1}, RoleName:{2}",
                                        info.RoleId, info.PrincipalId, info.RoleName);
                        }
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                        //report.AddDetail(new AveWrapperReportDto("RestoreRoleAssignments", "RestoreRoleAssignments", AveReportObjectType.ListRoleAssignments, AveStatus.Skipped, "you don't have permission to restore list roleassignments. " + ex.Message));
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while restore roleAssignmentsInfo. info.RoleId:{0}\n error message:{1} ", info.RoleId, e);
                    }
                }
                try
                {
                    if (roleAssignmentBindingCol.Count > 0)
                    {
                        securityParam.roleAssignments.Add(spRoleAssignment);
                    }
                    if (!isNewCreated && this.ParentSite != null && this.ParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                    {
                        spRoleAssignment.Update();
                    }
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                    //report.AddDetail(new AveWrapperReportDto("RestoreRoleAssignments", "RestoreRoleAssignments", AveReportObjectType.ListRoleAssignments, AveStatus.Skipped, "you don't have permission to restore list roleassignments. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restore roleAssignmentsInfo.\n Error message:{0} ", e);
                }

            }

        }
        public Dictionary<int, List<AveRoleAssignmentInfo>> GroupRoleAssignmentInfos(List<AveRoleAssignmentInfo> roleAssignmentInfos)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.ProcessItemByWeb"))
            {

                Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfos = new Dictionary<int, List<AveRoleAssignmentInfo>>();
                foreach (AveRoleAssignmentInfo info in roleAssignmentInfos)
                {
                    if (groupRoleAssignmentInfos.ContainsKey(info.PrincipalId))
                    {
                        groupRoleAssignmentInfos[info.PrincipalId].Add(info);
                    }
                    else
                    {
                        groupRoleAssignmentInfos[info.PrincipalId] = new List<AveRoleAssignmentInfo>();
                        groupRoleAssignmentInfos[info.PrincipalId].Add(info);
                    }
                }
                return groupRoleAssignmentInfos;

            }

        }

        public virtual void Restore(AveMemberInfoCollection memeberInfoCol, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.Restore"))
            {

                securableObject.BreakRoleInheritance(false);
                foreach (AveMemberInfo memberInfo in memeberInfoCol.MemberInfo)
                {
                    IAvePrincipal principal = null;
                    IAveRoleAssignmentCollection roleAssignmentCollection = securableObject.RoleAssignments;
                    try
                    {
                        switch (memberInfo.Type)
                        {
                            case AveAccessMemberType.BuiltInGroup:
                            case AveAccessMemberType.DomainGroup:
                            case AveAccessMemberType.OriginalSystemGroup:
                                principal = mWeb.SiteGroups[memberInfo.Name];
                                break;
                            case AveAccessMemberType.BuildInUser:
                            case AveAccessMemberType.DomainUser:
                            case AveAccessMemberType.OriginalSystemUser:
                                try
                                {
                                    principal = mWeb.EnsureAvailableUser(memberInfo.Name);
                                }
                                catch (Exception e)
                                {
                                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestoreUserFailedEventMessage(memberInfo.Name, e));
                                }
                                break;
                            default:
                                principal = null;
                                break;
                        }
                        if (principal != null)
                        {
                            IAveRoleAssignment roleAssignment = ParentSite.ObjectModelFactory.CreateRoleAssignment(principal);
                            foreach (AveRoleType roleType in memberInfo.Role)
                            {
                                roleAssignment.RoleDefinitionBindings.Add(mWeb.RoleDefinitions.GetByType(roleType));
                            }
                            roleAssignmentCollection.Add(roleAssignment);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("An error occurred while exporting security.", e);
                    }
                }

            }

        }


        public virtual void RestoreRoles(List<AveRoleInfo> roleInfos)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoles"))
            {

                if (!mWeb.IsRootWeb)
                {
                    mWeb.RoleDefinitions.BreakInheritance(true, true);
                }
                foreach (AveRoleInfo roleInfo in roleInfos)
                {
                    IAveRoleDefinition roleDef = mWeb.RoleDefinitions.GetByName(roleInfo.Title);
                    if (roleDef != null)
                    {
                        continue;
                    }
                    //if (!mWeb.IsRootWeb && !mWeb.HasUniqueRoleAssignments)               
                    AveRoleDefinitionCreationInformation role = new AveRoleDefinitionCreationInformation();
                    role.Name = roleInfo.Title;
                    role.Description = roleInfo.Description;
                    role.BasePermissions = (AveBasePermissions)roleInfo.PermMask;
                    mWeb.RoleDefinitions.Add(role);
                }

            }

        }
        /// <summary>
        /// 以后各个级别的security还原可以通过来个函数来还原，因为把所有的option都关联上了。
        /// </summary>
        /// <param name="roleAssignmentInfos"></param>
        /// <param name="spRoleAssignColl"></param>
        /// <param name="aveSPWeb"></param>
        /// <param name="restoreOption"></param>
        internal void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, IAveRoleAssignmentCollection spRoleAssignColl, AveSPWeb aveSPWeb, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignments"))
            {


                Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfo = GroupRoleAssignmentInfos(roleAssignmentInfos);

                Dictionary<int, IAveRoleAssignment> cacheUserAndGroup = new Dictionary<int, IAveRoleAssignment>();

                foreach (IAveRoleAssignment roleAssignment in spRoleAssignColl)
                {
                    int memberId = roleAssignment.Member.ID;
                    if (!cacheUserAndGroup.ContainsKey(memberId))
                    {
                        cacheUserAndGroup[memberId] = roleAssignment;
                    }
                }

                foreach (KeyValuePair<int, List<AveRoleAssignmentInfo>> keyValue in groupRoleAssignmentInfo)
                {
                    try
                    {
                        //if (keyValue.Key == AveConstants.SYSTEM_ACCOUNT_ID)
                        //{
                        //    if (cacheUserAndGroup.ContainsKey(keyValue.Key))
                        //    {
                        //        cacheUserAndGroup.Remove(keyValue.Key);
                        //    }
                        //    continue;
                        //}
                        IAvePrincipal member = aveSPWeb.ParentSite.SPMembers.FindMember(keyValue.Key, true);
                        if (member == null)
                        {
                            log.Log(AveLogLevel.WARN, string.Format("Cannot find one user/group with principal id. PrincipalId:{0}", keyValue.Key));
                            continue;
                        }

                        IAveRoleAssignment spRoleAssignment = null;
                        bool needToAdd = false;

                        if (cacheUserAndGroup.ContainsKey(member.ID))
                        {
                            spRoleAssignment = cacheUserAndGroup[member.ID];
                            cacheUserAndGroup.Remove(member.ID);
                            if (restoreOption.ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite || restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                            {
                                RemoveRoleDefinationsExceptLimitedAccess(spRoleAssignment, true);
                            }
                        }
                        else
                        {
                            spRoleAssignment = ParentSite.ObjectModelFactory.CreateRoleAssignment(member);
                            needToAdd = true;
                        }

                        foreach (AveRoleAssignmentInfo info in keyValue.Value)
                        {
                            try
                            {
                                IAveRoleDefinition spRoleDefinition = GetRoleWithCache(info.RoleId, aveSPWeb);
                                if (spRoleDefinition == null)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(info.RoleName))
                                        {
                                            info.RoleName = aveSPWeb.ParentSite.GetNameByLanguageMapping(info.RoleName, AveLanguageMappingType.PermissionMapping);
                                            spRoleDefinition = aveSPWeb.SPWeb.RoleDefinitions[info.RoleName];
                                        }
                                        else
                                        {
                                            spRoleDefinition = aveSPWeb.SPWeb.RoleDefinitions.GetById(info.RoleId);
                                        }
                                    }
                                    catch (Exception e)
                                    {//反插role，需要外围去load roleInfo
                                        log.Info(WrapperRestoreResource.NeedRestoreRolesHere, e);
                                        var roleInfo = aveSPWeb.GetRoleByName(info.RoleName);
                                        var newId = RestoreRole(roleInfo, aveSPWeb, false);
                                        if (roleInfo != null && newId > 0)
                                        {
                                            aveSPWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(aveSPWeb.GetRoleByName(info.RoleName).RoleId, newId);
                                            spRoleDefinition = aveSPWeb.SPWeb.FirstUniqueRoleDefinitionWeb.RoleDefinitions[info.RoleName];
                                        }
                                        else
                                        {
                                            log.Info(WrapperRestoreResource.RestoreRolesWhenRestoreRoleAssignmentFailed, securableObject);
                                            continue;
                                        }
                                    }
                                }
                                if (spRoleDefinition != null && spRoleDefinition.ID != 1073741825)
                                {
                                    if (!spRoleAssignment.RoleDefinitionBindings.Contains(spRoleDefinition))
                                    {
                                        spRoleAssignment.RoleDefinitionBindings.Add(spRoleDefinition);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Warn("Restore role definition failed, RoleId:{0}, PrincipalId:{1}, Exception:{2}", info.RoleId, info.PrincipalId, ex.ToString());
                            }
                        }

                        if (needToAdd)
                        {
                            if (spRoleAssignment.RoleDefinitionBindings.Count > 0)
                            {
                                spRoleAssignColl.Add(spRoleAssignment);
                            }
                        }
                        else if (spRoleAssignment.RoleDefinitionBindings.Count > 0)
                        {
                            spRoleAssignment.Update();
                        }
                        else if (spRoleAssignment.RoleDefinitionBindings.Count == 0)
                        {
                            spRoleAssignColl.Remove(spRoleAssignment.Member);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("An error occurred while restoring role assignment. Exception:{0}", ex.ToString());
                    }
                }

                if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                {
                    foreach (KeyValuePair<int, IAveRoleAssignment> keyValue in cacheUserAndGroup)
                    {
                        if (keyValue.Value.RoleDefinitionBindings.All(roleDefinition => !IsLimitAccessRole(roleDefinition)))
                        {
                            spRoleAssignColl.RemoveById(keyValue.Key);
                        }
                        else
                        {
                            RemoveRoleDefinationsExceptLimitedAccess(keyValue.Value);
                        }
                    }
                }

            }

        }

        private static void RemoveRoleDefinationsExceptLimitedAccess(IAveRoleAssignment spRoleAssignment, bool needPostUpdate = false)
        {
            for (int i = spRoleAssignment.RoleDefinitionBindings.Count - 1; i >= 0; --i)
            {
                var roleDefinition = spRoleAssignment.RoleDefinitionBindings[i];
                if (IsLimitAccessRole(roleDefinition))
                {
                    continue;
                }
                spRoleAssignment.RoleDefinitionBindings.Remove(i);
            }
            if (!needPostUpdate)
            {
                spRoleAssignment.Update();
            }
        }

        public class AveSecurityParameters
        {
            public string scopeString;
            public AveSPWeb aveSPWeb;
            public IAveRoleAssignmentCollection roleAssignments;
        }
        protected static bool IsLimitAccessRole(IAveRoleDefinition roleDefinition)
        {
            return roleDefinition.Name.Equals("Limited Access", StringComparison.OrdinalIgnoreCase) || (roleDefinition.ID == AveConstants.LIMIT_ACCESS_ROLE_ID && roleDefinition.ParentWeb.Language != 1033);
        }

        #region IAveObjectSecurity Members


        public IAveRoleDefinition GetRoleWithCache(int oldId, IAveSPWeb aveWeb)
        {
            return GetRoleWithCache(oldId, aveWeb as AveSPWeb);
        }

        public IAveUser GetSPUser(int principalId, IAveSPWeb aveSPWeb)
        {
            return GetSPUser(principalId, aveSPWeb as AveSPWeb);
        }

        IAveSPSite IAveObjectSecurity.ParentSite
        {
            get { return ParentSite; }
        }

        public int RestoreRole(AveRoleInfo roleInfo, IAveSPWeb aveWeb)
        {
            return RestoreRole(roleInfo, aveWeb as AveSPWeb);
        }

        #endregion
        public void Dispose()
        {
            report.Dispose();
        }
    }

    public class AveSiteSecurity : AveObjectSecurity
    {
        public AveSiteSecurity(AveSPSite aveSite)
        {
            ParentSite = aveSite;
            IsNewCreatedObject = aveSite.IsNewCreated;
        }

        public override void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSiteSecurity.Restore"))
            {

                try
                {
                    RestoreMemberAndMemberShip(securityInfo, restoreOption);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restoring site members and membership. SiteUrl:{0}, error:{1}",
                        (ParentSite == null || ParentSite.SPSite == null) ? "" : ParentSite.SPSite.Url, e.ToString());
                }

            }

        }

        protected override void RestoreMemberAndMemberShip(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
            if (restoreOption.NeedRestore)
            {
                ParentSite.SPMembers.RestoreMembers(securityInfo);
            }
            else
            {
                ParentSite.SPMembers.LoadMembers(securityInfo);
            }
        }
    }

    public class AveWebSecurity : AveObjectSecurity
    {
        private AveSPWeb mAveSPWeb;

        public AveWebSecurity(AveSPWeb aveWeb)
            : base(aveWeb.SPWeb, aveWeb.ParentSite, aveWeb.SPWeb)
        {
            mAveSPWeb = aveWeb;
            IsNewCreatedObject = aveWeb.IsNewCreated;
        }

        public override void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWebSecurity.Restore"))
            {

                try
                {
                    RestoreMemberAndMemberShip(securityInfo, restoreOption);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restoring web members and membership. WebUrl:{0}, error:{1}", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
                }

                try
                {
                    RestoreRoles(securityInfo.Roles, restoreOption);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restoring web roles. WebUrl:{0}, error:{1}", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
                }
                try
                {
                    RestoreRoleAssignments(securityInfo.RoleAssignments, restoreOption);
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restoring web role assignments. WebUrl:{0}, error:{1}", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
                }

            }

        }

        public override void RestoreRoles(List<AveRoleInfo> roleInfos, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWebSecurity.WebRoles"))
            {

                try
                {
                    if (roleInfos == null)
                    {
                        return;
                    }

                    //bool restoreComplete = false;

                    //RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restoreComplete);

                    //if (restoreComplete)
                    //{
                    //    return;
                    //}

                    foreach (AveRoleInfo roleInfo in roleInfos)
                    {
                        try
                        {
                            if (restoreOption.NeedRestore)
                            {
                                int newId = RestoreRole(roleInfo, mAveSPWeb);
                                if (newId > 0)
                                {
                                    mAveSPWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(roleInfo.RoleId, newId);
                                }
                            }
                            else
                            {
                                mAveSPWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(roleInfo.RoleId, roleInfo);
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while restoring role. RoleId:{0}, RoleTitle:{1}, error:{2}", roleInfo.RoleId, roleInfo.Title, e.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restoring web role. ", ex);
                    report.AddDetail(new AveWrapperReportDto(mAveSPWeb.SPWeb.Name, mAveSPWeb.SPWeb.Name, AveReportObjectType.WebRoles, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreRoles, ex.Message));
                }

            }

        }

        protected override void RestoreMemberAndMemberShip(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
            mAveSPWeb.needListRestore = !restoreOption.NeedRestore;
            if (restoreOption.NeedRestore)
            {
                mAveSPWeb.ParentSite.SPMembers.RestoreMembers(securityInfo);
            }
        }

        protected override bool RestoreInheritanceInternal(IAveSecurableObject securableObject, SecurityRestoreOption restoreOption)
        {
            var web = securableObject as IAveWeb;
            if (web.IsRootWeb && restoreOption.PromotePermissionToRootWeb)
            {
                return false;
            }

            if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
            {
                if (!web.IsRootWeb)
                {
                    securableObject.ResetRoleInheritance();
                }
                return true;
            }
            else
            {
                return !restoreOption.MergePermissionFromInheritanceWeb;
            }
        }

        public override void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebRoleAssignments"))
            {

                try
                {
                    if (!restoreOption.NeedRestore)
                    {
                        return;
                    }


                    bool restoreComplete = false;

                    RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restoreComplete);

                    if (restoreComplete)
                    {
                        return;
                    }

                    if (roleAssignmentInfos == null)
                    {
                        return;
                    }

                    ClearDefaultRoleAssignment();

                    if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                    {
                        try
                        {
                            RestoreRoleAssignments(roleAssignmentInfos, mAveSPWeb.SPWeb.RoleAssignments, mAveSPWeb, restoreOption);
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while remove roleAssignments. WebUrl:{0}, error:{1}.", mAveSPWeb.SPWeb.Url, e.ToString());
                        }
                        return;
                    }

                    AveSecurityParameters securityParam = new AveSecurityParameters { aveSPWeb = mAveSPWeb, roleAssignments = mAveSPWeb.SPWeb.RoleAssignments };
                    Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfo = GroupRoleAssignmentInfos(roleAssignmentInfos);
                    foreach (int principalId in groupRoleAssignmentInfo.Keys)
                    {
                        securityParam.roleAssignments = mAveSPWeb.SPWeb.RoleAssignments;
                        RestoreRoleAssignment(principalId, groupRoleAssignmentInfo[principalId], securityParam, restoreOption);
                    }
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestorePermissionFailedEventMessage(e));
                    //log.Warn("An error occurred while restore the web roleassignments. ", ex);
                    report.AddDetail(new AveWrapperReportDto("WebRoleAssignments", "WebRoleAssignments", AveReportObjectType.RoleAssignment, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreWebRoleAssignments, e.Message));
                }

            }

        }

        /// <summary>
        /// 该方法用于清除新创建的RootWeb上的role assignment
        /// </summary>
        private void ClearDefaultRoleAssignment()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWebSecurity.ClearDefaultRoleAssignment"))
            {
                try
                {
                    if (this.mAveSPWeb.SPWeb.IsRootWeb && this.mAveSPWeb.ParentSite.IsNewCreated)
                    {
                        IAveRoleAssignmentCollection roleAssignmentCollection = mAveSPWeb.SPWeb.RoleAssignments;
                        for (int k = roleAssignmentCollection.Count - 1; k >= 0; k--)
                        {
                            var roleAssignment = roleAssignmentCollection[k];
                            bool needUpdate = false;
                            //由于Limited Access role definition不应该清除，所以用清除roleAssignment上role definition的方法来清除权限，并对Limited Access加以限制，不进行清除
                            //如果单纯remove role assignment，会将limited access 同时移除，会导致子节点上对应的打破继承节点的权限同时被移除
                            for (int i = roleAssignment.RoleDefinitionBindings.Count - 1; i >= 0; i--)
                            {
                                var roleDefinition = roleAssignment.RoleDefinitionBindings[i];
                                if (!IsLimitAccessRole(roleDefinition))
                                {
                                    needUpdate = true;
                                    roleAssignment.RoleDefinitionBindings.Remove(i);

                                }
                            }
                            if (needUpdate)
                            {
                                roleAssignment.Update();
                            }
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ClearAllRoleAssignments, this.mAveSPWeb.Url, ex);
                    throw;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.ClearAllRoleAssignments, this.mAveSPWeb.Url, e);
                }
            }
        }

        protected override void RestoreAnonymousPermSetting()
        {
            if (mAveSPWeb.WebSettingInfo != null && mAveSPWeb.WebSettingInfo.AnonymousState != null && mAveSPWeb.WebSettingInfo.AnonymousState.IsAvailable && (int)mAveSPWeb.SPWeb.AnonymousState != mAveSPWeb.WebSettingInfo.AnonymousState.Value)
            {
                mAveSPWeb.SPWeb.AnonymousState = (AveWebAnonymousState)mAveSPWeb.WebSettingInfo.AnonymousState.Value;
            }
        }
    }

    public class AveListSecurity : AveObjectSecurity
    {
        private AveSPList mAveSPList;

        public AveListSecurity(AveSPList aveList)
            : base(aveList.SPList, aveList.ParentWeb.ParentSite, aveList.ParentWeb.SPWeb)
        {
            mAveSPList = aveList;
            IsNewCreatedObject = aveList.IsNewCreated;
        }

        public override void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
            RestoreRoleAssignments(securityInfo.RoleAssignments, restoreOption);
        }

        public override void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveListSecurity.RestoreRoleAssignments"))
            {

                try
                {
                    if (!restoreOption.NeedRestore)
                    {
                        return;
                    }

                    bool restorePermissionComplete = false;

                    RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restorePermissionComplete);

                    if (restorePermissionComplete)
                    {
                        return;
                    }

                    if (roleAssignmentInfos == null)
                    {
                        return;
                    }

                    if (mAveSPList.IsNewCreated || mAveSPList.ParentWeb.IsNewCreated || mAveSPList.ParentSite.IsNewCreated)
                    {
                        restoreOption.ConflictResolutionForSecurityObject = ConflictResolutionForSecurityObject.OverWrite;
                        restoreOption.ConflictResolutionForPincipal = ConflictResolutionForPincipal.OverWrite;
                    }
                    if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                    {
                        try
                        {
                            RestoreRoleAssignments(roleAssignmentInfos, mAveSPList.SPList.RoleAssignments, mAveSPList.ParentWeb, restoreOption);
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while remove roleAssignments. listUrl:{0}, error:{1}.", mAveSPList.SPList.RootFolder.ServerRelativeUrl, e.ToString());
                        }
                        return;
                    }
                    AveSecurityParameters securityParam = new AveSecurityParameters();
                    securityParam.aveSPWeb = mAveSPList.ParentWeb;
                    securityParam.roleAssignments = mAveSPList.SPList.RoleAssignments;
                    Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfo = GroupRoleAssignmentInfos(roleAssignmentInfos);
                    foreach (int principalId in groupRoleAssignmentInfo.Keys)
                    {
                        try
                        {
                            RestoreRoleAssignment(principalId, groupRoleAssignmentInfo[principalId], securityParam, restoreOption);
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while restoring list role assignment. ListId:{0}, ListTitle:{1}, error:{2}", mAveSPList.SPList.ID, mAveSPList.SPList.Title, e.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restoring list role assignment. ", ex);
                    report.AddDetail(new AveWrapperReportDto("ListRoleAssignment", "ListRoleAssignment", AveReportObjectType.RoleAssignment, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionToRestoreListRoleAssignments, ex.Message));
                }

            }

        }

        protected override void RestoreAnonymousPermSetting()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveListSecurity.ReSignRoleAssignment"))
            {

                if (mAveSPList.ListSettingInfo != null && mAveSPList.ListSettingInfo.AnonymousPermMask64 != null && mAveSPList.ListSettingInfo.AnonymousPermMask64.IsAvailable && mAveSPList.SPList.AnonymousPermMask64 != (AveBasePermissions)mAveSPList.ListSettingInfo.AnonymousPermMask64.Value)
                {
                    mAveSPList.SPList.AnonymousPermMask64 = (AveBasePermissions)mAveSPList.ListSettingInfo.AnonymousPermMask64.Value;
                }

            }


        }
    }

    public class AveItemSecurity : AveObjectSecurity
    {
        private AveSPItem mAveSPItem;

        public AveItemSecurity(AveSPItem aveItem)
            : base(aveItem.SPListItem, aveItem.ParentFolder.ParentList.ParentWeb.ParentSite, aveItem.ParentFolder.ParentList.ParentWeb.SPWeb)
        {
            mAveSPItem = aveItem;
            IsNewCreatedObject = aveItem.IsNewCreated;
        }

        public override void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {


            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveItemSecurity.Restore"))
            {

                RestoreRoleAssignments(securityInfo.RoleAssignments, restoreOption);

            }

        }

        public override void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {
            if (!restoreOption.NeedRestore || mAveSPItem.SPListItem == null)// || roleAssignmentInfos == null)
            {
                log.Debug("The item permission hasn't been restored. Option NeedRestore: {0}, ListItem is null: {1}", restoreOption.NeedRestore, mAveSPItem.SPListItem == null);
                return;
            }

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveItemSecurity.RestoreRoleAssignments"))
            {

                bool restoreComplete;
                try
                {
                    RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restoreComplete);
                    if (restoreComplete)
                    {
                        return;
                    }

                    if (roleAssignmentInfos == null)
                    {
                        return;
                    }

                    if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                    {
                        try
                        {
                            RestoreRoleAssignments(roleAssignmentInfos, mAveSPItem.SPListItem.RoleAssignments, mAveSPItem.ParentFolder.ParentList.ParentWeb, restoreOption);
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while remove roleAssignments. ItemUrl:{0}, error:{1}.", mAveSPItem.SPListItem.Url, e.ToString());
                        }
                        return;
                    }
                    AveSecurityParameters securityParam = new AveSecurityParameters();
                    securityParam.aveSPWeb = mAveSPItem.ParentFolder.ParentList.ParentWeb;
                    securityParam.roleAssignments = mAveSPItem.SPListItem.RoleAssignments;
                    Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfo = GroupRoleAssignmentInfos(roleAssignmentInfos);
                    foreach (int principalId in groupRoleAssignmentInfo.Keys)
                    {
                        try
                        {
                            if (!mAveSPItem.SPListItem.HasUniqueRoleAssignments)
                            {
                                mAveSPItem.SPListItem.BreakRoleInheritance(restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.Merge);
                                securityParam.roleAssignments = mAveSPItem.SPListItem.RoleAssignments;
                            }

                            RestoreRoleAssignment(principalId, groupRoleAssignmentInfo[principalId], securityParam, restoreOption);
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Warn("An error occurred while restoring list item role assignment. ListItem Id:{0}, ListItem Title:{1}, error:{2}", mAveSPItem.SPListItem.ID, mAveSPItem.SPListItem.Title, e.ToString());
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Warn("An error occurred while restoring item role assignment. ", ex);
                    report.AddDetail(new AveWrapperReportDto("ItemRoleAssignments", "ItemRoleAssignments", AveReportObjectType.RoleAssignment, AveStatus.Skipped, AveReportResource.Wrapper_Report_NoPermissionRestoreItemRoleAssignment, ex.Message));
                }


            }


        }
    }

    #region moved to wrapper contract
    //public class SecurityRestoreOption
    //{
    //    public bool NeedRestore = true;
    //    [Obsolete("use ConflictResolutionForPincipal instead ")]
    //    public bool OverWritePermission //对某个user的permission的控制
    //    {
    //        set
    //        {
    //            ConflictResolutionForPincipal = value ? ConflictResolutionForPincipal.OverWrite : ConflictResolutionForPincipal.Merge;
    //        }
    //        get
    //        {
    //            return ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite;
    //        }
    //    }
    //    [Obsolete("use ConflictResolutionForSecurityObject instead")]
    //    public bool OverWriteItemPermission //对web、list、item级别的所有permission的控制
    //    {
    //        set
    //        {
    //            ConflictResolutionForSecurityObject = value ? ConflictResolutionForSecurityObject.OverWrite : ConflictResolutionForSecurityObject.Merge;
    //        }
    //        get
    //        {
    //            return ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite;
    //        }
    //    }

    //    public ConflictResolutionForSecurityObject ConflictResolutionForSecurityObject { set; get; }
    //    public ConflictResolutionForPincipal ConflictResolutionForPincipal { set; get; }
    //    public bool PromotePermissionToRootWeb { set; get; }
    //}

    //public enum ConflictResolutionForSecurityObject
    //{
    //    Merge = 0,
    //    OverWrite
    //    //MergefromInherited
    //}

    //public enum ConflictResolutionForPincipal
    //{
    //    Merge = 0,
    //    OverWrite
    //}
    #endregion

}
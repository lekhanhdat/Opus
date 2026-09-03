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
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility.I18N;
using System.Text.RegularExpressions;

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

    public abstract class AveObjectSecurity : IDisposable
    {
        public static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveSPSite ParentSite { get; protected set; }

        private IAveWeb mWeb;

        public bool SourceHasUniqueRoleAssignment { set; get; }

        protected IAveSecurableObject securableObject { set; get; }

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

            string type = obj.GetType().Name;
            switch (type)
            {
                case "AveSPSite":
                    instance = new AveSiteSecurity((AveSPSite)obj);
                    break;
                case "AveSPWeb":
                    instance = new AveWebSecurity((AveSPWeb)obj);
                    break;
                case "AveSPList":
                    instance = new AveListSecurity((AveSPList)obj);
                    break;
                case "AveSPItem":
                    instance = new AveItemSecurity((AveSPItem)obj);
                    break;
                default:
                    throw new Exception("Cannot construct a instance for this object type: " + obj.GetType().ToString());
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
                    bool copyRoleAssignments = restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.Merge && !IsNewCreatedObject;
                    log.Info($"RestoreInheritanceState, copyRoleAssignments:{copyRoleAssignments}.RemoveParentLimitedAccess:{WrapperConfiguration.RemoveParentLimitedAccess}.");
                    if (WrapperConfiguration.RemoveParentLimitedAccess)
                    {
                        //it will assign current user permissions to securableobject and its parent object when set copyroleassignments as false, so always set copyroleassignments as false,
                        securableObject.BreakRoleInheritance(true);
                    }
                    else
                    {
                        securableObject.BreakRoleInheritance(copyRoleAssignments);
                    }
                    if (!copyRoleAssignments)
                    {
                        log.Info($"RestoreInheritanceState should ClearRoleAssignments.");
                        ClearRoleAssignments();
                    }
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
                    var web = securableObject as IAveWeb;
                    if (web != null && web.IsRootWeb && restoreOption.PromotePermissionToRootWeb)
                    {
                        restorePermissonComplete = false;
                        return;
                    }

                    if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                    {
                        log.Info($"RestoreInheritanceState should ResetRoleInheritance.");
                        securableObject.ResetRoleInheritance();
                    }
                    restorePermissonComplete = true;        //源端继承时不再需要还原权限            
                }
            }
        }

        protected virtual void RestoreAnonymousPermSetting()
        {
        }

        protected virtual void ClearRoleAssignments()
        {
            for (int i = securableObject.RoleAssignments.Count - 1; i >= 0; i--)
            {
                var roleAssignment = securableObject.RoleAssignments[i];
                if (roleAssignment != null && roleAssignment.RoleDefinitionBindings != null && roleAssignment.RoleDefinitionBindings.Where(r => r.Type == AveRoleType.Guest).Count() > 0)
                {
                    log.Info($"ClearRoleAssignments has limited access permission and skip.PrincipalId:{roleAssignment.PrincipalId}.");
                }
                else
                {
                    securableObject.RoleAssignments.Remove(i);
                }
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRole"))
            {
#endif

            IAveWeb web = aveWeb.SPWeb;
            bool needDispose = false;
            try
            {
                if (aveWeb.WebInfo.HasUniqueRoleDefinitions && aveWeb.RestorePermissionLevel)
                {
                    if (!aveWeb.SPWeb.HasUniqueRoleDefinitions)
                    {
                        if (!aveWeb.SPWeb.HasUniqueRoleAssignments)
                        {
                            aveWeb.SPWeb.BreakRoleInheritance(false);
                        }
                        //aveWeb.SPWeb.RoleDefinitions.BreakInheritance(false, false);
                        web = aveWeb.SPWeb.FirstUniqueRoleDefinitionWeb;
                        needDispose = true;
                    }
                }
                else
                {
                    web = aveWeb.SPWeb.FirstUniqueRoleDefinitionWeb;
                    needDispose = true;
                }
                bool needUpdate = false;
                bool hasSamePermissions = false;
                IAveRoleDefinition role;
                try
                {
                    role = web.RoleDefinitions[roleInfo.Title];
                    needUpdate = ((long)role.BasePermissions != roleInfo.PermMask) || (!role.Description.Equals(roleInfo.Description)) || (role.Order != roleInfo.RoleOrder);
                    if (needUpdate)
                    {
                        hasSamePermissions = ((long)role.BasePermissions == roleInfo.PermMask);
                    }
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.GetRolePermissionError, ex.ToString());
                    if (roleInfo.Hidden && roleInfo.Title.StartsWith("System.", StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn("[SAAS-34996] Don't need to restore hidden permission level. WebUrl:{0}, RoleTitle:{1}", web.Url, roleInfo.Title);
                        return -1;
                    }
                    try
                    {
                        log.Info($"[SAAS-38616]Try to add role definition:{roleInfo.Title} to web.");
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
                log.Info($"[SAAS-38616]Update role:{roleInfo.Title} definition, needUpdate:{needUpdate}, hasSamePermissions:{hasSamePermissions}");
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
            }
#if PerformanceLog
            }
#endif
        }

        public IAveRoleDefinition GetRoleWithCache(int oldId, AveSPWeb aveWeb)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.GetRoleWithCache"))
            {
#endif
            object x;
            if (!aveWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache.TryGetValue(oldId, out x))
            {
                return null;
            }
            int newId;
            if (x is AveRoleInfo)
            {
                newId = RestoreRole((AveRoleInfo)x, aveWeb);
                aveWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache[oldId] = newId;
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
#if PerformanceLog
            }
#endif
        }

        public virtual void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {
        }

        protected virtual void RestoreRoleAssignment(AveRoleAssignmentInfo roleAssignmentInfo, AveSecurityParameters securityParam)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignment"))
            {
#endif
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
                    //mLog.Log(AveLogLevel.WARN, "WP10RTecurity250", roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId, e);
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.CannotFindRole,
                        roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId, e);
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
            if (spRoleDefinition.Type == AveRoleType.Guest || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleName.Contains(spRoleDefinition.Name) || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleId.Contains(spRoleDefinition.ID))
            {
                log.Info($"AveObjectSecurity RestoreRoleAssignment skip limited access.RoleType:{spRoleDefinition.Type}.RuleName:{spRoleDefinition.Name}.RoleID:{spRoleDefinition.ID}.");
                return;
            }
            if (count == 0)
            {
                IAveRoleAssignmentCollection roleAssginmens = securityParam.roleAssignments;
                spRoleAssignment.RoleDefinitionBindings.Add(spRoleDefinition);
                roleAssginmens.Add(spRoleAssignment);
            }
#if PerformanceLog
            }
#endif
        }

        protected virtual void RestoreRoleAssignment(int pricipalId, List<AveRoleAssignmentInfo> roleAssignmentInfos, AveSecurityParameters securityParam, SecurityRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignment"))
            {
#endif
            if (pricipalId == AveConstants.SYSTEM_ACCOUNT_ID)
            {
                return;
            }
            IAvePrincipal member = securityParam.aveSPWeb.ParentSite.SPMembers.FindMember(pricipalId, true);
            if (member == null)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTecurity313", pricipalId);
                log.Warn("Cannot find one user/group with principal id. PrincipalId:{0}", pricipalId);
                return;
            }
            IAveRoleAssignment spRoleAssignment = null;
            if (restoreOption.ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite && restoreOption.ConflictResolutionForSecurityObject != ConflictResolutionForSecurityObject.OverWrite)//OverWriteItemPermission为true时，已经把所有的RoleAssignment全部Remove掉了
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
                                log.Info($"RestoreRoleAssignment.RoleDefinitionBindings Remove:{roleDefinition.ID}.");
                                spRoleAssignment.RoleDefinitionBindings.Remove(roleDefinition);
                                changed = true;
                            }
                        }
                        if (changed)
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
                            //SAAS-38616 没找到目的端web role definition并且没抛异常, 也走反差逻辑
                            if (spRoleDefinition == null)
                            {
                                throw new AveNullResultException($"[SAAS-38616]An error occured when find the target web role definition by source role id:{info.RoleId} or name:{info.RoleName}");
                            }
                        }
                        catch (Exception e)
                        {//反插role，需要外围去load roleInfo
                            log.Info(WrapperRestoreResource.NeedRestoreRolesHere, e);
                            var roleInfo = securityParam.aveSPWeb.GetRoleByName(info.RoleName);
                            var newId = RestoreRole(roleInfo, securityParam.aveSPWeb);
                            if (roleInfo != null && newId > 0)
                            {
                                securityParam.aveSPWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache[securityParam.aveSPWeb.GetRoleByName(info.RoleName).RoleId] = newId;
                                spRoleDefinition = securityParam.aveSPWeb.SPWeb.RoleDefinitions[info.RoleName];
                            }
                            else
                            {
                                log.Info(WrapperRestoreResource.RestoreRolesWhenRestoreRoleAssignmentFailed, securableObject);
                                continue;
                            }
                        }
                    }
                    log.Info($"[SAAS-38616]list level role assignment used role:{info.RoleId}-{info.RoleName} definition:{spRoleDefinition != null}, spRoleAssignment.RoleDefinitionBindings count:{spRoleAssignment.RoleDefinitionBindings.Count}");
                    if (spRoleDefinition != null)
                    {
                        if (spRoleDefinition.Type == AveRoleType.Guest || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleName.Contains(spRoleDefinition.Name) || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleId.Contains(spRoleDefinition.ID))
                        {
                            log.Info($"AveObjectSecurity skip limited access.RoleType:{spRoleDefinition.Type}.RuleName:{spRoleDefinition.Name}.RoleID:{spRoleDefinition.ID}.");
                            continue;
                        }
                        int count = securityParam.roleAssignments.GetRoleAssignmentCount(securityParam.roleAssignments.ID, spRoleDefinition.ID, member.ID);

                        if (count == 0)
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
                    //mLog.Log(AveLogLevel.WARN, "WP10RTecurity355", info.RoleId, e);
                    log.Warn("An error occurred while restore roleAssignmentsInfo. info.RoleId:{0}\n error message:{1} ", info.RoleId, e);
                }
            }
            log.Info($"[SAAS-38616]roleAssignmentBindingCol count:{roleAssignmentBindingCol.Count}");
            if (roleAssignmentBindingCol.Count > 0)
            {
                try
                {
                    securityParam.roleAssignments.Add(spRoleAssignment);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                    //report.AddDetail(new AveWrapperReportDto("RestoreRoleAssignments", "RestoreRoleAssignments", AveReportObjectType.ListRoleAssignments, AveStatus.Skipped, "you don't have permission to restore list roleassignments. " + ex.Message));
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while restore roleAssignmentsInfo.\n Error message:{0} ", e);
                    throw;
                }
            }

#if PerformanceLog
            }
#endif
        }
        public Dictionary<int, List<AveRoleAssignmentInfo>> GroupRoleAssignmentInfos(List<AveRoleAssignmentInfo> roleAssignmentInfos)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.ProcessItemByWeb"))
            {
#endif
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
#if PerformanceLog
            }
#endif
        }

        public virtual void Restore(AveMemberInfoCollection memeberInfoCol, SecurityRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.Restore"))
            {
#endif
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
                                principal = mWeb.EnsureUser(memberInfo.Name);
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
                            if (roleType == AveRoleType.Guest)
                            {
                                log.Info($"AveObjectSecurity skip limited access.PrincipalId:{roleAssignment.PrincipalId}.");
                                continue;
                            }
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
#if PerformanceLog
            }
#endif
        }


        public virtual void RestoreRoles(List<AveRoleInfo> roleInfos)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoles"))
            {
#endif
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
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveObjectSecurity.RestoreRoleAssignments"))
            {
#endif

            Dictionary<int, List<AveRoleAssignmentInfo>> groupRoleAssignmentInfo = GroupRoleAssignmentInfos(roleAssignmentInfos);

            Dictionary<int, IAveRoleAssignment> cacheUserAndGroup = new Dictionary<int, IAveRoleAssignment>();

            foreach (IAveRoleAssignment roleAssignment in spRoleAssignColl)
            {
                int memberId = roleAssignment.Member.ID;
                if (memberId != AveConstants.SYSTEM_ACCOUNT_ID || (!cacheUserAndGroup.ContainsKey(memberId)))
                {
                    cacheUserAndGroup[memberId] = roleAssignment;
                }
            }

            foreach (KeyValuePair<int, List<AveRoleAssignmentInfo>> keyValue in groupRoleAssignmentInfo)
            {
                try
                {
                    if (keyValue.Key == AveConstants.SYSTEM_ACCOUNT_ID)
                    {
                        log.Info($"RestoreRoleAssignments.Current account is SYSTEM_ACCOUNT_ID.");
                        continue;
                    }
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
                            RemoveRoleDefinationsExceptLimitedAccess(spRoleAssignment);
                            if (!spRoleAssignment.RoleDefinitionBindings.Any(roleDefinition => IsLimitAccessRole(roleDefinition)))
                            {
                                // SAAS-3921, if all the Definitions are removed from the roleAssignment, the roleAssignment will be
                                // removed and the update operation will fail, so we need to add a new roleAssignment
                                log.Info($"RestoreRoleAssignments.cacheUserAndGroup.Contains member:{member.ID}.Need to add.");
                                needToAdd = true;
                            }
                        }
                    }
                    else
                    {
                        log.Info($"RestoreRoleAssignments.Need to add member:{member.ID}.");
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
                                    //SAAS-38616 没找到目的端web role definition并且没抛异常, 也走反差逻辑
                                    if (spRoleDefinition == null)
                                    {
                                        throw new AveNullResultException($"[SAAS-38616]An error occured when find the target web role definition by source role id:{info.RoleId} or name:{info.RoleName}");
                                    }
                                }
                                catch (Exception e)
                                {//反插role，需要外围去load roleInfo
                                    log.Info(WrapperRestoreResource.NeedRestoreRolesHere, e);
                                    var roleInfo = aveSPWeb.GetRoleByName(info.RoleName);
                                    var newId = RestoreRole(roleInfo, aveSPWeb);
                                    if (roleInfo != null && newId > 0)
                                    {
                                        aveSPWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache[aveSPWeb.GetRoleByName(info.RoleName).RoleId] = newId;
                                        spRoleDefinition = aveSPWeb.SPWeb.RoleDefinitions[info.RoleName];
                                    }
                                    else
                                    {
                                        log.Info(WrapperRestoreResource.RestoreRolesWhenRestoreRoleAssignmentFailed, securableObject);
                                        continue;
                                    }
                                }
                            }
                            log.Info($"[SAAS-38616]list level role assignment used role:{info.RoleId}-{info.RoleName} definition:{spRoleDefinition != null}, spRoleAssignment.RoleDefinitionBindings count:{spRoleAssignment.RoleDefinitionBindings.Count}");
                            if (spRoleDefinition != null)
                            {
                                if (spRoleDefinition.Type == AveRoleType.Guest || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleName.Contains(spRoleDefinition.Name) || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleId.Contains(spRoleDefinition.ID))
                                {
                                    log.Info($"AveObjectSecurity skip limited access.RoleType:{spRoleDefinition.Type}.RuleName:{spRoleDefinition.Name}.RoleID:{spRoleDefinition.ID}.");
                                    continue;
                                }
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
                    log.Info($"[SAAS-38616]spRoleAssignment.RoleDefinitionBindings count:{spRoleAssignment.RoleDefinitionBindings.Count}");
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
                    throw;
                }
            }

            if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
            {
                foreach (KeyValuePair<int, IAveRoleAssignment> keyValue in cacheUserAndGroup)
                {
                    if (keyValue.Value.RoleDefinitionBindings.All(roleDefinition => !IsLimitAccessRole(roleDefinition)))
                    {
                        log.Info($"RestoreRoleAssignments.ConflictResolutionForSecurityObject.OverWrite.RemoveById:{keyValue.Key}.");
                        spRoleAssignColl.RemoveById(keyValue.Key);
                    }
                    else
                    {
                        RemoveRoleDefinationsExceptLimitedAccess(keyValue.Value);
                    }
                }
            }
#if PerformanceLog
            }
#endif
        }

        private static void RemoveRoleDefinationsExceptLimitedAccess(IAveRoleAssignment spRoleAssignment)
        {
            for (int i = spRoleAssignment.RoleDefinitionBindings.Count - 1; i >= 0; --i)
            {
                var roleDefinition = spRoleAssignment.RoleDefinitionBindings[i];
                if (IsLimitAccessRole(roleDefinition))
                {
                    continue;
                }
                log.Info($"RemoveRoleDefinationsExceptLimitedAccess.RemoveRoleDefinitionID:{roleDefinition.ID}.RoleDefinitionName:{roleDefinition.Name}.");
                spRoleAssignment.RoleDefinitionBindings.Remove(roleDefinition);
            }
            spRoleAssignment.Update();

        }

        public class AveSecurityParameters
        {
            public string scopeString;
            public AveSPWeb aveSPWeb;
            public IAveRoleAssignmentCollection roleAssignments;
        }
        protected static bool IsLimitAccessRole(IAveRoleDefinition roleDefinition)
        {
            bool isLimitAccessRole = false;
            isLimitAccessRole = roleDefinition.Type == AveRoleType.Guest || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleName.Contains(roleDefinition.Name) || WrapperConfiguration.WrapperConfigurationForBPOS.SkipRoleId.Contains(roleDefinition.ID);
            if (isLimitAccessRole)
            {
                log.Info($"IsLimitAccessRole.RoleType:{roleDefinition.Type}.RuleName:{roleDefinition.Name}.RoleID:{roleDefinition.ID}.");
            }
            return isLimitAccessRole;
        }

        public void Dispose()
        {
            report?.Dispose();
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSiteSecurity.Restore"))
            {
#endif
            try
            {
                RestoreMemberAndMemberShip(securityInfo, restoreOption);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTecurity425", (mAveParentSite == null || mAveParentSite.SPSite == null) ? "" : mAveParentSite.SPSite.Url, e);
                log.Warn("An error occurred while restoring site members and membership. SiteUrl:{0}, error:{1}",
                    (ParentSite == null || ParentSite.SPSite == null) ? "" : ParentSite.SPSite.Url, e.ToString());
            }
#if PerformanceLog
            }
#endif
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
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWebSecurity.Restore"))
            {
#endif
            try
            {
                RestoreMemberAndMemberShip(securityInfo, restoreOption);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTecurity481", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e);
                log.Warn("An error occurred while restoring web members and membership. WebUrl:{0}, error:{1}",
                    mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
            }

            try
            {
                RestoreRoles(securityInfo.Roles, restoreOption);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTecurity493", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e);
                log.Warn("An error occurred while restoring web roles. WebUrl:{0}, error:{1}",
                    mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
            }
            try
            {
                RestoreRoleAssignments(securityInfo.RoleAssignments, restoreOption);
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10RTecurity501", mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e);
                log.Warn("An error occurred while restoring web role assignments. WebUrl:{0}, error:{1}",
                    mAveSPWeb == null || mAveSPWeb.SPWeb == null ? "" : mAveSPWeb.SPWeb.Url, e.ToString());
            }
#if PerformanceLog
            }
#endif
        }

        public override void RestoreRoles(List<AveRoleInfo> roleInfos, SecurityRestoreOption restoreOption)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveWebSecurity.WebRoles"))
            {
#endif
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
                        roleInfo.Title = mAveSPWeb.ParentSite.GetNameByLanguageMapping(roleInfo.Title, AveLanguageMappingType.PermissionMapping);
                        log.Info($"[SAAS-38616]Start to restore role definition:{roleInfo.RoleId}-{roleInfo.Title}, need restore:{restoreOption.NeedRestore}");
                        if (restoreOption.NeedRestore)
                        {
                            int newId = RestoreRole(roleInfo, mAveSPWeb);
                            if (newId > 0)
                            {
                                mAveSPWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache[roleInfo.RoleId] = newId;
                            }
                            log.Info($"[SAAS-38616]End to restore role definition:{roleInfo.RoleId}-{roleInfo.Title}, newId:{newId}.");
                        }
                        else
                        {
                            mAveSPWeb.ParentSite.MappingManager.WebMappingManager.RoleDefinitionsCache[roleInfo.RoleId] = roleInfo;
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
                report.AddDetail(new AveWrapperReportDto(mAveSPWeb.SPWeb.Name, mAveSPWeb.SPWeb.Name, AveReportObjectType.WebRoles, AveStatus.Skipped, "You don't have permission to restore Roles" + ex.Message));
            }
#if PerformanceLog
            }
#endif
        }

        protected override void RestoreMemberAndMemberShip(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {
            mAveSPWeb.needListRestore = !restoreOption.NeedRestore;
            if (restoreOption.NeedRestore)
            {
                mAveSPWeb.ParentSite.SPMembers.RestoreMembers(securityInfo);
            }
        }

        public override void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {
            log.Info($"[SAAS-38616]Web level restore role assignments, need return:{roleAssignmentInfos == null || !restoreOption.NeedRestore}.ConflictResolutionForSecurityObject:{restoreOption.ConflictResolutionForSecurityObject}.ConflictResolutionForPincipal:{restoreOption.ConflictResolutionForPincipal}.");
#if PerformaceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebRoleAssignments"))
            {
#endif
            try
            {
                if (roleAssignmentInfos == null || !restoreOption.NeedRestore)
                {
                    log.Info($"Web level restore role assignments, no need restore or RoleAssignment is null.");
                    return;
                }
                log.Info($"[SAAS-38616]Web level restore role assignments, RoleAssignment Count:{roleAssignmentInfos.Count}.");


                bool restoreComplete = false;

                RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restoreComplete);

                if (restoreComplete)
                {
                    log.Info($"Web level restore role assignments, restore complete.");
                    return;
                }

                ClearDefaultRoleAssignment();

                if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                {
                    try
                    {
                        log.Info($"Web level restore role assignments,ConflictResolutionForSecurityObject OverWrite RestoreRoleAssignments.");
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
                    log.Info($"Web level restore role assignments,principalId:{principalId}.");
                    securityParam.roleAssignments = mAveSPWeb.SPWeb.RoleAssignments;
                    RestoreRoleAssignment(principalId, groupRoleAssignmentInfo[principalId], securityParam, restoreOption);
                }
            }
            catch (Exception e)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.Common_Wrapper, new EventIds.SharePoint.RestorePermissionFailedEventMessage(e));
                //log.Warn("An error occurred while restore the web roleassignments. ", ex);
                report.AddDetail(new AveWrapperReportDto("WebRoleAssignments", "WebRoleAssignments", AveReportObjectType.RoleAssignments, AveStatus.Skipped, "You don't have permission to Restore WebRoleAssignments. " + e.Message));
            }
#if PerformaceLog
            }
#endif
        }

        private void ClearDefaultRoleAssignment()
        {
            try
            {
                if (this.mAveSPWeb.SPWeb.IsRootWeb && this.mAveSPWeb.ParentSite.IsNewCreated)
                {
                    IAveRoleAssignmentCollection webRoleAssignments = mAveSPWeb.SPWeb.RoleAssignments;
                    log.Info($"Web level begin ClearDefaultRoleAssignment.Count:{webRoleAssignments.Count}.");
                    for (int i = webRoleAssignments.Count - 1; i >= 0; i--)
                    {
                        log.Info($"Web level ClearDefaultRoleAssignment.RoleAssignmentID:{webRoleAssignments[i].Member.ID}.RoleAssignmentName:{webRoleAssignments[i].Member.Name}.RoleAssignmentLoginName:{webRoleAssignments[i].Member.LoginName}.");
                        //不处理system account的permission，既不移除也不添加。
                        if ((webRoleAssignments[i].Member is IAveUser) && ((webRoleAssignments[i].Member as IAveUser).ID == AveConstants.SYSTEM_ACCOUNT_ID))
                        {
                            continue;
                        }
                        if (webRoleAssignments[i] != null && webRoleAssignments[i].RoleDefinitionBindings != null && webRoleAssignments[i].RoleDefinitionBindings.Where(r => r.Type == AveRoleType.Guest).Count() > 0)
                        {
                            log.Info($"ClearDefaultRoleAssignment has limited access permission and skip.PrincipalId:{webRoleAssignments[i].PrincipalId}.");
                            continue;
                        }
                        webRoleAssignments.Remove(i);
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
            log.Info($"[SAAS-38616]List level restore role assignments, need return:{roleAssignmentInfos == null || !restoreOption.NeedRestore}.ConflictResolutionForSecurityObject:{restoreOption.ConflictResolutionForSecurityObject}.ConflictResolutionForPincipal:{restoreOption.ConflictResolutionForPincipal}.");
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveListSecurity.RestoreRoleAssignments"))
            {
#endif
            try
            {
                if (roleAssignmentInfos == null || !restoreOption.NeedRestore)
                {
                    log.Info($"List level restore role assignments, no need restore or RoleAssignment is null.");
                    return;
                }
                log.Info($"[SAAS-38616]List level restore role assignments.RoleAssignment Count:{roleAssignmentInfos.Count}.");
                bool restorePermissionComplete = false;

                RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment, out restorePermissionComplete);

                if (restorePermissionComplete)
                {
                    log.Info($"List level restore role assignments, restore complete.");
                    return;
                }

                if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                {
                    try
                    {
                        log.Info($"List level restore role assignments,ConflictResolutionForSecurityObject OverWrite RestoreRoleAssignments.");
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
                        log.Info($"List level restore role assignments,RestoreRoleAssignment.principalId:{principalId}.");
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
                report.AddDetail(new AveWrapperReportDto("ListRoleAssignment", "ListRoleAssignment", AveReportObjectType.RoleAssignment, AveStatus.Skipped, "You don't have permission to Restore ListRoleAssignments. " + ex.Message));
            }
#if PerformanceLog
            }
#endif
        }

        protected override void RestoreAnonymousPermSetting()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveListSecurity.ReSignRoleAssignment"))
            {
#endif
            if (mAveSPList.ListSettingInfo != null && mAveSPList.ListSettingInfo.AnonymousPermMask64 != null && mAveSPList.ListSettingInfo.AnonymousPermMask64.IsAvailable && mAveSPList.SPList.AnonymousPermMask64 != (AveBasePermissions)mAveSPList.ListSettingInfo.AnonymousPermMask64.Value)
            {
                mAveSPList.SPList.AnonymousPermMask64 = (AveBasePermissions)mAveSPList.ListSettingInfo.AnonymousPermMask64.Value;
            }
#if PerformanceLog
            }
#endif

        }
    }

    public class AveItemSecurity : AveObjectSecurity
    {
        private AveSPItem mAveSPItem;

        public AveItemSecurity(AveSPItem aveItem)
            : base(aveItem.SPListItem, aveItem.ParentFolder.ParentList.ParentWeb.ParentSite, aveItem.ParentFolder.ParentList.ParentWeb.SPWeb)
        {
            mAveSPItem = aveItem;
            IsNewCreatedObject = aveItem.IsNewCreate;
        }

        public override void Restore(AveSecurityInfo securityInfo, SecurityRestoreOption restoreOption)
        {

#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveItemSecurity.Restore"))
            {
#endif
            RestoreRoleAssignments(securityInfo.RoleAssignments, restoreOption);
#if PerformanceLog
            }
#endif
        }

        public override void RestoreRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption)
        {
            log.Info($"[SAAS-38616]Item level restore role assignments, need return:{!restoreOption.NeedRestore || mAveSPItem.SPListItem == null || roleAssignmentInfos == null}.ConflictResolutionForSecurityObject:{restoreOption.ConflictResolutionForSecurityObject}.ConflictResolutionForPincipal:{restoreOption.ConflictResolutionForPincipal}.");
            if (!restoreOption.NeedRestore || mAveSPItem.SPListItem == null || roleAssignmentInfos == null)
            {
                log.Info($"Item level restore role assignments, no need restore or RoleAssignment is null.");
                return;
            }
            log.Info($"[SAAS-38616]Item level restore role assignments.RoleAssignment Count:{roleAssignmentInfos.Count}.");
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveItemSecurity.RestoreRoleAssignments"))
            {
#endif
            bool restoreComplete;
            try
            {
                RestoreInheritanceState(restoreOption, SourceHasUniqueRoleAssignment || roleAssignmentInfos.Count > 0, out restoreComplete);
                if (restoreComplete)
                {
                    log.Info($"Item level restore role assignments, restore complete.");
                    return;
                }

                if (restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite)
                {
                    try
                    {
                        log.Info($"Item level restore role assignments,ConflictResolutionForSecurityObject OverWrite RestoreRoleAssignments.");
                        RestoreRoleAssignments(roleAssignmentInfos, mAveSPItem.SPListItem.RoleAssignments, mAveSPItem.ParentFolder.ParentList.ParentWeb, restoreOption);
                    }
                    catch (AveSecurityTrimingException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while remove roleAssignments. ItemUrl:{0}, error:{1}.", mAveSPItem.SPListItem.Url, e.ToString());
                        throw;
                    }
                    return;
                }
                RestoreRoleAssignmentsInternal(roleAssignmentInfos, restoreOption, restoreOption.ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite);
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn("An error occurred while restoring item role assignment. ", ex);
                report.AddDetail(new AveWrapperReportDto("ItemRoleAssignments", "ItemRoleAssignments", AveReportObjectType.RoleAssignments, AveStatus.Skipped, "You don't have permission to Restore ItemRoleAssignments. " + ex.Message));
            }

#if PerformanceLog
            }
#endif

        }

        private void RestoreRoleAssignmentsInternal(List<AveRoleAssignmentInfo> roleAssignmentInfos, SecurityRestoreOption restoreOption, bool RestoreShareLinkOnly = false)
        {
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
                    //1.Sharing link groups don't need be migrated, just add to UserAndDomainMapping cache.
                    //2.Simple group will follow the normal logic.
                    var tempInfo = securityParam.aveSPWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(principalId);
                    var groupInfo = tempInfo as AveGroupInfo;
                    var userInfo = tempInfo as AveUserInfo;
                    var memberInfo = tempInfo as AveSPMemberInfo;
                    log.Info($"Try to restore role assignments with group:{groupInfo?.Title};{principalId}, RestoreShareLinkOnly:{RestoreShareLinkOnly}, Has backed up sharing link info:{groupInfo?.IsVerifiedSharelinkGroup}");
                    if (groupInfo == null && userInfo == null && memberInfo == null)
                    {
                        log.Error("group info is null");
                        throw new ArgumentNullException("group info is null");
                    }
                    if (userInfo==null && memberInfo == null && NeedToRestoreSharingLink(groupInfo, restoreOption))
                    {
                        if (groupInfo.IsVerifiedSharelinkGroup)
                        {
                            //Use new logic to migrate sharinglinks by AveSharingLinkInfo
                            RestoreSharingLink(securityParam, new List<AveGroupInfo>() { groupInfo }, securityParam.aveSPWeb.ServerRelativeUrl, mAveSPItem.ParentList.SPList.ID, mAveSPItem.RowId);
                        }
                        else
                        {
                            #region In order to be compatible with old backup data
                            int linkKind = (int)GetLinkKind(groupInfo.Title);
                            foreach (var member in groupInfo.Members)
                            {
                                securityParam.aveSPWeb.ParentSite.SPMembers.RestoreUser(member, false, true, false, false);
                                var userPrincipal = securityParam.aveSPWeb.ParentSite.SPMembers.FindMember(member.ID, false);
                                if (userPrincipal != null && restoreOption.IsIncludeShareLink)
                                {
                                    log.Info($"Share object with link. link type:{linkKind}, login:{userPrincipal.LoginName}, web url:{securityParam.aveSPWeb.ServerRelativeUrl}, list: {mAveSPItem.ParentList.SPList.ID}, item: {mAveSPItem.RowId}.");
                                    if (linkKind == (int)AveSharingLinkKind.Flexible)
                                    {
                                        int roleId = groupRoleAssignmentInfo[principalId].First().RoleId;
                                        string roleValue = "role:" + roleId;
                                        securityParam.roleAssignments.ShareObjectExternal(linkKind, userPrincipal.LoginName, member.DomainGroup, securityParam.aveSPWeb.ServerRelativeUrl, mAveSPItem.ParentList.SPList.ID, mAveSPItem.RowId, roleValue);
                                    }
                                    else
                                    {
                                        securityParam.roleAssignments.ShareLink(linkKind, userPrincipal.LoginName, member.DomainGroup, securityParam.aveSPWeb.ServerRelativeUrl, mAveSPItem.ParentList.SPList.ID, mAveSPItem.RowId);
                                    }
                                }
                                else
                                {
                                    log.Warn("An error occurred while restoring list item sharelink.This may casuse by user {0} doesn't exist, or IsIncludeShareLink is false.", member.Login);
                                }
                            }
                            #endregion
                        }
                    }
                    else if (!RestoreShareLinkOnly)
                    {
                        RestoreRoleAssignment(principalId, groupRoleAssignmentInfo[principalId], securityParam, restoreOption);
                    }
                }
                catch (ArgumentNullException ane)
                {
                    log.Warn($"RestoreRoleAssignmentsInternal failed.principalId:{principalId}.message:{ane.Message}.");
                    //add security report in future.
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.WARN, "WP10RTecurity741", mAveSPItem.SPListItem.ID, mAveSPItem.SPListItem.Title, e);
                    log.Warn("An error occurred while restoring list item role assignment. ListItem Id:{0}, ListItem Title:{1}, error:{2}",
                        mAveSPItem.SPListItem.ID, mAveSPItem.SPListItem.Title, e.ToString());
                    throw;
                }
            }
        }

        private static bool NeedToRestoreSharingLink(AveGroupInfo groupInfo, SecurityRestoreOption restoreOption)
        {
            return groupInfo != null && !string.IsNullOrWhiteSpace(groupInfo.Title) && AveSPUtility.MatchShareLink.IsMatch(groupInfo.Title) && restoreOption != null && restoreOption.IsIncludeShareLink;
        }

        public void RestoreSharingLink(AveSecurityParameters securityParam, List<AveGroupInfo> ShareLinks, string parentWebUrl, Guid listId, int itemId)
        {
            foreach (var groupinfo in ShareLinks)
            {
                try
                {
                    int linkKind = groupinfo.ShareLink.LinkKind;

                    List<IAvePrincipal> linkMembers = new List<IAvePrincipal>();

                    foreach (var memberId in groupinfo.Memberships)
                    {
                        var member = securityParam.aveSPWeb.ParentSite.SPMembers.UserAndDomainMapping.GetUserMapping(memberId) as AveUserInfo;
                        if (member != null)
                        {
                            securityParam.aveSPWeb.ParentSite.SPMembers.RestoreUser(member, false, true, false, false);
                        }
                        var userPrincipal = securityParam.aveSPWeb.ParentSite.SPMembers.FindMember(memberId, false);
                        if (userPrincipal != null)
                        {
                            linkMembers.Add(userPrincipal);
                        }
                    }
                    var membersInfo = new System.Text.StringBuilder();
                    linkMembers.ForEach(member => membersInfo.Append($"{member.LoginName},{member.ID};"));
                    if (groupinfo.ShareLink.RequiresPassword)
                    {
                        log.Warn($"Do not support to share link that requires password. Web:{parentWebUrl}, list:{listId}, item:{itemId}, share link:{groupinfo.ShareLink}");
                    }
                    else
                    {
                        log.Info($"Share link to {linkMembers.Count} user: {membersInfo} for web:{parentWebUrl}, list:{listId}, item:{itemId} on group:{groupinfo.Title}");
                        securityParam.roleAssignments.RestoreSharingLink(parentWebUrl, listId, itemId, linkMembers, groupinfo.ShareLink);
                    }
                }
                catch (Exception sharedlinkException)
                {
                    log.Error($"Can not cache shared link group. exception: {sharedlinkException}");
                }
            }
        }

        private AveSharingLinkKind GetLinkKind(string sharingLinkGroupTitle)
        {
            string[] breaks = sharingLinkGroupTitle.Split(new char[] { '.' });
            if (breaks.Length != 4)
            {
                return default(AveSharingLinkKind);
            }

            //string kind = breaks[2]; //[RECO-20916]

            AveSharingLinkKind result = default(AveSharingLinkKind);
            if (Enum.TryParse(breaks[2], out result))
            {
            }
            return result;
        }
    }

    public class SecurityRestoreOption
    {
        private bool mIsIncludeShreLink = false;
        public bool NeedRestore = true;
        [Obsolete("use ConflictResolutionForPincipal instead ")]
        public bool OverWritePermission //对某个user的permission的控制
        {
            set
            {
                ConflictResolutionForPincipal = value ? ConflictResolutionForPincipal.OverWrite : ConflictResolutionForPincipal.Merge;
            }
            get
            {
                return ConflictResolutionForPincipal == ConflictResolutionForPincipal.OverWrite;
            }
        }
        [Obsolete("use ConflictResolutionForSecurityObject instead")]
        public bool OverWriteItemPermission //对web、list、item级别的所有permission的控制
        {
            set
            {
                ConflictResolutionForSecurityObject = value ? ConflictResolutionForSecurityObject.OverWrite : ConflictResolutionForSecurityObject.Merge;
            }
            get
            {
                return ConflictResolutionForSecurityObject == ConflictResolutionForSecurityObject.OverWrite;
            }
        }

        public ConflictResolutionForSecurityObject ConflictResolutionForSecurityObject { set; get; }
        public ConflictResolutionForPincipal ConflictResolutionForPincipal { set; get; }
        public bool PromotePermissionToRootWeb { set; get; }
        public bool IsIncludeShareLink
        {
            set { mIsIncludeShreLink = value; }
            get { return mIsIncludeShreLink; }
        }
    }

    public enum ConflictResolutionForSecurityObject
    {
        Merge = 0,
        OverWrite
        //MergefromInherited
    }

    public enum ConflictResolutionForPincipal
    {
        Merge = 0,
        OverWrite
    }

}
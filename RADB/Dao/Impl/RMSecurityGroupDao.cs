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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSecurityGroupDao : BaseDao<RMSecurityGroup>, IRMSecurityGroupDao
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMSecurityGroupDao));
        public IRoleDao RoleDao { get; set; }
        public IRMSecurityGroupMembershipDao RMSecurityGroupMembershipDao { get; set; }
        public IRMLocationDao RMLocationDao { get; set; }
        public IPhysicalRecordSettingDao PhysicalRecordSettingDao { get; set; }
        public IRMSecurityTrimmingHelper RMSecurityTrimmingHelper { get; set; }
        public IRMRuleDao RMRuleDao { get; set; }
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        public RMSecurityGroup CreateSecurityGroup(SecurityGroupDto dto)
        {

            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    if (context.RMSecurityGroup.Where(g => g.Name.Equals(dto.Name) && g.IsRemoved == false).FirstOrDefault() != null)
                    {
                        throw new Exception("Group name already exists");
                    }
                    //Create role in role table. roleId 
                    string roleName = dto.Name + Guid.NewGuid();
                    var role = context.Role.Add(new RMRole()
                    {
                        RoleName = roleName,
                        IsRemoved = false,
                        IsSystemAdmin = false,
                        Modified = DateTime.UtcNow,
                        PermissionMasks = dto.PermissionMasks,
                        ReportingPermission = dto.ReportingPermission,
                        SubPermission1 = dto.SubPermission1Masks,
                        PermissionExtensionMasks = dto.PermissionExtensionMasks,
                        SOPermissionMasks = dto.SOPermissionMasks,
                        RoleType = Contract.RoleAssignments.RMRoleType.DeligatedAdmin,
                        UpgradeType = RMRoleUpgradeType.UpgradePhysicalAction,
                        IsNewGroup = true
                    });
                    context.SaveChanges();
                    var group = context.RMSecurityGroup.Add(new RMSecurityGroup()
                    {
                        Name = dto.Name,
                        Description = dto.Description,
                        IsEnableTrim = dto.IsEnableTrim,
                        IsRemoved = false,
                        RoleId = role.RoleId,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        RuleNodeString = dto.RuleTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.RuleTreeNodeInfo) : null,
                    });
                    if (dto.HasOpusILLicense)
                    {
                        group.NodeString = dto.TermTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.TermTreeNodeInfo) : null;
                    }
                    context.SaveChanges();
                    CheckDataSourceIfExists(role.RoleId, context, dto.DataSourceScopeInfo);
                    List<RMSecurityGroupMembership> memberships = new List<RMSecurityGroupMembership>();
                    foreach (var user in dto.Users)
                    {
                        memberships.Add(new RMSecurityGroupMembership()
                        {
                            GroupId = group.Id,
                            UserId = user.UserId
                        }
                        );
                    }
                    context.RMSecurityGroupMembership.AddRange(memberships);
                    context.SaveChanges();
                    CreateOrUpdateScopePermission(context, group.Id, dto.DataSourceScopeInfo);
                    CreateOrUpdateSecurityTermMapping(context, group.Id, dto.SelectedTermObjs, dto.HasOpusILLicense);
                    CreateOrUpdateSecurityRuleMapping(context, group.Id, dto.SelectedRuleObjs);
                    tran.Commit();
                    return group;
                }
            }

        }
        /// <summary>
        /// In July 2021 ,remove role when group is removed
        /// </summary>
        /// <param name="groupId"></param>
        public void DeleteSecurityGroup(int groupId)
        {
            using (var context = GetNewContext())
            {
                using (var tran = context.Database.BeginTransaction())
                {
                    var group = context.RMSecurityGroup.Where(g => g.Id == groupId && g.IsRemoved == false).FirstOrDefault();
                    if (group == null)
                    {
                        throw new Exception("Group not exist");
                    }
                    group.IsRemoved = true;
                    #region remove roles
                    var role = context.Role.Where(r => r.RoleId == group.RoleId).FirstOrDefault();
                    role.IsRemoved = true;
                    var entry = context.Entry(role);
                    if (entry.State == EntityState.Modified)
                    {
                        context.SaveChanges();
                    }
                    else if (entry.State == EntityState.Detached)
                    {
                        context.DetachLocalObject<RMRole>(role);
                        context.Set<RMRole>().Attach(role);
                        entry.State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    #endregion
                    #region remove group containers ///remove it from Database for query fast??
                    var containers = context.RMScopeRoleAssignment.Where(t => t.GroupId == groupId);
                    context.RMScopeRoleAssignment.RemoveRange(containers);
                    context.SaveChanges();
                    #endregion 
                    var gentry = context.Entry(group);
                    if (gentry.State == EntityState.Modified)
                    {
                        context.SaveChanges();
                    }
                    else if (gentry.State == EntityState.Detached)
                    {
                        context.DetachLocalObject<RMSecurityGroup>(group);
                        context.Set<RMSecurityGroup>().Attach(group);
                        entry.State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    var userMembers = context.RMSecurityGroupMembership.Where(u => u.GroupId.Equals(groupId));
                    context.RMSecurityGroupMembership.RemoveRange(userMembers);
                    context.SaveChanges();
                    tran.Commit();
                }
            }

        }

        public async Task<RMSecurityGroup> EditSecurityGroupAsync(SecurityGroupDto dto)
        {
            using (var context = GetNewContext())
            {
                var name = dto.Name;
                var groupId = dto.Id;
                var description = dto.Description;
                var permissionMasks = dto.PermissionMasks;
                var reportPermissionMasks = dto.ReportingPermission;
                if (context.RMSecurityGroup.Where(g => g.Name.Equals(name) && g.Id != groupId && g.IsRemoved == false).FirstOrDefault() != null)
                {
                    throw new Exception("Group name already exists");
                }
                var group = context.RMSecurityGroup.Where(g => g.Id.Equals(groupId) && g.IsRemoved == false).FirstOrDefault();
                if (group == null)
                {
                    throw new Exception("Group not exist");
                }
                CheckDataSourceIfExists(group.RoleId, context, dto.DataSourceScopeInfo);
                group.Name = name;
                group.Description = description;
                group.IsEnableTrim = dto.IsEnableTrim;
                group.ModifiedTime = DateTime.UtcNow.Ticks;
                if (dto.HasOpusILLicense)
                { 
                    group.NodeString = dto.TermTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.TermTreeNodeInfo) : null;
                }
                group.RuleNodeString = dto.RuleTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.RuleTreeNodeInfo) : null;
                await UpdateAsync(group);
                context.SaveChanges();
                //Update Role
                await RoleDao.UpdateRoleAsync(group.RoleId, permissionMasks, dto.SubPermission1Masks, dto.PermissionExtensionMasks, dto.SOPermissionMasks,dto.ReportingPermission);

                //Update Scope Containers
                CreateOrUpdateScopePermission(context, group.Id, dto.DataSourceScopeInfo);
                //update term permission
                CreateOrUpdateSecurityTermMapping(context, group.Id, dto.SelectedTermObjs, dto.HasOpusILLicense);
                CreateOrUpdateSecurityRuleMapping(context, group.Id, dto.SelectedRuleObjs);
                //Update Usermapping TO DO
                var userIds = new List<string>();
                if (dto.Users != null && dto.Users.Count > 0)
                {
                    userIds = dto.Users.Select(o => o.UserId).ToList();
                }
                RMSecurityGroupMembershipDao.CreateOrUpdateGroupMemberShips(groupId, userIds);
                return group;
            }
        }

        public async Task UpdateSecurityGroupPermissionAsync(int groupId, long permissionMasks, long subPermissionMasks, long permissionExtensionMasks, long soPermissionMasks,long reportPermissionMasks)
        {
            using (var context = GetNewContext())
            {
                var group = context.RMSecurityGroup.Where(g => g.Id.Equals(groupId) && g.IsRemoved == false).FirstOrDefault();
                if (group == null)
                {
                    throw new Exception("Group not exist");
                }
                await RoleDao.UpdateRoleAsync(group.RoleId, permissionMasks, subPermissionMasks, permissionExtensionMasks, soPermissionMasks,reportPermissionMasks);
            }
        }
        public async Task<RMSecurityGroup> EditBuiltInEndUserGroupAsync(SecurityGroupDto dto)
        {
            using (var context = GetNewContext())
            {
                var groupId = dto.Id;
                var group = await context.RMSecurityGroup.Where(g => g.Id.Equals(dto.Id) && g.IsRemoved == false).FirstOrDefaultAsync();
                if (group == null)
                {
                    throw new Exception("Group not exist");
                }
                group.IsEnableTrim = dto.IsEnableTrim;
                group.NodeString = dto.TermTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.TermTreeNodeInfo) : null;
                group.RuleNodeString = dto.RuleTreeNodeInfo != null ? JsonConvert.SerializeObject(dto.RuleTreeNodeInfo) : null;
                await UpdateAsync(group);

                context.SaveChanges();
                //Update Role
                RoleDao.UpdateRoleSubPermission(group.RoleId, dto.SubPermission1Masks);

                //update term permission
                CreateOrUpdateSecurityTermMapping(context, group.Id, dto.SelectedTermObjs, dto.HasOpusILLicense);
                CreateOrUpdateSecurityRuleMapping(context, group.Id, dto.SelectedRuleObjs);
                //update users
                var userIds = new List<string>();
                if (dto.Users != null && dto.Users.Count > 0)
                {
                    userIds = dto.Users.Select(o => o.UserId).ToList();
                }
                RMSecurityGroupMembershipDao.CreateOrUpdateGroupMemberShips(groupId, userIds);
                return group;
            }
        }

        public RMSecurityGroup EditBuiltInReviewUserGroup(SecurityGroupDto dto)
        {
            using var context = GetNewContext();
            var groupId = dto.Id;
            var group = context.RMSecurityGroup.Where(g => g.Id.Equals(groupId) && g.IsRemoved == false).FirstOrDefault() ?? throw new Exception("Group not exist");
            UpdateGroupUsers(groupId, dto.Users);
            return group;
        }

        public RMSecurityGroup EditBuiltInHoldManagerGroup(SecurityGroupDto dto)
        {
            using var context = GetNewContext();
            var groupId = dto.Id;
            var group = context.RMSecurityGroup.Where(g => g.Id.Equals(groupId) && g.IsRemoved == false).FirstOrDefault() ?? throw new Exception("Group not exist");
            UpdateGroupUsers(groupId, dto.Users);
            return group;
        }

        //Return all need show. group name description permission masks.
        public List<SimpleSecurityGroupDto> LoadAllGroup()
        {
            #region linq not used
            //using (var context = GetNewContext())
            //{
            //    var query = from ug in context.RMSecurityGroup.Where(g => g.IsRemoved == false)
            //                join up in context.Role on ug.RoleId equals up.RoleId

            //                select new
            //                {
            //                    Name = ug.Name,
            //                    Description = ug.Description,
            //                    PermissionMasks = up.PermissionMasks
            //                };
            //    query.ToList()
            //}
            #endregion
            List<SimpleSecurityGroupDto> gruopInfos = new List<SimpleSecurityGroupDto>();//RMSecurityGroups
            using (var context = GetNewContext())
            {
                string queryLoadGroup = $"select g.Id, g.Name, g.Description, r.PermissionMasks, r.SOPermissionMasks, r.PermissionExtensionMasks,r.ReportingPermission, g.IsEnableTrim from {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMSecurityGroups as g join {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.RMRoles as r on g.RoleId = r.RoleId where g.IsRemoved = 0 order by g.ModifiedTime desc, g.Id asc ";
                var result = context.Database.SqlQuery<SimpleSecurityGroupDto>(queryLoadGroup).ToList();
                if (result != null && result.Count > 0)
                {
                    foreach (var g in result)
                    {
                        gruopInfos.Add(new SimpleSecurityGroupDto
                        { 
                            Id= g.Id,
                            Name = g.Name,
                            Description = g.Description,
                            PermissionMasks = g.PermissionMasks,
                            PermissionExtensionMasks = g.PermissionExtensionMasks,
                            SOPermissionMasks = g.SOPermissionMasks,
                            ReportingPermission = (g.PermissionMasks & (long)RMPermissionMasks.ReportCenterEnduser) == (long)RMPermissionMasks.ReportCenterEnduser? (long)RMReportPermissionMasks.AccessAll: g.ReportingPermission,
                            IsEnableTrim = g.IsEnableTrim,
                            IsBuiltInGroup = IsBuiltInGroup(g.Id),
                            IsEnableApprovalSetting = ((RMPermissionExtensionMasks)g.PermissionExtensionMasks & RMPermissionExtensionMasks.ManualApprovalSettingEndUser) == RMPermissionExtensionMasks.ManualApprovalSettingEndUser,
                            IsEnableManageHold = ((RMPermissionExtensionMasks)g.PermissionExtensionMasks & RMPermissionExtensionMasks.ManageHoldEndUser) == RMPermissionExtensionMasks.ManageHoldEndUser,
                        });
                    }
                }
                var allGrpouIds = gruopInfos.Select(s => s.Id).ToList();
                var enableTrimGroupIds = context.RMSecurityGroup.AsNoTracking().Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id).ToHashSet();
                var termMappings = context.RMSecurityGroupTermMapping.AsNoTracking().Where(m => allGrpouIds.Contains(m.SecurityGroupId)).ToList();
                var termGroupMapped = termMappings.Where(m => m.Level == SecurityTermLevel.TermGroup).Select(m => m.TermObjId).ToList();
                var termSetMapped = termMappings.Where(m => m.Level == SecurityTermLevel.TermSet).Select(m => m.TermObjId).ToList();
                var allTermGroup = context.TermGruops.AsNoTracking().Where(tg => termGroupMapped.Contains(tg.UniqueId)).ToDictionary(tg => tg.UniqueId);
                var allTermSet = context.TermSets.AsNoTracking().Where(ts => termSetMapped.Contains(ts.UniqueId)).ToDictionary(ts => ts.UniqueId);

                var ruleMappings = context.RMSecurityGroupRuleMapping.AsNoTracking().Where(m => allGrpouIds.Contains(m.SecurityGroupId)).ToList();
                var ruleContainerMapped = ruleMappings.Where(m => m.Level == SecurityRuleLevel.RuleContainer).Select(m => m.RuleObjId).ToList(); ;
                var allRuleContainer = context.RMRuleContainers.AsNoTracking().Where(rc => ruleContainerMapped.Contains(rc.ContainerId)).ToDictionary(rc => rc.ContainerId);

                foreach (var groupInfo in gruopInfos)
                {
                    if (!groupInfo.IsEnableTrim)
                    {
                        groupInfo.TermScope = I18NEntity.GetString("RM_JS_Common_Pending");
                        groupInfo.RuleScope = I18NEntity.GetString("RM_JS_Common_Pending");
                        continue;
                    }
                    if (termMappings.Any(m => enableTrimGroupIds.Contains(m.SecurityGroupId) && m.SecurityGroupId == groupInfo.Id && m.Level == SecurityTermLevel.All))
                    {
                        groupInfo.TermScope = I18NEntity.GetString("RM_CP_AM_TermPermission_AllTermTitle");
                    }
                    else
                    {
                        var termGroupScope = termMappings.Where(m => m.SecurityGroupId == groupInfo.Id && m.Level == SecurityTermLevel.TermGroup)
                            .Select(m => allTermGroup.ContainsKey(m.TermObjId) ? allTermGroup[m.TermObjId].Name : string.Empty);
                        var termSetScope = termMappings.Where(m => m.SecurityGroupId == groupInfo.Id && m.Level == SecurityTermLevel.TermSet)
                            .Select(m => allTermSet.ContainsKey(m.TermObjId) ? allTermSet[m.TermObjId].Name : string.Empty);
                        groupInfo.TermScope = string.Join("; ", termGroupScope.Concat(termSetScope));
                    }
                    if (ruleMappings.Any(m => enableTrimGroupIds.Contains(m.SecurityGroupId) && m.SecurityGroupId == groupInfo.Id && m.Level == SecurityRuleLevel.All))
                    {
                        groupInfo.RuleScope = I18NEntity.GetString("RM_CP_AM_RulePermission_AllRuleTitle");
                    }
                    else
                    {
                        groupInfo.RuleScope = string.Join("; ", ruleMappings
                            .Where(m => m.SecurityGroupId == groupInfo.Id && m.Level == SecurityRuleLevel.RuleContainer).Select(m => allRuleContainer.ContainsKey(m.RuleObjId) ? I18NEntity.GetString(allRuleContainer[m.RuleObjId].Name) : string.Empty));
                    }
                }
                return gruopInfos;
            }
        }

        public SecurityGroupDto GetGroup(int id)
        {
            SecurityGroupDto groupDto = null;
            using (var ctx = GetNewContext())
            {
                var group = ctx.RMSecurityGroup.AsNoTracking().Where(o => o.Id == id).FirstOrDefault();
                if (group != null)
                {
                    var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
					groupDto = new SecurityGroupDto
                    {
                        Id = id,
                        Name = group.Name,
                        Description = group.Description,
                        IsEnableTrim = group.IsEnableTrim,
                        TermTreeNodeInfo = !String.IsNullOrEmpty(group.NodeString) ? JsonConvert.DeserializeObject<SecurityTermInfo>(group.NodeString, jsonSettings) : null,
                        RuleTreeNodeInfo = !String.IsNullOrEmpty(group.RuleNodeString) ? JsonConvert.DeserializeObject<SecurityRuleInfo>(group.RuleNodeString) : null
                    };
                    var termMappings = ctx.RMSecurityGroupTermMapping.AsNoTracking().Where(o => o.SecurityGroupId == id).ToList();
                    if (termMappings.Count > 0)
                    {
                        if (termMappings.Any(o => o.Level == SecurityTermLevel.All))
                        {
                            groupDto.SetTermPermissionMethod = TermPermissionMethod.All;
                        }
                        else {
                            groupDto.SetTermPermissionMethod = TermPermissionMethod.SpecifyScope;
                        }
                    }
                    else {
                        groupDto.SetTermPermissionMethod = TermPermissionMethod.None;
                    }


                    var ruleMappings = ctx.RMSecurityGroupRuleMapping.AsNoTracking().Where(o => o.SecurityGroupId == id).ToList();
                    if (ruleMappings.Count > 0)
                    {
                        if (ruleMappings.Any(o => o.Level == SecurityRuleLevel.All))
                        {
                            groupDto.SetRulePermissionMethod = RulePermissionMethod.All;
                        }
                        else
                        {
                            groupDto.SetRulePermissionMethod = RulePermissionMethod.SpecifyScope;
                        }
                    }
                    else
                    {
                        groupDto.SetRulePermissionMethod = RulePermissionMethod.None;
                    }


                    groupDto.UserIds = ctx.RMSecurityGroupMembership.AsNoTracking().Where(o => o.GroupId == id).Select(o => o.UserId).ToList();
                    groupDto.SelectedTermObjIds = ctx.RMSecurityGroupTermMapping.AsNoTracking().Where(o => id == o.SecurityGroupId).GroupBy(o => o.Level).ToDictionary(o => o.Key, p => p.Select(m => m.TermObjId).Distinct().ToList());
                    var groupRole = ctx.Role.AsNoTracking().Where(o => o.RoleId == group.RoleId).First();
                    groupDto.IsNewGroup = groupRole.IsNewGroup;
                    groupDto.PermissionMasks = groupRole.PermissionMasks;
                    bool isSOOnlyLicense = !LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense;
                    if (((RMPermissionMasks)groupRole?.PermissionMasks).UserHasThisPermission(RMPermissionMasks.ReportCenterEnduser))
                    {
                        groupDto.ReportingPermission = (int)RMReportPermissionMasks.AccessAll;
                    }
                    else if (isSOOnlyLicense && !groupDto.IsNewGroup)
                    {
                        groupDto.ReportingPermission = (long)RMReportPermissionMasks.RestoredDataEnduser | (long)RMReportPermissionMasks.ActionAuditEnduser;
                    }
                    else
                    {
                        groupDto.ReportingPermission = groupRole.ReportingPermission;
                    }
                    groupDto.IsUseReportingPermissionControl = groupDto.ReportingPermission > 0;
                    groupDto.SOPermissionMasks = groupRole.SOPermissionMasks;
                    groupDto.SubPermission1Masks = groupRole.SubPermission1;
                    groupDto.PermissionExtensionMasks = groupRole.PermissionExtensionMasks;
                    List<SecurityDataSourceScopeDto> scopeDto = new List<SecurityDataSourceScopeDto>();
                    var scopesInfo = ctx.RMScopeRoleAssignment.AsNoTracking().Where(o => o.GroupId == id).GroupBy(o => o.DataSourceType).ToDictionary(o => o.Key, p => p.ToList());
                    foreach (KeyValuePair<int, List<RMScopeRoleAssignment>> item in scopesInfo)
                    {
                        var dataSourceScopeInfo = new SecurityDataSourceScopeDto
                        {
                            DataSourceType = (SourceFlag)item.Key,
                            ScopeIds = item.Value.Select(o => o.ScopeId).ToList()
                        };
                        dataSourceScopeInfo.SubPermission = SubPermissionType.None;
                        scopeDto.Add(dataSourceScopeInfo);
                    }
                    groupDto.DataSourceScopeInfo = scopeDto;
                    groupDto.IsBuiltInGroup = IsBuiltInGroup(id);
                }
                return groupDto;
            }   
        }
        public List<RMSecurityGroup> GetAllGroup()
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSecurityGroup.AsNoTracking().Where(g => !g.IsRemoved).ToList();
            }
        }
        public List<RMSecurityGroup> GetAllGroupById(List<int> groupIds)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSecurityGroup.AsNoTracking().Where(g => !g.IsRemoved && groupIds.Contains(g.Id)).ToList();
            }
        }

        private void CreateOrUpdateScopePermission(RMDbContext context, int groupId, List<SecurityDataSourceScopeDto> dataSourceScopeInfo)
        {
            var allContainers = context.RMScopeRoleAssignment.RemoveRange(context.RMScopeRoleAssignment.Where(g => g.GroupId == groupId).ToList());
            context.SaveChanges();
            var allAddScopes = new List<RMScopeRoleAssignment>();
            foreach (var scopeInfo in dataSourceScopeInfo)
            {
                foreach (var scopeId in scopeInfo.ScopeIds)
                {
                    allAddScopes.Add(new RMScopeRoleAssignment()
                    {
                        DataSourceType = (int)scopeInfo.DataSourceType,
                        GroupId = groupId,
                        ScopeId = scopeId,
                    });
                }
            }
            context.RMScopeRoleAssignment.AddRange(allAddScopes);
            context.SaveChanges();
        }

        public List<string> GetGroupNames(List<int> ids)
        {
            using (var context = GetNewContext())
            {
                return context.RMSecurityGroup.AsNoTracking().Where(o => !o.IsRemoved && ids.Contains(o.Id)).OrderBy(o => o.Name).Select(o => o.Name).ToList();
            }
        }

        public List<string> GetGroupNames(List<string> userAndGroupIds)
        {
            using (var context = GetNewContext())
            {
                var groupNames = (from g in context.RMSecurityGroup.AsNoTracking().Where(o => !o.IsRemoved)
                             join m in context.RMSecurityGroupMembership.AsNoTracking().Where(o => userAndGroupIds.Contains(o.UserId))
                             on g.Id equals m.GroupId
                             orderby g.ModifiedTime descending
                             select g.Name).Distinct().ToList();
                return groupNames;
            }
        }

        public (TermPermissionMethod,Dictionary<Guid, List<Guid>>) GetTermGroupIdUserScopePermission(string userOrGroupId)
        {
            using var ctx = GetNewContext();
            var securityGroups = (from g in ctx.RMSecurityGroup.Where(o => !o.IsRemoved)
                                    join m in ctx.RMSecurityGroupMembership.Where(o => userOrGroupId == o.UserId)
                                    on g.Id equals m.GroupId
                                    select g).Distinct().ToList();
            var trimEndUserSecurityGroup = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups);
            List<int> trimEndUserSecurityGroupIds = trimEndUserSecurityGroup.Select(g => g.Id).ToList();
            TermPermissionMethod termPermissionType = TermPermissionMethod.None;
            Dictionary<Guid, List<Guid>> groupAndTermSetPermissions = new Dictionary<Guid, List<Guid>>();
            var termMappings = ctx.RMSecurityGroupTermMapping.Where(o => trimEndUserSecurityGroupIds.Contains(o.SecurityGroupId)).ToList();
            var disablePermissionTrimGroups = trimEndUserSecurityGroup.Where(o => !o.IsEnableTrim).ToList();

            if (termMappings.Any(o => o.Level == SecurityTermLevel.All) || disablePermissionTrimGroups.Any())
            {
                termPermissionType = TermPermissionMethod.All;
            }
            else if (termMappings.Count > 0)
            {
                var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(o => o.UniqueId).ToList();
                var hasSettingTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => trimEndUserSecurityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                var otherTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => trimEndUserSecurityGroupIds.Contains(o.SecurityGroupId) && allTermGroupIds.Contains(o.ParentId) && o.Level == SecurityTermLevel.TermSet).Select(o => o.ParentId).Distinct().ToList();
                var hasPermissionTermGroupIds = hasSettingTermGroupIds.Concat(otherTermGroupIds).Distinct().ToList();
                var hasPermissionTermGroups = ctx.TermGruops.Where(o => !o.IsRemoved && hasPermissionTermGroupIds.Contains(o.UniqueId)).ToList();

                termPermissionType = TermPermissionMethod.SpecifyScope;
                var termGroupDtos = new List<SimpleSecurityTermInfo>();
                foreach (var termGroup in hasPermissionTermGroups)
                {
                    var termGroupId = termGroup.UniqueId;
                    var termGroupDto = Convert2SimpleSecurityTerm(termGroup);
                    List<Guid> hasPermissionTermSetIds = null;
                    if (hasSettingTermGroupIds.Contains(termGroupId))
                    {
                        hasPermissionTermSetIds = ctx.TermSets.Where(o => !o.IsRemoved && o.TermSetType == (int)TermSetType.BusinessTerm && o.TermGroupId == termGroupId).Select(_ => _.UniqueId).ToList();
                    }
                    else
                    {
                        hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => trimEndUserSecurityGroupIds.Contains(o.SecurityGroupId) && o.ParentId == termGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                    }
                    groupAndTermSetPermissions.Add(termGroupId, hasPermissionTermSetIds);
                }
            }
            else
            {
                termPermissionType = TermPermissionMethod.None;
            }
            return (termPermissionType, groupAndTermSetPermissions);
        }

        public bool IsSupperAdminUser(List<string> userAndGroupIds)
        {
            using var ctx = GetNewContext();
            var securityGroupIds = (from g in ctx.RMSecurityGroup.Where(o => !o.IsRemoved)
                                    join m in ctx.RMSecurityGroupMembership.Where(o => userAndGroupIds.Contains(o.UserId))
                                    on g.Id equals m.GroupId
                                    select g.Id).Distinct().ToList();
            return ctx.LnkUserRole.Any(l => userAndGroupIds.Contains(l.UserId) && l.RoleId == (int)RMRoleType.ApplicationAdmin);
        }

        public SecurityUserPermissionsDto GetUserScopePermissions(List<string> userAndGroupIds)
        {
            var permissionInfo = new SecurityUserPermissionsDto();
            using (var ctx = GetNewContext())
            {
                var securityGroupIds = (from g in ctx.RMSecurityGroup.Where(o => !o.IsRemoved)
                                        join m in ctx.RMSecurityGroupMembership.Where(o => userAndGroupIds.Contains(o.UserId))
                                        on g.Id equals m.GroupId
                                        select g.Id).Distinct().ToList();
                List<int> trimEndUserSecurityGroupIds = new List<int>();
                permissionInfo.IsAdmin = ctx.LnkUserRole.Any(l => userAndGroupIds.Contains(l.UserId) && l.RoleId == (int)RMRoleType.ApplicationAdmin);
                var securityGroups = ctx.RMSecurityGroup.AsNoTracking().Where(g => securityGroupIds.Contains(g.Id)).ToList();
                if (!permissionInfo.IsAdmin)
                {
                    securityGroupIds = securityGroups.Select(g => g.Id).ToList();
                    //get sp or exo container ids
                    var scopePermissionList = new List<SecurityDataSourceScopeDto>();
                    var scopesInfo = ctx.RMScopeRoleAssignment.Where(o => securityGroupIds.Contains(o.GroupId)).GroupBy(o => o.DataSourceType).ToDictionary(o => o.Key, p => p.ToList());
                    foreach (KeyValuePair<int, List<RMScopeRoleAssignment>> item in scopesInfo)
                    {
                        var dataSourceScopeInfo = new SecurityDataSourceScopeDto
                        {
                            DataSourceType = (SourceFlag)item.Key,
                            ScopeIds = item.Value.Select(o => o.ScopeId).Distinct().ToList()
                        };
                        scopePermissionList.Add(dataSourceScopeInfo);
                    }
                    permissionInfo.ScopePermissionInfo = scopePermissionList;
                }
                trimEndUserSecurityGroupIds = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups).Select(g => g.Id).ToList();
                //get term permission 
                permissionInfo.TermPermissionInfo = GetUserSecurityTermInfo(ctx, trimEndUserSecurityGroupIds);
                permissionInfo.RulePermissionInfo = GetUserSecurityRuleInfo(ctx, trimEndUserSecurityGroupIds);
                //get all permissionMasks
                var roleIds = ctx.RMSecurityGroup.Where(o => securityGroupIds.Contains(o.Id)).Select(o => o.RoleId).Distinct();
                var roleInfo = ctx.Role.Where(o => roleIds.Contains(o.RoleId)).ToList();
                var rolePermissionMasks = roleInfo.Select(o => o.PermissionMasks).Distinct().ToList();
                permissionInfo.SecurityGroupPermissionMasks = rolePermissionMasks;
                permissionInfo.SecurityGroupSubPermissionMasks = roleInfo.Select(o => o.SubPermission1).Distinct().ToList();
                permissionInfo.SecurityGroupPermissionExtensionMasks = roleInfo.Select(o => o.PermissionExtensionMasks).Distinct().ToList();
                permissionInfo.SOPermissionMasks = roleInfo.Select(o => o.SOPermissionMasks).Distinct().ToList();
                permissionInfo.ReportPermissionMasks = roleInfo.Select(o => o.ReportingPermission).Distinct().ToList();
                permissionInfo.IsNewCreateGroupList = roleInfo.Select(o => o.IsNewGroup).Distinct().ToList();
                return permissionInfo;
            }
        }

        public List<Guid> GetScopeLocationPermission(SourceFlag sourceFlag)
        {
            using var ctx = GetNewContext();

            return ctx.RMScopeRoleAssignment
                .Where(x => x.DataSourceType == (int)sourceFlag)
                .Select(x => x.ScopeId)
                .Distinct()
                .ToList();
        }

        private UserSecurityTermPermissionDto GetUserSecurityTermInfo(RMDbContext ctx, List<int> securityGroupIds)
        {
            var termPermissionResult = new UserSecurityTermPermissionDto();
            var termMappings = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
            var disablePermissionTrimGroups = ctx.RMSecurityGroup.Where(o => securityGroupIds.Contains(o.Id) && !o.IsEnableTrim).ToList();

            if (termMappings.Any(o => o.Level == SecurityTermLevel.All) || disablePermissionTrimGroups.Any())
            {
                termPermissionResult.TermPermissionType = TermPermissionMethod.All;
                var allTermGroups = ctx.TermGruops.Where(o => !o.IsRemoved).ToList();
                var termGroupDtos = new List<SimpleSecurityTermInfo>();
                foreach (var termGroup in allTermGroups)
                {
                    var termGroupDto = Convert2SimpleSecurityTerm(termGroup);
                    var termSets = ctx.TermSets.Where(o => !o.IsRemoved && o.TermSetType == (int)TermSetType.BusinessTerm && o.TermGroupId == termGroup.UniqueId).ToList();
                    termGroupDto.SubTerms = termSets.ConvertAll(o => { return Convert2SimpleSecurityTerm(o); });
                    termGroupDto.SubTermCount = termSets.Count;
                    termGroupDto.SubPerSize = 10;
                    termGroupDtos.Add(termGroupDto);
                }
                termPermissionResult.TermGroups = termGroupDtos;
            }
            else if (termMappings.Count > 0)
            {
                var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(o => o.UniqueId).ToList();
                //有权限的TermGroup
                var hasSettingTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                //TermGroup下TermSet有权限
                var otherTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && allTermGroupIds.Contains(o.ParentId) && o.Level == SecurityTermLevel.TermSet).Select(o => o.ParentId).Distinct().ToList();
                var hasPermissionTermGroupIds = hasSettingTermGroupIds.Concat(otherTermGroupIds).Distinct().ToList();
                var hasPermissionTermGroups = ctx.TermGruops.Where(o => !o.IsRemoved && hasPermissionTermGroupIds.Contains(o.UniqueId)).ToList();

                termPermissionResult.TermPermissionType = TermPermissionMethod.SpecifyScope;
                var termGroupDtos = new List<SimpleSecurityTermInfo>();
                foreach (var termGroup in hasPermissionTermGroups)
                {
                    var termGroupId = termGroup.UniqueId;
                    var termGroupDto = Convert2SimpleSecurityTerm(termGroup);
                    List<RMTermSet> hasPermissionTermSets = null;
                    if (hasSettingTermGroupIds.Contains(termGroupId))
                    {
                        //termgroup是all权限
                        hasPermissionTermSets = ctx.TermSets.Where(o => !o.IsRemoved && o.TermSetType == (int)TermSetType.BusinessTerm && o.TermGroupId == termGroupId).ToList();
                    }
                    else
                    {
                        //TermGroup下TermSet有权限
                        var hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && o.ParentId == termGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                        hasPermissionTermSets = ctx.TermSets.Where(o => !o.IsRemoved && hasPermissionTermSetIds.Contains(o.UniqueId)).ToList();
                    }
                    termGroupDto.SubTerms = hasPermissionTermSets.ConvertAll(o => { return Convert2SimpleSecurityTerm(o); });
                    termGroupDto.SubTermCount = hasPermissionTermSets.Count;
                    termGroupDto.SubPerSize = 10;
                    termGroupDtos.Add(termGroupDto);
                }
                termPermissionResult.TermGroups = termGroupDtos;
            }
            else
            {
                termPermissionResult.TermPermissionType = TermPermissionMethod.None;
            }
            return termPermissionResult;
        }

        private SimpleSecurityTermInfo Convert2SimpleSecurityTerm(object termObj)
        {
            SimpleSecurityTermInfo simpleTermDto = new SimpleSecurityTermInfo();
            if (termObj is RMTermGroup)
            {
                var termGroup = termObj as RMTermGroup;
                simpleTermDto.Name = termGroup.Name;
                simpleTermDto.UniqueId = termGroup.UniqueId;
            }
            if (termObj is RMTermSet)
            {
                var termSet = termObj as RMTermSet;
                simpleTermDto.Name = termSet.Name;
                simpleTermDto.UniqueId = termSet.UniqueId;
                simpleTermDto.ParentId = termSet.TermGroupId;
            }
            return simpleTermDto;
        }

        public SecurityTermPermissionDto GetAllSecurityTerm(List<string> userAndGroupIds)
        {
            using (var ctx = GetNewContext())
            {
                SecurityTermPermissionDto result = new SecurityTermPermissionDto();
                var securityGroups = GetSecurityGroups(ctx, userAndGroupIds);
                securityGroups = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups);
                if (securityGroups.Any(g => !g.IsEnableTrim))
                {
                    result.TermPermissionType = TermPermissionMethod.All;
                    result.TermObjIds = new List<Guid>();
                    return result;
                }
                var securityGroupIds = securityGroups.Select(s => s.Id);
                var termMappings = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
                if (termMappings.Count > 0)
                {
                    if (termMappings.Any(o => o.Level == SecurityTermLevel.All))
                    {
                        var termSetIds = ctx.TermSets.Where(t =>t.TermSetType == TermSetType.Business || t.TermSetType == TermSetType.BusinessTerm).Select(t => t.Id).Distinct().ToList();
                        result.TermPermissionType = TermPermissionMethod.All;
                        result.TermObjIds = new List<Guid>();
                    }
                    else
                    {
                        //有权限的TermGroup
                        var hasSettingTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                        //有权限的TermSet
                        var hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();

                        var termSetIds = ctx.TermSets.Where(t => (hasSettingTermGroupIds.Contains(t.TermGroupId) || hasPermissionTermSetIds.Contains(t.UniqueId)) 
                                                            && (t.TermSetType == TermSetType.Business || t.TermSetType == TermSetType.BusinessTerm)).Select(t => t.Id).Distinct().ToList();
                        var termIds = ctx.Terms.Where(t => termSetIds.Contains(t.TermSetId)).Select(t => t.UniqueId).Distinct().ToList();
                        result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                        result.TermObjIds = termIds;
                    }
                }
                else
                {
                    result.TermPermissionType = TermPermissionMethod.None;
                    result.TermObjIds = new List<Guid>();
                }
                return result;
            }
        }

        public SecurityTermPermissionDto GetSecurityTermObjInfo(QuerySecurityTermObjDto dto)
        {
            using (var ctx = GetNewContext())
            {
                SecurityTermPermissionDto result = new SecurityTermPermissionDto();
                if (dto.ForPhysicalView && dto.SourceFlag == SourceFlag.Physical)
                {
                    var termObjIdsForView = GetForPhysicalView(dto, ctx);
                    result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                    result.TermObjIds = termObjIdsForView;
                    return result;
                }
                var securityGroups = GetSecurityGroups(ctx, dto.UserAndGroupIds);
                var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
                if (dto.SourceFlag == SourceFlag.Physical && dto.ContainerId != null && Guid.TryParse(dto.ContainerId, out var locationId))
                {
                    dto.ContainerId = RMLocationDao.LoadTopLocationIdBySubLocation(locationId).ToString();
                }
                if (dto.FilterByContentSource && dto.SourceFlag != SourceFlag.All)
                {
                    if (defaultContianerIdSources.Contains(dto.SourceFlag))
                    {
                        var contentGroupIds = RMSecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string>() { dto.ContainerId }, dto.SourceFlag);
                        securityGroups = ctx.RMSecurityGroup.Where(g => contentGroupIds.Contains(g.Id)).ToList();
                    }
                    else
                    {
                        securityGroups = RMSecurityTrimmingHelper.GetSecurityGroupsByContentScope(securityGroups, dto.SourceFlag, dto.ExcludeBuiltIn);
                    }
                }
                var onlyInEndUsersBuiltIn = securityGroups.Count == 1 && securityGroups.FirstOrDefault()?.RoleId == 2;
                securityGroups = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups);
                if (securityGroups.Any(g => !g.IsEnableTrim))
                {
                    result.TermPermissionType = TermPermissionMethod.All;
                    return result;
                }

                logger.Info($"GetSecurityTermObjInfo, SourceFlag: {dto.SourceFlag}, ContainerId: {dto.ContainerId}");
                if (securityGroups.Count != 0)
                {
                    logger.Info($"Get terms by group:{string.Join(", ", securityGroups.Select(g => g.Name))}");
                }
                else
                {
                    logger.Info("Get rest terms.");
                }

                if (securityGroups.Count == 0)
                {
                    var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id).ToList();
                    if (ctx.RMSecurityGroupTermMapping.Any(o => activeGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.All == o.Level))
                    {
                        result.TermPermissionType = TermPermissionMethod.None;
                        result.TermObjIds = new List<Guid>();
                        return result;
                    }
                    if (dto.Level == SecurityTermLevel.TermGroup)
                    {
                        var otherGroupMappedTermGroup = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                        if (otherGroupMappedTermGroup.Count > 0)
                        {
                            var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(o => o.UniqueId).ToList();
                            result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                            result.TermObjIds = allTermGroupIds.Where(t => !otherGroupMappedTermGroup.Contains(t)).ToList();
                        }
                        else
                        {
                            var otherGroupMappedTermSet = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                            if (otherGroupMappedTermSet.Count > 0)
                            {
                                var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(t => t.UniqueId).ToList();
                                result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                                result.TermObjIds = allTermGroupIds;
                            }
                            else
                            {
                                result.TermPermissionType = TermPermissionMethod.All;
                            }
                        }
                    }
                    else if (dto.Level == SecurityTermLevel.TermSet)
                    {
                        var parentTermGroupId = dto.ParentId;
                        var otherGroupMapped = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && o.ParentId == parentTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                        if (otherGroupMapped.Count > 0)
                        {
                            var allTermSetIds = ctx.TermSets.Where(o => o.TermGroupId == dto.ParentId && !o.IsRemoved).Select(o => o.UniqueId);
                            result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                            result.TermObjIds = allTermSetIds.Where(t => !otherGroupMapped.Contains(t)).ToList();
                        }
                        else
                        {
                            result.TermPermissionType = TermPermissionMethod.All;
                        }
                    }
                    return result;
                }

                var securityGroupIds = securityGroups.Select(s => s.Id);
                var termMappings = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
                if (termMappings.Count > 0)
                {
                    if (termMappings.Any(o => o.Level == SecurityTermLevel.All))
                    {
                        result.TermPermissionType = TermPermissionMethod.All;
                    }
                    else
                    {
                        if (dto.Level == SecurityTermLevel.TermGroup)
                        {
                            var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(o => o.UniqueId).ToList();
                            //有权限的TermGroup
                            var hasSettingTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                            //TermGroup下TermSet有权限
                            var otherTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && allTermGroupIds.Contains(o.ParentId) && o.Level == SecurityTermLevel.TermSet).Select(o => o.ParentId).Distinct().ToList();
                            result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                            result.TermObjIds = hasSettingTermGroupIds.Concat(otherTermGroupIds).Distinct().ToList();
                        }
                        else if (dto.Level == SecurityTermLevel.TermSet)
                        {
                            var parentTermGroupId = dto.ParentId;
                            //parent TermGroup有权限
                            if (ctx.RMSecurityGroupTermMapping.Any(o => securityGroupIds.Contains(o.SecurityGroupId) && o.TermObjId == parentTermGroupId && o.Level == SecurityTermLevel.TermGroup))
                            {
                                result.TermPermissionType = TermPermissionMethod.All;
                                ////返回TermGroup下所有TermSet
                                //result.TermObjIds = ctx.TermSets.Where(o => !o.IsRemoved && o.TermGroupId == parentTermGroupId).Select(o => o.UniqueId).Distinct().ToList();
                            }
                            else {
                                //返回TermGroup下有权限的Termset
                                var hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && o.ParentId == parentTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                if (hasPermissionTermSetIds.Count > 0)
                                {
                                    result.TermPermissionType = TermPermissionMethod.SpecifyScope;
                                    result.TermObjIds = hasPermissionTermSetIds;
                                }
                                else {
                                    result.TermPermissionType = TermPermissionMethod.None;
                                    result.TermObjIds = new List<Guid>();
                                }
                            }
                        }
                    }
                }
                else 
                {
                    result.TermPermissionType = TermPermissionMethod.None;
                    result.TermObjIds = new List<Guid>();
                }
                return result;
            }
        }

        private List<Guid> GetForPhysicalView(QuerySecurityTermObjDto dto, RMDbContext ctx)
        {
            List<Guid> termObjIds = new List<Guid>();
            var allPhysicalSettings = PhysicalRecordSettingDao.GetAllPhysicalRecordSettings();
            var allTermSetIds = allPhysicalSettings.Select(t => t.TermSetId).ToList();

            if (dto.Level == SecurityTermLevel.TermGroup)
            {
                var allTermGroupIds = ctx.TermSets.Where(t => allTermSetIds.Contains(t.UniqueId)).Select(t => t.TermGroupId).ToList();
                termObjIds = allTermGroupIds;
            }
            else if (dto.Level == SecurityTermLevel.TermSet)
            {
                termObjIds = allTermSetIds;
            }
            else if (dto.Level == SecurityTermLevel.Term)
            {
                var termSetIds = ctx.TermSets.Where(t => allTermSetIds.Contains(t.UniqueId)).Select(t => t.Id);
                termObjIds = ctx.Terms.Where(t => termSetIds.Contains(t.TermSetId)).Select(t => t.UniqueId).Distinct().ToList();
            }
            return termObjIds;
        }

        public List<RMSecurityGroup> GetSecurityGroupsBySource(List<RMSecurityGroup> securityGroups, SourceFlag sourceFlag)
        {
            var groupsFilterBySource = new List<RMSecurityGroup>();
            using (var ctx = GetNewContext())
            {
                var securityGroupRoleIds = securityGroups.Select(g => g.RoleId).ToList();
                var roles = ctx.Role.Where(r => securityGroupRoleIds.Contains(r.RoleId)).ToList();
                foreach (var group in securityGroups)
                {
                    var role = roles.FirstOrDefault(r => r.RoleId == group.RoleId);
                    var hasPermission = false;
                    switch (sourceFlag)
                    {
                        case SourceFlag.FileSystem:
                            hasPermission = ((RMPermissionMasks)role?.PermissionMasks).UserHasThisPermission(RMPermissionMasks.FSEnduser);
                            break;
                        case SourceFlag.Physical:
                            hasPermission = ((RMPermissionMasks)role?.PermissionMasks).UserHasThisPermission(RMPermissionMasks.PhysicalAdmin);
                            break;
                        case SourceFlag.SharePointOnPrem:
                            hasPermission = ((RMPermissionMasks)role?.PermissionMasks).UserHasThisPermission(RMPermissionMasks.SPOnPremEnduser);
                            break;
                        case SourceFlag.AzureFileShare:
                            hasPermission = ((RMPermissionExtensionMasks)role?.PermissionExtensionMasks).UserHasThisPermission(RMPermissionExtensionMasks.AzureFSAdmin);
                            break;
                        case SourceFlag.Box:
                            hasPermission = ((RMPermissionExtensionMasks)role?.PermissionExtensionMasks).UserHasThisPermission(RMPermissionExtensionMasks.BoxAdmin);
                            break;
                        case SourceFlag.Google:
                            hasPermission = ((RMPermissionExtensionMasks)role?.PermissionExtensionMasks).UserHasThisPermission(RMPermissionExtensionMasks.GoogleAdmin);
                            break;
                        default:
                            break;
                    }
                    if (hasPermission)
                    {
                        groupsFilterBySource.Add(group);
                    }
                }
            }
            return groupsFilterBySource;
        }

        public List<RMSecurityGroup> TrimEndUserAndFunctionSecurityGroups(List<RMSecurityGroup> securityGroups)
        {
            using (var ctx = GetNewContext())
            {
                var securityGroupRoleIds = securityGroups.Select(g => g.RoleId).ToList();
                var roleIdsOfExceptGroups = ctx.Role.Where(r => securityGroupRoleIds.Contains(r.RoleId)
                                                                    && r.PermissionMasks != (long)PermissionWrappers.StandardUser
                                                                    && r.PermissionMasks != (long)PermissionWrappers.ReviewUser
                                                                    && r.SOPermissionMasks != (long)RMSOPermissionMasks.RestoreCenterFullControl
                                                                    && r.SOPermissionMasks != (long)RMSOPermissionMasks.RestoreCenterExport
                                                                    && r.SOPermissionMasks != (long)RMSOPermissionMasks.RestoreCenterSearch
                                                                ).Select(r => r.RoleId).ToList();
                return securityGroups.Where(g => roleIdsOfExceptGroups.Contains(g.RoleId)).ToList();
            }
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRuleContainer ruleContainer)
        {
            return new SecurityRuleInfo
            {
                Id = ruleContainer.Id,
                UniqueId = ruleContainer.ContainerId,
                ParentId = Guid.Empty,
                Name = I18NEntity.GetString(ruleContainer.Name),
                Type = RMRuleType.RuleContainer,
            };
        }

        private SecurityRuleInfo Convert2SecurityRuleInfo(RMRule rule, Guid parentId)
        {
            return new SecurityRuleInfo
            {
                Id = rule.Id,
                UniqueId = rule.RuleId,
                Name = rule.RuleName,
                Type = RMRuleType.Rule,
                ParentId = parentId
            };
        }

        private UserSecurityRulePermissionDto GetUserSecurityRuleInfo(RMDbContext ctx, List<int> securityGroupIds)
        {
            var rulePermissionResult = new UserSecurityRulePermissionDto();
            var disablePermissionTrimGroups = ctx.RMSecurityGroup.Where(o => securityGroupIds.Contains(o.Id) && !o.IsEnableTrim).ToList();
            var rulesMappings = ctx.RMSecurityGroupRuleMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
            if (rulesMappings.Any(o => o.Level == SecurityRuleLevel.All) || disablePermissionTrimGroups.Any())
            {
                rulePermissionResult.RulePermissionType = RulePermissionMethod.All;
                var allRuleContainers = RMRuleDao.GetAllRuleContainers();
                var ruleContainerDtos = new List<SecurityRuleInfo>();
                foreach (var ruleContainer in allRuleContainers)
                {
                    var ruleContainerGroupDto = Convert2SecurityRuleInfo(ruleContainer);
                    var rules = RMRuleDao.GetAvailableRules(new List<Guid> { ruleContainer.ContainerId });
                    ruleContainerGroupDto.SubItems = rules.ConvertAll(o => { return Convert2SecurityRuleInfo(o, ruleContainer.ContainerId); });
                    ruleContainerGroupDto.SubItemCount = rules.Count;
                    ruleContainerGroupDto.SubPerSize = 10;
                    ruleContainerDtos.Add(ruleContainerGroupDto);
                }
                rulePermissionResult.RuleContainers = ruleContainerDtos;
            }
            else if (rulesMappings.Count > 0)
            {
                rulePermissionResult.RulePermissionType = RulePermissionMethod.SpecifyScope;
                var hasSettingRuleContainers = ctx.RMSecurityGroupRuleMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.RuleContainer == o.Level).Distinct().ToList();
                var ruleContainerDtos = new List<SecurityRuleInfo>();
                foreach (var ruleMapping in hasSettingRuleContainers)
                {
                    var ruleContainerId = ruleMapping.RuleObjId;
                    var ruleContainer = RMRuleDao.GetRuleContainersById(ruleContainerId);
                    var ruleContainerDto = Convert2SecurityRuleInfo(ruleContainer);
                    var hasPermissionTermSets = RMRuleDao.GetAvailableRules(new List<Guid> { ruleContainerId });
                    ruleContainerDto.SubItems = hasPermissionTermSets.ConvertAll(o => { return Convert2SecurityRuleInfo(o, ruleContainerId); });
                    ruleContainerDto.SubItemCount = hasPermissionTermSets.Count;
                    ruleContainerDto.SubPerSize = 10;
                    ruleContainerDtos.Add(ruleContainerDto);
                }
                rulePermissionResult.RuleContainers = ruleContainerDtos;
            }
            else
            {
                rulePermissionResult.RulePermissionType = RulePermissionMethod.None;
            }
            return rulePermissionResult;
        }


        public bool DoesUserHasPermisionToTerm(List<Guid> termObjIds, QuerySecurityTermObjDto dto)
        {
            var result = false;
            using (var ctx = GetNewContext())
            {
                if (dto.ForPhysicalView && dto.SourceFlag == SourceFlag.Physical)
                {
                    var termObjIdsForView = GetForPhysicalView(dto, ctx);
                    var tempObjResult = true;
                    foreach (var gId in termObjIds)
                    {
                        if (!termObjIdsForView.Contains(gId))
                        {
                            tempObjResult = false;
                            break;
                        }
                    }
                    result = tempObjResult;
                    return result;
                }

                if(dto.SourceFlag == SourceFlag.Physical && dto.ContainerId != null && Guid.TryParse(dto.ContainerId, out var locationId))
                {
                    dto.ContainerId = RMLocationDao.LoadTopLocationIdBySubLocation(locationId).ToString();
                }

                var securityGroups = GetSecurityGroups(ctx, dto.UserAndGroupIds);
                var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
                if (dto.FilterByContentSource && dto.SourceFlag != SourceFlag.All)
                {
                    if (defaultContianerIdSources.Contains(dto.SourceFlag))
                    {
                        var contentGroupIds = RMSecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string>() { dto.ContainerId }, dto.SourceFlag);
                        securityGroups = ctx.RMSecurityGroup.Where(g => contentGroupIds.Contains(g.Id)).ToList();
                    }
                    else
                    {
                        securityGroups = RMSecurityTrimmingHelper.GetSecurityGroupsByContentScope(securityGroups, dto.SourceFlag, dto.ExcludeBuiltIn);
                    }
                }
                var onlyInEndUsersBuiltIn = securityGroups.Count == 1 && securityGroups.FirstOrDefault()?.RoleId == 2;
                securityGroups = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups);
                if (securityGroups.Any(g => !g.IsEnableTrim))
                {
                    return true;
                }

                if (securityGroups.Count == 0)
                {
                    var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id).ToList();
                    if (ctx.RMSecurityGroupTermMapping.Any(o => activeGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.All == o.Level))
                    {
                        return result;
                    }
                    if (dto.Level == SecurityTermLevel.TermGroup)
                    {
                        var otherGroupMappedTermGroup = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                        if (otherGroupMappedTermGroup.Count > 0)
                        {
                            var tempGroupResult = true;
                            foreach (var gId in termObjIds)
                            {
                                if (otherGroupMappedTermGroup.Contains(gId))
                                {
                                    tempGroupResult = false;
                                    break;
                                }
                            }
                            result = tempGroupResult;
                        }
                        else
                        {
                            result = true;
                        }
                    }
                    else if (dto.Level == SecurityTermLevel.TermSet)
                    {
                        var tempResult = true;
                        foreach (var termSetId in termObjIds)
                        {
                            var pTermGroupId = ctx.TermSets.Where(o => !o.IsRemoved && o.UniqueId == termSetId).Select(o => o.TermGroupId).FirstOrDefault();
                            //Parent TermGroup在其他security group中
                            if (ctx.RMSecurityGroupTermMapping.Any(o => activeGroupIds.Contains(o.SecurityGroupId) && o.TermObjId == pTermGroupId && o.Level == SecurityTermLevel.TermGroup))
                            {
                                tempResult = false;
                                break;
                            }
                            else
                            {
                                //Termset在其他security group中
                                var otherGroupMapped = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && o.ParentId == pTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                if (otherGroupMapped.Contains(termSetId))
                                {
                                    tempResult = false;
                                    break;
                                }
                            }
                        }
                        result = tempResult;
                    }
                    else if (dto.Level == SecurityTermLevel.Term)
                    {
                        var pTermSetIds = ctx.Terms.Where(o => !o.IsRemoved && termObjIds.Contains(o.UniqueId)).Select(o => o.TermSetId).Distinct().ToList();
                        var pTermSets = ctx.TermSets.Where(o => !o.IsRemoved && pTermSetIds.Contains(o.Id)).ToList();
                        var temp = true;
                        foreach (var pTermSet in pTermSets)
                        {
                            var pTermGroupId = pTermSet.TermGroupId;
                            //parent TermGroup在其他security group中
                            if (ctx.RMSecurityGroupTermMapping.Any(o => activeGroupIds.Contains(o.SecurityGroupId) && o.TermObjId == pTermGroupId && o.Level == SecurityTermLevel.TermGroup))
                            {
                                temp = false;
                                break;
                            }
                            else
                            {
                                //Termset在其他security group中
                                var otherGroupMapped = ctx.RMSecurityGroupTermMapping.Where(o => activeGroupIds.Contains(o.SecurityGroupId) && o.ParentId == pTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                if (otherGroupMapped.Contains(pTermSet.UniqueId))
                                {
                                    temp = false;
                                    break;
                                }
                            }
                        }
                        result = temp;
                    }
                    return result;
                }

                var securityGroupIds = securityGroups.Select(s => s.Id);
                var termMappings = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
                if (termMappings.Count > 0)
                {
                    if (termMappings.Any(o => o.Level == SecurityTermLevel.All))
                    {
                        result = true;
                    }
                    else {
                        switch (dto.Level)
                        {
                            case SecurityTermLevel.TermGroup:
                                var allTermGroupIds = ctx.TermGruops.Where(o => !o.IsRemoved).Select(o => o.UniqueId).ToList();
                                //有权限的TermGroup
                                var hasSettingTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && SecurityTermLevel.TermGroup == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                //TermGroup下TermSet有权限
                                var otherTermGroupIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && allTermGroupIds.Contains(o.ParentId) && o.Level == SecurityTermLevel.TermSet).Select(o => o.ParentId).Distinct().ToList();
                               
                                var allHasSettingGtoupIds = hasSettingTermGroupIds.Concat(otherTermGroupIds).Distinct().ToList();
                                var tempGroupResult = false;
                                foreach (var gId in termObjIds)
                                {
                                    if (!allHasSettingGtoupIds.Contains(gId))
                                    {
                                        tempGroupResult = false;
                                        break;
                                    }
                                    tempGroupResult = true;
                                }
                                result = tempGroupResult;
                                break;
                            case SecurityTermLevel.TermSet:
                                var tempResult = false;
                                foreach (var termSetId in termObjIds)
                                {
                                    var pTermGroupId = ctx.TermSets.Where(o => !o.IsRemoved && o.UniqueId == termSetId).Select(o => o.TermGroupId).FirstOrDefault();
                                    //parent TermGroup有权限
                                    if (ctx.RMSecurityGroupTermMapping.Any(o => securityGroupIds.Contains(o.SecurityGroupId) && o.TermObjId == pTermGroupId && o.Level == SecurityTermLevel.TermGroup))
                                    {
                                        tempResult = true;
                                        continue;
                                    }
                                    else
                                    {
                                        //返回TermGroup下有权限的Termset
                                        var hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && o.ParentId == pTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                        if (!hasPermissionTermSetIds.Contains(termSetId))
                                        {
                                            tempResult = false;
                                            break;
                                        }
                                        tempResult = true;
                                    }
                                }
                                result = tempResult;
                                break;
                            case SecurityTermLevel.Term:
                                var pTermSetIds = ctx.Terms.Where(o => !o.IsRemoved && termObjIds.Contains(o.UniqueId)).Select(o => o.TermSetId).Distinct().ToList();
                                var pTermSets = ctx.TermSets.Where(o => !o.IsRemoved && pTermSetIds.Contains(o.Id)).ToList();
                                var temp = false;
                                foreach (var pTermSet in pTermSets)
                                {
                                    var pTermGroupId = pTermSet.TermGroupId;
                                    //parent TermGroup有权限
                                    if (ctx.RMSecurityGroupTermMapping.Any(o => securityGroupIds.Contains(o.SecurityGroupId) && o.TermObjId == pTermGroupId && o.Level == SecurityTermLevel.TermGroup))
                                    {
                                        temp = true;
                                        continue;
                                    }
                                    else
                                    {
                                        //返回TermGroup下有权限的Termset
                                        var hasPermissionTermSetIds = ctx.RMSecurityGroupTermMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId) && o.ParentId == pTermGroupId && SecurityTermLevel.TermSet == o.Level).Select(o => o.TermObjId).Distinct().ToList();
                                        if (!hasPermissionTermSetIds.Contains(pTermSet.UniqueId))
                                        {
                                            temp = false;
                                            break;
                                        }
                                        temp = true;
                                    }
                                }
                                result = temp;
                                break;
                            default:
                                break;
                        }
                    }
                }
                else {
                    result = false;
                }
                
            }
            return result;
        }

        public List<RMSecurityGroupTermMapping> GetMappedTermByOtherGroups(int securityGroupId = 0)
        {
            using (var ctx = GetNewContext())
            {
                var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id);
                return ctx.RMSecurityGroupTermMapping.Where(o => o.SecurityGroupId != securityGroupId && activeGroupIds.Contains(o.SecurityGroupId)).ToList();
            }
        }

        public List<RMSecurityGroupRuleMapping> GetMappedRuleByOtherGroups(int securityGroupId = 0)
        {
            using (var ctx = GetNewContext())
            {
                var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id);
                return ctx.RMSecurityGroupRuleMapping.Where(o => o.SecurityGroupId != securityGroupId && activeGroupIds.Contains(o.SecurityGroupId)).ToList();
            }
        }

        public List<RMSecurityGroupTermMapping> GetMappedTermByGroup(int securityGroupId)
        {
            using (var ctx = GetNewContext())
            {
                var activeGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id);
                return ctx.RMSecurityGroupTermMapping.Where(o => o.SecurityGroupId == securityGroupId && activeGroupIds.Contains(o.SecurityGroupId)).ToList();
            }
        }

        public void RemoveTermMappings(List<RMSecurityGroupTermMapping> mappings)
        {
            using (var ctx = GetNewContext())
            {
                var entities = new List<RMSecurityGroupTermMapping>();
                foreach (var item in mappings)
                {
                    entities.AddRange(ctx.RMSecurityGroupTermMapping.Where(o => o.SecurityGroupId == item.SecurityGroupId && o.TermObjId == item.TermObjId));
                }
                ctx.RMSecurityGroupTermMapping.RemoveRange(entities);
                ctx.SaveChanges();
            }
        }

        public void RemoveRuleMappings(List<RMSecurityGroupRuleMapping> mappings)
        {
            using (var ctx = GetNewContext())
            {
                var entities = new List<RMSecurityGroupRuleMapping>();
                foreach (var item in mappings)
                {
                    entities.AddRange(ctx.RMSecurityGroupRuleMapping.Where(o => o.SecurityGroupId == item.SecurityGroupId && o.RuleObjId == item.RuleObjId));
                }
                ctx.RMSecurityGroupRuleMapping.RemoveRange(entities);
                ctx.SaveChanges();
            }
        }



        private List<RMSecurityGroup> GetSecurityGroups(RMDbContext ctx, List<string> userAndGroupIds)
        {
            var securityGroups = (from g in ctx.RMSecurityGroup.Where(o => !o.IsRemoved)
                                    join m in ctx.RMSecurityGroupMembership.Where(o => userAndGroupIds.Contains(o.UserId))
                                    on g.Id equals m.GroupId
                                    select g).Distinct().ToList();
            return securityGroups;
        }
        
        public List<RMSecurityGroup> GetSecurityGroups(List<string> userAndGroupIds)
        {
            using var ctx = GetNewContext();
            var securityGroups = (from g in ctx.RMSecurityGroup.Where(o => !o.IsRemoved)
                join m in ctx.RMSecurityGroupMembership.Where(o => userAndGroupIds.Contains(o.UserId))
                    on g.Id equals m.GroupId
                select g).Distinct().ToList();
            return securityGroups;
        }

        public List<Guid> GetSecurityGroupRuleContainers(int termId, out int securityGroupId)
        {
            using (var ctx = GetNewContext())
            {
                var ruleContainerIds = new List<Guid>();
                securityGroupId = -1;
                var termSetMembership = ctx.TermSetMemberships.FirstOrDefault(t => t.TermId == termId);
                var activeTrimGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id);
                var termSet = ctx.TermSets.FirstOrDefault(t => t.Id == termSetMembership.TermSetId);
                var termSetUniqueId = termSet?.UniqueId;
                var termGroupUniqueId = termSet?.TermGroupId;
                Expression<Func<RMSecurityGroupTermMapping, bool>> predicate = m => activeTrimGroupIds.Contains(m.SecurityGroupId) &&
                (m.TermObjId == termSetUniqueId || m.TermObjId == termGroupUniqueId || m.Level == SecurityTermLevel.All);
                var termMapping = ctx.RMSecurityGroupTermMapping.FirstOrDefault(predicate);
                if (termMapping != null)
                {
                    securityGroupId = termMapping.SecurityGroupId;
                    var sGroupId = securityGroupId;
                    var ruleMappings = ctx.RMSecurityGroupRuleMapping.Where(o => o.SecurityGroupId == sGroupId).ToList();
                    if (ruleMappings.Any(m => m.Level == SecurityRuleLevel.All))
                    {
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers().Select(o => o.ContainerId).ToList();
                    }
                    else
                    {
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers(ruleMappings.Select(o => o.RuleObjId).ToList())
                        .Select(o => o.ContainerId).ToList();
                    }
                }
                else
                {
                    if (!ctx.RMSecurityGroupRuleMapping.Any(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.All == o.Level))
                    {
                        var otherGroupMapped = ctx.RMSecurityGroupRuleMapping.Where(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.RuleContainer == o.Level).Select(m => m.RuleObjId).ToList();
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers().Where(o => !otherGroupMapped.Contains(o.ContainerId)).Select(o => o.ContainerId).ToList();
                    }
                }
                return ruleContainerIds;
            }
        }

        public List<Guid> GetSecurityGroupRuleContainers(Guid ruleId)
        {
            var ruleContainerIds = new List<Guid>();
            using (var ctx = GetNewContext())
            {
                var activeTrimGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id);
                var container = RMRuleDao.GetRuleContainersByRuleId(ruleId);
                if (container != null)
                {
                    var containerId = container.ContainerId;
                    Expression<Func<RMSecurityGroupRuleMapping, bool>> predicate = m => activeTrimGroupIds.Contains(m.SecurityGroupId) &&
                           (m.RuleObjId == containerId || m.Level == SecurityRuleLevel.All);
                    var ruleMapping = ctx.RMSecurityGroupRuleMapping.FirstOrDefault(predicate);
                    if (ruleMapping != null)
                    {
                        var sGroupId = ruleMapping.SecurityGroupId;
                        var ruleMappings = ctx.RMSecurityGroupRuleMapping.Where(o => o.SecurityGroupId == sGroupId).ToList();
                        if (ruleMappings.Any(m => m.Level == SecurityRuleLevel.All))
                        {
                            ruleContainerIds = RMRuleDao.GetAllRuleContainers().Select(o => o.ContainerId).ToList();
                        }
                        else
                        {
                            ruleContainerIds = RMRuleDao.GetAllRuleContainers(ruleMappings.Select(o => o.RuleObjId).ToList())
                            .Select(o => o.ContainerId).ToList();
                        }
                    }
                    else
                    {
                        if (!ctx.RMSecurityGroupRuleMapping.Any(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.All == o.Level))
                        {
                            var otherGroupMapped = ctx.RMSecurityGroupRuleMapping.Where(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.RuleContainer == o.Level).Select(m => m.RuleObjId).ToList();
                            ruleContainerIds = RMRuleDao.GetAllRuleContainers().Where(o => !otherGroupMapped.Contains(o.ContainerId)).Select(o => o.ContainerId).ToList();
                        }
                    }
                }
            }
            return ruleContainerIds;
        }

        public List<Guid> GetSecurityGroupRuleContainers(List<string> userAndGroupIds)
        {
            using (var ctx = GetNewContext())
            {
                var ruleContainerIds = new List<Guid>();

                var securityGroups = GetSecurityGroups(ctx, userAndGroupIds);
                securityGroups = RMSecurityTrimmingHelper.TrimEndUserAndFunctionSecurityGroups(securityGroups);
                var securityGroupIds = securityGroups.Select(s => s.Id);
                var ruleMappings = ctx.RMSecurityGroupRuleMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
                if (securityGroups.Any(g => !g.IsEnableTrim) || ruleMappings.Any(o => o.Level == SecurityRuleLevel.All))
                {
                    ruleContainerIds = RMRuleDao.GetAllRuleContainers().Select(o => o.ContainerId).ToList();
                }
                else if (ruleMappings.Count > 0)
                {
                    ruleContainerIds = RMRuleDao.GetAllRuleContainers(ruleMappings.Select(o => o.RuleObjId).ToList())
                        .Select(o => o.ContainerId).ToList();
                }
                return ruleContainerIds;
            }
        }


        public Dictionary<SecurityTermLevel, List<Guid>> GetSecurityTermObjIds(int securityGroupId)
        {
            using (var ctx = GetNewContext())
            {
                return ctx.RMSecurityGroupTermMapping.Where(o => securityGroupId == o.SecurityGroupId).GroupBy(o => o.Level).ToDictionary(o => o.Key, p => p.Select(m => m.TermObjId).Distinct().ToList());
            }
        }

        private void CreateOrUpdateSecurityTermMapping(RMDbContext ctx, int securityGroupId, List<SecurityTermInfo> selectedTermObjs, bool hasOpusILLicense)
        {
            if (hasOpusILLicense)
            {
                ctx.RMSecurityGroupTermMapping.RemoveRange(ctx.RMSecurityGroupTermMapping.Where(g => g.SecurityGroupId == securityGroupId).ToList());
                ctx.SaveChanges();
                var items = new List<RMSecurityGroupTermMapping>();
                if (selectedTermObjs != null && selectedTermObjs.Count > 0)
                {
                    foreach (var selTermObj in selectedTermObjs)
                    {
                        items.Add(new RMSecurityGroupTermMapping
                        {
                            SecurityGroupId = securityGroupId,
                            TermObjId = selTermObj.UniqueId,
                            ParentId = selTermObj.ParentId,
                            Level = GetTermLevel(selTermObj.Type)
                        });
                    }
                    if (items.Count > 0)
                    {
                        ctx.RMSecurityGroupTermMapping.AddRange(items);
                        ctx.SaveChanges();
                    }
                }
            }
        }

        private void CreateOrUpdateSecurityRuleMapping(RMDbContext ctx, int securityGroupId, List<SecurityRuleInfo> selectedTermObjs)
        {
            ctx.RMSecurityGroupRuleMapping.RemoveRange(ctx.RMSecurityGroupRuleMapping.Where(g => g.SecurityGroupId == securityGroupId).ToList());
            ctx.SaveChanges();
            var items = new List<RMSecurityGroupRuleMapping>();
            if (selectedTermObjs != null && selectedTermObjs.Count > 0)
            {
                foreach (var selTermObj in selectedTermObjs)
                {
                    items.Add(new RMSecurityGroupRuleMapping
                    {
                        SecurityGroupId = securityGroupId,
                        RuleObjId = selTermObj.UniqueId,
                        ParentId = selTermObj.ParentId,
                        Level = GetRuleLevel(selTermObj.Type)
                    });
                }
                if (items.Count > 0)
                {
                    ctx.RMSecurityGroupRuleMapping.AddRange(items);
                    ctx.SaveChanges();
                }
            }
        }

        private SecurityTermLevel GetTermLevel(RMTermType type)
        {
            switch (type)
            {
                case RMTermType.Root:
                    return SecurityTermLevel.All;
                case RMTermType.TermGroup:
                    return SecurityTermLevel.TermGroup;
                case RMTermType.TermSet:
                    return SecurityTermLevel.TermSet;
                default:
                    return SecurityTermLevel.None;
            }
        }

        private SecurityRuleLevel GetRuleLevel(RMRuleType type)
        {
            switch (type)
            {
                case RMRuleType.Root:
                    return SecurityRuleLevel.All;
                case RMRuleType.RuleContainer:
                    return SecurityRuleLevel.RuleContainer;
                case RMRuleType.Rule:
                    return SecurityRuleLevel.Rule;
                default:
                    return SecurityRuleLevel.None;
            }
        }

        private void CheckDataSourceIfExists(int roleId, RMDbContext context, List<SecurityDataSourceScopeDto> dataSourceScopeInfo)
        {
            var allRoleIds = context.RMSecurityGroup.Where(o => !o.IsRemoved).Select(o => o.RoleId).Distinct().ToList();
            var exceptRoleIds = new List<int> { roleId, PermissionWrappers.BuildInAdminRoleId, PermissionWrappers.BuildInEndUserRoleId };
            var needQueryRoleIds = allRoleIds.Except(exceptRoleIds);
            if (needQueryRoleIds != null && needQueryRoleIds.Count() > 0)
            {

                if (dataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.FileSystem))
                {
                    if (ExistsCheckedPermissionInOtherGroup(context, needQueryRoleIds, RMPermissionMasks.FSAdmin))
                    {
                        throw new Exception("FS data source already exists in other groups.");
                    }
                }

                if (dataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.SharePointOnPrem))
                {
                    if (ExistsCheckedPermissionInOtherGroup(context, needQueryRoleIds, RMPermissionMasks.SPOnPremEnduser))
                    {
                        throw new Exception("SP-onprem data source already exists in other groups.");
                    }
                }

                if (dataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.AzureFileShare))
                {
                    if (ExistsCheckedPermissionExtensionInOtherGroup(context, needQueryRoleIds, RMPermissionExtensionMasks.AzureFSAdmin))
                    {
                        throw new Exception("Azure File Share data source already exists in other groups.");
                    }
                }

                if (dataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.Box))
                {
                    if (ExistsCheckedPermissionExtensionInOtherGroup(context, needQueryRoleIds, RMPermissionExtensionMasks.BoxAdmin))
                    {
                        throw new Exception("Box data source already exists in other groups.");
                    }
                }

                if (dataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.Google))
                {
                    if (ExistsCheckedPermissionExtensionInOtherGroup(context, needQueryRoleIds, RMPermissionExtensionMasks.GoogleAdmin))
                    {
                        throw new Exception("Google data source already exists in other groups.");
                    }
                }
            }
        }

        /// <summary>
        /// 查询在其他Security Group(build-in group和当前group不计算在内)中是否已经存在要检查的权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="roleIdsParameterizedStatement"></param>
        /// <param name="roleIdsQueryParas"></param>
        /// <param name="checkedPermissionMasks"></param>
        /// <returns></returns>
        private bool ExistsCheckedPermissionInOtherGroup(RMDbContext context, IEnumerable<int> needQueryRoleIds, RMPermissionMasks checkedPermissionMasks)
        {
            var roleIdsParameterizedStatement = DatabaseUtility.BuildInClause(needQueryRoleIds, out List<SqlParameter> roleIdsQueryParas);
            string query = string.Format(@"SELECT count(0) FROM {0}.RMRoles WHERE (PermissionMasks & @checkedPermissionMasks) = @checkedPermissionMasks and IsRemoved = 0 and RoleId IN {1}", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), roleIdsParameterizedStatement);
            return context.Database.SqlQuery<int>(query, new SqlParameter[] { new SqlParameter("checkedPermissionMasks", checkedPermissionMasks) }.Concat(roleIdsQueryParas).ToArray()).FirstOrDefault() > 0;
        }

        /// <summary>
        /// 查询在其他Security Group(build-in group和当前group不计算在内)中是否已经存在要检查的扩展权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="roleIdsParameterizedStatement"></param>
        /// <param name="roleIdsQueryParas"></param>
        /// <param name="checkedPermissionMasks"></param>
        /// <returns></returns>
        private bool ExistsCheckedPermissionExtensionInOtherGroup(RMDbContext context, IEnumerable<int> needQueryRoleIds, RMPermissionExtensionMasks checkedPermissionMasks)
        {
            var roleIdsParameterizedStatement = DatabaseUtility.BuildInClause(needQueryRoleIds, out List<SqlParameter> roleIdsQueryParas);
            string query = string.Format(@"SELECT count(0) FROM {0}.RMRoles WHERE (PermissionExtensionMasks & @checkedPermissionMasks) = @checkedPermissionMasks and IsRemoved = 0 and RoleId IN {1}", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName), roleIdsParameterizedStatement);
            var result = context.Database.SqlQuery<int>(query, new SqlParameter[] { new SqlParameter("checkedPermissionMasks", checkedPermissionMasks) }.Concat(roleIdsQueryParas).ToArray()).FirstOrDefault();
            return result > 0;
        }

        public int LoadGroupIdHavePhysicalRecordManagerPermission()
        {
            using var context = GetNewContext();
            string query = string.Format(@"SELECT g.Id FROM {0}.[RMSecurityGroups] as g where g.RoleId in (
                SELECT r.RoleId FROM {0}.[RMRoles] as r WHERE (r.PermissionMasks & @recordManagerMasks) = @recordManagerMasks AND r.IsRemoved = 0 and r.RoleType = @roleType
            )", SecurityUtils.SanitizeSQLSchemaName(context.SchemaName));
            var result = context.Database.SqlQuery<int>(query, new SqlParameter[]
            {
                new SqlParameter("recordManagerMasks", RMPermissionMasks.PhysicalAdmin),
                new SqlParameter("roleType", RMRoleType.DeligatedAdmin)
            }).FirstOrDefault();
            return result;
        }

        public List<Guid> GetSecurityGroupRuleContainerIds(List<int> securityGroupIds)
        {
            using (var ctx = GetNewContext())
            {
                var ruleContainerIds = new List<Guid>();
                var activeTrimGroupIds = ctx.RMSecurityGroup.Where(g => !g.IsRemoved && g.IsEnableTrim).Select(g => g.Id).ToList(); ;
                var securityGroups = ctx.RMSecurityGroup.Where(o => securityGroupIds.Contains(o.Id));
                var ruleMappings = ctx.RMSecurityGroupRuleMapping.Where(o => securityGroupIds.Contains(o.SecurityGroupId)).ToList();
                if (ruleMappings.Count > 0)
                {
                    if (ruleMappings.Any(o => o.Level == SecurityRuleLevel.All))
                    {
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers().Select(o => o.ContainerId).ToList();
                    }
                    else
                    {
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers(ruleMappings.Select(o => o.RuleObjId).ToList())
                            .Select(o => o.ContainerId).ToList();
                    }
                }
                else 
                {
                    if (!ctx.RMSecurityGroupRuleMapping.Any(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.All == o.Level))
                    {
                        var otherGroupMapped = ctx.RMSecurityGroupRuleMapping.Where(o => activeTrimGroupIds.Contains(o.SecurityGroupId) && SecurityRuleLevel.RuleContainer == o.Level).Select(m => m.RuleObjId).ToList();
                        ruleContainerIds = RMRuleDao.GetAllRuleContainers().Where(o => !otherGroupMapped.Contains(o.ContainerId)).Select(o => o.ContainerId).ToList();
                    }
                }
                return ruleContainerIds;
            }
        }

        public bool IsBuiltInReviewUserGroup(int groupId)
        {
            var result = false;
            using var context = GetNewContext();
            var group = context.RMSecurityGroup.Where(o=> o.Id == groupId).FirstOrDefault();
            if (group != null)
            {
                var reviewUserRole = context.Role.Where(o => o.RoleType == RMRoleType.ReviewUser && o.RoleName == RecordsConstants.BuiltIn_ReviewRole_Name && o.RoleId == group.RoleId).FirstOrDefault();
                result = reviewUserRole != null;
    }
            return result;
        }
        public bool IsBuiltInHoldManagerGroup(int groupId)
        {
            var result = false;
            using var context = GetNewContext();
            var group = context.RMSecurityGroup.Where(o => o.Id == groupId).FirstOrDefault();
            if (group != null)
            {
                var holdUserRole = context.Role.Where(o => o.RoleType == RMRoleType.ManageHoldUser && o.RoleName == RecordsConstants.BuiltIn_HoldRole_Name && o.RoleId == group.RoleId).FirstOrDefault();
                result = holdUserRole != null;
            }
            return result;
        }
        

        public int GetBuitInReviewUserGroupId()
        {
            using var context = GetNewContext();
            var group = (from g in context.RMSecurityGroup
                     join r in context.Role
                     on g.RoleId equals r.RoleId
                     where r.RoleType == RMRoleType.ReviewUser 
                     && r.RoleName == RecordsConstants.BuiltIn_ReviewRole_Name
                     select g).FirstOrDefault();
            return group != null ? group.Id : 0;
        }

        private void UpdateGroupUsers(int groupId, List<AOSUserDto> users)
        {
            var userIds = new List<string>();
            if (users != null && users.Count > 0)
            {
                userIds = users.Select(o => o.UserId).ToList();
            }
            RMSecurityGroupMembershipDao.CreateOrUpdateGroupMemberShips(groupId, userIds);
        }

        private bool IsBuiltInGroup(int groupId)
        {
            if (groupId == (int)BuiltInGroupId.EndUser || groupId == (int)BuiltInGroupId.Admin || IsBuiltInReviewUserGroup(groupId) || IsBuiltInHoldManagerGroup(groupId))
            {
                return true;
            }
            return false;
        }
    }
}

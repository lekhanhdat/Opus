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
using AvePoint.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using AvePoint.RA.Service.Services.ControlPanel.Utils;
using LiteDB;
using Newtonsoft.Json;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    [Audit]
    public class SecurityGroupManagementService : RMServiceBase, ISecurityGroupManagementService
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SecurityGroupManagementService));
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private static readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();
        public ISPSettingTreeService mSPSettingTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        public IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        public IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        public ITenantInfoDao TenantDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        public ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        public IRMRuleDao RuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();
        public ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        public IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper); private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ITeamsSettingTreeService TeamsSettingTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.CreateSecurityGroup, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> CreateGroupAsync(SecurityGroupDto group)
        {
            var validateResult = await ValidateBeforeSavingAsync(group);
            if (validateResult.MessageType == RAMessageType.Failed) {
                logger.Error("Invalid security group");
                return validateResult;
            }
            RAReturnMessage message = new RAReturnMessage();
            try
            {
                //await CheckSecurityGroupAsync(group);
                RemoveTheFunctionModNoNeededField(group);
                var validateGroupUtil = new SecurityValidateUtil(group, LicenseHelperService, CheckDataSourceLicenseAsync);
                await validateGroupUtil.CheckSecurityGroupAsync();
                CalculatePermissionMasks(group);
                SetTermSettings(group);
                SetRuleSettings(group);
                SecurityGroupDao.CreateSecurityGroup(group);
                await SecurityTrimmingHelper.RemovePermissionCacheAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("name already exists"))
                {
                    message.FaildType = RAFailedType.NameExisting;
                }
                message.MessageType = RAMessageType.Failed;
                logger.Error($"An error while create group, message:{ex}");
            }
            return message;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.EditSecurityGroup, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> EditGroupAsync(SecurityGroupDto group)
        {
            var validateResult = await ValidateBeforeSavingAsync(group);
            if (validateResult.MessageType == RAMessageType.Failed)
            {
                logger.Error("Invalid security group");
                return validateResult;
            }
            RAReturnMessage message = new RAReturnMessage();
            try
            {
                CheckIsAllowModifiedGroup(group.Id);
                RemoveTheFunctionModNoNeededField(group);
                var action = GetSecurityGroupEditAction(group);
                if (action != null)
                {
                    await action();
                }
                await SecurityTrimmingHelper.RemovePermissionCacheAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("name already exists"))
                {
                    message.FaildType = RAFailedType.NameExisting;
                }
                message.MessageType = RAMessageType.Failed;
                logger.Error($"An error while edit group, message:{ex}");
            }
            return message;
        }
        private void RemoveTheFunctionModNoNeededField(SecurityGroupDto group)
        {
            logger.Info($"check is need RemoveTheFunctionModNoNeededField");
            if (group.SecurityGroupControlType == SecurityGroupControlType.FunctionModule)
            {
                logger.Info($"need to RemoveTheFunctionModNoNeededField");
                group.IsUseReportingPermissionControl = false;
                group.ReportingPermission = 0;
            }
            else
            {
                logger.Info($"no need RemoveTheFunctionModNoNeededField");
            }
        }
        private Func<System.Threading.Tasks.Task> GetSecurityGroupEditAction(SecurityGroupDto group)
        {
            if (group.Id == (int)BuiltInGroupId.Admin)
            {
                return null;
            }

            if (group.Id == (int)BuiltInGroupId.EndUser)
            {
                return () => { return EditBuiltInEndUserGroupAsync(group);};
            }

            if (SecurityGroupDao.IsBuiltInReviewUserGroup(group.Id))
            {
                return () => { EditBuiltInReviewUserGroup(group); return System.Threading.Tasks.Task.CompletedTask; };
            }
            if (SecurityGroupDao.IsBuiltInHoldManagerGroup(group.Id))
            {
                return () => { EditBuiltInHoldUserGroup(group); return Task.CompletedTask; };

            }
            return () => { return EditCustomGroupAsync(group); };
        }

        private async System.Threading.Tasks.Task EditCustomGroupAsync(SecurityGroupDto group)
        {
            var validateGroupUtil = new SecurityValidateUtil(group, LicenseHelperService, CheckDataSourceLicenseAsync);
            await validateGroupUtil.CheckSecurityGroupAsync();
            //await CheckSecurityGroupAsync(group);
            if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
            {
                group.HasOpusILLicense = true;
                SetTermSettings(group);
            }
            SetRuleSettings(group);
            CalculatePermissionMasks(group);
            await SecurityGroupDao.EditSecurityGroupAsync(group);
        }

        private async System.Threading.Tasks.Task EditBuiltInEndUserGroupAsync(SecurityGroupDto group)
        {
            await CheckSecurityGroupAsync(group);
            SetTermSettings(group);
            SetRuleSettings(group);
            CalculatePermissionMasks(group);
            await SecurityGroupDao.EditBuiltInEndUserGroupAsync(group);
        }

        private void EditBuiltInReviewUserGroup(SecurityGroupDto group)
        {
            SecurityGroupDao.EditBuiltInReviewUserGroup(group);
        }
        private void EditBuiltInHoldUserGroup(SecurityGroupDto group)
        {
            SecurityGroupDao.EditBuiltInHoldManagerGroup(group);
        }

        private async Task<RAReturnMessage> ValidateBeforeSavingAsync(SecurityGroupDto group)
        {
            try
            {
                var vGroup = new ValidateSecurityGroupDto()
                {
                    ValidateType = SecurityGroupValidateType.ValidateAll,
                    ValidateGroup = group
                };
                return await ValidateGroupTermAndRuleAsync(vGroup);
            }
            catch (Exception e)
            {
                logger.Warn($"RemoveOthersBeforeSaving error: {e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        public string EncodingStringUsingBase64(string content)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(content);
            return Convert.ToBase64String(buffer);
        }

        public string DecodingStringUsingBase64(string content)
        {
            byte[] buffer = Convert.FromBase64String(content);
            return Encoding.Unicode.GetString(buffer);
        }


        public static List<SecurityTermRuleConflictDto> GetConflictDto(RAReturnMessage message, SecurityGroupValidateType securityGroupValidateType)
        {
            var securityGroupValidateTypeInt = (int)securityGroupValidateType;
            var extensionData = (Dictionary<int, List<SecurityTermRuleConflictDto>>)message.Extsion1;
            return extensionData.ContainsKey(securityGroupValidateTypeInt) ? extensionData[securityGroupValidateTypeInt] : null;
        }

        public async Task<RAReturnMessage> ValidateGroupTermAndRuleAsync(ValidateSecurityGroupDto vGgroup)
        {
            RAReturnMessage message = new RAReturnMessage();

            try
            {
                var extensionData = new Dictionary<int, List<SecurityTermRuleConflictDto>>();
                var group = vGgroup.ValidateGroup;
                var ruleMapped = SecurityGroupDao.GetMappedRuleByOtherGroups(group.Id);
                var assignedByOthers = (await RMScopeRoleAssignmentDao.FindListAsync(a => a.GroupId != group.Id)).ToList();

                #region SourceContainer
                logger.Info("Validate SourceContainer");
                var assignedConflicts = new List<RMScopeRoleAssignment>();
                foreach (var sourceInfo in group.DataSourceScopeInfo)
                {
                    foreach (var scopeId in sourceInfo.ScopeIds)
                    {
                        assignedConflicts.AddRange(assignedByOthers.Where(a => a.DataSourceType == (int)sourceInfo.DataSourceType && a.ScopeId == scopeId).ToList());
                    }
                }
                if (assignedConflicts.Count > 0)
                {
                    var sourceConflictItems = assignedConflicts.GroupBy(m => m.GroupId);
                    var mappedSecurityGroupIds = assignedConflicts.Select(m => m.GroupId);
                    var securityGroupDic = (await SecurityGroupDao.FindListAsync(g => mappedSecurityGroupIds.Contains(g.Id))).ToDictionary(s => s.Id);
                    List<SecurityContainerDto> allContainers = new List<SecurityContainerDto>();
                    allContainers.AddRange(await GetContainersAsync(SourceFlag.SharePoint));
                    allContainers.AddRange(await GetContainersAsync(SourceFlag.OneDrive));
                    allContainers.AddRange(await GetContainersAsync(SourceFlag.Teams));
                    allContainers.AddRange(await GetContainersAsync(SourceFlag.Google));
                    allContainers.AddRange(await GetContainersAsync(SourceFlag.Physical));
                    if (LicenseHelperService.HasOpusILLicense)
                    {
                        allContainers.AddRange(await GetContainersAsync(SourceFlag.Exchange));
                    }
                    var securitySourceContainerConflictDtos = new List<SecurityTermRuleConflictDto>();

                    foreach (var cItemGrouping in sourceConflictItems)
                    {
                        var sItem = securityGroupDic[cItemGrouping.Key];
                        var cInfo = new SecurityTermRuleConflictDto()
                        {
                            ObjectId = sItem?.Id.ToString(),
                            ObjectName = sItem?.Name,
                            ConflictItems = new List<TermRuleConflictItemDto>()
                        };

                        foreach (var containerItem in cItemGrouping)
                        {
                            var containerInfo = allContainers.FirstOrDefault(c => c.Id == containerItem.ScopeId.ToString());
                            cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                            {
                                ItemName = containerInfo.Name,
                                ItemFullPath = containerInfo.Name,
                                ItemId = containerInfo.Id
                            });
                        }
                        securitySourceContainerConflictDtos.Add(cInfo);
                    }
                    extensionData[(int)SecurityGroupValidateType.ValidateSourceContainerConflict] = securitySourceContainerConflictDtos;
                    message.MessageType = RAMessageType.Failed;
                }
                #endregion

                var selectedTermSetIds = new List<int>();
                var selectedRuleIds = new List<Guid>();
                var ruleTreeNodeInfo = group.RuleTreeNodeInfo;

                if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
                {
                    logger.Info("Validate terms");
                    #region Term
                    var termMapped = SecurityGroupDao.GetMappedTermByOtherGroups(group.Id);
                    var treeNodeInfo = group.TermTreeNodeInfo;
                    if (group.SetTermPermissionMethod == TermPermissionMethod.All)
                    {
                        selectedTermSetIds.AddRange((await TermSetDao.FindListAsync(t => !t.IsRemoved)).Select(t => t.Id));
                        if (termMapped.Count > 0)
                        {
                            extensionData[(int)SecurityGroupValidateType.ValidateTermConflict] = await FindConflictTermsAsync(termMapped);
                            message.MessageType = RAMessageType.Failed;
                        }
                    }
                    else if (group.SetTermPermissionMethod == TermPermissionMethod.SpecifyScope)
                    {
                        #region Specify TermGroup or TermSet
                        var allTermSetMapped = new List<RMSecurityGroupTermMapping>();
                        var termGroupNodes = treeNodeInfo.SubTerms;
                        if (termGroupNodes != null)
                        {
                            //遍历TermGroup节点
                            foreach (var tGroup in termGroupNodes)
                            {
                                if (tGroup.IsChecked)
                                {
                                    selectedTermSetIds.AddRange(TermSetDao.GetRMTermSetsByGroupUniqueId(tGroup.UniqueId).Select(t => t.Id));
                                    var termSetMapped = termMapped.Where(s => s.Level == SecurityTermLevel.TermSet && s.ParentId == tGroup.UniqueId).ToList();
                                    allTermSetMapped.AddRange(termSetMapped);
                                }
                                else
                                {
                                    var termSetNodes = tGroup.SubTerms;
                                    if (termSetNodes != null)
                                    {
                                        foreach (var tSet in termSetNodes)
                                        {
                                            if (tSet.IsChecked)
                                            {
                                                selectedTermSetIds.Add(tSet.Id);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (allTermSetMapped.Count > 0)
                        {
                            extensionData[(int)SecurityGroupValidateType.ValidateTermConflict] = await FindConflictTermsAsync(allTermSetMapped);
                            message.MessageType = RAMessageType.Failed;
                        }
                        #endregion
                    }
                    #endregion
                }

                #region Rule

                if (group.SetRulePermissionMethod == RulePermissionMethod.All)
                {
                    logger.Info("Validate rules");
                    selectedRuleIds.AddRange(RuleDao.GetAvailableRules().Select(r => r.RuleId).ToList());
                    if (ruleMapped.Count > 0)
                    {
                        var securityRuleConflictDtos = new List<SecurityTermRuleConflictDto>();
                        var mappedSecurityGroupIds = ruleMapped.Select(m => m.SecurityGroupId);
                        var mappedRuleContainerIds = ruleMapped.Where(m => m.Level == SecurityRuleLevel.RuleContainer).Select(m => m.RuleObjId).ToList();
                        var securityGroupDic = (await SecurityGroupDao.FindListAsync(g => mappedSecurityGroupIds.Contains(g.Id))).ToDictionary(s => s.Id);
                        var ruleContainerGroupDic = RuleDao.GetAllRuleContainers(mappedRuleContainerIds).ToDictionary(s => s.ContainerId);
                        var conflictItems = ruleMapped.GroupBy(m => m.SecurityGroupId);

                        foreach (var cItemGrouping in conflictItems)
                        {
                            var sItem = securityGroupDic[cItemGrouping.Key];
                            var cInfo = new SecurityTermRuleConflictDto()
                            {
                                ObjectId = sItem?.Id.ToString(),
                                ObjectName = sItem?.Name,
                                ConflictItems = new List<TermRuleConflictItemDto>()
                            };
                            foreach (var mappingItem in cItemGrouping)
                            {
                                switch (mappingItem.Level)
                                {
                                    case SecurityRuleLevel.All:
                                        cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                                        {
                                            ItemName = I18NEntity.GetString("RM_CP_AM_RulePermission_AllRuleTitle"),
                                            ItemFullPath = I18NEntity.GetString("RM_CP_AM_RulePermission_AllRuleTitle"),
                                            ItemLevel = (int)SecurityRuleLevel.All
                                        });
                                        break;
                                    case SecurityRuleLevel.RuleContainer:
                                        var ruleContainerFromDic = ruleContainerGroupDic[mappingItem.RuleObjId];
                                        cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                                        {
                                            ItemName = I18NEntity.GetString(ruleContainerFromDic.Name),
                                            ItemFullPath = I18NEntity.GetString(ruleContainerFromDic.Name),
                                            ItemId = ruleContainerFromDic.ContainerId.ToString(),
                                            ItemLevel = (int)SecurityRuleLevel.RuleContainer
                                        });
                                        break;
                                }
                            }
                            securityRuleConflictDtos.Add(cInfo);
                        }

                        extensionData[(int)SecurityGroupValidateType.ValidateRuleConflict] = securityRuleConflictDtos;
                        message.MessageType = RAMessageType.Failed;
                    }
                }
                else if (group.SetRulePermissionMethod == RulePermissionMethod.SpecifyScope)
                {
                    #region Specify TermGroup or TermSet
                    logger.Info("Validate Specify TermGroup or TermSet");
                    if (ruleTreeNodeInfo.SubItems != null)
                    {
                        foreach (var tGroup in ruleTreeNodeInfo.SubItems)
                        {
                            if (tGroup.IsChecked)
                            {
                                selectedRuleIds.AddRange(RuleDao.GetAvailableRules(new List<Guid> { tGroup.UniqueId }).Select(r => r.RuleId).ToList());
                            }
                        }
                    }
                    if (LicenseHelperService.HasOpusSOLicense)
                    {
                        logger.Info("Validate rule and source container");
                        var hasUpgradeTeams = TeamsPermissionHelper.HasUpgradeTeamsFeature();
                        var teamsContainerIds = new List<Guid>();
                        if(hasUpgradeTeams)
                        {
                            logger.Info("Skip valid rule use in SharePoint when the account update teams and the site group is teams container");
                            teamsContainerIds.Add(new Guid(RMConstants.DefaultPrivateChannelSitesGroupId));
                            var containerIds = RMRemoteNodeDao.GetAllTeamsContainerIds();
                            foreach(var containerId in containerIds)
                            {
                                if(Guid.TryParse(containerId, out var teamsContainerId))
                                {
                                    teamsContainerIds.Add(teamsContainerId);
                                }
                            }
                        }
                        var useSelectedRuleRemoteNodeIds = (await EXOSettingRuleDao.FindListAsync(node => selectedRuleIds.Contains(node.RuleId))).Select(node => node.ScopeId);
                        var useSelectedRuleIds = (await EXOSettingRuleDao.FindListAsync(node => selectedRuleIds.Contains(node.RuleId))).Select(node => node.RuleId).ToList();
                        var useSelectedRuleSettingGroupIds = (await ArchiverSettingDao.FindListAsync(setting => useSelectedRuleRemoteNodeIds.Contains(setting.Id) && (setting.ContentSourceType != (int)ContentSourceType.SharePoint || !teamsContainerIds.Contains(setting.SiteGroupId)))).Select(setting => setting.SiteGroupId).Distinct();
                        var useSelectedRuleSettingInfo = await ArchiverSettingDao.FindListAsync(setting => useSelectedRuleRemoteNodeIds.Contains(setting.Id));
                        var allSelectedSourceContainerIds = new List<Guid>();
                        group.DataSourceScopeInfo.ForEach(source => allSelectedSourceContainerIds.AddRange(source.ScopeIds));
                        var usedInNoSelectedSourceContainer = useSelectedRuleSettingGroupIds.Except(allSelectedSourceContainerIds);
                        if (usedInNoSelectedSourceContainer.Any())
                        {
                            var sourceContainerRuleConflicts = new List<SecurityTermRuleConflictDto>();
                            foreach (var sourceContainerId in usedInNoSelectedSourceContainer)
                            {
                                var ruleConflictItems = useSelectedRuleSettingInfo.Where(setting => setting.SiteGroupId == sourceContainerId && (setting.ContentSourceType != (int)ContentSourceType.SharePoint || !teamsContainerIds.Contains(setting.SiteGroupId))).OrderBy(setting => setting.SiteId);
                                var existNodeConflictItems = new List<RMArchiverSetting>();
                                foreach (var item in ruleConflictItems)
                                {    
                                    if (RMRemoteNodeDao.CheckSiteExistBySiteId(item.SiteId.ToString()))
                                    {
                                        existNodeConflictItems.Add(item);
                                    }
                                    else
                                    {
                                        if (item.SiteId == Guid.Empty && RMRemoteNodeDao.CheckSiteExistBySiteId(item.SiteGroupId.ToString()))
                                        {
                                            existNodeConflictItems.Add(item);
                                        }
                                    }
                                }
                                var containerInfo = RMRemoteNodeDao.GetWebApplicationById(sourceContainerId.ToString());
                                if (containerInfo != null)
                                {
                                    var ruleConflict = new SecurityTermRuleConflictDto()
                                    {
                                        ObjectName = containerInfo.url,
                                        ObjectId = containerInfo.id,
                                        ConflictItems = existNodeConflictItems.ConvertAll(item => new TermRuleConflictItemDto()
                                        {
                                            ItemFullPath = item.Url,
                                            ItemId = item.Id.ToString(),
                                            ItemName = item.Url
                                        }).ToList()
                                    };
                                    sourceContainerRuleConflicts.Add(ruleConflict);
                                }
                            }
                            extensionData[(int)SecurityGroupValidateType.ValidateRuleAssociationNodeMissing] = sourceContainerRuleConflicts;
                            message.MessageType = RAMessageType.Failed;
                        }
                    }

                    #endregion
                }
                #endregion

                if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
                {
                    #region Missing Rule
                    logger.Info("Validate Missing Rule");
                    var allSelectedTerms = TermDao.GetAllTerms(selectedTermSetIds);
                    var allAssociationRuleByTerms = TermRuleAssociationDao.GetTermRuleInfoByTermIds(allSelectedTerms.Select(t => t.Id).ToList());
                    var missingRules = allAssociationRuleByTerms.Where(r => !selectedRuleIds.Contains(r.RuleId)).ToList();
                    var missingTermAssoRuleConflictDtos = FindConflictTermAssociationRule(missingRules);
                    if (missingTermAssoRuleConflictDtos.Count > 0)
                    {
                        extensionData[(int)SecurityGroupValidateType.ValidateTermAssociationRuleMissing] = missingTermAssoRuleConflictDtos;
                        message.MessageType = RAMessageType.Failed;
                    }
                    #endregion

                    #region Missing Term
                    logger.Info("Validate Missing Term");
                    var allAssociationRuleByRules = TermRuleAssociationDao.GetTermRuleInfoByRuleIds(selectedRuleIds).ToList();
                    var missingTerms = allAssociationRuleByRules.Where(t => !allSelectedTerms.Select(a => a.Id).ToList().Contains(t.TermId)).ToList();
                    var missingRuleAssoTermConflictDtos = FindConflictRuleAssociationTerm(missingTerms);
                    if (missingRuleAssoTermConflictDtos.Count > 0)
                    {
                        extensionData[(int)SecurityGroupValidateType.ValidateRuleAssociationTermMissing] = missingRuleAssoTermConflictDtos;
                        message.MessageType = RAMessageType.Failed;
                    }
                    #endregion
                }

                if (message.MessageType == RAMessageType.Failed)
                {
                    message.Extsion1 = extensionData;
                }
            }
            catch (Exception e)
            {
                logger.Error($"valid security error : {e}");
            }
            
            return message;
        }

        private List<SecurityTermRuleConflictDto> FindConflictTermAssociationRule(List<RMTermRuleAssociation> missingRules)
        {
            var missingTermRuleConflictDtos = new List<SecurityTermRuleConflictDto>();
            if (missingRules.Count > 0)
            {
                var ruleContainerNameDic = RuleDao.GetRuleContainerNameMemberships(missingRules.Select(r => r.RuleId).ToList());

                var missingRuleConflictItems = missingRules.GroupBy(m => m.TermId);
                foreach (var cItemGrouping in missingRuleConflictItems)
                {
                    var cTerm = missingRules.First(t => t.TermId == cItemGrouping.Key);
                    var cInfo = new SecurityTermRuleConflictDto()
                    {
                        ObjectName = cTerm.TermName,
                        ObjectId = cTerm.TermId.ToString(),
                        ConflictItems = new List<TermRuleConflictItemDto>()
                    };
                    foreach (var associationItem in cItemGrouping)
                    {
                        var containerName = ruleContainerNameDic.ContainsKey(associationItem.RuleId) ? ruleContainerNameDic[associationItem.RuleId] : string.Empty;
                        cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                        {
                            ItemName = associationItem.RuleName,
                            ItemFullPath = $"{containerName}/{associationItem.RuleName}",
                            ItemId = associationItem.RuleId.ToString()
                        });
                    }
                    missingTermRuleConflictDtos.Add(cInfo);
                }
            }
            return missingTermRuleConflictDtos;
        }

        private List<SecurityTermRuleConflictDto> FindConflictRuleAssociationTerm(List<RMTermRuleAssociation> missingTerms)
        {
            var missingTermRuleConflictDtos = new List<SecurityTermRuleConflictDto>();
            if (missingTerms.Count > 0)
            {
                //var ruleContainerNameDic = RuleDao.GetRuleContainerNameMemberships(missingTerms.Select(r => r.TermId).ToList());
                var termFullPathDic = TermDao.GetTermFullPathByTermIds(missingTerms.Select(r => r.TermId).ToList());

                var missingTermConflictItems = missingTerms.GroupBy(m => m.RuleId);
                foreach (var cItemGrouping in missingTermConflictItems)
                {
                    var cRule = missingTerms.First(t => t.RuleId == cItemGrouping.Key);
                    var cInfo = new SecurityTermRuleConflictDto()
                    {
                        ObjectName = cRule?.RuleName,
                        ObjectId = cRule?.RuleId.ToString(),
                        ConflictItems = new List<TermRuleConflictItemDto>()
                    };
                    foreach (var associationItem in cItemGrouping)
                    {
                        var termFullPath = termFullPathDic.ContainsKey(associationItem.TermId) ? termFullPathDic[associationItem.TermId] : string.Empty;
                        cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                        {
                            ItemName = associationItem.TermName,
                            ItemFullPath = termFullPath,
                            ItemId = associationItem.TermId.ToString()
                        });
                    }
                    missingTermRuleConflictDtos.Add(cInfo);
                }
            }
            return missingTermRuleConflictDtos;
        }

        private async Task<List<SecurityTermRuleConflictDto>> FindConflictTermsAsync(List<RMSecurityGroupTermMapping> mapped)
        {
            var mappedSecurityGroupIds = mapped.Select(m => m.SecurityGroupId);
            var mappedTermGroupIds = mapped.Where(m => m.Level == SecurityTermLevel.TermGroup).Select(m => m.TermObjId);
            var mappedTermSetIds = mapped.Where(m => m.Level == SecurityTermLevel.TermSet).Select(m => m.TermObjId);
            var securityGroupDic = (await SecurityGroupDao.FindListAsync(g => mappedSecurityGroupIds.Contains(g.Id))).ToDictionary(s => s.Id);
            
            var termSetDic = (await TermSetDao.FindListAsync(g => mappedTermSetIds.Contains(g.UniqueId))).ToDictionary(s => s.UniqueId);
            var termGroupIds = mappedTermGroupIds.Concat(termSetDic.Select(ts => ts.Value.TermGroupId));
            var termGroupDic = (await TermGroupDao.FindListAsync(g => termGroupIds.Contains(g.UniqueId))).ToDictionary(s => s.UniqueId);

            var conflictItems = mapped.GroupBy(m => m.SecurityGroupId);
            List<SecurityTermRuleConflictDto> securityTermRuleConflictDtos = new List<SecurityTermRuleConflictDto>();
            foreach (var cItemGrouping in conflictItems)
            {
                var sItem = securityGroupDic[cItemGrouping.Key];
                var cInfo = new SecurityTermRuleConflictDto()
                {
                    ObjectId = sItem?.Id.ToString(),
                    ObjectName = sItem?.Name,
                    ConflictItems = new List<TermRuleConflictItemDto>()
                };
                foreach (var mappingItem in cItemGrouping)
                {
                    switch (mappingItem.Level)
                    {
                        case SecurityTermLevel.All:
                            cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                            {
                                ItemName = I18NEntity.GetString("RM_CP_AM_TermPermission_AllTermTitle"),
                                ItemFullPath = I18NEntity.GetString("RM_CP_AM_TermPermission_AllTermTitle"),
                                ItemLevel = (int)SecurityTermLevel.All
                            });
                            break;
                        case SecurityTermLevel.TermGroup:
                            var termGroupFromDic = termGroupDic[mappingItem.TermObjId];
                            cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                            {
                                ItemName = termGroupFromDic.Name,
                                ItemFullPath = termGroupFromDic.Name,
                                ItemId = termGroupFromDic.UniqueId.ToString(),
                                ItemLevel = (int)SecurityTermLevel.TermGroup
                            });
                            break;
                        case SecurityTermLevel.TermSet:
                            var termSetFromDic = termSetDic[mappingItem.TermObjId];
                            cInfo.ConflictItems.Add(new TermRuleConflictItemDto()
                            {
                                //get term group name by term set
                                ItemName = termSetFromDic.Name,
                                //ItemFullPath = $"{termGroupDic[mappingItem.ParentId].Name}/{termSetFromDic.Name}",
                                ItemFullPath = $"{termGroupDic[termSetFromDic.TermGroupId].Name}/{termSetFromDic.Name}",
                                ItemId = termSetFromDic.UniqueId.ToString(),
                                ItemLevel = (int)SecurityTermLevel.TermSet
                            });
                            break;
                    }
                }
                securityTermRuleConflictDtos.Add(cInfo);
            }
            return securityTermRuleConflictDtos;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AccountManagement, Action = AuditAction.DeleteSecurityGroup, AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> DeleteGroupAsync(int id)
        {
            try
            {
                CheckIsAllowDeleteGroup(id);
                SecurityGroupDao.DeleteSecurityGroup(id);
                await SecurityTrimmingHelper.RemovePermissionCacheAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"An error while delete group, message:{ex}");
                return false;
            }
        }

        public List<SecurityContainerDto> GetContianers(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<SecurityGroupDto> GetGroupAsync(int id)
        {
            try
            {
                var group = SecurityGroupDao.GetGroup(id);
                if (group != null)
                {
                    group.Users = await UserService.GetUsersByIdsAsync(group.UserIds);
                }
                await AddScopeInfoAsync(group);
                return group;
            }
            catch (Exception ex)
            {
                logger.Error($"An error while get group by id [{id}], message:{ex}");
                return null;
            }
        }

        public List<Guid> GetAllAssignContainerIds()
        {
            return RMScopeRoleAssignmentDao.FindAll().Select(r => r.ScopeId).ToList();
        }

        public SecurityGroupDto GetSimpleGroup(int id)
        {
            var group = new SecurityGroupDto();
            try
            {
                group = SecurityGroupDao.GetGroup(id);
            }
            catch (Exception ex)
            {
                logger.Error($"An error while get simple group by id [{id}], message:{ex}");
            }
            return group;
        }

        public async Task<List<SimpleSecurityGroupDto>> GetGroupsAsync()
        {
            var groups = new List<SimpleSecurityGroupDto>();
            try
            {
                groups = SecurityGroupDao.LoadAllGroup(); //.Where(g => !g.Name.Equals("RM_CP_AM_ArchiverDefaultGroup_Admin_Title")).ToList();
                bool isSOOnlyLicense = !LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense;
                if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense)
                {
                    groups = groups.Where(g => !g.IsBuiltInGroup || (g.IsBuiltInGroup && g.Id == (int)BuiltInGroupId.Admin)).ToList();
                }
                
                foreach (var group in groups)
                {
                    if (isSOOnlyLicense && !group.IsNewCreatedGroup)
                    {
                        group.ReportingPermission = (long)RMReportPermissionMasks.RestoredDataEnduser | (long)RMReportPermissionMasks.ActionAuditEnduser;
                    }
                    await AddContainsSourceTypeAsync(group);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while get groups, message:{ex}");
            }
            return groups;
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(List<AOSUserDto> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users, false);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public async Task<List<SecurityContainerDto>> GetContainersAsync(SourceFlag source, bool isExcludeAssigned = false)
        {
            List<Guid> allAssignContainerIds = new List<Guid>();
            List<(Guid,int)> allAssignContainerIdAndDataSource = new List<(Guid, int)>();
            bool hasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams();
            if (isExcludeAssigned)
            {
                allAssignContainerIds = RMScopeRoleAssignmentDao.FindAll().Select(r => r.ScopeId).ToList();
                allAssignContainerIdAndDataSource = RMScopeRoleAssignmentDao.FindAll().Select(r => (r.ScopeId, r.DataSourceType)).ToList();
            }
            var containers = new List<SecurityContainerDto>();
            try
            {
                if (source == SourceFlag.Exchange)
                {
                    var exoRootNode = mSPSettingTreeService.LoadExchangeRoot()[0];
                    if (exoRootNode != null && !exoRootNode.Id.Equals(System.Guid.Empty))
                    {
                        List<RMSampleEXOTreeNode> children = new List<RMSampleEXOTreeNode>();
                        children = (await mSPSettingTreeService.BrowseSampleExchangeTreeAsync(exoRootNode)).OrderBy(a => a.Name).ToList();
                        foreach (var item in children)
                        {
                            if (isExcludeAssigned && allAssignContainerIds.Contains(new Guid(item.Id)))
                            {
                                continue;
                            }
                            containers.Add(new SecurityContainerDto
                            {
                                Id = item.Id,
                                Name = item.DisplayName,
                            });
                        }
                    }
                }
                else if (source == SourceFlag.Teams)
                {
                    var teamsRootNode = TeamsSettingTreeService.LoadFarmSampleTree().FirstOrDefault();
                    if(teamsRootNode != null && !teamsRootNode.Id.Equals(Guid.Empty))
                    {
                        List<RMSPSampleTreeNode> children = new List<RMSPSampleTreeNode>();
                        children = await TeamsSettingTreeService.BrowseSampleTreeAsync(teamsRootNode, false);
                        foreach (var item in children)
                        {
                            if (isExcludeAssigned && allAssignContainerIdAndDataSource.Any(_ => _.Item1.ToString().Equals(item.Id) && (!hasUpgradeTeams || (hasUpgradeTeams && _.Item2 == (int)SourceFlag.Teams))))
                            {
                                continue;
                            }
                            containers.Add(new SecurityContainerDto
                            {
                                Id = item.Id,
                                Name = item.Name,
                            });
                        }
                    }
                }
                else
                {
                    switch (source)
                    {
                        case SourceFlag.Physical:
                            return await GetTopLocationOfPhysicalRecordContentSource(isExcludeAssigned, allAssignContainerIds);
                    }
                    var farmNode = mSPSettingTreeService.LoadFarmSampleTree()[0];
                    if (farmNode != null && !farmNode.Id.Equals(Guid.Empty))
                    {
                        List<RMSPSampleTreeNode> children = new List<RMSPSampleTreeNode>();
                        children = await mSPSettingTreeService.BrowseSampleTreeAsync(farmNode, false, source == SourceFlag.SharePoint? RMBrowseTreeNodeSourceType.SharepointOnline : RMBrowseTreeNodeSourceType.SkyDrivePro);
                        foreach (var item in children)
                        {
                            if (isExcludeAssigned && allAssignContainerIds.Contains(new Guid(item.Id)))
                            {
                                continue;
                            }
                            containers.Add(new SecurityContainerDto
                            {
                                Id = item.Id,
                                Name = item.Name,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while Get [{source}] Containers, message:{ex}");
            }
            return containers;
        }

        private async Task<List<SecurityContainerDto>> GetTopLocationOfPhysicalRecordContentSource(bool isExcludeAssigned, List<Guid> allAssignContainerIds)
        {
            var containers = new List<SecurityContainerDto>();
            var topLocations = await RMLocationDao.GetAllTopLocation();
            foreach (var location in topLocations)
            {
                if (isExcludeAssigned && allAssignContainerIds.Contains(location.UniqueId))
                {
                    continue;
                }
                containers.Add(new SecurityContainerDto
                {
                    Id = location.UniqueId.ToString(),
                    Name = location.Name,
                });
            }
            return containers;
        }

        public async Task<SecurityUserPermissionsDto> GetUserScopePermissionsAsync(string userId, bool isFromGControl = false)
        {
            var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(userId);
            SecurityUserPermissionsDto dto = SecurityGroupDao.GetUserScopePermissions(userAndGroupIds);
            if (dto.ScopePermissionInfo == null)
            {
            dto.ScopePermissionInfo = new();
            }
            if (!dto.IsAdmin)
            {
                #region SP
                if (HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.SPOEnduser) || HasPermission(dto.SOPermissionMasks, RMSOPermissionMasks.SPOEnduser))
                {
                    var spScopeInfo = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.SharePoint).FirstOrDefault();
                    if (spScopeInfo != null)
                    {
                        spScopeInfo.IsScopeAdmin = true;
                    }
                }
                #endregion

                #region EXO
                if (LicenseHelperService.HasOpusILLicense && HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.EXOEnduser))
                {
                    var exoScopeInfo = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.Exchange).FirstOrDefault();
                    if (exoScopeInfo != null)
                    {
                        exoScopeInfo.IsScopeAdmin = true;
                    }
                }
                #endregion

                #region Phy
                if ((LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense) && HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.PhysicalAdmin))
                {
                    var physicalScope = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.Physical).FirstOrDefault();
                    if(physicalScope != null)
                    {
                        physicalScope.IsScopeAdmin = true;
                        physicalScope.SubPermission = SubPermissionType.Admin;
                    }
                    else
                    {
                        dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.Physical,
                            IsScopeAdmin = true,
                            SubPermission = SubPermissionType.Admin
                        });
                    }
                }
                else if ((LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense) && HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.PhysicalEndUser))
                {
                    var physicalSocpe = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.Physical).FirstOrDefault();
                    if(physicalSocpe != null)
                    {
                        physicalSocpe.IsScopeAdmin = true;
                        physicalSocpe.SubPermissions = GetPhySubPermissions(dto.SecurityGroupSubPermissionMasks);
                        physicalSocpe.SubPermission = SubPermissionType.EndUser;
                        physicalSocpe.ScopePaths = PermissionManagementService.GetlocationPathsCanBeViewed(await UserService.GetUserAndGroupIdsAsync(userId));
                    }
                    else
                    {
                        dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.Physical,
                            IsScopeAdmin = true,
                            SubPermission = SubPermissionType.EndUser,
                            ScopePaths = PermissionManagementService.GetlocationPathsCanBeViewed(await UserService.GetUserAndGroupIdsAsync(userId)),
                            SubPermissions = GetPhySubPermissions(dto.SecurityGroupSubPermissionMasks),
                        });
                    }
                }
                #endregion

                #region OneDrive
                if (HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.OneDriveEnduser) || HasPermission(dto.SOPermissionMasks, RMSOPermissionMasks.OneDriveEnduser))
                {
                    var oneDriveScopeInfo = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.OneDrive).FirstOrDefault();
                    if (oneDriveScopeInfo != null)
                    {
                        oneDriveScopeInfo.IsScopeAdmin = true;
                    }
                }
                #endregion

                #region Teams
                if (HasPermission(dto.SecurityGroupPermissionExtensionMasks, RMPermissionExtensionMasks.TeamsEndUser) || HasPermission(dto.SOPermissionMasks, RMSOPermissionMasks.TeamsEndUser))
                {
                    var spScopeInfo = dto.ScopePermissionInfo.Where(o => o.DataSourceType == SourceFlag.Teams).FirstOrDefault();
                    if (spScopeInfo != null)
                    {
                        spScopeInfo.IsScopeAdmin = true;
                    }
                }
                #endregion
            }
            else {
                if(LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusSOLicense)
                {
                    dto.ScopePermissionInfo = new List<SecurityDataSourceScopeDto>() {
                        new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.SharePoint,
                            IsScopeAdmin = true
                        },
                        new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.Exchange,
                            IsScopeAdmin = true
                        },
                        new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.Physical,
                            IsScopeAdmin = true,
                            SubPermission = SubPermissionType.Admin
                        },
                        new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.OneDrive,
                            IsScopeAdmin = true
                        },
                        new SecurityDataSourceScopeDto
                        {
                            DataSourceType = SourceFlag.Teams,
                            IsScopeAdmin = true
                        },
                    };

                    if (!LicenseHelperService.HasOpusILLicense)
                    {
                        var recordsOnlyDataSourceList = new List<SourceFlag> { SourceFlag.Exchange, SourceFlag.Physical };
                        dto.ScopePermissionInfo.ForEach(o =>
                        {
                            if (recordsOnlyDataSourceList.Contains(o.DataSourceType))
                            {
                                o.Hidden = true;
                            }
                        });
                        dto.ScopePermissionInfo = dto.ScopePermissionInfo.Where(o => !o.Hidden).ToList();
                    }
                }
                if (LicenseHelperService.HasOpusGoogleLicense)
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.Google,
                        IsScopeAdmin = true
                    });
                    if(!dto.ScopePermissionInfo.Any(_ => _.DataSourceType == SourceFlag.Physical))
                    {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.Physical,
                        IsScopeAdmin = true
                    });
                    }else
                    {
                        dto.ScopePermissionInfo.ForEach(o =>
                        {
                            if (o.DataSourceType == SourceFlag.Physical)
                            {
                                o.Hidden = false;
                            }
                        });
                    }
                }
            }

            #region FS/SPOnPrem
            if (dto.SecurityGroupPermissionMasks != null && dto.SecurityGroupPermissionMasks.Count > 0)
            {
                var permissionList = dto.SecurityGroupPermissionMasks.CombinePermissionsIntoString().SplitPermission();
                //如果fs license设置none，user对fs scope也是没有权限的
                var filteredPermissions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionList);
                if (HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.FSAdmin) && HasPermission(filteredPermissions, RMPermissionMasks.FSAdmin))
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.FileSystem,
                        IsScopeAdmin = true
                    });
                }
                //如果sp-onprem license设置none，user对sp-onprem scope也是没有权限的 
                if (HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.SPOnPremEnduser) && HasPermission(filteredPermissions, RMPermissionMasks.SPOnPremEnduser))
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.SharePointOnPrem,
                        IsScopeAdmin = true
                    });
                }
            }
            #endregion

            #region Permission Extension 
            if (dto.SecurityGroupPermissionExtensionMasks != null && dto.SecurityGroupPermissionExtensionMasks.Count > 0)
            {
                var permissionList = dto.SecurityGroupPermissionExtensionMasks.CombinePermissions<RMPermissionExtensionMasks>().SplitPermission();
                //如果azure files license设置none，user对azure files scope也是没有权限的
                var filteredPermissions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionList);
                #region Should be deleted when the box is separated into a separate permission
                //if (permissionList.Count == 1 && permissionList.Contains(RMPermissionExtensionMasks.BoxAdmin))
                //{
                //    filteredPermissions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, new List<RMPermissionExtensionMasks> { RMPermissionExtensionMasks.AzureFSAdmin});
                //}
                #endregion 
                if (HasPermission(dto.SecurityGroupPermissionExtensionMasks, RMPermissionExtensionMasks.AzureFSAdmin) && HasPermission(filteredPermissions, RMPermissionExtensionMasks.AzureFSAdmin))
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.AzureFileShare,
                        IsScopeAdmin = true
                    });
                }
               
                if (HasPermission(dto.SecurityGroupPermissionExtensionMasks, RMPermissionExtensionMasks.BoxAdmin) && HasPermission(filteredPermissions, RMPermissionExtensionMasks.BoxAdmin)) //Change box admin
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.Box,
                        IsScopeAdmin = true
                    });
                }
            }
            #endregion

            #region Google permission
            if(dto.SecurityGroupPermissionExtensionMasks != null && dto.SecurityGroupPermissionExtensionMasks.Count > 0)
            {
                var permissionList = dto.SecurityGroupPermissionExtensionMasks.CombinePermissions<RMPermissionExtensionMasks>().SplitPermission();
                //如果azure files license设置none，user对azure files scope也是没有权限的
                var filteredPermissions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionList);
                if (!dto.ScopePermissionInfo.Any(o => o.DataSourceType == SourceFlag.Google) && HasPermission(dto.SecurityGroupPermissionExtensionMasks, RMPermissionExtensionMasks.GoogleAdmin) && HasPermission(filteredPermissions, RMPermissionExtensionMasks.GoogleAdmin)) //Change box admin
                {
                    dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                    {
                        DataSourceType = SourceFlag.Google,
                        IsScopeAdmin = true
                    });
                }
            }
            #endregion
            #region GControl
            if (isFromGControl && await _tenantService.HasInitGControlPlatForm())
            {
                dto.ScopePermissionInfo.Add(new SecurityDataSourceScopeDto
                {
                    DataSourceType = SourceFlag.GGControl,
                    IsScopeAdmin = true
                });
            }
            #endregion

            var restoreCenterMask = 0L;
            foreach (var mask in dto.SOPermissionMasks)
            {
                if ((mask & (long)RMSOPermissionMasks.RestoreCenterSearch) == (long)RMSOPermissionMasks.RestoreCenterSearch)
                {
                    restoreCenterMask |= mask;
                }
            }

            foreach (var mask in dto.ReportPermissionMasks)
            {
                dto.ReportingPermission |= (int)mask;
            }
            bool isSOOnlyLicense = !LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense;
            if (isSOOnlyLicense && dto.IsNewCreateGroupList.Contains(false))
            {
                dto.ReportingPermission = (int)RMReportPermissionMasks.RestoredDataEnduser | (int)RMReportPermissionMasks.ActionAuditEnduser;
            }
            foreach (var mask in dto.SecurityGroupPermissionMasks)
            {
                if ((mask & (long)RMPermissionMasks.ReportCenterEnduser) == (long)RMPermissionMasks.ReportCenterEnduser)
                {
                    dto.ReportingPermission = (int)RMReportPermissionMasks.AccessAll;
                    break;
                }
            }
            dto.IsUseReportingPermissionControl = dto.ReportingPermission > 0;
            foreach (var mask in dto.SecurityGroupPermissionExtensionMasks)
            {
                if(((RMPermissionExtensionMasks)mask & RMPermissionExtensionMasks.ManageHoldEndUser) == RMPermissionExtensionMasks.ManageHoldEndUser)
                {
                    dto.IsEnableManageHold = true;
                }
                if (((RMPermissionExtensionMasks)mask & RMPermissionExtensionMasks.ManualApprovalSettingEndUser) == RMPermissionExtensionMasks.ManualApprovalSettingEndUser)
                {
                    dto.IsEnableApprovalSetting = true;
                }
                if (dto.IsEnableApprovalSetting && dto.IsEnableManageHold) break;
            }

            dto.FunctionMoudleRestoreCenter = (RMSOPermissionMasks)restoreCenterMask switch
            {
                RMSOPermissionMasks.RestoreCenterSearch => FunctionSubPermission.RestoreCenterSearch,
                RMSOPermissionMasks.RestoreCenterExport => FunctionSubPermission.RestoreCenterExport,
                RMSOPermissionMasks.RestoreCenterFullControl => FunctionSubPermission.RestoreCenterFullControl,
                _ => FunctionSubPermission.None,
            };
            dto.HasHoldManagerPermission = HasPermission(dto.SecurityGroupPermissionMasks, RMPermissionMasks.ManageHold);
            dto.SecurityGroupPermissionExtensionMasks = null;
            dto.SecurityGroupPermissionMasks = null;
            dto.SOPermissionMasks = null;
            return dto;
        }

        public List<SubPermission> GetPhySubPermissions(List<long> subPermissionMasks = null)
        {
            List<SubPermission> subPermissions = new List<SubPermission>();
            if (subPermissionMasks != null)
            {
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalAccessControl))
                {
                    subPermissions.Add(SubPermission.SetAccessControl);
                }
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalBoxCreationRequest))
                {
                    subPermissions.Add(SubPermission.BoxCreationRequest);
                }
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderCreationRequest))
                {
                    subPermissions.Add(SubPermission.FolderCreationRequest);
                }
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderLoanRequest))
                {
                    subPermissions.Add(SubPermission.FolderLoanRequest);
                }
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderLoanReturn))
                {
                    subPermissions.Add(SubPermission.FolderLoanReturn);
                }
                if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalMoveRequest))
                {
                    subPermissions.Add(SubPermission.MoveRequest);
                }
            }
            return subPermissions;
        }

        public SecurityTermInfo GetSecurityTermRootNode()
        {
            return new SecurityTermInfo
            {
                Id = -1,
                UniqueId = Guid.Empty,
                Name = "",
                Type = RMTermType.Root,
                IsExpand = false,
                IsChecked = false,
                SubPerIndex = 0,
                SubPerSize = 10, //初始值TermGroup 10个分页
                SubTermCount = 0,
                SubTerms = null
            };
        }

        public SecurityRuleInfo GetSecurityRuleRootNode()
        {
            return new SecurityRuleInfo
            {
                Id = -1,
                UniqueId = Guid.Empty,
                Name = "",
                Type = RMRuleType.Root,
                IsExpand = false,
                IsChecked = false,
                SubPerIndex = 0,
                SubPerSize = 10, //初始值TermGroup 10个分页
                SubItemCount = 0,
                SubItems = null
            };
        }
        private void CalculatePermissionMasks(SecurityGroupDto group)
        {
            var temp = RMPermissionMasks.None;
            var tempExtention = RMPermissionExtensionMasks.None;
            var tempReportPermission = RMReportPermissionMasks.None;
            var isAdminMasks = RMPermissionMasks.ContentRepositoyEnduser
                      | RMPermissionMasks.JobMonitorEnduser | RMPermissionMasks.ManualReviewEnduser
                      | RMPermissionMasks.RuleManagementEnduser | RMPermissionMasks.TermManagementEnduser;
            var scopeInfo = group.DataSourceScopeInfo;
            foreach (var item in scopeInfo)
            {
                switch (item.DataSourceType)
                {
                    case SourceFlag.SharePoint:
                        if (LicenseHelperService.HasOpusILLicense)
                        {
                            temp |= RMPermissionMasks.SPOEnduser | RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        }
                        break;
                    case SourceFlag.FileSystem:
                        temp |= RMPermissionMasks.FSAdmin | RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        break;
                    case SourceFlag.Exchange:
                        temp |= RMPermissionMasks.EXOEnduser | RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        break;
                    case SourceFlag.Physical:
                        if (item.SubPermission == SubPermissionType.Admin)
                        {
                            temp |= RMPermissionMasks.PhysicalAdmin | isAdminMasks;
                        }
                        else {
                            temp |= RMPermissionMasks.PhysicalEndUser;
                            group.SubPermission1Masks = (long)CalculateSubPermissionMasks(item);
                        }
                        break;
                    case SourceFlag.SharePointOnPrem:
                        temp |= RMPermissionMasks.SPOnPremEnduser | RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        break;
                    case SourceFlag.OneDrive:
                        if (LicenseHelperService.HasOpusILLicense)
                        {
                            temp |= RMPermissionMasks.OneDriveEnduser | RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        }
                        break;
                    case SourceFlag.AzureFileShare:
                        temp |= RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        tempExtention |= RMPermissionExtensionMasks.AzureFSAdmin;
                        break;
                    case SourceFlag.Box:
                        temp |= RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        tempExtention |= RMPermissionExtensionMasks.BoxAdmin;
                        break;
                    case SourceFlag.Google:
                        temp |= RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        tempExtention |= RMPermissionExtensionMasks.GoogleAdmin;
                        break;
                    case SourceFlag.Teams:
                        temp |= RMPermissionMasks.EletricRecordExplorerEnduser | isAdminMasks;
                        tempExtention |= RMPermissionExtensionMasks.TeamsEndUser;
                        break;
                    default:
                        break;
                }
            }
            CalculateReportConfigurationPermission(group, ref tempReportPermission);
            if (temp != RMPermissionMasks.None)
            {
                temp |= RMPermissionMasks.CommonModuleAccess;
            }
            CalculateConfigurationPermission(group, ref tempExtention);
            group.PermissionMasks = (long)temp;
            group.ReportingPermission = (long)tempReportPermission;
            group.PermissionExtensionMasks = (long)tempExtention;
            CalculateSOPermissionMasks(group);
        }

        private void CalculateConfigurationPermission(SecurityGroupDto group, ref RMPermissionExtensionMasks tempExtensions)
        {
            if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
            {
                if (group.IsEnableManageHold) tempExtensions |= RMPermissionExtensionMasks.ManageHoldEndUser;
                if (group.IsEnableApprovalSetting) tempExtensions |= RMPermissionExtensionMasks.ManualApprovalSettingEndUser;
            }
        }

        private void CalculateReportConfigurationPermission(SecurityGroupDto group, ref RMReportPermissionMasks tempExtensions)
        {
            //if (LicenseHelperService.HasOpusILLicense)
            //{
                if (group.IsUseReportingPermissionControl)
                {
                    if ((group.ReportingPermission & (int)ReportingPermission.ContentDueForAction) == (int)ReportingPermission.ContentDueForAction)
                    {
                        tempExtensions |= RMReportPermissionMasks.ContentDueForActionEnduser;
                    }
                    if ((group.ReportingPermission & (int)ReportingPermission.TermUsage) == (int)ReportingPermission.TermUsage)
                    {
                        tempExtensions |= RMReportPermissionMasks.TermUsageEnduser;
                    }
                    if ((group.ReportingPermission & (int)ReportingPermission.RuleUsage) == (int)ReportingPermission.RuleUsage)
                    {
                        tempExtensions |= RMReportPermissionMasks.RuleUsageEnduser;
                    }
                    if ((group.ReportingPermission & (int)ReportingPermission.CreationAndDestruction) == (int)ReportingPermission.CreationAndDestruction)
                    {
                        tempExtensions |= RMReportPermissionMasks.CreationAndDestructionEnduser;
                    }
                    if ((group.ReportingPermission & (int)ReportingPermission.ActionAudit) == (int)ReportingPermission.ActionAudit)
                    {
                        tempExtensions |= RMReportPermissionMasks.ActionAuditEnduser;
                    }
                    if ((group.ReportingPermission & (int)ReportingPermission.RestoredData) == (int)ReportingPermission.RestoredData)
                    {
                        tempExtensions |= RMReportPermissionMasks.RestoredDataEnduser;
                    }
                    if((group.ReportingPermission & (int)ReportingPermission.AvailableSpace) == (int)ReportingPermission.AvailableSpace)
                    {
                        tempExtensions |= RMReportPermissionMasks.AvailableSpaceEndUser;
                    }
                }
            //}
        }

        private void CalculateSOPermissionMasks(SecurityGroupDto group)
        {
            var soPermission = RMSOPermissionMasks.None;
            var scopeInfo = group.DataSourceScopeInfo;
            if (scopeInfo.Any(s => s.DataSourceType == SourceFlag.SharePoint))
            {
                soPermission |= RMSOPermissionMasks.SPOEnduser;
            }
            if (scopeInfo.Any(s => s.DataSourceType == SourceFlag.OneDrive))
            {
                soPermission |= RMSOPermissionMasks.OneDriveEnduser;
            }
            if (scopeInfo.Any(s => s.DataSourceType == SourceFlag.Teams))
            {
                soPermission |= RMSOPermissionMasks.TeamsEndUser;
            }
            if (soPermission != RMSOPermissionMasks.None)
            {
                soPermission |= RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.JobMonitorEnduser
                    | RMSOPermissionMasks.RuleManagementEnduser | RMSOPermissionMasks.CommonModuleAccess;
            }

            if (group.SecurityGroupControlType == SecurityGroupControlType.FunctionModule)
            {
                soPermission |= group.FunctionSubPermission switch
                {
                    FunctionSubPermission.None => RMSOPermissionMasks.None,
                    FunctionSubPermission.RestoreCenterFullControl => RMSOPermissionMasks.RestoreCenterFullControl,
                    FunctionSubPermission.RestoreCenterExport => RMSOPermissionMasks.RestoreCenterExport,
                    FunctionSubPermission.RestoreCenterSearch => RMSOPermissionMasks.RestoreCenterSearch,
                    _ => RMSOPermissionMasks.None,
                };
            }

            group.SOPermissionMasks = (long)soPermission;
        }

        public RMSubPermissionMasks CalculateSubPermissionMasks(SecurityDataSourceScopeDto scopeItemInfo)
        {
            var subPermissions = scopeItemInfo.SubPermissions;
            var result = RMSubPermissionMasks.None;
            if (subPermissions != null && subPermissions.Count > 0)
            {
                foreach (var permission in subPermissions)
                {
                    switch (permission)
                    {
                        case SubPermission.SetAccessControl:
                            result |= RMSubPermissionMasks.PhysicalAccessControl;
                            break;
                        case SubPermission.BoxCreationRequest:
                            result |= RMSubPermissionMasks.PhysicalBoxCreationRequest;
                            break;
                        case SubPermission.FolderCreationRequest:
                            result |= RMSubPermissionMasks.PhysicalFolderCreationRequest;
                            break;
                        case SubPermission.FolderLoanRequest:
                            result |= RMSubPermissionMasks.PhysicalFolderLoanRequest;
                            break;
                        case SubPermission.FolderLoanReturn:
                            result |= RMSubPermissionMasks.PhysicalFolderLoanReturn;
                            break;
                        case SubPermission.MoveRequest:
                            result |= RMSubPermissionMasks.PhysicalMoveRequest;
                            break;
                        default:
                            break;
                    }
                }
            }
            return result;
        } 

        private void SetTermSettings(SecurityGroupDto group)
        {
            if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense)
            {
                return;
            }
            group.HasOpusILLicense = true;
            var treeNodeInfo = group.TermTreeNodeInfo;
            List<SecurityTermInfo> selectedTermObjs = null;
            if (group.SetTermPermissionMethod == TermPermissionMethod.All)
            {
                #region Select All TermSets
                if (treeNodeInfo != null)
                {
                    NotifyInvalidTermSettings();
                }
                //只保存Root节点信息，用户对Term是All权限
                selectedTermObjs = new List<SecurityTermInfo>();
                var rootNode = GetSecurityTermRootNode();
                //treeNodeInfo.SubTerms = null;
                selectedTermObjs.Add(rootNode);
                #endregion
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.SpecifyScope)
            {
                #region Specify TermGroup or TermSet
                if (treeNodeInfo == null)
                {
                    NotifyInvalidTermSettings();
                }
                selectedTermObjs = new List<SecurityTermInfo>();
                ArgumentCheck.NotNull(treeNodeInfo, nameof(treeNodeInfo));
                var termGroupNodes = treeNodeInfo.SubTerms;
                if (termGroupNodes != null)
                {
                    //遍历TermGroup节点
                    foreach (var tGroup in termGroupNodes)
                    {
                        if (tGroup.IsChecked)
                        {
                            //Termgroup被选中，只保存termgroup信息，不保存sub termsets信息
                            selectedTermObjs.Add(tGroup);
                        }
                        else
                        {
                            var termSetNodes = tGroup.SubTerms;
                            if (termSetNodes != null)
                            {
                                foreach (var tSet in termSetNodes)
                                {
                                    if (tSet.IsChecked)
                                    {
                                        //Termset被选中，保存TermSet信息
                                        selectedTermObjs.Add(tSet);
                                    }
                                }
                            }
                        }
                    }
                }

                if (selectedTermObjs.Count == 0)
                {
                    NotifyInvalidTermSettings();
                }
                #endregion
            }
            else if (group.SetTermPermissionMethod == TermPermissionMethod.None)
            {
                #region 没有Term权限
                if (group.TermTreeNodeInfo != null)
                {
                    NotifyInvalidTermSettings();
                }
                #endregion
            }
            group.SelectedTermObjs = selectedTermObjs;
        }

        private void NotifyInvalidTermSettings()
        {
            throw new Exception("It's not a legal term settings.");
        }
        private void NotifyInvalidRuleSettings()
        {
            throw new Exception("It's not a legal rule settings.");
        }
        private void SetRuleSettings(SecurityGroupDto group)
        {
            var treeNodeInfo = group.RuleTreeNodeInfo;
            List<SecurityRuleInfo> selectedTermObjs = null;
            if (group.SetRulePermissionMethod == RulePermissionMethod.All)
            {
                #region Select All TermSets
                if (treeNodeInfo != null)
                {
                    NotifyInvalidRuleSettings();
                }
                //只保存Root节点信息，用户对Term是All权限
                selectedTermObjs = new List<SecurityRuleInfo>();
                var rootNode = GetSecurityRuleRootNode();
                //treeNodeInfo.SubTerms = null;
                selectedTermObjs.Add(rootNode);
                #endregion
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.SpecifyScope)
            {
                #region Specify TermGroup or TermSet
                if (treeNodeInfo == null)
                {
                    NotifyInvalidRuleSettings();
                }
                selectedTermObjs = new List<SecurityRuleInfo>();
                ArgumentCheck.NotNull(treeNodeInfo, nameof(treeNodeInfo));
                var termGroupNodes = treeNodeInfo.SubItems;
                if (termGroupNodes != null)
                {
                    //遍历Rule Container节点
                    foreach (var tGroup in termGroupNodes)
                    {
                        if (tGroup.IsChecked)
                        {
                            //Rule Container，Rule Container，不保存sub termsets信息
                            selectedTermObjs.Add(tGroup);
                        }
                        else
                        {
                            var termSetNodes = tGroup.SubItems;
                            if (termSetNodes != null)
                            {
                                foreach (var tSet in termSetNodes)
                                {
                                    if (tSet.IsChecked)
                                    {
                                        //Rule被选中，保存Rule信息
                                        selectedTermObjs.Add(tSet);
                                    }
                                }
                            }
                        }
                    }
                }

                if (selectedTermObjs.Count == 0)
                {
                    NotifyInvalidRuleSettings();
                }
                #endregion
            }
            else if (group.SetRulePermissionMethod == RulePermissionMethod.None)
            {
                #region 没有Term权限
                if (group.RuleTreeNodeInfo != null)
                {
                    NotifyInvalidRuleSettings();
                }
                #endregion
            }
            group.SelectedRuleObjs = selectedTermObjs;
        }
        private async System.Threading.Tasks.Task AddScopeInfoAsync(SecurityGroupDto group)
        {
            var permissionMasks = group.PermissionMasks;
            var permissionExtensionMasks = group.PermissionExtensionMasks;
            var soPermissionMasks = group.SOPermissionMasks;
            if (HasPermission(permissionMasks, RMPermissionMasks.SPOEnduser) || HasPermission(soPermissionMasks, RMSOPermissionMasks.SPOEnduser))
            {
                var spScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.SharePoint).FirstOrDefault();
                if (spScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.SharePoint, SubPermission = SubPermissionType.None });
                }
            }
            if (HasPermission(permissionMasks, RMPermissionMasks.EXOEnduser))
            {
                var exoScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.Exchange).FirstOrDefault();
                if (exoScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Exchange, SubPermission = SubPermissionType.None });
                }
            }
            if (HasPermission(permissionMasks, RMPermissionMasks.FSAdmin))
            {
                group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.FileSystem, SubPermission = SubPermissionType.None });
            }

            if (HasPermission(permissionMasks, RMPermissionMasks.PhysicalAdmin))
            {
                var phyScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.Physical).FirstOrDefault();
                if(phyScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Physical, SubPermission = SubPermissionType.Admin });
                }
                else
                {
                    phyScopeInfo.SubPermission = SubPermissionType.Admin;
                }
            }
            else if (HasPermission(permissionMasks, RMPermissionMasks.PhysicalEndUser))
            {
                var phyScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.Physical).FirstOrDefault();
                if(phyScopeInfo != null)
                {
                    phyScopeInfo.SubPermission = SubPermissionType.EndUser;
                    AddSubPermissions(group.SubPermission1Masks, phyScopeInfo);
                }
                else
                {
                    var scopeDto = new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Physical, SubPermission = SubPermissionType.EndUser };
                    AddSubPermissions(group.SubPermission1Masks, scopeDto);
                    group.DataSourceScopeInfo.Add(scopeDto);
                }
            }

            if (HasPermission(permissionMasks, RMPermissionMasks.SPOnPremEnduser))
            {
                var spLocalScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.SharePointOnPrem).FirstOrDefault();
                if (spLocalScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.SharePointOnPrem, SubPermission = SubPermissionType.None });
                }
            }

            if (HasPermission(permissionMasks, RMPermissionMasks.OneDriveEnduser) || HasPermission(soPermissionMasks, RMSOPermissionMasks.OneDriveEnduser))
            {
                var oneDriveScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.OneDrive).FirstOrDefault();
                if (oneDriveScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.OneDrive, SubPermission = SubPermissionType.None });
                }
            }

            if (HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.AzureFSAdmin))
            {
                if (!group.DataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.AzureFileShare))
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.AzureFileShare, SubPermission = SubPermissionType.None });
                }
            }

            if (HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.BoxAdmin))
            {
                if (!group.DataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.Box))
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Box, SubPermission = SubPermissionType.None });
                }
            }

            if (HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.GoogleAdmin))
            {
                if (!group.DataSourceScopeInfo.Any(o => o.DataSourceType == SourceFlag.Google))
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Google, SubPermission = SubPermissionType.None });
                }
            }

            if (HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.TeamsEndUser) || HasPermission(soPermissionMasks, RMSOPermissionMasks.TeamsEndUser))
            {
                var spScopeInfo = group.DataSourceScopeInfo.Where(o => o.DataSourceType == SourceFlag.Teams).FirstOrDefault();
                if (spScopeInfo == null)
                {
                    group.DataSourceScopeInfo.Add(new SecurityDataSourceScopeDto { DataSourceType = SourceFlag.Teams, SubPermission = SubPermissionType.None });
                }
            }

            group.AvailableDataSourceScopeInfo = new GroupsAndContainers();
            group.AvailableDataSourceScopeInfo.SPContainerItems = await GetContainersAsync(SourceFlag.SharePoint, isExcludeAssigned: true);
            group.AvailableDataSourceScopeInfo.OneDriveContainerItems = await GetContainersAsync(SourceFlag.OneDrive, isExcludeAssigned: true);
            group.AvailableDataSourceScopeInfo.EXOContainerItems = await GetContainersAsync(SourceFlag.Exchange, isExcludeAssigned: true);
            group.AvailableDataSourceScopeInfo.TeamsContainerItems = await GetContainersAsync(SourceFlag.Teams, isExcludeAssigned: true);
            group.AvailableDataSourceScopeInfo.PhysicalLocationItems = await GetContainersAsync(SourceFlag.Physical, isExcludeAssigned: true);

            if (LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense)
            {
                group.IsEnableApprovalSetting = HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.ManualApprovalSettingEndUser);
                group.IsEnableManageHold = HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.ManageHoldEndUser);
            }
        }

        private bool HasOneInILSourcePermission(long permissionMasks, long permissionExtensionMasks)
        {
            return HasPermission(permissionExtensionMasks, RMPermissionExtensionMasks.TeamsEndUser) || HasPermission(permissionMasks, RMPermissionMasks.OneDriveEnduser) ||
                HasPermission(permissionMasks, RMPermissionMasks.SPOEnduser) || HasPermission(permissionMasks, RMPermissionMasks.EXOEnduser);
        }

        private void AddSubPermissions(long subPermissionMasks, SecurityDataSourceScopeDto scopeDto)
        {
            var result = new List<SubPermission>();
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalAccessControl))
            {
                result.Add(SubPermission.SetAccessControl);
            }
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalBoxCreationRequest))
            {
                result.Add(SubPermission.BoxCreationRequest);
            }
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderCreationRequest))
            {
                result.Add(SubPermission.FolderCreationRequest);
            }
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderLoanRequest))
            {
                result.Add(SubPermission.FolderLoanRequest);
            }
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalFolderLoanReturn))
            {
                result.Add(SubPermission.FolderLoanReturn);
            }
            if (HasSubPermission(subPermissionMasks, RMSubPermissionMasks.PhysicalMoveRequest))
            {
                result.Add(SubPermission.MoveRequest);
            }
            scopeDto.SubPermissions = result;
        }

        private async System.Threading.Tasks.Task AddContainsSourceTypeAsync(SimpleSecurityGroupDto group)
        {
            group.ContainsSourceType = new List<SourceFlag>();
            var filteredPermissions = await GetFilteredPermissionAsync<RMPermissionMasks>(group.PermissionMasks);
            var filteredSOPermissions = await GetFilteredPermissionAsync<RMSOPermissionMasks>(group.SOPermissionMasks);
            var filteredPermissionsExtenstion = await GetFilteredPermissionAsync<RMPermissionExtensionMasks>(group.PermissionExtensionMasks);
            #region Should be deleted when the box is separated into a separate permission
            //if (group.PermissionExtensionMasks == (long)RMPermissionExtensionMasks.BoxAdmin)
            //{
            //    filteredPermissionsExtenstion = await GetFilteredPermissionAsync<RMPermissionExtensionMasks>((long)RMPermissionExtensionMasks.AzureFSAdmin);
            //}
            #endregion

            if (HasPermission(filteredPermissions, RMPermissionMasks.SPOEnduser) 
                || HasPermission(filteredSOPermissions, RMSOPermissionMasks.SPOEnduser))
            {
                group.ContainsSourceType.Add(SourceFlag.SharePoint);
            }
            if (HasPermission(filteredPermissions, RMPermissionMasks.OneDriveEnduser) 
                || HasPermission(filteredSOPermissions, RMSOPermissionMasks.OneDriveEnduser))
            {
                group.ContainsSourceType.Add(SourceFlag.OneDrive);
            }
            if (HasPermission(filteredPermissions, RMPermissionMasks.EXOEnduser))
            {
                group.ContainsSourceType.Add(SourceFlag.Exchange);
            }
            if (HasPermission(filteredPermissions, RMPermissionMasks.FSAdmin))
            {
                group.ContainsSourceType.Add(SourceFlag.FileSystem);
            }

            if (HasPermission(filteredPermissions, RMPermissionMasks.PhysicalAdmin))
            {
                group.ContainsSourceType.Add(SourceFlag.Physical);
                group.PhysicalRole = SubPermissionType.Admin;
            }
            else if (HasPermission(filteredPermissions, RMPermissionMasks.PhysicalEndUser))
            {
                group.ContainsSourceType.Add(SourceFlag.Physical);
                group.PhysicalRole = SubPermissionType.EndUser;
            }
            if (HasPermission(filteredPermissions, RMPermissionMasks.SPOnPremEnduser))
            {
                group.ContainsSourceType.Add(SourceFlag.SharePointOnPrem);
            }

            if (HasPermission(filteredPermissionsExtenstion, RMPermissionExtensionMasks.AzureFSAdmin) && HasPermission(group.PermissionExtensionMasks, RMPermissionExtensionMasks.AzureFSAdmin))
            {
                group.ContainsSourceType.Add(SourceFlag.AzureFileShare);
            }

            if (HasPermission(group.PermissionExtensionMasks, RMPermissionExtensionMasks.BoxAdmin) && HasPermission(filteredPermissionsExtenstion, RMPermissionExtensionMasks.BoxAdmin))
            {
                group.ContainsSourceType.Add(SourceFlag.Box);
            }

            if (HasPermission(group.PermissionExtensionMasks, RMPermissionExtensionMasks.GoogleAdmin) && HasPermission(filteredPermissionsExtenstion, RMPermissionExtensionMasks.GoogleAdmin))
            {
                group.ContainsSourceType.Add(SourceFlag.Google);
            }

            if (HasPermission(filteredPermissionsExtenstion, RMPermissionExtensionMasks.TeamsEndUser)
                || HasPermission(filteredSOPermissions, RMSOPermissionMasks.TeamsEndUser))
            {
                group.ContainsSourceType.Add(SourceFlag.Teams);
            }
        }

        private Task<List<T>> GetFilteredPermissionAsync<T>(long permissionMasks) where T : struct
        {
            var permissionList = new List<long> { permissionMasks }.CombinePermissions<T>().SplitPermission<T>();
            return TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionList);
        }

        private bool HasPermission<T>(long selfPermissionMasks, T checkedPermissionMasks)
        {
            var selfMasks = selfPermissionMasks.ToString().UnpackPermissionsFromString<T>();
            return ((dynamic)selfMasks & checkedPermissionMasks) == checkedPermissionMasks;
        }

        private bool HasPermission(List<long> permissionMasksList, RMPermissionExtensionMasks checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => HasPermission(o, checkedPermissionMasks));
        }

        private bool HasPermission(List<RMPermissionExtensionMasks> permissionMasksList, RMPermissionExtensionMasks checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => (o & checkedPermissionMasks) == checkedPermissionMasks);
        }

        private bool HasPermission(long selfPermissionMasks, RMPermissionMasks checkedPermissionMasks)
        {
            var selfMasks = selfPermissionMasks.ToString().UnpackPermissionsFromString();
            return (selfMasks & checkedPermissionMasks) == checkedPermissionMasks;
        }

        private bool HasPermission(List<long> permissionMasksList, RMPermissionMasks checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => HasPermission(o, checkedPermissionMasks));
        }

        private bool HasPermission<T>(List<long> permissionMasksList, T checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => HasPermission(o, checkedPermissionMasks));
        }

        private bool HasPermission<T>(List<T> permissionMasksList, T checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => (o & (dynamic)checkedPermissionMasks) == checkedPermissionMasks);
        }

        private bool HasSubPermission(long selfPermissionMasks, RMSubPermissionMasks checkedPermissionMasks)
        {
            var selfMasks = selfPermissionMasks.ToString().UnpackSubPermissionsFromString();
            return (selfMasks & checkedPermissionMasks) == checkedPermissionMasks;
        }

        private bool HasSubPermission(List<long> permissionMasksList, RMSubPermissionMasks checkedPermissionMasks)
        {
            return permissionMasksList.Any(o => HasSubPermission(o, checkedPermissionMasks));
        }

        private void CheckIsAllowModifiedGroup(int id)
        {
            if ((int)BuiltInGroupId.Admin == id)
            {
                throw new Exception($"[{id}]:it's not a legal group id .");
            }
        }

        private void CheckIsAllowDeleteGroup(int id)
        {
            if ((int)BuiltInGroupId.Admin == id || (int)BuiltInGroupId.EndUser == id || SecurityGroupDao.IsBuiltInReviewUserGroup(id) || SecurityGroupDao.IsBuiltInHoldManagerGroup(id))
            {
                throw new Exception($"[{id}]:it's not a legal group id .");
            }
        }

        private async System.Threading.Tasks.Task CheckSecurityGroupAsync(SecurityGroupDto group)
        {
            if (string.IsNullOrEmpty(group.Name.Trim()) || ExistInvalidGroupScopesInfo(group.DataSourceScopeInfo) || !(await CheckDataSourceLicenseAsync(group.DataSourceScopeInfo)))
            {
                throw new Exception("It's not legal group.");
            }
        }

        private bool ExistInvalidGroupScopesInfo(List<SecurityDataSourceScopeDto> groupScopesInfo)
        {
            if (groupScopesInfo == null || groupScopesInfo.Count == 0)
            {
                return true;
            }
            if (!groupScopesInfo.Any(o => GetValidDataSourceList().Contains(o.DataSourceType)))
            {
                return true;
            }
            if (!IsValidScopeInfo(groupScopesInfo, SourceFlag.SharePoint)
                || !IsValidScopeInfo(groupScopesInfo, SourceFlag.Exchange)
                || !IsValidScopeInfo(groupScopesInfo, SourceFlag.Physical)
                || !IsValidScopeInfo(groupScopesInfo, SourceFlag.OneDrive))
            {
                return true;
            }
            return false;
        }

        private List<SourceFlag> GetValidDataSourceList()
        {
            return new List<SourceFlag> {
                        SourceFlag.SharePoint,
                        SourceFlag.Exchange,
                        SourceFlag.Physical,
                        SourceFlag.FileSystem,
                        SourceFlag.SharePointOnPrem,
                        SourceFlag.OneDrive,
                        SourceFlag.AzureFileShare,
                        SourceFlag.Box,
            };
        }

        private bool IsValidScopeInfo(List<SecurityDataSourceScopeDto> groupScopesInfo, SourceFlag sourceType)
        {
            var isValidScope = true;
            var scopeInfo = groupScopesInfo.Where(o => o.DataSourceType == sourceType).FirstOrDefault();
            if (scopeInfo != null)
            {
                switch (sourceType)
                {
                    case SourceFlag.SharePoint:
                    case SourceFlag.Exchange:
                    case SourceFlag.OneDrive:
                        if (scopeInfo.ScopeIds == null || scopeInfo.ScopeIds.Count == 0)
                        {
                            isValidScope = false;
                        }
                        break;
                    case SourceFlag.Physical:
                        if (scopeInfo.SubPermission != SubPermissionType.Admin && scopeInfo.SubPermission != SubPermissionType.EndUser)
                        {
                            isValidScope = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            return isValidScope;
        }

        private async Task<bool> CheckDataSourceLicenseAsync(List<SecurityDataSourceScopeDto> groupScopesInfo)
        {
            var dto = SecurityGroupDao.GetUserScopePermissions(await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId));
            if (groupScopesInfo.Any(o => o.DataSourceType == SourceFlag.AzureFileShare))
            {
                var permissionExtensionList = dto.SecurityGroupPermissionExtensionMasks.CombinePermissions<RMPermissionExtensionMasks>().SplitPermission();
                var filteredPermissionExtensions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionExtensionList);
                if (!HasPermission(filteredPermissionExtensions, RMPermissionExtensionMasks.AzureFSAdmin))
                {
                    return false;
                }
            }

            if (groupScopesInfo.Any(o => o.DataSourceType == SourceFlag.Box))
            {
                var permissionExtensionList = dto.SecurityGroupPermissionExtensionMasks.CombinePermissions<RMPermissionExtensionMasks>().SplitPermission();
                var filteredPermissionExtensions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionExtensionList);
                if (!HasPermission(filteredPermissionExtensions, RMPermissionExtensionMasks.BoxAdmin) && permissionExtensionList.Contains(RMPermissionExtensionMasks.BoxAdmin))
                {
                    return false;
                }
            }

            if (groupScopesInfo.Any(o => o.DataSourceType == SourceFlag.Google))
            {
                var permissionGoogleList = dto.SecurityGroupPermissionExtensionMasks.CombinePermissions<RMPermissionExtensionMasks>().SplitPermission();
                var filteredGooglePermission = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionGoogleList);
                if (!HasPermission(filteredGooglePermission, RMPermissionExtensionMasks.GoogleAdmin) && permissionGoogleList.Contains(RMPermissionExtensionMasks.GoogleAdmin))
                {
                    return false;
                }
            }

            var permissionList = dto.SecurityGroupPermissionMasks.CombinePermissionsIntoString().SplitPermission();
            var filteredPermissions = await TenantDao.CalcPermissionsWithModuleAsync(TenantLocalValue.LogonGroupId, permissionList);
            if (groupScopesInfo.Any(o => o.DataSourceType == SourceFlag.FileSystem) && !HasPermission(filteredPermissions, RMPermissionMasks.FSAdmin))
            {
                return false;
            }

            if (groupScopesInfo.Any(o => o.DataSourceType == SourceFlag.SharePointOnPrem) && !HasPermission(filteredPermissions, RMPermissionMasks.SPOnPremEnduser))
            {
                return false;
            }
            return true;
        }
        public async Task<SecurityTermPermissionDto> GetSecurityTermObjInfoAsync(QuerySecurityTermObjDto dto)
        {
            SecurityTermPermissionDto result = null;
            try
            {
                if (dto.UserAndGroupIds == null || dto.UserAndGroupIds.Count == 0)
                {
                    dto.UserAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(dto.UserId);
                }
                result = SecurityGroupDao.GetSecurityTermObjInfo(dto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error while GetSecurityTermObjIds, message: {ex}");
            }
            return result;
        }

        public async Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid> termObjIds)
        {
            var hasPermission = false;
            try
            {
                if (termObjIds != null && termObjIds.Count > 0)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(userId);
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        Level = level,
                        UserAndGroupIds = userAndGroupIds
                    };
                    hasPermission = SecurityGroupDao.DoesUserHasPermisionToTerm(termObjIds, dto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while check termobj permission for user, termObjId:{string.Join(";", termObjIds)}, level: {level} message: {ex}");
            }
            return hasPermission;
        }

        public async Task<bool> DoesUserHasPermisionToTermAsync(string userId, SecurityTermLevel level, List<Guid> termObjIds, FilterTermObjOption filterOption)
        {
            var hasPermission = false;
            try
            {
                if (termObjIds != null && termObjIds.Count > 0)
                {
                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(userId);
                    QuerySecurityTermObjDto dto = new QuerySecurityTermObjDto
                    {
                        Level = level,
                        UserAndGroupIds = userAndGroupIds,
                        FilterByContentSource = filterOption.FilterByContentSource,
                        ExcludeBuiltIn = filterOption.ExcludeBuiltIn,
                        ContainerId = filterOption.ContainerId,
                        SourceFlag = filterOption.SourceFlag,
                        ForPhysicalView = filterOption.ForPhysicalView,
                    };
                    hasPermission = SecurityGroupDao.DoesUserHasPermisionToTerm(termObjIds, dto);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error while check termobj permission for user, termObjId:{string.Join(";", termObjIds)}, level: {level} message: {ex}");
            }
            return hasPermission;
        }

        public async Task<List<AOSUserDto>> SearchUsersByPermissionScopeAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<AOSUserDto>();
            }

            var searchKey = keyword.Trim().ToLower();
            var matchedUsers = await UserService.SearchUsersAsync(TenantLocalValue.LogonGroupId, searchKey);
            if (matchedUsers == null || matchedUsers.Count == 0)
            {
                return new List<AOSUserDto>();
            }

            var eligibleUsers = new List<AOSUserDto>();
            foreach (var user in matchedUsers)
            {
                if (string.IsNullOrEmpty(user.UserId))
                {
                    continue;
                }

                var permissions = await GetUserScopePermissionsAsync(user.UserId);
                if (HasManageHoldsPermission(permissions))
                {
                    eligibleUsers.Add(user);
                }
            }

            return eligibleUsers;
        }

        public  bool HasManageHoldsPermission(SecurityUserPermissionsDto permissions)
        {
            if (permissions == null)
            {
                return false;
            }

            if (permissions.IsAdmin || permissions.IsEnableManageHold || permissions.HasHoldManagerPermission)
            {
                return true;
            }

            return false;
        }
    }
}

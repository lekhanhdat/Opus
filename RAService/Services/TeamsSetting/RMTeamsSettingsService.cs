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
using System.Data.SqlTypes;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Teams;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Teams.RMTeamsColumn;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.TeamsSetting
{
    [Audit]
    public class RMTeamsSettingsService : BaseContentRepositorySettingsService, IRMTeamsSettingsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTeamsSettingsService));

        #region Services
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMSettingJobDao RMSettingJobDao = PlatformWindsorManager.GetService<IRMSettingJobDao>();
        private IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMChangeClassificationDao RMChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private ILicenseHelperService licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private ITeamsChannelConflictSettingDao TeamsChannelConflictSettingDao => PlatformWindsorManager.GetService<ITeamsChannelConflictSettingDao>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private IRMArchiverSettingDao RMArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();


        #endregion

        public async Task LoadTeamsSettingIconAsync(List<RMSPSampleTreeNode> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMSPSampleTreeNode groupNode = nodes[0];
                    if (groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        groupNode = GetGroupNode(groupNode);

                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.SPObjectId);
                        }

                        var gsSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.TeamsDisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMTeamsSetting>();
                        var settings = TeamsSettingDao.LoadTeamsSettings(groupId, true).OrderBy(item => item.Id);
                        foreach (var setting in settings)
                        {
                            var key = setting.ScopeId.ToString() + setting.TeamsId.ToString() + setting.SiteId.ToString();
                            if (!allSettings.ContainsKey(key))
                            {
                                allSettings.Add(key, setting);
                            }
                        }
                        foreach (var node in nodes)
                        {
                            ArgumentCheck.NotNull(node, nameof(node));
                            var siteNode = node;
                            while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                            {
                                siteNode = siteNode.Parent;
                            }
                            var siteId = siteNode != null ? siteNode.SPObjectId : Guid.Empty.ToString();

                            var teamsNode = node;
                            while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                            {
                                teamsNode = teamsNode.Parent;
                            }

                            var teamsId = teamsNode != null ? teamsNode.TeamsId : Guid.Empty.ToString();

                            RMTeamsSetting csSetting = null;
                            var settingKey = node.SPObjectId + teamsId + siteId;
                            if (allSettings.TryGetValue(settingKey, out csSetting))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = ScheduleService.GetProfileId(node);
                            if (allSchedulesProfilesId.Contains(profileId))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            if (gsSetting != null)
                            {
                                node.IconStatus = IconStatus.Inhert;
                                continue;
                            }
                            node.IconStatus = IconStatus.NoSet;
                        }
                    }
                    else
                    {
                        foreach (var selfGroupNode in nodes)
                        {
                            var selfGSSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(selfGroupNode.SPObjectId), Guid.Empty, Guid.Empty);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }

                            if (selfGroupNode.Children != null && selfGroupNode.Children.Any())
                            {
                                await LoadTeamsSettingIconAsync(selfGroupNode.Children);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load TeamsSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        public List<string> GetDesignLists()
        {
            bool isCSDTenant = TenantService.IsCSDTenant();
            return WebUtil.GetDesignLists(isCSDTenant);
        }

        public async Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode sNode)
        {
            var configNode = new RMSPTreeNode();
            configNode.IconStatus = IconStatus.NoSet;
            #region copy node properties
            configNode.Id = sNode.Id;
            configNode.FarmId = sNode.FarmId;
            configNode.FarmName = sNode.FarmName;
            configNode.Name = sNode.Name;
            configNode.Title = sNode.Title;
            configNode.FullPath = sNode.FullPath;
            configNode.Level = sNode.Level;
            configNode.NodeType = sNode.NodeType;
            configNode.SPType = sNode.SPType;
            configNode.SPObjectId = sNode.SPObjectId;
            configNode.SPVersion = sNode.SPVersion;
            configNode.Expanded = sNode.Expanded;
            configNode.ChildrenCount = sNode.ChildrenCount;
            configNode.CheckNumber = sNode.CheckNumber;
            configNode.Hidden = sNode.Hidden;
            configNode.TemplateId = sNode.TemplateId;
            configNode.BposInfo = sNode.BposInfo;
            configNode.TeamsId = sNode.TeamsId;
            configNode.Type = ContentSourceType.Teams;
            #endregion

            try
            {
                RMSPSampleTreeNode groupNode = GetGroupNode(sNode);
                if (groupNode == null)
                {
                    return configNode;
                }
                //var groupNode = GetGroupNode(configNode);
                Guid groupId = Guid.Empty;
                bool ownSetting = true;
                bool folderDisable = false;
                string GlobalColumnName = string.Empty;
                string GlobalColumnNameDesc = string.Empty;
                if (groupNode != null && !string.IsNullOrEmpty(groupNode.SPObjectId))
                {
                    groupId = new Guid(groupNode.SPObjectId);
                }
                var teamsNode = sNode;
                while (teamsNode != null && teamsNode.Level != (int)NodeLevel.Office365GroupEntire)
                {
                    teamsNode = teamsNode.Parent;
                }
                var teamsId = Guid.Empty;
                if (teamsNode != null && !string.IsNullOrEmpty(teamsNode.TeamsId))
                {
                    teamsId = new Guid(teamsNode.TeamsId);
                }

                var GSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    GlobalColumnName = GSetting.ColumnName;
                    GlobalColumnNameDesc = GSetting.Description;
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                    var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }
                    configNode.ColumnName = GlobalColumnName;
                    configNode.ColumnRequired = GSetting.ColumnRequired == null ? true : (bool)GSetting.ColumnRequired;
                    configNode.ColumnHidden = GSetting.ColumnHidden == null ? false : (bool)GSetting.ColumnHidden;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ExistColumnName = GSetting.ExistColumnName;
                    configNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                    configNode.SetDocLevelTermForExistColumn = GSetting.SetDocLevelTermForExistColumn;
                    configNode.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.TermIdOfContainer = GSetting.TermIdOfContainer;
                    configNode.ContainerTermFullPath = GSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = GSetting.isEnableClassification;
                    configNode.DescriptionOfContainer = GSetting.DescriptionOfContainer;
                    configNode.IsInheritParentTerm = GSetting.IsInheritParentTerm;
                    configNode.TermSetId = GSetting.TermSetId;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.TermSetName = GSetting.TermSetName;
                    configNode.TermId = GSetting.TermId;
                    configNode.TermName = GSetting.TermName;
                    configNode.DefaultTermId = GSetting.DefaultTermId;
                    configNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                    configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";

                    //configNode.DefaultTermNameFullPath = termDefaultValue == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                    configNode.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                    configNode.IsShowUniqueId = GSetting.IsShowUniqueId == null ? true : (bool)GSetting.IsShowUniqueId;
                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    configNode.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = GSetting.ApplyExistType;

                    if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                    }
                    configNode.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.Teams);
                    configNode.SiteGroupId = GSetting.TeamsGroupId;
                    configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                    if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                    //configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                    configNode.IncludeDeclaredRecords = GSetting.IncludeDeclaredRecords;
                    configNode.ApplyTermIncludeFolder = GSetting.IsApplyTermIncludeFolder();
                    configNode.AlwaysScanAllExistDocuments = GSetting.AlwaysScanAllExistDocuments;
                    configNode.IsKeepSharePointDefaultValue = GSetting.IsKeepSharePointDefaultValue;
                    configNode.SetTermForEmptyDefaultValue = GSetting.SetTermForEmptyDefaultValue;
                    if (sNode.Level == (int)NodeLevel.Office365GroupEntire || sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Folder)
                    {
                        if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        else
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                    }
                    configNode.isEnableClassification = GSetting.isEnableClassification;
                    configNode.IsSyncData = GSetting.IsSyncData;
                    if (!string.IsNullOrEmpty(GSetting.NodeInfo))
                    {
                        RMSPTreeNode GSettingTeamTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(GSetting.NodeInfo);
                        configNode.SupportLockedSite = GSettingTeamTreeNode.SupportLockedSite;
                        configNode.EnableLifecycleManagementForSharePointLists = GSettingTeamTreeNode.EnableLifecycleManagementForSharePointLists;
                    }
                    configNode.ApprovalType = (int)GSetting.ApprovalType;
                    configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;

                    configNode.AITermUseType = GSetting.AITermUseType;
                    configNode.AIApprovalType = (int)GSetting.AIApprovalType;
                    configNode.AISendEMail = GSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.AITeams);
                    configNode.AIThenIsDefaultTermMethod = GSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = GSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = GSetting.AIThenDefaultTermName;

                    //SetDisposeJob(configNode, GSetting.DisposalJobId1);
                    //SetCollectionJob(configNode, GSetting.CollectionJobId1);
                }
                RMSPSampleTreeNode siteNode = sNode;
                while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                {
                    siteNode = siteNode.Parent;
                }

                Guid siteId = Guid.Empty;
                if (siteNode != null)
                {
                    siteId = new Guid(siteNode.SPObjectId);
                }
                var spSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(sNode.SPObjectId), teamsId, siteId, true);//TODO 暂时不考虑 only mark physical
                if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                {
                    var pNode = LoadFolderParentSeting(sNode, teamsId, siteId);
                    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                    {
                        if (spSetting != null)
                        {
                            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        folderDisable = true;
                    }
                }

                if (spSetting == null)
                {
                    if (sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.Folder)
                    {
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, teamsId, siteId);
                        if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                        {
                            if (spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            {
                                spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                        }
                        configNode.IsCustomSetting = false;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                    }
                }

                if (spSetting != null)
                {
                    var termScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                    var defaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(spSetting.TermIdOfContainer);
                    RMTermSet termSet = null;
                    if (spSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(spSetting.TermSetId);
                    }
                    RMSPTreeNode rMSPTree = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(spSetting.NodeInfo);
                    configNode.SupportLockedSite = rMSPTree.SupportLockedSite;
                    configNode.EnableLifecycleManagementForSharePointLists = rMSPTree.EnableLifecycleManagementForSharePointLists;
                    configNode.ColumnName = GlobalColumnName;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ColumnRequired = spSetting.ColumnRequired == null ? true : (bool)spSetting.ColumnRequired;
                    configNode.ColumnHidden = spSetting.ColumnHidden == null ? false : (bool)spSetting.ColumnHidden;
                    configNode.DefaultTermId = spSetting.DefaultTermId;
                    configNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                    configNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                    configNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                    //configNode.DefaultTermNameFullPath = defaultTerm == null ? spSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(spSetting.DefaultTermId);
                    configNode.TermId = spSetting.TermId;
                    configNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                    //configNode.TermNameFullPath = termScope == null ? spSetting.TermName : TermDao.GetTermFullPathByTermId(spSetting.TermId);
                    configNode.TermSetId = spSetting.TermSetId;
                    configNode.TermSetName = spSetting.TermSetName;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                    configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                    configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                    configNode.DescriptionOfContainer = spSetting.DescriptionOfContainer;
                    configNode.IsInheritParentTerm = spSetting.IsInheritParentTerm;
                    configNode.TermIdOfContainer = spSetting.TermIdOfContainer;
                    configNode.TermNameOfContainer = containerTerm == null ? spSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.ContainerTermFullPath = spSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = spSetting.isEnableClassification;
                    configNode.IsEnableHoldPhyical = spSetting.IsEnableHoldPhyical;
                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                    configNode.isFailedConfigClassification = spSetting.isFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = spSetting.isFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    //configNode.ExistColumnName = spSetting.ExistColumnName;
                    //configNode.IsUsingExistColumnName = spSetting.IsUsingExistColumnName;
                    configNode.IsDisplyaTermPath = spSetting.IsDisplyaTermPath;
                    //configNode.IsShowUniqueId = spSetting.IsShowUniqueId;
                    configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = spSetting.ApplyExistType;
                    if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    }

                    configNode.EnableRelatedRecords = spSetting.EnableRelatedRecords;
                    //configNode.RecordOwner = GetSettingRecordOnwers(spSetting.Id, SourceType.SharePoint);
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.Teams);
                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                    configNode.IsSyncData = spSetting.IsSyncData;
                    //configNode.ProfileId = spSetting.IdPath;
                    configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                    if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                    configNode.IncludeDeclaredRecords = spSetting.IncludeDeclaredRecords;
                    configNode.ApprovalType = (int)spSetting.ApprovalType;
                    configNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;
                    configNode.ApplyTermIncludeFolder = spSetting.IsApplyTermIncludeFolder();
                    configNode.AlwaysScanAllExistDocuments = spSetting.AlwaysScanAllExistDocuments;

                    configNode.AITermUseType = spSetting.AITermUseType;
                    configNode.AIApprovalType = (int)spSetting.AIApprovalType;
                    configNode.AISendEMail = spSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.AITeams);
                    configNode.AIThenIsDefaultTermMethod = spSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = spSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = spSetting.AIThenDefaultTermName;
                    //SetDisposeJob(configNode, spSetting.DisposalJobId1);
                    //if (sNode.Level == (int)NodeLevel.WebApplication || sNode.Level == (int)NodeLevel.SiteCollection)
                    //{
                    //    SetCollectionJob(configNode, spSetting.CollectionJobId1);
                    //}
                    //else
                    //{
                    //    var tempSetting = SharePointSettingDao.LoadSharePointSetting(siteId, siteId, true);//TODO 暂时不考虑 only mark physical
                    //    if (tempSetting != null)
                    //    {
                    //        SetCollectionJob(configNode, tempSetting.CollectionJobId1);
                    //    }
                    //}
                }

                if (string.IsNullOrEmpty(configNode.ColumnName))
                {
                    configNode.ColumnRequired = true;
                }
                //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                //{
                //    configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                //    configNode.DefaultTermId = Guid.Empty;
                //    configNode.DefaultTermName = string.Empty;
                //    configNode.TermId = Guid.Empty;
                //    configNode.TermName = string.Empty;
                //    configNode.AutoClassificationRules = null;
                //}

                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.TeamsDisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                    configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                    //{
                    //    configNode.DisposeScheduleInfo.Id = "1";
                    //}
                }
                else
                {
                    var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.TeamsDisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                        configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(ancestryDisposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                        configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        configNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                        configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    }
                    else
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load TeamsSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }

        public RMTeamsSetting LoadSampleNodeParentSeting(RMSPSampleTreeNode node, Guid teamsId, Guid siteId)
        {
            RMTeamsSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.Office365GroupEntire)
            {
                siteId = Guid.Empty; // clear siteId for teams node
            }

            if (node.Level == (int)NodeLevel.Office365GroupEntire || node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), teamsId, siteId, true);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadSampleNodeParentSeting(node.Parent, teamsId, siteId);
            }

            return SPSetting;
        }

        private RMSPSampleTreeNode GetGroupNode(RMSPSampleTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.WebApplication)
            {
                HydrateGroupParent(node);
                node = node.Parent;
            }

            return node;
        }

        private void HydrateGroupParent(RMSPSampleTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            if ((node.Level != (int)NodeLevel.SiteCollection && node.Level != (int)NodeLevel.Office365GroupEntire)
                || (node.Parent != null && node.Parent.Level != -2))
            {
                return;
            }

            string parentId = null;
            if (node.Level == (int)NodeLevel.SiteCollection)
            {
                var siteCollection = RemoteNodeService?.GetRemoteSiteCollectionById(node.Id);
                parentId = siteCollection?.parentId;
            }
            else
            {
                var teamsNode = RemoteNodeService?.GetTeamsNodeByTeamsId(node.Id);
                parentId = teamsNode?.ParentId;
            }

            if (string.IsNullOrEmpty(parentId))
            {
                return;
            }

            var parentWebApp = RemoteNodeService.GetWebApplicationById(parentId);
            node.ParentId = parentId;
            node.Parent = new RMSPSampleTreeNode
            {
                Id = parentId,
                SPObjectId = parentId,
                Level = (int)NodeLevel.WebApplication,
                Name = parentWebApp?.url,
                DisplayName = parentWebApp?.url,
                FullPath = parentWebApp?.url
            };
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Inherit Parent Settings");
                var siteCollectionNode = node.GetSiteCollectionNode();
                var teamsNode = node.GetTeamsNode();
                var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                await TeamsSettingDao.DeleteTeamsSettingAsync(new Guid(node.SPObjectId), teamsId, siteId);
                await CleanParentNodeSettingAsync(node);
                //Update the parent node setting to inherit settings. to do next.
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public bool CheckParentNodeDisable(RMSPTreeNode nodeSetting, string teamsId, string siteId, bool isCheckSelfNode = true)
        {
            var isDisableRecordsManagement = false;
            if (nodeSetting.DisposeScheduleInfo != null && nodeSetting.DisposeScheduleInfo.JobCategory == ScheduleType.TeamsArchiveJobSchedule)
            {
                isDisableRecordsManagement = false;
            }
            else
            {
                try
                {
                    Expression<Func<RMTeamsSetting, bool>> whereLambda = GetFilterLambda(nodeSetting, teamsId, siteId, isCheckSelfNode);
                    if (TeamsSettingDao.GetParentNode(whereLambda) != null)
                    {
                        isDisableRecordsManagement = true;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Check Parent Node Records Management error:{0}", ex.ToString());
                }
            }
            return isDisableRecordsManagement;
        }

        private Expression<Func<RMTeamsSetting, bool>> GetFilterLambda(RMSPTreeNode settingNode, string teamsId, string siteId, bool isCheckSelfNode = true)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMTeamsSetting), "c");
            List<Expression> nodeIdExpressionList = new List<Expression>();
            var scopeIds = GetParentScopeId(settingNode, isCheckSelfNode);

            if (scopeIds != null && scopeIds.Count > 0)
            {
                foreach (var scopeId in scopeIds)
                {
                    nodeIdExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "ScopeId", scopeId));
                }
            }
            allExpressionList.Add(nodeIdExpressionList.Aggregate(Expression.OrElse));

            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "EnableRecordManagement", (int)EnableRecordManagementSetting.Disable));

            if (string.IsNullOrEmpty(teamsId)) teamsId = Guid.Empty.ToString();
            if (string.IsNullOrEmpty(siteId)) siteId = Guid.Empty.ToString();

            Expression teamsEmptyCondition = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "TeamsId", Guid.Empty);
            Expression teamsEqualsCondition = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "TeamsId", new Guid(teamsId));
            Expression siteEmptyCondition = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "SiteId", Guid.Empty);
            Expression siteEqualsCondition = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "SiteId", new Guid(teamsId));

            Expression combinedSiteCondition = Expression.OrElse(siteEmptyCondition, siteEqualsCondition);
            Expression combinedTeamsSiteCondition = Expression.OrElse(teamsEmptyCondition, Expression.AndAlso(teamsEqualsCondition, combinedSiteCondition));

            allExpressionList.Add(combinedTeamsSiteCondition);

            var groupNode = settingNode.GetGroupNode();
            if (groupNode != null)
            {
                allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMTeamsSetting), param, "TeamsGroupId", new Guid(groupNode.SPObjectId)));
            }

            try
            {
                queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                logger.Info("allExpressionList:{0}", queryExpr.ToString());
                return Expression.Lambda<Func<RMTeamsSetting, bool>>(queryExpr, param);
            }
            catch (Exception ex)
            {
                logger.Error("allExpressionList error:{0}", ex.ToString());
                return null;
            }
        }

        private List<Guid> GetParentScopeId(RMSPTreeNode settingNode, bool isCheckSelfNode)
        {
            List<Guid> scopeIds = new List<Guid>();
            if (isCheckSelfNode)
            {
                if (settingNode.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    scopeIds.Add(new Guid(settingNode.Parent.TeamsId));
                }
                else
                {
                    scopeIds.Add(new Guid(settingNode.SPObjectId));
                }
            }
            while (settingNode.Parent != null && settingNode.Parent.SPObjectId != null)
            {
                if (settingNode.Parent.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    scopeIds.Add(new Guid(settingNode.Parent.TeamsId));
                }
                else
                {
                    scopeIds.Add(new Guid(settingNode.Parent.SPObjectId));
                }
                settingNode = settingNode.Parent;
            }
            return scopeIds;
        }

        public async System.Threading.Tasks.Task CleanParentNodeSettingAsync(RMSPTreeNode node)
        {
            do
            {
                if (await TeamsSettingDao.CleanSettingJobTimeAsync(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }

        public RMTeamsSetting LoadFolderParentSeting(RMSPSampleTreeNode node, Guid teamsId, Guid siteId)
        {
            RMTeamsSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), Guid.Empty, Guid.Empty, true);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.Office365GroupEntire || node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), teamsId, siteId, true);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, teamsId, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }
        public RMTeamsSetting LoadFolderParentSeting(RMSPTreeNode node, Guid teamsId, Guid siteId)
        {
            RMTeamsSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), Guid.Empty, Guid.Empty, true);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.Office365GroupEntire || node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(node.SPObjectId), teamsId, siteId, true);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, teamsId, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Set Teams Column Setting");
                if (groupNode.ColumnHidden && groupNode.ColumnRequired)
                {
                    logger.Warn($"ColumnHidden and ColumnRequired = true");
                    throw new Exception("invalid colum setting");
                }
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    UniqueIdSetting curUniqueIdSetting = UniqueIdSettingService.LoadingTeamsUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName)
                    {
                        TeamsSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName, groupNode.Description, groupNode.ColumnRequired, groupNode.ColumnHidden);
                        await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                        TeamsSettingDao.FlagCustomSettingNewColumn(groupNode.SiteGroupId);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                logger.Info("Begin save global using column name settings {0}:{1}", groupNode.FullPath, groupNode.ExistColumnName);
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    var curUniqueIdSetting = UniqueIdSettingService.LoadingTeamsUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                {
                    await TeamsSettingDao.AddOrUpdateGlobalSettingUsingExistColumnAsync(groupNode, true);
                    logger.Info("using column name add or update global serring succes,group node:{0}", groupNode.Name);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Warn("using column name add or update global serring occur error,group node:{0},info:{1}", groupNode.Name, e.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GeneralSetting4Teams, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGeneralSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage enableResult = await AddEnableColumnSettingAsync(settingNode);
            RAReturnMessage isSyncResult = new RAReturnMessage();
            if (settingNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                isSyncResult = await AddIsSyncSettingAsync(settingNode);
            }
            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isSyncResult.MessageType == RAMessageType.Failed)
            {
                result = isSyncResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }
            return result;
        }

        public async Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    TeamsSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired, settingNode.ColumnHidden);
                    await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                    TeamsSettingDao.FlagCustomSettingNewColumn(settingNode.SiteGroupId);
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = settingNode.GetSiteCollectionNode();
                    var teamsNode = settingNode.GetTeamsNode();
                    if (!CheckParentNodeDisable(settingNode, teamsNode?.TeamsId, siteCollectionNode?.SPObjectId, false))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.Teams);
                        var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                        var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                        await TeamsSettingDao.AddOrUpdateCustomSettingAsync(settingNode, teamsId, siteId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                TeamsSettingDao.RemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        private void SetPropertiesByNodeLevel(RMSPTreeNode settingNode, RMSPTreeNode siteCollectionNode)
        {
            if (settingNode.Level == (int)NodeLevel.Folder)
            {
                settingNode.FolderId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id
                                                                                   //RECO-1881
                settingNode.isEnableClassification = false;
                settingNode.DescriptionOfContainer = null;
                settingNode.IsInheritParentTerm = false;
                settingNode.TermIdOfContainer = Guid.Empty;
                settingNode.TermNameOfContainer = null;

                settingNode.FullPath = WebUtil.MakeFullUrl(siteCollectionNode.FullPath, settingNode.FullPath);
            }
            else if (settingNode.Level == (int)NodeLevel.List || settingNode.Level == (int)NodeLevel.Library)
            {
                settingNode.ListId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(settingNode.Parent.Parent.SPObjectId);//set Web Id
            }
            else if (settingNode.Level == (int)NodeLevel.Site)
            {
                settingNode.WebId = new Guid(settingNode.SPObjectId);
            }

            var groupNode = GetGroupNode(settingNode);
            Guid groupId = Guid.Empty;
            if (groupNode != null)
            {
                groupId = new Guid(groupNode.SPObjectId);
                settingNode.SiteGroupId = groupId;
            }
            var GSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
            if (GSetting != null)
            {
                settingNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
            }
        }

        public async Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                    {
                        TeamsSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired, settingNode.ColumnHidden);
                        await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                        TeamsSettingDao.FlagCustomSettingNewColumn(settingNode.SiteGroupId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = settingNode.GetSiteCollectionNode();
                    var teamsNode = settingNode.GetTeamsNode();
                    if (!CheckParentNodeDisable(settingNode, teamsNode?.TeamsId, siteCollectionNode?.SPObjectId))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.Teams);
                        var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                        var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                        await TeamsSettingDao.AddOrUpdateCustomSettingAsync(settingNode, teamsId, siteId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                //TeamsSettingDao.RemoveDescendantsSetting(settingNode);
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        public RMSPTreeNode GetListNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }
        public RMSPTreeNode GetWebNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.Site)
            {
                node = node.Parent;
            }
            return node;
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsConLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Container Teams Setting");
                var settingNode = containerNode;
                if (containerNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                    {
                        await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(containerNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    logger.Info("Set Container Teams Setting, current node save term as group : {0}", containerNode.FullPath);
                    var siteCollectionNode = settingNode.GetSiteCollectionNode();
                    var teamsNode = settingNode.GetTeamsNode();

                    if (!CheckParentNodeDisable(settingNode, teamsNode?.TeamsId, siteCollectionNode?.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                        var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                        await TeamsSettingDao.AddOrUpdateCustomSettingAsync(settingNode, teamsId, siteId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                bool HasRecordOwners(RMSPTreeNode node) => node.RecordOwner != null && node.RecordOwner.Count > 0;
                if ((node.ApprovalType == (int)ApprovalType.None && (!string.IsNullOrEmpty(node.WorkflowReferenceId) || HasRecordOwners(node)))
                    || (node.ApprovalType == (int)ApprovalType.ApprovalProcess && (string.IsNullOrEmpty(node.WorkflowReferenceId) || HasRecordOwners(node)))
                    || (node.ApprovalType == (int)ApprovalType.RecordOwners && (!string.IsNullOrEmpty(node.WorkflowReferenceId) || !HasRecordOwners(node)))
                    || (node.ApprovalType == (int)ApprovalType.AutoApproval && (!string.IsNullOrEmpty(node.WorkflowReferenceId) || HasRecordOwners(node))))
                {
                    logger.Warn($"invalid approval setting. ApprovalType: {(ApprovalType)node.ApprovalType}, WorkflowReferenceId: {node.WorkflowReferenceId}, RecordOwner count: {node.RecordOwner?.Count}");
                    throw new Exception("invalid approval setting");
                }
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Container Teams Setting");
                var settingNode = node;
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                    {
                        await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(node);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    logger.Info("Set Container Teams Setting, current node save term as group : {0}", node.FullPath);
                    var siteCollectionNode = settingNode.GetSiteCollectionNode();
                    var teamsNode = settingNode.GetTeamsNode();

                    if (!CheckParentNodeDisable(settingNode, teamsNode?.TeamsId, siteCollectionNode?.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                        var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                        await TeamsSettingDao.AddOrUpdateCustomSettingAsync(settingNode, teamsId, siteId);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Global Teams Setting");
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString(), Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName || (groupNode.IsUsingExistColumnName && groupNode.SetDocLevelTermForExistColumn))
                    {
                        AddFilterCretiaProperty(groupNode.AutoClassificationRules, SourceFlag.Teams);
                        //SharePointSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName);
                        await TeamsSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditTeamsDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode customNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                logger.Info("Set Custom Teams Setting");
                var settingNode = customNode;
                var siteCollectionNode = settingNode.GetSiteCollectionNode();
                var teamsNode = settingNode.GetTeamsNode();

                if (!CheckParentNodeDisable(settingNode, teamsNode?.TeamsId, siteCollectionNode?.SPObjectId))
                {
                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.Teams);
                    var teamsId = teamsNode != null ? new Guid(teamsNode.TeamsId) : Guid.Empty;
                    var siteId = siteCollectionNode != null ? new Guid(siteCollectionNode.SPObjectId) : Guid.Empty;
                    await TeamsSettingDao.AddOrUpdateCustomSettingAsync(settingNode, teamsId, siteId);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }

                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public bool CheckRunningTeamsSettingJob()
        {
            List<string> runningJobs = RMJobMonitorService.GetRunningTeamsSettingJob();
            return runningJobs.Count > 0;
        }

        public RAReturnMessage ApplySettingsOnSelectedNode(RMSPTreeNode node)
        {
            logger.Debug("Start Teams Apply Setting on selected node, path:{0}", node.FullPath);
            string id = string.Empty;
            RAReturnMessage msg = new();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ApplyTeamsSettings,
                    JobRunByUser = loginName,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    Parameters = string.Format("{0},{1},{2},{3},{4},{5}", false, Convert.ToInt32(RunApplySettingMethod.SelectedNode), GetTreeNodeScopeId(node), GetTreeNodeTeamsId(node), GetTreeNodeSiteId(node), node.FullPath)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while Apply Teams Settings On Selected Node,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.TeamsApplySetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string teamsId = null, string siteId = null, string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            string jobId = string.Empty;
            List<string> runningJobs = RMJobMonitorService.GetRunningTeamsSettingJob();

            try
            {
                if (runningJobs.Count == 0)
                {
                    jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.ApplyTeamsSettings, runJobMethod, scopeId, teamsId, siteId, fullPath, jobPriority);
                }
                else
                {
                    //TO DO for skipped jobs, how to set container id?
                    var settings = GetTeamsSettings(jobRunBy, runJobMethod, scopeId, teamsId, siteId);
                    if (settings.IsNullOrEmpty())
                    {
                        logger.Warn("No teams setting node found.");
                        throw new Exception("No teams setting node found.");
                    }
                    bool hasAvailableNode = false;
                    foreach (var setting in settings)
                    {
                        RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                        if (node == null)
                        {
                            logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                            continue;
                        }
                        var containerId = GetTeamsContainerId(node);
                        var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                        if (!(await IsTeamsAdminAsync(account.UserId)))
                        {
                            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                                continue;
                            }
                        }
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, containerId, scopeId, fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                        hasAvailableNode = true;
                        break;
                    }
                    if (!hasAvailableNode)
                    {
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, siteId, scopeId, fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SP_NoAvailableNodeError");
                        logger.Warn($"Has no available node for current user. JobId:{jobId}");
                    }
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, siteId, scopeId, fullPath, jobPriority);
                }
                if (e.Message == I18NEntity.GetString("RM_SP_NoAvailableSettingError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_NoAvailableSettingError");
                }
                else if (e.Message == I18NEntity.GetString("RM_SP_NoInhertSiteError"))
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SP_NoInhertSiteError");
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
                }


                logger.Error("real run apply teams setting job error: {0}", e.ToString());
            }

            return jobId;
        }

        private async Task<string> StartApplySettingJobAsync(JobRunBy runBy, string jobRunByUser, JobType jobType, RunApplySettingMethod runJobMethod, string scopeId = null, string teamsId = null, string siteId = null, string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            List<RMTeamsSetting> allSettings = new List<RMTeamsSetting>();
            using (var performance = new PerformanceScope("SPOApplySetting.GetSPSettings"))
            {
                allSettings = GetTeamsSettings(runBy, runJobMethod, scopeId, teamsId, siteId);
            }
            string jobId = string.Empty;

            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No Teams setting node found.");
                throw new Exception(I18NEntity.GetString("RM_SP_NoAvailableSettingError"));
            }
            Dictionary<Guid, RMTeamsSetting> gruopSetingMap = new Dictionary<Guid, RMTeamsSetting>();
            Dictionary<Guid, int> nodeSettingMap = new Dictionary<Guid, int>();
            List<RMTeamsSetting> excludeTeamsNodes = new List<RMTeamsSetting>();
            using (var performance = new PerformanceScope("SPOApplySetting.LoadExcludeSiteCollectionSetting"))
            {
                excludeTeamsNodes = TeamsSettingDao.LoadExcludeTeamsSetting();
            }
            List<Guid> excludeTeamsIds = new List<Guid>();
            List<ValidateNodeInfo> siteStatusCache = new List<ValidateNodeInfo>();
            foreach (var setting in excludeTeamsNodes)
            {
                if (setting.TeamsId != Guid.Empty)
                {
                    if (!ValidateTeamsAvailability(siteStatusCache, setting.TeamsId, setting.TeamsGroupId))
                    {
                        continue;
                    }
                }
                excludeTeamsIds.Add(setting.ScopeId);
            }
            Dictionary<Guid, int> applyExistScopes = new Dictionary<Guid, int>();

            //List<SPTreeNodeDto> subJobNodes = new List<SPTreeNodeDto>();
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            Dictionary<Guid, List<RMSPTreeNode>> settingGroup = new Dictionary<Guid, List<RMSPTreeNode>>();
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            Dictionary<string, string> emptyContainers = new Dictionary<string, string>();
            foreach (RMTeamsSetting setting in allSettings)
            {
                if (!ValidateGroupAvailability(siteStatusCache, setting.TeamsGroupId))
                {
                    await TeamsSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.TeamsGroupId, setting.ScopeId, false, false);
                    continue;
                }
                using (var getRemoteSite = new PerformanceScope("GetRemote", $"GetRemoteSite{setting.SiteId}"))
                {
                    if (setting.SiteId != Guid.Empty)
                    {
                        if (!ValidateTeamsAvailability(siteStatusCache, setting.TeamsId, setting.TeamsGroupId))
                        {
                            await TeamsSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.TeamsId, setting.ScopeId, false, false);
                            continue;
                        }
                    }
                }
                RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                if (node == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                    continue;
                }
                //will use common method later
                var containerId = GetTeamsContainerId(node);
                var isAdmin = await IsTeamsAdminAsync(account.UserId);
                if (!isAdmin)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                        continue;
                    }
                }
                List<RMSPTreeNode> nodes = new List<RMSPTreeNode>();
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    using (var initWebApp = new PerformanceScope("InitWebAppSettings", $"InitWebAppSettings{node.Name}"))
                    {
                        List<RMSPTreeNode> teams = await RMTeamsTreeService.BrowseAsync(node);
                        var totalSiteCount = teams.Count;
                        var hasCustomSiteCount = 0;

                        logger.Info("Group:{0} site collection count is {1}", node.Name, teams.Count);
                        if (teams.Count > 0)
                        {
                            foreach (RMSPTreeNode teamsNode in teams)
                            {
                                Guid teamsNodeId = Guid.Empty;
                                if(!Guid.TryParse(teamsNode.SPObjectId, out teamsNodeId))
                                {
                                    logger.Warn($"Can not convert teams id to Guid: {teamsNode?.FullPath}");
                                    continue;
                                }
                                if (excludeTeamsIds.Contains(teamsNodeId))
                                {
                                    logger.Info("Exclude SiteId {0}", teamsNode.SPObjectId);
                                    hasCustomSiteCount++;
                                }
                                else
                                {
                                    teamsNode.EnableLifecycleManagementForSharePointLists = node.EnableLifecycleManagementForSharePointLists;
                                    nodes.Add(teamsNode);
                                }
                                if (!gruopSetingMap.ContainsKey(new Guid(node.Id)))
                                {
                                    gruopSetingMap.Add(new Guid(node.Id), setting);
                                }
                            }
                        }
                        else
                        {
                            if (!emptyContainers.ContainsKey(containerId))
                            {
                                emptyContainers.Add(containerId, GetSPContainerName(node));
                            }
                        }
                        if (totalSiteCount == hasCustomSiteCount)
                        {
                            //update group node setting
                            //TeamsSettingDao.SetSettingJobTime(new Guid(node.Id), false, false);
                            await TeamsSettingDao.SetSettingJobTimeWithGroupIdAsync(setting.TeamsGroupId, setting.ScopeId, false, false);
                        }
                    }
                }
                else
                {
                    node.TeamsId = setting.TeamsId.ToString();
                    nodes.Add(node);
                }
                var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                foreach (var n in nodes)
                {
                    n.PredictionModeType = isZeroShotMode ? PredictionModeType.ZeroShot : PredictionModeType.MLTraining;
                }
                if (nodes.Count > 0)
                {
                    if (settingGroup.ContainsKey(setting.TeamsGroupId))
                    {
                        settingGroup[setting.TeamsGroupId].AddRange(nodes);
                    }
                    else
                    {
                        settingGroup.Add(setting.TeamsGroupId, nodes);
                    }
                }
            }
            if (settingGroup.Count > 0)
            {
                foreach (var group in settingGroup)
                {
                    jobId = CreateApplySettingJob(runBy, jobRunByUser, group.Key.ToString(), scopeId, fullPath, jobPriority);
                    SeparateSubJobForApplySetting(group.Value, gruopSetingMap, jobId, runBy, jobType);

                    #region Store job settings to db.
                    var settingsPerContainer = allSettings.Where(s => s.TeamsGroupId == group.Key).ToList();
                    logger.Info("Begin store job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, group.Key, settingsPerContainer.Count);
                    var isExist = RMSettingJobDao.GetRMSettingJob(item => item.Id == jobId && item.JobType == (int)jobType) != null;
                    if (!isExist)
                    {
                        RMSettingJobInfo settingJobInfo = new RMSettingJobInfo
                        {
                            Id = jobId,
                            JobType = (int)JobType.ApplyTeamsSettings,
                            JobInfos = SerializerHelper.SerializeByDataContractSerializer(settingsPerContainer),
                        };

                        RMSettingJobDao.AddRMSettingJob(settingJobInfo);
                    }
                    logger.Info("Finishing stored job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, group.Key, settingsPerContainer.Count);
                    #endregion
                }
            }
            else
            {
                if (emptyContainers.Count > 0)
                {
                    foreach (var container in emptyContainers)
                    {
                        jobId = CreateApplySettingJob(runBy, jobRunByUser, container.Key, null, fullPath, jobPriority);
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{container.Value}");
                    }
                }
                else
                {
                    logger.Warn("No teams setting node group found.");
                    throw new Exception(I18NEntity.GetString("RM_SP_NoInhertSiteError"));
                }
            }
            return jobId;
        }

        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return DefaultSecurityContainerNameHelper.GetI18NName(selectedNode.Name);
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }

        private Task<bool> IsTeamsAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
            () =>
            {
                return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin);
            });
        }

        private string GetTeamsContainerId(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Id;
            }
            else
            {
                return GetTeamsContainerId(selectedNode.Parent);
            }
        }

        private bool ValidateGroupAvailability(List<ValidateNodeInfo> groupStatusCache, Guid groupId)
        {
            bool isAvailable = true;
            ValidateNodeInfo nodeInfo = new ValidateNodeInfo()
            {
                ScopeId = groupId,
                GroupId = groupId
            };
            if (nodeInfo.NodeExistingInCache(groupStatusCache))
            {
                if (!nodeInfo.NodeIsValid(groupStatusCache))
                {
                    logger.Warn($"Can't find the group: [{groupId}] in database");
                    isAvailable = false;
                }
            }
            else
            {
                var webApp = RMRemoteNodeDao.GetWebApplicationById(groupId.ToString());
                if (webApp == null)
                {
                    if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                    {
                        nodeInfo.AddNode2Cache(groupStatusCache);
                    }
                    logger.Warn($"Can't find the group: [{groupId}] in database.");
                    isAvailable = false;
                }
                if (!nodeInfo.NodeExistingInCache(groupStatusCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(groupStatusCache);
                }

            }
            return isAvailable;
        }

        private bool ValidateTeamsAvailability(List<ValidateNodeInfo> siteStatusCache, Guid teamsId, Guid teamsGroupId)
        {
            bool isAvailable = true;
            ValidateNodeInfo nodeInfo = new ValidateNodeInfo()
            {
                ScopeId = teamsId,
                GroupId = teamsGroupId
            };

            if (nodeInfo.NodeExistingInCache(siteStatusCache))
            {
                if (!nodeInfo.NodeIsValid(siteStatusCache))
                {
                    logger.Warn($"Site is null or has been move to other group [{teamsId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
            }
            else
            {
                var teamsNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId.ToString()).Item1;
                if (teamsNode == null || !teamsNode.parentId.Equals(teamsGroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (!nodeInfo.NodeExistingInCache(siteStatusCache))
                    {
                        nodeInfo.AddNode2Cache(siteStatusCache);
                    }
                    logger.Warn($"Site is null or has been move to other group [{teamsId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
                if (!nodeInfo.NodeExistingInCache(siteStatusCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(siteStatusCache);
                }

            }
            return isAvailable;
        }

        private List<RMTeamsSetting> GetTeamsSettings(JobRunBy runBy, RunApplySettingMethod runJobMethod, string scopeId = null, string teamsId = null, string siteId = null)
        {
            List<RMTeamsSetting> allSettings = null;
            if (runBy == JobRunBy.Control)
            {
                switch (runJobMethod)
                {
                    case RunApplySettingMethod.UpdatedScope:
                        allSettings = TeamsSettingDao.LoadRunJobSetting();
                        break;
                    case RunApplySettingMethod.AllScope:
                        logger.Info("apply full teams setting job");
                        allSettings = TeamsSettingDao.LoadAllSetting();
                        break;
                    case RunApplySettingMethod.Auto:
                        //Part job by node.
                        allSettings = TeamsSettingDao.LoadRunJobSetting();
                        if (allSettings.Count == 0)
                        {
                            logger.Info("apply full teams setting job");
                            allSettings = TeamsSettingDao.LoadAllSetting();
                        }
                        break;
                    case RunApplySettingMethod.SelectedNode:
                        if (string.IsNullOrWhiteSpace(scopeId) || string.IsNullOrWhiteSpace(siteId))
                        {
                            throw new Exception("Scope id or site id is null.");
                        }
                        logger.Info("Apply setting on selected node, ScopeId:{0}, teamsId:{1}, SiteId:{2}", scopeId, teamsId, siteId);
                        var webApp = RMRemoteNodeDao.GetWebApplicationById(scopeId);
                        string groupId = string.Empty;
                        if (webApp != null)
                        {
                            groupId = scopeId;
                        }
                        else
                        {
                            var teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId);
                            groupId = teams.Item1?.parentId;
                        }
                        var setting = TeamsSettingDao.GetSettingInfoByScope(new Guid(groupId), new Guid(teamsId), new Guid(siteId), new Guid(scopeId));
                        logger.Info("Get setting of selected node successfully, exist:{0}", setting != null);
                        if (setting != null)
                        {
                            allSettings = new List<RMTeamsSetting>() { setting };
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Full job
                allSettings = TeamsSettingDao.LoadAllSetting();
            }
            if (allSettings != null)
            {
                logger.Info("Load sp setting finished. Count:{0}", allSettings.Count);
            }
            return allSettings;
        }

        private string GetTreeNodeScopeId(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return node.Id;
            }
            else
            {
                return node.SPObjectId.ToString();
            }
        }

        private string GetTreeNodeTeamsId(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return Guid.Empty.ToString();
            }
            else
            {
                var siteNode = node.GetTeamsNode();
                return siteNode.TeamsId;
            }
        }

        private string GetTreeNodeSiteId(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.WebApplication || node.Level == (int)NodeLevel.Office365GroupEntire)
            {
                return Guid.Empty.ToString();
            }
            else
            {
                var siteNode = node.GetSiteCollectionNode();
                return siteNode.SPObjectId;
            }
        }

        private string CreateApplySettingJob(JobRunBy runBy, string jobRunByUser, string containerId = null, string scopedId = null, string fullPath = null, JobPriority jobPriority = JobPriority.Normal)
        {
            if (!string.IsNullOrEmpty(scopedId))
            {
                var node = TeamsSettingDao.LoadTeamsSettingForImportSetting(Guid.Empty, new Guid(scopedId));
                if (node != null && fullPath.StartsWith("/"))
                {
                    fullPath = node.FullPath;
                }
            }
            string jobId = string.Empty;
            if (runBy == JobRunBy.Control)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplyTeamsSettings, jobRunByUser, containerId, scopedId, fullPath);
                logger.Info("Begin control Apply Job {0}", jobId);
            }
            else if (runBy == JobRunBy.Schedule)
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplyTeamsSettings, "RM_TS_RunSchedule", containerId, scopedId, fullPath);
                logger.Info("Begin schedule Apply Job {0}", jobId);
            }
            else
            {
                jobId = RMJobMonitorService.CreateJob(JobType.ApplyTeamsSettings, jobRunByUser, containerId, scopedId, fullPath);
                logger.Info("Begin default Sync Job {0}", jobId);
            }
            if (jobPriority != JobPriority.Normal) JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority).GetAwaiter().GetResult();
            return jobId;
        }

        private void SeparateSubJobForApplySetting(List<RMSPTreeNode> availableTeams, Dictionary<Guid, RMTeamsSetting> gruopSetingMap, string jobId, JobRunBy runBy, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            Dictionary<string, List<RMSPTreeNode>> dic = this.GroupNodeForSubJob(availableTeams);
            var orderDic = dic.OrderBy(a => a.Value.Count);
            Dictionary<int, List<RMSPTreeNode>> subJobNodeDic = new Dictionary<int, List<RMSPTreeNode>>();
            int count = 0;
            foreach (KeyValuePair<string, List<RMSPTreeNode>> pa in orderDic)
            {
                tempList.AddRange(pa.Value);
                if (tempList.Count >= RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    count++;
                    var temp = new List<RMSPTreeNode>();
                    temp.AddRange(tempList);
                    subJobNodeDic.Add(count, temp);
                    tempList.Clear();
                }
            }
            if (tempList.Count > 0)
            {
                count++;
                subJobNodeDic.Add(count, tempList);
            }
            SubJobDao.UpdateSubJobCount(jobId, count);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, count);

            int currentSubjobIndex = 0;
            using (var subJob = new PerformanceScope("AddSubJob", $"AddSubJob{jobId}:{count}"))
            {
                foreach (KeyValuePair<int, List<RMSPTreeNode>> pa in subJobNodeDic)
                {
                    var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                    var extension = new Dictionary<string, string>
                    {
                        { "IsZeroShotMode", isZeroShotMode.ToString() }
                    };
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, count, pa.Value, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        logger.Debug("Start sub job {0}", subJobId);
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = runBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                            Extension = JsonConvert.SerializeObject(extension)
                        });
                    }
                    currentSubjobIndex++;
                }
            }
        }

        private Dictionary<string, List<RMSPTreeNode>> GroupNodeForSubJob(List<RMSPTreeNode> treeNodes)
        {
            Dictionary<string, List<RMSPTreeNode>> result = new Dictionary<string, List<RMSPTreeNode>>();
            result = treeNodes.GroupBy(t => t.TeamsId.ToString()).ToDictionary(group => group.Key, group => group.ToList());
            return result;

        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMTeamsSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            if (gruopSetingMap != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            }
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        public RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod)
        {
            logger.Debug("start ApplySettings");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            if (runJobMethod == RunApplySettingMethod.UpdatedScope)
            {
                var updatedScopeCount = 0;
                var settings = TeamsSettingDao.LoadRunJobSetting();
                updatedScopeCount = settings.Count;
                msg.Extension = updatedScopeCount.ToString();
                if (updatedScopeCount == 0)
                {
                    //选择updated scope run job，如果settings count为0直接返回，不起job
                    msg.Extsion1 = I18NEntity.GetString("RM_JS_SPS_NoUpdatedScope");
                    return msg;
                }
                msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobNodes"), updatedScopeCount);
                if (updatedScopeCount == 1)
                {
                    msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobSingleNode"), updatedScopeCount);
                }
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ApplyTeamsSettings,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0},{1}", fromTimerJobPage, Convert.ToInt32(runJobMethod))
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public bool ExistConfiguredSettings(JobType jobType)
        {
            return TeamsSettingDao.Exist(s => !s.IsRemoved);
        }

        public bool NeedRunUniqueIdJob(List<RMSPTreeNode> needRunNodes = null)
        {
            bool result = false;
            try
            {
                var needRunJobNodes = GetNeedRunJobNodes();
                foreach (var nodeInfo in needRunJobNodes)
                {
                    var setting = CloneSetting(nodeInfo);
                    if (setting.NodeInfo == null)
                    {
                        logger.Info("no change, nodeinfo null.Id:{0}", setting.ScopeId);
                        continue;
                    }
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);

                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        var group = RABrowserClient.GetWebApplicationById(node.SPObjectId);
                        if (group == null)
                        {
                            logger.Info($"can not find the group:{node?.FullPath}.");
                            continue;
                        }

                        Guid groupId = Guid.Empty;
                        Guid.TryParse(node.SPObjectId, out groupId);

                        if (ExistsSiteNode(node.SPObjectId) && !RMNodeFlagDao.IsNodeFlagExist(groupId, Guid.Empty, (int)NodeFlagType.TeamsUniqueId))
                        {
                            //group存在site节点，并且没有任何一个site节点成功跑过UniqueId job
                            if (needRunNodes != null)
                            {
                                needRunNodes.Add(node);
                            }
                            else
                            {
                                needRunNodes = new List<RMSPTreeNode>();
                                needRunNodes.Add(node);
                            }
                            logger.Info("need run unique id node:{0}", node.FullPath);
                            result = true;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while check unique id,ERROR:{0}", ex.ToString());
            }
            return result;
        }

        private bool ExistsSiteNode(string groupId)
        {
            try
            {
                var states = new SiteCollectionState[] { SiteCollectionState.AccessAll, SiteCollectionState.AccessSome };
                //var siteCollections = RemoteNodeService.GetRemoteSiteCollectionsByParentId(groupId, states);
                //return siteCollections.Count > 0;
                var teams = RemoteNodeService.GetRemoteSiteCollectionsByParentId(groupId, states);
                return teams.Count > 0;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while ExistsSiteNode, {ex}");
            }
            return false;
        }

        public List<RMTeamsSetting> GetNeedRunJobNodes()
        {
            return TeamsSettingDao.LoadShowUniqueIdSetting();
        }

        private RMTeamsSetting CloneSetting(RMTeamsSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMTeamsSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMTeamsSetting>(xml);
            return result;
        }

        #region Data Synchronisation

        public RAReturnMessage RunDataSyncJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Info("Start data sync for Teams source.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!IsExistCanRunJobNodes(selectedTree))
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_EXO_SyncData_NoSC");
                    return msg;
                }
            }

            try
            {
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsDataSynchronisation,
                    JobRunType = jobRunBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while creating Teams data sync job,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4Teams, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.TeamsDataSynchronisation;
            if (string.IsNullOrEmpty(param))
            {
                return RunDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
                return RunDataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        public RAReturnMessage RunDataSyncScheduleJob(JobRunBy jobRunBy)
        {
            logger.Debug("Start creating Teams data sync schedule job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsDataSynchronisationSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while create Teams data sync schedule job,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4Teams, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.TeamsDataSynchronisation : JobType.TeamsDataSynchronisationSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);

            List<string> runningJobIds = RMJobMonitorService.GetRunningJobs(JobType.TeamsDataSynchronisationSchedule);
            if (!runningJobIds.IsNullOrEmpty())
            {
                logger.Info("Current running scheduled data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                string jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser);
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. Has a Teams data synchronization job is already running.");
                return jobId;
            }
            else
            {
                return await RunDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
        }

        #endregion

        #region Apply setting schedule
        public string RunTeamsSettingsScheduleJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsScheduleSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunSharepointSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.TeamsApplySetting, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealTeamsSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, JobPriority jobPriority = JobPriority.Normal)
        {
            string jobId = string.Empty;

            List<string> runningJobs = RMJobMonitorService.GetRunningTeamsSettingJob();

            if (runningJobs.Count == 0)
            {
                jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.TeamsScheduleSetting, RunApplySettingMethod.Auto, null, null, null, null, jobPriority);
            }
            else
            {
                jobId = RMJobMonitorService.CreateJob(Contract.JobMonitor.JobType.TeamsScheduleSetting, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);
                if (jobPriority != JobPriority.Normal) await JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority);
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                logger.Info("CustomSetting job or GlobalSetting job or InheritSetting job has job running,so shedule job is skip");
            }

            return jobId;
        }
        #endregion

        #region Record disposal

        public RAReturnMessage RunRecordsDisposalJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("Run records disposal Job");

            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            var indexDevice = StorageDeviceService.GetIndexDevice();
            if (indexDevice == null)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunEnforceRuleActionJob_Failed_NoIndexDeviceSetting");
                return msg;
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                //var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsRecordsDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsDisposalJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.TeamsRecordsDisposal;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            return RunRecordsDisposalJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);

        }

        public async Task<string> RunRecordsDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            List<JobType> types = JobTypeConstants.ArchiveTeamsConflictType;
            string teamsUrl = selectedNode.GetTeamsNode()?.DisplayName ?? (RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.GetTeamsNode()?.SPObjectId).Item1?.url ?? string.Empty);
            string nodeUrl = selectedNode.FullPath;
            string nodeFullPath = selectedNode.Level == (int)NodeLevel.Office365GroupEntire ? selectedNode.DisplayName ?? teamsUrl : selectedNode.FullPath;
            if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (siteNode != null)
                {
                    nodeUrl = WebUtil.MakeFullUrl(selectedNode.GetSiteCollectionNode().FullPath, selectedNode.FullPath);
                    nodeFullPath = nodeUrl;
                }
            }

            List<RMSPTreeNode> availableNode = await AssembleDisposalRunnableNodeAsync(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                if (jobType == JobType.TeamsRecordsDisposal)
                {
                    RMJobMonitorService.SetSumSCCountOfJobExtension(0, jobId);
                    logger.Info("Initialize extension for main job {0} ,support job run failed.", jobId);
                }
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JMD_DisableRecordManagement_Or_HasOwnSettingMessage");
                return jobId;
            }
            
            if (availableNode.Count == 0)
            {
                logger.Warn($"Current has job running on same scope.will skip job");
                jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            var runningUrls = RMJobMonitorService.GetRunningTeamsArchiverJobSiteUrl(types,
                RuleSPTreeUtil.CheckNeedLoadRuningSCUrlBySelectNode(selectedNode),
                RuleSPTreeUtil.BuildSearchFilter(selectedNode, availableNode));
            availableNode = RuleSPTreeUtil.FilterTeamsAvailableNodeByRunningUrl(availableNode, runningUrls, selectedNode);

            if (availableNode.Count == 0)
            {
                logger.Warn($"Current has job running on same scope.will skip job after check conflict");
                jobId = RMJobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            jobId = RMJobMonitorService.CreateJobWithScopeIdForTeams(jobType, jobRunByUser, nodeUrl, nodeFullPath, GetSPContainerId(selectedNode), null, RuleSPTreeUtil.GenerateTeamsArchiveJobMonitorExtension(selectedNode, TreeMode.LifeTeams, teamsUrl : teamsUrl));
            List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
            
            try
            {
                var mIndexJobs = RMJobMonitorService.GetRunningJobs(indexJobTypes);

                if (mIndexJobs.Count > 0)
                {
                    //has move index job, need skip.
                    logger.Warn("Current has move index job running.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetTeamsRules(selectedNode));
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while check index job and add job rule mappings,ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            UpdateJobVersion(jobId, jobType);
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            if (subJobCount > 0)
            {
                RMJobMonitorService.SetSumSCCountOfJobExtension(subJobCount, jobId);
                logger.Info("Initialize extension for main job {0}, sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, subJobCount);
            }
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            if (!IsTrailLicenceAndExceedSizeLimit())
            {
                if (licenseHelperService.HasOpusSOLicense)
                {
                    foreach (RMSPTreeNode site in availableNode)
                    {
                        tempList.Add(site);
                        string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, false, site.FullPath, site.O365TenantId);
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                else
                {
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_NOSOLicense");
                }
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
            }
            return jobId;
        }

        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)jobType))
            {
                using (var progresExecutor = AvePoint.RA.SharePoint.Common.JobExecutionProgress.JobExecutionProgressStatisticExecutor.Instance)
                {
                    logger.Info("Init progress for sub job {0}, type {1}", subJob.Id, subJob.JobType);
                    progresExecutor.InitializeJobExecutionProgressStatictics(subJob.String1, subJob.Id, subJob.ParentId, subJob.JobType);
                }
            }
            return subJobId;
        }

        private async Task<List<RMSPTreeNode>> AssembleDisposalRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            var breakNodePaths = GetDisposalBreakNodePaths(selectedNode);
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> teams = await RMTeamsTreeService.BrowseAsync(selectedNode);
                if (teams.IsNullOrEmpty())
                {
                    return availableNode;
                }

                foreach (var teamsNode in teams)
                {
                    var sites = await RMTeamsTreeService.BrowseDirectSitesByTeamNode(
                        RMDtoConverter.ConvertRMTree2SPTree(teamsNode));
                    await AddEnabledDisposalSitesAsync(availableNode, sites, teamsNode, breakNodePaths);
                }
            }
            else if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                var teamsNode = selectedNode.GetTeamsNode();
                if (ValidateTeamsExist(teamsNode))
                {
                    var sites = await RMTeamsTreeService.BrowseDirectSitesByTeamNode(
                        RMDtoConverter.ConvertRMTree2SPTree(selectedNode));
                    await AddEnabledDisposalSitesAsync(availableNode, sites, selectedNode, breakNodePaths);
                }
                else
                {
                    logger.Info("Teams not exist, teams:{0}", selectedNode.Name);
                }
            }
            else
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (ValidateSiteExist(siteNode))
                {
                    selectedNode.O365TenantId = siteNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private List<string> GetDisposalBreakNodePaths(RMSPTreeNode selectedNode)
        {
            var parentId = ScheduleService.GetProfileId(selectedNode) + "|";
            return RMScheduleDao.GetDisposalBreakNodes(parentId)
                .Select(item => JsonConvert.DeserializeObject<RMSPTreeNode>(item))
                .Where(node => node != null
                    && node.Level != (int)NodeLevel.WebApplication
                    && node.Level != (int)NodeLevel.Office365GroupEntire)
                .Select(node => node.FullPath)
                .ToList();
        }

        private async Task AddEnabledDisposalSitesAsync(
            List<RMSPTreeNode> availableNodes,
            List<RMSPTreeNode> sites,
            RMSPTreeNode teamsNode,
            List<string> breakNodePaths)
        {
            if (sites.IsNullOrEmpty())
            {
                return;
            }

            await LoadSiteSettingsUnderTeamsNodeAsync(sites, teamsNode);
            availableNodes.AddRange(sites.Where(site =>
                site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable
                && !breakNodePaths.Contains(site.FullPath)));
        }

        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            return TreeNodeUtil.GetSPContainderId(selectedNode);
        }

        private List<Guid> GetTeamsRules(RMSPTreeNode tree)
        {
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = RuleManagerService.GetRulesFromRecords();
            return TermRuleAssociationDao.GetTermWithRuleLevel(tree.Level, rules).Select(t => t.RuleId).Distinct().ToList();
        }

        private bool IsTrailLicenceAndExceedSizeLimit()
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    logger.Info("this is Trial licence");
                    var size = StorageDeviceService.GetArchiverStorageGBSize();
                    var resultSize = size;
                    if (resultSize >= 5)
                    {
                        logger.Info($"current trial licence user has run out of size {resultSize}gb is bigger than 5gb");
                        //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error($"some thing went wrong when check Trail Licence And Exceed Size,error{e.ToString()}");
                return false;
            }
        }

        #endregion

        #region Migrate teams
        public async Task<RAReturnMessage> UpgradeTeams(bool isUpgradeSettings)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.InitNodesFromAOS, $"{tenantId}", TimeSpan.FromMinutes(5)))
                {
                    CreateMigrationJob(tenantId);
                }
            }
            catch (Exception e) 
            {
                logger.Error($"Create sync node has errors: {e}");
                return new RAReturnMessage() { ErrorMessage = I18NEntity.GetString("RM_Teams_CreateSyncJobHasError") };
            }

            var keyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeams, Value = "True" };
            var settingKeyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeamsSettings, Value = isUpgradeSettings ? "False" : "True" };
            var dataKeyValueEntity = new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeamsData, Value = "False" };
            await RMKeyValueDao.SaveOrUpdateAsync(keyValueEntity);
            await RMKeyValueDao.SaveOrUpdateAsync(settingKeyValueEntity);
            await RMKeyValueDao.SaveOrUpdateAsync(dataKeyValueEntity);

            return new RAReturnMessage() { };
        }

        private void CreateMigrationJob(string tenantId)
        {   
            var runningJobs = RMJobMonitorService.GetRunningJobs(JobType.SyncNodesFromAOS);
            if (runningJobs.Count == 0)
            {
                logger.Info($"create job for InitNodesFromAOS - {tenantId}");
                var syncNodeJobId = RemoteNodeService.CreateSyncAllNodesJob();
                if (string.IsNullOrEmpty(syncNodeJobId))
                    throw new Exception("Can not create sync node job");
                logger.Info($"InitNodesFromAOS job created : {syncNodeJobId}");
            }
            else
            {
                logger.Info($"{runningJobs.Count} sync node job running.");
            }
        }

        #endregion
        #region Private methods
        private bool IsExistCanRunJobNodes(RMSPTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                return IsEnableRecordManagement(selectedTree);
            }
            return false;
        }

        private bool IsEnableRecordManagement(RMSPTreeNode selectedNode)
        {
            Guid siteId = Guid.NewGuid();
            Guid teamId = Guid.NewGuid();
            Guid teamGroupId = Guid.NewGuid();
            RMTeamsSetting setting = null;

            int cnt = 6;
            do
            {
                switch ((NodeLevel)selectedNode.Level)
                {
                    case NodeLevel.WebApplication:
                        {
                            teamGroupId = Guid.Parse(selectedNode.SPObjectId);
                            siteId = Guid.Empty;
                            teamId = Guid.Empty;
                            break;
                        }
                    case NodeLevel.Office365GroupEntire:
                        {
                            siteId = Guid.Empty;
                            teamId = Guid.Parse(selectedNode.TeamsId);
                            teamGroupId = selectedNode.SiteGroupId;
                            break;
                        }
                    case NodeLevel.SiteCollection:
                        {
                            siteId = Guid.Parse(selectedNode.SPObjectId);
                            teamId = Guid.Parse(selectedNode.TeamsId);
                            teamGroupId = selectedNode.SiteGroupId;
                            break;
                        }
                }
                setting = TeamsSettingDao.GetSettingInfoByScope(teamGroupId, teamId, siteId, Guid.Parse(selectedNode.SPObjectId));
                selectedNode = selectedNode.Parent;
            }
            while (setting == null && selectedNode != null && cnt-- > 0);

            if (setting == null || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                logger.Warn($"IsEnableRecordManagement:setting==null:{setting == null}");
                return false;
            }
            logger.Info($"IsEnableRecordManagement:{true}");
            return true;
        }

        private async Task<string> RunDataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            jobId = RMJobMonitorService.CreateJob(JobType.TeamsDataSynchronisation, jobRunByUser, GetTeamGroupId(selectedNode));
            List<RMSPTreeNode> availableNodes = new List<RMSPTreeNode>();
            try
            {
                availableNodes = await AssembleSyncDataRunnableNodeAsync(selectedNode);
            bool noContentModified = false;
            if (availableNodes.Any())
            {
                var IsChangedInheritOption = IsHasContainerLevelInheritChanged(selectedNode);
                if (!IsChangedInheritOption)
                {
                    availableNodes = await FilterTeamsModified(availableNodes);
                    if (availableNodes.Count == 0)
                    {
                        noContentModified = true;
                    }
                }
            }
            if (availableNodes.IsNullOrEmpty())
            {
                if (noContentModified)
                {
                    logger.Warn("No content modified under sites.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    logger.Warn("No available sc to run");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_SPS_NotEnableSyncSetting");
                }
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while AssembleSyncDataRunnableNodeAsync,ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_SPS_AsyncRunnableNodeError");
                return jobId;
            }
            
            int subJobCount = availableNodes.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            foreach (RMSPTreeNode site in availableNodes)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateTeamsDataSyncSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = JobRunBy.Control,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear(); 
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateTeamsDataSyncSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        private async Task<string> RunDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser);
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            var allSetting = TeamsSettingDao.LoadAllSetting().Where(s => s.IsSyncData);

            if (allSetting.IsNullOrEmpty())
            {
                logger.Warn("There is no site collection setting enable sync data into Explorer.");
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncSCUnderGroup");
                return jobId;
            }

            try
            {
            foreach (var setting in allSetting)
            {
                var webApp = RMRemoteNodeDao.GetWebApplicationById(setting.TeamsGroupId.ToString());
                if (webApp == null)
                {
                    logger.Warn($"Can't find the group: [{setting.TeamsGroupId}] in database.");
                    continue;
                }

                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);

                if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    var teamNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.TeamsId).Item1;
                    if (teamNode == null)
                    {
                        logger.Info("Teams/group not exist, site:{0}", selectedNode.Name);
                        continue;
                    }
                    if (!teamNode.parentId.Equals(setting.TeamsGroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Team group has been moved to other container, site:{0}", selectedNode.Name);
                        continue;
                    }
                }
                else if (selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var teamNode = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.TeamsId).Item1;
                    var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                    if (teamNode == null || site == null)
                    {
                        logger.Info("Team group or site collection not exist, site:{0} , team group id:{1}", selectedNode.Name, selectedNode.TeamsId);
                        continue;
                    }

                    if (!teamNode.parentId.Equals(setting.TeamsGroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Team group has been moved to other container, site:{0}", selectedNode.Name);
                        continue;
                    }
                }

                if (selectedNode.Level == (int)NodeLevel.WebApplication || selectedNode.Level == (int)NodeLevel.Office365GroupEntire || selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var tempNodes = await AssembleSyncDataRunnableNodeAsync(selectedNode);
                    foreach (var node in tempNodes)
                    {
                        if (!availableNode.Select(n => n.Id).ToList().Contains(node.Id))
                        {
                            availableNode.Add(node);
                        }
                    }
                }
            }
            //remove sites that not changed since last job
            bool noContentModified = false;
            if (availableNode.Count > 1)
            {
                using (var performance = new PerformanceScope("RMTeamsSettingsService.FilterNoContentModifiedSites"))
                {
                    Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                    var modifiedDateCache = GetSiteModifiedDateCache(availableNode);
                    List<string> notIncludeSiteIds = new List<string>();
                    foreach (var node in availableNode)
                    {
                        if (!NeedCollectSPSite(modifiedDateCache, node, termScopeCache))
                        {
                            notIncludeSiteIds.Add(node.SPObjectId);
                        }
                    }
                    availableNode = availableNode.Where(n => !notIncludeSiteIds.Contains(n.SPObjectId)).ToList();
                    if (availableNode.Count == 0)
                    {
                        noContentModified = true;
                    }
                }
            }
            if (availableNode.IsNullOrEmpty())
            {
                if (noContentModified)
                {
                    logger.Warn("No content modified under sites.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    logger.Warn("No available sc to run");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroupBySchedule");
                }
                return jobId;
            }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while assemble sync data runnable node. ERROR:{0}", ex.ToString());
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }

            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            jobType = JobType.TeamsDataSynchronisation;
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }
        
        private bool IsHasContainerLevelInheritChanged(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                Guid groupId = Guid.Empty;
                if (selectedNode != null && !string.IsNullOrEmpty(selectedNode.SPObjectId))
                {
                    groupId = new Guid(selectedNode.SPObjectId);
                }
                return TeamsSettingDao.CheckHasInheritChanged(groupId, Guid.Empty);
            }
            else if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                Guid groupId = Guid.Empty;
                Guid teamsId = Guid.Empty;
                if (selectedNode != null && !string.IsNullOrEmpty(selectedNode.SPObjectId))
                {
                    groupId = new Guid(selectedNode.ParentId);
                }
                if (selectedNode != null && !string.IsNullOrEmpty(selectedNode.TeamsId))
                {
                    teamsId = new Guid(selectedNode.TeamsId);
                }

                return TeamsSettingDao.CheckHasInheritChanged(groupId, teamsId);
            }

            return false;
        }

        private string GetTeamGroupId(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Id;
            }
            else
            {
                return GetTeamGroupId(selectedNode.Parent);
            }
        }

        private async Task<List<RMSPTreeNode>> AssembleSyncDataRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> teams = await RMTeamsTreeService.BrowseAsync(selectedNode);
                if (teams == null || !teams.Any()) return availableNode;
                await LoadTeamsSettingUnderGroupAsync(teams, selectedNode);
                foreach (var team in teams)
                {
                    if (IsEnableSyncAndRecordManagement(team))
                    {
                        availableNode.Add(team);
                    }
                }
            }
            else if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                var team = selectedNode;
                await LoadTeamsSettingUnderGroupAsync(new List<RMSPTreeNode> { team }, selectedNode);
                if (IsEnableSyncAndRecordManagement(team))
                {
                    availableNode.Add(selectedNode);
                }
            }
            else
            {
                if (ValidateSiteExist(selectedNode))
                {
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        public async Task LoadTeamsSettingUnderGroupAsync(List<RMSPTreeNode> nodes, RMSPTreeNode groupNode)
        {
            try
            {
                using (var performance = new PerformanceScope("RMTeamsSettingsService.LoadTeamsSettingUnderGroup"))
                {
                    Guid groupId = Guid.Empty;
                    if (groupNode != null && !string.IsNullOrEmpty(groupNode.SPObjectId))
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    logger.Info($"Begin to load teams settings for group: URL [{groupNode.FullPath}] ID [{groupId}], Site collection count:{nodes.Count}");
                    var GSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
                    string GlobalColumnName = string.Empty;
                    RMTerm termScope = null;
                    RMTerm containerTerm = null;
                    string groupTermFullPath = string.Empty;
                    bool groupTermExpired = false;
                    bool groupContainerTermExpired = false;
                    List<ToUserInfo> groupRecordOwner = null;
                    if (GSetting != null)
                    {
                        termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);
                        GlobalColumnName = GSetting.ColumnName;
                        if (termScope != null)
                        {
                            groupTermFullPath = TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                            groupTermExpired = TermDao.IsExpiredTerm(termScope.Id);
                        }
                        if (containerTerm != null)
                        {
                            groupContainerTermExpired = TermDao.IsExpiredTerm(containerTerm.Id);
                        }
                        groupRecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.Teams);
                    }
                    List<RMTeamsSetting> settings;
                    using (var performance0 = new PerformanceScope("RMTeamsSettingsService.LoadTeamsSettings"))
                    {
                        settings = TeamsSettingDao.LoadTeamsSettings(groupId, true);
                    }
                    foreach (var node in nodes)
                    {
                        ArgumentCheck.NotNull(node, nameof(node));
                        var teamsNode = node;
                        Guid teamsId = Guid.Empty;
                        if (teamsNode != null && !string.IsNullOrEmpty(teamsNode.TeamsId))
                        {
                            teamsId = new Guid(teamsNode.TeamsId);
                        }
                        logger.Info($"Load teams settings for teams: [{teamsNode?.FullPath}] [{teamsId}].");
                        var TeamsSetting = settings.Where(s => s.ScopeId == teamsId && s.TeamsId == teamsId).FirstOrDefault();
                        if (TeamsSetting == null)
                        {
                            if (GSetting != null)
                            {
                                node.ColumnName = GlobalColumnName;
                                node.ExistColumnName = GSetting.ExistColumnName;
                                node.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                                node.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                                node.TermSetName = GSetting.TermSetName;
                                node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                                node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : groupTermFullPath;
                                node.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                                node.RecordOwner = groupRecordOwner;
                                node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                                node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || groupTermExpired;
                                node.isFailedConfigClassification = GSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = GSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || groupContainerTermExpired;
                                node.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                                node.EnableRecordManagement = GSetting.EnableRecordManagement;
                                node.isEnableClassification = GSetting.isEnableClassification;
                                node.IsSyncData = GSetting.IsSyncData;
                                node.ApprovalType = (int)GSetting.ApprovalType;
                            }
                        }
                        else
                        {
                            if (TeamsSetting != null && (TeamsSetting.TermIdOfContainer != Guid.Empty || TeamsSetting.TermId != Guid.Empty || TeamsSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                            {
                                node.HasCustomSetting = true;
                            }
                            else
                            {
                                node.HasCustomSetting = false;
                            }
                            if (TeamsSetting != null)
                            {
                                var siteTermScope = TermDao.GetRMTermByGuId(TeamsSetting.TermId);
                                var siteDefaultTerm = TermDao.GetRMTermByGuId(TeamsSetting.DefaultTermId);
                                var siteContainerTerm = TermDao.GetRMTermByGuId(TeamsSetting.TermIdOfContainer);

                                node.IsCustomSetting = true;
                                node.ColumnName = GlobalColumnName;
                                node.Description = TeamsSetting.Description;
                                node.DefaultTermId = TeamsSetting.DefaultTermId;
                                node.DefaultTermName = siteDefaultTerm == null ? TeamsSetting.DefaultTermName : siteDefaultTerm.Name;
                                node.DefaultTermNameFullPath = siteDefaultTerm == null ? TeamsSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(TeamsSetting.DefaultTermId);
                                node.TermId = TeamsSetting.TermId;
                                node.TermName = siteTermScope == null ? TeamsSetting.TermName : siteTermScope.Name;
                                node.TermNameFullPath = siteTermScope == null ? TeamsSetting.TermName : TermDao.GetTermFullPathByTermId(TeamsSetting.TermId);
                                node.TermSetId = TeamsSetting.TermSetId;
                                node.TermSetName = TeamsSetting.TermSetName;
                                node.IsTermRemoved = siteTermScope == null ? false : siteTermScope.IsRemoved;
                                node.IsDefaultTermRemoved = siteDefaultTerm == null ? false : siteDefaultTerm.IsRemoved;
                                node.IsTermDeprecated = siteTermScope == null ? false : siteTermScope.IsDeprecated || TermDao.IsExpiredTerm(siteTermScope.Id);
                                node.IsDefaultTermDeprecated = siteDefaultTerm == null ? false : siteDefaultTerm.IsDeprecated || TermDao.IsExpiredTerm(siteDefaultTerm.Id);
                                node.DescriptionOfContainer = TeamsSetting.DescriptionOfContainer;
                                node.IsInheritParentTerm = TeamsSetting.IsInheritParentTerm;
                                node.TermIdOfContainer = TeamsSetting.TermIdOfContainer;
                                node.TermNameOfContainer = siteContainerTerm == null ? TeamsSetting.TermNameOfContainer : siteContainerTerm.Name;
                                node.isEnableClassification = TeamsSetting.isEnableClassification;
                                node.EnableRecordManagement = TeamsSetting.EnableRecordManagement;
                                node.IsEnableHoldPhyical = TeamsSetting.IsEnableHoldPhyical;
                                node.isFailedConfigClassification = TeamsSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = TeamsSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = siteContainerTerm == null ? false : siteContainerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = siteContainerTerm == null ? false : siteContainerTerm.IsDeprecated || TermDao.IsExpiredTerm(siteContainerTerm.Id);
                                node.ExistColumnName = TeamsSetting.ExistColumnName;
                                node.IsUsingExistColumnName = TeamsSetting.IsUsingExistColumnName;
                                node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(TeamsSetting.Id, RecordOwnerSettingType.Teams);
                                node.EMailToRecordOwner = TeamsSetting.EMailToRecordOwner;
                                node.IsDisplyaTermPath = TeamsSetting.IsDisplyaTermPath;
                                node.EnableRelatedRecords = TeamsSetting.EnableRelatedRecords;
                                node.IsSyncData = TeamsSetting.IsSyncData;
                                node.ApprovalType = (int)TeamsSetting.ApprovalType;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load Teams settings.Error:{0}", e.ToString());
                throw;
            }
        }

        public async Task LoadSiteSettingsUnderTeamsNodeAsync(List<RMSPTreeNode> nodes, RMSPTreeNode teamsNode)
        {
            try
            {
                logger.Info($"Begin to load sites settings for teams:{teamsNode.FullPath} Site collection count:{nodes.Count}");
                using (var performance = new PerformanceScope("RMTeamsSettingsService.LoadTeamsSettingUnderGroup"))
                {
                    Guid groupId = Guid.Empty;
                    Guid teamsId = Guid.Empty;
                    if (teamsNode != null)
                    {
                        groupId = teamsNode.SiteGroupId != Guid.Empty ? teamsNode.SiteGroupId : new Guid(teamsNode.Parent.Id);
                        teamsId = new Guid(teamsNode.SPObjectId);
                    }
                    var TSetting = TeamsSettingDao.LoadTeamsSetting(teamsId, teamsId, Guid.Empty);
                    if(TSetting == null)
                    {
                        TSetting = TeamsSettingDao.LoadTeamsSetting(groupId, Guid.Empty, Guid.Empty);
                    }
                    string GlobalColumnName = string.Empty;
                    RMTerm termScope = null;
                    RMTerm containerTerm = null;
                    string groupTermFullPath = string.Empty;
                    bool groupTermExpired = false;
                    bool groupContainerTermExpired = false;
                    List<ToUserInfo> groupRecordOwner = null;
                    if (TSetting != null)
                    {
                        termScope = TermDao.GetRMTermByGuId(TSetting.DefaultTermId);
                        containerTerm = TermDao.GetRMTermByGuId(TSetting.TermIdOfContainer);
                        GlobalColumnName = TSetting.ColumnName;
                        if (termScope != null)
                        {
                            groupTermFullPath = TermDao.GetTermFullPathByTermId(TSetting.DefaultTermId);
                            groupTermExpired = TermDao.IsExpiredTerm(termScope.Id);
                        }
                        if (containerTerm != null)
                        {
                            groupContainerTermExpired = TermDao.IsExpiredTerm(containerTerm.Id);
                        }
                        groupRecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(TSetting.Id, RecordOwnerSettingType.Teams);
                    }
                    List<RMTeamsSetting> settings;
                    using (var performance0 = new PerformanceScope("RMTeamsSettingsService.LoadTeamsSettings"))
                    {
                        settings = TeamsSettingDao.LoadTeamsSettings(groupId, true);
                    }
                    foreach (var node in nodes)
                    {
                        ArgumentCheck.NotNull(node, nameof(node));
                        var siteNode = node;
                        Guid siteId = Guid.Empty;
                        if (siteNode != null)
                        {
                            siteId = new Guid(siteNode.Id);
                        }
                        var siteSetting = settings.Where(s => s.ScopeId == siteId && s.SiteId == siteId).FirstOrDefault();
                        if (siteSetting == null)
                        {
                            if (TSetting != null)
                            {
                                node.ColumnName = GlobalColumnName;
                                node.ExistColumnName = TSetting.ExistColumnName;
                                node.IsUsingExistColumnName = TSetting.IsUsingExistColumnName;
                                node.TermNameOfContainer = containerTerm == null ? TSetting.TermNameOfContainer : containerTerm.Name;
                                node.TermSetName = TSetting.TermSetName;
                                node.DefaultTermName = termScope == null ? TSetting.DefaultTermName : termScope.Name;
                                node.DefaultTermNameFullPath = termScope == null ? TSetting.DefaultTermName : groupTermFullPath;
                                node.IsDisplyaTermPath = TSetting.IsDisplyaTermPath;
                                node.RecordOwner = groupRecordOwner;
                                node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                                node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || groupTermExpired;
                                node.isFailedConfigClassification = TSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = TSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || groupContainerTermExpired;
                                node.EnableRelatedRecords = TSetting.EnableRelatedRecords;
                                node.EnableRecordManagement = TSetting.EnableRecordManagement;
                                node.isEnableClassification = TSetting.isEnableClassification;
                                node.IsSyncData = TSetting.IsSyncData;
                                node.ApprovalType = (int)TSetting.ApprovalType;
                            }
                        }
                        else
                        {
                            if (siteSetting != null && (siteSetting.TermIdOfContainer != Guid.Empty || siteSetting.TermId != Guid.Empty || siteSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                            {
                                node.HasCustomSetting = true;
                            }
                            else
                            {
                                node.HasCustomSetting = false;
                            }

                            if (siteSetting != null)
                            {
                                var siteTermScope = TermDao.GetRMTermByGuId(siteSetting.TermId);
                                var siteDefaultTerm = TermDao.GetRMTermByGuId(siteSetting.DefaultTermId);
                                var siteContainerTerm = TermDao.GetRMTermByGuId(siteSetting.TermIdOfContainer);

                                node.IsCustomSetting = true;
                                node.ColumnName = GlobalColumnName;
                                node.Description = siteSetting.Description;
                                node.DefaultTermId = siteSetting.DefaultTermId;
                                node.DefaultTermName = siteDefaultTerm == null ? siteSetting.DefaultTermName : siteDefaultTerm.Name;
                                node.DefaultTermNameFullPath = siteDefaultTerm == null ? siteSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(siteSetting.DefaultTermId);
                                node.TermId = siteSetting.TermId;
                                node.TermName = siteTermScope == null ? siteSetting.TermName : siteTermScope.Name;
                                node.TermNameFullPath = siteTermScope == null ? siteSetting.TermName : TermDao.GetTermFullPathByTermId(siteSetting.TermId);
                                node.TermSetId = siteSetting.TermSetId;
                                node.TermSetName = siteSetting.TermSetName;
                                node.IsTermRemoved = siteTermScope == null ? false : siteTermScope.IsRemoved;
                                node.IsDefaultTermRemoved = siteDefaultTerm == null ? false : siteDefaultTerm.IsRemoved;
                                node.IsTermDeprecated = siteTermScope == null ? false : siteTermScope.IsDeprecated || TermDao.IsExpiredTerm(siteTermScope.Id);
                                node.IsDefaultTermDeprecated = siteDefaultTerm == null ? false : siteDefaultTerm.IsDeprecated || TermDao.IsExpiredTerm(siteDefaultTerm.Id);
                                node.DescriptionOfContainer = siteSetting.DescriptionOfContainer;
                                node.IsInheritParentTerm = siteSetting.IsInheritParentTerm;
                                node.TermIdOfContainer = siteSetting.TermIdOfContainer;
                                node.TermNameOfContainer = siteContainerTerm == null ? siteSetting.TermNameOfContainer : siteContainerTerm.Name;
                                node.isEnableClassification = siteSetting.isEnableClassification;
                                node.EnableRecordManagement = siteSetting.EnableRecordManagement;
                                node.IsEnableHoldPhyical = siteSetting.IsEnableHoldPhyical;
                                node.isFailedConfigClassification = siteSetting.isFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = siteSetting.isFailedConfigMetaDataColumn;
                                node.IsClassificationTermRemoved = siteContainerTerm == null ? false : siteContainerTerm.IsRemoved;
                                node.IsClassificationTermDeprecated = siteContainerTerm == null ? false : siteContainerTerm.IsDeprecated || TermDao.IsExpiredTerm(siteContainerTerm.Id);
                                node.ExistColumnName = siteSetting.ExistColumnName;
                                node.IsUsingExistColumnName = siteSetting.IsUsingExistColumnName;
                                node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(siteSetting.Id, RecordOwnerSettingType.Teams);
                                node.EMailToRecordOwner = siteSetting.EMailToRecordOwner;
                                node.IsDisplyaTermPath = siteSetting.IsDisplyaTermPath;
                                node.EnableRelatedRecords = siteSetting.EnableRelatedRecords;
                                node.IsSyncData = siteSetting.IsSyncData;
                                node.ApprovalType = (int)siteSetting.ApprovalType;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load Teams settings.Error:{0}", e.ToString());
                throw;
            }
        }

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("Get team node error: {0}", e);
            }

            if (site != null)
            {
                selectedNode.O365TenantId = site.TenantId;
                return true;
            }
            return false;
        }

        private bool ValidateTeamsExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                site = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.Id).Item1;
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get teams node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        private string CreateTeamsDataSyncSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMTeamsSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)JobStatus.Wait,
                Weight = 100d / subJobCount,
                Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            if (gruopSetingMap != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            }
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private Dictionary<string, DateTime> GetSiteModifiedDateCache(List<RMSPTreeNode> availableNode)
        {
            Dictionary<string, DateTime> siteModifiedDateCache = new Dictionary<string, DateTime>();
            try
            {
                using (var performance = new PerformanceScope("RMTeamsSettingsService.GetSiteModifiedDateCache"))
                {
                    List<string> siteUrls = availableNode.Select(s => s.FullPath).ToList();
                    var remoteSites = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(siteUrls);
                    var tenantIds = remoteSites.Select(s => s.TenantId).Distinct().ToList();
                    CommonClientContext clientContext = new CommonClientContext();
                    foreach (var tenantId in tenantIds)
                    {
                        try
                        {
                            var site = remoteSites.Where(s => s.TenantId == tenantId).FirstOrDefault();
                            var remoteSite = RABrowserClient.GetRemoteSiteCollectionById(site?.id);
                            var cache = clientContext.GetSiteModifiedDateCache(remoteSite);
                            if (cache != null && cache.Count > 0)
                            {
                                cache.ToList().ForEach(x => siteModifiedDateCache.Add(x.Key, x.Value));
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"An error occurred while getting site modified date cache,tenant id:{tenantId} error:{e.ToString()}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting site modified date cache, error:{e.ToString()}");
            }
            return siteModifiedDateCache;
        }

        private bool NeedCollectSPSite(Dictionary<string, DateTime> modifiedDateCache, RMSPTreeNode site, Dictionary<Guid, List<Guid>> termScopeCache)
        {
            if (modifiedDateCache.ContainsKey(site.FullPath.ToLower()))
            {
                var groupNode = GetGroupNode(site);
                var collectionTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.TeamsSync, new Guid(groupNode.Id), new Guid(site.SPObjectId));
                if (collectionTime != DateTime.MinValue.Ticks
                    && collectionTime >= modifiedDateCache[site.FullPath.ToLower()].Ticks
                    && !HasChangedTermIds(collectionTime, site, termScopeCache))
                {
                    logger.Info($"Site:{site.FullPath} content modified date:{modifiedDateCache[site.FullPath.ToLower()].Ticks} last collection time:{collectionTime}, no need run data sync job.");
                    return false;
                }
            }
            return true;
        }

        private bool HasChangedTermIds(long ticks, RMSPTreeNode site, Dictionary<Guid, List<Guid>> termScopeCache)
        {
            List<Guid> allTerms = new List<Guid>();
            try
            {
                List<Guid> subTerms = new List<Guid>();
                allTerms = RMChangeClassificationDao.GetAllChange(ticks, (int)Contract.Object.TermChangeType.TermRule);
                foreach (var id in allTerms)
                {
                    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
                }
                allTerms.AddRange(subTerms);

                if (allTerms.Count > 0)
                {
                    var settings = TeamsSettingDao.LoadSettingsUnderSite(new Guid(GetGroupNode(site).SPObjectId), new Guid(site.TeamsId), new Guid(site.SPObjectId));
                    var spSetting = TeamsSettingDao.GetSettingInfoByScope(new Guid(GetGroupNode(site).SPObjectId), new Guid(site.TeamsId), Guid.Empty, new Guid(site.TeamsId));
                    if (spSetting == null)
                    {
                        spSetting = TeamsSettingDao.GetSettingInfoByScope(new Guid(GetGroupNode(site).SPObjectId), Guid.Empty, Guid.Empty, new Guid(GetGroupNode(site).SPObjectId));
                    }
                    settings.Add(spSetting);

                    foreach (var setting in settings)
                    {
                        List<Guid> termIdsUnderScope = new List<Guid>();
                        if (setting.TermId != Guid.Empty)
                        {
                            if (termScopeCache.ContainsKey(setting.TermId))
                            {
                                termIdsUnderScope = termScopeCache[setting.TermId];
                            }
                            else
                            {
                                termIdsUnderScope.Add(setting.TermId);
                                termIdsUnderScope.AddRange(TermDao.GetAllSubTermUniqueIdsByTermId(setting.TermId));
                                termScopeCache.Add(setting.TermId, termIdsUnderScope);
                            }
                        }
                        else if (setting.TermSetId != Guid.Empty)
                        {
                            if (termScopeCache.ContainsKey(setting.TermSetId))
                            {
                                termIdsUnderScope = termScopeCache[setting.TermSetId];
                            }
                            else
                            {
                                var termIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(setting.TermSetId);
                                termIdsUnderScope.AddRange(termIds);
                                termScopeCache.Add(setting.TermSetId, termIdsUnderScope);
                            }
                        }

                        if (termIdsUnderScope.Any(t => allTerms.Contains(t)))
                        {
                            Guid termScopeId = setting.TermId != Guid.Empty ? setting.TermId : setting.TermSetId;
                            logger.Info($"Site: {site.FullPath} has changed term ids. Setting scope id:{setting.ScopeId} Setting group id:{setting.TeamsGroupId} Term scope id:{termScopeId}");
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("get change terms error {0}", e.ToString());
                return false;
            }
            return false;
        }

        private static string GetJobRunByUser(JobRunBy jobRunBy, string jobRunByUser)
        {
            if (jobRunBy == JobRunBy.Control)
            {
                jobRunByUser = string.IsNullOrEmpty(jobRunByUser) ? TenantLocalValue.LogonUserEmail : jobRunByUser;
            }
            else
            {
                jobRunByUser = "RM_TS_RunSchedule";
            }

            return jobRunByUser;
        }

        private bool IsEnableSyncAndRecordManagement(RMSPTreeNode node)
        {
            return node.IsSyncData && node.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable;
        }

        private async Task<List<RMSPTreeNode>> FilterTeamsModified(List<RMSPTreeNode> teamsNodes)
        {
            var filteredNodes =  teamsNodes;
            foreach (var teamsNode in teamsNodes)
            {
                List<string> notIncludeSiteIds = new List<string>();
                if (teamsNode.Level == (int)NodeLevel.Office365GroupEntire)
                {
                    List<RMSPTreeNode> teamsSites = await RMTeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(teamsNode));
                    var modifiedDateCache = GetSiteModifiedDateCache(teamsSites);
                    foreach (var site in teamsSites)
                    {
                        using (var performance = new PerformanceScope("RMTeamsSettingsService.FilterNoContentModifiedSites"))
                        {
                            Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                            if (!NeedCollectSPSite(modifiedDateCache, site, termScopeCache))
                            {
                                notIncludeSiteIds.Add(site.SPObjectId);
                            }
                        }
                    }
                    if (notIncludeSiteIds.Count == teamsSites.Count)
                    {
                        filteredNodes = filteredNodes.Where(n => n.TeamsId != teamsNode.TeamsId).ToList();
                    }
                }
            }
            return filteredNodes;
        }

        public void FilterSitesModified(List<RMSPTreeNode> sites, out List<RMSPTreeNode> modifiedSites)
        {
            Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
            var modifiedDateCache = GetSiteModifiedDateCache(sites);
            List<string> notIncludeSiteIds = new List<string>();
            foreach (var site in sites)
            {
                if (!NeedCollectSPSite(modifiedDateCache, site, termScopeCache))
                {
                    notIncludeSiteIds.Add(site.SPObjectId);
                }
            }
            modifiedSites = sites.Where(n => !notIncludeSiteIds.Contains(n.SPObjectId)).ToList();
        }

        public RAReturnMessage RunExportTeamsSettingJob(ExportSettingType type,JobRunBy jobRunBy)
        {
            RAReturnMessage message = new();
            try
            {
                var teamsSettings = TeamsSettingDao.GetAllGroupSettings();
                if (type == ExportSettingType.OnlyExportCustomSettingNodes && (teamsSettings == null || teamsSettings.Count < 1))
                {
                    message.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_ExportSPSetting_NotSetting");
                    message.MessageType = RAMessageType.Failed;
                    return message;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportTeamsSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = type.ToString()
                };

                message.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportTeamsSettingJob,ERROR:{0}", ex.ToString());
            }
            return message;
        }

        public RAReturnMessage RunExportTeamsSOSettingJob(ExportSettingType type, JobRunBy jobRunBy)
        {
            RAReturnMessage message = new();
            try
            {
                var teamsArchiverSettings = RMArchiverSettingDao.LoadAllArchiverSettingWithType(ContentSourceType.Teams);
                if(type == ExportSettingType.OnlyExportCustomSettingNodes && (teamsArchiverSettings == null || teamsArchiverSettings.Count < 1))
                {
                    message.ErrorMessage = I18NEntity.GetString("RM_JS_BCM_ExportSPSetting_NotSetting");
                    message.MessageType = RAMessageType.Failed;
                    return message;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportTeamsSOSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = type.ToString()
                };

                message.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportTeamsSettingJob,ERROR:{0}", ex.ToString());
            }
            return message;
        }

        public string RunImportTeamsSettingJob(JobRunBy jobRunBy, string extension, string blobName)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportTeamsSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0} {1}", extension, blobName),
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportTeamsSettingJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ExportTeamsSOSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunExportTeamsSOSettingJobAsync(JobRunBy jobRunBy, string exportSettingType, string jobRunByUser = null)
        {
            return await RealRunExportSettingJobAsync(jobRunBy, exportSettingType, JobType.ExportTeamsSOSetting, jobRunByUser);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ExportTeamsSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunExportTeamsSettingJobAsync(JobRunBy jobRunBy, string exportSettingType, string jobRunByUser = null)
        {
            return await RealRunExportSettingJobAsync(jobRunBy, exportSettingType, JobType.ExportTeamsSetting, jobRunByUser);
        }

        public async Task<string> RealRunExportSettingJobAsync(JobRunBy jobRunBy, string exportSettingType,JobType jobType, string jobRunByUser)
        {

            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string jobId = RMJobMonitorService.CreateJob(jobType, jobRunByUser, account?.UserId);
            if (!Enum.TryParse<ExportSettingType>(exportSettingType, out ExportSettingType type))
            {
                type = ExportSettingType.OnlyExportCustomSettingNodes;
            }
            List<string> runningJobIds = RMJobMonitorService.GetRunningJobs(jobType);
            var skip = runningJobIds.Any(j => j != jobId);
            if (!skip)
            {
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = jobType == JobType.ExportTeamsSetting ? DownloadContentType.ExportSettings : DownloadContentType.ExportTeamsSOSetting,
                });
                logger.Info("Start to export teams setting");
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1} {2}", jobType, jobId, type),
                });
                return jobId;
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_ExportSPSetting_SkipJob");
                return "";
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ImportTeamsSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunImportTeamsSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes)
        {
            string jobId = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = RMJobMonitorService.CreateJob(JobType.ImportTeamsSetting, jobRunByUser, account?.UserId);
                logger.Info("Begin control Import Term Job {0}", jobId);
            }

            logger.Info("create import teams Setting job in job monitor.Id:{0}", jobId);
            List<string> importJobs = RMJobMonitorService.GetRunningJobs(JobType.ImportTeamsSetting);
            bool isSkip = importJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartImportTeamsSettingJob(jobId, extension, strBytes);
            }
            else
            {
                RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_ImportSPSetting_JobSkip");
                logger.Info(I18NEntity.GetString("RM_ImportSPSetting_JobSkip"));
            }

            return jobId;
        }
        private void StartImportTeamsSettingJob(string jobId, string extension, string strBytes)
        {
            string content = "\"" + strBytes + "\"";
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportTeamsSetting,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportTeamsSetting, jobId, extension, content),
            });
        }

        #endregion

        public string RunTeamsChannelSettingConflictCheckJob()
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsChannelSettingConflictCheck,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while run channel setting conflict job,ERROR:{0}", ex);
            }

            return id;
        }

        public string RunTeamsDataUpgradeJob()
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsDataUpgrade,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while run upgrade teams data job,ERROR:{0}", ex);
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsDataUpdateJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealRunTeamsDataUpgradeJob(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            try
            {
                var runningJobs = RMJobMonitorService.GetRunningJobs(JobType.TeamsDataUpgrade);
                string jobId = string.Empty;
                if (runningJobs.Count == 0)
                {
                    jobId = RMJobMonitorService.CreateJob(JobType.TeamsDataUpgrade, jobRunByUser);

                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        JobType = JobType.TeamsDataUpgrade,
                        RunBy = jobRunBy,
                        CommandLine = string.Format("{0} {1}", JobType.TeamsDataUpgrade, jobId),
                    });
                }
                else
                {
                    logger.Info($"{runningJobs.Count} teams data upgrade job.");
                }
                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while real run upgrade data job,ERROR:{0}", ex);
                return string.Empty;
            }
        }

        public string RunTeamsNodeSettingUpgradeJob()
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto()
                {
                    JobType = JobType.TeamsNodeSettingUpgrade,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while run upgrade teams setting job,ERROR:{0}", ex);
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsConflictJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealRunTeamsChannelSettingConflictCheckJob(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            try
            {
                string jobId = RMJobMonitorService.CreateJob(JobType.TeamsChannelSettingConflictCheck, jobRunByUser);

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.TeamsChannelSettingConflictCheck,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1}", JobType.TeamsChannelSettingConflictCheck, jobId),
                });

                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while real run upgrade channel setting job,ERROR:{0}", ex);
                return string.Empty;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunTeamsUpgradeJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealRunTeamsNodeSettingUpgradeJob(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            try
            {
                string jobId = RMJobMonitorService.CreateJob(JobType.TeamsNodeSettingUpgrade, jobRunByUser);

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.TeamsNodeSettingUpgrade,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1}", JobType.TeamsNodeSettingUpgrade, jobId),
                });

                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while real run upgrade teams setting job,ERROR:{0}", ex);
                return string.Empty;
            }
        }

        public TeamsChannelConflictQueryResult GetTeamsChannelConflictsList(TeamsChannelConflictQueryParameter queryParameter)
        {
            return TeamsChannelConflictSettingDao.GetTeamsChannelConflictSettingWithTotal(
                TenantLocalValue.LogonGroupId,
                queryParameter.ModuleType,
                queryParameter.PageSize,
                queryParameter.PageIndex,
                queryParameter.SortBy,
                queryParameter.IsAscending);
        }

        public string RunConflictSettingDetailExportJob()
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ConflictSettingDetailExport,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = TenantLocalValue.LogonUserId
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run upgrade channel setting job,ERROR:{0}", ex);
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunExportSettingConflictJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealRunConflictSettingDetailExportJob(JobRunBy jobRunBy, string jobRunByUser = null, string jobRunByUserId = null)
        {
            try
            {
                string jobId = RMJobMonitorService.CreateJob(JobType.ConflictSettingDetailExport, jobRunByUser);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = jobRunByUserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.ExportConflictSettingDetail
                });

                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.ConflictSettingDetailExport,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1}", JobType.ConflictSettingDetailExport, jobId),
                });

                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while real run upgrade channel setting job,ERROR:{0}", ex);
                return string.Empty;
            }
        }
    }
}

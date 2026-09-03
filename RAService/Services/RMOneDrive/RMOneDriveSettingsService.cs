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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Service.Services.SharePointSetting.AuditHandler;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.SharePoint.Discover;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Global = AvePoint.RA.Contract.Global.Object;

namespace AvePoint.RA.Service.Services.RMOneDrive
{
    [Audit]
    public class RMOneDriveSettingsService : BaseContentRepositorySettingsService, IRMOneDriveSettingsService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMOneDriveSettingsService));
        #region Interface
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        protected IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();
        protected IRMChangeClassificationDao RMChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        public IEXOSettingRuleDao EXOSettingRuleDao => PlatformWindsorManager.GetService<IEXOSettingRuleDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>(); 
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private IRMSettingJobDao RMSettingJobDao = PlatformWindsorManager.GetService<IRMSettingJobDao>();

        #endregion
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditOneDriveTermSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddTermSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Logger.Info("Set Term Setting");

                string siteCollectionId = Guid.Empty.ToString();
                RMSPTreeNode siteCollectionNode = null;
                if (settingNode.Level != (int)NodeLevel.WebApplication)
                {
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    siteCollectionId = siteCollectionNode.SPObjectId;
                }
                if (!CheckParentNodeDisable(settingNode, siteCollectionId))
                {
                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.OneDrive);
                    await OneDriveSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionId));
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
                Logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public async Task<RAReturnMessage> AddEnableRecordsManagementSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string siteCollectionId = Guid.Empty.ToString();
                RMSPTreeNode siteCollectionNode = null;
                if (settingNode.Level != (int)NodeLevel.WebApplication)
                {
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    siteCollectionId = siteCollectionNode.SPObjectId;
                }

                if (!CheckParentNodeDisable(settingNode, siteCollectionId, false))
                {

                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.OneDrive);
                    await OneDriveSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionId));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                OneDriveSettingDao.CheckNeedRemoveDescendantsSetting(settingNode, nodeProfileIdPath);
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
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public async Task<RAReturnMessage> AddIsShowUniqueIdSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            if (groupNode.Level != (int)NodeLevel.WebApplication)
            {
                return result;
            }
            try
            {
                Logger.Info("Set OneDrive Group UniqueId Setting");
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    UniqueIdSetting curUniqueIdSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    groupNode.SiteGroupId = new Guid(groupNode.Id);
                    await OneDriveSettingDao.AddOrUpdateCustomSettingAsync(groupNode, Guid.Empty);
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
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GeneralSetting4OneDrive, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOneDriveGeneralSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage enableResult = await AddEnableRecordsManagementSettingAsync(groupNode);
            RAReturnMessage isShowUniqueIdResult = new RAReturnMessage();
            if (groupNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                isShowUniqueIdResult = await AddIsShowUniqueIdSettingAsync(groupNode);
            }
            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isShowUniqueIdResult.MessageType == RAMessageType.Failed)
            {
                result = isShowUniqueIdResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }
            return result;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditOneDriveLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            result.MessageType = RAMessageType.Successful;
            try
            {
                string siteCollectionId = Guid.Empty.ToString();
                RMSPTreeNode siteCollectionNode = null;
                if (settingNode.Level != (int)NodeLevel.WebApplication)
                {
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    siteCollectionId = siteCollectionNode.SPObjectId;
                }

                Logger.Info("Set Location Owners OneDrive SharePoint Setting");

                if (!CheckParentNodeDisable(settingNode, siteCollectionId))
                {
                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    await OneDriveSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionId));
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
                Logger.Warn("Save Location Owners Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditOneDriveInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                Logger.Info("Inherit Parent Settings");
                string siteCollectionId = Guid.Empty.ToString();
                RMSPTreeNode siteCollectionNode = null;
                if (settingNode.Level != (int)NodeLevel.WebApplication)
                {
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    siteCollectionId = siteCollectionNode.SPObjectId;
                }

                await OneDriveSettingDao.DeleteOneDriveSettingAsync(new Guid(settingNode.SPObjectId), new Guid(siteCollectionId));
                CleanParentNodeSetting(settingNode);
                //Update the parent node setting to inherit settings. to do next.
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            string scopeIdString = string.Empty;
            var isDisableRecordsManagement = false;
            if (settingNode.DisposeScheduleInfo != null && (settingNode.DisposeScheduleInfo.JobCategory == ScheduleType.SPArchiveJobSchedule || settingNode.DisposeScheduleInfo.JobCategory == ScheduleType.OneDriveArchiveJobSchedule))
            {
                isDisableRecordsManagement = false;
            }
            else
            {
                try
                {
                    Expression<Func<RMOneDriveSetting, bool>> whereLambda = this.GetCheckDisableLambda(settingNode, SPObjectId, isCheckSelfNode);
                    Logger.Debug($"CheckParentNodeDisable where lambda: {whereLambda}");
                    if (OneDriveSettingDao.GetParentNode(whereLambda) != null)
                    {
                        isDisableRecordsManagement = true;
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error("Check Parent Node Records Management error:{0}", ex.ToString());
                }
            }
            return isDisableRecordsManagement;
        }

        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        public async Task<RMSPTreeNode> LoadNodeSettingAsync(RMSPTreeNode sNode)
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
            #endregion

            try
            {
                RMSPTreeNode groupNode = sNode;
                while (groupNode.Level != (int)NodeLevel.WebApplication && groupNode != null)
                {
                    groupNode = groupNode.Parent;
                }
                if (groupNode == null)
                {
                    return configNode;
                }
                //var groupNode = GetGroupNode(configNode);
                Guid groupId = Guid.Empty;
                bool folderDisable = false;
                string GlobalColumnName = string.Empty;
                string GlobalColumnNameDesc = string.Empty;
                if (groupNode != null)
                {
                    groupId = new Guid(groupNode.SPObjectId);
                }
                Guid siteId = Guid.Empty;
                if (sNode.Level != (int)NodeLevel.WebApplication)
                {
                    var scNode = sNode.GetSiteCollectionNode();
                    siteId = new Guid(scNode.SPObjectId);
                }
                var GSetting = OneDriveSettingDao.LoadOneDriveSetting(groupId, Guid.Empty);

                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }
                    configNode.TermSetId = GSetting.TermSetId;
                    configNode.TermSetName = GSetting.TermSetName;
                    configNode.TermId = GSetting.TermId;
                    configNode.TermName = GSetting.TermName;
                    configNode.DefaultTermId = GSetting.DefaultTermId;
                    configNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                    configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);

                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    configNode.isFailedConfigClassification = GSetting.IsFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = GSetting.IsFailedConfigMetaDataColumn;
                    configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = GSetting.ApplyExistType;
                    configNode.ApprovalType = (int)GSetting.ApprovalType;
                    configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;
                    configNode.IsNullClassificationSetting = GSetting.IsNullClassificationSetting;
                    configNode.Rules = EXOSettingRuleDao.GetOneDriveMappingRules(groupId, siteId);
                    if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                    }
                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    //configNode.RecordOwner = GetSettingRecordOnwers(GSetting.Id, SourceType.SharePoint);
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.OneDrive);
                    configNode.SiteGroupId = GSetting.SiteGroupId;
                    //configNode.ProfileId = GSetting.IdPath;
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
                    if (sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Folder)
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
                    configNode.IsShowUniqueId = GSetting.IsShowUniqueId == null ? true : (bool)GSetting.IsShowUniqueId;

                    configNode.AITermUseType = GSetting.AITermUseType;
                    configNode.AIApprovalType = (int)GSetting.AIApprovalType;
                    configNode.AISendEMail = GSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.AIOneDrive);
                    configNode.AIThenIsDefaultTermMethod = GSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = GSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = GSetting.AIThenDefaultTermName;
                    //SetDisposeJob(configNode, GSetting.DisposalJobId1);
                    //SetCollectionJob(configNode, GSetting.CollectionJobId1);
                }              

               
                var spSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(sNode.SPObjectId), siteId);
                if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                {
                    var pNode = LoadFolderParentSeting(sNode, siteId);
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
                    if (sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.Folder)
                    {
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, siteId);
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
                    RMTermSet termSet = null;
                    if (spSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(spSetting.TermSetId);
                    }

                    configNode.DefaultTermId = spSetting.DefaultTermId;
                    configNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                    configNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                    configNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                    configNode.TermId = spSetting.TermId;
                    configNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                    configNode.TermSetId = spSetting.TermSetId;
                    configNode.TermSetName = spSetting.TermSetName;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                    configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                    configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                    configNode.isFailedConfigClassification = spSetting.IsFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = spSetting.IsFailedConfigMetaDataColumn;
                    configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = spSetting.ApplyExistType;
                    configNode.ApprovalType = (int)spSetting.ApprovalType;
                    configNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;

                    if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    }
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.OneDrive);
                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
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

                    configNode.AITermUseType = spSetting.AITermUseType;
                    configNode.AIApprovalType = (int)spSetting.AIApprovalType;
                    configNode.AISendEMail = spSetting.AISendEMail;
                    configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.AIOneDrive);
                    configNode.AIThenIsDefaultTermMethod = spSetting.AIThenIsDefaultTermMethod;
                    configNode.AIThenDefaultTermId = spSetting.AIThenDefaultTermId;
                    configNode.AIThenDefaultTermName = spSetting.AIThenDefaultTermName;
                }

                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.OneDriveDisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.IsEnableSuperUserDecrypt = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableSuperUserDecrypt).GetValueOrDefault();
                    configNode.IsEnableRemoveRetentionLabel = (JsonConvert.DeserializeObject<RMSPTreeNode>(disposeSchedule.Extentions)?.IsEnableRemoveRetentionLabel).GetValueOrDefault();
                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    configNode.IconStatus = IconStatus.Break;
                }
                else
                {
                    var ancestryDisposeSchedule = await  ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.OneDriveDisposalSchedule);
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
                Logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }

        public async System.Threading.Tasks.Task LoadSettingIconAsync(List<RMSPSampleTreeNode> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMSPSampleTreeNode tempNode = nodes[0];
                    if (tempNode.Level == (int)NodeLevel.Farm)
                    {
                        return;
                    }
                    RMSPSampleTreeNode groupNode = tempNode;
                    if (groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        while (groupNode.Level != (int)NodeLevel.WebApplication && groupNode != null)
                        {
                            groupNode = groupNode.Parent;
                        }

                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.SPObjectId);
                        }
                        var gsSetting = OneDriveSettingDao.LoadOneDriveSetting(groupId, Guid.Empty);
                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.OneDriveDisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMOneDriveSetting>();
                        var settings = OneDriveSettingDao.LoadOneDriveSettings(groupId).OrderBy(item => item.Id);
                        foreach (var setting in settings)
                        {
                            var key = setting.ScopeId.ToString() + setting.SiteId.ToString();
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
                            RMOneDriveSetting csSetting = null;
                            var settingKey = node?.SPObjectId + siteNode?.SPObjectId;
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
                            var selfGSSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(selfGroupNode.SPObjectId), Guid.Empty);
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
                                await LoadSettingIconAsync(selfGroupNode.Children);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        #region Privete Method
        private void SetPropertiesByNodeLevel(RMSPTreeNode settingNode, RMSPTreeNode siteCollectionNode)
        {
            if (settingNode.Level == (int)NodeLevel.Folder)
            {
                settingNode.FolderId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id
                settingNode.FullPath = WebUtil.MakeFullUrl(siteCollectionNode.FullPath, settingNode.FullPath);
            }
            if (settingNode.Level == (int)NodeLevel.List || settingNode.Level == (int)NodeLevel.Library)
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
        }

        private RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                while (node.Level != (int)NodeLevel.SiteCollection)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
            else
            {
                return node;
            }
        }

        private RMSPTreeNode GetListNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }

        private RMSPTreeNode GetWebNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.Site)
            {
                node = node.Parent;
            }
            return node;
        }

        private Expression<Func<RMOneDriveSetting, bool>> GetCheckDisableLambda(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMOneDriveSetting), "c");
            List<Expression> nodeIdExpressionList = new List<Expression>();
            List<Guid> scopeIds = GetParentScopeId(settingNode, isCheckSelfNode);
            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMOneDriveSetting), param, "ScopeId", scopeIds));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMOneDriveSetting), param, "EnableRecordManagement", (int)EnableRecordManagementSetting.Disable));
            if (SPObjectId == null || SPObjectId == "")
            {
                SPObjectId = Guid.Empty.ToString();
            }
            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMOneDriveSetting), param, "SiteId", new List<object> { new Guid(SPObjectId), Guid.Empty }));
            var groupNode = settingNode.GetGroupNode();
            if (groupNode != null)
            {
                allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMOneDriveSetting), param, "SiteGroupId", new Guid(groupNode.SPObjectId)));
            }
            queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<RMOneDriveSetting, bool>>(queryExpr, param);
        }

        private List<Guid> GetParentScopeId(RMSPTreeNode settingNode, bool isCheckSelfNode)
        {
            List<Guid> scopeIds = new List<Guid>();
            if (isCheckSelfNode)
            {
                scopeIds.Add(new Guid(settingNode.SPObjectId));
            }
            while (settingNode.Parent != null && settingNode.Parent.SPObjectId != null)
            {
                scopeIds.Add(new Guid(settingNode.Parent.SPObjectId));
                settingNode = settingNode.Parent;
            }
            return scopeIds;
        }

        private void CleanParentNodeSetting(RMSPTreeNode node)
        {
            do
            {
                if (OneDriveSettingDao.CleanSettingJobTime(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }

        private RMOneDriveSetting LoadSampleNodeParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMOneDriveSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(node.SPObjectId), siteId);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadSampleNodeParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        private RMOneDriveSetting LoadFolderParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMOneDriveSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(node.SPObjectId), Guid.Empty);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(node.SPObjectId), siteId);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, siteId);
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

        #endregion

        #region data sync job

        public RAReturnMessage RunRecordsDisposalJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            Logger.Debug("start onedrive disposal job");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();


            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            var indexDevice = StorageDeviceService.GetIndexDevice();
            if (indexDevice == null)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunJob_Failed_NoIndexDeviceSetting");
                return msg;
            }
            if (selectedTree != null)
            {
                List<JobType> types = new List<JobType>() { JobType.RMArchiverBackup, JobType.SpecifySitesArchiverBackup, JobType.OneDriveRecordsDisposal, JobType.RMEndUserArchiverBackup };
                if (RMJobService.HasRunningArchiverJobOnScope(types, selectedTree.FullPath))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_Job_ScheduledJobConflict");
                    Logger.Warn($"Already has a job running on current node:{selectedTree.FullPath}");
                    return msg;
                }
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                //var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.OneDriveRecordsDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while running onedrive data sync job,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunOneDriveDisposalJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.OneDriveRecordsDisposal;
            RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
            return RealRunRecordsDisposalJobBySelectedNodeAsync(jobRunByUser, jobType, selectedNode);
        }

        private async Task<string> RealRunRecordsDisposalJobBySelectedNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            List<JobType> types = JobTypeConstants.ArchiveSiteConflictType;
            Dictionary<Guid, RMOneDriveSetting> gruopSetingMap = new Dictionary<Guid, RMOneDriveSetting>();
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            string nodeUrl = selectedNode.FullPath;
            string folderFullPath = "";
            if (selectedNode.Level == (int)NodeLevel.Folder && !nodeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (siteNode != null)
                {
                    nodeUrl = WebUtil.MakeFullUrl(selectedNode.GetSiteCollectionNode().FullPath, selectedNode.FullPath);
                    folderFullPath = nodeUrl;
                }
            }
            
            List<RMSPTreeNode> availableNode = await this.AssembleLFScheduleDataRunnableNodeAsync(selectedNode);

            if (availableNode.IsNullOrEmpty())
            {
                Logger.Warn("No available sc to run");
                jobId = RMJobService.CreateJobWithScopeId(JobType.OneDriveRecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                if (jobType == JobType.OneDriveRecordsDisposal)
                {
                    RMJobService.SetSumSCCountOfJobExtension(0, jobId);
                    Logger.Info("Initialize extension for main job {0} ,support job run failed.", jobId);
                }
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JMD_DisableRecordManagement_Or_HasOwnSettingMessage");
                return jobId;
            }

            if(availableNode.All(site => RemoteNodeService.ValidOrphenSiteCollection(site)))
            {
                Logger.Warn("all is orphaned od");
                jobId = RMJobService.CreateJobWithScopeId(JobType.OneDriveRecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JMD_DisableRecordManagement_Or_HasOwnSettingMessage");
                return jobId;
            }

            availableNode = availableNode.Where(site => !RemoteNodeService.ValidOrphenSiteCollection(site)).ToList();

            var runningUrls = RMJobService.GetRunningArchiverJobSiteUrl(types, availableNode.Select(n => n.GetSiteCollectionNode().FullPath));
            availableNode = RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(availableNode, runningUrls, selectedNode, folderFullPath);
            if (availableNode.Count == 0)
            {
                Logger.Warn($"Current has job running on same scope.{nodeUrl}");
                jobId = RMJobService.CreateJobWithScopeId(JobType.OneDriveRecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode));
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            jobId = RMJobService.CreateJobWithScopeId(JobType.OneDriveRecordsDisposal, jobRunByUser, nodeUrl, GetSPContainerId(selectedNode),null, RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNode, TreeMode.LifeOD));
            List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                Logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            try
            {
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    var groupLevelSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(selectedNode.SPObjectId), Guid.Empty);
                    gruopSetingMap.Add(new Guid(selectedNode.Id), groupLevelSetting);
                }
                RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetOneDriveRuleIds(selectedNode));
            }
            catch (Exception ex)
            {
                Logger.Error($"error occurred while check job conflict and add job rule mapping for disposal job, error:{ex}");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            if (subJobCount > 0)
            {
                RMJobService.SetSumSCCountOfJobExtension(subJobCount, jobId);
                Logger.Info("Initialize extension for main job {0}, sub job count by selected node level {1}, estimated site count {2}.", jobId, selectedNode.Level, subJobCount);
            }
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, tempList, false, site.FullPath, site.O365TenantId);
                tempList.Clear();
                currentSubjobIndex++;
            }
            return jobId;
        }

        private List<Guid> GetOneDriveRuleIds(RMSPTreeNode tree)
        {
            if (tree.IsNullClassificationSetting && tree.Rules?.Count > 0)
            {
                return tree.Rules.Select(r => r.RuleId).Distinct().ToList();
            }

            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = RuleManagerService.GetRulesFromRecords();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> oneDriveRules = rules.AsQueryable().Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters.Count != 0).ToList();
            return TermRuleAssociationDao.GetTermWithRuleLevel(tree.Level, oneDriveRules).Select(r => r.RuleId).Distinct().ToList();
        }
     
        public RAReturnMessage RunDataSyncJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            Logger.Debug("start onedrive data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();


            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!IsExistCanRunJobNodes(selectedTree))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_EXO_SyncData_NoSC");
                    return msg;
                }

                if (IsNullClassificationNode(selectedTree))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = string.Format(I18NEntity.GetString("RM_EXO_GroupIsRuleSettingAndSkipApplySetting"), selectedTree.Name);
                    return msg;
            }
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.OneDriveDataSynchronisation,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while running onedrive data sync job,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        private bool IsExistCanRunJobNodes(RMSPTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                if (IsEnableRecordManagement(selectedTree) /*&& IsHaveAvailableNodes(selectedTree)*/)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsNullClassificationNode(RMSPTreeNode tree)
        {
            var groupNode = tree.GetGroupNode();
            var setting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(groupNode.SPObjectId), Guid.Empty);
            return setting != null && setting.IsNullClassificationSetting;
        }

        private bool IsEnableRecordManagement(RMSPTreeNode selectedTree)
        {
            Guid siteId = Guid.NewGuid();
            Guid siteGroupId = Guid.NewGuid();
            RMOneDriveSetting setting = null;

            //当前只有两个类型的结点可以启动Sync Job: 一类是Group,一类是SiteCollection
            int cnt = 6;
            do
            {
                switch ((NodeLevel)selectedTree.Level)
                {
                    case NodeLevel.WebApplication:
                        {
                            siteId = Guid.Empty;
                            siteGroupId = Guid.Parse(selectedTree.SPObjectId);
                            break;
                        }
                    case NodeLevel.SiteCollection:
                        {
                            siteId = Guid.Parse(selectedTree.SPObjectId);
                            siteGroupId = selectedTree.SiteGroupId;
                            break;
                        }
                }
                setting = OneDriveSettingDao.GetSettingInfoByScope(siteGroupId, siteId, Guid.Parse(selectedTree.SPObjectId));
                selectedTree = selectedTree.Parent;
            }
            while (setting == null && selectedTree != null && cnt-- > 0);

            if (setting == null || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                Logger.Info($"IsEnableRecordManagement:setting==null:{setting == null}");
                return false;
            }
            Logger.Info($"IsEnableRecordManagement:{true}");
            return true;
        }

        /*private async Task<bool> IsHaveAvailableNodesAsync(RMSPTreeNode selectedTree)
        {
            List<RMSPTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeAsync(selectedTree);
            if (lstAvailableNodes == null || lstAvailableNodes.Count() <= 0)
            {
                return false;
            }
            return true;
        }*/

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4OneDrive, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.OneDriveDataSynchronisation;
            if (string.IsNullOrEmpty(param))
            {
                return await RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
                return await RunDataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        private async Task<string> RunDataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            Dictionary<Guid, RMOneDriveSetting> gruopSetingMap = new Dictionary<Guid, RMOneDriveSetting>();
            Dictionary<Guid, RMOneDriveSetting> settingsMap = new Dictionary<Guid, RMOneDriveSetting>();

            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB( (int)jobType);
            jobId = RMJobService.CreateJob(JobType.OneDriveDataSynchronisation, jobRunByUser, GetSPContainerId(selectedNode));
            List<RMSPTreeNode> availableNode = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
            //remove sites that not changed since last job
            bool noContentModified = false;
            try
            {
            if (availableNode.Count > 1)
            {
                using (var performance = new PerformanceScope("RMOneDriveSettingsService.FilterNoContentModifiedSites"))
                {
                    var modifiedDateCache = GetSiteModifiedDateCache(availableNode);
                    List<string> notIncludeSiteIds = new List<string>();
                    Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                    foreach (var node in availableNode)
                    {
                        if (!NeedCollectOneDriveSite(modifiedDateCache, node, termScopeCache))
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
                    Logger.Warn("No content modified under sites.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    Logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                }
                return jobId;
            }
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                var groupLevelSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(selectedNode.SPObjectId), Guid.Empty);
                gruopSetingMap.Add(new Guid(selectedNode.Id), groupLevelSetting);
                settingsMap.Add(new Guid(selectedNode.Id), groupLevelSetting);
                await OneDriveSettingDao.SetSettingJobTimeAsync(new Guid(selectedNode.Id), Guid.Empty);
                SaveJobSetting(jobId, selectedNode, jobType, settingsMap);
            }
                else if (selectedNode.Level == (int)NodeLevel.SiteCollection)
            {
                var nodeSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(selectedNode.SPObjectId), new Guid(selectedNode.Id));
                    if (nodeSetting != null)
                {
                    settingsMap.Add(new Guid(selectedNode.Id), nodeSetting);
                }
            }
                var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
            foreach (var node in availableNode)
            {
                node.PredictionModeType = isZeroShotMode ? PredictionModeType.ZeroShot : PredictionModeType.MLTraining;
            }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while loading settings. JobId:{0} Error:{1}", jobId, ex.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        mJobQueueService.HandleMessage(new JobQueueMessage()
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

                #region Store job settings to db.
                SaveJobSetting(jobId, site, jobType, settingsMap);
                #endregion
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
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

        private void SaveJobSetting(string jobId, RMSPTreeNode node, JobType jobType, Dictionary<Guid, RMOneDriveSetting> settingsMap)
        {
            var settingsPerContainer = settingsMap.Where(s => s.Key == new Guid(node.Id)).Select(v => v.Value).ToList();
            Logger.Info("Begin store job setting, JobId: {0}, Site Container: {1}", jobId, node.Id);
            var isExist = RMSettingJobDao.GetRMSettingJob(item => item.Id == jobId && item.JobType == (int)jobType) != null;
            if (!isExist)
            {
                RMSettingJobInfo settingJobInfo = new RMSettingJobInfo
                {
                    Id = jobId,
                    JobType = (int)JobType.OneDriveDataSynchronisation,
                    JobInfos = SerializerHelper.SerializeByDataContractSerializer(settingsPerContainer),
                };

                RMSettingJobDao.AddRMSettingJob(settingJobInfo);
            }
            Logger.Info("Finishing stored job setting, JobId: {0}, Site Container: {1}", jobId, node.Id);
        }


        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Id;
            }
            else
            {
                return GetSPContainerId(selectedNode.Parent);
            }
        }

        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Name;
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }

        private async Task<List<RMSPTreeNode>> AssembleSyncDataRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                await LoadOneDriveSettingUnderGroupAsync(sites, selectedNode);
                //this.LoadOneDriveSetting(sites);
                foreach (RMSPTreeNode site in sites)
                {
                    //TODO Need Derek Review
                    if (/*site.IsSyncData && */site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)//RECO-3282  RECO-3268
                    //if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    {
                        if (RemoteNodeService.ValidOrphenSiteCollection(site))
                        {
                            Logger.Info($@"Skip orphen OD in AssembleSyncDataRunnableNodeAsync method, od:{site.FullPath}");
                            continue;
                        }
                        availableNode.Add(site);
                    }
                }
            }
            else
            {
                //TODO Need Derek Review
                if (ValidateSiteExist(selectedNode.GetSiteCollectionNode()) && selectedNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    if (RemoteNodeService.ValidOrphenSiteCollection(selectedNode))
                    {
                        Logger.Info($@"Skip orphen OD in AssembleSyncDataRunnableNodeAsync method, od:{selectedNode.FullPath}");
                    }
                    else
                    {
                        availableNode.Add(selectedNode);
                    }
                }
                else
                {
                    Logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }
        private async Task<List<RMSPTreeNode>> AssembleLFScheduleDataRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            List<string> mBreakTreeNode = new List<string>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                var parentId = ScheduleService.GetProfileId(selectedNode) + "|";
                var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                foreach (var item in treeNodes)
                {

                    var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        continue;
                    }
                    mBreakTreeNode.Add(node.FullPath);
                }
                await LoadOneDriveSettingUnderGroupAsync(sites, selectedNode);
                //this.LoadOneDriveSetting(sites);
                foreach (RMSPTreeNode site in sites)
                {
                    //TODO Need Derek Review
                    if (/*site.IsSyncData && */site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && !mBreakTreeNode.Contains(site.FullPath))//RECO-3282  RECO-3268
                    //if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    {
                        availableNode.Add(site);
                    }
                }
            }
            else
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                //TODO Need Derek Review
                if (ValidateSiteExist(siteNode) && selectedNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    selectedNode.O365TenantId = siteNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    Logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                Logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        public async System.Threading.Tasks.Task LoadOneDriveSettingAsync(List<RMSPTreeNode> nodes)
        {
            try
            {
                foreach (var node in nodes)
                {
                    bool ownSetting = true;
                    var groupNode = GetGroupNode(node);
                    Guid groupId = Guid.Empty;
                    string GlobalColumnName = string.Empty;
                    bool folderDisable = false;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var GSetting = OneDriveSettingDao.LoadOneDriveSetting(groupId, Guid.Empty);
                    if (GSetting != null)
                    {
                        //GlobalColumnName = GSetting.ColumnName;
                        var termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        // var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                        // node.ColumnName = GlobalColumnName;
                        //node.ExistColumnName = GSetting.ExistColumnName;
                        //node.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                        //node.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                        node.TermSetName = GSetting.TermSetName;
                        node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                        node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                        //node.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.isFailedConfigClassification = GSetting.IsFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = GSetting.IsFailedConfigMetaDataColumn;
                        //node.IsTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        //node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        //node.EnableRelatedRecords = GSetting.rela;
                        node.EnableRecordManagement = GSetting.EnableRecordManagement;
                        // node.isEnableClassification = GSetting.EnableRecordManagement;
                    }
                    var siteNode = GetSiteCollectionNode(node);
                    Guid siteId = Guid.Empty;
                    if (siteNode != null)
                    {
                        siteId = new Guid(siteNode.SPObjectId);
                    }
                    var SPSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(node.SPObjectId), siteId);
                    if (SPSetting != null && (SPSetting.TermId != Guid.Empty || SPSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                    {
                        node.HasCustomSetting = true;
                    }
                    else
                    {
                        node.HasCustomSetting = false;
                    }

                    if (SPSetting != null)
                    {
                        node.IsCustomSetting = true;
                    }
                    if (node.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                    {
                        var pNode = LoadFolderParentSeting(node, siteId);
                        if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                        {
                            if (SPSetting != null)
                            {
                                SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                            folderDisable = true;
                        }
                    }

                    if (SPSetting == null)
                    {
                        if (node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.Folder)
                        {
                            SPSetting = LoadParentSeting(node.Parent, siteId);
                            if (SPSetting != null && node.Level != (int)NodeLevel.WebApplication)
                            {
                                if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                                {
                                    SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                                }

                            }
                        }
                    }
                    //else
                    //{
                    //    node.IsCustomSetting = true;
                    //}



                    if (SPSetting != null)
                    {
                        var termScope = TermDao.GetRMTermByGuId(SPSetting.TermId);
                        var defaultTerm = TermDao.GetRMTermByGuId(SPSetting.DefaultTermId);
                        //var containerTerm = TermDao.GetRMTermByGuId(SPSetting.TermIdOfContainer);

                        //node.ColumnName = GlobalColumnName;
                        //node.Description = SPSetting.Description;
                        node.DefaultTermId = SPSetting.DefaultTermId;
                        node.DefaultTermName = defaultTerm == null ? SPSetting.DefaultTermName : defaultTerm.Name;
                        node.DefaultTermNameFullPath = defaultTerm == null ? SPSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(SPSetting.DefaultTermId);
                        node.TermId = SPSetting.TermId;
                        node.TermName = termScope == null ? SPSetting.TermName : termScope.Name;
                        node.TermNameFullPath = termScope == null ? SPSetting.TermName : TermDao.GetTermFullPathByTermId(SPSetting.TermId);
                        node.TermSetId = SPSetting.TermSetId;
                        node.TermSetName = SPSetting.TermSetName;
                        node.IsTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                        node.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                        //node.des = SPSetting.DescriptionOfContainer;
                        //node.TermIdOfContainer = SPSetting.TermIdOfContainer;
                        //node.TermNameOfContainer = containerTerm == null ? SPSetting.TermNameOfContainer : containerTerm.Name;
                        //node.isEnableClassification = SPSetting.;
                        node.EnableRecordManagement = SPSetting.EnableRecordManagement;
                        //node.IsEnableHoldPhyical = SPSetting.hold;
                        node.isFailedConfigClassification = SPSetting.IsFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = SPSetting.IsFailedConfigMetaDataColumn;
                        //node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        //node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        //node.ExistColumnName = SPSetting.ExistColumnName;
                        //node.IsUsingExistColumnName = SPSetting.IsUsingExistColumnName;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(SPSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.EMailToRecordOwner = SPSetting.EMailToRecordOwner;
                        // node.IsDisplyaTermPath = SPSetting.IsDisplyaTermPath;
                        //node.EnableRelatedRecords = SPSetting.EnableRelatedRecords;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public async System.Threading.Tasks.Task LoadOneDriveSettingUnderGroupAsync(List<RMSPTreeNode> nodes, RMSPTreeNode groupNode)
        {
            try
            {
                Logger.Info($"Begin to load onedive settings for group:{groupNode.FullPath} Site collection count:{nodes.Count}");
                using (var performance = new PerformanceScope("RMOneDriveSettingsService.LoadOneDriveSettingUnderGroup"))
                {
                    Guid groupId = Guid.Empty;
                    string GlobalColumnName = string.Empty;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var GSetting = OneDriveSettingDao.LoadOneDriveSetting(groupId, Guid.Empty);

                    RMTerm termScope = null;
                    string groupTermFullPath = string.Empty;
                    bool groupTermExpired = false;
                    List<Contract.RMWeb.ReportCenter.ToUserInfo> groupRecordOwner = null;
                    if (GSetting != null)
                    {
                        termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        if (termScope != null)
                        {
                            groupTermFullPath = TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                            groupTermExpired = TermDao.IsExpiredTerm(termScope.Id);
                        }
                        groupRecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                    }
                    List<RMOneDriveSetting> settings;
                    using (var performance0 = new PerformanceScope("RMOneDriveSettingsService.LoadOneDriveSettings"))
                    {
                        settings = OneDriveSettingDao.LoadOneDriveSettings(groupId);
                    }
                    foreach (var node in nodes)
                    {
                        ArgumentCheck.NotNull(node, nameof(node));
                        var siteNode = node;
                        Guid siteId = Guid.Empty;
                        if (siteNode != null)
                        {
                            siteId = new Guid(siteNode.SPObjectId);
                        }
                        var SPSetting = settings.Where(s => s.ScopeId == siteId && s.SiteId == siteId).FirstOrDefault();
                        if (SPSetting == null)
                        {
                            if (GSetting != null)
                            {
                                node.TermSetName = GSetting.TermSetName;
                                node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                                node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : groupTermFullPath;
                                node.RecordOwner = groupRecordOwner;
                                node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                                node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || groupTermExpired;
                                node.isFailedConfigClassification = GSetting.IsFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = GSetting.IsFailedConfigMetaDataColumn;
                                node.EnableRecordManagement = GSetting.EnableRecordManagement;
                                node.ApprovalType = (int)GSetting.ApprovalType;
                            }
                        }
                        else
                        {
                            if (SPSetting != null && (SPSetting.TermId != Guid.Empty || SPSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                            {
                                node.HasCustomSetting = true;
                            }
                            else
                            {
                                node.HasCustomSetting = false;
                            }

                            if (SPSetting != null)
                            {
                                node.IsCustomSetting = true;
                            }

                            if (SPSetting != null)
                            {
                                var siteTermScope = TermDao.GetRMTermByGuId(SPSetting.TermId);
                                var siteDefaultTerm = TermDao.GetRMTermByGuId(SPSetting.DefaultTermId);
                                node.DefaultTermId = SPSetting.DefaultTermId;
                                node.DefaultTermName = siteDefaultTerm == null ? SPSetting.DefaultTermName : siteDefaultTerm.Name;
                                node.DefaultTermNameFullPath = siteDefaultTerm == null ? SPSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(SPSetting.DefaultTermId);
                                node.TermId = SPSetting.TermId;
                                node.TermName = siteTermScope == null ? SPSetting.TermName : siteTermScope.Name;
                                node.TermNameFullPath = siteTermScope == null ? SPSetting.TermName : TermDao.GetTermFullPathByTermId(SPSetting.TermId);
                                node.TermSetId = SPSetting.TermSetId;
                                node.TermSetName = SPSetting.TermSetName;
                                node.IsTermRemoved = siteTermScope == null ? false : siteTermScope.IsRemoved;
                                node.IsDefaultTermRemoved = siteDefaultTerm == null ? false : siteDefaultTerm.IsRemoved;
                                node.IsTermDeprecated = siteTermScope == null ? false : siteTermScope.IsDeprecated || TermDao.IsExpiredTerm(siteTermScope.Id);
                                node.IsDefaultTermDeprecated = siteDefaultTerm == null ? false : siteDefaultTerm.IsDeprecated || TermDao.IsExpiredTerm(siteDefaultTerm.Id);
                                node.EnableRecordManagement = SPSetting.EnableRecordManagement;
                                node.isFailedConfigClassification = SPSetting.IsFailedConfigClassification;
                                node.isFailedConfigMetaDataColumn = SPSetting.IsFailedConfigMetaDataColumn;
                                node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(SPSetting.Id, RecordOwnerSettingType.SharePoint);
                                node.EMailToRecordOwner = SPSetting.EMailToRecordOwner;
                                node.ApprovalType = (int)SPSetting.ApprovalType;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public RAReturnMessage RunOneDriveDataSyncScheduleJob(JobRunBy jobRunBy)
        {
            Logger.Debug("start onedrive all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.OneDriveDataSynchronisationSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while SP DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4OneDrive, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunOneDriveDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.OneDriveDataSynchronisation : JobType.OneDriveDataSynchronisationSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);
            //Skip if a schedule job is running
            List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.OneDriveDataSynchronisationSchedule);
            if (!runningJobIds.IsNullOrEmpty())
            {
                Logger.Info("Current running scheduled onedrive data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A OneDrive Data Synchronization job is already running.");
                return jobId;
            }
            else
            {
                return await RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
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

        public RMOneDriveSetting LoadParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMOneDriveSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(node.SPObjectId), siteId);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        private async Task<string> RunSPDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            Dictionary<Guid, RMOneDriveSetting> gruopSetingMap = new Dictionary<Guid, RMOneDriveSetting>();
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB( (int)jobType);
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            //TODO Need Derek Review
            var allSetting = OneDriveSettingDao.LoadAllSetting()/*.Where(s => s.IsSyncData)*/;

            if (allSetting.IsNullOrEmpty())
            {
                Logger.Warn("There is no site collection setting enable sync data into Explorer.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncSCUnderGroup");
                return jobId;
            }

            Dictionary<string, string> OneDriveRuleSettingContainers = new Dictionary<string, string>();
            try
            {
            var enableNullClassificationGroupIds = allSetting.Where(s => s.SiteGroupId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.SiteGroupId.ToString()).ToList();
            foreach (var setting in allSetting)
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);

                var groupNode = selectedNode.GetGroupNode();
                if (enableNullClassificationGroupIds != null && enableNullClassificationGroupIds.Count > 0 && enableNullClassificationGroupIds.Contains(groupNode.SPObjectId))
                {
                    Logger.Info("Onedrive group enable null classification, site:{0}", selectedNode.Name);
                    if (!OneDriveRuleSettingContainers.ContainsKey(groupNode.SPObjectId))
                    {
                        OneDriveRuleSettingContainers.Add(groupNode.SPObjectId, GetSPContainerName(groupNode));
                    }
                    continue;
                }
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    var groupSetting = OneDriveSettingDao.LoadOneDriveSetting(new Guid(selectedNode.SPObjectId), Guid.Empty);
                    gruopSetingMap.Add(new Guid(selectedNode.Id), groupSetting);
                    await OneDriveSettingDao.SetSettingJobTimeAsync(new Guid(selectedNode.Id), Guid.Empty);
                }

                if (selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                    if (site == null)
                    {
                        Logger.Info("Onedrive site not exist, site:{0}", selectedNode.Name);
                        continue;
                    }

                    if (!site.parentId.Equals(setting.SiteGroupId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info("Onedrive site has been moved to other container, site:{0}", selectedNode.Name);
                        continue;
                    }
                }

                if (selectedNode.Level == (int)NodeLevel.WebApplication || selectedNode.Level == (int)NodeLevel.SiteCollection)
                {
                    var tempNodes = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
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
                using (var performance = new PerformanceScope("RMOneDriveSettingsService.FilterNoContentModifiedSites"))
                {
                    var modifiedDateCache = GetSiteModifiedDateCache(availableNode);
                    List<string> notIncludeSiteIds = new List<string>();
                    Dictionary<Guid, List<Guid>> termScopeCache = new Dictionary<Guid, List<Guid>>();
                    foreach (var node in availableNode)
                    {
                        if (!NeedCollectOneDriveSite(modifiedDateCache, node, termScopeCache))
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
                if (OneDriveRuleSettingContainers != null && OneDriveRuleSettingContainers.Count > 0)
                {
                    Logger.Warn($"Onedrive group enable null classification. Skip run job. Group name:{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                }
                else if (noContentModified)
                {
                    Logger.Warn("No content modified under sites.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished);
                }
                else
                {
                    Logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroupBySchedule");
                }
                return jobId;
            }
            var isZeroShotMode = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                foreach (var node in availableNode)
            {
                node.PredictionModeType = isZeroShotMode ? PredictionModeType.ZeroShot : PredictionModeType.MLTraining;
            }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            jobType = JobType.OneDriveDataSynchronisation;
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        mJobQueueService.HandleMessage(new JobQueueMessage()
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
                #region Store job settings to db.
                var settingsPerContainer = allSetting.Where(s => s.ScopeId == new Guid(site.Id)).ToList();
                Logger.Info("Begin store job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, site.Id, settingsPerContainer.Count);
                var isExist = RMSettingJobDao.GetRMSettingJob(item => item.Id == jobId && item.JobType == (int)jobType) != null;
                if (!isExist)
                {
                    RMSettingJobInfo settingJobInfo = new RMSettingJobInfo
                    {
                        Id = jobId,
                        JobType = (int)JobType.OneDriveDataSynchronisation,
                        JobInfos = SerializerHelper.SerializeByDataContractSerializer(settingsPerContainer),
                    };

                    RMSettingJobDao.AddRMSettingJob(settingJobInfo);
                }
                Logger.Info("Finishing stored job setting, JobId: {0}, Site Container: {1} Setting Count: {2}.", jobId, site.Id, settingsPerContainer.Count);
                #endregion
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, gruopSetingMap);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
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

        private bool NeedCollectOneDriveSite(Dictionary<string, DateTime> modifiedDateCache, RMSPTreeNode site, Dictionary<Guid, List<Guid>> termScopeCache)
        {
            if (modifiedDateCache.ContainsKey(site.FullPath.ToLower()))
            {
                var collectionTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.OneDriveExplorerSync, new Guid(site.Parent.SPObjectId), new Guid(site.SPObjectId));
                if (collectionTime != DateTime.MinValue.Ticks
                    && collectionTime >= modifiedDateCache[site.FullPath.ToLower()].Ticks
                    && !HasChangedTermIds(collectionTime, site, termScopeCache)
                    && !HasNeedScanSetting(site))
                {
                    Logger.Info($"Site:{site.FullPath} content modified date:{modifiedDateCache[site.FullPath.ToLower()].Ticks} last collection time:{collectionTime}, no need run data sync job.");
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
                    var settings = OneDriveSettingDao.LoadOneDriveSettingsUnderSite(new Guid(site.SPObjectId));
                    var spSetting = OneDriveSettingDao.GetSettingInfoByScope(new Guid(site.Parent.SPObjectId), new Guid(site.SPObjectId), new Guid(site.SPObjectId));
                    if (spSetting == null)
                    {
                        spSetting = OneDriveSettingDao.GetSettingInfoByScope(new Guid(site.Parent.SPObjectId), Guid.Empty, new Guid(site.Parent.SPObjectId));
                    }

                    if (spSetting != null)
                    {
                        settings.Add(spSetting);
                    }

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
                            Logger.Info($"Site: {site.FullPath} has changed term ids. Setting scope id:{setting.ScopeId} Setting group id:{setting.SiteGroupId} Term scope id:{termScopeId}");
                            return true;
                        }
                    }
                    //if (spSetting != null)
                    //{
                    //    List<Guid> termIdsUnderScope = new List<Guid>();
                    //    if (spSetting.TermId != Guid.Empty)
                    //    {
                    //        if (termScopeCache.ContainsKey(spSetting.TermId))
                    //        {
                    //            termIdsUnderScope = termScopeCache[spSetting.TermId];
                    //        }
                    //        else
                    //        {
                    //            termIdsUnderScope.Add(spSetting.TermId);
                    //            termIdsUnderScope.AddRange(TermDao.GetAllSubTermUniqueIds(spSetting.TermId));
                    //            termScopeCache.Add(spSetting.TermId, termIdsUnderScope);
                    //        }
                    //    }
                    //    else if (spSetting.TermSetId != Guid.Empty)
                    //    {
                    //        if (termScopeCache.ContainsKey(spSetting.TermSetId))
                    //        {
                    //            termIdsUnderScope = termScopeCache[spSetting.TermSetId];
                    //        }
                    //        else
                    //        {
                    //            var termIds = TermDao.GetAllSubTermUniqueIdsByTermSetId(spSetting.TermSetId);
                    //            termIdsUnderScope.AddRange(termIds);
                    //            termScopeCache.Add(spSetting.TermSetId, termIdsUnderScope);
                    //        }
                    //    }

                    //    if (termIdsUnderScope.Any(t => allTerms.Contains(t)))
                    //    {
                    //        Logger.Info($"Site: {site.FullPath} has changed term ids.");
                    //        return true;
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Logger.Error("get change terms error {0}", e.ToString());
                return false;
            }
            return false;
        }

        private bool HasNeedScanSetting(RMSPTreeNode site)
        {
            Guid mSiteId = new Guid(site.SPObjectId);
            Guid groupId = new Guid(site.Parent.SPObjectId);
            List<RMOneDriveSetting> allSettingUnderSite = new List<RMOneDriveSetting>();
            var settings = OneDriveSettingDao.LoadOneDriveSettingsUnderSite(mSiteId);
            if (settings != null && settings.Count > 0)
            {
                allSettingUnderSite.AddRange(settings);
            }
            var spSetting = OneDriveSettingDao.GetSettingInfoByScope(groupId, mSiteId, mSiteId);
            if (spSetting == null)
            {
                spSetting = OneDriveSettingDao.GetSettingInfoByScope(groupId, Guid.Empty, groupId);
            }

            if (spSetting != null)
            {
                allSettingUnderSite.Add(spSetting);
            }
            foreach (var setting in allSettingUnderSite)
            {
                if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
                {
                    if (setting.RunAutoFullJob)
                    {
                        Logger.Info("Has auto full setting, should run full job. Setting id:{0} FullPath:{1}", setting.ScopeId, site.FullPath);
                        return true;
                    }

                    if (HasAutoOlderThanRule(setting.AutoClassificationRules))
                    {
                        Logger.Info("Has auto older than rule, should run full job. Setting id:{0} FullPath:{1}", setting.ScopeId, site.FullPath);
                        return true;
                    }
                }

                if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
                {
                    if (setting.NeedCheckDefaultValue)
                    {
                        Logger.Info("Has setting that needs to check default value, should run full job. Setting id:{0} FullPath:{1}", setting.ScopeId, site.FullPath);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasAutoOlderThanRule(string autoRulesStr)
        {
            List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(autoRulesStr);
            foreach (var autoRule in autoRules)
            {
                if (!autoRule.IsDefaultRule)
                {
                    foreach (var filterGroup in autoRule.FilterGroups)
                    {
                        if (filterGroup.Filters.Any(f => f.Condition == ArchiverFilterCondition.OlderThan))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Dictionary<string, DateTime> GetSiteModifiedDateCache(List<RMSPTreeNode> availableNode)
        {
            Dictionary<string, DateTime> siteModifiedDateCache = new Dictionary<string, DateTime>();
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveSettingsService.GetSiteModifiedDateCache"))
                {
                    List<string> siteUrls = availableNode.Select(s => s.FullPath).ToList();
                    var remoteSites = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(siteUrls);
                    var tenantIds = remoteSites.Select(s => s.TenantId).Distinct().ToList();
                    AvePoint.RA.RACommonUtility.CommonClientContext clientContext = new AvePoint.RA.RACommonUtility.CommonClientContext();
                    foreach (var tenantId in tenantIds)
                    {
                        try
                        {
                            var site = remoteSites.Where(s => s.TenantId == tenantId).FirstOrDefault();
                            var remoteSite = RACommonUtility.Browser.RABrowserClient.GetRemoteSiteCollectionById(site?.id);
                            var cache = clientContext.GetSiteModifiedDateCache(remoteSite, true);
                            if (cache != null && cache.Count > 0)
                            {
                                cache.ToList().ForEach(x => siteModifiedDateCache.Add(x.Key, x.Value));
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"An error occurred while getting site modified date cache,tenant id:{tenantId} error:{e.ToString()}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while getting site modified date cache, error:{e.ToString()}");
            }
            return siteModifiedDateCache;
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMOneDriveSetting> gruopSetingMap = null)
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
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.String1 = scope;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList)};           
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            if (JobServiceUtility.NewJobDetailsJobs.Contains((int)jobType))
            {
                using (var progresExecutor = AvePoint.RA.SharePoint.Common.JobExecutionProgress.JobExecutionProgressStatisticExecutor.Instance)
                {
                    Logger.Info("Init progress for sub job {0}, type {1}", subJob.Id, subJob.JobType);
                    progresExecutor.InitializeJobExecutionProgressStatictics(subJob.String1, subJob.Id, subJob.ParentId, subJob.JobType);
                }
            }
            return subJobId;
        }
        #endregion region
    }
}

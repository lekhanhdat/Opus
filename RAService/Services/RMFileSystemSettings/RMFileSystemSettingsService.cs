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
using Aspose.Pdf.Operators;
using Aspose.Slides.Export.Web;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.JobControl.JPMC;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.Multi_Geo;
using AvePoint.RA.Service.Services.RMFileSystemSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using DocumentFormat.OpenXml.Spreadsheet;
using HSMAzureCommon;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeZoneConverter;
using HybridJobType = AvePoint.Hybrid.Contract.JobType;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    [Audit]
    public class RMFileSystemSettingsService : BaseContentRepositorySettingsService, IRMFileSystemSettingsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMFileSystemSettingsService));

        #region dao&service
        private IFileSystemSettingDao FileSystemSettingDao => PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();

        private IJobMonitorService RMJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITermRuleAssociationDao TermRuleInfos => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IHybridFileSystemWorkerService HybridFileSystemWorkerService => PlatformWindsorManager.GetService<IHybridFileSystemWorkerService>();
        private IFileSystemJobTimeReferenceDao FileSystemJobTimeReferenceDao => PlatformWindsorManager.GetService<IFileSystemJobTimeReferenceDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();
        private ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMFileSystemBrowserService FSBrowerTreeService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IRMFunctionSettingDao RMFunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IMyhubReportJobDao MyhubReportJobDao => PlatformWindsorManager.GetService<IMyhubReportJobDao>();

        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private IFSMasterIndexService FSMasterIndexService => PlatformWindsorManager.GetService<IFSMasterIndexService>();
        private IFileSystemTreeCacheDao FileSystemTreeCacheDao => PlatformWindsorManager.GetService<IFileSystemTreeCacheDao>();

        private IHybridBrowserService hybridBrowserService => PlatformWindsorManager.GetService<IHybridBrowserService>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IFSAuditSinkService AuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();
        private IRMFileSystemSettingsCreateSubJobService RMFileSystemSettingsCreateSubJobService => PlatformWindsorManager.GetService<IRMFileSystemSettingsCreateSubJobService>();
        private IAgentMgmtService AgentMgmtService => PlatformWindsorManager.GetService<IAgentMgmtService>();
        private IRMFSConnectionAndOwnerRelationshipDao RMFSConnAndOwnerRela => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        //private IJobWeightProvider JobWeightProvider => PlatformWindsorManager.GetService<IJobWeightProvider>();
        //private ITenantResourceProvider TenantResourceProvider => PlatformWindsorManager.GetService<ITenantResourceProvider>();
        //private IConcurrencyBudgetCalculator ConcurrencyBudgetCalculator => PlatformWindsorManager.GetService<IConcurrencyBudgetCalculator>();
        #endregion

        private List<JobType> _agentJobTypes = new List<JobType>
        {
            JobType.FSArchiverRestore,
            JobType.FSDataSynchronization,
            JobType.FSDataSynchronizationSchedule,
            JobType.FSDisposal,
            JobType.FSDisposalSchedule,
            JobType.FSDisposalByClassCode,
            JobType.FSRetain,
            JobType.FSRetainSimulate,
            JobType.FSCreateAndDestroyedFileReport,
            JobType.FSItemsFilesDueDisposal,
            JobType.DiscoveryAnalysisFileSystemV1,
            JobType.DiscoveryFileSystemV1,
            JobType.SPOnPremUniqueIDSettingFullSchedule,
            JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            JobType.SPOnPremApplySetting,
            JobType.SPOnPremDataSync,
            JobType.SPOnPremDataSyncSchedule,
            JobType.SPOnPremTermSynchronization,
            JobType.SPOnPremTermSynchronizationSchedule,
            JobType.SPOnPremEnforceRuleAction,
            JobType.SPOnPremEnforceRuleActionSchedule,
            JobType.SPOnPremItemsFilesDueDisposal,
            JobType.SPOnPremCreateAndDestroyedFileReport,
            JobType.SPOnPremScanLocalNodes,
        };
        public async Task<bool> LoadFSNodeEnableRecordManagement(Guid nodeId)
        {
            return await FileSystemSettingDao.IsFSEnableRecordManagement(nodeId);
        }

        public async Task<bool> CheckFullPathConnectionAsync(RMFSTreeNode sNode)
        {
            return await FileSystemSettingDao.IsFullPathConnectionExist(sNode);
        }
        
        public List<Guid> ValidateEnableRecordManagementNodes(List<Guid> nodeIds)
        {
            try
            {
                if (nodeIds == null || nodeIds.Count == 0)
                {
                    logger.Warn("ValidateEnableRecordManagementNodes: nodeIds is null or empty.");
                    return new List<Guid>();
                }
                return FileSystemSettingDao.ValidateEnableRecordManagementNodes(nodeIds);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while validating enable record management nodes: {0}", ex);
                throw;
            }
        }
        public async Task<RMFSTreeNode> LoadFSNodeSettingAsync(RMFSTreeNode sNode, bool loadLocalInfo = false)
        {
            var GSetting = FileSystemSettingDao.LoadFSSetting(sNode.ConnGroupId, sNode.ConnGroupId);//TODO
            if (GSetting != null)
            {
                sNode.IconStatus = IconStatus.Inhert;
                var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);
                var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                RMTermSet termSet = null;
                if (GSetting.TermId == Guid.Empty)
                {
                    termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                }
                sNode.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                sNode.TermIdOfContainer = GSetting.TermIdOfContainer;
                sNode.isEnableClassification = GSetting.IsEnableContainerLevelClassification;
                sNode.DescriptionOfContainer = GSetting.DescriptionOfContainer;
                sNode.TermSetId = GSetting.TermSetId;
                sNode.TermSetName = GSetting.TermSetName;
                sNode.TermId = GSetting.TermId;
                sNode.TermName = GSetting.TermName;
                sNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                sNode.DefaultTermId = GSetting.DefaultTermId;
                sNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                sNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                sNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";
                sNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                sNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                sNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                sNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                sNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                sNode.ApplyExistType = GSetting.ApplyExistType;
                //Contract.TaxonomyModel.ApplyExistingTermType
                if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)ApplyExistingTermType.None)
                {
                    sNode.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                }
                sNode.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                sNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                //sNode.RecordOwner = GetSettingRecordOnwers(GSetting.Id, SourceFlag.FileSystem);
                sNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.FileSystem);
                sNode.ConnGroupId = GSetting.ConnectionGroupId;
                sNode.ProfileId = GSetting.IdPath;
                sNode.IsActive = GSetting.IsActive;
                sNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                sNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                    null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                SetAutoTermStatus(sNode.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(sNode.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(sNode.AutoClassificationRules);
                sNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                sNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                sNode.ApprovalType = (int)GSetting.ApprovalType;
                sNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;
                sNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                sNode.IsAllowUserDownloadRCCReport = GSetting.IsAllowUserDownloadRCCReport;
                sNode.ApplyExistDocument = GSetting.ApplyExistDocument;
            }
            //reset IsCustomSetting property
            sNode.IsCustomSetting = false;
            var spSetting = FileSystemSettingDao.LoadFSSetting(sNode.Id, sNode.ConnGroupId);
            if (spSetting == null)
            {
                if (sNode.Level == (int)NodeLevel.FSFolder)
                {
                    var parentNode = sNode.Parent;
                    spSetting = LoadParentAllFSSeting(parentNode, sNode.ConnGroupId);
                    sNode.IsCustomSetting = false;
                }
            }
            else
            {
                sNode.IconStatus = IconStatus.Break;
                if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                {
                    sNode.IsCustomSetting = true;
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

                sNode.DefaultTermId = spSetting.DefaultTermId;
                sNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                sNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                sNode.TermId = spSetting.TermId;
                sNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                sNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                sNode.TermSetId = spSetting.TermSetId;
                sNode.TermSetName = spSetting.TermSetName;
                sNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                sNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                sNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                sNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                sNode.DescriptionOfContainer = spSetting.DescriptionOfContainer;
                sNode.TermIdOfContainer = spSetting.TermIdOfContainer;
                sNode.TermNameOfContainer = containerTerm == null ? spSetting.TermNameOfContainer : containerTerm.Name;
                sNode.isEnableClassification = spSetting.IsEnableContainerLevelClassification;
                sNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                sNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                sNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                sNode.ApplyExistType = spSetting.ApplyExistType;
                if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                {
                    sNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                }

                sNode.EnableRelatedRecords = spSetting.EnableRelatedRecords;
                sNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.FileSystem);
                sNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                sNode.ProfileId = spSetting.IdPath;
                sNode.IsActive = spSetting.IsActive;
                sNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                sNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                    null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                SetAutoTermStatus(sNode.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(sNode.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(sNode.AutoClassificationRules);
                sNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                sNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                sNode.ApprovalType = (int)spSetting.ApprovalType;
                sNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;
                sNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                sNode.IsAllowUserDownloadRCCReport = spSetting.IsAllowUserDownloadRCCReport;
                sNode.ApplyExistDocument = spSetting.ApplyExistDocument;
            }

            if (CheckParentNodeDisable(sNode.Parent, sNode.ConnGroupId))
            {
                sNode.EnableRecordManagement = (int)RMFSTreeNode.EnableRecordManagementSetting.ParentDisable;
            }

            var profileId = ScheduleService.GetProfileId(sNode);
            var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.FSDisposalSchedule);
            if (disposeSchedule != null)
            {
                var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");

                sNode.DisposeScheduleInfo = disposeSchedule;
                sNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(sNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                //configNode.IsCustomSetting = true;
                sNode.IconStatus = IconStatus.Break;
            }
            else
            {
                var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.FSDisposalSchedule);
                if (ancestryDisposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                    ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                    ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                    sNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                    sNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                    sNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(sNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                }
                else
                {
                    sNode.DisposeScheduleInfo = null;
                }
            }

            if (loadLocalInfo)
            {
                try
                {
                    List<string> paths = new List<string>();
                    switch ((NodeLevel)sNode.Level)
                    {
                        case NodeLevel.WebApplication:
                            var groupInfo = await FSConnectionDao.GetAllConnectionsByGroupIdAsync(sNode.ConnGroupId);
                            paths.AddRange(groupInfo.Select(g => g.UNCPath));
                            break;

                        case NodeLevel.SiteCollection:
                        case NodeLevel.FSFolder:
                            paths.Add(sNode.FullPath);
                            break;
                    }
                    var formattedPaths = paths
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Select(p =>
                        {
                            var normalized = p.Replace(@"\\", @"\");

                            if (!normalized.StartsWith(@"\\"))
                                normalized = @"\" + normalized;

                            return normalized;
                        })
                        .ToList();

                    if (formattedPaths.Count() > 0)
                    {

                        var records = explorerDao.SearchByFullPath(paths, (int)NodeLevel.FSFolder, (int)SourceFlag.FileSystem, string.Empty, 1000);

                        var record = records.Item1.FirstOrDefault();
                        if (record != null && ((NodeLevel)sNode.Level == NodeLevel.SiteCollection || (NodeLevel)sNode.Level == NodeLevel.FSFolder))
                        {
                            var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                            if (metaInfo != null)
                            {
                                sNode.FolderCreationDate = metaInfo.CreatedTime != 0
                                    ? (await mGeneralSettingService
                                        .ConvertTiksToDateTimeAsync(metaInfo.CreatedTime, true))
                                        .SimplifyFormatTime
                                    : null;

                                sNode.FolderLastModifiedDate = metaInfo.LastModifiedTime != 0
                                    ? (await mGeneralSettingService
                                        .ConvertTiksToDateTimeAsync(metaInfo.LastModifiedTime, true))
                                        .SimplifyFormatTime
                                    : null;
                            }
                        }

                        var totalSize = explorerDao.GetTotalSizeByNodeTypeAndDirPaths((int)NodeLevel.FSFile, (int)SourceFlag.FileSystem, (int)RMRecordStatus.Active, formattedPaths);
                        if (record != null && totalSize >= 0)
                        {
                            sNode.NodeSize = ConvertUnitUtil.FormatBytes(totalSize);
                        }
                        var group = FSConnectionGroupDao.GetGroup(sNode.ConnGroupId);

                        sNode.AgentName = group.AccessConnectionType == AccessConnectionType.All ?
                           I18NEntity.GetString("RM_FS_InformationBoard_Type_All") :
                           string.Join(", ", group.Agents.Select(a => a.Name));
                        if (sNode.Level == (int)NodeLevel.SiteCollection) // only site collection level has JPMC connectionId
                        {
                            var connection = FSConnectionDao.GetConnectionById(sNode.Id);
                            sNode.ConnectionId = connection?.JPMCConnectionId.ToString(); // TODO later

                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Load local info error {0}", ex.ToString());
                }

            }
            var enabledJPMCFSFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if (enabledJPMCFSFeature)
            {
                // need compare classcode value from cosmos db and sql db, if different, use the value from cosmosDb.
                var nodeId = sNode.Id;
                if (sNode.Level == (int)NodeLevel.SiteCollection)
                {
                    nodeId = sNode.FullPath.ToLowerInvariant().ToMd5();
                }
                var nodeInfoInCosmos = ExplorerService.GetFSDBRecords(new List<Guid> { nodeId }).FirstOrDefault();
                var termSetIDFromCosmos = Guid.Empty;
                if (nodeInfoInCosmos != null)
                {
                    var termSetFromCosmos = await TermDao.GetTermSetFromTermUniqueId(nodeInfoInCosmos.TermId);
                    termSetIDFromCosmos = termSetFromCosmos != null ? termSetFromCosmos.UniqueId : Guid.Empty ;
                }
                var effectiveSetting = ResolveEffectiveClassCodeSetting(spSetting, GSetting);
                if (effectiveSetting != null)
                {
                    var term = TermDao.GetRMTermByGuId(effectiveSetting.DefaultTermId);
                    if (term == null || (termSetIDFromCosmos != Guid.Empty && termSetIDFromCosmos != sNode.TermSetId))
                    {
                        sNode.ClassCode = null;
                        return sNode;
                    }

                    if (term.IsDeprecated || term.IsRemoved)
                    {
                        sNode.ClassCode = new FSClassCodeDto();
                        return sNode;
                    }

                    var retentionDate = effectiveSetting.StartDate != 0
                        ? (await mGeneralSettingService.ConvertTiksToDateTimeAsync(effectiveSetting.StartDate, true)).SimplifyFormatTime
                        : null;

                    sNode.ClassCode = new FSClassCodeDto
                    {
                        ClassCodeId = term.Name,
                        CountryCode = effectiveSetting.CountryCode,
                        RetentionDate = retentionDate,
                        RetentionType = effectiveSetting.RetentionScheduleType,
                        TermUniqueId = effectiveSetting.DefaultTermId.ToString(),
                        StartDate = effectiveSetting.StartDate,
                        ApplyExistDocuments = effectiveSetting.ApplyExistDocument
                    };
                }

                var isValidCosmosData = nodeInfoInCosmos != null && nodeInfoInCosmos.TermId != Guid.Empty;

                var isDifferentClassCode = sNode.ClassCode != null && nodeInfoInCosmos?.TermId.ToString() != sNode.ClassCode.TermUniqueId;

                if (!isValidCosmosData || !isDifferentClassCode)
                {
                    return sNode;
                }
                if (sNode.ClassCode != null)
                {
                    sNode.ClassCode.EndTime = nodeInfoInCosmos.EndTime;
                }
                sNode.ClassCode.ClassCodeId = nodeInfoInCosmos.ClassCode;
                sNode.ClassCode.CountryCode = nodeInfoInCosmos.CountryCode;
                sNode.ClassCode.RetentionDate = nodeInfoInCosmos.StartDate != 0
                    ? (await mGeneralSettingService.ConvertTiksToDateTimeAsync(nodeInfoInCosmos.StartDate, true)).SimplifyFormatTime
                    : string.Empty;
                sNode.ClassCode.RetentionType = (RetentionScheduleType)nodeInfoInCosmos.RetentionType;
                sNode.ClassCode.TermUniqueId = nodeInfoInCosmos.TermId.ToString();
                sNode.ClassCode.StartDate = nodeInfoInCosmos.StartDate;
            }
            return sNode;
        }
        private RMFileSystemSetting ResolveEffectiveClassCodeSetting(RMFileSystemSetting nodeSetting, RMFileSystemSetting groupSetting)
        {
            if (nodeSetting != null)
            {
                return nodeSetting;
            }

            if (groupSetting != null)
            {
                return groupSetting;
            }

            return null;
        }


        public async Task PropagateClassCodeToChildrenAsync(RMFileSystemSetting currentSetting, ClassCodePolicyInfo classCodePolicyInfo, long startDateTicks)
        {
            var connGroupId = currentSetting.ConnectionGroupId;
            bool isGroupLevel = currentSetting.ScopeId == connGroupId;

            if (isGroupLevel)
            {
                await PropagateClassCodeFromGroupAsync(currentSetting, classCodePolicyInfo, startDateTicks);
            }
            else
            {
                await PropagateClassCodeFromConnectionAsync(currentSetting, classCodePolicyInfo, startDateTicks);
            }
        }

        /// <summary>
        /// Propagates ClassCode from Group level to all Connections and their child Folders/Documents.
        /// </summary>
        private async Task PropagateClassCodeFromGroupAsync(RMFileSystemSetting groupSetting, ClassCodePolicyInfo classCodePolicyInfo, long startDateTicks)
        {
            logger.Info("Propagating ClassCode from Group level. GroupId: {0}, ClassCode: {1}", groupSetting.ConnectionGroupId, classCodePolicyInfo.ClassCode);

            var allSettingsUnderGroup = FileSystemSettingDao.LoadAllSettingsUnderGroup(groupSetting.ConnectionGroupId);

            // Exclude the group setting itself (already updated)
            var childSettings = allSettingsUnderGroup
                .Where(s => s.Id != groupSetting.Id)
                .ToList();

            if (childSettings.Count == 0)
            {
                logger.Info("No child settings found under Group. GroupId: {0}", groupSetting.ConnectionGroupId);
                return;
            }

            await FileSystemSettingDao.BatchUpdateClassCodeAsync(
                childSettings,
                new Guid(classCodePolicyInfo.TermUniqueId),
                classCodePolicyInfo.ClassCode,
                classCodePolicyInfo.CountryCode,
                classCodePolicyInfo.RetentionScheduleType,
                startDateTicks,
                classCodePolicyInfo.ApplyExistDocument);

            logger.Info("ClassCode propagated from Group to {0} child settings. GroupId: {1}", childSettings.Count, groupSetting.ConnectionGroupId);
        }

        /// <summary>
        /// Propagates ClassCode from Connection level to all child Folders/Documents.
        /// </summary>
        private async Task PropagateClassCodeFromConnectionAsync(RMFileSystemSetting connectionSetting, ClassCodePolicyInfo classCodePolicyInfo, long startDateTicks)
        {
            logger.Info("Propagating ClassCode from Connection level. ConnectionId: {0}, ClassCode: {1}", connectionSetting.Id, classCodePolicyInfo.ClassCode);

            var allSettingsUnderConnection = FileSystemSettingDao.LoadAllSettingsByConnectionGroupIdAndConnectionPath(
                connectionSetting.ConnectionGroupId,
                connectionSetting.FullPath);

            // Exclude the connection setting itself (already updated)
            var childSettings = allSettingsUnderConnection
                .Where(s => s.Id != connectionSetting.Id)
                .ToList();

            if (childSettings.Count == 0)
            {
                logger.Info("No child settings found under Connection. ConnectionId: {0}", connectionSetting.Id);
                return;
            }

            await FileSystemSettingDao.BatchUpdateClassCodeAsync(
                childSettings,
                new Guid(classCodePolicyInfo.TermUniqueId),
                classCodePolicyInfo.ClassCode,
                classCodePolicyInfo.CountryCode,
                classCodePolicyInfo.RetentionScheduleType,
                startDateTicks,
                classCodePolicyInfo.ApplyExistDocument);

            logger.Info("ClassCode propagated from Connection to {0} child settings. ConnectionId: {1}", childSettings.Count, connectionSetting.Id);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSEditLocationOwnersSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.FSEditLocationOwnersSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async System.Threading.Tasks.Task AddFSLocationOwnersAsync(RMFSTreeNode locationNode)
        {
            try
            {
                logger.Info("Set Location Owners FS Setting");
                var settingNode = locationNode;
                await FileSystemSettingDao.AddOrUpdateFSSettingAsync(locationNode, locationNode.ConnGroupId);
            }
            catch (Exception ex)
            {
                logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
            }
        }
        private bool CheckParentNodeDisable(RMFSTreeNode parentNode, Guid connGroupId)
        {
            while (parentNode != null)
            {
                var parentSetting = FileSystemSettingDao.LoadFSSetting(parentNode.Id, connGroupId);
                if (parentSetting != null
                    && parentSetting.EnableRecordManagement == (int)RMFSTreeNode.EnableRecordManagementSetting.Disable)
                {
                    return true;
                }
                parentNode = parentNode.Parent;
            }
            return false;
        }
        private RMFileSystemSetting LoadParentAllFSSeting(RMFSTreeNode node, Guid siteId)
        {
            RMFileSystemSetting fsSetting = null;
            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return fsSetting;
            }
            //TODO 没有看懂之前判断Level的逻辑
            //if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
            //{
            fsSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
            //}
            if (fsSetting == null)
            {
                fsSetting = LoadParentAllFSSeting(node.Parent, node.ConnGroupId);
            }
            return fsSetting;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSEditDocLevelSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.FSEditDocLevelSettingForJPMC, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async System.Threading.Tasks.Task SaveFSNodeSettingAsync(RMFSTreeNode sNode)
        {
            AddFilterCretiaProperty(sNode.AutoClassificationRules);
            await FileSystemSettingDao.AddOrUpdateFSSettingAsync(sNode, sNode.ConnGroupId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSEditGeneralSettingForJPMC, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.FSEditGeneralSettingForJPMC, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<RAReturnMessage> SaveFSGeneralSetting4JPMC(RMFSTreeNode sNode)
        {
            RAReturnMessage result = new RAReturnMessage { MessageType = RAMessageType.Successful };

            try
            {
                logger.Info("Start SaveFSGeneralSetting4JPMC for node: {0}", sNode.FullPath);
                if (sNode.Level != (int)NodeLevel.WebApplication && CheckParentNodeDisable(sNode.Parent, sNode.ConnGroupId))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }

                AddFilterCretiaProperty(sNode.AutoClassificationRules);

                if (sNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable)
                {
                    sNode.IsAllowUserDownloadRCCReport = false;
                }

                string nodeProfileIdPath = ScheduleService.GetProfileId(sNode);
                await FileSystemSettingDao.RemoveDescendantsSettingAsync(sNode, nodeProfileIdPath);
                await FileSystemSettingDao.AddOrUpdateFSSettingAsync(sNode, sNode.ConnGroupId);
            }
            catch (Exception ex)
            {
                logger.Error("SaveFSGeneralSetting4JPMC Error: {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
            }

            return result;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplyClassCodeSettings4FS, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.ApplyClassCodeSettings4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<RAReturnMessage> SaveClassCodePolicyAsync(ClassCodePolicyInfo classCodePolicyInfo)
        {
            RAReturnMessage msg = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                logger.Info("Save Class Code Policy, ClassCode: {0}, CountryCode: {1}, RetentionScheduleType: {2}, StartDate: {3}, CurrentNodeId: {4}, ConnGroupId: {5}, TermSetId: {6}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.CountryCode, classCodePolicyInfo.RetentionScheduleType, classCodePolicyInfo.StartDate, classCodePolicyInfo.CurrentNodeId, classCodePolicyInfo.ConnGroupId, classCodePolicyInfo.TermSetId);
                // var terms = await TermDao.GetTermFromTermSetUniqueId(Guid.Parse(classCodePolicyInfo.TermSetId));
                // if(!terms.Any(t => t.Name.Equals(classCodePolicyInfo.ClassCode, StringComparison.OrdinalIgnoreCase)))
                // {
                //     logger.Error("Save Class Code Policy Error, the class code is not exist in term store or is deprecated. ClassCode: {0}, TermSetId: {1}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.TermSetId);
                //     msg.MessageType = RAMessageType.Failed;
                //     msg.ErrorMessage = I18NEntity.GetString("RM_FS_ClassCode_NotInTermSetScope");
                //     return msg;
                // }
                var validationResult = await ValidateClassCodePolicyAsync(classCodePolicyInfo);
                if (!validationResult.Item1)
                {
                    logger.Error("Save Class Code Policy Error, validation failed. ClassCode: {0}, CountryCode: {1}, RetentionScheduleType: {2}. Error: {3}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.CountryCode, classCodePolicyInfo.RetentionScheduleType, validationResult.Item2);
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = validationResult.Item2;
                    return msg;
                }
                if (classCodePolicyInfo.FSTreeNode.ClassCode == null)
                {
                    classCodePolicyInfo.FSTreeNode.ClassCode = new FSClassCodeDto();
                }
                if (classCodePolicyInfo.RetentionScheduleType == RetentionScheduleType.Flat)
                {
                    classCodePolicyInfo.StartDate = new DateTime(0);
                }

                classCodePolicyInfo.FSTreeNode.ClassCode.ClassCodeId = classCodePolicyInfo.ClassCode;
                classCodePolicyInfo.FSTreeNode.ClassCode.CountryCode = classCodePolicyInfo.CountryCode;
                classCodePolicyInfo.FSTreeNode.ClassCode.RetentionType = classCodePolicyInfo.RetentionScheduleType;
                classCodePolicyInfo.FSTreeNode.ClassCode.RetentionDate = classCodePolicyInfo.StartDate.Ticks > 0 ? classCodePolicyInfo.StartDate.Ticks.ToString() : null;
                classCodePolicyInfo.FSTreeNode.ClassCode.TermUniqueId = classCodePolicyInfo.TermUniqueId;
                classCodePolicyInfo.FSTreeNode.ClassCode.StartDate = classCodePolicyInfo.StartDate.Ticks > 0 ? classCodePolicyInfo.StartDate.Ticks : 0;
                classCodePolicyInfo.FSTreeNode.ClassCode.ApplyExistDocuments = classCodePolicyInfo.ApplyExistDocument;
                classCodePolicyInfo.FSTreeNode.ApplyExistDocument = classCodePolicyInfo.ApplyExistDocument;
                classCodePolicyInfo.FSTreeNode.DeployTermMethod = (int)DeployTermMethod.UseDefaultTerm;
                classCodePolicyInfo.FSTreeNode.DefaultTermId = Guid.Parse(classCodePolicyInfo.TermUniqueId);
                long startDateTicks = classCodePolicyInfo.StartDate.Ticks;
                if (classCodePolicyInfo.FSTreeNode.Level != (int)NodeLevel.FSFolder)
                {
                    await FileSystemSettingDao.AddOrUpdateFSSettingAsync(classCodePolicyInfo.FSTreeNode, Guid.Parse(classCodePolicyInfo.ConnGroupId));

                    var nodeSetting = FileSystemSettingDao.LoadFSSetting(Guid.Parse(classCodePolicyInfo.CurrentNodeId), Guid.Parse(classCodePolicyInfo.ConnGroupId));

                    if (classCodePolicyInfo.ApplyExistDocument)
                    {
                        await PropagateClassCodeToChildrenAsync(nodeSetting, classCodePolicyInfo, startDateTicks);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Save Class Code Policy Error {0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_FS_ClassCodePolicy_SaveFailed");
            }
            return msg;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.MyhubClassify, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.MyhubClassify, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<RAReturnMessage> MyhubSaveClassCodePolicyAsync(ClassCodePolicyInfo classCodePolicyInfo, RMMyhubClassifyQueryInfo queryInfo)
        {
            return await SaveClassCodePolicyAsync(classCodePolicyInfo);
        }
        private async Task<(bool, string)> ValidateClassCodePolicyAsync(ClassCodePolicyInfo classCodePolicyInfo)
        {
            if (!Guid.TryParse(classCodePolicyInfo.TermSetId, out var termSetId))
            {
                return (false, "Invalid TermSetId");
            }

            var terms = await TermDao.GetTermFromTermSetUniqueId(termSetId);

            var term = terms.FirstOrDefault(t =>
                t.Name.Equals(classCodePolicyInfo.ClassCode, StringComparison.OrdinalIgnoreCase));

            if (term == null)
            {
                logger.Error("Save Class Code Policy Error, the class code is not exist in term store or is deprecated. ClassCode: {0}, TermSetId: {1}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.TermSetId);
                return (false, I18NEntity.GetString("RM_FS_ClassCode_NotInTermSetScope"));
            }

            var termRuleMapping = TermRuleInfos.GetTermRuleInfoByTermIds(new List<int> { term.Id });
            var ruleIds = termRuleMapping.Select(trm => trm.RuleId).Distinct().ToList();
            var allRules = RuleManagerService.GetRulesByIds(ruleIds).ToDictionary(d => d.Id);

            foreach (var termRule in termRuleMapping)
            {
                if (allRules.TryGetValue(termRule.RuleId.ToString(), out Rule rule) && rule.FSRule?.Filters != null)
                {
                    var countryFilter = rule.FSRule.Filters.FirstOrDefault(f =>
                        (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals)
                        && f.Rule is ColumnTextRule
                        && string.Equals(f.Rule.Value1, "[CountryCode]", StringComparison.OrdinalIgnoreCase));

                    var retentionFilter = rule.FSRule.Filters.FirstOrDefault(f =>
                        f.Condition == PolicyCondition.Equals
                        && f.Rule is ColumnTextRule
                        && string.Equals(f.Rule.Value1, "[RetentionType]", StringComparison.OrdinalIgnoreCase));

                    if (countryFilter == null || retentionFilter == null)
                    {
                        logger.Warn("Class Code Policy Validation Failed. ClassCode: {0}, CountryCode: {1}, RetentionScheduleType: {2}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.CountryCode, classCodePolicyInfo.RetentionScheduleType);
                        continue;
                    }

                    var countryCodes = countryFilter.Value?.Value1?
                        .Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .ToList();

                    var retentionTypeValue = retentionFilter.Value?.Value1;
                    var isCountryMatch = countryCodes != null &&
                                         countryCodes.Contains(classCodePolicyInfo.CountryCode, StringComparer.OrdinalIgnoreCase);

                    var isRetentionMatch =
                        string.Equals(retentionTypeValue,
                            ((RetentionScheduleType)classCodePolicyInfo.RetentionScheduleType).ToString(),
                            StringComparison.OrdinalIgnoreCase);

                    if (isCountryMatch && isRetentionMatch)
                    {
                        logger.Info("Class Code Policy Validation Passed. ClassCode: {0}, CountryCode: {1}, RetentionScheduleType: {2}", classCodePolicyInfo.ClassCode, classCodePolicyInfo.CountryCode, classCodePolicyInfo.RetentionScheduleType);
                        return (true, string.Empty);
                    }
                }
            }

            return (false, I18NEntity.GetString("RM_FS_ClassCode_NotMatchAnyRule"));
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSDeactiveSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.FSActiveSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async System.Threading.Tasks.Task SaveFSActiveSettingAsync(RMFSTreeNode sNode)
        {
            await FileSystemSettingDao.AddOrUpdateFSSettingAsync(sNode, sNode.ConnGroupId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSEditInheritSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.FSEditInheritSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async System.Threading.Tasks.Task InheritFSParentSettingAsync(RMFSTreeNode node)
        {
            try
            {
                logger.Info("Inherit Parent Settings");
                await FileSystemSettingDao.DeleteFileSystemSettingAsync(node.Id, node.ConnGroupId);
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
            }
        }

        public void LoadFSSettingIcon(List<RMFSTreeNode> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMFSTreeNode firstNode = nodes[0];
                    if (firstNode.Level != (int)NodeLevel.WebApplication)// != connection group
                    {
                        var groupId = firstNode.ConnGroupId;
                        var gsSetting = FileSystemSettingDao.LoadFSSetting(groupId, groupId);
                        //TODO FS disposal schedule
                        //var allSchedulesProfilesId = ScheduleService.GetScheduleProfileIdByTypes(new List<ScheduleType> { ScheduleType.FSDisposalSchedule, ScheduleType.FSColletionDataSchedule });
                        foreach (var node in nodes)
                        {
                            var csSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                            if (csSetting != null)
                            {
                                node.IconStatus = IconStatus.Break;
                                node.IsActive = csSetting.IsActive;
                                continue;
                            }
                            //var profileId = ScheduleService.GetProfileId(node);
                            //if (allSchedulesProfilesId.Contains(profileId))
                            //{
                            //    node.IconStatus = IconStatus.Break;
                            //    continue;
                            //}
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
                            var selfGSSetting = FileSystemSettingDao.LoadFSSetting(selfGroupNode.Id, selfGroupNode.ConnGroupId);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred when load SharePointSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        public bool CheckFSNodeSettingExist(List<Guid> connectionIds)
        {
            try
            {
                var scopeIds = new List<Guid>();
                scopeIds.AddRange(connectionIds);
                var deleteConnectionsGroup = FSConnectionDao.GetConnectionByIds(connectionIds);
                if (deleteConnectionsGroup != null)
                {
                    scopeIds.AddRange(deleteConnectionsGroup.Select(g => g.GroupId));
                }
                return FileSystemSettingDao.Exist(s => scopeIds.Contains(s.ScopeId));
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred when load CheckFSNodeSettingExist. Error:{e}");
                return false;
            }
        }

        public RAReturnMessage RunFSDataSyncScheduleJob(JobRunBy jobRunBy)
        {
            logger.Debug("start all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                var loginName = TenantLocalValue.LogonUserEmail;

                if (!TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, Contract.RoleAssignments.PaidForModule.FileSystem))
                {
                    logger.Warn($"Run FS Data Sync Job failed, Tenant:{TenantLocalValue.LogonGroupId} FS license is not available.");
                    return msg;
                }
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.FSDataSynchronizationSchedule,
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
                logger.Error("error occurred while SP DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSDisposalJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSDisposalJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RunFSDisposalScheduleJobAsync(RMFSTreeNode treeNode, JobRunBy jobRunBy)
        {
            logger.Debug("start fs disposal schedule job");
            string jobId = string.Empty;
            try
            {
                if (!TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, Contract.RoleAssignments.PaidForModule.FileSystem))
                {
                    logger.Warn($"Run FS disposal Job failed, Tenant:{TenantLocalValue.LogonGroupId} FS license is not available.");
                    return jobId;
                }
                if (FSConnectionGroupDao.GetGroupById(treeNode.ConnGroupId) == null)
                {
                    logger.Warn("Cannot find the parent group of this node, will not start fs disposal schedule job. NodeId:{0} Group id:{1}", treeNode?.Id, treeNode?.ConnGroupId);
                    return jobId;
                }
                jobId = await RunDisposalJobBySelectdNodeAsync("RM_TS_RunSchedule", JobType.FSDisposalSchedule, treeNode);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while fs disposal job,ERROR:{0}", ex.ToString());
            }

            return jobId;
        }
        public bool HasRunningJobOnAgentIds(List<Guid> agentIds)
        {
            return SubJobDao.GetRunningAgentJob(_agentJobTypes, agentIds.ConvertAll(id => id.ToString())).Count > 0;
        }
        /// <summary>
        /// job互斥，只判断上层或者当前节点上是否有job在运行，如果下层节点有job在运行，获取job message时一起获取到当前节点下层正在运行job的folder，在job中将这些folder过滤
        /// </summary>
        /// <param name="scopeId"></param>
        /// <param name="groupId"></param>
        /// <param name="currentJobId"></param>
        /// <param name="currentNodeLevel"></param>
        /// <param name="type"></param>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private bool HasRunningJobOnNode(string scopeId, string groupId, string currentJobId, int currentNodeLevel, JobType type, RMFSTreeNode treeNode = null)
        {
            try
            {
                using (new PerformanceScope("Check HasRunningJobOnNode:" + currentJobId))
                {
                    List<JobType> jobTypes = new List<JobType>();
                    if (type == JobType.FSDataSynchronization || type == JobType.FSDataSynchronizationSchedule || type == JobType.ImportFSSetting || type == JobType.ExportFSSetting)
                    {
                        jobTypes.Add(JobType.FSDataSynchronization);
                        jobTypes.Add(JobType.FSDataSynchronizationSchedule);
                    }
                    else if (type == JobType.FSDisposal || type == JobType.FSDisposalSchedule || type == JobType.FSDisposalByClassCode)
                    {
                        jobTypes.Add(JobType.FSDisposal);
                        jobTypes.Add(JobType.FSDisposalSchedule);
                        jobTypes.Add(JobType.FSDisposalByClassCode);
                    }
                    else if (type == JobType.ApplyClassCode)
                    {
                        jobTypes.Add(JobType.ApplyClassCode);
                    }

                    string currentScopeId = string.Empty;
                    if (currentNodeLevel == (int)NodeLevel.FSFolder)
                    {
                        var dto = ConvertRMTree2FSTree(treeNode);
                        currentScopeId = QueryScopeTermIdSetting(dto, treeNode.ConnGroupId).ScopeId.ToString();
                    }

                    switch (currentNodeLevel)
                    {
                        //group级别有job在运行，再次在group级别运行job，job会skip
                        case (int)NodeLevel.WebApplication:
                            var groupJobs = JobMonitorService.GetRunningJobs(jobTypes, groupId).Where(j => !j.Id.Equals(currentJobId)).ToList();
                            if (groupJobs.Count > 0)
                            {
                                logger.Debug("Has running job on group. Job ids:{0}", string.Join(",", groupJobs));
                            }
                            return groupJobs.Count > 0;
                        //connection级别运行job，检查connection上是否有正在运行的job
                        case (int)NodeLevel.SiteCollection:
                            var connectionJobs = new List<RMSubJob>();
                            if (type == JobType.FSDisposalByClassCode)
                            {
                                connectionJobs = SubJobDao.GetRunningAgentJob(jobTypes).Where(j => (j.String1 == treeNode.FullPath || j.String1.StartsWith(treeNode.FullPath + "\\")) 
                                                        && !j.Id.Equals(currentJobId)).ToList();
                            }
                            else
                            {
                                connectionJobs = SubJobDao.GetRunningAgentJob(jobTypes).Where(j => j.String1 == treeNode.FullPath && !j.Id.Equals(currentJobId)).ToList();
                            }
                            if (connectionJobs != null && connectionJobs.Count > 0)
                            {
                                logger.Debug("Has running job on connection. Job ids:{0}", string.Join(",", connectionJobs));
                            }
                            ArgumentCheck.NotNull(connectionJobs, nameof(connectionJobs));
                            return connectionJobs.Count > 0;
                        //folder级别运行job，根据路径找到正在运行的job，如果上层folder使用的fs setting与当前folder的fs setting相同，
                        //认为上层级别有job在运行（使用的FS Setting是同一个,认为当前folder没有打破继承），skip当前folder的job
                        case (int)NodeLevel.FSFolder:
                            var folderAndParentJobs = SubJobDao.GetRunningAgentJob(jobTypes)
                                .Where(j => treeNode.FullPath.StartsWith(j.String1) && !j.Id.Equals(currentJobId)).OrderByDescending(j => j.String1).ToList();
                            if (folderAndParentJobs.Count > 0)
                            {
                                //check if use the same fs setting
                                var subJob = folderAndParentJobs[0];
                                var context = SubJobDao.GetSubJob(subJob.Id, true)?.JobContext;
                                if (context != null)
                                {
                                    RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(context.Settings).First();
                                    var dto = ConvertRMTree2FSTree(node);
                                    var tempScopeId = QueryScopeTermIdSetting(dto, node.ConnGroupId).ScopeId.ToString();
                                    if (currentScopeId.Equals(tempScopeId.ToString()))
                                    {
                                        logger.Debug("Has running job on parent node. Job id:{0} NodeId:{1}", subJob.Id, node?.Id);
                                    }
                                    return currentScopeId.Equals(tempScopeId.ToString());
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
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while checking if has job running on node. Error:{0}", e.ToString());
            }
            return false;
        }

        #region FSDisposalByClassCode

        public async Task<RAReturnMessage> RunDisposalByClassCodeJobAsync(AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request, JobRunBy jobRunBy)
        {
            logger.Debug("Start FS disposal by class code.");
            RAReturnMessage msg = new RAReturnMessage();

            if (TermRuleInfos.GetTermWithRule().Count == 0)
            {
                logger.Error(I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules"));
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules");
                return msg;
            }

            

            if (!await HasAnyAvailableNodeForClassCodeDisposalAsync(request.ConnectionGroupID, request.NodeId, request.FullPath))
            {
                logger.Info("No available node for class code disposal. All connections have unique enforcement schedules. GroupId:[{0}], NodeId:[{1}]", request.ConnectionGroupID, request.NodeId);
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_Disposal_NoSC");
                return msg;
            }

            try
            {
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.FSDisposalByClassCode,
                    JobRunType = jobRunBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(request)
                };

                string id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.None,
                        Extension = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while queuing FS disposal by class code. Error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
            }

            return msg;
        }

        public async Task<bool> HasAnyAvailableNodeForClassCodeDisposalAsync(Guid connectionGroupId, Guid nodeId, string fullPath)
        {
            bool isSpecificNode = nodeId != Guid.Empty && nodeId != connectionGroupId;

            if (isSpecificNode) //connection level
            {
                //var cosmosNode = explorerDao.GetFSRecordById(fullPath.ToLowerInvariant().ToMd5());

                //if (cosmosNode == null)
                //{
                //    logger.Info("Could not load connection from cosmos");
                //    return false;
                //}

                //if (string.IsNullOrEmpty(cosmosNode.ClassCode))
                //{
                //    logger.Info("Connection has not set class code. GroupId:[{0}], NodeId:[{1}]", connectionGroupId, nodeId);
                //    return false;
                //}
                return true;
            }

            var groupNode = new RMFSTreeNode
            {
                Id = connectionGroupId,
                ConnGroupId = connectionGroupId,
                Level = (int)NodeLevel.WebApplication,
                Parent = FSBrowerTreeService.LoadFSRoot()?.FirstOrDefault()
            };

            if (groupNode.Parent == null)
            {
                logger.Warn("Could not load FS root node. GroupId:[{0}]", connectionGroupId);
                return false;
            }

            var connections = await FSBrowerTreeService.FSBrowseAsync(groupNode).ConfigureAwait(false);
            if (connections == null || connections.Count == 0)
            {
                logger.Info("No connections found under group [{0}].", connectionGroupId);
                return false;
            }

            //var flagClassCode = false;
            //var cons = explorerDao.GetFSConnectionUnderGroup(connectionGroupId, 2);
            //foreach (var conn in cons)
            //{
            //    if (!string.IsNullOrEmpty(conn.ClassCode))
            //    {
            //        flagClassCode = true;
            //        break;
            //    }
            //}

            bool flagSchedule = true;
            bool isAllNotNull = true;
            bool hasValidConnection = false;

            foreach (var connection in connections)
            {
                if (IsConnectionDeleted(connection.Id) || IsDeactivedNode(connection))
                    continue;

                hasValidConnection = true;

                var schedule = await ScheduleService.GetScheduleByProfileIdAsync(BuildEnforceScheduleProfileId(connectionGroupId, connection.Id)).ConfigureAwait(false);

                if (schedule == null)
                {
                    isAllNotNull = false; 
                    break;
                }
            }

            if (hasValidConnection && isAllNotNull)
            {
                flagSchedule = false;
                logger.Info("All valid connections under group [{0}] have unique enforce rule schedule setting. No available node to run.", connectionGroupId);
            }

            //return flagClassCode && flagSchedule;
            return flagSchedule;
        }

        private static string BuildEnforceScheduleProfileId(Guid connectionGroupId, Guid connectionId)
        {
            return $"{connectionGroupId}|{connectionId}";
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSClassCodeDisposalJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunEnforceRuleWithClassCode, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunDisposalByClassCodeJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            if (string.IsNullOrEmpty(param))
            {
                throw new Exception("Request parameter is null for FSDisposalByClassCode.");
            }

            var request = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest>(param);

            if (request == null)
            {
                throw new Exception("Failed to deserialize FSDisposalByClassCodeRequest.");
            }

            return await RunDisposalByClassCodeInternalAsync(jobRunByUser, request);
        }

        private async Task<string> RunDisposalByClassCodeInternalAsync(string jobRunByUser, AvePoint.RA.Contract.JPMC.FSDisposalByClassCodeRequest request)
        {
            string jobId = string.Empty;
            var jobType = JobType.FSDisposalByClassCode;
            var groupId = request.ConnectionGroupID;

            try
            {
                var locationPath = ResolveClassCodeJobLocationPath(groupId, request.NodeId);
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, groupId.ToString(), null, locationPath);

                var defaultTermId = Guid.Empty;
                if (request.Level == (int)NodeLevel.WebApplication)
                {
                    defaultTermId = FileSystemSettingDao.GetSettingByConnGroupId(request.NodeId).DefaultTermId;
                    var conflictJobs = RMJobService.GetRunningJobs(JobTypeConstants.FSDisposalConflictType);
                    if (conflictJobs != null && conflictJobs.Count > 0)
                    {
                        if (OnlyHasRemoveAction(defaultTermId))
                        {
                            logger.Info("this fs disposal job only has remove action, no need to skip");
                        }
                        else
                        {
                            logger.Info("FS disposal by class code job has conflicts within the same connection group. JobId:{0}, GroupId:{1}", jobId, groupId);
                            JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            return jobId;
                        }
                    }

                    if (HasRunningJobOnNode(request.ConnectionGroupID.ToString(), request.ConnectionGroupID.ToString(), jobId, request.Level, jobType))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }
                else
                {
                    defaultTermId = explorerDao.GetFSRecordById(request.FullPath.ToLowerInvariant().ToMd5())?.TermId ?? Guid.Empty;
                    var conflictJobs = RMJobService.GetRunningJobs(JobTypeConstants.FSDisposalConflictType);
                    if (conflictJobs != null && conflictJobs.Count > 0)
                    {
                        if (OnlyHasRemoveAction(defaultTermId))
                        {
                            logger.Info("this fs disposal job only has remove action, no need to skip");
                        }
                        else
                        {
                            logger.Info("FS disposal by class code job has conflicts within the same connection group. JobId:{0}, GroupId:{1}", jobId, groupId);
                            JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            return jobId;
                        }
                    }
                    if (HasRunningJobOnNode(request.NodeId.ToString(), request.ConnectionGroupID.ToString(), jobId, request.Level, jobType))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                }


                var availableNodes = await GetAvailableNodesForClassCodeDisposalAsync(groupId, request.NodeId, request.TermID, jobId);

                if (availableNodes == null || availableNodes.Count == 0)
                {
                    logger.Warn("No matching nodes found for the provided class codes. JobId:{0}", jobId);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Finished, "RM_JM_FS_Disposal_NoSC");
                    return jobId;
                }

                var pauseNodes = availableNodes.Where(item => item.IsPause == 1).ToList();
                if (pauseNodes != null && pauseNodes.Count == availableNodes.Count)
                {
                    logger.Warn("All nodes is pause. JobId:{0}", jobId);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_FS_Disposal_NoSC_Pause");
                    return jobId;
                }
                var jobContent = SerializerHelper.SerializeByDataContractSerializer(request.TermID);
                await DispatchWithPerAgentCapacityAsync(jobId, jobType, availableNodes, AvePoint.Hybrid.Contract.JobType.FSDisposalByClassCode, true, jobContent);
               // await DispatchClassCodeSubJobsAsync(jobId, jobType, jobRunByUser, availableNodes, request.TermID);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred in FSDisposalByClassCode. JobId:{0} Error:{1}", jobId, e.ToString());
                if (e.Message == "RM_Job_ScheduledJobConflict")
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                else
                {
                    if (!string.IsNullOrEmpty(jobId))
                    {
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                    }
                }
            }

            return jobId;
        }

        private string ResolveClassCodeJobLocationPath(Guid groupId, Guid nodeId)
        {
            var group = FSConnectionGroupDao.GetGroupById(groupId);
            var groupName = group?.Name ?? groupId.ToString();

            if (nodeId == Guid.Empty || nodeId == groupId)
            {
                return groupName;
            }

            var connection = FSConnectionDao.GetConnectionById(nodeId);
            if (connection == null)
            {
                logger.Warn("Connection not found for NodeId:{0}, falling back to group name as location path.", nodeId);
                return groupName;
            }

            return groupName + "\\" + connection.Name;
        }

        private async Task<List<RMFSTreeNode>> GetAvailableNodesForClassCodeDisposalAsync(Guid groupId, Guid nodeId, List<Guid> classCodeTermIds, string jobId)
        {
            var availableNodes = new List<RMFSTreeNode>();
            var allSettings = FileSystemSettingDao.LoadAllSettingsUnderGroup(groupId);
            var matchingTermIds = new HashSet<Guid>(classCodeTermIds);

            var rootNode = FSBrowerTreeService.LoadFSRoot()?.FirstOrDefault();
            var groupNode = new RMFSTreeNode
            {
                Id = groupId,
                ConnGroupId = groupId,
                Level = (int)NodeLevel.WebApplication,
                Parent = rootNode
            };

            if (nodeId != Guid.Empty && nodeId != groupId)
            {
                return await GetSingleNodeForClassCodeAsync(nodeId, groupNode, allSettings, matchingTermIds, jobId);
            }

            var connections = await FSBrowerTreeService.FSBrowseAsync(groupNode);
            if (connections == null || connections.Count == 0)
            {
                return availableNodes;
            }

            foreach (var connection in connections)
            {
                if (IsConnectionDeleted(connection.Id))
                {
                    logger.Debug("Connection has been deleted, skipping. Id:{0}", connection.Id);
                    continue;
                }

                if (IsDeactivedNode(connection))
                {
                    logger.Debug("Connection is deactivated, skipping. Id:{0}", connection.Id);
                    continue;
                }

                var profileId = groupId.ToString() + "|" + connection.Id;
                var schedule = await ScheduleService.GetScheduleByProfileIdAsync(profileId);
                if (schedule != null)
                {
                    logger.Info("Connection has a unique enforcement schedule, skipping for group-level job. Id:{0}", connection.Id);
                    continue;
                }

                if (HasRunningJobOnNode(connection.Id.ToString(), groupId.ToString(), jobId, (int)NodeLevel.SiteCollection, JobType.FSDisposalByClassCode, connection))
                {
                    logger.Info("A disposal job is already running on connection, skipping. Id:{0}", connection.Id);
                    throw new Exception("RM_Job_ScheduledJobConflict");
                }

                connection.Parent = groupNode;
                connection.ConnGroupId = groupId;
                await LoadFSNodeSettingAsync(connection);
                availableNodes.Add(connection);
            }

            return availableNodes;
        }

        private async Task<List<RMFSTreeNode>> GetSingleNodeForClassCodeAsync(Guid nodeId, RMFSTreeNode groupNode, List<RMFileSystemSetting> allSettings, HashSet<Guid> matchingTermIds, string jobId)
        {
            var result = new List<RMFSTreeNode>();
            var connection = FSConnectionDao.GetConnectionById(nodeId);
            if (connection == null)
            {
                logger.Warn("Connection not found for NodeId:{0}", nodeId);
                return result;
            }

            //if (!NodeHasMatchingTermId(nodeId, groupNode.Id, allSettings, matchingTermIds))
            //{
            //    logger.Warn("Specified node does not match any class code TermId. NodeId:{0}", nodeId);
            //    return result;
            //}

            var node = new RMFSTreeNode
            {
                Id = nodeId,
                ConnGroupId = groupNode.Id,
                FullPath = connection.UNCPath,
                Name = connection.Name,
                Level = (int)NodeLevel.SiteCollection,
                Parent = groupNode,
                IsPause = connection.IsPause
            };

            if (IsDeactivedNode(node))
            {
                logger.Warn("Specified node is deactivated. NodeId:{0}", nodeId);
                return result;
            }

            //var profileId = groupNode.Id.ToString() + "|" + nodeId;
            //var schedule = await ScheduleService.GetScheduleByProfileIdAsync(profileId);
            //if (schedule != null)
            //{
            //    logger.Warn("Specified node has a unique enforcement schedule, cannot run group-level job. NodeId:{0}", nodeId);
            //    return result;
            //}

            if (HasRunningJobOnNode(nodeId.ToString(), groupNode.Id.ToString(), jobId, (int)NodeLevel.SiteCollection, JobType.FSDisposalByClassCode, node))
            {
                logger.Info("A disposal job is already running on connection, skipping. Id:{0}", nodeId);
                throw new Exception("RM_Job_ScheduledJobConflict");
            }

            await LoadFSNodeSettingAsync(node);
            result.Add(node);
            return result;
        }

        private bool NodeHasMatchingTermId(Guid connectionId, Guid groupId, List<RMFileSystemSetting> allSettings, HashSet<Guid> matchingTermIds)
        {
            var groupSetting = allSettings.FirstOrDefault(s => s.ScopeId == groupId);
            if (groupSetting != null && groupSetting.DefaultTermId != Guid.Empty && matchingTermIds.Contains(groupSetting.DefaultTermId))
            {
                return true;
            }

            var connectionSetting = allSettings.FirstOrDefault(s => s.ScopeId == connectionId);
            if (connectionSetting != null && connectionSetting.DefaultTermId != Guid.Empty && matchingTermIds.Contains(connectionSetting.DefaultTermId))
            {
                return true;
            }

            var connectionPath = connectionSetting?.FullPath;
            if (!string.IsNullOrEmpty(connectionPath))
            {
                return allSettings.Any(s =>
                    s.ScopeId != connectionId &&
                    s.ScopeId != groupId &&
                    !string.IsNullOrEmpty(s.FullPath) &&
                    s.FullPath.StartsWith(connectionPath + "\\", StringComparison.OrdinalIgnoreCase) &&
                    s.DefaultTermId != Guid.Empty &&
                    matchingTermIds.Contains(s.DefaultTermId));
            }

            return false;
        }

        private async Task DispatchClassCodeSubJobsAsync(string jobId, JobType jobType, string jobRunByUser, List<RMFSTreeNode> availableNodes, List<Guid> classCodeIds)
        {
            int subJobCount = availableNodes.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            var connGroupIds = availableNodes.Select(n => n.ConnGroupId).ToHashSet();
            var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(connGroupIds);

            ConcurrencyBudgetUtil concurrencyBudgetUtil = new ConcurrencyBudgetUtil();
            parallelSubJobCount = await concurrencyBudgetUtil.DetermineParallelSubJobCountAsync(TenantLocalValue.LogonGroupId, parallelSubJobCount);

            if (parallelSubJobCount == 0)
            {
                logger.Error("No available agent server for FSDisposalByClassCode. JobId:{0}", jobId);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return;
            }

            logger.Info("FSDisposalByClassCode - Total sub-jobs:{0}, Max parallel:{1}", subJobCount, parallelSubJobCount);


            var enabledJPMCFSFeature = FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature();
            logger.Info($"This tenant enable JPMC FS feature: {enabledJPMCFSFeature}");

            var classCodeContent = SerializerHelper.SerializeByDataContractSerializer(classCodeIds);

            int currentSubjobIndex = 0;
            foreach (RMFSTreeNode node in availableNodes)
            {
                var tempList = new List<RMFSTreeNode> { node };
                bool sendNow = currentSubjobIndex < parallelSubJobCount;

                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, sendNow, node.FullPath, classCodeContent);

                if (sendNow)
                {
                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(
                        new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = AvePoint.Hybrid.Contract.JobType.FSDisposalByClassCode,
                            TenantId = TenantLocalValue.LogonGroupId,
                            Extensions = enabledJPMCFSFeature
                                ? KeyNameCollection.EnableJPMCFileSystemFeature
                                : string.Empty
                        }, node.ConnGroupId);
                }

                currentSubjobIndex++;
            }
        }

        public async Task<string> GetDisposalByClassCodeJobMessageAsync(string subJobId)
        {
            try
            {
                logger.Debug("Start getting FSDisposalByClassCode job message. SubJobId:{0}", subJobId);

                var subJob = SubJobDao.GetSubJob(subJobId, true);
                if (subJob == null)
                {
                    logger.Error("Sub job not found. SubJobId:{0}", subJobId);
                    return string.Empty;
                }

                var connectionNode = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(subJob.JobContext.Settings).FirstOrDefault();

                if (connectionNode == null)
                {
                    logger.Error("Failed to deserialize connection node from sub job settings. SubJobId:{0}", subJobId);
                    return string.Empty;
                }

                var classCodeIds = !string.IsNullOrEmpty(subJob.JobContext?.Content)
                    ? SerializerHelper.DeserializeByDataContractSerializer<List<Guid>>(subJob.JobContext.Content)
                    : new List<Guid>();

                var groupId = connectionNode.ConnGroupId != Guid.Empty
                    ? connectionNode.ConnGroupId
                    : FindTop3LevelNodes(ConvertRMTree2FSTree(connectionNode)).Item2.Id;

                var targetNodes = BuildTargetNodesForClassCodeDisposal(connectionNode, groupId, classCodeIds);

                var classCodeInfoList = new List<ClassCodeInfoDto>();
                if (classCodeIds != null && classCodeIds.Any())
                {
                    FSClassCodeDto nodeClassCodeSetting = connectionNode?.ClassCode;
                    if (nodeClassCodeSetting == null || string.IsNullOrEmpty(nodeClassCodeSetting.ClassCodeId))
                    {
                        nodeClassCodeSetting = connectionNode?.Parent?.ClassCode;
                    }

                    if (nodeClassCodeSetting == null || string.IsNullOrEmpty(nodeClassCodeSetting.ClassCodeId))
                    {
                        var dbSetting = FileSystemSettingDao.LoadFSSetting(connectionNode.Id, groupId)
                            ?? FileSystemSettingDao.LoadFSSetting(groupId, groupId);

                        if (dbSetting != null && !string.IsNullOrEmpty(dbSetting.ClassCode))
                        {
                            nodeClassCodeSetting = new FSClassCodeDto
                            {
                                ClassCodeId = dbSetting.ClassCode,
                                CountryCode = dbSetting.CountryCode,
                                RetentionType = dbSetting.RetentionScheduleType,
                                StartDate = dbSetting.StartDate,
                                TermUniqueId = dbSetting.DefaultTermId.ToString(),
                                ApplyExistDocuments = dbSetting.ApplyExistDocument
                            };
                            logger.Info("ClassCode setting loaded from DB for connection {0}. ClassCode: {1}", connectionNode.Id, nodeClassCodeSetting.ClassCodeId);
                        }
                        else
                        {
                            logger.Warn("No class code setting found for connectionNode {0}, groupId {1}. ClassCodeInfoList will have empty values.", connectionNode.Id, groupId);
                        }
                    }

                    if (nodeClassCodeSetting != null)
                    {
                        try
                        {
                            var cosmosNodeId = connectionNode.Level == (int)NodeLevel.SiteCollection
                                ? connectionNode.FullPath.ToLowerInvariant().ToMd5()
                                : connectionNode.Id;
                            var cosmosRecord = ExplorerService.GetFSDBRecords(new List<Guid> { cosmosNodeId }).FirstOrDefault();
                            if (cosmosRecord != null && cosmosRecord.EndTime != 0)
                            {
                                nodeClassCodeSetting.EndTime = cosmosRecord.EndTime;
                                logger.Info("EndTime loaded from Cosmos for connectionNode {0}. EndTime: {1}", connectionNode.Id, cosmosRecord.EndTime);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn("Failed to load EndTime from Cosmos for connectionNode {0}. Error: {1}", connectionNode.Id, ex.Message);
                        }
                    }

                    foreach (var termId in classCodeIds)
                    {
                        var term = TermDao.GetRMTermByGuId(termId);
                        if (term != null)
                        {
                            var dateTimeDtoForEndTime = TaxonomyService.GetTheRetentionUnitByClassCode(new ApplyClassCodeSettingDto()
                            {
                                TermId = termId.ToString(),
                                CountryCode = nodeClassCodeSetting?.CountryCode,
                                RetentionType = (int)(nodeClassCodeSetting?.RetentionType ?? 0)
                            });

                            var classCodeInfo = new ClassCodeInfoDto()
                            {
                                TermId = termId,
                                ClassCode = term.Name,
                                CountryCode = nodeClassCodeSetting?.CountryCode,
                                RetentionType = (int)(nodeClassCodeSetting?.RetentionType ?? 0),
                                StartDate = nodeClassCodeSetting?.StartDate ?? 0,
                                EndTime = nodeClassCodeSetting?.EndTime ?? 0,
                                ApplyExistDocuments = nodeClassCodeSetting?.ApplyExistDocuments ?? false,
                                PolicyValueUnit = dateTimeDtoForEndTime != null ? (int)dateTimeDtoForEndTime.PolicyValueUnit : 0,
                                PolicyValueNumber = dateTimeDtoForEndTime != null ? dateTimeDtoForEndTime.Number : 0
                            };

                            classCodeInfoList.Add(classCodeInfo);
                        }
                    }
                }

                logger.Info("FSDisposalByClassCode - Built {0} target nodes for agent. SubJobId:{1}", targetNodes.Count, subJobId);

                var jobMsg = new AvePoint.RA.Contract.Global.Object.FSJobMessage()
                {
                    Job = new BaseJobDto() { Id = subJob.Id, JobType = subJob.JobType },
                    JobId = subJobId,
                    JobType = JobType.FSDisposalByClassCode,
                    ClassCodeIds = classCodeIds,
                    FSTreeNodes = targetNodes,
                    ClassCodeInfoList = classCodeInfoList
                };

                await AssembleCacheDataForDisposalAsync(groupId, jobMsg);

                jobMsg.BreakTreeNodeUrls = FSBuildBreakTreeNode(connectionNode);
                jobMsg.RunningJobNodeUrls = FSBuildRunningJobNode(connectionNode, JobType.FSDisposalByClassCode, subJobId);
                jobMsg.ConnectionCache = GetConnectionCache();
                jobMsg.ClassificationLevel = GetClassificationLevel();

                var generalSetting = await GetGeneralSettingModelAsync();
                if (generalSetting != null)
                {
                    jobMsg.GeneralSettingModel = SerializerHelper.SerializeByDataContractSerializer(generalSetting);
                    jobMsg.TimeFormat = DateTimeUtil.GetAllStaticTimeZones()
                        .Where(x => x.Id == GeneralSettingConfig
                            .GetTimeZoneInforById(generalSetting.TimeZoneId).Id)
                        .FirstOrDefault()?.DisplayName;
                }

                bool isCosmosBulkEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
                if (isCosmosBulkEnabled)
                {
                    jobMsg.BulkImportEnabled = true;
                    int bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int))
                    {
                        bulkSize = DB.Explorer.Bulk.CosmosBulkOperator.DefualtBufferSize;
                    }
                    logger.Info("Cosmos bulk operation enabled for FSDisposalByClassCode. BulkSize:{0}", bulkSize);
                    jobMsg.BulkSize = bulkSize;
                }

                return SerializerHelper.SerializeByDataContractSerializer(jobMsg);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred getting FSDisposalByClassCode job message. SubJobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Finished getting FSDisposalByClassCode job message. SubJobId:{0}", subJobId);
            }
        }

        private List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> BuildTargetNodesForClassCodeDisposal(RMFSTreeNode connectionNode, Guid groupId, List<Guid> classCodeIds)
        {
            var result = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();
            if (classCodeIds == null || classCodeIds.Count == 0)
            {
                return result;
            }

            var matchingTermIds = new HashSet<Guid>(classCodeIds);

            var allGroupSettings = FileSystemSettingDao.LoadAllSettingsUnderGroup(groupId);

            var groupSetting = allGroupSettings.FirstOrDefault(s => s.ScopeId == groupId);

            var connectionSettingsAndFolders =
                FileSystemSettingDao.LoadAllSettingsByConnectionGroupIdAndConnectionPath(
                    groupId, connectionNode.FullPath);

            var connectionSetting = connectionSettingsAndFolders.FirstOrDefault(s => s.ScopeId == connectionNode.Id);

            var connectionFsNode = ConvertRMTree2FSTree(connectionNode);
            var scopeId = connectionFsNode.Id;

            bool anyExplicitMatch = false;

            if (connectionSetting != null
                && connectionSetting.DefaultTermId != Guid.Empty
                && matchingTermIds.Contains(connectionSetting.DefaultTermId))
            {
                result.Add(connectionFsNode);
                anyExplicitMatch = true;
                logger.Debug("Connection-level setting matches class code. ConnectionId:{0}", connectionNode.Id);
            }
            else
            {
                var isHaveFileMatch = ExplorerService.HasFileMatchTerm(connectionFsNode.FullPath, scopeId.ToString(), classCodeIds);
                if (isHaveFileMatch)
                {
                    result.Add(connectionFsNode);
                    anyExplicitMatch = true;
                    logger.Debug("Connection-level setting matches class code. ConnectionId:{0}", connectionNode.Id);
                }
            }
            var folderSettings = connectionSettingsAndFolders
                .Where(s =>
                    s.ScopeId != connectionNode.Id &&
                    s.ScopeId != groupId &&
                    !string.IsNullOrEmpty(s.FullPath) &&
                    s.FullPath.StartsWith(connectionNode.FullPath + "\\", StringComparison.OrdinalIgnoreCase) &&
                    s.DefaultTermId != Guid.Empty &&
                    matchingTermIds.Contains(s.DefaultTermId))
                .ToList();

            foreach (var folderSetting in folderSettings)
            {
                var folderTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(folderSetting.NodeInfo);
                var profileId = ScheduleService.GetProfileId(folderTreeNode);
                var schedule = ScheduleService.GetScheduleByProfileIdAsync(profileId);
                if (schedule != null)
                {
                    logger.Debug($"Folder {folderSetting.FullPath} has disposal schedule setitng, skipped");
                    continue;
                }
                else
                {
                    var folderNode = BuildFolderNodeFromSetting(folderSetting, connectionNode, connectionFsNode);
                    if (folderNode != null)
                    {
                        result.Add(folderNode);
                        anyExplicitMatch = true;
                        logger.Debug("Folder-level setting matches class code. FolderPath:{0}", folderSetting.FullPath);
                    }
                    else
                    {
                        var isHaveFileMatch = ExplorerService.HasFileMatchTerm(folderSetting.FullPath, scopeId.ToString(), classCodeIds);
                        if (isHaveFileMatch)
                        {
                            result.Add(folderNode);
                            anyExplicitMatch = true;
                            logger.Debug("Folder-level setting matches class code. FolderPath:{0}", folderSetting.FullPath);
                        }
                    }
                }

            }

            if (!anyExplicitMatch
                && groupSetting != null
                && groupSetting.DefaultTermId != Guid.Empty
                && matchingTermIds.Contains(groupSetting.DefaultTermId))
            {
                result.Add(connectionFsNode);
                logger.Debug("Group-level inherited setting matches class code. ConnectionId:{0}", connectionNode.Id);
            }

            return result;
        }

        private AvePoint.RA.Contract.Global.Object.FSTreeNodeDto BuildFolderNodeFromSetting(RMFileSystemSetting folderSetting, RMFSTreeNode connectionNode, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto connectionFsNode)
        {
            try
            {
                var folderFullPath = folderSetting.FullPath;
                var folderName = folderFullPath.Contains("\\")
                    ? folderFullPath.Substring(folderFullPath.LastIndexOf('\\') + 1)
                    : folderFullPath;

                var folderNode = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto
                {
                    Id = folderSetting.ScopeId,
                    FullPath = folderFullPath,
                    Name = folderName,
                    Level = (int)NodeLevel.FSFolder,
                    ConnGroupId = connectionNode.ConnGroupId,
                    Parent = connectionFsNode,
                    DefaultTermId = folderSetting.DefaultTermId,
                    TermId = folderSetting.TermId,
                    TermSetId = folderSetting.TermSetId
                };

                return folderNode;
            }
            catch (Exception ex)
            {
                logger.Warn("Failed to build folder node from setting. ScopeId:{0} FullPath:{1} Error:{2}",
                    folderSetting.ScopeId, folderSetting.FullPath, ex.Message);
                return null;
            }
        }

        #endregion

        #region Download RCC report

        public RAReturnMessage RunDownloadRCCReportJob(RCCReportRequest request, JobRunBy jobRunBy)
        {
            logger.Debug("Start Download RCC Report");
            RAReturnMessage msg = new RAReturnMessage();

            try
            {
                if (request.TimeRange != null)
                {
                    var generalSetting = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();

                    if (request.TimeZoneId == null)
                    {
                        request.TimeZoneId = generalSetting?.TimeZoneId ?? TimeZoneInfo.Local.Id;
                        request.IsDaylight = generalSetting?.DayLight ?? false;
                    }
                    else
                    {
                        request.TimeZoneId = DateTimeUtil.AllTimeZones[Convert.ToInt32(request.TimeZoneId)];
                    }

                    TimeZoneInfo timeZone;
                    try
                    {
                        timeZone = TZConvert.GetTimeZoneInfo(request.TimeZoneId);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Failed to load TimeZone for '{request.TimeZoneId}'. Falling back to UTC. Error: {ex.Message}");
                        timeZone = TimeZoneInfo.Utc;
                        request.TimeZoneId = timeZone.Id;
                    }

                    var (start, end) = request.TimeRange.Resolve(timeZone);

                    request.TimeRange.StartDateTicks = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(start, DateTimeKind.Unspecified), timeZone).Ticks;
                    request.TimeRange.EndDateTicks = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(end, DateTimeKind.Unspecified), timeZone).Ticks;

                    if (request.TimeRange.StartDateTicks > request.TimeRange.EndDateTicks)
                    {
                        logger.Warn("Invalid time range for RCC report. Start:{0} End:{1}", request.TimeRange.StartDateTicks, request.TimeRange.EndDateTicks);
                        msg = new RAReturnMessage()
                        {
                            MessageType = RAMessageType.Failed,
                            FaildType = RAFailedType.None,
                            ErrorMessage = "The start date cannot be later than the end date.",
                            Extension = string.Empty
                        };
                        return msg;
                    }
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;

                //Handle Display name
                var connection = FSConnectionDao.GetConnectionById(request.ConnectionId);
                if (connection != null)
                {
                    request.DisplayName = ResolvedRCCDisplayName(connection);
                }

                if (request.IsMyHub)
                {
                    var curUser = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);

                    var connids = RMFSConnAndOwnerRela.GetConnectionsByUserIdsAndRoles(curUser.Distinct().ToList()).Result.Select(c => c.Id).ToHashSet();

                    var flag = connids.Contains(request.ConnectionId);

                    if (!flag)
                    {
                        logger.Warn("Current user is not an owner of the connection. UserId:{0}, ConnectionId:{1}", TenantLocalValue.LogonUserId, request.ConnectionId);
                        msg = new RAReturnMessage()
                        {
                            MessageType = RAMessageType.Failed,
                            FaildType = RAFailedType.None,
                            ErrorMessage = "Current user is not an owner of the connection.",
                            Extension = string.Empty
                        };
                        return msg;
                    }
                }

                var requestJson = JsonConvert.SerializeObject(request);
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DownloadRCCReport,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = requestJson,
                };

                var id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.None,
                        Extension = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while creating RCC report job. ERROR:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }

            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GenerateRCCReport, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        public async Task<string> RealRunDownloadRCCReportJobAsync(JobRunBy jobRunBy, string jobRunByUser, string requestJson)
        {
            logger.Debug("Start real run Download RCC Report job.");
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string userId = account?.UserId ?? string.Empty;
            var request = JsonConvert.DeserializeObject<RCCReportRequest>(requestJson);

            var scopeId = string.Empty;
            var scopeName = string.Empty;
            //var nodeSetting = new RMFSTreeNode();
            List<RCCReportContentDto> reportContents = new List<RCCReportContentDto>();

            string jobId = string.Empty;

            if (request.Nodes.Count == 1)
            {
                var node = request.Nodes.FirstOrDefault();
                //nodeSetting = JsonConvert.DeserializeObject<RMFSTreeNode>(FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId).NodeInfo);

                scopeId = node.Id.ToString();
                scopeName = node.Name;
                //jobId = RMJobService.CreateJob(JobType.DownloadRCCReport, jobRunByUser, userId, scopeId, scopeName);

                RCCReportContentDto reportContent = new RCCReportContentDto()
                {
                    TimeRange = request.TimeRange,
                    Level = request.Level,
                    NodeId = node.Id.ToString(),
                };

                reportContents.Add(reportContent);
            }
            else
            {
                scopeName = string.Join(" | ", request.Nodes.Select(n => n.FullPath));
                foreach (var node in request.Nodes)
                {
                    //nodeSetting = JsonConvert.DeserializeObject<RMFSTreeNode>(FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId).NodeInfo);

                    scopeId = node.Id.ToString();
                    //jobId = RMJobService.CreateJob(JobType.DownloadRCCReport, jobRunByUser, userId, scopeId, scopeName);

                    RCCReportContentDto reportContent = new RCCReportContentDto()
                    {
                        TimeRange = request.TimeRange,
                        Level = request.Level,
                        NodeId = node.Id.ToString(),
                    };

                    reportContents.Add(reportContent);
                }
            }

            jobId = RMJobService.CreateJob(JobType.DownloadRCCReport, jobRunByUser, userId, scopeId, scopeName);
            string fileName = request.IsMyHub ? request.DisplayName : (jobId + ".zip");
            var downloadDataInfo = new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = userId,
                Name = fileName,
                DownloadType = DownloadContentType.DownloadRCCReport,
                ExtendString1 = JsonConvert.SerializeObject(reportContents),
            };

            DownloadDataInfoDao.Create(downloadDataInfo);
            await MyhubReportJobDao.CreateJobReports(downloadDataInfo);

            logger.Info("Created DownloadDataInfo for RCC report. JobId: {0}", jobId);

            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.DownloadRCCReport,
                RunBy = jobRunBy,
                CommandLine = string.Format("{0} {1}", JobType.DownloadRCCReport, jobId),
                Extension = requestJson,
            });

            logger.Info("Dispatched RCC report job to queue. JobId: {0}", jobId);

            if (request.IsMyHub)
            {
                await AuditSinkService.RCCFlushAsync(request, string.Empty);
            }
            else
            {
                await AuditSinkService.RCCFlushAsync(request, jobId);
            }
            return jobId;
        }

        private string ResolvedRCCDisplayName(FSConnection connection)
        {
            if (connection == null)
            {
                logger.Warn("Connection not found.");
                return string.Empty;
            }

            string jpmcId = SanitizeFileName(connection.JPMCConnectionId);
            string connName = SanitizeFileName(connection.Name ?? connection.UNCPath);

            if (string.IsNullOrWhiteSpace(jpmcId) && string.IsNullOrWhiteSpace(connName))
            {
                logger.Warn("Connection produced an empty file name after sanitization.");
                return string.Empty;
            }

            jpmcId = TruncateString(jpmcId, 100);
            connName = TruncateString(connName, 100);
            return $"RCC_Report_{jpmcId}_{connName}.zip";
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length > maxLength ? text[..maxLength] : text;
        }

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (Array.IndexOf(invalidChars, c) >= 0)
                {
                    sb.Append('_');
                }
                else if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }

            string sanitized = sb.ToString();
            sanitized = Regex.Replace(sanitized, @"_{2,}", "_");

            return sanitized.Trim();
        }


        #endregion

        #region auto tool functions

        #endregion
        #region public method

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSCollectionJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSCollectionJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunFSDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.FSDataSynchronization : JobType.FSDataSynchronizationSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);
            //Skip if a schedule job is running
            List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.FSDataSynchronizationSchedule);
            if (!runningJobIds.IsNullOrEmpty())
            {
                logger.Info("Current running scheduled data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A File System Data Synchronization job is already running.");
                return jobId;
            }
            else
            {
                return await RunFSDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSCollectionJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSCollectionJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.FSDataSynchronization;
            if (string.IsNullOrEmpty(param))
            {
                List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.FSDataSynchronizationSchedule);
                if (!runningJobIds.IsNullOrEmpty())
                {
                    logger.Info("Current running scheduled data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                    string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A File System Data Synchronization job is already running.");
                    return jobId;
                }
                return await RunFSDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, JobType.FSDataSynchronizationSchedule);
            }
            else
            {
                RMFSTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(param);
                return await RunDataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ImportFSSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.ImportSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunImportFSSettingJob(JobRunBy jobRunBy, string jobRunByUser, string extension, string strBytes)
        {
            JobType jobType = JobType.ImportFSSetting;
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string jobId = RMJobService.CreateJob(jobType, jobRunByUser, account?.UserId);
            List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.ImportFSSetting);
            var skip = runningJobIds.Any(j => j != jobId);
            if (!skip)
            {
                logger.Info("Start to import file system setting");
                StartImportFSSettingJob(jobId, extension, strBytes);
                return jobId;
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A File System Data Synchronization job is already running.");
                return "";
            }
        }
        private void StartImportFSSettingJob(string jobId, string extension, string strBytes)
        {
            string content = "\"" + strBytes + "\"";
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = JobType.ImportFSSetting,
                CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportFSSetting, jobId, extension, content),
            });
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSDisposalJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSDisposalJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.FSDisposal;
            if (string.IsNullOrEmpty(param))
            {
                throw new Exception("param is null");
                //return RunFSDisposalJobAllSettingNode(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMFSTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(param);
                return await RunDisposalJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }
        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSApplyClassCodeJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.ApplyClassCodeSettings4FS, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunApplyClassCodeJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.ApplyClassCode;
            if (string.IsNullOrEmpty(param))
            {
                throw new Exception("param is null");
                //return RunFSDisposalJobAllSettingNode(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                ApplyClassCodeSettingDto settingDto = SerializerHelper.DeserializeByDataContractSerializer<ApplyClassCodeSettingDto>(param);
                return await RunApplyClassCodeJobBySelectdNodeAsync(jobRunByUser, jobType, settingDto);
            }
        }
        public async Task<string> RunApplyClassCodeJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, ApplyClassCodeSettingDto settingDto)
        {
            string jobId = string.Empty;
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            List<RMFSTreeNode> selectedNodes = settingDto?.FSTreeNode?.Where(node => node != null).ToList() ?? new List<RMFSTreeNode>();
            bool isRunJobOnWebApplication = false;
            bool isRunJobOnWebApplicationForJob = false;
            bool isApplyForAllChild = false;
            try
            {
                if (selectedNodes.IsNullOrEmpty())
                {
                    logger.Warn("No selected nodes found for apply class code.");
                    return jobId;
                }

                RMFSTreeNode firstNode = selectedNodes.First();
                jobId = CreateApplyClassCodeJob(jobRunByUser, jobType, firstNode);

                foreach (RMFSTreeNode selectedNode in selectedNodes)
                {
                    if (selectedNode.Level == (int)NodeLevel.WebApplication)
                    {
                        isRunJobOnWebApplication = true;
                        isRunJobOnWebApplicationForJob = true;
                        isApplyForAllChild = settingDto.ApplyToExistingDoc;
                        availableNode.AddRange(await GetIncludeNodeListForApplyClassCodeAsync(selectedNode, jobId, isApplyForAllChild));
                    }
                    else
                    {
                        //if (IsDeactivedNode(selectedNode))
                        //{
                        //    logger.Warn("This node is deactived, job will be skipped. JobId:{0} NodeId:{1}", jobId, selectedNode.Id);
                        //    continue;
                        //}

                        var nodeDto = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(selectedNode) };
                        if (nodeDto[0].Parent != null)
                        {
                            var top3Nodes = FindTop3LevelNodes(nodeDto[0]);
                            var scopeId = top3Nodes.Item3.Id.ToString();

                            if (IsConnectionDeleted(new Guid(scopeId)))
                            {
                                logger.Debug($"Current connection has been deleted, will not start disposal job Id:{scopeId}");
                                continue;
                            }
                        }
                        availableNode.Add(selectedNode);
                    }
                }

                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available connection to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JM_FS_ApplyClassCode_NoSC");
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                if (!string.IsNullOrEmpty(jobId)) RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
            var connGroupIds = availableNode.Select(item => item.ConnGroupId).ToHashSet();
            logger.Info($"Total sub-jobs for apply class code: {availableNode.Count}");
            var enabledJPMCFSFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            logger.Info($"This tenant enable JPMC FS feature: {enabledJPMCFSFeature}");

            //foreach (RMFSTreeNode site in availableNode)
            //{
            //    tempList.Add(site);
            //    {
            //        ApplyClassCodeSettingDto subJobSettingDto = new ApplyClassCodeSettingDto()
            //        {
            //            ApplyToExistingDoc = settingDto.ApplyToExistingDoc,
            //            ClassCode = settingDto.ClassCode,
            //            CountryCode = settingDto.CountryCode,
            //            RetentionType = settingDto.RetentionType,
            //            StartDate = settingDto.StartDate,
            //            FSTreeNode = new List<RMFSTreeNode>() { site },
            //            TermId = settingDto.TermId,
            //            IsConnectionGroup = isRunJobOnWebApplicationForJob
            //        };
            //        if (isRunJobOnWebApplication)
            //        {
            //            subJobSettingDto.NeedToUpdateConnectionGroup = true;
            //            isRunJobOnWebApplication = false;
            //        }
            //        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, site.FullPath, SerializerHelper.SerializeByDataContractSerializer(subJobSettingDto));
            //        if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
            //        {
            //            mJobQueueService.HandleMessage(new JobQueueMessage()
            //            {
            //                JobId = subJobId,
            //                RunBy = JobRunBy.Control,
            //                JobType = jobType,
            //                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            //            });
            //        }
            //        tempList.Clear();
            //        currentSubjobIndex++;
            //    }
            //}
            foreach (RMFSTreeNode site in availableNode)
            {
                tempList.Add(site);
                {
                    ApplyClassCodeSettingDto subJobSettingDto = new ApplyClassCodeSettingDto()
                    {
                        ApplyToExistingDoc = settingDto.ApplyToExistingDoc,
                        ClassCode = settingDto.ClassCode,
                        CountryCode = settingDto.CountryCode,
                        RetentionType = settingDto.RetentionType,
                        StartDate = settingDto.StartDate,
                        FSTreeNode = new List<RMFSTreeNode>() { site },
                        TermId = settingDto.TermId,
                        IsConnectionGroup = isRunJobOnWebApplicationForJob,
                        IsMyhubClassify = settingDto.IsMyhubClassify
                    };
                    if (isRunJobOnWebApplication)
                    {
                        subJobSettingDto.NeedToUpdateConnectionGroup = true;
                        isRunJobOnWebApplication = false;
                    }
                    string settingData = SerializerHelper.SerializeByDataContractSerializer(subJobSettingDto);
                    bool canExecuteNow = currentSubjobIndex < subJobCountInConfigFile;
                    string subJobId;
                    if (subJobSettingDto.IsMyhubClassify)
                        RMFileSystemSettingsCreateSubJobService.CreateAndExecuteMyhubSubJobWithAudit(jobId, currentSubjobIndex, jobType, subJobCount, tempList, canExecuteNow, site.FullPath, settingData, out subJobId);
                    else
                        RMFileSystemSettingsCreateSubJobService.CreateAndExecuteSubJobWithAudit(jobId, currentSubjobIndex, jobType, subJobCount, tempList, canExecuteNow, site.FullPath, settingData, out subJobId);

                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            return jobId;
        }
        private string CreateApplyClassCodeJob(string jobRunByUser, JobType jobType, RMFSTreeNode selectedNode)
        {
            var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.ConnGroupId.ToString(), account.UserId, selectedNode.Name);
            }

            var nodeDto = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(selectedNode) };
            string scopeId = string.Empty;
            string fullPath = string.Empty;
            if (nodeDto[0].Parent != null)
            {
                var top3Nodes = FindTop3LevelNodes(nodeDto[0]);
                scopeId = top3Nodes.Item3.Id.ToString();
                var lastIndex = selectedNode.FullPath.LastIndexOf(top3Nodes.Item3.FullPath);
                var splitPath = lastIndex + top3Nodes.Item3.FullPath.Length;
                fullPath = top3Nodes.Item2.Name + "\\" + top3Nodes.Item3.Name + selectedNode.FullPath.Substring(splitPath);
            }
            else
            {
                scopeId = nodeDto[0].Id.ToString();
                fullPath = nodeDto[0].FullPath;
            }

            return RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, scopeId, account.UserId, fullPath);
        }
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunFSRestoreJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSRestoreJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunFSRestoreJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.FSArchiverRestore;
            if (string.IsNullOrEmpty(param))
            {
                throw new Exception("param is null");
            }
            else
            {
                RestoreInfo selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RestoreInfo>(param);
                return await RunFSRestoreJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }
        public async Task<string> RealRunDisposalJobForApprovalAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            return await RunDisposalJobBySelectdNodeForApprovalAsync(jobRunByUser, JobType.FSDisposal);
        }
        public async Task<string> GetRetentionUnitAsync(ApplyClassCodeSettingDto dto)
        {
            var result = TaxonomyService.GetTheRetentionUnitByClassCode(dto);
            if (result != null)
            {
                OlderThanTimeDtoForAgent resultDto = new OlderThanTimeDtoForAgent()
                {
                    Number = result.Number,
                    PolicyValueUnit = (int)result.PolicyValueUnit
                };
                return SerializerHelper.SerializeByDataContractSerializer(resultDto);
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task<string> GetJobMessageAsync(string subJobId)
        {
            try
            {
                logger.Debug("Start to get job message. Job Id:" + subJobId);
                var subJob = SubJobDao.GetSubJob(subJobId, true);

                BaseJobDto jobDto = new BaseJobDto()
                {
                    Id = subJob.Id,
                    JobType = subJob.JobType
                };

                RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(subJob.JobContext.Settings).FirstOrDefault();
                AvePoint.RA.Contract.Global.Object.FSJobMessage jobMsg = new AvePoint.RA.Contract.Global.Object.FSJobMessage();
                jobMsg.Job = jobDto;
                jobMsg.JobId = subJobId;
                jobMsg.FSTreeNodes = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(node) };

                AvePoint.RA.Contract.Explorer.FileSystemRecordDto folderDBRecord = null;
                try
                {
                    folderDBRecord = ExplorerService.GetFSDBRecords(new List<Guid>() { jobMsg.FSTreeNodes[0].FullPath.ToLower().ToMd5() }).FirstOrDefault();
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while get record for fs data job message. Error:{e.ToString()}");
                }
                jobMsg.FolderTermId = folderDBRecord != null ? folderDBRecord.TermId.ToString() : string.Empty;
                var top3Nodes = FindTop3LevelNodes(jobMsg.FSTreeNodes[0]);
                await AssembleCacheDataAsync(top3Nodes.Item2.Id, jobMsg);
                jobMsg.RunningJobNodeUrls = FSBuildRunningJobNode(node, JobType.FSDataSynchronization, subJobId);
                var result = GetJobType(node);
                jobMsg.FSJobType = result.Item1;
                jobMsg.TermConflictOption = result.Item2;
                jobMsg.IBStartTime = result.Item3;
                jobMsg.NeedChangeProfile = result.Item4;
                jobMsg.RecordOwner = result.Item5;
                jobMsg.CurrentSettingScopeId = result.Item6;
                jobMsg.ChangedTermIds = result.Item7;
                jobMsg.ClassificationLevel = GetClassificationLevel();
                try
                {
                    FSClassCodeDto tempClassCodeSetting = node?.ClassCode;
                    if (tempClassCodeSetting != null && !string.IsNullOrEmpty(tempClassCodeSetting.ClassCodeId))
                    {
                        logger.Info($"this sync job will use the connection classcode setting,class code:{tempClassCodeSetting?.ClassCodeId}");
                    }
                    else
                    {
                        tempClassCodeSetting = node.Parent.ClassCode;
                        logger.Info($"this sync job will use the parent classcode setting,class code:{tempClassCodeSetting?.ClassCodeId}");
                    }
                    var dateTimeDtoForEndTime = TaxonomyService.GetTheRetentionUnitByClassCode(new ApplyClassCodeSettingDto()
                    {
                        TermId = tempClassCodeSetting?.TermUniqueId,
                        CountryCode = tempClassCodeSetting?.CountryCode,
                        RetentionType = (int)tempClassCodeSetting?.RetentionType
                    });
                    jobMsg.ClassCodeDto = new ClassCodeInfoDto()
                    {
                        ClassCode = tempClassCodeSetting?.ClassCodeId,
                        CountryCode = tempClassCodeSetting?.CountryCode,
                        RetentionType = (int)tempClassCodeSetting?.RetentionType,
                        StartDate = tempClassCodeSetting.StartDate,
                        TermId = new Guid(tempClassCodeSetting?.TermUniqueId),
                        PolicyValueUnit = (int)dateTimeDtoForEndTime.PolicyValueUnit,
                        PolicyValueNumber = dateTimeDtoForEndTime.Number,
                        ApplyExistDocuments = tempClassCodeSetting.ApplyExistDocuments
                    };
                }
                catch (Exception e)
                {
                    logger.Error($"GetJobMessageAsync error. Job Id:{subJobId},error:{e}");
                }
                var generalSetting = await GetGeneralSettingModelAsync();
                if (generalSetting != null)
                {
                    jobMsg.GeneralSettingModel = SerializerHelper.SerializeByDataContractSerializer(generalSetting);
                    jobMsg.TimeFormat = DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == GeneralSettingConfig.GetTimeZoneInforById(generalSetting.TimeZoneId).Id)?.FirstOrDefault()?.DisplayName;
                }
                bool isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
                if (isCosmosBulkOperationEnabled)
                {
                    jobMsg.BulkImportEnabled = true;
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int))
                    {
                        bulkSize = DB.Explorer.Bulk.CosmosBulkOperator.DefualtBufferSize;
                    }
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    jobMsg.BulkSize = bulkSize;
                }
                return SerializerHelper.SerializeByDataContractSerializer(jobMsg);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get job message finished. Job Id: " + subJobId);
            }
        }

        public int GetClassificationLevel()
        {
            RMFunctionSetting setting;
            RMFunctionSettingDao.TryGet(Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
            NodeLevel result;
            if (setting == null)
            {

                return (int)NodeLevel.FSFile;
            }
            if (Enum.TryParse<NodeLevel>(setting.SettingInfo, out result))
            {
                if (RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled && result == NodeLevel.FSFolder)
                {
                    return (int)NodeLevel.FSFile;
                }
                return (int)result;
            }
            logger.Warn("Error while getting classification level, default is folder");
            return (int)NodeLevel.FSFolder;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.FSClassificationSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.FSClassificationSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public System.Threading.Tasks.Task SetClassificationLevelAsync(int classificationLevel)
        {
            string newClassifyLevel = ((NodeLevel)classificationLevel).ToString();
            logger.Info("Set mew Classification Level {0}", newClassifyLevel);
            return RMFunctionSettingDao.AddOrUpdateSettingInfoAsync(Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, newClassifyLevel);
        }
        private async Task<GeneralSettingModel> GetGeneralSettingModelAsync()
        {
            var timeSetting = await GeneralSettingService.GetGeneralSettingAsync();
            return timeSetting;
        }

        public async Task<string> GetDisposalJobMessageAsync(string subJobId)
        {
            try
            {
                logger.Debug("Start to get disposal job message. Job Id:{0}", subJobId);
                var subJob = SubJobDao.GetSubJob(subJobId, true);

                BaseJobDto jobDto = new BaseJobDto()
                {
                    Id = subJob.Id,
                    JobType = subJob.JobType
                };

                RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(subJob.JobContext.Settings).FirstOrDefault();
                AvePoint.RA.Contract.Global.Object.FSJobMessage jobMsg = new AvePoint.RA.Contract.Global.Object.FSJobMessage();
                jobMsg.Job = jobDto;
                jobMsg.JobId = subJobId;
                jobMsg.FSTreeNodes = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(node) };
                var top3Nodes = FindTop3LevelNodes(jobMsg.FSTreeNodes[0]);
                await AssembleCacheDataForDisposalAsync(top3Nodes.Item2.Id, jobMsg);
                jobMsg.BreakTreeNodeUrls = FSBuildBreakTreeNode(node);
                jobMsg.RunningJobNodeUrls = FSBuildRunningJobNode(node, JobType.FSDisposal, subJobId);
                jobMsg.ConnectionCache = GetConnectionCache();
                jobMsg.ClassificationLevel = this.GetClassificationLevel();
                //var result = GetJobType(node);
                //jobMsg.FSJobType = result.Item1;
                //jobMsg.TermConflictOption = result.Item2;
                //jobMsg.IBStartTime = result.Item3;
                //jobMsg.NeedChangeProfile = result.Item4;
                //jobMsg.RecordOwner = result.Item5;
                var generalSetting = await GetGeneralSettingModelAsync();
                if (generalSetting != null)
                {
                    jobMsg.GeneralSettingModel = SerializerHelper.SerializeByDataContractSerializer(generalSetting);
                    jobMsg.TimeFormat = DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == GeneralSettingConfig.GetTimeZoneInforById(generalSetting.TimeZoneId).Id).FirstOrDefault()?.DisplayName;
                }
                bool isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
                if (isCosmosBulkOperationEnabled)
                {
                    jobMsg.BulkImportEnabled = true;
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int))
                    {
                        bulkSize = DB.Explorer.Bulk.CosmosBulkOperator.DefualtBufferSize;
                    }
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    jobMsg.BulkSize = bulkSize;
                }
                return SerializerHelper.SerializeByDataContractSerializer(jobMsg);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting disposal job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get disposal job message finished. Job Id: " + subJobId);
            }
        }
        public async Task<string> GetFSRestoreJobMessageAsync(string subJobId)
        {
            try
            {
                logger.Debug("Start to get fs restore job message. Job Id:{0}", subJobId);
                var subJob = SubJobDao.GetSubJob(subJobId, true);

                BaseJobDto jobDto = new BaseJobDto()
                {
                    Id = subJob.Id,
                    JobType = subJob.JobType
                };

                RestoreInfo jobMsg = SerializerHelper.DeserializeByDataContractSerializer<RestoreInfo>(subJob.JobContext.Settings);
                return SerializerHelper.SerializeByJsonSerializer(jobMsg);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting fs restore job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get fs restore job message finished. Job Id: " + subJobId);
            }
        }
        public async Task<string> GetFSRetainJobMessageAsync(string subJobId)
        {
            try
            {
                logger.Debug("Start to get fs retain job message. Job Id:{0}", subJobId);
                var subJob = SubJobDao.GetSubJob(subJobId, true);
                return subJob.JobContext.Settings;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting fs retain job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get fs retain job message finished. Job Id: " + subJobId);
            }
        }

        public async Task<string> GetFSDiscoveryJobMessageAsync(string subJobId)
        {
            try
            {
                IRMDiscoveryFSConfigurationService configService = new RMDiscoveryFSConfigurationService();
                return configService.GetFSDiscoveryJobMessageAsync(subJobId);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting FS discovery job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get FS discovery job message finished. Job Id: " + subJobId);
            }
        }

        private Dictionary<string, Guid> GetConnectionCache()
        {
            Dictionary<string, Guid> connectionCache = new Dictionary<string, Guid>();
            try
            {
                var connections = FSConnectionDao.GetAllConnections().OrderByDescending(c => c.UNCPath.Length);

                foreach (var connection in connections)
                {
                    if (!connectionCache.ContainsKey(connection.UNCPath))
                    {
                        connectionCache.Add(connection.UNCPath, connection.Id);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting connection cache. Error:{0}", e.ToString());
            }
            return connectionCache;
        }

        private List<string> FSBuildBreakTreeNode(RMFSTreeNode tree)
        {
            List<string> breakNodeUrls = new List<string>();
            var parentId = ScheduleService.GetProfileId(tree) + "|";
            var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
            foreach (var item in treeNodes)
            {
                var node = JsonConvert.DeserializeObject<RMFSTreeNode>(item);
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    continue;
                }
                string url = EncodeUtil.EncryptBySHA1(node.FullPath.ToLowerInvariant());
                if (!breakNodeUrls.Contains(url))
                {
                    breakNodeUrls.Add(url);
                }
            }
            var pathList = FileSystemSettingDao.AllDisabledRecordManagementPath().GetAwaiter().GetResult();
            foreach (var path in pathList)
            {
                string url = EncodeUtil.EncryptBySHA1(path.ToLowerInvariant());
                if(!breakNodeUrls.Contains(url))
                {
                    breakNodeUrls.Add(url);
                }
            }
            return breakNodeUrls;
        }

        private List<string> FSBuildRunningJobNode(RMFSTreeNode tree, JobType type, string currentJobId)
        {
            List<string> runningJobNodeUrls = new List<string>();
            try
            {
                List<JobType> jobTypes = new List<JobType>();
                if (type == JobType.FSDataSynchronization || type == JobType.FSDataSynchronizationSchedule || type == JobType.ImportFSSetting
                    || type == JobType.ExportFSSetting || type == JobType.DownloadRCCReport)
                {
                    jobTypes.Add(JobType.FSDataSynchronization);
                    jobTypes.Add(JobType.FSDataSynchronizationSchedule);
                }
                else if (type == JobType.FSDisposal || type == JobType.FSDisposalSchedule)
                {
                    jobTypes.Add(JobType.FSDisposal);
                    jobTypes.Add(JobType.FSDisposalSchedule);
                }

                var subFolderJobs = SubJobDao.GetRunningAgentJob(jobTypes)
                            .Where(j => j.String1.StartsWith(tree.FullPath) && !j.Id.Equals(currentJobId)).OrderByDescending(j => j.String1).ToList();
                foreach (var subFolder in subFolderJobs)
                {
                    var context = SubJobDao.GetSubJob(subFolder.Id, true)?.JobContext;
                    if (context != null)
                    {
                        RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(context.Settings).First();
                        string url = EncodeUtil.EncryptBySHA1(node.FullPath.ToLowerInvariant());
                        if (!runningJobNodeUrls.Contains(url))
                        {
                            runningJobNodeUrls.Add(url);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting FSBuildRunningJobNode. Error:{0}", e.ToString());
            }
            return runningJobNodeUrls;
        }
        //private List<RuleNodeContract> BuildBreakTreeNode(RMSPTreeNode tree)
        //{
        //    List<RuleNodeContract> breakInherting = new List<RuleNodeContract>();
        //    try
        //    {
        //        var parentId = ScheduleService.GetProfileId(tree) + "|";

        //        var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
        //        foreach (var item in treeNodes)
        //        {

        //            var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
        //            if (node.Level == (int)NodeLevel.WebApplication)
        //            {
        //                continue;
        //            }
        //            SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
        //            var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
        //            breakInherting.Add(breakNode);

        //        }

        //        var spsettings = SharepointSettingDao.GetDescendantsDisableNodes(tree);
        //        foreach (var item in spsettings)
        //        {
        //            var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(item.NodeInfo);
        //            if (node.Level == (int)NodeLevel.WebApplication)
        //            {
        //                continue;
        //            }
        //            SPTreeNodeDto spTree = RMDtoConverter.ConvertRMTree2SPTree(node);
        //            var breakNode = ConvertTreeNodeToRuleNodeConfig(spTree, RuleNodeType.Archiver);
        //            breakInherting.Add(breakNode);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while build break tree node,ERROR:{0}", ex.ToString());
        //    }
        //    return breakInherting;
        //}

        public bool ResetApplyExistingOption(Guid scopeId)
        {
            try
            {
                logger.Debug("Start to reset existing setting. Scope Id:" + scopeId);
                return FileSystemSettingDao.ResetApplyExistingOption(scopeId);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while resetting existing setting. Scope Id:{0} Error:{1}", scopeId, e.ToString());
                return false;
            }
            finally
            {
                logger.Debug("Reset existing setting finished. Scope Id:" + scopeId);
            }
        }

        public bool ResetApplyExistingOptionForRealTimeJob(string jobId)
        {
            try
            {
                logger.Debug("Start to reset existing setting. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(subJob.JobContext.Settings).FirstOrDefault();
                var top3Nodes = FindTop3LevelNodes(ConvertRMTree2FSTree(node));
                var result = GetJobType(node);
                if (result.Item4)
                {
                    if (FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature())
                    {
                        FileSystemSettingDao.ResetApplyClassCodeExistingOption(new Guid(result.Item6));
                        return true;
                    }
                    FileSystemSettingDao.ResetApplyExistingOption(new Guid(result.Item6));
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while resetting existing setting for realtime job. JobId:{0} Error:{1}", jobId, e.ToString());
                return false;
            }
            finally
            {
                logger.Debug("Reset existing setting. finished. Job Id: " + jobId);
            }
        }

        public async Task<bool> ResetApplyExistingOptionForScheduleJobAsync(string jobId)
        {
            try
            {
                logger.Debug("Start to reset existing setting for schedule job. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                RMFSTreeNode fsnode = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(subJob.JobContext.Settings).FirstOrDefault();
                var allSetting = FileSystemSettingDao.LoadAllSetting().Where(s => s.IsActive && s.ConnectionGroupId.Equals(fsnode.ConnGroupId));
                if (allSetting.IsNullOrEmpty())
                {
                    logger.Warn("There is no fs setting enable sync data into Explorer.");
                    return false;
                }
                logger.Debug("Get file system setting finished. Count:" + allSetting.Count());
                List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
                foreach (var setting in allSetting)
                {
                    RMFSTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(setting.NodeInfo);
                    if (selectedNode.Level == (int)NodeLevel.WebApplication)
                    {
                        availableNode.AddRange(await this.AssembleSyncDataRunnableNodeAsync(selectedNode));
                    }
                    else
                    {
                        availableNode.Add(selectedNode);
                        logger.Debug("Add sub job node successfully. NodeName:" + selectedNode.Name);
                    }
                }
                foreach (var node in availableNode)
                {
                    try
                    {
                        var top3Nodes = FindTop3LevelNodes(ConvertRMTree2FSTree(node));
                        var result = GetJobType(node);
                        if (result.Item4)
                        {
                            FileSystemSettingDao.ResetApplyExistingOption(new Guid(result.Item6));
                        }
                        logger.Debug("Reset existing setting for node:{0} success.", node.FullPath);
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Reset existing setting for node:{0} failed, error:{1}", node.FullPath, e.ToString());
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Debug("Reset existing setting for schedule job. Job Id:" + jobId);
                return false;
            }
        }

        public async Task<RAReturnMessage> RunDataSyncJobAsync(RMFSTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            if (!await FileSystemSettingDao.IsFullPathConnectionExist(selectedTree))
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_SyncData_NoSC");
                return msg;
            }

            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!(await IsExistCanRunJobNodesAsync(selectedTree)))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_SyncData_NoSC");
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
                    JobType = JobType.FSDataSynchronization,
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
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        public RAReturnMessage RunImportFSSettingJob(JobRunBy jobRunBy, string extension, string strBytes)
        {
            logger.Debug("start Import FS Setting");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportFSSetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0} {1}", extension, strBytes),
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
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

        public RAReturnMessage RunExportFSSettingJob(JobRunBy jobRunBy)
        {
            logger.Debug("start Export FS Setting");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportFSSetting,
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
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        public async Task<RAReturnMessage> RunApplyClassCodeJobAsync(ApplyClassCodeSettingDto settingDto, JobRunBy jobRunBy)
        {
            logger.Debug("start fs apply class code");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            foreach (var fsTreeNode in settingDto.FSTreeNode)
            {
                var validationResult = await ValidateClassCodePolicyAsync(new ClassCodePolicyInfo
                {
                    ClassCode = settingDto.ClassCode,
                    CountryCode = settingDto.CountryCode,
                    RetentionScheduleType = (RetentionScheduleType)settingDto.RetentionType,
                    TermUniqueId = settingDto.TermId,
                    TermSetId = fsTreeNode.TermSetId.ToString(),
                });
                if (!validationResult.Item1)
                {
                    logger.Error("Save Class Code Policy Error, validation failed. ClassCode: {0}, CountryCode: {1}, RetentionScheduleType: {2}. Error: {3}", settingDto.ClassCode, settingDto.CountryCode, settingDto.RetentionType, validationResult.Item2);
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = validationResult.Item2;
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
                    JobType = JobType.ApplyClassCode,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = settingDto == null ? null : SerializerHelper.SerializeByDataContractSerializer(settingDto)
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while Apply class code,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public async Task<RAReturnMessage> RunDisposalJobAsync(RMFSTreeNode selectedTree, JobRunBy jobRunBy)
        {
            logger.Debug("start fs disposal sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();


            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!(await IsExistCanRunJobNodesForDisposalAsync(selectedTree)))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_Disposal_NoSC");
                    return msg;
                }
            }

            if (TermRuleInfos.GetTermWithRule().Count == 0)
            {
                logger.Error(I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules"));
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules");
                return msg;
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.FSDisposal,
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
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        #region export FS setting
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ExportFSSetting, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.ExportSetting, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RealRunExportFSSettingJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            JobType jobType = JobType.ExportFSSetting;
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            string jobId = RMJobService.CreateJob(jobType, jobRunByUser, account?.UserId);
            List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.ExportFSSetting);
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
                    DownloadType = DownloadContentType.ExportSettings,
                });
                logger.Info("Start to export file system setting");
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.ExportFSSetting,
                    RunBy = jobRunBy,
                    CommandLine = string.Format("{0} {1}", JobType.ExportFSSetting, jobId),
                });
                return jobId;
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_BCM_ExportFSSetting_SkipJob");
                return "";
            }
        }

        #endregion
        public RAReturnMessage CheckNodeInfo(RMFSTreeNode node)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            if (IsDeactivedNode(node))
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = "The selected node is not active.";
            }
            return msg;
        }

        public bool IsDeactivedNode(RMFSTreeNode node)
        {
            return FileSystemSettingDao.IsDeactivedNode(ScheduleService.GetProfileId(node));
        }
        #endregion

        #region private method
        private async Task<bool> IsExistCanRunJobNodesAsync(RMFSTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                if (await IsHaveAvailableNodesAsync(selectedTree))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> IsExistCanRunJobNodesForDisposalAsync(RMFSTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                if (await IsHaveAvailableNodesForDisposalAsync(selectedTree))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> IsHaveAvailableNodesAsync(RMFSTreeNode selectedTree)
        {
            List<RMFSTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeAsync(selectedTree);
            if (lstAvailableNodes == null || lstAvailableNodes.Count() <= 0)
            {
                return false;
            }
            return true;
        }

        private async Task<bool> IsHaveAvailableNodesForDisposalAsync(RMFSTreeNode selectedTree)
        {
            List<RMFSTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeForDisposalAsync(selectedTree);
            if (lstAvailableNodes == null || lstAvailableNodes.Count() <= 0)
            {
                return false;
            }
            return true;
        }

        private async Task<List<RMFSTreeNode>> AssembleSyncDataRunnableNodeAsync(RMFSTreeNode selectedNode)
        {
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMFSTreeNode> nodes = await FSBrowerTreeService.FSBrowseAsync(selectedNode);
                if (nodes.IsNullOrEmpty())
                {
                    return availableNode;
                }
                foreach (RMFSTreeNode node in nodes)
                {
                    var tempCustomSetting = FileSystemSettingDao.LoadFSSetting(node.Id, node.ConnGroupId);
                    if (tempCustomSetting == null)
                    {

                        await this.LoadFSNodeSettingAsync(node);
                        availableNode.Add(node);
                        logger.Debug("Add sub job node successfully. NodeName:" + node.Name);
                    }
                    else
                    {
                        //has unique setting, will skip this connection
                        logger.Info($"Current connection: {node.Name} has unique setting, will not be included in this job.");
                    }
                    //this.LoadFSNodeSetting(node);
                    //if (node.IsActive)//RECO-3282  RECO-3268
                    ////if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    //{
                    //    availableNode.Add(node);
                    //}
                }
            }
            else
            {
                availableNode.Add(selectedNode);
            }
            return availableNode;
        }

        private async Task<List<RMFSTreeNode>> AssembleSyncDataRunnableNodeForDisposalAsync(RMFSTreeNode selectedTree)
        {
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            if (selectedTree.Level == (int)NodeLevel.WebApplication)
            {
                List<RMFSTreeNode> nodes = await FSBrowerTreeService.FSBrowseAsync(selectedTree);
                if (nodes.IsNullOrEmpty())
                {
                    return availableNode;
                }
                foreach (RMFSTreeNode tempConnection in nodes)
                {
                    string profileId = selectedTree.Id.ToString() + "|" + tempConnection.Id;
                    var schedule = await ScheduleService.GetScheduleByProfileIdAsync(profileId);
                    if (schedule == null)
                    {
                        await this.LoadFSNodeSettingAsync(tempConnection);
                        availableNode.Add(tempConnection);
                    }
                    else
                    {
                        //has unique setting, will skip this connection
                        logger.Info($"Current connection: {tempConnection.Name} has unique schedule, will not be included in this job.");
                    }
                }
            }
            else
            {
                availableNode.Add(selectedTree);
            }
            return availableNode;
        }

        private async Task<string> RunFSDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            IEnumerable<RMFileSystemSetting> allSetting;
            if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
            {
                var fSConnectionGroupIdsBelongCurrentDC = MultiGeoDataCenterService.IsMainDC() ? FSConnectionGroupDao.LoadAllConnectionGroupIdOfMainDC() : FSConnectionGroupDao.LoadAllConnectionGroupIdByDCInternalName(RMSSOHelper.CurrentDCName);
                allSetting = FileSystemSettingDao.LoadAllSettingByGroupIds(fSConnectionGroupIdsBelongCurrentDC);
            }
            else
            {
                allSetting = FileSystemSettingDao.LoadAllSetting().Where(s => s.IsActive);
            }

            try
            {
                if (allSetting.IsNullOrEmpty())
                {
                    logger.Warn("There is no fs setting enable sync data into Explorer.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_FS_NoIsSyncSCUnderGroup");
                    return jobId;
                }
                logger.Debug("Get file system setting finished. Count:" + allSetting.Count());
                foreach (var setting in allSetting)
                {
                    var group = FSConnectionGroupDao.GetGroupById(setting.ConnectionGroupId);
                    if (group == null)
                    {
                        logger.Debug("Group has been deleted, will not run job on node. scopeId: {0}", setting?.ScopeId);
                        continue;
                    }
                    RMFSTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(setting.NodeInfo);
                    if (selectedNode.Level == (int)NodeLevel.WebApplication)
                    {
                        if (HasRunningJobOnNode(selectedNode.ConnGroupId.ToString(), selectedNode.ConnGroupId.ToString(), jobId, selectedNode.Level, jobType))
                        {
                            logger.Debug("There is already a data sync job running on this node, schedule job will not include this node. NodeId:{0}", selectedNode?.Id);
                            continue;
                        }
                        availableNode.AddRange(await GetIncludeNodeListAsync(selectedNode, jobId));
                    }
                    else
                    {
                        var nodeDto = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(selectedNode) };
                        var top3Nodes = FindTop3LevelNodes(nodeDto[0]);
                        var groupId = top3Nodes.Item2.Id.ToString();
                        var scopeId = top3Nodes.Item3.Id.ToString();
                        if (HasRunningJobOnNode(scopeId, groupId, jobId, selectedNode.Level, jobType, selectedNode))
                        {
                            logger.Debug("There is already a data sync job running on this node, schedule job will not include this node. NodeId:{0}", selectedNode?.Id);
                            continue;
                        }

                        if (IsConnectionDeleted(new Guid(scopeId)))
                        {
                            logger.Debug($"Current connection has been deleted. Id:{scopeId}");
                            continue;
                        }
                        availableNode.Add(selectedNode);
                        logger.Debug("Add sub job node successfully. NodeName:" + selectedNode.Name);
                    }
                }

                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoConnectionUnderGroup");
                    return jobId;
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, ex.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }

            logger.Debug("availableNode count:" + availableNode.Count);
            await StartJobByGroupsAsync(availableNode, subJobCountInConfigFile, jobId, jobRunByUser, jobType);
            return jobId;
        }


        private async System.Threading.Tasks.Task StartJobByGroupsAsync(List<RMFSTreeNode> availableNode, int subJobCountInConfigFile, string jobId, string jobRunByUser, JobType jobType)
        {
            var groupNodes = availableNode.GroupBy(n => n.ConnGroupId).ToDictionary(k => k.Key, v => v.ToList());
            bool isFirstJob = true;
            foreach (var group in groupNodes)
            {
                if (!isFirstJob)
                {
                    jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                }
                var enabledJPMCFSFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                //var enabledFSHighPerformance = FSHighPerformanceUtility.IsFSHighPerformanceModeEnabled();
                logger.Info($"This tenant enable JPMC FS feature: {enabledJPMCFSFeature}");
                if (enabledJPMCFSFeature)
                {
                    await DispatchWithPerAgentCapacityAsync(jobId, jobType, availableNode, AvePoint.Hybrid.Contract.JobType.FSDataSync, enabledJPMCFSFeature);
                    return;
                }
                isFirstJob = false;
                var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(new List<Guid> { group.Key });
                
                if (parallelSubJobCount == 0)
                {
                    logger.Error("No available agent server. Set main job failed.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                    continue;
                }
                int subJobCount = group.Value.Count;
                int currentSubjobIndex = 0;
                List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
                List<Hybrid.Contract.RecordsJobArgs> realtimeJobs = new List<Hybrid.Contract.RecordsJobArgs>();
                foreach (RMFSTreeNode site in group.Value)
                {
                    tempList.Add(site);
                    //if (tempList.Count == RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB)
                    {
                        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, site.FullPath);
                        if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            realtimeJobs.Add(new Hybrid.Contract.RecordsJobArgs()
                            {
                                JobId = subJobId,
                                JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                                TenantId = TenantLocalValue.LogonGroupId,
                                Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                            });
                            //HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                            //{
                            //    JobId = subJobId,
                            //    JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                            //    TenantId = TenantLocalValue.LogonGroupId
                            //}, site.ConnGroupId);
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }

                if (realtimeJobs.Count > 0)
                {
                    StartJobs(realtimeJobs, group.Key);
                }
            }
        }

        private void StartJobs(List<Hybrid.Contract.RecordsJobArgs> realtimeJobs, Guid connGroupId)
        {
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => RealStartJobsAsync(realtimeJobs, connGroupId));

        }

        private async System.Threading.Tasks.Task RealStartJobsAsync(List<Hybrid.Contract.RecordsJobArgs> realtimeJobs, Guid connGroupId)
        {
            foreach (var args in realtimeJobs)
            {
                await HybridFileSystemWorkerService.StartJobWithConnectionGroupIdDirectlyAsync(args, connGroupId);
            }
        }
        private async System.Threading.Tasks.Task AssembleCacheDataAsync(Guid groupId, AvePoint.RA.Contract.Global.Object.FSJobMessage message)
        {
            message.AllTerms = TaxonomyService.GetAllTermsForce();
            var fsRules = AgentRuleUtil.FilterRuleWithDataSource(RuleManagerService.GetRulesFromRecords(), Contract.Explorer.SourceFlag.FileSystem);

            var globalRules = fsRules.ConvertAll(r => RMDtoConverter.ConvertRule2GlobalDto(r));
            message.AllRecordsRule = SerializerHelper.SerializeByDataContractSerializer(globalRules);
            message.TermRuleMapping = await GetTermRuleMappingAsync();
            message.RMScopeSettings = GetFSSettings(FileSystemSettingDao.LoadAllSettingsUnderGroup(groupId));
        }

        public async System.Threading.Tasks.Task AssembleCacheDataForDisposalAsync(Guid groupId, AvePoint.RA.Contract.Global.Object.FSJobMessage message)
        {
            message.AllTerms = TaxonomyService.GetAllTermsForce();
            //var fsRules = RuleManagerService.GetRulesFromDA().Where(r => r.FSRule != null).ToList();
            var fsRules = AgentRuleUtil.FilterRuleWithDataSource(RuleManagerService.GetRulesFromRecords(), Contract.Explorer.SourceFlag.FileSystem);
            if (fsRules.Count == 0)
            {
                throw new Exception("No available rules");
            }
            if (message.AllTerms.Count == 0)
            {
                throw new Exception("No available terms");
            }
            var globalRules = fsRules.ConvertAll(r => RMDtoConverter.ConvertRule2GlobalDto(r));
            message.AllRecordsRule = SerializerHelper.SerializeByDataContractSerializer(globalRules);
            message.TermRuleMapping = await GetTermRuleMappingAsync();
            if (message.TermRuleMapping.Count == 0)
            {
                throw new Exception("No available term rules");
            }
            message.RMScopeSettings = GetFSSettings(FileSystemSettingDao.LoadAllSettingsUnderGroup(groupId));//Group or connection.??
                                                                                                             // message.UniqueIdPrefix = GetUniqueIdPrefix();

        }
        private RMFileSystemSetting QueryScopeTermIdSetting(AvePoint.RA.Contract.Global.Object.FSTreeNodeDto node, Guid groupId)
        {
            Guid scopeId = node.Level == (int)NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : node.Id;
            //Guid id = node.FullPath.ToLowerInvariant().ToMd5();
            var setting = FileSystemSettingDao.LoadFSSetting(scopeId, groupId);
            if (setting != null)
            {
                logger.Debug("Get fs setting. ScopeId:{0}", scopeId);
                return setting;
            }
            else if (node.Parent != null)
            {
                return QueryScopeTermIdSetting(node.Parent, groupId);
            }
            else
            {
                return null;
            }
        }

        private Tuple<FSJobType, TermConflictOption, DateTime, bool, string, string, List<Guid>> GetJobType(RMFSTreeNode node)
        {
            FSJobType jobType = FSJobType.UserFullJob;
            TermConflictOption mTermConflictOption;
            DateTime mIBStartTime = DateTime.MinValue;
            bool mNeedToChangeScopeProfile = false;
            List<Guid> changedTermIds = new List<Guid>();
            string recordOwner = string.Empty;
            var nodeDto = ConvertRMTree2FSTree(node);
            System.Tuple<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> top3Nodes = FindTop3LevelNodes(nodeDto);
            string fullPath = node.FullPath;
            string connectionPath = top3Nodes.Item3.FullPath;
            string groupId = top3Nodes.Item2.Id.ToString();
            string highName = fullPath.Substring(connectionPath.Length).Trim('\\');
            RMFileSystemSetting setting = QueryScopeTermIdSetting(nodeDto, new Guid(groupId));
            string settingScopeId = setting.ScopeId.ToString();
            var owners = RecordOwnerDao.GetRecordOwner(setting.Id, RecordOwnerSettingType.FileSystem);
            if (owners?.Count > 0)
            {
                recordOwner = string.Join("|", owners.Select(o => o.Id.ToString()).ToList());
            }
            //TODO  Alphaleonis.Win32.Filesystem.Path ?=> IO.Path            
            Guid scopeId = fullPath.Replace("/", "\\").ToLowerInvariant().ToMd5();
            //Alphaleonis.Win32.Filesystem.Path.Combine(connectionPath, highName).ToLowerInvariant().ToMd5();
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                mTermConflictOption = setting.AutoJobOption == (int)AutoJobOption.Override ? TermConflictOption.Overwrite : TermConflictOption.Skip;
                if (setting.RunAutoFullJob)
                {
                    logger.Info("The job is user started full job.");
                    jobType = FSJobType.UserFullJob;
                    mNeedToChangeScopeProfile = true;
                    mIBStartTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                }
                else
                {
                    jobType = FSJobType.IncrementalJob;
                    var result = InitINCStartTime(scopeId, setting);
                    jobType = result.Item2;
                    mIBStartTime = result.Item1;
                    changedTermIds = result.Item3;
                }
            }
            else
            {
                int classificationLevel = this.GetClassificationLevel();
                mTermConflictOption = TermConflictOption.Skip;
                bool isCheckApplyExistClassCodeOption = FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature() && setting.ApplyExistDocument;
                if (setting.NeedCheckDefaultValue || isCheckApplyExistClassCodeOption)
                {
                    logger.Info("The job is user started full job.");
                    if (setting.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                    {
                        //Override 相当于重新跑full
                        jobType = FSJobType.UserFullJob;
                        mTermConflictOption = TermConflictOption.Overwrite;
                        mIBStartTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                    }
                    else if (classificationLevel == (int)NodeLevel.FSFolder)
                    {
                        //Folder classification, Skip,  sub folder自动继承Parent的Term， 不需要跑Full.
                        jobType = FSJobType.IncrementalJob;
                        var result = InitINCStartTime(scopeId, setting);
                        //jobType = result.Item2;
                        mIBStartTime = result.Item1;
                    }
                    mNeedToChangeScopeProfile = true;
                }
                else
                {
                    jobType = FSJobType.IncrementalJob;
                    var result = InitINCStartTime(scopeId, setting);
                    jobType = result.Item2;
                    mIBStartTime = result.Item1;
                    changedTermIds = result.Item3;
                }
            }
            return new Tuple<FSJobType, TermConflictOption, DateTime, bool, string, string, List<Guid>>(jobType, mTermConflictOption, mIBStartTime, mNeedToChangeScopeProfile, recordOwner, settingScopeId, changedTermIds);
        }

        private Tuple<DateTime, FSJobType, List<Guid>> InitINCStartTime(Guid scopeId, RMFileSystemSetting setting)
        {
            DateTime startTime = QueryIncJobStartTime(scopeId);
            FSJobType JobType;
            List<Guid> changedTermIds = new List<Guid>();
            if (startTime != System.Data.SqlTypes.SqlDateTime.MinValue.Value)
            {
                List<RMTerm> possibleTermsInTheJob = QueryJobTerms(setting);
                List<Guid> possibleTermIds = possibleTermsInTheJob.Select(t => t.UniqueId).ToList();
                logger.Info("There are {0} positive terms in the job.", possibleTermsInTheJob.Count);
                List<Guid> changedTerms = QueryChangedTerms(startTime);
                if (changedTerms.Any(t => possibleTermsInTheJob.Any(p => p.UniqueId == t)))
                {
                    changedTermIds = possibleTermIds;
                    logger.Info("The criterias of the rule or the term-rule association was changed since last sync job.So this job will also match the rule for the scanned files again.");
                    JobType = FSJobType.RematchRuleFullJob;
                }
                else
                {
                    logger.Info("There is a record for this scope,and there is no term/rule changed. The job will be incremental job.");
                    JobType = FSJobType.IncrementalJob;
                }
            }
            else
            {
                logger.Info("There is no record for this scope[{0}]. The job will be full job.", scopeId);
                JobType = FSJobType.UserFullJob;
            }
            return new Tuple<DateTime, FSJobType, List<Guid>>(startTime, JobType, changedTermIds);
        }

        private List<Guid> QueryChangedTerms(DateTime startTime)
        {
            IRMChangeClassificationDao changeDao = new RMChangeClassificationDao();
            var changes = changeDao.GetAllChange(startTime.Ticks, 0);
            logger.Info("There were {0} terms changed since last job{1}.", changes.Count, startTime);
            return changes;
        }

        private List<RMTerm> QueryJobTerms(RMFileSystemSetting setting)
        {
            List<RMTerm> terms = new List<RMTerm>();
            if (setting.TermId == Guid.Empty)
            {
                //get all terms under the termset

                var termSet = TermSetDao.GetRMTermSetByGuid(setting.TermSetId);
                if (termSet != null)
                {
                    //terms = dao.GetTermFromTermSet(termSet.Id);
                    terms = TermDao.FSGetAllTermsUnderTermSet(termSet.Id);
                }
            }
            else
            {
                //get all terms under the term             
                var term = TermDao.GetRMTermByGuId(setting.TermId);
                if (term != null)
                {
                    terms = TermDao.GetAllSubLocationTerm(term.Id);
                }
            }
            return terms;
        }

        private DateTime QueryIncJobStartTime(Guid mScopeId)
        {
            var time = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            try
            {
                RMFileSystemJobTimeReference jobEntry = FileSystemJobTimeReferenceDao.GetJobEntry(mScopeId);
                if (jobEntry != null)
                {
                    time = new DateTime(jobEntry.LastJobTimeTicks);
                }
                else
                {
                    logger.Info("There is no job entry in the database. So this job will process full scan.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get the job start time from Explorer database. Exception:{0}", ex.ToString());
            }
            logger.Info("Incremental start time from Job Reference table:{0}", time);
            return time;
        }

        //private static Guid QueryScopeTermIdSetting(FSTreeNodeDto node)
        //{
        //    Guid id = node.FullPath.ToLowerInvariant().ToMd5();
        //    if (FSDataCollectJobCache.Instance.ScopeSettingCache.ContainsKey(id))
        //    {
        //        return id;
        //    }
        //    else if (node.Parent != null)
        //    {
        //        return QueryScopeTermIdSetting(node.Parent);
        //    }
        //    else
        //    {
        //        return Guid.Empty;
        //    }
        //}

        private async Task<Dictionary<Guid, List<Guid>>> GetTermRuleMappingAsync()
        {
            Dictionary<Guid, List<Guid>> mapping = new Dictionary<Guid, List<Guid>>();

            Dictionary<int, Guid> termIdUniqueIdMapping = TaxonomyService.GetAllTermsForce().ToDictionary(t => t.Id, t => t.UniqueId);

            Dictionary<int, List<Guid>> termRuleMapping = TaxonomyService.GetTermRuleMapping();


            ITermSetMembershipDao membershipDao = new TermSetMembershipDao();
            Dictionary<int, List<int>> memberships = (await membershipDao.FindListWithColumnsAsync(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved))
                .GroupBy(t => t.ParentTermId, v => v.TermId)
                .ToDictionary(t => t.Key, v => v.ToList());

            memberships.Keys.OrderBy(k => k).ForEach(pId =>
            {
                if (termRuleMapping.ContainsKey(pId))
                {
                    memberships[pId].ForEach(cId =>
                    {
                        if (!termRuleMapping.ContainsKey(cId))
                        {
                            termRuleMapping[cId] = termRuleMapping[pId];
                        }
                    });
                }
            });
            termRuleMapping.Keys.ForEach(termId =>
            {
                if (termIdUniqueIdMapping.ContainsKey(termId))
                {
                    Guid termGuid = termIdUniqueIdMapping[termId];
                    mapping[termGuid] = termRuleMapping[termId];
                }
            });
            return mapping;
        }

        private List<FSSettingDto> GetFSSettings(List<RMFileSystemSetting> settings)
        {
            List<FSSettingDto> dtos = new List<FSSettingDto>();
            settings.ForEach(s =>
            {
                dtos.Add(ConvertFSSetting2FSSettingDto(s));
            });
            return dtos;
        }

        private bool IsConnectionDeleted(Guid connectionId)
        {
            var conn = FSConnectionDao.GetConnectionById(connectionId);
            return conn == null;
        }

        private async Task<string> RunDataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMFSTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            //Group , Load COnnectionInfos , RMSPTreeeNode? ? connection
            //??JobContext 
            //SIte coolection //Query from jOb 
            try
            {
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    var setting = this.FileSystemSettingDao.GetSettingByConnGroupId(selectedNode.ConnGroupId);
                    if (setting != null)
                    {
                        var tempNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(setting.NodeInfo);
                        logger.Info($"real run fs sync job for WebApplication,FE class code:{selectedNode.ClassCode?.ClassCodeId},the db class code:{tempNodeInfo?.ClassCode?.ClassCodeId}");
                        if (tempNodeInfo != null)
                        {
                            selectedNode.ClassCode = tempNodeInfo.ClassCode;
                        }
                    }
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.ConnGroupId.ToString());
                    if (HasRunningJobOnNode(selectedNode.ConnGroupId.ToString(), selectedNode.ConnGroupId.ToString(), jobId, selectedNode.Level, jobType))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                        return jobId;
                    }
                    availableNode.AddRange(await GetIncludeNodeListAsync(selectedNode, jobId));
                }
                else
                {
                    var setting = this.FileSystemSettingDao.GetSettingByConnGroupId(selectedNode.Id);
                    if (setting != null)
                    {
                        var tempNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(setting?.NodeInfo);
                        logger.Info($"real run fs sync job for node,FE class code:{selectedNode.ClassCode?.ClassCodeId},the db class code:{tempNodeInfo?.ClassCode?.ClassCodeId}");
                        if (tempNodeInfo != null)
                        {
                            selectedNode.ClassCode = tempNodeInfo.ClassCode;
                        }
                    }
                    var nodeDto = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(selectedNode) };
                    var top3Nodes = FindTop3LevelNodes(nodeDto[0]);
                    var groupId = top3Nodes.Item2.Id.ToString();
                    var scopeId = top3Nodes.Item3.Id.ToString();
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, scopeId.ToString());
                    if (HasRunningJobOnNode(scopeId, groupId, jobId, selectedNode.Level, jobType, selectedNode))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                        return jobId;
                    }
                    availableNode.Add(selectedNode);
                }

                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available connection to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "        ");
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                if (!string.IsNullOrEmpty(jobId)) RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            var enabledJPMCFSFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            if(enabledJPMCFSFeature)
            {
                await DispatchWithPerAgentCapacityAsync(jobId, jobType, availableNode, AvePoint.Hybrid.Contract.JobType.FSDataSync, enabledJPMCFSFeature);
                return jobId;
            }
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
            var groupIds = availableNode.Select(item => item.ConnGroupId).ToHashSet();
            //var parallelSubJobCount = subJobCountInConfigFile * HybridFileSystemWorkerService.GetAgentCount();
            var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(groupIds);
            if (parallelSubJobCount == 0)
            {
                logger.Error("No available agent server. Set main job failed.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }
            //var enabledFSHighPerformance = FSHighPerformanceUtility.IsFSHighPerformanceModeEnabled();
            logger.Info($"This tenant enable JPMC FS feature: {enabledJPMCFSFeature}");
            foreach (RMFSTreeNode site in availableNode)
            {
                tempList.Add(site);
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, site.FullPath);
                if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    //HybridFileSystemWorkerService.StartJob(new Hybrid.Contract.RecordsJobArgs()
                    //{
                    //    JobId = subJobId,
                    //    JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                    //    TenantId = TenantLocalValue.LogonGroupId
                    //});
                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                        TenantId = TenantLocalValue.LogonGroupId,
                        Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                    }, site.ConnGroupId);
                }
                tempList.Clear();
                currentSubjobIndex++;
            }

            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSDisposalJob, BeforeHandler = typeof(FileSystemServiceBeforeAuditHandler), AfterHandler = typeof(FileSystemServiceAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSDisposalJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public async Task<string> RunDisposalJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMFSTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            var mOtherJobs = RMJobService.GetRunningJobs(JobTypeConstants.FSDisposalConflictType);
            try
            {
                if (selectedNode.Level == (int)NodeLevel.WebApplication)
                {
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.ConnGroupId.ToString(), null, selectedNode.Name);
                    if (mOtherJobs != null && mOtherJobs.Count > 0)
                    {
                        if (OnlyHasRemoveAction(selectedNode.DefaultTermId))
                        {
                            logger.Info("this fs disposal job only has remove action,no need to skip");
                        }
                        else
                        {
                            logger.Info("this fs disposal job has conflict");
                            JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            return jobId;
                        }
                    }

                    if (HasRunningJobOnNode(selectedNode.ConnGroupId.ToString(), selectedNode.ConnGroupId.ToString(), jobId, selectedNode.Level, jobType))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }
                    availableNode.AddRange(await GetIncludeNodeListForDisposalAsync(selectedNode, jobId));
                }
                else
                {
                    var nodeDto = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>() { ConvertRMTree2FSTree(selectedNode) };
                    var top3Nodes = FindTop3LevelNodes(nodeDto[0]);
                    var groupId = top3Nodes.Item2.Id.ToString();
                    var scopeId = top3Nodes.Item3.Id.ToString();
                    var lastIndex = selectedNode.FullPath.LastIndexOf(top3Nodes.Item3.FullPath);
                    var splitPath = lastIndex + top3Nodes.Item3.FullPath.Length;
                    var fullPath = top3Nodes.Item2.Name + "\\" + top3Nodes.Item3.Name + selectedNode.FullPath.Substring(splitPath);  //sonar 
                    jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, scopeId.ToString(), null, fullPath);

                    if (mOtherJobs != null && mOtherJobs.Count > 0)
                    {
                        if (OnlyHasRemoveAction(selectedNode.DefaultTermId))
                        {
                            logger.Info("this fs disposal job only has remove action,no need to skip1");
                        }
                        else
                        {
                            logger.Info("this fs disposal job has conflicts");
                            JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                            return jobId;
                        }
                    }
                    if (HasRunningJobOnNode(scopeId, groupId, jobId, selectedNode.Level, jobType, selectedNode))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                        return jobId;
                    }

                    if (IsDeactivedNode(selectedNode))
                    {
                        logger.Warn("This node is deactived, job will be skipped. JobId:{0} NodeId:{1}", jobId, selectedNode?.Id);
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_FS_DisposalDeactiveFolder_JobFailed");
                        return jobId;
                    }
                    if (IsConnectionDeleted(new Guid(scopeId)))
                    {
                        logger.Debug($"Current connection has been deleted, will not start disposal job Id:{scopeId}");
                    }
                    else
                    {
                        var isEnabledFileSystemJPMC = await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableJPMCFileSystemFeature, false);
                        var isEnabledRecordManagement = isEnabledFileSystemJPMC ? await LoadFSNodeEnableRecordManagement(selectedNode.Id) : true;

                        var conn = FSConnectionDao.GetConnectionById(new Guid(scopeId));
                        selectedNode.IsPause = conn.IsPause;

                        if (isEnabledRecordManagement)
                        {
                            availableNode.Add(selectedNode);
                        }
                    }
                }

                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available connection to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JM_FS_Disposal_NoSC");
                    return jobId;
                }

                var pauseNodes = availableNode.Where(item => item.IsPause == 1).ToList();
                if (pauseNodes != null && pauseNodes.Count == availableNode.Count)
                {
                    logger.Warn("All nodes is pause. JobId:{0}", jobId);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_FS_Disposal_NoSC_Pause");
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                if (!string.IsNullOrEmpty(jobId)) RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            int subJobCount = availableNode.Count;
            var enabledJPMCFSFeature = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
            //var enabledFSHighPerformance = FSHighPerformanceUtility.IsFSHighPerformanceModeEnabled();
            logger.Info($"This tenant enable JPMC FS feature: {enabledJPMCFSFeature}");
            if (enabledJPMCFSFeature)
            {
                await DispatchWithPerAgentCapacityAsync(jobId, jobType, availableNode, AvePoint.Hybrid.Contract.JobType.FSDisposal, enabledJPMCFSFeature);
                return jobId;
            }
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
            var connGroupIds = availableNode.Select(item => item.ConnGroupId).ToHashSet();
            //var parallelSubJobCount = subJobCountInConfigFile * HybridFileSystemWorkerService.GetAgentCount();
            var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(connGroupIds);
           
            if (parallelSubJobCount == 0)
            {
                logger.Error("No available agent server. Set main job failed.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }
            foreach (RMFSTreeNode site in availableNode)
            {
                tempList.Add(site);
                //if (tempList.Count == RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, site.FullPath);
                    if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        //HybridFileSystemWorkerService.StartJob(new Hybrid.Contract.RecordsJobArgs()
                        //{
                        //    JobId = subJobId,
                        //    JobType = AvePoint.Hybrid.Contract.JobType.FSDisposal,
                        //    TenantId = TenantLocalValue.LogonGroupId
                        //});
                        HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = AvePoint.Hybrid.Contract.JobType.FSDisposal,
                            TenantId = TenantLocalValue.LogonGroupId,
                            Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                        }, site.ConnGroupId);
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }

            return jobId;
        }
        private bool OnlyHasRemoveAction(Guid termId)
        {
            var mapping = GetTermRuleMappingAsync().GetAwaiter().GetResult();
            var ruleIds = mapping.ContainsKey(termId) ? mapping[termId] : null;
            if (ruleIds == null || ruleIds.Count == 0)
            {
                logger.Warn("There is no rule for this term, termId:{0}", termId);
                return false;
            }
            else
            {
                var rules = RuleManagerService.GetRulesByIds(ruleIds);
                foreach (var rule in rules)
                {
                    if (rule.FSRule != null)
                    {
                        if ((rule.FSRule.KeepDataOption == (int)KeepDataOption.LinkDocument || rule.FSRule.KeepDataOption == (int)KeepDataOption.Delete) || rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveSetting != null && rule.FSRule.spMoveOption.MoveDestination != null)
                        {
                            logger.Info($"this node has rule is remove only or move to another location,rule id:{rule.Id}");
                        }
                        else
                        {
                            logger.Info($"this node has rule that include backup,rule id:{rule.Id}");
                            return false;
                        }
                    }
                    else
                    {
                        logger.Warn("There is no fs rule for this rule, ruleId:{0}", rule.Id);
                        return false;
                    }
                }
                return true;
            }
        }
        public async Task<string> RunFSRestoreJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RestoreInfo selectedNode)
        {
            string jobId = string.Empty;
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            //List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
            var otherJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex, JobType.FSRetain };
            string scopeId = selectedNode.NodeObjects[0].SitePath;
            jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, scopeId, null, scopeId);
            var mOtherJobs = RMJobService.GetRunningJobs(otherJobTypes);
            if (mOtherJobs.Any())
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            int subJobCount = 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
            ConnectionDto connection = new ConnectionDto();
            try
            {
                connection = await FSRegisterService.GetConnectionByIdAsync(new Guid(scopeId));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get connection by id:{0}, exception:{1}", scopeId, ex.ToString());
            }
            string subJobId = CreateFSRestoreSubJob(jobId, currentSubjobIndex, jobType, subJobCount, selectedNode, scopeId);
            var masterInfo = FSMasterIndexService.GetConnectionMasterInfoByConnectionId(scopeId);
            string agentId = masterInfo.AgentId ?? "";
            HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
            {
                JobId = subJobId,
                JobType = AvePoint.Hybrid.Contract.JobType.FSArchiverRestore,
                TenantId = TenantLocalValue.LogonGroupId,
                AgentId = agentId
            }, connection == null ? new Guid() : connection.GroupId);
            return jobId;
        }
        public async Task<string> RunDisposalJobBySelectdNodeForApprovalAsync(string jobRunByUser, JobType jobType)//RMFSTreeNode selectedNode
        {
            try
            {
                string jobId = string.Empty;
                RMFSTreeNode selectedNode = new RMFSTreeNode();
                List<RMFSTreeNode> availableNode = new List<RMFSTreeNode>();
                var allG = FSConnectionGroupDao.LoadAllGroups();
                foreach (var item in allG)
                {
                    var child = new RMFSTreeNode();
                    child.Id = item.Id;
                    child.Name = item.Name;
                    child.Level = (int)NodeLevel.WebApplication;//NodeLevel.FSGroup
                    child.ConnGroupId = item.Id;
                    child.FullPath = item.Name;
                    child.Parent = new RMFSTreeNode();
                    var allC = FSConnectionDao.GetAllConnectionsByGroupId(child.Id);
                    foreach (var subItem in allC)
                    {
                        var subChild = new RMFSTreeNode();
                        subChild.Id = subItem.Id;
                        subChild.Name = subItem.Name;
                        subChild.Level = (int)NodeLevel.SiteCollection;//RMNodeLevel.FSConnection
                        subChild.AgentId = subItem.AgentId;
                        subChild.FullPath = subItem.UNCPath;
                        subChild.ConnGroupId = child.ConnGroupId;
                        subChild.Parent = child;
                        subChild.ParentId = child.Id.ToString();
                        bool isExsitApproval = explorerDao.Exist(r => r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && r.SourceFlag == (int)SourceFlag.FileSystem && (subItem.Id.ToString().Equals(r.AveSiteId, StringComparison.OrdinalIgnoreCase) ? true : r.ManualFullPath.StartsWith(subItem.UNCPath)));
                        if (isExsitApproval)
                        {
                            child.IsProcessApprovalDatasOnly = true;
                            availableNode.Add(subChild);
                        }
                        else
                        {
                            logger.Info($"there is no approval data for this connection,skip it,name:{subChild.Name}");
                        }
                    }
                }
                //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
                int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);

                if (HasRunningJobOnNode(jobType.ToString(), jobType.ToString(), jobId, (int)NodeLevel.WebApplication, jobType))
                {
                    logger.Info("there is the same group id when run fs disposal,skip it");
                    return jobId;
                }
                availableNode.AddRange(await GetIncludeNodeListForDisposalAsync(selectedNode, jobId));
                if (availableNode.IsNullOrEmpty())
                {
                    logger.Warn("No available connection to run when run fs disposal");
                    return jobId;
                }
                jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, jobType.ToString(), null, JobType.FSDisposal.ToString());
                int subJobCount = availableNode.Count;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);
                int currentSubjobIndex = 0;
                List<RMFSTreeNode> tempList = new List<RMFSTreeNode>();
                var connGroupIds = availableNode.Select(item => item.ConnGroupId).ToHashSet();
                //var parallelSubJobCount = subJobCountInConfigFile * HybridFileSystemWorkerService.GetAgentCount();
                var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(connGroupIds);
                ConcurrencyBudgetUtil concurrencyBudgetUtil = new ConcurrencyBudgetUtil();
                parallelSubJobCount = await concurrencyBudgetUtil.DetermineParallelSubJobCountAsync(TenantLocalValue.LogonGroupId, parallelSubJobCount);
                if (parallelSubJobCount == 0)
                {
                    logger.Error("No available agent server. Set main job failed.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                    return jobId;
                }
                foreach (RMFSTreeNode site in availableNode)
                {
                    tempList.Add(site);
                    {
                        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, site.FullPath);
                        if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                            {
                                JobId = subJobId,
                                JobType = AvePoint.Hybrid.Contract.JobType.FSDisposal,
                                TenantId = TenantLocalValue.LogonGroupId
                            }, site.ConnGroupId);
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                return jobId;
            }
            catch (Exception ex)
            {
                logger.Error("RunDisposalJobBySelectdNodeForApprovalAsync error:{0}", ex.ToString());
                return string.Empty;
            }
        }
        private async Task<List<RMFSTreeNode>> GetIncludeNodeListAsync(RMFSTreeNode selectedTree, string currentJobId)
        {
            List<RMFSTreeNode> nodes = new List<RMFSTreeNode>();
            selectedTree.PageIndex = 0;
            selectedTree.PageSize = 0;
            var tempConnections = await FSBrowerTreeService.FSBrowseAsync(selectedTree);
            if (tempConnections != null && tempConnections.Count > 0)
            {
                foreach (var tempConnection in tempConnections)
                {
                    var tempCustomSetting = FileSystemSettingDao.LoadFSSetting(tempConnection.Id, selectedTree.ConnGroupId);
                    if (tempCustomSetting == null)
                    {
                        if (HasRunningJobOnNode(tempConnection.Id.ToString(), selectedTree.Id.ToString(), currentJobId, (int)NodeLevel.SiteCollection, JobType.FSDataSynchronization, tempConnection))
                        {
                            logger.Info("There is already a file system data sync job running on this connection. Connection id: {0}", tempConnection.Id.ToString());
                            continue;
                        }
                        await this.LoadFSNodeSettingAsync(tempConnection);
                        nodes.Add(tempConnection);
                    }
                    else
                    {
                        //has unique setting, will skip this connection
                        logger.Info($"Current connection: {tempConnection.Name} has unique setting, will not be included in this job.");
                    }
                }
            }
            return nodes;
        }



        /// <summary>
        /// disposal job will check if connection has unique schedule
        /// </summary>
        /// <param name="selectedTree"></param>
        /// <param name="currentJobId"></param>
        /// <param name="jobType"></param>
        /// <returns></returns>
        private async Task<List<RMFSTreeNode>> GetIncludeNodeListForDisposalAsync(RMFSTreeNode selectedTree, string currentJobId)
        {
            List<RMFSTreeNode> nodes = new List<RMFSTreeNode>();
            var tempConnections = await FSBrowerTreeService.FSBrowseAsync(selectedTree);
            var isEnabledFileSystemJPMC = await RMKeyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableJPMCFileSystemFeature, false);
            if (tempConnections != null && tempConnections.Count > 0)
            {
                foreach (var tempConnection in tempConnections)
                {
                    string profileId = selectedTree.Id.ToString() + "|" + tempConnection.Id;
                    var schedule = await ScheduleService.GetScheduleByProfileIdAsync(profileId);
                    if (schedule == null)
                    {
                        if (HasRunningJobOnNode(tempConnection.Id.ToString(), selectedTree.Id.ToString(), currentJobId, (int)NodeLevel.SiteCollection, JobType.FSDisposalSchedule, tempConnection))
                        {
                            logger.Info("There is already a file system dispoal job running on this connection. Connection id: {0}", tempConnection.Id.ToString());
                            continue;
                        }
                        await this.LoadFSNodeSettingAsync(tempConnection);
                        nodes.Add(tempConnection);
                    }
                    else
                    {
                        //has unique setting, will skip this connection
                        logger.Info($"Current connection: {tempConnection.Name} has unique schedule, will not be included in this job.");
                    }
                }
            }

            if (isEnabledFileSystemJPMC && nodes.Any())
            {
                var validatedNodeIds = FileSystemSettingDao.ValidateEnableRecordManagementNodes(nodes.Select(n => n.Id).ToList());
                nodes = nodes.Where(n => validatedNodeIds.Contains(n.Id)).ToList();
            }

            return nodes;
        }


        private async Task<List<RMFSTreeNode>> GetIncludeNodeListForApplyClassCodeAsync(RMFSTreeNode selectedTree, string currentJobId, bool isApplyForAllChild)
        {
            List<RMFSTreeNode> nodes = new List<RMFSTreeNode>();
            selectedTree.PageIndex = 0;
            selectedTree.PageSize = 0;
            var tempConnections = await FSBrowerTreeService.FSBrowseAsync(selectedTree);
            if (tempConnections != null && tempConnections.Count > 0)
            {
                foreach (var tempConnection in tempConnections)
                {
                    string profileId = selectedTree.Id.ToString() + "|" + tempConnection.Id;
                    await this.LoadFSNodeSettingAsync(tempConnection);
                    if (tempConnection.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                    {
                        nodes.Add(tempConnection);
                        if (!isApplyForAllChild)
                        {
                            logger.Warn($"this job run on group level node and apply all option is false");
                            return nodes;
                        }
                    }
                    else
                    {
                        logger.Warn($"this node EnableRecordManagement is:{tempConnection.EnableRecordManagement},will skip apply class code for it,node path:{tempConnection.FullPath}");
                    }
                }
            }
            return nodes;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMFSTreeNode> tempList, bool sendNow, string fullPath, string jobContent = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = fullPath };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList), Content = jobContent };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private string CreateFSRestoreSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RestoreInfo fsRestoreInfo, string fullPath)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = fullPath };
            subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(fsRestoreInfo) };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create fs sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        private FSSettingDto ConvertFSSetting2FSSettingDto(RMFileSystemSetting setting)
        {
            FSSettingDto dto = new FSSettingDto()
            {
                ApplyExistType = setting.ApplyExistType,
                //AutoClassificationRules = setting.AutoClassificationRules,
                AutoJobOption = setting.AutoJobOption,
                //ConnectionGroupId = setting.ConnectionGroupId,
                DefaultTermId = setting.DefaultTermId,
                //DefaultTermName = setting.DefaultTermName,
                DeployTermMethod = setting.DeployTermMethod,
                //DescriptionOfContainer = setting.DescriptionOfContainer,
                //EMailToRecordOwner = setting.EMailToRecordOwner,
                //EnableRelatedRecords = setting.EnableRelatedRecords,
                //FSSettingJobId = setting.FSSettingJobId,
                FullPath = setting.FullPath,
                //HaveConfigSetting = setting.HaveConfigSetting,
                //Id = setting.Id,
                //IdPath = setting.IdPath,
                IsActive = setting.IsActive,
                //IsEnableContainerLevelClassification = setting.IsEnableContainerLevelClassification,
                //IsNewEdited = setting.IsNewEdited,
                //Name = setting.Name,
                NeedCheckDefaultValue = setting.NeedCheckDefaultValue,
                //NodeInfo = setting.NodeInfo,
                RunAutoFullJob = setting.RunAutoFullJob,
                ScopeId = setting.ScopeId,
                //SettingTime = setting.SettingTime,
                TermId = setting.TermId,
                //TermIdOfContainer = setting.TermIdOfContainer,
                //TermName = setting.TermName,
                //TermNameOfContainer = setting.TermNameOfContainer,
                TermSetId = setting.TermSetId,
                //TermSetName = setting.TermSetName
            };
            if (!string.IsNullOrWhiteSpace(setting.AutoClassificationRules))
            {
                var autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                dto.AutoClassificationRules = SerializerHelper.SerializeByDataContractSerializer(autoRules.ConvertAll(r => RMDtoConverter.ConvertClassificationRule2GlobalDto(r)));
            }
            return dto;
        }

        private System.Tuple<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> FindTop3LevelNodes(AvePoint.RA.Contract.Global.Object.FSTreeNodeDto node)
        {
            if (node.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent == null)
            {
                throw new Exception("The level of current node is less then 3.");
            }
            if (node.Parent.Parent.Parent == null)
            {
                return new System.Tuple<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>(node.Parent.Parent, node.Parent, node);
            }
            var tempNode = node;
            while (tempNode.Parent.Parent.Parent != null)
            {
                tempNode = tempNode.Parent;
            }
            return new System.Tuple<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>(tempNode.Parent.Parent, tempNode.Parent, tempNode);
        }

        public RMFSTreeNode FindConnectionLevelNode(RMFSTreeNode node)
        {
            if (node == null || node.Parent == null || node.Parent.Parent == null)
            {
                return null;
            }

            if (node.Parent.Parent.Parent == null)
            {
                return node;
            }

            var tempNode = node;

            while (tempNode.Parent.Parent.Parent != null)
            {
                tempNode = tempNode.Parent;
            }

            return tempNode;
        }

        private AvePoint.RA.Contract.Global.Object.FSTreeNodeDto ConvertRMTree2FSTree(RMFSTreeNode rmTree, AvePoint.RA.Contract.Global.Object.FSTreeNodeDto fs = null, bool needDecryptPath = false)
        {
            if (fs == null)
            {
                fs = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
            }
            fs.Id = rmTree.Id;
            fs.FarmID = rmTree.FarmID;
            fs.Name = rmTree.Name;
            fs.FullPath = needDecryptPath ? EncodeUtil.DecryptByCommunicationKey(rmTree.FullPath) : rmTree.FullPath;
            fs.Level = rmTree.Level;
            fs.NodeType = rmTree.NodeType;
            fs.Expanded = rmTree.Expanded;
            fs.ChildrenCount = rmTree.ChildrenCount;
            fs.CheckNumber = rmTree.CheckNumber;

            fs.Domain = rmTree.Domain;
            fs.Username = rmTree.Username;
            fs.EncryptedPassword = rmTree.EncryptedPassword;
            //fs.IncludeNew = Convert.ToBoolean(rmTree.IncludeNew) ? IncludeNewState.Checked : IncludeNewState.Unchecked;
            //if (fs.NodeExtension == null)
            //{
            //    fs.NodeExtension = new NodeExtensionDto();
            //}
            //sp.NodeExtension.BposInfo = rmTree.BposInfo;
            if (rmTree.Parent != null && fs.Parent == null)
            {
                AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempParent = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                tempParent.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> { fs };
                fs.Parent = ConvertRMTree2FSTree(rmTree.Parent, tempParent, needDecryptPath);
                fs.ParentId = rmTree.Parent.Id.ToString();
            }
            if (rmTree.CheckNumber == 1)
            {
                return fs;
            }
            if (rmTree.Children != null && rmTree.Children.Count > 0 &&
                (fs.Children == null || fs.Children.Count == 0))
            {
                fs.Children = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();
                foreach (RMFSTreeNode child in rmTree.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        AvePoint.RA.Contract.Global.Object.FSTreeNodeDto tempChild = new AvePoint.RA.Contract.Global.Object.FSTreeNodeDto();
                        tempChild.Parent = fs;
                        tempChild.ParentId = fs.Id.ToString();
                        fs.Children.Add(ConvertRMTree2FSTree(child, tempChild, needDecryptPath));
                    }
                    else
                    {
                        logger.Debug("No select node in {0}", child.Name);
                    }
                }
            }
            return fs;
        }

        private static bool HasSelectNodeForFS(RMFSTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                foreach (RMFSTreeNode child in current.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public async Task<bool> HasRunningJobOnSelectedNode(RMFSTreeNode node)
        {
            return HasRunningJobOnNode(node.ConnGroupId.ToString(), node.ConnGroupId.ToString(), string.Empty, node.Level, JobType.ApplyClassCode, node);
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
        // Distributes sub-jobs using per-agent DB limits instead of a tenant-wide burst window.
        private async Task<string> DispatchWithPerAgentCapacityAsync(string jobId, JobType jobType, List<RMFSTreeNode> availableNode, AvePoint.Hybrid.Contract.JobType hybridJobType, bool enabledJPMCFSFeature, string jobContent = null)
        {
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);  
            logger.Info($"Total sub-jobs: {subJobCount}");

            var connGroupIds = availableNode.Select(n => n.ConnGroupId).ToHashSet();

            // 1) Per-agent limit strictly from DB config 
            var config = FSHighPerformanceUtility.LoadFSHighPerformanceConfig();
            int maxJobPerAgent = config.Setting.MaxJobPerAgent;

            // 2) Tenant budget guard is kept intact.
            var concurrencyBudgetUtil = new ConcurrencyBudgetUtil();
            if (!await concurrencyBudgetUtil.CheckRunableAgentJob(TenantLocalValue.LogonGroupId))
            {
                logger.Error("No available agent server. Set main job failed.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }

            // 3) Resolve the concrete available agents for these groups.
            var availableAgentIds = await HybridFileSystemWorkerService.GetAvailableAgentIdsByGroupsAsync(connGroupIds);   
            if (availableAgentIds.Count < 1)
            {
                logger.Error("No available agent server. Set main job failed.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }

            // 4) Build the plan (even spread, capacity-bounded).
            var planner = new AgentCapacityPlanner(SubJobDao, _agentJobTypes);
            var capacities = planner.BuildCapacities(availableAgentIds, maxJobPerAgent);

            // 5) Persist every sub-job first (all Waiting), then reserve+send those that fit.
            var subJobConnectionGroups = new Dictionary<string, Guid>();
            var pendingIds = new List<string>();
            int index = 0;
            foreach (var site in availableNode)
            {
                var tempList = new List<RMFSTreeNode> { site };
                // sendNow=false: creation only. Reservation decides who runs now.
                string subJobId = CreateSubJob(jobId, index++, jobType, subJobCount, tempList, false, site.FullPath, jobContent);
                pendingIds.Add(subJobId);
                subJobConnectionGroups[subJobId] = site.ConnGroupId;
            }

            var plan = planner.Distribute(pendingIds, capacities);

            foreach (var kvp in plan.ImmediateAssignments)
            {
                string subJobId = kvp.Key;
                string agentId = kvp.Value;

                // 6) Atomic per-agent slot reservation (optimistic concurrency).
                if (!SubJobDao.TryReserveAgentSlot(subJobId, agentId, _agentJobTypes, maxJobPerAgent))
                {
                    logger.Info("Slot lost at commit for agent {0}; sub job {1} stays Waiting.", agentId, subJobId);
                    RMSubJobDaoUpdateWaiting(subJobId);
                    continue;
                }
                if (!subJobConnectionGroups.TryGetValue(subJobId, out var connGroupId))
                {
                    logger.Error("Connection group not found for sub job {0}.",subJobId);

                    continue;
                }
                // 7) Send — execution logic itself is untouched.
                HybridFileSystemWorkerService.StartJobWithConnectionGroupId(
                    new Hybrid.Contract.RecordsJobArgs
                    {
                        JobId = subJobId,
                        JobType = hybridJobType,
                        TenantId = TenantLocalValue.LogonGroupId,
                        AgentId = agentId,
                        Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                    }, connGroupId);
            }

            // Deferred sub-jobs are already persisted as Waiting; JobInfoUpdater re-triggers them.
            logger.Info("JobId:{0} dispatched {1} now, {2} deferred.", jobId, plan.ImmediateAssignments.Count, plan.DeferredSubJobIds.Count);
            return jobId;
        }

        private void RMSubJobDaoUpdateWaiting(string subJobId) => SubJobDao.UpdateRunable(subJobId);
        #endregion

    }

    internal static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            CodeContract.NullThrowing(source, "Source");
            CodeContract.NullThrowing(action, "Action");
            foreach (var item in source)
            {
                action(item);
            }
        }
    }

    internal static class StringExtentions
    {
        public static Guid ToMd5(this string source)
        {
            return HashCodeHelper.StringHash(source);
        }
        public static bool Eq(this string left, string right)
        {
            return left.Equals(right, StringComparison.OrdinalIgnoreCase);
        }

    }
}

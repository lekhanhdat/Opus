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
using AvePoint.Common.Api.Services;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.DocAve;
using AvePoint.RA.Common.Email;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Items.Views;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMEmail;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp.Builder;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Extension;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACloudFS.FSActions;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Reclassify;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.RAPhysical.ExplorerMove;
using AvePoint.RA.RAPhysical.Reclassify;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.AzureFileShare.Actions;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.CustomizeConnector.Actions;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using AvePoint.RA.Service.Services.PhysicalObject.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.OneDrive.RMOneDriveExplorer;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.RA.SharePoint.Teams.Reclassifier;
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Cop.Dop;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PnP.Core.Services;
using RABox.Reclassify;
using RAExportCommon;
using RAGlobalSearch.Common;
using RAGlobalSearch.Export;
using RAGoogle.GoogleExplorer;
using RAManualApproval.Converters;
using RAManualApprovalCommon;
using RAManualApprovalCommon.Model;
using RATeams.TeamsExplorer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;
using ARE = AvePoint.RA.Contract.Explorer;
using ColumnType = AvePoint.RA.Contract.TemplateManagement.ColumnType;
using DateFormat = AvePoint.RA.Contract.RMWeb.CP.DateFormat;
using Path = System.IO.Path;
using SOApproveDBStatus = AvePoint.RA.Contract.SOApproveDBStatus;
//using AvePoint.Wrapper.Common;
//using AvePoint.RA.Common.Lock;

namespace AvePoint.RA.Service.Services.Explorer
{
    [Audit]
    public class ExplorerService : RMServiceBase, IExplorerService
    {
        private RALogger logger = RALogger.GetInstance(typeof(IExplorerService));

        //private static MemoryLocker _memoryLocker = new MemoryLocker();
        private readonly ConcurrentDictionary<string, Dictionary<SourceFlag, ManualApprovalRuleModel>> RuleInfos =
    new ConcurrentDictionary<string, Dictionary<SourceFlag, ManualApprovalRuleModel>>();
        private readonly IRuleManagerService RuleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();
        private readonly IFileSystemSettingDao FileSytemSettingDao = PlatformWindsorManager.GetService<IFileSystemSettingDao>();
        private readonly IScheduleService ScheduleService = PlatformWindsorManager.GetService<IScheduleService>();
        private List<RMFileSystemSetting> Settings;
        private List<FSConnection> Connections;
        private readonly ConcurrentDictionary<string, WorkflowDefinitionDto> Workflows = new ConcurrentDictionary<string, WorkflowDefinitionDto>();
        private readonly IManualProcessManagementService ManualProcessManagementService = PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private readonly int PartitionKey = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
        private RMWorkflowProcessor s_workflowProcessor = new();
        private ICosmosBulkOperator CosmosOperator = new CosmosBulkOperator();
        private RMEmailSender s_emailSender;
        private Func<Record, Task> ProcessItemSucceedCallback { get; set; }
        protected readonly string LocalAzConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private Action<Record, string> ProcessItemFailedCallback { get; set; }
        private readonly AvePoint.RA.Service.Services.ManualApproval.Actions.HistoryAddAction AddAction = new AvePoint.RA.Service.Services.ManualApproval.Actions.HistoryAddAction();
        #region Interface

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        public ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        public IRMChangeClassificationDao ChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();

        public IRMClassificationHistoryDao ClassificationHistoryDao => PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public ILabelDao LabelDao => PlatformWindsorManager.GetService<ILabelDao>();
        //public IExplorerDao ExplorerDao { get; set; }

        public IHoldDao HoldDao => PlatformWindsorManager.GetService<IHoldDao>();
        public IHoldMembershipDao HoldMembershipDao => PlatformWindsorManager.GetService<IHoldMembershipDao>();

        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        //TODO add to config file
        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        public IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        private IPhysicalReqeustService PhysicalRequestService => PlatformWindsorManager.GetService<IPhysicalReqeustService>();
        private IArchivedContentDownloadService ArchivedContentDownloadService => PlatformWindsorManager.GetService<IArchivedContentDownloadService>();

        private IArchiverService ControlArchiverService { get { return new AvePoint.Common.Api.Services.ArchiverService(); } set { } }
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMFSConnectionAndOwnerRelationshipDao FSConnectionOwnerDao => PlatformWindsorManager.GetService<IRMFSConnectionAndOwnerRelationshipDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService<IRMMyhubServices>();
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();
        private IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRecordLoanAllianceDao RecordLoanAllianceDao => PlatformWindsorManager.GetService<IRecordLoanAllianceDao>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        private IManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IManualApprovalService>();
        private ITemplateManagementService TemplateManagementService => PlatformWindsorManager.GetService<ITemplateManagementService>();
        private IRMRecordsUpdateTempDao RMRecordsUpdateTempDao => PlatformWindsorManager.GetService<IRMRecordsUpdateTempDao>();
       
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        protected IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        protected IRMRuleDao RMRuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();

        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        public ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        public IRMPhysicalPushColumnDao RMPhysicalPushColumnDao => PlatformWindsorManager.GetService<IRMPhysicalPushColumnDao>();
        public IRMTemplateDao TemplateDao => PlatformWindsorManager.GetService<IRMTemplateDao>();

        public IPhysicalUniqueIdSettingDao PhysicalUniqueIdSettingDao => PlatformWindsorManager.GetService<IPhysicalUniqueIdSettingDao>();

        public IPermissionManagementService PermissionManagementService => PlatformWindsorManager.GetService<IPermissionManagementService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService<IFSConnectionDao>();
        //public IRMBoardCacheDao BoardCacheDao { get; set; }
        public IRMFileSystemBrowserService RMFileSystemBrowserService => PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();
        private IRMCustomBarcodeTemplateSuiteDao CustomBarcodeTemplateSuiteDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateSuiteDao>();
        private IRMCustomBarcodeTemplateDao CustomBarcodeTemplateDao => PlatformWindsorManager.GetService<IRMCustomBarcodeTemplateDao>();

        private IBarcodeTemplateService BarcodeTemplateService => PlatformWindsorManager.GetService<IBarcodeTemplateService>();
        public IExplorerQueryParamProcesser ExplorerQueryParamProcesser => PlatformWindsorManager.GetService<IExplorerQueryParamProcesser>();
        public IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService<IExplorerQueryService>();
        public IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();
        private IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();
        private IRMSharePointOnPremSettingsService RMSharePointOnPremSettingsService => PlatformWindsorManager.GetService<IRMSharePointOnPremSettingsService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IMyhubReportJobDao MyhubReportJobDao => PlatformWindsorManager.GetService<IMyhubReportJobDao>();
        protected IRMNodeFlagDao NodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMEmailManagementService EmailManagementService => PlatformWindsorManager.GetService<IRMEmailManagementService>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();

        private IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ISecurityGroupManagementService _securityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        private readonly ConcurrentDictionary<Guid, List<Rule>> _rulesCache = new ConcurrentDictionary<Guid, List<Rule>>();
        #endregion

        #region Public Function

        //public bool AddRangeData(List<ManagedRecordDto> list)
        //{
        //    var dbList = new List<RMManagedRecord>();
        //    foreach (ManagedRecordDto dto in list)
        //    {
        //        dbList.Add(ConvertUtil.ConvertToRMManagedRecord(dto));
        //    }
        //    return CollectionDataDao.BatchAddRecords(dbList);
        //}

        //public bool AddOrUpdateRecord(ManagedRecordDto dto, bool forceUpdate)
        //{
        //    bool result = false;
        //    try
        //    {
        //        if (dto != null)
        //        {
        //            var rec = ConvertUtil.ConvertToRMManagedRecord(dto);

        //            result = CollectionDataDao.AddOrUpdateRecord(rec, forceUpdate);

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("add data to managed record error:{0}", ex.ToString());
        //    }
        //    return result;
        //}
        private ManualApprovalRuleModel AssemblyRuleModel(SourceFlag flag, string ruleId, string ruleName, string disposalClass, RMRuleInfos ruleInfo)
        {
            if (ruleInfo == null)
            {
                logger.Warn($"Can't find [{flag}] rule by id: [{ruleId}].");
                return new ManualApprovalRuleModel
                {
                    Flag = flag
                };
            }

            AvePoint.GCommon.Contract.StorageOptimization.Object.RetentionInfo retentionInfo = null;
            if (ruleInfo.RetentionInfo != null)
            {
                retentionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.RetentionInfo();
                retentionInfo.IsManualApproval = ruleInfo.RetentionInfo.IsManualApproval;
                retentionInfo.IsSendEamilToOwner = ruleInfo.RetentionInfo.IsSendEamilToOwner;
                retentionInfo.ReviewType = ruleInfo.RetentionInfo.ReviewType;
                retentionInfo.WorkflowId = ruleInfo.RetentionInfo.WorkflowId;
                retentionInfo.UserInfos = ruleInfo.RetentionInfo.UserInfos;
            }

            return new ManualApprovalRuleModel
            {
                IsHasRule = true,
                Flag = flag,
                RuleId = ruleId,
                RuleName = ruleName,
                RuleCriterias = string.Join(" ", ruleInfo.RuleCretias),
                RuleDisposalClass = disposalClass,
                WorkflowId = ruleInfo.WorkflowId,
                IsSendEmailToOwner = ruleInfo.IsSendEmailToOwner,
                Owners = RuleManagerService.Convert2RecordOwnerInfos(ruleInfo.Users),
                RetentionInfo = retentionInfo,
                EnableManualApproval = ruleInfo.EnableManualApproval
            };
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.ManageHold, Action = AuditAction.CreateHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> CreateHoldAsync(UpdateHoldDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (dto.HoldSetting.Name.Length > 255)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                    return msg;
                }
                if (dto != null && dto.HoldSetting != null)
                {
                    CheckHoldInfo(dto);
                    RMHold hold = ConvertUtil.ConvertToRMHold(dto.HoldSetting, await GeneralSettingService.GetGeneralSettingAsync());
                    if (HoldDao.CheckHoldNameExist(hold))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.FaildType = RAFailedType.NameExisting;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_HoldNameExist");
                        return msg;
                    }
                    if (hold.HoldDateType == (int)HoldDateType.Calendar)
                    {
                        if (hold.CalendarTime < DateTime.UtcNow.Ticks)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = "-1";
                            return msg;
                        }
                        var validateReminderDuration = ValidateReminderDuration(msg, hold);
                        if(validateReminderDuration.MessageType == RAMessageType.Failed)
                        {
                            return msg;
                        }
                        if (dto.HoldSetting.EmailNotification?.EmailRecipients != null &&!await ValidateEmailRecipientsManageHoldsPermissionAsync(dto.HoldSetting.EmailNotification.EmailRecipients))
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_EditHoldFailed_RecipientNoPermission");
                            return msg;
                        }
                    }
                    else
                    {
                        if (hold.Number <= 0 || hold.Number > Int32.MaxValue)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_HoldFailed");
                            return msg;
                        }
                    }
                    var userId = TenantLocalValue.LogonUserId;
                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(userId);
                    SecurityUserPermissionsDto permission = SecurityGroupDao.GetUserScopePermissions(userAndGroupIds);
                    var ownerHold = dto.HoldSetting.HoldUserManagers?.Select(u => u.UserId).Contains(userId);
                    if (ownerHold == null && !permission.IsAdmin)
                    {
                        var owner = UserService.GetUserByUserId(userId);

                        dto.HoldSetting.HoldUserManagers = owner != null
                            ? new List<ToUserInfo> { ConvertToToUserInfo(owner) }
                            : new List<ToUserInfo>();
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled) hold.IsHoldManagerEmailNotificationEnabled = true;

                    bool isSaveHoldSuccess = HoldDao.SaveHold(hold, dto.HoldSetting.HoldUserManagers);
                    if (!isSaveHoldSuccess)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "Save hold failed";
                        return msg;
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled)
                    {
                        await SendEmailToHoldManagers(hold, dto.HoldSetting.HoldUserManagers);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("save hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        private ToUserInfo ConvertToToUserInfo(AOSUserDto user)
        {
            return new ToUserInfo
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                UserPrincipalName = user.UserPrincipalName,
                DisplayName = user.DisplayName,
                InviteType = user.InviteType,
                RMUserId = user.RMUserId,
                Id = user.Id,
                SurName = user.SurName,
                GivenName = user.GivenName,
                TenantId = user.TenantId
            };
        }
        private RAReturnMessage ValidateReminderDuration(RAReturnMessage msg, RMHold hold)
        {
            if (hold.IsEmailNotificationEnabled)
            {
                var createdDate = new DateTime(hold.CreateTime).Date;
                var holdUntilDate = new DateTime(hold.CalendarTime).Date;
                int holdDuration = (holdUntilDate - createdDate).Days;
                if (hold.ReminderDurationDays >= holdDuration)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_ErrorReminderDuration");
                }
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.ManageHold, Action = AuditAction.EditHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> EditHoldAsync(UpdateHoldDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };

            if (ExplorerDao.GetRecordbyHoldId(dto.HoldSetting.Id).Any())
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_EditHoldFailed_UsedHold");
                return msg;
            }

            try
            {
                if (dto.HoldSetting.Name.Length > 255)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                    return msg;
                }
                if (dto != null && dto.HoldSetting != null)
                {
                    CheckHoldInfo(dto);
                    var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                    RMHold hold = ConvertUtil.ConvertToRMHold(dto.HoldSetting, generalSetting);
                    if (hold.HoldDateType == (int)HoldDateType.Calendar)
                    {
                        if (hold.CalendarTime < DateTime.UtcNow.Ticks)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_CreateRule_Validation_ConditionErrorDateTime");
                            return msg;
                        }
                        var validateReminderDuration = ValidateReminderDuration(msg, hold);
                        if (validateReminderDuration.MessageType == RAMessageType.Failed)
                        {
                            return msg;
                        }
                        if (dto.HoldSetting.EmailNotification?.EmailRecipients != null && !await ValidateEmailRecipientsManageHoldsPermissionAsync(dto.HoldSetting.EmailNotification.EmailRecipients))
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_EditHoldFailed_RecipientNoPermission");
                            return msg;
                        }
                    }
                    else
                    {
                        if (hold.Number <= 0 || hold.Number > Int32.MaxValue)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_PRM_PRE_Msg_EditHoldFailed");
                            return msg;
                        }
                    }

                    var existingHold = HoldDao.GetHoldById(hold.Id);
                    ResetNotificationState(hold, existingHold);
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled) hold.IsHoldManagerEmailNotificationEnabled = true;
                    bool isSaveHoldSuccess = HoldDao.EditHold(hold, dto.HoldSetting.HoldUserManagers);
                    if (!isSaveHoldSuccess)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "edit hold failed";
                        return msg;
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled)
                    {
                        await SendEmailToHoldManagers(hold, dto.HoldSetting.HoldUserManagers);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("save hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        private void ResetNotificationState(RMHold hold, RMHold existingHold)
        {
            if (!hold.IsEmailNotificationEnabled)
            {
                hold.ReminderDurationDays = 0;
                hold.LastSentEmailTime = 0;
                return;
            }
            var notificationSettingsChanged =
                existingHold.CalendarTime != hold.CalendarTime ||
                existingHold.ReminderDurationDays != hold.ReminderDurationDays;

            hold.LastSentEmailTime = notificationSettingsChanged ? 0 : existingHold.LastSentEmailTime;
        }
        private async Task<bool> ValidateEmailRecipientsManageHoldsPermissionAsync(List<AOSUserDto> users)
        {
            foreach (var user in users)
            {
                var userPermission = await _securityGroupManagementService.GetUserScopePermissionsAsync(user.UserId);
                if (!_securityGroupManagementService.HasManageHoldsPermission(userPermission))
                {
                    return false;
                }
            }
            return true;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeHoldCreate, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> ChangeHoldCreateAsync(UpdateHoldDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var userName = WebUtil.LogOnUserName;
                if (dto.HoldSetting.Name.Length > 255)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                    return msg;
                }
                if (dto != null && dto.HoldSetting != null)
                {
                    RMHold hold = ConvertUtil.ConvertToRMHold(dto.HoldSetting, await GeneralSettingService.GetGeneralSettingAsync());
                    if (HoldDao.CheckHoldNameExist(hold))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.FaildType = RAFailedType.NameExisting;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_HoldNameExist");
                        return msg;
                    }
                    if (dto.HoldSetting.Type == HoldDateType.Calendar)
                    {
                        if (hold.CalendarTime < DateTime.UtcNow.Ticks)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = "-1";
                            return msg;
                        }
                        var validateReminderDuration = ValidateReminderDuration(msg, hold);
                        if (validateReminderDuration.MessageType == RAMessageType.Failed)
                        {
                            return msg;
                        }
                    }
                    var hasOwnerHold = dto.HoldSetting.HoldUserManagers.Select(u => u.UserId).Contains(TenantLocalValue.LogonUserId);
                    if (!hasOwnerHold)
                    {
                        var owner = UserService.GetUserByUserId(TenantLocalValue.LogonUserId);
                        dto.HoldSetting.HoldUserManagers.Add(ConvertToToUserInfo(owner));
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled) hold.IsHoldManagerEmailNotificationEnabled = true;
                    bool isSaveHoldSuccess = HoldDao.SaveHold(hold, dto.HoldSetting.HoldUserManagers);
                    if (!isSaveHoldSuccess)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "Save hold failed";
                        return msg;
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled)
                    {
                        await SendEmailToHoldManagers(hold, dto.HoldSetting.HoldUserManagers);
                    }
                    DateTime utcReleaseTime = this.CalculateHoldReleaseTime(dto.HoldSetting);
                    HoldSettingDto holdDto = new HoldSettingDto()
                    {
                        HoldId = dto.HoldSetting.Id,
                        AllianceType = dto.HoldCategory,
                        HoldAction = dto.HoldAction,
                        ReleaseTime = utcReleaseTime.Ticks,
                        HoldBy = userName
                    };
                    var recordId = dto.ReletedIds.FirstOrDefault();
                    var record = ExplorerDao.GetFSRecordById(recordId);
                    string folderHoldId = record?.HoldId;
                    PlaceHold(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.ChangeHoldCreate);

                    if (record != null && record.NodeType == 2100)
                    {
                        RAReturnMessage returnMessage = new RAReturnMessage();
                        string id = string.Empty;
                        try
                        {
                            var groupId = TenantLocalValue.LogonGroupId;
                            var loginName = TenantLocalValue.LogonUserEmail;

                            HoldOption holdOption = new HoldOption();
                            holdOption.HoldId = holdDto.HoldId;
                            holdOption.RelatedRecords = dto.ReletedIds;
                            holdOption.ReleaseTime = holdDto.ReleaseTime;
                            holdOption.Action = (int)AuditAction.ChangeHoldCreate;
                            holdOption.PlaceHoldAction = dto.HoldAction;
                            holdOption.HoldBy = userName;
                            holdOption.IsOverWrite = dto.IsOverRide;
                            holdOption.FolderOriginalHoldId = folderHoldId;
                            holdOption.UserId = TenantLocalValue.LogonUserId;
                            JobQueueDto jqDto = new JobQueueDto()
                            {
                                JobType = JobType.FSFolderManageHold,
                                Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                                JobRunType = JobRunBy.Control,
                                TenantGroupId = groupId,
                                JobRunByUser = loginName
                            };
                            returnMessage.MessageType = RAMessageType.Successful;
                            returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                        }
                        catch (Exception ex)
                        {
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = ex.Message;
                        }
                        return returnMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("save hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        private DateTime CalculateHoldReleaseTime(HoldSetting hold)
        {
            if (hold.Type == HoldDateType.Custom)
            {
                DateTime tempNow = new DateTime();
                if (hold.Unit == HoldDateUnit.Day)
                {
                    tempNow = DateTime.UtcNow.AddDays(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Week)
                {
                    tempNow = DateTime.UtcNow.AddDays(7 * hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Month)
                {
                    tempNow = DateTime.UtcNow.AddMonths(hold.Number);
                }
                else if (hold.Unit == HoldDateUnit.Years)
                {
                    tempNow = DateTime.UtcNow.AddYears(hold.Number);
                }
                return tempNow;
            }
            else
            {
                DateTime calenderTime = DateTime.Parse(hold.CalenderTime);
                calenderTime = DateTime.SpecifyKind(calenderTime, DateTimeKind.Unspecified);
                //DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, TimeZoneInfo.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, GeneralSettingConfig.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                return utcTime;
                //if (hold.ProfileType == HoldProfileType.Normal)
                //{
                //    DateTime calenderTime = DateTime.Parse(hold.CalenderTime);
                //    calenderTime = DateTime.SpecifyKind(calenderTime, DateTimeKind.Unspecified);
                //    DateTime utcTime = DateTimeUtil.ConvertTimeToUtcDate(calenderTime, TimeZoneInfo.FindSystemTimeZoneById(hold.TimeZoneId), !hold.IsDayLightSaving);
                //    return utcTime;
                //}
                //else
                //{
                //    return hold.CalendarDate;
                //}
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeHoldReuse, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage ChangeHoldReuse(UpdateHoldDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                CheckHoldSetting(dto);
                var userName = WebUtil.LogOnUserName;
                DateTime tempUtcReleaseTime = this.CalculateHoldReleaseTime(dto.HoldSetting);
                if (dto.HoldSetting.Type == HoldDateType.Calendar && tempUtcReleaseTime < DateTime.UtcNow)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = "-1";
                    return msg;
                }
                HoldSettingDto holdDto = new HoldSettingDto()
                {
                    HoldId = dto.HoldSetting.Id,
                    AllianceType = dto.HoldCategory,
                    HoldAction = dto.HoldAction,
                    ReleaseTime = tempUtcReleaseTime.Ticks,
                    HoldBy = userName
                };
                var recordId = dto.ReletedIds.FirstOrDefault();
                var record = ExplorerDao.GetFSRecordById(recordId);
                string folderHoldId = record?.HoldId;
                PlaceHold(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.ChangeHoldReuse);

                if (record != null && record.NodeType == 2100)
                {
                    RAReturnMessage returnMessage = new RAReturnMessage();
                    string id = string.Empty;
                    try
                    {
                        var groupId = TenantLocalValue.LogonGroupId;
                        var loginName = TenantLocalValue.LogonUserEmail;

                        HoldOption holdOption = new HoldOption();
                        holdOption.HoldId = holdDto.HoldId;
                        holdOption.RelatedRecords = dto.ReletedIds;
                        holdOption.ReleaseTime = holdDto.ReleaseTime;
                        holdOption.Action = (int)AuditAction.ChangeHoldReuse;
                        holdOption.PlaceHoldAction = dto.HoldAction;
                        holdOption.HoldBy = userName;
                        holdOption.IsOverWrite = dto.IsOverRide;
                        holdOption.FolderOriginalHoldId = folderHoldId;
                        holdOption.UserId = TenantLocalValue.LogonUserId;
                        JobQueueDto jqDto = new JobQueueDto()
                        {
                            JobType = JobType.FSFolderManageHold,
                            Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                            JobRunType = JobRunBy.Control,
                            TenantGroupId = groupId,
                            JobRunByUser = loginName
                        };
                        returnMessage.MessageType = RAMessageType.Successful;
                        returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = ex.Message;
                    }
                    return returnMessage;
                }
            }
            catch (Exception ex)
            {
                logger.Error("reuse hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }



        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.CreateHoldTypeWithRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> CreateHoldTypeWithRecordAsync(UpdateHoldDto dto, bool isFS = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var listItemOnLoaned = RecordLoanAllianceDao.GetChildAndParentRecordAllianceByIds(dto.ReletedIds);
                var userName = WebUtil.LogOnUserName;
                if (dto.HoldSetting.Name.Length > 255)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                    return msg;
                }
                if (dto != null && dto.HoldSetting != null)
                {
                    RMHold hold = ConvertUtil.ConvertToRMHold(dto.HoldSetting, await GeneralSettingService.GetGeneralSettingAsync());
                    if (HoldDao.CheckHoldNameExist(hold))
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.FaildType = RAFailedType.NameExisting;
                        msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_HoldNameExist");
                        return msg;
                    }
                    if (dto.HoldSetting.Type == HoldDateType.Calendar)
                    {
                        if (hold.CalendarTime < DateTime.UtcNow.Ticks)
                        {
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = "-1";
                            return msg;
                        }
                        var validateReminderDuration = ValidateReminderDuration(msg, hold);
                        if (validateReminderDuration.MessageType == RAMessageType.Failed)
                        {
                            return msg;
                        }
                    }

                    var hasOwnerHold = dto.HoldSetting.HoldUserManagers.Select(u => u.UserId).Contains(TenantLocalValue.LogonUserId);
                    if (!hasOwnerHold)
                    {
                        var owner = UserService.GetUserByUserId(TenantLocalValue.LogonUserId);
                        dto.HoldSetting.HoldUserManagers.Add(ConvertToToUserInfo(owner));
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled) hold.IsHoldManagerEmailNotificationEnabled = true;
                    bool isSaveHoldSuccess = HoldDao.SaveHold(hold, dto.HoldSetting.HoldUserManagers);
                    if (!isSaveHoldSuccess)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "Save hold failed";
                        return msg;
                    }
                    if (dto.HoldSetting.IsHoldManagerEmailNotificationEnabled)
                    {
                        await SendEmailToHoldManagers(hold, dto.HoldSetting.HoldUserManagers);
                    }
                    DateTime tempUtcReleaseTime = this.CalculateHoldReleaseTime(dto.HoldSetting);
                    if (dto.HoldSetting.Type == HoldDateType.Calendar && tempUtcReleaseTime < DateTime.UtcNow)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = "-1";
                        return msg;
                    }
                    HoldSettingDto holdDto = new HoldSettingDto()
                    {
                        HoldId = dto.HoldSetting.Id,
                        AllianceType = dto.HoldCategory,
                        ReleaseTime = tempUtcReleaseTime.Ticks,
                        HoldBy = userName,
                        NeedCheckConflicted = dto.NeedCheckOverride,
                        IsOverride = dto.IsOverRide,
                        HoldAction = dto.HoldAction
                    };
                    if (holdDto.NeedCheckConflicted)
                    {
                        this.PlaceHoldWithConflictedResolution(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.CreateHoldTypeWithRecord);
                    }
                    else
                    {
                        PlaceHold(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.CreateHoldTypeWithRecord);
                    }
                    if (listItemOnLoaned.Any())
                    {
                        logger.Info($"Starting update return date after placing hold for items id: {0}", string.Join(", ", listItemOnLoaned.Select(x => x.RecordsId)));
                        await UpdateReturnDateAndSendEmailAsync(listItemOnLoaned, dto.ReletedIds, dto.IsSendEmailToBorrower);
                    }
                    var recordId = dto.ReletedIds.FirstOrDefault();
                    var record = ExplorerDao.GetFSRecordById(recordId);
                    if (record != null && record.NodeType == 2100)
                    {
                        RAReturnMessage returnMessage = new RAReturnMessage();
                        string id = string.Empty;
                        try
                        {
                            var groupId = TenantLocalValue.LogonGroupId;
                            var loginName = TenantLocalValue.LogonUserEmail;

                            HoldOption holdOption = new HoldOption();
                            holdOption.HoldId = holdDto.HoldId;
                            holdOption.RelatedRecords = dto.ReletedIds;
                            holdOption.ReleaseTime = holdDto.ReleaseTime;
                            holdOption.Action = (int)AuditAction.CreateHoldTypeWithRecord;
                            holdOption.PlaceHoldAction = dto.HoldAction;
                            holdOption.HoldBy = userName;
                            holdOption.IsOverWrite = holdDto.IsOverride;
                            holdOption.UserId = TenantLocalValue.LogonUserId;
                            JobQueueDto jqDto = new JobQueueDto()
                            {
                                JobType = JobType.FSFolderManageHold,
                                Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                                JobRunType = JobRunBy.Control,
                                TenantGroupId = groupId,
                                JobRunByUser = loginName
                            };
                            returnMessage.MessageType = RAMessageType.Successful;
                            returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                        }
                        catch (Exception ex)
                        {
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = ex.Message;
                        }
                        return returnMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("save hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }


        public RAReturnMessage CheckItemOnLoaned(List<Guid> ids)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
                Extension = JsonConvert.SerializeObject(new { CanPlaceHold = true }),
            };
            var listItemOnLoaned = RecordLoanAllianceDao.GetChildAndParentRecordAllianceByIds(ids);
            if (listItemOnLoaned.Any())
            {
                msg.MessageType = RAMessageType.Confirmation;
                msg.ErrorMessage = I18NEntity.GetString("RM_TM_ConfirmPlaceHold");
                return msg;
            }
            return msg;
        }
        public async Task<int> ChangeTermForGlobalSearchAsync(List<Guid> recordsId, SourceFlag flag, string jobId, ChangeTermOption changeTermOption, bool isJob)
        {
            int failedCount = 0;
            if (changeTermOption == null)
            {
                return recordsId.Count;
            }

            switch (flag)
            {
                case SourceFlag.SharePoint:
                    failedCount = await ChangeTermRealTimeSPForGlobalSearchAsync(changeTermOption, jobId, false, isJob);
                    break;
                case SourceFlag.Exchange:
                    failedCount = ChangeTermRealTimeEXOForGlobalSearch(changeTermOption, jobId, false, isJob);
                    break;
                case SourceFlag.FileSystem:
                    failedCount = ChangeTermRealTimeFSForGlobalSearch(changeTermOption, jobId, isJob);
                    break;
                case SourceFlag.Physical:
                    failedCount = ChangeTermRealTimePhyForGlobalSearch(changeTermOption, jobId, isJob);
                    break;
                case SourceFlag.OneDrive:
                    failedCount = await ChangeTermRealTimeOneDriveForGlobalSearchAsync(changeTermOption, jobId, isJob);
                    break;
                case SourceFlag.AzureFileShare:
                    var reclassifier = new AzureFileShareReclassifier(changeTermOption, jobId, isJob);
                    reclassifier.Reclassify();
                    failedCount = reclassifier.FailedItemsCount;
                    break;
                case SourceFlag.Box:
                    var boxReclassifier = new BoxReclassifier(changeTermOption, jobId, isJob);
                    boxReclassifier.Reclassify();
                    failedCount = boxReclassifier.FailedItemsCount;
                    break;
                case SourceFlag.Teams:
                    failedCount = await ChangeTermRealTimeTeamsForGlobalSearchAsync(changeTermOption, jobId, false, isJob);
                    break;
                case var f when (int)f >= 1000:
                    var connectorReclassifier = new CustomizeConnectorReclassifier(changeTermOption, jobId, isJob);
                    await connectorReclassifier.ReclassifyAsync();
                    failedCount = connectorReclassifier.FailedItemsCount;
                    break;
            }

            return failedCount;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ReuseHoldTypeWithRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> ReuseHoldTypeWithRecord(UpdateHoldDto dto, bool isFS = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var listItemOnLoaned = RecordLoanAllianceDao.GetChildAndParentRecordAllianceByIds(dto.ReletedIds);
                CheckHoldSetting(dto);
                var userName = WebUtil.LogOnUserName;
                DateTime tempUtcReleaseTime = this.CalculateHoldReleaseTime(dto.HoldSetting);
                if (dto.HoldSetting.Type == HoldDateType.Calendar && tempUtcReleaseTime < DateTime.UtcNow)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = "-1";
                    return msg;
                }
                HoldSettingDto holdDto = new HoldSettingDto()
                {
                    HoldId = dto.HoldSetting.Id,
                    AllianceType = dto.HoldCategory,
                    ReleaseTime = tempUtcReleaseTime.Ticks,
                    HoldBy = userName,
                    NeedCheckConflicted = dto.NeedCheckOverride,
                    IsOverride = dto.IsOverRide,
                    HoldAction = dto.HoldAction
                };
                if (holdDto.NeedCheckConflicted)
                {
                    this.PlaceHoldWithConflictedResolution(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.ReuseHoldTypeWithRecord);
                }
                else
                {
                    PlaceHold(dto.ReletedIds, holdDto, dto.FileIds, dto.HoldSetting.Name, AuditAction.ReuseHoldTypeWithRecord);
                }
                if (listItemOnLoaned.Any())
                {
                    logger.Info($"Starting update return date after placing hold for items id: {0}", string.Join(", ", listItemOnLoaned.Select(x => x.RecordsId)));
                    await UpdateReturnDateAndSendEmailAsync(listItemOnLoaned, dto.ReletedIds, dto.IsSendEmailToBorrower);
                }
                var recordId = dto.ReletedIds.FirstOrDefault();
                var record = ExplorerDao.GetFSRecordById(recordId);
                if (record != null && record.NodeType == 2100)
                {
                    RAReturnMessage returnMessage = new RAReturnMessage();
                    string id = string.Empty;
                    try
                    {
                        var groupId = TenantLocalValue.LogonGroupId;
                        var loginName = TenantLocalValue.LogonUserEmail;

                        HoldOption holdOption = new HoldOption();
                        holdOption.HoldId = holdDto.HoldId;
                        holdOption.RelatedRecords = dto.ReletedIds;
                        holdOption.ReleaseTime = holdDto.ReleaseTime;
                        holdOption.Action = (int)AuditAction.ReuseHoldTypeWithRecord;
                        holdOption.PlaceHoldAction = dto.HoldAction;
                        holdOption.HoldBy = userName;
                        holdOption.IsOverWrite = holdDto.IsOverride;
                        holdOption.UserId = TenantLocalValue.LogonUserId;
                        JobQueueDto jqDto = new JobQueueDto()
                        {
                            JobType = JobType.FSFolderManageHold,
                            Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                            JobRunType = JobRunBy.Control,
                            TenantGroupId = groupId,
                            JobRunByUser = loginName
                        };
                        returnMessage.MessageType = RAMessageType.Successful;
                        returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = ex.Message;
                    }
                    return returnMessage;
                }
            }
            catch (Exception ex)
            {
                logger.Error("reuse hold and update record error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        public async Task<List<HoldSetting>> GetHoldAsync(int profileType = 0)
        {
            List<HoldSetting> settings = new List<HoldSetting>();
            List<RMHold> holds = HoldDao.GetAllHolds(profileType);
            if (holds != null && holds.Count > 0)
            {
                Dictionary<string, List<ToUserInfo>> holdUserDic = await HoldDao.GetUsersManageHold(holds.Select(a => a.Id).ToList());
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var records = ExplorerDao.GetRecordbyHoldIds(holds.Select(h => h.Id).ToList());
                foreach (RMHold hold in holds)
                {
                    HoldSetting holdDto = ConvertUtil.ConvertToHoldSetting(hold, gls);
                    holdDto.CreateTime = mGeneralSettingService.ConvertTiksToDateTime(gls, hold.CreateTime, true).SimplifyFormatTime;
                    holdDto.hasRelated = records.Where(r => (r.AppendHolds_Array != null && r.AppendHolds_Array.Contains(hold.Id)) || r.HoldId == hold.Id).Any();
                    holdDto.HoldUserManagers = holdUserDic.ContainsKey(hold.Id) ? holdUserDic[hold.Id] : new List<ToUserInfo>();
                    settings.Add(holdDto);
                }
            }
            return settings;
        }

        public async Task<List<HoldSetting>> GetAssignedHoldsAsync()
        {
            var isHoldManage = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold);
            List<HoldSetting> settings = new List<HoldSetting>();
            var holds = new List<RMHold>();
            if (isHoldManage)
            {
                holds = HoldDao.GetAllHoldsByUserAssignedManage();
            }

            if (holds != null && holds.Count > 0)
            {
                Dictionary<string, List<ToUserInfo>> holdUserDic = await HoldDao.GetUsersManageHold(holds.Select(a => a.Id).ToList());
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var records = ExplorerDao.GetRecordbyHoldIds(holds.Select(h => h.Id).ToList());
                foreach (RMHold hold in holds)
                {
                    HoldSetting holdDto = ConvertUtil.ConvertToHoldSetting(hold, gls);
                    holdDto.CreateTime = mGeneralSettingService.ConvertTiksToDateTime(gls, hold.CreateTime, true).SimplifyFormatTime;
                    holdDto.hasRelated = records.Where(r => (r.AppendHolds_Array != null && r.AppendHolds_Array.Contains(hold.Id)) || r.HoldId == hold.Id).Any();
                    holdDto.HoldUserManagers = holdUserDic.ContainsKey(hold.Id) ? holdUserDic[hold.Id] : new List<ToUserInfo>();
                    settings.Add(holdDto);
                }
            }
            return settings;
        }

        
        public async Task<List<HoldSetting>> GetSampleHoldAsync(int profileType = 0)
        {
            List<HoldSetting> settings = new List<HoldSetting>();
            List<RMHold> holds = new List<RMHold>();
            if (!await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.ManageHoldEndUser))
            {
                holds = HoldDao.GetAllHoldsByUserAssignedManage();
            }
            else
            {
                holds = HoldDao.GetAllHolds(profileType);
            }

            if (holds != null && holds.Count > 0)
            {
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (RMHold hold in holds)
                {
                    HoldSetting holdDto = ConvertUtil.ConvertToHoldSetting(hold, gls);
                    holdDto.CreateTime = mGeneralSettingService.ConvertTiksToDateTime(gls, hold.CreateTime, true).SimplifyFormatTime;
                    settings.Add(holdDto);
                }
            }
            return settings;
        }

        public bool IsBoxHasHoldChildren(List<Guid> boxId)
        {
            List<Guid> tempIds = boxId.Where(a => a != Guid.Empty).ToList();
            if (tempIds.IsNullOrEmpty())
            {
                logger.Warn("Box id is empty.");
                return false;
            }
            var fileAls = ExplorerDao.QueryAll(r => tempIds.Contains(r.BoxId) && r.HoldStatus);
            if (!fileAls.IsNullOrEmpty())
            {
                return true;
            }
            return false;
        }
        public List<string> GetHoldChildrenByBox(List<Guid> boxId)
        {
            List<string> result = null;
            List<Guid> tempIds = boxId.Where(a => a != Guid.Empty).ToList();
            if (tempIds.IsNullOrEmpty())
            {
                logger.Warn("Box id is empty.");
                return result;
            }
            var fileAls = ExplorerDao.QueryAll(r => tempIds.Contains(r.BoxId) && r.HoldStatus);
            if (!fileAls.IsNullOrEmpty())
            {
                List<Guid> ids = fileAls.Select(a => a.Id).ToList();
                if (ids.Count > 20)
                {
                    ids = ids.Take(20).ToList();
                }
                try
                {
                    List<Record> files = ExplorerDao.QueryAll(a => ids.Contains(a.Id)).ToList();
                    result = files.Select(a => a.LeafName).ToList();
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                    result = fileAls.Select(a => a.RecordsId.ToString()).ToList();
                }
            }
            return result;
        }

        /// <summary>
        /// 不可用于Physical
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public async Task<HoldSetting> GetHoldByRecoedIdAsync(Guid recordId, int allianceType = RecordsConstants.RecordHold_Electronic)
        {
            HoldSetting hold = new HoldSetting();
            Record alliance = ExplorerDao.GetRecordByIds(new List<Guid>() { recordId }).FirstOrDefault();
            if (alliance != null && !string.IsNullOrEmpty(alliance.HoldId))
            {
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                List<RMHold> holds = HoldDao.GetHoldByIds(new List<string>() { alliance.HoldId });
                hold = ConvertUtil.ConvertToHoldSetting(holds[0], gls);
            }

            return hold;
        }
        
        public List<string> GetHoldsByRecoedId(Guid recordId)
        {
            List<string> holdIds = new List<string>();
            var record = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.HoldId))
                {
                    holdIds.Add(record.HoldId);
                }
                if (record.AppendHolds_Array != null)
                {
                    holdIds.AddRange(record.AppendHolds_Array.ToList());
                }
            }
            return holdIds;
        }

        public async Task<List<RemoveHoldSetting>> GetHoldListByRecoedIdAsync(Guid recordId)
        {
            List<RemoveHoldSetting> holdSettings = new List<RemoveHoldSetting>();
            List<string> holdIds = new List<string>();
            var record = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { recordId }).FirstOrDefault();
            if (record != null)
            {
                if (!string.IsNullOrEmpty(record.HoldId))
                {
                    holdIds.Add(record.HoldId);
                }
                if (record.AppendHolds_Array != null)
                {
                    holdIds.AddRange(record.AppendHolds_Array.ToList());
                }

                if (!await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin) && !await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.ManageHoldEndUser))
                {
                    logger.Info("Current user is not ControlPanelAdmin, only can cancel hold which created by current user.");
                    holdIds = HoldMembershipDao.GetCurrentUserHoldIds(holdIds);
                    logger.Info("Current user can cancel hold ids: {0}", string.Join(", ", holdIds));
                }
                List<RMHold> holds = HoldDao.GetHoldByIds(holdIds).OrderBy(h => h.Id, new HoldSpecialComparer(holdIds)).ToList();
                List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);

                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (RMHold hold in holds)
                {
                    var holdUntilTime = allHoldUntilTimes.First(h => h.HoldId == hold.Id);
                    var holdUntilTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, holdUntilTime.UntilTime, true).SimplifyFormatTime;
                    holdSettings.Add(new RemoveHoldSetting() { Id = hold.Id, Name = hold.Name, HoldUntilTime = holdUntilTimeStr });
                }
            }
            return holdSettings;
        }
        /// <summary>
        /// 不可用于Physical
        /// </summary>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public RMHold GetRMHoldByRecoedId(Guid recordId)
        {
            RMHold hold = new RMHold();
            Record alliance = ExplorerDao.GetRecordByIds(new List<Guid>() { recordId }).FirstOrDefault();
            if (alliance != null && !string.IsNullOrEmpty(alliance.HoldId))
            {
                List<RMHold> holds = HoldDao.GetHoldByIds(new List<string>() { alliance.HoldId });
                if (holds.IsNullOrEmpty())
                {
                    hold = holds[0];
                }
            }
            return hold;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.CancelHoldByRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage CancelHoldByRecords(List<Guid> recordsId, bool isPhysical = false, List<string> removeHoldIds = null)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var userName = WebUtil.LogOnUserName;
                var recordId = recordsId.FirstOrDefault();
                var record = ExplorerDao.GetRecordByIds(new List<Guid>() { recordId }).FirstOrDefault();

                if (removeHoldIds == null)
                {
                    ExplorerDao.UpdateAll(r => recordsId.Contains(r.Id), s =>
                    {
                        s.HoldStatus = false; s.HoldType = 0;
                        s.HoldReleaseTime = DateTime.MinValue.Ticks;
                        s.HoldId = null; s.HoldBy = null;
                        s.HoldByUsers = null;
                        s.HoldUntilTimes = null;
                        s.AppendHolds_Array = new string[0];
                        s.DisposalDueDate = s.PreviosDisposalDueDate;
                    });
                }
                else
                {
                    if (isPhysical)
                    {
                        RecordsHistoryService.AddPhysicalCommonHoldActionAudit(record.Id, PhysicalActionType.CancelHold);
                    }
                    CancelHoldBySelected(removeHoldIds, record);
                }

                if (record != null && record.NodeType == 2100)
                {
                    RAReturnMessage returnMessage = new RAReturnMessage();
                    string id = string.Empty;
                    try
                    {
                        var groupId = TenantLocalValue.LogonGroupId;
                        var loginName = TenantLocalValue.LogonUserEmail;

                        HoldOption holdOption = new HoldOption();
                        holdOption.HoldId = string.Empty;
                        holdOption.RelatedRecords = recordsId;
                        holdOption.ReleaseTime = 0;
                        holdOption.Action = (int)AuditAction.CancelHoldByRecords;
                        holdOption.RemoveHolds = removeHoldIds;
                        holdOption.HoldBy = userName;
                        holdOption.IsOverWrite = false;
                        holdOption.UserId = TenantLocalValue.LogonUserId;
                        JobQueueDto jqDto = new JobQueueDto()
                        {
                            JobType = JobType.FSFolderManageHold,
                            Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                            JobRunType = JobRunBy.Control,
                            TenantGroupId = groupId,
                            JobRunByUser = loginName
                        };
                        returnMessage.MessageType = RAMessageType.Successful;
                        returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = ex.Message;
                    }
                    return returnMessage;
                }
            }
            catch (Exception ex)
            {
                logger.Error("cancel hold and delete alliance by recordId has error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SusPendRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage SusPendRecords(UpdateHoldDto dto, bool isFS = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var isAdmin = SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin).GetAwaiter().GetResult();
                var canManageHoldEndUser = SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.ManageHoldEndUser).GetAwaiter().GetResult();
                var userName = WebUtil.LogOnUserName;
                DateTime releaseTime = DateTime.MinValue;
                var allowedHoldIds = new List<string>();
                var settingHoldRecords = ExplorerDao.GetHoldRecordsByIds(dto.ReletedIds);
                if (settingHoldRecords != null && settingHoldRecords.Count > 0)
                {
                    var account = AccountDao.GetActiveUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult();
                    if (settingHoldRecords.Count > 0)
                    {
                        allowedHoldIds = HoldMembershipDao.GetCurrentUserHoldIds(settingHoldRecords.Select(r => r.HoldId).ToList());
                    }
                    foreach (var record in settingHoldRecords)
                    {
                        if (!isAdmin && !canManageHoldEndUser)
                        {
                            
                            if (!allowedHoldIds.Contains(record.HoldId))
                            {
                                logger.Info("you don't have permission for this hold: " + record.HoldId);
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_InvalidPermission");
                                continue;
                            }
                        }
                        if (record.SourceFlag == (int)SourceFlag.Physical)
                        {
                            RecordsHistoryService.AddPhysicalCommonHoldActionAudit(record.Id, PhysicalActionType.ExtendHold);
                        }
                        //选择Record Extend 直接在最长Hold Release Time上 Extend
                        DateTime oldReleaseTimeUtc = new DateTime(record.HoldReleaseTime, DateTimeKind.Utc);
                        if (dto.HoldSetting.Unit == HoldDateUnit.Day)
                        {
                            releaseTime = oldReleaseTimeUtc.AddDays(dto.HoldSetting.Number);
                        }
                        else if (dto.HoldSetting.Unit == HoldDateUnit.Week)
                        {
                            releaseTime = oldReleaseTimeUtc.AddDays(7 * dto.HoldSetting.Number);
                        }
                        else if (dto.HoldSetting.Unit == HoldDateUnit.Month)
                        {
                            releaseTime = oldReleaseTimeUtc.AddMonths(dto.HoldSetting.Number);
                        }
                        else if (dto.HoldSetting.Unit == HoldDateUnit.Years)
                        {
                            releaseTime = oldReleaseTimeUtc.AddYears(dto.HoldSetting.Number);
                        }
                        record.HoldReleaseTime = releaseTime.Ticks;
                        List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);
                        var maxReleaseTimeHold = allHoldUntilTimes.FirstOrDefault(h => h.HoldId == record.HoldId);
                        if (maxReleaseTimeHold != null)
                        {
                            maxReleaseTimeHold.UntilTime = record.HoldReleaseTime;
                        }    
                        //Hold状态Record重新计算Due Date;
                        if (record.RuleId != null && record.RuleId != Guid.Empty)
                        {
                            var tempRule = RMRuleDao.GetRuleById(record.RuleId);
                            if (tempRule != null && IsRemoveRule(tempRule, record.SourceFlag))
                            {
                                var newDisposalDueDate = record.DisposalDueDate;
                                //Remove Rule需要计算Due Date
                                if (record.PreviosDisposalDueDate == DueDateUtil.NextJob)
                                {
                                    newDisposalDueDate = record.HoldReleaseTime;
                                }
                                if (record.PreviosDisposalDueDate > 0)
                                {
                                    if (record.PreviosDisposalDueDate > record.HoldReleaseTime)
                                    {
                                        newDisposalDueDate = record.PreviosDisposalDueDate;
                                    }
                                    else
                                    {
                                        newDisposalDueDate = record.HoldReleaseTime;
                                    }
                                }
                                //更新Remove类型Item的Due Date为新值
                                ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                                {
                                    s.HoldReleaseTime = record.HoldReleaseTime;
                                    s.HoldBy = userName;
                                    s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                    s.DisposalDueDate = newDisposalDueDate;
                                });
                            }
                            else {
                                ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                                {
                                    s.HoldReleaseTime = record.HoldReleaseTime;
                                    s.HoldBy = userName;
                                    s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                });
                            }
                        }
                        else
                        {
                            ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                            {
                                s.HoldReleaseTime = record.HoldReleaseTime;
                                s.HoldBy = userName;
                                s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            });
                        }
                        record.HoldBy = userName;
                    }
                }

                var recordId = dto.ReletedIds.FirstOrDefault();
                var folderRecord = ExplorerDao.GetFSRecordById(recordId);
                if (folderRecord != null && folderRecord.NodeType == 2100)
                {
                    var myHoldIds = new List<string>();
                    RAReturnMessage returnMessage = new RAReturnMessage();
                    if (!isAdmin && !canManageHoldEndUser)
                    {
                        if (!allowedHoldIds.Contains(folderRecord.HoldId))
                        {
                            logger.Info("You don't have permission to manage this hold");
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_InvalidPermission");
                            return returnMessage;
                        }
                    }

                    string id = string.Empty;
                    try
                    {
                        var groupId = TenantLocalValue.LogonGroupId;
                        var loginName = TenantLocalValue.LogonUserEmail;

                        HoldOption holdOption = new HoldOption();
                        //holdOption.HoldId = dto.HoldIds.FirstOrDefault();
                        holdOption.RelatedRecords = dto.ReletedIds;
                        holdOption.ReleaseTime = releaseTime.Ticks;
                        holdOption.Action = (int)AuditAction.SusPendRecords;
                        holdOption.HoldBy = userName;
                        holdOption.Unit = dto.HoldSetting.Unit;
                        holdOption.Number = dto.HoldSetting.Number;
                        holdOption.IsOverWrite = dto.IsOverRide;
                        holdOption.HoldId = folderRecord.HoldId;
                        holdOption.UserId = TenantLocalValue.LogonUserId;
                        JobQueueDto jqDto = new JobQueueDto()
                        {
                            JobType = JobType.FSFolderManageHold,
                            Parameters = SerializerHelper.SerializeByDataContractSerializer(holdOption),
                            JobRunType = JobRunBy.Control,
                            TenantGroupId = groupId,
                            JobRunByUser = loginName
                        };
                        returnMessage.MessageType = RAMessageType.Successful;
                        returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = ex.Message;
                    }
                    return returnMessage;
                }

            }
            catch (Exception ex)
            {
                logger.Error("save hold and update hold error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Hold_InvalidDuration");
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.SuspendHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage SusPendHolds(UpdateHoldDto dto, bool isFS = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                foreach (var holdId in dto.HoldIds)
                {
                    var records = ExplorerDao.GetRecordbyHoldId(holdId);
                    long releaseTime = 0;
                    if (records != null && records.Count > 0)
                    {
                        //根据所选的Hold 进行Extend，Extend之后，在找出最长Release 时间的Hold，需要每条数据单独处理
                        foreach (var record in records)
                        {
                            List<HoldUser> allHoldByUsers = GetAllHoldByUsers(record);
                            List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);
                            var selectedHoldReleaseTime = allHoldUntilTimes.FirstOrDefault(h => h.HoldId == holdId);
                            if (selectedHoldReleaseTime != null)
                            {
                                long oldReleaseTime = selectedHoldReleaseTime.UntilTime;
                                if (dto.HoldSetting.Unit == HoldDateUnit.Day)
                                {
                                    releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddDays(dto.HoldSetting.Number).Ticks;
                                }
                                else if (dto.HoldSetting.Unit == HoldDateUnit.Week)
                                {
                                    releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddDays(7 * dto.HoldSetting.Number).Ticks;
                                }
                                else if (dto.HoldSetting.Unit == HoldDateUnit.Month)
                                {
                                    releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddMonths(dto.HoldSetting.Number).Ticks;
                                }
                                else if (dto.HoldSetting.Unit == HoldDateUnit.Years)
                                {
                                    releaseTime = new DateTime(oldReleaseTime, DateTimeKind.Utc).AddYears(dto.HoldSetting.Number).Ticks;
                                }
                                selectedHoldReleaseTime.UntilTime = releaseTime;
                                record.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            }

                            Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(record, out string[] appendHoldsArray, null);
                            long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                            string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                            //Hold状态Record重新计算Due Date;
                            var isRemoveRuleData = false;
                            if (record != null && record.RuleId != null && record.RuleId != Guid.Empty)
                            {
                                var tempRule = RMRuleDao.GetRuleById(record.RuleId);
                                if (tempRule != null && IsRemoveRule(tempRule, record.SourceFlag))
                                {
                                    isRemoveRuleData = true;
                                    var newDisposalDueDate = new List<long>() { record.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                                    //Remove Rule需要计算Due Date
                                    //更新Remove类型Item的Due Date为新值
                                    ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                                    {
                                        s.HoldReleaseTime = firstMaxHoldTime;
                                        s.HoldId = firstMaxHoldSettingId;
                                        s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                        s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                        s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                        s.AppendHolds_Array = appendHoldsArray;
                                        s.DisposalDueDate = newDisposalDueDate;
                                    });
                                }
                            }
                            if (!isRemoveRuleData)
                            {
                                ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                                {
                                    s.HoldReleaseTime = firstMaxHoldTime;
                                    s.HoldId = firstMaxHoldSettingId;
                                    s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                    s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                    s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                    s.AppendHolds_Array = appendHoldsArray;
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("save hold and update hold error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.CancelHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage CancelHoldSetting(List<string> holdIds, bool isPersonal = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var records = ExplorerDao.GetRecordbyHoldIds(holdIds).ToList();
                foreach (var record in records)
                {
                    CancelHoldBySelected(holdIds, record);
                }
            }
            catch (Exception ex)
            {
                logger.Error("cancel hold and delete alliance by holdId has error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.ManageHold, Action = AuditAction.DeleteHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteHoldAndSettingAsync(List<string> holdIds, bool isPersonal = false)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                List<Guid> recordIds = ExplorerDao.GetRecordbyHoldIds(holdIds).Select(r => r.Id).ToList();
                if (recordIds.Count > 0)
                {
                    ExplorerDao.UpdateAll(r => recordIds.Contains(r.Id), s =>
                    {
                        s.HoldStatus = false;
                        s.HoldReleaseTime = DateTime.MinValue.Ticks;
                        s.HoldId = null; s.HoldBy = null; 
                        s.HoldByUsers = null;
                        s.HoldUntilTimes = null;
                        s.AppendHolds_Array = new string[0];
                        s.DisposalDueDate = s.PreviosDisposalDueDate;
                    });
                }
                RecordsHistoryService.AddRecordsHistory(recordIds, AuditAction.CancelHoldByRecords.ToDescription());
                await HoldDao.DeleteHoldAsync(holdIds);
                HoldMembershipDao.DeleteHoldMembershipsByHoldIds(holdIds);
            }
            catch (Exception ex)
            {
                logger.Error("delete hold has error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        //public void MoveToArchived(Guid scopeId, Guid nodeId)
        //{
        //    try
        //    {
        //        CollectionDataDao.MoveToArchived(scopeId, nodeId);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while move record to destroyed,ERROR:{0}", ex.ToString());
        //    }

        //}

        //public void MoveToDeleted(Guid scopeId, Guid nodeId)
        //{
        //    try
        //    {
        //        CollectionDataDao.MoveToDeleted(scopeId, nodeId);

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while move record to destroyed,ERROR:{0}", ex.ToString());
        //        throw;
        //    }

        //}

        //public ExplorerResultInfo QueryDataListWithoutTotal(ExplorerQueryDto dto, bool isGlobalSearch = false)
        //{
        //    ExplorerResultInfo resultInfo = new ExplorerResultInfo();
        //    List<BaseRecordDto> resultList = new List<BaseRecordDto>();
        //    ExplorerPagingInfo pagingInfo = null;
        //    //Dictionary<Guid, RMRule> daRulesDic = new Dictionary<Guid, RMRule>(); 
        //    bool hasNext;
        //    try
        //    {
        //        if (dto == null)
        //        {
        //            throw new Exception("query dto is null.");
        //        }
        //        else
        //        {
        //            var filterOption = dto.FilterOption;
        //            if (!string.IsNullOrEmpty(filterOption.NodeId))
        //            {
        //                var connObj = FSConnDao.GetConnectionById(new Guid(filterOption.NodeId));
        //                if (connObj != null)
        //                {
        //                    filterOption.NodeId = connObj.UNCPath.TrimEnd('\\').ToLowerInvariant().ToMd5().ToString();
        //                }
        //            }
        //            pagingInfo = dto.PagingInfo;
        //            if (dto.PagingInfo == null || dto.PagingInfo.PageIndex == "1")
        //            {
        //                //default setting
        //                pagingInfo = new ExplorerPagingInfo()
        //                {
        //                    PageIndex = string.Empty,
        //                    PageSize = 15
        //                };
        //            }
        //            var now = DateTime.UtcNow.Ticks;
        //            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
        //            string keywords = dto.FilterOption == null || dto.FilterOption.SearchOption == null ? null : dto.FilterOption.SearchOption.Key;
        //            Tuple<IEnumerable<Record>, string> queryData = null;
        //            using (new PerformanceScope("RecordsExplorer_QueryPageData"))
        //            {
        //                bool endUserSearch = false;


        //                if (RMSecurityTrimmingHelper.IsGlobalSecurityTrimmingEnabled())
        //                {
        //                    ReAssembleFourceFlags(dto);
        //                    if (isGlobalSearch && IsPhysicalEndUser() && dto.FilterOption.SourceFlags != null && dto.FilterOption.SourceFlags.Contains(SourceFlag.Physical))
        //                    {
        //                        endUserSearch = true;
        //                        dto.PermissionIds = GetPermissionCondition();
        //                    }

        //                    if (dto.FilterOption.SourceFlags == null || dto.FilterOption.SourceFlags.Count == 0)
        //                    {
        //                        logger.Warn("No permission to access data, source flags are removed.");
        //                        return new ExplorerResultInfo()
        //                        {
        //                            Datas = new List<BaseRecordDto>(),
        //                            PagingInfo = pagingInfo
        //                        };
        //                    }
        //                }

        //                if (string.IsNullOrEmpty(keywords) && !isGlobalSearch && !endUserSearch)
        //                {
        //                    Expression<Func<Record, bool>> whereLambda = GetFilterLambda(dto, true, true, isGlobalSearch, true, !isGlobalSearch);
        //                    if (whereLambda == null)
        //                    {
        //                        logger.Warn("No permission to access data.");
        //                        return new ExplorerResultInfo()
        //                        {
        //                            Datas = new List<BaseRecordDto>(),
        //                            PagingInfo = pagingInfo
        //                        };
        //                    }
        //                    logger.Debug(whereLambda.ToString());
        //                    queryData = ExplorerDao.QueryDataWithoutTotal(pagingInfo.PageIndex, pagingInfo.PageSize, out hasNext, whereLambda);
        //                }
        //                else
        //                {
        //                    logger.Info($"search by keywords : {keywords}");

        //                    queryData = ExplorerDao.QueryDataBySqlWithoutTotal(dto, isGlobalSearch, pagingInfo.PageIndex, pagingInfo.PageSize, out hasNext, GetSecurityTermDto());
        //                }

        //            }
        //            #region no use
        //            //if (queryData.Count == 0 && pagingInfo.PageIndex > 1)
        //            //{
        //            //    //REC-4364 极端情况下，此处可能浪费较长时间。
        //            //    //该处增加逻辑，当处理最后一页所有数据使其不满足Filter时，更新Grid会导致没有数据，且翻页控件隐藏，此时在后台主动将翻页-1，且更新前台self.currentPage
        //            //    pagingInfo.PageIndex--;
        //            //    queryData = ExplorerDao.QueryDataWithoutTotal(dto.IsArchived, keywords, pagingInfo.PageIndex, pagingInfo.PageSize, out hasNext, whereLambda);
        //            //}
        //            //该种情况并不是由REC-4364而引起的，是由于对Folder操作后，起Job在后台更新所导致
        //            //if (queryData.Count == 0 && pagingInfo.PageIndex > 1)
        //            //{
        //            //    //var totalCnt = ExplorerDao.QueryDataGetTotal(dto.IsArchived, keywords, whereLambda);
        //            //    //pagingInfo.Total = 0;
        //            //    //if (totalCnt == 0)
        //            //    //{
        //            //    //    pagingInfo.PageIndex = string.Empty;
        //            //    //}
        //            //    //else
        //            //    //{
        //            //    var lastPager = Math.Ceiling(Convert.ToDouble(totalCnt) / pagingInfo.PageSize);
        //            //    pagingInfo.PageIndex = (int)lastPager;
        //            //    //}
        //            //    queryData = ExplorerDao.QueryDataWithoutTotal(dto.IsArchived, keywords, pagingInfo.PageIndex, pagingInfo.PageSize, out hasNext, whereLambda);
        //            //}
        //            #endregion
        //            using (new PerformanceScope("RecordsExplorer_Convert"))
        //            {
        //                if (queryData.Item2 != null)
        //                {
        //                    pagingInfo.PageIndex = queryData.Item2;
        //                }
        //                var queryList = queryData.Item1.ToList();
        //                var scopeIds = queryList.Select(q => q.ScopeId).Distinct().ToList();
        //                var pathDic = RMScopeDao.GetScopeInfoByIds(scopeIds);
        //                List<RMRule> allRules = RMRuleDao.GetAvailableRules();

        //                GeneralSettingModel gls = GeneralSettingService.GetGeneralSetting();
        //                resultList = queryList.ConvertAll(e =>
        //                {
        //                    BaseRecordDto record = ConvertUtil.ConvertToBaseRecordDto(e, accountMap);

        //                    MakeSPObjectFullPath(pathDic, record);

        //                    if (!isGlobalSearch)
        //                    {
        //                        //no these columns in global search
        //                        SetSPObjectDisposalDueDate(now, gls, record);
        //                        //REC-3883
        //                        SetSPObjectReleaseTime(e, gls, record);
        //                    }

        //                    SetRuleInfos(record, allRules);

        //                    SetObjectType(record);

        //                    if (record.SourceFlag == (int)SourceFlag.Physical)
        //                    {
        //                        var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(e);
        //                        if (!isGlobalSearch)
        //                        {
        //                            SetPhysicalObjectHoldStatus(record, physicalObjectDto);
        //                        }
        //                        //if(record.RuleId == Guid.Empty)
        //                        //{
        //                        //    SetPhysicalRcordFile(gls, record, physicalObjectDto);
        //                        //}
        //                    }
        //                    return record;
        //                });
        //                //AppendPhyTermName(resultList);
        //                InheritRuleInfoFromParent(resultList, allRules);
        //                AppendTermInfoForRecordLevel(resultList);

        //            }
        //        }
        //        //REC-3551
        //        resultInfo.Datas = resultList;
        //        resultInfo.PagingInfo = pagingInfo;
        //        resultInfo.PagingInfo.HasNextPage = hasNext;
        //    }
        //    catch (Exception ex)
        //    {
        //        resultInfo.Datas = new List<BaseRecordDto>();
        //        logger.Error("error occurred while query data for explorer,ERROR:{0}", ex.ToString());
        //    }
        //    return resultInfo;
        //}



       

        private void SetPhysicalObjectHoldStatus(BaseRecordDto record, PhysicalObjectDto physicalObjectDto)
        {
            this.AppendPhyHoldInfo(physicalObjectDto);
            record.HoldStatus = physicalObjectDto.DisposalHold;
        }

        private void SetPhysicalObjectDisposalDueDateByCalculate(GeneralSettingModel gls, BaseRecordDto record, PhysicalObjectDto physicalObjectDto)
        {
            this.CalculateDisposalDueDateNormal(physicalObjectDto, gls, 0);
            if (physicalObjectDto.NodeType == RMNodeType.PhyFile && physicalObjectDto.BoxId != Guid.Empty
                && physicalObjectDto.DisposalHold == true && physicalObjectDto.HoldStatus == HoldStatus.Inherit)
            {
                Record box = ExplorerDao.GetPhysicalRecordById(physicalObjectDto.BoxId);
                this.CalculateDisposalDueDateNormal(physicalObjectDto, gls, box.DisposalDueDate);
            }
            record.DisposalDueDate = physicalObjectDto.DisposalDueDate;
        }

        private void SetPhysicalFileClassification(Record e, BaseRecordDto record)
        {
            try
            {
                string termName = GetPhysicalTermNameFromMetaInfo(e).Name;
                record.TermName = termName;
            }
            catch (Exception ex)
            {
                logger.Warn("get physical object classification error {0}", ex.ToString());
            }
        }

        private TaxonomyColumnValue GetPhysicalTermNameFromMetaInfo(Record record)
        {
            var termMataInfo = new PhysicalRecord(record)[MetaInfo.Classification];
            return JsonConvert.DeserializeObject<TaxonomyColumnValue>(termMataInfo);
        }

        private void SetRuleInfos(BaseRecordDto record, List<RMRule> rules = null)
        {
            if (record.RuleId != Guid.Empty)
            {
                RMRule rule = null;
                if (rules != null)
                {
                    rule = rules.FirstOrDefault(a => a.RuleId == record.RuleId);
                }
                else
                {

                    rule = RMRuleDao.GetRuleById(record.RuleId);
                }
                record.RuleName = rule?.RuleName;
                if (rule == null)
                {
                    record.DisposalAction = (int)RMContentDisposalAction.None;
                }
                else
                {
                    if (record.SourceFlag == (int)SourceFlag.Physical)
                    {
                        record.DisposalAction = (int)rule.PhysicalDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.FileSystem)
                    {
                        record.DisposalAction = (int)rule.FSDisposalAction;
                    }
                    else if (record.SourceFlag == (int)SourceFlag.Box)
                    {
                        record.DisposalAction = (int)rule.BoxDisposalAction;
                    }
                    else
                    {
                        record.DisposalAction = (int)rule.DisposalAction;
                    }
                }

                record.ExchangeDisposalAction = rule == null ? (int)RMContentDisposalAction.None : rule.ExchangeDisposalAction;
            }
            else
            {
                record.DisposalAction = (int)RMContentDisposalAction.None;
                record.ExchangeDisposalAction = (int)RMContentDisposalAction.None;
            }
        }

        private void SetPhysicalRcordFile(GeneralSettingModel gls, BaseRecordDto record, PhysicalObjectDto physicalObjectDto)
        {
            try
            {
                if (physicalObjectDto.NodeType == RMNodeType.PhyRecord)
                {
                    List<Guid> parentIds = new List<Guid>() { physicalObjectDto.FileId, physicalObjectDto.BoxId };
                    List<Record> parentRecs = ExplorerDao.QueryAll(a => parentIds.Contains(a.Id) && a.ScopeId == Guid.Empty).OrderBy(a => a.NodeType).ToList();
                    Record file = parentRecs.FirstOrDefault(a => a.NodeType == (int)RMNodeType.PhyFile);
                    if (file != null && file.RuleId != Guid.Empty)
                    {
                        physicalObjectDto.RuleId = file.RuleId;
                        if (gls != null && file.DisposalDueDate > DateTime.MinValue.Ticks)
                        {
                            record.DisposalDueDate = this.GetDisposalDueDateStr(file.DisposalDueDate, (RMRecordStatus)file.RecordStatus, gls, false);
                        }
                        this.AppendPhysicalRuleAction(physicalObjectDto, RMRuleDao.GetRuleById(file.RuleId));
                        this.SetPhysicalFileClassification(file, record);
                    }
                    else
                    {
                        Record box = parentRecs.FirstOrDefault(a => a.NodeType == (int)RMNodeType.PhyBox);
                        if (box != null && box.RuleId != Guid.Empty)
                        {
                            physicalObjectDto.RuleId = box.RuleId;
                            if (gls != null && box.DisposalDueDate > DateTime.MinValue.Ticks)
                            {
                                record.DisposalDueDate = this.GetDisposalDueDateStr(box.DisposalDueDate, (RMRecordStatus)box.RecordStatus, gls, false);
                            }
                            this.AppendPhysicalRuleAction(physicalObjectDto, RMRuleDao.GetRuleById(box.RuleId));
                            this.SetPhysicalFileClassification(box, record);
                        }
                    }
                    if (file != null)
                    {
                        var fileTerm = GetPhysicalTermNameFromMetaInfo(file);
                        var classifyField = new TaxonomyColumnValue() { Id = fileTerm.Id, Name = fileTerm.Name };
                        physicalObjectDto.MetaInfo[MetaInfo.Classification] = JsonConvert.SerializeObject(classifyField);
                    }
                }
                else if (physicalObjectDto.RuleId != Guid.Empty)
                {
                    this.AppendPhysicalRuleAction(physicalObjectDto, RMRuleDao.GetRuleById(physicalObjectDto.RuleId));
                }
                record.RuleId = physicalObjectDto.RuleId;
                record.DisposalAction = physicalObjectDto.RuleAction;
                record.MetaInfo = JsonConvert.SerializeObject(physicalObjectDto.MetaInfo);
            }
            catch (Exception e)
            {
                logger.Warn("set physical record file error: {0}", e.ToString());
            }
        }

        private void SetObjectType(BaseRecordDto record)
        {
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_FileNull")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FileNull");
            }
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPItem");
            }
            if (record.NodeType == (int)NodeLevel.FSFolder && string.IsNullOrEmpty(record.ExtensionForFile))
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FSFolder");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalBox");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalFile");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalRecord)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_Filter_PhysicalRecord");
            }
            if (record.NodeType == (int)RMNodeLevel.PhysicalCustom)
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_PRM_PRE_TableItemType_Container");
            }
        }

        private void SetSPObjectDisposalDueDate(long now, GeneralSettingModel gls, BaseRecordDto record)
        {
            if (record != null && !string.IsNullOrEmpty(record.DisposalDueDate))
            {
                long tempTicks;
                if (long.TryParse(record.DisposalDueDate, out tempTicks))
                {
                    var minDate = DateTime.MinValue;
                    if (tempTicks > minDate.Ticks)
                    {
                        //if (tempTicks > now)
                        //{
                        //    record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                        //}
                        //else
                        //{
                        //    record.DisposalDueDate = I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
                        //}
                        record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                    }
                }
                else
                {
                    record.DisposalDueDate = I18NEntity.GetString(record.DisposalDueDate);
                }
            }
        }

        private void SetSPObjectReleaseTime(Record e, GeneralSettingModel gls, BaseRecordDto record)
        {
            if (record.HoldStatus)
            {
                //record.DisposalDueDate = string.Empty;    //RECO-2607, Hold操作会真实处理disposalDueDate字段, 不需要控制显示\隐藏该字段信息
                record.ReleaseTime = mGeneralSettingService.ConvertTiksToDateTime(gls, e.HoldReleaseTime, true).SimplifyFormatTime;
            }
            else
            {
                record.ReleaseTime = string.Empty;
            }
        }



        //public int QueryDataListGetTotal(ExplorerQueryDto dto)
        //{
        //    ExplorerPagingInfo pagingInfo = null;
        //    int totalCount = 0;
        //    try
        //    {
        //        if (dto == null)
        //        {
        //            throw new Exception("query dto is null.");
        //        }
        //        else
        //        {
        //            var filterOption = dto.FilterOption;
        //            pagingInfo = dto.PagingInfo;

        //            if (dto.PagingInfo == null)
        //            {
        //                //default setting
        //                pagingInfo = new ExplorerPagingInfo()
        //                {
        //                    PageIndex = string.Empty,
        //                    PageSize = 15
        //                };
        //            }
        //            Expression<Func<Record, bool>> whereLambda = GetFilterLambda(dto, true, true, false, true);
        //            string keywords = dto.FilterOption == null || dto.FilterOption.SearchOption == null ? null : dto.FilterOption.SearchOption.Key;
        //            //totalCount = ExplorerDao.QueryDataGetTotal(dto.IsArchived, keywords, whereLambda);
        //        }
        //        //REC-3551

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("error occurred while query data for explorer,ERROR:{0}", ex.ToString());
        //    }
        //    return totalCount;
        //}
        public async Task<ExplorerResultInfo> GetRecordbyHoldIdAsync(ExplorerSetHoldDto dto)
        {
            ExplorerResultInfo info = new ExplorerResultInfo();
            List<BaseRecordDto> resultList = new List<BaseRecordDto>();
            ExplorerPagingInfo pageInfo = null;
            try
            {
                if (dto == null)
                {
                    throw new Exception("query dto is null.");
                }

                int totalCount = 0;
                pageInfo = dto.PagingInfo;
                string holdId = dto.holdId;
                if (pageInfo == null)
                {
                    //default setting
                    pageInfo = new ExplorerPagingInfo()
                    {
                        PageIndex = string.Empty,
                        PageSize = 5
                    };
                }
                //List<RMBaseRecord> baseRecords = ExplorerDao.QueryDataById(pageInfo.PageIndex, pageInfo.PageSize, out totalCount, holdId);
                List<Guid> recordIds = new List<Guid>();
                List<Record> baseRecords = ExplorerDao.GetRecordbyHoldId(holdId);
                var SPOAdmin = (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin)) || (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.ManageHoldEndUser)) || (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold));
                if (!SPOAdmin)
                {
                    baseRecords = baseRecords.Where(r => r.SourceFlag == (int)SourceFlag.Physical).ToList();
                }
                totalCount = baseRecords.Count;
                baseRecords = baseRecords.Skip((int.Parse(pageInfo.PageIndex) - 1) * pageInfo.PageSize).Take(pageInfo.PageSize).ToList();
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();              
                foreach (Record baseRecord in baseRecords)
                {
                    BaseRecordDto recordDto = ConvertUtil.ConvertToBaseRecordDto(baseRecord);
                    recordDto.ReleaseTime = mGeneralSettingService.ConvertTiksToDateTime(gls, recordDto.HoldReleaseTime, true).SimplifyFormatTime;
                    resultList.Add(recordDto);
                }             
                pageInfo.Total = totalCount;

                info.Datas = resultList;
                info.PagingInfo = pageInfo;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while query data for explorer,ERROR:{0}", ex.ToString());
            }

            return info;
        }

        public async Task<RAReturnMessage> RunExportHoldRecordsJobAsync(JobRunBy jobRunBy, List<string> holdIds)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportHoldRecords,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(holdIds),
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunExportHoldRecords,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }

            return returnMessage;
        }

        public async Task<RAReturnMessage> RunImportHoldRecordsJobAsync(JobRunBy jobRunBy, string blobName)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportHoldRecords,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = blobName,
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportHoldRecords,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ManageHold, Action = AuditAction.ImportHoldRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> RealRunImportHoldRecordsJobAsync(string blobName)
        {
            logger.Info("RealRunImportHoldRecordsJobAsync start.");
            string jobId = string.Empty;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                jobId = RMJobService.CreateJob(JobType.ImportHoldRecords, TenantLocalValue.LogonUserEmail, account.UserId);

                List<string> runningJobs = JobMonitorService.GetRunningJobs(JobType.ImportHoldRecords);
                bool isSkip = runningJobs.Any(j => j != jobId);

                if (!isSkip)
                {
                    SubJobDao.UpdateSubJobCount(jobId, 1);

                    var subJobId = CreateSubJob(jobId, 0, JobType.ImportHoldRecords, JobStatus.InProgress, 1, blobName);

                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = JobType.ImportHoldRecords,
                        CommandLine = string.Format("{0} {1} {2} {3}", JobType.ImportHoldRecords, subJobId, jobId, blobName)
                    });
                }
                else
                {
                    logger.Info(I18NEntity.GetString("RM_SYNC_JobSkip"));
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }

                logger.Info($"RealRunImportHoldRecordsJobAsync end. JobId: {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunImportHoldRecordsJobAsync, reason: {ex}.");
            }
            return jobId;
        }

        public async Task<ArchivedContentResultInfo> LoadDownloadArchivedContentAsync(ArchivedContentSearchInfo searchInfo)
        {
            ArchivedContentResultInfo info = new ArchivedContentResultInfo();
            List<ArchivedContentDto> resultList = new List<ArchivedContentDto>();
            ExplorerPagingInfo pageInfo = null;
            try
            {
                logger.Info($"Begin to load archived content, search key:{searchInfo?.SearchKey}");
                int totalCount = 0;
                if (searchInfo == null)
                {
                    throw new Exception("search dto is null.");
                }
                pageInfo = searchInfo.PagingInfo;
                if (pageInfo == null)
                {
                    //default setting
                    pageInfo = new ExplorerPagingInfo()
                    {
                        PageIndex = string.Empty,
                        PageSize = 5
                    };
                }

                List<RMDownloadDataInfo> downloadDataInfos = DownloadDataInfoDao.QueryDownloadDataInfoById(searchInfo.SearchKey, int.Parse(pageInfo.PageIndex), pageInfo.PageSize, out totalCount);
                Dictionary<Guid, Record> recDic = new Dictionary<Guid, Record>();
                ArgumentCheck.NotNull(downloadDataInfos, nameof(downloadDataInfos));
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var recordIds = downloadDataInfos.Select(r => r.RecordsId).ToList();
                var baseRecords = new List<Record>();
                if (LicenseHelperService.HasOpusILLicense)
                {
                    baseRecords = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                }
                logger.Info($"Archived content info:{downloadDataInfos?.Count} Records count:{baseRecords?.Count}");
                Dictionary<Guid, RMScope> scopes = RMScopeDao.GetScopeInfoByIds(baseRecords.Select(r => r.ScopeId).Distinct().ToList());

                var exportJobTypes = new DownloadContentType[] {
                    DownloadContentType.HistoryContent, DownloadContentType.LoanPickListContent, DownloadContentType.DestructionPickListContent
                    ,DownloadContentType.ReportContent,DownloadContentType.UnderReviewContent,DownloadContentType.WaitingForDisposalContent
                    ,DownloadContentType.DisposalExtendContent , DownloadContentType.RelatedRecordsContent, DownloadContentType.ExportTermStructure
                    ,DownloadContentType.ExportSearchRecords , DownloadContentType.ExportDiscoveryProfile
                    ,DownloadContentType.PhysicalBuklExport, DownloadContentType.JobReportContent,DownloadContentType.MachineLearningExportReport
                    ,DownloadContentType.ExportSiteMetrics,DownloadContentType.ExportSettings,DownloadContentType.ExportIndex,DownloadContentType.Others
                    ,DownloadContentType.DiscoveryExportRowDataJob, DownloadContentType.ReturnLoanHistory, DownloadContentType.ExportConflictSettingDetail
                    ,DownloadContentType.ExportRestoreCenterSeachResult,DownloadContentType.ExportDeduplicationReport, DownloadContentType.ExportSCMapping, DownloadContentType.ExportSCWhitelist,
                    DownloadContentType.ExportSCBlacklist, DownloadContentType.ExportSPSOSetting, DownloadContentType.ExportTeamsSOSetting,  DownloadContentType.DiscoveryExportDuplicationReport,
                    DownloadContentType.DownloadRCCReport, DownloadContentType.ExportHoldRecords, DownloadContentType.DiscoveryExportExcludeList, DownloadContentType.SharePointSiteMetricsReport, DownloadContentType.MovePickListContent
                };
                foreach (var contentInfo in downloadDataInfos)
                {
                    if (contentInfo.DownloadType == DownloadContentType.JobReportContentForCOP)
                    {
                        // internal COP job report download; hide from end-user explorer
                        continue;
                    }
                    var record = baseRecords.Where(r => r.Id == contentInfo.RecordsId).FirstOrDefault();
                    if (record == null && !exportJobTypes.Contains(contentInfo.DownloadType))
                    {
                        logger.Warn($"Cannot find related record in db, id:{contentInfo.RecordsId}");
                        continue;
                    }
                    ArchivedContentDto recordDto = new ArchivedContentDto()
                    {
                        RecordId = contentInfo.RecordsId,
                        Name = contentInfo.Name,
                        SourceFlag = exportJobTypes.Contains(contentInfo.DownloadType) || record == null ? (int)SourceFlag.All : record.SourceFlag,
                        FileType = exportJobTypes.Contains(contentInfo.DownloadType) ? ArchivedContentFileType.Zip : ArchivedContentFileType.None,
                        DJobId = contentInfo.JobId,
                        JobId = contentInfo.JobId
                    };

                    if (contentInfo.DownloadType is DownloadContentType.JobReportContent && !string.IsNullOrEmpty(contentInfo.ExtendString1))
                    {
                        recordDto.JobId = contentInfo.ExtendString1;
                    }

                    var fileDownloadTime = contentInfo.FileDownloadTime;
                    recordDto.DownloadTime = mGeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                    recordDto.FullPath = exportJobTypes.Contains(contentInfo.DownloadType) ? contentInfo.Name : GetFullPath(record, scopes);
                    if (contentInfo.JobStatus == (int)DownloadContentJobStatus.Finished || contentInfo.JobStatus == (int)DownloadContentJobStatus.Wait || contentInfo.JobStatus == (int)DownloadContentJobStatus.InProgress)
                    {
                        recordDto.JobStatus = contentInfo.JobStatus;
                    }
                    else
                    {
                        recordDto.JobStatus = (int)DownloadContentJobStatus.Failed;
                    }

                    long fileSize = contentInfo.FileSize.HasValue ? contentInfo.FileSize.GetValueOrDefault() : long.MinValue;
                    if(fileSize == long.MinValue)
                    {
                        recordDto.FileSize = "N/A";
                    }
                    else
                    {
                        recordDto.FileSize = ConvertToFormatSizeWithoutBytes(fileSize);
                    }
                    switch(contentInfo.DownloadType)
                    {
                        case DownloadContentType.ArchivedContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ArchivedContent");
                            break;
                        case DownloadContentType.HistoryContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_HistoryContent");
                            break;
                        case DownloadContentType.UnderReviewContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportRecordsForReviewDatasJob");
                            break;
                        case DownloadContentType.WaitingForDisposalContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportRecordsForReviewDatasJob");
                            break;
                        case DownloadContentType.DisposalExtendContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportRecordsForReviewDatasJob");
                            break;
                        case DownloadContentType.RelatedRecordsContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ManualExportRecordsForReviewDatasJob");
                            break;
                        case DownloadContentType.ExportTermStructure:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ExportTermStructure");
                            break;
                        case DownloadContentType.LoanPickListContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_LoanPickListContent");
                            break;
                        case DownloadContentType.DestructionPickListContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_DestructionPickListContent");
                            break;
                        case DownloadContentType.ReportContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ReportContent");
                            break;
                        case DownloadContentType.ReturnLoanHistory:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ReturnHistory");
                            break;
                        case DownloadContentType.MovePickListContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_MovePick");
                            break;
                        case DownloadContentType.ExportConflictSettingDetail:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ConflictSettingDetailExport");
                            break;
                        case DownloadContentType.JobReportContent:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_JobReportContent");
                            break;
                        case DownloadContentType.ExportSCMapping:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSCMapping");
                            break;
                        case DownloadContentType.ExportSCWhitelist:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSCWhitelist");
                            break;
                        case DownloadContentType.DiscoveryExportExcludeList:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryExportExcludeSCList");
                            break;
                        case DownloadContentType.ExportSCBlacklist:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSCBlacklist");
                            break;
                        case DownloadContentType.PhysicalBuklExport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_PhysicalBuklExport");
                            break;
                        case DownloadContentType.MachineLearningExportReport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_MachineLearningExportReport");
                            break;
                        case DownloadContentType.ExportSiteMetrics:
                            recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_ExportSiteMetrics");
                            break;
                        case DownloadContentType.ExportSettings:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSPSetting");
                            break;
                        case DownloadContentType.ExportIndex:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportIndex");
                            break;
                        case DownloadContentType.ExportSearchRecords:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSearchRecords");
                            break;
                        case DownloadContentType.ExportDiscoveryProfile:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_Action_ExportDiscoveryProfile");
                            break;
                        case DownloadContentType.DiscoveryExportRowDataJob:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryExportRowDataJob");
                            break;
                        case DownloadContentType.ExportRestoreCenterSeachResult:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportRestoreCenterSeachResult");
                            break;
                        case DownloadContentType.ExportDeduplicationReport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ArchiverDeduplicationReport");
                            break;
                        case DownloadContentType.ExportTeamsSOSetting:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportTeamsSOSetting");
                            break;
                        case DownloadContentType.ExportSPSOSetting:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_ExportSPSOSetting");
                            break;
                        case DownloadContentType.DiscoveryExportDuplicationReport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryExportDuplicationReport");
                            break;
                        case DownloadContentType.DownloadRCCReport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_FS_DownloadRCCReport");
                            break;
                        case DownloadContentType.ExportHoldRecords:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_HoldsRecordsExportJob");
                            break;
                        case DownloadContentType.SharePointSiteMetricsReport:
                            recordDto.DownloadType = I18NEntity.GetString("RM_JS_JM_JobType_SharePointReportExport");
                            break;
                        default:
                            recordDto.DownloadType = "N/A";
                            break;
                    }
                    
                    recordDto.SasUri = contentInfo.BlobSasUri;
                    resultList.Add(recordDto);
                }
                //foreach (Record baseRecord in baseRecords)
                //{
                //    if (!recDic.ContainsKey(baseRecord.Id))
                //    {
                //        logger.Info($"Download content not found, id:{baseRecord.Id}");
                //        continue;
                //    }
                //    ArchivedContentDto recordDto = new ArchivedContentDto()
                //    {
                //        RecordId = baseRecord.Id,
                //        JobStatus = recDic[baseRecord.Id].JobStatus,
                //        Name = baseRecord.LeafName,
                //        SourceFlag = baseRecord.SourceFlag
                //    };
                //    var fileDownloadTime = recDic[baseRecord.Id].FileDownloadTime;
                //    recordDto.DownloadTime = mGeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                //    recordDto.FullPath = GetFullPath(baseRecord, scopes);
                //    resultList.Add(recordDto);
                //}
                pageInfo.Total = totalCount;

                info.Datas = resultList;
                info.PagingInfo = pageInfo;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while query archived content for explorer,ERROR:{0}", ex.ToString());
            }
            return info;
        }

        public async Task<RMRCCReportResult> LoadRCCInfoByIdAsync(RMRCCReportInfo requestInfo, string timeZoneId, bool isDaylight)
        {
            RMRCCReportResult info = new RMRCCReportResult();
            List<RCCReportContentDto> resultList = new List<RCCReportContentDto>();
            RCCPagingInfo pageInfo = null;
            try
            {
                logger.Info($"Begin to load RCC report archived content");
                int totalCount = 0;
                if (requestInfo == null)
                {
                    throw new Exception("search dto is null.");
                }
                pageInfo = requestInfo.PagingInfo;
                if (pageInfo == null || string.IsNullOrEmpty(pageInfo.PageIndex))
                {
                    pageInfo = new RCCPagingInfo()
                    {
                        PageIndex = "1",
                        PageSize = 30
                    };
                }

                var jobIds = new List<string>();
                var downloadDataInfos = new List<RMDownloadDataInfo>();
                var rccJobQueues = string.Empty;
                var loginName = TenantLocalValue.LogonUserEmail;

                int parsedPageIndex = 1;
                int.TryParse(pageInfo.PageIndex, out parsedPageIndex);
                if (parsedPageIndex <= 0) parsedPageIndex = 1;

                if (requestInfo.Ids.Count > 0)
                {
                    if (parsedPageIndex == 1) rccJobQueues = await JobQueueService.GetRCCDBJobQueueByLoginNameAsync(loginName, requestInfo.Ids);
                    downloadDataInfos = DownloadDataInfoDao.QueryDownloadReportInfoByScopeIds(
                        requestInfo.Ids,
                        (int)MyhubReportJobType.DownloadRCCReport,
                        parsedPageIndex,
                        pageInfo.PageSize,
                        out totalCount,
                        requestInfo.OrderBy,
                        requestInfo.IsDesc
                    );
                }
                else
                {
                    if (parsedPageIndex == 1) rccJobQueues = await JobQueueService.GetAllDBJobQueueByLoginNameAsync(loginName, (int)JobType.DownloadRCCReport);
                    downloadDataInfos = DownloadDataInfoDao.QueryAllDownloadReportInfo(
                        (int)DownloadContentType.DownloadRCCReport,
                        parsedPageIndex,
                        pageInfo.PageSize,
                        out totalCount,
                        requestInfo.OrderBy,
                        requestInfo.IsDesc
                    );
                }

                if (downloadDataInfos == null)
                {
                    logger.Info($"No archived content found for RCC report, jobIds:{string.Join(",", jobIds)}");
                    info.Datas = resultList;
                    info.PagingInfo = pageInfo;
                    return info;
                }
                else if (!string.IsNullOrWhiteSpace(rccJobQueues) && (JsonConvert.DeserializeObject<List<RMJobQueue>>(rccJobQueues)?.Count ?? 0) > 0)
                {
                    info.IsInProgress = true;
                }
                else
                {
                    var foundJobIds = downloadDataInfos.Select(d => d.JobId).Distinct().ToList();
                    info.IsInProgress = DownloadDataInfoDao.IsHasInprogressRCCReport(foundJobIds);
                }

                Dictionary<Guid, Record> recDic = new Dictionary<Guid, Record>();
                ArgumentCheck.NotNull(downloadDataInfos, nameof(downloadDataInfos));
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
                var recordIds = downloadDataInfos.Select(r => r.RecordsId).ToList();
                var baseRecords = new List<Record>();
                if (LicenseHelperService.HasOpusILLicense)
                {
                    baseRecords = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                }
                logger.Info($"Archived content info:{downloadDataInfos?.Count} Records count:{baseRecords?.Count}");
                Dictionary<Guid, RMScope> scopes = RMScopeDao.GetScopeInfoByIds(baseRecords.Select(r => r.ScopeId).Distinct().ToList());

                var exportJobTypes = new DownloadContentType[] {
                    DownloadContentType.DownloadRCCReport
                };

                if (!string.IsNullOrEmpty(rccJobQueues))
                {
                    var jobQueueList = JsonConvert.DeserializeObject<List<RMJobQueue>>(rccJobQueues);
                    if (jobQueueList != null && jobQueueList.Count > 0)
                    {
                        foreach (var jobQueue in jobQueueList)
                        {
                            ArchivedContentDto recordJobQueueDto = new ArchivedContentDto()
                            {
                                Name = I18NEntity.GetString("RM_FS_DownloadRCCReport"),
                                JobStatus = (int)DownloadContentJobStatus.Wait,
                                JobId = jobQueue.MessageId
                            };

                            var rccReportContent = new RCCReportContentDto()
                            {
                                ContentDto = recordJobQueueDto,
                            };

                            if (!string.IsNullOrEmpty(jobQueue.Parameters))
                            {
                                try
                                {
                                    var paramObj = JsonConvert.DeserializeObject<RCCReportRequest>(jobQueue.Parameters);
                                    if (paramObj != null)
                                    {
                                        rccReportContent.TimeRange = paramObj.TimeRange;
                                        rccReportContent.EndDateWithin = HandleTimeRange(paramObj.TimeRange, gls, timeZoneId, isDaylight);
                                        rccReportContent.DisplayName = paramObj.DisplayName;
                                        //rccReportContent.NodeId = paramObj.Node?.Select(n => n.Id.ToString()).ToList() ?? new List<string>();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn($"Failed to parse job queue parameters: {ex.Message}");
                                }
                            }

                            resultList.Add(rccReportContent);
                        }
                    }
                }

                foreach (var contentInfo in downloadDataInfos)
                {
                    var record = baseRecords.Where(r => r.Id == contentInfo.RecordsId).FirstOrDefault();
                    if (record == null && !exportJobTypes.Contains(contentInfo.DownloadType))
                    {
                        logger.Warn($"Cannot find related record in db, id:{contentInfo.RecordsId}");
                        continue;
                    }
                    ArchivedContentDto recordDto = new ArchivedContentDto()
                    {
                        RecordId = contentInfo.RecordsId,
                        Name = contentInfo.Name,
                        SourceFlag = exportJobTypes.Contains(contentInfo.DownloadType) || record == null ? (int)SourceFlag.All : record.SourceFlag,
                        FileType = exportJobTypes.Contains(contentInfo.DownloadType) ? ArchivedContentFileType.Zip : ArchivedContentFileType.None,
                        DJobId = contentInfo.JobId,
                        JobId = contentInfo.JobId
                    };

                    if (contentInfo.DownloadType is DownloadContentType.JobReportContent && !string.IsNullOrEmpty(contentInfo.ExtendString1))
                    {
                        recordDto.JobId = contentInfo.ExtendString1;
                    }

                    var fileDownloadTime = contentInfo.FileDownloadTime;
                    //recordDto.DownloadTime = mGeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                    if (!string.IsNullOrEmpty(timeZoneId))
                    {
                        recordDto.DownloadTime = GeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
                    }
                    else
                    {
                        recordDto.DownloadTime = GeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                    }
                    recordDto.FullPath = exportJobTypes.Contains(contentInfo.DownloadType) ? contentInfo.Name : GetFullPath(record, scopes);
                    if (contentInfo.JobStatus == (int)DownloadContentJobStatus.Finished || contentInfo.JobStatus == (int)DownloadContentJobStatus.Wait || contentInfo.JobStatus == (int)DownloadContentJobStatus.InProgress)
                    {
                        recordDto.JobStatus = contentInfo.JobStatus;
                    }
                    else
                    {
                        recordDto.JobStatus = (int)DownloadContentJobStatus.Failed;
                    }

                    long fileSize = contentInfo.FileSize.HasValue ? contentInfo.FileSize.GetValueOrDefault() : long.MinValue;
                    if (fileSize == long.MinValue)
                    {
                        recordDto.FileSize = "N/A";
                    }
                    else
                    {
                        recordDto.FileSize = ConvertToFormatSizeWithoutBytes(fileSize);
                    }
                    recordDto.DownloadType = I18NEntity.GetString("RM_FS_DownloadRCCReport");

                    recordDto.SasUri = contentInfo.BlobSasUri;

                    var rccReportContent = JsonConvert.DeserializeObject<List<RCCReportContentDto>>(contentInfo.ExtendString1 ?? string.Empty).FirstOrDefault() ?? new RCCReportContentDto();

                    rccReportContent.DisplayName = !string.IsNullOrEmpty(rccReportContent.DisplayName) ? rccReportContent.DisplayName : recordDto.Name;
                    rccReportContent.ContentDto = recordDto;
                    rccReportContent.EndDateWithin = HandleTimeRange(rccReportContent.TimeRange, gls, timeZoneId, isDaylight);

                    resultList.Add(rccReportContent);
                }

                pageInfo.HasNextPage = (parsedPageIndex * pageInfo.PageSize) < totalCount;
                pageInfo.Total = totalCount;
                info.Datas = resultList;
                info.PagingInfo = pageInfo;
                info.IsEnableMultiGeo = await MultiGeoSettingService.IsEnableMultiGeoFeature();
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while query archived content for explorer,ERROR:{0}", ex.ToString());
            }
            return info;
        }

        public async Task<RMDisposalHistoryReportResult> LoadDisposalHistoryReportAsync(RMDisposalHistoryReportInfo requestInfo, string timeZoneId, bool isDaylight)
        {
            RMDisposalHistoryReportResult info = new RMDisposalHistoryReportResult();
            List<DisposalHistoryReportContentDto> resultList = new List<DisposalHistoryReportContentDto>();
            DisposalHistoryPagingInfo pageInfo = null;
            try
            {
                logger.Info($"Begin to load Disposal History report archived content");
                if (requestInfo == null)
                {
                    throw new Exception("search dto is null.");
                }
                pageInfo = requestInfo.PagingInfo;
                int totalCount = 0;
                if (pageInfo == null || string.IsNullOrEmpty(pageInfo.PageIndex))
                {
                    pageInfo = new DisposalHistoryPagingInfo()
                    {
                        PageIndex = "1",
                        PageSize = 30
                    };
                }

                var jobIds = new List<string>();
                var downloadDataInfos = new List<RMDownloadDataInfo>();
                var rccJobQueues = string.Empty;
                var loginName = TenantLocalValue.LogonUserEmail;

                int parsedPageIndex = 1;
                int.TryParse(pageInfo.PageIndex, out parsedPageIndex);
                if (parsedPageIndex <= 0) parsedPageIndex = 1;

                if (requestInfo.Id != null)
                {
                    if (parsedPageIndex == 1) rccJobQueues = await JobQueueService.GetDisposalHistoryDBJobQueueByLoginNameAsync(loginName, requestInfo.Id);
                    downloadDataInfos = DownloadDataInfoDao.QueryDownloadReportInfoByScopeIds(
                        new List<string> { requestInfo.Id },
                        (int)MyhubReportJobType.HistoryContent,
                        parsedPageIndex,
                        pageInfo.PageSize,
                        out totalCount,
                        requestInfo.OrderBy,
                        requestInfo.IsDesc
                    );
                }
                else
                {
                    if (parsedPageIndex == 1) rccJobQueues = await JobQueueService.GetAllDBJobQueueByLoginNameAsync(loginName, (int)JobType.ManualExportHistoryDatasJob);
                    downloadDataInfos = DownloadDataInfoDao.QueryAllDownloadReportInfo(
                        (int)DownloadContentType.HistoryContent,
                        parsedPageIndex,
                        pageInfo.PageSize,
                        out totalCount,
                        requestInfo.OrderBy,
                        requestInfo.IsDesc
                    );
                }

                if (downloadDataInfos == null)
                {
                    logger.Info($"No archived content found for Disposal history report, jobIds:{string.Join(",", jobIds)}");
                    info.Datas = resultList;
                    info.PagingInfo = pageInfo;
                    return info;
                }
                else if (!string.IsNullOrWhiteSpace(rccJobQueues) && (JsonConvert.DeserializeObject<List<RMJobQueue>>(rccJobQueues)?.Count ?? 0) > 0)
                {
                    info.IsInProgress = true;
                }
                else
                {
                    var foundJobIds = downloadDataInfos.Select(d => d.JobId).Distinct().ToList();
                    info.IsInProgress = DownloadDataInfoDao.IsHasInprogressRCCReport(foundJobIds);
                }

                Dictionary<Guid, Record> recDic = new Dictionary<Guid, Record>();
                ArgumentCheck.NotNull(downloadDataInfos, nameof(downloadDataInfos));
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
                var recordIds = downloadDataInfos.Select(r => r.RecordsId).ToList();
                var baseRecords = new List<Record>();
                if (LicenseHelperService.HasOpusILLicense)
                {
                    baseRecords = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                }
                logger.Info($"Archived content info:{downloadDataInfos?.Count} Records count:{baseRecords?.Count}");
                Dictionary<Guid, RMScope> scopes = RMScopeDao.GetScopeInfoByIds(baseRecords.Select(r => r.ScopeId).Distinct().ToList());

                var exportJobTypes = new DownloadContentType[] {
                    DownloadContentType.HistoryContent
                };

                if (!string.IsNullOrEmpty(rccJobQueues))
                {
                    var jobQueueList = JsonConvert.DeserializeObject<List<RMJobQueue>>(rccJobQueues);
                    if (jobQueueList != null && jobQueueList.Count > 0)
                    {
                        foreach (var jobQueue in jobQueueList)
                        {
                            ArchivedContentDto recordJobQueueDto = new ArchivedContentDto()
                            {
                                Name = I18NEntity.GetString("RM_DC_DownloadType_HistoryContent"),
                                JobStatus = (int)DownloadContentJobStatus.Wait,
                                JobId = jobQueue.MessageId
                            };

                            var rccReportContent = new DisposalHistoryReportContentDto()
                            {
                                ContentDto = recordJobQueueDto,
                            };

                            if (!string.IsNullOrEmpty(jobQueue.Parameters))
                            {
                                try
                                {
                                    var paramObj = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(jobQueue.Parameters);
                                    if (paramObj != null)
                                    {
                                        rccReportContent.TimeRange = paramObj.CustomDate;
                                        rccReportContent.EndDateWithin = HandleDisposalHistoryTimeRange(paramObj.CustomDate, paramObj.LatestExportType, gls, timeZoneId, isDaylight);
                                        rccReportContent.DisplayName = paramObj.DisplayName;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn($"Failed to parse job queue parameters: {ex.Message}");
                                }
                            }

                            resultList.Add(rccReportContent);
                        }
                    }
                }

                foreach (var contentInfo in downloadDataInfos)
                {
                    var record = baseRecords.Where(r => r.Id == contentInfo.RecordsId).FirstOrDefault();
                    if (record == null && !exportJobTypes.Contains(contentInfo.DownloadType))
                    {
                        logger.Warn($"Cannot find related record in db, id:{contentInfo.RecordsId}");
                        continue;
                    }
                    ArchivedContentDto recordDto = new ArchivedContentDto()
                    {
                        RecordId = contentInfo.RecordsId,
                        Name = contentInfo.Name,
                        SourceFlag = exportJobTypes.Contains(contentInfo.DownloadType) || record == null ? (int)SourceFlag.All : record.SourceFlag,
                        FileType = exportJobTypes.Contains(contentInfo.DownloadType) ? ArchivedContentFileType.Zip : ArchivedContentFileType.None,
                        DJobId = contentInfo.JobId,
                        JobId = contentInfo.JobId
                    };

                    if (contentInfo.DownloadType is DownloadContentType.JobReportContent && !string.IsNullOrEmpty(contentInfo.ExtendString1))
                    {
                        recordDto.JobId = contentInfo.ExtendString1;
                    }

                    var fileDownloadTime = contentInfo.FileDownloadTime;
                    //recordDto.DownloadTime = mGeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                    if (!string.IsNullOrEmpty(timeZoneId))
                    {
                        recordDto.DownloadTime = GeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
                    }
                    else
                    {
                        recordDto.DownloadTime = GeneralSettingService.ConvertTiksToDateTime(gls, fileDownloadTime, true).SimplifyFormatTime;
                    }
                    recordDto.FullPath = exportJobTypes.Contains(contentInfo.DownloadType) ? contentInfo.Name : GetFullPath(record, scopes);
                    if (contentInfo.JobStatus == (int)DownloadContentJobStatus.Finished || contentInfo.JobStatus == (int)DownloadContentJobStatus.Wait || contentInfo.JobStatus == (int)DownloadContentJobStatus.InProgress)
                    {
                        recordDto.JobStatus = contentInfo.JobStatus;
                    }
                    else
                    {
                        recordDto.JobStatus = (int)DownloadContentJobStatus.Failed;
                    }

                    long fileSize = contentInfo.FileSize.HasValue ? contentInfo.FileSize.GetValueOrDefault() : long.MinValue;
                    if (fileSize == long.MinValue)
                    {
                        recordDto.FileSize = "N/A";
                    }
                    else
                    {
                        recordDto.FileSize = ConvertToFormatSizeWithoutBytes(fileSize);
                    }

                    var historyNode = JsonConvert.DeserializeObject<ManualApprovalHistoryOption>(contentInfo.ExtendString1 ?? string.Empty) ?? new ManualApprovalHistoryOption();
                    recordDto.DownloadType = I18NEntity.GetString("RM_DC_DownloadType_HistoryContent");

                    recordDto.SasUri = contentInfo.BlobSasUri;

                    var rccReportContent = new DisposalHistoryReportContentDto();
                    rccReportContent.DisplayName = !string.IsNullOrEmpty(historyNode.DisplayName) ? historyNode.DisplayName : recordDto.Name;
                    rccReportContent.ContentDto = recordDto;
                    rccReportContent.TimeRange = historyNode?.CustomDate;
                    rccReportContent.EndDateWithin = HandleDisposalHistoryTimeRange(historyNode?.CustomDate, historyNode?.LatestExportType ?? 0, gls, timeZoneId, isDaylight);

                    resultList.Add(rccReportContent);
                }

                pageInfo.HasNextPage = (parsedPageIndex * pageInfo.PageSize) < totalCount;
                pageInfo.Total = totalCount;
                info.Datas = resultList;
                info.PagingInfo = pageInfo;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while query archived content for explorer,ERROR:{0}", ex.ToString());
            }
            return info;
        }

        private string HandleDisposalHistoryTimeRange(ManualHistoryCustomDataTime timeRange, int type, GeneralSettingModel gls, string timeZoneId, bool isDaylight)
        {
            return type switch
            {
                (int)TimeRange.After3Month => I18NEntity.GetString("RM_MA_EntendDisposalTime_3M"),
                (int)TimeRange.After6Month => I18NEntity.GetString("RM_MA_EntendDisposalTime_6M"),
                (int)TimeRange.After1Year => I18NEntity.GetString("RM_MA_EntendDisposalTime_1Y"),
                (int)TimeRange.Custom => BuildDisposalHistoryDateString(timeRange, gls, timeZoneId, isDaylight),
                (int)TimeRange.All => I18NEntity.GetString("RM_MA_EntendDisposalTime_All"),
                _ => string.Empty
            };
        }

        private string BuildDisposalHistoryDateString(ManualHistoryCustomDataTime timeRange, GeneralSettingModel gls, string timeZoneId, bool isDaylight)
        {
            string fromLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_From");
            string toLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_To");
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            var startStr = string.Empty;
            var endStr = string.Empty;
            if (!string.IsNullOrEmpty(timeZoneId))
            {
                startStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.StartDateTimeTicks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
                endStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.EndDateTimeTicks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
            }
            else
            {
                startStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.StartDateTimeTicks, true).SimplifyFormatTime;
                endStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.EndDateTimeTicks, true).SimplifyFormatTime;
            }
            return $"{fromLabel} {startStr} {toLabel} {endStr}";
        }

        private string HandleTimeRange(RCCReportTimeRange timeRange, GeneralSettingModel gls, string timeZoneId, bool isDaylight)
        {
            if (timeRange == null) return string.Empty;

            return timeRange.PresetType switch
            {
                1 => I18NEntity.GetString("RM_FS_DateRangeCustom_3M"),
                2 => I18NEntity.GetString("RM_FS_DateRangeCustom_6M"),
                3 => I18NEntity.GetString("RM_FS_DateRangeCustom_1Y"),
                _ => BuildCustomDateString(timeRange, gls, timeZoneId, isDaylight)
            };
        }

        private string BuildCustomDateString(RCCReportTimeRange timeRange, GeneralSettingModel gls, string timeZoneId, bool isDaylight)
        {
            string fromLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_From");
            string toLabel = I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_To");
            string dateFormat = GeneralSettingConfig.DateFormats[(DateFormat)Enum.Parse(typeof(DateFormat), Enum.GetName(typeof(DateFormat), gls.DataFormatId), true)];
            var startStr = string.Empty;
            var endStr = string.Empty;
            if (!string.IsNullOrEmpty(timeZoneId))
            {
                startStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.StartDateTicks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
                endStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.EndDateTicks, true, Convert.ToInt32(timeZoneId), isDaylight, dateFormat).SimplifyFormatTime;
            }
            else
            {
                startStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.StartDateTicks, true).SimplifyFormatTime;
                endStr = GeneralSettingService.ConvertTiksToDateTime(gls, timeRange.EndDateTicks, true).SimplifyFormatTime;
            }
            return $"{fromLabel} {startStr} {toLabel} {endStr}";
        }

        [Audit(Module = AuditModule.DownloadCenter, Category = AuditCategory.DownloadCenter, Action = AuditAction.DeleteArchivedContent, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage DeleteArchivedContent(List<Guid> jobIds)
        {
            RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                List<int> finalJobStatus = new List<int>()
                {
                    (int)DownloadContentJobStatus.None,
                    (int)DownloadContentJobStatus.Calculating,
                    (int)DownloadContentJobStatus.Failed,
                    (int)DownloadContentJobStatus.Finished,
                    (int)DownloadContentJobStatus.FinishWithException,
                    (int)DownloadContentJobStatus.Skipped,
                    (int)DownloadContentJobStatus.Stopped,
                    (int)DownloadContentJobStatus.Stopping
                };
                var contentInfos = DownloadDataInfoDao.GetDownloadDataInfos(jobIds, finalJobStatus);
                
                //Audit trail JPMC
                if (contentInfos[0].DownloadType == DownloadContentType.DownloadRCCReport)
                {
                    List<RMMyhubReportAuditItem> auditItems = RMMyhubServices.GetMyhubReports(jobIds, (int)MyhubReportJobType.DownloadRCCReport, false);
                    FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)FSAuditType.DeleteRCCReport, (int)MyhubReportJobType.DownloadRCCReport);
                }
                else if (contentInfos[0].DownloadType == DownloadContentType.HistoryContent)
                {
                    List<RMMyhubReportAuditItem> auditItems = RMMyhubServices.GetMyhubReports(jobIds, (int)MyhubReportJobType.HistoryContent, false);
                    FSAuditSinkService.MyhubReportContentFlushAsync(auditItems, (int)FSAuditType.DeleteDisposalHistory, (int)MyhubReportJobType.HistoryContent);
                }
                // end Audit trail JPMC

                if (contentInfos != null && contentInfos.Count > 0)
                {
                    message.Extension = JsonConvert.SerializeObject(contentInfos.Select(c => c.Name).ToList());
                }
                List<RMDownloadDataInfo> deletedInfos = new List<RMDownloadDataInfo>();
                ArgumentCheck.NotNull(contentInfos, nameof(contentInfos));
                foreach (var info in contentInfos)
                {
                    try
                    {
                        //if (info.JobStatus == (int)DownloadContentJobStatus.Finished)
                        {
                            ArchivedContentDownloadService.DeleteExpiredData(info.JobId);
                        }
                        deletedInfos.Add(info);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while deleting archived content. Id:{info?.RecordsId} Error:{e.ToString()}");
                        message.MessageType = RAMessageType.Failed;
                    }
                }
                if (deletedInfos.Count > 0)
                {
                    try
                    {
                        DownloadDataInfoDao.BatchDelete(deletedInfos);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while batch deleting archived content. Error:{e.ToString()}");
                        message.MessageType = RAMessageType.Failed;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while deleting archived contents. Error:{e.ToString()}");
                message.MessageType = RAMessageType.Failed;
            }
            return message;
        }


        private string GetFullPath(Record data, Dictionary<Guid, RMScope> dicMap)
        {
            string fullPath = string.Empty;
            if (dicMap.ContainsKey(data.ScopeId))
            {
                var sPath = dicMap[data.ScopeId];
                fullPath = WebUtil.MakeFullUrl(sPath?.FullPath, data.DirPath);
            }
            else
            {
                //RECO-2576
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(data.AveSiteId);
                fullPath = site == null ? data.LeafName : WebUtil.MakeFullUrl(site.url, data.DirPath);
                logger.Info("get site info from dao:siteId:{0}, siteUrl:{1},path:{2}", data.AveSiteId.ToString(), site?.url, data.FullPath);
                if (site != null)
                {
                    var scope = new RMScope()
                    {
                        FullPath = site.url,
                        ScopeId = data.ScopeId,
                        ScopeName = site.Name,
                        IsRemoved = false,
                    };
                    RMScopeDao.AddOrUpateSiteScope(scope);
                    dicMap.Add(data.ScopeId, scope);
                }
            }
            return fullPath;
        }

        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeTerm, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> ChangeTermAsync(ChangeTermDto changeTermInfo)
        {
            #region send message
            //debug 
            //string xml = "testmessage";
            //MessageBuilder messageBuilder = new MessageBuilder();
            //messageBuilder.PutMessage(xml);

            #endregion
            RAReturnMessage msg = new RAReturnMessage();
            RMTerm selectedTerm = TermDao.GetRMTermByUniqueId(changeTermInfo.TermInfo.UniqueId, false);
            if (selectedTerm.IsDeprecated || selectedTerm.IsExpired)
            {
                string message = I18NEntity.GetString("RM_JS_JMD_Comment_Auto_TermNotAvailable");
                msg.ErrorMessage = message;
                msg.MessageType = RAMessageType.Failed;
                return msg;
            }
            // ListenerPocessStart();
            string jobId = string.Empty;
            int updateResult;


            if (changeTermInfo.FSRecordIds?.Count > 0)
            {
                var recordId = changeTermInfo.FSRecordIds.FirstOrDefault();
                var folderRecord = ExplorerDao.GetFSRecordById(recordId);
                if (folderRecord != null && folderRecord.NodeType == 2100)
                {
                    RAReturnMessage returnMessage = new RAReturnMessage();
                    string id = string.Empty;
                    try
                    {
                        var groupId = TenantLocalValue.LogonGroupId;
                        var loginName = TenantLocalValue.LogonUserEmail;
                        changeTermInfo.UserId = TenantLocalValue.LogonUserId;
                        JobQueueDto jqDto = new JobQueueDto()
                        {
                            JobType = JobType.FSFolderChangeTerm,
                            Parameters = SerializerHelper.SerializeByDataContractSerializer(changeTermInfo),
                            JobRunType = JobRunBy.Control,
                            TenantGroupId = groupId,
                            JobRunByUser = loginName
                        };
                        returnMessage.MessageType = RAMessageType.Successful;
                        returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
                    }
                    catch (Exception ex)
                    {
                        returnMessage.MessageType = RAMessageType.Failed;
                        returnMessage.ErrorMessage = ex.Message;
                    }
                    return returnMessage;
                    #region remove code
                    //Start DA Job  //TO DO next
                    //    Adonis.Records.Object.RecordsControlMessage jobMessage = new Adonis.Records.Object.RecordsControlMessage();
                    //    AssembleDBInfo(jobMessage);
                    //    jobMessage.JobType = Adonis.Records.Object.RMMessageType.FSReclassify;
                    //    RMCPGlobalStorageSetting setting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                    //    jobMessage.ProcessingPoolId = setting == null ? null : setting.ProcessingPoolId.ToString();
                    //    List<RMBaseRecord> records = new List<RMBaseRecord>();

                    //    if (changeTermInfo.RecordIds != null && changeTermInfo.RecordIds.Count > 0)
                    //    {
                    //        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                    //        {
                    //            records = CollectionDataDao.GetRecordByIds(changeTermInfo.RecordIds);//to do
                    //        }

                    //        var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.Select(t => t.Id).ToList());
                    //        jobMessage.FSChangeFolderRecords = recDic;
                    //        jobMessage.TermId = changeTermInfo.TermInfo.UniqueId;
                    //        try
                    //        {
                    //            Adonis.Records.Object.ResultBase result = MRecordsService.StartJob(jobMessage);
                    //            RecordsReturnMessage raResult = result as RecordsReturnMessage;
                    //            if (raResult != null && raResult.ResultType == ResultType.Success)
                    //            {
                    //                jobId = result.JobId;
                    //                string runBy = LoginService.GetCurrentUserInfo().DisplayName; //RMSessionStore.GetLogonUserInfo()
                    //                RMJobService.CreateJob(JobType.FSFolderChangeTerm, runBy, jobId);     //Todo jobType
                    //                msg.Extsion1 = jobId;
                    //            }
                    //            else
                    //            {
                    //                msg.MessageType = RAMessageType.Failed;
                    //                msg.ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError");
                    //            }
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            msg.ErrorMessage = e.Message;
                    //            msg.MessageType = RAMessageType.Failed;
                    //            msg.FaildType = RAFailedType.None;
                    //            logger.Error("Start DA job for change term failed {0}", e.ToString());
                    //        }
                    //        return msg;
                    //    }
                    #endregion
                }
            }

            bool isOnPremJob = IsSPOnPremJob(changeTermInfo);
            using (new RA.Common.PerformanceScope(string.Format("change.Term.change.send reuqest")))
            {
                if (isOnPremJob)
                {
                    (_,jobId) = await RMSharePointOnPremSettingsService.UpdateOnPremTermsAsync(GetChangeTermOption(changeTermInfo));
                }
                else
                {
                    updateResult = UpdateTerms(changeTermInfo, ref jobId);
                }
            }
            msg.Extension = jobId;
            try
            {
                List<Guid> allGuids = new List<Guid>();
                if (isOnPremJob)
                {
                    allGuids.AddRange(changeTermInfo.SPOnPremRecordIds?.ToList());
                }
                else
                {
                    allGuids.AddRange(changeTermInfo.RecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.EXORecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.FSRecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.OneDriveRecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.AzureFileShareRecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.BoxRecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.CustomizeConnectorRecordIds?.ToList());
                    allGuids.AddRange(changeTermInfo.TeamsRecordIds?.ToList());
                }
                msg.Extsion1 = JsonConvert.SerializeObject(ExplorerDao.GetRecordByIds(allGuids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                logger.Warn("get records name error");
            }
            return msg;
        }
        public async Task<RAReturnMessage> ChangeGoogleTermAsync(ChangeTermDto changeTermDto)
        {
            RAReturnMessage messenger = new RAReturnMessage();
            ListenerPocessStart();
            string jobId = string.Empty;
            int updateResult;
            updateResult = UpdateTerms(changeTermDto, ref jobId);
            messenger.Extension = jobId;
            try
            {
                List<Guid> ids = new List<Guid>();
                ids.AddRange(changeTermDto.GoogleDriveRecordIds?.ToList());
                messenger.Extsion1 = JsonConvert.SerializeObject(ExplorerDao.GetRecordByIds(ids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                logger.Warn("get records name error");
            }
            return messenger;
        }
    

        public bool CheckItemsInTheSameSecurityGroup(List<Guid> recordIds)
        {
            if (recordIds.Count == 1)
            {
                return true;
            }
            var records = ExplorerDao.GetRecordByIds(recordIds);
            var oneSourceRecordsGroupings = records.GroupBy(r => r.SourceFlag);
            var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
            var allDataSecurityGroups = new List<int>();
            var hasNotInAnyGroupData = false;
            foreach (var recordsGrouping in oneSourceRecordsGroupings)
            {
                if (defaultContianerIdSources.Contains((SourceFlag)recordsGrouping.Key))
                {
                    var containerGuids = recordsGrouping.Where(g => !string.IsNullOrEmpty(g.ContainerId)).Select(g => new Guid(g.ContainerId)).ToList();
                    var scopeRoleDic = RMScopeRoleAssignmentDao.GetAllScopeRoleByContainerId(containerGuids, recordsGrouping.Key);
                    var securityGroupIdsByData = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(recordsGrouping.Select(g => g.ContainerId).ToList(), (SourceFlag)recordsGrouping.Key);
                    if (containerGuids.Any(s => !scopeRoleDic.ContainsKey(s)))
                    {
                        hasNotInAnyGroupData = true;
                    }
                    allDataSecurityGroups.AddRange(securityGroupIdsByData);
                }
            }
            var distinctSecurityGroups = allDataSecurityGroups.Distinct().ToList();
            if (hasNotInAnyGroupData && distinctSecurityGroups.Count > 0)
            {
                return false;
            }
            else if (distinctSecurityGroups.Count > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private ChangeTermOption GetChangeTermOption(ChangeTermDto changeTermInfo)
        {
            ChangeTermOption ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds,
                SourceFSRecordIds = changeTermInfo.FSRecordIds,
                SourceEXORecordIds = changeTermInfo.EXORecordIds,
                SourcePhyRecordIds = changeTermInfo.PhyRecordIds,
                SourceSPOnPremRecordIds = changeTermInfo.SPOnPremRecordIds,
                TargetTermId = changeTermInfo.TermInfo.Id,
                TargetTermName = changeTermInfo.TermInfo.Name,
                TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeTermInfo.Comment,
                ChangeTermOrigin = changeTermInfo.ChangeTermOrigin,
            };
            return ChangeTermOption;
        }

        private bool IsSPOnPremJob(ChangeTermDto dto)
        {
            bool isOnPremise = false;
            if (dto.SPOnPremRecordIds != null && dto.SPOnPremRecordIds.Count > 0)
            {
                isOnPremise = true;
            }
            logger.Info("Is SP on premise job:{0}", isOnPremise);
            return isOnPremise;
        }

        private bool IsSPOnPremJob(Guid id)
        {
            bool isOnPremise = false;
            var record = ExplorerDao.GetRecordByIds(new List<Guid>() { id })?.FirstOrDefault();
            if (record != null && record.SourceFlag == (int)SourceFlag.SharePointOnPrem)
            {
                isOnPremise = true;
            }
            logger.Info("Is SP on premise job:{0}", isOnPremise);
            return isOnPremise;
        }

        public RAReturnMessage DoGlobalSearchRealTimeAction(GlobalSearchActionDto globalSearchActionDto)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            var result = DoAction(globalSearchActionDto);
            returnMessage.Extension = result;
            return returnMessage;
        }

        public RAReturnMessage StartGlobalSearchActionJob(GlobalSearchActionDto globalSearchActionDto)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            string id = string.Empty;
            try
            {
                if ((SourceFlag)globalSearchActionDto.SourceFlag == SourceFlag.FileSystem && !TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem))
                {
                    logger.Error("StartGlobalSearchActionJob Error");
                    var msg = I18NEntity.GetString("RM_FS_LicensePermissions");
                    throw new Exception($"{msg}");
                }
                var isSupportRecordLabel = ((SourceFlag)globalSearchActionDto.SourceFlag == SourceFlag.SharePoint || (SourceFlag)globalSearchActionDto.SourceFlag == SourceFlag.Teams
                    || (SourceFlag)globalSearchActionDto.SourceFlag == SourceFlag.OneDrive) && !DataCenterUtil.Is21V() && RMKeyValueDao.IsNewOpusTenant();
                if (isSupportRecordLabel)
                {
                    globalSearchActionDto.Action = globalSearchActionDto.Action switch
                    {
                        GlobalSearchAction.DeclareRecords => GlobalSearchAction.AddRecordLabel,
                        GlobalSearchAction.UnDeclareRecords => GlobalSearchAction.RemoveRecordLabel,
                        _ => globalSearchActionDto.Action
                    };
                }
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                globalSearchActionDto.UserId = TenantLocalValue.LogonUserId;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GlobalSearchAction,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(globalSearchActionDto),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        public async Task<RAReturnMessage> ValidateParameterAsync(GlobalSearchActionDto actionDto, ChangeTermPage page)
        {
            var returnMessage = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                logger.Info($"Current change term page is {page.ToString()}");
                switch (actionDto.Action)
                {
                    case GlobalSearchAction.Reclassify:
                        ChangeTermDto changeTermDto = JsonConvert.DeserializeObject<ChangeTermDto>(actionDto.ActionExtension.ToString());
                        RMTerm selectedTerm = new();
                        selectedTerm = TermDao.GetRMTermByUniqueId(changeTermDto.TermInfo.UniqueId, false);
                        if (selectedTerm.IsDeprecated || selectedTerm.IsExpired || changeTermDto.TermInfo == null)
                        {
                            string message = I18NEntity.GetString("RM_JS_JMD_Comment_Auto_TermNotAvailable");
                            returnMessage.ErrorMessage = message;
                            returnMessage.MessageType = RAMessageType.Failed;
                            return returnMessage;
                        }
                        actionDto.ActionExtension = SerializerHelper.SerializeByDataContractSerializer(GetChangeTermOption2(changeTermDto, page));
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while validating parameter. Error{e.ToString()}");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        private ChangeTermOption GetChangeTermOption2(ChangeTermDto changeTermInfo, ChangeTermPage page)
        {
            ChangeTermOption ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds,
                SourceFSRecordIds = changeTermInfo.FSRecordIds,
                SourceEXORecordIds = changeTermInfo.EXORecordIds,
                SourcePhyRecordIds = changeTermInfo.PhyRecordIds,
                SourceSPOnPremRecordIds = changeTermInfo.SPOnPremRecordIds,
                SourceOneDriveRecordIds = changeTermInfo.OneDriveRecordIds,
                GoogleDriveRecordIds = changeTermInfo.GoogleDriveRecordIds,
                SourceTeamsRecordIds = changeTermInfo.TeamsRecordIds,
                TargetTermId = changeTermInfo.TermInfo.Id,
                TargetTermName = changeTermInfo.TermInfo.Name,
                TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                ReclassifySubFiles = changeTermInfo.ReclassifySubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeTermInfo.Comment,
                ChangeTermOrigin = changeTermInfo.ChangeTermOrigin,
                IsManualData = changeTermInfo.IsManualData || page == ChangeTermPage.MyHub
            };
            return ChangeTermOption;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RunFSReclassicfyJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        //[FSAudit(AuditType = FSAuditType.Reclassify, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public string RealRunFSFolderReclassifyJob(JobRunBy JobRunType, string param)
        {
            logger.Info($"Start Run RealRunFSFolderReclassifyJob");
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                var jobType = JobType.FSFolderChangeTerm;
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);

                logger.Info(string.Format("Start explorer fs folder reclassify job : {0}", subJobId));
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                });
                logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunFSFolderReclassifyJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RunFSManageHoldJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public string RealRunFSFolderHoldJob(JobRunBy JobRunType, string param)
        {
            logger.Info($"Start Run RealRunFSFolderHoldJob");
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                var jobType = JobType.FSFolderManageHold;
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);

                logger.Info(string.Format("Start explorer fs folder hold job : {0}", subJobId));
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                });
                logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunFSFolderHoldJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RunGlobalSearchActionJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> RealRunGlobalSearchActionJobAsync(string param)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                var jobType = JobType.GlobalSearchAction;
                jobId = RMJobService.CreateJob(jobType, jobRunByUser, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, param);

                logger.Info(string.Format("Start global search action job : {0}", subJobId));
                if (IsOnPremiseJob(param))
                {
                    var farmId = (await SharePointOnPremClient.BrowseFarmsAsync())?.NodeList?.FirstOrDefault()?.FarmID;
                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.SPOnPremGlobalSearch,
                        TenantId = TenantLocalValue.LogonGroupId,
                        FarmId = farmId
                    });
                }
                else
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType.ToString(), subJobId),
                    });
                }
                logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunGlobalSearchActionJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        private bool IsOnPremiseJob(string param)
        {
            bool isOnPrem = false;
            var dto = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchActionDto>(param);
            if (dto != null && (SourceFlag)dto.SourceFlag == SourceFlag.SharePointOnPrem)
            {
                isOnPrem = true;
            }
            logger.Info("Is on premise job:", dto);
            return isOnPrem;
        }


        public RAReturnMessage PhysicalMove(PhysicalMoveDto moveDto)
        {
            var msg = new RAReturnMessage();
            var hasError = false;
            var errorMessage = string.Empty;
            if (string.IsNullOrEmpty(moveDto.LocationId) || string.IsNullOrEmpty(moveDto.BoxId))
            {
                errorMessage = I18NEntity.GetString(".Please select the destination node.");
            }
            if (moveDto.SourcePhyRecordIds == null || moveDto.SourcePhyRecordIds.Count == 0)
            {
                errorMessage = I18NEntity.GetString(".Please select the source items.");
            }
            //if (hasError)
            //{
            //    msg.ErrorMessage = errorMessage;
            //    msg.MessageType = RAMessageType.Failed;
            //    return msg;
            //}

            RMPhysicalExplorerMoveUtility utility = new RMPhysicalExplorerMoveUtility();
            var moveOption = new PhysicalMoveOption()
            {
                SourcePhyRecordIds = moveDto.SourcePhyRecordIds,
                LocationId = moveDto.LocationId,
                BoxId = moveDto.BoxId,
                FolderId = moveDto.FolderId,
                NameConflictOption = (AvePoint.RA.Contract.Object.RealTime.NameConflictOption)moveDto.NameConflictOption,
                HoldConflictOption = (AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption)moveDto.HoldConflictOption,
                FromModule = moveDto.FromModule
            };
            if (moveDto.HoldConflictOption == Contract.RMWeb.PhysicalMoveHoldConflictOption.None)
            {
                var holdConflict = utility.CheckMoveHasHoldConflict(moveOption);
                if (holdConflict)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.FaildType = RAFailedType.PhysicalMoveHasHoldConflict;
                    return msg;
                }
            }

            ListenerPocessStart();
            string jobId = string.Empty;
            int updateResult;

            using (new RA.Common.PerformanceScope(string.Format("move.physical.move.send reuqest")))
            {
                updateResult = MovePhysicalRecords(moveDto, ref jobId);
            }
            msg.Extension = jobId;
            try
            {
                List<Guid> allGuids = new List<Guid>();
                allGuids.AddRange(moveDto.SourcePhyRecordIds);
                msg.Extsion1 = JsonConvert.SerializeObject(ExplorerDao.GetRecordByIds(allGuids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                logger.Warn("get records name error");
            }
            return msg;
        }
        public RARealTimeJobMessage GetRealTimeJobStatusInfo(string jobId)
        {
            RARealTimeJobMessage msg = new RARealTimeJobMessage();
            try
            {
                var updateResult = RMRecordsUpdateTempDao.GetRealTimeJob(jobId);
                if (updateResult == null)
                {
                    msg.MessageType = RAMessageType.Successful;
                    msg.Status = RecordsConstants.Explorer_RealTime_Running;
                    return msg;
                }              
                if (jobId.StartsWith("UT"))
                {
                    //logger.Info($"{JsonConvert.SerializeObject(updateResult)}");
                    if (updateResult.Status == RecordsConstants.Explorer_RealTime_Running || updateResult.Waiting4OtherSourceChangeTerm)
                    {
                        //waiting for exo change term, keep noti in progress
                        msg.MessageType = RAMessageType.Successful;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords) ? null : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                        msg.Status = RecordsConstants.Explorer_RealTime_Running;
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Finished)
                    {
                        //stopTimer
                        msg.MessageType = RAMessageType.Successful;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords) ? null : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                        msg.Status = RecordsConstants.Explorer_RealTime_Finished;
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Failed_Partial)
                    {
                        //stopTimer
                        msg.ErrorMessage = string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), RMRecordsUpdateTempDao.GetFailedRecords(jobId));//to do next I18N
                        msg.MessageType = RAMessageType.Exception;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords) ? null : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Failed_All)
                    {
                        //RecordsListener.exe down, this code can't execute
                        msg.ErrorMessage = I18NEntity.GetString("RM_RDM_Explorer_ChangeTerm_All_Failed"); //to do next I18N
                        msg.MessageType = RAMessageType.Failed;
                    }

                    msg.Waiting4EXO = updateResult.Waiting4OtherSourceChangeTerm;
                    if (updateResult.Status != RecordsConstants.Explorer_RealTime_Running)
                    {
                        try
                        {
                            //skip Waiting4EXO in Dao method
                            RMRecordsUpdateTempDao.DeleteFinishedTempRecords(jobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Remove temp failed records failed {0},{1}", jobId, e.ToString());
                        }
                        try
                        {
                            RMRecordsUpdateTempDao.DeleteDirtData();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("delete temp data records failed {0},{1}", jobId, e.ToString());
                        }
                    }
                }
                else if (jobId.StartsWith("UD") || jobId.StartsWith("PM"))
                {
                    if (updateResult.Status == RecordsConstants.Explorer_RealTime_Running)
                    {
                        msg.MessageType = RAMessageType.Successful;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords) ? null : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                        msg.Status = RecordsConstants.Explorer_RealTime_Running;
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Finished)
                    {
                        msg.MessageType = RAMessageType.Successful;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords) ? null : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                        msg.Status = RecordsConstants.Explorer_RealTime_Finished;
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Failed_Partial)
                    {
                        msg.MessageType = RAMessageType.Failed;
                        msg.Items = string.IsNullOrEmpty(updateResult.ProcessRecords)
                                    ? string.IsNullOrEmpty(updateResult.ProcessRecords) ? null 
                                    : new List<string> { updateResult.FailedRecords.Trim('"') } 
                                    : JsonConvert.DeserializeObject<List<string>>(updateResult.ProcessRecords);
                        msg.ErrorMessage = string.Format(GetErrorMessage(jobId), RMRecordsUpdateTempDao.GetFailedRecords(jobId));
                    }
                    else if (updateResult.Status == RecordsConstants.Explorer_RealTime_Failed_All)
                    {
                        //RecordsListener.exe down, this code can't execute
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_RDM_Explorer_ChangeTerm_All_Failed");  //申请词条
                    }

                    if (updateResult.Status != RecordsConstants.Explorer_RealTime_Running)
                    {
                        try
                        {
                            RMRecordsUpdateTempDao.DeleteFinishedTempRecords(jobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn("Remove temp failed records failed {0},{1}", jobId, e.ToString());
                        }
                        try
                        {
                            RMRecordsUpdateTempDao.DeleteDirtData();
                        }
                        catch (Exception e)
                        {
                            logger.Warn("delete temp data records failed {0},{1}", jobId, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = e.Message;
                logger.Warn($"get real time job status error:{e}");
            }
            return msg;
        }

        private string GetErrorMessage(string jobId)
        {
            var msg = "";
            if (jobId.StartsWith("UD"))
            {
                msg = I18NEntity.GetString("RM_JS_BCM_Explorer_DeclareErrorMsg");
            }
            else if (jobId.StartsWith("PM"))
            {
                msg = I18NEntity.GetString("RM_RDM_Explorer_PhysicalMoveError");
            }
            return msg;
        }
        private void ListenerPocessStart()
        {
            if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment) return;
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SERVICE_BUS_CONNECTION_STRING]))
            {
                return;
            }
            string filePath = string.Empty;
            try
            {
                System.Diagnostics.Process[] ps = GetProcesses("RecordsListener");
                if (ps.Length > 0)
                {
                    logger.Info("Listener Exist");
                    return;
                }
                string installPath = WebUtil.GetInstallPath();
                filePath = installPath + "\\bin\\RecordsListener.exe";

                var startInfo = new ProcessStartInfo(
                    filePath);

                startInfo.CreateNoWindow = true;
                var process = Process.Start(startInfo);
                logger.Info("service Listener process has started.");
            }
            catch (Exception e)
            {
                logger.Error("Start Listener failed path:{0}, {1}", filePath, e.ToString());
            }
        }
        private System.Diagnostics.Process[] GetProcesses(string processName)
        {
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(processName);

            try
            {
                var invalid = false;
                foreach (var process in ps)
                {
                    if (!File.Exists(process.MainModule.FileName))
                    {
                        process.Kill();
                        logger.Warn("The process is not valid:{0}", process.MainModule.FileName);
                        invalid = true;
                    }
                }

                if (invalid)
                {
                    ps = System.Diagnostics.Process.GetProcessesByName(processName);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Get processes with name:{0} failed:{1}", processName, ex);
            }

            return ps;
        }
        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeTerm, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        //public RAReturnMessage ChangeTerm(ChangeTermDto changeTermInfo)
        //{
        //    string jobId = string.Empty;
        //    RAReturnMessage msg = new RAReturnMessage();
        //    //#region old logic in Web 
        //    //SharePoint.RMExplorer.RMExplorerUtility utility = new SharePoint.RMExplorer.RMExplorerUtility();
        //    //string jobId = string.Empty;
        //    ////System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() => utility.ChangeAllTerms(changeTermInfo, jobId));
        //    //try
        //    //{
        //    //    utility.ChangeAllTerms(changeTermInfo, jobId);
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    msg.ErrorMessage = e.Message;
        //    //    msg.MessageType = RAMessageType.Failed;
        //    //}
        //    //#endregion
        //    int updateResult;
        //    using (new RA.Common.PerformanceScope(string.Format("change.Term.change.send reuqest")))
        //    {
        //        updateResult = UpdateTerms(changeTermInfo, ref jobId);
        //    }

        //    if (updateResult == RecordsConstants.Explorer_RealTime_Failed_Partial)
        //    {
        //        string result = RMRecordsUpdateTempDao.GetFailedRecords(jobId);
        //        string message = string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), result);//to do next I18N
        //        msg.ErrorMessage = message;
        //        msg.MessageType = RAMessageType.Failed;
        //        try
        //        {
        //            RMRecordsUpdateTempDao.DeleteFailedTempRecords(jobId);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Warn("Remove temp failed records failed {0},{1}", jobId, e.ToString());
        //        }
        //    }
        //    else if (updateResult == RecordsConstants.Explorer_RealTime_Failed_All)
        //    {
        //        string message = I18NEntity.GetString("RM_RDM_Explorer_ChangeTerm_All_Failed"); //to do next I18N
        //        msg.ErrorMessage = message;
        //        msg.MessageType = RAMessageType.Failed;
        //    }
        //    else
        //    {
        //        msg.MessageType = RAMessageType.Successful;
        //    }
        //    return msg;
        //}
        private int UpdateTerms(ChangeTermDto changeTermInfo, ref string updateTermTempJobId)
        {
            updateTermTempJobId = "UT" + Guid.NewGuid().ToString();
            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = updateTermTempJobId;
            jobMessage.Action = RealTimeAction.ChangeTerm;
            jobMessage.ChangeTermOption = new ChangeTermOption()
            {
                SourceRecordIds = changeTermInfo.RecordIds,
                SourceFSRecordIds = changeTermInfo.FSRecordIds,
                SourceEXORecordIds = changeTermInfo.EXORecordIds,
                SourcePhyRecordIds = changeTermInfo.PhyRecordIds,
                SourceOneDriveRecordIds = changeTermInfo.OneDriveRecordIds,
                SourceAzureFileShareRecordIds = changeTermInfo.AzureFileShareRecordIds,
                SourceBoxRecordIds = changeTermInfo.BoxRecordIds,
                SourceCustomizeConnectorRecordIds = changeTermInfo.CustomizeConnectorRecordIds,
                GoogleDriveRecordIds = changeTermInfo.GoogleDriveRecordIds,
                SourceTeamsRecordIds = changeTermInfo.TeamsRecordIds,
                TargetTermId = changeTermInfo.TermInfo.Id,
                TargetTermName = changeTermInfo.TermInfo.Name,
                TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                ReclassifySubFiles = changeTermInfo.ReclassifySubFiles,
                Comment = changeTermInfo.Comment,
                IsManualData = changeTermInfo.IsManualData,
                ChangeTermOrigin = changeTermInfo.ChangeTermOrigin
            };
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;
            jobMessage.RecordsDBInfo = new RecordsDBInfo() { ConnString = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING] };

            //var agent = new ServiceDto() { Address = WebUtil.GetIPAddress(), Port = WebUtil.ListenerPort, Schema = "net.tcp" };

            try
            {
                SendMessageAsync(jobMessage);
                //Task task = Task.Run(() =>
                //{
                //    logger.Info("Send change term {0} to agent {1}", jobMessage.JobId, agent.Address);
                //    jobMessage.AgentInfo = agent;
                //    IARecordsListener ARecordsService = DocAveServiceHelper.CreateAgentService<IARecordsListener>(agent.Port, agent.Schema, agent.Address);
                //    SendMessageToListener(jobMessage, ARecordsService);
                //});
                //if (result != null)
                //{
                //    RecordsReturnMessage realReturn = result as RecordsReturnMessage;
                //    logger.Info("Change term result type : {0}", realReturn.ResultType);
                //    return realReturn.ResultType == ResultType.Success ? RecordsConstants.Explorer_RealTime_Success : RecordsConstants.Explorer_RealTime_Failed_Partial;
                //}
                //RecordsAgentCacheManager.FinishOneProcess(agent.Address);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                //RecordsAgentCacheManager.FinishOneProcess(agent.Address);
                ////如果通信异常, 则说明有可能此Agent状态发生了变化, 主动触发一次更新Cache.
                //RecordsAgentCacheManager.UpdateCache(processingPoolId);
                return RecordsConstants.Explorer_RealTime_Failed_All;
            }
            return RecordsConstants.Explorer_RealTime_Success;
        }
        //private int UpdateOnPremTerms(ChangeTermDto changeTermInfo, ref string updateTermTempJobId)
        //{
        //    updateTermTempJobId = "UT" + Guid.NewGuid().ToString();
        //    AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage = new AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage();
        //    jobMessage.JobId = updateTermTempJobId;
        //    jobMessage.Action = AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.ChangeTerm;
        //    jobMessage.ChangeTermOption = new AvePoint.RA.Contract.Global.JobMessage.ChangeTermOption()
        //    {
        //        SourceSPOnPremRecordIds = changeTermInfo.SPOnPremRecordIds,
        //        TargetTermId = changeTermInfo.TermInfo.Id,
        //        TargetTermName = changeTermInfo.TermInfo.Name,
        //        TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId,
        //        OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
        //        LogonUser = WebUtil.LogOnUserName
        //    };

        //    try
        //    {
        //        SendRealtimeJobToAgent(jobMessage);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.Message, e);
        //        return RecordsConstants.Explorer_RealTime_Failed_All;
        //    }
        //    return RecordsConstants.Explorer_RealTime_Success;
        //}

        //private void SendRealtimeJobToAgent(AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage)
        //{
        //    var batchId = Guid.NewGuid();
        //    var farmId = jobMessage.Action == Contract.Global.JobMessage.RealTimeAction.ChangeTerm ?
        //        GetFarmId(jobMessage.ChangeTermOption.SourceSPOnPremRecordIds.FirstOrDefault()) :
        //        GetFarmId(jobMessage.DeclareIds.FirstOrDefault());
        //    logger.Info("Begin get proxy");
        //    var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
        //    logger.Info("End get proxy");

        //    var agents = SignalRService.GetAgentsByFarmId(TenantLocalValue.LogonGroupId, farmId);
        //    logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
        //    var agent = agents.FirstOrDefault();
        //    logger.Info($"Farm: [{farmId}] used agent: [{agent.AgentId}].");


        //    var args = new SharePointOnPremRealtimeJobArgs
        //    {
        //        BatchId = batchId.ToString(),
        //        Message = SerializerHelper.SerializeByDataContractSerializer(jobMessage)
        //    };

        //    var result = System.Threading.Tasks.Task.Run(() =>
        //        proxy.InvokeOneAgentAysnc<SharePointOnPremRealtimeJobExecute, SharePointOnPremRealtimeJobArgs, SharePointOnPremRealtimeJobResult>(agent, new SharePointOnPremRealtimeJobExecute { MethodArgs = args })
        //    ).Result;

        //    if (result.Result == SharePointOnPremRealtimeJobResultEnum.Failed)
        //    {
        //        logger.Error($"Process sharepoint on-prem realtime job failed. Error: {result.Message}");
        //    }
        //}

        //private string GetFarmId(Guid id)
        //{
        //    var record = ExplorerDao.GetRecordByIds(new List<Guid>() { id });
        //    var siteId = record.FirstOrDefault().AveSiteId;
        //    var site = AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.GetLocalSiteCollectionById(siteId);
        //    return site.FarmId;
        //}

        private string DoAction(GlobalSearchActionDto globalSearchActionDto)
        {
            string messageId = Guid.NewGuid().ToString();
            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = messageId;
            jobMessage.Action = RealTimeAction.GlobalSearchAction;
            jobMessage.GlobalSearchInfo = globalSearchActionDto;
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;
            jobMessage.RecordsDBInfo = new RecordsDBInfo() { ConnString = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING] };

            try
            {
                SendMessageAsync(jobMessage);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return string.Empty;
            }
            return messageId;
        }

        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.DeclareAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> DeclareAsRecordAsync(List<Guid> ids)
        {
            return await DeclareOrUndeclareAsRecordAsync(ids, true);
        }

        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.UndeclareAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> UndeclareAsRecordAsync(List<Guid> ids)
        {
            return await DeclareOrUndeclareAsRecordAsync(ids, false);
        }

        private async Task<RAReturnMessage> DeclareOrUndeclareAsRecordAsync(List<Guid> ids, bool isDeclared)
        {
            ListenerPocessStart();
            RAReturnMessage msg = new RAReturnMessage();
            #region old logic
            //try
            //{
            //    SharePoint.RMExplorer.RMExplorerUtility utility = new SharePoint.RMExplorer.RMExplorerUtility();
            //    string jobId = string.Empty;
            //    var displayName = LoginService.GetCurrentUserInfo().LoginName; //RMSessionStore.GetLogonUserInfo()
            //    //System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() => utility.DeclaredRecords(ids, jobId, isDeclared, displayName));
            //    utility.DeclaredRecords(ids, jobId, isDeclared, displayName);
            //    msg.MessageType = RAMessageType.Successful;
            //}
            //catch (Exception e)
            //{
            //    logger.Warn("Declared Recordsd Error {0}", e.ToString());
            //    msg.ErrorMessage = e.Message;
            //    //throw new Exception("Declared Recordsd Error");
            //    msg.MessageType = RAMessageType.Failed;
            //}
            #endregion
            string jobId = string.Empty;
            int updateResult = 0;
            var isOnPremJob = IsSPOnPremJob(ids.FirstOrDefault());
            if (isOnPremJob)
            {
                (var message,jobId) = isDeclared ? await RMSharePointOnPremSettingsService.SPOnPremDeclaredItemRecordsAsync(ids, isDeclared) : await RMSharePointOnPremSettingsService.SPOnPremUnDeclaredItemRecordsAsync(ids, isDeclared);
                updateResult = message.ResultType == ResultType.Success ? RecordsConstants.Explorer_RealTime_Success : RecordsConstants.Explorer_RealTime_Failed_All;
            }
            else
            {
                updateResult = DeclaredItemRecords(ids, isDeclared, ref jobId);
            }
            msg.Extension = jobId;
            try
            {
                msg.Extsion1 = JsonConvert.SerializeObject(ExplorerDao.GetRecordByIds(ids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                logger.Warn("get records name error");
            }
            return msg;
        }

        //TODO 
        private int DeclaredItemRecords(List<Guid> ids, bool isDeclared, ref string declaredTempJobId)
        {
            declaredTempJobId = "UD" + Guid.NewGuid().ToString();

            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = declaredTempJobId;
            jobMessage.Action = isDeclared ? RealTimeAction.Declare : RealTimeAction.UnDeclare;

            jobMessage.RecordIds = ids;
            jobMessage.DeclareBy = WebUtil.LogOnUserName;
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;
            jobMessage.RecordsDBInfo = new RecordsDBInfo() { ConnString = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING] };

            //var agent = new ServiceDto() { Address = WebUtil.GetIPAddress(), Port = WebUtil.ListenerPort, Schema = "net.tcp" };

            try
            {
                SendMessageAsync(jobMessage);
                //Task task = Task.Run(() =>
                //{
                //    logger.Info("Send declare records {0} to agent {1}", jobMessage.JobId, agent.Address);
                //    jobMessage.AgentInfo = agent;
                //    IARecordsListener ARecordsService = DocAveServiceHelper.CreateAgentService<IARecordsListener>(agent.Port, agent.Schema, agent.Address);
                //    SendMessageToListener(jobMessage, ARecordsService);
                //});
                //if (result != null)
                //{
                //    RecordsReturnMessage realReturn = result as RecordsReturnMessage;
                //    logger.Info("Declare result type : {0}", realReturn.ResultType);
                //    return realReturn.ResultType == ResultType.Success ? RecordsConstants.Explorer_RealTime_Success : RecordsConstants.Explorer_RealTime_Failed_Partial;
                //}

            }
            catch (Exception e)
            {

                logger.Error(e.Message, e);
                return RecordsConstants.Explorer_RealTime_Failed_All;
            }
            return RecordsConstants.Explorer_RealTime_Success;
        }

        //private int SPOnPremDeclaredItemRecords(List<Guid> ids, bool isDeclared, ref string declaredTempJobId)
        //{
        //    declaredTempJobId = "UD" + Guid.NewGuid().ToString();
        //    AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage = new AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage();
        //    jobMessage.JobId = declaredTempJobId;
        //    jobMessage.Action = isDeclared ? AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.Declare : AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.UnDeclare;

        //    jobMessage.DeclareIds = ids;
        //    jobMessage.DeclaredBy = WebUtil.LogOnUserName;
        //    try
        //    {
        //        SendRealtimeJobToAgent(jobMessage);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(e.Message, e);
        //        return RecordsConstants.Explorer_RealTime_Failed_All;
        //    }
        //    return RecordsConstants.Explorer_RealTime_Success;
        //}

        private int MovePhysicalRecords(PhysicalMoveDto moveDto, ref string tempJobId)
        {
            tempJobId = "PM" + Guid.NewGuid().ToString();
            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = tempJobId;
            jobMessage.Action = RealTimeAction.PhysicalMove;
            jobMessage.PhysicalMoveOption = new PhysicalMoveOption()
            {
                SourcePhyRecordIds = moveDto.SourcePhyRecordIds,
                LocationId = moveDto.LocationId,
                BoxId = moveDto.BoxId,
                FolderId = moveDto.FolderId,
                NameConflictOption = (AvePoint.RA.Contract.Object.RealTime.NameConflictOption)moveDto.NameConflictOption,
                HoldConflictOption = (AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption)moveDto.HoldConflictOption,
                DestinationPath = string.IsNullOrEmpty(moveDto.BoxId) ? LocationManagementService.GetLocationPathById(new Guid(moveDto.LocationId)) :  GetPhysicalBoxPathByIdAsync(new Guid(moveDto.LocationId)).GetAwaiter().GetResult(),
                FromModule = moveDto.FromModule
        };
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;
            jobMessage.RecordsDBInfo = new RecordsDBInfo() { ConnString = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING] };
            //var agent = new ServiceDto() { Address = WebUtil.GetIPAddress(), Port = WebUtil.ListenerPort, Schema = "net.tcp" };
            try
            {
                SendMessageAsync(jobMessage);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                //RecordsAgentCacheManager.FinishOneProcess(agent.Address);
                ////如果通信异常, 则说明有可能此Agent状态发生了变化, 主动触发一次更新Cache.
                //RecordsAgentCacheManager.UpdateCache(processingPoolId);
                return RecordsConstants.Explorer_RealTime_Failed_All;
            }
            return RecordsConstants.Explorer_RealTime_Success;
        }
        private int MovePhysicalRecordsRequest(List<PhysicalMoveRequest> moveRequests, ref string tempJobId)
        {
            tempJobId = "PM" + Guid.NewGuid().ToString();
            RecordsRealTimeMessage jobMessage = new RecordsRealTimeMessage();
            jobMessage.JobId = tempJobId;
            jobMessage.Action = RealTimeAction.PhysicalMoveRequest;
            jobMessage.PhysicalMoveRequests = moveRequests;
            jobMessage.LogonGroupId = TenantLocalValue.LogonGroupId;
            jobMessage.CurrentUserName = TenantLocalValue.LogonUserEmail;
            jobMessage.RecordsDBInfo = new RecordsDBInfo() { ConnString = RMGlobalConfiguration.DBConfig[Contract.Configurations.RMDatabaseSettingKey.RECO_CONTROL_SQL_CONNECTION_STRING] };
            try
            {
                SendMessageAsync(jobMessage);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return RecordsConstants.Explorer_RealTime_Failed_All;
            }
            return RecordsConstants.Explorer_RealTime_Success;
        }

        private void SendMessageAsync(RecordsRealTimeMessage jobMessage)
        {
            jobMessage.ClientIP = ClientRequestLocalValue.ClientIP;
            if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        logger.Info($"Run real time action locally in dev. LogonGroupId : {jobMessage.LogonGroupId}, Action: {jobMessage.Action.ToString()}, JobId:  {jobMessage.JobId}");
                        await ProcessRealTimeMessageAsync(jobMessage);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Run real time action locally failed. JobId: {jobMessage.JobId}", ex);
                    }
                });
                return;
            }

            System.Threading.Tasks.Task.Run(() =>
            {                
                logger.Info($"Send  message to service bus. LogonGroupId : {jobMessage.LogonGroupId}, Action: {jobMessage.Action.ToString()}, JobId:  {jobMessage.JobId}");
                SendMessageToCloud(jobMessage);              
            });
        }

        private async Task ProcessRealTimeMessageAsync(RecordsRealTimeMessage msg)
        {
            TenantLocalValue.LogonGroupId = msg.LogonGroupId;
            TenantLocalValue.LogonUserEmail = msg.CurrentUserName;
            ClientRequestLocalValue.ClientIP = msg.ClientIP;
            logger.Info($"Try to process real time action message locally. LogonGroupId: {msg.LogonGroupId}, Action: {msg.Action.ToString()}, job id: {msg.JobId}");

            if (msg.Action == RealTimeAction.ChangeTerm)
            {
                await ChangeTermRealTimeAllSourceAsync(msg.ChangeTermOption, msg.JobId);
            }
            else if (msg.Action == RealTimeAction.Declare)
            {
                await DeclareAsRecordRealTimeAsync(msg.RecordIds, msg.JobId, msg.DeclareBy);
            }
            else if (msg.Action == RealTimeAction.UnDeclare)
            {
                await UndeclareAsRecordRealTimeAsync(msg.RecordIds, msg.JobId, msg.DeclareBy);
            }
            else if (msg.Action == RealTimeAction.PhysicalMove)
            {
                await PhysicalExplorerMoveRealTimeAsync(msg.PhysicalMoveOption, msg.JobId);
            }
            else if(msg.Action == RealTimeAction.PhysicalMoveRequest)
            {
                RecordsReturnMessage messageResult = new RecordsReturnMessage
                {
                    ResultType = ResultType.Success,
                };
                foreach (var moveRequest in msg.PhysicalMoveRequests)
                {
                    await PhysicalExplorerMoveRealTimeAsync(moveRequest.PhysicalMoveOption, msg.JobId, moveRequest.GroupRequestId);
                }
            }
            else if (msg.Action == RealTimeAction.GlobalSearchAction && msg.GlobalSearchInfo != null)
            {
                var globalSearchInfo = msg.GlobalSearchInfo;
                var action = GlobalSearchActionFactory.GetGlobalSearchAction(globalSearchInfo.Action);
                List<Contract.Explorer.BaseRecordDto> records = new List<Contract.Explorer.BaseRecordDto>();
                foreach (var id in globalSearchInfo.RecordIds)
                {
                    records.Add(new Contract.Explorer.BaseRecordDto()
                    {
                        NodeId = id,
                        Id = id
                    });
                }
                await action.DoActionAsync(records, (SourceFlag)globalSearchInfo.SourceFlag, globalSearchInfo.ActionExtension, msg.JobId, false);
            }
        }

        /*private void SendOnPremiseJobMessage(AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage)
        {

        }*/

        private void SendMessageToCloud(RecordsRealTimeMessage jobMessage)
        {
            var maxRetryTimes = 3;
            var retryTimes = 0;
            while (retryTimes < maxRetryTimes)
            {
                try
                {
                    QueueMessageUtilFactory.GetUtil(QueueMessageType.RealTime).SendMessage(jobMessage);
                    break;
                }
                catch (Exception e)
                {
                    logger.Info($"Will retry to send real time action message to cloud, max retry times : {maxRetryTimes}, current retry times : {++retryTimes}");
                    System.Threading.Thread.Sleep(1000);
                }
            }


        }

        /*private void SendMessageToListener(RecordsRealTimeMessage jobMessage, IARecordsListener ARecordsService)
        {
            try
            {
                ResultBase result = ARecordsService.RealTimeAction(jobMessage);
            }
            catch (System.ServiceModel.EndpointNotFoundException e)
            {
                logger.Warn("send listener error:{0}", e.Message);

                int retryMaxCount = 3;
                int retryIndex = 0;
                while (true)
                {
                    try
                    {
                        retryIndex++;
                        if (retryIndex > retryMaxCount)
                        {
                            logger.Error("retry count 3 can not connect to listener.");
                            break;
                        }
                        System.Threading.Thread.Sleep(1000);
                        ResultBase result = ARecordsService.RealTimeAction(jobMessage);
                        logger.Warn("send listener retry {0} count success.", retryIndex);
                        break;
                    }
                    catch (Exception)
                    {
                        logger.Warn("retry count: {0} failed.", retryIndex);
                    }
                }
            }
        }*/

        //to do next get sp object data.
        //public BaseRecordDto GetObjectData(Guid scopeId, Guid spObjectId)
        //{
        //    BaseRecordDto dto = null;
        //    try
        //    {
        //        var data = CollectionDataDao.GetRecordByNodeId(scopeId, spObjectId);
        //        if (data != null)
        //        {
        //            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v.DisplayName);
        //            dto = ConvertUtil.ConvertToBaseRecordDto(data, accountMap);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //logger.Error("get data by id:{0}, archived:{1}, error:{2}", key.ToString(), isArchived, ex.ToString());
        //    }
        //    return dto;
        //}
        public async Task<RecordDetailDto> LoadDetailByKeyAsync(int status, Guid id, ExplorerDetailTab tab, bool isControlPlus = false)
        {
            RecordDetailDto detail = new RecordDetailDto();
            try
            {
                var data = ExplorerDao.QueryAll(r => r.Id == id).First();
                //var data = CollectionDataDao.GetDataById(isArchived, id);

                if (data.SourceFlag == (int)SourceFlag.SharePoint || data.SourceFlag == (int)SourceFlag.SharePointOnPrem || data.SourceFlag == (int)SourceFlag.OneDrive || data.SourceFlag == (int)SourceFlag.Teams)
                {
                    var dicMap = RMScopeDao.GetScopeInfoByIds(new List<Guid>() { data.ScopeId });
                    if (dicMap.ContainsKey(data.ScopeId))
                    {
                        var sPath = dicMap[data.ScopeId];
                        data.FullPath = WebUtil.MakeFullUrl(sPath?.FullPath, data.DirPath);
                    }
                    else
                    {
                        //RECO-2576
                        SharePointSettingUtility SPUtility = new SharePointSettingUtility();
                        var site = SPUtility.GetRemoteSiteCollection(data.AveSiteId.ToString());
                        data.FullPath = site == null ? string.Empty : WebUtil.MakeFullUrl(site.url, data.DirPath);
                        logger.Info("get site info from dao:siteId:{0}, siteUrl:{1},path:{2}", data.AveSiteId.ToString(), site?.url, data.FullPath);
                        if (site != null)
                        {
                            RMScopeDao.AddOrUpateSiteScope(new RMScope()
                            {
                                FullPath = site.url,
                                ScopeId = data.ScopeId,
                                ScopeName = site.Name,
                                IsRemoved = false,
                            });
                        }
                    }
                }
                switch (tab)
                {
                    case ExplorerDetailTab.All:
                        detail.Summary = await GetSummaryInfoAsync(data);
                        detail.GeneralProperty = await GetGeneralPropertyAsync(data, isControlPlus);
                        detail.ManualReviewInfo = await GetManualReviewInfoAsync(data);
                        detail.RelatedRecordInfo = await GetRelatedRecordInfoAsync(data.RelatedRecords);
                        detail.RecordHistory = await GetHistoryInfoAsync(data.RecordHistory, id, isControlPlus);
                        break;
                    case ExplorerDetailTab.Summary:
                        detail.Summary = await GetSummaryInfoAsync(data);
                        detail.Record = await GetBaseRecordDtoAsync(data);
                        break;
                    case ExplorerDetailTab.Property:
                        detail.GeneralProperty = await GetGeneralPropertyAsync(data, isControlPlus);
                        break;
                    case ExplorerDetailTab.RelatedRecord:

                        //if (data.SourceFlag == (int)SourceFlag.SharePoint)
                        //{
                        detail.RelatedRecordInfo = await GetRelatedRecordInfoAsync(data.RelatedRecords);
                        //}
                        //else if (data.SourceFlag == (int)SourceFlag.FileSystem)
                        //{
                        //    detail.RelatedRecordInfo = FSGetRelatedRecordInfo(data.Id);
                        //}
                        break;
                    case ExplorerDetailTab.History:
                        detail.RecordHistory = await GetHistoryInfoAsync(data.RecordHistory, id, isControlPlus);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("can not find detail type:{0}", tab));
                }

            }
            catch (Exception ex)
            {
                logger.Error("get detail by id:{0}, tabInfo:{1}, error:{2}", id, tab, ex.ToString());
            }
            return detail;
        }

        public AvePoint.Wrapper.Common.IAveSite GetIAveSite(string siteUrl)
        {
            var remoteSiteCollection = RABrowserClient.GetRemoteSiteCollectionByUrl(siteUrl);
            if (remoteSiteCollection == null)
            {
                return null;
            }

            var bposInfo = CommonPoolUserUtil.GetBPOSInfo(remoteSiteCollection);
            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, bposInfo, AvePoint.Wrapper.Common.AveContextKind.ClientObjectModel);
            var site = aveObjectModelFactory.CreateSite(siteUrl);
            return site;
        }

        public RecordDetailDto GetRelatedItemDetailsInfo(RelatedItemSubmitInfo submitInfo)
        {
            RecordDetailDto detailDto = new RecordDetailDto();
            RecordSummary detailsSummary = new RecordSummary();
            detailDto.Summary = detailsSummary;
            try
            {
                AvePoint.Wrapper.Common.IAveSite site = this.GetIAveSite(submitInfo.SiteUrl);
                var web = site.OpenWeb(submitInfo.WebId);
                var webServerRelativeUrl = web.ServerRelativeUrl;
                var webUrl = web.Url;
                AvePoint.Wrapper.Common.IAveListItem item = web.GetListItem(string.Empty, submitInfo.ListId, submitInfo.ListItemId);
                detailsSummary.SourceFlag = SourceFlag.SharePoint;

                if (item.FieldValues.TryGetValue("FileRef", out object value4FileRef))
                {
                    string fileRef = value4FileRef.ToString();
                    detailsSummary.FullPath = WebUtil.MakeFullUrl(submitInfo.SiteUrl, fileRef);
                    if (detailsSummary.FullPath.EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                    {
                        detailsSummary.FullPath = WebUtil.GetListItemRealPath(detailsSummary.FullPath);
                    }
                }
                if ((item.FieldValues["FSObjType"] as string).Equals(((int)Microsoft.SharePoint.Client.FileSystemObjectType.File).ToString()))
                {
                    if ((item.FieldValues["FileLeafRef"] as string).EndsWith("_.000", StringComparison.OrdinalIgnoreCase))
                    {
                        detailsSummary.LeafName = item.FieldValues["Title"] as string;
                        if (string.IsNullOrEmpty(detailsSummary.LeafName))
                        {
                            detailsSummary.LeafName = RelatedRecordsUtility.GetSpecialListItemName(item);
                        }
                    }
                    else
                    {
                        detailsSummary.LeafName = item.FieldValues["FileLeafRef"].ToString();
                    }
                }
                else
                {
                    detailsSummary.LeafName = item.FieldValues["FileLeafRef"].ToString();
                }

                if (item.FieldValues.TryGetValue(SPColumnConstants.DocumentId, out object value4DocumentId))
                {
                    detailsSummary.RecordId = value4DocumentId?.ToString();
                }
                else if (item.FieldValues.ContainsKey(RcordsBuiltInColumn.UNIQUEID_NAME))
                {
                    detailsSummary.RecordId = item.FieldValues[RcordsBuiltInColumn.UNIQUEID_NAME]?.ToString();
                }

                try
                {
                    var columnInternalName = RcordsBuiltInColumn.ITEM_BCS_NAME;
                    var remoteSiteCollection = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(submitInfo.SiteUrl);
                    if (remoteSiteCollection != null)
                    {
                        var containerNodeSettings = SharePointSettingDao.LoadSharePointSetting(new Guid(remoteSiteCollection.parentId), Guid.Empty);
                        if (containerNodeSettings.IsUsingExistColumnName)
                        {
                            var collection = item.Fields;
                            var tempField = collection.Where(f => f.Title == containerNodeSettings.ExistColumnName).FirstOrDefault();
                            tempField ??= collection.Where(f => f.InternalName == containerNodeSettings.ExistColumnName).FirstOrDefault();
                            if (tempField == null)
                            {
                                string staticName = SPCommonUtility.GetSiteLevelExistColumnStaticName(site, containerNodeSettings.ExistColumnName);
                                tempField ??= collection.Where(f => f.StaticName == staticName).FirstOrDefault();
                            }
                            if (tempField == null)
                            {
                                logger.Warn($"[RelatedApp] Can not get column by name. site: {submitInfo.SiteUrl}, exist colum name: {containerNodeSettings.ExistColumnName}");
                            }
                            else
                            {
                                columnInternalName = tempField.InternalName;
                            }
                        }
                    }

                    if (item.FieldValues.TryGetValue(columnInternalName, out object termTempValue))
                    {
                        var termString = termTempValue?.ToString();
                        var termId = termString?.Split('|')?.LastOrDefault();
                        if (!string.IsNullOrEmpty(termId))
                        {
                            detailsSummary.Term = TermDao.GetRMTermWithPathByTermId(new Guid(termId))?.FullPath;
                            var termInfo = TermDao.GetParentInhertSetting(new Guid(termId));
                            var isEnableRententionLabel = (termInfo.EnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint;
                            detailsSummary.TermSettings = isEnableRententionLabel ? $"{I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionStatus")}, {I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionLabel")}: {termInfo.SPRetentionLabel}" : "";
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Error occurred while gettting related item term, error: {e}");
                }
                detailsSummary.DeclareAsRecord = item.IsBlockEditAndDeleteRecord();
                return detailDto;
            }
            catch (Exception e)
            {
                logger.Warn($"Error occurred while gettting related item. Id:{submitInfo.ListItemId},Error:{e}");
            }
            return null;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.SpfxManageRelatedRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage SubmitRelatedItems(RelatedItemSubmit saveInfo)
        {
            RAReturnMessage rstMsg = new RAReturnMessage();
            var commonErrorMsg = I18NEntity.GetString("RM_JS_BCM_Explorer_ManageRelatedRecordsApplyError");
            rstMsg.MessageType = RAMessageType.Successful;
            try
            {
                var currentItemInfo = saveInfo?.CurrentInfo;
                var utility = new RelatedRecordsUtility(currentItemInfo.SiteUrl, currentItemInfo.WebId, currentItemInfo.ListId, currentItemInfo.ListItemId);
                var itemInfos = new List<RMRelatedItemInfo>();
                var deletedItemInfos = new List<RelatedItemSubmitInfo>();
                foreach (var submitInfo in saveInfo.RelatedInfos)
                {
                    //TODO Cyrus related infos needs group by site list, then can batch action
                    var itemInfo = utility.GetRelatedItemInfo(submitInfo);
                    if(itemInfo == null)
                    {
                        logger.Warn($"Related item is null, will not process it. ListItemId:{submitInfo.ListItemId}");
                        deletedItemInfos.Add(submitInfo);
                        continue;
                    }
                    itemInfos.Add(itemInfo);
                }
                var relatedChangedInfos = utility.UpdateRelatedPropertiesForApp(itemInfos, deletedItemInfos, isSpfxApp: true);
                rstMsg.Extsion1 = relatedChangedInfos;
            }
            catch (RelatedRecordsAppDisableExcetion re)
            {
                logger.Error("remove realted records for sp error:{0}", re.ToString());
                rstMsg.MessageType = RAMessageType.Failed;
                rstMsg.ErrorMessage = re.Message;
            }
            catch (Exception e)
            {
                logger.Error("remove realted records for sp error:{0}", e.ToString());
                rstMsg.MessageType = RAMessageType.Failed;
                rstMsg.ErrorMessage = commonErrorMsg;
            }
            return rstMsg;
        }

        [Audit(Module = AuditModule.DownloadCenter, Category = AuditCategory.DownloadCenter, Action = AuditAction.StartDownloadArchivedContentJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage StartRestoreArchivedContent(List<Guid> ids)
        {
            logger.Info($"Begin to restore archived content. Id:{string.Join(",", ids)}");
            RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            string jobId = string.Empty;
            try
            {
                var record = ExplorerDao.QueryAll(r => ids.Contains(r.Id) && r.RecordStatus == (int)RMRecordStatus.Archived).FirstOrDefault();
                if (record != null)
                {
                    var metaInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                    if(metaInfo == null)
                    {
                        throw new Exception("no metaInfo");
                    }
                    //var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(record.AveSiteId);
                    var flagType = (SourceFlag)record.SourceFlag switch
                    {
                        SourceFlag.SharePoint => NodeFlagType.ExplorerSync,
                        SourceFlag.Teams => NodeFlagType.TeamsSync,
                        SourceFlag.OneDrive => NodeFlagType.OneDriveExplorerSync,
                        _ => throw new Exception($"Not support content source {(SourceFlag)record.SourceFlag}"),
                    };
                    var siteInfo = NodeFlagDao.GetNodeFlagInfoById(new Guid(record.AveSiteId), flagType);
                    if (siteInfo != null)
                    {
                        string fullUrl = WebUtil.MakeFullUrl(siteInfo.FullPath, record.DirPath);
                        if (!string.IsNullOrWhiteSpace(siteInfo?.FullPath) && (!string.IsNullOrWhiteSpace(metaInfo.ArchiverIndex) || !string.IsNullOrWhiteSpace(fullUrl)))
                        {
                            if (DownloadDataInfoDao.ExistAvailableJob(record.Id))
                            {
                                logger.Info("Already has an archived content download job running. No need to run job.");
                            }
                            else
                            {
                                if (TenantService.IsNewOpusTenant())
                                {
                                    DocAveOnline.WebApi.Contracts.ArchivedContentRestoreConfig info = new DocAveOnline.WebApi.Contracts.ArchivedContentRestoreConfig()
                                    {
                                        SiteUrl = siteInfo.FullPath,
                                        ArchivedContentInfos = new List<DocAveOnline.WebApi.Contracts.ArchivedContentInfo>()
                                    {
                                        new DocAveOnline.WebApi.Contracts.ArchivedContentInfo()
                                        {
                                            BackUpJobId = metaInfo.BackUpJobId,
                                            PathMD5 = metaInfo.PathMD5,
                                            FileUrl = fullUrl,
                                            ExtensionString = JsonConvert.SerializeObject(record)
                                        }
                                    },
                                    };
                                    jobId = ControlArchiverService.RunArchiverContentDownloadJob(info).Jobs.First().Id;
                                }
                                else
                                {
                                    var client = new DAOAPIClientV1();
                                    ArgumentCheck.NotNull(metaInfo, nameof(metaInfo));
                                    jobId = client.StartDownloadArchivedContent(siteInfo.FullPath, metaInfo.PathMD5, metaInfo.BackUpJobId, fullUrl, metaInfo.ArchiverIndex);
                                }
                                
                            }
                        }
                        else
                        {
                            logger.Error($"Some parameter is null, cannot download archived content. SiteUrl:{siteInfo?.FullPath} FileUrl:{fullUrl} Index Exist:{!string.IsNullOrWhiteSpace(metaInfo?.ArchiverIndex)}");
                        }
                    }
                    else
                    {
                        logger.Error($"Site not found, will not download archived content. SiteId:{record.AveSiteId}");
                    }
                }
                else
                {
                    logger.Error($"Cannot find archived records. Ids:{string.Join(",", ids)}");
                }
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    message.MessageType = RAMessageType.Failed;
                }
                else
                {
                    message.Extension = jobId;
                }
                return message;
            }
            catch (Exception ex)
            {
                logger.Error("An error accourd while download file in search", ex);
                message.MessageType = RAMessageType.Failed;
                message.ErrorMessage = "An error accourd while download in search";
                return message;
            }
        }

        public void StartFCJob()
        {
            string jobId = RMJobService.CreateJob(JobType.DataSynchronisation, TenantLocalValue.DisplayName);
            RealStartCollectionJob(jobId, JobType.DataSynchronisation);

        }
        private async Task ProcessHistoryItemSucceed(Record item)
        {
            try
            {
                var tempStatus = GetHistoryApprovalStatus(item);
                if (tempStatus != SOApproveDBStatus.None)
                {
                    var historyDataApprovalStatus = tempStatus;
                    var historyData = AddAction.ConvertForFS(item, historyDataApprovalStatus, item.ManualApprovedBy, item.ManualActionTime);
                    await AddAction.AddAsync(historyData);
                    logger.Info($"Succeed insert fs item [{item.Id}] to history table.");
                }
                else
                {
                    logger.Warn($"The fs item [{item.Id}] is not need to insert to history table.");
                }
                await ManualApprovalService.MarkApprovalingObjectsToExportedStatusForFSAsync(LocalAzConnectStr, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.ManualRowKey);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while process history item succeed. Error: {e}");
            }
        }
        private SOApproveDBStatus GetHistoryApprovalStatus(Record item)
        {
            if ((SOApproveDBStatus)item.ManualApprovedStatus == SOApproveDBStatus.Archived)
            {
                return SOApproveDBStatus.Approved;
            }
            else if ((SOApproveDBStatus)item.ManualApprovedStatus == SOApproveDBStatus.WaitingApprove)
            {
                if ((SOApproveDBStatus)item.ManualApprovedStatusForHistory == SOApproveDBStatus.Rejected)
                {
                    return SOApproveDBStatus.Rejected;
                }
            }
            else if ((SOApproveDBStatus)item.ManualApprovedStatus == SOApproveDBStatus.Rejected)
            {
                return SOApproveDBStatus.Rejected;
            }
            return SOApproveDBStatus.None;
        }
        private void ProcessHistoryItemFailed(Record item, string errorMessage)
        {
            logger.Error($"Failed insert fs item [{item.Id}] to history table. Error: {errorMessage}");
        }
        private Task SucceedProcessRecord(Record item)
        {
            logger.Info($"Succeed process record. Source: [{(SourceFlag)item.SourceFlag}], Id: [{item.Id}], Container id: [{item.ContainerId}], Node id: [{item.NodeId}].");
            if (ProcessItemFailedCallback == null)
            {
                return Task.CompletedTask;
            }
            return ProcessItemSucceedCallback.Invoke(item);
        }

        private void FailedProcessRecord(Record item, Exception e)
        {
            ProcessItemFailedCallback?.Invoke(item, e.Message);
        }
        public void RunSendEmailJobAsync(string jobId)
        {
            EmailManagementService.SendEmailJobMessageToQueue(jobId);
        }
        public async Task AddArchiverItemsForFSAsync(string tenantGroupId, List<FSAzureTableEntityDto> dtos,string jobId, bool isFSHighPerformanceMode = false)
        {
            s_emailSender = new(new RMEmailRedisStorage(jobId, new RMEMailStorageManualMiddleware()));
            List<FileSystemTableEntity> entities = new List<FileSystemTableEntity>();
            List<ManualExportReportInfo> reportInfos = new List<ManualExportReportInfo>();
            var BulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (BulkSize <= 0)
            {
                BulkSize = CosmosBulkOperator.DefualtBufferSize;
            }
            RegisteProcessItemCallback(ProcessHistoryItemSucceed, ProcessHistoryItemFailed);
            CosmosOperator.Start(BulkSize, SucceedProcessRecord, FailedProcessRecord);
            logger.Info("run adjust storage size schedule job.");
            var key = RMKeyValueDao.GetValueByKey("FSInsertManualToCosmosByDisposal");
            if (key == null)
            {
                RMKeyValueDao.Save(new RMKeyValue() { Key = "FSInsertManualToCosmosByDisposal", Value = "true" });
                logger.Info("not exist FSInsertManualToCosmosByDisposal,create it as true");
            }
            Settings = FileSytemSettingDao.FindAll().OrderByDescending(item => item.FullPath).ToList();
            Connections = FSConnectionDao.FindAll().OrderByDescending(item => item.UNCPath).ToList();
            foreach (var dto in dtos)
            {
                entities.Add(ConvertUtil.ConvertFSDto2ArchiverTableEntity(dto));
            }

            foreach (var temp in entities)
            {
                reportInfos.Add(RMArchiverItemConverter.ConvertToReportInfo(temp));
            }
            foreach (var report in reportInfos)
            {

                (var hasRule, var ruleInfo) = await TryGetAsync(report.RuleID);
                if (!hasRule)
                {
                    logger.Warn("Failed to load rule info, failed report {0}", report.ScopeID);
                    continue;
                }
                try
                {
                    using (new PerformanceScope($"ManualApproval:LoadSetting"))
                    {

                        var settingInfo = GetReportRelateSettingInfo(report);
                        if (settingInfo.IsEnableSettingManualApproval)
                        {
                            ruleInfo.EnableManualApproval = settingInfo.IsEnableSettingManualApproval;
                            ruleInfo.WorkflowId = settingInfo.WorkflowId;
                            ruleInfo.IsSendEmailToOwner = settingInfo.IsSendEmialToOwner;
                            ruleInfo.Owners = settingInfo.Owners;
                        }
                        logger.Info($"The [{SourceFlag.FileSystem}] current manual approval report is enable setting manual approval: [{settingInfo.IsEnableSettingManualApproval}], approval type: [{ruleInfo.ManualApprovalType}], workflow id: [{ruleInfo.WorkflowId}], is send email: [{ruleInfo.IsSendEmailToOwner}].");
                    }

                    PerProcessManualApprovalReport(report);
                    var manualApprovalRecord = BasicConvertReportToManualAprovalRecord(report, ruleInfo);
                    if(!ruleInfo.EnableManualApproval)
                    {
                        logger.Info($"The [{SourceFlag.FileSystem}] current manual approval report manual approval is disabled by setting.");
                        continue;
                    }
                    report.IsFSHighPerformanceMode = isFSHighPerformanceMode;
                    if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
                    {
                        //这里用的是Reference ID 获取最新version的Definition
                        var workflowInfoDef = Get(ruleInfo.WorkflowId);
                        var workflowInstance = await s_workflowProcessor.LoadAsync(workflowInfoDef.Id);
                        var step = workflowInstance.Start();
                        manualApprovalRecord.ManualWorkflowStepId = step.Id;
                        manualApprovalRecord.ManualWorkflowDefinitionId = workflowInfoDef.Id;
                        if (report.IsFSHighPerformanceMode && report.DestroyedTime > 0)
                        {
                            manualApprovalRecord.RecordStatus = (int)report.RecordStatus;
                            manualApprovalRecord.DestroyedTime = report.DestroyedTime;
                        }
                        await ProcessManualApprovalReportByWorkflowNewAsync(report, manualApprovalRecord, step, ruleInfo, workflowInstance.HasStepUsedSiteOwnerApprovalMode(), workflowInstance.HasStepUsedInfomationOwnerApprovalMode());
                    }
                    else if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
                    {
                        ProcessManualApprovalReportByOwner(report, ruleInfo);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while process fs manual approval report Failed. PartKey: [{report.PartKey}], RowKey: [{report.RowKey}]. Error: {e}");
                }
            }
            CosmosOperator.Complete();
            CosmosOperator.Reset();
        }
        private void RegisteProcessItemCallback(Func<Record, Task> processItemSucceed, Action<Record, string> processItemFailed)
        {
            ProcessItemSucceedCallback = processItemSucceed;
            ProcessItemFailedCallback = processItemFailed;
        }
        protected void ProcessManualApprovalReportByOwner(ManualExportReportInfo manualApprovalReport, ManualApprovalRuleModel ruleInfo)
        {
            if (ruleInfo.Owners.Count == 0)
            {
                logger.Error($"The current manual approval report onwers is not set. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}].");
                return;
            }


            var manualApprovalRecord = BasicConvertReportToManualAprovalRecord(manualApprovalReport, ruleInfo);
            var ownerIds = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners);

            manualApprovalRecord.ManualWorkflowDefinitionId = Guid.Empty;
            manualApprovalRecord.ManualWorkflowStepId = Guid.Empty;
            manualApprovalRecord.ManualWorkflowInstanceId = Guid.Empty;
            //manualApprovalRecord.ManualInternalApprovedStatus = (int)Contract.SOApproveDBStatus.WaitingApprove;
            manualApprovalRecord.ManualInternalApprovedStatus = (int)Contract.SOApproveDBStatus.WaitingApprove;
            manualApprovalRecord.ManualReviewer = ownerIds.ToArray();
            manualApprovalRecord.ManualReviewerForHistory = ownerIds.ToArray();
            CosmosOperator.Add(manualApprovalRecord);

            if (ruleInfo.IsSendEmailToOwner && manualApprovalRecord.ManualApprovedStatus != (int)SOApproveDBStatus.Archived)
            {
                foreach (var owner in ruleInfo.Owners)
                {
                    s_emailSender.Add(RMEmailTemplateId.MANUAL_APPROVAL, new RMManualEmailTemplateParameters
                    {
                        UserId = owner.UserId,
                        ToUser = owner.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual
                    });
                }
            }
        }
        private async System.Threading.Tasks.Task ProcessManualApprovalReportByWorkflowNewAsync(ManualExportReportInfo manualApprovalReport, Record manualApprovalRecord, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step, ManualApprovalRuleModel ruleInfo, bool usedSiteOwnerMode, bool usedInformationOwnerMode)
        {
            if (usedSiteOwnerMode)
            {
                logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has use site owner reviewer step.");
                var message = $"not surpport siteOwner for fs";
                throw new Exception(message);
            }
            var reviewers = new List<ReviewerUser>();
            var ownerInfors = new List<RMAccount>();
            if (usedInformationOwnerMode)
            {
                logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has use information owner reviewer step.");
                var connectionInfo = Connections.FirstOrDefault(conn => conn.Id.ToString() == manualApprovalRecord.AveSiteId);
                if (connectionInfo == null)
                {
                    logger.Warn($"Connection not found for AveSiteId: {manualApprovalRecord.AveSiteId}");
                    return;
                }
                logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has use information owner reviewer step.");
                var inforOwners = FSConnectionOwnerDao.GetOwnersByConnectionId(connectionInfo.Id, FSConnectionOwnerType.InformationOwner).ToList();
                var inforOwnerLookup = inforOwners.ToDictionary(x => x.UserIntId, x => x);
                ownerInfors = await AccountDao.GetUserByIdsAsync(inforOwners.Select(infor => infor.UserIntId).ToList());
                logger.Info($"Get information owner reviewers for workflow: [{ruleInfo.WorkflowId}] and connection id: [{connectionInfo.Id}]. Reviewer count: {reviewers.Count}.");
                await ProcessWorkflowInformationOwnersAsync(ownerInfors, connectionInfo.Id, ruleInfo.WorkflowId);
            }
            if (manualApprovalRecord.IsFsControlRecordJPMC)
            {
                reviewers = await step.GetReviewersAsync(new Guid(manualApprovalRecord.AveSiteId));
            }
            else
            {
                reviewers = await step.GetReviewersAsync(manualApprovalRecord.ScopeId);
            }
            var templateId = step.UsedEmailTemplateId;
            manualApprovalRecord.ManualReviewerForHistory = manualApprovalRecord.ManualReviewer?.Length > 0 ? manualApprovalRecord.ManualReviewer : reviewers.Select(item => item.RMUserId).ToArray();
            manualApprovalRecord.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            var status = GetHistoryApprovalStatus(manualApprovalRecord);
            if(status != SOApproveDBStatus.None)
            {
                var lastStep = step.GetLastStep();
                if (manualApprovalRecord.IsFsControlRecordJPMC)
                {
                    var lastReviewers = await lastStep.GetReviewersAsync(new Guid(manualApprovalRecord.AveSiteId));
                    manualApprovalRecord.ManualReviewerForHistory = lastReviewers.Select(item => item.RMUserId).ToArray();
                }
                else
                {
                    var lastReviewers = await lastStep.GetReviewersAsync(manualApprovalRecord.ScopeId);
                    manualApprovalRecord.ManualReviewerForHistory = lastReviewers.Select(item => item.RMUserId).ToArray();
                }
            }

            if (step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
            {
                var customIntervalSetting = step.CustomIntervalSettings[0];
                if (customIntervalSetting == null)
                {
                    templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                }
                else
                {
                    templateId = new Guid(customIntervalSetting.UsedEmailTemplateId);
                    if (templateId == Guid.Empty)
                    {
                        templateId = RMEmailTemplateId.MANUAL_APPROVAL;
                    }
                }

            }
            if (ruleInfo.IsSendEmailToOwner && manualApprovalRecord.ManualApprovedStatus != (int)SOApproveDBStatus.Archived)
            {
                foreach (var reviewer in reviewers)
                {
                    s_emailSender.Add(templateId, new RMManualEmailTemplateParameters
                    {
                        UserId = reviewer.UserId,
                        ToUser = reviewer.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual
                    });
                }
            }

            //manualApprovalRecord.ManualInternalApprovedStatus = (int)Contract.SOApproveDBStatus.WorkflowInProgress;
            manualApprovalRecord.ManualInternalApprovedStatus = (int)Contract.SOApproveDBStatus.WorkflowInProgress;
            CosmosOperator.Add(manualApprovalRecord);
        }

        private async Task ProcessWorkflowInformationOwnersAsync(List<RMAccount> ownerInfors, Guid connectionId, string workflowId)
        {
            // Implementation for processing workflow information owners
            //return await ManualApprovalWorkflowManager.SyncInformationOwnerToWorkflowInfomationOwnersAsync(workflowId, reportInfo, siteId);
            logger.Info($"Start to sync information owners to workflow information owners. WorkflowId: [{workflowId}], ConnectionId: [{connectionId}], Owner count: {ownerInfors.Count}.");
            await ManualApprovalWorkflowManager.SyncInformationOwnerToWorkflowInfomationOwnersAsync(ownerInfors, connectionId, workflowId);
        }

        public WorkflowDefinitionDto Get(string workflowRefernceId)
        {
            if (!Workflows.TryGetValue(workflowRefernceId, out var workflow))
            {
                workflow = ManualProcessManagementService.GetWorkflow(Guid.Parse(workflowRefernceId));
                if (!Workflows.TryAdd(workflowRefernceId, workflow))
                {
                    logger.Warn($"Add workflow: [{workflowRefernceId}] to cache failed");
                }
            }
            return workflow;
        }
        public bool TryGet(Expression<Func<Record, bool>> predicate, out Record record)
        {
            record = ExplorerDao.GetFirstOrDefault(predicate);
            return record != null;
        }
        private Expression<Func<Record, bool>> GetQueryItemExpression(Record data)
        {
            return (record) => record.Id == data.NodeId;
        }
        protected Record BasicConvertReportToManualAprovalRecord(ManualExportReportInfo manualApparovalReport, ManualApprovalRuleModel ruleInfo)
        {
            using (new PerformanceScope("ManualApproval:GetItem", "", true))
            {
                var basicRecord = new Record();
                basicRecord = ConvertReportToManualApprovalRecord(manualApparovalReport, basicRecord);
                if (!TryGet(GetQueryItemExpression(basicRecord), out var record))
                {
                    record = new Record
                    {
                        Id = basicRecord.Id,
                        RecordStatus = (int)RMRecordStatus.ManualPreSync,
                        CreateDate = manualApparovalReport.CreatedTime > 0 ?
                        int.Parse(new DateTime(manualApparovalReport.CreatedTime).ToString("yyyyMMdd")) : 0,
                        ManualExtendCount = 0,
                    };
                }
                else
                {
                    if (!ruleInfo.RuleId.ToString().Equals(record.RuleId.ToString()))
                    {
                        record.ManualExtendCount = 0;
                    }
                }
                if (record.CreateDate == 0)
                {
                    record.CreateDate = PartitionKey;
                }
                record.ManualModifiedTime = manualApparovalReport.ModifiedTime;
                record.ManualRelatedRecords = basicRecord.ManualRelatedRecords;
                record.NodeId = manualApparovalReport.NodeID;
                record.IsManualSynced = true;
                record.LeafName = manualApparovalReport.LeafName;
                record.NodeType = ConvertObjectLevelToNodeLevel(manualApparovalReport.ObjectLevel);
                record.RuleId = new Guid(ruleInfo.RuleId);
                record.ManualRuleName = ruleInfo.RuleName;
                record.ManualRuleCriteria = ruleInfo.RuleCriterias;
                record.ManualRuleDisposalClass = ruleInfo.RuleDisposalClass;
                record.ExtensionForFile = GetFileExtension(manualApparovalReport, record);
                record.SourceFlag = (int)SourceFlag.FileSystem;
                record.ManualActionTime = DateTime.UtcNow.Ticks;
                record.ManualApprovedBy = record.ManualApprovedBy ==0? manualApparovalReport.ManualApprovalBy: record.ManualApprovedBy;
                record.ManualApprovedStatus = manualApparovalReport.Status == Contract.SOApproveDBStatus.None? (int)Contract.SOApproveDBStatus.WaitingApprove: (int)manualApparovalReport.Status;
                record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
                record.ManualFullPath = manualApparovalReport.Path;
                record.ManualFolderPath = manualApparovalReport.FolderPath;
                record.ManualSiteUrl = manualApparovalReport.SiteUrl;
                record.ManualEscalateFrom = manualApparovalReport.ManualEscalateFrom;
                record.ManualEscalatedComment = "";
                record.ManualExtendTime = 0;
                record.ManualExtendComment = "";
                record.ManualCollectionTime = DateTime.UtcNow.Ticks;
                record.ManualArchivedTime = 0;
                record.ManualPartitionKey = manualApparovalReport.PartKey;
                record.ManualRowKey = manualApparovalReport.RowKey;
                record.ManualVersion = GetVersion(manualApparovalReport.UIVersion);
                record.ManualIsRelatedRecords = manualApparovalReport.HasRelatedDocument > 0;
                record.ManualRelatedRecordsAction = manualApparovalReport.DeleteRelatedRecords;
                record.ManualNeedEmailNotification = ruleInfo.IsSendEmailToOwner;
                record.ManualEmailNotificationCount = 0;
                record.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                //record.ManualExtendCount = 0;
                record.ManualIsAutoReassigned = false;
                record.ManualRetentionStatus = manualApparovalReport.RetentionStatus;
                record.ManualLastExtendType = ManualApprovalExtendType.After1Month;
                record.ManualLastCustomeExtendDate = DateTime.UtcNow;
                record.ManualApprovedStatusForHistory = manualApparovalReport.InternalStatus == (int)Contract.SOApproveDBStatus.None ? (int)Contract.SOApproveDBStatus.WaitingApprove : (int)manualApparovalReport.InternalStatus;
                if (string.IsNullOrEmpty(record.CreatedBy))
                {
                    record.CreatedBy = manualApparovalReport.CreatedBy;
                }

                if (!string.IsNullOrEmpty(record.CreatedBy))
                {
                    if (record.CreatedBy.StartsWith("i:0#.f|membership|"))
                    {
                        record.CreatedBy = record.CreatedBy.Substring("i:0#.f|membership|".Length);
                    }
                    if (record.CreatedBy.StartsWith("i:0i.t|00000003-0000-0ff1-ce00-000000000000|"))
                    {
                        record.CreatedBy = record.CreatedBy.Substring("i:0i.t|00000003-0000-0ff1-ce00-000000000000|".Length);
                    }
                }

                if (string.IsNullOrEmpty(record.ModifiedBy) && !string.IsNullOrEmpty(manualApparovalReport.ModifiedBy))
                {
                    record.ModifiedBy = manualApparovalReport.ModifiedBy;
                }

                if (manualApparovalReport.IsFSHighPerformanceMode && manualApparovalReport.DestroyedTime > 0)
                {
                    record.RecordStatus = (int)manualApparovalReport.RecordStatus;
                    record.DestroyedTime = manualApparovalReport.DestroyedTime;
                }

                return record;
            }
        }
        private string GetVersion(int uiversion)
        {
            var version = string.Empty;
            if (uiversion > 0)
            {
                int majorVers = uiversion / 512;
                int minorVers = uiversion % 512;
                version = string.Format("{0}.{1}", majorVers, minorVers);
            }
            return version;
        }
        private static int ConvertObjectLevelToNodeLevel(RMReportObjectLevel objectLevel)
        {
            var nodeLevel = RMNodeLevel.Undefined;
            switch (objectLevel)
            {
                case RMReportObjectLevel.FSFolder:
                    nodeLevel = RMNodeLevel.FSFolder;
                    break;
                case RMReportObjectLevel.FSFile:
                    nodeLevel = RMNodeLevel.FSFile;
                    break;
            }

            return (int)nodeLevel;
        }
        private string GetFileExtension(ManualExportReportInfo data, Record record)
        {
            if (!string.IsNullOrEmpty(record.ExtensionForFile))
            {
                return record.ExtensionForFile;
            }

            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.FSFolder:
                    return "RM_RDM_RecordDetails_DataType_FSFolder";
                case RMNodeLevel.FSFile:
                    var fsExt = Path.GetExtension(data.LeafName);
                    if (fsExt.Contains('.', StringComparison.CurrentCulture))
                    {
                        return fsExt[1..];
                    }
                    return "";
            }


            return "";
        }
        private Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            record.Id = manualApprovalReportInfo.NodeID;
            record.ScopeId = new Guid(manualApprovalReportInfo.ScopeID);
            record.NodeId = manualApprovalReportInfo.NodeID;
            record.AveSiteId = manualApprovalReportInfo.SiteID.ToString();
            if (string.IsNullOrEmpty(manualApprovalReportInfo.RelatedRecordInfo))
            {
                logger.Warn($"The fs node: [{manualApprovalReportInfo.NodeID}] not has related record info.");
                return record;
            }

            try
            {
                var reportRelatedRecords = new List<ReportRelatedRecords>();
                var relatedRecordInfos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(manualApprovalReportInfo.RelatedRecordInfo);
                foreach (var relatedRecordInfo in relatedRecordInfos)
                {
                    var url = $"{relatedRecordInfo.name}";
                    reportRelatedRecords.Add(new ReportRelatedRecords { Name = relatedRecordInfo.id.ToString(), Url = relatedRecordInfo.url });
                }
                record.ManualRelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get fs node: [{manualApprovalReportInfo.NodeID}] related record info. Error: {e}");
            }
            return record;
        }
        private void PerProcessManualApprovalReport(ManualExportReportInfo manualApprovalReport)
        {
            if (manualApprovalReport.ObjectLevel == AvePoint.RA.Contract.RMWeb.ReportCenter.RMReportObjectLevel.SiteCollection)
            {
                manualApprovalReport.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
            }
            else if (manualApprovalReport.ObjectLevel == AvePoint.RA.Contract.RMWeb.ReportCenter.RMReportObjectLevel.ExchangeOnlineItem)
            {
                manualApprovalReport.ContentType = "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem";
            }
        }
        private async Task<(bool, ManualApprovalRuleModel)> TryGetAsync(string ruleId)
        {
            ManualApprovalRuleModel ruleInfo = null;

            try
            {
                if (string.IsNullOrEmpty(ruleId))
                {
                    throw new ArgumentNullException("[ruleId]");
                }

                if (!RuleInfos.TryGetValue(ruleId, out var sourceRules))
                {
                    sourceRules = await LoadRuleInfoAsync(ruleId);
                    if (!RuleInfos.TryAdd(ruleId, sourceRules))
                    {
                        logger.Warn($"The rule: [{ruleId}] add to memory cache failed.");
                    }
                }

                ruleInfo = sourceRules[SourceFlag.FileSystem].DeepCopy();

                return (true, ruleInfo);
            }
            catch (Exception e)

            {
                logger.Error($"An error occurred while get rule info by id: [{ruleId}]. Error: {e}");
                return (false, ruleInfo);
            }
        }
        private async Task<Dictionary<SourceFlag, ManualApprovalRuleModel>> LoadRuleInfoAsync(string ruleId)
        {
            Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ManualApprovalRuleModel>> SourceGetRuleMethods =
    new Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ManualApprovalRuleModel>>
    {
                { SourceFlag.FileSystem, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.FSRule)},
    };
            using (new PerformanceScope($"Load rule: [{ruleId}]"))
            {
                var result = new Dictionary<SourceFlag, ManualApprovalRuleModel>();

                var rule = await RuleManagerService.LoadRuleAsync(ruleId);
                if (rule == null)
                {
                    throw new Exception($"Load rule by [{ruleId}] is empty.");
                }

                foreach (var sourceGetRuleMethod in SourceGetRuleMethods)
                {
                    result.Add(sourceGetRuleMethod.Key, sourceGetRuleMethod.Value(sourceGetRuleMethod.Key, rule));
                }

                return result;
            }
        }
        public ManualApprovalSettingModel GetReportRelateSettingInfo(ManualExportReportInfo manualApprovalReportInfo)
        {

            var localSetting = Settings.FirstOrDefault(item =>
            manualApprovalReportInfo.Path.StartsWith(item.FullPath.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase));
            if (localSetting == null)
            {
                var connectionInfo = Connections.FirstOrDefault(item =>
                manualApprovalReportInfo.Path.StartsWith(item.UNCPath.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase));

                localSetting = Settings.FirstOrDefault(item => item.ScopeId == connectionInfo.Id);

                localSetting ??= Settings.FirstOrDefault(item => item.ScopeId == connectionInfo.GroupId);
            }

            if (localSetting == null)
            {
                return new ManualApprovalSettingModel();
            }

            var settingInfo = new ManualApprovalSettingModel
            {
                SettingId = localSetting.Id,
                ManualApprovalType = localSetting.ApprovalType,
                IsSendEmialToOwner = localSetting.EMailToRecordOwner
            };

            if (localSetting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
            {
                settingInfo.WorkflowId = localSetting.WorkflowReferenceId;
            }
            else if (localSetting.ApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
            {
                settingInfo.Owners = FileSytemSettingDao.GetReocrdOwnersBySettingId(localSetting.Id);
            }

            return settingInfo;
        }
        public string RealRunCollectionJob(JobRunBy jobRunBy, JobType jobType)
        {
            string jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningJobs(jobType);

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartCollectionJob(jobType, jobId, jobRunBy);
                logger.Info("run enforce retention job success, JobId:{0}", jobId);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                logger.Info("collection data job has job running,so shedule job is skip");
            }
            return jobId;
        }

        private void StartCollectionJob(JobType jobType, string jobId, JobRunBy runBy)
        {
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = jobType,
                RunBy = runBy,
                CommandLine = string.Format("{0} {1}", jobType, jobId),
            });
        }



        //public void StartICJob()
        //{ //RMSessionStore.GetLogonUserInfo()
        //    string jobId = RMJobService.CreateJob(JobType.CollectionDataIncremental, LoginService.GetCurrentUserInfo().DisplayName);
        //    RealStartCollectionJob(jobId, JobType.CollectionDataIncremental);

        //}

        private void RealStartCollectionJob(string jobId, JobType type)
        {
            throw new NotImplementedException();
            //string installPath = WebUtil.GetInstallPath();
            //installPath = installPath.Substring(0, installPath.LastIndexOf('\\'));
            ////由于安装包路径和开发包路径不一样，所以这里判断一下，根据环境不同取不同的路径
            //string filePath = installPath + string.Format("\\{0}\\bin\\RecordsScheduleJob.exe", WebUtil.GetJobFolder(installPath));
            //FileInfo thisfile = new FileInfo(filePath);
            //if (thisfile.Exists)
            //{
            //    var startInfo = new ProcessStartInfo(
            //    installPath + string.Format("\\{0}\\bin\\RecordsScheduleJob.exe", WebUtil.GetJobFolder(installPath)),
            //    string.Format("{0} {1}", type, jobId));//, "\"" + jobInfo + "\""));
            //    startInfo.CreateNoWindow = true;
            //    var process = Process.Start(startInfo);
            //}
            //else
            //{
            //    var startInfo = new ProcessStartInfo(
            //    installPath + "\\RAScheduleJob\\bin\\RecordsScheduleJob.exe",
            //    string.Format("{0} {1}", type, jobId));//, "\"" + jobInfo + "\""));
            //    startInfo.CreateNoWindow = true;
            //    var process = Process.Start(startInfo);
            //}
        }


        //public Dictionary<Guid, int> TestUnion()
        //{
        //    return ExplorerDao.TestUnion();
        //}

        /// <summary>
        /// TEST METHOD!!! ????//Used by related records .need fix the performance
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ExplorerResultInfo> GetRelatedRecoredsInfoAsync(Guid id)
        {
            var rst = new ExplorerResultInfo
            {
                PagingInfo = new ExplorerPagingInfo()
            };
            var currRecord = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
            //var currRecord = CollectionDataDao.GetRecordByIds(new List<int> { id }).FirstOrDefault();
            if (string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                return rst;
            }
            List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);

            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            rst.Datas = new List<BaseRecordDto>();
            //TO DO Query All records in One query with security trimming? ylgu
            List<int> PermissionIds = new List<int>();
            bool isEnduser = await IsPhysicalEndUserAsync();
            if (isEnduser)
            {
                PermissionIds = await GetPermissionConditionAsync();
            }
            foreach (var info in infos)
            {
                var record = ExplorerDao.QueryAll(r => r.NodeId == info.id).FirstOrDefault();
                //var record = CollectionDataDao.GetRecordByItemId(info.SiteId, info.id);
                if (record != null)
                {
                    try
                    {
                        if (isEnduser)
                        {
                            if (record.ScopePermissionId != 0 && !PermissionIds.Contains(record.ScopePermissionId))
                            {
                                continue;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info($"check permission {e.ToString()}");
                    }
                    var recordDto = ConvertUtil.ConvertToBaseRecordDto(record, accountMap);
                    if (record.SourceFlag == (int)SourceFlag.Physical)
                    {
                        if(record.RecordStatus == 3)
                        {
                            logger.Info($"The physical record {record.Id} is deleted,so skip it");
                            continue;
                        }
                        var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(record);
                        SetPhysicalObjectHoldStatus(recordDto, physicalObjectDto);
                        SetPhysicalRcordFile(null, recordDto, physicalObjectDto);
                    }

                    SetRuleInfos(recordDto);
                    SetObjectType(recordDto);

                    rst.Datas.Add(recordDto);
                }
            }
            rst.PagingInfo.Total = rst.Datas.Count;
            return rst;
        }

        public List<RMRelatedItemInfo> GetRelatedRecoredsBaseInfoForStandardUser(Guid id, List<int> ScopePermissions)
        {
            List<RMRelatedItemInfo> relatedItemInfo = new List<RMRelatedItemInfo>();
            var currRecord = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
            if (string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                return relatedItemInfo;
            }
            relatedItemInfo = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);
            var ids = relatedItemInfo.Select(r => r.id).ToList();

            var records = ExplorerDao.GetRecordsByIdPermssions(ScopePermissions, ids);
            relatedItemInfo = records.Select(s => new RMRelatedItemInfo()
            {
                NodeType = s.NodeType,
                name = s.LeafName,
                id = s.Id,
                SourceFlag = s.SourceFlag,
                SiteId = s.ScopeId
            }).ToList();
            //兼容旧数据,SP的Flag为0
            relatedItemInfo.ForEach(r => r.SourceFlag = r.SourceFlag == 0 ? (int)SourceFlag.SharePoint : r.SourceFlag);
            return relatedItemInfo;
        }

        public List<RMRelatedItemInfo> GetRelatedRecoredsBaseInfo(Guid id)
        {
            //List<BaseRecordDto> result = new List<BaseRecordDto>();
            List<RMRelatedItemInfo> relatedItemInfo = new List<RMRelatedItemInfo>();
            var currRecord = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
            if (string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                return relatedItemInfo;
            }
            relatedItemInfo = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);
            var ids = relatedItemInfo.Select(r => r.id).ToList();
            relatedItemInfo = ExplorerDao.QueryAll(r => ids.Contains(r.NodeId))
                .Select(s => new RMRelatedItemInfo()
                {
                    NodeType = s.NodeType,
                    name = s.LeafName,
                    id = s.Id,
                    SourceFlag = s.SourceFlag,
                    SiteId = s.ScopeId
                }).ToList();
            //兼容旧数据,SP的Flag为0
            relatedItemInfo.ForEach(r => r.SourceFlag = r.SourceFlag == 0 ? (int)SourceFlag.SharePoint : r.SourceFlag);
            return relatedItemInfo;
        }

        public async Task<ExplorerResultInfo> SearchRecordsAsync(string pageIndex, int pageSize, string value, Guid currentId, List<Guid> relatedsCache)
        {
            var rst = new ExplorerResultInfo
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };
            bool hasNext = false;
            var exceptIds = new List<Guid>() { currentId };
            if (relatedsCache != null && relatedsCache.Count > 0)
            {
                exceptIds.AddRange(relatedsCache);
            }
            var currRecord = ExplorerDao.QueryAll(r => r.Id == currentId).First();
            //var currRecord = CollectionDataDao.GetRecordByIds(new List<int> { currentId }).FirstOrDefault();
            if (!string.IsNullOrEmpty(currRecord.RelatedRecords))
            {
                List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);
                foreach (var info in infos)
                {
                    var record = ExplorerDao.QueryAll(r => r.ScopeId == info.SiteId && r.NodeId == info.id).FirstOrDefault();
                    if (record != null)
                    {
                        exceptIds.Add(record.Id);
                    }
                }
            }

            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            var sourceFlag = currRecord.SourceFlag == (int)SourceFlag.FileSystem ? SourceFlag.FileSystem : SourceFlag.All;
            var recT = await SearchRecordsForRelatedAsync(sourceFlag, value, exceptIds, pageIndex, pageSize, (SourceFlag)currRecord.SourceFlag);
            var list = recT.Item1.ToList();
            var datas = list.ConvertAll(e =>
            {
                var recordDto = ConvertUtil.ConvertToBaseRecordDto(e, accountMap);
                if (e.SourceFlag == (int)SourceFlag.Physical)
                {
                    var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(e);
                    SetPhysicalObjectHoldStatus(recordDto, physicalObjectDto);
                    SetPhysicalRcordFile(null, recordDto, physicalObjectDto);
                }
                SetRuleInfos(recordDto);
                SetObjectType(recordDto);
                return recordDto;
            });
            rst.Datas = datas;
            rst.PagingInfo.HasNextPage = !string.IsNullOrEmpty(recT.Item2);
            rst.PagingInfo.PageIndex = recT.Item2;
            return rst;
        }

        private async Task<Tuple<IEnumerable<Record>, string>> SearchRecordsForRelatedAsync(SourceFlag sourceFlag, string searchKey, List<Guid> exceptIds, string pageIndex, int pageSize, SourceFlag currentSourceFlag)
        {
            ExplorerQueryV2Dto queryDto = new ExplorerQueryV2Dto()
            {
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = AssembleFilterOptionForRelated(sourceFlag, exceptIds, currentSourceFlag),
                    SearchOption = string.IsNullOrEmpty(searchKey) ? null : new ExplorerSearchOptionV2()
                    {
                        Key = searchKey,
                        Columns = new List<ExplorerQueryColumn>
                        {
                            new ExplorerQueryColumn {  Id = DefaultColumnIDs.UniqueId },
                            new ExplorerQueryColumn { Id = DefaultColumnIDs.NameOrTitle },
                        }
                    }
                },
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };
            Tuple<IEnumerable<Record>, string> result = null;
            try
            {
                await ExplorerQueryParamProcesser.ProcessAsync(queryDto.QueryOption);
                //remove term permission filter
                //ExplorerQueryService.ProcessWithoutNodeTypeParam(queryDto.QueryOption.FilterOption);
                result = ExplorerDao.SearchRecordsV2(queryDto);
                foreach (Record rec in result.Item1)
                {
                    rec.AppendMetaInfoForOldLogic();
                }
            }
            catch (ExplorerQueryNoPermissionException e)
            {
                logger.Warn("No permission to access data in search data for related. ERROR:{0}", e.ToString());
                result = new Tuple<IEnumerable<Record>, string>(new List<Record>(), string.Empty);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while query data for related, ERROR:{0}", ex.ToString());
                result = new Tuple<IEnumerable<Record>, string>(new List<Record>(), string.Empty);
            }
            return result;
        }

        private ExplorerFilterOptionV2 AssembleFilterOptionForRelated(SourceFlag sourceFlag, List<Guid> exceptIds, SourceFlag currentSourceFlag)
        {
            // 1. SourceFlags
            List<SourceFlag> sourceFlags;
            if (sourceFlag == SourceFlag.FileSystem)
            {
                sourceFlags = new List<SourceFlag> { SourceFlag.FileSystem };
            }
            else if (currentSourceFlag == SourceFlag.SharePointOnPrem)
            {
                sourceFlags = new List<SourceFlag> { SourceFlag.SharePointOnPrem, SourceFlag.Physical };
            }
            else
            {
                sourceFlags = new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.Physical, SourceFlag.Teams };
                if (currentSourceFlag == SourceFlag.Physical && !sourceFlags.Contains(SourceFlag.SharePointOnPrem))
                {
                    sourceFlags.Add(SourceFlag.SharePointOnPrem);
                }
            }

            // 2. NodeTypes
            List<RMNodeLevel> nodeTypes = sourceFlag == SourceFlag.FileSystem
                ? new List<RMNodeLevel> { RMNodeLevel.FSFile }
                : new List<RMNodeLevel> { RMNodeLevel.Item, RMNodeLevel.PhysicalFile, RMNodeLevel.PhysicalRecord };

            // 3. Status
            List<RMRecordStatus> rmRecordStatus = sourceFlag == SourceFlag.FileSystem
                ? new List<RMRecordStatus> { RMRecordStatus.Active }
                : new List<RMRecordStatus> { RMRecordStatus.Active, RMRecordStatus.Closed };

            // 4. Build filter option
            return new ExplorerFilterOptionV2
            {
                SourceFlags = sourceFlags,
                NodeTypes = nodeTypes,
                Status = rmRecordStatus,
                DeclaredRecord = false,
                ExceptIds = exceptIds,
                //TermIds = termIds
            };
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ManageRelatedRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage UpdateRelatedRecords(Guid id, List<Guid> relatedIds, List<Guid> removeRelatedIds, Dictionary<Guid, string> idNameDict, out List<Guid> addrelatedIdsForHistory)
        {
            RAReturnMessage rstMsg = new RAReturnMessage();
            var commonErrorMsg = I18NEntity.GetString("RM_JS_BCM_Explorer_ManageRelatedRecordsApplyError");
            rstMsg.MessageType = RAMessageType.Successful;
            addrelatedIdsForHistory = new List<Guid>();
            //relatedIds是所有关联的item，包括新加的和已经存在的，
            //addRelatedIds 需要找出本次操作需要添加的，已经存在的，不需要处理。
            List<Guid> addRelatedIds = new List<Guid>();
            Record currRecord = null;
            var allRelatedItemDBInfo = new List<Record>();
            var allRelatedIds = new List<Guid>();
            allRelatedIds.Add(id);
            if (relatedIds != null)
            {
                allRelatedIds.AddRange(relatedIds);
            }
            if (removeRelatedIds != null)
            {
                allRelatedIds.AddRange(removeRelatedIds);
            }
            allRelatedItemDBInfo = ExplorerDao.GetRecordByIds(allRelatedIds);

            // Some callers send NodeId (SP UniqueId) instead of Record.Id.
            // Add a fallback lookup so related update can resolve the target records.
            var missingIds = allRelatedIds.Except(allRelatedItemDBInfo.Select(r => r.Id)).ToList();
            if (missingIds.Count > 0)
            {
                var nodeIdMatchedRecords = ExplorerDao.QueryAll(r => missingIds.Contains(r.NodeId)).ToList();
                foreach (var record in nodeIdMatchedRecords)
                {
                    if (!allRelatedItemDBInfo.Any(r => r.Id == record.Id))
                    {
                        allRelatedItemDBInfo.Add(record);
                    }
                }
            }

            relatedIds = NormalizeRecordIdsByIdOrNodeId(relatedIds, allRelatedItemDBInfo);
            removeRelatedIds = NormalizeRecordIdsByIdOrNodeId(removeRelatedIds, allRelatedItemDBInfo);

            // fill id->name for audit handlers (Before/After)
            if (idNameDict != null)
            {
                foreach (var item in allRelatedItemDBInfo)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    if (!idNameDict.ContainsKey(item.Id))
                    {
                        if (string.IsNullOrWhiteSpace(item.LeafName))
                        {
                            logger.Warn($"The record: [{item.Id}] has empty leaf name.");
                        }
                        idNameDict[item.Id] = item.LeafName ?? item.Id.ToString();
                    }
                }
            }

            foreach (var item in allRelatedItemDBInfo)
            {
                if (item.SourceFlag == (int)SourceFlag.Teams) {
                    item.SourceFlag = (int)SourceFlag.SharePoint;
                }
            }
            #region 此处逻辑out 出去的value 目前并没有人用，保留逻辑
            try
            {
                currRecord = allRelatedItemDBInfo.Find(a => a.Id == id);
                rstMsg.Extension = currRecord.SourceFlag.ToString();
                addrelatedIdsForHistory = this.GetNewAddedRelatedIds(currRecord, relatedIds, allRelatedItemDBInfo);
            }
            catch (Exception e)
            {
                logger.Error("get current item from sp error:{0}", e.ToString());

                rstMsg.MessageType = RAMessageType.Exception;
                rstMsg.ErrorMessage = commonErrorMsg;
                return rstMsg;
            }
            #endregion

            RelatedRecordsUtility utility = null;
            try
            {
                utility = new RelatedRecordsUtility(currRecord);
            }
            catch (Exception e)
            {
                logger.Error("ctor RelRecUtility error,{0}", e.ToString());
                rstMsg.MessageType = RAMessageType.Exception;
                rstMsg.ErrorMessage = commonErrorMsg;
                return rstMsg;
            }
            if (currRecord.SourceFlag == (int)SourceFlag.SharePoint && !utility.CheckCurrentListEnableApp())
            {
                logger.Error("current item not contains related column.");
                rstMsg.MessageType = RAMessageType.Exception;
                rstMsg.ErrorMessage = I18NEntity.GetString("RM_Explorer_Related_CurrentdRecordsDisableApp");
                return rstMsg;
            }

            #region 更新对应Source 中的related 信息（如果需要的话，目前只有SP 需要），并且返回需要更新到DB 的信息：relatedvalue ， related count
            try
            {
                var allrelatedInfo = new List<RMRelatedItemInfo>();
                var scopeIds = allRelatedItemDBInfo.Select(s => s.ScopeId).Distinct().ToList();
                var dicMap = RMScopeDao.GetScopeInfoByIds(scopeIds);
                if (removeRelatedIds != null && removeRelatedIds.Count > 0)
                {
                    var removeRelatedRecords = allRelatedItemDBInfo.FindAll(r => removeRelatedIds.Contains(r.Id));
                    var removeInfos = removeRelatedRecords.Select(r => utility.GenerateRMRelatedItemInfo(r)).ToList();
                    removeInfos.ForEach(r => r.NeedDelete = true);
                    //var removeInfos = this.GetRelatedItemInfo(removeRelatedRecords, dicMap, true);
                    allrelatedInfo.AddRange(removeInfos);
                }

                if (relatedIds != null && relatedIds.Count > 0)
                {
                    var addRelatedRecords = allRelatedItemDBInfo.FindAll(r => relatedIds.Contains(r.Id));
                    var addInfos = addRelatedRecords.Select(r => utility.GenerateRMRelatedItemInfo(r));
                    //var addInfos = this.GetRelatedItemInfo(addRelatedRecords, dicMap, false);
                    allrelatedInfo.AddRange(addInfos);
                }
                utility.UpdateRelatedPropertiesForExplorer(currRecord, allrelatedInfo, allRelatedItemDBInfo);
            }
            catch (RelatedRecordsAppDisableExcetion re)
            {
                logger.Error("remove realted records for sp error:{0}", re.ToString());
                rstMsg.MessageType = RAMessageType.Exception;
                rstMsg.ErrorMessage = re.Message;
            }
            catch (Exception e)
            {
                logger.Error("remove realted records for sp error:{0}", e.ToString());
                rstMsg.MessageType = RAMessageType.Exception;
                rstMsg.ErrorMessage = commonErrorMsg;
                //return rstMsg;//需要走更新DB逻辑
            }
            finally
            {
                if (utility != null)
                {
                    utility.Dispose();
                }
            }
            #endregion

            if (rstMsg.MessageType == RAMessageType.Successful && currRecord.SourceFlag == (int)SourceFlag.Physical)
            {
                var addRecordNames = allRelatedItemDBInfo.Where(record => record.NodeId != currRecord.NodeId && !removeRelatedIds.Contains(record.Id)).Select(s => s.LeafName).ToList();
                RecordsHistoryService.AddPhysicalRelatedActionAudit(currRecord.Id, currRecord.RelatedRecords, addRecordNames);
            }

            return rstMsg;
        }

        private static Guid ResolveRecordIdByIdOrNodeId(Guid inputId, List<Record> records)
        {
            var matchedById = records.FirstOrDefault(r => r.Id == inputId);
            if (matchedById != null)
            {
                return matchedById.Id;
            }

            var matchedByNodeId = records.FirstOrDefault(r => r.NodeId == inputId);
            return matchedByNodeId?.Id ?? inputId;
        }

        private static List<Guid> NormalizeRecordIdsByIdOrNodeId(List<Guid> inputIds, List<Record> records)
        {
            if (inputIds == null || inputIds.Count == 0)
            {
                return inputIds;
            }

            var normalizedIds = new List<Guid>();
            foreach (var inputId in inputIds)
            {
                var resolvedId = ResolveRecordIdByIdOrNodeId(inputId, records);
                if (resolvedId != Guid.Empty && !normalizedIds.Contains(resolvedId))
                {
                    normalizedIds.Add(resolvedId);
                }
            }

            return normalizedIds;
        }


        private List<Guid> GetNewAddedRelatedIds(Record record, List<Guid> relatedIds, List<Record> allRelatedItemInfo)
        {
            var addRelatedIds = new List<Guid>();
            //如果DB 中没有related 信息，那么GUI 上传过来的，全部都是新添加的
            if (string.IsNullOrEmpty(record.RelatedRecords))
            {
                addRelatedIds = relatedIds;
            }
            else
            {
                try
                {
                    List<RMRelatedItemInfo> infos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(record.RelatedRecords);
                    var originalRelatedIds = new List<Guid>();
                    foreach (var info in infos)
                    {
                        var relatedRecord = allRelatedItemInfo.Find(r => r.ScopeId == info.SiteId && r.NodeId == info.id);
                        //var record = CollectionDataDao.GetRecordByItemId(info.SiteId, info.id);
                        if (relatedRecord != null)
                        {
                            originalRelatedIds.Add(relatedRecord.Id);
                        }
                    }

                    foreach (var relatedId in relatedIds)
                    {
                        if (!originalRelatedIds.Contains(relatedId))
                        {
                            addRelatedIds.Add(relatedId);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("find add action items error ,we will add all items (add and exist), current id:{0}, error: {1}", record.Id, e.ToString());
                }
            }
            return addRelatedIds;
        }
        #endregion

        #region Private Function

        #region Get Details

        private async Task<BaseRecordDto> GetBaseRecordDtoAsync(Record baseRecord)
        {
            logger.Debug("Begin Convert record dto");
            BaseRecordDto record = new BaseRecordDto();
            record = ConvertUtil.ConvertToBaseRecordDto(baseRecord);
            if (record != null && !string.IsNullOrEmpty(record.DisposalDueDate))
            {
                long tempTicks;
                if (long.TryParse(record.DisposalDueDate, out tempTicks))
                {
                    var minDate = DateTime.MinValue;
                    if (tempTicks > minDate.Ticks)
                    {
                        //if (tempTicks > DateTime.UtcNow.Ticks)
                        //{
                        //    record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(tempTicks, true).SimplifyFormatTime;
                        //}
                        //else
                        //{
                        //    record.DisposalDueDate = I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
                        //}
                        record.DisposalDueDate = (await GeneralSettingService.ConvertTiksToDateTimeAsync(tempTicks, true)).SimplifyFormatTime;
                    }
                }
                else
                {
                    record.DisposalDueDate = I18NEntity.GetString(record.DisposalDueDate); ;
                }
            }
            //REC - 3883
            ArgumentCheck.NotNull(record, nameof(record));
            if (record.HoldStatus)
            {
                record.DisposalDueDate = string.Empty;
                record.ReleaseTime = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(record.HoldReleaseTime, true)).SimplifyFormatTime;
            }
            else
            {
                record.ReleaseTime = string.Empty;
            }

            if (record.RuleId != Guid.Empty)
            {
                var rule = RMRuleDao.GetRuleById(record.RuleId);
                record.RuleName = rule?.RuleName;
                record.DisposalAction = rule == null ? (int)RMContentDisposalAction.None : rule.DisposalAction;
                record.ExchangeDisposalAction = rule == null ? (int)RMContentDisposalAction.None : rule.ExchangeDisposalAction;
            }
            else
            {
                record.DisposalAction = (int)RMContentDisposalAction.None;
                record.ExchangeDisposalAction = (int)RMContentDisposalAction.None;
            }

            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_FileNull")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_FileNull");
            }
            if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                record.ExtensionForFile = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPItem");
            }

            if (record.RecordStatus == (int)RMRecordStatus.Archived)
            {
                var contentDownloadInfo = DownloadDataInfoDao.GetDownloadDataInfos(new List<Guid>() { record.Id });
                if (contentDownloadInfo != null && contentDownloadInfo.Count > 0)
                {
                    //如果有下载成功的job，返回Finish(2)状态
                    if (contentDownloadInfo.Any(c => c.JobStatus == (int)DownloadContentJobStatus.Finished))
                    {
                        record.ContentDownloadStatus = (int)DownloadContentJobStatus.Finished;
                    }
                    else if (contentDownloadInfo.Any(c => c.JobStatus == (int)DownloadContentJobStatus.Wait || c.JobStatus == (int)DownloadContentJobStatus.InProgress))
                    {
                        //如果有正在下载尚未成功job，返回InProgress(1)状态
                        record.ContentDownloadStatus = (int)DownloadContentJobStatus.InProgress;
                    }
                    else
                    {
                        //其余情况当做Failed(3)
                        record.ContentDownloadStatus = (int)DownloadContentJobStatus.Failed;
                    }
                }
                else
                {
                    //没有记录为None(-1)
                    record.ContentDownloadStatus = (int)DownloadContentJobStatus.None;
                }
            }
            return record;
        }
        private async Task<RecordSummary> GetSummaryInfoAsync(Record info)
        {
            string dueDateStr = DueDateUtil.ConvertLongDueDate2String(info.DisposalDueDate);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var termNameFullPath = TaxonomyService.GetTermPathByTermId(info.TermId);
            if (info.SourceFlag == (int)SourceFlag.Google)
            {
                if (!string.IsNullOrEmpty(info.TermName))
                {
                    termNameFullPath = info.TermName;
                }
                else if (!string.IsNullOrEmpty(info.TermId.ToString()) || info.TermId != Guid.Empty)
                {
                    var labelReclassify = (await LabelDao.GetLabelByUniqueIdAsync(info.TermId.ToString()));
                    if(labelReclassify != null)
                    {
                        termNameFullPath = labelReclassify.Name;
                    }
                }             
            }

            RecordSummary sum = new RecordSummary
            {
                SourceFlag = (SourceFlag)info.SourceFlag,
                LeafName = info.LeafName,
                FullPath = info.SourceFlag == 1 || info.SourceFlag == 5 || info.SourceFlag == 6 || info.SourceFlag == 11 ? info.FullPath : string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, info.EmailAddress, info.DirPath, new DateTime(info.TimeCreated).ToString("R")),
                RecordId = info.RecordsId,
                Term = termNameFullPath
                
            };
            if (info.SourceFlag == 2 || info.SourceFlag == (int)SourceFlag.AzureFileShare || info.SourceFlag == (int)SourceFlag.Box || info.SourceFlag == (int)SourceFlag.Google)
            {
                sum.FullPath = info.DirPath;
            }
            
            sum.TermSettings = GetRetentionSettingInfo(info);

            if (info.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
            {
                sum.FullPath = WebUtil.GetListItemRealPath(info.FullPath);
            }

            long now = DateTime.UtcNow.Ticks;
            //RECO-3246, Disposal Due Date目前为真实值， 不需要通过逻辑控制隐藏;
            if (info != null && !string.IsNullOrEmpty(dueDateStr))
            {
                long tempTicks;

                if (long.TryParse(dueDateStr, out tempTicks))
                {
                    var minDate = DateTime.MinValue;
                    if (tempTicks > minDate.Ticks)
                    {
                        //if (tempTicks > now)
                        //{
                        //    sum.DisposalDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                        //}
                        //else
                        //{
                        //    sum.DisposalDate = I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
                        //}
                        sum.DisposalDate = GeneralSettingService.ConvertTiksToDateTime(gls, tempTicks, true).SimplifyFormatTime;
                    }
                }
                else
                {
                    sum.DisposalDate = I18NEntity.GetString(dueDateStr);
                }
            }
            sum.DeclareAsRecord = info.DeclareAsRecord;
            sum.DeclaredBy = info.DeclaredBy;
            sum.LockByRecordLabel = info.LockedByRecordLabel;
            sum.ApplyRecordLabelBy = info.ApplyRecordLabelBy;
            sum.HoldStatus = info.HoldStatus;
            //sum.HoldId = info.HoldSetting;
            if (info.HoldStatus)
            {
                var recordAllExistHoldIds = GetAllExistHoldIds(info);
                var holds = HoldDao.GetHoldByIds(recordAllExistHoldIds).OrderBy(h => h.Id, new HoldSpecialComparer(recordAllExistHoldIds)).ToList();
                List<HoldUser> holdByUsers = string.IsNullOrEmpty(info.HoldByUsers) ? new List<HoldUser>() : JsonConvert.DeserializeObject<List<HoldUser>>(info.HoldByUsers);
                var distinctHoldByUsers = holdByUsers.Select(h => h.HoldBy).Distinct();
                sum.HoldSetting = new HoldSetting()
                {
                    Name = string.Join(", ", holds.Select(h => h.Name)),
                    Description = string.Join("; ", holds.Select(h => string.IsNullOrEmpty(h.Description) ? I18NEntity.GetString("RM_JS_Common_Pending") : h.Description))
                };
                if (holdByUsers.Count > 0)
                {
                    var userEmails = holdByUsers.Select(u => u.HoldBy).ToList();
                    var accountMap = await AccountDao.FindListAsync(a => userEmails.Contains(a.UserPrincipalName));
                    foreach (var holdByUser in holdByUsers)
                    {
                        holdByUser.HoldBy = AssembleAccountDisplayName(holdByUser.HoldBy, accountMap);
                    }
                    if (distinctHoldByUsers.Count() == 1)
                    {
                        sum.HoldBy = distinctHoldByUsers.FirstOrDefault();
                    }
                    else
                    {
                        sum.HoldBy = string.Join("; ", holdByUsers.OrderBy(h => h.HoldId, new HoldSpecialComparer(recordAllExistHoldIds)).Select(h => h.HoldBy).Distinct());
                    }
                }
                else
                {
                    sum.HoldBy = info.HoldBy;
                }
                sum.HoldReleaseTime = GeneralSettingService.ConvertTiksToDateTime(gls, info.HoldReleaseTime, true).SimplifyFormatTime;
            }

            try
            {
                sum.RuleId = Guid.Empty;
                sum.RuleName = string.Empty;
                sum.DisposalAction = I18NEntity.GetString("RM_JS_RDM_CreateRule_Options_None");

                if (info.RuleId != Guid.Empty)
                {
                    var tempRule = RMRuleDao.GetRuleById(info.RuleId);
                    if (tempRule != null)
                    {
                        sum.RuleId = tempRule.RuleId;
                        sum.RuleName = tempRule.RuleName;
                        sum.DisposalAction = string.Empty;
                        if (info.SourceFlag == (int)SourceFlag.SharePoint || info.SourceFlag == (int)SourceFlag.Teams)
                        {
                            sum.DisposalAction = tempRule.DisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.Exchange)
                        {
                            sum.DisposalAction = tempRule.ExchangeDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.FileSystem)
                        {
                            sum.DisposalAction = tempRule.FSDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                        {
                            sum.DisposalAction = tempRule.SPLocalDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.OneDrive)
                        {
                            sum.DisposalAction = tempRule.OneDriveDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.AzureFileShare)
                        {
                            sum.DisposalAction = tempRule.AzureFileDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.Box)
                        {
                            sum.DisposalAction = tempRule.BoxDisposalAction.ToString();
                        }
                        if (info.SourceFlag == (int)SourceFlag.Google)
                        {
                            sum.DisposalAction = tempRule.GoogleDriveDisposalAction.ToString();
                        }
                        if (info.SourceFlag > (int)SourceFlag.Connector)
                        {
                            sum.DisposalAction = tempRule.ConnectorDisposalAction.ToString();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Occur an error load rule in Get Summary Info,message:{0} ", e.Message);
            }
            //Hold to do
            return sum;
        }
        private string GetRetentionSettingInfo(Record info)
        {
            var retentionSettingsInfo = string.Empty;
            var termInfo = TermDao.GetParentInhertSetting(info.TermId);
            bool isM365 = (info.SourceFlag == (int)SourceFlag.SharePoint
                        || info.SourceFlag == (int)SourceFlag.Exchange
                        || info.SourceFlag == (int)SourceFlag.OneDrive
                        || info.SourceFlag == (int)SourceFlag.Teams);
            if (isM365)
            {
                retentionSettingsInfo = I18NEntity.GetString("RM_JS_BCM_Explorer_Details_DisabeRetentionStatus");

                if (termInfo != null)
                {
                    if (info.SourceFlag == (int)SourceFlag.SharePoint)
                    {
                        if (CheckTermEnforceRetention(termInfo.EnforceRetention, EnforceRetentionType.SharePoint))
                        {
                        retentionSettingsInfo = $"{I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionStatus")}, {I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionLabel")}: {termInfo.SPRetentionLabel}";
                        }
                    }
                    else if (info.SourceFlag == (int)SourceFlag.Exchange)
                    {
                        if (CheckTermEnforceRetention(termInfo.EnforceRetention, EnforceRetentionType.Exchange))
                        {
                        retentionSettingsInfo = $"{I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionStatus")}, {I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionLabel")}: {termInfo.EXORetentionLabel}";
                        }
                    }
                    else if (info.SourceFlag == (int)SourceFlag.OneDrive)
                    {
                        if (CheckTermEnforceRetention(termInfo.EnforceRetention, EnforceRetentionType.OneDrive))
                        {
                        retentionSettingsInfo = $"{I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionStatus")}, {I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionLabel")}: {termInfo.OneDriveRetentionLabel}";
                        }
                    }
                    else if (info.SourceFlag == (int)SourceFlag.Teams)
                    {
                        if (CheckTermEnforceRetention(termInfo.EnforceRetention, EnforceRetentionType.Teams))
                        {
                            retentionSettingsInfo = $"{I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionStatus")}, {I18NEntity.GetString("RM_JS_BCM_Explorer_Details_RetentionLabel")}: {termInfo.TeamsRetentionLabel}";
                        }
                    }
                }
                else
                {
                    retentionSettingsInfo = string.Empty;
                }
            }

            return retentionSettingsInfo;
        }


        private bool CheckTermEnforceRetention(int termRetention, EnforceRetentionType needCheckRetentionType)
        {
            return (termRetention & (int)needCheckRetentionType) == (int)needCheckRetentionType;
        }
        //protected int GetOperationType(RMRuleInfos rule)
        //{
        //    if (rule == null)
        //    {
        //        return (int)RMContentDisposalAction.None;
        //    }
        //    int keepDataOption = rule.RuleKeepDataOption;
        //    if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
        //    {
        //        return (int)RMContentDisposalAction.ArchiveLeaveStub;
        //    }
        //    else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
        //    {
        //        return (int)RMContentDisposalAction.ArchiveAndKeepData;
        //    }
        //    else if (keepDataOption == (int)KeepDataStatus.Delete && rule.MoveToRecordCenterSettings != null && rule.MoveToRecordCenterSettings.DestinationLocation != null)
        //    {
        //        return (int)RMContentDisposalAction.Move;
        //    }
        //    else
        //    {
        //        return (int)RMContentDisposalAction.ArchiveAndRemove;
        //    }
        //}

        private async Task<GeneralProperty> GetGeneralPropertyAsync(Record info, bool isControlPlus = false)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (isControlPlus) gls.TimeZoneId = TenantLocalValue.TimezoneId;

            GeneralProperty ppty = new GeneralProperty
            {
                DateType = GetRecordDataType(info),
                TimeCreated = GeneralSettingService.ConvertTiksToDateTime(gls, info.TimeCreated, true).SimplifyFormatTime,
                CreatedBy = info.CreatedBy,
                TimeModified = info.TimeModified == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, info.TimeModified, true).SimplifyFormatTime,
                ModifiedBy = info.ModifiedBy,
                SendTime = GeneralSettingService.ConvertTiksToDateTime(gls, info.TimeCreated, true).SimplifyFormatTime,
                Sender = info.CreatedBy,
                Recipient = info.SendTo,
            };
            if (info.MetaInfo != null)
            {
                RecordMetaInfo attachment = JsonConvert.DeserializeObject<RecordMetaInfo>(info.MetaInfo);
                if (attachment.AttachmentNames != null && attachment.AttachmentNames.Count() > 0)
                {
                    foreach (var attachmentNames in attachment.AttachmentNames)
                    {
                        ppty.Attachment += attachmentNames + "; ";
                    }
                    ppty.Attachment = ppty.Attachment.TrimEnd(' ');
                    ppty.Attachment = ppty.Attachment.TrimEnd(';');
                }
            }

            if (info.SourceFlag == (int)SourceFlag.SharePoint || info.SourceFlag == (int)SourceFlag.SharePointOnPrem || info.SourceFlag == (int)SourceFlag.OneDrive || info.SourceFlag == (int)SourceFlag.Google || info.SourceFlag == (int)SourceFlag.Teams)
            {
                if (info.SourceFlag == (int)SourceFlag.Google)
                {
                    info.FullPath = info.DirPath;
                }
                int folderLen = info.FullPath.LastIndexOf("/") == -1 ? info.FullPath.LastIndexOf("\\") : info.FullPath.LastIndexOf("/");
                if (folderLen > 0)
                {
                    ppty.FolderPath = info.FullPath.Substring(0, folderLen);
                }
            }
            else
            {
                ppty.FolderPath = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, info.EmailAddress, info.DirPath, new DateTime(info.TimeCreated).ToString("R"));
                if (info.SourceFlag == (int)SourceFlag.FileSystem || info.SourceFlag == (int)SourceFlag.AzureFileShare)
                {
                    ppty.FolderPath = info.DirPath;
                }
                
                if(info.SourceFlag == (int)SourceFlag.Box)
                {
                    var targetIndex = info.DirPath.LastIndexOf($"\\{info.LeafName}");
                    ppty.FolderPath = info.DirPath.Substring(0, targetIndex);
                }
            }
            ppty.CollectionTime = info.CollectTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, info.CollectTime, true).SimplifyFormatTime;
            if (info.NodeType != (int)RMNodeLevel.GoogleFolder && !string.IsNullOrEmpty(info.MetaInfo))
            {
                try
                {
                    RecordMetaInfo metaInfo = JsonUtil.JsonDeserialize<RecordMetaInfo>(info.MetaInfo);
                    //RecordMetaInfo metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(info.MetaInfo);
                    ppty.FileSize = ConvertToFormatSize(metaInfo.FileSize);                   
                }
                catch (Exception ex)
                {
                    logger.Error("error occurred while JsonDeserialize MetaInfo, ERROR:{0}", ex.ToString());
                }
            }
            return ppty;
        }

        private string GetRecordDataType(Record info)
        {
            var result = info.NodeType switch
            {
                (int)NodeLevel.FSFolder => I18NEntity.GetString("RM_JM_GlobalSearch_FSFolderType"),
                (int)NodeLevel.Folder => I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPFolder"),
                _ => JobReportUtility.GetColumnByI18N(info.ExtensionForFile),
            };
            return result;
        }
        private string ConvertToFormatSize(long size)
        {
            int _GB = 1024 * 1024 * 1024;
            int _MB = 1024 * 1024;
            int _KB = 1024;
            var result = string.Empty;
            var displayResult = @"{0} ({1} " + I18NEntity.GetString("RM_JS_BCM_Explorer_Details_Bytes") + @")";
            if (size / _GB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_GB, 2) + $" {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_GB")}", size.ToString("N0"));
            }
            else if (size / _MB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_MB, 2) + $" {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_MB")}", size.ToString("N0"));
            }
            else if (size / _KB >= 1)
            {
                result = string.Format(displayResult, Math.Round(size / (float)_KB, 2) + $" {I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_KB")}", size.ToString("N0"));
            }
            else
            {
                result = size + " bytes";
            }
            return result;
        }
        private string ConvertToFormatSizeWithoutBytes(long size)
        {
            int _GB = 1024 * 1024 * 1024;
            int _MB = 1024 * 1024;
            int _KB = 1024;
            var result = string.Empty;
            if (size / _GB >= 1)
            {
                result =  Math.Round(size / (float)_GB, 2) + " GB";
            }
            else if (size / _MB >= 1)
            {
                result = Math.Round(size / (float)_MB, 2) + " MB";
            }
            else if (size / _KB >= 1)
            {
                result = Math.Round(size / (float)_KB, 2) + " KB";
            }
            else
            {
                result = size + " bytes";
            }
            return result;
        }

        private async Task<ManualReviewInfo> GetManualReviewInfoAsync(Record info)
        {
            ManualReviewInfo mri = new ManualReviewInfo
            {
                ReviewAudits = new List<ReviewAudits>()
            };

            try
            {
                //switch (info.SourceFlag)
                //{
                //    case (int)SourceFlag.SharePoint:
                //    case (int)SourceFlag.SharePointOnPrem:
                //    case (int)SourceFlag.OneDrive:
                //        mri = RMManualApproveDao.GetAuditInfos(info.ScopeId, info.ItemId);
                //        break;
                //    case (int)SourceFlag.Exchange:
                //        mri = RMManualApproveDao.GetAuditInfos(info.ScopeId, info.ItemId);
                //        break;
                //    case (int)SourceFlag.FileSystem:
                //        //fs的scoped id对应approve表的PartKey  itemId对应nodeid  仅用nodeid判定即可
                //        mri = RMManualApproveDao.GetAuditInfos(info.ScopeId, info.ItemId, true);
                //        break;
                //    default:
                //        break;
                //}
                if (!string.IsNullOrEmpty(info.ManualAudits))
                {
                    mri.ReviewAudits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(info.ManualAudits);
                }

                var reviewerIntIds = info.ManualReviewer ?? new int[0];

                var accounts = await AccountDao.FindListAsync(item => Enumerable.Contains(reviewerIntIds, item.Id) && item.IsRemoved == 0);
                var displayNames = accounts.Select(item => item.DisplayName);
                mri.RecordOwner = string.Join(",", displayNames);

                if (mri.ReviewAudits != null && mri.ReviewAudits.Count > 0)
                {
                    mri.ReviewAudits = mri.ReviewAudits.OrderByDescending(a => Convert.ToInt64(a.ReviewTime)).ToList();
                    GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                    foreach (var item in mri.ReviewAudits)
                    {
                        if (!string.IsNullOrEmpty(item.ReviewTime))
                        {
                            item.ReviewTime = GeneralSettingService.ConvertTiksToDateTime(gls, Convert.ToInt64(item.ReviewTime), true).SimplifyFormatTime;
                        }
                        if(item.Action == "RM_MA_Escalate")
                        {
                            item.Action = "RM_JS_MA_ApproveStatus_Escalated";
                        }

                        if(item.Action == "RM_MA_Reassign")
                        {
                            item.Action = "RM_JS_MA_ApproveStatus_Reassigned";
                        }

                        item.Action = I18NEntity.GetString(item.Action);
                    }
                    string escalateTos = mri.RecordOwner;
                    if (!string.IsNullOrEmpty(escalateTos))
                    {
                        mri.RecordOwner = await ManualApprovalService.GesEscalateUsersAsync(escalateTos);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(info.RecordOwner))
                    {
                        mri.RecordOwner = await ManualApprovalService.GesEscalateUsersAsync(info.RecordOwner);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while GetManualReviewInfo, ERROR:{0}", ex.ToString());
            }
            return mri;
        }

        private async Task<RelatedRecordInfo> GetRelatedRecordInfoAsync(string relatedField)
        {
            RelatedRecordInfo rri = new RelatedRecordInfo();
            rri.Records = new List<BaseRecordDto>();
            List<RMRelatedItemInfo> releatedRecords = null;
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(Record), "d");
            try
            {
                if (!string.IsNullOrEmpty(relatedField) && relatedField.Length > 10)
                {
                    var now = DateTime.UtcNow.Ticks;
                    releatedRecords = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(relatedField);
                    foreach (var record in releatedRecords)
                    {
                        var itemUniqueID = record.id;
                        var siteId = record.SiteId;
                        Expression left = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeId", itemUniqueID);
                        Expression right = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", siteId);
                        var dd = Expression.AndAlso(left, right);
                        allExpressionList.Add(dd);

                    }
                    if (allExpressionList.Count > 0)
                    {
                        queryExpr = allExpressionList.Aggregate(Expression.OrElse);
                        var filter = Expression.Lambda<Func<Record, bool>>(queryExpr, param);
                        List<Record> records = ExplorerDao.QueryAll(filter).OrderByDescending(d => d.Id).ToList();
                        var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                        GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                        rri.Records = records.ConvertAll(e =>
                        {
                            BaseRecordDto record = ConvertUtil.ConvertToBaseRecordDto(e, accountMap);
                            SetSPObjectDisposalDueDate(now, gls, record);
                            //REC - 3883
                            SetSPObjectReleaseTime(e, gls, record);

                            SetRuleInfos(record);

                            SetObjectType(record);

                            if (record.SourceFlag == (int)SourceFlag.Physical)
                            {
                                if (e.NodeType == (int)RMNodeLevel.PhysicalFile)
                                {
                                    SetPhysicalFileClassification(e, record);
                                }

                                var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(e);

                                SetPhysicalObjectDisposalDueDateByCalculate(gls, record, physicalObjectDto);

                                SetPhysicalObjectHoldStatus(record, physicalObjectDto);

                                SetPhysicalRcordFile(gls, record, physicalObjectDto);
                            }
                            return record;
                        });
                        rri.RelateRecordCount = records.Count;
                    }
                }
                else
                {
                    var itemUniqueID1 = new Guid("F04E69B2-DAB6-4513-9449-DE39CEBAF765");
                    var siteId = new Guid("846D6CCA-60C2-4D44-A73A-7EABE64E7C00");
                    Expression left1 = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ItemId", itemUniqueID1);
                    Expression right1 = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", siteId);
                    var dd1 = Expression.AndAlso(left1, right1);
                    allExpressionList.Add(dd1);

                    var itemUniqueID2 = new Guid("05980056-5E73-49E0-B9AC-043BBC91E2DE");
                    var siteId2 = new Guid("846D6CCA-60C2-4D44-A73A-7EABE64E7C01");
                    Expression left2 = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ItemId", itemUniqueID2);
                    Expression right2 = Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", siteId2);
                    var dd2 = Expression.AndAlso(left2, right2);
                    allExpressionList.Add(dd2);

                    if (allExpressionList.Count > 0)
                    {
                        queryExpr = allExpressionList.Aggregate(Expression.OrElse);
                        var filter = Expression.Lambda<Func<Record, bool>>(queryExpr, param);
                        List<Record> records = ExplorerDao.QueryAll(filter).ToList();
                        var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                        rri.Records = records.ConvertAll(e => ConvertUtil.ConvertToBaseRecordDto(e, accountMap));
                        rri.RelateRecordCount = records.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get releatedInfo[tab], ERROR:{0}", ex.ToString());
            }
            return rri;
        }


        private Record GetLongestFileHold(List<Record> holdFiles)
        {
            Record tempRecord = null;
            if (holdFiles.Count > 0)
            {
                if (holdFiles.Count > 1)
                {
                    tempRecord = holdFiles[0];
                }
                else
                {
                    return holdFiles[0];
                }
            }
            ArgumentCheck.NotNull(tempRecord, nameof(tempRecord));
            foreach (var holdFile in holdFiles)
            {
                if (holdFile.HoldReleaseTime > tempRecord.HoldReleaseTime)
                {
                    tempRecord = holdFile;
                }
            }
            return tempRecord;
        }

        private void PlaceHoldWithConflictedResolution(List<Guid> ids, HoldSettingDto holdDto, List<CompactRecord> fileIds, string holdName = "", AuditAction actionType = AuditAction.CreateHoldTypeWithRecord)
        {
            if (holdDto.NeedCheckConflicted)
            {
                if (holdDto.IsOverride)   //使用当前的Setting, 覆盖所有的子节点
                {
                    this.PlaceHold(ids, holdDto, fileIds, holdName, actionType);
                    var fileAlliances = ExplorerDao.QueryAll(r => ids.Contains(r.BoxId) && r.HoldStatus);
                    this.CancelHoldByRecords(fileAlliances.Select(a => a.Id).ToList(), true);
                }
                else
                {   //查找子节点的Hold Setting, 寻找最大的那个Hold使用, 每个Box找自己的.
                    List<CompactRecord> files = fileIds.Where(a => a.NodeType == RMNodeType.PhyFile).ToList();
                    if (files.Count > 0)
                    {
                        //先处理File级别的节点
                        this.PlaceHold(files.Select(a => a.Id).ToList(), holdDto, files, holdName, actionType);
                    }
                    List<CompactRecord> boxes = fileIds.Where(a => a.NodeType == RMNodeType.PhyBox).ToList();
                    foreach (CompactRecord box in boxes)
                    {
                        var holdFiles = ExplorerDao.QueryAll(r => r.BoxId == box.Id && r.HoldStatus).ToList();
                        List<RMHold> fileHolds = HoldDao.GetHoldByIds(holdFiles.Select(r => r.HoldId).ToList());
                        Record longestHoldRecord = this.GetLongestFileHold(holdFiles);
                        if (longestHoldRecord == null || holdDto.ReleaseTime > longestHoldRecord.HoldReleaseTime)
                        {
                            this.PlaceHold(new List<Guid>() { box.Id }, holdDto, fileIds, holdName, actionType);
                        }
                        else
                        {
                            ExplorerDao.UpdateAll(r => box.Id == r.Id, s =>
                            {
                                s.HoldStatus = true;
                                s.HoldType = longestHoldRecord.HoldType;
                                s.HoldBy = longestHoldRecord.HoldBy;
                                s.HoldReleaseTime = longestHoldRecord.HoldReleaseTime;
                                s.HoldId = longestHoldRecord.HoldId;
                                s.HoldStatus = longestHoldRecord.HoldStatus;
                                s.HoldByUsers = longestHoldRecord.HoldByUsers;
                                s.HoldUntilTimes = longestHoldRecord.HoldUntilTimes;
                                s.AppendHolds_Array = longestHoldRecord.AppendHolds_Array;
                            });
                        }
                        //this.PlaceHold(new List<Guid>() { box.Id }, longestHold, fileIds);
                    }
                    var boxIds = boxes.Select(a => a.Id).ToList();
                    var fileAlliances = ExplorerDao.QueryAll(r => boxIds.Contains(r.BoxId) && r.HoldStatus);
                    this.CancelHoldByRecords(fileAlliances.Select(a => a.Id).ToList(), true);
                }
            }
        }

        private string GetAllHoldIdsString(Record item)
        {
            // Primary source: HoldUntilTimes
            if (!string.IsNullOrEmpty(item.HoldUntilTimes))
            {
                var holdUntilTimes = JsonConvert.DeserializeObject<List<HoldUntilTime>>(item.HoldUntilTimes);
                if (holdUntilTimes != null && holdUntilTimes.Count > 0)
                {
                    return string.Join(",", holdUntilTimes.Select(h => h.HoldId).Where(id => !string.IsNullOrEmpty(id)).Distinct());
                }
            }
            // Fallback
            return string.Join(",", GetAllExistHoldIds(item));
        }
        private async Task UpdateReturnDateAndSendEmailAsync(List<RMRecordLoanAlliance> itemsUpdate, List<Guid> itemIds, bool IsSendEmailToBorrower)
        {
            var dateAppliedHold = DateTime.UtcNow.Ticks;
            this.UpdateReturnDateWhenPlacedHoldToItemOnLoan(itemsUpdate, dateAppliedHold);
            if (IsSendEmailToBorrower)
            {
                foreach (var id in itemsUpdate.Select(x=>x.RecordsId))
                {
                    var phyFileInfo = ExplorerDao.GetPhysicalRecordById(id);
                    phyFileInfo.AppendCustomColumns();
                    var dicBorrower = phyFileInfo.CustomColumnDic
                            .Where(kv => kv.Value is CustomColumn cv && cv.Users != null)
                            .SelectMany(kv => ((CustomColumn)kv.Value).Users)
                            .ToList();
                    var user = dicBorrower.FirstOrDefault();
                    if (phyFileInfo == null || user == null)
                    {
                        logger.Info($"Skip to send email notification to borrower for item id {0} because of physical item is null or no anyone loan", id);
                        continue;
                    }
                    await SendEmailAsync(phyFileInfo, user, dateAppliedHold);
                }
            }
        }
        private async System.Threading.Tasks.Task SendEmailAsync(Record phyItemInfo, AOSUserDto user, long dateAppliedHold)
        {
            EmailTemplateDto template = EmailTemplateService.GetEmailTemplateByInternalType(EmailTemplateInternalType.BorrowerNotification);
            try
            {
                var emailSender = new RMEmailSender(new RMEmailMemoryStorage(new RMEmailStorageDefaultMiddleware()));
                var parameter = new RMBorrowerNotificationEmailTemplateParameters();
                parameter.PhysicalItemName = phyItemInfo.LeafName;
                parameter.PhysicalRecordName = phyItemInfo.LeafName;
                parameter.PhysicalRecordUID = phyItemInfo.RecordsId;
                parameter.BorrowerName = user.DisplayName;
                parameter.ReturnDate = new DateTime(dateAppliedHold).ToString();
                parameter.ToUser = user.UserPrincipalName;
                parameter.TemplateType = RMEmailTemplateType.BorrowerNotification;
                emailSender.Add(template.UniqueId, parameter);
                await emailSender.SendAsync();
                logger.Info($"Succeed send email to borrower.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while send email to borrower. Error: {e}");
            }
        }
        
        private async Task SendEmailToHoldManagers(RMHold hold, List<ToUserInfo> holdManagers)
        {
            try
            {
                var emailSender = new RMEmailSender(new RMEmailMemoryStorage(new RMEmailStorageDefaultMiddleware()));
                var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                if (holdManagers != null && holdManagers.Any())
                {
                    var notifications = new List<UserHoldNotification>();
                    foreach (var user in holdManagers)
                    {
                        var parameter = new RMHoldManagerEmailTemplateParameters();
                        parameter.HoldManager = user.DisplayName?? user.UserPrincipalName;
                        parameter.HoldName = hold.Name;
                        parameter.ToUser = user.UserPrincipalName;
                        parameter.TemplateType = RMEmailTemplateType.HoldManagerNotification;
                        emailSender.Add(RMEmailTemplateId.HOLD_MANAGER_NOTIFICATION, parameter);
                        await emailSender.SendAsync();

                        logger.Info($"Succeed send email to Hold manager.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while send email to Hold managers. Error: {e}");
            }

        }
        private void UpdateReturnDateWhenPlacedHoldToItemOnLoan(List<RMRecordLoanAlliance> listRecordIds, long dateAppliedHold)
        {
            foreach (var item in listRecordIds)
            {
                item.HoldReleaseTime = dateAppliedHold;
                RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(item);
            }
        }

        public void AssignRecordsToHoldAsync(UpdateHoldDto dto, string currentHoldBy)
        {
            logger.Info($"AssignRecordsToHoldAsync start. HoldId: {dto.HoldSetting.Id}, RecordCount: {dto.ReletedIds.Count}.");
            DateTime tempUtcReleaseTime = this.CalculateHoldReleaseTime(dto.HoldSetting);
            HoldSettingDto holdDto = new HoldSettingDto()
            {
                HoldId = dto.HoldSetting.Id,
                AllianceType = dto.HoldCategory,
                ReleaseTime = tempUtcReleaseTime.Ticks,
                HoldBy = currentHoldBy,
                NeedCheckConflicted = dto.NeedCheckOverride,
                IsOverride = dto.IsOverRide,
            };
            if(holdDto.NeedCheckConflicted)
            {
                this.PlaceHoldWithConflictedResolution(dto.ReletedIds, holdDto, dto.FileIds ?? new List<CompactRecord>(), dto.HoldSetting.Name, AuditAction.ImportHoldRecords);
            }
            else
            {
                PlaceHold(dto.ReletedIds, holdDto, dto.FileIds ?? new List<CompactRecord>(), dto.HoldSetting.Name, AuditAction.ImportHoldRecords);
            }
            logger.Info($"AssignRecordsToHoldAsync complete. HoldId: {holdDto?.HoldId}.");
        }

        private void PlaceHold(List<Guid> ids, HoldSettingDto holdDto, List<CompactRecord> fileIds, string holdName = "", AuditAction actionType = AuditAction.CreateHoldTypeWithRecord)
        {
            var caculateDisposalDueDate = DateTime.MinValue.Ticks;
            var tempExplorers = ExplorerDao.GetRecordByIds(ids);
            var account = AccountDao.GetActiveUserByUserIdAsync(TenantLocalValue.LogonUserId).GetAwaiter().GetResult();
            if (tempExplorers != null && tempExplorers.Count > 0)
            {
                var physicalItems = tempExplorers.Where(item => item.SourceFlag == (int)SourceFlag.Physical).ToDictionary(item => item.Id, item => GetAllHoldIdsString(item));
                RecordsHistoryService.AddPhysicalHoldActionAudit(physicalItems, holdDto, holdName, actionType);
                foreach (var tempExplorerItem in tempExplorers)
                {
                    Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(tempExplorerItem, out string[] appendHoldsArray, holdDto);
                    long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                    string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                    var allHolds = new List<string>(appendHoldsArray) { firstMaxHoldSettingId };
                    List<HoldUser> allHoldByUsers = GetAllHoldByUsers(tempExplorerItem);
                    allHoldByUsers.Add(new HoldUser() { HoldId = holdDto.HoldId, HoldBy = holdDto.HoldBy });
                    allHoldByUsers = allHoldByUsers.Where(h => allHolds.Contains(h.HoldId)).ToList();
                    List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(tempExplorerItem);
                    allHoldUntilTimes.Add(new HoldUntilTime() { HoldId = holdDto.HoldId, UntilTime = holdDto.ReleaseTime });
                    allHoldUntilTimes = allHoldUntilTimes.Where(h => allHolds.Contains(h.HoldId)).ToList();

                    var isRemoveRuleData = false;
                    if (tempExplorerItem.RuleId != null && tempExplorerItem.RuleId != Guid.Empty)
                    {
                        var tempRule = RMRuleDao.GetRuleById(tempExplorerItem.RuleId);
                        if (tempRule != null && IsRemoveRule(tempRule, tempExplorerItem.SourceFlag))
                        {
                            isRemoveRuleData = true;
                            //Remove Rule需要计算Due Date
                            caculateDisposalDueDate = new List<long>() { tempExplorerItem.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                            //更新Remove类型Item的Due Date为新值
                            ExplorerDao.UpdateAll(r => tempExplorerItem.Id == r.Id, s =>
                            {
                                s.HoldStatus = true;
                                s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                                s.HoldReleaseTime = firstMaxHoldTime;
                                s.HoldId = firstMaxHoldSettingId;
                                s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                                s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                                s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                                s.AppendHolds_Array = appendHoldsArray;
                                s.DisposalDueDate = caculateDisposalDueDate;
                            });
                        }
                    }
                    if (!isRemoveRuleData)
                    {
                        ExplorerDao.UpdateAll(r => tempExplorerItem.Id == r.Id, s =>
                        {
                            s.HoldStatus = true;
                            s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                            s.HoldReleaseTime = firstMaxHoldTime;
                            s.HoldId = firstMaxHoldSettingId;
                            s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                            s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                            s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            s.AppendHolds_Array = appendHoldsArray;                            
                        });
                    }
                }
            }
            //由于目前每条Record需要和自己的其他Hold比较，所以不存在统一更新的情况了
            ////不需要计算Due Date的记录统一更新Hold状态
            //ExplorerDao.UpdateAll(r => noneRemoveRuleIds.Contains(r.Id), s => { s.HoldStatus = true; s.HoldType = RecordsConstants.RecordHold_PhyProfile; s.HoldReleaseTime = holdDto.ReleaseTime; s.HoldId = holdDto.HoldId; s.HoldBy = holdDto.HoldBy; });
        }
  
        private static List<HoldUntilTime> GetAllHoldUntilTimes(Record tempExplorerItem)
        {
            var allHoldUntilTimes = string.IsNullOrEmpty(tempExplorerItem.HoldUntilTimes) ? new List<HoldUntilTime>() : JsonConvert.DeserializeObject<List<HoldUntilTime>>(tempExplorerItem.HoldUntilTimes);
            if (tempExplorerItem.HoldStatus && allHoldUntilTimes.Count == 0)
            {
                allHoldUntilTimes.Add(new HoldUntilTime() { HoldId = tempExplorerItem.HoldId, UntilTime = tempExplorerItem.HoldReleaseTime });
            }
            return allHoldUntilTimes;
        }
        private static List<HoldUser> GetAllHoldByUsers(Record tempExplorerItem)
        {
            List<HoldUser> allHoldByUsers = string.IsNullOrEmpty(tempExplorerItem.HoldByUsers) ? new List<HoldUser>() : JsonConvert.DeserializeObject<List<HoldUser>>(tempExplorerItem.HoldByUsers);
            if (tempExplorerItem.HoldStatus && allHoldByUsers.Count == 0)
            {
                allHoldByUsers.Add(new HoldUser() { HoldId = tempExplorerItem.HoldId, HoldBy = tempExplorerItem.HoldBy });
            }
            return allHoldByUsers;
        }

        private Tuple<long, string> GetMaxHoldTime(Record tempExplorerItem, out string[] appendHoldsArray, HoldSettingDto holdDto, List<string> removeHoldIds = null)
        {
            Tuple<long, string> holdTuple = null;
            string firstMaxHoldSettingId = string.Empty;
            long firstMaxHoldTime = 0;//最长时间的Hold Time相同，以第一个hold为准
            
            List<Tuple<long, string>> holdTimeAndHoldIdList = new List<Tuple<long, string>>();
            if (holdDto == null || holdDto.HoldAction != RecordsConstants.HOLD_ACTION_CHANGE)
            {
                var recordAllExistHoldIds = GetAllExistHoldIds(tempExplorerItem);
                if (removeHoldIds != null)
                {
                    recordAllExistHoldIds.RemoveAll(h => removeHoldIds.Contains(h));
                }

                var recordAllExistHolds = HoldDao.GetHoldByIds(recordAllExistHoldIds);

                List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(tempExplorerItem);

                foreach (var hold in recordAllExistHolds)
                {
                    long? untilTime = allHoldUntilTimes.FirstOrDefault(h => h.HoldId == hold.Id)?.UntilTime;
                    if (untilTime.HasValue)
                    {
                        holdTimeAndHoldIdList.Add(new Tuple<long, string>(untilTime.Value, hold.Id));
                    }
                }
            }

            if (holdDto != null && !holdTimeAndHoldIdList.Any(h => h.Item2 == holdDto.HoldId))
            {
                holdTimeAndHoldIdList.Add(new Tuple<long, string>(holdDto.ReleaseTime, holdDto.HoldId));
            }
            holdTimeAndHoldIdList = holdTimeAndHoldIdList.Distinct().ToList();

            foreach (var holdTimeAndHoldId in holdTimeAndHoldIdList)
            {
                if (holdTimeAndHoldId.Item1 > firstMaxHoldTime)
                {
                    holdTuple = holdTimeAndHoldId;
                    firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                    firstMaxHoldTime = holdTimeAndHoldId.Item1;
                }
            }
            appendHoldsArray = holdTimeAndHoldIdList.Select(h => h.Item2).Where(h => h != firstMaxHoldSettingId).ToArray();
            return holdTuple;
        }

        private void CancelHoldBySelected(List<string> holdIds, Record record)
        {
            var caculateDisposalDueDate = DateTime.MinValue.Ticks;
            Tuple<long, string> holdTimeAndHoldId = GetMaxHoldTime(record, out string[] appendHoldsArray, null, holdIds);
            if (holdTimeAndHoldId == null)
            {
                ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                {
                    s.HoldStatus = false;
                    s.HoldType = 0;
                    s.HoldReleaseTime = DateTime.MinValue.Ticks;
                    s.HoldId = null; s.HoldBy = null;
                    s.HoldByUsers = null;
                    s.HoldUntilTimes = null;
                    s.AppendHolds_Array = new string[0];
                    s.DisposalDueDate = s.PreviosDisposalDueDate;
                });
            }
            else
            {
                long firstMaxHoldTime = holdTimeAndHoldId.Item1;
                string firstMaxHoldSettingId = holdTimeAndHoldId.Item2;
                var allHolds = new List<string>(appendHoldsArray) { firstMaxHoldSettingId };
                
                var allHoldByUsers = GetAllHoldByUsers(record);
                allHoldByUsers = allHoldByUsers.Where(h => allHolds.Contains(h.HoldId)).ToList();

                List<HoldUntilTime> allHoldUntilTimes = GetAllHoldUntilTimes(record);
                allHoldUntilTimes = allHoldUntilTimes.Where(h => allHolds.Contains(h.HoldId)).ToList();

                var isRemoveRuleData = false;
                if (record.RuleId != null && record.RuleId != Guid.Empty)
                {
                    var tempRule = RMRuleDao.GetRuleById(record.RuleId);
                    if (tempRule != null && IsRemoveRule(tempRule, record.SourceFlag))
                    {
                        isRemoveRuleData = true;
                        //Remove Rule需要计算Due Date
                        caculateDisposalDueDate = new List<long>() { record.PreviosDisposalDueDate, firstMaxHoldTime }.Max();
                        //更新Remove类型Item的Due Date为新值
                        ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                        {
                            s.HoldStatus = true;
                            s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                            s.HoldReleaseTime = firstMaxHoldTime;
                            s.HoldId = firstMaxHoldSettingId;
                            s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                            s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                            s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                            s.AppendHolds_Array = appendHoldsArray;
                            s.DisposalDueDate = caculateDisposalDueDate;
                        });
                    }
                }
                if (!isRemoveRuleData)
                {
                    ExplorerDao.UpdateAll(r => record.Id == r.Id, s =>
                    {
                        s.HoldStatus = true;
                        s.HoldType = RecordsConstants.RecordHold_PhyProfile;
                        s.HoldReleaseTime = firstMaxHoldTime;
                        s.HoldId = firstMaxHoldSettingId;
                        s.HoldBy = allHoldByUsers.FirstOrDefault(u => u.HoldId == firstMaxHoldSettingId)?.HoldBy;
                        s.HoldByUsers = JsonConvert.SerializeObject(allHoldByUsers);
                        s.HoldUntilTimes = JsonConvert.SerializeObject(allHoldUntilTimes);
                        s.AppendHolds_Array = appendHoldsArray;
                    });
                }
            }
        }

        private List<string> GetAllExistHoldIds(Record tempExplorerItem)
        {
            List<string> recordAllExistHoldIds = new List<string>();
            if (!string.IsNullOrEmpty(tempExplorerItem.HoldId))
            {
                recordAllExistHoldIds.Add(tempExplorerItem.HoldId);
            }
            if (tempExplorerItem.AppendHolds_Array != null)
            {
                recordAllExistHoldIds.AddRange(tempExplorerItem.AppendHolds_Array.ToList());
            }
            return recordAllExistHoldIds;
        }

        private bool IsRemoveRule(RMRule tempRule, int sourceFlag)
        {
            var result = false;
            int disposalAction = -1;
            if ((int)SourceFlag.SharePoint == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.DisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 25 || disposalAction == 26
                || disposalAction == 28 || disposalAction == 29 || disposalAction == 31 || disposalAction == 130 || disposalAction == 135
                || disposalAction == 138 || disposalAction == 143 || disposalAction == 146 || disposalAction == 151 || disposalAction == 154
                || disposalAction == 156 || disposalAction == 159)
                {
                    result = true;
                }
            }
            if ((int)SourceFlag.OneDrive == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.OneDriveDisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 25 || disposalAction == 26
                || disposalAction == 28 || disposalAction == 29 || disposalAction == 31 || disposalAction == 130 || disposalAction == 135
                || disposalAction == 138 || disposalAction == 143 || disposalAction == 146 || disposalAction == 151 || disposalAction == 154
                || disposalAction == 156 || disposalAction == 159)
                {
                    result = true;
                }
            }
            else if ((int)SourceFlag.SharePointOnPrem == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.SPLocalDisposalAction);
                if (disposalAction == 0 || disposalAction == 2 || disposalAction == 5 || disposalAction == 7 || disposalAction == 8
                || disposalAction == 10 || disposalAction == 13 || disposalAction == 15 || disposalAction == 16 || disposalAction == 18
                || disposalAction == 21 || disposalAction == 23 || disposalAction == 24 || disposalAction == 26 || disposalAction == 29
                || disposalAction == 31 || disposalAction == 130 || disposalAction == 135 || disposalAction == 138 || disposalAction == 143
                || disposalAction == 146 || disposalAction == 151 || disposalAction == 154 || disposalAction == 159)
                {
                    result = true;
                }
            }
            else if ((int)SourceFlag.Exchange == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.ExchangeDisposalAction);
                if (disposalAction == 0)
                {
                    result = true;
                }
            }
            else if ((int)SourceFlag.Physical == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.PhysicalDisposalAction);
                if (disposalAction == (int)RMContentDisposalAction.Remove)
                {
                    return true;
                }
            }
            else if ((int)SourceFlag.FileSystem == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.FSDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
                        return true;
                    default:
                        break;
                }
            }
            else if ((int)SourceFlag.AzureFileShare == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.AzureFileDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
                        return true;
                    default:
                        break;
                }
            }
            else if ((int)SourceFlag.Box == sourceFlag)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.BoxDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                        return true;
                    default:
                        break;
                }
            }
            else if ((int)SourceFlag.Teams == sourceFlag)
            {
                var validDisposalActions = new HashSet<int>
                {
                    0, 2, 5, 7, 8, 10, 13, 15, 16, 18,
                    21, 23, 24, 25, 26, 28, 29, 31,
                    130, 135, 138, 143, 146, 151, 154, 156, 159
                };
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.DisposalAction);
                if (validDisposalActions.Contains(disposalAction))
                {
                    result = true;
                }
            }
            else if(sourceFlag >= 1000)
            {
                disposalAction = RuleHelper.GetOldLogicDisposalAction(tempRule.ConnectorDisposalAction);
                switch (disposalAction)
                {
                    case (int)RMContentDisposalAction.Remove:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.RelatedRecords:
                    case (int)RMContentDisposalAction.Remove | (int)RMContentDisposalAction.LeaveStub | (int)RMContentDisposalAction.RelatedRecords:
                        return true;
                    default:
                        break;
                }
            }
            return result;
        }

        private async Task<List<RecordHistory>> GetHistoryInfoAsync(string historyInfo, Guid id, bool isControlPlus = false)
        {
            return await RecordsHistoryService.GetRecordsHistoryAsync(historyInfo, id, isControlPlus);

            //var historyList = new List<RecordHistory>();
            //if (!string.IsNullOrEmpty(historyInfo))
            //{
            //    historyList = XmlUtil.GetXmlObject<RecordHistoryXml>(historyInfo).HistoryList;
            //    historyList = historyList.OrderByDescending(o => o.TimeUTC).ToList();
            //    GeneralSettingModel gls = GeneralSettingService.GetGeneralSetting();
            //    foreach (var item in historyList)
            //    {
            //        if (item.TimeUTC != 0)
            //        {
            //            item.DisplayTime = GeneralSettingService.ConvertTiksToDateTime(gls, item.TimeUTC, true).SimplifyFormatTime;
            //        }
            //        item.Action = I18NEntity.GetStringWithSeparator(item.Action);
            //    }
            //}
            //else
            //{

            //}
            //return historyList;
        }

        //public void UpdateCollectionTime(Guid scopeId, long timeTicks)
        //{
        //    ExplorerDao.UpdateCollectionTime(scopeId, timeTicks);
        //}

        public List<Guid> GetChangeTermIds(long ticks)
        {
            try
            {
                List<Guid> subTerms = new List<Guid>();
                List<Guid> allTerms = ChangeClassificationDao.GetAllChange(ticks, (int)TermChangeType.TermRule);
                foreach (var id in allTerms)
                {
                    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
                }
                allTerms.AddRange(subTerms);
                return allTerms;
            }
            catch (Exception e)
            {
                logger.Info("get change term ids error {0}", e.ToString());
                return new List<Guid>();
            }
        }

        public Dictionary<Guid, long> GetChangedTerms(long ticks)
        {
            try
            {
                var changedTerms = new Dictionary<Guid, long>();
                var allTerms = ChangeClassificationDao.GetAllChangedInfo(ticks, (int)TermChangeType.TermRule);

                foreach(var term in allTerms)
                {
                    var changedTime = term.ChangeTime;

                    if(changedTerms.TryGetValue(term.TermId, out var lastChangedTime) && lastChangedTime > changedTime)
                    {
                        changedTime = lastChangedTime;
                    }

                    var subTerms = TermDao.GetAllSubTermUniqueIds(term.TermId);
                    subTerms.ForEach(item =>
                    {
                        var subTermChangedTime = changedTime;
                        if(changedTerms.TryGetValue(item, out var subTermLastChangedTime) && subTermLastChangedTime > subTermChangedTime)
                        {
                            subTermChangedTime = subTermLastChangedTime;
                        }
                        changedTerms[item] = subTermChangedTime;
                    });

                    changedTerms[term.TermId] = changedTime;
                }

                return changedTerms;
            }
            catch (Exception e)
            {
                logger.Info("get change terms error {0}", e.ToString());
                return new Dictionary<Guid, long>();
            }
        }

        public async System.Threading.Tasks.Task RemoveAllChangeTermIdsAsync()
        {
            try
            {
                await ChangeClassificationDao.RemoveChangeAsync((int)TermChangeType.TermRule);
            }
            catch (Exception e)
            {
                logger.Warn("remove change terms error {0}", e.ToString());
            }
        }

        public List<BaseRecordDto> GetObjectDatas(Guid scopeId, List<Guid> termIds, long ticks)
        {
            List<BaseRecordDto> dtos = new List<BaseRecordDto>();
            try
            {
                var datas = ExplorerDao.QueryAll(m => termIds.Any(TermId => m.TermId == TermId) && m.ScopeId == scopeId && m.ItemRowId != 0 && m.CollectTime < ticks).ToList();
                //var datas = CollectionDataDao.GetRecordsByTerms(scopeId, termIds, ticks);
                if (datas != null)
                {
                    var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                    foreach (var data in datas)
                    {
                        var dto = ConvertUtil.ConvertToBaseRecordDto(data, accountMap);
                        dtos.Add(dto);
                    }
                }
                return dtos;
            }
            catch (Exception ex)
            {
                logger.Info("Get Object Data failed {0}", ex.ToString());
                //logger.Error("get data by id:{0}, archived:{1}, error:{2}", key.ToString(), isArchived, ex.ToString());
            }
            return dtos;

        }
        public bool IsExplorerDBDataExist()
        {
            bool exist = false;
            try
            {
                exist = ExplorerDao.CheckHasData();
            }
            catch (Exception ex)
            {
                logger.Error("check db data error:{0}", ex.ToString());
            }
            return exist;
        }


        #endregion

        #endregion

        #region Move

        //private bool IsSPAdmin(string userId)
        //{
        //    return UserService.DoesUserHasThisPermission(TenantLocalValue.LogonGroupId, userId, RMPermissionMasks.SPOAdmin);
        //}

        //private bool IsOneDriveAdmin(string userId)
        //{
        //    return UserService.DoesUserHasThisPermission(TenantLocalValue.LogonGroupId, userId, RMPermissionMasks.OneDriveAdmin);
        //}

        //private bool IsAdmin(string userId, RemoveNodeType nodeType)
        //{
        //    bool isAdmin = false;
        //    if (nodeType == RemoveNodeType.SkyDrivePro)
        //    {
        //        isAdmin = IsOneDriveAdmin(userId);
        //    }
        //    else
        //    {
        //        isAdmin = IsSPAdmin(userId);
        //    }
        //    return isAdmin;
        //}
        //public string AddMoveJobTODBJobQueue(MoveToDto dto)
        //{
        //    string id = string.Empty;
        //    RemoteSiteCollection site = null;
        //    if (dto.DestMode == Contract.RMWeb.DestMode.SharePoint)
        //    {
        //        if (dto.IsSpecifyLocation)
        //        {
        //            site = RABrowserClient.GetRemoteSiteCollectionByListUrl(dto.LocationPath);
        //        }
        //        else
        //        {
        //            var siteCollNode = GetSiteCollectionNode(dto.SPTree);
        //            site = RABrowserClient.GetRemoteSiteCollectionById(siteCollNode.SPObjectId);
        //        }
        //    }
        //    //TODO? 
        //    var account = AccountDao.GetActiveUserByName(TenantLocalValue.LogonUserEmail);
        //    if (!IsAdmin(account.UserId, site.NodeType))
        //    {
        //        List<string> userAndGroupUserIds = UserService.GetUserAndGroupUserIds(account.UserId);
        //        if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(site.parentId), userAndGroupUserIds))
        //        {
        //            logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.DesUrl:{(dto.IsSpecifyLocation ? dto.LocationPath : dto.SPTree.FullPath)}.");
        //            return null;
        //        }
        //    }

        //    try
        //    {
        //        var jobMsg = ConvertToRMExplorerMoveJobMessage(dto);
        //        jobMsg.Operator = TenantLocalValue.LogonUserEmail;
        //        var groupId = TenantLocalValue.LogonGroupId;
        //        var loginName = TenantLocalValue.LogonUserEmail;
        //        JobQueueDto jqDto = new JobQueueDto()
        //        {
        //            JobType = JobType.RecordsExplorerMove,
        //            JobRunType = JobRunBy.Control,
        //            TenantGroupId = groupId,
        //            JobRunByUser = loginName,
        //            Parameters = SerializerHelper.SerializeToXmlString(jobMsg)
        //        };
        //        id = mJobQueueService.AddToDBJobQueue(jqDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("An error occurr while MoveTo, reason : {0}.", ex.ToString());
        //    }
        //    return id;
        //}

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ExplorerRecordsMove, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public string RunMoveToJob(string jobRunBy, string param)
        {
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(JobType.RecordsExplorerMove, jobRunBy);
            StartExplorerMoveJob(param, jobId);
            return jobId;
        }

      

        private string GetDestUrl(ARE.MoveDestination desInfo)
        {
            string destinationContainerUrl = null;
            if (desInfo.DestMode == ARE.DestMode.UrlMode)
            {
                destinationContainerUrl = desInfo.SPUrl;
            }
            else if (desInfo.DestMode == ARE.DestMode.TreeMode)
            {
                destinationContainerUrl = desInfo.SPTreeNode.FullPath;
            }
            //destinationContainerUrl = HttpUtility.UrlDecode(destinationContainerUrl);
            return destinationContainerUrl;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void StartExplorerMoveJob(string jobMsg, string jobId)
        {
            try
            {
                string destUrl = null;
                RMExplorerMoveJobMessage JobMessage = SerializerHelper.DeserializeFromXmlString<RMExplorerMoveJobMessage>(jobMsg);
                if (JobMessage != null)
                {
                    destUrl = GetDestUrl(JobMessage.MoveDestination);
                }
                logger.Debug("start move job destination url {0}", destUrl);
                //Move job only have 1 sub job in a main job
                SubJobDao.UpdateSubJobCount(jobId, 1);
                List<string> runningId = SubJobDao.GetRunningMoveSubJobByDest(destUrl, true);
                if (runningId.IsNullOrEmpty())
                {
                    var subJobId = this.CreateSubJob(jobId, 0, JobType.RecordsExplorerMove, JobStatus.InProgress, 1, jobMsg, destUrl);
                    logger.Info(string.Format("Start explorer move job : {0}", subJobId));
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = JobType.RecordsExplorerMove,
                        CommandLine = string.Format("{0} {1}", JobType.RecordsExplorerMove.ToString(), subJobId),
                    });
                    logger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
                }
                else
                {
                    var subJobId = this.CreateSubJob(jobId, 0, JobType.RecordsExplorerMove, JobStatus.Wait, 1, jobMsg, destUrl);
                    logger.Info("Move records job {0} can not run, for there is {1} using the same destination {2}", subJobId, string.Join(",", runningId.ToArray()), destUrl);
                }
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Error in start explorer move job, reason : {0}.", e.ToString()));
                //return new Adonis.Records.Object.RecordsReturnMessage() { ExceptionMessage = e.Message, ResultType = Adonis.Records.Object.ResultType.Failed };
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Runable = jobState == JobStatus.InProgress ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }

        [Audit(Action = AuditAction.RunPhysicalExplorerTimer, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public string RealRunPhysicalTimerJob(string param, JobRunBy JobRunType)
        {
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                jobId = RMJobService.CreateJob(JobType.PhysicalExplorerTimer, jobRunByUser);
                //StartExplorerMoveJob(param, jobId);
                List<string> runningJobs = RMJobService.GetRunningJobs(JobType.PhysicalExplorerTimer);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExplorerTimer,
                        CommandLine = string.Format("{0} {1}", AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExplorerTimer, jobId),
                    });
                    logger.Info($"run physical explorer timer job success, JobId : {jobId}.");
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                    logger.Info("enforce retention job has job running,so shedule job is skip");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunPhysicalTimerJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        public string RunPhysicalTimerJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            var jobType = JobType.PhysicalExplorerTimer;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while run physical explorer timer job, ERROR : {ex.ToString()}.");
            }
            return id;
        }

        [Audit(Action = AuditAction.RunConnectorExplorerTimer, Category = AuditCategory.CustomizeConnector, Module = AuditModule.CustomizeConnector, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public string RealRunConnectorTimerJob(string param, JobRunBy JobRunType)
        {
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                jobId = RMJobService.CreateJob(JobType.ConnectorTimer, jobRunByUser);
                //StartExplorerMoveJob(param, jobId);
                List<string> runningJobs = RMJobService.GetRunningJobs(JobType.ConnectorTimer);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = AvePoint.RA.Contract.JobMonitor.JobType.ConnectorTimer,
                        CommandLine = string.Format("{0} {1}", AvePoint.RA.Contract.JobMonitor.JobType.ConnectorTimer, jobId),
                    });
                    logger.Info($"run connector timer job success, JobId : {jobId}.");
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                    logger.Info("connector timer job has job running,so shedule job is skip");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunConnectorTimerJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        public string RunConnectorTimerJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            var jobType = JobType.ConnectorTimer;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while run physical explorer timer job, ERROR : {ex.ToString()}.");
            }
            return id;
        }


        public List<Office365AccountInfo> GetAllO365Accounts()
        {
            throw new NotImplementedException();
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.MoveCheckSPUrl, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public Task<CheckLocationObject> CheckSPUrlAsync(string locationPath, RMAccountProfileDto account)
        {
            return InternalCheckSPUrlAsync(locationPath, account);
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.MoveCheckSPUrl, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public Task<CheckLocationObject> CheckSPUrl4RuleAsync(string locationPath, RMAccountProfileDto account)
        {
            return InternalCheckSPUrlAsync(locationPath, account);
        }

        // check url in the job process, no need audit
        public DestinationSPOLocationInfo CheckSPUrl4Job(string locationPath, RMAccountProfileDto account, bool isSupportSiteLevel = false)
        {
            return InternalCheckSPLibOrFolderUrlAsync(locationPath, account, isSupportSiteLevel).GetAwaiter().GetResult();
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.MoveCheckSPUrl, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public CheckLocationObject CheckUNCLocation(string locationPath, RMAccountProfileDto account)
        {
            //return InternalCheckUNCLocation(locationPath, null);
            throw new NotImplementedException();
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.RuleManagement, Action = AuditAction.MoveCheckSPUrl, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<CheckLocationObject> CheckUNCLocation4RuleAsync(string locationPath, Office365AccountInfo account)
        {
            //return InternalCheckUNCLocation(locationPath, null);
            var connection = FSConnectionDao.GetParentConnectionInfo(locationPath);
            if (connection == null)
            {
                throw new Exception("Connection not config");
            }
            else
            {
                var connectionDto = new ConnectionDto() { UNCPath = locationPath };
                if (await RMFileSystemBrowserService.ValidationTestConnectionAsync(connectionDto))
                {
                    return new CheckLocationObject() { AveSiteId = connection.Id, DestRootPath = locationPath };
                }
                else
                {
                    return null;
                    //throw new Exception("Invalidate path");
                }
            }
        }
        private async Task<CheckLocationObject> InternalCheckSPUrlAsync(string locationPath, RMAccountProfileDto account)
        {
            try
            {
                CheckLocationObject rstObj = new CheckLocationObject();

                DestinationLocationInfo destinationInfo = new DestinationLocationInfo();
                if (account == null)
                {
                    destinationInfo.Url = locationPath;
                }
                else
                {

                }
                RMExplorerUtility utility = new RMExplorerUtility();
                rstObj = await utility.ValidationDestUrlForRAAsync(locationPath);
                return rstObj;
            }
            catch (Exception e)
            {
                logger.Warn("CheckSPUrl error: {0}", e.ToString());
                return null;
            }
        }

        private async Task<DestinationSPOLocationInfo> InternalCheckSPLibOrFolderUrlAsync(string locationPath, RMAccountProfileDto account, bool isSupportSiteLevel = false)
        {
            try
            {
                DestinationSPOLocationInfo rstObj = new DestinationSPOLocationInfo();

                RMExplorerUtility utility = new RMExplorerUtility();
                rstObj = await utility.ValidationDestUrlForRestore(locationPath, isSupportSiteLevel);
                return rstObj;
            }
            catch (Exception e)
            {
                logger.Warn("CheckSPUrl error: {0}", e.ToString());
                return null;
            }
        }

        #endregion

        #region Tool Method
        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }


        private string GetJsonStrByObj(object o)
        {
            return JsonConvert.SerializeObject(o);
        }


        #endregion

        private const string BARCODE_STANDARD_KEY = "Barcode_Standard";

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeTerm, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> ChangeTermRealTimeAllSourceAsync(ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            if (changeTermOption == null)
            {
                return returnMessage;
            }
            var hasEXORecords = !changeTermOption.SourceEXORecordIds.IsNullOrEmpty();
            var hasFSRecords = !changeTermOption.SourceFSRecordIds.IsNullOrEmpty();
            var hasOneDriveRecords = !changeTermOption.SourceOneDriveRecordIds.IsNullOrEmpty();
            var hasGoogleRecords = !changeTermOption.GoogleDriveRecordIds.IsNullOrEmpty();

            if (!changeTermOption.SourceRecordIds.IsNullOrEmpty())
            {
                returnMessage = await ChangeTermRealTimeSPAsync(changeTermOption, jobId, hasEXORecords);
            }
            if (hasEXORecords)
            {
                returnMessage = ChangeTermRealTimeEXO(changeTermOption, jobId, hasFSRecords);
            }
            if (hasFSRecords)
            {
                returnMessage = ChangeTermRealTimeFS(changeTermOption, jobId);
            }
            if (hasOneDriveRecords)
            {
                returnMessage = await ChangeTermRealTimeOneDriveAsync(changeTermOption, jobId);
            }
            if (hasGoogleRecords)
            {
                returnMessage = await ChangeTermRealTimeGoogleAsync(changeTermOption, jobId);
            }

            if (!changeTermOption.SourcePhyRecordIds.IsNullOrEmpty())
            {
                returnMessage = ChangeTermRealTimePhy(changeTermOption, jobId);
            }

            if (!changeTermOption.SourceAzureFileShareRecordIds.IsNullOrEmpty())
            {
                returnMessage = new RecordsReturnMessage
                {
                    ResultType = ResultType.Success
                };
                var reclassifier = new AzureFileShareReclassifier(changeTermOption, jobId, false);
                reclassifier.Reclassify();
            }

            if (!changeTermOption.SourceBoxRecordIds.IsNullOrEmpty())
            {
                var boxReclassifier = new BoxReclassifier(changeTermOption, jobId, false);
                returnMessage = boxReclassifier.Reclassify();
            }

            if (!changeTermOption.SourceCustomizeConnectorRecordIds.IsNullOrEmpty())
            {
                returnMessage = new RecordsReturnMessage
                {
                    ResultType = ResultType.Success
                };
                var reclassifier = new CustomizeConnectorReclassifier(changeTermOption, jobId, false);
                await reclassifier.ReclassifyAsync();
            }
            //if (hasGoogleRecords)
            //{
            //    returnMessage.ResultType = ResultType.Success;
            //    try
            //    {
            //        var googleReclassifyUtil = new RMGoogleDriveExplorerUtility(jobId);
            //        await googleReclassifyUtil.ChangeAllTermsForGoogleDriveAsync(changeTermOption, jobId);
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Warn("Update terms error {0}", e.ToString());
            //        returnMessage.ResultType = ResultType.Failed;
            //        returnMessage.ExceptionMessage = e.Message;
            //    }
            //}

            if (!changeTermOption.SourceTeamsRecordIds.IsNullOrEmpty())
            {
                returnMessage = await ChangeTermRealtimeTeamsAsync(changeTermOption, jobId, false);
            }

            return returnMessage;
        }

        //[Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeLabel, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        //public async Task<RecordsReturnMessage> ChangeLabelRealTimeForGoogleAsync(ChangeLabelOption changeLabelOption, string jobId)
        //{
        //    RecordsReturnMessage returnMessage = new RecordsReturnMessage();
        //    if (changeLabelOption == null)
        //    {
        //        return returnMessage;
        //    }
        //    var hasGoogleRecords = !changeLabelOption.GoogleDriveRecordIds.IsNullOrEmpty();

        //    if (hasGoogleRecords)
        //    {
        //        returnMessage.ResultType = ResultType.Success;
        //        try
        //        {
        //            var googleReclassifyUtil = new RMGoogleDriveExplorerUtility(jobId);
        //            await googleReclassifyUtil.ChangeAllTermsForGoogleDriveAsync(changeOption, jobId);
        //        }
        //        catch (Exception e)
        //        {
        //            logger.Warn("Update labels error {0}", e.ToString());
        //            returnMessage.ResultType = ResultType.Failed;
        //        }
        //    }
        //    return returnMessage;
        //}

        public async Task<RecordsReturnMessage> ChangeTermRealTimeSPAsync(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(true);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public RecordsReturnMessage ChangeTermRealTimeEXO(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                ReclassifyUtility reclassifyUtility = new ReclassifyUtility();
                reclassifyUtility.ChangeAllTerms(changeTermOption, jobId, waiting4OtherSource);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealtimeTeamsAsync(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMTeamsExplorerUtility explorerUtility = new RMTeamsExplorerUtility(true);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
            }
            catch (Exception e)
            {
                logger.Error("An error occured while changing term realtime for Teams {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public RecordsReturnMessage ChangeTermRealTimePhy(ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                PhysicalReclassifyUtility reclassifyUtility = new PhysicalReclassifyUtility();
                reclassifyUtility.ChangeAllTermsForPhy(changeTermOption, jobId);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public RecordsReturnMessage ChangeTermRealTimeFS(ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                var reclassifyUtil = new FSReclassifyUtil();
                reclassifyUtil.ChangeAllTerms(changeTermOption, jobId, false);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealTimeOneDriveAsync(ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                var reclassifyUtil = new RMOneDriveExplorerUtility(false);
                await reclassifyUtil.ChangeAllTermsForOneDriveAsync(changeTermOption, jobId);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealTimeGoogleAsync(ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                var reclassifyUtil = new RMGoogleDriveExplorerUtility(jobId);
                await reclassifyUtil.ChangeAllTermsForGoogleDriveAsync(changeTermOption, jobId);
                var failedCount = reclassifyUtil.FailedCount;
                if (failedCount > 0)
                {
                    returnMessage.ResultType = ResultType.Failed;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.MLChangeTerm, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> ChangeTermRealTimeForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            if (changeTermOption == null)
            {
                return returnMessage;
            }
            logger.Info($"Change term SourceRecordIds:[{string.Join(",", changeTermOption.SourceRecordIds)}]");
            logger.Info($"Change term SourceOneDriveRecordIds:[{string.Join(",", changeTermOption.SourceOneDriveRecordIds)}]");
            var hasOneDriveRecords = !changeTermOption.SourceOneDriveRecordIds.IsNullOrEmpty();
            if (!changeTermOption.SourceRecordIds.IsNullOrEmpty())
            {
                logger.Info("Start change spo records");
                returnMessage = await ChangeTermRealTimeSPForAIAsync(changeTermType, changeTermOption, jobId, hasOneDriveRecords);
            }
            if (hasOneDriveRecords)
            {
                logger.Info("Start change onedrive records");
                returnMessage = await ChangeTermRealTimeOneDriveForAIAsync(changeTermType, changeTermOption, jobId);
            }
            if (!changeTermOption.SourceTeamsRecordIds.IsNullOrEmpty())
            {
                logger.Info("Start change teams records");
                returnMessage = await ChangeTermRealtimeTeamsForAIAsync(changeTermType, changeTermOption, jobId, hasOneDriveRecords);
            }
            if (!changeTermOption.GoogleDriveRecordIds.IsNullOrEmpty())
            {
                logger.Info("Start change google records");
                returnMessage = await ChangeTermRealTimeGoogleForAIAsync(changeTermType, changeTermOption, jobId);
            }
            if (RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot && (changeTermType == ChangeTermType.AIMADirectlyApprove || changeTermType == ChangeTermType.AIMAChangeTerm))
            {
                var recordIds = new List<Guid>();
                recordIds.AddRange(changeTermOption.SourceTeamsRecordIds);
                recordIds.AddRange(changeTermOption.SourceRecordIds);
                recordIds.AddRange(changeTermOption.SourceOneDriveRecordIds);
                recordIds.AddRange(changeTermOption.GoogleDriveRecordIds);
                var allRecords = ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                logger.Info($"Get records from db, records: [{string.Join(",", allRecords.Select(r => r.Id))}]");
                List<Guid> predictTermIds = allRecords.Select(r => r.PredictTermId).Distinct().ToList();
                HandleCalculateZeroShotAccuracy(predictTermIds, changeTermType);
            }
            return returnMessage;
        }
        public void HandleCalculateZeroShotAccuracy(List<Guid> predictTermIds, ChangeTermType type)
        {
            List<SourceFlag> sourceFlagQueries = GetSourceFlagByLicense();
            foreach (var termId in predictTermIds)
            {
                try
                {
                    long approvalCount = CountTermsByApprovalStatus(termId, ChangeTermType.AIMADirectlyApprove);
                    long reclassifyCount = CountTermsByApprovalStatus(termId, ChangeTermType.AIMAChangeTerm);
                    RMMLTermDao.UpdateZeroApprovalReclassifyCount(termId, approvalCount, reclassifyCount);
                }
                catch (Exception e)
                {
                    logger.Error($"Calculate zero approval count for current term {termId} has errors: {e}");
                }
            }
        }

        private int CountTermsByApprovalStatus(Guid termId, ChangeTermType type)
        {
            List<SourceFlag> sourceFlagQueries = GetSourceFlagByLicense();
            var query = new ExplorerQueryV3Dto()
            {
                QueryOption = new ExplorerQueryOptionV3()
                {
                    Values = new List<ExplorerSearchOptionV3>()
                        {
                            new ExplorerSearchOptionV3()
                            {
                                 Column = new ExplorerQueryColumn { Id = QueryCloumnIds.SourceFlag },
                                 Value = JsonConvert.SerializeObject(sourceFlagQueries)
                            },
                            new ExplorerSearchOptionV3()
                            {
                                 Column = new ExplorerQueryColumn { Id = QueryCloumnIds.PredictTermId },
                                 Value = JsonConvert.SerializeObject(new List<Guid> {termId})
                            },
                            new ExplorerSearchOptionV3()
                            {
                                Column = new ExplorerQueryColumn { Id = QueryCloumnIds.MLApprovalStatus},
                                Value = JsonConvert.SerializeObject(new List<int> {(type == ChangeTermType.AIMAChangeTerm ? (int)RMMLApprovalStatus.Rejected : (int)RMMLApprovalStatus.Approved)})
                            },
                        }
                },
                PagingInfo = new ExplorerPagingInfo
                {
                    PageIndex = string.Empty,
                    PageSize = 1000,
                }
            };
            var currentApprovalTermCount = ExplorerDao.QueryCountV3(query, null);
            return currentApprovalTermCount;
        }

        private List<SourceFlag> GetSourceFlagByLicense()
        {
            var isSPOLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
            var isGoogleLicense = TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PaidForModule.Google);
            var isGControlLicense = TenantService.HasInitGControlPlatForm().Result;
            var isILLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusIL);
            List<SourceFlag> sources = new List<SourceFlag>();
            if (isGoogleLicense || isGControlLicense)
            {
                sources.Add(SourceFlag.Google);
            }
            if (isILLicense)
            {
                sources.AddRange(new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Teams });
            }
            return sources;
        }
        public async Task<int> ChangeTermForAIJobAsync(List<Guid> recordsId, SourceFlag flag, string jobId, ChangeTermType changeTermType, ChangeTermOption changeTermOption, bool isJob)
        {
            int failedCount = 0;
            if (changeTermOption == null)
            {
                return recordsId.Count;
            }

            switch (flag)
            {
                case SourceFlag.SharePoint:
                    failedCount = await ChangeTermJobSPForAIAsync(changeTermType,changeTermOption, jobId, false, isJob);
                    break;
                case SourceFlag.OneDrive:
                    failedCount = await ChangeTermJobOneDriveForAIAsync(changeTermType,changeTermOption, jobId, isJob);
                    break;
                case SourceFlag.Teams:
                    failedCount = await ChangeTermJobTeamsForAIAsync(changeTermType,changeTermOption, jobId, false, isJob);
                    break;
                case SourceFlag.Google:
                    failedCount = await ChangeTermJobGoogleForAIAsync(changeTermType, changeTermOption, jobId);
                    break;
            }

            return failedCount;
        }

        #region global search
        public async Task<int> ChangeTermRealTimeSPForGlobalSearchAsync(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(isGlobalSearchJob, true);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceRecordIds.Count;
            }
            return failedCount;
        }

        public int ChangeTermRealTimeEXOForGlobalSearch(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                ReclassifyUtility reclassifyUtility = new ReclassifyUtility(isGlobalSearchJob);
                reclassifyUtility.ChangeAllTerms(changeTermOption, jobId, waiting4OtherSource);
                failedCount = reclassifyUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceEXORecordIds.Count;
            }
            return failedCount;
        }

        public int ChangeTermRealTimePhyForGlobalSearch(ChangeTermOption changeTermOption, string jobId, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                PhysicalReclassifyUtility reclassifyUtility = new PhysicalReclassifyUtility(isGlobalSearchJob);
                reclassifyUtility.ChangeAllTermsForPhy(changeTermOption, jobId);
                failedCount = reclassifyUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourcePhyRecordIds.Count;
            }
            return failedCount;
        }

        public async Task<int> ChangeTermRealTimeOneDriveForGlobalSearchAsync(ChangeTermOption changeTermOption, string jobId, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                RMOneDriveExplorerUtility reclassifyUtility = new RMOneDriveExplorerUtility(isGlobalSearchJob);
                await reclassifyUtility.ChangeAllTermsForOneDriveAsync(changeTermOption, jobId);
                failedCount = reclassifyUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceOneDriveRecordIds.Count;
            }
            return failedCount;
        }

        public int ChangeTermRealTimeFSForGlobalSearch(ChangeTermOption changeTermOption, string jobId, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                var reclassifyUtil = new FSReclassifyUtil(isGlobalSearchJob);
                reclassifyUtil.ChangeAllTerms(changeTermOption, jobId, false);
                failedCount = reclassifyUtil.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceFSRecordIds.Count;
            }
            return failedCount;
        }

        public async Task<int> ChangeTermRealTimeTeamsForGlobalSearchAsync(ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                RMTeamsExplorerUtility explorerUtility = new RMTeamsExplorerUtility(isGlobalSearchJob, true);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceRecordIds.Count;
            }
            return failedCount;
        }
        #endregion

        #region AI term

        public async Task<RecordsReturnMessage> ChangeTermRealTimeSPForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(true, changeTermType);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealtimeTeamsForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMTeamsExplorerUtility explorerUtility = new RMTeamsExplorerUtility(true, changeTermType);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
            }
            catch (Exception e)
            {
                logger.Error("An error occured while changing term realtime for Teams {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealTimeOneDriveForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                var reclassifyUtil = new RMOneDriveExplorerUtility(false, changeTermType);
                await reclassifyUtil.ChangeAllTermsForOneDriveAsync(changeTermOption, jobId);
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<RecordsReturnMessage> ChangeTermRealTimeGoogleForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                var reclassifyUtil = new RMGoogleDriveExplorerUtility(jobId, changeTermType);
                await reclassifyUtil.ChangeAllTermsForGoogleDriveAsync(changeTermOption, jobId);
                var failedCount = reclassifyUtil.FailedCount;
                if (failedCount > 0)
                {
                    returnMessage.ResultType = ResultType.Failed;
                }
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<int> ChangeTermJobSPForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(isGlobalSearchJob, true, changeTermType);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceRecordIds.Count;
            }
            return failedCount;
        }

        public async Task<int> ChangeTermJobOneDriveForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId, bool isGlobalSearchJob = false)
        {
            int failedCount = 0;
            try
            {
                RMOneDriveExplorerUtility reclassifyUtility = new RMOneDriveExplorerUtility(isGlobalSearchJob, changeTermType);
                await reclassifyUtility.ChangeAllTermsForOneDriveAsync(changeTermOption, jobId);
                failedCount = reclassifyUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceOneDriveRecordIds.Count;
            }
            return failedCount;
        }

        public async Task<int> ChangeTermJobTeamsForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId, bool waiting4OtherSource, bool isGlobalSearchJob = false)
        {

            int failedCount = 0;
            try
            {
                RMTeamsExplorerUtility explorerUtility = new RMTeamsExplorerUtility(isGlobalSearchJob, true, changeTermType);
                await explorerUtility.ChangeAllTermsAsync(changeTermOption, jobId, waiting4OtherSource);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceTeamsRecordIds.Count;
            }
            return failedCount;
        }

        public async Task<int> ChangeTermJobGoogleForAIAsync(ChangeTermType changeTermType, ChangeTermOption changeTermOption, string jobId)
        {

            int failedCount = 0;
            try
            {
                RMGoogleDriveExplorerUtility explorerUtility = new RMGoogleDriveExplorerUtility(jobId, changeTermType);
                await explorerUtility.ChangeAllTermsForGoogleDriveAsync(changeTermOption, jobId);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Update terms error {0}", e.ToString());
                failedCount = changeTermOption.SourceRecordIds.Count;
            }
            return failedCount;
        }
        #endregion

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.DeclareAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> DeclareAsRecordRealTimeAsync(List<Guid> ids, string jobId, string declareBy)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility();
                await explorerUtility.DeclaredRecordsAsync(ids, jobId, true, declareBy);
            }
            catch (Exception e)
            {
                logger.Warn("Declared Recordsd Error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.UndeclareAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> UndeclareAsRecordRealTimeAsync(List<Guid> ids, string jobId, string declareBy)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility();
                await explorerUtility.DeclaredRecordsAsync(ids, jobId, false, declareBy);
            }
            catch (Exception e)
            {
                logger.Warn("Undeclared Recordsd Error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            return returnMessage;
        }

        public async Task<int> DeclareAsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob)
        {
            int failedCount = 0;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(isGlobalSearchJob, false);
                await explorerUtility.DeclaredRecordsAsync(ids, jobId, true, declareBy);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Declared Recordsd Error {0}", e.ToString());
                failedCount = ids.Count;
            }
            return failedCount;
        }

        public async Task<int> UndeclareAsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob)
        {
            int failedCount = 0;
            try
            {
                RMExplorerUtility explorerUtility = new RMExplorerUtility(isGlobalSearchJob, false);
                await explorerUtility.DeclaredRecordsAsync(ids, jobId, false, declareBy);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Undeclared Recordsd Error {0}", e.ToString());
                failedCount = ids.Count;
            }
            return failedCount;
        }

        public async Task<int> DeclareTeamsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob)
        {
            int failedCount = 0;
            try
            {
                RMTeamsGlobalSearchProcessor explorerUtility = new RMTeamsGlobalSearchProcessor();
                await explorerUtility.HandleDeclareRecords(ids, jobId, true, declareBy);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Declared Recordsd Error {0}", e.ToString());
                failedCount = ids.Count;
            }
            return failedCount;
        }

        public async Task<int> UndeclareTeamsRecordForGlobalSearchAsync(List<Guid> ids, string jobId, string declareBy, bool isGlobalSearchJob)
        {
            int failedCount = 0;
            try
            {
                RMTeamsGlobalSearchProcessor explorerUtility = new RMTeamsGlobalSearchProcessor();
                await explorerUtility.HandleDeclareRecords(ids, jobId, false, declareBy);
                failedCount = explorerUtility.FailedCount;
            }
            catch (Exception e)
            {
                logger.Warn("Declared Recordsd Error {0}", e.ToString());
                failedCount = ids.Count;
            }
            return failedCount;
        }


        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.PhysicalExplorerMove, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> PhysicalExplorerMoveRealTimeAsync(PhysicalMoveOption moveOption, string jobId, Guid groupRequestId = default)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            RMPhysicalExplorerMoveUtility phyMoveUtility = new RMPhysicalExplorerMoveUtility();
            try
            {

                await phyMoveUtility.MoveAsync(moveOption, jobId, isRealTimeMove: true, groupRequestId);
            }
            catch (Exception e)
            {
                logger.Warn("Physical move error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            if (returnMessage.ResultType == ResultType.Failed && phyMoveUtility.failedRecordIds?.Count > 0)
            {
                returnMessage.FailedIds = phyMoveUtility.failedRecordIds;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.Mobile, Category = AuditCategory.Mobile, Action = AuditAction.MobileMove, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RecordsReturnMessage> PhysicalMoveForMobileAsync(PhysicalMoveOption moveOption, string jobId)
        {
            RecordsReturnMessage returnMessage = new RecordsReturnMessage();
            returnMessage.ResultType = ResultType.Success;
            RMPhysicalExplorerMoveUtility phyMoveUtility = new RMPhysicalExplorerMoveUtility();
            try
            {
                if (PhysicalObjectUnderContainer(moveOption.SourcePhyRecordIds.FirstOrDefault()))
                {
                    logger.Info("Physical object is under container, will not move. Id:{0}", moveOption.SourcePhyRecordIds.FirstOrDefault());
                    returnMessage.ResultType = ResultType.Failed;
                }
                else
                {
                    await phyMoveUtility.MoveAsync(moveOption, jobId);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Physical move error {0}", e.ToString());
                returnMessage.ResultType = ResultType.Failed;
            }
            if (returnMessage.ResultType == ResultType.Failed && phyMoveUtility.failedRecordIds?.Count > 0)
            {
                returnMessage.FailedIds = phyMoveUtility.failedRecordIds;
            }
            return returnMessage;
        }

        public bool PhysicalObjectUnderContainer(Guid id)
        {
            bool underContainer = false;
            try
            {
                _explorerDao = new ExplorerDao(true);
                var record = _explorerDao.GetPhysicalRecordById(id);
                underContainer = record.IsUnderContainer();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while checking physical object under container. Id:{0} Error:{1}", id, e.ToString());
            }
            return underContainer;
        }

        #region Physical Records
        [Audit(Action = AuditAction.AddOrUpdatePhysicalObject, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdatePhysicalObjectAsync(PhysicalObjectDto dto)
        {
            var isNew = string.IsNullOrEmpty(dto.UniqueId);
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (dto.Ancestors?.Count > 0)
                {
                    dto.ParentId = dto.Ancestors.Last();
                }
                var validateResult = ValidationPhysicalDto(dto);
                if (validateResult.MessageType == RAMessageType.Failed)
                {
                    logger.Error($"Validation failed, the dto is changed.");
                    return validateResult;
                }
                _explorerDao = new ExplorerDao(true);
                if (dto != null)
                {
                    #region Generate Unique Id
                    if (string.IsNullOrEmpty(dto.UniqueId))
                    {
                        //Generate Unique Id
                        if (dto.Template != null)
                        {
                            try
                            {
                                dto.UniqueId = await GeneratePhysicalObjectUniqueIdAsync(dto.Template.type, dto.TemplateId.ToString(), dto.Template.prefix, dto.Template.numberOfDigits);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message.Equals("Over the digit number"))
                                {
                                    msg.MessageType = RAMessageType.Failed;
                                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_PRM_Explorer_RecordIdOverLength");
                                    return msg;
                                }
                                logger.Warn($"Cannot generate unique id for physical record {dto.Name},  reason : {ex.ToString()}.");
                            }
                        }
                        else
                        {
                            logger.Warn($"Cannot generate unique id for physical record due to null template, template id: [{dto.TemplateId}]");
                        }
                    }
                    #endregion
                    if (dto.NodeType == RMNodeType.PhyRecord)
                    {
                        var parentNode = ExplorerDao.GetPhysicalRecordById(dto.FileId);
                        if (parentNode != null)
                        {
                            string statusObj;
                            var parentMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(parentNode.MetaInfo);
                            parentMetaInfo.TryGetValue(DefaultColumnIDs.Status, out statusObj);
                            dto.MetaInfo[DefaultColumnIDs.Status] = statusObj;
                        }
                    }
                    //Convert General Info
                    dto = await CheckBarcodeExist(dto, false);
                    var record = ConvertUtil.ConvertPhysicalToRMBaseRecord(dto);
                    ArgumentCheck.NotNull(record, nameof(record));
                    await RecordsHistoryService.AddPhysicalRecordActionAuditAsync(isNew ? PhysicalActionType.Create : PhysicalActionType.Edit, record.Id, dto, isNew);
                    //Calculate Rule Property
                    UpdateDestroyedTime(record);
                    var addSucceed = true;
                    var updateSucceed = true;
                    CalculateRuleProperty(dto, record);
                    AddPushColumnToDB(dto, record);
                    logger.Info($"Add record : {record?.Id} to db.");
                    if (isNew)
                    {
                        dto.Id = record.Id;
                        record.ScopePermissionId = dto.ScopePermissionId;
                        addSucceed = ExplorerDao.AddPhysicalRecord(record);
                    }
                    else
                    {
                        updateSucceed = ExplorerDao.UpdatePhysicalRecord(record, false);
                    }
                    if (!addSucceed || !updateSucceed)
                    {
                        logger.Error("Error occured when add or update physical record to cosmos db");
                        msg.MessageType = RAMessageType.Exception;
                    }
                    msg.Extsion1 = record;
                    logger.Info($"Finish adding record : {record?.Id} to db.");
                    if (updateSucceed && dto.NodeType == RMNodeType.PhyFile && dto.BoxId != Guid.Empty)
                    {
                        await ReEvaluateParentBoxesAsync([dto.BoxId]);
                    }
                }
            }
            catch (BarcodeDuplicateException e)
            {
                logger.Error($"Error in AddOrUpdatePhysicalObject : [{e.ToString()}]");
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_Phy_Import_BarcodeDuplicateError");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in AddOrUpdatePhysicalObject : [{ex.ToString()}]");
                msg.MessageType = RAMessageType.Failed;
            }
            return msg;
        }

        public async Task<string> GeneratePhysicalObjectUniqueIdAsync(TemplateType type, string templateId, string prefix, int digit)
        {
            var physicalUniqueIdSetting = PhysicalUniqueIdSettingDao.LoadingUniqueIdSetting();
            var isGlobalUniqueId = physicalUniqueIdSetting == null ? false : physicalUniqueIdSetting.IsGlobalSetting;
            if (isGlobalUniqueId)
            {
                var defaultTemplateIds = new Guid[] { new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID), new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID), new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID) };
                var defaultTemplates = await TemplateDao.FindListAsync(t => Enumerable.Contains(defaultTemplateIds, t.UniqueId));
                var customTemplates = TemplateDao.GetTemplateByType(TemplateType.Custom).Select(t => t.Id.ToString()).ToList();
                switch (type)
                {
                    case TemplateType.Box:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.BOX_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.BoxTemplatePrefix;
                        digit = physicalUniqueIdSetting.BoxTemplateNumberOfDigits;
                        break;
                    case TemplateType.Folder:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.FOLDER_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.FolderTemplatePrefix;
                        digit = physicalUniqueIdSetting.FolderTemplateNumberOfDigits;
                        break;
                    case TemplateType.Records:
                        templateId = defaultTemplates.First(t => t.UniqueId == new Guid(DefaultTemplateIds.RECORD_TEMPLATE_ID)).Id.ToString();
                        prefix = physicalUniqueIdSetting.RecordTemplatePrefix;
                        digit = physicalUniqueIdSetting.RecordTemplateNumberOfDigits;
                        break;
                    case TemplateType.Custom:
                        prefix = physicalUniqueIdSetting.CustomTemplatePrefix;
                        digit = physicalUniqueIdSetting.CustomTemplateNumberOfDigits;
                        break;
                    default:
                        break;
                }
                if(type == TemplateType.Custom)
                {
                    var customUId = await UniqueIdGenerator.GenerateCustomUniqueIdAsync(customTemplates, templateId, prefix, digit);
                    return customUId;
                }
            }
            var uid = await UniqueIdGenerator.GenerateUniqueIdAsync(templateId, prefix, digit);
            return uid;
        }

        [Audit(Action = AuditAction.AddOrUpdatePhysicalObject, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public async Task<RAReturnMessage> EditPhysicalObjectAsync(PhysicalObjectDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var validateResult = ValidationPhysicalDto(dto);
                if (validateResult.MessageType == RAMessageType.Failed)
                {
                    logger.Error($"Validation failed in Edit physical object, the dto is changed.");
                    return validateResult;
                }
                _explorerDao = new ExplorerDao(true);
                if (dto != null)
                {
                    if (dto.NodeType == RMNodeType.PhyRecord)
                    {
                        var parentNode = ExplorerDao.GetPhysicalRecordById(dto.FileId != Guid.Empty ? dto.FileId : dto.ParentId);
                        if (parentNode != null)
                        {
                            string statusObj;
                            var parentMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(parentNode.MetaInfo);
                            parentMetaInfo.TryGetValue(DefaultColumnIDs.Status, out statusObj);
                            dto.MetaInfo[DefaultColumnIDs.Status] = statusObj;
                        }
                    }
                    string loanedByString = "";
                    List<AOSUserDto> loanByUsers = new List<AOSUserDto>();
                    if (dto.MetaInfo.TryGetValue(DefaultColumnIDs.LoanedBy, out loanedByString))
                    {
                        if (!string.IsNullOrEmpty(loanedByString))
                        {
                            loanByUsers = JsonConvert.DeserializeObject<List<AOSUserDto>>(loanedByString);
                            //UserService.SyncUsers(TenantLocalValue.LogonGroupId, loanByUsers);

                            foreach (var loanByUser in loanByUsers)
                            {
                                loanByUser.UserPrincipalName ??= "";
                            }
                            dto.MetaInfo[DefaultColumnIDs.LoanedBy] = JsonConvert.SerializeObject(loanByUsers);
                        }
                    }

                    //Convert General Info
                    dto = await CheckBarcodeExist(dto, true);
                    var record = ConvertUtil.ConvertPhysicalToRMBaseRecord(dto);
                    await RecordsHistoryService.AddPhysicalRecordActionAuditAsync(PhysicalActionType.Edit, record.Id, dto, false);
                    //Calculate Rule Property
                    CalculateRuleProperty(dto, record);
                    AddPushColumnToDB(dto, record);
                    UpdateDestroyedTime(record);
                    var changedLoanByRecord = ExplorerDao.GetPhysicalRawDataById(record.Id);
                    var currentLoanBy = changedLoanByRecord.GetPersonalHoldData()?.GetPeopleOrGroupColumnValue()?.FirstOrDefault();
                    if (dto.PersonHold && !string.IsNullOrEmpty(dto.PersonHoldBy))
                    {
                        List<RMRecordLoanAlliance> loanInfos = RecordLoanAllianceDao.GetPhyRecordAllianceById(record.Id);
                        if (loanInfos != null && loanInfos.Count > 0)
                        {
                            RecordLoanAllianceDao.UpdateLoanedBy(record.Id, dto.PersonHoldBy);
                        }
                        else
                        {
                            RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = record.Id, HoldBy = dto.PersonHoldBy, HoldReleaseTime = DateTime.MaxValue.Ticks, ParentId = record.BoxId });
                        }

                        if (dto.NodeType == RMNodeType.PhyBox)
                        {
                            if (!loanByUsers.IsNullOrEmpty())
                            {
                                await UpdatePhyFilesHoldStateByBoxIdAsync(new Tuple<Guid, AOSUserDto, long>(record.Id, loanByUsers.FirstOrDefault(), DateTime.MaxValue.Ticks));
                            }
                        }

                        if (currentLoanBy != null)
                        {
                            var isChangedLoanBy = false;
                            if (string.IsNullOrEmpty(currentLoanBy.UserPrincipalName) && string.IsNullOrEmpty(loanByUsers.FirstOrDefault().UserPrincipalName))
                            {
                                //AOS Unregistered Users
                                if (currentLoanBy.DisplayName != loanByUsers.FirstOrDefault().DisplayName)
                                {
                                    isChangedLoanBy = true;
                                }
                            }

                            if (currentLoanBy.UserPrincipalName != loanByUsers.FirstOrDefault().UserPrincipalName)
                            {
                                isChangedLoanBy = true;
                            }

                            if (isChangedLoanBy)
                            {
                                changedLoanByRecord.LoanPickStatus = (int)PickStatusType.Pendding;
                                ExplorerDao.Upsert(changedLoanByRecord);
                            }
                        }
                    }
                    else
                    {
                        //ui上没有设置current held by ,会执行return逻辑
                        if (dto.NodeType == RMNodeType.PhyFile)
                        {
                            await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(new List<Guid> { record.Id });
                            if (currentLoanBy != null)
                            {
                                changedLoanByRecord.LoanPickStatus = (int)PickStatusType.Pendding;
                                ExplorerDao.Upsert(changedLoanByRecord);
                            }
                        }
                        if (dto.NodeType == RMNodeType.PhyBox)
                        {
                            await ReutrnPhyBoxAndFileByBoxIdAsync(record.Id, false);
                        }
                    }
                    var holdRecord = ExplorerDao.GetHoldRecordsByIds(new List<Guid> { record.Id }).FirstOrDefault();
                    if (holdRecord != null)
                    {
                        record.HoldType = holdRecord.HoldType;
                        record.HoldBy = holdRecord.HoldBy;
                        record.HoldReleaseTime = holdRecord.HoldReleaseTime;
                        record.HoldId = holdRecord.HoldId;
                        record.HoldStatus = holdRecord.HoldStatus;
                        record.HoldByUsers = holdRecord.HoldByUsers;
                        record.HoldUntilTimes = holdRecord.HoldUntilTimes;
                        record.AppendHolds_Array = holdRecord.AppendHolds_Array;
                    }

                    var updateRecordSucessfull = ExplorerDao.UpdatePhysicalRecord(record, false);
                    if (!updateRecordSucessfull)
                    {
                        logger.Error($"Error occured when update physical record : [{record.Id}] to cosmos db");
                        msg.MessageType = RAMessageType.Exception;
                    }
                    else if ((dto.NodeType == RMNodeType.PhyCustom || dto.NodeType == RMNodeType.PhyFile || dto.NodeType == RMNodeType.PhyBox)
                        && (record.RecordStatus == (int)RMRecordStatus.Missing || record.RecordStatus == (int)RMRecordStatus.Destroyed))
                    {
                        //Get All sub Records
                        IEnumerable<Record> children = null;
                        if (dto.NodeType == RMNodeType.PhyFile)
                        {
                            children = ExplorerDao.QueryAll(r => (r.ParentId == dto.Id || r.FileId == dto.Id) && (r.RecordStatus == (int)RMRecordStatus.Active || r.RecordStatus == (int)RMRecordStatus.Closed || r.RecordStatus == (int)RMRecordStatus.Missing || r.RecordStatus == (int)RMRecordStatus.Destroyed) && r.SourceFlag == (int)SourceFlag.Physical && r.NodeType == (int)RMNodeLevel.PhysicalRecord);
                        }
                        else
                        {
                            children = ExplorerDao.QueryAll(r => (r.BoxId == dto.Id || r.Ancestors.Contains(dto.Id)) && (r.RecordStatus == (int)RMRecordStatus.Active || r.RecordStatus == (int)RMRecordStatus.Closed || r.RecordStatus == (int)RMRecordStatus.Missing || r.RecordStatus == (int)RMRecordStatus.Destroyed) && r.SourceFlag == (int)SourceFlag.Physical);
                        }

                        if (children != null && children.Count() > 0)
                        {
                            string statusObj;
                            if (dto.MetaInfo.TryGetValue(DefaultColumnIDs.Status, out statusObj))
                            {
                                foreach (var subRecord in children)
                                {
                                    if (subRecord.RecordStatus != record.RecordStatus)
                                    {
                                        subRecord.RecordStatus = record.RecordStatus;
                                        var tempMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(subRecord.MetaInfo);
                                        tempMetaInfo[DefaultColumnIDs.Status] = statusObj;
                                        subRecord.MetaInfo = JsonConvert.SerializeObject(tempMetaInfo);
                                        //RECO-5249 在更新成Destroyed 的时候，需要查看当前文件的destroyed time 是不是 0 ，如果不是0 ，表示之前不是destroyed 的数据，需要将parent box/Folder record 的destroyed time 更新到当前record上
                                        if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                                        {
                                            subRecord.DestroyedTime = subRecord.DestroyedTime == 0 ? record.DestroyedTime : subRecord.DestroyedTime;
                                        }
                                        subRecord.RuleId = Guid.Empty;
                                        subRecord.RuleLevel = 0;
                                        subRecord.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("");
                                        subRecord.PreviosDisposalDueDate = subRecord.DisposalDueDate;
                                        if (!ExplorerDao.UpdatePhysicalRecord(subRecord, false))
                                        {
                                            logger.Error($"Error occured when update sub physical record : [{subRecord.Id}] to cosmos db");
                                            throw new Exception();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logger.Error("Cannot get status value from dto info.");
                                throw new Exception();
                            }
                        }
                    }
                    if (updateRecordSucessfull && dto.NodeType == RMNodeType.PhyFile && dto.BoxId != Guid.Empty)
                    {
                        await ReEvaluateParentBoxesAsync([dto.BoxId]);
                    }
                }
                else
                {
                    logger.Error($"The physical record dto is null.");
                }
            }
            catch (BarcodeDuplicateException e)
            {
                logger.Error($"Error in AddOrUpdatePhysicalObject : [{e.ToString()}]");
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_Phy_Import_BarcodeDuplicateError");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in EditPhysicalObject : reason : [{ex.ToString()}]");
                msg.MessageType = RAMessageType.Failed;
            }
            return msg;
        }

        private async Task ReEvaluateParentBoxesAsync(List<Guid> parentBoxIds)
        {
            if (parentBoxIds == null || !parentBoxIds.Any()) return;

            var uniqueParentIds = parentBoxIds.Distinct().ToList();

            try
            {
                var parentNodes = ExplorerDao.GetRecordBoxsByBoxIds(uniqueParentIds);
                if (parentNodes == null) return;

                var parentBoxes = parentNodes.Where(p => p != null && p.TermId != Guid.Empty).ToList();
                if (!parentBoxes.Any()) return;

                var updateTasks = parentBoxes.Select(async parentBox =>
                {
                    try
                    {
                        var rules = GetRulesByTermId(parentBox.TermId);
                        if (!rules.HasLatestFolderDisposalDueDateRule())
                        {
                            return;
                        }
                        logger.Info($"Reevaluating and updating parent box: {parentBox.Id} due to folder changes.");

                        var dto = ConvertUtil.ConvertRMBaseRecordToPhysical(parentBox);
                        dto.Template = await TemplateManagementService.LoadTemplateDtoAsync(parentBox.TemplateId);

                        await EditPhysicalObjectAsync(dto);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error re-evaluating parent box {parentBox.Id}: {ex.Message}", ex);
                    }
                });

                await Task.WhenAll(updateTasks);
            }
            catch (Exception ex)
            {
                logger.Error($"Critical error in ReEvaluateParentBoxesAsync: {ex.Message}", ex);
            }
        }
        private List<Rule> GetRulesByTermId(Guid termId)
        {
            if (_rulesCache.TryGetValue(termId, out var cachedRules))
            {
                return cachedRules;
            }

            try
            {
                logger.Info($"Cache missed for TermId {termId}. Fetching rules from DB.");

                var rulesFromDb = GetRuleByTermId(termId);

                var finalRules = rulesFromDb ?? new List<Rule>();

                _rulesCache.TryAdd(termId, finalRules);

                return finalRules;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to fetch rules for TermId {termId}: {ex.Message}", ex);
                return new List<Rule>();
            }
        }

        //[Audit(Action = AuditAction.AddOrUpdatePhysicalObject, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public async Task<RAReturnMessage> BulkEditPhysicalObjectAsync(List<Guid> recordIds, Dictionary<string, string> bulkMetaInfoDic, int templateId)
        {
            RAReturnMessage resultMsg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var template = await TemplateManagementService.LoadTemplateDtoAsync(templateId);
                ValidationMetaInfo(bulkMetaInfoDic, template);

                List<Guid> failedDataGuid = new List<Guid>();
                var records = ExplorerDao.GetRecordByIds(recordIds);
                foreach (var record in records)
                {
                    var metaInfo = string.IsNullOrEmpty(record.MetaInfo) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
                    var oldObject = ConvertUtil.ConvertRMBaseRecordToPhysical(record);
                    foreach (var bulkColumnId in bulkMetaInfoDic.Keys)
                    {
                        if (metaInfo != null)
                        {
                            var bulkColumn = bulkMetaInfoDic[bulkColumnId];
                            metaInfo[bulkColumnId] = bulkColumn;

                            record.ModifiedBy = TenantLocalValue.DisplayName;
                            record.TimeModified = DateTime.UtcNow.Ticks;
                        }
                    }
                    record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
                    var newObject = ConvertUtil.ConvertRMBaseRecordToPhysical(record);
                    await RecordsHistoryService.AddPhysicalRecordActionAuditAsync(PhysicalActionType.Edit, record.Id, newObject, false, oldObject);
                }
                var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                if (bulkSize == default)
                {
                    bulkSize = CosmosBulkOperator.DefualtBufferSize;
                }
                logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                failedDataGuid = ExplorerDao.BatchUpdate(records, bulkSize);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in EditPhysicalObject : reason : [{ex.ToString()}]");
                resultMsg.MessageType = RAMessageType.Failed;
            }
            return resultMsg;
        }

        /// <summary>
        /// Personal Hold 专用的
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="holdBy"></param>
        /// <param name="releaseTime"></param>
        /// <returns></returns>
        public RAReturnMessage UpdatePhysicalRecordState2Hold(List<string> uniqueId, AOSUserDto holdByUser, long releaseTime)
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var holdBy = holdByUser?.DisplayName;
                List<Record> tempRecords = ExplorerDao.QueryAll(a => uniqueId.Contains(a.RecordsId), false).ToList();
                var auditList = new List<PhysicalRecordActionAudit>();
                foreach (Record re in tempRecords)
                {
                    logger.Debug("update record {0}, personal hold by {1}", re.RecordsId, holdBy);
                    var loanApprovalSuccess = RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = re.Id, HoldBy = holdBy, HoldReleaseTime = releaseTime, ParentId = re.BoxId });
                    if (!loanApprovalSuccess)
                    {
                        logger.Info("{0} has loaned", re?.Id);
                        msg.MessageType = RAMessageType.Failed;
                        msg.ErrorMessage = I18NEntity.GetString("RM_RDM_Hold_PhysicalRecordHasLoaned");
                    }
                    else
                    {
                        var physicalAudit = RecordsHistoryService.BuildPhysicalLoanAudit(re.Id, re.CustomColumnDic, holdBy);
                        auditList.Add(physicalAudit);
                        re.UpdatePersonalHoldData(holdByUser); // update the personal hold by info to cosmos db
                        ExplorerDao.Upsert(re);
                    }
                    //Person Hold不再更新Record记录, 避免与Disposal hold冲突.
                    //ExplorerDao.UpdateAll(a => a.Id == re.Id, r => { r.HoldType = (int)HoldType.PersonalHold; r.HoldBy = holdBy; r.HoldReleaseTime = releaseTime; });
                }
                RecordsHistoryService.AddPhysicalAudit(auditList);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = e.Message;
            }
            return msg;

        }

        /// <summary>
        /// Loan Box 批量处理 Folder用，已经Loan的Folder会直接Skip
        /// </summary>
        /// <param name="uniqueIds"></param>
        /// <param name="holdByUser"></param>
        /// <param name="releaseTime"></param>
        /// <returns></returns>
        public async Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> UpdatePhyFilesHoldStateByBoxIdAsync(Tuple<Guid, AOSUserDto, long> request)
        {
            var resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            try
            {

                bool hasNext = true;
                string pageIndex = string.Empty;
                var pateSize = 5000;

                while (hasNext)
                {
                    (var item, pageIndex, hasNext) = await UpdatePhyFilesHoldStateByBoxIdAsync(request, pateSize, pageIndex);
                    resultList.AddRange(item);
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            return resultList;
        }

        public async Task<(List<Tuple<ItemActionResult, PhysicalObjectDto>>,string,bool)> UpdatePhyFilesHoldStateByBoxIdAsync(Tuple<Guid, AOSUserDto, long> request, int pateSize, string pageIndex)
        {
            bool hasNext = false;
            List<Tuple<ItemActionResult, PhysicalObjectDto>> resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            Guid uniqueId = request.Item1;
            AOSUserDto holdByUser = request.Item2;
            long releaseTime = request.Item3;
            var holdBy = holdByUser?.DisplayName;
            var statusArray = RecordStatusHelper.GetIntDefaultPhysicalStatus();
            Expression<Func<Record, bool>> predicate = a => a.BoxId == uniqueId && a.NodeType == (int)RMNodeType.PhyFile && Enumerable.Contains(statusArray, a.RecordStatus);
            Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(predicate, pateSize, pageIndex, false);
            hasNext = !string.IsNullOrEmpty(result.Item2);
            pageIndex = result.Item2;
            List<Record> physicalRecords = result.Item1.ToList();
            var physicalRecordIds = physicalRecords.Select(i => i.Id).ToList();
            var allLoaneds = (await RecordLoanAllianceDao.FindListAsync(r => physicalRecordIds.Contains(r.RecordsId))).ToList();
            List<RMRecordLoanAlliance> recordLoanAlliances = new List<RMRecordLoanAlliance>();
            foreach (Record re in physicalRecords)
            {
                try
                {
                    if (re.RecordStatus == (int)RMRecordStatus.Destroyed || re.RecordStatus == (int)RMRecordStatus.Missing)
                    {
                        resultList.Add(new Tuple<ItemActionResult, PhysicalObjectDto>(new ItemActionResult() { Status = ActionResultStatus.Skipped }, ConvertUtil.ConvertRMBaseRecordToPhysical(re)));
                        logger.Info($"records status is, {re.RecordStatus}");
                        continue;
                    }

                    var existLoanedAlliance = allLoaneds.FirstOrDefault(a => a.RecordsId == re.Id);
                    if (existLoanedAlliance != null)
                    {
                        if (existLoanedAlliance.HoldBy?.ToLower() == holdBy?.ToLower() && existLoanedAlliance?.HoldReleaseTime == releaseTime)
                        {
                            resultList.Add(new Tuple<ItemActionResult, PhysicalObjectDto>(new ItemActionResult() { Status = ActionResultStatus.Skipped }, ConvertUtil.ConvertRMBaseRecordToPhysical(re)));
                            logger.Info($"records is loaned and does not change hold time, {re?.Id}");
                        }
                        else
                        {
                            RecordLoanAllianceDao.CreateOrUpdateLoanAlliance(new RMRecordLoanAlliance() { RecordsId = re.Id, HoldBy = holdBy, HoldReleaseTime = releaseTime, ParentId = re.BoxId });
                            //RecordLoanAllianceDao.UpdateLoanedBy(re.Id, holdBy);
                            re.LoanPickStatus = (int)PickStatusType.Pendding;
                            //update the personal hold by info to cosmos db
                            re.UpdatePersonalHoldData(holdByUser);
                            ExplorerDao.Upsert(re);
                        }
                    }
                    else
                    {
                        var loanedAlliance = new RMRecordLoanAlliance() { RecordsId = re.Id, HoldBy = holdBy, HoldReleaseTime = releaseTime, ParentId = re.BoxId };
                        recordLoanAlliances.Add(loanedAlliance);

                        //update the personal hold by info to cosmos db
                        re.UpdatePersonalHoldData(holdByUser);
                        ExplorerDao.Upsert(re);
                        resultList.Add(new Tuple<ItemActionResult, PhysicalObjectDto>(new ItemActionResult() { Status = ActionResultStatus.Successful }, ConvertUtil.ConvertRMBaseRecordToPhysical(re)));
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("update record {0} error, message: {1}", re.RecordsId, e.ToString());
                }
            }
            RecordLoanAllianceDao.BatchCreate(recordLoanAlliances);
            return (resultList,pageIndex,hasNext);
        }

        public async Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> ReutrnPhyBoxAndFileByBoxIdAsync(Guid boxId, bool ifAddAudit = true)
        {
            var resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            try
            {
                var phyBox = ExplorerDao.GetPhysicalRawDataById(boxId);
                AOSUserDto boxLoanUser = boxLoanUser = phyBox.GetPersonalHoldData()?.GetPeopleOrGroupColumnValue()?.FirstOrDefault();
                var allLoanedFolderIds = RecordLoanAllianceDao.GetPhyFoldersIdByBoxIds(new List<Guid>() { boxId });
                var pageSize = 100;
                for (int pageIndex = 0; pageIndex <= allLoanedFolderIds.Count / pageSize; pageIndex++)
                {
                    var loanedIds = allLoanedFolderIds.Skip(pageSize * pageIndex).Take(pageSize).ToList();
                    resultList.AddRange(await ReutrnPhyFilesByBoxIdAsync(boxLoanUser, loanedIds, pageSize, pageIndex));
                }

                await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(new List<Guid>() { boxId });
                RemovePersonalHold4Record(new List<Guid>() { boxId }, ifAddAudit);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
            return resultList;
        }



        public async Task<List<Tuple<ItemActionResult, PhysicalObjectDto>>> ReutrnPhyFilesByBoxIdAsync(AOSUserDto boxLoanUser, List<Guid> loanedIds, int pageSize, int pageIndex)
        {
            List<Tuple<ItemActionResult, PhysicalObjectDto>> resultList = new List<Tuple<ItemActionResult, PhysicalObjectDto>>();
            var allLoaneds = (await RecordLoanAllianceDao.FindListAsync(r => loanedIds.Contains(r.RecordsId))).ToList();
            List<RMRecordLoanAlliance> removeLoanAlliances = new List<RMRecordLoanAlliance>();
            var statusArray = RecordStatusHelper.GetIntDefaultPhysicalStatus();
            var records = ExplorerDao.QueryAll(r => loanedIds.Contains(r.Id) && Enumerable.Contains(statusArray, r.RecordStatus), false);
            var returnHistory = new List<RecordReturnLoanDataHistory>();
            foreach (var re in records)
            {
                try
                {
                    var loanAlliance = allLoaneds.FirstOrDefault(a => a.RecordsId == re.Id);
                    var folderLoanUser = re.GetPersonalHoldData()?.GetPeopleOrGroupColumnValue()?.FirstOrDefault();
                    if (folderLoanUser != null && (folderLoanUser.UserId == boxLoanUser.UserId || folderLoanUser.UserPrincipalName == boxLoanUser.UserPrincipalName))
                    {
                        removeLoanAlliances.Add(loanAlliance);
                        re.RemovePersonalHoldData();
                        ExplorerDao.Upsert(re);
                        resultList.Add(new Tuple<ItemActionResult, PhysicalObjectDto>(new ItemActionResult() { Status = ActionResultStatus.Successful }, ConvertUtil.ConvertRMBaseRecordToPhysical(re)));
                        returnHistory.Add(new RecordReturnLoanDataHistory
                        {
                            PartitionKey = re.Id.ToString(),
                            RowKey = Guid.NewGuid().ToString(),
                            ItemName = re.LeafName,
                            UniqueId = re.RecordsId,
                            ReturnTime = DateTime.UtcNow.Ticks,
                            RequestBy = TenantLocalValue.DisplayName,
                            HomeLocation = GetPhysicalObjectFullPath(re.Id)
                        });
                    }
                    else
                    {
                        logger.Info($"records has been loaned by others, {re?.Id}");
                        resultList.Add(new Tuple<ItemActionResult, PhysicalObjectDto>(new ItemActionResult() { Status = ActionResultStatus.Skipped }, ConvertUtil.ConvertRMBaseRecordToPhysical(re)));
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("update record {0} error, message: {1}", re.RecordsId, e.ToString());
                }
            }
            RecordsHistoryService.AddRecordReturnLoanHistory(returnHistory);
            RecordLoanAllianceDao.BatchDelete(removeLoanAlliances);
            return resultList;
        }

        public int GetPhyBoxAndFileCountByBoxIds(List<Guid> uniqueIds)
        {
            if (uniqueIds == null || !uniqueIds.Any()) return 0;
            var statusSet = RecordStatusHelper.GetIntDefaultPhysicalStatus().ToHashSet();
            var idSet = uniqueIds.ToHashSet();

            int phyBoxNode = (int)RMNodeType.PhyBox;
            int phyFileNode = (int)RMNodeType.PhyFile;

            Expression<Func<Record, bool>> predicate = a => (idSet.Contains(a.Id) || idSet.Contains(a.BoxId)) && (a.NodeType == phyBoxNode || a.NodeType == phyFileNode) && statusSet.Contains(a.RecordStatus);

            var count = ExplorerDao.QueryCount(predicate);
            return count;
        }

        public List<PhysicalObjectDto> GetAllLoanedFolders(List<Guid> guids)
        {
            if (guids == null || !guids.Any()) return [];

            var guidSet = guids.ToHashSet();
            var statusSet = RecordStatusHelper.GetIntDefaultPhysicalStatus().ToHashSet();
            int fileNodeType = (int)RMNodeType.PhyFile;

            Expression<Func<Record, bool>> predicate = a => guidSet.Contains(a.Id) && a.NodeType == fileNodeType  && statusSet.Contains(a.RecordStatus);

            var records = ExplorerDao.QueryAll(predicate);
            return records.Select(c => ConvertUtil.ConvertRMBaseRecordToPhysical(c)).ToList();
        }

        [Audit(Action = AuditAction.MobileChangeStatus, Category = AuditCategory.Mobile, Module = AuditModule.Mobile, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public RAReturnMessage UpdatePhysicalRecordStatusForMobile(MobileChangeStatusDto requestDto)
        {
            RAReturnMessage msg = new RAReturnMessage();
            var ids = requestDto.RecordIds.Select(r => r.Id).ToList();
            var recordStatus = requestDto.PhysicalRecordStatus;
            var records = ExplorerDao.GetRecordByIds(ids);
            foreach (var record in records)
            {
                record.RecordStatus = (int)recordStatus;
                var metaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(record.MetaInfo);
                var statusColumn = new ChoiceColumnValue();
                statusColumn.Name = recordStatus.ToString();
                statusColumn.Value = ((int)recordStatus).ToString();
                metaInfo[DefaultColumnIDs.Status] = JsonConvert.SerializeObject(statusColumn);
                string metaInfoStr = JsonConvert.SerializeObject(metaInfo);
                //RECO-5249 在更新成Destroyed 的时候，需要查看当前文件的destroyed time 是不是 0 ，如果不是0 ，表示之前不是destroyed 的数据，需要将parent box/Folder record 的destroyed time 更新到当前record上
                //if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                //{
                //    record.DestroyedTime = record.DestroyedTime == 0 ? record.DestroyedTime : record.DestroyedTime;
                //}

                if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                {
                    record.DestroyedTime = record.DestroyedTime == 0 ? DateTime.UtcNow.Ticks : record.DestroyedTime;
                }

                record.MetaInfo = metaInfoStr;
                ExplorerDao.UpdatePhysicalRecord(record, false);
                if ((record.NodeType == (int)RMNodeType.PhyFile || record.NodeType == (int)RMNodeType.PhyBox)
                        && (record.RecordStatus == (int)RMRecordStatus.Missing || record.RecordStatus == (int)RMRecordStatus.Destroyed))
                {
                    //Get All sub Records
                    IEnumerable<Record> children = null;
                    if (record.NodeType == (int)RMNodeType.PhyFile)
                    {
                        children = ExplorerDao.QueryAll(r => r.FileId == record.Id && (r.RecordStatus == (int)RMRecordStatus.Active || r.RecordStatus == (int)RMRecordStatus.Closed || r.RecordStatus == (int)RMRecordStatus.Missing || r.RecordStatus == (int)RMRecordStatus.Destroyed) && r.SourceFlag == (int)SourceFlag.Physical && r.NodeType == (int)RMNodeLevel.PhysicalRecord);
                    }
                    else if (record.NodeType == (int)RMNodeType.PhyBox)
                    {
                        children = ExplorerDao.QueryAll(r => r.BoxId == record.Id && (r.RecordStatus == (int)RMRecordStatus.Active || r.RecordStatus == (int)RMRecordStatus.Closed || r.RecordStatus == (int)RMRecordStatus.Missing || r.RecordStatus == (int)RMRecordStatus.Destroyed) && r.SourceFlag == (int)SourceFlag.Physical);
                    }

                    if (children != null && children.Count() > 0)
                    {
                        var recordMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfoStr);
                        foreach (var subRecord in children)
                        {
                            if (subRecord.RecordStatus != record.RecordStatus)
                            {
                                subRecord.RecordStatus = record.RecordStatus;
                                var tempMetaInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(subRecord.MetaInfo);
                                tempMetaInfo[DefaultColumnIDs.Status] = recordMetaInfo[DefaultColumnIDs.Status];
                                subRecord.MetaInfo = JsonConvert.SerializeObject(tempMetaInfo);
                                //RECO-5249 在更新成Destroyed 的时候，需要查看当前文件的destroyed time 是不是 0 ，如果不是0 ，表示之前不是destroyed 的数据，需要将parent box/Folder record 的destroyed time 更新到当前record上
                                if (record.RecordStatus == (int)RMRecordStatus.Destroyed)
                                {
                                    subRecord.DestroyedTime = subRecord.DestroyedTime == 0 ? record.DestroyedTime : subRecord.DestroyedTime;
                                }
                                if (!ExplorerDao.UpdatePhysicalRecord(subRecord, false))
                                {
                                    logger.Error($"Error occured when update sub physical record : [{subRecord.Id}] to cosmos db");
                                    throw new Exception();
                                }
                            }
                        }
                    }
                }
            }
            return msg;
        }

        public async Task<PhysicalObjectDto> GetPhysicalObjectByIdAsync(Guid id, bool getBarcode = false)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var data = ExplorerDao.GetFirstOrDefault(r => r.ScopeId == Guid.Empty && r.Id == id);
                result = ConvertUtil.ConvertRMBaseRecordToPhysical(data);
                if (result.BoxId != Guid.Empty)
                {
                    var box = ExplorerDao.GetPhysicalRecordById(result.BoxId);
                    if (box == null)
                    {
                        result.BoxId = Guid.Empty;
                    }
                }
                var barcode = result.BarcodeId ?? result.UniqueId;
                if (getBarcode && (result.NodeType == RMNodeType.PhyBox || result.NodeType == RMNodeType.PhyFile))
                {
                    var barcodeUtil = new BarcodeUtil();
                    var isValid = barcodeUtil.PreCheckBarcodeInfo(barcode);
                    if (isValid)
                    {
                        result.BarcodeBase64Str = new BarcodeUtil().GetBarcodeImgBase64Str(barcode);
                    }
                    else
                    {
                        result.BarcodeBase64Str = string.Empty;
                    }
                }
                this.AppendPhyHoldInfo(result);
                var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
                result.DestroyedTime = data.DestroyedTime > 0 ? GeneralSettingService.ConvertTiksToDateTime(generalSetting, data.DestroyedTime, true).SimplifyFormatTime : "";
                if (result.NodeType == RMNodeType.PhyRecord)
                {
                    List<Guid> parentIds = new List<Guid>() { result.FileId, result.BoxId };
                    List<Record> parentRecs = ExplorerDao.QueryAll(a => parentIds.Contains(a.Id) && a.ScopeId == Guid.Empty).OrderBy(a => a.NodeType).ToList();
                    Record file = parentRecs.FirstOrDefault(a => a.NodeType == (int)RMNodeType.PhyFile);
                    if (file != null && file.RuleId != Guid.Empty)
                    {
                        result.RuleId = file.RuleId;
                        if (file.DisposalDueDate > DateTime.MinValue.Ticks)
                        { 
                            result.DisposalDueDate = this.GetDisposalDueDateStr(file.DisposalDueDate, (RMRecordStatus)file.RecordStatus, generalSetting, false);
                        }
                        this.AppendPhysicalRuleAction(result, RMRuleDao.GetRuleById(file.RuleId));
                    }
                    else
                    {
                        this.AppendPhysicalRuleAction(result, RMRuleDao.GetRuleById(result.RuleId));
                        //Record box = parentRecs.FirstOrDefault(a => a.NodeType == (int)RMNodeType.PhyBox);
                        //if (box != null && box.RuleId != Guid.Empty)
                        //{
                        //    result.RuleId = box.RuleId;
                        //    result.DisposalDueDate = this.GetDisposalDueDateStr(box.DisposalDueDate, (PhysicalRecordStatus)box.RecordStatus, generalSetting, false);
                        //    this.AppendPhysicalRuleAction(result, RMRuleDao.GetRuleById(box.RuleId));
                        //}
                    }

                }
                else
                {
                    this.AppendPhysicalRuleAction(result, RMRuleDao.GetRuleById(result.RuleId));
                }
                this.CalculateDisposalDueDateNormal(result, generalSetting, 0);
                //if (result.NodeType == RMNodeType.PhyFile && result.BoxId != Guid.Empty && result.DisposalHold == true && result.HoldStatus == HoldStatus.Inherit)
                //{
                //    Record box = ExplorerDao.GetPhysicalRecordById(result.BoxId);
                //    this.CalculateDisposalDueDateNormal(result, generalSetting, box.DisposalDueDate);
                //}

                if (result.NodeType == RMNodeType.PhyFile && result.BoxId != Guid.Empty)
                {
                    Record box = ExplorerDao.GetPhysicalRecordById(result.BoxId);
                    if (result.DisposalHold == true && result.HoldStatus == HoldStatus.Inherit)
                    {
                        this.CalculateDisposalDueDateNormal(result, generalSetting, box.DisposalDueDate);
                    }
                    result.BoxTemplateId = box.TemplateId;
                }
                this.AppendPushedColumns(new List<PhysicalObjectDto>() { result });
                result.RecordHistory = await GetHistoryInfoAsync(data.RecordHistory, id);
                if (result.NodeType == RMNodeType.PhyRecord)
                {
                    this.AppendPushedColumns(new List<PhysicalObjectDto>() { result });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalObject by id: [{id}], error: [{ex.ToString()}]");
            }
            return result;
        }

        public bool IsPhysicaRecordExistForCreateTime(Guid id, DateTime startUtcTime, DateTime endUtcTime)
        {
            bool result = false;
            Record record = ExplorerDao.GetPhysicalRecordById(id);
            if (record != null && record.TimeCreated > startUtcTime.Ticks && record.TimeCreated < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        public bool IsPhysicaRecordExistForDestroyedTime(Guid id, DateTime startUtcTime, DateTime endUtcTime)
        {
            bool result = false;
            Record record = ExplorerDao.GetPhysicalRecordById(id);
            if (record != null && record.DestroyedTime > startUtcTime.Ticks && record.DestroyedTime < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        private void AppendPhyHoldInfo(PhysicalObjectDto result)
        {
            RMRecordLoanAlliance al = RecordLoanAllianceDao.Find(a => a.RecordsId == result.Id);
            GeneralSettingModel gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            if (al != null)
            {
                result.PersonHold = true;
                result.PersonHoldBy = al.HoldBy;
                result.PersonHoldReleaseTime = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? 0 : al.HoldReleaseTime;
                result.PersonHoldReleaseTimeStr = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, al.HoldReleaseTime, true).SimplifyFormatTime;
            }

            if (result.NodeType == RMNodeType.PhyFile)
            {
                RMRecordLoanAlliance boxAl = RecordLoanAllianceDao.Find(a => a.RecordsId == result.BoxId);
                if (boxAl != null)
                {
                    result.BoxPersonHold = true;
                }
            }

            //处理老数据, 如果只是PersonHold的老数据, 清除Hold信息, 避免混淆Disposal Hold.   新版本不会再产生HoldType= 1的数据
            if (result.HoldType == (int)HoldType.PersonalHold)
            {
                result.HoldBy = I18N.Core.I18NEntity.GetString("RM_JS_PRM_PRE_UserIsNull");
                result.HoldReleaseTime = 0;
            }
            List<string> holdIds = new List<string>();
            if (result.NodeType == RMNodeType.PhyBox)
            {
                var box = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { result.Id }).FirstOrDefault();
                if (box != null)
                {
                    holdIds = GetAllExistHoldIds(box);
                    result.DisposalHold = true;
                    result.HoldType = 2;
                    result.HoldStatus = HoldStatus.Self;
                    result.HoldBy = AssembleAccountDisplayName(box.HoldBy, box.HoldByUsers, holdIds);
                    result.HoldReleaseTime = box.HoldReleaseTime;
                    result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                }
            }
            else if (result.NodeType == RMNodeType.PhyFile)
            {
                var parentPhysicalObject = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { result.Id, result.BoxId });
                var file = parentPhysicalObject.FirstOrDefault(a => a.Id == result.Id);
                var box = result.BoxId == Guid.Empty ? null : parentPhysicalObject.FirstOrDefault(a => a.Id == result.BoxId);
                if (file != null)
                {
                    holdIds = GetAllExistHoldIds(file);
                    result.DisposalHold = true;
                    result.HoldType = 2;
                    result.HoldStatus = HoldStatus.Self;
                    result.HoldBy = AssembleAccountDisplayName(file.HoldBy, file.HoldByUsers, holdIds);
                    result.HoldReleaseTime = file.HoldReleaseTime;
                    result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                }
                else if (box != null)
                {
                    holdIds = GetAllExistHoldIds(box);
                    result.DisposalHold = true;
                    result.HoldType = 2;
                    result.HoldStatus = HoldStatus.Inherit;
                    result.HoldBy = AssembleAccountDisplayName(box.HoldBy, box.HoldByUsers, holdIds);
                    result.HoldReleaseTime = box.HoldReleaseTime;
                    result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                }
            }
            else if (result.NodeType == RMNodeType.PhyRecord)
            {
                var parentAls = ExplorerDao.GetHoldRecordsByIds(new List<Guid>() { result.FileId, result.BoxId });
                var file = parentAls.FirstOrDefault(a => a.Id == result.FileId);
                var box = result.BoxId == Guid.Empty ? null : parentAls.FirstOrDefault(a => a.Id == result.BoxId);
                if (file != null)
                {
                    holdIds = GetAllExistHoldIds(file);
                    result.DisposalHold = true;
                    result.HoldType = 2;
                    result.HoldStatus = HoldStatus.Inherit;
                    result.HoldBy = AssembleAccountDisplayName(file.HoldBy, file.HoldByUsers, holdIds);
                    result.HoldReleaseTime = file.HoldReleaseTime;
                    result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                }
                else if (box != null)
                {
                    holdIds = GetAllExistHoldIds(box);
                    result.DisposalHold = true;
                    result.HoldType = 2;
                    result.HoldStatus = HoldStatus.Inherit;
                    result.HoldBy = AssembleAccountDisplayName(box.HoldBy, box.HoldByUsers, holdIds);
                    result.HoldReleaseTime = box.HoldReleaseTime;
                    result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                }

                //inherit folder loan info
                RMRecordLoanAlliance fileLoanInfo = RecordLoanAllianceDao.Find(a => a.RecordsId == result.FileId);
                if (fileLoanInfo != null)
                {
                    result.PersonHold = true;
                    result.PersonHoldBy = fileLoanInfo.HoldBy;
                    result.PersonHoldReleaseTime = fileLoanInfo.HoldReleaseTime == DateTime.MaxValue.Ticks ? 0 : fileLoanInfo.HoldReleaseTime;
                    result.PersonHoldReleaseTimeStr = fileLoanInfo.HoldReleaseTime == DateTime.MaxValue.Ticks ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, fileLoanInfo.HoldReleaseTime, true).SimplifyFormatTime;
                }
            }
            if (holdIds != null && holdIds.Count > 0)
            {
                this.AppendHoldInfo(result, holdIds);
            }
            if (!result.DisposalHold)
            {
                result.HoldType = 0;
                result.HoldBy = null;
            }
        }
        private void AppendPhyPersonalHoldInfo(List<PhysicalObjectDto> results, Dictionary<int, RMAccount> accountMap)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            if (results.IsNullOrEmpty())
            {
                return;
            }
            List<Guid> tempParam = new List<Guid>();
            foreach (PhysicalObjectDto result in results)
            {
                if (result.NodeType == RMNodeType.PhyBox)
                {
                    tempParam.Add(result.Id);
                }
                else if (result.NodeType == RMNodeType.PhyFile)
                {
                    tempParam.Add(result.Id);
                    tempParam.Add(result.BoxId);
                }
                else if (result.NodeType == RMNodeType.PhyRecord)
                {
                    tempParam.Add(result.FileId);
                    tempParam.Add(result.BoxId);
                }
            }
            tempParam = tempParam.Distinct().ToList();

            List<RMRecordLoanAlliance> loanAls = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(tempParam);
            var allHoldRelatedRecords = ExplorerDao.GetHoldRecordsByIds(tempParam);
            var allRelatedHoldIds = new List<string>();
            foreach (var relatedHold in allHoldRelatedRecords)
            {
                allRelatedHoldIds.AddRange(GetAllExistHoldIds(relatedHold));
            }
            List<RMHold> allRelatedHolds = HoldDao.GetHoldByIds(allRelatedHoldIds);
            //List<RMRecordAlliance> als = RecordAllianceDao.GetPhyRecordAllianceByIds(tempParam);
            foreach (PhysicalObjectDto result in results)
            {
                if (result.NodeType == RMNodeType.PhyFile)
                {
                    RMRecordLoanAlliance al = loanAls.FirstOrDefault(a => a.RecordsId == result.Id);
                    if (al != null)
                    {
                        result.PersonHold = true;
                        result.PersonHoldBy = this.AssembleAccountDisplayName(al.HoldBy, accountMap.Values);
                        result.PersonHoldReleaseTime = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? 0 : al.HoldReleaseTime;
                        result.PersonHoldReleaseTimeStr = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, al.HoldReleaseTime, true).SimplifyFormatTime;
                    }
                    //处理老数据, 如果只是PersonHold的老数据, 清除Hold信息, 避免混淆Disposal Hold.  新版本不会再产生HoldType= 1的数据
                    if (result.HoldType == (int)HoldType.PersonalHold)
                    {
                        result.HoldBy = I18N.Core.I18NEntity.GetString("RM_JS_PRM_PRE_UserIsNull");
                        result.HoldReleaseTime = 0;
                    }

                    RMRecordLoanAlliance boxAl = loanAls.FirstOrDefault(a => a.RecordsId == result.BoxId);
                    if (boxAl != null)
                    {
                        result.BoxPersonHold = true;
                    }
                }
                else if (result.NodeType == RMNodeType.PhyBox)
                {
                    RMRecordLoanAlliance al = loanAls.FirstOrDefault(a => a.RecordsId == result.Id);
                    if (al != null)
                    {
                        result.PersonHold = true;
                        result.PersonHoldBy = this.AssembleAccountDisplayName(al.HoldBy, accountMap.Values);
                        result.PersonHoldReleaseTime = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? 0 : al.HoldReleaseTime;
                        result.PersonHoldReleaseTimeStr = al.HoldReleaseTime == DateTime.MaxValue.Ticks ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, al.HoldReleaseTime, true).SimplifyFormatTime;
                    }
                }
                else if (result.NodeType == RMNodeType.PhyRecord)
                {
                    //inherit folder loan info
                    RMRecordLoanAlliance fileLoanInfo = RecordLoanAllianceDao.Find(a => a.RecordsId == result.FileId);
                    if (fileLoanInfo != null)
                    {
                        result.PersonHold = true;
                        result.PersonHoldBy = fileLoanInfo.HoldBy;
                        result.PersonHoldReleaseTime = fileLoanInfo.HoldReleaseTime == DateTime.MaxValue.Ticks ? 0 : fileLoanInfo.HoldReleaseTime;
                        result.PersonHoldReleaseTimeStr = fileLoanInfo.HoldReleaseTime == DateTime.MaxValue.Ticks ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, fileLoanInfo.HoldReleaseTime, true).SimplifyFormatTime;
                    }
                }
                    
                //result.DisposalHold = result.HoldType == 2;  //临时方案
                if (result.NodeType == RMNodeType.PhyBox)
                {
                    //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
                    var box = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.Id);
                    if (box != null)
                    {
                        result.DisposalHold = true;
                        result.HoldType = 2;
                        result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                        result.HoldReleaseTime = box.HoldReleaseTime;
                        result.HoldProfileTitle = GetHoldTitle(allRelatedHolds, box);
                        result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                    }
                }
                else if (result.NodeType == RMNodeType.PhyFile)
                {
                    // && a.AllianceType == RecordsConstants.RecordHold_PhyProfile
                    var file = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.Id);
                    var box = result.BoxId == Guid.Empty ? null : allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.BoxId);
                    if (file != null)
                    {
                        result.DisposalHold = true;
                        result.HoldType = 2;
                        result.HoldStatus = HoldStatus.Self;
                        result.HoldBy = this.AssembleAccountDisplayName(file.HoldBy, accountMap.Values);
                        result.HoldReleaseTime = file.HoldReleaseTime;
                        result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                        result.HoldProfileTitle = GetHoldTitle(allRelatedHolds, file);
                    }
                    else if (box != null)
                    {
                        result.DisposalHold = true;
                        result.HoldType = 2;
                        result.HoldStatus = HoldStatus.Inherit;
                        result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                        result.HoldReleaseTime = box.HoldReleaseTime;
                        result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                        result.HoldProfileTitle = GetHoldTitle(allRelatedHolds, box);
                    }
                }
                else if (result.NodeType == RMNodeType.PhyRecord)
                {
                    //&& a.AllianceType == RecordsConstants.RecordHold_PhyProfile
                    var file = allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.FileId);
                    var box = result.BoxId == Guid.Empty ? null : allHoldRelatedRecords.FirstOrDefault(a => a.Id == result.BoxId);
                    if (file != null)
                    {
                        result.DisposalHold = true;
                        result.HoldType = 2;
                        result.HoldStatus = HoldStatus.Self;
                        result.HoldBy = this.AssembleAccountDisplayName(file.HoldBy, accountMap.Values);
                        result.HoldReleaseTime = file.HoldReleaseTime;
                        result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, file.HoldReleaseTime, true).SimplifyFormatTime;
                        result.HoldProfileTitle = GetHoldTitle(allRelatedHolds, file);
                    }
                    else if (box != null)
                    {
                        result.DisposalHold = true;
                        result.HoldType = 2;
                        result.HoldStatus = HoldStatus.Inherit;
                        result.HoldBy = this.AssembleAccountDisplayName(box.HoldBy, accountMap.Values);
                        result.HoldReleaseTime = box.HoldReleaseTime;
                        result.HoldReleaseTimeStr = GeneralSettingService.ConvertTiksToDateTime(gls, box.HoldReleaseTime, true).SimplifyFormatTime;
                        result.HoldProfileTitle = GetHoldTitle(allRelatedHolds, box);
                    }
                }
                if (!result.DisposalHold)
                {
                    result.HoldType = 0;
                    result.HoldBy = null;
                }
            }
        }

        private string GetHoldTitle(List<RMHold> allRelatedHolds, Record box)
        {
            var existHoldIds = GetAllExistHoldIds(box);
            var existHolds = allRelatedHolds.Where(h => existHoldIds.Contains(h.Id)).OrderBy(h => h.Id, new HoldSpecialComparer(existHoldIds)).ToList();
            var HoldProfileTitle = string.Join("; ", existHolds.Select(h => h.Name));
            return HoldProfileTitle;
        }

        private void AppendHoldInfo(PhysicalObjectDto info, List<string> holdIds)
        {
            List<RMHold> holds = HoldDao.GetHoldByIds(holdIds).OrderBy(h => h.Id, new HoldSpecialComparer(holdIds)).ToList();
            if (holds != null && holdIds.Count > 0)
            {
                info.HoldProfileTitle = string.Join("; ", holds.Select(h => h.Name));
                info.HoldProfileComment = string.Join("; ", holds.Select(h => string.IsNullOrEmpty(h.Description) ? I18NEntity.GetString("RM_JS_Common_Pending") : h.Description));
            }
        }

        private string AssembleAccountDisplayName(string principalName, IEnumerable<RMAccount> accounts)
        {
            RMAccount temp = accounts.FirstOrDefault(a => a.UserPrincipalName == principalName);
            if (temp != null)
            {
                return temp.DisplayName;
            }
            return principalName;
        }
        private string AssembleAccountDisplayName(string orignHoldBy, string holdByUsers, List<string> holdIds)
        {
            List<HoldUser> allHoldByUsers = string.IsNullOrEmpty(holdByUsers) ? new List<HoldUser>() : JsonConvert.DeserializeObject<List<HoldUser>>(holdByUsers);
            if (allHoldByUsers.Count > 0)
            {
                var accountMap = AccountDao.FindAll();
                foreach (var holdByUser in allHoldByUsers)
                {
                    holdByUser.HoldBy = AssembleAccountDisplayName(holdByUser.HoldBy, accountMap);
                }
                var distinctHoldByUsers = allHoldByUsers.Select(h => h.HoldBy).Distinct();
                if (distinctHoldByUsers.Count() == 1)
                {
                    return distinctHoldByUsers.FirstOrDefault();
                }
                else
                {
                    return string.Join("; ", allHoldByUsers.OrderBy(h => h.HoldId, new HoldSpecialComparer(holdIds)).Select(h => h.HoldBy));
                }
            }
            else
            {
                RMAccount temp = AccountDao.Find(a => a.UserPrincipalName == orignHoldBy);
                if (temp != null)
                {
                    return temp.DisplayName;
                }
                return orignHoldBy;
            }
        }

        //此方法注重了减少DB 连接，防止出现大量DB 连接的case，目前实现为：一个push column 连接一次PushColumn表
        public void AppendPushedColumns(List<PhysicalObjectDto> results)
        {
            var templateIds = results.Select(r => r.TemplateId).ToList();
            var templates = TemplateDao.GetTemplateByIds(templateIds);
            var templateAndPushColumnsMapping = GetTemplateIdAndPushColumnsMapping(templates, results);
            if (templateAndPushColumnsMapping.Count == 0)
            {
                logger.Info("No push columns in current results.");
                return;
            }
            var allPushColumnValues = GetAllPushColumnValues(templateAndPushColumnsMapping, results);
            //遍历Result，对push column 进行赋值操作
            foreach (var physicalDto in results)
            {
                if (templateAndPushColumnsMapping.Keys.Contains(physicalDto.TemplateId))
                {
                    var columns = templateAndPushColumnsMapping[physicalDto.TemplateId];
                    foreach (var column in columns)
                    {
                        //if(physicalDto.MetaInfo.ContainsKey(column.UniqueId.ToString()))
                        //{
                        var physicalObjectId = GetPhysicalObjectId(column, physicalDto);
                        var pushColumnInDB = allPushColumnValues.Where(a => a.PhysicalObjectId == physicalObjectId && a.ColumnUniqueId == column.UniqueId).FirstOrDefault();
                        //RECO-5160 对于刚变成Push 的column，由于Push Column 表没有值，会导致当前节点的value 为空，所以在回显逻辑上添加处理，如果是当前template 创建的push column 并且push column 没有DB 记录，就使用metainfo 的值回显
                        if (PushColumnCreatedOnCurrentTemplate(column) && pushColumnInDB == null)
                        {
                            //当前级别创建的push column，而且push column db没有值，使用metainfo 的值，不需要赋值
                        }
                        else
                        {
                            physicalDto.MetaInfo[column.UniqueId.ToString()] = pushColumnInDB?.ColumnValue;
                        }
                        //}
                    }
                }
            }
        }

        private bool PushColumnCreatedOnCurrentTemplate(ColumnXmlSchema column)
        {
            bool result = false;
            if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild
                && (column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) != (int)TemplateInheritSettingEnum.InheritFromParentBox
                && (column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) != (int)TemplateInheritSettingEnum.InheritFromParentFolder
                )
            {
                result = true;
            }
            return result;
        }

        //获取TemplateId 和Template下面所有push 相关column 的mapping 集合
        private Dictionary<int, List<ColumnXmlSchema>> GetTemplateIdAndPushColumnsMapping(List<RMTemplate> templates, List<PhysicalObjectDto> results)
        {
            var templateAndPushColumnsMapping = new Dictionary<int, List<ColumnXmlSchema>>();
            results.ForEach(r =>
            {
                var template = templates.FirstOrDefault(t => t.Id == r.TemplateId);
                if (template != null)
                {
                    List<ColumnXmlSchema> pushColumns = GetPushColumns(template, r);
                    if (!templateAndPushColumnsMapping.ContainsKey(template.Id))
                    {
                        templateAndPushColumnsMapping.Add(template.Id, pushColumns);
                    }
                }
            });
            return templateAndPushColumnsMapping;
        }

        private List<ColumnXmlSchema> GetPushColumns(RMTemplate template, PhysicalObjectDto results)
        {
            var templateAndPushColumnsMapping = new List<ColumnXmlSchema>();
            if (results.NodeType == RMNodeType.PhyBox)
            {
                var pushColumns = new List<ColumnXmlSchema>();
                var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                pushColumns = schema.Columns.Where(col =>
                ((col.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)).ToList();
                if (pushColumns.Count > 0)
                {
                    templateAndPushColumnsMapping.AddRange(pushColumns);
                }
            }
            else if (results.NodeType == RMNodeType.PhyFile)
            {
                templateAndPushColumnsMapping.AddRange(GetPushColumnToFold(template, results));
            }
            else if (results.NodeType == RMNodeType.PhyRecord)
            {
                templateAndPushColumnsMapping.AddRange(GetPushColumnToRecord(template, results));
            }
            return templateAndPushColumnsMapping;
        }

        private List<ColumnXmlSchema> GetPushColumnToFold(RMTemplate template, PhysicalObjectDto physicalObject)
        {
            List<ColumnXmlSchema> pushColumns = new List<ColumnXmlSchema>();
            if (physicalObject.BoxId == Guid.Empty)
            {
                logger.Error("box id is empty.");
                var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                pushColumns = schema.Columns.Where(col =>
                ((col.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)).ToList();
                //boxid ==null 说明自身是跟节点  需要返回TemplateInheritSettingEnum.PushToChild的column
                return pushColumns;
            }
            else
            {
                var foldSchema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
                pushColumns = foldSchema.Columns.Where(col =>
                ((col.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)).ToList();

                Record box = ExplorerDao.GetPhysicalRecordById(physicalObject.BoxId);
                if (box == null)
                {
                    logger.Error("Can't find fold's parent box,box id is {0}", physicalObject.BoxId.ToString());
                    return pushColumns;
                }
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
                if (boxTemplate == null)
                {
                    logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                    return pushColumns;
                }
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue)
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentBox | (int)TemplateInheritSettingEnum.AllowModifyValue;
                        }
                        else
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentBox;
                        }
                        pushColumns.Add(item);
                    }
                }
                return pushColumns;
            }
        }

        private List<ColumnXmlSchema> GetPushColumnToRecord(RMTemplate template, PhysicalObjectDto physicalObject)
        {
            List<ColumnXmlSchema> pushColumns = new List<ColumnXmlSchema>();
            if (physicalObject.FileId != Guid.Empty)
            {
                Record fold = ExplorerDao.GetPhysicalRecordById(physicalObject.FileId);
                if (fold == null)
                {
                    logger.Error("Can't find node's parent fold,fold id is {0}", physicalObject.FileId.ToString());
                    return pushColumns;
                }
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(fold.TemplateId);
                if (foldTemplate == null)
                {
                    logger.Error("Can't find fold's template ,template id is {0}", fold.TemplateId.ToString());
                    return pushColumns;
                }
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(foldTemplate.ColumnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue)
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentFolder | (int)TemplateInheritSettingEnum.AllowModifyValue;
                        }
                        else
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentFolder;
                        }
                        pushColumns.Add(item);
                    }
                }
            }
            if (physicalObject.BoxId != Guid.Empty)
            {
                Record box = ExplorerDao.GetPhysicalRecordById(physicalObject.BoxId);
                if (box == null)
                {
                    logger.Error("Can't find node's parent fold,box id is {0}", physicalObject.BoxId.ToString());
                    return pushColumns;
                }
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
                if (boxTemplate == null)
                {
                    logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                    return pushColumns;
                }
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue)
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentBox | (int)TemplateInheritSettingEnum.AllowModifyValue;
                        }
                        else
                        {
                            item.TemplateInheritSetting = (int)TemplateInheritSettingEnum.InheritFromParentBox;
                        }
                        pushColumns.Add(item);
                    }
                }
            }
            return pushColumns;
        }

        //获取所有push column 和physical object 的值的集合
        private List<RMPhysicalPushColumn> GetAllPushColumnValues(Dictionary<int, List<ColumnXmlSchema>> templateAndPushColumnsMapping, List<PhysicalObjectDto> results)
        {
            var allPushColumnValues = new List<RMPhysicalPushColumn>();
            foreach (var columns in templateAndPushColumnsMapping.Values)
            {
                foreach (var col in columns)
                {
                    var columnPhysicalObjectIdInPushDB = new List<Guid>();
                    foreach (var physicalDto in results)
                    {
                        var physicalObjectId = GetPhysicalObjectId(col, physicalDto);
                        columnPhysicalObjectIdInPushDB.Add(physicalObjectId);
                    }
                    var pushColumnsValues = GetPushedColumns(col.UniqueId, columnPhysicalObjectIdInPushDB.Distinct().ToList());
                    allPushColumnValues.AddRange(pushColumnsValues);
                }
            }
            return allPushColumnValues;
        }

        private Guid GetPhysicalObjectId(ColumnXmlSchema column, PhysicalObjectDto physicalObject)
        {
            Guid physicalObjectId = Guid.Empty;
            if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentBox) == (int)TemplateInheritSettingEnum.InheritFromParentBox)
            {
                physicalObjectId = physicalObject.BoxId;
            }
            else if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.InheritFromParentFolder) == (int)TemplateInheritSettingEnum.InheritFromParentFolder)
            {
                physicalObjectId = physicalObject.FileId;
            }
            else if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
            {
                physicalObjectId = physicalObject.Id;
            }
            return physicalObjectId;
        }

        private List<RMPhysicalPushColumn> GetPushedColumns(Guid columnId, List<Guid> physicalObjectId)
        {
            return RMPhysicalPushColumnDao.GetPushColumns(columnId, physicalObjectId);
        }

        public PhysicalObjectDto GetPhysicalObjectByUniqueId(string uniqueId)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var data = ExplorerDao.QueryAll(r => r.RecordsId == uniqueId).FirstOrDefault();
                result = ConvertUtil.ConvertRMBaseRecordToPhysical(data);
                this.AppendPhyHoldInfo(result);
            }
            catch (Exception ex)
            {
                logger.Error($"Get GetPhysicalObject by unique id: [{uniqueId}], error: [{ex.ToString()}]");
            }
            return result;
        }

        public PhysicalObjectDto GetPhysicalObjectById(Guid id)
        {
            var result = new PhysicalObjectDto();
            try
            {
                var data = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
                result = ConvertUtil.ConvertRMBaseRecordToPhysical(data);
            }
            catch (Exception ex)
            {
                logger.Error($"Get GetPhysicalObject by id: [{id}], error: [{ex.ToString()}]");
            }
            return result;
        }

        public Dictionary<Guid, string> GetPushedColumnValues(Guid phyObjUniqueId, IEnumerable<PushColumnDto> columnUniqueIDs)
        {
            var pushColumnInfos = columnUniqueIDs.GroupBy(c => c.PhyObjId).ToDictionary(key => key.Key, value => value.Select(v => v.ColumnUniqueId).ToList());
            var totalColumnValues = new List<RMPhysicalPushColumn>();
            foreach (var pushColumnInfo in pushColumnInfos)
            {
                var columnValues = RMPhysicalPushColumnDao.GetColumnValues(pushColumnInfo.Key, pushColumnInfo.Value);
                if (columnValues != null && columnValues.Count > 0)
                {
                    totalColumnValues.AddRange(columnValues);
                }
            }
            return totalColumnValues.ToDictionary(s => s.ColumnUniqueId, s => s.ColumnValue);
        }

        [Audit(Action = AuditAction.DeletePhysicalObject, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public DeleteResultInfo DeletePhysicalObject(List<PhysicalObjectDto> physicalObjectDtos)
        {
            var result = new DeleteResultInfo();
            result.HasError = false;
            result.ErrorDatas = new List<Guid>();
            var ids = new List<Guid>();
            var parentBoxIds = new List<Guid>();
            if (physicalObjectDtos != null && physicalObjectDtos.Count > 0)
            {
                var builder = SqlQuerySpecBuilderFactory.CreatePhysicalExplorerBuilder();
                var allRecords = ExplorerDao.GetRecordByIds(physicalObjectDtos.Select(p => p.Id).ToList());
                foreach (var physicalObjectDto in physicalObjectDtos)
                {
                    var tempRecord = allRecords.FirstOrDefault(r => r.Id == physicalObjectDto.Id);
                    if (tempRecord != null)
                    {
                        var tempIds = new List<Guid>
                        {
                            tempRecord.Id
                        };
                        if (tempRecord.BoxId != Guid.Empty) { tempIds.Add(tempRecord.BoxId); }
                        if (tempRecord.FileId != Guid.Empty) { tempIds.Add(tempRecord.FileId); }
                        var isHold = ExplorerDao.GetHoldRecordsByIds(tempIds).Any();
                        if (isHold)
                        {
                            logger.Error($"Delete physicla object is hold, object id: {physicalObjectDto.Id}");
                            result.HasError = true;
                            result.ErrorDatas.Add(physicalObjectDto.Id);
                            continue;
                        }
                    }

                    ids.Add(physicalObjectDto.Id);
                    if(physicalObjectDto.NodeType == RMNodeType.PhyFile && physicalObjectDto.BoxId != Guid.Empty)
                    {
                        parentBoxIds.Add(physicalObjectDto.BoxId);
                    }
                    if (physicalObjectDto.NodeType != RMNodeType.PhyRecord)
                    {
                        #region Obsolete code
                        //List<Expression> allExpressionList = new List<Expression>();
                        //ParameterExpression param = Expression.Parameter(typeof(Record), "c");
                        //GenerateDeepQueryExpression((int)physicalObjectDto.NodeType, physicalObjectDto.Id, allExpressionList, param);
                        //if (allExpressionList.Count > 0)
                        //{
                        //    List<Expression> nodeStatusExpressionList = new List<Expression>();
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Active));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Closed));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Missing));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Destroyed));
                        //    allExpressionList.Add(nodeStatusExpressionList.Aggregate(Expression.OrElse));
                        //    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", SourceFlag.Physical));
                        //    var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);

                        //var subNodes = ExplorerDao.QueryAll(Expression.Lambda<Func<Record, bool>>(queryExpr, param));
                        //}
                        #endregion
                        var queryDtoV2 = physicalObjectDto.Convert2ExplorerQueryV2Dto();
                        PhysicalExplorerQueryDtoExtension.GenerateDeepQueryExpression(PhysicalNodeTypeConverter.Convert2NodeLevel(physicalObjectDto.NodeType), physicalObjectDto.Id, queryDtoV2.QueryOption.FilterOption);
                        do
                        {
                            var queryData = ExplorerDao.SearchRecordsV2(queryDtoV2, builder);
                            queryDtoV2.PagingInfo.PageIndex = queryData.Item2;
                            foreach (var subNode in queryData.Item1)
                            {
                                ids.Add(subNode.Id);
                            }
                        }
                        while (!string.IsNullOrEmpty(queryDtoV2.PagingInfo.PageIndex));
                    }
                }
                try
                {
                    //TODO: 未更新MetaInfo里的Status
                    var idString = string.Join(";", ids.ToArray());
                    logger.Info($"Delete records, ids :{idString}.");
                    ExplorerDao.UpdateAll(s => ids.Contains(s.Id), r => { r.RecordStatus = (int)RMRecordStatus.RMDeleted; });
                    ReEvaluateParentBoxesAsync(parentBoxIds).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.Error($"Delete physicla object by ids: [{string.Join(";", ids.ToArray())}], error: [{ex.ToString()}]");
                    result.HasError = true;
                    result.ErrorDatas.AddRange(physicalObjectDtos.Select(p => p.Id).ToList());
                }
            }
            return result;
        }

        public List<PhysicalObjectDto> PreDeletePhysicalObjects(List<PhysicalObjectDto> physicalObjectDtos)
        {
            List<PhysicalObjectDto> result = new List<PhysicalObjectDto>();
            var builder = SqlQuerySpecBuilderFactory.CreatePhysicalExplorerBuilder();
            foreach (var physicalDto in physicalObjectDtos)
            {
                if (physicalDto.NodeType != RMNodeType.PhyRecord)
                {
                    try
                    {
                        #region Obsolete code
                        //List<Expression> allExpressionList = new List<Expression>();
                        //ParameterExpression param = Expression.Parameter(typeof(Record), "c");
                        //GenerateShallowQueryExpression((int)physicalDto.NodeType, physicalDto.Id, allExpressionList, param);
                        //if (allExpressionList.Count > 0)
                        //{
                        //    List<Expression> nodeStatusExpressionList = new List<Expression>();
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Active));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Closed));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Missing));
                        //    nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Destroyed));
                        //    allExpressionList.Add(nodeStatusExpressionList.Aggregate(Expression.OrElse));
                        //    allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", SourceFlag.Physical));
                        //    var queryExpr = allExpressionList.Aggregate(Expression.AndAlso);

                        //    if (ExplorerDao.QueryByPage(Expression.Lambda<Func<Record, bool>>(queryExpr, param), 1).Item1.Count() > 0)
                        //    {
                        //        result.Add(physicalDto);
                        //    }
                        //}
                        #endregion
                        var queryDtoV2 = physicalDto.Convert2ExplorerQueryV2Dto(pageSize: 1);
                        PhysicalExplorerQueryDtoExtension.GenerateShallowQueryExpression(PhysicalNodeTypeConverter.Convert2NodeLevel(physicalDto.NodeType), physicalDto.Id, queryDtoV2.QueryOption.FilterOption);
                        if (ExplorerDao.SearchRecordsV2(queryDtoV2, builder).Item1.Count() > 0)
                        {
                            result.Add(physicalDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error in PreDeletePhysicalObjects for physical object : {physicalDto.Name}, {physicalDto.Id}. reason : {ex.ToString()}");
                    }
                }
            }
            return result;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RemovePersonalHold, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> RemovePersonalHoldAsync(List<Guid> nodeIDs)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var loanBoxIds = new List<Guid>();
                var loanFolderIds = new List<Guid>();
                var returnsList = new List<PhysicalReturnObject>();
                var nodes = ExplorerDao.QueryAll(o => nodeIDs.Contains(o.Id)).ToList();
                foreach (var node in nodes)
                {
                    if (node.NodeType == (int)RMNodeType.PhyFile)
                    {
                        loanFolderIds.Add(node.Id);
                    }
                    if (node.NodeType == (int)RMNodeType.PhyBox)
                    {
                        loanBoxIds.Add(node.Id);
                    }
                    returnsList.Add(new PhysicalReturnObject() { UniqueId = node.Id, NodeType = node.NodeType });
                }
                if (loanBoxIds.Count > 0)
                {
                    if (RecordsConstants.PhysicalLoanOrReturnBatchOperationMaxCount < GetPhyBoxAndFileCountByBoxIds(loanBoxIds) + loanFolderIds.Count)
                    {
                        var param = new BoxLoanJobMessage()
                        {
                            LoanAction = LoanAction.Reutrn,
                            Returns = returnsList
                        };
                        PhysicalRequestService.StartLoanOrReturnBoxJob(JobType.PhysicalReturnBox, param);
                        msg.Extension = true.ToString();
                        return msg;
                    }
                    else
                    {
                        foreach (var boxId in loanBoxIds)
                        {
                            await ReutrnPhyBoxAndFileByBoxIdAsync(boxId);
                        }
                        await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(nodeIDs);
                        RemovePersonalHold4Record(nodeIDs.Where(id => !loanBoxIds.Contains(id)).ToList());
                    }
                }
                else
                {
                    await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(nodeIDs);
                    RemovePersonalHold4Record(nodeIDs);
                }
            }
            catch (Exception ex)
            {
                logger.Error("cancel hold and delete alliance by recordId has error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        /// <summary>
        /// remove the personal hold in Cosmos DB
        /// </summary>
        /// <param name="ids"></param>
        private void RemovePersonalHold4Record(List<Guid> ids, bool ifAddAudit = true)
        {
            var auditList = new List<PhysicalRecordActionAudit>();
            var returnHistory = new List<RecordReturnLoanDataHistory>();
            foreach (var id in ids)
            {
                var record = ExplorerDao.GetPhysicalRawDataById(id);
                if (record == null) continue;
                var physicalAudit = RecordsHistoryService.BuildPhysicalReturnLoanAudit(record.Id, record.CustomColumnDic);
                auditList.Add(physicalAudit);
                record.RemovePersonalHoldData();
                ExplorerDao.Upsert(record);
                returnHistory.Add(new RecordReturnLoanDataHistory
                {
                    PartitionKey = id.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    ItemName = record.LeafName,
                    UniqueId = record.RecordsId,
                    ReturnTime = DateTime.UtcNow.Ticks,
                    RequestBy = TenantLocalValue.DisplayName,
                    HomeLocation = GetPhysicalObjectFullPath(id)
                });
            }

            RecordsHistoryService.AddRecordReturnLoanHistory(returnHistory);

            if (ifAddAudit)
            {
                RecordsHistoryService.AddPhysicalAudit(auditList);
            }
        }

        [Audit(Module = AuditModule.Mobile, Category = AuditCategory.Mobile, Action = AuditAction.MobileReturn, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<RAReturnMessage> RemovePersonalHoldForMobileAsync(List<Guid> nodeIDs)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                string currentUserDisplayName = (await UserService.GetUserByNameAsync(TenantLocalValue.LogonUserEmail))?.DisplayName ?? string.Empty;
                var returnHistory = new List<RecordReturnLoanDataHistory>();
                foreach (var id in nodeIDs)
                {
                    var record = ExplorerDao.GetPhysicalRawDataById(id);
                    if (record == null) continue;
                    returnHistory.Add(new RecordReturnLoanDataHistory
                    {
                        PartitionKey = id.ToString(),
                        RowKey = Guid.NewGuid().ToString(),
                        ItemName = record.LeafName,
                        UniqueId = record.RecordsId,
                        ReturnTime = DateTime.UtcNow.Ticks,
                        RequestBy = currentUserDisplayName,
                        HomeLocation = GetPhysicalObjectFullPath(id)
                    });
                }
                await RecordLoanAllianceDao.BatchDeleteRecordAllianceByIdsAsync(nodeIDs);
                RecordsHistoryService.AddRecordReturnLoanHistory(returnHistory);
            }
            catch (Exception ex)
            {
                logger.Error("cancel hold and delete alliance by recordId has error:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        public async Task<PhysicalResultInfo> QueryPhysicalNodesAsync(PhysicalExplorerQueryDto dto)
        {
            PhysicalResultInfo result = new PhysicalResultInfo();
            try
            {
                if (dto == null)
                {
                    throw new Exception("query dto is null.");
                }
                int.TryParse(dto.NodeId, out int locationId);
                await DealWithPhysicalBottomLocationIdAsync(dto);
                var nodeId = Guid.Empty;
                if (Guid.TryParse(dto.NodeId, out nodeId))
                {
                    result.Datas = new List<PhysicalObjectDto>();
                    if (dto != null)
                    {
                        if (dto.PagingInfo != null)
                        {
                            result.PagingInfo = dto.PagingInfo;
                        }
                        else
                        {
                            result.PagingInfo = new PhysicalExplorerPagingInfo()
                            {
                                PageIndex = 0,
                                PageSize = 5
                            };
                        }
                    }
                    string keywords = dto.FilterOption == null || string.IsNullOrEmpty(dto.FilterOption.SearchKey) ? null : dto.FilterOption.SearchKey;
                    var hasNext = false;
                    Tuple<IEnumerable<Record>, string> queryData = null;
                    int totalCount = 0;
                    using (new PerformanceScope("RecordsExplorer_QueryPhysicalNodes"))
                    {
                        ////remove term permission filter
                        //var termPermDto = GetSecurityTermDto();
                        //bool filterOutPhyRecord = NeedFilterOutPhyRecord(termPermDto, dto);
                        bool filterOutPhyRecord = false;
                        if (!(await IsPhysicalEndUserAsync()))
                        {
                            //if (string.IsNullOrEmpty(keywords))
                            //{
                            //    Expression<Func<Record, bool>> whereLambda = GetFilterLambdaForPhysical(dto, filterOutPhyRecord);
                            //    logger.Debug($"Query lambda express is : {whereLambda.ToString()}.");
                            //    totalCount = ExplorerDao.QueryCount(whereLambda);
                            //    queryData = ExplorerDao.QueryDataWithoutTotal(result.PagingInfo.currentBrowserState, result.PagingInfo.PageSize, out hasNext, whereLambda);
                            //}
                            //else
                            //{
                            //    totalCount = ExplorerDao.QueryCountBySql(dto, termPermDto, filterOutPhyRecord);
                            //    queryData = ExplorerDao.QueryDataBySqlWithoutTotal(dto, result.PagingInfo.currentBrowserState, result.PagingInfo.PageSize, out hasNext, termPermDto, filterOutPhyRecord);
                            //}
                        }
                        else
                        {
                            //end user
                            if (!string.IsNullOrEmpty(keywords))
                            {
                                logger.Info($"search by keywords : {keywords}");
                            }
                            var isLocationNode = locationId > 0;
                            var scopeId = isLocationNode ? locationId.ToString() : dto.NodeId;
                            (dto.PermissionIds, dto.HaveCurrentNodePermission) = await GetPermissonConditionAsync(scopeId, dto);

                            //totalCount = ExplorerDao.QueryCountBySql(dto, termPermDto, filterOutPhyRecord);
                            //queryData = ExplorerDao.QueryDataBySqlWithoutTotal(dto, result.PagingInfo.currentBrowserState, result.PagingInfo.PageSize, out hasNext, termPermDto, filterOutPhyRecord);
                        }

                        ///new query mode, will use it in the future

                        var queryDtoV2 = dto.Convert2ExplorerQueryV2Dto(null, filterOutPhyRecord);
                        var builder = DB.Explorer.Dao.CosmosImp.Builder.SqlQuerySpecBuilderFactory.CreatePhysicalExplorerBuilder();
                        queryData = ExplorerDao.SearchRecordsV2(queryDtoV2, builder);
                        foreach (Record rec in queryData.Item1)
                        {
                            rec.AppendMetaInfoForOldLogic();
                        }
                        totalCount = ExplorerDao.QueryCount(queryDtoV2, builder);
                    }

                    using (new PerformanceScope("RecordsExplorer_Convert"))
                    {
                        result.PagingInfo.Total = totalCount;
                        //result.PagingInfo.HasNextPage = hasNext;
                        result.PagingInfo.HasNextPage = !string.IsNullOrEmpty(queryData.Item2);
                        if (queryData.Item2 != null)
                        {
                            result.PagingInfo.currentBrowserState = queryData.Item2;
                        }
                        var queryList = queryData.Item1.ToList();
                        var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                        result.Datas = queryList.ConvertAll(e => { return ConvertUtil.ConvertRMBaseRecordToPhysical(e, accountMap); });
                        this.AppendPhyPersonalHoldInfo(result.Datas, accountMap);
                        //目前前台Table回显的时候，不会回显push column，所以此处反悔的数据，不对push column 赋值，来提高性能
                        //this.AppendPushedColumns(result.Datas);
                        await this.AppendDisposalDueDateAndRuleInfoAsync(result.Datas, nodeId, dto.CurrentNodeType);
                    }
                }
                else
                {
                    logger.Error($"QueryPhysicalNodes, current id seems is not in correct format, id value: [{dto.NodeId}].");
                }

            }
            catch (Exception ex)
            {
                logger.Error($"ERROR in QueryPhysicalNodes : [{ex.ToString()}]");
            }
            return result;
        }

        public async Task<bool> IsPhysicalEndUserAsync()
        {
           return await ExplorerQueryParamProcesser.IsPhysicalEndUserAsync();
        }

        /// <summary>
        /// Get PermissionIds for Search.
        /// </summary>
        /// <returns></returns>
        public async Task<List<int>> GetPermissionConditionAsync()
        {
           return await ExplorerQueryParamProcesser.GetPermissionConditionAsync();
        }

        public string GetPhysicalScopeIdFullPath(Guid nodeId)
        {
            var node = ExplorerDao.QueryAll(o => o.Id == nodeId).First();
            var id = node.NodeId;
            var location = LocationDao.GetLocationInfo(node.LocationId);
            string locationPath = location.DirPath;
            string idFullPath = string.Empty;
            bool isNewData = node.Ancestors != null;
            switch (node.NodeType)
            {
                case (int)RMNodeType.PhyCustom:
                    idFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                    break;
                case (int)RMNodeType.PhyBox:
                    idFullPath = isNewData ? $"{locationPath}{node.GetScopeIdPath()}/" : $"{locationPath}{id}/";
                    break;
                case (int)RMNodeType.PhyFile:
                    if (isNewData)
                    {
                        idFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                    }
                    else
                    {
                        if (node.BoxId != Guid.Empty)
                        {
                            idFullPath = $"{locationPath}{node.BoxId}/{id}/";
                        }
                        else
                        {
                            //location下的folder
                            idFullPath = $"{locationPath}{id}/";
                        }
                    }
                    break;
                case (int)RMNodeType.PhyRecord:
                    if (isNewData)
                    {
                        idFullPath = $"{locationPath}{node.GetScopeIdPath()}/";
                    }
                    else
                    {
                        if (node.BoxId != Guid.Empty)
                        {
                            idFullPath = $"{locationPath}{node.BoxId}/{node.FileId}/{id}/";
                        }
                        else
                        {
                            idFullPath = $"{locationPath}{node.FileId}/{id}/";
                        }
                    }
                    break;
                default:
                    break;
            }

            return idFullPath;
        }

        private async Task<(List<int>, bool)> GetPermissonConditionAsync(string scopeId, PhysicalExplorerQueryDto dto)
        {
            var isEnduser = await IsPhysicalEndUserAsync();
            var permissionConditions = new List<Expression>();
            bool hasScopePermission = true;
            if (!isEnduser)
            {
                return (null,hasScopePermission);//管理员不做限制
            }
            else
            {
                var permissionIds = new List<int>();
                try
                {
                    var isSearch = IsExistFilterOrSearch(dto);
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var idFullPath = PermissionManagementService.GetScopeIdFullPath(scopeId);
                    hasScopePermission = PermissionManagementService.HasCurrentScopePermission(idFullPath, userAndGroupIds);
                    if (!hasScopePermission)
                    {
                        if (isSearch)
                        {
                            //Explorer Table Search数据时，需要平铺显示, 否则只查询下层节点
                            permissionIds = PermissionManagementService.GetIncludeScopePermissionIdsForSearch(idFullPath, userAndGroupIds);
                        }
                        else
                        {
                            permissionIds = PermissionManagementService.GetIncludeScopePermissionIds(scopeId, userAndGroupIds);
                        }
                    }
                    else
                    {
                        if (isSearch)
                        {
                            permissionIds = PermissionManagementService.GetExcludeScopePermissionIdsForSearch(idFullPath, userAndGroupIds);
                        }
                        else
                        {
                            permissionIds = PermissionManagementService.GetExcludeScopePermissionIds(scopeId, userAndGroupIds);
                        }
                    }
                    return (permissionIds, hasScopePermission);
                }
                catch (Exception ex)
                {

                    logger.Warn($"An error occured when GetPermissonCondition, message:{ex.ToString()}");
                    return (permissionIds, hasScopePermission);
                }
            }
        }

        private bool IsExistFilterOrSearch(PhysicalExplorerQueryDto dto)
        {
            bool result = false;
            if (dto.FilterOption != null)
            {
                if (!string.IsNullOrEmpty(dto.FilterOption.SearchKey)
                    || dto.FilterOption.Status != (int)RMRecordStatus.None
                    || dto.FilterOption.NodeType != RMNodeLevel.Undefined
                    || (dto.FilterOption.RecordsOwner != null && dto.FilterOption.RecordsOwner.Count > 0)
                    || (dto.FilterOption.CreatedBy != null && dto.FilterOption.CreatedBy.Count > 0)
                    || (dto.FilterOption.ModifiedBy != null && dto.FilterOption.ModifiedBy.Count > 0))
                {
                    result = true;
                }
            }
            return result;
        }

        public async Task<PhysicalObjectDto> FindPhysicalObjectByRecordsIdAsync(string recordsId)
        {
            PhysicalObjectDto phyObj = null;
            var record = ExplorerDao.QueryAll(s => s.RecordsId == recordsId && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
            if (record != null)
            {
                var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                phyObj = ConvertUtil.ConvertRMBaseRecordToPhysical(record, accountMap);
                var tempList = new List<PhysicalObjectDto> { phyObj };
                Guid parentId = Guid.Empty;
                RMNodeLevel parentLevel = RMNodeLevel.PhysicalBottomLocation;
                switch (phyObj.NodeType)
                {
                    case RMNodeType.PhyBox:
                        parentId = record.LocationId;
                        parentLevel = RMNodeLevel.PhysicalBottomLocation;
                        break;
                    case RMNodeType.PhyFile:
                        if (record.BoxId == Guid.Empty)
                        {
                            parentId = record.LocationId;
                            parentLevel = RMNodeLevel.PhysicalBottomLocation;
                        }
                        else
                        {
                            parentId = record.BoxId;
                            parentLevel = RMNodeLevel.PhysicalBox;
                        }
                        break;
                    case RMNodeType.PhyRecord:
                        parentId = record.FolderId;
                        parentLevel = RMNodeLevel.PhysicalRecord;
                        break;
                    default:
                        break;
                }
                this.AppendPhyPersonalHoldInfo(tempList, accountMap);
                await this.AppendDisposalDueDateAndRuleInfoAsync(tempList, parentId, parentLevel);
                this.AppendPhyTermName(tempList);
                this.AppendPhyHomeLocationName(tempList);
            }
            return phyObj;
        }

        public async Task<PhysicalObjectDto> FindPhysicalObjectByBarcodeAsync(string barcode)
        {
            PhysicalObjectDto phyObj = null;
            var record = ExplorerDao.QueryAll(s => s.CustomColumnDic[DefaultColumnIDs.Barcode].Value == barcode && (s.RecordStatus == (int)RMRecordStatus.Active || s.RecordStatus == (int)RMRecordStatus.Closed || s.RecordStatus == (int)RMRecordStatus.Missing || s.RecordStatus == (int)RMRecordStatus.Destroyed)).FirstOrDefault();
            if (record != null)
            {
                var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
                phyObj = ConvertUtil.ConvertRMBaseRecordToPhysical(record, accountMap);
                var tempList = new List<PhysicalObjectDto> { phyObj };
                Guid parentId = Guid.Empty;
                RMNodeLevel parentLevel = RMNodeLevel.PhysicalBottomLocation;
                switch (phyObj.NodeType)
                {
                    case RMNodeType.PhyBox:
                        parentId = record.LocationId;
                        parentLevel = RMNodeLevel.PhysicalBottomLocation;
                        break;
                    case RMNodeType.PhyFile:
                        if (record.BoxId == Guid.Empty)
                        {
                            parentId = record.LocationId;
                            parentLevel = RMNodeLevel.PhysicalBottomLocation;
                        }
                        else
                        {
                            parentId = record.BoxId;
                            parentLevel = RMNodeLevel.PhysicalBox;
                        }
                        break;
                    case RMNodeType.PhyRecord:
                        parentId = record.FolderId;
                        parentLevel = RMNodeLevel.PhysicalRecord;
                        break;
                    default:
                        break;
                }
                this.AppendPhyPersonalHoldInfo(tempList, accountMap);
                await this.AppendDisposalDueDateAndRuleInfoAsync(tempList, parentId, parentLevel);
                this.AppendPhyTermName(tempList);
                this.AppendPhyHomeLocationName(tempList);
            }
            return phyObj;
        }

        #region Append Physical Object Home Location Name
        private void AppendPhyHomeLocationName(List<PhysicalObjectDto> results)
        {
            var locationIds = results.Select(r => r.LocationId).Distinct().ToList();
            var locations = LocationDao.GetLocationByUniqueIds(locationIds);
            results.ForEach(r =>
            {
                r.LocationName = locations.Where(l => l.UniqueId.Equals(r.LocationId)).FirstOrDefault()?.Name;
            });
        }
        #endregion

        #region Append Physical object TermName
        private void AppendPhyTermName(List<PhysicalObjectDto> results)
        {
            var termIds = results.Select(r => r.TermId).Distinct().ToList();
            var terms = TermDao.GetRMTermsByTermIds(termIds);
            results.ForEach(r =>
            {
                r.TermName = terms.Where(term => term.UniqueId.Equals(r.TermId)).FirstOrDefault()?.Name;
            });
        }
        #endregion

        #region Deal with Due Date and Rule Info in Physical Record
        private async System.Threading.Tasks.Task AppendDisposalDueDateAndRuleInfoAsync(List<PhysicalObjectDto> records, Guid parentId, RMNodeLevel parentLevel)
        {
            long parentDueDate = 0;
            if (parentLevel == RMNodeLevel.PhysicalBox)
            {
                Record box = ExplorerDao.GetPhysicalRecordById(parentId);
                parentDueDate = box.HoldStatus ? box.DisposalDueDate : parentDueDate;
            }
            Dictionary<Guid, RMRule> daRulesDic = new Dictionary<Guid, RMRule>();
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (PhysicalObjectDto record in records)
            {
                AppendPhysicalRuleInfo(record, daRulesDic, gls);
                if (!string.IsNullOrEmpty(record.DisposalDueDate))
                {
                    CalculateDisposalDueDateNormal(record, gls, parentDueDate);
                }
            }
        }
        private void AppendPhysicalRuleInfo(PhysicalObjectDto record, Dictionary<Guid, RMRule> daRulesDic, GeneralSettingModel gls)
        {

            if (record.RuleId != Guid.Empty)
            {
                RMRule tempRule = null;
                if (!daRulesDic.TryGetValue(record.RuleId, out tempRule))
                {
                    tempRule = RMRuleDao.GetRuleById(record.RuleId);
                    if (tempRule != null)
                    {
                        daRulesDic.Add(tempRule.RuleId, tempRule);
                    }
                }
                AppendPhysicalRuleAction(record, tempRule);
                //if physical folder use parent box's hold and match remove rule, should set DisposalDueDate to HoldReleaseTime
                if (record.NodeType == RMNodeType.PhyFile && RuleHelper.GetOldLogicDisposalAction(record.RuleAction) == (int)RMContentDisposalAction.Remove && record.HoldStatus == HoldStatus.Inherit)
                {
                    record.DisposalDueDate = record.HoldReleaseTime.ToString();
                }
            }
            else
            {
                record.RuleAction = (int)RMContentDisposalAction.None;
            }
        }
        private void CalculateDisposalDueDateNormal(PhysicalObjectDto record, GeneralSettingModel gls, long parentDueDate)
        {
            long tempTicks;
            if (record.DisposalDueDate == "RDM_RecordsExporer_Status_NextJob" && parentDueDate > DateTime.UtcNow.Ticks)
            {
                record.DisposalDueDate = GeneralSettingService.ConvertTiksToDateTime(gls, parentDueDate, true).SimplifyFormatTime;
            }
            else if (long.TryParse(record.DisposalDueDate, out tempTicks))
            {
                tempTicks = tempTicks > parentDueDate ? tempTicks : parentDueDate;
                var minDate = DateTime.MinValue;
                if (tempTicks > minDate.Ticks)
                {
                    record.DisposalDueDate = this.GetDisposalDueDateStr(tempTicks, (RMRecordStatus)record.Status, gls);
                }
            }
            else
            {
                record.DisposalDueDate = I18NEntity.GetString(record.DisposalDueDate);
            }
        }
        private string GetDisposalDueDateStr(long dueDateLong, RMRecordStatus recordStatus, GeneralSettingModel gls, bool isForGUI = true)
        {
            return GeneralSettingService.ConvertTiksToDateTime(gls, dueDateLong, true).SimplifyFormatTime;
            ////RECO-4643 Destroyed状态的数据，due date 会显示destroyed 的时间，所以不会遵循： 与当前时间判断显示NextJob 的逻辑
            //if (dueDateLong > DateTime.UtcNow.Ticks || recordStatus == RMRecordStatus.Destroyed)
            //{
            //    return GeneralSettingService.ConvertTiksToDateTime(gls, dueDateLong, true).SimplifyFormatTime;
            //}
            //else
            //{
            //    if (isForGUI)
            //    {

            //        return I18NEntity.GetString("RDM_RecordsExporer_Status_NextJob");
            //    }
            //    else
            //    {
            //        return "RDM_RecordsExporer_Status_NextJob";
            //    }
            //}
        }
        private void AppendPhysicalRuleAction(PhysicalObjectDto record, RMRule tempRule)
        {
            if (tempRule != null)
            {
                record.RuleName = tempRule?.RuleName;
                record.RuleAction = record.SourceFlag == 4 ? (int)tempRule.PhysicalDisposalAction : (int)tempRule.DisposalAction;
            }
            else
            {
                record.RuleName = string.Empty;
                record.RuleAction = (int)RMContentDisposalAction.None;
            }
        }
        #endregion

        public async Task<string> GetPhysicalBoxPathByIdAsync(Guid id)
        {
            var result = string.Empty;
            try
            {
                var tempBox = await this.GetPhysicalObjectByIdAsync(id);
                if (tempBox.NodeType == RMNodeType.PhyBox && tempBox.Status == 1)
                {
                    if (tempBox.LocationId != null && tempBox.LocationId != Guid.Empty)
                    {
                        var tempLocation = LocationDao.GetLocationByUniqueId(tempBox.LocationId);
                        if (tempLocation != null)
                        {
                            result = string.Format($"{tempLocation.PathForDisplay}/{tempLocation.Name}/{tempBox.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalBoxPath by id: [{id}], error: [{ex.ToString()}]");
            }
            return result;
        }
        public string GetPhysicalObjectFullPath(Guid id, bool isReplaceI18NKey = true)
        {
            var path = new StringBuilder();
            try
            {
                var oPhy = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
                if (oPhy != null)
                {
                    path.Append(LocationManagementService.GetLocationPathById(oPhy.LocationId, isReplaceI18NKey));
                }

                if (oPhy.Ancestors != null) return oPhy.GetPhysicalLocationFullPathByAncestors(path.ToString(), ExplorerDao); //new format data

                //old format data
                if (oPhy.NodeType != (int)RMNodeType.PhyBox)
                {
                    if (oPhy.BoxId != Guid.Empty)
                    {
                        var parentBox = ExplorerDao.QueryAll(r => r.Id == oPhy.BoxId).FirstOrDefault();
                        path.Append($"/{parentBox?.LeafName}");
                    }
                    if (oPhy.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        var parentFile = ExplorerDao.QueryAll(r => r.Id == oPhy.FileId).FirstOrDefault();
                        path.Append($"/{parentFile?.LeafName}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalObject FullPath by id: [{id}], error: [{ex.ToString()}]");
            }
            return path.ToString();
        }

        public string GetPhysicalObjectFullPath(PhysicalObjectDto oPhy)
        {
            var path = new StringBuilder();
            try
            {
                if (oPhy != null)
                {
                    path.Append(LocationManagementService.GetLocationPathById(oPhy.LocationId));
                }
                if (oPhy.NodeType != RMNodeType.PhyBox)
                {
                    if (oPhy.BoxId != Guid.Empty)
                    {
                        var parentBox = ExplorerDao.QueryAll(r => r.Id == oPhy.BoxId).FirstOrDefault();
                        path.Append($"/{parentBox?.LeafName}");
                    }
                    if (oPhy.NodeType == RMNodeType.PhyRecord)
                    {
                        var parentFile = ExplorerDao.QueryAll(r => r.Id == oPhy.FileId).FirstOrDefault();
                        path.Append($"/{parentFile?.LeafName}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalObject FullPath, error: [{ex.ToString()}]");
            }
            return path.ToString();
        }

        public int GetSelectNodeAllChildCount(ExportBarcodeDto exportBarcodeDto)
        {
            long num = 0;
            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation)
            {
                PhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                GetSubBottomLocation(location, allSubBottomLocation);
                foreach (IPhysicalLocation bottomLocation in allSubBottomLocation)
                {
                    num += bottomLocation.GetBoxesCount(b => (b.RecordStatus != (int)RMRecordStatus.RMDeleted));
                    num += bottomLocation.GetFilesCount(f => (f.RecordStatus != (int)RMRecordStatus.RMDeleted));
                    if (num > 300)
                    {
                        break;
                    }
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                PhysicalLocation bottomLocation = new PhysicalLocation(exportBarcodeDto.NodeId);
                num += bottomLocation.GetBoxesCount(b => (b.RecordStatus != (int)RMRecordStatus.RMDeleted));
                num += bottomLocation.GetFilesCount(f => (f.RecordStatus != (int)RMRecordStatus.RMDeleted));
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                IPhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                num += box.GetFilesCount(f => (f.RecordStatus != (int)RMRecordStatus.RMDeleted));
            }
            if (num == 0)
            {
                return (int)BoxNumType.BoxAndFildIsZero;
            }
            else if (num <= 300)
            {
                return (int)BoxNumType.BoxAndFildIsZeroLessThan300;
            }
            else
            {
                return (int)BoxNumType.BoxAndFildIsZeroMoreThan300;
            }
        }

        public enum BoxNumType
        {
            BoxAndFildIsZero = 1,
            BoxAndFildIsZeroLessThan300 = 2,
            BoxAndFildIsZeroMoreThan300 = 3
        }

        //Add audit later
        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.DownLoadPhysicalExportBarcodeReport, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<ExportResultDto> ExportBarcodeAsync(ExportBarcodeDto exportBarcodeDto)
        {
            logger.Info($"Begin exporting barcode, export type");
            var result = new ExportResultDto();
            try
            {
                DateTime nowTime = DateTime.UtcNow;
                string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
                string fileName = I18NEntity.GetString("RM_DAM_ExportBarcodesReport") + "_" + GetSelectNodeName(exportBarcodeDto) + "_" + nowTimeStr;
                string folderPath = SecurityUtils.SafeCombinePath(JobReportUtility.GetDownloadBarcodeInfoReportTempleFolder("Temple"), fileName);
                await GenerateDownLoadReportDataInfoAsync(folderPath, fileName, exportBarcodeDto);
                logger.Info("Begin update to Internet browse.");
                AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
                result.FileContent = StreamUtl.ReadFile(folderPath + ".zip");
                result.FileName = $"{fileName}.zip";
                logger.Info("Finish export barcode");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("export barcode report error Info:{0},{1}", ex.Message, ex.StackTrace);
            }
            logger.Info("Finish exporting barcode.");
            return result;
        }

        public string GetSelectNodeName(ExportBarcodeDto exportBarcodeDto)
        {
            string selectNodeName = string.Empty;
            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation || exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                PhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                if (location.Name.Length > 50)
                {
                    selectNodeName = location.Name.Substring(0, 50);
                }
                else
                {
                    selectNodeName = location.Name;
                }

            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                if (box.Name.Length > 50)
                {
                    selectNodeName = box.Name.Substring(0, 50);
                }
                else
                {
                    selectNodeName = box.Name;
                }
            }
            return selectNodeName;
        }
        public async Task<RAReturnMessage> ExportBarcodeToLocationAsync(ExportBarcodeDto exportBarcodeDto)
        {
            logger.Info($"Begin exporting barcode to location, export type : {exportBarcodeDto.ExportType}.");
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                if (exportBarcodeDto != null && !string.IsNullOrWhiteSpace(exportBarcodeDto.ExportLocationId))
                {
                    var exportLocationDic = await GlobalSettingService.GetExportLocationTypesAsync();
                    Guid exportLocationId = Guid.Empty;
                    if (Guid.TryParse(exportBarcodeDto.ExportLocationId, out exportLocationId))
                    {
                        if (exportLocationDic.ContainsKey(exportLocationId) && exportLocationDic[exportLocationId] == 1)
                        {
                            returnMessage.MessageType = RAMessageType.Failed;
                            returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_CP_GSS_FTPExportLocationNotSupported");
                            return returnMessage;
                        }
                    }
                }
                this.AddJobToDBJobQueue(JobRunBy.Control, JobType.PhysicalExportBarcode, exportBarcodeDto);
                returnMessage.MessageType = RAMessageType.Successful;
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            logger.Info("Finish exporting barcode to location.");
            return returnMessage;

        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ExportSearchResult, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> ExportSearchResultAsync(GlobalSearchExportDto globalSearchExportDto)
        {
            ExportSearchResult export = new ExportSearchResult(globalSearchExportDto);
            return await export.ExportDirectlyAsync();
        }

        public bool IsPhysicalRecord(Guid id)
        {
            var record = ExplorerDao.GetRecordByIds(new List<Guid>() { id }).FirstOrDefault();
            return record != null && record.SourceFlag == (int)SourceFlag.Physical ? true : false;
        }

        public async Task<RAReturnMessage> StartExportSearchResultJobAsync(GlobalSearchExportDto globalSearchExportDto)
        {
            logger.Info($"Begin exporting search result to location, location name : {globalSearchExportDto.ExportLocationName}.");
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                globalSearchExportDto.UserId = TenantLocalValue.LogonUserId;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportSearchResult,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(globalSearchExportDto),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                mJobQueueService.AddToDBJobQueue(jqDto);
                returnMessage.MessageType = RAMessageType.Successful;
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            logger.Info("Finish exporting search result to location.");
            return returnMessage;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.PhysicalRecordsExplorer, Action = AuditAction.RunPhysicalExportBarcodeJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public string RealExportBarcode(JobRunBy JobRunType, string exportLocationId, string nodeId, string nodeType, string exportLocationName, string suiteId)
        {
            logger.Info($"Run RealExportBarcode");
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                jobId = RMJobService.CreateJob(JobType.PhysicalExportBarcode, jobRunByUser);
                //开发自行考虑要不要有job 冲突skip 逻辑
                //List<string> runningJobs = RMJobService.GetRunningJobs(JobType.PhysicalExportBarcode);
                //bool isSkip = runningJobs.Any(j => j != jobId);
                //if (!isSkip)
                //{
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = JobRunType,
                    JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExportBarcode,
                    //TODO Add export path in {2}
                    CommandLine = string.Format("{0} {1} {2} {3} {4} {5} {6}", AvePoint.RA.Contract.JobMonitor.JobType.PhysicalExportBarcode, jobId, exportLocationId, nodeId, nodeType, exportLocationName, suiteId),
                });
                logger.Info($"run physical Export Barcode job success, JobId : {jobId}.");
                //}
                //else
                //{
                //    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                //    logger.Info("Export Barcode job has job running, so shedule job is skip");
                //}
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealExportBarcode, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.RunExportSearchResultJob, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> RealRunExportSearchResultJobAsync(string parameter)
        {
            logger.Info($"RealRunExportSearchResultJobAsync start ... ");
            string jobId = string.Empty;
            try
            {

                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                jobId = RMJobService.CreateJob(JobType.ExportSearchResult, TenantLocalValue.LogonUserEmail, account.UserId);

                SubJobDao.UpdateSubJobCount(jobId, 1);

                var subJobId = CreateSubJob(jobId, 0, JobType.ExportSearchResult, JobStatus.InProgress, 1, parameter);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.ExportSearchRecords,
                });

                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = AvePoint.RA.Contract.JobMonitor.JobType.ExportSearchResult,
                    CommandLine = string.Format("{0} {1} {2}", AvePoint.RA.Contract.JobMonitor.JobType.ExportSearchResult, subJobId, jobId),
                });

                logger.Info($"RealRunExportSearchResultJobAsync End , JobId : {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunExportSearchResultJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }

        [Audit(Module = AuditModule.PhysicalRecordManagement, Category = AuditCategory.ManageHold, Action = AuditAction.ExportHoldRecords, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<string> RealRunExportHoldRecordsJobAsync(string parameter)
        {
            logger.Info("RealRunExportHoldRecordsJobAsync start.");
            string jobId = string.Empty;
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                jobId = RMJobService.CreateJob(JobType.ExportHoldRecords, TenantLocalValue.LogonUserEmail, account.UserId);

                SubJobDao.UpdateSubJobCount(jobId, 1);

                var subJobId = CreateSubJob(jobId, 0, JobType.ExportHoldRecords, JobStatus.InProgress, 1, parameter);
                var holdIds = SerializerHelper.DeserializeByDataContractSerializer<List<string>>(parameter);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.ExportHoldRecords,
                });

                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = JobType.ExportHoldRecords,
                    CommandLine = string.Format("{0} {1} {2} {3}", JobType.ExportHoldRecords, subJobId, jobId, string.Join(",", holdIds))
                });

                logger.Info($"RealRunExportHoldRecordsJobAsync end. JobId: {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealRunExportHoldRecordsJobAsync, reason: {ex}.");
            }
            return jobId;
        }

        /// <summary>
        /// RECO-20916 Fortify scan ,need validate folder path using SecurityUtils.SafeCombinePath to validate
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="exportBarcodeDto"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task GenerateDownLoadReportDataInfoAsync(string folderPath, string fileName, ExportBarcodeDto exportBarcodeDto)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var templateSuite = await CustomBarcodeTemplateSuiteDao.GetByUniqueIdAsync(exportBarcodeDto.SuiteId);
                if (templateSuite.IsDefault)
                {
                    logger.Info("Begin export default barcode report.");
                    await GetDownLoadReportDataInfoAsync(exportBarcodeDto, folderPath, fileName);
                }
                else
                {
                    logger.Info("Begin export custom barcode report.");
                    var gls = await GeneralSettingService.GetGeneralSettingAsync();
                    await this.GenerateCustomBarcodeLabelAsync(
                        exportBarcodeDto,
                        folderPath,
                        fileName,
                        templateSuite,
                        gls);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GenerateDownLoadReportDataInfo, message is : {ex.ToString()}.");
            }
        }

        public async Task GenerateCustomBarcodeLabelAsync(
            ExportBarcodeDto exportBarcodeDto,
            string folderPath,
            string fileName,
            RMCustomBarcodeTemplateSuite suite,
            GeneralSettingModel gls)
        {
            if (exportBarcodeDto == null) throw new ArgumentNullException(nameof(exportBarcodeDto));
            if (suite == null) throw new ArgumentNullException(nameof(suite));

            var labelType = suite.LabelType;
            var customTemplateInfo = await BarcodeTemplateService.GetBarcodeTemplateBySuiteIdAsync(exportBarcodeDto.SuiteId) as BarcodeCustomTemplateDto;
            var boxBarTemplate = customTemplateInfo?.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Box);
            var foldBarTemplate = customTemplateInfo?.Templates.FirstOrDefault(t => t.Type == BarcodeTemplateType.Folder);
            if (customTemplateInfo == null)
            {
                logger.Error("Custom template suite not found or invalid.");
                return;
            }

            LabelItem BuildLabelItem(PhysicalObjectDto node, RMTemplate contentTemplate)
            {
                var li = new LabelItem
                {
                    Barcode = string.IsNullOrEmpty(node.BarcodeId) ? node.UniqueId : node.BarcodeId,
                    Properties = new List<PropertyItem>()
                };
                var cfg = (node.NodeType == RMNodeType.PhyBox) ? boxBarTemplate : foldBarTemplate;
                if (cfg != null)
                {
                    // Logo
                    if (cfg.LogoProperties != null && !string.IsNullOrEmpty(cfg.LogoProperties.LogoImgBase64Str))
                    {
                        cfg.LogoProperties.LogoImgBase64Str = cfg.LogoProperties.LogoImgBase64Str[(cfg.LogoProperties.LogoImgBase64Str.IndexOf(",") + 1)..];
                        var isEnableLogo = !string.IsNullOrWhiteSpace(cfg.LogoProperties.LogoImgBase64Str);
                        var imageBytes = new byte[0];
                        var width = 50;
                        var height = 50;

                        if (isEnableLogo)
                        {
                            imageBytes = Convert.FromBase64String(cfg.LogoProperties.LogoImgBase64Str);
                            var imageInfo = BarcodeUtil.GetImageInfo(imageBytes);
                            width = imageInfo.Width > 0 ? imageInfo.Width : 50;
                            height = imageInfo.Height > 0 ? imageInfo.Height : 50;
                        }

                        li.Logo = new LogoItem
                        {
                            Enabled = isEnableLogo,
                            ImageBytes = imageBytes,
                            Mime = string.IsNullOrWhiteSpace(cfg.LogoProperties.LogoImgType) ? "image/png" : cfg.LogoProperties.LogoImgType,
                            FileName = string.IsNullOrWhiteSpace(cfg.LogoProperties.LogoImgName) ? "logo" : cfg.LogoProperties.LogoImgName,
                            Position = cfg.LogoProperties.Position,
                            Width = width,
                            Height = height
                        };
                    }
                    // Properties
                    if (cfg.Properties != null && cfg.Properties.Count > 0)
                    {
                        foreach (var p in cfg.Properties)
                        {
                            string value = GetPropertyValueByName(node, contentTemplate, p.Name, gls);
                            if(string.IsNullOrEmpty(value))
                            {
                                continue;
                            }
                            var pi = new PropertyItem
                            {
                                Name = AvePoint.RA.I18N.Core.I18NEntity.GetString(p.Name),
                                Value = value ?? string.Empty,
                                Position = p.Position,
                                FontSize = p.FontSize > 0 ? p.FontSize * 2 : (int?)null
                            };
                            li.Properties.Add(pi);
                        }
                    }
                }
                return li;
            }

            string vmlTemplatePath = GetTemplatePath(labelType);

            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation)
            {
                IPhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                // 递归获取所有底层location
                GetSubBottomLocation(location, allSubBottomLocation);
                foreach (IPhysicalLocation bottomLocation in allSubBottomLocation)
                {
                    string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + "_" + bottomLocation.Name + ".docx";
                    var records = GetBoxesAndFoldOrderByDescending(bottomLocation) ?? new List<Record>();
                    var labels = new List<LabelItem>();
                    foreach (var r in records)
                    {
                        if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                        {
                            PhysicalBox box = new PhysicalBox(r);
                            PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                            boxDto.HomeLocationFullPath = GetPhysicalObjectFullPath(boxDto.Id);
                            RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                            if (boxTemplate != null)
                                labels.Add(BuildLabelItem(boxDto, boxTemplate));
                        }
                        else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                        {
                            PhysicalFile file = new PhysicalFile(r);
                            PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(file.Record);
                            foldDto.HomeLocationFullPath = GetPhysicalObjectFullPath(foldDto.Id);
                            RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldDto.TemplateId);
                            if (foldTemplate != null)
                                labels.Add(BuildLabelItem(foldDto, foldTemplate));
                        }
                    }
                    ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                IPhysicalLocation bottomLocation = new PhysicalLocation(exportBarcodeDto.NodeId);
                var records = GetBoxesAndFoldOrderByDescending(bottomLocation) ?? new List<Record>();
                var labels = new List<LabelItem>();
                foreach (var r in records)
                {
                    if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        PhysicalBox box = new PhysicalBox(r);
                        PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                        boxDto.HomeLocationFullPath = GetPhysicalObjectFullPath(boxDto.Id);
                        RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                        if (boxTemplate != null)
                        {
                            labels.Add(BuildLabelItem(boxDto, boxTemplate));
                        }
                        List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
                        var foldTemplateMap = new Dictionary<int, RMTemplate>();
                        foreach (var f in folds)
                        {
                            PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(f.Record);
                            foldDto.BoxTemplateId = box.TemplateId;
                            foldDto.HomeLocationFullPath = GetPhysicalObjectFullPath(foldDto.Id);
                            if (!foldTemplateMap.ContainsKey(foldDto.TemplateId))
                            {
                                var ft = TemplateDao.GetTemplateById(foldDto.TemplateId);
                                if (ft != null)
                                {
                                    AddPushColumnToFoldTemplate(ft, boxTemplate);
                                    foldTemplateMap[foldDto.TemplateId] = ft;
                                }
                            }
                            if (foldTemplateMap.TryGetValue(foldDto.TemplateId, out var useTpl))
                            {
                                labels.Add(BuildLabelItem(foldDto, useTpl));
                            }
                        }
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        PhysicalFile file = new PhysicalFile(r);
                        PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(file.Record);
                        foldDto.HomeLocationFullPath = GetPhysicalObjectFullPath(foldDto.Id);
                        RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldDto.TemplateId);
                        if (foldTemplate != null)
                        {
                            labels.Add(BuildLabelItem(foldDto, foldTemplate));
                        }
                    }
                }
                ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                var labels = new List<LabelItem>();
                PhysicalObjectDto boxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
                boxDto.HomeLocationFullPath = GetPhysicalObjectFullPath(boxDto.Id);
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(boxDto.TemplateId);
                if (boxTemplate != null)
                {
                    labels.Add(BuildLabelItem(boxDto, boxTemplate));
                }
                List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
                var foldTemplateMap = new Dictionary<int, RMTemplate>();
                foreach (var f in folds)
                {
                    PhysicalObjectDto foldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(f.Record);
                    foldDto.BoxTemplateId = box.TemplateId;
                    foldDto.HomeLocationFullPath = GetPhysicalObjectFullPath(foldDto.Id);
                    if (!foldTemplateMap.ContainsKey(foldDto.TemplateId))
                    {
                        var ft = TemplateDao.GetTemplateById(foldDto.TemplateId);
                        if (ft != null)
                        {
                            AddPushColumnToFoldTemplate(ft, boxTemplate);
                            foldTemplateMap[foldDto.TemplateId] = ft;
                        }
                    }
                    if (foldTemplateMap.TryGetValue(foldDto.TemplateId, out var useTpl))
                    {
                        labels.Add(BuildLabelItem(foldDto, useTpl));
                    }
                }
                ReportWordUtil.CopyTemplateAndFillVml(vmlTemplatePath, reportFilePath, labels);
            }
        }

        // Helper: Map property name value for a node based on template schema and general settings
        private string GetPropertyValueByName(PhysicalObjectDto node, RMTemplate template, string propName, GeneralSettingModel gls)
        {
            if (node == null || template == null || string.IsNullOrWhiteSpace(propName)) return string.Empty;

            // Built-in mappings
            if (propName == BuildInColumnIDs.RecordsId)
            {
                return node.UniqueId ?? string.Empty;
            }
            if (propName == BuildInColumnIDs.CreatedBy) return node.CreatedBy;
            if (propName == BuildInColumnIDs.CreatedTime) return DateTimeUtil.ConvertTimeFromUtc(node.CreateTime, gls).ToString();
            if (propName == BuildInColumnIDs.ModifiedBy) return node.ModifiedBy;
            if (propName == BuildInColumnIDs.ModifiedTime) return DateTimeUtil.ConvertTimeFromUtc(node.ModifiedTime, gls).ToString();

            // Meta columns by display name
            var schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(template.ColumnSchema);
            var column = schema?.Columns?.FirstOrDefault(c => string.Equals(c.Name, propName, StringComparison.OrdinalIgnoreCase));
            if (column != null)
            {
                var metaInfo = node.MetaInfo;
                if (metaInfo != null && metaInfo.ContainsKey(column.UniqueId.ToString()))
                {
                    return HandleMetaInfoColumn(column, node, gls);
                }
            }
            return string.Empty;
        }

        // Helper: Get template path for barcode label type
        private static string GetTemplatePath(BarcodeTemplateLabelType labelType)
        {
            return labelType switch
            {
                BarcodeTemplateLabelType.Label_200x93 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_200x93-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_135x95 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_135x95-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_95x65 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_95x65-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_99x67 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_99x67-R_Word_Template.docx"),
                BarcodeTemplateLabelType.Label_72x63 => Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "Label_72x63-R_Word_Template.docx"),
                _ => throw new ArgumentException($"Unsupported label type: {labelType}"),
            };
        }

        public async System.Threading.Tasks.Task GetDownLoadReportDataInfoAsync(ExportBarcodeDto exportBarcodeDto, string folderPath, string fileName)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            int boxWidth = 0, boxHeight = 0, foldWidth = 0, foldHeight = 0;
            var boxBarTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Box);
            var foldBarTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Folder);
            try
            {
                if (boxBarTemplate != null && !boxBarTemplate.ImageColumnA.IsNullOrEmpty())
                {
                    var bi = BarcodeUtil.GetImageInfo(boxBarTemplate.ImageColumnA);
                    boxWidth = bi.Width;
                    boxHeight = bi.Height;
                }
                if (foldBarTemplate != null && !foldBarTemplate.ImageColumnA.IsNullOrEmpty())
                {
                    var bi = BarcodeUtil.GetImageInfo(foldBarTemplate.ImageColumnA);
                    foldWidth = bi.Width;
                    foldHeight = bi.Height;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get bitmap size , message is : {ex.Message.ToString()}.");
            }
            //如果为空  应该在word给出提示或者前台给出提示
            logger.Info("Begin get download data.");
            List<string[]> sheetDatasInfo = new List<string[]>();
            if (exportBarcodeDto.NodeType == RMNodeType.PhysicalNormalLocation)
            {
                IPhysicalLocation location = new PhysicalLocation(exportBarcodeDto.NodeId);
                List<IPhysicalLocation> allSubBottomLocation = new List<IPhysicalLocation>();
                GetSubBottomLocation(location, allSubBottomLocation);
                var templatePath = Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "BarcodeTemplate.docx");
                foreach (IPhysicalLocation bottomLocation in allSubBottomLocation)
                {
                    string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + "_" + bottomLocation.Name + ".docx";
                    ReportWordUtil.CopyFile(templatePath, reportFilePath);
                    List<ExportBarcodeDataModel> models = new List<ExportBarcodeDataModel>();
                    GetBoxesAndFoldOrderByDescending(bottomLocation)?.ForEach(r =>
                    {
                        if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                        {
                            PhysicalBox box = new PhysicalBox(r);
                            List<ExportBarcodeDataModel> boxDatas = GetBoxExportValue(box, boxBarTemplate, foldBarTemplate, gls);
                            models.AddRange(boxDatas);
                        }
                        else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                        {
                            PhysicalFile file = new PhysicalFile(r);
                            ExportBarcodeDataModel foldData = GetFoldExportValue(file, foldBarTemplate, gls);
                            models.Add(foldData);
                        }
                    });
                    using (ReportWordUtil utility = new ReportWordUtil(reportFilePath))
                    {
                        models.ForEach(m =>
                        {
                            if (m.NodeType == RMNodeType.PhyBox)
                            {
                                m.ImageWidth = boxWidth;
                                m.ImageHeight = boxHeight;
                            }
                            else if (m.NodeType == RMNodeType.PhyFile)
                            {
                                m.ImageWidth = foldWidth;
                                m.ImageHeight = foldHeight;
                            }
                        });
                        utility.CreateTable("Table", models);
                    }
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhysicalBottomLocation)
            {
                var templatePath = Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "BarcodeTemplate.docx");
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                ReportWordUtil.CopyFile(templatePath, reportFilePath);
                List<ExportBarcodeDataModel> models = new List<ExportBarcodeDataModel>();
                IPhysicalLocation bottomLocation = new PhysicalLocation(exportBarcodeDto.NodeId);
                GetBoxesAndFoldOrderByDescending(bottomLocation)?.ForEach(r =>
                {
                    if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                    {
                        PhysicalBox box = new PhysicalBox(r);
                        List<ExportBarcodeDataModel> boxDatas = GetBoxExportValue(box, boxBarTemplate, foldBarTemplate, gls);
                        models.AddRange(boxDatas);
                    }
                    else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                    {
                        PhysicalFile file = new PhysicalFile(r);
                        ExportBarcodeDataModel foldData = GetFoldExportValue(file, foldBarTemplate, gls);
                        models.Add(foldData);
                    }
                });
                using (ReportWordUtil utility = new ReportWordUtil(reportFilePath))
                {
                    models.ForEach(m =>
                    {
                        if (m.NodeType == RMNodeType.PhyBox)
                        {
                            m.ImageWidth = boxWidth;
                            m.ImageHeight = boxHeight;
                        }
                        else if (m.NodeType == RMNodeType.PhyFile)
                        {
                            m.ImageWidth = foldWidth;
                            m.ImageHeight = foldHeight;
                        }
                    });
                    utility.CreateTable("Table", models);
                }
            }
            else if (exportBarcodeDto.NodeType == RMNodeType.PhyBox)
            {
                PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
                var templatePath = Path.Combine(WebUtil.GetInstallPath(), "Config", "BarcodeTemplate", "BarcodeTemplate.docx");
                string reportFilePath = folderPath + Path.DirectorySeparatorChar + fileName + ".docx";
                ReportWordUtil.CopyFile(templatePath, reportFilePath);
                using (ReportWordUtil utility = new ReportWordUtil(reportFilePath))
                {
                    List<ExportBarcodeDataModel> models = GetBoxExportValue(box, boxBarTemplate, foldBarTemplate, gls);
                    models.ForEach(m =>
                    {
                        if (m.NodeType == RMNodeType.PhyBox)
                        {
                            m.ImageWidth = boxWidth;
                            m.ImageHeight = boxHeight;
                        }
                        else if (m.NodeType == RMNodeType.PhyFile)
                        {
                            m.ImageWidth = foldWidth;
                            m.ImageHeight = foldHeight;
                        }
                    });
                    utility.CreateTable("Table", models);
                }
            }
        }

        public List<ExportBarcodeDataModel> GetBoxExportValue(PhysicalBox box, RMBarcodeTemplate boxBarTemplate, RMBarcodeTemplate foldBarTemplate, GeneralSettingModel gls)
        {
            List<ExportBarcodeDataModel> models = new List<ExportBarcodeDataModel>();
            //这里得判断 BarTemplate 如果为空会怎样
            List<int> foldTemplateIds = new List<int>();
            Dictionary<int, RMTemplate> idAndTemplate = new Dictionary<int, RMTemplate>();

            List<PhysicalObjectDto> objectList = new List<PhysicalObjectDto>();
            //PhysicalBox box = new PhysicalBox(exportBarcodeDto.NodeId);
            PhysicalObjectDto physicalBoxDto = ConvertUtil.ConvertRMBaseRecordToPhysical(box.Record);
            physicalBoxDto.HomeLocationFullPath = GetPhysicalObjectFullPath(physicalBoxDto.Id);
            objectList.Add(physicalBoxDto);
            RMTemplate boxTemplate = TemplateDao.GetTemplateById(physicalBoxDto.TemplateId);
            if (boxTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                return models;
            }

            List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
            foreach (PhysicalFile fold in folds)
            {
                PhysicalObjectDto foldObject = ConvertUtil.ConvertRMBaseRecordToPhysical(fold.Record);
                foldObject.BoxTemplateId = box.TemplateId;
                foldObject.HomeLocationFullPath = GetPhysicalObjectFullPath(foldObject.Id);
                objectList.Add(foldObject);
                if (!foldTemplateIds.Contains(foldObject.TemplateId))
                {
                    foldTemplateIds.Add(foldObject.TemplateId);
                }
            }
            this.AppendPushedColumns(objectList);

            foreach (int foldTemplateId in foldTemplateIds)
            {
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(foldTemplateId);
                if (foldTemplate == null)
                {
                    logger.Error("Can't find fold's template ,template id is {0}", foldTemplateId.ToString());
                    continue;
                }
                AddPushColumnToFoldTemplate(foldTemplate, boxTemplate);
                idAndTemplate[foldTemplateId] = foldTemplate;
            }

            foreach (PhysicalObjectDto node in objectList)
            {
                if (node.NodeType == RMNodeType.PhyBox)
                {
                    ExportBarcodeDataModel model = GetColumnValue(node, boxTemplate, boxBarTemplate, gls);
                    model.Image = boxBarTemplate == null ? null : boxBarTemplate.ImageColumnA;
                    model.NodeType = RMNodeType.PhyBox;
                    models.Add(model);
                }
                else
                {
                    ExportBarcodeDataModel model = GetColumnValue(node, idAndTemplate[node.TemplateId], foldBarTemplate, gls);
                    model.Image = foldBarTemplate == null ? null : foldBarTemplate.ImageColumnA;
                    model.NodeType = RMNodeType.PhyFile;
                    models.Add(model);
                }
            }
            return models;
        }

        public ExportBarcodeDataModel GetFoldExportValue(PhysicalFile fold, RMBarcodeTemplate foldBarTemplate, GeneralSettingModel gls)
        {
            ExportBarcodeDataModel model = new ExportBarcodeDataModel();
            //这里得判断 BarTemplate 如果为空会怎样

            //List<PhysicalObjectDto> objectList = new List<PhysicalObjectDto>();
            PhysicalObjectDto physicalFoldDto = ConvertUtil.ConvertRMBaseRecordToPhysical(fold.Record);
            physicalFoldDto.HomeLocationFullPath = GetPhysicalObjectFullPath(physicalFoldDto.Id);
            //objectList.Add(physicalFoldDto);
            RMTemplate foldTemplate = TemplateDao.GetTemplateById(physicalFoldDto.TemplateId);
            if (foldTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", physicalFoldDto.TemplateId.ToString());
                return model;
            }
            model = GetColumnValue(physicalFoldDto, foldTemplate, foldBarTemplate, gls);
            model.Image = foldBarTemplate == null ? null : foldBarTemplate.ImageColumnA;
            model.NodeType = RMNodeType.PhyFile;
            return model;
        }

        public void AddPushColumnToFoldTemplate(RMTemplate foldTemplate, RMTemplate boxTemplate)
        {
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
            foreach (ColumnXmlSchema column in schemaTemp.Columns)
            {
                if ((column.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    if (column.pushFoldTemplateCategoriesId == null)
                    {
                        continue;
                    }
                    foreach (TemplateIdAndCategoryId temp in column.pushFoldTemplateCategoriesId)
                    {
                        if (temp.tempalteId == foldTemplate.UniqueId.ToString())
                        {
                            var foldSchemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(foldTemplate.ColumnSchema);
                            foldSchemaTemp.Columns.Add(column);
                            foldTemplate.ColumnSchema = SerializerHelper.SerializeByDataContractSerializer(foldSchemaTemp);
                            break;
                        }
                    }
                }
            }
        }

        public ExportBarcodeDataModel GetColumnValue(PhysicalObjectDto node, RMTemplate templte, RMBarcodeTemplate barcodeTemplate, GeneralSettingModel gls)
        {

            ExportBarcodeDataModel model = new ExportBarcodeDataModel();
            if (node == null || templte == null || barcodeTemplate == null)
            {
                return model;
            }
            string ColumnB = barcodeTemplate.ColumnB;
            string ColumnC = barcodeTemplate.ColumnC;
            string ColumnE = barcodeTemplate.ColumnE;
            string ColumnF = barcodeTemplate.ColumnF;
            Dictionary<string, string> dvalueDic = new Dictionary<string, string>();
            List<string> defaultColumnIds = GetDefaultColumnIds();
            foreach (string defaultColumnId in defaultColumnIds)
            {
                string result = "";
                if (defaultColumnId == BuildInColumnIDs.RecordsId)
                {
                    result = string.IsNullOrEmpty(node.BarcodeId) ? node.UniqueId : node.BarcodeId;
                }
                else if (defaultColumnId == BuildInColumnIDs.CreatedBy)
                {
                    result = node.CreatedBy;
                }
                else if (defaultColumnId == BuildInColumnIDs.CreatedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(node.CreateTime, gls).ToString();
                }
                else if (defaultColumnId == BuildInColumnIDs.ModifiedBy)
                {
                    result = node.ModifiedBy;
                }
                else if (defaultColumnId == BuildInColumnIDs.ModifiedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(node.ModifiedTime, gls).ToString();
                }
                if (ColumnB == defaultColumnId)
                {
                    model.ColumnB = result;
                }
                if (ColumnC == defaultColumnId)
                {
                    model.ColumnC = result;
                }
                if (barcodeTemplate.ColumnDList != null)
                {
                    foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                    {
                        if (dcolumnName == defaultColumnId)
                        {
                            if (!dvalueDic.ContainsKey(dcolumnName))
                            {
                                dvalueDic.Add(dcolumnName, result);
                            }
                        }
                    }
                }
                if (ColumnE == defaultColumnId)
                {
                    model.ColumnE = result;
                }
                if (ColumnF == defaultColumnId)
                {
                    model.ColumnF = result;
                }
            }

            Dictionary<string, string> metaInfo = node.MetaInfo;
            var schemaTemp = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(templte.ColumnSchema);
            foreach (ColumnXmlSchema column in schemaTemp.Columns)
            {
                if (ColumnB == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnB = result;
                    }
                }
                if (ColumnC == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnC = result;
                    }

                }
                if (barcodeTemplate.ColumnDList != null)
                {
                    foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                    {
                        if (dcolumnName == column.Name)
                        {
                            if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                            {
                                string result = HandleMetaInfoColumn(column, node, gls);
                                if (!dvalueDic.ContainsKey(dcolumnName))
                                {
                                    dvalueDic.Add(dcolumnName, result);
                                }
                            }
                        }
                    }
                }
                if (ColumnE == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnE = result;
                    }
                }
                if (ColumnF == column.Name)
                {
                    if (metaInfo.ContainsKey(column.UniqueId.ToString()))
                    {
                        string result = HandleMetaInfoColumn(column, node, gls);
                        model.ColumnF = result;
                    }
                }
            }
            model.ColumnDValue = dvalueDic;
            model.UniqueId = node.UniqueId;
            model.Barcode = string.IsNullOrEmpty(node.BarcodeId) ? node.UniqueId : node.BarcodeId;
            return model;
        }

        public string HandleMetaInfoColumn(ColumnXmlSchema column, PhysicalObjectDto node, GeneralSettingModel gls)
        {
            string result = "";
            Dictionary<string, string> metaInfo = node.MetaInfo;
            if (column.UniqueId.ToString() == DefaultColumnIDs.Classification
                || column.UniqueId.ToString() == DefaultColumnIDs.Status
                || column.UniqueId.ToString() == DefaultColumnIDs.Format
                || column.UniqueId.ToString() == DefaultColumnIDs.ProtectiveMarking)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.UniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.UniqueId.ToString() == DefaultColumnIDs.HomeLocation)
            {//home location
                result = node.HomeLocationFullPath;
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<DateTimeColumnValue>(metaInfo[column.UniqueId.ToString()]);
                    if (field.TimeZoneId == gls.TimeZoneId && field.IsSetDayLight == gls.DayLight)
                    {
                        result = field.Date.ToString();
                    }
                    else
                    {
                        var columnUTCDate = field.GetUtcDate();
                        var glsTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
                        var glsTimeZoneDateTime = DateTimeUtil.ConvertTimeFromUtc(columnUTCDate, gls);
                        if (glsTimeZoneDateTime.Kind == DateTimeKind.Utc)
                        {
                            glsTimeZoneDateTime = DateTime.SpecifyKind(glsTimeZoneDateTime, DateTimeKind.Unspecified);
                        }
                        result = glsTimeZoneDateTime.ToString();
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.UniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(metaInfo[column.UniqueId.ToString()]);
                    foreach (ChoiceColumnValue temp in choices)
                    {
                        result += temp.Name + ';';
                    }
                }
            }
            else if (column.ColumnType == AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup)
            {
                if (metaInfo[column.UniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<List<PeopleColumnValue>>(metaInfo[column.UniqueId.ToString()]);
                    result = string.Join(";", field.Select(f => f.DisplayName.Trim()).ToList()).Trim(';');
                }
            }
            else
            {
                result = metaInfo[column.UniqueId.ToString()];
            }
            return result;
        }

        public List<string> GetDefaultColumnIds()
        {
            List<string> defaultColumnIds = new List<string>();
            defaultColumnIds.Add(BuildInColumnIDs.RecordsId);
            defaultColumnIds.Add(BuildInColumnIDs.CreatedBy);
            defaultColumnIds.Add(BuildInColumnIDs.CreatedTime);
            defaultColumnIds.Add(BuildInColumnIDs.ModifiedBy);
            defaultColumnIds.Add(BuildInColumnIDs.ModifiedTime);
            return defaultColumnIds;
        }

        public async System.Threading.Tasks.Task GetPhysicalBarcodeInfoAsync(PhysicalObjectDto dto)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            TemplateDto templateDto = dto.Template;
            RMBarcodeTemplate barcodeTemplate = new RMBarcodeTemplate();
            if (templateDto == null)
            {
                logger.Error("The template dto is null");
                return;
            }
            if (dto.NodeType == RMNodeType.PhyBox)
            {
                bool exist = await CustomBarcodeTemplateDao.CheckDefaultBarcodeTemplateExistByTypeAsync(BarcodeTemplateType.Box);
                if (!exist)
                {
                    return;
                }
                barcodeTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Box);
            }
            else
            {
                bool exist = await CustomBarcodeTemplateDao.CheckDefaultBarcodeTemplateExistByTypeAsync(BarcodeTemplateType.Folder);
                if (!exist)
                {
                    return;
                }
                barcodeTemplate = await CustomBarcodeTemplateDao.GetDefaultTemplateAsync(BarcodeTemplateType.Folder);
            }
            if (barcodeTemplate.ImageColumnA != null)
            {
                dto.ImageBase64Str = barcodeTemplate.Prefix + Convert.ToBase64String(barcodeTemplate.ImageColumnA);
            }

            string ColumnB = barcodeTemplate.ColumnB;
            string ColumnC = barcodeTemplate.ColumnC;
            string ColumnE = barcodeTemplate.ColumnE;
            string ColumnF = barcodeTemplate.ColumnF;
            Dictionary<string, string> dvalueDic = new Dictionary<string, string>();

            List<string> defaultColumnIds = GetDefaultColumnIds();
            foreach (string defaultColumnId in defaultColumnIds)
            {
                string result = "";
                if (defaultColumnId == BuildInColumnIDs.RecordsId)
                {
                    result = dto.UniqueId;
                }
                else if (defaultColumnId == BuildInColumnIDs.CreatedBy)
                {
                    result = dto.CreatedBy;
                }
                else if (defaultColumnId == BuildInColumnIDs.CreatedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(dto.CreateTime, gls).ToString();
                }
                else if (defaultColumnId == BuildInColumnIDs.ModifiedBy)
                {
                    result = dto.ModifiedBy;
                }
                else if (defaultColumnId == BuildInColumnIDs.ModifiedTime)
                {
                    result = DateTimeUtil.ConvertTimeFromUtc(dto.ModifiedTime, gls).ToString();
                }
                if (ColumnB == defaultColumnId)
                {
                    dto.ColumnB = result;
                }
                if (ColumnC == defaultColumnId)
                {
                    dto.ColumnC = result;
                }
                if (barcodeTemplate.ColumnDList != null)
                {
                    foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                    {
                        if (dcolumnName == defaultColumnId)
                        {
                            if (!dvalueDic.ContainsKey(dcolumnName))
                            {
                                dvalueDic.Add(dcolumnName, result);
                            }
                        }
                    }
                }
                if (ColumnE == defaultColumnId)
                {
                    dto.ColumnE = result;
                }
                if (ColumnF == defaultColumnId)
                {
                    dto.ColumnF = result;
                }
            }

            Dictionary<string, string> metaInfo = dto.MetaInfo;
            foreach (TemplateCategoryDto category in templateDto.categories)
            {
                foreach (TemplateColumnDto columnDto in category.columns)
                {
                    if (ColumnB == columnDto.columnName)
                    {
                        if (metaInfo.ContainsKey(columnDto.uniqueId.ToString()))
                        {
                            string result = HandleMetaInfoColumn(columnDto, dto);
                            dto.ColumnB = result;
                        }
                    }
                    if (ColumnC == columnDto.columnName)
                    {
                        if (metaInfo.ContainsKey(columnDto.uniqueId.ToString()))
                        {
                            string result = HandleMetaInfoColumn(columnDto, dto);
                            dto.ColumnC = result;
                        }
                    }
                    if (barcodeTemplate.ColumnDList != null)
                    {
                        foreach (string dcolumnName in barcodeTemplate.ColumnDList)
                        {
                            if (dcolumnName == columnDto.columnName)
                            {
                                if (metaInfo.ContainsKey(columnDto.uniqueId.ToString()))
                                {
                                    string result = HandleMetaInfoColumn(columnDto, dto);
                                    if (!dvalueDic.ContainsKey(dcolumnName))
                                    {
                                        dvalueDic.Add(dcolumnName, result);
                                    }
                                }
                            }
                        }
                    }
                    if (ColumnE == columnDto.columnName)
                    {
                        if (metaInfo.ContainsKey(columnDto.uniqueId.ToString()))
                        {
                            string result = HandleMetaInfoColumn(columnDto, dto);
                            dto.ColumnE = result;
                        }
                    }
                    if (ColumnF == columnDto.columnName)
                    {
                        if (metaInfo.ContainsKey(columnDto.uniqueId.ToString()))
                        {
                            string result = HandleMetaInfoColumn(columnDto, dto);
                            dto.ColumnF = result;
                        }
                    }
                }
            }
            dto.ColumnD = dvalueDic;
        }

        public string HandleMetaInfoColumn(TemplateColumnDto column, PhysicalObjectDto node)
        {
            string result = "";
            Dictionary<string, string> metaInfo = node.MetaInfo;
            if (column.uniqueId.ToString() == DefaultColumnIDs.Classification
                || column.uniqueId.ToString() == DefaultColumnIDs.Status
                || column.uniqueId.ToString() == DefaultColumnIDs.Format
                || column.uniqueId.ToString() == DefaultColumnIDs.ProtectiveMarking)
            {
                if (metaInfo[column.uniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.uniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.uniqueId.ToString() == DefaultColumnIDs.HomeLocation)
            {//home location
                result = node.HomeLocationFullPath;
            }
            else if (column.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
            {
                if (metaInfo[column.uniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<DateTimeColumnValue>(metaInfo[column.uniqueId.ToString()]);
                    result = field.Date.ToString();
                }
            }
            else if (column.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.SingleChoice)
            {
                if (metaInfo[column.uniqueId.ToString()] != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(metaInfo[column.uniqueId.ToString()]);
                    if (dic.ContainsKey("Name"))
                    {
                        result = dic["Name"];
                    }
                }
            }
            else if (column.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.MultipleChoice)
            {
                if (metaInfo[column.uniqueId.ToString()] != null)
                {
                    List<ChoiceColumnValue> choices = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(metaInfo[column.uniqueId.ToString()]);
                    foreach (ChoiceColumnValue temp in choices)
                    {
                        result += temp.Name + ';';
                    }
                }
            }
            else if (column.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.PeopleOrGroup)
            {
                if (metaInfo[column.uniqueId.ToString()] != null)
                {
                    var field = JsonConvert.DeserializeObject<List<PeopleColumnValue>>(metaInfo[column.uniqueId.ToString()]);
                    result = string.Join(";", field.Select(f => f.DisplayName.Trim()).ToList()).Trim(';');
                }
            }
            else
            {
                result = metaInfo[column.uniqueId.ToString()];
            }
            return result;
        }

        public void GetSubBottomLocation(IPhysicalLocation location, List<IPhysicalLocation> allSubBottomLocation)
        {
            List<IPhysicalLocation> subLocations = location.AllSubLocations;
            foreach (IPhysicalLocation subLocation in subLocations)
            {
                if (subLocation.IsBottomLocation)
                {
                    allSubBottomLocation.Add(subLocation);
                }
                else
                {
                    GetSubBottomLocation(subLocation, allSubBottomLocation);
                }
            }
        }

        public void ConvertDataToArrayForBottomLocation(IPhysicalLocation bottomlocation, List<string[]> sheetDatasInfo)
        {
            //GetBoxes(bottomlocation)?.ForEach(b => ProcessBox(b, sheetDatasInfo));
            //GetFiles(bottomlocation)?.ForEach(f => ProcessFile(f, sheetDatasInfo));
            GetBoxesAndFoldOrderByDescending(bottomlocation)?.ForEach(r =>
            {
                if (r.NodeType == (int)RMNodeLevel.PhysicalBox)
                {
                    PhysicalBox box = new PhysicalBox(r);
                    ProcessBox(box, sheetDatasInfo);
                }
                else if (r.NodeType == (int)RMNodeLevel.PhysicalFile)
                {
                    PhysicalFile file = new PhysicalFile(r);
                    ProcessFile(file, sheetDatasInfo);
                }
            });
        }
        public void ProcessBox(IPhysicalBox box, List<string[]> sheetDatasInfo)
        {
            ConvertBoxAndFoldBarcordInfoToArray(box, sheetDatasInfo);
        }


        public void ProcessFile(IPhysicalFile file, List<string[]> sheetDatasInfo)
        {
            ConvertFoldBarcordInfoToArray(file, sheetDatasInfo);
        }

        private List<Record> GetBoxesAndFoldOrderByDescending(IPhysicalLocation location)
        {
            return location.GetBoxesAndFoldOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed
          || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
        }

        public async Task<List<LocationPermissionDto>> GetEffectiveLocationPermissionsAsync()
        {
            var isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            var isPhysicalEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalEndUser);

            var adminLocationIds = SecurityGroupDao.GetScopeLocationPermission(SourceFlag.Physical);
            return LocationDao.GetLocationUniqueIds()
                .Select(locationId => new LocationPermissionDto
                {
                    LocationId = locationId.ToString(),
                    IsHoldManager = true,
                    IsPhysicalEndUser = isPhysicalEndUser,
                    IsPhysicalAdmin = isPhysicalAdmin && adminLocationIds.Contains(locationId)
                })
                .ToList();
        }

        public async Task<List<RecordPermissionDto>> GetRecordsPermission(List<ExplorerRecordPermission> recordPermission)
        {
            try
            {
                var defaultContianerIdSources = SourceFlagHelper.GetDefaultContainerIdSource();
                var result = new List<RecordPermissionDto>();
                var recordWithoutContainerId = recordPermission.Where(rp => !defaultContianerIdSources.Contains(rp.ContentSource)).ToList();
                var permission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>();
                var permissionExtensionMasks = await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionExtensionMasks>();
                int[] permissionCache = new int[11];
                if (recordWithoutContainerId.Count > 0)
                {
                    foreach (var record in recordWithoutContainerId)
                    {
                        var sourceFlag = (int)record.ContentSource;
                        if (permissionCache[sourceFlag] == 0)
                        {
                            permissionCache[sourceFlag] = HaveUserSourcePermission(record.ContentSource, permission, permissionExtensionMasks) ? 1 : -1;
                        }
                        result.Add(new RecordPermissionDto
                        {
                            RecordId = record.RecordId,
                            HasDelegatedAdmin = permissionCache[(int)sourceFlag] == 1
                        });
                    }
                }

                var recordNeedCheckContainerId = recordPermission.Where(rp => defaultContianerIdSources.Contains(rp.ContentSource)).ToList();
                if(recordNeedCheckContainerId.Count > 0)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    Dictionary<Guid, bool> permissionContainerCache = new Dictionary<Guid, bool>();
                    var allRecords = ExplorerDao.GetRecordByIds(recordNeedCheckContainerId.Select(rp => rp.RecordId).ToList()).ToList();
                    foreach (var record in allRecords)
                    {
                        var sourceFlag = (SourceFlag)record.SourceFlag;

                        if (permissionCache[record.SourceFlag] == 0)
                        {
                            permissionCache[record.SourceFlag] = HaveUserSourcePermission(sourceFlag, permission, permissionExtensionMasks) ? 1 : -1;
                        }
                        var containerId = new Guid(record.ContainerId);
                        if (!permissionContainerCache.ContainsKey(containerId))
                        {
                            permissionContainerCache[containerId] = RMScopeRoleAssignmentDao.HavePermissionOnContainerId(containerId, userAndGroupUserIds);
                        }
                        result.Add(new RecordPermissionDto
                        {
                            RecordId = record.Id,
                            HasDelegatedAdmin = permissionCache[record.SourceFlag] == 1 && permissionContainerCache[containerId]
                        });
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                logger.Error($"Error in GetRecordsPermission: {ex.Message}", ex);
                throw;
            }   
        }

        private bool HaveUserSourcePermission(SourceFlag contentSource, RMPermissionMasks permission, RMPermissionExtensionMasks permissionExtensionMasks)
            => contentSource switch
            {
                SourceFlag.FileSystem => permission.HasFlag(RMPermissionMasks.FSAdmin),
                SourceFlag.AzureFileShare => permissionExtensionMasks.HasFlag(RMPermissionExtensionMasks.AzureFSAdmin),
                SourceFlag.Box => permissionExtensionMasks.HasFlag(RMPermissionExtensionMasks.BoxAdmin),
                SourceFlag.SharePointOnPrem => permission.HasFlag(RMPermissionMasks.SPOnPremEnduser),
                SourceFlag.SharePoint => permission.HasFlag(RMPermissionMasks.SPOEnduser),
                SourceFlag.Teams => permissionExtensionMasks.HasFlag(RMPermissionExtensionMasks.TeamsAdmin),
                SourceFlag.OneDrive => permission.HasFlag(RMPermissionMasks.OneDriveEnduser),
                SourceFlag.Google => permissionExtensionMasks.HasFlag(RMPermissionExtensionMasks.GoogleAdmin),
                SourceFlag.Physical => permission.HasFlag(RMPermissionMasks.PhysicalAdmin),
                SourceFlag.Exchange => permission.HasFlag(RMPermissionMasks.EXOEnduser),
                _ => false,
            };

        public async Task<bool> HasDelegatedAdminpermission(List<Guid> recordIds)
        {
            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            return ValidatePermissionAsync(recordIds, userAndGroupUserIds);

        }
        public bool ValidatePermissionAsync(List<Guid> recordIds, List<string> userAndGroupUserIds)
        {
            ExplorerDao ExplorerDao = new ExplorerDao();
            var allRecord = ExplorerDao.GetRecordByIds(recordIds).ToList();

            if (allRecord.Count > 0)
            {
                List<string> containerIds = allRecord.Where(r => r.SourceFlag == (int)SourceFlag.Exchange || r.SourceFlag == (int)SourceFlag.SharePoint || r.SourceFlag == (int)SourceFlag.OneDrive ||
                                                                 r.SourceFlag == (int)SourceFlag.Teams || r.SourceFlag == (int)SourceFlag.Physical).Select(r => r.ContainerId).Distinct().ToList();
                if (containerIds.Count > 0 && !RMScopeRoleAssignmentDao.ValidateContainerIdPermission(containerIds, userAndGroupUserIds))
                {
                    logger.Info($"No access on container");
                    return false;
                }
            }
            return true;
        }
        public async Task<(bool, string)> ValidateDataSourcePermissionAsync(List<Record> allRecord)
        {
            string errorMessage = "";
            bool valid = true;
            if (allRecord.Count > 0)
            {
                if (allRecord.Where(r => string.IsNullOrEmpty(r.ContainerId) && (r.SourceFlag == 1 || r.SourceFlag == 3)).FirstOrDefault() != null)
                {
                    logger.Info($"record data need upgrade");
                    errorMessage = "record data need upgrade";
                    valid = false;
                    return (valid, errorMessage);
                }
                if (allRecord.Any(r => r.SourceFlag == 1 || r.SourceFlag == 0))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no sp access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 2))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser)))
                    {
                        logger.Info($"User have no file system access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no file system access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 3))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser)))
                    {
                        logger.Info($"User have no exchange access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no exchange access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 4))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalEndUser)))
                    {
                        logger.Info($"User have no physical access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no physical access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var userPermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                    var physcialRecords = allRecord.Where(r => r.SourceFlag == 4).ToList();
                    var permissionIds = physcialRecords.Where(r => r.ScopePermissionId != 0).Select(r => r.ScopePermissionId).Distinct().ToList();
                    if (permissionIds != null && permissionIds.Count > 0 && permissionIds.Any(p => !userPermissionIds.Contains(p)))
                    {
                        logger.Info($"User have no permission for some record. TenantId: {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no permission for some record";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 5))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser)))
                    {
                        logger.Info($"User have no sp on premise access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no sp on premise access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 6))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no od access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 7))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin)))
                    {
                        logger.Info($"User have no azure file share access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no azure file share access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == (int)SourceFlag.Teams))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no teams access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
            }
            else
            {
                logger.Info($"do not need to check permission {TenantLocalValue.LogonUserId}");
            }

            return (valid, errorMessage);
        }


        public List<Guid> GetRecordIds(Dictionary<SourceFlag, List<Guid>> idMapping)
        {
            List<Guid> recordIds = new List<Guid>();
            foreach (var keyValuePair in idMapping)
            {
                recordIds.AddRange(keyValuePair.Value.ToList());
            }
            return recordIds.ToList();
        }

        public void ConvertBoxAndFoldBarcordInfoToArray(IPhysicalBox box, List<string[]> sheetDatasInfo)
        {
            List<string> boxNameInfo = new List<string>();
            List<string> boxUniqueIDInfo = new List<string>();
            List<string> boxBarcodeString = new List<string>();
            boxNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
            boxNameInfo.Add(box.Name);
            boxNameInfo.Add("");
            boxUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
            boxUniqueIDInfo.Add(box.RecordId);
            boxUniqueIDInfo.Add("");
            boxBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
            boxBarcodeString.Add(box.RecordId);
            boxBarcodeString.Add("");
            sheetDatasInfo.Add(boxNameInfo.ToArray());
            sheetDatasInfo.Add(boxUniqueIDInfo.ToArray());
            sheetDatasInfo.Add(boxBarcodeString.ToArray());
            InsetEmptyRow(sheetDatasInfo);

            List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));
            List<string> fileNameInfo = new List<string>();
            List<string> fileUniqueIDInfo = new List<string>();
            List<string> fileBarcodeString = new List<string>();

            for (int num = 1; num <= folds.Count; num++)
            {
                fileNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
                fileNameInfo.Add(folds[num - 1].Name);
                fileNameInfo.Add("");
                fileUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
                fileUniqueIDInfo.Add(folds[num - 1].RecordId);
                fileUniqueIDInfo.Add("");
                fileBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
                fileBarcodeString.Add(folds[num - 1].RecordId);
                fileBarcodeString.Add("");
                if (num % 2 == 0)
                {
                    sheetDatasInfo.Add(fileNameInfo.ToArray());
                    sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
                    sheetDatasInfo.Add(fileBarcodeString.ToArray());
                    InsetEmptyRow(sheetDatasInfo);
                    fileNameInfo = new List<string>();
                    fileUniqueIDInfo = new List<string>();
                    fileBarcodeString = new List<string>();
                }
            }
            if (!fileNameInfo.IsNullOrEmpty())
            {
                sheetDatasInfo.Add(fileNameInfo.ToArray());
                sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
                sheetDatasInfo.Add(fileBarcodeString.ToArray());
                InsetEmptyRow(sheetDatasInfo);
            }
        }

       /* public void ConvertBoxAndFoldBarcordInfoToDto(IPhysicalBox box, List<ExportBarcodeDataModel> datas)
        {
            //ExportBarcodeDataModel boxData = new ExportBarcodeDataModel();
            //boxData.Title = box.Name;
            //boxData.UniqueID = box.RecordId;
            //boxData.Desc = "";
            //boxData.Location = "City state library. - ltc";
            //boxData.CreateTime = box.CreateTimeTicks.ToString() ;
            //List<string> boxNameInfo = new List<string>();
            //List<string> boxUniqueIDInfo = new List<string>();
            //List<string> boxBarcodeString = new List<string>();
            //datas.Add(boxData);

            //List<PhysicalFile> folds = box.GetFilesOrderByDescending(b => (b.RecordStatus == (int)RMRecordStatus.Active || b.RecordStatus == (int)RMRecordStatus.Closed || b.RecordStatus == (int)RMRecordStatus.Missing || b.RecordStatus == (int)RMRecordStatus.Destroyed));

            //for (int num = 1; num <= folds.Count; num++)
            //{
            //    ExportBarcodeDataModel foldData = new ExportBarcodeDataModel();
            //    foldData.Title = folds[num - 1].Name;
            //    foldData.UniqueID = folds[num - 1].RecordId;
            //    foldData.Desc = "";
            //    foldData.Location = "City state library. - ltc";
            //    foldData.CreateTime = folds[num - 1].CreateTimeTicks.ToString();
            //    datas.Add(foldData);
            //}
        }*/

        public void ConvertFoldBarcordInfoToArray(IPhysicalFile file, List<string[]> sheetDatasInfo)
        {
            List<string> fileNameInfo = new List<string>();
            List<string> fileUniqueIDInfo = new List<string>();
            List<string> fileBarcodeString = new List<string>();
            //PhysicalBox box = new PhysicalBox(NodeId);
            fileNameInfo.Add(I18NEntity.GetString("RM_EBR_Name"));
            fileNameInfo.Add(file.Name);
            fileNameInfo.Add("");
            fileUniqueIDInfo.Add(I18NEntity.GetString("RM_EBR_UniqueId"));
            fileUniqueIDInfo.Add(file.RecordId);
            fileUniqueIDInfo.Add("");
            fileBarcodeString.Add(I18NEntity.GetString("RM_EBR_Barcode"));
            fileBarcodeString.Add(file.RecordId);
            fileBarcodeString.Add("");
            sheetDatasInfo.Add(fileNameInfo.ToArray());
            sheetDatasInfo.Add(fileUniqueIDInfo.ToArray());
            sheetDatasInfo.Add(fileBarcodeString.ToArray());
            InsetEmptyRow(sheetDatasInfo);
        }

        private void InsetEmptyRow(List<string[]> sheetDatasInfo)
        {
            List<string> emptyRow = new List<string>();
            for (int index = 0; index < 6; index++)
            {
                emptyRow.Add("");
            }
            sheetDatasInfo.Add(emptyRow.ToArray());
        }

        #region Private Zone

        private string AddJobToDBJobQueue(JobRunBy jobRunBy, JobType jobType, ExportBarcodeDto exportBarcodeDto)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0} {1} {2} {3} {4}", exportBarcodeDto.ExportLocationId, exportBarcodeDto.NodeId, exportBarcodeDto.NodeType, exportBarcodeDto.ExportLocationName, exportBarcodeDto.SuiteId),
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while run physical explorer timer job, ERROR : {ex.ToString()}.");
            }
            return id;
        }

        private void AddPushColumnToDB(PhysicalObjectDto dto, Record record)
        {
            var pushColumnCollection = new Dictionary<Guid, TemplateColumnDto>();
            dto.Template.categories.ForEach(cat =>
            {
                cat.columns.ForEach(col =>
                {
                    logger.Info($"foreach column, {col.columnName}, {col.pushToChild}, {col.inheritFromParent}, {col.inheritFromParentFolder}");
                    if (col.pushToChild || col.inheritFromParent || col.inheritFromParentFolder)
                    {
                        logger.Info($"add push column to db, {col.columnName}, {col.pushToChild}, {col.inheritFromParent}, {col.inheritFromParentFolder}");
                        pushColumnCollection[col.uniqueId] = col;
                    }
                });
            });
            if (pushColumnCollection.Count > 0)
            {
                foreach (var pushColumn in pushColumnCollection)
                {
                    string pushColumnValue;
                    dto.MetaInfo.TryGetValue(pushColumn.Key.ToString(), out pushColumnValue);
                    logger.Info($"get push column value, {pushColumn.Key}, {pushColumnValue}");
                    var physicalObjectId = record.Id;
                    if (pushColumn.Value.inheritFromParent)
                    {
                        physicalObjectId = record.BoxId;
                    }
                    else if (pushColumn.Value.inheritFromParentFolder)
                    {
                        physicalObjectId = record.FileId;
                    }

                    RMPhysicalPushColumnDao.AddOrUpdate(new RMPhysicalPushColumn()
                    {
                        ColumnUniqueId = pushColumn.Key,
                        TemplateId = dto.Template.id,
                        ColumnValue = pushColumnValue,
                        PhysicalObjectId = physicalObjectId,
                    });
                    logger.Info($"finsh to update push column, {pushColumn.Key}, {dto.Template.id}, {pushColumnValue}, {physicalObjectId}");
                }
            }
        }

        private void UpdateDestroyedTime(Record uiRecord)
        {
            if (uiRecord.RecordStatus == (int)RMRecordStatus.Destroyed)
            {
                var dbRecord = ExplorerDao.GetPhysicalRecordById(uiRecord.Id);
                if (dbRecord == null)
                {
                    uiRecord.DestroyedTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    uiRecord.DestroyedTime = dbRecord.DestroyedTime == 0 ? DateTime.UtcNow.Ticks : dbRecord.DestroyedTime;
                }
            }
        }

        /// <summary>
        /// Edit Physical Object Need keep DisposalStatus
        /// </summary>
        /// <param name="uiRecord"></param>
        private void UpdateDisposalStatus(Record uiRecord, Guid currentRuleId)
        {
            if (uiRecord.RecordStatus == (int)RMRecordStatus.Active || uiRecord.RecordStatus == (int)RMRecordStatus.Closed)
            {
                var dbRecord = ExplorerDao.GetPhysicalRecordById(uiRecord.Id);
                if (dbRecord != null && dbRecord.RuleId == currentRuleId)
                {
                    uiRecord.DisposalStatus = dbRecord.DisposalStatus;
                }
            }
        }

        private async System.Threading.Tasks.Task DealWithPhysicalBottomLocationIdAsync(PhysicalExplorerQueryDto dto)
        {
            if (dto.CurrentNodeType == RMNodeLevel.PhysicalBottomLocation)
            {
                var nodeId = 0;
                if (int.TryParse(dto.NodeId, out nodeId))
                {
                    if (nodeId != 0)
                    {
                        var tempLocation = await LocationManagementService.GetPhysicalObjectByIdAsync(nodeId);
                        if (tempLocation != null)
                        {
                            dto.NodeId = tempLocation.Id.ToString();
                        }
                    }
                }
                else
                {
                    logger.Error($"DealWithPhysicalBottomLocationId, current id seems is not in correct format, id value: [{dto.NodeId}].");
                }
            }
        }

        public Task<SecurityTermPermissionDto> GetSecurityTermDtoAsync()
        {
            return ExplorerQueryService.GetSecurityTermDtoAsync();
        }

        //private void GetSecurityTermExpression(ParameterExpression param, ref List<Expression> allExpressionList)
        //{
        //    var permissionDto = await GetSecurityTermDtoAsync();
        //    List<Expression> securityTermExp = new List<Expression>();
        //    switch (permissionDto.TermPermissionType)
        //    {
        //        case TermPermissionMethod.All:
        //            break;
        //        case TermPermissionMethod.SpecifyScope:
        //            var termIds = permissionDto.TermObjIds;
        //            foreach (var guid in termIds)
        //            {
        //                securityTermExp.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TermId", guid));
        //            }
        //            securityTermExp.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TermId", Guid.Empty));
        //            break;
        //        case TermPermissionMethod.None:
        //            securityTermExp.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TermId", Guid.Empty));
        //            break;
        //        default:
        //            break;
        //    }

        //    if (securityTermExp.Count > 0)
        //    {
        //        allExpressionList.Add(securityTermExp.Aggregate(Expression.OrElse));
        //    }
        //}
        #region Obsolete code
        //private void GetWithoutPhyRecordExpression(ParameterExpression param, ref List<Expression> allExpressionList)
        //{
        //    allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalRecord));
        //}

        //private Expression<Func<Record, bool>> GetFilterLambdaForPhysical(PhysicalExplorerQueryDto queryDto, bool withoutPhysicalRecord = false)
        //{
        //    Expression queryExpr = null;
        //    List<Expression> allExpressionList = new List<Expression>();
        //    ParameterExpression param = Expression.Parameter(typeof(Record), "c");

        //    GetSecurityTermExpression(param, ref allExpressionList);
        //    if (withoutPhysicalRecord)
        //    {
        //        GetWithoutPhyRecordExpression(param, ref allExpressionList);
        //    }
        //    if (queryDto != null)
        //    {
        //        if (queryDto.NodeId != null)
        //        {
        //            //Name && UniqueId Search using contain search filter
        //            if (queryDto.FilterOption != null)
        //            {
        //                bool hasFilterCase = false;
        //                if (!string.IsNullOrEmpty(queryDto.FilterOption.SearchKey))
        //                {
        //                    hasFilterCase = true;
        //                    var key = queryDto.FilterOption.SearchKey;
        //                    List<Expression> searchKeyExpressionList = new List<Expression>();
        //                    searchKeyExpressionList.Add(Expression4DynamicQuery.GetContainsExpression(typeof(Record), param, "LeafName", key.ToLower()));
        //                    searchKeyExpressionList.Add(Expression4DynamicQuery.GetContainsExpression(typeof(Record), param, "RecordsId", key.ToLower()));
        //                    allExpressionList.Add(searchKeyExpressionList.Aggregate(Expression.OrElse));
        //                }
        //                if (queryDto.FilterOption.Status != 0)
        //                {
        //                    hasFilterCase = true;
        //                    // -1 为前后台约定好的值，在传递 -1 的时候，表示搜索所有类型。
        //                    if (queryDto.FilterOption.Status != -1)
        //                    {
        //                        allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", queryDto.FilterOption.Status));
        //                    }
        //                }
        //                if (queryDto.FilterOption.NodeType != RMNodeLevel.Undefined)
        //                {
        //                    hasFilterCase = true;
        //                    // -4 为前后台约定好的值，在传递 -4 的时候，表示搜索所有类型。
        //                    if (queryDto.FilterOption.NodeType != RMNodeLevel.RMSelectAll)
        //                    {
        //                        allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", queryDto.FilterOption.NodeType));
        //                    }
        //                }
        //                if (queryDto.FilterOption.RecordsOwner != null)
        //                {
        //                    List<Expression> ownerExpressionList = new List<Expression>();
        //                    foreach (var owner in queryDto.FilterOption.RecordsOwner)
        //                    {
        //                        ownerExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "RecordOwner", '|' + owner + '|'));
        //                    }
        //                    if (ownerExpressionList.Count > 0)
        //                    {
        //                        hasFilterCase = true;
        //                        allExpressionList.Add(ownerExpressionList.Aggregate(Expression.OrElse));
        //                    }
        //                }
        //                if (queryDto.FilterOption.CreatedBy != null)
        //                {
        //                    List<Expression> createdByExpressionList = new List<Expression>();
        //                    foreach (var createdBy in queryDto.FilterOption.CreatedBy)
        //                    {
        //                        createdByExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "CreatedBy", createdBy.ToLower()));
        //                    }
        //                    if (createdByExpressionList.Count > 0)
        //                    {
        //                        hasFilterCase = true;
        //                        allExpressionList.Add(createdByExpressionList.Aggregate(Expression.OrElse));
        //                    }
        //                }
        //                if (queryDto.FilterOption.ModifiedBy != null)
        //                {
        //                    List<Expression> createdByExpressionList = new List<Expression>();
        //                    foreach (var createdBy in queryDto.FilterOption.ModifiedBy)
        //                    {
        //                        createdByExpressionList.Add(Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(Record), param, "ModifiedBy", createdBy.ToLower()));
        //                    }
        //                    if (createdByExpressionList.Count > 0)
        //                    {
        //                        hasFilterCase = true;
        //                        allExpressionList.Add(createdByExpressionList.Aggregate(Expression.OrElse));
        //                    }
        //                }

        //                if (queryDto.FilterOption.TermTreeFilter != null && queryDto.FilterOption.TermTreeFilter != Guid.Empty)
        //                {
        //                    if (queryDto.CurrentNodeType != RMNodeLevel.PhysicalFile)
        //                    {
        //                        allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "TermId", queryDto.FilterOption.TermTreeFilter));
        //                    }
        //                }

        //                if (hasFilterCase)
        //                {
        //                    GenerateDeepQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, param);
        //                }
        //                else
        //                {
        //                    if (queryDto != null)
        //                    {
        //                        GenerateShallowQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, param);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (queryDto != null)
        //                {
        //                    GenerateShallowQueryExpression((int)queryDto.CurrentNodeType, new Guid(queryDto.NodeId), allExpressionList, param);
        //                }
        //            }
        //        }
        //    }
        //    if (allExpressionList.Count > 0)
        //    {
        //        List<Expression> nodeStatusExpressionList = new List<Expression>();
        //        nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Active));
        //        nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Closed));
        //        nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Missing));
        //        nodeStatusExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "RecordStatus", (int)RMRecordStatus.Destroyed));
        //        allExpressionList.Add(nodeStatusExpressionList.Aggregate(Expression.OrElse));
        //        //allExpressionList.Add(Expression4DynamicQuery.GetNotEqualityExpression(typeof(Record), param, "RecordStatus", 3));
        //        //allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "SourceFlag", SourceFlag.Physical)); 
        //        //增加ScopeID条件避免Cross Partition Filter
        //        allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ScopeId", Guid.Empty));
        //        queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
        //        return Expression.Lambda<Func<Record, bool>>(queryExpr, param);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// 此方法只提供browser 下一层数据的Expression， 对于深层search 不work。
        /// </summary>
        /// <param name="currentNodeLevel">当前节点的NodeLevel是什么，用来指定拼装Express 的级别</param>
        /// <param name="nodeId">当前节点的Id</param>
        /// <param name="allExpressionList">外围实例化一个Expression 集合，用来添加每个级别的特殊条件，最终按照and 关系拼接。PS： 此处可以重构，用另外的方法去维护。</param>
        /// <param name="param"></param>
        //private void GenerateShallowQueryExpression(int currentNodeLevel, Guid nodeId, List<Expression> allExpressionList, ParameterExpression param)
        //{
        //    List<Expression> nodeTypeExpressionList = new List<Expression>();
        //    switch (currentNodeLevel)
        //    {
        //        case (int)RMNodeLevel.PhysicalBottomLocation:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalBox));
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", nodeId));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", Guid.Empty));
        //            break;
        //        case (int)RMNodeLevel.PhysicalBox:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalFile));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalFile:
        //            nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "NodeType", RMNodeLevel.PhysicalRecord));
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "FileId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalRecord:
        //        case (int)RMNodeLevel.Undefined:
        //        default:
        //            break;
        //    }

        //    nodeTypeExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "ParentId", nodeId)); //for custom

        //    allExpressionList.Add(nodeTypeExpressionList.Aggregate(Expression.OrElse));
        //}

        /// <summary>
        /// 此方法提供browser 深层数据的Expression
        /// </summary>
        /// <param name="currentNodeLevel">当前节点的NodeLevel是什么，用来指定拼装Express 的级别</param>
        /// <param name="nodeId">当前节点的Id</param>
        /// <param name="allExpressionList">外围实例化一个Expression 集合，用来添加每个级别的特殊条件，最终按照and 关系拼接。PS： 此处可以重构，用另外的方法去维护。</param>
        /// <param name="param"></param>
        //private void GenerateDeepQueryExpression(int currentNodeLevel, Guid nodeId, List<Expression> allExpressionList, ParameterExpression param)
        //{
        //    List<Expression> nodeTypeExpressionList = new List<Expression>();
        //    switch (currentNodeLevel)
        //    {
        //        case (int)RMNodeLevel.PhysicalBottomLocation:
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "LocationId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalBox:
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "BoxId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalFile:
        //            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(Record), param, "FileId", nodeId));
        //            break;
        //        case (int)RMNodeLevel.PhysicalRecord:
        //        case (int)RMNodeLevel.Undefined:
        //        default:
        //            break;
        //    }
        //}
        #endregion
        private void CalculateRuleProperty(PhysicalObjectDto dto, Record record)
        {
            var termId = 0;
            Rule ruleResult = null;
            var dueDisposalTime = string.Empty;
            if (record.NodeType != (int)RMNodeType.PhyRecord && record.NodeType != (int)RMNodeType.PhyCustom && (record.RecordStatus == (int)RMRecordStatus.Active || record.RecordStatus == (int)RMRecordStatus.Closed))
            {
                var daRules = new List<Rule>();
                if (record.TermId != Guid.Empty)
                {
                    try
                    {
                        var term = TermDao.GetRMTermByGuId(record.TermId);
                        termId = term.Id;
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error in CalculateRuleProperty, reason :  [{ex.ToString()}]");
                    }
                }
                else
                {
                    logger.Warn($"No term for current physical object. [{dto.Name}]");
                }
                if (termId != 0)
                {
                    daRules = GetRuleByTermId(termId);
                    if (daRules != null && daRules.Count > 0)
                    {
                        var columnCollection = new Dictionary<Guid, TemplateColumnDto>();
                        if (dto.Template.type == TemplateType.Folder && dto.BoxId != Guid.Empty)
                        {
                            AddPushColumnToFold(dto.Template, dto.BoxId);
                        }
                        dto.Template.categories.ForEach(cat =>
                        {
                            cat.columns.ForEach(col => columnCollection[col.uniqueId] = col);
                        });
                        var ruleEngine = new PhysicalRuleEngine(daRules);
                        var ids = new List<Guid>();
                        ids.Add(record.Id);
                        if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
                        {
                            ids.Add(record.BoxId);
                        }
                        ruleResult = ruleEngine.CheckRule(record, columnCollection);
                        if (ruleResult != null)
                        {
                            if (IsMoveToRuleForObjectUnderContaner(ruleResult, record))
                            {
                                logger.Info("Matched rule is moveto rule and current record is under container, will not set rule id.");
                                ruleResult = null;
                            }
                            else
                            {
                                dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                            }
                        }
                        else
                        {
                            ruleResult = ruleEngine.CheckDueDisposalRule(record, columnCollection, ref dueDisposalTime);
                        }
                    }
                }
            }
            else if (dto.Template.type == TemplateType.Records && (record.RecordStatus == (int)RMRecordStatus.Active || record.RecordStatus == (int)RMRecordStatus.Closed))
            {
                AddPushColumnToRecord(dto.Template, dto);
                logger.Info($"Physical record status is : {record.RecordStatus}, no need to check rule.");
            }
            //Fix Rule and Disposal Info
            if (ruleResult != null && !string.IsNullOrEmpty(ruleResult.Id))
            {
                UpdateDisposalStatus(record, new Guid(ruleResult.Id));
            }
            record.RuleId = ruleResult != null ? new Guid(ruleResult.Id) : Guid.Empty;
            record.RuleLevel = ruleResult != null ? (int)ruleResult.PolicyLevel : 0;
            record.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime);
            record.PreviosDisposalDueDate = record.DisposalDueDate;
        }

        private bool IsMoveToRuleForObjectUnderContaner(Rule rule, Record record)
        {
            if (rule.IsPhysicalMoveToRule())
            {
                if (record.NodeType == (int)RMNodeType.PhyBox && BoxUnderContainer(record))
                {
                    return true;
                }

                if (record.NodeType == (int)RMNodeType.PhyFile && FolderUnderContainer(record))
                {
                    return true;
                }
            }

            return false;
        }

        private bool BoxUnderContainer(Record box)
        {
            if (box.Ancestors != null && box.Ancestors.Count > 0 && box.ParentId != box.LocationId)
            {
                return true;
            }
            return false;
        }

        private bool FolderUnderContainer(Record folder)
        {
            if (folder.Ancestors != null && folder.Ancestors.Count > 1)
            {
                if (folder.ParentId == folder.LocationId || folder.Ancestors[1] == folder.BoxId)
                {
                    //folder under location or location/box
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void AddPushColumnToFold(TemplateDto resultDto, Guid boxId)
        {
            if (boxId == Guid.Empty)
            {
                logger.Error("box id is empty.");
                return;
            }
            Record box = ExplorerDao.GetPhysicalRecordById(boxId);
            if (box == null)
            {
                logger.Error("Can't find fold's parent box,box id is {0}", boxId.ToString());
                return;
            }
            RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
            if (boxTemplate == null)
            {
                logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                return;
            }
            var columnSchema = boxTemplate.ColumnSchema;
            TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(columnSchema);
            List<ColumnXmlSchema> columns = schema.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var item = columns[i];
                if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                {
                    List<TemplateIdAndCategoryId> pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId;
                    if (pushFoldTemplateCategoriesId != null && pushFoldTemplateCategoriesId.Count > 0)
                    {
                        TemplateIdAndCategoryId templateCategoryId = pushFoldTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                        if (templateCategoryId != null)
                        {
                            foreach (var category in resultDto.categories)
                            {
                                if (category.id.ToString() == templateCategoryId.categoryId)
                                {
                                    bool isInheritFromBox = true;
                                    TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                                    category.columns.Add(columnDto);
                                }
                            }
                        }
                        //如果没有存储当前sub template的信息,则把push column add到默认category里 即第一个
                        else
                        {
                            bool isInheritFromBox = true;
                            TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                            resultDto.categories[0].columns.Add(columnDto);
                        }
                    }
                }
            }
        }

        public void AddPushColumnToRecord(TemplateDto resultDto, PhysicalObjectDto physical)
        {
            if (physical.FileId != Guid.Empty)
            {
                Record fold = ExplorerDao.GetPhysicalRecordById(physical.FileId);
                if (fold == null)
                {
                    logger.Error("Can't find node's parent fold,fold id is {0}", physical.FileId.ToString());
                    ArgumentCheck.NotNull(fold, nameof(fold));
                }
                RMTemplate foldTemplate = TemplateDao.GetTemplateById(fold.TemplateId);
                if (foldTemplate == null)
                {
                    logger.Error("Can't find fold's template ,template id is {0}", fold.TemplateId.ToString());
                    ArgumentCheck.NotNull(foldTemplate, nameof(foldTemplate));
                }
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(foldTemplate.ColumnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                        if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                        {
                            TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());
                            if (templateCategoryId != null)
                            {
                                foreach (var category in resultDto.categories)
                                {
                                    if (category.id.ToString() == templateCategoryId.categoryId)
                                    {
                                        bool isInheritFromBox = false;
                                        TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                                        category.columns.Add(columnDto);
                                    }
                                }
                            }
                            else
                            {
                                bool isInheritFromBox = false;
                                TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                                resultDto.categories[0].columns.Add(columnDto);
                            }
                        }
                    }
                }
            }
            if (physical.BoxId != Guid.Empty)
            {
                Record box = ExplorerDao.GetPhysicalRecordById(physical.BoxId);
                if (box == null)
                {
                    logger.Error("Can't find node's parent fold,box id is {0}", physical.BoxId.ToString());
                    ArgumentCheck.NotNull(box, nameof(box));
                }
                RMTemplate boxTemplate = TemplateDao.GetTemplateById(box.TemplateId);
                if (boxTemplate == null)
                {
                    logger.Error("Can't find box's template ,template id is {0}", box.TemplateId.ToString());
                    ArgumentCheck.NotNull(boxTemplate, nameof(boxTemplate));
                }
                TemplateColumnsSchema schema = SerializerHelper.DeserializeByDataContractSerializer<TemplateColumnsSchema>(boxTemplate.ColumnSchema);
                List<ColumnXmlSchema> columns = schema.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    var item = columns[i];
                    if ((item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild)
                    {
                        List<TemplateIdAndCategoryId> pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId;
                        if (pushRecordTemplateCategoriesId != null && pushRecordTemplateCategoriesId.Count > 0)
                        {
                            TemplateIdAndCategoryId templateCategoryId = pushRecordTemplateCategoriesId.Find(t => t.tempalteId == resultDto.uniqueId.ToString());

                            if (templateCategoryId != null)
                            {
                                foreach (var category in resultDto.categories)
                                {
                                    if (category.id.ToString() == templateCategoryId.categoryId)
                                    {
                                        bool isInheritFromBox = true;
                                        TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                                        category.columns.Add(columnDto);
                                    }
                                }
                            }
                            else
                            {
                                bool isInheritFromBox = true;
                                TemplateColumnDto columnDto = ConvertToPageColumnDto(item, isInheritFromBox);
                                resultDto.categories[0].columns.Add(columnDto);
                            }
                        }
                    }
                }
            }
        }

        public TemplateColumnDto ConvertToPageColumnDto(ColumnXmlSchema item, bool isInheritFromBox)
        {
            var columnDto = new TemplateColumnDto()
            {
                categoryId = item.CategoryId,
                columnName = item.Name,
                uniqueId = item.UniqueId,
                required = item.Required,
                typeId = (int)item.ColumnType,
                showInEditForm = item.ShowInEditForm,
                allowEdit = item.AllowEdit,
                allowSort = item.AllowSort,
                allowEditSort = item.AllowEditSort(),
                inheritFromParent = isInheritFromBox,
                inheritFromParentFolder = !isInheritFromBox,
                pushToChild = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.PushToChild) == (int)TemplateInheritSettingEnum.PushToChild,
                //childInheritsValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.ChildInheritsValue) == (int)TemplateInheritSettingEnum.ChildInheritsValue,
                allowModifyValue = (item.TemplateInheritSetting & (int)TemplateInheritSettingEnum.AllowModifyValue) == (int)TemplateInheritSettingEnum.AllowModifyValue,
                pushFoldTemplateCategoriesId = item.pushFoldTemplateCategoriesId,
                pushRecordTemplateCategoriesId = item.pushRecordTemplateCategoriesId,
            };
            //RECO-4254
            if (item.UniqueId == new Guid(DefaultColumnIDs.Description))
            {
                columnDto.allowEdit = true;
            }
            switch (item.ColumnType)
            {
                case Contract.TemplateManagement.ColumnType.SingleText:
                case Contract.TemplateManagement.ColumnType.MultipleText:
                case Contract.TemplateManagement.ColumnType.DateTime:
                case Contract.TemplateManagement.ColumnType.PeopleOrGroup:
                case Contract.TemplateManagement.ColumnType.Number:
                    break;
                case Contract.TemplateManagement.ColumnType.Taxonomy:
                    break;
                case Contract.TemplateManagement.ColumnType.SingleChoice:
                case Contract.TemplateManagement.ColumnType.MultipleChoice:
                    columnDto.optionsJSON = item.OptionsJSON;
                    columnDto.optionsMaxIdReachedValue = item.OptionsMaxIdReachedValue;
                    break;
                default:
                    break;
            }
            return columnDto;
        }

        /// <summary>
        /// TO DO change from load rule from records.
        /// </summary>
        /// <param name="termId"></param>
        /// <returns></returns>
        private List<Rule> GetRuleByTermId(int termId)
        {
            var result = new List<Rule>();
            try
            {
                var listRule = TermRuleAssociationDao.GetTermRuleInfoByTermid(termId);
                if (listRule != null && listRule.Count > 0)
                {
                    var ruleIds = listRule.OrderBy(a => a.RuleOrder).Select(b => b.RuleId).ToList();
                    foreach (var ruleId in ruleIds)
                    {
                        try
                        {
                            var client = new DAOAPIClientV1();
                            var tempRule = client.LoadRule(ruleId.ToString());
                            if (tempRule != null && tempRule.Id != Guid.Empty.ToString())
                            {
                                result.Add(tempRule);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error in get rule from DAO, reason : [{ex.ToString()}]");
                            throw;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetRuleByTermId, reason : [{ex.ToString()}]");
                throw;
            }
            return result;
        }
        private List<Rule> GetRuleByTermId(Guid termId)
        {
            var result = new List<Rule>();
            try
            {
                var listRule = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(termId);
                if (listRule != null && listRule.Count > 0)
                {
                    var ruleIds = listRule.OrderBy(a => a.RuleOrder).Select(b => b.RuleId).ToList();
                    foreach (var ruleId in ruleIds)
                    {
                        try
                        {
                            var client = new DAOAPIClientV1();
                            var tempRule = client.LoadRule(ruleId.ToString());
                            if (tempRule != null && tempRule.Id != Guid.Empty.ToString())
                            {
                                result.Add(tempRule);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error in get rule from DAO, reason : [{ex.ToString()}]");
                            throw;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetRuleByTermId, reason : [{ex.ToString()}]");
                throw;
            }
            return result;
        }

        private async Task<PhysicalObjectDto> CheckBarcodeExist(PhysicalObjectDto dto, bool isEdit)
        {
            var barcodeRecords = new List<Record>();
            if (dto.MetaInfo.TryGetValue(DefaultColumnIDs.Barcode, out string value) && !string.IsNullOrEmpty(value))
            {
                barcodeRecords.AddRange(_explorerDao.QueryAll(r => (value.Equals(r.CustomColumnDic[DefaultColumnIDs.Barcode].Value, StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals(r.RecordsId, StringComparison.InvariantCultureIgnoreCase)) && r.RecordStatus != 3, false).ToList());
            }
            else
            {
                //value = dto.UniqueId;
                barcodeRecords.AddRange(_explorerDao.QueryAll(r => (dto.UniqueId.Equals(r.CustomColumnDic[DefaultColumnIDs.Barcode].Value, StringComparison.InvariantCultureIgnoreCase)
                || dto.UniqueId.Equals(r.RecordsId, StringComparison.InvariantCultureIgnoreCase)) && r.RecordStatus != 3, false).ToList());
            }

            if(barcodeRecords.Count == 0)
            {
                return dto;
            }

            if (!isEdit)
            {
                if (string.IsNullOrEmpty(value))
                {
                    if(barcodeRecords.Any(a => a.CustomColumnDic.ContainsKey(DefaultColumnIDs.Barcode) && a.CustomColumnDic[DefaultColumnIDs.Barcode].Value == dto.UniqueId))
                    {
                        dto.UniqueId = await GeneratePhysicalObjectUniqueIdAsync(dto.Template.type, dto.TemplateId.ToString(), dto.Template.prefix, dto.Template.numberOfDigits);
                        return await CheckBarcodeExist(dto, false);
                    }
                }
                else
                {
                    if (barcodeRecords.Any(a => ((a.CustomColumnDic.ContainsKey(DefaultColumnIDs.Barcode) && a.CustomColumnDic[DefaultColumnIDs.Barcode].Value == value) || a.RecordsId == value)
                        && a.RecordsId != dto.UniqueId))
                    {
                        throw new BarcodeDuplicateException("RM_Phy_Import_BarcodeDuplicateError");
                    }
                }

            }
            else
            {
                if (barcodeRecords.Any(a => ((a.CustomColumnDic.ContainsKey(DefaultColumnIDs.Barcode) && a.CustomColumnDic[DefaultColumnIDs.Barcode].Value == value) || a.RecordsId == value)
                    && a.RecordsId != dto.UniqueId))
                {
                    throw new BarcodeDuplicateException("RM_Phy_Import_BarcodeDuplicateError");
                }
            }

            return dto;
        }

        private RAReturnMessage ValidationPhysicalDto(PhysicalObjectDto dto)
        {
            var resultMsg = new RAReturnMessage();
            resultMsg.MessageType = RAMessageType.Successful;

            if (dto.MetaInfo != null && dto.MetaInfo.Count > 0)
            {
                //Check name 
                if (dto.MetaInfo.ContainsKey(DefaultColumnIDs.NameOrTitle))
                {
                    if (dto.MetaInfo[DefaultColumnIDs.NameOrTitle] != dto.Name)
                    {
                        logger.Info($"Name dose not equal with meta info.");
                        resultMsg.MessageType = RAMessageType.Failed;
                        return resultMsg;
                    }
                }
                //Check Capacity
                if (dto.MetaInfo.ContainsKey(DefaultColumnIDs.Capability))
                {
                    var capacity = dto.MetaInfo[DefaultColumnIDs.Capability];
                    if (capacity != string.Empty)
                    {

                        double value;
                        if (double.TryParse(capacity, out value))
                        {
                            if (value <= 0)
                            {
                                logger.Info($"Capacity is : {value}.");
                                resultMsg.MessageType = RAMessageType.Failed;
                                return resultMsg;
                            }
                        }
                        else
                        {
                            resultMsg.MessageType = RAMessageType.Failed;
                            return resultMsg;
                        }
                    }
                }
                //Check Classification
                if (dto.MetaInfo.ContainsKey(DefaultColumnIDs.Classification))
                {
                    var classification = JsonConvert.DeserializeObject<TaxonomyColumnValue>(dto.MetaInfo[DefaultColumnIDs.Classification]);
                    Guid termId;
                    if (Guid.TryParse(classification.Id, out termId))
                    {
                        var term = TermDao.GetAvailableTermByGuId(termId);
                        if(term == null)
                        {
                            logger.Info($"Current term has been removed, UI name : {classification.Name}, term id : {termId}.");
                            resultMsg.MessageType = RAMessageType.Failed;
                            resultMsg.ErrorMessage = I18NEntity.GetString("RM_JS_PHY_InvalidTerm");
                            return resultMsg;
                        }
                        if (!term.Name.Equals(classification.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info($"Classification had been changed, UI name : {classification.Name}, DB  name : {term.Name}.");
                            resultMsg.MessageType = RAMessageType.Failed;
                            resultMsg.ErrorMessage = I18NEntity.GetString("RM_JS_RDM_Valid_ChangeTermName");
                            return resultMsg;
                        }
                    }
                    else
                    {
                        logger.Info($"Cannot convert to unique for classification : {classification.Id}, {classification.Name}");
                        resultMsg.MessageType = RAMessageType.Failed;
                        return resultMsg;
                    }
                }
                //Check LocationName
                if (dto.MetaInfo.ContainsKey(DefaultColumnIDs.HomeLocation))
                {
                    var locationId = dto.LocationId;
                    var location = LocationDao.GetLocationByUniqueId(locationId);
                    var homeLocation = JsonConvert.DeserializeObject<TaxonomyColumnValue>(dto.MetaInfo[DefaultColumnIDs.HomeLocation]);
                    if (!location.Name.Equals(homeLocation.Name) || !homeLocation.Id.Equals(location.UniqueId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info($"Home location :{homeLocation.Name} has been changed, {location.Name}.");
                    }
                }
                ValidationMetaInfo(dto.MetaInfo, dto.Template);
            }
            return resultMsg;
        }

        private void ValidationMetaInfo(Dictionary<string, string> metaInfo, TemplateDto template)
        {
            var updateCustomColums = ExplorerDao.GetUpdateColumns(metaInfo);
            foreach (var category in template.categories)
            {
                foreach (var templateColumn in category.columns)
                {
                    if (templateColumn.typeId == (int)ColumnType.SingleChoice || templateColumn.typeId == (int)ColumnType.MultipleChoice)
                    {
                        foreach (var updateColumn in updateCustomColums)
                        {
                            Dictionary<int, string> templateColumnOption = JsonConvert.DeserializeObject<Dictionary<int, string>>(templateColumn.optionsJSON);
                            if (templateColumn.uniqueId.ToString() == updateColumn.Key)
                            {
                                if (updateColumn.Value.MultiChoice != null && updateColumn.Value.MultiChoice.Count != 0)
                                {
                                    //MultipleChoice
                                    foreach (var updateColumnChoiceOption in updateColumn.Value.MultiChoice)
                                    {
                                        if (templateColumnOption.Keys.Contains(int.Parse(updateColumnChoiceOption.Value)))
                                        {
                                            if (templateColumnOption[int.Parse(updateColumnChoiceOption.Value)] != updateColumnChoiceOption.Name)
                                            {
                                                var valueList = JsonConvert.DeserializeObject<List<ChoiceColumnValue>>(metaInfo[updateColumn.Key]);
                                                var metaInfoOptionValue = valueList.FirstOrDefault(v => v.Value == updateColumnChoiceOption.Value);
                                                if (metaInfoOptionValue != null)
                                                {
                                                    metaInfoOptionValue.Name = templateColumnOption[int.Parse(updateColumnChoiceOption.Value)];
                                                }
                                                metaInfo[updateColumn.Key] = JsonConvert.SerializeObject(valueList);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Choice option not exist");
                                        }
                                    }
                                }
                                else
                                {
                                    //SingleChoice
                                    if (templateColumnOption.Keys.Contains(int.Parse(updateColumn.Value.Value)))
                                    {
                                        if (templateColumnOption[int.Parse(updateColumn.Value.Value)] != updateColumn.Value.Name)
                                        {
                                            var value = JsonConvert.DeserializeObject<ChoiceColumnValue>(metaInfo[updateColumn.Key]);
                                            value.Name = templateColumnOption[int.Parse(updateColumn.Value.Value)];
                                            metaInfo[updateColumn.Key] = JsonConvert.SerializeObject(value);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Choice option not exist");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var category in template.categories)
            {
                foreach (var templateColumn in category.columns)
                {
                    if (metaInfo.ContainsKey(templateColumn.uniqueId.ToString()) && templateColumn.required && string.IsNullOrEmpty(metaInfo[templateColumn.uniqueId.ToString()]))
                    {
                        throw new Exception("Required column value is null");
                    }
                }
            }
        }

        public List<int> GetPhysicalObjectPermissionIds(List<Guid> nodeIds)
        {
            var nodes = ExplorerDao.GetRecordByIds(nodeIds);
            return nodes.Select(o => o.ScopePermissionId).Distinct().ToList();
        }
        private void CheckHoldInfo(UpdateHoldDto dto)
        {
            if (string.IsNullOrEmpty(dto.HoldSetting.Name.Trim()))
            {
                throw new Exception("Hold title cannot be empty.");
            }
        }

        private void CheckHoldSetting(UpdateHoldDto dto)
        {
            if (dto.HoldSetting == null || string.IsNullOrEmpty(dto.HoldSetting.Id) || string.IsNullOrEmpty(dto.HoldSetting.Name))
            {
                throw new Exception("Please check the hold setting");
            }
            var existingHold = HoldDao.GetHoldById(dto.HoldSetting.Id);
            if (existingHold == null || !existingHold.Name.Equals(dto.HoldSetting.Name)) 
            {
                throw new Exception("Hold setting is not exist, please check the hold setting.");
            }
        }

        public async System.Threading.Tasks.Task ConvertDateTimeColumnValueTimeZoneAsync(PhysicalObjectDto dto)
        {
            var allColumnsDic = new Dictionary<Guid, TemplateColumnDto>();
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (dto.MetaInfo == null)
            {
                return;
            }
            dto.Template.categories.ForEach(g => g.columns.ForEach(c =>
            {
                if (!allColumnsDic.ContainsKey(c.uniqueId))
                {
                    allColumnsDic.Add(c.uniqueId, c);
                }
            }));

            List<KeyValuePair<string, string>> needChangeDateTimeValue = new List<KeyValuePair<string, string>>();

            foreach (var key in dto.MetaInfo.Keys)
            {
                try
                {
                    TemplateColumnDto tempColumn = null;
                    if (allColumnsDic.TryGetValue(Guid.Parse(key), out tempColumn))
                    {
                        if (tempColumn.typeId == (int)AvePoint.RA.Contract.TemplateManagement.ColumnType.DateTime)
                        {
                            var metaInfoValue = dto.MetaInfo[key];
                            if (string.IsNullOrEmpty(metaInfoValue))
                            {
                                continue;
                            }
                            var tempDateTimeColumnValue = JsonConvert.DeserializeObject<DateTimeColumnValue>(metaInfoValue);
                            if (tempDateTimeColumnValue.TimeZoneId == gls.TimeZoneId && tempDateTimeColumnValue.IsSetDayLight == gls.DayLight)
                            {
                                continue;
                            }
                            var columnUTCDate = tempDateTimeColumnValue.GetUtcDate();
                            var glsTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
                            var glsTimeZoneDateTime = DateTimeUtil.ConvertTimeFromUtc(columnUTCDate, gls);
                            if (glsTimeZoneDateTime.Kind == DateTimeKind.Utc)
                            {
                                glsTimeZoneDateTime = DateTime.SpecifyKind(glsTimeZoneDateTime, DateTimeKind.Unspecified);
                            }
                            tempDateTimeColumnValue.Date = glsTimeZoneDateTime;
                            tempDateTimeColumnValue.TimeZoneId = gls.TimeZoneId;
                            tempDateTimeColumnValue.IsSetDayLight = gls.DayLight;
                            needChangeDateTimeValue.Add(new KeyValuePair<string, string>(key, JsonConvert.SerializeObject(tempDateTimeColumnValue)));
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Convert one PhyDto DateTimeColumnValueTimeZone Errorr {e}");
                }
            }
            if (needChangeDateTimeValue.Count > 0)
            {
                foreach (var change in needChangeDateTimeValue)
                {
                    dto.MetaInfo[change.Key] = change.Value;
                }
            }
        }

        #endregion
        #endregion

        #region fs records
        public RAReturnMessage AddOrUpdateFileSystemObject(FileSystemRecordDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            Record record = ConvertUtil.ConvertFSDtoToRMBaseRecord(dto);
            try
            {
                _explorerDao = new ExplorerDao(true);

                UpdateFSDestroyedTime(record);
                var addSucceed = true;
                var updateSucceed = true;
                logger.Info($"Add record : {record?.Id} to db.");
                Record dbRecord = null;
                using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.ReadById"))
                {
                    ArgumentCheck.NotNull(record, nameof(record));
                    dbRecord = _explorerDao.ReadById(record.ScopeId, record.Id);
                }
                if (dbRecord == null)
                {
                    using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.AddFileSystemRecord"))
                    {
                        addSucceed = _explorerDao.AddFileSystemRecord(record);
                    }
                }
                else
                {
                    using (var scope = new PerformanceScope("AddOrUpdateFileSystemObject.UpdateFileSystemRecord"))
                    {
                        updateSucceed = _explorerDao.UpdateFileSystemRecord(record, true);
                    }
                }
                if (!addSucceed || !updateSucceed)
                {
                    logger.Error("Error occured when add or update fs record to cosmos db");
                    msg.MessageType = RAMessageType.Exception;
                }
                logger.Info($"Finish adding record : {record?.Id} to db.");

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is CosmosException && ((CosmosException)ex.InnerException).StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    logger.Warn($"Exception Code is Conflict, will try to add record again");
                    _explorerDao = new ExplorerDao(true);
                    if (_explorerDao != null)
                    {
                        try
                        {
                            _explorerDao.AddFileSystemRecord(record);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException != null && e.InnerException is CosmosException && ((CosmosException)e.InnerException).StatusCode == System.Net.HttpStatusCode.Conflict)
                            {
                                logger.Error($"Retry add record failed, still conflict [{ex.ToString()}]");
                                msg.MessageType = RAMessageType.Exception;
                            }
                            else
                            {
                                logger.Error($"Retry add record failed: [{ex.ToString()}]");
                                msg.MessageType = RAMessageType.Failed;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to connect to cosmos db.");
                    }
                }
                else
                {
                    logger.Error($"Error in AddOrUpdateFSObject : [{ex.ToString()}]");
                    msg.MessageType = RAMessageType.Failed;
                }
            }
            return msg;
        }

        public string GetFSRecordId(int nodeType, Guid nodeId, Guid scopeId)
        {
            string recordsId = string.Empty;
            try
            {
                _explorerDao = new ExplorerDao(true);
                if (_explorerDao != null)
                {
                    var result = ExplorerDao.ReadById(scopeId, nodeId);
                    if (result != null)
                    {
                        recordsId = result.RecordsId;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while getting fs records id.NodeId:{nodeId} Error:{e.ToString()}");
                throw;
            }
            return recordsId;
        }

        public string GetFSConnectionIdByItemId(Guid nodeId)
        {
            return ExplorerDao.GetFSConnectionIdByItemId(nodeId);
        }


        public List<FileSystemRecordDto> GetFileSystemObjectByGuids(List<Guid> nodeIds)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            try
            {
                _explorerDao = new ExplorerDao(true);
                _explorerDao.QueryAll(n => nodeIds.Contains(n.NodeId) && n.RecordStatus == (int)RMRecordStatus.Active).ToList().ForEach(r =>
                {
                    dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
                });
                logger.Info("Get fs records successfully.");
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting fs records. Error: " + e.ToString());
            }
            return dtos;
        }

        public bool UpdateFSFolderSize(List<FolderSizeUpdateDto> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                logger.Info("UpdateFSFolderSize: dtos is null or empty, skip update.");
                return true;
            }

            _explorerDao = new ExplorerDao(true);
            try
            {
                var pathDeletedMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

                foreach (var dto in dtos)
                {
                    if (string.IsNullOrWhiteSpace(dto.RootFolderPath) ||
                        string.IsNullOrWhiteSpace(dto.FolderPath))
                    {
                        logger.Warn($"Invalid DTO detected. RootPath: {dto.RootFolderPath}, FolderPath: {dto.FolderPath}");
                        continue;
                    }

                    var parentPaths = GetAllParentPaths(dto.RootFolderPath, dto.FolderPath);

                    foreach (var path in parentPaths)
                    {
                        var normalizedPath = NormalizePath(path);

                        if (!pathDeletedMap.ContainsKey(normalizedPath))
                            pathDeletedMap[normalizedPath] = 0;

                        pathDeletedMap[normalizedPath] += dto.DeletedBytes;
                    }
                }

                if (!pathDeletedMap.Any())
                {
                    logger.Info("No valid paths to update.");
                    return true;
                }

                var allPaths = pathDeletedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                var needUpdateRecords = _explorerDao.QueryAll(r =>
                    allPaths.Contains(r.DirPath + "\\" + r.LeafName)
                ).ToList();

                if (!needUpdateRecords.Any())
                {
                    logger.Warn("No matching records found in DB for update.");
                    return true;
                }

                long totalBefore = 0;
                long totalAfter = 0;

                foreach (var r in needUpdateRecords)
                {
                    var fullPath = NormalizePath(r.DirPath + "\\" + r.LeafName);

                    if (!pathDeletedMap.TryGetValue(fullPath, out var deletedBytes))
                        continue;

                    var before = r.JPMCFSFileSize;
                    var after = before - deletedBytes;

                    if (after < 0)
                    {
                        logger.Warn($"Negative size detected. Folder: {fullPath}, Before: {before}, Deleted: {deletedBytes}");
                        after = 0;
                    }

                    r.JPMCFSFileSize = after;

                    totalBefore += before;
                    totalAfter += after;

                    logger.Info($"[FS SIZE UPDATE] Path: {fullPath} | Before: {before} | Deleted: {deletedBytes} | After: {after}");
                }

                var updatedIds = _explorerDao.BatchUpdate(needUpdateRecords, 5);

                logger.Info($"UpdateFSFolderSize SUCCESS. Updated: {updatedIds.Count} folders | TotalBefore: {totalBefore} | TotalAfter: {totalAfter}");

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"UpdateFSFolderSize FAILED. Error: {ex}");
                return false;
            }
        }

        private string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return path
                .Replace("/", "\\")
                .TrimEnd('\\');
        }

        private static List<string> GetAllParentPaths(string rootPath, string folderPath)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(folderPath))
                return result;

            rootPath = rootPath.TrimEnd('\\');
            var current = folderPath.TrimEnd('\\');

            while (!string.IsNullOrEmpty(current) &&
                   current.StartsWith(rootPath))
            {
                result.Add(current);

                if (current.Equals(rootPath))
                    break;

                var lastIndex = current.LastIndexOf('\\');

                if (lastIndex <= rootPath.Length)
                {
                    current = rootPath;
                }
                else
                {
                    current = current.Substring(0, lastIndex);
                }
            }

            return result;
        }

        public List<Guid> UpdateFSDeleteRecord(List<FSExplorerDeleteDto> dtos)
        {
            if (dtos == null)
            {
                return new List<Guid>(); 
            }
            List<Guid> ids = new List<Guid>();
            _explorerDao = new ExplorerDao(true);
            Guid scopeId = dtos[0].ConnectionId;
            List<Guid> movedRecordIds = dtos.Where(r => r.RecordStatus == (int)RMRecordStatus.Moved).Select(r => r.Id).ToList();
            if (movedRecordIds != null && movedRecordIds.Count > 0)
            {
                //_explorerDao.UpdateAll(r => r.ScopeId == scopeId && movedRecords.Contains(r.Id), rec => { rec.RecordStatus = (int)RMRecordStatus.Moved; rec.DestroyedTime = DateTime.UtcNow.Ticks; });
                var movedRecords = _explorerDao.QueryAll(r => movedRecordIds.Contains(r.Id)).ToList();
                movedRecords.ForEach(r =>
                {
                    r.RecordStatus = (int)RMRecordStatus.Moved;
                    r.DestroyedTime = DateTime.UtcNow.Ticks;
                });
                var failedIds =_explorerDao.BatchUpdate(movedRecords, 5);
                ids.AddRange(failedIds);
            }
            List<Guid> destroyedRecordIds = dtos.Where(r => r.RecordStatus == (int)RMRecordStatus.Destroyed).Select(r => r.Id).ToList();
            if (destroyedRecordIds != null && destroyedRecordIds.Count > 0)
            {
                //_explorerDao.UpdateAll(r => r.ScopeId == scopeId && destroyedRecords.Contains(r.Id), rec => { rec.RecordStatus = (int)RMRecordStatus.Destroyed; rec.DestroyedTime = DateTime.UtcNow.Ticks; });

                var destroyedRecords = _explorerDao.QueryAll(r => destroyedRecordIds.Contains(r.Id)).ToList();
                destroyedRecords.ForEach(r =>
                {
                    r.RecordStatus = (int)RMRecordStatus.Destroyed;
                    r.DestroyedTime = DateTime.UtcNow.Ticks;
                });
                var failedIds = _explorerDao.BatchUpdate(destroyedRecords, 5);
                ids.AddRange(failedIds);
            }
            //foreach (var dto in dtos)
            //{
            //    try
            //    {
            //        _explorerDao.UpdateFSDeleteRecord(dto.Id, dto.ConnectionId, dto.RecordStatus);
            //    }
            //    catch (Exception e)
            //    {
            //        logger.Debug("An error occurred while updating deleted records for fs. Id:{0} Error:{1}", dto?.Id, e.ToString());
            //        ids.Add(dto.Id);
            //    }
            //}
            logger.Info("Update deleted fs records successfully.");
            return ids;
        }

        /// <summary>
        /// 获取TreeJson数据字符串前台分页
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="treeNodeId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public string GetFSTreeData(int treeNodeType, string treeNodeId)
        {
            string strResult = string.Empty;
            switch (treeNodeType)
            {
                case (int)FSTreeType.Root:
                case (int)FSTreeType.ConnGroup:
                case (int)FSTreeType.Folder:
                    strResult = GetJsonStrByObj(ExplorerDao.GetFSChildNodes(new Guid(treeNodeId), treeNodeType));
                    break;
                case (int)FSTreeType.None:
                    strResult = GetJsonStrByObj(ExplorerDao.GetFSRootNode());
                    break;
            }
            return strResult;
        }

        private void UpdateFSDestroyedTime(Record uiRecord)
        {
            if (uiRecord.RecordStatus == (int)RMRecordStatus.Destroyed)
            {
                var dbRecord = ExplorerDao.GetFSRecord(uiRecord.ScopeId, uiRecord.Id);
                if (dbRecord == null)
                {
                    uiRecord.DestroyedTime = DateTime.UtcNow.Ticks;
                }
                else
                {
                    uiRecord.DestroyedTime = dbRecord.DestroyedTime == 0 ? DateTime.UtcNow.Ticks : dbRecord.DestroyedTime;
                }
            }
        }

        public List<FSFolderCacheDto> GetExplorerDataByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            List<FSFolderCacheDto> dtos = new List<FSFolderCacheDto>();
            ExplorerDao.GetExplorerDataByFolder(folderId, scopeId, sortTicks, pageSize).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertExplorerData2FSFolderCacheDto(r));
            });
            return dtos;
        }

        public List<FileSystemRecordDto> GetDBRecordsByFolder(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetExplorerDataByFolder(folderId, scopeId, sortTicks, pageSize).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }

        public bool HasFileMatchTerm(string dirPath, string scopeId, List<Guid> classCodeIds)
        {
            return ExplorerDao.HasFileMatchTerm(dirPath, scopeId, classCodeIds);
        }

        public List<FileSystemRecordDto> GetDBRecordsByFolderAndEndTime(string folderId, string scopeId, long sortTicks, int pageSize)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetExplorerDataByFolderAndEndTime(folderId, scopeId, sortTicks, pageSize).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }

        public List<FileSystemRecordDto> GetDBRecordsByNodeIds(List<Guid> nodeIds, string scopeId, long sortTicks)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetExplorerDataByNodeIds(nodeIds, scopeId, sortTicks).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }
        public List<FileSystemRecordDto> GetDBRecordsByNodeIdsAndEndTime(List<Guid> nodeIds, string scopeId, long sortTicks)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetExplorerDataByNodeIdsAndEndTime(nodeIds, scopeId, sortTicks).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }
        public List<FileSystemRecordDto> GetDBRecordsByClassCodeAndFilterByEndTime(IEnumerable<Guid> nodeIds, IEnumerable<Guid> classCodeIds, string scopeId, long sortTicks)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetDBRecordsByClassCodeAndFilterByEndTime(nodeIds, classCodeIds, scopeId, sortTicks).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }
        /// <summary>
        /// 获取和Parent Term不同的子Folder的基本信息
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="termId"></param>
        /// <returns></returns>
        public List<FSFolderCacheDto> GetDifferentTermDBRecordsByFolder(string folderId, string termId)
        {
            List<FSFolderCacheDto> dtos = new List<FSFolderCacheDto>();
            var result = ExplorerDao.QueryAll(s => s.SourceFlag == (int)SourceFlag.FileSystem
            && (s.NodeType == (int)NodeLevel.FSFolder
            && s.RecordStatus == (int)RMRecordStatus.Active
            && s.ParentId == (new Guid(folderId))
            && s.TermId != Guid.Empty && s.TermId != (new Guid(termId))));
            dtos.AddRange(result.Select(a => new FSFolderCacheDto() { Id = a.NodeId, TermId = a.TermId, TermName = a.TermName }));
            return dtos;
        }
        public List<FileSystemRecordDto> GetFSDBRecords(List<Guid> ids)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetRecordByIds(ids).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }

        public List<FileSystemRecordDto> GetFSConnectionUnderGroup(Guid connectionGroupId, int level)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetFSConnectionUnderGroup(connectionGroupId, level).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });

            return dtos;
        }

        public List<FileSystemRecordDto> GetFSDBRecordsByRecordsId(List<string> recordsId)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.GetRecordByRecordsIds(recordsId).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }

        public List<FsRecordProcessDto> GetFSRecordsForAdsProcessing(List<string> recordsId)
        {
            return ExplorerDao.GetRecordByRecordsIds(recordsId)
                .Select(ConvertUtil.ConvertRecordToFsRecordProcessDto)
                .ToList();
        }

        public List<FileSystemRecordDto> GetFSManualRecords(List<Guid> ids)
        {
            List<FileSystemRecordDto> dtos = new List<FileSystemRecordDto>();
            ExplorerDao.QueryAll(r => ids.Contains(r.NodeId) && r.IsManualSynced && r.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd).ToList().ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            });
            return dtos;
        }

        public FSDueRecordsDto GetFSDueRecords(SearchFilterParam searchFilterParam)
        {
            FSDueRecordsDto dto = new FSDueRecordsDto();
            Tuple<IEnumerable<Record>, string> result = null;
            if(searchFilterParam.DueDate == 0L)
            {
                logger.Info("due date empty, query folders.");
                int sourceFlag = (int)SourceFlag.FileSystem;
                int nodeType = (int)(int)NodeLevel.FSFolder;
                Guid scopeId = new Guid(searchFilterParam.ScopeId);
                Guid folderId = searchFilterParam.FolderId; 
                //DueDate 是0, 获取Folders， （Term不同，或者Term为空）或者 Hold的
                if (searchFilterParam.Filter != null && !string.IsNullOrEmpty(searchFilterParam.Filter.SearchScope))
                {
                    logger.Info($"due date empty, query folders,the scope is not null ,searchFilterParam.Filter.SearchScope:{searchFilterParam.Filter.SearchScope}");
                    string sql = "select * from Record r where (r.termId != @termId1 or r.termId =@termId2 or r.holdStatus) and r.sourceFlag = @sourceFlag and r.nodeType = @nodeType and r.scopeId = @scopeId and (r.nodeId = @folderId or StartsWith(r.dirPath, @searchScope, true))";
                    
                    QueryDefinition dq = new QueryDefinition(sql);
                    dq = dq.WithParameter("@termId1", searchFilterParam.TermId).WithParameter("@termId2", Guid.Empty).WithParameter("@sourceFlag", sourceFlag)
                        .WithParameter("@nodeType", nodeType).WithParameter("@scopeId", scopeId).WithParameter("@searchScope", searchFilterParam.Filter.SearchScope).WithParameter("@folderId", folderId);
                    result = ExplorerDao.QueryPageBySql(dq, searchFilterParam.PageInfo.PageSize, searchFilterParam.PageInfo.PageIndex);
                    //result = ExplorerDao.QueryByPage(a => (a.TermId != searchFilterParam.TermId || a.TermId == Guid.Empty || a.HoldStatus)
                    //        && a.SourceFlag == sourceFlag && a.NodeType == nodeType && a.ScopeId == scopeId 
                    //        && a.DirPath.StartsWith(searchFilterParam.Filter.SearchScope, true, System.Globalization.CultureInfo.InvariantCulture), searchFilterParam.PageInfo.PageSize, searchFilterParam.PageInfo.PageIndex, false);
                }
                else
                {
                    logger.Info($"due date empty, query folders,the scope is null ,scopeid:{scopeId},nodeType:{nodeType},sourceFlag:{sourceFlag}");
                    result = ExplorerDao.QueryByPage(a => (a.TermId != searchFilterParam.TermId || a.TermId == Guid.Empty || a.HoldStatus)
                            && a.SourceFlag == sourceFlag && a.NodeType == nodeType && a.ScopeId == scopeId, searchFilterParam.PageInfo.PageSize, searchFilterParam.PageInfo.PageIndex, false);
                }
            }
            else
            {
                logger.Info($"due date not empty,searchFilterParam.DueDate:{searchFilterParam.DueDate}");
                result = ExplorerDao.QueryDueRecordsByPage(searchFilterParam);
            }

            if (result != null && result.Item1 != null && result.Item1.Count() > 0)
            {
                dto.Records = result.Item1.ToList().ConvertAll(r => ConvertUtil.ConvertRMBaseRecordToFSDto(r));
            }
            else
            {
                dto.Records = new List<FileSystemRecordDto>();
            }

            dto.PageInfo = new SearchPageInfo()
            {
                PageIndex = result != null ? result.Item2 : null,
                HasNextPage = result != null ? !string.IsNullOrWhiteSpace(result.Item2) : false
            };

            return dto;
        }

        public List<OnPremiseSPListCacheDto> GetOnPremiseSPExplorerDataByListId(string listId, string scopeId, long sortTicks, int pageSize)
        {
            List<OnPremiseSPListCacheDto> dtos = new List<OnPremiseSPListCacheDto>();
            ExplorerDao.GetOnPremiseSPExplorerDataByListId(listId, scopeId, sortTicks, pageSize).ForEach(r =>
            {
                dtos.Add(ConvertUtil.ConvertExplorerData2OnPremiseSPListCacheDto(r));
            });
            return dtos;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunFSDashboardJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealStartFSDashBoard(JobRunBy JobRunType)
        {
            logger.Info($"Run RealExportBarcode");
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                jobId = RMJobService.CreateJob(JobType.FSDashBoard, jobRunByUser);
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = JobRunType,
                    JobType = AvePoint.RA.Contract.JobMonitor.JobType.FSDashBoard,
                    //TODO Add export path in {2}
                    CommandLine = string.Format("{0} {1}", AvePoint.RA.Contract.JobMonitor.JobType.FSDashBoard, jobId),
                });
                logger.Info($"run physical Export Barcode job success, JobId : {jobId}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in RealExportBarcode, reason : {ex.ToString()}.");
            }
            return jobId;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.FSMyhub, Action = AuditAction.FSMyHubDashboard, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        [FSAudit(AuditType = FSAuditType.RunFSDashboardJob, AuditHandler = typeof(FileSystemServiceAuditHandler))]
        public string RealStartFSMyHubDashBoard(JobRunBy runBy, string param)
        {
            logger.Info($"Run RealExportBarcode");
            var jobId = string.Empty;

            try
            {
                var connectionId = string.IsNullOrEmpty(param)
                    ? string.Empty
                    : System.Xml.Linq.XDocument.Parse(param)
                        .Descendants()
                        .FirstOrDefault(e => e.Name.LocalName == "PartitionKeyId")
                        ?.Value ?? string.Empty;

                var hasRunningJob = JobMonitorService.HasRunningFSSyncDataJobAsync(connectionId).GetAwaiter().GetResult();
                if (hasRunningJob)
                {
                    logger.Warn("A running FS MyHub dashboard job already exists. Skipping job creation.");
                    return jobId;
                }

                //create job
                var username = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var account = AccountDao.GetActiveUserByNameAsync(username).GetAwaiter().GetResult();
                jobId = RMJobService.CreateJob(JobType.FSMyHubDashboard, username, account.UserId);

                var extensionJson = JobMonitorDao.GetJobById(jobId)?.Extension;
                var extensionObject = string.IsNullOrWhiteSpace(extensionJson)
                    ? new JObject()
                    : JObject.Parse(extensionJson);
                extensionObject["connectionId"] = connectionId;

                RMJobService.UpdateJobExtensionById(jobId, extensionObject.ToString(Formatting.None));

                logger.Info($"Real run dashboard job: [{jobId}]");
                mJobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.FSMyHubDashboard,
                    RunBy = runBy,
                    CommandLine = string.Format("{0} {1} {2}", JobType.FSMyHubDashboard, jobId, runBy),
                    Extension = param,
                });
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run FS MyHub dashboard job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunSPOnPremDashboardJob, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public string RealStartSPOnPremDashBoard(JobRunBy jobRunBy)
        {
            logger.Info("Run sharepoint on-prem real time export dashboard.");
            var jobId = string.Empty;
            var jobRunByUser = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";

            try
            {
                jobId = RMJobService.CreateJob(JobType.SPOnPremDashBoard, jobRunByUser);
                mJobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    RunBy = jobRunBy,
                    JobType = JobType.SPOnPremDashBoard,
                    CommandLine = $"{JobType.SPOnPremDashBoard} {jobId}"
                });
                logger.Info($"Successful real run sharepoint on-prem export dashboard. JobId: [{jobId}]");
            }
            catch (Exception e)
            {
                logger.Info($"An error occurred while real run sharepoint on-prem export dashboard. Error: {e}");
            }

            return jobId;
        }

        #endregion

        public async Task<List<string>> GetRecordReleaseTimeAsync(List<Guid> recordIds)
        {
            List<string> result = new List<string>();
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            var allRecords = ExplorerDao.GetRecordByIds(recordIds);
            int allianceType = -1;
            foreach (var e in allRecords)
            {
                if (allianceType == -1)
                {
                    if (e.SourceFlag == (int)SourceFlag.Physical)
                    {
                        allianceType = RecordsConstants.RecordHold_PhyProfile;
                    }
                    else
                    {
                        allianceType = RecordsConstants.RecordHold_Electronic;
                    }
                }

                if (e != null && e.HoldStatus)
                {
                    result.Add(mGeneralSettingService.ConvertTiksToDateTime(gls, e.HoldReleaseTime, true).SimplifyFormatTime);
                }
                else
                {
                    result.Add("");
                }
            }
            return result;
        }

        public bool IsFolderHasParentHold(List<Guid> recordIds, out List<string> holdingBoxes)
        {
            var records = ExplorerDao.GetRecordByIds(recordIds);
            var boxIds = records.Where(r => r.NodeType == (int)RMNodeType.PhyFile).Select(f => f.BoxId).ToList();
            var boxAls = ExplorerDao.GetHoldRecordsByIds(boxIds);
            var holdingBoxesId = boxAls.Select(b => b.Id).ToList();
            holdingBoxes = ExplorerDao.GetRecordByIds(holdingBoxesId).Select(b => b.LeafName).ToList();
            return boxAls.Count > 0;
        }

        #region sp on premise
        public RAReturnMessage AddOrUpdateSPOnPremObject(RecordDto dto)
        {
            RAReturnMessage msg = new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                _explorerDao = new ExplorerDao(true);
                if (dto != null)
                {
                    _explorerDao.AddOrUpdateRecord(ConvertUtil.ConvertRecordDto2Record(dto), false);
                    logger.Info($"Finish adding record : {dto?.Id} to db.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is CosmosException && ((CosmosException)ex.InnerException).StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    logger.Warn($"Exception Code is Conflict, will try to add on premise record again");
                    _explorerDao = new ExplorerDao(true);
                    try
                    {
                        _explorerDao.AddOrUpdateRecord(ConvertUtil.ConvertRecordDto2Record(dto), false);
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException != null && e.InnerException is CosmosException && ((CosmosException)e.InnerException).StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            logger.Error($"Retry add on premise record failed, still conflict [{ex.ToString()}]");
                            msg.MessageType = RAMessageType.Exception;
                        }
                        else
                        {
                            logger.Error($"Retry add on premise record failed: [{ex.ToString()}]");
                            msg.MessageType = RAMessageType.Failed;
                        }
                    }
                }
                else
                {
                    logger.Error($"Error in AddOrUpdateSPOnPremObject : [{ex.ToString()}]");
                    msg.MessageType = RAMessageType.Failed;
                }
            }
            return msg;
        }

        public bool IsSPOnPremObjectExist(Guid scopeId, Guid id)
        {
            _explorerDao = new ExplorerDao(true);
            var record = _explorerDao.ReadById(scopeId, id);
            if (null == record)
            {
                return false;
            }
            return record != null
                && record.SourceFlag == (int)SourceFlag.SharePointOnPrem
                && record.RecordStatus == (int)RMRecordStatus.Active;
        }

        public bool CheckIsHoldRecord(Guid Id)
        {
            _explorerDao = new ExplorerDao(true);
            var records = _explorerDao.GetRecordByIds(new List<Guid>() { Id });
            if (records != null && records.Count > 0)
            {
                var record = records.FirstOrDefault();
                return record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks;
            }
            else
            {
                return false;
            }
        }
        public List<Guid> OnPremiseSPUpdateRecordsInExplorer(List<OnPremiseSPAzureTableEntityDto> dtos)
        {
            List<Guid> ids = new List<Guid>();
            _explorerDao = new ExplorerDao(true);
            foreach (var dto in dtos)
            {
                if (dto == null)
                {
                    logger.Warn("dto is null continue");
                    continue;
                }
                try
                {
                    _explorerDao.UpdateFSDeleteRecord(dto.Id, new Guid(dto.SiteId), dto.ExplorerStatus);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while updating deleted records for OnPremiseSP. Id:{0} Error:{1}", dto?.Id, e.ToString());
                    ids.Add(dto.Id);
                }
            }
            logger.Info("Update deleted OnPremiseSP records successfully.");
            return ids;
        }
        #endregion

        [Audit(Action = AuditAction.SaveBarcodeStandard, Category = AuditCategory.PhysicalRecordsExplorer, Module = AuditModule.PhysicalRecordManagement, AfterHandler = typeof(PhysicalObjectAfterAuditHandler), BeforeHandler = typeof(PhysicalObjectBeforeAuditHandler))]
        public async Task<bool> SaveBarcodeStandardAsync(int barcodeType)
        {
            var keyValue = new RMKeyValue()
            {
                Key = BARCODE_STANDARD_KEY,
                Value = barcodeType.ToString(),
            };
            var result = await RMKeyValueDao.SaveOrUpdateAsync(keyValue);
            if (result)
            {
                await Cache.RemoveAsync(BARCODE_STANDARD_KEY);
                await Cache.SetAsync(BARCODE_STANDARD_KEY, keyValue.Value);
            }
            return result;
        }

        public async Task<int> GetBarcodeStandardAsync()
        {
            var result = RMKeyValueDao.GetValueByKey(BARCODE_STANDARD_KEY);
            if (result == null)
            {
                var keyValue = new RMKeyValue()
                {
                    Key = BARCODE_STANDARD_KEY,
                    Value = "0",
                };
                var saveResult = await RMKeyValueDao.SaveOrUpdateAsync(keyValue);
                if (saveResult)
                {
                    await Cache.SetAsync(BARCODE_STANDARD_KEY, keyValue.Value);
                }
                return 0;
            }
            if (string.IsNullOrEmpty(await Cache.GetAsync<string>(BARCODE_STANDARD_KEY)))
            {
                await Cache.SetAsync(BARCODE_STANDARD_KEY, result.Value);
            }
            return int.Parse(result.Value);
        }

        #region FS JPMC
        public List<FileSystemRecordDto> QueryFileSystemRecords(string aveSiteId, List<Guid> ids)
        {
            return ExplorerDao.QueryJPMCRecords((int)SourceFlag.FileSystem, aveSiteId, ids).Select(ConvertUtil.ConvertRMBaseRecordToFSDto).ToList();
        }
        
        public List<FsRecordProcessDto> QueryFileSystemRecords(string aveSiteId, List<string> ids)
        {
            return ExplorerDao.QueryJPMCRecords((int)SourceFlag.FileSystem, aveSiteId, ids).Select(ConvertUtil.ConvertRecordToFsRecordProcessDto).ToList();
        }

        public bool HasJPMCConnectionRecord(string connectionId)
        {
            return ExplorerDao.HasJPMCConnectionRecord((int)SourceFlag.FileSystem, connectionId);
        }

        #endregion

        #region Maestro AI
        public bool ResetMARecordsForRemovedMLTerms(List<Guid> predictTermIds)
        {
            try
            {
                if (predictTermIds == null || predictTermIds.Count == 0) return false;
                return ExplorerDao.ResetMARecordsForRemovedMLTerms(predictTermIds) > 0;
            }
            catch (Exception e)
            {
                logger.Error($"Reset MA records for removed ML terms {string.Join(",", predictTermIds)} has error: {e}");
                return false;
            }
        }

        public async Task BuildHoldNotificationScheduleJob(UpdateHoldDto dto)
        {
            if(dto.HoldSetting.EmailNotification?.IsEnabled == true)
            {
                logger.Error($"Save hold sucessfully, create hold notification schedue job");
                await ScheduleService.CreateScheduleNotificationAsync(ScheduleType.HoldNotificationSchedule);
            }
        }

        public async Task<ExplorerResultInfo> SearchPhysicalRecordsAsync(string pageIndex, int pageSize, string value)
        {
            var normalizedPageIndex = NormalizeContinuationToken(pageIndex);
            var rst = new ExplorerResultInfo
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = normalizedPageIndex,
                    PageSize = pageSize
                }
            };
            var accountMap = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            var recT = await SearchRecordsForRelatedAsync(BuildFuzzySearchKey(value), pageIndex, pageSize);
            var list = recT.Item1.ToList();
            var datas = list.ConvertAll(e =>
            {
                var recordDto = ConvertUtil.ConvertToBaseRecordDto(e, accountMap);
                if (e.SourceFlag == (int)SourceFlag.Physical)
                {
                    var physicalObjectDto = ConvertUtil.ConvertRMBaseRecordToPhysical(e);
                    SetPhysicalObjectHoldStatus(recordDto, physicalObjectDto);
                    SetPhysicalRcordFile(null, recordDto, physicalObjectDto);
                }
                SetRuleInfos(recordDto);
                SetObjectType(recordDto);
                recordDto.FullPath = GetPhysicalObjectFullPath(recordDto.Id);
                return recordDto;
            });
            rst.Datas = datas;
            rst.PagingInfo.HasNextPage = !string.IsNullOrEmpty(recT.Item2);
            rst.PagingInfo.PageIndex = recT.Item2;
            rst.PagingInfo.Total = recT.Item3;
            return rst;
        }


        private async Task<Tuple<IEnumerable<Record>, string, int>> SearchRecordsForRelatedAsync(string searchKey, string pageIndex, int pageSize)
        {
            ExplorerQueryV2Dto queryDto = new ExplorerQueryV2Dto()
            {
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2
                    {
                        SourceFlags = new List<SourceFlag> { SourceFlag.Physical },
                        NodeTypes = new List<RMNodeLevel> { RMNodeLevel.PhysicalRecord, RMNodeLevel.PhysicalFile, RMNodeLevel.Item },
                        Status = new List<RMRecordStatus> { RMRecordStatus.Active },
                        DeclaredRecord = false,
                    },
                    SearchOption = string.IsNullOrEmpty(searchKey) ? null : new ExplorerSearchOptionV2()
                    {
                        Key = searchKey,
                        Columns = new List<ExplorerQueryColumn>
                        {
                            new ExplorerQueryColumn {  Id = DefaultColumnIDs.UniqueId },
                            new ExplorerQueryColumn { Id = DefaultColumnIDs.NameOrTitle },
                        }
                    }
                },
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };
            Tuple<IEnumerable<Record>, string> result = null;
            var totalCount = 0;
            try
            {
                await ExplorerQueryParamProcesser.ProcessAsync(queryDto.QueryOption);
                //remove term permission filter
                //ExplorerQueryService.ProcessWithoutNodeTypeParam(queryDto.QueryOption.FilterOption);
                result = ExplorerDao.SearchRecordsV2(queryDto);
                foreach (Record rec in result.Item1)
                {
                    rec.AppendMetaInfoForOldLogic();
                }
                totalCount = ExplorerDao.QueryCount(queryDto);
            }
            catch (ExplorerQueryNoPermissionException e)
            {
                logger.Warn("No permission to access data in search data for related. ERROR:{0}", e.ToString());
                result = new Tuple<IEnumerable<Record>, string>(new List<Record>(), string.Empty);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while query data for related, ERROR:{0}", ex.ToString());
                result = new Tuple<IEnumerable<Record>, string>(new List<Record>(), string.Empty);
            }
            return new Tuple<IEnumerable<Record>, string, int>(result.Item1, result.Item2, totalCount);
        }

        private static string BuildFuzzySearchKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.Equals("*", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return $"*{trimmedValue}*";
        }

        private static string NormalizeContinuationToken(string pageIndex)
        {
            if (string.IsNullOrWhiteSpace(pageIndex))
            {
                return string.Empty;
            }

            var trimmedPageIndex = pageIndex.Trim();
            return int.TryParse(trimmedPageIndex, out _) ? string.Empty : trimmedPageIndex;
        }

        public RAReturnMessage PhysicalMoves(List<PhysicalMoveRequest> moveRequests)
        {
            var msg = new RAReturnMessage();
            ListenerPocessStart();
            string jobId = string.Empty;

            using (new PerformanceScope(string.Format("move.physical.move.send reuqest")))
            {
                MovePhysicalRecordsRequest(moveRequests, ref jobId);
            }
            msg.Extension = jobId;
            try
            {
                var allGuids = moveRequests.SelectMany(x => x.PhysicalMoveOption.SourcePhyRecordIds).Distinct().ToList();
                msg.Extsion1 = JsonConvert.SerializeObject(ExplorerDao.GetRecordByIds(allGuids).Select(r => r.LeafName).ToList());
            }
            catch (Exception e)
            {
                logger.Warn("get records name error");
            }
            return msg;
        }
        #endregion

    }

    public class HoldSpecialComparer : IComparer<string>
    {
        private List<string> HoldIds;
        public HoldSpecialComparer(List<string> holdIds)
        {
            HoldIds = holdIds;
        }
        public int Compare(string s1, string s2)
        {
            var idx1 = HoldIds.FindIndex(h => h.ToString() == s1);
            var idx2 = HoldIds.FindIndex(h => h.ToString() == s2);
            return idx1 - idx2;
        }
    }

    [Serializable]
    public class BarcodeDuplicateException : Exception
    {
        public BarcodeDuplicateException(string errorMsg)
    : base(errorMsg)
        {
        }
    }
}



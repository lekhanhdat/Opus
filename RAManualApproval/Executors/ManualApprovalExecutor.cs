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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Email;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Workflow;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.SharePoint.Archiver;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using RAManualApproval.ManualExceptions;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RAManualApproval.Executors
{
    public abstract class ManualApprovalExecutor
    {
        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected static readonly IManualApprovalService ManualApprovalService = PlatformWindsorManager.GetService<IManualApprovalService>();

        protected static readonly IExplorerDao ExplorerDao = new ExplorerDao();



        protected static readonly AzureTableConnectContract AzConnectContract = new DAOAPIClientV1().GetArchiverDataBaseConfigAsync().Result;

        protected static readonly string LocalAzConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        protected static readonly int PartitionKey = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

        private static readonly HistoryAddAction AddAction = new ();

        private static Dictionary<string, List<string>> disposalExtensionsDict = new ();

        public abstract SourceFlag Flag { get; }
        ///// <summary>
        ///// The priority when executed in the job for each data source.  retention should be the last one.
        ///// </summary>
        //public abstract int Priority { get; }

        protected abstract IEnumerable<List<ManualExportReportInfo>> GetManualApprovalReports();

        protected abstract Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo);

        protected abstract Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record);

        protected abstract Expression<Func<Record, bool>> GetQueryItemExpression(Record data);

        protected abstract Task MarkManualApprovalDataToExportedStatusAsync(Record item);

        protected abstract bool ProcessApprovedAndRejectedData(Record manualApproveData);

        protected abstract Task ProcessWorkflowSiteOwnersAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId);

        protected abstract Task ProcessWorkflowSPGroupAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step);

        protected abstract SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo);

        protected static readonly RMWorkflowProcessor s_workflowProcessor = new();

        private readonly RMEmailSender _emailSender;

        public ManualApprovalExecutor(RMEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                ManualApprovalJobManager.IncreaseBase(1000);

                ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessHistoryItemSucceed, ProcessHistoryItemFailed);
                using (new PerformanceScope($"Process approvaed and rejected datas by source: [{Flag}]."))
                {
                    ProcessApprovedAndRejectedDatas();
                    ManualApprovalDataSyncManager.Commit();
                }

                ManualApprovalDataSyncManager.RegisteProcessItemCallback(ProcessWaitingItemSucceedAsync, ProcessWaitingItemFailed);
                var reportInfoesList = GetManualApprovalReports();

                foreach (var reportInfoes in reportInfoesList)
                {
                    var reportInfoesNotIncludeDisposalExtendItems = reportInfoes.Where(item => 
                    {
                        
                      if(disposalExtensionsDict.ContainsKey(item.RuleID))
                      {
                            if (disposalExtensionsDict.GetValue(item.RuleID).Contains(item.NodeID.ToString())) 
                            {
                                Logger.Info($"disposalExtensionsDict is same{item.RuleID } and {item.NodeID} and {item.LeafName}");
                                return false;
                            }
                      }
                        Logger.Info($"{item.RuleID} and {item.LeafName}");
                        return true;

                    }).ToList();

                    Logger.Info($"Start process [{Flag}] manual approval. Need process reports count: [{reportInfoes.Count}].");

                    ManualApprovalJobManager.IncreaseBase(reportInfoes.Count);

                    await ProcessManualApprovalReportBatchAsync(reportInfoesNotIncludeDisposalExtendItems);
                    Logger.Info($"The [{Flag}] need to manual approval reports process end.");
                }
                ManualApprovalDataSyncManager.Commit();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute: [{Flag}] manual approval job. Error: {e}");
            }
            finally
            {
                ManualApprovalJobManager.Increase();
            }
        }

        private void ProcessApprovedAndRejectedDatas()
        {
            try
            {
                var datasInCosmosList = ManualApprovalDataSyncManager.GetAllApproveOrRejectedRecord(Flag);
                foreach(var datasInCosmos in datasInCosmosList)
                {
                    Logger.Info($"Start process [{Flag}] approved and rejected manual datas. Count: [{datasInCosmos.Count}].");
                    ManualApprovalJobManager.IncreaseBase(datasInCosmos.Count);

                    foreach (var manualApproveData in datasInCosmos)
                    {
                        try
                        {
                            //extend item
                            if (manualApproveData.ManualExtendTime >= DateTime.UtcNow.Ticks )
                            {
                                var ruleId = manualApproveData.RuleId.ToString();
                                var nodeId = manualApproveData.NodeId.ToString();

                                Logger.Info($"ProcessApprovedAndRejectedDatas Extend {ruleId}, {nodeId}.");

                                if (!disposalExtensionsDict.TryGetValue(ruleId, out var extensionsList))
                                {
                                    extensionsList = new List<string> { nodeId };
                                    disposalExtensionsDict.Add(ruleId, extensionsList);
                                }
                                else
                                {
                                    extensionsList.Add(nodeId);
                                }

                                continue;
                            }

                            var needNextStep = ProcessApprovedAndRejectedData(manualApproveData);

                            if (!needNextStep)
                            {
                                continue;
                            }

                           ManualApprovalDataSyncManager.Add(manualApproveData);
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"An error occurred while proccess manual approval data: [{manualApproveData.Id}]. Error: {e}");
                        }
                        finally
                        {
                            ManualApprovalJobManager.Increase();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process approved and rejected manual datas. Error: {e}");
            }
        }

        internal virtual async Task ProcessManualApprovalReportBatchAsync(List<ManualExportReportInfo> manualApprovalReports)
        {
            foreach (var report in manualApprovalReports)
            {
                (var hasRule, var ruleInfo) = await ManualApprovalRuleInfoManager.TryGetAsync(Flag, report.RuleID);
                if (!hasRule)
                {
                    Logger.Warn("Failed to load rule info, failed report {0}", report.ScopeID);
                    await MarkManualApprovalDataToExportedStatusAsync(new Record
                    {
                        NodeId = report.NodeID,
                        ManualPartitionKey = report.PartKey,
                        ManualRowKey = report.RowKey
                    });
                    ManualApprovalJobManager.AddFailedJobDetail(report, ruleInfo, "RM_RDM_Rule_RuleIsDeleted");
                    continue;
                }
                try
                {
                    using (new PerformanceScope($"ManualApproval:LoadSetting"))
                    {

                        var settingInfo = await GetManualApprovalSettingInfoAsync(report, ruleInfo);
                        if (settingInfo.IsEnableSettingManualApproval)
                        {
                            ruleInfo.WorkflowId = settingInfo.WorkflowId;
                            ruleInfo.IsSendEmailToOwner = settingInfo.IsSendEmialToOwner;
                            ruleInfo.Owners = settingInfo.Owners;
                        }
                        Logger.Info($"The [{Flag}] current manual approval report is enable setting manual approval: [{settingInfo.IsEnableSettingManualApproval}], approval type: [{ruleInfo.ManualApprovalType}], workflow id: [{ruleInfo.WorkflowId}], is send email: [{ruleInfo.IsSendEmailToOwner}].");
                    }

                    PerProcessManualApprovalReport(report);
                    var manualApprovalRecord = BasicConvertReportToManualAprovalRecord(report, ruleInfo);
                    if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess)
                    {
                        //这里用的是Reference ID 获取最新version的Definition
                        var workflowInfoDef = ManualApprovalWorkflowManager.Get(ruleInfo.WorkflowId);
                        var workflowInstance = await s_workflowProcessor.LoadAsync(workflowInfoDef.Id);
                        var step = workflowInstance.Start();

                        manualApprovalRecord.ManualWorkflowStepId = step.Id;
                        manualApprovalRecord.ManualWorkflowDefinitionId = workflowInfoDef.Id;
                        await ProcessManualApprovalReportByWorkflowNewAsync(report, manualApprovalRecord, step, ruleInfo, workflowInstance.HasStepUsedSiteOwnerApprovalMode(), workflowInstance.HasStepUsedSharePointGroupApprovalMode());
                    }
                    else if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
                    {
                        ProcessManualApprovalReportByOwner(report, ruleInfo);
                    }                     
                }               
                catch (ManualApprovalException e)
                {
                    Logger.Error($"An error occurred while process [{Flag}] manual approval report Failed. PartKey: [{report.PartKey}], RowKey: [{report.RowKey}]. Error: {e}");
                    ManualApprovalJobManager.AddFailedJobDetail(report, null, e.Message);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process [{Flag}] manual approval report Failed. PartKey: [{report.PartKey}], RowKey: [{report.RowKey}]. Error: {e}");
                    if (e.Message.Equals("RM_MA_SiteOwner_NoSiteOwner", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ManualApprovalJobManager.AddFailedJobDetail(report, null, "RM_MA_SiteOwner_NoSiteOwner");
                    }
                    else
                    {
                        ManualApprovalJobManager.AddFailedJobDetail(report, null, "");
                    }
                }
                finally
                {
                    ManualApprovalJobManager.Increase();
                }
            }
        }

        protected static void PerProcessManualApprovalReport(ManualExportReportInfo manualApprovalReport)
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

        protected async Task ProcessManualApprovalReportByWorkflowNewAsync(ManualExportReportInfo manualApprovalReport, Record manualApprovalRecord, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step, ManualApprovalRuleModel ruleInfo, bool usedSiteOwnerMode, bool isUsedSPGroup)
        {
            if (usedSiteOwnerMode)
            {
                Logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has use site owner reviewer step.");
                await ProcessWorkflowSiteOwnersAsync(ruleInfo.WorkflowId, manualApprovalReport, manualApprovalRecord.ScopeId);
            }

            if (isUsedSPGroup)
            {
                Logger.Info($"The workflow: [{ruleInfo.WorkflowId}] has use SP group reviewer step.");
                await ProcessWorkflowSPGroupAsync(ruleInfo.WorkflowId, manualApprovalReport, manualApprovalRecord.ScopeId, step);
            }

            var reviewers = await step.GetReviewersAsync(manualApprovalRecord.ScopeId);
            var templateId = step.UsedEmailTemplateId;
            manualApprovalRecord.ManualReviewer = reviewers.Select(item => item.RMUserId).ToArray();
            if(step.UsedEmailTemplateMode == RMWorkflowStepUsedEmailTemplateMode.Custom)
            {
                var customIntervalSetting = step.CustomIntervalSettings[0];
                if(customIntervalSetting == null)
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
            if(ruleInfo.IsSendEmailToOwner)
            {
                foreach (var reviewer in reviewers)
                {
                    _emailSender.Add(templateId, new RMManualEmailTemplateParameters
                    {
                        UserId = reviewer.UserId,
                        ToUser = reviewer.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual
                    }); 
                }
            }

            manualApprovalRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WorkflowInProgress;
            if (Flag == SourceFlag.Physical || Flag == SourceFlag.Connector)
            {
                manualApprovalRecord.ExportToRECO = true;
            }

            ManualApprovalDataSyncManager.Add(manualApprovalRecord); 
        }

        protected void ProcessManualApprovalReportByOwner(ManualExportReportInfo manualApprovalReport, ManualApprovalRuleModel ruleInfo)
        {
            if (ruleInfo.Owners.Count == 0)
            {
                Logger.Error($"The current manual approval report onwers is not set. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}].");
                ManualApprovalJobManager.AddFailedJobDetail(manualApprovalReport, ruleInfo, $"RM_MA_NoRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                return;
            }


            var manualApprovalRecord = BasicConvertReportToManualAprovalRecord(manualApprovalReport, ruleInfo);
            var ownerIds = ManualApprovalOwnerManager.GetOwnerIds(ruleInfo.Owners);

            manualApprovalRecord.ManualWorkflowDefinitionId = Guid.Empty;
            manualApprovalRecord.ManualWorkflowStepId = Guid.Empty;
            manualApprovalRecord.ManualWorkflowInstanceId = Guid.Empty;
            manualApprovalRecord.ManualInternalApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
            manualApprovalRecord.ManualReviewer = ownerIds.ToArray();
            if (Flag == SourceFlag.Physical || Flag == SourceFlag.Connector)
            {
                manualApprovalRecord.ExportToRECO = true;
            }

            ManualApprovalDataSyncManager.Add(manualApprovalRecord);

            if (ruleInfo.IsSendEmailToOwner)
            {
                foreach(var owner in ruleInfo.Owners)
                {
                    _emailSender.Add(RMEmailTemplateId.MANUAL_APPROVAL, new RMManualEmailTemplateParameters
                    {
                        UserId = owner.UserId,
                        ToUser = owner.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.Manual
                    });
                }
            }
        }

        private async Task ProcessWaitingItemSucceedAsync(Record item)
        {
            try
            {
                if(item.SourceFlag != (int)SourceFlag.Physical && item.SourceFlag < 1000)
                {
                    using (new PerformanceScope("ManualApproval:MarkArchiverDataToExported", "", true))
                    {
                        await MarkManualApprovalDataToExportedStatusAsync(item);
                    }
                }
                ManualApprovalJobManager.AddSucceedJobDetail(item, ManualApprovalAction.Export);
            }
            catch(ManualApprovalException e)
            {
                Logger.Error($"An error occurred while mark manual archive data to export. Error: {e}");
                ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Export, e.Message);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while mark manual archive data to export. Error: {e}");
                ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Export, "");
            }
        }

        private void ProcessWaitingItemFailed(Record item, string errorMessage)
        {
            ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Export, errorMessage);
        }

        private async Task ProcessHistoryItemSucceed(Record item)
        {
            try
            {
                var historyData = AddAction.Convert(item, (SOApproveDBStatus)item.ManualApprovedStatus, item.ManualApprovedBy, item.ManualActionTime);
                await AddAction.AddAsync(historyData);
                Logger.Info($"Succeed insert [{Flag}] item [{item.Id}] to history table.");
                ManualApprovalJobManager.AddSucceedJobDetail(item, ManualApprovalAction.Import);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while process history item succeed. Error: {e}");
                ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Import, e.Message);
            }
        }

        private void ProcessHistoryItemFailed(Record item, string errorMessage)
        {
            ManualApprovalJobManager.AddFailedJobDetail(item, ManualApprovalAction.Import, errorMessage);
        }

        protected Record BasicConvertReportToManualAprovalRecord(ManualExportReportInfo manualApparovalReport, ManualApprovalRuleModel ruleInfo)
        {
            using(new PerformanceScope("ManualApproval:GetItem", "", true))
            {
                var basicRecord = new Record();
                basicRecord = ConvertReportToManualApprovalRecord(manualApparovalReport, basicRecord);
                if (!ManualApprovalDataSyncManager.TryGet(GetQueryItemExpression(basicRecord), out var record))
                {
                    record = new Record
                    {
                        Id = basicRecord.Id,
                        RecordStatus = (int)RMRecordStatus.ManualPreSync,
                        CreateDate = manualApparovalReport.CreatedTime > 0 ?
                        int.Parse(new DateTime(manualApparovalReport.CreatedTime).ToString("yyyyMMdd")) : 0,
                        ManualExtendCount = 0 ,
                    };
                }
                else 
                {
                    if (!ruleInfo.RuleId.ToString().Equals(record.RuleId.ToString()))
                    {
                        record.ManualExtendCount = 0;
                    }
                }


                if(record.CreateDate == 0 && Flag == SourceFlag.FileSystem)
                {
                    record.CreateDate = PartitionKey;
                }

                if (Flag != SourceFlag.FileSystem)
                {
                    record.ScopeId = basicRecord.ScopeId;
                }
                if(record.AveSiteId == null && Flag == SourceFlag.SharePointOnPrem)
                {
                    record.AveSiteId = basicRecord.AveSiteId;
                }
                if(Flag == SourceFlag.SharePointOnPrem && record.ItemId == Guid.Empty)
                {
                    record.ItemId = basicRecord.ItemId;
                    record.WebId = basicRecord.WebId;
                    record.ListId = basicRecord.ListId;
                    record.FolderId = basicRecord.FolderId;
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
                record.SourceFlag = (int)GetInnerRuleFlag(manualApparovalReport); //(int)Flag; //todo
                record.ManualActionTime = DateTime.UtcNow.Ticks;
                record.ManualApprovedBy = 0;
                record.ManualApprovedStatus = (int)SOApproveDBStatus.WaitingApprove;
                record.ManualArchiveStatus = (int)AvePoint.RA.Contract.Schedule.ActionStatus.None;
                record.ManualFullPath = manualApparovalReport.Path;
                record.ManualFolderPath = manualApparovalReport.FolderPath;
                record.ManualSiteUrl = manualApparovalReport.SiteUrl;
                record.ManualEscalateFrom = 0;
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
                if (string.IsNullOrEmpty(record.CreatedBy))
                {
                    record.CreatedBy = manualApparovalReport.CreatedBy;
                    if (Flag == SourceFlag.Exchange)
                    {
                        if (!string.IsNullOrEmpty(record.CreatedBy))
                        {
                            var index = record.CreatedBy.LastIndexOf("<");
                            if (index > -1)
                            {
                                record.CreatedBy = record.CreatedBy.Substring(0, index);
                            }
                        }
                    }
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

                return record;
            }
        }

        private static string GetVersion(int uiversion)
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

        private static string GetFileExtension(ManualExportReportInfo data, Record record)
        {
            if (!string.IsNullOrEmpty(record.ExtensionForFile))
            {
                return record.ExtensionForFile;
            }

            switch ((RMNodeLevel)record.NodeType)
            {
                case RMNodeLevel.ExchangeOnlineItem:
                    return "msg";
                case RMNodeLevel.Item:
                    if (data.ArchiveLevel == (int)CacheNodeType.Item)
                    {
                        return "RM_RDM_RecordDetails_DataType_SPItem";
                    }
                    var ext = Path.GetExtension(data.LeafName);
                    return ext.Contains('.', StringComparison.CurrentCulture) ? ext[1..] : "RM_RDM_RecordDetails_DataType_FileNull";
                case RMNodeLevel.SiteCollection:
                    return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case RMNodeLevel.Site:
                    return "RM_JS_Rule_ObjectLevel_Site";
                case RMNodeLevel.List:
                    return "RM_Common_ObjectLevel_List";
                case RMNodeLevel.Folder:
                    return "RM_Common_ObjectLevel_Folder";
                case RMNodeLevel.FSFolder:
                    return "RM_RDM_RecordDetails_DataType_FSFolder";
                case RMNodeLevel.FSFile:
                    var fsExt = Path.GetExtension(data.LeafName);
                    if (fsExt.Contains('.', StringComparison.CurrentCulture))
                    {
                        return fsExt[1..];
                    }
                    return "";
                case RMNodeLevel.PhysicalBox:
                    return "RM_PRM_PRE_Filter_PhysicalBox";
                case RMNodeLevel.PhysicalFile:
                    return "RM_PRM_PRE_Filter_PhysicalFile";
                case RMNodeLevel.PhysicalRecord:
                    return "RM_PRM_PRE_Filter_PhysicalRecord";
                case RMNodeLevel.PhysicalCustom:
                    return "RM_PRM_PRE_TableItemType_Container";
                case RMNodeLevel.BoxFile:
                    var extension = Path.GetExtension(data.LeafName);
                    return extension.Contains('.', StringComparison.CurrentCulture) ? extension[1..] : "RM_RDM_RecordDetails_DataType_FileNull";
                case RMNodeLevel.CustomizeConnectorItem:
                    return "RM_Connector_ItemLevel_Item";
            }


            return "";
        }

        private static int ConvertObjectLevelToNodeLevel(RMReportObjectLevel objectLevel)
        {
            var nodeLevel = RMNodeLevel.Undefined;
            switch (objectLevel)
            {
                case RMReportObjectLevel.Item:
                    nodeLevel = RMNodeLevel.Item;
                    break;
                case RMReportObjectLevel.SiteCollection:
                    nodeLevel = RMNodeLevel.SiteCollection;
                    break;
                case RMReportObjectLevel.Site:
                    nodeLevel = RMNodeLevel.Site;
                    break;
                case RMReportObjectLevel.List:
                    nodeLevel = RMNodeLevel.List;
                    break;
                case RMReportObjectLevel.Folder:
                    nodeLevel = RMNodeLevel.Folder;
                    break;
                case RMReportObjectLevel.PhyBox:
                case RMReportObjectLevel.PhysicalBox:
                    nodeLevel = RMNodeLevel.PhysicalBox;
                    break;
                case RMReportObjectLevel.PhyCustom:
                    nodeLevel = RMNodeLevel.PhysicalCustom;
                    break;
                case RMReportObjectLevel.PhyFolder:
                    break;
                case RMReportObjectLevel.PhyRecord:
                case RMReportObjectLevel.PhysicalRecord:
                    nodeLevel = RMNodeLevel.PhysicalRecord;
                    break;
                case RMReportObjectLevel.PhysicalFile:
                    nodeLevel = RMNodeLevel.PhysicalFile;
                    break;
                case RMReportObjectLevel.ExchangeOnlineItem:
                    nodeLevel = RMNodeLevel.ExchangeOnlineItem;
                    break;
                case RMReportObjectLevel.FSFolder:
                    nodeLevel = RMNodeLevel.FSFolder;
                    break;
                case RMReportObjectLevel.FSFile:
                    nodeLevel = RMNodeLevel.FSFile;
                    break;
                case RMReportObjectLevel.BoxFile:
                    nodeLevel = RMNodeLevel.BoxFile;
                    break;
                case RMReportObjectLevel.CustomizeConnectorItem:
                    nodeLevel = RMNodeLevel.CustomizeConnectorItem;
                    break;
            }

            return (int)nodeLevel;
        }
    }
    public class NewOpusManualApprovalAttribute : Attribute
    {

    }
}

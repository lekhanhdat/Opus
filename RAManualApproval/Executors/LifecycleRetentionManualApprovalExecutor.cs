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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using RAManualApproval.Converters;
using RAManualApproval.I18ns;
using RAManualApproval.ManualExceptions;
using RAManualApproval.Model;
using RAManualApproval.ReportRelateSettingManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Executors
{
    [NewOpusManualApproval]
    public class LifecycleRetentionManualApprovalExecutor : ManualApprovalExecutor
    {
        public LifecycleRetentionManualApprovalExecutor(RMEmailSender emailSender) : base(emailSender)
        {
        }

        public override SourceFlag Flag => SourceFlag.LifecycleRetention;

        protected override Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            var siteId = manualApprovalReportInfo.ScopeID;
            record.Id = (siteId.ToString().ToLowerInvariant() + manualApprovalReportInfo.NodeID.ToString().ToLowerInvariant()).ToMd5(); ;
            record.ScopeId = new Guid(siteId);
            record.NodeId = manualApprovalReportInfo.NodeID;
            return record;
        }

        protected override Expression<Func<Record, bool>> GetQueryItemExpression(Record data)
        {
            return (record) => record.ScopeId == data.ScopeId && record.NodeId == data.NodeId;
        }

        protected override IEnumerable<List<ManualExportReportInfo>> GetManualApprovalReports()
        {
            var dataSet = RMArchiverStorageAzureTableContext.GetInstance(
                AzConnectContract.AccountName,
                AzConnectContract.AccountKey,
                AzConnectContract.Endpoint
            ).ManualArchiverSharePointOnlineItems;

            var pageSize = 1000;
            var continuationToken = string.Empty;
            do
            {

                var (token, values) = dataSet.QueryWithPagination(
                    item => item.Status == (int)SOApproveDBStatus.WaitingApprove &&
                    !item.ExportToRECO && item.SourceFlag == (int)Flag &&
                    item.CacheNodeType != 10001 && item.CacheNodeType != 20000,
                    pageSize,
                    continuationToken
                ).GetAwaiter().GetResult();

                continuationToken = token;

                var infoes = values.ConvertAll(item => {
                    var res = RMArchiverItemConverter.ConvertToReportInfo(item);
                    if(res == null)
                    {
                        return null;
                    }
                    res.CreatedTime = res.ArchivedTime;
                    return res;
                }).Where(item => item != null).ToList();

                List<ManualExportReportInfo> noVersionList = new List<ManualExportReportInfo>();
                //only keep latest version
                foreach (var item in infoes)
                {
                    item.RetentionStatus = 1;
                    if (item.LeafName.Contains(":"))
                    {
                        Logger.Info("skip version of item {0}", item.NodeID);
                    }
                    else
                    {
                        noVersionList.Add(item);
                    }
                }

                yield return noVersionList;

            } while (!string.IsNullOrEmpty(continuationToken));
        }

        protected override async Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            var model = new ManualApprovalSettingModel();
            if(ruleInfo.RetentionInfo != null)
            {
                model.IsSendEmialToOwner = ruleInfo.RetentionInfo.IsSendEamilToOwner;
                model.ManualApprovalType = ruleInfo.RetentionInfo.ReviewType == AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType.RecordOwner ? ApprovalType.RecordOwners : ApprovalType.ApprovalProcess;
                model.Owners = ruleInfo.RetentionInfo.UserInfos;
                model.WorkflowId = ruleInfo.RetentionInfo.WorkflowId; 
            }
            return model;
        }

        protected override Task MarkManualApprovalDataToExportedStatusAsync(Record item)
        {
            return ManualApprovalService.MarkApprovalingObjectsToExportedStatusAsync(AzConnectContract, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.ManualRowKey);
        }

        protected override bool ProcessApprovedAndRejectedData(Record manualApproveData)
        {
            var destoryItem = ManualApprovalService.GetDestoryItem(AzConnectContract, TenantLocalValue.LogonGroupId, manualApproveData.ScopeId.ToString(), manualApproveData.NodeId, manualApproveData.ManualVersion, true);
            if (destoryItem == null)
            {
                Logger.Warn($"Can't load [{Flag}] destory item from azure table by manual data. site id: [{manualApproveData.ScopeId}], node id: [{manualApproveData.NodeId}].");
                return false;
            }

            if (destoryItem.Status != SOApproveDBStatus.Archived && destoryItem.Status != SOApproveDBStatus.Rejected)
            {
                Logger.Warn($"The loaded [{Flag}] destory item status: [{destoryItem.Status}] is not archived or rejected. Manual data  site id: [{manualApproveData.ScopeId}], node id: [{manualApproveData.NodeId}].");
                return false;
            }

            manualApproveData.ManualArchiveStatus = (int)ActionStatus.Archiverd;
            manualApproveData.ManualArchivedTime = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta).ExpireTime.Ticks;
            return true;
        }

        protected override Task ProcessWorkflowSiteOwnersAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId)
        {
            var message = $"RM_MA_NoSupport_SiteOwner{I18NEntity.Separator}{SourceFlagI18n.SourceFlagI18ns[Flag]}";
            throw new NotImplementedException(message);
        }

        protected override Task ProcessWorkflowSPGroupAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step)
        {
            var message = $"RM_MA_NoSupport_SPGroup{I18NEntity.Separator}{SourceFlagI18n.SourceFlagI18ns[Flag]}";
            throw new ManualApprovalException(message);
        }

        protected override SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            if(reportInfo.JsonMeta != null)
            {
                try
                {
                    ArchiverSharePointDto dto = JsonConvert.DeserializeObject<ArchiverSharePointDto>(reportInfo.JsonMeta);
                    return (SourceFlag)dto.SourceFlag;
                }
                catch(Exception ex)
                {
                    Logger.Warn(@$"fail get inner rule falg:ex:{ex}");
                }
            }
            return Flag;
        }

        internal override async Task ProcessManualApprovalReportBatchAsync(List<ManualExportReportInfo> manualApprovalReports)
        { 
            foreach (var manualApprovalReport in manualApprovalReports)
            {
                try
                {
                    SourceFlag ruleFlag = this.GetInnerRuleFlag(manualApprovalReport);
                    Logger.Info($"Process [{Flag}] manual approval report, rule flag [{ruleFlag}]. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}].");
                    (var hasRule, var ruleInfo) = await ManualApprovalRuleInfoManager.TryGetAsync(ruleFlag, manualApprovalReport.RuleID);

                    if (!hasRule)
                    {
                        await MarkManualApprovalDataToExportedStatusAsync(new Record
                        {
                            NodeId = manualApprovalReport.NodeID,
                            ManualPartitionKey = manualApprovalReport.PartKey,
                            ManualRowKey = manualApprovalReport.RowKey
                        });

                        ManualApprovalJobManager.AddFailedJobDetail(manualApprovalReport, ruleInfo, "RM_RDM_Rule_RuleIsDeleted");
                        return;
                    }
                    if (ruleInfo.RetentionInfo == null)
                    {
                        Logger.Info("Retention setting is canceled on flag {0}", ruleFlag);
                        continue;
                    }
                    
                    using (new PerformanceScope($"ManualApproval:LoadSetting"))
                    {
                        var settingInfo = await GetManualApprovalSettingInfoAsync(manualApprovalReport, ruleInfo);
                        if (settingInfo.IsEnableSettingManualApproval)
                        {
                            ruleInfo.WorkflowId = settingInfo.WorkflowId;
                            ruleInfo.IsSendEmailToOwner = settingInfo.IsSendEmialToOwner;
                            ruleInfo.Owners = settingInfo.Owners;
                        }
                        Logger.Info($"The [{Flag}] current manual approval report is enable setting manual approval: [{settingInfo.IsEnableSettingManualApproval}], approval type: [{ruleInfo.ManualApprovalType}], workflow id: [{ruleInfo.WorkflowId}], is send email: [{ruleInfo.IsSendEmailToOwner}].");
                    }
                    PerProcessManualApprovalReport(manualApprovalReport);
                    Record manualApprovalRecord = BasicConvertReportToManualAprovalRecord(manualApprovalReport, ruleInfo); 
                    if (ruleInfo.RetentionInfo.ReviewType == AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType.Workflow)
                    {
                        var workflowInfoDef = ManualApprovalWorkflowManager.Get(ruleInfo.RetentionInfo.WorkflowId);
                        var workflowInstance = await s_workflowProcessor.LoadAsync(workflowInfoDef.Id);
                        var step = workflowInstance.Start();

                        manualApprovalRecord.ManualWorkflowStepId = step.Id;
                        manualApprovalRecord.ManualWorkflowDefinitionId = workflowInfoDef.Id;

                        await ProcessManualApprovalReportByWorkflowNewAsync(manualApprovalReport, manualApprovalRecord, step, ruleInfo, workflowInstance.HasStepUsedSiteOwnerApprovalMode(), workflowInstance.HasStepUsedSharePointGroupApprovalMode());
                    }
                    else if (ruleInfo.ManualApprovalType == AvePoint.RA.DB.Model.ApprovalType.RecordOwners)
                    {
                        ProcessManualApprovalReportByOwner(manualApprovalReport, ruleInfo);
                    }
                }
                catch(ManualApprovalException e)
                {
                    Logger.Error($"An error occurred while process [{Flag}] manual approval report Failed. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}]. Error: {e}");
                    ManualApprovalJobManager.AddFailedJobDetail(manualApprovalReport, null, e.Message);
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while process [{Flag}] manual approval report Failed. PartKey: [{manualApprovalReport.PartKey}], RowKey: [{manualApprovalReport.RowKey}]. Error: {e}");
                    ManualApprovalJobManager.AddFailedJobDetail(manualApprovalReport, null, e.Message);
                }
                finally
                {
                    ManualApprovalJobManager.Increase();
                }
            }
        }
    }
}

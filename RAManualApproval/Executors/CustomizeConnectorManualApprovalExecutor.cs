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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Sender;
using RAManualApproval.I18ns;
using RAManualApproval.ManualExceptions;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Executors
{
    public class CustomizeConnectorManualApprovalExecutor : ManualApprovalExecutor
    {
        public override SourceFlag Flag => SourceFlag.Connector;

        private static readonly RMAzureCosmosDBContainer CosmosContainer = RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();

        public CustomizeConnectorManualApprovalExecutor(RMEmailSender emailSender) : base(emailSender)
        {
        }

        protected override Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            record.Id = manualApprovalReportInfo.NodeID;
            record.ScopeId = Guid.Empty;
            record.NodeId = manualApprovalReportInfo.NodeID;
            return record;
        }

        protected override SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            return (SourceFlag)reportInfo.SourceFlag;
        }

        protected override IEnumerable<List<ManualExportReportInfo>> GetManualApprovalReports()
        {
            var resultSet = CosmosContainer.UseLinqQuery().Where(item => item.SourceFlag >= 1000
            && item.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove
            && !item.ExportToRECO).AsResultSet();

            var pageSize = 1000;
            string continuationToken = null;

            do
            {
                var result = resultSet.PaginateAsync(continuationToken, pageSize).GetAwaiter().GetResult();

                continuationToken = result.ContinuationToken;

                var items = result.Values.ConvertAll(item => new ManualExportReportInfo
                {
                    SourceFlag = item.SourceFlag,
                    LeafName = item.LeafName,
                    RuleID = item.RuleId.ToString(),
                    ScopeID = item.ContainerId.ToString(),
                    ObjectLevel = AvePoint.RA.Contract.RMWeb.ReportCenter.RMReportObjectLevel.CustomizeConnectorItem,
                    NodeID = item.Id,
                    ArchivedTime = item.DestroyedTime,
                    CreatedBy = item.CreatedBy,
                    ModifiedBy = item.ModifiedBy,
                    Status = (SOApproveDBStatus)item.DisposalStatus,
                    RecordStatus = (RMRecordStatus)item.RecordStatus,
                    HasRelatedDocument = 0,
                    DeleteRelatedRecords = 0,
                    RelatedRecordInfo = "",
                    Path = "",
                    ModifiedTime = item.ManualModifiedTime,
                });

                yield return items.ToList();

            } while (!string.IsNullOrEmpty(continuationToken));
        }

        protected override async Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            return new ();
        }

        protected override Expression<Func<Record, bool>> GetQueryItemExpression(Record data)
        {
            return item => item.Id == data.NodeId;
        }

        protected override async Task MarkManualApprovalDataToExportedStatusAsync(Record item)
        {
            ManualApprovalService.MarkToExportedStatusForPhysical(item.NodeId);
        }

        protected override bool ProcessApprovedAndRejectedData(Record manualApproveData)
        {
            manualApproveData.ManualArchiveStatus = (int)ActionStatus.Archiverd;
            manualApproveData.ManualArchivedTime = manualApproveData.DestroyedTime;
            return true;
        }

        protected override Task ProcessWorkflowSiteOwnersAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId)
        {
            var message = $"RM_MA_NoSupport_SiteOwner{I18NEntity.Separator}{SourceFlagI18n.SourceFlagI18ns[Flag]}";
            throw new ManualApprovalException(message);
        }

        protected override Task ProcessWorkflowSPGroupAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step)
        {
            var message = $"RM_MA_NoSupport_SPGroup{I18NEntity.Separator}{SourceFlagI18n.SourceFlagI18ns[Flag]}";
            throw new ManualApprovalException(message);
        }
    }
}

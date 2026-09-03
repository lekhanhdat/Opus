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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Sender;
using RABox.Converters;
using RAManualApproval.I18ns;
using RAManualApproval.ManualExceptions;
using RAManualApproval.Model;
using RAManualApproval.ReportRelateSettingManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RAManualApproval.Executors
{
    [NewOpusManualApproval]
    public class BoxManualApprovalExecutor : ManualApprovalExecutor
    {

        private static readonly RMAzureCosmosDBContainer CosmosContainer = RMAzureCosmosDBContext.GetContainerAsync(false).GetAwaiter().GetResult();

        private readonly IReportRelateSettingManager ReportRelateSettinManager = new BoxReportRelateSettingManager();

        public BoxManualApprovalExecutor(RMEmailSender emailSender) : base(emailSender)
        {
        }

        public override SourceFlag Flag => SourceFlag.Box;

        protected override Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            record.Id = manualApprovalReportInfo.NodeID;
            record.ScopeId = new Guid(manualApprovalReportInfo.ScopeID);
            record.NodeId = manualApprovalReportInfo.NodeID;
            var relatedRecordInfo = manualApprovalReportInfo.RelatedRecordInfo;

            if (string.IsNullOrEmpty(relatedRecordInfo))
            {
                Logger.Warn($"The [{Flag}] node: [{manualApprovalReportInfo.NodeID}] not has related record info.");
                return record;
            }

            try
            {
                var reportRelatedRecords = new List<ReportRelatedRecords>();
                var relatedRecordInfos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(manualApprovalReportInfo.RelatedRecordInfo);

                relatedRecordInfos.ForEach(item =>
                {
                    if (item.SourceFlag == (int)Flag)
                    {
                        reportRelatedRecords.Add(
                            new ReportRelatedRecords
                            {
                                Name = item.name,
                                Url = manualApprovalReportInfo.Path
                            }
                        );
                    }
                });

                record.ManualRelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get [{Flag}] node: [{manualApprovalReportInfo.NodeID}] related record info. Error: {e}");
            }

            return record;
        }

        protected override SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            return Flag;
        }

        protected override IEnumerable<List<ManualExportReportInfo>> GetManualApprovalReports()
        {
            var resultSet = CosmosContainer.UseLinqQuery().Where(item =>
              (item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove) && 
              item.SourceFlag == (int)Flag &&
              item.NodeType == (int)RMNodeLevel.BoxFile).AsResultSet();

            var limit = 1000;
            string continuationToken = null;

            do
            {
                var result = resultSet.PaginateAsync(continuationToken, limit).GetAwaiter().GetResult();

                continuationToken = result.ContinuationToken;

                var items = result.Values.ConvertAll(item => item.ConvertToManualExportReportForBox());

                yield return items.ToList();

            } while (!string.IsNullOrEmpty(continuationToken));
        }

        protected override Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            return ReportRelateSettinManager.GetReportRelateSettingInfoAsync(manualApprovalReportInfo);
        }

        protected override Expression<Func<Record, bool>> GetQueryItemExpression(Record data)
        {
            return (record) => record.ScopeId == data.ScopeId && record.NodeId == data.NodeId;
        }

        protected override async Task MarkManualApprovalDataToExportedStatusAsync(Record record)
        {
            ManualApprovalService.MarkToExportedStatusForBox(record.Id);
        }

        protected override bool ProcessApprovedAndRejectedData(Record manualApproveData)
        {
            var destoryItem = ManualApprovalService.GetBoxRecord(manualApproveData.NodeId);

            if (destoryItem == null)
            {
                Logger.Warn($"Can't load [{Flag}] destory item from cosmos db by manual data, node id: [{manualApproveData.NodeId}].");
                return false;
            }

            if (destoryItem.Status != SOApproveDBStatus.Approved && destoryItem.Status != SOApproveDBStatus.Rejected)
            {
                Logger.Warn($"The loaded [{Flag}] destory item status: [{destoryItem.Status}] is not approved or rejected. Manual data node id: [{manualApproveData.NodeId}].");
                return false;
            }

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

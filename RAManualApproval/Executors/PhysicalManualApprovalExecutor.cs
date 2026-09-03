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
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Sender;
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
    public class PhysicalManualApprovalExecutor : ManualApprovalExecutor
    {
        public override SourceFlag Flag => SourceFlag.Physical;

        private readonly IReportRelateSettingManager ReportRelateSettinManager = new PhysicalReportRelateSettingManager();

        private static readonly RMAzureCosmosDBContainer CosmosContainer = RMAzureCosmosDBContext.GetContainerAsync().GetAwaiter().GetResult();

        public PhysicalManualApprovalExecutor(RMEmailSender emailSender) : base(emailSender)
        {
        }

        protected override Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
        {
            record.Id = manualApprovalReportInfo.NodeID;
            record.ScopeId = Guid.Empty;
            record.NodeId = manualApprovalReportInfo.NodeID;

            if (string.IsNullOrEmpty(manualApprovalReportInfo.RelatedRecordInfo))
            {
                Logger.Warn($"The [{Flag}] node: [{manualApprovalReportInfo.NodeID}] not has related record info.");
                return record;
            }

            try
            {
                string GetFullPath(string siteUrl, string itemUrl)
                {
                    if (itemUrl.StartsWith("http:") || itemUrl.StartsWith("https:"))
                    {
                        return itemUrl;
                    }
                    var stringBuilder = new StringBuilder(512);
                    var siteUri = new Uri(siteUrl);
                    stringBuilder.Append("https:");
                    stringBuilder.Append("//");
                    stringBuilder.Append(siteUri.Host);
                    return stringBuilder.ToString() + itemUrl;
                }
                var reportRelatedRecords = new List<ReportRelatedRecords>();
                var relatedRecordInfos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(manualApprovalReportInfo.RelatedRecordInfo);
                foreach (var relatedRecordInfo in relatedRecordInfos)
                {
                    if (relatedRecordInfo.SourceFlag == (int)Flag)
                    {
                        var url = $"/Root/PRM/RecordsExplorer/?uniqueId={relatedRecordInfo.recId}";
                        reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedRecordInfo.recId, Url = url });
                    }
                    else if (relatedRecordInfo.SourceFlag == (int)SourceFlag.SharePoint || relatedRecordInfo.SourceFlag == (int)SourceFlag.All)
                    {
                        var relatedItemUrl = GetFullPath(relatedRecordInfo.SiteUrl, relatedRecordInfo.url);
                        reportRelatedRecords.Add(
                            new ReportRelatedRecords
                            {
                                Name = relatedRecordInfo.name,
                                Url = relatedItemUrl
                            }
                        );
                    }
                }
                record.ManualRelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get [{Flag}] node: [{manualApprovalReportInfo.NodeID}] related record info. Error: {e}");
            }

            return record;
        }

        protected override Expression<Func<Record, bool>> GetQueryItemExpression(Record data)
        {
            return (record) => record.Id == data.NodeId;
        }

        protected override IEnumerable<List<ManualExportReportInfo>> GetManualApprovalReports()
        {
            var resultSet = CosmosContainer.UseLinqQuery().Where(item =>
            item.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove &&
            item.ExportToRECO == false &&
            item.SourceFlag == (int)SourceFlag.Physical && 
            (item.NodeType == (int)RMNodeType.PhyFile || item.NodeType == (int)RMNodeType.PhyBox)).AsResultSet();

            var pageSize = 1000;
            string continuationToken = null;

            do
            {
                var result = resultSet.PaginateAsync(continuationToken, pageSize).GetAwaiter().GetResult();

                continuationToken = result.ContinuationToken;

                var items = result.Values.ConvertAll(RMArchiverItemConverter.ConvertToReportInfo);

                yield return items.ToList();

            } while (!string.IsNullOrEmpty(continuationToken));
        }

        protected override Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            return ReportRelateSettinManager.GetReportRelateSettingInfoAsync(manualApprovalReportInfo);
        }

        protected override async Task MarkManualApprovalDataToExportedStatusAsync(Record item)
        {
            ManualApprovalService.MarkToExportedStatusForPhysical(item.NodeId);
        }

        protected override bool ProcessApprovedAndRejectedData(Record manualApproveData)
        {
            var destoryItem = ManualApprovalService.GetPhysicalRecord(manualApproveData.NodeId);
            if (destoryItem == null)
            {
                Logger.Warn($"Can't load [{Flag}] destory item from azure table by manual data, node id: [{manualApproveData.NodeId}].");
                return false;
            }

            if (destoryItem.Status != SOApproveDBStatus.Archived && destoryItem.Status != SOApproveDBStatus.WaitingApprove)
            {
                Logger.Warn($"The loaded [{Flag}] destory item status: [{destoryItem.Status}] is not archived or rejected. Manual data node id: [{manualApproveData.NodeId}].");
                return false;
            }

            manualApproveData.ManualArchiveStatus = (int)ActionStatus.Archiverd;
            manualApproveData.ManualArchivedTime = destoryItem.ArchivedTime;
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

        protected override SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            return Flag;
        }
    }
}

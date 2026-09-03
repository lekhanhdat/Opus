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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Email.Sender;
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
    public class SharePointOnlineManualApprovalExecutor : ManualApprovalExecutor
    {
        public override SourceFlag Flag => SourceFlag.SharePoint;

        private readonly IReportRelateSettingManager ReportRelateSettinManager = new SharePointOnlineReportRelateSettingManager();

        public SharePointOnlineManualApprovalExecutor(RMEmailSender emailSender) : base(emailSender)
        {
        }

        protected override Record ConvertReportToManualApprovalRecord(ManualExportReportInfo manualApprovalReportInfo, Record record)
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

            var siteId = JsonConvert.DeserializeObject<ArchiverSharePointDto>(manualApprovalReportInfo.JsonMeta).SiteId;
            record.Id = (siteId.ToString().ToLowerInvariant() + manualApprovalReportInfo.NodeID.ToString().ToLowerInvariant()).ToMd5();
            record.ScopeId = siteId;
            record.NodeId = manualApprovalReportInfo.NodeID;

            if (string.IsNullOrEmpty(manualApprovalReportInfo.RelatedRecordInfo))
            {
                Logger.Warn($"The [{Flag}] node: [{manualApprovalReportInfo.NodeID}] not has related record info.");
                return record;
            }

            try
            {
                var reportRelatedRecords = new List<ReportRelatedRecords>();
                var relatedRecordInfo = manualApprovalReportInfo.RelatedRecordInfo;

                var relatedInfos = new RelatedRecordsUtility().GetRelatedPropertiesBySPColumnValue(relatedRecordInfo);
                relatedInfos.ForEach(item =>
                {
                    if (item.SourceFlag == (int)Flag || item.SourceFlag == (int)SourceFlag.All)
                    {
                        var relatedItemUrl = GetFullPath(item.SiteUrl, item.url);
                        reportRelatedRecords.Add(
                            new ReportRelatedRecords
                            {
                                Name = item.name,
                                Url = relatedItemUrl
                            }
                        );
                    }
                    else if (item.SourceFlag == (int)SourceFlag.Physical)
                    {
                        var url = $"/Root/PRM/RecordsExplorer/?uniqueId={item.recId}";
                        reportRelatedRecords.Add(new ReportRelatedRecords() { Name = item.recId, Url = url });
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

                var infoes = values.ConvertAll(item => RMArchiverItemConverter.ConvertToReportInfo(item)).Where(item => item != null).ToList();
                yield return infoes;

            } while (!string.IsNullOrEmpty(continuationToken));
        }

        protected override Task<ManualApprovalSettingModel> GetManualApprovalSettingInfoAsync(ManualExportReportInfo manualApprovalReportInfo, ManualApprovalRuleModel ruleInfo)
        {
            return ReportRelateSettinManager.GetReportRelateSettingInfoAsync(manualApprovalReportInfo);
        }

        protected override Task MarkManualApprovalDataToExportedStatusAsync(Record item)
        {
            return ManualApprovalService.MarkApprovalingObjectsToExportedStatusAsync(AzConnectContract, TenantLocalValue.LogonGroupId, item.ManualPartitionKey, item.ManualRowKey);
        }

        protected override bool ProcessApprovedAndRejectedData(Record manualApproveData)
        {
            var destoryItem = ManualApprovalService.GetDestoryItem(AzConnectContract, TenantLocalValue.LogonGroupId, manualApproveData.ScopeId.ToString(), manualApproveData.NodeId, manualApproveData.ManualVersion);
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
            manualApproveData.ManualArchivedTime = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta).ArchivedTime.Ticks;
            return true;

        }

        protected override Task ProcessWorkflowSiteOwnersAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId)
        {
            return ManualApprovalWorkflowManager.SyncSiteOwnerAsync(workflowId, reportInfo, siteId);
        }

        protected override Task ProcessWorkflowSPGroupAsync(string workflowId, ManualExportReportInfo reportInfo, Guid siteId, AvePoint.RA.RACommonUtility.Workflow.RMWorkflowStep step)
        {
            return ManualApprovalWorkflowManager.SyncSharePointGroupAsync(workflowId, reportInfo, siteId, step);
        }

        protected override SourceFlag GetInnerRuleFlag(ManualExportReportInfo reportInfo)
        {
            return Flag;
        }


    }
}

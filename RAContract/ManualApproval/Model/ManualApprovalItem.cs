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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("sourceFlag")]
        public int SourceFlag { get; set; }

        [JsonProperty("sourceName")]
        public string SourceName { get; set; }

        [JsonProperty("sourceIcon")]
        public string SourceIcon { get; set; }

        [JsonProperty("nodeType")]
        public int NodeType { get; set; }

        [JsonProperty("leafName")]
        public string LeafName { get; set; }

        [JsonProperty("fileExtension")]
        public string FileExtension { get; set; }

        [JsonProperty("nodeId")]
        public Guid NodeId { get; set; }

        [JsonProperty("ruleId")]
        public Guid RuleId { get; set; }

        [JsonProperty("reviewerDisplayNames")]
        public List<string> ReviewerDisplayNames { get; set; }

        [JsonProperty("escalateFromDisplayName")]
        public string EscalateFromDisplayName { get; set; }

        [JsonProperty("fullPath")]
        public string FullPath { get; set; }

        [JsonProperty("fullPathRealLocation")]
        public string FullPathRealLocation { get; set; }

        [JsonProperty("approvedByUserId")]
        public int ApprovedByUserId { get; set; }

        [JsonProperty("approvedByDisplayName")]
        public string ApprovedByDisplayName { get; set; }

        [JsonProperty("approvedStatus")]
        public int ApprovedStatus { get; set; }

        [JsonProperty("internalApprovedStatus")]
        public int InternalApprovedStatus { get; set; }

        [JsonProperty("escalatedComment")]
        public string EscalatedComment { get; set; }

        [JsonProperty("extendTime")]
        public string ExtendTime { get; set; }

        [JsonProperty("extendTicks")]
        public long ExtendTicks { get; set; }

        [JsonProperty("extendComment")]
        public string ExtendComment { get; set; }

        [JsonProperty("collectionTime")]
        public string CollectionTime { get; set; }

        [JsonProperty("collectionDateTime")]
        public DateTime CollectionDateTime { get; set; }

        [JsonProperty("collectionTicks")] 
        public long CollectionTicks { get; set; }

        [JsonProperty("actionTime")]
        public string ActionTime { get; set; }

        [JsonProperty("ruleName")]
        public string RuleName { get; set; }

        [JsonProperty("ruleCriteria")]
        public string RuleCriteria { get; set; }

        [JsonProperty("ruleDisposalClass")]
        public string RuleDisposalClass { get; set; }

        [JsonProperty("audits")]
        public string ManualAudits { get; set; }

        [JsonProperty("relatedRecordsAction")]
        public int RelatedRecordsAction { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty("modifiedTime")]
        public string ModifiedTime { get; set; }

        [JsonProperty("modifiedTicks")]
        public long ModifiedTicks { get; set; }

        [JsonProperty("isRelatedRecords")]
        public bool IsRelatedRecords { get; set; }

        [JsonProperty("relatedRecords")]
        public List<ReportRelatedRecords> RelatedRecords { get; set; }

        [JsonProperty("extendCount")]
        public int ExtendCount { get; set; }

        [JsonProperty("emailNotificationCount")]
        public int EmailNotificationCount { get; set; }

        [JsonProperty("emailNotificationLastTime")]
        public long EmailNotificationLastTime { get; set; }

        [JsonProperty("needEmailNotification")]
        public bool NeedEmailNotification { get; set; }

        [JsonProperty("retentionStatus")]
        public int RetentionStatus { set; get; }

        [JsonProperty("recordsId")]
        public string RecordsId { get; set; }

        [JsonProperty("createdTime")]
        public string CreatedTime { get; set; }

        [JsonProperty("manualAudit")]
        public string ManualAudit { get; set; }

        [JsonProperty("termFullPath")]
        public string TermFullPath { get; set; }

        [JsonProperty("fileSize")]
        public string FileSize { get; set; }

        [JsonProperty("timeModified")]
        public string TimeModified { get; set; }

        #region Machine Learning

        [JsonProperty("predictTermId")]
        public Guid PredictTermId { get; set; }

        [JsonProperty("predictTermName")]
        public string PredictTermName { get; set; }

        [JsonProperty("containerId")]
        public string ContainerId { get; set; }

        [JsonProperty("predictTermFullPath")]
        public string PredictTermFullPath { get; set; }
        #endregion

        [JsonProperty("manualApprovalComment")]
        public string ManualApprovalComment { get; set; }

        [JsonProperty("manualQuickReason")]
        public string QuickReason { get; set; }

        [JsonProperty("manualFolderPath")]
        public string FolderPath { get; set; }

        [JsonProperty("manualSiteUrl")]
        public string SiteUrl { get; set; }

        [JsonProperty("manualSiteUrlId")]
        public string SiteUrlId { get; set; }

        [JsonProperty(PropertyName = "manualLastReasonForRejection")]
        public string ManualLastReasonForRejection { get; set; }

        [JsonProperty(PropertyName = "manualLastExtendType")]
        public ManualApprovalExtendType ManualLastExtendType { get; set; }

        [JsonProperty(PropertyName = "manualLastCustomeExtendDate")]
        public DateTime ManualLastCustomeExtendDate { get; set; }

        [JsonProperty(PropertyName = "manualIsNextEndWorkFlow")]
        public bool ManualIsNextEndWorkFlow { get; set; }

        [JsonProperty(PropertyName = "manualLastApproveRejectComment")]
        public string ManualLastApproveRejectComment { get; set; }

        [JsonProperty(PropertyName = "manualLastReviewedBy")]
        public string ManualLastReviewedBy { get; set; }

        [JsonProperty(PropertyName = "manualLastReviewTime")]
        public string ManuaLastlReviewTime { get; set; }

        [JsonProperty(PropertyName = "manualLastReviewTicks")]
        public long ManualLastReviewTicks { get; set; }

        [JsonProperty(PropertyName = "customeColumnDic")]
        public Dictionary<string, CustomColumn> CustomColumnDic { set; get; }

        [JsonProperty(PropertyName = "manualDisposalDueDate")]
        public string ManualDisposalDueDate { set; get; }

        [JsonProperty(PropertyName = "termName")]
        public string TermName { set; get; }

        [JsonProperty(PropertyName = "termId")]
        public string TermId { set; get; }
        [JsonProperty(PropertyName = "webviewlink")]
        public string WebViewLink { set; get; }

        [JsonProperty(PropertyName = "enableClassificationByOpus")]
        public bool EnableClassificationByOpus { get; set; }

        [JsonProperty(PropertyName = "pendingDisposal")]
        public int PendingDisposal { get; set; }

        [JsonProperty(PropertyName = "manualApprovedStatus")]
        public int ManualApprovedStatus { set; get; }
    }
}

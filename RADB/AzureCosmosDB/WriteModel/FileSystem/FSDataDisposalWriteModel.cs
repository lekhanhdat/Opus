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
using AvePoint.RA.DB.AzureCosmosDB.WriteMode;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace AvePoint.RA.DB.AzureCosmosDB.WriteModel.FileSystem
{
    public class FSDataDisposalWriteModel
    {
        [JsonProperty(PropertyName = "containerId")]
        public string ContainerId { get; set; }

        [JsonProperty(PropertyName = "scopeId")]
        public Guid ScopeId { get; set; }

        [JsonProperty(PropertyName = "nodeId")]
        public Guid NodeId { get; set; }

        [JsonProperty(PropertyName = "timeModified")]
        public long TimeModified { get; set; }

        [JsonProperty(PropertyName = "sourceFlag")]
        [DefaultValue((int)Contract.Explorer.SourceFlag.FileSystem)]
        public int SourceFlag { get; set; }

        [JsonProperty(PropertyName = "recordStatus")]
        public int RecordStatus { get; set; }

        [JsonProperty(PropertyName = "nodeType")]
        public int NodeType { get; set; }

        [JsonProperty(PropertyName = "termId")]
        public Guid TermId { get; set; }

        [JsonProperty(PropertyName = "termName")]
        public string TermName { get; set; }

        [JsonProperty(PropertyName = "itemId")]
        public Guid ItemId { get; set; }

        [JsonProperty(PropertyName = "rowKey")]
        public string RowKey { get; set; }

        [JsonProperty(PropertyName = "itemRowId")]
        public int ItemRowId { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public Guid ParentId { get; set; }

        [JsonProperty(PropertyName = "aveSiteId")]
        public string AveSiteId { get; set; }

        [JsonProperty(PropertyName = "collectTime")]
        public long CollectTime { get; set; }

        [JsonProperty(PropertyName = "folderId")]
        public Guid FolderId { get; set; }

        [JsonProperty(PropertyName = "ruleId")]
        public Guid RuleId { get; set; }

        [JsonProperty(PropertyName = "ruleLevel")]
        public int RuleLevel { get; set; }

        [JsonProperty(PropertyName = "metaInfo")]
        public string MetaInfo { get; set; }

        [JsonProperty(PropertyName = "extsion1")]
        public string Extsion1 { get; set; }

        [JsonIgnore]
        public string FullPath { get; set; }

        [JsonProperty(PropertyName = "extensionForFile")]
        public string ExtensionForFile { get; set; }

        [JsonProperty(PropertyName = "holdStatus")]
        public bool HoldStatus { get; set; }

        [JsonProperty(PropertyName = "holdReleaseTime")]
        public long HoldReleaseTime { get; set; }

        [JsonProperty(PropertyName = "holdBy")]
        public string HoldBy { get; set; }

        [JsonProperty(PropertyName = "holdId")]
        public string HoldId { get; set; }

        [JsonProperty(PropertyName = "holdType")]
        public int HoldType { get; set; }

        [JsonProperty(PropertyName = "holdByUsers")]
        public string HoldByUsers { get; set; }

        [JsonProperty(PropertyName = "holdUntilTimes")]
        public string HoldUntilTimes { get; set; }

        [JsonProperty(PropertyName = "appendHolds_Array")]
        public string[] AppendHolds_Array { get; set; }

        [JsonProperty(PropertyName = "previousDisposalDueDate")]
        public long PreviosDisposalDueDate { get; set; }

        [JsonProperty(PropertyName = "disposalDueDate")]
        public long DisposalDueDate { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty(PropertyName = "sortTicks")]
        public long SortTicks { get; set; }

        [JsonProperty(PropertyName = "manual_isManualSynced")]
        public bool IsManualSynced { get; set; }

        [JsonProperty(PropertyName = "manual_actionTime")]
        public long ManualActionTime { get; set; }

        [JsonProperty(PropertyName = "manual_approvedBy")]
        public int ManualApprovedBy { get; set; }

        [JsonProperty(PropertyName = "manual_escalatedComment")]
        public string ManualEscalatedComment { get; set; }

        [JsonProperty(PropertyName = "manual_approvedStatus")]
        public int ManualApprovedStatus { get; set; }

        [JsonProperty(PropertyName = "manual_internalApprovedStatus")]
        public int ManualInternalApprovedStatus { get; set; }

        [JsonProperty(PropertyName = "manual_archiveStatus")]
        public int ManualArchiveStatus { get; set; }

        [JsonProperty(PropertyName = "manual_fullPath")]
        public string ManualFullPath { get; set; }

        [JsonProperty(PropertyName = "manual_folderPath")]
        public string ManualFolderPath { get; set; }
        [JsonProperty(PropertyName = "manual_lastReasonForRejection")]
        public string ManualLastReasonForRejection { get; set; }

        [JsonProperty(PropertyName = "manual_siteUrl")]
        public string ManualSiteUrl { get; set; }

        [JsonProperty(PropertyName = "manual_escalateFrom")]
        public int ManualEscalateFrom { get; set; }

        [JsonProperty(PropertyName = "manual_extendTime")]
        public long ManualExtendTime { get; set; }

        [JsonProperty(PropertyName = "manual_extendComment")]
        public string ManualExtendComment { get; set; }

        [JsonProperty(PropertyName = "manual_collectionTime")]
        public long ManualCollectionTime { get; set; }

        [JsonProperty(PropertyName = "manual_audits")]
        public string ManualAudits { get; set; }

        [JsonProperty(PropertyName = "manual_archivedTime")]
        public long ManualArchivedTime { get; set; }

        [JsonProperty(PropertyName = "manual_partitionKey")]
        public string ManualPartitionKey { get; set; }

        [JsonProperty(PropertyName = "manual_rowKey")]
        public string ManualRowKey { get; set; }

        [JsonProperty(PropertyName = "manual_ruleName")]
        public string ManualRuleName { get; set; }

        [JsonProperty(PropertyName = "manual_ruleCriteria")]
        public string ManualRuleCriteria { get; set; }

        [JsonProperty(PropertyName = "manual_ruleDisposalClass")]
        public string ManualRuleDisposalClass { get; set; }

        [JsonProperty(PropertyName = "manual_version")]
        public string ManualVersion { get; set; }

        [JsonProperty(PropertyName = "manual_reviewer_Array")]
        public int[] ManualReviewer { get; set; }

        [JsonProperty(PropertyName = "manual_workflowInstanceId")]
        public Guid ManualWorkflowInstanceId { get; set; }

        [JsonProperty(PropertyName = "manual_workflowDefinitionId")]
        public Guid ManualWorkflowDefinitionId { get; set; }

        [JsonProperty(PropertyName = "manual_workflowStepId")]
        public Guid ManualWorkflowStepId { get; set; }

        [JsonProperty(PropertyName = "manual_extendCount")]
        public int ManualExtendCount { get; set; }

        [JsonProperty(PropertyName = "manual_emailNotificationCount")]
        public int ManualEmailNotificationCount { get; set; }

        [JsonProperty(PropertyName = "manual_emailNotificationLastTime")]
        public long ManualEmailNotificationLastTime { get; set; }

        [JsonProperty(PropertyName = "manual_needEmailNotification")]
        public bool ManualNeedEmailNotification { get; set; }

        [JsonProperty(PropertyName = "manual_isAutoReassigned")]
        public bool ManualIsAutoReassigned { get; set; }
        [JsonProperty(PropertyName = "manual_isRelatedRecords")]
        public bool ManualIsRelatedRecords { get; set; }
        [JsonProperty(PropertyName = "manual_relatedRecords", NullValueHandling = NullValueHandling.Ignore)]
        public string ManualRelatedRecords { get; set; }
        [JsonProperty(PropertyName = "manual_RelatedRecordsAction")]
        public int ManualRelatedRecordsAction { get; set; }
        [JsonProperty(PropertyName = "manual_retentionStatus")]
        public int ManualRetentionStatus { set; get; }
        [JsonProperty(PropertyName = "manual_modifiedTime")]
        public long ManualModifiedTime { get; set; }
    }
}

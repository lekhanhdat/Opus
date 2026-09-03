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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.CosmosDBControl;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AvePoint.RA.DB.Explorer.Model
{
    /// <summary>
    /// 在DAO中也使用了这个类，如果要添加或者修改属性，请查看DAO中是否也需要修改
    /// </summary>
    public class Record
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "scopeId")]
        public Guid ScopeId { get; set; }

        [JsonProperty(PropertyName = CosmosFieldName.ContainerId, NullValueHandling = NullValueHandling.Ignore)]
        public string ContainerId { get; set; }

        /// <summary>
        /// -1: None
        /// 0: All
        /// 1: SharePoint
        /// 2: FileSystem
        /// 3: Exchange
        /// 4: Physical
        /// </summary>
        [JsonProperty(PropertyName = "sourceFlag")]
        public int SourceFlag { get; set; }

        /// <summary>
        /// For Physical Record
        /// 0: Undefined
        /// 1: Location
        /// 100: Box
        /// 500: File
        /// 1000: Record
        /// </summary>
        [JsonProperty(PropertyName = "nodeType")]
        public int NodeType { get; set; }

        [JsonProperty(PropertyName = "nodeId")]
        public Guid NodeId { get; set; }

        [JsonProperty(PropertyName = "teamsId")]
        public Guid TeamsId { get; set; }

        /// <summary>
        /// Root Location Id for Physical...
        /// </summary>
        [JsonProperty(PropertyName = "aveSiteId")]
        public string AveSiteId { get; set; }

        [JsonProperty(PropertyName = "webId")]
        public Guid WebId { get; set; }

        [JsonProperty(PropertyName = "listId")]
        public Guid ListId { get; set; }

        /// <summary>
        /// Parent Folder Id
        /// </summary>
        [JsonProperty(PropertyName = "folderId")]
        public Guid FolderId { get; set; }

        [JsonProperty(PropertyName = "itemId")]
        public Guid ItemId { get; set; }

        [JsonProperty(PropertyName = "itemRowId")]
        public int ItemRowId { get; set; }

        //[JsonProperty(PropertyName = "fullPath")]
        //public string FullPath { get; set; }

        [JsonProperty(PropertyName = "metaInfo", NullValueHandling = NullValueHandling.Ignore)]
        public string MetaInfo { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "modifiedBy")]
        public string ModifiedBy { get; set; }

        [JsonProperty(PropertyName = "dirPath")]
        public string DirPath { get; set; }

        [JsonProperty(PropertyName = "leafName")]
        public string LeafName { get; set; }

        [JsonProperty(PropertyName = "extensionForFile")]
        public string ExtensionForFile { get; set; }

        [JsonProperty(PropertyName = "timeCreated")]
        public long TimeCreated { get; set; }

        [JsonProperty(PropertyName = "timeModified")]
        public long TimeModified { get; set; }

        [JsonProperty(PropertyName = "collectTime")]
        public long CollectTime { get; set; }

        [JsonProperty(PropertyName = "createDate")]
        public int CreateDate { get; set; }  //this is a duplicated property for the aggregation

        [JsonProperty(PropertyName = "recordHistory", NullValueHandling = NullValueHandling.Ignore)]
        public string RecordHistory { get; set; }

        [JsonProperty(PropertyName = "recordsId")]
        public string RecordsId { get; set; }

        [JsonProperty(PropertyName = "rowKey")]
        public string RowKey { get; set; } //this column is used for connector data source

        [JsonProperty(PropertyName = "disposalDueDate")]
        public long DisposalDueDate { get; set; }

        [JsonProperty(PropertyName = "previousDisposalDueDate")]
        public long PreviosDisposalDueDate { get; set; }

        [JsonProperty(PropertyName = CosmosFieldName.TermId)]
        public Guid TermId { get; set; }

        [JsonProperty(PropertyName = "termName")]
        public string TermName { get; set; }

        [JsonProperty(PropertyName = "isInheritedTerm")]
        public bool IsInheritedTerm { get; set; }

        [JsonProperty(PropertyName = "ruleId")]
        public Guid RuleId { get; set; }

        [JsonProperty(PropertyName = "ruleLevel")]
        public int RuleLevel { get; set; }

        [JsonProperty(PropertyName = "recordOwner")]
        public string RecordOwner { get; set; }

        [JsonProperty(PropertyName = "declareAsRecord")]
        public bool DeclareAsRecord { get; set; }

        [JsonProperty(PropertyName = "declaredBy")]
        public string DeclaredBy { get; set; }

        [JsonProperty(PropertyName = "lockedByRecordLabel")]
        public bool LockedByRecordLabel { get; set; }

        [JsonProperty(PropertyName = "applyRecordLabelBy")]
        public string ApplyRecordLabelBy { get; set; }

        [JsonProperty(PropertyName = "relatedRecordsCount")]
        public int RelatedRecordsCount { get; set; }

        [JsonProperty(PropertyName = "relatedRecords", NullValueHandling = NullValueHandling.Ignore)]
        public string RelatedRecords { get; set; }

        [JsonProperty(PropertyName = "extsion1", NullValueHandling = NullValueHandling.Ignore)]
        public string Extsion1 { get; set; }

        [JsonProperty(PropertyName = "parentId")]
        public Guid ParentId { get; set; }

        [JsonProperty(PropertyName = "holdStatus")]
        public bool HoldStatus { get; set; }

        [JsonProperty(PropertyName = "holdId")]
        public string HoldId { get; set; }
        [JsonProperty(PropertyName = "holdReleaseTime")]
        public long HoldReleaseTime { get; set; }
        [JsonProperty(PropertyName = "holdBy")]
        public string HoldBy { get; set; }

        [JsonProperty(PropertyName = "holdByUsers", NullValueHandling = NullValueHandling.Ignore)]
        public string HoldByUsers { get; set; }

        [JsonProperty(PropertyName = "holdUntilTimes", NullValueHandling = NullValueHandling.Ignore)]
        public string HoldUntilTimes { get; set; }

        /// <summary>
        /// 标记Hold 类型，目前有None, Personal Hold 和Disposal hold
        /// </summary>
        [JsonProperty(PropertyName = "holdType")]
        public int HoldType { get; set; }
        /// <summary>
        /// **此属性不足以判断具体数据类型，如果需要当做条件，应确认好原端类型以及ID 等字段进行确认
        /// 1:active 2, archived, 3 delete, 4 moved, 5 overwrited(Move job destination file can be overwrited)
        /// For physical: 1:Open, 2:Destroyed, 3: delete(RM 删除的文件，理论上不显示),  6:closed, 7: Missing. 不使用3， 4， 5 防止与其他值混淆
        /// </summary>
        [JsonProperty(PropertyName = "recordStatus")]
        public int RecordStatus { get; set; }

        [JsonProperty(PropertyName = "previousRecordStatus")]
        public int PreviousRecordStatus { get; set; }

        [JsonProperty(PropertyName = "destroyedTime")]
        public long DestroyedTime { get; set; } //Physical action time.
        [JsonIgnore]
        //TODO hyw remove 
        [Obsolete("this will be removed.  you can use dirpath+leafname")]
        public string FullPath { get; set; }

        [Obsolete("used for job detail only")]
        public string Comment { get; set; }

        [JsonProperty(PropertyName = "externalId")]
        public string ExternalId { get; set; }
        [JsonProperty(PropertyName = "emailaddress")]
        public string EmailAddress { get; set; }
        [JsonProperty(PropertyName = "sendto")]
        public string SendTo { get; set; }
        [JsonProperty(PropertyName = "webviewlink")]
        public string WebViewLink { get; set; }

        #region Physical Property
        /// <summary>
        /// Nearest Parent Location Id
        /// </summary>
        [JsonProperty(PropertyName = "locationId")]
        public Guid LocationId { get; set; }
        [JsonProperty(PropertyName = "boxId")]
        public Guid BoxId { get; set; }
        [JsonProperty(PropertyName = "fileId")]
        public Guid FileId { get; set; }
        [JsonProperty(PropertyName = "isLocked")]
        public bool IsLocked { get; set; }
        [JsonProperty(PropertyName = CosmosFieldName.TemplateId)]
        public int TemplateId { get; set; }
        /// <summary>
        /// currently used for physical records, it includes the id list start from bottom location to parent node
        /// </summary>
        [JsonProperty(PropertyName = "ancestor_Array", NullValueHandling = NullValueHandling.Ignore)]
        public List<Guid> Ancestors { get; set; }
        [JsonProperty(PropertyName = "disposalStatus")]
        public int DisposalStatus { get; set; }//Physical Disposal Status.
        //[JsonProperty(PropertyName = "disposalActionTime")]
        //public long DisposalActionTime { get; set; }//Physical Disposal Time Utc ticks.
        [JsonProperty(PropertyName = "ApproveUsers")]
        public string ApproveUsers { get; set; }//To Do Consider.
        [JsonProperty(PropertyName = "exportToRECO")]
        public bool ExportToRECO { get; set; }
        [JsonProperty(PropertyName = "deleteRelatedRecords")]
        public int DeleteRelatedRecords { get; set; }//标记是否在删除文件的同时，删除RelatedRecord. 1 means delated related record, 0 means skip
        [JsonProperty(PropertyName = CosmosFieldName.ScopePermissionId)]
        public int ScopePermissionId { get; set; }
        [JsonProperty(PropertyName = "physicalActionAudit")]
        public string PhysicalActionAudit { get; set; }
        #endregion

        #region fs property
        [JsonProperty(PropertyName = "sortTicks")]
        public long SortTicks { get; set; }

        [JsonProperty(PropertyName = "classCode")]
        public string ClassCode { set; get; }
        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { set; get; }

        [JsonProperty(PropertyName = "retentionType")]
        public string RetentionType { set; get; }

        [JsonProperty(PropertyName = "startDate", NullValueHandling = NullValueHandling.Ignore)]
        public long StartDate { set; get; }

        [JsonProperty(PropertyName = "endTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EndTime { set; get; }

        [JsonProperty(PropertyName = "policyValueUnit")]
        public string PolicyValueUnit { set; get; }

        [JsonProperty(PropertyName = "policyValueNumber")]
        public string PolicyValueNumber { set; get; }
        #endregion

        #region Array String for search
        [JsonProperty(PropertyName = "modifiedBy_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ModifiedBy_Array { get; set; }

        [JsonProperty(PropertyName = "createdBy_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] CreatedBy_Array { get; set; }
        [JsonProperty(PropertyName = "modifiedBy_Lower", NullValueHandling = NullValueHandling.Ignore)]
        public string ModifiedBy_Lower { get; set; }

        [JsonProperty(PropertyName = "createdBy_Lower", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedBy_Lower { get; set; }

        [JsonProperty(PropertyName = "declaredBy_Lower", NullValueHandling = NullValueHandling.Ignore)]
        public string DeclaredBy_Lower { get; set; }

        //[JsonProperty(PropertyName = "dirPath_Array", NullValueHandling = NullValueHandling.Ignore)]
        //public string[] DirPath_Array { set; get; }

        [JsonProperty(PropertyName = "leafName_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] LeafName_Array { get; set; }

        [JsonProperty(PropertyName = "recordsId_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] RecordsId_Array { get; set; }  //存本身

        [JsonProperty(PropertyName = "declaredBy_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] DeclaredBy_Array { get; set; }

        [JsonProperty(PropertyName = "recordOwner_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] RecordOwner_Array { get; set; }

        [JsonProperty(PropertyName = "appendHolds_Array", NullValueHandling = NullValueHandling.Ignore)]
        public string[] AppendHolds_Array { get; set; }

        #endregion

        #region Manual Approval

        [JsonProperty(PropertyName = "manual_reviewer_Array", NullValueHandling = NullValueHandling.Ignore)]
        public int[] ManualReviewer { get; set; }

        [JsonProperty(PropertyName = "manual_escalateFrom")]
        public int ManualEscalateFrom { get; set; }

        [JsonProperty(PropertyName = "manual_workflowInstanceId")]
        public Guid ManualWorkflowInstanceId { get; set; }

        [JsonProperty(PropertyName = "manual_workflowDefinitionId")]
        public Guid ManualWorkflowDefinitionId { get; set; }

        [JsonProperty(PropertyName = "manual_workflowStepId")]
        public Guid ManualWorkflowStepId { get; set; }

        [JsonProperty(PropertyName = "manual_fullPath")]
        public string ManualFullPath { get; set; }

        [JsonProperty(PropertyName = "manual_version")]
        public string ManualVersion { get; set; }

        //[JsonProperty(PropertyName = "manual_isArchived")]
        //public bool ManualIsArchived { get; set; }
        [JsonProperty(PropertyName = "manual_archiveStatus")]
        public int ManualArchiveStatus { get; set; }

        [JsonProperty(PropertyName = "manual_approvedBy")]
        public int ManualApprovedBy { get; set; }

        [JsonProperty(PropertyName = "manual_approvedStatus")]
        public int ManualApprovedStatus { get; set; }

        [JsonProperty(PropertyName = "manual_internalApprovedStatus")]
        public int ManualInternalApprovedStatus { get; set; }

        [JsonProperty(PropertyName = "manual_escalatedComment")]
        public string ManualEscalatedComment { get; set; }

        [JsonProperty(PropertyName = "manual_extendTime")]
        public long ManualExtendTime { get; set; }

        [JsonProperty(PropertyName = "manual_extendComment")]
        public string ManualExtendComment { get; set; }

        [JsonProperty(PropertyName = "manual_collectionTime")]
        public long ManualCollectionTime { get; set; }

        [JsonProperty(PropertyName = "manual_archivedTime")]
        public long ManualArchivedTime { get; set; }

        [JsonProperty(PropertyName = "manual_actionTime")]
        public long ManualActionTime { get; set; }

        [JsonProperty(PropertyName = "manual_ruleName")]
        public string ManualRuleName { get; set; }

        [JsonProperty(PropertyName = "manual_ruleCriteria")]
        public string ManualRuleCriteria { get; set; }

        [JsonProperty(PropertyName = "manual_ruleDisposalClass")]
        public string ManualRuleDisposalClass { get; set; }

        [JsonProperty(PropertyName = "manual_audits")]
        public string ManualAudits { get; set; }

        [JsonProperty(PropertyName = "manual_relatedRecords", NullValueHandling = NullValueHandling.Ignore)]
        public string ManualRelatedRecords { get; set; }

        [JsonProperty(PropertyName = "manual_isRelatedRecords")]
        public bool ManualIsRelatedRecords { get; set; }

        [JsonProperty(PropertyName = "manual_RelatedRecordsAction")]
        public int ManualRelatedRecordsAction { get; set; }

        [JsonProperty(PropertyName = "manual_partitionKey")]
        public string ManualPartitionKey { get; set; }

        [JsonProperty(PropertyName = "manual_rowKey")]
        public string ManualRowKey { get; set; }

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

        [JsonProperty(PropertyName = "manual_isManualSynced")]
        public bool IsManualSynced { get; set; }
        /// <summary>
        ///用于标记Archived数据的Retention approve状态  不为0时是Retention的manual report
        /// </summary>
        [JsonProperty(PropertyName = "manual_retentionStatus")]
        public int ManualRetentionStatus { set; get; }

        [JsonProperty(PropertyName = "manual_approvalComment")]
        public string ManualApprovalComment { get; set; }

        [JsonProperty(PropertyName = "manual_quickReason")]
        public string QuickReason { get; set; }
        [JsonProperty(PropertyName = "manual_lastReasonForRejection")]
        public string ManualLastReasonForRejection { get; set; }

        [JsonProperty(PropertyName = "manual_lastApproveRejectComment")]
        public string ManualLastApproveRejectComment { get; set; }

        [JsonProperty(PropertyName = "manual_lastReviewedBy")]
        public string ManualLastReviewedBy { get; set; }

        [JsonProperty(PropertyName = "manual_lastReviewTime")]
        public long ManualLastlReviewTime { get; set; }

        [JsonProperty(PropertyName = "manual_lastExtendType")]
        public ManualApprovalExtendType ManualLastExtendType { get; set; }

        [JsonProperty(PropertyName = "manual_lastCustomeExtendDate")]
        public DateTime ManualLastCustomeExtendDate { get; set; }

        [JsonProperty(PropertyName = "manual_folderPath")]
        public string ManualFolderPath { get; set; }

        [JsonProperty(PropertyName = "manual_siteUrl")]
        public string ManualSiteUrl { get; set; }

        [JsonProperty(PropertyName = "manual_modifiedTime")]
        public long ManualModifiedTime { get; set; }

        [JsonProperty(PropertyName = "manual_disposalDueDate")]
        public long ManualDisposalDueDate { get; set; }
        #endregion

        #region Pick Status

        [JsonProperty(PropertyName = "loanPickStatus")]
        public int LoanPickStatus { get; set; }

        [JsonProperty(PropertyName = "destructionPickStatus")]
        public int DestructionPickStatus { get; set; }

        [JsonProperty(PropertyName = "training_Scope")]
        public int TrainingScope { get; set; }

        [JsonProperty(PropertyName = "training_TermId")]
        public Guid TrainingTermId { get; set; }

        [JsonProperty(PropertyName = "training_addType")]
        public int TrainingAddType { get; set; }

        #endregion

        #region Machine Learning

        [JsonProperty(PropertyName = "ai_predictTermId")]
        public Guid PredictTermId { get; set; }

        [JsonProperty(PropertyName = "ai_predictTermScore")]
        public double PredictTermScore { get; set; }

        [JsonProperty(PropertyName = "ai_predictTime")]
        public long PredictTime { get; set; }

        [JsonProperty(PropertyName = "ai_underReview")]
        public int MLUnderReview { get; set; }

        [JsonProperty(PropertyName = "ai_classificationType")]
        public int MLClassificationType { get; set; } //Manual/AutoApply

        [JsonProperty(PropertyName = "ai_reviewer", NullValueHandling = NullValueHandling.Ignore)]
        public int[] MLReviewer { get; set; }

        [JsonProperty(PropertyName = "ai_approvalStatus")]
        public int MLApprovalStatus { get; set; }

        [JsonProperty(PropertyName = "ai_escalateFrom")]
        public int MLEscalateFrom { get; set; }

        [JsonProperty(PropertyName = "ai_escalatedComment")]
        public string MLEscalatedComment { get; set; }

        [JsonProperty(PropertyName = "ai_trainingModelId")]
        public Guid TrainingModelId { get; set; }

        [JsonProperty(PropertyName = "ai_trainingParseTimeoutFile")]
        public bool TrainingParseTimeoutFile { get; set; }

        #endregion

        #region GControl Properties

        [JsonProperty(PropertyName = "gControlTaskId")]
        public string GControlPlatformTaskId { get; set; }

        [JsonProperty(PropertyName = "gControlApprovalProcessId")]
        public string GControlApprovalProcessId { get; set; }
        
        [JsonProperty(PropertyName = "gControlCurrentStageId")]
        public string GControlCurrentStageId { get; set; }
        
        [JsonProperty(PropertyName = "gControlCurrentApproverId")]
        public string GControlCurrentApproverId { get; set; }
        
        [JsonProperty(PropertyName = "gControlManualReviewers")]
        public int[] GControlManualReviewers { get; set; }
        
        [JsonProperty(PropertyName = "gControlCurrentStatus")]
        public int GControlManualApprovedStatus { get; set; }
        
        [JsonProperty(PropertyName = "isGControlRecord")]
        public bool IsGControlRecord { get; set; }
        
        [JsonProperty(PropertyName = "gControlManualInternalApprovedStatus")]
        public int GControlManualInternalApprovedStatus { get; set; }
        #endregion
        [JsonProperty(PropertyName = "isFsControlRecordJPMC")]
        public bool IsFsControlRecordJPMC { get; set; }

        /// <summary>
        /// Cosmos系统字段， 用于标记最新更新时间， 只读
        /// </summary>
        [JsonProperty(PropertyName = "_ts")]
        public long TimeStamp { set; get; }

        /// <summary>
        /// 转换存储原数据中的MetaInfo
        /// </summary>
        //[JsonProperty(PropertyName = "customColumns")]
        //public List<CustomColumn> CustomColumns { set; get; }
        [JsonProperty(PropertyName = CosmosFieldName.CustomColumnDic, NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CustomColumn> CustomColumnDic { set; get; }

        [JsonIgnore]
        public bool CustomColumnNotExist { get; set; }
        [JsonIgnore]
        public bool hasDuplicate { get; set; }

        [JsonIgnore]
        public bool labelNotExist { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        [JsonIgnore]
        public int RetriedCount { get; set; }


        [JsonIgnore]
        public string SpecialComment { get; set; }
        [JsonIgnore]
        public bool IsAutoApproval { get; set; }
        [JsonIgnore]
        public int ManualApprovedStatusForHistory { get; set; }
        [JsonIgnore]
        public int[] ManualReviewerForHistory { get; set; }
        [JsonIgnore]
        public long Depth { get; set; }

        [JsonProperty(PropertyName = "jpmcFileSize")]
        public long JPMCFSFileSize { get; set; }
        [JsonProperty(PropertyName = "jpmcFileCount")]
        public long JPMCFSFileCount { get; set; }

        [JsonProperty(PropertyName = "l1PartitionKey")]
        public string L1PartitionKey { get; set; }

        [JsonProperty(PropertyName = "l2PartitionKey")]
        public string L2PartitionKey { get; set; }

        [JsonProperty(PropertyName = "l3PartitionKey")]
        public string L3PartitionKey { get; set; }

        public PartitionKey BuildPartitionKey()
        {
            if(RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                if (string.IsNullOrWhiteSpace(L1PartitionKey))
                {
                    throw new Exception($"L1PartitionKey is null or whitespace, record id: {Id}");
                }
                if (string.IsNullOrWhiteSpace(L2PartitionKey))
                {
                    throw new Exception($"L2PartitionKey is null or whitespace, record id: {Id}");
                }
                if (string.IsNullOrWhiteSpace(L3PartitionKey))
                {
                    throw new Exception($"L3PartitionKey is null or whitespace, record id: {Id}");
                }
                return new PartitionKeyBuilder()
                    .Add(L1PartitionKey)
                    .Add(L2PartitionKey)
                    .Add(L3PartitionKey)
                    .Build();
            }

            return new PartitionKey(CreateDate);
        }

        public Record SetPartitionKeys()
        {
            if (!RMCosmosDBIndependentController.IsEnabledIndependent())
            {
                return this;
            }

            L1PartitionKey = GetL1PartitionKey();
            if (string.IsNullOrWhiteSpace(L1PartitionKey))
            {
                throw new Exception($"L1PartitionKey is null or whitespace, record id: {Id}");
            }
            L2PartitionKey = GetL2PartitionKey();
            if (string.IsNullOrWhiteSpace(L2PartitionKey))
            {
                throw new Exception($"L2PartitionKey is null or whitespace, record id: {Id}");
            }
            L3PartitionKey = GetL3PartitionKey();
            if (string.IsNullOrWhiteSpace(L3PartitionKey))
            {
                throw new Exception($"L3PartitionKey is null or whitespace, record id: {Id}");
            }
            return this;
        }

        private string GetL1PartitionKey()
        {
            if (!string.IsNullOrWhiteSpace(L1PartitionKey))
            {
                return L1PartitionKey;
            }
            return SourceFlag.ToString();
        }

        private string GetL2PartitionKey()
        {
            if (!string.IsNullOrWhiteSpace(L2PartitionKey))
            {
                return L2PartitionKey;
            }
            return SourceFlag switch 
            {
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.SharePoint => AveSiteId,
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.FileSystem => AveSiteId,
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.Physical => LocationId.ToString(),
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.OneDrive => AveSiteId,
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.Google => AveSiteId,
                (int)AvePoint.RA.Contract.Explorer.SourceFlag.SharePointOnPrem => AveSiteId,
                //(int)AvePoint.RA.Contract.Explorer.SourceFlag.AzureFileShare => ScopeId.ToString(),
                //(int)AvePoint.RA.Contract.Explorer.SourceFlag.Box => ScopeId.ToString(),
                //(int)AvePoint.RA.Contract.Explorer.SourceFlag.SalesForce => ScopeId.ToString(),
                //(int)AvePoint.RA.Contract.Explorer.SourceFlag.Teams => ScopeId.ToString(),
                //(int)AvePoint.RA.Contract.Explorer.SourceFlag.Groups => ScopeId.ToString(),
                _ => ScopeId.ToString(),
            };
        }

        private string GetL3PartitionKey()
        {
            if (!string.IsNullOrWhiteSpace(L3PartitionKey))
            {
                return L3PartitionKey;
            }

            //if (TimeCreated <= 0)
            //{
            //    throw new Exception($"TimeCreated is illegal, record id: {Id}");
            //}
            return TimeCreated % 1000 + "";
            //using var sha256 = SHA256.Create();
            //var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Id.ToString()));
            //var bucket = BitConverter.ToInt32(hashBytes) % 1000;
            //return bucket + "";
        }

        public Record CreateFromExisted()
        {
            Record record = new();
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                property.SetValue(record, property.GetValue(this));
            }
            return record;
        }
    }
}

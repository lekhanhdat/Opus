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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Explorer
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FileSystemRecordDto
    {
        [DataMember(EmitDefaultValue = false)]
        public int Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int CreateDate { set; get; }

        // <summary>
        /// -1: None
        /// 1: SharePoint
        /// 2: FileSystem
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int SourceFlag { get; set; }
        /// <summary>
        /// sp: real site id
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid ScopeId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid NodeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DirPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RuleName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RecordsId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int NodeType { get; set; }
        /// <summary>
        /// LeafName 是FullTextIndex字段, 需要not null
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string LeafName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ExtensionForFile { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid RuleId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RuleLevel { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool HoldStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long HoldReleaseTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int HoldType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RelatedRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RelatedRecordsCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CreatedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DisposalDueDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PreviosDisposalDueDate { get; set; }//TODO merge July2020

        [DataMember(EmitDefaultValue = false)]
        public bool DeclareAsRecord { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime TimeCreated1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long TimeLastModified { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long CollectionTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RecordHistory { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long SortTicks { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ClassCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string CountryCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int RetentionType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public long StartDate { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public long EndTime { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int PolicyValueUnit { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int PolicyValueNumber { set; get; }

        #region for SP
        /// <summary>
        /// docave siteId, not real sp siteId
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string AveSiteId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid WebId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ListId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid FolderId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ItemId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ItemRowId { get; set; }
        /// <summary>
        /// FullPath 是FullTextIndex字段, 需要not null
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SourceLocation { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DestinationLocation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MetaInfo { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        public string DeclaredBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ModifiedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Extsion1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ParentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldByUsers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string HoldUntilTimes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string[] AppendHolds_Array { get; set; }

        /// <summary>
        /// **此属性不足以判断具体数据类型，如果需要当做条件，应确认好原端类型以及ID 等字段进行确认
        /// 1:active 2, archived, 3 delete, 4 moved, 5 overwrited(Move job destination file can be overwrited)
        /// For physical: 1:Open, 2:Destroyed, 3: delete(RM 删除的文件，理论上不显示),  6:closed, 7: Missing. 不使用3， 4， 5 防止与其他值混淆
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int RecordStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long DestroyedTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ADSID { get; set; }

        #region Physical Property
        /// <summary>
        /// Nearest Parent Location Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid LocationId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid BoxId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid FileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TemplateId { get; set; }
        #endregion

        #region not mapped propertity
        //[NotMapped]
        //public string FullPath { get; set; }
        #endregion

        #region Manual Approval

        [DataMember(EmitDefaultValue = false)]
        public int[] ManualReviewer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualEscalateFrom { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ManualWorkflowInstanceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ManualWorkflowDefinitionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid ManualWorkflowStepId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualFullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ManualFolderPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualSiteUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualArchiveStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualApprovedBy { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualApprovedStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualInternalApprovedStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualEscalatedComment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long ManualExtendTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualExtendComment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long ManualCollectionTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long ManualArchivedTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long ManualActionTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualRuleName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualRuleCriteria { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualRuleDisposalClass { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualAudits { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualRelatedRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ManualIsRelatedRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualRelatedRecordsAction { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualPartitionKey { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ManualRowKey { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsManualSynced { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualExtendCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ManualEmailNotificationCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long ManualEmailNotificationLastTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ManualNeedEmailNotification { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ManualIsAutoReassigned { get; set; }
        #endregion

        #region
        [DataMember(EmitDefaultValue = false)]
        public int HasRelatedDocument { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int DeleteRelatedRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RelatedRecordInfo { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        public bool BulkImportEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int BulkSize { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FileSystem.FSJobType FSJobType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool hasDuplicated { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long FileSize { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FSSettingDto FSSettingDto { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool HasTermChanged { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool HasRuleChanged { get; set; }

        public int RuleAction { get; set; }
        public long Depth { get; set; }
        public long DiscoverOrder { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long JPMCFSFileSize { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long JPMCFSFileCount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RMAgentSyncFailureItem
    {
        [DataMember(EmitDefaultValue = false)]
        public string SiteId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string WebId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ListId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ItemId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int IntemIntId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ParentId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string JobId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string Message { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string URL { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ObjectName { set; get; }
        //for fs, store guid of dirpath
        [DataMember(EmitDefaultValue = false)]
        public string NodeId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public long SortTicks { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int SourceFlag { set; get; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ClassCodeInfoDto
    {
        [DataMember(EmitDefaultValue = false)]
        public string ClassCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string CountryCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int RetentionType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public long StartDate { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long EndTime { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int PolicyValueUnit { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int PolicyValueNumber { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool ApplyExistDocuments { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool EnableRecordManagement { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid NodeId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public long CollectionTime { set; get; }
    }
    [DataContract(IsReference = true)]
    [JsonObject]
    public class OlderThanTimeDtoForAgent
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Number { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PolicyValueUnit { set; get; }

    }
}

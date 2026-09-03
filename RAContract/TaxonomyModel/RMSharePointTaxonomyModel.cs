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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.TaxonomyModel
{
    public class TermGroupModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Icon { get; set; }
        public List<TermSetModel> Children { get; set; }
        public string Description { get; set; }
        public string Type { get { return "TermGroup"; } }
        public int ChildrenCount { get; set; }
    }

    public class TermSetModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Icon { get; set; }
        public List<TermModel> Children { get; set; }
        public string Description { get; set; }
        public string Type { get { return "TermSet"; } }
        public int ChildrenCount { get; set; }

    }

    public class TermModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Icon { get; set; }
        public List<TermModel> Children { get; set; }
        public string Description { get; set; }
        public string Type { get { return "Term"; } }
        public int ChildrenCount { get; set; }
    }
    public enum IconEnum
    {
        //图片的名字
        EMMTermSet,
        EMMTerm,
        EMMGroup
    }
    [DataContract]
    public class TreePage
    {
        [DataMember]
        public int? PageIndex { get; set; }
        [DataMember]
        public int? PageSize { get; set; }
        //待展开的NodeId
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string NodeType { get; set; }
        [DataMember]
        public List<RMSPTreeNode> SPTreeNodes { get; set; }
        [DataMember]
        public int? SettingType { get; set; }
        [DataMember]
        public bool IconStatus { get; set; }
        [DataMember]
        public string ContainerId { get; set; }
        [DataMember]
        public SourceFlag SourceFlag { get; set; }
        [DataMember]
        public bool ExcludeBuiltIn { get; set; }
        [DataMember]
        public bool ForPhysicalView { get; set; }
        [DataMember]
        public bool ShowAllTerms { get; set; }
    }

    [DataContract]
    public class LabelPageModel
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int LabelId { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string SearchKey { get; set; }
        [DataMember]
        public SourceFlag SourceFlag { get; set; }
    }

    [RACodeReview("Allen Yin", comment: "命名费解")]
    [DataContract]
    public class CurrentSettingsInfo
    {
        //当前展开到的NodeId
        [DataMember] 
        public string CurrentNodeId { get; set; }
        [DataMember]
        public string TermSetId { get; set; }
        [DataMember]
        public List<RMSPTreeNode> spTreeNodes { get; set; }
        [DataMember]
        public string GroupId { get; set; }
        //分页每一页的term数
        [DataMember]
        public int perPageCount { get; set; }
        [DataMember]
        public string AgentGroupId { get; set; }
        [DataMember]
        public int SettingType { get; set; }

        //For FS 
        [DataMember] 
        public string ConnGroupId { get; set; }
    }
    public class SaveTreePage
    {
        public List<RMSPTreeNode> allRMSPTreeNode { get; set; }
        public SettingsType settingsType { get; set; }
        public bool NeedCheckDefaultVaule { get; set; }
        public ApplyExistingTermType applyType { get; set; }
        public bool EnableRelatedRecords { get; set; }

    }
    public enum SettingsType
    {
        GlobalSettings,
        CustomSettings
    }
    public enum SaveSPSettingResult
    {
        Sucess,
        Failed,
        UpdateCommonDataFailed,
    }
    public class RuleInfo
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RuleOrder { get; set; }
        public string RuleLevel { get; set; }
    }
    public class TermAuditInfo
    {
        public bool IsBreakInheritance { get; set; }
        public string RuleNames { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public bool Permanent { get; set; }
        public int EnfoceRentention { get; set; }
        public bool IsRootTerm { get; set; }
        public string ExchangeLabel { get; set; }
        public string SPLabel { get; set; }
        public string OneDriveLabel { get; set; }
        public string TeamsLabel { get; set; }
    }

    public class TermGroupAuditInfo
    {
        //public string Name { get; set; }
        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public string Description { get; set; }
        public string M365TermSyncOption  { get; set; }
        public string GoogleTermSyncOption  { get; set; }
        public string UsingAllMMSSMessage { get; set; }
        public string UsingpecificMMSSMessage { get; set; }
        public string UsingNoneMMSSMessage { get; set; }
        public string UsingSpecificGoogleMessage { get; set; }
        public string UsingAllGoogleMessage { get; set; }
        public string UsingNoneGoogleMessage { get; set; }
    }

    public enum SaveTimeErrorType
    {
        startTimeIsEarlierNow = 1,
        endTimeIsEarlierNow = 2,
        fromTimeGtToTime = 3,
        sTimeIsNull = 4,
        eTimeIsNull = 5,
        fTimeAndToTimeIsNull = 6
    }
    public enum DateType
    {
        startTime = 0,
        endTime = 1,
        fromTimeAndToTime = 2,
        noExpireDate = 3
    }
    public enum CreateTermSetErrorType
    {
        [Description("Has Exists Term Set")]
        IsExists = 0,
        [Description("Term Set has same name")]
        HasSame = 1
    }
    //public class TermInfo
    //{
    //    public int TermId { get; set; }
    //    public int TermSetId { get; set; }
    //    public int TermGroupId { get; set; }
    //    public int ParentTermId { get; set; }
    //    public Guid TermGroupUniqueId { get; set; }
    //    public string TermName { get; set; }
    //    public string TermSetName { get; set; }
    //    public string TermGroupName { get; set; }
    //    public string TermStoreId { get; set; }
    //    public string TermStoreName { get; set; }
    //    public string Description { get; set; }
    //    public bool UsingMMSSpecified { get; set; }
    //    public List<RMSiteInfo> ReSiteInfos { get; set; }
    //}
    public class TermInfoWithRule
    {
        public string TermName { get; set; }
        public string TermDescription { get; set; }
        public string TermStatus { get; set; }
        public string RuleName { get; set; }
        public string RuleDescription { get; set; }
        public string RuleLevel { get; set; }
        public string Criteria { get; set; }
        public string Action { get; set; }
        public string EnableManualApproval { get; set; }
        public string SendEmailRecordOwner { get; set; }
        public string RecordOwner { get; set; }
        public string DeleteRecords { get; set; }
        public string IncludeDeleteRecordLabel { get; set; }
        public string DeleteSiteCollectionToRecycleBin { get; set; }
        public string ExportSharePointContent { get; set; }
        public string ExportFormat { get; set; }
        public bool Permanent { get; set; }
        public string EnforceRetention { get; set; }
        public string IncludeRelatedRecord { get; set; }
        public string DisposalClass { get; set; }
        public bool IsSPSource { get; set; }
        public bool IsEXOSource { get; set; }
    }
    public enum ApplyExistingTermType
    {
        None = 0,
        OverWrite = 1,
        SkipAndKeep = 2
    }
    [DataContract]
    public enum AutoJobOption
    {
        [EnumMember]
        None = 0,
        [EnumMember] 
        SkipAndKeep = 1,
        [EnumMember] 
        Override = 2,
        [EnumMember]
        Append = 3
    }
    [DataContract]
    public enum CleanRestoreOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        FileOrVersionOnly = 1,
        [EnumMember]
        FileAndReletedVersions = 2
    }
    [DataContract]
    public class FSTreePage
    {
        [DataMember]
        public int? PageIndex { get; set; }
        [DataMember]
        public int? PageSize { get; set; }
        //待展开的NodeId
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string NodeType { get; set; }
        [DataMember]
        public string ConnGroupId { get; set; }
        [DataMember]
        public SettingType? SettingType { get; set; }
    }

    public class CurrentFSSettingsInfo
    {
        //当前展开到的NodeId
        public string CurrentNodeId { get; set; }
        public string TermSetId { get; set; }
        public string ConnGroupId { get; set; }

        //分页每一页的term数
        public int perPageCount { get; set; }
        public int SettingType { get; set; }
    }

    public class CurrentPRSettingsInfo
    {
        //当前展开到的NodeId
        public Guid CurrentTermId { get; set; }
        public Guid TermSetId { get; set; }
        public Guid GroupId { get; set; }
        //分页每一页的term数
        public int PerPageCount { get; set; }
        public int SettingType { get; set; }
    }

    public class CurrentOneDriveSettingsInfo
    {
        //当前展开到的NodeId
        public string CurrentNodeId { get; set; }
        public string TermSetId { get; set; }
        public string GroupId { get; set; }
        //分页每一页的term数
        public int perPageCount { get; set; }
        public string AgentGroupId { get; set; }
        public int SettingType { get; set; }
    }

    public class DeclarationSetting
    {
        //0: none 1: block delete 2: block and delete
        public int RecordRestrictions { get; set; }
    }

    public enum SettingType
    {
        None = 0,
        LoadByGroup = 1
    }
    [DataContract]
    public class TermTreeView
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int? PageIndex { get; set; }
        [DataMember]
        public string PagePosition { get; set; }
        [DataMember]
        public string NodeType { get; set; }
        [DataMember]
        public string TermId { get; set; }
    }
    [DataContract]
    public class RunPhysicalJobParam
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public bool SkipRemove { get; set; }
    }

    [DataContract]
    public class ClassCodeRequest
    {
        [DataMember]
        public Guid TermSetId { get; set; }
        [DataMember]
        public string SearchKey { get; set; }
        [DataMember]
        public int PageIndex { get; set; } = 0;
        [DataMember]
        public int PageSize { get; set; } = 0;
    }
}

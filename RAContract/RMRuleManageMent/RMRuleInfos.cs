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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMRuleManageMent
{
    [RACodeReview("Allen Yin")]
    [DataContract]
    public class RMRuleInfos
    {
        [DataMember]
        public string RuleId { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public RuleModel ModelType { get; set; }
        [DataMember]
        public PolicyLevel RuleLevel { get; set; }
        [DataMember]
        public string Modified { get; set; }
        [DataMember]
        public bool isChecked { get; set; }
        //public string Criteria { get; set; }
        [DataMember]
        public string ArchiverActions { get; set; }
        [DataMember]
        public string ExportFormat { get; set; }
        [DataMember]
        public int RuleKeepDataOption { get; set; }

        //for export info
        [DataMember]
        public SOExportInfo ExportInfo { get; set; }
        //for keep data tag
        [DataMember]
        public List<RMTagContentInfo> TagContentInfo { get; set; }
        [DataMember]
        public bool EnableManualApproval { get; set; }
        [DataMember]
        public bool EnableExport { get; set; }
        [DataMember]
        public bool ExportDataBeforeArchiving { get; set; }
        [DataMember]
        public bool DeclareLinkFile { get; set; }
        //for build rule
        [DataMember]
        public List<RuleFilter> RuleFilters { get; set; }
        //for display rule details
        [DataMember]
        public List<string> RuleCretias { get; set; }
        [DataMember]
        public MoveToRecordCenterAndDelareSetting MoveToRecordCenterSettings { get; set; }
        [DataMember]
        public bool MoveToLocationPasswordEncrypted { get; set; }
        [DataMember]
        public string FilterCombineMode { get; set; }
        [DataMember]
        public string DisposalClass { get; set; }
        [DataMember]
        public AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption RelatedRecordOption { get; set; }
        [DataMember]
        public bool DestroyEmptyBoxOnFolderRule { get; set; }
        [DataMember]
        public bool DeleteRecords { get; set; }
        [DataMember]
        public bool IncludeDeleteRecordLabel { get; set; }
        [DataMember]
        public bool LockRecordBeforeDestroy { get; set; } = true;
        [DataMember]
        public bool DeleteSiteCollectionToRecycleBin { get; set; }
        [DataMember]
        public bool DeleteToRecycleBin { get; set; }
        [DataMember]
        public bool IsSendEmailToOwner { get; set; }
        [DataMember]
        public List<AOSUserDto> Users { get; set; }
        /******************************/
        [DataMember]
        public MoveToDto MoveDto { get; set; }
        [DataMember]
        public bool IsSpSource { get; set; }
        [DataMember]
        public bool IsExoSource { get; set; }
        [DataMember]
        public bool IsPhySource { get; set; }
        [DataMember]
        public bool IsFSSource { get; set; }
        [DataMember]
        public bool IsSPLocalSource { get; set; }
        [DataMember]
        public bool IsOneDriveSource { get; set; }
        [DataMember]
        public bool IsAzureFileSource { get; set; }
        [DataMember]
        public bool IsBoxSource { get; set; }
        [DataMember]
        public bool IsConnectorSource { get; set; }
        [DataMember]
        public bool IsGoogleDriveSource { get; set; }
        [DataMember]
        public bool IsTeamsSource { get; set; }
        [DataMember]
        public RMRuleInfos EXORule { get; set; }
        [DataMember]
        public RMRuleInfos PhysicalRule { get; set; }
        [DataMember]
        public RMRuleInfos FSRule { get; set; }
        [DataMember]
        public RMRuleInfos SPLocalRule { get; set; }
        [DataMember]
        public RMRuleInfos OneDriveRule { get; set; }
        [DataMember]
        public RMRuleInfos AzureFileRule { get; set; }
        [DataMember]
        public RMRuleInfos BoxRule { get; set; }
        [DataMember]
        public RMRuleInfos ConnectorRule { get; set; }
        [DataMember]
        public RMRuleInfos GoogleDriveRule { get; set; }
        [DataMember]
        public RMRuleInfos TeamsRule { get; set; }
        [DataMember]
        public long ModifiedTicks { get; set; }
        [DataMember]
        public int MyProperty { get; set; }
        [DataMember]
        public ReviewType ManualReviewType { get; set; }
        [DataMember]
        public string WorkflowId { get; set; }
        [DataMember]
        public string WorkflowName { get; set; }
        [DataMember]
        public bool IsGControlManualApproval { get; set; }
        [DataMember]
        public string LeaveStubMessage { get; set; }
        [DataMember]
        public Guid ContainerId { get; set; }
        [DataMember]
        public string ContainerName{ get; set; }
        [DataMember]
        public bool IsRestoreLink { get; set; }
        [DataMember]
        public bool IsEnableRetention { set; get; }
        [DataMember]
        public RetentionSettings RetentionInfo { set; get; }
        [DataMember]
        public string StoragePolicyId { get; set; }
        [DataMember]
        public string StoragePolicyName { get; set; }
        [DataMember]
        public int StoragePolicyType { get; set; }
        [DataMember]
        public bool IsSystemStorage { get; set; }
        [DataMember]
        public string StubTemplateId { get; set; }
        [DataMember]
        public string StubTemplateName { get; set; }
        [DataMember]
        public bool MoveToArchiverTierWhenArchiving { get; set; }
        [DataMember]
        public int ArchivedLatestVersion { get; set; }
        [DataMember]
        public int KeepLatestMajorAndMinorVersion { get; set; }
        [DataMember]
        public int KeepLatestMajorAndMinorVersionAndArchiveOthers { get; set; }
        [DataMember]
        public int ArchiverOnlyLastestVersion { get; set; }
        [DataMember]
        public bool IsMoveToSP { set; get; }
        [DataMember]
        public List<MoveMetadataInfo> MoveToSPDataList { set; get; }
        [DataMember]
        public int? MoveToAnotherTierType { get; set; }
        [DataMember]
        public List<RetentionSettings> RetentionInfoList { set; get; }
        [DataMember]
        public bool IsCalculationDisposalDate { set; get; }
    }

    public static class RMRuleInfosExtension
    {
        public static bool IsDeleteSiteCollectionToRecycleBin(this RMRuleInfos rule)
        {
            if (rule.IsSpSource && rule.RuleLevel == PolicyLevel.SiteCollection 
                && rule.DeleteSiteCollectionToRecycleBin && rule.IsRuleActionCanDeleteSC())
            {
                return true;
            }
            return false;
        }

        public static bool IsRuleActionCanDeleteSC(this RMRuleInfos rule)
        {
            //string strArchiverActions = "";
            int keepDataOption = rule.RuleKeepDataOption;
            var canDeleteSCAction = false;
            if (!rule.IsSpSource || rule.RuleLevel != PolicyLevel.SiteCollection)
            {
                return canDeleteSCAction;
            }
            else
            {
                if ((keepDataOption & (int)KeepDataStatus.ArchiverOnly) == (int)KeepDataStatus.ArchiverOnly)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_Backup";
                }
                else if (rule.ExportInfo != null && rule.ExportInfo.exportSPDataOption == ExportSPDataOption.ExportWithoutArchive)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ExportOnly";
                }
                else if ((keepDataOption & (int)KeepDataStatus.Delete) != (int)KeepDataStatus.Delete
                    && (keepDataOption & (int)KeepDataStatus.Remove) != (int)KeepDataStatus.Remove
                    && (keepDataOption & 128) != (int)KeepDataStatus.LinkToDocument
                    && (keepDataOption & 256) != (int)KeepDataStatus.NotBackup
                    && (keepDataOption & (int)KeepDataStatus.Vault) != (int)KeepDataStatus.Vault
                    && (keepDataOption & (int)KeepDataStatus.Archive) != (int)KeepDataStatus.Archive
                    && (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) != (int)KeepDataStatus.ArchiveAndLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) != (int)KeepDataStatus.ArchiveBackupAndRemove
                    && (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) != (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub
                    && (keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) != (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndKeep";
                }
                else if (ExcludeOptionUnderMoveAction(keepDataOption) == (int)KeepDataStatus.Delete && rule.MoveDto != null)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_MoveRecord";
                }
                else if ((keepDataOption & (int)KeepDataStatus.Archive) == (int)KeepDataStatus.Archive
                    || (keepDataOption & (int)KeepDataStatus.ArchiveAndLeaveStub) == (int)KeepDataStatus.ArchiveAndLeaveStub)
                {
                    //strArchiverActions = "RM_RDM_CreateRule_ArchiveToAzureBlobStorage";
                }
                else if ((keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemove) == (int)KeepDataStatus.ArchiveBackupAndRemove
                    || (keepDataOption & (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub) == (int)KeepDataStatus.ArchiveBackupAndRemoveLeaveStub)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_BackupAndRemove";
                    canDeleteSCAction = true;
                }
                else if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_StoreInM365Archive";
                }
                else if (rule.TagContentInfo != null && rule.TagContentInfo.Any()) { }
                else if (keepDataOption == 20) { }
                else
                {
                    //strArchiverActions = "RM_JS_RDM_CreateRule_Options_ArchiveAndRemove";
                    canDeleteSCAction = true;
                }
            }
            return canDeleteSCAction;
        }

        public static int ExcludeOptionUnderMoveAction(int keepDataOption)
        {
            if ((keepDataOption & (int)KeepDataStatus.IsEnableRemoveRetentionLabel) == (int)KeepDataStatus.IsEnableRemoveRetentionLabel)
            {
                keepDataOption -= (int)KeepDataStatus.IsEnableRemoveRetentionLabel;
            }
            if ((keepDataOption & (int)KeepDataStatus.TriggerMicrosoft365Archiving) == (int)KeepDataStatus.TriggerMicrosoft365Archiving)
            {
                keepDataOption -= (int)KeepDataStatus.TriggerMicrosoft365Archiving;
            }
            return keepDataOption;
        }
    }

    [DataContract]
    public class RetentionSettings
    {
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public TimeFilterCondition Condition { set; get; }
        [DataMember]
        public TimeUnit KeepDateUnite { get; set; }
        [DataMember]
        public int KeepDateNumber { get; set; }
        /// <summary>
        /// Local时间用于前台控件回显， 后台使用要转成UTC
        /// </summary>
        [DataMember] 
        public string Date { set; get; }
        [DataMember]
        public bool IsManualApproval { get; set; }
        [DataMember]
        public ReviewType ReviewType { get; set; }
        [DataMember]
        public string WorkflowId { get; set; }
        [DataMember]
        public bool IsSendEamilToOwner { get; set; }
        [DataMember]
        public List<UserInfo> UserInfos { get; set; }
        [DataMember]
        public bool RemoveOrphanedStub { get; set; }
        [DataMember]
        public int? OperateDataType { get; set; }
        [DataMember]
        public int? TierType { get; set; }
        [DataMember]
        public bool IsEnableRetention { get; set; }
        [DataMember]
        public KeepDateType RetentionDataTimeType { get; set; }
        [DataMember]
        public TimeUnit SoftKeepDateUnite { get; set; }
        [DataMember]
        public int SoftKeepDateNumber { get; set; }
        [DataMember]
        public bool IsSoftDelete { get; set; }
    }

    #region For AutoClassification
    [DataContract]
    public class ClassificationRule
    {
        //public bool UseDefaultTerm;
        [DataMember]
        public bool IsDefaultRule { get; set; }
        [DataMember]
        public bool NoDefaultTerm { get; set; }
        [DataMember]
        public string TermId { get; set; }
        [DataMember]
        public string TermName { get; set; }
        [DataMember]
        public bool TermIsRemoved { get; set; }
        [DataMember]
        public bool TermIsDeprecated { get; set; }
        [DataMember]
        public bool TermHasNoPermission { get; set; }
        [DataMember]
        public PolicyLevel RuleLevel { get; set; }
        [DataMember]
        public PolicyLevel Category { get; set; }
        [DataMember]
        public int RuleOrder { get; set; }
        [DataMember]
        public List<FilterGroup> FilterGroups { get; set; }
        [DataMember]
        public string AndOrExpression { get; set; }
        [DataMember]
        public bool TermExistingTermGroup { get; set; }
    }
    [DataContract]
    public class FilterGroup
    {
        [DataMember]
        public List<RuleFilter> Filters { get; set; }
        [DataMember]
        public List<FilterGroup> FilterGroups { get; set; }
        /// <summary>
        /// And Or
        /// </summary>
        [DataMember]
        public ArchiverFilterCombineMode CombineMode { get; set; }
        /// <summary>
        /// True False
        /// </summary>
        [DataMember]
        public string TrueFalse { get; set; }
    }
    #endregion
    [DataContract]
    public class RuleFilter
    {
        [DataMember]
        public int SequenceNo { get; set; }
        [DataMember]
        public PolicyLevel Level { get; set; }
        [DataMember]
        public ArchiverFilterCondition Condition { get; set; }
        [DataMember]
        public ArchiverFilterCombineMode CombineMode { get; set; }
        [DataMember]
        public ArchiverFilterRuleType RuleType { get; set; }
        [DataMember]
        public string filterName { get; set; }
        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Value2 { get; set; }
        [DataMember]
        public string Value3 { get; set; }
        [DataMember]
        public PolicyValueUnit Value1Unit { get; set; }
        [DataMember]
        public PolicyValueUnit Value2Unit { get; set; }
        [DataMember]
        public PolicyValueUnit Value3Unit { get; set; }
        [DataMember]
        public string FilterCretia { get; set; }
        [DataMember]
        public PolicyRuleBase RuleBase { get; set; }
        [DataMember]
        public DisplayDateTime StartTimeInfo { get; set; }
        [DataMember]
        public DisplayDateTime EndTimeInfo { get; set; }
    }
    [DataContract]
    public class RuleDisplayInfo
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string RuleId { get; set; }
        [DataMember]
        public string RuleName { get; set; }
        [DataMember]
        public int RuleOrder { get; set; }
        [DataMember]
        public string RuleLevel { get; set; }
    }

    [RACodeReview("Allen Yin")]
    public class RMRuleTermInfos
    {
        public string RuleName { get; set; }
        public string RuleId { get; set; }
        public string TermNames { get; set; }
    }

    [RACodeReview("Allen Yin", comment: "Terms 这个名字不太容易理解")]
    public class RMRuleTermsDto
    {
        public bool HasTerms { get; set; }
        public int TermsCount { get; set; }
        public List<RMRuleTermInfos> Terms { get; set; }
    } 

    public class MoveDestinationInfo
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
    }
    [DataContract]
    public class RMTagContentInfo
    {
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public bool IsDayLightSaving { get; set; }
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public DateTime DateTime { get; set; }
        [DataMember]
        public TagContentInfoType Type { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public int Option { get; set; }
    }

    public class RMSimpleRule
    {
        public Guid RuleId { get; set; }
        public string RuleName { get; set; }
        public int RuleOrder { get; set; }
        public int IntRuleLevel { get; set; }
    }

    public enum RMRuleSourceType
    {
        SP = 1,
        FS = 2,
        EXO = 3,
        Physical = 4,
        SPLocal = 5,
        OneDrive = 6,
        AzureFile = 7,
        Box = 8,
        GoogleDrive = 9,
        Teams = 10,
        Connector = 999
    }
    public enum OperateDateTypeEnum
    {
        None = 0,
        Delete = 1,
        MarkTier = 2,
    }
}

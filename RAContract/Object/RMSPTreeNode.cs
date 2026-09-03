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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Object.Base;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    [JsonObject]
    public class RMSPTreeNode : RMBaseTreeNode<RMSPTreeNode>, IDisposable
    {
        #region == basetreenode属性 老数据需要 ==
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Id { set { base.Id = value; } get { return base.Id; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Level { set { base.Level = value; } get { return base.Level; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Name { set { base.Name = value; } get { return base.Name; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DisplayName { set { base.DisplayName = value; } get { return base.DisplayName; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Title { set { base.Title = value; } get { return base.Title; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullPath { set { base.FullPath = value; } get { return base.FullPath; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int NodeType { set { base.NodeType = value; } get { return base.NodeType; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool Hidden { set { base.Hidden = value; } get { return base.Hidden; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool? IsOrphenOneDrive { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ChildrenCount { set { base.ChildrenCount = value; } get { return base.ChildrenCount; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool? Loaded { set { base.Loaded = value; } get { return base.Loaded; } }

        /// <summary>
        /// IncludeNew为-1代表当前节点没有Include New的逻辑，为0代表不是Include New，为1代表是Include New
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int IncludeNew { set { base.IncludeNew = value; } get { return base.IncludeNew; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool Expanded { set { base.Expanded = value; } get { return base.Expanded; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ParentId { set { base.ParentId = value; } get { return base.ParentId; } }

        /// <summary>
        /// CheckNumber为1代表当前节点是Checked状态，为0代表UnChecked状态
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int CheckNumber { set { base.CheckNumber = value; } get { return base.CheckNumber; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<string> ChildrenIds { set { base.ChildrenIds = value; } get { return base.ChildrenIds; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public IconStatus IconStatus { set { base.IconStatus = value; } get { return base.IconStatus; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int PageIndex { set { base.PageIndex = value; } get { return base.PageIndex; } }
        #endregion
        #region == ForArchivrer ==
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsWorkflowDefinition { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsManagedMetadataService { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsEnableSuperUserDecrypt { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMSimpleRule> Rules { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int EnableArchiverManagement { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ContentSourceType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public EndUserArchiveSiteCollectionConfig EndUserArchiveSiteCollectionConfig { get; set; }

        #endregion
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SPObjectId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FarmId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FarmName { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SPType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SPVersion { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int TemplateId { set; get; }

        private string teamName;

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TeamName
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string emailPattern = @"^(?i)[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
                    Regex emailRegex = new (emailPattern, RegexOptions.IgnoreCase);

                    teamName = emailRegex.IsMatch(value) ? value.Split('@')[0] : value;
                }
            }
            get { return teamName; }

        }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TeamsId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public BposInfo BposInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMSPTreeNode Parent { set { base.Parent = value; } get { return base.Parent; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMSPTreeNode> Children { set { base.Children = value; } get { return base.Children; } }

        #region for sharepoint column settings

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermStoreId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermSetId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid DefaultTermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermNameFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermSetName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermNameFullPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool InitDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDefaultTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTermDeprecated { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDefaultTermDeprecated { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DescriptionOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsInheritParentTerm { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TermIdOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermNameOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool isEnableClassification { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int EnableRecordManagement { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool isFailedConfigClassification { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool isFailedConfigMetaDataColumn { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsClassificationTermRemoved { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsClassificationTermDeprecated { get; set; }
        [JsonProperty]
        [DataMember(EmitDefaultValue = false)]
        public bool IsEnableRemoveRetentionLabel { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsEnableHoldPhyical { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid WebId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid ListId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid FolderId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid SiteGroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedCheckDefaultValue { get; set; }

        public bool IsEnableUniqueIDSetting { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ExistColumnName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsUsingExistColumnName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid SettingScopeId { get; set; }
        #endregion
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EMailToRecordOwner { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedLoadSchedule { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo ScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsContainScheduleForOwnAndChildNodes { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool HasCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApplyExistType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EnableRelatedRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsDisplyaTermPath { set; get; }
        /// <summary>
        /// ！！！该属性要使用GroupLevel中的
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsShowUniqueId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo DisposeScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo CollectionScheduleInfo { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //[JsonProperty]
        //public string ProfileId { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //[JsonProperty]
        //public bool UseAutoClassification { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ClassificationRule> AutoClassificationRules { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public DeployTermMethod DeployTermMethod { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ColumnRequired { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ColumnHidden { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool RunAutoFullJob { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public AutoJobOption AutoJobOption { get; set; }
        [JsonProperty]
        public bool AlwaysScanAllExistDocuments { set; get; }


        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IncludeDeclaredRecords { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsSyncData { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool SetDocLevelTermForExistColumn { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool SkipRemoveContentAndDestroyAction { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermScopeFullPath { set; get; }
        
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermFullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ContainerTermFullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApprovalType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string WorkflowReferenceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string WorkflowReferenceName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ApplyTermIncludeFolder { get; set; }


        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsNullClassificationSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsKeepSharePointDefaultValue { get; set; }

        #region Ai Term

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ArtificialIntelligenceTermUseType AITermUseType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int AIApprovalType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string AIWorkflowReferenceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string AIWorkflowReferenceName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool AISendEMail { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ToUserInfo> AIReviewers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid AIThenDefaultTermId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string AIThenDefaultTermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool AIThenIsDefaultTermMethod { set; get; }

        #endregion


        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool SetTermForEmptyDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string O365TenantId { get; set; }

        /// <summary>
        /// Site Id
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Guid SiteId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<string> ArchiverImportSitesUrl { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool UserArchiverImportFile { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EnableDelArchivedData { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EnableCleanStubs { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public CleanRestoreOption CleanupAndDelRestoredType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long DayNum { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsProcessApprovalDatasOnly { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string FullUrl { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool SupportLockedSite { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool? EnableLifecycleManagementForSharePointLists { get; set; } = true;
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool SupportArchivedTeams { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public SplitScanDBInfo SplitScanDBInfo { get => splitScanDBInfo; set => splitScanDBInfo = value; }
        [JsonIgnore]
        private SplitScanDBInfo splitScanDBInfo = new SplitScanDBInfo();

        public void Dispose()
        {
            try
            {
                foreach (var child in this.Children)
                {
                    using (child as IDisposable)
                    { }
                }
                this.Children = null;
            }
            catch
            {
                //出现异常概率极低
            }
        }
        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMSPTreeNode Clone()
        {
            return this.MemberwiseClone() as RMSPTreeNode;
        }
    }

    [DataContract]
    [JsonObject]
    public class SplitScanDBInfo
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool FinishedSplitAndRunVritalJob { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string BriefScanDBName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string BriefScanDBFolder { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsLatestVirtalJob { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ArchiveJobSplitLimit ArchiveJobSplitLimit { get; set; }
    }

    public enum EnableRecordManagementSetting
    {
        Enable = 1,
        Disable = 2,
        ParentDisable = 3,
    }

    public enum ArtificialIntelligenceTermUseType
    {
        None = 0,
        ApplyTerm = 1,
        AutoDefault = 2
    }
    public enum ContentSourceType
    {
        None = 0,
        SharePoint = 1,
        OneDrive = 2,
        Teams = 3
    }

    public enum PredictionModeType
    {
        MLTraining = 0,
        ZeroShot = 1
    }
}

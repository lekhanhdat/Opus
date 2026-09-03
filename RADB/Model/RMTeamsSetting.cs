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
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvePoint.RA.Contract.Object;

namespace AvePoint.RA.DB.Model
{
    public class RMTeamsSetting : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid ScopeId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string ColumnName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid FieldId { set; get; }

        [Column(TypeName = "nvarchar")]
        [Required]
        public string FullPath { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid TeamsGroupId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid TeamsId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid SiteId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid WebId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ListId { set; get; }
        [Column(TypeName = "uniqueidentifier")]
        public Guid FolderId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermStoreId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermSetId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DefaultTermId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string TermSetName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar")]
        public string TermName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DefaultTermName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DescriptionOfContainer { get; set; }

        [Column(TypeName = "nvarchar")]
        public string TermNameOfContainer { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermIdOfContainer { set; get; }

        [Column(TypeName = "bit")]
        public bool IsInheritParentTerm { get; set; }

        [Column(TypeName = "bit")]
        public bool IsChangedInheritOption { get; set; }

        [Column(TypeName = "bit")]
        public bool isEnableClassification { get; set; }

        [Column(TypeName = "bit")]
        public bool isFailedConfigClassification { get; set; }

        [Column(TypeName = "bit")]
        public bool isFailedConfigMetaDataColumn { get; set; }

        [Column(TypeName = "bit")]
        public bool IsEnableHoldPhyical { get; set; }


        [Column(TypeName = "nvarchar")]
        public string ExistColumnName { get; set; }

        [Column(TypeName = "bit")]
        public bool IsUsingExistColumnName { get; set; }

        [Column(TypeName = "bit")]
        public bool SetDocLevelTermForExistColumn { get; set; }

        #region use this for quick config custom setting.
        [Column(TypeName = "bit")]
        public bool HaveConfigSetting { get; set; }//to do lock this setting for get job node
        [Column(TypeName = "bigint")]
        public long SettingTime { get; set; }//update the datetime
        [Column(TypeName = "nvarchar(max)")]
        public string NodeInfo { get; set; }
        #endregion
        [Column(TypeName = "bit")]
        public bool NeedCheckDefaultValue { get; set; }

        [Column(TypeName = "bit")]
        public bool EMailToRecordOwner { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDisplyaTermPath { set; get; }
        [Column(TypeName = "int"), DefaultValue(0)]
        public int ApplyExistType { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsRemoved { set; get; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool EnableRelatedRecords { set; get; }

        [Column("DocLevelEnableClassification", TypeName = "int")]
        public int EnableRecordManagement { get; set; }

        [Column(TypeName = "bit")]
        public bool IncludeDeclaredRecords { get; set; }

        [Column(TypeName = "bit")]
        public bool? ColumnRequired { set; get; }
        [Column(TypeName = "bit")]
        public bool? ColumnHidden { set; get; }

        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string CollectionJobId1 { get; set; }
        /// <summary>
        ///  ！！！该属性要使用GroupLevel中的
        /// </summary>
        [Column(TypeName = "bit")]
        public bool? IsShowUniqueId { set; get; }
        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string DisposalJobId1 { get; set; }
        //[Column(TypeName = "nvarchar(max)")]
        //public string IdPath { get; set; }
        //[Column(TypeName = "bit")]
        public bool IsRunning { get; set; }

        [Column(TypeName = "bit")]
        public bool IsNewEdited { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string SharePointSettingJobId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string AutoClassificationRules { get; set; }

        [Column(TypeName = "int"), DefaultValue(0)]
        public int DeployTermMethod { get; set; }

        [Column(TypeName = "int"), DefaultValue(1)]
        public int AutoJobOption { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool RunAutoFullJob { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsSyncData { set; get; }

        [Column(TypeName = "int")]
        public ApprovalType ApprovalType { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string WorkflowReferenceId { get; set; }

        [Column(TypeName = "bit")]
        public bool? ApplyTermIncludeFolder { set; get; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsKeepSharePointDefaultValue { set; get; }

        [Column(TypeName = "int")]
        public ArtificialIntelligenceTermUseType AITermUseType { set; get; }

        [Column(TypeName = "int")]
        public ApprovalType AIApprovalType { get; set; }

        [Column(TypeName = "bit")]
        public bool AISendEMail { get; set; }

        [Column(TypeName = "bit")]
        public bool AIThenIsDefaultTermMethod { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid AIThenDefaultTermId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string AIThenDefaultTermName { get; set; }



        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool SetTermForEmptyDefaultValue { set; get; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool AlwaysScanAllExistDocuments { set; get; }
    }
}

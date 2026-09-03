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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;

namespace AvePoint.RA.DB.Model
{
    public class RMGoogleSetting : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid ScopeId { get; set; }

        [Column(TypeName = "nvarchar")]
        [Index]
        public string ObjectId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string FullPath { get; set; }

        [Column(TypeName = "nvarchar")]
        public string LabelId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string LabelName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DefaultLabelId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DefaultLabelName { get; set; }
        
        [Column(TypeName = "uniqueidentifier")]
        public Guid LabelStoreId { set; get; }
        
        [Column(TypeName = "uniqueidentifier")]
        public Guid LabelSetId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string LabelSetName { get; set; }

        [Column(TypeName = "bigint")]
        public long SettingTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string NodeInfo { get; set; }

        [Column(TypeName = "bit")]
        public bool NeedCheckDefaultValue { get; set; }

        [Column(TypeName = "int"), DefaultValue(0)]
        public int ApplyExistType { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsActive { get; set; }

        [Column(TypeName = "int"), DefaultValue(0)]
        public int DeployLabelMethod { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string AutoClassificationRules { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool RunAutoFullJob { get; set; }

        [Column(TypeName = "int"), DefaultValue(1)]
        public int AutoJobOption { get; set; }
        
        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsSyncData{ set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ContainerId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DriveId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid FolderId { get; set; }
        
        [Column(TypeName = "bit")]
        public bool IsNewEdited { get; set; }

        [Column(TypeName = "bit")]
        public bool IsEnableClassification { get; set; }

        [Column(TypeName = "int")]
        public int EnableRecordManagement { get; set; }

        [Column(TypeName = "bit")]
        public bool EnableSyncData { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }
        
        [Column(TypeName = "bigint")]
        public long UpdateDate { get; set; }
        
        [Column(TypeName = "int")]
        public ApprovalType ApprovalType { get; set; }
        
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string WorkflowReferenceId { get; set; }
        
        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsNullClassificationSetting { get; set; }
        
        [Column(TypeName = "bit")]
        public bool? IsShowUniqueId { set; get; }
        
        [Column(TypeName = "bit")]
        public bool IsFailedConfigClassification { get; set; }

        [Column(TypeName = "bit")]
        public bool IsFailedConfigMetaDataColumn { get; set; }

        #region Ai Term

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

        #endregion

        public RMGoogleSetting CopyProperties(RMGoogleTreeNode node, Guid containerId, Guid driveId)
        {
            this.DefaultLabelId = node.DefaultLabelId.ToString();
            this.DefaultLabelName = node.DefaultLabelName;
            this.FullPath = node.FullPath;
            this.ScopeId = new Guid(node.Id);
            this.LabelId = node.LabelId.ToString();
            this.LabelName = node.LabelName;
            this.LabelSetId = node.LabelSetId;
            this.LabelSetName = node.LabelSetName;
            this.LabelStoreId = node.LabelStoreId;
            this.EnableRecordManagement = node.EnableRecordManagement;
            this.IsFailedConfigMetaDataColumn = node.isFailedConfigMetaDataColumn;
            this.IsFailedConfigClassification = node.isFailedConfigClassification;
            this.IsSyncData = node.IsSyncData;
            this.DriveId = driveId;
            this.ContainerId = containerId;
            this.SettingTime = 0;
            this.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(node);
            this.NeedCheckDefaultValue = node.NeedCheckDefaultValue;
            this.ApplyExistType = node.ApplyExistType;
            this.DeployLabelMethod = (int)node.DeployLabelMethod;
            this.AutoClassificationRules = node.AutoClassificationRules == null
                ? null
                : SerializerHelper.SerializeByDataContractSerializer(node.AutoClassificationRules);
            this.RunAutoFullJob = node.RunAutoFullJob;
            this.AutoJobOption = (int)node.AutoJobOption;
            this.ApprovalType = (ApprovalType)node.ApprovalType;
            this.WorkflowReferenceId = node.WorkflowReferenceId;
            this.IsShowUniqueId = node.IsShowUniqueId;
            this.IsNullClassificationSetting = node.IsNullClassificationSetting;
            this.ObjectId = node.ObjectId;

            this.AITermUseType = node.AITermUseType;
            this.AIApprovalType = (ApprovalType)node.AIApprovalType;
            this.AISendEMail = node.AISendEMail;
            this.AIThenIsDefaultTermMethod = node.AIThenIsDefaultTermMethod;
            this.AIThenDefaultTermId = node.AIThenDefaultTermId;
            this.AIThenDefaultTermName = node.AIThenDefaultTermName;

            return this;
        }

        public string UpdateLabelNameInRules(string autoClassificationRules, string uniqueLabelId, string newName)
        {
            var rules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(autoClassificationRules);

            foreach (var rule in rules)
            {
                if (rule.TermId.EqualsIgnoreCase(uniqueLabelId))
                {
                    rule.TermName = newName;
                }
            }
            return SerializerHelper.SerializeByDataContractSerializer(rules);
        }
    }
}
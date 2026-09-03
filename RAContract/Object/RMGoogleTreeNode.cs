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
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object.Base;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Object;

[DataContract(IsReference = true)]
[JsonObject]
public class RMGoogleTreeNode : RMBaseTreeNode<RMGoogleTreeNode>
{
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public new RMGoogleTreeNode Parent { set => base.Parent = value;
        get => base.Parent;
    }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public new List<RMGoogleTreeNode> Children { set => base.Children = value;
        get => base.Children;
    }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public Guid LabelStoreId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public Guid LabelSetId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public Guid LabelId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public Guid DefaultLabelId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string DefaultLabelName { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string LabelSetName { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string LabelName { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public DeployLabelMethod DeployLabelMethod { set; get; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public int ApprovalType { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool isFailedConfigClassification { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool isFailedConfigMetaDataColumn { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool NeedCheckDefaultValue { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public int ApplyExistType { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsCustomSetting { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public List<ToUserInfo> RecordOwner { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public ScheduleInfo DisposeScheduleInfo { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public List<ClassificationRule> AutoClassificationRules { set; get; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool RunAutoFullJob { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public AutoJobOption AutoJobOption { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string DescriptionOfContainer { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool isEnableClassification { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public int EnableRecordManagement { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsSyncData { set; get; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool SkipRemoveContentAndDestroyAction { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string WorkflowReferenceId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string WorkflowReferenceName { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string ObjectId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string ContainerId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string DriveId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public string GoogleTenantId { get; set; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsNullClassificationSetting { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public List<RMSimpleRule> Rules { get; set; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsShowUniqueId { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public Guid TermGroupId { set; get; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool HasContainerSetting { set; get; }

    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public PredictionModeType PredictionModeType { set; get; }
    
    [DataMember(EmitDefaultValue = false)]
    [JsonProperty]
    public bool IsNodeProcessFromGControl { get; set; }


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
    public RMGoogleTreeNode Clone()
    {
        return this.MemberwiseClone() as RMGoogleTreeNode;
    }

    public RMGoogleTreeNode CopyProperties(RMSampleGoogleTreeNode sNode)
    {
        this.IconStatus = IconStatus.NoSet;
        this.Id = sNode.Id;
        this.Name = sNode.Name;
        this.DisplayName = sNode.DisplayName;
        this.Title = sNode.Title;
        this.FullPath = sNode.FullPath;
        this.Level = sNode.Level;
        this.NodeType = sNode.NodeType;
        this.Expanded = sNode.Expanded;
        this.ChildrenCount = sNode.ChildrenCount;
        this.CheckNumber = sNode.CheckNumber;
        this.Hidden = sNode.Hidden;
        this.ObjectId = sNode.ObjectId;
        this.DriveId = sNode.NodeId;
        this.ContainerId = sNode.ContainerId;
        this.GoogleTenantId = sNode.GoogleTenantId;
        this.ParentId = sNode.ParentId;
        return this;
    }

}

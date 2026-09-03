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
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    [DataContract]
    public class WorkflowDefinitionDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public Guid ReferenceId { get; set; }
        [DataMember]
        public Guid OperationUniqueId { get; set; }
        [DataMember]
        public Guid UpgradedVersionId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public RMWorkflowType Type { get; set; }
        [DataMember]
        public int LevelCount { get; set; }
        [DataMember]
        public string ContentStr { get; set; }
        [DataMember]
        public string XamlStr { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [DataMember]
        public string HashCode { get; set; }
        [DataMember]
        public DateTime LastUpdatedTime { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public RMWorkflowContentDto Content { get; set; }
        [DataMember]
        public bool UpgradeVersion { get; set; }
    }

    public class WorkflowDefinitionViewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedOnStr { get; set; }
        public List<string> UserDisplayNames { get; set; }
        public int LevelCount { get; set; }

        public RMWorkflowContentDto StepInfo { get; set; }
    }
    public class NewWorkflowDefinitionViewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedOnStr { get; set; }
        public List<string> UserDisplayNames { get; set; }

        public string ContentStr { get; set; }
        public int LevelCount { get; set; }
    }

    public class WorkflowSimpleDto {
        public Guid ReferenceId { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
    [DataContract]
    public class RMWorkflowContentDto
    {
        [DataMember]
        public List<RMWorkflowStepNode> WorkflowNodes { get; set; }
        [DataMember]
        public double ZoomNum { get; set; }
    }
    [DataContract]
    public class ProcessQueryDto
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
    }
    [DataContract]
    public class ReviewerUser
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public RMActiveDirectoryObjectType InviteType { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public int RMUserId { get; set; }
        [DataMember]
        public string SurName { get; set; }
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public string TenantId { get; set; }
    }

    public class QueryProcessesResultDto
    {
        public int TotalCount { get; set; }

        public List<NewWorkflowDefinitionViewDto> ResultList { get; set; }
    }
    [DataContract]
    public class RMWorkflowStepNode
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public WorkflowNodeStatus Status { get; set; }
        [DataMember]
        public WorkflowNodeType NodeType { get; set; }
        [DataMember]
        public WorkflowReviewerType ReviewerType { get; set; }
        [DataMember]
        public int Position_X { get; set; }
        [DataMember]
        public int Position_Y { get; set; }
        [DataMember]
        public Guid ParentId { get; set; }
        [DataMember]
        public List<Guid> ChildrenIds { get; set; } = new List<Guid>();
        [DataMember]
        public List<ReviewerUser> Reviewers { get; set; } = new List<ReviewerUser>();
        [DataMember]
        public RMWorkflowStepUsedEmailTemplateMode UsedEmailTemplateMode { get; set; }
        [DataMember]
        public Guid UsedEmailTemplateId { get; set; }
        [DataMember]
        public List<CustomIntervalSetting> CustomIntervalSetting { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public bool IsAssignSiteOwnersChecked { get; set; }
    }

    public class CustomIntervalSetting
    {
        public int Interval { get; set; }

        public string UsedEmailTemplateId { get; set; }
    }


    public enum RMWorkflowStepUsedEmailTemplateMode
    {
        Default = 0,
        Specify = 1,
        Custom = 2,
    }
    [DataContract]
    public enum WorkflowReviewerType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        RecordUsers = 0,
        [EnumMember]
        SiteOwners = 1,
        [EnumMember]
        SharePointGroup = 2,
        [EnumMember]
        InformationOwner = 3
    }
    [DataContract]
    public enum WorkflowNodeStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Approve = 1,
        [EnumMember]
        Reject = 2,
        [EnumMember]
        Delay = 3,
        [EnumMember]
        ApproveOrReject = 4
    }
    [DataContract]
    public enum WorkflowNodeType
    {
        [EnumMember]
        Start = 0,
        [EnumMember]
        BeginDisposalReview = 1,
        [EnumMember]
        DisposalReview = 2,
        [EnumMember]
        Destroy = 3,
        [EnumMember]
        NotDestroy = 4,
        [EnumMember]
        Delay = 5,
        [EnumMember]
        End = 6
    }
}

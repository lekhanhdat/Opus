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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.RA.Contract.Object.Base;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    /// <summary>
    /// Exchange online tree node for Backend 
    /// </summary>
    [DataContract(IsReference = true)]
    [JsonObject]
    public class RMEXOTreeNode : RMBaseTreeNode<RMEXOTreeNode>
    {
        //[DataMember(EmitDefaultValue = false)]
        //[JsonProperty]
        //public Guid SettingScopeId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string O365TenantId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string GroupName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid GroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid MailBoxId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid FolderId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public MailboxType MailboxType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string InternalFolderPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string SiteCollectionUrl { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Sender { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long SendDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DisplayTo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Email { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string Category { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool HasAttachment { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int OffSet { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int SubFolderCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMEXOTreeNode Parent { set { base.Parent = value; } get { return base.Parent; } }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMEXOTreeNode> Children { set { base.Children = value; } get { return base.Children; } }

        //[DataMember(EmitDefaultValue = false)]
        //[JsonProperty]
        //public string ProfileId { get; set; }

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

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedCheckDefaultValue { get; set; }

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

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EMailToRecordOwner { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo DisposeScheduleInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo CollectionScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedLoadSchedule { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ClassificationRule> AutoClassificationRules { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public DeployTermMethod DeployTermMethod { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool RunAutoFullJob { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public AutoJobOption AutoJobOption { get; set; }

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
        public Guid TermIdOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermNameOfContainer { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool isEnableClassification { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo ScheduleInfo { get; set; }

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
        public string TermScopeFullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermFullPath { get; set; }

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
        public bool IsNullClassificationSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMSimpleRule> Rules { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsCustomTermSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsProcessApprovalDatasOnly { get; set; }

        /// <summary>
        /// 浅拷贝
        /// </summary>
        /// <returns></returns>
        public RMEXOTreeNode Clone()
        {
            return this.MemberwiseClone() as RMEXOTreeNode;
        }
    }
}

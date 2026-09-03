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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    [JsonObject]
    public class RMPRTreeNode
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid UniqueId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int NodeType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsTopLevelSetting { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public Guid TopLevelSettingUniqueId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ChildrenCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ParentId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<string> ChildrenIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public IconStatus IconStatus { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public RMPRTreeNode Parent { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMPRTreeNode> Children { set; get; }

        #region for physical column settings

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ColumnName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ColumnRequired { get; set; }

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
        public string TermSetName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermName { get; set; }

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
        public bool NeedCheckDefaultValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsEnableUniqueIDSetting { set; get; }

        #endregion


        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool EMailToRecordOwner { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public DeployTermMethod DeployTermMethod { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApplyExistType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsCustomSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo DisposeScheduleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string DefaultTermFullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermScopeFullPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int ApprovalType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string WorkflowReferenceId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string WorkflowReferenceName { get; set; }

    }



    [DataContract]
    public class RMPRSaveTermDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid UniqueId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsTopLevelSetting { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermSetId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid DefaultTermId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TermSetName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DefaultTermName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DeployTermMethod DeployTermMethod { set; get; }
    }

    [DataContract]
    public class RMPRSaveRecordOwnerDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid UniqueId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsTopLevelSetting { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<ToUserInfo> RecordOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool EMailToRecordOwner { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public int ApprovalType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string WorkflowReferenceId { get; set; }
    }
}

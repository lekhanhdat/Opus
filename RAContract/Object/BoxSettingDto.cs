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
using AvePoint.RA.Contract.Box;
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
    [DataContract]
    public class BoxSettingDto
    {
        [DataMember]
        public Guid TermSetId { get; set; }
        [DataMember]
        public Guid TermId { get; set; }
        [DataMember]
        public Guid DefaultTermId { get; set; }
        [DataMember]
        public string DefaultTermName { get; set; }
        [DataMember]
        public string DefaultTermFullPath { get; set; }
        [DataMember]
        public string TermSetName { get; set; }
        [DataMember]
        public string TermName { get; set; }
        [DataMember]
        public bool IsTermRemoved { get; set; }
        [DataMember]
        public bool IsDefaultTermRemoved { get; set; }
        [DataMember]
        public bool IsTermDeprecated { set; get; }
        [DataMember]
        public bool IsDefaultTermDeprecated { set; get; }
        [DataMember]
        public bool NeedCheckDefaultValue { get; set; }
        [DataMember]
        public bool IsCustomSetting { get; set; }
        [DataMember]
        public bool IsActive { set; get; }
        [DataMember]
        public int ApplyExistType { get; set; }
        [DataMember]
        public List<ClassificationRule> AutoClassificationRules { set; get; }
        [DataMember]
        public DeployTermMethod DeployTermMethod { set; get; }
        [DataMember]
        public bool RunAutoFullJob { get; set; }
        [DataMember]
        public AutoJobOption AutoJobOption { get; set; }
        [DataMember]
        public string TermScopeFullPath { get; set; }
        [DataMember]
        public BoxTreeNode SelectedNode { get; set; }
        [DataMember]
        public string ScopeId { get; set; }

        [DataMember]
        public bool EMailToRecordOwner { set; get; }
        [DataMember]
        public int ApprovalType { get; set; }
        [DataMember]
        public string WorkflowReferenceId { get; set; }
        [DataMember]
        public List<ToUserInfo> RecordOwner { get; set; }
        [DataMember]
        public string WorkflowReferenceName { get; set; }
        [DataMember]
        public bool IsEnableSettingManualApproval { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public ScheduleInfo DisposeScheduleInfo { get; set; }

        public BoxSettingDto Clone()
        {
            return this.MemberwiseClone() as BoxSettingDto;
        }
    }
}

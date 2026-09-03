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
using System.Runtime.Serialization;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.TaxonomyModel;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    public class GoogleSettingDto
    {
        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string ScopeId { get; set; }

        [DataMember]
        public string LabelId { get; set; }

        [DataMember]
        public string LabelName { get; set; }

        [DataMember]
        public string DefaultLabelId { get; set; }

        [DataMember]
        public string DefaultLabelName { get; set; }

        [DataMember]
        public RMGoogleTreeNode NodeInfo { get; set; }

        [DataMember]
        public List<ClassificationRule> AutoClassificationRules { get; set; }

        [DataMember]
        public DeployLabelMethod DeployLabelMethod { set; get; }

        [DataMember]
        public AutoJobOption AutoJobOption { get; set; }

        [DataMember]
        public bool RunAutoFullJob { get; set; }
        [DataMember]
        public int ApplyExistType { get; set; }

        [DataMember]
        public string ContainerId { get; set; }

        [DataMember]
        public string DriveId { get; set; }

        [DataMember]
        public string FolderId { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public bool NeedCheckDefaultValue { get; set; }

        [DataMember]
        public int EnableRecordManagement { get; set; }

        [DataMember]
        public bool EnableSyncData { get; set; }
        
        [DataMember]
        public List<ToUserInfo> RecordOwner { get; set; }
        
        [DataMember]
        public bool EmailToRecordOwner { set; get; }
        
        [DataMember]
        public int ApprovalType { get; set; }

        [DataMember]
        public string WorkflowReferenceId { get; set; }

        [DataMember]
        public ArtificialIntelligenceTermUseType AITermUseType { get; set; }

        [DataMember]
        public string AIThenDefaultTermId { set; get; }

        [DataMember]
        public string AIThenDefaultTermName { get; set; }

        [DataMember]
        public bool AIThenIsDefaultTermMethod { set; get; }

        [DataMember]
        public int AIApprovalType { get; set; }


        public GoogleSettingDto Clone()
        {
            return this.MemberwiseClone() as GoogleSettingDto;
        }
    }

    [DataContract]
    public enum DeployLabelMethod
    {
        [EnumMember]
        UseManualClassification = 0,
        [EnumMember]
        UseAutoClassification = 1,
        [EnumMember]
        UseIntelligenceClassification = 2,
    }
}
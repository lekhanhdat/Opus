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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApprovalCommon.Model
{
    public class ManualApprovalRuleModel
    {

        public bool IsHasRule { get; set; }

        public SourceFlag Flag { get; set; }

        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public string RuleCriterias { get; set; }

        public string RuleDisposalClass { get; set; }

        public bool EnableManualApproval { get; set; }

        public AvePoint.RA.DB.Model.ApprovalType ManualApprovalType
        {
            get =>
                !string.IsNullOrEmpty(WorkflowId) ?
                AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess :
                AvePoint.RA.DB.Model.ApprovalType.RecordOwners;
        }

        public bool IsSendEmailToOwner { get; set; }

        public List<UserInfo> Owners { get; set; } = new List<UserInfo>();
        /// <summary>
        /// Workflow Reference Id
        /// </summary>
        public string WorkflowId { get; set; }

        public RetentionInfo RetentionInfo { set; get; }
        
        public bool IsGControlWorkflow { set; get; }

        public AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption RelatedRecordOption { get; set; }

        public ManualApprovalRuleModel DeepCopy()
        {
            return new ManualApprovalRuleModel
            {
                IsHasRule = IsHasRule,
                Flag = Flag,
                RuleId = RuleId,
                RuleName = RuleName,
                RuleCriterias = RuleCriterias,
                RuleDisposalClass = RuleDisposalClass,
                IsSendEmailToOwner = IsSendEmailToOwner,
                Owners = Owners == null ? new List<UserInfo>() : new List<UserInfo>(Owners),
                WorkflowId = WorkflowId,
                RetentionInfo = RetentionInfo,
                RelatedRecordOption = RelatedRecordOption,
                IsGControlWorkflow = IsGControlWorkflow,
                EnableManualApproval = EnableManualApproval
            };
        }
             
    }
}

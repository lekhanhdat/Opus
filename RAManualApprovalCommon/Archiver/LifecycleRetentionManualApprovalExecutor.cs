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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using RAManualApprovalCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApprovalCommon.Archiver
{
    public class LifecycleRetentionManualApprovalExecutor : ArchiverManualAction
    {
        public LifecycleRetentionManualApprovalExecutor(string jobId, Guid containerId) : base(jobId, containerId)
        {
        }

        protected override SourceFlag ContentSource => SourceFlag.LifecycleRetention;

        protected override ManualApprovalSettingModel GetSettingInfo(Record record)
        {
            if (!ManualApprovalRuleInfoManager.TryGet((SourceFlag)record.SourceFlag, record.RuleId.ToString(), out var ruleInfo))
            {
                s_logger.Error($"Cannot find RuleId {record.RuleId} SourceFlag {record.SourceFlag}");
                throw new Exception("RM_RDM_Rule_RuleIsDeleted");
            }

            var model = new ManualApprovalSettingModel();
            if (ruleInfo.RetentionInfo != null)
            {
                model.IsSendEmialToOwner = ruleInfo.RetentionInfo.IsSendEamilToOwner;
                model.ManualApprovalType = ruleInfo.RetentionInfo.ReviewType == AvePoint.GCommon.Contract.StorageOptimization.Object.ReviewType.RecordOwner ? AvePoint.RA.DB.Model.ApprovalType.RecordOwners : AvePoint.RA.DB.Model.ApprovalType.ApprovalProcess;
                model.Owners = ruleInfo.RetentionInfo.UserInfos;
                model.WorkflowId = ruleInfo.RetentionInfo.WorkflowId;
            }
            else
            {
                s_logger.Info($"Retention setting is canceled. RuleId {record.RuleId} SourceFlag {record.SourceFlag}");
            }
            return model;

        }

    }
}

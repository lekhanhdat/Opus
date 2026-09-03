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
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;

namespace AvePoint.RA.Service.Services.MachineLearningManualApproval.AuditHandler
{
    public class MLManualApprovalAfterAuditHandler : IAsyncAuditAfterHandler
    {
        public Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args, object returnValue)
        {
            if (action == AuditAction.MLChangeTermJob || action == AuditAction.MLApproveJob)
            {
                ChangeTerm(auditInfo, args, returnValue);
            }
            else
            {
                var actionResult = returnValue as ManualApprovalActionResult;
                auditInfo.Status = (int)(actionResult.CompletedStatus == ActionCompletedStatus.Failed ? AuditStatus.Failed : AuditStatus.Successful);

                var effectItems = actionResult.EffectItems;
                var effectFullPaths = effectItems.Select(item => item.EffectItemFullPath);
                auditInfo.Object = string.Join("; ", effectFullPaths) + "; ";

                switch (action)
                {
                    case AuditAction.MLReassign:
                        Reassign(auditInfo, args);
                        break;
                    default:
                        throw new Exception("The action is not support.");
                }
            }
            return System.Threading.Tasks.Task.FromResult(auditInfo);
        }

        private static void Reassign(RMAuditInfo info, object[] args)
        {
            var definition = args[0] as ManualAprovalEscalateDefinition;
            var emails = definition.ToUsers.Select(item => item.UserPrincipalName);
            var actionAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_ReassignToUsers",
                NewValue = string.Join("; ", emails) + "; "
            };
            info.ModifyContent.Add(actionAudit);

            var sendAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_IsSendEmail",
                NewValue = definition.NeedSendEmail ? "RM_JS_Common_Yes" : "RM_JS_Common_No"
            };
            info.ModifyContent.Add(sendAudit);

            var commentAudit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_Comment",
                NewValue = definition.Comment
            };
            info.ModifyContent.Add(commentAudit);
        }

        private static void ChangeTerm(RMAuditInfo info, object[] args, object returnValue)
        {
            JobType jobType = (JobType)args[1];
            info.Object = returnValue as string;
        }
    }
}

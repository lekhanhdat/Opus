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
using AvePoint.RA.Common.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.ManualApproval.AuditHandler
{
    public class ManualApprovalAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(ManualApprovalAfterAuditHandler));
        public IRMManualApproveDao ManualApproveDao => PlatformWindsorManager.GetService<IRMManualApproveDao>();
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            if ((AuditAction)action == AuditAction.RunManualApproveOrReject || (AuditAction)action == AuditAction.RunManualApproval)
            {
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Object = returnValue as string;
                if ((int)args[0] == (int)JobRunBy.Schedule)
                {
                    auditInfo.UserName = "RM_TS_RunSchedule";
                }
                return auditInfo;
            }
            else if ((AuditAction)action == AuditAction.MarkToApproved || (AuditAction)action == AuditAction.MarkToRejected || info.Action == AuditAction.MarkToExtend)
            {
                RAReturnMessage returnMessage = returnValue as RAReturnMessage;
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Status = returnMessage.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                auditInfo.Object = info != null ? info.Object : null;
                return auditInfo;
            }
            else if ((AuditAction)action == AuditAction.EscalateTo || (AuditAction)action == AuditAction.ReassignTo)
            {
                auditInfo.ModifyContent = new List<AuditItem>();
                AuditItem auditItem = new AuditItem();
                auditItem.TargetSetting = "RM_JS_MA_EscalateToUsers";
                if ((AuditAction)action == AuditAction.ReassignTo)
                {
                    auditItem.TargetSetting = "RM_JS_MA_ReassignToUsers";
                }
                EscalateModel setting = args[0] as EscalateModel;
                List<ToUserInfo> userInfos = setting.EscalateTos;

                auditItem.NewValue = string.Join(";", userInfos.Select(u => u.UserPrincipalName));
                auditInfo.ModifyContent.Add(auditItem);

                AuditItem auditItem1 = new AuditItem();
                auditItem1.TargetSetting = "RM_JS_MA_IsSendEmail";
                auditItem1.NewValue = setting.isSendMail ? "RM_JS_Common_Yes" : "RM_JS_Common_No";
                auditInfo.ModifyContent.Add(auditItem1);

                AuditItem auditItem2 = new AuditItem();
                auditItem2.TargetSetting = "RM_JS_MA_Comment";
                auditItem2.NewValue = setting.Comment;
                auditInfo.ModifyContent.Add(auditItem2);

                var ids = setting.ids;
                List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
                auditInfo.Object = string.Join(";", items.Select(o => o.Url).ToList());
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Status = (int)AuditStatus.Successful;
                return auditInfo;
            }
            else if ((AuditAction)action == AuditAction.ExportHistory)
            {
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Object = args[1] + ".xlsx";
                auditInfo.Status = (int)AuditStatus.Successful;
                return auditInfo;
            }
            else if ((AuditAction)action == AuditAction.ChangeAction)
            {
                RAReturnMessage returnMessage = returnValue as RAReturnMessage;
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.Status = returnMessage.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                auditInfo.ModifyContent = info != null && info.ModifyContent != null ? info.ModifyContent : auditInfo.ModifyContent;
                auditInfo.Object = info?.Object;
                return auditInfo;
            }
            else
            {
                try
                {
                    auditInfo.Module = (AuditModule)model;
                    auditInfo.Category = (AuditCategory)category;
                    auditInfo.Action = (AuditAction)action;
                    auditInfo.Object = returnValue as string;
                    if (info != null && info.E != null)
                    {
                        auditInfo.Status = (int)AuditStatus.Failed;
                    }
                    else
                    {
                        auditInfo.Status = (int)AuditStatus.Successful;
                    }

                    if ((int)args[0] == (int)JobRunBy.Schedule)
                    {
                        auditInfo.UserName = "RM_TS_RunSchedule";
                    }
                    return auditInfo;
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
            }

            return info;
        }
    }
}

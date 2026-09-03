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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.Service.Services.SharePointSetting.AuditHandler
{
    public class DisposalActivityManagementAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(DisposalActivityManagementAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            var auditInfo = new RMAuditInfo();
            try
            {
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                auditInfo.ModifyContent = new List<AuditItem>();
                var runby = (JobRunBy)args[1];
                if (runby == JobRunBy.Schedule)
                {
                    auditInfo.UserName = "RM_TS_RunSchedule";
                }
                else
                {
                    auditInfo.UserName = TenantLocalValue.PartnerUser ?? TenantLocalValue.LogonUserEmail;
                }
                var msg = returnValue as RAReturnMessage;
                //sp exo od 旧逻辑  args里面是一个    新账号args里面是俩
                if (action == (int)AuditAction.RunDisposalJob || action == (int)AuditAction.RunOneDriveDisposalJob
                    || action == (int)AuditAction.RunEXODisposalJob || action == (int)AuditAction.RunPRDisposalJob
                    )
                {
                    auditInfo.Category = AuditCategory.SharePointSettings;
                    auditInfo.Module = AuditModule.BusinessClassificationManagement;
                    if (msg != null)
                    {
                        auditInfo.Object = msg.Extsion1 as string;
                    }
                    else
                    {
                        auditInfo.Object = returnValue.ToString();
                    }
                    if (args.Length > 2)
                    {
                        //新账号  pr走这个逻辑
                        string newResult = args[2].ToString().Contains("False") ? "False" : "True";
                        auditInfo.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = newResult });
                    }
                    else
                    {
                        //老账号
                        if (action == (int)AuditAction.RunEXODisposalJob)
                        {
                            var argresult = args[0] as RMEXOTreeNode;
                            string newResult = argresult.SkipRemoveContentAndDestroyAction ? "True" : "False";
                            auditInfo.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = newResult });
                        }
                        else 
                        {
                            var argresult = args[0] as RMSPTreeNode;
                            string newResult = argresult.SkipRemoveContentAndDestroyAction ? "True" : "False";
                            auditInfo.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = newResult });
                        }
                        
                        
                    }

                }
                if (action == (int)AuditAction.RunSPOnPremDisposalJob) 
                {
                    auditInfo.Category = AuditCategory.SharePointSettings;
                    auditInfo.Module = AuditModule.BusinessClassificationManagement;
                    if (msg != null)
                    {
                        auditInfo.Object = msg.Extsion1 as string;
                    }
                    else
                    {
                        auditInfo.Object = returnValue.ToString();
                    }
                }
                ArgumentCheck.NotNull(msg, nameof(msg));
                if (info != null && info.E != null || msg.MessageType == RAMessageType.Failed)
                {
                    auditInfo.Status = (int)AuditStatus.Failed;
                }
                else
                {
                    auditInfo.Status = (int)AuditStatus.Successful;
                    auditInfo.Object = msg.Extension;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return auditInfo;
        }
    }
}

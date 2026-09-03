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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Account.Security;

namespace AvePoint.RA.Service.Services.AccountManager.AuditHandler
{
    public class LogOutAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(LogOutAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            try
            {
                auditInfo.Module = (AuditModule)model;
                auditInfo.Category = (AuditCategory)category;
                auditInfo.Action = (AuditAction)action;
                RMIdentity userInfo = args[0] as RMIdentity;
                auditInfo.UserName = userInfo?.PartnerUser ?? userInfo?.Name;

                auditInfo.Object = userInfo?.PartnerUser ?? userInfo?.Name;
                if (info != null && info.E != null)
                {
                    auditInfo.Status = (int)AuditStatus.Failed;
                }
                else
                {
                    auditInfo.Status = (int)AuditStatus.Successful;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                auditInfo.Status = (int)AuditStatus.Failed;
            }
            return auditInfo;
        }
    }
}

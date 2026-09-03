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
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AuthenticationManager.AuditHandler
{
    public class AuthenticationAfterAuditHandler : IAfterAuditHandler
    {
        private AveLogger logger = AveLogger.GetInstance(typeof(AuthenticationAfterAuditHandler));
        public RMAuditInfo Collect(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            try
            {
                auditInfo.Module = info.Module;
                auditInfo.Category = info.Category;
                auditInfo.Action = info.Action;
                auditInfo.ModifyContent = info.ModifyContent;
                auditInfo.Object = info.Object;

                if (auditInfo.Action == AuditAction.AddADDomain)
                {
                    auditInfo.Status = (int)GetAddADDomainAuditStatus(info, returnValue);
                }
                else
                {
                    auditInfo.Status = (int)GetReturnBooleanFunctionAuditStatus(info, returnValue);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return auditInfo;
        }

        private AuditStatus GetReturnBooleanFunctionAuditStatus(RMAuditInfo info, object returnValue)
        {
            if (info != null && info.E != null)
            {
                return AuditStatus.Exception;
            }
            else
            {
                return (bool)returnValue ? AuditStatus.Successful : AuditStatus.Failed;
            }
        }

        private AuditStatus GetAddADDomainAuditStatus(RMAuditInfo info, object returnValue)
        {
            if (info != null && info.E != null)
            {
                return AuditStatus.Exception;
            }
            else
            {
                var domain = (RMDomainDto)returnValue;
                return domain == null ? AuditStatus.Failed : AuditStatus.Successful;
            }
        }
    }
}

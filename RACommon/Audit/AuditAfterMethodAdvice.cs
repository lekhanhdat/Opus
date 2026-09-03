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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common.ClientRequest;

namespace AvePoint.RA.Common.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public class AuditAfterMethodAdvice : IAfterMethodAdvice
    {
        private static RALogger logger = RALogger.GetInstance(typeof( AuditAfterMethodAdvice));

        public async Task AfterMethodInvokeAsync(RMAuditInfo auditInfo,object returnValue, System.Reflection.MethodInfo method, object target, object[] args , AuditAttribute attr)
        {
            try
            {
                AuditArg arg = new AuditArg
                {
                    Module = (int)attr.Module,
                    Category = (int)attr.Category,
                    Action = (int)attr.Action,
                    Args = args,
                    Target = target,
                    HandlerType = attr.AfterHandler,
                    IsHandled = attr.IsHandled,
                    ReturnValue = returnValue,
                    Method=method,
                    StartNewThread = attr.StartNewThread,
                    auditInfo = auditInfo,
                    UserName = WebUtil.LogOnUserName,
                    ClientIP = ClientRequestLocalValue.ClientIP
                };
                var worker = new AuditWorker(arg);
                await worker.WorkAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}

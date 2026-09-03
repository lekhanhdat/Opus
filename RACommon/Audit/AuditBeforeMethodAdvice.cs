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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    public class AuditBeforeMethodAdvice : IBeforeMethodAdvice
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AuditAfterMethodAdvice));

        public async Task<RMAuditInfo> BeforeMethodInvokeAsync(System.Reflection.MethodInfo method, object target, object[] args, AuditAttribute attr)
        {
            RMAuditInfo info = null; 
            try
            {
                //in fact we don't need to build this arg here,right? we can pass these values directly
                AuditArg arg = new AuditArg
                {
                    Module = (int)attr.Module,
                    Category = (int)attr.Category,
                    Action = (int)attr.Action,
                    Args = args,
                    HandlerType = attr.BeforeHandler,
                    IsHandled = attr.IsHandled,
                    Method = method,
                    StartNewThread = attr.StartNewThread,
                };
                IBeforeAuditHandler AuditHandler = (IBeforeAuditHandler)PlatformWindsorManager.GetService(arg.HandlerType.ToString(), arg.HandlerType);
                info = await AuditHandler.CollectAsync(arg.Module,arg.Category,arg.Action, arg.Args, target);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return info;
        }
    }
}

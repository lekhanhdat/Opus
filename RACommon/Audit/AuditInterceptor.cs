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
using AngleSharp.Media;
using AvePoint.RA.Common.Audit.Async;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.RMWeb.Audit;
using Castle.DynamicProxy;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit
{
    /// <summary>
    /// have reviewed by allen yin.
    /// </summary>
    public class AuditInterceptor : IAsyncInterceptor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AuditInterceptor));

        public IAfterMethodAdvice AfterMethodAdvice => new AuditAfterMethodAdvice();

        public IBeforeMethodAdvice BeforeMethodAdvice => new AuditBeforeMethodAdvice();

        private static bool TryGetMethodAuditAttribute(IInvocation invocation, out AuditAttribute auditAttribute)
        {
            auditAttribute = default;

            var auditAttributeList = invocation.MethodInvocationTarget.GetCustomAttributes(typeof(AuditAttribute), false);
            if (auditAttributeList.Any())
            {
                auditAttribute = auditAttributeList[0] as AuditAttribute;
                return true;
            }

            return false;
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            if(!TryGetMethodAuditAttribute(invocation, out var auditAttribute))
            {
                invocation.Proceed();
                return;
            }

            RMAuditInfo auditInfo = null;

            if (auditAttribute.BeforeHandler != null)
            {
                auditInfo = BeforeMethodAdvice.BeforeMethodInvokeAsync(
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute).GetAwaiter().GetResult();
            }

            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                auditInfo ??= new RMAuditInfo();
                auditInfo.E = e;
                Logger.Error(e.ToString());
            }

            if (auditAttribute.AfterHandler != null)
            {
                AfterMethodAdvice.AfterMethodInvokeAsync(
                    auditInfo, 
                    invocation.ReturnValue, 
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute);
            }

            if (auditInfo != null && auditInfo.E != null)
            {
                throw auditInfo.E;
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
			invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
		}

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            if (!TryGetMethodAuditAttribute(invocation, out var auditAttribute))
            {
                invocation.Proceed();
                await (Task)invocation.ReturnValue;
                return;
            }

            RMAuditInfo auditInfo = null;
            if (auditAttribute.BeforeHandler != null)
            {
                auditInfo = BeforeMethodAdvice.BeforeMethodInvokeAsync(
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute).Result;
            }

            try
            {

                invocation.Proceed();
                await (Task)invocation.ReturnValue;
            }
            catch (Exception e)
            {
                auditInfo ??= new RMAuditInfo();
                auditInfo.E = e;
                Logger.Error(e.ToString());
            }

            if (auditAttribute.AfterHandler != null)
            {
                _ = AfterMethodAdvice.AfterMethodInvokeAsync(
                    auditInfo, 
                    invocation.ReturnValue, 
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute);
            }

            if (auditInfo != null && auditInfo.E != null)
            {
                throw auditInfo.E;
            }
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            if (!TryGetMethodAuditAttribute(invocation, out var auditAttribute))
            {
                invocation.Proceed();
                return await (Task<TResult>)invocation.ReturnValue;

            }

            RMAuditInfo auditInfo = null;

            if (auditAttribute.BeforeHandler != null)
            {
                auditInfo = BeforeMethodAdvice.BeforeMethodInvokeAsync(
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute).Result;
            }

            TResult result = default;
            try
            {
                invocation.Proceed();
                result = await (Task<TResult>)invocation.ReturnValue;
            }
            catch (Exception e)
            {
                auditInfo ??= new RMAuditInfo();
                auditInfo.E = e;
                Logger.Error(e.ToString());
            }

            if (auditAttribute.AfterHandler != null)
            {
                _ = AfterMethodAdvice.AfterMethodInvokeAsync(
                    auditInfo, 
                    result, 
                    invocation.MethodInvocationTarget, 
                    invocation.InvocationTarget, 
                    invocation.Arguments,
                    auditAttribute);
            }

            if (auditInfo != null && auditInfo.E != null)
            {
                throw auditInfo.E;
            }

            return result;
        }
    }
}

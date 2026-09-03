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
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit.Async
{
    public class AuditAsyncInterceptor : AsyncInterceptorBase
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AuditAsyncInterceptAdapter));

        private IAuditCommonService AuditCommentService => PlatformWindsorManager.GetService<IAuditCommonService>();

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            try
            {
                if (!TryGetMethodAuditAttribute(invocation, out var auditAttribute))
                {
                    await proceed(invocation, proceedInfo).ConfigureAwait(false);
                    return;
                }

                RMAuditInfo auditInfo = null;
                if (auditAttribute.IAsyncBeforeHandler != null)
                {
                    auditInfo = await AsyncInvokeBeforeHandlerAsync(invocation, auditAttribute);
                }

                await proceed(invocation, proceedInfo).ConfigureAwait(false);

                if (auditAttribute.IAsyncAfterHandler != null)
                {
                    auditInfo = await AsyncInvokeAfterHandlerAsync(invocation, auditAttribute, auditInfo, null);
                }

                if (auditInfo == null)
                {
                    Logger.Error($"An error occurred while add audit. [{invocation.InvocationTarget}]");
                    return;
                }

                Func<AsyncAuditAttribute, RMAuditInfo, MethodInfo,Task> func = AsyncAddAuditAsync;
                await func(auditAttribute, auditInfo, invocation.MethodInvocationTarget);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while intercept add audit. Error: {e}");
                throw;
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            try
            {
                if (!TryGetMethodAuditAttribute(invocation, out var auditAttribute))
                {
                    return await proceed(invocation, proceedInfo).ConfigureAwait(false);
                }

                RMAuditInfo auditInfo = null;
                if (auditAttribute.IAsyncBeforeHandler != null)
                {
                    auditInfo = await AsyncInvokeBeforeHandlerAsync(invocation, auditAttribute);
                }

                var returnValue = await proceed(invocation, proceedInfo).ConfigureAwait(false);

                if (auditAttribute.IAsyncAfterHandler != null)
                {
                    auditInfo = await AsyncInvokeAfterHandlerAsync(invocation, auditAttribute, auditInfo, returnValue);
                }

                if (auditInfo == null)
                {
                    Logger.Error($"An error occurred while add audit. [{invocation.InvocationTarget}]");
                    return returnValue;
                }

                Func<AsyncAuditAttribute, RMAuditInfo, MethodInfo, Task> func = AsyncAddAuditAsync;
                await func(auditAttribute, auditInfo, invocation.MethodInvocationTarget);

                return returnValue;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while intercept add audit. Error: {e}");
                throw;
            }
        }

        private static async Task<RMAuditInfo> AsyncInvokeBeforeHandlerAsync(IInvocation invocation, AsyncAuditAttribute auditAttribute)
        {
            try
            {
                var res = new RMAuditInfo
                {
                    ModifyContent = new List<AuditItem>()
                };
                var handler = (IAsyncAuditBeforeHandler)PlatformWindsorManager.GetService(auditAttribute.IAsyncBeforeHandler.ToString(), auditAttribute.IAsyncBeforeHandler);

                res = await handler.CollectAsync(res, auditAttribute.Module, auditAttribute.Action, auditAttribute.Category, invocation.Arguments);
                return res;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while invoke audit before handler. Error: {e}");
                return null;
            }
        }

        private static async Task<RMAuditInfo> AsyncInvokeAfterHandlerAsync(IInvocation invocation, AsyncAuditAttribute auditAttribute, RMAuditInfo auditInfo, object returnValue)
        {
            try
            {
                auditInfo ??= new RMAuditInfo
                {
                    ModifyContent = new List<AuditItem>()
                };
                var handler = (IAsyncAuditAfterHandler)PlatformWindsorManager.GetService(auditAttribute.IAsyncAfterHandler.ToString(), auditAttribute.IAsyncAfterHandler);

                return await handler.CollectAsync(auditInfo, auditAttribute.Module, auditAttribute.Action, auditAttribute.Category, invocation.Arguments, returnValue);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while invoke audit after handler. Error: {e}");
                return null;
            }
        }

        private async Task AsyncAddAuditAsync(AsyncAuditAttribute auditAttribute, RMAuditInfo auditInfo, MethodInfo methodInfo)
        {
            try
            {
                auditInfo.Module = auditAttribute.Module;
                if(auditInfo.Category == AuditCategory.Unknown) auditInfo.Category = auditAttribute.Category;
                if (auditInfo.Action == AuditAction.Unknown) auditInfo.Action = auditAttribute.Action;
                auditInfo.ExecuteOn = DateTime.UtcNow;
                auditInfo.Method = methodInfo.DeclaringType + "." + methodInfo.Name;
                auditInfo.Role ??= "Administrator";
                auditInfo.UserName = TenantLocalValue.PartnerUser ?? TenantLocalValue.LogonUserEmail;
                auditInfo.ClientIP = ClientRequestLocalValue.ClientIP;
                await AuditCommentService.Add(auditInfo);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while add audit. Error: {e}");
            }
        }

        private static bool TryGetMethodAuditAttribute(IInvocation invocation, out AsyncAuditAttribute auditAttribute)
        {
            auditAttribute = default;

            var auditAttributeList = invocation.MethodInvocationTarget.GetCustomAttributes(typeof(AsyncAuditAttribute), false);
            if(auditAttributeList.Any())
            {
                auditAttribute = auditAttributeList[0] as AsyncAuditAttribute;
                return true;
            }

            return false;
        }
    }
}

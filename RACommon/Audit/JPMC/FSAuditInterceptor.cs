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
using AvePoint.RA.Contract.Audit.JPMC;
using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Audit.JPMC
{
    public class FSAuditInterceptor : AsyncInterceptorBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(FSAuditInterceptor));
        private static readonly ConcurrentDictionary<Type, FSAuditHandlerBase> _handlerCache = new();
        private static readonly ConcurrentDictionary<MethodInfo, FSAuditAttribute> _attributeCache = new();
        private readonly FSAuditDispatcher _dispatcher;
     
        public FSAuditInterceptor()
        {
            _dispatcher = FSAuditDispatcher.Default;
        }

        protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            if (!TryGetAttribute(invocation, out var attribute))
            {
                await proceed(invocation, proceedInfo).ConfigureAwait(false);
                return;
            }

            var handler = ResolveHandler(attribute.AuditHandler);
            var context = await ExecuteBeforeAsync(handler, attribute, invocation);

            try
            {
                await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.ErrorMessage = ex.Message;
                Logger.Error("Exception in audited method {0}: {1}", GetMethodName(invocation.MethodInvocationTarget), ex);
            }
            finally
            {
                await FinalizeAuditAsync(handler, attribute, invocation, context, returnValue: null);
            }
        }

        protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            if (!TryGetAttribute(invocation, out var attribute))
            {
                return await proceed(invocation, proceedInfo).ConfigureAwait(false);
            }

            var handler = ResolveHandler(attribute.AuditHandler);
            var context = await ExecuteBeforeAsync(handler, attribute, invocation);
            TResult result = default;

            try
            {
                result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                context.ErrorMessage = ex.Message;
                Logger.Error("Exception in audited method {0}: {1}", GetMethodName(invocation.MethodInvocationTarget), ex);
                throw;
            }
            finally
            {
                await FinalizeAuditAsync(handler, attribute, invocation, context, result);
            }
        }

        private static async Task<FSAuditContext> ExecuteBeforeAsync(FSAuditHandlerBase handler, FSAuditAttribute attr, IInvocation invocation)
        {
            var context = FSAuditContext.GetNewContext(attr.AuditType, attr.AuditLevel);
            if (handler == null) return context;
            try
            {
                context = await handler.CollectBeforeAsync(context, attr.AuditType, attr.AuditLevel, invocation.Arguments);
            }
            catch (Exception ex)
            {
                context.ErrorMessage = ex.Message;
                Logger.Error("Error in FSAudit before-handler for {0}, method {1}: {2}", attr.AuditType, GetMethodName(invocation.MethodInvocationTarget), ex);
                throw;
            }
            return context;
        }

        private async Task FinalizeAuditAsync(FSAuditHandlerBase handler, FSAuditAttribute attr, IInvocation invocation, FSAuditContext context, object returnValue)
        {
            try
            {
                if (handler != null)
                {
                    context = await handler.CollectAfterAsync(context, attr.AuditType, attr.AuditLevel, invocation.Arguments, returnValue);
                }

                FSAuditRecord auditRecord = FSAuditRecordBuilder.BuildWithValidation(context, returnValue);
                await _dispatcher.DispatchAsync(auditRecord);
            }
            catch (Exception ex)
            {
                Logger.Error("Error finalizing FSAudit for {0}, method {1}: {2}", attr.AuditType, GetMethodName(invocation.MethodInvocationTarget), ex);
            }
        }

        private static bool TryGetAttribute(IInvocation invocation, out FSAuditAttribute attribute)
        {
            attribute = _attributeCache.GetOrAdd(
                invocation.MethodInvocationTarget,
                method =>
                method.GetCustomAttributes(typeof(FSAuditAttribute), false).FirstOrDefault() as FSAuditAttribute);
            return attribute != null;
        }

        private static FSAuditHandlerBase ResolveHandler(Type handlerType)
        {
            if (handlerType == null) return null;

            if (!typeof(FSAuditHandlerBase).IsAssignableFrom(handlerType))
            {
                Logger.Error("Audit handler {0} must inherit from FSAuditHandlerBase.", handlerType.Name);
                return null;
            }

            return _handlerCache.GetOrAdd(handlerType, type =>
            {
                try
                {
                    return Activator.CreateInstance(type) as FSAuditHandlerBase;
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to instantiate audit handler {0}: {1}", type.Name, ex);
                    return null;
                }
            });
        }

        private static string GetMethodName(MethodInfo methodInfo)
        {
            return $"{methodInfo.DeclaringType?.Name}.{methodInfo.Name}";
        }
    }
}

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
namespace Microsoft365.SharePoint.CSOM
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using Microsoft.SharePoint.Client;
    using Microsoft365.Common.Logger;

    internal static class RetryableContextWrapper
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RetryableContextWrapper));
        private static Action<ClientContext, ClientRequest> SetRequests;
        private static Action<ClientRequest, ClientRequestStatus> SetRequestStatus;
        private static Action<ClientRequest, WebRequestExecutor> SetWebRequestExecutor;
        static RetryableContextWrapper()
        {
            FieldInfo fi_request = typeof(ClientRuntimeContext).GetField("m_request", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_status = typeof(ClientRequest).GetField("m_requestStatus", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_WebRequestExecutor = typeof(ClientRequest).GetField("m_requestExecutor", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi_request == null || fi_status == null
                || fi_WebRequestExecutor == null)
            {
                throw new ArgumentNullException("RetryableClientContext", "Please verify the private field");
            }

            SetRequests = CreateFieldSetterDelegate<ClientContext, ClientRequest>(fi_request);
            SetRequestStatus = CreateFieldSetterDelegate<ClientRequest, ClientRequestStatus>(fi_status);
            SetWebRequestExecutor = CreateFieldSetterDelegate<ClientRequest, WebRequestExecutor>(fi_WebRequestExecutor);
        }
        private static Action<T1, T2> CreateFieldSetterDelegate<T1, T2>(FieldInfo field)
        {
            if (field.ReflectedType.IsValueType) throw new ArgumentException("cannot set field for value type.");

            string methodName = string.Format("{0}.set_{1}", field.ReflectedType, field.Name);
            var setter = new DynamicMethod(methodName, null, new Type[] { typeof(T1), typeof(T2) }, field.ReflectedType, true);
            var ilGen = setter.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, field);
            ilGen.Emit(OpCodes.Ret);
            return (Action<T1, T2>)setter.CreateDelegate(typeof(Action<T1, T2>));
        }

        public static void ResetClientContext(this ClientContext context)
        {
            SetRequestStatus(context.PendingRequest, ClientRequestStatus.Active);
            SetWebRequestExecutor(context.PendingRequest, null);
            SetRequests(context, context.PendingRequest);
        }


        public static void ExecuteQueryWithRetry(this RetryableClientContext context, int retryCount = 3, int retryInterval = 5000)
        {
            ExecuteQueryWithRetryInternal(context, retryCount, retryInterval, context.RefreshTokenAction);
        }

        public static void ExecuteQueryWithRetry(this RetryableProjectClientContext context, int retryCount = 3, int retryInterval = 5000)
        {
            ExecuteQueryWithRetryInternal(context, retryCount, retryInterval, context.RefreshTokenAction);
        }

        private static void ExecuteQueryWithRetryInternal(ClientContext context,int retryCount,int retryInterval, Action<ClientContext> refreshTokenAction)
        {
            if (!context.HasPendingRequest)
            {
                return;
            }
            for (int count = 0; count < retryCount; count++)
            {
                try
                {
                    if (count > 0)
                    {
                        context.ResetClientContext();
                    }
                    context.ExecuteQuery();
                    break;
                }
                catch (Exception e)
                {
                    if (!ShouldRetry(context,e,retryInterval,refreshTokenAction) || count == retryCount - 1)
                    {
                        throw;
                    }
                }
            }
        }

        private static bool IsForbiddenException(WebException we)
        {
            if (we.Response != null)
            {
                var webResponse = we.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ShouldRetry(ClientContext context,Exception e,int retryInterval,Action<ClientContext> refreshTokenAction)
        {
            int retrInterval = 0;
            if (RequestExceptionHanddler.IsTimedoutException(e, ref retrInterval))
            {
                logger.Warn("Timeout exception will not be retried.");
                return false;
            }
            if (e is ServerException)
            {
                var serverException = e as ServerException;
                logger.Warn("Failed to execute discover context, TraceCorrelationId:{0} ServerErrorCode:{1} Error:{2}",
                    serverException.ServerErrorTraceCorrelationId, serverException.ServerErrorCode, serverException);

                if (serverException.ServerErrorCode == SPErrorCode.TP_E_LISTDELETED
                    || serverException.ServerErrorCode == SPErrorCode.FILE_NOT_FOUND
                    || serverException.ServerErrorCode == SPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST
                    || serverException.ServerErrorCode == SPErrorCode.ACCESS_DENIED
                    || serverException.ServerErrorCode == SPErrorCode.TP_E_FIELDNOTFOUND)
                {
                    return false;
                }
                Thread.Sleep(retryInterval);
            }
            else if (e is WebException)
            {
                var webException = e as WebException;
                var webResponse = webException.Response as HttpWebResponse;
                if (webResponse != null)
                {
                    logger.Warn("Failed to execute context, WebExceptionStatus:{0} StatusCode:{1} Error:{2}", webException.Status, webResponse.StatusCode, webException);
                    if (webResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                else
                {
                    logger.Warn("Failed to execute context, WebExceptionStatus:{0} Error:{1}", webException.Status, webException);
                }
                if (IsForbiddenException(webException) || RequestExceptionHanddler.IsUnauthorizedException(webException))
                {
                    refreshTokenAction(context);
                }
                Thread.Sleep(retryInterval * 2);
            }
            else
            {
                logger.Warn("Failed to execute context, Error:{0}", e);
                Thread.Sleep(retryInterval);
            }
            return true;
        }

    }
}
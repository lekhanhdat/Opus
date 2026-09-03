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

namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.Net;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using AveClientRequest.Common;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;

    /// <summary>
    /// 仅限于read操作，比如写操作的retry在这里可能不work。
    /// </summary>
    public class AveClientContextForDiscover : ClientContext
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveClientContextForDiscover));
        private static Action<ClientContext, ClientRequest> SetRequests;
        private static Action<ClientRequest, ClientRequestStatus> SetRequestStatus;
        private static Action<ClientRequest, WebRequestExecutor> SetWebRequestExecutor;
        private Action refreshTokenAction;
        private const int RETRYCOUNT = 3;
        private const int RETRYINTERVAL = 5000;
        private Guid mTenantId = Guid.Empty;
        static AveClientContextForDiscover()
        {
            FieldInfo fi_request = typeof(ClientRuntimeContext).GetField("m_request", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_status = typeof(ClientRequest).GetField("m_requestStatus", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_WebRequestExecutor = typeof(ClientRequest).GetField("m_requestExecutor", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi_request == null || fi_status == null
                || fi_WebRequestExecutor == null)
            {
                throw new ArgumentNullException("AveClientContextForDiscover", "Please verify the private field");
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

        public AveClientContextForDiscover(string webFullUrl, string tenantId = null, Action<WebRequest> changeTokenFunc = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
            : base(webFullUrl)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    mTenantId = new Guid(tenantId);
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occured when AveClientContextForDiscover,error:{e}");
            }
            this.WebRequestExecutorFactory = new AveWebRequestExecutorFactory(new DataMonitor(), changeTokenFunc, getTenantIdAndDefaultAppIdFunc);
        }

        public void RefreshToken(Action refreshToken)
        {
            this.refreshTokenAction = refreshToken;
        }

        private void ResetContext(ClientContext context, ClientRequest pendingRequest)
        {
            SetRequestStatus(pendingRequest, ClientRequestStatus.Active);
            SetWebRequestExecutor(pendingRequest, null);
            SetRequests(context, pendingRequest);
        }

        public override void ExecuteQuery()
        {
            if (!base.HasPendingRequest)
            {
                return;
            }
            var pendingRequest = base.PendingRequest;
            string baseScopeName = GetPerformanceScope();
            for (int count = 0; count < RETRYCOUNT; count++)
            {
                try
                {
                    string currentScopeName = (count == 0) ? baseScopeName : $"{baseScopeName}.Retry{count}";
                    using (var pc = new AveRequestStatisticScope(currentScopeName))
                    {
                        if (count > 0)
                        {
                            ResetContext(this, pendingRequest);
                        }

                        base.ExecuteQuery();
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (!ShouldRetry(e) || count == RETRYCOUNT - 1)
                    {
                        throw;
                    }
                }
            }
        }

        private string GetPerformanceScope()
        {
            try
            {
                // Skip 2 frames: 
                // Frame 0: GetPerformanceScope()
                // Frame 1: AveClientContextForDiscover.ExecuteQuery()
                var st = new System.Diagnostics.StackTrace(skipFrames: 2, fNeedFileInfo: false);

                for (int i = 0; i < st.FrameCount; i++)
                {
                    var method = st.GetFrame(i)?.GetMethod();
                    var declaringType = method?.DeclaringType;

                    if (declaringType == null) continue;

                    if (typeof(ClientContext).IsAssignableFrom(declaringType)) continue;

                    bool isCompilerGenerated = declaringType.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);

                    if (isCompilerGenerated)
                    {
                        // async/yield/iterator method
                        var realClass = declaringType.DeclaringType?.Name ?? declaringType.Name;

                        string realMethod = declaringType.Name;
                        int startIndex = realMethod.IndexOf('<');
                        int endIndex = realMethod.IndexOf('>');
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            realMethod = realMethod.Substring(startIndex + 1, endIndex - startIndex - 1);
                        }

                        return $"AveClientContextForDiscover.ExecuteQuery-{realClass}.{realMethod}";
                    }
                    else
                    {
                        // sync
                        return $"AveClientContextForDiscover.ExecuteQuery-{declaringType.Name}.{method.Name}";
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("[AveClientContextForDiscover.GetPerformanceScope] An error occurred while getting caller class and method. Error:{0}", e);
            }

            return "AveClientContextForDiscover.ExecuteQuery-Unknown";
        }

        private bool IsForbiddenException(WebException we)
        {
            if (this.refreshTokenAction != null && we.Response != null)
            {
                var webResponse = we.Response as HttpWebResponse;
                if (webResponse != null && webResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ShouldRetry(Exception e)
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

                if (serverException.ServerErrorCode == AveSPErrorCode.TP_E_LISTDELETED
                    || serverException.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND
                    || serverException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST
                    || serverException.ServerErrorCode == AveSPErrorCode.ACCESS_DENIED
                    || serverException.ServerErrorCode == AveSPErrorCode.TP_E_FIELDNOTFOUND)
                {
                    return false;
                }
                Thread.Sleep(RETRYINTERVAL);
            }
            else if (e is WebException)
            {
                var webException = e as WebException;
                var webResponse = webException.Response as HttpWebResponse;
                if (webResponse != null)
                {
                    logger.Warn("Failed to execute discover context, WebExceptionStatus:{0} StatusCode:{1} Error:{2}", webException.Status, webResponse.StatusCode, webException);
                }
                else
                {
                    logger.Warn("Failed to execute discover context, WebExceptionStatus:{0} Error:{1}", webException.Status, webException);
                }
                if (IsForbiddenException(webException) || RequestExceptionHanddler.IsUnauthorizedWebException(webException))
                {
                    this.refreshTokenAction();
                }
                Thread.Sleep(RETRYINTERVAL * 2);
            }
            else
            {
                logger.Warn("Failed to execute discover context, Error:{0}", e);
                logger.Warn($"Failed to execute discover context addtional message, FullStactTrace:{System.Environment.StackTrace}");
                Thread.Sleep(RETRYINTERVAL);
            }
            return true;
        }
    }
}
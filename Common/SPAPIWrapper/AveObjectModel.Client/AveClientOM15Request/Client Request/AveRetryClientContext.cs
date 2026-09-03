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
    using AveClientRequest.Common;
    using AvePoint.Wrapper.Common;
    using AvePoint.GCommon;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// 仅限于read操作，比如写操作的retry在这里可能不work。
    /// </summary>
    public class AveRetryClientContext : ClientContext
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveRetryClientContext));
        static Action<ClientContext, ClientRequest> SetRequests;
        static Action<ClientRequest, ClientRequestStatus> SetRequestStatus;
        static Action<ClientRequest, WebRequestExecutor> SetWebRequestExecutor;
        private Action refreshTokenAction;
        private const int RETRYCOUNT = 3;
        private const int RETRYINTERVAL = 5000;
        private Guid mTenantId = Guid.Empty;

        static AveRetryClientContext()
        {
            FieldInfo fi_request = typeof(ClientRuntimeContext).GetField("m_request", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_status = typeof(ClientRequest).GetField("m_requestStatus", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_WebRequestExecutor = typeof(ClientRequest).GetField("m_requestExecutor", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi_request == null || fi_status == null
                || fi_WebRequestExecutor == null)
            {
                throw new ArgumentNullException("AveRetryClientContext", "Please verify the private field");
            }

            SetRequests = CreateFieldSetterDelegate<ClientContext, ClientRequest>(fi_request);
            SetRequestStatus = CreateFieldSetterDelegate<ClientRequest, ClientRequestStatus>(fi_status);
            SetWebRequestExecutor = CreateFieldSetterDelegate<ClientRequest, WebRequestExecutor>(fi_WebRequestExecutor);
        }

        static Action<T1, T2> CreateFieldSetterDelegate<T1, T2>(FieldInfo field)
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

        public AveRetryClientContext(string webFullUrl, string tenantId = null, Action<WebRequest> changeTokenFunc = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
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
                mLog.Error($"Convert to guid failed. Error : {e}");
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
            bool needRetry = false;
            string scopeName = GetPerformanceScope();
            for (int count = 0; count < RETRYCOUNT; count++)
            {
                try
                {
                    string currentScopeName = (count == 0) ? scopeName : $"{scopeName}.Retry{count}";
                    using (var pc = new AveRequestStatisticScope(currentScopeName))
                    {
                        if (needRetry)
                        {
                            ResetContext(this, pendingRequest);
                        }
                        base.ExecuteQuery();
                    }
                    break;
                }
                catch (SiteLockException)
                {
                    throw;
                }
                catch (ServerException se)/*review-qlluo*/
                {
                    if (se.ServerErrorCode == AveSPErrorCode.TP_E_CHANGE_TOKEN_TOO_EARLY)
                    {
                        throw new AveChangeTokenExpireException(se.Message);
                    }
                    if (count == RETRYCOUNT - 1)
                    {
                        throw;
                    }
                    mLog.Warn("An error occurred while executing query. Error:{0}", se);
                    if (ShouldRetry(se))
                    {
                        needRetry = true;
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (WebException we) /*review-qlluo*/
                {
                    if (count == RETRYCOUNT - 1)
                    {
                        if (IsForbiddenException(we))
                        {
                            count--;
                            needRetry = true;
                            this.refreshTokenAction = null;
                            continue;
                        }
                        throw;
                    }
                    mLog.Warn("An error occurred while executing query. Error:{0}", we);
                    if (ShouldRetry(we))
                    {
                        needRetry = true;
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
               
                catch (Exception ex)/*review-qlluo*/
                {
                    if (count == RETRYCOUNT - 1)
                    {
                        throw;
                    }
                    mLog.Warn("An error occurred while executing query. Error:{0}", ex);
                    if (ShouldRetry(ex))
                    {
                        needRetry = true;
                        continue;
                    }
                    else
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
                // Frame 1: AveRetryClientContext.ExecuteQuery()
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

                        // something like "<GetFolders>d__834".MoveNext -> "GetFolders"
                        string realMethod = declaringType.Name;
                        int startIndex = realMethod.IndexOf('<');
                        int endIndex = realMethod.IndexOf('>');
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            realMethod = realMethod.Substring(startIndex + 1, endIndex - startIndex - 1);
                        }

                        return $"AveRetryClientContext.ExecuteQuery-{realClass}.{realMethod}";
                    }
                    else
                    {
                        // sync
                        return $"AveRetryClientContext.ExecuteQuery-{declaringType.Name}.{method.Name}";
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error("[AveRetryClientContext.GetPerformanceScope] An error occurred while getting caller class and method. Error:{0}", e);
            }

            return "AveRetryClientContext.ExecuteQuery-Unknown";
        }

        private bool ShouldRetry(ServerException se)
        {
            int errorCode = se.ServerErrorCode;
            if (errorCode == AveSPErrorCode.TP_E_OVERQUOTA || errorCode == AveSPErrorCode.V_OVER_QUOTA
                || errorCode == AveSPErrorCode.ERROR_NOT_ENOUGH_QUOTA)
            {
                return false;
            }
            if (errorCode == AveSPErrorCode.TP_E_USER_DOESNOT_EXIST)
            {
                return false;
            }
            if (string.Equals(se.ServerErrorTypeName, "System.IO.FileNotFoundException", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            Thread.Sleep(RETRYINTERVAL);

            return true;
        }

        private bool ShouldRetry(WebException we)
        {
            if (IsForbiddenException(we))
            {
                Thread.Sleep(RETRYINTERVAL);
                return true;
            }
            if (IsUnstableNetworkException(we))
            {
                Thread.Sleep(RETRYINTERVAL);
                return true;
            }
            if (IsServerProtocolViolationError(we))
            {
                Thread.Sleep(RETRYINTERVAL * 2);
                return true;
            }
            return false;
        }

        private bool ShouldRetry(Exception ex)
        {
            if (IsConnectonForciblyClosedExceptioin(ex) || IsRetryableServerException(ex))
            {
                Thread.Sleep(RETRYINTERVAL);
                return true;
            }
            if (IsSpecialException(ex))
            {
                Thread.Sleep(RETRYINTERVAL);
                return true;
            }
            if (RequestExceptionHanddler.IsMetadataServiceServerException(ex))
            {
                System.Threading.Thread.Sleep(RETRYINTERVAL);
                return true;
            }
            return false;
        }

        private bool IsForbiddenException(WebException we)
        {
            if (this.refreshTokenAction != null)
            {
                if (RequestExceptionHanddler.IsForbiddenWebException(we))
                {
                    this.refreshTokenAction();
                    return true;
                }
            }
            return false;
        }

        private bool IsConnectonForciblyClosedExceptioin(Exception te)
        {
            return RequestExceptionHanddler.IsConnectonForciblyClosedExceptioin(te);
        }

        private bool IsUnstableNetworkException(WebException e)
        {
            return RequestExceptionHanddler.IsUnstableNetworkException(e);
        }

        private bool IsRetryableServerException(Exception e)
        {
            return RequestExceptionHanddler.IsServerException(e) 
                && RequestExceptionHanddler.Is0x80131904Exception(e);
        }

        private bool IsServerProtocolViolationError(WebException we)
        {
            int interval=0;
            return RequestExceptionHanddler.IsServerProtocolViolationError(we,ref interval);
        }

        private bool IsSpecialException(Exception e)
        {
            return RequestExceptionHanddler.IsUnexpectedResponseException(e)
                || RequestExceptionHanddler.IsRequestChannelTimeoutException(e);
        }
    }
}

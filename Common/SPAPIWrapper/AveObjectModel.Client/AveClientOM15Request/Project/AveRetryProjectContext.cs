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
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using Microsoft.SharePoint.Client;
using Microsoft.ProjectServer.Client;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Net.Sockets;
using System.IO;
using System.Net;
using Microsoft365.SharePoint.CSOM.Extension;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveRetryProjectContext : ProjectContext
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        static Action<ClientContext, ClientRequest> SetRequests;
        static Action<ClientRequest, ClientRequestStatus> SetRequestStatus;
        static Action<ClientRequest, WebRequestExecutor> SetWebRequestExecutor;
        private const int RETRYCOUNT = 3;
        private Guid mTenantId = Guid.Empty;
        static AveRetryProjectContext()
        {
            FieldInfo fi_request = typeof(ClientRuntimeContext).GetField("m_request", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_status = typeof(ClientRequest).GetField("m_requestStatus", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fi_WebRequestExecutor = typeof(ClientRequest).GetField("m_requestExecutor", BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi_request == null || fi_status == null
                || fi_WebRequestExecutor == null)
            {
                throw new ArgumentNullException("AveRetryProjectContext", "Please verify the private field");
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

        public AveRetryProjectContext(string webFullUrl, string tenantId = null, Action<WebRequest> changeTokenFunc = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
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
                mLog.Error($"error occured when AveRetryProjectContext,error:{e}");
            }
            this.WebRequestExecutorFactory = new AveWebRequestExecutorFactory(new DataMonitor(), changeTokenFunc, getTenantIdAndDefaultAppIdFunc);
        }

        private void ResetContext(ClientContext context, ClientRequest pendingRequest)
        {
            SetRequestStatus(pendingRequest, ClientRequestStatus.Active);
            SetWebRequestExecutor(pendingRequest, null);
            SetRequests(context, pendingRequest);
        }

        private void RefreshFormDigest(ClientContext context)
        {
            context.SetFormDigest();
        }

        public override void ExecuteQuery()
        {
            if (!base.HasPendingRequest)
            {
                return;
            }
            var pendingRequest = base.PendingRequest;
            bool needRetry = false;
            for (int count = 0; count < RETRYCOUNT; count++)
            {
                try
                {
                    if (needRetry)
                    {
                        ResetContext(this, pendingRequest);
                    }
                    RefreshFormDigest(this);
                    base.ExecuteQuery();
                    break;
                }
                catch (SiteLockException ex)
                {
                    throw;
                }
                /*review-qlluo*/
                catch (ServerException se)
                {
                    if (count == RETRYCOUNT - 1)
                    {
                        throw;
                    }

                    if (ShouldRetry(se))
                    {
                        mLog.Warn("retry count:{0}", count + 1);
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
        private bool ShouldRetry(ServerException se)
        {
            int errorCode = se.ServerErrorCode;
            mLog.Warn("error happened, error message:{0}, errorCode:{1}", se.ToString(), errorCode);
            return false;
        }    
    }
}

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
using AvePoint.Hybrid.Utility.Net;
using HybirdProxy.Implement;
using HybirdProxy.Token;
using HybridCommonModel.DataModel;
using HybridCommonModel.Utils;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.Hybrid.AgentService
{
    public class RASignalRProxy 
    {
        private static ManagerProxy managerSingleton;
        private static Object mLock = new Object();
        private static bool Connected = false;

        public static void SignalRConnected(bool connected)
        {
            Connected = connected;
        }

        public static ManagerProxy GetManagerProxy()
        {
            if(managerSingleton != null && Connected)
            {
                return managerSingleton;
            }
            throw new Exception("SignalR server not setup.");
        }

        public static ManagerProxy GetManagerProxy(string connectionUrl, string clientId, string clientAuth, string scope, string IdentityServerClientId, string IdentityServerAddress, Func<X509Certificate2> communicationCertificateFunc, string tenantId = null, ILoggerFactory logFactory = null)
        {
            
            if (managerSingleton == null)
            {
                lock (mLock)
                {
                    if (managerSingleton == null)
                    {
                        managerSingleton = ManagerProxy.Get(connectionUrl, 
                            () => TokenHelper.RequestToken(AveHttpConnectionUtil.CreateHttpClient(), clientId, clientAuth, scope, IdentityServerClientId, IdentityServerAddress, communicationCertificateFunc, tenantId),  
                            logFactory, config: ConfigHttpProxy);
                    }
                }
            }

            return managerSingleton;
        }

        private static void ConfigHttpProxy(HttpConnectionOptions httpConnectionOptions)
        {
            var proxySetting = AveWebProxyUtil.ReadProxySetting();
            if (proxySetting == null || !proxySetting.Enabled) return;
            var webProxy = new AveWebProxy(proxySetting);
            httpConnectionOptions.Proxy = webProxy.Create();
        }
    }

}

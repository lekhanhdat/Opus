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
using AvePoint.RA.Common.HybridLogger;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.RACommonUtility.Http;
using HybirdProxy.Implement;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.RA.RACommonUtility
{
    public class RASignalRAgentProxy 
    {
        private static AgentProxy agentSingleton;
        private readonly static Object mLock = new Object();
        private static bool Connected = false;
        private static IRALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SignalRConnected(bool connected)
        {
            Connected = true ;
            logger.Info("signalr servce ensure connect status :" + Connected);
        }

        public static AgentProxy GetProxy()
        {
            if(agentSingleton != null)
            {
                return agentSingleton;
            }

            logger.Warn("Agent proxy is not initialize.");

            throw new Exception("SignalR server not setup.");
        }

        public static AgentProxy GetAgentProxy(string connectionUrl, string clientId, string clientAuth, string scope, string IdentityServerClientId, string IdentityServerAddress, Func<X509Certificate2> communicationCertificateFunc, string tenantId = null, ILoggerFactory logFactory = null)
        {
            
            if (agentSingleton == null)
            {
                lock (mLock)
                {
                    if (agentSingleton == null)
                    {
                        agentSingleton = AgentProxy.Get(connectionUrl,
                                        () => HybirdProxy.Token.TokenHelper.RequestToken(AveHttpClient.Create(), clientId, clientAuth, scope, IdentityServerClientId, IdentityServerAddress, communicationCertificateFunc, null),
                                        LoggerFactory.Create(builder => {
                                            builder.AddProvider(HybridLogger.loggerProvider);
                                        }), true);
                        logger.Info("Signalr agent proxy initionlize.");
                    }
                }
            }

            return agentSingleton;
        }

    }

}

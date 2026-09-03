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




using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Threading;

using AvePoint.GCommon;
using AvePoint.GCommon.Contract;
using AvePoint.GCommon.MicroKernel;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cloud;

namespace AvePoint.Common
{
    public class WcfUtility
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(WcfUtility));

        public static void CloseChannelSafely(ICommunicationObject channel)
        {
            if (channel != null)
            {
                try
                {
                    channel.Close();
                }
                catch (Exception e)
                {
                    channel.Abort();
                    mLogger.Debug(e.Message, e);
                }
            }
        }

        public static T GetAgentProcessService<T>()
        {
            return GetAgentProcessService<T>(AveEnv.AgentAddress, AveEnv.AgentPort, AveEnv.AgentSchema);
        }

        public static T GetAgentProcessService<T>(string address, int port)
        {
            return GetAgentProcessService<T>(address, port, AveEnv.AgentSchema);
        }

        public static T GetAgentProcessService<T>(string address, int port, string schema)
        {
            CoreServiceEndpointInfo endPointInfo = new CoreServiceEndpointInfo
            {
                HostOrIpAddress = address,
                Port = port,
                Scheme = schema,
                EndpointConfigurationName = "AgentCoreService",
                AuthorizationKey = CspCommunicationWrapper.AuthToken,
                X509CertificateValidationThumbprintFindValue=GCommonRoleConfiguration.WCF_Certificate
            };
            IProxyBuilder proxyBuilder = new RemotingProxyBuilder();
            return proxyBuilder.CreateProxy<T>(endPointInfo);
        }

        public static T GetManagerService<T>()
        {
            CoreServiceEndpointInfo endPointInfo = new CoreServiceEndpointInfo()
            {
                HostOrIpAddress = AveEnv.ManagerAddress,
                Port = AveEnv.ManagerPort,
                Scheme = "https",
                EndpointConfigurationName = "managerCoreService",
                AuthorizationKey = CspCommunicationWrapper.AuthToken,
                X509CertificateValidationThumbprintFindValue = GCommonRoleConfiguration.WCF_Certificate

            };
            IProxyBuilder proxyBuilder = new RemotingProxyBuilder();
            return proxyBuilder.CreateProxy<T>(endPointInfo);
        }

        public static Object GetManagerService(string interfaceFullName)
        {
            CoreServiceEndpointInfo endPointInfo = new CoreServiceEndpointInfo()
            {
                HostOrIpAddress = AveEnv.ManagerAddress,
                Port = AveEnv.ManagerPort,
                Scheme = "https",
                EndpointConfigurationName = "managerCoreService",
                AuthorizationKey = CspCommunicationWrapper.AuthToken,
                X509CertificateValidationThumbprintFindValue = GCommonRoleConfiguration.WCF_Certificate
            };
            IProxyBuilder proxyBuilder = new RemotingProxyBuilder();
            return proxyBuilder.CreateProxy(interfaceFullName, endPointInfo);
        }

        public static T GetMediaService<T>(string address, int port, string schema)
        {
            CoreServiceEndpointInfo endPointInfo = new CoreServiceEndpointInfo()
            {
                HostOrIpAddress = address,
                Port = port,
                Scheme = schema,
                EndpointConfigurationName = "mediaAgentCoreService",
                AuthorizationKey = CspCommunicationWrapper.AuthToken,
                X509CertificateValidationThumbprintFindValue = GCommonRoleConfiguration.WCF_Certificate
            };
            IProxyBuilder proxyBuilder = new RemotingProxyBuilder();
            return proxyBuilder.CreateProxy<T>(endPointInfo);
        }
    }
}

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

using AvePoint.GCommon;
using AvePoint.Hybrid.AgentService.Handler;
using AvePoint.Hybrid.AgentService.ServiceEndpoint;
using AvePoint.Hybrid.AgentService.SignalRHandler;
using AvePoint.Hybrid.Contract;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.CommonUtil;
using CommonModel.MethodInfo;
using HybirdProxy.Extensions;
using HybirdProxy.Implement;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;

namespace AvePoint.Hybrid.AgentService
{
    public class SignalRListenerService : IStartable
    {
        AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool threadQuit = false;
        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(60, TimeSpan.FromSeconds(10)));

        public void Start()
        {

            string SignalRServer = CommonConfiguration.getConfig(HybridAppSettingKey.SignalRServer);
            
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(StartSignalRClient);
            Thread signalrThread = new Thread(ParStart);
            object param = SignalRServer;
            signalrThread.Start(param);

        }

        public void StartSignalRClient(object SignalRHubUrl)
        {
            ManagerProxy proxy = null;
            try
            {

                logger.Info("Begin to register agent service .");

                string CustomerAgentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
                string CustomerAgentAuth = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAuthCode);
                string IdentityServerClientId = CommonConfiguration.getConfig(HybridAppSettingKey.PublicClientIdInIdentityService);
                string IdentityServerAddress = CommonConfiguration.getConfig(HybridAppSettingKey.PublicIdentityServiceURL);
                string CustomerTenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);

                logger.Info("SignalRHubUrl : " + SignalRHubUrl);
                logger.Info("IdentityServerClientId : " + IdentityServerClientId);
                logger.Info("IdentityServerAddress : " + IdentityServerAddress);
                logger.Info("CustomerTenantId : " + CustomerTenantId);

                proxy = RASignalRProxy.GetManagerProxy((string)SignalRHubUrl,
                    CustomerAgentId, CustomerAgentAuth, APIScope.Agent,
                    IdentityServerClientId, IdentityServerAddress, () => CommonConfiguration.getAppCert(),
                    CustomerTenantId, LoggerFactory.Create(builder => { builder.AddProvider(HybridCommonLogger.loggerProvider);}));
                proxy.ConfigureProxy(config => {
                    config.RetryInterval = 5000;
                    config.Retrytime = 3;
                });
                logger.Info("Finish to get manager proxy.");

                MethodTable.Registered(MethodMapping.MT);
                proxy.RegisterEndpoint(hub =>
                {
                    
                    hub.OnFuncWithReturn<STreeBrowserExecute, TreeBrowserArgs, BrowserResult>(proxy, new TreeBrowserHandler());
                    hub.OnFuncWithReturn<SFileSystemJobExecute, RecordsJobArgs, FileSystemJobResult>(proxy, new FileSystemJobHandler());
                    hub.OnFuncWithReturn<SharePointOnPremBrowserExecute, SharePointOnPremBrowserArgs, SharePointOnPremBrowserResult>(proxy, new SharePointOnPremBrowserHandler());
                    hub.OnFuncWithReturn<SharePointOnPremTermBrowserExecute, SharePointOnPremTermBrowserArgs, SharePointOnPremTermBrowserResult>(proxy, new SharePointOnPremTermBrowserHandler());
                    hub.OnFuncWithReturn<SharePointOnPremParentIdsBrowserExecute, SharePointOnPremParentIdsBrowserArgs, SharePointOnPremParentIdsBrowserResult>(proxy, new SharePointOnPremParentIdsBrowserHandler());
                    hub.OnFuncWithReturn<SharePointOnPremRealtimeJobExecute, SharePointOnPremRealtimeJobArgs, SharePointOnPremRealtimeJobResult>(proxy, new SharePointOnPremRealtimeJobHandler());
                    hub.OnFuncWithReturn<SAgentCertificateUpdateExecute, AgentCertificateUpdateArgs, AgentCertificateUpdateResult>(proxy, new CertificateUpdateHandler());
                    hub.OnFuncWithReturn<SharePointOnPremRelatedExecute, SharePointOnPremRelatedArgs, SharePointOnPremRelatedResult>(proxy, new SharePointOnPremRelatedHandler());
                    hub.OnFuncWithReturn<SharePointOnPremQuererExecute, SharePointOnPremQuererArgs, SharePointOnPremQuererResult>(proxy, new SharePointOnPremQuererHandler());
                    hub.OnFuncWithReturn<SharePointOnPremDisposalExecute, SharePointOnPremDisposalArgs, SharePointOnPremDisposalResult>(proxy, new SharePointOnPremDisposalHandler());
                    hub.OnFuncWithReturn<RecordsAgentUpgradeExecute, RecordsAgentUpgradeArgs, RecordsAgentUpgradeResult>(proxy, new RecordsAgentUpgradeHandler());
                    hub.OnFuncWithReturn(proxy, new FileSystemUNCPathValidateHandler());

                    hub.On<SRecordsJobStop>(MethodMapping.MT[typeof(SRecordsJobStop)], (jobStop) =>
                    {
                        logger.Info("Receive jobStop message. JobId: " + jobStop.MethodArgs?.JobId);
                        new FileSystemJobStopHandler().Handle(jobStop);
                    });

                    hub.On<SRecordsExplore>(MethodMapping.MT[typeof(SRecordsExplore)], (recordsExplore) =>
                    {
                        logger.Info("Receive message, job start message " + recordsExplore.MethodArgs);
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            var service = (IEPExploreService)WindsorManager.GetService("AvePoint.Hybrid.AgentService.ServiceEndpoint.EPExploreService", typeof(IEPExploreService));
                            service.Start(recordsExplore.MethodArgs);
                        });

                    });

                });

                bool conncted = proxy.EnsureConnect();
                logger.Info("Setup connection to signal server successfully.", conncted);
            }
            catch (Exception e)
            {
                logger.Error("Signalr server registered fail.",e);
            }
            finally
            {
                try
                {
                    if (proxy == null)
                    {
                        logger.Error("Proxy is not initilize, break the loop.");
                        RASignalRProxy.SignalRConnected(false);
                    }

                    if (proxy.EnsureConnect())
                    {
                        RASignalRProxy.SignalRConnected(true);
                        logger.Info("Finish to setup signalr server conneciton.");
                    }
                }
                catch(Exception e)
                {
                    logger.Error("Failed to connect to signalr server, ", e);
                    throw;
                }
                

            }

            while (!threadQuit)
            {
                logger.Info("SignalR listener service keeps running, sleep 60s.");
                Thread.Sleep(60 * 1000);
            }

            logger.Warn("Signalr listner service quit.");

        } 
        public event EventHandler OnStarting;
        public event EventHandler OnStarted;
        public event EventHandler OnStopping;
        public event EventHandler OnStopped;


        public void Stop()
        {
            threadQuit = true;
            logger.Info("Stop the signalr listner service.");
            
        }


    }



}

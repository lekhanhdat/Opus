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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Pipe;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Hybrid.Browser.Browser;
using AvePoint.RA.Hybrid.Browser.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.Util
{
    public class HybridBrowseCommunicator
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(HybridBrowseCommunicator));

        private static readonly string DefaultPipeName = "hranpcs";

        private static readonly Dictionary<HybridBrowserType, IBrowser> BrowserInstances = new Dictionary<HybridBrowserType, IBrowser>();

        private static Action HasCommunicationCallback = null;

        private static RANamedPipeServerStream Server;

        public static void Init()
        {
            InitBrowserInstace();
            InitPipeServer();
        }

        private static void InitBrowserInstace()
        {
            try
            {
                Logger.Info($"Begin initial browser instance.");
                var assembly = Assembly.GetAssembly(typeof(IBrowser));
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(typeof(IBrowser)))
                    {
                        var browserInstance = Activator.CreateInstance(type) as IBrowser;
                        if (browserInstance == null || BrowserInstances.ContainsKey(browserInstance.BrowserType))
                        {
                            Logger.Warn($"Browser Instance with duplicate type [{browserInstance.BrowserType}]");
                            continue;
                        }
                        BrowserInstances.Add(browserInstance.BrowserType, browserInstance);
                    }
                }
                Logger.Info($"Initial browser instance successful.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while initial browser instance. Error: {e}");
                throw;
            }
        }
        
        private static void InitPipeServer()
        {
            try
            {
                Logger.Info("Begin initial pipe server.");
                Server = new RANamedPipeServerStream(DefaultPipeName);
                Server.RegisterClientOnConnectCallBack(ClientOnConnect);
                Server.Connect();
                Logger.Info("Initial pipe server successful.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while initial pipe server. Error: {e}");
                throw;
            }
        }

        private static void ClientOnConnect(RANamedPipeServerStreamInner serverStream)
        {
            try
            {
                Logger.Info("Client on connect.");
                HasCommunicationCallback();
                var contract = serverStream.ReadMessage<HybridBrowserContract>();
                Logger.Info($"Request browser type: [{contract.BrowserType}], tenant id: [{contract.LogonGroupId}].");
                TenantLocalValue.LogonGroupId = contract.LogonGroupId;
                if (BrowserInstances.TryGetValue(contract.BrowserType, out var browserInstance))
                {
                    var result = browserInstance.Browse(contract.Message);
                    serverStream.SendMessage(result);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occur while processing the request. Error: {e}");
            }
        }

        public static void RegisteBrowseCallBack(Action callback)
        {
            HasCommunicationCallback = callback;
        }

        public static void Dispose()
        {
            Server?.Dispose();
        }
    }
}

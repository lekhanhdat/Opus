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
using CommonModel.Extensions;
using CommonModel.MethodInfo;
using HybirdProxy.Interface;
using HybirdProxy.Token;
using HybridProxy;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static HybirdProxy.Token.TokenHelper;

namespace HybirdProxy.Implement
{
    public class ManagerProxy :SignalRProxy, IManagerProxy
    {

        #region factory
        private readonly static object lockObj = new object();
        private static ManagerProxy Singleton { get; set; }
        
        public static ManagerProxy Get(string connectionUrl, Func<Task<string>> accessTokenProvider = null, ILoggerFactory loggerFactory = null,bool CheckTls=true, Action<HttpConnectionOptions> config = null)
        {
            if (Singleton == null)
            {
                lock (lockObj)
                {
                    if (Singleton == null)
                    {
                        Singleton = new ManagerProxy(connectionUrl, accessTokenProvider, ProxyConstants.Token_Source_Public, loggerFactory, CheckTls, config);
                    }
                }
            }

            return Singleton;
        }

        #endregion
        private ManagerProxy(string connectionUrl, Func<Task<string>> accessTokenProvider, string tokenSource, ILoggerFactory loggerFactory = null, bool CheckTls = true, Action<HttpConnectionOptions> config = null, SignalRConfiguration signalRConfig = null) :base(connectionUrl, accessTokenProvider, tokenSource,loggerFactory, CheckTls,config, signalRConfig)
        {

            this.connection.Closed += Connection_Closed;
            this.connection.Reconnecting += Connection_Reconnecting;
            this.connection.Reconnected += Connection_Reconnected;
            //this.connection.On();

            
        }

        protected override Task Connection_Reconnected(string arg)
        {
            //log here
            return base.Connection_Reconnected(arg);
        }

        protected override Task Connection_Reconnecting(Exception arg)
        {
            //log here
            return base.Connection_Reconnecting(arg);
        }

        protected override Task Connection_Closed(Exception arg)
        {
            return base.Connection_Closed(arg);
        }

        public async Task SendToManagerAsync<T>(T methodInfo) where T : RemoteMethod
        {
            EnsureConnect();
            await connection.InvokeAsync(HubMethodNames.SendMessageToManager, new HubMethodParam() { MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to manager,methodName:{methodInfo.MethodName}");
        }

        public async Task SendCallbackToManagerAsync<T>(T result) where T : RemoteInvoke
        {
            EnsureConnect();
            await connection.InvokeAsync(HubMethodNames.SendCallbacvkToManagerAsync, new HubMethodParam() { Mode = DeliverMode.RPCResult, ManagerId = result.ManagerId }, result);
            this._logger.Info($"call back to manager, managerId:{result.ManagerId}");
        }

        public override bool EnsureConnect(Action postaction = null)
        {
            StartHeartbeat();
            return base.EnsureConnect(postaction);
        }
    }
}

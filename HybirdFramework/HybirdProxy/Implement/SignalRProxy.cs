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
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HybirdProxy.Implement
{
    public class SignalRProxy : IProxy
    {
        private readonly object lockRoot = new object();
        private readonly object lockObj = new object();
        protected ILogger _logger { get; private set; }
        protected SignalRProxyLogProvider _loggerProvider { get; private set; }
        protected HubConnection connection { get; set; }

        protected IHubConnectionBuilder cBuilder { get; set; }
        protected string ConnectionUrl { get; set; }
        protected Func<Task<string>> accessTokenProvider { set;get; }
        protected string tokenSource { set; get; }
        protected ILoggerFactory logFactory { set; get; }
        protected bool CheckTLS { set; get; }
        protected Action<HttpConnectionOptions> config { set; get; }


        protected bool IsConnected
        {
            get
            {
                return this.connection != null && this.connection.State == HubConnectionState.Connected;
            }
        }

        protected bool HasRegistered { get; set; }

        protected bool HasConfiguredLog { get; set; }

        protected bool HasHeartbeat { get; set; }

        protected virtual ProxyConfiguration Configuration { get; set; } = new ProxyConfiguration();

        protected SignalRProxy(string connectionUrl, Func<Task<string>> accessTokenProvider, string tokenSource, ILoggerFactory logFactory = null, bool CheckTLS = true, Action<HttpConnectionOptions> config = null, SignalRConfiguration signalRConfig = null)
        {
            #region config log

            if (logFactory == null)
            {
                //default no log
                logFactory = new LoggerFactory();
            }
            this._logger = logFactory.CreateLogger(this.GetType().FullName);
            this._loggerProvider = new SignalRProxyLogProvider(logFactory);
            this.HasConfiguredLog = true;

            #endregion

            this.ConnectionUrl = connectionUrl;
            this.accessTokenProvider = accessTokenProvider;
            this.tokenSource = tokenSource;
            this.CheckTLS = CheckTLS;
            this.config = config;
            this.connection = CreateConnection();

            #region config connection

            if(signalRConfig != null)
            {
                this.connection.ServerTimeout = signalRConfig.ServerTimeout;
                this.connection.HandshakeTimeout = signalRConfig.HandshakeTimeout;
                this.connection.KeepAliveInterval = signalRConfig.KeepAliveInterval;
            }

            #endregion 

        }

        private HubConnection CreateConnection()
        {
            return new HubConnectionBuilder().WithUrl(this.ConnectionUrl, HttpTransportType.WebSockets | HttpTransportType.LongPolling | HttpTransportType.ServerSentEvents, options =>
            {
                this._logger.Info($"CheckTLS is: {CheckTLS}.");
#if DEBUG
                options.HttpMessageHandlerFactory = (handler) =>
                {
                    var newHandler = handler as HttpClientHandler;
                    newHandler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        return true;
                    };
                    return newHandler;
                };
#endif
                if (!CheckTLS)
                {
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        var newHandler = handler as HttpClientHandler;
                        //newHandler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                        //{
                        //    return true;
                        //};
                        return newHandler;
                    };
                }
                if (config != null)
                {
                    config(options);
                }
                if(accessTokenProvider != null)
                {
                    options.AccessTokenProvider = accessTokenProvider;
                    options.Headers.Add("Token-Source", tokenSource);
                }

            }).ConfigureLogging(logging => { logging.AddProvider(this._loggerProvider); }).Build(); 
        }


        protected virtual Task Connection_Reconnected(string arg)
        {
            this._logger.Info($"Connection reconnected,arg:{arg}");
            //log here
            return Task.CompletedTask;
        }

        protected virtual Task Connection_Reconnecting(Exception arg)
        {
            this._logger.Info($"Connection reconnecting,exception:{arg?.ToString()}");

            //log here
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg">note: some senario the arg is null</param>
        /// <returns></returns>
        protected virtual Task Connection_Closed(Exception arg)
        {
            //log here
            this._logger.Info($"Connection closed, exception:{arg?.ToString()}");

            //todo exception handling?
            EnsureConnect();
            return Task.CompletedTask;
        }

        protected virtual void StartHeartbeat(string message = "")
        {
            if (!this.HasHeartbeat)
            {
                lock (lockRoot)
                {
                    this.HasHeartbeat = true;

                    //long running timer job
                    Task timerjob = new Task(() =>
                    {
                         do
                         {
                             try
                             {
                                 EnsureConnect();
                                 connection.InvokeAsync(HubMethodNames.Heartbeat, message).Wait();
                                 _logger.Info("connectionId : "+ connection.ConnectionId + ", heart beat!");
                             }
                            catch (Exception e)
                             {
                                _logger.Warn("error occured while heart beat:" + e.ToString());
                             }

                             finally
                             {
                                 Thread.Sleep(this.Configuration.HeartbeatInterval);
                             }
                         }
                         while (true);        

                    }, TaskCreationOptions.LongRunning);
                    timerjob.Start();
                }
            }        
        }

        public virtual bool EnsureConnect(Action postaction = null)
        {
            if (this.IsConnected)
            {
                return true;
            }
            else
            {
                //need init connections
                if (!this.HasRegistered)
                {
                    throw new InvalidOperationException("The exposed method has not been registered!");
                }

                if(!this.HasConfiguredLog)
                {
                    throw new InvalidOperationException("The logger has not been configured!");
                }

                lock (lockObj)
                {
                    //if (this.connection == null)
                    //{
                    //    this.connection = this.cBuilder.Build();
                    //}

                    if (this.connection.State == HubConnectionState.Disconnected)
                    {
                        int retryTime = 0;
                        do
                        {
                            try
                            {
                                this._logger.Info($"start connection...");
                                this.connection.StartAsync().Wait();
                                Debug.Assert(connection.State == HubConnectionState.Connected);

                                if (connection.State == HubConnectionState.Connected)
                                {
                                    this._logger.Info($"connection success");
                                    break;
                                }
                            }
                            catch (Exception e)
                            {
                                this._logger.Warn($"error occurred when start connection: retrytimes: {retryTime},exception: {e.ToString()}");
                                
                                Debug.Assert(connection.State == HubConnectionState.Disconnected);
                                Thread.Sleep(this.Configuration.RetryInterval);
                            }
                            finally
                            {
                                retryTime++;
                            }
                        }
                        while (retryTime < this.Configuration.Retrytime);

                        if (connection.State != HubConnectionState.Connected)
                        {
                            throw new UnexpectedException("Can not connect to signalR server.");
                        }
                        else
                        {
                            //handshake
                            HandShake();
                            if(postaction != null)
                            {
                                postaction();
                            }
                            return true;
                        }
                    }
                    else if (this.connection.State == HubConnectionState.Connecting || this.connection.State == HubConnectionState.Reconnecting)
                    {
                        //to do
                        // how about reconnecting and connecting?
                        throw new UnexpectedException("unexpected error happens");
                    }
                }
                return false;
            }
        }

        public virtual void HandShake()
        {
            EnsureConnect();
            this._logger.Info("start handshake with server");
            connection.InvokeAsync(HubMethodNames.HandShake, string.Empty).Wait();
            this._logger.Info("handshake complate");
        }

        public virtual void RegisterEndpoint(Action<HubConnection> register)
        {
            if (this.connection == null)
            {
                throw new ArgumentNullException("connection is null");
            }

            if (this.HasRegistered)
            {
                throw new InvalidOperationException("you should not register endpoint more than once");
            }

            register(this.connection);
            this.HasRegistered = true;
        }

        //public void ConfigureLog(ILoggerFactory logFactory)
        //{
        //    if(logFactory == null)
        //    {
        //        throw new ArgumentNullException("logProvider");
        //    }
        //    this._logger = logFactory.CreateLogger(this.GetType().FullName);
        //    this._loggerProvider = new SignalRProxyLogProvider(logFactory);
        //    this.cBuilder.ConfigureLogging(logging => { logging.AddProvider(this._loggerProvider); });
        //    this.HasConfiguredLog = true;

        //}

        public void ConfigureProxy(Action<ProxyConfiguration> action)
        {
            action(this.Configuration);
        }
    }

    public class ProxyConfiguration
    {
        public int Retrytime = int.MaxValue;
        public int RetryInterval = 5000;
        public int HeartbeatInterval = 60000;
        //seconds
        public int InvokeTimeout = 300;
    }

    public class SignalRConfiguration
    {
        public TimeSpan ServerTimeout = TimeSpan.FromSeconds(300);
        public TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(15);
        public TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(15);
    }

    public class SignalRProxyLogProvider : ILoggerProvider
    {
        private ILoggerFactory logfactory;
        public SignalRProxyLogProvider(ILoggerFactory factory)
        {
            logfactory = factory;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return logfactory.CreateLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}

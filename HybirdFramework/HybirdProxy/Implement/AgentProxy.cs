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
using CommonModel.DataModel;
using CommonModel.MethodInfo;
using CommonModel.Utils;
using CommonModel.Extensions;
using HybirdProxy.Interface;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using HybirdProxy.Token;
using static HybirdProxy.Token.TokenHelper;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using HybridProxy;
using Microsoft.AspNetCore.Http.Connections.Client;

namespace HybirdProxy.Implement
{
    public class AgentProxy :SignalRProxy, IAgentProxy
    {

        #region factory
        private readonly static object lockObj = new object();
        private static AgentProxy Singleton { get; set; }

        /// <summary>
        /// This managerId will be generated automatically to support server auto scale senario.
        /// The id will be passed to signalR server when handshake, the signalR server will records this id in memory but not persist it.
        /// </summary>
        private string ManagerId { get; set; } = Guid.NewGuid().ToString();

        
        public static AgentProxy Get(string connectionUrl, Func<Task<string>> accessTokenProvider = null, ILoggerFactory logFactory = null,bool checkTls = true)
        {
            if (Singleton == null)
            {
                lock (lockObj)
                {
                    if (Singleton == null)
                    {
                        
                        Singleton = new AgentProxy(connectionUrl, accessTokenProvider, ProxyConstants.Token_Source_Internal, logFactory, checkTls);
                    }
                }
            }

            return Singleton;
        }

            #endregion

        private Dictionary<string, List<AgentInformation>> AgentInfos = new Dictionary<string, List<AgentInformation>>();

        public event EventHandler AgentConnectionStateChange;

          
        private AgentProxy(string connectionUrl, Func<Task<string>> accessTokenProvider, string tokenSouce,ILoggerFactory logFactory = null, bool checkTls = false, Action<HttpConnectionOptions> config = null, SignalRConfiguration signalRconfig = null) : base(connectionUrl, accessTokenProvider, tokenSouce, logFactory, checkTls, config,signalRconfig)
        {            
            this.connection.Closed += Connection_Closed;
            this.connection.Reconnecting += Connection_Reconnecting;
            this.connection.Reconnected += Connection_Reconnected;
            this.connection.On(HubMethodNames.AgentConnectionNotification, () =>
            {
                this._logger.Info("Received agent connection changed notification.");
                EnsureConnect();
                this.GetAgentsInfo();
                AgentConnectionStateChange(this, new EventArgs());
            });

            this.connection.On<AgentProxyCallback>(HubMethodNames.AgentRPCCallback, (result) =>
            {
                try
                {
                    var s = result.MethodResult.ToString();

                    if (PendingCalls.TryRemove(result.SessionId, out TaskCompletionSourceHandler handler))
                    {
                        if (handler == null)
                        {
                            throw new UnexpectedException($"can not remove handler! handler is null, {result.SessionId}");
                        }

                        handler.CompleteTaskSource(s);
                    }
                    else
                    {
                        this._logger.Info($"can not find handler, might has been removed due to timeout, please check log.");
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e.ToString());
                    throw;
                }
            });


        }

        protected override Task Connection_Reconnected(string arg)
        {
            return base.Connection_Reconnected(arg);
        }

        protected override Task Connection_Reconnecting(Exception arg)
        {
            return base.Connection_Reconnecting(arg);
        }

        protected override Task Connection_Closed(Exception arg)
        {
            return base.Connection_Closed(arg);
        }

        public ICollection<AgentInformation> GetAgents(string tenantId)
        {
            EnsureTenantExist(tenantId);
            return this.AgentInfos[tenantId].ToArray();
        }

        public Dictionary<string, List<AgentInformation>> GetAllAgents()
        {
            return this.AgentInfos;
        }

        public ICollection<AgentInformation> GetAgentsForce(string tenantId)
        {
            this.GetAgentsInfo();
            return this.GetAgents(tenantId);
        }

        public Dictionary<string, List<AgentInformation>> GetAllAgentsForce()
        {
            this.GetAgentsInfo();
            return this.GetAllAgents();
        }

        public async Task SendToAgentAsync<T>(string tenantId, string agentId, T methodInfo) where T : RemoteMethod
        {
            //EnsureTenantExist(tenantId);
            EnsureTenantAgentRelationShip(tenantId, agentId);
            EnsureConnect();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { AgentId = agentId, Mode = DeliverMode.One, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to agent:{agentId},tenantId:{tenantId},methodName:{methodInfo.MethodName}");
        }

        public async Task SendToAllAgentAsync<T>(string tenantId, T methodInfo) where T : RemoteMethod
        {
            EnsureTenantExist(tenantId);
            var agents = this.AgentInfos[tenantId];
            var activeAgents = agents.Where(a => a.Status == ConnectionStatus.Connected).ToList();

            if (activeAgents.Count == 0)
            {
                throw new InvalidOperationException($"There is no active agents for tenant:{tenantId} at current");
            }

            EnsureConnect();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { Mode = DeliverMode.All, TenantId = tenantId, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to all agents,tenantId:{tenantId},methodName:{methodInfo.MethodName}");
        }

        public async Task SendToOneAgentAsync<T>(string tenantId, T methodInfo) where T : RemoteMethod
        {
            EnsureTenantExist(tenantId);
            var agents = this.AgentInfos[tenantId];
            //random active one
            var activeAgents = agents.Where(a => a.Status == ConnectionStatus.Connected).ToList();

            if(activeAgents.Count == 0)
            {
                throw new InvalidOperationException($"There is no active agents for tenant:{tenantId} at current");
            }

            var agentid = activeAgents.RndOne().AgentId;
            EnsureConnect();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { AgentId = agentid, Mode = DeliverMode.One, TenantId= tenantId, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to agent:{agentid},tenantId:{tenantId},methodName:{methodInfo.MethodName}");
        }

        public async Task<Result> InvokeAgentAysnc<Func,Arg,Result>(string tenantId, string agentId, Func methodInfo) where Func: RemoteInvoke<Arg,Result>
        {
            EnsureTenantAgentRelationShip(tenantId, agentId);
            EnsureConnect();
            methodInfo.ManagerId = this.ManagerId;
            methodInfo.SessionId = Guid.NewGuid().ToString();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { AgentId = agentId, Mode = DeliverMode.RPCInvoke, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"invoke to agent:{agentId},tenantId:{tenantId},methodName:{methodInfo.MethodName}");

            return await RegisterPendingTask<Func, Result>(methodInfo);
        }

        public async Task<Result> InvokeOneAgentAysnc<Func, Arg, Result>(string tenantId, Func methodInfo) where Func : RemoteInvoke<Arg, Result>
        {
            EnsureTenantExist(tenantId);
            var agents = this.AgentInfos[tenantId];
            //random active one
            var activeAgents = agents.Where(a => a.Status == ConnectionStatus.Connected).ToList();

            if (activeAgents.Count == 0)
            {
                throw new InvalidOperationException($"There is no active agents for tenant:{tenantId} at current");
            }

            var agentid = activeAgents.RndOne().AgentId;
            EnsureConnect();
            methodInfo.ManagerId = this.ManagerId;
            methodInfo.SessionId = Guid.NewGuid().ToString();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { AgentId = agentid, Mode = DeliverMode.One, TenantId = tenantId, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to agent:{agentid},tenantId:{tenantId},methodName:{methodInfo.MethodName}");

            return await RegisterPendingTask<Func, Result>(methodInfo);
        }

        public async Task<Result> InvokeOneAgentAysnc<Func, Arg, Result>(AgentInformation agent, Func methodInfo) where Func : RemoteInvoke<Arg, Result>
        {

            EnsureConnect();
            methodInfo.ManagerId = this.ManagerId;
            methodInfo.SessionId = Guid.NewGuid().ToString();
            await connection.InvokeAsync(HubMethodNames.SendMessageToAgent, new HubMethodParam() { AgentId = agent.AgentId, Mode = DeliverMode.One, TenantId = agent.TenantId, MethodName = methodInfo.MethodName }, methodInfo);
            this._logger.Info($"sent to agent:{agent.AgentId},tenantId:{agent.TenantId},methodName:{methodInfo.MethodName}");

            return await RegisterPendingTask<Func, Result>(methodInfo);
        }

        public override bool EnsureConnect(Action postAction = null)
        {
            StartHeartbeat(ManagerId);

            var result = base.EnsureConnect(()=>{ 
                this.GetAgentsInfo();
                if (postAction != null)
                {
                    postAction();
                }
            });

            return result;
        }

        public override void HandShake()
        {
            EnsureConnect();
            this._logger.Info("start handshake with server");
            connection.InvokeAsync(HubMethodNames.HandShake, this.ManagerId).Wait();
            this._logger.Info("handshake complate");
        }

        private async Task<Result> RegisterPendingTask<T, Result>(T methodInfo) where T : RemoteInvoke
        {
            //add taskcompletesource
            TaskCompletionSource<Result> completeSource = new TaskCompletionSource<Result>();
            TaskCompletionSourceHandler<Result> handler = new TaskCompletionSourceHandler<Result>(completeSource, this.Configuration.InvokeTimeout,PendingCalls,methodInfo.SessionId,this._logger);

            if (PendingCalls.TryAdd(methodInfo.SessionId, handler))
            {
                return await handler.TaskCompletionSource.Task;
            }
            else
            {
                throw new UnexpectedException($"can not add task complate source, sessionId:{methodInfo.SessionId}");
            }
        }

        private void EnsureTenantExist(string tenantId)
        {
            if(!this.AgentInfos.ContainsKey(tenantId))
            {
                throw new ArgumentOutOfRangeException($"unknown tenantId:{tenantId}");
            }
        }

        private void EnsureTenantAgentRelationShip(string tenantId, string agentId)
        {
            if (!this.AgentInfos.ContainsKey(tenantId))
            {
                throw new ArgumentOutOfRangeException($"unknown tenantId:{tenantId}");
            }

            if(!this.AgentInfos[tenantId].Exists(a=>a.AgentId == agentId))
            {
                throw new UnexpectedException($"the agentid {agentId} does not belongs to tenantId:{tenantId}");
            }

            if(this.AgentInfos[tenantId].Where(a=>a.AgentId == agentId).First().Status != ConnectionStatus.Connected)
            {
                throw new InvalidOperationException($"the agent: {agentId} does not active for now.");
            }
        }

        private void GetAgentsInfo()
        {
            EnsureConnect();
            this.AgentInfos= connection.InvokeAsync<Dictionary<string, List<AgentInformation>>>(HubMethodNames.GetAgents).Result;
        }

        #region invoke wait callback

        /// <summary>
        /// key: sessionId
        /// Value: TaskComplationSource
        /// </summary>
        private readonly ConcurrentDictionary<string, TaskCompletionSourceHandler> PendingCalls = new ConcurrentDictionary<string, TaskCompletionSourceHandler>();

        private class TaskCompletionSourceHandler
        { 
            public virtual void CompleteTaskSource(string result)
            {

            }
        }
        private class TaskCompletionSourceHandler<Result>:TaskCompletionSourceHandler,IDisposable
        {
            public readonly TaskCompletionSource<Result> TaskCompletionSource;
            private readonly CancellationTokenSource CancelToken;
            private readonly ConcurrentDictionary<string, TaskCompletionSourceHandler> PendingCalls;
            private readonly string SessionId;
            private bool ResultRecevied;
            private ILogger logger;

            public TaskCompletionSourceHandler(TaskCompletionSource<Result> task, int timeout, ConcurrentDictionary<string, TaskCompletionSourceHandler> pendingCalls, string sessionId, ILogger logger)
            {
                this.TaskCompletionSource = task;
                this.PendingCalls = pendingCalls;
                this.SessionId = sessionId;
                this.logger = logger;
                if (timeout != 0)
                {
                    this.CancelToken = new CancellationTokenSource(timeout * 1000);
                    this.CancelToken.Token.Register(() =>
                    {
                        if (!ResultRecevied)
                        {
                            bool removed = PendingCalls.TryRemove(this.SessionId, out TaskCompletionSourceHandler handler);
                            this.TaskCompletionSource.SetException(new TimeoutException($"The result does not be recevied in {timeout} seconds."));
                            logger.Info($"session time out, throw exception to caller, has removed from dic:{removed}");
                        }
                    });
                }
            }

            public void Dispose()
            {
                if(this.CancelToken != null)
                {
                    this.CancelToken.Dispose();
                }
            }

            public override void CompleteTaskSource(string result)
            {
                logger.Info($"remote call result recevied");
                ResultRecevied = true;
                var res = JsonConvert.DeserializeObject<Result>(result);
                TaskCompletionSource.SetResult(res);  
            }
        }
       
        #endregion 
    }
}

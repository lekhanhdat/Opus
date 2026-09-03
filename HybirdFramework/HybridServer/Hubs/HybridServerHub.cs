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
using HybirdProxy;
using HybirdProxy.Interface;
using HybridServer.EF;
using HybridServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonModel.Utils;
using HybridServer.Log;

namespace HybridServer.Hubs
{

    public class HybridServerHub : Hub, IHybirdHub
    {        
        private readonly SignalRRepository _repository;
        private readonly ConnectionManagementService _connectionService;
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(HybridServerHub));

        public HybridServerHub(SignalRRepository reporsitory, ConnectionManagementService service)
        {
            this._repository = reporsitory;
            this._connectionService = service;
        }

        [Authorize(APIScope.Manager)]
        public Dictionary<string, List<AgentInformation>> GetAgents()
        {
            return this._connectionService.GetAgentsGroupByTenantId();
        }

        [Authorize(APIScope.Common)]
        public async Task Heartbeat(string message)
        {
            try
            {
                await _connectionService.Heartbeat(this, message);
                this.logger.Info($"heart beat receviced: {this.Context.ConnectionId}, message: {message}");
            }
            catch (Exception e)
            {
                logger.Warn($"error occured when process heartbeat request.e:{e.ToString()}");
                throw new HubException($"error occurred when process heartbeat request, see inner exception:{e.Message}");
            }
        }

        /// <summary>
        /// In fact, this method can be considered placed in OnConnected Async instead of being called Explicitly 
        /// </summary>
        /// <param name="message"></param>
        [Authorize(APIScope.Common)]
        public async Task HandShake(string message)
        {
            try
            {
                var isNew = await _connectionService.NewConnectionComingIn(this, message);
                this.logger.Info("New handShake comes");

                if (isNew)
                {
                    await NotificateManagerAgentConnectionChangeAsync();
                }
            }
            catch(Exception e)
            {
                logger.Warn($"error occured when process hand shake request.e:{e.ToString()}");
                throw new HubException($"error occurred when process hand shake request, see inner exception:{e.Message}");
            }
        }
        /// <summary>
        /// tenant isolation is important, and plan to log every request here.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="methodInfo"></param>
        /// 
        [Authorize(APIScope.Manager)]
        public async Task SendMessageToAgentAsync(HubMethodParam param, object methodInfo)
        {
            try
            {
                if (param.Mode == DeliverMode.All)
                {
                    //get agents belong to tenantId
                    var connectionids = _connectionService.GetAgentConnectionIds(param.TenantId);
                    await this.Clients.Clients(connectionids).SendAsync(param.MethodName, methodInfo);
                    logger.Info($"sent to all agent, connectionids: {string.Join(" # ",connectionids)}");
                }
                else if(param.Mode == DeliverMode.One)
                {
                    var connectionid = _connectionService.GetAgentConnectionId(param.AgentId);
                    await this.Clients.Clients(connectionid).SendAsync(param.MethodName, methodInfo);
                    logger.Info($"sent to agent, connectionid: {connectionid}");
                }
                else if(param.Mode == DeliverMode.RPCInvoke)
                {
                    var connectionid = _connectionService.GetAgentConnectionId(param.AgentId);
                    await this.Clients.Clients(connectionid).SendAsync(param.MethodName, methodInfo);
                    logger.Info($"sent to agent, connectionid: {connectionid}");
                }
                else
                {
                    throw new UnexpectedException("invalidate mode");
                }
            }
            catch(Exception e)
            {
                logger.Warn($"error occured when send message to agent, e:{e.ToString()}");
                //log here
                throw new HubException($"error occurred when send message to agent, see inner exception:{e.Message}");

            }
            finally
            {

            }
        }

        [Authorize(APIScope.Agent)]
        public async Task SendMessageToManagerAsync(HubMethodParam param, object methodInfo)
        {
            try
            {
                var connectionids = _connectionService.GetManagerConnectionId();
                //ensure active manager
                if(connectionids.Count == 0)
                {
                    throw new UnexpectedException("there is no avaliable active manager connection");
                }

                var conn = connectionids.RndOne();
                //logger.Info($"Try to connect to client : {conn}");
                await this.Clients.Clients(conn).SendAsync(param.MethodName, methodInfo);
            }
            catch (Exception e) 
            {
                logger.Warn($"error occured when send message to manager, e:{e.ToString()}");
                throw new HubException($"error occurred when send message to manager, see inner exception:{e.Message}");
            }
            finally
            {

            }
        }

        [Authorize(APIScope.Agent)]
        public async Task SendCallbackToManagerAsync(HubMethodParam param,object result)
        {
            try
            {
                var connectionids = _connectionService.GetManagerConnectionId();
                //ensure active manager
                if (connectionids.Count == 0)
                {
                    throw new UnexpectedException("there is no avaliable active manager connection");
                }

                if(param.Mode != DeliverMode.RPCResult)
                {
                    throw new UnexpectedException("deliver mode is wrong, please check your logic");
                }

                if(string.IsNullOrEmpty(param.ManagerId))
                {
                    throw new ArgumentNullException("Manager id");
                }

                var connectionid = _connectionService.GetManagerConnectionId(param.ManagerId);

                await this.Clients.Clients(connectionid).SendAsync(HubMethodNames.AgentRPCCallback, result);
            }
            catch (Exception e)
            {
                logger.Warn($"error occured when send message to manager, e:{e.ToString()}");
                throw new HubException($"error occurred when send message to manager, see inner exception:{e.Message}");
            }
            finally
            {

            }
        }

        [Authorize(APIScope.Common)]
        public async Task UnregisterAgent(string agentId)
        {
            try
            {
                await _connectionService.UnregisterAgent(agentId);
            }
            catch(Exception e)
            {
                logger.Warn($"error occured when unregister agent, e:{e.ToString()}");
                throw new HubException($"error occurred when unregister agent, see inner exception:{e.Message}");
            }
            finally
            {

            }
        }

        #region private method
        internal async Task NotificateManagerAgentConnectionChangeAsync()
        {
            this.logger.Info("Send notification to all managers");
            //notification every manager we have new agents here! welcome!
            var connectionIds = this._connectionService.GetManagerConnectionId();
            await this.Clients.Clients(connectionIds).SendAsync(HubMethodNames.AgentConnectionNotification);
        }
        #endregion

        //public async override Task OnDisconnectedAsync(Exception exception)
        //{
        //    try
        //    {
        //        var connectionId = this.Context.ConnectionId;
        //        this.logger.Warn($"OnDisconnected event triggered, connection: {connectionId}" + exception?.ToString());
        //        var sendNotification = await _connectionService.ConnectionDisconnected(connectionId);
        //        if(sendNotification)
        //        {
        //            await NotificateManagerAgentConnectionChangeAsync();
        //        }

        //        await base.OnDisconnectedAsync(exception);
        //    }
        //    catch (Exception e)
        //    {
        //        //log here
        //        //
        //        this.logger.Warn($"error occurred when process disconnect event, e: {e.ToString()}");
        //        await Task.CompletedTask;
        //    }
        //    finally {
               
        //    }
        //}
    }
}

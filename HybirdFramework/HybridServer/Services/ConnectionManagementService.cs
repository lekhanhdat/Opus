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
using HybridServer.EF;
using HybridServer.EF.Entity;
using HybridServer.Log;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HybridServer.Services
{
    /// <summary>
    /// Need take care of performance and concurrency!
    /// Should use redis as distribution cache later if signalR server need to be scaled.
    /// performance sensitvie- does not provide log here
    /// core logic, taking care of test
    /// </summary>
    public class ConnectionManagementService
    {
        private readonly SignalRRepository _repository;
        private readonly CacheManagementService _cacheService;
        private IMemoryCache _cache;
        private TimeSpan MemoryCacheExpired = new TimeSpan(1, 0, 0);
        private readonly IInMemoryHeartbeatQueue _heartbeatQueue;
        private readonly AsyncRetryPolicy _retryPolicy;


        private readonly AveLogger logger = AveLogger.GetInstance(typeof(ConnectionManagementService));

        #region cacheKey 

        private const string AgentCacheKey = "ack";
        private const string AgentGroupByTenantCacheKey = "agbtck";
        private const string AgentConnectionMappingCacheKey = "acmck";
        private const string AgentConnectionMappingGroupByTenantCacheKey = "acmgbtck";
        private const string ConnectionAgentMappingCacheKey = "camck";

        #endregion

        #region claims values

        private const string ClaimAgentId = "agent_id";
        private const string ClaimTenantId = "realm";


        #endregion

        #region cached property

        #region manager cached property

        /// <summary>
        /// managerid-conectionid
        /// </summary>
        private Dictionary<string, string> ManagerConnectionMapping
        {
            get
            {
                return this._cacheService.GetManager();
            }
        }
        /// <summary>
        /// connectionid-managerid
        /// </summary>
        private Dictionary<string, string> ConnectionManagerMapping
        {
            get
            {
                return this.ManagerConnectionMapping.ToDictionary(p => p.Value, p => p.Key);
            }
        }

        #endregion

        /// <summary>
        /// tenantid=> agentid-agent
        /// </summary>
        private Dictionary<string, Dictionary<string,Agent>> AgentsGroupByTenant
        {
            get
            {
                this._cacheService.CheckIfNeedRefreshAgents(RefreshMemoryCacheForAgents).Wait();
               
                Dictionary<string, Dictionary<string,Agent>> temp;
                if (_cache.TryGetValue<Dictionary<string, Dictionary<string,Agent>>>(AgentGroupByTenantCacheKey, out temp))
                {
                    return temp;
                }
                else
                {
                    //ignore write multiple issue
                    temp = this.Agents.GroupBy(a=>a.TenantId).ToDictionary(g=>g.Key,g=>g.ToDictionary(a=>a.AgentId,a=>a));
                   _cache.Set(AgentGroupByTenantCacheKey, temp, MemoryCacheExpired);
                   return temp;
                }
            }
        }


        private List<Agent> Agents
        {
            get
            {
                this._cacheService.CheckIfNeedRefreshAgents(RefreshMemoryCacheForAgents).Wait();

                List<Agent> temp;
                // Level1 Get agents from Memory cache
                if(_cache.TryGetValue<List<Agent>>(AgentCacheKey, out temp))
                {
                    logger.Debug($"[CACHE LEVEL 1] HIT: Found {temp?.Count} agents in Memory.");
                    return temp;
                }
                else
                {
                    // Level2 get agents from redis
                    try
                    {
                        logger.Debug("[CACHE LEVEL 1] MISS: Not found in Memory, checking Redis...");
                        temp = _cacheService.GetAgentsFromRedis().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to get agents from Redis: " + ex.Message);
                        temp = null;
                    }
                    if (temp != null && temp.Any())
                    {
                        logger.Info($"[CACHE LEVEL 2] HIT: Found {temp.Count} agents in Redis. Syncing to Memory.");
                        _cache.Set(AgentCacheKey, temp, MemoryCacheExpired);
                        return temp;
                    }
                    //// Level3 Get agent from db
                    logger.Info("[CACHE LEVEL 2] MISS: Not found in Redis (or empty). Fallback to Database.");
                    temp = _repository.GetAgents();
                    if (temp != null && temp.Any())
                    {
                        logger.Info($"[CACHE LEVEL 3] DB LOAD: Loaded {temp.Count} agents from Database.");
                        _cache.Set(AgentCacheKey, temp, MemoryCacheExpired);
                        try
                        {
                            _cacheService.SetAgentsToRedis(temp).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Failed to set agents to Redis: " + ex.Message);
                        }

                    }
                    return temp;
                }
            }
        }     

        /// <summary>
        /// agentid-connectionid
        /// </summary>
        private Dictionary<string, string> AgentConnectionMapping
        {
            get
            {
                this._cacheService.CheckIfNeedRefreshAgents(RefreshMemoryCacheForAgents).Wait();

                Dictionary<string, string> temp;
                if (_cache.TryGetValue<Dictionary<string, string>>(AgentConnectionMappingCacheKey, out temp))
                {
                    return temp;
                }
                else
                {
                    temp = this.Agents.ToDictionary(a => a.AgentId, a => a.ConnectionId);
                    _cache.Set(AgentConnectionMappingCacheKey, temp, MemoryCacheExpired);
                    return temp;
                }
            }
        }

        private Dictionary<string, string> ConnectionAgentMapping
        {
            get
            {
                this._cacheService.CheckIfNeedRefreshAgents(RefreshMemoryCacheForAgents).Wait();

                Dictionary<string, string> temp;
                if (_cache.TryGetValue<Dictionary<string, string>>(ConnectionAgentMappingCacheKey, out temp))
                {
                    return temp;
                }
                else
                {
                    temp = this.AgentConnectionMapping.ToDictionary(p => p.Value, p => p.Key);
                    _cache.Set(ConnectionAgentMappingCacheKey, temp, MemoryCacheExpired);
                    return temp;
                }
            }
        }

        /// <summary>
        /// tenantid- connectionIds
        /// </summary>
        private Dictionary<string, List<string>> AgentConnectionMappingGroupbyTenant
        {
            get {
                this._cacheService.CheckIfNeedRefreshAgents(RefreshMemoryCacheForAgents).Wait();

                Dictionary<string, List<string>> temp;
                if (_cache.TryGetValue<Dictionary<string, List<string>>>(AgentConnectionMappingGroupByTenantCacheKey, out temp))
                {
                    return temp;
                }
                else
                {
                    temp =this.AgentsGroupByTenant.ToDictionary(p=>p.Key, p=>p.Value.Values.Select(a=>this.GetAgentConnectionId(a.AgentId)).ToList());
                    _cache.Set(AgentConnectionMappingGroupByTenantCacheKey, temp, MemoryCacheExpired);
                    return temp;
                }
            }
        }

        #endregion

        public ConnectionManagementService(SignalRRepository repository, IMemoryCache cache, CacheManagementService cacheService, IInMemoryHeartbeatQueue inMemoryHeartbeatQueue)
        {
            _repository = repository;
            _cache = cache;
            _cacheService = cacheService;
            _heartbeatQueue = inMemoryHeartbeatQueue;
            _retryPolicy = Policy.Handle<System.TimeoutException>()
                    .Or<SocketException>()
                    .Or<Exception>(ex => !(ex is ArgumentNullException || ex is ArgumentOutOfRangeException || ex is UnexpectedException))
                    .WaitAndRetryAsync(
                            retryCount: 3,
                            sleepDurationProvider: retryAttempt =>
                                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                            onRetry: (exception, timeSpan, retryCount, context) =>
                            {
                                logger.Warn($"[Transient Failure] Retry time {retryCount} after {timeSpan.TotalSeconds}s. Error: {exception.Message}");
                            });
        }

        #region get informations 

        public List<AgentInformation> GetAgents()
        {
            return this.AgentsGroupByTenant.Values.Aggregate<Dictionary<string, Agent>, List<Agent>, List<AgentInformation>>(new List<Agent>(), (seed, source) => { seed.Add(source.Values.First()); return seed; }, result => result.Select(agent => new AgentInformation() {
                AgentId = agent.AgentId,
                TenantId = agent.TenantId,
                Status = (CommonModel.DataModel.ConnectionStatus)agent.Status,
            }).ToList());
        }

        public Dictionary<string, List<AgentInformation>> GetAgentsGroupByTenantId()
        {
            return this.AgentsGroupByTenant.ToDictionary(pair => pair.Key, pair => pair.Value.Values.Select(agent => new AgentInformation()
            {
                AgentId = agent.AgentId,
                TenantId = agent.TenantId,
                Status = (CommonModel.DataModel.ConnectionStatus)agent.Status,
            }).ToList());
        }

        public List<string> GetAgentConnectionIds(string tenantId)
        {
            List<string> result;
            //use this form of calling to avoid dictionary refreshed during judgement
            bool exist = this.AgentConnectionMappingGroupbyTenant.TryGetValue(tenantId, out result);
            if (!exist)
            {
                throw new ArgumentOutOfRangeException($"tenantId not found:{tenantId}");
            }
            return result;
        }

        public string GetAgentConnectionId(string agentId)
        {
            string result;
            //use this form of calling to avoid dictionary refreshed during judgement
            bool exist = this.AgentConnectionMapping.TryGetValue(agentId, out result);
            if(!exist)
            {
                throw new ArgumentOutOfRangeException($"agentId not found:{agentId}");
            }
            return result;
        }

        public string GetManagerConnectionId(string managerId)
        {
            string result;
            //use this form of calling to avoid dictionary refreshed during judgement
            bool exist = this.ManagerConnectionMapping.TryGetValue(managerId, out result);
            if (!exist)
            {
                throw new ArgumentOutOfRangeException($"managerId not found:{managerId}");
            }
            return result;
        }

        public List<string> GetManagerConnectionId()
        {
            return this.ManagerConnectionMapping.Values.ToList();
        }

        #endregion

        #region operations

        /// <summary>
        /// This method should be sync to ensure registration process
        /// </summary>
        /// <param name="caller"></param>
        /// <returns>true if it's one new agent or manager, otherwise false</returns>
        public async Task<bool> NewConnectionComingIn(Hub caller, string managerId)
        {
            var user = caller.Context.User;

            var isManager = user.HasClaim("scope", APIScope.Manager);
            var isAgent = user.HasClaim("scope", APIScope.Agent);



            if (isManager)
            {
                if (string.IsNullOrEmpty(managerId))
                {
                    throw new ArgumentNullException("managerId");
                }
                await this._cacheService.AddManager(caller, managerId);
                return false;
            }
            else if (isAgent)
            {
                var agentId = user.FindFirst(ClaimAgentId).Value;
                var tenantId = user.FindFirst(ClaimTenantId).Value;


                bool isNew = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _repository.AddOrUpdateAgentAsync(new Agent()
                    {
                        AgentId = agentId,
                        TenantId = tenantId,
                        ConnectionId = caller.Context.ConnectionId,
                        RegistrationTime = DateTime.UtcNow,
                        LastConnected = DateTime.UtcNow,
                        Status = EF.Entity.ConnectionStatus.Connected
                    });
                });

                if (isNew)
                {
                    await this._cacheService.RaiseRefreshAgentsRequest();
                }

                return isNew;
            }
            else
            {
                throw new UnexpectedException($"unexpected role type of connection");
            }
        }

        /// <summary>
        /// logic similar to handshake
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="managerId"></param>
        public async Task Heartbeat(Hub caller, string managerId)
        {
            var user = caller.Context.User;
            var isManager = user.HasClaim("scope", APIScope.Manager);
            var isAgent = user.HasClaim("scope", APIScope.Agent);

            if (isManager)
            {
                //refresh manager id 
                await this._cacheService.RefreshManager(caller, managerId);
            }
            else if (isAgent)
            {
                var agentId = user.FindFirst(ClaimAgentId).Value;
                var tenantId = user.FindFirst(ClaimTenantId).Value;

                bool isAlreadyOnline = await _cacheService.CheckAgentOnlineAsync(agentId);
                logger.Info($"Push agent to chanel agentId: {agentId}");

                await _heartbeatQueue.EnqueueAsync(new Agent
                {
                    AgentId = agentId,
                    TenantId = tenantId,
                    ConnectionId = caller.Context.ConnectionId,
                    RegistrationTime = DateTime.UtcNow,
                    LastConnected = DateTime.UtcNow,
                    Status = EF.Entity.ConnectionStatus.Connected
                });
                //var isNew = _repository.AddOrUpdateAgentAsync(new Agent()
                //{
                //    AgentId = agentId,
                //    TenantId = tenantId,
                //    ConnectionId = caller.Context.ConnectionId,
                //    RegistrationTime = DateTime.UtcNow,
                //    LastConnected = DateTime.UtcNow,
                //    Status = EF.Entity.ConnectionStatus.Connected
                //}).Result;  // DAIKI TODO: change to async notify

                if (!isAlreadyOnline)
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        await _cacheService.SetAgentOnlineAsync(agentId);
                    });
                    this._cacheService.RaiseRefreshAgentsRequest().Wait();
                }
                //return isNew;
            }
            else
            {
                throw new UnexpectedException($"unexpected role type of connection");
            }
        }

        public async Task<bool> ConnectionDisconnected(string connectionId)
        {
            try
            {
                logger.Info($"Connection disconnected: {connectionId}");
                if (this.ConnectionManagerMapping.ContainsKey(connectionId))
                {
                    var managerId = this.ConnectionManagerMapping[connectionId];
                    logger.Info($"remove manager: managerId: {managerId}");
                    await this._cacheService.RemoveManager(managerId);
                    return false;
                }
                else if (this.ConnectionAgentMapping.ContainsKey(connectionId))
                {
                    var agentId = this.ConnectionAgentMapping[connectionId];
                    logger.Info($"remove agent: agentId: {agentId}");
                    await this._repository.AgentDisconnect(agentId);
                    await this._cacheService.RaiseRefreshAgentsRequest();

                    return true;
                }
                else
                {
                    throw new UnexpectedException($"connectionId of this connection not belong to any agent or manager when being disconnected. Connection Id:{connectionId}");
                }
            }
            catch
            {
                //for safty concern, refresh local cache here!
                RefreshMemoryCacheForAgents();
                throw;
            }
        }

        public async Task<bool> UnregisterAgent(string agentId)
        {
            //Note: might need to ensure agent not connected.
            //might need to be called through system managent token.
            throw new NotImplementedException();
        }

        #endregion

        private void RefreshMemoryCacheForAgents()
        {
            _cache.Remove(AgentCacheKey);
            _cache.Remove(AgentGroupByTenantCacheKey);
            _cache.Remove(AgentConnectionMappingCacheKey);
            _cache.Remove(ConnectionAgentMappingCacheKey);
            _cache.Remove(AgentConnectionMappingGroupByTenantCacheKey);
        }
    }
}

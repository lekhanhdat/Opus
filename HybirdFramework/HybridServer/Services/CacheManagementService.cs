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
using HybridServer.EF.Entity;
using Microsoft.AspNetCore.SignalR;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HybridServer.Services
{
    public class CacheManagementService
    {
        private readonly RedisCacheService _redis;
        private readonly TimeSpan RedisKeyExpire = new TimeSpan(0, 5, 0);
        private const string RedisManagerConection = "rmc";
        private const string RedisAgentUpdateTimestamp = "rauts";
        private const string RedisAgentOnlineKeyPrefix = "agent_online:";
        private readonly TimeSpan AgentOnlineTtl = new TimeSpan(1, 0, 0);
        private const string RedisAllAgentsKey = "all_agents_list";
        private readonly TimeSpan RedisAgentsTtl = TimeSpan.FromMinutes(30);

        public CacheManagementService(RedisCacheService redis)
        {
            this._redis = redis;
        }

        #region manager

        public async Task AddManager(Hub caller, string managerId)
        {
            await this._redis.AddHashTableAsync(RedisManagerConection, managerId, caller.Context.ConnectionId);
            await this._redis.AddAsync(this.GetManagerRedisKey(managerId), (DateTime.UtcNow + this.RedisKeyExpire).ToString(), RedisKeyExpire);
        }

        public async Task RefreshManager(Hub caller, string managerId)
        {
            //for case, Redis restarted
            await this._redis.AddHashTableAsync(RedisManagerConection, managerId, caller.Context.ConnectionId);
            await this._redis.AddAsync(this.GetManagerRedisKey(managerId), (DateTime.UtcNow + this.RedisKeyExpire).ToString(), RedisKeyExpire);
        }

        public async Task RemoveManager(string managerId)
        {
            await this._redis.DeleteHashTableItemAsync(RedisManagerConection, managerId);
            await this._redis.RemoveAsync(this.GetManagerRedisKey(managerId));
        }

        public async Task ClearManager()
        {
            var allManager = await this._redis.GetHashTableAsync(RedisManagerConection);
            allManager.Keys.ToList().ForEach(async managerId => {

                var isExpired = await IsManagerExpired(managerId);
                if (isExpired)
                {
                    //clear hashset key
                    await this._redis.DeleteHashTableItemAsync(RedisManagerConection, managerId);
                }
            });
        }

        public Dictionary<string, string> GetManager()
        {
            var allManager = this._redis.GetHashTableAsync(RedisManagerConection).Result;
            return allManager;
        }

        private async Task<bool> IsManagerExpired(string managerId)
        {
            var ExpireTime = await this._redis.GetAsync(this.GetManagerRedisKey(managerId));
            if (string.IsNullOrEmpty(ExpireTime))
            {
                //already expire
                return true;
            }
            else
            {
                return ((DateTime.UtcNow - DateTime.Parse(ExpireTime)).Ticks > 0);
            }
        }
        private string GetManagerRedisKey(string managerId)
        {
            return RedisManagerConection + managerId;
        }

        #endregion

        #region agent

        private string LastAgentRefresh { get; set; } = DateTime.MinValue.Ticks.ToString();

        public async Task RaiseRefreshAgentsRequest()
        {
            await RemoveAgentsFromRedis();
            await this._redis.AddAsync(RedisAgentUpdateTimestamp, DateTime.UtcNow.Ticks.ToString());
        }

        //public async Task FinishRefreshAgentsLocal()
        //{
        //    var timeStampString = await this._redis.GetAsync(RedisAgentUpdateTimestamp);
        //    this.LastAgentRefresh = DateTime.Parse(timeStampString);
        //}

        public async Task CheckIfNeedRefreshAgents(Action refreshAction)
        {
            var timeStampString = await this._redis.GetAsync(RedisAgentUpdateTimestamp);
            if(string.IsNullOrEmpty(timeStampString))
            {
                //redis might crash or restart
                await RaiseRefreshAgentsRequest();
                //not possible still no value after raise request
                await CheckIfNeedRefreshAgents(refreshAction);
            }
            else
            {
                if(this.LastAgentRefresh != timeStampString)
                {
                    //need refresh
                    refreshAction();
                    this.LastAgentRefresh = timeStampString;
                }
            }
        }
        #endregion

        #region agent
        public async Task<bool> CheckAgentOnlineAsync(string agentId)
        {
            var key = RedisAgentOnlineKeyPrefix + agentId;
            return await _redis.ExistsAsync(key);
        }

        public async Task SetAgentOnlineAsync(string agentId)
        {
            var key = RedisAgentOnlineKeyPrefix + agentId;
            await _redis.AddAsync(key, DateTime.UtcNow.Ticks.ToString(), AgentOnlineTtl);
        }

        public async Task<List<Agent>> GetAgentsFromRedis()
        {
            return await _redis.Get<List<Agent>>(RedisAllAgentsKey);
        }

        public async Task SetAgentsToRedis(List<Agent> agents)
        {
            await _redis.Add(RedisAllAgentsKey, agents, RedisAgentsTtl);
        }

        public async Task RemoveAgentsFromRedis()
        {
            await _redis.RemoveAsync(RedisAllAgentsKey);
        }
        #endregion
    }
}


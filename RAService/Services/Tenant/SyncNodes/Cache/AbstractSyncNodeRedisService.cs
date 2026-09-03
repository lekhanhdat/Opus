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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using AOS_SDK = Cloud.Sdk.Data.Aos.Tenant;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common.SyncNode.Compatible;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache
{
    public abstract class AbstractSyncNodeRedisService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AbstractSyncNodeRedisService));

        protected abstract string GenerateCacheKey(string tenantGroupId);
        protected abstract string GenerateFieldKeyForGroup(RMCompatibleRemoteNode aosSyncNode);
        protected abstract string GenerateFieldKeyForGroup(RemoteNodePara daoGroup);
        protected abstract List<RemoteNodePara> GetAllGroupsInDB();
        //protected abstract List<SyncRemoteNodePara> GetAllNodesInDB();

        protected abstract IEnumerable<List<SyncRemoteNodePara>> GetAllNodesInDBByPage();

        #region Get Cache
        public Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosSyncNodes)
        {
            string cacheKey = GenerateCacheKey(tenantGroupId);
            var groupFieldKeys = new HashSet<string>();
            aosSyncNodes.ForEach(node =>
            {
                string groupFieldKey = GenerateFieldKeyForGroup(node);
                if (!groupFieldKeys.Contains(groupFieldKey))
                {
                    groupFieldKeys.Add(GenerateFieldKeyForGroup(node));
                }
            });
            List<string> fieldKeyList = ConvertHashsetToList(groupFieldKeys);
            return RedisCacheService.CacheProvider.HBatchGet<RemoteNodePara>(cacheKey, fieldKeyList);
        }

        private List<string> ConvertHashsetToList(HashSet<string> hashSet)
        {
            if (hashSet == null || hashSet.Count == 0)
            {
                return new List<string>();
            }
            var list = new List<string>();
            foreach (string str in hashSet)
            {
                list.Add(str);
            }
            return list;
        }

        public Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> urls)
        {
            string cacheKey = GenerateCacheKey(tenantGroupId);
            return RedisCacheService.CacheProvider.HBatchGet<SyncRemoteNodePara>(cacheKey, urls);
        }

        private void InitGroupsCache(string tenantGroupId)
        {
            try
            {
                List<RemoteNodePara> allGroups = GetAllGroupsInDB();
                if (allGroups == null || allGroups.Count == 0)
                {
                    logger.Info("No groups should be add to redis.");
                    return;
                }
                var cacheGroupDict = new Dictionary<string, RemoteNodePara>();
                foreach (RemoteNodePara group in allGroups)
                {
                    string groupFieldKey = GenerateFieldKeyForGroup(group);
                    if (!cacheGroupDict.ContainsKey(groupFieldKey))
                    {
                        cacheGroupDict.Add(groupFieldKey, group);
                    }
                    else
                    {
                        logger.Info("Cannot be added to init group dict, because of duplicated group {0}.", groupFieldKey);
                    }
                }
                AddGroupsToCache(tenantGroupId, cacheGroupDict);
                logger.Info("Init the group cache successfully. TenantGroupId is {0}, count of groups is {1}.", tenantGroupId, allGroups.Count);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to init group caches. Tenant group id is {0}, exception is {1}.", tenantGroupId, ex.ToString());
                throw;
            }
        }

        private void InitNodesCache(string tenantGroupId)
        {
            try
            {
                foreach(var nodesInDB in GetAllNodesInDBByPage())
                {
                    if (nodesInDB == null || nodesInDB.Count == 0)
                    {
                        logger.Info("No nodes to init the database. Tenant group id is {0}.", tenantGroupId);
                        return;
                    }
                    string key = GenerateCacheKey(tenantGroupId);
                    var urlToCacheNodeDict = new Dictionary<string, SyncRemoteNodePara>();
                    foreach (SyncRemoteNodePara nodeInDB in nodesInDB)
                    {
                        urlToCacheNodeDict.Add(nodeInDB.NodeName.ToLower(), nodeInDB);
                    }
                    RedisCacheService.CacheProvider.HSet(key, urlToCacheNodeDict);
                    logger.Info("Init nodes cache successfully, TenantGroupId is {0}, count of nodes is {1}.", tenantGroupId, nodesInDB.Count);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to init nodes cache. TenantGroupId is {0}, exception is {1}.", tenantGroupId, ex.ToString());
                throw;
            }
        }
        #endregion

        #region Add Cache
        public void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> cacheGroupsDict, Action sqlExecution = null)
        {
            if (cacheGroupsDict == null || cacheGroupsDict.Count == 0)
            {
                logger.Info("No groups to add.");
                return;
            }
            string key = GenerateCacheKey(tenantGroupId);
            if (sqlExecution == null)
            {
                RedisCacheService.CacheProvider.HSet(key, cacheGroupsDict, false);
            }
            else
            {
                sqlExecution();
                RedisCacheService.CacheProvider.HSet(key, cacheGroupsDict, false);
            }
        }

        public void UpdateGroupToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> cacheNodes, Action sqlAction = null)
        {
            if (cacheNodes == null || cacheNodes.Count == 0)
            {
                logger.Info("No nodes to update.");
                return;
            }
            string key = GenerateCacheKey(tenantGroupId);
            if (sqlAction != null) 
            {
                sqlAction();
            }
            RedisCacheService.CacheProvider.HSet(key, cacheNodes, false);
        }


        public void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> cacheNodes, Action sqlAction = null)
        {
            if (cacheNodes == null || cacheNodes.Count == 0)
            {
                logger.Info("No nodes to add");
                return;
            }
            string key = GenerateCacheKey(tenantGroupId);
            if (sqlAction == null)
            {
                RedisCacheService.CacheProvider.HSet(key, cacheNodes);
            }
            else
            {
                sqlAction();
                RedisCacheService.CacheProvider.HSet(key, cacheNodes);
            }
        }
        #endregion

        #region Update
        public void UpdateNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> cacheNodes, Action sqlAction = null)
        {
            if (cacheNodes == null || cacheNodes.Count == 0)
            {
                logger.Info("No nodes to update.");
                return;
            }
            string key = GenerateCacheKey(tenantGroupId);
            if (sqlAction != null)
            {
                sqlAction();
            }
            RedisCacheService.CacheProvider.HSet(key, cacheNodes);
        }
        #endregion

        #region Delete
        public void DeleteNodesFromCache(string tenantGroupId, List<string> deleteFieldKeys, Action sqlAction, bool ignoreCase = true)
        {
            if (deleteFieldKeys == null || deleteFieldKeys.Count == 0)
            {
                logger.Error("No keys to delete.");
                return;
            }
            string key = GenerateCacheKey(tenantGroupId);
            sqlAction();
            RedisCacheService.CacheProvider.HDelWithIgnoreCase(key, deleteFieldKeys, ignoreCase);
        }
        #endregion

        public void InitCache(string tenantGroupId)
        {
            var cacheKey = GenerateCacheKey(tenantGroupId);
            bool hasKeyExisted = RedisCacheService.CacheProvider.KeyExists(cacheKey);
            if (!hasKeyExisted)
            {
                InitGroupsCache(tenantGroupId);
                InitNodesCache(tenantGroupId);
                RedisCacheService.CacheProvider.KeyExpire(cacheKey, new TimeSpan(1, 0, 0, 0));
                logger.Info("Cache {0} hasn't existed and create a new one.", cacheKey);
                return;
            }
            TimeSpan? ttl = RedisCacheService.CacheProvider.KeyTimeToLive(cacheKey);
            logger.Info($"Cache: {cacheKey}, TTL: {ttl?.ToString()}");
            if (ttl == null && hasKeyExisted)
            {// Cache Key存在但没设置过期时间，防止Cache设置过期时间时失败
                RedisCacheService.CacheProvider.KeyExpire(cacheKey, new TimeSpan(1, 0, 0, 0));
                logger.Info("Cache {0} has no TTL and extend it.", cacheKey);
                return;
            }
            if (hasKeyExisted && ttl != null && ttl.Value <= new TimeSpan(2, 0, 0))
            {// 为了防止在同步的过程中，Key消失，导致对Redis的后续操作出现一个没有TTL的Key
             // 因此如果发现TTL小于2小时，将TTL时间延长至2小时，保证本次同步能大概率完成
                RedisCacheService.CacheProvider.KeyExpire(cacheKey, new TimeSpan(2, 0, 0));
                logger.Info($"The ttl of cache {cacheKey} is less that floor and extend it.");
                return;
            }
        }
    }
}


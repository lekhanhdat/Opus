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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class RMAOSNotificationService : RMServiceBase, IRMAOSNotificationService
    {
        private const string RedisKey_RunningSRNJobsCount = "RunningSRNJobsCount";
        private const int RunningSRNJobTimeoutMinutes = 120;
        private static readonly List<int> SyncNodesMessagesTypes = new List<int>()
        {
            (int) RMAosQueueMessageType.SyncNodes, (int) RMAosQueueMessageType.DeleteNodes, (int) RMAosQueueMessageType.LastSyncMessage,
            (int)RMAosQueueMessageType.UpdateNodes,
        };
        private RALogger logger = RALogger.GetInstance(typeof(RMAOSNotificationService));

        public IRMAOSNotificationDao AOSNotificationDao { get; set; }

        public void Add(RMAosQueueMessage message)
        {
            AOSNotificationDao.Add(message);
        }

        public void Refresh(RMAosQueueMessage message)
        {
            AOSNotificationDao.Refresh(message);
        }

        public void Delete(string id)
        {
            AOSNotificationDao.Delete(id);
        }

        public void DeleteAll(string tenantId)
        {
            AOSNotificationDao.DeleteAll(tenantId);
        }

        public List<RMAosQueueMessage> GetInitNodeMessage(string tenantId)
        {
            return AOSNotificationDao.GetSyncNodeMessages(tenantId, new List<int>() { (int)RMAosQueueMessageType.InitNodes });
        }

        public List<RMAosQueueMessage> GetSyncNodeMessages(string tenantId)
        {
            return AOSNotificationDao.GetSyncNodeMessages(tenantId, SyncNodesMessagesTypes);
        }

        public RMAosQueueMessage GetSyncAOSSecurityProfileMessage(string tenantId)
        {
            return AOSNotificationDao.GetSyncAOSSecurityProfileMessage(tenantId);
        }

        public List<string> GetPendingTenants(long timePeriod)
        {
            return AOSNotificationDao.GetPendingTenants(SyncNodesMessagesTypes, timePeriod);
        }        
        
        public List<RMAosQueueMessage> GetChangeOwnerTenants()
        {
            return AOSNotificationDao.GetChangeTenantOwnerMessage();
        }

        public int GetRunningSRNJobCount()
        {
            int count = 0;
            try
            {

                var timeoutPeriod = DateTime.UtcNow.AddMinutes(-RunningSRNJobTimeoutMinutes).Ticks;
                var allRunJobTenants = RedisCacheService.CacheProvider.HGetAll<long>(RedisKey_RunningSRNJobsCount);
                foreach (var item in allRunJobTenants)
                {
                    var tenantId = item.Key;
                    var jobCreatedTime = item.Value;
                    if (timeoutPeriod > jobCreatedTime)
                    {
                        RedisCacheService.CacheProvider.HDelWithIgnoreCase(RedisKey_RunningSRNJobsCount, new List<string> { tenantId });
                        logger.Warn($"Running SRN job has been timeout. Tenant: {tenantId}, JobCreated: {new DateTime(jobCreatedTime).ToString("G")}");
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get running SRNJob Count failed. {ex}");
            }
            return count;
        }

        public void IncrementRunningSRNJobCount(string tenantId)
        {
            try
            {
                RedisCacheService.CacheProvider.HSet(RedisKey_RunningSRNJobsCount, tenantId, DateTime.UtcNow.Ticks.ToString());
            }
            catch (Exception ex)
            {
                logger.Error($"Increment running SRNJob Count failed. Tenant: {tenantId}, {ex}");
            }
        }

        public void DecrementRunningSRNJobCount(string tenantId)
        {
            try
            {
                RedisCacheService.CacheProvider.HDelWithIgnoreCase(RedisKey_RunningSRNJobsCount, new List<string> { tenantId });
            }
            catch (Exception ex)
            {
                logger.Error($"Decrement running SRNJob Count failed. Tenant: {tenantId}, {ex}");
            }
        }
    }
}
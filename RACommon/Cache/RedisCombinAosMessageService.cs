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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Cache
{
    public class RedisCombinAosMessageService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RedisCombinAosMessageService));

        private static readonly string RedisKey = "Combine_Aos_Message";

        private static readonly object Locker = new object();

        private static RedisCombinAosMessageService UniqueInstance;

        private static long LastReciverMessageTick = DateTime.UtcNow.Ticks;

        private RedisCombinAosMessageService()
        {

        }

        public static RedisCombinAosMessageService Instance
        {
            get
            {
                if(UniqueInstance == null)
                {
                    lock (Locker)
                    {
                        if(UniqueInstance == null)
                        {
                            UniqueInstance = new RedisCombinAosMessageService();
                        }
                    }
                }
                return UniqueInstance;
            }
        }

        public void AddMessage(RMAosQueueMessage message)
        {
            try
            {
                message.ReceiveMessageTime = GetReciverMessageTick();
                var messageStr = JsonConvert.SerializeObject(message);
                RedisCacheService.Redis.HashPut(RedisKey, message.QueueMessageId, messageStr);
                Logger.Info($"Add message to redis success. Message Id: {message.QueueMessageId}.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while add message: {message.QueueMessageId}. Error: {e}");
            }
        }

        public Dictionary<string, List<RMAosQueueMessage>> SeekAllMessage()
        {
            try
            {
                var messageDict = RedisCacheService.Redis.HashAll<RMAosQueueMessage>(RedisKey);
                Logger.Info($"Get message count: {messageDict.Count}.");
                return messageDict.Values.GroupBy(item => item.TenantGroupId).
                    ToDictionary(
                        item => item.Key, 
                        item => item.OrderBy(i => i.ReceiveMessageTime).ToList()
                    );
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while seek all message. Error: {e}");
            }
            return new Dictionary<string, List<RMAosQueueMessage>>();
        }

        public void DeleteMessage(RMAosQueueMessage message)
        {
            DeleteMessage(message.QueueMessageId);
        }

        public void DeleteMessage(string queueMessageId)
        {
            try
            {
                RedisCacheService.Redis.HashDelete(RedisKey, queueMessageId);
                Logger.Info($"Delete message in redis success. Message Id: {queueMessageId}");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while delete message: {queueMessageId}. Error: {e}");
            }
        }

        private long GetReciverMessageTick()
        {
            var currentReciverMessageTick = DateTime.UtcNow.Ticks;
            while (currentReciverMessageTick == LastReciverMessageTick)
            {
                Task.Delay(1).Wait();
                currentReciverMessageTick = DateTime.UtcNow.Ticks;
            }
            LastReciverMessageTick = currentReciverMessageTick;
            return currentReciverMessageTick;
        }
    }
}

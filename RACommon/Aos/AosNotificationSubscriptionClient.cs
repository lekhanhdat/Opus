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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;
using Util;
using Util.MSAzure;

namespace AvePoint.RA.Common.Aos
{
    public class AosNotificationSubscriptionClient
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(AosNotificationSubscriptionClient));
        private static readonly object locker = new object();
        private static AosNotificationSubscriptionClient subscriptionClient = null;

        #region Client Instance
        private static string TopicConnectionString => RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.AOS_SERVICE_BUS_CONNECTION_STRING];
        private static string TopicName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_SERVICE_BUS_TOPIC_NAME_PREFIX];
        private static string SubscriptionName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_SERVICE_BUS_SUBSCRIPTION_NAME_PREFIX];
        private static ServiceBusReceiver _TopicReceiver = null;
        private HashSet<string> TenantGroupIdsCache = new HashSet<string>();
        private DateTime TenantIdsCacheExpired;
        private const int TenantIdsCacheUpdatedInterval = 5;//5min
        private readonly HashSet<int> ProcessedQueueMessageTypes = new HashSet<int>() {
            (int)RMAosQueueMessageType.SyncNodes,
            (int)RMAosQueueMessageType.DeleteNodes,
            (int)RMAosQueueMessageType.SyncAOSSecurityProfile,
            (int)RMAosQueueMessageType.UpdateNodes,
            (int)RMAosQueueMessageType.ChangeTenantOwner,
            (int)RMAosQueueMessageType.InitNodes // delete node for google
        };
        private ITenantService TenantService = null;
        private int _unReceiveMessageTimes = 0;


        private AosNotificationSubscriptionClient()
        {
            TenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
            InitSubscription();

        }

        private ServiceBusReceiver TopicReceiver
        {
            get
            {
                if (_unReceiveMessageTimes > 15)
                {
                    var msgCount = GetMessageCount();
                    if (msgCount > 5000)
                    {
                        try
                        {
                            _TopicReceiver.DisposeAsync().GetAwaiter().GetResult();
                            logger.Info($"dispose topic receiver");
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"dispose topic receiver falied: {ex}");
                        }
                        _TopicReceiver = null;
                    }
                    _unReceiveMessageTimes = 0;
                }

                if (_TopicReceiver == null)
                {
                    var connStr = TopicConnectionString;
                    connStr = connStr.StartsWith("Endpoint=") ? $"{connStr?.TrimEnd(';')};EntityPath={TopicName}" : $"{TopicName}@{connStr}";
                    _TopicReceiver = ServiceBusUtil.GetClient(connStr).CreateReceiver(TopicName, SubscriptionName, new ServiceBusReceiverOptions
                    {
                        ReceiveMode = ServiceBusReceiveMode.PeekLock
                    });
                }
                return _TopicReceiver;
            }
        }

        private void InitSubscription()
        {
            string topicName = TopicName;
            string subscriptionName = SubscriptionName;
            try
            {
                var serBusClient = ServiceBusUtil.GetAdminClient(TopicConnectionString);
                if (!serBusClient.SubscriptionExistsAsync(topicName, subscriptionName).Result)
                {
                    //logger.Info($"try to create subscription: {subscriptionName} in topic: {topicName}");
                    //var options = new CreateSubscriptionOptions(topicName, subscriptionName)
                    //{
                    //    DefaultMessageTimeToLive = new TimeSpan(10, 0, 0, 0),
                    //    LockDuration = new TimeSpan(0, 1, 0),
                    //    MaxDeliveryCount = 100,
                    //    Status = Azure.Messaging.ServiceBus.Administration.EntityStatus.Active,
                    //    EnableDeadLetteringOnFilterEvaluationExceptions = true,
                    //    //EnableBatchedOperations = true
                    //};
                    //var res = serBusClient.CreateSubscriptionAsync(options).Result;
                    //logger.Info($"Create subscription success.");
                    throw new Exception($"topic or subscription not found, {topicName}, {subscriptionName}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while init subscription: {subscriptionName}, ERROR: {ex}");
            }

            GetMessageCount();
        }

        private long GetMessageCount()
        {
            try
            {
                string topicName = TopicName;
                string subscriptionName = SubscriptionName;
                var serBusClient = ServiceBusUtil.GetAdminClient(TopicConnectionString);
                var runTimeInfoTask = serBusClient.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName);

                var timeoutPoint = DateTime.UtcNow.AddMinutes(1);
                while (!runTimeInfoTask.IsCompleted)
                {
                    Thread.Sleep(1000);
                    if(timeoutPoint < DateTime.UtcNow)
                    {
                        logger.Warn($"Get message count timeout");
                        return 0;
                    }
                }

                var runTimeInfo = runTimeInfoTask.GetAwaiter().GetResult().Value;
                var msg = new StringBuilder("current message count of subscription.")
                    .Append($" TotalMessageCount: {runTimeInfo.TotalMessageCount}")
                    .Append($", ActiveMessageCount: {runTimeInfo.ActiveMessageCount}")
                    .Append($", DeadLetterMessageCount: {runTimeInfo.DeadLetterMessageCount}")
                    .Append($", TransferMessageCount: {runTimeInfo.TransferMessageCount}")
                    .Append($", TransferDeadLetterMessageCount: {runTimeInfo.TransferDeadLetterMessageCount}");
                logger.Info(msg.ToString());
                return runTimeInfo.ActiveMessageCount;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while get message count of subscription: {ex}");
            }

            return 0;
        }

        private ServiceBusReceivedMessage Receive()
        {
            do
            {
                ServiceBusReceivedMessage brokeredMessage = null;
                try
                {
                    logger.Info($"ReceiveAosMsg start");
                    using (var scope = new PerformanceScope("ReceiveAosMsg"))
                    {
                        brokeredMessage = TopicReceiver.ReceiveMessageAsync().GetAwaiter().GetResult();
                        if (brokeredMessage == null)
                        {
                            _unReceiveMessageTimes++;
                            return null;
                        }
                        _unReceiveMessageTimes = 0;
                        logger.Info($"ReceiveAosMsg received");

                        if (!IsServiceBusMessageValid(brokeredMessage))
                        {
                            CompleteMessage(brokeredMessage);
                            continue;
                        }

                        scope.AppendMessage($" {brokeredMessage.ApplicationProperties["tenantGroupId"]}|{brokeredMessage.ApplicationProperties["jobId"]}|{brokeredMessage.ApplicationProperties["messageType"]}");
                    }

                    var tenantGroupId = brokeredMessage.ApplicationProperties["tenantGroupId"].ToString();
                    if (!IsRMTenant(tenantGroupId))
                    {
                        CompleteMessage(brokeredMessage);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Receive and check msg failed. {ex}");
                    if (brokeredMessage != null)
                    {
                        CompleteMessage(brokeredMessage);
                        continue;
                    }
                }

                return brokeredMessage;
            } while (true);
        }

        private void TryUpdateTenantIdCache()
        {
            if (TenantIdsCacheExpired < DateTime.UtcNow)
            {
                TenantGroupIdsCache.Clear();
                foreach (var tenantInfo in TenantService.GetAllAvailableTenantInfo())
                {
                    TenantGroupIdsCache.Add(tenantInfo.TenantId.ToLower());
                }
                TenantIdsCacheExpired = DateTime.UtcNow.AddMinutes(TenantIdsCacheUpdatedInterval);
            }
        }

        private bool IsRMTenant(string tenantId)
        {
            bool hasExisted = true;
            if (string.IsNullOrEmpty(tenantId))
            {
                logger.Info("This tenant group id is null.");
                hasExisted = false;
            }
            TryUpdateTenantIdCache();
            ArgumentCheck.NotNull(tenantId, nameof(tenantId));
            if (!TenantGroupIdsCache.Contains(tenantId.ToLower()))
            {
                // 不存在的TenantGroup不处理
                logger.Info("Tenant group {0} is not tenant of Opus.", tenantId);
                hasExisted = false;
            }
            return hasExisted;
        }

        private bool IsServiceBusMessageValid(ServiceBusReceivedMessage message)
        {
            if (message == null)
            {
                logger.Error("The received message is null.");
                return false;
            }
            if (message.ApplicationProperties == null || message.ApplicationProperties.Count == 0)
            {
                logger.Error("The received message has no property.");
                return false;
            }
            object tenantGroupIdObj = null;
            if (!message.ApplicationProperties.TryGetValue("tenantGroupId", out tenantGroupIdObj) ||
                (string.IsNullOrEmpty((string)tenantGroupIdObj)))
            {
                logger.Error("The received message doesn't contains TenantGroupId.");
                return false;
            }
            object messageTypeObj = null;
            if (!message.ApplicationProperties.TryGetValue("messageType", out messageTypeObj))
            {
                logger.Error("The received message doesn't contains MessageType.");
                return false;
            }
            int messageType = 0;
            if (!int.TryParse(messageTypeObj?.ToString(), out messageType) || !ProcessedQueueMessageTypes.Contains(messageType))
            {
                logger.Warn("The {0} is out of TenantQueueMessageType", messageTypeObj?.ToString());
                return false;
            }
            object jobIdObg = null;
            if (!message.ApplicationProperties.TryGetValue("jobId", out jobIdObg))
            {
                logger.Error("The received message doesn't contains JobId.");
                return false;
            }
            return true;
        }

        #endregion

        private static AosNotificationSubscriptionClient Instance
        {
            get
            {
                if (subscriptionClient == null)
                {
                    lock (locker)
                    {
                        if (subscriptionClient == null)
                        {
                            subscriptionClient = new AosNotificationSubscriptionClient();
                        }
                    }
                }
                return subscriptionClient;
            }
        }

        #region public static functions

        private void CompleteMessage(ServiceBusReceivedMessage message)
        {
            logger.Info($"CompleteAosMsg start");
            using (var scope = new PerformanceScope("CompleteAosMsg"))
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(25000);
                    var task = TopicReceiver.CompleteMessageAsync(message);
                    if (!task.Wait(30000))
                    {
                        scope.AppendMessage(" Operation timeout.");
                        if (task.IsCanceled)
                        {
                            try
                            {
                                task.Dispose();
                                scope.AppendMessage(" Dispose task.");
                            }
                            catch (Exception ex)
                            {
                                scope.AppendMessage($" Dispose task error: {ex}");
                            }
                        }
                    }
                }
            }
        }

        public static void Process(Action<RMAosQueueMessage> processMsg)
        {
            if(string.IsNullOrEmpty(TopicConnectionString))
            {
                logger.Error($"AOS_SERVICE_BUS_CONNECTION_STRING is empty.");
                return;
            }

            if (subscriptionClient != null)
            {
                logger.Warn($"The Process method is allready executed");
                return;
            }

            while (true)
            {
                ServiceBusReceivedMessage brokeredMessage = null;
                try
                {
                    brokeredMessage = Instance.Receive();
                    if (brokeredMessage != null)
                    {
                        using (var scope = new PerformanceScope("SaveAosMsg"))
                        {
                            processMsg(ConvertServiceBusMessageToQueueMessage(brokeredMessage));
                        }

                        Instance.CompleteMessage(brokeredMessage);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Process message failed: {ex}");
                    Thread.Sleep(5 * 1000);
                }
            }
        }

        #endregion


        #region Message Convertor
        private static RMAosQueueMessage ConvertServiceBusMessageToQueueMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            int messageTypeValue = int.Parse(serviceBusMessage.ApplicationProperties["messageType"].ToString());
            RMAosQueueMessageType messageType = (RMAosQueueMessageType)messageTypeValue;
            var queueMsg = new RMAosQueueMessage()
            {
                QueueMessageId = Guid.NewGuid().ToString(),
                MessageType = messageType,
                TenantGroupId = serviceBusMessage.ApplicationProperties["tenantGroupId"].ToString(),
                ServiceBusMessageId = serviceBusMessage.MessageId,
            };
            switch (messageType)
            {
                case RMAosQueueMessageType.ExtendPhysicalDevice:
                    {
                        queueMsg.ExtendPhysicalDeviceMessage = ConvertToExtendPhysicalDeviceMessage(serviceBusMessage);
                    }
                    break;
                case RMAosQueueMessageType.SyncNodes:
                    {
                        queueMsg.SyncNodesMessage = ConvertToSyncRemoteNodesMessage(serviceBusMessage);
                    }
                    break;
                case RMAosQueueMessageType.InitNodes:
                    {
                        queueMsg.MessageType = RMAosQueueMessageType.DeleteNodes;
                        queueMsg.DeleteNodesMessage = ConvertToDeleteRemoteNodesMessage(serviceBusMessage);
                    }
                    break;
                case RMAosQueueMessageType.DeleteNodes:
                    {
                        queueMsg.DeleteNodesMessage = ConvertToDeleteRemoteNodesMessage(serviceBusMessage);
                    }
                    break;
                case RMAosQueueMessageType.SyncAOSSecurityProfile:
                    {
                        queueMsg.SyncAOSSecurityProfileMessage = ConvertToSyncAOSSecurityProfileMessage(serviceBusMessage);
                    }
                    break;
                case RMAosQueueMessageType.SyncServiceAccount:
                    {
                        queueMsg.SyncServiceAccountMessage = ConvertToSyncServiceAccountMessage(serviceBusMessage);
                    }
                    break;
                default:
                    {
                        string errorMsg = string.Format("Mesage type is out of range. Message type is {0}.", messageType);
                        logger.Warn(errorMsg);
                        break;
                    }
            }
            if (queueMsg.IsLastSyncJob)
            {
                queueMsg.MessageType = RMAosQueueMessageType.LastSyncMessage;
            }
            return queueMsg;
        }

        private static ExtendPhysicalDeviceMessage ConvertToExtendPhysicalDeviceMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            return new ExtendPhysicalDeviceMessage()
            {
                JobId = serviceBusMessage.ApplicationProperties["jobId"].ToString(),
            };
        }

        private static SyncNodesMessage ConvertToSyncRemoteNodesMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            return new SyncNodesMessage()
            {
                Content = GetMessage<RemoteNodesMessageModel>(serviceBusMessage).Convert(),
            };
        }

        private static DeleteNodesMessage ConvertToDeleteRemoteNodesMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            return new DeleteNodesMessage()
            {
                Content = GetMessage<RemoteNodesMessageModel>(serviceBusMessage).Convert(),
            };
        }

        private static SyncAOSSecurityProfileMessage ConvertToSyncAOSSecurityProfileMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            return new SyncAOSSecurityProfileMessage()
            {
                JobId = serviceBusMessage.ApplicationProperties["jobId"].ToString(),
                Content = GetMessage<ApplyKeyVaultMessage>(serviceBusMessage),
            };
        }

        private static SyncServiceAccountMessage ConvertToSyncServiceAccountMessage(ServiceBusReceivedMessage serviceBusMessage)
        {
            return new SyncServiceAccountMessage()
            {
                Content = GetMessage<ServiceAccountMessageModel>(serviceBusMessage).Convert(),
            };
        }

        private static T GetMessage<T>(ServiceBusReceivedMessage message)
        {
            if (message != null)
            {
                string jsonContent = null;
                try
                {
                    jsonContent = message.Body.ToString();
                    return JsonConvert.DeserializeObject<T>(jsonContent);
                }
                catch (Exception e)
                {
                    logger.Error("Deserialize message failed, Content: {0}, Error: {1}", jsonContent, e.ToString());
                }
            }
            return default(T);
        }
        #endregion
    }
}

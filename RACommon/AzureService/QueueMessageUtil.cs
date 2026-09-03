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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CloudService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Azure.Messaging.ServiceBus;
using Util.MSAzure;
using Azure.Messaging.ServiceBus.Administration;
using Newtonsoft.Json;

namespace AvePoint.RA.Common.AzureService
{
    /// <summary>
    /// Please create the instance from QueueMessageUtilFactory
    /// </summary>
    public class QueueMessageUtil
    {
        private string _connectionString;
        private string _queueName;
        private ServiceBusSender _sender;
        private ServiceBusReceiver _receiver;
        private static readonly RALogger logger = RALogger.GetInstance(typeof(QueueMessageUtil));

        /// <summary>
        /// Inavoid of creating the multiple underlying connection, please do not create a new QueueMessageUtil instance directly, but use QueueMessageUtilFactory.GetUtil method instead.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="queueName"></param>
        public QueueMessageUtil(string connectionString, string queueName)
        {
            _queueName = queueName.ToLower();
            _connectionString = connectionString.StartsWith("Endpoint=") ? $"{connectionString?.TrimEnd(';')};EntityPath={_queueName}" : $"{_queueName}@{connectionString}";
            InternalInit();
        }

        private ServiceBusSender Sender
        {
            get
            {
                if (_sender == null)
                {
                    _sender = ServiceBusUtil.CreateSender(_connectionString);
                }
                return _sender;
            }
        }

        private ServiceBusReceiver Receiver
        {
            get
            {
                if (_receiver == null)
                {
                    _receiver = ServiceBusUtil.GetClient(_connectionString).CreateReceiver(_queueName, new ServiceBusReceiverOptions
                    {
                        ReceiveMode = ServiceBusReceiveMode.PeekLock
                    });
                    //_receiver = ServiceBusUtil.GetClient(_connectionString, ServiceBusReceiveMode.PeekLock);
                }
                return _receiver;
            }
        }

        private void InternalInit()
        {
            logger.Info("begin internal init");
            try
            {
                var serBusClient = ServiceBusUtil.GetAdminClient(_connectionString);
                if (!serBusClient.QueueExistsAsync(_queueName).Result)
                {
                    logger.Info($"try to create queue. queue name:  {_queueName}");
                    var options = new CreateQueueOptions(_queueName)
                    {
                        MaxSizeInMegabytes = 5120,
                        DefaultMessageTimeToLive = new TimeSpan(1, 0, 0, 0),
                        MaxDeliveryCount = 2000,
                    };
                    var res = serBusClient.CreateQueueAsync(options).Result;
                    logger.Info($"Create queue success. queue name:  {_queueName}");
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while create service bus queue {0}, ERROR:{1}", _queueName, ex.ToString());
                throw new ServiceBusException("Service bus connection string is invalid");
            }
        }

        #region Send Message

        public bool SendMessage<T>(T body, IDictionary<string, object> settings = null)
        {
            try
            {
                var message = AssembleMessage(body, settings);
                Sender.SendMessageAsync(message).Wait();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("send message throw error: {0}", ex.ToString());
                throw;
            }
        }

        private ServiceBusMessage AssembleMessage<T>(T body, IDictionary<string, object> settings = null)
        {
            var message = GetServiceBusMessageWithBody(body);
            if (settings != null && settings.Any())
            {
                foreach (var setting in settings)
                {
                    logger.Debug("set message property:" + setting.Key);
                    message.ApplicationProperties[setting.Key] = setting.Value;
                }
            }
            return message;
        }

        private ServiceBusMessage GetServiceBusMessageWithBody(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var message = new ServiceBusMessage(json);
            message.ContentType = "Application/Json";
            return message;
        }
        #endregion

        #region Receive Message
        /// <summary>
        /// Call this method will block until get an message
        /// </summary>
        public T ReceiveMessage<T>(Func<T, QueueMessageAction> validator = null) where T: class
        {
            logger.Debug("begin to receive message");
            T queueMessage = default(T);

            try
            {
                var receiver = Receiver;
                var message = receiver.ReceiveMessageAsync(TimeSpan.MaxValue).Result;
                if (message != null)
                {
                    logger.Debug("begin to deserialize message");
                    string jsonContent = null;
                    try
                    {
                        jsonContent = message.Body.ToString();
                        queueMessage = JsonConvert.DeserializeObject<T>(jsonContent);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Deserialize message failed, Content: {0}, Error: {1}", jsonContent, e.ToString());
                    }

                    if (queueMessage == null || validator == null)
                    {
                        receiver.CompleteMessageAsync(message).Wait();
                    }
                    else
                    {
                        var validateResult = validator(queueMessage);
                        switch (validateResult)
                        {
                            case QueueMessageAction.Receive:
                                receiver.CompleteMessageAsync(message).Wait();
                                break;
                            case QueueMessageAction.Drop:
                                receiver.CompleteMessageAsync(message).Wait();
                                break;
                            case QueueMessageAction.Abandon:
                                receiver.AbandonMessageAsync(message).Wait();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Receive message failed,ERROR:{0}" + ex.ToString());
            }

            return queueMessage;
        }

        /// <summary>
        /// Call this method will block until get an message
        /// </summary>
        public T ReceiveMessageWithRetry<T>(Func<T, QueueMessageAction> validator = null) where T : class  //Quality Issue
        {
            while (true)
            {
                try
                {
                    return ReceiveMessage(validator);
                }
                catch (Exception ex)
                {
                    if (ShouldRetryReceiveMessage(ex))
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                    throw;
                }
            }
        }

        private static bool ShouldRetryReceiveMessage(Exception e)
        {
            return e is TimeoutException
                || e is UnauthorizedAccessException;
                //|| e is MessagingEntityNotFoundException
                //|| e is MessagingException;
        }

        #endregion

        [Serializable]
        private class ServiceBusException : Exception
        {
            public ServiceBusException(string message)
                : base(message)
            {

            }
        }
    }
}

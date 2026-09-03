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


using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Utility;
using System.Threading;
using AvePoint.GCommon.Transfer.MQ.Channel;
using AvePoint.GCommon.Transfer.MQ.Interface;
using AvePoint.GCommon;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// Client给每个Service发送消息时，需要缓存Channel，并且要保持Alive，这样才能保证Callback好使
    /// 如果不需要Callback或者不支持Callback，就需要使用Poll线程来定时获取消息。
    /// </summary>
    internal class AveMQClient2ServerPeer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMQClient2ServerPeer), false);

        #region Private Fields
        private Object syncObj = new Object();
        private AveMQClient client;
        private AveMQServer server;
        private string host = string.Empty;
        private int port = 0;
        private string relatedBaseUri = string.Empty;
        private string jobId = string.Empty;
        private IMessageChannel messageChannel;
        private AveThreadWrapper keepAliveThread;
        private AveThreadWrapper pollMessageThread;
        private int connectionTimeout = DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.MaxReconnectionTimeOut;
        private DateTime lastReconnectionSuccessTime = DateTime.MinValue;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private bool isConnected = false;
        ///// <summary>
        ///// 是否已经disposed
        ///// </summary>
        //private bool isDisposed = false;

        /// <summary>
        /// 需要CallBack请求。
        /// </summary>
        private bool requireCallBack = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// 单位为 MilliSecond
        /// 此属性仅为Connect时的Time Out，注意区分Message的Time Out
        /// </summary>
        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
            set { connectionTimeout = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 主要是为WCF类型通信提供一些基本信息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout">在建立连接时的TimeOut，默认为AveMQConfigure.MaxReconnectionTimeOut</param>
        public AveMQClient2ServerPeer(AveMQClient client, string host, int port, string relatedBaseUri, string jobId, int connectionTimeout)
            : this(client, host, port, relatedBaseUri, jobId, connectionTimeout, true)
        { }

        /// <summary>
        /// 内部构造函数，可以提供是否需要callBack，外围不需要调用该函数
        /// </summary>
        /// <param name="client"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="requireCallback"></param>
        internal AveMQClient2ServerPeer(AveMQClient client, string host, int port, string relatedBaseUri, string jobId, int connectionTimeout, bool requireCallback)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }
            this.client = client;
            this.host = host;
            this.port = port;
            this.relatedBaseUri = relatedBaseUri;
            this.jobId = jobId;
            this.connectionTimeout = connectionTimeout;
            this.requireCallBack = requireCallback;
            //Init(requireCallback);
        }

        /// <summary>
        /// InProcess Level的Client2ServerPeer
        /// </summary>
        /// <param name="client"></param>
        /// <param name="server"></param>
        public AveMQClient2ServerPeer(AveMQClient client, AveMQServer server)
        {
            this.client = client;
            this.server = server;
            requireCallBack = true;
            //Init(true);
        }

        /// <summary>
        /// 提供方法来保证Connection
        /// </summary>
        /// <returns>如果start成功，则返回true，如果不成功，则会抛出异常</returns>
        public bool Start()
        {
            if (!isConnected)
            {
                lock (syncObj)
                {
                    if (!isConnected)
                    {
                        isConnected = Init(requireCallBack);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 为外界提供方法，判断Client2ServerPeer是否为同一个Host/Port/RelatedBaseUri/JobId。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool IsMatch(string host, int port, string relatedBaseUri, string jobId)
        {
            return this.host.Equals(host, StringComparison.OrdinalIgnoreCase) &&
                this.port == port && this.relatedBaseUri.Equals(relatedBaseUri, StringComparison.OrdinalIgnoreCase) &&
                this.jobId.Equals(jobId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 通过MessageChannel发送消息给目的端，也可能是进程内MQServer。
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(AveMessage message, int msgTimeout = 30 * 1000)
        {
            DateTime endTime = DateTime.UtcNow.AddMilliseconds(msgTimeout);
            //string messageContent = null;

            while (true)
            {
                try
                {
                    messageChannel.SendMessage(message);
                    break;
                }
                catch (Exception e)
                {
                    if (endTime < DateTime.UtcNow)
                    {
                        throw new Exception(string.Format("Send message timeout,sessionId:{0},receiver:{1},timeout:{2},error:{3}", message.SessionId, message.Receiver, msgTimeout, e.ToString()));
                    }

                    logger.Error("Current Message can not send by MQ logic, error information: {0}. We will try to reconnect and resend the message.", e.ToString());

                    //if (messageContent == null)
                    //{
                    //    messageContent = message.GetDataString();
                    //    logger.Error("Current Message can not send by MQ logic, Message content:{0},ErrorInformation:{1}. we will try to reconnect and resend the message.", messageContent, e.ToString());
                    //}
                    //else
                    //{
                    //    if (messageContent.Length > 64 * 1024)
                    //    {
                    //        logger.Error("Current Message can not send by MQ logic, Message content:{0} ...,ErrorInformation:{1}. we will try to reconnect and resend the message.", messageContent.Substring(0, 10240), e.ToString());
                    //    }
                    //    else
                    //    {
                    //        logger.Error("Current Message can not send by MQ logic, Message content:{0},ErrorInformation:{1}. we will try to reconnect and resend the message.", messageContent, e.ToString());
                    //    }
                    //}

                    //ReopenConnectionWithLock();
                }
                //retryCount++;
                Thread.Sleep(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.NoReconnectTimeOut * 1000);//MQ重连成功结束之后一定时间内不会重连，所以避免失误，这里需要等待相应的时间，确保下次判断的时候可以重连
            }
            //}
        }
        /// <summary>
        /// 停止当前的MQ服务：停止所有的监听线程，防止由于目的端进程退出导致的远程服务调用失败的错误。
        /// </summary>
        /// <param name="isCloseChannel">
        /// 控制关闭处理线程的时候是否关闭通讯信道
        /// 因为可能需要保留通道发送最后一个消息，例如：关闭Restore进程的消息。
        /// </param>
        public void Stop(Boolean isCloseChannel)
        {
            //停止工作线程
            if (pollMessageThread != null)
            {
                pollMessageThread.SafeStop(10000, "close client MQ thread of PoolMessage.");
                pollMessageThread = null;
            }
            if (keepAliveThread != null)
            {
                keepAliveThread.SafeStop(10000, "close client MQ thread of keeping alive.");
                keepAliveThread = null;
            }
            //关闭信道
            if (isCloseChannel && messageChannel != null)
            {
                try
                {
                    logger.Debug("stop the message channel --> {0}:{1}/{2}/{3}", this.host, this.port, this.relatedBaseUri, this.jobId);
                    messageChannel.Close();
                }
                catch (Exception e)
                {
                    logger.Warn("stop MQClient2ServerPeer error.", e);
                }
            }
            isConnected = false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 内部初始化，如果是mHost为空，则为InProcess Level的Channel，否则为WCF的Channel
        /// 如果为远程Channel，需要开启KeepAlive线程或者PollMessage线程。
        /// </summary>
        /// <param name="requireCallback">是否需要回调，如果不需要，则不会开启KeepAlive线程</param>
        private bool Init(bool requireCallback)
        {
            if (string.IsNullOrEmpty(host))
            {
                messageChannel = MessageChannelFactory.GetInProcessMessageChannel(server, new AveMQClientCallback(client));

                ReopenConnection();
            }
            else
            {
                if (DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.IsOneWayConnection)
                {
                    messageChannel = MessageChannelFactory.GetMessageChannel(host, port, relatedBaseUri, jobId, null, client.EnableSsl);
                }
                else
                {
                    messageChannel = MessageChannelFactory.GetMessageChannel(host, port, relatedBaseUri, jobId, new AveMQClientCallback(client), client.EnableSsl);
                }

                ReopenConnection();

                if (requireCallback)
                {
                    if (DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.IsOneWayConnection)
                    {
                        pollMessageThread = AveThreadUtility.StartThread(PollMessage, string.Format("MQClient2Server Poll Message_{0}_{1}", this.client.Identifier, this.client.SessionId), "AveMQClient2ServerPeer");
                    }
                    else
                    {
                        keepAliveThread = AveThreadUtility.StartThread(KeepAlive, string.Format("MQClient2Server Keep Alive_{0}_{1}", this.client.Identifier, this.client.SessionId), "AveMQClient2ServerPeer");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 每隔一定时间和目的端保持KeepAlive，保证目的端的Callback好使。
        /// </summary>
        private void KeepAlive()
        {
            AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            while (currentThreadWrapper.KeepRunning)
            {
                Thread.Sleep(5000);
                try
                {
                    messageChannel.KeepAlive();
                }
                catch (Exception ex)
                {
                    //logger.Error(ex.ToString());//TODO
                    logger.Error("Keep alive failed,host:{0},port:{1},error:{2}", this.host, this.port, ex.ToString());
                    try
                    {
                        ReopenConnection();
                    }
                    catch (Exception e)
                    {
                        logger.Error("Reopen connection timeout,we will continue to reopen,host:{0},port:{1},timeout:{2},error:{3}", this.host, this.port, this.connectionTimeout, e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 如果不支持Callback，就需要改变配置文件，让源端主动去目的端获取
        /// </summary>
        private void PollMessage()
        {
            AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            while (currentThreadWrapper.KeepRunning)
            {
                try
                {
                    AveMessage msg = null;

                    if (messageChannel.ReceiveMessage(out msg))
                    {
                        client.PutMessage(msg);
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());//TODO
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// 包含重连机制，TODO_Long以后需要加一些锁机制，不能一个段了就需要重连，需要一个规则
        /// </summary>
        private void ReopenConnection()
        {
            //int retryTimes = AveMQConfigure.MaxRetryTimes;
            logger.Debug("MQ reconnection begin. reconnection MQ information:SessionId:{0},Identifier:{1}", client.SessionId, client.Identifier);
            DateTime ReconnectionTimeOut = DateTime.UtcNow.AddMilliseconds(ConnectionTimeout);
            AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            while (true)
            {
                if (currentThreadWrapper != null && (!currentThreadWrapper.KeepRunning))
                {
                    break;
                }
                try
                {
                    string errorMsg = string.Empty;
                    if (!messageChannel.Open(client.SessionId, client.Identifier, out errorMsg))
                    {
                        throw new Exception(string.Format("Cannot open connection with host:{0} port:{1}, error message:{2}", host, port, errorMsg));
                    }
                    lastReconnectionSuccessTime = DateTime.Now;
                    logger.Info("MQ reconnection success, host:{0}, port:{1}", this.host, this.port);
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());//TODO add retry times...
                    if (DateTime.UtcNow > ReconnectionTimeOut)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.ReconnectionTime * 1000);
                    }
                }

            }
        }

        /// <summary>
        /// 重练机制带锁
        /// </summary>
        private void ReopenConnectionWithLock()
        {
            lock (syncObj)
            {
                if (lastReconnectionSuccessTime.AddSeconds(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.NoReconnectTimeOut) > DateTime.Now)
                {
                    Thread.Sleep(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.NoReconnectTimeOut * 1000);
                }

                ReopenConnection();
            }
        }
        #endregion
    }
}

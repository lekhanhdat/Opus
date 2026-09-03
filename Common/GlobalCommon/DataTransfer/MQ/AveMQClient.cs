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
using System.Threading;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// Client端使用的对象。
    /// </summary>
    public class AveMQClient
    {
        #region Private Fields
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveMQClient), false);

        private string sessionId = string.Empty;
        private string identifier = string.Empty;
        private LinkedList<AveMQClient2ServerPeer> clientPeers = new LinkedList<AveMQClient2ServerPeer>();
        private SynchronizedLinkedList<AveMessage> receivedMessages = new SynchronizedLinkedList<AveMessage>();
        private AveThreadWrapper callbackThread;
        private int connectionTimeout = AveMQConfigure.MaxReconnectionTimeOut;
        private AveMQClient2ServerPeer inProcessPeer;
        #endregion

        #region Public Fields
        /// <summary>
        /// 一般都是JobId
        /// </summary>
        public string SessionId
        {
            get { return sessionId; }
        }
        /// <summary>
        /// 消息接收者的身份
        /// </summary>
        public string Identifier
        {
            get { return identifier; }
        }
        /// <summary>
        /// 重连时需要的时间
        /// </summary>
        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
            set { connectionTimeout = value; }
        }
        #endregion

        public delegate void MessageReceiver(AveMessage msg);
        public delegate void ReConnectFailedMonitor(string errorMsg);
        public event MessageReceiver MessageReceivers;
        public event ReConnectFailedMonitor ReConnectFailedMonitors;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId">please use job id</param>
        /// <param name="identifier"></param>
        public AveMQClient(string sessionId, string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new Exception("Please make sure the identifier is not empty.");
            }

            this.sessionId = sessionId;
            this.identifier = identifier;
        }

        /// <summary>
        /// 开启CallbackThread，所以需要给消息处理委托赋值。
        /// 默认注册一个InProcess Level的Client2ServerPeer
        /// </summary>
        public void Start()
        {
            Start(CallbackThread);
        }

        /// <summary>
        /// 外围提供获取消息的方式
        /// </summary>
        /// <param name="callbackThread"></param>
        public void Start(ThreadStart callbackThread)
        {
            this.callbackThread = AveThreadUtility.StartThread(callbackThread, "Callback Thread", "MQClient");
            //为了进程内通信
            inProcessPeer = new AveMQClient2ServerPeer(this, AveMQServer.GetInstance());
            lock (clientPeers)
            {
                clientPeers.AddLast(inProcessPeer);
            }
            inProcessPeer.Start();
        }

        /// <summary>
        /// 根据提供的信息获得对应的MQClient对象，将其关闭。
        /// 该方法必须放置在关闭目的段进程的命令之前被调用，
        /// 因为目的段进程退出，那么它host的MQservice也将不存在，
        /// 导致调用keep alive调用反复出现channel exception。
        /// </summary>
        public void Stop(string host, int port, string relatedBaseUri, string jobId)
        {
            AveMQClient2ServerPeer currMQClient = GetMQClient2ServerPeer(host, port, relatedBaseUri, jobId, false);
            if (currMQClient != null)
                currMQClient.Stop(true);
        }
        /// <summary>
        /// 停止相关的工作线程，但是保证消息通道可用，可以正常发送
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        public void StopThread(string host, int port, string relatedBaseUri, string jobId)
        {
            AveMQClient2ServerPeer currMQClient = GetMQClient2ServerPeer(host, port, relatedBaseUri, jobId, false);
            if (currMQClient != null)
                currMQClient.Stop(false);
        }

        /// <summary>
        /// 需要关闭所有的Channel
        /// </summary>
        public void Stop()
        {
            lock (clientPeers)
            {
                foreach (var client in clientPeers)
                {
                    client.Stop(true);
                }
                clientPeers.Clear();
            }
        }

        /// <summary>
        /// 用于目的端反馈消息给源端时，需要将消息缓存到MQServer的cache中，如果支持Callback，则直接调用Callback
        /// 反馈给源端，如果不支持CallBack，源端会定时来取Message。
        /// 
        /// Note:如果自己开启WCF服务或者没有Callback，请禁止使用该函数，否则消息发送不出去。
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(AveMessage msg, int msgTimeout = 0)
        {
            //AveMQServer.GetInstance().PutMessage(msg);
            //SendMessage(string.Empty, 0, string.Empty, string.Empty, msg, msgTimeout);
            if (inProcessPeer == null)
            {
                throw new ArgumentNullException("inProcessPeer");
            }
            inProcessPeer.SendMessage(msg, msgTimeout);
        }

        /// <summary>
        /// 只能在源端调用该函数，目的端部署WCF的host，这样才能把消息发送给目的端。
        /// 另外，发送消息是阻塞的，因为产品中大多数模块都是发送完消息才可以进行下一步
        /// 如果需要异步处理，请和Common组联系。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="msg"></param>
        public void SendMessage(string host, int port, string relatedBaseUri, AveMessage msg, int msgTimeout = 0)
        {
            SendMessage(host, port, relatedBaseUri, sessionId, msg, msgTimeout);
        }

        /// <summary>
        /// 在发送消息的基础上，可以对新建的Client2ServerPeer进行Timeout初始化工作。
        /// 
        /// 只能在源端调用该函数，目的端部署WCF的host，这样才能把消息发送给目的端。
        /// 另外，发送消息是阻塞的，因为产品中大多数模块都是发送完消息才可以进行下一步
        /// 如果需要异步处理，请和Common组联系。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="msg"></param>
        /// <param name="timeout">单位为millisecond, 默认为无time out如果Send失败会无限重试</param>
        public void SendMessage(string host, int port, string relatedBaseUri, string jobId, AveMessage msg, int msgTimeout = 30 * 1000)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }
            GetMQClient2ServerPeer(host, port, relatedBaseUri, jobId).SendMessage(msg, msgTimeout);
        }

        /// <summary>
        /// 添加方法可以改变内部Connection的Timeout
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout"></param>
        public void ChangeConnectionTimeout(string host, int port, string relatedBaseUri, string jobId, int connectionTimeout)
        {
            var client = GetMQClient2ServerPeer(host, port, relatedBaseUri, jobId, false);
            if (client != null)
            {
                client.ConnectionTimeout = connectionTimeout;
            }
        }

        /// <summary>
        /// 外部可以先调用Connect方法，然后再调用发送方法
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout"></param>
        public void ConnectMQServer(string host, int port, string relatedBaseUri, string jobId, int connectionTimeout)
        {
            this.connectionTimeout = connectionTimeout;
            var client2ServerPeer = GetMQClient2ServerPeer(host, port, relatedBaseUri, jobId, true);
        }

        /// <summary>
        /// 将消息放到接收队列中
        /// </summary>
        /// <param name="msg"></param>
        internal void PutMessage(AveMessage msg)
        {
            receivedMessages.AddLast(msg);
        }

        /// <summary>
        /// 获取Message
        /// </summary>
        /// <returns></returns>
        public AveMessage GetMessage()
        {
            AveMessage msg = null;
            receivedMessages.TryGetFirst(out msg);
            return msg;
        }

        #region Private Methods
        /// <summary>
        /// 获取Client2ServerPeer对象，用于管理给不同Server发送消息的Channel。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout">在建立连接时的Time Out，单位为MilliSecond</param>
        /// <returns></returns>
        private AveMQClient2ServerPeer GetMQClient2ServerPeer(string host, int port, string relatedBaseUri, string jobId, bool createPeerIfNotExist = true)
        {
            AveMQClient2ServerPeer client2ServerPeer = null;

            lock (clientPeers)
            {
                foreach (AveMQClient2ServerPeer peer in clientPeers)
                {
                    if (peer.IsMatch(host, port, relatedBaseUri, jobId))
                    {
                        client2ServerPeer = peer;
                        client2ServerPeer.ConnectionTimeout = connectionTimeout;
                        break;
                    }
                }

                if (client2ServerPeer == null && createPeerIfNotExist)
                {
                    client2ServerPeer = new AveMQClient2ServerPeer(this, host, port, relatedBaseUri, jobId, connectionTimeout);
                    clientPeers.AddLast(client2ServerPeer);
                }
            }

            if (client2ServerPeer != null)
            {
                client2ServerPeer.Start();
            }
            return client2ServerPeer;
        }

        /// <summary>
        /// 循环遍历消息，然后调用消息处理委托，让外围去处理消息内容。
        /// </summary>
        private void CallbackThread()
        {
            AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            while (currentThreadWrapper.KeepRunning)
            {
                AveMessage msg = null;
                if (MessageReceivers != null && receivedMessages.TryGetFirst(out msg))
                {
                    AveThreadPoolRunner.RunThread(new InnerMessageCallbackWorker(msg, MessageReceivers));
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// 静态方法，发送消息给一个进程
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="identifier"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendMessage(string host, int port, string relatedBaseUri, string jobId, string identifier, int connectionTimeout, AveMessage msg, int msgTimeout = 30 * 1000)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }
            bool sendSuccessfully = false;
            AveMQClient client = null;
            try
            {
                client = new AveMQClient(jobId, identifier);
                client.ConnectionTimeout = connectionTimeout;
                client.SendMessage(host, port, relatedBaseUri, jobId, msg, msgTimeout);
                sendSuccessfully = true;
            }
            catch (Exception ex)
            {
                sendSuccessfully = false;
                logger.Error("Send message to host:{0}, port:{1}, relatedBaseUri:{2}, jobId:{3}, identifier:{4}, connectionTimeout:{5}, exception:{6}",
                    host, port, relatedBaseUri, jobId, identifier, connectionTimeout, ex.ToString());
                throw;
            }
            finally
            {
                if (client != null)
                {
                    client.Stop(host, port, relatedBaseUri, jobId);
                }
            }

            return sendSuccessfully;
        }

        /// <summary>
        /// 一般用于只发送一个消息给目的端，但是注意，发送时，源端的Identifier需要设置临时的，
        /// 否则目的端WCF Service会覆盖之前存在的源端的callback
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="identifier"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="msg"></param>
        /// <param name="msgTimeout"></param>
        /// <returns></returns>
        public static bool SendMessageDirectly(string host, int port, string relatedBaseUri, string jobId, string identifier, int connectionTimeout, AveMessage msg, int msgTimeout = 30*1000)
        {
            bool sendSuccessfully = false;
            AveMQClient client = null;
            AveMQClient2ServerPeer client2ServerPeer = null;
            try
            {
                client = new AveMQClient(jobId, identifier);
                client.ConnectionTimeout = connectionTimeout;
                client2ServerPeer = new AveMQClient2ServerPeer(client, host, port, relatedBaseUri, jobId, connectionTimeout, false);
                client2ServerPeer.Start();
                client2ServerPeer.SendMessage(msg, msgTimeout);
                sendSuccessfully = true;
            }
            catch (Exception ex)
            {
                sendSuccessfully = false;
                logger.Error("Send message to host:{0}, port:{1}, relatedBaseUri:{2}, jobId:{3}, identifier:{4}, connectionTimeout:{5}, exception:{6}",
                    host, port, relatedBaseUri, jobId, identifier, connectionTimeout, ex.ToString());
                throw;
            }
            finally
            {
                if (client2ServerPeer != null)
                {
                    client2ServerPeer.Stop(true);
                }
            }

            return sendSuccessfully;
        }

        /// <summary>
        /// 使用匿名者发送信息，不带任何反馈。
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="msg"></param>
        /// <param name="msgTimeout"></param>
        /// <returns></returns>
        public static bool SendMessageDirectly(string host, int port, string relatedBaseUri, string jobId, int connectionTimeout, AveMessage msg, int msgTimeout = 30*1000)
        {
            return SendMessageDirectly(host, port, relatedBaseUri, jobId, "Anonymous " + Guid.NewGuid().ToString(), connectionTimeout, msg, msgTimeout);
        }

        /// <summary>
        /// 静态方法，发送消息给一个进程
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="relatedBaseUri"></param>
        /// <param name="jobId"></param>
        /// <param name="identifier"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool SendMessage(string host, int port, string relatedBaseUri, string jobId, string identifier, AveMessage msg)
        {
            return SendMessage(host, port, relatedBaseUri, jobId, identifier, AveMQConfigure.MaxReconnectionTimeOut, msg);
        }
        #endregion

        #region Inner Class
        /// <summary>
        /// AveThreadPoolItem的一个基本实现类，主要是调用消息处理委托。
        /// 另外使用ThreadPool可能会更有效的分配CPU和Memory。
        /// </summary>
        private class InnerMessageCallbackWorker : AveThreadPoolItemBase
        {
            #region Private Fields
            private AveMessage mMessage;
            private MessageReceiver mMessageReceiver;
            #endregion

            public InnerMessageCallbackWorker(AveMessage msg, MessageReceiver messageReceiver)
                : base("InnerMessage Callback Worker")
            {
                this.mMessage = msg;
                this.mMessageReceiver = messageReceiver;
            }

            public override void Run()
            {
                try
                {
                    mMessageReceiver(mMessage);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());//TODO
                }
            }
        }
        #endregion
    }
}

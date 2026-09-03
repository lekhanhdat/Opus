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
using AvePoint.GCommon.Utility;
using System.Threading;
using System.IO;
using AvePoint.GCommon.Transfer.MQ.Channel;
using AvePoint.GCommon.Transfer.MQ.Interface;
using AvePoint.GCommon;
using AvePoint.GCommon.Transfer.Common;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// WCF Service得到的消息交给MQServer来处理，包括Callback等
    /// </summary>
    internal class AveMQServer : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveMQServer), false);
        private static object SyncObj = new object();
        private static AveMQServer MQServer;

        private LinkedList<AveMQServer2ClientPeer> mClientPeers = new LinkedList<AveMQServer2ClientPeer>();
        private LinkedList<AveMessage> mMessages = new LinkedList<AveMessage>();
        private AveThreadWrapper mDispatchMessageThread;
        private AveThreadWrapper mDumpMessageThread;
        private MQPerformanceCounter performanceCounter;

        #region Static Methods
        public static AveMQServer GetInstance()
        {
            if (MQServer == null)
            {
                lock (SyncObj)
                {
                    if (MQServer == null)
                    {
                        MQServer = new AveMQServer();
                    }
                }
            }

            return MQServer;
        }
        #endregion

        #region Public Methods
        public void PutMessage(AveMessage msg)
        {
            lock (mMessages)
            {
                if (msg.TimeOut < 0)
                {
                    msg.TimeOut = DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.MaxMessageTimeout;
                }
                msg.EnqueueTime = DateTime.UtcNow;
                mMessages.AddLast(msg);
                performanceCounter.IncreaseCount(1);
                performanceCounter.RecordActiveMessage(mMessages.Count);
            }
        }

        public AveMessage GetMessage(string sessionId, string identifier)
        {
            AveMessage msg = null;
            bool isPeerExist = false;

            lock (mClientPeers)
            {
                foreach (AveMQServer2ClientPeer clientPeer in mClientPeers)
                {
                    if (clientPeer.IsMatch(sessionId, identifier))
                    {
                        isPeerExist = true;
                        clientPeer.GetMessage(out msg);
                        break;
                    }
                }
            }

            if (!isPeerExist)
            {
                AddOrUpdateMQClientPeer(sessionId, identifier, null);
            }

            return msg;
        }

        /// <summary>
        /// 添加或者更新ClientPeer
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <param name="clientCallback"></param>
        public void AddOrUpdateMQClientPeer(string sessionId, string identifier, IMQClientCallback clientCallback)
        {
            AveMQServer2ClientPeer clientPeer = null;
            bool needToUpdate = false;
            lock (mClientPeers)
            {
                foreach (AveMQServer2ClientPeer peer in mClientPeers)
                {
                    if (peer.IsMatch(sessionId, identifier))
                    {
                        clientPeer = peer;
                        break;
                    }
                }

                if (clientPeer == null)
                {
                    clientPeer = new AveMQServer2ClientPeer(this, sessionId, identifier, clientCallback);
                    mClientPeers.AddLast(clientPeer);
                }
                else
                {
                    needToUpdate = true;
                }
                performanceCounter.RecordActiveClients(mClientPeers.Count);
            }
            if (needToUpdate)
            {
                clientPeer.UpdateClientCallBack(clientCallback);
            }
        }

        public void RemoveMQClientPeer(AveMQServer2ClientPeer clientPeer, bool normalExist = true)
        {
            lock (mClientPeers)
            {
                mClientPeers.Remove(clientPeer);
                performanceCounter.RecordActiveClients(mClientPeers.Count);
            }

            mLogger.Debug(string.Format("Remove ClientPeer:{0}. Normally exist:{1}", clientPeer.ToString(), normalExist));
        }

        public void RemoveMQClientPeer(string sessionId, string identifier)
        {
            List<AveMQServer2ClientPeer> removedPeers = new List<AveMQServer2ClientPeer>();
            lock (mClientPeers)
            {
                foreach (AveMQServer2ClientPeer peer in mClientPeers)
                {
                    if (peer.IsMatch(sessionId, identifier))
                    {
                        removedPeers.Add(peer);
                    }
                }

                foreach (AveMQServer2ClientPeer peer in removedPeers)
                {
                    mClientPeers.Remove(peer);
                    mLogger.Debug("Remove client peer:" + peer.ToString());
                }
                performanceCounter.RecordActiveClients(mClientPeers.Count);
            }
            foreach (AveMQServer2ClientPeer peer in removedPeers)
            {
                peer.Close();
                mLogger.Debug("Close client peer:" + peer.ToString());
            }
        }

        public bool IsClientPeerAvailable(string sessionId, string identifier)
        {
            bool isAvailable = false;
            lock (mClientPeers)
            {
                foreach (AveMQServer2ClientPeer peer in mClientPeers)
                {
                    if (peer.IsMatch(sessionId, identifier))
                    {
                        isAvailable = true;
                        break;
                    }
                }
            }

            return isAvailable;
        }
        #endregion

        #region Private Methods
        private AveMQServer()
        {
            Init();
        }
        private void Init()
        {
            mDispatchMessageThread = AveThreadUtility.StartThread(DispatchMessage, "Dispatch message from MQ Server", "MQServer");
            mDumpMessageThread = AveThreadUtility.StartThread(DumpMessage, "Dump message from MQ Server", "MQServer");
            performanceCounter = new MQPerformanceCounter(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.EnablePerformanceCounter, System.Diagnostics.Process.GetCurrentProcess().ProcessName);
        }
        private void DispatchMessage()
        {
            try
            {
                AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (currentThreadWrapper.KeepRunning)
                {
                    LinkedList<AveMessage> messageRemoved = new LinkedList<AveMessage>();
                    lock (mMessages)
                    {
                        long expiredMessage = 0;
                        foreach (AveMessage msg in mMessages)
                        {
                            if (msg.IsTimeout)
                            {
                                messageRemoved.AddLast(msg);
                                expiredMessage++;
                                mLogger.Warn("One message is timeout. " + msg.ToString());
                            }
                            else
                            {
                                lock (mClientPeers)
                                {
                                    foreach (AveMQServer2ClientPeer clientPeer in mClientPeers)
                                    {
                                        if (clientPeer.IsMatch(msg))
                                        {
                                            clientPeer.PutMessage(msg);
                                            messageRemoved.AddLast(msg);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (messageRemoved.Count > 0)
                        {
                            foreach (AveMessage msg in messageRemoved)
                            {
                                mMessages.Remove(msg);
                            }
                            performanceCounter.IncreaseExpired(expiredMessage);
                            performanceCounter.RecordActiveMessage(mMessages.Count);
                        }
                    }

                    foreach (AveMessage msg in messageRemoved)
                    {
                        msg.OnMessageDelivered();
                    }

                    if (messageRemoved.Count == 0)
                    {
                        Thread.Sleep(200);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Dispatch message failed:{0}", ex.ToString());
            }
        }
        private void DumpMessage()
        {
            try
            {
                AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
                string DumpFileName = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "DumpMQMessageQueue.AvePoint");
                while (currentThreadWrapper.KeepRunning)
                {
                    Thread.Sleep(2000);

                    if (File.Exists(DumpFileName))
                    {
                        string fileName = Path.Combine(DataTransferGlobalConfig.DataTransferConfiguration.MqConfig.TempFolder, "MQDUMP " + AveDateTimeUtility.ConvertToType013(DateTime.Now) + ".txt");
                        lock (mMessages)
                        {
                            using (StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8))
                            {
                                foreach (AveMessage aveMsg in mMessages)
                                {
                                    sw.WriteLine("-----------------------------------------------------------------------");
                                    sw.WriteLine(aveMsg.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Dump message failed:{0}", ex.ToString());
            }
        }
        #endregion

        /// <summary>
        /// Release performance counter
        /// </summary>
        public void Dispose()
        {
            if (performanceCounter != null)
            {
                performanceCounter.Dispose();
            }
        }
    }
}

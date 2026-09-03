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
using System.Threading;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.MQ.Interface;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Transfer.MQ
{
    /// <summary>
    /// Server用来分发消息给每个Client时对应的Peer。
    /// </summary>
    internal class AveMQServer2ClientPeer
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveMQServer2ClientPeer), false);

        #region Private Fields
        private AveMQServer mServer;
        private string mSessionId;
        private string mIdentifier;
        private IMQClientCallback mClient;
        private AveThreadWrapper mSendMessageThread;
        private SynchronizedLinkedList<AveMessage> mMessages = new SynchronizedLinkedList<AveMessage>();
        private AveMessage lastSendingMessage = null;
        private bool isRollback = true;
        #endregion

        #region Public Fields
        public string SessionId
        {
            get { return mSessionId; }
        }
        public string Identifier
        {
            get { return mIdentifier; }
        }
        #endregion

        #region Public Methods

        public AveMQServer2ClientPeer(AveMQServer server, string sessionId, string identifier, IMQClientCallback mqClient)
        {
            this.mServer = server;
            this.mSessionId = sessionId;
            this.mIdentifier = identifier;
            this.mClient = mqClient;

            Init();
        }

        public bool IsMatch(AveMessage message)
        {
            return message.IsMatch(mSessionId, mIdentifier);
        }

        public bool IsMatch(AveMQServer2ClientPeer clientPeer)
        {
            return IsMatch(clientPeer.SessionId, clientPeer.Identifier);
        }
        /// <summary>
        /// only for match MQClientPeer...
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool IsMatch(string sessionId, string identifier)
        {
            return SessionId.Equals(sessionId, StringComparison.OrdinalIgnoreCase)
                && Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase);
        }

        public void PutMessage(AveMessage message)
        {
            lock (mMessages)
            {
                mMessages.AddLast(message);
            }
        }

        public bool GetMessage(out AveMessage message)
        {
            lock (mMessages)
            {
               return mMessages.TryGetFirst(out message);
            }
        }

        public override string ToString()
        {
            return string.Format("SessionId:{0}, Identifier:{1}", SessionId, Identifier);
        }

        public void Close()
        {
            lock (this)
            {
                try
                {
                    mLogger.Debug("Close the server to client peer:{0}.", this.ToString());
                    isRollback = false;
                    if (mSendMessageThread != null)
                    {
                        AveThreadUtility.SafeStopThread(mSendMessageThread, 5000, "Close", true);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Close the client peer:{0} failed:{1}.", this.ToString(), ex.ToString());
                }
                finally
                {
                    mSendMessageThread = null;
                    if (lastSendingMessage != null)
                    {
                        mServer.PutMessage(lastSendingMessage);
                        lastSendingMessage = null;
                    }
                    //mServer.RemoveMQClientPeer(this, false);
                    RollbackMessage();
                    //ObjectUtility.DisposeAndCloseChannel(mClient);
                }
            }
        }

        public void UpdateClientCallBack(IMQClientCallback callBack)
        {
            lock (this)
            {
                mLogger.Info("The peer:{0} is updating the callback.", this.ToString());
                try
                {
                    isRollback = false;
                    if (mSendMessageThread != null)
                    {
                        AveThreadUtility.SafeStopThread(mSendMessageThread, 1000, "Update client call back message", true);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Stop the previous peer:{0} failed:{1}.", this.ToString(), ex.ToString());
                }
                finally
                {
                    mSendMessageThread = null;
                    if (lastSendingMessage != null)
                    {
                        PutMessage(lastSendingMessage);
                        lastSendingMessage = null;
                    }
                    ObjectUtility.DisposeAndCloseChannel(mClient);
                    isRollback = true;
                }
                mClient = callBack;
                Init();
            }
        }
        #endregion

        #region Private Methods

        private void Init()
        {
            if (object.Equals(mClient, null))
            {
                mLogger.Info("One Way Connection Mode, {0}", this.ToString());
            }
            else
            {
                mSendMessageThread = AveThreadUtility.StartThread(SendMessage, "Send Message To Client:" + this.ToString(), "MQServer2ClientPeer");
            }
        }

        private void SendMessage()
        {
            //AveMessage msg = null;
            try
            {
                AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (currentThreadWrapper.KeepRunning)
                {
                    if (mMessages.TryGetFirst(out lastSendingMessage))
                    {
                        lock (mClient)
                        {
                            mClient.PutMessage(lastSendingMessage);
                            lastSendingMessage.OnMessageDelivered();
                            lastSendingMessage = null;
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                while (mMessages.TryGetFirst(out lastSendingMessage))
                {
                    lock (mClient)
                    {
                        mClient.PutMessage(lastSendingMessage);
                        lastSendingMessage.OnMessageDelivered();
                        lastSendingMessage = null;
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Send message from client peer:{0} failed:{1}", this.ToString(), ex.ToString());
            }
            finally
            {
                if (lastSendingMessage != null)
                {
                    mServer.PutMessage(lastSendingMessage);
                    lastSendingMessage = null;
                }
                if (isRollback)
                {
                    RollbackMessage();
                    this.mServer.RemoveMQClientPeer(this, true);
                }
            }
        }

        private void RollbackMessage()
        {
            AveMessage msg = null;
            while (mMessages.TryGetFirst(out msg))
            {
                mServer.PutMessage(msg);
                msg = null;
            }
        }
        #endregion
    }
}

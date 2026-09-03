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
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace AvePoint.GCommon.Network
{
    internal class AveNetworkTransferSession : IReConnectable, IDisposable
    {
        private string remoteHost;
        private int remotePort;
        private IAveNetworkChannel mInternalChannel;
        private bool mIsServerRole;
        private Guid mSessionId;

        private DateTime lastReadStartTime = DateTime.MinValue;
        private DateTime lastReadSucceedTime = DateTime.MinValue;
        private DateTime lastWriteStartTime = DateTime.MinValue;
        private DateTime lastWriteSucceedTime = DateTime.MinValue;

        private HandShakeException mHandShakeException = null;
        private SessionTimeoutException mSessionTimeoutException = null;

        private int mReconnectTimeout = 30 * 60 * 1000;
        private int mReconnectRetryInterval = 30 * 1000;
        private bool enableSSL;
        private string sslThumbprint;

        public int ReconnectTimeout { set { mReconnectTimeout = value; } }
        public int ReconnectRetryInterval { set { mReconnectRetryInterval = value; } }
        public bool EnableSSL { set { enableSSL = value; } }
        public string SSLThumbprint { set { sslThumbprint = value; } }

        public long TotalBytesReceived { get { return mInternalChannel.TotalBytesReceived; } }
        public long TotalReadTime { get { return mInternalChannel.TotalReadTime; } }
        public long TotalBytesSent { get { return mInternalChannel.TotalBytesSent; } }
        public long TotalWriteTime { get { return mInternalChannel.TotalWriteTime; } }
        public int Available { get { return mInternalChannel.Available; } }

        //包装一个Channel
        public void Wrap(string host, int port, IAveNetworkChannel channel, bool isServer, Guid guid)
        {
            this.remoteHost = host;
            this.remotePort = port;
            this.mInternalChannel = new AveReconnectableChannel(channel, this);
            this.mIsServerRole = isServer;
            this.mSessionId = guid;
        }

        public void WriteBytes(byte[] data, int offset, int len)
        {
            int curLen;
            while (len > 0)
            {
                curLen = len > 65535 ? 65536 : len;
                lastWriteStartTime = DateTime.Now;
                mInternalChannel.Write(data, offset, curLen);
                lastWriteSucceedTime = DateTime.Now;
                offset += curLen;
                len -= curLen;
            }
        }

        public int ReadBytes(byte[] data, int offset, int len)
        {
            lastReadStartTime = DateTime.Now;
            int readLen = mInternalChannel.Read(data, offset, len);
            lastReadSucceedTime = DateTime.Now;
            return readLen;
        }

        public void Shutdown(ShutDownOptions option)
        {
            mInternalChannel.Shutdown(option);
        }

        public void Close()
        {
            mInternalChannel.Close();
        }

        /*
         * 兼容老的Network 
         */
        virtual public int ReceiveBinary(byte[] data, int nIndex, int nLength)
        {
            // Receive data directly from server
            return ReadBytes(data, nIndex, nLength);
        }


        #region ReConnectAble Members

        private Semaphore mReConnectedSemaphore = new Semaphore(0, 1);

        public void ResetSocket(Socket socket, Stream socketStream)
        {
            //当session为Server Role的时候重连，不需要主动连接对方，只需要等待AveNetworkServer来reset socket即可
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to reset internal socket. newSocketHandle:{0} ", socket.Handle);

            //当client端重连的时候，作为server端的socket.read(...) socket.write(...)可能还处于阻塞状态，所以在reset socket之前要先关闭
            //原来的channel,这样就会迫使作为server的socket去重连，否则可能出现AveNetworkServer虽然reset了socket，但读写线程还处在原来socket
            //的读写阻塞状态之中，所以在真实replace的第一步就要先把原来的channel关掉。
            mInternalChannel.Close();


            //发送自己成功接收的数据长度
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to send bytes received. newSocketHandle:{0} TotalBytesReceived:{1}", socket.Handle, mInternalChannel.TotalBytesReceived);
            SendReceiveUtility.SafeSend(socketStream, BitConverter.GetBytes(mInternalChannel.TotalBytesReceived));

            //接收对方做BackToByte的结果
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to get BackToByte result. newSocketHandle:{0} ", socket.Handle);
            byte[] peerBackToByteStatusBuffer = new byte[1];
            SendReceiveUtility.SafeReceive(socketStream, peerBackToByteStatusBuffer, 0, 1);
            bool peerBackToByteStatus = BitConverter.ToBoolean(peerBackToByteStatusBuffer, 0);
            if (peerBackToByteStatus == false)
            {
                throw new CachedBufferOverflowException("AveNetwork cached buffer overflow.");
            }

            //接收对方成功接收的数据长度
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to get totalReceiveBytes on the other side. newSocketHandle:{0} ", socket.Handle);
            byte[] peerReceivedLengthBuffer = new byte[8];
            SendReceiveUtility.SafeReceive(socketStream, peerReceivedLengthBuffer, 0, peerReceivedLengthBuffer.Length);
            long receivedLentgh = BitConverter.ToInt64(peerReceivedLengthBuffer, 0);
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession get totalReceiveBytes on the other side. newSocketHandle:{0} totalReceiveBytes:{1} ", socket.Handle, receivedLentgh);

            //做BackToByte并返回结果给对方
            bool backToByteStatus = mInternalChannel.BackToByte(receivedLentgh);
            byte[] backToByteStatusBuffer = BitConverter.GetBytes(backToByteStatus);
            SendReceiveUtility.SafeSend(socketStream, backToByteStatusBuffer, 0, backToByteStatusBuffer.Length);
            if (backToByteStatus == false)
            {
                throw new CachedBufferOverflowException("AveNetworkServer cached buffer overflow.");
            }

            //重置socket
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to replace internal channel socket. newSocketHandle:{0} ", socket.Handle);
            mInternalChannel.ReplaceSocket(socket, socketStream);

            //通知作为Server Role的上层底层已经做完同步，可以恢复执行
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession try to release reconnect semaphore . newSocketHandle:{0} ", socket.Handle);
            try
            {
                mReConnectedSemaphore.Release();
            }
            catch (SemaphoreFullException se)
            {
                AveNetworkTrace.TraceWarning("Release semaphore exception: {0}", se.ToString());
                AveNetworkTrace.TraceWarning("The count of the semaphore exceed its maximum. it may be caused by server role of session not use socket for a long time.");
                AveNetworkTrace.TraceWarning("LastReadStartTime:{0} LastReadSucceedTime:{1} LastWriteStartTime:{2} LastWriteSucceedTime:{3}", lastReadStartTime.ToString(), lastReadSucceedTime.ToString(), lastWriteStartTime.ToString(), lastWriteSucceedTime.ToString());
            }
            AveNetworkTrace.TraceVerbose("AveNetworkTransferSession finish to reset internal socket. newSocketHandle:{0} ", socket.Handle);
        }

        public void ReConnect()
        {
            AveNetworkTrace.TraceVerbose("reconnecting  mIsServerRole:{0}", mIsServerRole);
            if (mIsServerRole)
            {
                //当session作为socket的server端使用的时候，不需要主动连接对方，只需要等待AveNetworkServer来reset socket即可
                AveNetworkTrace.TraceVerbose("waiting for socket reset. sessionID:{0}", mSessionId);
                if (this.mSessionTimeoutException != null)
                {
                    AveNetworkTrace.TraceError("session timeout when waiting last time, throw this exception.");
                    throw this.mSessionTimeoutException;
                }
                bool succeed = mReConnectedSemaphore.WaitOne(mReconnectTimeout);
                AveNetworkTrace.TraceVerbose(string.Format("waiting for socket reset returned. succeed:{0} sessionID:{1}", succeed, mSessionId));
                if (!succeed)
                {
                    this.mSessionTimeoutException = new SessionTimeoutException(mReconnectTimeout, string.Format("waiting for socket reset session time out. {0} milliseconds elapsed.", mReconnectTimeout));
                    throw this.mSessionTimeoutException;
                }
            }
            else
            {
                //当session作为socket的client端得时候，需要主动连接对方进行第一阶段的握手
                DateTime deadLine = DateTime.Now.AddMilliseconds(mReconnectTimeout);
                Socket newSocket = null;
                Stream newSocketStream = null;
                while (true)
                {
                    if (this.mHandShakeException != null)
                    {
                        AveNetworkTrace.TraceWarning("hand shake exception occurred when reconnecting last time, throw this exception.");
                        throw this.mHandShakeException;
                    }
                    try
                    {
                        AveNetworkTrace.TraceVerbose("try to reconnect to remote endpoint: {0} Time: {1} Deadline: {2} sessionID:{3}", this.remoteHost + ":" + this.remotePort, DateTime.Now.ToString(), deadLine.ToString(), mSessionId);
                        AveNetworkConnector.ReConnectToServer(this.remoteHost, this.remotePort, this.mSessionId, this.enableSSL, this.sslThumbprint, out newSocket, out newSocketStream);
                        AveNetworkTrace.TraceVerbose("reconnect successfully to remote endpoint: {0} Time: {1} Deadline: {2} sessionID: {3}", this.remoteHost + ":" + this.remotePort, DateTime.Now.ToString(), deadLine.ToString(), mSessionId);
                        break;
                    }
                    catch (HandShakeException hse)
                    {
                        AveNetworkTrace.TraceVerbose("hand shake exception occurred while reconnecting to remote endpoint: {0} Time: {1} Deadline: {2} Message:{3}", this.remoteHost + ":" + this.remotePort, DateTime.Now.ToString(), deadLine.ToString(), hse.ToString());
                        this.mHandShakeException = hse;
                        throw;
                    }
                    catch (Exception e)
                    {
                        AveNetworkTrace.TraceError("cannot reconnect to remote endpoint : {0} Exception:{1}", this.remoteHost + ":" + this.remotePort, e.ToString());
                        if (DateTime.Now > deadLine)
                        {
                            throw new NetworkBrokenException(string.Format("retry reconnect deadline reached. remote endpoint : {0} ", this.remoteHost + ":" + this.remotePort), e);
                        }
                        AveNetworkTrace.TraceError("sleep for a while and try again to reconnect remote endpoint : {0}", this.remoteHost + ":" + this.remotePort);
                        Thread.Sleep(mReconnectRetryInterval);
                    }
                }

                //接收对方成功接收的数据长度
                AveNetworkTrace.TraceVerbose("try to get totalReceivedBytes on server side. sessionID: {0}", mSessionId);
                byte[] peerReceivedLengthBuffer = new byte[8];
                SendReceiveUtility.SafeReceive(newSocketStream, peerReceivedLengthBuffer, 0, peerReceivedLengthBuffer.Length);
                long receivedLength = BitConverter.ToInt64(peerReceivedLengthBuffer, 0);
                AveNetworkTrace.TraceVerbose("get totalReceivedBytes on server side. sessionID: {0} receivedLength:{1}", mSessionId, receivedLength);
                //做BackToByte并返回结果给对方

                bool backToByteStatus = mInternalChannel.BackToByte(receivedLength);
                byte[] backToByteStatusBuffer = BitConverter.GetBytes(backToByteStatus);
                SendReceiveUtility.SafeSend(newSocketStream, backToByteStatusBuffer, 0, backToByteStatusBuffer.Length);
                if (backToByteStatus == false)
                {
                    throw new CachedBufferOverflowException("AveNetwork cached buffer overflow.");
                }

                //发送自己成功接收的数据长度
                AveNetworkTrace.TraceVerbose("try to send TotalBytesReceived to server side. sessionID: {0} TotalBytesReceived:{1}", mSessionId, mInternalChannel.TotalBytesReceived);
                long received = mInternalChannel.TotalBytesReceived;
                SendReceiveUtility.SafeSend(newSocketStream, BitConverter.GetBytes(received));

                //接收对方做BackToByte的结果
                AveNetworkTrace.TraceVerbose("try to get BackToByte result on server side. sessionID: {0} ", mSessionId);
                byte[] peerBackToByteStatusBuffer = new byte[1];
                SendReceiveUtility.SafeReceive(newSocketStream, peerBackToByteStatusBuffer, 0, 1);
                bool peerBackToByteStatus = BitConverter.ToBoolean(peerBackToByteStatusBuffer, 0);
                if (peerBackToByteStatus == false)
                {
                    throw new CachedBufferOverflowException("AveNetworkServer cached buffer overflow.");
                }

                //重置socket
                AveNetworkTrace.TraceVerbose("try to replace internal channel socket. sessionID: {0} ", mSessionId);
                mInternalChannel.ReplaceSocket(newSocket, newSocketStream);
                AveNetworkTrace.TraceVerbose("successfully replace internal channel socket. sessionID: {0} ", mSessionId);
            }
        }

        #endregion


        public void Dispose()
        {
            if (mReConnectedSemaphore != null)
            {
                mReConnectedSemaphore.Close();
                mReConnectedSemaphore = null;
            }
        }
    }
}

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




using System.Globalization;

namespace AvePoint.GCommon.Network
{
    #region using directives
    using System;
    using System.Net.Sockets;
    using System.Threading;
    using System.IO;
    #endregion

    internal class AveNetworkTransferSession
        : IReConnectable
        , IDisposable
    {
        readonly AveConnectionOptions connectionOptions;
        IAveNetworkChannel internalChannel;
        bool mIsServerRole;
        Guid sessionId;

        DateTime lastReadStartTime;
        DateTime lastReadSucceedTime;
        DateTime lastWriteStartTime;
        DateTime lastWriteSucceedTime;

        HandShakeException handShakeException;
        SessionTimeoutException sessionTimeoutException;

        public AveConnectionOptions ConnOptions { get { return connectionOptions; } }
        public long TotalBytesReceived { get { return internalChannel.TotalBytesReceived; } }
        public long TotalReadTime { get { return internalChannel.TotalReadTime; } }
        public long TotalBytesSent { get { return internalChannel.TotalBytesSent; } }
        public long TotalWriteTime { get { return internalChannel.TotalWriteTime; } }
        public int Available { get { return internalChannel.Available; } }

        public AveNetworkTransferSession(AveConnectionOptions connOptions)
        {
            this.connectionOptions = connOptions;
        }

        //包装一个Channel
        public void Wrap(IAveNetworkChannel channel, bool isServer, Guid guid)
        {
            this.internalChannel = new AveReconnectableChannel(channel, this, this.connectionOptions);
            this.mIsServerRole = isServer;
            this.sessionId = guid;
        }

        public void WriteBytes(byte[] data, int offset, int len)
        {
            while (len > 0)
            {
                var curLen = len > 65535 ? 65536 : len;
                lastWriteStartTime = DateTime.Now;
                internalChannel.Write(data, offset, curLen);
                lastWriteSucceedTime = DateTime.Now;
                offset += curLen;
                len -= curLen;
            }
        }

        public int ReadBytes(byte[] data, int offset, int len, bool mustGet)
        {
            lastReadStartTime = DateTime.Now;
            var readLen = internalChannel.Read(data, offset, len, mustGet);
            lastReadSucceedTime = DateTime.Now;
            return readLen;
        }

        public void Shutdown(ShutDownOptions option)
        {
            internalChannel.Shutdown(option);
        }

        public void Close()
        {
            internalChannel.Close();
        }

        #region ReConnectAble Members

        private Semaphore mReConnectedSemaphore = new Semaphore(0, 1);

        public void ResetSocket(Socket socket, Stream socketStream)
        {
            //当session为Server Role的时候重连，不需要主动连接对方，只需要等待AveNetworkServer来reset socket即可

            //当client端重连的时候，作为server端的socket.read(...) socket.write(...)可能还处于阻塞状态，所以在reset socket之前要先关闭
            //原来的channel,这样就会迫使作为server的socket去重连，否则可能出现AveNetworkServer虽然reset了socket，但读写线程还处在原来socket
            //的读写阻塞状态之中，所以在真实replace的第一步就要先把原来的channel关掉。
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to close original socket. newSocketHandle:{0} ", socket.Handle);
            internalChannel.Close();
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession already closed original socket. newSocketHandle:{0} ", socket.Handle);

            //这里面等待一会，如果前一条注释说的线程正处于阻塞状态的话，这时候应该抛出异常，然后作为server role进入重连步骤，也就是等待接下来的socket reset
            Thread.Sleep(5000);

            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to reset internal socket. newSocketHandle:{0} ", socket.Handle);
            //发送自己成功接收的数据长度
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to send bytes received. newSocketHandle:{0} TotalBytesReceived:{1}", socket.Handle, internalChannel.TotalBytesReceived);
            SendReceiveUtility.SafeSend(socketStream, BitConverter.GetBytes(internalChannel.TotalBytesReceived));

            //接收对方做BackToByte的结果
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to get BackToByte result. newSocketHandle:{0} ", socket.Handle);
            var peerBackToByteStatusBuffer = new byte[1];
            SendReceiveUtility.SafeReceive(socketStream, peerBackToByteStatusBuffer, 0, 1);
            bool peerBackToByteStatus = BitConverter.ToBoolean(peerBackToByteStatusBuffer, 0);
            if (peerBackToByteStatus == false)
            {
                throw new CachedBufferOverflowException("AveNetwork cached buffer overflow.");
            }

            //接收对方成功接收的数据长度
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to get totalReceiveBytes on the other side. newSocketHandle:{0} ", socket.Handle);
            var peerReceivedLengthBuffer = new byte[8];
            SendReceiveUtility.SafeReceive(socketStream, peerReceivedLengthBuffer, 0, peerReceivedLengthBuffer.Length);
            var receivedLentgh = BitConverter.ToInt64(peerReceivedLengthBuffer, 0);
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession get totalReceiveBytes on the other side. newSocketHandle:{0} totalReceiveBytes:{1} ", socket.Handle, receivedLentgh);

            //做BackToByte并返回结果给对方
            var backToByteStatus = internalChannel.BackToByte(receivedLentgh);
            var backToByteStatusBuffer = BitConverter.GetBytes(backToByteStatus);
            SendReceiveUtility.SafeSend(socketStream, backToByteStatusBuffer, 0, backToByteStatusBuffer.Length);
            if (backToByteStatus == false)
            {
                throw new CachedBufferOverflowException("AveNetworkServer cached buffer overflow.");
            }

            //重置socket
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to replace internal channel socket. newSocketHandle:{0} ", socket.Handle);
            internalChannel.ReplaceSocket(socket, socketStream);
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession finish to reset internal socket. newSocketHandle:{0} ", socket.Handle);

            //通知作为Server Role的上层底层已经做完同步，可以恢复执行.
            //等待片刻之后再通知server role已经完成reset可以恢复执行，这样从log的时间点能更清晰的看出这个过程
            Thread.Sleep(5000);//
            AveNetworkTrace.TraceInformation("AveNetworkTransferSession try to release reconnect semaphore . newSocketHandle:{0} ", socket.Handle);
            try
            {
                mReConnectedSemaphore.Release();
            }
            catch (SemaphoreFullException se)
            {
                //这里出异常，说明server role没有卡在前面注释中说的socket.read()/write()上，可能正在处理数据或其他逻辑中执行，
                //也就没有Wait信号，所以Release才会出异常，忽略即可，因为现在已经完成了reset, 当server role继续再来读写的时候socket已经是可用的了，
                //相当于这段时间reset底层socket的流程对他透明了。
                AveNetworkTrace.TraceWarning("Release semaphore exception: {0}", se.ToString());
                AveNetworkTrace.TraceWarning("The count of the semaphore exceed its maximum. it may be caused by server role of session not use socket for a long time.");
                AveNetworkTrace.TraceWarning("LastReadStartTime:{0} LastReadSucceedTime:{1} LastWriteStartTime:{2} LastWriteSucceedTime:{3}", lastReadStartTime.ToString(CultureInfo.InvariantCulture), lastReadSucceedTime.ToString(CultureInfo.InvariantCulture), lastWriteStartTime.ToString(CultureInfo.InvariantCulture), lastWriteSucceedTime.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void ReConnect()
        {
            AveNetworkTrace.TraceInformation("reconnecting  mIsServerRole:{0}", mIsServerRole);
            if (mIsServerRole)
            {
                //当session作为socket的server端使用的时候，不需要主动连接对方，只需要等待AveNetworkServer来reset socket即可
                AveNetworkTrace.TraceInformation("waiting for socket reset. sessionID:{0}", sessionId);
                if (this.sessionTimeoutException != null)
                {
                    AveNetworkTrace.TraceError("session timeout when waiting last time, throw this exception.");
                    throw this.sessionTimeoutException;
                }
                var succeed = mReConnectedSemaphore.WaitOne(connectionOptions.ReconnectTimeout);
                AveNetworkTrace.TraceInformation(string.Format("waiting for socket reset returned. succeed:{0} sessionID:{1}", succeed, sessionId));
                if (!succeed)
                {
                    this.sessionTimeoutException = new SessionTimeoutException(connectionOptions.ReconnectTimeout, string.Format("waiting for socket reset session time out. {0} milliseconds elapsed.", connectionOptions.ReconnectTimeout));
                    throw this.sessionTimeoutException;
                }
            }
            else
            {
                //当session作为socket的client端得时候，需要主动连接对方进行第一阶段的握手
                var deadLine = DateTime.Now.AddMilliseconds(connectionOptions.ReconnectTimeout);
                Socket newSocket;
                Stream newSocketStream;
                while (true)
                {
                    if (this.handShakeException != null)
                    {
                        AveNetworkTrace.TraceWarning("hand shake exception occurred when reconnecting last time, throw this exception.");
                        throw this.handShakeException;
                    }
                    try
                    {
                        AveNetworkTrace.TraceInformation("try to reconnect to remote endpoint: {0} Time: {1} Deadline: {2} sessionID:{3}", this.connectionOptions.Host + ":" + this.connectionOptions.Port, DateTime.Now.ToString(CultureInfo.InvariantCulture), deadLine.ToString(CultureInfo.InvariantCulture), sessionId);
                        AveNetworkConnector.ReConnectToServer(this.sessionId, this.connectionOptions, out newSocket, out newSocketStream);
                        AveNetworkTrace.TraceInformation("reconnect successfully to remote endpoint: {0} Time: {1} Deadline: {2} sessionID: {3}", this.connectionOptions.Host + ":" + this.connectionOptions.Port, DateTime.Now.ToString(CultureInfo.InvariantCulture), deadLine.ToString(CultureInfo.InvariantCulture), sessionId);
                        break;
                    }
                    catch (HandShakeException hse)
                    {
                        AveNetworkTrace.TraceError("hand shake exception occurred while reconnecting to remote endpoint: {0} Time: {1} Deadline: {2} Message:{3}", this.connectionOptions.Host + ":" + this.connectionOptions.Port, DateTime.Now.ToString(CultureInfo.InvariantCulture), deadLine.ToString(CultureInfo.InvariantCulture), hse.ToString());
                        this.handShakeException = hse;
                        throw;
                    }
                    catch (Exception e)
                    {
                        AveNetworkTrace.TraceError("cannot reconnect to remote endpoint : {0} Exception:{1}", this.connectionOptions.Host + ":" + this.connectionOptions.Port, e.ToString());
                        if (DateTime.Now > deadLine)
                        {
                            throw new NetworkBrokenException(string.Format("Retry reconnect deadline reached. remote endpoint : {0} " + "I18NKey: CommonNetwork_TransferSession_ReConnectTimeOutException.", this.connectionOptions.Host + ":" + this.connectionOptions.Port), e);
                        }
                        AveNetworkTrace.TraceError("sleep for a while and try again to reconnect remote endpoint : {0}", this.connectionOptions.Host + ":" + this.connectionOptions.Port);
                        Thread.Sleep(connectionOptions.ReconnectRetryInterval);
                    }
                }

                //接收对方成功接收的数据长度
                AveNetworkTrace.TraceInformation("try to get totalReceivedBytes on server side. sessionID: {0}", sessionId);
                var peerReceivedLengthBuffer = new byte[8];
                SendReceiveUtility.SafeReceive(newSocketStream, peerReceivedLengthBuffer, 0, peerReceivedLengthBuffer.Length);
                long receivedLength = BitConverter.ToInt64(peerReceivedLengthBuffer, 0);
                AveNetworkTrace.TraceInformation("get totalReceivedBytes on server side. sessionID: {0} receivedLength:{1}", sessionId, receivedLength);
                //做BackToByte并返回结果给对方

                bool backToByteStatus = internalChannel.BackToByte(receivedLength);
                byte[] backToByteStatusBuffer = BitConverter.GetBytes(backToByteStatus);
                SendReceiveUtility.SafeSend(newSocketStream, backToByteStatusBuffer, 0, backToByteStatusBuffer.Length);
                if (backToByteStatus == false)
                {
                    throw new CachedBufferOverflowException("AveNetwork cached buffer overflow.");
                }

                //发送自己成功接收的数据长度
                AveNetworkTrace.TraceInformation("try to send TotalBytesReceived to server side. sessionID: {0} TotalBytesReceived:{1}", sessionId, internalChannel.TotalBytesReceived);
                long received = internalChannel.TotalBytesReceived;
                SendReceiveUtility.SafeSend(newSocketStream, BitConverter.GetBytes(received));

                //接收对方做BackToByte的结果
                AveNetworkTrace.TraceInformation("try to get BackToByte result on server side. sessionID: {0} ", sessionId);
                var peerBackToByteStatusBuffer = new byte[1];
                SendReceiveUtility.SafeReceive(newSocketStream, peerBackToByteStatusBuffer, 0, 1);
                bool peerBackToByteStatus = BitConverter.ToBoolean(peerBackToByteStatusBuffer, 0);
                if (peerBackToByteStatus == false)
                {
                    throw new CachedBufferOverflowException("AveNetworkServer cached buffer overflow.");
                }

                //重置socket
                AveNetworkTrace.TraceInformation("try to replace internal channel socket. sessionID: {0} ", sessionId);
                internalChannel.ReplaceSocket(newSocket, newSocketStream);
                AveNetworkTrace.TraceInformation("successfully replace internal channel socket. sessionID: {0} ", sessionId);
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

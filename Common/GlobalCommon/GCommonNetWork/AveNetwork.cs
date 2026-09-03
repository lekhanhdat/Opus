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




namespace AvePoint.GCommon.Network
{
    #region using directives

    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    #endregion using directives

    public enum ShutDownOptions
    {
        Send,
        Receive,
        Both
    }

    /// <summary>
    /// Handles the network connection for Agent
    /// </summary>
    public class AveNetwork : IAveNetwork
    {
        private AveNetworkTransferSession internalSession;

        internal AveNetwork(AveNetworkTransferSession session)
        {
            internalSession = session;
        }

        public long TotalBytesReceived { get { return internalSession.TotalBytesReceived; } }

        public long TotalReadTime { get { return internalSession.TotalReadTime; } }

        public long TotalBytesSent { get { return internalSession.TotalBytesSent; } }

        public long TotalWriteTime { get { return internalSession.TotalWriteTime; } }

        public int Available { get { return internalSession.Available; } }

        public int ReconnectTimeout { set { internalSession.ReconnectTimeout = value; } }

        public int ReconnectRetryInterval { set { internalSession.ReconnectRetryInterval = value; } }

        #region connection control

        public static IAveNetwork Connect(string host, int port, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            return Connect(host, port, false, string.Empty, reconnectTimeOut, reconnectInterval);
        }

        public static IAveNetwork Connect(string host, int port, bool enableSSL, string sslThumbprint, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            DateTime deadLine = DateTime.Now.AddMilliseconds(reconnectTimeOut);
            while (true)
            {
                try
                {
                    AveNetworkTrace.TraceVerbose("try to connect to host: {0} Port: {1} Time: {2} Deadline: {3}", host, port, DateTime.Now.ToString(), deadLine.ToString());
                    AveNetworkTransferSession session = AveNetworkConnector.ConnectToServer(host, port, enableSSL, sslThumbprint);
                    session.EnableSSL = enableSSL;
                    session.SSLThumbprint = sslThumbprint;
                    session.ReconnectTimeout = reconnectTimeOut;
                    session.ReconnectRetryInterval = reconnectInterval;
                    AveNetworkTrace.TraceVerbose("connect successfully to host: {0} Port: {1} Time: {2} Deadline: {3}", host, port, DateTime.Now.ToString(), deadLine.ToString());
                    return new AveNetwork(session);
                }
                catch (HandShakeException he)
                {
                    AveNetworkTrace.TraceError("Handshake failed. Exception: {0}", he.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("cannot connect to host: {0} Port: {1} Exception: {2}", host, port, e.ToString());
                    if (DateTime.Now > deadLine)
                    {
                        throw new NetworkBrokenException(string.Format("retry connect deadline reached. host: {0} Port: {1} ", host, port), e);
                    }
                    AveNetworkTrace.TraceVerbose("sleep for a while and try again to connect host: {0} Port: {1}", host, port);
                    Thread.Sleep(reconnectInterval);
                }
            }
        }

        public static IAveNetwork ConnectToRawSocket(string host, int port)
        {
            return new AveNetworkRawSocket(host, port);
        }

        public virtual void Shutdown(ShutDownOptions shutDownOption = ShutDownOptions.Both)
        {
            if (internalSession != null)
            {
                internalSession.Shutdown(shutDownOption);
            }
        }

        public virtual void Close()
        {
            if (internalSession != null)
            {
                internalSession.Close();
            }
            AveNetworkTrace.TraceVerbose("The performance of this connection is : {0} TotalBytesReceived : {1} {0} TotalBytesSent : {2} {0} TotalReadTime : {3} {0} TotalWriteTime : {4} {0} ", Environment.NewLine, TotalBytesReceived, TotalBytesSent, TotalReadTime, TotalWriteTime);
        }

        #endregion connection control

        #region binary operation

        public void SendBinary(byte[] data, int nIndex, int nLength)
        {
            internalSession.WriteBytes(data, nIndex, nLength);
        }

        public int ReceiveBinary(byte[] data, int nIndex, int nLength)
        {
            return internalSession.ReadBytes(data, nIndex, nLength);
        }

        #endregion binary operation

        #region message operation

        public void SendMessage(string message)
        {
            byte[] msgData = new byte[Encoding.UTF8.GetByteCount(message) + AveDataBlock.DATA_BLOCK_HEADER_LEN];
            Encoding.UTF8.GetBytes(message, 0, message.Length, msgData, AveDataBlock.DATA_BLOCK_HEADER_LEN);

            AveDataBlock msgBlock = new AveDataBlock(msgData);
            msgBlock.DataSize = msgData.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN;
            msgBlock.Type = AveDataBlockType.MESSAGE_TYPE;

            SendDataBlock(msgBlock);
        }

        public string ReceiveMessage()
        {
            AveDataBlock msgBlock = new AveDataBlock();
            ReceiveDataBlock(msgBlock);
            return msgBlock.RetrieveString();
        }

        #endregion message operation

        #region data block operation

        public void SendDataBlock(AveDataBlock dataBlock)
        {
            internalSession.WriteBytes(
                dataBlock.Buffer,
                0,
                dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN
            );
        }

        /// <summary>
        /// Receive a data block from the server
        /// </summary>
        /// <param name="dataBlock">Result Data Block</param>
        public void ReceiveDataBlock(AveDataBlock dataBlock)
        {
            SafeReceive(dataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
            if (dataBlock.DataSize <= AveDataBlock.DATA_BLOCK_DATA_LEN)
            {
                SafeReceive(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
            }
            else
            {
                byte[] tempHeaderBuffer = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                dataBlock.CopyFromHeader(tempHeaderBuffer);
                dataBlock.Buffer = new byte[dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                dataBlock.CopyToHeader(tempHeaderBuffer);
                SafeReceive(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
            }
        }

        #endregion data block operation

        #region tool helper

        /// <summary>
        /// Receive data from the server until reach the length
        /// </summary>
        /// <param name="buffer">Destination Buffer</param
        /// >
        /// <param name="offset">Offset to be written</param>
        /// <param name="length">Desired length</param>
        virtual protected void SafeReceive(byte[] buffer, int offset, int length)
        {
            int retryTimes = 0;
            int onceLen = 0;
            int readLen = 0;
            while (readLen < length)
            {
                // continue reading until finish the header
                onceLen = internalSession.ReadBytes(
                    buffer,
                    offset + readLen,
                    length - readLen

                );
                if (onceLen <= 0)
                {
                    if ((retryTimes++) == 10)
                    {
                        throw new ArgumentException("Read data from a closed connection.");
                    }
                    AveNetworkTrace.TraceWarning("Zero bytes returned from network reading.");
                    Thread.Sleep(100);
                }
                readLen += onceLen;
            }
        }

        #endregion tool helper

        internal class AveNetworkRawSocket : IAveNetwork, IDisposable
        {
            private Socket socket;
            private TcpClient tcpClient;

            public int ReconnectRetryInterval { set { } }

            public int ReconnectTimeout { set { } }

            public int Available
            {
                get { return socket.Available; }
            }

            public long TotalBytesReceived
            {
                get { throw new NotImplementedException(); }
            }

            public long TotalReadTime
            {
                get { throw new NotImplementedException(); }
            }

            public long TotalBytesSent
            {
                get { throw new NotImplementedException(); }
            }

            public long TotalWriteTime
            {
                get { throw new NotImplementedException(); }
            }

            public AveNetworkRawSocket(Socket socket)
            {
                this.socket = socket;
            }

            public AveNetworkRawSocket(string host, int port)
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 60 * 60 * 1000;
                tcpClient.SendTimeout = 60 * 60 * 1000;
                tcpClient.ReceiveBufferSize = 30 * 1024;
                tcpClient.SendBufferSize = 30 * 1024;
                tcpClient.Connect(host, port);

                this.tcpClient = tcpClient;
                this.socket = tcpClient.Client;
            }

            public int ReceiveBinary(byte[] data, int nIndex, int nLength)
            {
                int readLen = socket.Receive(data, nIndex, nLength, SocketFlags.None);
                if (readLen <= 0)
                {
                    //Notes: The caller will also check the return value of this call,
                    AveNetworkTrace.TraceWarning("Zero bytes returned from network reading.");
                }
                return readLen;
            }

            public void ReceiveDataBlock(AveDataBlock dataBlock)
            {
                SafeReceive(dataBlock.Buffer, 0, AveDataBlock.DATA_BLOCK_HEADER_LEN);
                if (dataBlock.DataSize <= AveDataBlock.DATA_BLOCK_DATA_LEN)
                {
                    SafeReceive(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                }
                else
                {
                    byte[] tempHeaderBuffer = new byte[AveDataBlock.DATA_BLOCK_HEADER_LEN];
                    dataBlock.CopyFromHeader(tempHeaderBuffer);
                    dataBlock.Buffer = new byte[dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                    dataBlock.CopyToHeader(tempHeaderBuffer);
                    SafeReceive(dataBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, dataBlock.DataSize);
                }
            }

            public string ReceiveMessage()
            {
                AveDataBlock msgBlock = new AveDataBlock();
                ReceiveDataBlock(msgBlock);
                return msgBlock.RetrieveString();
            }

            public void SendBinary(byte[] data, int nIndex, int nLength)
            {
                socket.Send(data, nIndex, nLength, SocketFlags.None);
            }

            public void SendDataBlock(AveDataBlock dataBlock)
            {
                socket.Send(
                   dataBlock.Buffer,
                   0,
                   dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN,
                   SocketFlags.None
                    );
            }

            public void SendMessage(string message)
            {
                byte[] msgData = new byte[Encoding.UTF8.GetByteCount(message) + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                Encoding.UTF8.GetBytes(message, 0, message.Length, msgData, AveDataBlock.DATA_BLOCK_HEADER_LEN);

                AveDataBlock msgBlock = new AveDataBlock(msgData);
                msgBlock.DataSize = msgData.Length - AveDataBlock.DATA_BLOCK_HEADER_LEN;
                msgBlock.Type = AveDataBlockType.MESSAGE_TYPE;

                SendDataBlock(msgBlock);
            }

            public void Shutdown(ShutDownOptions shutDownOption = ShutDownOptions.Both)
            {
                if (socket != null)
                {
                    if (shutDownOption == ShutDownOptions.Both)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    if (shutDownOption == ShutDownOptions.Send)
                    {
                        socket.Shutdown(SocketShutdown.Send);
                    }
                    if (shutDownOption == ShutDownOptions.Receive)
                    {
                        socket.Shutdown(SocketShutdown.Receive);
                    }
                }
            }

            public void Close()
            {
                socket.Close();
            }

            virtual protected void SafeReceive(byte[] buffer, int offset, int length)
            {
                int retryTimes = 0;
                int onceLen = 0;
                int readLen = 0;
                while (readLen < length)
                {
                    // continue reading until finish the header
                    onceLen = socket.Receive(
                        buffer,
                        offset + readLen,
                        length - readLen,
                        SocketFlags.None
                    );
                    if (onceLen <= 0)
                    {
                        if ((retryTimes++) == 10)
                        {
                            throw new ArgumentException("Read data from a closed connection.");
                        }
                        AveNetworkTrace.TraceWarning("Zero bytes returned from network reading.");
                        Thread.Sleep(100);
                    }
                    readLen += onceLen;
                }
            }

            public void Dispose()
            {
                if(this.socket != null)
                {
                    this.socket.Dispose();
                }

                if(this.tcpClient != null)
                {
                    this.tcpClient.Dispose();
                }
            }
        }
    }
}
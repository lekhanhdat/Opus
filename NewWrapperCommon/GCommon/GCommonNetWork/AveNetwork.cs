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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;

    #endregion using directives

    public class AveConnectionOptions
    {
        private string host = string.Empty;
        private int port = -1;
        private bool enableSSL = false;
        private bool enableReconnect = true;
        private string sslThumbprint = string.Empty;
        private int reconnectTimeOut = 1800000;
        private int reconnectRetryInterval = 30000;
        private int receiveTimeout = 60 * 60 * 1000;
        private int sendTimeout = 60 * 60 * 1000;
        private int receiveBufferSize = 30 * 1024;
        private int sendBufferSize = 30 * 1024;
        private int dataBlockQueueSize = 100;
        private int sentCacheBufferSize = 5 * 1024 * 1024;
        private int sentCacheConfirmSize = 4 * 1024 * 1024;

        public string Host { get { return host; } set { host = value; } }
        public int Port { get { return port; } set { port = value; } }
        public bool EnableSSL { get { return enableSSL; } set { enableSSL = value; } }
        public bool EnableReconnect { get { return enableReconnect; } set { enableReconnect = value; } }
        public string SSLThumbprint { get { return sslThumbprint; } set { sslThumbprint = value; } }
        public int ReconnectTimeout { get { return reconnectTimeOut; } set { reconnectTimeOut = value; } }
        public int ReconnectRetryInterval { get { return reconnectRetryInterval; } set { reconnectRetryInterval = value; } }
        public int ReceiveTimeout { get { return receiveTimeout; } set { receiveTimeout = value; } }
        public int SendTimeout { get { return sendTimeout; } set { sendTimeout = value; } }
        public int ReceiveBufferSize { get { return receiveBufferSize; } set { receiveBufferSize = value; } }
        public int SendBufferSize { get { return sendBufferSize; } set { sendBufferSize = value; } }
        public int DataBlockQueueSize { get { return dataBlockQueueSize; } set { dataBlockQueueSize = value; } }
        public int SentCacheBufferSize { get { return sentCacheBufferSize; } set { if (value < sentCacheConfirmSize)throw new ArgumentException(); sentCacheBufferSize = value; } }
        public int SentCacheConfirmSize { get { return sentCacheConfirmSize; } set { if (value > sentCacheBufferSize)throw new ArgumentException(); sentCacheConfirmSize = value; } }
    }

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

        public int ReconnectTimeout { set { internalSession.ConnOptions.ReconnectTimeout = value; } }

        public int ReconnectRetryInterval { set { internalSession.ConnOptions.ReconnectRetryInterval = value; } }

        public AveConnectionOptions ConnOptions { get { return internalSession.ConnOptions; } }

        #region connection control

        [Obsolete]
        public static IAveNetwork Connect(string host, int port, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            return Connect(host, port, false, string.Empty, reconnectTimeOut, reconnectInterval);
        }

        [Obsolete]
        public static IAveNetwork Connect(string host, int port, bool enableSSL, string sslThumbprint, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            AveConnectionOptions connOptions = new AveConnectionOptions();
            connOptions.Host = host;
            connOptions.Port = port;
            connOptions.EnableSSL = enableSSL;
            connOptions.SSLThumbprint = sslThumbprint;
            connOptions.ReconnectTimeout = reconnectTimeOut;
            connOptions.ReconnectRetryInterval = reconnectInterval;
            return Connect(connOptions);
        }

        public static IAveNetwork Connect(AveConnectionOptions connOptions)
        {
            DateTime deadLine = DateTime.Now.AddMilliseconds(connOptions.ReconnectTimeout);
            AveNetworkTrace.TraceInformation("Connect time out {0}.", connOptions.ReconnectTimeout);
            while (true)
            {
                try
                {
                    AveNetworkTrace.TraceVerbose("try to connect to host: {0} Port: {1} Time: {2} Deadline: {3}", connOptions.Host, connOptions.Port, DateTime.Now.ToString(), deadLine.ToString());
                    AveNetworkTransferSession session = AveNetworkConnector.ConnectToServer(connOptions);
                    AveNetworkTrace.TraceVerbose("connect successfully to host: {0} Port: {1} Time: {2} Deadline: {3}", connOptions.Host, connOptions.Port, DateTime.Now.ToString(), deadLine.ToString());
                    return new AveNetwork(session);
                }
                catch (HandShakeException he)
                {
                    AveNetworkTrace.TraceError("Handshake failed. Exception: {0}", he.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("cannot connect to host: {0} Port: {1} Exception: {2}", connOptions.Host, connOptions.Port, e.ToString());
                    if (DateTime.Now > deadLine)
                    {
                        throw new NetworkBrokenException(string.Format("retry connect deadline reached. host: {0} Port: {1} ", connOptions.Host, connOptions.Port), e);
                    }
                    AveNetworkTrace.TraceVerbose("sleep for a while and try again to connect host: {0} Port: {1}", connOptions.Host, connOptions.Port);
                    Thread.Sleep(connOptions.ReconnectRetryInterval);
                }
            }
        }

        [Obsolete]
        public static IAveNetwork ConnectToRawSocket(string host, int port)
        {
            AveConnectionOptions connOptions = new AveConnectionOptions();
            connOptions.Host = host;
            connOptions.Port = port;
            connOptions.ReceiveTimeout = 60 * 60 * 1000;
            connOptions.SendTimeout = 60 * 60 * 1000;
            connOptions.ReceiveBufferSize = 30 * 1024;
            connOptions.SendBufferSize = 30 * 1024;
            return ConnectToRawSocket(connOptions);
        }

        public static IAveNetwork ConnectToRawSocket(AveConnectionOptions connOptions)
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = connOptions.ReceiveTimeout;
                tcpClient.SendTimeout = connOptions.SendTimeout;
                tcpClient.ReceiveBufferSize = connOptions.ReceiveBufferSize;
                tcpClient.SendBufferSize = connOptions.SendBufferSize;
                tcpClient.Connect(connOptions.Host, connOptions.Port);
                return new AveNetworkRawSocket(tcpClient.Client);
            }
            catch (Exception e)
            {
                AveNetworkTrace.TraceError("cannot connect to host: {0} Port: {1} Exception: {2}", connOptions.Host, connOptions.Port, e.ToString());
                throw new NetworkBrokenException(string.Format("connect raw socket exception. host: {0} Port: {1} ", connOptions.Host, connOptions.Port), e);
            }
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

        public int ReceiveBinary(byte[] data, int index, int length)
        {
            return internalSession.ReadBytes(data, index, length, false);
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
            int onceLen = 0;
            int readLen = 0;
            while (readLen < length)
            {
                // continue reading until finish the header
                onceLen = internalSession.ReadBytes(buffer, offset + readLen, length - readLen, true);
                readLen += onceLen;
            }
        }

        #endregion tool helper

        public class AveNetworkRawSocket : IAveNetwork
        {
            private Socket socket;

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

            public int ReceiveBinary(byte[] data, int index, int length)
            {
                int readLen = socket.Receive(data, index, length, SocketFlags.None);
                if (readLen <= 0)
                {
                    //Notes: The caller will also check the return value of this call,
                    AveNetworkTrace.TraceInformation("Zero bytes returned from network reading.");
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
                            throw new ReadEmptyDataFromSocketException();
                        }
                        AveNetworkTrace.TraceWarning("Zero bytes returned from network reading.");
                        Thread.Sleep(100);
                    }
                    readLen += onceLen;
                }
            }
        }
    }
}
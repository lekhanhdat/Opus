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
    using AvePoint.Common;
    using AvePoint.GCommon.Utility;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    #endregion

    public interface IAveNetworkServer
    {
        void Start();

        void Stop();

        void Pause();

        void Resume();

        Int32 ListeningPort { get; }
    }

    public class AveNetworkServer : IAveNetworkServer, IDisposable
    {
        public const Int32 HAND_SHAKE_DATA_LENGTH = 37; //byte[0]: is reconnect? ,byte[1-37] GUID,sessionn id
        static readonly object dictionarySyncRoot = new object();

        NetworkServerState networkServerState;
        Int32 listenPort;
        Int32 reconnectTimeout = 30 * 60 * 1000;
        Int32 reconnectRetryInterval = 30 * 1000;

        IAveNetworkEvent networkEventHandler;
        TcpListener tcpListener;
        Socket serverSocket;
        Thread acceptThread;

        Dictionary<Guid, AveNetworkTransferSession> networkTransferSessionObjectDictionary = new Dictionary<Guid, AveNetworkTransferSession>();
        Dictionary<Guid, String> networkTransferSessionErrorDictionary = new Dictionary<Guid, String>();

        private bool enabledSSL;
        private string sslThumbprint;
        X509Certificate2 localServerCertificate;

        public Int32 ListeningPort { get { return listenPort; } }

        static AveNetworkServer()
        {
            ThreadPool.SetMaxThreads(500, 500);
            ThreadPool.SetMinThreads(10, 10);
        }

        //DocAve.pfx: ef b6 aa a0 3d 17 26 8b ad 4d e3 d4 e0 9f c0 5e 24 c1 b3 c8
        public AveNetworkServer(
            int listenPort,
            IAveNetworkEvent networkEventHandler,
            int reconnectTimeout = 1800000,
            int reconnectInterval = 30000)
            : this(listenPort, networkEventHandler, false, string.Empty, reconnectTimeout, reconnectInterval)
        { }

        public AveNetworkServer(
            int listenPort,
            IAveNetworkEvent networkEventHandler,
            bool enableSSL,
            string sslThumbprint,
            int reconnectTimeout = 1800000,
            int reconnectInterval = 30000)
        {
            this.listenPort = listenPort;
            this.networkEventHandler = networkEventHandler;
            this.enabledSSL = enableSSL;
            this.sslThumbprint = sslThumbprint;
            this.reconnectTimeout = reconnectTimeout;
            this.reconnectRetryInterval = reconnectInterval;
        }

        public void Start()
        {
            AveNetworkTrace.TraceVerbose("AveNetworkServer start listening on port:{0} Time:{1}", this.listenPort, DateTime.Now.ToString());
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        serverSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, 0);
                        serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, this.listenPort));
                        serverSocket.Listen(4);
                    }
                    else
                    {
                        string IPv6Enabled = ConfigurationManager.AppSettings["IPv6Enabled"];
                        if (!string.IsNullOrEmpty(IPv6Enabled) && bool.Parse(IPv6Enabled))
                        {
                            this.tcpListener = new TcpListener(IPAddress.IPv6Any, this.listenPort);
                        }
                        else
                        {
                            this.tcpListener = new TcpListener(IPAddress.Any, this.listenPort);
                        }
                        this.tcpListener.Start();
                    }
                    break;
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("AveNetworkServer start failed. Exception: {0}", e.ToString());
                    if (retryCount++ == 12) throw;
                    Thread.Sleep(10 * 1000);
                }
            }

            this.acceptThread = new Thread(AcceptThread);
            this.acceptThread.Name = "AveNetworkServer AcceptThread";
            this.acceptThread.IsBackground = true;
            this.acceptThread.Start();
            this.networkServerState = NetworkServerState.RUNNING;
        }

        public void Pause()
        {
            this.networkServerState = NetworkServerState.PAUSED;
            Destroy(false);
        }

        public void Resume()
        {
            this.Start();
        }

        public void Stop()
        {
            this.networkServerState = NetworkServerState.STOPPED;
            Destroy(true);
        }

        private void Destroy(bool clearSession)
        {
            if (this.serverSocket != null)
            {
                this.serverSocket.Close();
            }
            if (this.tcpListener != null)
            {
                this.tcpListener.Stop();
            }
            this.acceptThread.Join();
            if (clearSession)
            {
                lock (dictionarySyncRoot)
                {
                    this.networkTransferSessionObjectDictionary.Clear();
                    this.networkTransferSessionErrorDictionary.Clear();
                }
            }
        }

        private void AcceptThread()
        {
            try
            {
                while (true)
                {
                    using (Socket socket = Environment.OSVersion.Version.Major >= 6 ? serverSocket.Accept() : tcpListener.AcceptSocket())
                    {
                        AveNetworkTrace.TraceVerbose("AveNetworkServer accepts socket from:{0} Handle:{1} Time:{2}", socket.RemoteEndPoint.ToString(), socket.Handle.ToString(), DateTime.Now.ToString());
                        Thread t = new Thread(new ParameterizedThreadStart(this.SocketAccepted));
                        t.IsBackground = true;
                        t.Start(socket);
                    }
                }
            }
            catch (Exception se)
            {
                if (this.networkServerState == NetworkServerState.STOPPED || this.networkServerState == NetworkServerState.PAUSED)
                {
                    AveNetworkTrace.TraceVerbose("AveNetworkServer stopped.");
                }
                else
                {
                    AveNetworkTrace.TraceError("An error occurred while accepting on port: {0} . Exception: {1}", this.listenPort, se.ToString());
                }
            }
        }


        private void SocketAccepted(object socketObj)
        {
            Socket socket = null;
            try
            {
                socket = socketObj as Socket;

                Stream socketStream = new NetworkStream(socket, false);
                if (enabledSSL)
                {
                    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, sslThumbprint, false);
                    if (certCollection != null && certCollection.Count != 1)
                    {
                        throw new ArgumentException("can not find certificate.");
                    }
                    ArgumentCheck.NotNull(certCollection, nameof(certCollection));
                    X509Certificate2 serverCertificate = certCollection[0];
                    localServerCertificate = serverCertificate;
                    store.Close();
                    SslStream sslStream = new SslStream(socketStream, false, delegate { return true; });
                    sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Default, false);
                    socketStream = sslStream;
                }

                byte[] handShakeData = new byte[HAND_SHAKE_DATA_LENGTH];
                try
                {
                    AveNetworkTrace.TraceVerbose("try to read hand shake data from socket. Handle: {0}", socket.Handle.ToString());
                    SendReceiveUtility.SafeReceive(socketStream, handShakeData, 0, handShakeData.Length);
                }
                catch (Exception ex)
                {
                    AveNetworkTrace.TraceError("An error occurred while reading hand shake data. {0}", ex.ToString());
                    socketStream.Close();
                    socket.Close();
                    return;
                }

                bool isReconnect = BitConverter.ToBoolean(handShakeData, 0);
                string sessionIdString = Encoding.UTF8.GetString(handShakeData, 1, handShakeData.Length - 1);
                Guid sessionId = new Guid(sessionIdString);
                AveNetworkTrace.TraceVerbose("isReconnect:{0} sessionID:{1} socketHandle:{2}", isReconnect, sessionId, socket.Handle);

                if (this.networkTransferSessionErrorDictionary.ContainsKey(sessionId))
                {
                    //session已经出错退出，无法重连，返回失败信息
                    string errorMsg = this.networkTransferSessionErrorDictionary[sessionId];
                    AveNetworkTrace.TraceError("reconnect session already quit on AveNetworkServer. Message: {0}", errorMsg);
                    SendHandShakeResponse(socketStream, false, errorMsg);
                    socket.Close();
                    lock (dictionarySyncRoot)
                    {
                        networkTransferSessionObjectDictionary.Remove(sessionId);
                        networkTransferSessionErrorDictionary.Remove(sessionId);
                    }
                    return;
                }

                if (isReconnect)
                {
                    if (this.networkTransferSessionObjectDictionary.ContainsKey(sessionId))
                    {
                        //可以重连
                        AveNetworkTrace.TraceVerbose("find session on AveNetworkServer. sessionID: {0}", sessionId);
                        SendHandShakeResponse(socketStream, true, string.Empty);
                        AveNetworkTransferSession oldSession = this.networkTransferSessionObjectDictionary[sessionId];
                        AveNetworkTrace.TraceVerbose("AveNetworkServer try to reset old session socket. sessionID:{0} newSocketHandle:{1}", sessionId, socket.Handle);
                        oldSession.ResetSocket(socket, socketStream);
                        AveNetworkTrace.TraceVerbose("AveNetworkServer finish to reset old session socket. sessionID:{0} newSocketHandle:{1}", sessionId, socket.Handle);
                        return;
                    }
                    else
                    {
                        //不可以重连，session丢失
                        AveNetworkTrace.TraceError("Cannot find session on AveNetworkServer. sessionID: {0}", sessionId);
                        SendHandShakeResponse(socketStream, false, "Cannot find session state.");
                        socket.Close();
                        lock (dictionarySyncRoot)
                        {
                            networkTransferSessionObjectDictionary.Remove(sessionId);
                            networkTransferSessionErrorDictionary.Remove(sessionId);
                        }
                        return;
                    }
                }
                else
                {
                    AveNetworkTrace.TraceVerbose("A new session will be created on AveNetworkServer. sessionID: {0} socketHandle:{1}", sessionId, socket.Handle);
                    SendHandShakeResponse(socketStream, true, string.Empty);

                    AveNetworkTransferSession newSession = new AveNetworkTransferSession();
                    newSession.EnableSSL = enabledSSL;
                    newSession.SSLThumbprint = sslThumbprint;
                    newSession.ReconnectTimeout = reconnectTimeout;
                    newSession.ReconnectRetryInterval = reconnectRetryInterval;
                    AveSocketChannel socketChannel = new AveSocketChannel(socket, socketStream);
                    newSession.Wrap(string.Empty, 0, socketChannel, true, sessionId);
                    lock (dictionarySyncRoot)
                    {
                        this.networkTransferSessionObjectDictionary.Add(sessionId, newSession);
                    }
                    string errorMsg = null;
                    try
                    {
                        AveNetworkTrace.TraceVerbose("AveNetworkServer try to call AveNetworkAccepted process request. sessionID: {0} ", sessionId);
                        AveNetwork network = new AveNetwork(newSession);
                        this.networkEventHandler.AveNetworkAccepted(network);
                        AveNetworkTrace.TraceVerbose("AveNetworkServer finish call AveNetworkAccepted process request. sessionID: {0} ", sessionId);
                        //当AveNetworkServer的AveNetworkAccepted抛出异常，我们会在下一次AveNetwork重练的时候
                        //把这个消息发送给它，AveNetwork收到这个错误后向上层抛出而不是不停的重连。
                    }
                    catch (Exception ex)
                    {
                        AveNetworkTrace.TraceError("An error occurred while doing AveNetworkAccepted process request. sessionID: {0} Exception:{1}", sessionId, ex.ToString());
                        errorMsg = ex.Message;
                        //errorMsg = ex.Message + " Details:" + ex.ToString();
                    }
                    finally
                    {
                        if (!string.IsNullOrEmpty(errorMsg))
                        {
                            lock (dictionarySyncRoot)
                            {
                                this.networkTransferSessionErrorDictionary.Add(sessionId, errorMsg);
                            }
                        }
                        newSession.Close();
                        lock (dictionarySyncRoot)
                        {
                            this.networkTransferSessionObjectDictionary.Remove(sessionId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AveNetworkTrace.TraceError("An error occurred while processing accepted socket. {0}", ex.ToString());
                try
                {
                    if (socket != null && socket.Connected) socket.Close();
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("An error occurred while closing the socket. {0}", e.ToString());
                }
            }
        }



        private static void SendHandShakeResponse(Stream socketStream, bool succeed, string errorMsg)
        {
            byte[] status = BitConverter.GetBytes(succeed);
            SendReceiveUtility.SafeSend(socketStream, status, 0, status.Length);
            if (!succeed)
            {
                byte[] errorMsgBuffer = Encoding.UTF8.GetBytes(errorMsg);
                byte[] errorMsgBufferLength = new byte[4];
                NetworkBytesConverter.ToBigBytes(errorMsgBuffer.Length, errorMsgBufferLength, 0);
                SendReceiveUtility.SafeSend(socketStream, errorMsgBufferLength, 0, errorMsgBufferLength.Length);
                SendReceiveUtility.SafeSend(socketStream, errorMsgBuffer, 0, errorMsgBuffer.Length);
            }
        }

        public void Dispose()
        {
            if(serverSocket != null)
            {
                serverSocket.Dispose();
            }
        }
    }

    public class AveRawNetWorkServer : AveNetworkServer, IAveNetworkServer
    {
        private readonly IAveNetworkEvent networkEventHandler;

        public AveRawNetWorkServer(
            Int32 listenPort,
            IAveNetworkEvent networkEvent)
            : base(listenPort, networkEvent, false, null)
        {
            this.networkEventHandler = networkEvent;
        }

        protected void SocketAccepted(Object socketObj)
        {
            RawSocketAccepted(socketObj);
        }

        protected virtual void RawSocketAccepted(Object socketObj)
        {
            Socket socket = null;
            try
            {
                socket = socketObj as Socket;
                if (socket != null)
                {
                    string errorMsg = null;
                    try
                    {
                        var network = new AvePoint.GCommon.Network.AveNetwork.AveNetworkRawSocket(socket);
                        this.networkEventHandler.AveNetworkAccepted(network);
                    }
                    catch (Exception ex)
                    {
                        AveNetworkTrace.TraceError("An error occurred while doing AveNetworkAccepted process request. Exception:{1}", ex.ToString());
                        errorMsg = ex.Message;
                    }
                    finally
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                AveNetworkTrace.TraceError("An error occurred while processing accepted raw socket. {0}", ex.ToString());
            }
            finally
            {
                try
                {
                    if (socket != null && socket.Connected) socket.Close();
                    
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("An error occurred while closing the raw socket. {0}", e.ToString());
                }
            }
        }
    }

    public enum NetworkServerState
    {
        None,
        RUNNING,
        PAUSED,
        STOPPED
    }

    internal class SendReceiveUtility
    {
        public static void SafeReceive(Stream stream, byte[] buffer, int offset, int length)
        {
            int onceLen = 0;
            int readLen = 0;
            while (readLen < length)
            {
                onceLen = stream.Read(buffer, offset + readLen, length - readLen);
                if (onceLen <= 0)
                {
                    throw new ArgumentException("Read data from a closed connection.");
                }
                readLen += onceLen;
            }
        }

        public static void SafeSend(Stream stream, byte[] buffer)
        {
            SafeSend(stream, buffer, 0, buffer.Length);
        }

        public static void SafeSend(Stream stream, byte[] buffer, int offset, int length)
        {
            stream.Write(buffer, offset, length);
        }
    }
}
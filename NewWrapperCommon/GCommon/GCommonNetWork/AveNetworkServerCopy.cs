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
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;

    #endregion

    public class AveNetworkServerCopy : IAveNetworkServerCopy
    {
        //byte[0]: is reconnect? ,byte[1-37] GUID,session id
        const Int32 HandShakeDataLength = 37;
        static readonly Object dictionarySyncRoot = new object();

        readonly IAveNetworkEvent networkEventHandler;
        readonly AveConnectionOptions connectionOptions;

        readonly Dictionary<Guid, AveNetworkTransferSession> networkTransferSessionDictionary = new Dictionary<Guid, AveNetworkTransferSession>();
        readonly Dictionary<Guid, String> networkTransferSessionErrorDictionary = new Dictionary<Guid, String>();


        public AveNetworkServerCopy(
              IAveNetworkEvent networkEventHandler,
            // ReSharper disable InconsistentNaming
              Boolean enableSSL,
            // ReSharper restore InconsistentNaming
              String sslThumbprint,
              Int32 reconnectTimeout = 1800000,
              Int32 reconnectInterval = 30000)
        {
            this.networkEventHandler = networkEventHandler;
            connectionOptions = new AveConnectionOptions
            {
                EnableSSL = enableSSL,
                SSLThumbprint = sslThumbprint,
                ReconnectTimeout = reconnectTimeout,
                ReconnectRetryInterval = reconnectInterval
            };
        }

        public void SetSocketInformation(SocketInformation socketInformation)
        {
            var duplicatedSocket = new Socket(socketInformation);
            var thread = new Thread(AcceptDuplicatedSocket) { IsBackground = false };
            thread.Start(duplicatedSocket);
        }

        void AcceptDuplicatedSocket(Object socketObj)
        {
            Socket socket = null;
            try
            {
                socket = socketObj as Socket;

                if (socket == null) return;
                Stream socketStream = new NetworkStream(socket, false);
                if (connectionOptions.EnableSSL)
                {
                    var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, connectionOptions.SSLThumbprint, false);
                    if (certCollection.Count != 1)
                    {
                        throw new ArgumentException("can not find certificate.");
                    }
                    var serverCertificate = certCollection[0];
                    store.Close();
                    var sslStream = new SslStream(socketStream, false, delegate { return true; });
                    sslStream.AuthenticateAsServer(serverCertificate, true, (SslProtocols.Default | (SslProtocols)768 | (SslProtocols)3072 | (SslProtocols)3072), false);
                    socketStream = sslStream;
                }

                var handShakeData = new byte[HandShakeDataLength];
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

                var isReconnect = BitConverter.ToBoolean(handShakeData, 0);
                var sessionIdString = Encoding.UTF8.GetString(handShakeData, 1, handShakeData.Length - 1);
                var sessionId = new Guid(sessionIdString);
                AveNetworkTrace.TraceVerbose("isReconnect:{0} sessionID:{1} socketHandle:{2}", isReconnect, sessionId, socket.Handle);

                if (this.networkTransferSessionErrorDictionary.ContainsKey(sessionId))
                {
                    //session已经出错退出，无法重连，返回失败信息
                    var errorMsg = this.networkTransferSessionErrorDictionary[sessionId];
                    AveNetworkTrace.TraceError("reconnect session already quit on AveNetworkServer. Message: {0}", errorMsg);
                    SendHandShakeResponse(socketStream, false, errorMsg);
                    socket.Close();
                    lock (dictionarySyncRoot)
                    {
                        networkTransferSessionDictionary.Remove(sessionId);
                        networkTransferSessionErrorDictionary.Remove(sessionId);
                    }
                    return;
                }

                if (isReconnect)
                {
                    if (this.networkTransferSessionDictionary.ContainsKey(sessionId))
                    {
                        //可以重连
                        AveNetworkTrace.TraceInformation("find session on AveNetworkServer. sessionID: {0}", sessionId);
                        SendHandShakeResponse(socketStream, true, string.Empty);
                        AveNetworkTransferSession oldSession = this.networkTransferSessionDictionary[sessionId];
                        AveNetworkTrace.TraceInformation("AveNetworkServer try to reset old session socket. sessionID:{0} newSocketHandle:{1}", sessionId, socket.Handle);
                        oldSession.ResetSocket(socket, socketStream);
                        AveNetworkTrace.TraceInformation("AveNetworkServer finish to reset old session socket. sessionID:{0} newSocketHandle:{1}", sessionId, socket.Handle);
                    }
                    else
                    {
                        //不可以重连，session丢失
                        AveNetworkTrace.TraceError("Cannot find session on AveNetworkServer. sessionID: {0}", sessionId);
                        SendHandShakeResponse(socketStream, false, "Cannot find session state.");
                        socket.Close();
                        lock (dictionarySyncRoot)
                        {
                            networkTransferSessionDictionary.Remove(sessionId);
                            networkTransferSessionErrorDictionary.Remove(sessionId);
                        }
                    }
                }
                else
                {
                    AveNetworkTrace.TraceVerbose("A new session will be created on AveNetworkServer. sessionID: {0} socketHandle:{1}", sessionId, socket.Handle);
                    SendHandShakeResponse(socketStream, true, string.Empty);

                    var newSession = new AveNetworkTransferSession(connectionOptions);
                    var socketChannel = new AveSocketChannel(socket, socketStream);
                    newSession.Wrap(socketChannel, true, sessionId);
                    lock (dictionarySyncRoot)
                    {
                        this.networkTransferSessionDictionary.Add(sessionId, newSession);
                    }
                    string errorMsg = null;
                    try
                    {
                        AveNetworkTrace.TraceVerbose("AveNetworkServer try to call AveNetworkAccepted process request. sessionID: {0} ", sessionId);
                        var network = new AveNetwork(newSession);
                        this.networkEventHandler.AveNetworkAccepted(network);
                        AveNetworkTrace.TraceVerbose("AveNetworkServer finish call AveNetworkAccepted process request. sessionID: {0} ", sessionId);
                    }
                    catch (Exception ex)
                    {
                        AveNetworkTrace.TraceError("An error occurred while doing AveNetworkAccepted process request. sessionID: {0} Exception:{1}", sessionId, ex.ToString());
                        errorMsg = ex.Message;
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
                            this.networkTransferSessionDictionary.Remove(sessionId);
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

        void SendHandShakeResponse(Stream socketStream, Boolean succeed, String errorMsg)
        {
            var status = BitConverter.GetBytes(succeed);
            SendReceiveUtility.SafeSend(socketStream, status, 0, status.Length);
            if (!succeed)
            {
                var errorMsgBuffer = Encoding.UTF8.GetBytes(errorMsg);
                var errorMsgBufferLength = new Byte[4];
                NetworkBytesConverter.ToBigBytes(errorMsgBuffer.Length, errorMsgBufferLength, 0);
                SendReceiveUtility.SafeSend(socketStream, errorMsgBufferLength, 0, errorMsgBufferLength.Length);
                SendReceiveUtility.SafeSend(socketStream, errorMsgBuffer, 0, errorMsgBuffer.Length);
            }
        }
    }
}
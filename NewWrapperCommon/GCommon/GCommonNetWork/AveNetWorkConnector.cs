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
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace AvePoint.GCommon.Network
{
    internal class AveNetworkConnector
    {
        public static AveNetworkTransferSession ConnectToServer(AveConnectionOptions connOptions)
        {
            TcpClient tcpClient = GetTcpClient(connOptions);
            Socket socket = tcpClient.Client;
            Stream socketStream = GetSocketStream(socket, connOptions);

            Guid sessionId = Guid.NewGuid();
            HandShake(socketStream, sessionId, false);

            AveSocketChannel channel = new AveSocketChannel(socket, socketStream);
            AveNetworkTransferSession session = new AveNetworkTransferSession(connOptions);
            session.Wrap(channel, false, sessionId);
            return session;
        }

        public static void ReConnectToServer(Guid sessionId, AveConnectionOptions connOptions, out Socket socket, out Stream socketStream)
        {
            TcpClient tcpClient = GetTcpClient(connOptions);
            socket = tcpClient.Client;
            socketStream = GetSocketStream(socket, connOptions);

            HandShake(socketStream, sessionId, true);
        }

        private static TcpClient GetTcpClient(AveConnectionOptions connOptions)
        {
            TcpClient tcpClient = new TcpClient(connOptions.Host, connOptions.Port);
            tcpClient.ReceiveTimeout = connOptions.ReceiveTimeout;
            tcpClient.SendTimeout = connOptions.SendTimeout;
            tcpClient.ReceiveBufferSize = connOptions.ReceiveBufferSize;
            tcpClient.SendBufferSize = connOptions.SendBufferSize;
            return tcpClient;
        }

        private static Stream GetSocketStream(Socket socket, AveConnectionOptions connOptions)
        {
            Stream socketStream = new NetworkStream(socket, false);
            if (connOptions.EnableSSL)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, connOptions.SSLThumbprint, false);
                if (certCollection != null && certCollection.Count != 1)
                {
                    throw new ArgumentException("can not find certificate.");
                }
                X509Certificate2 clientCertificate = certCollection[0];
                store.Close();
                X509Certificate2Collection clientCertCollection = new X509Certificate2Collection(new X509Certificate2[] { clientCertificate });
                SslStream sslStream = new SslStream(socketStream, false, new CertificateValidator(clientCertificate).ValidateServerCertificate);
                sslStream.AuthenticateAsClient(string.Empty, clientCertCollection, (SslProtocols.Default | (SslProtocols)768 | (SslProtocols)3072 | (SslProtocols)12288), false);
                socketStream = sslStream;
            }
            return socketStream;
        }

        private static void HandShake(Stream socketStream, Guid sessionId, bool isReconnect)
        {
            bool succeed = false;
            try
            {
                byte[] status = BitConverter.GetBytes(isReconnect);
                byte[] sessionIdBuffer = Encoding.UTF8.GetBytes(sessionId.ToString());
                SendReceiveUtility.SafeSend(socketStream, status, 0, status.Length);
                SendReceiveUtility.SafeSend(socketStream, sessionIdBuffer, 0, sessionIdBuffer.Length);

                byte[] responseStatus = new byte[1];
                SendReceiveUtility.SafeReceive(socketStream, responseStatus, 0, 1);
                succeed = BitConverter.ToBoolean(responseStatus, 0);
            }
            catch (Exception ex)
            {
                throw new HandShakeException("Unexpected handshake exception phrase 1.", ex);
            }
            if (!succeed)
            {
                string errorMsg;
                try
                {
                    byte[] errorMsgLengthBuffer = new byte[4];
                    SendReceiveUtility.SafeReceive(socketStream, errorMsgLengthBuffer, 0, errorMsgLengthBuffer.Length);
                    int errorMsgLength = NetworkBytesConverter.ToBigInt(errorMsgLengthBuffer, 0);
                    byte[] errorMsgBuffer = new byte[errorMsgLength];
                    SendReceiveUtility.SafeReceive(socketStream, errorMsgBuffer, 0, errorMsgBuffer.Length);
                    errorMsg = Encoding.UTF8.GetString(errorMsgBuffer);
                }
                catch (Exception ex)
                {
                    throw new HandShakeException("Unexpected handshake exception phrase 2.", ex);
                }
                throw new HandShakeException(errorMsg);
            }
        }
    }

    internal class CertificateValidator
    {
        X509Certificate2 localClientCertificate;

        public CertificateValidator(X509Certificate2 clientCertificate)
        {
            localClientCertificate = clientCertificate;
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (localClientCertificate == null)
            {
                AveNetworkTrace.TraceError("Local certificate is empty.");
                throw new Exception("Local certificate is empty.");
            }
            if (certificate == null)
            {
                AveNetworkTrace.TraceError("Remote certificate is empty.");
                throw new Exception("Remote certificate is empty.");
            }
            X509Certificate2 remoteCertificate = certificate as X509Certificate2;
            if (localClientCertificate.Thumbprint == remoteCertificate.Thumbprint)
            {
                AveNetworkTrace.TraceVerbose("The certificates are same.");
                return true;
            }
            if (IsBuiltinCertificate(localClientCertificate.Thumbprint)
                && IsBuiltinCertificate(remoteCertificate.Thumbprint))
            {
                AveNetworkTrace.TraceVerbose("The certificates are all built-in.");
                return true;
            }
            X509Chain localChain = new X509Chain();
            localChain.ChainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
            localChain.Build(localClientCertificate);
            if ((chain.ChainElements.Count > 1) && (localChain.ChainElements.Count > 1))
            {
                if (chain.ChainElements[1].Certificate.Thumbprint == localChain.ChainElements[1].Certificate.Thumbprint)
                {
                    AveNetworkTrace.TraceVerbose("The certificates are brothers.");
                    return true;
                }
            }
            AveNetworkTrace.TraceInformation("Local certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", localClientCertificate.Thumbprint, localClientCertificate.Subject, localClientCertificate.Issuer);
            AveNetworkTrace.TraceInformation("Remote certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", remoteCertificate.Thumbprint, remoteCertificate.Subject, remoteCertificate.Issuer);
            AveNetworkTrace.TraceError("Certificate relationship is invalid.");
            return false;
        }

        private bool IsBuiltinCertificate(string thrumbprint)
        {
            var defaults = new string[] 
            {
                BuiltInCertificates.DocAveBuiltInCertificate,
                BuiltInCertificates.DocAveBuiltInCertificateEx,
                BuiltInCertificates.DocAveBuiltInCertificateSHA2
            };

            foreach (var d in defaults)
            {
                if (d.Equals(thrumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

    }
}

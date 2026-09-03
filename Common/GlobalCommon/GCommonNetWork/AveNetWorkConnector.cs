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




using AvePoint.Common;
using AvePoint.GCommon.Utility;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AvePoint.GCommon.Network
{
    internal class AveNetworkConnector
    {
        public static AveNetworkTransferSession ConnectToServer(string host, int port, bool enableSSL, string sslThumbprint)
        {
            using (TcpClient tcpClient = GetTcpClient(host, port))
            {
                Socket socket = tcpClient.Client;
                Stream socketStream = GetSocketStream(socket, enableSSL, sslThumbprint);

                Guid sessionId = Guid.NewGuid();
                HandShake(socketStream, sessionId, false);

                AveSocketChannel channel = new AveSocketChannel(socket, socketStream);
                AveNetworkTransferSession session = new AveNetworkTransferSession();
                session.Wrap(host, port, channel, false, sessionId);
                return session;
            }
        }

        public static void ReConnectToServer(string host, int port, Guid sessionId, bool enableSSL, string sslThumbprint, out Socket socket, out Stream socketStream)
        {
            using (TcpClient tcpClient = GetTcpClient(host, port))
            {
                socket = tcpClient.Client;
                socketStream = GetSocketStream(socket, enableSSL, sslThumbprint);

                HandShake(socketStream, sessionId, true);
            }
        }

        private static TcpClient GetTcpClient(string host, int port)
        {
            TcpClient tcpClient = new TcpClient(host, port);
            tcpClient.ReceiveTimeout = 60 * 60 * 1000;
            tcpClient.SendTimeout = 60 * 60 * 1000;
            tcpClient.ReceiveBufferSize = 30 * 1024;
            tcpClient.SendBufferSize = 30 * 1024;
            return tcpClient;
        }

        private static Stream GetSocketStream(Socket socket, bool enableSSL, string sslThumbprint)
        {
            Stream socketStream = new NetworkStream(socket, false);
            if (enableSSL)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, sslThumbprint, false);
                if (certCollection != null && certCollection.Count != 1)
                {
                    throw new ArgumentException("can not find certificate.");
                }
                ArgumentCheck.NotNull(certCollection, nameof(certCollection));
                X509Certificate2 clientCertificate = certCollection[0];
                store.Close();
                X509Certificate2Collection clientCertCollection = new X509Certificate2Collection(new X509Certificate2[] { clientCertificate });
                SslStream sslStream = new SslStream(socketStream, false, new CertificateValidator(clientCertificate).ValidateServerCertificate);
                sslStream.AuthenticateAsClient(string.Empty, clientCertCollection, SslProtocols.Default, false);
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
                throw new HandShakeException("Error: unexpected handshake exception phrase 1.", ex);
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
                    throw new HandShakeException("Error: unexpected handshake exception phrase 2.", ex);
                }
                throw new HandShakeException("Error: " + errorMsg);
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
            AveNetworkTrace.TraceVerbose("Local certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", localClientCertificate.Thumbprint, localClientCertificate.Subject, localClientCertificate.Issuer);
            AveNetworkTrace.TraceVerbose("Remote certificate Thumbprint:{0} IssueTo:{1} IssueBy:{2}", remoteCertificate.Thumbprint, remoteCertificate.Subject, remoteCertificate.Issuer);
            AveNetworkTrace.TraceError("Certificate relationship is invalid.");
            return false;
        }

    }
}

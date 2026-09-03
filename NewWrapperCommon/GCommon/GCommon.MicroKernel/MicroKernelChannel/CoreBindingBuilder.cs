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



namespace AvePoint.GCommon.MicroKernel
{
    #region using direcives
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    #endregion

    #region Attribute

    /// <summary>
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    #endregion
    public static class CoreBindingBuilder
    {
        static BasicHttpBinding basicHttpBinding;
        static CustomBinding customBinding;

        /// <summary>
        /// 
        /// </summary>
        public static BasicHttpBinding BasicHttpBinding
        {
            get { return basicHttpBinding ?? (basicHttpBinding = BuildBasicHttpBinding()); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static CustomBinding CustomBinding
        {
            get { return customBinding ?? (customBinding = BuildCustomBinding()); }
        }

        static BasicHttpBinding BuildBasicHttpBinding()
        {
            var httpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                Name = "globalDefaultBasicHttpBinding",
                CloseTimeout = new TimeSpan(1, 10, 0),
                OpenTimeout = new TimeSpan(1, 10, 0),
                ReceiveTimeout = new TimeSpan(1, 10, 0),
                SendTimeout = new TimeSpan(1, 10, 0),
                TransferMode = TransferMode.Buffered,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = 0x80000,
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                AllowCookies = true,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                    {
                        MaxDepth = 320,
                        MaxStringContentLength = 2147483647,
                        MaxArrayLength = 2147483647,
                        MaxBytesPerRead = 4096,
                        MaxNameTableCharCount = 65536,
                    }
            };

            httpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            return httpBinding;
        }

        static CustomBinding BuildCustomBinding()
        {
            var transactionFlowBindingElement = new TransactionFlowBindingElement { TransactionProtocol = TransactionProtocol.OleTransactions };
            var binaryMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement
            {
                MaxReadPoolSize = 64,
                MaxSessionSize = 2048,
                MaxWritePoolSize = 16,
            };

            binaryMessageEncodingBindingElement.ReaderQuotas.MaxArrayLength = 2147483647;
            binaryMessageEncodingBindingElement.ReaderQuotas.MaxBytesPerRead = 4096;
            binaryMessageEncodingBindingElement.ReaderQuotas.MaxDepth = 320;
            binaryMessageEncodingBindingElement.ReaderQuotas.MaxNameTableCharCount = 65536;
            binaryMessageEncodingBindingElement.ReaderQuotas.MaxStringContentLength = 2147483647;

            var sslStreamSecurityBindingElement = new SslStreamSecurityBindingElement
            {
                IdentityVerifier = new CoreChannelIdentityVerifier(),
                RequireClientCertificate = true
            };

            var tcpTransportBindingElement = new TcpTransportBindingElement
            {
                ManualAddressing = false,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 2147483647,
                ConnectionBufferSize = 6553600,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                ChannelInitializationTimeout = new TimeSpan(0, 1, 0),
                MaxBufferSize = 2147483647,
                MaxPendingConnections = 10,
                MaxOutputDelay = new TimeSpan(0, 0, 2),
                MaxPendingAccepts = 10,
                PortSharingEnabled = true,
                ListenBacklog = 10,
                TransferMode = TransferMode.Buffered,
                TeredoEnabled = false
            };
            tcpTransportBindingElement.ConnectionPoolSettings.GroupName = "default";
            tcpTransportBindingElement.ConnectionPoolSettings.IdleTimeout = new TimeSpan(0, 2, 0);
            tcpTransportBindingElement.ConnectionPoolSettings.LeaseTimeout = new TimeSpan(0, 5, 0);
            tcpTransportBindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint = 10;

            var binding = new CustomBinding(
                transactionFlowBindingElement,
                binaryMessageEncodingBindingElement,
                sslStreamSecurityBindingElement,
                tcpTransportBindingElement)
            {
                CloseTimeout = new TimeSpan(1, 10, 0),
                OpenTimeout = new TimeSpan(1, 10, 0),
                ReceiveTimeout = new TimeSpan(1, 10, 0),
                SendTimeout = new TimeSpan(1, 10, 0)
            };

            return binding;
        }
    }
}
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
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XSslStreamSecurityUpgradeAcceptor : StreamSecurityUpgradeAcceptor
    {
        private class AcceptUpgradeAsyncResult : XStreamSecurityUpgradeAcceptorAsyncResult
        {
            private XSslStreamSecurityUpgradeAcceptor acceptor;

            private SslStream sslStream;

            //private ChannelBinding channelBindingToken;

            public AcceptUpgradeAsyncResult(XSslStreamSecurityUpgradeAcceptor acceptor, AsyncCallback callback, object state) : base(callback, state)
            {
                this.acceptor = acceptor;
            }
            protected override IAsyncResult OnBegin(Stream stream, AsyncCallback callback)
            {
                this.sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.acceptor.ValidateRemoteCertificate));
                return this.sslStream.BeginAuthenticateAsServer(this.acceptor.parent.ServerCertificate, this.acceptor.parent.RequireClientCertificate, acceptor.parent.SslProtocols, false, callback, this);
            }

            protected override Stream OnCompleteAuthenticateAsServer(IAsyncResult result)
            {
                this.sslStream.EndAuthenticateAsServer(result);
                if (XSecurityUtils.ShouldValidateSslCipherStrength())
                {
                    XSecurityUtils.ValidateSslCipherStrength(this.sslStream.CipherStrength);
                }
                //if (this.acceptor.IsChannelBindingSupportEnabled)
                //{
                //    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                //}
                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.acceptor.clientSecurity;
            }

            public static new Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity /*, out ChannelBinding channelBinding*/)
            {
                Stream result2 = XStreamSecurityUpgradeAcceptorAsyncResult.End(result, out remoteSecurity);
                //channelBinding = ((AcceptUpgradeAsyncResult)result).channelBindingToken;
                //channelBinding = null;
                return result2;
            }
        }

        private SecurityMessageProperty remoteSecurity;

        private bool securityUpgraded;

        private string upgradeString;

        private XSslStreamSecurityUpgradeProvider parent;

        private SecurityMessageProperty clientSecurity;

        private X509Certificate2 clientCertificate;

       // private ChannelBinding channelBindingToken;

        //internal ChannelBinding ChannelBinding
        //{
        //    get
        //    {
        //        return this.channelBindingToken;
        //    }
        //}

        public XSslStreamSecurityUpgradeAcceptor(XSslStreamSecurityUpgradeProvider parent)
        {
            this.upgradeString = "application/ssl-tls";
            this.parent = parent;
            this.clientSecurity = new SecurityMessageProperty();
        }

        public override Stream AcceptUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            Stream result = this.OnAcceptUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return result;
        }

        public override IAsyncResult BeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            return this.OnBeginAcceptUpgrade(stream, callback, state);
        }


        public override Stream EndAcceptUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream result2 = this.OnEndAcceptUpgrade(result, out this.remoteSecurity);
            this.securityUpgraded = true;
            return result2;
        }

        public override bool CanUpgrade(string contentType)
        {
            return !this.securityUpgraded && contentType == this.upgradeString;
        }


        protected IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            AcceptUpgradeAsyncResult acceptUpgradeAsyncResult = new AcceptUpgradeAsyncResult(this, callback, state);
            acceptUpgradeAsyncResult.Begin(stream);
            return acceptUpgradeAsyncResult;
        }

        protected Stream OnEndAcceptUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            return AcceptUpgradeAsyncResult.End(result, out remoteSecurity /*, out this.channelBindingToken*/);
        }


        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (this.clientSecurity.TransportToken != null)
            {
                return this.clientSecurity;
            }
            if (this.clientCertificate != null)
            {
                SecurityToken token = new X509SecurityToken(this.clientCertificate);
                ReadOnlyCollection<IAuthorizationPolicy> readOnlyCollection = XSecurityUtils.NonValidatingX509Authenticator.ValidateToken(token);
                this.clientSecurity = new SecurityMessageProperty();
                this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, readOnlyCollection);
                this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(readOnlyCollection);
                return this.clientSecurity;
            }
            return this.remoteSecurity;
        }

        protected Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            SslStream sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.ValidateRemoteCertificate));
            try
            {
                sslStream.AuthenticateAsServer(this.parent.ServerCertificate, this.parent.RequireClientCertificate, this.parent.SslProtocols, false);
            }
            catch (AuthenticationException ex)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(ex.Message, ex));
            }
            catch (IOException ex2)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(XSR.GetString("NegotiationFailedIO", new object[]
                {
                    ex2.Message
                }), ex2));
            }
            if (XSecurityUtils.ShouldValidateSslCipherStrength())
            {
                XSecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
            }
            remoteSecurity = this.clientSecurity;
            //if (this.IsChannelBindingSupportEnabled)
            //{
            //    this.channelBindingToken = ChannelBindingUtility.GetToken(sslStream);
            //}
            return sslStream;
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (this.parent.RequireClientCertificate)
            {
                if (certificate == null)
                {
                    if (XDiagnosticUtility.ShouldTraceError)
                    {
                        XTraceUtility.TraceEvent(TraceEventType.Error, 262188, this);
                    }
                    return false;
                }
                X509Certificate2 certificate2 = new X509Certificate2(certificate);
                this.clientCertificate = certificate2;
                try
                {
                    SecurityToken token = (SecurityToken)Invoker.CreateNewInstance(typeof(X509SecurityToken), new Type[] { typeof(X509Certificate2), typeof(bool) }, certificate2, false);    //new X509SecurityToken(certificate2, false);
                    ReadOnlyCollection<IAuthorizationPolicy> readOnlyCollection = this.parent.ClientCertificateAuthenticator.ValidateToken(token);
                    this.clientSecurity = new SecurityMessageProperty();
                    this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, readOnlyCollection);
                    this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(readOnlyCollection);
                }
                catch (SecurityTokenException exception)
                {
                    if (XDiagnosticUtility.ShouldTraceInformation)
                    {
                        XDiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    return false;
                }
                return true;
            }
            return true;
        }

    }
}
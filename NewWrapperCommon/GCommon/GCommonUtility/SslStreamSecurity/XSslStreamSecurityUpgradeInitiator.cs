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
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XSslStreamSecurityUpgradeInitiator : StreamSecurityUpgradeInitiator
    {
        private class InitiateUpgradeAsyncResult : XStreamSecurityUpgradeInitiatorAsyncResult
        {
            private X509CertificateCollection clientCertificates;

            private XSslStreamSecurityUpgradeInitiator initiator;

            private LocalCertificateSelectionCallback selectionCallback;

            private SslStream sslStream;

            //private ChannelBinding channelBindingToken;

            public InitiateUpgradeAsyncResult(XSslStreamSecurityUpgradeInitiator initiator, AsyncCallback callback, object state) : base(callback, state)
            {
                this.initiator = initiator;
                if (initiator.clientToken != null)
                {
                    this.clientCertificates = new X509CertificateCollection();
                    this.clientCertificates.Add(initiator.clientToken.Certificate);
                    this.selectionCallback = XSslStreamSecurityUpgradeInitiator.ClientCertificateSelectionCallback;
                }
            }

            protected override IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback)
            {
                this.sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.initiator.ValidateRemoteCertificate), this.selectionCallback);
                IAsyncResult result = null;
                try
                {
                    result = this.sslStream.BeginAuthenticateAsClient(string.Empty, this.clientCertificates, initiator.parent.SslProtocols, false, callback, this);
                }
                catch (SecurityTokenValidationException ex)
                {
                    throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(ex.Message, ex));
                }
                return result;
            }

            protected override Stream OnCompleteAuthenticateAsClient(IAsyncResult result)
            {
                try
                {
                    this.sslStream.EndAuthenticateAsClient(result);
                }
                catch (SecurityTokenValidationException ex)
                {
                    throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(ex.Message, ex));
                }
                if (XSecurityUtils.ShouldValidateSslCipherStrength())
                {
                    XSecurityUtils.ValidateSslCipherStrength(this.sslStream.CipherStrength);
                }
                //if (this.initiator.IsChannelBindingSupportEnabled)
                //{
                //    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                //}
                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.initiator.serverSecurity;
            }

            public static new Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity /*, out ChannelBinding channelBinding*/)
            {
                Stream result2 = XStreamSecurityUpgradeInitiatorAsyncResult.End(result, out remoteSecurity);
                //channelBinding = ((InitiateUpgradeAsyncResult)result).channelBindingToken;
                //channelBinding = null;
                return result2;
            }
        }

        private EndpointAddress remoteAddress;

        private Uri via;

        private SecurityMessageProperty remoteSecurity;

        private bool securityUpgraded;

        private string nextUpgrade;

        private bool isOpen;

        protected EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        protected Uri Via
        {
            get
            {
                return this.via;
            }
        }

        private XSslStreamSecurityUpgradeProvider parent;

        private SecurityMessageProperty serverSecurity;

        private SecurityTokenProvider clientCertificateProvider;

        private X509SecurityToken clientToken;

        private SecurityTokenAuthenticator serverCertificateAuthenticator;

        //private ChannelBinding channelBindingToken;

        private static LocalCertificateSelectionCallback clientCertificateSelectionCallback;

        private static LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get
            {
                if (clientCertificateSelectionCallback == null)
                {
                    clientCertificateSelectionCallback =
                        (object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
                        => { return localCertificates[0]; };
                }
                return clientCertificateSelectionCallback;
            }
        }

        public XSslStreamSecurityUpgradeInitiator(XSslStreamSecurityUpgradeProvider parent, EndpointAddress remoteAddress, Uri via)
        {
            this.parent = parent;
            this.remoteAddress = remoteAddress;
            this.via = via;
            this.nextUpgrade = "application/ssl-tls";

            InitiatorServiceModelSecurityTokenRequirement initiatorServiceModelSecurityTokenRequirement = new InitiatorServiceModelSecurityTokenRequirement();
            initiatorServiceModelSecurityTokenRequirement.TokenType = SecurityTokenTypes.X509Certificate;
            initiatorServiceModelSecurityTokenRequirement.RequireCryptographicToken = true;
            initiatorServiceModelSecurityTokenRequirement.KeyUsage = SecurityKeyUsage.Exchange;
            initiatorServiceModelSecurityTokenRequirement.TargetAddress = remoteAddress;
            initiatorServiceModelSecurityTokenRequirement.Via = via;
            initiatorServiceModelSecurityTokenRequirement.TransportScheme = this.parent.Scheme;
            SecurityTokenResolver securityTokenResolver;
            this.serverCertificateAuthenticator = parent.ClientSecurityTokenManager.CreateSecurityTokenAuthenticator(initiatorServiceModelSecurityTokenRequirement, out securityTokenResolver);
            if (parent.RequireClientCertificate)
            {
                InitiatorServiceModelSecurityTokenRequirement initiatorServiceModelSecurityTokenRequirement2 = new InitiatorServiceModelSecurityTokenRequirement();
                initiatorServiceModelSecurityTokenRequirement2.TokenType = SecurityTokenTypes.X509Certificate;
                initiatorServiceModelSecurityTokenRequirement2.RequireCryptographicToken = true;
                initiatorServiceModelSecurityTokenRequirement2.KeyUsage = SecurityKeyUsage.Signature;
                initiatorServiceModelSecurityTokenRequirement2.TargetAddress = remoteAddress;
                initiatorServiceModelSecurityTokenRequirement2.Via = via;
                initiatorServiceModelSecurityTokenRequirement2.TransportScheme = this.parent.Scheme;
                this.clientCertificateProvider = parent.ClientSecurityTokenManager.CreateSecurityTokenProvider(initiatorServiceModelSecurityTokenRequirement2);
                if (this.clientCertificateProvider == null)
                {
                    throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(XSR.GetString("ClientCredentialsUnableToCreateLocalTokenProvider", new object[]
                    {
                initiatorServiceModelSecurityTokenRequirement2
                    })));
                }
            }
        }

        public override IAsyncResult BeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }
            InitiateUpgradeAsyncResult initiateUpgradeAsyncResult = new InitiateUpgradeAsyncResult(this, callback, state);
            initiateUpgradeAsyncResult.Begin(stream);
            return initiateUpgradeAsyncResult;
        }

        public override Stream EndInitiateUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream result2 = InitiateUpgradeAsyncResult.End(result, out remoteSecurity /*, out this.channelBindingToken*/);
            this.securityUpgraded = true;
            return result2;
        }

        public override string GetNextUpgrade()
        {
            string result = this.nextUpgrade;
            this.nextUpgrade = null;
            return result;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (!this.securityUpgraded)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(XSR.GetString("OperationInvalidBeforeSecurityNegotiation")));
            }
            return this.remoteSecurity;
        }

        public override Stream InitiateUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }
            Stream result = this.OnInitiateUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return result;
        }

        private void Open(TimeSpan timeout)
        {
            XTimeoutHelper timeoutHelper = new XTimeoutHelper(timeout);

            if (this.clientCertificateProvider != null)
            {
                XSecurityUtils.OpenTokenProviderIfRequired(this.clientCertificateProvider, timeoutHelper.RemainingTime());
                this.clientToken = (X509SecurityToken)this.clientCertificateProvider.GetToken(timeoutHelper.RemainingTime());
            }

            this.isOpen = true;
        }

        private Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            X509CertificateCollection x509CertificateCollection = null;
            LocalCertificateSelectionCallback userCertificateSelectionCallback = null;
            if (this.clientToken != null)
            {
                x509CertificateCollection = new X509CertificateCollection();
                x509CertificateCollection.Add(this.clientToken.Certificate);
                userCertificateSelectionCallback = XSslStreamSecurityUpgradeInitiator.ClientCertificateSelectionCallback;
            }
            SslStream sslStream = new SslStream(stream, false, new RemoteCertificateValidationCallback(this.ValidateRemoteCertificate), userCertificateSelectionCallback);
            try
            {
                sslStream.AuthenticateAsClient(string.Empty, x509CertificateCollection, this.parent.SslProtocols, false);
            }
            catch (SecurityTokenValidationException ex)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(ex.Message, ex));
            }
            catch (AuthenticationException ex2)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(ex2.Message, ex2));
            }
            catch (IOException ex3)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(XSR.GetString("NegotiationFailedIO", new object[]
                {
                    ex3.Message
                }), ex3));
            }
            if (XSecurityUtils.ShouldValidateSslCipherStrength())
            {
                XSecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
            }
            remoteSecurity = this.serverSecurity;
            //if (this.IsChannelBindingSupportEnabled)
            //{
            //    this.channelBindingToken = ChannelBindingUtility.GetToken(sslStream);
            //}
            return sslStream;
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            //SecurityToken token = new X509SecurityToken(certificate2, false);
            SecurityToken token = (SecurityToken)Invoker.CreateNewInstance(typeof(X509SecurityToken), new Type[] { typeof(X509Certificate2), typeof(bool) }, certificate2, false);    //new X509SecurityToken(certificate2, false);
            ReadOnlyCollection<IAuthorizationPolicy> readOnlyCollection = this.serverCertificateAuthenticator.ValidateToken(token);
            this.serverSecurity = new SecurityMessageProperty();
            this.serverSecurity.TransportToken = new SecurityTokenSpecification(token, readOnlyCollection);
            this.serverSecurity.ServiceSecurityContext = new ServiceSecurityContext(readOnlyCollection);
            AuthorizationContext authorizationContext = this.serverSecurity.ServiceSecurityContext.AuthorizationContext;
            //this.parent.IdentityVerifier.EnsureOutgoingIdentity(base.RemoteAddress, base.Via, authorizationContext); //no need;
            return true;
        }
    }
}

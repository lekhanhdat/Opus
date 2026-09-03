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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XSslStreamSecurityUpgradeProvider : StreamSecurityUpgradeProvider
    {
        private class OpenAsyncResult : XAsyncResult
        {
            private XSslStreamSecurityUpgradeProvider parent;

            private XTimeoutHelper timeoutHelper;

            private AsyncCallback onOpenTokenAuthenticator;

            private AsyncCallback onOpenTokenProvider;

            private AsyncCallback onGetToken;

            private AsyncCallback onCloseTokenProvider;

            public OpenAsyncResult(XSslStreamSecurityUpgradeProvider parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new XTimeoutHelper(timeout);
                this.onOpenTokenAuthenticator = OnOpenTokenAuthenticator;
                IAsyncResult asyncResult = XSecurityUtils.BeginOpenTokenAuthenticatorIfRequired(parent.ClientCertificateAuthenticator, this.timeoutHelper.RemainingTime(), this.onOpenTokenAuthenticator, this);
                if (!asyncResult.CompletedSynchronously)
                {
                    return;
                }
                if (this.HandleOpenAuthenticatorComplete(asyncResult))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                XAsyncResult.End<XSslStreamSecurityUpgradeProvider.OpenAsyncResult>(result);
            }

            private bool HandleOpenAuthenticatorComplete(IAsyncResult result)
            {
                XSecurityUtils.EndOpenTokenAuthenticatorIfRequired(result);
                if (this.parent.serverTokenProvider == null)
                {
                    return true;
                }
                this.onOpenTokenProvider = OnOpenTokenProvider;
                IAsyncResult asyncResult = XSecurityUtils.BeginOpenTokenProviderIfRequired(this.parent.serverTokenProvider, this.timeoutHelper.RemainingTime(), this.onOpenTokenProvider, this);
                return asyncResult.CompletedSynchronously && this.HandleOpenTokenProviderComplete(asyncResult);
            }

            private bool HandleOpenTokenProviderComplete(IAsyncResult result)
            {
                XSecurityUtils.EndOpenTokenProviderIfRequired(result);
                this.onGetToken = OnGetToken;
                IAsyncResult asyncResult = this.parent.serverTokenProvider.BeginGetToken(this.timeoutHelper.RemainingTime(), this.onGetToken, this);
                return asyncResult.CompletedSynchronously && this.HandleGetTokenComplete(asyncResult);
            }

            private bool HandleGetTokenComplete(IAsyncResult result)
            {
                SecurityToken token = this.parent.serverTokenProvider.EndGetToken(result);
                this.parent.SetupServerCertificate(token);
                this.onCloseTokenProvider = OnCloseTokenProvider;
                IAsyncResult asyncResult = XSecurityUtils.BeginCloseTokenProviderIfRequired(this.parent.serverTokenProvider, this.timeoutHelper.RemainingTime(), this.onCloseTokenProvider, this);
                return asyncResult.CompletedSynchronously && this.HandleCloseTokenProviderComplete(asyncResult);
            }

            private bool HandleCloseTokenProviderComplete(IAsyncResult result)
            {
                XSecurityUtils.EndCloseTokenProviderIfRequired(result);
                this.parent.serverTokenProvider = null;
                return true;
            }

            private void OnOpenTokenAuthenticator(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                Exception exception = null;
                bool flag = false;
                try
                {
                    flag = this.HandleOpenAuthenticatorComplete(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    flag = true;
                    exception = ex;
                }
                if (flag)
                {
                    base.Complete(false, exception);
                }
            }

            private void OnOpenTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                Exception exception = null;
                bool flag = false;
                try
                {
                    flag = this.HandleOpenTokenProviderComplete(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    flag = true;
                    exception = ex;
                }
                if (flag)
                {
                    base.Complete(false, exception);
                }
            }

            private void OnGetToken(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                Exception exception = null;
                bool flag = false;
                try
                {
                    flag = this.HandleGetTokenComplete(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    flag = true;
                    exception = ex;
                }
                if (flag)
                {
                    base.Complete(false, exception);
                }
            }

            private void OnCloseTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                Exception exception = null;
                bool flag = false;
                try
                {
                    flag = this.HandleCloseTokenProviderComplete(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    flag = true;
                    exception = ex;
                }
                if (flag)
                {
                    base.Complete(false, exception);
                }
            }
        }

        static XSslStreamSecurityUpgradeProvider()
        {
            Invoker.AddTypeSearchAssembly(typeof(ChannelFactory).Assembly);
        }

        private SecurityTokenAuthenticator clientCertificateAuthenticator;

        private SecurityTokenManager clientSecurityTokenManager;

        private SecurityTokenProvider serverTokenProvider;

        private EndpointIdentity identity;

        private IdentityVerifier identityVerifier;

        private X509Certificate2 serverCertificate;

        private bool requireClientCertificate;

        private string scheme;

        //private bool enableChannelBinding;

        private SslProtocols sslProtocols;

        public override EndpointIdentity Identity
        {
            get
            {
                if (this.identity == null && this.serverCertificate != null)
                {
                    this.identity = XSecurityUtils.GetServiceCertificateIdentity(this.serverCertificate);
                }
                return this.identity;
            }
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public X509Certificate2 ServerCertificate
        {
            get
            {
                return this.serverCertificate;
            }
        }

        public SecurityTokenAuthenticator ClientCertificateAuthenticator
        {
            get
            {
                if (this.clientCertificateAuthenticator == null)
                {
                    this.clientCertificateAuthenticator = new X509SecurityTokenAuthenticator(XX509ClientCertificateAuthentication.DefaultCertificateValidator);
                }
                return this.clientCertificateAuthenticator;
            }
        }

        public SecurityTokenManager ClientSecurityTokenManager
        {
            get
            {
                return this.clientSecurityTokenManager;
            }
        }

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
        }

        public SslProtocols SslProtocols
        {
            get
            {
                return this.sslProtocols;
            }
        }

        private XSslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenManager clientSecurityTokenManager, bool requireClientCertificate, string scheme, IdentityVerifier identityVerifier, SslProtocols sslProtocols)
            : base(timeouts)
        {
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
            this.clientSecurityTokenManager = clientSecurityTokenManager;
            this.requireClientCertificate = requireClientCertificate;
            this.sslProtocols = sslProtocols;
        }
        private XSslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenProvider serverTokenProvider, bool requireClientCertificate, SecurityTokenAuthenticator clientCertificateAuthenticator, string scheme, IdentityVerifier identityVerifier, SslProtocols sslProtocols)
            : base(timeouts)
        {
            this.serverTokenProvider = serverTokenProvider;
            this.requireClientCertificate = requireClientCertificate;
            this.clientCertificateAuthenticator = clientCertificateAuthenticator;
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
            this.sslProtocols = sslProtocols;
        }

        public override StreamUpgradeAcceptor CreateUpgradeAcceptor()
        {
            base.ThrowIfDisposedOrNotOpen();
            return new XSslStreamSecurityUpgradeAcceptor(this);
        }

        public override StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via)
        {
            base.ThrowIfDisposedOrNotOpen();
            return new XSslStreamSecurityUpgradeInitiator(this, remoteAddress, via);
        }

        protected override void OnAbort()
        {
            if (this.clientCertificateAuthenticator != null)
            {
                XSecurityUtils.AbortTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator);
            }
            this.CleanupServerCertificate();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return XSecurityUtils.BeginCloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new XSslStreamSecurityUpgradeProvider.OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.clientCertificateAuthenticator != null)
            {
                XSecurityUtils.CloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout);
            }
            this.CleanupServerCertificate();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            XSecurityUtils.EndCloseTokenAuthenticatorIfRequired(result);
            this.CleanupServerCertificate();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            XTimeoutHelper timeoutHelper = new XTimeoutHelper(timeout);
            XSecurityUtils.OpenTokenAuthenticatorIfRequired(this.ClientCertificateAuthenticator, timeoutHelper.RemainingTime());
            if (this.serverTokenProvider != null)
            {
                XSecurityUtils.OpenTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
                SecurityToken token = this.serverTokenProvider.GetToken(timeout);
                this.SetupServerCertificate(token);
                XSecurityUtils.CloseTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
                this.serverTokenProvider = null;
            }
        }

        private void SetupServerCertificate(SecurityToken token)
        {
            X509SecurityToken x509SecurityToken = token as X509SecurityToken;
            if (x509SecurityToken == null)
            {
                XSecurityUtils.AbortTokenProviderIfRequired(this.serverTokenProvider);
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(XSR.GetString("InvalidTokenProvided", new object[]
                {
                    this.serverTokenProvider.GetType(),
                    typeof(X509SecurityToken)
                })));
            }
            this.serverCertificate = new X509Certificate2(x509SecurityToken.Certificate);
        }

        private void CleanupServerCertificate()
        {
            if (this.serverCertificate != null)
            {
                this.serverCertificate.Reset();
                this.serverCertificate = null;
            }
        }

        public static XSslStreamSecurityUpgradeProvider CreateClientProvider(XSslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager securityCredentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (securityCredentialsManager == null)
            {
                securityCredentialsManager = new ClientCredentials();
            }
            SecurityTokenManager securityTokenManager = securityCredentialsManager.CreateSecurityTokenManager();
            return new XSslStreamSecurityUpgradeProvider(context.Binding, securityTokenManager, bindingElement.RequireClientCertificate, context.Binding.Scheme, bindingElement.IdentityVerifier, bindingElement.XSslProtocols);
        }

        public static XSslStreamSecurityUpgradeProvider CreateServerProvider(XSslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager securityCredentialsManager = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (securityCredentialsManager == null)
            {
                securityCredentialsManager = new ServiceCredentials();
            }
            SecurityTokenManager securityTokenManager = securityCredentialsManager.CreateSecurityTokenManager();
            RecipientServiceModelSecurityTokenRequirement recipientServiceModelSecurityTokenRequirement = new RecipientServiceModelSecurityTokenRequirement();
            recipientServiceModelSecurityTokenRequirement.TokenType = SecurityTokenTypes.X509Certificate;
            recipientServiceModelSecurityTokenRequirement.RequireCryptographicToken = true;
            recipientServiceModelSecurityTokenRequirement.KeyUsage = SecurityKeyUsage.Exchange;
            recipientServiceModelSecurityTokenRequirement.TransportScheme = context.Binding.Scheme;
            SecurityTokenProvider securityTokenProvider = securityTokenManager.CreateSecurityTokenProvider(recipientServiceModelSecurityTokenRequirement);
            if (securityTokenProvider == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(XSR.GetString("ClientCredentialsUnableToCreateLocalTokenProvider", new object[]
                {
                    recipientServiceModelSecurityTokenRequirement
                })));
            }

            SecurityTokenResolver securityTokenResolver;
            SecurityTokenAuthenticator certificateTokenAuthenticator = securityTokenManager.CreateSecurityTokenAuthenticator(new RecipientServiceModelSecurityTokenRequirement
            {
                TokenType = SecurityTokenTypes.X509Certificate,
                RequireCryptographicToken = true,
                KeyUsage = SecurityKeyUsage.Signature,
                TransportScheme = context.Binding.Scheme,
            }, out securityTokenResolver);

            return new XSslStreamSecurityUpgradeProvider(context.Binding, securityTokenProvider, bindingElement.RequireClientCertificate, certificateTokenAuthenticator, context.Binding.Scheme, bindingElement.IdentityVerifier, bindingElement.XSslProtocols);
        }


    }
}

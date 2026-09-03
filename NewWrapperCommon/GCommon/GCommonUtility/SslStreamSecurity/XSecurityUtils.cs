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
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XSecurityUtils
    {
        private class OpenCommunicationObjectAsyncResult : XAsyncResult
        {
            private ICommunicationObject communicationObject;

            private static AsyncCallback onOpen;

            public OpenCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = (obj as ICommunicationObject);
                bool flag = false;
                if (this.communicationObject == null)
                {
                    flag = true;
                }
                else
                {
                    if (OpenCommunicationObjectAsyncResult.onOpen == null)
                    {
                        OpenCommunicationObjectAsyncResult.onOpen = OpenCommunicationObjectAsyncResult.OnOpen;
                    }
                    IAsyncResult asyncResult = this.communicationObject.BeginOpen(timeout, OpenCommunicationObjectAsyncResult.onOpen, this);
                    if (asyncResult.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(asyncResult);
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                XAsyncResult.End<OpenCommunicationObjectAsyncResult>(result);
            }

            private static void OnOpen(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                OpenCommunicationObjectAsyncResult openCommunicationObjectAsyncResult = (OpenCommunicationObjectAsyncResult)result.AsyncState;
                Exception exception = null;
                try
                {
                    openCommunicationObjectAsyncResult.communicationObject.EndOpen(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    exception = ex;
                }
                openCommunicationObjectAsyncResult.Complete(false, exception);
            }
        }

        private class CloseCommunicationObjectAsyncResult : XAsyncResult
        {
            private ICommunicationObject communicationObject;

            private static AsyncCallback onClose;

            public CloseCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = (obj as ICommunicationObject);
                bool flag = false;
                if (this.communicationObject == null)
                {
                    IDisposable disposable = obj as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                    flag = true;
                }
                else
                {
                    if (CloseCommunicationObjectAsyncResult.onClose == null)
                    {
                        CloseCommunicationObjectAsyncResult.onClose = CloseCommunicationObjectAsyncResult.OnClose;
                    }
                    IAsyncResult asyncResult = this.communicationObject.BeginClose(timeout, CloseCommunicationObjectAsyncResult.onClose, this);
                    if (asyncResult.CompletedSynchronously)
                    {
                        this.communicationObject.EndClose(asyncResult);
                        flag = true;
                    }
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                XAsyncResult.End<CloseCommunicationObjectAsyncResult>(result);
            }

            private static void OnClose(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                XSecurityUtils.CloseCommunicationObjectAsyncResult closeCommunicationObjectAsyncResult = (XSecurityUtils.CloseCommunicationObjectAsyncResult)result.AsyncState;
                Exception exception = null;
                try
                {
                    closeCommunicationObjectAsyncResult.communicationObject.EndClose(result);
                }
                catch (Exception ex)
                {
                    if (XDiagnosticUtility.IsFatal(ex))
                    {
                        throw;
                    }
                    exception = ex;
                }
                closeCommunicationObjectAsyncResult.Complete(false, exception);
            }
        }

        private static Func<bool> shouldValidateSslCipherStrength;
        public static bool ShouldValidateSslCipherStrength()
        {
            if (shouldValidateSslCipherStrength == null)
            {
                var mi = Invoker.GetMethod(typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Security.SecurityUtils"), "ShouldValidateSslCipherStrength", null);
                shouldValidateSslCipherStrength = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), mi);
            }
            return shouldValidateSslCipherStrength();
        }

        private static Action<int> validateSslCipherStrength;
        public static void ValidateSslCipherStrength(int keySizeInBits)
        {
            if (validateSslCipherStrength == null)
            {
                var mi = Invoker.GetMethod(typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Security.SecurityUtils"), "ValidateSslCipherStrength", new Type[] { typeof(int) });
                validateSslCipherStrength = (Action<int>)Delegate.CreateDelegate(typeof(Action<int>), mi);
            }
            validateSslCipherStrength(keySizeInBits);
        }

        internal static void AbortTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator)
        {
            CloseCommunicationObject(tokenAuthenticator, true, TimeSpan.Zero);
        }

        internal static void AbortTokenProviderIfRequired(SecurityTokenProvider tokenProvider)
        {
            CloseCommunicationObject(tokenProvider, true, TimeSpan.Zero);
        }


        private static X509SecurityTokenAuthenticator nonValidatingX509Authenticator;

        internal static X509SecurityTokenAuthenticator NonValidatingX509Authenticator
        {
            get
            {
                if (nonValidatingX509Authenticator == null)
                {
                    nonValidatingX509Authenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                }
                return nonValidatingX509Authenticator;
            }
        }
        internal static EndpointIdentity GetServiceCertificateIdentity(X509Certificate2 certificate)
        {
            EndpointIdentity result;
            using (X509CertificateClaimSet x509CertificateClaimSet = new X509CertificateClaimSet(certificate))
            {
                EndpointIdentity endpointIdentity;
                if (!TryCreateIdentity(x509CertificateClaimSet, ClaimTypes.Dns, out endpointIdentity))
                {
                    TryCreateIdentity(x509CertificateClaimSet, ClaimTypes.Rsa, out endpointIdentity);
                }
                result = endpointIdentity;
            }
            return result;
        }

        private static bool TryCreateIdentity(ClaimSet claimSet, string claimType, out EndpointIdentity identity)
        {
            identity = null;
            using (IEnumerator<Claim> enumerator = claimSet.FindClaims(claimType, null).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    Claim current = enumerator.Current;
                    identity = EndpointIdentity.CreateIdentity(current);
                    return true;
                }
            }
            return false;
        }

        internal static IAsyncResult BeginOpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static IAsyncResult BeginOpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }

        internal static IAsyncResult BeginCloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static IAsyncResult BeginCloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }


        internal static void EndOpenTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndOpenTokenProviderIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndCloseTokenProviderIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void EndCloseTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            CloseTokenAuthenticatorIfRequired(tokenAuthenticator, false, timeout);
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, bool aborted, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenAuthenticator, aborted, timeout);
        }


        internal static void CloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenProvider, false, timeout);
        }

        private static void CloseCommunicationObject(object obj, bool aborted, TimeSpan timeout)
        {
            if (obj != null)
            {
                ICommunicationObject communicationObject = obj as ICommunicationObject;
                if (communicationObject != null)
                {
                    if (aborted)
                    {
                        try
                        {
                            communicationObject.Abort();
                            return;
                        }
                        catch (CommunicationException exception)
                        {
                            if (XDiagnosticUtility.ShouldTraceInformation)
                            {
                                XDiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            }
                            return;
                        }
                    }
                    communicationObject.Close(timeout);
                    return;
                }
                if (obj is IDisposable)
                {
                    ((IDisposable)obj).Dispose();
                }
            }
        }

        internal static void OpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenAuthenticator as ICommunicationObject, timeout);
        }

        internal static void OpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenProvider as ICommunicationObject, timeout);
        }

        private static void OpenCommunicationObject(ICommunicationObject obj, TimeSpan timeout)
        {
            if (obj != null)
            {
                obj.Open(timeout);
            }
        }


    }
}

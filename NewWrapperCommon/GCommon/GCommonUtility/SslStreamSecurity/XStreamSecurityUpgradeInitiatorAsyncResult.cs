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
using System.IO;
using System.Security.Authentication;
using System.ServiceModel.Security;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    abstract class XStreamSecurityUpgradeInitiatorAsyncResult : XAsyncResult
    {
        private Stream originalStream;

        private SecurityMessageProperty remoteSecurity;

        private Stream upgradedStream;

        private static AsyncCallback onAuthenticateAsClient = XDiagnosticUtility.ThunkAsyncCallback(new AsyncCallback(OnAuthenticateAsClient));

        public XStreamSecurityUpgradeInitiatorAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        public void Begin(Stream stream)
        {
            this.originalStream = stream;
            IAsyncResult asyncResult = null;
            try
            {
                asyncResult = this.OnBeginAuthenticateAsClient(this.originalStream, onAuthenticateAsClient);
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
            if (!asyncResult.CompletedSynchronously)
            {
                return;
            }
            this.CompleteAuthenticateAsClient(asyncResult);
            base.Complete(true);
        }

        private void CompleteAuthenticateAsClient(IAsyncResult result)
        {
            try
            {
                this.upgradedStream = this.OnCompleteAuthenticateAsClient(result);
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
            this.remoteSecurity = this.ValidateCreateSecurity();
        }

        public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            XStreamSecurityUpgradeInitiatorAsyncResult streamSecurityUpgradeInitiatorAsyncResult = End<XStreamSecurityUpgradeInitiatorAsyncResult>(result);
            remoteSecurity = streamSecurityUpgradeInitiatorAsyncResult.remoteSecurity;
            return streamSecurityUpgradeInitiatorAsyncResult.upgradedStream;
        }

        private static void OnAuthenticateAsClient(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            XStreamSecurityUpgradeInitiatorAsyncResult streamSecurityUpgradeInitiatorAsyncResult = (XStreamSecurityUpgradeInitiatorAsyncResult)result.AsyncState;
            Exception exception = null;
            try
            {
                streamSecurityUpgradeInitiatorAsyncResult.CompleteAuthenticateAsClient(result);
            }
            catch (Exception ex)
            {
                if (XDiagnosticUtility.IsFatal(ex))
                {
                    throw;
                }
                exception = ex;
            }
            streamSecurityUpgradeInitiatorAsyncResult.Complete(false, exception);
        }

        protected abstract IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback);

        protected abstract Stream OnCompleteAuthenticateAsClient(IAsyncResult result);

        protected abstract SecurityMessageProperty ValidateCreateSecurity();
    }
}

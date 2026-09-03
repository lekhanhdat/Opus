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
using System.Diagnostics;
using System.Threading;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XAsyncResult : IAsyncResult
    {
        private AsyncCallback callback;

        private object state;

        private bool completedSynchronously;

        private bool endCalled;

        private Exception exception;

        private bool isCompleted;

        private ManualResetEvent manualResetEvent;

        private object thisLock;

        private IDisposable callbackActivity;

        public IDisposable CallbackActivity
        {
            get { return callbackActivity; }
            set { callbackActivity = value; }
        }

        public object AsyncState
        {
            get
            {
                return this.state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.manualResetEvent != null)
                {
                    return this.manualResetEvent;
                }
                lock (this.ThisLock)
                {
                    if (this.manualResetEvent == null)
                    {
                        this.manualResetEvent = new ManualResetEvent(this.isCompleted);
                    }
                }
                return this.manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return this.completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        public bool HasCallback
        {
            get
            {
                return this.callback != null;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        protected XAsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        protected void Complete(bool completedSynchronously)
        {
            if (this.isCompleted)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperInternal(false);
            }
            this.completedSynchronously = completedSynchronously;
            if (completedSynchronously)
            {
                this.isCompleted = true;
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }
            if (this.callback != null)
            {
                try
                {
                    using ((this.CallbackActivity == null) ? null : XServiceModelActivity.BoundOperation(this.CallbackActivity))
                    {
                        this.callback(this);
                    }
                }
                catch (Exception innerException)
                {
                    if (XDiagnosticUtility.ShouldTraceWarning)
                    {
                        XTraceUtility.TraceEvent(TraceEventType.Warning, 524289, innerException, null);
                    }
                    if (XDiagnosticUtility.IsFatal(innerException))
                    {
                        throw;
                    }
                    throw XDiagnosticUtility.ExceptionUtility.ThrowHelperCallback(XSR.GetString("AsyncCallbackException"), innerException);
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Complete(completedSynchronously);
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : XAsyncResult
        {
            if (result == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            TAsyncResult tAsyncResult = result as TAsyncResult;
            if (tAsyncResult == null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", XSR.GetString("InvalidAsyncResult"));
            }
            if (tAsyncResult.endCalled)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(XSR.GetString("AsyncObjectAlreadyEnded")));
            }
            tAsyncResult.endCalled = true;
            if (!tAsyncResult.isCompleted)
            {
                tAsyncResult.AsyncWaitHandle.WaitOne();
            }
            if (tAsyncResult.manualResetEvent != null)
            {
                tAsyncResult.manualResetEvent.Close();
            }
            if (tAsyncResult.exception != null)
            {
                throw XDiagnosticUtility.ExceptionUtility.ThrowHelperError(tAsyncResult.exception);
            }
            return tAsyncResult;
        }
    }
}

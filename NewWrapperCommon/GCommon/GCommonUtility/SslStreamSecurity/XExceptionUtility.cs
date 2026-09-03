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

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XExceptionUtility
    {
        private object exceptionUtility;

        private Func<string, Exception, Exception> throwHelperCallback;
        private Func<Exception, TraceEventType, Exception> throwHelper;
        private Action<Exception, TraceEventType> traceHandledException;
        private Func<bool, Exception> throwHelperInternal;
        private static Func<Exception, bool> isFatal;
        private static Type utiType;


        public XExceptionUtility(object exceptionUtility)
        {
            this.exceptionUtility = exceptionUtility;
            utiType = exceptionUtility.GetType();
        }
        internal void TraceHandledException(Exception exception, TraceEventType eventType)
        {
            if (traceHandledException == null)
            {
                traceHandledException = (Action<Exception, TraceEventType>)Delegate.CreateDelegate(typeof(Action<Exception, TraceEventType>), exceptionUtility, "TraceHandledException");
            }
            traceHandledException(exception, eventType);
        }
        internal Exception ThrowHelperCallback(string message, Exception innerException)
        {
            if (throwHelperCallback == null)
            {
                throwHelperCallback = (Func<string, Exception, Exception>)Delegate.CreateDelegate(typeof(Func<string, Exception, Exception>), exceptionUtility, "ThrowHelperCallback");
            }
            return throwHelperCallback(message, innerException);
        }

        internal Exception ThrowHelper(Exception exception, TraceEventType eventType)
        {
            if (throwHelper == null)
            {
                throwHelper = (Func<Exception, TraceEventType, Exception>)Delegate.CreateDelegate(typeof(Func<Exception, TraceEventType, Exception>), exceptionUtility, "ThrowHelper");
            }
            return throwHelper(exception, eventType);
        }

        internal Exception ThrowHelperInternal(bool fatal)
        {
            if (throwHelperInternal == null)
            {
                throwHelperInternal = (Func<bool, Exception>)Delegate.CreateDelegate(typeof(Func<bool, Exception>), exceptionUtility, "ThrowHelperInternal");
            }
            return throwHelperInternal(fatal);
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName)
        {
            return (ArgumentNullException)this.ThrowHelperError(new ArgumentNullException(paramName));
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName, string message)
        {
            return (ArgumentNullException)this.ThrowHelperError(new ArgumentNullException(paramName, message));
        }

        internal Exception ThrowHelperError(Exception exception)
        {
            return this.ThrowHelper(exception, TraceEventType.Error);
        }

        internal ArgumentException ThrowHelperArgument(string message)
        {
            return (ArgumentException)this.ThrowHelperError(new ArgumentException(message));
        }

        internal ArgumentException ThrowHelperArgument(string paramName, string message)
        {
            return (ArgumentException)this.ThrowHelperError(new ArgumentException(message, paramName));
        }
    }
}

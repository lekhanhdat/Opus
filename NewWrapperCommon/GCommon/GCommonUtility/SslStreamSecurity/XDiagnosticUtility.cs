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
using System.ServiceModel;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal class XDiagnosticUtility
    {
        private static Func<bool> shouldUseActivity;
        private static Func<bool> shouldTraceError;
        private static Func<bool> shouldTraceWarning;
        private static Func<bool> shouldTraceInformation;
        private static XExceptionUtility exceptionUtility;
        private static Func<Exception, bool> isFatal;
        private static Func<AsyncCallback, AsyncCallback> thunkAsyncCallback;
        private static Type utilityType;

        static XDiagnosticUtility()
        {
            utilityType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.DiagnosticUtility");
        }

        internal static bool ShouldUseActivity
        {
            get
            {
                if (shouldUseActivity == null)
                {
                    var mi = Invoker.GetMethod(utilityType, "get_ShouldUseActivity", null);
                    shouldUseActivity = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), mi);
                }
                return shouldUseActivity();
            }
        }

        internal static bool ShouldTraceError
        {
            get
            {
                if (shouldTraceError == null)
                {
                    var mi = Invoker.GetMethod(utilityType, "get_ShouldTraceError", null);
                    shouldTraceError = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), mi);
                }
                return shouldTraceError();
            }
        }
        internal static bool ShouldTraceWarning
        {
            get
            {
                if (shouldTraceWarning == null)
                {
                    var mi = Invoker.GetMethod(utilityType, "get_ShouldTraceWarning", null);
                    shouldTraceWarning = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), mi);
                }
                return shouldTraceWarning();
            }
        }

        internal static bool ShouldTraceInformation
        {
            get
            {
                if (shouldTraceInformation == null)
                {
                    var mi = Invoker.GetMethod(utilityType, "get_ShouldTraceInformation", null);
                    shouldTraceInformation = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), mi);
                }
                return shouldTraceInformation();
            }
        }

        internal static XExceptionUtility ExceptionUtility
        {
            get
            {
                if (exceptionUtility == null)
                {
                    exceptionUtility = new XExceptionUtility(Invoker.CallStaticMethod(utilityType, "GetExceptionUtility", null));
                }
                return exceptionUtility;
            }
        }

        internal static bool IsFatal(Exception exception)
        {
            if (isFatal == null)
            {
                var mi = Invoker.GetMethod(utilityType, "IsFatal", new Type[] { typeof(Exception) });
                isFatal = (Func<Exception, bool>)Delegate.CreateDelegate(typeof(Func<Exception, bool>), mi);
            }
            return isFatal(exception);
        }

        internal static AsyncCallback ThunkAsyncCallback(AsyncCallback callback)
        {
            if (thunkAsyncCallback == null)
            {
                var mi = Invoker.GetMethod(utilityType, "ThunkAsyncCallback", new Type[] { typeof(AsyncCallback) });
                thunkAsyncCallback = (Func<AsyncCallback, AsyncCallback>)Delegate.CreateDelegate(typeof(Func<AsyncCallback, AsyncCallback>), mi);
            }
            return thunkAsyncCallback(callback);
        }

    }
}

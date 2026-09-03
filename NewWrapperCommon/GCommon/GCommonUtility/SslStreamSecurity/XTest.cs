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
using System.Threading;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    public class XTest
    {

        public static void TestXServiceModelActivity()
        {
            var cur = XServiceModelActivity.Current;
            XServiceModelActivity.BoundOperation(cur);

        }

        public static void TestXDiagnosticUtility()
        {
            Console.WriteLine(XDiagnosticUtility.ShouldTraceError);
            Console.WriteLine(XDiagnosticUtility.ShouldTraceWarning);
            Console.WriteLine(XDiagnosticUtility.ShouldTraceInformation);
            Console.WriteLine(XDiagnosticUtility.ShouldUseActivity);

            Console.WriteLine(XDiagnosticUtility.IsFatal(new Exception("test")));

            AsyncCallback cb = (result => { Console.WriteLine(result.IsCompleted); });
            XDiagnosticUtility.ThunkAsyncCallback(cb);
            cb(new AyncResultTest());


        }

        public static void TestXExceptionUtility()
        {
            var exceptionUti = XDiagnosticUtility.ExceptionUtility;

            var exception = new Exception("TestMessage");

            exceptionUti.TraceHandledException(exception, System.Diagnostics.TraceEventType.Critical);
            exceptionUti.ThrowHelperCallback("TestMessage2", exception);
            exceptionUti.ThrowHelper(exception, System.Diagnostics.TraceEventType.Error);
            var ex = exceptionUti.ThrowHelperInternal(true);
            Console.WriteLine(ex.ToString());

            var nullEx = exceptionUti.ThrowHelperArgumentNull("Stream");
            Console.WriteLine(nullEx.ToString());

            var nullEx2 = exceptionUti.ThrowHelperArgumentNull("Stream", "StreamMessage");
            Console.WriteLine(nullEx2.ToString());

            var nullEx3 = exceptionUti.ThrowHelperError(exception);
            Console.WriteLine(nullEx3.ToString());

            //Console.WriteLine(XExceptionUtility.IsFatal(ex));
        }

        public static void TestXSecurityUtils()
        {
            Console.WriteLine(XSecurityUtils.ShouldValidateSslCipherStrength());
            XSecurityUtils.ValidateSslCipherStrength(1024);

        }

        public static void TestXSR()
        {
            Console.WriteLine(XSR.GetString("ClientCredentialsUnableToCreateLocalTokenProvider"));
        }
    }

    class AyncResultTest : IAsyncResult
    {
        public object AsyncState
        {
            get
            {
                return new object();
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return new ManualResetEvent(false);
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return true;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return true;
            }
        }
    }
}

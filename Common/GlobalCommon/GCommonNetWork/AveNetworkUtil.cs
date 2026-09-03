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




namespace AvePoint.GCommon.Network
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.Reflection;
    #endregion

    internal class AveNetworkTrace
    {
        static TraceSource commonNetworkTraceSource = new TraceSource("GCommonNetwork");

        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void TraceVerbose(string formatStr, params object[] objs)
        {
            string msg = string.Empty;
            try
            {
                msg = string.Format(formatStr, objs);
            }
            catch (Exception e)
            {
                msg = formatStr + "\t" + e.Message;
            }
            Trace.WriteLine(msg);
            commonNetworkTraceSource.TraceEvent(TraceEventType.Verbose, 0, msg);
            logger.Debug(msg);
        }

        public static void TraceInformation(string formatStr, params object[] objs)
        {
            string msg = string.Empty;
            try
            {
                msg = string.Format(formatStr, objs);
            }
            catch (Exception e)
            {
                msg = formatStr + "\t" + e.Message;
            }
            Trace.WriteLine(msg);
            commonNetworkTraceSource.TraceEvent(TraceEventType.Information, 0, msg);
            logger.Info(msg);
        }

        public static void TraceWarning(string formatStr, params object[] objs)
        {
            string msg = string.Empty;
            try
            {
                msg = string.Format(formatStr, objs);
            }
            catch (Exception e)
            {
                msg = formatStr + "\t" + e.Message;
            }
            Trace.WriteLine(msg);
            commonNetworkTraceSource.TraceEvent(TraceEventType.Warning, 0, msg);
            logger.Warn(msg);
        }

        public static void TraceError(string formatStr, params object[] objs)
        {
            string msg = string.Empty;
            try
            {
                msg = string.Format(formatStr, objs);
            }
            catch (Exception e)
            {
                msg = formatStr + "\t" + e.Message;
            }
            Trace.WriteLine(msg);
            commonNetworkTraceSource.TraceEvent(TraceEventType.Error, 0, msg);
            logger.Error(msg);
        }
    }

    internal class NetworkBytesConverter
    {
        public static int ToBigInt(byte[] buf, int offset)
        {
            int i;
            int a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static uint ToBigUint(byte[] buf, int offset)
        {
            int i;
            uint a = 0;
            for (i = 0; i < 4; i++)
            {
                a <<= 8;
                a += buf[offset++];
            }
            return a;
        }

        public static int ToBigBytes(int a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }

        public static int ToBigBytes(uint a, byte[] buf, int offset)
        {
            buf[offset + 3] = (byte)a;
            a >>= 8;
            buf[offset + 2] = (byte)a;
            a >>= 8;
            buf[offset + 1] = (byte)a;
            a >>= 8;
            buf[offset + 0] = (byte)a;
            a >>= 8;
            return 4;
        }
    }
}
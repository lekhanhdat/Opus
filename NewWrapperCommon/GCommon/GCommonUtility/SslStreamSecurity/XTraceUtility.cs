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
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace AvePoint.GCommon.Utility.SslStreamSecurity
{
    internal static class XTraceUtility
    {
        private static Type utiType;
        private static Action<TraceEventType, int, object> traceEvent1;
        private static Action<TraceEventType, int, Exception, Message> traceEvent2;

        static XTraceUtility()
        {
            utiType = typeof(ChannelFactory).Assembly.GetType("System.ServiceModel.Diagnostics.TraceUtility");
        }
        internal static void TraceEvent(TraceEventType severity, int traceCode, object source)
        {
            if (traceEvent1 == null)
            {
                var mi = Invoker.GetMethod(utiType, "TraceEvent", new Type[] { typeof(TraceEventType), typeof(int), typeof(object) });
                traceEvent1 = (Action<TraceEventType, int, object>)Delegate.CreateDelegate(typeof(Action<TraceEventType, int, object>), mi);
            }
            traceEvent1(severity, traceCode, source);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, Exception exception, Message message)
        {
            if (traceEvent2 == null)
            {
                var mi = Invoker.GetMethod(utiType, "TraceEvent", new Type[] { typeof(TraceEventType), typeof(int), typeof(Exception), typeof(Message) });
                traceEvent2 = (Action<TraceEventType, int, Exception, Message>)Delegate.CreateDelegate(typeof(Action<TraceEventType, int, Exception, Message>), mi);
            }
            traceEvent2(severity, traceCode, exception, message);
        }

    }
}

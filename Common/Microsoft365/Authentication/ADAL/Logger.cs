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


namespace Microsoft365.Authentication.ADAL
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using Microsoft365.Common.Logger;

    internal class ADALLogger
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(ADALLogger));
        internal static string PrepareLogMessage(CallState callState, string classOrComponent, string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1} - {2}: {3}", 
                DateTime.UtcNow,
                (callState != null) ? callState.CorrelationId.ToString() : string.Empty, 
                classOrComponent,
                string.Format(CultureInfo.InvariantCulture, format, args));
        }

        internal static void Verbose(CallState callState, string format, params object[] args)
        {
            logger.Trace(PrepareLogMessage(callState, GetCallerType(), format, args));
        }

        internal static void Information(CallState callState, string format, params object[] args)
        {
            logger.Info(PrepareLogMessage(callState, GetCallerType(), format, args));
        }

        internal static void Warning(CallState callState, string format, params object[] args)
        {
            logger.Warn(PrepareLogMessage(callState, GetCallerType(), format, args));
        }

        internal static void Error(CallState callState, Exception ex)
        {
            logger.Error(PrepareLogMessage(callState, GetCallerType(), "{0}", ex));
        }

        private static string GetCallerType()
        {
            StackFrame stackFrame = new StackFrame(2,false);
            MethodBase method = stackFrame.GetMethod();
            if (!(method.ReflectedType != null))
            {
                return null;
            }
            return method.ReflectedType.Name;
        }
    }
}
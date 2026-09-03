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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Common
{
    public class WrapperLogger
    {
        static WrapperLogger()
        {
            Instance = new AveWrapperLogger();//以后需要替换成本身，这样做成第三方之后就可以让外围实现logger类了。
        }

        public enum Level
        {
            Always,
            Critical,
            Error,
            Warning,
            Info,
            Verbose,
            VerboseEx,
        }

        /// <summary>
        /// Is Logging Level Enabled
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public virtual bool IsLoggingLevelEnabled(Level level)
        {
            return Debugger.IsAttached && level <= Level.VerboseEx;
        }

        public virtual void WriteToLogWithResourceKey(Level level, string key, params object[] args)
        {
            return;
        }

        public static string FormatString(string key, params object[] args)
        {
            return WrapperResource.GetString(key, args);
        }

        /// <summary>
        /// Instance
        /// </summary>
        public static WrapperLogger Instance { get; set; }
    }

    class AveWrapperLogger : WrapperLogger
    {
        readonly static IAveLogger logger = AveLogger.GetInstance(typeof(AveWrapperLogger));

        public override bool IsLoggingLevelEnabled(WrapperLogger.Level level)
        {
            return logger.CurrentLogLevel <= AveLogLevelFromLevel(level);
        }

        public bool IsLoggingLevelEnabled(AveLogLevel level)
        {
            return logger.CurrentLogLevel < level;
        }

        //public override void WriteToLog(Level level, string message)
        //{
        //    logger.Log(AveLogLevelFromLevel(level), message);
        //}

        //public override void WriteToLog(WrapperLogger.Level level, string format, params object[] args)
        //{
        //    var logLevel = AveLogLevelFromLevel(level);

        //    if(IsLoggingLevelEnabled(logLevel))
        //    {
        //        logger.Log(logLevel, format, args);
        //    }
        //}

        public override void WriteToLogWithResourceKey(WrapperLogger.Level level, string key, params object[] args)
        {
            var logLevel = AveLogLevelFromLevel(level);

            if (IsLoggingLevelEnabled(logLevel))
            {
                logger.Log(logLevel, FormatString(key, args));
            }
        }

        static AveLogLevel AveLogLevelFromLevel(Level level)
        {
            switch(level)
            {
                case Level.Always:
                case Level.Critical:
                case Level.Error:
                    return AveLogLevel.ERROR;
                case Level.Info:
                    return AveLogLevel.INFO;
                case Level.Verbose:
                case Level.VerboseEx:
                    return AveLogLevel.DEBUG;
                case Level.Warning:
                    return AveLogLevel.WARN;

            }

            return AveLogLevel.INFO;
        }
    }
}

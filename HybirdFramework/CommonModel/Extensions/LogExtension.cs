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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonModel.Extensions
{
    public static class LogExtension
    {
        #region Debug
        public static void Debug(this ILogger logger,string msg, params object[] args)
        {
            WriteLog(logger, msg, LogLevel.Debug, null, args);
        }

        public static void Debug(this ILogger logger, string msg)
        {
            WriteLog(logger, msg, LogLevel.Debug);
        }
        #endregion

        #region Info
        public static void Info(this ILogger logger, string msg, params object[] args)
        {
            WriteLog(logger, msg, LogLevel.Information, null, args);
        }

        public static void Info(this ILogger logger, string msg)
        {
            WriteLog(logger, msg, LogLevel.Information);
        }
        #endregion

        #region Warn
        public static void Warn(this ILogger logger, string msg, params object[] args)
        {
            WriteLog(logger, msg, LogLevel.Warning, null, args);
        }

        public static void Warn(this ILogger logger, string msg, Exception err)
        {
            WriteLog(logger, msg, LogLevel.Warning, err);
        }
        #endregion

        #region Error
        public static void Error(this ILogger logger, string msg, params object[] args)
        {
            WriteLog(logger, msg, LogLevel.Error, null, args);
        }

        public static void Error(this ILogger logger, string msg, Exception err)
        {
            WriteLog(logger, msg, LogLevel.Error, err);
        }
        #endregion

        #region Trace

        public static void Trace(this ILogger logger, string msg, params object[] args)
        {
            WriteLog(logger, msg, LogLevel.Trace, null, args);
        }

        public static void Trace(this ILogger logger, string msg, Exception err)
        {
            WriteLog(logger, msg, LogLevel.Trace, err);
        }

        #endregion

        private static void WriteLog(ILogger logger,string msg, LogLevel level, Exception e = null, params object[] args)
        {
            //var ei = new LogEventInfo(level, _logger.Name, CultureInfo.CurrentCulture, msg, args, e)
            //{
            //    TimeStamp = DateTime.Now,
            //    Level = level
            //};
            switch(level)
            {
                case LogLevel.Debug:
                    logger.LogDebug(e, msg, args);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(e, msg, args);
                    break;
                case LogLevel.Error:
                    logger.LogError(e, msg, args);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(e, msg, args);
                    break;
                case LogLevel.Trace:
                    logger.LogTrace(e, msg, args);
                    break;
            }
            
        }
    }
}

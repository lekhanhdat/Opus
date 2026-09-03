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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Microsoft.Extensions.Logging;
using System;

namespace AvePoint.Hybrid.Utility
{
    public class HybridCommonLogger : ILogger
    {
        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(HybridCommonLogger));
        public static ILoggerProvider loggerProvider = new HybridProvider();
        public string CategortName { get; private set; }
        class HybridProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new HybridCommonLogger(categoryName);

            }

            public void Dispose()
            { }
        }
        public HybridCommonLogger(string categortName) 
        {
            CategortName = categortName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var ex = exception == null ? "" : exception.ToString();
            var message = formatter != null ? $"{formatter(state, exception)}" : $"{state.ToString()}, {ex}";
            switch (logLevel)
            {
                case LogLevel.Trace:
                    break;
                case LogLevel.Debug:
                    logger.Debug($"{CategortName}, {eventId}, {message}");
                    break;
                case LogLevel.Information:
                    logger.Info($"{CategortName}, {eventId}, {message}");
                    break;
                case LogLevel.Warning:
                    logger.Warn($"{CategortName}, {eventId}, {message}");
                    break;
                case LogLevel.Error:
                    logger.Error($"{CategortName}, {eventId}, {message}");
                    break;
                case LogLevel.Critical:
                    logger.Error($"{CategortName}, {eventId}, {message}");
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}

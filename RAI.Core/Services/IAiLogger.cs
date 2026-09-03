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

namespace AvePoint.RAI.Core.Services
{
    /// <summary>
    /// AI service logger interface
    /// </summary>
    public interface IAiLogger
    {
        /// <summary>
        /// Log information message
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="args">Message format arguments</param>
        void LogInfo(string message, params object[] args);

        /// <summary>
        /// Log warning message
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="args">Message format arguments</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="exception">Exception instance</param>
        /// <param name="args">Message format arguments</param>
        void LogError(string message, Exception? exception = null, params object[] args);

        /// <summary>
        /// Log debug message
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="args">Message format arguments</param>
        void LogDebug(string message, params object[] args);
    }

    /// <summary>
    /// NLog implementation of IAiLogger
    /// </summary>
    public class NLogAiLogger : IAiLogger
    {
        private readonly NLog.ILogger _logger;

        public NLogAiLogger(NLog.ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogInfo(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        public void LogError(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Error(exception, message, args);
            }
            else
            {
                _logger.Error(message, args);
            }
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
    }

    /// <summary>
    /// Console fallback implementation of IAiLogger for backward compatibility
    /// </summary>
    public class ConsoleAiLogger : IAiLogger
    {
        public void LogInfo(string message, params object[] args)
        {
            System.Console.WriteLine($"[INFO] {string.Format(message, args)}");
        }

        public void LogWarning(string message, params object[] args)
        {
            System.Console.WriteLine($"[WARN] {string.Format(message, args)}");
        }

        public void LogError(string message, Exception? exception = null, params object[] args)
        {
            System.Console.WriteLine($"[ERROR] {string.Format(message, args)}");
            if (exception != null)
            {
                System.Console.WriteLine($"[ERROR] Exception: {exception}");
            }
        }

        public void LogDebug(string message, params object[] args)
        {
            System.Console.WriteLine($"[DEBUG] {string.Format(message, args)}");
        }
    }
}

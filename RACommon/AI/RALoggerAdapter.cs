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
using AvePoint.RAI.Core.Services;
using System;

namespace AvePoint.RA.Common.AI
{
    /// <summary>
    /// Adapter to convert RALogger to IAiLogger interface
    /// Bridges the gap between legacy RALogger and modern IAiLogger interface
    /// </summary>
    public class RALoggerAdapter : IAiLogger
    {
        private readonly RALogger _logger;

        /// <summary>
        /// Initializes a new instance of the RALoggerAdapter class
        /// </summary>
        /// <param name="logger">The RALogger instance to wrap</param>
        /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
        public RALoggerAdapter(RALogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message template</param>
        /// <param name="args">Message arguments</param>
        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(string.Format(message, args));
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message">The message template</param>
        /// <param name="args">Message arguments</param>
        public void LogInfo(string message, params object[] args)
        {
            _logger.Info(string.Format(message, args));
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message template</param>
        /// <param name="args">Message arguments</param>
        public void LogWarning(string message, params object[] args)
        {
            _logger.Warn(string.Format(message, args));
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message template</param>
        /// <param name="exception">Optional exception</param>
        /// <param name="args">Message arguments</param>
        public void LogError(string message, Exception exception = null, params object[] args)
        {
            if (exception != null)
            {
                _logger.Error(string.Format(message, args), exception);
            }
            else
            {
                _logger.Error(string.Format(message, args));
            }
        }
    }
}

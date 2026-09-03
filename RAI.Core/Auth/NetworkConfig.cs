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
using System.Net;

namespace AvePoint.RAI.Core.Auth
{
    /// <summary>
    /// Network configuration for Google Cloud authentication
    /// </summary>
    public static class NetworkConfig
    {
        /// <summary>
        /// HTTP proxy configuration (optional)
        /// </summary>
        public static IWebProxy? HttpProxy { get; set; }

        /// <summary>
        /// Connection timeout in milliseconds (default: 30 seconds)
        /// </summary>
        public static int TimeoutMs { get; set; } = 30000;

        /// <summary>
        /// Number of retry attempts for network failures (default: 3)
        /// </summary>
        public static int RetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds (default: 1 second)
        /// </summary>
        public static int RetryDelayMs { get; set; } = 1000;
    }
}

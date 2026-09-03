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
using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.Common
{
    public interface IO365Authentication
    {
        /// <summary>
        /// Version number
        /// </summary>
        Version O365Version { get; }

        /// <summary>
        /// Authentication Mode
        /// </summary>
        O365AuthenticationMode AuthenticationMode { get; }

        /// <summary>
        /// Get for windows Authentication Mode
        /// </summary>
        System.Net.ICredentials Credentials { get; }

        /// <summary>
        /// Cookie Container
        /// </summary>
        System.Net.CookieContainer CookieContainer { get; }

        /// <summary>
        /// Url
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Account Info
        /// </summary>
        O365AccountInfo AccountInfo { get; }

        /// <summary>
        /// Refresh cookie
        /// </summary>
        /// <returns></returns>
        bool RefreshCookie();

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="url"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        bool Login(string url, O365AccountInfo account);
    }
}

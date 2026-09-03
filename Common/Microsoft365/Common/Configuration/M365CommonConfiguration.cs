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


namespace Microsoft365.Common.Configuration
{
    using Microsoft365.Common.Http;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using System;

    internal class M365CommonConfiguration : IM365CommonConfiguration
    { 
        public IWebRequestProvider WebRequestProvider { get; private set; } = new DefaultWebRequestProvider();
        public ILoggerFactory LoggerFactory { get; private set; }
        public string UserAgent { get; private set; }

        public IM365CommonConfiguration AddLoggerFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            return this;
        }

        public IM365CommonConfiguration AddUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            return this;
        }
    }
}
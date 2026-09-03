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
using System.Net.Http;

namespace AvePoint.RA.RACommonUtility.Http
{
    internal class AveHttpClient
    {
        //private static int _timeoutSeconds = 30; //30 secondskk
        private static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMinutes(5);
        private static readonly object lockObj = new object();

        private static HttpClient _httpClient;
        private static HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    lock (lockObj)
                    {
                        if (_httpClient == null)
                        {
                            //_httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
                            _httpClient = new HttpClient() { Timeout = DefaultTimeOut };
                        }
                    }
                }
                return _httpClient;
            }
        }

        /// <summary>
        /// Create a single instance of HttpClient object.
        /// Please do not dispose the instance returned in your code.
        /// </summary>
        /// <returns></returns>
        public static HttpClient Create()
        {
            return Client;
        }

    }
}

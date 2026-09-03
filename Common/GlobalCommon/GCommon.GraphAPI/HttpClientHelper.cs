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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace AvePoint.GCommon.GraphAPI
{
    internal class HttpClientHelper
    {
        //public static readonly HttpClientHandler HttpClientHandler = new HttpClientHandler()
        //{
        //    //AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
        //    AllowAutoRedirect = true
        //};
        public static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(10) };
        public static readonly HttpClient SdkClient = CreateClient();

        public static HttpClient CreateClient() => new HttpClient(new SocketsHttpHandler()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = System.Net.DecompressionMethods.None,
            PooledConnectionLifetime = TimeSpan.FromMinutes(60),
            ConnectTimeout = TimeSpan.FromMinutes(6),
        })//studo
        { Timeout = TimeSpan.FromMinutes(10) };
    }
}
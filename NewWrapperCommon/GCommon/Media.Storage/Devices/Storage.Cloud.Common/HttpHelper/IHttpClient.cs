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
using System.Text;
using System.Net;

namespace AvePoint.Media.Storage.Cloud.Common
{
    interface IHttpClient
    {
        HttpWebResponse Execute(BasicRequest request);

        //new interface
        /// <summary>
        /// Request without method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequest(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// request with GET method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequestGet(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// reqeust with PUT method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequestPut(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// request with POST method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequestPost(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// reqeust with DELETE method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequestDelete(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// request with HEAD method
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequestHead(string url, Dictionary<string, string> queryParams);

        /// <summary>
        /// 加入Headers
        /// </summary>
        /// <param name="request"></param>
        /// <param name="headers"></param>
        void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers);

        void SetUpProxy(HttpWebRequest request, IWebProxy proxy, NetworkCredential credential);
    }
}

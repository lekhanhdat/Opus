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
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class HttpUtil
    {
        public static string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("%2f", "/").Replace("%5c", "/");//make .Net Framework4.5 happy
        }

        public static void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return;
            }
            MethodInfo method = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                new Type[] { typeof(string), typeof(string) }, null);

            foreach (KeyValuePair<string, string> item in headers)
            {
                if (item.Key.Equals("Content-Length"))
                {
                    request.ContentLength = Convert.ToInt64(item.Value);
                }

                method.Invoke(request.Headers, new object[] { item.Key, item.Value });
            }
        }

        public static string CombiningQueryParams(string baseURL, Dictionary<string, string> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
            {
                return baseURL;
            }
            StringBuilder builder = new StringBuilder(baseURL);


            bool first = true;

            foreach (KeyValuePair<string, string> item in queryParams)
            {
                if (first)
                {
                    if (!baseURL.Contains("?"))
                    {
                        builder.Append("?");
                    }
                    else
                    {
                        builder.Append("&");
                    }
                    first = false;
                }

                else
                {
                    builder.Append("&");
                }

                builder.Append(Encode(item.Key))
                       .Append("=")
                       .Append(Encode(item.Value));
            }
            return builder.ToString();
        }

    }
}

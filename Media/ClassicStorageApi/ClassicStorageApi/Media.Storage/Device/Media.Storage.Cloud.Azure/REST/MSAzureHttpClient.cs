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




namespace AvePoint.Media.ClassicStorage.Cloud.Azure.REST
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper;
    using System.Net;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Request;
    using System.Reflection;
    using AvePoint.GCommon.Utility;
    #endregion

    class MSAzureHttpClient : AbstractHttpClient
    {
        public override HttpWebRequest GetHttpWebRequest(BasicRequest request)
        {
            MSAzureRequest azureRequest = request as MSAzureRequest;
            if (azureRequest != null)
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(SecurityUtils.SanitizeRequestUrl(azureRequest.URI));
                webRequest.Method = azureRequest.Method;
                AddHeaders(webRequest, azureRequest.Headers);
                MSAzureUtils.signRequest(webRequest, azureRequest);
                return webRequest;
            }
            throw new InvalidOperationException();
        }

        public override void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            base.CombiningRequestWithHeaders(request, headers);
            MSAzureUtils.signRequest(request, OpenParam);
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;
            request.Timeout = StorageConstants.DefaultHttpRequestTimeout; //1 hour
        }

        //为了处理range而特意重写的方法
        public override void AddHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            MethodInfo method = request.Headers.GetType().GetMethod("AddWithoutValidate",
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                new Type[] { typeof(string), typeof(string) }, null);

            if (headers.ContainsKey("Range"))
            {
                string tempValue = headers["Range"];
                headers.Remove("Range");
                headers.Add("x-ms-range", tempValue);
            }
            foreach (KeyValuePair<string, string> item in headers)
            {
                if (item.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    request.ContentLength = Convert.ToInt64(item.Value);
                    //continue;
                }

                method.Invoke(request.Headers, new object[] { item.Key, item.Value });
            }
        }
    }
}

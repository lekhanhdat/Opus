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




namespace AvePoint.Media.ClassicStorage.Cloud.Common.HttpHelper
{

    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.IO;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Request;
    using System.Reflection;
    using AvePoint.GCommon;
    using System.Web;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Config;
    using AvePoint.Media.ClassicStorage.Util;
    using System.Diagnostics;
    using AvePoint.GCommon.Utility;

    //using AvePoint.GCommon.Contract.CodeReview;
    #endregion

    //#region CodeReview
    //[AveCodeReview(
    //"2012/3/22",
    //"rongbiao.sun@avepoint.com",
    //"yanxin.fu@avepoint.com",
    // new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_10 },
    //"ADO-28237",
    // true)]
    //#endregion
    public abstract class AbstractHttpClient : IHttpClient
    {
        public CloudOpenParameter OpenParam { get; set; }
        public TimeSpan TimeOffset { get; set; }
        public AbstractXSystem CurrentSystem{get;set;}
        //private static int requestCount;
        private static AveLogger logger = AveLogger.GetInstance(typeof(AbstractHttpClient));
        public HttpWebResponse Execute(BasicRequest request)
        {
            HttpWebResponse response = null;
            HttpWebRequest webRequest = null;
            try
            {
                webRequest = GetHttpWebRequest(request);
                webRequest.AllowWriteStreamBuffering = false;
                webRequest.AllowAutoRedirect = false;
                webRequest.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout
                if (RESTCommands.PUT.Equals(webRequest.Method) || RESTCommands.POST.Equals(webRequest.Method))
                {
                    if (request.DataStream != null)
                    {
                        using (Stream httpWebRequestStream = webRequest.GetRequestStream())
                        {
                            byte[] buffer = new byte[64 * 1024];
                            using (Stream dataStream = request.DataStream)
                            {
                                while (true)
                                {
                                    int readLen = dataStream.Read(buffer, 0, buffer.Length);
                                    if (readLen <= 0) break;
                                    httpWebRequestStream.Write(buffer, 0, readLen);
                                }
                            }
                        }
                    }
                }
                response = webRequest.GetResponse() as HttpWebResponse;
                CalcDataFlow(webRequest,response);
                //requestCount++;
                //logger.Info("Request Count : " + requestCount);
                
            }
            catch (WebException e)
            {
                Trace.TraceWarning(e.ToString());
                if (webRequest != null)
                {
                    webRequest.Abort();
                }
                throw ;
            }
            return response;

        }

        public HttpWebResponse UpLoad(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message,ex);
                throw;
            }
            return response;
        }

        public HttpWebRequest GetWebRequestForUpLoad(BasicRequest request)
        {
            HttpWebRequest webRequest = null;
            try
            {
                webRequest = GetHttpWebRequest(request);
                webRequest.AllowWriteStreamBuffering = false;
                webRequest.AllowAutoRedirect = false;
                webRequest.Timeout = StorageConstants.DefaultHttpRequestTimeout; //never timeout
            }
            catch (Exception e)
            {
                logger.Error(e.Message,e);
                throw ;
            }
            return webRequest;

        }
        
        public virtual void AddHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            MethodInfo method = request.Headers.GetType().GetMethod("AddWithoutValidate", 
                                BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance, null,
                                new Type[] { typeof(string), typeof(string) }, null);

            foreach (KeyValuePair<string, string> item in headers)
            {
                if (item.Key.Equals("Content-Length"))
                {
                    request.ContentLength = Convert.ToInt64(item.Value);
                    //continue;
                } 

                method.Invoke(request.Headers, new object[] { item.Key, item.Value });
            }
        }

        public string Encode(string str2Encode)
        {
            return HttpUtility.UrlEncode(str2Encode).Replace("+", "%20").Replace("/","%2F");
        }

        public string CombiningQueryParams(string baseURL, Dictionary<string, string> queryParams)
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
                    builder.Append("?");
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

        public abstract HttpWebRequest GetHttpWebRequest(BasicRequest request);


        public void CalcDataFlow(HttpWebRequest webRequest, HttpWebResponse response) 
        {
            if (RESTCommands.PUT.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_PUT, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.POST.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_POST, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.DELETE.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_DELETE, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.GET.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_GET, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.HEAD.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_HEAD, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.COPY.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_COPY, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
            else if (RESTCommands.LIST.Equals(webRequest.Method))
            {
                CurrentSystem.IncreaseValue(SystemPropertyKeys.REQUEST_LIST, 1);
                CommonCalcDataFlowCode(webRequest, response);
            }
        }

        private void CommonCalcDataFlowCode(HttpWebRequest webRequest, HttpWebResponse response)
        {
            long responseContentLength = response.ContentLength == -1 ? 0 : response.ContentLength;
            long requestContentLength = webRequest.ContentLength == -1 ? 0 : webRequest.ContentLength;

            CurrentSystem.IncreaseValue(SystemPropertyKeys.DATA_TRANSFER_IN, CalcRequestHeaderLength(webRequest));
            CurrentSystem.IncreaseValue(SystemPropertyKeys.DATA_TRANSFER_IN, requestContentLength);
            CurrentSystem.IncreaseValue(SystemPropertyKeys.DATA_TRANSFER_OUT, CalcResponseHeaderLength(response));
            CurrentSystem.IncreaseValue(SystemPropertyKeys.DATA_TRANSFER_OUT, responseContentLength);
        }
        private long CalcRequestHeaderLength(HttpWebRequest webRequest) 
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < webRequest.Headers.Count;i++ )
            {
                string headerKey=webRequest.Headers.GetKey(0);
                string[] headerValues=webRequest.Headers.GetValues(headerKey);
                sb.Append(headerKey)
                  .Append(":")
                  .Append(headerValues[0]);
            }
            return sb.ToString().Length;
        }

        private long CalcResponseHeaderLength(HttpWebResponse response)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < response.Headers.Count; i++)
            {
                string headerKey = response.Headers.GetKey(0);
                string[] headerValues = response.Headers.GetValues(headerKey);
                sb.Append(headerKey)
                  .Append(":")
                  .Append(headerValues[0]);
            }
            return sb.ToString().Length;
        }


        //new interfaces
        #region new interfaces
        public HttpWebRequest CreateRequest(string url, Dictionary<string, string> queryParams)
        {
            string finalURL = CombiningQueryParams(url, queryParams);
            return WebRequest.Create(SecurityUtils.SanitizeRequestUrl(finalURL)) as HttpWebRequest;
        }

        public HttpWebRequest CreateRequestGet(string url, Dictionary<string, string> queryParams)
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.GET;
            return request;
        }

        public HttpWebRequest CreateRequestPut(string url, Dictionary<string, string> queryParams)
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.PUT;
            return request;
        }

        public HttpWebRequest CreateRequestCopy(string url, Dictionary<string, string> queryParams) 
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.COPY;
            return request;
        }

        public HttpWebRequest CreateRequestPost(string url, Dictionary<string, string> queryParams)
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.POST;
            return request;
        }

        public HttpWebRequest CreateRequestDelete(string url, Dictionary<string, string> queryParams)
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.DELETE;
            return request;
        }

        public HttpWebRequest CreateRequestHead(string url, Dictionary<string, string> queryParams)
        {
            HttpWebRequest request = this.CreateRequest(url, queryParams);
            request.Method = RESTCommands.HEAD;
            return request;
        }

        public virtual void CombiningRequestWithHeaders(HttpWebRequest request, Dictionary<string, string> headers)
        {
            AddHeaders(request, headers);
        }

        public void SetUpProxy(HttpWebRequest request, IWebProxy proxy, NetworkCredential credential)
        {
            if (proxy != null)
            {
                if (credential != null)
                {
                    proxy.Credentials = credential;
                }
                request.Proxy = proxy;
            }
        }
        #endregion

    }
}

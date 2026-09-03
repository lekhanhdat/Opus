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
using AvePoint.Wrapper.Common;
using Microsoft365.Authentication;
using Microsoft365.SharePoint.Extension;
namespace ExchangeUtility.Graph.SharePointRestAPI
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    abstract class SharePointRestBase<T>
    {
        private const int BUFFER_LENGTH = 64 * 1024;
        public const string METHOD_POST = "POST";

        protected virtual event RequestFailedEventHandler OnRequestFailed;
        public string SiteUrl { get;private set; }
        public ITokenProvider TokenProvider { get; private set; }

        protected string restBaseUrl;

        public int RequestTimeout { get; set; } = 600000;
        public int ReadWriteTimeout { get; set; } = 1800000;

        public abstract string RequestUrl { get; }

        public abstract string RequestMethod { get;  }

        public abstract Stream PostRequestStream { get; }

        public SharePointRestBase(string siteUrl, ITokenProvider tokenProvider)
        {
            this.SiteUrl = siteUrl.TrimEnd('/');
            this.TokenProvider = tokenProvider;
            this.restBaseUrl = $"{SiteUrl}{EndPoint.APIRoot}";
        }

        public object Execute()
        {
            var webRequest = HttpPostRequest(this.RequestUrl, this.PostRequestStream.Length);
            if (this.IsHttpPost)
            {
                using (Stream to = webRequest.GetRequestStream())
                {
                    this.PostRequestStream.CopyTo(to, BUFFER_LENGTH);
                }
            }

            return GetResult(webRequest);
        }

        private bool IsHttpPost
        {
            get
            {
                return string.Equals(this.RequestMethod, METHOD_POST, StringComparison.OrdinalIgnoreCase);
            }
        }

        protected virtual void ValidateArguments() { }

        protected virtual object GetResult(WebRequest webRequest)
        {
            using (var res = webRequest.GetResponse())
            {
                if (this.IsHttpPost) return new EmptyObject();
                using (var stream = res.GetResponseStream())
                {
                    return JsonDeserializer(new StreamReader(stream).ReadToEnd());
                }
            }
        }

        protected static Stream ConvertToMMStream(string body)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(body));
        }

        public T JsonDeserializer(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        private WebRequest HttpPostRequest(string requestUrl, long contentLength)
        {
            var webRequest = ReliableHttpWebRequest.CreateRequest(requestUrl);

            webRequest.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            webRequest.RequestFailed += OnRequestFailed;
            webRequest.SetTokenProvider(this.SiteUrl, this.TokenProvider);
            webRequest.ContentLength = contentLength;

            webRequest.Accept = "application/json;odata=verbose";
            webRequest.ContentType = "application/json;odata=verbose";
            webRequest.Method = METHOD_POST;
            webRequest.Timeout = RequestTimeout;
            webRequest.ReadWriteTimeout = ReadWriteTimeout;
            webRequest.AllowWriteStreamBuffering = false;
            return webRequest;
        }

        public static class EndPoint
        {
            public const string APIRoot = "/_api";
            public static string AddFile(string fileName, string overwrite)
            {
                return $"/files/add(overwrite={overwrite}, url='{fileName}')";
            }

            public static string GetFolder(string folderServerRelatedUrl)
            {
                return $"/web/getfolderbyserverrelativeurl('{folderServerRelatedUrl}')";
            }

            public static string CreateFolder()
            {
                return $"/web/folders";
            }
        }
    }
}
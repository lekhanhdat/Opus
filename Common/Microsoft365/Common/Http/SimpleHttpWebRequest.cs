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
namespace Microsoft365.Common.Http
{
    using Microsoft365.Common.Extension;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    class SimpleHttpWebRequest : IHttpWebRequest2
    {
        private readonly HttpWebRequest request;

        public SimpleHttpWebRequest(HttpWebRequest request)
        {
            request.ArgumentNullValidation("request");

            this.request = request;
        }

        public CookieContainer CookieContainer { get { return request.CookieContainer; } set { request.CookieContainer = value; } }
        public int ReadWriteTimeout { get { return request.ReadWriteTimeout; } set { request.ReadWriteTimeout = value; } }
        public RequestCachePolicy CachePolicy { get { return request.CachePolicy; } set { request.CachePolicy = value; } }
        public string ConnectionGroupName { get { return request.ConnectionGroupName; } set { request.ConnectionGroupName = value; } }
        public long ContentLength { get { return request.ContentLength; } set { request.ContentLength = value; } }
        public string ContentType { get { return request.ContentType; } set { request.ContentType = value; } }
        public ICredentials Credentials { get { return request.Credentials; } set { request.Credentials = value; } }
        public WebHeaderCollection Headers { get { return request.Headers; } set { request.Headers = value; } }
        public string Method { get { return request.Method; } set { request.Method = value; } }
        public bool PreAuthenticate { get { return request.PreAuthenticate; } set { request.PreAuthenticate = value; } }
        public bool KeepAlive { get { return request.KeepAlive; } set { request.KeepAlive = value; } }
        public IWebProxy Proxy { get { return request.Proxy; } set { request.Proxy = value; } }
        public Uri RequestUri { get { return request.RequestUri; } }
        public int Timeout { get { return request.Timeout; } set { request.Timeout = value; } }
        public bool UseDefaultCredentials { get { return request.UseDefaultCredentials; } set { request.UseDefaultCredentials = value; } }
        public string UserAgent { get { return request.UserAgent; } set { request.UserAgent = value; } }
        public bool AllowAutoRedirect { get { return request.AllowAutoRedirect; } set { request.AllowAutoRedirect = value; } }
        public string Accept { get { return request.Accept; } set { request.Accept = value; } }
        public X509CertificateCollection ClientCertificates { get { return request.ClientCertificates; } set { request.ClientCertificates = value; } }
        public AuthenticationLevel AuthenticationLevel { get { return request.AuthenticationLevel; } set { request.AuthenticationLevel = value; } }
        public string Host { get { return request.Host; } set { request.Host = value; } }
        public DecompressionMethods AutomaticDecompression { get { return request.AutomaticDecompression; } set { request.AutomaticDecompression = value; } }
        public HttpWebRequest WebRequest { get { return request; } }

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return request.BeginGetRequestStream(callback, state);
        }

        public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return request.BeginGetResponse(callback, state);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return request.EndGetRequestStream(asyncResult);
        }

        public WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return request.EndGetResponse(asyncResult);
        }

        public Stream GetRequestStream()
        {
            return request.GetRequestStream();
        }

        public WebResponse GetResponse()
        {
            return request.GetResponse();
        }
    }
}
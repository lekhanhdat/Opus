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


namespace Microsoft.Exchange.WebServices.Data
{
    using System;
    using System.Net;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Diagnostics;

    [DebuggerNonUserCode]
    class BufferFreeEwsHttpWebRequestFactory : IEwsHttpWebRequestFactory
    {
        public IEwsHttpWebResponse CreateExceptionResponse(WebException exception)
        {
            if (exception.Response == null) throw new InvalidOperationException("The exception does not contain response.");
            return new BufferFreeEwsHttpWebResponse(exception.Response as HttpWebResponse);
        }

        public IEwsHttpWebRequest CreateRequest(Uri uri)
        {
            return new BufferFreeEwsHttpWebRequest(uri);
        }
    }

    /// <summary>
    /// Disable HttpRequest buffer
    /// </summary>
    [DebuggerNonUserCode]
    class BufferFreeEwsHttpWebRequest : IEwsHttpWebRequest
    {
        public string Accept
        {
            get
            {
                return this.request.Accept;
            }
            set
            {
                this.request.Accept = value;
            }
        }
        public bool AllowAutoRedirect
        {
            get
            {
                return this.request.AllowAutoRedirect;
            }
            set
            {
                this.request.AllowAutoRedirect = value;
            }
        }

        public X509CertificateCollection ClientCertificates
        {
            get
            {
                return this.request.ClientCertificates;
            }
            set
            {
                this.request.ClientCertificates = value;
            }
        }
        public string ConnectionGroupName
        {
            get
            {
                return this.request.ConnectionGroupName;
            }
            set
            {
                this.request.ConnectionGroupName = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this.request.ContentType;
            }
            set
            {
                this.request.ContentType = value;
            }
        }

        public CookieContainer CookieContainer
        {
            get
            {
                return this.request.CookieContainer;
            }
            set
            {
                this.request.CookieContainer = value;
            }
        }

        public ICredentials Credentials
        {
            get
            {
                return this.request.Credentials;
            }
            set
            {
                this.request.Credentials = value;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.request.Headers;
            }
            set
            {
                this.request.Headers = value;
            }
        }

        public bool KeepAlive
        {
            get
            {
                return this.request.KeepAlive;
            }
            set
            {
                this.request.KeepAlive = value;
            }
        }
        public string Method
        {
            get
            {
                return this.request.Method;
            }
            set
            {
                this.request.Method = value;
            }
        }
        public bool PreAuthenticate
        {
            get
            {
                return this.request.PreAuthenticate;
            }
            set
            {
                this.request.PreAuthenticate = value;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return this.request.Proxy;
            }
            set
            {
                this.request.Proxy = value;
            }
        }

        public Uri RequestUri
        {
            get
            {
                return this.request.RequestUri;
            }
        }

        public int Timeout
        {
            get
            {
                return this.request.Timeout;
            }
            set
            {
                this.request.Timeout = value;
            }
        }

        public bool UseDefaultCredentials
        {
            get
            {
                return this.request.UseDefaultCredentials;
            }
            set
            {
                this.request.UseDefaultCredentials = value;
            }
        }

        public string UserAgent
        {
            get
            {
                return this.request.UserAgent;
            }
            set
            {
                this.request.UserAgent = value;
            }
        }
        private HttpWebRequest request;

        public BufferFreeEwsHttpWebRequest(Uri uri)
        {
            this.request = (HttpWebRequest)WebRequest.Create(uri);
        }


        public void Abort()
        {
            this.request.Abort();
        }

        public IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            DoPreAuthentication();
            return this.request.BeginGetRequestStream(callback, state);
        }

        /// <summary>
        /// Pre authenticate and disable request buffer.
        /// Pre authenticate may not work for all kind of authentication method, only basic authentication(user\passoword, OAuth access token) is well tested.
        /// </summary>
        private void DoPreAuthentication()
        {
            //https://support.microsoft.com/en-us/kb/908573
            //Clone current request obj, and send pre authentication request with HEAD method
            var preAuthenticationRequest = SendPreAuthenticationRequest();
            //Copy authorization header from pre authentication request to current request.
            CopyAuthorizationHeaders(preAuthenticationRequest, this.request);
            //Disable request write buffer and enable send chunked
            DisableWriteStreamBuffer();
        }

        private HttpWebRequest SendPreAuthenticationRequest()
        {
            var request2 = CloneCurrentRequest();
            request2.Method = "HEAD";
            request2.PreAuthenticate = true;
            request2.AllowAutoRedirect = false;
            using (request2.GetResponse()) { }

            return request2;
        }

        private void DisableWriteStreamBuffer()
        {
            this.request.AllowWriteStreamBuffering = false;
            this.request.SendChunked = true;
        }

        private static void CopyAuthorizationHeaders(HttpWebRequest from, HttpWebRequest to)
        {
            if (from.Headers != null)
            {
                //only copy header: Authorization, ProxyAuthorization
                CopyHeader(from, to, HttpRequestHeader.Authorization);
                CopyHeader(from, to, HttpRequestHeader.ProxyAuthorization);
            }
        }

        private static void CopyHeader(HttpWebRequest from, HttpWebRequest to, HttpRequestHeader headerName)
        {
            var source = from.Headers[headerName];
            var target = to.Headers[headerName];
            if (string.IsNullOrEmpty(target) && !string.IsNullOrEmpty(source))
            {
                to.Headers[headerName] = source;
            }
        }

        private HttpWebRequest CloneCurrentRequest()
        {
            var request2 = (HttpWebRequest)WebRequest.Create(this.request.RequestUri);
            request2.Accept = this.Accept;
            request2.AllowAutoRedirect = AllowAutoRedirect;
            request2.ClientCertificates = this.ClientCertificates;
            request2.ConnectionGroupName = this.ConnectionGroupName;
            request2.ContentType = this.ContentType;
            request2.CookieContainer = this.CookieContainer;
            request2.Credentials = this.Credentials;
            //request2.Headers = this.Headers;
            request2.Proxy = this.Proxy;
            request2.Timeout = this.Timeout;
            request2.UserAgent = this.UserAgent;
            foreach (var headerName in this.Headers.AllKeys)
            {
                if (WebHeaderCollection.IsRestricted(headerName, false)) continue;
                request2.Headers.Add(headerName, this.Headers[headerName]);
            }
            return request2;
        }

        public IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return this.request.BeginGetResponse(callback, state);
        }

        public Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return this.request.EndGetRequestStream(asyncResult);
        }

        public IEwsHttpWebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            return new BufferFreeEwsHttpWebResponse((HttpWebResponse)this.request.EndGetResponse(asyncResult));
        }

        public Stream GetRequestStream()
        {
            return this.request.GetRequestStream();
        }

        public IEwsHttpWebResponse GetResponse()
        {
            return new BufferFreeEwsHttpWebResponse(this.request.GetResponse() as HttpWebResponse);
        }
        
    }
    
    [DebuggerNonUserCode]
    class BufferFreeEwsHttpWebResponse : IEwsHttpWebResponse
    {
        private HttpWebResponse response;

        public BufferFreeEwsHttpWebResponse(HttpWebResponse response)
        {
            this.response = response;
        }


        public string ContentEncoding
        {
            get
            {
                return this.response.ContentEncoding;
            }
        }

        public string ContentType
        {
            get
            {
                return this.response.ContentType;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this.response.Headers;
            }
        }

        public Version ProtocolVersion
        {
            get
            {
                return this.response.ProtocolVersion;
            }
        }

        public Uri ResponseUri
        {
            get
            {
                return this.response.ResponseUri;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                return this.response.StatusCode;
            }
        }

        public string StatusDescription
        {
            get
            {
                return this.response.StatusDescription;
            }
        }

        public void Close()
        {
            this.response.Close();
        }

        public void Dispose()
        {
            this.response.Close();
        }

        public Stream GetResponseStream()
        {
            return this.response.GetResponseStream();
        }
    }

    //static class ExchangeServiceExtension
    //{
    //    private static Lazy<Action<ExchangeService, IEwsHttpWebRequestFactory>> setHttpWebRequestFactoryDelegate;

    //    static ExchangeServiceExtension()
    //    {
    //        setHttpWebRequestFactoryDelegate = new Lazy<Action<ExchangeService, IEwsHttpWebRequestFactory>>(() =>
    //        {
    //            var method = typeof(ExchangeService).GetMethod("set_HttpWebRequestFactory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
    //            return (Action<ExchangeService, IEwsHttpWebRequestFactory>)Delegate.CreateDelegate(typeof(Action<ExchangeService, IEwsHttpWebRequestFactory>), method);
    //        }, LazyThreadSafetyMode.PublicationOnly);
    //    }


    //    public static void SetHttpWebRequestFactory(this ExchangeService service, IEwsHttpWebRequestFactory factory)
    //    {
    //        if (service == null) throw new ArgumentNullException("service");
    //        if (factory == null) throw new ArgumentNullException("factory");
    //        setHttpWebRequestFactoryDelegate.Value(service, factory);
    //    }
    //}
}

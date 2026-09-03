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



namespace AveClientRequest.Common
{
    using System.Net;
    using System.IO;
    using System.Runtime.Serialization;
    using AvePoint.Wrapper.Common;
    using System;

    [Serializable]
    public class AveHttpWebRequest : WebRequest
    {
        public static AveHttpWebRequest Create(string url)
        {
            return new AveHttpWebRequest(HttpWebRequest.Create(url) as HttpWebRequest, new DataMonitor());
        }
        public static AveHttpWebRequest Create(System.Uri uri)
        {
            return new AveHttpWebRequest(HttpWebRequest.Create(uri) as HttpWebRequest, new DataMonitor());
        }

        HttpWebRequest m_HttpWebRequest = null;
        DataMonitor m_DataMonitor = null;
        public CookieContainer CookieContainer
        {
            get { return this.m_HttpWebRequest.CookieContainer; }
            set { this.m_HttpWebRequest.CookieContainer = value; }
        }
        public int ReadWriteTimeout
        {
            get { return this.m_HttpWebRequest.ReadWriteTimeout; }
            set { this.m_HttpWebRequest.ReadWriteTimeout = value; }
        }
        public DataMonitor DataMonitor
        {
            get
            {
                if (m_DataMonitor == null)
                {
                    this.m_DataMonitor = new DataMonitor();
                }
                return this.m_DataMonitor;
            }
        }
        public AveHttpWebRequest(HttpWebRequest webRequest, DataMonitor dataMonitor)
        {
            m_HttpWebRequest = webRequest;
            m_HttpWebRequest.Timeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;
            m_HttpWebRequest.ReadWriteTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestReadWriteTimeout;
            m_DataMonitor = dataMonitor;
        }
        public override Stream GetRequestStream()
        {
            this.DataMonitor.RecordStream();
            this.DataMonitor.ByteSend += this.Headers.ToString().Length;
            Stream stream = m_HttpWebRequest.GetRequestStream();
            return new AveWebStream(stream, DataMonitor);
            //return stream;
        }
        public override WebResponse GetResponse()
        {
            WebResponse response = m_HttpWebRequest.GetResponse();
            return new AveHttpWebResponse(response, DataMonitor);
            //return response;
        }
        public override System.IAsyncResult BeginGetRequestStream(System.AsyncCallback callback, object state)
        {
            return this.m_HttpWebRequest.BeginGetRequestStream(callback, state);
        }
        public override Stream EndGetRequestStream(System.IAsyncResult asyncResult)
        {
            Stream stream = this.m_HttpWebRequest.EndGetRequestStream(asyncResult);
            return new AveWebStream(stream, DataMonitor);
        }
        public override System.IAsyncResult BeginGetResponse(System.AsyncCallback callback, object state)
        {
            return this.m_HttpWebRequest.BeginGetResponse(callback, state);
        }
        public override WebResponse EndGetResponse(System.IAsyncResult asyncResult)
        {
            WebResponse response = this.m_HttpWebRequest.EndGetResponse(asyncResult);
            return new AveHttpWebResponse(response, DataMonitor);
        }
        public override void Abort()
        {
            this.m_HttpWebRequest.Abort();
        }
        public override System.Net.Cache.RequestCachePolicy CachePolicy
        {
            get
            {
                return this.m_HttpWebRequest.CachePolicy;
            }
            set
            {
                this.m_HttpWebRequest.CachePolicy = value;
            }
        }
        public override string ConnectionGroupName
        {
            get
            {
                return this.m_HttpWebRequest.ConnectionGroupName;
            }
            set
            {
                this.m_HttpWebRequest.ConnectionGroupName = value;
            }
        }
        public override long ContentLength
        {
            get
            {
                return this.m_HttpWebRequest.ContentLength;
            }
            set
            {
                this.m_HttpWebRequest.ContentLength = value;
            }
        }
        public override string ContentType
        {
            get
            {
                return this.m_HttpWebRequest.ContentType;
            }
            set
            {
                this.m_HttpWebRequest.ContentType = value;
            }
        }
        public override ICredentials Credentials
        {
            get
            {
                return this.m_HttpWebRequest.Credentials;
            }
            set
            {
                this.m_HttpWebRequest.Credentials = value;
            }
        }
        public override WebHeaderCollection Headers
        {
            get
            {
                return this.m_HttpWebRequest.Headers;
            }
            set
            {
                this.m_HttpWebRequest.Headers = value;
            }
        }
        public override string Method
        {
            get
            {
                return this.m_HttpWebRequest.Method;
            }
            set
            {
                this.m_HttpWebRequest.Method = value;
            }
        }
        public override bool PreAuthenticate
        {
            get
            {
                return this.m_HttpWebRequest.PreAuthenticate;
            }
            set
            {
                this.m_HttpWebRequest.PreAuthenticate = value;
            }
        }
        public override IWebProxy Proxy
        {
            get
            {
                return this.m_HttpWebRequest.Proxy;
            }
            set
            {
                this.m_HttpWebRequest.Proxy = value;
            }
        }
        public override System.Uri RequestUri
        {
            get
            {
                return this.m_HttpWebRequest.RequestUri;
            }
        }
        public override int Timeout
        {
            get
            {
                return this.m_HttpWebRequest.Timeout;
            }
            set
            {
                this.m_HttpWebRequest.Timeout = value;
            }
        }
        public override bool UseDefaultCredentials
        {
            get
            {
                return this.m_HttpWebRequest.UseDefaultCredentials;
            }
            set
            {
                this.m_HttpWebRequest.UseDefaultCredentials = value;
            }
        }

        public string UserAgent
        {
            get
            {
                return this.m_HttpWebRequest.UserAgent;
            }
            set
            {
                this.m_HttpWebRequest.UserAgent = value;
            }
        }
    }
}

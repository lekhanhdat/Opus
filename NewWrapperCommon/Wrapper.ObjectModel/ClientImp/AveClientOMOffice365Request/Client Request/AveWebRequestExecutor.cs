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



using System.Reflection;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.IO;
using AveClientRequest.Common;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.O365
{
    public class AveWebRequestExecutor : WebRequestExecutor
    {
        private ClientRuntimeContext m_context;
        private bool m_setupCredential;
        private ReconnectableHttpWebRequest m_webRequest;
        private HttpWebResponse m_webResponse;
        private DataMonitor m_dataMonitor;

        // Methods
        public AveWebRequestExecutor(ClientRuntimeContext context, string requestUrl, DataMonitor dataMonitor = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (string.IsNullOrEmpty(requestUrl))
            {
                throw new ArgumentNullException("requestUrl");
            }
            this.m_context = context;
            this.m_webRequest = ReconnectableHttpWebRequest.CreateRequest(requestUrl) as ReconnectableHttpWebRequest;
            this.m_webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            this.m_webRequest.Timeout = context.RequestTimeout;
            this.m_webRequest.Method = "POST";
            this.m_webRequest.RefreshDigestInfo(context as ClientContext);
            this.m_webRequest.UserAgent = WrapperConfiguration.UserAgentTag;
            this.m_dataMonitor = dataMonitor;
        }

        public override void Dispose()
        {
            if (this.m_webResponse != null)
            {
                this.m_webResponse.Close();
            }
        }

        public override void Execute()
        {            
            this.m_webResponse = (HttpWebResponse) this.m_webRequest.GetResponse();
        }

        public override Stream GetRequestStream()
        {            
            return this.m_webRequest.GetRequestStream();
        }

        public override Stream GetResponseStream()
        {
            if (this.m_webResponse == null)
            {
                throw new InvalidOperationException();
            }
            return this.m_webResponse.GetResponseStream();
        }

        // Properties
        public override string RequestContentType
        {
            get
            {
                return this.m_webRequest.ContentType;
            }
            set
            {
                this.m_webRequest.ContentType = value;
            }
        }

        public override WebHeaderCollection RequestHeaders
        {
            get
            {
                return this.m_webRequest.Headers;
            }
        }

        public override bool RequestKeepAlive
        {
            get
            {
                return this.m_webRequest.KeepAlive;
            }
            set
            {
                this.m_webRequest.KeepAlive = value;
            }
        }

        public override string RequestMethod
        {
            get
            {
                return this.m_webRequest.Method;
            }
            set
            {
                this.m_webRequest.Method = value;
            }
        }

        public override string ResponseContentType
        {
            get
            {
                if (this.m_webResponse == null)
                {
                    throw new InvalidOperationException();
                }
                return this.m_webResponse.ContentType;
            }
        }

        public override WebHeaderCollection ResponseHeaders
        {
            get
            {
                if (this.m_webResponse == null)
                {
                    throw new InvalidOperationException();
                }
                return this.m_webResponse.Headers;
            }
        }

        public override HttpStatusCode StatusCode
        {
            get
            {
                if (this.m_webResponse == null)
                {
                    throw new InvalidOperationException();
                }
                return this.m_webResponse.StatusCode;
            }
        }

        public override HttpWebRequest WebRequest
        {
            get
            {
                return null;
            }
        }

        public ReconnectableHttpWebRequest Request
        {
            get
            {
                return this.m_webRequest;
            }
        }
    }
}

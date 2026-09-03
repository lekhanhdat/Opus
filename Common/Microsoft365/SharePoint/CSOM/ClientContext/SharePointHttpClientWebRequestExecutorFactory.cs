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
namespace Microsoft.SharePoint.Client
{
	using System;
	using System.IO;
	using System.Net;
	using System.Threading.Tasks;

	using Microsoft365.Common.HttpUtil;
	public class SharePointHttpClientWebRequestExecutorFactory : WebRequestExecutorFactory
	{
		public override WebRequestExecutor CreateWebRequestExecutor(ClientRuntimeContext context, string requestUrl)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (string.IsNullOrEmpty(requestUrl))
			{
				throw new ArgumentNullException("requestUrl");
			}
			return new HttpCLientWebRequestExecutor(context, requestUrl);
		}
	}

	internal class HttpCLientWebRequestExecutor : WebRequestExecutor
	{
		private HttpWebRequest m_webRequest;

		private HttpWebResponse m_webResponse;

		private ClientRuntimeContext m_context;

		public override HttpWebRequest WebRequest => m_webRequest;

		public override string RequestContentType
		{
			get
			{
				return m_webRequest.ContentType;
			}
			set
			{
				m_webRequest.ContentType = value;
			}
		}

		public override string RequestMethod
		{
			get
			{
				return m_webRequest.Method;
			}
			set
			{
				m_webRequest.Method = value;
			}
		}

		public override bool RequestKeepAlive
		{
			get
			{
				return m_webRequest.KeepAlive;
			}
			set
			{
				m_webRequest.KeepAlive = value;
			}
		}

		public override WebHeaderCollection RequestHeaders => m_webRequest.Headers;

		public override HttpStatusCode StatusCode
		{
			get
			{
				if (m_webResponse == null)
				{
					throw new InvalidOperationException();
				}
				return m_webResponse.StatusCode;
			}
		}

		public override string ResponseContentType
		{
			get
			{
				if (m_webResponse == null)
				{
					throw new InvalidOperationException();
				}
				return m_webResponse.ContentType;
			}
		}

		public override WebHeaderCollection ResponseHeaders
		{
			get
			{
				if (m_webResponse == null)
				{
					throw new InvalidOperationException();
				}
				return m_webResponse.Headers;
			}
		}

		public HttpCLientWebRequestExecutor(ClientRuntimeContext context, string requestUrl)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (string.IsNullOrEmpty(requestUrl))
			{
				throw new ArgumentNullException("requestUrl");
			}
			m_context = context;
			m_webRequest = (HttpWebRequest)System.Net.WebRequest.Create(requestUrl);
			m_webRequest.Timeout = context.RequestTimeout;
			m_webRequest.Method = "POST";
			m_webRequest.Pipelined = false;
		}

		public override Stream GetRequestStream()
		{
			return m_webRequest.GetRequestStream();
		}

		public override void Execute()
		{
			m_webResponse = (HttpWebResponse)m_webRequest.GetResponseByHttpClient(null,"CSOM");
		}

		public override async Task ExecuteAsync()
		{
			await Task.Run(Execute);
		}

		public override Stream GetResponseStream()
		{
			if (m_webResponse == null)
			{
				throw new InvalidOperationException();
			}
			return m_webResponse.GetResponseStream();
		}

		public override void Dispose()
		{
			if (m_webResponse != null)
			{
				m_webResponse.Close();
			}
		}
	}

}
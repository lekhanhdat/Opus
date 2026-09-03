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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	internal class HttpWebRequestWrapper : IHttpWebRequest
	{
		private readonly HttpWebRequest request;

		private int timeoutInMilliSeconds = 30000;

		public RequestParameters BodyParameters
		{
			get;
			set;
		}

		public string Accept
		{
			set
			{
				request.Accept = value;
			}
		}

		public string ContentType
		{
			set
			{
				request.ContentType = value;
			}
		}

		public string Method
		{
			set
			{
				request.Method = value;
			}
		}

		public bool UseDefaultCredentials
		{
			set
			{
				request.UseDefaultCredentials = value;
			}
		}

		public WebHeaderCollection Headers => request.Headers;

		public int TimeoutInMilliSeconds
		{
			set
			{
				timeoutInMilliSeconds = value;
			}
		}

		public HttpWebRequestWrapper(string uri)
		{
			request = (HttpWebRequest)WebRequest.Create(uri);
		}

		public async Task<IHttpWebResponse> GetResponseSyncOrAsync(CallState callState)
		{
			if (BodyParameters != null)
			{
				using (Stream stream = await GetRequestStreamSyncOrAsync(callState))
				{
					BodyParameters.WriteToStream(stream);
				}
			}
			if (callState != null && callState.CallSync)
			{
				request.Timeout = timeoutInMilliSeconds;
				return NetworkPlugin.HttpWebRequestFactory.CreateResponse(request.GetResponse());
			}
			Task<WebResponse> getResponseTask = request.GetResponseAsync();
			ThreadPool.RegisterWaitForSingleObject(((IAsyncResult)getResponseTask).AsyncWaitHandle, delegate(object state, bool timedOut)
			{
				if (timedOut)
				{
					((HttpWebRequest)state).Abort();
				}
			}, request, timeoutInMilliSeconds, executeOnlyOnce: true);
			return NetworkPlugin.HttpWebRequestFactory.CreateResponse(await getResponseTask);
		}

		public async Task<Stream> GetRequestStreamSyncOrAsync(CallState callState)
		{
			if (callState != null && callState.CallSync)
			{
				return request.GetRequestStream();
			}
			return await request.GetRequestStreamAsync();
		}
	}
}
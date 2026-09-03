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
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	internal static class HttpHelper
	{
		public static async Task<T> SendPostRequestAndDeserializeJsonResponseAsync<T>(string uri, RequestParameters requestParameters, CallState callState)
		{
			ClientMetrics clientMetrics = new ClientMetrics();
			try
			{
				int num = default(int);
				int num2 = num;
				try
				{
					IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(uri);
					request.ContentType = "application/x-www-form-urlencoded";
					AddCorrelationIdHeadersToRequest(request, callState);
					AdalIdHelper.AddAsHeaders(request);
					clientMetrics.BeginClientMetricsRecord(request, callState);
					SetPostRequest(request, requestParameters, callState);
					using (IHttpWebResponse response = await request.GetResponseSyncOrAsync(callState))
					{
						VerifyCorrelationIdHeaderInReponse(response, callState);
						clientMetrics.SetLastError(null);
						return DeserializeResponse<T>(response);
					}
				}
				catch (WebException ex)
				{
					TokenResponse tokenResponse = OAuth2Response.ReadErrorResponse(ex.Response);
					clientMetrics.SetLastError(tokenResponse?.ErrorCodes);
					throw new AdalServiceException(tokenResponse?.Error, tokenResponse?.ErrorDescription, tokenResponse?.ErrorCodes, ex);
				}
			}
			finally
			{
				clientMetrics.EndClientMetricsRecord("token", callState);
			}
		}

		public static void SetPostRequest(IHttpWebRequest request, RequestParameters requestParameters, CallState callState, Dictionary<string, string> headers = null)
		{
			request.Method = "POST";
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					request.Headers[header.Key] = header.Value;
				}
			}
			request.BodyParameters = requestParameters;
		}

		public static T DeserializeResponse<T>(IHttpWebResponse response)
		{
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
			Stream responseStream = response.GetResponseStream();
			if (responseStream == null)
			{
				return default(T);
			}
			using (Stream stream = responseStream)
			{
				return (T)dataContractJsonSerializer.ReadObject(stream);
			}
		}

		public static string ReadStreamContent(Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				return streamReader.ReadToEnd();
			}
		}

		public static string CheckForExtraQueryParameter(string url)
		{
			string environmentVariable = PlatformSpecificHelper.GetEnvironmentVariable("ExtraQueryParameter");
			string str = (url.IndexOf('?') > 0) ? "&" : "?";
			if (!string.IsNullOrWhiteSpace(environmentVariable))
			{
				url += str + environmentVariable;
			}
			return url;
		}

		public static void AddCorrelationIdHeadersToRequest(IHttpWebRequest request, CallState callState)
		{
			if (callState != null && !(callState.CorrelationId == Guid.Empty))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("client-request-id", callState.CorrelationId.ToString());
				dictionary.Add("return-client-request-id", "true");
				Dictionary<string, string> headers = dictionary;
				AddHeadersToRequest(request, headers);
			}
		}

		public static void VerifyCorrelationIdHeaderInReponse(IHttpWebResponse response, CallState callState)
		{
			if (callState != null && !(callState.CorrelationId == Guid.Empty))
			{
				WebHeaderCollection headers = response.Headers;
				string[] allKeys = headers.AllKeys;
				int num = 0;
				string text2;
				while (true)
				{
					if (num >= allKeys.Length)
					{
						return;
					}
					string text = allKeys[num];
					text2 = text.Trim();
					if (string.Compare(text2, "client-request-id", StringComparison.OrdinalIgnoreCase) == 0)
					{
						break;
					}
					num++;
				}
				string text3 = headers[text2].Trim();
				if (!Guid.TryParse(text3, out Guid result))
				{
					ADALLogger.Warning(callState, "Returned correlation id '{0}' is not in GUID format.", text3);
				}
				else if (result != callState.CorrelationId)
				{
					ADALLogger.Warning(callState, "Returned correlation id '{0}' does not match the sent correlation id '{1}'", text3, callState.CorrelationId);
				}
			}
		}

		public static void AddHeadersToRequest(IHttpWebRequest request, Dictionary<string, string> headers)
		{
			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					request.Headers[header.Key] = header.Value;
				}
			}
		}
	}
}
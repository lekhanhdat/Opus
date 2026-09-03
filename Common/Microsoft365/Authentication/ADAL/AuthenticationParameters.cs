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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Contains authentication parameters based on unauthorized response from resource server.
	/// </summary>
	/// <summary>
	/// Contains authentication parameters based on unauthorized response from resource server.
	/// </summary>
	public sealed class AuthenticationParameters
	{




		/// <summary>
		/// Gets or sets the address of the authority to issue token.
		/// </summary>
		public string Authority
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the identifier of the target resource that is the recipient of the requested token.
		/// </summary>
		public string Resource
		{
			get;
			set;
		}

		/// <summary>
		/// Creates authentication parameters from the WWW-Authenticate header in response received from resource. This method expects the header to contain authentication parameters.
		/// </summary>
		/// <param name="authenticateHeader">Content of header WWW-Authenticate header</param>
		/// <returns>AuthenticationParameters object containing authentication parameters</returns>
		public static AuthenticationParameters CreateFromResponseAuthenticateHeader(string authenticateHeader)
		{
			if (string.IsNullOrWhiteSpace(authenticateHeader))
			{
				throw new ArgumentNullException("authenticateHeader");
			}
			authenticateHeader = authenticateHeader.Trim();
			if (!authenticateHeader.StartsWith("bearer", StringComparison.OrdinalIgnoreCase) || authenticateHeader.Length < "bearer".Length + 2 || !char.IsWhiteSpace(authenticateHeader["bearer".Length]))
			{
				ArgumentException ex = new ArgumentException("Invalid authenticate header format", "authenticateHeader");
				ADALLogger.Error(null, ex);
				throw ex;
			}
			authenticateHeader = authenticateHeader.Substring("bearer".Length).Trim();
			Dictionary<string, string> dictionary = EncodingHelper.ParseKeyValueList(authenticateHeader, ',', urlDecode: false, null);
			AuthenticationParameters authenticationParameters = new AuthenticationParameters();
			dictionary.TryGetValue("authorization_uri", out string value);
			authenticationParameters.Authority = value;
			dictionary.TryGetValue("resource_id", out value);
			authenticationParameters.Resource = value;
			return authenticationParameters;
		}

		private static async Task<AuthenticationParameters> CreateFromResourceUrlCommonAsync(Uri resourceUrl)
		{
			CallState callState = new CallState(Guid.NewGuid(), callSync: false);
			if (!(resourceUrl == null))
			{
				IHttpWebResponse response = null;
				try
				{
					int num = default(int);
					int num2 = num;
					try
					{
						IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(resourceUrl.AbsoluteUri);
						request.ContentType = "application/x-www-form-urlencoded";
						response = await request.GetResponseSyncOrAsync(callState);
						AdalException ex = new AdalException("unauthorized_response_expected");
						ADALLogger.Error(null, ex);
						throw ex;
					}
					catch (WebException ex2)
					{
						response = NetworkPlugin.HttpWebRequestFactory.CreateResponse(ex2.Response);
						if (response == null)
						{
							AdalServiceException ex3 = new AdalServiceException("Unauthorized Http Status Code (401) was expected in the response", ex2);
							ADALLogger.Error(null, ex3);
							throw ex3;
						}
						return CreateFromUnauthorizedResponseCommon(response);
					}
				}
				finally
				{
					response?.Close();
				}
			}
			throw new ArgumentNullException("resourceUrl");
		}

		private static AuthenticationParameters CreateFromUnauthorizedResponseCommon(IHttpWebResponse response)
		{
			if (response == null)
			{
				throw new ArgumentNullException("response");
			}
			if (response.StatusCode != HttpStatusCode.Unauthorized)
			{
				ArgumentException ex = new ArgumentException("Unauthorized Http Status Code (401) was expected in the response", "response");
				ADALLogger.Error(null, ex);
				throw ex;
			}
			if (!response.Headers.AllKeys.Contains("WWW-Authenticate"))
			{
				ArgumentException ex2 = new ArgumentException("WWW-Authenticate header was expected in the response", "response");
				ADALLogger.Error(null, ex2);
				throw ex2;
			}
			return CreateFromResponseAuthenticateHeader(response.Headers["WWW-Authenticate"]);
		}

		/// <summary>
		/// Creates authentication parameters from address of the resource. This method expects the resource server to return unauthorized response
		/// with WWW-Authenticate header containing authentication parameters.
		/// </summary>
		/// <param name="resourceUrl">Address of the resource</param>
		/// <returns>AuthenticationParameters object containing authentication parameters</returns>
		public static async Task<AuthenticationParameters> CreateFromResourceUrlAsync(Uri resourceUrl)
		{
			return await CreateFromResourceUrlCommonAsync(resourceUrl);
		}

		/// <summary>
		/// Creates authentication parameters from the response received from the response received from the resource. This method expects the response to have unauthorized status and
		/// WWW-Authenticate header containing authentication parameters.</summary>
		/// <param name="response">Response received from the resource.</param>
		/// <returns>AuthenticationParameters object containing authentication parameters</returns>
		public static AuthenticationParameters CreateFromUnauthorizedResponse(HttpWebResponse response)
		{
			return CreateFromUnauthorizedResponseCommon(NetworkPlugin.HttpWebRequestFactory.CreateResponse(response));
		}
	}
}
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Microsoft365.Authentication.ADAL
{
	internal static class OAuth2Response
	{
		public static AuthenticationResult ParseTokenResponse(TokenResponse tokenResponse, CallState callState)
		{
			if (tokenResponse.AccessToken == null)
			{
				if (tokenResponse.Error != null)
				{
					throw new AdalServiceException(tokenResponse.Error, tokenResponse.ErrorDescription);
				}
				throw new AdalServiceException("unknown_error", "Unknown error");
			}
			DateTimeOffset expiresOn = DateTime.UtcNow + TimeSpan.FromSeconds((double)tokenResponse.ExpiresIn);
			AuthenticationResult authenticationResult = new AuthenticationResult(tokenResponse.TokenType, tokenResponse.AccessToken, tokenResponse.RefreshToken, expiresOn);
			authenticationResult.Resource = tokenResponse.Resource;
			AuthenticationResult authenticationResult2 = authenticationResult;
			IdToken idToken = ParseIdToken(tokenResponse.IdToken);
			if (idToken != null)
			{
				string tenantId = idToken.TenantId;
				string uniqueId = null;
				string displayableId = null;
				if (!string.IsNullOrWhiteSpace(idToken.ObjectId))
				{
					uniqueId = idToken.ObjectId;
				}
				else if (!string.IsNullOrWhiteSpace(idToken.Subject))
				{
					uniqueId = idToken.Subject;
				}
				if (!string.IsNullOrWhiteSpace(idToken.UPN))
				{
					displayableId = idToken.UPN;
				}
				else if (!string.IsNullOrWhiteSpace(idToken.Email))
				{
					displayableId = idToken.Email;
				}
				string givenName = idToken.GivenName;
				string familyName = idToken.FamilyName;
				string identityProvider = idToken.IdentityProvider ?? idToken.Issuer;
				DateTimeOffset? passwordExpiresOn = null;
				if (idToken.PasswordExpiration > 0)
				{
					passwordExpiresOn = DateTime.UtcNow + TimeSpan.FromSeconds((double)idToken.PasswordExpiration);
				}
				Uri passwordChangeUrl = null;
				if (!string.IsNullOrEmpty(idToken.PasswordChangeUrl))
				{
					passwordChangeUrl = new Uri(idToken.PasswordChangeUrl);
				}
				authenticationResult2.UpdateTenantAndUserInfo(tenantId, tokenResponse.IdToken, new UserInfo
				{
					UniqueId = uniqueId,
					DisplayableId = displayableId,
					GivenName = givenName,
					FamilyName = familyName,
					IdentityProvider = identityProvider,
					PasswordExpiresOn = passwordExpiresOn,
					PasswordChangeUrl = passwordChangeUrl
				});
			}
			return authenticationResult2;
		}

		public static AuthorizationResult ParseAuthorizeResponse(string webAuthenticationResult, CallState callState)
		{
			AuthorizationResult result = null;
			Uri uri = new Uri(webAuthenticationResult);
			string query = uri.Query;
			if (!string.IsNullOrWhiteSpace(query))
			{
				Dictionary<string, string> dictionary = EncodingHelper.ParseKeyValueList(query.Substring(1), '&', urlDecode: true, callState);
				result = (dictionary.ContainsKey("code") ? new AuthorizationResult(dictionary["code"]) : ((!dictionary.ContainsKey("error")) ? new AuthorizationResult("authentication_failed", "The authorization server returned an invalid response") : new AuthorizationResult(dictionary["error"], dictionary.ContainsKey("error_description") ? dictionary["error_description"] : null)));
			}
			return result;
		}

		public static TokenResponse ReadErrorResponse(WebResponse response)
		{
			if (response == null)
			{
				TokenResponse tokenResponse = new TokenResponse();
				tokenResponse.Error = "service_returned_error";
				tokenResponse.ErrorDescription = "Service returned error. Check InnerException for more details";
				return tokenResponse;
			}
			Stream responseStream = response.GetResponseStream();
			if (responseStream != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				try
				{
					stringBuilder.Append(HttpHelper.ReadStreamContent(responseStream));
					using (MemoryStream stream = new MemoryStream(stringBuilder.ToByteArray()))
					{
						DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(TokenResponse));
						return (TokenResponse)dataContractJsonSerializer.ReadObject(stream);
					}
				}
				catch (SerializationException)
				{
					TokenResponse tokenResponse2 = new TokenResponse();
					tokenResponse2.Error = ((((HttpWebResponse)response).StatusCode == HttpStatusCode.ServiceUnavailable) ? "service_unavailable" : "unknown_error");
					tokenResponse2.ErrorDescription = stringBuilder.ToString();
					return tokenResponse2;
				}
			}
			TokenResponse tokenResponse3 = new TokenResponse();
			tokenResponse3.Error = "unknown_error";
			tokenResponse3.ErrorDescription = "Unknown error";
			return tokenResponse3;
		}

		private static IdToken ParseIdToken(string idToken)
		{
			IdToken result = null;
			if (!string.IsNullOrWhiteSpace(idToken))
			{
				string[] array = idToken.Split('.');
				if (array.Length == 3)
				{
					try
					{
						byte[] buffer = Base64UrlEncoder.DecodeBytes(array[1]);
						using (MemoryStream stream = new MemoryStream(buffer))
						{
							DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(IdToken));
							result = (IdToken)dataContractJsonSerializer.ReadObject(stream);
							return result;
						}
					}
					catch (SerializationException)
					{
						return result;
					}
					catch (ArgumentException)
					{
						return result;
					}
				}
			}
			return result;
		}
	}
}
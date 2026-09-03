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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Contains the results of one token acquisition operation. 
	/// </summary>
	/// <summary>
	/// Contains the results of one token acquisition operation. 
	/// </summary>
	[DataContract]
	public sealed class AuthenticationResult
	{

		/// <summary>
		/// Gets the type of the Access Token returned. 
		/// </summary>
		[DataMember]
		public string AccessTokenType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the Access Token requested.
		/// </summary>
		[DataMember]
		public string AccessToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the Refresh Token associated with the requested Access Token. Note: not all operations will return a Refresh Token.
		/// </summary>
		[DataMember]
		public string RefreshToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
		/// This value is calculated based on current UTC time measured locally and the value expiresIn received from the service.
		/// </summary>
		[DataMember]
		public DateTimeOffset ExpiresOn
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets an identifier for the tenant the token was acquired from. This property will be null if tenant information is not returned by the service.
		/// </summary>
		[DataMember]
		public string TenantId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets user information including user Id. Some elements in UserInfo might be null if not returned by the service.
		/// </summary>
		[DataMember]
		public UserInfo UserInfo
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the entire Id Token if returned by the service or null if no Id Token is returned.
		/// </summary>
		[DataMember]
		public string IdToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a value indicating whether the refresh token can be used for requesting access token for other resources.
		/// </summary>
		[DataMember]
		public bool IsMultipleResourceRefreshToken
		{
			get;
			internal set;
		}

		[DataMember]
		internal string UserAssertionHash
		{
			get;
			set;
		}

		internal string Resource
		{
			get;
			set;
		}

		/// <summary>
		/// Creates result returned from AcquireToken. Except in advanced scenarios related to token caching, you do not need to create any instance of AuthenticationResult.
		/// </summary>
		/// <param name="accessTokenType">Type of the Access Token returned</param>
		/// <param name="accessToken">The Access Token requested</param>
		/// <param name="refreshToken">The Refresh Token associated with the requested Access Token</param>
		/// <param name="expiresOn">The point in time in which the Access Token returned in the AccessToken property ceases to be valid</param>
		internal AuthenticationResult(string accessTokenType, string accessToken, string refreshToken, DateTimeOffset expiresOn)
		{
			AccessTokenType = accessTokenType;
			AccessToken = accessToken;
			RefreshToken = refreshToken;
			ExpiresOn = DateTime.SpecifyKind(expiresOn.DateTime, DateTimeKind.Utc);
		}

		/// <summary>
		/// Serializes the object to a JSON string
		/// </summary>
		/// <returns>Deserialized authentication result</returns>
		public static AuthenticationResult Deserialize(string serializedObject)
		{
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticationResult));
			byte[] bytes = Encoding.UTF8.GetBytes(serializedObject);
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				return (AuthenticationResult)dataContractJsonSerializer.ReadObject(stream);
			}
		}

		/// <summary>
		/// Creates authorization header from authentication result.
		/// </summary>
		/// <returns>Created authorization header</returns>
		public string CreateAuthorizationHeader()
		{
			return "Bearer " + AccessToken;
		}

		/// <summary>
		/// Serializes the object to a JSON string
		/// </summary>
		/// <returns>Serialized authentication result</returns>
		public string Serialize()
		{
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticationResult));
			using (MemoryStream memoryStream = new MemoryStream())
			{
				dataContractJsonSerializer.WriteObject(memoryStream, this);
				return Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Position);
			}
		}

		internal void UpdateTenantAndUserInfo(string tenantId, string idToken, UserInfo userInfo)
		{
			TenantId = tenantId;
			IdToken = idToken;
			if (userInfo != null)
			{
				UserInfo = new UserInfo(userInfo);
			}
		}
	}
}
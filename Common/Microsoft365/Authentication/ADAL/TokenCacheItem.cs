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

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Token cache item
	/// </summary>
	public sealed class TokenCacheItem
	{
		/// <summary>
		/// Gets the Authority.
		/// </summary>
		public string Authority
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the ClientId.
		/// </summary>
		public string ClientId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the Expiration.
		/// </summary>
		public DateTimeOffset ExpiresOn
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the FamilyName.
		/// </summary>
		public string FamilyName
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the GivenName.
		/// </summary>
		public string GivenName
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the IdentityProviderName.
		/// </summary>
		public string IdentityProvider
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a value indicating whether the RefreshToken applies to multiple resources.
		/// </summary>
		public bool IsMultipleResourceRefreshToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the Resource.
		/// </summary>
		public string Resource
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the TenantId.
		/// </summary>
		public string TenantId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the user's unique Id.
		/// </summary>
		public string UniqueId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the user's displayable Id.
		/// </summary>
		public string DisplayableId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the Access Token requested.
		/// </summary>
		public string AccessToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the Refresh Token associated with the requested Access Token. Note: not all operations will return a Refresh Token.
		/// </summary>
		public string RefreshToken
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the entire Id Token if returned by the service or null if no Id Token is returned.
		/// </summary>
		public string IdToken
		{
			get;
			internal set;
		}

		internal TokenSubjectType TokenSubjectType
		{
			get;
			set;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		internal TokenCacheItem(TokenCacheKey key, AuthenticationResult result)
		{
			Authority = key.Authority;
			Resource = key.Resource;
			ClientId = key.ClientId;
			TokenSubjectType = key.TokenSubjectType;
			UniqueId = key.UniqueId;
			DisplayableId = key.DisplayableId;
			TenantId = result.TenantId;
			ExpiresOn = result.ExpiresOn;
			IsMultipleResourceRefreshToken = result.IsMultipleResourceRefreshToken;
			AccessToken = result.AccessToken;
			RefreshToken = result.RefreshToken;
			IdToken = result.IdToken;
			if (result.UserInfo != null)
			{
				FamilyName = result.UserInfo.FamilyName;
				GivenName = result.UserInfo.GivenName;
				IdentityProvider = result.UserInfo.IdentityProvider;
			}
		}

		internal bool Match(TokenCacheKey key)
		{
			if (key.Authority == Authority && key.ResourceEquals(Resource) && key.ClientIdEquals(ClientId) && key.TokenSubjectType == TokenSubjectType && key.UniqueId == UniqueId)
			{
				return key.DisplayableIdEquals(DisplayableId);
			}
			return false;
		}
	}
}
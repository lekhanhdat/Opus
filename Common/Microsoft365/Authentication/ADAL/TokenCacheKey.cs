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
	/// <see cref="T:Portal.ADAL.TokenCacheKey" /> can be used with Linq to access items from the TokenCache dictionary.
	/// </summary>
	internal sealed class TokenCacheKey
	{
		public string Authority
		{
			get;
			private set;
		}

		public string Resource
		{
			get;
			internal set;
		}

		public string ClientId
		{
			get;
			private set;
		}

		public string UniqueId
		{
			get;
			private set;
		}

		public string DisplayableId
		{
			get;
			private set;
		}

		public TokenSubjectType TokenSubjectType
		{
			get;
			private set;
		}

		internal TokenCacheKey(string authority, string resource, string clientId, TokenSubjectType tokenSubjectType, UserInfo userInfo)
			: this(authority, resource, clientId, tokenSubjectType, userInfo?.UniqueId, userInfo?.DisplayableId)
		{
		}

		internal TokenCacheKey(string authority, string resource, string clientId, TokenSubjectType tokenSubjectType, string uniqueId, string displayableId)
		{
			Authority = authority;
			Resource = resource;
			ClientId = clientId;
			TokenSubjectType = tokenSubjectType;
			UniqueId = uniqueId;
			DisplayableId = displayableId;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <returns>
		/// true if the specified object is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			TokenCacheKey tokenCacheKey = obj as TokenCacheKey;
			if (tokenCacheKey != null)
			{
				return Equals(tokenCacheKey);
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified TokenCacheKey is equal to the current object.
		/// </summary>
		/// <returns>
		/// true if the specified TokenCacheKey is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="other">The TokenCacheKey to compare with the current object. </param><filterpriority>2</filterpriority>
		public bool Equals(TokenCacheKey other)
		{
			if (!object.ReferenceEquals(this, other))
			{
				if (other != null && other.Authority == Authority && ResourceEquals(other.Resource) && ClientIdEquals(other.ClientId) && other.UniqueId == UniqueId && DisplayableIdEquals(other.DisplayableId))
				{
					return other.TokenSubjectType == TokenSubjectType;
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the hash code for this TokenCacheKey.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer hash code.
		/// </returns>
		public override int GetHashCode()
		{
			return (Authority + ":::" + Resource.PlatformSpecificToLower() + ":::" + ClientId.PlatformSpecificToLower() + ":::" + UniqueId + ":::" + ((DisplayableId != null) ? DisplayableId.PlatformSpecificToLower() : null) + ":::" + (int)TokenSubjectType).GetHashCode();
		}

		internal bool ResourceEquals(string otherResource)
		{
			return string.Compare(otherResource, Resource, StringComparison.OrdinalIgnoreCase) == 0;
		}

		internal bool ClientIdEquals(string otherClientId)
		{
			return string.Compare(otherClientId, ClientId, StringComparison.OrdinalIgnoreCase) == 0;
		}

		internal bool DisplayableIdEquals(string otherDisplayableId)
		{
			return string.Compare(otherDisplayableId, DisplayableId, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}
}
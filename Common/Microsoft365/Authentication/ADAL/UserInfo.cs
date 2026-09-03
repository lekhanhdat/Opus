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
using System.Runtime.Serialization;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Contains information of a single user. This information is used for token cache lookup. Also if created with userId, userId is sent to the service when login_hint is accepted.
	/// </summary>
	[DataContract]
	public sealed class UserInfo
	{
		/// <summary>
		/// Gets identifier of the user authenticated during token acquisition. 
		/// </summary>
		[DataMember]
		public string UniqueId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a displayable value in UserPrincipalName (UPN) format. The value can be null.
		/// </summary>
		[DataMember]
		public string DisplayableId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets given name of the user if provided by the service. If not, the value is null. 
		/// </summary>
		[DataMember]
		public string GivenName
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets family name of the user if provided by the service. If not, the value is null. 
		/// </summary>
		[DataMember]
		public string FamilyName
		{
			get;
			internal set;
		}

		[DataMember]
		public DateTimeOffset? PasswordExpiresOn
		{
			get;
			internal set;
		}

		[DataMember]
		public Uri PasswordChangeUrl
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets identity provider if returned by the service. If not, the value is null. 
		/// </summary>
		[DataMember]
		public string IdentityProvider
		{
			get;
			internal set;
		}

		internal bool ForcePrompt
		{
			get;
			private set;
		}

		internal UserInfo()
		{
		}

		internal UserInfo(UserInfo other)
		{
			UniqueId = other.UniqueId;
			DisplayableId = other.DisplayableId;
			GivenName = other.GivenName;
			FamilyName = other.FamilyName;
			IdentityProvider = other.IdentityProvider;
			PasswordChangeUrl = other.PasswordChangeUrl;
			PasswordExpiresOn = other.PasswordExpiresOn;
		}
	}
}
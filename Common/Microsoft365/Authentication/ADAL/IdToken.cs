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
using System.Runtime.Serialization;

namespace Microsoft365.Authentication.ADAL
{
	[DataContract]
	internal class IdToken
	{
		[DataMember(Name = "oid", IsRequired = false)]
		public string ObjectId
		{
			get;
			set;
		}

		[DataMember(Name = "sub", IsRequired = false)]
		public string Subject
		{
			get;
			set;
		}

		[DataMember(Name = "tid", IsRequired = false)]
		public string TenantId
		{
			get;
			set;
		}

		[DataMember(Name = "upn", IsRequired = false)]
		public string UPN
		{
			get;
			set;
		}

		[DataMember(Name = "given_name", IsRequired = false)]
		public string GivenName
		{
			get;
			set;
		}

		[DataMember(Name = "family_name", IsRequired = false)]
		public string FamilyName
		{
			get;
			set;
		}

		[DataMember(Name = "email", IsRequired = false)]
		public string Email
		{
			get;
			set;
		}

		[DataMember(Name = "pwd_exp", IsRequired = false)]
		public long PasswordExpiration
		{
			get;
			set;
		}

		[DataMember(Name = "pwd_url", IsRequired = false)]
		public string PasswordChangeUrl
		{
			get;
			set;
		}

		[DataMember(Name = "idp", IsRequired = false)]
		public string IdentityProvider
		{
			get;
			set;
		}

		[DataMember(Name = "iss", IsRequired = false)]
		public string Issuer
		{
			get;
			set;
		}
	}
}
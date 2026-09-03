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
	internal class TokenResponse
	{

		[DataMember(Name = "token_type", IsRequired = false)]
		public string TokenType
		{
			get;
			set;
		}

		[DataMember(Name = "access_token", IsRequired = false)]
		public string AccessToken
		{
			get;
			set;
		}

		[DataMember(Name = "refresh_token", IsRequired = false)]
		public string RefreshToken
		{
			get;
			set;
		}

		[DataMember(Name = "resource", IsRequired = false)]
		public string Resource
		{
			get;
			set;
		}

		[DataMember(Name = "id_token", IsRequired = false)]
		public string IdToken
		{
			get;
			set;
		}

		[DataMember(Name = "created_on", IsRequired = false)]
		public long CreatedOn
		{
			get;
			set;
		}

		[DataMember(Name = "expires_on", IsRequired = false)]
		public long ExpiresOn
		{
			get;
			set;
		}

		[DataMember(Name = "expires_in", IsRequired = false)]
		public long ExpiresIn
		{
			get;
			set;
		}

		[DataMember(Name = "error", IsRequired = false)]
		public string Error
		{
			get;
			set;
		}

		[DataMember(Name = "error_description", IsRequired = false)]
		public string ErrorDescription
		{
			get;
			set;
		}

		[DataMember(Name = "error_codes", IsRequired = false)]
		public string[] ErrorCodes
		{
			get;
			set;
		}

		[DataMember(Name = "correlation_id", IsRequired = false)]
		public string CorrelationId
		{
			get;
			set;
		}
	}
}
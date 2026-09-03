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
using System.Security;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Credential including client id and secret.
	/// </summary>
	public sealed class ClientCredential
	{
		internal SecureString SecureClientSecret
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the identifier of the client requesting the token.
		/// </summary>
		public string ClientId
		{
			get;
			private set;
		}

		internal string ClientSecret
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor to create credential with client id and secret
		/// </summary>
		/// <param name="clientId">Identifier of the client requesting the token.</param>
		/// <param name="clientSecret">Secret of the client requesting the token.</param>
		public ClientCredential(string clientId, string clientSecret)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				throw new ArgumentNullException("clientId");
			}
			if (string.IsNullOrWhiteSpace(clientSecret))
			{
				throw new ArgumentNullException("clientSecret");
			}
			ClientId = clientId;
			ClientSecret = clientSecret;
		}

		/// <summary>
		/// Constructor to create credential with client id and secret. This constructor accepts client secret as SecureString.
		/// </summary>
		/// <param name="clientId">Identifier of the client requesting the token.</param>
		/// <param name="secureClientSecret">Secret of the client requesting the token in form of SecureString.</param>
		public ClientCredential(string clientId, SecureString secureClientSecret)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				throw new ArgumentNullException("clientId");
			}
			ClientId = clientId;
			SecureClientSecret = secureClientSecret;
		}
	}
}
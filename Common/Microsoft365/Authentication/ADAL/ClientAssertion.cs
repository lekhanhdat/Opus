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
	/// Credential type containing an assertion of type "urn:ietf:params:oauth:token-type:jwt".
	/// </summary>
	public sealed class ClientAssertion
	{
		/// <summary>
		/// Gets the identifier of the client requesting the token.
		/// </summary>
		public string ClientId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the assertion.
		/// </summary>
		public string Assertion
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the assertion type.
		/// </summary>
		public string AssertionType
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor to create credential with a jwt token encoded as a base64 url encoded string.
		/// </summary>
		/// <param name="clientId">Identifier of the client requesting the token.</param>
		/// <param name="assertion">The jwt used as credential.</param>
		public ClientAssertion(string clientId, string assertion)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				throw new ArgumentNullException("clientId");
			}
			if (string.IsNullOrWhiteSpace(assertion))
			{
				throw new ArgumentNullException("assertion");
			}
			ClientId = clientId;
			AssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
			Assertion = assertion;
		}
	}
}
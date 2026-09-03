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
	/// Credential type containing an assertion representing user credential.
	/// </summary>
	public sealed class UserAssertion
	{
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
		/// Gets name of the user.
		/// </summary>
		public string UserName
		{
			get;
			internal set;
		}

		/// <summary>
		/// Constructor to create the object with an assertion. This constructor can be used for On Behalf Of flow which assumes the
		/// assertion is a JWT token. For other flows, the other construction with assertionType must be used.
		/// </summary>
		/// <param name="assertion">Assertion representing the user.</param>
		public UserAssertion(string assertion)
		{
			if (string.IsNullOrWhiteSpace(assertion))
			{
				throw new ArgumentNullException("assertion");
			}
			Assertion = assertion;
		}

		/// <summary>
		/// Constructor to create credential with assertion and assertionType
		/// </summary>
		/// <param name="assertion">Assertion representing the user.</param>
		/// <param name="assertionType">Type of the assertion representing the user.</param>
		public UserAssertion(string assertion, string assertionType)
			: this(assertion, assertionType, null)
		{
		}

		/// <summary>
		/// Constructor to create credential with assertion, assertionType and userId
		/// </summary>
		/// <param name="assertion">Assertion representing the user.</param>
		/// <param name="assertionType">Type of the assertion representing the user.</param>
		/// <param name="userName">Identity of the user token is requested for. This parameter can be null.</param>
		public UserAssertion(string assertion, string assertionType, string userName)
		{
			if (string.IsNullOrWhiteSpace(assertion))
			{
				throw new ArgumentNullException("assertion");
			}
			if (string.IsNullOrWhiteSpace(assertionType))
			{
				throw new ArgumentNullException("assertionType");
			}
			AssertionType = assertionType;
			Assertion = assertion;
			UserName = userName;
		}
	}
}
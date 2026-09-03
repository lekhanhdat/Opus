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
	internal class ClientKey
	{
		public string ClientId
		{
			get;
			private set;
		}

		public bool HasCredential
		{
			get;
			private set;
		}

		public ClientCredential Credential
		{
			get;
			private set;
		}

		public ClientAssertionCertificate Certificate
		{
			get;
			private set;
		}

		public ClientAssertion Assertion
		{
			get;
			private set;
		}

		public Authenticator Authenticator
		{
			get;
			private set;
		}

		public ClientKey(string clientId)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				throw new ArgumentNullException("clientId");
			}
			ClientId = clientId;
			HasCredential = false;
		}

		public ClientKey(ClientCredential clientCredential)
		{
			if (clientCredential == null)
			{
				throw new ArgumentNullException("clientCredential");
			}
			Credential = clientCredential;
			ClientId = clientCredential.ClientId;
			HasCredential = true;
		}

		public ClientKey(ClientAssertionCertificate clientCertificate, Authenticator authenticator)
		{
			Authenticator = authenticator;
			if (clientCertificate == null)
			{
				throw new ArgumentNullException("clientCertificate");
			}
			Certificate = clientCertificate;
			ClientId = clientCertificate.ClientId;
			HasCredential = true;
		}

		public ClientKey(ClientAssertion clientAssertion)
		{
			if (clientAssertion == null)
			{
				throw new ArgumentNullException("clientAssertion");
			}
			Assertion = clientAssertion;
			ClientId = clientAssertion.ClientId;
			HasCredential = true;
		}
	}
}
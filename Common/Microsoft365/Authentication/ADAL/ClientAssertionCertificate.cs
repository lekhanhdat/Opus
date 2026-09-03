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
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// Containing certificate used to create client assertion.
	/// </summary>
	public sealed class ClientAssertionCertificate
	{
		/// <summary>
		/// Gets minimum X509 certificate key size in bits
		/// </summary>
		public static int MinKeySizeInBits => 2048;

		/// <summary>
		/// Gets the identifier of the client requesting the token.
		/// </summary>
		public string ClientId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the certificate used as credential.
		/// </summary>
		public X509Certificate2 Certificate
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor to create credential with client Id and certificate.
		/// </summary>
		/// <param name="clientId">Identifier of the client requesting the token.</param>
		/// <param name="certificate">The certificate used as credential.</param>
		public ClientAssertionCertificate(string clientId, X509Certificate2 certificate)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				throw new ArgumentNullException("clientId");
			}
			if (certificate == null)
			{
				throw new ArgumentNullException("certificate");
			}
			if (certificate.GetRSAPublicKey().KeySize < MinKeySizeInBits)
			{
				throw new ArgumentOutOfRangeException("certificate", string.Format(CultureInfo.InvariantCulture, "The certificate used must have a key size of at least {0} bits", new object[1]
				{
					MinKeySizeInBits
				}));
			}
			ClientId = clientId;
			Certificate = certificate;
		}

		internal byte[] Sign(string message)
		{
            return null;
		}
	}
}
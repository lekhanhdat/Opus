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
using Microsoft365.Authentication.ADAL.Internal;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// The AuthenticationContext class retrieves authentication tokens from Azure Active Directory and ADFS services.
	/// </summary>
	/// <summary>
	/// The main class representing the authority issuing tokens for resources.
	/// </summary>
	public sealed class AuthenticationContext
    {
		internal Authenticator Authenticator;

		/// <summary>
		/// Gets address of the authority to issue token.
		/// </summary>
		public string Authority => Authenticator.Authority;

		/// <summary>
		/// Gets a value indicating whether address validation is ON or OFF.
		/// </summary>
		public bool ValidateAuthority => Authenticator.ValidateAuthority;

		/// <summary>
		/// Gets the TokenCache
		/// </summary>
		/// <remarks>
		/// By default, TokenCache is an in-memory collection of key/value pairs. 
		/// Library will automatically save tokens in the cache when AcquireToken is called.  
		/// The default token cache is static so all tokens will available to all instances of AuthenticationContext. To use a custom TokenCache pass one to the <see cref="T:Portal.ADAL.AuthenticationContext">.constructor</see>.
		/// To turn OFF token caching, use the constructor and set TokenCache to null.
		/// </remarks>
		public TokenCache TokenCache
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets correlation Id which would be sent to the service with the next request. 
		/// Correlation Id is to be used for diagnostics purposes. 
		/// </summary>
		public Guid CorrelationId
		{
			get
			{
				return Authenticator.CorrelationId;
			}
			set
			{
				Authenticator.CorrelationId = value;
			}
		}

		static AuthenticationContext()
		{
			ADALLogger.Information(null, string.Format(CultureInfo.InvariantCulture, "ADAL {0} with assembly version '{1}', file version '{2}' and informational version '{3}' is running...", PlatformSpecificHelper.GetProductName(), AdalIdHelper.GetAdalVersion(), AdalIdHelper.GetAssemblyFileVersion(), AdalIdHelper.GetAssemblyInformationalVersion()));
		}

		/// <summary>
		/// Constructor to create the context with the address of the authority.
		/// Using this constructor will turn ON validation of the authority URL by default if validation is supported for the authority address.
		/// </summary>
		/// <param name="authority">Address of the authority to issue token.</param>
		public AuthenticationContext(string authority)
			: this(authority, AuthorityValidationType.NotProvided, TokenCache.DefaultShared)
		{
		}

		/// <summary>
		/// Constructor to create the context with the address of the authority and flag to turn address validation off.
		/// Using this constructor, address validation can be turned off. Make sure you are aware of the security implication of not validating the address.
		/// </summary>
		/// <param name="authority">Address of the authority to issue token.</param>
		/// <param name="validateAuthority">Flag to turn address validation ON or OFF.</param>
		public AuthenticationContext(string authority, bool validateAuthority)
			: this(authority, (!validateAuthority) ? AuthorityValidationType.False : AuthorityValidationType.True, TokenCache.DefaultShared)
		{
		}

		/// <summary>
		/// Constructor to create the context with the address of the authority.
		/// Using this constructor will turn ON validation of the authority URL by default if validation is supported for the authority address.
		/// </summary>
		/// <param name="authority">Address of the authority to issue token.</param>
		/// <param name="tokenCache">Token cache used to lookup cached tokens on calls to AcquireToken</param>
		public AuthenticationContext(string authority, TokenCache tokenCache)
			: this(authority, AuthorityValidationType.NotProvided, tokenCache)
		{
		}

		/// <summary>
		/// Constructor to create the context with the address of the authority and flag to turn address validation off.
		/// Using this constructor, address validation can be turned off. Make sure you are aware of the security implication of not validating the address.
		/// </summary>
		/// <param name="authority">Address of the authority to issue token.</param>
		/// <param name="validateAuthority">Flag to turn address validation ON or OFF.</param>
		/// <param name="tokenCache">Token cache used to lookup cached tokens on calls to AcquireToken</param>
		public AuthenticationContext(string authority, bool validateAuthority, TokenCache tokenCache)
			: this(authority, (!validateAuthority) ? AuthorityValidationType.False : AuthorityValidationType.True, tokenCache)
		{
		}

		private AuthenticationContext(string authority, AuthorityValidationType validateAuthority, TokenCache tokenCache)
		{
			Authenticator = new Authenticator(authority, validateAuthority != AuthorityValidationType.False);
			TokenCache = tokenCache;
		}

		/// <summary>
		/// Acquires security token from the authority.
		/// </summary>
		/// <remarks>This feature is supported only for Azure Active Directory and Active Directory Federation Services (ADFS) on Windows 10.</remarks> 
		/// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
		/// <param name="clientId">Identifier of the client requesting the token.</param>
		/// <param name="userCredential">The user credential to use for token acquisition.</param>
		/// <returns>It contains Access Token, Refresh Token and the Access Token's expiration time.</returns>
		public async Task<AuthenticationResult> AcquireTokenAsync(string resource, string clientId, UserCredential userCredential)
		{
			AcquireTokenNonInteractiveHandler handler = new AcquireTokenNonInteractiveHandler(Authenticator, TokenCache, resource, clientId, userCredential, false);
			return await handler.RunAsync();
		}
	}
}
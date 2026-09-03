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
using System.Text;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	internal class AcquireTokenNonInteractiveHandler : AcquireTokenHandlerBase
    {
		private readonly UserCredential userCredential;

		private UserAssertion userAssertion;

		public AcquireTokenNonInteractiveHandler(Authenticator authenticator, TokenCache tokenCache, string resource, string clientId, UserCredential userCredential, bool callSync)
			: base(authenticator, tokenCache, resource, new ClientKey(clientId), TokenSubjectType.User, userCredential, callSync)
		{
			if (userCredential == null)
			{
				throw new ArgumentNullException("userCredential");
			}
			this.userCredential = userCredential;
			base.SupportADFS = true;
		}

		
		protected override async Task PreRunAsync()
		{
			await base.PreRunAsync();
			if (userCredential != null)
			{
				if (string.IsNullOrWhiteSpace(userCredential.UserName))
				{
					userCredential.UserName = PlatformSpecificHelper.GetUserPrincipalName();
					if (string.IsNullOrWhiteSpace(userCredential.UserName))
					{
						ADALLogger.Information(base.CallState, "Could not find UPN for logged in user");
						throw new AdalException("unknown_user");
					}
					ADALLogger.Verbose(base.CallState, "Logged in user with hash '{0}' detected", PlatformSpecificHelper.CreateSha256Hash(userCredential.UserName));
				}
				base.DisplayableId = userCredential.UserName;
			}
			else if (userAssertion != null)
			{
				base.DisplayableId = userAssertion.UserName;
			}
		}

		protected override async Task PreTokenRequest()
		{
			await base.PreTokenRequest();
			if (userAssertion == null && base.Authenticator.AuthorityType != AuthorityType.ADFS)
			{
				UserRealmDiscoveryResponse userRealmResponse = await UserRealmDiscoveryResponse.CreateByDiscoveryAsync(base.Authenticator.UserRealmUri, userCredential.UserName, base.CallState);
				ADALLogger.Information(base.CallState, "User with hash '{0}' detected as '{1}'", PlatformSpecificHelper.CreateSha256Hash(userCredential.UserName), userRealmResponse.AccountType);
				if (string.Compare(userRealmResponse.AccountType, "federated", StringComparison.OrdinalIgnoreCase) == 0)
				{
					if (string.IsNullOrWhiteSpace(userRealmResponse.FederationMetadataUrl))
					{
						throw new AdalException("missing_federation_metadata_url");
					}
					WsTrustAddress wsTrustAddress = await MexParser.FetchWsTrustAddressFromMexAsync(userRealmResponse.FederationMetadataUrl, userCredential.UserAuthType, base.CallState);
					ADALLogger.Information(base.CallState, "WS-Trust endpoint '{0}' fetched from MEX at '{1}'", wsTrustAddress.Uri, userRealmResponse.FederationMetadataUrl);
					WsTrustResponse wsTrustResponse = await WsTrustRequest.SendRequestAsync(wsTrustAddress, userCredential, base.CallState);
					ADALLogger.Information(base.CallState, "Token of type '{0}' acquired from WS-Trust endpoint", wsTrustResponse.TokenType);
					userAssertion = new UserAssertion(wsTrustResponse.Token, (wsTrustResponse.TokenType == "urn:oasis:names:tc:SAML:1.0:assertion") ? "urn:ietf:params:oauth:grant-type:saml1_1-bearer" : "urn:ietf:params:oauth:grant-type:saml2-bearer");
				}
				else
				{
					if (string.Compare(userRealmResponse.AccountType, "managed", StringComparison.OrdinalIgnoreCase) != 0)
					{
						throw new AdalException("unknown_user_type");
					}
					if (userCredential.PasswordToCharArray() == null)
					{
						throw new AdalException("password_required_for_managed_user");
					}
				}
			}
		}

		protected override void AddAditionalRequestParameters(RequestParameters requestParameters)
		{
			if (userAssertion != null)
			{
				requestParameters["grant_type"] = userAssertion.AssertionType;
				requestParameters["assertion"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(userAssertion.Assertion));
			}
			else
			{
				requestParameters["grant_type"] = "password";
				requestParameters["username"] = userCredential.UserName;
				if (userCredential.SecurePassword != null)
				{
					requestParameters.AddSecureParameter("password", userCredential.SecurePassword);
				}
			}
			requestParameters["scope"] = "openid";
		}
	}
}
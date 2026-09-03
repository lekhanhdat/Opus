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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	internal class Authenticator
	{

		private static readonly AuthenticatorTemplateList AuthenticatorTemplateList = new AuthenticatorTemplateList();

		private bool updatedFromTemplate;

		public string Authority
		{
			get;
			private set;
		}

		public AuthorityType AuthorityType
		{
			get;
			private set;
		}

		public bool ValidateAuthority
		{
			get;
			private set;
		}

		public bool IsTenantless
		{
			get;
			private set;
		}

		public string AuthorizationUri
		{
			get;
			set;
		}

		public string TokenUri
		{
			get;
			private set;
		}

		public string UserRealmUri
		{
			get;
			private set;
		}

		public string SelfSignedJwtAudience
		{
			get;
			private set;
		}

		public Guid CorrelationId
		{
			get;
			set;
		}

		public Authenticator(string authority, bool validateAuthority)
		{
			Authority = CanonicalizeUri(authority);
			AuthorityType = DetectAuthorityType(Authority);
			if (AuthorityType != 0 && validateAuthority)
			{
				throw new ArgumentException("Authority validation is not supported for this type of authority", "validateAuthority");
			}
			ValidateAuthority = validateAuthority;
		}

		public async Task UpdateFromTemplateAsync(CallState callState)
		{
			if (!updatedFromTemplate)
			{
				Uri authorityUri = new Uri(Authority);
				string host = authorityUri.Authority;
				string path = authorityUri.AbsolutePath.Substring(1);
				string tenant = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
				AuthenticatorTemplate matchingTemplate = await AuthenticatorTemplateList.FindMatchingItemAsync(ValidateAuthority, host, tenant, callState);
				AuthorizationUri = matchingTemplate.AuthorizeEndpoint.Replace("{tenant}", tenant);
				TokenUri = matchingTemplate.TokenEndpoint.Replace("{tenant}", tenant);
				UserRealmUri = CanonicalizeUri(matchingTemplate.UserRealmEndpoint);
				IsTenantless = (string.Compare(tenant, "Common", StringComparison.OrdinalIgnoreCase) == 0);
				SelfSignedJwtAudience = matchingTemplate.Issuer.Replace("{tenant}", tenant);
				updatedFromTemplate = true;
			}
		}

		public void UpdateTenantId(string tenantId)
		{
			if (IsTenantless && !string.IsNullOrWhiteSpace(tenantId))
			{
				Authority = ReplaceTenantlessTenant(Authority, tenantId);
				updatedFromTemplate = false;
			}
		}

		internal static AuthorityType DetectAuthorityType(string authority)
		{
			if (string.IsNullOrWhiteSpace(authority))
			{
				throw new ArgumentNullException("authority");
			}
			if (!Uri.IsWellFormedUriString(authority, UriKind.Absolute))
			{
				throw new ArgumentException("'authority' should be in Uri format", "authority");
			}
			Uri uri = new Uri(authority);
			if (uri.Scheme != "https")
			{
				throw new ArgumentException("'authority' should use the 'https' scheme", "authority");
			}
			string text = uri.AbsolutePath.Substring(1);
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new ArgumentException("'authority' Uri should have at least one segment in the path (i.e. https://<host>/<path>/...)", "authority");
			}
			string firstPath = text.Substring(0, text.IndexOf("/", StringComparison.Ordinal));
			return IsAdfsAuthority(firstPath) ? AuthorityType.ADFS : AuthorityType.AAD;
		}

		private static string CanonicalizeUri(string uri)
		{
			if (!string.IsNullOrWhiteSpace(uri) && !uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				uri += "/";
			}
			return uri;
		}

		private static string ReplaceTenantlessTenant(string authority, string tenantId)
		{
			Regex regex = new Regex(Regex.Escape("Common"), RegexOptions.IgnoreCase);
			return regex.Replace(authority, tenantId, 1);
		}

		private static bool IsAdfsAuthority(string firstPath)
		{
			return string.Compare(firstPath, "adfs", StringComparison.OrdinalIgnoreCase) == 0;
		}
	}
}
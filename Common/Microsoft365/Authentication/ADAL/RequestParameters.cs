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
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System;

namespace Microsoft365.Authentication.ADAL
{
    [Serializable]
    internal class RequestParameters : Dictionary<string, string>
	{
		private readonly StringBuilder stringBuilderParameter;

		private Dictionary<string, SecureString> secureParameters;

		public string ExtraQueryParameter
		{
			get;
			set;
		}

		public RequestParameters(string resource, ClientKey clientKey)
		{
			if (!string.IsNullOrWhiteSpace(resource))
			{
				base["resource"] = resource;
			}
			AddClientKey(clientKey);
		}

		public RequestParameters(StringBuilder stringBuilderParameter)
		{
			this.stringBuilderParameter = stringBuilderParameter;
		}

		public override string ToString()
		{
			return ToStringBuilder().ToString();
		}

		public void WriteToStream(Stream stream)
		{
			StringBuilder stringBuilder = ToStringBuilder();
			byte[] array = null;
			try
			{
				array = stringBuilder.ToByteArray();
				stream.Write(array, 0, array.Length);
			}
			finally
			{
				array.SecureClear();
				stringBuilder.SecureClear();
			}
		}

		private StringBuilder ToStringBuilder()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stringBuilderParameter != null)
			{
				stringBuilder.Append(stringBuilderParameter);
			}
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> current = enumerator.Current;
					EncodingHelper.AddKeyValueString(stringBuilder, EncodingHelper.UrlEncode(current.Key), EncodingHelper.UrlEncode(current.Value));
				}
			}
			AddSecureParametersToMessageBuilder(stringBuilder);
			if (ExtraQueryParameter != null)
			{
				stringBuilder.Append('&' + ExtraQueryParameter);
			}
			return stringBuilder;
		}

		public void AddSecureParameter(string key, SecureString value)
		{
			if (secureParameters == null)
			{
				secureParameters = new Dictionary<string, SecureString>();
			}
			secureParameters.Add(key, value);
		}

		private void AddSecureParametersToMessageBuilder(StringBuilder messageBuilder)
		{
			if (secureParameters != null)
			{
				foreach (KeyValuePair<string, SecureString> secureParameter in secureParameters)
				{
					char[] array = null;
					try
					{
						array = secureParameter.Value.ToCharArray();
						EncodingHelper.AddStringWithUrlEncoding(messageBuilder, secureParameter.Key, array);
					}
					finally
					{
						array.SecureClear();
					}
				}
			}
		}

		private void AddClientKey(ClientKey clientKey)
		{
			if (clientKey.ClientId != null)
			{
				base["client_id"] = clientKey.ClientId;
			}
			if (clientKey.Credential != null)
			{
				if (clientKey.Credential.ClientSecret != null)
				{
					base["client_secret"] = clientKey.Credential.ClientSecret;
				}
				else
				{
					AddSecureParameter("client_secret", clientKey.Credential.SecureClientSecret);
				}
			}
			else if (clientKey.Assertion != null)
			{
				base["client_assertion_type"] = clientKey.Assertion.AssertionType;
				base["client_assertion"] = clientKey.Assertion.Assertion;
			}
			else if (clientKey.Certificate != null)
			{
				JsonWebToken jsonWebToken = new JsonWebToken(clientKey.Certificate, clientKey.Authenticator.SelfSignedJwtAudience);
				ClientAssertion clientAssertion = jsonWebToken.Sign(clientKey.Certificate);
				base["client_assertion_type"] = clientAssertion.AssertionType;
				base["client_assertion"] = clientAssertion.Assertion;
			}
		}
	}
}
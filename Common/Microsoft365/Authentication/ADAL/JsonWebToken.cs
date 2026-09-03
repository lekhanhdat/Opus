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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Microsoft365.Authentication.ADAL
{
	internal class JsonWebToken
	{
		[DataContract]
		internal class JWTHeader
		{
			protected ClientAssertionCertificate Credential
			{
				get;
				private set;
			}

			[DataMember(Name = "typ")]
			public static string Type
			{
				get
				{
					return "JWT";
				}
				set
				{
				}
			}

			[DataMember(Name = "alg")]
			public string Algorithm
			{
				get
				{
					if (Credential != null)
					{
						return "RS256";
					}
					return "none";
				}
				set
				{
				}
			}

			public JWTHeader(ClientAssertionCertificate credential)
			{
				Credential = credential;
			}
		}

		[DataContract]
		internal class JWTPayload
		{
			[DataMember(Name = "aud")]
			public string Audience
			{
				get;
				set;
			}

			[DataMember(Name = "iss")]
			public string Issuer
			{
				get;
				set;
			}

			[DataMember(Name = "nbf")]
			public long ValidFrom
			{
				get;
				set;
			}

			[DataMember(Name = "exp")]
			public long ValidTo
			{
				get;
				set;
			}

			[DataMember(Name = "sub", IsRequired = false, EmitDefaultValue = false)]
			public string Subject
			{
				get;
				set;
			}

			[DataMember(Name = "jti", IsRequired = false, EmitDefaultValue = false)]
			public string JwtIdentifier
			{
				get;
				set;
			}
		}

		[DataContract]
		internal sealed class JWTHeaderWithCertificate : JWTHeader
		{
			[DataMember(Name = "x5t")]
			public string X509CertificateThumbprint
			{
				get
				{
					return Base64UrlEncoder.Encode(base.Credential.Certificate.GetCertHash());
				}
				set
				{
				}
			}

			public JWTHeaderWithCertificate(ClientAssertionCertificate credential)
				: base(credential)
			{
			}
		}


		private readonly JWTPayload payload;

		public JsonWebToken(ClientAssertionCertificate certificate, string audience)
		{
			DateTime jsonWebTokenValidFrom = NetworkPlugin.RequestCreationHelper.GetJsonWebTokenValidFrom();
			DateTime time = jsonWebTokenValidFrom + TimeSpan.FromSeconds(600.0);
			payload = new JWTPayload
			{
				Audience = audience,
				Issuer = certificate.ClientId,
				ValidFrom = DateTimeHelper.ConvertToTimeT(jsonWebTokenValidFrom),
				ValidTo = DateTimeHelper.ConvertToTimeT(time),
				Subject = certificate.ClientId
			};
			payload.JwtIdentifier = NetworkPlugin.RequestCreationHelper.GetJsonWebTokenId();
		}

		public ClientAssertion Sign(ClientAssertionCertificate credential)
		{
			string text = Encode(credential);
			if (65536 < text.Length)
			{
				throw new AdalException("encoded_token_too_long");
			}
			return new ClientAssertion(payload.Issuer, text + "." + UrlEncodeSegment(credential.Sign(text)));
		}

		private static string EncodeSegment(string segment)
		{
			return UrlEncodeSegment(Encoding.UTF8.GetBytes(segment));
		}

		private static string UrlEncodeSegment(byte[] segment)
		{
			return Base64UrlEncoder.Encode(segment);
		}

		private static string EncodeToJson<T>(T toEncode)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(T));
				dataContractJsonSerializer.WriteObject(memoryStream, toEncode);
				return Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Position);
			}
		}

		private static string EncodeHeaderToJson(ClientAssertionCertificate credential)
		{
			JWTHeaderWithCertificate toEncode = new JWTHeaderWithCertificate(credential);
			return EncodeToJson(toEncode);
		}

		private string Encode(ClientAssertionCertificate credential)
		{
			string segment = EncodeHeaderToJson(credential);
			string str = EncodeSegment(segment);
			string segment2 = EncodePayloadToJson();
			string str2 = EncodeSegment(segment2);
			return str + "." + str2;
		}

		private string EncodePayloadToJson()
		{
			return EncodeToJson(payload);
		}
	}
}
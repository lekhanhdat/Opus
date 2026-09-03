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
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft365.Authentication.ADAL
{
	internal class WsTrustResponse
	{
		public const string Saml1Assertion = "urn:oasis:names:tc:SAML:1.0:assertion";

		public string Token
		{
			get;
			private set;
		}

		public string TokenType
		{
			get;
			private set;
		}

		public static WsTrustResponse CreateFromResponse(Stream responseStream, WsTrustVersion version)
		{
			XDocument responseDocument = ReadDocumentFromResponse(responseStream);
			return CreateFromResponseDocument(responseDocument, version);
		}

		public static string ReadErrorResponse(XDocument responseDocument, CallState callState)
		{
			string result = null;
			try
			{
				XElement xElement = responseDocument.Descendants(XmlNamespace.SoapEnvelope + "Body").FirstOrDefault();
				if (xElement == null)
				{
					return result;
				}
				XElement xElement2 = xElement.Elements(XmlNamespace.SoapEnvelope + "Fault").FirstOrDefault();
				if (xElement2 == null)
				{
					return result;
				}
				XElement xElement3 = xElement2.Elements(XmlNamespace.SoapEnvelope + "Reason").FirstOrDefault();
				if (xElement3 == null)
				{
					return result;
				}
				XElement xElement4 = xElement3.Elements(XmlNamespace.SoapEnvelope + "Text").FirstOrDefault();
				if (xElement4 != null)
				{
					using (XmlReader xmlReader = xElement4.CreateReader())
					{
						xmlReader.MoveToContent();
						return xmlReader.ReadInnerXml();
					}
				}
				return result;
			}
			catch (XmlException innerException)
			{
				throw new AdalException("parsing_wstrust_response_failed", innerException);
			}
		}

		internal static XDocument ReadDocumentFromResponse(Stream responseStream)
		{
			try
			{
				return XDocument.Load(responseStream, LoadOptions.None);
			}
			catch (XmlException innerException)
			{
				throw new AdalException("parsing_wstrust_response_failed", innerException);
			}
		}

		internal static WsTrustResponse CreateFromResponseDocument(XDocument responseDocument, WsTrustVersion version)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				XNamespace ns = XmlNamespace.Trust;
				if (version == WsTrustVersion.WsTrust2005)
				{
					ns = XmlNamespace.Trust2005;
				}
				bool flag = true;
				if (version == WsTrustVersion.WsTrust13)
				{
					XElement xElement = responseDocument.Descendants(ns + "RequestSecurityTokenResponseCollection").FirstOrDefault();
					if (xElement == null)
					{
						flag = false;
					}
				}
				if (flag)
				{
					IEnumerable<XElement> enumerable = responseDocument.Descendants(ns + "RequestSecurityTokenResponse");
					foreach (XElement item in enumerable)
					{
						XElement xElement2 = item.Elements(ns + "TokenType").FirstOrDefault();
						if (xElement2 != null)
						{
							XElement xElement3 = item.Elements(ns + "RequestedSecurityToken").FirstOrDefault();
							if (xElement3 != null)
							{
								dictionary.Add(xElement2.Value, xElement3.FirstNode.ToString(SaveOptions.DisableFormatting));
							}
						}
					}
				}
			}
			catch (XmlException innerException)
			{
				throw new AdalException("parsing_wstrust_response_failed", innerException);
			}
			if (dictionary.Count == 0)
			{
				throw new AdalException("parsing_wstrust_response_failed");
			}
			string text = dictionary.ContainsKey("urn:oasis:names:tc:SAML:1.0:assertion") ? "urn:oasis:names:tc:SAML:1.0:assertion" : dictionary.Keys.First();
			WsTrustResponse wsTrustResponse = new WsTrustResponse();
			wsTrustResponse.TokenType = text;
			wsTrustResponse.Token = dictionary[text];
			return wsTrustResponse;
		}
	}
}
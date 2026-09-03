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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft365.Authentication.ADAL
{
	internal class MexParser
	{

		public static async Task<WsTrustAddress> FetchWsTrustAddressFromMexAsync(string federationMetadataUrl, UserAuthType userAuthType, CallState callState)
		{
			return ExtractWsTrustAddressFromMex(await FetchMexAsync(federationMetadataUrl, callState), userAuthType, callState);
		}

		internal static async Task<XDocument> FetchMexAsync(string federationMetadataUrl, CallState callState)
		{
			int num = default(int);
			int num2 = num;
			try
			{
				IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(federationMetadataUrl);
				request.Method = "GET";
				request.ContentType = "application/soap+xml";
				using (IHttpWebResponse response = await request.GetResponseSyncOrAsync(callState))
				{
					return XDocument.Load(response.GetResponseStream(), LoadOptions.None);
				}
			}
			catch (WebException innerException)
			{
				throw new AdalServiceException("accessing_ws_metadata_exchange_failed", innerException);
			}
			catch (XmlException innerException2)
			{
				throw new AdalException("parsing_ws_metadata_exchange_failed", innerException2);
			}
		}

		internal static WsTrustAddress ExtractWsTrustAddressFromMex(XDocument mexDocument, UserAuthType userAuthType, CallState callState)
		{
			WsTrustAddress wsTrustAddress = null;
			MexPolicy mexPolicy = null;
			try
			{
				Dictionary<string, MexPolicy> dictionary = ReadPolicies(mexDocument);
				Dictionary<string, MexPolicy> bindings = ReadPolicyBindings(mexDocument, dictionary);
				SetPolicyEndpointAddresses(mexDocument, bindings);
                /* Fortify Issue Type: Insecure Randomness 
				* Sink Details: this nethod 
				* Ignore Reason: random用于打乱mexpolicy排序，不涉及安全问题 
				*/
                Random random = new Random();
				mexPolicy = ((from p in dictionary.Values.Where(delegate(MexPolicy p)
				{
					if (p.Url != null && p.AuthType == userAuthType)
					{
						return p.Version == WsTrustVersion.WsTrust13;
					}
					return false;
				})
				orderby random.Next()
				select p).FirstOrDefault() ?? (from p in dictionary.Values.Where(delegate(MexPolicy p)
				{
					if (p.Url != null)
					{
						return p.AuthType == userAuthType;
					}
					return false;
				})
				orderby random.Next()
				select p).FirstOrDefault());
				if (mexPolicy == null)
				{
					if (userAuthType == UserAuthType.IntegratedAuth)
					{
						throw new AdalException("integrated_authentication_failed", new AdalException("wstrust_endpoint_not_found"));
					}
					throw new AdalException("wstrust_endpoint_not_found");
				}
				wsTrustAddress = new WsTrustAddress();
				wsTrustAddress.Uri = mexPolicy.Url;
				wsTrustAddress.Version = mexPolicy.Version;
				return wsTrustAddress;
			}
			catch (XmlException innerException)
			{
				throw new AdalException("parsing_ws_metadata_exchange_failed", innerException);
			}
		}

		internal static Dictionary<string, MexPolicy> ReadPolicies(XContainer mexDocument)
		{
			Dictionary<string, MexPolicy> dictionary = new Dictionary<string, MexPolicy>();
			IEnumerable<XElement> enumerable = mexDocument.Elements().First().Elements(XmlNamespace.Wsp + "Policy");
			foreach (XElement item in enumerable)
			{
				XElement xElement = item.Elements(XmlNamespace.Wsp + "ExactlyOne").FirstOrDefault();
				if (xElement != null)
				{
					IEnumerable<XElement> enumerable2 = xElement.Descendants(XmlNamespace.Wsp + "All");
					foreach (XElement item2 in enumerable2)
					{
						XNamespace ns = XmlNamespace.Sp;
						XElement xElement2 = item2.Elements(XmlNamespace.Http + "NegotiateAuthentication").FirstOrDefault();
						if (xElement2 != null)
						{
							AddPolicy(dictionary, item, UserAuthType.IntegratedAuth);
						}
						xElement2 = item2.Elements(ns + "SignedEncryptedSupportingTokens").FirstOrDefault();
						if (xElement2 == null)
						{
							ns = XmlNamespace.Sp2005;
							if ((xElement2 = item2.Elements(ns + "SignedSupportingTokens").FirstOrDefault()) == null)
							{
								continue;
							}
						}
						XElement xElement3 = xElement2.Elements(XmlNamespace.Wsp + "Policy").FirstOrDefault();
						if (xElement3 != null)
						{
							XElement xElement4 = xElement3.Elements(ns + "UsernameToken").FirstOrDefault();
							if (xElement4 != null)
							{
								XElement xElement5 = xElement4.Elements(XmlNamespace.Wsp + "Policy").FirstOrDefault();
								if (xElement5 != null)
								{
									XElement xElement6 = xElement5.Elements(ns + "WssUsernameToken10").FirstOrDefault();
									if (xElement6 != null)
									{
										AddPolicy(dictionary, item, UserAuthType.UsernamePassword);
									}
								}
							}
						}
					}
				}
			}
			return dictionary;
		}

		private static Dictionary<string, MexPolicy> ReadPolicyBindings(XContainer mexDocument, IReadOnlyDictionary<string, MexPolicy> policies)
		{
			Dictionary<string, MexPolicy> dictionary = new Dictionary<string, MexPolicy>();
			IEnumerable<XElement> enumerable = mexDocument.Elements().First().Elements(XmlNamespace.Wsdl + "binding");
			foreach (XElement item in enumerable)
			{
				IEnumerable<XElement> enumerable2 = item.Elements(XmlNamespace.Wsp + "PolicyReference");
				foreach (XElement item2 in enumerable2)
				{
					XAttribute xAttribute = item2.Attribute("URI");
					if (xAttribute != null && policies.ContainsKey(xAttribute.Value))
					{
						XAttribute xAttribute2 = item.Attribute("name");
						if (xAttribute2 != null)
						{
							XElement xElement = item.Elements(XmlNamespace.Wsdl + "operation").FirstOrDefault();
							if (xElement != null)
							{
								XElement xElement2 = xElement.Elements(XmlNamespace.Soap12 + "operation").FirstOrDefault();
								if (xElement2 != null)
								{
									XAttribute xAttribute3 = xElement2.Attribute("soapAction");
									if (xAttribute3 != null && (string.Compare(XmlNamespace.Issue.ToString(), xAttribute3.Value, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(XmlNamespace.Issue2005.ToString(), xAttribute3.Value, StringComparison.OrdinalIgnoreCase) == 0))
									{
										bool flag = string.Compare(XmlNamespace.Issue2005.ToString(), xAttribute3.Value, StringComparison.OrdinalIgnoreCase) == 0;
										policies[xAttribute.Value].Version = (flag ? WsTrustVersion.WsTrust2005 : WsTrustVersion.WsTrust13);
										XElement xElement3 = item.Elements(XmlNamespace.Soap12 + "binding").FirstOrDefault();
										if (xElement3 != null)
										{
											XAttribute xAttribute4 = xElement3.Attribute("transport");
											if (xAttribute4 != null && string.Compare("http://schemas.xmlsoap.org/soap/http", xAttribute4.Value, StringComparison.OrdinalIgnoreCase) == 0)
											{
												dictionary.Add(xAttribute2.Value, policies[xAttribute.Value]);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return dictionary;
		}

		private static void SetPolicyEndpointAddresses(XContainer mexDocument, IReadOnlyDictionary<string, MexPolicy> bindings)
		{
			XElement xElement = mexDocument.Elements().First().Elements(XmlNamespace.Wsdl + "service")
				.First();
			IEnumerable<XElement> enumerable = xElement.Elements(XmlNamespace.Wsdl + "port");
			foreach (XElement item in enumerable)
			{
				XAttribute xAttribute = item.Attribute("binding");
				if (xAttribute != null)
				{
					string value = xAttribute.Value;
					string[] array = value.Split(new char[1]
					{
						':'
					}, 2);
					if (array.Length >= 2 && bindings.ContainsKey(array[1]))
					{
						XElement xElement2 = item.Elements(XmlNamespace.Wsa10 + "EndpointReference").FirstOrDefault();
						if (xElement2 != null)
						{
							XElement xElement3 = xElement2.Elements(XmlNamespace.Wsa10 + "Address").FirstOrDefault();
							if (xElement3 != null && Uri.IsWellFormedUriString(xElement3.Value, UriKind.Absolute))
							{
								bindings[array[1]].Url = new Uri(xElement3.Value);
							}
						}
					}
				}
			}
		}

		private static void AddPolicy(IDictionary<string, MexPolicy> policies, XElement policy, UserAuthType policyAuthType)
		{
			XElement xElement = policy.Descendants(XmlNamespace.Sp + "TransportBinding").FirstOrDefault() ?? policy.Descendants(XmlNamespace.Sp2005 + "TransportBinding").FirstOrDefault();
			if (xElement != null)
			{
				XAttribute xAttribute = policy.Attribute(XmlNamespace.Wsu + "Id");
				if (xAttribute != null)
				{
					policies.Add("#" + xAttribute.Value, new MexPolicy
					{
						Id = xAttribute.Value,
						AuthType = policyAuthType
					});
				}
			}
		}
	}
}
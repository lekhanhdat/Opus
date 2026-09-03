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
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Microsoft365.Authentication.ADAL
{
	internal static class WsTrustRequest
	{


		public static async Task<WsTrustResponse> SendRequestAsync(WsTrustAddress wsTrustAddress, UserCredential credential, CallState callState)
		{
			IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(wsTrustAddress.Uri.AbsoluteUri);
			request.ContentType = "application/soap+xml;";
			if (credential.UserAuthType == UserAuthType.IntegratedAuth)
			{
				SetKerberosOption(request);
			}
			StringBuilder messageBuilder = BuildMessage("urn:federation:MicrosoftOnline", wsTrustAddress, credential);
			string soapAction = XmlNamespace.Issue.ToString();
			if (wsTrustAddress.Version == WsTrustVersion.WsTrust2005)
			{
				soapAction = XmlNamespace.Issue2005.ToString();
			}
			Dictionary<string, string> headers = new Dictionary<string, string>
			{
				{
					"SOAPAction",
					soapAction
				}
			};
			try
			{
				HttpHelper.SetPostRequest(request, new RequestParameters(messageBuilder), callState, headers);
				return WsTrustResponse.CreateFromResponse((await request.GetResponseSyncOrAsync(callState)).GetResponseStream(), wsTrustAddress.Version);
			}
			catch (WebException ex)
			{
				string arg;
				try
				{
					XDocument responseDocument = WsTrustResponse.ReadDocumentFromResponse(ex.Response.GetResponseStream());
					arg = WsTrustResponse.ReadErrorResponse(responseDocument, callState);
				}
				catch (AdalException)
				{
					arg = "See inner exception for detail.";
				}
				throw new AdalServiceException("federated_service_returned_error", $"Federated service at {wsTrustAddress.Uri} returned error: {arg}", null, ex);
			}
		}

		private static void SetKerberosOption(IHttpWebRequest request)
		{
			request.UseDefaultCredentials = true;
		}

		public static StringBuilder BuildMessage(string appliesTo, WsTrustAddress wsTrustAddress, UserCredential credential)
		{
			StringBuilder stringBuilder = BuildSecurityHeader(wsTrustAddress, credential);
			string text = Guid.NewGuid().ToString();
			StringBuilder stringBuilder2 = new StringBuilder(1024);
			string text2 = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
			string text3 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue";
			string text4 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
			string text5 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Bearer";
			string text6 = "http://docs.oasis-open.org/ws-sx/ws-trust/200512/Issue";
			if (wsTrustAddress.Version == WsTrustVersion.WsTrust2005)
			{
				text3 = "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue";
				text4 = "http://schemas.xmlsoap.org/ws/2005/02/trust";
				text5 = "http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey";
				text6 = "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue";
			}
			stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "<s:Envelope xmlns:s='http://www.w3.org/2003/05/soap-envelope' xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:u='{0}'>\r\n              <s:Header>\r\n              <a:Action s:mustUnderstand='1'>{1}</a:Action>\r\n              <a:messageID>urn:uuid:{2}</a:messageID>\r\n              <a:ReplyTo><a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address></a:ReplyTo>\r\n              <a:To s:mustUnderstand='1'>{3}</a:To>\r\n              {4}\r\n              </s:Header>\r\n              <s:Body>\r\n              <trust:RequestSecurityToken xmlns:trust='{5}'>\r\n              <wsp:AppliesTo xmlns:wsp='http://schemas.xmlsoap.org/ws/2004/09/policy'>\r\n              <a:EndpointReference>\r\n              <a:Address>{6}</a:Address>\r\n              </a:EndpointReference>\r\n              </wsp:AppliesTo>\r\n              <trust:KeyType>{7}</trust:KeyType>\r\n              <trust:RequestType>{8}</trust:RequestType>\r\n              </trust:RequestSecurityToken>\r\n              </s:Body>\r\n              </s:Envelope>", text2, text3, text, wsTrustAddress.Uri, stringBuilder, text4, appliesTo, text5, text6);
			stringBuilder.SecureClear();
			return stringBuilder2;
		}

		internal static string XmlEscape(string escapeStr)
		{
			escapeStr = escapeStr.Replace("&", "&amp;");
			escapeStr = escapeStr.Replace("\"", "&quot;");
			escapeStr = escapeStr.Replace("'", "&apos;");
			escapeStr = escapeStr.Replace("<", "&lt;");
			escapeStr = escapeStr.Replace(">", "&gt;");
			return escapeStr;
		}

		private static StringBuilder BuildSecurityHeader(WsTrustAddress address, UserCredential credential)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			if (credential.UserAuthType == UserAuthType.UsernamePassword)
			{
				StringBuilder stringBuilder2 = new StringBuilder(1024);
				string text = Guid.NewGuid().ToString();
				stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "<o:UsernameToken u:Id='uuid-{0}'><o:Username>{1}</o:Username><o:Password>", new object[2]
				{
					text,
					credential.UserName
				});
				char[] array = null;
				try
				{
					array = credential.PasswordToCharArray();
					string value = XmlEscape(new string(array));
					stringBuilder2.Append(value);
					value = "";
				}
				finally
				{
					array.SecureClear();
				}
				stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "</o:Password></o:UsernameToken>");
				DateTime utcNow = DateTime.UtcNow;
				string text2 = DateTimeHelper.BuildTimeString(utcNow);
				DateTime utcTime = utcNow.AddMinutes(10.0);
				string text3 = DateTimeHelper.BuildTimeString(utcTime);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "<o:Security s:mustUnderstand='1' xmlns:o='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd'><u:Timestamp u:Id='_0'><u:Created>{0}</u:Created><u:Expires>{1}</u:Expires></u:Timestamp>{2}</o:Security>", new object[3]
				{
					text2,
					text3,
					stringBuilder2
				});
				stringBuilder2.SecureClear();
			}
			return stringBuilder;
		}
	}
}
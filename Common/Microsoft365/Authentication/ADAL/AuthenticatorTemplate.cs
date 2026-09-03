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
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	[DataContract]
	internal class AuthenticatorTemplate
	{
		[DataContract]
		internal sealed class InstanceDiscoveryResponse
		{
			[DataMember(Name = "tenant_discovery_endpoint")]
			public string TenantDiscoveryEndpoint
			{
				get;
				set;
			}
		}



		[DataMember]
		public string Host
		{
			get;
			internal set;
		}

		[DataMember]
		public string Issuer
		{
			get;
			internal set;
		}

		[DataMember]
		public string Authority
		{
			get;
			internal set;
		}

		[DataMember]
		public string InstanceDiscoveryEndpoint
		{
			get;
			internal set;
		}

		[DataMember]
		public string AuthorizeEndpoint
		{
			get;
			internal set;
		}

		[DataMember]
		public string TokenEndpoint
		{
			get;
			internal set;
		}

		[DataMember]
		public string UserRealmEndpoint
		{
			get;
			internal set;
		}

		public static AuthenticatorTemplate CreateFromHost(string host)
		{
			string s = "{\"Host\":\"{host}\", \"Authority\":\"https://{host}/{tenant}/\", \"InstanceDiscoveryEndpoint\":\"https://{host}/common/discovery/instance\", \"AuthorizeEndpoint\":\"https://{host}/{tenant}/oauth2/authorize\", \"TokenEndpoint\":\"https://{host}/{tenant}/oauth2/token\", \"UserRealmEndpoint\":\"https://{host}/common/UserRealm\"}".Replace("{host}", host);
			DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(AuthenticatorTemplate));
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				AuthenticatorTemplate authenticatorTemplate = (AuthenticatorTemplate)dataContractJsonSerializer.ReadObject(stream);
				authenticatorTemplate.Issuer = authenticatorTemplate.TokenEndpoint;
				return authenticatorTemplate;
			}
		}

		public async Task VerifyAnotherHostByInstanceDiscoveryAsync(string host, string tenant, CallState callState)
		{
			string instanceDiscoveryEndpoint = InstanceDiscoveryEndpoint;
			instanceDiscoveryEndpoint += "?api-version=1.0&authorization_endpoint=https://{host}/{tenant}/oauth2/authorize";
			instanceDiscoveryEndpoint = instanceDiscoveryEndpoint.Replace("{host}", host);
			instanceDiscoveryEndpoint = instanceDiscoveryEndpoint.Replace("{tenant}", tenant);
			instanceDiscoveryEndpoint = HttpHelper.CheckForExtraQueryParameter(instanceDiscoveryEndpoint);
			ClientMetrics clientMetrics = new ClientMetrics();
			try
			{
				int num = default(int);
				int num2 = num;
				try
				{
					IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(instanceDiscoveryEndpoint);
					request.Method = "GET";
					HttpHelper.AddCorrelationIdHeadersToRequest(request, callState);
					AdalIdHelper.AddAsHeaders(request);
					clientMetrics.BeginClientMetricsRecord(request, callState);
					using (IHttpWebResponse response = await request.GetResponseSyncOrAsync(callState))
					{
						HttpHelper.VerifyCorrelationIdHeaderInReponse(response, callState);
						InstanceDiscoveryResponse instanceDiscoveryResponse = HttpHelper.DeserializeResponse<InstanceDiscoveryResponse>(response);
						clientMetrics.SetLastError(null);
						if (instanceDiscoveryResponse.TenantDiscoveryEndpoint == null)
						{
							throw new AdalException("authority_not_in_valid_list");
						}
					}
				}
				catch (WebException ex)
				{
					TokenResponse tokenResponse = OAuth2Response.ReadErrorResponse(ex.Response);
					clientMetrics.SetLastError(tokenResponse?.ErrorCodes);
					if (tokenResponse == null)
					{ 
						throw new ArgumentNullException(nameof(tokenResponse));
					}
					
					if (tokenResponse.Error == "invalid_instance")
					{
						throw new AdalServiceException("authority_not_in_valid_list", ex);
					}
					throw new AdalServiceException("authority_validation_failed", string.Format(CultureInfo.InvariantCulture, "{0}. {1}: {2}", new object[3]
					{
						"Authority validation failed",
						tokenResponse.Error,
						tokenResponse.ErrorDescription
					}), tokenResponse.ErrorCodes, ex);
				}
			}
			finally
			{
				clientMetrics.EndClientMetricsRecord("instance", callState);
			}
		}
	}
}
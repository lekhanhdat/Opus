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
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft365.Authentication.ADAL
{
	[DataContract]
	internal sealed class UserRealmDiscoveryResponse
	{
		[DataMember(Name = "ver")]
		public string Version
		{
			get;
			set;
		}

		[DataMember(Name = "account_type")]
		public string AccountType
		{
			get;
			set;
		}

		[DataMember(Name = "federation_protocol")]
		public string FederationProtocol
		{
			get;
			set;
		}

		[DataMember(Name = "federation_metadata_url")]
		public string FederationMetadataUrl
		{
			get;
			set;
		}

		[DataMember(Name = "federation_active_auth_url")]
		public string FederationActiveAuthUrl
		{
			get;
			set;
		}

		internal static async Task<UserRealmDiscoveryResponse> CreateByDiscoveryAsync(string userRealmUri, string userName, CallState callState)
		{
			string userRealmEndpoint = userRealmUri + userName + "?api-version=1.0";
			userRealmEndpoint = HttpHelper.CheckForExtraQueryParameter(userRealmEndpoint);
			ADALLogger.Information(callState, "Sending user realm discovery request to '{0}'", userRealmEndpoint);
			ClientMetrics clientMetrics = new ClientMetrics();
			try
			{
				int num = default(int);
				int num2 = num;
				try
				{
					IHttpWebRequest request = NetworkPlugin.HttpWebRequestFactory.Create(userRealmEndpoint);
					request.Method = "GET";
					request.Accept = "application/json";
					HttpHelper.AddCorrelationIdHeadersToRequest(request, callState);
					AdalIdHelper.AddAsHeaders(request);
					clientMetrics.BeginClientMetricsRecord(request, callState);
					using (IHttpWebResponse response = await request.GetResponseSyncOrAsync(callState))
					{
						HttpHelper.VerifyCorrelationIdHeaderInReponse(response, callState);
						UserRealmDiscoveryResponse userRealmResponse = HttpHelper.DeserializeResponse<UserRealmDiscoveryResponse>(response);
						clientMetrics.SetLastError(null);
						return userRealmResponse;
					}
				}
				catch (WebException innerException)
				{
					AdalServiceException ex = new AdalServiceException("user_realm_discovery_failed", innerException);
					clientMetrics.SetLastError(new string[1]
					{
						ex.StatusCode.ToString()
					});
					throw ex;
				}
			}
			finally
			{
				clientMetrics.EndClientMetricsRecord("user_realm", callState);
			}
		}
	}
}
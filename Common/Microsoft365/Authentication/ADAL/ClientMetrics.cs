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
using System.Diagnostics;
using System.Globalization;

namespace Microsoft365.Authentication.ADAL
{
	internal class ClientMetrics
	{


		private static ClientMetrics pendingClientMetrics;

		private static readonly object PendingClientMetricsLock = new object();

		private Stopwatch metricsTimer;

		private string lastError;

		private Guid lastCorrelationId;

		private long lastResponseTime;

		private string lastEndpoint;

		public void BeginClientMetricsRecord(IHttpWebRequest request, CallState callState)
		{
			if (callState != null && callState.AuthorityType == AuthorityType.AAD)
			{
				AddClientMetricsHeadersToRequest(request);
				metricsTimer = Stopwatch.StartNew();
			}
		}

		public void EndClientMetricsRecord(string endpoint, CallState callState)
		{
			if (callState != null && callState.AuthorityType == AuthorityType.AAD && metricsTimer != null)
			{
				metricsTimer.Stop();
				lastResponseTime = metricsTimer.ElapsedMilliseconds;
				lastCorrelationId = callState.CorrelationId;
				lastEndpoint = endpoint;
				lock (PendingClientMetricsLock)
				{
					if (pendingClientMetrics == null)
					{
						pendingClientMetrics = this;
					}
				}
			}
		}

		public void SetLastError(string[] errorCodes)
		{
			lastError = ((errorCodes != null) ? string.Join(",", errorCodes) : null);
		}

		private static void AddClientMetricsHeadersToRequest(IHttpWebRequest request)
		{
			lock (PendingClientMetricsLock)
			{
				if (pendingClientMetrics != null && NetworkPlugin.RequestCreationHelper.RecordClientMetrics)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					if (pendingClientMetrics.lastError != null)
					{
						dictionary["x-client-last-error"] = pendingClientMetrics.lastError;
					}
					dictionary["x-client-last-request"] = pendingClientMetrics.lastCorrelationId.ToString();
					dictionary["x-client-last-response-time"] = pendingClientMetrics.lastResponseTime.ToString(CultureInfo.InvariantCulture);
					dictionary["x-client-last-endpoint"] = pendingClientMetrics.lastEndpoint;
					HttpHelper.AddHeadersToRequest(request, dictionary);
					pendingClientMetrics = null;
				}
			}
		}
	}
}
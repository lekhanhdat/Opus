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
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SharePoint.Client.Extension
{
    public static class ClientContextExtension
    {
        public static void ExecuteQueryWithIncrementalRetry(this ClientContext clientContext, int retryCount, int delay)
        {
            int retryAttempts = 0;
            int backoffInterval = delay;
            int retryAfterInterval = 0;
            bool retry = false;
            ClientRequestWrapper wrapper = null;
            if (retryCount <= 0)
                throw new ArgumentException("Provide a retry count greater than zero.");
            if (delay <= 0)
                throw new ArgumentException("Provide a delay greater than zero.");

            // Do while retry attempt is less than retry count
            while (retryAttempts < retryCount)
            {
                try
                {
                    if (!retry)
                    {
                        clientContext.ExecuteQuery();
                        return;
                    }
                    else
                    {
                        //increment the retry count
                        retryAttempts++;

                        // retry the previous request using wrapper
                        if (wrapper != null && wrapper.Value != null)
                        {
                            clientContext.RetryQuery(wrapper.Value);
                            return;
                        }
                        // retry the previous request as normal
                        else
                        {
                            clientContext.ExecuteQuery();
                            return;
                        }
                    }
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    // Check if request was throttled - http status code 429
                    // Check is request failed due to server unavailable - http status code 503
                    if (response != null && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503))
                    {
                        wrapper = (ClientRequestWrapper)ex.Data["ClientRequest"];
                        retry = true;

                        // Determine the retry after value - use the `Retry-After` header when available
                        string retryAfterHeader = response.GetResponseHeader("Retry-After");
                        if (!string.IsNullOrEmpty(retryAfterHeader))
                        {
                            if (!Int32.TryParse(retryAfterHeader, out retryAfterInterval))
                            {
                                retryAfterInterval = backoffInterval;
                            }
                        }
                        else
                        {
                            retryAfterInterval = backoffInterval;
                        }

                        // Delay for the requested seconds
                        Thread.Sleep(retryAfterInterval * 1000);

                        // Increase counters
                        backoffInterval = backoffInterval * 2;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            throw new MaximumRetryAttemptedException($"Maximum retry attempts {retryCount}, has be attempted.");
        }

        [Serializable]
        public class MaximumRetryAttemptedException : Exception
        {
            public MaximumRetryAttemptedException(string message) : base(message) { }
        }
    }
}

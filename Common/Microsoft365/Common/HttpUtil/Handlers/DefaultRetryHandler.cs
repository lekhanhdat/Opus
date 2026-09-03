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
namespace Microsoft365.Common.HttpUtil
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// this handler will follow the retry after header value, wait for specific time and then retry
    /// </summary>
    public class DefaultRetryHandler : DelegatingHandler
    {
        protected IList<IRetryStrategy> RetryStrategies =new List<IRetryStrategy>();


        public DefaultRetryHandler(IList<IRetryStrategy> retryStrategies)
        {
            RetryStrategies=retryStrategies??new List<IRetryStrategy>();
        }

        public DefaultRetryHandler(DelegatingHandler innerHandler, IList<IRetryStrategy> retryStrategies)
            : this((HttpMessageHandler)innerHandler, retryStrategies)
        {
        }

        public DefaultRetryHandler(HttpMessageHandler innerHandler, IList<IRetryStrategy> retryStrategies)
            : base(innerHandler)
        {
            RetryStrategies = retryStrategies ?? new List<IRetryStrategy>();
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryContext = new RetryContext();
            HttpResponseMessage response;
            Exception exception;
            while (true)
            {
                response = null;
                exception = null;
                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                    if ((!response.IsSuccessStatusCode) && response.Content != null)
                    {
                        await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                retryContext.SetContextInfo(response, exception);
                RetryCondition executeCondition = null;
                foreach (var strategy in RetryStrategies)
                {
                    var condition = await strategy.DetermineCondition(retryContext);
                    if (condition != null && condition.RetryAllowed)
                    {
                        executeCondition = condition;
                        break;
                    }
                }

                if (executeCondition != null)
                {
                    await Task.Delay(executeCondition.RetryAfterTime).ConfigureAwait(false);
                    executeCondition.RetryAfterCustomAction?.Invoke();
                }
                else
                {
                    //no retry strategy can take affect, should break
                    break;
                }
            }

            return response;
        }
    }
}

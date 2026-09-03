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
using AvePoint.RA.Common.RateLimitsPolicyManager;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using Microsoft.AspNetCore.Mvc.Filters;
using Polly.RateLimit;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class APIRateLimitFilter : Attribute, IAsyncResourceFilter
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(APIRateLimitFilter));
        private RateLimitsPolicyManager rateLimitsPolicyManager;
        public APIRateLimitFilter(RateLimitsPolicyManager rateLimitsPolicyManager)
        {
            this.rateLimitsPolicyManager = rateLimitsPolicyManager;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (rateLimitsPolicyManager != null)
            {
                var (reachLimit, errorMessage, retryAfter) = await IsRequestBeyondRateLimitAsync();
                if (reachLimit)
                {
                    context.Result = new ReachAPIRateLimitResult(errorMessage);
                    context.HttpContext.Request.Headers["Retry-After"] = retryAfter;
                }
                else
                {
                    await next();
                }
            }
        }

        private async Task<(bool, string, string)> IsRequestBeyondRateLimitAsync()
        {
            string errorMessage = string.Empty;
            string retryAfterStr = string.Empty;
            bool beyondLimit = false;
            var globalRatePolicy = rateLimitsPolicyManager.GlobalRateLimitsPolicy.CurrentPolicy;
            if (string.IsNullOrWhiteSpace(TenantLocalValue.LogonGroupId))
            {
                try
                {
                    await globalRatePolicy.Execute(() => Task.CompletedTask);
                }
                catch (RateLimitRejectedException ex)
                {
                    logger.Warn($"API Rate Limit Rejected:{ex.ToString()}");
                    string retryAfter = DateTimeOffset.UtcNow
                                       .Add(ex.RetryAfter)
                                       .ToUnixTimeSeconds()
                    .ToString(CultureInfo.InvariantCulture);
                    errorMessage = ex.Message;
                    retryAfterStr = retryAfter;
                    beyondLimit = true;
                }
            }
            else
            {
                var tenantRatePolicy = rateLimitsPolicyManager.GetTenantRateLimitPolicy(TenantLocalValue.LogonGroupId).CurrentPolicy;
                try
                {
                    await globalRatePolicy.Execute(async () => await tenantRatePolicy.Execute(() => Task.CompletedTask));
                }
                catch (RateLimitRejectedException ex)
                {
                    logger.Warn($"API Rate Limit Rejected:{ex.ToString()}");
                    string retryAfter = DateTimeOffset.UtcNow
                                       .Add(ex.RetryAfter)
                                       .ToUnixTimeSeconds()
                    .ToString(CultureInfo.InvariantCulture);
                    errorMessage = ex.Message;
                    retryAfterStr = retryAfter;
                    beyondLimit = true;
                }
            }
            return (beyondLimit, errorMessage, retryAfterStr);
        }


    }
}

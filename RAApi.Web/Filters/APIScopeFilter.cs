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
using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class APIScopeFilter : Attribute, IAsyncAuthorizationFilter
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(APIScopeFilter));
        public string ApiScope { get; private set; }
        public APIScopeFilter(string scope)
        {
            ApiScope = scope;
        }
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var claimsPrinciple = context.HttpContext.User;
                var apiScope = GetClaimValueByCliamTypeName(claimsPrinciple.Claims, "scope");
                if (string.IsNullOrWhiteSpace(apiScope) || !apiScope.Contains(ApiScope))
                {
                    context.Result = new AuthenticationFailureResult($"Failed to validate api scope.");
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var exceptionStr = ex.ToString();
                logger.Error("error occured  when validate api scope." + exceptionStr);
                context.Result = new AuthenticationFailureResult($"Error occured when validate api scope, exception is {ex.Message}");

                return Task.CompletedTask;
            }
        }

        private string GetClaimValueByCliamTypeName(IEnumerable<Claim> claims, string claimType)
        {
            var result = string.Empty;
            foreach (Claim claim in claims)
            {
                if (string.Equals(claim.Type, claimType))
                {
                    result = claim.Value;
                    break;
                }
            }
            return result;
        }
    }

    
}

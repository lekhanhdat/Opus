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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Web.Extentions.Util;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RMAppAuthorizeAttribute: AuthorizeAttribute, IAuthorizationFilter
    {

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            UnauthorizedState state = UnauthorizedState.None;
            var isAllowAnonymous = filterContext.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
            if (!isAllowAnonymous)
            {
                if (!IsAuthorized(filterContext.HttpContext, ref state))
                {
                    HandleUnauthorizedRequest(filterContext, state);
                }
            }
        }

        private bool IsAuthorized(HttpContext httpContext, ref UnauthorizedState state)
        {
            Uri reqUrl = null;
            try
            {
                reqUrl = httpContext.Request.GetUrl();
                // var principal = httpContext.User;

                //if (null == principal || !principal.Identity.IsAuthenticated)
                //{
                //    state = UnauthorizedState.NoLogin;
                //}
                var identity = httpContext.User.Identity;
                var isAuth = identity.IsAuthenticated;
                //RMIdentity identity = ParseAuthorizationHeader(httpContext);
                if (identity != null)
                {
                    ILoginService loginService = new LoginService();
                    //var principal = loginService.WriteClaimsPrincipal(identity);
                    //HttpContext.Current.User = principal;
                    //Thread.CurrentPrincipal = principal;
                    TenantLocalValue.LogonUserId = identity.Name;
                    TenantLocalValue.LogonGroupId = httpContext.User.Claims.FirstOrDefault(c => c.Type == RMClaimTypes.TenantGroupId)?.Value;
                }
                //System.Security.Claims.ClaimsIdentity claimsIdentity = new System.Security.Claims.ClaimsIdentity(principal.Identity);
                //if (claimsIdentity.Claims?.Count() > 0)
                //{
                //    TenantLocalValue.LogonGroupId = httpContext.Request.QueryString[SPAppConstants.ParamTenantId];
                //    TenantLocalValue.LogonUserEmail ="Related";//Related Debug from request
                //    TenantLocalValue.LogonUserId = "Related User";
                //    TenantLocalValue.DisplayName = "Related User";
                //    TenantLocalValue.AccountType = RMAccountType.RelatedEndUser;
                //}

            }
            catch
            {
                // to do : log
                throw;
            }
            if (state == UnauthorizedState.None)
            {
                return true;
            }
            return false;
        }
        /*private RMIdentity ParseAuthorizationHeader(HttpContext context)
        {
            RMIdentity rmIdentity = new RMIdentity();
            return rmIdentity;
            //rmIdentity.Claims
        }*/

        private void HandleUnauthorizedRequest(AuthorizationFilterContext context, UnauthorizedState state)
        {
            if (context == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            else
            {
                var request = context.HttpContext.Request;
                var reqUrl = request.GetUrl();
                string tmpUrl = string.Empty, currentRequestUrl = string.Empty;
                if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && null != request.GetTypedHeaders().Referer)
                {
                    tmpUrl = request.GetTypedHeaders().Referer.ToString();
                }
                else
                {
                    tmpUrl = reqUrl.ToString();
                }
                //https://test.avepointonlineservices.com/login?redirecturl={0}
                currentRequestUrl = AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.AOS_URL],"account/LogOff");
                if (state == UnauthorizedState.AccessDenied)
                {
                    currentRequestUrl += "&needLogOut=true";
                }
                context.Result = new RedirectResult(currentRequestUrl);
            }
        }


    }
}
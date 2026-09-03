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
using AvePoint.RA.Common;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal abstract class BaseAuthorizeAttribute : Attribute,IAsyncAuthorizationFilter, IActionFilter
    {
        private const string RMIdentityKey = "RMIdentity";
        private static RALogger Logger = RALogger.GetInstance(typeof(BaseAuthorizeAttribute));
        public int Order { get; set; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
        {
            var isAllowAnonymous = filterContext.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
            if (!isAllowAnonymous)
            {
               
                TenantLocalValue.Clear();
                ClientRequestLocalValue.ClientIP = null;
                RMIdentity Identity = await GetIdentityAsync(filterContext);
                if (IsAuthenticated(filterContext, Identity))
                {
                    TenantLocalValue.Init(Identity);
                    ClientRequestLocalValue.ClientIP = filterContext.HttpContext.GetClientIP();
                    if (await IsAuthorizedAsync(filterContext, Identity))
                    {
                        filterContext.HttpContext.Items[RMIdentityKey] = Identity;
                        await ExtendsSessionAsync(filterContext, Identity);
                    }
                    else if (filterContext.Result == null)
                    {
                        Logger.Warn($"user is not authenticated-1: {Identity?.TenantGroupId}, {Identity?.SessionId}");
                        await OnUnauthorizedAsync(filterContext);
                    }
                }
                else
                {
                    OnUnauthenticated(filterContext, Identity);
                }
            }
        }

        protected virtual Task<RMIdentity> GetIdentityAsync(AuthorizationFilterContext filterContext)
        {
            return filterContext.HttpContext.Request.GetRMIdentityAsync();
        }

        protected virtual bool IsAuthenticated(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            var httpContext = filterContext.HttpContext;
            try
            {
                if (null == Identity || !Identity.IsAuthenticated || (SessionManger.useSqlSessionStore && Identity.ExpiredTime < DateTime.UtcNow))
                {
                    var reqUrl = httpContext.Request.GetUrl();
                    Logger.Warn($"user is not authenticated-2: {Identity?.AccountId}, {reqUrl}");
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"authenticated error: {ex}");
            }

            return false;
        }

        protected virtual void OnUnauthenticated(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            TenantLocalValue.Clear();
            ClientRequestLocalValue.ClientIP = null;
            var request = filterContext.HttpContext.Request;
            var reqPath = request.Path.ToString();
            if (reqPath.StartWithIgnoreCase("/api/"))
            {
                var content = "Unauthenticated";
                filterContext.Result = new ObjectResult(content) { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            else
            {
                var redirectUrl = string.Empty;
                if (IsCheckSessionRequest(reqPath))
                {
                    if (Identity != null && Identity.ForcedLogout)
                    {
                        filterContext.Result = new ObjectResult((int)CheckSessionResult.ForcedLogout);
                    }
                    else
                    {
                        filterContext.Result = new ObjectResult((int)CheckSessionResult.SessionTimeout);
                    }
                }
                else
                {
                    redirectUrl = RMSSOHelper.RecoLogoutUrl;
                    filterContext.Result = new RedirectResult(redirectUrl);
                }
            }
        }

        protected virtual Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            return Task.FromResult(true);
        }

        protected virtual Task OnUnauthorizedAsync(AuthorizationFilterContext filterContext)
        {
            TenantLocalValue.Clear();
            ClientRequestLocalValue.ClientIP = null;
            var request = filterContext.HttpContext.Request;
            var reqPath = request.Path.ToString();
            if(reqPath.StartWithIgnoreCase("/api/"))
            {
                var content = "Unauthorized";
                filterContext.Result = new ObjectResult(content) { StatusCode = (int)HttpStatusCode.Unauthorized };
            }
            else
            {
                filterContext.Result = new RedirectResult("/ErrorPage/NoPermission");
            }

            return Task.CompletedTask; 
        }


        private bool IsCheckSessionRequest(string path)
        {
            return path.EndsWith("/Account/CheckSession", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExtendsSessionAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            string path = filterContext.HttpContext.Request.Path;
            if (!path.StartWithIgnoreCase("/Account/LogOut") && !IsCheckSessionRequest(path))
            {
                await filterContext.HttpContext.Response.RenewRMIdentityAsync(Identity);
            }
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext context)
        {
            var identity = context.HttpContext.Items[RMIdentityKey] as RMIdentity;
            if(identity != null)
            {
                TenantLocalValue.Init(identity);
                ClientRequestLocalValue.ClientIP = context.HttpContext.GetClientIP();
                SessionManger.CurrentSessionId = identity.SessionId;
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext context)
        {
            //throw new NotImplementedException();
        }
    }


}
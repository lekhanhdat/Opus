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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RMAuthorizeFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private bool Active = true;
        public RMAuthorizeFilterAttribute()
        {
        }
        bool requireAuthentication = true;
        public bool RequireAuthentication
        {
            get { return requireAuthentication; }
            set { requireAuthentication = value; }
        }
        /// <summary>
        /// Mark if disable the filter
        /// </summary>
        /// <param name="active"></param>
        public RMAuthorizeFilterAttribute(bool active)
        {
            this.Active = active;
        }
        /// <summary>
        /// Override to Web API filter method to handle Basic Auth check
        /// </summary>
        /// <param name="actionContext"></param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext actionContext)
        {
            var isAllowAnonymous = actionContext.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
            if (isAllowAnonymous)
            {
                return;
            }
            if (Active)
            {
                if (!requireAuthentication)
                {
                    return;
                }
                RMIdentity identity = ParseAuthorizationHeader(actionContext);
                if (identity != null)
                {
                    ILoginService loginService = new LoginService();
                    var principal = await loginService.ConvertClaimsPrincipalAsync(identity);
                    actionContext.HttpContext.User = principal;
                    Thread.CurrentPrincipal = principal;
                }
                else
                {
                    Challenge(actionContext);
                    return;
                }
            }
        }
        /// <summary>
        ///验证当前用户是否有执行当前方法的权限
        /// </summary>
        /// <param name="username"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool OnAuthorizeUser(string username, string password, AuthorizationFilterContext actionContext)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;

            return true;
        }

        /// <summary>
        /// 解析Header中的Token 获取真正的user
        /// </summary>
        /// <param name="actionContext"></param>
        protected virtual RMIdentity ParseAuthorizationHeader(AuthorizationFilterContext actionContext)
        {
            string authHeader = actionContext.HttpContext.Request.Headers.Authorization;
            if (authHeader != null && authHeader.StartWithIgnoreCase("Token "))
            {
                authHeader = authHeader.Split(' ')[1];
            }
            if (string.IsNullOrEmpty(authHeader))
            {
                return null;
            }
            string unwrapped = Encoding.UTF8.GetString(CspCrossPlatformExchangeWrapper.UnWrapKey(authHeader));
            var tokens = unwrapped.Split('$');
            if (tokens.Length < 2)
            {
                return null;
            }
            var user = tokens[0];
            var groupId = tokens[1];
            var userId = tokens[2];
            var tokenBeginTimeTicks = tokens[3];
            DateTime tokenBeginTime = new DateTime(long.Parse(tokenBeginTimeTicks));
            if (tokenBeginTime < DateTime.UtcNow.AddHours(-1))
            {
                TenantLocalValue.LogonUserEmail = null;
                TenantLocalValue.LogonGroupId = null;
                return null;
            }
            AosAuthentication aosAuthentication = new AosAuthentication();
            var credential = new AOSCredential()
            {
                UserId = userId,
                UserName = user,
                TenantGroupId = groupId,
            };
            RMIdentity identity = aosAuthentication.AuthenticateCredential(credential);
            TenantLocalValue.LogonUserEmail = identity.Name;
            TenantLocalValue.LogonGroupId = identity.TenantGroupId;
            return identity;
        }


        /// <summary>
        /// 验证失败, 返回401
        /// </summary>
        /// <param name="message"></param>
        /// <param name="actionContext"></param>
        void Challenge(AuthorizationFilterContext actionContext)
        {
            var host = actionContext.HttpContext.Request.GetUrl().DnsSafeHost;
            actionContext.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            actionContext.HttpContext.Response.Headers.Add("www-Authenticate", string.Format("Basic realm=\"{0}\"", host));
        }

    }
    public class BasicAuthenticationIdentity : GenericIdentity
    {
        public BasicAuthenticationIdentity(string name, string password)
            : base(name, "Basic")
        {
            this.Password = password;
        }

        public string Password { get; set; }
    }
}
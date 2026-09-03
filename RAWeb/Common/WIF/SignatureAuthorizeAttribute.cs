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
using AvePoint.RA.APIContract;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    //public class SignatureAuthorizeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    //{
    //    private RALogger mLogger = RALogger.GetInstance(typeof(SignatureAuthorizeAttribute));
    //    protected ISecurityService securityService = new SecurityService();
    //    protected ITenantService Tenantservice => PlatformWindsorManager.GetService<ITenantService>();
    //    public async System.Threading.Tasks.Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
    //    {
    //        var isAllowAnonymous = filterContext.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
    //        if (!isAllowAnonymous)
    //        {
    //            await IsAuthorizedAsync(filterContext);
    //        }
    //    }
    //    private async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext actionContext)
    //    {
    //        if (actionContext.ActionDescriptor.EndpointMetadata.Any(a => a is SignatureAuthorizeAttribute))
    //        {
    //            var portalAccessToken = default(String);
    //            if (actionContext.HttpContext.Request.Headers.ContainsKey("X_Records_Access_Token"))
    //            {
    //                var token = actionContext.HttpContext.Request.Headers["X_Records_Access_Token"].First();
    //                if (!string.IsNullOrEmpty(token))
    //                {
    //                    portalAccessToken = token;
    //                }
    //            }

    //            if (!string.IsNullOrEmpty((portalAccessToken)))
    //            {
    //                AccessTokenModel accessTokenModel = null;
    //                try
    //                {
    //                    accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(portalAccessToken);
    //                }
    //                catch (Exception)
    //                {
    //                    HandleUnauthorizedRequest(actionContext, "Access token is invalid.");
    //                    mLogger.Warn("convert access token is invalid.");
    //                    return false;
    //                }
                   
    //                if (accessTokenModel.TenantGroupId == null || accessTokenModel.Email == null || accessTokenModel.ExpiredTime == null)
    //                {
    //                    HandleUnauthorizedRequest(actionContext, "Access token signature is invalid.");
    //                    mLogger.Warn("current token properties is invalid.");
    //                    return false;
    //                }
    //                var accessToken = ModeConvertUtil.ToAccessToken(accessTokenModel);
    //                if (securityService.ValidateToken(accessToken))
    //                {
    //                    var accessTokenKeyExpiredTime = Convert.ToInt64(accessToken.ExpiredTime);
    //                    if (accessTokenKeyExpiredTime < DateTime.UtcNow.Ticks)
    //                    {
    //                        TenantLocalValue.LogonUserEmail = null;
    //                        TenantLocalValue.LogonGroupId = null;
    //                        HandleUnauthorizedRequest(actionContext, "Access token signature is expired.");
    //                        mLogger.Warn("current token is expired:{0}.", accessTokenModel?.TenantGroupId);
    //                        return false;
    //                    }
                        
    //                    string ownerEmail = string.Empty;
    //                    if (Tenantservice.CheckTenantIsAvailable(accessToken.TenantGroupId))
    //                    {
    //                        RMIdentity identity = GetIdentity(accessToken);
    //                        if (identity != null)
    //                        {
    //                            ILoginService loginService = new LoginService();
    //                            var principal = await loginService.ConvertClaimsPrincipalAsync(identity);
    //                            actionContext.HttpContext.User = principal;
    //                            Thread.CurrentPrincipal = principal;
    //                            return true;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        HandleUnauthorizedRequest(actionContext, "There is no user info.");
    //                        mLogger.Warn("current account is invalid:{0}.", accessTokenModel?.Email);
    //                        return false;
    //                    }

    //                }
    //            }

    //        }
    //        HandleUnauthorizedRequest(actionContext, "Access token signature is invalid.");
    //        mLogger.Warn("token signature is invalid.");
    //        return false;
            

    //    }

    //    private RMIdentity GetIdentity(Contract.Security.RMAccessToken token)
    //    {
    //        AosAuthentication aosAuthentication = new AosAuthentication();
    //        var credential = new AOSCredential()
    //        {
    //            UserName = token.Email,
    //            TenantGroupId = token.TenantGroupId,
    //            AccountType = RMAccountType.StandardUser
    //        };
    //        RMIdentity identity = aosAuthentication.AuthenticateCredential(credential);
    //        TenantLocalValue.LogonUserEmail = identity.Name;
    //        TenantLocalValue.LogonGroupId = identity.TenantGroupId;
    //        TenantLocalValue.AccountType = identity.AccountType;
    //        return identity;
    //    }

    //    private void HandleUnauthorizedRequest(AuthorizationFilterContext actionContext, string message)
    //    {
    //        var content = GetAuthorizedErrorMessage(message);
    //        actionContext.Result = new ObjectResult(content) { StatusCode = (int)HttpStatusCode.Unauthorized };
    //    }

    //    private string GetAuthorizedErrorMessage(string message)
    //    {
    //        JObject errorMessage = new JObject
    //        {
    //            new JProperty("ErrorCode", (int)RestStateCode.LoginAutherized),
    //            new JProperty("ErrorMessage", string.Format("Login authorized failed: {0}", message)),
    //        };
    //        return JsonConvert.SerializeObject(errorMessage);
    //    }
    //}

}


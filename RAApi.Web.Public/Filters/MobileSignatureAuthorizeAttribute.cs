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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.WIF
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    //public class MobileSignatureAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    //{
    //    private RALogger logger = RALogger.GetInstance(typeof(MobileSignatureAuthorizeAttribute));
    //    static readonly ISecurityService securityService = new SecurityService();
    //    static readonly IUserService userService = new UserService();
    //    private ITenantService mTenantservice = null;
        

    //    protected ITenantService Tenantservice
    //    {
    //        get
    //        {
    //            if (mTenantservice == null)
    //            {
    //                mTenantservice = new TenantService();
    //            }
    //            return mTenantservice;
    //        }
    //    }
    //    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    //    {
    //        var request = context.HttpContext.Request;
    //        var isAllowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
    //        if (isAllowAnonymous)
    //        {
    //            logger.Info($"Anonymous request: {request?.GetUrl()?.LocalPath}");
    //        }
    //        else
    //        {
    //            await IsAuthorizedAsync(context);
    //        }
    //    }
    //    private async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext context)
    //    {
    //        var request = context.HttpContext.Request;
    //        ClientRequestLocalValue.ClientIP = context.HttpContext.GetClientIP();
    //        if (context.ActionDescriptor.EndpointMetadata.Any(a => a is MobileSignatureAuthorizeAttribute))
    //        {
    //            var portalAccessToken = default(String);
    //            if (request.Headers.ContainsKey("X_Records_Access_Token"))
    //            {
    //                var token = request.Headers["X_Records_Access_Token"].First();
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
    //                    //portalAccessToken = HttpUtility.UrlDecode(portalAccessToken);
    //                    accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(portalAccessToken);
    //                    logger.Info($"Finished analyse the token. expire time : {accessTokenModel.ExpiredTime}");
    //                }
    //                catch (Exception)
    //                {
    //                    HandleUnauthorizedRequest(context, HttpStatusCode.Unauthorized, "Access token is invalid.");
    //                    logger.Warn("convert access token is invalid.");
    //                    return false;
    //                }

    //                if (accessTokenModel.TenantGroupId == null || accessTokenModel.Email == null || accessTokenModel.ExpiredTime == null)
    //                {
    //                    HandleUnauthorizedRequest(context, HttpStatusCode.Unauthorized, "Access token signature is invalid.");
    //                    logger.Warn("current token properties is invalid.");
    //                    return false;
    //                }
    //                var accessToken = ConvertUtil.ToAccessToken(accessTokenModel);
    //                if (securityService.ValidateToken(accessToken))
    //                {
    //                    var accessTokenKeyExpiredTime = Convert.ToInt64(accessToken.ExpiredTime);
    //                    if (accessTokenKeyExpiredTime < DateTime.UtcNow.Ticks)
    //                    {
    //                        TenantLocalValue.LogonUserEmail = null;
    //                        TenantLocalValue.LogonGroupId = null;
    //                        HandleUnauthorizedRequest(context, HttpStatusCode.Unauthorized, "Access token signature is expired.");
    //                        logger.Warn("current token is expired:{0}.", accessTokenModel?.TenantGroupId);
    //                        return false;
    //                    }

    //                    string ownerEmail = string.Empty;
    //                    if (Tenantservice.CheckTenantIsAvailable(accessToken.TenantGroupId))
    //                    {
    //                        RMIdentity identity = await GetIdentityAsync(accessToken);
    //                        if (identity != null)
    //                        {
    //                            ILoginService loginService = new LoginService();
    //                            var principal = await loginService.ConvertClaimsPrincipalAsync(identity);
    //                            context.HttpContext.User = principal;
    //                            Thread.CurrentPrincipal = principal;
    //                            return true;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        HandleUnauthorizedRequest(context, HttpStatusCode.Unauthorized, "There is no user info.");
    //                        logger.Warn("current account is invalid:{0}.", accessTokenModel?.Email);
    //                        return false;
    //                    }

    //                }
    //            }

    //        }
    //        HandleUnauthorizedRequest(context, HttpStatusCode.Unauthorized, "Access token signature is invalid.");
    //        logger.Warn("token signature is invalid.");
    //        return false;
    //    }

    //    private void HandleUnauthorizedRequest(AuthorizationFilterContext filterContext, HttpStatusCode httpStatus, string message)
    //    {
    //        if (filterContext == null)
    //        {
    //            throw new ArgumentNullException("filterContext");
    //        }
    //        else
    //        {
    //            //HttpStatusCode = 800 是与Mobile 端商定好的，不用401 是因为401 会主动跳转AOS
    //            filterContext.Result = new ObjectResult(GetAuthorizedErrorMessage(httpStatus, message)) { StatusCode = 800 };
    //        }
    //    }

    //    private async Task<RMIdentity> GetIdentityAsync(Contract.Security.RMAccessToken token)
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
    //        try
    //        {
    //            TenantLocalValue.LogonUserId = (await userService.GetUserByNameAsync(TenantLocalValue.LogonUserEmail)).UserId;
    //        }
    //        catch(Exception e)
    //        {
    //            logger.Info($"init user falied {e}");
    //        }
    //        return identity;
    //    }

    //    private JObject GetAuthorizedErrorMessage(HttpStatusCode httpStatus, string message)
    //    {
    //        return new JObject
    //        {
    //            new JProperty("ErrorCode", (int)httpStatus),
    //            new JProperty("ErrorMessage", string.Format($"Login authorized failed: {message}")),
    //        };
    //    }
    //}

}


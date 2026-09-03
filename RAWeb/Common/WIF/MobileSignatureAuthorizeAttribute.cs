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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using AvePoint.RA.Api.Contract;
using static System.Net.WebRequestMethods;
using Castle.Core.Resource;
using Microsoft.Exchange.WebServices.Data;
using AccessTokenModel = AvePoint.RA.APIContract.AccessTokenModel;
using log4net.Filter;
using AvePoint.RA.Common.ClientRequest;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class MobileSignatureAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(MobileSignatureAuthorizeAttribute));
        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        public System.Threading.Tasks.Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
        {
            var isAllowAnonymous = filterContext.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
            if (!isAllowAnonymous)
            {
                if (!IsAuthorized(filterContext, out var message))
                {
                    HandleUnauthorizedRequest(filterContext, message);
                }
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
        private bool IsAuthorized(AuthorizationFilterContext actionContext, out string message)
        {
            message = null;
            if (actionContext.ActionDescriptor.EndpointMetadata.Any(a => a is MobileSignatureAuthorizeAttribute))
            {
                ClearIdentity();
                var tokenFromHeader = string.Empty;
                if (actionContext.HttpContext.Request.Headers.ContainsHeader("X_Records_Access_Token"))
                {
                    tokenFromHeader = actionContext.HttpContext.Request.Headers.GetFirstHeaderValue("X_Records_Access_Token");
                    if (!string.IsNullOrEmpty((tokenFromHeader)))
                    {
                        AccessTokenModel accessTokenModel = JsonConvert.DeserializeObject<AccessTokenModel>(tokenFromHeader);
                        mLogger.Info($"step1 to validate mobile signarture.");
                        var accessTokenKeyExpiredTime = Convert.ToInt64(accessTokenModel.ExpiredTime);
                        if (accessTokenKeyExpiredTime < DateTime.UtcNow.Ticks)
                        {
                            TenantLocalValue.LogonUserEmail = null;
                            TenantLocalValue.PartnerUser = null;
                            TenantLocalValue.LogonGroupId = null;
                            ClientRequestLocalValue.ClientIP = null;
                            message = "Access token signature is expired.";
                            mLogger.Warn("current token is expired:{0}.", accessTokenModel?.TenantGroupId);
                            return false;
                        }
                        var tokenObj = ModeConvertUtil.ToAccessToken(accessTokenModel);
                        mLogger.Info($"step1 to validate mobile signarture.");
                        var identity = ValidateToken(tokenObj.AccessToken);
                        if (identity != null)
                        {
                            SetIdentity(identity);
                            ClientRequestLocalValue.ClientIP = actionContext.HttpContext.GetClientIP();
                            mLogger.Info($"mobile token validate success.");
                            return true;
                        }
                        else
                        {
                            mLogger.Warn($"mobile identity is invalid:{tokenObj?.TenantGroupId}, {tokenObj?.Email}.");
                            return false;
                        }
                    }
                }
                message = "Access token signature is null.";
                mLogger.Error($"access token is null.");
                return false;
            }
            message = "Access token signature is invalid.";
            mLogger.Warn("token signature is invalid.");
            return false;
        }

        private void HandleUnauthorizedRequest(AuthorizationFilterContext actionContext, string message)
        {
            ClearIdentity();
            var content = GetAuthorizedErrorMessage((int)HttpStatusCode.Unauthorized, message);
            actionContext.Result = new ObjectResult(content) { StatusCode = 800 };
        }
        private ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                mLogger.Info("Validate identity access token.");

                var identityServiceUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_SERVICE_URL];
                var clientId = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_CLIENT_ID];
                SecurityToken securityToken = null;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                if (!cache.TryGetValue<OpenIdConnectConfiguration>(identityServiceUrl, out OpenIdConnectConfiguration openIdConfig))
                {
                    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{identityServiceUrl}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                    openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
                    cache.Set(identityServiceUrl, openIdConfig, TimeSpan.FromMinutes(20));
                }

                mLogger.Info("IdentityServerResource is {0}, IdentityServerIssuers is {1}", clientId, identityServiceUrl);
                var result = handler.ValidateToken(token, new TokenValidationParameters()
                {
                    AuthenticationType = "IdentityServer",
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidIssuers = identityServiceUrl.Split(";"),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                }, out securityToken);

                mLogger.Info($"Validate identity access token success, {TenantLocalValue.LogonGroupId}");
                return result;

            }
            catch (Exception ex)
            {
                mLogger.Error("Validate access token failed. Exception: {0}", ex.ToString());
                throw;
            }
        }

        private void SetIdentity(ClaimsPrincipal identity)
        {
            var userName = identity.Claims.FirstOrDefault(u => u.Type.Equals(ClaimTypes.Upn))?.Value;
            var customerId = identity.Claims.FirstOrDefault(u => u.Type.Equals("customer_id"))?.Value;
            var userId = identity.Claims.FirstOrDefault(u => u.Type.Equals("uid"))?.Value;
            TenantLocalValue.LogonUserEmail = userName;
            TenantLocalValue.LogonGroupId = customerId;
            TenantLocalValue.AccountType = RMAccountType.ApplicationAdmin;
            TenantLocalValue.LogonUserId = userId;
        }

        private void ClearIdentity()
        {
            TenantLocalValue.LogonUserEmail = "";
            TenantLocalValue.LogonGroupId = "";
            TenantLocalValue.AccountType = RMAccountType.None;
            TenantLocalValue.LogonUserId = "";
        }

        private string GetAuthorizedErrorMessage(int httpStatus, string message)
        {
            JObject errorMessage = new JObject
            {
                new JProperty("ErrorCode", httpStatus),
                new JProperty("ErrorMessage", string.Format("Login authorized failed: {0}", message)),
            };
            return JsonConvert.SerializeObject(errorMessage);
        }
    }

}


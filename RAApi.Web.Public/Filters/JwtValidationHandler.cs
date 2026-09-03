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

using Aspose.Pdf.Operators;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Contract.Services;
using AvePoint.RA.Api.Web.Public.Authorize;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker.Common;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecurityToken = Microsoft.IdentityModel.Tokens.SecurityToken;
using SecurityTokenValidationException = Microsoft.IdentityModel.Tokens.SecurityTokenValidationException;

namespace AvePoint.RA.Web.Common.WIF
{
    /// <summary>
    /// 验证新型sdk中传递过来的jwttoken 并将其中的tenantGroupId取出来用来放进Thread中方便执行数据库相关操作
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtValidationHandler : Attribute, IAsyncAuthorizationFilter
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(JwtValidationHandler));
        public ITenantInfoService TenantInfoService
        {
            get { return PlatformWindsorManager.GetService(typeof(ITenantInfoService)) as ITenantInfoService; }
        }
        public IAgentMgmtService AgentMgmtService
        {
            get { return PlatformWindsorManager.GetService(typeof(IAgentMgmtService)) as IAgentMgmtService; }
        }
        public ITenantService TenantService
        {
            get { return PlatformWindsorManager.GetService(typeof(ITenantService)) as ITenantService; }
        }

        public IAccountDao AccountDao
        {
            get { return PlatformWindsorManager.GetService(typeof(IAccountDao)) as IAccountDao; }
        }
        public IRMSecurityGroupDao RMSecurityGroupDao { get { return (IRMSecurityGroupDao)PlatformWindsorManager.GetService(typeof(IRMSecurityGroupDao)); } }

        public IAccountWrapperService AccountWrapperService
        {
            get { return PlatformWindsorManager.GetService(typeof(IAccountWrapperService)) as IAccountWrapperService; }
        }

        public IUserService UserService
        {
            get { return PlatformWindsorManager.GetService(typeof(IUserService)) as IUserService; }
        }

        public bool AllowMultiple => true;

        private static MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        private static readonly List<string> WhiteList =
        [
            "/api/manualApproval/UnderReviewQuery",
            "/api/manualApproval/Approve",
            "/api/manualApproval/Reject",
            "/api/manualApproval/RunFolderViewActionJob",
            "/api/manualApproval/GetApprovalCommentOption",
            "/api/manualApproval/RunBulkActionJob",
            "/api/manualApproval/GetFilterDefaultOptions",
            "/api/manualApproval/QueryWorkspaces",
            "/api/manualApproval/QueryFolderPath",
            "/api/manualApproval/SpecialReviewerResult",
            "/api/manualApproval/SearchAADUsers",
            "/api/manualApproval/GetTaskDueDate",
            "/api/manualApproval/GetSettingInfo",
            "/api/termApi/GetFSClassificationLevel",
            "/api/termApi/GetAllTerms",
            "/api/termApi/GetAllLabels",
            "/api/termApi/ChangeTerm",
            "/api/termApi/ChangeLabel",
            "/api/termApi/GetTermWithPath",
            "/api/manualApproval/DoAction",
            "/api/manualApproval/GetRealTimeJobStatusInfo",
            "/api/manualApproval/IsHideReclassifyBtnInManualApproval",
            "/api/manualApproval/IsNewLogicalAccount",
            "/api/MyHub/QueryDrives",
            "/api/MyHub/QueryTreeFolders",
            "/api/MyHub/QueryFolderAndItems",
            "/api/MyHub/ReadAllClassCodeName",
            "/api/MyHub/ReadClassCodeNameByPartitionKeyIds",
            "/api/MyHub/ReadClassifyDataByPartitionKeyIds",
            "/api/MyHub/ReadAllCountryCodeName",
            "/api/MyHub/QueryDetailTable",
            "/api/MyHub/QueryDrivesVolume",
            "/api/MyHub/ClassifyUpdate",
            "/api/MyHub/ReadCountryCodeByClassCode",
            "/api/MyHub/GetRetentionType",
            "/api/MyHub/QueryClassifyInfo",
            "/api/MyHub/QueryAuditTrial",
            "/api/MyHub/GetConnectionPermission",
            "/api/MyHub/SearchAvaliableOwners",
            "/api/MyHub/UpdateConnectionRecordOwners",
            "/api/MyHub/RunFSDashboardDataSyncJob",
            "/api/MyHub/GetFSDashboardData",
			"/api/MyHub/GetAllConnectionPermission",
            "/api/MyHub/GetNodeIdByConnectionId",
            "/api/MyHub/GetPendingDisposalVolume",
            "/api/MyHub/GetPendingDisposalVolumeDisc",
			"/api/MyHub/GetPendingDisposalFolderFilter",
            "/api/MyHub/GetParameterBeforeUnderReviewQuery",
            "/api/MyHub/LoadRCCInfosById",
            "/api/MyHub/DeleteReportContent",
            "/api/MyHub/GenerateRCCReport",
            "/api/MyHub/DownloadReportContentMyhub",
            "/api/MyHub/LoadDisposalReportData",
            "/api/MyHub/GenerateDisposalHistoryReport",
            "/api/MyHub/PauseOrResume",
            "/api/MyHub/QueryAuditTrialFilters",
            "/api/MyHub/GetFolderStatistics",
            "/api/MyHub/CheckJobExists"
        ];

        private const string ApplicationName = "AvePointRecords";
        private const string AllProducts = "All Product";

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            try
            {
                var allowAnonymous = context.ActionDescriptor.EndpointMetadata.Any(a => a is AllowAnonymousAttribute);
                if (allowAnonymous)
                {
                    //logger.Info($"Anonymous request: {request?.GetUrl()?.LocalPath}");
                    return Task.CompletedTask;
                }
                string token = GetAuthorizationHeader(request);
                if (string.IsNullOrEmpty(token))
                {
                    context.Result = new AuthenticationFailureResult($"{I18NEntity.GetString("RM_VT_TokenNull")}");
                    return Task.CompletedTask;
                }
                ProductType product = ProductType.None;
                var isInternalIdentityServer = UseInternalIdentityServer(request, ref product);
                var tokenResult = ValidateIdentityServerToken(token, context, isInternalIdentityServer);
                context.HttpContext.User = tokenResult;
                GenerateBasicInfoByProduct(product, tokenResult, request);
            }
            catch (SecurityTokenValidationException e)
            {
                var exceptionStr = e.ToString();
                logger.Error("SecurityTokenValidationException:error occured  when validate jwt token." + exceptionStr);
                context.Result = new AuthenticationFailureResult($"{string.Format(I18NEntity.GetString("RM_VT_SecurityTokenEror"), "")}");
            }
            catch (Exception ex)
            {
                var exceptionStr = ex.ToString();
                logger.Error("error occured  when validate jwt token." + exceptionStr);
                context.Result = new AuthenticationFailureResult($"{string.Format(I18NEntity.GetString("RM_VT_TokenEror"), "")}");
            }
            return Task.CompletedTask;
        }

        private string GetAuthorizationHeader(HttpRequest request)
        {
            string token = request.GetRequestHeadersParam("Authorization");
            if (string.IsNullOrEmpty(token))
            {
                return token;
            }
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring("Bearer ".Length).Trim();
            }
            return token;
        }



        private void ValidateAgentAuthCode(RMAgentDto agentInfo)
        {
            try
            {
                Guid agentId = agentInfo.Id;
                string agentAuthCode = agentInfo.AuthCode;

                if (string.IsNullOrEmpty(agentAuthCode) || agentId == null)
                {
                    throw new Exception($"{I18NEntity.GetString("RM_VT_CodeEmpty")}");
                }

                logger.Warn("Begin to verify agent id and code, tenant id: " + agentInfo.TenantId + ", agent id : " + agentId.ToString());

                var cacheKey = string.Format("{0}_{1}", agentInfo.TenantId, agentInfo.Id.ToString());
                if (!cache.TryGetValue<string>(cacheKey, out string cachedCode))
                {
                    RMAgentDto agent = AgentMgmtService.Get(agentId, true);
                    cachedCode = agent.AuthCode;
                    if (agentAuthCode.Equals(cachedCode))
                    {
                        cache.Set(cacheKey, cachedCode, TimeSpan.FromHours(6));
                    }
                }
                if (agentAuthCode.Equals(cachedCode))
                {
                    //as installation code will not be changed frequently, keep timeout for 6 hours.
                    logger.Info("Validate auth code successfully.");
                    return;
                }
                throw new Exception($"{I18NEntity.GetString("RM_VT_AgentAndAuthFail")}");
            }
            catch (Exception e)
            {
                logger.Error("Verify agent id and auth code error :", e);
                throw e;
            }

        }

        private ClaimsPrincipal ValidateIdentityServerToken(string token, AuthorizationFilterContext context, bool isInternalIdentityServer)
        {
            try
            {
                logger.Info("Validate identity server token.");
                var request = context.HttpContext.Request;
                SecurityToken securityToken = null;
                var handler = new JwtSecurityTokenHandler();
                var isMyhubRequest = GetIsMyhub(token);
                if (isMyhubRequest)
                {
                    var requestPath = request.Path;
                    var isWhitelisted = WhiteList.Any(w =>
                    {
                        var wPath = new PathString(w);
                        if (string.Equals(requestPath.Value, w, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                        if (requestPath.StartsWithSegments(wPath, out var remainder))
                        {
                            return !remainder.HasValue || remainder.Value == "/";
                        }
                        return false;
                    });
                    if (!isWhitelisted)
                    {
                        throw new Exception("Current request is invalid");
                    }
                    var publicKey = RMAosApiClient.GetPortalPublicKey();
                    var cloudIdentityValidAudience = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_PRODUCT_AUDIENCE_URL];
                    var validIssuer = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_SERVICE_URL];
                    logger.Info("IdentityServerResource is {0} ", cloudIdentityValidAudience);
                    RSACryptoServiceProvider rsa = new(2048);
                    rsa.FromXmlString(publicKey);
                    var rsaKey = new RsaSecurityKey(rsa);

                    var paremeterMyhub = new TokenValidationParameters
                    {
                        AuthenticationType = "IdentityServer",
                        ValidateAudience = true,
                        ValidAudience = cloudIdentityValidAudience,
                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaKey
                    };
                    return handler.ValidateToken(token, paremeterMyhub, out securityToken);
                }

                if (request.GetRequestHeadersParam(RequestHeadersParam.TOKEN_SOURCE) == TokenSource.SpfxOAuth.ToString())
                {
                    var tenantId = GetTenantId(token);
                    if (!cache.TryGetValue(tenantId, out OpenIdConnectConfiguration appOpenIdConfig))
                    {
                        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://login.microsoftonline.com/{tenantId}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                        appOpenIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
                        cache.Set(tenantId, appOpenIdConfig, TimeSpan.FromMinutes(20));
                    }
                    var tokenClaims = handler.ValidateToken(token, new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_LOGIN_APP_ID],//"c4763714-72c1-4746-a68e-a17bcf7ad292",
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = appOpenIdConfig.SigningKeys
                    }, out securityToken);
                    return tokenClaims;
                }

                if (request.Headers.ContainsHeader(RequestHeadersParam.AOS_VNEXT))
                {
                    var cloudIdentityValidAudience = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_PRODUCT_AUDIENCE_URL];
                    var validIssuer = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_SERVICE_URL];
                    logger.Info("IdentityServerResource is {0} ", cloudIdentityValidAudience);
                    if (!cache.TryGetValue<OpenIdConnectConfiguration>(validIssuer, out OpenIdConnectConfiguration aosVNextOpenIdConfig))
                    {
                        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{validIssuer}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                        aosVNextOpenIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
                        cache.Set(validIssuer, aosVNextOpenIdConfig, TimeSpan.FromMinutes(20));
                    }

                    var paremeterVNext = new TokenValidationParameters
                    {
                        AuthenticationType = "IdentityServer",
                        ValidateAudience = true,
                        ValidAudience = cloudIdentityValidAudience,
                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = aosVNextOpenIdConfig.SigningKeys
                    };

                    if (aosVNextOpenIdConfig.SigningKeys.Count == 0)
                    {
                        paremeterVNext.ValidateIssuerSigningKey = false;
                        paremeterVNext.SignatureValidator = (string token, TokenValidationParameters parameters) =>
                        {
                            var jwt = new JwtSecurityToken(token);
                            return jwt;
                        };
                    }

                    return handler.ValidateToken(token, paremeterVNext, out securityToken);
                }

                var identityServiceUrl = isInternalIdentityServer ? RMGlobalConfiguration.AppConfig[RMAppSettingKey.IDENTITY_SERVICE_URL] : RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_IDENTITY_SERVICE_URL];
                var identityValidAudience = isInternalIdentityServer ? RMGlobalConfiguration.AppConfig[RMAppSettingKey.AUDIENCE_URL] : RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_AUDIENCE_URL];
                var validIssuers = RMGlobalConfiguration.AppConfig[RMAppSettingKey.VALID_ISSUER_URLS];
                if (!cache.TryGetValue<OpenIdConnectConfiguration>(identityServiceUrl, out OpenIdConnectConfiguration openIdConfig))
                {
                    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{identityServiceUrl}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                    openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
                    cache.Set(identityServiceUrl, openIdConfig, TimeSpan.FromMinutes(20));
                }
                logger.Info("IdentityServerResource is {0}, IdentityServerIssuers is {1}", identityValidAudience, identityServiceUrl);
                var paremeter = new TokenValidationParameters()
                {
                    AuthenticationType = "IdentityServer",
                    ValidateAudience = true,
                    ValidAudience = identityValidAudience,
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers.Split(';'),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                if (openIdConfig.SigningKeys.Count == 0)
                {
                    paremeter.ValidateIssuerSigningKey = false;
                    paremeter.SignatureValidator = (string token, TokenValidationParameters parameters) =>
                    {
                        var jwt = new JwtSecurityToken(token);
                        return jwt;
                    };
                }

                var result = handler.ValidateToken(token, paremeter, out securityToken);

                logger.Info("Validate identity server token success!");
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Validate identity server token failed. Exception: {0}", ex.ToString());
                throw;
            }
        }

        private string GetTenantId(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var tenantId = jwtToken.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
            return tenantId;
        }

        private bool GetIsMyhub(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var myhubValue = jwtToken.Claims.FirstOrDefault(c => c.Type == "exchange_source")?.Value;
            return !string.IsNullOrEmpty(myhubValue) && myhubValue == "MyHub";
        }

        private void GenerateBasicInfoByProduct(ProductType product, ClaimsPrincipal claims, HttpRequest httpRequestMessage)
        {
            if (product != ProductType.RecordsAgent)
            {
                var agentId = GetClaimValueByCliamTypeName(claims.Claims, "agent_id");
                if (!string.IsNullOrEmpty(agentId))
                {
                    product = ProductType.RecordsAgent;
                }
            }
            switch (product)
            {
                case ProductType.Records:
                case ProductType.OC:
                    GenerateBasicInfoForRecords(claims, httpRequestMessage);
                    break;
                case ProductType.RecordsAgent:
                    var tenantGroupId = GenerateBasicInfoForRecordsAgent(claims, httpRequestMessage);
                    ValidAgentInfo(tenantGroupId, claims);
                    break;
                case ProductType.COP:
                    GenerateBasicInfoForCOP(claims, httpRequestMessage);
                    break;
                case ProductType.Myhub:
                    GenerateBasicInfoForMyhub(claims, httpRequestMessage);
                    break;
                case ProductType.RecordsSpfx:
                    GenerateBasicInfoForSpfx(claims, httpRequestMessage);
                    break;
                case ProductType.AOSVNext:
                    GenerateBasicInfoForAosNext(claims, httpRequestMessage);
                    break;
                default:
                    GenerateBasicInfoForRecords(claims, httpRequestMessage);
                    break;
            }
        }
        private bool UseInternalIdentityServer(HttpRequest httpRequest, ref ProductType productName)
        {
            bool useInternalIds = false;
            if (httpRequest.Headers.ContainsHeader(RequestHeadersParam.USE_INTERNAL_IDS))
            {
                //Records Agent使用Public Ids, TODO 统一使用Product参数做区分
                useInternalIds = httpRequest.GetRequestHeadersParam(RequestHeadersParam.USE_INTERNAL_IDS) == "1";
                productName = useInternalIds ? ProductType.Records : ProductType.RecordsAgent;
            }
            else if (httpRequest.Headers.ContainsHeader(RequestHeadersParam.PRODUCT))
            {
                var productNameStr = httpRequest.GetRequestHeadersParam(RequestHeadersParam.PRODUCT);
                useInternalIds = CheckProductionNeedUsedInternalIdentityServer(productNameStr, ref productName);
            }
            else if (httpRequest.Headers.ContainsHeader(RequestHeadersParam.CLOUD_SDK))
            {
                useInternalIds = true;
            }

            if (httpRequest.Headers.ContainsHeader(RequestHeadersParam.CALLER))
            {
                var productNameStr = httpRequest.GetRequestHeadersParam(RequestHeadersParam.CALLER);
                if (productNameStr == "COP")
                {
                    productName = ProductType.COP;
                    useInternalIds = true;
                }
            }

            if (bool.TryParse(httpRequest.GetRequestHeadersParam("X_CLOUD-GOVERNANCE_VNEXT"), out var result))
            {
                useInternalIds = false;
                productName = ProductType.Myhub;
            }

            if (httpRequest.Headers.ContainsHeader(RequestHeadersParam.AOS_VNEXT))
            {
                useInternalIds = false;
                productName = ProductType.AOSVNext;
            }

            var token_source = httpRequest.GetRequestHeadersParam(RequestHeadersParam.TOKEN_SOURCE);
            if (token_source == TokenSource.SpfxOAuth.ToString())
            {
                useInternalIds = false;
                productName = ProductType.RecordsSpfx;
            }

            return useInternalIds;
        }

        private bool CheckProductionNeedUsedInternalIdentityServer(string productName, ref ProductType productType)
        {
            var useInternalIdentityServer = false;
            if (productName == ProductName.COP)
            {
                productType = ProductType.COP;
                useInternalIdentityServer = true;
            }
            else if (productName == ProductName.OC)
            {
                productType = ProductType.OC;
                useInternalIdentityServer = false;
            }

            return useInternalIdentityServer;
        }

        private string GenerateBasicInfoForRecords(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            string userName = string.Empty;
            string userId = string.Empty;
            string traceId = string.Empty;
            var tenantGroupId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "realm");
            var clientName = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "client_name");
            if (string.IsNullOrEmpty(tenantGroupId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdEmpty"), tenantGroupId)}");
            }
            else if (httpRequest.Headers.ContainsHeader("UserName"))
            {
                userName = httpRequest.GetRequestHeadersParam("UserName");
                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdUserNameEmpty"), tenantGroupId)}");
                }
            }

            if (httpRequest.Headers.ContainsHeader("ImpersonateUser"))
            {
                userName = httpRequest.GetRequestHeadersParam("ImpersonateUser");
            }
            else if(String.IsNullOrEmpty(userName))
            {
                var tenantInfo = TenantService.GetTenantInfo(tenantGroupId);
                userName = tenantInfo.RegisterEmail;
            }
            if (httpRequest.Headers.ContainsHeader("TraceId"))
            {
                traceId = httpRequest.GetRequestHeadersParam("TraceId");
            }

            TenantLocalValue.LogonGroupId = tenantGroupId;
            TenantLocalValue.LogonUserEmail = userName;
            TenantLocalValue.LogonUserId = userId;
            TenantLocalValue.TraceId = traceId;
            TenantLocalValue.ClientName = clientName;
            if (string.IsNullOrEmpty(TenantLocalValue.LogonUserId))
            {
                var tenantInfoDb = TenantService.GetTenantInfo(tenantGroupId);
                if (tenantInfoDb != null)
                {
                    var accountInfo = AccountDao.GetActiveUserByNameAsync(tenantInfoDb.RegisterEmail).GetAwaiter().GetResult();
                    if (accountInfo != null)
                    {
                        TenantLocalValue.LogonUserId = accountInfo.UserId;
                        TenantLocalValue.LogonUserEmail = accountInfo.UserPrincipalName;
                        TenantLocalValue.LogonGroupEmail = accountInfo.UserPrincipalName;
                    }
                }
            }
            ClientRequestLocalValue.ClientIP = httpRequest.HttpContext.GetClientIP();
            return tenantGroupId;
        }

        private string GenerateBasicInfoForRecordsAgent(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            string userName = string.Empty;
            string userId = string.Empty;
            string traceId = string.Empty;
            var tenantGroupId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "realm");
            var clientName = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "client_name");
            if (string.IsNullOrEmpty(tenantGroupId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdEmpty"), tenantGroupId)}");
            }
            else if (httpRequest.Headers.ContainsHeader("UserName"))
            {
                userName = httpRequest.GetRequestHeadersParam("UserName");
                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdUserNameEmpty"), tenantGroupId)}");
                }
            }

            if (httpRequest.Headers.ContainsHeader("ImpersonateUser"))
            {
                userName = httpRequest.GetRequestHeadersParam("ImpersonateUser");
            }
            else if (String.IsNullOrEmpty(userName))
            {
                var tenantInfo = TenantService.GetTenantInfo(tenantGroupId);
                userName = tenantInfo.RegisterEmail;
            }
            if (httpRequest.Headers.ContainsHeader("TraceId"))
            {
                traceId = httpRequest.GetRequestHeadersParam("TraceId");
            }

            TenantLocalValue.LogonGroupId = tenantGroupId;
            TenantLocalValue.LogonUserEmail = userName;
            TenantLocalValue.LogonUserId = userId;
            TenantLocalValue.TraceId = traceId;
            TenantLocalValue.ClientName = clientName;
            ClientRequestLocalValue.ClientIP = httpRequest.HttpContext.GetClientIP();
            return tenantGroupId;
        }

        private string GenerateBasicInfoForCOP(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            string userName = string.Empty;
            string userId = string.Empty;
            var tenantGroupId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "realm");

            if (httpRequest.Headers.ContainsHeader("UserName"))
            {
                userName = httpRequest.GetRequestHeadersParam("UserName");
                if (string.IsNullOrEmpty(userName))
                {
                    throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdUserNameEmpty"), tenantGroupId)}");
                }
            }

            TenantLocalValue.LogonGroupId = tenantGroupId;
            TenantLocalValue.LogonUserEmail = userName;
            TenantLocalValue.LogonUserId = userId;
            return tenantGroupId;
        }

        private string GenerateBasicInfoForMyhub(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            ClientRequestLocalValue.ClientIP = httpRequest.HttpContext.GetClientIP();
            var tenantGroupId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "customer_id");
            if (string.IsNullOrEmpty(tenantGroupId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdEmpty"), tenantGroupId)}");
            }

            var userId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "uid");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_UserIdEmpty"), tenantGroupId)}");
            }

            var userName = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            if (!string.IsNullOrEmpty(userName))
            {
                TenantLocalValue.LogonUserEmail = userName;
            }

            var licenseInfos = claimsPrincipal.FindAll("licenses");

            if (!licenseInfos.Any())
            {
                throw new Exception($"{string.Format("No Opus license, can not use myhub module, {0}", tenantGroupId)}");
            }

            var licenses = new List<AosSsoLicenseInfo>();
            foreach (var licenseInfo in licenseInfos)
            {
                if (!string.IsNullOrEmpty(licenseInfo.Value))
                {
                    licenses.Add(JsonConvert.DeserializeObject<AosSsoLicenseInfo>(licenseInfo.Value));
                }
            }

            var recordsLicenseInfo = licenses.FirstOrDefault(o => o.Product.Equals(RecordsConstants.RECORDS_APPLICATION_NAME));
            if (recordsLicenseInfo == null)
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_NoLicense"), tenantGroupId)}");
            }

            if (!recordsLicenseInfo.AcceptedLicenseAgreement && (DateTime.UtcNow.Ticks - recordsLicenseInfo.LicenseAgreementUpdateTime) / (double)TimeSpan.TicksPerDay > 30)
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_NoAcceptLicense"), tenantGroupId)}");
            }

            _ = int.TryParse(httpRequest.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-LANGUAGE"), out var cultureInfo);

            TenantLocalValue.LogonGroupId = tenantGroupId;
            TenantLocalValue.LogonUserId = userId;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureInfo);

            try
            {
                var userAadId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "objectid");
                if (!string.IsNullOrEmpty(userAadId))
                {
                    logger.Debug($"Get userAadId from claim: {userAadId}");
                    var o365TenantId = RMAosApiClient.GetO365TenantIdByUserAadId(tenantGroupId, userAadId);
                    var groups = AccountWrapperService.GetGroupsByUserId(tenantGroupId, userAadId, o365TenantId);
                    var groupIds = groups.Select(g => g.Id).ToList();
                    var aosGroups = RMAosApiClient.GetGroupsByAadIds(tenantGroupId, groupIds);
                    TenantLocalValue.UserGroups = aosGroups.ConvertAll(item => new AzureADGroupInfo
                    {
                        ObjectId = item.UserId,
                        DisplayName = item.DisplayName
                    });
                    UserService.SyncLogonUserGroupAsync(userId).GetAwaiter().GetResult();
                }
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while sync user and group infoes to opus. Error: {e}");
            }
            

            return tenantGroupId;
        }

        private string GenerateBasicInfoForSpfx(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            ClientRequestLocalValue.ClientIP = httpRequest.HttpContext.GetClientIP();
            var cidInRequest = httpRequest.GetRequestHeadersParam("Customer-Id")?.ToString();
            var userId = claimsPrincipal.FindFirstValue(ClaimConstants.ObjectId);
            var upn = claimsPrincipal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            string tenantGroupId = TenantHelper.GetTenantByUPNAsync(upn, cidInRequest).GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(tenantGroupId))
            {
                TenantLocalValue.LogonUserEmail = upn;
                TenantLocalValue.LogonUserId = userId;
                TenantLocalValue.LogonGroupId = tenantGroupId;
                return tenantGroupId;
            }
            throw new Exception($"Can not get AOS tenant.");
        }

        private string GenerateBasicInfoForAosNext(ClaimsPrincipal claimsPrincipal, HttpRequest httpRequest)
        {
            var tenantGroupId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "customer_id");
            if (string.IsNullOrEmpty(tenantGroupId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantGroupIdEmpty"), tenantGroupId)}");
            }

            var userId = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "uid");
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_UserIdEmpty"), tenantGroupId)}");
            }

            var userName = GetClaimValueByCliamTypeName(claimsPrincipal.Claims, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
            if (!string.IsNullOrEmpty(userName))
            {
                TenantLocalValue.LogonUserEmail = userName;
            }

            var cultureInfo = httpRequest.GetRequestHeadersParam("X-CLOUD-GOVERNANCE-LANGUAGE")?.ToString();

            TenantLocalValue.LogonGroupId = tenantGroupId;
            TenantLocalValue.LogonUserId = userId;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(cultureInfo);
            return tenantGroupId;
        }

        private void ValidAgentInfo(string tenantId, ClaimsPrincipal result)
        {
            try
            {
                if (!CheckLicense(tenantId))
                {
                    throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_TenantIdExpired"), tenantId)}");
                }
                string agentId = GetClaimValueByCliamTypeName(result.Claims, "agent_id");
                string installationCode = GetClaimValueByCliamTypeName(result.Claims, "installation_code");
                RMAgentDto agent = new RMAgentDto();
                if (!string.IsNullOrEmpty(agentId) && !string.IsNullOrEmpty(installationCode))
                {
                    var hasAdditionalDataSource = TenantInfoService.AdditionalDataSourceEnable(tenantId);
                    var hasCopLicense = TenantService.CheckLicenseWithAdditionalProduct(tenantId, PaidForProduct.OpusFileSystemDiscovery);

                    if (!hasAdditionalDataSource && !hasCopLicense)
                    {
                        throw new Exception($"{string.Format(I18NEntity.GetString("RM_VT_SourceLicenseExpired"), tenantId)}");
                    }

                    agent.Id = new Guid(agentId);
                    agent.AuthCode = installationCode;
                    agent.TenantId = tenantId;
                    ValidateAgentAuthCode(agent);
                    logger.Info($"valid agent info success, tenant:{tenantId}, agent:{agentId}.");
                }

            }
            catch (Exception e)
            {
                logger.Error("validate agent information fail, ", e.ToString());
                throw e;
            }
        }

        private bool CheckLicense(string customerId)
        {
            var cacheKey = $"jwttoken_lic_{customerId}";
            if (!cache.TryGetValue<bool>(cacheKey, out bool hasLicense) || !hasLicense)
            {
                hasLicense = TenantInfoService.CheckTenantLicenseIsAvailable(customerId);
                cache.Set(cacheKey, hasLicense, TimeSpan.FromMinutes(20));
            }
            return hasLicense;
        }

        //private static bool IsIgnoreUserApiAttribute(HttpRequest request)
        //{
        //    try
        //    {
        //        var httpConfig = request?.GetConfiguration() as HttpConfiguration;
        //        var selectController = httpConfig?.Services?.GetHttpControllerSelector().SelectController(request);

        //        return (selectController.GetCustomAttributes<IgnoreUserApiAttribute>().Any() || string.Equals(selectController.ControllerName, "Metadata"));
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("Valid log api attribute failed, error: {0}", e.ToString());
        //        return false;
        //    }
        //}


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

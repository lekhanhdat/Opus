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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RecordsLogIn.Controllers
{
    public class AccountController : Controller
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AccountController));
        //private static readonly IRMDigitalSignatureHelperFactory digitalSignatureHelperFactory =
        //  new RMDigitalSignatureHelperFactory();
        private readonly List<string> whiteList = new List<string>()
        { 
           "",
           "Root/PRM/MyRequest", 
           "Root/RDM/ManualApprovalReview",
           "Root/JM/Index", 
           "Root/Home" 
        };
        // GET: Account
        [AllowAnonymous]
        public ActionResult LogOn()
        {
            try
            {
                #region init property
                var idToken = Request.Form["id_token"].ToString();
                string url = Request.Form["state"].ToString();
                if (whiteList.Any(w => w == url))
                {
                    url = Request.Form["state"].ToString();
                }
                else
                {
                    logger.Warn("Invaild Redirect Url");
                    this.Response.StatusCode = 400;
                    return null;
                }
                logger.Info("url:{0}", url);
                if (!ValidateToken(idToken))
                {
                    logger.Warn("Invaild ID Token");
                    this.Response.StatusCode = 401;
                    return null;
                }
                //logger.Info($"login Id token info {idToken}");
                JsonWebToken token = new JsonWebToken(idToken);

                var tenantId = token.Claims.SingleOrDefault(item => item.Type.Equals("tid"))?.Value;
                var userName = token.Claims.SingleOrDefault(item => item.Type.Equals("preferred_username"))?.Value;
                var dUserName = token.Claims.SingleOrDefault(item => item.Type.Equals("preferred_username"))?.Value;
                var UserObjectId = token.Claims.SingleOrDefault(item => item.Type.Equals("oid"))?.Value;
                //userName = userName + "#" + UserObjectId;
                logger.Info($"login tanant info: {tenantId} : {UserObjectId}");
                var loginInfo = RMAosApiClient.ValidateUserByObjectId(UserObjectId, userName, tenantId);
                #endregion
                if (loginInfo.IsInAOS)
                {
                    logger.Info($"User in AOS start init login info {tenantId} : {UserObjectId}");
                    string aveTenantId = loginInfo.Account.Customer.Id;
                    AosUserInfo userInfo = new AosUserInfo();
                    userInfo.InviteType = (RMActiveDirectoryObjectType)((int)loginInfo.Account.InviteType);
                    List<AosRole> aosRoles = new List<AosRole>();
                    foreach (var role in loginInfo.Account.PostRole.OrderByDescending(a => a.UserType).ToList())
                    {
                        var aosRole = new AosRole();
                        aosRole.ApplicationName = role.ApplicationName;
                        aosRole.IsAcceptedLicenseAgreement = role.IsAcceptedLicenseAgreement;
                        aosRole.Url = role.Url;
                        aosRole.UserType = (RMAccountType)role.UserType;
                        aosRoles.Add(aosRole);
                    }

                    userInfo.UserId = loginInfo.Account.Id;
                    userInfo.Roles = aosRoles;
                    userInfo.Username = loginInfo.Account.Name;
                    userInfo.CustomerId = loginInfo.Account.Customer.Id;
                    userInfo.UserGroups = loginInfo.Account.UserGroups;
                    userInfo.ExpireTime = DateTime.UtcNow;
                    //RMCertificateHelper.InitCerts();
                    //var digitalSignatureHelper = digitalSignatureHelperFactory.Create(RecordsConstants.RECORDS_APPLICATION_NAME);
                    //var user = JsonUtil.JsonSerializerObj(userInfo);

                    //var signature = digitalSignatureHelper.SignData(user);
                    if (RMAosApiClient.IsCustomerLicenseAvailable(aveTenantId))
                    {
                        var serviceUrl = RMAosApiClient.GetRecordsServiceUrl(aveTenantId);
                        var queryNameValueCollection = HttpUtility.ParseQueryString(string.Empty);
                        //queryNameValueCollection.Add("user", user);
                       // queryNameValueCollection.Add("signature", signature);//not use now
                        queryNameValueCollection.Add("product", "AvePointRecords");
                        queryNameValueCollection.Add("redirect", url);
                        UriBuilder returnUrlBuilder = new UriBuilder($"{serviceUrl}/Account/Office365Logon");
                        returnUrlBuilder.Query = queryNameValueCollection.ToString();
                        logger.Info("Redirect to records web");
                        return this.Redirect(returnUrlBuilder.Uri.AbsoluteUri);
                    }
                    else 
                    {
                        logger.Info("Customer license no available.");
                        this.Response.StatusCode = 500;
                        return this.RedirectToAction("NoPermission", "ErrorPage");
                    }
                }
                else
                {
                    logger.Info($"User validate from aos failed {UserObjectId}");
                    this.Response.StatusCode = 401;
                    return this.RedirectToAction("NotAvailableService", "ErrorPage");
                }
            }
            catch (Exception e)
            {
                logger.Info($"Init login info failed {e.ToString()}");
                this.Response.StatusCode = 500;
                return this.RedirectToAction("NoPermission", "ErrorPage");
            }
        }
        public class LogOnInfo
        {
            public string product { get; set; }
            public string signature { get; set; }
            public string user { get; set; }
        }

        public static Boolean ValidateToken(String token)
        {
            OpenIdConnectConfiguration config = null;
            try
            {
                string stsDiscoveryEndpoint = "https://login.microsoftonline.com/common/.well-known/openid-configuration";
                ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
                config = configManager.GetConfigurationAsync().Result;
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while getting open id configuration. {ex}");
            }


            if (config == null)
            {
                //cannot get open id configuration, think token as valid.
                return true;
            }
            try
            {

                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateLifetime = true,
                };
                JsonWebTokenHandler tokendHandler = new JsonWebTokenHandler();
                var result = tokendHandler.ValidateToken(token, validationParameters);
                if (result != null && result.SecurityToken != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while validating token. {ex}");
            }
            logger.Info("the azure ad token is invalid.");
            return false;
        }
    }
}

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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace RecordsLogIn.Controllers
{
    public class HomeController : Controller
    {
        private RALogger logger = RALogger.GetInstance(typeof(HomeController));

        private const string EnvironmentName = "21V China North";

        [AllowAnonymous]
        public Microsoft.AspNetCore.Mvc.ActionResult Index()
        {
            try
            {
                string formatUrl = @"https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize?response_type=id_token&response_mode=form_post&client_id={0}&scope={1}&redirect_uri={2}&state={3}&nonce={4}";
                var environmentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                if(environmentName.Equals(EnvironmentName, StringComparison.OrdinalIgnoreCase))
                {
                    formatUrl = @"https://login.partner.microsoftonline.cn/organizations/oauth2/v2.0/authorize?response_type=id_token&response_mode=form_post&client_id={0}&scope={1}&redirect_uri={2}&state={3}&nonce={4}";
                }
                string graphResouce = "https://graph.microsoft.com/";
                string Scope = "openid%20profile%20offline_access%20https://graph.microsoft.com/User.Read";
                string clientId = RMGlobalConfiguration.AppConfig[AvePoint.RA.Contract.Configurations.RMAppSettingKey.CLIENT_ID_IN_RECO_LOGIN_WEB];
                
                string loginUrl = Request.GetUrl().AbsoluteUri;
                if (!loginUrl.StartsWith("https"))
                {
                    loginUrl = loginUrl.Replace("http", "https");
                }
                string originalHost = string.Empty;
    
                logger.Info("loginUrl:{0}",loginUrl);
                //Need remove log later.
                if (Request.Headers.Keys.Any(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase)))
                {
                    string originalHostKey = Request.Headers.Keys.FirstOrDefault(a => a.Equals("X-Original-Host", StringComparison.OrdinalIgnoreCase));
                    originalHost = Request.Headers.GetHeaderValue(originalHostKey);
                    if (loginUrl.IndexOf('?') >= 0)
                    {
                        loginUrl = "https://" + originalHost + "/" + loginUrl.Substring(loginUrl.IndexOf("?"));
                    }
                    else 
                    {
                        loginUrl = "https://" + originalHost + "/";
                    }
                    logger.Info("Original Host {0}, URL: {1}, {2}", originalHostKey, originalHost, loginUrl);
                }
                var domainUrl = loginUrl;
                var state = string.Empty;
                if (loginUrl.IndexOf('?') >= 0)
                {
                    domainUrl = loginUrl.Substring(0, loginUrl.IndexOf("?"));
                    state = loginUrl.Substring(loginUrl.IndexOf("=") + 1);
                }
                logger.Info("loginUrl:{0},headUrl:{1},queryUrl{2}", loginUrl, domainUrl, state);
                var redirectUrl = string.Format(formatUrl, clientId, Scope, $"{domainUrl}Account/Logon" , state, Guid.NewGuid().ToString());
                logger.Info($"red url {redirectUrl}");
                return this.Redirect(redirectUrl);
            }
            catch(Exception e)
            {
                logger.Error("One error occured in Index page redirect to Account Logon page {0} ", e.ToString());
                return null;
            }
        }
    }
}
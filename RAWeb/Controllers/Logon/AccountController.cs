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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.Authentication;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Security.Aos;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Extentions.Util;
using AvePoint.RA.Contract.Logon;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.Web.Controllers.Logon
{
    public class AccountController : BaseController
    {
        private ILoginService _LoginService;
        private ILoginService LoginService => PlatformWindsorManager.GetService(ref _LoginService);
        private IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMFunctionSettingDao _functionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();


        private readonly List<string> whiteList = new List<string>()
        {
           "Root/PRM/MyRequest",
           "Root/RDM/ManualApprovalReview",
           "Root/Home",
           "Root/MT/MachineLearningReview",
           "records",
           "Root/JM/Index",
           "Root/FileAnalysis/ROTOptimization",
           "Root/FileAnalysis/Discovery/Configuration",
           "Root/BCM/ManageHold",
           "Root/PRM/RecordsExplorer"
        };

        private static readonly RALogger logger = RALogger.GetInstance(typeof(AccountController));
        private ISecurityService _SecurityService = null;
        public ISecurityService SecurityService
        {
            get
            {
                if (_SecurityService == null)
                {
                    _SecurityService = new SecurityService();
                }
                return _SecurityService;
            }
        }
        /// <summary>
        /// 登录方法
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <param name="needLogOut">如果为true，则返回首页登录页面</param>
        /// <returns></returns>
        [RACodeReview("Allen Yin", "逻辑稍微有点绕，直观上不太容易看懂")]
        [AllowAnonymous]
        public IActionResult LogOn(string redirectUrl = "", bool needLogOut = false)
        {

            //if (!needLogOut && LoginService.IsAuthenticated())
            //{
            //    //如果验证通过，redirect至先前的页面

            //    return this.RedirectFromLogin(redirectUrl);
            //}
            //else
            //{
            //    InitLogOnPageData();
            return this.RedirectFromLogin("");
            //}
        }

       

        /// <summary>
        /// 用于前台ajax请求，如果能够正常返回消息，说明session没有过期
        /// </summary>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [RMAuthorize(Contract.RoleAssignments.RMPermissionMasks.CommonModuleAccess, Contract.RoleAssignments.RMSOPermissionMasks.CommonModuleAccess | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public IActionResult CheckSession()
        {
            var checkResult = (int)CheckSessionResult.Success;
            return Content(checkResult.ToString());
        }

        [AllowAnonymous]
        public IActionResult SessionTimeout()
        {
            var checkResult = (int)CheckSessionResult.SessionTimeout;
            return Content(checkResult.ToString());
        }

        [AllowAnonymous]
        public IActionResult ForcedLogout()
        {
            var checkResult = (int)CheckSessionResult.ForcedLogout;
            return Content(checkResult.ToString());
        }

        [RACodeReview("Allen Yin")]
        public async Task<IActionResult> LogOut()
        {
            try
            {
                var rmIdentity = await HttpContext.Request.GetRMIdentityAsync();
                await LoginService.LogOutAsync(rmIdentity);

                Response.Cookies.Delete(AuthCookie.CookieName, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
                });

                if (CurrentUser != null && CurrentUser.AuthType == RMAuthenticationTypes.Office365)
                {
                    return this.Redirect("https://login.microsoftonline.com/common/oauth2/logout");
                }
                return this.RedirectToAction("SSOLogout", new { userId = CurrentUser?.AccountId });
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while logout:{ex.ToString()}");
            }

            return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response));
        }

        //[AllowAnonymous]

        //public async Task<IActionResult> Office365Logon()
        //{
        //    string user = Request.Query["user"];
        //    string signature = Request.Query["signature"];
        //    string product = "AvePointRecords";
        //    string redirect = Request.Query["redirect"];
        //    IActionResult result = null;
        //    LogOnInfo loginInfo = new LogOnInfo();
        //    loginInfo.user = user;
        //    loginInfo.product = product;
        //    loginInfo.signature = signature;
        //    {

        //        //logger.Info($"login user is { loginInfo?.user }");
        //        ClientRequestLocalValue.ClientIP = HttpContext.GetClientIP();
        //        var (loginResult, identity) = await LoginService.Office365LoginAsync(loginInfo);
        //        if (loginResult.MessageType == Contract.Object.RAMessageType.Successful)
        //        {
        //            if (whiteList.Any(w => redirect.Contains(w)) || string.IsNullOrEmpty(redirect))
        //            {
        //                var serviceUrl = RMAosApiClient.GetRecordsServiceUrl(TenantLocalValue.LogonGroupId);
        //                var redirectUrl = string.IsNullOrEmpty(redirect) ? "" : serviceUrl.TrimEnd(new char[] { '/' }) + "/" + redirect;

        //                Response.SetRMIdentity(identity, HttpContext.Request.Host.Host);
        //                result = RedirectFromLogin(redirectUrl);
        //            }
        //            else
        //            {
        //                result = this.RedirectToAction("NotAvailableService", "ErrorPage");
        //            }
        //        }
        //        else
        //        {
        //            logger.Error($"login error:{loginResult.FaildType}, :{loginResult.ErrorMessage}");
        //            if (loginResult.FaildType == Contract.Object.RAFailedType.SoftDeleted)
        //            {
        //                result = this.RedirectToAction("NotAvailableService", "ErrorPage");
        //            }
        //            else if (loginResult.FaildType == Contract.Object.RAFailedType.AccessDenied)
        //            {
        //                result = this.RedirectToAction("NoPermission", "ErrorPage");
        //            }
        //            else if (loginResult.FaildType == Contract.Object.RAFailedType.CloudArchiverLicenseExpired)
        //            {
        //                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_CloudArchiver_LicenseExpired_Message") });
        //            }
        //            else if (loginResult.FaildType == Contract.Object.RAFailedType.LicenseDoesNotAllowLogin)
        //            {
        //                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_CloudArchiver_LicenseNotAllowLogin_Message") });
        //            }
        //            else if (loginResult.FaildType == Contract.Object.RAFailedType.UseCloudArchiving)
        //            {
        //                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_AR_UsingCloudArchiving") });
        //            }
        //            else
        //            {
        //                this.TempData["ErrorMessage"] = I18NEntity.GetString("RM_LogOn_ErrorMsg_IncorrectInput");
        //                result = this.Redirect(AveUrlUtility.CombineUrl(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.AOS_URL], "account/LogOff"));

        //            }

        //        }
        //    }

        //    return result;
        //}
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [RACodeReview("Allen Yin")]
        private IActionResult RedirectFromLogin(string redirectUrl)
        {
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return this.Redirect(HttpUtility.UrlDecode(redirectUrl));
            }
            else
            {
                return this.RedirectToAction("Home", "Root");
            }
        }


        [AllowAnonymous]
        public IActionResult LoginApp()
        {
            string redirectUrl = Request.Query["redirect"];
            string enhanced = Request.Query["enhanced"];
            bool isEnhanced = !string.IsNullOrEmpty(enhanced) && enhanced.ToLower() == "true";

            string enhancedParam = isEnhanced ? "|enhanced=true" : "";

            if (redirectUrl.IsNullOrEmpty())
            {
                return this.Redirect(RMSSOHelper.AppSSOLoginUrl + $"&state={enhancedParam}");
            }
            else if (whiteList.Any(w => redirectUrl.Contains(w)))
            {
                return this.Redirect(RMSSOHelper.AppSSOLoginUrl + $"&state={redirectUrl}{enhancedParam}");
            }
            else
            {
                return this.RedirectToAction("NotAvailableService", "ErrorPage");
            }
        }

        [Authorize]
        public new IActionResult SignOut()
        {
            this.HttpContext.SignOutAsync().GetAwaiter().GetResult();
            return new EmptyResult();
        }

        [Authorize]
        public IActionResult SignOutCleanup()
        {
            // this should only signout of cookies since it will be running in an iframe
            this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            return new EmptyResult();
        }

        [AllowAnonymous]
        public IActionResult SSOLogin(string product = null, string login_hint = null)
        {
            if(Request.Method == "POST" && Request.Form != null && Request.Form.Count != 0)
            {
                var state = Request.Form["state"];
                var token = Request.Form["token"];
                var correlation_id = Request.Form["correlation_id"];
                var access_token = Request.Form["access_token"];
                var refresh_token = Request.Form["refresh_token"];
                return LoginForSSO(state, token, correlation_id, access_token, refresh_token, false).GetAwaiter().GetResult();
            }
            string redirectUrl = Request.Query["redirect"];
            if (redirectUrl.IsNullOrEmpty())
            {
                if (string.IsNullOrEmpty(login_hint))
                {
                    return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response) + $"&state=");
                }
                return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response) + $"&state=" + $"&login_hint={login_hint}");
            }
            else if (whiteList.Any(w => redirectUrl.Contains(w)))
            {
                if (string.IsNullOrEmpty(login_hint))
                {
                    return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response) + $"&state={redirectUrl}");
                }

                return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response) + $"&state={redirectUrl}" + $"&login_hint={login_hint}");
            }
            else
            {
                return this.RedirectToAction("NotAvailableService", "ErrorPage");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoginForSSO(string state, string token, string correlation_id, string access_token, string refresh_token, bool needVerifyNonce = true)
        {
            IActionResult result = null;
            if (needVerifyNonce && !VerifyNonce(correlation_id))
            {
                result = this.RedirectToAction("NotAvailableService", "ErrorPage");
                return result;
            }
            ClientRequestLocalValue.ClientIP = HttpContext.GetClientIP();
            var logonInfo = new RMLogonInfo(state, token, correlation_id, access_token, refresh_token);
            var (loginResult, identity) = await LoginService.SSOLoginAsync(logonInfo);
            if (loginResult.MessageType != Contract.Object.RAMessageType.Successful)
            {
                return HandleLoginFailed(loginResult, identity.DataCenter);
            }
            TenantLocalValue.Init(identity);
            (loginResult, identity) = await LoginService.SSOLoginAsync(logonInfo, identity);

            if (loginResult.MessageType != Contract.Object.RAMessageType.Successful)
            {
                return HandleLoginFailed(loginResult);
            }

            var serviceUrl = RMAosApiClient.GetRecordsServiceUrl(identity.TenantGroupId);
            if (await _functionSettingDao.IsEnableMultiGeoFeature(_keyValueDao))
            {
                serviceUrl = RMSSOHelper.RecoHostUrl;
            }
            var redirectUrl = string.IsNullOrEmpty(state) ? "" : serviceUrl.TrimEnd(new char[] { '/' }) + "/" + state;
            if (string.IsNullOrEmpty(state) || whiteList.Any(w => state.Contains(w)))
            {
                Response.SetRMIdentity(identity, logonInfo, RMSSOHelper.RECO_SSO_DOMAIN_NAME);
                result = RedirectFromLogin(redirectUrl);
            }
            else
            {
                result = this.RedirectToAction("NotAvailableService", "ErrorPage");
            }

            return result;
        }
        /// <summary>
        /// For mobile app login
        /// </summary>
        /// <param name="state">redirect url param</param>
        /// <param name="token">login token</param>
        /// <param name="access_token">access api token</param>
        [AllowAnonymous]
        public async System.Threading.Tasks.Task Login2AppSSO(string state, string token, string access_token)
        {
            var postURL = string.Empty;
            var redirectUrl = "records";

            // Check if enhanced mode is enabled via state parameter
            bool isEnhanced = !string.IsNullOrEmpty(state) && state.Contains("enhanced=true");

            try
            {
                var (loginResult, loginInfo) = await LoginService.MobileSSOLogin(state, token, access_token);
                if (loginResult.MessageType != Contract.Object.RAMessageType.Successful)
                {
                    logger.Error($"login error:{loginResult.FaildType}, :{loginResult.ErrorMessage}");
                    postURL = $"{redirectUrl}://error={(int)HttpStatusCode.Forbidden}";
                    if (isEnhanced)
                    {
                        RedirectToAppWithWebView(postURL);
                    }
                    else
                    {
                        Redirect2App(postURL);
                    }
                    return;
                }
                var accessToken = GetAccessToken(loginInfo);
                postURL = $"{redirectUrl}://token=" + accessToken + "|url=" + HttpUtility.UrlEncode(loginInfo?.AppUrl);
            }
            catch (Exception ex)
            {
                postURL = $"{redirectUrl}://error={(int)HttpStatusCode.Forbidden}";
                logger.Error($"mobile login error: {ex.ToString()}");
            }

            if (isEnhanced)
            {
                RedirectToAppWithWebView(postURL);
            }
            else
            {
                Redirect2App(postURL);
            }
        }

        /// <summary>
        /// Enhanced redirect method for WebView communication
        /// </summary>
        /// <param name="redirectUrl">URL to redirect to</param>
        private void RedirectToAppWithWebView(string redirectUrl)
        {
            this.Request.Method = "Post";
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<script>");
            sb.Append("window.onload = function() {");
            sb.AppendFormat("var message = '{0}';", redirectUrl);
            sb.Append("");
            //sb.Append("// Try React Native WebView (React Native apps)");
            sb.Append("if (window.ReactNativeWebView && window.ReactNativeWebView.postMessage) {");
            sb.Append("window.ReactNativeWebView.postMessage(message);");
            sb.Append("return;");
            sb.Append("}");
            sb.Append("");
            //sb.Append("// Try Android WebView (Native Android apps)");
            sb.Append("if (window.AndroidInterface && window.AndroidInterface.handleRedirect) {");
            sb.Append("window.AndroidInterface.handleRedirect(message);");
            sb.Append("return;");
            sb.Append("}");
            sb.Append("");
            //sb.Append("// Try iOS WebKit (Native iOS apps)");
            sb.Append("if (window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.iosHandler) {");
            sb.Append("window.webkit.messageHandlers.iosHandler.postMessage(message);");
            sb.Append("return;");
            sb.Append("}");
            sb.Append("");
            //sb.Append("// Fallback: direct URL redirect for web browsers");
            sb.Append("window.location.href = message;");
            sb.Append("};");
            sb.Append("</script>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<p>Redirecting to app...</p>");
            sb.Append("</body>");
            sb.Append("</html>");
            Response.Clear();
            Response.ContentType = "text/html";
            Response.WriteAsync(sb.ToString()).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 防止CSRF问题加入了Nonce校验, 登录前存入cookie与AOS返回的值对比.
        /// </summary>
        /// <param name="correlation_id"></param>
        /// <returns></returns>
        private bool VerifyNonce(string correlation_id)
        {

            if (RMGlobalConfiguration.AppConfig.IsGovStaging())
            {
                //due to aos test env host changed, correlation is is null, skip to check nonce
                logger.Warn($"staging env skip check nonce:{correlation_id}.");
                return true;
            }
            var nonceId = NonceCookie.GetNonce(Request);
            var result = string.Equals(correlation_id, nonceId);
            if (!result)
            {
                logger.Warn($"Nonce validate failed:{correlation_id}, {nonceId}");
            }
            return result;
        }
        private string GetAccessToken(RMLoginInfo loginInfo)
        {
            var sessionTimeOutMinute = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.MOBILE_SESSION_TIMEOUT_MINUTES]);

            //为了兼容旧App, 这里还是保持原有的token模型, API验证使用的是access_token.
            var tokenObj = this.SecurityService.GenerateToken(loginInfo, sessionTimeOutMinute);
            return HttpUtility.UrlEncode(JsonConvert.SerializeObject(ModeConvertUtil.FromAccessToken(tokenObj)));
        }
        private void Redirect2App(string redirectUrl)
        {
            this.Request.Method = "Post";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"<html><meta http-equiv='refresh' content='0;url={redirectUrl}'></html>");
            Response.Clear();
            Response.ContentType = "text/html";
            Response.WriteAsync(sb.ToString()).GetAwaiter().GetResult();
        }
        [AllowAnonymous]
        public IActionResult AcceptLicenseAgreement(string ui, string un)
        {
            LoginService.AcceptLicenseAgreement(new SsoSamplerUserInfo { CustomerId = ui, UserName = un }, this.HttpContext.GetClientIP());
            return this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response));
        }

        [AllowAnonymous]
        public IActionResult SSOLogout(string userId)
        {
            var nonce = HttpContext.Items["Nonce"] as string;
            ViewBag.Nonce = nonce;
            ViewBag.ssoLogoutUrl = RMSSOHelper.SsoLogoutUrl;
            ViewBag.clientId = RMSSOHelper.SsoClientId;
            ViewBag.redirectUrl = HttpUtility.UrlDecode(RMSSOHelper.RecoSsoLoginUrl);
            ViewBag.userId = !string.IsNullOrEmpty(userId) ? userId : CurrentUser?.AccountId;
            return View();
        }

        private IActionResult HandleLoginFailed(RAReturnMessage loginResult, string dataCenter = "") 
        {
            IActionResult result;
            logger.Error($"login error:{loginResult.FaildType}, :{loginResult.ErrorMessage}");
            if (loginResult.FaildType == Contract.Object.RAFailedType.SoftDeleted)
            {
                result = this.RedirectToAction("NotAvailableService", "ErrorPage");
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.AccessDenied)
            {
                result = this.RedirectToAction("NoPermission", "ErrorPage");
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.SSOLoginFailed)
            {
                var failedMessage = LoginService.GetSsoLoginFailedMessage(loginResult.ErrorMessage);
                var logoutLink = $"<a href='{RMSSOHelper.RecoSsoLogoutUrl}'>{I18NEntity.GetString("RM_SSO_GOBack_Link_Title")}</a>";
                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = string.Format(failedMessage, logoutLink) });
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.BlockedByIpRestriction)
            {
                var configurationSections = RMGlobalConfiguration.AppConfig.GetMultiGeoDomainUrl();
                var mainDC = dataCenter;
                var domainUrl = configurationSections?.FirstOrDefault(section => string.Equals(section.Key, mainDC, StringComparison.OrdinalIgnoreCase)).Value;

                if (string.IsNullOrEmpty(mainDC) || string.IsNullOrEmpty(domainUrl))
                {
                    return View("LoginFailed", new SsoSamplerUserInfo
                    {
                        FailedMessage = string.Format(I18NEntity.GetString("RM_SSO_Message_IPMultiGeoForbidden"), "")
                    });
                }

                var redirectMainDC = $"{domainUrl.TrimEnd('/')}/Root/Home";
                var failedMessage = I18NEntity.GetString("RM_SSO_Message_IPMultiGeoForbidden");
                var mainDCLink = $"<a href='{redirectMainDC}'>{I18NEntity.GetString("RM_SSO_GOBack_Link_Title")}</a>";

                return View("LoginFailed", new SsoSamplerUserInfo
                {
                    FailedMessage = string.Format(failedMessage, mainDCLink)
                });
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.NotAcceptLicenseAgreement)
            {
                var userInfo = loginResult.Extsion1 as SsoSamplerUserInfo;
                if (userInfo != null && userInfo.AccountType != RMAccountType.ApplicationAdmin)
                {
                    return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_JS_LicenseAgreement_Message") });
                }
                var nonce = HttpContext.Items["Nonce"] as string;
                ViewBag.Nonce = nonce;
                return View("LicenseAgreement", userInfo);
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.CloudArchiverLicenseExpired)
            {
                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_CloudArchiver_LicenseExpired_Message") });
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.UseCloudArchiving)
            {
                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_AR_UsingCloudArchiving") });
            }
            else if (loginResult.FaildType == Contract.Object.RAFailedType.LicenseDoesNotAllowLogin)
            {
                return View("LoginFailed", new SsoSamplerUserInfo { FailedMessage = I18NEntity.GetString("RM_CloudArchiver_LicenseNotAllowLogin_Message") });
            }
            else
            {
                this.TempData["ErrorMessage"] = I18NEntity.GetString("RM_LogOn_ErrorMsg_IncorrectInput");
                result = this.Redirect(NonceCookie.GetSSOLoginUrlWithNonceID(Response));
            }
            return result;
        }
    }

}
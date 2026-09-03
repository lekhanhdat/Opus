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
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Web.Common.Context;
using AvePoint.RA.Web.Common.Session;
using AvePoint.RA.Web.Common.Utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Threading;
using System.Web;
using AvePoint.RA.Web;

namespace AvePoint.RA.Web
{
    public partial class MvcApplication
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MvcApplication));
        private void SessionAuthenticationModule_SessionSecurityTokenReceived(object sender, SessionSecurityTokenReceivedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var sst = e.SessionToken;
            var claimsPrincipal = sst.ClaimsPrincipal;
            var validTo = sst.ValidTo;
            if(IsMobileRequest())
            {
                //Mobile 的request 不需要check session
                return;
            }
           
            if (now >= validTo)
            {
                var sam = sender as SessionAuthenticationModule;
                sam.SignOut();

                if (!IsLogOnRequest() && (IsCheckSessionTimeOutRequest() || IsPostRequest()))
                {
                    logger.Info($"session timeout:{ClaimUtils.GetAccountId(claimsPrincipal)}, {ClaimUtils.GetSessionId(claimsPrincipal)}");
                    SessionHelper.Logout(claimsPrincipal);
                    HttpContextHelper.Current.Response.Redirect("/Account/SessionTimeout");
                }
            }
            else
            {
                var forceLoingedStatus = ClaimUtils.GetForceLoginedStatus(claimsPrincipal);
                if (forceLoingedStatus && !IsLogOnRequest())
                {
                    //forceLoingedStatus为Ture表示在AOS的SesstionAllow concurrent sign - ins from multiple locations
                    var isInvalidSession = SessionHelper.IsMarkRemovedSessionId(claimsPrincipal) || !SessionHelper.ExistSessionId(claimsPrincipal);
                    if (isInvalidSession)
                    {
                        if (IsCheckSessionTimeOutRequest())
                        {
                            logger.Info($"forced logout:{ClaimUtils.GetAccountId(claimsPrincipal)}, {ClaimUtils.GetSessionId(claimsPrincipal)}");
                            SessionHelper.Logout(claimsPrincipal);
                            HttpContext.Current.Response.Redirect("/Account/ForcedLogout");
                        }
                        else 
                        {
                            SessionHelper.Logout(claimsPrincipal);
                            return;
                        }
                    }
                }

                if (IsLogOutRequest())
                {
                    logger.Info($"Accout logout:{ClaimUtils.GetAccountId(claimsPrincipal)}, {ClaimUtils.GetSessionId(claimsPrincipal)}");
                    SessionHelper.Logout(sst.ClaimsPrincipal);
                    return;
                }

                var validFrom = sst.ValidFrom;
                //每次Extend超时时间30秒后，Request才会再次Extend超时时间
                if (!IsCheckSessionTimeOutRequest() && now > validFrom.AddSeconds(30))
                {
                    double sessionTimeout = (validTo - validFrom).TotalMinutes;
                    var sam = sender as SessionAuthenticationModule;
                    e.SessionToken = sam.CreateSessionSecurityToken(sst.ClaimsPrincipal,
                        sst.Context,
                        now,
                        now.AddMinutes(sessionTimeout),
                        sst.IsPersistent);
                    e.ReissueCookie = true;
                    SessionHelper.UpdateSession(claimsPrincipal, now.AddMinutes(sessionTimeout));
                }

                SetCulture();
            }
        }
        private bool IsPostRequest()
        {
            bool isPost = false;
            if (HttpContextHelper.Current != null && HttpContextHelper.Current.Request != null)
            {
                isPost = HttpContextHelper.Current.Request.HttpMethod.Equals("post", StringComparison.OrdinalIgnoreCase);
            }
            return isPost;
        }

        private bool IsCheckSessionTimeOutRequest()
        {
            bool isCheckSessionRequest = false;
            if (HttpContextHelper.Current != null && HttpContextHelper.Current.Request != null)
            {
                isCheckSessionRequest = HttpContextHelper.Current.Request.Url.AbsolutePath.EndsWith("/Account/CheckSession", StringComparison.OrdinalIgnoreCase);
            }
            return isCheckSessionRequest;
        }

        private bool IsLogOutRequest()
        {
            bool isLogOutRequest = false;
            if (HttpContextHelper.Current != null && HttpContextHelper.Current.Request != null)
            {
                isLogOutRequest = HttpContextHelper.Current.Request.Url.AbsolutePath.EndsWith("/Account/LogOut", StringComparison.OrdinalIgnoreCase);
            }
            return isLogOutRequest;
        }

        private bool IsMobileRequest()
        {
            bool isMobileRequest = false;
            if (HttpContextHelper.Current != null && HttpContextHelper.Current.Request != null)
            {
                isMobileRequest = HttpContextHelper.Current.Request.Url.AbsolutePath.IndexOf("api/MobileAPI", StringComparison.OrdinalIgnoreCase) > 0;
            }
            return isMobileRequest;
        }

        private bool IsLogOnRequest()
        {
            bool isLogOnRequest = false;
            if (HttpContextHelper.Current != null && HttpContextHelper.Current.Request != null)
            {
                var logOnUrls = new List<string> { "/Account/LogOn" , "/Account/LoginRecords", "/Account/LoginForCOP", "/Account/LoginForSSO" };
                if (logOnUrls.Any(o => o.EndsWith(HttpContext.Current.Request.Url.AbsolutePath, StringComparison.OrdinalIgnoreCase)))
                {
                    isLogOnRequest = true;
                }
            }
            return isLogOnRequest;
        }

        private void SetCulture()
        {

            string cultureName = null;

            var personalLanguageCookie = HttpContextHelper.Current.Request.Cookies["RM_PersonalLanguage"];
            if (personalLanguageCookie != null)
            {
                cultureName = personalLanguageCookie.Value;
            }
            else if (HttpContextHelper.Current.Request.UserLanguages != null && HttpContextHelper.Current.Request.UserLanguages.Count() > 0)
            {
                cultureName = HttpContextHelper.Current.Request.UserLanguages[0]; // obtain it from HTTP header AcceptLanguages
            }
            System.Globalization.CultureInfo ci = null;
            try
            {
                ci = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            }
            catch
            {
                ci = EnvironmentContext.GetDefaultCulture();
            }

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        //private void Application_OnPostAuthenticateRequest(object sender, EventArgs e)
        //{
        //    var identity = LoginService.GetRMIdentity();
        //    if (identity != null)
        //    {
        //        var principal = LoginService.WriteClaimsPrincipal(identity);
        //        HttpContext.Current.User = principal;
        //        Thread.CurrentPrincipal = principal;
        //    }
        //}
    }
}
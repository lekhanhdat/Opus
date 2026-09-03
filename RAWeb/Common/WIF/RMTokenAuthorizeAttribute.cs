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
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Reflection;

namespace AvePoint.RA.Web.Common.WIF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RMTokenAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {


        //public RMTokenAuthorizeAttribute() : base()
        //{
        //    this.AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme;
        //}

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            string result = string.Empty;
            string path = string.Empty;
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
//#if !DEBUG
                var redirectUrlFromConfig = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.RECO_APP_LOGIN_URL];
                var reqUrl = filterContext.HttpContext.Request.GetUrl();
                path = reqUrl.LocalPath;
                if (!string.IsNullOrEmpty(path) && redirectUrlFromConfig.IndexOf(path, StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    result = redirectUrlFromConfig.Substring(0, redirectUrlFromConfig.IndexOf(path, StringComparison.InvariantCultureIgnoreCase)) + reqUrl.PathAndQuery;

                    filterContext.HttpContext.Response.Redirect(result);
                    //HttpContext.Current.GetOwinContext().Authentication.Challenge(new AuthenticationProperties
                    //{
                    //    RedirectUri = result
                    //});
                }
                
//#endif
                filterContext.Result = new UnauthorizedResult();
            }
            //Logger.Info($"url:{result}, request:{filterContext.HttpContext.Request.Url}, Path:{path}");
        }
    }


}
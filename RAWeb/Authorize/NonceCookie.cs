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
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Http;
using System;


namespace AvePoint.RA.Web.Extentions.Authorize
{
    public static class NonceCookie
    {
        private static RALogger Logger = RALogger.GetInstance(typeof(NonceCookie));
        public static readonly string CookieName = ".NONCE_CLOUDRECORDS";
        public static void SetNonce(this HttpResponse response, string nonce, string domain)
        {
            try
            {
                response.Cookies.Append(
                    CookieName,
                    nonce,
                    new CookieOptions
                    {
                        Secure = true,
                        HttpOnly = true,
                        Expires = DateTime.Now.AddMinutes(20),
                        SameSite = SameSiteMode.Lax,
                        Domain = domain,
                    });
            }
            catch (Exception ex)
            {
                Logger.Error($"set nonce error:{ex.ToString()}");
            }
           

        }

        public static string GetNonce(this HttpRequest request)
        {
            var nonce = string.Empty;
            try
            {
                if (request.Cookies.ContainsKey(CookieName))
                {
                    nonce = request.Cookies[CookieName];
                }
               
            }
            catch (Exception ex)
            {
                Logger.Error($"get nonce error:{ex.ToString()}");
            }
            return nonce;
        }

        public static string GetSSOLoginUrlWithNonceID(HttpResponse response)
        {
            var nonceId = Guid.NewGuid().ToString();
            response.SetNonce(nonceId, RMSSOHelper.RECO_SSO_DOMAIN_NAME);
            return $"{RMSSOHelper.SsoLoginUrl}&client_request_id={nonceId}";
        }
    }
}

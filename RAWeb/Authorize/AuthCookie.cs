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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Cryptography;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Logon;
using log4net.Repository.Hierarchy;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Babel;
using System.Linq;

namespace AvePoint.RA.Web.Extentions.Authorize
{
    public static class AuthCookie
    {
        private static RALogger Logger = RALogger.GetInstance(typeof(AuthCookie));
        public static readonly string CookieName = ".AUTH_CLOUDRECORDS";
        private static readonly long SessionExtendsTicks = TimeSpan.FromSeconds(30).Ticks;

        public static void SetRMIdentity(this HttpResponse response, RMIdentity userData, RMLogonInfo logonInfo, string domain)
        {
            using (new PerformanceScope($"AuthCookie.SetRMIdentity"))
            {
                SetSessionTimeoutTime(userData);
                string token = $"{userData.TenantGroupId}|{userData.SessionId}|{logonInfo.refresh_token}";
                response.Cookies.Append(
                    CookieName,
                    Encode(token),
                    new CookieOptions
                    {
                        Secure = true,
                        HttpOnly = true,
                        //Expires = DateTime.Now.AddMinutes(userData.SessionOut),
                        SameSite = SameSiteMode.Lax,
                        Domain = domain
                    });
                Logger.Info($"set identity: {userData.TenantGroupId}");
            }


        }

        public static async Task RenewRMIdentityAsync(this HttpResponse response, RMIdentity userData)
        {
            if (userData.SessionFrom > 0 && DateTime.UtcNow.Ticks > (userData.SessionFrom + SessionExtendsTicks))
            {
                SetSessionTimeoutTime(userData);
                
                await SessionManger.RenewAsync(userData, TimeSpan.FromMinutes(userData.SessionOut));
            }
        }

        private static void SetSessionTimeoutTime(RMIdentity userData)
        {
            if (userData.SessionOut <= 0)
            {
                userData.SessionOut = 15;
            }
            userData.ExpiredTime = DateTime.UtcNow.AddMinutes(userData.SessionOut);
        }

        public static async Task<RMIdentity> GetRMIdentityAsync(this HttpRequest request)
        {

            string token = null;
            RMIdentity mIdentity = null;
            if (request.Cookies.ContainsKey(AuthCookie.CookieName))
            {
                token = request.Cookies[AuthCookie.CookieName];
                token = Decode(token);
            }
            if (string.IsNullOrEmpty(token) || token.IndexOf('|') == -1)
            {
                Logger.Debug($"sessionId is null.");
                return null;
            }
            var tokenArr = token.Split('|');
            var sessionId = tokenArr[1];
            var tenantId = tokenArr[0];
            if (Guid.TryParse(sessionId, out Guid sessionGUID))
            {
                TenantLocalValue.LogonGroupId = tenantId;
                
                mIdentity = await SessionManger.GetAsync(sessionGUID);
                if (mIdentity != null)
                {
                    if (!string.IsNullOrEmpty(mIdentity.AccessToken))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var accessToken = handler.ReadJwtToken(mIdentity.AccessToken);
                        var customerId = accessToken?.Claims?.FirstOrDefault(p => p?.Type == "customer_id")?.Value?.ToString();
                        if (customerId != tenantId) 
                        {
                            Logger.Error($"tenant is mismatch, c {tenantId}, t {customerId}.");
                            return null;
                        }
                        SessionManger.CurrentSessionId = mIdentity.SessionId;
                    }
                    else 
                    {
                        TenantLocalValue.LogonGroupId = null;
                        SessionManger.CurrentSessionId = Guid.Empty;
                    }
                    
                }
            }
            else
            {
                Logger.Warn($"invalid session id:{token}");
            }

            return mIdentity;


        }

        public static RMLogonInfo GetRefreshToken(this HttpRequest request)
        {

            string token = null;
            if (request.Cookies.ContainsKey(AuthCookie.CookieName))
            {
                token = request.Cookies[AuthCookie.CookieName];
                token = Decode(token);
            }
            if (string.IsNullOrEmpty(token) || token.IndexOf('|') == -1)
            {
                Logger.Debug($"sessionId is null.");
                return null;
            }
            var tokenArr = token.Split('|');
            var refreshToken = tokenArr[2];

            return new RMLogonInfo("", "",  "", "", refreshToken);


        }

        public async static Task<CurrentUserInfo> GetCurrentUserInfoAsync(this HttpRequest request)
        {
            var identity = await request.GetRMIdentityAsync();
            if (identity != null)
            {
                return ConvertUserInfo(identity);
            }
            return null;
        }

        public static string Encode(string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(dataBytes);
        }

        public static string Decode(string data)
        {
            try
            {
                var dataBytes = Convert.FromBase64String(data);
                return Encoding.UTF8.GetString(dataBytes);

            }
            catch (Exception ex)
            {
                Logger.Warn($"decode error, need relogin:{ex.ToString()}");
                return null;
            }
            
        }

        private static CurrentUserInfo ConvertUserInfo(RMIdentity identity)
        {
            return new CurrentUserInfo()
            {
                AccountId = identity.AccountId,
                AccountNumber = identity.AccountNumber,
                AccountType = identity.AccountType,
                Company = identity.Company,
                DisplayName = identity.DisplayName,
                PartnarUser = identity.PartnerUser,
                PartnarOwner = identity.PartnerOwner,
                TenantGroupId = identity.TenantGroupId,
                LoginName = identity.RegisterEmail,
                RegisterEmail = identity.RegisterEmail,
                SessionId = identity.SessionId.ToString(),
                SessionOut = identity.SessionOut,
                PermissionMark = identity.GPermission,
            };
        }
    }

}
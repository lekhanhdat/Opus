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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.Account;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Tenant;
using Newtonsoft.Json.Linq;

namespace AvePoint.RA.Common.Security
{
    public class RMSSOHelper
    {
        private static RALogger logger = RALogger.GetInstance(typeof(RMSSOHelper));
        public static string SsoClientId => RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_CLIENT_ID];
        public static string SsoServiceUrl => RMGlobalConfiguration.AppConfig[RMAppSettingKey.SSO_SERVICE_URL];
        public static string RecoHostUrl => RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_DOMAIN_URL];
        public static string CurrentDCName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];
        public static string RecoSsoLoginUrl => $"{RecoHostUrl}/sso";
        public static string SsoLoginUrl => $"{SsoServiceUrl}/oauth/authorize?client_id={SsoClientId}&scope=offline_access&redirect_uri={HttpUtility.UrlEncode($"{RecoHostUrl}/Account/LoginForSSO")}";
        public static string AppSSOLoginUrl => $"{SsoServiceUrl}/oauth/authorize?client_id={SsoClientId}&redirect_uri={HttpUtility.UrlEncode($"{RecoHostUrl}/Account/Login2AppSSO")}";
        public static string RefreshToken => $"{SsoServiceUrl}/connect/token?";
        public static string SsoLogoutUrl => $"{SsoServiceUrl}/logout";
        public static string RecoSsoLogoutUrl => $"{RecoHostUrl}/Account/SSOLogout";
        public static string RecoLogoutUrl => $"{RecoHostUrl}/Account/LogOut";
        public static string RecoMultiGeoSettingUrl =>  $"{RecoHostUrl}/Root/CP/Multi/GEOSettings";
        private static string _RECO_SSO_DOMAIN_NAME = "";

        private static HttpClient client = new HttpClient();

        public static string RECO_SSO_DOMAIN_NAME 
        { 
            get 
            {
                if (string.IsNullOrEmpty(_RECO_SSO_DOMAIN_NAME) && !string.IsNullOrEmpty(RecoHostUrl)) 
                {
                    _RECO_SSO_DOMAIN_NAME = new Uri(RecoHostUrl)?.Host;
                }
                return _RECO_SSO_DOMAIN_NAME;
            } 
        }
        /// <summary>
        /// 验证sso返回的token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static bool ValidateSsoToken(string token, string publicKey)
        {
            var tokenHander = new JwtSecurityTokenHandler();
            try
            {
                //Fortify scan: [RECO-20916] Privacy Violation:Heap Inspection add Disposal Class.
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                rsa.FromXmlString(publicKey);
                TokenValidationParameters parameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(3),
                    ValidateAudience = true,
                    ValidAudience = SsoClientId,
                    ValidateIssuer = true,
                    ValidIssuer = SsoServiceUrl,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa)
                };
                tokenHander.ValidateToken(token, parameters, out var securityToken);
                
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"[SsoLogin] validate token failed, error: {e}");
                return false;
            }
        }

        public static string GetBotToken(string token)
        {
            //using (HttpClient client = new())
            //{
                HttpRequestMessage request = new(HttpMethod.Post, $"{SsoServiceUrl}/connect/token");
                var paras = new List<KeyValuePair<string, string>>
                    {
                        new("clientid", SsoClientId),
                        new("granttype", "urn:ietf:params:oauth:grant-type:token-exchange"),
                        new("assertion", token),
                        new("clientassertiontype", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"),
                        new("scope", "https://cloud.app.com/copilot")
                    };
                request.Content = new FormUrlEncodedContent(paras);

                HttpResponseMessage response = client.Send(request);
                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var tokenContent = JsonConvert.DeserializeObject<JObject>(content);
                    return tokenContent.GetValue("access_token").ToString();
                }
                else
                {
                    string error = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    logger.Warn($"GetBotToken error: {error}");
                }
            //}
            return "";
        }

        public static string GetAccessToken(string refresh_token)
        {
            //using (HttpClient client = new HttpClient())
            //{
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{SsoServiceUrl}/connect/token");
                var paras = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("clientid", SsoClientId),
                        new KeyValuePair<string, string>("tenantid", TenantLocalValue.LogonGroupId),
                        new KeyValuePair<string, string>("granttype", "refresh_token"),
                        new KeyValuePair<string, string>("refreshtoken", refresh_token)
                    };
                request.Content = new FormUrlEncodedContent(paras);
                HttpResponseMessage response = client.Send(request);
                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var token=JsonConvert.DeserializeObject<JObject>(content);
                    return token.GetValue("access_token").ToString();
                }
                else
                {
                    logger.Error("Error：" + response.StatusCode);
                    string error = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            //}
            return "";
        }

        /// <summary>
        /// 解析sso返回的token, token中包含identity, loginresult, licenses
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AosSsoTokenInfo AnalysisToken(string token)
        {
            try
            {
                var tokenHander = new JwtSecurityTokenHandler();
                var tokenInfo = tokenHander.ReadJwtToken(token);
                var identityJson = tokenInfo?.Payload?.FirstOrDefault(p => p.Key == "identity").Value?.ToString();
                var resultJson = tokenInfo?.Payload?.FirstOrDefault(p => p.Key == "result").Value?.ToString();
                var licensesJson = tokenInfo?.Payload?.FirstOrDefault(p => p.Key == "licenses").Value?.ToString();
                var aosTokenInfo = new AosSsoTokenInfo
                {
                    IdentityInfo = JsonConvert.DeserializeObject<AosSsoIdentityInfo>(identityJson),
                    Licenses = JsonConvert.DeserializeObject<List<AosSsoLicenseInfo>>(licensesJson),
                    Result = (SSOLoginResultMessage)Enum.Parse(typeof(SSOLoginResultMessage), resultJson, ignoreCase: true)
                };
                logger.Info("[SsoLogin] Analysis Token successful.");
                return aosTokenInfo;
            }
            catch (Exception e)
            {
                logger.Error($"[SsoLogin] Analysis Token, error: {e}");
            }
            return null;
        }
    }
}

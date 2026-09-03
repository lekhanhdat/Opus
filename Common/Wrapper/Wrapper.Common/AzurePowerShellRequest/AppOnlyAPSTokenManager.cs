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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    class AppOnlyAPSTokenManager : IAPSTokenManager
    {
        private static APPOnlyAPSTokenCache tokenCache = new APPOnlyAPSTokenCache(128);
        private static AveLogger logger = AveLogger.GetInstance(typeof(AppOnlyAPSTokenManager), false);
        private AveBPOSAccountInfo account;
        private string customerId;
        private string aosApiUrl;
        private string clientId;
        private string tenantId;
        private AuthenticationResult token;

        public AppOnlyAPSTokenManager(AveBPOSAccountInfo account, string customerId, string aosApiUrl, string clientId)
            : this(account, customerId, null, aosApiUrl, clientId)
        {

        }

        public AppOnlyAPSTokenManager(AveBPOSAccountInfo account, string customerId, string tenantId, string aosApiUrl, string clientId)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            this.account = account;
            this.customerId = customerId;
            this.tenantId = tenantId;
            this.aosApiUrl = aosApiUrl;
            this.clientId = clientId;
        }

        private AuthenticationResult GetToken(string tenantId, string clientId, bool verifyUser, bool throwExceptionIfVerifyFailed, string username)
        {
            var token = GCommon.Utility.Cloud.AppTokenHelper.GetTokenFromCert(tenantId, clientId, "https://graph.windows.net");

            if (verifyUser)
            {
                if (VerifyUserExist(token.AccessToken, username, tenantId, throwExceptionIfVerifyFailed))
                {
                    tokenCache.Add(username, tenantId);

                    return token;
                }
                else if (throwExceptionIfVerifyFailed)
                {
                    throw new Exception(string.Format("The user:{0} is not available in tenant:{1}", username, tenantId));
                }
                else
                {
                    return null;
                }
            }

            return token;
        }

        private AuthenticationResult GetToken()
        {
            logger.Info("start get token with user:{0}, tenant id:{1}, customer id:{2}, aos api url:{3}, client id:{4}", account.UserName, tenantId, customerId, aosApiUrl, clientId);

            string newTenantId;

            if (tokenCache.TryGet(account.UserName, out newTenantId))
            {
                return GCommon.Utility.Cloud.AppTokenHelper.GetTokenFromCert(newTenantId, clientId, "https://graph.windows.net");
            }
            else
            {
                if (!string.IsNullOrEmpty(tenantId))
                {
                    return GetToken(tenantId, clientId, true, true, account.UserName);
                }

                var tenantIds = GCommon.Utility.Cloud.AuthenticationProfileUtility.GetTenantIds(customerId, aosApiUrl);

                foreach (var item in tenantIds)
                {
                    var token = GetToken(item, clientId, true, false, account.UserName);

                    if (token != null)
                    {
                        return token;
                    }
                }

                throw new Exception(
string.Format(@"If the multi-factor authentication is enabled for user:{0}.
Please navigate to AOS to add an app profile with particular Office 365 tenant, 
and make sure the app password is used for DAOL to access SharePoint Online and Exchange Online.
Otherwise, please verify the username and password is correct and the account has enough permission to access the resource.
More details: Office 365 tenant id:{1}, customer id:{2}, client id:{3}, aosApiUrl:{4}
", account.UserName, tenantId, customerId, clientId, aosApiUrl));
            }
        }

        private bool VerifyUserExist(string token, string accountName, string tenantId, bool throwExceptionIfVerifyFailed)
        {
            var user = RetryLogic(3, () => GetUser(token, accountName), string.Format("Get user:{0} with tenant id:{1}", accountName, tenantId), throwExceptionIfVerifyFailed);

            if (user != null)
            {
                Newtonsoft.Json.Linq.JToken accountEnabledValue;
                if (user.TryGetValue("accountEnabled", out accountEnabledValue))
                {
                    return true;
                }

                logger.Warn("Cannot get user:{0} with tenant id:{1}, details:{2}", accountName, tenantId, user["odata.error"]["message"]["value"].ToString());
            }

            return false;
        }

        private T RetryLogic<T>(int times, Func<T> func, string action, bool throwExceptionIfFound)
        {
            var currentTime = 0;

            while (currentTime < times)
            {
                try
                {
                    currentTime++;

                    return func();
                }
                catch (Exception ex)
                {
                    logger.Error("Action:{0} has exception:{1}", action, ex);

                    if (currentTime < times)
                    {
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        if (throwExceptionIfFound)
                        {
                            throw;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return default(T);
        }

        private Newtonsoft.Json.Linq.JObject GetUser(string token, string accountName)
        {
            var url = string.Format("https://graph.windows.net/myorganization/users/{0}?api-version=1.6", accountName);

            var webRequest = WebRequest.CreateHttp(url);
            webRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            webRequest.Accept = "application/json";
            webRequest.Timeout = 3600000;

            WebResponse response = null;

            try
            {
                response = webRequest.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response;
                if (response == null || 
                    string.IsNullOrEmpty(response.ContentType) || 
                    response.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw;
                }
            }

            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd());
            }
        }

        public string Token
        {
            get
            {
                if (token == null || token.ExpiresOn < DateTimeOffset.UtcNow.AddMinutes(-5))
                {
                    token = GetToken();
                }

                return token.AccessToken;
            }
        }

        public APSTokenType TokenType
        {
            get
            {
                return APSTokenType.AppOnlyBearer;
            }
        }

        public override string ToString()
        {
            return account.UserName;
        }
    }
}

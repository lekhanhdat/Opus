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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Services;
using DocAveOnline.WebApi.Contracts;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RADataBroker.Common
{
    public static class ODataUtil
    {
        public static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ODataUtil));
        //public static DateTime LastFailedTime { get; set; } = DateTime.MinValue;
        public static ODataClient AddLoginToken(this ODataClient client, string authenticationHeader)
        {
            Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();
            headers.Add("X_DocAve_Access_Token", new List<string>() { authenticationHeader });
            client.UpdateRequestHeaders(headers);
            return client;
        }

        public static ODataClient AddIdentityServiceToken(this ODataClient client, string userName, string token)
        {
            Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();
            headers.Add("Authorization", new List<string>() { $"Bearer {token}" });
            headers.Add("UserName", new List<string>() { userName });
            headers.Add("Token-Source", new List<string>() { "IdentityServer" });
            client.UpdateRequestHeaders(headers);
            return client;
        }

        public static ODataClient AddBPOSUserInfoHeader(this ODataClient client, string userName, string loginGroupId)
        {
            Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();
            headers.Add("RealConnection", new List<string>() { "{'UserName': '" + userName + "' ,'GroupId': '" + loginGroupId + "'}" });
            client.UpdateRequestHeaders(headers);

            return client;
        }

        public static void AssembleToken(this ODataClient client, DAOTokenInfo info, int retryCount = 0)
        {
            logger.Info($"start to access dao api by {info.TokenType}, {info.CustomerId}");
            switch (info.TokenType)
            {
                case DAOTokenType.DBPO:
                    client.AddBPOSUserInfoHeader(info.UserName, info.CustomerId);
                    break;
                    ///RECO-20916 currently Connect DAO with Identity service, remove unused code.
                //case DAOTokenType.Cert:
                //    var logintoken = CacheService.Get(CacheNamespace.DAOToken, info.CustomerId + info.UserName, () =>
                //    {
                //        var service = new ClientSecurityService();
                //        Login loginInfo = new Login() { GroupId = info.CustomerId, UserName = info.UserName, Signature = service.GetLoginSignature("DocAve", string.Empty) };
                //        var result = client.For<Login>().Set(loginInfo).InsertEntryAsync().Result;
                //        logger.Info($"login dao api by {info.TokenType} success.");
                //        return result.Token;
                //    }, TimeSpan.FromMinutes(40));

                //    client.AddLoginToken(logintoken);
                //    break;
                case DAOTokenType.IdentityService:
                    try
                    {
                        var token = CacheService.Get(CacheNamespace.DAOToken, info.CustomerId + info.UserName + "Token", () =>
                        {
                            RMIdentityServerTokenService tokenService = new RMIdentityServerTokenService(info.IdentityServerAddress, info.IdentityServerClientId, RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
                            var identityServerToken = tokenService.GetIdentityServerToken(info.CustomerId);
                            client.AddIdentityServiceToken(info.UserName, identityServerToken);
                            DAOUser registerParam = new DAOUser();
                            registerParam.TenantGroupId = info.CustomerId;
                            registerParam.UserName = info.UserName;
                            //var resultnew = client.For("User").Action("Register").Set(new { register = registerParam }).ExecuteAsSingleAsync().Result;
                            AutoRegisterUser(client, registerParam);
                            logger.Info($"login dao api by {info.TokenType} success.");
                            return identityServerToken;
                        }, TimeSpan.FromMinutes(40));
                        client.AddIdentityServiceToken(info.UserName, token);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred while access dao api by identity service. ERROR:{ex.ToString()} Retry Count:{retryCount}");
                        //LastFailedTime = DateTime.UtcNow;
                        //info.TokenType = DAOTokenType.Cert;
                        if (retryCount < 1)
                        {
                            AssembleToken(client, info, ++retryCount);
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"not supported tokenType:{info.TokenType}");
            }
        }
        private static void AutoRegisterUser(this ODataClient client, DAOUser registerParam, int retryCount = 0)
        {
            IDictionary<string, object> resultnew = null;
            try
            {
                resultnew = client.For("User").Action("Register").Set(new { register = registerParam }).ExecuteAsSingleAsync().Result;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while auto register user to dao. ERROR:{e.ToString()}, retry count {retryCount}");
                if (retryCount < 3)
                {
                    AutoRegisterUser(client, registerParam, ++retryCount);
                }
                else
                {
                    throw e;
                }
            }
            if (resultnew != null)
            {
                if (resultnew.ContainsKey("__result"))
                {
                    object o = resultnew["__result"];
                    bool result = (bool)o;
                    if (!result && retryCount < 3)
                    {
                        logger.Warn($"Result is false, retry, count {retryCount}");
                        AutoRegisterUser(client, registerParam, ++retryCount);
                    }
                }
                else
                {
                    foreach (var item in resultnew)
                    {
                        logger.Warn($"Key:{item.Key}, Value:{item.Value}");
                    }
                    if (retryCount < 3)
                    {
                        AutoRegisterUser(client, registerParam, ++retryCount);
                    }
                }
            }
        }


//        public static ODataClient CreateODataClient(string baseUri)
//        {
//            var timeout = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.ODATA_CLIENT_TIMEOUT_MINUTES, 120);
//            logger.Info($"ODataClientTimeoutMinutes: {timeout}");
//            clientSetting = new ODataClientSettings(baseUri)
//            {
//                IncludeAnnotationsInResults = false,
//                IgnoreUnmappedProperties = false,
//                RequestTimeout = new TimeSpan(0, timeout, 0),
//                PreferredUpdateMethod = ODataUpdateMethod.Put,
//                OnApplyClientHandler = (httpClientHanlder) =>
//                {
//#if DEBUG
//                    var newHandler = httpClientHanlder as HttpClientHandler;
//                    newHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
//                    {
//                        return true;
//                    };
//#endif
//                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
//                    ServicePointManager.DefaultConnectionLimit = 64;
//                    ServicePointManager.SetTcpKeepAlive(true, 60000, 10000);
//                }
//            };
//            return new ODataClient(clientSetting);
//        }
    }
    public class DAOTokenInfo
    {
        public DAOTokenType TokenType { get; set; }
        public string ApiUrl { get; set; }
        public string UserName { get; set; }
        public string CustomerId { get; set; }
        public string IdentityServerClientId { get; set; }
        public string IdentityServerAddress { get; set; }

    }
    class DAOUser 
    {
        public string TenantGroupId { get; set; }
        public string UserName { get; set; }
    }
    public enum DAOTokenType
    {
        DBPO = 1,
        Cert,
        IdentityService
    }
}

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

using AvePoint.GCommon.Contract.CentralAdmin.Object;
using ExchangeUtility.Graph.SkypeAPI.Settings;
using Microsoft365.Authentication.TokenProvider;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Util.MSAzure;

namespace ExchangeUtility.Graph.SkypeAPI.Auth
{
    public static class SkypeTokenHelper
    {
        public const string Skype_Resource_URL = "https://api.spaces.skype.com";
        public const string UIS_Resoruce_URL = "https://uis.teams.microsoft.com";
        public const string Chatsvcagg_Resoruce_URL = "https://chatsvcagg.teams.microsoft.com";

        public static SkypeTokens ToSkypeTokens(this string json)
        {
            var jObj = JObject.Parse(json);
            return new SkypeTokens(
                jObj["tokens"]["skypeToken"].ToString(),
                DateTime.Now.AddSeconds((long)(jObj["tokens"]["expiresIn"] as JValue).Value),
                jObj["region"].ToString());
        }

        public static async Task<string> GetSkypeAuthEndpointsAsync(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var message = await client.PostAsync(TSSettings.Instance.SkypeAuthEndpointAddressV2, null);
                return await message.Content.ReadAsStringAsync();
            }
        }

        public static AOSTokenAuthObjectV2 ToAosSkypeAuthObject(string tenantId, string userName, IATokenProviderBase tokenProvider)
        {
            return ToAosAuthObject(tenantId, userName, Skype_Resource_URL, tokenProvider);
        }

        public static AOSTokenAuthObjectV2 ToAosAuthObject(string tenantId, string userName, string resourceUrl, IATokenProviderBase tokenProvider)
        {
            return new AOSTokenAuthObjectV2(
                tokenProvider,
                new AuthenticationInfo
                {
                    Resource = resourceUrl,
                    TenantId = tenantId,//"Common",
                    Environment = AzureEnvironment.Worldwide
                },
                new AOSAuthInfo
                {
                    Username = userName,
                    AosTokenType = AvePoint.Application.AosApi.Invoker.AosTokenType.ServiceAccount,
                    GraphTokenType = AvePoint.Application.AosApi.Invoker.GraphTokenType.TeamsSkype,
                },
                null,
                AzureEnvironment.Worldwide
                );
        }
    }

    public class SkypeTokens
    {
        public SkypeTokens(string skypeToken, DateTime expireTime, string region)
        {
            this.ExpireTime = expireTime;
            this.SkypeToken = skypeToken;
            this.Region = region;
        }

        public string SkypeToken { get; private set; }

        public DateTime ExpireTime { get; private set; }

        public string Region { get; private set; }

    }
}
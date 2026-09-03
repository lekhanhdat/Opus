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

namespace ExchangeUtility.Graph.SkypeAPI
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using ExchangeUtility.Graph.SkypeAPI.Auth;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using Microsoft365.Authentication.TokenProvider;

    /// <summary>
    /// 这套API主要使用Skype API(api.spaces.skype.com)更新一些team setting，也是当前MS Teams UI调用的API
    /// 实现这套API的目的，graph team API更新比较慢，对graph API功能的补充
    /// **这套API是undocument API，只能用于开发测试使用，请勿在生产环境上使用，否则会引起不适。** 生产环境请尽量使用graph API操作teams
    /// **这套API是undocument API，只能用于开发测试使用，请勿在生产环境上使用，否则会引起不适。** 生产环境请尽量使用graph API操作teams
    /// **这套API是undocument API，只能用于开发测试使用，请勿在生产环境上使用，否则会引起不适。** 生产环境请尽量使用graph API操作teams
    /// undocument API, only for test&dev, never deploy it into production
    /// </summary>
    public class TeamsSkypeService
    {
        private readonly long secondBeforeTokenExpired = 10 * 60;
        private AppTokenAuthObject skypeAuthObj;

        public Endpoints AuthEndpoints { get; private set; }
        private SkypeTokens skypeToken;
        private string authEndpointsJson;
        private bool loginSucceed = false;

        public bool LoginWithSkypeTokenOnly(string tenantId, string userName, IATokenProviderBase tokenProvider)
        {
            this.skypeAuthObj = SkypeTokenHelper.ToAosSkypeAuthObject(tenantId, userName, tokenProvider);
            RefreshSkypeTokenInternal();
            return loginSucceed;
        }
        private void RefreshSkypeTokenInternal()
        {
            if (this.skypeToken == null || DateTime.Now.AddSeconds(this.secondBeforeTokenExpired) > this.skypeToken.ExpireTime)
            {
                this.authEndpointsJson = SkypeTokenHelper.GetSkypeAuthEndpointsAsync(this.skypeAuthObj.GetAccessToken()).ConfigureAwait(false).GetAwaiter().GetResult();
                this.skypeToken = this.authEndpointsJson.ToSkypeTokens();
                this.AuthEndpoints = new Endpoints(this.authEndpointsJson);
                this.loginSucceed = true;
            }
        }

        private void RefreshToken(TokenType type)
        {
            if (!this.loginSucceed) throw new InvalidOperationException("Login required.");
            if (type.HasFlag(TokenType.XSkype))
            {
                RefreshSkypeTokenInternal();
            }
        }

        /// <summary>
        /// Update space admin settings
        /// </summary>
        /// <param name="threadId">team.InternalId which is same as general channel id</param>
        /// <param name="spaceAdminSettings">setting json string</param>
        public async Task UpdateTeamSettingsAsync(string threadId, string spaceAdminSettings)
        {
            await UpdateThreadPropertiesAsync(threadId, "spaceAdminSettings", spaceAdminSettings);
        }

        /// <summary>
        /// Add channel moderator
        /// </summary>
        /// <param name="threadId">channel id</param>
        /// <param name="userDisplayName">user display name or first\give name</param>
        /// <returns></returns>
        public async Task AddChannelModeratorsAsync(string threadId, string userDisplayName)
        {
            var mri = await GetUserProfileByDisplayName(threadId, userDisplayName);
            string body = AssemblyMemberBody(mri);
            await PostMembersAsync(threadId, body);
        }

        /// <summary>
        /// Get user mri by display name
        /// </summary>
        /// <param name="threadId">channel id</param>
        /// <param name="userDisplayName">user display name or first\give name</param>
        /// <returns></returns>
        public async Task<string> GetUserProfileByDisplayName(string threadId, string userDisplayName)
        {
            var url = $"{this.AuthEndpoints.UserProfileService}/api/v1.0/teams/{threadId}/users/profilesearch?top=100&includeDLs=false&includeBots=false&enableGuest=false";
            using (var client = BuildHttpClientWithSkypeToken(TokenType.BearerSkype))
            {
                var jObj = new JObject() { { "keyword", userDisplayName } };
                using (var response = await client.PostAsync(url, new StringContent(jObj.ToString(), Encoding.UTF8, "application/json")))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JArray.Parse(json);
                    return result[0]["mri"].ToString();
                }
            }
        }

        /// <summary>
        /// update channel settings
        /// </summary>
        /// <param name="threadId">threadId</param>
        /// <param name="channelSettings">setting json</param>
        /// <returns></returns>
        public async Task UpdateChannelSettingsAsync(string threadId, string channelSettings)
        {
            await UpdateThreadPropertiesAsync(threadId, "channelSettings", channelSettings);
        }

        public string InitPrivateChannelSite(string teamIntenalId, string channelid)
        {
            var result = string.Empty;
            var url = $"{this.AuthEndpoints.TeamsAndChannelsService}/beta/teams/{teamIntenalId}/files/documentlibrary?channelId={channelid}";
            var webRequest = BuildHttpWebRequestWithSkypeToken(url);
            using (StreamReader reader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
        private static async Task PutAsync(HttpClient client, string url, string body)
        {
            using (var response = await client.PutAsync(url, new StringContent(body, Encoding.UTF8, "application/json")))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private static async Task PostAsync(HttpClient client, string url, string body)
        {
            using (var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json")))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task UpdateConversationForMe(string threadId, string propertyName, string body)
        {
            var url = $"{this.AuthEndpoints.ChatService}/v1/users/ME/conversations/{threadId}/properties?name={propertyName}";
            using (var client = BuildHttpClientWithSkypeToken(TokenType.XSkype))
            {
                await PutAsync(client, url, body);
            }
        }

        private async Task UpdateThreadPropertiesAsync(string threadId, string propertyName, string body)
        {
            var url = $"{this.AuthEndpoints.ChatService}/v1/threads/{threadId}/properties?name={propertyName}";
            using (var client = BuildHttpClientWithSkypeToken(TokenType.XSkype))
            {
                await PutAsync(client, url, body);
            }
        }

        private async Task PostMembersAsync(string threadId, string body)
        {
            var url = $"{this.AuthEndpoints.ChatService}/v1/threads/{threadId}/members";
            using (var client = BuildHttpClientWithSkypeToken(TokenType.XSkype))
            {
                await PostAsync(client, url, body);
            }
        }

        private static string AssemblyMemberBody(string mri)
        {
            return new JObject()
            {
                {
                    "members", new JArray()
                    {
                        new JObject
                        {
                            { "id", mri },
                            { "isModerator", true },
                        },
                    }
                }
            }.ToString();
        }

        private HttpClient BuildHttpClientWithSkypeToken(TokenType type)
        {
            RefreshToken(TokenType.BearerSkype);
            var client = new HttpClient();
            if (type.HasFlag(TokenType.XSkype))
            {
                client.DefaultRequestHeaders.AddAuthenticationHeader(this.skypeToken);
            }
            if (type.HasFlag(TokenType.BearerSkype))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.skypeAuthObj.GetAccessToken());
            }
            client.DefaultRequestHeaders.Add("x-ms-client-type", "web");
            return client;
        }

        private HttpWebRequest BuildHttpWebRequestWithSkypeToken(string url)
        {
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "PUT";
            webRequest.ContentType = "application/json";
            //webRequest.UserAgent = @"TeamsCmdlet/0.9.3/Get-Team";
            webRequest.Headers["Authorization"] = $"Bearer {skypeAuthObj.GetAccessToken()}";
            webRequest.Headers["X-Skypetoken"] = skypeToken.SkypeToken;
            webRequest.ContentLength = 0;
            return webRequest;
        }

        public class Endpoints
        {
            /// <summary>
            /// 不要用region拼接endpoint URL，可能不准确，region相关的endpoint需要从regionGtms中取
            /// </summary>
            public string Region { get; private set; }

            //https://teams.microsoft.com/fabric/apac/templates/api
            public string TeamsAndChannelsProvisioningService { get; private set; }

            // https://teams.microsoft.com/api/mt/apac
            public string TeamsAndChannelsService { get; private set; }

            //https://apac.ng.msg.teams.microsoft.com
            public string ChatService { get; private set; }

            //https://teams.microsoft.com/api/userprofilesvc/apac
            public string UserProfileService { get; set; }
            //https://teams.microsoft.com/api/nss/apac
            public string UserIntelligenceService { get; set; }

            public Endpoints(string endpointJson)
            {
                var json = JObject.Parse(endpointJson);
                this.Region = json["region"].ToString();
                InitRegionGtms(json["regionGtms"]);
            }

            private void InitRegionGtms(JToken gtms)
            {
                this.TeamsAndChannelsProvisioningService = GetUrlProperty(gtms, "teamsAndChannelsProvisioningService");
                this.TeamsAndChannelsService = GetUrlProperty(gtms, "teamsAndChannelsService");
                this.ChatService = GetUrlProperty(gtms, "chatService");
                this.UserProfileService = GetUrlProperty(gtms, "userProfileService");
                this.UserIntelligenceService = GetUrlProperty(gtms, "userIntelligenceService");
            }

            private string GetUrlProperty(JToken parent, string propertyName)
            {
                return parent[propertyName].ToString().TrimEnd('/');
            }
        }

        [Flags]
        enum TokenType
        {
            None = 0,
            //Bearer token
            BearerSkype = 1,
            BearerChatsvcagg = 2,
            BearerUIS = 4,
            //other token
            XSkype = 256,
        }
    }
}
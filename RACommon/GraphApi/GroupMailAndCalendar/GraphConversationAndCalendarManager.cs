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
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.Common.Util;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Data.Nexus.Foundation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Duende.IdentityModel.Client;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Graph.Models;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.GroupMailAndCalendar
{
    public class GraphConversationAndCalendarManager : RMGraphApiManager
    {
        private const int DelegateAppUserPermissionCheckRetryTimes = 60;
        private static readonly TimeSpan DelegateAppUserPermissionCheckInterval = TimeSpan.FromSeconds(5);
        public bool MailEnabled { get; set; }
        private string GroupId { get; set; }
        private string ConversationsNextLink { get; set; }
        private string CalendarEventNextLink { get; set; }
        public GraphConversationAndCalendarManager(string o365TenantId, string mail,string groupId) : base(o365TenantId,true)
        {
            //var group = GetGroupByIdAsync(groupId).GetAwaiter().GetResult();
            this.MailEnabled = true;// = bool.Parse(group.MailEnabled.ToString());
            this.GroupId = groupId;
        }
        public async Task<RMGroup> GetGroupByMailAsync(string mail)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/?$filter=mail eq '{mail}'";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ListGroupsObj>(resultJson);
            return result.Value[0];
        }
        public async Task<GCommon.GraphAPI.GraphUser> GetUserByMailAsync(string mail)
        {
            var escapedMail = EscapeODataString(mail);
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users/?$filter=mail eq '{escapedMail}' or userPrincipalName eq '{escapedMail}'";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<GCommon.GraphAPI.ListGraphUserObj>(resultJson);
            return result.Value?.FirstOrDefault();
        }
        public async Task<RMGroup> GetGroupByIdAsync(string groupId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{groupId}";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<RMGroup>(resultJson);
            return result;
        }
        public async Task<string> GetGroupOwnersAsync()
        {
            try
            {
                var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/owners";
                var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
                var result = JsonConvert.DeserializeObject<ListGroupOwnersObj>(resultJson);
                return result.Value[0].Mail;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        public async Task<List<GroupConversation>> GetConversationsAsync(int skipSize,int pageSize = 100)
        {
            string requestUri = string.Empty;
            if (string.IsNullOrEmpty(ConversationsNextLink))
            {
                requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations?$top={pageSize}&$skip={skipSize}";
            }
            else
            {
                requestUri = ConversationsNextLink;
            }
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ListGroupsConversationsObj>(resultJson);
            ConversationsNextLink = result.OdataNextLink;
            return result.Value?.ToList();
        }
        public async Task<List<ThreadPost>> GetThreadPostsByConversationIdAsync(string conversationId)
        {
            string requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations/{conversationId}/threads";
            var resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
            var result = JsonConvert.DeserializeObject<ListConversationsThreadObj>(resultJson);
            var threads = result.Value?.ToList();
            while(!string.IsNullOrEmpty(result.OdataNextLink))
            {
                requestUri = result.OdataNextLink;
                resultJson = await HttpHelper.GetAsync(requestUri, AccessToken);
                result = JsonConvert.DeserializeObject<ListConversationsThreadObj>(resultJson);
                threads.AddRange(result.Value?.ToList());
            }
            List<ThreadPost> threadPostsResult = new List<ThreadPost>();
            foreach (var trd in threads)
            {
                var threadRequestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations/{conversationId}/threads/{trd.Id}/posts";
                var threadResultJson = await HttpHelper.GetAsync(threadRequestUri, AccessToken);
                var threadResult = JsonConvert.DeserializeObject<ListThreadPostObj>(threadResultJson);
                threadPostsResult.AddRange(threadResult.Value);
                foreach (var a in threadResult.Value)
                {
                    a.ConversationId = conversationId;
                    a.ThreadId = trd.Id;
                }
                while (!string.IsNullOrEmpty(threadResult.OdataNextLink))
                {
                    threadRequestUri = threadResult.OdataNextLink;
                    threadResultJson = await HttpHelper.GetAsync(threadRequestUri, AccessToken);
                    threadResult = JsonConvert.DeserializeObject<ListThreadPostObj>(threadResultJson);
                    threadPostsResult.AddRange(threadResult.Value);
                    foreach (var a in threadResult.Value)
                    {
                        a.ConversationId = conversationId;
                        a.ThreadId = trd.Id;
                    }
                }
            }
            return threadPostsResult;
        }
        public async Task<List<PostAttachment>> GetAttachmentByThreadPostAsync(ThreadPost post)
        {
            string requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations/{post.ConversationId}/threads/{post.ThreadId}/posts/{post.Id}/attachments";

            var resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);

            var result = JsonConvert.DeserializeObject<ListPostAttachmentObj>(resultJson);
            List<PostAttachment> postAttachments = new List<PostAttachment>();
            postAttachments.AddRange(result.Value);
            while (!string.IsNullOrEmpty(result.OdataNextLink))
            {
                requestUri = result.OdataNextLink;
                resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);
                result = JsonConvert.DeserializeObject<ListPostAttachmentObj>(resultJson);
                postAttachments.AddRange(result.Value);
            }
            return postAttachments;
        }
        public async Task<List<PostAttachment>> GetCalendarEventAttachmentByEventAsync(GroupCalendarEvent calendarEvent)
        {
            string requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/calendar/events/{calendarEvent.Id}/attachments";
            var resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);
            var result = JsonConvert.DeserializeObject<ListPostAttachmentObj>(resultJson);
            List<PostAttachment> postAttachments = new List<PostAttachment>();
            postAttachments.AddRange(result.Value);
            while (!string.IsNullOrEmpty(result.OdataNextLink))
            {
                requestUri = result.OdataNextLink;
                resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);
                result = JsonConvert.DeserializeObject<ListPostAttachmentObj>(resultJson);
                postAttachments.AddRange(result.Value);
            }
            return postAttachments;
        }
        public async Task<GroupCalendar> GetCalendarAsync()
        {
            string requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/calendar";

            var resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);

            var result = JsonConvert.DeserializeObject<GroupCalendar>(resultJson);
            return result;
        }
        public async Task<List<GroupCalendarEvent>> GetCalendarEventsAsync(int skipSize, int pageSize = 100)
        {
            string requestUri = string.Empty;
            if (string.IsNullOrEmpty(CalendarEventNextLink))
            {
                requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/calendar/events?$top={pageSize}&$skip={skipSize}";
            }
            else
            {
                requestUri = CalendarEventNextLink;
            }

            var resultJson = await HttpHelper.GetAsync(requestUri, DelegateAccessToken);

            var result = JsonConvert.DeserializeObject<ListCalendarEventObj>(resultJson);
            CalendarEventNextLink = result.OdataNextLink;
            return result.Value?.ToList();
        }
        public void DeleteConversationById(KeyValuePair<string,List<string>> keyValue)
        {
            string requestUri = string.Empty;
            foreach (string threadId in keyValue.Value)
            {
                requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations/{keyValue.Key}/threads/{threadId}";
                try
                {
                    HttpHelper.Delete(requestUri, AccessToken);
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to delete conversation thread with application token. Retrying with delegated token, group id:{GroupId}, conversation id:{keyValue.Key}, thread id:{threadId}, error:{ex.Message}");
                    HttpHelper.Delete(requestUri, DelegateAccessToken);
                }
            }
        }
        public void DeleteEventById(string eventId)
        {
            string requestUri = string.Empty;
            requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/calendar/events/{eventId}";
            HttpHelper.Delete(requestUri, DelegateAccessToken);
        }

        public bool EnsureDelegateAppUserAsMemberForPrivateTeam(bool waitUserAvailable, out string delegateAppUserId)
        {
            delegateAppUserId = string.Empty;
            if (!IsPrivateTeam())
            {
                return false;
            }

            var isTeam = IsTeam();
            var delegateAppUser = GetDelegateAppUser();
            delegateAppUserId = delegateAppUser.Id;
            if (ExistMember(delegateAppUser.Id).GetAwaiter().GetResult())
            {
                logger.Info($"Delegate app user already is private Team member, group id:{GroupId}, user id:{delegateAppUser.Id}.");
                return false;
            }

            logger.Info($"Add delegate app user to private Team members, group id:{GroupId}, user id:{delegateAppUser.Id}.");
            AddMember(delegateAppUser.Id, isTeam);
            if(waitUserAvailable)
            {
                WaitDelegateAppUserCanAccessConversationData(delegateAppUser.Id);
            }
            return true;
        }

        public void RemoveDelegateAppUserFromTeamMembers(string delegateAppUserId)
        {
            if (string.IsNullOrWhiteSpace(delegateAppUserId))
            {
                logger.Warn($"Skip remove delegate app user from private Team members because user id is empty, group id:{GroupId}.");
                return;
            }

            RemoveMember(delegateAppUserId);
            logger.Info($"Remove delegate app user from private Team members, group id:{GroupId}, user id:{delegateAppUserId}.");
        }

        public async Task<bool> ExistMember(string userIdOrUpn)
        {
            var encodedUserIdOrUpn = Uri.EscapeDataString(userIdOrUpn);
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{encodedUserIdOrUpn}/checkMemberGroups";
            var requestBody = JsonConvert.SerializeObject(new { groupIds = new List<string> { GroupId } });
            var resultJson = await HttpHelper.PostAsync(requestUri, requestBody, AccessToken);
            var result = JsonConvert.DeserializeObject<CheckMemberGroupsResponse>(resultJson);
            return result?.Value?.Contains(GroupId, StringComparer.OrdinalIgnoreCase) == true;
        }

        private bool IsPrivateTeam()
        {
            var group = GetGroupByIdAsync(GroupId).GetAwaiter().GetResult();
            return group?.Visibility?.Equals("Private", StringComparison.OrdinalIgnoreCase) == true;
        }

        private bool IsTeam()
        {
            try
            {
                var requestUri = $"{GraphEndPoint}/{ApiVersion}/teams/{GroupId}?$select=id";
                HttpHelper.Get(requestUri, AccessToken);
                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                logger.Info($"Current group is not a Team, group id:{GroupId}.");
                return false;
            }
        }

        private GCommon.GraphAPI.GraphUser GetDelegateAppUser()
        {
            var delegateAppUserName = Profile?.AuthorizationUserName;
            if (string.IsNullOrWhiteSpace(delegateAppUserName))
            {
                logger.Warn($"Cannot get delegate app authorization user name from profile, profile id:{Profile?.Id}. Try to get delegate app user by Graph me.");
                return GetDelegateAppUserByMe();
            }

            logger.Info($"Get user info by user mail: {delegateAppUserName}");
            var delegateAppUser = GetUserByMailAsync(delegateAppUserName).GetAwaiter().GetResult();
            if (delegateAppUser == null || string.IsNullOrWhiteSpace(delegateAppUser.Id))
            {
                throw new Exception($"Cannot get delegate app user by authorization user name:{delegateAppUserName}.");
            }

            return delegateAppUser;
        }

        private GCommon.GraphAPI.GraphUser GetDelegateAppUserByMe()
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/me";
            var resultJson = HttpHelper.Get(requestUri, DelegateAccessToken);
            var delegateAppUser = JsonConvert.DeserializeObject<GCommon.GraphAPI.GraphUser>(resultJson);
            if (delegateAppUser == null || string.IsNullOrWhiteSpace(delegateAppUser.Id))
            {
                throw new Exception("Cannot get delegate app user by Graph me.");
            }

            return delegateAppUser;
        }

        private void AddMember(string userId, bool isTeam)
        {
            if (!isTeam)
            {
                AddGroupMember(userId);
                return;
            }

            var requestUri = $"{GraphEndPoint}/{ApiVersion}/teams/{GroupId}/members";
            var requestBody = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "@odata.type", "#microsoft.graph.aadUserConversationMember" },
                { "roles", new List<string>() },
                { "user@odata.bind", $"{GraphEndPoint}/{ApiVersion}/users('{userId}')" }
            });
            try
            {
                HttpHelper.Post(requestUri, requestBody, AccessToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                logger.Warn($"Add delegate app user to Team members failed because Team was not found. Add user to group members instead, group id:{GroupId}, user id:{userId}.");
                AddGroupMember(userId);
            }
        }

        private void AddGroupMember(string userId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/members/$ref";
            var requestBody = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "@odata.id", $"{GraphEndPoint}/{ApiVersion}/directoryObjects/{userId}" }
            });
            HttpHelper.Post(requestUri, requestBody, AccessToken);
        }

        public void WaitDelegateAppUserCanAccessConversationData(string delegateAppUserId)
        {
            for (int retryIndex = 1; retryIndex <= DelegateAppUserPermissionCheckRetryTimes; retryIndex++)
            {
                try
                {
                    CheckDelegateAppUserCanAccessConversationData();
                    logger.Info($"Delegate app user can access group conversation data, group id:{GroupId}, user id:{delegateAppUserId}.");
                    return;
                }
                catch (Exception ex)
                {
                    if (retryIndex >= DelegateAppUserPermissionCheckRetryTimes)
                    {
                        throw new Exception($"Delegate app user cannot access group conversation data after added to private Team members, group id:{GroupId}, user id:{delegateAppUserId}.", ex);
                    }

                    logger.Warn($"Delegate app user cannot access group conversation data after added to private Team members, group id:{GroupId}, user id:{delegateAppUserId}, retry:{retryIndex}/{DelegateAppUserPermissionCheckRetryTimes}, error:{ex}.");
                    Task.Delay(DelegateAppUserPermissionCheckInterval).GetAwaiter().GetResult();
                }
            }
        }

        private void CheckDelegateAppUserCanAccessConversationData()
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/conversations?$top=1";
            HttpHelper.Get(requestUri, DelegateAccessToken);
        }

        private void RemoveMember(string userId)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/groups/{GroupId}/members/{userId}/$ref";
            HttpHelper.Delete(requestUri, AccessToken);
        }

        private static string EscapeODataString(string value)
        {
            return value?.Replace("'", "''");
        }
    }
}

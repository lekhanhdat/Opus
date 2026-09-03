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


namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    //using Polly;

    public partial class MicrosoftGraphAPIService
    {
        public TeamObj GetTeam(string groupId)
        {
            return new GetTeam(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }

        public Channel GetTeamPrimaryChannel(string groupId)
        {
            return new GetTeamPrimaryChannel(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }

        public IList<Channel> ListChannels(string groupId)
        {
            try
            {
                //v1.0 version cannot return shared channel type correctly
                return new ListChannels(resourceUrl, refreshAccessToken, groupId, RetryController, false).GetApiResult();
            }
            catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.BadGateway)
            {
                return new ListChannels(resourceUrl, refreshAccessToken, groupId, RetryController, true).GetApiResult();
            }
        }
        public IList<Channel> ListIncomingChannels(string groupId)
        {
            try
            {
                //v1.0 version cannot return shared channel type correctly
                return new ListIncomingChannels(resourceUrl, refreshAccessToken, groupId, RetryController, false).GetApiResult();
            }
            catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.BadGateway)
            {
                return new ListIncomingChannels(resourceUrl, refreshAccessToken, groupId, RetryController, true).GetApiResult();
            }
        }
        public IList<Channel> ListAllChannels(string groupId)
        {
            try
            {
                //v1.0 version cannot return shared channel type correctly
                return new ListAllChannels(resourceUrl, refreshAccessToken, groupId, RetryController, false).GetApiResult();
            }
            catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.BadGateway)
            {
                return new ListAllChannels(resourceUrl, refreshAccessToken, groupId, RetryController, true).GetApiResult();
            }
        }

        public void UpdateTeam(string groupId, TeamObj team)
        {
            new UpdateTeam(this.resourceUrl, this.refreshAccessToken, groupId, team, this.RetryController).GetApiResult();
        }

        public void RemoveChannel(string groupId, string channelId)
        {
            new DeleteTeamsChannel(this.resourceUrl, this.refreshAccessToken, groupId, channelId, this.RetryController).GetApiResult();
        }

        public Channel CreateChannel(string groupId, Channel channel)
        {
            return new CreateChannel(this.resourceUrl, this.refreshAccessToken, groupId, channel, this.RetryController).GetApiResult();
        }
        public Channel CreatePrivateChannel(string groupId, Channel channel)
        {
            return new CreatePrivateChannel(this.resourceUrl, this.refreshAccessToken, groupId, channel, this.RetryController).GetApiResult();
        }

        public void UpdateChannel(string groupId, Channel channel)
        {
            new UpdateChannel(this.resourceUrl, this.refreshAccessToken, groupId, channel, this.RetryController).GetApiResult();
        }

        /// <summary>
        /// enable team on an existing group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public TeamObj CreateTeam(string groupId)
        {
            return new CreateTeam(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupToCreate"></param>
        /// <param name="ownerName">add user as team owner, input null to skip add owner, note for app only token, you must input a valid user here.</param>
        /// <returns></returns>
        public Tuple<Group, TeamObj> CreateTeam(Group groupToCreate, string ownerId)
        {
            Group g = null;
            TeamObj t = null;
            try
            {
                g = CreateUnifiedGroup(groupToCreate);
                Logger.Info($"CreateTeam successful.");
                //app only token, have to add owner before create team.
                if (!string.IsNullOrEmpty(ownerId))
                {
                    //In order to create a team, the group must have a least one owner.
                    var user = GetUser(ownerId);
                    int retryAddGroupOwner = 0;
                    while (retryAddGroupOwner < 5)
                    {
                        try
                        {
                            //delegate permission token will add owner while creating group, app only token will not
                            AddGroupOwner(g.Id, user.Id, false);
                            Logger.Info($"CreateTeam AddGroupOwner successful.GroupID:{g.Id}.UserID:{user.Id}.RetryTime:{retryAddGroupOwner}.");
                            break;
                        }
                        catch (GraphAPIException ex) when (ex.HttpStatusCode  == HttpStatusCode.NotFound)
                        {
                            retryAddGroupOwner++;
                            Logger.Error($"Error occured while AddGroupOwner. Error message: {ex.Message}.RetryTime:{retryAddGroupOwner}.");
                            Thread.Sleep(6000); // 等待 5 秒再试
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error occured while AddGroupOwner. Error message: {e.Message}");
                            throw;
                        }
                    }

                    int retryAddGroupMember = 0;
                    while (retryAddGroupMember < 5)
                    {
                        try
                        {
                            AddGroupMember(g.Id, user.Id);
                            Logger.Info($"CreateTeam AddGroupMember successful.GroupID:{g.Id}.UserID:{user.Id}.RetryTime:{retryAddGroupMember}.");
                            break;
                        }
                        catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
                        {
                            retryAddGroupMember++;
                            Logger.Error($"Error occured while AddGroupMember. Error message: {ex.Message}.RetryTime:{retryAddGroupMember}.");
                            Thread.Sleep(6000); // 等待 5 秒再试
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error occured while AddGroupMember. Error message: {e.Message}");
                            throw;
                        }
                    }
                    
                }
                //https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/api/team_put_teams
                //If the group was created less than 15 minutes ago, it's possible for the Create team call to fail with a 404 error code due to replication delays. 
                //The recommended pattern is to retry the Create team call three times, with a 10 second delay between calls.
                t = CreateTeamWithRetry(g.Id, 3);

                return new Tuple<Group, TeamObj>(g, t);
            }
            catch
            {
                if (t == null && g != null)
                {
                    Rollback(g);
                }
                throw;
            }
        }

        //optimize the logic for AOSBR-20278 and merge this request into create group so that reduce the number of requests in the future.
        private void AddGroupOwnerWithRetry(string groupId, string userId, int retry = 5, bool addAsMember = true)
        {
            int delayMs = 5000;
            for (int i = 1; i <= retry; ++i)
            {
                try
                {
                    AddGroupOwner(groupId, userId, addAsMember);
                    return;
                }
                catch (GraphAPIException ex)
                {
                    if (!ex.Message.Contains("does not exist or one of its queried reference-property objects are not present") || i == retry) throw;
                    Thread.Sleep(delayMs);
                    delayMs *= 2;
                    Logger.Warn($"Group owner is not added successfully, retrying... Attempt: {i}, GroupId: {groupId}, UserId: {userId}");
                }
            }
        }

        private void Rollback(Group g)
        {
            try
            {
                DeleteGroup(g.Id);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to delete group. Error message : {e.Message}");
            }
        }

        private TeamObj CreateTeamWithRetry(string id, int retry)
        {
            for (int i = 1; i <= retry; ++i)
            {
                try
                {
                    return CreateTeam(id);
                }
                catch (GraphAPIException ex) //when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (i == retry) throw;
                    Thread.Sleep(20000);
                }
            }
            throw new InvalidOperationException("Unreachable code.");
        }

        public IList<InstalledApp> ListInstalledApps(string groupId)
        {
            return new ListInstalledApps(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }

        public IList<CatalogTeamsApp> ListCatalogTeamsApps()
        {
            return new ListCatalogApps(this.resourceUrl, this.refreshAccessToken, this.RetryController).GetApiResult();
        }

        public void AddTeamsApp(string groupId, TeamsAppObj teamsAppObj)
        {
            new AddTeamsApp(this.resourceUrl, this.refreshAccessToken, groupId, teamsAppObj, this.RetryController).GetApiResult();
        }

        public void DeleteTeamsApp(string groupId, string teamsAppId)
        {
            new DeleteTeamsApp(this.resourceUrl, this.refreshAccessToken, groupId, teamsAppId, this.RetryController).GetApiResult();
        }

        public IList<Tab> ListChannelTabs(string groupId, string channelId)
        {
            return new ListChannelTabs(this.resourceUrl, this.refreshAccessToken, groupId, channelId, this.RetryController).GetApiResult();
        }

        public IList<Tab> GetChatTabs(string chatId)
        {
            return new GetChatTabs(resourceUrl, refreshAccessToken, RetryController, chatId).GetApiResult();
        }

        public Tab GetChannelTab(string groupId, string channelId, string tabId)
        {
            return new GetChannelTab(this.resourceUrl, this.refreshAccessToken, groupId, channelId, tabId, this.RetryController).GetApiResult();
        }

        public Tab AddChannelTab(string groupId, string channelId, TabAddObj tab)
        {
            return new AddChannelTab(this.resourceUrl, this.refreshAccessToken, groupId, channelId, tab, this.RetryController).GetApiResult();
        }

        public void UpdateChannelTab(string groupId, string channelId, TabUpdateObj tab)
        {
            new UpdateTab(this.resourceUrl, this.refreshAccessToken, groupId, channelId, tab, this.RetryController).GetApiResult();
        }

        public void DeleteTeamsTab(string groupId, string channelId, string tabId)
        {
            new DeleteTeamsTab(this.resourceUrl, this.refreshAccessToken, groupId, channelId, tabId, this.RetryController).GetApiResult();
        }

        public void CompleteChannelMigration(string teamsId, string channelId)
        {
            new CompleteChannelMigration(this.resourceUrl, this.refreshAccessToken, teamsId, channelId, this.RetryController).GetApiResult();
        }

        public void CompleteTeamsMigration(string teamsId)
        {
            new CompleteTeamsMigration(this.resourceUrl, this.refreshAccessToken, teamsId, this.RetryController).GetApiResult();
        }

        #region Channel Member

        public List<Member> GetTeamMembers(string groupId, string email = null)
        {
            var result = new GetTeamMembers(resourceUrl, refreshAccessToken, groupId, RetryController);
            if (!string.IsNullOrEmpty(email))
            {
                result.QueryParameters.Filter($"(microsoft.graph.aadUserConversationMember/email eq '{email}')");
            }
            return result.GetApiResult().ToList();
        }

        public List<Member> ListChannelMembers(string groupId, string channelId)
        {
            return new ListChannelMembers(this.resourceUrl, this.refreshAccessToken, groupId, channelId, this.RetryController).GetApiResult().ToList();
        }

        public Member AddTeamMember(string groupId, OTJChannelMember member) => new AddTeamMember(resourceUrl, refreshAccessToken, groupId, member, RetryController).GetApiResult();

        public Member AddChannelMember(string groupId, string channelId, OTJChannelMember member) => new AddChannelMember(resourceUrl, refreshAccessToken, groupId, channelId, member, RetryController).GetApiResult();

        public void RemoveTeamMember(string teamId, string membershipId) => new RemoveTeamMember(resourceUrl, refreshAccessToken, RetryController, teamId, membershipId).GetApiResult();

        public void RemoveChannelMember(string teamId, string channelId, string membershipId) => new RemoveChannelMember(resourceUrl, refreshAccessToken, RetryController, teamId, channelId, membershipId).GetApiResult();

        /// <summary>
        /// Update Channel Member Roles.
        /// </summary>
        /// <param name="memberId">Convert.ToBase64String(Encoding.UTF8.GetBytes($"{channelId}##{userId}"));</param>
        /// <param name="roles">string[]{"owner"} or strring[]{}</param>
        public void UpdateChannelMemberRoles(string groupId, string channelId, string memberId, string[] roles)
        {
            new UpdateChannelMemberRoles(this.resourceUrl, this.refreshAccessToken, groupId, channelId, memberId, new OTJChannelMember { ODataType = "#microsoft.graph.aadUserConversationMember", Roles = roles }, this.RetryController).GetApiResult();
        }
        #endregion

        #region Channel Message

        public List<ChatMessage> ListChannelAllMessages(string groupId, string channelId)
        {
            return new ListChannelAllMessages(resourceUrl, refreshAccessToken, groupId, channelId, RetryController, useBeta).GetApiResult().ToList();
        }

        public ChannelMessageCollection ListChannelMessages(string groupId, string channelId, int pageSize, string skipToken = null)
        {
            var queryString = string.Empty;

            if (!string.IsNullOrEmpty(skipToken))
            {
                if (skipToken.Contains("?"))
                {
                    queryString = skipToken.Substring(skipToken.IndexOf("?"));
                }
                else throw new NotSupportedException($"Unsupported skipToken: {skipToken}");
            }
            else
            {
                queryString = pageSize > 0 ? $"?$top={pageSize}" : string.Empty;
            }

            return new ListChannelMessages(resourceUrl, refreshAccessToken, groupId, channelId, queryString, RetryController, useBeta).GetApiResult();
        }

        public ChannelMessageCollection ListChannelMessages(string teamId, string channelId, string skipToken, string[] queryParams)
        {
            var queryString = string.Empty;

            if (!string.IsNullOrEmpty(skipToken))
            {
                if (skipToken.Contains("?"))
                {
                    queryString = skipToken.Substring(skipToken.IndexOf("?"));
                }
                else throw new NotSupportedException($"Unsupported skipToken: {skipToken}");
            }
            else
            {
                if (queryParams?.Any() ?? false)
                {
                    queryString = $"?{String.Join('&', queryParams)}";
                }
            }
            return new ListChannelMessages(resourceUrl, refreshAccessToken, teamId, channelId, queryString, RetryController, useBeta).GetApiResult();
        }

        public ChannelMessageCollection QueryChannelMessagesDelta(string groupId, string channelId, string queryToken)
        {
            var queryString = String.Empty;
            if (!string.IsNullOrEmpty(queryToken))
            {
                if (queryToken.Contains("?$skiptoken="))
                {
                    queryString = queryToken.Substring(queryToken.IndexOf("?$skiptoken="));
                }
                else if (queryToken.Contains("?$deltatoken="))
                {
                    queryString = queryToken.Substring(queryToken.IndexOf("?$deltatoken="));
                }
                else if (queryToken.StartsWith("?$"))
                {
                    queryString = queryToken;
                }
            }

            return new QueryChannelMessagesDelta(resourceUrl, refreshAccessToken, groupId, channelId, RetryController, useBeta, queryString).GetApiResult();
        }

        public ChatMessage GetChannelMessage(string groupId, string channelId, string messageId)
        {
            return new GetChannelMessage(resourceUrl, refreshAccessToken, groupId, channelId, messageId, RetryController, useBeta).GetApiResult();
        }

        public ChannelMessageReplyCollection ListChannelMessageReplies(string groupId, string channelId, string messageId, int pageSize, string skipToken = null)
        {
            var queryString = string.Empty;

            if (!string.IsNullOrEmpty(skipToken))
            {
                if (skipToken.Contains("?"))
                {
                    queryString = skipToken.Substring(skipToken.IndexOf("?"));
                }
                else throw new NotSupportedException($"Unsupported skipToken: {skipToken}");
            }
            else
            {
                queryString = pageSize > 0 ? $"?$top={pageSize}" : string.Empty;
            }

            return new ListChannelMessageReplies(resourceUrl, refreshAccessToken, groupId, channelId, messageId, queryString, RetryController, useBeta).GetApiResult();
        }

        public List<ChatMessage> ListChannelMessageReplies(string groupId, string channelId, string messageId)
        {
            return new ListChannelMessageAllReplies(resourceUrl, refreshAccessToken, groupId, channelId, messageId, RetryController, useBeta).GetApiResult().ToList();
        }

        public ChatMessage GetChannelMessageReply(string groupId, string channelId, string messageId, string replyId)
        {
            return new GetChannelMessageReply(resourceUrl, refreshAccessToken, groupId, channelId, messageId, replyId, RetryController, useBeta).GetApiResult();
        }

        public ChatMessage SendChannelMessage(string groupId, string channelId, ChatMessage chatMessage)
        {
            return new SendChannelMessage(this.resourceUrl, this.refreshAccessToken, groupId, channelId, chatMessage, this.RetryController).GetApiResult();
        }
        public ChatMessage ReplyChannelMessage(string groupId, string channelId, string messageId, ChatMessage chatMessage)
        {
            return new ReplyChannelMessage(this.resourceUrl, this.refreshAccessToken, groupId, channelId, messageId, chatMessage, this.RetryController).GetApiResult();
        }

        #endregion

        /// <summary> The shouldSetSpoSiteReadOnlyForMembers parameter is not supported in the application context. </summary>
        public void ArchiveTeam(string groupId, bool makeSiteReadOnly)
        {
            TeamsArchiveObj teamsArchiveObj = new TeamsArchiveObj
            {
                ShouldSetSpoSiteReadOnlyForMembers = makeSiteReadOnly
            };
            new ArchiveTeam(this.ResourceUrl, this.refreshAccessToken, groupId, teamsArchiveObj, this.RetryController).GetApiResult();
        }
        public void UnarchiveTeam(string groupId)
        {
            new UnarchiveTeam(this.ResourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }
        public Channel GetSharedChannel(string groupId, string channelName)
        {
            return new FilterChannels(this.ResourceUrl, this.refreshAccessToken, groupId, $"displayName eq '{channelName}' and membershipType eq 'shared'", this.RetryController, true).GetApiResult().FirstOrDefault();
        }
        public Channel GetPrivateChannel(string groupId, string channelName)
        {
            return new GetPrivateChannel(this.ResourceUrl, this.refreshAccessToken, groupId, channelName, this.RetryController).GetApiResult().FirstOrDefault();
        }

        public PrivateChannelSite GetChannelFilesFolder(string groupId, string channelId)
        {
            return new GetChannelFilesFolder(this.resourceUrl, this.refreshAccessToken, groupId, channelId, this.RetryController).GetApiResult();
        }

        public DriveObj GetRecordingDrive(string groupId, string driveId)
        {
            return new GetRecordingDrive(this.resourceUrl, this.refreshAccessToken, groupId, driveId, this.RetryController).GetApiResult();
        }

        public IEnumerable<ChatMessage> GetChatMessages(string userId, string startTime, string endTime, string? model, int? top)
        {
            var request = new GetChatMessages(resourceUrl, refreshAccessToken, RetryController, userId);
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                request.QueryParameters.Filter($"lastModifiedDateTime gt {startTime} and lastModifiedDateTime lt {endTime}");
            }
            if (!string.IsNullOrEmpty(model))
            {
                request.QueryParameters.Model(model);
            }
            if (top.HasValue)
            {
                request.QueryParameters.Top(top.Value);
            }
            request.QueryParameters.OrderBy("lastModifiedDateTime desc");
            return request.GetApiResult();
        }

        public IEnumerable<ChatMessage> GetChatMessagesInChat(string userId, string chatId, string startTime, string endTime, int? top, bool isGcc = false)
        {
            var request = new GetChatMessagesInChat(resourceUrl, refreshAccessToken, RetryController, userId, chatId);
            if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
            {
                request.QueryParameters.Filter($"lastModifiedDateTime gt {startTime} and lastModifiedDateTime lt {endTime}");
            }
            if (top.HasValue)
            {
                request.QueryParameters.Top(top.Value);
            }
            if (!isGcc)
            {
                request.QueryParameters.OrderBy("lastModifiedDateTime desc");
            }
            return request.GetApiResult();
        }

        public ChatMessage GetChatMessage(string chatId, string messageId)
            => new GetChatMessage(resourceUrl, refreshAccessToken, RetryController, chatId, messageId).GetApiResult();

        public Chat GetChat(string chatId)
        {
            var request = new GetChat(resourceUrl, refreshAccessToken, RetryController, chatId);
            request.QueryParameters.Expand("members");
            return request.GetApiResult();
        }

        public IEnumerable<Chat> GetChats(string userId, int? top, bool isNoSort = false)
        {
            var request = new GetChats(resourceUrl, refreshAccessToken, RetryController, userId);
            if (!isNoSort) request.QueryParameters.OrderBy("lastMessagePreview/createdDateTime desc");
            request.QueryParameters.Expand("members,lastMessagePreview");
            if (top.HasValue) request.QueryParameters.Top(top.Value);
            return request.GetApiResult();
        }

        public byte[] GetHostedContentAsByte(string url) => new GetHostedContentAsByte(url, refreshAccessToken, RetryController).GetApiResult();

        public string GetHostedContentAsString(string url) => new GetHostedContentAsString(url, refreshAccessToken, RetryController).GetApiResult();

        public Event GetEvent(string groupId, string eventId) => new GetEvent(resourceUrl, refreshAccessToken, RetryController, groupId, eventId).GetApiResult();
    }
}
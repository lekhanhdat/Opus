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

//todo:qlluo:avoid svn conflic, move to MicrosoftGraphAPI.Get

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    class GetTeam : GetRequest<TeamObj>
    {
        public GetTeam(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}";
    }

   

    class ListChannels : ListRequest<Channel>
    {
        public ListChannels(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable, bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            RequestHeader = new System.Collections.Generic.Dictionary<string, string>() { { "Prefer", "include-unknown-enum-members" } };
            GroupId = groupId;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels";
    }
    
     class GetTeamPrimaryChannel : GetRequest<Channel>
    {
        public GetTeamPrimaryChannel(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/primaryChannel";
    }

    class GetChannelFilesFolder : GetRequest<PrivateChannelSite>
    {
        public GetChannelFilesFolder(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/filesFolder";
    }

    class GetRecordingDrive : GetRequest<DriveObj>
    {
        public GetRecordingDrive(string baseUrl, Func<string> getToken, string groupId, string driveId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            DriveId = driveId;
        }

        public string GroupId { get; private set; }

        public string DriveId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/groups/{GroupId}/drives/{DriveId}/special/recordings";
    }
    
    class ListIncomingChannels : ListRequest<Channel>
    {
        public ListIncomingChannels(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable, bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            RequestHeader = new System.Collections.Generic.Dictionary<string, string>() { { "Prefer", "include-unknown-enum-members" } };
            GroupId = groupId;
            UseBeta = useBeta;
        }
        public string GroupId { get; private set; }

        public bool UseBeta { get; private set; }
        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/incomingChannels";
    }
    class ListAllChannels : ListRequest<Channel>
    {
        public ListAllChannels(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable, bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            RequestHeader = new System.Collections.Generic.Dictionary<string, string>() { { "Prefer", "include-unknown-enum-members" } };
            GroupId = groupId;
            UseBeta = useBeta;
        }
        public string GroupId { get; private set; }

        public bool UseBeta { get; private set; }
        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/allChannels";
    }
    class ListInstalledApps : ListRequest<InstalledApp>
    {
        public string GroupId { get; private set; }

        public ListInstalledApps(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
            => GroupId = groupId;

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/installedApps?$expand=teamsAppDefinition";
    }

    class ListCatalogApps : ListRequest<CatalogTeamsApp>
    {
        public ListCatalogApps(string baseUrl, Func<string> getToken, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        { }

        protected override string RequestUrl => $"{apiUrlV1}/appCatalogs/teamsApps";
    }

    class ListChannelTabs : ListRequest<Tab>
    {
        public ListChannelTabs(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/tabs?$expand=teamsApp";
    }

    class GetChatTabs : ListRequest<Tab>
    {
        public GetChatTabs(string baseUrl, Func<string> getToken, IRetryable retryable, string chatId)
           : base(baseUrl, getToken, retryable)
        {
            ChatId = chatId;
        }

        public string ChatId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/chats/{ChatId}/tabs?$expand=teamsApp";
    }

    class GetChannelTab : GetRequest<Tab>
    {
        public GetChannelTab(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string tabId,
            IRetryable retryable)
           : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            TabId = tabId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string TabId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/tabs/{TabId}?$expand=teamsApp";
    }

    #region Channel Member

    public class GetTeamMembers : ListRequest<Member>
    {
        public string GroupId { get; private set; }

        public GetTeamMembers(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable = null) : base(baseUrl, getToken, retryable) => GroupId = groupId;

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/members";
    }

    class ListChannelMembers : ListRequest<Member>
    {
        public ListChannelMembers(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable)
           : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/members";
    }

    #endregion

    #region Channel Message

    class ListChannelAllMessages : ListRequest<ChatMessage>
    {
        public ListChannelAllMessages(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable, bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages";
    }

    class ListChannelMessages : GetRequest<ChannelMessageCollection>
    {
        public ListChannelMessages(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string queryString,
            IRetryable retryable,
            bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            QueryString = queryString;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string QueryString { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages{QueryString}";
    }

    class QueryChannelMessagesDelta : GetRequest<ChannelMessageCollection>
    {
        public QueryChannelMessagesDelta(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            IRetryable retryable,
            bool useBeta,
            string queryString = null)
           : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            QueryString = queryString;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string QueryString { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages/delta{QueryString}";
    }

    class GetChannelMessage : GetRequest<ChatMessage>
    {
        public GetChannelMessage(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string messageId,
            IRetryable retryable,
            bool useBeta)
           : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MessageId = messageId;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MessageId { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages/{MessageId}";
    }

    class ListChannelMessageAllReplies : ListRequest<ChatMessage>
    {
        public ListChannelMessageAllReplies(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string messageId,
            IRetryable retryable,
            bool useBeta)
          : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MessageId = messageId;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MessageId { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages/{MessageId}/replies";
    }

    class ListChannelMessageReplies : GetRequest<ChannelMessageReplyCollection>
    {
        public ListChannelMessageReplies(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string messageId,
            string queryString,
            IRetryable retryable,
            bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MessageId = messageId;
            QueryString = queryString;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MessageId { get; private set; }

        public string QueryString { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages/{MessageId}/replies{QueryString}";
    }

    class GetChannelMessageReply : GetRequest<ChatMessage>
    {
        public GetChannelMessageReply(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string messageId,
            string replyId,
            IRetryable retryable,
            bool useBeta)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MessageId = messageId;
            ReplyId = replyId;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MessageId { get; private set; }

        public string ReplyId { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels/{ChannelId}/messages/{MessageId}/replies/{ReplyId}";
    }

    class FilterChannels : ListRequest<Channel>
    {
        public FilterChannels(string baseUrl, Func<string> getToken, string groupId, string condition, IRetryable retryable, bool useBeta = false)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            Condition = condition;
            UseBeta = useBeta;
        }

        public string GroupId { get; private set; }

        public string Condition { get; private set; }

        public bool UseBeta { get; private set; }

        protected override string RequestUrl => $"{(UseBeta ? apiUrlBeta : apiUrlV1)}/teams/{GroupId}/channels?$filter={Condition}";
    }

    class GetPrivateChannel : ListRequest<Channel>
    {
        public GetPrivateChannel(string baseUrl, Func<string> getToken, string groupId, string channelName, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelName = channelName;
        }

        public string GroupId { get; private set; }

        public string ChannelName { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels?$filter=displayName eq '{ChannelName}' and membershipType eq 'private'";
    }

    class GetPrivateChannelSite : GetRequest<PrivateChannelSite>
    {
        public GetPrivateChannelSite(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable)
            : base(baseUrl, getToken, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/filesFolder";
    }

    #endregion

    #region Chat

    class GetChatMessages : IEnumerableRequest<ChatMessage>
    {
        public GetChatMessages(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable) => UserId = userId;

        public string UserId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/chats/getAllMessages";
    }

    class GetChatMessagesInChat : IEnumerableRequest<ChatMessage>
    {
        public GetChatMessagesInChat(string baseUrl, Func<string> getToken, IRetryable retryable, string userId, string chatId)
            : base(baseUrl, getToken, retryable)
        {
            UserId = userId;
            ChatId = chatId;
        }

        public string UserId { get; private set; }

        public string ChatId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/chats/{ChatId}/messages";
    }

    class GetChatMessage : GetRequest<ChatMessage>
    {
        public GetChatMessage(string baseUrl, Func<string> getToken, IRetryable retryable, string chatId, string messageId)
            : base(baseUrl, getToken, retryable)
        {
            ChatId = chatId;
            MessageId = messageId;
        }

        public string ChatId { get; private set; }

        public string MessageId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/chats/{ChatId}/messages/{MessageId}";
    }

    class GetChats : IEnumerableRequest<Chat>
    {
        public GetChats(string baseUrl, Func<string> getToken, IRetryable retryable, string userId)
            : base(baseUrl, getToken, retryable) =>
            UserId = userId;

        public string UserId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/users/{UserId}/chats";
    }

    class GetChat : GetRequest<Chat>
    {
        public GetChat(string baseUrl, Func<string> getToken, IRetryable retryable, string chatId)
            : base(baseUrl, getToken, retryable) =>
            ChatId = chatId;

        public string ChatId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/chats/{ChatId}";
    }

    #endregion
}
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

//todo:qlluo:avoid svn conflic, move to MicrosoftGraphAPI.Post

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Generic;

    class CompleteChannelMigration : PostRequest<Empty, Empty>
    {
        public string ChannelId { get; private set; }
        public string TeamsId { get; private set; }

        public CompleteChannelMigration(string baseUrl, Func<string> getToken, string teamsId, string channelId, IRetryable retryable) : base(baseUrl, getToken, null, retryable)
        {
            ChannelId = channelId;
            TeamsId = teamsId;
        }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{TeamsId}/channels/{ChannelId}/completeMigration";
    }

    class CompleteTeamsMigration : PostRequest<Empty, Empty>
    {
        public string TeamsId { get; private set; }

        public CompleteTeamsMigration(string baseUrl, Func<string> getToken, string teamsId, IRetryable retryable) : base(baseUrl, getToken, null, retryable)
        {
            TeamsId = teamsId;
        }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{TeamsId}/completeMigration";
    }

    class ArchiveTeam : PostRequest<TeamsArchiveObj, Empty>
    {
        public ArchiveTeam(string baseUrl, Func<string> getToken, string groupId, TeamsArchiveObj teamsArchiveObj, IRetryable retryable)
            : base(baseUrl, getToken, teamsArchiveObj, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/archive";

        // Only delegated permission supports ShouldSetSpoSiteReadOnlyForMembers property, and Graph API will return BadRequest if this property is included in request body with application permission
        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(TeamsArchiveObj.ShouldSetSpoSiteReadOnlyForMembers)
        };
    }

    class UnarchiveTeam : PostRequest<Empty, Empty>
    {
        public UnarchiveTeam(string baseUrl, Func<string> getToken, string groupId, IRetryable retryable)
            : base(baseUrl, getToken, null, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/unarchive";
    }

    class CreateChannel : PostRequest<Channel, Channel>
    {
        public CreateChannel(string baseUrl, Func<string> getToken, string groupId, Channel channel, IRetryable retryable)
            : base(baseUrl, getToken, channel, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels";

        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(Channel.DisplayName),
            nameof(Channel.Description),
            nameof(Channel.CreationMode),
            nameof(Channel.CreatedDateTime)
        };
    }

    class CreatePrivateChannel : PostRequest<Channel, Channel>
    {
        public CreatePrivateChannel(string baseUrl, Func<string> getToken, string groupId, Channel channel, IRetryable retryable)
            : base(baseUrl, getToken, channel, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels";
    }

    class AddTeamsApp : PostRequest<TeamsAppObj, InstalledApp>
    {
        public AddTeamsApp(string baseUrl, Func<string> getToken, string groupId, TeamsAppObj teamsAppObj, IRetryable retryable)
            : base(baseUrl, getToken, teamsAppObj, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/installedApps";

        protected override IEnumerable<string> IncludePropertiesName => new string[] { nameof(TeamsAppObj.TeamsAppOdataBind) };
    }

    public class AddChannelTab : PostRequest<TabAddObj, Tab>
    {
        public AddChannelTab(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            TabAddObj tab,
            IRetryable retryable)
            : base(baseUrl, getToken, tab, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/tabs";
    }

    #region Channel Member

    class AddTeamMember : PostRequest<OTJChannelMember, Member>
    {
        public AddTeamMember(
            string baseUrl,
            Func<string> getToken,
            string groupId,
            OTJChannelMember member,
            IRetryable retryable)
            : base(baseUrl, getToken, member, retryable) => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/members";
    }

    class AddChannelMember : PostRequest<OTJChannelMember, Member>
    {
        public AddChannelMember(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            OTJChannelMember member,
            IRetryable retryable)
            : base(baseUrl, getToken, member, retryable)
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

    class SendChannelMessage : PostRequest<ChatMessage, ChatMessage>
    {
        public SendChannelMessage(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            ChatMessage msg,
            IRetryable retryable)
            : base(baseUrl, getToken, msg, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/messages";
    }

    public class ReplyChannelMessage : PostRequest<ChatMessage, ChatMessage>
    {
        public ReplyChannelMessage(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string messageId,
            ChatMessage msg,
            IRetryable retryable)
            : base(baseUrl, getToken, msg, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MessageId = messageId;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MessageId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/messages/{MessageId}/replies";
    }

    #endregion
}
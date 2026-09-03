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

//todo:qlluo:avoid svn conflic, move to MicrosoftGraphAPI.Patch

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Collections.Generic;

    class UpdateTeam : PatchRequest<TeamObj>
    {
        public UpdateTeam(string baseUrl, Func<string> getToken, string groupId, TeamObj settings, IRetryable retryable)
            : base(baseUrl, getToken, settings, retryable)
            => GroupId = groupId;

        public string GroupId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}";
    }

    class UpdateChannel : PatchRequest<Channel>
    {
        public UpdateChannel(string baseUrl, Func<string> getToken, string groupId, Channel channel, IRetryable retryable)
            : base(baseUrl, getToken, channel, retryable)
        {
            GroupId = groupId;
            ChannelId = channel.Id;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}";

        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(Channel.DisplayName),
            nameof(Channel.Description)
        };
    }

    class UpdateTab : PatchRequest<TabUpdateObj>
    {
        public UpdateTab(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            TabUpdateObj tab,
            IRetryable retryable)
            : base(baseUrl, getToken, tab, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            TabId = tab.Id;
        }

        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string TabId { get; private set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/tabs/{TabId}";
    }

    class UpdateChannelMemberRoles : PatchRequest<OTJChannelMember>
    {
        public string GroupId { get; private set; }

        public string ChannelId { get; private set; }

        public string MemberId { get; private set; }

        public UpdateChannelMemberRoles(string baseUrl,
            Func<string> getToken,
            string groupId,
            string channelId,
            string memberId,
            OTJChannelMember member,
            IRetryable retryable)
            : base(baseUrl, getToken, member, retryable)
        {
            GroupId = groupId;
            ChannelId = channelId;
            MemberId = memberId;
        }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{GroupId}/channels/{ChannelId}/members/{MemberId}";

        protected override IEnumerable<string> IncludePropertiesName => new string[]
        {
            nameof(OTJChannelMember.ODataType),
            nameof(OTJChannelMember.Roles),
        };
    }
}
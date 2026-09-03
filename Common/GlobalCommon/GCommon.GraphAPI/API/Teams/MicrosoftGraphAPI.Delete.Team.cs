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
    using System.Text;
    using System.Threading.Tasks;

    public class DeleteTeamsChannel : DeleteRequest
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/teams/{this.GroupId}/channels/{this.ChannelId}";

        public string GroupId { get; set; }
        public string ChannelId { get; set; }
        public DeleteTeamsChannel(string baseUrl, Func<string> getToken, string groupId, string channelId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.ChannelId = channelId;
        }
    }

    public class DeleteTeamsApp : DeleteRequest
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/teams/{this.GroupId}/installedApps/{this.AppId}";

        public string GroupId { get; set; }
        public string AppId { get; set; }
        public DeleteTeamsApp(string baseUrl, Func<string> getToken, string groupId, string appId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.AppId = appId;
        }
    }

    public class DeleteTeamsTab : DeleteRequest
    {
        protected override string RequestUrl => $"{this.apiUrlV1}/teams/{this.GroupId}/channels/{this.ChannelId}/tabs/{this.TabId}";

        public string GroupId { get; set; }
        public string ChannelId { get; set; }
        public string TabId { get; set; }
        public DeleteTeamsTab(string baseUrl, Func<string> getToken, string groupId, string channelId, string tabId, IRetryable retryable) : base(baseUrl, getToken, retryable)
        {
            this.GroupId = groupId;
            this.ChannelId = channelId;
            this.TabId = tabId;
        }
    }

    public class RemoveTeamMember : DeleteRequest
    {
        public RemoveTeamMember(string baseUrl, Func<string> getToken, IRetryable retryable, string teamId, string membershipId) : base(baseUrl, getToken, retryable)
        {
            TeamId = teamId;
            MembershipId = membershipId;
        }

        public string TeamId { get; set; }

        public string MembershipId { get; set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{TeamId}/members/{MembershipId}";
    }

    public class RemoveChannelMember : DeleteRequest
    {
        public RemoveChannelMember(string baseUrl, Func<string> getToken, IRetryable retryable, string teamId, string channelId, string membershipId) : base(baseUrl, getToken, retryable)
        {
            TeamId = teamId;
            ChannelId = channelId;
            MembershipId = membershipId;
        }

        public string TeamId { get; set; }

        public string ChannelId { get; set; }

        public string MembershipId { get; set; }

        protected override string RequestUrl => $"{apiUrlV1}/teams/{TeamId}/channels/{ChannelId}/members/{MembershipId}";
    }
}
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
namespace ExchangeCommonWrapper
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    [DataContract]
    public class MicrosoftTeamsEntity : ExchangeEntityBase
    {
        //[DataMember]
        //public Team Team { get; set; }
        [DataMember]
        public bool? IsArchived { get; set; }
        [DataMember]
        public List<TeamMember> TeamMembers { get; set; }
        [DataMember]
        public List<TeamChannel> TeamChannels { get; set; }
        [DataMember]
        public TeamFunSetting TeamFunSettings { get; set; }
        [DataMember]
        public TeamGuestSetting TeamGuestSettings { get; set; }
        [DataMember]
        public TeamMemberSetting TeamMemberSettings { get; set; }
        [DataMember]
        public TeamMessagingSetting TeamMessagingSettings { get; set; }
        [DataMember]
        public List<TeamsApp> TeamsApps { get; set; }

    }

    [DataContract]
    public class Team : ExchangeEntityBase
    {
        [DataMember]
        public string GroupId { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string Classification { get; set; }
        [DataMember]
        public TeamAccessType AccessType { get; set; }
        [DataMember]
        public bool AddCreatorAsMember { get; set; }
    }

    [DataContract]
    public enum TeamAccessType
    {
        [EnumMember]
        Public = 0,
        [EnumMember]
        Private = 1,
    }

    [DataContract]
    public class TeamMember : ExchangeEntityBase
    {
        public string Id { get; set; }

        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        //UserPrincipalName, not email address
        public string MailboxAddress { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public TeamMemberRoleType RoleType { get; set; }

        public override bool Equals(object obj)
        {
            var target = obj as TeamMember;
            if (target == null) return false;
            return this.MailboxAddress.Equals(target.MailboxAddress, System.StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return this.MailboxAddress.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.RoleType},{this.UserId},{this.MailboxAddress}";
        }

    }

    [DataContract]
    public enum TeamMemberRoleType
    {
        [EnumMember]
        Owner = 0,
        [EnumMember]
        Member = 1,
        [EnumMember]
        Guest = 2,
    }

    [DataContract]
    public class TeamChannel : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string MembershipType { get; set; }
        [DataMember]
        public List<ChannelTab> ChannelTabs { get; set; }
        [DataMember]
        public List<ChannelMember> ChannelMembers { get; set; }
        [DataMember]
        public string FilesFolderUrl { get; set; }
        [DataMember]
        public bool IsInComing { get; set; }
        [DataMember]
        public bool IsExternal { get; set; }
        public override bool Equals(object obj)
        {
            var target = obj as TeamChannel;
            if (target == null) return false;
            return this.Id.Equals(target.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }

    [DataContract]
    public class TeamFunSetting : ExchangeEntityBase
    {
        [DataMember]
        public bool AllowGiphy { get; set; }
        [DataMember]
        public GiphyContentRating GiphyContentRating { get; set; }
        [DataMember]
        public bool AllowStickersAndMemes { get; set; }
        [DataMember]
        public bool AllowCustomMemes { get; set; }
    }

    [DataContract]
    public enum GiphyContentRating
    {
        [EnumMember]
        strict = 0,
        [EnumMember]
        moderate = 1,
        [EnumMember(Value = "unknownFutureValue")]
        allowallcontent = 2,
    }

    [DataContract]
    public class TeamGuestSetting : ExchangeEntityBase
    {
        [DataMember]
        public bool AllowCreateUpdateChannels { get; set; }
        [DataMember]
        public bool AllowDeleteChannels { get; set; }
    }

    [DataContract]
    public class TeamMemberSetting : ExchangeEntityBase
    {
        [DataMember]
        public bool AllowCreateUpdateChannels { get; set; }
        [DataMember]
        public bool AllowDeleteChannels { get; set; }
        [DataMember]
        public bool AllowAddRemoveApps { get; set; }
        [DataMember]
        public bool AllowCreateUpdateRemoveTabs { get; set; }
        [DataMember]
        public bool AllowCreateUpdateRemoveConnectors { get; set; }
    }

    [DataContract]
    public class TeamMessagingSetting : ExchangeEntityBase
    {
        [DataMember]
        public bool AllowUserEditMessages { get; set; }
        [DataMember]
        public bool AllowUserDeleteMessages { get; set; }
        [DataMember]
        public bool AllowOwnerDeleteMessages { get; set; }
        [DataMember]
        public bool AllowTeamMentions { get; set; }
        [DataMember]
        public bool AllowChannelMentions { get; set; }
    }

    [DataContract]
    public class TeamsApp : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public TeamsAppDefinition TeamsAppDefinition { get; set; }
    }

    [DataContract]
    public class TeamsAppDefinition : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string TeamsAppId { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Version { get; set; }
    }

    [DataContract]
    public class CatalogApp : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public object ExternalId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string DistributionMethod { get; set; }
    }

    [DataContract]
    public class ChannelTab : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string TeamsAppId { get; set; }

        [DataMember]
        public TeamsAppInfo TeamsApp { get; set; }

        [DataMember]
        public string SortOrderIndex { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string WebUrl { get; set; }

        [DataMember]
        public string Configuration { get; set; }
    }

    [DataContract]
    public class TeamsAppInfo
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }

        public string DisplayName { get; set; }

        public string DistributionMethod { get; set; }
    }

    [DataContract]
    public class ChannelMember : ExchangeEntityBase
    {
        [IgnoreDataMember]
        public string Id { get; set; }

        [DataMember]
        public string ODataType { get; set; }

        [DataMember]
        public string[] Roles { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string VisibleHistoryStartDateTime { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string TenantId { get; set; }
    }
}
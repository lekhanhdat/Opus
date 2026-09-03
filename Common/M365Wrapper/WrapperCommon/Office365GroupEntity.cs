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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeCommonWrapper
{
    [Serializable]
    public class Office365GroupEntity
    {
        public GroupAccessType AccessType { get; set; }
        public GroupAdditionalProperties AdditionalProperties { get; set; }
        public string Classification { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string ExternalDirectoryObjectId { get; set; }
        public GroupResource[] GroupResources { get; set; }
        public string MailboxGuid { get; set; }
        public MailboxSettings MailboxSettings { get; set; }
        public int OwnerCount { get; set; }
        public string SmtpAddress { get; set; }
        public UnifiedGroupSKU UnifiedGroupSKU { get; set; }
        public UserGroupRelationship UserGroupRelationship { get; set; }
        public List<GroupMemberV2> GroupMemberList { get; set; }
        public bool IsTeamsGroup { get; set; }
        public int SendToMeida { get; set; }
    }
    [Serializable]
    public enum GroupAccessType
    {
        Public = 0,
        Private = 1,
        HiddenMembership = 2
    }
    [Serializable]
    public class GroupAdditionalProperties
    {
        public int ExternalMemberCount { get; set; }
        public bool IsGroupMembershipHidden { get; set; }
        public bool IsMembershipDynamic { get; set; }
        public string MembershipRule { get; set; }
        public string MembershipRuleProcessingState { get; set; }
        public bool SubscriptionEnabled { get; set; }
    }
    [Serializable]
    public class GroupResource
    {
        public GroupResouceType Type { get; set; }
        public string Url { get; set; }
    }
    [Serializable]
    public enum GroupResouceType
    {
        Site = 0,
        Files = 1,
        Notebook = 2,
        Planner = 3,
        ProvisionedNotebook = 4,
        Profile = 5,
        Integrations = 6,
        People = 7,
        Inbox = 8,
        Calendar = 9
    }
    [Serializable]
    public class MailboxSettings
    {
        public bool AlwaysSubscribeMembersToCalendarEvents { get; set; }
        public bool AutoSubscribeNewMembers { get; set; }
        public bool ExternalSendersEnabled { get; set; }
        public string MailboxCultureName { get; set; }
    }
    [Serializable]
    public enum UnifiedGroupSKU
    {
        Default = 0,
        Yammer = 1
    }
    [Serializable]
    public class UserGroupRelationship
    {
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }
        public bool IsSubscribed { get; set; }
    }
    [Serializable]
    public class GroupMember
    {
        public bool IsOwner { get; set; }
        public string UserName { get; set; }
    }


    [DataContract]
    public class Office365GroupEntityV2
    {
        [DataMember]
        public GroupAccessTypeV2 AccessType { get; set; }
        [DataMember]
        public GroupAdditionalPropertiesV2 AdditionalProperties { get; set; }
        [DataMember]
        public string Classification { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string ExternalDirectoryObjectId { get; set; }
        [DataMember]
        public GroupResourceV2[] GroupResources { get; set; }
        [DataMember]
        public string MailboxGuid { get; set; }
        [DataMember]
        public MailboxSettingsV2 MailboxSettings { get; set; }
        /// <summary>
        /// 通过Graph Api $select 获取到的 setting
        /// </summary>
        [DataMember]
        public ExtraSettings ExtraSettings { get; set; }
        [DataMember]
        public int OwnerCount { get; set; }
        [DataMember]
        public string SmtpAddress { get; set; }
        [DataMember]
        public UnifiedGroupSKUV2 UnifiedGroupSKU { get; set; }
        [DataMember]
        public UserGroupRelationshipV2 UserGroupRelationship { get; set; }
        [DataMember]
        public List<GroupMemberV2> GroupMemberList { get; set; }
        [DataMember]
        public bool IsTeamsGroup { get; set; }
        [DataMember]
        public bool IsVivaGroup { get; set; }
        [DataMember]
        public int SendToMeida { get; set; }
        [DataMember]
        public string PreferredDataLocation { get; set; }
        [DataMember]
        public string CreatedDateTime { get; set; }
    }
    [DataContract]
    public enum GroupAccessTypeV2
    {
        [EnumMember]
        Public = 0,
        [EnumMember]
        Private = 1,
        [EnumMember]
        HiddenMembership = 2
    }
    [DataContract]
    public class GroupAdditionalPropertiesV2
    {
        [DataMember]
        public int ExternalMemberCount { get; set; }

        [DataMember]
        public bool IsGroupMembershipHidden { get; set; }

        [IgnoreDataMember]
        private bool? _isMembershipDynamic;
        [IgnoreDataMember]
        public bool IsMembershipDynamic
        {
            get
            {
                if (_isMembershipDynamic.HasValue)
                {
                    return _isMembershipDynamic.Value;
                }
                else
                {
                    _isMembershipDynamic = GroupTypes.Contains("DynamicMembership", StringComparer.OrdinalIgnoreCase) 
                        && string.Equals(MembershipRuleProcessingState, "On", StringComparison.OrdinalIgnoreCase);
                    return _isMembershipDynamic.Value;
                }
            }
            //set
            //{
            //    _isMembershipDynamic = value;
            //}
        }

        [DataMember]
        public string[] GroupTypes { get; set; } = [];

        [DataMember]
        public string MembershipRule { get; set; }

        [DataMember]
        public string MembershipRuleProcessingState { get; set; }

        [DataMember]
        public bool SubscriptionEnabled { get; set; }

        public string ToMembershipString()
        {
            if (!IsMembershipDynamic)
            {
                return "Assigned Membership Type";
            }
            else
            {
                return $"Dynamic Membership Type; Rule:{MembershipRule}; State:{MembershipRuleProcessingState}";
            }
        }
    }
    [DataContract]
    public class GroupResourceV2
    {
        [DataMember]
        public GroupResouceTypeV2 Type { get; set; }
        [DataMember]
        public string Url { get; set; }
    }
    [DataContract]
    public enum GroupResouceTypeV2
    {
        [EnumMember]
        Site = 0,
        [EnumMember]
        Files = 1,
        [EnumMember]
        Notebook = 2,
        [EnumMember]
        Planner = 3,
        [EnumMember]
        ProvisionedNotebook = 4,
        [EnumMember]
        Profile = 5,
        [EnumMember]
        Integrations = 6,
        [EnumMember]
        People = 7,
        [EnumMember]
        Inbox = 8,
        [EnumMember]
        Calendar = 9
    }
    [DataContract]
    public class MailboxSettingsV2
    {

        [DataMember]
        public bool AlwaysSubscribeMembersToCalendarEvents { get; set; }
        [DataMember]
        public bool AutoSubscribeNewMembers { get; set; }
        [DataMember]
        public bool ExternalSendersEnabled { get; set; }
        [DataMember]
        public string MailboxCultureName { get; set; }
    }
    [DataContract]
    public class UnifiedGroupSKUV2
    {
        [DataMember]
        public string GroupType { get; set; }
        [DataMember]
        public bool IsNull { get; set; }
    }
    [DataContract]
    public class UserGroupRelationshipV2
    {
        [DataMember]
        public bool IsMember { get; set; }
        [DataMember]
        public bool IsOwner { get; set; }
        [DataMember]
        public bool IsSubscribed { get; set; }
    }
    [DataContract]
    public class GroupMemberV2
    {
        [DataMember]
        public string OdataType { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public bool IsOwner { get; set; }
        [DataMember]
        public bool IsMember { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Name {  get; set; }
    }
    /// <summary>
    /// 通过Graph Api $select 获取到的 setting
    /// </summary>
    [DataContract]
    public class ExtraSettings
    {
        [DataMember]
        public Boolean AllowExternalSenders { get; set; }
        [DataMember]
        public Boolean AutoSubscribeNewMembers { get; set; }
        [DataMember]
        public Boolean HideFromAddressLists { get; set; }
        [DataMember]
        public Boolean HideFromOutlookClients { get; set; }
    }

    [DataContract]
    public class AssignedLabelsV2
    {
        [DataMember]
        public string LabelId { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
    }
}
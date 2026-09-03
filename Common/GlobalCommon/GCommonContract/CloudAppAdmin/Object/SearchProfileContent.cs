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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchProfileContent : IProfileContent
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<SearchRule> Rules { get; set; }

        [DataMember]
        public SearchLevel Level { get; set; }

        [DataMember]
        public string Expression { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchRule
    {
        [DataMember]
        public int SequenceNo { get; set; }

        [DataMember]
        public RuleLevel RuleLevel { get; set; }

        [DataMember]
        public RuleType RuleType { get; set; }

        [DataMember]
        public RuleCondition Condition { get; set; }

        [DataMember]
        public RuleValue Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchLevel
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        ADUser = 1,

        [EnumMember]
        ADGroup = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleLevel
    {
        [EnumMember]
        UserProperty = 0,

        [EnumMember]
        GroupProperty = 1,

        [EnumMember]
        Settings = 2,

        [EnumMember]
        License = 3,

        [EnumMember]
        Details = 4,

        [EnumMember]
        Role = 5,

        [EnumMember]
        Type = 6,

        [EnumMember]
        Mailbox = 7,

        [EnumMember]
        Application = 8,

        [EnumMember]
        UserOrganization = 9,

        [EnumMember]
        Audit = 10,

        [EnumMember]
        SyncInfo = 11,

        [EnumMember]
        GroupOrganization = 12,

        [EnumMember]
        MailBoxGeneral = 13,

        [EnumMember]
        MailBoxFeature = 14,

        [EnumMember]
        CustomField = 15,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        DisplayName = 1,

        [EnumMember]
        JobTitle = 2,

        [EnumMember]
        FirstName = 3,

        [EnumMember]
        LastName = 4,

        [EnumMember]
        UserPrincipalName = 5,

        [EnumMember]
        OrganizationRole = 6,

        [EnumMember]
        UsageLocation = 7,

        [EnumMember]
        Department = 8,

        [EnumMember]
        OfficeNumber = 9,

        [EnumMember]
        OfficePhone = 10,

        [EnumMember]
        MobilePhone = 11,

        [EnumMember]
        StreetAddress = 12,

        [EnumMember]
        City = 13,

        [EnumMember]
        StateOrProvince = 14,

        [EnumMember]
        ZipOrPostalCode = 15,

        [EnumMember]
        CountryOrRegion = 16,

        //Level: settings
        [EnumMember]
        SigninStatus = 17,

        //Level: license
        [EnumMember]
        Assignment = 18,

        //Level:details
        [EnumMember]
        Error = 19,

        //Level: role
        [EnumMember]
        AdminAccess = 20,

        //Level: type
        [EnumMember]
        GroupType = 21,

        //Level: mailbox
        [EnumMember]
        RecipientType = 22,

        [EnumMember]
        Permission = 23,

        [EnumMember]
        ProhibitSendQuota = 24,

        [EnumMember]
        DistributionList = 25,

        [EnumMember]
        Activity = 26,

        //Level: application
        [EnumMember]
        Access = 27,

        //Level: organization
        [EnumMember]
        Group = 28,

        [EnumMember]
        Manager = 29,

        //Level: audit
        [EnumMember]
        Password = 30,

        //Level: syncinfo
        [EnumMember]
        Status = 31,

        //Level: organization
        [EnumMember]
        Members = 32,

        //Level: organization
        [EnumMember]
        Owners = 33,

        //Level: mailbox
        [EnumMember]
        ProhibitSendReceiveQuota = 34,

        [EnumMember]
        IssueWarningQuota = 35,

        //Level: audit
        [EnumMember]
        Login = 36,

        [EnumMember]
        CreateTime = 37,

        [EnumMember]
        Initials = 38,

        [EnumMember]
        Alias = 39,

        [EnumMember]
        UserID = 40,

        [EnumMember]
        EmailAddresses = 41,

        //[EnumMember]
        //Number = 42,

        [EnumMember]
        SharingPolicy = 42,

        [EnumMember]
        RoleAssignmentPolicy = 43,

        [EnumMember]
        RetentionPolicy = 44,

        [EnumMember]
        UMEnabled = 45,

        [EnumMember]
        ActiveSyncEnabled = 46,

        [EnumMember]
        OWAforDevicesEnabled = 47,

        [EnumMember]
        OWAEnabled = 48,

        [EnumMember]
        ImapEnabled = 49,

        [EnumMember]
        PopEnabled = 50,

        [EnumMember]
        MAPIEnabled = 51,

        [EnumMember]
        LitigationHoldEnabled = 52,

        [EnumMember]
        ArchiveStatus = 53,

        [EnumMember]
        AddressBookPolicy = 54,

        [EnumMember]
        ForwardingAddress = 55,

        [EnumMember]
        RecipientLimits = 56,

        [EnumMember]
        MaxSendSize = 57,

        [EnumMember]
        MaxReceiveSize = 58,

        [EnumMember]
        AcceptMessagesOnlyFrom = 59,

        [EnumMember]
        RejectMessagesFrom = 60,

        [EnumMember]
        CustomAttribute1 = 61,

        [EnumMember]
        CustomAttribute2 = 62,

        [EnumMember]
        CustomAttribute3 = 63,

        [EnumMember]
        CustomAttribute4 = 64,

        [EnumMember]
        CustomAttribute5 = 65,

        [EnumMember]
        CustomAttribute6 = 66,

        [EnumMember]
        CustomAttribute7 = 67,

        [EnumMember]
        CustomAttribute8 = 68,

        [EnumMember]
        CustomAttribute9 = 69,

        [EnumMember]
        CustomAttribute10 = 70,

        [EnumMember]
        CustomAttribute11 = 71,

        [EnumMember]
        CustomAttribute12 = 72,

        [EnumMember]
        CustomAttribute13 = 73,

        [EnumMember]
        CustomAttribute14 = 74,

        [EnumMember]
        CustomAttribute15 = 75,

        [EnumMember]
        UserType = 76,

        [EnumMember]
        ExternalUser = 77,

        [EnumMember]
        InternalUser = 78,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleCondition
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Equals = 1,

        [EnumMember]
        Contains = 2,

        [EnumMember]
        Is = 3,

        [EnumMember]
        NotEquals = 4,

        [EnumMember]
        HasFullAccessTo = 5,

        [EnumMember]
        SendOnBehalfOf = 6,

        [EnumMember]
        SendAs = 7,

        [EnumMember]
        HasSharedMailboxPermissionOf = 8,

        [EnumMember]
        HasPermissionOfMailboxCalendarFolder = 9,

        [EnumMember]
        NoActivityInLastXDays = 10,

        [EnumMember]
        DidNotChangePasswordInLastXDays = 11,

        [EnumMember]
        In = 12,

        [EnumMember]
        LargerThan = 13,

        [EnumMember]
        LessThan = 14,

        [EnumMember]
        EqualTo = 15,

        [EnumMember]
        NotIn = 16,

        [EnumMember]
        ChangePasswordInLastXDays = 17,

        [EnumMember]
        DidNotLoginInLastXDays = 18,

        [EnumMember]
        LoginInLastXDays = 19,

        [EnumMember]
        On = 20,

        [EnumMember]
        From = 21,

        [EnumMember]
        NotContains = 22,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleValue
    {
        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public string ExtendValue { get; set; }

        [DataMember]
        public string Extension { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtensionList { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TimeZoneId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SigninStatus
    {
        [EnumMember]
        Allowed = 0,

        [EnumMember]
        Blocked = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RecipientType
    {
        [EnumMember]
        User = 0,

        [EnumMember]
        UserMailbox = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SyncInfoStatus
    {
        [EnumMember]
        SyncedFromOnPremise = 0,

        [EnumMember]
        SyncedFromOnPremiseAndNolongerSync = 1,

        [EnumMember]
        NeverBeenSyncedFromOnPremise = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IMNumberUnit
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        KB = 1,

        [EnumMember]
        MB = 2,

        [EnumMember]
        GB = 3
    }
}
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
    using Common;
    using Server.Common.Profile.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CloudAppAdmin.Message;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPEProfileContent : IProfileContent
    {
        [DataMember]
        public string DtoId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<CAAPERule> Rules { get; set; }

        [DataMember]
        public int ScanIntervalCount { get; set; }

        [DataMember]
        public DateTime ScanStartTime { get; set; }

        [DataMember]
        public bool IsSendDaily { get; set; }

        [DataMember]
        public DateTime SendDailyStartTime { get; set; }

        [DataMember]
        public int SendDayOfWeek { get; set; }

        [DataMember]
        public DateTime SendWeeklyStartTime { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }

        [DataMember]
        public List<ADUser> Users { get; set; }

        [DataMember]
        public List<string> UserSetIds { get; set; }

        [DataMember]
        public List<string> UserFilterIds { get; set; }

        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public List<string> WhatIfResultProfileIds { get; set; }

        [DataMember]
        public List<string> ResultProfileIds { get; set; }

        [DataMember]
        public List<SearchProfileContent> SearchProfileContents { get; set; }

        public List<SimpleADUser> TempSimpleUsers { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleCAAPEProfileContent
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public string PlanId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPERule
    {
        [DataMember]
        public CAAPERuleCategory RuleCategory { get; set; }

        [DataMember]
        public CAAPERuleValue RuleValue { get; set; }

        [DataMember]
        public bool ActionRequired { get; set; }

        [DataMember]
        public string ActionValue { get; set; }

        [DataMember]
        public bool IsSendImmediately { get; set; }

        [DataMember]
        public string Recipients { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public UserScope BlackList { get; set; }

        [DataMember]
        public UserScope WhiteList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPERuleValue
    {
        [DataMember]
        public bool IsAllAD { get; set; }

        [DataMember]
        public List<ADGroup> Groups { get; set; }

        [DataMember]
        public List<string> GroupSetIds { get; set; }

        [DataMember]
        public List<ADApplication> Applications { get; set; }

        [DataMember]
        public List<ADLicense> Licenses { get; set; }

        [DataMember]
        public List<string> UserSetIds { get; set; }

        [DataMember]
        public List<ADUser> Users { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAAPERuleCategory
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        UserLocation = 1,

        [EnumMember]
        UserLicenseLogon = 2,

        [EnumMember]
        UserPasswordChange = 3,

        [EnumMember]
        UserAddToGroup = 4,

        [EnumMember]
        UserApplicationAccess = 5,

        [EnumMember]
        UserLicenseAssignment = 6,

        [EnumMember]
        UserCreateO365Group = 7,

        [EnumMember]
        UserAdminRole = 8,

        [EnumMember]
        UserMembershipConflict = 9,

        [EnumMember]
        UserEnableDelve = 10,

        [EnumMember]
        MailboxArchive = 11,

        [EnumMember]
        GhostuserCleaning = 12,
    }
}
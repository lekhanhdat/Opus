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
    using AvePoint.GCommon.Contract.CloudAppAdmin.Message;
    using AvePoint.GCommon.Contract.Common;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADUser
    {
        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserPrincipalName { get; set; }

        [DataMember]
        public ADRole role { get; set; }

        [DataMember]
        public List<string> AlternateEmailAddress { get; set; }

        //public bool AllowUserSigninAndAccessService { get; set; }

        [DataMember]
        public string UsageLocation { get; set; }

        //public string SourcedFrom { get; set; }

        [DataMember]
        public string JobTitle { get; set; }

        [DataMember]
        public string Department { get; set; }

        [DataMember]
        public string OfficeNumber { get; set; }

        //public string ManagerID { get; set; }

        [DataMember]
        public string OfficePhone { get; set; }

        [DataMember]
        public string MobilePhone { get; set; }

        [DataMember]
        public string StreetAddress { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string StateOrProvince { get; set; }

        [DataMember]
        public string ZipOrPostalCode { get; set; }

        [DataMember]
        public string CountryOrRegion { get; set; }

        [DataMember]
        public string FaxNumber { get; set; }

        [DataMember]
        public string SoftDeletionTimestamp { get; set; }

        //public string AuthenticationPhone { get; set; }
        //public string AlternateAuthenticationPhone { get; set; }
        //public string AuthenticationEmail { get; set; }
        [DataMember]
        public List<ADAppRoleAssignment> Applications { get; set; }

        [DataMember]
        public List<string> AssignedLicenseSkus { get; set; }

        [DataMember]
        public List<ADLicense> AssignedLicenses { get; set; }

        [DataMember]
        public List<ADMailbox> Mailboxes { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADGroup> MemberOf { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string GroupSetId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ADPasswordProfile PasswordProfile { get; set; }

        [DataMember]
        public ExpireTime ExpireTime { get; set; }

        [DataMember]
        public bool? DirSyncEnabled { get; set; }

        [DataMember]
        public string UserType { get; set; }

        #region PowerShell Property

        public bool? BlockCredential { get; set; }
        public DateTime? CreatedTime { get; set; }
        public bool? NoError { get; set; }

        #endregion PowerShell Property

        [DataMember]
        public List<ADExtensionProperty> ExtensionProperties { get; set; }

        [DataMember]
        public bool IsInviteUser { get; set; }

        [DataMember]
        public string InviteMsg { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADExtensionProperty
    {
        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public List<string> TargetObjects { get; set; }

        [DataMember]
        public bool TargetIsUser { get; set; }

        [DataMember]
        public bool TargetIsGroup { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleADUser
    {
        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string UserPrincipalName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public bool? Checked { get; set; }

        [DataMember]
        public bool? Chioce { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADAppRoleAssignment
    {
        //application service principal object id
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        //application display name
        [DataMember]
        public string DisplayName { get; set; }

        //application role id
        [DataMember]
        public string RoleId { get; set; }

        //application reference object id
        [DataMember]
        public string ReferenceObjectId { get; set; }

        [DataMember]
        public string PrincipalType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UserRoleInDirectory : int
    {
        [EnumMember]
        User = 1,

        [EnumMember]
        GlobalAdmin = 2,

        [EnumMember]
        BillingAdmin = 3,

        [EnumMember]
        ServiceAdmin = 4,

        [EnumMember]
        UserAdmin = 5,

        [EnumMember]
        PasswordAdmin = 6
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADPasswordProfile
    {
        [DataMember]
        public bool? ForceChangePasswordNextLogin { get; set; }

        [DataMember]
        public string Password { get; set; }
    }

    public class ExpireTime
    {
        public int No { get; set; }
        public ExpireTimeUnit Unit { get; set; }
    }

    public enum ExpireTimeUnit
    {
        Day = 1,
        Week = 2,
        Month = 3,
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ADRightType
    {
        [EnumMember]
        SendOnBehalf = 1,

        [EnumMember]
        SendAs = 2,

        [EnumMember]
        FullAccess = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADMailbox
    {
        [DataMember]
        public string Mailbox { get; set; }

        [DataMember]
        public ADRightType ADRightType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TempUserInfo
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string UserPrincipalName { get; set; }

        [DataMember]
        public long ExpireTime { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public bool IsDayLightSaving { get; set; }

        [DataMember]
        public TempUserExpireType ExpireType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TempUserExpireType
    {
        [EnumMember]
        ExpireRightNow = 0,

        [EnumMember]
        ExpireOn = 1,

        [EnumMember]
        NeverExpire = 2,
    }
}
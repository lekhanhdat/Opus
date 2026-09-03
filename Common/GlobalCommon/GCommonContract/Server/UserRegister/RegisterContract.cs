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


using System.Collections.Generic;
using System.Runtime.Serialization;
//using System.ServiceModel.Configuration;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.Login;

namespace AvePoint.GCommon.Contract.Server.UserRegister
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserRegisterDto
    {
        [DataMember]
        public AccountDto Account { get; set; }

        [DataMember]
        public string Schema { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public UserRegisterSetting Setting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserRegisterResultDto
    {
        [DataMember]
        public UserRegisterState State { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class InviteUserResultDto
    {
        [DataMember]
        public UserRegisterState State { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SendInviteEmailResultDto
    {
        [DataMember]
        public List<AccountDto> EmailFailedUsers { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailArg
    {
        [DataMember]
        public string Schema { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int Port { get; set; }
    }

    public enum WarnState
    {
        Last10Days = 10,
        LastDay = 1,
        AlreadyExpiration = 0,
    }

    public delegate OnlineUserResultDto DeleteAccountFunction(List<string> ids);

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "UserRegisterSetting")]
    public class UserRegisterSetting
    {
        [DataMember]
        [XmlAttribute]
        public string FirstName { get; set; }

        [DataMember]
        [XmlAttribute]
        public string LastName { get; set; }

        [DataMember]
        [XmlAttribute]
        public string CompanyName { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Telephone { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Email { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Address { get; set; }

        [DataMember]
        [XmlAttribute]
        public string City { get; set; }

        [DataMember]
        [XmlAttribute]
        public string State { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Country { get; set; }

        [DataMember]
        [XmlAttribute]
        public string ZipCode { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool NeedResendFeedback { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool RegisterFromApp { get; set; }

        [DataMember]
        [XmlAttribute]
        public string SiteCollectionUrl { get; set; }

        [DataMember]
        [XmlAttribute]
        public int StateEnum { get; set; }

        [DataMember]
        [XmlAttribute]
        public int CountryEnum { get; set; }

        [DataMember]
        [XmlAttribute]
        public string TenantId { get; set; }

        [DataMember]
        [XmlAttribute]
        public bool IsMySite { get; set; }

        [DataMember]
        [XmlAttribute]
        public string ContactName { get; set; }

        [DataMember]
        public TenantUserInfo TenantUserInfo { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantUserInfo
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string GroupId { get; set; }
        [DataMember]
        public long ExpiredTime { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UserRegisterState
    {
        [EnumMember]
        Succeed = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Warning = 2,
        [EnumMember]
        NotExisted = 3,
        [EnumMember]
        HasExpirated = 4,
        [EnumMember]
        InitDefaultSettingsFailed = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnlineUserDataDto
    {
        [DataMember]
        public AccountMappingDto CurrentAccountMapping { get; set; }
        [DataMember]
        public CurrentUserModel CurrentUserModel { get; set; }

        [DataMember]
        public AccountDto CurrentAccount { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Accounts { get; set; }

        [DataMember]
        public string Schema { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string GroupId { get; set; }

        [DataMember]
        public long Time { get; set; }

        [DataMember]
        public List<string> UserIds { get; set; }

        [DataMember]
        public List<string> PlanIds { get; set; }

        [DataMember]
        public List<string> SiteCollectionIds { get; set; }

        [DataMember]
        public ObjectRoleType RoleType { get; set; }

        [DataMember]
        public OnlineUserActiveStatus ActiveStatus { get; set; }

        [DataMember]
        public List<UserPermission> Permissions { get; set; }

        [DataMember]
        public List<string> PermissionLevels { get; set; }

        [DataMember]
        public Dictionary<string, int> PermissionDic { get; set; }

        [DataMember]
        public InviteType InviteType { get; set; }

        [DataMember]
        public List<UserGroup> UserGroups { get; set; }

        [DataMember]
        public bool IsPowerUser { get; set; }

        [DataMember]
        public bool IsInviteSupport { get; set; }

        #region for invite support user
        [DataMember]
        public string ProductType { get; set; }

        [DataMember]
        public string IssueType { get; set; }

        [DataMember]
        public int Severity { get; set; }

        [DataMember]
        public string ContactType { get; set; }

        [DataMember]
        public string ContactInfo { get; set; }

        [DataMember]
        public string AdditionalContactInfo { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string AttachmentFileName { get; set; }

        [DataMember]
        public byte[] Attachment { get; set; }

        [DataMember]
        public string StorageContainerName { get; set; }

        [DataMember]
        public string StorageFileName { get; set; }

        [DataMember]
        public string AccountNumber { get; set; }
        #endregion

        [DataMember]
        public InviteUserType InviteUserType { get; set; }

        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public bool IsDisableTemporaryAccount { get; set; }

        [DataMember]
        public bool IsManagerByPartner { get; set; }
    }

    public enum InviteUserType
    {
        LocalUser,
        Office365User,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UsingPlanInfoDto
    {
        [DataMember]
        public List<string> UsingPlanNames { get; set; }
        [DataMember]
        public List<string> NotDeleteSiteCollectionIds { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnlineUserResultDto
    {
        [DataMember]
        public List<AccountDto> Accounts { get; set; }

        [DataMember]
        public List<AccountDto> SuccAccounts { get; set; }

        [DataMember]
        public List<AccountDto> ExsitedAccounts { get; set; }

        [DataMember]
        public List<AccountDto> FailedAccounts { get; set; }

        [DataMember]
        public AccountDto Account { get; set; }

        [DataMember]
        public GroupDto Group { get; set; }

        [DataMember]
        public OnlineUserResultStatus Status { get; set; }

        [DataMember]
        public List<PlanDto> CheckUserPlans { get; set; }

        [DataMember]
        public List<ProfileDto> CheckUserProfiles { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> PlanSiteCollectionMapping { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> CheckUserRemoteSiteCollection { get; set; }

        [DataMember]
        public List<EmailAccountDto> CheckUserMailBoxes { get; set; }

        [DataMember]
        public string RemoteSiteCollectionsJson { get; set; }
        [DataMember]
        public string MailBoxesJson { get; set; }
        [DataMember]
        public string PlansJson { get; set; }

        [DataMember]
        public List<object> Plans { get; set; }
        [DataMember]
        public List<object> RemoteSiteCollections { get; set; }
        [DataMember]
        public List<object> MailBoxes { get; set; }

        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public bool IsDisableTemporaryAccount { get; set; }
        [DataMember]
        public bool IsManagerByPartner { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExpirationWarningMessageDto
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public bool IsExpiredAlready { get; set; }
    }


    public enum OnlineUserResultStatus
    {
        SUCC,
        FAIL
    }

    public enum OnlineUserActiveStatus
    {
        ACTIVE,
        DEACTIVE
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserPermission
    {

        [DataMember]
        public string ObjectId { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public List<EntityObjectPermissionType> Permission { get; set; }

        [DataMember]
        public EntityObjectPermissionType PermissionForJson { get; set; }

        public EntityObjectPermissionType GetPermission()
        {

            EntityObjectPermissionType permissionType = EntityObjectPermissionType.None;
            if (Permission == null)
            {
                return permissionType;
            }
            foreach (EntityObjectPermissionType objectPermissionType in Permission)
            {
                permissionType |= objectPermissionType;
            }
            return permissionType;
        }
    }
}

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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.TimeZone;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.Server.UserRegister;

namespace AvePoint.GCommon.Contract.Server.Login
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LoginResult
    {
        [DataMember]
        public LoginResultType Type { get; set; }
        [DataMember]
        public AccountMappingDto Account { get; set; }
        [DataMember]
        public byte[] CommunicationEncryptionKey { get; set; }
        [DataMember]
        public List<PermissionDto> Permissions { get; set; }
        [DataMember]
        public UserConfirmDto UserConfirm { get; set; }
        [DataMember]
        public SecurityTrimmingType SecurityTrimmingType { get; set; }
        [DataMember]
        public int CryptoMode { get; set; }
        [DataMember]
        public SystemSettingContent SystemSettingContent { get; set; }
        [DataMember]
        public UserRegisterSetting UserRegisterSetting { get; set; }
        [DataMember]
        public AppModule AppModule { get; set; }
        [DataMember]
        public CurrentUserModel PortalLoginInfo { get; set; }
        [DataMember]
        public Boolean GatewayCheck { get; set; }
        [DataMember]
        public Boolean SimpleLogin { get; set; }
        [DataMember]
        public Dictionary<string, int> CloudRolePublicPorts { get; set; }
        [DataMember]
        public EnviromentInfoDto EnviromentInfo { get; set; }
        [DataMember]
        public long PackageTime { get; set; }
#if DEBUG
        [DataMember]
        public bool IsDebugEnvironment { get; set; }
#endif
        [DataMember]
        public List<AveTimeZone> TimeZones { get; set; }

        [DataMember]
        /// <summary>
        /// Encryption Header for AccountInfo string
        /// </summary>
        public string AveRequestHeader { get; set; }

        [DataMember]
        public List<LicenseUnitDto> Licenses { get; set; }

        [DataMember]
        public string Info { get; set; }

        [DataMember]
        public List<UserGroup> UserGroups { get; set; }
        [DataMember]
        public InviteType InviteType { get; set; }

        [DataMember]
        public string ArchivingExtension { get; set; }

        [DataMember]
        public string ReleaseVersion { get; set; }
    }

    public class SimpleLoginResult
    {
        public LoginResultType ResultType { get; set; }
        public AccountMappingDto Account { get; set; }

        public SecurityTrimmingType SecurityTrimmingType { get; set; }

        public List<PermissionDto> Permissions { get; set; }

        public LanguageDto DisplayLanguage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LoginResultType
    {
        [EnumMember]
        UsernameOrPasswordIncorrect,
        [EnumMember]
        NotAuthorized,
        [EnumMember]
        Success,
        [EnumMember]
        NeedToChangePassword,
        [EnumMember]
        PasswordHasBeenUsed,
        [EnumMember]
        DomainNotAdded,
        [EnumMember]
        DomainLoginFailed,
        [EnumMember]
        GroupNotAdded,
        [EnumMember]
        HasBeenLocked,
        [EnumMember]
        NotExist,
        [EnumMember]
        AddressRestricted,
        [EnumMember]
        HasBeenDisabled,
        [EnumMember]
        AutoLoginFailed,
        [EnumMember]
        PasswordHasBeenExpeired,
        [EnumMember]
        BetaLicenseExpired,
        [EnumMember]
        MatchMaxAccountSessions,
        [EnumMember]
        NotActivation,
        [EnumMember]
        HasBeenExpired,
        [EnumMember]
        RegisterFailed,
        [EnumMember]
        InitDefaultSettingsFailed,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseAgreementResult
    {
        [DataMember]
        public byte[] CommunicationEncryptionKey { get; set; }
        [DataMember]
        public string LicenseAgreement { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InviteType
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Group = 1,
        [EnumMember]
        UserInGroup = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CurrentUserModel
    {
        [DataMember]
        public String UserId { get; set; }
        [DataMember]
        public String Username { get; set; }
        [DataMember]
        public String FirstName { get; set; }
        [DataMember]
        public String LastName { get; set; }
        [DataMember]
        public Boolean LegalPerson { get; set; }
        [DataMember]
        public String IdentityType { get; set; }
        [DataMember]
        public String CustomerId { get; set; }
        [DataMember]
        public String Country { get; set; }
        [DataMember]
        public String Company { get; set; }
        [DataMember]
        public List<RoleModel> Roles { get; set; }
        [DataMember]
        public String DataCenter { get; set; }
        [DataMember]
        public DateTime ExpireTime { get; set; }
        [DataMember]
        public int Type { get; set; } //for admin login=3
        [DataMember]
        public InviteType InviteType { get; set; }
        [DataMember]
        public List<UserGroup> UserGroups { get; set; }

        public ObjectRoleType RoleType
        {
            get
            {
                ObjectRoleType type = ObjectRoleType.Member;
                if (Type == 3)
                {
                    type = ObjectRoleType.DocAveSystem;
                }
                else
                {
                    var role = DocAveRole;
                    if (role != null)
                    {
                        switch (role.UserType)
                        {
                            case 0:
                                type = ObjectRoleType.Member;
                                break;
                            case 1:
                                type = ObjectRoleType.PowerUser;
                                break;
                            case 2:
                                type = LegalPerson ? ObjectRoleType.Owner : ObjectRoleType.PowerUser;
                                break;
                            default:
                                break;
                        }
                    }
                }
                return type;
            }
        }

        public String GetProductUrl(string product)
        {
            if (Roles != null && Roles.Count > 0)
            {
                var role = Roles.SingleOrDefault(r => string.Equals(r.ApplicationName, product, StringComparison.OrdinalIgnoreCase));
                if (role != null)
                {
                    return role.Url;
                }
            }
            return null;
        }

        //public bool IsExpire
        //{
        //    get
        //    {
        //        if (ExpireTime != null)
        //        {
        //            return ExpireTime.Ticks <= DateTime.UtcNow.Ticks;
        //        }
        //        return true;
        //    }
        //}

        public bool IsAcceptedLicenseAgreement
        {
            get
            {
                var role = DocAveRole;
                if (role != null)
                {
                    return role.IsAcceptedLicenseAgreement;
                }
                return true;
            }
            set
            {
                var role = DocAveRole;
                if (role != null)
                {
                    role.IsAcceptedLicenseAgreement = value;
                }
            }
        }

        public RoleModel DocAveRole
        {
            get
            {
                if (Type == 3)
                {
                    return null;
                }
                else if (Roles != null && Roles.Count > 0)
                {
                    return Roles.FirstOrDefault(r =>
                    string.Equals(r.ApplicationName, "DocAve", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.ApplicationName, "Office365Management", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.ApplicationName, "Office365Archiving", StringComparison.OrdinalIgnoreCase));

                }
                return null;
            }
        }
        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public long RegistrationTime { get; set; }

        [DataMember]
        public int SessionOutDuration { get; set; }

        [DataMember]
        public bool ForceLogined { get; set; }
    }

    public class UserGroup
    {
        public string DisplayName { get; set; }

        public string Id { get; set; }
    }

    public class PortalLoginModel
    {
        public String Username { get; set; }
        public String Product { get; set; }
        public String ExpiredTime { get; set; }
        public String ForwardControllerName { get; set; }
        public String ForwardActionName { get; set; }
        public String Signature { get; set; }

        public override String ToString()
        {
            return string.Format("{0}##{1}$${2}%%{3}^^{4}",
                this.Username,
                this.Product,
                this.ExpiredTime,
                this.ForwardControllerName ?? "home",
                this.ForwardActionName ?? "index");
        }
    }

    public struct MySettingPortalNavigationParameter
    {
        public string PortalUrl { get; set; }

        public MySettingPortalNavigationItemParameter[] Data { get; set; }
    }

    public struct MySettingPortalNavigationItemParameter
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RoleModel
    {
        [DataMember]
        public String ApplicationName { get; set; }
        [DataMember]
        public String Url { get; set; }
        [DataMember]
        public Boolean IsAcceptedLicenseAgreement { get; set; }
        [DataMember]
        public int UserType { get; set; }
    }
}

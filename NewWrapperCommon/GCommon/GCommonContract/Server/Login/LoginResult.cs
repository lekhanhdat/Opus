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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.AveLicense;

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
        public AccountStatusDto AccountStatus { get; set; }
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
        public BaseCredential Credential { get; set; }
        /// <summary>
        /// 为Ad user分处不同组而用
        /// </summary>
        [DataMember]
        public List<AccountMappingDto> GroupDtos { get; set; }
        [DataMember]
        public LicenseNotificationSetting LicenseNotificationSetting { get; set; }

        [DataMember]
        public LogonInfoItem LastLogonInfo { get; set; }
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
        MatchMaxGroups,
        [EnumMember]
        ShowLicenseAgreement,
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
}

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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    public class SystemRoleConstants
    {
        public const string GROUP_ADMINISTRATORS = "Administrators";
        public const string GROUP_DEFAULT_SECURITY_TRIMMING_GROUP = "Default Security Trimming Group";
        public const string ACCOUNT_ADMIN = "admin";
        public const string PERMISSION_FULL_CONTROL = "Full Control";
        public const string PERMISSION_DEFAULT_SECURITY_TRIMMING = "Default Security Trimming";
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Result
    {
        [EnumMember]
        AlreadyExisted,
        [EnumMember]
        NotExist,
        [EnumMember]
        Successful,
        [EnumMember]
        Failed,
        [EnumMember]
        ArgumentNull,
        [EnumMember]
        HasChildren,
        [EnumMember]
        IncorrectPassword,
        [EnumMember]
        NoPermission,
        [EnumMember]
        BeTiedUp,
        [EnumMember]
        GroupContainUser,
        [EnumMember]
        InitDefaultSettingsFailed,
        [EnumMember]
        SuccessfullNeedReloadPage

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ResultMessage
    {
        [DataMember]
        public Result ResultType { get; set; }

        //已经存在的user
        [DataMember]
        public List<AccountDto> AccountDtos { get; set; }

        //占用permission level的group
        [DataMember]
        public Dictionary<string, List<GroupDto>> DictionaryGroupContainPermLevel { get; set; }

        [DataMember]
        public int DeletePermissionLevelcount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionScope
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Global = 1,
        [EnumMember]
        Farm = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum AccountType : int
    {
        [EnumMember]
        Local = 1 << 0,
        [EnumMember]
        ActiveDirectory = 1 << 1,
        [EnumMember]
        SecurityTrimming = 1 << 2,
        [EnumMember]
        SuperAccount = 1 << 3,
        [EnumMember]
        ActiveDirectoryGroup = 1 << 4,
        [EnumMember]
        ADFS = 1 << 5,
        [EnumMember]
        WindowsAccount = 1 << 6,
        [EnumMember]
        WindowsGroup = 1 << 7,
        [EnumMember]
        RegisterUser = 1 << 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum GroupType : int
    {
        [EnumMember]
        SuperAdmin = 1 << 0,
        [EnumMember]
        ActiveDirectory = 1 << 1,
        [EnumMember]
        SecurityTrimming = 1 << 2,//add tenant group
        [EnumMember]
        Local = 1 << 3,//add system group
        [EnumMember]
        DedicatedTenantGroup = 1 << 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CheckADResultType
    {
        [EnumMember]
        NotFound = 0,   //  没有在Active Directory中check到
        [EnumMember]
        DomainNotAddedInDB = 1, //  数据库中还没有加入该domain
        [EnumMember]
        User = 2,
        [EnumMember]
        Group = 3,
    }

    public class AccountManagerEnumUtil
    {
        public static string ToString(GroupType type)
        {
            switch (type)
            {
                case GroupType.ActiveDirectory:
                    return "Active Directory Group";
                case GroupType.SecurityTrimming:
                    return "Security Trimming Group";
                case GroupType.Local:
                    return "Local Group";
                case GroupType.SuperAdmin | GroupType.Local:
                    return "Super Admin Group";
                case GroupType.ActiveDirectory | GroupType.SecurityTrimming:
                    return "Active Directory and SharePoint Integrated";
                default:
                    return string.Empty;
            }
        }

        public static string ToString(AccountType type)
        {
            switch (type)
            {
                case AccountType.ActiveDirectory:
                    return "Active Directory User";
                case AccountType.ActiveDirectoryGroup:
                    return "Active Directory Group";
                case AccountType.SuperAccount:
                    return "Super Admin";
                case AccountType.Local:
                    return "Local User";
                case AccountType.ActiveDirectory | AccountType.SecurityTrimming:
                    return "Active Directory and SharePoint Integrated";
                case AccountType.ADFS:
                    return "ADFS Claim";
                case AccountType.WindowsAccount:
                    return "Windows Account";
                case AccountType.WindowsGroup:
                    return "Windows Group";
                default:
                    return string.Empty;
            }
        }

        public static string ToString(PermissionScope scope)
        {
            return scope.ToString();
        }
    }


}

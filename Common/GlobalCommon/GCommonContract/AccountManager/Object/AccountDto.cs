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
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountDto
    {
        /// <summary>
        /// DocAve user的专有属性，只有是DocAve User这个属性才不为空，如果是AD User或ADFS Claim User,则这个属性为null.
        /// </summary>
        [DataMember]
        public LocalAccountDto LocalAccount { get; set; }

        [DataMember]
        public AccountMappingDto AccountMapping { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LocalAccountDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int Mode { get; set; }

        [DataMember]
        public string UserName { get; set; }

        //[DataMember]
        //public long LastActiveDate { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string NewPassword { get; set; }

        [DataMember]
        public string PwdQuestion { get; set; }

        [DataMember]
        public string PwdAnswer { get; set; }

        //[DataMember]
        //public string Email { get; set; }

        [DataMember]
        public bool IsLockOut { get; set; }

        [DataMember]
        public long LastLockOut { get; set; }

        [DataMember]
        public long LastPwdChange { get; set; }

        [DataMember]
        public int PwdAttemptCount { get; set; }

        [DataMember]
        public string RegisterID { get; set; }

        [DataMember]
        public string Extension { get; set; }

        //[DataMember]
        //public LocalUserStatus Status { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountMappingDto
    {
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Local user存储的是LocalAccountDto的id;
        /// AD user存储的是DomainDto的id.
        /// ADFS存储的是____.
        /// </summary>
        [DataMember]
        public string ObjectId { get; set; }

        /// <summary>
        /// 暂时只有AD user用到，存储的是Domain name
        /// </summary>
        [DataMember]
        public string DomainName { get; set; }

        /// <summary>
        /// Local user存储的是LocalAccountDto的UserName
        /// AD user存储的是AD的UserName(没有domain)
        /// ADFS存储的是____.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public AccountType Type { get; set; }

        [DataMember]
        public long CreateTime { get; set; }

        [DataMember]
        public long LastLogon { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public SecuritySetting SecuritySetting { get; set; }

        [DataMember]
        public List<GroupDto> Groups { get; set; }

        [DataMember]
        public List<PermissionDto> Permissions { get; set; }

        [DataMember]
        public AccountMode Mode { get; set; }

        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Account在当前Group的Role
        /// </summary>
        [DataMember]
        public ObjectRoleType Role { get; set; }

        /// <summary>
        /// 保存一些可以用二进制位表示的属性
        /// </summary>
        [DataMember]
        public long Extension2 { get; set; }

        [DataMember]
        public long ExpirationTime { get; set; }

        [DataMember]
        public int PurchasedUsers { get; set; }

        /// <summary>
        /// 保存用户的环境语言信息
        /// </summary>
        [DataMember]
        public AccountLanguageType Language { get; set; }

        public AccountMappingDto()
        {
            this.Groups = new List<GroupDto>();
            this.Permissions = new List<PermissionDto>();
        }

        public override string ToString()
        {
            return string.Format("AccountMappingDto[Id {0}, Name {1}]", Id, Name);
        }
    }

    /// <summary>
    /// DocAve Online register account detail
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountDetailDto
    {
        /// <summary>
        /// 必须是Guid
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string TenantGroupId { get; set; }

        /// <summary>
        /// register email
        /// </summary>
        [DataMember]
        public string AccountName { get; set; }

        /// <summary>
        /// utc time
        /// </summary>
        [DataMember]
        public DateTime RegistrationTime { get; set; }

        /// <summary>
        /// utc time
        /// </summary>
        [DataMember]
        public DateTime ExpirationTime { get; set; }

        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Tenant group内全部用户数量，包括register user
        /// </summary>
        [DataMember]
        public int LocalUserCount { get; set; }

        /// <summary>
        /// 小于0表示N/A
        /// </summary>
        [DataMember]
        public int PurchasedUserCount { get; set; }

        /// <summary>
        /// 小于0表示N/A
        /// </summary>
        [DataMember]
        public int ActualUserCount { get; set; }

        [DataMember]
        public AccountMode Status { get; set; }

        /// <summary>
        /// Account所在的Data Center service的Id
        /// </summary>
        [DataMember]
        public int ServiceId { get; set; }

        /// <summary>
        /// 该Tenant account下plan信息
        /// </summary>
        [DataMember]
        public List<PlanSummaryDto> PlanSummaries { get; set; }

        [DataMember]
        public LicenseType LicenseType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountExtensionSetting : long
    {
        [EnumMember]
        MindContact = 0,
        [EnumMember]
        NotMindContact = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace), Flags]
    public enum AccountMode
    {
        [EnumMember]
        Enabled = 0,
        [EnumMember]
        Disabled = 1 << 0,
        [EnumMember]
        Deleted = 1 << 1,
        [EnumMember]
        Expired = 1 << 2,
        [EnumMember]
        Hide = 1 << 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SecurityTrimmingType : int
    {
        [EnumMember]
        None = -1,

        [EnumMember]
        Regular = 0,

        [EnumMember]
        SecurityTrimming = 1,
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountLanguageType : int
    {
        [EnumMember]
        English = 0,

        [EnumMember]
        Japanese = 1,

        [EnumMember]
        French = 2,

        [EnumMember]
        Chinese_Simplified = 3,
    }
}

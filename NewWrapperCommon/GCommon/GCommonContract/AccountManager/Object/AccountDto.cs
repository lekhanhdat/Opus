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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

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

        public UserLockedOutDto LockedOutDetail()
        {
            return new UserLockedOutDto
            {
                Id = this.Id,
                UserName = this.UserName,
                IsLockOut = this.IsLockOut,
                LastLockOut = this.LastLockOut,
                LastPwdChange = this.LastPwdChange,
                PwdAttemptCount = this.PwdAttemptCount,
            };
        }
        //[DataMember]
        //public LocalUserStatus Status { get; set; }
    }

    public class UserLockedOutDto
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public bool IsLockOut { get; set; }

        public long LastLockOut { get; set; }

        public long LastPwdChange { get; set; }

        public int PwdAttemptCount { get; set; }
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
        public AccountMappingDto Parent { get; set; }

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

        public AccountMappingDto()
        {
            this.Groups = new List<GroupDto>();
            this.Permissions = new List<PermissionDto>();
        }
        public bool IsSystemRole()
        {
            return this.Type == AccountType.SuperAccount;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountMode
    {
        [EnumMember]
        Enabled = 0,

        /// <summary>
        /// 管理员操作将Account设置失效
        /// </summary>
        [EnumMember]
        Disabled = 1,

        [EnumMember]
        Deleted = 2,

        /// <summary>
        /// 系统操作，导致Account不可用
        /// </summary>
        [EnumMember]
        DisabledAutomatic = 3,
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

    public class AccountBatchImportDto
    {
        public AccountImportExcelType ExcelType { get; set; }
        public List<BatchImportDto> ImportDtos { get; set; }
        public AccountBatchImportDto()
        {
            ImportDtos = new List<BatchImportDto>();
        }
        public string Reason { get; set; }// 存储Excel本身有问题的原因（比如Excel 类型不正确或Excel被占用等）
    }

    public class BatchImportDto
    {
        public string UserName { get; set; }

        public string UserType { get; set; } //Standard User,Power User

        public string GroupName { get; set; }

        public string Reason { get; set; }  // 存储Excel内不满足条件的原因

        public List<CheckUsersResult> CheckUsersResults { get; set; }
    }

    public enum AccountImportExcelType
    {
        None = 0,
        TanentExcel = 1,
        SystemExcel = 2,
    }
}

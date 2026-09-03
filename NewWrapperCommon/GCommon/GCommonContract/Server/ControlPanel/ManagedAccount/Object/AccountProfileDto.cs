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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Aspect;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AccountProfileDto
    {
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// profile type
        /// </summary>
        [DataMember]
        public AccountProfileType Type { get; set; }

        /// <summary>
        /// 存储的是UserName
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string NewPassword { get; set; }

        [DataMember]
        public long LastModifyTime { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [DataMember]
        public string NotificationId { get; set; }

        [DataMember]
        public List<ScheduleDto> Schedules { get; set; }

        //VPAT
        public override string ToString()
        {
            return this.UserName;
        }

        //auto select
        public override bool Equals(object obj)
        {
            if (!(obj is AccountProfileDto))
            {
                return false;
            }
            else
            {
                AccountProfileDto other = (AccountProfileDto)obj;
                return string.Equals(this.Id, other.Id);
            }
        }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AccountProfileType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ActiveDirectoryAuthentication = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OpertionResult
    {
        [DataMember]
        public OpertionStatus Status { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public Dictionary<AccountProfileDto, List<CheckResult>> ValidateResultMapping { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OpertionStatus
    {
        [EnumMember]
        Failed = 0,
        [EnumMember]
        Successfull = 1,
        [EnumMember]
        NameExisted = 2,
        [EnumMember]
        CannotValidate = 3,
        /// <summary>
        /// 用户被锁了
        /// </summary>
        [EnumMember]
        UserLockedFailed = 7,
        /// <summary>
        /// 用户被disabled了
        /// </summary>
        [EnumMember]
        UserDisabledFailed = 8,
    }
}

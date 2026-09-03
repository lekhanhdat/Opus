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
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASecurityGrantTemporaryPermissionOperation : CASecurityUsersOperation
    {
        /// <summary>
        /// 此属性用于Server把当前时间传给Agent, Agent通过此属性去比较删除过期的Group
        /// </summary>
        [DataMember]
        public string ExpiredTime { get; set; }
        /// <summary>
        /// 此属性用于存储当前这个Farm中剩余的(还没过期)Temp Group个数
        /// </summary>
        [DataMember]
        public int TemporaryGroupCount { get; set; }
        /// <summary>
        /// 此属性用于存储删除Temp Group后的Job结果
        /// </summary>
        [DataMember]
        public List<CAGrantTemporaryPermissionResult> DeleteResult { get; set; }
        /// <summary>
        /// 此属性用于存储Grant Temp Permission的时候创建出来的Group名字
        /// </summary>
        [DataMember]
        public string GroupName { get; set; }

        /// <summary>
        /// 此属性存储过期前多少天给用户发邮件提醒
        /// </summary>
        //[DataMember]
        //public int ExpireAlertDay { get; set; } 

        /// <summary>
        /// 此属性用于标记temporary permission过期时间的时区
        /// </summary>
        [DataMember]
        public string TimeZone { get; set; }

        [DataMember]
        public bool IsDayLightSaving { get; set; }

        [DataMember]
        public List<CAGrantTemporaryPermissionEmailInfo> GrantTempPermissionEmailInfos { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAGrantTemporaryPermissionGroupInfo 
    {
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string ExpiredTime { get; set; }
        [DataMember]
        public NodeLevel Level { get; set; }
        [DataMember]
        public string SecurableObjectUrl { get; set; }
        [DataMember]
        public List<CAPermissionInfo> Permissions { get; set; }
        [DataMember]
        public List<UserDetail> Users { get; set; }
        [DataMember]
        public string SiteURL { get; set; }
        [DataMember]
        public string WebURL { get; set; }
        [DataMember]
        public bool SendEmail { get; set; }
        [DataMember]
        public string TimeZone { get; set; }
        [DataMember]
        public bool IsDayLightSaving { get; set; }
       
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAGrantTemporaryPermissionResult : ResultBase
    {
        [DataMember]
        public CAGrantTemporaryPermissionGroupInfo GroupInfo { get; set; }
        [DataMember]
        public GroupInfoStatus Status { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public CAStringFormatMessage FormatComment { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GroupInfoStatus
    {
        [EnumMember]
        Succeed,
        [EnumMember]
        Failed,
        [EnumMember]
        Skipped,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAGrantTemporaryPermissionEmailInfo
    {
        /// <summary>
        /// Delete时只有一个operation,需要用List存储Email地址
        /// </summary>
        [DataMember]
        public List<string> EmailTo { get; set; }

        [DataMember]
        public string SecurableObjectUrl { get; set; }

        [DataMember]
        public NodeLevel NodeLevel { get; set; }

        [DataMember]
        public List<string> PermissionName { get; set; }

        [DataMember]
        public TempPermissionAction Action { get; set; }

        [DataMember]
        public string ExpiredTime { get; set; }

        [DataMember]
        public string TimeZone { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TempPermissionAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Grant,
        [EnumMember]
        Edit,
        [EnumMember]
        Delete,
    }
}
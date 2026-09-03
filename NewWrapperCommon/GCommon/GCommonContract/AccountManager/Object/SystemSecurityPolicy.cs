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
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SystemSecurityPolicy : ISystemSettingContent
    {
        /// <summary>
        /// 同一个用户的最大session数
        /// </summary>
        [DataMember]
        public int MaximunSessions { get; set; }

        /// <summary>
        /// session过期时间
        /// </summary>
        [DataMember]
        public int SessionTimeOut { get; set; }

        /// <summary>
        /// session过期时间单位
        /// </summary>
        [DataMember]
        public PeriodType TimeOutType { get; set; }

        /// <summary>
        /// 允许登录的时间段
        /// </summary>
        [DataMember]
        public Dictionary<DayOfWeek, List<int>> LogOnTime { get; set; }

        /// <summary>
        /// 登录失败次数的限制
        /// </summary>
        [DataMember]
        public int FailedLogOnLimitationTimes { get; set; }

        /// <summary>
        /// 登录失败次数限制时间段
        /// </summary>
        [DataMember]
        public int FailedLogOnLimitationMinutes { get; set; }

        /// <summary>
        /// 不活跃用户的时间
        /// </summary>
        [DataMember]
        public int InactivePeriod { get; set; }

        /// <summary>
        /// 不活跃用户时间单位
        /// </summary>
        [DataMember]
        public PeriodType InactivePeriodType { get; set; }

        /// <summary>
        /// IP或subnet的黑/白名单
        /// </summary>
        [DataMember]
        public Dictionary<NetworkSecurityType, List<NetworkObj>> NetworkSecurity { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NetworkSecurityType
    {
        /// <summary>
        /// 信任网络
        /// </summary>
        [EnumMember]
        TrustedNetwork = 0,

        /// <summary>
        /// 受限网络
        /// </summary>
        [EnumMember]
        RestrictedNetwork = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NetworkObj
    {
        /// <summary>
        /// 类型(IP或subnet)
        /// </summary>
        [DataMember]
        public NetworkType Type { get; set; }

        /// <summary>
        /// IP或subnet的值
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NetworkType
    {
        [EnumMember]
        IP = 1,
        [EnumMember]
        Subnet = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PeriodType
    {
        [EnumMember]
        Minutes = 0,

        [EnumMember]
        Hours = 1,

        [EnumMember]
        Days = 2,

        [EnumMember]
        Month = 3,
    }
}

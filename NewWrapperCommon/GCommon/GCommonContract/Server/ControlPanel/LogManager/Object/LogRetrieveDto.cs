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

using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object
{
    /// <summary>
    /// 记录敏感信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LogRetrieveDto 
    {
        /// <summary>
        /// 敏感信息的类型
        /// </summary>
        [DataMember]
        public SensitiveType Type { get; set; }
        /// <summary>
        /// 记录各模块的返回的敏感信息
        /// </summary>
        [DataMember]
        public string OldString { get; set; }
        /// <summary>
        /// CP模块使用，各模块忽略此属性
        /// </summary>
        [DataMember]
        public string NewString { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SensitiveType
    {
        [EnumMember]
        Ip = 0,
        [EnumMember]
        UserName = 1,
        [EnumMember]
        Port = 2,
        [EnumMember]
        UncPath = 3,//暂时不用
        [EnumMember]
        Url = 4, //暂时不用
        [EnumMember]
        Host = 5,
        [EnumMember]
        IpOrHost = 6, //如果不确认其是IP还是Host则使用该类型
        [EnumMember]
        HostHeader = 7,//比如 http://www.baidu.com:5000/ ,返回 www.baidu.com即可
        [EnumMember]
        DataBaseName = 8,//SO模块添加的敏感信息类型 By Chunyang Xu
        [EnumMember]
        DomainName = 9,
    }
}

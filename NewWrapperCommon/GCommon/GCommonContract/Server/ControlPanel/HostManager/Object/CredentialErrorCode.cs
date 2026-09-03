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
using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.HostManager.Object
{
    [DataContract(Name = ContractConstants.Namespace)]
    public enum CredentialErrorCode
    {
        [EnumMember]
        NoError = 0,

        /// <summary>
        /// name is already exist
        /// </summary>
        [EnumMember]
        NameError = 1,

        /// <summary>
        /// 用户名或者密码错误。
        /// </summary>
        [EnumMember]
        AccountInfoError = 2,

        /// <summary>
        /// connect host failed, no available web service url
        /// 如果客户自定义了Web Service Url会出现这个情况。
        /// ProtocolPorts或者Host Address填错了.
        /// </summary>
        [EnumMember]
        NoAvailableWebServiceUrl = 3,

        /// <summary>
        /// connect host failed
        /// </summary>
        [EnumMember]
        ConnectFailed = 4,

        /// <summary>
        /// there is something wrong with agent
        /// </summary>
        [EnumMember]
        AgentError = 5,

        /// <summary>
        /// the selected agent is unavailable or failed to connect agent
        /// 没有可用agent或是和agent通信失败
        /// </summary>
        [EnumMember]
        UnAvailableAgent = 6,

        /// <summary>
        /// an unknown error occurred, for more details please refer to agent log
        /// </summary>
        [EnumMember]
        UnknownError = 7,

        /// <summary>
        /// hyperV agent已经创建过profile
        /// </summary>
        [EnumMember]
        ExistHyperVAgent = 8,

        /// <summary>
        /// Authentication information with the host type does not match the selected fill
        /// </summary>
        [EnumMember]
        HostTypeNotMatch = 9,

        /// <summary>
        /// VMware vCenter currently being used by the management
        /// </summary>
        [EnumMember]
        HostManagedByVCenter = 10,

        /// <summary>
        /// Hyper-V host does not in Hyper-V cluster
        /// </summary>
        [EnumMember]
        HostNotInCluster = 11,

        /// <summary>
        /// Host cluster service down
        /// </summary>
        [EnumMember]
        HostClusterServiceDown = 12,

        /// <summary>
        /// 原Cluster与新建Cluster不是同一个
        /// </summary>
        [EnumMember]
        HostClusterNotMatch = 13,

        /// <summary>
        /// Cluster Name已经创建过profile
        /// </summary>
        [EnumMember]
        ExistHyperVClusterName = 14,

        /// <summary>
        /// 不支持HyperV 2016的cluster
        /// </summary>
        [EnumMember]
        DoNotSupportHyperV2016Cluster = 16,
    }
}

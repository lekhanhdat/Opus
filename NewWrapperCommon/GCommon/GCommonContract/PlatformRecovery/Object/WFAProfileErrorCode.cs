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

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;

    /// <summary>
    /// WFA Profile的相关错误
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WFAProfileErrorCode
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        [EnumMember]
        Unknown = -1,

        /// <summary>
        /// 没有错误
        /// </summary>
        [EnumMember]
        NoError = 0,

        /// <summary>
        /// profile name已存在
        /// </summary>
        [EnumMember]
        NameError = 1,

        /// <summary>
        /// profile被占用
        /// </summary>
        [EnumMember]
        DeleteError = 2,

        /// <summary>
        /// 使用指定的URL查找WFA Server没有找到
        /// </summary>
        [EnumMember]
        URLNotFound = 3,

        /// <summary>
        /// 指定的WFA Server上没有找到导入的Workflow
        /// </summary>
        [EnumMember]
        WorkflowNotFound = 4,

        /// <summary>
        /// 指定的WFA Server上的Workflow版本和当前Manager中的版本不匹配
        /// </summary>
        [EnumMember]
        WorkflowVersionNotMatched
    }
}
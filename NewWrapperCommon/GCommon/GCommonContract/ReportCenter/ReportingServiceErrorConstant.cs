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

namespace AvePoint.Adonis.ReportCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportingServiceErrorConstant
    {
        [EnumMember]
        Normal,

        [EnumMember]
        NormalWithWarning,

        /// <summary>
        /// 无法连接到数据库
        /// </summary>
        [EnumMember]
        UnableConnectDataBase,

        /// <summary>
        /// 表示无法连接到报表服务器,可能的原因url输入不正确，服务器没有启动
        /// </summary>
        [EnumMember]
        UnableConnectReportingService,

        /// <summary>
        /// 请求不到report server服务器，但是后台没有判断出来
        /// </summary>
        [EnumMember]
        ReportServerOtherError,

        [EnumMember]
        ReportingServicePermissionDeny, 

        [EnumMember]
        SharepointPermissionDeny,

        /// <summary>
        /// 这个表示是在报表的制作过程中的错误，应该是数据库连接的问题，所以放在数据库连接那块
        /// </summary>
        [EnumMember]
        ProcessingError,

        /// <summary>
        /// 表示服务器存在，但是无法通过验证，用户权限不够或者是用户名密码错误
        /// </summary>
        [EnumMember]
        ReportServerUnauthorized,

        [EnumMember]
        InternalError,

        /// <summary>
        /// web service为05版本,使用ssrs2010endpoint config抛出的异常
        /// 遇到此异常重新使用05/06 endpoint再config一次
        /// </summary>
        [EnumMember]
        Sql05Error,
    }
}

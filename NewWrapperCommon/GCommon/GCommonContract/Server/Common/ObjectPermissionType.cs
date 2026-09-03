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

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// User对Entity(例如Plan Job等)的权限
    /// 权限可以组合
    /// </summary>
    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EntityObjectPermissionType
    {
        /// <summary>
        /// 没有任何权限
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// 可以用Entity执行动作，例如使用Plan Run Job
        /// </summary>
        [EnumMember]
        Execute = 1,

        /// <summary>
        /// 可以修改删除Entity
        /// </summary>
        [EnumMember]
        Write = 1 << 1,

        /// <summary>
        /// 可以查看Entity
        /// </summary>
        [EnumMember]
        Read = 1 << 2,

        /// <summary>
        /// 赋予其他用户操作Entity权限的权限
        /// 例如给Plan增加其他User的Read权限
        /// </summary>
        [EnumMember]
        Grant = 1 << 3,

        /// <summary>
        /// 其他全部权限的组合
        /// </summary>
        [EnumMember]
        FullPermission = Execute | Write | Read | Grant,
    }
}

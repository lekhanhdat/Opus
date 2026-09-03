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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb
{
    /// <summary>
    /// 注释中：User是还未添加到系统里的，而Account是已添加到系统里的
    /// </summary>
    public enum RMAccountStatus
    {
        None = 0,
        /// <summary>
        /// 可添加到系统Accounts的User
        /// </summary>
        Available = 1,
        /// <summary>
        /// 不是可添加到系统Accounts的User，例如在AD Domain中不存在的User
        /// </summary>
        Unavailable = 2,
        /// <summary>
        /// 可登录系统的Account
        /// </summary>
        Active = 3,
        /// <summary>
        /// Account所在的Domain已经被Disable
        /// </summary>
        Deactive = 4,
        /// <summary>
        /// Account所在的Domain已经被删除
        /// </summary>
        Delete = 5,
        /// <summary>
        /// 此User之前已经添加到Accounts里了
        /// </summary>
        Added = 6,
        /// <summary>
        /// User在此次要添加的User集合中重复出现
        /// </summary>
        Repeated = 7
    }

    public enum RMOperatingAccountError
    {
        None = 0,
        SamePassword,
        SavePasswordFailed
    }
    [DataContract]
    public enum RMActiveDirectoryObjectType
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Group = 1,
        [EnumMember]
        UserInGroup = 2,
        [EnumMember]
        PortalSupport = 3,
        [EnumMember]
        ProductSupport = 4,
    }
    [DataContract]
    public enum RMWorkflowType
    {
        [EnumMember]
        DisposalReview
    }
}
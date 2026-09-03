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

namespace AvePoint.Wrapper.Common
{
    public class AveWebObject
    {
        public Guid WebID { get; set; }
        public string Title { get; set; }
        public string FullUrl { get; set; }
        public string Name { get; set; }
        public bool NavigationChanged { get; set; }
        public DateTime EventTime { get; set; }
        public ChangeType ChangeType { get; set; }
        public List<AveSecurityObject> DeleteSecurities = new List<AveSecurityObject>();//存放permission及permission level的删除事件
        public bool IsAppWeb { get; set; }
        public Guid AppInstanceId { get; set; }
        /// <summary>
        /// 表示Role Assignments是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType RoleAssignmentsChangeType { get; set; }
        /// <summary>
        /// 表示permission level是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType PermissionLevelChangeType { get; set; }
        /// <summary>
        /// 表示content type是否改变
        ///
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType ContentTypeChangeType { get; set; }
        /// <summary>
        /// 表示Column是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType ColumnChangeType { get; set; }
        /// <summary>
        /// 表示Navigation是否有改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType NavigationChangeType { get; set; }

        public ChangeType ChangeTypeBeforeDelete { get; set; }
        public byte[] DeleteTransactionId { get; set; }//Just For Extender
    }
}


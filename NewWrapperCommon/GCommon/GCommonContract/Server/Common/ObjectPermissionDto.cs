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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// User在Ojbect上的权限
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ObjectPermissionDto
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        /// <summary>
        /// User在Ojbect上的权限，不同的Object类型有不同的值
        /// 
        /// Object Type: entity such as Plan Schedule etc...
        ///     EntityObjectPermissionType 权限可以组合
        /// </summary>
        [DataMember]
        public int Permission { get; set; }

        [DataMember]
        public List<EntityObjectPermissionType> PermissionList
        {
            get
            {
                var currentPermission = Permission;
                var permissions = new List<EntityObjectPermissionType>();
                if ((currentPermission & (int)EntityObjectPermissionType.Read) == (int)EntityObjectPermissionType.Read)
                {
                    permissions.Add(EntityObjectPermissionType.Read);
                }
                if ((currentPermission & (int)EntityObjectPermissionType.Write) == (int)EntityObjectPermissionType.Write)
                {
                    permissions.Add(EntityObjectPermissionType.Write);
                }
                if ((currentPermission & (int)EntityObjectPermissionType.Execute) == (int)EntityObjectPermissionType.Execute)
                {
                    permissions.Add(EntityObjectPermissionType.Execute);
                }
                if ((currentPermission & (int)EntityObjectPermissionType.Grant) == (int)EntityObjectPermissionType.Grant)
                {
                    permissions.Add(EntityObjectPermissionType.Grant);
                }
                return permissions;
            }
        }

        public ObjectPermissionScopeType PermissionScope { get; set; }
    }
}

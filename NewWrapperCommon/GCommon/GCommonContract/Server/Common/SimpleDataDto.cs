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



namespace AvePoint.GCommon.Contract.Server.Common
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleDataDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 该属性只有在SP07To10ImportTreeFilter页面使用 Added By [lwei][ADO-134491]
        /// 由于之前对Name进行国际化，导致比对Plan 创建时间出现问题，现增加此属性，用于显示国际化后的Instance Plan Name到页面
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// 该属性在Backup PlanManager页面Plan显示时,赋的值是PlanType.  
        /// 注释：如果其它模块使用情况不一样，请添加相应使用注释说明
        /// </summary>
        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        /// <summary> Front desk show users farm name. </summary>
        [DataMember]
        public string FarmDisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Category { get; set; }

        [DataMember]
        public long LastModifiedTime { get; set; }

        [DataMember]
        public bool IsBposPlan { get; set; }

        [DataMember]
        public List<NameAndIdDto> PlanGroups { get; set; }

        /// <summary>
        /// Object上User的Permissions
        /// </summary>
        [DataMember]
        public List<ObjectPermissionDto> ObjectPermissions { get; set; }

        /// <summary>
        /// 当前用户对Object的Permission
        /// </summary>
        [DataMember]
        public int ObjectPermission { get; set; }

        public bool IsShared
        {
            get
            {
                if (ObjectPermissions == null)
                {
                    throw new Exception("Object permission info is null");
                }
                int sharedCount = 0;
                foreach (var permission in ObjectPermissions)
                {
                    if (permission.PermissionScope == ObjectPermissionScopeType.User && permission.Permission > 0)
                    {
                        sharedCount++;
                    }
                }
                var isPlanShared = sharedCount > 1;
                return isPlanShared;
            }
        }
    }
}

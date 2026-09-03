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
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// SiteCollection中users和groups的还原选项。
    /// </summary>
    public class SPSecurityRestoreOption
    {
        /// <summary>
        /// restore security，控制是否还原security
        /// </summary>
        public bool RestoreSecurity { get; set; }

        /// <summary>
        /// SiteCollection中User和Group的还原控制
        /// </summary>
        public SPUserGroupRestoreOption UserGroupRestoreOption { get; set; }

        /// <summary>
        /// 还原RoleAssignment的选项。
        /// </summary>
        public SPRoleAssignmentsRestoreOption RoleAssignmentsRestoreOption { get; set; }


    }

    public class SPUserGroupRestoreOption
    {
        /// <summary>
        /// 是否还原user/group  属性
        /// </summary>
        public bool OverWrite { get; set; }

        /// <summary>
        /// 是否还原没有权限的user / group
        /// </summary>
        public bool SkipWithoutPermissions { get; set; }

        /// <summary>
        /// 是否还原覆盖目的端user 的administrator 属性
        /// </summary>
        public bool UpdateAdminSetting { get; set; }

        /// <summary>
        /// //是否将目的端已存在User，使用源端删除状态,默认为删除
        /// </summary>
        public bool UpdateDeletedSetting { get; set; }

        /// <summary>
        /// 外围通过该func过滤userInfo
        /// </summary>
        public Func<List<AveUserInfo>, List<AveUserInfo>> ProcessUserInfoBeforeRestore { get; set; }


        /// <summary>
        /// 还原Group前的操作
        /// </summary>
        public Func<List<AveGroupInfo>, List<AveGroupInfo>> ProcessGroupInfoBrforeRestore { get; set; }
    }

    /// <summary>
    /// SPRole Assignments Restore Option
    /// </summary>
    public class SPRoleAssignmentsRestoreOption
    {
        /// <summary>
        /// 是否还原Inherit状态
        /// </summary>
        public bool RestoreInheritance { get; set; }


        /// <summary>
        /// 源端是继承权限，目的端是独立权限，不还原继承状态情况下是否还原源端权限。
        /// </summary>
        public bool MergePermissionFromInheritance { get; set; }

        /// <summary>
        /// Filter Role Assignments
        /// </summary>
        public Func<List<AveRoleAssignmentInfo>, List<AveRoleAssignmentInfo>> FilterRoleAssignments { get; set; }

        /// <summary>
        /// object level的控制
        /// </summary>
        public SPRoleAssignmentsConflictResolution ConflictResolution { get; set; }

        /// <summary>
        /// 没用用户的权限
        /// </summary>
        public SPRoleAssignmentConflictResolution ConflictResolutionPerUser { get; set; }

    }

    /// <summary>
    /// 控制sp object的security还原选项，是merge还是删除再还原
    /// </summary>
    [Flags]
    public enum SPRoleAssignmentsConflictResolution
    {
        Merge = 0,
        OverWrite
    }

    /// <summary>
    /// 控制User的security还原选项，是merge还是删除再还原
    /// </summary>
    [Flags]
    public enum SPRoleAssignmentConflictResolution
    {
        Merge = 0,
        OverWrite
    }
}

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
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Permission
{
    public static class PermissionWrappers
    {
        public readonly static RMPermissionMasks StandardUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.PhysicalEndUser;
        public readonly static RMPermissionMasks ReviewUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.JobMonitorEnduser;
        public readonly static RMPermissionMasks StandardReviewUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.JobMonitorEnduser;
        public readonly static RMPermissionMasks HoldManagerUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.ManageHold | RMPermissionMasks.JobMonitorEnduser;
        
        public static string PackerPermissionsIntoString(this IEnumerable<RMPermissionMasks> permissions)
        {
            return ((long)permissions.Aggregate((i, j) => (i | j))).ToString();
        }

        public static RMPermissionMasks CombinePermissionsIntoString(this IEnumerable<long> permissions)
        {
            var permission = (permissions.Aggregate((i, j) => (i | j)));
            return (RMPermissionMasks)Enum.Parse(typeof(RMPermissionMasks), permission.ToString());
        }

        public static string PackerPermissionsIntoString(this IEnumerable<RMSubPermissionMasks> permissions)
        {
            return ((long)permissions.Aggregate((i, j) => (i | j))).ToString();
        }

        public static RMSubPermissionMasks CombineSubPermissionsIntoString(this IEnumerable<long> permissions)
        {
            var permission = (permissions.Aggregate((i, j) => (i | j)));
            return (RMSubPermissionMasks)Enum.Parse(typeof(RMSubPermissionMasks), permission.ToString());
        }

        public static string PackerPermissionsIntoString(this RMPermissionMasks permissions)
        {
            return ((long)permissions).ToString();
        }

        public static List<RMPermissionMasks> SplitPermission(this RMPermissionMasks permissions)
        {
            List<RMPermissionMasks> result = new List<RMPermissionMasks>();
            if (permissions == RMPermissionMasks.AccessAll)
            {
                foreach (var item in Enum.GetValues(typeof(RMPermissionMasks)))
                {
                    var permission = (RMPermissionMasks)item;
                    if (permission == RMPermissionMasks.AccessAll || permission == RMPermissionMasks.None) 
                    {
                        continue;
                    }
                    result.Add(permission);
                }
            }
            else
            {
                var permissionList = permissions.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var permission in permissionList)
                {
                    if (Enum.TryParse(permission, out RMPermissionMasks p))
                    {
                        result.Add(p);
                    }

                }

            }
            return result;
        }

        public static RMPermissionMasks UnpackPermissionsFromString(this string packedPermissions)
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            if (long.TryParse(packedPermissions, out long p))
            {
                return (RMPermissionMasks)Enum.Parse(typeof(RMPermissionMasks), p.ToString());
            }
            return RMPermissionMasks.None;
        }

        public static T UnpackPermissionsFromString2<T>(this string packedPermissions) where T : struct
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            if (long.TryParse(packedPermissions, out long p))
            {
                return (T)Enum.Parse(typeof(T), p.ToString());
            }
            return default;
        }

        public static RMPermissionMasks? FindPermissionViaName(this string permissionName)
        {
            return Enum.TryParse(permissionName, out RMPermissionMasks permission)
                ? (RMPermissionMasks?)permission
                : null;
        }

        public static int PermissionToRole(this string packedPermissions) 
        {
            var permission = UnpackPermissionsFromString(packedPermissions);
            RMRoleType roleType = RMRoleType.None;
            if (permission != RMPermissionMasks.None)
            {
                if (permission == StandardUser)
                {
                    roleType = RMRoleType.StandardUser;
                }
                else if (permission == ReviewUser)
                {
                    roleType = RMRoleType.ReviewUser;
                }
                else if (permission == HoldManagerUser)
                {
                    roleType = RMRoleType.ManageHoldUser;
                }
                else if (permission == StandardReviewUser)
                {
                    roleType = RMRoleType.StandardReviewUser;
                }
                else if (permission.HasFlag(RMPermissionMasks.ControlPanelAdmin))
                {
                    roleType = RMRoleType.ApplicationAdmin;
                }
                else
                {
                    roleType = RMRoleType.DeligatedAdmin;
                }
            }
            return (int)roleType;
        }

        public static int SOPermissionToRole(this string packedPermissions)
        {
            var permission = UnpackPermissionsFromString2<RMSOPermissionMasks>(packedPermissions);
            RMRoleType roleType = RMRoleType.None;
            if (permission != RMSOPermissionMasks.None)
            {
                if (permission.HasFlag(RMSOPermissionMasks.ControlPanelAdmin))
                {
                    roleType = RMRoleType.ApplicationAdmin;
                }
                else if(permission.HasFlag(RMSOPermissionMasks.ContentRepositoyEnduser))
                {
                    roleType = RMRoleType.DeligatedAdmin;
                }
            }
            return (int)roleType;
        }

        public static RMSubPermissionMasks UnpackSubPermissionsFromString(this string packedPermissions)
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            if (long.TryParse(packedPermissions, out long p))
            {
                return (RMSubPermissionMasks)Enum.Parse(typeof(RMSubPermissionMasks), p.ToString());
            }
            return RMSubPermissionMasks.None;
        }

        public static List<RMSubPermissionMasks> SplitPermission(this RMSubPermissionMasks permissions)
        {
            List<RMSubPermissionMasks> result = new List<RMSubPermissionMasks>();
            var permissionList = permissions.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var permission in permissionList)
            {
                if (Enum.TryParse(permission, out RMSubPermissionMasks p))
                {
                    result.Add(p);
                }

            }
            return result;
        }

    }
}

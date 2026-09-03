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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.SecurityTrimming
{
    public static class PermissionWrappers
    {
        public readonly static RMPermissionMasks StandardUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.PhysicalEndUser;
        public readonly static RMPermissionMasks ReviewUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.JobMonitorEnduser;
        public readonly static RMPermissionMasks HoldUser = RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.ManageHold | RMPermissionMasks.JobMonitorEnduser;
        public readonly static int BuildInAdminRoleId = 1;
        public readonly static int BuildInEndUserRoleId = 2;
        public static List<T> SplitPermission<T>(this T permissions) where T : struct
        {
            List<T> result = new List<T>();

            if (permissions.Equals(RMPermissionMasks.AccessAll))
            {
                var types = Enum.GetValues(typeof(T));
                foreach (var item in types)
                {
                    if (!item.Equals(RMPermissionMasks.AccessAll) && !item.Equals(RMPermissionMasks.None))
                    {
                        if (Enum.TryParse(item.ToString(), out T p))
                        {
                            result.Add(p);
                        }
                    }
                }
            }
            else if (permissions.Equals(RMPermissionExtensionMasks.AccessAll))
            {
                var types = Enum.GetValues(typeof(T));
                foreach (var item in types)
                {
                    if (!item.Equals(RMPermissionExtensionMasks.AccessAll) && !item.Equals(RMPermissionExtensionMasks.None))
                    {
                        if (Enum.TryParse(item.ToString(), out T p))
                        {
                            result.Add(p);
                        }
                    }
                }
            }
            else if (permissions.Equals(RMSOPermissionMasks.AccessAll))
            {
                var types = Enum.GetValues(typeof(T));
                foreach (var item in types)
                {
                    if (!item.Equals(RMSOPermissionMasks.AccessAll) && !item.Equals(RMSOPermissionMasks.None))
                    {
                        if (Enum.TryParse(item.ToString(), out T p))
                        {
                            result.Add(p);
                        }
                    }
                }
            }
            else if (permissions.Equals(RMReportPermissionMasks.AccessAll))
            {
                var types = Enum.GetValues(typeof(T));
                foreach (var item in types)
                {
                    if (!item.Equals(RMReportPermissionMasks.AccessAll) && !item.Equals(RMReportPermissionMasks.None))
                    {
                        if (Enum.TryParse(item.ToString(), out T p))
                        {
                            result.Add(p);
                        }
                    }
                }
            }
            else
            {
                var permissionList = permissions.ToString().Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var permission in permissionList)
                {
                    if (permission != null)
                    {
                        if (Enum.TryParse(permission, out T p))
                        {
                            result.Add(p);
                        }
                    }
                }
            }
            
            return result;
        }
        public static T CombinePermissions<T>(this IEnumerable<long> permissions)
        {
            var permission = (permissions.Aggregate((i, j) => (i | j)));
            return (T)Enum.Parse(typeof(T), permission.ToString());
        }

        public static T PackerPermissions<T>(this IEnumerable<T> permissions)
        {
            return permissions.Aggregate((i, j) => ((dynamic)i | (dynamic)j));
        }

        public static T Convert2Permission<T>(this string permission) where T : struct
        {
            List<T> result = new List<T>();
            var permissionNames = permission.Split(new String[1] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var field in permissionNames)
            {
                if (field != null)
                {
                    if (Enum.TryParse(field, out T p))
                    {
                        result.Add(p);
                    }
                }
            }
            return result.PackerPermissions();
        }
        public static T UnpackPermissionsFromString<T>(this string packedPermissions)
        {
            if (packedPermissions == null)
                throw new ArgumentNullException(nameof(packedPermissions));
            if (long.TryParse(packedPermissions, out long p))
            {
                return (T)Enum.Parse(typeof(T), p.ToString());
            }
            return default(T);
        }
        public static bool ThisPermissionIsAllowed<T>(this string packedPermissions, string permissionName) where T : struct
        {
            var usersPermissions = packedPermissions.UnpackPermissionsFromString<T>();

            if (!Enum.TryParse(permissionName, true, out T permissionToCheck))
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(T)}.");

            return usersPermissions.UserHasThisPermission(permissionToCheck);
        }

        public static bool UserHasThisPermission<T>(this T usersPermissions, T permissionToCheck)
        {
            return ((dynamic)usersPermissions & (dynamic)permissionToCheck) == permissionToCheck;
        }
       
    }
}

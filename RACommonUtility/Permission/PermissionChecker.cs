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

namespace AvePoint.RA.RACommonUtility.Permission
{
    public static class PermissionChecker
    {
        public static bool ThisPermissionIsAllowed(this string packedPermissions, string permissionName)
        {
            var usersPermissions = packedPermissions.UnpackPermissionsFromString();

            if (!Enum.TryParse(permissionName, true, out RMPermissionMasks permissionToCheck))
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(RMPermissionMasks)}.");

            return usersPermissions.UserHasThisPermission(permissionToCheck);
        }

        public static bool EqualsPermission(this string packedPermissions, string permissionName)
        {
            var usersPermissions = packedPermissions.UnpackPermissionsFromString();

            if (!Enum.TryParse(permissionName, true, out RMPermissionMasks permissionToCheck))
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(RMPermissionMasks)}.");

            return usersPermissions.EqualsThisPermission(permissionToCheck);
        }


        /// <summary>
        /// This is the main checker of whether a user permissions allows them to access something with the given permission
        /// </summary>
        /// <param name="usersPermissions"></param>
        /// <param name="permissionToCheck"></param>
        /// <returns></returns>
        public static bool UserHasThisPermission(this RMPermissionMasks usersPermissions, RMPermissionMasks permissionToCheck)
        {
            return (usersPermissions & permissionToCheck) == permissionToCheck;
        }

        public static bool EqualsThisPermission(this RMPermissionMasks usersPermissions, RMPermissionMasks permissionToCheck)
        {
            return usersPermissions == permissionToCheck;
        }

        #region SubPermission
        public static bool ThisSubPermissionIsAllowed(this string packedPermissions, string permissionName)
        {
            var usersPermissions = packedPermissions.UnpackSubPermissionsFromString();

            if (!Enum.TryParse(permissionName, true, out RMSubPermissionMasks permissionToCheck))
                throw new InvalidEnumArgumentException($"{permissionName} could not be converted to a {nameof(RMSubPermissionMasks)}.");

            return usersPermissions.UserHasThisSubPermission(permissionToCheck);
        }

        public static bool UserHasThisSubPermission(this RMSubPermissionMasks usersPermissions, RMSubPermissionMasks permissionToCheck)
        {
            return (usersPermissions & permissionToCheck) == permissionToCheck;
        }
        #endregion
    }
}

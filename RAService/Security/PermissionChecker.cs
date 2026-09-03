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
using AvePoint.RA.Common;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.DB.SecurityTrimming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Security
{
    public class PermissionChecker<T> where T : struct
    {
        public PermissionJoinType permissionJoinType { get; set; } = PermissionJoinType.And;
        private IRMSecurityTrimmingHelper trimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private T requiredPermission;

        public bool IsNonePermission { get; private set; }

        public bool LicenseEnable { get; private set; }

        public PermissionChecker(T requiredPermission, bool licenseEnable, PermissionJoinType permissionJoinType = PermissionJoinType.And)
        {
            this.requiredPermission = requiredPermission;
            this.permissionJoinType = permissionJoinType;
            this.LicenseEnable = licenseEnable;
            IsNonePermission = (dynamic)requiredPermission == default(T);
        }

        public async Task<bool> CheckPermissionAsync()
        {
            //if (IsNonePermission)
            //{
            //    return true;
            //}

            if (!LicenseEnable)
            {
                return false;
            }
            return await trimmingHelper.DoesUserHasThisPermissionAsync(requiredPermission, permissionJoinType);
        }
    }
}

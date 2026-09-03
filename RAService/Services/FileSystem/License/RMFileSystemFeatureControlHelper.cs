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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;

namespace AvePoint.RA.Service.Services.FileSystem.License
{
    public class RMFileSystemFeatureControlHelper
    {
        private static readonly IRMSecurityTrimmingHelper _securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static ITenantService s_tenantService => PlatformWindsorManager.GetService<ITenantService>();

        public static bool HasPermissionForJPMCFileSystemFeature()
        {
            return s_tenantService.IsNewOpusTenant()
                && _keyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableJPMCFileSystemFeature, false).GetAwaiter().GetResult()
                && _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin).GetAwaiter().GetResult();
        }
    }
}

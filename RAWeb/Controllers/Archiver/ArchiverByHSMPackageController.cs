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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AvePoint.RA.Web.Controllers.Archiver
{
    public class ArchiverByHSMPackageController : BaseApiController
    {
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);
        //[HttpPost]
        //public RAReturnMessage RunHSMArchiverJob([FromBody] HSMArchiverDto hsmDto)
        //{
        //    var permission1 = ((long)SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>().GetAwaiter().GetResult()).ToString();
        //    var soPermission = ((long)SecurityTrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>().GetAwaiter().GetResult()).ToString();
        //    int roleType = GetUserRoleType(permission1, soPermission);
        //    if (roleType == (int)RMRoleType.ApplicationAdmin)
        //    {
        //        return RMArchiverSettingsService.RunHSMArchiverJob(hsmDto, JobRunBy.Control);
        //    }
        //    else
        //    {
        //        return new RAReturnMessage() { FaildType = RAFailedType.AccessDenied };
        //    }
        //}
        private int GetUserRoleType(string opusILPermission, string opusSOPermission)
        {
            var roleType = opusILPermission.PermissionToRole();
            roleType = roleType > -1 ? roleType : opusSOPermission.SOPermissionToRole();
            return roleType;
        }
    }
}

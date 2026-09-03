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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AvePoint.RA.Web.Common.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class ValidSCWhitelistPermissionFilter : BaseAuthorizeAttribute
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidSCWhitelistPermissionFilter));

        private ITenantService _tenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRestoreSearchService _restoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();

        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            if (!_tenantService.IsNewOpusTenant())
            {
                logger.Error($"old logic account cann't use get sc whitelist");
                return false;
            }

            if (!_restoreSearchService.IsEnableFullTextIndexSearch())
            {
                logger.Error($"not enable full text index search");
                return false;
            }

            return true;
        }
    }
}

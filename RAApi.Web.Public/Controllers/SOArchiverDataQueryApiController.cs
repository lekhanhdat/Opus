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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/soArchiverDataQuery/[action]")]
    [ApiController]
    public class SOArchiverDataQueryApiController : RAWebApiBase
    {
        private static IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();

        [HttpPost]
        public Task<TenantArchiverDataInfo> GetArchivedDataInfo([FromBody] Dictionary<string, Guid> parameters)
        {
            return DashboardService.GetTenantArchivedDataInfo(parameters["o365TenantId"]);
        }

        [HttpPost]
        public Task<TenantArchiverDataInfo> GetArchiverDataInfoByType([FromBody]TenantArchiverDataRequest request)
        {
            return DashboardService.GetTenantArchivedDataInfo(request.TenantId, request.Type);
        }
    }
}

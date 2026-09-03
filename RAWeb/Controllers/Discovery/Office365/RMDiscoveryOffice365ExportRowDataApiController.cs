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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365;

[ValidateDiscoveryExportRowDataFilter]
public class RMDiscoveryOffice365ExportRowDataApiController : BaseApiController
{
    private readonly IRMDiscoveryOffice365ExportJobService _exportJobService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ExportJobService>();

    [HttpGet]
    public Task<RAReturnMessage> ExportRowDataJob()
    {
        return _exportJobService.ExportRowDataJobAsync();
    }
}
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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Archiver;
using Cloud.sdk.Data.Opus;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/archiverImportApi/[action]")]
    [ApiController]
    public class ArchiverImportApiController : RAWebApiBase
    {
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);

        [HttpPost]
        [APIScopeFilter(ContractConstants.RecordsPublicScope)]
        public HSMArchiverResult RunHSMArchiverJob([FromBody] HSMArchiverDto hsmDto)
        {
            return RMArchiverSettingsService.RunHSMArchiverJob(hsmDto, JobRunBy.Schedule);
        }

        [HttpPost]
        [APIScopeFilter(ContractConstants.RecordsPublicScope)]
        public HSMArchiverJobInfo GetHSMArchiverJobInfo([FromBody] string location)
        {
            return JobMonitorService.GetHSMArchiverJobInfo(location);
        }

        [HttpPost]
        [APIScopeFilter(ContractConstants.RecordsPublicScope)]
        public HSMArchvierJobDetailsResult GetHSMJobFailedDetails([FromBody] JMDetailsQuery queryModel)
        {
            return JobMonitorService.GetHSMJobFailedDetails(queryModel);
        }
    }
}

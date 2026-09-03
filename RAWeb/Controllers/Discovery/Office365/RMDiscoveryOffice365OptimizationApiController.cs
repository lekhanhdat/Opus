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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365OptimizationApiController : BaseApiController
    {
        private readonly IRMDiscoveryOffice365OptimizationService _optimizationService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365OptimizationService>();
        [HttpPost]
        public Task<RAReturnMessage> SaveOptimizationSetting([FromForm] IFormFile fileUp,[FromForm] string setting)
        {
            List<string> importUrls = new List<string>();
            bool useImportSite = false;
            RMDiscoveryOffice365OptimizationSetting internalSetting = JsonConvert.DeserializeObject<RMDiscoveryOffice365OptimizationSetting>(setting);
            if (fileUp != null)
            {
                useImportSite = true;
                string fileName = fileUp.FileName;
                Logger.Info("sp dso archiver import sites url file,file name :{0}.", fileName);
                string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
                if (extension.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    importUrls = ApiMessageUtil.GetArchiverImportSitesUrl(fileUp);
                }
            }
            return _optimizationService.SaveOptimizationSettingAsync(internalSetting, importUrls, useImportSite);
        }

        [HttpPost]
        public Task<RAReturnMessage> SaveOptimizationPreScanSetting([FromForm] IFormFile fileUp, [FromForm] string setting)
        {
            RMDiscoveryOffice365OptimizationSetting internalSetting = JsonConvert.DeserializeObject<RMDiscoveryOffice365OptimizationSetting>(setting);
            return _optimizationService.SaveOptimizationPreScanSettingAsync(internalSetting);
        }

        [HttpPost]
        public Task<RAReturnMessage> SaveDiscoveryPlanProOptimizationSetting([FromBody] RMDiscoveryPlanProfileJobRequest request)
        {
            return _optimizationService.SaveDiscoveryPlanProOptimizationSettingAsync(request.Profiles);
        }

        [HttpPost]
        public Task<RAReturnMessage> SaveDiscoveryPlanProScanSetting([FromBody] RMDiscoveryPlanProfileJobRequest request)
        {
            return _optimizationService.SaveDiscoveryPlanProScanSettingAsync(request.Profiles);
        }

    }

    public class RMDiscoveryPlanProfileJobRequest
    {
        public List<string> Profiles { get; set; }
    }
}

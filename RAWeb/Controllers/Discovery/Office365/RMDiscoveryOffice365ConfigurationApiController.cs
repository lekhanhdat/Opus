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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System.IO;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365ConfigurationApiController : BaseApiController
    {
        private readonly IRMDiscoveryOffice365ConfigurationService _configurationService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ConfigurationService>();

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        [HttpGet]
        public Task<RMDiscoveryOffice365ConfigurationInfo> GetConfigurationInfo()
        {
            return _configurationService.GetConfigurationInfoAsync();
        }

        [HttpPost]
        public async Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfo([FromBody] RMDiscoveryOffice365ConfigurationInfo discoveryConfigurationInfo)
        {
            return await _configurationService.AddOrUpdateNewlyConfigurationInfoAsync(discoveryConfigurationInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateAppendConfigurationInfo([FromBody] List<Guid> specifyContainerIds)
        {
            return _configurationService.AddOrUpdateAppendConfigurationInfoAsync(specifyContainerIds);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateRerunConfigurationInfo()
        {
            return _configurationService.AddOrUpdateRerunConfigurationAsync();
        }

        [HttpGet]
        public Task<List<RMRemoteNode>> GetNewlyAvaliableOpusContainers()
        {
            return _nodeDao.GetOpusContainersAsync();
        }

        [HttpGet]
        public Task<List<RemoteWebApplication>> GetAppendAvailableOpusContainer()
        {
            return _configurationService.GetAppendAvailableOpusContainerAsync();
        }

        [HttpGet]
        public Task<RMDiscoveryOffice365CostSavingInfo> GetCostSavingInfo()
        {
            return _configurationService.GetCostSavingInfoAsync();
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateCostSavingInfo([FromBody] RMDiscoveryOffice365CostSavingInfo costInfo)
        {
            return _configurationService.AddOrUpdateCostSavingInfoAsync(costInfo);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDiscoveryJobReport()
        {
            var filePath = await _configurationService.DownloadDiscoveryJobReportAsync();
            var stream = System.IO.File.Open(filePath, FileMode.Open);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = $"{I18NEntity.GetString("RM_FA_Discovery_Microsoft365_Report")}.csv"
            };
        }
    }
}

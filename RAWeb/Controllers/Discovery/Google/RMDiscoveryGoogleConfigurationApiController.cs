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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Web.Controllers.Discovery.Google
{
    [RMApiAuthorize(RMDiscoveryGoogleROTPermissionMask.AccessAll, preferred: false)]
    public class RMDiscoveryGoogleConfigurationApiController : BaseApiController
    {
        private readonly IRMDiscoveryGoogleConfigurationService _configurationService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleConfigurationService>();

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

        [HttpGet]
        public Task<RMDiscoveryGoogleConfigurationInfo> GetConfigurationInfo()
        {
            return _configurationService.GetConfigurationInfoAsync();
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfo([FromBody] RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            return _configurationService.AddOrUpdateNewlyConfigurationInfoAsync(configurationInfo);
        }

        [HttpGet]
        public Task<List<RMRemoteNode>> GetNewlyAvaliableOpusContainers()
        {
            return _nodeDao.GetOpusGoogleContainersAsync();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDiscoveryJobReport()
        {
            var filePath = await _configurationService.DownloadDiscoveryJobReportAsync();
            var stream = System.IO.File.Open(filePath, FileMode.Open);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = $"{I18NEntity.GetString("RM_FA_Discovery_Google_Report")}.csv"
            };
        }
    }
}

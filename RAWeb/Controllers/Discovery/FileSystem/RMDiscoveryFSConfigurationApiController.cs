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
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.FileSystem;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;

namespace AvePoint.RA.Web.Controllers.Discovery.FS
{
    //[RMApiAuthorize(RMPermissionMasks.FSAdmin, preferred: false)] //Add FS Discovery License
    [ValidFileSystemDiscoveryPermissionFilter]
    public class RMDiscoveryFSConfigurationApiController : BaseApiController
    {
        private readonly IRMDiscoveryFSNodeDao _nodeDao = new RMDiscoveryFSNodeDao();

        private readonly IRMDiscoveryFSConfigurationService _configurationService = PlatformWindsorManager.GetService<IRMDiscoveryFSConfigurationService>();

        [HttpGet]
        public List<FSConnectionGroup> GetNewlyAvaliableConnectionGroups()
        {
            return _nodeDao.LoadAllGroupsWithoutConnection();
        }

        [HttpGet]
        public Task<RMDiscoveryFSConfigurationInfo> GetConfigurationInfo()
        {
            return _configurationService.GetConfigurationInfoAsync();
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfo([FromBody] RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            return _configurationService.AddOrUpdateNewlyConfigurationInfoAsync(configurationInfo);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDiscoveryJobReport()
        {
            var filePath = await _configurationService.DownloadDiscoveryJobReportAsync();
            var stream = System.IO.File.Open(filePath, FileMode.Open);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = $"{I18NEntity.GetString("RM_FA_Discovery_FileSystem_Report")}.csv"
            };
        }
    }
}

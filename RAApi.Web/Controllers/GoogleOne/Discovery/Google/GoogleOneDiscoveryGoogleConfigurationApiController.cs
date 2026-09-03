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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.GoogleOne.Model.Discovery;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.Discovery.Google
{
    [Route("api/googleone/discovery/google/configuration")]
    public class GoogleOneDiscoveryGoogleConfigurationApiController : GoogleOneApiBaseController
    {
        private readonly IRMDiscoveryGoogleConfigurationService _configurationService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleConfigurationService>();

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao = new RMDiscoveryGoogleNodeDao();

        [HttpGet("getconfigurationinfo")]
        public async Task<RMDiscoveryGoogleConfigurationInfo> GetConfigurationInfo()
        {
            return await _configurationService.GetConfigurationInfoAsync();
        }

        [HttpGet("getavailablecontainers")]
        public async Task<List<RMRemoteNode>> GetAvailableContainers()
        {
            var containers = await _nodeDao.GetOpusGoogleContainersAsync();
            containers.ForEach(c =>
            {
                switch (c.Name)
                {
                    case RMConstants.DEFAULT_GOOGLE_USER_GROUP:
                        c.Name = I18N.Core.I18NEntity.GetString("RM_GoogleUser_Default_Container");
                        break;
                    case RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP:
                        c.Name = I18N.Core.I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
                        break;
                    default:
                        break;
                }
            });
            return containers;
        }

        [HttpPost("info/newly/addorupdate")]
        public async Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfo([FromBody] RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            return await _configurationService.AddOrUpdateNewlyConfigurationInfoAsync(configurationInfo);
        }

        [HttpPost("report/download")]
        public async Task<DiscoveryReportFileInfo> DownloadDiscoveryJobReport()
        {
            var filePath = await _configurationService.DownloadDiscoveryJobReportAsync();
            return new DiscoveryReportFileInfo
            {
                FileName = System.IO.Path.GetFileName(filePath),
                FileContent = System.IO.File.ReadAllBytes(filePath)
            };
        }
    }
}

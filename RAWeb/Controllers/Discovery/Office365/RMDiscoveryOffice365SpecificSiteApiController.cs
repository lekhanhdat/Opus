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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Archiver.Restore;
using AvePoint.RA.Service.Services.Discovery.Common;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    [ValidNewLogicAccount]
    [ValidNotTrialAccount]
    public class RMDiscoveryOffice365SpecificSiteApiController : BaseApiController
    {
        private readonly IRMDiscoverySpecificSiteService _siteService = new RMDiscoverySpecificSiteService();

        [HttpPost]
        public Task<DiscoverySpecificSiteInfo> GetExclusionListSitesByPagination([FromBody] DiscoverySpecificPageRequest request)
        {
            return _siteService.LoadM365ExclusionListSitesByPaginationAsync(request.PageIndex, request.PageSize);
        }

        [HttpPost]
        public RAReturnMessage AddExcludeSites([FromBody] IEnumerable<DiscoverySpecificSiteDto> sites)
        {
            return _siteService.AddM365ExcludeSites(sites);
        }

        [HttpPost]
        public RAReturnMessage RemoveExclusionListSites([FromBody] IEnumerable<int> ids)
        {
            return _siteService.RemoveM365ExclusionListSitesByIds(ids);
        }

        [HttpPost]
        public RAReturnMessage ImportExcludeSClist()
        {
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (extension != "csv")
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JS_JM_ImportFileFormatError") };
                }
                return _siteService.ImportExcludeSCList(file.OpenReadStream());
            }
            catch (Exception ex)
            {
                Logger.Info($"Fail request import ExcludeSCList,ex:{ex}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        [HttpPost]
        public RAReturnMessage ExportSCExcludelist()
        {
            return _siteService.ExportSCExcludelist();
        }
    }
}

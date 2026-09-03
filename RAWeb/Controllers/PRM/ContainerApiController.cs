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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class ContainerApiController: BaseApiController
    {
        private IContainerManagementService _ContainerManagementService;
        private IContainerManagementService ContainerManagementService => PlatformWindsorManager.GetService(ref _ContainerManagementService);


        [HttpPost]
        public string GetAllContainers()
        {
            return ContainerManagementService.GetAllContainers();
        }

        [HttpPost]
        public string SaveContainerType([FromBody]ContainerTypeInfo info)
        {
            return ContainerManagementService.SaveContainerType(info.TypeName, info.Size, info.Description, info.IsDefault);
        }

        [HttpPost]
        public Task<string> UpdateContainerType([FromBody]ContainerTypeInfo info)
        {
            return ContainerManagementService.UpdateContainerTypeAsync(info.ContainerId, info.TypeName, info.Size, info.Description, info.IsDefault);
        }

        [HttpPost]
        public Task<bool> UpdateContainerIsDefault([FromBody]ContainerTypeInfo info)
        {
            return ContainerManagementService.UpdateContainerIsDefaultAsync(info.ContainerId,info.IsDefault);
        }

        [HttpPost]
        public Task<bool> DeleteContainerType([FromBody]int containerId)
        {
            return ContainerManagementService.DeleteContainerTypeAsync(containerId);
        }

    }
}
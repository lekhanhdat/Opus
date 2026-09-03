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
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.SuperUser
{
    [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class SuperUserController : BaseApiController
    {
        private ISuperUserService _SuperUserService;
        private ISuperUserService SuperUserService => PlatformWindsorManager.GetService(ref _SuperUserService);
        [HttpPost]
        public SuperUserResult GetAllSuperUserConfigInfo([FromBody] SuperUserResult info)
        {
            info.SuperUserConfigurationDtoList = new List<SuperUserConfigurationDto>();
            return SuperUserService.GetAllTenantInfo(info);
        }

        [HttpPost]
        public async Task<int> SaveConfiguration([FromBody] SuperUserConfigurationDto dto)
        {
            if (SuperUserService.CheckConfigurationProfileExist(dto))
            {
                return await SuperUserService.UpdateConfigurationProfileAsync(dto);
            }
            else
            {
                return SuperUserService.CreateConfigurationProfile(dto);
            }
        }
    }
}

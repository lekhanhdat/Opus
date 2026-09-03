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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Archiver.Export;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AvePoint.RA.Web.Controllers.Export
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred: false, NeedNewOpusTenant = true)]
    public class ExportSettingController : BaseApiController
    {
        private ICompliantExportService _CompliantExportService;
        private ICompliantExportService StorageDeviceService => PlatformWindsorManager.GetService(ref _CompliantExportService); 

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public async Task<RAReturnMessage> SaveExportSetting([FromBody] ExportSettingsInfo exportInfo)
        {
            return await StorageDeviceService.SaveExportSetting(exportInfo);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public async Task<List<BaseExportInfo>> LoadExportSetting(ExportTypeValue type)
        {
            return await StorageDeviceService.LoadExportSetting(type);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public async Task<List<BaseExportInfo>> LoadAllExportSettings()
        {
            return await StorageDeviceService.LoadAllExportSettings();
        }
    }
}

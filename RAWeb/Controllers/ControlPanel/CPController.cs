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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ControlPanel
{
    [RACodeReview("Allen Yin")]
    [RMAuthorize(RMPermissionMasks.ControlPanelAdmin, preferred:false)]
    public class CPController : BaseController
    {
        private IExportSettingService _ExportSettingService;
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService(ref _ExportSettingService);
        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);
        private ISettingProfileService _SettingProfileService;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService(ref _SettingProfileService);
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private RALogger logger = RALogger.GetInstance(typeof(CPController));

        //GET: ControlPanel

        [HttpPost]
        [RMAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<JsonResult> ExportSettignsUploadCoinfig()
        {
            try
            {
                var isNewOpusTenant = TenantService.IsNewOpusTenant();

                var veoFileName = string.Empty;
                Stream veoFileStream = null;
                var file = Request.Form.Files["fileUp"];
                var veoIsNoChangeDirectSave = Request.Form["veoIsNoChangeDirectSave"];
                var needToUpgradeVEOV3 = bool.Parse(Request.Form["needToUpgradeVEOV3"]);

                if (file != null)
                {
                    CheckUpladFile(file);
                    veoFileName = Path.GetFileName(file.FileName);
                    veoFileStream = file.OpenReadStream();
                }

                var naaFileName = string.Empty;
                Stream naaFileStream = null;
                var nnaFile = Request.Form.Files["nnaFileUp"];
                var naaIsNoChangeDirectSave = Request.Form["naaIsNoChangeDirectSave"];

                if (nnaFile != null)
                {
                    CheckUpladFile(nnaFile);
                    naaFileName = Path.GetFileName(nnaFile.FileName);
                    naaFileStream = nnaFile.OpenReadStream();
                }

                //NARA
                var naraFileName = string.Empty;
                Stream naraFileStream = null;
                var naraFile = Request.Form.Files["naraFileUp"];
                var naraIsNoChangeDirectSave = Request.Form["naraIsNoChangeDirectSave"];

                if (naraFile != null)
                {
                    CheckUpladFile(naraFile);
                    naraFileName = Path.GetFileName(naraFile.FileName);
                    naraFileStream = naraFile.OpenReadStream();
                }
                var deviceId = Request.Form["exportLocationId"];
                if (deviceId != string.Empty)
                {
                    if (isNewOpusTenant)
                    {
                        var storageDto = StorageDeviceService.GetStorageDeviceById(deviceId);
                        if (storageDto != null)
                        {
                            await StorageDeviceService.SetUsingDeviceByIdAsync(storageDto.Id, SettingProfilesType.ExportLocationDevice);
                        }
                        else
                        {
                            throw new Exception("device id is not exist");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            await GlobalSettingService.SaveExportLocationInfoAsync(deviceId);
                        }
                    }
                }
                var enabled = Convert.ToBoolean(Request.Form["exportEncryptionEnabled"]);
                var enabledDatasum = Convert.ToBoolean(Request.Form["exportNARADataChecksumEnabled"]);
                var uploadResult = await ExportSettingService.UploadCoinfigAsync(veoFileName, veoFileStream, bool.Parse(veoIsNoChangeDirectSave), naaFileName, naaFileStream, bool.Parse(naaIsNoChangeDirectSave), naraFileName, naraFileStream, bool.Parse(naraIsNoChangeDirectSave), enabled, enabledDatasum, needToUpgradeVEOV3);
                await UpdateEnableNARAExportSignature(enabledDatasum);
                //这样判断是不合理的，这里处理更细一些，一个成功一个没有成功，应该是exception不是erro
                if (uploadResult)
                {
                    return Json(new { success = true, message = I18NEntity.GetString("RM_ES_SaveScuessfully") });
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (ExportConfigZipIllegalException ei)
            {
                logger.Error("the upload config file illegal, {0}", ei.ToString());
                return Json(new { success = false, message = ei.Message, details = ei.ToString() });
            }
            catch (Exception e)
            {
                logger.Error("upload file save error, {0}", e.ToString());
                return Json(new { success = false, message = I18NEntity.GetString("RM_ES_SaveError"), details = e.Message });
            }
        }

        private void CheckUpladFile(IFormFile file)
        {
            if (file != null)
            {
                var ext = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                WebUtil.CheckFileExtension(ext, new List<FileExtension> { FileExtension.ZIP });
                WebUtil.CheckFileSize(file.Length, 5);
            }
        }
        private async Task UpdateEnableNARAExportSignature(bool isEnable)
        {
            var profileDto = SettingProfileService.GetProfileDtoByType(SettingProfilesType.ExportSignatureInfo);
            if (profileDto != null)
            {
                var tempSetting = JsonSerializer.Deserialize<ExportSignatureInfo>(profileDto.Settings);
                tempSetting.EnableExportSignature = isEnable;
                profileDto.Settings = JsonSerializer.Serialize(tempSetting);
                await SettingProfileService.UpdateSettingAsync(profileDto);
            }
        }
    }
}
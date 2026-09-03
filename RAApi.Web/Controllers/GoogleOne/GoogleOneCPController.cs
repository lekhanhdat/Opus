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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Common.Util;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.AspNetCore.StaticFiles;
using AvePoint.RA.Contract.RMWeb.CP;
using System.Net;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/cp")]
    public class GoogleOneCPController : GoogleOneApiBaseController
    {
        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();
        public IExportDataEncryptionSettingService ExportDataEncryptionSettingService => PlatformWindsorManager.GetService<IExportDataEncryptionSettingService>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private RALogger logger = RALogger.GetInstance(typeof(GoogleOneCPController));

        [HttpPost("exportsetting/config/upload")]
        public async Task<String> ExportSettingsUploadConfig([FromBody] FileSettingInfo fileInfo)
        {
            try
            {
                var isNewOpusTenant = TenantService.IsNewOpusTenant();
                var naraFileName = fileInfo.FileName;
                var naraIsNoChangeDirectSave = fileInfo.IsNoChangeDirectSave;
                var naraFileStream = fileInfo.File;
                var enabledDatasum = fileInfo.IsEnableChecksum;
                var uploadResult = await ExportSettingService.UploadConfigAsyncForGoogleOne(naraFileName, naraFileStream, naraIsNoChangeDirectSave, enabledDatasum);
                await UpdateEnableNARAExportSignature(enabledDatasum);
                if (uploadResult)
                {
                    return "";
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (ExportConfigZipIllegalException ei)
            {
                logger.Error("the upload config file illegal, {0}", ei.ToString());
                return ei.Message;
            }
            catch (Exception e)
            {
                logger.Error("upload file save error, {0}", e.ToString());
                return I18NEntity.GetString("RM_ES_SaveError");
            }
        }

        [HttpGet("exportsettings/get")]
        public Task<ExportSettingEx> GetSavedFileInfos()
        {
            return ExportSettingService.GetSavedFileInfosAsyncForGoogleOne();
        }

        [HttpGet("exportsettings/savedFile/download")]
        public FileSettingInfo DownSavedloadNaraFile()
        {
                string filename;
                var stream = ExportSettingService.DownloadNARAConfigureFileToStream(out filename);
                stream.Position = 0;
                var file =  File(stream, GetContentType(filename), filename);
                return new FileSettingInfo
                {
                    FileName = file.FileDownloadName,
                    File = StreamToBytes(file.FileStream)
                }; 
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

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private byte[] StreamToBytes(Stream stream)
        {
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}

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
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace AvePoint.RA.Web.Controllers.BusinessClassification;

public class BCMAdminTeamsSettingApiController : BaseApiController
{
    private IRMTeamsSettingsService _teamsSettingsService => PlatformWindsorManager.GetService<IRMTeamsSettingsService>();
    [HttpPost]
    [ValidAccountHasTeamsPermissionFilter]
    [RMApiAuthorize(RMPermissionExtensionMasks.TeamsEndUser)]
    public RAReturnMessage ExportTeamsSetting([FromBody][BindRequired] ExportSettingType type)
    {
        return _teamsSettingsService.RunExportTeamsSettingJob(type, JobRunBy.Control);
    }

    [HttpPost]
    [ValidAccountHasTeamsPermissionFilter]
    [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
    public RAReturnMessage ExportTeamsSOSetting([FromBody][BindRequired] ExportSettingType type)
    {
        return _teamsSettingsService.RunExportTeamsSOSettingJob(type, JobRunBy.Control);
    }

    [HttpPost]
    [ValidAccountHasTeamsPermissionFilter]
    public string ImportTeamsSetting()
    {
        try
        {

            var file = Request.Form.Files["fileUp"];
            Logger.Info("teams setting import file,file name, error:{0}", file.FileName);
            CheckFile(file, FileExtension.CSV);
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            DateTime dateTimeNow = DateTime.Now;
            string fileName = "ImportTeamsSettings_" + dateTimeNow.Ticks.ToString() + ".csv";
            var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
            RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
            Logger.Info("save file success.");
            return _teamsSettingsService.RunImportTeamsSettingJob(JobRunBy.Control, extension, blobName);
        }
        catch (Exception ex)
        {
            Logger.Error("error occurred import teams setting data, error: {0}", ex.ToString());
            return string.Empty;
        }
    }

    [HttpPost]
    [ValidAccountHasTeamsPermissionFilter]
    [RMApiAuthorize(RMPermissionExtensionMasks.TeamsEndUser)]
    public IActionResult DownloadTeamsTemplate()
    {
        try
        {
            var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config",
                "Import Content Sources Settings for Teams.csv");
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }

            memoryStream.Position = 0;
            return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
        }
        catch
        {
            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
    }

    #region Private Method
    private void CheckFile(IFormFile file, FileExtension fileExtension)
    {
        string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
        var allowFileExts = fileExtension == FileExtension.CSV ? new List<FileExtension> { FileExtension.CSV } : new List<FileExtension> { FileExtension.XLSX };
        WebUtil.CheckFileExtension(extension, allowFileExts);
        WebUtil.CheckFileHeadCode(file.OpenReadStream(), allowFileExts);
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

    #endregion
}
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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class PhysicalRecordsBulkImportApiController : BaseApiController
    {
        //private RALogger mLogger = new RALogger(typeof(PRMController));
        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);
        private IPhysicalRecordsBulkImportService _PhysicalRecordsBulkImportService;
        private IPhysicalRecordsBulkImportService PhysicalRecordsBulkImportService => PlatformWindsorManager.GetService(ref _PhysicalRecordsBulkImportService);


        #region New Physical Import logic in GUI

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<ActionResult> DownloadTemplate()
        {
            string filepath = null;
            try
            {
                string suiteId = Request.Form["suiteId"];
                filepath = await PhysicalRecordsBulkImportService.DownloadTemplateForImportAsync(new Guid(suiteId));
                return File(StreamUtl.ReadFile(filepath), "application/octet-stream", Path.GetFileName(filepath));
            }
            catch(Exception e)
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
            finally
            {
                try
                {
                    if (filepath != null)
                    {
                        System.IO.File.Delete(filepath);
                    }
                }
                catch(Exception e)
                {
                    Logger.Error($"error occured when DownloadTemplate,error:{e}");
                }
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public string ImportData()
        {
            try
            {
                var conflict = Request.Form["conflictOption"];
                var customTime = Request.Form["enableCustomTime"];
                var file = Request.Form.Files["recordsFileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                CheckFile(file); 
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportRecord" + dt.Ticks.ToString() + "." + extension.ToLower();
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                int settingId = conflict == "0" ? 1 : 2;
                int customId = customTime == "0" ? 1 : 2;
                LocationManagementService.RunImportPhysicalFilesAndRecords(JobRunBy.Control, blobName, settingId, customId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }


        /// <summary>
        /// 导入Zip
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public async Task<string> ImportZipDataAsync()
        {
            var folderPath = "ImportRecord" + DateTime.Now.Ticks.ToString();
            Directory.CreateDirectory(folderPath);
            var zipFilePath = folderPath + JobMonitorConstants.ZIP;
            try
            {
                var files = Request.Form.Files;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        Logger.Info("tm import file,file name :{0}", file.FileName);
                        CheckCSVFile(file);
                        var tempfilePath = SecurityUtils.SafeCombinePath(folderPath, file.FileName);//Path.Combine(folderPath, file.FileName);
                        using var stream = new FileStream(tempfilePath, FileMode.Create);
                        await file.CopyToAsync(stream);
                    }
                    ZipUtil.ZipFolder(folderPath, zipFilePath, Encoding.UTF8);
                    Directory.Delete(folderPath, true);
                }

                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, zipFilePath);
                RAStorageUtil.UploadReportBlob(blobName, zipFilePath);
                System.IO.File.Delete(zipFilePath);
                int settingId = 2;//Overwrite
                LocationManagementService.RunImportPhysicalZipFilesAndRecords(JobRunBy.Control, blobName, settingId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }

        /// <summary>
        /// 导出Zip
        /// </summary>
        /// <param name="templateIds">用,分割</param>
        /// <returns></returns>
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin)]
        public string ExportZipData([FromBody]string templateIds)
        {
            try
            {
                LocationManagementService.RunExportPhysicalZipFilesAndRecords(JobRunBy.Control, templateIds);
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }

        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.XLSX };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            //WebUtil.CheckFileHeadCode(file.InputStream, allowFileExts);
        }

        /// <summary>
        /// 校验文件
        /// </summary>
        /// <param name="file"></param>
        private void CheckCSVFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            //WebUtil.CheckFileHeadCode(file.InputStream, allowFileExts);
        }
        #endregion
    }
}
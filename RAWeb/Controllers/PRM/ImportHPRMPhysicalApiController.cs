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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Import;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMApiAuthorize(RMPermissionMasks.PhysicalAdmin, preferred: false)]
    public class ImportHPRMPhysicalApiController : BaseApiController
    {
     

        private ILocationManagementService _LocationManagementService;
        private ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService(ref _LocationManagementService);
        private IImportTRIMService _ImportTRIMService;
        private IImportTRIMService ImportTRIMService => PlatformWindsorManager.GetService(ref _ImportTRIMService);


        [HttpPost]
        public async Task<string> ImportMetaDataOld()
        {
            try
            {
                var file = Request.Form.Files["metaFileUp"]; 
                Logger.Info("import records file, file name :{0}", file.FileName);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (!"xlsx".Equals(extension, StringComparison.OrdinalIgnoreCase) && !"csv".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("The file is not a 'CSV' or 'XLSX' file.");
                }
                Dictionary<string, List<string[]>> sheetDatas = new Dictionary<string, List<string[]>>();
                if ("xlsx".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        sheetDatas = ExcelUtil.ReadExcelWithHeader(file.OpenReadStream());
                        //datas = ExcelUtil.ReadExcel(file.InputStream, LocationSheetName, false);
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                        //if (e.ToString().Contains("Invalid Hyperlink"))
                        //{ 
                        //    UriFixer.FixInvalidUri(file.InputStream, brokenUri => UriFixer.FixUri(brokenUri));  
                        //    datas = ExcelUtil.ReadExcel(file.InputStream, LocationSheetName, false);
                        //}
                    } 
                    await ImportTRIMService.ImportMetaFileAsync(sheetDatas);
                } 
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }
        [HttpPost]
        public string ImportMetaData()
        {
            try
            {
                var file = Request.Form.Files["metaFileUp"];
                Logger.Info("import records file, file name :{0}", file.FileName);
                CheckFile(file);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportRecordMeta"+"." + extension.ToLower();
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }

        [HttpPost]
        public string ImportData()
        {
            try
            {
                var file = Request.Form.Files["recordsFileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                CheckFile(file);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportRecord" + dt.Ticks.ToString() + "." + extension.ToLower();
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                LocationManagementService.RunImportPhysicalFilesAndRecords(JobRunBy.Control, blobName, 0);
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }

        [HttpPost]
        public string ImportRelated()
        {
            try
            {
                var file = Request.Form.Files["relationFileUp"];
                Logger.Info("import records related file, file name :{0}", file.FileName);
                CheckFile(file);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                DateTime dt = DateTime.Now;
                string fileName = "ImportRecordTRIMRelationship" + ".csv";
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }


        [HttpPost]
        public string ImportDeletionData()
        {
            try
            {
                var file = Request.Form.Files["deletionFileUp"];
                Logger.Info("tm deletion file,file name :{0}", file.FileName);
                //CheckFile(file);  txt file, common method is checking csv and xlsx
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (!"txt".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("The file is not a 'txt'");
                }
                DateTime dt = DateTime.Now;
                string fileName = "ImportDeletionRecord" + dt.Ticks.ToString() + "." + extension.ToLower();
                var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
                RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
                ImportTRIMService.StartDeletionJob(blobName); 
            }
            catch (Exception ex)
            {
                Trace.TraceError("error occurred import data:{0}", ex.ToString());
                return ex.Message;
            }
            return "ok";
        }
        [HttpPost]
        public string RelatedBaseOnPhysical()
        {
            ImportTRIMService.RunImportRecordsRelated(JobRunBy.Control, "ImportRecordTRIMRelationship.csv", 0);
            return "ok";
        }

        [HttpPost]
        public string RelatedBaseOnElectronic()
        {
            ImportTRIMService.RunImportRecordsRelated(JobRunBy.Control, "ImportRecordTRIMRelationship.csv", 1);
            return "ok";
        }

        [HttpPost]
        public string ClearSubFolder()
        { 
            return ImportTRIMService.ClearSubFolders();
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public IActionResult DownloadSubFolderList()
        {
            var downloadCSVFile = "PhysicalRecordsSubFolderList" + ".csv";
            var fileName = ImportTRIMService.DownloadSubFolderList(downloadCSVFile);
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            var ContentType = GetContentType(fileName);
            return File(memoryStream, ContentType, downloadCSVFile);

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

        private void CheckFile(IFormFile file)
        {
            string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            var allowFileExts = new List<FileExtension> { FileExtension.CSV, FileExtension.XLSX };
            WebUtil.CheckFileExtension(extension, allowFileExts);
            //WebUtil.CheckFileHeadCode(file.InputStream, allowFileExts);
        }
    }
}
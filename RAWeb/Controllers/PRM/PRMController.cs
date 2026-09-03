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

using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;



namespace AvePoint.RA.Web.Controllers.PRM
{
    [RMAuthorize(RMPermissionMasks.PhysicalAdmin)]
    public class PRMController : BaseController
    {
        //private RALogger mLogger = new RALogger(typeof(PRMController));

        #region import physical record
        [HttpPost]
        public JsonResult ImportData()
        {
            string jobId = "";
            var file = Request.Form.Files["fileUp"];
            int settingId = 0;
            long fileSize = file.Length;
            string fileType = file.FileName;
            fileType = Path.GetExtension(fileType);
            if (fileSize / (1024 * 1024) > 20 || fileType != ".csv")
            {
                return Json(new { id = jobId }, "text/html");
            }
            if (Request.Form.ContainsKey("hSettingId"))
            {
                settingId = Convert.ToInt32(Request.Form["hSettingId"]);
            }
            DateTime dt = DateTime.Now;
            string fileName = dt.Ticks.ToString() + ".csv";
            var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, fileName);
            RAStorageUtil.UploadReportBlob(blobName, file.OpenReadStream());
            //jobId = LocationManagementService.RunImportPhysicalFilesAndRecords(JobRunBy.Control, blobName, settingId);
            return Json(new { id = jobId }, "text/html");
        }

        #endregion
      
    }
}
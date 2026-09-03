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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RecordManager.Controllers.BusinessClassification
{
    /// <summary>
    /// have reviewed by allen yin 
    /// </summary>
    /// 
    [RMApiAuthorize(RMPermissionMasks.TermManagementEnduser, preferred: false)]
    public class TermSynchronizationApiController : BaseApiController
    {
        private IRMSharePointTaxonomyService _TermSynchronization;
        private IRMSharePointTaxonomyService TermSynchronization => PlatformWindsorManager.GetService(ref _TermSynchronization);
        private IManualApprovalService _ManualApprovalService;
        private IManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService(ref _ManualApprovalService);
        private IScheduleService _RMScheduleService;
        private IScheduleService RMScheduleService => PlatformWindsorManager.GetService(ref _RMScheduleService);
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.TermManagementAdmin)]
        public async Task<RAReturnMessage> RunSync([FromBody]bool fromTimerJobPage)
        {
            RAReturnMessage reMsg = await TermSynchronization.RunSyncRMTermTreeToSharePointAsync(JobRunBy.Control, fromTimerJobPage);
            return reMsg;
        }

        [HttpPost]
        [ValidScheduleSettingActionFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public async Task<string> GetScheduleByType([FromBody]ScheduleType type)
        {
            List<ScheduleInfo> Schedule = await RMScheduleService.GetScheduleByTypeServiceAsync(type);
            return JsonConvert.SerializeObject(Schedule);
        }

        [HttpPost]
        [ValidScheduleSettingActionFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<string> CreateSchedule([FromBody]ScheduleInfo info)
        {
            info.Id = Guid.NewGuid().ToString();
            //ProcessExtensionValue(info);
            return RMScheduleService.CreateScheduleServiceAsync(info);            
        }

        [HttpPost]
        [ValidScheduleSettingActionFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<string> UpdateScheduleService([FromBody]ScheduleInfo info)
        {
            //ProcessExtensionValue(info);
            return RMScheduleService.UpdateScheduleServiceAsync(info);
        }

        [HttpPost]
        [ValidScheduleSettingActionFilter]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public void DeleteScheduleService([FromBody]string Id)
        {
            RMScheduleService.DeleteScheduleService(Id);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewAdmin)]
        public void RunManualApprovalJob()
        {
            ManualApprovalService.RunManualApprovalJob(JobRunBy.Control);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewAdmin)]
        public string RunManualApprovalTimerJob()
        {
            ManualApprovalService.RunManualApprovalTimerJob(JobRunBy.Control);
            return string.Empty;
        }


        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [RMApiAuthorize(RMPermissionMasks.ManualReviewEnduser)]
        public async Task<HttpResponseMessage> DownLoadReport()
        {
            var serverUrl = !string.IsNullOrEmpty(Request.Headers.Host) ? $"https://{Request.Headers.Host}" : "";
            HttpResponseMessage response = new HttpResponseMessage();
            DateTime nowTime = DateTime.UtcNow;
            string nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            string fileName = I18NEntity.GetString("RM_DAM_ManualApprovalReviewReport") + "_" + nowTimeStr;
            string folderPath = JobReportUtility.GetDownloadManualApprovalReviewReportTempleFolder("Temple") + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(
                folderPath, fileName + ".xlsx");
            await ManualApprovalService.GenerateReportForManualApprovalReviewingAsync(reportFilePath, fileName, I18NEntity.GetString("RM_DAM_ManualApprovalReview"), serverUrl);
            AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            FileTransferStream resultStream = new FileTransferStream(folderPath + ".zip", folderPath, FileMode.Open);
            response.Content = new StreamContent(resultStream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = WebUtil.ConvertFileName(fileName + ".zip");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response.Content.Headers.ContentLength = resultStream.Length;
            return response;
        }

      
        [DataContract]
        public class ManualReviewStatus
        {
            [DataMember]
            public int status { get; set; }
            [DataMember]
            public List<int> ids { get; set; }
            [DataMember]
            public ExtendDispositionType ExtendDispositionType { get;set;}
            [DataMember]
            public string ExtendDispositionCustomTime { get; set; }
            [DataMember]
            public string ExtendDispositionComment { get; set; }
        }
        [DataContract]
        public enum ExtendDispositionType
        {
            [EnumMember]
            None = 0,
            [EnumMember]
            ThreeMonths = 1,
            [EnumMember]
            SixMonths = 2,
            [EnumMember]
            OneYear = 3,
            [EnumMember]
            Custom = 4
        }

    }
}
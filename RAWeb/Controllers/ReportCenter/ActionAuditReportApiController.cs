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
using AngleSharp.Io;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMPermissionMasks.ReportCenterAdmin, RMSOPermissionMasks.TeamsEndUser | RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser, RMReportPermissionMasks.ActionAuditEnduser, preferred: false)]
    public class ActionAuditReportApiController : BaseApiController
    {
        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);

        private ITeamsSettingTreeService _TeamsSettingTreeService;
        private ITeamsSettingTreeService TeamsSettingTreeService => PlatformWindsorManager.GetService(ref _TeamsSettingTreeService);

        [HttpPost]
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }

        [HttpPost]
        public string GetReportJobFilterData([FromBody] ShowReportQuery query)
        {
            return JsonConvert.SerializeObject(RMReportService.GetReportJobFilterData(query));
        }

        [HttpPost]
        public async Task<string> GetProfileReport([FromBody] ShowProfilesReportPageInfo pageInfo)
        {
            ShowProfilesReportPageInfo result = await RMReportService.GetProfilesAsync(pageInfo);
            foreach (var profile in result.Profiles)
            {
                profile.Extension1 = null;
                profile.Extension2 = null;
            }

            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        public string GenerateReport([FromBody] RMProfileDto profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (profile.Type != JobType.SPOActionAuditReport && profile.Type != JobType.OneDriveActionAuditReport 
                && profile.Type != JobType.TeamsActionAuditReport)
            {
                throw new ArgumentException("profile.Type");
            }
            return RMReportService.StartReportJob(profile.Type, profile.Id);
        }

        [HttpPost]
        public async Task<string> CreateProfile([FromBody]RMProfileDto profile)
        {
            try
            {
                CheckParameters(profile);
                var spFarm = SPSettingTreeService.LoadFarm()[0];
                //TeamsSettingTreeService.LoadFarm()[0];
                //profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                profile.Extension3 = string.IsNullOrEmpty(profile.Extension3) ? profile.Extension3 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);
            }
            catch (System.Exception)
            {
                Logger.Error("Build Tree XML Error: {0}", profile.Extension2);
                return "Parameter exception";
            }
            try
            {
                RAReturnMessage returnMessage = await RMReportService.BuildProfileAsync(profile);
                if (returnMessage.MessageType == RAMessageType.Failed)
                {
                    Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                    return returnMessage.ErrorMessage;
                }
                await UpdateReportScheduleAsync(returnMessage.Extsion1 as RMProfileDto, false);
                Logger.Info("create profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create action audit profile or schedule, Error:{0}", ex);
                return "Failed to create profile schedule.";
            }
        }

        [HttpPost]
        public async Task<RMProfileDto> LoadProfileById([FromBody] string Id)
        {
            var profileDto = await RMReportService.GetProfileByIdAsync(Id);
            if (!string.IsNullOrWhiteSpace(profileDto.ScheduleId))
            {
                try
                {
                    profileDto.scheduleInfo = await ScheduleService.GetScheduleByIdAsync(profileDto.ScheduleId);
                }
                catch
                {
                    profileDto.scheduleInfo = null;
                }
            }
            if (!string.IsNullOrWhiteSpace(profileDto.Extension3))
            {
                profileDto.Extension3 = SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension3);
            }
            ValidReportUtil util = new ValidReportUtil();
            if (profileDto.Type == JobType.SPOActionAuditReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredSPTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.OneDriveActionAuditReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredOneDriveTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            else if (profileDto.Type == JobType.TeamsActionAuditReport)
            {
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredTeamsTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }
            return profileDto;
        }

        [HttpPost]
        public async Task<List<string>> DeleteProfiles([FromBody]DelProfileInfo dpi)
        {
            Dictionary<int, string> deleteJobProfileNames = new Dictionary<int, string>();
            List<string> CanNotdeleteJobProfileNames = new List<string>();

            for (var i = 0; i < dpi.Ids.Count; i++)
            {
                deleteJobProfileNames.Add(dpi.Ids[i], dpi.Names[i]);
            }
            dpi.ProfileNames = deleteJobProfileNames;
            (_, CanNotdeleteJobProfileNames)  = await RMReportService.DeleteProfilesAsync(dpi);
            return CanNotdeleteJobProfileNames;
        }

        [HttpPost]
        public async Task<string> EditProfile([FromBody]RMProfileDto profile)
        {
            CheckParameters(profile);
            try
            {
                var spFarm = SPSettingTreeService.LoadFarm()[0];
                //profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension2, spFarm.FarmId);
                profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                profile.Extension3 = string.IsNullOrWhiteSpace(profile.Extension3) ? null : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);
            }
            catch (System.Exception e)
            {
                Logger.Info("Build Tree XML:{0}, Error: {1}", profile.Extension2, e.ToString());
                return "Parameter exception";
            }
            try
            {
                RAReturnMessage returnMessage = await RMReportService.EidtProfileAsync(profile);
                if (returnMessage.MessageType == RAMessageType.Failed)
                {
                    Logger.Error("an error occurred while create profile,name:{1},type:{2},ERROR:{0}", returnMessage.ErrorMessage, profile.ProfileName, profile.Type);
                    return returnMessage.ErrorMessage;
                }
                await UpdateReportScheduleAsync(profile, true);
                Logger.Info("edit profile success,name:{0},Type:{1}", profile.ProfileName, profile.Type);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to edit action audit profile or schedule, Error:{0}", ex);
                return "Failed to update profile schedule.";
            }
        }

        [RACodeReview("Allen Yin", comment: "没有删临时文件")]
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<IActionResult> DownloadFile()
        {
            try
            {
                string jobId = "";
                string profileName = "";
                HttpContext context = this.HttpContext;
                jobId = HttpUtility.UrlDecode(context.Request.Form["jobId"]);
                profileName = HttpUtility.UrlDecode(context.Request.Form["profileName"]);

                BaseJobDto baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.SPOActionAuditReport, ProfileName = profileName };
                await RMReportService.GenerateReportAsync(baseJobDto);
                var filename = JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto) + ".zip";
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                // set the position to return the file from
                memoryStream.Position = 0;
                return GetValidatedFile(memoryStream, GetContentType(filename), Path.GetFileName(filename));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
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

        private async Task UpdateReportScheduleAsync(RMProfileDto profile, bool isEdit)
        {
            if (profile == null)
            {
                return;
            }

            if (profile.scheduleInfo == null || profile.scheduleInfo.NoSchedule)
            {
                if (isEdit && !string.IsNullOrWhiteSpace(profile.ScheduleId))
                {
                    ScheduleService.DeleteScheduleService(profile.ScheduleId);
                    await RMReportService.UpdateProfileScheduleIdAsync(profile.Id, null);
                }

                profile.ScheduleId = null;
                return;
            }

            profile.scheduleInfo.JobCategory = ScheduleType.SPOActionAuditReport;
            profile.scheduleInfo.ProfileId = profile.Id.ToString();
            if (string.IsNullOrWhiteSpace(profile.scheduleInfo.Id) || profile.scheduleInfo.Id == "1")
            {
                profile.scheduleInfo.Id = Guid.NewGuid().ToString();
            }
            string scheduleId;
            if (string.IsNullOrWhiteSpace(profile.scheduleInfo.Id) || profile.scheduleInfo.Id == "1")
            {
                profile.scheduleInfo.Id = Guid.NewGuid().ToString();
            }
            if (isEdit)
            {
                if (string.IsNullOrWhiteSpace(profile.scheduleInfo.Id))
                {
                    profile.scheduleInfo.Id = profile.ScheduleId;
                }

                scheduleId = await ScheduleService.UpdateScheduleServiceAsync(profile.scheduleInfo);
            }
            else
            {
                scheduleId = await ScheduleService.CreateScheduleServiceAsync(profile.scheduleInfo);
            }

            if (string.IsNullOrWhiteSpace(scheduleId) || scheduleId == "-1")
            {
                throw new System.InvalidOperationException("Failed to create or update report schedule.");
            }

            profile.ScheduleId = scheduleId;
            profile.scheduleInfo.Id = scheduleId;
            await RMReportService.UpdateProfileScheduleIdAsync(profile.Id, profile.ScheduleId);
        }
        private void CheckParameters(RMProfileDto profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException("profile");
            }

            if (profile.Type != JobType.SPOActionAuditReport && profile.Type != JobType.OneDriveActionAuditReport && profile.Type != JobType.TeamsActionAuditReport)
            {
                throw new ArgumentException("profile.Type");
            }
            ClientAuditReportDto mClientAuditReportDto = string.IsNullOrEmpty(profile.Extension1) ? null : SerializerHelper.DeserializeByJsonConvert<ClientAuditReportDto>(profile.Extension1);
            if (mClientAuditReportDto == null)
            {
                throw new ArgumentException("profile.Extension1");
            }
            //if (mClientAuditReportDto.TimeFilterMode == TimeRangeType.None)
            //{
            //    throw new ArgumentException("profile.Extension1");
            //}
            if (profile.RangeType == TimeRangeType.None)
            {
                throw new ArgumentNullException("profile.RangeType");
            }
            if (profile.RangeType == TimeRangeType.Custom)
            {
                if (mClientAuditReportDto.StartDateTime == null || mClientAuditReportDto.EndDateTime == null)
                {
                    throw new ArgumentException("profile.Extension1");
                }
                DateTime mStartDateTime, mEndDateTime;
                if (DateTime.TryParse(mClientAuditReportDto.StartDateTime, out mStartDateTime) && DateTime.TryParse(mClientAuditReportDto.EndDateTime, out mEndDateTime))
                {
                    if (mStartDateTime > mEndDateTime)
                    {
                        throw new ArgumentException("profile.Extension1");
                    }
                }
                else
                {
                    throw new ArgumentException("profile.Extension1");
                }
            }
            if (mClientAuditReportDto.ObjType == 0 || mClientAuditReportDto.ActionType == 0)
            {
                throw new ArgumentException("profile.Extension1");
            }

            if (mClientAuditReportDto.UserScope == UserScopeSettings.None)
            {
                throw new ArgumentException("profile.Extension1");
            }
            else if (mClientAuditReportDto.UserScope == UserScopeSettings.SpecificUsers && (mClientAuditReportDto.userInfos == null || mClientAuditReportDto.userInfos.Count == 0))
            {
                throw new ArgumentException("profile.Extension1");
            }
            if (mClientAuditReportDto.TreeScope == TreeModeSettings.None)
            {
                throw new ArgumentException("profile.Extension1");
            }
            else if (mClientAuditReportDto.TreeScope == TreeModeSettings.SpecificSites)
            {
                if (string.IsNullOrEmpty(profile.Extension2))
                {
                    throw new ArgumentException("profile.Extension2");
                }
                SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(profile.Extension2, true);
            }

            //if (profile.Source != Contract.Explorer.SourceFlag.OneDrive || profile.Source != Contract.Explorer.SourceFlag.SharePoint)
            //{
            //    throw new ArgumentException("profile.Source");
            //}
        }
    }
}

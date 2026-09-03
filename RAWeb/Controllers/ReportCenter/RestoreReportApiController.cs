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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser | RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.GoogleAdmin, joinType: PermissionJoinType.Any)]
    public class RestoreReportApiController : BaseApiController
    {

        private IRMReportService _RMReportService;
        private IRMReportService RMReportService => PlatformWindsorManager.GetService(ref _RMReportService);
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();


        [HttpPost]
        [ValidCreateReportProfileParameterActionFilter]
        public async Task<string> CreateProfile([FromBody] RMProfileDto profile)
        {
            try {
                profile.Extension3 = string.IsNullOrEmpty(profile.Extension3) ? profile.Extension3 : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);
                switch (profile.Type)
                {
                    case JobType.RestoreReport:
                    case JobType.OneDriverRestoreReport:
                    case JobType.TeamsRestoreReport:
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                        break;
                    case JobType.GoogleRestoreReport:
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                        break;
                    default:
                        Logger.Error("profile type error: {0}", profile.Type);
                        return "Parameter exception";
                }
            }
            catch (System.Exception)
            {
                Logger.Info("Build Tree XML Error: {0}", profile.Extension2);
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
                Logger.Error("Failed to create restore profile or schedule, Error:{0}", ex);
                return "Failed to create profile schedule.";
            }
        }

        [HttpPost]
        [ValidEditReportProfileParameterActionFilter]
        public async Task<string> EditProfile([FromBody] RMProfileDto profile)
        {
            try
            {
                switch (profile.Type)
                {
                    case JobType.RestoreReport:
                    case JobType.OneDriverRestoreReport:
                    case JobType.TeamsRestoreReport:
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : SerializerHelper.SerializeByDataContractSerializer(SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(profile.Extension2));
                        break;
                    case JobType.GoogleRestoreReport:
                        profile.Extension2 = string.IsNullOrEmpty(profile.Extension2) ? profile.Extension2 : RuleSPTreeUtil.ConvertGoogleTreeJsonStrToListStr(profile.Extension2);
                        break;
                    default:
                        Logger.Error("profile type error: {0}", profile.Type);
                        return "Parameter exception";
                }
                profile.Extension3 = string.IsNullOrWhiteSpace(profile.Extension3) ? null : SPTreeUtil.BuildSPTreeXMLStr(profile.Extension3);
            }
            catch (System.Exception)
            {
                Logger.Info("Build Tree XML Error: {0}", profile.Extension2);
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
                Logger.Error("Failed to edit restore profile or schedule, Error:{0}", ex);
                return "Failed to update profile schedule.";
            }
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

            profile.scheduleInfo.JobCategory = ScheduleType.RestoreReport;
            profile.scheduleInfo.ProfileId = profile.Id.ToString();

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
        [HttpPost]
        [ValidReportIdParameterActionFilter]
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
            if (profileDto.Type == JobType.OneDriverRestoreReport)
            {
                ValidReportUtil util = new ValidReportUtil();
                profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredOneDriveTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
            }else
            {
                switch(profileDto.Type)
                {
                    case JobType.TeamsRestoreReport:
                        {
                            ValidReportUtil util = new ValidReportUtil();
                            profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredTeamsTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
                        }
                        break;
                    case JobType.GoogleRestoreReport:
                        profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : RuleSPTreeUtil.BuildGoogleTreeJsonStr(profileDto.Extension2);
                        break;
                    default:
                        {
                            ValidReportUtil util = new ValidReportUtil();
                            profileDto.Extension2 = string.IsNullOrEmpty(profileDto.Extension2) ? profileDto.Extension2 : await util.GetFilteredSPTreeNodesAsync(SPTreeUtil.ConvertXmlStrToSPTreeJsonStr(profileDto.Extension2), profileDto.Type);
                        }
                        break;
                }
            }
            return profileDto;
        }


        [HttpPost]
        [ValidDeleteReportProfileParameterActionFilter]
        public async Task<List<string>> DeleteProfiles([FromBody] DelProfileInfo dpi)
        {
            Dictionary<int, string> deleteJobProfileNames = new Dictionary<int, string>();
            List<string> CanNotdeleteJobProfileNames = new List<string>();

            for (var i = 0; i < dpi.Ids.Count; i++)
            {
                deleteJobProfileNames.Add(dpi.Ids[i], dpi.Names[i]);
            }
            dpi.ProfileNames = deleteJobProfileNames;
            (_, CanNotdeleteJobProfileNames) = await RMReportService.DeleteProfilesAsync(dpi);
            return CanNotdeleteJobProfileNames;
        }

        [HttpPost]  //获取profile列表
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
        public async Task<string> GetProfileReportByGenerateReportId([FromBody] int profileId)
        {
            ShowProfilesReportPageInfo pageInfo = new ShowProfilesReportPageInfo
            {
                PageIndex = 1,
                PageSize = 15,
                TotalCount = 0,
                Type = JobType.RestoreReport,
                IsDesc = true,
                Profiles = null,
                SearchValue = null
            };
            int pageIndex = RMReportService.GetPageIndexByProfileId(profileId);
            pageInfo.PageIndex = pageIndex;
            ShowProfilesReportPageInfo result = await RMReportService.GetProfilesAsync(pageInfo);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidReportProfileParameterActionFilter]
        public string GenerateReport([FromBody] RMProfileDto profile)
        {
            return RMReportService.StartReportJob(profile.Type, profile.Id);
        }

        [ValidShowReportQueryPagerActionFilter]   //report详情页   等朝翰哥
        public Task<string> ShowReportQueryPager([FromBody] ShowReportQuery query)
        {
            if (query.ReportJobType == JobType.RestoreReport || query.ReportJobType == JobType.OneDriverRestoreReport || query.ReportJobType == JobType.TeamsRestoreReport || query.ReportJobType == JobType.GoogleRestoreReport)
            {
                query.ReportJobType = JobType.GenerateRestoreReport;
            }
            RMReportService.SetRestoreReportDisplayMod(true);
            return RMReportService.GetCommonReportJobDatasAsync(query);
        }


    }
}

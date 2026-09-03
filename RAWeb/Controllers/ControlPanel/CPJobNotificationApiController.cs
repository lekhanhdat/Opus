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
using Aspose.Pdf.Operators;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Service.RMTasks;
using AvePoint.RA.Service.Services.Schedule;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ControlPanel
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, preferred: false)]
    public class CPJobNotificationApiController : BaseApiController
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(CPJobNotificationApiController));

        private readonly IRMReportService RMReportService = PlatformWindsorManager.GetService<IRMReportService>();

        private readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IScheduleService ScheduleService = PlatformWindsorManager.GetService<IScheduleService>();

        [HttpPost]
        public async Task<List<JobNotificationResult>> GetAllProfiles()
        {
            try
            {
                var result = await RMReportService.GetJobNotificationProfiles();
                if (result != null && result.Count != 0)
                {
                    var profiles = await Task.WhenAll(result.OrderByDescending(r => r.Modified).ConvertAll(ConvertToJobNotificationProfile));
                    return [.. profiles];
                }
                return [];
            }
            catch (Exception e)
            {
                s_logger.Error($"Get job notification profile failed, error : {e}");
                return [];
            }
        }

        [HttpPost]
        [ValidJobNotificationParameterFilter("CreateProfile")]
        public async Task<RAReturnMessage> CreateProfile([FromBody] JobNotificationDto jobNotificationInfo)
        {
            return await RMReportService.BuildJobNotificationProfileAsync(jobNotificationInfo);
        }

        [HttpPost]
        [ValidJobNotificationParameterFilter("EditProfile")]
        public async Task<RAReturnMessage> EditProfile([FromBody] JobNotificationDto jobNotificationInfo)
        {
            return await RMReportService.EditJobNotificationProfileAsync(jobNotificationInfo);
        }

        [HttpPost]
        public RAReturnMessage DeleteProfile([FromBody] List<string> profileIds)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                RMReportService.DeleteJobNotificationProfile(profileIds.ConvertAll(int.Parse));
                returnMessage.MessageType = RAMessageType.Successful;
            }
            catch(Exception e)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }

            return returnMessage;
        }

        [HttpPost]
        public async Task<JobNotificationResult> GetProfile([FromBody] string profileId)
        {
            try
            {
                var result = await RMReportService.GetProfileByIdAsync(profileId);
                return await ConvertToJobNotificationProfile(result);
            }
            catch(Exception e)
            {
                s_logger.Error($"Get job notification profile by profile id failed, error: {e}");
            }
            return new();
        }

        [HttpPost]
        public async Task<bool> RunJobNotificationSchedule()
        {
            await new ProcessJobEmailNotificationExecutor().ExecutorAsync();
            return true;
        }

        private async Task<JobNotificationResult> ConvertToJobNotificationProfile(RMProfileDto profile)
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            var result = SerializerHelper.DeserializeByDataContractSerializer<JobNotificationDto>(profile.Extension1);
            return new()
            {
                ProfileId = profile.Id,
                ProfileName = result.ProfileName,
                ProfileCreatedTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, long.Parse(result.ProfileCreatedTime), true).SimplifyFormatTime,
                ProfileDes = result.ProfileDes,
                ProfileEmailReceivers = result.ProfileEmailReceivers,
                ProfileInterval = result.ProfileInterval,
                ProfileJobInfos = result.ProfileJobInfos
            };
        }

        private async Task CreateJobNotificationSchedule()
        {
            var jobNotificationSchedule = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.JobNotificationSchedule);
            if (jobNotificationSchedule != null && jobNotificationSchedule.Count > 0)
            {
                return;
            }
            var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
            var info = new ScheduleInfo
            {
                Id = Guid.NewGuid().ToString()
            };

            var utcNow = DateTime.UtcNow;
            var globalTimeZoneId = (await generalSetting).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
            localNow = localNow.AddDays(1);

            var startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            info.StartTime = startTime.ToString();
            info.EndTime = startTime.ToString();
            info.EndType = 0;
            info.Interval = 1;
            info.IntervalType = IntervalType.Daily;
            info.JobCategory = ScheduleType.JobNotificationSchedule;
            info.OccurrencesTotal = 1;
            info.TimeZoneId = (await generalSetting).TimeZoneId;
            await ScheduleService.CreateScheduleServiceAsync(info);
        }
    }
}

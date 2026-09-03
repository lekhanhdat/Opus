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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidJobNotificationParameterFilter : BaseActionFilter
    {
        private readonly IRMReportService RMReportService = PlatformWindsorManager.GetService<IRMReportService>();

        private readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private string action;

        public ValidJobNotificationParameterFilter()
        {

        }

        public ValidJobNotificationParameterFilter(string action)
        {
            this.action = action;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var jobNotificationDto = actionContext.ActionArguments.Values.FirstOrDefault() as JobNotificationDto;
			AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(jobNotificationDto);
            if (jobNotificationDto?.ProfileJobInfos.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            var result = await RMReportService.GetJobNotificationProfiles();
            if(result != null)
            {
                if (result.Count == 0)
                {
                    return;
                }

                var profiles = await Task.WhenAll(result.OrderByDescending(r => r.Modified).ConvertAll(ConvertToJobNotificationProfile));
                var busyJobTypes = new List<int>();
                if (action.Equals("CreateProfile"))
                {
                    foreach (var item in profiles)
                    {
                        busyJobTypes.AddRange(item.ProfileJobInfos.Select(job => (int)job.JobType));
                    }
                }
                else if (action.Equals("EditProfile"))
                {
                    foreach (var item in profiles)
                    {
                        if(item?.ProfileId != jobNotificationDto?.ProfileId)
                        {
                            busyJobTypes.AddRange(item?.ProfileJobInfos.Select(job => (int)job.JobType));
                        }
                    }
                }

                var currentJobTypes = jobNotificationDto?.ProfileJobInfos.Select(job => (int)job.JobType);
                if(busyJobTypes.Intersect(currentJobTypes).Any())
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
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
    }
}

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
using Aspose.Words.Lists;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Razor.Language;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidScheduleSettingActionFilter : BaseActionFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidScheduleSettingActionFilter));
        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        public ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();

        private static HashSet<ScheduleType> _CommonScheduleTypes = new () {
            ScheduleType.ArchiverDedupJobSchedule,
            ScheduleType.StubDisposalSchedule
        };

        private static HashSet<ScheduleType> _SOOnlyScheduleTypes = new() {
            ScheduleType.ArchiveDataRetentionSchedule,
            ScheduleType.ArchiverDeleteRestoredDataSchedule,
            ScheduleType.ApprovalProcessJob
        };

        private static HashSet<ScheduleType> _GoogleScheduleTypes = new()
        {
            ScheduleType.GoogleArchiveJobSchedule,
            ScheduleType.GoogleDataSyncSchedule,
            ScheduleType.GoogleDisposalSchedule,
            ScheduleType.GoogleSettingSchedule
        };

        private static HashSet<ScheduleType> _GoogleOnlyScheduleTypes = new()
        {
            ScheduleType.SyncSchedule,
            ScheduleType.GoogleArchiveJobSchedule,
            ScheduleType.GoogleDataSyncSchedule,
            ScheduleType.GoogleDisposalSchedule,
            ScheduleType.GoogleSettingSchedule,
            ScheduleType.ArchiverDedupJobSchedule,
            ScheduleType.ArchiveDataRetentionSchedule,
        };

        public ValidScheduleSettingActionFilter()
        {
        }

        protected override Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            ScheduleType scheduleType = ScheduleType.None;
            if (parmObj is ScheduleInfo scheduleInfo)
            {
                scheduleType = scheduleInfo.JobCategory;
            }
            else if (parmObj is string scheduleSettingId)
            {
                var schedule = RMScheduleDao.GetSchedule(scheduleSettingId);
                if (schedule != null)
                {
                    scheduleType = (ScheduleType)schedule.JobCategory;
                }
            }
            else if (parmObj is ScheduleType paramSchedueType)
            {
                scheduleType = paramSchedueType;
            }

            if(scheduleType != ScheduleType.None)
            {
                if (!ValidateScheduleSetting(scheduleType))
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }

        private bool ValidateScheduleSetting(ScheduleType scheduleType)
        {
            var res = true;

            var hasOpusILLicense = LicenseHelperService.HasOpusILLicense;
            var hasOpusSOLicense = LicenseHelperService.HasOpusSOLicense;
            var hasOpusGoogleLicense = LicenseHelperService.HasOpusGoogleLicense;
            if (!((hasOpusILLicense || hasOpusGoogleLicense) && hasOpusSOLicense))
            {
                if (hasOpusILLicense)
                {
                    if (IsSOOnlyScheduleType(scheduleType))
                    {
                        Logger.Warn($"ScheduleType {scheduleType} is not allowed for ILLicense");
                        return false;
                    }
                }

                if (hasOpusSOLicense)
                {
                    if (!IsSOScheduleType(scheduleType))
                    {
                        Logger.Warn($"ScheduleType {scheduleType} is not allowed for SOLicense");
                        return false;
                    }
                }
            }

            if (!hasOpusGoogleLicense)
            {
                if (IsGoogleScheduleType(scheduleType))
                {
                    Logger.Warn($"ScheduleType {scheduleType} is not allowed without Google License");
                    return false;
                }
            }

            if (hasOpusGoogleLicense && !(hasOpusILLicense || hasOpusSOLicense))
            {
                if (!IsGoogleOnlyScheduleType(scheduleType))
                {
                    Logger.Warn($"ScheduleType {scheduleType} is not allowed for Google License only");
                    return false;
                }
            }

            if(res)
            {
                res = scheduleType switch
                {
                    ScheduleType.ArchiverDeleteRestoredDataSchedule => LicenseHelperService.IsEnableDeleteRestoreDataFeature(),
                    ScheduleType.ArchiverDedupJobSchedule => SettingProfileService.IsEnableArchiverDeduplication(),
                    _ => res,
                };
                Logger.Info($"ScheduleType {scheduleType} validation result: {res}");
            };

            return res;
        }

        private bool IsSOScheduleType(ScheduleType type)
        {
            return IsCommonScheduleType(type) || IsSOOnlyScheduleType(type);
        }

        private bool IsCommonScheduleType(ScheduleType type)
        {
            return _CommonScheduleTypes.Contains(type);
        }

        private bool IsSOOnlyScheduleType(ScheduleType type)
        { 
            return _SOOnlyScheduleTypes.Contains(type);
        }

        private bool IsGoogleScheduleType(ScheduleType type)
        {
            return _GoogleScheduleTypes.Contains(type);
        }

        private bool IsGoogleOnlyScheduleType(ScheduleType type)
        {
            return _GoogleOnlyScheduleTypes.Contains(type);
        }

    }
}

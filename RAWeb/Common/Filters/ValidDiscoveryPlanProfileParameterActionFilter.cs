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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.I18N.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidDiscoveryPlanProfileParameterActionFilter : BaseActionFilter
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(ValidDiscoveryPlanProfileParameterActionFilter));
        private readonly IRMDiscoveryPlanProfileDao _planProfileDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanProfileDao>();
        private string _action;

        public ValidDiscoveryPlanProfileParameterActionFilter()
        {
        }

        public ValidDiscoveryPlanProfileParameterActionFilter(string action)
        {
            _action = action;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (_action == "SaveOrUpdatePlanProfile")
            {
                var profileInfo = actionContext.ActionArguments.Values.FirstOrDefault() as RMDiscoveryPlanProfileInfo;

                if (profileInfo == null)
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidData") ?? "Invalid Plan Profile data.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(profileInfo.Name))
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_NameRequired") ?? "Plan Profile name is required.");
                    return;
                }

                if (profileInfo.Name.Length > 255)
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_NameTooLong") ?? "Plan Profile name cannot exceed 255 characters.");
                    return;
                }

                bool nameExists = await _planProfileDao.ExistsByNameAsync(profileInfo.Name, profileInfo.Id);
                if (nameExists)
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_JS_RC_ProfileNameExist") ?? "Plan Profile name already exists.");
                    return;
                }

                if (!Enum.IsDefined(typeof(RMDiscoveryPlanAction), profileInfo.Action))
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidAction") ?? "Invalid Action value.");
                    return;
                }

                if (!IsValidActionOptions(profileInfo.ActionOptions))
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidActionOptions") ?? "Invalid ActionOptions value.");
                    return;
                }

                if ((profileInfo.ActionOptions & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub
                    && string.IsNullOrWhiteSpace(profileInfo.StubSetting?.Id))
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_StubSettingRequired") ?? "Stub Setting is required when the Leave Stub option is selected.");
                    return;
                }

                if (profileInfo.PreviousVersion < 0)
                {
                    SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_NegativeVersion") ?? "PreviousVersion cannot be negative.");
                    return;
                }

                if (profileInfo.Action == RMDiscoveryPlanAction.ArchiveAndDestroy)
                {
                    if (string.IsNullOrWhiteSpace(profileInfo.StorageLocationId))
                    {
                        SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_StorageLocationRequired") ?? "Storage Location ID is required.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(profileInfo.StorageName))
                    {
                        SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_StorageNameRequired") ?? "Storage Name is required.");
                        return;
                    }
                }

                if (profileInfo.ScheduleSetting != null)
                {
                    ValidateScheduleSetting(profileInfo.ScheduleSetting, actionContext);
                    if (actionContext.Result != null) return;
                }
            }

            await Task.CompletedTask;
        }

        private void ValidateScheduleSetting(RMDiscoveryPlanScheduleInfo schedule, ActionExecutingContext actionContext)
        {
            if (schedule.NoSchedule) return;

            if (string.IsNullOrWhiteSpace(schedule.StartTime))
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_ScheduleStartRequired") ?? "Schedule start time is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(schedule.TimeZoneId))
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_ScheduleTimezoneRequired") ?? "Schedule timezone is required.");
                return;
            }

            if (!Enum.IsDefined(typeof(EndType), schedule.EndType))
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidEndType") ?? "Invalid schedule end type.");
                return;
            }

            if (schedule.EndType == EndType.EndByTime && string.IsNullOrWhiteSpace(schedule.EndTime))
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_ScheduleEndRequired") ?? "Schedule end time is required when ending by a specific time.");
                return;
            }

            if (schedule.EndType == EndType.EndByOccurrences && schedule.OccurrencesTotal <= 0)
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidOccurrences") ?? "Total occurrences must be greater than zero.");
                return;
            }

            if (!Enum.IsDefined(typeof(IntervalType), schedule.IntervalType) || schedule.IntervalType == IntervalType.None)
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidIntervalType") ?? "A valid schedule interval type is required.");
                return;
            }

            if (schedule.Interval <= 0)
            {
                SetBadRequest(actionContext, I18NEntity.GetString("RM_Discovery_PlanProfile_Validate_InvalidInterval") ?? "Schedule interval must be greater than zero.");
                return;
            }
        }

        private void SetBadRequest(ActionExecutingContext actionContext, string message)
        {
            _logger.Info(message);

            var errorResponse = new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = message
            };

            actionContext.Result = new OkObjectResult(errorResponse);
        }

        private static bool IsValidActionOptions(RMDiscoveryPlanActionOptions value)
        {
            if (value == RMDiscoveryPlanActionOptions.None) return true;

            var allFlags = (RMDiscoveryPlanActionOptions[])Enum.GetValues(typeof(RMDiscoveryPlanActionOptions));
            int validMask = allFlags.Aggregate(0, (mask, flag) => mask | (int)flag);

            if (((int)value & ~validMask) != 0)
            {
                return false;
            }
            
            bool hasOption1 = (value & RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest) == RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest;
            bool hasOption2 = (value & RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious) == RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious;
            bool hasOption4 = (value & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub;

            if (hasOption1 && (hasOption2 || hasOption4))
            {
                return false;
            }

            return true;
        }
    }
}
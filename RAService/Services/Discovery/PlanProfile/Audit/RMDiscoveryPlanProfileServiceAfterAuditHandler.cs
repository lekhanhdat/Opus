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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model.Discovery.Plan;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.PlanProfile.Audit
{
    public class RMDiscoveryPlanProfileServiceAfterAuditHandler : IAsyncAuditAfterHandler
    {
        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private readonly IRMRemoteNodeService _remoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private readonly IRMDiscoveryPlanSiteMappingDao _planSiteMappingDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanSiteMappingDao>();

        private static readonly string[] CriteriaDateTimeFormats =
        {
            "yyyy/M/d H:mm",
            "yyyy/MM/dd HH:mm",
            "yyyy/M/d HH:mm",
            "yyyy/MM/dd H:mm",
            "yyyy/M/d H:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args, object returnValue)
        {
            if (action == AuditAction.CreateDiscoveryPlanProfile || action == AuditAction.UpdateDiscoveryPlanProfile)
            {
                var profileInfo = args[0] as RMDiscoveryPlanProfileInfo;
                if (profileInfo == null) return auditInfo;

                bool isUpdate = action == AuditAction.UpdateDiscoveryPlanProfile;

                if (isUpdate)
                {
                    var result = returnValue is bool b && b;
                    auditInfo.Status = result ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
                }
                else
                {
                    var newId = returnValue is int id ? id : 0;
                    auditInfo.Status = newId > 0 ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;

                    if (newId > 0) profileInfo.Id = newId;
                }

                auditInfo.Object = profileInfo.Name;

                await ProcessPlanProfileNewValuesAsync(auditInfo, profileInfo, isUpdate);
            }
            else if (action == AuditAction.DeleteDiscoveryPlanProfile)
            {
                var result = returnValue is bool b && b;
                auditInfo.Status = result ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            else if( action == AuditAction.SaveDiscoveryPlanDalJobConfiguration)
            {
                auditInfo.Object = returnValue?.ToString();
                auditInfo.Status = !string.IsNullOrEmpty(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }

            return auditInfo;
        }

        private async Task ProcessPlanProfileNewValuesAsync(RMAuditInfo auditInfo, RMDiscoveryPlanProfileInfo profileInfo, bool isUpdate)
        {
            // 1. Prepare Data
            string newRulesText = await FormatRulesToTextAsync(profileInfo.CriteriaInfoes);

            var currentNodeIds = await _planSiteMappingDao.GetNodeIdsByProfileId(profileInfo.Id);
            int mappingType = await _planSiteMappingDao.GetSiteMappingTypeAsync(profileInfo.Id);
            string newUrlsText = await GetMappedSiteAuditTextAsync(mappingType, currentNodeIds);

            // Parse Action Options
            string actionOptionsText = FormatActionOptionsText(profileInfo.ActionOptions, profileInfo.PreviousVersion);

            bool isLeaveStub = (profileInfo.ActionOptions & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub;
            string newStubSettingText = profileInfo.StubSetting?.Name ?? string.Empty;

            // 2. Add/Update items in strict order
            if (isUpdate)
            {
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_Name", profileInfo.Name);
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_FA_PlanProfile_Scope", newUrlsText);
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_Rules", newRulesText);
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_Action", GetActionText(profileInfo.Action));
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_ActionOptions", actionOptionsText);

                RemoveAuditItem(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_PreviousVersion");

                if (isLeaveStub)
                {
                    UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_StubSetting", newStubSettingText);
                }
                else
                {
                    RemoveAuditItem(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_StubSetting");
                }

                UpdateOrRemoveIfUnchanged(auditInfo, "RM_RC_Audit_Discovery_PlanProfile_Storage", profileInfo.StorageName ?? string.Empty);

                await DiffScheduleSettingAsync(auditInfo, profileInfo.ScheduleSetting);
            }
            else
            {
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Name", NewValue = profileInfo.Name });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_FA_PlanProfile_Scope", NewValue = newUrlsText });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Rules", NewValue = newRulesText });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Action", NewValue = GetActionText(profileInfo.Action) });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_ActionOptions", NewValue = actionOptionsText });
                if (isLeaveStub)
                {
                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_StubSetting", NewValue = newStubSettingText });
                }
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Storage", NewValue = profileInfo.StorageName ?? string.Empty });

                if (profileInfo.ScheduleSetting == null || profileInfo.ScheduleSetting.NoSchedule)
                {
                    string noScheduleLabel = I18NEntity.GetString("RM_JS_ScheduleSetting_NoSchedule") ?? "No schedule";
                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_NoSchedule", NewValue = noScheduleLabel });
                }
                else
                {
                    string newStart = await FormatScheduleStartTimeAsync(profileInfo.ScheduleSetting.StartTime, profileInfo.ScheduleSetting.TimeZoneId, false);
                    string newEnd = await FormatScheduleEndTimeAsync(profileInfo.ScheduleSetting);
                    string newInterval = FormatScheduleInterval(profileInfo.ScheduleSetting.Interval, profileInfo.ScheduleSetting.IntervalType);

                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_StratTime", NewValue = newStart });
                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_EndTime", NewValue = newEnd });
                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_TS_IntervalTime", NewValue = newInterval });
                }
            }
        }

        private async Task DiffScheduleSettingAsync(RMAuditInfo auditInfo, RMDiscoveryPlanScheduleInfo newSchedule)
        {
            var noScheduleItem = auditInfo.ModifyContent.FirstOrDefault(c => c.TargetSetting == "RM_JS_ScheduleSetting_NoSchedule");
            string noScheduleLabel = I18NEntity.GetString("RM_JS_ScheduleSetting_NoSchedule") ?? "No schedule";

            if (newSchedule == null || newSchedule.NoSchedule)
            {
                if (noScheduleItem != null)
                {
                    auditInfo.ModifyContent.Remove(noScheduleItem);
                }
                else
                {
                    auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_NoSchedule", NewValue = noScheduleLabel });
                    RemoveAuditItem(auditInfo, "RM_JS_ScheduleSetting_StratTime");
                    RemoveAuditItem(auditInfo, "RM_JS_ScheduleSetting_EndTime");
                    RemoveAuditItem(auditInfo, "RM_TS_IntervalTime");
                }
            }
            else
            {
                if (noScheduleItem != null)
                {
                    auditInfo.ModifyContent.Remove(noScheduleItem);
                }

                string newStart = await FormatScheduleStartTimeAsync(newSchedule.StartTime, newSchedule.TimeZoneId, false);
                string newEnd = await FormatScheduleEndTimeAsync(newSchedule);
                string newInterval = FormatScheduleInterval(newSchedule.Interval, newSchedule.IntervalType);

                UpdateOrRemoveIfUnchanged(auditInfo, "RM_JS_ScheduleSetting_StratTime", newStart);
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_JS_ScheduleSetting_EndTime", newEnd);
                UpdateOrRemoveIfUnchanged(auditInfo, "RM_TS_IntervalTime", newInterval);
            }
        }

        #region Schedule Formatting Helpers
        private async Task<string> FormatScheduleStartTimeAsync(string startTime, string timeZoneId, bool isUtc)
        {
            if (string.IsNullOrWhiteSpace(startTime)) return string.Empty;
            try
            {
                var ticks = ConvertScheduleTimeToUtcTicks(startTime, timeZoneId, isUtc);
                if (ticks <= 0) return string.Empty;
                var gls = await _generalSettingService.GetGeneralSettingAsync();
                return _generalSettingService.ConvertTiksToDateTime(gls, ticks, true).SimplifyFormatTime;
            }
            catch
            {
                return startTime;
            }
        }

        private async Task<string> FormatScheduleEndTimeAsync(RMDiscoveryPlanScheduleInfo schedule)
        {
            switch (schedule.EndType)
            {
                case EndType.EndByOccurrences:
                    string endAfterLabel = I18NEntity.GetString("RM_JS_ScheduleSetting_EndAfter") ?? "End after";
                    string occurrencesLabel = I18NEntity.GetString("RM_JS_ScheduleSetting_Occurrences") ?? "occurrences";
                    return $"{endAfterLabel} {schedule.OccurrencesTotal} {occurrencesLabel}".Trim();

                case EndType.EndByTime:
                    return await FormatScheduleStartTimeAsync(schedule.EndTime, schedule.TimeZoneId, false);

                default:
                    return I18NEntity.GetString("RM_JS_ScheduleSetting_NoEndDate") ?? "No end date";
            }
        }

        private static long ConvertScheduleTimeToUtcTicks(string timeStr, string timeZoneId, bool isUtc)
        {
            if (string.IsNullOrWhiteSpace(timeStr)) return 0;
            var cleanTimeStr = StripTimeZoneSuffix(timeStr, out _);
            if (!DateTime.TryParse(cleanTimeStr, out var parsed)) return 0;

            if (isUtc) return DateTime.SpecifyKind(parsed, DateTimeKind.Utc).Ticks;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified), tz).Ticks;
        }

        private static string StripTimeZoneSuffix(string timeStr, out string suffix)
        {
            var idx = timeStr.IndexOf(" (UTC", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                suffix = timeStr.Substring(idx).Trim();
                return timeStr.Substring(0, idx).Trim();
            }
            suffix = string.Empty;
            return timeStr.Trim();
        }

        private string FormatScheduleInterval(int interval, IntervalType type)
        {
            return type switch
            {
                IntervalType.Hourly => $"{interval} {(I18NEntity.GetString("RM_JS_ScheduleSetting_Hours") ?? "Hours")}",
                IntervalType.Daily => $"{interval} {(I18NEntity.GetString("RM_JS_ScheduleSetting_Days") ?? "Days")}",
                IntervalType.Weekly => $"{interval} {(I18NEntity.GetString("RM_JS_ScheduleSetting_Weeks") ?? "Weeks")}",
                IntervalType.Monthly => $"{interval} {(I18NEntity.GetString("RM_JS_ScheduleSetting_Months") ?? "Months")}",
                _ => string.Empty
            };
        }

        private void RemoveAuditItem(RMAuditInfo auditInfo, string targetSetting)
        {
            var item = auditInfo.ModifyContent.FirstOrDefault(c => c.TargetSetting == targetSetting);
            if (item != null)
            {
                item.NewValue = string.Empty;
            }
        }

        private void UpdateOrRemoveIfUnchanged(RMAuditInfo auditInfo, string targetSetting, string newValue)
        {
            var item = auditInfo.ModifyContent.FirstOrDefault(c => c.TargetSetting == targetSetting);

            if (item != null)
            {
                if (string.Equals(item.OldValue, newValue, StringComparison.OrdinalIgnoreCase))
                {
                    auditInfo.ModifyContent.Remove(item);
                }
                else
                {
                    item.NewValue = newValue;
                }
            }
            else if (!string.IsNullOrEmpty(newValue))
            {
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = targetSetting, NewValue = newValue });
            }
        }

        #endregion

        #region Format Rule Helpers

        private async Task<string> FormatRulesToTextAsync(List<RMDiscoveryRuleCriteriaInfo> rules)
        {
            if (rules == null || rules.Count == 0) return string.Empty;

            var sortedRules = rules.OrderBy(r => r.Order).ToList();
            var sb = new System.Text.StringBuilder();
            var logicExpr = new System.Text.StringBuilder("(");

            for (int i = 0; i < sortedRules.Count; i++)
            {
                var r = sortedRules[i];
                if (r.ConditionInfo == null) continue;

                int category = (int)r.ConditionInfo.Category;

                string criteriaName = GetCriteriaNameText(r.CriteriaType);
                string operatorName = GetOperatorNameText(category, r.ConditionInfo.Logic);
                string parsedValue = await ParseRuleValueTextAsync(category, r.ConditionInfo.Value);

                string extra = !string.IsNullOrWhiteSpace(r.ConditionInfo.ExtraValue)
                    ? $" {r.ConditionInfo.ExtraValue}"
                    : string.Empty;

                sb.AppendLine($"{r.Order}. {criteriaName}, {operatorName}, {parsedValue}{extra}");

                if (i > 0)
                {
                    string logicStr = sortedRules[i - 1].LogicType == RMDiscoveryCriteriaLogicType.Or
                        ? (I18NEntity.GetString("RM_JS_Rule_ConditionOr") ?? "Or")
                        : (I18NEntity.GetString("RM_JS_Rule_ConditionAnd") ?? "And");
                    logicExpr.Append($" {logicStr} ");
                }
                logicExpr.Append(r.Order);
            }

            logicExpr.Append(")");

            if (sortedRules.Count > 1)
            {
                sb.AppendLine(logicExpr.ToString());
            }

            return sb.ToString().TrimEnd();
        }

        private string GetCriteriaNameText(int criteriaType)
        {
            return criteriaType switch
            {
                1 => I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Name") ?? "Name",
                2 => I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Size") ?? "Size",
                3 => I18NEntity.GetString("RM_JS_BCM_Explorer_ExoMoveToSP_ExoCol_Created") ?? "Created time",
                4 => I18NEntity.GetString("RM_RDM_CreateRule_RemoveModified_Time") ?? "Modified time",
                5 => I18NEntity.GetString("RM_FA_Discovery_RuleType_DocumentType") ?? "Type",
                6 => I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleType_Size") ?? "Size",
                _ => $"Field_{criteriaType}"
            };
        }

        private string GetOperatorNameText(int category, int logic)
        {
            if (category == 4)
            {
                return logic switch
                {
                    1 => I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_Before") ?? "Before",
                    2 => I18NEntity.GetString("RM_JS_RDM_CreateRule_DateOption_Older") ?? "Older than",
                    _ => logic.ToString()
                };
            }
            if (category == 5)
            {
                return logic switch
                {
                    1 => I18NEntity.GetString("RM_FA_Discovery_RuleCondition_In") ?? "In",
                    2 => I18NEntity.GetString("RM_FA_Discovery_RuleCondition_NotIn") ?? "Not in",
                    3 => I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_Maths") ?? "Matches",
                    4 => I18NEntity.GetString("RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains") ?? "Does not match",
                    _ => logic.ToString()
                };
            }
            if (category == 6)
            {
                return logic == 1 ? (I18NEntity.GetString("RM_FA_Discovery_RuleCondition_IsEmpty") ?? "Empty") : logic.ToString();
            }
            if (category == 7)
            {
                return logic switch
                {
                    1 => "<=",
                    2 => ">=",
                    _ => logic.ToString()
                };
            }
            return logic.ToString();
        }


        private async Task<string> ParseRuleValueTextAsync(int category, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            try
            {
                if ((category == 4 || category == 7) && value.Trim().StartsWith("{"))
                {
                    var jObj = JObject.Parse(value);
                    string unitStr = jObj["unit"]?.ToString() ?? "";
                    int unitType = jObj["unitType"]?.Value<int>() ?? 0;

                    if (category == 4)
                    {
                        string unitTypeStr = unitType switch
                        {
                            1 => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Days") ?? "Days",
                            2 => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Weeks") ?? "Weeks",
                            3 => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months") ?? "Months",
                            4 => I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Years") ?? "Years",
                            _ => ""
                        };
                        return $"{unitStr} {unitTypeStr}".Trim();
                    }
                    if (category == 7)
                    {
                        string unitTypeStr = unitType switch
                        {
                            1 => I18NEntity.GetString("RM_FA_Progress_Unit_KB") ?? "KB",
                            2 => I18NEntity.GetString("RM_FA_Progress_Unit_MB") ?? "MB",
                            3 => I18NEntity.GetString("RM_FA_Progress_Unit_GB") ?? "GB",
                            4 => "TB",
                            _ => ""
                        };
                        return $"{unitStr} {unitTypeStr}".Trim();
                    }
                }

                if (category == 4)
                {
                    if (long.TryParse(value, out long ticks))
                    {
                        var gls = await _generalSettingService.GetGeneralSettingAsync();
                        var timeModel = _generalSettingService.ConvertTiksToDateTime(gls, ticks, true);
                        return timeModel?.SimplifyFormatTime ?? value;
                    }

                    if (DateTime.TryParseExact(
                            value,
                            CriteriaDateTimeFormats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out _))
                    {
                        return value;
                    }
                }

                if (category == 5 && value.Trim().StartsWith("["))
                {
                    var jArray = JArray.Parse(value);
                    var stringList = jArray.Select(t => t.ToString());
                    return string.Join(", ", stringList);
                }

                if (category == 6)
                {
                    if (bool.TryParse(value, out bool boolVal))
                    {
                        return boolVal
                            ? (I18NEntity.GetString("RM_JS_Common_Yes") ?? "Yes")
                            : (I18NEntity.GetString("RM_JS_Common_No") ?? "No");
                    }
                }

                return value;
            }
            catch
            {
                return value;
            }
        }

        #endregion

        #region Remote node mapping helpers

        private async Task<string> GetMappedSiteAuditTextAsync(int type, List<string> nodeIds)
        {
            if (nodeIds == null || !nodeIds.Any()) return string.Empty;

            //string typeName = ((RMDiscoveryPlanSiteType)type).ToString();

            var urls = new List<string>();
            foreach (var id in nodeIds)
            {
                try
                {
                    var node = _remoteNodeService.GetRemoteSiteCollectionById(id);
                    if (node != null && !string.IsNullOrWhiteSpace(node.url))
                    {
                        urls.Add(node.url);
                    }
                }
                catch (Exception)
                {
                }
            }

            //if (!urls.Any()) return $"Type: {typeName}";

            var sb = new System.Text.StringBuilder();
            //sb.AppendLine($"Type: {typeName}");
            for (int i = 0; i < urls.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {urls[i]}");
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Action Options Formatting Helpers
        private string GetActionText(RMDiscoveryPlanAction action)
        {
            return action switch
            {
                RMDiscoveryPlanAction.ArchiveAndDestroy => I18NEntity.GetString("RM_FA_PlanProfile_Action_Radio_ArchiveAndDestroy") ?? "Archive and destroy",
                RMDiscoveryPlanAction.DestroyFile => I18NEntity.GetString("RM_FA_PlanProfile_Action_Radio_Destroy") ?? "Destroy",
                _ => action.ToString()
            };
        }

        private string FormatActionOptionsText(RMDiscoveryPlanActionOptions options, int previousVersion)
        {
            if (options == RMDiscoveryPlanActionOptions.None) return "None";

            var sb = new System.Text.StringBuilder();

            // 1: KeepCurrentAndSpecifiedArchiveRest
            if ((options & RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest) == RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest)
            {
                string label = I18NEntity.GetString("RM_JS_Rule_KeepVersionAndArchiveOther") ?? "Keep the current and the specified number of previous versions, and archive and destroy the rest";
                sb.AppendLine($"{label} {previousVersion}");
            }

            // 2: ArchiveCurrentAndPrevious
            if ((options & RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious) == RMDiscoveryPlanActionOptions.ArchiveCurrentAndPrevious)
            {
                string label = I18NEntity.GetString("RM_JS_Audit_ArchiveVersionAndDestroyFile") ?? "Archive the current and the number of previous versions:";
                sb.AppendLine(label);
            }

            // 4: LeaveStub
            if ((options & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub)
            {
                string label = I18NEntity.GetString("RM_FA_DataOptimize_File_LeaveStub") ?? "Leave a stub in place for each document";
                sb.AppendLine(label);
            }

            // 8: IncludeDeclaredRecords
            if ((options & RMDiscoveryPlanActionOptions.IncludeDeclaredRecords) == RMDiscoveryPlanActionOptions.IncludeDeclaredRecords)
            {
                string label = I18NEntity.GetString("RM_FA_DataOptimize_File_IncludeRecords") ?? "Include declared records";
                sb.AppendLine(label);
            }

            // 16: IncludeLockedByRecordsLabel
            if ((options & RMDiscoveryPlanActionOptions.IncludeLockedByRecordsLabel) == RMDiscoveryPlanActionOptions.IncludeLockedByRecordsLabel)
            {
                string label = I18NEntity.GetString("RM_RDM_CreateRule_RecordsLabelOption") ?? "Include documents/items locked by records label";
                sb.AppendLine(label);
            }

            // 32: KeepCurrentAndPrevious
            if ((options & RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious) == RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious)
            {
                string label = I18NEntity.GetString("RM_JS_Rule_KeepLatestVersionAndDestroyOther") ?? "Keep the current version and the number of previous versions";
                sb.AppendLine($"{label} {previousVersion}");
            }

            // 64: DeleteToRecycleBin
            if ((options & RMDiscoveryPlanActionOptions.DeleteToRecycleBin) == RMDiscoveryPlanActionOptions.DeleteToRecycleBin)
            {
                string label = I18NEntity.GetString("RM_RDM_CreateRule_Options_DeleteToRecycleBin") ?? "Delete to the Recycle Bin";
                sb.AppendLine(label);
            }

            return sb.ToString().TrimEnd();
        }

        #endregion
    }
}
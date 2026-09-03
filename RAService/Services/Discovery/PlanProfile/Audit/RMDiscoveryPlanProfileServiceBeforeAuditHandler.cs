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
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CustomizeConnector.I18ns;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleType = AvePoint.RA.Contract.Schedule.ScheduleType;

namespace AvePoint.RA.Service.Services.Discovery.PlanProfile.Audit
{
    public class RMDiscoveryPlanProfileServiceBeforeAuditHandler : IAsyncAuditBeforeHandler
    {
        private readonly IRMDiscoveryPlanProfileDao _planProfileDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanProfileDao>();
        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        private readonly IScheduleService _scheduleService = PlatformWindsorManager.GetService<IScheduleService>();
        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private readonly IRMDiscoveryPlanSiteMappingDao _planSiteMappingDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanSiteMappingDao>();
        private readonly IRMRemoteNodeService _remoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private readonly IRMDiscoveryPlanDalJobConfiguration _configInfoDao = new RMDiscoveryDalJobConfigurationDao();
        private readonly IStubSettingService _stubSettingService = PlatformWindsorManager.GetService<IStubSettingService>();
        private static readonly string[] CriteriaDateTimeFormats =
        {
            "yyyy/M/d H:mm",
            "yyyy/MM/dd HH:mm",
            "yyyy/M/d HH:mm",
            "yyyy/MM/dd H:mm",
            "yyyy/M/d H:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            if (action == AuditAction.UpdateDiscoveryPlanProfile)
            {
                var profileInfo = args[0] as RMDiscoveryPlanProfileInfo;
                if (profileInfo == null) return auditInfo;

                await ProcessPlanProfileOldValuesAsync(auditInfo, profileInfo.Id);
            }
            else if (action == AuditAction.DeleteDiscoveryPlanProfile)
            {
                var profileIds = args[0] as List<int>;
                if (profileIds == null || !profileIds.Any()) return auditInfo;

                var deletedNames = new List<string>();

                foreach (var id in profileIds)
                {
                    var existing = await _planProfileDao.GetByIdAsync(id);
                    if (existing != null)
                    {
                        deletedNames.Add(existing.Name);
                    }
                }

                if (deletedNames.Any())
                {
                    auditInfo.Object = string.Join(", ", deletedNames);
                }

            }
            else if (action == AuditAction.CreateDiscoveryPlanProfile)
            {
                var profileInfo = args[0] as RMDiscoveryPlanProfileInfo;
                if (profileInfo != null) auditInfo.Object = profileInfo.Name;
            }
            else if(action == AuditAction.SaveDiscoveryPlanDalJobConfiguration)
            {
                var parameters = args[2] as string;
                var profileInfo = string.IsNullOrEmpty(parameters) ? null : SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryTriggerDalJob>(parameters);
                var oldScopeInfo = await _configInfoDao.GetAsync<RMDiscoveryTriggerDalJob>(Contract.Discovery.Model.RMDiscoveryConfigurationType.Office365NewlyScope);
                await CollectScopeConfig(auditInfo, oldScopeInfo, profileInfo);
            }
            return auditInfo;
        }
        public async Task CollectScopeConfig(RMAuditInfo auditInfo, RMDiscoveryTriggerDalJob oldScopeInfo, RMDiscoveryTriggerDalJob scopeInfo)
        {
            var scopeAudit = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Discovery_ScopeType",
                OldValue = oldScopeInfo.ScopeType.ToString(),
                NewValue = scopeInfo.ScopeType.ToString()
            };
            auditInfo.ModifyContent.Add(scopeAudit);

            if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource ||
                scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                oldScopeInfo = oldScopeInfo.CompatibleConvert();
                var oldDataSource = oldScopeInfo.ContentSources.ConvertAll(item =>
                    I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[item]));
                var newDataSource = scopeInfo.ContentSources.ConvertAll(item =>
                    I18NEntity.GetString(BuildInContentSourceI18Ns.SourceFlagI18ns[item]));
                var dataSourceAudit = new AuditItem
                {
                    TargetSetting = "RM_FA_Discovery_JobPage_Scope_DataSource",
                    OldValue = string.Join(";\n ", oldDataSource),
                    NewValue = string.Join(";\n ", newDataSource),
                };
                auditInfo.ModifyContent.Add(dataSourceAudit);
            }
            if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify ||
                scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
            {
                var IdAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_Audit_Discovery_ScopeSpecify",
                };
                if (oldScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
                {
                    var oldContainerInfo = await _planProfileDao.GetOpusContainersAsync(oldScopeInfo.SpecifyContainerIds);
                    IdAudit.OldValue = string.Join(";\n ", GetContainerUrls(oldContainerInfo));
                }

                if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
                {
                    var containerInfo = await _planProfileDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
                    IdAudit.NewValue = string.Join(";\n ", GetContainerUrls(containerInfo));
                }

                auditInfo.ModifyContent.Add(IdAudit);
            }
        }

        private async Task ProcessPlanProfileOldValuesAsync(RMAuditInfo auditInfo, int profileId)
        {
            var existing = await _planProfileDao.GetByIdAsync(profileId);
            if (existing == null) return;

            // 1. Prepare Data
            string oldUrlsText = await GetMappedSiteAuditTextAsync(profileId);
            string oldRulesText = await FormatRulesJsonToTextAsync(existing.Rules);
            var storage = string.IsNullOrWhiteSpace(existing.StorageLocationId)
                ? null
                : _storageDeviceService.GetStorageDeviceById(existing.StorageLocationId);
            string oldStorageText = storage?.Name ?? string.Empty;
            var oldStubSetting = string.IsNullOrWhiteSpace(existing.StubSettingId)
                ? null
                : _stubSettingService.GetStubSettingById(existing.StubSettingId);
            string oldStubSettingText = oldStubSetting?.Name ?? string.Empty;

            // Parse Action Options
            string actionOptionsText = FormatActionOptionsText(existing.ActionOptions, existing.PreviousVersion);

            // 2. Add to Audit in strict Order
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Name", OldValue = existing.Name });
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_FA_PlanProfile_Scope", OldValue = oldUrlsText });
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Rules", OldValue = oldRulesText });
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Action", OldValue = GetActionText(existing.Action) });
            if ((existing.ActionOptions & RMDiscoveryPlanActionOptions.LeaveStub) == RMDiscoveryPlanActionOptions.LeaveStub)
            {
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_StubSetting", OldValue = oldStubSettingText });
            }
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_ActionOptions", OldValue = actionOptionsText });
            auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_RC_Audit_Discovery_PlanProfile_Storage", OldValue = oldStorageText });

            // 3. Process Schedule
            var oldSchedule = await _scheduleService.GetScheduleAsync(profileId.ToString(), ScheduleType.DiscoveryPlanSchedule);

            if (oldSchedule == null || oldSchedule.NoSchedule)
            {
                string noScheduleLabel = I18NEntity.GetString("RM_JS_ScheduleSetting_NoSchedule") ?? "No schedule";
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_NoSchedule", OldValue = noScheduleLabel });
            }
            else
            {
                string oldStart = await FormatScheduleStartTimeAsync(oldSchedule.StartTime, oldSchedule.TimeZoneId, false);
                string oldEnd = await FormatScheduleEndTimeAsync(oldSchedule);
                string oldInterval = FormatScheduleInterval(oldSchedule.Interval, oldSchedule.IntervalType);

                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_StratTime", OldValue = oldStart });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_JS_ScheduleSetting_EndTime", OldValue = oldEnd });
                auditInfo.ModifyContent.Add(new AuditItem { TargetSetting = "RM_TS_IntervalTime", OldValue = oldInterval });
            }
        }
        private List<string> GetContainerUrls(List<RMRemoteNode> contianerInfo)
        {
            return contianerInfo.ConvertAll(c =>
            {
                if (c.Url.Equals("Default_ SharePoint Sites_ Group"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                }
                else if (c.Url.Equals("Default Office 365 Group Sites Group"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                }
                else if (c.Url.Equals("Default Private Channel Sites Container"))
                {
                    c.Url = I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                }

                return c;
            }).Select(c => c.Url).ToList();
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

        private async Task<string> FormatScheduleEndTimeAsync(ScheduleInfo schedule)
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
        #endregion

        #region Format Rule Helpers
        private async Task<string> FormatRulesJsonToTextAsync(string rulesJson)
        {
            if (string.IsNullOrWhiteSpace(rulesJson)) return string.Empty;
            try
            {
                var rules = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(rulesJson);
                return await FormatRulesToTextAsync(rules);
            }
            catch
            {
                return rulesJson;
            }
        }

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

        private async Task<string> GetMappedSiteAuditTextAsync(int planProfileId)
        {
            var mappingType = await _planSiteMappingDao.GetSiteMappingTypeAsync(planProfileId);
            var nodeIds = await _planSiteMappingDao.GetNodeIdsByPlanProfileIdAsync(planProfileId);

            return await GetMappedSiteAuditTextAsync(mappingType, nodeIds);
        }

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
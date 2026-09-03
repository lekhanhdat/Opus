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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.ManualApproval.Actions;
using AvePoint.RA.Contract.FunctionSetting;


namespace AvePoint.RA.Service.Services.ManualApproval.AuditHandler
{
    public class RMManualApprovalBeforeAuditHandler : IAsyncAuditBeforeHandler
    {
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static ManualApprovalRecordRepository Repository => new ManualApprovalRecordRepository();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();


        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args)
        {
            if (action == AuditAction.ManualApprovalConfigSetting)
            {
                var newSetting = args[0] as ManualApprovalSettings;
                var oldSettingJson = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualSetting);
                var oldSetting = JsonConvert.DeserializeObject<ManualApprovalSettings>(oldSettingJson);
                CollectEmailNotificationSetting(auditInfo, newSetting, oldSetting);
                CollectEscalationSetting(auditInfo, newSetting, oldSetting);
                CollectDisposalExtention(auditInfo, newSetting, oldSetting);
            }
            else if (action == AuditAction.RunExportHistoryJob)
            {
                var historyOption = SerializerHelper.DeserializeByJsonSerializer<ManualApprovalHistoryOption>(args[0]?.ToString());
                CollectExportSetting(historyOption, auditInfo);
                if (!string.IsNullOrEmpty(historyOption.FullPath))
                {
                    auditInfo.Action = AuditAction.GenerateDisposalHistory;
                    auditInfo.Category = AuditCategory.FSMyhub;
                }
            }
            else if (action == AuditAction.RunImportUnderReviewJob)
            {
                CollectImportData(args[0].ToString(), auditInfo);
            }
            else if (action == AuditAction.ReassignTo
                || action == AuditAction.EscalateTo)
            {
                await CollectEscalateOrRejectAsync(auditInfo, args);
            }
            else if (action == AuditAction.SaveApprovalCommentOption)
            {
                var newSetting = (ManualApprovalCommentInfos)args[0];
                var oldSettingJsonApprovalComment = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualApprovalCommentOption);
                var oldSettingApprovalComment = JsonConvert.DeserializeObject<ManualApprovalCommentOptions>(oldSettingJsonApprovalComment).ToString();
                var oldSettingJsonQucikReason = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualApprovalCommentSetting);
                var oldSettingQucikReason = JsonConvert.DeserializeObject<ManualApprovalCommentSetting>(oldSettingJsonQucikReason);
                var oldSettingJsonCustomButtonName = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualApprovalButtonName);
                var oldSettingCustomButton = JsonConvert.DeserializeObject<ManualApprovalModifyName>(oldSettingJsonCustomButtonName);
                var oldDuration = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualApprovalDuration);

                var oldSettingJsonAutoProcess = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.EnableAutoApprovedProcess);
                var oldSettingAutoProcess = string.IsNullOrEmpty(oldSettingJsonAutoProcess) ? false : Convert.ToBoolean(oldSettingJsonAutoProcess);
                var oldSettingJsonAutoProcessRecheck = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.IsRecheckRule);
                var oldSettingAutoProcessRecheck = string.IsNullOrEmpty(oldSettingJsonAutoProcessRecheck) ? false : Convert.ToBoolean(oldSettingJsonAutoProcessRecheck);
                var oldSettingJsonAutoProcessDeleteInvalidRecords = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.EnableDeleteInvalidRecords);
                var oldSettingAutoProcessDeleteInvalidRecords= string.IsNullOrEmpty(oldSettingJsonAutoProcessDeleteInvalidRecords) ? false : Convert.ToBoolean(oldSettingJsonAutoProcessDeleteInvalidRecords);
                await SaveApprovalCommentOption(auditInfo, newSetting, oldSettingApprovalComment, oldSettingQucikReason, oldSettingCustomButton, oldDuration, oldSettingAutoProcess, oldSettingAutoProcessRecheck, oldSettingAutoProcessDeleteInvalidRecords);
            }
            else if (action == AuditAction.ManualApprovalSetting)
            {
                var newSetting = (ManualApprovalSettingInfo)args[0];

                var oldApprovalProcessSetting = await GetOldSettingFromFunctionSetting<ManualApprovalSettings>(FunctionSettingType.ManualSetting);
                CollectEmailNotificationSetting(auditInfo, newSetting.ApprovalProcessSetting, oldApprovalProcessSetting);
                CollectEscalationSetting(auditInfo, newSetting.ApprovalProcessSetting, oldApprovalProcessSetting);
                CollectDisposalExtention(auditInfo, newSetting.ApprovalProcessSetting, oldApprovalProcessSetting);

                var oldSettingApprovalComment = (await GetOldSettingFromFunctionSetting<ManualApprovalCommentOptions>(FunctionSettingType.ManualApprovalCommentOption)).ToString();
                var oldSettingQuickReason = await GetOldSettingFromFunctionSetting<ManualApprovalCommentSetting>(FunctionSettingType.ManualApprovalCommentSetting);
                var oldSettingCustomButton = await GetOldSettingFromFunctionSetting<ManualApprovalModifyName>(FunctionSettingType.ManualApprovalButtonName);
                var oldDuration = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualApprovalDuration);
                var oldSettingJsonAutoProcess = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.EnableAutoApprovedProcess);
                var oldSettingAutoProcess = string.IsNullOrEmpty(oldSettingJsonAutoProcess) ? false : Convert.ToBoolean(oldSettingJsonAutoProcess);
                var oldSettingJsonAutoProcessRecheck = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.IsRecheckRule);
                var oldSettingAutoProcessRecheck = string.IsNullOrEmpty(oldSettingJsonAutoProcessRecheck) ? false : Convert.ToBoolean(oldSettingJsonAutoProcessRecheck);
                var oldSettingJsonAutoProcessDeleteInvalidRecords = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.EnableDeleteInvalidRecords);
                var oldSettingAutoProcessDeleteInvalidRecords = string.IsNullOrEmpty(oldSettingJsonAutoProcessDeleteInvalidRecords) ? false : Convert.ToBoolean(oldSettingJsonAutoProcessDeleteInvalidRecords);
                await SaveApprovalCommentOption(auditInfo, newSetting.CommentSettingInfo, oldSettingApprovalComment, oldSettingQuickReason, oldSettingCustomButton, oldDuration, oldSettingAutoProcess, oldSettingAutoProcessRecheck, oldSettingAutoProcessDeleteInvalidRecords);

                auditInfo.Category = newSetting.Module switch
                {
                    Module.RecordForReview => RA.Contract.RMWeb.Audit.AuditCategory.ManualApprovalTimer,
                    Module.ApprovalSetting or _ => RA.Contract.RMWeb.Audit.AuditCategory.ApprovalProcesses,
                };
                auditInfo.Action = action;
            }
            else if (action == AuditAction.MarkToApproved || action == AuditAction.MarkToRejected)
            {
                bool isFromMyhub = (bool)args[1];
                if (isFromMyhub == true)
                {
                    auditInfo.Category = AuditCategory.FSMyhub;
                }
                auditInfo.Action = action;
            }
            else if (action == AuditAction.MarkToPause || action == AuditAction.MarkToResume)
            {
                auditInfo.Action = action;
            }

            return auditInfo;
        }

        private async Task<T> GetOldSettingFromFunctionSetting<T>(FunctionSettingType functionSettingType)
        {
            var oldSettingJson = await FunctionSettingDao.GetSettingInfo(functionSettingType);
            if (string.IsNullOrEmpty(oldSettingJson))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(oldSettingJson);
        }

        private async System.Threading.Tasks.Task SaveApprovalCommentOption(RMAuditInfo info, ManualApprovalCommentInfos newSetting, string oldSettingApprovalComment,
            ManualApprovalCommentSetting oldSettingQucikReason, ManualApprovalModifyName oldSettingCustomButton, string oldDuration, bool oldAutoProcess, bool oldAutoProcessRecheck, bool oldAutoProcessDeleteInvalidRecords)
        {
            var settingMap = new Dictionary<object, string>
            {
                { ManualApprovalCommentOptions.BothApproveAndReject, I18NEntity.GetString("RM_MA_ApprovalComment_Both") },
                { ManualApprovalCommentOptions.ApproveOnly, I18NEntity.GetString("RM_MA_ApprovalComment_ApproveOnly") },
                { ManualApprovalCommentOptions.RejectOnly, I18NEntity.GetString("RM_MA_ApprovalComment_RejectOnly") },
                { ManualApprovalCommentOptions.Optional, I18NEntity.GetString("RM_MA_ApprovalComment_Optional") }
            };
            oldSettingApprovalComment = settingMap[Enum.Parse(typeof(ManualApprovalCommentOptions), oldSettingApprovalComment)];
            var newSettingApprovalComment = settingMap[newSetting.Option];

            var oldSettingQUickReasonOption = oldSettingQucikReason.ManualApprovalQuickReasonInfo.NeedQuickReason;
            var oldSettingQuickReasonInfo = new List<string>();
            if (oldSettingQUickReasonOption)
            {
                oldSettingQuickReasonInfo = [.. oldSettingQucikReason.ManualApprovalQuickReasonInfo.QuickReasonInfo];
            }

            var newSettingQuickReasonOption = newSetting.CommentSetting.ManualApprovalQuickReasonInfo.NeedQuickReason;
            var newSettingQuickReasonInfo = new List<string>();
            if (newSettingQuickReasonOption)
            {
                newSettingQuickReasonInfo = [.. newSetting.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo];
            }

            var oldNeedCustomButton = oldSettingCustomButton.ManualApprovalModifyButton.EnableModifyButtonName;
            var oldCustomButtonNames = new List<ManualApprovalModifiedButtonNames>();
            if (oldNeedCustomButton)
            {
                oldCustomButtonNames = [.. oldSettingCustomButton.ManualApprovalModifyButton.ModifiedButtonNames];
            }

            var newNeedCustomButton = newSetting.ModifyButtonName.ManualApprovalModifyButton.EnableModifyButtonName;
            var newCustomButtonNames = new List<ManualApprovalModifiedButtonNames>();
            if (newNeedCustomButton)
            {
                newCustomButtonNames = [.. newSetting.ModifyButtonName.ManualApprovalModifyButton.ModifiedButtonNames];
            }

            var auditApprovalComment = new AuditItem
            {
                TargetSetting = "RM_JS_ApprovalComment_Configuration",
                OldValue = oldSettingApprovalComment,
                NewValue = newSettingApprovalComment
            };
            var auditQuickReasonOption = new AuditItem
            {
                TargetSetting = "RM_MA_QuickReasonOption",
                OldValue = oldSettingQUickReasonOption.ToString(),
                NewValue = newSettingQuickReasonOption.ToString()
            };
            var auditQuickReason = new AuditItem
            {
                TargetSetting = "RM_MA_QuickReason",
                OldValue = await GetOldorNewQucikReasonInfo(oldSettingQuickReasonInfo),
                NewValue = await GetOldorNewQucikReasonInfo(newSettingQuickReasonInfo)
            };
            var auditNeedCustomButton = new AuditItem
            {
                TargetSetting = "RM_MA_CustomButton_EnableCustom",
                OldValue = oldNeedCustomButton.ToString(),
                NewValue = newNeedCustomButton.ToString(),
            };
            var auditCustomButtonNames = new AuditItem
            {
                TargetSetting = "RM_MA_CustomButton_CustomButtonNames",
                OldValue = GetCustomButtonNames(oldCustomButtonNames),
                NewValue = GetCustomButtonNames(newCustomButtonNames),
            };

            var auditDuration = new AuditItem
            {
                TargetSetting = "RM_MA_Duration_Title",
                OldValue = oldDuration,
                NewValue = newSetting.Duration.ToString(),
            };
            var auditAutoProcess = new AuditItem
            {
                TargetSetting = "RM_MA_ApprovalComment_AutoApproved",
                OldValue = oldAutoProcess ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
                NewValue = newSetting.EnableAutoApprovedProcess ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
            };
            var auditAutoProcessRecheckRule = new AuditItem
            {
                TargetSetting = "RM_MA_ApprovalComment_RecheckRule",
                OldValue = oldAutoProcessRecheck ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
                NewValue = newSetting.isRecheckRule ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
            };
            var auditAutoProcessDeleteInvalidRecords = new AuditItem
            {
                TargetSetting = "RM_MA_ApprovalComment_DeleteInvalidRecords",
                OldValue = oldAutoProcessDeleteInvalidRecords ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
                NewValue = newSetting.EnableDeleteInvalidRecords ? "RM_JS_Common_Enabled" : "RM_JS_Common_No",
            };
            info.ModifyContent.Add(auditApprovalComment);
            info.ModifyContent.Add(auditQuickReasonOption);
            info.ModifyContent.Add(auditQuickReason);
            info.ModifyContent.Add(auditNeedCustomButton);
            info.ModifyContent.Add(auditCustomButtonNames);
            info.ModifyContent.Add(auditDuration);
            info.ModifyContent.Add(auditAutoProcess);
            info.ModifyContent.Add(auditAutoProcessRecheckRule);
            info.ModifyContent.Add(auditAutoProcessDeleteInvalidRecords);
        }

        private static string GetCustomButtonNames(List<ManualApprovalModifiedButtonNames> customButtonNames)
        {
            var customButtonNameString = string.Empty;
            if (customButtonNames.Count > 0)
            {
                var approveButtonNames = customButtonNames[0];
                var rejectButtonNames = customButtonNames[1];
                customButtonNameString += I18NEntity.GetString("RM_MA_Approve") + ": " + approveButtonNames.EnglishName + "; " + approveButtonNames.JapaneseName + "; " + approveButtonNames.ChineseName + '\n';
                customButtonNameString += I18NEntity.GetString("RM_MA_Reject") + ": " + rejectButtonNames.EnglishName + "; " + rejectButtonNames.JapaneseName + "; " + rejectButtonNames.ChineseName;
            }

            return customButtonNameString;
        }
        private async Task<string> GetOldorNewQucikReasonInfo(List<string> ReasonInfo)
        {
            var result = new List<string>();
            if (!ReasonInfo.Any())
            {
                return string.Empty;
            }
            result.AddRange(ReasonInfo);

            return string.Join("; ", result);
        }

        private void CollectEmailNotificationSetting(RMAuditInfo info, ManualApprovalSettings newSetting, ManualApprovalSettings oldSetting)
        {
            var newEmailSetting = newSetting.EmailNotificationSetting;
            var oldEmailSetting = oldSetting.EmailNotificationSetting;
            if (newEmailSetting.Interval != oldEmailSetting.Interval || newEmailSetting.IntervalType != oldEmailSetting.IntervalType)
            {
                var audit = new AuditItem
                {
                    TargetSetting = "RM_TS_IntervalTime",
                    OldValue = oldEmailSetting.Interval + " " + (oldEmailSetting.IntervalType == ManualApprovalIntervalType.Days ? "RM_JS_ScheduleSetting_Days " : "RM_JS_ScheduleSetting_Weeks "),
                    NewValue = newEmailSetting.Interval + " " + (newEmailSetting.IntervalType == ManualApprovalIntervalType.Days ? "RM_JS_ScheduleSetting_Days " : "RM_JS_ScheduleSetting_Weeks "),
                };
                info.ModifyContent.Add(audit);
            }

            if (newEmailSetting.EndType != oldEmailSetting.EndType || newEmailSetting.OccurrencesTimes != oldEmailSetting.OccurrencesTimes)
            {
                string oldValue;
                if (oldEmailSetting.EndType == ManualApprovalEndType.NoEnd)
                {
                    oldValue = "RM_JS_ScheduleSetting_NoEndDate";
                }
                else
                {
                    oldValue = "RM_JS_ScheduleSetting_EndAfter" + " " + oldEmailSetting.OccurrencesTimes + " " + "RM_JS_ScheduleSetting_Occurrences ";
                }

                string newValue;
                if (newEmailSetting.EndType == ManualApprovalEndType.NoEnd)
                {
                    newValue = "RM_JS_ScheduleSetting_NoEndDate";
                }
                else
                {
                    newValue = "RM_JS_ScheduleSetting_EndAfter" + " " + newEmailSetting.OccurrencesTimes + " " + "RM_JS_ScheduleSetting_Occurrences ";
                }

                var audit = new AuditItem
                {
                    TargetSetting = "RM_JS_ScheduleSetting_EndTime",
                    OldValue = oldValue,
                    NewValue = newValue
                };
                info.ModifyContent.Add(audit);
            }
        }

        private void CollectEscalationSetting(RMAuditInfo info, ManualApprovalSettings newSettings, ManualApprovalSettings oldSettings)
        {
            var newEscalationSetting = newSettings.EscalationSetting;
            var oldEscalationSetting = oldSettings.EscalationSetting;

            if (newEscalationSetting.EscalateSettingType != oldEscalationSetting.EscalateSettingType
                || newEscalationSetting.ApprovalStatus != oldEscalationSetting.ApprovalStatus
                || !AreReassignUsersEqual(newEscalationSetting.ReassignUsers, oldEscalationSetting.ReassignUsers))
            {
                var audit = new AuditItem
                {
                    TargetSetting = "RM_MA_Setting_Escalation",
                    OldValue = GetEscalationAuditValue(oldEscalationSetting),
                    NewValue = GetEscalationAuditValue(newEscalationSetting),
                };
                info.ModifyContent.Add(audit);
            }
        }

        private static string GetEscalationAuditValue(ManualApprovalEscalationSetting setting)
        {
            return setting.EscalateSettingType switch
            {
                ManualApprovalEscalateSettingType.NoAction => I18NEntity.GetString("RM_MA_Setting_Escalation_NoAction"),
                ManualApprovalEscalateSettingType.WorkflowNextStep =>
                    I18NEntity.GetString("RM_MA_Setting_Escalation_Workflow") + "; " +
                    (setting.ApprovalStatus == SOApproveDBStatus.Approved
                        ? I18NEntity.GetString("RM_MA_Approve")
                        : I18NEntity.GetString("RM_MA_Reject")) +
                    " and reassign the task",
                ManualApprovalEscalateSettingType.ReassignSpecificUsers =>
                    I18NEntity.GetString("RM_MA_Setting_Escalation_Reassign") + ": " +
                    string.Join("; ", setting.ReassignUsers?.Select(u => u.DisplayName ?? u.UserPrincipalName) ?? Enumerable.Empty<string>()),
                _ => string.Empty,
            };
        }

        private static bool AreReassignUsersEqual(List<AvePoint.RA.Contract.RMWeb.ReportCenter.ToUserInfo> newUsers, List<AvePoint.RA.Contract.RMWeb.ReportCenter.ToUserInfo> oldUsers)
        {
            var newEmails = (newUsers ?? []).Select(u => u.UserPrincipalName).OrderBy(x => x);
            var oldEmails = (oldUsers ?? []).Select(u => u.UserPrincipalName).OrderBy(x => x);
            return newEmails.SequenceEqual(oldEmails);
        }

        private void CollectDisposalExtention(RMAuditInfo info, ManualApprovalSettings newSettings, ManualApprovalSettings oldSettings)
        {
            var newDisposalSetting = newSettings.DisposalExtentionSetting;
            var oldDisposalSetting = oldSettings.DisposalExtentionSetting;
            if (newDisposalSetting.MaxDelayTimes != oldDisposalSetting.MaxDelayTimes)
            {
                var audit = new AuditItem
                {
                    TargetSetting = "RM_MA_Setting_Disposal_Extention_Delay_Times",
                    OldValue = oldDisposalSetting.MaxDelayTimes.ToString(),
                    NewValue = newDisposalSetting.MaxDelayTimes.ToString()
                };
                info.ModifyContent.Add(audit);
            }

            if (newDisposalSetting.LatestExtendType != oldDisposalSetting.LatestExtendType || newDisposalSetting.LatestExtendNumber != oldDisposalSetting.LatestExtendNumber)
            {
                var auditNumber = new AuditItem
                {
                    TargetSetting = "RM_MA_Setting_Disposal_Extention_Delay_LatestNumber",
                    OldValue = oldDisposalSetting.LatestExtendNumber.ToString() + "  " + I18NEntity.GetString(ExtendTypeI18n(oldDisposalSetting.LatestExtendType)),
                    NewValue = newDisposalSetting.LatestExtendNumber.ToString() + "  " + I18NEntity.GetString(ExtendTypeI18n(newDisposalSetting.LatestExtendType))
                };
                info.ModifyContent.Add(auditNumber);
            }
        }

        private void CollectExportSetting(ManualApprovalHistoryOption historyOption, RMAuditInfo info)
        {
            var newValue = string.Empty;
            var timeFrame = string.Empty;
            switch (historyOption.LatestExportType)
            {
                case (int)TimeRange.After3Month:
                    newValue = "RM_MA_HistoryExport_TimeRange_3M";
                    break;
                case (int)TimeRange.After6Month:
                    newValue = "RM_MA_HistoryExport_TimeRange_6M";
                    break;
                case (int)TimeRange.After1Year:
                    newValue = "RM_MA_HistoryExport_TimeRange_1Y";
                    break;
                case (int)TimeRange.Custom:
                    newValue = "RM_MA_HistoryExport_TimeRange_Custom";
                    timeFrame = historyOption.CustomDate.StartDateTime.ToString() + '\n' + historyOption.CustomDate.EndDateTime.ToString();
                    break;
                case (int)TimeRange.All:
                    newValue = "RM_MA_HistoryExport_All";
                    break;
            }
            var audit = new AuditItem
            {
                TargetSetting = "RM_MA_HistoryExport_Title",
                NewValue = newValue
            };
            info.ModifyContent.Add(audit);
            if (timeFrame != string.Empty)
            {
                var timeAudit = new AuditItem
                {
                    TargetSetting = "RM_RC_DueDisposalViewDetail_Time",
                    NewValue = timeFrame
                };
                info.ModifyContent.Add(timeAudit);
            }
        }

        private void CollectImportData(string importParamStr, RMAuditInfo info)
        {
            var importParam = SerializerHelper.DeserializeByJsonConvert<ManualApprovalImportParams>(importParamStr);
            var audit = new AuditItem
            {
                TargetSetting = "RM_JS_MA_Import_UploadFileName",
                NewValue = importParam.FileName
            };
            info.ModifyContent.Add(audit);
        }

        private async System.Threading.Tasks.Task CollectEscalateOrRejectAsync(RMAuditInfo info, object[] args)
        {
            var definition = args[0] as ManualAprovalEscalateDefinition;

            if (definition.ItemIds.Count == 1)
            {
                var item = Repository.QueryItemsAsync(item => definition.ItemIds.Contains(item.Id)).GetAwaiter().GetResult().FirstOrDefault();
                var reviewerAudit = new AuditItem
                {
                    TargetSetting = "RM_JS_MA_Grid_RecordOwner",
                    OldValue = string.Join("; ", await GetUserDisplayNameAsync(item?.ManualReviewer)) + "; "
                };
                info.ModifyContent.Add(reviewerAudit);
            }
        }


        private string ExtendTypeI18n(ManualApprovalExtendType extendType)
        {
            switch (extendType)
            {
                case ManualApprovalExtendType.After1Month:
                    return "RM_MA_EntendDisposalTime_1Month";
                case ManualApprovalExtendType.After3Month:
                    return "RM_MA_EntendDisposalTime_3M";
                case ManualApprovalExtendType.After6Month:
                    return "RM_MA_EntendDisposalTime_6M";
                case ManualApprovalExtendType.After1Year:
                    return "RM_MA_EntendDisposalTime_1Y";
                case ManualApprovalExtendType.Month:
                    return "RM_MA_EntendDisposalTime_Month";
                case ManualApprovalExtendType.Year:
                    return "RM_MA_EntendDisposalTime_Year";
            }

            return "";
        }

        private async Task<List<string>> GetUserDisplayNameAsync(int[] userIntIds)
        {
            if (userIntIds == null || userIntIds.Length == 0)
            {
                return new List<string>();
            }
            var users = await AccountDao.GetUserByIdsAsync(userIntIds.ToHashSet().ToList());
            var displayNames = users.ConvertAll(item => item.UserPrincipalName);
            return displayNames;
        }

    }
    public enum TimeRange
    {
        None = 0,
        After3Month = 1,
        After6Month = 2,
        After1Year = 3,
        Custom = 4,
        All = 5,
    }
}

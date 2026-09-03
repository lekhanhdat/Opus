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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.Schedule.AuditHandler
{
    public class RMScheduleAfterAuditHandler : IAfterAuditHandler
    {
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private RALogger logger = RALogger.GetInstance(typeof(RMScheduleAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(Contract.RMWeb.Audit.RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            try
            {
                if (action == (int)AuditAction.ConfigureSharePointSettingsSchedule)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(info.Object))
                        {
                            if (args.Length == 2)
                            {
                                //info.Object = EncodeUtil.DecryptByCommunicationKey(args[1] as string);
                                info.Object = args[1] as string;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("get DisposalSchedule node path error {0}", e.ToString());
                    }
                    return info;
                }

                if (returnValue != null && returnValue.ToString() == "-1")
                {
                    info.Status = 1;
                    return null;
                }
                //info.Action = (AuditAction)action;
                //info.Module = (AuditModule)model;
                //info.Category = (AuditCategory)category;
                List<ScheduleInfo> scheduleInfos = new List<ScheduleInfo>();
                if (args.Length == 0)
                {
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = string.Empty, NewValue = "RM_JS_ScheduleSetting_NoSchedule" });
                    return info;
                }
                ScheduleInfo schedule = args[0] as ScheduleInfo;
                if (schedule == null)
                {
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = string.Empty, NewValue = "RM_JS_ScheduleSetting_NoSchedule" });
                    return info;
                }

                if (schedule.JobCategory == ScheduleType.DisposalSchedule 
                    || schedule.JobCategory == ScheduleType.EXODisposalSchedule
                    || schedule.JobCategory == ScheduleType.PRDisposalSchedule 
                    || schedule.JobCategory == ScheduleType.FSDisposalSchedule
                    || schedule.JobCategory == ScheduleType.SPOnPremDisposalSchedule 
                    || schedule.JobCategory == ScheduleType.OneDriveDisposalSchedule
                    || schedule.JobCategory == ScheduleType.SPArchiveJobSchedule 
                    || schedule.JobCategory == ScheduleType.OneDriveArchiveJobSchedule
                    || schedule.JobCategory == ScheduleType.BoxDisposalSchedule
                    || schedule.JobCategory == ScheduleType.ColletionDataSchedule
                    || schedule.JobCategory == ScheduleType.TeamsDisposalSchedule
                    || schedule.JobCategory == ScheduleType.TeamsArchiveJobSchedule
                    )
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = RMScheduleAuditUtil.GetDisposalScheduleAction(schedule.JobCategory);
                    var tempSchedule = await ScheduleService.GetScheduleAsync(schedule.ProfileId, schedule.JobCategory);
                    if (tempSchedule != null)
                    {
                        scheduleInfos.Add(tempSchedule);
                    }
                    try
                    {
                        if (string.IsNullOrEmpty(info.Object))
                        {
                            if (args.Length == 3)
                            {
                                info.Object = args[2] as string;
                                //info.Object = EncodeUtil.DecryptByCommunicationKey(args[2] as string);
                            }
                            if (args.Length == 2)
                            {
                                info.Object = args[1] as string;
                                //info.Object = EncodeUtil.DecryptByCommunicationKey(args[1] as string);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("get DisposalSchedule node path error {0}", e.ToString());
                    }
                }
                else if (schedule.JobCategory == ScheduleType.GoogleDisposalSchedule)
                {
                    info.Module = AuditModule.GoogleDrive;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = RMScheduleAuditUtil.GetDisposalScheduleAction(schedule.JobCategory);
                    var tempSchedule = await ScheduleService.GetScheduleAsync(schedule.ProfileId, schedule.JobCategory);
                    if (tempSchedule != null)
                    {
                        scheduleInfos.Add(tempSchedule);
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(info.Object))
                        {
                            if (args.Length == 3)
                            {
                                info.Object = args[2] as string;
                            }
                            if (args.Length == 2)
                            {
                                info.Object = args[1] as string;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("get DisposalSchedule node path error {0}", e.ToString());
                    }
                }
                else
                {
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);

                    if (schedule.JobCategory == ScheduleType.LocationSyncSchedule)
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.LocationTermSynchronisation;
                        info.Action = AuditAction.ConfigureScheduleForLocationTermSynchronization;
                    }
                    else if (schedule.JobCategory == ScheduleType.UpdateRecordLocationSchedule)
                    {
                        info.Module = AuditModule.PhysicalRecordManagement;
                        info.Category = AuditCategory.UpdateRecordLocation;
                        info.Action = AuditAction.ConfigureUpdateRecordSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.SharePointSettingSchedule)
                    {
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.TimerJobSettings;
                        info.Action = AuditAction.ConfigureSharePointOnlineSettingsSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.TeamsSettingSchedule)
                    {
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.TimerJobSettings;
                        info.Action = AuditAction.ConfigureTeamsSettingsSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.SPOnPremApplySettingSchedule)
                    {
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.TimerJobSettings;
                        info.Action = AuditAction.ConfigureSPOnPremApplySettingSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.GoogleSettingSchedule)
                    {
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.SharePointSettings;
                        info.Action = AuditAction.ConfigureGoogleApplySettingSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.ManualApprovalSchedule)
                    {
                        info.Module = AuditModule.RetentionAndDisposalManagement;
                        info.Category = AuditCategory.ManualApproval;
                        info.Action = AuditAction.ConfigureManualApproval;
                    }
                    else if (schedule.JobCategory == ScheduleType.UniqueIDSettingSchedule)
                    {
                        info.NotNeedRecordAudit = true;
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.SharePointSettings;
                        info.Action = AuditAction.ConfigureUniqueIDSettingSchedule;
                    }
                    else if (schedule.JobCategory == ScheduleType.EnforceRetention)
                    {
                        info.Module = AuditModule.BusinessClassificationManagement;
                        info.Category = AuditCategory.TermManagement;
                        info.Action = AuditAction.RunEnforceRetentionJob;
                    }
                    else
                    {
                        info.Module = AuditModule.ControlPanel;
                        info.Category = AuditCategory.TimerJobSettings;

                        info.Action = schedule.JobCategory switch
                        {
                            ScheduleType.SPSyncDataSchedule => AuditAction.ConfigureSPOnlineSyncDataSchedule,
                            ScheduleType.EXOSyncDataSchedule => AuditAction.ConfigureEXOSyncDataSchedule,
                            ScheduleType.FSColletionDataSchedule => AuditAction.ConfigureFSSyncDataSchedule,
                            ScheduleType.SPOnPremDataSyncSchedule => AuditAction.ConfigureSPOnPremSyncDataSchedule,
                            ScheduleType.AzureFileShareDataSyncSchedule => AuditAction.ConfigureAzureFileShareDataSyncSchedule,
                            ScheduleType.OneDriveSyncDataSchedule => AuditAction.ConfigureOneDriveSyncDataSchedule,
                            ScheduleType.BoxDataSyncSchedule => AuditAction.ConfigureBoxDataSyncSchedule,
                            ScheduleType.GoogleDataSyncSchedule => AuditAction.ConfigureGoogleDataSyncSchedule,
                            ScheduleType.EXOApplypSchedule => AuditAction.ConfigureEXOSettingsScheduleJob,
                            ScheduleType.SPOnPremScanNodesSchedule => AuditAction.ConfigureScanLocalNodeSettingsScheduleJob,
                            ScheduleType.ArchiveDataRetentionSchedule => AuditAction.ConfigureRetentionScheduleJob,
                            ScheduleType.ArchiverDeleteRestoredDataSchedule => AuditAction.ConfigureArchiverDeleteRestoredData,
                            ScheduleType.ApprovalProcessJob => AuditAction.ApprovalProcessConfig,
                            ScheduleType.ArchiverDedupJobSchedule => AuditAction.ConfigureDedupScheduleJob,
                            ScheduleType.SyncSchedule => AuditAction.ConfigureScheduleForTermSynchronization,
                            ScheduleType.TeamsSyncDataSchedule => AuditAction.ConfigureTeamsSyncDataSchedule,
                            ScheduleType.StubDisposalSchedule => AuditAction.ConfigureStubDisposalSchedule,
                            _ => AuditAction.Unknown
                        };
                    }
                }

                if (scheduleInfos.Count == 0 || scheduleInfos[0].NoSchedule) //Schedule -> NoSchedule
                {

                    info.ModifyContent.Add(new AuditItem() { TargetSetting = string.Empty, NewValue = I18NEntity.GetString("RM_JS_ScheduleSetting_NoSchedule") });
                    if ((info.Action == AuditAction.ConfigureManualApproval))
                    {
                        if (scheduleInfos.Count > 0)
                        {
                            object o = GetExtensionValue(scheduleInfos[0]);
                            if (o != null)
                            {
                                ManualApprovalStoreLocation mas;
                                if ((mas = o as ManualApprovalStoreLocation) != null)
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_MA_LibraryUrl", NewValue = mas.Url });
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_MA_LibraryUserName", NewValue = mas.UserName });
                                }
                            }
                        }
                    }
                }
                else if ((info.Action == AuditAction.ConfigureManualApproval && info.ModifyContent.Count > 3)
                    || (info.Action != AuditAction.ConfigureManualApproval && info.ModifyContent.Count > 1))
                //Edit Schedule
                //Ohters:           count==1 No schedule 
                //ManualApproval:   connt==3 No schedule
                {
                    ScheduleInfo scheduleInfo = scheduleInfos[0];
                    AuditItem startTimeItem = info.ModifyContent[0];
                    AuditItem endTimeItem = info.ModifyContent[1];
                    AuditItem IntervalTimeItem = info.ModifyContent[2];
                    startTimeItem.NewValue = scheduleInfo.StartTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId);
                    if (scheduleInfo.EndType == EndType.EndByOccurrences)
                    {
                        endTimeItem.NewValue = "RM_JS_ScheduleSetting_EndAfter" + " " + scheduleInfo.OccurrencesTotal.ToString() + " " + I18NEntity.GetString("RM_JS_ScheduleSetting_Occurrences ");
                    }
                    else if (scheduleInfo.EndType == EndType.EndByTime)
                    {
                        endTimeItem.NewValue = scheduleInfo.EndTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId);
                    }
                    else if (scheduleInfo.EndType == EndType.NoEnd)
                    {
                        endTimeItem.NewValue = "RM_JS_ScheduleSetting_NoEndDate";
                    }
                    if (scheduleInfo.IntervalType == IntervalType.Hourly)
                    {
                        IntervalTimeItem.NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Hours ";
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Daily)
                    {
                        IntervalTimeItem.NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Days ";
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Weekly)
                    {
                        IntervalTimeItem.NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Weeks ";
                    }

                    if (RMScheduleAuditUtil.ContainsSkipRemoveScheduleTypes.Contains(schedule.JobCategory))
                    {
                        bool skip = RMScheduleAuditUtil.GetExtensionSkipValue(scheduleInfo);
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = skip ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                    }
                    if (schedule.JobCategory is ScheduleType.DisposalSchedule or ScheduleType.OneDriveDisposalSchedule)
                    {
                        if (TenantService.IsNewOpusTenant())
                        {
                            var isUseDecrypt = RMScheduleAuditUtil.GetIsUseDecryptValue(scheduleInfo);
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_BCM_EnsureRun_DecryptIRM", NewValue = isUseDecrypt ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                        }
                    }

                    if ((info.Action == AuditAction.ConfigureManualApproval))
                    {
                        var libraryUrl = info.ModifyContent[3];
                        var libraryUsername = info.ModifyContent[4];
                        if (scheduleInfos.Count > 0)
                        {
                            object o = GetExtensionValue(scheduleInfos[0]);
                            if (o != null)
                            {
                                ManualApprovalStoreLocation mas;
                                if ((mas = o as ManualApprovalStoreLocation) != null)
                                {
                                    libraryUrl.NewValue = mas.Url;
                                    libraryUsername.NewValue = mas.UserName;
                                }
                            }
                        }
                    }
                }
                else //NoSchedule -> Schedule
                {
                    var tempModidyContent = new List<AuditItem>();
                    ScheduleInfo scheduleInfo = scheduleInfos[0];
                    tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_StratTime", NewValue = scheduleInfo.StartTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId) });

                    if (scheduleInfo.EndType == EndType.EndByOccurrences)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", NewValue = "RM_JS_ScheduleSetting_EndAfter" + " " + scheduleInfo.OccurrencesTotal.ToString() + " " + "RM_JS_ScheduleSetting_Occurrences " });
                    }
                    else if (scheduleInfo.EndType == EndType.EndByTime)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", NewValue = scheduleInfo.EndTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId) });
                    }
                    else if (scheduleInfo.EndType == EndType.NoEnd)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", NewValue = "RM_JS_ScheduleSetting_NoEndDate" });
                    }

                    if (scheduleInfo.IntervalType == IntervalType.Hourly)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Hours " });
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Daily)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Days " });
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Weekly)
                    {
                        tempModidyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", NewValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Weeks " });
                    }

                    if (RMScheduleAuditUtil.ContainsSkipRemoveScheduleTypes.Contains(schedule.JobCategory))
                    {
                        bool skip = RMScheduleAuditUtil.GetExtensionSkipValue(scheduleInfo);
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", NewValue = skip ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                    }
                    if (schedule.JobCategory is ScheduleType.DisposalSchedule or ScheduleType.OneDriveDisposalSchedule or ScheduleType.TeamsDisposalSchedule)
                    {
                        if (TenantService.IsNewOpusTenant())
                        {
                            var isUseDecrypt = RMScheduleAuditUtil.GetIsUseDecryptValue(scheduleInfo);
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_BCM_EnsureRun_DecryptIRM", NewValue = isUseDecrypt ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                        }
                    }

                    info.ModifyContent.InsertRange(1, tempModidyContent);
                    if ((info.Action == AuditAction.ConfigureManualApproval))
                    {
                        if (scheduleInfos.Count > 0)
                        {
                            AuditItem libraryUrl = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_MA_LibraryUrl")).FirstOrDefault();
                            AuditItem libraryUsername = info.ModifyContent.Where(a => a.TargetSetting != null && a.TargetSetting.Equals("RM_MA_LibraryUserName")).FirstOrDefault();
                            if (scheduleInfos.Count > 0)
                            {
                                object o = GetExtensionValue(scheduleInfos[0]);
                                if (o != null)
                                {
                                    ManualApprovalStoreLocation mas;
                                    if ((mas = o as ManualApprovalStoreLocation) != null)
                                    {
                                        libraryUrl.NewValue = mas.Url;
                                        libraryUsername.NewValue = mas.UserName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("record schedule after audit handler, error message {0}", e.ToString());
            }
            return info;
        }

        private object GetExtensionValue(ScheduleInfo info)
        {
            object o = null;
            if (info.JobCategory == ScheduleType.ManualApprovalSchedule)
            {
                o = JsonConvert.DeserializeObject<ManualApprovalStoreLocation>(info.Extentions);

            }
            return o;
        }

        private string GetTimeZoneNameById(string timeZoneId)
        {
            return DateTimeUtil.GetSimplifyZoneInfo(timeZoneId);
        }
    }
}

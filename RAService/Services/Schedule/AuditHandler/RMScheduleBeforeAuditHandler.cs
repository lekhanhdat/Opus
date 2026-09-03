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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.Schedule.AuditHandler
{
    public class RMScheduleBeforeAuditHandler: IBeforeAuditHandler
    {
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private RALogger logger = RALogger.GetInstance(typeof(RMScheduleBeforeAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            if (action == (int)AuditAction.ConfigureSharePointSettingsSchedule)
            {
                info.Module = AuditModule.BusinessClassificationManagement;
                info.Category = AuditCategory.SharePointSettings;
                SettingScheduleType type = (SettingScheduleType)args[0];
                if (type == SettingScheduleType.Dispose)
                {
                    info.Action = AuditAction.ConfigureDisposalJobSchedule;
                }
                else if(type == SettingScheduleType.OneDriveDisposal)
                {
                    info.Action = AuditAction.ConfigureDisposalJobSchedule4OneDrive;
                }
                else if (type == SettingScheduleType.TeamsDisposal)
                {
                    info.Action = AuditAction.ConfigureDisposalJobSchedule4Teams;
                }
                info.ModifyContent = new List<AuditItem>();
                info.ModifyContent.Add(new AuditItem() { TargetSetting = string.Empty, NewValue = "RM_JS_ScheduleSetting_NoSchedule" });
                return info;
            }
            try
            {
                info.ModifyContent = new List<AuditItem>();
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;
                List<ScheduleInfo> scheduleInfos = new List<ScheduleInfo>();
                ScheduleInfo schedule = args[0] as ScheduleInfo;
                if (schedule == null)// delete schedule
                {
                    var scheduleId = args[0] as string;
                    if (scheduleId != null)
                    {
                        schedule = await ScheduleService.GetScheduleByIdAsync(scheduleId);
                    }
                }
                ArgumentCheck.NotNull(schedule, nameof(schedule));
                if (schedule.JobCategory == ScheduleType.LocationSyncSchedule)
                {
                    info.Module = AuditModule.PhysicalRecordManagement;
                    info.Category = AuditCategory.LocationTermSynchronisation;
                    info.Action = AuditAction.ConfigureScheduleForLocationTermSynchronization;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.LocationSyncSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.UpdateRecordLocationSchedule)
                {
                    info.Module = AuditModule.PhysicalRecordManagement;
                    info.Category = AuditCategory.UpdateRecordLocation;
                    info.Action = AuditAction.ConfigureUpdateRecordSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.UpdateRecordLocationSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.SharePointSettingSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    //info.Action = AuditAction.ConfigureSharePointSettingsSchedule;
                    info.Action = AuditAction.ConfigureSharePointOnlineSettingsSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.SharePointSettingSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.TeamsSettingSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    //info.Action = AuditAction.ConfigureSharePointSettingsSchedule;
                    info.Action = AuditAction.ConfigureTeamsSettingsSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.TeamsSettingSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.SPOnPremApplySettingSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = AuditAction.ConfigureSPOnPremApplySettingSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.SPOnPremApplySettingSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.GoogleSettingSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = AuditAction.ConfigureGoogleApplySettingSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.GoogleSettingSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.ManualApprovalSchedule)
                {
                    info.Module = AuditModule.RetentionAndDisposalManagement;
                    info.Category = AuditCategory.ManualApproval;
                    info.Action = AuditAction.ConfigureManualApproval;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.ManualApprovalSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.DisposalSchedule || schedule.JobCategory == ScheduleType.EXODisposalSchedule
                    || schedule.JobCategory == ScheduleType.PRDisposalSchedule || schedule.JobCategory == ScheduleType.FSDisposalSchedule
                    || schedule.JobCategory == ScheduleType.SPOnPremDisposalSchedule || schedule.JobCategory == ScheduleType.OneDriveDisposalSchedule
                    || schedule.JobCategory == ScheduleType.SPArchiveJobSchedule || schedule.JobCategory == ScheduleType.OneDriveArchiveJobSchedule
                    || schedule.JobCategory == ScheduleType.BoxDisposalSchedule || schedule.JobCategory == ScheduleType.TeamsDisposalSchedule
                    || schedule.JobCategory == ScheduleType.TeamsArchiveJobSchedule)
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
                else if(schedule.JobCategory == ScheduleType.GoogleDisposalSchedule)
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
                else if (schedule.JobCategory == ScheduleType.UniqueIDSettingSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = AuditAction.ConfigureUniqueIDSettingSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.UniqueIDSettingSchedule);
                }
                else if (schedule.JobCategory == ScheduleType.EnforceRetention)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.TermManagement;
                    info.Action = AuditAction.RunEnforceRetentionJob;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.EnforceRetention);
                }
                else if (schedule.JobCategory == ScheduleType.ColletionDataSchedule)
                {
                    info.Module = AuditModule.BusinessClassificationManagement;
                    info.Category = AuditCategory.SharePointSettings;
                    info.Action = AuditAction.ConfigureCollectionJobSchedule;
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
                                //info.Object = EncodeUtil.DecryptByCommunicationKey(args[2] as string);
                                info.Object = args[2] as string;
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
                        logger.Warn("get ColletionDataSchedule node path error {0}", e.ToString());
                    }
                }
                else if (schedule.JobCategory == ScheduleType.SPSyncDataSchedule)
                {
                    info.Action = AuditAction.ConfigureSPOnlineSyncDataSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.EXOSyncDataSchedule)
                {
                    info.Action = AuditAction.ConfigureEXOSyncDataSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.FSColletionDataSchedule)
                {
                    info.Action = AuditAction.ConfigureFSSyncDataSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.SPOnPremDataSyncSchedule)
                {
                    info.Action = AuditAction.ConfigureSPOnPremSyncDataSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.AzureFileShareDataSyncSchedule)
                {
                    info.Action = AuditAction.ConfigureAzureFileShareDataSyncSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.OneDriveSyncDataSchedule)
                {
                    info.Action = AuditAction.ConfigureOneDriveSyncDataSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.BoxDataSyncSchedule)
                {
                    info.Action = AuditAction.ConfigureBoxDataSyncSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.GoogleDataSyncSchedule)
                {
                    info.Action = AuditAction.ConfigureGoogleDataSyncSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.EXOApplypSchedule)
                {
                    info.Action = AuditAction.ConfigureEXOSettingsScheduleJob;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.SPOnPremScanNodesSchedule)
                {
                    info.Action = AuditAction.ConfigureScanLocalNodeSettingsScheduleJob;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.ArchiveDataRetentionSchedule)
                {
                    info.Action = AuditAction.ConfigureRetentionScheduleJob;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.ArchiverDeleteRestoredDataSchedule)
                {
                    info.Action = AuditAction.ConfigureArchiverDeleteRestoredData;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.ApprovalProcessJob)
                {
                    info.Action = AuditAction.ApprovalProcessConfig;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.ArchiverDedupJobSchedule)
                {
                    info.Action = AuditAction.ConfigureDedupScheduleJob;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else if (schedule.JobCategory == ScheduleType.StubDisposalSchedule)
                {
                    info.Action = AuditAction.ConfigureStubDisposalSchedule;
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(schedule.JobCategory);
                }
                else
                {
                    scheduleInfos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.SyncSchedule);
                }

                if (schedule.JobCategory == ScheduleType.SyncSchedule || schedule.JobCategory == ScheduleType.LocationSyncSchedule 
                    || schedule.JobCategory == ScheduleType.UpdateRecordLocationSchedule || schedule.JobCategory == ScheduleType.SharePointSettingSchedule 
                    || schedule.JobCategory == ScheduleType.SPSyncDataSchedule || schedule.JobCategory == ScheduleType.EXOSyncDataSchedule 
                    || schedule.JobCategory == ScheduleType.EXOApplypSchedule || schedule.JobCategory == ScheduleType.FSColletionDataSchedule
                    || schedule.JobCategory == ScheduleType.SPOnPremApplySettingSchedule || schedule.JobCategory == ScheduleType.SPOnPremDataSyncSchedule
                    || schedule.JobCategory == ScheduleType.SPOnPremScanNodesSchedule || schedule.JobCategory == ScheduleType.OneDriveSyncDataSchedule
                    || schedule.JobCategory == ScheduleType.AzureFileShareDataSyncSchedule || schedule.JobCategory == ScheduleType.ArchiveDataRetentionSchedule
                    || schedule.JobCategory == ScheduleType.BoxDataSyncSchedule || schedule.JobCategory == ScheduleType.ArchiverDeleteRestoredDataSchedule                    
                    || schedule.JobCategory == ScheduleType.ApprovalProcessJob || schedule.JobCategory == ScheduleType.ArchiverDedupJobSchedule                    
                    || schedule.JobCategory == ScheduleType.GoogleSettingSchedule || schedule.JobCategory == ScheduleType.GoogleDataSyncSchedule
                    || schedule.JobCategory == ScheduleType.TeamsSettingSchedule || schedule.JobCategory == ScheduleType.StubDisposalSchedule)
                                       
                {
                    info.Module = AuditModule.ControlPanel;
                    info.Category = AuditCategory.TimerJobSettings;
                }

                if (scheduleInfos.Count == 0 || scheduleInfos[0].NoSchedule)
                {
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = string.Empty, OldValue = "RM_JS_ScheduleSetting_NoSchedule" });
                }
                else
                {
                    ScheduleInfo scheduleInfo = scheduleInfos[0];
                   
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_StratTime", OldValue = scheduleInfo.StartTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId) });

                    if (scheduleInfo.EndType == EndType.EndByOccurrences)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", OldValue = "RM_JS_ScheduleSetting_EndAfter" + " " + scheduleInfo.OccurrencesTotal.ToString() + " " + "RM_JS_ScheduleSetting_Occurrences " });
                    }
                    else if (scheduleInfo.EndType == EndType.EndByTime)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", OldValue = scheduleInfo.EndTime + " " + GetTimeZoneNameById(scheduleInfo.TimeZoneId) });
                    }
                    else if (scheduleInfo.EndType == EndType.NoEnd)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_EndTime", OldValue = "RM_JS_ScheduleSetting_NoEndDate" });
                    }
                    if (scheduleInfo.IntervalType == IntervalType.Hourly)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", OldValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Hours " });
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Daily)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", OldValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Days " });
                    }
                    else if (scheduleInfo.IntervalType == IntervalType.Weekly)
                    {
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_TS_IntervalTime", OldValue = scheduleInfo.Interval + " " + "RM_JS_ScheduleSetting_Weeks " });
                    }

                    if (RMScheduleAuditUtil.ContainsSkipRemoveScheduleTypes.Contains(schedule.JobCategory))
                    {
                        bool skip = RMScheduleAuditUtil.GetExtensionSkipValue(scheduleInfo);
                        info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_ScheduleSetting_Skip", OldValue = skip ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                    }

                    if (schedule.JobCategory is ScheduleType.DisposalSchedule or ScheduleType.OneDriveDisposalSchedule)
                    {
                        if (TenantService.IsNewOpusTenant())
                        {
                            var isUseDecrypt = RMScheduleAuditUtil.GetIsUseDecryptValue(scheduleInfo);
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_BCM_EnsureRun_DecryptIRM", OldValue = isUseDecrypt ? "RM_JS_Common_Yes" : "RM_JS_Common_No" });
                        }
                    }
                }
                if (scheduleInfos.Count != 0)
                {
                    object o = GetExtensionValue(scheduleInfos[0]);
                    if (o != null)
                    {
                        ManualApprovalStoreLocation mas;
                        if ((mas = o as ManualApprovalStoreLocation) != null)
                        {
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_MA_LibraryUrl", OldValue = mas.Url });
                            info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_MA_LibraryUserName", OldValue = mas.UserName });
                        }

                    }
                }
                
            }
            catch (Exception e)
            {
                logger.Warn("record schedule before audit handler, error message {0}", e.ToString());
                throw;
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

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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Utility;
using Newtonsoft.Json;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Service.Services.CommonExtension;

namespace AvePoint.RA.Service.Services.RMReport.AuditHandler
{
    public class TermUsageOrDueForDisposalBeforeAuditHandler : IBeforeAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(TermUsageOrDueForDisposalBeforeAuditHandler));

        public IProfileDao profileDAO => PlatformWindsorManager.GetService<IProfileDao>();

        private static readonly Dictionary<DayOfWeek, string> WeeklyTypesI18N = new()
        {
            { DayOfWeek.Monday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Monday") },
            { DayOfWeek.Tuesday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Tuesday") },
            { DayOfWeek.Wednesday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Wednesday") },
            { DayOfWeek.Thursday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Thursday") },
            { DayOfWeek.Friday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Friday") },
            { DayOfWeek.Saturday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Saturday") },
            { DayOfWeek.Sunday, I18NEntity.GetString("RM_JS_JN_WeeklyType_Sunday") },
        };

        private static readonly Dictionary<NotificationJobType, string> JobTypeI18N = new()
        {
            {NotificationJobType.RMArchiverBackup,I18NEntity.GetString("RM_JS_JM_JobType_RMArchiverBackup") },
            {NotificationJobType.SOPreScan,I18NEntity.GetString("RM_JS_JM_JobType_SOPreScan") },
            {NotificationJobType.EnforceRetention,I18NEntity.GetString("RM_JS_JM_JobType_EnforceRetention") },
            {NotificationJobType.ArchiverRestore,I18NEntity.GetString("RM_JS_JN_JobType_Restore") },
            {NotificationJobType.Discovery,I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryJobV3") },
            {NotificationJobType.DataSync, I18NEntity.GetString("RM_JS_JM_JobType_DataSynchronisation") },
            {NotificationJobType.EnforceRuleAction,I18NEntity.GetString("RM_JS_JM_JobType_DisposalActivityManagement") },
            {NotificationJobType.SyncNode,I18NEntity.GetString("RM_JS_JM_JobType_SyncNodesFromAOS") },
            {NotificationJobType.DashboardData,I18NEntity.GetString("RM_JS_JM_JobType_Dashboard") },
            {NotificationJobType.TermSync,I18NEntity.GetString("RM_JS_JM_JobType_TermSynchronization") },
        };        
        
        private static readonly Dictionary<JobStatus, string> JobStatusI18N = new Dictionary<JobStatus, string>
        {
            { JobStatus.Finished, I18NEntity.GetString("RM_JS_JM_Status_Finished")},
            { JobStatus.Failed, I18NEntity.GetString("RM_JS_JM_Status_Failed") },
            { JobStatus.FinishWithException, I18NEntity.GetString("RM_JS_JM_Status_FinishWithException") },
            { JobStatus.Stopped, I18NEntity.GetString("RM_JS_JM_Status_Stopped") },
            { JobStatus.Skipped, I18NEntity.GetString("RM_JS_JM_Status_Skipped") },
        };

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            try
            {
                switch ((AuditAction)action)
                {
                    case AuditAction.DeleteJobNotificationProfile:
                        List<int> profileIds = (List<int>)args[0];
                        info.Object = string.Join("; ", profileDAO.GetProfileByIds(profileIds).Select(p => p.Name));
                        break;
                    case AuditAction.DeleteProfile:
                        DelProfileInfo dpi = (DelProfileInfo)args[0];
                        info.Object = string.Join(";",profileDAO.GetProfileByIds(dpi.ProfileNames.Keys.ToList()));
                        break;

                    case AuditAction.ExportContentDueDisposalReport:
                        break;

                    case AuditAction.ExportBCSTermUsageReport:
                        break;
                    case AuditAction.CreateProfile:
                    case AuditAction.EditProfile:
                        RMProfileDto profileDto = (RMProfileDto)args[0];
                        RMProfile oldProfile = profileDAO.GetProfileById(((RMProfileDto)args[0]).Id);
                        info.Object = profileDto.ProfileName;

                        AuditItem profileNameAuditItem = new AuditItem();
                        profileNameAuditItem.TargetSetting = I18NEntity.GetString("RM_JS_RC_DueDisposal_ProfileName");
                        profileNameAuditItem.NewValue = profileDto.ProfileName;
                        if(action != (int)AuditAction.CreateProfile)
                        {
                            profileNameAuditItem.OldValue = oldProfile.Name;
                        }

                        AuditItem profileDesAuditItem = new AuditItem();
                        profileDesAuditItem.TargetSetting = I18NEntity.GetString("RM_RC_Profile_Description");
                        profileDesAuditItem.NewValue = profileDto.Description;
                        if (action != (int)AuditAction.CreateProfile)
                        {
                            profileDesAuditItem.OldValue = oldProfile.Description;
                        }

                        info.ModifyContent.Add(profileNameAuditItem);
                        info.ModifyContent.Add(profileDesAuditItem); 
                        //Term Usage Type
                        if (profileDto.Type == JobType.OrphanedTermReport || profileDto.Type == JobType.RetiredTermReport || profileDto.Type == JobType.BCSTermUsageReport
                            || profileDto.Type == JobType.TeamsOrphanedTermUsageReport || profileDto.Type == JobType.TeamsRetiredTermUsageReport || profileDto.Type == JobType.TeamsBCSTermUsageReport
                            || profileDto.Type == JobType.FSBCSTermUsageReport || profileDto.Type == JobType.FSOrphanedTermReport || profileDto.Type == JobType.FSRetiredTermReport
                            || profileDto.Type == JobType.EXOTermUsageReport || profileDto.Type == JobType.EXOOrphanedTermUsageReport || profileDto.Type == JobType.EXORetiredTermUsageReport
                            || profileDto.Type == JobType.PhysicalTermUsageReport || profileDto.Type == JobType.PhysicalOrphanedTermUsageReport || profileDto.Type == JobType.PhysicalRetiredTermUsageReport
                            || profileDto.Type == JobType.OneDriveTermUsageReport || profileDto.Type == JobType.OneDriveOrphanedTermUsageReport || profileDto.Type == JobType.OneDriveRetiredTermUsageReport
                            || profileDto.Type == JobType.SPOnPremBCSTermUsageReport || profileDto.Type == JobType.SPOnPremOrphanedTermReport || profileDto.Type == JobType.SPOnPremRetiredTermReport
                            || profileDto.Type == JobType.BoxBCSTermUsageReport || profileDto.Type == JobType.BoxOrphanedTermUsageReport || profileDto.Type == JobType.BoxRetiredTermUsageReport
                            || profileDto.Type == JobType.GoogleBCSTermUsageReport || profileDto.Type == JobType.GoogleOrphanedTermUsageReport || profileDto.Type == JobType.GoogleRetiredTermUsageReport)
                        {
                            var profileTermTypeAuditsItemForTeams = new AuditItem
                            {
                                TargetSetting = I18NEntity.GetString("RM_JS_TermUsageReport_SelectReportType"),
                                NewValue = GetReportTypeString(profileDto.Type)
                            };

                            if (action != (int)AuditAction.CreateProfile)
                            {
                                var oldJobType = (JobType)oldProfile.Type;
                                profileTermTypeAuditsItemForTeams.OldValue = GetReportTypeString(oldJobType);
                            }

                            info.ModifyContent.Add(profileTermTypeAuditsItemForTeams); 
                        }
                        else if (profileDto.Type == JobType.JobNotification)
                        {
                            var newJobNotificationInfo = SerializerHelper.DeserializeByDataContractSerializer<JobNotificationDto>(profileDto.Extension1);
                            var profileUsersAduitItem = new AuditItem();
                            profileUsersAduitItem.TargetSetting = I18NEntity.GetString("RM_JS_JN_Receiver"); 
                            profileUsersAduitItem.NewValue = string.Join("; ", newJobNotificationInfo.ProfileEmailReceivers.Select(r => r.DisplayName));

                            var profileIntervalAduitItem = new AuditItem();
                            profileIntervalAduitItem.TargetSetting = I18NEntity.GetString("RM_JS_JN_Interval");
                            profileIntervalAduitItem.NewValue = GetIntervalString(newJobNotificationInfo.ProfileInterval);

                            var profileJobStatusAduitItem = new AuditItem();
                            profileJobStatusAduitItem.TargetSetting = I18NEntity.GetString("RM_JS_JN_JobStatus");
                            profileJobStatusAduitItem.NewValue = GetJobStatusString(newJobNotificationInfo.ProfileJobInfos);

                            if (action != (int)AuditAction.CreateProfile)
                            {
                                var oldJobNotificationInfo = SerializerHelper.DeserializeByDataContractSerializer<JobNotificationDto>(oldProfile.Extension1);
                                profileUsersAduitItem.OldValue = string.Join("; ", oldJobNotificationInfo.ProfileEmailReceivers.Select(r => r.DisplayName));
                                profileIntervalAduitItem.OldValue = GetIntervalString(oldJobNotificationInfo.ProfileInterval);
                                profileJobStatusAduitItem.OldValue = GetJobStatusString(oldJobNotificationInfo.ProfileJobInfos);
                            }
                            info.ModifyContent.Add(profileUsersAduitItem);
                            info.ModifyContent.Add(profileIntervalAduitItem);
                            info.ModifyContent.Add(profileJobStatusAduitItem);
                        }
                        //Content Due for Action Time
                        else if (profileDto.Extension1 != "null" && !string.IsNullOrEmpty(profileDto.Extension1) && profileDto.Type != JobType.SPOActionAuditReport && profileDto.Type != JobType.OneDriveActionAuditReport && profileDto.Type != JobType.TeamsActionAuditReport)
                        {
                            AuditItem reportedTimeAuditItem = new AuditItem();
                            var newDateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(profileDto.Extension1);
                            DateTime newStartUtcDt = DateTime.Parse(newDateTimeObj.StartTime);
                            newStartUtcDt = DateTime.SpecifyKind(newStartUtcDt, DateTimeKind.Utc);
                            reportedTimeAuditItem.TargetSetting = I18NEntity.GetString("RM_RC_DueDisposalViewDetail_Time");
                            reportedTimeAuditItem.NewValue = newStartUtcDt.ToString();
                            if (!string.IsNullOrEmpty(newDateTimeObj.EndTime))
                            {
                                DateTime newEndUtcDt = DateTime.Parse(newDateTimeObj.EndTime);
                                newEndUtcDt = DateTime.SpecifyKind(newEndUtcDt, DateTimeKind.Utc);
                                reportedTimeAuditItem.NewValue = String.Concat(newStartUtcDt, "-", newEndUtcDt);
                            }
                            

                            if (action != (int)AuditAction.CreateProfile &&  !string.IsNullOrEmpty(oldProfile.Extension1.ToString()) && oldProfile.Extension1.ToString()!= "null") 
                            {
                                var oldDateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(oldProfile.Extension1);

                                DateTime oldStartUtcDt = DateTime.Parse(oldDateTimeObj.StartTime);
                                oldStartUtcDt = DateTime.SpecifyKind(oldStartUtcDt, DateTimeKind.Utc);
                                reportedTimeAuditItem.OldValue = oldStartUtcDt.ToString();
                                
                                if (!string.IsNullOrEmpty(oldDateTimeObj.EndTime))
                                {
                                    DateTime oldEndUtcDt = DateTime.Parse(oldDateTimeObj.EndTime);
                                    oldEndUtcDt = DateTime.SpecifyKind(oldEndUtcDt, DateTimeKind.Utc);
                                    reportedTimeAuditItem.OldValue = String.Concat(oldStartUtcDt, "-", oldEndUtcDt);
                                }
                            }
                            info.ModifyContent.Add(reportedTimeAuditItem);
                        }
                        //Creation and Destruction action Type
                        if (profileDto.IsCreated != false || profileDto.IsDestoryed != false)
                        {
                            AuditItem actionTypeAduitItem = new AuditItem();
                            actionTypeAduitItem.TargetSetting = I18NEntity.GetString("RM_JS_RC_TimeFrame_OprationType");
                            if(profileDto.IsCreated == true && profileDto.IsDestoryed == true)
                            {
                                actionTypeAduitItem.NewValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Create") + '\n' + I18NEntity.GetString("RM_JS_RC_TimeFrame_Destroyed");
                            }
                            else if(profileDto.IsCreated == true && profileDto.IsDestoryed == false)
                            {
                                actionTypeAduitItem.NewValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Create");
                            }
                            else 
                            {
                                actionTypeAduitItem.NewValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Destroyed");
                            }
                            if (oldProfile != null && oldProfile.IsCreated == true && oldProfile.IsDestoryed == true)
                            {
                                actionTypeAduitItem.OldValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Create") + '\n' + I18NEntity.GetString("RM_JS_RC_TimeFrame_Destroyed");
                            }
                            else if (oldProfile != null && oldProfile.IsCreated == true && oldProfile.IsDestoryed == false)
                            {
                                actionTypeAduitItem.OldValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Create");
                            }
                            else if (oldProfile != null && oldProfile.IsCreated == false && oldProfile.IsDestoryed == true)
                            {
                                actionTypeAduitItem.OldValue = I18NEntity.GetString("RM_JS_RC_TimeFrame_Destroyed");
                            }
                            AuditItem RangeTypeAduitItem = new AuditItem();
                            TimeRangeType oldTimeRange= oldProfile == null? new TimeRangeType() : (TimeRangeType)oldProfile.RangeType;
                            RangeTypeAduitItem.TargetSetting = I18NEntity.GetString("RM_JS_RC_TimeFrame_Range");
                            RangeTypeAduitItem.NewValue = profileDto.RangeType.ConvertI18NTimeRangeType();
                            if (action != (int)AuditAction.CreateProfile)
                            {
                                RangeTypeAduitItem.OldValue = oldTimeRange.ConvertI18NTimeRangeType();
                            }

                            var idx = info.ModifyContent.FindIndex(x => x.TargetSetting == I18NEntity.GetString("RM_RC_DueDisposalViewDetail_Time"));
                            if (idx == -1)
                            {
                                info.ModifyContent.Add(actionTypeAduitItem);
                                info.ModifyContent.Add(RangeTypeAduitItem);
                            }
                            else
                            {
                                info.ModifyContent.Insert(idx, actionTypeAduitItem);
                                info.ModifyContent.Insert(idx + 1, RangeTypeAduitItem);
                            }
                        }

                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return info;
        }

        private string GetReportTypeString(JobType jobType)
        {
            return jobType switch
            {
                JobType.TeamsBCSTermUsageReport or JobType.BCSTermUsageReport or JobType.FSBCSTermUsageReport or
                JobType.EXOTermUsageReport or JobType.PhysicalTermUsageReport or JobType.OneDriveTermUsageReport or
                JobType.SPOnPremBCSTermUsageReport or JobType.BoxBCSTermUsageReport or JobType.GoogleBCSTermUsageReport => I18NEntity.GetString("RM_JS_TermUsageReport_ActiveTermsReport"),

                JobType.TeamsOrphanedTermUsageReport or JobType.OrphanedTermReport or JobType.FSOrphanedTermReport or
                JobType.EXOOrphanedTermUsageReport or JobType.PhysicalOrphanedTermUsageReport or JobType.OneDriveOrphanedTermUsageReport or
                JobType.SPOnPremOrphanedTermReport or JobType.BoxOrphanedTermUsageReport or JobType.GoogleOrphanedTermUsageReport => I18NEntity.GetString("RM_JS_TermUsageReport_OrphanTermsReport"),

                JobType.TeamsRetiredTermUsageReport or JobType.RetiredTermReport or JobType.FSRetiredTermReport or
                JobType.EXORetiredTermUsageReport or JobType.PhysicalRetiredTermUsageReport or JobType.OneDriveRetiredTermUsageReport or
                JobType.SPOnPremRetiredTermReport or JobType.BoxRetiredTermUsageReport or JobType.GoogleRetiredTermUsageReport => I18NEntity.GetString("RM_JS_TermUsageReport_RetiredTermsReport"),
                _ => jobType.ToString()
            };
        }

        private string GetIntervalString(NotificationInterval profileInterval)
        {
            if(profileInterval.IntervalType == NotificationIntervalType.Daily)
            {
                return I18NEntity.GetString("RM_JS_JN_IntervalType_Daily");
            }

            return I18NEntity.GetString("RM_JS_JN_Every") + " " + WeeklyTypesI18N[profileInterval.WeeklyType];
        }
        
        private string GetJobStatusString(List<NotificationJobInfo> profileJobInfos)
        {
            var stringBuilder = new StringBuilder();
            foreach(var jobInfo in profileJobInfos)
            {
                stringBuilder.AppendLine(JobTypeI18N[jobInfo.JobType] + ": " + string.Join("; ", jobInfo.JobStatuses.Select(j => JobStatusI18N[j])));
            }
            return stringBuilder.ToString();
        }
    }
}

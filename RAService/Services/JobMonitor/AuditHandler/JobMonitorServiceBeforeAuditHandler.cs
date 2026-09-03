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
using AvePoint.RA.Common.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.JobMonitor.AuditHandler
{
    public class JobMonitorServiceBeforeAuditHandler : IBeforeAuditHandler
    {
        private IRMJobQueueDao  mRMJobQueueDao => PlatformWindsorManager.GetService<IRMJobQueueDao >();
        private IJobMonitorDao  mJobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao >();
        private IRMJobExportSettingDao JESDao => PlatformWindsorManager.GetService<IRMJobExportSettingDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            info.Module = (AuditModule)model;
            info.Category = (AuditCategory)category;
            info.Action = (AuditAction)action;
            if (action == (int)AuditAction.DeleteQueues)
            {
                var jobqueue = mRMJobQueueDao.GetQueue(args[0].ToString(), args[1].ToString());
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_JobType", NewValue = "RM_JS_JM_JobType_" + ((JobType)jobqueue.JobType).ToString() });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_CreatedBy", NewValue = jobqueue.JobRunBy.ToString() });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_CreatedTime", NewValue = (await GeneralSettingService.ConvertTiksToDateTimeAsync(jobqueue.CreateTime, true)).SimplifyFormatTime });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_Priority", OldValue = "RM_JS_JM_Priority_" + jobqueue.JobPriority.ToString() });
            }
            else if (action == (int)AuditAction.UpdateJobQueuePriority)
            {
                var jobqueue = mRMJobQueueDao.GetQueue(args[0].ToString(), args[2].ToString());
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_JobType", OldValue = "RM_JS_JM_JobType_" + ((JobType)jobqueue.JobType).ToString(), NewValue = "RM_JS_JM_JobType_" + ((JobType)jobqueue.JobType).ToString() });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_CreatedBy", OldValue = jobqueue.JobRunBy.ToString(), NewValue = jobqueue.JobRunBy.ToString() });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_CreatedTime", OldValue = (await GeneralSettingService.ConvertTiksToDateTimeAsync(jobqueue.CreateTime, true)).SimplifyFormatTime, NewValue = (await GeneralSettingService.ConvertTiksToDateTimeAsync(jobqueue.CreateTime, true)).SimplifyFormatTime });
                info.ModifyContent.Add(new AuditItem() { TargetSetting = "RM_JS_JM_AA_Priority", OldValue = "RM_JS_JM_Priority_" + jobqueue.JobPriority.ToString(), NewValue = "RM_JS_JM_Priority_" + ((JobPriority)args[1]).ToString() });
            }
            else if (action == (int)AuditAction.UpdateJobMonitorPriority)
            {
                var jobIds = args[0] as List<string>;
                var jobPriority = (JobPriority)args[1];
                var jobMonitors = mJobMonitorDao.GetJobs(jobIds);
                info.ModifyContent.Add(new AuditItem()
                {
                    TargetSetting = "RM_JS_JM_AA_Priority",
                    OldValue = jobMonitors.Count > 0 ? string.Join(";", jobMonitors.Select(j => $"[{j.Id}:{I18NEntity.GetString("RM_JS_JM_Priority_" + j.JobPriority.ToString())}]")) : string.Empty,
                    NewValue = "RM_JS_JM_Priority_" + ((JobPriority)args[1]).ToString(),
                });
            }
            else if (action == (int)AuditAction.ConfigDownloadSettings)
            {
                var setting = JESDao.GetExportSetting();
                string oldValue = string.Empty;
                if (setting == null)
                {
                    oldValue = I18NEntity.GetString("RM_SPS_NoRecordOwner");
                }
                else
                {
                    var useBrowser = setting.ExportSetting == 0;

                    oldValue = useBrowser ? I18NEntity.GetString("RM_EL_Radio_Browser") : I18NEntity.GetString("RM_EL_Radio_Location") + string.Format(" [{0}]", setting.LocationName);
                }
                info.ModifyContent.Add(new AuditItem() { OldValue = oldValue });
            }
            return info;
        }
    }
}

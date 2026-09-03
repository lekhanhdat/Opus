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
using System;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RestoreReport.Statistic;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.Item.Restore;
using AvePoint.RA.Common;

namespace AvePoint.RA.SharePoint.Teams.ReportData
{
    public class TeamsRestoreReportProcessor
    {
        private RMCreationJobMessage msg = null;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private StatisticRestoreJobDetailsExecutor statisticRestoreJobDetailsExecutor;
        private JobContext jobContext;
        private IRMReportManager _ReportManager;

        public IRMReportManager ReportManager
        {
            get
            {
                if (_ReportManager == null)
                {
                    _ReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return _ReportManager;
            }
        }
        private readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();
        private readonly IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();
        public TeamsRestoreReportProcessor(RMCreationJobMessage msg)
        {
            this.msg = msg;
            this.msg.EndTime = this.msg.EndTime.AddDays(1);
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
            jobContext = JobContext.GetInstance(msg.JobID, msg.JobType);
            jobContext.ReportManager.StartUpdateJobProgress();
            statisticRestoreJobDetailsExecutor = new StatisticRestoreJobDetailsExecutor(startUtcTime, endUtcTime, msg.ProfileId, jobContext, Contract.Explorer.SourceFlag.Teams);
        }

        public async Task RunReportJobAsync()
        {
            try
            {
                statisticRestoreJobDetailsExecutor.StatictisRestoreJobDetails();
                StartScheduledExport();
            }
            catch (Exception ex)
            {
               // mLog.Error($"Run report job has errors: {ex}");
            }
        }
        private void StartScheduledExport()
        {
            RMProfileDto profile = ReportService.GetProfileByIdAsync(msg.ProfileId).GetAwaiter().GetResult();
            if (profile?.ScheduleId != null)
            {
                var jobIdReal = msg.JobID?.Split('_')[0];
                var job = JobMonitorDao.GetJobById(jobIdReal);
                if (job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished || job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)
                {
                    var exportModel = new ExportReportCommonModel
                    {
                        ReportJobType = ((int)profile.Type).ToString(),
                        ReportJobId = jobIdReal,
                        ProfileName = profile.ProfileName,
                        ProfileId = profile.Id.ToString(),
                    };
                    var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                    ReportService.RunExportReportJob(reportParameters);
                }
            }
        }
    }
}

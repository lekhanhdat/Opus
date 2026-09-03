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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.Wrapper.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.RestoreReport.Worker;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Object;
using System.IO;
using AvePoint.GCommon.Utility;
using AvePoint.RA.SharePoint.RestoreReport.Statistic;
using AvePoint.RA.SharePoint.Common;
using RAExportCommon;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb.ReportCenter;

namespace AvePoint.RA.SharePoint.OneDrive.Discover
{
    public class OneDriveRestoreReportProcessor : RMOneDriveReportProcessor
    {
        

        private RMCreationJobMessage msg = null;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private StatisticRestoreJobDetailsExecutor statisticRestoreJobDetailsExecutor;
        private JobContext jobContext = null;
        private readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();
        private readonly IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();
        public OneDriveRestoreReportProcessor(RMCreationJobMessage msg)
            : base(msg.JobID, (int)JobType.RestoreReport, false)
        {
            this.msg = msg;
            this.msg.EndTime = this.msg.EndTime.AddDays(1);//包含当天
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime, globalTimeZone);
            jobContext = JobContext.GetInstance(msg.JobID, msg.JobType);
            jobContext.ReportManager.StartUpdateJobProgress();
            statisticRestoreJobDetailsExecutor = new StatisticRestoreJobDetailsExecutor(startUtcTime, endUtcTime, msg.ProfileId, jobContext);
        }

        public override async Task RunReportJobAsync()
        {
            try
            {
                statisticRestoreJobDetailsExecutor.StatictisRestoreJobDetails();
                StartScheduledExport();
            }
            catch (Exception ex)
            {
                mLog.Error($"Run Report job fail, error message:{ex.Message},error:{ex}");
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


        protected override int ProcessItems(IAveWeb web, IAveList list, List<BaseRecordDto> items)
        {
            throw new NotImplementedException();
        }
    }
}

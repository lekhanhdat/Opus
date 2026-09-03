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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using RAReportCenter.ClientAuditReport.Scanner;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.ClientAuditReport
{
    public class ClientAuditReportProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ClientAuditReportProcessor));
        private IRMReportService mReportService;
        private readonly IJobMonitorDao mJobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }

        public async Task ProcessAsync(string jobId, string profileId)
        {
            Logger.Info("ClientAuditReportProcessor Process start.");
            var reportModel = await ReportService.GetProfileByIdAsync(profileId);

            ReportMangerFactory.Instance.Init(jobId, reportModel.Type, true);

            var scanner = reportModel.Type switch
            {
                JobType.TeamsActionAuditReport => new TeamsAuditReportScanner(reportModel, jobId, reportModel.Type),

                JobType.SPOActionAuditReport or 
                JobType.OneDriveActionAuditReport => new SharePointOnlineAuditReportScanner(reportModel, jobId, reportModel.Type),

                _ => throw new Exception($"Action audit report does not support this job type: {reportModel.Type}")
            };
            scanner.Scan();

            StartScheduledExport(reportModel, jobId);

            Logger.Info("ClientAuditReportProcessor Process end.");
        }

        private void StartScheduledExport(RMProfileDto profile, string jobId)
        {
            if (profile?.ScheduleId == null)
            {
                return;
            }

            var mainJobId = jobId?.Split('_')[0];
            var job = mJobMonitorDao.GetJobById(mainJobId);
            if (job?.Status != (int)JobStatus.Finished && job?.Status != (int)JobStatus.FinishWithException)
            {
                Logger.Info("Action-audit report export was not started because the main job is not finished. JobId:{0}, Status:{1}", mainJobId, job?.Status);
                return;
            }

            var exportModel = new ExportReportCommonModel
            {
                ReportJobType = ((int)profile.Type).ToString(),
                ReportJobId = mainJobId,
                ProfileName = profile.ProfileName,
                ProfileId = profile.Id.ToString(),
            };
            var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
            ReportService.RunExportReportJob(reportParameters);
            Logger.Info("Started scheduled action-audit report export. JobId:{0}, ProfileId:{1}, JobType:{2}", mainJobId, profile.Id, profile.Type);
        }
    }
}

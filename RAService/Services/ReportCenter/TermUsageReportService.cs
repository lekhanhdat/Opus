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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.ReportCenter.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.ReportCenter.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ReportCenter
{
    public class TermUsageReportService : RMServiceBase, ITermUsageReportService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TermUsageReportService));

        private ITermUsageReportDao TermUsageReportDao => PlatformWindsorManager.GetService<ITermUsageReportDao>();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public async Task<bool> Create(TermUsageReportModel reportInfo)
        {
            try
            {
                var profileModel = TermUsageReportAdapter.ConvertToDbModel(reportInfo);
                profileModel.Type = (int)JobType.TermUsageReport;
                return await TermUsageReportDao.Create(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while create term usage report. Error: {e}");
                return false;
            }
        }

        public async Task<TermUsageReportModel> Get(int id)
        {
            try
            {
                var profileModel = await TermUsageReportDao.Get(id);
                return TermUsageReportAdapter.ConvertToReportModel(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get term usage report by: [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<bool> Edit(TermUsageReportModel reportInfo)
        {
            try
            {
                var profileModel = TermUsageReportAdapter.ConvertToDbModel(reportInfo);
                return await TermUsageReportDao.Edit(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while edit term usage report by: [{reportInfo.Id}]. Error: {e}");
                return false;
            }
        }

        public bool GenerateReportJob(int id)
        {
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto()
                {
                    JobType = JobType.TermUsageReport,
                    Parameters = id.ToString(),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                var jobId = JobQueueService.AddToDBJobQueue(jqDto);
                return !string.IsNullOrEmpty(jobId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while generate report job. Error: {e}");
                return false;
            }
        }

        public string RealRunReportJob(int id)
        {
            Logger.Info("Start run term usage report job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                jobId = JobMonitorService.CreateJobWithProfileId(JobType.TermUsageReport, username, id);
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.TermUsageReport,
                    CommandLine = $"{JobType.TermUsageReport} {jobId} {id}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run term usage report job. Error: {e}");
            }

            return jobId;
        }
    }
}

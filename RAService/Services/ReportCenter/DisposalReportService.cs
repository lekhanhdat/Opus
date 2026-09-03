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
    public class DisposalReportService : RMServiceBase, IDisposalReportService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DisposalReportService));

        private IDisposalReportDao DisposalReportDao => PlatformWindsorManager.GetService<IDisposalReportDao>();

        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public async Task<bool> Create(DisposalReportModel reportInfo)
        {
            try
            {
                var profileModel = DisposalReportAdapter.ConvertToDbModel(reportInfo);
                profileModel.Type = (int)JobType.DisposalReport;
                profileModel.Extension1 = await GeneralSettingService.ConvertToUTCDateTimeAsync(profileModel.Extension1);
                return await DisposalReportDao.Create(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while create disposal report. Error: {e}");
                return false;
            }
        }

        public async Task<DisposalReportModel> Get(int id)
        {
            try
            {
                var profileModel = await DisposalReportDao.Get(id);
                profileModel.Extension1 = await GeneralSettingService.ConvertFromUTCDateTimeAsync(profileModel.Extension1);
                return DisposalReportAdapter.ConvertToReportModel(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get disposal report by: [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<bool> Edit(DisposalReportModel reportInfo)
        {
            try
            {
                var profileModel = DisposalReportAdapter.ConvertToDbModel(reportInfo);
                profileModel.Extension1 = await GeneralSettingService.ConvertToUTCDateTimeAsync(profileModel.Extension1);
                return await DisposalReportDao.Edit(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while edit disposal report by: [{reportInfo.Id}]. Error: {e}");
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
                    JobType = JobType.DisposalReport,
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
            Logger.Info("Start run disposal report job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                jobId = JobMonitorService.CreateJobWithProfileId(JobType.DisposalReport, username, id);
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.DisposalReport,
                    CommandLine = $"{JobType.DisposalReport} {jobId} {id}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run disposal report job. Error: {e}");
            }

            return jobId;
        }
    }
}

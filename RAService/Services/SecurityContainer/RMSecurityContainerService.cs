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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AvePoint.RA.Service.Services.SecurityContainer
{
    public class RMSecurityContainerService : RMServiceBase, IRMSecurityContainerService
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private IRMSecurityContainerDao RMSecurityContainerDao => PlatformWindsorManager.GetService<IRMSecurityContainerDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        //public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        public IList<NameAndIdDto> GetRootContainers(SourceFlag sourceFlag)
        {
            try
            {
                var result = RMSecurityContainerDao.GetContainers(sourceFlag);
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while get root containers, error : {e.ToString()}");
            }

            return new List<NameAndIdDto>();

        }

        public int UpSert(RMSecurityContainerDto dto)
        {
            return RMSecurityContainerDao.CreateOrUpdate(new List<RMSecurityContainerDto> { dto });
        }

        public IList<NameAndIdDto> GetSubContainers(string rootContainerId)
        {
            try
            {
                var result = RMSecurityContainerDao.GetSubContainersByParent(rootContainerId);
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while get sub containers, error : {e.ToString()}");
            }

            return new List<NameAndIdDto>();
        }

        public string RealScheduleJob(JobRunBy jobRunBy, string parameter = "", string jobRunByUser = "")
        {
            var jobType = JobType.SyncSecurityContainer;
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);

            List<string> runningJobs = RMJobService.GetRunningSyncSecurityContainerJob();

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartJob(jobType, jobId, jobRunBy, parameter);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                logger.Info("sync security container job has job running,so shedule job is skip");
            }
            return jobId;
        }

        private void StartJob(JobType jobType, string jobId, JobRunBy runBy, string parameter)
        {
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                //JobId = subJobId,
                JobId = jobId,
                RunBy = runBy,
                JobType = jobType,
                //CommandLine = string.Format("{0} {1}", jobType, subJobId),
                CommandLine = string.Format("{0} {1} {2}", jobType, jobId, parameter),
            });
        }

        public string RunScheduleJob(JobRunBy jobRunBy, string syncNodeJobId = "")
        {
            var hasGControlGoogleLicense = TenantService.HasInitGControlPlatForm().GetAwaiter().GetResult();

            if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && !hasGControlGoogleLicense)
            {
                logger.Error("No lifecycle license,can not run sync permission schedule job.");
                return string.Empty;
            }
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SyncSecurityContainer,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = syncNodeJobId,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run sync security container job,ERROR:{0}", ex.ToString());
            }
            return id;
        }
    }
}

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
using Aspose.Pdf.Operators;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class JobWebAPIController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobWebAPIController));

        string jobId = "TS20160630153200354637";

        private IJobMonitorService _JobMonitorService;

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);

        private IRMFileSystemSettingsService _RMFileSystemSettingsService;

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService(ref _RMFileSystemSettingsService);

        private IFileSystemJobTimeReferenceService _FileSystemJobTimeReferenceService;

        private IFileSystemJobTimeReferenceService FileSystemJobTimeReferenceService => PlatformWindsorManager.GetService(ref _FileSystemJobTimeReferenceService);

        private IRMSharePointOnPremSettingsService _RMSharePointOnPremSettingsService;

        private IRMSharePointOnPremSettingsService RMSharePointOnPremSettingsService => PlatformWindsorManager.GetService(ref _RMSharePointOnPremSettingsService);

        private IRMSharePointTaxonomyService _RMSharePointTaxonomyService;

        private IRMSharePointTaxonomyService RMSharePointTaxonomyService => PlatformWindsorManager.GetService(ref _RMSharePointTaxonomyService);

        private IRMReportService RMReportService  => PlatformWindsorManager.GetService<IRMReportService>();
        [HttpGet]
        public System.Threading.Tasks.Task<JMItemInfo> GetOneJob()
        {
            return JobMonitorService.GetJobAsync(jobId);
        }
        [HttpGet]
        public string GetTest()
        {
            return "OK";
        }

        [HttpPost]
        public System.Threading.Tasks.Task<string> GetJobMessage([FromBody] JobInfo jobInfo)
        {
            if(jobInfo.JobType == JobType.FSContentDueReport || jobInfo.JobType == JobType.FSCreationAndDestructionReport)
            {
                return RMReportService.GetJobMessageForFSAsync(jobInfo.JobId);
            }
            else
            {
                return RMFileSystemSettingsService.GetJobMessageAsync(jobInfo.JobId);
            }
        }
        [HttpPost]
        public Task<string> GetRetentionUnit([FromBody] ApplyClassCodeDto dto)
        {
            return RMFileSystemSettingsService.GetRetentionUnitAsync(new ApplyClassCodeSettingDto() { TermId = dto.TermId, RetentionType = dto.RetentionType, CountryCode = dto.CountryCode });
        }

        [HttpPost]
        public Task<string> GetDisposalJobMessage([FromBody] JobInfo jobInfo)
        {
            return RMFileSystemSettingsService.GetDisposalJobMessageAsync(jobInfo.JobId);
        }
        [HttpPost]
        public Task<string> GetDisposalByClassCodeJobMessage([FromBody] JobInfo jobInfo)
        {
            return RMFileSystemSettingsService.GetDisposalByClassCodeJobMessageAsync(jobInfo.JobId);
        }
        [HttpPost]
        public Task<bool> LoadFSNodeEnableRecordManagement([FromBody] Guid nodeId)
        {
            return RMFileSystemSettingsService.LoadFSNodeEnableRecordManagement(nodeId);
        }
        [HttpPost]
        public List<Guid> ValidateEnableRecordManagementNodes([FromBody] List<Guid> nodeIds)
        {
            return RMFileSystemSettingsService.ValidateEnableRecordManagementNodes(nodeIds);
        }
        [HttpPost]
        public Task<string> GetFSRestoreJobMessage([FromBody] JobInfo jobInfo)
        {
            return RMFileSystemSettingsService.GetFSRestoreJobMessageAsync(jobInfo.JobId);
        }
        [HttpPost]
        public Task<string> GetFSRetainJobMessage([FromBody] JobInfo jobInfo)
        {
            return RMFileSystemSettingsService.GetFSRetainJobMessageAsync(jobInfo.JobId);
        }
        [HttpPost]
        public bool ResetApplyExistingOption([FromBody]string scopeId)
        {
            return RMFileSystemSettingsService.ResetApplyExistingOption(new Guid(scopeId));
        }

        [HttpPost]
        public Task<bool> UpdateJobTime([FromBody] RMFileSystemJobTimeReferenceDto dto)
        {
            return FileSystemJobTimeReferenceService.UpdateJobTimeAsync(dto.LastJobTime, dto.Path, dto.ScopeId);
        }

        [HttpPost]
        public Task<string> GetFSDiscoveryJobMessage([FromBody] JobInfo jobInfo)
        {
            return RMFileSystemSettingsService.GetFSDiscoveryJobMessageAsync(jobInfo.JobId);
        }

        [HttpPost]
        public async Task<string> GetSPJobMessage([FromBody] JobInfo jobInfo)
        {
            string message = string.Empty;
            try
            {
                using (new PerformanceScope("get sp job message"))
                {
                    string jobId = jobInfo.JobId;
                    switch (jobInfo.JobType)
                    {
                        case JobType.SharePointOnPremApplySetting:
                            message = RMSharePointOnPremSettingsService.GetApplySettingJobMessage(jobId);
                            break;
                        case JobType.SharePointOnPremEnforceRuleAction:
                            message = await RMSharePointOnPremSettingsService.GetEnforceRuleActionJobMessageAsync(jobId);
                            break;
                        case JobType.SPOnPremTermSynchronization:
                            message = await RMSharePointTaxonomyService.GetTermSyncJobMessageAsync(jobId);
                            break;
                        case JobType.SharePointOnPremDataSync:
                            message = RMSharePointOnPremSettingsService.GetDataSyncJobMessage(jobId);
                            break;
                        case JobType.SPOnPremUniqueIDSetting:
                            message = RMSharePointOnPremSettingsService.GetUniqueIdSettingJobMessage(jobId);
                            break;
                        case JobType.SPOnPremGlobalSearch:
                            message = RMSharePointOnPremSettingsService.GetGlobalSearchActionJobMessage(jobId);
                            break;
                    }
                }
                
               

            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while get sp job message:{ex.ToString()}");
            }
            return message;
        }
    }
}

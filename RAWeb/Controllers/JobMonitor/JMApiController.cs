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
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;


namespace AvePoint.RA.Web.Controllers.JobMonitor
{
    [RMApiAuthorize(RMPermissionMasks.JobMonitorEnduser, RMSOPermissionMasks.JobMonitorEnduser | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, preferred: false)]
    public class JMApiController : BaseApiController
    {
        private readonly CommonUtil.RALogger logger = CommonUtil.RALogger.GetInstance(typeof(JMApiController));
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private IJobQueueService _JobQueueService;
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService(ref _JobQueueService);
        private IGlobalSettingService _GlobalSettingService;
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService(ref _GlobalSettingService);
        private IRMJobExportSettingDao _JESDao;
        private IRMJobExportSettingDao JESDao => PlatformWindsorManager.GetService(ref _JESDao);
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        [HttpPost]
        public string QueryStopMap()
        {
            Dictionary<string, string> stopMap = new Dictionary<string, string>();
            stopMap.Add("0", "4");
            stopMap.Add("1", "2");
            stopMap.Add("2", "2");
            stopMap.Add("3", "2");
            stopMap.Add("4", "2");
            stopMap.Add("6", "1");
            stopMap.Add("7", "2");
            return JsonConvert.SerializeObject(stopMap);
        }

        [HttpGet]
        public Task<string> QueryFilterList([FromQuery]string filterValue)
        {
            return JobMonitorService.GetFilterListAsync(filterValue);
        }

        [HttpPost]
        public Task<string> QueryPager([FromBody] JMPager pager)
        {
            return JobMonitorService.GetJobsDataAsync(pager);
        }

        [HttpGet]
        [ValidJobParameterActionFilter]
        public Task<string> QueryPagerForDisposal([FromQuery]string id)
        {
            return JobMonitorService.GetJobsDataForDisposalAsync(id);
        }

        [HttpPost]
        [ValidMultipleJobParameterActionFilter]
        public async Task<RAReturnMessage> BatchDelete([FromBody] List<string> idArr)
        {
            var returnMessage = new RAReturnMessage();
            returnMessage.MessageType = await JobMonitorService.DeleteJobsAsync(idArr) == idArr.Count ? RAMessageType.Successful : RAMessageType.Failed;
            return returnMessage;
        }

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        [ValidJobParameterActionFilter]
        public async Task<string> DownloadLogFile([FromBody] List<string> jobIds)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();
            try
            {
                logger.Debug("DownloadLogFile controller");
                var message = DownloadJobReportByRunJob(jobIds);
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = message;
            }
            catch(Exception ex)
            {
                Logger.Error("Filed to Download ", ex);
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return JsonConvert.SerializeObject(returnMessage);

        }
        
        private string DownloadJobReportByRunJob(List<string> jobIdArray)
        {
            var jobType = JobType.DownloadJobReports;
            var groupId = TenantLocalValue.LogonGroupId;
            var loginName = TenantLocalValue.LogonUserEmail;
            var jobRunby = JobRunBy.Control;
            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = jobType,
                //JobRunType = jobRunBy,
                TenantGroupId = groupId,
                JobRunByUser = loginName,
                JobRunType = jobRunby,
                Parameters = jobIdArray == null ? null : SerializerHelper.SerializeByDataContractSerializer(jobIdArray),
            };
            return JobQueueService.AddToDBJobQueue(jqDto);
        }

		[HttpPost]
        [ValidMultipleJobParameterActionFilter]
        public string StopJobs([FromBody] List<string> ids)
        {
            return JobMonitorService.StopJobs(ids).ToString();
        }


        [HttpGet]
        [ValidJobParameterActionFilter]
        public string GetTermSelection([FromQuery]string id)
        {
            return JobMonitorService.GetTermSelection(id);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<string> GetJobDetails([FromBody] JMDetailsQuery queryModel)
        {
            return JobMonitorService.GetJobDetailsAsync(queryModel);
        }
        [HttpPost]
        [ValidJobParameterActionFilter]
        public async Task<JMJobSummary> GetJobSummary([FromBody]string id)
        {
            JMJobSummary summary = await JobMonitorService.GetJobSummaryAsync(id);
            if (summary == null)
            {
                throw new Exception("The job not exist");
            }
            else
            {
                return summary;
            }
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<JMJobDetails> GetSOJobSummaryDetails([FromBody] string id)
        {
            return JobMonitorService.GetSOJobSummaryDetailsAsync(id);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<JMJobDetails> GetRestoreJobSummaryDetails([FromBody] string id)
        {
            return JobMonitorService.GetRestoreJobSummaryDetailsAsync(id);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<JMJobSetting> GetJobSetting([FromBody] JMJobSettingRequest req)
        {
            return JobMonitorService.GetJobSettingAsync(req.JobId, req.JobType);
        }

        [HttpPost]
        [ValidEnforceRuleActionJobParameterFilter]
        public Task<JMJobSummary> GetDisposalJobSummary([FromBody] JMDetailsQuery query)
        {
            if (TenantService.IsNewOpusTenant())
            {
                return JobMonitorService.GetDAOJobSummaryDetailsAsync(query.JobID, query.JobType);
            }
            else
            {
                return JobMonitorService.GetDisposalJobSummaryAsync(query.JobID);
            }
        }
        //Test
        //[HttpGet]
        //public void AddData()
        //{
        //    JobMonitorService.CreateJobWithScopeId(JobType.TermSynchronization, "", "12345");

        //    var currentId = "";
        //    JobMonitorService.GetJobIdByJobTypeExceptCurrent(JobType.TermSynchronization, currentId, "12345");
        //}
        //[HttpGet]
        //public void EditData([FromQuery]string id, [FromQuery]int progress)
        //{
        //    JobMonitorService.UpdateJobStatus(id, JobStatus.Finished);
        //    //JobMonitorService.UpdateJobProgress(id, progress);
        //}

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<string> GetSubJobDetails([FromBody] JMDetailsQuery queryModel)
        {
            if (!JobServiceUtility.NewJobDetailsJobs.Contains(queryModel.JobType))
            {
                return Task.FromResult(JsonConvert.SerializeObject(new JMDetailsResult() { Success = true }));
            }
            return JobMonitorService.GetJobDetailsAsync(queryModel, true);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<JMJobDetails> GetSOSubJobSummaryDetails([FromBody] string id)
        {
            return JobMonitorService.GetSOJobSummaryDetailsAsync(id);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<string> GetSubJobDetailsById([FromBody] JMDetailsQuery queryModel)
        {
            if (!JobServiceUtility.NewJobDetailsJobs.Contains(queryModel.JobType))
            {
                return Task.FromResult(JsonConvert.SerializeObject(new JMDetailsResult() { Success = true }));
            }
            return JobMonitorService.GetJobDetailsAsync(queryModel);
        }

        [HttpPost]
        [ValidJobParameterActionFilter]
        public Task<JMDetailsResult> GetJobProgress([FromBody] JMProgressDetailsQuery queryModel)
        {
            if (!JobServiceUtility.NewJobDetailsJobs.Contains(queryModel.JobType))
            {
                return Task.FromResult(new JMDetailsResult() { Success = true });
            }
            return JobMonitorService.GetJobProgress(queryModel);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.JobMonitorAdmin, RMSOPermissionMasks.JobMonitorAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public Task<string> QueueQueryPager([FromBody] JMPager pager)
        {
            return JobQueueService.GetDBJobQueueDataAsync(pager);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.JobMonitorAdmin, RMSOPermissionMasks.JobMonitorAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public bool DeletaJobQueue([FromBody] string[] ids)
        {
            try
            {
                foreach (var id in ids)
                {
                    if (!JobQueueService.IsDBExsitJobQueue(id, TenantLocalValue.LogonGroupId))
                    {
                        return false;
                    }
                    JobQueueService.DeleteDBJobQueue(id, TenantLocalValue.LogonGroupId);
                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.JobMonitorAdmin, RMSOPermissionMasks.JobMonitorAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public bool UpdateJobQueuePriority([FromBody] JobPriorityUpdateDto dto)
        {
            try
            {
                foreach (var id in dto.JobIds)
                {
                    if (!JobQueueService.IsDBExsitJobQueue(id, TenantLocalValue.LogonGroupId))
                    {
                        return false;
                    }
                    JobQueueService.UpdateJobPriority(id, dto.JobPriority, TenantLocalValue.LogonGroupId);
                }
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.JobMonitorAdmin, RMSOPermissionMasks.JobMonitorAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public Task<bool> UpdateJobMonitorPriority([FromBody] JobPriorityUpdateDto dto)
        {
            return JobMonitorService.UpdateJobPriorityAsync(dto.JobIds, dto.JobPriority);
        }

        [HttpGet]
        [ValidJobParameterActionFilter]
        public async Task<string> GetJob(string id)
        {
            var data = await JobMonitorService.GetJobAsync(id);
            if (string.IsNullOrEmpty(data.JobId))
            {
                return string.Empty;
            }
            else
            {
                return JsonConvert.SerializeObject(data);
            }
        }

        [HttpGet]
        public async Task<string> GetJobExportSetting()
        {
            return await JobMonitorService.GetExportSettingsAsync(true);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.JobMonitorAdmin, RMSOPermissionMasks.JobMonitorAdmin | RMSOPermissionMasks.RestoreCenterSearch, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll)]
        public Task<RAReturnMessage> SaveJobExportSetting([FromBody] JobExportSettingDto setting)
        {
            return JobMonitorService.SaveExportSettingsAsync(setting);
        }

        [HttpPost]
        [ValidMultipleJobParameterActionFilter]
        public async Task<string> StartJobExport([FromBody] List<string> jobs)
        {
            RAReturnMessage message = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            var exportLocationTypes = await GlobalSettingService.GetExportLocationTypesAsync();
            var rmSettings = JESDao.GetExportSetting();
            if (rmSettings != null && exportLocationTypes.ContainsKey(rmSettings.ExportLocationId) && exportLocationTypes[rmSettings.ExportLocationId] == 1)
            {
                message.MessageType = RAMessageType.Failed;
                message.ErrorMessage = I18NEntity.GetString("RM_JS_EL_RunJob_FTPLocationNotSupported");
            }
            else
            {
                foreach (var job in jobs)
                {
                    JobMonitorService.StartExportJob(job);
                }
            }
            return JsonConvert.SerializeObject(message);
        }

        [HttpGet]
        public Task<string> GetJobDownloadSetting()
        {
            return JobMonitorService.GetExportSettingsAsync(false);
        }
    }
}

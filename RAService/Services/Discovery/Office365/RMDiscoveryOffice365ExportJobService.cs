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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using Azure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Util.MSAzure;

namespace AvePoint.RA.Service.Services.Discovery.Office365;

[AsyncAudit]
public class RMDiscoveryOffice365ExportJobService : IRMDiscoveryOffice365ExportJobService
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ExportJobService));

    private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

    private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

    private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

    private readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

    private readonly IRMDiscoveryOffice365JobDao _jobDao = new RMDiscoveryOffice365JobDao();

    private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

    private ILicenseHelperService _licenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

    private IRMSubJobDao _subJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
    private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

    private const string STORAGE_CONTAINER_NAME = "opus-import-container";

    private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.ExportO365RowData, IAsyncAfterHandler = typeof(RMDiscoveryOffice365ExportRowDataAfterAuditHandler))]
    public async Task<RAReturnMessage> ExportRowDataJobAsync()
    {
        _logger.Debug("start run export row data job");
        RAReturnMessage resultMessage = new();
        try
        {
            if (_keyValueDao.TryGetBoolValue(DiscoveryConstants.EXPORT_ROW_DATA_JOB, out var isExported) && isExported)
            {
                resultMessage.MessageType = RAMessageType.Failed;
                resultMessage.ErrorMessage = I18NEntity.GetString("RM_JS_JM_DiscoveryExportRowOnce");
                return resultMessage;
            }

            var (has, jobInfo) = await _jobDao.TryGetLatestMainJobAsync();

            if (!has || jobInfo.Version < RMDiscoveryJobVersion.V4)
            {
                resultMessage.MessageType = RAMessageType.Failed;
                resultMessage.ErrorMessage = I18NEntity.GetString("RM_JS_JM_DiscoveryExportRowJobFromVersion4");
                return resultMessage;
            }

            var dto = new JobQueueDto
            {
                JobType = JobType.DiscoveryExportRowDataJob,
                JobRunType = JobRunBy.Control,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
            };
            _jobQueueService.AddToDBJobQueue(dto);

            return resultMessage;
        }
        catch (Exception exception)
        {
            _logger.Error($"An error occured while export row data job. Error: {exception}");
            resultMessage.MessageType = RAMessageType.Failed;
            resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
            return resultMessage;
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.ExportO365RowData, IAsyncAfterHandler = typeof(RMDiscoveryOffice365ExportRowDataAfterAuditHandler))]
    public async Task<string> RealExportRowDataJobAsync(JobQueueDto jobQueueDto)
    {
        string jobId = string.Empty;
        try
        {
            JobType jobType = jobQueueDto.JobType;
            string jobRunByUser = jobQueueDto.JobRunByUser;
            var hasJobRunning = _jobMonitorService.GetRunningJobs([jobQueueDto.JobType]);
            jobId = _jobMonitorService.CreateJob(jobType, jobRunByUser);
            if (hasJobRunning.IsNullOrEmpty())
            {
                // Create job

                var account = await _accountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.DiscoveryExportRowDataJob,
                });

                _jobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    CommandLine = $"{jobType} {jobId}",
                });
            }
            else
            {
                _logger.Warn($"Discovery export row data job is running");
                _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, I18NEntity.GetString("RM_Job_ScheduledJobConflict"));
            }
        }
        catch (Exception e)
        {
            _logger.Error("real run Discovery export row data job error: {0}", e.ToString());
            _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_CreateJobError"));
        }
        return jobId;
    }

    #region Duplication report

    public async Task<RAReturnMessage> ExportDuplicationReportAsync(string o365TenantId)
    {
        _logger.Debug("Start run the export duplicate data job.");
        RAReturnMessage resultMessage = new() { MessageType = RAMessageType.Successful };
        try
        {
            var (hasJob, jobInfo) = await _jobDao.TryGetLatestMainJobAsync();
            var (isValid, errMessage) = await IsValidJobForExportDuplicationAsync(hasJob, jobInfo);
            if (!isValid)
            {
                resultMessage.MessageType = RAMessageType.Failed;
                resultMessage.ErrorMessage = I18NEntity.GetMultiStringWithSeparator(errMessage);
                return resultMessage;
            }

            _jobQueueService.AddToDBJobQueue(new JobQueueDto
            {
                JobType = JobType.DiscoveryExportDuplicationReport,
                JobRunType = JobRunBy.Control,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
                Parameters = o365TenantId,
            }); 

            return resultMessage;
        }
        catch (Exception exception)
        {
            _logger.Error($"An error occured while export duplicate data job. Error: {exception}");
            resultMessage.MessageType = RAMessageType.Failed;
            resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
            return resultMessage;
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.ExportDiscoveryDuplicationReport, IAsyncAfterHandler = typeof(RMDiscoveryOffice365ExportDuplicationReportAfterAuditHandler))]
    public async Task<string> RealRunExportDuplicationReportAsync(JobQueueDto jobQueueDto)
    {
        string jobId = string.Empty;
        try
        {
            JobType jobType = jobQueueDto.JobType;
            string jobRunByUser = jobQueueDto.JobRunByUser;
            string o365TenantId = jobQueueDto.Parameters;
            var hasJobRunning = _jobMonitorService.GetRunningJobs([jobQueueDto.JobType]);
            jobId = _jobMonitorService.CreateJob(jobType, jobRunByUser);
            if (hasJobRunning.IsNullOrEmpty())
            {
                var account = await _accountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.DiscoveryExportDuplicationReport,
                });

                _jobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    CommandLine = $"{jobType} {jobId} {o365TenantId}",
                });
            }
            else
            {
                _logger.Warn($"Discovery export duplicate data job is running");
                _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, I18NEntity.GetString("RM_Job_ScheduledJobConflict"));
            }
        }
        catch (Exception e)
        {
            _logger.Error("real run Discovery export row data job error: {0}", e.ToString());
            _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, I18NEntity.GetString("RM_SP_CreateJobError"));
        }
        return jobId;
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.DiscoveryCleanUpDuplicateDatas, IAsyncAfterHandler = typeof(RMDiscoveryOffice365ExportDuplicationReportAfterAuditHandler))]
    public async Task<string> RealRunCleanDuplicateDatasJob(JobRunBy jobRunBy, string jobRunByUser, string param)
    {
        bool hasSoLicense = _licenseHelperService.HasOpusSOLicense;
        if (!hasSoLicense)
        {
            _logger.Error("this user has no so license,cannot run job");
            return "HasNoSoLicense";
        }
        string jobId = string.Empty;
        JobType jobType = JobType.CleanUpDuplicateDatas;
        //RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
        var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
        var conflictJobTypes = new List<JobType>() { JobType.CleanUpDuplicateDatas };
        List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
        var mIndexJobs = _jobMonitorService.GetRunningJobs(indexJobTypes);
        if (mIndexJobs.Count > 0)
        {
            _logger.Warn("so RealRunCleanDuplicateDatasJob Current has move index or retention job running.");
            jobId = _jobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, "", "");
            _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
            return jobId;
        }
        var mconflictJobs = _jobMonitorService.GetRunningJobs(conflictJobTypes);
        if (mconflictJobs.Count > 0)
        {
            _logger.Warn("so RealRunCleanDuplicateDatasJob Current has same type job running.");
            jobId = _jobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, "", "");
            _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
            return jobId;
        }
        jobId = _jobMonitorService.CreateJobWithScopeId(jobType, jobRunByUser, "", "");
        _subJobDao.UpdateSubJobCount(jobId, 1);
        int currentSubjobIndex = 0;
        List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
        if (!IsTrailLicenceAndExceedSizeLimit())
        {
            string subJobId = CreateSubJobForCleanUpDuplicateDatas(jobId, currentSubjobIndex, jobType, param);
        }
        else
        {
            _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_TrailSizeLimit");
        }
        return jobId;
    }

    private string CreateSubJobForCleanUpDuplicateDatas(string jobId, int currentSubjobIndex, JobType jobType, string setting)
    {
        string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
        var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / 1 };
        subJob.Runable = RecordsConstants.SubJob_Runnable_Waiting;
        subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = setting };
        //subJob.String1 = scope;
        _subJobDao.CreateJob(subJob);
        _logger.Info("CreateSubJobForCleanUpDuplicateDatas Create sub job {0} sucessfull, type {1}, weight {2}", subJob.Id, subJob.JobType, subJob.Weight);
        return subJobId;
    }

    private bool IsTrailLicenceAndExceedSizeLimit()
    {
        try
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
            if (info.Type == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
            {
                _logger.Info("this is Trial licence");
                var size = StorageDeviceService.GetArchiverStorageGBSize();
                var resultSize = size;
                if (resultSize >= 5)
                {
                    _logger.Info($"current trial licence user has run out of size {resultSize}gb is bigger than 5gb");
                    //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.Error($"some thing went wrong when check Trail Licence And Exceed Size,error{e.ToString()}");
            return false;
        }
    }

    public async Task UploadDuplicationReportToBlobAsync(string filePath)
    {
        try
        {
            var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.DiscoveryDuplicationReportZip);

            var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();

            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            await containerClient.UploadBlobAsync(blobName, fileStream);
        }
        catch (Exception e)
        {
            _logger.Error("Upload duplication report to blob error: {0}", e.ToString());
            throw;
        }
    }

    public async Task<string> DownloadDuplicationReportAsync()
    {
        try
        {
            var folderName = Path.Combine("DiscoveryDuplicationReport", $"Report_{DateTime.UtcNow.Ticks.ToString()}");

            var folderPath = SecurityUtils.SafeCombinePath(Path.GetTempPath(), "AvePoint", folderName);

            var zipFilePath = SecurityUtils.SafeCombinePath(folderPath, JobReportUtility.DiscoveryDuplicationReportZip);

            PrepareDirectories(folderPath, zipFilePath);

            var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);

            await containerClient.CreateIfNotExistsAsync();

            var blobName = SecurityUtils.SafeCombinePath(JobReportUtility.GetTenantIdentity(), JobReportUtility.DiscoveryDuplicationReportZip);

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DownloadToAsync(zipFilePath);

            ZipUtil.UnZipFile(zipFilePath, folderPath);

            return folderPath;
        }
        catch (RequestFailedException ae) when (ae.Status == 404)
        {
            _logger.Warn("The duplication report blob does not exist.");
            throw;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private async Task<(bool isValid, string errorMessage)> IsValidJobForExportDuplicationAsync(bool hasJob, RMDiscoveryOffice365MainJob jobInfo)
    {
        RMDiscoveryOffice365RuleInfoDao ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
        HashSet<RMDiscoveryJobStatus> ProcessingJobStatuses = new()
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };

        if (!await ruleInfoDao.CheckExistingRuleByAnalyzeMethodsAsync(true, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument))
        {
            _logger.Warn("No discovery duplication rule found, please configure that kind of rule before processing.");
            return (false, "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_NoDuplicationRule");
        }

        if (!await RMDiscoveryOffice365LicenseHelper.IsAllowedToExportDuplicationDataAsync())
        {
            _logger.Warn("The current license does not allow exporting duplication report.");
            return (false, "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_NoLicense");
        }

        if (!hasJob || jobInfo == null)
        {
            _logger.Warn("No discovery job found for exporting duplication report.");
            return (false, "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_NoDiscoveryJob");
        }

        if (ProcessingJobStatuses.Contains(jobInfo.Status))
        {
            _logger.Warn($"The discovery job is in processing status: {jobInfo.Status}, duplication report cannot be exported.");
            var key = "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_IsRunningJobDiscovey";
            var message = $"{key}{I18NEntity.Separator}{jobInfo.Status}";
            return (false, message);
        }

        if(jobInfo.Version is not RMDiscoveryJobVersion.V3 and not RMDiscoveryJobVersion.V4 and not RMDiscoveryJobVersion.V5)
        {
            _logger.Warn($"The discovery job version is {jobInfo.Version}, duplication report cannot be exported.");
            return (false, "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_VersionDiscoveryJob");
        }

        return (true, string.Empty);
    }

    private void PrepareDirectories(string folderPath, string zipFilePath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        if (!File.Exists(zipFilePath))
        {
            File.Create(zipFilePath).Dispose();
        }
        else
        {
            File.Delete(zipFilePath);
            File.Create(zipFilePath).Dispose();
        }
    }

    #endregion
}
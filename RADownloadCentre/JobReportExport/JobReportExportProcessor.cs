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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using System.Management.Automation;
using System.Net;
using System.Text;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using BaseJobDto = AvePoint.RA.Contract.JobMonitor.BaseJobDto;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;

namespace RADownloadCenter.JobReportExport
{
    public class JobReportExportProcessor : GenerateAndUploadFileExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(JobReportExportProcessor));

        private static readonly IJobMonitorService JobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private static readonly IJobMonitorDetailDownloadWorker JobMonitorDetailDownloadWorker = PlatformWindsorManager.GetService<IJobMonitorDetailDownloadWorker>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static readonly ITenantInfoDao TenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();
        private readonly List<string> ExportJobIds;

        private FileTransferStream FileStream;

        private readonly string JobId;

        private readonly bool _isDownloadJobReports;

        public JobReportExportProcessor(string jobId, string param, bool forceNeedSasUri = false, bool isDownloadJobReports = false)
        {
            GenerateAndUploadFileManager.Init(jobId, JobType.DownloadJobReports);
            JobId = jobId;
            DownloadCenterUtility.ForceNeedSasUri |= forceNeedSasUri;
            ExportJobIds = AvePoint.RA.Common.Global.Utils.SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param);

            _isDownloadJobReports = isDownloadJobReports;
        }
        public override async Task RunAsync()
        {
            var reportProfile = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait })
                                   .FirstOrDefault(item => item.JobId == BaseJobId);
            try
            {
                if(DownloadCenterUtility.ForceNeedSasUri)
                {
                    Logger.Info($"ForceNeedSasUri flag set for job {JobId}, will always generate SAS URI.");
                }
                if (reportProfile == null)
                {
                    GenerateAndUploadFileManager.HasFailed = true;
                    Logger.Error($"Can not find report download info!");
                    return;
                }

                reportProfile.JobStatus = (int)DownloadContentJobStatus.InProgress;

                await DownloadDataInfoDao.UpdateAsync(reportProfile);

                await GenerateDataAsync();

                
                Logger.Info("Generate Data success!");

                await UploadBlobAsync();
                if (fileInfo != null)
                {
                    reportProfile.FileSize = fileInfo.Length;
                }

                reportProfile.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                Logger.Info("Upload blob success!");
              

                reportProfile.JobStatus = (int)DownloadContentJobStatus.Finished;

                DownloadDataInfoDao.UpdateDownloadInfo(reportProfile);
            }
            catch (Exception e)
            {
                reportProfile!.JobStatus = (int)DownloadContentJobStatus.Failed;
                await DownloadDataInfoDao.UpdateAsync(reportProfile);
                GenerateAndUploadFileManager.HasFailed = true;
                GenerateAndUploadFileManager.JobComment = e.Message;
                Logger.Error($"Generate And Upload File failed! Error : {e}");
            }
            finally
            {
                FileStream.Close();
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
            }
        }

        protected override string BaseJobId => JobId;

        protected override ArchiverExportReportDto ExportReportDto => throw new NotImplementedException();

        protected override async Task GenerateDataAsync()
        {

            List<JMItemInfo> infos;
            List<BaseJobDto> jobDtos = new List<BaseJobDto>();
            if (ExportJobIds.Any(a => a.Contains("_")))
            {
                Logger.Info("can not find these jobs in job monitor,it may be subjob,it is delete orphan job");
                infos = await GenerateJMItemInfosAsync();
            }
            else
            {
                infos = await JobMonitorService.GetJobsAsync(ExportJobIds);
            }
            foreach (var info in infos)
            {
                JMDownloadJobReport? detail = null;
                try
                {
                    var jobDto = new BaseJobDto();
                    jobDto.Id = info.JobId;
                    jobDto.SiteCollectionUrl = info.SiteUrl;
                    jobDto.JobType = info.JobTypeCode == (int)JobType.SharePointCustomSetting || info.JobTypeCode == (int)JobType.SharePointInheritSetting ? (int)JobType.SharePointGlobalSetting : info.JobTypeCode == (int)JobType.MailBoxBackup? (int)JobType.TeamsArchiverBackup : info.JobTypeCode;
                    jobDto.SubJobCount = info.SubJobCount;
                    jobDto.JobVersion = info.JobVersion;
                    if (jobDto.JobType == (int)JobType.EXOEnforceRetention || jobDto.JobType == (int)JobType.OneDriveEnforceRetention || jobDto.JobType == (int)JobType.TeamsEnforceRetention)
                    {
                        jobDto.JobType = (int)JobType.EnforceRetention;
                    }
                    else if (!string.IsNullOrEmpty(info.AdditionalInformation)
                        && (jobDto.JobType == (int)JobType.MigrationArchiverRestore 
                            || jobDto.JobType == (int)JobType.MigrationArchiverRetention 
                            || jobDto.JobType == (int)JobType.MigrationArchiverFileLevelRetention
                            || (jobDto.JobType == (int)JobType.ArchiverDeduplication && jobDto.Id.StartsWith("DD"))))
                    {
                        ArchiverMigratedJobExtension jobExtension = new();
                        try
                        {
                            jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(info.AdditionalInformation);
                            
                        }
                        catch (Exception e)
                        {
                            Logger.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");        
                        }
                        if (jobExtension != null)
                        {
                            jobDto.PlanId = jobExtension.PlanId;
                            jobDto.Category = jobExtension.JobCategory;
                        }
                        jobDto.TenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
                    }
                    jobDtos.Add(jobDto);
                    detail = new JMDownloadJobReport()
                    {
                        JobId = info.JobId,
                        Status = JobDetailsStatus.Successful,
                        Comment = info.Comment,
                    };
                    GenerateAndUploadFileManager.AddSucceedJobDetail(detail);
                }
                catch(Exception ex)
                {
                    detail = new JMDownloadJobReport()
                    {
                        JobId = info.JobId,
                        Status = JobDetailsStatus.Failed,
                        Comment = info.Comment,
                    };
                    GenerateAndUploadFileManager.AddFailedJobDetail(detail);
                    Logger.Warn($"Failed to build download report detail for job {info.JobId}: {ex}");
                }
                
            }
            FileStream = await JobMonitorDetailDownloadWorker.GenerateDetailReportAsync(jobDtos, _isDownloadJobReports);

            if (FileStream == null)
            {
                throw new Exception("An error accured while create report file stream");
            }
        }

        private async Task<List<JMItemInfo>> GenerateJMItemInfosAsync()
        {
            var subjobInfos = SubJobDao.GetSubJobsByIds(ExportJobIds);
            List<JMItemInfo> JSJobInfos = new List<JMItemInfo>();
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var dbJob in subjobInfos)
            {
                var JSJobInfo = new JMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobTypeCode = dbJob.JobType,
                    Status = (JobStatus)dbJob.Status,
                    StartTime = dbJob.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.StartTime, true).SimplifyFormatTime,
                    EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.EndTime, true).SimplifyFormatTime,
                    SiteUrl = dbJob.String1,
                };
                JSJobInfos.Add(JSJobInfo);
            }
            return JSJobInfos;
        }
        protected override async Task UploadBlobAsync()
        {
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FileStream);
                    Logger.Info($"Upload Job Report Export success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload Job Report Export failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FileStream.Name);
        }
    }
}

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
using System.Text;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using RADownloadCenter;
using System.Text.Json;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using AvePoint.RA.I18N.Core;

namespace RADownloadCentre.SettingExport.Base
{
    public abstract class ExportSettingProcessor<T> where T : class
    {
        protected readonly ITermDao _termDAO = PlatformWindsorManager.GetService<ITermDao>();
        protected readonly IRMWorkflowDefinitionDao RMWorkflowDefinitionDAO = PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();
        protected readonly IRecordOwnerDao RecordOwnerDao = PlatformWindsorManager.GetService<IRecordOwnerDao>();

        protected const Char PathSeparator = '|';
        private readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        // Download center
        private readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        private readonly RALogger Logger = RALogger.GetInstance(typeof(ExportSettingProcessor<T>));
        protected readonly string JobId;
        protected readonly BaseJobDto BaseJobDto;
        protected readonly string FolderPath;
        protected string FilePath;
        protected FileInfo? fileInfo;
        protected string BaseJobId => JobId;
        protected readonly int CountOfOneSheet = 65535;
        protected ExportSettingType exportSettingType = ExportSettingType.OnlyExportCustomSettingNodes;
        #region Column and value in excel file
        protected const string ManuallyChooseATerm = "Manually choose a term";
        protected const string SetADefaultTerm = "Set a default term";
        protected const string AutoPopulate = "Auto populate a term based on criteria (Doesn't support import)";
        protected const string SmartClassification = "Smart classification (Doesn't support import)";
        protected const string NoManualSetting = "No manual setting";
        protected const string WorkflowProcess = "Workflow process";
        protected const string RecordOwner = "Record owner";
        protected const string AutoApprove = "Skip manual review for this location";
        protected string ApplyTermByColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyTermByColumn");
        protected string TermScopeColumn = I18NEntity.GetString("RM_JS_BCM_Export_TermScopeColumn");
        protected string DefaultTermColumn = I18NEntity.GetString("RM_JS_BCM_Export_DefaultTermColumn");
        protected string ApplyToExistingDocumentsColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToExistingDocumentsColumn");
        protected string ApplyToExistingDeclaredRecordsColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToExistingDeclaredRecordsColumn");
        protected string ApplyToDocumentSetsAndFoldersColumn = I18NEntity.GetString("RM_JS_BCM_Export_ApplyToDocumentSetsAndFoldersColumn");
        protected string SendEmailForPersonColumn = I18NEntity.GetString("RM_JS_BCM_Export_SendEmailForPersonColumn");
        protected string SendEmailNotificationColumn = I18NEntity.GetString("RM_JS_BCM_Export_SendEmailNotificationColumn");
        protected string OverwriteTheExistingTermColumn = I18NEntity.GetString("RM_JS_BCM_Export_OverwriteTheExistingTermColumn");
        protected string IsInheritSetting = I18NEntity.GetString("RM_JS_BCM_Export_IsInheritSettingColumn");
        #endregion

        public ExportSettingProcessor(string jobId, JobType jobType)
        {
            JobId = jobId;
            GenerateAndUploadFileManager.Init(jobId, jobType);
            BaseJobDto = new BaseJobDto()
            {
                Id = jobId,
                JobType = (int)jobType
            };
            FolderPath = JobReportUtility.GetDownloadReportDetailTempleFolder(BaseJobDto);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }
        public virtual async Task RunAsync()
        {
            await GetListGroupHasSetting();
            var reportProfile = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait })
                                   .FirstOrDefault(item => item.JobId == BaseJobId);
            try
            {
                using CheckJobStopScope jScope = new();
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
                GenerateAndUploadFileManager.SendJobDetail();
                GenerateAndUploadFileManager.SetJobFinished();
                if (GenerateAndUploadFileManager.HasFailed && !GenerateAndUploadFileManager.HasSucceed)
                {
                    if (reportProfile != null)
                    {
                        DownloadDataInfoDao.BatchDelete(new List<RMDownloadDataInfo> { reportProfile });
                    }
                }
            }
        }

        protected async virtual Task UploadBlobAsync()
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");//Path.Combine(customId, JobId + ".zip");
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                    Logger.Info($"Upload Export Setting success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Upload Export Setting failed,error is :{e}");
                throw;
            }

            Logger.Info($"finish to upload blob name:{blobName}");
            fileInfo = new FileInfo(FolderPath + ".zip");
        }

        protected virtual async Task<string[][]> GenerateSettingsAsync(int settingJobType, string[][] datas, List<T> settings, bool isCreateHeader, string name = "")
        {
            if (settings == null)
            {
                Logger.Warn($"Settings is null");
                return datas;
            }
            try
            {
                if (isCreateHeader)
                {
                    datas = AssembleSettingHeaderTittle(datas, name);
                }
                return await ConvertSettingToArrayAsync(settings, datas);
            }
            catch (Exception e)
            {
                Logger.Error($"Generate settings for export job failed {e}");
                throw;
            }
        }

        protected virtual void GenerateJobDetail(string ObjectName, string Url, string comment = "", bool isSuccess = true)
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                ObjectName = ObjectName,
                Url = Url,
                Status = isSuccess ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                Comment = comment,
            };
            if (isSuccess)
            {
                GenerateAndUploadFileManager.AddSucceedJobDetail(detail);
                return;
            }
            GenerateAndUploadFileManager.AddFailedJobDetail(detail);
        }

        protected virtual void GenerateJobDetailWithStatus(string objectName, string url, JobDetailsStatus status = JobDetailsStatus.Successful, string comment = "")
        {
            JMImportSPSettingDetail detail = new JMImportSPSettingDetail()
            {
                ObjectName = objectName,
                Url = url,
                Status = status,
                Comment = comment,
            };
            if (status == JobDetailsStatus.Failed)
            {
                GenerateAndUploadFileManager.AddFailedJobDetail(detail);
                return;
            }
            GenerateAndUploadFileManager.AddSucceedJobDetail(detail);
        }

        protected T? Clone(T? setting)
        {
            if (setting == null) return default;
            string temp = JsonSerializer.Serialize<T>(setting);
            return JsonSerializer.Deserialize<T>(temp);
        }

        protected abstract Task GenerateDataAsync();

        protected abstract Task GetListGroupHasSetting();
        protected abstract Task<string[][]> ConvertSettingToArrayAsync(List<T> settings, string[][] datas);
        protected abstract string[][] AssembleSettingHeaderTittle(string[][] datas, string connectionName);
    }

    public enum ApplyExistingTermType
    {
        None = 0,
        OverWrite = 1,
        SkipAndKeep = 2
    }
}

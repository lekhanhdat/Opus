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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.TermManagement;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using RAExportCommon;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

namespace RAManualApproval.ExportAction.ExportTermAndRule
{
    public class ExportTermAndRuleExportProcessor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ExportTermAndRuleExportProcessor));

        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();

        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private string FullPath { get; set; }

        private string FolderPath { get; set; }

        private string JobId { get; set; }

        public ExportTermAndRuleExportProcessor()
        {
    
        }

        public async Task RunAsync(string jobId)
        {
            JobId = jobId;

            ExportTermAndRuleExportJobManager.Init(JobId, AvePoint.RA.Contract.JobMonitor.JobType.ExportTermStructure);
    
            var downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).First();
            try
            {
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                FullPath = await GetFileName();

                FolderPath = JobReportUtility.GetDownloadTermInfoReportTempleFolder("Temple") + Path.DirectorySeparatorChar + FullPath + Guid.NewGuid();

                await TaxonomyService.GenerateReportForTermInfoAsync(FolderPath, FullPath, I18NEntity.GetString("RM_RC_RUR_TermDetail"));

                var fileInfo = await UploadBlobAsync();

                if (fileInfo != null)
                {
                    downloadDataInfo.FileSize = fileInfo.Length;
                }

                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                ExportTermAndRuleExportJobManager.HasSucceedDetail = true;

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);
            }
            catch (Exception e)
            {
                ExportTermAndRuleExportJobManager.HasFailedDetail = true;
                ExportTermAndRuleExportJobManager.JobComment = e.Message;
                Logger.Error($"Export  for term failed ,{e}");
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
            finally
            {
                ExportTermAndRuleExportJobManager.SetJobFinished();
                PerformanceMonitor.WritePerformanceResult();
            }
        }

        private async Task<string> GetFileName()
        {
            DateTime nowTime = DateTime.UtcNow;
            var nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            return  I18NEntity.GetString("RM_RC_Audit_Action_ExportTerm") + "_" + nowTimeStr;
        }

        private async Task<FileInfo> UploadBlobAsync()
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                AvePoint.GCommon.ZipUtil.ZipFolder(FolderPath, FolderPath + ".zip", Encoding.UTF8);
                var customId = TenantLocalValue.LogonGroupId;
                var blobName = SecurityUtils.SafeCombinePath(customId, JobId + ".zip");
                try
                {
                    await Retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, FolderPath + ".zip");
                        Logger.Info($"Upload term rule export success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    Logger.Error($"Upload term rule export failed,error is :{e}");
                    throw;
                }

                Logger.Info($"finish to upload blob name:{blobName}");
                return new FileInfo(FolderPath + ".zip");
            }
        }


        private static void UpdateDownloadDataInfo(RMDownloadDataInfo DownCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}")) ;
            {
                DownCenterInfo.JobStatus = (int)downloadStatus;
                var success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                if (success)
                {
                    Logger.Info($"Update download file status to {downloadStatus} finished.");
                }
                else
                {
                    Logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                    success = DownloadDataInfoDao.UpdateDownloadInfo(DownCenterInfo);
                    var status = success ? "finished" : "failed";
                    Logger.Info($"Update retry download file {status}.");
                }
            }
        }
    }
}

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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.Service.Services.Discovery.Office365.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export;
using Cloud.Sdk.IE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work;

public class RMDiscoveryOffice365ExportRowDataJobRunner
{
    private readonly IRALogger _logger = new RALogger(typeof(RMDiscoveryOffice365ExportRowDataJobRunner));
    
    private readonly IRMReportManager _reportManager;

    private readonly string _jobId;
        
    private readonly IDownloadDataInfoDao _downloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

    private string _folderPath;
    
    private string _fullPath;
    
    private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();
    
    private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

    private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

    public RMDiscoveryOffice365ExportRowDataJobRunner(string jobId)
    {
        _jobId = jobId;
        ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryExportRowDataJob);
        _reportManager = ReportMangerFactory.Instance.ReportManager;
    }
    
    public async Task RunAsync()
    {
        var downloadDataInfo = _downloadDataInfoDao.GetDownloadDataInfosByStatus(
            [(int)DownloadContentJobStatus.Wait]).First(item => item.JobId == _jobId);
        try
        {
            _reportManager.StartUpdateJobProgress();
            
            PerformanceTimer timer = new("DiscoveryExportData");
            
            UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);
            
            _fullPath = await GetFileName() + Guid.NewGuid();

            _folderPath = JobReportUtility.GetDownloadExportRowDataInfoReportTempleFolder("Temple") + Path.DirectorySeparatorChar + _fullPath;
            
            timer.Start();

            RMDiscoveryOffice365ExportRowDataProcessor processor = new(_folderPath, _reportManager);

            await processor.ProcessAsync();
            
            timer.Stop();

            var runningTime = (long)timer.GetTimerSecond();

            var fileInfo = await UploadBlobAsync();

            downloadDataInfo.FileSize = fileInfo.Length;

            downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

            UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);
            
            await _keyValueDao.UpsertAsync(DiscoveryConstants.EXPORT_ROW_DATA_JOB, "True");
            
            TelemetryContext.SendToQueue(TelemetryModule.DiscoveryExportRowData, TelemetryEventType.ExportCsvFile, [_fullPath, fileInfo.Length, processor.FileCount, runningTime]);

            _reportManager.SetJobFinished(JobStatus.Finished);

            await TelemetryContext.FlushAsync();
        }
        catch (Exception exception)
        {
            _logger.Error($"An error occurred while run job. Error: {exception}");
            _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
            UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
        }
    }
    
    private async Task<string> GetFileName()
    {
        DateTime nowTime = DateTime.UtcNow;
        var nowTimeStr = (await _generalSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
        return  I18NEntity.GetString("ExportRowDataJob") + "_" + nowTimeStr;
    }

    private async Task<FileInfo> UploadBlobAsync()
    {
        using (new PerformanceScope("Upload blob to azure storage", "", true))
        {
            GCommon.ZipUtil.ZipFolder(_folderPath, _folderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = Path.Combine(customId, _jobId + ".zip");
            try
            {
                await _retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, _folderPath + ".zip");
                    _logger.Info($"Upload report profile details success");
                    return Task.CompletedTask;
                });
            }
            catch (Exception e)
            {
                _logger.Error($"Upload report profile details failed,error is :{e}");
                throw;
            }

            _logger.Info($"finish to upload blob name:{blobName}");
            return new FileInfo(_folderPath + ".zip");
        }
    }

    private void UpdateDownloadDataInfo(RMDownloadDataInfo downloadDataInfo,DownloadContentJobStatus downloadStatus)
    {
        using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}"))
        {
            downloadDataInfo.JobStatus = (int)downloadStatus;
            var success = _downloadDataInfoDao.UpdateDownloadInfo(downloadDataInfo);
            if (success)
            {
                _logger.Info($"Update download file status to {downloadStatus} finished.");
            }
            else
            {
                _logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                success = _downloadDataInfoDao.UpdateDownloadInfo(downloadDataInfo);
                var status = success ? "finished" : "failed";
                _logger.Info($"Update retry download file {status}.");
            }
            if (!success)
            {
                throw new Exception("Updated download data info failed");
            }
        }
    }
}
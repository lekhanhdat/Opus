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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Export;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work
{
    public class RMDiscoveryOffice365ExportDuplicationReportRunner
    {
        private readonly IRALogger _logger = new RALogger(typeof(RMDiscoveryOffice365ExportDuplicationReportRunner));

        private readonly IRMReportManager _reportManager;

        private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();

        private readonly IDownloadDataInfoDao _downloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private readonly IRMDiscoveryOffice365JobDao _discoveryJobDao = new RMDiscoveryOffice365JobDao();

        private readonly string _jobId;

        private readonly string _o365TenantId;

        private string _folderPath;

        public RMDiscoveryOffice365ExportDuplicationReportRunner(string jobId, string o365TenantId)
        {
            _jobId = jobId;
            _o365TenantId = o365TenantId;
            ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryExportDuplicationReport);
            _reportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public async Task RunAsync()
        {
            RMDownloadDataInfo downloadDataInfo = null;
            try
            {
                downloadDataInfo = _downloadDataInfoDao
                    .GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.Wait])
                    .FirstOrDefault(item => item.JobId == _jobId);

                var (hasMainJob, mainJob) = await _discoveryJobDao.TryGetLatestMainJobAsync();

                if (downloadDataInfo == null)
                    throw new InvalidOperationException("Download data info not found.");

                if (!hasMainJob)
                    throw new InvalidOperationException("Main job not found.");

                _logger.Info("Start to export duplication report runner for job id: {0}", _jobId);

                _reportManager.StartUpdateJobProgress();
                
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                _folderPath = JobReportUtility.GetDownloadDiscoveryExportDuplicationReportTempleFolder("Temple") + Path.DirectorySeparatorChar + _jobId.Replace("-", "");

                var processor = new RMDiscoveryOffice365ExportDuplicationReportProcessor(mainJob, _reportManager, _folderPath, new Guid(_o365TenantId));

                await processor.Initialize().ExecuteAsync();

                downloadDataInfo.FileSize = await UploadBlobAsync();
                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                _reportManager.SetJobFinished(JobStatus.Finished);
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);

                _logger.Info("Finish to export duplication report runner for job id: {0}", _jobId);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while running job.", ex);
                _reportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
                if (downloadDataInfo != null)
                    UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
            }
        }

        private async Task<long> UploadBlobAsync()
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                var zipPath = _folderPath + ".zip";
                try
                {
                    string blobName = null;
                    long fileLength = 0;
                    GCommon.ZipUtil.ZipFolder(_folderPath, zipPath, Encoding.UTF8);

                    var customId = TenantLocalValue.LogonGroupId;
                    blobName = Path.Combine(customId, _jobId + ".zip");

                    await _retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, zipPath);
                        _logger.Info("Upload report profile details success");
                        return Task.CompletedTask;
                    });

                    _logger.Info($"Finish to upload blob name: {blobName}");

                    TryGetFileLength(zipPath, out fileLength);
                    return fileLength;
                }
                catch (Exception e)
                {
                    _logger.Error($"Upload report profile details failed, error is: {e}");
                    throw;
                }
                finally
                {
                    DropFilesOrDirectories(zipPath, _folderPath);
                }
            }
        }

        private void UpdateDownloadDataInfo(RMDownloadDataInfo downloadDataInfo, DownloadContentJobStatus downloadStatus)
        {
            const int maxRetry = 3;
            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    downloadDataInfo.JobStatus = (int)downloadStatus;

                    var success = _downloadDataInfoDao.UpdateDownloadInfo(downloadDataInfo);

                    _logger.Info("Update download file status to {0} attempt {1}/{2}: {3}.", downloadStatus, attempt, maxRetry, success ? "successful" : "failure");

                    if (success) return;
                }
                catch (Exception ex)
                {
                    _logger.Warn("Update download file status to {0} attempt {1}/{2} failed. Error: {3}", downloadStatus, attempt, maxRetry, ex.Message);
                }
                Thread.Sleep(1000);
            }
            _logger.Error("Update download file status to {0} failed after {1} retries.", downloadStatus, maxRetry);
        }

        private bool TryGetFileLength(string path, out long length)
        {
            length = 0;

            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    return false;

                length = fileInfo.Length;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to get file length: {path}, error: {ex}");
                return false;
            }
        }

        private void DropFilesOrDirectories(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to delete path: {path}, error: {ex}");
                }
            }
        }
    }
}

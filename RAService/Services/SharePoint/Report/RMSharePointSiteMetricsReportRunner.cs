using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
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
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SQLiteDB.Reporting.DBManager;
using AvePoint.RA.RACommonUtility.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SharePoint.Report
{
    public class RMSharePointSiteMetricsReportRunner
    {
        private readonly IRALogger _logger = new RALogger(typeof(RMSharePointSiteMetricsReportRunner));

        private readonly IRMReportManager ReportManager;

        private readonly RMRetryer _retryer = RMRetryerBuilder.CreateBuilder().Build();

        private readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private readonly string _jobId;

        private readonly List<string> _targetSharePointSiteUrls;

        private readonly string _targetSharePointLibraryUrl;

        private string _exportFolderPath;

        private string _dbPath;

        public RMSharePointSiteMetricsReportRunner(string jobId, List<string> targetSharePointSiteUrls, string targetSharePointLibraryUrl)
        {
            _jobId = jobId;
            _targetSharePointSiteUrls = targetSharePointSiteUrls ?? new List<string>();
            _targetSharePointLibraryUrl = targetSharePointLibraryUrl;
            _exportFolderPath = JobReportUtility.GetDownloadSharePointReportSiteExportTempleFolder("Temple") + Path.DirectorySeparatorChar + _jobId.Replace("-", "");
            ReportMangerFactory.Instance.Init(jobId, JobType.SharePointSiteMetricsReport);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
        }

        public async Task RunAsync()
        {
            RMDownloadDataInfo downloadDataInfo = null;
            try
            {
                downloadDataInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int> { (int)DownloadContentJobStatus.Wait })
                    .FirstOrDefault(item => item.JobId == _jobId);

                if (downloadDataInfo == null)
                {
                    throw new InvalidOperationException("Download data info not found.");
                }

                var targets = ResolveTargets(_targetSharePointSiteUrls);
                if (targets.Count == 0)
                {
                    throw new InvalidOperationException("No valid site collection can be exported.");
                }

                _logger.Info("Start SharePoint report export runner for job id: {0}, site count: {1}", _jobId, targets.Count);

                ReportManager.StartUpdateJobProgress();

                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.InProgress);

                _dbPath = SharePointReportSQLiteDBManager.CreateDatabase(_jobId.Replace("-", "").ToLowerInvariant(), _exportFolderPath);

                var processor = new RMSharePointSiteMetricsReportProcessor(ReportManager, _dbPath, new RMSharePointSiteMetricsReportProcessor.SiteMetricsExportPayload()
                {
                    SiteExportTargets = targets,
                    DestinationLibUrl = _targetSharePointLibraryUrl
                });

                await processor.ExecuteAsync();

                downloadDataInfo.FileSize = await UploadBlobAsync();
                downloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                ReportManager.SetJobFinished(JobStatus.Finished);
                UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Finished);

                _logger.Info("Finish SharePoint report export runner for job id: {0}", _jobId);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while running job.", ex);

                if (ex is InvalidOperationException)
                {
                    ReportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_UploadReportFileToLibraryError");
                }
                else
                {
                    ReportManager.SetJobFinished(JobStatus.Failed, "RM_HS_Criteria_View_Msg_ValidOtherError");
                }

                if (downloadDataInfo != null)
                {
                    UpdateDownloadDataInfo(downloadDataInfo, DownloadContentJobStatus.Failed);
                }
            }
            finally
            {
                DropFilesOrDirectories(_dbPath, _exportFolderPath);
            }
        }

        private List<RMSharePointSiteMetricsReportProcessor.SiteExportTarget> ResolveTargets(List<string> siteCollectionUrls)
        {
            var targets = new List<RMSharePointSiteMetricsReportProcessor.SiteExportTarget>();

            foreach (var url in siteCollectionUrls)
            {
                RemoteSiteCollection siteCollection = RemoteNodeDao.GetRemoteSiteCollectionByUrl(url);
                if (siteCollection == null)
                {
                    _logger.Warn("Remote site collection not found for url: {0}", url);
                    continue;
                }

                targets.Add(new RMSharePointSiteMetricsReportProcessor.SiteExportTarget
                {
                    SiteCollectionId = siteCollection.id,
                    SiteCollectionUrl = url
                });
            }

            return targets;
        }

        private async Task<long> UploadBlobAsync()
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                var zipPath = _exportFolderPath + ".zip";
                try
                {
                    string blobName = null;
                    long fileLength = 0;
                    ZipUtil.ZipFolder(_exportFolderPath, zipPath, Encoding.UTF8);
                    var customId = TenantLocalValue.LogonGroupId;
                    blobName = SecurityUtils.SafeCombinePath(customId, _jobId + ".zip");

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
                    DropFilesOrDirectories(zipPath);
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

                    var success = DownloadDataInfoDao.UpdateDownloadInfo(downloadDataInfo);

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
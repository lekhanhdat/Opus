using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using FluentFTP.Helpers;
using PnP.Framework.Modernization.Pages;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace AvePoint.RA.DB.Core.Util
{
    public class JobReportShardHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(JobReportShardHelper));

        private readonly long _maxRowsPerShardFile;

        private readonly ConcurrentDictionary<string, (string ShardPath, long RowCount)> _cache = new();
        private readonly object _lock = new();

        public JobReportShardHelper(long maxRowsPerShardFile = 1_500_000)
        {
            _maxRowsPerShardFile = maxRowsPerShardFile;
        }

        /// <summary>
        /// Returns the active shard path for <paramref name="basePath"/>, creating the
        /// SQLite table via <paramref name="createTable"/> when a new shard is needed.
        /// Increments the in-memory counter by <paramref name="insertCount"/> after a successful insert.
        /// </summary>
        public string GetOrCreateShardFile(BaseJobDto jobInfo, string basePath, string tableName, int insertCount, Action<string> createTable)
        {
            lock (_lock)
            {
                if (!_cache.TryGetValue(basePath, out var cached))
                {
                    cached = LoadFromDisk(basePath, tableName, createTable);
                    _cache[basePath] = cached;
                }

                if (cached.RowCount >= _maxRowsPerShardFile)
                {
                    string baseBlobUri = JobReportUtility.GetJobReportUri(jobInfo.Id, jobInfo.JobType, Path.GetExtension(basePath));
                    UploadShardFile(baseBlobUri, basePath, cached.ShardPath, jobInfo.Id);
                    cached = AdvanceToNextShard(basePath, cached.ShardPath, createTable);
                    _cache[basePath] = cached;
                }

                _cache[basePath] = (cached.ShardPath, cached.RowCount + insertCount);
                return cached.ShardPath;
            }
        }

        /// <summary>
        /// Builds a shard path: base_001.rpt, base_002.rpt,… Shard index 0 returns the original basePath unchanged.
        /// </summary>
        public static string BuildShardPath(string basePath, int shardIndex, bool isTeams = false, int teamsIndex = 0)
        {
            string dir = Path.GetDirectoryName(basePath);
            string name = Path.GetFileNameWithoutExtension(basePath);
            string ext = Path.GetExtension(basePath);
            if (isTeams) name = $"{name}{teamsIndex:D3}";
            return SecurityUtils.SafeCombinePath(dir, $"{name}{(shardIndex > 0 ? "_" + $"{shardIndex:D3}" : "")}{ext}");
        }

        public static void MergeDetailsForSubJob(string jobId, int jobType)
        {
            try
            {
                BaseJobDto jobDto = new BaseJobDto
                {
                    Id = jobId,
                    JobType = jobType,
                };
                string targetDBPath = JobReportUtility.GetJobReportPath(jobDto, JobMonitorConstants.REPORT_EXTENSION);
                CheckAndCreateDirectory(targetDBPath);
                string baseBlobUri = JobReportUtility.GetJobReportUri(jobId, jobType, string.Empty).Replace("\\", "/").TrimEnd('/');
                var blobUriList = RAStorageUtil.GetAllReportBlobNames(baseBlobUri + "_");
                bool isFirstMerge = true;
                foreach (var blobUri in blobUriList)
                {
                    var fileName = Path.GetFileName(blobUri);
                    var localPath = SecurityUtils.SafeCombinePath(Path.GetTempPath(), fileName);
                    logger.Info($"Downloading shard file {fileName} for job {jobId} for merging.");
                    RAStorageUtil.DownloadReportBlobToFile(blobUri, localPath);
                    if (isFirstMerge)
                    {
                        logger.Info($"Merging first shard file for job {jobId} by moving it to target path.");
                        File.Move(localPath, targetDBPath, true);
                        isFirstMerge = false;
                    }
                    else
                    {
                        JobDetailHelper.MergeJobDetails(JobMonitorConstants.JOBSUMMAYDETAIL, localPath, targetDBPath);
                        JobDetailHelper.MergeJobDetails(JobMonitorConstants.JOBDETAIL, localPath, targetDBPath);
                    }
                    if (File.Exists(localPath))
                    {
                        File.Delete(localPath);
                    }
                    RAStorageUtil.DeleteReportBlob(blobUri);
                }
                logger.Info($"Finish merging all shard files for job {jobId}.");
                if (File.Exists(targetDBPath))
                {
                    logger.Info($"Uploading merged report blob for job {jobId}.");
                    RAStorageUtil.UploadReportBlob(baseBlobUri + JobMonitorConstants.REPORT_EXTENSION, targetDBPath);
                    logger.Info($"Finish uploading merged report blob for job {jobId}, now deleting local merged file.");
                    File.Delete(targetDBPath);
                    logger.Info($"Finish deleting local merged file for job {jobId}.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error merging details for sub-jobs of job {jobId}: {ex}");
            }
        }

        private static void CheckAndCreateDirectory(string reportFilePath)
        {
            FileInfo reportFile = new FileInfo(reportFilePath);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
            }
        }

        private (string ShardPath, long RowCount) LoadFromDisk(string basePath, string tableName, Action<string> createTable)
        {
            int shard = 1;
            string candidatePath = BuildShardPath(basePath, shard);

            while (File.Exists(candidatePath) && SQLCommond.IsExistTable(candidatePath, tableName))
            {
                long rowCount = SQLCommond.GetRowCount(candidatePath, tableName);
                if (rowCount < _maxRowsPerShardFile)
                {
                    return (candidatePath, rowCount);
                }

                shard++;
                candidatePath = BuildShardPath(basePath, shard);
            }
            // candidatePath does not exist yet — create the table in it.
            createTable(candidatePath);
            return (candidatePath, 0);
        }

        private (string ShardPath, long RowCount) AdvanceToNextShard(string basePath, string currentShardPath, Action<string> createTable)
        {
            int nextIndex = ParseShardIndex(basePath, currentShardPath) + 1;
            string nextPath = BuildShardPath(basePath, nextIndex);
            logger.Info($"Advancing to shard {nextIndex}, path: {nextPath}");
            createTable(nextPath);
            return (nextPath, 0);
        }

        private int ParseShardIndex(string basePath, string currentShardPath)
        {
            if (string.Equals(basePath, currentShardPath, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }
            string baseNameWithoutExt = Path.GetFileNameWithoutExtension(basePath);
            string currentNameWithoutExt = Path.GetFileNameWithoutExtension(currentShardPath);
            string suffix = currentNameWithoutExt[(baseNameWithoutExt.Length + 1)..];
            return int.TryParse(suffix, out int index) ? index : 0;
        }

        private void UploadShardFile(string baseBlobUri, string basePath, string shardPath, string jobId)
        {
            var currentShardNumber = ParseShardIndex(basePath, shardPath);
            try
            {
                string shardBlobUri = BuildShardPath(baseBlobUri, currentShardNumber);
                logger.Info($"Uploading shard file for shard {currentShardNumber:D3}.");
                RAStorageUtil.UploadReportBlob(shardBlobUri, shardPath);
                logger.Info($"Finish uploading shard file, now deleting local file for shard {currentShardNumber:D3}");
                if (File.Exists(shardPath))
                {
                    File.Delete(shardPath);
                }
                logger.Info($"Finish deleting local shard file for shard {currentShardNumber:D3}.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error uploading shard file for shard {currentShardNumber:D3} to blob storage: {ex}");
            }
        }
    }
}

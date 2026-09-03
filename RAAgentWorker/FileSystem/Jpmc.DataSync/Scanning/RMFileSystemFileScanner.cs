using AvePoint.GCommon;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemFileScanner
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemFileScanner));

        private readonly RMFileSystemJobProgressTracker _jobProgressTracker;

        private readonly RMFileSystemRecoveryProcessor _recoveryProcessor;

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        private readonly RMFileSystemDirectoryMetadata _directory;

        private readonly IXSystem _filesystemClient;

        public RMFileSystemFileScanner(
            RMFileSystemJobProgressTracker jobProgressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemJobExecutionInfo executionInfo,
            RMFileSystemDirectoryMetadata directory,
            IXSystem filesystemClient)
        {
            _jobProgressTracker = jobProgressTracker;
            _recoveryProcessor = recoveryProcessor;
            _executionInfo = executionInfo;
            _directory = directory;
            _filesystemClient = filesystemClient;
        }

        public async IAsyncEnumerable<List<RMFileSystemFileMetadata>> ScanStreamingAsync()
        {
            _logger.Info($"[Scanning] Start scanning files under parent directory {_directory.FullPath.LogBase64()}.");
            foreach (var files in _filesystemClient.GetFilesInBatch(_directory.DirectoryInfo, 1000))
            {
                var eligibleFiles = GetEligibleFiles(files);
                if (eligibleFiles.Count == 0)
                {
                    continue;
                }

                eligibleFiles = await EnrichBatchDataAsync(eligibleFiles).ConfigureAwait(false);
                yield return eligibleFiles;
            }
            _logger.Info($"[Scanning] Completed scanning files under parent directory {_directory.FullPath.LogBase64()}.");
        }

        private async Task<List<RMFileSystemFileMetadata>> EnrichBatchDataAsync(List<RMFileSystemFileMetadata> files)
        {
            using var performanceScope = RMPerformanceMonitor.Scope("File Enrich Data");
            try
            {
                var records = new List<FileSystemRecordDto>();
                if (_directory.HasSynced)
                {
                    records = await performanceScope.StepAsync("Query Records",
                        async () => await GetFileSystemRecordsAsync(files).ConfigureAwait(false))
                        .ConfigureAwait(false);
                }

                if (records != null && records.Count > 0)
                {
                    var recordDict = records.ToDictionary(r => r.ItemId, r => r);
                    foreach (var file in files)
                    {
                        if (recordDict.TryGetValue(file.Id, out var record))
                        {
                            file.CurrentRecordInfo = record;
                            file.HasSynced = true;
                        }
                    }
                }

                var pendingAdsFiles = files.Where(file => file.CurrentRecordInfo == null && file.HasAds).ToList();
                if (pendingAdsFiles.Count == 0)
                {
                    return files;
                }

                var adsIds = pendingAdsFiles.Select(d => d.AdsId).Distinct().ToList();
                if (adsIds.Count > 0)
                {
                    using (performanceScope.Step("Query Records By ADS Ids"))
                    {
                        var adsRecordsDic = (await HyperHybridAPIClient.Instance
                        .QueryFileSystemRecordsByAdsIdsAsync(_executionInfo.ConnectionId, adsIds).ConfigureAwait(false))
                        .GroupBy(item => item.RecordsId)
                        .ToDictionary(item => item.Key, item => item.ToList());

                        foreach (var file in pendingAdsFiles)
                        {
                            if (adsRecordsDic.TryGetValue(file.AdsId, out var adsRecords))
                            {
                                file.SameAdsIdRecords = adsRecords.ConvertAll(item => (System.IO.File.Exists(item.FullPath), item));
                                var exists = file.SameAdsIdRecords.Any(item => item.existInLocal);
                                file.IsCopy = exists;
                                file.IsMove = !exists;
                            }
                        }
                    }
                }

                return files;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Scanning] Failed to enrich batch data under parent directory {_directory.FullPath.LogBase64()}. Error: {ex}");
                performanceScope.MarkFaulted();
                files.ForEach(file =>
                {
                    _recoveryProcessor.AddFailedItem(file);
                    _jobProgressTracker.AddFailedJobDetail(file);
                });
                return new List<RMFileSystemFileMetadata>();
            }
        }

        private List<RMFileSystemFileMetadata> GetEligibleFiles(List<XFileInfo> files)
        {
            using var performanceScope = RMPerformanceMonitor.Scope("File Eligible");

            var eligibleFiles = new List<RMFileSystemFileMetadata>();

            foreach (var file in files)
            {
                try
                {
                    var extendedFileInfo = new XFileInfoEx(file);
                    if (extendedFileInfo.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0 || extendedFileInfo.IsHidden || file.FileSize <= 0)
                    {
                        continue;
                    }

                    var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, extendedFileInfo.HighName, extendedFileInfo.LowName);
                    var id = fullPath.ToLowerInvariant().ToMd5();

                    var fileMetadata = new RMFileSystemFileMetadata
                    {
                        Id = id,
                        ParentId = _directory.Id,
                        FailedRerunId = fullPath.Substring(_executionInfo.ConnectionPath.Length + 1).ToLowerInvariant().ToMd5(),
                        FullPath = fullPath,
                        FileInfo = extendedFileInfo,
                    };

                    if (_executionInfo.ExecutionType == FSJobType.IncrementalJob &&
                        extendedFileInfo.LastWriteTimeUtc <= _executionInfo.LastScanTime &&
                        extendedFileInfo.CreationTimeUtc <= _executionInfo.LastScanTime &&
                        extendedFileInfo.LastAccessTimeUtc <= _executionInfo.LastScanTime &&
                        !_directory.IsPriorFailure &&
                        !_recoveryProcessor.ContainsFailedItem(fileMetadata.FailedRerunId))
                    {
                        continue;
                    }

                    if (_executionInfo.UniqueIdSetting.Actived)
                    {
                        var adsInfoJson = AdsHelper.ReadUniqueIdAds(fullPath);
                        if (!string.IsNullOrWhiteSpace(adsInfoJson))
                        {
                            var adsInfo = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(adsInfoJson);
                            fileMetadata.HasAds = true;
                            fileMetadata.AdsId = adsInfo.UniqueId;
                        }
                    }

                    eligibleFiles.Add(fileMetadata);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[Scanning] Failed to process eligible file {ExternalUtil.CombinePath(_executionInfo.ConnectionPath, file.HighName, file.LowName).LogBase64()} under parent path {_directory.FullPath.LogBase64()}. Error: {ex}");
                    _recoveryProcessor.AddFailedItem(file);
                    _jobProgressTracker.AddFailedJobDetail(file);
                }
            }

            return eligibleFiles;
        }

        private Task<List<FileSystemRecordDto>> GetFileSystemRecordsAsync(List<RMFileSystemFileMetadata> files)
        {
            var fileIds = files.Select(d => d.Id).ToList();
            return HyperHybridAPIClient.Instance.QueryFileSystemRecordsAsync(_executionInfo.ConnectionId, fileIds);
        }
    }
}

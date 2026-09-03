using AvePoint.GCommon;
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemDirectoryScanner
    {

        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemDirectoryScanner));

        private readonly RMFileSystemJobProgressTracker _jobProgressTracker;

        private readonly RMFileSystemRecoveryProcessor _recoveryProcessor;

        private readonly RMFileSystemJobExecutionInfo _executeNodeInfo;

        private readonly HashSet<Guid> _inheritanceBreakNodes;

        private readonly HashSet<string> _jobProcessingNodes;

        private readonly IXSystem _filesystemClient;

        public RMFileSystemDirectoryScanner(
            RMFileSystemJobProgressTracker jobProgressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemJobExecutionInfo executionNodeInfo,
            HashSet<Guid> inheritanceBreakNodes,
            HashSet<string> jobProcessingNodes,
            IXSystem filesystemClient)
        {
            _executeNodeInfo = executionNodeInfo;
            _jobProcessingNodes = jobProcessingNodes;
            _jobProgressTracker = jobProgressTracker;
            _recoveryProcessor = recoveryProcessor;
            _inheritanceBreakNodes = inheritanceBreakNodes;
            _filesystemClient = filesystemClient;
        }

        public async IAsyncEnumerable<RMFileSystemDirectoryMetadata> ScanStreamingAsync(Func<RMFileSystemDirectoryMetadata, Task<bool>> onDirectoryFoundAsync = null)
        {
            _logger.Info($"[Scanning] Starting scan streaming for connection id: [{_executeNodeInfo.ConnectionId}], connection path: [{_executeNodeInfo.ConnectionPath.LogBase64()}], execute node path: [{_executeNodeInfo.DirectoryFullPath.LogBase64()}]");

            var pendingScanDirectoryQueue = new Queue<RMFileSystemDirectoryMetadata>();
            var rootDirectory = _filesystemClient.OpenDirectory(new StorageInfo { HighName = _executeNodeInfo.DirectoryRelativePath }, FileMode.Open);
            var rootDirectoryMetadata = new RMFileSystemDirectoryMetadata
            {
                Id = _executeNodeInfo.DirectoryId,
                FailedRerunId = _executeNodeInfo.DirectoryId,
                ParentId = _executeNodeInfo.DirectoryParentId,
                FullPath = _executeNodeInfo.DirectoryFullPath,
                DirectoryInfo = rootDirectory,
                IsPriorFailure = _recoveryProcessor.ContainsFailedItem(_executeNodeInfo.DirectoryId),
                IsRoot = true,
                IsHidden = new XDirectoryInfoEx(rootDirectory).IsHidden,
                HasChanged = rootDirectory.LastWriteTimeUtc >= _executeNodeInfo.LastScanTime || rootDirectory.CreationTimeUtc >= _executeNodeInfo.LastScanTime
            };
            rootDirectoryMetadata = await EnrichDirectoryDataAsync(rootDirectoryMetadata).ConfigureAwait(false);
            if (rootDirectoryMetadata == null)
            {
                _logger.Error($"[Scanning] Failed to enrich root directory data for connection id: [{_executeNodeInfo.ConnectionId}], connection path: [{_executeNodeInfo.ConnectionPath.LogBase64()}], execute node path: [{_executeNodeInfo.DirectoryFullPath.LogBase64()}]");
                yield break;
            }

            if (!(await onDirectoryFoundAsync?.Invoke(rootDirectoryMetadata)))
            {
                _logger.Error($"[Scanning] The root directory {rootDirectoryMetadata.FullPath.LogBase64()} is not eligible for processing, skipping the scan streaming.");
                yield break;
            }

            yield return rootDirectoryMetadata;

            pendingScanDirectoryQueue.Enqueue(rootDirectoryMetadata);

            while (pendingScanDirectoryQueue.Count > 0)
            {
                var currentDirectory = pendingScanDirectoryQueue.Dequeue();

                var directoriesEnumerable = _filesystemClient.GetDirectoriesInBatch(currentDirectory.DirectoryInfo, 1000);

                foreach (var directories in directoriesEnumerable)
                {
                    var eligibleDirectories = GetEligibleDirectories(directories, currentDirectory);
                    if (eligibleDirectories == null || eligibleDirectories.Count == 0)
                    {
                        continue;
                    }

                    eligibleDirectories = await EnrichBatchDataAsync(eligibleDirectories, currentDirectory.FullPath).ConfigureAwait(false);

                    foreach (var eligibleDirectory in eligibleDirectories)
                    {
                        if (await onDirectoryFoundAsync?.Invoke(eligibleDirectory))
                        {
                            yield return eligibleDirectory;
                            pendingScanDirectoryQueue.Enqueue(eligibleDirectory);
                        }
                    }
                }
            }
            _logger.Info($"[Scanning] Completed scan streaming for connection id: [{_executeNodeInfo.ConnectionId}], connection path: [{_executeNodeInfo.ConnectionPath.LogBase64()}], execute node path: [{_executeNodeInfo.DirectoryFullPath.LogBase64()}]");
        }

        private async Task<RMFileSystemDirectoryMetadata> EnrichDirectoryDataAsync(RMFileSystemDirectoryMetadata directory)
        {
            var directories = await EnrichBatchDataAsync(new List<RMFileSystemDirectoryMetadata> { directory }, "root").ConfigureAwait(false);
            return directories.FirstOrDefault();
        }

        private async Task<List<RMFileSystemDirectoryMetadata>> EnrichBatchDataAsync(List<RMFileSystemDirectoryMetadata> directories, string exceptionTraceMarker)
        {
            using var performanceScope = RMPerformanceMonitor.Scope("Directory Enrich Data");

            try
            {
                var records = await performanceScope.StepAsync("Query Records",
                    async () => (await GetFileSystemRecordsAsync(directories).ConfigureAwait(false))
                                .ToDictionary(r => r.NodeId))
                    .ConfigureAwait(false);

                foreach (var dir in directories)
                {
                    if (records.TryGetValue(dir.Id, out var record))
                    {
                        dir.CurrentRecordInfo = record;
                        dir.HasSynced = true;
                    }
                }

                var pendingAdsDirs = directories.Where(dir => dir.CurrentRecordInfo == null && dir.HasAds).ToList();
                if (pendingAdsDirs.Count == 0) return directories;

                var adsIds = pendingAdsDirs.Select(dir => dir.AdsId).Distinct().ToList();
                if (adsIds.Count > 0)
                {
                    using (performanceScope.Step("Query Records By ADS Ids"))
                    {
                        var adsRecordsDic = (await HyperHybridAPIClient.Instance
                        .QueryFileSystemRecordsByAdsIdsAsync(_executeNodeInfo.ConnectionId, adsIds).ConfigureAwait(false))
                        .GroupBy(item => item.RecordsId)
                        .ToDictionary(item => item.Key, item => item.ToList());

                        foreach (var dir in pendingAdsDirs)
                        {
                            if (adsRecordsDic.TryGetValue(dir.AdsId, out var adsRecords))
                            {
                                dir.SameAdsIdRecords = adsRecords.ConvertAll(item => (Directory.Exists(item.FullPath), item));
                                var exists = dir.SameAdsIdRecords.Any(item => item.existInLocal);
                                dir.IsCopy = exists;
                                dir.IsMove = !exists;
                            }
                        }
                    }
                }

                return directories;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Scanning] Failed to enrich directory data for exception trace marker: [{exceptionTraceMarker.LogBase64()}]. Error: {ex}");
                performanceScope.MarkFaulted();
                directories.ForEach(dir =>
                {
                    _recoveryProcessor.AddFailedItem(dir);
                    _jobProgressTracker.AddFailedJobDetail(dir);
                });
                return new List<RMFileSystemDirectoryMetadata>();
            }
        }

        private List<RMFileSystemDirectoryMetadata> GetEligibleDirectories(List<XDirectoryInfo> directories, RMFileSystemDirectoryMetadata parentDirecotry)
        {
            using var performanceScope = RMPerformanceMonitor.Scope("Directory Eligible");

            var eligibleList = new List<RMFileSystemDirectoryMetadata>(directories.Count);

            string basePath = _executeNodeInfo.ConnectionPath;

            foreach (var directory in directories)
            {
                try
                {
                    var fullPath = ExternalUtil.CombinePath(basePath, directory.HighName, directory.LowName);
                    var lowerFullPath = fullPath.ToLowerInvariant();
                    var md5Id = lowerFullPath.ToMd5();

                    if (_inheritanceBreakNodes.Contains(md5Id))
                    {
                        _logger.Warn($"[Scanning] The directory {fullPath.LogBase64()} already inheritance breaked.");
                        continue;
                    }

                    string fullPathSha1 = RAEncodeUtil.EncryptBySHA1(lowerFullPath);
                    if (_jobProcessingNodes.Contains(fullPathSha1))
                    {
                        _logger.Warn($"[Scanning] The directory {fullPath.LogBase64()} has running job.");
                        continue;
                    }

                    if (!CanAccessDirectory(directory))
                    {
                        continue;
                    }


                    var extendedDirectoryInfo = new XDirectoryInfoEx(directory);

                    var directoryMetadata = new RMFileSystemDirectoryMetadata
                    {
                        Id = md5Id,
                        ParentId = parentDirecotry.Id,
                        FailedRerunId = md5Id,
                        FullPath = fullPath,
                        DirectoryInfo = directory,
                        IsPriorFailure = parentDirecotry.IsPriorFailure || _recoveryProcessor.ContainsFailedItem(md5Id),
                        IsHidden = extendedDirectoryInfo.IsHidden,
                        HasChanged = directory.LastWriteTimeUtc >= _executeNodeInfo.LastScanTime || directory.CreationTimeUtc >= _executeNodeInfo.LastScanTime
                    };

                    if (_executeNodeInfo.UniqueIdSetting.Actived)
                    {
                        var adsInfoJson = AdsHelper.ReadUniqueIdAds(fullPath);
                        if (!string.IsNullOrWhiteSpace(adsInfoJson))
                        {
                            var adsInfo = JsonConvert.DeserializeObject<FileSystemADSUniqueInfo>(adsInfoJson);
                            directoryMetadata.HasAds = true;
                            directoryMetadata.AdsId = adsInfo.UniqueId;
                        }
                    }

                    eligibleList.Add(directoryMetadata);
                }
                catch (Exception ex)
                {
                    _logger.Error($"[Scanning] Failed to process eligible directory {ExternalUtil.CombinePath(basePath, directory.HighName, directory.LowName).LogBase64()} under parent path {parentDirecotry.FullPath.LogBase64()}. Error: {ex}");
                    _recoveryProcessor.AddFailedItem(directory);
                    _jobProgressTracker.AddFailedJobDetail(directory);
                }
            }

            return eligibleList;
        }

        private Task<List<FileSystemRecordDto>> GetFileSystemRecordsAsync(List<RMFileSystemDirectoryMetadata> directories)
        {
            var directoryIds = directories.Select(d => d.Id).ToList();
            return HyperHybridAPIClient.Instance.QueryFileSystemRecordsAsync(_executeNodeInfo.ConnectionId, directoryIds);
        }

        private bool CanAccessDirectory(XDirectoryInfo directory)
        {
            try
            {
                var testAccess = _filesystemClient.OpenDirectory(directory, FileMode.Open);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warn($"[Scanning] Cannot access directory {ExternalUtil.CombinePath(_executeNodeInfo.ConnectionPath, directory.HighName, directory.LowName).LogBase64()}. Error: {ex}");
                _recoveryProcessor.AddFailedItem(directory);
                _jobProgressTracker.AddFailedJobDetail(directory);
                return false;
            }
        }
    }
}

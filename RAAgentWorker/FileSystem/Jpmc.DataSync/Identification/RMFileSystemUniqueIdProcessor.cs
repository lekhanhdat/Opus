using AvePoint.GCommon;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.Common.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{

    public class RMFileSystemUniqueIdProcessor
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemUniqueIdProcessor));

        private readonly RMFileSystemJobProgressTracker _jobProgressTracker;

        private readonly RMFileSystemRecoveryProcessor _recoveryProcessor;

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        public RMFileSystemUniqueIdProcessor(
            RMFileSystemJobProgressTracker jobProgressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemJobExecutionInfo executionInfo
            )
        {
            _jobProgressTracker = jobProgressTracker;
            _recoveryProcessor = recoveryProcessor;
            _executionInfo = executionInfo;
        }

        public async Task<bool> ProcessDirectoryAsync(RMFileSystemDirectoryMetadata directory)
        {
            var succeedItems = await ProcessItemsAsync(new List<RMFileSystemDirectoryMetadata> { directory }, isFolder: true);
            return succeedItems.Count > 0;
        }

        public Task<List<RMFileSystemFileMetadata>> ProcessFilesAsync(List<RMFileSystemFileMetadata> files)
        {
            return ProcessItemsAsync(files, isFolder: false);
        }

        private async Task<List<T>> ProcessItemsAsync<T>(List<T> items, bool isFolder) where T : RMFileSystemItemMetadata
        {
            if (items == null || items.Count == 0) return items;

            if (!_executionInfo.UniqueIdSetting.Actived)
            {
                return await ProcessItemsByDefaultAsync(items).ConfigureAwait(false);
            }

            var succeedItems = new List<T>();

            var noAdsItems = items.Where(item => !item.HasAds).ToList();
            var hasAdsItems = items.Where(item => item.HasAds).ToList();

            if (noAdsItems.Count > 0)
            {
                var processNoAdsSucceedItems = await ProcessNoAdsItemsAsync(noAdsItems, isFolder).ConfigureAwait(false);
                succeedItems.AddRange(processNoAdsSucceedItems);
            }

            if (hasAdsItems.Count > 0)
            {
                var deleteUnmatchedSucceedItems = await DeleteUnmatchedRecordsAsync(hasAdsItems).ConfigureAwait(false);
                succeedItems.AddRange(deleteUnmatchedSucceedItems);
            }

            return succeedItems;
        }


        private async Task<List<T>> ProcessNoAdsItemsAsync<T>(List<T> items, bool isFolder) where T : RMFileSystemItemMetadata
        {
            using var performanceScope = RMPerformanceMonitor.Scope($"{(isFolder ? "Folder" : "File")} No Ads");
            try
            {
                var succeedItem = new HashSet<T>(items);

                var needNewUniqueIdItems = items
                .Where(item => !item.HasSynced || string.IsNullOrWhiteSpace(item.CurrentRecordInfo.RecordsId))
                .ToList();

                if (needNewUniqueIdItems.Count > 0)
                {
                    using (performanceScope.Step("Get Available Unique Id"))
                    {
                        var uniqueIds = await GetAvailableUniqueIdsAsync(needNewUniqueIdItems.Count).ConfigureAwait(false);
                        for (var i = 0; i < needNewUniqueIdItems.Count; i++)
                        {
                            needNewUniqueIdItems[i].AdsId = FormatUniqueId(uniqueIds[i]);
                        }
                    }
                }

                foreach (var item in items.Where(item => item.HasSynced && !string.IsNullOrWhiteSpace(item.CurrentRecordInfo.RecordsId)))
                {
                    item.AdsId = item.CurrentRecordInfo.RecordsId;
                }

                if (_executionInfo.UniqueIdSetting.Stored)
                {
                    using (performanceScope.Step("Write Unique Id to Local"))
                    {
                        foreach (var item in items)
                        {
                            if (!WriteAdsIdToLocal(isFolder, item))
                            {
                                succeedItem.Remove(item);
                            }
                        }
                    }
                }
                return succeedItem.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"[Identification] Failed to process {(isFolder ? "folder" : "file")} items for UniqueId processing. Error: {ex}");
                performanceScope.MarkFaulted();
                items.ForEach(item =>
                {
                    _recoveryProcessor.AddFailedItem(item);
                    _jobProgressTracker.AddFailedJobDetail(item);
                });
                return new List<T>();
            }
        }

        private async Task<List<T>> DeleteUnmatchedRecordsAsync<T>(List<T> hasAdsItems) where T : RMFileSystemItemMetadata
        {
            try
            {
                var needDeleteRecordIds = hasAdsItems
                .Where(item => item.SameAdsIdRecords != null)
                .SelectMany(item => item.SameAdsIdRecords)
                .Where(item => !item.existInLocal)
                .Select(item => item.record.NodeId)
                .Distinct()
                .ToList();

                if (needDeleteRecordIds.Count > 0)
                {
                    _logger.Info($"[Identification] Deleting {string.Join(", ", needDeleteRecordIds)} unmatched records for UniqueId processing.");
                    await HyperHybridAPIClient.Instance
                        .DeleteFileSystemRecordsAsync(_executionInfo.ConnectionId, needDeleteRecordIds)
                        .ConfigureAwait(false);
                }
                return hasAdsItems;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Identification] Failed to delete unmatched records for UniqueId processing. Error: {ex}");
                hasAdsItems.ForEach(item =>
                {
                    _recoveryProcessor.AddFailedItem(item);
                    _jobProgressTracker.AddFailedJobDetail(item);
                });
                return new List<T>();
            }
        }

        private async Task<List<T>> ProcessItemsByDefaultAsync<T>(List<T> items) where T : RMFileSystemItemMetadata
        {
            try
            {
                var needNewUniqueIdItems = items
                .Where(item => !item.HasSynced || string.IsNullOrWhiteSpace(item.CurrentRecordInfo.RecordsId))
                .ToList();
                if (needNewUniqueIdItems.Count > 0)
                {
                    var uniqueIds = await GetAvailableUniqueIdsAsync(needNewUniqueIdItems.Count).ConfigureAwait(false);
                    for (var i = 0; i < needNewUniqueIdItems.Count; i++)
                    {
                        needNewUniqueIdItems[i].AdsId = FormatUniqueId(uniqueIds[i]);
                    }
                }
                foreach (var item in items.Where(item => item.HasSynced && !string.IsNullOrWhiteSpace(item.CurrentRecordInfo.RecordsId)))
                {
                    item.AdsId = item.CurrentRecordInfo.RecordsId;
                }

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Identification] Failed to process {(typeof(T) == typeof(RMFileSystemDirectoryMetadata) ? "folder" : "file")} items for default UniqueId processing. Error: {ex}");
                items.ForEach(item =>
                {
                    _recoveryProcessor.AddFailedItem(item);
                    _jobProgressTracker.AddFailedJobDetail(item);
                });
                return new List<T>();
            }
        }

        private bool WriteAdsIdToLocal<T>(bool isFolder, T item) where T : RMFileSystemItemMetadata
        {
            try
            {
                AdsHelper.WriteUniqueIdAdsAndRevertTime(item.FullPath, new FileSystemADSUniqueInfo
                {
                    UniqueId = item.AdsId
                }, isFolder);

                AdsHelper.WriteTermIdAdsAndRevertTime(item.FullPath, new FileSystemADSTermInfo
                {
                    TermId = item.ClassCodeInfo?.Id.ToString()
                }, isFolder);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Identification] Failed to write ADS for {(isFolder ? "folder" : "file")} '{item.FullPath.LogBase64()}' with UniqueId '{item.AdsId}' and TermId '{item.ClassCodeInfo?.Id.ToString()}'. Error: {ex}");
                _recoveryProcessor.AddFailedItem(item);
                _jobProgressTracker.AddFailedJobDetail(item);
                return false;
            }
        }

        private Task<List<long>> GetAvailableUniqueIdsAsync(int count)
        {
            return HyperHybridAPIClient.Instance.GetFileSystemAvailableUniqueIdsAsync(count);
        }

        private string FormatUniqueId(long uniqueId)
        {
            var uniqueIdStr = uniqueId.ToString();
            if(uniqueId < Math.Pow(10, 10))
            {
                uniqueIdStr = uniqueIdStr.PadLeft(10, '0');
            }

            var prefx = _executionInfo.UniqueIdSetting.Actived ? _executionInfo.UniqueIdSetting.Prefix : "REC";
            return $"{prefx}-{uniqueIdStr}";
        }
    }
}


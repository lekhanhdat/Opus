using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemRecoveryProcessor
    {

        private const int FAILED_ITEM_THROTTLING = 100_000;

        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemRecoveryProcessor));

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        private readonly RMFileSystemConcurrentHashSet<Guid> _lastFailedRerunIds = new RMFileSystemConcurrentHashSet<Guid>();

        private readonly RMFileSystemConcurrentHashSet<RMFileSystemFailedItem> _currentFailedItems = new RMFileSystemConcurrentHashSet<RMFileSystemFailedItem>();

        public RMFileSystemRecoveryProcessor(RMFileSystemJobExecutionInfo exectionInfo)
        {
            _executionInfo = exectionInfo;
        }

        public async Task InitAsync()
        {
            _logger.Info($"Start to init recovery processor for directory {_executionInfo.DirectoryId}.");

            string continuationToken = null;
            do
            {
                var pagination = await HyperHybridAPIClient.Instance.QueryFileSystemFailedItemsAsync(_executionInfo.DirectoryId.ToString(), continuationToken);
                continuationToken = pagination.ContinuationToken;
                pagination.FailedItems.ForEach(item => _lastFailedRerunIds.Add(item.FailedRerunId));
            } while (continuationToken != null);

            _logger.Info($"End to init recovery processor for directory {_executionInfo.DirectoryId}.");
        }

        public bool ContainsFailedItem(Guid itemId)
        {
            return _lastFailedRerunIds.Contains(itemId);
        }

        public void AddFailedItem(RMFileSystemItemMetadata item)
        {
            _currentFailedItems.Add(new RMFileSystemFailedItem
            {
                ItemId = Guid.NewGuid(),
                FailedRerunId = item.FailedRerunId,
                ScopeId = _executionInfo.DirectoryId.ToString(),
                JobId = JobContext.Current.JobId,
                FullPath = item.FullPath,
            });
        }

        public void AddFailedItem(FileSystemRecordDto item)
        {
            _currentFailedItems.Add(new RMFileSystemFailedItem
            {
                ItemId = Guid.NewGuid(),
                FailedRerunId = item.NodeType == (int)NodeLevel.FSFile ? item.FullPath.Substring(_executionInfo.ConnectionPath.Length + 1).ToLowerInvariant().ToMd5() : item.NodeId,
                ScopeId = _executionInfo.DirectoryId.ToString(),
                JobId = JobContext.Current.JobId,
                FullPath = item.FullPath,
            });
        }

        public void AddFailedItems(List<FileSystemRecordDto> items)
        {
            foreach (var item in items)
            {
                AddFailedItem(item);
            }
        }

        public void AddFailedItem(XDirectoryInfo directory)
        {
            var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, directory.HighName, directory.LowName);
            _currentFailedItems.Add(new RMFileSystemFailedItem
            {
                ItemId = Guid.NewGuid(),
                FailedRerunId = fullPath.ToLowerInvariant().ToMd5(),
                ScopeId = _executionInfo.DirectoryId.ToString(),
                JobId = JobContext.Current.JobId,
                FullPath = fullPath,
            });
        }

        public void AddFailedItem(XFileInfo file)
        {
            var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, file.HighName, file.LowName);
            _currentFailedItems.Add(new RMFileSystemFailedItem
            {
                ItemId = Guid.NewGuid(),
                FailedRerunId = fullPath.Substring(_executionInfo.ConnectionPath.Length + 1).ToLowerInvariant().ToMd5(),
                ScopeId = _executionInfo.DirectoryId.ToString(),
                JobId = JobContext.Current.JobId,
                FullPath = fullPath,
            });
        }

        public void AddFailedItems(List<XFileInfo> files)
        {
            foreach (var file in files)
            {
                var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, file.HighName, file.LowName);
                _currentFailedItems.Add(new RMFileSystemFailedItem
                {
                    ItemId = Guid.NewGuid(),
                    FailedRerunId = fullPath.Substring(_executionInfo.ConnectionPath.Length + 1).ToLowerInvariant().ToMd5(),
                    ScopeId = _executionInfo.DirectoryId.ToString(),
                    JobId = JobContext.Current.JobId,
                    FullPath = fullPath,
                });
            }
        }

        public async Task<bool> SyncFailedItemsAsync()
        {
            try
            {
                if (_currentFailedItems.Count > FAILED_ITEM_THROTTLING)
                {
                    _logger.Warn($"The number of failed items {_currentFailedItems.Count} exceeds the threshold {FAILED_ITEM_THROTTLING}, will not sync to database.");

                    var currentFailedItemIds = _currentFailedItems.ToList().Select(item => item.FailedRerunId).ToHashSet();
                    var succeedRerunItemIds = _lastFailedRerunIds.ToList().Where(id => !currentFailedItemIds.Contains(id)).ToList();
                    await HyperHybridAPIClient.Instance.DeleteFileSystemFailedItemsAsync(_executionInfo.DirectoryId.ToString(), succeedRerunItemIds);

                    _logger.Info($"Successfully deleted {succeedRerunItemIds.Count} succeeded failed items for directory {_executionInfo.DirectoryId}.");
                    return false;
                }

                _logger.Info($"Start to sync failed items for directory {_executionInfo.DirectoryId}.");

                await HyperHybridAPIClient.Instance.DeleteFileSystemFailedItemsAsync(_executionInfo.DirectoryId.ToString());
                await HyperHybridAPIClient.Instance.AddFileSystemFailedItemsAsync(_currentFailedItems.ToList());

                _logger.Info($"End to sync failed items for directory {_executionInfo.DirectoryId}.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to sync failed items for directory {_executionInfo.DirectoryId}. Error: {ex}");
                return false;
            }
        }
    }
}

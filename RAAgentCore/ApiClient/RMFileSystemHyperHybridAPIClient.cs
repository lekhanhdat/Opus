using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAFileSystemCore.ApiClient
{
    public partial class HyperHybridAPIClient
    {
        private static readonly AveRetryPolicy ApiRetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(5)));

        public async Task<FileSystemRecordDto> QueryFileSystemRecordAsync(string connectionId, Guid itemId)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.QueryFileSystemRecordAsync(connectionId, itemId);
            });
        }

        public async Task<List<FileSystemRecordDto>> QueryFileSystemRecordsAsync(string connectionId, List<Guid> itemIds)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.QueryFileSystemRecordsAsync(new RMFileSystemItemRequestModel
                {
                    ConnectionId = connectionId,
                    ItemIds = itemIds
                });
            });
        }

        public async Task<List<FileSystemRecordDto>> QueryFileSystemRecordsByAdsIdsAsync(string connectionId, List<string> adsIds)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.QueryFileSystemRecordsByAdsAsync(new RMFileSystemAdsQueryModel
                {
                    ConnectionId = connectionId,
                    AdsIds = adsIds
                });
            });
        }

        public async Task<List<long>> GetFileSystemAvailableUniqueIdsAsync(int count)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.GetFileSystemAvailableUniqueIdsAsync(count);
            });
        }

        public async Task<bool> DeleteFileSystemRecordsAsync(string connectionId, List<Guid> itemIds)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.DeleteFileSystemRecordAsync(new RMFileSystemItemRequestModel
                {
                    ConnectionId = connectionId,
                    ItemIds = itemIds
                });
            });
        }

        public async Task UpsertFileSystemRecordsAsync(List<FileSystemRecordDto> records)
        {
            await ApiRetryPolicy.ExecuteAction(async () =>
            {
                await HybridClient.FileSystemService.UpsertFileSystemRecordsAsync(records);
            });
        }

        public async Task AddFileSystemAuditAsync(List<RMFileSystemAudit> audits)
        {
            await ApiRetryPolicy.ExecuteAction(async () =>
            {
                await HybridClient.FileSystemService.AddFileSystemAuditAsync(audits);
            });
        }

        public async Task<RMFileSystemFailedItemPagination> QueryFileSystemFailedItemsAsync(string scopeId, string continuationToken, int pageSize = 1_000)
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.QueryFileSystemFailedItemsAsync(new RMFileSystemFailedItemPaginationQueryModel
                {
                    ScopeId = scopeId,
                    ContinuationToken = continuationToken,
                    PageSize = pageSize
                });
            });
        }

        public async Task DeleteFileSystemFailedItemsAsync(string scopeId)
        {
            await ApiRetryPolicy.ExecuteAction(async () =>
             {
                 await HybridClient.FileSystemService.DeleteFileSystemFailedItemsByScopeIdAsync(scopeId);
             });
        }

        public async Task DeleteFileSystemFailedItemsAsync(string scopeId, List<Guid> itemIds)
        {
            await ApiRetryPolicy.ExecuteAction(async () =>
             {
                 await HybridClient.FileSystemService.DeleteFileSystemFailedItemsAsync(new RMFileSystemFailedItemDeleteModel
                 {
                     ScopeId = scopeId,
                     ItemIds = itemIds
                 });
             });
        }

        public async Task AddFileSystemFailedItemsAsync(List<RMFileSystemFailedItem> failedItems)
        {
            await ApiRetryPolicy.ExecuteAction(async () =>
             {
                 await HybridClient.FileSystemService.AddFileSystemFailedItemsAsync(failedItems);
             });
        }

        public async Task<int> GetMaxConcurrentExecutionAsync()
        {
            return await ApiRetryPolicy.ExecuteAction(async () =>
            {
                return await HybridClient.FileSystemService.GetMaxConcurrentExecutionAsync();
            });
        }
    }
}

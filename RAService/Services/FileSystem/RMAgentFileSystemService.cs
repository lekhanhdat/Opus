using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.UniqueId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.FileSystem
{
    public class RMAgentFileSystemService : IRMAgentFileSystemService
    {
        private IFSAuditSinkService FSAuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMAgentFileSystemService));

        public async Task<FileSystemRecordDto> QueryRecordAsync(string connectionId, Guid itemId)
        {
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var record = await container
                    .UseLinqQuery()
                    .Where(item => item.L2PartitionKey == connectionId && item.Id == itemId)
                    .AsResultSet()
                    .FirstOrDefault()
                    .ConfigureAwait(false);
                return ConvertUtil.ConvertRMBaseRecordToFSDto(record);
            }
            catch (Exception ex)
            {
                _logger.Error($"QueryRecordAsync failed for connectionId: {connectionId}, itemId: {itemId}. Exception: {ex}");
                throw;
            }
        }

        public async Task<List<FileSystemRecordDto>> QueryRecordsAsync(RMFileSystemItemRequestModel requestModel)
        {
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var records = await container
                    .UseLinqQuery()
                    .Where(item => item.L2PartitionKey == requestModel.ConnectionId && requestModel.ItemIds.Contains(item.Id))
                    .AsResultSet()
                    .AllAsync()
                    .ToListAsync()
                    .ConfigureAwait(false);
                return records.Select(ConvertUtil.ConvertRMBaseRecordToFSDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"QueryRecordsAsync failed for connectionId: {requestModel.ConnectionId}, itemIds: {string.Join(",", requestModel.ItemIds)}. Exception: {ex}");
                throw;
            }
        }

        public async Task<List<FileSystemRecordDto>> QueryRecordsByAdsAsync(RMFileSystemAdsQueryModel queryModel)
        {
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var records = await container
                    .UseLinqQuery()
                    .Where(item => item.L2PartitionKey == queryModel.ConnectionId && queryModel.AdsIds.Contains(item.RecordsId))
                    .AsResultSet()
                    .AllAsync()
                    .ToListAsync()
                    .ConfigureAwait(false);
                return records.Select(ConvertUtil.ConvertRMBaseRecordToFSDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"QueryRecordsByAdsAsync failed for connectionId: {queryModel.ConnectionId}, adsIds: {string.Join(",", queryModel.AdsIds)}. Exception: {ex}");
                throw;
            }
        }

        public async Task UpsertRecordsAsync(List<FileSystemRecordDto> records)
        {
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var rmRecords = records.Select(ConvertUtil.ConvertFSDtoToRMBaseRecord).ToList();
                await container.UpsertRangeAsync(rmRecords);
            }
            catch (Exception ex)
            {
                _logger.Error($"UpsertRecordsAsync failed for records count: {records.Count}. Exception: {ex}");
                throw;
            }
        }

        public async Task<bool> DeleteRecordsAsync(RMFileSystemItemRequestModel requestModel)
        {
            try
            {
                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                var records = await container
                    .UseLinqQuery()
                    .Where(item => item.L2PartitionKey == requestModel.ConnectionId && requestModel.ItemIds.Contains(item.Id))
                    .AsResultSet()
                    .AllAsync()
                    .ToListAsync()
                    .ConfigureAwait(false);
                await container.DeleteRangeAsync(records);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"DeleteRecordsAsync failed for connectionId: {requestModel.ConnectionId}, itemIds: {string.Join(",", requestModel.ItemIds)}. Exception: {ex}");
                throw;
            }
        }

        public async Task<List<long>> GetAvailableUniqueIdsAsync(int count)
        {
            try
            {
                var idUtil = new UniqueIdUtil();
                return idUtil.GetFSUniqueIdList(TenantLocalValue.LogonGroupId, count);
            }
            catch (Exception ex)
            {
                _logger.Error($"GetAvailableUniqueIdsAsync failed for count: {count}. Exception: {ex}");
                throw;
            }
        }

        public async Task AddAuditAsync(List<RMFileSystemAudit> audits)
        {
            await FSAuditSinkService.FlushAsync(audits);
        }

        public async Task<RMFileSystemFailedItemPagination> QueryFailedItemsAsync(RMFileSystemFailedItemPaginationQueryModel queryModel)
        {
            try
            {
                var result = await RMRecordStorageAzureTableContext.DataSyncFailureList.QueryWithPagination(i => i.PartitionKey == queryModel.ScopeId, queryModel.PageSize, queryModel.ContinuationToken);
                return new RMFileSystemFailedItemPagination
                {
                    ContinuationToken = result.ContinuatioinToken,
                    FailedItems = result.Values.Select(e => new RMFileSystemFailedItem
                    {
                        FailedRerunId = new Guid(e.RowKey),
                        FullPath = e.FullPath,
                        ScopeId = e.PartitionKey,
                        JobId = e.JobId
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"QueryFailedItemsAsync failed for scopeId: {queryModel.ScopeId}, pageSize: {queryModel.PageSize}, continuationToken: {queryModel.ContinuationToken}. Exception: {ex}");
                throw;
            }
        }

        public async Task DeleteFailedItemsAsync(RMFileSystemFailedItemDeleteModel deletionModel)
        {
            if (deletionModel?.ItemIds == null || deletionModel.ItemIds.Count == 0) return;
            try
            {
                foreach (var batch in deletionModel.ItemIds.Chunk(500))
                {
                    var rowKeys = batch.Select(id => id.ToString()).ToHashSet();
                    await RMRecordStorageAzureTableContext.DataSyncFailureList.Delete(failureItem => failureItem.PartitionKey == deletionModel.ScopeId && rowKeys.Contains(failureItem.RowKey));
                    _logger.Info($"DeleteFailedItemsAsync succeeded for scopeId: {deletionModel.ScopeId}, itemIds count: {batch.Count()}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"DeleteFailedItemsAsync failed for scopeId: {deletionModel.ScopeId}, " + $"itemIds count: {deletionModel.ItemIds.Count}. Exception: {ex}");
                throw;
            }
        }

        public async Task DeleteFailedItemsByScopeIdAsync(string scopeId)
        {
            try
            {
                await RMRecordStorageAzureTableContext.DataSyncFailureList.Delete(failureItem => failureItem.PartitionKey == scopeId);
                _logger.Info($"DeleteFailedItemsByScopeIdAsync succeeded for scopeId: {scopeId}");
            }
            catch (Exception ex)
            {
                _logger.Error($"DeleteFailedItemsByScopeIdAsync failed for scopeId: {scopeId}. Exception: {ex}");
                throw;
            }
        }

        public async Task AddFailedItemsAsync(List<RMFileSystemFailedItem> failedItems)
        {
            try
            {
                if (failedItems == null || failedItems.Count == 0) return;

                foreach (var batch in failedItems.Chunk(500))
                {
                    var failureEntities = failedItems.Select(item =>
                    {
                        var rowKey = item.FailedRerunId.ToString();
                        return new SyncFailureItemEntity(item.ScopeId, rowKey)
                        {
                            DataSource = (int)SourceFlag.FileSystem,
                            JobId = item.JobId,
                            FullPath = item.FullPath,
                            ItemId = item.GetHashCode(),
                            RowKey = rowKey,
                        };
                    }).ToList();

                    await RMRecordStorageAzureTableContext.DataSyncFailureList.UpsertMergeRange(failureEntities);
                    _logger.Info($"AddFailedItemsAsync succeeded for failedItems count: {failedItems.Count}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"AddFailedItemsAsync failed for failedItems count: {failedItems.Count}. Exception: {ex}");
                throw;
            }
        }
    }
}

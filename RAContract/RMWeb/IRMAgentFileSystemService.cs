using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMAgentFileSystemService
    {
        Task<FileSystemRecordDto> QueryRecordAsync(string connectionId, Guid itemId);

        Task<List<FileSystemRecordDto>> QueryRecordsAsync(RMFileSystemItemRequestModel requestModel);

        Task<List<FileSystemRecordDto>> QueryRecordsByAdsAsync(RMFileSystemAdsQueryModel queryModel);

        System.Threading.Tasks.Task UpsertRecordsAsync(List<FileSystemRecordDto> records);

        Task<bool> DeleteRecordsAsync(RMFileSystemItemRequestModel requestModel);

        Task<List<long>> GetAvailableUniqueIdsAsync(int count);

        System.Threading.Tasks.Task AddAuditAsync(List<RMFileSystemAudit> audits);

        System.Threading.Tasks.Task DeleteFailedItemsAsync(RMFileSystemFailedItemDeleteModel deletionModel);

        System.Threading.Tasks.Task DeleteFailedItemsByScopeIdAsync(string scopeId);

        Task<RMFileSystemFailedItemPagination> QueryFailedItemsAsync(RMFileSystemFailedItemPaginationQueryModel queryModel);

        System.Threading.Tasks.Task AddFailedItemsAsync(List<RMFileSystemFailedItem> failedItems);
    }
}

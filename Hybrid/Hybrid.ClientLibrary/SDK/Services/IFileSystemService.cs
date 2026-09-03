using AvePoint.Hybrid.ClientCore;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.ClientLibrary.SDK.Services
{
    public interface IFileSystemService
    {
        [Api(Url = "api/filesystem/queryrecord", HttpMethod = "GET")]
        Task<FileSystemRecordDto> QueryFileSystemRecordAsync(string connectionId, Guid itemId);

        [Api(Url = "api/filesystem/queryrecords", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> QueryFileSystemRecordsAsync(RMFileSystemItemRequestModel requestModel);

        [Api(Url = "api/filesystem/queryrecordsbyads", HttpMethod = "POST")]
        Task<List<FileSystemRecordDto>> QueryFileSystemRecordsByAdsAsync(RMFileSystemAdsQueryModel requestModel);

        [Api(Url = "api/filesystem/upsertrecords", HttpMethod = "POST")]
        Task UpsertFileSystemRecordsAsync(List<FileSystemRecordDto> records);

        [Api(Url = "api/filesystem/deleterecords", HttpMethod = "POST")]
        Task<bool> DeleteFileSystemRecordAsync(RMFileSystemItemRequestModel requestModel);

        [Api(Url = "api/filesystem/getavailableuniqueids", HttpMethod = "GET")]
        Task<List<long>> GetFileSystemAvailableUniqueIdsAsync(int count);

        [Api(Url = "api/filesystem/addaudit", HttpMethod = "POST")]
        Task AddFileSystemAuditAsync(List<RMFileSystemAudit> audits);

        [Api(Url = "api/filesystem/queryfaileditems", HttpMethod = "POST")]
        Task<RMFileSystemFailedItemPagination> QueryFileSystemFailedItemsAsync(RMFileSystemFailedItemPaginationQueryModel queryModel);

        [Api(Url = "api/filesystem/deletefaileditemsbyscopeid", HttpMethod = "GET")]
        Task DeleteFileSystemFailedItemsByScopeIdAsync(string scopeId);

        [Api(Url = "api/filesystem/deletefaileditems", HttpMethod = "POST")]
        Task DeleteFileSystemFailedItemsAsync(RMFileSystemFailedItemDeleteModel deleteModel);

        [Api(Url = "api/filesystem/addfaileditems", HttpMethod = "POST")]
        Task AddFileSystemFailedItemsAsync(List<RMFileSystemFailedItem> failedItems);

        [Api(Url = "api/filesystem/getmaxconcurrentexecution", HttpMethod = "GET")]
        Task<int> GetMaxConcurrentExecutionAsync();
    }
}

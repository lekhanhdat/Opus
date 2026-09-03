using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.UniqueId;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/filesystem/[action]")]
    [APIScopeFilter(RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [ApiController]
    public class RMFileSystemController : RAWebApiBase
    {
        private IRMAgentFileSystemService AgentFileSystemService => PlatformWindsorManager.GetService<IRMAgentFileSystemService>();

        [HttpGet]
        public async Task<FileSystemRecordDto> QueryRecord(string connectionId, Guid itemId)
        {
            return await AgentFileSystemService.QueryRecordAsync(connectionId, itemId);
        }

        [HttpPost]
        public async Task<List<FileSystemRecordDto>> QueryRecords([FromBody] RMFileSystemItemRequestModel requestModel)
        {
            return await AgentFileSystemService.QueryRecordsAsync(requestModel);
        }

        [HttpPost]
        public async Task<List<FileSystemRecordDto>> QueryRecordsByAds([FromBody] RMFileSystemAdsQueryModel queryModel)
        {
            return await AgentFileSystemService.QueryRecordsByAdsAsync(queryModel);
        }

        [HttpPost]
        public async Task UpsertRecords([FromBody] List<FileSystemRecordDto> records)
        {
            await AgentFileSystemService.UpsertRecordsAsync(records);
        }

        [HttpPost]
        public async Task<bool> DeleteRecords([FromBody] RMFileSystemItemRequestModel requestModel)
        {
            return await AgentFileSystemService.DeleteRecordsAsync(requestModel);
        }

        [HttpGet]
        public async Task<List<long>> GetAvailableUniqueIds(int count)
        {
            UniqueIdUtil idUtil = new UniqueIdUtil();
            return idUtil.GetFSUniqueIdList(TenantLocalValue.LogonGroupId, count);
        }

        [HttpPost]
        public async Task AddAudit([FromBody] List<RMFileSystemAudit> audits)
        {
            await AgentFileSystemService.AddAuditAsync(audits);
        }

        [HttpPost]
        public async Task<RMFileSystemFailedItemPagination> QueryFailedItems([FromBody] RMFileSystemFailedItemPaginationQueryModel queryModel)
        {
            return await AgentFileSystemService.QueryFailedItemsAsync(queryModel);
        }

        [HttpPost]
        public Task DeleteFailedItems([FromBody] RMFileSystemFailedItemDeleteModel deletionModel)
        {
            return AgentFileSystemService.DeleteFailedItemsAsync(deletionModel);
        }

        [HttpGet]
        public Task DeleteFailedItemsByScopeId(string scopeId)
        {
            return AgentFileSystemService.DeleteFailedItemsByScopeIdAsync(scopeId);
        }

        [HttpPost]
        public Task AddFailedItems([FromBody] List<RMFileSystemFailedItem> failedItems)
        {
            return AgentFileSystemService.AddFailedItemsAsync(failedItems);
        }

        [HttpGet]
        public async Task<int> GetMaxConcurrentExecutionAsync()
        {
            return FSHighPerformanceUtility.LoadFSHighPerformanceConfig().Setting.MaxDegreeOfParallelism;
        }
    }
}

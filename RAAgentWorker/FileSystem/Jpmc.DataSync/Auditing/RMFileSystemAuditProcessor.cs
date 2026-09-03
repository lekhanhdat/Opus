using AvePoint.GCommon;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Services;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemAuditProcessor
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemAuditProcessor));

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        public RMFileSystemAuditProcessor(RMFileSystemJobExecutionInfo executionInfo)
        {
            _executionInfo = executionInfo;
        }

        public Task ProcessDirectoryAsync(RMFileSystemDirectoryMetadata directory)
        {
            return ProcessItemsAsync(new List<RMFileSystemDirectoryMetadata> { directory });
        }

        public Task ProcessFilesAsync(List<RMFileSystemFileMetadata> files)
        {
            return ProcessItemsAsync(files);
        }

        private async Task ProcessItemsAsync<T>(List<T> items) where T : RMFileSystemItemMetadata
        {
            try
            {
                var audits = items.Where(item => item.IsMove).Select(item => new RMFileSystemAudit
                {
                    ConnectionGroupId = _executionInfo.ConnectionGroupId,
                    ConnectionId = _executionInfo.ConnectionId,
                    ItemId = item.Id,
                    Level = item.FullPath == _executionInfo.ConnectionPath ? FSJPMCAuditLevel.Connection : FSJPMCAuditLevel.Folder,
                    OriginPath = item.SameAdsIdRecords.OrderByDescending(item => item.record.CollectionTime).FirstOrDefault().record?.FullPath,
                    TargetPath = item.FullPath,
                }).ToList();
                if (audits.Count > 0)
                {
                    using (RMPerformanceMonitor.Scope("Add Audit"))
                    {
                        await HyperHybridAPIClient.Instance.AddFileSystemAuditAsync(audits);
                        _logger.Info($"[Auditing] Successfully processed move audit for items: {string.Join(",", items.Select(item => item.FullPath.LogBase64()))}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[Auditing] Failed to process move audit for items: {string.Join(",", items.Select(item => item.FullPath.LogBase64()))}. Error: {ex}");
            }
        }
    }
}

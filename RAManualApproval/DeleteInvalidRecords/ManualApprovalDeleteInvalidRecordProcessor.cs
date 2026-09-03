using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RAManualApproval.BulkAction;
using RAManualApproval.ImportAction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAManualApproval.DeleteInvalidRecords
{
    public class ManualApprovalDeleteInvalidRecordProcessor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalDeleteInvalidRecordProcessor));
        private static readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        private ITeamsSettingDao TeamsSettingsDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMMailboxDao RMMailboxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();
        private IRMBoxConnectionGroupDao BoxConnectionGroupDao => PlatformWindsorManager.GetService<IRMBoxConnectionGroupDao>();
        private IRMBoxConnectionDao BoxConnectionDao => PlatformWindsorManager.GetService<IRMBoxConnectionDao>();
        private IBoxSettingDao BoxSettingDao => PlatformWindsorManager.GetService<IBoxSettingDao>();
        private readonly string _jobId;
        private readonly ConcurrentDictionary<Guid, int> _originalManualStatuses = new();
        private readonly ConcurrentBag<Record> _succeedRecords = new();
        private readonly ConcurrentBag<(Record record, string error)> _failedRecords = new();

        public ManualApprovalDeleteInvalidRecordProcessor(string jobId)
        {
            _jobId = jobId;
            ManualApprovalBulkActionManager.Init(jobId, JobType.DeleteInvalidRecords);
        }

        public async Task RunAsync()
        {
            try
            {
                Logger.Info($"Start DeleteInvalidRecords job: {_jobId}");
                ManualApprovalDataSyncManager.RegisteProcessItemCallback(
                    ProcessItemSucceedAsync,
                    ProcessItemFailed);

                ManualApprovalBulkActionManager.IncreaseBase(10000);

                var invalidContainerIds = await GetInvalidRemoteNodesContainerId();
                var invalidBoxConnectionIds = await GetInvalidBoxConnectionIds();
                var invalidExchangeOnlineGroupIds = await GetInvalidExchangeOnlineGroupIds();
                var invalidPhysicalLocationIds = await GetInvalidPhysicalRecordLocationIds();
                invalidContainerIds.UnionWith(invalidBoxConnectionIds);
                invalidContainerIds.UnionWith(invalidExchangeOnlineGroupIds);

                var invalidTargets = invalidContainerIds
                    .Select(id => (Id: id, IsPhysicalRecord: false))
                    .Concat(invalidPhysicalLocationIds.Select(id => (Id: id, IsPhysicalRecord: true)))
                    .ToList();

                if (!invalidTargets.Any())
                {
                    Logger.Info("No invalid containers found.");
                    ManualApprovalBulkActionManager.SetJobFinished();
                    return;
                }

                Logger.Info($"Found {invalidTargets.Count} invalid containers to process.");

                int totalContainers = invalidTargets.Count;
                int processed = 0;
                var failedContainers = new List<string>();

                foreach (var target in invalidTargets)
                {
                    try
                    {
                        var result = await DeleteInvalidWaitingDisposalRecordsInCosmosAsync(
                            target.Id,
                            target.IsPhysicalRecord);

                        Logger.Info($"Cleaned container: {target.Id}, " +
                                    $"Submitted: {result.SubmittedCount}, " +
                                    $"Failed: {result.FailedRecords.Count}");
                    }
                    catch (Exception ex)
                    {
                        failedContainers.Add(target.Id);
                        Logger.Error($"Failed to clean container: {target.Id}, ERROR: {ex}");
                    }
                    finally
                    {
                        processed++;
                        ManualApprovalBulkActionManager.Increase();
                    }
                }

                ManualApprovalDataSyncManager.WaitComplete();
                var totalDeleted = _succeedRecords.Count;
                var totalFailed = _failedRecords.Count;

                string resultMessage = $"Total containers: {totalContainers}, " +
                                       $"Cleaned: {totalContainers - failedContainers.Count}, " +
                                       $"Failed: {failedContainers.Count + totalFailed}, " +
                                       $"Records deleted: {totalDeleted}";

                if (failedContainers.Any() || totalFailed > 0)
                {
                    ManualApprovalBulkActionManager.SetJobFailed("RM_SS_CommonErrorMessage");
                }
                else
                {
                    ManualApprovalBulkActionManager.SetJobFinished();
                }

                Logger.Info($"DeleteInvalidRecordsJob {_jobId} completed. {resultMessage}");
            }
            catch (Exception ex)
            {
                Logger.Error($"DeleteInvalidRecordsJob {_jobId} failed: {ex}");
                ManualApprovalBulkActionManager.SetJobFailed("RM_TS_SS_Summary");
                throw;
            }
            finally
            {
                ManualApprovalDataSyncManager.RegisteProcessItemCallback(null, null);
            }
        }

        private async Task<DeleteResult> DeleteInvalidWaitingDisposalRecordsInCosmosAsync(
            string containerId,
            bool isPhysicalRecord = false)
        {
            const int queryPageSize = 1000;
            const int commitBatchSize = 200;
            string continuationToken = null;
            var result = new DeleteResult();
            var validSourceFlags = new HashSet<int>
            {
                (int)SourceFlag.SharePoint,
                (int)SourceFlag.Exchange,
                (int)SourceFlag.OneDrive,
                (int)SourceFlag.Teams,
                (int)SourceFlag.Box
            };
            try
            {

                var container = await RMAzureCosmosDBContext.GetContainerAsync();

                var now = DateTime.UtcNow.Ticks;
                var physicalLocationId = isPhysicalRecord
                    ? Guid.Parse(containerId)
                    : Guid.Empty;

                do
                {
                    var queryResult = await container.UseLinqQuery().Where(item =>
                    (
                        (!isPhysicalRecord &&
                         item.ContainerId == containerId &&
                         validSourceFlags.Contains(item.SourceFlag))
                        ||
                        (isPhysicalRecord &&
                         item.SourceFlag == (int)SourceFlag.Physical &&
                         item.LocationId == physicalLocationId)
                    ) &&
                    item.NodeType != (int)NodeLevel.SiteCollection &&
                    item.RecordStatus != (int)RMRecordStatus.RMDeleted &&
                    item.RecordStatus != (int)RMRecordStatus.Destroyed &&

                            //  ManualApproval 
                            //(item.ManualApprovedStatus != (int)SOApproveDBStatus.None ||
                            // item.ManualInternalApprovedStatus != (int)SOApproveDBStatus.None ||
                            // item.ManualArchiveStatus != (int)AvePoint.RA.Contract.Schedule.ActionStatus.None)
                            // ||
                            // WaitDiposalQueryAsync
                            (
                                (
                                    (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved ||
                                     item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected ||
                                     item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.Approved ||
                                     item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.Rejected)
                                    && item.ManualExtendTime < now
                                )
                                //||
                                //(
                                //    // ExtendQueryAsync: 
                                //    (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved ||
                                //     item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected ||
                                //     item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove ||
                                //     item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.Approved ||
                                //     item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.Rejected ||
                                //     item.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove)
                                //    && item.ManualExtendTime >= now
                                //)
                            )
                        )
                        .AsResultSet()
                        .PaginateAsync(continuationToken, queryPageSize);

                    var currentPage = queryResult.Values.ToList();
                    continuationToken = queryResult.ContinuationToken;

                    if (!currentPage.Any()) break;

                    for (int i = 0; i < currentPage.Count; i += commitBatchSize)
                    {
                        var batch = currentPage.Skip(i).Take(commitBatchSize).ToList();

                        try
                        {
                            foreach (var record in batch)
                            {
                                _originalManualStatuses[record.Id] = GetManualStatus(record);
                                record.RemoveManualFields();
                                ManualApprovalDataSyncManager.Add(record);
                            }
                            result.SubmittedCount += batch.Count;
                            ManualApprovalDataSyncManager.Commit();
                            Logger.Debug($"Batch submitted {batch.Count} records for removal from waiting for disposal in container {containerId}");
                        }
                        catch (Exception ex)
                        {
                            foreach (var record in batch)
                            {
                                result.FailedRecords.Add((record, ex.Message));
                            }
                            Logger.Warn($"Batch submission failed for {batch.Count} records in container {containerId}: {ex.Message}");
                        }
                    }

                    Logger.Debug($"Container {containerId}: Deleted {result.DeletedCount}, Failed {result.FailedRecords.Count}");

                } while (!string.IsNullOrEmpty(continuationToken));
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to process container {containerId}, ERROR: {ex}");
                throw;
            }
            finally
            {
            }
        }

        private void BatchRecordSuccessDetails(List<Record> records)
        {
            try
            {
                foreach (var record in records)
                {
                    int status = GetOriginalManualStatus(record);

                    ManualApprovalBulkActionManager.AddSucceedJobDetail(
                        record,
                        status,
                        "DeleteInvalidRecords",
                        new string[] { "System" }
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to record success details: {ex.Message}");
            }
        }

        private void BatchRecordFailedDetails(List<(Record record, string error)> failedRecords)
        {
            try
            {
                foreach (var (record, error) in failedRecords)
                {
                    int status = GetOriginalManualStatus(record);

                    ManualApprovalBulkActionManager.AddFailedJobDetail(
                        record,
                        status,
                        new string[] { "System" },
                        "DeleteInvalidRecords",
                        error
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to record failure details: {ex.Message}");
            }
        }

        private Task ProcessItemSucceedAsync(Record record)
        {
            _succeedRecords.Add(record);
            BatchRecordSuccessDetails(new List<Record> { record });
            return Task.CompletedTask;
        }

        private void ProcessItemFailed(Record record, string error)
        {
            _failedRecords.Add((record, error));
            BatchRecordFailedDetails(
                new List<(Record record, string error)> { (record, error) });
        }

        private static int GetManualStatus(Record record)
        {
            return record.ManualApprovedStatus != (int)SOApproveDBStatus.None
                ? record.ManualApprovedStatus
                : record.ManualInternalApprovedStatus;
        }

        private int GetOriginalManualStatus(Record record)
        {
            return _originalManualStatuses.TryGetValue(record.Id, out var status)
                ? status
                : GetManualStatus(record);
        }

        private async Task<HashSet<string>> GetInvalidRemoteNodesContainerId()
        {
            HashSet<string> allContainerIds = new HashSet<string>(
                RMRemoteNodeDao.GetAllContainers().Select(c => c.Id),
                StringComparer.OrdinalIgnoreCase);

            HashSet<string> settingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var isTeamsSettingUpgradeCompleted = IsTeamsSettingUpgradeCompleted();
            var teamsContainerIds = isTeamsSettingUpgradeCompleted
                ? new HashSet<string>(RMRemoteNodeDao.GetAllTeamsContainerIds(), StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>();

            var sharePointSettingIds = SharePointSettingDao.GetAllGroupSettings()
                .Where(setting => !setting.IsRemoved)
                .Select(setting => setting.SiteGroupId.ToString());

            if (isTeamsSettingUpgradeCompleted && teamsContainerIds.Any())
            {
                sharePointSettingIds = sharePointSettingIds.Except(
                    teamsContainerIds,
                    StringComparer.OrdinalIgnoreCase);
            }
            settingIds.UnionWith(sharePointSettingIds);

            var oneDriveSettingIds = OneDriveSettingDao.GetAllGroupSettings()
                .Where(setting => !setting.IsRemoved)
                .Select(setting => setting.SiteGroupId.ToString());

            if (isTeamsSettingUpgradeCompleted && teamsContainerIds.Any())
            {
                oneDriveSettingIds = oneDriveSettingIds.Except(
                    teamsContainerIds,
                    StringComparer.OrdinalIgnoreCase);
            }
            settingIds.UnionWith(oneDriveSettingIds);

            if (isTeamsSettingUpgradeCompleted)
            {
                settingIds.UnionWith(TeamsSettingsDao.GetAllGroupSettings()
                    .Where(setting => !setting.IsRemoved && setting.TeamsGroupId != Guid.Empty)
                    .Select(setting => setting.TeamsGroupId.ToString()));
            }
            else
            {
                allContainerIds.ExceptWith(RMRemoteNodeDao.GetAllTeamsContainerIds());
                Logger.Info("Teams setting upgrade is not completed. Skip Teams container cleanup.");
            }

            allContainerIds.ExceptWith(settingIds);
            allContainerIds.Remove(RMConstants.DefaultPrivateChannelSitesGroupId);

            return allContainerIds;
        }

        private bool IsTeamsSettingUpgradeCompleted()
        {
            var upgradeSetting = KeyValueDao.GetValueByKey(KeyNameCollection.HasUpgradeTeamsSettings);
            return upgradeSetting != null &&
                   bool.TryParse(upgradeSetting.Value, out var isCompleted) &&
                   isCompleted;
        }

        private async Task<HashSet<string>> GetInvalidBoxConnectionIds()
        {
            var groupIds = new HashSet<Guid>(BoxConnectionGroupDao.GetAll().Select(g => g.Id));
            groupIds.ExceptWith(BoxSettingDao.LoadAllSetting().Select(setting => setting.ConnectionGroupId));
            var invalidConnectionIds = BoxConnectionDao.GetConnectionIdsByConnectionGroups(groupIds).Select(id => id.ToString()).ToHashSet();
            return invalidConnectionIds;
        }
        private async Task<HashSet<string>> GetInvalidExchangeOnlineGroupIds()
        {
            var allGroupIds = new HashSet<string>(
                RMMailboxDao.GetRemoteMailGroupNodes()
                    .Where(node => !string.IsNullOrWhiteSpace(node.NodeId))
                    .Select(node => node.NodeId),
                StringComparer.OrdinalIgnoreCase);

            var settingGroupIds = new HashSet<string>(
                EXOSettingDao.LoadAllGroupSettings()
                    .Where(setting => setting.GroupId != Guid.Empty)
                    .Select(setting => setting.GroupId.ToString()),
                StringComparer.OrdinalIgnoreCase);

            allGroupIds.ExceptWith(settingGroupIds);
            return allGroupIds;
        }

        private async Task<HashSet<string>> GetInvalidPhysicalRecordLocationIds()
        {
            var locations = RMLocationDao.GetAllLocations()
                .Where(location => !location.IsRemoved)
                .ToList();
            var locationById = locations.ToDictionary(location => location.Id);
            var settingLocationIds = new HashSet<Guid>(
                PhysicalRecordSettingDao.GetAllPhysicalRecordSettings()
                    .Where(setting => !setting.IsRemoved && setting.LocationUniqueId != Guid.Empty)
                    .Select(setting => setting.LocationUniqueId));
            var invalidLocationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var location in locations)
            {
                var pathIds = location.DirPath?
                    .Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                var topLevelLocationId = pathIds != null && pathIds.Length > 1
                    ? Convert.ToInt32(pathIds[1])
                    : location.Id;

                if (!locationById.TryGetValue(topLevelLocationId, out var topLevelLocation) ||
                    !settingLocationIds.Contains(topLevelLocation.UniqueId))
                {
                    invalidLocationIds.Add(location.UniqueId.ToString());
                }
            }

            return invalidLocationIds;
        }
    }

    public class DeleteResult
    {
        public int SubmittedCount { get; set; }
        public int DeletedCount { get; set; }
        public List<Record> DeletedRecords { get; set; } = new List<Record>();
        public List<(Record record, string error)> FailedRecords { get; set; } = new List<(Record record, string error)>();
    }
}
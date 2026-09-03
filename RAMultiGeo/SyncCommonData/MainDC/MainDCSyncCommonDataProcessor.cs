using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.MultiGeo;
using RAMultiGeo.Domain.Constants;
using RAMultiGeo.Helper;
using RAMultiGeo.SyncCommonData.MainDC.DataCenterSync;
using System.Text.Json;

namespace RAMultiGeo.SyncCommonData.MainDC
{
    public class MainDCSyncCommonDataProcessor
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MainDCSyncCommonDataProcessor));
        private string StorageConnectionString => RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private const string InitTenantFileName = "init-tenant-info";
        private const string NoInitTenantFileName = "sync-common-data-info";
        private const string SyncCommonDataContainerName = "sync-common-data";
        private string TemplateSQLiteFilePath;
        private string TemplateSQLiteFolderPath;
        private string JobId;
        private string Password;
        private IEnumerable<string> OnlySyncCommonDataDCs;
        private IEnumerable<string> SyncNeedInitDCs;
        private IEnumerable<string> SyncCommonDataFailedDCs;
        private IEnumerable<string> SyncAzureFailedDCs;
        private IEnumerable<DataCenterInfo> DataCenterInfos;

        public MainDCSyncCommonDataProcessor(string jobId)
        {
            JobId = jobId;
            TemplateSQLiteFolderPath = Path.Combine(Path.GetTempPath(), $"{JobId}");
            TemplateSQLiteFilePath = Path.Combine(TemplateSQLiteFolderPath, $"{JobId}_{DateTime.UtcNow:yyyyMMddHHmmss}.db");
        }

        public MainDCSyncCommonDataProcessor SetSyncDCs(IEnumerable<string> onlySyncCommonDataDCs, IEnumerable<string> syncNeedInitDCs, 
            IEnumerable<string> syncCommonDataFailedDCs, IEnumerable<string> syncCommonAzureFailedDCs, IEnumerable<DataCenterInfo> dataCenterInfos)
        {
            OnlySyncCommonDataDCs = onlySyncCommonDataDCs;
            SyncNeedInitDCs = syncNeedInitDCs;
            SyncCommonDataFailedDCs = syncCommonDataFailedDCs;
            SyncAzureFailedDCs = syncCommonAzureFailedDCs;
            DataCenterInfos = dataCenterInfos;
            return this;
        }

        public async Task RunAsync(long lastSyncTime)
        {
            try
            {
                var syncTable = MultiGeoCommonSyncTable.None;
                SyncDataFromSQLServerToSQLite converter = new SyncDataFromSQLServerToSQLite(TemplateSQLiteFilePath);
                if (OnlySyncCommonDataDCs.Any())
                {
                    ChangeLogReader changeLogReader = new ChangeLogReader();
                    changeLogReader.SetLastSyncTime(lastSyncTime);
                    syncTable = (MultiGeoCommonSyncTable)changeLogReader.GetAllTableNeedSync();
                    bool syncAzure = changeLogReader.GetSyncImageEmailTemplateFailed();
                    Logger.Info($"Syncing changed tables [{syncTable}] for no-need-init DCs.");

                    if(syncTable != MultiGeoCommonSyncTable.None)
                    {

                        Logger.Info("Start sync SQL to SQLite for only sync common data url");
                        await converter.SetNeedSyncTable(syncTable).StartSyncTable();
                        var onlySyncCommonDataUrl = await UploadSQLiteFile(false);
                        if (syncAzure)
                        {
                            var syncCommonImageInfos = GetEmailTemplateImagesForSyncCommonData();
                            if (syncCommonImageInfos != null && syncCommonImageInfos.Any())
                            {
                                Logger.Info("start sync email template images to Azure for only sync common data url");
                                await SendRequestSyncDataToOtherDCsAsync(onlySyncCommonDataUrl, OnlySyncCommonDataDCs, syncTable, syncCommonImageInfos);
                            }
                        }
                        else
                        {
                            await SendRequestSyncDataToOtherDCsAsync(onlySyncCommonDataUrl, OnlySyncCommonDataDCs, syncTable);
                        }

                    }
                }

                if(SyncNeedInitDCs.Any() || SyncCommonDataFailedDCs.Any() || SyncAzureFailedDCs.Any())
                {
                    var remainingSyncTable = MultiGeoConstants.AllSyncTable & ~syncTable;
                    if (remainingSyncTable != MultiGeoCommonSyncTable.None)
                    {
                        Logger.Info($"Syncing remaining tables [{remainingSyncTable}] for need-init DCs.");
                        await converter.SetNeedSyncTable(remainingSyncTable).StartSyncTable();
                    }
                    var initTenantDataUrl = await UploadSQLiteFile(true);
                    var syncCommonImageInfos = GetEmailTemplateImagesForSyncCommonData();
                    Logger.Info($"Send request sync data for need-init DCs.");
                    var hasSyncCommonImages = syncCommonImageInfos is { Count: > 0 };

                    if (hasSyncCommonImages)
                    {
                        Logger.Info("start sync email template images to Azure for need-init DCs");
                        await SendRequestSyncDataToOtherDCsAsync(initTenantDataUrl, SyncNeedInitDCs, MultiGeoCommonSyncTable.AllTable, syncCommonImageInfos);
                    }
                    else
                    {
                        await SendRequestSyncDataToOtherDCsAsync(initTenantDataUrl, SyncNeedInitDCs, MultiGeoCommonSyncTable.AllTable);
                    }

                    var azureFailedDCs = SyncAzureFailedDCs as IList<string> ?? SyncAzureFailedDCs.ToList();

                    if (azureFailedDCs.Count > 0)
                    {
                        Logger.Info("Send request sync data for Azure failed DCs.");
                        if (hasSyncCommonImages)
                        {
                            await SendRequestSyncDataToOtherDCsAsync(initTenantDataUrl, azureFailedDCs, syncTable, syncCommonImageInfos);
                        }
                        else
                        {
                            await SendRequestSyncDataToOtherDCsAsync(initTenantDataUrl, azureFailedDCs, syncTable);
                        }
                    }

                    var commonDataOnlyFailedDCs = new List<string>();
                    if (azureFailedDCs.Count == 0)
                    {
                        commonDataOnlyFailedDCs.AddRange(SyncCommonDataFailedDCs);
                    }
                    else
                    {
                        var azureFailedSet = new HashSet<string>(azureFailedDCs);
                        foreach (var dc in SyncCommonDataFailedDCs)
                        {
                            if (!azureFailedSet.Contains(dc))
                            {
                                commonDataOnlyFailedDCs.Add(dc);
                            }
                        }
                    }

                    if (commonDataOnlyFailedDCs.Count > 0)
                    {
                        Logger.Info("Send request sync data for common data failed DCs.");
                        await SendRequestSyncDataToOtherDCsAsync(initTenantDataUrl, commonDataOnlyFailedDCs, syncTable);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Run Main DC Sync Common Data failed. {e}");
            }
            finally
            {
                SQLiteHelper.Dispose();
                DeleteTemplateFile();
            }
        }

        private List<SyncCommonAzureInfoDto>? GetEmailTemplateImagesForSyncCommonData()
        {
            try
            {
                var emailTemplateImages = new List<SyncCommonAzureInfoDto>();
                var blobNames = RAStorageUtil.AllBlobNames(TenantLocalValue.LogonGroupId);
                return blobNames.Select(blobName => new SyncCommonAzureInfoDto
                {
                    BlobName = blobName,
                    SasUrl = RAStorageUtil.GetSasUriForImageBlob(blobName, TimeSpan.FromDays(6))
                }).ToList();
            } 
            catch(Exception ex)
            {   
                Logger.Error($"Get email template images for sync common data failed. {ex.Message}");
                MultiGeoReplicaFailureLogWriter.WriteForSync(MultiGeoOperationType.UploadImages.ToString(), TenantLocalValue.LogonGroupId);
                return null;
            }

        }

        private async Task SendRequestSyncDataToOtherDCsAsync(string sQLiteUrl, IEnumerable<string> DCs, MultiGeoCommonSyncTable syncTable, List<SyncCommonAzureInfoDto> syncCommonAzure = null)
        {
            Logger.Info($"Start to send request to other DCs to sync common data with sync table {syncTable}");
            var result = await RAMultiGeoClient.RouteApiActionAsync<SyncCommonDataInforDto, string>(MultiGeoOperationType.RunSyncCommonDataOtherDCJob, new SyncCommonDataInforDto
            {
                NeedUpdateTable = (long)syncTable,
                SQLiteDownloadUrl = sQLiteUrl,
                SyncCommonImages = syncCommonAzure
            }, DCs);
            foreach (var dc in DCs)
            {
                if(string.IsNullOrEmpty(result[dc]))
                {
                    Logger.Error($"Send request to {dc} to sync common data failed.");
                }
                else
                {
                    Logger.Info($"Send request to {dc} to sync common data, result: {result[dc]}");
                }
            }
        }

        private void DeleteTemplateFile()
        {
            try
            {
                if (Directory.Exists(TemplateSQLiteFolderPath))
                {
                    Directory.Delete(TemplateSQLiteFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Delete template file failed. {ex}");
            }
        }

        private async Task<string> UploadSQLiteFile(bool isInitTenant)
        {
            AzureBlobStorageManager azureStorageHelper = new AzureBlobStorageManager(StorageConnectionString, SyncCommonDataContainerName);
            return await azureStorageHelper.ZipAndUploadFolder(TemplateSQLiteFolderPath, Password, TenantLocalValue.LogonGroupId, isInitTenant ? InitTenantFileName : NoInitTenantFileName);
        }
    }
}

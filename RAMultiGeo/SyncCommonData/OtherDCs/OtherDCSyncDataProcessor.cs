using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Azure.Storage.Blobs;
using Dapper;
using RAMultiGeo.Helper;
using RAMultiGeo.Repositories;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RAMultiGeo.SyncCommonData.OtherDCs
{
    public class OtherDCSyncDataProcessor(string jobId, string downloadUrl, long syncTable, string syncImages)
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(OtherDCSyncDataProcessor));

        private readonly IJobMonitorService JobService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly IRMKeyValueDao KeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();
        private string JobId => jobId;
        private string ExtractFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateSyncSQLite", TenantLocalValue.LogonGroupId, $"{JobId}");
        private string DownloadUrl => downloadUrl;
        private long SyncTable = syncTable;
        private string SyncImages = syncImages;
        public async Task RunAsync()
        {
            SyncCommonDataJobDetailManager.Init(JobId, AvePoint.RA.Contract.JobMonitor.JobType.MultiGeoOtherDCSyncCommonData);
            try
            {
                Logger.Info($"Start to sync data for job {JobId}, sync table: {SyncTable}");
                await GetReSyncFailTable();
                SqlMapper.AddTypeHandler(new GuidTypeHandler());
                var sQLiteFilePath = await DownLoadSyncSQLiteFile();
                Logger.Info($"Download SQLite file to {sQLiteFilePath}");
                SyncDataFromSQLiteToSQLServer syncDataFromSQLiteToSQLServer = new SyncDataFromSQLiteToSQLServer(sQLiteFilePath, (MultiGeoCommonSyncTable)SyncTable);
                await syncDataFromSQLiteToSQLServer.StartSync();
                await SetFailTableValue(syncDataFromSQLiteToSQLServer.GetSyncFailedTable());
                if (!string.IsNullOrEmpty(SyncImages))
                {
                   Logger.Info($"Start to sync images for job {JobId}");
                   string syncImagesJson = Encoding.UTF8.GetString(Convert.FromBase64String(SyncImages));
                   var syncCommonImageInfoList = JsonSerializer.Deserialize<List<SyncCommonAzureInfoDto>>(syncImagesJson);
                   await SyncImagesAsync(syncCommonImageInfoList);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Sync data for job {JobId} failed. {ex}");
            }
            finally
            {
                SQLiteHelper.Dispose();
                DeleteTemplateFolder();
            }
            SyncCommonDataJobDetailManager.SetJobFinished();
        }

        private async Task SetFailTableValue(long syncFailedTable)
        {
            if (syncFailedTable == 0)
            {
                await TenantService.UpdateMultiGeoTenantInitStatus(AvePoint.RA.Contract.Aos.Notification.MultiGeoStatus.MultiGeoDC);
            }
            else
            {
                await TenantService.UpdateMultiGeoTenantInitStatus(AvePoint.RA.Contract.Aos.Notification.MultiGeoStatus.MulitGeoDCSyncFailed);
            }
            Logger.Info($"Set re-sync failed table to key value store, table bit value: {syncFailedTable}");
            await KeyValueDao.SaveOrUpdateAsync(new RMKeyValue
            {
                Key = KeyNameCollection.SyncFailedCommonTable,
                Value = syncFailedTable.ToString()
            });
        }

        private async Task GetReSyncFailTable()
        {
            try
            {
                var syncFailedTableValue = await KeyValueDao.GetValueByKeyAsync(KeyNameCollection.SyncFailedCommonTable);
                Logger.Info($"Get re-sync failed table from key value store, table bit value: {syncFailedTableValue}"); 
                if (!string.IsNullOrEmpty(syncFailedTableValue) && long.TryParse(syncFailedTableValue, out var syncFailedTable) && syncFailedTable > 0)
                {
                    SyncTable |= syncFailedTable;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Get re-sync failed table from key value store failed. {ex}");
            }
        }

        private void DeleteTemplateFolder()
        {
            try
            {
                if (Directory.Exists(ExtractFolderPath))
                {
                    Directory.Delete(ExtractFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Delete template file failed. {ex}");
            }
        }

        private async Task<string> DownLoadSyncSQLiteFile()
        {
            Logger.Info($"Start download the SQLite file.");
            AzureBlobStorageManager storageManager = new AzureBlobStorageManager(string.Empty, string.Empty);
            return await storageManager.DownloadAndExtractSQLiteFile(DownloadUrl, ExtractFolderPath);
        }

        private async Task SyncImagesAsync(List<SyncCommonAzureInfoDto>? files)
        {
            if (files == null || !files.Any())
                return;

            using var semaphore = new SemaphoreSlim(5);
            int errorCount = 0;
            Logger.Info($"Start syncing {files.Count} images to Azure Blob Storage.");
            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var sourceBlob = new BlobClient(new Uri(file.SasUrl));

                    var download = await sourceBlob.DownloadStreamingAsync();
                    using var reader = new StreamReader(download.Value.Content);
                    string base64Content = await reader.ReadToEndAsync();

                    var newBlobName = $"{file.BlobName}";
                    RAStorageUtil.UploadImage(
                       newBlobName,
                       base64Content);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to sync {file.BlobName}: {ex.Message}");
                    Interlocked.Increment(ref errorCount);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            if (errorCount > 0)
            {
                Logger.Error($"SyncImagesAsync completed with {errorCount}/{files.Count} image(s) failed.");
                await TenantService.UpdateMultiGeoTenantInitStatus(
                    AvePoint.RA.Contract.Aos.Notification.MultiGeoStatus.MultiGeoDCSyncAzureFailed);
                if(errorCount == files.Count)
                {
                    Logger.Error($"All images failed to sync. Setting status to MultiGeoDCSyncAzureFailed.");
                }
                else
                {
                    Logger.Error($"Partial image sync failure. {errorCount} out of {files.Count} images failed.");
                }
            }
            else
            {
                Logger.Info($"SyncImagesAsync completed successfully. {files.Count} image(s) synced.");
            }
        }

    }
}

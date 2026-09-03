using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RAMultiGeo.Helper;

namespace RAMultiGeo.SyncCommonData.MainDC
{
    public class MainDCSyncDataProcessor(string jobId)
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MainDCSyncDataProcessor));
        private readonly IJobMonitorService JobService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IMultiGeoDataCenterService multiGeoDataCenterService = PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        private readonly IRMKeyValueDao keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private string JobId = jobId;
        private const string LastSyncTimeKey = "MainDCSyncDataProcessor_LastSyncTime";
        private List<DataCenterInfo> dataCenterInfos;
        public async Task RunAsync()
        {
            SyncCommonDataJobDetailManager.Init(JobId, AvePoint.RA.Contract.JobMonitor.JobType.MultiGeoMainDCSyncCommonData);
            try
            {
                Logger.Info("MainDCSyncDataProcessor started.");
                dataCenterInfos = await multiGeoDataCenterService.GetDCsSupported();
                MainDCInitTenantInfoProcessor mainDCInitTenantInfoProcessor = new MainDCInitTenantInfoProcessor();
                await mainDCInitTenantInfoProcessor.SetSupportedDCs(dataCenterInfos).RunAsync();
                var (onlySyncCommonDataDCs, syncNeedInitDCs, syncCommonDataFailedDCs, syncCommonAzureFailedDCs) = mainDCInitTenantInfoProcessor.GetSyncDCs();
                Logger.Info($"Finish Init Tenant Info for Other DC {string.Join(",", syncNeedInitDCs)}");
                Logger.Info("Start Sync SQL data to SQLite Data");
                MainDCSyncCommonDataProcessor mainDCSyncCommonDataProcessor = new MainDCSyncCommonDataProcessor(JobId);
                await mainDCSyncCommonDataProcessor.SetSyncDCs(onlySyncCommonDataDCs, syncNeedInitDCs, syncCommonDataFailedDCs,
                    syncCommonAzureFailedDCs, dataCenterInfos).RunAsync(await GetLastSyncTime());
                await UpdateLastSyncTime(DateTime.UtcNow.Ticks);
                Logger.Info($"Finish sync SQL data to SQLite and send request for other DCs");
            }
            catch (Exception ex)
            {
                Logger.Error($"MainDCSyncDataProcessor failed. {ex}");
                JobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
            }
            SyncCommonDataJobDetailManager.SetJobFinished();
        }

        private async Task<long> GetLastSyncTime()
        {
            long lastSyncTime = 0;
            string lastSyncTimeStr = await keyValueDao.GetValueByKeyAsync(LastSyncTimeKey);
            long.TryParse(lastSyncTimeStr, out lastSyncTime);
            return lastSyncTime;
        }

        private async Task UpdateLastSyncTime(long lastSyncTime)
        {
            await keyValueDao.SaveOrUpdateAsync(new RMKeyValue
            {
                Key = LastSyncTimeKey,
                Value = lastSyncTime.ToString()
            });
        }
    }
}

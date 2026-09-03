using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.MultiGeo;
using Newtonsoft.Json;
using RAMultiGeo.Helper;

namespace RAMultiGeo.SyncCommonData.MainDC
{
    public class MainDCInitTenantInfoProcessor
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(MainDCInitTenantInfoProcessor));

        private IEnumerable<string> OnlySyncCommonDataDCs;
        private IEnumerable<string> SyncCommonDataFailedDCs;
        private IEnumerable<string> SyncNeedInitDCs;
        private IEnumerable<DataCenterInfo> SupportedDCs;
        private IEnumerable<string> SyncImageEmailTemplateFailedDCs;

        private List<string> InitDCSuccess = new List<string>();
        private readonly ITenantService tenantService = PlatformWindsorManager.GetService<ITenantService>();
        private readonly IRMKeyValueDao keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IUserService userService = PlatformWindsorManager.GetService<IUserService>();  

        public MainDCInitTenantInfoProcessor SetSupportedDCs(IEnumerable<DataCenterInfo> supportedDCs)
        {
            SupportedDCs = supportedDCs;
            return this;
        }

        public (IEnumerable<string>, IEnumerable<string>, IEnumerable<string>, IEnumerable<string>) GetSyncDCs()
        {
            return (OnlySyncCommonDataDCs, InitDCSuccess, SyncCommonDataFailedDCs, SyncImageEmailTemplateFailedDCs);
        }

        public async Task RunAsync()
        {
            try
            {
                Logger.Info($"Start Main DC Init Tenant Info.");
                await GetDCSyncInfoAsync();
                await SendRequestInitTenant();
                UpdateMainDCTenantInfo();
            }
            catch (Exception e)
            {
                Logger.Error($"Run Main DC Init Tenant Info failed. {e}");
                throw;
            }
        }

        private void UpdateMainDCTenantInfo()
        {
            Logger.Info($"Start to update status Geo for tenant {TenantLocalValue.LogonGroupId} in Main DC.");
            var tenantInfo = tenantService.GetTenantInfo(TenantLocalValue.LogonGroupId);
            if(tenantInfo == null)
            {
                Logger.Error($"Get tenant info null for tenant {TenantLocalValue.LogonGroupId} in Main DC.");
                return;
            }
            if(tenantInfo.MultiGeoStatus == (int)AvePoint.RA.Contract.Aos.Notification.MultiGeoStatus.MainDC)
            {
                Logger.Info($"Tenant {TenantLocalValue.LogonGroupId} has been updated to Main DC status, no need to update again.");
                return;
            }
            tenantService.UpdateMultiGeoStatus(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.Aos.Notification.MultiGeoStatus.MainDC);
        }

        private async Task SendRequestInitTenant()
        {
            var registerEmail = tenantService.GetRegisterEmailByTenantId(TenantLocalValue.LogonGroupId);
            if(string.IsNullOrEmpty(registerEmail))
            {
                Logger.Error($"Get register email null or empty for tenant {TenantLocalValue.LogonGroupId} in Main DC, cannot send request to init tenant info.");
                return;
            }
            Logger.Info($"Send request to init tenant info for tenant {TenantLocalValue.LogonGroupId} to DCs: {string.Join(",", SyncNeedInitDCs)}.");
            var tenantInfo = await GetTenantInfo(registerEmail);
            var dcInitStatuses = await RAMultiGeoClient.RouteApiActionAsync<InitMultiGeoTenantInfo ,bool>(MultiGeoOperationType.InitTenant, tenantInfo, SyncNeedInitDCs);
            foreach(var dcInitStatus in dcInitStatuses)
            {
                if (dcInitStatus.Value)
                {
                    Logger.Info($"Init tenant info successfully for DC {dcInitStatus.Key}.");
                    InitDCSuccess.Add(dcInitStatus.Key);
                }
                else
                {
                    Logger.Error($"Init tenant info failed for DC {dcInitStatus.Key}.");
                }
            }
        }

        private async Task<InitMultiGeoTenantInfo> GetTenantInfo(string registerEmail)
        {
            var supportedDCsValue = await keyValueDao.GetValueByKeyAsync(KeyNameCollection.JPMCMultiGEODC);
            var mainDCValue = await keyValueDao.GetValueByKeyAsync(KeyNameCollection.JPMCMultiGEOMainDC);
            var hasUpgradeTeamsValue = await keyValueDao.GetValueByKeyAsync(KeyNameCollection.HasUpgradeTeams);
            var enableTeamsFeatureValue = await keyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableTeamsFeature);
            var enableFolderPathValue = await keyValueDao.GetValueByKeyAsync(KeyNameCollection.EnableFolderPath);
            var listAdminAccount = await userService.GetApplicationAdminsAsync();
            return new InitMultiGeoTenantInfo
            {
                RegisterEmail = registerEmail,
                JPMCMultiGeoDC = JsonConvert.SerializeObject(supportedDCsValue),
                JPMCMultiGeoMainDC = mainDCValue,
                HasUpgradeTeams = hasUpgradeTeamsValue,
                EnableTeamsFeature = enableTeamsFeatureValue,
                AdminAccountInfo = listAdminAccount,
                EnableFolderPath = enableFolderPathValue,
            };
        }

        private async Task GetDCSyncInfoAsync()
        {
            var initTenantInfos = await RAMultiGeoClient.RouteApiActionAsync<int>(MultiGeoOperationType.IsInitTenant, SupportedDCs.Select(dc => dc.DCInternalName), false);
            OnlySyncCommonDataDCs = initTenantInfos.Where(kv => kv.Value == (int)MultiGeoStatus.MultiGeoDC || kv.Value == (int)MultiGeoStatus.MainDC || kv.Value == (int)MultiGeoStatus.Normal).Select(kv => kv.Key);
            SyncNeedInitDCs = initTenantInfos.Where(kv => kv.Value == (int)MultiGeoStatus.NotInit).Select(kv => kv.Key);
            SyncCommonDataFailedDCs = initTenantInfos.Where(kv => kv.Value == (int)MultiGeoStatus.MulitGeoDCSyncFailed).Select(kv => kv.Key);
            SyncImageEmailTemplateFailedDCs = initTenantInfos.Where(kv => kv.Value == (int)MultiGeoStatus.MultiGeoDCSyncAzureFailed).Select(kv => kv.Key);
            Logger.Info($"Only sync common data DCs: {string.Join(",", OnlySyncCommonDataDCs)}. ");
            Logger.Info($"DCs sync common data failed: {string.Join(",", SyncCommonDataFailedDCs)}. ");
            Logger.Info($"DCs need to init tenant info: {string.Join(",", SyncNeedInitDCs)}. ");
            Logger.Info($"DCs sync image email template data failed: {string.Join(",", SyncImageEmailTemplateFailedDCs)}. ");

        }
    }
}

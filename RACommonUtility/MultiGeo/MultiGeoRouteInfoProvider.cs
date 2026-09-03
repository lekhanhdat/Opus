using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.MultiGeo;

internal static class MultiGeoRouteInfoProvider
{
    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(MultiGeoRouteInfoProvider));
    private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
    private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
    private static IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
    private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
    private static string CurrentDCName => RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_DATA_CENTER];
    private static IReadOnlyDictionary<string, string> ConfiguredApiUrls => RMGlobalConfiguration.AppConfig.GetMultiGeoDCResourceApiUrl();
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(2);
    private const string IS_ENABLE_MULTI_GEO_FEATURE_KEY = "IsEnableMultiGeoFeature";
    private const string MULTI_GEO_MAIN_DC = "MultiGeoMainDC";
    private const string MULTI_GEO_SUPPORTED_DC = "MultiGeoSupportedDC";

    public static async Task<MultiGeoRouteInfo> CreateAsync()
    {
        var isEnableMultiGeo = await GetIsEnableMultiGeoFeatureAsync();
        if (!isEnableMultiGeo)
        {
            return new MultiGeoRouteInfo
            {
                IsEnableMultiGeoFeature = false,
            };
        }

        var configuredApiUrls = GetConfiguredApiUrls();
        if (configuredApiUrls.Count == 0)
        {
            return CreateRouteDisabledResult("No configured multi-geo api urls were found.");
        }

        var mainDC = await GetMainDataCenterAsync();
        if (string.IsNullOrEmpty(mainDC))
        {
            return CreateRouteDisabledResult("Main DC is not configured.");
        }

        var mainDCApi = GetConfiguredApiUrl(configuredApiUrls, mainDC);
        if (string.IsNullOrEmpty(mainDCApi))
        {
            return CreateRouteDisabledResult($"Main api is not configured for main DC [{mainDC}].", mainDC);
        }

        var supportedDCs = await GetSupportedDataCentersAsync();
        if (supportedDCs.Count == 0)
        {
            return CreateRouteDisabledResult("Supported DC list is empty.", mainDC, mainDCApi);
        }

        var routeApis = supportedDCs.Select(dc => dc.DCInternalName)
            .Where(dc => !string.IsNullOrWhiteSpace(dc) && !dc.Equals(mainDC, StringComparison.OrdinalIgnoreCase))
            .Select(dc => new MultiGeoApiTarget
            {
                DataCenter = dc,
                ApiUrl = GetConfiguredApiUrl(configuredApiUrls, dc),
            })
            .Where(target => !string.IsNullOrWhiteSpace(target.ApiUrl) && !target.ApiUrl.Equals(mainDCApi, StringComparison.OrdinalIgnoreCase))
            .GroupBy(target => target.ApiUrl, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        if (routeApis.Count == 0)
        {
            LogNoReplicaTargets(mainDC, mainDCApi, supportedDCs.Count);
        }

        return new MultiGeoRouteInfo
        {
            IsRoute = true,
            MainDataCenter = mainDC,
            MainApiUrl = mainDCApi,
            RouteApis = routeApis,
        };
    }

    public static async Task<MultiGeoRouteInfo> CreateMainDCAsync()
    {
        var isEnableMultiGeo = await GetIsEnableMultiGeoFeatureAsync();
        if (!isEnableMultiGeo)
        {
            return new MultiGeoRouteInfo
            {
                IsEnableMultiGeoFeature = false,
            };
        }

        var configuredApiUrls = GetConfiguredApiUrls();
        if (configuredApiUrls.Count == 0)
        {
            return CreateRouteDisabledResult("No configured multi-geo api urls were found.");
        }

        var mainDC = await GetMainDataCenterAsync();
        if (string.IsNullOrEmpty(mainDC))
        {
            return CreateRouteDisabledResult("Main DC is not configured.");
        }

        var mainDCApi = GetConfiguredApiUrl(configuredApiUrls, mainDC);
        if (string.IsNullOrEmpty(mainDCApi))
        {
            return CreateRouteDisabledResult($"Main api is not configured for main DC [{mainDC}].", mainDC);
        }

        var supportedDCs = await GetSupportedDataCentersAsync();
        if (supportedDCs.Count == 0)
        {
            return CreateRouteDisabledResult("Supported DC list is empty.", mainDC, mainDCApi);
        }

        List<MultiGeoApiTarget> routeApis = [
            new(){
                DataCenter = mainDC,
                ApiUrl = mainDCApi,
            }];

        if (routeApis.Count == 0)
        {
            LogNoReplicaTargets(mainDC, mainDCApi, supportedDCs.Count);
        }

        return new MultiGeoRouteInfo
        {
            IsRoute = true,
            MainDataCenter = mainDC,
            MainApiUrl = mainDCApi,
            RouteApis = routeApis,
        };
    }

    private static MultiGeoRouteInfo CreateRouteDisabledResult(string reason, string mainDC = null, string mainApi = null)
    {
        LogRouteDisabled(reason, mainDC, mainApi);
        return new MultiGeoRouteInfo();
    }

    private static void LogRouteDisabled(string reason, string mainDC, string mainApi)
    {
        s_logger.Info(
            $"Multi-geo routing is skipped. Reason: [{reason}] Current DC: [{CurrentDCName}], Main DC: [{mainDC}], Main Api: [{mainApi}].");
    }

    private static void LogNoReplicaTargets(string mainDC, string mainApi, int configuredSupportedDcCount)
    {
        s_logger.Info(
            $"Multi-geo routing has no replica targets after filtering. Current DC: [{CurrentDCName}], Main DC: [{mainDC}], Main Api: [{mainApi}], Configured Supported DC Count: [{configuredSupportedDcCount}].");
    }

    private static async Task<bool> GetIsEnableMultiGeoFeatureAsync()
    {
        try
        {
            if (await Cache.ExistAsync(IS_ENABLE_MULTI_GEO_FEATURE_KEY))
            {
                return await Cache.GetAsync<bool>(IS_ENABLE_MULTI_GEO_FEATURE_KEY);
            }

            var isEnableMultiGeo = await FunctionSettingDao.IsEnableMultiGeoFeature(KeyValueDao);
            await Cache.SetAsync(IS_ENABLE_MULTI_GEO_FEATURE_KEY, isEnableMultiGeo, s_cacheDuration);
            return isEnableMultiGeo;
        }
        catch (Exception e)
        {
            s_logger.Error($"Error occurred while getting multi-geo feature flag. Exception: {e}");
            return false;
        }
    }

    public static async Task<string> GetMainDataCenterAsync()
    {
        try
        {
            if (await Cache.ExistAsync(MULTI_GEO_MAIN_DC))
            {
                return await Cache.GetAsync<string>(MULTI_GEO_MAIN_DC) ?? string.Empty;
            }

            var mainDC = KeyValueDao.GetValueByKey(KeyNameCollection.JPMCMultiGEOMainDC)?.Value ?? string.Empty;
            await Cache.SetAsync(MULTI_GEO_MAIN_DC, mainDC, s_cacheDuration);
            return mainDC;
        }
        catch (Exception e)
        {
            s_logger.Error($"Error occurred while getting main data center. Exception: {e}");
            return string.Empty;
        }
    }

    private static async Task<List<DataCenterInfo>> GetSupportedDataCentersAsync()
    {
        try
        {
            if (await Cache.ExistAsync(MULTI_GEO_SUPPORTED_DC))
            {
                return await Cache.GetAsync<List<DataCenterInfo>>(MULTI_GEO_SUPPORTED_DC) ?? [];
            }

            var value = await MultiGeoDataCenterService.GetDCsSupported();
            await Cache.SetAsync(MULTI_GEO_SUPPORTED_DC, value, s_cacheDuration);
            return value;
        }
        catch (Exception e)
        {
            s_logger.Error($"Error occurred while getting supported data centers. Exception: {e}");
            return [];
        }
    }

    private static Dictionary<string, string> GetConfiguredApiUrls()
    {
        return ConfiguredApiUrls.ToDictionary(group => group.Key, group => group.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetConfiguredApiUrl(IReadOnlyDictionary<string, string> configuredApiUrls, string dataCenter)
    {
        if (string.IsNullOrWhiteSpace(dataCenter))
        {
            return null;
        }

        configuredApiUrls.TryGetValue(dataCenter, out var apiUrl);
        return apiUrl;
    }
}
import ExceptionHandler from "./ExceptionHandler";
import { CacheUtil } from "../Utils";

const CACHE_KEY_PREFIX = "configuration";

class ConfigurationRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getCostSavingConfigurationInfo = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("saving"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365ConfigurationApi/GetCostSavingInfo",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, {
            spFreeStorage: 1024,
            spStoragePrice: 0.2,
            archivedDataStoragePrice: 0.02
        });
    };

    static getCanAppendOpusContainer = () => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryOffice365ConfigurationApi/GetAppendAvailableOpusContainer",
                method: "GET",
            };
            return await fetchUtility(requestOption);
        }, []);
    };

    static saveAppendDiscoveryConfig = (specifyContainerIds) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryOffice365ConfigurationApi/AddOrUpdateAppendConfigurationInfo",
                method: "POST",
                data: specifyContainerIds
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_FA_Discovery_RunJobFailed,
        });
    };

    static retryFailedAnalysisJob = () => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: "/api/RMDiscoveryOffice365ConfigurationApi/AddOrUpdateRerunConfigurationInfo",
                method: "POST",
            };
            return await fetchUtility(requestOption);
        }, {
            MessageType: 1,
            ErrorMessage: RMResx.RM_FA_Discovery_RunJobFailed,
        });
    };
}

export default ConfigurationRequester;

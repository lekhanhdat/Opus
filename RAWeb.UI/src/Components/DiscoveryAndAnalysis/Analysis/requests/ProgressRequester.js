import { SourceFlag } from "../../../Common/Constants";
import { DataSizeType } from "../Constants";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "progress";

const currentDate = new Date();
const sixMonthsAgoDate = new Date();
sixMonthsAgoDate.setMonth(sixMonthsAgoDate.getMonth() - 6);

class ProgressRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getSummaryOptimizedInfo = (o365TenantId) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(o365TenantId),
                    async () => {
                        const requestOption = {
                            url: "/api/RMDiscoveryOffice365ProgressApi/GetSummaryOptimizedInfo",
                            method: "POST",
                            data: o365TenantId,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                return {
                    fileTotalSize: CalculateUtil.GetConvertValue(res.fileTotalSize),
                    fileSumCount: res.fileSumCount,
                    nextOptimizableFileTotalSize: CalculateUtil.GetConvertValue(res.nextOptimizableFileTotalSize),
                    nextOptimizableVersionTotalSize: CalculateUtil.GetConvertValue(res.nextOptimizableVersionTotalSize),
                    archived: CalculateUtil.GetConvertValue(res.archived),
                    deleted: CalculateUtil.GetConvertValue(res.deleted),
                };
            },
            {
                fileTotalSize: 0,
                fileSumCount: 0,
                nextOptimizableFileTotalSize: 0,
                nextOptimizableVersionTotalSize: 0,
                archived: 0,
                deleted: 0,
            }
        );
    };

    static getContainerOptimizedInfoesAsync = (paginateInfo) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    "container_" + HashCodeUtil.hash(paginateInfo),
                    async () => {
                        const requestOption = {
                            url: "/api/RMDiscoveryOffice365ProgressApi/GetContainerOptimizedInfoes",
                            method: "POST",
                            data: paginateInfo,
                        };
                        return await fetchUtility(requestOption);
                    }
                );

                for (const item of res.items) {
                    item.remaining = CalculateUtil.GetConvertValue(item.remaining);
                    item.archived = CalculateUtil.GetConvertValue(item.archived);
                    item.deleted = CalculateUtil.GetConvertValue(item.deleted);
                }

                return res;
            },
            {
                items: [],
                count: 0,
            }
        );
    };

    static getSiteOptimizedInfoesAsync = (paginateInfo) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    "site_" + HashCodeUtil.hash(paginateInfo),
                    async () => {
                        const requestOption = {
                            url: "/api/RMDiscoveryOffice365ProgressApi/GetSiteOptimizedInfoes",
                            method: "POST",
                            data: paginateInfo,
                        };
                        return await fetchUtility(requestOption);
                    }
                );

                for (const item of res.items) {
                    item.fileTotalSize = CalculateUtil.GetConvertValue(item.fileTotalSize);
                    item.nextOptimizableFileTotalSize = CalculateUtil.GetConvertValue(item.nextOptimizableFileTotalSize);
                    item.nextOptimizableVersionTotalSize = CalculateUtil.GetConvertValue(item.nextOptimizableVersionTotalSize);
                    item.archived = CalculateUtil.GetConvertValue(item.archived);
                    item.deleted = CalculateUtil.GetConvertValue(item.deleted);
                }

                return res;
            },
            {
                items: [],
                count: 0,
            }
        );
    };

    static getOptimizationPlanInfoes = (paginateInfo) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const requestOption = {
                    url: "/api/RMDiscoveryOffice365ProgressApi/GetOptimizationPlanInfoes",
                    method: "POST",
                    data: paginateInfo,
                };
                var res = await fetchUtility(requestOption);
                return res;
            },
            {
                items: [],
                count: 0,
            }
        );
    };

    static getOptimizationSettingDetail = (o365TenantId, settingId) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryOffice365ProgressApi/GetOptimizationSettingDetail?o365TenantId=${o365TenantId}&settingId=${settingId}`,
                method: "GET",
            };
            var res = await fetchUtility(requestOption);
            return res;
        }, {});
    };

    static requestCancelOptimizationJob = (o365TenantId, settingId) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryOffice365ProgressApi/RequestCancelOptimizationJob?o365TenantId=${o365TenantId}&settingId=${settingId}`,
                method: "GET",
            };
            var res = await fetchUtility(requestOption);
            return res;
        }, false);
    };

    static updateProjectionConfigurationInfo = (configurationInfo) => {
        return ExceptionHandler.handleAsync(async () => {
            const requestOption = {
                url: `/api/RMDiscoveryOffice365ProgressApi/UpdateProjectionConfigurationInfo`,
                method: "POST",
                data: configurationInfo,
            };
            var res = await fetchUtility(requestOption);
            return res;
        }, false);
    };

    static getProjectionConfigurationInfo = (o365TenantId) => {
        return ExceptionHandler.handleAsync(
            async () => {
                if (_.isNil(o365TenantId)) {
                    return null;
                }
                const requestOption = {
                    url: `/api/RMDiscoveryOffice365ProgressApi/GetProjectionConfigurationInfo?o365TenantId=${o365TenantId}`,
                    method: "GET",
                };
                var res = await fetchUtility(requestOption);
                return res;
            },
            {
                o365TenantId: null,
                latestYear: currentDate.getFullYear(),
                latestMonth: currentDate.getMonth() + 1,
                latestStorageSize: 0,
                oldestYear: sixMonthsAgoDate.getFullYear(),
                oldestMonth: sixMonthsAgoDate.getMonth() + 1,
                oldestStorageSize: 0,
                realityMonthlyGrowthRate: 0,
                monthlyGrowthRate: 0,
                odLatestYear: currentDate.getFullYear(),
                odLatestMonth: currentDate.getMonth() + 1,
                odLatestStorageSize: 0,
                odOldestYear: sixMonthsAgoDate.getFullYear(),
                odOldestMonth: sixMonthsAgoDate.getMonth() + 1,
                odOldestStorageSize: 0,
                odRealityMonthlyGrowthRate: 0,
                odMonthlyGrowthRate: 0,
                realityDailyOptimizationSpeed: 0,
                dailyOptimizationSpeed: 0,
                dataSizeUnitType: DataSizeType.TB,
                contentSource: SourceFlag.SharePoint,
            }
        );
    };
}

export default ProgressRequester;

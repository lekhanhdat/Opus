import { CacheUtil, CalculateUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import { DiscoverySizeRangeQueryMode, WithoutInDateUnitConstants } from "../Constants";

const CACHE_KEY_PREFIX = "basic";

class BasicDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getO365TenantInfoes = () => {
        return ExceptionHandler.handleAsync(async () => {
            return await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("o365Tenant"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetO365TenantInfoes",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
        }, []);
    };

    static getWithoutInDateList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("withoutInDate"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetWithoutInDateList",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            const dateList = [
                {
                    id: -1,
                    name: RMResx.RM_FA_Inactive_ModifiedOption_Latest,
                },
            ];
            for (const dateInfo of res) {
                dateList.push({
                    id: dateInfo.id,
                    name: `${
                        dateInfo.unit
                    } ${dateInfo.unit <= 1 ? RMResx.RM_JS_RDM_CreateRule_Unit_Month : WithoutInDateUnitConstants.i18n.get(
                        dateInfo.unitType
                    )}`,
                    unit: dateInfo.unit,
                    unitType: dateInfo.unitType
                });
            }
            dateList.push({
                id: 999,
                name: RMResx.RM_FA_Inactive_ModifiedOption_Max,
            });

            return dateList;
        }, []);
    };

    static getInactiveTableColumns = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("inactive_table_columns"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetInactiveTableColumns",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };

    static getSizeRangeList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("size_range_list"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetSizeRangeList",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            if (res.length > 0) {
                res[0].queryMode = DiscoverySizeRangeQueryMode.LessThanEqual;
                for (let i = 1; i < res.length; i++) {
                    res[i].queryMode = DiscoverySizeRangeQueryMode.GenerateThanEqual;
                }
            }
            return res;
        }, []);
    };

    static getFileExtensions = (o365TenantId) => {
        return ExceptionHandler.handleAsync(async () => {
            if (_.isNil(o365TenantId)) {
                return [];
            }

            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extension_" + o365TenantId),
                async () => {
                    const requestOption = {
                        url:
                            "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetFileExtensions?o365TenantId=" +
                            o365TenantId,
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };

    static getRotRuleInfoes = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("rot_rule_infoes"),
                async () => {
                    const requestOption = {
                        url:
                            "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetRotRuleInfoes",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };

    static getSummaryStatisticalDataInfo = (o365TenantId) => {
        return ExceptionHandler.handleAsync(
            async () => {
                if (_.isNil(o365TenantId)) {
                    return {
                        duplicateFileTotalSize: -1,
                        fileTotalSize: 0,
                        fileSumCount: 0,
                        maxFileAge: 0,
                        totalVersionSize: 0,
                        phlVolume: 0,
                    };
                }

                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(`summary_${o365TenantId}`),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365BasicInfoQueryApi/GetSummaryStatisticalDataInfo?o365TenantId=${o365TenantId}`,
                            method: "GET",
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                return {
                    fileTotalSize: CalculateUtil.GetConvertValue(res.fileTotalSize),
                    fileSumCount: res.fileSumCount,
                    maxFileAge: res.maxFileAge,
                    totalVersionSize: CalculateUtil.GetConvertValue(res.totalVersionSize),
                    phlVolume: CalculateUtil.GetConvertValue(res.phlVolume),
                    duplicateFileTotalSize: res.duplicateFileTotalSize <= -1 ? -1 : res.duplicateFileTotalSize ? CalculateUtil.GetConvertValue(res.duplicateFileTotalSize) : 0
                };
            },
            {
                fileTotalSize: 0,
                fileSumCount: 0,
                maxFileAge: 0,
                totalVersionSize: 0,
                phlVolume: 0,
                duplicateFileTotalSize: -1
            }
        );
    };

    static queryRuleInfos = (o365TenantId) =>{
        if (_.isNil(o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("filter_rule_Infos"),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365BasicInfoQueryApi/GetRotRuleDataInfo`,
                        method: "Get",
                    };
                    return await fetchUtility(requestOption);
                }
            );

            return res;
        }, {});
    };

    static getRotEnable = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("rot_enable"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryOffice365BasicInfoQueryApi/GetRotEnable",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, false);
    };

}

export default BasicDataRequester;

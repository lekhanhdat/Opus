import _ from "lodash";
import { CacheUtil, CalculateUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";
import {
    DiscoverySizeRangeQueryMode,
    WithoutInDateUnitConstants,
} from "../../Constants";

const CACHE_KEY_PREFIX = "basic_file_system";

class BasicDataRequesterForFileSystem {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getWithoutInDateList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("withoutInDate"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryFSBasicInfoQueryApi/GetWithoutInDateList",
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
                    name: `${dateInfo.unit} ${
                        dateInfo.unit <= 1
                            ? RMResx.RM_JS_RDM_CreateRule_Unit_Month
                            : WithoutInDateUnitConstants.i18n.get(
                                  dateInfo.unitType
                              )
                    }`,
                    unit: dateInfo.unit,
                    unitType: dateInfo.unitType,
                });
            }
            dateList.push({
                id: 999,
                name: RMResx.RM_FA_Inactive_ModifiedOption_Max,
            });

            return dateList;
        }, []);
    };

    static getSummaryStatisticalDataInfo = () => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(`summary_file_system`),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryFSBasicInfoQueryApi/GetSummaryStatisticalDataInfo`,
                            method: "GET",
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                return {
                    fileTotalSize: CalculateUtil.GetConvertValue(res.fileTotalSize),
                    fileSumCount: res.fileSumCount,
                    maxFileAge: res.maxFileAge,
                };
            },
            {
                fileTotalSize: 0,
                fileSumCount: 0,
                maxFileAge: 0,
            }
        );
    };

    static getInactiveTableColumns = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("inactive_table_columns"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryFSBasicInfoQueryApi/GetInactiveTableColumns",
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
                        url: "/api/RMDiscoveryFSBasicInfoQueryApi/GetSizeRangeList",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            if (res.length > 0) {
                res[0].queryMode = DiscoverySizeRangeQueryMode.LessThanEqual;
                for (let i = 1; i < res.length; i++) {
                    res[i].queryMode =
                        DiscoverySizeRangeQueryMode.GenerateThanEqual;
                }
            }
            return res;
        }, []);
    };

    static getFileExtensions = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extension_file_system"),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryFSBasicInfoQueryApi/GetFileExtensions`,
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };

    // ROT
    static getRotEnable = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("rot_enable"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryFSBasicInfoQueryApi/GetRotEnable",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res; // Boolean
        }, false);
    };

    static getRotRuleInfoList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("rot_rule_info"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryFSBasicInfoQueryApi/GetRotRuleInfoes",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };
}

export default BasicDataRequesterForFileSystem;

import _ from "lodash";
import { CacheUtil, CalculateUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";
import { DiscoverySizeRangeQueryMode, WithoutInDateUnitConstants } from "../../Constants";

const CACHE_KEY_PREFIX = "basic_google_drive";

class BasicDataRequesterForGoogleDrive {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getOrganizationInfoes = () => {
        return ExceptionHandler.handleAsync(async () => {
            return await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey(""),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetOrganizationInfoes",
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
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetWithoutInDateList",
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

    static getSummaryStatisticalDataInfo = (organizationId) => {
        return ExceptionHandler.handleAsync(
            async () => {
                if (_.isNil(organizationId)) {
                    return {
                        fileTotalSize: 0,
                        fileSumCount: 0,
                        maxFileAge: 0,
                    };
                }

                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(`summary_${organizationId}`),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleBasicInfoQueryApi/GetSummaryStatisticalDataInfo?organizationId=${organizationId}`,
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
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetInactiveTableColumns",
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
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetSizeRangeList",
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

    static getFileExtensions = (organizationId) => {
        return ExceptionHandler.handleAsync(async () => {
            if (_.isNil(organizationId)) {
                return [];
            }

            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extension_" + organizationId),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleBasicInfoQueryApi/GetFileExtensions?organizationId=${organizationId}`,
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
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetRotEnable",
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
                        url: "/api/RMDiscoveryGoogleBasicInfoQueryApi/GetRotRuleInfoes",
                        method: "GET",
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, []);
    };
}

export default BasicDataRequesterForGoogleDrive;

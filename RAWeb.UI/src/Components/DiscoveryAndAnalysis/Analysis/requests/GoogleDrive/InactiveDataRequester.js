import _ from "lodash";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../../Utils";
import { DiscoveryNodeViewMode } from "../../Constants";
import ExceptionHandler from "../ExceptionHandler";

const CACHE_KEY_PREFIX = "google_drive_inactive";

class InactiveDataRequesterForGoogleDrive {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {
                fileTotalSize: 0,
                fileSumCount: 0,
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(
                        "aggregate_info" + HashCodeUtil.hash(queryParameter)
                    ),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleDataQueryApi/QueryInactiveAggregateInfo`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                res.fileTotalSize = CalculateUtil.GetConvertValue(res.fileTotalSize);
                return res;
            },
            {
                fileTotalSize: 0,
                fileSumCount: 0,
            }
        );
    };

    static querySummaryNodesData = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {
                count: 0,
                items: []
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("summary_node_" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleDataQueryApi/QueryInactiveSummaryNodes`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                return res;
            },
            {
                count: 0,
                items: []
            }
        );
    };

    static querySummaryNodeTotalAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {};
        }

        const clonedQueryParamter = _.cloneDeep(queryParameter);
        const viewMode = clonedQueryParamter.nodeQueryParameter.viewMode;
        delete clonedQueryParamter["nodeQueryParameter"];
        clonedQueryParamter.nodeQueryParameter = {
            viewMode: viewMode === DiscoveryNodeViewMode.SiteInContainer ? DiscoveryNodeViewMode.Site : viewMode
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("summary_node_total_aggregate_info" + HashCodeUtil.hash(clonedQueryParamter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleDataQueryApi/QueryInactiveSummaryNodeTotalAggregateInfo`,
                            method: "POST",
                            data: clonedQueryParamter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                return res;
            },
            {}
        );
    };

    static querySizeRanges = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("size_range_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleDataQueryApi/QueryInactiveSizeRanges`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );
            for (const sizeRangeInfo of res) {
                sizeRangeInfo.fileTotalSize = CalculateUtil.GetConvertValue(sizeRangeInfo.fileTotalSize);
            }
            return res;
        }, []);
    };

    static queryFileExtensions = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extensions_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleDataQueryApi/QueryInactiveFileExtensions`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );
            for (const fileExtensionInfo of res) {
                fileExtensionInfo.fileTotalSize = CalculateUtil.GetConvertValue(fileExtensionInfo.fileTotalSize);
            }
            return res;
        }, []);
    };
}

export default InactiveDataRequesterForGoogleDrive;

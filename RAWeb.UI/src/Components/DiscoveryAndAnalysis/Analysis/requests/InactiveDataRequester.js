import { DiscoveryNodeViewMode } from "../Constants";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "inactive";

class InactiveDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static querySizeRanges = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("size_range_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveSizeRanges`,
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
        if (_.isNil(queryParameter.o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extensions_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveFileExtensions`,
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

    static queryAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
            return {
                fileTotalSize: 0,
                fileSumCount: 0,
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_info" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveAggregateInfo`,
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
        if (_.isNil(queryParameter.o365TenantId)) {
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveSummaryNodesData`,
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

    static querySummaryNodesDataV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
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
                            url: `/api/RMDiscoveryOffice365DataQueryApiV3/QueryInactiveSummaryNodesData`,
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
        if (_.isNil(queryParameter.o365TenantId)) {
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveSummaryNodeTotalAggregateInfo`,
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

    static querySummaryNodeTotalAggregateInfoV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
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
                            url: `/api/RMDiscoveryOffice365DataQueryApiV3/QueryInactiveSummaryNodeTotalAggregateInfo`,
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

    static queryOptimizationNodeTotalAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
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
                    this.generateCacheKey("optimization_node_total_aggregate_info" + HashCodeUtil.hash(clonedQueryParamter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveOptimizationNodeTotalAggregateInfo`,
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

    static queryOptimizationNodesData = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
            return {
                count: 0,
                items: []
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("optimization_node_" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryInactiveOptimizationNodesData`,
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
    }

}

export default InactiveDataRequester;

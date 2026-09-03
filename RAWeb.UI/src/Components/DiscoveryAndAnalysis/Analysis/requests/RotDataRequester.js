import { DiscoveryNodeViewMode } from "../Constants";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "rot";

class RotDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryTreeRuleInfos = (queryParameter) =>{
        if (_.isNil(queryParameter.o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("tree_rule_Infos_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryTreeRotRuleInfo`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );

            return res;
        }, {});
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
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryROTFileExtensions`,
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
                siteCount: 0
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_info" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryROTTotalData`,
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
                siteCount: 0,
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotSummaryNodes`,
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotOptmizationNodes`,
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotSummaryNodeTotalAggregateInfo`,
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
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotOptimizationNodeTotalAggregateInfo`,
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

    static queryFileExtensionsV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extensionsV3_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotV3FileExtensionData`,
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
                    this.generateCacheKey("summaryV3_node_" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotV3SummaryNodeData`,
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
                    this.generateCacheKey("summaryV3_node_total_aggregate_info" + HashCodeUtil.hash(clonedQueryParamter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotV3SummaryNodeTotalAggregateInfoData`,
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

    static queryTreeRuleInfosV3 = (queryParameter) =>{
        if (_.isNil(queryParameter.o365TenantId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("tree_rule_InfosV3_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotV3RuleInfoOfTree`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );

            return res;
        }, {});
    };

    static queryAggregateInfoV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId)) {
            return {
                fileTotalSize: 0,
                fileSumCount: 0,
                siteCount: 0
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_infoV3" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365DataQueryApi/QueryRotV3AggregateInfo`,
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
                siteCount: 0,
            }
        );
    };
}

export default RotDataRequester;
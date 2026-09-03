import { DiscoveryNodeViewMode } from "../Constants";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "profile_inactive";

class ProfileInactiveDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId) || 
        queryParameter.o365TenantId === "00000000-0000-0000-0000-000000000000" || 
        _.isNil(queryParameter.profileId) || 
        queryParameter.profileId === "00000000-0000-0000-0000-000000000000" || 
        _.isNil(queryParameter.nodeQueryParameter)) {
            return {
                fileTotalSize: 0,
                optimizableFileTotalSize: 0,
                optimizableFileSumCount: 0,
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_info" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApi/QueryInactiveAggregateInfo`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                res.fileTotalSize = CalculateUtil.GetConvertValue(res.fileTotalSize);
                res.optimizableFileTotalSize = CalculateUtil.GetConvertValue(res.optimizableFileTotalSize);
                return res;
            },
            {
                fileTotalSize: 0,
                optimizableFileTotalSize: 0,
                optimizableFileSumCount: 0,
            }
        );
    };

    static queryOptimizationNodeTotalAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId) || _.isNil(queryParameter.profileId)) {
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
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApi/QueryInactiveOptimizationNodeTotalAggregateInfo`,
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

    static queryOptimizationNodeTotalAggregateInfoV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId) || _.isNil(queryParameter.profileId)) {
            return {};
        }

        const clonedQueryParamter = _.cloneDeep(queryParameter);
        const viewMode = clonedQueryParamter.nodeQueryParameter.viewMode;
        const joinedContainerId = clonedQueryParamter.nodeQueryParameter.joinedContainerId;
        delete clonedQueryParamter["nodeQueryParameter"];
        clonedQueryParamter.nodeQueryParameter = {
            viewMode,
            joinedContainerId
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("optimization_node_total_aggregate_info" + HashCodeUtil.hash(clonedQueryParamter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApiV3/QueryInactiveOptimizationNodeTotalAggregateInfo`,
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
        if (_.isNil(queryParameter.o365TenantId) || _.isNil(queryParameter.profileId)) {
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
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApi/QueryInactiveOptimizationNodesData`,
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

    static queryOptimizationNodesDataV3 = (queryParameter) => {
        if (_.isNil(queryParameter.o365TenantId) || _.isNil(queryParameter.profileId)) {
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
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApiV3/QueryInactiveOptimizationNodesData`,
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

export default ProfileInactiveDataRequester;
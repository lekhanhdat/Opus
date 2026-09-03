import { DiscoveryNodeViewMode } from "../Constants";
import { CacheUtil, HashCodeUtil, UnitConvertsionUtil } from "../Utils";
import ExceptionHandler from "./ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "profile_inactive";

class ProfileRotDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryOptimizationNodeTotalAggregateInfo = (queryParameter) => {
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
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApi/QueryRotOptimizationNodeTotalAggregateInfo`,
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
                            url: `/api/RMDiscoveryOffice365ProfileDataQueryApi/QueryRotOptimizationNodesData`,
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

export default ProfileRotDataRequester;
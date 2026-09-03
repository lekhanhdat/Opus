import { DiscoveryNodeViewMode } from "../../Constants";
import { CacheUtil, HashCodeUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "profile_inactive";

class ProfileRotDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryOptimizationNodeTotalAggregateInfo = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId) || _.isNil(queryParameter.profileId)) {
            return {};
        }

        const clonedQueryParameter = _.cloneDeep(queryParameter);
        const viewMode = clonedQueryParameter.nodeQueryParameter.viewMode;
        delete clonedQueryParameter["nodeQueryParameter"];
        clonedQueryParameter.nodeQueryParameter = {
            viewMode: viewMode === DiscoveryNodeViewMode.SiteInContainer ? DiscoveryNodeViewMode.Site : viewMode
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("optimization_node_total_aggregate_info" + HashCodeUtil.hash(clonedQueryParameter)),
                    async () => {
                        const requestOption = {
                            url: "/api/RMDiscoveryGoogleProfileDataQueryApi/queryRotOptimizationNodeTotalAggregateInfo",
                            method: "POST",
                            data: clonedQueryParameter,
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
        if (_.isNil(queryParameter.organizationId) || _.isNil(queryParameter.profileId)) {
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
                            url: "/api/RMDiscoveryGoogleProfileDataQueryApi/queryRotOptimizationNodesData",
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
import { DiscoveryNodeViewMode } from "../../Constants";
import { CacheUtil, CalculateUtil, HashCodeUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";

const CACHE_KEY_PREFIX = "google_drive_rot";

class RotDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static querySummaryNodesDataV3 = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {
                count: 0,
                items: [],
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(
                        "summaryV3_node_" + HashCodeUtil.hash(queryParameter)
                    ),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleDataQueryApi/QueryRotSummaryNodeData`,
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
                items: [],
            }
        );
    };

    static querySummaryNodeTotalAggregateInfoV3 = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {};
        }

        const clonedQueryParamter = _.cloneDeep(queryParameter);
        const viewMode = clonedQueryParamter.nodeQueryParameter.viewMode;
        delete clonedQueryParamter["nodeQueryParameter"];
        clonedQueryParamter.nodeQueryParameter = {
            viewMode:
                viewMode === DiscoveryNodeViewMode.SiteInContainer
                    ? DiscoveryNodeViewMode.Site
                    : viewMode,
        };

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey(
                    "summaryV3_node_total_aggregate_info" +
                        HashCodeUtil.hash(clonedQueryParamter)
                ),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleDataQueryApi/QueryRotSummaryNodeTotalAggregateInfoData`,
                        method: "POST",
                        data: clonedQueryParamter,
                    };
                    return await fetchUtility(requestOption);
                }
            );
            return res;
        }, {});
    };

    static queryTreeRuleInfosV3 = (queryParameter) =>{
        if (_.isNil(queryParameter.organizationId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("tree_rule_InfosV3_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleDataQueryApi/QueryRotRuleInfoOfTree`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );

            const result = res;
            return { ...result, children: result.children.sort((a, b) => a.category - b.category) }; // Sort by category to display ROT order
        }, {});
    };

    static queryAggregateInfoV3 = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return {
                fileTotalSize: 0,
                fileSumCount: 0,
                driveCount: 0
            };
        }

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_infoV3" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoveryGoogleDataQueryApi/QueryRotAggregateInfo`,
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
                driveCount: 0,
            }
        );
    };

    static queryFileExtensionsV3 = (queryParameter) => {
        if (_.isNil(queryParameter.organizationId)) {
            return [];
        }

        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extensionsV3_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoveryGoogleDataQueryApi/QueryRotFileExtensionData`,
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

export default RotDataRequester;

import { DataSizeType } from "../../Constants";
import { CacheUtil, HashCodeUtil, NumberUtil, UnitConvertsionUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";
import _ from "lodash";

const CACHE_KEY_PREFIX = "inactive";

class SFInactiveDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static queryAggregateInfo = (queryParameter) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("aggregate_info_sf" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/QueryInactiveAggregateInfo`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                res.fileTotalSize = UnitConvertsionUtil.DecimalConvert(res.FileTotalSize, 2);
                res.dataTotalSize = UnitConvertsionUtil.DecimalConvert(res.DataTotalSize, 2);
                res.recordsTotalCount = res.RecordsTotalCount;
                return res;
            },
            {
                fileTotalSize: 0,
                dataTotalSize: 0,
                recordsTotalCount:0
                
            }
        );
    };


    static queryFileExtensions = (queryParameter) => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("file_extensions_sf" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoverySalesforceDataQueryApi/QueryInactiveFileExtensions`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );
            const fileTypes = _.cloneDeep(res);
            const unit = UnitConvertsionUtil.GetUnitForChart(_.max(fileTypes.map((item) => item.fileTotalSize)));
            for (const fileExtensionInfo of fileTypes) {
                fileExtensionInfo.fileTotalSize = UnitConvertsionUtil.DecimalConvert(fileExtensionInfo.fileTotalSize, 2, unit);
            }
            return { fileTypes: fileTypes, unit };
        }, { fileTypes: [], unit: DataSizeType.MB });
    };

    static queryAnalysis = (queryParameter) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("analysis_sf_" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/QueryAnalysis`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                if (res?.items?.length > 0) {
                    res?.items?.forEach(item => {
                        item.inactiveTotalSize = UnitConvertsionUtil.DecimalConvert(item.inactiveTotalSize, 2);
                        item.totalSize = UnitConvertsionUtil.DecimalConvert(item.totalSize, 2);
                        item.inactiveSumCount = NumberUtil.internationalCountingSF(item.inactiveSumCount);
                        item.totalItemCount = NumberUtil.internationalCountingSF(item.totalItemCount);
                        item.inactiveCountOfTotal = `${Number(item.inactiveCountOfTotal || 0).toFixed(0)}%`;
                        item.inactiveSizeOfTotal = `${Number(item.inactiveSizeOfTotal || 0).toFixed(0)}%`;
                    });
                }
                return res;
            },
            {
                count: 0,
                items: []
            }
        );
    };
    static querySummaryNodeTotalAggregateInfo = (queryParameter) => {

        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("summary_node_total_aggregate_info_sf" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/QueryInactiveSummaryObjectTotalInfo`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                res.totalSize = UnitConvertsionUtil.DecimalConvert(res?.totalSize, 2);
                res.inactiveTotalSize = UnitConvertsionUtil.DecimalConvert(res?.inactiveTotalSize, 2);
                res.inactiveCountOfTotal = `${Number(res?.inactiveCountOfTotal || 0).toFixed(0)}%`;
                res.inactiveSizeOfTotal = `${Number(res?.inactiveSizeOfTotal || 0).toFixed(0)}%`;
                res.inactiveSumCount = NumberUtil.internationalCountingSF(res.inactiveSumCount);
                res.totalItemCount = NumberUtil.internationalCountingSF(res.totalItemCount);
                return res;
            },
            {
                inactiveTotalSize: 0,
                totalSize: 0,
                inactiveSumCount: 0,
                totalItemCount: 0,
                inactiveSizeOfTotal: 0,
                inactiveCountOfTotal: 0,
            }
        );
    };
    static querySizeRanges = (queryParameter) => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("size_range_sf_" + HashCodeUtil.hash(queryParameter)),
                async () => {
                    const requestOption = {
                        url: `/api/RMDiscoverySalesforceDataQueryApi/QueryInactiveSizeRanges`,
                        method: "POST",
                        data: queryParameter,
                    };
                    return await fetchUtility(requestOption);
                }
            );
            const items = _.cloneDeep(res);
            const unit = UnitConvertsionUtil.GetUnitForChart(_.max(items.map((item) => item.fileTotalSize)));

            for (const sizeRangeInfo of items) {
                sizeRangeInfo.fileTotalSize = UnitConvertsionUtil.DecimalConvert(sizeRangeInfo.fileTotalSize, 2, unit);
            }
            return { sizeRanges: items, unit };
        }, { sizeRanges: [], unit: DataSizeType.MB });
    };

    static queryFigureDataInfo = (queryParameter) => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("figure_data" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/QueryFigureDataInfo`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                let items = _.cloneDeep(res);
                // only take data for 30 years ago plus this year and the predicted next year
                items = items?.slice(-32);
                let unit = DataSizeType.MB;
                if (items?.length > 0) {
                    unit = UnitConvertsionUtil.GetUnitForChart(_.max(items.map((item) => item.TotalStorageUsed)));
                    items.forEach(item => {
                        item.TotalStorageUsed = UnitConvertsionUtil.DecimalConvert(item.TotalStorageUsed, 2, unit);
                    });
                }

                return { items, unit };
            },
            {
                items: [],
                unit: DataSizeType.MB
            }
        );
    };
}

export default SFInactiveDataRequester;

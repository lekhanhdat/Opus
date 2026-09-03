import { CacheUtil, HashCodeUtil } from "../../Utils";
import ExceptionHandler from "../ExceptionHandler";
import { SFObjectType, SFObjectTypeI18ns, WithoutInDateUnitConstants } from "../../Constants";

const CACHE_KEY_PREFIX = "basic_sf";

class SFBasicDataRequester {
    static generateCacheKey = (cacheKey) => {
        return CACHE_KEY_PREFIX + "_" + cacheKey;
    };

    static getWithoutInDateList = () => {
        return ExceptionHandler.handleAsync(async () => {
            const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                this.generateCacheKey("withoutInDate"),
                async () => {
                    const requestOption = {
                        url: "/api/RMDiscoverySalesforceDataQueryApi/GetWithoutInDateList",
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

    static getSummaryStatisticalDataInfo = () => {
        return ExceptionHandler.handleAsync(
            async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey(`summary_sf`),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/GetSummaryStatisticalDataInfo`,
                            method: "GET",
                        };
                        return await fetchUtility(requestOption);
                    }
                );

                return res;
            },
            {
                BiggestObjectByDataSize: "",
                BiggestObjectByFileSize: "",
                BiggestObjectByRecordCount: "",
                DataStorageUsage: 0,
                DataTotalSize: 0,
                FileStorageUsage: 0,
                FileTotalSize: 0,
                ObjectTotalCount: 0,
                OldestRecords: 0,
                RecordsTotalCount: 0,
            }
        );
    };

    static getObjects = async (queryParameter) => {
            return ExceptionHandler.handleAsync(async () => {
                const res = await CacheUtil.WindowCacheUtil.tryGetValueAsync(
                    this.generateCacheKey("object_sf" + HashCodeUtil.hash(queryParameter)),
                    async () => {
                        const requestOption = {
                            url: `/api/RMDiscoverySalesforceDataQueryApi/SearchObject`,
                            method: "POST",
                            data: queryParameter,
                        };
                        return await fetchUtility(requestOption);
                    }
                );
                res.sort((prevObject, nextObject) => prevObject.ObjectType - nextObject.ObjectType);
                for (const object of res) {
                    switch (object.ObjectType) {
                        case SFObjectType.StandardObject:
                            object.Group = SFObjectTypeI18ns.get(SFObjectType.StandardObject)
                            break;
                        case SFObjectType.CustomObject:
                            object.Group = SFObjectTypeI18ns.get(SFObjectType.CustomObject)
                            break;
                        default:
                            break;
                    }
                    object.Checked = false;
                }
                return res;
            }, []);
    };
}

export default SFBasicDataRequester;

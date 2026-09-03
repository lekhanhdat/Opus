import _ from "lodash";

class SessionCacheUtil {
    static tryGetValueAsync = async (key, getValue) => {
        const cacheKey = `ave_discovery_${key}`;
        let cacheValue = sessionStorage.getItem(cacheKey);
        if ((_.isNil(cacheValue) || cacheValue === 'null') && !_.isNil(getValue)) {
            let value = "";
            if (getValue.constructor.name === "AsyncFunction") {
                value = await getValue();
            } else {
                value = getValue();
            }
            cacheValue = JSON.stringify(value);
            sessionStorage.setItem(cacheKey, cacheValue);
        }

        return {
            has: false,
            value: JSON.parse(cacheValue),
        };
    };
}

class WindowCacheUtil {
    static tryGetValueAsync = async (key, getValue) => {
        const cacheKey = `ave_discovery_${key}`;
        let cacheValue = window[cacheKey];
        if ((_.isNil(cacheValue) || cacheValue === 'null') && !_.isNil(getValue)) {
            let value = "";
            if (getValue.constructor.name === "AsyncFunction") {
                value = await getValue();
            } else {
                value = getValue();
            }
            cacheValue = JSON.stringify(value);
            window[cacheKey] = cacheValue
        }

        return JSON.parse(cacheValue);
    };
}

const CacheUtil = {
    SessionCacheUtil,
    WindowCacheUtil
}

export default CacheUtil;

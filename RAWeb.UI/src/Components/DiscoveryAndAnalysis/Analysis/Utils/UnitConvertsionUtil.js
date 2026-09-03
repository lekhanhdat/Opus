import _ from "lodash";
import { DataSizeType, DataSizeTypeI18ns } from "../Constants";
const CONVERT_CACHE_KEY = "UNIT_CONVERT";
const BASE = 1024;

class UnitConvertsionUtil {
    static Convert = (bValue, defaultSizeType = DataSizeType.GB, force = false) => {
        try {
            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (_.isNil(unit) || force) {
                unit = defaultSizeType;
            } else {
                unit = Number.parseInt(unit);
            }

            if (_.isNil(bValue)) {
                return 0;
            }

            if (unit === DataSizeType.B) {
                return bValue;
            } else if (unit === DataSizeType.KB) {
                return Math.ceil(bValue / 1024);
            } else if (unit === DataSizeType.MB) {
                return Math.ceil(bValue / 1024 / 1024);
            } else if (unit === DataSizeType.GB) {
                return Math.ceil(bValue / 1024 / 1024 / 1024);
            } else {
                return Math.ceil(bValue / 1024 / 1024 / 1024 / 1024);
            }
        } catch (err) {
            console.error(err);
            return 0;
        }
    };
    static GetUnitI18N = (defaultSizeType = DataSizeType.GB) => {
        try {
            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (_.isNil(unit)) {
                unit = defaultSizeType;
            } else {
                unit = Number.parseInt(unit);
            }
            return DataSizeTypeI18ns.get(unit)

        } catch (error) {
            console.error(error);
            return 0;
        }
    }

    static GetUnit = (bValue) => {
        const UnitRangeMapping = new Map([
            [DataSizeType.MB, [0, Math.pow(BASE, DataSizeType.MB) - 1]],
            [DataSizeType.GB, [Math.pow(BASE, DataSizeType.GB - 1), Math.pow(BASE, DataSizeType.GB) - 1]],
            [DataSizeType.TB, [Math.pow(BASE, DataSizeType.TB - 1), Math.pow(BASE, DataSizeType.TB) - 1]]
        ]);

        let result = DataSizeType.TB;
        UnitRangeMapping.forEach((value, key) => {
            if (_.inRange(bValue, value[0], value[1])) {
                result = key;
            }
        });

        return result;
    }

    static DynamicConvert = (bValue, numberOfDecimal = 1) => {
        try {
            if (!bValue) {
                return 0;
            }

            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (!_.isNil(unit)) {
                return Number((bValue / Math.pow(BASE, Number.parseInt(unit) - 1)).toFixed(numberOfDecimal));
            }

            return Number((bValue / Math.pow(BASE, this.GetUnit(bValue) - 1)).toFixed(numberOfDecimal));

        } catch (err) {
            console.error(err);
            return 0;
        }
    }
    static DecimalConvert = (bValue, numberOfDecimal = 1, defaultSizeType = DataSizeType.GB, force = false) => {
        try {
            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (_.isNil(unit) || force) {
                unit = defaultSizeType;
            } else {
                unit = Number.parseInt(unit);
            }

            if (_.isNil(bValue)) {
                return 0;
            }

            const result = (Number(bValue / Math.pow(BASE, Number.parseInt(unit) - 1)));

            if (Number.isInteger(result)) {
                return result;
            }
            if (result > 0 && Number(result.toFixed(numberOfDecimal)) === 0) {
                const roundedNumber = Number(Math.pow(10, -numberOfDecimal)).toFixed(numberOfDecimal);
                return Number.parseFloat(roundedNumber);
            }
            return Number.parseFloat(result.toFixed(numberOfDecimal));

        } catch (err) {
            console.error(err);
            return 0;
        }
    }

    static GetUnitForChart = (bValue) => {

        if (_.inRange(bValue, 50 * Math.pow(BASE, DataSizeType.GB - 1), 50 * Math.pow(BASE, DataSizeType.TB - 1))) {
            return DataSizeType.GB;
        }
        if (bValue > 50 * Math.pow(BASE, DataSizeType.TB - 1)) {
            return DataSizeType.TB;
        }

        return DataSizeType.MB;
    }

    static DynamicConvertForChart = (bValue, numberOfDecimal = 1) => {
        try {
            if (_.isNil(bValue) || bValue === 0) {
                return 0;
            }

            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (!_.isNil(unit)) {
                return Number(bValue / Math.pow(BASE, Number.parseInt(unit) - 1)).toFixed(numberOfDecimal);
            }

            return Number(bValue / Math.pow(BASE, this.GetUnitForChart(bValue) - 1)).toFixed(numberOfDecimal);

        } catch (err) {
            console.error(err);
            return 0;
        }
    }
    static GetUnitForJobDetail = (bValue) => {
        const Units = _.omit(DataSizeType, 'None');
        const UnitRangeMapping = new Map();

        Object.entries(Units).forEach(([key, value]) => {
            UnitRangeMapping.set(value, [Math.pow(BASE, value - 1), Math.pow(BASE, value) - 1])
        })

        if (bValue === 0) {
            return DataSizeType.B;
        }
        let result = DataSizeType.TB;
        UnitRangeMapping.forEach((value, key) => {
            if (_.inRange(bValue, value[0], value[1])) {
                result = key;
            }
        });

        return result;
    }

    static DynamicConvertForJobDetail = (bValue, numberOfDecimal = 3) => {
        try {
            if (_.isNil(bValue) || bValue === 0) {
                return 0;
            }

            let unit = localStorage.getItem(CONVERT_CACHE_KEY);
            if (!_.isNil(unit)) {
                return Number(bValue / Math.pow(BASE, Number.parseInt(unit) - 1)).toFixed(numberOfDecimal);
            }
            const result = Number(bValue / Math.pow(BASE, this.GetUnitForJobDetail(bValue) - 1));
            
            if (Number.isInteger(result)) {
                return Number.parseInt(result);
            }

            return result.toFixed(numberOfDecimal);

        } catch (err) {
            console.error(err);
            return 0;
        }
    }
}

export default UnitConvertsionUtil;

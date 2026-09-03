import I18N from "./i18n";

const BASE = 1024;
const DataSizeType = {
    None: 0,
    B: 1,
    KB: 2,
    MB: 3,
    GB: 4,
    TB: 5
};

class UnitConvertingUtil {
    get dataSizeTypeI18ns() {
        return {
            [DataSizeType.B]: I18N.get("OpusApp.Unit.FileSize.B"),
            [DataSizeType.KB]: I18N.get("OpusApp.Unit.FileSize.KB"),
            [DataSizeType.MB]: I18N.get("OpusApp.Unit.FileSize.MB"),
            [DataSizeType.GB]: I18N.get("OpusApp.Unit.FileSize.GB"),
            [DataSizeType.TB]: I18N.get("OpusApp.Unit.FileSize.TB")
        }
    }

    GetUnit = (bValue) => {
        const UnitRangeMapping = new Map([
            [DataSizeType.MB, [0, Math.pow(BASE, DataSizeType.MB) - 1]],
            [DataSizeType.GB, [Math.pow(BASE, DataSizeType.MB), Math.pow(BASE, DataSizeType.GB) - 1]],
            [DataSizeType.TB, [Math.pow(BASE, DataSizeType.GB), Math.pow(BASE, DataSizeType.TB) - 1]]
        ]);
    
        let result = DataSizeType.TB;
        UnitRangeMapping.forEach((value, key) => {
            if (bValue >= value[0] && bValue < value[1]) {
                result = key;
            }
        });
    
        return result;
    }

    GetUnitI18N = (bValue) => {
        let unit = this.GetUnit(bValue);
        return this.dataSizeTypeI18ns[unit];
    }
    
    DynamicConvert = (bValue, numberOfDecimal = 1) => {
        if (!bValue) {
            return 0;
        }
    
        return Number((bValue / Math.pow(BASE, this.GetUnit(bValue) - 1)).toFixed(numberOfDecimal));
    }

    DecimalConvert = (bValue, numberOfDecimal = 1, defaultSizeType = DataSizeType.GB) => {
        if (!bValue) {
            return 0;
        }

        const result = (Number(bValue / Math.pow(BASE, defaultSizeType - 1)));

        if (Number.isInteger(result)) {
            return result;
        }
        if (Number(result.toFixed(numberOfDecimal)) == 0) {
            return '0';
        }
        return result.toFixed(numberOfDecimal);
    }
}

const unitConverting = new UnitConvertingUtil();
export default unitConverting;
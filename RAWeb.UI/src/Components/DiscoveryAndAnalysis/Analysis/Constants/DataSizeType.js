const DataSizeType = {
    None: 0,
    B: 1,
    KB: 2,
    MB: 3,
    GB: 4,
    TB: 5
};

const DataSizeTypeI18ns = new Map([
    [DataSizeType.B, RMResx.RM_FA_Progress_Unit_B],
    [DataSizeType.KB, RMResx.RM_FA_Progress_Unit_KB],
    [DataSizeType.MB, RMResx.RM_FA_Progress_Unit_MB],
    [DataSizeType.GB, RMResx.RM_FA_Progress_Unit_GB],
    [DataSizeType.TB, RMResx.RM_FA_Progress_Unit_TB]
])

export {DataSizeType, DataSizeTypeI18ns};
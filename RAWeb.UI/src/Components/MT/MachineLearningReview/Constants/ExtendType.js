export const ExtendType = {
    None: 0,
    After3Month: 1,
    After6Month: 2,
    After1Year: 3,
    Custom: 4,
};

export const ExtendTypeI18Ns = new Map([
    [ExtendType.After3Month, RMResx.RM_MA_EntendDisposalTime_3M],
    [ExtendType.After6Month, RMResx.RM_MA_EntendDisposalTime_6M],
    [ExtendType.After1Year, RMResx.RM_MA_EntendDisposalTime_1Y],
    [ExtendType.Custom, RMResx.RM_MA_EntendDisposalTime_Custom],
]);

export const ExportTypeI18Ns = new Map([
    [ExtendType.After3Month, RMResx.RM_MA_HistoryExport_TimeRange_3M],
    [ExtendType.After6Month, RMResx.RM_MA_HistoryExport_TimeRange_6M],
    [ExtendType.After1Year, RMResx.RM_MA_HistoryExport_TimeRange_1Y],
    [ExtendType.Custom, RMResx.RM_MA_HistoryExport_All],
]);
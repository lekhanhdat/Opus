export const ExportType = {
    None: 0,
    After3Month: 1,
    After6Month: 2,
    After1Year: 3,
    Custom:4,
    All: 5,
};


export const ExportTypeI18Ns = new Map([
    [ExportType.After3Month, RMResx.RM_MA_HistoryExport_TimeRange_3M],
    [ExportType.After6Month, RMResx.RM_MA_HistoryExport_TimeRange_6M],
    [ExportType.After1Year, RMResx.RM_MA_HistoryExport_TimeRange_1Y],
    [ExportType.Custom, RMResx.RM_MA_HistoryExport_TimeRange_Custom],
    [ExportType.All, RMResx.RM_MA_HistoryExport_All],
]);
export const TermUsageReportType = {
    None: 0,
    Active: 1,
    Retired: 2,
    Orphaned: 3
};

export const TermUsageReportTypeName = new Map([
    [TermUsageReportType.Active, RMResx.RM_JS_TermUsageReport_ActiveTermsReport],
    [TermUsageReportType.Retired, RMResx.RM_JS_TermUsageReport_RetiredTermsReport],
    [TermUsageReportType.Orphaned, RMResx.RM_JS_TermUsageReport_OrphanTermsReport]
]);
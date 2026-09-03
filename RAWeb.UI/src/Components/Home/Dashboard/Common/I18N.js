import { SourceFlag, DateRange, DashboardJobCreationStatus } from "./Constants";

export const SourceFlagI18N = new Map([
    [SourceFlag.SharePoint, RMResx.RM_JS_SPS_TabLabel_SP],
    [SourceFlag.FileSystem, RMResx.RM_JS_SPS_TabLabel_FS],
    [SourceFlag.Exchange, RMResx.RM_JS_SPS_TabLabel_EXO],
    [SourceFlag.Physical, RMResx.RM_JS_SPS_TabLabel_Physical],
    [SourceFlag.SharePointOnPrem, RMResx.RM_JS_SPS_TabLabel_SPLocal]
]);

export const DateRangeI18N = new Map([
    [DateRange.Last10Day, RMResx.RM_JS_DSB_Last10Days],
    [DateRange.Last10Week, RMResx.RM_JS_DSB_Last10Weeks],
    [DateRange.Last12Month, RMResx.RM_JS_DSB_Last12Month]
]);

export const DashboardJobCreationStatusI18n = new Map([
    [DashboardJobCreationStatus.None, ""],
    [DashboardJobCreationStatus.ExistsJobQueue, RMResx.RM_DSB_ExistsJobQueue],
    [DashboardJobCreationStatus.HasRunningJob, RMResx.RM_DSB_HasRunningJob],
    [DashboardJobCreationStatus.Failed, RMResx.RM_DSB_Failed],
    [DashboardJobCreationStatus.Succeed, RMResx.RM_DSB_Succeed]
]);
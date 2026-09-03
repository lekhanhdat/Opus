export const JobType = {
    RMArchiverBackup: 1,
    SOPreScan: 2,
    EnforceRetention : 3,
    ArchiverRestore: 4,
    Discovery: 5,
    DataSync: 6,
    EnforceRuleAction: 7,
    SyncNode: 8,
    DashboardData: 9,
    TermSync: 10,
};

export const JobTypes = [
    JobType.RMArchiverBackup,
    JobType.SOPreScan,
    JobType.EnforceRetention,
    JobType.ArchiverRestore,
    JobType.Discovery,
    JobType.DataSync,
    JobType.EnforceRuleAction,
    JobType.SyncNode,
    JobType.DashboardData,
    JobType.TermSync,
];

export const JobTypeI18N = new Map([
    [JobType.RMArchiverBackup, RMResx.RM_JS_JM_JobType_RMArchiverBackup],
    [JobType.SOPreScan, RMResx.RM_JS_JM_JobType_SOPreScan],
    [JobType.EnforceRetention, RMResx.RM_JS_JM_JobType_EnforceRetention],
    [JobType.ArchiverRestore, RMResx.RM_JS_JN_JobType_Restore],
    [JobType.Discovery, RMResx.RM_JS_JM_JobType_DiscoveryJobV3 ],
    [JobType.DataSync, RMResx.RM_JS_JM_JobType_DataSynchronisation],
    [JobType.EnforceRuleAction, RMResx.RM_JS_JM_JobType_DisposalActivityManagement],
    [JobType.SyncNode, RMResx.RM_JS_JM_JobType_SyncNodesFromAOS],
    [JobType.DashboardData, RMResx.RM_JS_JM_JobType_Dashboard],
    [JobType.TermSync, RMResx.RM_JS_JM_JobType_TermSynchronization],
]);

export const JobStatus = {
    Finished: 2,
    Failed: 3,
    FinishWithException: 4,
    Stopped: 5,
    Skipped: 6,
};

export const JobStatusI18N = new Map([
    [JobStatus.Finished, RMResx.RM_JS_JM_Status_Finished],
    [JobStatus.Failed, RMResx.RM_JS_JM_Status_Failed],
    [JobStatus.FinishWithException, RMResx.RM_JS_JM_Status_FinishWithException],
    [JobStatus.Stopped, RMResx.RM_JS_JM_Status_Stopped],
    [JobStatus.Skipped, RMResx.RM_JS_JM_Status_Skipped]
]);

export const RMArchiverBackupStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const SOPreScanStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const EnforceRetentionStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const ArchiverRestoreStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const DiscoveryStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
];
export const DataSyncStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const EnforceRuleActionStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const SyncNodeStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const DashboardDataStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];
export const TermSyncStatus = [
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Finished), text: JobStatusI18N.get(JobStatus.Finished), tooltip: JobStatusI18N.get(JobStatus.Finished), value: JobStatus.Finished },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Failed), text: JobStatusI18N.get(JobStatus.Failed), tooltip: JobStatusI18N.get(JobStatus.Failed), value: JobStatus.Failed },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.FinishWithException), text: JobStatusI18N.get(JobStatus.FinishWithException), tooltip: JobStatusI18N.get(JobStatus.FinishWithException), value: JobStatus.FinishWithException },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Skipped), text: JobStatusI18N.get(JobStatus.Skipped), tooltip: JobStatusI18N.get(JobStatus.Skipped), value: JobStatus.Skipped },
    { checked: false, disabled: false, name: JobStatusI18N.get(JobStatus.Stopped), text: JobStatusI18N.get(JobStatus.Stopped), tooltip: JobStatusI18N.get(JobStatus.Stopped), value: JobStatus.Stopped },
];


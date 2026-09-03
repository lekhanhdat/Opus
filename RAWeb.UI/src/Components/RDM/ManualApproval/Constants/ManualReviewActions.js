export const ManualReviewAction = {
    None: 0,
    Approve: 1,
    Reject: 2,
    Extend: 3,
    Reassign: 4,
    Escalate: 5,
    ChangeAction: 6,
    ExtendRestore: 7,
    ResetManualWorkflow: 8,
    Export: 9,
    Import: 10,
    Reclassify: 11,
};

export const ManualReviewActionI18Ns = new Map([
    [ManualReviewAction.Approve, RMResx.RM_MA_Approve],
    [ManualReviewAction.Reject, RMResx.RM_MA_Reject],
    [ManualReviewAction.Reclassify, RMResx.RM_MA_Reclassify],
    [ManualReviewAction.Extend, RMResx.RM_MA_Extend],
    [ManualReviewAction.Reassign, RMResx.RM_MA_Reassign],
    [ManualReviewAction.Escalate, RMResx.RM_MA_Escalate],
    [ManualReviewAction.ChangeAction, RMResx.RM_MA_ChangeAction],
    [ManualReviewAction.ExtendRestore,RMResx.RM_MA_Extended_Restore],
    [ManualReviewAction.ResetManualWorkflow, RMResx.RM_MA_ResetManualWorkflow],
    [ManualReviewAction.Export, RMResx.RM_MA_Export],
    [ManualReviewAction.Import, RMResx.RM_MA_Import],
]);

export const ManualReviewActionIcons = new Map([
    [ManualReviewAction.Approve, "fia-check"],
    [ManualReviewAction.Reject, "fia-close"],
    [ManualReviewAction.Reclassify, "fia-term"],
    [ManualReviewAction.Extend, "fia-clock-regular"],
    [ManualReviewAction.Reassign, "fia-export-settings"],
    [ManualReviewAction.Escalate, "fia-escalate"],
    [ManualReviewAction.ChangeAction, "fia-change-disposal-action"],
    [ManualReviewAction.ExtendRestore, "fia-remove-disposal-extension"],
    [ManualReviewAction.ResetManualWorkflow, "fia-restart"],
    [ManualReviewAction.Import,"fia-import"],
]);

export const ChangeTermOrigin = {
    Search: 0,
    Manual: 1,
}
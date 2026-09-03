export const ApprovalStatus = {
    WaitingApprove: 1,
    Approved: 3,
    Rejected: 4,
    WorkflowInProgress: 10,
    WorkflowComplete: 11,
    Cancel: 13
};

export const ApprovalStatusI18Ns = new Map([
    [ApprovalStatus.WaitingApprove, RMResx.RM_JS_MA_ApproveStatus_WaitingApprove],
    [ApprovalStatus.Approved, RMResx.RM_JS_MA_ApproveStatus_Approved],
    [ApprovalStatus.Rejected, RMResx.RM_JS_MA_ApproveStatus_Rejected],
    [ApprovalStatus.WorkflowInProgress, RMResx.RM_JS_MA_WorkflowStatus_Inprogress],
    [ApprovalStatus.WorkflowComplete, RMResx.RM_JS_MA_WorkflowStatus_Complete],
    [ApprovalStatus.Cancel, RMResx.RM_JS_MA_ApproveStatus_Cancelled]
]);

export const InProgressApprovalStatus = {
    1: ApprovalStatus.WaitingApprove,
    10: ApprovalStatus.WorkflowInProgress,
};
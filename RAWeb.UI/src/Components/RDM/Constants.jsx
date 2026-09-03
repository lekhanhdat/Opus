const ManualApprovalColumn = [
    "",
    RMResx.RM_JS_MA_Grid_Title,
    RMResx.RM_JS_BCM_Explorer_Datagrid_FileType,
    RMResx.RM_JS_MA_Grid_ApprovalStatus,
    RMResx.RM_JS_MA_Grid_Rule,
    RMResx.RM_JS_Rule_DisposalClass_Title,
    RMResx.RM_JS_MA_Grid_RelatedRecords,
    RMResx.RM_JS_MA_Grid_RelatedRecordsAction,
    RMResx.RM_MA_Grid_EscalateOrReassignFrom,
    RMResx.RM_JS_MA_Grid_RecordOwner,
    RMResx.RM_JS_MA_Grid_ApprovedBy,
    RMResx.RM_JS_MA_Grid_Comment,
    RMResx.RM_JS_MA_Grid_ModifiedBy,
    RMResx.RM_JS_MA_Grid_CreatedBy,
    RMResx.RM_JS_MA_Grid_CreatedTime
];
const ManualApprovalColumnWidth = [
    35, 200, 150, 200, 100, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200
];
const ManualApprovalColumnSortValue = [
    "", "LeafName", "ContentType", "Status", "RuleName","DisposalClass","", "RelatedRecordsAction", "EscalateFrom", "RecordOwner", "ApprovedBy", "", "ModifiedBy", "CreatedBy", "CreatedTime"];
const ApprovalStatus = {
    1: RMResx.RM_JS_MA_ApproveStatus_WaitingApprove,
    3: RMResx.RM_JS_MA_ApproveStatus_Approved,
    4: RMResx.RM_JS_MA_ApproveStatus_Rejected,
    10: RMResx.RM_JS_MA_WorkflowStatus_Inprogress,
    11: RMResx.RM_JS_MA_WorkflowStatus_Complete
};
const FilterApprovalStatus = {
    1: RMResx.RM_JS_MA_ApproveStatus_WaitingApprove,
    3: RMResx.RM_JS_MA_ApproveStatus_Approved,
    4: RMResx.RM_JS_MA_ApproveStatus_Rejected
};
const DisposalAction = {
    0: RMResx.RM_JS_RDM_RelatedRecordsAction_None,
    1: RMResx.RM_JS_RDM_RelatedRecordsAction_Both,
};

const ExtendDispositionType = {
    None: 0,
    ThreeMonths: 1, 
    SixMonths: 2,
    OneYear: 3,
    Custom: 4
};

export {
    ManualApprovalColumn, ManualApprovalColumnWidth, ApprovalStatus, FilterApprovalStatus, DisposalAction,
    ManualApprovalColumnSortValue, ExtendDispositionType
};
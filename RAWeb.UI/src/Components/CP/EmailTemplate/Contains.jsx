const ActionTypes = {
    ADD: "add",
    EDIT: "edit",
    DELETE: "delete",
    COPY: "copy",
};

const EmailTemplateInternalType = {
    LoanRequsetToEndUser: 1,
    LoanRequsetToRM: 2,
    LoanRequsetApproved: 3,
    LoanRequsetRejected: 4,
    CreationRequestToEndUser: 5,
    CreationRequestToRM: 6,
    CreationRequestApproved: 7,
    CreationRequestRejected: 8,
    WaitingApproval: 9,
    Approved: 10,
    Rejected: 11,
    Escalated: 12,
    ManualApproval: 13,
    MLManualApproval: 14,
    ExportZipPassword: 15
};

const EmailTemplateType = {
    BoxOrFile: 1,
    FileOrRecord: 2,
    RecordsForReview: 3,
    MLRecordsForReview: 4,
    ExportZipPasswordForReview: 5,
};

const MovementEmailTemplateIds = {
    EndUserSubmitted: "3f2c6a0d-1b74-47c8-a9e2-5d9f2b1a8e61",
    RMAssigned: "d4a8f13e-96b7-4e2d-8b51-71c3f5a2d984",
    ApprovedEndUser: "9b7e1f2a-3d4c-4a6f-bf90-2e15c8d7436b",
    Rejected: "f1c4a8d7-5e2b-41c9-8d63-a7b95e14f032",
    ApprovedDestinationRM: "a7f3c291-8d64-4b17-a9e2-53c8f0d6b421",
    HoldManagerAssignment: "d4e5f6a7-b8c9-4d0e-9f1a-2b3c4d5e6f7a"
};

export { ActionTypes, EmailTemplateInternalType, EmailTemplateType, MovementEmailTemplateIds };
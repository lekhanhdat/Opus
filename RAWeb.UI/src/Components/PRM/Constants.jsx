import {NodeType} from "../../Constants/DAEnums";

export const dispatchAction = {
    elementDisabled: 'elementDisabled',
    clearData: 'clearData',
    save: 'save',
    setData: 'setData',
};
export const panelType = {
    Category: 0,
    Box: 1,
    Folder: 2,
    Record: 3
};
export const columnTypes = [
    {id: 1, name: RMResx.RM_PRM_EditTemplate_ColumnType_SingleText},
    {id: 2, name: RMResx.RM_PRM_EditTemplate_ColumnType_MultipleText},
    {id: 3, name: RMResx.RM_PRM_EditTemplate_ColumnType_DateTime},
    {id: 4, name: RMResx.RM_PRM_EditTemplate_ColumnType_SingleChoice},
    {id: 7, name: RMResx.RM_PRM_EditTemplate_ColumnType_MultipleChoice},
    {id: 5, name: RMResx.RM_PRM_EditTemplate_ColumnType_PeopleorGroup},
    {id: 6, name: RMResx.RM_PRM_EditTemplate_ColumnType_Number}
];

export const ColumnTypesEnum = {
    SingleText: 1,
    MultipleText: 2,
    DateTime: 3,
    SingleChoice: 4,
    PeopleOrGroup: 5,
    Number: 6,
    MultipleChoice: 7,
    Taxonomy: 10
};

export const categoryAction = {
    load: 1,
    create: 2,
    edit: 3,
    remove: 4
};

export const YesOrNo = {
    0: RMResx.RM_PRM_PRE_Cell_HoldStatusYes,
    1: RMResx.RM_PRM_PRE_Cell_HoldStatusNo
};

export const RequestStatus = [
    {id: 0, name: RMResx.RM_PRM_RequestStatus_WaitingForApprove},
    {id: 1, name: RMResx.RM_PRM_RequestStatus_Approved},
    {id: 2, name: RMResx.RM_PRM_RequestStatus_Reject},
    {id: 3, name: RMResx.RM_PRM_RequestStatus_Canceled},
];

export const PhysicalRequestStatus = [
    {id: 0, name: RMResx.RM_PRM_RequestStatus_WaitingForApprove},
    {id: 1, name: RMResx.RM_PRM_RequestStatus_Approved},
    {id: 2, name: RMResx.RM_PRM_RequestStatus_Reject},
    {id: 3, name: RMResx.RM_PRM_RequestStatus_Canceled},
];

export const PhysicalRequestType = [
    {id: 0, name: RMResx.RM_PRM_RequestType_Loan},
    {id: 1, name: RMResx.RM_PRM_RequestType_Creation},
    {id: 2, name: RMResx.RM_PRM_PRE_MovementRequest} 
];

export const RequestTypeMode = {
    Loan: 0,
    Creation: 1,
    Movement: 2,
};

export const RequestStatusMode = {
    WaitingForAproval: 0,
    Approved: 1,
    Reject: 2,
    Canceled: 3,
};

export const PhysicalRequestFilterColumn = {
    None:0,
    Type:1,
    Status:2, 
    RequestBy:3
};

export const ActionTypeMode = {
    None: 0,
    Approval: 1,
    Reject: 2,
};
export const TemplateInheritSettingEnum = {
    PushToChild: 32,       //0010_0000
    ChildInheritsValue: 8, //0000_1000
    InheritFromParent: 4,  //0000_0100
    AllowModifyValue: 2,   //0000_0010
    None: 0
};
export const PhyCategoryBaseInfoId = {
    boxId: '11d303d8-d6fb-4a2b-a87d-3a18e2ac2d9a',
    fileId: 'd192c525-4a1e-48a2-9c00-f864a26571cf',
    recordId: '5815d70c-1e9d-404f-89bb-933e365a057c'
};
export const PhyNodeTypeNames = {
    9250: RMResx.RM_PRM_PRE_TableItemType_Container,
    9300: RMResx.RM_PRM_PRE_TableItemType_Box,
    9400: RMResx.RM_PRM_PRE_TableItemType_File,
    9500: RMResx.RM_PRM_PRE_TableItemType_Record
};

export const PhysicalTableColumnInfo = {
    Location: {
        header: [
            RMResx.RM_PRM_PRE_Column_Name, RMResx.RM_PRM_PRE_Column_TotalSpace, RMResx.RM_PRM_PRE_Column_Creator,
            RMResx.RM_PRM_PRE_Column_CreatedTime, RMResx.RM_PRM_PRE_Column_Modifier, RMResx.RM_PRM_PRE_Column_ModifiedTime
        ],
        width: [200, 200, 150, 285, 150, 285],
        id: ['location-1', 'location-2', 'location-3', 'location-4', 'location-5', 'location-6']
    },
    9200: {
        header: [
            RMResx.RM_PRM_PRE_Column_Name, RMResx.RM_PRM_PRE_Column_Type, RMResx.RM_PRM_PRE_Column_ID,
            RMResx.RM_PRM_PRE_Column_Capacity, RMResx.RM_PRM_PRE_Column_Status, RMResx.RM_PRM_PRE_Column_DisposalClass,
            RMResx.RM_PRM_PRE_Column_RuleName, RMResx.RM_PRM_PRE_Column_RuleAction, RMResx.RM_PRM_PRE_Column_ActionDueSate,
            RMResx.RM_PRM_PRE_Column_RecordOwner, RMResx.RM_PRM_PRE_Column_PersonHoldStatus, RMResx.RM_PRM_PRE_Column_LoanBy , RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldBy, RMResx.RM_PRM_PRE_Column_HoldType, RMResx.RM_PRM_PRE_Column_HoldUntil, RMResx.RM_PRM_PRE_Column_Creator, RMResx.RM_PRM_PRE_Column_Modifier, RMResx.RM_PRM_PRE_Column_ModifiedTime
        ],
        width: [200, 120, 160, 160, 100, 180, 150, 200, 285, 150, 150, 150, 150, 150, 150, 150, 150, 150, 285],
        id: ['phy-1', 'phy-2', 'phy-3', 'phy-4', 'phy-5', 'phy-6', 'phy-7', 'phy-8', 'phy-9', 'phy-10', 'phy-11', 'phy-12', 'phy-13', 'phy-14', 'phy-15', 'phy-16', 'phy-17', 'phy-18', 'phy-19']
    },
    9300: {
        header: [
            RMResx.RM_PRM_PRE_Column_FileTitle, RMResx.RM_PRM_PRE_Column_Type, RMResx.RM_PRM_PRE_Column_ID, RMResx.RM_PRM_PRE_Column_Status,
            RMResx.RM_PRM_PRE_Column_DisposalClass, RMResx.RM_PRM_PRE_Column_RuleName, RMResx.RM_PRM_PRE_Column_RuleAction,
            RMResx.RM_PRM_PRE_Column_ActionDueSate, RMResx.RM_PRM_PRE_Column_RecordOwner, RMResx.RM_PRM_PRE_Column_PersonHoldStatus, RMResx.RM_PRM_PRE_Column_LoanBy,
            RMResx.RM_PRM_PRE_Column_DisposalStatus, RMResx.RM_PRM_PRE_Column_HoldBy, RMResx.RM_PRM_PRE_Column_HoldType, RMResx.RM_PRM_PRE_Column_HoldUntil, RMResx.RM_PRM_PRE_Column_Creator, RMResx.RM_PRM_PRE_Column_Modifier,
            RMResx.RM_PRM_PRE_Column_ModifiedTime
        ],
        width: [200, 120, 160, 100, 180, 150, 200, 285, 150, 150, 150, 150, 150, 150, 150, 150, 150, 285],
        id: ['box-1', 'box-2', 'box-3', 'box-4', 'box-5', 'box-6', 'box-7', 'box-8', 'box-9', 'box-10', 'box-11', 'box-12', 'box-13', 'box-14', 'box-15', 'box-16', 'box-17', 'box-18']
    },
    9400: {
        header: [
            RMResx.RM_PRM_PRE_Column_RecordTitle, RMResx.RM_PRM_PRE_Column_ID, RMResx.RM_PRM_PRE_Column_Status, RMResx.RM_PRM_PRE_Column_Creator,
            RMResx.RM_PRM_PRE_Column_CreatedTime, RMResx.RM_PRM_PRE_Column_Modifier, RMResx.RM_PRM_PRE_Column_ModifiedTime
        ],
        width: [200, 160, 100, 150, 285, 150, 285],
        id: ['file-1', 'file-2', 'file-3', 'file-4', 'file-5', 'file-6', 'file-7']
    },
    9250: {
        header: [
            RMResx.RM_PRM_PRE_Column_Name, RMResx.RM_PRM_PRE_Column_Type, RMResx.RM_PRM_PRE_Column_ID,
            RMResx.RM_PRM_PRE_Column_Capacity, RMResx.RM_PRM_PRE_Column_Status, RMResx.RM_PRM_PRE_Column_DisposalClass,
            RMResx.RM_PRM_PRE_Column_RuleName, RMResx.RM_PRM_PRE_Column_RuleAction, RMResx.RM_PRM_PRE_Column_ActionDueSate,
            RMResx.RM_PRM_PRE_Column_RecordOwner, RMResx.RM_PRM_PRE_Column_PersonHoldStatus, RMResx.RM_PRM_PRE_Column_LoanBy , RMResx.RM_PRM_PRE_Column_DisposalStatus,
            RMResx.RM_PRM_PRE_Column_HoldBy, RMResx.RM_PRM_PRE_Column_HoldType, RMResx.RM_PRM_PRE_Column_HoldUntil, RMResx.RM_PRM_PRE_Column_Creator, RMResx.RM_PRM_PRE_Column_Modifier, RMResx.RM_PRM_PRE_Column_ModifiedTime
        ],
        width: [200, 120, 160, 160, 100, 180, 150, 200, 285, 150, 150, 150, 150, 150, 150, 150, 150, 150, 285],
        id: ['con-1', 'con-2', 'con-3', 'con-4', 'con-5', 'con-6', 'con-7', 'con-8', 'con-9', 'con-10', 'con-11', 'con-12', 'con-13', 'con-14', 'con-15', 'con-16', 'con-17', 'con-18', 'con-19']
    },
};

const PhysicalTemplateIDs = {};
PhysicalTemplateIDs[NodeType.PhyBox] = 1;
PhysicalTemplateIDs[NodeType.PhyFile] = 2;
PhysicalTemplateIDs[NodeType.PhyRecord] = 3;
export { PhysicalTemplateIDs };

export const CardType = {
    EmptyBoxSuite: 1,
    EmptyFolderSuite: 2,
    BoxSuite:3,
    FolderSuite:4,
    Folder:5,
    Record:6
};

export const CardMenuAction = {
    EditSuite:1,
    DeleteSuite:2,
    EditBox:3,
    DeleteBox:4
};

export const StartFromType = {
    None: 0,
    Box: 1,
    Folder: 2,
    CustomTemplate: 3
};

export const TemplateCreateMethod = {
    New: 0,
    ExistingFolder: 1
};

export const TemplateTypes = {
    None: 0,
    Records: 1,
    Folder: 2,
    Box: 3,
    CustomTemplate: 5
};

export const ViewDataLevel = {
    Suite: 1,
    Box: 2,
    Folder: 3,
    Record: 4
};

export const CommonTemplateManagementType = {
    Suite: 1,
    Folder: 2,
    Record: 3
};

export const SaveTemplateResult = {
    None: 0,
    MissUniqueIdSettingMode: 1,
    PrefixDuplicate: 2,
    NameDuplicate: 3,
    CustomTemplateExceedMaxDepth: 4,
    Success : 10,
    Failed : 11,
};

export const BoxAndFolderNumType = {
    BoxAndFildIsZero: 1,
    BoxAndFildIsZeroLessThan300: 2,
    BoxAndFildIsZeroMoreThan300: 3
};

export const NewOrEditTemplateCookieNames = {
    CreateSuccess: "templateCreateSuccess",
    EditSuccess:"templateEditSuccess"
};

export const BarcodeTemplateType = {
    Box: 1,
    Folder: 2
};

export const BarcodeTemplateComboboxNames = [
    RMResx.RM_PRM_BarcodeTemp_AreaB_Title,
    RMResx.RM_PRM_BarcodeTemp_AreaC_Title,
    RMResx.RM_PRM_BarcodeTemp_AreaD_Title,
    RMResx.RM_PRM_BarcodeTemp_AreaE_Title,
    RMResx.RM_PRM_BarcodeTemp_AreaF_Title,
];

export const BarcodeTemplateBuildInIDs = [
    "62AB4A7B-960E-4D34-9D44-ACAD71EC3E13",
    "BB2CFC11-0DE6-4DAE-8414-0FBAD2EBD8D7",
    "96CE5E52-3A1B-4E99-9F75-6954D27D2FEE",
    "B12C2382-FCFD-4B55-8446-B41A20C25AF0",
    "332844A6-DAF6-4488-9BF4-1F36BAD58426"
];


export const BarcodeTemplateBuildInNames = {
    "62AB4A7B-960E-4D34-9D44-ACAD71EC3E13": RMResx.RM_PRM_PRE_Column_ID,
    "BB2CFC11-0DE6-4DAE-8414-0FBAD2EBD8D7": RMResx.RM_PRM_PRE_Column_Creator,
    "96CE5E52-3A1B-4E99-9F75-6954D27D2FEE": RMResx.RM_PRM_PRE_Column_CreatedTime,
    "B12C2382-FCFD-4B55-8446-B41A20C25AF0": RMResx.RM_PRM_PRE_Column_Modifier,
    "332844A6-DAF6-4488-9BF4-1F36BAD58426": RMResx.RM_PRM_PRE_Column_ModifiedTime,
};

export const TemplateTreeNodeType = {
    Root: 0,
    Records: 1,
    Folder: 2,
    Box: 3,
    Location: 4,
    Custom: 5,
    Suite : 6
}

export const TemplateDisplayMode = {
    None: 0,
    NewSuiteSettings: 1,
    EditSuiteSettings: 2,
    NewTemplateSettings: 3,
    EditTemplateSettings: 4,
    ViewTemplateDetails: 5,
    ViewSuiteDetails: 6
};

export const SuiteSettingDisplayMode = {
    Edit: 1,
    View: 2
};

export const UITemplateTreeNodeType = {
    Root: 0,
    BoxSuite: 1,
    FolderSuite: 2,
    CustomSuite: 3,
    CustomTemplate: 4,
    BoxTemplate: 5,
    FolderTemplate: 6,
    RecordTemplate: 7
}

export const DefaultSuiteUniqueIds = [
    "6feecea2-2076-4557-ae9c-a90f9eb91617", 
    "c7a9a849-c9a3-4c0b-ba38-ba0db43af048"
];

export const DefaultTemplateUniqueIds = [
    "f0b53a20-d955-476b-bb83-41488cfb2750", 
    "b775e3c7-20a8-4141-98fc-49824a028331", 
    "01bd2c27-d4d5-4714-8ef3-e460323a977b"
];

export const PhysicalRecordActionType = {
    Create: 0,
    Edit: 1,
    ManageRelated: 2,
    ImportCreate: 3,
    ImportEdit: 4,
    Disposal: 5,
    Move: 6,
    PlaceHold: 7, 
    CancelHold: 8,
    ExtendHold: 9,
    AccessControl: 10,
    Loan: 11,
    Reclassify: 12,
    ReturnLoan: 13,
    AddHold: 14,
    ChangeHold: 15,
}

export const PhysicalRecordActionTypeI18Ns = new Map([
    [PhysicalRecordActionType.Create, RMResx.RM_PRM_PRE_AuditAction_Create],
    [PhysicalRecordActionType.Edit, RMResx.RM_PRM_PRE_AuditAction_Edit],
    [PhysicalRecordActionType.ManageRelated, RMResx.RM_PRM_PRE_AuditAction_ManageRelated],
    [PhysicalRecordActionType.ImportCreate, RMResx.RM_PRM_PRE_AuditAction_ImportCreate],
    [PhysicalRecordActionType.ImportEdit, RMResx.RM_PRM_PRE_AuditAction_ImportEdit],
    [PhysicalRecordActionType.Disposal, RMResx.RM_PRM_PRE_AuditAction_Disposal],
    [PhysicalRecordActionType.Move, RMResx.RM_PRM_PRE_AuditAction_Move],
    [PhysicalRecordActionType.PlaceHold, RMResx.RM_PRM_PRE_AuditAction_PlaceHold],
    [PhysicalRecordActionType.CancelHold, RMResx.RM_PRM_PRE_AuditAction_CancelHold],
    [PhysicalRecordActionType.ExtendHold, RMResx.RM_PRM_PRE_AuditAction_ExtendHold],
    [PhysicalRecordActionType.AccessControl, RMResx.RM_PRM_PRE_AuditAction_AccessControl],
    [PhysicalRecordActionType.Loan, RMResx.RM_PRM_PRE_AuditAction_Loan],
    [PhysicalRecordActionType.Reclassify, RMResx.RM_PRM_PRE_AuditAction_Reclassify],
    [PhysicalRecordActionType.ReturnLoan, RMResx.RM_PRM_PRE_AuditAction_ReturnLoan],
    [PhysicalRecordActionType.AddHold, RMResx.RM_PRM_PRE_AuditAction_AddHold],
    [PhysicalRecordActionType.ChangeHold, RMResx.RM_PRM_PRE_AuditAction_ChangeHold],
]);

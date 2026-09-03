import { getActionDueDateI18n } from "../../../Utilities/CommonUtil";

export const SearchViewTypes = {
    Save: 0,
    SaveAs: 1,
    SaveAsDefaut: 2,
    Delete: 3,
    View: 4,
    Share: 5
};

export const SearchViewActions = [
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_Save,
        ActionType: SearchViewTypes.Save,
    },
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_SaveAs,
        ActionType: SearchViewTypes.SaveAs,
    },
    {
        Name: RMResx.RM_HS_SearchView_Btn_Share,
        ActionType: SearchViewTypes.Share,
    },
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_SaveAsDefaultView,
        ActionType: SearchViewTypes.SaveAsDefaut,
    },
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_Delete_View,
        ActionType: SearchViewTypes.Delete,
    }
];

export const BuildInViewActions = [
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_SaveAs,
        ActionType: SearchViewTypes.SaveAs,
    },
    {
        Name: RMResx.RM_HS_Criteria_View_Btn_SaveAsDefaultView,
        ActionType: SearchViewTypes.SaveAsDefaut,
    },
];

export const DispatchSearchBoxTypes = {
    IsContainsPhySource: 0,
    EchoSearchContent: 1
};

export const BuildColumnIds = {
    NameOrUniqueId: "38f015c0-f507-4925-a855-d1546dc0b0f9",
    NameOrTitle: "de5e99cb-4fb4-4e25-b732-a1dce71dd048",
    UniqueId: "c980eb95-ea92-4f07-9f97-1a8ab2a053fa",
    SourceFlag: "edbac887-d4cc-ed92-ad0d-0e68ceb336a0",
    Type: "90c0f7ce-ad79-4a9d-a5eb-3b097006b03d",
    Classification: "ce693d2c-ab58-4d29-9db5-3191bfc5c81a",
    RuleName: "da9dcebc-5628-45b7-9dff-37ca8a601e31",
    RuleAction: "4de03a10-4b33-4091-8929-68be1f7d2325",
    Owners: "38e1e287-4077-44a5-ba57-3de64561c51f",
    HoldStatus: "f9806a66-1be8-4f85-867e-f0de4fa4c073",
    HoldBy: "8499e388-9c52-4366-a7b3-df77c70e648f",
    HoldTitle: "3667DC37-36EE-40FD-AEE3-7BFE0F80A123",
    HoldDueDate: "7E60D9C2-C833-4831-80C3-66C8C36E75FA",
    ActionDueDate: "9117fd6b-4171-4405-b881-cbe139e6ced7",
    CreatedDateInfo: "c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5",
    CreatedBy: "91a08d45-c5dd-43da-b6c4-670f11ac273e",
    ModifiedTime: "3ec9a488-90fa-4d62-835f-0df0cd2e9f97",
    ArchivedTime: "1f525384-e4bf-ed14-78fe-ca9ef9f0d930",
    ModifiedBy: "1f2e8c3f-e49a-473c-bd16-8647258cf15c",
    DeclaredRecord: "bf4e131c-1d9b-403b-8a9f-a1fa3b63cd15",
    LockedByRecordLabel: "a8b3c9d1-e2f4-4a5c-9b8d-7e6f5a4c3b21",
    FileSystem: "becf61cd-bd6b-440c-8e33-4b6300be58d5",
    LoanBy: "df21d79c-bc37-fdfd-f59e-641f7d630488",
    OnLoan: "b3512f95-198e-c3c9-c2d6-ec21c81e0bae",
    SPOLocation: "ee86426d-488f-4bdb-a63b-2ef6a61c7bef",
    TeamsLocation: "f0a0d8f6-71f9-4d42-a3bf-ab828eb73ded",
    PhyTemplates: "04a17a80-1bd9-4eaa-9cf1-4bce21c1df01",
    ReturnDate: "f693dcc8-6e52-423f-849c-1cbac642ad3f",
    ContentArchived: "8cd3ffc4-0ebd-461a-8d63-347851abc60e",
    GoogleLocation: "831d8bcf-fb12-4cce-ba5e-a1a37ccf5234",
    HoldByUsersId: "e2e2e7e2-1c2a-4b7a-9b2e-2e2e7e2e7e2e",
};

export const OldBuildColumnIds = {
    NameOrTitle: "6428d70f-95a9-4da7-a8a3-96ea3316f4cf",
    UniqueId: "53b02b1a-65b1-4001-a2d7-02fe1c4ed5bc",
};

export const OperationLogicValues = {
    And: 0,
    Or: 1
};

export const SearchKeyOperationLogic = {
    Contains: 0,
    Equals: 1
};

export const LocationOperationLogic = {
    Contains: 0,
    Within: 1
}

export const ColumnNumberCondition = {
    Equal: "0",
    GreaterThenOrEqual: "1",
    LessThenOrEqual: "2"
};

export const DateConditions = {
    Overdue: "-3",
    Pending: "-2",
    NextJob: "-1",
    NotSpecified: "0",
    Before: "1",
    After: "2",
    FromTo: "3",
};

export const ToSearchComponentDispatchType = {
    DisableBackBaseBtn: 1,
    SourceType: 2,
    Valid: 3,
    InitData: 4,
    TransSelectedTableIds: 5,
    SortColumn: 6
};

export const MsgComponentType={
    MsgBar: 1,
    Toast: 2
};

export const SpecialSearchViewIds = {
    ReturnDate: "-1",
    Active: "7",
    Archived: "8",
    SharePoint: "1",
    FileSystem: "2",
    Exchange: "3",
    Physical: "4",
    SharePointOnPrem: "5",
    OneDrive: "6"
};

export const AuditModule = {
    PhysicalRecordsGlobalSearch: 67,
}

export const GlobalSearchColumns = [
    {
        header: RMResx.RM_PRM_PRE_Column_Name,
        width: [300],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "leafName" },
        disabled: true,
        id: BuildColumnIds.NameOrTitle,
        NameHash: BuildColumnIds.NameOrTitle,
        OldUniqueId: OldBuildColumnIds.NameOrTitle //兼容老数据
    }, {
        header: RMResx.RM_PRM_PRE_Column_ID,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "recordsId" },
        id: BuildColumnIds.UniqueId,
        NameHash: BuildColumnIds.UniqueId,
        OldUniqueId: OldBuildColumnIds.UniqueId //兼容老数据
    }, {
        header: RMResx.RM_PRM_PRE_MRR_Column_Type,
        width: [200],
        resizeable: true,
        id: BuildColumnIds.Type,
        NameHash: BuildColumnIds.Type
    }, {
        header: RMResx.RM_PRM_PRE_Column_DisposalClass,
        width: [200],
        resizeable: true,
        id: BuildColumnIds.Classification,
        NameHash: BuildColumnIds.Classification
    },
    {
        header: RMResx.RM_PRM_PRE_Column_RuleName,
        width: [200],
        resizeable: true,
        id: BuildColumnIds.RuleName,
        NameHash: BuildColumnIds.RuleName
    },
    {
        header: RMResx.RM_PRM_PRE_Column_RuleAction,
        width: [200],
        resizeable: true,
        id: BuildColumnIds.RuleAction,
        NameHash: BuildColumnIds.RuleAction
    }, {
        header: RMResx.RM_PRM_PRE_Column_DisposalStatus,
        width: [150],
        resizeable: true,
        sortable: true,
        id: BuildColumnIds.HoldStatus,
        NameHash: BuildColumnIds.HoldStatus,
        valuePath: { Name: "holdStatus" },
    },
    {
        header: RMResx.RM_PRM_PRE_Column_HoldBy,
        width: [150],
        resizeable: true,
        id: BuildColumnIds.HoldByUsersId,
        NameHash: BuildColumnIds.HoldByUsersId
    },
    {
        header: RMResx.RM_PRM_PRE_Column_HoldType,
        width: [150],
        resizeable: true,
        id: BuildColumnIds.HoldTitle,
        NameHash: BuildColumnIds.HoldTitle
    },
    {
        header: RMResx.RM_PRM_PRE_Column_HoldUntil,
        width: [150],
        resizeable: true,
        id: BuildColumnIds.HoldDueDate,
        NameHash: BuildColumnIds.HoldDueDate
    }, {
        header: getActionDueDateI18n(),
        headerTooltip: RMResx.RM_PRM_PRE_Column_ActionDueDate_Desc,
        width: [200],
        resizeable: true,
        id: BuildColumnIds.ActionDueDate,
        NameHash: BuildColumnIds.ActionDueDate
    }, {
        header: RMResx.RM_PRM_PRE_Column_RecordOwner,
        width: [150],
        resizeable: true,
        id: BuildColumnIds.Owners,
        NameHash: BuildColumnIds.Owners
    }, {
        header: RMResx.RM_PRM_PRE_Column_CreatedTime,
        width: [200],
        resizeable: true,
        sortable: true,
        id: BuildColumnIds.CreatedDateInfo,
        NameHash: BuildColumnIds.CreatedDateInfo,
        valuePath: { Name: "timeCreated" },
    }, {
        header: RMResx.RM_JS_BCM_Explorer_Datagrid_Declared,
        width: [150],
        resizeable: true,
        sortable: true,
        id: BuildColumnIds.DeclaredRecord,
        NameHash: BuildColumnIds.DeclaredRecord,
        valuePath: { Name: "declareAsRecord" },
    }, {
        header: RMResx.RM_JS_BCM_Explorer_Datagrid_RecordsLabel,
        width: [150],
        resizeable: true,
        sortable: true,
        id: BuildColumnIds.LockedByRecordLabel,
        NameHash: BuildColumnIds.LockedByRecordLabel,
        valuePath: { Name: "lockedByRecordLabel" },
    }, {
        header: RMResx.RM_PRM_PRE_Column_Creator,
        width: [150],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "createdBy" },
        id: BuildColumnIds.CreatedBy,
        NameHash: BuildColumnIds.CreatedBy
    }, {
        header: RMResx.RM_PRM_PRE_Column_Modifier,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "modifiedBy" },
        id: BuildColumnIds.ModifiedBy,
        NameHash: BuildColumnIds.ModifiedBy
    }, {
        header: RMResx.RM_PRM_PRE_Column_ModifiedTime,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "timeModified" },
        id: BuildColumnIds.ModifiedTime,
        NameHash: BuildColumnIds.ModifiedTime
    }, {
        header: RMResx.RM_PRM_PRE_Column_ArchivedTime,
        width: [200],
        resizeable: true,
        sortable: true,
        valuePath: { Name: "timeArchived" },
        id: BuildColumnIds.ArchivedTime,
        NameHash: BuildColumnIds.ArchivedTime
    }, {
        header: RMResx.RM_PRM_PRE_Column_HoldTypes_PersonalHold,
        width: [200],
        resizeable: true,
        sortable: false,
        id: BuildColumnIds.OnLoan,
        NameHash: BuildColumnIds.OnLoan,
    }, {
        header: RMResx.RM_PRM_PRE_Column_LoanBy,
        width: [200],
        resizeable: true,
        sortable: false,
        id: BuildColumnIds.LoanBy,
        NameHash: BuildColumnIds.LoanBy,
    }
]
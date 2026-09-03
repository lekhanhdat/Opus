const ActionTypes = {
    Create: "1",
    Destroyed: "2"
};

const ActionTypeNames = {
    1: RMResx.RM_JS_RC_TimeFrame_Create,
    2: RMResx.RM_JS_RC_TimeFrame_Destroyed
};

const RangeTypes = {
    "5D": "1",
    "1M": "2",
    "3M": "3",
    "6M": "4",
    "Custom": "5"
};

const AuditRangeTypes = {
    "5D": "0",
    "1M": "2",
    "3M": "3",
    "6M": "4",
    "Custom": "5"
};

const RangeNames = {
    1: RMResx.RM_RC_Audit_Range_5D,
    2: RMResx.RM_RC_Audit_Range_1M,
    3: RMResx.RM_RC_Audit_Range_3M,
    4: RMResx.RM_RC_Audit_Range_6M,
    5: RMResx.RM_RC_Audit_Range_Custom
};
export const ReportType = {
    ItemDueForDisposalReport: 1,
    BCSTermUsageReport: 2,
    CreationAndDestructionReport: 13,
    AvailableSpaceReport: 14,
    RestoreReport: 21,
    SPOActionAuditReport: 8000,
    StorageOptimizationReport: 8001
};
const PhysicalReportTypes = [
    "4000", "4001", "4100", "4101", "4102", "4103", "4104"
];
const ObjectLevel = {
    1: RMResx.RM_JS_Rule_ObjectLevel_Document,
    2: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection,
    3: RMResx.RM_JS_Rule_ObjectLevel_Site,
    4: RMResx.RM_JS_Rule_ObjectLevel_List,
    5: RMResx.RM_JS_Rule_ObjectLevel_Item,
    6: RMResx.RM_JS_Rule_ObjectLevel_PhysicalFile,
    7: RMResx.RM_JS_Rule_ObjectLevel_PhysicalRecord,
    8: RMResx.RM_JS_Rule_ObjectLevel_Folder,
    9: RMResx.RM_JS_Rule_ObjectLevel_Attachment,
    2200: RMResx.RM_JS_Rule_ObjectLevel_FSFile,
    2100: RMResx.RM_JS_Rule_ObjectLevel_FSFolder,
    5110: RMResx.RM_JS_Rule_ObjectLevel_ExchangeOnlineItem,
    7103: RMResx.RM_RDM_RecordDetails_DataType_BoxFolder,
    7104: RMResx.RM_RDM_RecordDetails_DataType_BoxFile,
    9300: RMResx.RM_Common_ObjectLevel_PhysicalBox,
    9250: RMResx.RM_PRM_PRE_TableItemType_Container,
    9400: RMResx.RM_Common_ObjectLevel_PhysicalFile,
    9500: RMResx.RM_PRM_PRE_TableItemType_Record,
    7202: RMResx.RM_JS_Rule_ObjectLevel_GoogleFolder,
    7203: RMResx.RM_JS_Rule_ObjectLevel_GoogleFile
};
const ManualApproval = {
    0: RMResx.RM_JS_Common_Pending,
    1: RMResx.RM_JS_Common_Yes,
    2: RMResx.RM_JS_Common_No,
};
const ExportTypeValue = {
    0: RMResx.RM_JS_RDM_CreateRule_ExportType_Autonomy,
    1: RMResx.RM_JS_RDM_CreateRule_ExportType_Concordance,
    2: RMResx.RM_JS_RDM_CreateRule_ExportType_EDRM,
    3: RMResx.RM_JS_RDM_CreateRule_ExportType_VEO,
    4: RMResx.RM_JS_RDM_CreateRule_ExportType_NAA,
    5: RMResx.RM_JS_RDM_CreateRule_ExportType_NARA
};
const Status = {
    0: RMResx.RM_JS_JMD_Status_Successful,
    1: RMResx.RM_JS_JMD_Status_Failed,
    2: RMResx.RM_JS_JMD_Status_Skipped,
};
const TermStatus = {
    0: RMResx.RM_JS_RC_ReportColumn_TermStatus_Avaliable,
    1: RMResx.RM_JS_RC_ReportColumn_TermStatus_Retired,
    2: RMResx.RM_JS_RC_ReportColumn_TermStatus_Invalid,
    3: RMResx.RM_JS_RC_ReportColumn_TermStatus_Removed

};

const ObjectLevelsForSP = [
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection, level: 2 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Site, level: 3 },
    { isChecked: true, value: RMResx.RM_Common_ObjectLevel_List, level: 4 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Document, level: 1 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Item, level: 5 },
    // {isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_PhysicalFile, level: 6},
    //{ isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_PhysicalRecord, level: 7 },
    { isChecked: true, value: RMResx.RM_Common_ObjectLevel_Folder, level: 8 }
];

const ObjectLevelsForLSP = [
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Document, level: 1 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Item, level: 5 },
];

const CreateAndDesObjectLevelsForSP = [
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Document, level: 1 },
    // {isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_PhysicalFile, level: 6},
];

const ObjectLevelsForExo = [
    { isChecked: true, value: "Exchange Online Item", level: 20 }
];
const ObjectLevelsForPhy = [
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Box, level: 9300 },
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_File, level: 9400 },
    // {isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Record, level: 9500},
    // {isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Container, level: 9250},
];

const CreationObjectLevelForPhysical = [
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Box, level: 9300 },
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_File, level: 9400 },
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Record, level: 9500 },
    { isChecked: true, value: RMResx.RM_PRM_PRE_TableItemType_Container, level: 9250 },
];

const ObjectLevelsForFS = [
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_FSFile, level: 2200 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_FSFolder, level: 2100 }
];

const ObjectLevelsForRestoreReport = [
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Document, level: 1 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Item, level: 5 },
    { isChecked: true, value: RMResx.RM_JS_Rule_ObjectLevel_Attachment, level: 9 },
]

const AuditReportFilterType = {
    Time: 0,
    User: 1,
    Role: 2,
    DocAveModule: 3,
    Object: 4,
    Action: 5,
    Status: 6
};

const AuditReportColumnInfo = [
    RMResx.RM_JS_RC_Audit_ViewBy_Option_Time,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_User,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_Module,
    // RMResx.RM_JS_RC_Audit_ViewBy_Option_Function,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_Action,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_Object,
    RMResx.RM_JS_RC_Audit_ManageCol_NewVal,
    RMResx.RM_JS_RC_Audit_ManageCol_OldVal,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_Status,
    RMResx.RM_JS_RC_Audit_ViewBy_Option_ClientIP
];

const AuditReportcolumnsWidth = [
    230, 200, 200, 300, 200, 200, 200, 200, 200
];

const DueShowReportColumnsInfo = [
    RMResx.RM_JS_RC_ReportColumn_ObjectLevel,
    RMResx.RM_JS_RC_ReportColumn_TitleOrName,
    RMResx.RM_JS_RC_ReportColumn_Url,
    RMResx.RM_JS_RC_ReportColumn_SiteCollectionTitle,
    RMResx.RM_JS_RC_ReportColumn_BCSTermName,
    RMResx.RM_JS_RC_ReportColumn_AppliedRuleName,
    RMResx.RM_JS_Rule_DisposalClass_Title,
    RMResx.RM_JS_RC_ReportColumn_RelatedRecords,
    RMResx.RM_JS_RC_ReportColumn_RelatedRecordsAction,
    RMResx.RM_JS_RC_ReportColumn_DisposalAction,
    RMResx.RM_JS_RC_ReportColumn_Status,
    RMResx.RM_JS_RC_ReportColumn_ManualApproval,
    RMResx.RM_JS_RC_ReportColumn_ExportType,
    RMResx.RM_JS_RC_ReportColumn_CreatedBy,
    RMResx.RM_JS_RC_ReportColumn_CreatedTime,
    RMResx.RM_JS_RC_ReportColumn_LastModifiedBy,
    RMResx.RM_JS_RC_ReportColumn_LastModifiedTime,
    RMResx.RM_JS_RC_ReportColumn_Comment,
];

const DueShowReportColumnsWidth = [
    200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200
];

const TermShowReportColumnsInfo = [
    RMResx.RM_JS_RC_ReportColumn_ObjectLevel,
    RMResx.RM_JS_RC_ReportColumn_TitleOrName,
    RMResx.RM_JS_RC_ReportColumn_Url,
    RMResx.RM_JS_RC_ReportColumn_BCSTermName,
    RMResx.RM_JS_RC_ReportColumn_TermStatus,
    RMResx.RM_JS_RC_ReportColumn_TermFullPath,
    RMResx.RM_JS_RC_ReportColumn_CreatedBy,
    RMResx.RM_JS_RC_ReportColumn_CreatedTime,
    RMResx.RM_JS_RC_ReportColumn_LastModifiedBy,
    RMResx.RM_JS_RC_ReportColumn_LastModifiedTime,
];

const TermShowReportColumnsWidth = [
    200, 200, 500, 200, 200, 200, 200, 200, 200, 200
];

const CreateAndDesShowReportColumnsInfo = [
    RMResx.RM_JS_RC_TimeFrame_Time,
    RMResx.RM_JS_RC_ReportColumn_ObjectLevel,
    RMResx.RM_JS_RC_ReportColumn_TitleOrName,
    RMResx.RM_JS_RC_ReportColumn_Url,
    RMResx.RM_PRM_PRE_Column_Type, // Type
    RMResx.RM_JS_RC_ReportColumn_CreateTime, // create time
    RMResx.RM_JS_RC_ReportColumn_LastModifiedTime,
    RMResx.RM_JS_RC_TimeFrame_Operation,
    RMResx.RM_JS_RC_TimeFrame_By,
    RMResx.RM_JS_RC_ReportColumn_RecordsID, // "Records ID"
    RMResx.RM_JS_RC_ReportColumn_BCSTermName,
    RMResx.RM_JS_MA_Grid_Rule,
    RMResx.RM_JS_Rule_DisposalClass_Title,
    RMResx.RM_JS_JMD_Grid_ApprovalStatus,
    RMResx.RM_JS_RC_ReportColumn_ApprovedBy
];

const RestoreShowReportColumnsInfo = [
    RMResx.RM_JS_RC_ReportColumn_TitleOrName,
    RMResx.RM_JS_RC_ReportColumn_Url,
    RMResx.RM_JS_RC_ReportColumn_Size,
    RMResx.RM_JS_RC_ReportColumn_RestoreBy,
    RMResx.RM_JS_RC_ReportColumn_JobId,
    RMResx.RM_JS_RC_ReportColumn_StartTime,
    RMResx.RM_JS_RC_ReportColumn_EndTime,
    RMResx.RM_JS_RC_ReportColumn_RestoreTo,
];

const RestoreShowReportColumnsWidth = [
    300, 400, 200, 200, 300, 300, 300, 300
];

const RestoreShowReportSortColumns = [
    RMResx.RM_JS_RC_ReportColumn_TitleOrName,
    RMResx.RM_JS_RC_ReportColumn_Size,
    RMResx.RM_JS_RC_ReportColumn_StartTime,
    RMResx.RM_JS_RC_ReportColumn_EndTime,
];

const CreateAndDesShowReportColumnsWidth = [
    200, 200, 300, 300, 200, 200, 200, 200, 200, 300, 300, 200, 300, 200, [300, 200]
];

const AvaSpaceShowReportReportColumnsInfo = [
    RMResx.RM_JS_RC_ReportColumn_LocationPath,
    RMResx.RM_JS_RC_ReportColumn_AvailableSpace,
    RMResx.RM_JS_RC_ReportColumn_LocationSize,
];

const AvaSpaceShowReportColumnsWidth = [
    500, 500, 300
];

const JobTypeMaxRange = {
    SP: 2000,
    EXO: 4000,
    PHY: 5000,
    FS: 5020,
    SPOnPrem: 6000,
    OneDrive: 6104,
    Google: 10299,
    Teams: 10320
};

const UserScopeType = {
    All: 1,
    Special: 2
};

const TreeScopeType = {
    All: 1,
    Special: 2
};

const AuditEventType = {
    All: -1,
    None: 0,
    CheckOut: 1,
    CheckIn: 2,
    View: 4,
    Delete: 8,
    Update: 16,
    Undelete: 32,
    Download: 64,
    Search: 128,
    CreateGroup: 256,
    DeleteGroup: 512,
    AddGroupMember: 1024,
    DeleteGroupMember: 2048,
    CreatePermissionLevel: 4096,
    DeletePermissionLevel: 8192,
    ChangePermissionLevel: 16384,
    BreakPermissionLevelInheritance: 32768,
    ChangePermission: 65536,
    InheritPermissionSetting: 131072,
    Copy: 262144,
    Move: 524288,
    ProfileChange: 1048576,
    SchemaChange: 2097152,
    ChildMove: 4194304,
    AuditMaskChange: 8388608,
    BreakPermissionInheritance: 16777216,
    EventsDeleted: 33554432,
    Custom: 67108864,
    ChildDelete: 134217728,
    FileFragmentWrite: 268435456,
    Others: 516685824,
    AppPermissionGrant: 536870912,
    AppPermissionRemoval: 1073741824,
};

const AuditObjType = {
    All: -1,
    SiteCollection: 1,
    Site: 2,
    List: 4,
    Folder: 8,
    ListItem: 16,
    Document: 32,
};

const ActionTypeCol = [
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_View,
        value: AuditEventType.View,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Delete,
        value: AuditEventType.Delete,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Restore,
        value: AuditEventType.Undelete,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Update,
        value: AuditEventType.Update,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Download,
        value: AuditEventType.Download,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Search,
        value: AuditEventType.Search,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_CheckIn,
        value: AuditEventType.CheckIn,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_CheckOut,
        value: AuditEventType.CheckOut,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_ProfileChange,
        value: AuditEventType.ProfileChange,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_SchemaChange,
        value: AuditEventType.SchemaChange,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_CreateGroup,
        value: AuditEventType.CreateGroup,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_DeleteGroup,
        value: AuditEventType.DeleteGroup,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_AddGroupMember,
        value: AuditEventType.AddGroupMember,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_DelGroupMember,
        value: AuditEventType.DeleteGroupMember,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_CreatePermissionLevel,
        value: AuditEventType.CreatePermissionLevel,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_ChangePermission,
        value: AuditEventType.ChangePermission,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_ChangePermissionLevel,
        value: AuditEventType.ChangePermissionLevel,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_DeletePermissionLevel,
        value: AuditEventType.DeletePermissionLevel,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_BreakPermission,
        value: AuditEventType.BreakPermissionLevelInheritance,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_InheritPermission,
        value: AuditEventType.InheritPermissionSetting,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ActionType_Others,
        value: AuditEventType.Others,
        checked: true,
        disabled: false,
    },
];

const ObjTypeCol = [
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_SiteCollection,
        value: AuditObjType.SiteCollection,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_Site,
        value: AuditObjType.Site,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_List,
        value: AuditObjType.List,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_ListItem,
        value: AuditObjType.ListItem,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_Folder,
        value: AuditObjType.Folder,
        checked: true,
        disabled: false,
    },
    {
        name: RMResx.RM_JS_RC_ActionAudit_ObjType_Document,
        value: AuditObjType.Document,
        checked: true,
        disabled: false,
    },
];

const ActionAuditShowReportColumnsWidth = [
    250, 250, 350, 250, 250, 250
];

const ActionAuditShowReportColumnsInfo = [
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_Time,
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_User,
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_Url,
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_ObjType,
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_Type,
    RMResx.RM_JS_RC_ActionAudit_ShowReportCol_Action,
];

const ArchivedSiteColumnsWidth = [
    250, 250, 350, 250, 250, 250
];

const ArchivedSiteColumnsInfo = [
    "^^Type",
    "^^Source URL",
    "^^Archived data size (KB)",
    "^^Created time",
    "^^Last modified time",
    "^^Archived time"
]; 


export {
    ActionTypes,
    ActionTypeNames,
    RangeTypes,
    AuditRangeTypes,
    RangeNames,
    PhysicalReportTypes,
    ObjectLevel,
    ManualApproval,
    ExportTypeValue,
    Status,
    TermStatus,
    ObjectLevelsForSP,
    ObjectLevelsForLSP,
    CreateAndDesObjectLevelsForSP,
    ObjectLevelsForExo,
    ObjectLevelsForPhy,
    ObjectLevelsForFS,
    ObjectLevelsForRestoreReport,
    AuditReportFilterType,
    AuditReportColumnInfo,
    AuditReportcolumnsWidth,
    DueShowReportColumnsInfo,
    DueShowReportColumnsWidth,
    TermShowReportColumnsInfo,
    TermShowReportColumnsWidth,
    CreateAndDesShowReportColumnsInfo,
    CreateAndDesShowReportColumnsWidth,
    AvaSpaceShowReportReportColumnsInfo,
    AvaSpaceShowReportColumnsWidth,
    JobTypeMaxRange,
    CreationObjectLevelForPhysical,
    UserScopeType,
    TreeScopeType,
    AuditEventType,
    AuditObjType,
    ActionTypeCol,
    ObjTypeCol,
    ActionAuditShowReportColumnsWidth,
    ActionAuditShowReportColumnsInfo,
    RestoreShowReportColumnsInfo,
    RestoreShowReportColumnsWidth,
    RestoreShowReportSortColumns,
    ArchivedSiteColumnsWidth,
    ArchivedSiteColumnsInfo
};
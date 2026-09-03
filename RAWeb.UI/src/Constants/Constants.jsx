export const EmptyGUID = "00000000-0000-0000-0000-000000000000";

export const GridCellType = {
    SelectAll: 'SelectAll',
    Select: 'Select',
    LinkCell: 'LinkCell',
    TextCell: 'TextCell',
    ButtonCell: 'ButtonCell'
};

export const GridCellButtonType = {
    IconLink: 'IconLink',   //default type
    Switch: 'Switch'
};

export const RuleOperatedType = {
    Canceled: 'canceled',
    Created: 'created',
    Edited: 'edited'
};

export const NewLogicDisposalAction = {
    //main option
    32768: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage,

    //sub option
    16384: RMResx.RM_RDM_CreateRule_Options_IncludeRetentionLabels, //RMContentDisposalAction.IsEnableRemoveRetentionLabel
    65536: RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile
};

export const NewLogicMainOptionSet = {
    32768: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage
};

export const DisposalAction = {
    0: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove,
    1: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep,
    2: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    3: RMResx.RM_JS_RDM_CreateRule_Options_MoveLocation,
    99: RMResx.RM_JS_RDM_CreateRule_Options_None,
    4: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord,
    8: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord,
    10: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " +
        RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    16: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    18: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " +
        RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    24: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " +
        RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    26: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " +
        RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    32: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_DestroyEmptyBox,
    40: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " +
        RMResx.RM_RDM_CreateRule_DestroyEmptyBox,
    64: RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly,

    // Main option for phy
    2097152: RMResx.RM_JS_RDM_CreateRule_Options_CalculateDisposalDate,
};

export const SPDisposalAction = {
    0: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove,
    1: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep,
    2: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    3: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord,
    4: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord,
    5: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    7: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    8: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord,
    10: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    11: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_BCM_Rule_Move_IsRemoveEmail,
    13: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    15: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    16: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    18: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    19: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_BCM_Rule_Move_IsReclassify,
    20: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord + "; " + RMResx.RM_JS_BCM_Rule_Move_IsReclassify,
    21: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    23: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    24: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    25: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage,    
    26: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    28: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    27: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_BCM_Rule_Move_IsRemoveEmail + "; " + RMResx.RM_JS_BCM_Rule_Move_IsReclassify,
    29: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    31: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    40: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions,
    41: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions,
    42: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions,
    43: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure,
    44: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_AllVersions,
    45: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_FolderStructure,
    64: RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly,
    130: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    135: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    138: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub, 
    143: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    146: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    151: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    154: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    156: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,
    159: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub + "; " + RMResx.RM_JS_RDM_CreateRule_Options_DeclareStub,    
    4096: RMResx.RM_JS_RDM_CreateRule_Options_BackupAndRemove,
    8192: RMResx.RM_JS_RDM_CreateRule_Options_BackupAndRemove + ";" + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub,
    99: RMResx.RM_JS_RDM_CreateRule_Options_None,
};

export const FSDisposalAction = {
    0: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove,
    1: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndKeep,
    2: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    3: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord_FS,
    4: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord_FS + "; " + RMResx.RM_JS_RDM_CreateRule_Options_Move_DeclareRecord,
    5: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    7: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    8: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord,
    10: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    13: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    15: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    16: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    18: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    21: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    23: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    24: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile,
    25: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage,
    26: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    29: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying,
    31: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove + "; " + RMResx.RM_JS_Rule_Detail_IncludeRelatedRecord + "; " + RMResx.RM_JS_Rule_Detail_IncludeDeclaredFile + "; " + RMResx.RM_RDM_CreateRule_BackupBeforeDestroying + "; " + RMResx.RM_JS_RDM_CreateRule_Options_LeaveStub_FS,
    64: RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly,
    99: RMResx.RM_JS_RDM_CreateRule_Options_None,
};
//google disposal action
export const GoogleDisposalAction = {
    0: RMResx.RM_JS_RDM_CreateRule_Options_ArchiveAndRemove,
    3: RMResx.RM_JS_RDM_CreateRule_Options_MoveRecord,
    64: RMResx.RM_JS_RDM_CreateRule_Options_ExportOnly,
    25: RMResx.RM_RDM_CreateRule_ArchiveToAzureBlobStorage,
    99: RMResx.RM_JS_RDM_CreateRule_Options_None,
}


export const PhysicalObjectColumnType = {
    SingleText: 1,
    MutipleText: 2,
    DateTime: 3,
    SingleChoice: 4,
    PeopleOrGroup: 5,
    Number: 6,
    MultipleChoice: 7,
    Taxonomy: 10,
    YesOrNo: 12,
};

export const SourceFlags = {
    None: 0,
    SP: 1,
    FS: 2,
    Exo: 3,
    Phy: 4,
    SPLocal: 5,
    OneDrive: 6,
    AzureFile: 7,
    Box: 8,
    Google: 9,
    // Salesforce is 10
    Teams: 11,
};

export const AgentSourceType = {
    // None: 0,
    FileSystem: 1,
    SharePoint: 2,
};

export const PhysicalDefaultColumnIDs = {
    NameOrTitle: "de5e99cb-4fb4-4e25-b732-a1dce71dd048",
    Description: "dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81",
    Status: "eb4e9ab7-c939-425b-9e29-235236c9ce5b",
    Format: "9333da20-6a70-4a4e-9013-33ee8f0539cd",
    ProtectiveMarking: "01aefeb6-6cb1-419f-b476-fee650045778",
    Rights: "e56dcd2e-fbb7-4e31-bfff-5347d538f88e",
    Coverage: "fe3dce03-2e91-400a-8b6d-446b6ef8820e",
    DataClosed: "99b6d3fb-688d-4d19-9cfb-2a3f70c07aa9",
    HomeLocation: "d2568d7d-4891-46d2-8eb2-2e8c032a41bf",
    Capability: "8951a839-c8df-4bfe-8dfb-de204297629d",
    Path: "8951a839-c8df-4bfe-8dfb-de204297629e",
    Classification: "aedcf21f-dfdb-41d3-935a-5c5859187754",
    UniqueId: "c980eb95-ea92-4f07-9f97-1a8ab2a053fa",
    CreatedBy: "cf054564-6482-4ff4-95ad-2a84ab3f8262",
    ModifiedBy: "16aa21f3-88a4-47ed-ae4c-07ea2e5c0e45",
    LoanedBy: "df21d79c-bc37-fdfd-f59e-641f7d630488",
};

export const PhysicalDefaultArray = ["de5e99cb-4fb4-4e25-b732-a1dce71dd048", "dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81", "eb4e9ab7-c939-425b-9e29-235236c9ce5b",
    "9333da20-6a70-4a4e-9013-33ee8f0539cd", "01aefeb6-6cb1-419f-b476-fee650045778", "e56dcd2e-fbb7-4e31-bfff-5347d538f88e", "fe3dce03-2e91-400a-8b6d-446b6ef8820e",
    "99b6d3fb-688d-4d19-9cfb-2a3f70c07aa9", "d2568d7d-4891-46d2-8eb2-2e8c032a41bf", "8951a839-c8df-4bfe-8dfb-de204297629d",
    "8951a839-c8df-4bfe-8dfb-de204297629e", "aedcf21f-dfdb-41d3-935a-5c5859187754", "c980eb95-ea92-4f07-9f97-1a8ab2a053fa",
    "cf054564-6482-4ff4-95ad-2a84ab3f8262", "16aa21f3-88a4-47ed-ae4c-07ea2e5c0e45", "df21d79c-bc37-fdfd-f59e-641f7d630488"];


export const PhysicalDefaultColumnNames = {
    "de5e99cb-4fb4-4e25-b732-a1dce71dd048": RMResx.RM_PRM_PRE_Column_Name,
    "dd5b8a0a-96d7-42c0-ba1b-a6cbef4bdb81": RMResx.RM_PRM_PRE_Column_Description,
    "8951a839-c8df-4bfe-8dfb-de204297629e": RMResx.RM_PRM_PRE_Column_FullLocation,
    "8951a839-c8df-4bfe-8dfb-de204297629d": RMResx.RM_PRM_PRE_Column_Capacity,
};

export const PhysicalObjectStatus = {
    Open: 1,
    Destroyed: 2,
    Closed: 6,
    Missing: 7
};

export const PhysicalObjectStatusNames = {
    '1': RMResx.RM_PRM_PRE_Column_Status_Open,
    '2': RMResx.RM_PRM_PRE_Column_Status_Destroyed,
    '6': RMResx.RM_PRM_PRE_Column_Status_Closed,
    '7': RMResx.RM_PRM_PRE_Column_Status_Missing,
};

export const PhysicalObjectHoldType = {
    None: 0,
    PersonalHold: 1,
    DisposalHold: 2
};

export const PhysicalDefaultColumnHoldTypeNames = {
    '0': RMResx.RM_PRM_PRE_Column_HoldTypes_None,
    '1': RMResx.RM_PRM_PRE_Column_HoldTypes_PersonalHold,
    '2': RMResx.RM_PRM_PRE_Column_HoldTypes_DisposalHold,
};

export const UserTypeNames = {
    '0': RMResx.RM_CP_AccountManagement_StandardUser,
    '1': RMResx.RM_CP_AccountManagement_Admin
};

export const JobType = {
    None: -1,
    TermSynchronization: 0,
    ItemsFilesDueDisposal: 1,
    BCSTermUsageReport: 2,
    SharePointGlobalSetting: 3,
    SharePointCustomSetting: 4,
    SharePointInheritSetting: 5,
    OrphanedTermReport: 6,
    SharePointScheduleSetting: 7,//SharePoint Setting Schedule job include new logic
    PhysicalTermSynchronization: 8,
    PhysicalFolderSynchronization: 9,
    TermDeletion: 10,
    UpdateLocation: 11,
    ImportPhysicalRecords: 12,
    CreateAndDestroyedFileReport: 13,
    AvailableSpaceReport: 14,
    ManualApproval: 15,
    ManualApprovalLocationTest: 16,
    ApplySharePointSettings: 18,// to do next Validate Job Type
    RetiredTermReport: 19,
    DisposalActivityManagement: 20,
    ArchiverScan: 24,
    MigrationArchiverRestore: 28,
    ArchiverBackup: 29,
    ExportToLocation: 30,
    MigrationArchiverRetention: 35,
    //40 ~ 50 UniqueIDSetting
    UniqueIDSettingFullSchedule: 40,
    UniqueIDSettingIncrementalSchedule: 41,
    ReportAfterDataSync: 49,
    CollectionDataFull: 50,
    DataSynchronisation: 501,//TODO CollectionDataFull-->DataSynchronisation
    CollectionDataIncremental: 51,
    ManualApprovalTimer: 52,
    EnforceRetention: 53,
    OldEnforceRetention: 54,
    //60-70 Records Explorer Update.
    UpdateTerms: 60,
    DeclaredRecords: 61,
    UndeclaredRecords: 62,
    ExportSiteMetrics: 82,

    ImportTermStructure: 100,
    ExportTermStructure : 103,
    All: 111,

    RecordsExplorerMove: 1005,

    //EXO Job Type
    ExchangeArchiverScan: 124,
    ExchangeArchiverBackup: 125,
    EXOApplySetting: 2000,
    EXODataSynchronisation: 2001,
    EXOItemsFilesDueDisposalReport: 2100,
    EXOTermUsageReport: 2101,
    EXOOrphanedTermUsageReport: 2102,
    EXORetiredTermUsageReport: 2103,
    EXOCreateAndDestroyedFileReport: 2104,
    EXOApplySettingSchedule: 2105,

    EXOEnforceRetention: 2153,

    SPDataSynchronisationSchedule: 3000,
    EXODataSynchronisationSchedule: 3001,

    //New Physical Disposal
    PhysicalDisposal: 4000,
    PhysicalExplorerTimer: 4001,
    PhysicalItemsFilesDueDisposalReport: 4100,
    PhysicalTermUsageReport: 4101,
    PhysicalOrphanedTermUsageReport: 4102,
    PhysicalRetiredTermUsageReport: 4103,
    PhysicalCreateAndDestroyedFileReport: 4104,
    //New Physcial Disposal Report job.

    //5000+ File System Report
    FSItemsFilesDueDisposal: 5004,
    FSCreateAndDestroyedFileReport: 5006,
    FSBCSTermUsageReport: 5010,
    FSOrphanedTermReport: 5011,
    FSRetiredTermReport: 5012,
    SPOnPremEnforceRuleAction: 5507,
    SPOnPremTermSynchronization: 5600,
    SyncSecurityContainer: 6000,

    OneDriveTermUsageReport: 6100,
    OneDriveOrphanedTermReport: 6101,
    OneDriveRetiredTermReport: 6102,
    OneDriveItemsFilesDueDisposal: 6103,
    OneDriveCreateAndDestroyedFileReport: 6104,

    SPOnPremiseItemsFilesDueDisposal: 5510,
    SPOnPremiseCreateAndDestroyedFileReport: 5511,
    SPOnPremiseTermUsageReport: 5512,
    SPOnPremiseOrphanedTermUsageReport: 5513,
    SPOnPremiseRetiredTermUsageReport: 5514,

    DisposalReport: 6200,
    TermUsageReport: 6201,
    CreateAndDestroyedReport: 6202,

    // 8000- 8019 ActionAuditReport
    SPOActionAuditReport: 8000,
    OneDriveActionAuditReport: 8019,

    RMEndUserArchiverBackup: 8069,

    RestoreReport: 21,
    OneDriverRestoreReport: 6113,

    BoxItemsFilesDueDisposal: 10102,
    BoxCreateAndDestroyedFileReport: 10103,
    BoxBCSTermUsageReport: 10104,
    BoxOrphanedTermUsageReport : 10105,
    BoxRetiredTermUsageReport: 10106,

    // Google Drive
    LabelSyncFromGoogle : 10200,
    LabelSyncToGoogle : 10201,
    GoogleDriveCreateAndDestroyedFileReport: 10205,
    GoogleDriveItemsFilesDueDisposal: 10208,
    GoogleBCSTermUsageReport: 10209,
    GoogleOrphanedTermUsageReport: 10210,
    GoogleRetiredTermUsageReport: 10211,
    GoogleRestoreReport: 10214,

    // Teams
    TeamsCreateAndDestroyedFileReport: 10305,
    TeamsItemsFilesDueDisposalReport: 10306,
    TeamsBCSTermUsageReport: 10307,
    TeamsOrphanedTermUsageReport: 10308,
    TeamsRetiredTermUsageReport: 10309,
    TeamsActionAuditReport: 10310,
    TeamsRestoreReport: 10311,
    TeamsUniqueIDSettingFullSchedule: 10315,
    TeamsUniqueIDSettingIncrementalSchedule: 10316,

    //Archived Site
    ArchivedSiteReportSharePointOnline: 11600,
    ArchivedSiteReportSOneDrive: 11601,
    ArchivedSiteReportGoogle: 11602,
    ArchivedSiteReportTerm: 11603
};

export const SourceTypeI18N = {
    1: RMResx.RM_JS_SPS_TabLabel_SP,
    2: RMResx.RM_JS_SPS_TabLabel_FS,
    3: RMResx.RM_JS_SPS_TabLabel_EXO,
    4: RMResx.RM_JS_SPS_TabLabel_Physical,
    5: RMResx.RM_JS_SPS_TabLabel_SPLocal,
    6: RMResx.RM_JS_SPS_TabLabel_OneDrive,
    7: RMResx.RM_JS_SPS_TabLabel_AZS,
    8: RMResx.RM_JS_SPS_TabLabel_Box,
    9: RMResx.RM_JS_SPS_TabLabel_GoogleDrive,
    11: RMResx.RM_JS_SPS_TabLabel_Teams,
};

export const SourceType = {
    SP: 1,
    FS: 2,
    EXO: 3,
    PHY: 4,
    SPLocal: 5,
    OneDrive: 6
};

export const DefaultSecurityGroupId = [1, 2];
export const DefaultSecurityGroup = {
    BuiltInAdmin: 1,
    BuiltInEndUser: 2
};
export const RoleType = {
    StandardUser: 0,
    SupAdmin: 1,
    DelegateAdmin: 2,
    ReviewUser: 3,
    StandardReviewUser: 4,
    ManageHoldUser: 5
};
export const PanelDisplayMode = {
    Create: 0,
    Edit: 1,
    View: 2
};

export const TermObjType = {
    Root: 1,
    TermGroup: 2,
    TermSet: 3,
    Term: 4
};

export const RuleObjType = {
    Root: 1,
    RuleContainer: 2,
    Rule: 3
};

export const SecurityGroupValidateType = {
    SourceContainerConflict: 1,
    TermConflict: 2,
    RuleConflict: 3,
    TermAssociationRuleMissing: 4,
    RuleAssociationTermMissing: 5,
    RuleAssociationNodeMissing: 6,
};

export const SetTermPermissionMethod = {
    None: 0,
    All: 1,
    SpecifyScope: 2
};

export const RulePermissionMethod = {
    None: 0,
    All: 1,
    SpecifyScope: 2
};

export const TypeString = {
    ROOT: "Root",
    TERM_GROUP: "TermGroup",
    TERM_SET: "TermSet",
    TERM: "Term",
    SUB_TERM: "SubTerms",
    BOXES: "Boxes",
    FILES: "Files",
    PhyBox: "PhyBox",
    PhyFile: "PhyFile",
    Label: "Label"
};

export const TelemetryModule = {
    None: 0,
    HomePage: 1,
    ContentRepositoryManagement: 2,
    RecordsExplorer: 3,
    PhysicalRecordsExplorer: 4,
    ReportCenter: 5,
    GlobalSearch: 6,
    JobMonitor: 7,
    TermManagement: 8,
    RuleManagement: 9,
    AgentManagement: 10,
    Dashboard: 11,
};

export const TelemetryEventType = {
    None: 0,
    HomepageLoaded: 1,
    ContentPageLoaded: 2,
    ApplySettings: 3,
    RunEnforceRuleActions: 4,
    Search: 5,
    Filter: 6,
    DashboardLoaded: 7,
    CreateContentDueProfile: 8,
    CreateCreationAndDestructionProfile: 9,
    ViewAuditReport: 10,
    RunJob: 11,
    TermSynchronise: 12,
    LoanRequest: 13,
    RecordCreationRequest: 14,
    BoxCreationRequest: 15,
    FolderCreationRequest: 16,
    RuleAdded: 17,
    RuleModified: 18,
    RuleDeleted: 19,
    ActionAuditProfile: 20,
    MonitorFailedJob: 21,
    MonitorLongRunningJob: 22,
    MonitorSpecificExceptionJob: 23,
    MonitorAgentStatus: 24
};

export const PermissionManageModule = {
    None: 0,
    PhysicalExplorer: 1
};

export const SubPermission = {
    None: 0,
    SetAccessControl: 1,
    BoxCreationRequest: 2,
    FolderCreationRequest: 3,
    FolderLoanRequest: 4,
    FolderLoanReturn: 5,
    SubmitMoveRequest: 6
};

export const PhyUserRoleType = {
    None: 0,
    Admin: 1,
    EndUser: 2
};

export const TreeType = {
    Filter: 1,
    Move: 2,
    Report: 3,
    ActionReport: 4,
};

export const PickListForLoanStatusType = {
    Pendding: 0,
    Complete: 1
};

export const PickListForDestroyStatusType = {
    Pendding: 0,
    Complete: 1
};

export const PickListForMoveStatusType = {
    PendingMove: 1,
    Failed: 2
};

export const TooptipByApiCellType = {
    Term: 1,
    HomeLocation: 2,
    PredictTerm: 3
};

export const ArchivedContentFileType = {
    None: 0,
    Zip: 1
};

export const CRComponentType = {
    RM: 1,
    TM: 2,
    EXOSetting: 3,
    OnedriveSetting: 4,
    SPSetting: 5,
    LabelManagement: 6,
    TeamsSetting: 7,
};

export const SecurityGroupMenuItems = [
    { displayName: RMResx.RM_JS_Common_Edit, index: 1 },
    { displayName: RMResx.RM_JS_Common_Delete, index: 2 },
    { displayName: RMResx.RM_CP_AM_Action_View, index: 3 }
];

export const SourceIcons = {
    [SourceFlags.SP]: 'fi-ms-sharepoint',
    [SourceFlags.FS]: "fia-fs",
    [SourceFlags.Exo]: 'fi-ms-exchange',
    [SourceFlags.Phy]: 'fia-physical-record',
    [SourceFlags.SPLocal]: "fia-sharepoint",
    [SourceFlags.OneDrive]: "fi-ms-onedrive",
    [SourceFlags.AzureFile]: "fi-ms-azure-file-share",
    [SourceFlags.Box]: "fia-box-blue-b",
    [SourceFlags.Google]: "fia-google-drive-f",
    [SourceFlags.Teams]: "fi-ms-teams",
};

export const HomePageType = {
    OpusSOOnly: 1,
    OpusAll: 2,
    OpusDiscoveryOnly: 3,
    RestoreCenterOnly: 4,
    HoldOnly: 5,
};

export const LicenseType = {
    Trial: 0,
    Enterprise: 1,
    Internal: 2,
};

export const BrowseTreeNodeSourceType = {
    All : 0,
    SharePointOnline : 1,
    OneDrive : 2,
};

export const DefaultExportLimit = 5000;

export const StorageTypeIndex = {
    Amazon: 401,
    S3Compatible: 601,
    WasabiS3Compatible: 602,
    Box: 9,
    Dropbox: 407,
    FTP: 1,
    AzureBlob: 403,
    NetApp_Alta_Vault: 510,
    Rackspace: 402,
    SFTP: 12,
    Google: 14,
};

export const PermissionSettingType = {
    DataScope: 0,
    FunctionMoudle: 1,
};

export const RestoreCenterType = {
    None: 0,
    FullControl: 1,
    SearchAndExport: 2,
    SearchOnly: 3,
};

export const RestoreCenterTypeTitle = {
    0: RMResx.RM_JS_Common_None,
    1: RMResx.RM_CP_AM_SubPermission_FullControl,
    2: RMResx.RM_CP_AM_SubPermission_SearchAndExport,
    3: RMResx.RM_CP_AM_SubPermission_SearchOnly
}

export const AgentStatus = {
    NotInstalled: 0,
    InActive: 1,
    Active: 2,
    Deleted: 3,
    Disabled: 4,
    Mismatched: 5,
    ActiveException: 6,
    Upgrading: 7
};

export const OpusExternalRequestType = {
    BuildPlanOpus: 1,
    AskYourData: 2,
    OptimizeCurrentPlan: 3,
    OpusViewHistoryPlan: 4
};

export const ExternalRequestProductType = {
    Opus: "Opus"
};
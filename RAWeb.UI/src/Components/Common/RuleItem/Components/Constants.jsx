export const RuleModuleTypes = {
    None: 0,
    Records: 1,
    SOArchiver: 2
};

export const levels = [
    {id: 64, Name: RMResx.RM_JS_Rule_ObjectLevel_Document},
    {id: 32, Name: RMResx.RM_JS_Rule_ObjectLevel_Item},
    {id: 16, Name: RMResx.RM_JS_Rule_ObjectLevel_Folder},
    {id: 8, Name: RMResx.RM_JS_Rule_ObjectLevel_List},
    {id: 4, Name: RMResx.RM_JS_Rule_ObjectLevel_Site},
    {id: 2, Name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection},
];

export const ArchiveLevelOptions = [
    {id: 64, Name: RMResx.RM_JS_Rule_ObjectLevel_Document},
    {id: 256, Name: RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion},
    {id: 32, Name: RMResx.RM_JS_Rule_ObjectLevel_Item},
    {id: 512, Name: RMResx.RM_JS_Rule_ObjectLevel_ItemVersion},
    {id: 128, Name: RMResx.RM_JS_Rule_ObjectLevel_Attachment},
    {id: 16, Name: RMResx.RM_JS_Rule_ObjectLevel_Folder},
    {id: 8, Name: RMResx.RM_JS_Rule_ObjectLevel_List},
    {id: 4, Name: RMResx.RM_JS_Rule_ObjectLevel_Site},
    {id: 2, Name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection},
];

export const TeamsArchiveLevelOption = {
    id: 33554432, Name: RMResx.RM_JS_Rule_ObjectLevel_Teams
};

export const RuleLevel = {
    Teams: 33554432,
    ItemVersion: 512,
    DocumentVersion: 256,
    Attachment: 128,
    Document: 64,
    Item: 32,
    Folder: 16,
    List: 8,
    Site: 4,
    SiteCollection: 2,
};

export const RuleLevelOptions = {
    [RuleModuleTypes.Records]: levels,
    [RuleModuleTypes.SOArchiver]: ArchiveLevelOptions,
};

export const RuleSourceTabIndex = 
{
    SP: 0,
    OneDrive: 1,
    Exchange: 2,
    Physical :3,
    FS: 4,
    SPLocal: 5,
    AzureFile: 6,
    Box: 7,
    Connector: 8,
    GoogleDrive: 9,
    Teams: 10,
};

export const RecordsRuleSources = {
    2: [RuleSourceTabIndex.SP],
    4: [RuleSourceTabIndex.SP],
    8: [RuleSourceTabIndex.SP, RuleSourceTabIndex.Physical],
    16: [RuleSourceTabIndex.SP, RuleSourceTabIndex.Physical],
    32: [RuleSourceTabIndex.SP, RuleSourceTabIndex.SPLocal],
    64: [RuleSourceTabIndex.SP, 
        RuleSourceTabIndex.OneDrive, 
        RuleSourceTabIndex.Exchange, 
        RuleSourceTabIndex.FS, 
        RuleSourceTabIndex.SPLocal, 
        RuleSourceTabIndex.AzureFile, 
        RuleSourceTabIndex.Box,
        RuleSourceTabIndex.GoogleDrive,
        RuleSourceTabIndex.Connector,
    ]
};

export const ArchiveRuleSources = {
    2: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    4: [RuleSourceTabIndex.SP],
    8: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    16: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    32: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    64: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    128: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    256: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    512: [RuleSourceTabIndex.SP, RuleSourceTabIndex.OneDrive],
    33554432: [RuleSourceTabIndex.Teams],
};

export const SourcesByRuleLevel = {
    [RuleModuleTypes.Records]: RecordsRuleSources,
    [RuleModuleTypes.SOArchiver]: ArchiveRuleSources
};

export const sharePointCriteriaTabs = [
    {title: RMResx.RM_JS_SPS_TabLabel_SP},
    {title: RMResx.RM_JS_SPS_TabLabel_EXO },
    {title: RMResx.RM_JS_SPS_TabLabel_Physical},
];
export const TagType = {
    Text: 1,
    Nubmer: 2,
    DateTime: 3,
    YesNo: 4,
    Archived: 5,
    ArchivedBy: 6,
    ArchivedDate: 7,
    RetentionLabel:8
};
export const TagMode = {
    ColumnName: "",
    DateTime: new Date(),
    IsDayLightSaving: false,
    StartTimeForGui: new Date(),
    TimeZoneId: "",
    Type: 0,
    Value: ""
};
export const ConfigMissingLinks = [
    RMResx.RM_ES_VEO_ConfigMissingLink,
    RMResx.RM_ES_NAA_ConfigMissingLink,
    RMResx.RM_ES_NARA_ConfigMissingLink
];
export const exportTypeFilted = [
    {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_VEO}
];
export const exportTypeAll = [
    {id: -1, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_None},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_VEO},
    {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_NAA},
    {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_ExportType_NARA}
];
export const tagType = [
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_TagType_Text},
    {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_TagType_Nubmer},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_TagType_DateTime},
    {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_TagType_YesNo},
    {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_TagType_Archived},
    {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_TagType_ArchivedBy},
    {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_TagType_ArchivedDate}
];
export const yesOrNo = [{ id: 0, Name: RMResx.RM_JS_Common_Yes }, { id: 1, Name: RMResx.RM_JS_Common_No}];
export const Matchs1 = [
    {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains},
    {id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains},
    {id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths},
    {id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath},
    {id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
    {id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot}
];

// For Teams 
export const TeamsPrivacy = [
    {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Public},
    {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Private},
];

export const TeamsStatus = [
    {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Active},
    {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Archived},
];

export const TeamsType = [
    {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_StandaloneM365Group},
    {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_TeamsEnabledM365Group},
    {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_VivaEngageM365Group},
];

export const KeepVersionConditions = [
    {id: 8413952, Name: RMResx.RM_JS_RDM_CreateRule_KeepVersion_MajorAndMinor},
    {id: 33554432, Name: RMResx.RM_JS_RDM_CreateRule_KeepVersion_MajorNoMinor},
    {id: 67108864, Name: RMResx.RM_JS_RDM_CreateRule_KeepVersion_MinorEachMajor},
    {id: 134217728, Name: RMResx.RM_JS_RDM_CreateRule_KeepVersion_MinorLatestMajor},
];

const archiveRuleTypes = {
    512:[
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 16, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_KeepVersion},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
    ],
    256: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 61, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentModified},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified}, // Version modified time
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 16, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_KeepVersion},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
    ],
    128: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
    ],
    32: [
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList}
    ],
    16: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 75, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_OrphanedFolderRule},
    ],
    8: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 18, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TextCustomProperty},
        {id: 19, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_NumberCustomProperty},
        {id: 20, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_BooleanCustomProperty},
        {id: 21, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DateTimeCustomProperty}
    ],
    4: [
        {id: 17, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_URL},
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 18, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TextCustomProperty},
        {id: 19, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_NumberCustomProperty},
        {id: 20, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_BooleanCustomProperty},
        {id: 21, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DateTimeCustomProperty}
    ],
};

const level64Type4IL = [
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
    {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
    {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
    {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
    {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
    {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
    {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
    {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
    {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
    {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
    {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
    {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
    {id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime},
    {id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    {id: 38, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibraryName},
    {id: 46, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderNameHeirarchically },
    {id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    {id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    {id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    {id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    {id: 70, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagText },
    {id: 71, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagNumber },
    {id: 72, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagBoolean },
    {id: 73, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagDateTime },
    {id: 66, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText },
    {id: 67, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber },
    {id: 68, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean },
    {id: 69, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime },
    {id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    {id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn },
    {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
    {id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName},
    {id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName },
];

const level64Type4SO = [
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
    {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
    {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
    {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
    {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
    {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
    {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
    {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
    {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
    {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
    {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
    {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
    {id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime},
    {id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    {id: 38, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibraryName},
    {id: 46, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderNameHeirarchically },
    {id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    {id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    {id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    {id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    {id: 70, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagText },
    {id: 71, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagNumber },
    {id: 72, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagBoolean },
    {id: 73, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagDateTime },
    {id: 66, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText },
    {id: 67, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber },
    {id: 68, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean },
    {id: 69, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime },
    {id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    {id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn },
    {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
    {id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName},
    {id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName},
];

const level64Type421V = [
    { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
    { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
    { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
    { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
    { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy },
    { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy },
    { id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType },
    { id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText },
    { id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber },
    { id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean },
    { id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime },
    { id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList },
    { id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime },
    { id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime },
    { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    { id: 38, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibraryName },
    { id: 46, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderNameHeirarchically },
    { id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    { id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    { id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    { id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    { id: 70, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagText },
    { id: 71, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagNumber },
    { id: 72, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagBoolean },
    { id: 73, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagDateTime },
    { id: 66, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText },
    { id: 67, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber },
    { id: 68, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean },
    { id: 69, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime },
    { id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    { id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn },
];

export const rulTypes = {
    ...archiveRuleTypes,
    2: [
        {id: 17, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_URL},
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 22, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PrimaryAdministrator},
        {id: 23, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SiteCollectionSizeTrigger},
        {id: 18, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TextCustomProperty},
        {id: 19, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_NumberCustomProperty},
        {id: 20, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_BooleanCustomProperty},
        {id: 21, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DateTimeCustomProperty}
    ],
    64: level64Type4IL,
};

export const rulTypes21V = {
    ...rulTypes,
    64: level64Type421V,
}

export const rulTypes4SO = {
    ...archiveRuleTypes,
    2: [
        {id: 17, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_URL},
        {id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        {id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime},
        {id: 22, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_PrimaryAdministrator},
        {id: 23, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SiteCollectionSizeTrigger},
        {id: 18, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TextCustomProperty},
        {id: 19, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_NumberCustomProperty},
        {id: 20, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_BooleanCustomProperty},
        {id: 21, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DateTimeCustomProperty}
    ],
    256: [
        ...archiveRuleTypes[256],
        {id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    ]
}

export const rulTypes4SO21V = {
    ...rulTypes4SO,
    64: level64Type421V,
};

export const rulTypes4SONormal = {
    ...rulTypes4SO,
    64: level64Type4SO,
    256: [
        ...rulTypes4SO[256],
        {id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocSensitiveLabel},
    ]
};

export const SPLocalRulTypes = {
    64: [{id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList},
        { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    ],
    32: [{id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList}]
};

const level64Type4ILOneDrive = [
    { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
    { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
    { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
    { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
    { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy },
    { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy },
    { id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType },
    { id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText },
    { id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber },
    { id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean },
    { id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime },
    { id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList },
    { id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime },
    { id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime },
    { id: 38, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibraryName },
    { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    { id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    { id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    { id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    { id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    { id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel },
    { id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName },
    { id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName },
];

const level64Type4SOOneDrive = [
    { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
    { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
    { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
    { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
    { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy },
    { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy },
    { id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType },
    { id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText },
    { id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber },
    { id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean },
    { id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime },
    { id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList },
    { id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime },
    { id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastActivedTime },
    { id: 38, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibraryName },
    { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    { id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    { id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    { id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    { id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    { id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel },
    { id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName },
    { id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName },
];

const level64Type4SOOneDrive21V = level64Type4SOOneDrive.filter((item) => ![47, 49, 60].includes(item.id));

export const oneDriveRuleTypes = {
    ...archiveRuleTypes,
    64: level64Type4ILOneDrive,
};

export const oneDriveRuleTypes21V = {
    ...oneDriveRuleTypes,
    64: oneDriveRuleTypes[64].filter((item) => ![47, 49, 60].includes(item.id)),
};

export const oneDriveRuleTypes4SO = {
    ...archiveRuleTypes,
    2: [
        { id: 17, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_URL },
        { id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Title },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
    ],
    16: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
    ],
    256: [
        ...archiveRuleTypes[256],
        {id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocSensitiveLabel},
    ],
    64: level64Type4SOOneDrive,
};

export const oneDriveRuleTypes4SO21V = {
    ...oneDriveRuleTypes4SO,
    64: level64Type4SOOneDrive21V,
    256: oneDriveRuleTypes4SO[256].slice(0, -1),
};

export const exoRulTypes = {
    6553601: [{id: 40, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Subjecjt},
        {id: 41, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_AttachmentCount},
        {id: 15, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Size},
        {id: 42, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendDateUTC},
        {id: 43, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendFrom},
        {id: 44, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendTo},
        {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
        {id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel},
    ],
    6553607: [
        {id: 15, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Size},
        {id: 42, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendDateUTC},
        {id: 43, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendFrom},
        {id: 44, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendTo}
    ]
};
export const phyRulTypes = {
    10001: [{id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
        {id: 74, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SubfolderDisposalDate},
    ],
    10002: [{id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
    ]
};
export const FsRulTypes = {
    64: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        {id: 32, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileType},
        {id: 33, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileOwner},
        {id: 35, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FilePath}]
};

export const JPMCFsRuleTypes = {
    64: [
        ...FsRulTypes[64],
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime}
    ]
};

export const AzureFileRulTypes = {
    4194304: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        {id: 32, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileType},
        {id: 35, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FilePath}]
};

export const BoxRulTypes = {
    8388608: [
        { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
        { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
        { id: 32, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileType },
        { id: 35, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FilePath }]
};

export const GoogleDriveRuleTypes = {
    16777216: [
        { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
        { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
        // { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy },
        { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy },
        { id: 59, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LabelName},
        { id: 50, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LabelText, isGoogle: true },
        { id: 51, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LabelNumber, isGoogle: true },
        { id: 52, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LabelDate, isGoogle: true },
        // temp: Curtis, we will do it in the future
        // { id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime, disabled: true, tooltip: RMResx.RM_JS_RDM_CreateRule_Options_TooltipsComingSoon },
    ],
};

export const TeamsRuleTypes = {
    33554432: [
        { id: 54, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DisplayName },
        { id: 33, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileOwner },
        { id: 55, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Member },
        { id: 53, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Classification },
        { id: 58, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsType },
        { id: 57, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamStatus },
        { id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel },
        { id: 56, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Privacy },
        { id: 17, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_URL },
        { id: 14, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_Title },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_Modified },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_CreateTime },
        { id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_LastAccessedTime },
        { id: 48, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_LastActivedTime },
        { id: 22, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_PrimaryAdministrator },
        { id: 23, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_SiteCollectionSizeTrigger },
        { id: 18, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_TextCustomProperty },
        { id: 19, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_NumberCustomProperty },
        { id: 20, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_BooleanCustomProperty },
        { id: 21, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_TeamsGroup_DateTimeCustomProperty },
    ]
}

export const ConnectorRulTypes = [
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
    {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
    {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
    {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
    {id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy},
    {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
    {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
    {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime},
];

export const phyLevelIds = {
    PhysicalBox: 10001,
    PhysicalFile: 10002,
};
export const RuleLevelIds = {
    FS: 1048576,
};

export const GoogleLevelIds = {
    GG: 16777216,
};

export const TeamsLevelIds = {
    Teams: 33554432,
}

export const Regexs = [{id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains},
    {id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains},
    {id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths},
    {id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath},
    {id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
    {id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot}
];

export const SpecialRegexs = [
    { id: 65736, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsEmpty },
    { id: 65737, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_In },
];

export const RuleType = {
    Name: 1,
    DocumentSize: 2,
    Modified: 3,
    CreateTime: 4,
    CreateBy: 5,
    ModifiedBy: 6,
    ContentType: 7,
    ColumnText: 8,
    ColumnNumber: 9,
    ColumnBoolean: 10,
    ColumnDateTime: 11,
    ParentListId: 12,
    LastAccessTime: 13,
    Title: 14,
    Size: 15,
    KeepTheLatestVersion: 16,
    URL: 17,
    TextCustomProperty: 18,
    NumberCustomProperty: 19,
    BooleanCustomProperty: 20,
    DateTimeCustomProperty: 21,
    PrimaryAdministrator: 22,
    SiteCollectionSizeTrigger: 23,
    Type: 32,
    Owner: 33,
    Path: 35,
    MetadataTextColumn: 36,
    MetadataNumberColumn : 37,
    ParentLibraryName: 38,
    Subject: 40,
    AttachmentCount: 41,
    SendDateUTC: 42,
    SendFrom: 43,
    SendTo: 44,
    ParentFolderName: 45,
    ParentFolderNameHeirarchically: 46,
    RetentionLabelRule: 47,
    LastActiveTime: 48,
    SensitiveLabel: 49,
    LabelPropertyText: 50,
    LabelPropertyNumber: 51,
    LabelPropertyDate: 52,
    SensitiveLabelFullName: 60,
    DocumentModifiedTime: 61,
    ParentLibraryText: 62,
    ParentLibraryNumber: 63,
    ParentLibraryYestNo: 64,
    ParentLibraryDateTime: 65,
    ParentSiteCollectionText: 66,
    ParentSiteCollectionNumber: 67,
    ParentSiteCollectionYestNo: 68,
    ParentSiteCollectionDateTime: 69,
    OrphanedFolder: 75,

    // Property bag
    PropertyBagText: 70,
    PropertyBagNumber: 71,
    PropertyBagBoolean: 72,
    PropertyBagDateTime: 73,

    // Teams
    Classification: 53,
    DisplayName: 54,
    Member: 55,
    Privacy: 56,
    TeamsStatus: 57,
    TeamsType: 58,

    // Google drive
    LabelName: 59,

    // Physical box
    LatestSubfolderDisposalDate: 74,
};
export const dateOption = [
    {id: 2048, Name: RMResx.RM_JS_RDM_CreateRule_DateOption_FromTo},
    {id: 4096, Name: RMResx.RM_JS_RDM_CreateRule_DateOption_Before},
    {id: 65734, Name: RMResx.RM_JS_RDM_CreateRule_DateOption_Older}
];
export const ConditionType = {
    Contains: 8,
    DoesNotContains: 525872,
    Maths: 1051744,
    DoesNtoMath: 2103488,
    Equals: 262936,
    IsExactlyNot: 4206976,
    FromTo: 2048,
    Before: 4096,
    OlderThan: 65734,
    IsEmpty: 65736,
    ListIn: 65737,
};
export const unitSize = [
    { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_Unit_KB},
    { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_Unit_MB},
    { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_Unit_GB},
    { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_Unit_Days},
    { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_Unit_Weeks},
    { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_Unit_Months},
    { id: 7, Name: RMResx.RM_JS_RDM_CreateRule_Unit_Years}
];
export const compare = [{id: 32, Name: ">="}, {id: 16, Name: "<="}, {id: 262936, Name: "="}];
export const AllOrAny = [
    {id: 0, Name: RMResx.RM_JS_RDM_CreateRule_AllOrAny_All},
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_AllOrAny_Any}
];
export const TrueOrFaseOptions = [{ id: 0, Name: RMResx.RM_JS_Common_Yes, value: "yes" }, { id: 1, Name: RMResx.RM_JS_Common_No, value: "no" }];    //为了修改RECO-18301 所以添加value
export const dispatchAction = {
    elementDisabled:'elementDisabled',
    clearData:'clearData',
    save:'save',
    setData:'setData',
    approvalCheckboxDisabledAndChecked:'approvalCheckboxDisabledAndChecked',
    ExportCheckboxDisabledAndChecked:'ExportCheckboxDisabledAndChecked',
    selectedModuleType:'selectedModuleType',
    selectedStorage: 'selectedStorage',
    resetRetentionInfo: 'resetRetentionInfo',
};
export const ReviewType = {
    RecordOwner: 0,
    Workflow: 1
};
export const ExportSPDataOption = {
    None: 0,
    ExportBeforeArchive: 1,
    ExportWithoutArchive: 2
};

export const ResultFailedType = 
{
    None: 0,
    NoGlobalStorageSetting: 1,
    NotConnDocAve: 2
};

export const OperationLogicValues = {
    And: 0,
    Or: 1
};

export const TierTypes = {
    DefaultTier: 0,
    ArchiveTier: 3,
    ColdTier: 4,
};

export const RetentionConditionType = {
    OlderThan: 1,
    Before: 3,
};

export const RetentionConditionUnit = {
    Year: 1,
    Month: 2,
    Week: 3,
    Days: 4,
};

export const RetentionOperateType = {
    DeleteData: 1,
    MarkDataTier: 2,
};

export const RetentionDataTimeRadioValue = {
    ArchivedTime: 1,
    ModifiedTime: 2,
};

export const RetentionLabelOptions = {
    Default: 0,
    GetFromGeneralSetting: 1,
}
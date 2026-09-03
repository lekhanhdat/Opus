export const levels = [
    // {id: 64, Name: RMResx.RM_JS_Rule_ObjectLevel_Document},
    {id: 64, Name: RMResx.RM_JS_Rule_ObjectLevel_Document},
    {id: 32, Name: RMResx.RM_JS_Rule_ObjectLevel_Item},
    {id: 16, Name: RMResx.RM_JS_Rule_ObjectLevel_Folder},
    {id: 8, Name: RMResx.RM_JS_Rule_ObjectLevel_List},
    {id: 4, Name: RMResx.RM_JS_Rule_ObjectLevel_Site},
    {id: 2, Name: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection},
];
export const RuleLevel = {
    Document: 64,
    Item: 32,
    Folder: 16,
    List: 8,
    Site: 4,
    SiteCollection: 2,
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
// export const Matchs1 = [
//     {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains},
//     {id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains},
//     {id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths},
//     {id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath},
//     {id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
//     {id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot}
// ];

export const level64RuleType = [
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
    { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    // {id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    // {id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    // {id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    // {id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    // {id: 66, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText },
    // {id: 67, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber },
    // {id: 68, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean },
    // {id: 69, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime },
    {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
    {id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName},
    {id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName },
    // { id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    // { id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn }
]

export const level64RuleType21V = [
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
    { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
    // Add back in the future: 62 to 69
    // {id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
    // {id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
    // {id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
    // {id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    // {id: 66, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText },
    // {id: 67, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber },
    // {id: 68, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean },
    // {id: 69, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime },

    // { id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
    // { id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn }
]

export const rulTypes = {
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
        {id: 12, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentList}],
    16: [{id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy},
        {id: 7, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ContentType},
        {id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnText},
        {id: 9, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnNumber},
        {id: 10, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnBoolean},
        {id: 11, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ColumnDateTime}],
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
    ]
};

export const rulTypesNormal = {
    ...rulTypes,
    64: level64RuleType,
}

export const rulTypes21V = {
    ...rulTypes,
    64: level64RuleType21V,
}

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
        // {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
        // { id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
        // { id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn }
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

export const oneDriveRuleTypes = {
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
        // {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        { id: 45, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderName },
        {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
        { id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_DisplayName },
        { id: 60, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel_FullName },
        // { id: 36, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataTextColumn },
        // { id: 37, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_MetadataNumberColumn }
        // {id: 62, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText },
        // {id: 63, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber },
        // {id: 64, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean },
        // {id: 65, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime },
    ]
};

export const exoRulTypes = {
    6553601: [{id: 40, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Subjecjt},
        {id: 41, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_AttachmentCount},
        {id: 15, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Size},
        {id: 42, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendDateUTC},
        {id: 43, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendFrom},
        {id: 44, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SendTo},
        {id: 47, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_RetentionLabel},
        {id: 49, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_SensitiveLabel}
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
export const azureFileRulTypes = {
    64: [
        {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name},
        {id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize},
        {id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal},
        {id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime},
        {id: 13, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_LastAccessedTime},
        {id: 32, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileType},
        {id: 35, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FilePath}]
};
export const boxRulTypes = {
    64: [
        { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
        { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
        { id: 32, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FileType },
        { id: 35, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_FilePath },
    ]
};

export const GoogleDriveRuleTypes = {
    64: [
        { id: 1, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Name },
        { id: 2, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_DocumentSize },
        { id: 3, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal },
        { id: 4, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime },
        // { id: 5, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_CreatedBy },
        { id: 6, Name: RMResx.RM_JS_RDM_CreateRule_RuleType_ModifiedBy },
    ],
};


export const phyLevelIds = {
    PhysicalBox: 10001,
    PhysicalFile: 10002,
};
// export const Regexs = [{id: 8, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Contains},
//     {id: 525872, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNotContains},
//     {id: 1051744, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Maths},
//     {id: 2103488, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_DoesNtoMath},
//     {id: 262936, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_Equals},
//     {id: 4206976, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsExactlyNot}
// ];
export const SpecialRegexs = [
    { id: 65736, Name: RMResx.RM_JS_RDM_CreateRule_RuleRegexs_IsEmpty },
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
    Subject: 40,
    AttachmentCount: 41,
    SendDateUTC: 42,
    SendFrom: 43,
    SendTo: 44,
    ParentFolderName: 45,
    RetentionLabelRule: 47,
    LastActiveTime: 48,
    SensitiveLabel: 49,
    SensitiveLabelFullName: 60,
    ParentLibraryText: 62,
    ParentLibraryNumber: 63,
    ParentLibraryYestNo: 64,
    ParentLibraryDateTime: 65,
    ParentSiteCollectionText: 66,
    ParentSiteCollectionNumber: 67,
    ParentSiteCollectionYestNo: 68,
    ParentSiteCollectionDateTime: 69,
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
    // Equals: 262936,
    Equals: 1,
    IsExactlyNot: 4206976,
    FromTo: 2048,
    Before: 4096,
    OlderThan: 65734,
    GreaterThanOrEqualTo: 32,
    LessThanOrEqualTo: 16,
    IsEmpty: 65736,
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
// export const compare = [{id: 32, Name: ">="}, {id: 16, Name: "<="}, {id: 262936, Name: "="}];
export const compare = [{id: 32, Name: ">="}, {id: 16, Name: "<="}, {id: 1, Name: "="}];
export const AllOrAny = [
    {id: 0, Name: RMResx.RM_JS_RDM_CreateRule_AllOrAny_All},
    {id: 1, Name: RMResx.RM_JS_RDM_CreateRule_AllOrAny_Any}
];
export const TrueOrFaseOptions = [{ id: 0, Name: RMResx.RM_JS_Common_Yes }, { id: 1, Name: RMResx.RM_JS_Common_No}];
export const dispatchAction = {
    elementDisabled:'elementDisabled',
    clearData:'clearData',
    save:'save',
    setData:'setData',
    approvalCheckboxDisabledAndChecked:'approvalCheckboxDisabledAndChecked',
    ExportCheckboxDisabledAndChecked:'ExportCheckboxDisabledAndChecked'
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
export const RuleSourceTabIndex = 
{
    SP: 0,
    OneDrive: 1,
    Exchange: 2,
    Physical :3,
    FS: 4,
    SPLocal: 5
};
export const ResultFailedType = 
{
    None: 0,
    NoGlobalStorageSetting: 1,
    NotConnDocAve: 2
};
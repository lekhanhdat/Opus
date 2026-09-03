const DocumentCriteriaType = {
    None: 0,
    Name: 1,
    ParentFolder: 2, 
    CreatedTime: 3,
    ModifiedTime: 4,
    DocumentType: 5,
    DocumentSize: 6,
    //Google label
    GoogleLabel: 7,
    ParentLibraryText: 8,
    ParentLibraryNumber: 9,
    ParentLibraryBoolean: 10,
    ParentLibraryDateTime: 11,
    ParentSiteCollectionText: 12,
    ParentSiteCollectionNumber: 13,
    ParentSiteCollectionBoolean: 14,
    ParentSiteCollectionDateTime: 15,
    PropertyBagText: 16,
    PropertyBagNumber: 17,
    PropertyBagBoolean: 18,
    PropertyBagDateTime: 19,
};

const DocumentCriteriaTypeI18ns = new Map([
    [DocumentCriteriaType.Name, RMResx.RM_JS_RDM_CreateRule_RuleType_Name],
    [DocumentCriteriaType.ParentFolder, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentFolderNameHeirarchically],
    [DocumentCriteriaType.CreatedTime, RMResx.RM_JS_RDM_CreateRule_RuleType_CreateTime],
    [DocumentCriteriaType.ModifiedTime, RMResx.RM_JS_RDM_CreateRule_RuleType_Modified_Normal],
    [DocumentCriteriaType.DocumentType, RMResx.RM_FA_Discovery_RuleType_DocumentType],
    [DocumentCriteriaType.DocumentSize, RMResx.RM_FA_Discovery_RuleType_DocumentSize],
    [DocumentCriteriaType.ParentLibraryText, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibText],
    [DocumentCriteriaType.ParentLibraryNumber, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibNumber],
    [DocumentCriteriaType.ParentLibraryBoolean, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibBoolean],
    [DocumentCriteriaType.ParentLibraryDateTime, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentLibDateTime],
    [DocumentCriteriaType.PropertyBagText, RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagText],
    [DocumentCriteriaType.PropertyBagNumber, RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagNumber],
    [DocumentCriteriaType.PropertyBagBoolean, RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagBoolean],
    [DocumentCriteriaType.PropertyBagDateTime, RMResx.RM_JS_RDM_CreateRule_RuleType_PropertyBagDateTime],
    [DocumentCriteriaType.ParentSiteCollectionText, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCText],
    [DocumentCriteriaType.ParentSiteCollectionNumber, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCNumber],
    [DocumentCriteriaType.ParentSiteCollectionBoolean, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCBoolean],
    [DocumentCriteriaType.ParentSiteCollectionDateTime, RMResx.RM_JS_RDM_CreateRule_RuleType_ParentSCDateTime],
    [DocumentCriteriaType.GoogleLabel, RMResx.RM_FA_Discovery_RuleType_Label],
]);

const VersionCriteriaType = {
    None: 0,
    KeepLastVersions: 1,
    ModifiedTime: 2,
    DocumentType: 3,
    DocumentSize: 4,
};

const VersionCriteriaTypeI18ns = new Map([
    [VersionCriteriaType.KeepLastVersions, RMResx.RM_JS_RDM_CreateRule_RuleType_KeepVersion],
    [VersionCriteriaType.ModifiedTime, RMResx.RM_JS_RDM_CreateRule_RuleType_Modified],
    [VersionCriteriaType.DocumentType, RMResx.RM_FA_Discovery_RuleType_DocumentType],
    [VersionCriteriaType.DocumentSize, RMResx.RM_FA_Discovery_RuleType_DocumentSize],
]);

const DuplicateCriteriaType = {
    None: 0,
    Duplicate: 1,
};

const DuplicateCriteriaTypeI18ns = new Map([
    [DuplicateCriteriaType.Duplicate, RMResx.RM_FA_Discovery_RuleType_Duplicate],
]);

const CriteriaConstants = {
    document: {
        type: DocumentCriteriaType,
        i18n: DocumentCriteriaTypeI18ns
    },
    version: {
        type: VersionCriteriaType,
        i18n: VersionCriteriaTypeI18ns
    },
    duplicate: {
        type: DuplicateCriteriaType,
        i18n: DuplicateCriteriaTypeI18ns
    }
};

export default CriteriaConstants;

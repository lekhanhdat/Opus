export const SourceFlag = {
    None: -1,
    All: 0,
    SharePoint: 1,
    FileSystem: 2,
    Exchange: 3,
    Physical: 4,
    SharePointOnPrem: 5,
    OneDrive: 6,
    AzureFileShare: 7,
    Box: 8,
    GoogleDrive: 9,
    Teams: 11,
    LifecycleRetention: 99,
};

export const SourceFlagI18Ns = new Map([
    [ SourceFlag.Exchange, RMResx.RM_JS_SPS_TabLabel_EXO ],
    [ SourceFlag.OneDrive, RMResx.RM_JS_SPS_TabLabel_OneDrive ],
    [ SourceFlag.Physical, RMResx.RM_JS_SPS_TabLabel_Physical ],
    [ SourceFlag.SharePointOnPrem, RMResx.RM_JS_SPS_TabLabel_SPLocal ],
    [ SourceFlag.FileSystem, RMResx.RM_JS_SPS_TabLabel_FS ],
    [ SourceFlag.SharePoint, RMResx.RM_JS_SPS_TabLabel_SP ],
    [ SourceFlag.AzureFileShare, RMResx.RM_JS_SPS_TabLabel_AF ],
    [ SourceFlag.All, RMResx.RM_JS_SPS_TabLabel_All],
    [ SourceFlag.LifecycleRetention, RMResx.RM_JM_DeletionStatus_Retention]
]);
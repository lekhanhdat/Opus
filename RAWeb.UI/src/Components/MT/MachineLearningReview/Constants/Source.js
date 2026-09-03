export const Source = {
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

export const SourceI18Ns = new Map([
    [Source.Exchange, RMResx.RM_JS_SPS_TabLabel_EXO],
    [Source.OneDrive, RMResx.RM_JS_SPS_TabLabel_OneDrive],
    [Source.Physical, RMResx.RM_JS_SPS_TabLabel_Physical],
    [Source.SharePointOnPrem, RMResx.RM_JS_SPS_TabLabel_SPLocal],
    [Source.FileSystem, RMResx.RM_JS_SPS_TabLabel_FS],
    [Source.SharePoint, RMResx.RM_JS_SPS_TabLabel_SP],
    [Source.Teams, RMResx.RM_JS_SPS_TabLabel_Teams],
]);

export const SourceIcons = new Map([
    [Source.Exchange, "fi-ms-exchange"],
    [Source.OneDrive, "fi-ms-onedrive"],
    [Source.Physical, "fia-physical-record"],
    [Source.SharePointOnPrem, "fia-sharepoint"],
    [Source.FileSystem, "fia-file-system-c"],
    [Source.SharePoint, "fi-ms-sharepoint"],
    [Source.Teams, "fi-ms-teams"],
]);
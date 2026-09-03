export const StatisticJobStatus = {
    InProgress: 1,
    Finished: 2,
    Failed: 3,
    FinishWithException: 4,
}

export const StatisticMessageType = {
    Failed: 1,
}

export const ActiveTab = {
    Search: 0,
    EDiscovery: 1,
}

export const DataSourceType = {
    None: 0,
    M365: 1,
    FS: 2,
    Teams: 3,
    Google: 4
}

export const SearchMode = {
    NormalSearch: 1,
    FullTextAdvanceSearch: 2,
    FullTextSimpleSearch: 3,
};

export const LevelType = {
    SiteCollection: 2,
    Site: 4,
    List: 8,
    Folder: 16,
    Item: 32,
    Document: 64,
    Attachment: 128,
    DocumentVersion: 256,
    GoogleDriveDocument : 16777216,
    Teams: 33554432,
    Mailbox: 33554433,
};

export const ObjectLevelItems = [
    {
        name: RMResx["StorageOptimization.Gui_Site Collection"],
        value: LevelType.SiteCollection,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_Site"],
        value: LevelType.Site,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_List"],
        value: LevelType.List,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_Folder"],
        value: LevelType.Folder,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_Item"],
        value: LevelType.Item,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_Document"],
        value: LevelType.Document,
        checked: true,
    },
    {
        name: RMResx["RM_JS_Rule_ObjectLevel_DocumentVersion"],
        value: LevelType.DocumentVersion,
        checked: false,
    },
];

export const TeamsObjectLevelItems = [
    {
        name: RMResx.RM_JS_Rule_ObjectLevel_Teams,
        value: LevelType.Teams,
        checked: true,
    },
    {
        name: RMResx.RM_JS_Rule_ObjectLevel_Mailbox,
        value: LevelType.Mailbox,
        checked: false,
    },
];

export const RestoreType = {
    InPlace: 1,
    OutOfPlace: 2,
    SPOLibOrFolder: 4, // 3 is stubOOP
};

export const ContentSearchType = {
    Whitelist: 0,
    Blacklist: 1,
};

export const getContentSearchOptions = (selectedOption) => {
    return [
        {
            text: RMResx.RM_AR_RC_White_Blacklist_Option01,
            value: ContentSearchType.Whitelist,
            checked: selectedOption === ContentSearchType.Whitelist,
        },
        {
            text: RMResx.RM_AR_RC_White_Blacklist_Option02,
            value: ContentSearchType.Blacklist,
            checked: selectedOption === ContentSearchType.Blacklist,
        },
    ];
};
export const Priority = [
	{ name: RMResx.RM_JS_JM_Priority_High, value: 1, checked: false },
	{ name: RMResx.RM_JS_JM_Priority_Normal, value: 0, checked: true },
	{ name: RMResx.RM_JS_JM_Priority_Low, value: -1, checked: false },
]

export const RestoreDocumentVersionsOption = {
    None: 0,
    SpecifyVersions: 1,
    AllVersions: 2
};

export const AdvanceRestoreType = {
    Microsoft365InPlace: 6,
    OpusArchivedStubs: 5
};

export const AdvanceLocationType = {
    Url: 1,
    Tree: 2
}

export const AdvanceRestoreScope = {
    IncludeChildren: 0,
    SelectedLocationOnly: 1
};

export const RestoreOption = {
    OverWrite: 0,
    Skip: 1,
    Append: 2,
};

export const ConflictItems = [
    {
        name: RMResx["StorageOptimization.Gui_55FAA921-761C-4085-B272-FFA469BFBA71"],
        value: RestoreOption.Skip,
        checked: true,
    },
    {
        name: RMResx["StorageOptimization.Gui_Overwrite"],
        value: RestoreOption.OverWrite,
        checked: false,
    },
    {
        name: RMResx["StorageOptimization.Gui_D9C9D8AF-2E0C-4DE6-A5B5-EFE3B5146EEE"],
        value: RestoreOption.Append,
        checked: false,
    },
];

export const AppConflictItems = [
    {
        name: RMResx["StorageOptimization.Gui_55FAA921-761C-4085-B272-FFA469BFBA71"],
        value: RestoreOption.Skip,
        checked: true,
    },
    {
        name: RMResx["StorageOptimization.Gui_Overwrite"],
        value: RestoreOption.OverWrite,
        checked: false,
    },
];

export const RestoreLevel = {
    1: RMResx.RM_RestoreCenter_SiteCollection_Impact,
    2: RMResx.RM_RestoreCenter_Site_Impact,
    3: RMResx.RM_RestoreCenter_ListLibrary_Impact,
    4: RMResx.RM_RestoreCenter_Folder_Impact,
    5: RMResx.RM_RestoreCenter_Item_Impact,
    6: RMResx.RM_RestoreCenter_ItemVersion_Impact,
    7: RMResx.RM_RestoreCenter_Document_Impact,
    8: RMResx.RM_RestoreCenter_DocumentVersion_Impact,
    9: RMResx.RM_RestoreCenter_Attachment_Impact,
};
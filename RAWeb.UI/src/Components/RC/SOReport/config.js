const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [180],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Size,
        width: [150],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Deleted_Size,
        width: [150],
        resizeable: true,
    },
];

const RAMessageType = {
    Successful: 0,
    Failed: 1,
    Exception: 2
};

const DefaultPager = {
    PageIndex: 0,
    PageSize: 10,
    SearchKey: '',
};

const ReportType = {
    AllItemsOrSubSite: -1,
    None: 0,
    SiteCollection: 1,
    AllItem: 2,
    DedupData: 3,
    SubSite: 4,
    AllTeamsGroup: 5,
    AllGoogleDrive: 7,
    AllGoogleDriveItems: 8,
};

const ReportTitle = {
    [-1]: RMResx.RM_AR_Report_ExportItem,
    [1]: RMResx.RM_AR_Report_ExportAllSites,
    [2]: RMResx.RM_AR_Report_ExportItem,
    [3]: RMResx.RM_AR_Report_ExportDedupData,
    [4]: RMResx.RM_AR_Report_ExportItem,
    [5]: RMResx.RM_AR_Report_ExportAllTeamsGroup,
    [7]: RMResx.RM_AR_Report_ExportAllTeamsGroup,
    [8]: RMResx.RM_AR_Report_ExportItem,
};

export {
    TableColumns,
    DefaultPager,
    RAMessageType,
    ReportType,
    ReportTitle,
};
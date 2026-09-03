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
    Google: 9,
    Teams: 11,
};

export const DateRange = {
    Last12Month: 0,
    Last10Week: 1,
    Last10Day: 2
};

export const DashboardJobCreationStatus = {
    None: 0,
    ExistsJobQueue: 1,
    HasRunningJob: 2,
    Failed: 3,
    Succeed: 4
};

export const CacheKey = {
    SourceFlag: "SourceFlag"
};

export const DashboardEndUserPermission = { 
    None: 0,
    ReviewEndUser: 1,
    EndUser: 2,
};
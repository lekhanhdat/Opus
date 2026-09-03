const DiscoveryDataSource = {
    None: 0,
    Office365: 1,
    Salesforce: 2,
    Google: 3,
    FileSystem: 4,
}

const DiscoveryDataSourceI18ns = new Map([
    [DiscoveryDataSource.Office365, RMResx.RM_FA_Discovery_Common_O365_Source],
    [DiscoveryDataSource.Salesforce, RMResx.RM_FA_Discovery_Common_Salesforce_Source],
    [DiscoveryDataSource.Google, RMResx.RM_FA_Discovery_Common_GoogleDrive_Source],
    [DiscoveryDataSource.FileSystem, RMResx.RM_FA_Discovery_Common_FS_Source],
]);

export {
    DiscoveryDataSource,
    DiscoveryDataSourceI18ns
}
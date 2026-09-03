const TableColumns = [
    {
        header: RMResx.RM_DSB_Column_URL,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Size,
        width: [200],
        resizeable: true,
    },
    {
        header: RMResx.RM_DSB_Column_Deleted_Size,
        width: [200],
        resizeable: true,
    },
];

const ArchivedDataSizeRequestOption = {
    url: "/api/Dashboard/GetArchiverDataSize"
};

const ArchivedFileCountRequestOption = {
    url: "/api/Dashboard/GetArchiverFileCount"
};

const ArchivedVersionCountRequestOption = {
    url: "/api/Dashboard/GetArchiverVersionCount"
};

const YearlySavingRequestOption = {
    url: "/api/Dashboard/GetYearlySaving"
};

const SiteCollectionRequestOption = {
    url: "/api/Dashboard/GetArchiverSiteInfo"
};

const TeamsGroupsRequestOption = {
    url: "/api/Dashboard/GetArchiverTeamsGroupInfo"
};

const GetConfigurationDataRequestOption = {
    url: "/api/Dashboard/GetSOPriceConfiguration"
};

const SavaConfigurationDataRequestOption = {
    url: "/api/Dashboard/SaveSOPriceConfiguration"
};

const ArchiverDataUnit = {
    Unknown: 0,
    GB: 1,
    TB: 2,
    K: 3,
    Million: 4,
};

const ArchiverDataUnitName = {
    0: "",
    1: RMResx.RM_DSB_Unit_GB,
    2: RMResx.RM_DSB_Unit_TB,
    3: RMResx.RM_DSB_Unit_K,
    4: RMResx.RM_DSB_Unit_Million,
};

export {
    TableColumns,
    ArchivedDataSizeRequestOption,
    ArchivedFileCountRequestOption,
    ArchivedVersionCountRequestOption,
    YearlySavingRequestOption,
    SiteCollectionRequestOption,
    TeamsGroupsRequestOption,
    ArchiverDataUnit,
    ArchiverDataUnitName,
    GetConfigurationDataRequestOption,
    SavaConfigurationDataRequestOption,
};
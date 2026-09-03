const TimeRangeTypes = {
    All: 0,
    Last12Months: 1,
    Last6Months: 2,
    Last3Months: 3,
};

const ResourceTypes = {
    All: 0,
    SharePoint: 1,
    OneDrive: 2,
};

const ArchiveDataUnitTypes = {
    GB: 1,
    TB: 2,
};

const ArchiveDataUnitI18NMapping = {
    [ArchiveDataUnitTypes.GB]: RMResx.RM_DSB_Unit_GB,
    [ArchiveDataUnitTypes.TB]: RMResx.RM_DSB_Unit_TB,
}

const DateRangeSelectorItems = [
    {
        checked: true,
        name: RMResx.RM_JS_DSB_AllTime,
        value: TimeRangeTypes.All,
        disabled: false,
    },
    {
        checked: false,
        name: RMResx.RM_JS_DSB_Last12Month,
        value: TimeRangeTypes.Last12Months,
        disabled: false
    },
    {
        checked: false,
        name: RMResx.RM_JS_DSB_Last6Month,
        value: TimeRangeTypes.Last6Months,
        disabled: false
    },
    {
        checked: false,
        name: RMResx.RM_JS_DSB_Last3Month,
        value: TimeRangeTypes.Last3Months,
        disabled: false
    },
];

const ResourceSelectorItems = [
    {
        checked: true,
        name: RMResx.RM_JS_Common_ReportType_AllSources,
        value: ResourceTypes.All,
        disabled: false,
    },
    {
        checked: false,
        name: RMResx.RM_JS_Common_ReportType_SharePoint,
        value: ResourceTypes.SharePoint,
        disabled: false,
    },
    {
        checked: false,
        name: RMResx.RM_JS_Common_ReportType_OneDrive,
        value: ResourceTypes.OneDrive,
        disabled: false,
    },
];

const LEGEND_ITEMS = {
    ArchiveStorageOverview: [
        { color: '#149EB0', label: RMResx.RM_JS_DSB_ChartLegend_NewlyArchived, indicator: 'dot' },
        { color: '#D01A83', label: RMResx.RM_JS_DSB_ChartLegend_DestroyedDataFromArchived, indicator: 'dot' },
        { color: '#0072D0', label: RMResx.RM_JS_DSB_ChartLegend_ArchivedBalance, indicator: 'line' },
    ],
    StorageOptimizationBySource: [
        { color: '#248AED', label: RMResx.RM_JS_DSB_ChartLegend_ArchivedBalance, indicator: 'dot', opacity: 1 },
        { color: '#D01A83', label: RMResx.RM_JS_DSB_ChartLegend_DestroyedData, indicator: 'dot', opacity: 0.7 },
        { color: '#0072D0', label: RMResx.RM_JS_DSB_ChartLegend_SavingFromArchiving, indicator: 'line', opacity: 1 },
        { color: '#D01A83', label: RMResx.RM_JS_DSB_ChartLegend_SavingFromDestruction, indicator: 'line', opacity: 1 },
    ],
    StorageOptimizationContributionBySource: [
        { color: '#0072D0', label: RMResx.RM_JS_DSB_ChartLegend_SPOContribution, indicator: 'dot', opacity: 1 },
        { color: '#149EB0', label: RMResx.RM_JS_DSB_ChartLegend_ODContribution, indicator: 'dot', opacity: 1 },
        { color: '#D95630', label: RMResx.RM_JS_DSB_ChartLegend_TotalArchiving, indicator: 'line', opacity: 1 },
    ],
};

const AXIS_LABEL_STYLE = {
    fontFamily: 'Open Sans, sans-serif',
    fontSize: '12px',
    colors: '#323E4D',
};

const STORAGE_VALUE_SUMMARY_CARD_ITEMS = [
    {
        key: 'TotalDestroyedDataSize',
        label: RMResx.RM_JS_DSB_TotalDestroyDataSize_Title,
        description: RMResx.RM_JS_DSB_TotalDestroyDataSize_Desc,
        hasUnit: true,
        unit: ArchiveDataUnitI18NMapping[ArchiveDataUnitTypes.GB],
    },
    {
        key: 'TotalSavingsFromArchiving',
        label: RMResx.RM_JS_DSB_TotalSavingFromArchiving_Title,
        description: RMResx.RM_JS_DSB_TotalSavingFromArchiving_Desc,
        hasUnit: false,
    },
    {
        key: 'TotalSavingsFromDestruction',
        label: RMResx.RM_JS_DSB_TotalSavingsFromDestruction_Title,
        description: RMResx.RM_JS_DSB_TotalSavingsFromDestruction_Desc,
        hasUnit: false,
    },
    {
        key: 'EstimatedCo2eReduction',
        label: RMResx.RM_JS_DSB_EstimatedCo2eReduction_Title,
        description: RMResx.RM_FA_Progress_ProjectionContext_Co2_Desc.format(
            "https://www.iea.org/commentaries/the-carbon-footprint-of-streaming-video-fact-checking-the-headlines"
        ),
        hasUnit: true,
        unit: RMResx.RM_JS_DSB_Unit_KG_CO2,
    },
];

export {
    TimeRangeTypes,
    ResourceTypes,
    DateRangeSelectorItems,
    ResourceSelectorItems,
    LEGEND_ITEMS,
    AXIS_LABEL_STYLE,
    STORAGE_VALUE_SUMMARY_CARD_ITEMS,
    ArchiveDataUnitTypes,
    ArchiveDataUnitI18NMapping,
};
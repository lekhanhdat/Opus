import { SourceFlag } from "../Common/Constants";
const ElecRecordColumnsInfo = [
    '',
    RMResx.RM_JS_BCM_Explorer_Datagrid_Name,
    RMResx.RM_JS_BCM_Explorer_Datagrid_FileType,
    RMResx.RM_JS_BCM_Explorer_Datagrid_UniqueID,
    RMResx.RM_JS_BCM_Explorer_Datagrid_Author,
    RMResx.RM_PRM_PRE_PanelTitle_DisposalClass,
    RMResx.RM_JS_BCM_Explorer_Datagrid_Rule,
    RMResx.RM_JS_BCM_Explorer_Datagrid_DisposalAction,
    RMResx.RM_JS_BCM_Explorer_Datagrid_DisposalDueDate,
    RMResx.RM_JS_BCM_Explorer_Datagrid_RecordsOwner,
    RMResx.RM_JS_BCM_Explorer_Datagrid_OnHold,
    RMResx.RM_JS_BCM_Explorer_Datagrid_Declared,
];
const ElecRecordColumnsWidth = [
    50, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200, 200
];
const EleDateCondition = {
    Pending: -2,
    NextJob: -1,
    None: 0,
    Before: 1,
    After: 2,
    FromTo: 3
};
const ElecActDueDateTypes = {
    '-2': RMResx.RM_JS_BCM_Explorer_DueDatePending_Title,
    '-1': RMResx.RM_JS_BCM_Explorer_DueDateNextJob_Title,
    '0': RMResx.RM_JS_BCM_Explorer_Filter_All,
    '1': RMResx.RM_JS_BCM_Explorer_DueDateBefore_Title,
    '2': RMResx.RM_JS_BCM_Explorer_DueDateAfter_Title,
    '3': RMResx.RM_JS_BCM_Explorer_DueDateRange_Title,
};
const ElecAllOrCommon = {
    All: '0',
    Common: '1'
};
const ElecStatusEnum = {
    InProgress: 0,
    Failed: 1,
    Completed: 2,
    Exception: 3,
};

const IndustryList = [
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryConsumer,
        value: RMResx.RM_TM_AI_Recommendations_IndustryConsumer,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryEducation,
        value: RMResx.RM_TM_AI_Recommendations_IndustryEducation,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryEnergy,
        value: RMResx.RM_TM_AI_Recommendations_IndustryEnergy,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryFinancial,
        value: RMResx.RM_TM_AI_Recommendations_IndustryFinancial,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryGovernment,
        value: RMResx.RM_TM_AI_Recommendations_IndustryGovernment,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryHealthCare,
        value: RMResx.RM_TM_AI_Recommendations_IndustryHealthCare,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryIndustrial,
        value: RMResx.RM_TM_AI_Recommendations_IndustryIndustrial,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryIT,
        value: RMResx.RM_TM_AI_Recommendations_IndustryIT,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryProfit,
        value: RMResx.RM_TM_AI_Recommendations_IndustryProfit,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryServices,
        value: RMResx.RM_TM_AI_Recommendations_IndustryServices,
        checked: false,
    },
    {
        text: RMResx.RM_TM_AI_Recommendations_IndustryOther,
        value: RMResx.RM_TM_AI_Recommendations_IndustryOther,
        checked: false,
    },
]

const ExportSettingEnumType = {
    CustomSetting: 0,
    AllSetting: 1,
}

const getExportSettingTypes = (selectedOption, sourceFlag) => {
    const allSettingText = {
        [SourceFlag.SharePoint]: RMResx.RM_JS_SP_ExportSetting_OptionAll,
        [SourceFlag.Teams]: RMResx.RM_JS_Teams_ExportSetting_OptionAll
    }
 
    return [
        {
            text: RMResx.RM_JS_SP_ExportSetting_OptionCustom,
            value: ExportSettingEnumType.CustomSetting,
            checked: selectedOption === ExportSettingEnumType.CustomSetting,
        },
        {
            text: allSettingText[sourceFlag] ?? '',
            value: ExportSettingEnumType.AllSetting,
            checked: selectedOption === ExportSettingEnumType.AllSetting,
        },
    ];
}

const TeamsTreeBrowseType = {
    Office365GroupEntire: "office365GroupEntire",
    Container: "container",
};

const SPOTreeBrowseType = {
    SiteCollection: "siteCollection",
    Container: "container",
};

const getTeamsSearchPlaceholder = (browseType) => {
    if (browseType === TeamsTreeBrowseType.Container) {
        return RMResx.RM_JS_BCM_ContainerName_Search;
    }
    return RMResx.RM_JS_BCM_GroupMailbox_Search;
}

const getSOSearchPlaceholder = (browseType) => {
    if (browseType=== SPOTreeBrowseType.Container) {
        return RMResx.RM_JS_BCM_ContainerName_Search;
    }
    return RMResx.RM_JS_BCM_SiteCollection_Search;
}

export {
    ElecRecordColumnsInfo,
    ElecRecordColumnsWidth,
    EleDateCondition,
    ElecActDueDateTypes,
    ElecAllOrCommon,
    ElecStatusEnum,
    IndustryList,
    ExportSettingEnumType,
    getExportSettingTypes,
    TeamsTreeBrowseType,
    SPOTreeBrowseType,
    getTeamsSearchPlaceholder,
    getSOSearchPlaceholder
};

export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: true,
                SPDisplayUniqueId: true,
                enableKeepSharePointDefaultValue: true,
                enableHiddenColumn: true,
                hiddenColumnDetail: RMResx.RM_JS_SPS_HiddenColumn,
                hiddenColumnTitle: RMResx.RM_JS_SPS_HiddenColumnTitle,
                hiddenColumnMsg: RMResx.RM_JS_SPS_HiddenColumnMsg,
                uniqueIdDetail: RMResx.RM_JS_SPS_EditKey_ShowUniqueID,
                uniqueIdTitle: RMResx.RM_JS_SPS_UniqueIsShow,
                defaultTermDetail: RMResx.RM_JS_SPS_EditKey_KeepSPDefaultValue,
                defaultTermTitle: RMResx.RM_JS_SPS_KeepSPDefaultValue,
                defaultTermYesText: RMResx.RM_JS_SPS_KeepSPDefaultValue_Option_Yes,
                defaultTermCheckboxText: RMResx.RM_SPS_NoSetTermForEmptyDefaultValue_Title,
                defaultTermMsg: RMResx.RM_JS_SPS_KeepSPDefaultValueMsg,
            },
            saveExistColumnData: "/api/SPSettingApi/SaveColumnSettingExistColumn",
            saveCreateColumnData: "/api/SPSettingApi/SaveColumnSetting",
            downloadRelatedApp: "/api/SPSettingApi/DownloadRelatedApp",
        };
    },
};
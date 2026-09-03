export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: true,
                SPDisplayUniqueId: true,
                enableKeepSharePointDefaultValue: true,
                enableHiddenColumn: true,
                hiddenColumnDetail: RMResx.RM_JS_SPS_Teams_HiddenColumn,
                hiddenColumnTitle: RMResx.RM_JS_SPS_Teams_HiddenColumnTitle,
                hiddenColumnMsg: RMResx.RM_JS_SPS_Teams_HiddenColumnMsg,
                uniqueIdDetail: RMResx.RM_JS_SPS_Teams_EditKey_ShowUniqueID,
                uniqueIdTitle: RMResx.RM_JS_SPS_Teams_UniqueIsShow,
                defaultTermDetail: RMResx.RM_JS_SPS_Teams_EditKey_KeepSPDefaultValue,
                defaultTermTitle: RMResx.RM_JS_SPS_Teams_KeepSPDefaultValue,
                defaultTermYesText: RMResx.RM_JS_SPS_Teams_KeepSPDefaultValue_Option_Yes,
                defaultTermCheckboxText: RMResx.RM_SPS_Teams_NoSetTermForEmptyDefaultValue_Title,
                defaultTermMsg: RMResx.RM_JS_SPS_Teams_KeepSPDefaultValueMsg,
            },
            saveExistColumnData: "/api/TeamsSettingApi/SaveColumnSettingExistColumn",
            saveCreateColumnData: "/api/TeamsSettingApi/SaveColumnSetting",
            downloadRelatedApp: "/api/SPSettingApi/DownloadRelatedApp",
        };
    },
};
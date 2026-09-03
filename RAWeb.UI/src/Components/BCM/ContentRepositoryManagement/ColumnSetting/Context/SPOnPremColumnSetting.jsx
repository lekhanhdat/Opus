export default {
    getContext() {
        return {
            configurations: {
                enableRelatedRecords: true,
                SPDisplayUniqueId: false,
                enableKeepSharePointDefaultValue: false,
                enableHiddenColumn: false,
                uniqueIdDetail: RMResx.RM_JS_SPS_EditKey_ShowUniqueID,
                uniqueIdTitle: RMResx.RM_JS_SPS_UniqueIsShow,
                defaultTermDetail: RMResx.RM_JS_SPS_EditKey_KeepSPDefaultValue,
                defaultTermTitle: RMResx.RM_JS_SPS_KeepSPDefaultValue,
                defaultTermYesText: RMResx.RM_JS_SPS_KeepSPDefaultValue_Option_Yes,
                defaultTermCheckboxText: RMResx.RM_SPS_NoSetTermForEmptyDefaultValue_Title,
                defaultTermMsg: RMResx.RM_JS_SPS_KeepSPDefaultValueMsg,
            },
            saveExistColumnData: "/api/SPOnPremSettingApi/SaveColumnSettingExistColumn",
            saveCreateColumnData: "/api/SPOnPremSettingApi/SaveColumnSetting",
            downloadRelatedApp: "/api/SPOnPremSettingApi/DownloadRelatedApp",
        };
    },
};
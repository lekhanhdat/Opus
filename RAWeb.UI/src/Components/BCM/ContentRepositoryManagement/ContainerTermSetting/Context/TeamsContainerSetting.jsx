export default {
    getContext() {
        return {
            configurations: {
                containerDes: RMResx.RM_JS_SPS_Teams_EditTitle_ContainerLevel_Description,
                inheritParentTermText: RMResx.RM_JS_SPS_ContainerLevel_InheritParentTerm,
            },
            getSavedTermUrl: "/api/TeamsSettingApi/GetSavedTree",
            saveContainerSettingUrl: "/api/TeamsSettingApi/SaveContainerLevelSetting",
        };
    },
};
export default {
    getContext() {
        return {
            configurations: {
                containerDes: RMResx.RM_JS_SPS_EditTitle_ContainerLevel_Description,
                inheritParentTermText: RMResx.RM_JS_SPS_ContainerLevel_InheritParentTerm,
            },
            getSavedTermUrl: "/api/SPSettingApi/GetSavedTree",
            saveContainerSettingUrl: "/api/SPSettingApi/SaveContainerLevelSetting",
        };
    },
};
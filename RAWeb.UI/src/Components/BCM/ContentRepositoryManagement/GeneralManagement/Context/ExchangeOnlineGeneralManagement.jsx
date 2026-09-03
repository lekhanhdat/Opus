
export default {
    getContext() {
        return {
            saveDataUrl: "/api/EXOSettingApi/SaveGeneralSetting",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            showGeneralToast: true,
            showUniqueIdWarn: false,
        };
    },
    supportSync(generalSetting) {
        return true;
    },
    supperDisplayUniqueId(generalSetting) {
        return false;
    },
};

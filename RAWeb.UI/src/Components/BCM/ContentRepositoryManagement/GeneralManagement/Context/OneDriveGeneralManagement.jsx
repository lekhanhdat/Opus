import CRMCommonUtil from "../../Common/CRMCommonUtil";

export default {
    getContext() {
        return {
            saveDataUrl: "/api/OneDriveSettingApi/SaveGeneralSetting",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            showGeneralToast: true,
            showUniqueIdWarn: true,
        };
    },
    supportSync(generalSetting) {
        return false;
    },
    supperDisplayUniqueId(generalSetting) {
        return CRMCommonUtil.isGroup(generalSetting);
    },
};
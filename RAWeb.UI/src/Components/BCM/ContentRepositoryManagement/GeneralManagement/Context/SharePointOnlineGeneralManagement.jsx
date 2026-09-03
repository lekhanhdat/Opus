import CRMCommonUtil from "../../Common/CRMCommonUtil";

export default {
    getContext() {
        return {
            saveDataUrl: "/api/SPSettingApi/SaveGeneralSetting",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            supportUnlockSite: true,
            showGeneralToast: true,
            showUniqueIdWarn: true,
        };
    },
    supportSync(generalSetting) {
        return CRMCommonUtil.isGroup(generalSetting) || CRMCommonUtil.isSiteCollection(generalSetting);
    },
    supperDisplayUniqueId(generalSetting) {
        return false;
    },
};

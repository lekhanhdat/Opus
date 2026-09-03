import CRMCommonUtil from "../../Common/CRMCommonUtil";

export default {
    getContext() {
        return {
            saveDataUrl: "/api/TeamsSettingApi/SaveGeneralSetting",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            supportUnlockSite: true,
            showGeneralToast: true,
            showUniqueIdWarn: true,
        };
    },

    supportSync(generalSetting) {
        return CRMCommonUtil.isGroup(generalSetting) || CRMCommonUtil.isTeams(generalSetting);
    },

    supperDisplayUniqueId(generalSetting) {
        return false;
    },
};
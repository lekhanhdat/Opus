import CRMCommonUtil from "../../Common/CRMCommonUtil";

export default {
    getContext() {
        return {
            saveDataUrl: "/api/FSSettingApi/SaveFSGeneralSetting",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            supportDownloadRCCReport: true,
            showGeneralToast: true,
            showUniqueIdWarn: true,
        };
    },
    supportSync(generalSetting) {
        return false;
    },
    supperDisplayUniqueId(generalSetting) {
        return false;
    },
};
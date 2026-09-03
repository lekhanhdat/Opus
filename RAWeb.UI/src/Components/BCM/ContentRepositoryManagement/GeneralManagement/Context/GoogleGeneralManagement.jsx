import CRMCommonUtil from "../../Common/CRMCommonUtil";

export default {
    getContext() {
        return {
            saveDataUrl: "/api/GoogleDriveSettingApi/SaveGeneralSetting",
            loadingUniqueIdSettingUrl: "/API/BCMAdminSettingApi/LoadingUniqueIdSetting",
            resource: "BCM_ContentRepositoryManagement_UniqueId",
            supportSync: this.supportSync,
            supperDisplayUniqueId: this.supperDisplayUniqueId,
            showGeneralToast: true,
            showUniqueIdWarn: false,
        };
    },
    supportSync(generalSetting) {
        return CRMCommonUtil.isGoogleContainer(generalSetting) || CRMCommonUtil.isGoogleDriveItem(generalSetting);
    },
    supperDisplayUniqueId(generalSetting) {
        return false;
    },
};

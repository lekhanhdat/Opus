import { DateUtil } from "../Components/DiscoveryAndAnalysis/Analysis/Utils";
import { SourceIcons, SourceFlags, DefaultExportLimit, LicenseType } from "../Constants/Constants";
import Enviroments from "../Constants/Enviroments";
import { productKeys, storageKeys } from "./Constant";

export function bindEvents() {
    let [ that, ...eventList ] = arguments;
    eventList.forEach(event => {
        if (that[event]) {
            that[event] = that[event].bind(that);
        }
    });
}

export function setCheckedStatus(keyField, checkedField, items, checkedItem) {
    if (items && checkedItem) {
        let checkedKey = checkedItem[keyField];
        for (let item of items) {
            item[checkedField] = item[keyField] == checkedKey;
        }
    }
    return items;
}

export function setCheckedStatusByValue(keyField, checkedField, items, checkedItemValue) {
    if (items && checkedItemValue) {
        for (let item of items) {
            item[checkedField] = item[keyField] == checkedItemValue;
        }
    }
    return items;
}

export function getCheckedItem(checkedField, items) {
    if (items && checkedField) {
        for (let item of items) {
            if (item[checkedField]) {
                return item;
            }
        }
    }
    return null;
}
export function getRequestVerificationToken() {
    let hidToken = document.getElementById("hiddenRequestVerificationToken");
    return !hidToken ? null : hidToken.value;
}

export const showToast = {
    _showMsg: (type, content) => {
        $$.toast({classify: type, content: content});
    },
    _renderCustomToast: function(toastId, title, content, type) {
        let container = document.getElementById("custom-toast-container");
        if (!container) {
            container = document.createElement("div");
            container.id = "custom-toast-container";
            document.body.appendChild(container);
        }

        const mountPoint = document.createElement("div");
        container.appendChild(mountPoint);

        const root = ReactDOM.createRoot ? ReactDOM.createRoot(mountPoint) : null;
        const toastElement = (
            <R.Toast id={toastId} classify={type}>
                <div tabIndex={0} slot="title">{title}</div>
                <div tabIndex={0} slot="content">{content}</div>
            </R.Toast>
        );

        if (root) {
            root.render(toastElement);
        } else {
            ReactDOM.render(toastElement, mountPoint);
        }

        setTimeout(() => {
            $$.toast(true, `#${toastId}`);
        }, 0);

        setTimeout(() => {
            if (root) {
                root.unmount();
            } else {
                ReactDOM.unmountComponentAtNode(mountPoint);
            }
            mountPoint.remove();
        }, 5000);
    },
    success: function(content) {
        this._showMsg("success", content);
    },
    error: function(content) {
        this._showMsg("error", content);
    },
    warn: function(content) {
        this._showMsg("warn", content);
    },
    info: function(content) {
        this._showMsg("info", content);
    },
    withTitle: function({ id, title, content, type = "info" }) {
        showToast._renderCustomToast(id, title, content, type);
    }
};

export function SelectedProportionWord(selectedCount, totalCount) {
    return RMResx.RM_Common_SelectTableItemsCounter.format(selectedCount, totalCount);
}

export function GetIsExistDuplicate(arr) {
    return new Set(arr).size !== arr.length;
}

export function GetDuplicateValues(arr = []) {
    let duplicateValue = arr.filter(value => arr.indexOf(value) !== arr.lastIndexOf(value));
    return [...new Set(duplicateValue)];
}

export function formatBoolean (
    bool, trueText = RMResx.RM_JS_Common_Yes, falseText = RMResx.RM_JS_Common_No
){
    return bool ? trueText : falseText;
}

export function isEmptyObject (obj){
    return obj && Object.keys(obj).length == 0;
}

export function getSourceIcon(sourceFlag) {
    if(sourceFlag){
        if(Object.values(SourceFlags).includes(sourceFlag)){
            return SourceIcons[sourceFlag];
        }
        return "fi-ms-azure-file-share";
    }
}

export class LicenseHelper {
    
    static HasDiscoveryLicenseOnly() {
        return LicenseHelper.HasDiscoveryLicense() &&
            !LicenseHelper.HasOpusILLicense() &&
            !LicenseHelper.HasOpusSOLicense() &&
            !LicenseHelper.HasOpusGoogleLicense();
    }

    static HasOpusILLicense()
    {
        return RM.gData.hasRecordsLicense;
    }

    static HasFileSystemLicense()
    {
        return RM.gData.hasFileSystemLicense;
    }

    static HasOpusGoogleLicense()
    {
        return RM.gData.hasGoogleLicense;
    }

    static HasDiscoveryExportRowData()
    {
        return RM.gData.HasDiscoveryExportRowData;
    }

    static HasDiscoveryLicense() {
        return RM.gData.hasDiscoveryLicense || RM.gData.hasDiscoverySalesforceLicense || RM.gData.hasDiscoveryGoogleLicense || RM.gData.hasDiscoveryFileSystemLicense;
    }

    static HasOpusSOLicense()
    {
        return RM.gData.hasArchiverLicense;
    }

    static EnableRecordsArchiver()
    {
        return RM.gData.enableRecordsArchiver;
    }

    static HasOpusSOLicenseOnly()
    {
        return RM.gData.hasArchiverLicense && !RM.gData.hasRecordsLicense && !RM.gData.hasGoogleLicense;
    }

    static HasOpusGoogleLicenseOnly()
    {
        return RM.gData.hasGoogleLicense && !RM.gData.hasRecordsLicense && !RM.gData.hasArchiverLicense;
    }

    static HasOpusILAndSOLicense() 
    {
        return RM.gData.hasRecordsLicense && RM.gData.hasArchiverLicense;
    }

    static HasOpusGoogleAndSOLicense()
    {
        return RM.gData.hasGoogleLicense && RM.gData.hasArchiverLicense;
    }
    static HasUpgradeTeams() {
        return RM.gData.hasUpgradeTeams;
    }

    static EnableArchiverOnly() {
        return RM.gData.enableArchiverOnly;
    }

    static EnableJPMCFileSystemFeature() {
        return RM.gData.enableJPMCFileSystemFeature;
    }

    static Is21VEnv() {
        return RM.gData.enviromentName == Enviroments.ChinaNorth;
    }

    static IsTrialLicense() {
        return RM.gData.licenseType == LicenseType.Trial;
    }
}

export class ServiceHelper {

    static CanArchiverImportSC() {
        return RM.gData.useArchiverImportFile;
    }

}

export class EnvironmentHelper {
    static get IsGCPEnvironment() {
        return RM.gData.enviromentName === Enviroments.GCP || RM.gData.enviromentName === Enviroments.GCPTest;
    }
    //GCC/GCCH environment
    static get IsGovAzureEnv() {
        return RM.gData.enviromentName === Enviroments.GCC;
    }
}

export const LocalStorage = {

    get: function(key) {
        let localStorageKey = RM.gData.logonGroupId + RM.gData.userId;
        const storage = localStorage.getItem(localStorageKey);
        return storage ? JSON.parse(storage)[key] : storage;
    },

    set: function(key, value) {
        if(key && value){
            let localStorageKey = RM.gData.logonGroupId + RM.gData.userId;
            let storage = localStorage.getItem(localStorageKey);
            storage = storage ? JSON.parse(storage) : {};
            storage[key] = value;
            localStorage.setItem(localStorageKey, JSON.stringify(storage));
        }
    }
};

export function WrapperLinkUrl(sourceUrl) {
    try {
        if(sourceUrl.indexOf("Root/PRM/RecordsExplorer") != -1)
        {
            return sourceUrl;
        }
        let [isNeedExclude, fileExtentionsConfig] = [false, RM.gData.fileExtentionsConfig];
        if(sourceUrl?.indexOf(".") > -1 && fileExtentionsConfig?.EnableExclusion)
        {
            let fileExtension = sourceUrl.substring(sourceUrl.lastIndexOf(".") + 1);
            isNeedExclude = fileExtentionsConfig?.FileExtensions?.includes(fileExtension.toLocaleLowerCase());
        }
        return isNeedExclude? sourceUrl : `${sourceUrl}?web=1`;
    } catch (error) {
        console.log(error);
        return sourceUrl;
    }
}

export function GetExportResultCountLimit() {
    try {
        let [defaultLimit, limitInConfig] = [DefaultExportLimit, RM.gData.exportResultLimit];
        return limitInConfig? limitInConfig : defaultLimit;
        
    } catch (error) {
        console.log(error);
    }
}

export function formatDatePosition(month, year) {
    const language = navigator.language;
    switch (language) {
        case 'ko':
            // ko-KR
            return `${year}년 ${DateUtil.i18nMonth(month)}`;
        case 'zh-CN':
            return `${year}年${DateUtil.i18nMonth(month)}`;
        case 'ja':
            // ja-JP
            return `${year} 年 ${DateUtil.i18nMonth(month)}`;
        default:
            // en-US
            return `${DateUtil.i18nMonth(month)} ${year}`;
    };
}

export function hasDuplicateName(arr, name) {
    const names = arr.map((item) => item[name]);
    const uniqueNames = new Set(names);
    return uniqueNames.size !== names.length;
};

export function getUserGuildTagPage(tag) {
    if(LicenseHelper.Is21VEnv()){
        return "https://cdn.avepoint.com/pdfs/cn/user_guides/AvePoint_Opus_User_Guide.pdf" 
    }

    let baseUserGuidUrl = `https://learn.avepoint.com/avepoint-opus`;
    const tagMap = {
        [storageKeys.exportArchiveIndex]: "/appendices/how-to-export-archived-data-from-archival-storages.html",
        [storageKeys.storageConfiguration]: "/settings/manage-your-storage.html",
        [storageKeys.stubManagement]: "/settings/configure-stub-templates.html",
        [storageKeys.veoExportSetting]: "/appendices/manage-configuration-files-for-export-formats.html#export-content-into-veo-files",
        [storageKeys.naaExportSetting]: "/appendices/manage-configuration-files-for-export-formats.html#export-content-into-naa-files",
        [storageKeys.naraExportSetting]: "/appendices/manage-configuration-files-for-export-formats.html#export-content-into-nara-files",
    }
    const isMatchedTag = !!tagMap[tag];

    if (!isMatchedTag) {
        baseUserGuidUrl = 'https://learn.avepoint.com/avepoint-opus/about-avepoint-opus.html'
    }

    return baseUserGuidUrl + (tagMap[tag] || "");
}

export function getActionDueDateI18n() {
    return RM.gData.enableCustomizationApp ? RMResx.RM_JS_MA_Grid_DisposalDueDate_JPMC : RMResx.RM_JS_MA_Grid_DisposalDueDate
};

export function sortArrayByField(arr = [], sortField, order = "asc") {
    if (!Array.isArray(arr)) return [];

    const factor = order === "desc" ? -1 : 1;

    return [...arr].sort((a, b) => {
        const v1 = a[sortField] ?? 0;
        const v2 = b[sortField] ?? 0;
        return (v1 - v2) * factor;
    });
}

export const isShowActionByDC = () => {
    if(RM.gData.enableMultiGEOFeature) {
        return RM.gData.isMultiGeoMainDC;
    }
 
    return true;
}

export const isEnableMultiGeoFeature = () => {
    return RM?.gData?.enableMultiGEOFeature ?? false;
}

export const getMulticomboboxAllItems = (selectItems = [], allItems = [], mappingId = "id", checkedField = "isChecked") => {
    const selectItemIds = selectItems.map(selectItem => selectItem[mappingId]);
    return allItems.map(item => ({
        ...item,
        [checkedField]: selectItemIds.includes(item[mappingId])
    }));
};
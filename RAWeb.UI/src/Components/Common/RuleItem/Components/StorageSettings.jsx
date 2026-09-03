import { useEffect, useState, forwardRef, useImperativeHandle } from "react";
import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import { RuleSourceTabIndex, TierTypes } from "./Constants";
import { StorageTypeIndex } from "../../../../Constants/Constants";

const GetStoreDataItems = (type) => [
    {
        text: RMResx.RM_RDM_CreateRule_DefaultTier,
        title: RMResx.RM_RDM_CreateRule_DefaultTier,
        value: TierTypes.DefaultTier,
        checked: type === TierTypes.DefaultTier,
    },
    {
        text: RMResx.RM_RDM_CreateRule_ColdTier,
        title: RMResx.RM_RDM_CreateRule_ColdTier,
        value: TierTypes.ColdTier,
        checked: type === TierTypes.ColdTier,
    },
    {
        text: RMResx.RM_RDM_CreateRule_ArchivedTier,
        title: RMResx.RM_RDM_CreateRule_ArchivedTier,
        value: TierTypes.ArchiveTier,
        checked: type === TierTypes.ArchiveTier,
    },
];

const StorageSettings = ({ storagePolicyList, storagePolicyId, storeDataTierInfo, getSelecteStorage, resetRetentionInfo, id, sourceTab }, ref) => {

    const defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

    const [storageList, setStorageList] = useState([]);

    const [selectedStorage, setSelectedStorage] = useState("");

    const [isEmpty, setIsEmpty] = useState(false);

    const [tierType, setTierType] = useState(TierTypes.DefaultTier);

    const [isShowStoreDataOption, setIsShowStoreDataOption] = useState(false);

    useEffect(() => {
        loadStorageList();
        loadTierType();
    }, [storagePolicyId, storeDataTierInfo.moveToAnotherTierType]);

    useImperativeHandle(ref, () => ({
        getSelectedStorage: () => {
            return selectedStorage;
        },
        getTierType: () => {
            return tierType;
        },
        isValid: () => {
            setIsEmpty(!selectedStorage);
            return !!selectedStorage;
        }
    }));

    const loadStorageList = () => {
        let storageSettingList = RM.deepcopy(storagePolicyList);
        if (sourceTab === RuleSourceTabIndex.FS) {
            storageSettingList = storageSettingList.filter(s => !(s.IsSystemStorage || s.Id.toLowerCase() === defaultDeviceId || s.Type === StorageTypeIndex.Google || s.Type === StorageTypeIndex.Dropbox));
        }
        if (storageSettingList.length > 0 && storagePolicyId) {
            for (let item of storageSettingList) {
                item.Checked = item.Id === storagePolicyId;
                if (item.Checked) {
                    setSelectedStorage(item);
                    getSelecteStorage && getSelecteStorage(item);
                    checkShowStoreDataOption(item);
                }
            }
        }
        setStorageList(RM.deepcopy(storageSettingList));
    };

    const loadTierType = () => {
        setTierType(storeDataTierInfo.moveToAnotherTierType);
    };

    const onChangeStorage = (args) => {
        checkShowStoreDataOption(args.newValue);
        getSelecteStorage && getSelecteStorage(args.newValue);
        setSelectedStorage(args.newValue);
        setIsEmpty(false);
        setTierType(TierTypes.DefaultTier);
        resetRetentionInfo && resetRetentionInfo();
    };

    const checkShowStoreDataOption = (storage) => {
        if (LicenseHelper.EnableRecordsArchiver() && storeDataTierInfo.showStoreDataOption && storage.mCurrentXRI.VIM === "azure_vim" && storage.Id.toLowerCase() != defaultDeviceId && !storage.IsSystemStorage) {
            setIsShowStoreDataOption(true);
        } else {
            setIsShowStoreDataOption(false);
        }
    };

    return <div className="ra-cr-form-row">
        <$g.FormRow
            id="ariaRuleStorage"
            label={RMResx.RM_JS_RDM_CreateRule_ArchiveStorage}
            tipMsg={LicenseHelper.EnableRecordsArchiver() ?
                <$g.I18NProvider msg={RMResx.RM_JS_RDM_CreateRule_ArchiveStorageTip}>
                    <a className="ra-link-a" href="/Root/CP/StorageSettings">{RMResx.RM_JS_CP_StorageSetting}</a>
                </$g.I18NProvider> :
                RMResx.RM_JS_RDM_CreateRule_OldUserArchiveStorageTip}
        >
            <R.Combobox
                id="raCrStorageSettingsCombobox"
                width={"100%"}
                searchable={false}
                textField='Name'
                valueField='Id'
                checkedField='Checked'
                items={storageList}
                onChange={onChangeStorage}
                aria="#ariaRuleStorage"
            />
            <$g.ValidationMsg show={isEmpty}>
                {RMResx.RM_JS_RDM_CreateRule_ArchiveStorageError}
            </$g.ValidationMsg>
        </$g.FormRow>
        {LicenseHelper.EnableRecordsArchiver() && isShowStoreDataOption && <$g.FormRow
            id="ariaRuleStoreData"
            label={RMResx.RM_JS_RDM_CreateRule_StoreDataTitle}
            tipMsg={RMResx.RM_JS_RDM_CreateRule_ArchivedTierTip}
        >
            <R.Radio.Group
                block
                name={`${id}-radiogroup-type`}
                aria="#ariaRuleStoreData"
                items={GetStoreDataItems(tierType)}
                onChange={(value) => setTierType(value)}
            />
        </$g.FormRow>}
    </div>;
};

export default forwardRef(StorageSettings);

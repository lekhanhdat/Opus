import _ from "lodash";
import { useEffect, useState } from "react";
import { ArchiveDataType, ArchiveOrRemoveFileType, ArchiveOrRemoveVersionType, GetStoreDataItems, TierTypes } from "../../../Constants/DataOptimizeType";

const StorageLocation = ({ dataOptimizeParameter, onChange }) => {

    const defaultDeviceId = "6a040c17-af8a-4f1f-96c1-7ceb2e23b1f3";

    const [isShowStoreDataOption, setIsShowStoreDataOption] = useState(false);
    const [storageList, setStorageList] = useState([]);

    useEffect(() => {
        const handler = async () => {
            if (_.isEmpty(dataOptimizeParameter.selectedStorageParameter)) {
                const response = await fetchUtility({
                    url: "/api/StorageDevice/GetAllActiveStorage",
                    method: "Post",
                    data: {
                        PageIndex: -1,
                        PageSize: 10,
                        SearchValue: "",
                        TotalNumber: 0
                    }
                });
                if (response.StorageDeviceUIDtosList.length > 0 && response.IndexDeviceId) {
                    for (let item of response.StorageDeviceUIDtosList) {
                        item.Checked = item.Id === response.IndexDeviceId;
                        if (item.Checked) {
                            dataOptimizeParameter.selectedStorageParameter = item;
                            checkShowStoreDataOption(item);
                        }
                    }
                }
                setStorageList(RM.deepcopy(response.StorageDeviceUIDtosList));
            }
        };
        handler();
    }, [dataOptimizeParameter]);

    useEffect(() => {
        if (!_.isEmpty(dataOptimizeParameter.selectedStorageParameter)) {
            checkShowStoreDataOption(dataOptimizeParameter.selectedStorageParameter);
        }
    }, [dataOptimizeParameter])

    const onStorageChanged = (args) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.selectedStorageParameter = args.newValue;
        clonedParameter.moveToAnotherTierType = TierTypes.DefaultTier;
        // checkShowStoreDataOption(args.newValue);
        setStorageList((prev) => prev.map((item) => ({
            ...item,
            Checked: item.Id === args.newValue.Id
        })));
        onChange(clonedParameter);
    };

    const onTierTypeChanged = (args) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.moveToAnotherTierType = args;
        onChange(clonedParameter);
    };

    const isShow = (parameter) => {
        const clonedParameter = _.cloneDeep(parameter);
        let archiverFile = [ArchiveOrRemoveFileType.ArchiveAndRemove, ArchiveOrRemoveFileType.Archive].includes(clonedParameter.processActionParameter.archiveOrRemoveFile);
        let archiverVersion = clonedParameter.processActionParameter.archiveOrRemoveVersion === ArchiveOrRemoveVersionType.ArchiveAndRemove;

        if (clonedParameter.archiveDataType === ArchiveDataType.All) {
            return archiverFile;
        } else {
            if (clonedParameter.rotRuleQueryParameter.enable) {
                return archiverFile || archiverVersion;
            } else {
                return archiverVersion;
            }
        }
    };

    const checkShowStoreDataOption = (storage) => {
        if (storage.mCurrentXRI.VIM === "azure_vim" && storage.Id.toLowerCase() != defaultDeviceId && !storage.IsSystemStorage) {
            setIsShowStoreDataOption(true);
        } else {
            setIsShowStoreDataOption(false);
        }
    };

    return (
        <div className="reco-optimize-option margin-top-l">
            {isShow(dataOptimizeParameter) && <div>
                <div id="ariaStorage" className="reco-optimize-title require">{RMResx.RM_FA_DataOptimize_StorageTitle}</div>
                <div>
                    <R.Validation
                        element="Combobox"
                        require={RMResx.RM_FA_DataOptimize_Validation_ErrorMsg}>
                        <R.Combobox
                            id="raStorage"
                            width={"100%"}
                            searchable={false}
                            textField='Name'
                            valueField='Id'
                            checkedField='Checked'
                            items={storageList}
                            onChange={onStorageChanged}
                            aria={{
                                ariaLabelledby: "ariaStorage",
                                ariaRequired: true
                            }}
                        />
                    </R.Validation>
                </div>

                {isShowStoreDataOption && <div className="margin-top-l">
                    <div className="reco-optimize-title-font">
                        <span id="ariaRuleStoreData">{RMResx.RM_JS_RDM_CreateRule_StoreDataTitle}</span>
                        <$g.Popover>{RMResx.RM_JS_RDM_CreateRule_ArchivedTierTip}</$g.Popover>
                    </div>
                    <R.Radio.Group
                        block
                        name="radiogroup-type"
                        aria="#ariaRuleStoreData"
                        items={GetStoreDataItems(dataOptimizeParameter.moveToAnotherTierType)}
                        onChange={onTierTypeChanged}
                    />
                </div>}
            </div>}
        </div>
    );
};

export default StorageLocation;
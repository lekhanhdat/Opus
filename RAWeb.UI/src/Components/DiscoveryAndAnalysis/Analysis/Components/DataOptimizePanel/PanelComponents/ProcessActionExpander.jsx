import _ from "lodash";
import { ArchiveDataType, MS365DataType, ArchiveOrRemoveFileType, ArchiveOrRemoveVersionType } from "../../../Constants/DataOptimizeType";
import { useState, useEffect, useRef } from "react";
import { LicenseHelper } from "../../../../../../Utilities/CommonUtil";
import StubPanel from "../../../../../CP/StubSettings/StubPanel";

const ProcessActionExpander = ({ dataOptimizeParameter, onChange }) => {

    const [levelStubSettingList, setLevelStubSettingList] = useState([]);

    const [showStubSettingsPanel, setShowStubSettingsPanel] = useState(false);

    const [recordsLabelValue, setRecordsLabelValue] = useState(RMResx.RM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel);

    const stubSettingPanelRef = useRef(null);

    useEffect(() => {
        handleLoadAllStubSettings();
    }, []);

    useEffect(() => {
        loadDeclaredRecords();
    }, []);

    const handleLoadAllStubSettings = async () => {
        const response = await fetchUtility({
            url: "/api/StubSetting/GetAllStubSettingsNotPaged",
            method: "Post",
        });
        setLevelStubSettingList(response);
        return response;
    };

    const loadDeclaredRecords = () => {
        $$.loading(true);
        const options = {
            url: "/api/CPApi/GetGeneralSetting",
        };
        fetchUtility(options)
            .then((res) => {
                if (res) {
                    const generalSettingModel = res.GeneralSettingModel;
                    setRecordsLabelValue(generalSettingModel.RecordsLabel ?? RMResxRM_JS_SP_MigrateDeclaredRecords_NoneRecordsLabel);
                }
            })
            .finally(() => $$.loading(false));
    }

    const onChanged = (field, value) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);

        // Reset value for sub option and input (archive and remove / archive only)
        if (field === "archiveOrRemoveFile") {
            clonedParameter.processActionParameter.isArchiveOnlyVersionOption = false;
            clonedParameter.processActionParameter.archiverOnlyLastestVersion = '0';
            clonedParameter.processActionParameter.isArchiveVersionOption = false;
            clonedParameter.processActionParameter.archiveVersionValue = '0';
        }

        clonedParameter.processActionParameter[field] = value;
        onChange(clonedParameter);
    };

    const isShowFileOption = (parameter) => {
        const clonedParameter = _.cloneDeep(parameter);
        if (clonedParameter.archiveDataType === ArchiveDataType.All) {
            return true;
        } else {
            if (clonedParameter.rotRuleQueryParameter.enable) {
                return true;
            } else {
                return false;
            }
        }
    };

    const isShowVersionOption = (parameter) => {
        const clonedParameter = _.cloneDeep(parameter);
        if (clonedParameter.archiveDataType === ArchiveDataType.All) {
            return false;
        } else {
            if (clonedParameter.inactiveRuleQueryParameter.enable || clonedParameter.rotRuleQueryParameter.enable) {
                return true;
            } else {
                return false;
            }
        }
    };

    const customVerify = (value) => {
        value = value.trim();
        if (_.isNil(value) || _.isEmpty(value)) {
            return RMResx.RM_JS_RDM_CreateRule_Validation_ConditionNoValue;
        } else if (value < 0) {
            return RMResx.RM_JS_RDM_NotNumber;
        }
        return true;
    };

    const handleCancelStubSettingsPanel = () => {
        setShowStubSettingsPanel(false);
    }

    const handleSaveStubSettings = () => {
        if (stubSettingPanelRef.current) {
            stubSettingPanelRef.current.onSave((isSuccess) => {
                if (isSuccess) {
                    handleCancelStubSettingsPanel();
                    handleLoadAllStubSettings()
                        .then((res) => {
                            const clonedParameter = _.cloneDeep(dataOptimizeParameter);
                            clonedParameter.processActionParameter.selectedLevelStub = res[0];
                            setLevelStubSettingList((prev) => prev.map((item, index) => ({
                                ...item,
                                Checked: index === 0, // auto set checked true to the first item
                            })));
                            onChange(clonedParameter);
                        })
                }
            })
        }
    }

    const renderLeaveStubSetting = () => {
        return <div>
            {LicenseHelper.EnableRecordsArchiver() ? (
                <R.Validation
                    element="Combobox"
                    require={RMResx.RM_FA_DataOptimize_Validation_ErrorMsg}>
                    <R.Combobox
                        id="raLeaveStubOption"
                        width={"100%"}
                        textField='Name'
                        valueField='Id'
                        checkedField='Checked'
                        items={levelStubSettingList}
                        createNewText={RMResx.RM_JS_Rule_Stub_CreateTemplate_Btn}
                        onChange={(args) =>
                            onChanged("selectedLevelStub", args.newValue)
                        }
                        doCreateNew={() => setShowStubSettingsPanel(true)}
                    />
                </R.Validation>
            ) : (
                <R.Validation
                    element="Combobox"
                    require={RMResx.RM_FA_DataOptimize_Validation_ErrorMsg}>
                    <R.Combobox
                        id="raLeaveStubOption"
                        width={"100%"}
                        textField='Name'
                        valueField='Id'
                        checkedField='Checked'
                        items={levelStubSettingList}
                        onChange={(args) =>
                            onChanged("selectedLevelStub", args.newValue)
                        }
                    />
                </R.Validation>
            )}
        </div>;
    };

    const renderArchiveVersionSetting = () => {
        return (
            <div style={{ marginLeft: 28 }}>
                <R.Validation
                    element="Input"
                    rules={{
                        customVerify,
                    }}
                >
                    <R.Input
                        id="raKeepVersionNumber"
                        type="number"
                        width="50%"
                        value={dataOptimizeParameter.processActionParameter.archiveVersionValue}
                        onChange={(args) =>
                            onChanged("archiveVersionValue", args)
                        }
                    />
                </R.Validation>
            </div>
        );
    }

    const renderArchiveOnlyVersionSetting = () => {
        return (
            <div style={{ marginLeft: 28 }}>
                <R.Validation
                    element="Input"
                    rules={{
                        customVerify,
                    }}
                >
                    <R.Input
                        id="raArchiveOnlyKeepVersionNumber"
                        type="number"
                        width="50%"
                        value={dataOptimizeParameter.processActionParameter.archiverOnlyLastestVersion}
                        onChange={(args) =>
                            onChanged("archiverOnlyLastestVersion", args)
                        }
                    />
                </R.Validation>
            </div>
        );
    }

    const renderStubSettingsPanel = () => {
        return (
            <R.Panel
                id="raStubSettingsPanel"
                header={RMResx.RM_JS_Rule_Stub_PanelTitle_CreateTemplate}
                size={670}
                status={{ show: showStubSettingsPanel }}
                destroy={true}
                onClose={handleCancelStubSettingsPanel}
            >
                <StubPanel
                    id="stubSettingsPanel"
                    ref={(r) => stubSettingPanelRef.current = r}
                    cellStubId={null}
                    recordsLabelValue={recordsLabelValue}
                ></StubPanel>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={handleCancelStubSettingsPanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={handleSaveStubSettings}
                    />
                </>
            </R.Panel>
        );
    }

    return (
        <R.Expander title={RMResx.RM_FA_DataOptimize_ProcessActionExpander} level={2} status={{ show: true }} togglable={false}>
            <div>
                {
                    isShowFileOption(dataOptimizeParameter) && <div className="reco-optimize-option">
                        <div className="reco-optimize-title">{RMResx.RM_FA_DataOptimize_FileTitle}</div>
                        {
                            dataOptimizeParameter.ms365DataType === MS365DataType.Phl &&
                            <div >
                                <R.Messagebar
                                    message={RMResx.RM_FA_DataOptimize_UnableDeletePhlOrphanedDataWarn}
                                    classify="warn"
                                    hasClose={false}
                                    status={{ show: true }}
                                />
                            </div>
                        }
                        <div role="radiogroup" aria-label={RMResx.RM_FA_DataOptimize_FileTitle}>
                            <div>
                                <R.Radio
                                    name="raFileRadio"
                                    text={RMResx.RM_FA_DataOptimize_File_ArchiveAndRemove}
                                    value={ArchiveOrRemoveFileType.ArchiveAndRemove}
                                    checked={dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.ArchiveAndRemove}
                                    onChange={(value) =>
                                        onChanged("archiveOrRemoveFile", value)
                                    }
                                />
                                <$g.Popover>{RMResx.RM_FA_DataOptimize_File_ArchiveAndRemoveDes}</$g.Popover>
                            </div>
                            {dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.ArchiveAndRemove && <div className="reco-optimize-stub">
                                <R.Checkbox
                                    id="raStubChk"
                                    text={RMResx.RM_FA_DataOptimize_File_LeaveStub}
                                    checked={dataOptimizeParameter.processActionParameter.isEnableLeaveStub}
                                    onChange={(value) =>
                                        onChanged("isEnableLeaveStub", value)
                                    }
                                />
                                <$g.Popover>
                                    <$g.I18NProvider msg={RMResx.RM_FA_DataOptimize_File_LeaveStubDes}>
                                        <a className="ra-link-a" href="/Root/CP/StubSettings">{RMResx.RM_AR_CP_StubSettings}</a>
                                    </$g.I18NProvider>
                                </$g.Popover>
                                {dataOptimizeParameter.processActionParameter.isEnableLeaveStub && renderLeaveStubSetting()}
                            </div>}
                            
                            {/* Hide Include Records option when PHL mode is active */}
                            {dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.ArchiveAndRemove && (
                                <div className={`reco-optimize-stub ${dataOptimizeParameter.processActionParameter.isEnableLeaveStub && "margin-top-s"}`}>
                                    <R.Checkbox
                                        id="raInCludeRecords"
                                        text={RMResx.RM_FA_DataOptimize_File_IncludeRecords}
                                        checked={dataOptimizeParameter.processActionParameter.deleteRecords}
                                        onChange={(value) => onChanged("deleteRecords", value)}
                                    />
                                    <$g.Popover>{RMResx.RM_FA_DataOptimize_File_IncludeRecordsDes}</$g.Popover>
                                </div>
                            )}
                            
                            {/* Hide Archive Version option when PHL mode is active */}
                            {dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.ArchiveAndRemove && (
                                <div className={`reco-optimize-stub`}>
                                    <R.Checkbox
                                        id="raPreviousVersions"
                                        text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                        checked={dataOptimizeParameter.processActionParameter.isArchiveVersionOption}
                                        onChange={(value) => onChanged("isArchiveVersionOption", value)}
                                    />
                                    <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                                    {dataOptimizeParameter.processActionParameter.isArchiveVersionOption && renderArchiveVersionSetting()}
                                </div>
                            )}
                            {/* Hide Remove File option when PHL mode is active */}
                            {RM.gData.enableDeleteOnly && <div>
                                    <R.Radio
                                        name="raFileRadio"
                                        text={RMResx.RM_FA_DataOptimize_File_RemoveFile}
                                        value={ArchiveOrRemoveFileType.Remove}
                                        checked={dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Remove}
                                        onChange={(value) =>
                                            onChanged("archiveOrRemoveFile", value)
                                        }
                                    />
                                    <$g.Popover>{RMResx.RM_FA_DataOptimize_File_RemoveFileDes}</$g.Popover>
                                </div>
                            }
                            {dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Remove && (
                                <div className="reco-optimize-stub">
                                    <R.Checkbox
                                        id="raInCludeRecords"
                                        text={RMResx.RM_FA_DataOptimize_File_IncludeRecords}
                                        checked={dataOptimizeParameter.processActionParameter.deleteRecords}
                                        onChange={(value) => onChanged("deleteRecords", value)}
                                    />
                                    <$g.Popover>{RMResx.RM_FA_DataOptimize_File_IncludeRecordsDes}</$g.Popover>
                                </div>
                            )}
                            {dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Remove && (
                                <div className="reco-optimize-stub">  
                                    <R.Checkbox
                                        id="raDeleteToRecycleBin"  
                                        text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                        checked={dataOptimizeParameter.processActionParameter.deleteRecordToRecycleBin}
                                        onChange={(value) => onChanged("deleteRecordToRecycleBin", value)}
                                    /> 
                                </div>
                            )}
                            {LicenseHelper.EnableArchiverOnly() && (
                                <>
                                    <div>
                                        <R.Radio
                                            name="raFileRadio"
                                            text={RMResx.RM_FA_DataOptimize_File_ArchiveFile}
                                            value={ArchiveOrRemoveFileType.Archive}
                                            checked={dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Archive}
                                            onChange={(value) =>
                                                onChanged("archiveOrRemoveFile", value)
                                            }
                                        />
                                        <$g.Popover>{RMResx.RM_FA_DataOptimize_File_ArchiveFileDesc}</$g.Popover>
                                    </div>
                                    {dataOptimizeParameter.processActionParameter.archiveOrRemoveFile === ArchiveOrRemoveFileType.Archive && (
                                            <div className={`reco-optimize-stub`}>
                                                <R.Checkbox
                                                    id="raPreviousVersions"
                                                    text={RMResx.RM_JS_Rule_ArchiveVersionAndDestroyFile}
                                                    checked={dataOptimizeParameter.processActionParameter.isArchiveOnlyVersionOption}
                                                    onChange={(value) => onChanged("isArchiveOnlyVersionOption", value)}
                                                />
                                                <$g.Popover>{RMResx.RM_JS_Rule_ArchiveVersion_Message}</$g.Popover>
                                                {dataOptimizeParameter.processActionParameter.isArchiveOnlyVersionOption && renderArchiveOnlyVersionSetting()}
                                            </div>
                                    )}
                                </>
                            )}
                        </div>
                    </div>
                }

                {
                    isShowVersionOption(dataOptimizeParameter) && <div>
                        <div className="reco-optimize-title">{RMResx.RM_FA_DataOptimize_VersionTitle}</div>
                        <div role="radiogroup" aria-label={RMResx.RM_FA_DataOptimize_VersionTitle}>
                            <div>
                                <R.Radio
                                    name="raVersionRadio"
                                    text={RMResx.RM_FA_DataOptimize_Version_ArchiveAndRemove}
                                    value={ArchiveOrRemoveVersionType.ArchiveAndRemove}
                                    checked={dataOptimizeParameter.processActionParameter.archiveOrRemoveVersion === ArchiveOrRemoveVersionType.ArchiveAndRemove}
                                    onChange={(value) =>
                                        onChanged("archiveOrRemoveVersion", value)
                                    }
                                />
                                <$g.Popover>{RMResx.RM_FA_DataOptimize_Version_ArchiveAndRemoveDes}</$g.Popover>
                            </div>
                            {
                                RM.gData.enableDeleteOnly && <div>
                                    <R.Radio
                                        name="raVersionRadio"
                                        text={RMResx.RM_FA_DataOptimize_Version_RemoveVersion}
                                        value={ArchiveOrRemoveVersionType.Remove}
                                        checked={dataOptimizeParameter.processActionParameter.archiveOrRemoveVersion === ArchiveOrRemoveVersionType.Remove}
                                        onChange={(value) =>
                                            onChanged("archiveOrRemoveVersion", value)
                                        }
                                    />
                                    <$g.Popover>{RMResx.RM_FA_DataOptimize_Version_RemoveVersionDes}</$g.Popover>
                                </div>
                            }
                            {
                                dataOptimizeParameter.processActionParameter.archiveOrRemoveVersion === ArchiveOrRemoveVersionType.Remove && <div className="reco-optimize-deleteToRecycleBin">
                                    <R.Checkbox
                                        id="raDeleteToRecycleBin"  
                                        text={RMResx.RM_JS_Rule_Delete_RecycleBinOption}
                                        checked={dataOptimizeParameter.processActionParameter.deleteVersionToRecycleBin}
                                        onChange={(value) => onChanged("deleteVersionToRecycleBin", value)}
                                    />
                                </div>
                            }
                        </div>
                    </div>
                }
                {LicenseHelper.EnableRecordsArchiver() && renderStubSettingsPanel()}
            </div>
        </R.Expander>
    );
};

export default ProcessActionExpander;
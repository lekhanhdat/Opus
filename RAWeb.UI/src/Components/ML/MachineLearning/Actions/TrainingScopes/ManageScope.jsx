import {
    forwardRef,
    useEffect,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";

import {
    MTSSourceFlag,
    RAMessageType,
    TrainingMode,
} from "../../Config/Constains";
import { LicenseHelper, showToast } from "../../../../../Utilities/CommonUtil";

const DefaultTrainingScopeInfo = {
    locationId: "",
    location: "",
    sourceFlag: MTSSourceFlag.SPO,
    trainingScopeOption: TrainingMode.Auto,
};

const ManageTrainingScope = ({ trainingScopeInfo, doAction }, ref) => {
    const allValidationRef = useRef();
    const [showManageScopePanel, setShowManageScopePanel] = useState(false);
    const [fileLocationOptions, setFileLocationOptions] = useState([]);
    const [trainingScopeInfos, setTrainingScopeInfos] = useState(
        _.cloneDeep(DefaultTrainingScopeInfo)
    );
    const [locationItems, setLocationItems] = useState([]);

    useImperativeHandle(ref, () => ({
        openPanel: () => setShowManageScopePanel(true),
    }));

    useEffect(() => {
        setLocationItems((prev) => {
            let items = [...prev];

            if (LicenseHelper.HasOpusILLicense()) {
                items = [
                    {
                        name: RMResx.RM_ML_TrainingScope_ManagePanel_Location_SPO,
                        value: MTSSourceFlag.SPO,
                        checked: prev.length > 1,
                    },
                    ...items,
                ];
            }

            if (LicenseHelper.HasOpusGoogleLicense()) {
                items = [
                    ...items,
                    {
                        name: RMResx.RM_ML_TrainingScope_ManagePanel_Location_GoogleDrive,
                        value: MTSSourceFlag.GoogleDrive,
                        checked: prev.length === 1,
                    },
                ];
            }

            return items;
        });
    }, []);

    useEffect(() => {
        if (trainingScopeInfo) {
            setTrainingScopeInfos(trainingScopeInfo);
            if (
                trainingScopeInfo.trainingScopeOption === TrainingMode.Location
            ) {
                const clonedLocationItems = _.cloneDeep(locationItems);
                clonedLocationItems.forEach((item) => {
                    item.checked = item.value === trainingScopeInfo.sourceFlag;
                });
                setLocationItems(clonedLocationItems);
            }
            if (trainingScopeInfo.sourceFlag === MTSSourceFlag.GoogleDrive || trainingScopeInfo.sourceFlag == MTSSourceFlag.None) {
                handleGetAllGoogleDriveName(trainingScopeInfo);
            }
        }
    }, [trainingScopeInfo]);

    const handleRadioGroupChange = (value) => {
        const clonedTrainingScopeInfos = _.cloneDeep(trainingScopeInfos);
        clonedTrainingScopeInfos.trainingScopeOption = value;
        if (value === TrainingMode.Location) {
            const clonedLocationItems = _.cloneDeep(locationItems);
            clonedLocationItems.forEach((item) => {
                item.checked = item.value === (LicenseHelper.HasOpusILLicense() ? MTSSourceFlag.SPO : MTSSourceFlag.GoogleDrive);
            });
            clonedTrainingScopeInfos.sourceFlag = (LicenseHelper.HasOpusILLicense() ? MTSSourceFlag.SPO : MTSSourceFlag.GoogleDrive);
            setLocationItems(clonedLocationItems);
        }
        setTrainingScopeInfos(clonedTrainingScopeInfos);
    };

    const handleChangeManageLocation = (args) => {
        const newValue = args.newValue.value;
        const clonedTrainingScopeInfos = _.cloneDeep(trainingScopeInfos);
        clonedTrainingScopeInfos.sourceFlag = newValue;
        clonedTrainingScopeInfos.locationId = "";
        clonedTrainingScopeInfos.location = "";
        setTrainingScopeInfos(clonedTrainingScopeInfos);

        if (newValue === MTSSourceFlag.GoogleDrive) {
            handleGetAllGoogleDriveName(trainingScopeInfos);
        }
    };

    const handleChangeLocationInp = (value) => {
        const clonedTrainingScopeInfos = _.cloneDeep(trainingScopeInfos);
        clonedTrainingScopeInfos.location = value;
        setTrainingScopeInfos(clonedTrainingScopeInfos);
    };

    const handleChangeFileLocation = (args) => {
        const clonedTrainingScopeInfos = _.cloneDeep(trainingScopeInfos);
        clonedTrainingScopeInfos.locationId = args.newValue.value;
        clonedTrainingScopeInfos.location = args.newValue.name;
        setTrainingScopeInfos(clonedTrainingScopeInfos);
    };

    const onCloseManageScopePanel = () => {
        setShowManageScopePanel(false);
    };

    const onSaveManageScope = async () => {
        if (
            trainingScopeInfos.trainingScopeOption === TrainingMode.Location &&
            !$$.verify(allValidationRef.current)
        ) {
            return false;
        }

        const requestOption = {
            url: "/api/TrainingScopeApi/ChangeTrainingScopeOption",
            method: "POST",
            data: trainingScopeInfos,
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        if (res) {
            if (res.MessageType === RAMessageType.Successful) {
                doAction("REFRESH");
                onCloseManageScopePanel();
            } else {
                if (res.ErrorMessage) {
                    showToast.error(res.ErrorMessage);
                } else {
                    showToast.error(
                        RMResx.RM_ML_IntelligentTerm_SwitchFailedMsg
                    );
                }
            }
        }
        $$.loading(false);
    };

    const handleGetAllGoogleDriveName = async (trainingScopeInfos) => {
        const requestOption = {
            url: "/api/TrainingScopeApi/GetAllGoogleDriveName?searchKey=",
            method: "GET",
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        $$.loading(false);
        if (res) {
            const result = Object.entries(res).map(([value, name]) => ({
                name,
                value,
                checked: trainingScopeInfos.locationId === value,
            }));
            setFileLocationOptions(result);
        }
    };

    const renderLoadFileFrom = () => {
        if (!LicenseHelper.HasOpusILLicense() && !LicenseHelper.HasOpusGoogleLicense()) {
            return null;
        }

        return (
            <R.Validation>
                <div ref={allValidationRef}>
                    <div
                        id="raMlManageScopeLocationLoadFileFromLabel"
                        className="ra-setting-panel-title margin-top-m margin-bottom-xs require"
                        tabIndex={0}
                    >
                        {RMResx.RM_ML_TrainingScope_ManagePanel_Location_Label}
                    </div>
                    {LicenseHelper.HasOpusILLicense() && trainingScopeInfos.sourceFlag === MTSSourceFlag.SPO && (
                        <div>
                            <R.Validation element="Input" require>
                                <R.Input
                                    id="raMlManageScopeLocationLoadFileFromIpt"
                                    type="text"
                                    placeholder={RMResx.RM_ML_TrainingScope_ManagePanel_Location_Placeholder}
                                    value={trainingScopeInfos.location}
                                    onChange={handleChangeLocationInp}
                                />
                            </R.Validation>
                        </div>
                    )}
                    {LicenseHelper.HasOpusGoogleLicense() && trainingScopeInfos.sourceFlag ===
                        MTSSourceFlag.GoogleDrive && (
                            <div>
                                <R.Validation element="Combobox" require>
                                    <R.Combobox
                                        id="raMlManageScopeLocationLoadFileFromCbx"
                                        textField="name"
                                        valueField="value"
                                        checkedField="checked"
                                        tooltipField="name"
                                        template={(item) => (
                                            <div className="flex align-center gap-s">
                                                <span style={{ marginTop: 2 }} className="fia-google-drive-f font-m">
                                                    <span className="path1"></span>
                                                    <span className="path2"></span>
                                                    <span className="path3"></span>
                                                    <span className="path4"></span>
                                                    <span className="path5"></span>
                                                    <span className="path6"></span>
                                                </span>
                                                <div>{item.name}</div>
                                            </div>
                                        )}
                                        items={fileLocationOptions}
                                        onChange={handleChangeFileLocation}
                                    />
                                </R.Validation>
                            </div>
                        )}
                </div>
            </R.Validation>
        );
    };

    return (
        <div>
            <R.Panel
                id="raMtManageTrainingScopePanel"
                header={RMResx.RM_ML_TrainingScope_ManageBtn}
                size={664}
                status={{ show: showManageScopePanel }}
                onHide={onCloseManageScopePanel}
                destroy={true}
            >
                <div
                    id="raMlManageScopeTable"
                    className="flex flex-column gap-s"
                >
                    <div tabIndex="0" className="ra-setting-panel-title flex align-center">
                        {RMResx.RM_ML_TrainingScope_ManagePanel_Title}
                        <$g.Popover>{RMResx.RM_ML_TrainingScope_ManagePanel_Title_Desc}</$g.Popover>
                    </div>
                    <div className="flex flex-column gap-s">
                        <div>
                            <R.Radio
                                name={"raMlmanageScopeRadio"}
                                text={
                                    RMResx.RM_ML_TrainingScope_ManagePanel_Option01
                                }
                                value={TrainingMode.Auto}
                                checked={
                                    trainingScopeInfos.trainingScopeOption ===
                                    TrainingMode.Auto
                                }
                                onChange={handleRadioGroupChange}
                            />
                        </div>
                        <div>
                            <R.Radio
                                name={"raMlmanageScopeRadio"}
                                text={
                                    RMResx.RM_ML_TrainingScope_ManagePanel_Option03
                                }
                                value={TrainingMode.Manual}
                                checked={
                                    trainingScopeInfos.trainingScopeOption ===
                                    TrainingMode.Manual
                                }
                                onChange={handleRadioGroupChange}
                            />
                        </div>
                        <div>
                            <R.Radio
                                name={"raMlmanageScopeRadio"}
                                text={RMResx.RM_ML_TrainingScope_ManagePanel_Option02}
                                value={TrainingMode.Location}
                                checked={
                                    trainingScopeInfos.trainingScopeOption ===
                                    TrainingMode.Location
                                }
                                onChange={handleRadioGroupChange}
                            />
                            {trainingScopeInfos.trainingScopeOption ===
                                TrainingMode.Location && (
                                    <div
                                        style={{ width: "auto", marginLeft: 28 }}
                                        className="margin-top-s"
                                    >
                                        <R.Combobox
                                            id="raMlmanageScopeLocationCbx"
                                            textField="name"
                                            valueField="value"
                                            checkedField="checked"
                                            tooltipField="tooltip"
                                            width="100%"
                                            searchable={false}
                                            items={locationItems}
                                            onChange={handleChangeManageLocation}
                                        />
                                    </div>
                                )}
                        </div>
                    </div>
                    {trainingScopeInfos.trainingScopeOption ===
                        TrainingMode.Location && renderLoadFileFrom()}
                </div>
                <>
                    <R.Button
                        slot="buttons"
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={onCloseManageScopePanel}
                    />
                    <R.Button
                        slot="buttons"
                        primary
                        classify="theme"
                        text={RMResx.RM_JS_Common_Save}
                        onClick={onSaveManageScope}
                    />
                </>
            </R.Panel>
        </div>
    );
};

export default forwardRef(ManageTrainingScope);

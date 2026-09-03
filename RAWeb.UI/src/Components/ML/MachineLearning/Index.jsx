import { useEffect, useState } from "react";
import _ from "lodash";
import { MLTabIndex, MLModelStatus, RAMessageType } from "./Config/Constains";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import IntelligentTerm from "./Main/IntelligentTerm";
import TrainingReport from "./Main/TrainingReport";
import TrainingScope from "./Main/TrainingScope";
import ZeroIntelligentTerm from "./Zero/Main/IntelligentTerm";
import "./Index.less";
import { showToast } from "../../../Utilities/CommonUtil";

const MachineLearning = () => {
    const [activeTab, setActiveTab] = useState(MLTabIndex.IntelligentTerm);

    const [termId, setTermId] = useState("");

    const [isShowTip, setIsShowTip] = useState(false);

    const [tipType, setTipType] = useState("");

    const [tipMsg, setTipMsg] = useState("");

    const [updateTime, setUpdateTime] = useState("");

    const [modeItems, setModeItems] = useState([
        {
            name: RMResx.RM_ML_IntelligentTerm_Tab_Zero,
            value: 1,
            checked: false,
        },
        {
            name: RMResx.RM_ML_IntelligentTerm_Tab_ML,
            value: 0,
            checked: true,
        },
    ]);

    const [modeValue, setModeValue] = useState(0);

    useEffect(() => {
        if (RM.gData.enableMachineLearningFeature && !RM.gData.enableZeroShotFeature) {
            setModeValue(0);
        } else if (!RM.gData.enableMachineLearningFeature && RM.gData.enableZeroShotFeature) {
            setModeValue(1);
        } else {
            getCurrentMode();
        }
    }, []);

    useEffect(() => {
        initMachineLearning();
    }, []);

    const getCurrentMode = () => {
        const requestOption = {
            url: "/api/RMMLTermApi/GetCurrentMode",
            method: "GET",
        };
        $$.loading(true);
        const res = fetchUtility(requestOption)
            .then((res) => {
                const cloneModeItems = _.cloneDeep(modeItems);
                cloneModeItems.forEach((item) => {
                    item.checked = item.value === res;
                });
                setModeValue(res);
                setModeItems(cloneModeItems);
            })
            .finally(() => {
                $$.loading(false);
            });
    };

    const setMLUpdateTime = async () => {
        const requestOption = {
            url: "/api/RMMLTermApi/GetLastUpdatedTime",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        setUpdateTime(result);
        $$.loading(false);
    };

    const setTrainingStatusTip = async () => {
        const requestOption = {
            url: "/api/TrainingScopeApi/GetTrainingModelStatus",
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result === MLModelStatus.Running) {
            setTipInfo("warn", RMResx.RM_ML_TrainStatus_Tip_Running);
            return;
        }
        if (
            result === MLModelStatus.Failed ||
            result === MLModelStatus.Exception
        ) {
            setTipInfo("error", RMResx.RM_ML_TrainStatus_Tip_Failed);
            return;
        }
        setIsShowTip(false);
    };

    const initMachineLearning = () => {
        if (
            RM.gData.enableMachineLearningFeature &&
            (!RM.gData.enableZeroShotFeature || modeValue === 0)
        ) {
            setMLUpdateTime();
        }
        setTrainingStatusTip();
    };

    const setTipInfo = (tipType, tipMsg) => {
        setIsShowTip(true);
        setTipType(tipType);
        setTipMsg(tipMsg);
    };

    const onChangeTabPanel = (index) => {
        setActiveTab(index);
        setTermId("");
    };

    const switchTabToTrainingScope = (termId) => {
        setActiveTab(MLTabIndex.TrainingScope);
        setTermId(termId);
    };

    const onModeChange = async (args) => {
        const newValue = args.newValue.value;
        const requestOption = {
            url: "/api/RMMLTermApi/SwitchMode",
            method: "POST",
            data: newValue,
        };
        $$.loading(true);
        const res = await fetchUtility(requestOption);
        if (res) {
            if (res.MessageType === RAMessageType.Successful) {
                const cloneModeItems = _.cloneDeep(modeItems);
                cloneModeItems.forEach((item) => {
                    item.checked = item.value === newValue;
                });
                setModeValue(newValue);
                setModeItems(cloneModeItems);
            } else {
                showToast.error(RMResx.RM_ML_IntelligentTerm_SwitchFailedMsg);
            }
        }
        $$.loading(false);
    };

    const onCheckPredictionJobRunning = async () => {
        let errorMessage = "";
        const requestOption = {
            url: "/api/RMMLTermApi/CheckPredictionJobRunning",
            method: "POST",
            data: 0, // Switch mode
        };
        $$.loading(true);
        let result = await fetchUtility(requestOption);
        $$.loading(false);
        if (result.MessageType != RAMessageType.Successful) {
            errorMessage = result.ErrorMessage;
        }
        return errorMessage;
    };

    const openChangeModeMessagebox = async (argsParam) => {
        await $$.messagedialog(false); // Close the previous message dialog if any
        const args = {
            width: "550px",
            title: RMResx.RM_ML_IntelligentTerm_SwitchMode_ConfirmTitle,
            content: (
                <span tabIndex="0">
                    {modeValue === 1
                        ? RMResx.RM_ML_IntelligentTerm_SwitchMode_ZeroToML
                        : RMResx.RM_ML_IntelligentTerm_SwitchMode_MLToZero}
                </span>
            ),
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    id: "mtSwitchButton",
                    text: RMResx.RM_ML_IntelligentTerm_SwitchBtn,
                    primary: true,
                    classify: "theme",
                    onClick: () => onModeChange(argsParam),
                },
            ],
        };
        $$.messagedialog(true, args);
        return false;
    };

    const onWillModeChange = async (argsParam) => {
        const errorMessage = await onCheckPredictionJobRunning();
        if (errorMessage) {
            const args = {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: <span tabIndex="0">{errorMessage}</span>,
                buttons: [
                    {
                        id: "mtSwitchConfirmButton",
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                    },
                ],
            };
            $$.messagedialog(true, args);
        } else {
            openChangeModeMessagebox(argsParam);
        }
        return false;
    };

    const renderSiteMap = () => {
        let content = null;

        if (
            RM.gData.enableZeroShotFeature &&
            RM.gData.enableMachineLearningFeature
        ) {
            content = (
                <div className="flex justify-end align-center gap-s">
                    <div className="fia-mode"></div>
                    <R.Combobox
                        items={modeItems}
                        textField="name"
                        tooltipField="name"
                        valueField="value"
                        checkedField="checked"
                        willChange={onWillModeChange}
                        searchable={false}
                    />
                </div>
            );

            if (modeValue === 1) {
                content = (
                    <div className="flex justify-end align-center gap-s">
                        <div className="fia-mode"></div>
                        <R.Combobox
                            items={modeItems}
                            textField="name"
                            tooltipField="name"
                            valueField="value"
                            checkedField="checked"
                            willChange={onWillModeChange}
                            searchable={false}
                        />
                    </div>
                );
            } else {
                content = (
                    <div className="flex flex-column gap-xs">
                        <div className="flex justify-end align-center gap-s">
                            <div className="fia-mode"></div>
                            <R.Combobox
                                items={modeItems}
                                textField="name"
                                tooltipField="name"
                                valueField="value"
                                checkedField="checked"
                                willChange={onWillModeChange}
                                searchable={false}
                            />
                        </div>
                        {updateTime && (
                            <div className="ra-ml-update-time">
                                <span tabIndex="0">
                                    {RMResx.RM_ML_LastUpdateTime.format(
                                        updateTime
                                    )}
                                </span>
                                <$g.Popover>
                                    <div>{RMResx.RM_ML_LastUpdateTime_Des}</div>
                                </$g.Popover>
                            </div>
                        )}
                    </div>
                );
            }
        } else {
            if (updateTime) {
                content = (
                    <div className="ra-ml-update-time">
                        <span tabIndex="0">
                            {RMResx.RM_ML_LastUpdateTime.format(updateTime)}
                        </span>
                        <$g.Popover>
                            <div>{RMResx.RM_ML_LastUpdateTime_Des}</div>
                        </$g.Popover>
                    </div>
                );
            }
        }

        return (
            <$g.SiteMap data={[SiteMapLinks.MT_MachineLearning]}>
                {content}
            </$g.SiteMap>
        );
    };

    const renderMessageBar = () => {
        return (
            <div className="margin-bottom-m">
                <R.Messagebar
                    message={tipMsg}
                    classify={tipType}
                    onClose={() => {
                        setIsShowTip(false);
                    }}
                    status={{ show: isShowTip }}
                />
            </div>
        );
    };

    // const renderQuickStartTraining = () =>{
    //     return <QuickStartTraining
    //         containerType={RenderTermsContainer.Dialog}
    //     ></QuickStartTraining>;
    // };

    const renderMaestroAIContent = () => {
        if (modeValue === 1) {
            return renderZeroAIContent();
        }

        return renderMLContent();
    };

    const renderZeroAIContent = () => {
        return (
            <R.Tabcontrol flex destroy active={0}>
                <R.TabPanel tab={RMResx.RM_ML_IntelligentTerm}>
                    <ZeroIntelligentTerm />
                </R.TabPanel>
            </R.Tabcontrol>
        );
    };

    const renderMLContent = () => {
        return (
            <R.Tabcontrol flex onChange={onChangeTabPanel} active={activeTab}>
                <R.TabPanel tab={RMResx.RM_ML_IntelligentTerm}>
                    {activeTab === MLTabIndex.IntelligentTerm && (
                        <IntelligentTerm
                            clickTrainingScope={switchTabToTrainingScope}
                            refresh={initMachineLearning}
                        />
                    )}
                </R.TabPanel>
                <R.TabPanel tab={RMResx.RM_ML_TrainingScope}>
                    {activeTab === MLTabIndex.TrainingScope && (
                        <TrainingScope termId={termId} />
                    )}
                </R.TabPanel>
                <R.TabPanel tab={RMResx.RM_ML_TrainingReport}>
                    {activeTab === MLTabIndex.TrainingReport && (
                        <TrainingReport />
                    )}
                </R.TabPanel>
            </R.Tabcontrol>
        );
    };

    return (
        <div id="raMachineLeaning">
            {renderSiteMap()}
            {renderMessageBar()}
            {renderMaestroAIContent()}
            {/* {renderQuickStartTraining()} */}
        </div>
    );
};

export default MachineLearning;

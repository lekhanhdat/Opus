import { useEffect, useRef, useState } from "react";
import _ from "lodash";

import {
    FormatSelectionComponent,
    MetadataMappingComponent,
    ExportLocationComponent,
} from "./components";
import Wizard from "../../Common/Wizard";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import {
    ConfiguratorContentSource,
    ConfiguratorFormatValue,
    ConfiguratorSteps,
    ConfiguratorStepStatus,
    RAMessageType,
} from "./Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import { showToast } from "../../../Utilities/CommonUtil";
import { getConfigurationFormats } from "./utils";

import "./CompliantExports.less";

// MORE IMPORTANT!!! This page is very complicated, please care fully before you edit it.
function CompliantExport({ history }) {
    const [steps, setSteps] = useState(ConfiguratorSteps);

    const [allExportSettings, setAllExportSettings] = useState([]);
    const [filteredExportSettingsByFormat, setFilteredExportSettingsByFormat] =
        useState([]);
    const [hasUpgradeVEOV3, setHasUpgradeVEOV3] = useState(false);
    const [formatSelection, setFormatSelection] = useState({
        list: getConfigurationFormats(ConfiguratorFormatValue.VEO, false),
        selected: ConfiguratorFormatValue.VEO,
    });

    // Step 3 Export Location states
    const [exportLocationList, setExportLocationList] = useState([]);
    const [exportLocationId, setExportLocationId] = useState("");

    const metadataMappingRef = useRef(null);

    useEffect(() => {
        const clonedSteps = _.cloneDeep(ConfiguratorSteps);
        setSteps(clonedSteps);
    }, []);

    // Init all export settings data
    useEffect(() => {
        loadExportSettingByFormatType();
    }, []);

    // Get data for step 3 Export Location
    useEffect(() => {
        $$.loading(true);
        const option = {
            url: "/api/CPApi/GetSavedFileInfos",
        };
        fetchUtility(option)
            .then((res) => {
                const exportList = [];
                const hasUpgradeVEOV3 = res.HasUpgradeVEOV3 || false;
                res.StorageInfo?.forEach((item) => {
                    item.checked =
                        item.Id === res.CurrentExportLocationId ? true : false;
                    exportList.push(item);
                });
                const formatList = getConfigurationFormats(null, hasUpgradeVEOV3);
                const defaultSelectedFormat = formatList.find((item) => [ConfiguratorFormatValue.VEO, ConfiguratorFormatValue.NAA].includes(item.value))?.value ?? formatList[0]?.value;
                setHasUpgradeVEOV3(hasUpgradeVEOV3);
                setFormatSelection({
                    list: getConfigurationFormats(
                        defaultSelectedFormat,
                        hasUpgradeVEOV3
                    ),
                    selected: defaultSelectedFormat,
                });
                setExportLocationList(exportList);
                setExportLocationId(res.CurrentExportLocationId || "");
            })
            .catch(() => {
                showToast.error(RMResx.RM_RDM_Explorer_ChangeTerm_All_Failed);
            })
            .finally(() => $$.loading(false));
    }, []);

    const loadExportSettingByFormatType = () => {
        $$.loading(true);
        const option = {
            url: "/api/ExportSetting/LoadAllExportSettings",
            method: "POST",
        };
        fetchUtility(option)
            .then((res) => {
                if (Array.isArray(res) && res.length > 0) {
                    setAllExportSettings(res);
                }
            })
            .catch(() => console.error("Get all export settings failed!"))
            .finally(() => $$.loading(false));
    };

    const changeStepToProcessing = (step) => {
        const nextProcessingStep = step;
        const clonedSteps = steps.map((item) => {
            if (item.step < nextProcessingStep) {
                item.status = ConfiguratorStepStatus.Completed;
            } else if (item.step > nextProcessingStep) {
                item.status = ConfiguratorStepStatus.Waiting;
            } else {
                item.status = ConfiguratorStepStatus.Processing;
            }
            return item;
        });
        setSteps(clonedSteps);
    };

    const isProgressingStep = (step) => {
        return (
            steps.find((item) => item.step === step)?.status ===
            ConfiguratorStepStatus.Processing
        );
    };

    // Step 1 Format Selection function
    const handleChangeFormatSelection = (newValue) => {
        setFormatSelection({
            list: getConfigurationFormats(newValue, hasUpgradeVEOV3),
            selected: newValue,
        });
    };

    // Step 2 Metadata Mapping function
    const handleChangeChildTableItems = (
        selectedContentSource,
        updatedChildRows
    ) => {
        let matchedSetting = allExportSettings.find(
            (item) =>
                item.ExportType === formatSelection.selected &&
                item.SourceFlag === selectedContentSource
        );
        if (matchedSetting) {
            if (formatSelection.selected === ConfiguratorFormatValue.VEO) {
                matchedSetting = { ...updatedChildRows };
            } else {
                matchedSetting.ExportColumnInfoes = [...updatedChildRows];
            }
            setAllExportSettings((prev) =>
                prev.map((item) => {
                    if (
                        item.ExportType === formatSelection.selected &&
                        item.SourceFlag === selectedContentSource
                    ) {
                        return { ...matchedSetting };
                    }
                    return item;
                })
            );
            setFilteredExportSettingsByFormat((prev) =>
                prev.map((item) => {
                    if (item.SourceFlag === selectedContentSource) {
                        return { ...matchedSetting };
                    }
                    return item;
                })
            );
        }
    };

    // Step 3 Export Location function
    const handleChangeExportLocation = (args) => {
        setExportLocationId(args.newValue.Id);
    };

    // Footer button functions
    const handleCancel = () => {
        history.push({
            pathname: RouterUrls.CP_ExportSettings,
        });
    };

    const handleBack = () => {
        const processingStep = steps.find(
            (item) => item.status === ConfiguratorStepStatus.Processing
        );

        if (metadataMappingRef.current?.isEditingNodeMetadataName() && processingStep.step === 2) {
            metadataMappingRef.current?.saveMetadataName();
        }

        changeStepToProcessing(processingStep.step - 1);
        setTimeout(() => {
            $(".ce-component-title-main:visible span").focus();
        }, 100);
    };

    const handleNext = () => {
        if (!metadataMappingRef.current?.validateMetadataName()) return false;

        const processingStep = steps.find(
            (item) => item.status === ConfiguratorStepStatus.Processing
        );

        if (processingStep.step === 1) {
            // 1 is select format step
            const filteredExportSettingsByFormat = allExportSettings.filter(
                (item) => item.ExportType === formatSelection.selected
            );

            if (
                filteredExportSettingsByFormat &&
                filteredExportSettingsByFormat.length > 0
            ) {
                setFilteredExportSettingsByFormat(
                    filteredExportSettingsByFormat
                );
            }
        }

        if (metadataMappingRef.current?.isEditingNodeMetadataName() && processingStep.step === 2) {
            metadataMappingRef.current.saveMetadataName();
        }

        // if (processingStep.ref.current) {
        //     if (processingStep.ref.current.onValidate()) {
        //         changeStepToProcessing(processingStep.step + 1);
        //         getJobInfo(processingStep.step + 1);
        //         setTimeout(() => {
        //             $(".reco-ac-component-title-main:visible span").focus();
        //         }, 100);
        //     } else {
        //         $(".reco-error-message").eq(0).focus();
        //     }
        // }
        changeStepToProcessing(processingStep.step + 1);
        setTimeout(() => {
            $(".ce-component-title-main:visible span").focus();
        }, 100);
    };

    const handleSave = () => {
        $$.loading(true);
        const veoExportInfos = allExportSettings.filter((item) => item.ExportType === ConfiguratorFormatValue.VEO)
        const naaExportInfos = allExportSettings.filter((item) => item.ExportType === ConfiguratorFormatValue.NAA)
        const naraExportInfos = allExportSettings.filter((item) => item.ExportType === ConfiguratorFormatValue.NARA)
        const option = {
            url: "/api/ExportSetting/SaveExportSetting",
            method: "POST",
            data: {
                VEOExportInfos: veoExportInfos || [],
                NAAExportInfos: naaExportInfos,
                NARAExportInfos: naraExportInfos,
                DefaultStorageDeviceId: exportLocationId,
            },
        };
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType === RAMessageType.Successful) {
                    showToast.success(RMResx.RM_ES_CompliantExport_SaveSuccess);
                    handleCancel();
                } else {
                    showToast.error(res.ErrorMessage);
                }
            })
            .catch(() => console.error("Save failed!"))
            .finally(() => $$.loading(false));
    };

    const renderPromptForLocal = (onClick) => {
        const args = {
            width: "550px",
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_ES_CompliantExport_Wizard_PromptLeave,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel,
                    onClick: () => $$.messagedialog(false),
                },
                {
                    id: "raCancel",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: async () => {
                        await $$.messagedialog(false);
                        onClick();
                    },
                },
            ],
        }
        $$.messagedialog(true, args);
    }

    return (
        <div id="raCPCompliantExports">
            <$g.SiteMap
                data={[
                    SiteMapLinks.CP,
                    SiteMapLinks.CP_ExportSettings,
                    SiteMapLinks.CP_ExportSettings_CompliantExports,
                ]}
            />
            <div className="ce-configurator-container">
                <Wizard
                    headerName={RMResx.RM_ES_CompliantExport_Wizard_Title}
                    activeStep={
                        steps.find(
                            (item) =>
                                item.status ===
                                ConfiguratorStepStatus.Processing
                        )?.step || 1
                    }
                    items={steps.map((step) => ({ text: step.name }))}
                    onChange={(index) => {
                        if (metadataMappingRef.current?.isEditingNodeMetadataName()) {
                            renderPromptForLocal(() => {
                                metadataMappingRef.current.cancelMetadataName();
                                changeStepToProcessing(index + 1);
                            });
                        } else {
                            changeStepToProcessing(index + 1);
                        }
                    }}
                >
                    <div hidden={!isProgressingStep(1)}>
                        <FormatSelectionComponent
                            formatSelection={formatSelection}
                            onChangeFormatSelection={
                                handleChangeFormatSelection
                            }
                        />
                    </div>
                    <div
                        style={{ height: "100%" }}
                        hidden={!isProgressingStep(2)}
                    >
                        <MetadataMappingComponent
                            ref={metadataMappingRef}
                            filteredExportSettingsByFormat={
                                filteredExportSettingsByFormat
                            }
                            selectedFormat={formatSelection.selected}
                            renderPromptForLocal={renderPromptForLocal}
                            onChangeChildTableItems={
                                handleChangeChildTableItems
                            }
                        />
                    </div>
                    <div hidden={!isProgressingStep(3)}>
                        <ExportLocationComponent
                            exportLocationList={exportLocationList}
                            onChangeExportLocation={handleChangeExportLocation}
                        />
                    </div>
                </Wizard>
                <div className="ce-configurator-bottom">
                    <R.Button
                        primary={false}
                        classify={"default"}
                        text={RMResx.RM_JS_Common_Cancel}
                        onClick={handleCancel}
                    />
                    {steps[0].status !== ConfiguratorStepStatus.Processing && (
                        <R.Button
                            primary={false}
                            classify={"default"}
                            text={RMResx.RM_JS_Common_Back}
                            onClick={handleBack}
                        />
                    )}
                    {steps[2].status === ConfiguratorStepStatus.Processing ? (
                        <R.Button
                            primary={true}
                            classify={"theme"}
                            text={RMResx.RM_JS_Common_Save}
                            onClick={handleSave}
                        />
                    ) : (
                        <R.Button
                            primary={true}
                            classify={"theme"}
                            text={RMResx.RM_JS_Common_Next}
                            onClick={handleNext}
                        />
                    )}
                </div>
            </div>
        </div>
    );
}

export default CompliantExport;

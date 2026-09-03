import React, { useEffect, useRef, useState } from "react";
import _ from "lodash";

import "../index.less";
import { showToast } from "../../../../../Utilities/CommonUtil";
import RouterUrls from "../../../../../Constants/RouterUrls";
import ExclusionsComponent from "../Components/ExclusionsComponent";
import { SalesforceAnalysisConfigurationInactiveComponent, SalesforceAnalysisConfigurationScopeComponent } from './Components'
import { DiscoveryDataSource } from "../Constants";

const DEFAULT_DISCOVERY_INFO = {
    scopeInfo: {
        organizations: [],
    },
    sizeRangeInfoes: [],
    dateRangeInfoes: [],
};

const ConfiguratorStepStatus = {
    None: 0,
    Waiting: 1,
    Processing: 2,
    Completed: 3,
};

const ConfiguratorSteps = [
    {
        step: 1,
        name: RMResx.RM_FA_Discovery_Config_Scope,
        status: ConfiguratorStepStatus.Processing,
    },
    {
        step: 2,
        name: RMResx.RM_FA_Discovery_JobPage_Exclusion_Title,
        status: ConfiguratorStepStatus.Waiting,
    },
    {
        step: 3,
        name: RMResx.RM_FA_Discovery_Config_Inactive,
        status: ConfiguratorStepStatus.Waiting,
    },
];


const AnalysisConfigurationEditPage = ({ history }) => {
    const [configurationInfo, setConfigurationInfo] = useState(DEFAULT_DISCOVERY_INFO);

    const [organizations, setOrganizations] = useState([]);

    const [steps, setSteps] = useState(ConfiguratorSteps);

    const stepsRef = useRef(null);

    useEffect(() => {
        const fetchData = async () => {
            const clonedSteps = _.cloneDeep(ConfiguratorSteps);
            const steps = clonedSteps.map((item) => {
                item.ref = React.createRef();
                return item;
            });

            $$.loading(true);
            const configurationInfos = await fetchUtility({ url: "/api/RMDiscoverySalesforceConfigurationApi/GetConfigurationInfo", method: "Get" });
            if (RM.gData.hasDiscoverySalesforceLicense) {
                const organizationInfos = await fetchUtility({ url: "/api/RMDiscoverySalesforceConfigurationApi/GetOrganizations", method: "GET" });
                setOrganizations(organizationInfos);
            }

            $$.loading(false);

            stepsRef.current = steps;

            // License: RM.gData.hasDiscoverySalesforceLicense
            const clonedConfigurationInfo = {
                ...configurationInfos,
                sizeRangeInfoes: configurationInfos.sizeRangeInfoes || [],
                dateRangeInfoes: configurationInfos.dateRangeInfoes || [],
            }

            setSteps(steps);
            setConfigurationInfo(clonedConfigurationInfo);
        };

        fetchData();
    }, []);

    const getProgressBarClass = (item) => {
        switch (item.status) {
            case ConfiguratorStepStatus.Waiting:
                return "reco-progress-bar-waiting";
            case ConfiguratorStepStatus.Processing:
                return "reco-progress-bar-progressing";
            default:
                return "reco-progress-bar-completed";
        }
    };

    const isProgressingStep = (step) => {
        return (
            steps.find((item) => item.step === step)?.status ===
            ConfiguratorStepStatus.Processing
        );
    };

    const changeStepStatus = (step, steps) => {
        const nextProcessingStep = step;

        return steps.map((item) => {
            if (item.step < nextProcessingStep) {
                item.status = ConfiguratorStepStatus.Completed;
            } else if (item.step > nextProcessingStep) {
                item.status = ConfiguratorStepStatus.Waiting;
            } else {
                item.status = ConfiguratorStepStatus.Processing;
            }
            return item;
        });
    }

    const changeStepToProcessing = (step) => {
        const clonedSteps = changeStepStatus(step, steps);
        stepsRef.current = changeStepStatus(step, stepsRef.current);
        setSteps(clonedSteps);
    };

    const onChange = (field, value) => {
        const clonedConfigurationInfo = Object.assign({}, configurationInfo);
        clonedConfigurationInfo[field] = value;
        setConfigurationInfo(clonedConfigurationInfo);
    };

    const onCancel = () => {
        history.push({
            pathname: RouterUrls.FA_Discovery,
            search: `?dataSource=${DiscoveryDataSource.Salesforce}`,
        });
    };

    const onBack = () => {
        const processingStep = steps.find(
            (item) => item.status === ConfiguratorStepStatus.Processing
        );
        changeStepToProcessing(processingStep.step - 1);
        setTimeout(() => {
            $(".reco-ac-component-title-main:visible span").focus();
        }, 100);
    };

    const onNext = () => {
        const processingStep = steps.find(
            (item) => item.status === ConfiguratorStepStatus.Processing
        );
        if (processingStep.ref.current) {
            if (processingStep.ref.current.onValidate()) {
                changeStepToProcessing(processingStep.step + 1);
                setTimeout(() => {
                    $(".reco-ac-component-title-main:visible span").focus();
                }, 100);
            } else {
                $(".reco-error-message").eq(0).focus();
            }
        }
    };

    const onFinish = async () => {
        const processingStep = steps.find(
            (item) => item.status === ConfiguratorStepStatus.Processing
        );
        if (processingStep.ref.current) {
            if (processingStep.ref.current.onValidate()) {
                $$.messagedialog(true, {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_EnsureDiscovery}</span>,
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_Cancel,
                            onClick: () => {
                                $$.messagedialog(false);
                            },
                        },
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: runDiscoveryDoAction
                        },
                    ],
                });
            } else {
                $(".reco-error-message").eq(0).focus();
            }
        }
    };

    const runDiscoveryDoAction = async () => {
        $$.messagedialog(false);
        $$.loading(true);
        const result = await fetchUtility({
            url: "/api/RMDiscoverySalesforceConfigurationApi/AddOrUpdateNewlyConfigurationInfo",
            data: configurationInfo
        });
        $$.loading(false);
        if (result.MessageType !== 0) {
            showToast.error(result.ErrorMessage);
            return false;
        }
        history.push({
            pathname: RouterUrls.FA_Discovery_RunJob,
            search: `?dataSource=${DiscoveryDataSource.Salesforce}`,
        });
    };

    const onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    return (
        <div className="reco-analysis-configurator-container">
            <div className="reco-fac-panel">
                <div className="reco-fac-progress-panel">
                    <div className="reco-fac-progress">
                        <div className="reco-title">{RMResx.RM_FA_Discovery_ConfigTitle}</div>
                        <div className="reco-progress-bars">
                            {steps.map((item, index) => (
                                <div
                                    key={index}
                                    className={`reco-progress-bar ${getProgressBarClass(
                                        item
                                    )}`}
                                ></div>
                            ))}
                        </div>
                    </div>
                    <div className="reco-fac-steps">
                        {steps.map((item, index) => {
                            return (
                                <div key={index} className="reco-fac-step">
                                    {item.status ===
                                        ConfiguratorStepStatus.Waiting && (
                                        <div className="reco-fac-step-number reco-fac-step-waiting">
                                            {item.step}
                                        </div>
                                    )}
                                    {item.status ===
                                        ConfiguratorStepStatus.Processing && (
                                        <div className="reco-fac-step-number reco-fac-step-processing">
                                            {item.step}
                                        </div>
                                    )}
                                    {item.status ===
                                        ConfiguratorStepStatus.Completed && (
                                        <div
                                            className="reco-fac-step-number reco-fac-step-completed"
                                            onKeyDown={onKeyDown}
                                            onClick={() =>
                                                changeStepToProcessing(
                                                    item.step
                                                )
                                            }
                                        >
                                            <span className="fia-check"></span>
                                        </div>
                                    )}
                                    <div className="reco-fac-step-name">
                                        {item.name}
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
                <div className="reco-fac-form-panel">
                    <div className="reco-fac-form-content">
                        <div
                            style={{
                                display: isProgressingStep(1)
                                    ? "block"
                                    : "none",
                            }}
                        >
                            <SalesforceAnalysisConfigurationScopeComponent
                                allOrganization={organizations}
                                info={configurationInfo.scopeInfo}
                                onChange={(value) => onChange("scopeInfo", value)}
                                ref={steps[0].ref}
                            />
                        </div>
                        <div
                            style={{
                                display: isProgressingStep(2)
                                    ? "block"
                                    : "none",
                            }}
                        >
                            <ExclusionsComponent
                                info={configurationInfo}
                                onChange={(field, value) => {
                                    onChange(field, value);
                                }}
                                ref={steps[1].ref}
                            />
                        </div>
                        <div
                            style={{
                                display: isProgressingStep(3)
                                    ? "block"
                                    : "none",
                            }}
                        >
                            <SalesforceAnalysisConfigurationInactiveComponent ref={steps[2].ref} />
                        </div>
                    </div>
                    <div className="reco-fac-form-bottom">
                        <div>
                            <R.Button
                                primary={false}
                                classify={"default"}
                                text={RMResx.RM_JS_Common_Cancel}
                                onClick={onCancel}
                            />
                        </div>
                        {steps[0].status !== ConfiguratorStepStatus.Processing && (
                            <div>
                                <R.Button
                                    primary={false}
                                    classify={"default"}
                                    text={RMResx.RM_JS_Common_Back}
                                    onClick={onBack}
                                />
                            </div>
                        )}
                        {steps[2].status === ConfiguratorStepStatus.Processing ? (
                                <div>
                                    <R.Button
                                        primary={true}
                                        classify={"theme"}
                                        text={RMResx.RM_JS_Common_Finish}
                                        onClick={onFinish}
                                    />
                                </div>
                            ) : (
                                <div>
                                    <R.Button
                                        primary={true}
                                        classify={"theme"}
                                        text={RMResx.RM_JS_Common_Next}
                                        onClick={onNext}
                                    />
                                </div>
                            )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AnalysisConfigurationEditPage;

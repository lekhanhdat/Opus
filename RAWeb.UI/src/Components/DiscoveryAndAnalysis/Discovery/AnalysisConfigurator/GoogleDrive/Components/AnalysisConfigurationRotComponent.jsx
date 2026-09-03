import { useImperativeHandle, useState, forwardRef } from "react";
import _ from "lodash";

import { AnalyseMethodConstants } from "../../../../RuleManagement/Constants";
import { DiscoveryQueryDataType } from "../../../../Analysis/Constants";
import RuleCategoryInfoesManager from "../../Components/RuleCategoryInfoesManager";

const defaultValidate = {
    isValidated: true,
    errorMessages: [],
};

const AnalysisConfigurationRotComponent = ({ info, hasJob, onChange }, ref) => {
    const [validateInfo, setValidateInfo] = useState(defaultValidate);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_ErrorMsg],
            };
            if (info.enable) {
                if (info.redundantRules.every(i => !i.isEnable) && info.obsoleteRules.every(i => !i.isEnable) && info.trivialRules.every(i => !i.isEnable)) {
                    setValidateInfo(validateRes);
                    return validateRes.isValidated;
                }
                return true;
            } else {
                return true;
            }
        },
        onShowDialog: (jobInfos) => {
            if (jobInfos.hasLatestJob) {
                setIsShowDialog(false);
            } else {
                setIsShowDialog(true);
            }
        }
    }));

    const onSwitchChange = (checked) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.enable = checked;
        onChange(clonedInfo);

        if (!checked) {
            setValidateInfo(defaultValidate);
        }
    };

    const onActiveTabChange = (index) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.activeTab = index;
        onChange(clonedInfo);
    };

    const onRuleCategoryInfoesChange = (field, value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo[field] = value;
        onChange(clonedInfo);
        setValidateInfo(defaultValidate);
    };

    return (
        <div className="reco-analysis-configurator-rot-definition">
            <section className="reco-ac-component-title-main flex align-center gap-xs">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_ROT}</span>
            </section>
            <section className="reco-ac-id-switch">
                <R.Switch id="raRotSwitch" checked={!hasJob || info.enable} onChange={onSwitchChange} />
                <div
                    className="reco-ac-component-title-secondary"
                    style={{ marginBottom: 0 }}
                    tabIndex="0"
                >
                    {RMResx.RM_FA_Discovery_ROTConfig_Switch}
                </div>
            </section>
            {!validateInfo.isValidated && (
                <div className="reco-error-messages margin-bottom-s">
                    {validateInfo.errorMessages.map(
                        (item, index) => (
                            <div
                                className="reco-error-message"
                                key={index}
                                tabIndex="0"
                            >
                                {item}
                            </div>
                        )
                    )}
                </div>
            )}
            <div style={{ display: info.enable ? "block" : "none" }}>
                <R.Tabcontrol
                    flex
                    onChange={(index) => onActiveTabChange(index)}
                    active={info.activeTab}
                >
                    <R.TabPanel tab={RMResx.RM_FA_Discovery_ROTConfig_RedundantTab} aria-label={RMResx.RM_FA_Discovery_ROTConfig_RedundantTab}>
                        <RuleCategoryInfoesManager
                            supportAnalyseMethods={[
                                AnalyseMethodConstants.type.GoogleDocument,
                            ]}
                            ruleCategoryInfoes={
                                info.redundantRules
                            }
                            onChange={(value) =>
                                onRuleCategoryInfoesChange(
                                    "redundantRules",
                                    value
                                )
                            }
                            dataType={DiscoveryQueryDataType.Rot}
                        />
                    </R.TabPanel>
                    <R.TabPanel tab={RMResx.RM_FA_Discovery_ROTConfig_ObsoleteTab} aria-label={RMResx.RM_FA_Discovery_ROTConfig_ObsoleteTab}>
                        <RuleCategoryInfoesManager
                            supportAnalyseMethods={[
                                AnalyseMethodConstants.type.GoogleDocument,
                            ]}
                            ruleCategoryInfoes={
                                info.obsoleteRules
                            }
                            onChange={(value) =>
                                onRuleCategoryInfoesChange(
                                    "obsoleteRules",
                                    value
                                )
                            }
                            dataType={DiscoveryQueryDataType.Rot}
                        />
                    </R.TabPanel>
                    <R.TabPanel tab={RMResx.RM_FA_Discovery_ROTConfig_TrivialTab} aria-label={RMResx.RM_FA_Discovery_ROTConfig_TrivialTab}>
                        <RuleCategoryInfoesManager
                            supportAnalyseMethods={[
                                AnalyseMethodConstants.type.GoogleDocument,
                            ]}
                            ruleCategoryInfoes={
                                info.trivialRules
                            }
                            onChange={(value) =>
                                onRuleCategoryInfoesChange(
                                    "trivialRules",
                                    value
                                )
                            }
                            dataType={DiscoveryQueryDataType.Rot}
                        />
                    </R.TabPanel>
                </R.Tabcontrol>
            </div>
        </div>
    );
};

export default forwardRef(AnalysisConfigurationRotComponent);

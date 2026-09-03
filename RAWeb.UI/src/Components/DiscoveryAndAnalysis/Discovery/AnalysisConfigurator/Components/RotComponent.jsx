import { useImperativeHandle, useState, forwardRef, useEffect } from "react";
import _ from "lodash";
import RuleCategoryInfoesManager from "./RuleCategoryInfoesManager";
import DialogInfo from "../QuestionMarkDialog/Dialog";
import { RotMsg } from "../Constants/DialogMsg";
import { AnalyseMethodConstants } from "../../../RuleManagement/Constants";
import { DiscoveryQueryDataType } from "../../../Analysis/Constants";

const defaultValidate = {
    isValidated: true,
    errorMessages: [],
};

const RotComponent = ({ info, hasJob, onChange }, ref) => {
    const [isShowDialog, setIsShowDialog] = useState(false);

    const [validateInfo, setValidateInfo] = useState(defaultValidate);
    const [sourceRotDefinition, setSourceRotDefinition] = useState(info.rotDefinition);

    useEffect(() => {
        if (info) {
            // Only Google Drive and Office365 have ROT
            setSourceRotDefinition(info.rotDefinition);
        }
    }, [info])

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_ErrorMsg],
            };
            if (sourceRotDefinition.enable) {
                if (sourceRotDefinition.redundantRules.every(i => !i.isEnable) && sourceRotDefinition.obsoleteRules.every(i => !i.isEnable) && sourceRotDefinition.trivialRules.every(i => !i.isEnable)) {
                    setValidateInfo(validateRes);
                    return validateRes.isValidated;
                }
                return true;
            } else {
                return true;
            }
        },
        onShowDialog: (jobInfos) => {
            if (jobInfos?.hasLatestJob) {
                setIsShowDialog(false);
            } else {
                setIsShowDialog(true);
            }
        }
    }));

    const onSwitchChange = (checked) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.rotDefinition.enable = checked;
        onChange(clonedInfo);

        if (!checked) {
            setValidateInfo(defaultValidate);
        }
    };

    const onActiveTabChange = (index) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.rotDefinition.activeTab = index;
        onChange(clonedInfo);
    };

    const onRuleCategoryInfoesChange = (field, value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.rotDefinition[field] = value; // E.g: clonedInfo.rotDefinition.redundantRules
        onChange(clonedInfo);
        setValidateInfo(defaultValidate);
    };

    const onQuestionMarkClick = () => {
        setIsShowDialog(true);
    };

    const onClose = () => {
        setIsShowDialog(false);
    };

    return (
        <div className="reco-analysis-configurator-rot-definition">
            <section className="reco-ac-component-title-main flex align-center gap-xs">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_ROT}</span>
                <R.Button
                    id="raQuestion"
                    classify={"blank"}
                    type={"icon"}
                    icon={"fia-question"}
                    onClick={onQuestionMarkClick}
                />
                <DialogInfo
                    isShow={isShowDialog}
                    onCloseDialog={onClose}
                    messages={RotMsg}
                />
            </section>
            <section className="reco-ac-id-switch">
                <R.Switch id="raRotSwitch" checked={!hasJob || sourceRotDefinition.enable} onChange={onSwitchChange} />
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
            <div style={{ display: sourceRotDefinition.enable ? "block" : "none" }}>
                <R.Tabcontrol
                    flex
                    onChange={(index) => onActiveTabChange(index)}
                    active={sourceRotDefinition.activeTab}
                >
                    <R.TabPanel tab={RMResx.RM_FA_Discovery_ROTConfig_RedundantTab} aria-label={RMResx.RM_FA_Discovery_ROTConfig_RedundantTab}>
                        <RuleCategoryInfoesManager
                            supportAnalyseMethods={[
                                AnalyseMethodConstants.type.Document,
                                AnalyseMethodConstants.type.Version,
                                AnalyseMethodConstants.type.DuplicateDocument,
                            ]}
                            ruleCategoryInfoes={
                                sourceRotDefinition.redundantRules
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
                                AnalyseMethodConstants.type.Document,
                                AnalyseMethodConstants.type.Version,
                                AnalyseMethodConstants.type.DuplicateDocument,
                            ]}
                            ruleCategoryInfoes={
                                sourceRotDefinition.obsoleteRules
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
                                AnalyseMethodConstants.type.Document,
                                AnalyseMethodConstants.type.Version,
                                AnalyseMethodConstants.type.DuplicateDocument,
                            ]}
                            ruleCategoryInfoes={
                                sourceRotDefinition.trivialRules
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

export default forwardRef(RotComponent);

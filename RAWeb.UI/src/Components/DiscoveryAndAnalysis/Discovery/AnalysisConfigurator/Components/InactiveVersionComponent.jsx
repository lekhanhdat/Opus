import { useImperativeHandle, useState, forwardRef, useEffect } from "react";
import _ from "lodash";
import RuleCategoryInfoesManager from "./RuleCategoryInfoesManager";
import DialogInfo from "../QuestionMarkDialog/Dialog";
import { InactiveMsg } from "../Constants/DialogMsg";
import { AnalyseMethodConstants } from "../../../RuleManagement/Constants";
import { DiscoveryQueryDataType } from "../../../Analysis/Constants";

const defaultValidate = {
    isValidated: true,
    errorMessages: [],
};

const InactiveVersionComponent = ({ info, hasJob, isSalesForce, onChange }, ref) => {
    const [isShowDialog, setIsShowDialog] = useState(false);

    const [validateInfo, setValidateInfo] = useState(defaultValidate);
    const [sourceInactiveDefinition, setSourceInactiveDefinition] = useState(info.inactiveDefinition);

    useEffect(() => {
        if (info) {
            setSourceInactiveDefinition(info.inactiveDefinition);
        }
    }, [info])

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            if (isSalesForce){
                return true;
            }
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_ErrorMsg],
            };
            if (sourceInactiveDefinition.enable && sourceInactiveDefinition.rules.every(i => !i.isEnable)) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }

            const validateRes1 = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_RuleGreaterThan50],
            };
            if (sourceInactiveDefinition.enable && sourceInactiveDefinition.rules.filter(i => i.isEnable).length > 50) {
                setValidateInfo(validateRes1);
                return validateRes.isValidated;
            }

            return true;
        },
        onShowDialog: (jobInfos) => {
            if (jobInfos?.hasLatestJob || isSalesForce) {
                setIsShowDialog(false);
            } else {
                setIsShowDialog(true);
            }
        }
    }));

    const onSwitchChange = (checked) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.inactiveDefinition.enable = checked;
        onChange(clonedInfo);

        if (!checked) {
            setValidateInfo(defaultValidate);
        }
    };

    const onRuleCategoryInfoesChange = (value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.inactiveDefinition.rules = value;
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
        <div className="reco-analysis-configurator-inactive-definition">
            <section className="reco-ac-component-title-main flex align-center gap-xs">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_Inactive}</span>
                {
                    !isSalesForce &&(
                        <R.Button
                        id="raQuestion"
                        classify={"blank"}
                        type={"icon"}
                        icon={'fia-question'}
                        onClick={onQuestionMarkClick}
                        />
                    )
                }
             
                <DialogInfo
                    isShow={isShowDialog}
                    onCloseDialog={onClose}
                    messages={InactiveMsg}
                />
            </section>
            {isSalesForce ? (
                <section className="flex flex-column gap-xs">
                    <strong className="reco-ac-component-title-salesforce" tabIndex={0}>{RMResx.RM_FA_Discovery_InactiveConfig_SF_Record}</strong>
                    <span tabIndex={0}>{RMResx.RM_FA_Discovery_InactiveConfig_SF_Record_Desc}</span>
                </section>
            ) : (
                <>
                    <section className="reco-ac-id-switch">
                        <R.Switch id="raInactiveSwitch" checked={!hasJob || sourceInactiveDefinition.enable} onChange={onSwitchChange} />
                        <div
                            className="reco-ac-component-title-secondary"
                            style={{ marginBottom: 0 }}
                            tabIndex="0"
                        >
                            {RMResx.RM_FA_Discovery_InactiveConfig_Switch}
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
                    <div style={{ display: sourceInactiveDefinition.enable ? "block" : "none" }}>
                        <RuleCategoryInfoesManager
                            supportAnalyseMethods={[AnalyseMethodConstants.type.Version]}
                            ruleCategoryInfoes={sourceInactiveDefinition.rules}
                            onChange={onRuleCategoryInfoesChange}
                            dataType={DiscoveryQueryDataType.Inactive}
                        />
                    </div>
                </>
            )}
        </div>
    );
};

export default forwardRef(InactiveVersionComponent);

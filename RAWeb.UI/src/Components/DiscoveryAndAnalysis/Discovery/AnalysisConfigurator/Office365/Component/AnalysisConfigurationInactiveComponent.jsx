import { useImperativeHandle, useState, forwardRef } from "react";
import _ from "lodash";
import DialogInfo from "../../QuestionMarkDialog/Dialog";
import { InactiveMsg } from "../../Constants/DialogMsg";
import { AnalyseMethodConstants } from "../../../../RuleManagement/Constants";
import { DiscoveryQueryDataType } from "../../../../Analysis/Constants";
import RuleCategoryInfoesManager from "../../Components/RuleCategoryInfoesManager";

const defaultValidate = {
    isValidated: true,
    errorMessages: [],
};

const AnalysisConfigurationInactiveComponent = ({ info, hasJob, onChange }, ref) => {

    const [isShowDialog, setIsShowDialog] = useState(false);

    const [validateInfo, setValidateInfo] = useState(defaultValidate);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_ErrorMsg],
            };
            if (info.enable && info.rules.every(i => !i.isEnable)) {
                setValidateInfo(validateRes);
                return validateRes.isValidated;
            }

            const validateRes1 = {
                isValidated: false,
                errorMessages: [RMResx.RM_FA_Discovery_InactiveConfig_RuleGreaterThan50],
            };
            if (info.enable && info.rules.filter(i => i.isEnable).length > 50) {
                setValidateInfo(validateRes1);
                return validateRes.isValidated;
            }

            return true;
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

    const onRuleCategoryInfoesChange = (value) => {
        const clonedInfo = _.cloneDeep(info);
        clonedInfo.rules = value;
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
            <section className="reco-ac-component-title-main">
                <span tabIndex="0">{RMResx.RM_FA_Discovery_Config_Inactive}</span>
                <R.Button
                    id="raQuestion"
                    classify={"blank"}
                    type={"icon"}
                    icon={'fia-question'}
                    onClick={onQuestionMarkClick}
                />
                <DialogInfo
                    isShow={isShowDialog}
                    onCloseDialog={onClose}
                    messages={InactiveMsg}
                />
            </section>
            <section className="reco-ac-id-switch">
                <R.Switch id="raInactiveSwitch" checked={!hasJob || info.enable} onChange={onSwitchChange} />
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
            <div style={{ display: info.enable ? "block" : "none" }}>
                <RuleCategoryInfoesManager
                    supportAnalyseMethods={[AnalyseMethodConstants.type.Version]}
                    ruleCategoryInfoes={info.rules}
                    onChange={onRuleCategoryInfoesChange}
                    dataType={DiscoveryQueryDataType.Inactive}
                />
            </div>
        </div>
    );
};

export default forwardRef(AnalysisConfigurationInactiveComponent);

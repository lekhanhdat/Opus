import _ from "lodash";
import { forwardRef, useRef } from "react";
import { ArchiveDataType, MS365DataType } from "../../../../Constants/DataOptimizeType";
import InactiveVersionRule from "./InactiveVersionRule";
import ROTRule from "./ROTRule";

const ObjectRuleExpander = ({ dataOptimizeParameter, onChange, o365TenantId }) => {

    const refRuleSwitchValid = useRef(null);

    const onArchiveRadioChanged = (value) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.archiveDataType = value;
        if (value === ArchiveDataType.Special) {
            clonedParameter.rotRuleQueryParameter.enable = true;
        }
        onChange(clonedParameter);
    };

    const onObjectRuleChanged = (value) => {
        onChange(value);
        $$.verify(refRuleSwitchValid.current.ref.current);
    };

    const ruleSwitchValid = () => {
        return !dataOptimizeParameter.inactiveRuleQueryParameter.enable && !dataOptimizeParameter.rotRuleQueryParameter.enable ? RMResx.RM_FA_DataOptimize_Validation_RuleSwitch : true;
    };

    const ObjectRuleExpanderView = () => {
        return (
            <div className="margin-bottom-l">
                <R.Expander title={RMResx.RM_FA_DataOptimize_ObjectRuleExpander} level={2} status={{ show: true }} togglable={false}>
                    <div>
                        <div className="reco-optimize-title require">{RMResx.RM_FA_DataOptimize_ArchiveTitle}</div>
                        <div role="radiogroup" aria-label={RMResx.RM_FA_DataOptimize_ArchiveTitle}>
                            <div>
                                <R.Radio
                                    name="raArchiveRadio"
                                    text={RMResx.RM_FA_DataOptimize_Archive_All}
                                    value={ArchiveDataType.All}
                                    checked={dataOptimizeParameter.archiveDataType === ArchiveDataType.All}
                                    onChange={onArchiveRadioChanged}
                                />
                            </div>
                            <div>
                                <R.Radio
                                    name="raArchiveRadio"
                                    text={RMResx.RM_FA_DataOptimize_Archive_Special}
                                    value={ArchiveDataType.Special}
                                    checked={dataOptimizeParameter.archiveDataType === ArchiveDataType.Special}
                                    onChange={onArchiveRadioChanged}
                                />
                                <$g.Popover>{RMResx.RM_FA_DataOptimize_Archive_SpecialDes}</$g.Popover>
                            </div>
                            {dataOptimizeParameter.archiveDataType === ArchiveDataType.Special && <div>
                                <InactiveVersionRule
                                    dataOptimizeParameter={dataOptimizeParameter}
                                    onChange={onObjectRuleChanged}
                                />
                                <ROTRule
                                    dataOptimizeParameter={dataOptimizeParameter}
                                    o365TenantId={o365TenantId}
                                    onChange={onObjectRuleChanged}
                                />
                                <div className="margin-top-s">
                                    <R.ValidationFaker valid={ruleSwitchValid} ref={refRuleSwitchValid} />
                                </div>
                            </div>}
                        </div>
                    </div>
                </R.Expander>
            </div>);
    }

    return (dataOptimizeParameter.ms365DataType !== MS365DataType.Phl && ObjectRuleExpanderView());
};

export default forwardRef(ObjectRuleExpander);
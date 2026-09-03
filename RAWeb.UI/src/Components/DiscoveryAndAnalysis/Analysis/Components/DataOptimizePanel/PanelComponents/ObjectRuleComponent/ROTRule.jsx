import _ from "lodash";
import ROTCategories from "../../../../ROT/Components/Category";

const ROTRule = ({ dataOptimizeParameter, onChange, o365TenantId }) => {

    const onSwitchChange = (checked) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.rotRuleQueryParameter.enable = checked;
        onChange(clonedParameter);
    };

    return (
        <div className="margin-top-m">
            <div className="reco-optimize-switch">
                <R.Switch id="raROTRuleSwitch" checked={dataOptimizeParameter.rotRuleQueryParameter.enable} onChange={onSwitchChange} />
                <div className="reco-optimize-switch-text" tabIndex="0">{RMResx.RM_FA_DataOptimize_Archive_ROTRuleSwitch}</div>
            </div>
            {dataOptimizeParameter.rotRuleQueryParameter.enable &&
                <ROTCategories
                    queryParameter={dataOptimizeParameter}
                    onChange={onChange}
                    o365TenantId={o365TenantId}
                    isOptimizePanel={true}
                />}
        </div>
    );
};

export default ROTRule;
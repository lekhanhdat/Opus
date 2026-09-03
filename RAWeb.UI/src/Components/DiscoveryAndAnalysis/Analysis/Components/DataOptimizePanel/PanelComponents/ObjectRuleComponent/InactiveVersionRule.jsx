import _ from "lodash";
import { useState, useEffect } from "react";
import Category from "../../../Category";
import { BasicDataRequester } from "../../../../requests";

const InactiveVersionRule = ({ dataOptimizeParameter, onChange, o365TenantId }) => {

    const [inactiveVersionItems, setInactiveVersionItems] = useState([]);

    useEffect(() => {
        const handler = async () => {
            let convertItems = [];
            const ruleColumns = await BasicDataRequester.getInactiveTableColumns();
            if (!_.isNil(ruleColumns)) {
                ruleColumns.map(item => {
                    convertItems.push({
                        id: item.id,
                        name: item.displayName,
                        checked: true
                    });
                });
            }
            setInactiveVersionItems(convertItems);
        };
        handler();
    }, []);

    const onSwitchChange = (checked) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.inactiveRuleQueryParameter.enable = checked;
        onChange(clonedParameter);
    };

    const onSelectedVersionRuleInfo = (ids) => {
        const clonedValue = _.cloneDeep(dataOptimizeParameter);
        clonedValue.inactiveRuleQueryParameter.ruleIds = ids.length === inactiveVersionItems.length ? [] : ids;
        onChange(clonedValue);
    };

    return (
        <div className="margin-top-m">
            <div className="reco-optimize-switch">
                <R.Switch id="raInactiveVersionSwitch" checked={dataOptimizeParameter.inactiveRuleQueryParameter.enable} onChange={onSwitchChange} />
                <div className="reco-optimize-switch-text" tabIndex="0">{RMResx.RM_FA_DataOptimize_Archive_InactiveVersionSwitch}</div>
            </div>
            {dataOptimizeParameter.inactiveRuleQueryParameter.enable && <Category
                categoryItems={inactiveVersionItems}
                onChange={onSelectedVersionRuleInfo}
            />}
        </div>
    );
};

export default InactiveVersionRule;
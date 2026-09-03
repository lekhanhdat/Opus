import { useEffect, useRef, useState } from "react";
import { BasicDataRequester } from "../../requests";
import _ from "lodash";
import { AnalyseMethod } from "../../../Discovery/AnalysisConfigurator/Constants/AnalyseMethod";

const GroupedRule = ({ queryParameter, onChange, ariaId }) => {

    const availableRuleInfoes = useRef([]);

    const [ruleInfoOptions, setRuleInfoOptions] = useState([]);

    useEffect(() => {
        const handler = async () => {
            const items = await BasicDataRequester.getRotRuleInfoes();
            availableRuleInfoes.current = items;
        };
        handler();
    }, []);

    useEffect(() => {
        const ruleInfoes = availableRuleInfoes.current;
        const selectedRuleIds = queryParameter.rotRuleQueryParameter.ruleIds;
        
        const res = ruleInfoes.map(item => ({
            id: item.id,
            name: item.name,
            category: item.category,
            categoryDisplayName: item.categoryDisplayName,
            checked: selectedRuleIds.some(i => i === item.id),
            group: item.categoryDisplayName,
            analyseMethod: item.analyseMethod,
        }));
        setRuleInfoOptions(res);
    }, [queryParameter]);

    const onInnerChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        const checkedVersionRules = args.newValue.filter(item => item.checked && item.analyseMethod === AnalyseMethod.Version);
        if(checkedVersionRules.length > 1) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content:
                    RMResx.RM_DA_Profile_VersionRuleConflict.format(checkedVersionRules.map(item => item.name).join(", ")),
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => onChange(clonedValue),
                    },
                ],
            });
        }
        else {
            clonedValue.rotRuleQueryParameter = {
                ruleIds: args.newValue.map(item => item.id)
            };
            onChange(clonedValue)
        }        
    };

    const onWillChange = (args) => {
        console.log(args);
        const checkedVersionRules = args.newValue.filter(item => item.checked && item.analyseMethod === AnalyseMethod.Version);
        if(checkedVersionRules.length > 1) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content:
                    RMResx.RM_DA_Profile_VersionRuleConflict.format(checkedVersionRules.map(item => item.name).join(", ")),
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => true,
                    },
                ],
            });
            return false;
        }

        return true;
    };

    return (
        <div className="reco-size-range">
            <div className="reco-fr-content">
                <div className="reco-fr-content-style">
                    <div>
                        <R.Validation element="Multicombobox" require>
                            <R.Multicombobox
                                id="raFileTypeMultiCombobox"
                                width="100%"
                                popupMaxHeight={400}
                                searchable={false}
                                items={ruleInfoOptions}
                                textField="name"
                                valueField="id"
                                tooltipField="name"
                                groupField="group"
                                checkedField="checked"
                                onChange={onInnerChange}
                                willChange={onWillChange}
                                hasSelectAll={false}
                                aria={{ ariaLabel: ariaId }}
                            />
                        </R.Validation>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default GroupedRule;
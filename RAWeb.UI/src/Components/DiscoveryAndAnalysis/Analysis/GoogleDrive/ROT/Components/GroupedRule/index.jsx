import { useEffect, useRef, useState } from "react";
import _ from "lodash";
import { GoogleDriveBasicDataRequester } from "../../../../requests/GoogleDrive";

const AnalysisMethod = {
    None: 0,
    Document: 1,
    Version: 2,
    DuplicateDocument: 3,
}

const GroupedRule = ({ queryParameter, onChange, ariaId }) => {
    const availableRuleInfo = useRef([]);

    const [ruleInfoOptions, setRuleInfoOptions] = useState([]);

    useEffect(() => {
        (async () => {
            const items = await GoogleDriveBasicDataRequester.getRotRuleInfoList();
            availableRuleInfo.current = items;
        })();
    }, []);

    useEffect(() => {
        const ruleInfo = availableRuleInfo.current;
        const selectedRuleIds = queryParameter.rotRuleQueryParameter.ruleIds;
        
        const ruleInfoOptions = ruleInfo.map(item => ({
            id: item.id,
            name: item.name,
            category: item.category,
            categoryDisplayName: item.categoryDisplayName,
            checked: selectedRuleIds.some(id => id === item.id),
            group: item.categoryDisplayName,
            analysisMethod: item.analyseMethod,
        }));
        setRuleInfoOptions(ruleInfoOptions);
    }, [queryParameter]);

    const onInnerChange = (args) => {
        const clonedValue = _.cloneDeep(queryParameter);
        const checkedVersionRules = args.newValue.filter(item => item.checked && item.analysisMethod === AnalysisMethod.Version);
        if(checkedVersionRules.length > 1) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_DA_Profile_VersionRuleConflict.format(checkedVersionRules.map(item => item.name).join(", ")),
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
            const ruleIds = args.newValue.map(item => item.id);
            clonedValue.rotRuleQueryParameter = {ruleIds};
            onChange(clonedValue)
        }        
    };

    const onWillChange = (args) => {
        const checkedVersionRules = args.newValue.filter(item => item.checked && item.analysisMethod === AnalysisMethod.Version);
        if(checkedVersionRules.length > 1) {
            $$.messagedialog(true, {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_DA_Profile_VersionRuleConflict.format(checkedVersionRules.map(item => item.name).join(", ")),
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
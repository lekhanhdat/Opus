import { useState, useImperativeHandle, useRef, forwardRef } from "react";
import "./index.less";
import _ from "lodash";
import RuleCriteriaManager from "./RuleCriteriaManager";
import { useStableCallback } from "../../Common/Hooks";
import {
    AnalyseMethodConstants,
    AnalyseMethodCriterias,
    LogicConstants,
} from "./Constants";
import { CriteriaConditionAnalyzer } from "./util";
import StringUtil from "../../../Utilities/StringUtil";

const DefaultValidateInfo = {
    name: {
        isValidated: true,
    },
    description: {
        isValidated: true,
    },
    method: {
        isValidated: true,
    },
    criteriaInfoes: {
        isValidated: true,
    },
};

const DefaultAnalyseMethodOptions = [
    {
        name: AnalyseMethodConstants.i18n.get(
            AnalyseMethodConstants.type.Document
        ),
        value: AnalyseMethodConstants.type.Document,
    },
    {
        name: AnalyseMethodConstants.i18n.get(
            AnalyseMethodConstants.type.Version
        ),
        value: AnalyseMethodConstants.type.Version,
    },
    {
        name: AnalyseMethodConstants.i18n.get(
            AnalyseMethodConstants.type.DuplicateDocument
        ),
        value: AnalyseMethodConstants.type.DuplicateDocument,
    },
];

const ruleNameMaxLength = 255;
const ruleDescritionMaxLength = 255;

const RuleCategoryPanel = ({ supportAnaylseMethods, onChange, ruleCategoryInfoes }, ref) => {
    const ruleManagerRef = useRef(null);

    const [title, setTitle] = useState("");

    const [showPanel, setShowPanel] = useState(false);

    const [ruleCategoryInfo, setRuleCategoryInfo] = useState({
        name: "",
        description: "",
        analyseMethod: supportAnaylseMethods[0],
        criteriaInfoes: [],
    });

    const [validateInfo, setValidateInfo] = useState(DefaultValidateInfo);

    const [analyseMethodOptions, setAnalyseMethodOptions] = useState([]);

    useImperativeHandle(ref, () => ({
        onShow: (title, ruleCategoryInfo) => {
            let clonedRuleCategoryInfo = _.cloneDeep(ruleCategoryInfo);
            if (_.isNil(clonedRuleCategoryInfo)) {
                const criterias = AnalyseMethodCriterias.get(
                    supportAnaylseMethods[0]
                );
                const defaultCriteria = criterias[0];
                const defaultCondition = defaultCriteria.conditions[0];
                const criteriaComponentInfo = CriteriaConditionAnalyzer.getCriteriaComponentInfo(criterias, criterias[0].value);
                clonedRuleCategoryInfo = {
                    preId: StringUtil.newGuid(),
                    name: "",
                    description: "",
                    analyseMethod: supportAnaylseMethods[0],
                    criteriaInfoes: [
                        {
                            order: 1,
                            criteriaType: defaultCriteria.value,
                            conditionInfo: {
                                category: defaultCondition.category,
                                logic: defaultCondition.type,
                                value: criteriaComponentInfo.defaultValue,
                                extraValue: criteriaComponentInfo.extraDefaultValue,
                            },
                            logic: LogicConstants.type.None,
                        },
                    ],
                };
            }
            setTitle(title);
            setValidateInfo(DefaultValidateInfo);
            setRuleCategoryInfo(clonedRuleCategoryInfo);
            setAnalyseMethodOptions(getAnalyseMethodOptions(clonedRuleCategoryInfo.analyseMethod));
            setShowPanel(true);
        },
    }));

    const onSave = useStableCallback(() => {
        const validateInfo = {};
        let isValidated = true;
        const nameValidateRes = onValidateName(ruleCategoryInfo);
        validateInfo.name = nameValidateRes;
        isValidated = isValidated && nameValidateRes.isValidated;

        const descriptionValidateRes = onValidateDescription(
            ruleCategoryInfo.description
        );
        validateInfo.description = descriptionValidateRes;
        isValidated = isValidated && descriptionValidateRes.isValidated;

        const methodValidateRes = onValidateMethod();
        validateInfo.method = methodValidateRes;
        isValidated = isValidated && methodValidateRes.isValidated;

        const ruleManagerValidateRes = ruleManagerRef.current.onValidate();
        isValidated = isValidated && ruleManagerValidateRes;
        validateInfo.criteriaInfoes = { isValidated: isValidated };
        if (!isValidated) {
            setValidateInfo(validateInfo);
            return false;
        }

        onChange(ruleCategoryInfo);
        setShowPanel(false);
    });

    const onInnerChange = (field, value) => {
        const clonedValidateInfo = _.clone(validateInfo);
        if (!clonedValidateInfo[field].isValidate) {
            clonedValidateInfo[field].isValidated = true;
            setValidateInfo(clonedValidateInfo);
        }

        const clonedRuleCategoryInfo = Object.assign({}, ruleCategoryInfo);
        clonedRuleCategoryInfo[field] = value;
        setRuleCategoryInfo(clonedRuleCategoryInfo);
    };

    const onAnalyseMethodChange = (value) => {
        const clonedRuleCategoryInfo = _.cloneDeep(ruleCategoryInfo);
        const criterias = AnalyseMethodCriterias.get(value);
        clonedRuleCategoryInfo.analyseMethod = value;
        const criteriaComponentInfo = CriteriaConditionAnalyzer.getCriteriaComponentInfo(criterias, criterias[0].value);
        clonedRuleCategoryInfo.criteriaInfoes = [
            {
                order: 1,
                criteriaType: criteriaComponentInfo.criteriaType,
                logic: LogicConstants.type.None,
                conditionInfo: {
                    category: criteriaComponentInfo.conditionInfo.category,
                    logic: criteriaComponentInfo.conditionInfo.type,
                    value: criteriaComponentInfo.defaultValue,
                    extraValue: criteriaComponentInfo.extraDefaultValue,
                },
            }
        ];
        setAnalyseMethodOptions(getAnalyseMethodOptions(value));
        setRuleCategoryInfo(clonedRuleCategoryInfo);
    };

    const onValidateName = (infos) => {
        if (_.isNil(infos.name) || infos.name === "" || infos.name.trim() === "") {
            return {
                isValidated: false,
                errorMessages: [RMResx["Gui.Common_5a85c7e7-8cf1-4ff0-a15b-21ddb92088e2"]],
            };
        }

        if(infos.name.length > ruleNameMaxLength){
            return {
                isValidated: false,
                errorMessages: [RMResx.RM_JS_RDM_CreateRule_Validation_RuleNameTooLong.format(ruleNameMaxLength)],
            };    
        }

        if (infos.id) {
            if (ruleCategoryInfoes.length > 0 && ruleCategoryInfoes.some(i => i.id != infos.id && i.name === infos.name)) {
                return {
                    isValidated: false,
                    errorMessages: [RMResx.RM_FA_Discovery_Validation_NameExist],
                };
            }
        } else {
            if (ruleCategoryInfoes.length > 0 && ruleCategoryInfoes.some(i => i.preId != infos.preId && i.name === infos.name)) {
                return {
                    isValidated: false,
                    errorMessages: [RMResx.RM_FA_Discovery_Validation_NameExist],
                };
            }
        }

        return {
            isValidated: true,
        };
    };

    const onValidateDescription = (description) => {

        if(!_.isNil(description) && description.length > ruleDescritionMaxLength){
            return {
                isValidated: false,
                errorMessages: [RMResx.RM_JS_RDM_CreateRule_Validation_RuleDesTooLong.format(ruleDescritionMaxLength)],
            };   
        }

        return {
            isValidated: true,
        };
    };

    const onValidateMethod = () => {
        return {
            isValidated: true,
        };
    };

    const getAnalyseMethodOptions = (selectedAnalyseMethod) => {
        return _.cloneDeep(DefaultAnalyseMethodOptions)
            .filter((item) =>
                supportAnaylseMethods.some((method) => item.value === method)
            )
            .map((item) => {
                item.checked = item.value === selectedAnalyseMethod;
                return item;
            });
    };

    return (
        <R.Panel
            id="reco-rule-category-panel"
            header={title}
            size={660}
            status={{ show: showPanel }}
            onHide={() => setShowPanel(false)}
            destroy={true}
        >
            <div>
                <div className="reco-rule-category-container">
                    <div className="reco-rule-category-input-item">
                        <div className="reco-input-label require">
                            {RMResx.RM_FA_Discovery_RulePanel_Name}
                        </div>
                        <div>
                            <R.Input
                                id="raNameIpt"
                                name={RMResx.RM_FA_Discovery_RulePanel_Name}
                                type={"text"}
                                width={"100%"}
                                height={34}
                                value={ruleCategoryInfo.name}
                                onChange={(value) =>
                                    onInnerChange("name", value)
                                }
                                aria={{ ariaLabel: RMResx.RM_FA_Discovery_RulePanel_Name }}
                            />
                        </div>
                        {!validateInfo.name.isValidated && (
                            <div className="reco-error-messages">
                                {validateInfo.name.errorMessages.map(
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
                    </div>

                    <div className="reco-rule-category-input-item">
                        <div className="reco-rule-category-input-item">
                            <div className="reco-input-label">
                                {RMResx.RM_JS_RDM_Rule_Description}
                            </div>
                            <div>
                                <R.Input
                                    id="raDesIpt"
                                    name={RMResx.RM_JS_RDM_Rule_Description}
                                    type={"textarea"}
                                    width={"100%"}
                                    height={100}
                                    value={ruleCategoryInfo.description}
                                    onChange={(value) =>
                                        onInnerChange("description", value)
                                    }
                                    aria={{ ariaLabel: RMResx.RM_JS_RDM_Rule_Description }}
                                />
                            </div>
                            {!validateInfo.description.isValidated && (
                                <div className="reco-error-messages">
                                    {validateInfo.description.errorMessages.map(
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
                        </div>
                    </div>
                    {supportAnaylseMethods.length > 1 && (
                        <div className="reco-rule-category-input-item">
                            <div className="reco-input-label require">
                                {RMResx.RM_FA_Discovery_RulePanel_Method}
                            </div>
                            <div>
                                <R.Combobox
                                    id="raMethodCom"
                                    width="100%"
                                    searchable={false}
                                    textField="name"
                                    valueField="value"
                                    checkedField="checked"
                                    items={analyseMethodOptions}
                                    onChange={(value) =>
                                        onAnalyseMethodChange(
                                            value.newValue.value
                                        )
                                    }
                                    aria={{ ariaLabel: RMResx.RM_FA_Discovery_RulePanel_Method }}
                                />
                            </div>
                            <div>
                                <$g.TopMessageBar 
                                    show={ruleCategoryInfo.analyseMethod == AnalyseMethodConstants.type.DuplicateDocument}
                                    type="warning"
                                    className="tm-inheritMsg"
                                >
                                    {RMResx.RM_FA_Discovery_Method_DuplicateDoc_WarningMsg}
                                </$g.TopMessageBar>
                            </div>
                        </div>
                    )}

                    <div className="reco-rule-category-input-item">
                        <div className="reco-input-label require">
                            {RMResx.RM_FA_Discovery_RulePanel_Criteria}
                        </div>
                        <div className="reco-rule-category-criterias">
                            <RuleCriteriaManager
                                analyseMethod={ruleCategoryInfo.analyseMethod}
                                criteriaInfoes={ruleCategoryInfo.criteriaInfoes}
                                onChange={(value) =>
                                    onInnerChange("criteriaInfoes", value)
                                }
                                ref={ruleManagerRef}
                            />
                        </div>
                    </div>
                </div>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={() => setShowPanel(false)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
            </>
        </R.Panel>
    );
};

export default forwardRef(RuleCategoryPanel);

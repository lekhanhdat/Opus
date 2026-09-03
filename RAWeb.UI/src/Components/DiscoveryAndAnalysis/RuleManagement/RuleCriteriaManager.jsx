import React from "react";
import {
    forwardRef,
    useEffect,
    useImperativeHandle,
    useRef,
    useState,
} from "react";
import _ from "lodash";
import RuleCriteriaCondition from "./RuleCriteriaCondition";
import {
    AnalyseMethodConstants,
    AnalyseMethodCriterias,
    LogicConstants,
} from "./Constants";
import "./index.less";
import { CriteriaConditionAnalyzer, LogicBuilder } from "./util";

const RuleCriteriaManager = (
    { analyseMethod, criteriaInfoes, onChange },
    ref
) => {
    const [innerCriteriaInfoes, setInnerCriteriaInfoes] = useState([]);

    const [criteriaLogicText, setCriteriaLogicText] = useState("");

    const componentRefs = useRef([]);

    useImperativeHandle(ref, () => ({
        onValidate: () => {
            const validateRes = componentRefs.current.map((item) =>
                item.current.onValidate()
            );
            return validateRes.every((item) => item);
        },
    }));

    useEffect(() => {
        const refs = [];
        for (let i = 0; i < criteriaInfoes.length; i++) {
            refs.push(React.createRef());
        }
        componentRefs.current = refs;

        setInnerCriteriaInfoes(criteriaInfoes);
        const logicText = LogicBuilder.build(criteriaInfoes);
        setCriteriaLogicText(logicText);
    }, [criteriaInfoes]);

    const onCriteriaChange = (order, info) => {
        const clonedCriteriaInfoes = criteriaInfoes.map((item) => {
            if (item.order === order) {
                return info;
            }
            return item;
        });
        onChange(clonedCriteriaInfoes);
    };

    const onDelete = (order) => {
        let clonedCriteriaInfoes = [...criteriaInfoes];
        clonedCriteriaInfoes.splice(order - 1, 1);
        clonedCriteriaInfoes = clonedCriteriaInfoes.map((item, index) => {
            item.order = index + 1;
            return item;
        });
        const criteriaInfo =
            clonedCriteriaInfoes[clonedCriteriaInfoes.length - 1];
        criteriaInfo.logic = LogicConstants.type.None;
        onChange(clonedCriteriaInfoes);
    };

    const onAdd = (order) => {
        const clonedCriteriaInfoes = [...criteriaInfoes];
        const criteriaInfo =
            clonedCriteriaInfoes[clonedCriteriaInfoes.length - 1];
        criteriaInfo.logic = LogicConstants.type.And;

        const criteriaInfoesOrder = clonedCriteriaInfoes.length + 1;

        const criterias = AnalyseMethodCriterias.get(analyseMethod);

        const criteriaComponentInfo = CriteriaConditionAnalyzer.getCriteriaComponentInfo(criterias, criterias[0].value);
        clonedCriteriaInfoes.push({
            order: criteriaInfoesOrder,
            criteriaType: criteriaComponentInfo.criteriaType,
            logic: LogicConstants.type.None,
            conditionInfo: {
                category: criteriaComponentInfo.conditionInfo.category,
                logic: criteriaComponentInfo.conditionInfo.type,
                value: criteriaComponentInfo.defaultValue,
                extraValue: criteriaComponentInfo.extraDefaultValue,
            },
        });
        onChange(clonedCriteriaInfoes);
    };

    const onLogicChange = (order, logic) => {
        const clonedCriteriaInfoes = [...criteriaInfoes];
        clonedCriteriaInfoes[order - 1].logic = logic;
        onChange(clonedCriteriaInfoes);
    };

    return (
        <div className="reco-criterias-container">
            {analyseMethod !== AnalyseMethodConstants.type.None &&
                innerCriteriaInfoes.map((item, index) => {
                    return (
                        <div key={index}>
                            <div className="reco-criteria">
                                <RuleCriteriaCondition
                                    key={index}
                                    criterias={AnalyseMethodCriterias.get(
                                        analyseMethod
                                    )}
                                    criteriaInfo={item}
                                    onChange={(info) =>
                                        onCriteriaChange(item.order, info)
                                    }
                                    ref={componentRefs.current[index]}
                                />
                                {innerCriteriaInfoes.length > 1 && (
                                    <div>
                                        <R.Button
                                            id="raDelBtn"
                                            type="bald"
                                            icon="crm-criteria fia-close"
                                            tooltip={RMResx.RM_JS_Common_Delete}
                                            onClick={() => onDelete(item.order)}
                                        />
                                    </div>
                                )}

                                {analyseMethod !== AnalyseMethodConstants.type.DuplicateDocument && (
                                    <div>
                                        <R.Button
                                            id="raAddBtn"
                                            type="bald"
                                            icon="crm-criteria fia-plus"
                                            tooltip={
                                                RMResx.RM_JS_BCM_Explorer_MRR_Add_Button_Add
                                            }
                                            onClick={() => onAdd(item.order)}
                                        />
                                    </div>
                                )}
                            </div>
                            {item.logic !== LogicConstants.type.None &&
                                innerCriteriaInfoes.length !== 1 && (
                                    <div className="reco-criteria-logic">
                                        <div
                                            tabIndex="0"
                                            role="button"
                                            className={
                                                item.logic ===
                                                LogicConstants.type.And
                                                    ? "reco-criteria-logic-btn-ckecked"
                                                    : "reco-criteria-logic-button"
                                            }
                                            onClick={() =>
                                                onLogicChange(
                                                    item.order,
                                                    LogicConstants.type.And
                                                )
                                            }
                                        >
                                            {LogicConstants.i18n.get(
                                                LogicConstants.type.And
                                            )}
                                        </div>
                                        <div
                                            tabIndex="0"
                                            role="button"
                                            className={
                                                item.logic ===
                                                LogicConstants.type.Or
                                                    ? "reco-criteria-logic-btn-ckecked"
                                                    : "reco-criteria-logic-button"
                                            }
                                            onClick={() =>
                                                onLogicChange(
                                                    item.order,
                                                    LogicConstants.type.Or
                                                )
                                            }
                                        >
                                            {LogicConstants.i18n.get(
                                                LogicConstants.type.Or
                                            )}
                                        </div>
                                    </div>
                                )}
                        </div>
                    );
                })}
            <div className="reco-criterias-logic">
                {LogicBuilder.translate(criteriaLogicText)}
            </div>
        </div>
    );
};

export default forwardRef(RuleCriteriaManager);

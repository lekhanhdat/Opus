import { useRef } from "react";
import _ from "lodash";
import RuleCategoryPanel from "../../../RuleManagement/RuleCategoryPanel";
import LogicBuilder from "../../../RuleManagement/util/LogicBuilder";
import { AnalyseMethodConstants } from "../../../RuleManagement/Constants";
import { DiscoveryQueryDataType } from "../../../Analysis/Constants";

const RuleCategoryInfoesManager = ({
    supportAnalyseMethods,
    ruleCategoryInfoes,
    onChange,
    dataType,
}) => {

    const ruleCategoryRef = useRef(null);

    const onAddRuleCategory = () => {
        ruleCategoryRef.current.onShow(RMResx.RM_JS_Common_Create);
    };

    const onRuleCategoryChange = (ruleCategoryInfo) => {
        const clonedRuleCategoryInfoes = _.cloneDeep(ruleCategoryInfoes);
        if (_.isNil(ruleCategoryInfo.order)) {
            ruleCategoryInfo.order =
                (clonedRuleCategoryInfoes.length > 0
                    ? _.last(clonedRuleCategoryInfoes).order
                    : 0) + 1;
            clonedRuleCategoryInfoes.push(ruleCategoryInfo);
        } else {
            clonedRuleCategoryInfoes[ruleCategoryInfo.order - 1] =
                ruleCategoryInfo;
        }

        onChange(clonedRuleCategoryInfoes);
    };

    const onDelete = (order) => {
        let clonedRuleCategoryInfoes = _.cloneDeep(ruleCategoryInfoes);
        const filteredCategoryInfoes = clonedRuleCategoryInfoes.filter(
            (item) => item.order !== order
        );
        clonedRuleCategoryInfoes = filteredCategoryInfoes.map((item, index) => {
            item.order = index + 1;
            return item;
        });
        onChange(clonedRuleCategoryInfoes);
    };

    const onEdit = (order) => {
        const willEditCategoryInfo = _.cloneDeep(ruleCategoryInfoes[order - 1]);
        ruleCategoryRef.current.onShow(RMResx.RM_JS_Common_Edit, willEditCategoryInfo);
    };

    const onExpand = (order) => {
        let clonedRuleCategoryInfoes = _.cloneDeep(ruleCategoryInfoes);
        clonedRuleCategoryInfoes[order - 1].expanded =
            !clonedRuleCategoryInfoes[order - 1].expanded;
        onChange(clonedRuleCategoryInfoes);
    };

    const onCheckboxChanged = (isChecked, order) => {
        let clonedRuleCategoryInfoes = _.cloneDeep(ruleCategoryInfoes);
        clonedRuleCategoryInfoes[order - 1].checked = isChecked;
        clonedRuleCategoryInfoes[order - 1].isEnable = isChecked;
        onChange(clonedRuleCategoryInfoes);
    };

    return (
        <>
            <div className="reco-ac-id-actions">
                <R.Button
                    id="raAddBtn"
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_FA_Discovery_Rule_AddBtn}
                    onClick={onAddRuleCategory}
                />
            </div>
            <div className="reco-rule-categories">
                <div className="reco-ac-rule-categories">
                    {ruleCategoryInfoes.map((item, index) => (
                        <div key={index} className={"reco-ac-rule-category" + (item.isEnable ? " reco-ac-rule-category-checked" : " reco-ac-rule-category-unchecked")}>
                            <div className="reco-ac-rule-top">
                                <div className="reco-ac-rule-name-and-actions">
                                    <div className="reco-ac-rule-name">
                                        <R.Checkbox
                                            xid={"raDiscoveryRuleName"}
                                            text={item.name}
                                            title={item.name}
                                            value={item.id}
                                            checked={item.isEnable}
                                            onChange={(isChecked) => onCheckboxChanged(isChecked, item.order)}
                                        />
                                    </div>
                                    <div className="reco-ac-rule-actions">
                                        <div>
                                            <R.Button
                                                id="raEditBtn"
                                                type="bald"
                                                icon="fia-edit"
                                                tooltip={RMResx.RM_JS_Common_Edit}
                                                onClick={() => onEdit(item.order)}
                                            />
                                        </div>
                                        <div>
                                            <R.Button
                                                id="raDelBtn"
                                                type="bald"
                                                icon="fia-delete"
                                                tooltip={RMResx.RM_JS_Common_Delete}
                                                onClick={() => onDelete(item.order)}
                                            />
                                        </div>
                                        <div>
                                            <R.Button
                                                id="raExpandBtn"
                                                type="bald"
                                                icon={
                                                    item.expanded
                                                        ? "fia-angle-up"
                                                        : "fia-angle-down"
                                                }
                                                tooltip={
                                                    item.expanded
                                                        ? RMResx.RM_JS_Common_Collapse
                                                        : RMResx.RM_JS_Common_Expand
                                                }
                                                onClick={() => onExpand(item.order)}
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="reco-ac-description" tabIndex="0">
                                    {item.description}
                                </div>
                            </div>
                            {item.expanded && (
                                <>
                                    <div className="reco-ac-rule-split-line"></div>
                                    <div className="reco-ac-rule-bottom">
                                        {dataType === DiscoveryQueryDataType.Rot && <div>
                                            {RMResx.RM_FA_Discovery_RulePanel_Method + ": " + AnalyseMethodConstants.i18n.get(item.analyseMethod)}
                                        </div>}

                                        {LogicBuilder.getCriteriaDisplayInfoes(
                                            item.analyseMethod,
                                            item.criteriaInfoes
                                        ).map((item, index) => (
                                            <div
                                                key={index}
                                                className="reco-ac-rule-display-texts"
                                                tabIndex="0"
                                            >
                                                <div className="reco-ac-rule-display-text">
                                                    {item.order}.
                                                </div>
                                                <div className="reco-ac-rule-display-text">
                                                    {item.criteriaName},
                                                </div>
                                                {item.extraComponent && (
                                                    <div className="reco-ac-rule-display-text">
                                                        ( {item.extraValue} ),
                                                    </div>
                                                )}
                                                <div className="reco-ac-rule-display-text">
                                                    {item.conidtionName},
                                                </div>
                                                <div className="reco-ac-rule-display-text">
                                                    ( {item.value} )
                                                </div>
                                            </div>
                                        ))}
                                        {
                                            <div className="reco-ac-rule-logic-text" tabIndex="0">
                                                {LogicBuilder.translate(
                                                    LogicBuilder.build(
                                                        item.criteriaInfoes
                                                    )
                                                )}
                                            </div>
                                        }
                                    </div>
                                </>
                            )}
                        </div>
                    ))}
                </div>
            </div>
            <RuleCategoryPanel
                ruleCategoryInfoes={ruleCategoryInfoes}
                supportAnaylseMethods={supportAnalyseMethods}
                onChange={onRuleCategoryChange}
                ref={ruleCategoryRef}
            />
        </>
    );
};

export default RuleCategoryInfoesManager;

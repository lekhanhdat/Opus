import RuleDetail from "../../Common/RuleDetail/Index";
import { RuleSourceComponents } from "./RuleDetailConfig";
import { useRef } from "react";

const RuleExpanderDetail = ({ ruleItem }) => {
    const ruleDetail = useRef();

    const onKeyDown = (event) => {
        if (event.keyCode == 13) {
            event.target.click();
        }
    };

    const onRuleViewClick = () => {
        ruleDetail.current.load({ ruleId: ruleItem.RuleId, checkModule: -1 });      //-1:all view detail from rule management,it is possible to support two types.
    };

    return (
        <div className="ra-rule-expander-detail">
            <RuleDetail
                isExistPanel={false}
                showRuleBaseDetail={false}
                ruleSourceComponents={RuleSourceComponents}
                ruleInfo={ruleItem}
            ></RuleDetail>
            <div className="ra-rule-detail-align">
                <span
                    tabIndex="0"
                    className="ra-rule-detail-span"
                    onClick={onRuleViewClick}
                    onKeyDown={onKeyDown}
                >
                    {RMResx.RM_PRM_PRE_RuleView}
                </span>
            </div>
            <RuleDetail ref={ruleDetail} />
        </div>
    );
};

export default RuleExpanderDetail;

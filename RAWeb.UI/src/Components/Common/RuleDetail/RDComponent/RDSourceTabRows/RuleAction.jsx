import DetailRow from "../../Common/DetailRow";

const RuleAction = ({ruleItem}) =>{
    return <DetailRow label={RMResx.RM_JS_Rule_Detail_DWSP}> 
        {ruleItem.ArchiverActions}
    </DetailRow>;
};

export default RuleAction;
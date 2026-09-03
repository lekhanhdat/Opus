import DetailRow from "../../Common/DetailRow";

const RuleCriterias = ({ruleItem, icon}) =>{

    const renderRuleCriterias = () =>{
        let combineModeRex = /\s*and\s*\)$/gi;
        let combineMode = ruleItem.FilterCombineMode;
        if (combineModeRex.test(combineMode)) {
            combineMode = combineMode.replace(combineModeRex, ')');
        }
        return <React.Fragment>
            <div>
                {
                    ruleItem.RuleCretias?.map((item, index) => {
                        return <div key={index} className="flex align-center">
                            <span className={icon}>
                                <span className="path1"></span>
                                <span className="path2"></span>
                                <span className="path3"></span>
                                <span className="path4"></span>
                                <span className="path5"></span>
                                <span className="path6"></span>
                            </span>
                            <span data-tooltip="ifneed" className="margin-left-xs ra-criteria">{item}</span>
                        </div>;
                    })
                }
            </div>
            <div>{combineMode}</div>
        </React.Fragment>;
    };

    return <DetailRow label={RMResx.RM_JS_Rule_Detail_Criteria}>
        {renderRuleCriterias()}
    </DetailRow>;
};

export default RuleCriterias;
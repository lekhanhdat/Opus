import { useEffect, useState } from 'react';
import RDConfig from "../Config";
import { isEmptyObject } from '../../../../Utilities/CommonUtil';

const BaseRows = ({ ruleItem, module }) => {

    const [baseRows, setBaseRows] = useState([]);

    useEffect(()=>{
        if(ruleItem && !isEmptyObject(ruleItem)){
            setBaseRows(new RDConfig(ruleItem, module).getRuleBaseConfig());
        }
    },[ruleItem]);

    return <div className="ra-rd-base-column">
        <$g.DetailList labelWidth={220} >
            {baseRows.map((item, index) => {
                return <$g.DetailRow key={index}>
                    <$g.DetailCell label={item.name}>
                        <span tabIndex="0">{item.value}</span>
                    </$g.DetailCell>
                </$g.DetailRow>; })}
        </$g.DetailList>
    </div>;
};

export default BaseRows;
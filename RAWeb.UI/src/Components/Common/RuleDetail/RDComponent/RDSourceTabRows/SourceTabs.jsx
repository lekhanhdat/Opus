import { useEffect, useState } from 'react';
import RDConfig from "../../Config";
import { isEmptyObject } from '../../../../../Utilities/CommonUtil';

const SourceTabs = ({
    ruleItem, 
    sourceComponents = new RDConfig().getRuleSourceComponents(),
    showRuleBaseDetail
}) =>{

    const sourceConfig = new RDConfig(ruleItem).getRuleSourceConfig();

    const [tabControlIndex, setTabControlIndex] = useState(0);

    const [ruleSourceConfig, setRuleSourceConfig] = useState([]);

    const [ruleSourceComponents, setRuleSourceComponents] = useState([]);

    const onChangeTabIndex = (tabIndex) =>{ setTabControlIndex(tabIndex);};

    useEffect(()=>{
        setRuleSourceConfig(sourceConfig);
        setRuleSourceComponents(sourceComponents);
        // setTabControlIndex(sourceConfig.find(item => item.show)?.tabIndex);
    },[ruleItem]);

    const renderContentSourceTitle = () => {
        if(showRuleBaseDetail && !isEmptyObject(ruleItem)){
            return <div className="ra-rd-rule-setting-title" tabIndex='0'>
                {RMResx.RM_JS_Rule_SourcesAndCriterias_Title}
            </div>;
        }
    };

    const renderDetailSourceRows = (item) =>{
        if(item.show){ 
            return <$g.DetailList>
                {
                    ruleSourceComponents[ruleItem.ModelType][item.tabIndex].map((Component, index)=>{
                        return <Component key={index} 
                            ruleItem={item.content}
                            icon={item.icon}
                        />;
                    })
                }
            </$g.DetailList>;
        }
    };

    return <React.Fragment>
        {renderContentSourceTitle()}
        <R.Tabcontrol
            active={tabControlIndex}
            onChange={onChangeTabIndex}>
            {
                // Use filter instead of hide props because hide props doesn't work when open the "..." dropdown
                ruleSourceConfig.filter((item) => item.show).map((item, index)=>{
                    return <R.TabPanel tab={item.name} key={index}>
                        {renderDetailSourceRows(item)}
                    </R.TabPanel>;
                })
            } 
        </R.Tabcontrol> 
    </React.Fragment>; 
};

export default SourceTabs;
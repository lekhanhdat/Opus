import React, {createContext, useEffect, useState} from 'react';

const RuleContext = createContext();

const RuleContextProvider = (props) => {
    const [reloadRules, setReloadRules] = useState(true);
    const [allRules, setAllRules] = useState([]);
    const loadRules = async () => {
        const result = await fetchUtility({url:'/api/EXOSettingApi/GetAvailableRuleList', method: 'get'});
        result && setAllRules(result);
    };

    const reload = () =>  {
        setReloadRules(true);
    };

    useEffect(() => {
        if(reloadRules) {
            loadRules();
            setReloadRules(false);
        } 
    },[reloadRules]);
    
    return (<RuleContext.Provider value={{allRules, reload}}>
        {props.children}
    </RuleContext.Provider>);
};

export {RuleContext, RuleContextProvider};
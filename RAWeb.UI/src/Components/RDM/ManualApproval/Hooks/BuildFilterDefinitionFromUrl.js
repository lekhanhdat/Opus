import {useEffect} from "react";
import { useLocation } from 'react-router-dom';

const Filter_Param_Name = "filter";

const Value_Param_Name = "value";

const useBuildFilterDefinitionFromUrl = (effect) => {

    const location = useLocation();

    useEffect(() => {
        const locationQueryparam = location.query;
        if(!locationQueryparam){
            if(RM.Url.getParam(window.location.href, "filter") === "All"){
                var url = window.location.href;                    
                if(url.indexOf("?") > 0){                        
                    url = url.replace(/(\?|#)[^'"]*/, '');          
                    window.history.pushState({}, "" ,url);
                }
                effect([], true);
                return;
            }
            effect([]);
            return;
        }
        const filterOption = locationQueryparam[Filter_Param_Name];
        const valueOption = locationQueryparam[Value_Param_Name];
        const value = {
            FilterOption: parseInt(filterOption),
            Value: JSON.stringify([parseInt(valueOption)])
        };

        effect([value]);

    }, []);
};

export default useBuildFilterDefinitionFromUrl;
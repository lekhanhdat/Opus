import {useEffect} from "react";

const Filter_Param_Name = "filter";

const Value_Param_Name = "value";

const useBuildFilterDefinitionFromUrl = (effect) => {
    useEffect(() => {
        const filterOption = RM.Url.getParam(window.location.href, Filter_Param_Name);
        const valueOption = RM.Url.getParam(window.location.href, Value_Param_Name);
        if(filterOption === "" || valueOption === ""){
            effect([]);
            return;
        }

        const value = {
            FilterOption: parseInt(filterOption),
            Value: JSON.stringify([parseInt(valueOption)])
        };

        effect([value]);

    }, []);
};

export default useBuildFilterDefinitionFromUrl;
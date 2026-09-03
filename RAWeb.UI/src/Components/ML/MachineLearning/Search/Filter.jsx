import React, { useRef } from "react";
import FilterPanel from "./FilterPanel";

const Filter = ({FilterForm, onFilter, filterParam}) =>{

    const filterPanel = useRef();

    const openFilterPanel = () => {
        filterPanel.current.openPanel();
    };

    const onSave = (filterOptions) =>{
        onFilter(filterOptions);
    };

    return <div>
        <R.Button  
            icon="fia-filter"  
            text={RMResx.RM_Common_Filter} 
            onClick={openFilterPanel} 
        />
        <FilterPanel 
            ref={filterPanel} 
            onFilter={onSave}  
            FilterForm={FilterForm}
            filterParam={filterParam}
        />
    </div>;
};

export default Filter;
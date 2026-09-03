import { useState, forwardRef, useImperativeHandle, useRef } from "react";

const FilterPanel = ({onFilter, FilterForm, filterParam}, ref) =>{

    const [showFilterPanel, setShowFilterPanel] = useState(false);

    const [filterColumnsParam, setFilterColumnsParam] = useState([]);

    const filterForm = useRef();

    useImperativeHandle(ref, () => ({
        openPanel: () => {
            setShowFilterPanel(true);
        },
    }));

    const onCloseFilterPanel = () => {
        setShowFilterPanel(false);
    };

    const onSave = () => {
        setFilterColumnsParam(filterForm.current.getColumns());
        onFilter(filterForm.current.getColumns());
        onCloseFilterPanel();
    };

    const onClear = () => {
        filterForm.current.clearColumns();
    };

    return <div>   
        <R.Panel  
            header={RMResx.RM_Common_Filter}
            size={664}
            status={{ show: showFilterPanel }}
            onHide={onCloseFilterPanel}
            destroy={true}
        >
            <div>
                <a  
                    role="button" 
                    tabIndex="0"
                    onClick={onClear} 
                    className="ra-main-filter-clear fia-funnel-clear"
                > {RMResx.RM_Common_ClearFilter}</a>
                <FilterForm ref={filterForm} filterColumnsParam={filterParam || filterColumnsParam}/>
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={onCloseFilterPanel} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={onSave} />
            </>
        </R.Panel>
    </div>;
};

export default forwardRef(FilterPanel);

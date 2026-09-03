import { useEffect, useState } from "react";
import { FilterOptions } from "../Constants";

function CustomColumnTextFilter(props) {
    const {
        title,
        filterOption,
        customColumnId,
        filterDefinitions,
        onFilterChange,
        onRemoveFilterChange,
    } = props;

    const [inputValue, setInputValue] = useState("");

    useEffect(() => {
        if (!filterDefinitions.has(customColumnId)) {
            setInputValue("");
            return;
        }
        const objValue = filterDefinitions.get(customColumnId);
        const value = objValue.Value;
        setInputValue(value);
    }, [filterDefinitions])

    const onChange = (value) => {
        setInputValue(value);
        if (!value) {
            onRemoveFilterChange(customColumnId);
            return;
        }
        const args = {
            FilterOption: filterOption,
            Value: value,
            CustomColumnId: customColumnId,
        };
        onFilterChange(args);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title" tabIndex="0">
                {title}
            </div>
            <R.Input
                id={`raCustomColumnIpt-${customColumnId}`}
                type={"text"}
                placeholder={filterOption === FilterOptions.CustomColumnNumber ? RMResx.RM_JS_MA_Grid_CustomColumnNumberPlaceholder : RMResx.RM_JS_MA_Grid_CustomColumnTextPlaceholder}
                value={inputValue}
                onChange={onChange}
                aria={{ ariaLabel: title }}
            />
        </div>
    );
}

export default CustomColumnTextFilter;

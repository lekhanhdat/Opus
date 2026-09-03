import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import _ from "lodash";

const FilterOption = FilterOptions.Source;

const BuildDefaultOptionsSelectItems = (options, selectedItems) => {
    const result = [];
    for (const option of options) {
        result.push({
            key: option.Key,
            value: option.Value,
            checked: selectedItems.some(item => item === option.Key),
        });
    }
    return result;
};

const SourceFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions, availableOptions }) => {

    const [selectorItems, setSelectorItems] = useState([]);

    useEffect(() => {

        if(_.isNil(availableOptions)) {
            return;
        }

        if(!filterDefinitions.has(FilterOption)) {
            const items = BuildDefaultOptionsSelectItems(availableOptions, availableOptions.map(item => item.Key));
            setSelectorItems(items);
            return;
        }

        const value = filterDefinitions.get(FilterOption);
        const items = BuildDefaultOptionsSelectItems(availableOptions, JSON.parse(value.Value));
        setSelectorItems(items);
    }, [filterDefinitions, availableOptions]);

    const onChange = (args) => {

        if (
            args.isSelectAll &&
            onRemoveFilterChange
        ) {
            onRemoveFilterChange(FilterOption);
            return;
        }

        const selectedItems = [];
        for (const selectedItem of args.newValue) {
            selectedItems.push(selectedItem.key);
        }

        const value = {
            FilterOption: FilterOption,
            Value: JSON.stringify(selectedItems)
        };
        if (onFilterChange) {
            onFilterChange(value);
        }
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title">
                {
                    FilterI18Ns.get(FilterOption)
                }
            </div>
            <R.Multicombobox
                height={34}
                width={"100%"}
                checkedField="checked"
                textField="value"
                valueField="key"
                hasFilter={true}
                required={true}
                items={selectorItems}
                noneText="Manage Columns"
                onChange={onChange}
            />
        </div>
    );
};

export default SourceFilter;


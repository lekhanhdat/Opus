import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns, ApprovalStatusI18Ns } from "../Constants/index";
import _ from "lodash";

const FilterOption = FilterOptions.ApprovalStatus;

const BuildDefaultOptionsSelectItems = (options, selectedItems) => {
    const result = [];
    for (const option of options) {
        const optionValue = ApprovalStatusI18Ns.get(option);
        result.push({
            key: option,
            value: optionValue,
            checked: selectedItems.some(item => item === option),
        });
    }
    return result;
};

const ApprovalStatusFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions, availableOptions }) => {

    const [selectorItems, setSelectorItems] = useState([]);

    useEffect(() => {

        if(_.isNil(availableOptions)) {
            return;
        }

        if (!filterDefinitions.has(FilterOption)) {
            const items = BuildDefaultOptionsSelectItems(availableOptions, availableOptions);
            setSelectorItems(items);
            return;
        }

        const value = filterDefinitions.get(FilterOption);
        const items = BuildDefaultOptionsSelectItems(availableOptions, JSON.parse(value.Value));
        setSelectorItems(items);
    }, [filterDefinitions, availableOptions]);

    const onChange = (args) => {
        if (args.isSelectAll && onRemoveFilterChange) {
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

        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title" tabIndex="0">
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
                noneText={FilterI18Ns.get(FilterOption)}
                onChange={onChange}
            />
        </div>
    );
};

export default ApprovalStatusFilter;


import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import _ from "lodash";

const FilterOption = FilterOptions.RuleDisposalClass;

const BuildDefaultOptionsSelectItems = (options, selectedItems) => {
    const result = [];
    for (const option of options) {
        result.push({
            key: option,
            value: option,
            checked: selectedItems.some(item => item === option),
        });
    }
    return result;
};

const RuleDisposalClassFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions, availableOptions }) => {

    const [selectorItems, setSelectorItems] = useState([]);

    useEffect(() => {

        if(_.isNil(availableOptions)) {
            return;
        }

        if(!filterDefinitions.has(FilterOption)) {
            const items = BuildDefaultOptionsSelectItems(availableOptions, availableOptions);
            const uniqueItems = items.filter((item, index, arr) => index === arr.findIndex(x => x.key === item.key));
            setSelectorItems(uniqueItems);
            return;
        }

        const value = filterDefinitions.get(FilterOption);
        const items = BuildDefaultOptionsSelectItems(availableOptions, JSON.parse(value.Value));
        const uniqueItems = items.filter((item, index, arr) => index === arr.findIndex(x => x.key === item.key));
        setSelectorItems(uniqueItems);
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
        if (onFilterChange) {
            onFilterChange(value);
        }
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
                lazyStep={false}
            />
        </div>
    );
};

export default RuleDisposalClassFilter;


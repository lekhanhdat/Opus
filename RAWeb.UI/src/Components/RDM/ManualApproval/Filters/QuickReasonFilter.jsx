import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import _ from "lodash";

const FilterOption = FilterOptions.QuickReason;

const BuildDefaultOptionsSelectItems = (options, selectedItems) => {
    const setOptions = [...new Set(options)]; 
    const itemOptions = [];
    for (const option of setOptions) {
        itemOptions.push({
            key: option,
            value: option,
            checked: selectedItems.some(item => item === option),
        });
    }
    const result = [
        ...itemOptions.filter(item => item.value !== RMResx.RM_JS_DN_DisposalNull),
        ...itemOptions.filter(item => item.value === RMResx.RM_JS_DN_DisposalNull),
    ];
    return result;
};

function ignoreCaseSort(a, b) {
    const valA = a.toLowerCase();
    const valB = b.toLowerCase();
  
    if (valA < valB) {
      return -1;
    }
    if (valA > valB) {
      return 1;
    }
    return 0;
  }

const QuickReasonFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions ,approvalCommentQuickReasons}) => {

    const [selectorItems, setSelectorItems] = useState([]);

    useEffect(() => {

        let clonedReasons = _.cloneDeep(approvalCommentQuickReasons)
        clonedReasons = clonedReasons.filter(reason => reason !== "");
        clonedReasons.push(RMResx.RM_JS_DN_DisposalNull);

        clonedReasons.sort(ignoreCaseSort);

        if(!filterDefinitions.has(FilterOption)) {
            const items = BuildDefaultOptionsSelectItems(clonedReasons, clonedReasons);
            setSelectorItems(items);
            return;
        }

        const value = filterDefinitions.get(FilterOption);
        const items = BuildDefaultOptionsSelectItems(clonedReasons, JSON.parse(value.Value));
        setSelectorItems(items);
    }, [filterDefinitions, approvalCommentQuickReasons]);

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
            />
        </div>
    );
};

export default QuickReasonFilter;
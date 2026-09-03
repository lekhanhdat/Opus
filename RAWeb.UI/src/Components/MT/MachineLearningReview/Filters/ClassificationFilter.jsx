import React, { useState, useEffect } from "react";
import _ from "lodash";

import { FilterOptions, FilterI18Ns } from "../Constants/index";
import { getChoiceOptions } from "../../../ML/MachineLearning/Filter/Common/Function";

const FilterOption = FilterOptions.Classification;

const ClassificationFilter = (props) => {
    const {
        onFilterChange,
        onRemoveFilterChange,
        filterDefinitions,
        options,
    } = props;
    
    const [selectedUsers, setSelectedClassifications] = useState([]);

    useEffect(() => {
        const value = filterDefinitions.get(FilterOption);
        const classificationOptions = getChoiceOptions(options, value ? JSON.parse(value.Value) : []);
        setSelectedClassifications(classificationOptions);
    }, [filterDefinitions, options]);

    const onChange = (args) => {
        if (args.isSelectAll) {
            onRemoveFilterChange(FilterOption);
            return;
        }

        const selectedItems = [];
        for (const selectedItem of args.newValue) {
            selectedItems.push(selectedItem.value);
        }
        const value = {
            FilterOption: FilterOption,
            Value: JSON.stringify(selectedItems),
        };
        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title">
                {FilterI18Ns.get(FilterOption)}
            </div>
            <R.Multicombobox
                height={34}
                width={"100%"}
                hasFilter
                required
                textField="name"
                items={selectedUsers}
                noneText={FilterI18Ns.get(FilterOption)}
                onChange={onChange}
            />
        </div>
    );
};

export default ClassificationFilter;
import React, { useState, useEffect } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import _ from "lodash";

import PeoplePicker from "../../../Common/PeoplePicker";

const FilterOption = FilterOptions.MLReviewer;

const RecordReviewerFilter = ({onFilterChange, onRemoveFilterChange, filterDefinitions}) => {

    const [selectedUsers, setSelectedUsers] = useState([]);

    useEffect(() => {
        if(!filterDefinitions.has(FilterOption)) {
            setSelectedUsers([]);
            return;
        }

        const value = filterDefinitions.get(FilterOption);
        setSelectedUsers(value.AttacheValue);
    }, [filterDefinitions]);

    const onChange = (users) => {
        setSelectedUsers(users);

        if(_.isNil(users) || users.length === 0) {
            onRemoveFilterChange(FilterOption);
            return;
        }

        const userIds = users.map(item => item.RMUserId);
        var value = {
            FilterOption: FilterOption,
            Value: JSON.stringify(userIds),
            AttacheValue: _.cloneDeep(users)
        };
        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title">
                {
                    FilterI18Ns.get(FilterOption)
                }
            </div>
            <PeoplePicker
                width="100%"
                items={selectedUsers}
                selectionChanged={onChange}
                onlyFromRecord={true}
            />
        </div>
    );
};

export default RecordReviewerFilter;
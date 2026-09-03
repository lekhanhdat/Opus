import React, { useEffect, useState } from "react";
import _ from "lodash";

import { FilterOptions, FilterI18Ns } from "../Constants/index";

const DateModifiedFilter = (props) => {
    const {
        onFilterChange,
        onRemoveFilterChange,
        filterDefinitions,
        filterOption = FilterOptions.DateModified,
    } = props;

    const [startDate, setStartDate] = useState(null);

    const [endDate, setEndDate] = useState(null);

    useEffect(() => {
        if (!filterDefinitions.has(filterOption)) {
            setStartDate(null);
            setEndDate(null);
            return;
        }

        const jsonValue = filterDefinitions.get(filterOption);
        const value = JSON.parse(jsonValue.Value);
        setStartDate(new Date(value.StartTime));
        setEndDate(new Date(value.EndTime));
    }, [filterDefinitions]);

    const onChange = (args) => {
        if (_.isNil(args.newValue)) {
            setStartDate(null);
            setEndDate(null);
            onRemoveFilterChange(filterOption);
            return;
        }
        const startDate = args.newValue.start;
        const endDate = args.newValue.end;
        setStartDate(startDate);
        setEndDate(endDate);
        const dateValue = {
            StartTime: RM.TimeUtil.getCommonDateStr(startDate),
            EndTime: RM.TimeUtil.getCommonDateStr(endDate),
        };

        const value = {
            FilterOption: filterOption,
            Value: JSON.stringify(dateValue),
        };

        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div
                className="reco-manual-review-filter-title"
                id={"ariaDateModified" + filterOption}
            >
                {FilterI18Ns.get(filterOption)}
            </div>
            <R.Rangepicker
                selectedDate={
                    _.isNil(startDate) || _.isNil(endDate)
                        ? null
                        : {
                              start: startDate,
                              end: endDate,
                          }
                }
                data-part="vtWidget"
                width={"100%"}
                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                onChange={onChange}
                clearable={true}
                aria={`#ariaDateModified${filterOption}`}
            />
        </div>
    );
};

export default DateModifiedFilter;

import React, { useEffect, useState } from "react";
import { FilterOptions, FilterI18Ns } from "../Constants/index";
import _ from "lodash";

const CollectionTimeFilter = ({ onFilterChange, onRemoveFilterChange, filterDefinitions, filterOption = FilterOptions.PredictTime }) => {

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
        if(_.isNil(args.newValue)) {
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
            EndTime: RM.TimeUtil.getCommonDateStr(endDate)
        };

        const value = {
            FilterOption: filterOption,
            Value: JSON.stringify(dateValue)
        };

        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title" id={"ariaCollectionTime" + filterOption}>
                {
                    FilterI18Ns.get(filterOption)
                }
            </div>
            <R.Rangepicker
                selectedDate={_.isNil(startDate) || _.isNil(endDate) ? null : {
                    start: startDate,
                    end: endDate,
                }}
                data-part="vtWidget"
                width={"100%"}
                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                onChange={onChange}
                clearable={true}
                aria={`#ariaCollectionTime${filterOption}`}
            />
        </div>
    );

};

export default CollectionTimeFilter;


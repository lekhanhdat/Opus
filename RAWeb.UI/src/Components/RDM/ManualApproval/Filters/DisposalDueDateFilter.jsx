import { useEffect, useState } from "react";
import _ from "lodash";

import { FilterI18Ns, FilterOptions } from "../Constants/index";

const FilterOption = FilterOptions.DisposalDueDate;

const DisposalDueDateFilter = (props) => {
    const { onFilterChange, onRemoveFilterChange, filterDefinitions } = props;

    const [startDate, setStartDate] = useState(null);
    const [endDate, setEndDate] = useState(null);

    useEffect(() => {
        if (!filterDefinitions.has(FilterOption)) {
            setStartDate(null);
            setEndDate(null);
            return;
        }

        const jsonValue = filterDefinitions.get(FilterOption);
        const value = JSON.parse(jsonValue.Value);
        setStartDate(new Date(value.StartTime));
        setEndDate(new Date(value.EndTime));
    }, [filterDefinitions]);

    const onChange = (args) => {
        if (_.isNil(args.newValue)) {
            setStartDate(null);
            setEndDate(null);
            onRemoveFilterChange(FilterOption);
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
            FilterOption: FilterOption,
            Value: JSON.stringify(dateValue),
        };

        onFilterChange(value);
    };

    return (
        <div className="reco-manual-review-filter">
            <div className="reco-manual-review-filter-title" tabIndex="0">
                {FilterI18Ns.get(FilterOption)}
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
            />
        </div>
    );
};

export default DisposalDueDateFilter;

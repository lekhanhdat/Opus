import { useEffect, useState } from 'react';
import _ from "lodash";

const DatetimeColumn = ({label, options, onChange})=>{

    const [startDate, setStartDate] = useState(null);
    const [endDate, setEndDate] = useState(null);

    useEffect(() => {
        if (options && options.length > 0) {
            setStartDate(new Date(options[0]));
            setEndDate(new Date(options[1]));
        } else {
            setStartDate(null);
            setEndDate(null);
            return;
        }

        
    }, [options]);

    const onChangeDateRange = (args) => {
        if(_.isNil(args.newValue)) {
            setStartDate(null);
            setEndDate(null);
            return;
        }
        const startDate = args.newValue.start;
        const endDate = args.newValue.end;
        setStartDate(startDate);
        setEndDate(endDate);
        const dateValue = [
            RM.TimeUtil.getCommonDateStr(startDate),
            RM.TimeUtil.getCommonDateStr(endDate)
        ];

        onChange(dateValue);
    };

    return <$g.FormRow label={label}>
        <R.Rangepicker
            selectedDate={_.isNil(startDate) || _.isNil(endDate) ? null : {
                start: startDate,
                end: endDate,
            }}
            data-part="vtWidget"
            width={"100%"}
            dateTimeFormat={RM.TimeSettingModel.DateFormat}
            onChange={onChangeDateRange}
            clearable={true}
        />
    </$g.FormRow >;
};
export default DatetimeColumn;
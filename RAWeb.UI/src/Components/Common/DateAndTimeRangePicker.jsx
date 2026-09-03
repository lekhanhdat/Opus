import React, { useState } from "react";
import PropTypes from 'prop-types';

const DateAndTimeRangePicker = ({ startPickerInfo, endPickerInfo, onChange }) => {

    const [isTimeEndBeforeStart, setIsTimeEndBeforeStart] = useState(false);
    const selectedRangeTime = {
        isValid: true,
        newValue: { start: startPickerInfo.selectedDate, end: endPickerInfo.selectedDate },
    };

    let onChangeStartTime = (args, attr) => {
        selectedRangeTime.newValue[attr] = args.newValue;
        selectedRangeTime.isValid = checkIsValid();
        if(onChange){
            onChange(selectedRangeTime);
        }
    };

    let checkTimeEndBeforeStart = () => {
        let isTimeEndBeforeStart = true;
        if(selectedRangeTime.newValue.start && selectedRangeTime.newValue.end){
            let startMillisecond = selectedRangeTime.newValue.end.getTime();
            let endMillisecond = selectedRangeTime.newValue.start.getTime();
            isTimeEndBeforeStart = endMillisecond > startMillisecond;
            setIsTimeEndBeforeStart( isTimeEndBeforeStart );
        }
        return isTimeEndBeforeStart;
    };

    let checkIsValid = () => {
        let isTimeEndBeforeStart = checkTimeEndBeforeStart();
        let isValid = !!(selectedRangeTime.newValue.start && selectedRangeTime.newValue.end && !isTimeEndBeforeStart);
        return isValid;
    };

    let startPickerVerifyMsg = startPickerInfo.verifyMsg === undefined ? RMResx.RM_Common_UnselectedStartTimeTip : startPickerInfo.verifyMsg;
    let endPickerVerifyMsg = endPickerInfo.verifyMsg === undefined ? RMResx.RM_Common_UnselectedEndTimeTip : endPickerInfo.verifyMsg;
    return (
        <div className="ra-timeRange-picker">
            <div className="ra-timeRange-picker-content">
                <R.Validation element="Datepicker" require={startPickerVerifyMsg}>
                    <R.Datepicker
                        id={startPickerInfo.id}
                        width={startPickerInfo.width}
                        height={startPickerInfo.height}
                        tooltip={startPickerInfo.tooltip}
                        selectedDate={startPickerInfo.selectedDate}
                        enableDates={startPickerInfo.enableDates}
                        disabled={startPickerInfo.disabled}
                        onChange={(args)=>{ onChangeStartTime(args, "start");}}
                        clearable={startPickerInfo.clearable}
                        placeholder={RMResx.RM_Common_StartDate}
                        hasTimePicker={true}
                        dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                    />
                </R.Validation>
                <R.Validation element="Datepicker" require={endPickerVerifyMsg}>
                    <R.Datepicker
                        id={endPickerInfo.id}
                        width={endPickerInfo.width}
                        height={endPickerInfo.height}
                        tooltip={endPickerInfo.tooltip}
                        selectedDate={endPickerInfo.selectedDate}
                        enableDates={endPickerInfo.enableDates}
                        disabled={endPickerInfo.disabled}
                        onChange={(args)=>{ onChangeStartTime(args, "end"); }}
                        clearable={endPickerInfo.clearable}
                        placeholder={RMResx.RM_Common_EndDate}
                        hasTimePicker={true}
                        dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                    />
                </R.Validation>
            </div>
            <$g.ValidationMsg show={isTimeEndBeforeStart && (startPickerVerifyMsg && endPickerVerifyMsg)}>
                {RMResx.RM_JS_RDM_CreateRule_Validation_ConditionDateTime}
            </$g.ValidationMsg>
        </div>
    );
};

DateAndTimeRangePicker.propTypes = {
    startPickerInfo: PropTypes.object,
    endPickerInfo: PropTypes.object,
    onChange: PropTypes.func,

};
DateAndTimeRangePicker.defaultProps = {
    startPickerInfo: {
        selectedDate: null,
        verifyMsg: RMResx.RM_Common_UnselectedStartTimeTip
    },
    endPickerInfo: {
        selectedDate: null,
        verifyMsg: RMResx.RM_Common_UnselectedEndTimeTip
    },
    onChange: null,
};

export { DateAndTimeRangePicker };

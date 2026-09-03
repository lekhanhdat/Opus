import _ from "lodash";
import { useState, useRef } from "react";
import { ScheduleType } from "../../../Constants/DataOptimizeType";

const getScheduleOptions = (selectedOption) => {
    return [
        {
            name: RMResx.RM_FA_DataOptimize_Schedule_Now,
            text: RMResx.RM_FA_DataOptimize_Schedule_Now,
            value: ScheduleType.Now,
            checked: selectedOption === ScheduleType.Now,
        },
        {
            name: RMResx.RM_FA_DataOptimize_Schedule_Configure,
            text: RMResx.RM_FA_DataOptimize_Schedule_Configure,
            value: ScheduleType.ConfigSchedule,
            checked: selectedOption === ScheduleType.ConfigSchedule,
        },
    ];
};

const ScheduleConfig = ({ dataOptimizeParameter, onChange }) => {

    const refDateValid = useRef(null);

    const defaultTimeZone = RM.TimeUtil.getGlobalTimezoneInfo();

    const [scheduleOptions, setScheduleOptions] = useState(
        getScheduleOptions(ScheduleType.Now)
    );

    const [selectedSchedule, setSelectedSchedule] = useState(ScheduleType.Now);

    const onScheduleOptionChange = (value) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.scheduleParameter.scheduleType = value;
        if (value === ScheduleType.ConfigSchedule) {
            clonedParameter.scheduleParameter.selectedDate = new Date();
            clonedParameter.scheduleParameter.selectedTime = RM.TimeUtil.getCommonDateStr(new Date()) + ':00';
            clonedParameter.scheduleParameter.timeZoneId = defaultTimeZone.id;
        }
        setScheduleOptions(getScheduleOptions(value));
        setSelectedSchedule(value);
        onChange(clonedParameter);
    };

    const onTimeChange = (args) => {
        const clonedParameter = _.cloneDeep(dataOptimizeParameter);
        clonedParameter.scheduleParameter.selectedDate = args.newValue;
        clonedParameter.scheduleParameter.selectedTime = RM.TimeUtil.getCommonDateStr(args.newValue) + ':00';
        clonedParameter.scheduleParameter.timeZoneId = defaultTimeZone.id;
        onChange(clonedParameter);
        $$.verify(refDateValid.current.ref.current);
    };

    const verifyDate = () => {
        let selectedDate = dataOptimizeParameter.scheduleParameter.selectedDate;
        return selectedDate && (new Date(selectedDate).getTime() > new Date().getTime()) ? true : RMResx.RM_FA_DataOptimize_Validation_Schedule;
    };

    return (
        <div className="reco-optimize-option">
            <div className="reco-optimize-title">{RMResx.RM_FA_DataOptimize_ScheduleTitle}</div>
            <div>
                <R.Radio.Group
                    id="raScheduleRadioGroup"
                    name="reco-data-schedule"
                    block={true}
                    items={scheduleOptions}
                    onChange={onScheduleOptionChange}
                />
                {selectedSchedule === ScheduleType.ConfigSchedule && <div className="reco-optimize-datepicker">
                    <R.Datepicker
                        id="raConfigSchedule"
                        dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                        selectedDate={dataOptimizeParameter.scheduleParameter.selectedDate}
                        hasTimePicker={true}
                        onChange={onTimeChange}
                    />
                    <div className="margin-top-s">
                        <R.ValidationFaker valid={verifyDate} ref={refDateValid} />
                    </div>
                </div>}
            </div>
        </div>
    );
};

export default ScheduleConfig;
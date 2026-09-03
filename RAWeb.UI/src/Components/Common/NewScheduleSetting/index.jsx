import { forwardRef, useEffect, useImperativeHandle, useState } from "react";
import PropTypes from "prop-types";
import "./index.less";
import {
    END_TYPE_OPTIONS,
    END_TYPES,
    getDefaultValue,
    INTERVAL_OPTIONS,
    SCHEDULE_TYPES,
} from "./constants";
import { getBackendValue, getField, getOptionValue } from "./utils";

const toDate = (value) => (value ? new Date(value) : new Date());

const normalizeScheduleData = (scheduleData) => ({
    ...getDefaultValue(),
    ...scheduleData,
    scheduleType:
        scheduleData.scheduleType ||
        (scheduleData.NoSchedule === false
            ? SCHEDULE_TYPES.CONFIGURE
            : SCHEDULE_TYPES.NONE),
    startTime: toDate(
        getField(scheduleData, "startTime", "StartTimeDate") ||
            scheduleData.StartTime,
    ),
    endTime: toDate(
        getField(scheduleData, "endTime", "EndTimeDate") ||
            scheduleData.EndTime,
    ),
    interval: scheduleData.interval ?? scheduleData.Interval ?? 1,
    intervalUnit:
        scheduleData.intervalUnit ||
        getOptionValue(
            INTERVAL_OPTIONS,
            scheduleData.IntervalType,
            undefined,
        ) ||
        INTERVAL_OPTIONS[0].value,
    endType:
        scheduleData.endType ||
        getOptionValue(END_TYPE_OPTIONS, scheduleData.EndType, undefined) ||
        END_TYPES.NONE,
    occurrences: scheduleData.occurrences ?? scheduleData.OccurrencesTotal ?? 1,
});

const getBackendScheduleData = (formData) => ({
    NoSchedule: formData.scheduleType === SCHEDULE_TYPES.NONE,
    StartTime: `${RM.TimeUtil.getCommonDateStr(formData.startTime)}:00`,
    EndTime: `${RM.TimeUtil.getCommonDateStr(formData.endTime)}:00`,
    StartTimeDate: formData.startTime,
    EndTimeDate: formData.endTime,
    TimeZoneId: formData.TimeZoneId,
    Interval: formData.interval,
    IntervalType: getBackendValue(INTERVAL_OPTIONS, formData.intervalUnit, "1"),
    EndType: getBackendValue(END_TYPE_OPTIONS, formData.endType, "0"),
    OccurrencesTotal: formData.occurrences,
    IsDaylightSaving: formData.IsDaylightSaving,
});

export const NewScheduleSetting = forwardRef(
    ({ scheduleData = {}, onScheduleTypeChange = () => {} }, ref) => {
        const [formData, setFormData] = useState(() =>
            normalizeScheduleData(scheduleData),
        );

        useEffect(() => {
            if (scheduleData) {
                setFormData((previous) => ({
                    ...previous,
                    ...normalizeScheduleData(scheduleData),
                }));
            }
        }, [scheduleData]);

        useImperativeHandle(
            ref,
            () => ({
                getScheduleData: () =>
                    formData.scheduleType === SCHEDULE_TYPES.NONE
                        ? null
                        : getBackendScheduleData(formData),
            }),
            [formData],
        );

        const updateField = (field, nextValue) => {
            setFormData((previous) => ({ ...previous, [field]: nextValue }));
        };

        const handleScheduleTypeChange = (scheduleType) => {
            updateField("scheduleType", scheduleType);
            onScheduleTypeChange(scheduleType);
        };

        const handleDateChange = (field) => (args) =>
            updateField(field, args.newValue);
        const handleValueChange = (field) => (nextValue) =>
            updateField(field, nextValue);

        const scheduleTypeItems = [
            {
                text: RMResx.RM_JS_ScheduleSetting_NoSchedule,
                value: SCHEDULE_TYPES.NONE,
                checked: formData.scheduleType === SCHEDULE_TYPES.NONE,
            },
            {
                text: RMResx.RM_ScheduleSetting_ConfigureSchedule,
                value: SCHEDULE_TYPES.CONFIGURE,
                checked: formData.scheduleType === SCHEDULE_TYPES.CONFIGURE,
            },
        ];

        const isConfiguring =
            formData.scheduleType === SCHEDULE_TYPES.CONFIGURE;
        const isEndingAfterOccurrences =
            formData.endType === END_TYPES.OCCURRENCES;
        const isEndingByDate = formData.endType === END_TYPES.DATE;

        const renderStartTime = () => (
            <div className="ra-scheduled-settings__row">
                <label className="ra-scheduled-settings__label">
                    Start time:
                </label>
                <R.Datepicker
                    id="scheduled-settings-start-time"
                    width={318}
                    dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                    selectedDate={formData.startTime}
                    hasTimePicker={true}
                    onChange={handleDateChange("startTime")}
                />
            </div>
        );

        const renderInterval = () => (
            <div className="ra-scheduled-settings__row">
                <label className="ra-scheduled-settings__label">
                    Interval:
                </label>
                <div className="ra-scheduled-settings__interval-controls">
                    <span>Every</span>
                    <R.Input
                        id="scheduled-settings-interval"
                        type="number"
                        hasControl
                        width={155}
                        min={1}
                        value={formData.interval}
                        onChange={handleValueChange("interval")}
                    />
                    <R.Combobox
                        id="scheduled-settings-interval-unit"
                        width={175}
                        searchable={false}
                        textField="text"
                        valueField="value"
                        value={formData.intervalUnit}
                        items={INTERVAL_OPTIONS}
                        onChange={(args) =>
                            updateField("intervalUnit", args.newValue.value)
                        }
                    />
                </div>
            </div>
        );

        const renderEndTime = () => (
            <div className="ra-scheduled-settings__row ra-scheduled-settings__end-row">
                <label className="ra-scheduled-settings__label">
                    End time:
                </label>
                <$g.RadioGroup
                    name="scheduled-settings-end"
                    value={formData.endType}
                    onChange={handleValueChange("endType")}
                >
                    <$g.RadioOption value={END_TYPES.NONE} text="No end date" />
                    <$g.RadioOption
                        value={END_TYPES.OCCURRENCES}
                        text="End after"
                    >
                        <div className="ra-scheduled-settings__occurrences-input">
                            <R.Input
                                type="number"
                                hasControl
                                width={120}
                                min={1}
                                value={formData.occurrences}
                                disabled={!isEndingAfterOccurrences}
                                onChange={handleValueChange("occurrences")}
                            />
                        </div>
                        <span>occurrences</span>
                    </$g.RadioOption>
                    <$g.RadioOption value={END_TYPES.DATE} text="End by">
                        <div className="ra-scheduled-settings__end-date">
                            <R.Datepicker
                                id="scheduled-settings-end-time"
                                width={220}
                                dateTimeFormat={RM.TimeUtil.getGlobalAuiFormat()}
                                selectedDate={formData.endTime}
                                disabled={!isEndingByDate}
                                hasTimePicker={true}
                                onChange={handleDateChange("endTime")}
                            />
                        </div>
                    </$g.RadioOption>
                </$g.RadioGroup>
            </div>
        );

        const renderScheduleBody = () => (
            <div className="ra-scheduled-settings__body">
                {renderStartTime()}
                {renderInterval()}
                {renderEndTime()}
            </div>
        );

        return (
            <div className="ra-scheduled-settings">
                <div className="ra-scheduled-settings__title">
                    {RMResx.RM_ScheduleSetting_Title}
                </div>
                <R.Radio.Group
                    name="scheduled-settings-type"
                    items={scheduleTypeItems}
                    value={formData.scheduleType}
                    onChange={handleScheduleTypeChange}
                    block={true}
                />
                {isConfiguring && renderScheduleBody()}
            </div>
        );
    },
);

NewScheduleSetting.propTypes = {
    scheduleData: PropTypes.object,
    onScheduleTypeChange: PropTypes.func,
};

export { SCHEDULE_TYPES };

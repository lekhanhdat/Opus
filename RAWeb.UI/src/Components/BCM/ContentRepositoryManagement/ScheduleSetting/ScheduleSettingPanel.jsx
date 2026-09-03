import "../../../../Less/BCM/ContentRepositoryManagement/scheduleSetting.less";
import { bindEvents, showToast } from "../../../../Utilities/CommonUtil";
import { RAFailedType, RAMessageType } from "../Common/CRMCommonUtil";

export const ScheduleConfigureType = {
    NoSchedule: "1",
    ConfigureSchedule: "2"
};

export const IntervalOptions = {
    Weeks: "1",
    Days: "2",
    Hours: "3",
    Months: "4",
};

export const WeekOrderOptions = {
    First: "100",
    Second: "101",
    Third: "102",
    Fourth: "103",
}

export const WeekDayOptions = {
    Monday: "1",
    Tuesday: "2",
    Wednesday: "3",
    Thursday: "4",
    Friday: "5",
    Saturday: "6",
    Sunday: "0",
}

export const EndTimeType = {
    NoEndTime: 0,
    EndByDate: 1,
    EndByCount: 2
};

export default class ScheduleSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.changeStatus = false;
        this.existSelfSchedule = false;
        this.defaultTimeZone = RM.TimeUtil.getGlobalTimezoneInfo();
        this.postForm = {
            Id: "1",
            StartTime: 0,
            EndTime: 0,
            StartTimeDate: new Date(),
            EndTimeDate: new Date(),
            TimeZoneId: "China Standard Time",
            Interval: 1,
            IntervalType: 0,
            DayOfMonth: 1,
            WeekType: 1,
            EndType: 0,
            OccurrencesTotal: 1,
            JobCategory: this.props.context.scheduleType,
            ProfileId: "",
            IsDaylightSaving: false
        };
        this.state = {
            selScheduleType: ScheduleConfigureType.NoSchedule,
            isShowUnderStartTime: true,
            selectedDate: new Date(),
            dateTimeFormat: RM.TimeUtil.getGlobalAuiFormat(),
            // Every option
            intervalValue: '',
            intervalSelect: IntervalOptions.Weeks,
            weekOrderSelect: "1",
            weekDaySelect: WeekDayOptions.Sunday,
            selEndTimeType: EndTimeType.NoEndTime,
            occurrencesValue: 1,
            occurInputDisabled: true,
            selectedEndDate: new Date(),
            endDatepickDisabled: true,
            selectedTimeZone: this.defaultTimeZone,
            skipAction: false,
            useDecrypt: false,
        };
        this.startTimeId = "startTimeId";
        bindEvents(this, "onScheduleTypeChange", "onStartTimeChange", "onIntervalChange", "onEndTypeChange", "onEndDateChange",
            "onSkipActionChanged", 'onOccurrencesChange','onUseDecryptChanged');
    }

    componentInit() {
        this.initScheduleData();
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    initScheduleData() {
        let schedule = null;
        this.settingData = RM.deepcopy(this.props.data);
        if (this.settingData && this.settingData.DisposeScheduleInfo) {
            schedule = this.settingData.DisposeScheduleInfo;
        }
        if (!schedule) {
            this.setState({
                selScheduleType: ScheduleConfigureType.NoSchedule,
            });
            this.existSelfSchedule = false;
            return false;
        } else {
            this.existSelfSchedule = schedule.Id != "1";
            this.postForm = schedule;
            this.setState({
                selScheduleType: ScheduleConfigureType.ConfigureSchedule,
                intervalSelect: this.postForm.IntervalType,
                selectedDate: new Date(this.postForm.StartTime),
                intervalValue: this.postForm.Interval,
                weekOrderSelect: this.postForm.DayOfMonth,
                weekDaySelect: this.postForm.WeekType,
                skipAction: this.postForm.Extentions.toLowerCase() === "true",
                useDecrypt: this.settingData.IsEnableSuperUserDecrypt
            });
            switch (this.postForm.EndType) {
                case 0:
                    this.setState({
                        selEndTimeType: EndTimeType.NoEndTime,
                        occurInputDisabled: true,
                        endDatepickDisabled: true,
                    });
                    break;
                case 1:
                    this.setState({
                        selEndTimeType: EndTimeType.EndByDate,
                        endDatepickDisabled: false,
                        occurInputDisabled: true,
                        selectedEndDate: new Date(this.postForm.EndTime),
                    });
                    $(".ra-schedule-endBy-datepicker").children().removeClass('aui-datepicker-disabled');
                    break;
                case 2:
                    this.setState({
                        selEndTimeType: EndTimeType.EndByCount,
                        occurrencesValue: this.postForm.OccurrencesTotal,
                        occurInputDisabled: false,
                        endDatepickDisabled: true,
                    });
                    break;
            }
        }
    }

    onSave(callback) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        var postUrl = "",
            postData = "";

        let postStartTime = RM.TimeUtil.getCommonDateStr(this.state.selectedDate);
        let postInterval = this.state.intervalValue - 0;
        let postOccurrencesTotal = this.state.occurrencesValue - 0;
        let postEndTime = RM.TimeUtil.getCommonDateStr(this.state.selectedEndDate);
        let postEndType = this.state.selEndTimeType;

        if (new Date(this.postForm.StartTime).getTime()
            != new Date(postStartTime).getTime()) {
            // the frist load
            this.changeStatus = true;
        }
        if (this.postForm.Interval != postInterval) {
            this.changeStatus = true;
        }
        if (this.postForm.IntervalType != postInterval) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType != postEndType) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType == postEndType &&
            this.postForm.OccurrencesTotal != postOccurrencesTotal) {
            this.changeStatus = true;
        }
        if (this.postForm.EndType == postEndType && postEndType == 1
            && new Date(this.postForm.EndTime).getTime() != new Date(postEndTime).getTime()) {
            this.changeStatus = true;
        }

        if (this.state.selScheduleType != ScheduleConfigureType.NoSchedule && this.changeStatus) {
            if (postEndType == 2) {
                this.postForm.OccurrencesTotal = this.state.occurrencesValue - 0;
            } else if (postEndType != 2) {
                this.postForm.OccurrencesTotal = 1;
            }
        }
        this.settingData.NoSchedule = this.state.selScheduleType == ScheduleConfigureType.NoSchedule;
        this.postForm.StartTime = RM.TimeUtil.getCommonDateStr(this.state.selectedDate) + ':00';
        this.postForm.StartTimeDate = this.state.selectedDate;
        this.postForm.EndTime = RM.TimeUtil.getCommonDateStr(this.state.selectedEndDate) + ':00';
        this.postForm.EndTimeDate = this.state.selectedEndDate;
        this.postForm.Interval = this.state.intervalValue - 0;
        this.postForm.OccurrencesTotal = this.state.occurrencesValue;
        this.postForm.IntervalType = this.state.intervalSelect;
        this.postForm.DayOfMonth = this.state.weekOrderSelect;
        this.postForm.WeekType = this.state.weekDaySelect;
        this.postForm.TimeZoneId = this.state.selectedTimeZone.id;
        this.postForm.IsDaylightSaving = this.state.selectedTimeZone.autoAdjustClock;
        this.postForm.EndType = this.state.selEndTimeType;
        this.settingData.SkipRemoveContentAndDestroyAction = this.state.skipAction;
        this.settingData.IsEnableSuperUserDecrypt = this.state.useDecrypt;

        if (!this.existSelfSchedule && this.state.selScheduleType != ScheduleConfigureType.NoSchedule) {
            postUrl = this.props.context.createScheduleUrl;
        } else if (this.existSelfSchedule && this.state.selScheduleType != ScheduleConfigureType.NoSchedule) {
            postUrl = this.props.context.updateScheduleUrl;
        } else if (this.existSelfSchedule && this.state.selScheduleType == ScheduleConfigureType.NoSchedule) {
            postUrl = this.props.context.deleteScheduleUrl;
        } else {
            postUrl = this.props.context.breakScheduleUrl;
            postData = JSON.stringify(this.postForm);
        }
        this.settingData.DisposeScheduleInfo = this.postForm;
        this.settingData.DisposeScheduleInfo.Extentions = this.state.skipAction.toString();
        postData = this.settingData;
        $$.loading(true);
        let option = {
            url: postUrl,
            method: "Post",
            data: postData
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            if (res) {
                let result = JSON.parse(res);
                $$.loading(false);
                if (result.MessageType == RAMessageType.Successful) {
                    callback(true);
                }else{
                    if (result.FaildType == RAFailedType.ScheduleServiceFailed) {
                        this.setState({ isShowUnderStartTime: false });
                    } else if (result.FaildType == RAFailedType.DisableRecordsManagement) {
                        callback(true);
                        showToast.error(result.ErrorMessage);
                    }
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        return this;
    }



    getScheduleTypeOptions() {
        let options = [
            { text: this.props.context.noScheduleText, value: ScheduleConfigureType.NoSchedule, checked: this.state.selScheduleType == ScheduleConfigureType.NoSchedule, disabled: this.getDisabledStatus() },
            { text: RMResx.RM_ScheduleSetting_ConfigureSchedule, value: ScheduleConfigureType.ConfigureSchedule, checked: this.state.selScheduleType == ScheduleConfigureType.ConfigureSchedule }
        ];
        return options.map(op => {
            op.title = op.text;
            return op;
        });
    }

    onScheduleTypeChange(value) {
        this.setState({
            selScheduleType: value,
        });
    }

    onStartTimeChange(args) {
        this.setState({
            selectedDate: args.newValue,
            isShowUnderStartTime: true
        });
    }

    onIntervalChange(value) {
        this.setState({
            intervalValue: value,
        });
    }

    getIntervalOptions() {
        let options = [
            { text: RMResx.RM_JS_ScheduleSetting_Weeks, value: IntervalOptions.Weeks },
            { text: RMResx.RM_JS_ScheduleSetting_Days, value: IntervalOptions.Days },
            { text: RMResx.RM_JS_ScheduleSetting_Hours, value: IntervalOptions.Hours },
        ];

        if (this.props.context.showNewIntervalSetting) {
            options.push({ text: RMResx.RM_JS_ScheduleSetting_Months, value: IntervalOptions.Months });
        }

        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.intervalSelect == op.value;
            return op;
        });
    }

    getWeekOrderOptions() {
        let options = [
            { text: RMResx.RM_JS_ScheduleSetting_WeekOrder_First, value: WeekOrderOptions.First, group: RMResx.RM_JS_ScheduleSetting_Weekday },
            { text: RMResx.RM_JS_ScheduleSetting_WeekOrder_Second, value: WeekOrderOptions.Second, group: RMResx.RM_JS_ScheduleSetting_Weekday },
            { text: RMResx.RM_JS_ScheduleSetting_WeekOrder_Third, value: WeekOrderOptions.Third, group: RMResx.RM_JS_ScheduleSetting_Weekday },
            { text: RMResx.RM_JS_ScheduleSetting_WeekOrder_Fourth, value: WeekOrderOptions.Fourth, group: RMResx.RM_JS_ScheduleSetting_Weekday },
        ];

        for (let i = 0; i < 30; i++) {
            options.push({ text: `${this.props.getOrdinalSuffix(i + 1)}`, value: `${i + 1}`, group: RMResx.RM_JS_ScheduleSetting_Monthday });
        }

        options.push({ text: RMResx.RM_JS_ScheduleSetting_LastDayOfMonth, value: "31", group: RMResx.RM_JS_ScheduleSetting_Monthday });
        
        return options.map((item) => {
            item.title = item.text;
            item.checked = this.state.weekOrderSelect == item.value;
            return item;
        });
    }

    getWeekDayOptions() {
        let options = [
            { text: RMResx.RM_JS_JN_WeeklyType_Sunday, value: WeekDayOptions.Sunday },
            { text: RMResx.RM_JS_JN_WeeklyType_Monday, value: WeekDayOptions.Monday },
            { text: RMResx.RM_JS_JN_WeeklyType_Tuesday, value: WeekDayOptions.Tuesday },
            { text: RMResx.RM_JS_JN_WeeklyType_Wednesday, value: WeekDayOptions.Wednesday },
            { text: RMResx.RM_JS_JN_WeeklyType_Thursday, value: WeekDayOptions.Thursday },
            { text: RMResx.RM_JS_JN_WeeklyType_Friday, value: WeekDayOptions.Friday },
            { text: RMResx.RM_JS_JN_WeeklyType_Saturday, value: WeekDayOptions.Saturday },
        ];
        return options.map((item) => {
            item.title = item.text;
            item.checked = this.state.weekDaySelect == item.value;
            return item;
        });
    }

    onIntSeleChange = (args) => {
        this.setState({
            intervalSelect: args.newValue.value,
            weekOrderSelect: "1",
            weekDaySelect: WeekDayOptions.Sunday,
        });
    }

    onIntervalWeekOrderChange = (args) => {
        this.setState({
            weekOrderSelect: args.newValue.value,
            weekDaySelect: WeekDayOptions.Sunday,
        })
    }

    onIntervalWeekDayChange = (args) => {
        this.setState({
            weekDaySelect: args.newValue.value,
        })
    }

    onEndTypeChange(val) {
        this.setState({
            selEndTimeType: val
        });
        if (val == EndTimeType.EndByCount) {
            this.setState({ occurInputDisabled: false });
        } else {
            this.setState({ occurInputDisabled: true }, () => {
                setTimeout(() => {
                    $$.verify(this.refOccurrencesValid && this.refOccurrencesValid.element);
                }, 500);
            });
            // $$.verify(this.refOccurrencesValid.element);
        }
        if (val == EndTimeType.EndByDate) {
            this.setState({ endDatepickDisabled: false });
            $(".ra-schedule-endBy-datepicker").children().removeClass('aui-datepicker-disabled');
        } else {
            this.setState({ endDatepickDisabled: true });
        }
    }

    onOccurrencesChange(value) {
        this.setState({
            occurrencesValue: value,
        });
    }

    onEndDateChange(args) {
        this.setState({
            selectedEndDate: args.newValue,
        });
    }

    onSkipActionChanged(args) {
        this.setState({
            skipAction: args,
        });
    }

    onUseDecryptChanged(args) {
        this.setState({
            useDecrypt: args,
        });
    }

    getDisabledStatus() {
        let disposeScheduleInfo = this.props.data.DisposeScheduleInfo;
        if (disposeScheduleInfo == null) {
            return false;
        }
        else {
            return disposeScheduleInfo.Id == "1" ? true : false;
        }
    }

    cancel() {
        return true;
    }

    verifyEndDate(value) {
        return this.state.selectedDate < value ? true : RMResx.RM_JS_ScheduleSetting_TimeError;
    }

    renderIntervalSection() {
        return (
            <>
                <R.Input id="raCrmScheduleIntervalNumIpt" type="number" hasControl width={90} min={1} max={65535}
                    value={this.state.intervalValue} onChange={this.onIntervalChange} aria={{ariaLabel:RMResx.RM_JS_ScheduleSetting_Interval}} />
                <div className="inline-block margin-left-8">
                    <R.Combobox
                        id="raCrmScheduleTimeIntervalUnit"
                        width={100}
                        searchable={false}
                        textField='text'
                        valueField='value'
                        checkedField='checked'
                        items={this.getIntervalOptions()}
                        onChange={this.onIntSeleChange}
                    />
                </div>
            </>
        )
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div>
                        <div className="margin-bottom-m">
                            <R.Radio.Group
                                name="radiogroup-schedule"
                                items={this.getScheduleTypeOptions()}
                                onChange={this.onScheduleTypeChange}
                                block={true}
                                isSeparate={false} />
                        </div>
                        <div className={"schedule-body " + (this.state.selScheduleType == ScheduleConfigureType.ConfigureSchedule ? "block" : "none")}>
                            <div className="schedule-line-top margin-bottom-m">
                                <div className="schedule-label schedule-label-line">
                                    <span id="ariaStartTime">{RMResx.RM_JS_ScheduleSetting_StratTime}:</span>
                                </div>
                                <R.Datepicker
                                    id={this.startTimeId}
                                    width={318}
                                    dateTimeFormat={this.state.dateTimeFormat}
                                    selectedDate={this.state.selectedDate}
                                    disabled={false}
                                    hasTimePicker={true}
                                    onChange={this.onStartTimeChange}
                                    aria="#ariaStartTime"
                                />
                                <R.ValidationFaker valid={this.state.isShowUnderStartTime} of={`#${this.startTimeId}`} message={RMResx.RM_JS_ScheduleSetting_TimeError} />
                            </div>
                            <div className="schedule-line-top margin-bottom-m">
                                <div className="schedule-label schedule-label-line" >
                                    {RMResx.RM_JS_ScheduleSetting_Interval}:
                                </div>
                                <R.Validation
                                    element="Input"
                                    ref={r => this.refIntervalNumValid = r}
                                    require={RMResx.RM_JS_ScheduleSetting_NumberError}
                                >
                                    {this.props.context.showNewIntervalSetting ? (
                                        <$g.I18NProvider msg={RMResx.RM_JS_ScheduleSetting_Interval_EveryMonthValue}>
                                            {this.renderIntervalSection()}
                                            {this.state.intervalSelect == IntervalOptions.Months && (
                                                <>
                                                    <span className="margin-left-8">
                                                        {RMResx.RM_JS_ScheduleSetting_On}
                                                    </span>
                                                    <div className="inline-block margin-left-8" >
                                                        <R.Combobox
                                                            id="raCrmScheduleTimeIntervalWeekOrder"
                                                            width={this.state.weekOrderSelect == "31" ? 164 : 90}
                                                            searchable={false}
                                                            textField='text'
                                                            valueField='value'
                                                            checkedField='checked'
                                                            groupField="group"
                                                            items={this.getWeekOrderOptions()}
                                                            onChange={this.onIntervalWeekOrderChange}
                                                        />
                                                    </div>
                                                    {Object.values(WeekOrderOptions).includes(this.state.weekOrderSelect.toString()) && (
                                                        <div className="inline-block margin-left-8">
                                                            <R.Combobox
                                                                id="raCrmScheduleTimeIntervalWeekDay"
                                                                width={116}
                                                                searchable={false}
                                                                textField='text'
                                                                valueField='value'
                                                                checkedField='checked'
                                                                items={this.getWeekDayOptions()}
                                                                onChange={this.onIntervalWeekDayChange}
                                                            />
                                                        </div> 
                                                    )}
                                                </>
                                            )}
                                        </$g.I18NProvider>
                                    ) : this.renderIntervalSection()}
                                </R.Validation>
                            </div>
                            <div className="schedule-line-top">
                                <div className="schedule-label">
                                    {RMResx.RM_JS_ScheduleSetting_EndTime}:
                                </div>

                                <$g.RadioGroup
                                    name="bcm-schedule-end-type"
                                    className="end-time-container"
                                    onChange={this.onEndTypeChange}
                                    value={this.state.selEndTimeType}>
                                    <$g.RadioOption value={EndTimeType.NoEndTime} text={RMResx.RM_JS_ScheduleSetting_NoEndDate} />
                                    <$g.RadioOption value={EndTimeType.EndByCount} text={RMResx.RM_JS_ScheduleSetting_EndAfter}>
                                        {this.state.occurInputDisabled && <div>
                                            <div className="margin-left-8" style={{ width: "130px", display: "inline-block" }}>
                                                <R.Input id="raCrmScheduleOccurrencesNumIpt" type="number" hasControl width={130} min={1} max={65535}
                                                    value={this.state.occurrencesValue} disabled={this.state.occurInputDisabled}
                                                    onChange={this.onOccurrencesChange} aria={{ariaLabel:RMResx.RM_JS_ScheduleSetting_EndTime}}/>
                                            </div>
                                            <span className="schedule-label-line margin-left-8">
                                                {RMResx.RM_JS_ScheduleSetting_Occurrences}
                                            </span>
                                        </div>}
                                        {!this.state.occurInputDisabled && <R.Validation
                                            element="Input"
                                            require={RMResx.RM_JS_ScheduleSetting_NumberError}
                                            ref={r => this.refOccurrencesValid = r}
                                        >
                                            <div className="margin-left-8" style={{ width: "130px", display: "inline-block" }}>
                                                <R.Input id="raCrmScheduleOccurrencesNumIpt" type="number" hasControl width={130} min={1} max={65535}
                                                    value={this.state.occurrencesValue} disabled={this.state.occurInputDisabled}
                                                    onChange={this.onOccurrencesChange} aria={{ariaLabel:RMResx.RM_JS_ScheduleSetting_EndTime}} />
                                            </div>
                                            <span className="schedule-label-line margin-left-8">
                                                {RMResx.RM_JS_ScheduleSetting_Occurrences}
                                            </span>
                                        </R.Validation>}
                                    </$g.RadioOption>
                                    <$g.RadioOption value={EndTimeType.EndByDate} text={RMResx.RM_JS_ScheduleSetting_EndByDate}>
                                        <R.Validation
                                            element="Datepicker"
                                            require
                                            rules={{
                                                customVerify: this.verifyEndDate.bind(this),
                                            }}
                                        >
                                            <div className='ra-schedule-endBy-datepicker'>
                                                <R.Datepicker
                                                    id="raCrmScheduleEndDate"
                                                    width={220}
                                                    dateTimeFormat={this.state.dateTimeFormat}
                                                    selectedDate={this.state.selectedEndDate}
                                                    disabled={this.state.endDatepickDisabled}
                                                    hasTimePicker={true}
                                                    onChange={this.onEndDateChange} />
                                            </div>
                                        </R.Validation>
                                    </$g.RadioOption>
                                </$g.RadioGroup>
                            </div>
                            {this.props.context.showScheduleSkipAction && <div className="ra-inline-middle margin-bottom-m">
                                <R.Checkbox
                                    id="raCrmScheduleSkipRemoveActionChk"
                                    text={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                                    title={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                                    checked={this.state.skipAction}
                                    onChange={this.onSkipActionChanged}
                                />
                            </div>}
                            {this.props.context.showScheduleUseDecrypt && <div className="ra-inline-middle margin-bottom-m">
                                <R.Checkbox
                                    id="raCrmScheduleUseDecryptActionChk"
                                    text={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                                    title={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                                    checked={this.state.useDecrypt}
                                    onChange={this.onUseDecryptChanged}
                                />
                            </div>}
                        </div>
                    </div>
                </div>
            </R.Validation>
        </div>;
    }
}
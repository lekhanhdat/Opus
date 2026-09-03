import { DateConditions } from '../../Constants';
let idCount = 0;
export default class HSFilteDateAndTime extends R.Component {
    idAttr = true;
    componentCreate() {
        this.dateRangePickerId = "rangePicker" + idCount++;
        this.dateTimeFormat = RM.TimeUtil.getGlobalAuiFormat();
        this.conditionOptions = [
            { name: RMResx.RM_JS_BCM_Explorer_DueDateBefore_Title, value: DateConditions.Before, checked: true },
            { name: RMResx.RM_JS_BCM_Explorer_DueDateAfter_Title, value: DateConditions.After, checked: false },
            { name: RMResx.RM_JS_BCM_Explorer_DueDateRange_Title, value: DateConditions.FromTo, checked: false },
            { name: RMResx.RM_JS_BCM_Explorer_DueDatePending_Title, value: DateConditions.Pending, checked: false },
            { name: RMResx.RM_JS_BCM_Explorer_DueDateNextJob_Title, value: DateConditions.NextJob, checked: false },
        ];
        this.selectedDateAndTimeInfo = {
            Condition: DateConditions.Before
        };
        this.state = {
            conditionOptions: this.getConditionOptions(),
            selectedBeforeDate: null,
            selectedAfterDate: null,
            selectedFromToDate: {},
            conditionValue: DateConditions.Before,

        };
    }

    componentReceive(data, type) {
        let selectedDateAndTime = data.Value || { Condition: DateConditions.Before };
        this.echoDateAndTime(selectedDateAndTime);
    }

    getConditionOptions() {
        let conditionOptions = this.conditionOptions;
        if (this.props.onlyShowDateAndTime) {
            conditionOptions = this.conditionOptions.splice(0, 3);
        }
        if(this.props.otherConditions){
            conditionOptions = [...conditionOptions, ...this.props.otherConditions];
        }
        return conditionOptions;
    }

    onDateConditionChange = (args) => {
        let value = args.newValue.value;
        this.setState({
            selectedBeforeDate: null,
            selectedAfterDate: null,
            selectedFromToDate: {},
            conditionValue: value,
        });
        this.selectedDateAndTimeInfo.Condition = value;
        if(value == DateConditions.Pending 
            || value == DateConditions.NextJob 
            || value == DateConditions.Overdue
            || value == DateConditions.NotSpecified
        ){
            this.props.onChange({Condition: value});
        }else{
            this.props.onChange(null);
        }
    }

    onSelectedDateAndTime = (args) => {
        let selectedDateAndTimeValue = this.selectedDateAndTimeInfo.Condition;
        let selectedDateAndTime = null;
        this.selectedDateAndTimeInfo = {};
        switch (selectedDateAndTimeValue.toString()) {
            case DateConditions.Before:
                this.selectedDateAndTimeInfo.Condition = DateConditions.Before;
                this.selectedDateAndTimeInfo.Value1 = RM.TimeUtil.getCommonDateStr(args.newValue);
                selectedDateAndTime = this.selectedDateAndTimeInfo;
                this.setState({ selectedBeforeDate: args.newValue });
                break;
            case DateConditions.After:
                this.selectedDateAndTimeInfo.Condition = DateConditions.After;
                this.selectedDateAndTimeInfo.Value2 = RM.TimeUtil.getCommonDateStr(args.newValue);
                selectedDateAndTime = this.selectedDateAndTimeInfo;
                this.setState({ selectedAfterDate: args.newValue });
                break;
            case DateConditions.FromTo:
                this.selectedDateAndTimeInfo.Condition = DateConditions.FromTo;
                this.selectedDateAndTimeInfo.Value1 = RM.TimeUtil.getCommonDateStr(args.newValue.start);
                this.selectedDateAndTimeInfo.Value2 = RM.TimeUtil.getCommonDateStr(args.newValue.end);
                selectedDateAndTime = this.selectedDateAndTimeInfo;
                this.setState({ selectedFromToDate: { start: args.newValue.start, end: args.newValue.end } });
                if (!args.isValid) {
                    selectedDateAndTime = null;
                }
                break;
        }
        this.props.onChange(selectedDateAndTime);
    }

    echoDateAndTime = (selectedDateAndTime) => {
        let conditionValue = selectedDateAndTime.Condition;
        let value1 = selectedDateAndTime.Value1 ? new Date(selectedDateAndTime.Value1) : null;
        let value2 = selectedDateAndTime.Value2 ? new Date(selectedDateAndTime.Value2) : null;
        switch (conditionValue.toString()) {
            case DateConditions.Before:
                this.setState({ selectedBeforeDate: value1 });
                break;
            case DateConditions.After:
                this.setState({ selectedAfterDate: value2 });
                break;
            case DateConditions.FromTo:
                this.setState({ selectedFromToDate: { start: value1, end: value2 } });
                break;
        }
        for (let item of this.state.conditionOptions) {
            item.checked = item.value == conditionValue;
        }

        this.selectedDateAndTimeInfo = RM.deepcopy(selectedDateAndTime);
        this.setState({
            conditionValue: conditionValue,
            conditionOptions: RM.deepcopy(this.state.conditionOptions)
        });
    }

    renderBeforeTime() {
        return <div className="margin-left-m flex-1">
            <R.Validation element="Datepicker" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                <R.Datepicker
                    width={"100%"}
                    height={40}
                    dateTimeFormat={this.dateTimeFormat}
                    selectedDate={this.state.selectedBeforeDate}
                    hasTimePicker={true}
                    onChange={this.onSelectedDateAndTime}
                />
            </R.Validation>
        </div>;
    }

    renderAfterTime() {
        return <div className="margin-left-m flex-1">
            <R.Validation element="Datepicker" require={RMResx.RM_HS_NoSearchColValValidMsg}>
                <R.Datepicker
                    width={"100%"}
                    height={40}
                    dateTimeFormat={this.dateTimeFormat}
                    selectedDate={this.state.selectedAfterDate}
                    hasTimePicker={true}
                    onChange={this.onSelectedDateAndTime}
                />
            </R.Validation>
        </div>;
    }

    renderFromToTime() {
        return <div className="margin-left-m flex-1">
            <$g.DateAndTimeRangePicker
                startPickerInfo={{selectedDate: this.state.selectedFromToDate.start}}
                endPickerInfo={{selectedDate: this.state.selectedFromToDate.end}}
                onChange={this.onSelectedDateAndTime}
            /> 
        </ div>;
    }

    renderDateCondition() {
        return <div className="flex-1">
            <R.Combobox
                searchable={false}
                height={40}
                width={"100%"}
                textField='name'
                valueField='value'
                checkedField='checked'
                items={this.state.conditionOptions}
                onChange={this.onDateConditionChange}
            />
        </div>;
    }

    renderDateAndTime() {
        let selectedConditionVal = this.state.conditionValue;
        switch (selectedConditionVal.toString()) {
            case DateConditions.Before:
                return this.renderBeforeTime();
            case DateConditions.After:
                return this.renderAfterTime();
            case DateConditions.FromTo:
                return this.renderFromToTime();
        }
    }

    render() {
        return <div className="flex-start">
            {this.renderDateCondition()}
            {this.renderDateAndTime()}
        </div>;
    }
}


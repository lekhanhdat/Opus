import { showToast } from "../../../../Utilities/CommonUtil";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import ScheduleSettingPanel, { EndTimeType, IntervalOptions, WeekDayOptions, WeekOrderOptions } from "./ScheduleSettingPanel";

export default class ScheduleSettingComponent extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.settings = { NoSchedule: true };
        this.state = {
            startTime: '',
            endTime: '',
            intervalValue: '',
            skipAction: false,
            isEnableSuperUserDecrypt: false,
            isShowScheduleSettingsPanel: { show: false },
            scheduleSettingInfo: {},
        };
        this.scheduleSettingComponent = "scheduleSettingPanel";
    }

    componentInit() {
        //this.loadScheduleData();
    }

    componentReceive(type, args) {
        switch (type) {
            case "scheduleData":
                this.data = args;
                this.initScheduleData();
                this.setState({ scheduleSettingInfo: args });
                break;
        }
    }

    initScheduleData() {
        // let schedule = this.data.DisposeScheduleInfo;
        let schedule = null;
        if (this.data == null) {
            return;
        } else {
            schedule = this.data.DisposeScheduleInfo;
        }
        if (schedule) {
            this.settings = schedule;
        } else {
            this.settings = { NoSchedule: true };
        }
        this.initDetails();
    }

    initDetails() {
        if (this.settings.NoSchedule) {
            this.setState({
                startTime: '',
                endTime: '',
                intervalValue: '',
                skipAction: false,
                isEnableSuperUserDecrypt: false
            });
        } else {
            let timeZone = RM.TimeUtil.getTimezoneInfo(this.settings.TimeZoneId, this.settings.IsDaylightSaving);
            let intervalValue = `${this.settings.Interval} ${this.getIntervalUnit()}`;
            
            // if (this.props.context.showNewIntervalSetting) {
            //     intervalValue = `${RMResx.RM_JS_ScheduleSetting_Interval_Every} ${this.settings.Interval} ${this.getIntervalUnit()}`;
            // }

            this.setState({
                startTime: RM.TimeUtil.dateToString(new Date(this.settings.StartTime), timeZone, true),
                intervalValue,
                endTime: this.getEndTimeDisplayStr(timeZone),
                skipAction: this.data.DisposeScheduleInfo.Extentions.toLowerCase() === "true",
                isEnableSuperUserDecrypt: this.data.IsEnableSuperUserDecrypt
            });
        }
    }

    getOrdinalSuffix(i) {
        if (i % 10 === 1 && i % 100 !== 11) return `${i}${RMResx.RM_JS_ScheduleSetting_Number_ST}`;
        if (i % 10 === 2 && i % 100 !== 12) return `${i}${RMResx.RM_JS_ScheduleSetting_Number_ND}`;
        if (i % 10 === 3 && i % 100 !== 13) return `${i}${RMResx.RM_JS_ScheduleSetting_Number_RD}`;
        return `${i}${RMResx.RM_JS_ScheduleSetting_Number_TH}`;
    }

    getIntervalUnit() {
        if (this.settings.IntervalType == IntervalOptions.Weeks) {
            return RMResx.RM_JS_ScheduleSetting_Weeks;
        } else if (this.settings.IntervalType == IntervalOptions.Days) {
            return RMResx.RM_JS_ScheduleSetting_Days;
        } else if (this.settings.IntervalType == IntervalOptions.Hours) {
            return RMResx.RM_JS_ScheduleSetting_Hours;
        } else {
            const dayOfMonthMapping = {
                [WeekOrderOptions.First]: RMResx.RM_JS_ScheduleSetting_WeekOrder_First,
                [WeekOrderOptions.Second]: RMResx.RM_JS_ScheduleSetting_WeekOrder_Second,
                [WeekOrderOptions.Third]: RMResx.RM_JS_ScheduleSetting_WeekOrder_Third,
                [WeekOrderOptions.Fourth]: RMResx.RM_JS_ScheduleSetting_WeekOrder_Fourth,
            }

            const weekTypeMapping = {
                [WeekDayOptions.Monday]: RMResx.RM_JS_JN_WeeklyType_Monday,
                [WeekDayOptions.Tuesday]: RMResx.RM_JS_JN_WeeklyType_Tuesday,
                [WeekDayOptions.Wednesday]: RMResx.RM_JS_JN_WeeklyType_Wednesday,
                [WeekDayOptions.Thursday]: RMResx.RM_JS_JN_WeeklyType_Thursday,
                [WeekDayOptions.Friday]: RMResx.RM_JS_JN_WeeklyType_Friday,
                [WeekDayOptions.Saturday]: RMResx.RM_JS_JN_WeeklyType_Saturday,
                [WeekDayOptions.Sunday]: RMResx.RM_JS_JN_WeeklyType_Sunday,
            }

            if (Object.values(WeekOrderOptions).includes(this.settings.DayOfMonth.toString())) {
                return `${RMResx.RM_JS_ScheduleSetting_Months} ${RMResx.RM_JS_ScheduleSetting_On} ${dayOfMonthMapping[this.settings.DayOfMonth]} ${weekTypeMapping[this.settings.WeekType]}`;
            }

            const dayOfMonth = this.settings.DayOfMonth === 31 ? RMResx.RM_JS_ScheduleSetting_LastDayOfMonth : this.getOrdinalSuffix(this.settings.DayOfMonth);
            return `${RMResx.RM_JS_ScheduleSetting_Months} ${RMResx.RM_JS_ScheduleSetting_On} ${dayOfMonth}`;
        }
    }

    getEndTimeDisplayStr(timeZone) {
        if (this.settings.EndType == EndTimeType.EndByDate) {
            return RM.TimeUtil.dateToString(new Date(this.settings.EndTime), timeZone, true);
        } else if (this.settings.EndType == EndTimeType.EndByCount) {
            return RMResx.RM_JS_ScheduleSetting_EndAfter + ' ' + this.settings.OccurrencesTotal + ' ' + RMResx.RM_JS_ScheduleSetting_Occurrences;
        } else {
            return RMResx.RM_JS_ScheduleSetting_NoEndDate;
        }
    }

    showScheduleSettingsClick = (e) => {
        if (!(this.props.checkMissingConfig && this.props.checkMissingConfig())) {
            this.setState({ isShowScheduleSettingsPanel: { show: true } });
        }
    }

    isShowEditBtnForArchiver = (e) => {
        let isShow = false;
        if (this.data) {
            if (CRMCommonUtil.isGroup(this.data)) {
                if (this.data.Rules) {
                    isShow = true;
                }
            } else {
                if (this.data.IsCustomSetting) {
                    if (this.data.Rules) {
                        isShow = true;
                    }
                }
            }
        }
        return isShow;
    }

    saveColumnSettings = (e) => {
        this.dispatch(this.scheduleSettingComponent, 'onSave', (success, data) => {
            if (success) {
                this.props.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
                this.setState({ isShowScheduleSettingsPanel: { show: false } });
            }
            this.initDetails();
        });
        return false;
    }

    cancelColumnSettings = () => {
        this.setState({ isShowScheduleSettingsPanel: { show: false } });
    }

    render() {
        let scheduleSettingInfo = this.state.scheduleSettingInfo;
        let isShowEditBtn = (this.props.context.scheduleType == 37 || this.props.context.scheduleType == 38 || this.props.context.scheduleType == 70) ? this.isShowEditBtnForArchiver() : !this.props.disabled;

        return <div id={this.props.id}>
            <R.Expander
                status={false}
                groupName="title">
                <div className="ra-crm-expander">
                    <div data-tooltip="ifneed" className="ra-expander-fontStyle">{this.props.context.scheduleSettingTitle}</div>
                    {isShowEditBtn && <R.Scope>
                        <R.Button
                            id="raCrmScheculeEditBtn"
                            type="bald"
                            icon="fia-edit"
                            title={this.props.context.scheduleSettingTitle}
                            tooltip={RMResx.RM_JS_SPS_Settings_EditSettings}
                            onClick={this.showScheduleSettingsClick} />
                    </R.Scope>}
                </div>
                <div>
                    {scheduleSettingInfo && <div>
                        <$g.DetailList>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_ScheduleSetting_StratTime}>
                                    <span tabIndex="0">{this.state.startTime}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_ScheduleSetting_Interval}>
                                    {this.props.context.showNewIntervalSetting && this.state.intervalValue ? (
                                        <$g.I18NProvider msg={RMResx.RM_JS_ScheduleSetting_Interval_EveryDisplay}>
                                            <span tabIndex="0">{this.state.intervalValue}</span>
                                        </$g.I18NProvider>
                                    ) : (
                                        <span tabIndex="0">{this.state.intervalValue}</span>
                                    )}
                                </$g.DetailCell>
                            </$g.DetailRow>
                            <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_ScheduleSetting_EndTime}>
                                    <span tabIndex="0">{this.state.endTime}</span>
                                </$g.DetailCell>
                            </$g.DetailRow>
                            {this.props.context.showScheduleSkipAction && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}>
                                    {(!this.settings.NoSchedule) && <span tabIndex="0">{this.state.skipAction ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>}
                            {this.props.context.showScheduleUseDecrypt && <$g.DetailRow>
                                <$g.DetailCell label={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}>
                                    {(!this.settings.NoSchedule) && <span tabIndex="0">{this.state.isEnableSuperUserDecrypt ? RMResx.RM_JS_Common_Yes : RMResx.RM_JS_Common_No}</span>}
                                </$g.DetailCell>
                            </$g.DetailRow>}
                        </$g.DetailList>
                    </div>}
                </div>
            </R.Expander>

            <R.Panel
                header={RMResx.RM_JS_SPS_EditSetting}
                size={670}
                status={this.state.isShowScheduleSettingsPanel}
                destroy={true}
            >
                <div className="br" slot="header">
                    <span className="ra-setting-panel-header">{this.props.context.scheduleSettingTitle}</span>
                </div>
                <ScheduleSettingPanel
                    id={this.scheduleSettingComponent}
                    context={this.props.context}
                    data={this.data}
                    getOrdinalSuffix={this.getOrdinalSuffix}
                ></ScheduleSettingPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelColumnSettings} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveColumnSettings} />
                </>
            </R.Panel>
        </div>;
    }
}
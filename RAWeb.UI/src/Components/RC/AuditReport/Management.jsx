import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { AuditRangeTypes } from "../Constants";
import { bindEvents, getRequestVerificationToken } from "../../../Utilities/CommonUtil";
import Diagram from "./Diagram";
import Details from "./Details";
import "../../../Less/RC/auditReport.less";
import { addTelemetryRecord } from '../../../Utilities/TelemetryUtil';
import { TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import { Messagebox } from "../../Common/Messagebox";

export default class AuditReportManagement extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        bindEvents(this, "exportReport", "onDateRangesNotCustomChange", "onDateRangeOfCustomChange",
            "onSelectTime", "setDateRangeText", "handleTabControlChanged", "chartClick");
        this.timeInfo = RM.TimeUtil.getTodayStartEndTime();
        this.viewData = {};
        this.rangeCustomId = AuditRangeTypes["Custom"];
        this.isTabChangeByChart = false;
        this.state = {
            tipStatus: { show: false },
            tipType: "success",
            tipMsg: "",
            dateRangesNotCustom: this.getDateRangesNotCustom(),
            dateRangeOfCustomChecked: false,
            selectedRangeType: AuditRangeTypes["5D"],
            timeInfo: this.timeInfo,
            tabControlIdx: 0,
            dateRangeText: '',
            enableDates: {
                end: new Date(),
            }
        };
    }

    componentInit() {
        addTelemetryRecord(TelemetryModule.ReportCenter, TelemetryEventType.ViewAuditReport);
    }

    routerTo(routerUrl, param) {
        this.props.history.push({
            pathname: routerUrl,
            state: param
        });
    }

    getDateRangesNotCustom() {
        return [
            {
                text: RMResx.RM_RC_Audit_Range_5D,
                title: RMResx.RM_RC_Audit_Range_5D,
                value: AuditRangeTypes["5D"],
                checked: true
            },
            {
                text: RMResx.RM_RC_Audit_Range_1M,
                title: RMResx.RM_RC_Audit_Range_1M,
                value: AuditRangeTypes["1M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_3M,
                title: RMResx.RM_RC_Audit_Range_3M,
                value: AuditRangeTypes["3M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_6M,
                title: RMResx.RM_RC_Audit_Range_6M,
                value: AuditRangeTypes["6M"],
                checked: false
            },
            {
                text: RMResx.RM_RC_Audit_Range_Custom,
                title: RMResx.RM_RC_Audit_Range_Custom,
                value: this.rangeCustomId,
                checked: false
            }
        ];
    }

    onExportReportBtn = () => {
        Messagebox({ content: RMResx.RM_JS_Common_ExportMsg, actionFun: this.exportReport });
    }

    exportReport() {
        let requestVerificationToken = getRequestVerificationToken();
        let divElement = document.getElementById("downloadDiv");
        let downloadUrl = "/api/AuditApi/DownLoadReport";

        let param = {
            Range: this.state.selectedRangeType,
            StartTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.start),
            EndTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.end),
        };

        ReactDOM.render(
            <form action={downloadUrl} method='post'>
                <input name='RequestVerificationToken' type='text' value={requestVerificationToken} readOnly />
                <input name='datetimeRange' type='text' value={JSON.stringify(param)} readOnly />
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
    }

    getAuiFormatForDate(time) {
        let date = null;
        let timeArr = time.split(' ');
        if (timeArr[1].includes(',')) {
            date = timeArr[0] + ' ' + timeArr[1];
        } else {
            date = timeArr[0];
        }
        return date;
    }

    getDateRangeText = (param) => {
        if (this.state.tabControlIdx === 1) {
            $$.loading(true);
            const option = {
                url: '/api/AuditApi/GetChartInfo',
                data: param,
            };
            fetchUtility(option).then((data) => {
                $$.loading(false);
                const dateRangText = data.Start + ' - ' + data.End;
                this.setDateRangeText(dateRangText);
            });
        } else {
            this.dispatch('auditReportDiagram', param);
        }
    }

    onDateRangesNotCustomChange(value) {
        let dateRangesNotCustom = this.setDateRangesNotCustom(value);
        let showDatePicker = false;
        let dateRangeOfCustomChecked = false;
        if(value === this.rangeCustomId) {
            showDatePicker = true;
            dateRangeOfCustomChecked = true;
        }
        this.setState({
            selectedRangeType: value,
            dateRangesNotCustom: dateRangesNotCustom,
            dateRangeOfCustomChecked: dateRangeOfCustomChecked,
            showDatePicker: showDatePicker,
        });

        if(value === this.rangeCustomId) {
            this.onSelectTime();
            return;
        }

        if (this.viewData.viewBy == 0) {
            this.viewData.viewByValue = null;
        }
        let param = {
            Range: value,
            viewBy: this.viewData.viewBy,
            viewByValue: this.viewData.viewByValue
        };
        const data = {
            Range: value,
            ViewBy: 0,
        };
        this.getDateRangeText(data);
        this.dispatch('auditReportDetail', "selectTime", param);
    }

    onDateRangeOfCustomChange(value) {
        let dateRangesNotCustom = this.setDateRangesNotCustom(this.rangeCustomId);
        this.setState({
            selectedRangeType: value,
            dateRangesNotCustom: dateRangesNotCustom,
            dateRangeOfCustomChecked: true,
            showDatePicker: true
        });
    }

    setDateRangesNotCustom(value) {
        let dateRangesNotCustom = RM.deepcopy(this.state.dateRangesNotCustom);
        for (let item of dateRangesNotCustom) {
            if (value === item.value) {
                item.checked = true;
            } else {
                item.checked = false;
            }
        }
        return dateRangesNotCustom;
    }

    onSelectTime(args) {
        this.timeInfo = args ? args.newValue : this.timeInfo;
        this.setState({ timeInfo: this.timeInfo });
        if (this.viewData.viewBy == 0) {
            this.viewData.viewByValue = null;
        }
        let param = {
            Range: this.rangeCustomId,
            StartTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.start),
            EndTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.end),
            viewBy: this.viewData.viewBy,
            viewByValue: this.viewData.viewByValue
        };
        this.getDateRangeText(param);
        this.dispatch('auditReportDetail', "selectTime", param);
    }

    setDateRangeText(dateRangeText) {
        this.setState({
            dateRangeText: dateRangeText
        });
    }

    handleTabControlChanged(index) {
        let param = {
            Range: this.state.selectedRangeType,
            StartTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.start),
            EndTime: RM.TimeUtil.getCommonDateStr(this.timeInfo.end),
        };
        if (this.isTabChangeByChart == true) {
            param.viewBy = this.viewData.viewBy;
            param.viewByValue = this.viewData.viewByValue;
        } else {
            param.viewBy = 0;
            param.viewByValue = null;
        }
        this.setState({ tabControlIdx: index }, () => {
            setTimeout(() => {
                if (index == 1) {
                    this.dispatch('auditReportDetail', "selectTime", param);
                } else {
                    this.dispatch('auditReportDiagram', param, 'resetChart');
                    this.dispatch('auditReportDiagram', param);
                }
            }, 100);

        });
        this.isTabChangeByChart = false;
    }

    chartClick(viewBy, viewByValue) {
        this.viewData = {
            viewBy: viewBy,
            viewByValue: viewByValue
        };
        this.isTabChangeByChart = true;
        // this.setState({ tabControlIdx: 1 });
        this.handleTabControlChanged(1);
    }

    renderReportDesc() {
        return <div className="introduction">
            <div className="introduction-title">
                <span tabIndex='0'>{RMResx.RM_Report_SectionTitle_Introduction}</span>
            </div>
            <div className="introduction-headline"></div>
            <div className="introduction-content">
                <span tabIndex='0'>{RMResx.RM_RC_Audit_AuditDes}</span>
            </div>
        </div>;
    }

    renderDateRangDatePicker() {
        if (this.state.showDatePicker) {
            let timeInfo = this.state.timeInfo;
            if (timeInfo && timeInfo.start != null && timeInfo.end != null) {
                return <R.Rangepicker
                    selectedDate={timeInfo}
                    data-part="vtWidget"
                    width={240}
                    dateTimeFormat={RM.TimeSettingModel.DateFormat}
                    enableDates={this.state.enableDates}
                    onChange={this.onSelectTime}
                />;
            }
        }
    }

    render() {
        return (
            <div className="reco-admin-audit-report-wrapper" id={this.props.id}>
                <section className="reco-admin-audit-report-header">
                    <$g.SiteMap
                        data={[SiteMapLinks.RC_AuditReportManagement]} />
                    <div className="reco-admin-audit-report-header-message">
                        <R.Messagebar
                            message={this.state.tipMsg}
                            classify={this.state.tipType}
                            onClose={this.hideMessageTip}
                            status={{ show: this.state.tipStatus.show }}
                        />
                    </div>
                    <R.Button primary={true} classify="theme" text={RMResx.RM_RC_Audit_AuditExp} onClick={this.onExportReportBtn} />
                </section>

                <section className="reco-admin-audit-report-card">
                    <div className="reco-admin-audit-report-form">
                        <div className="reco-admin-audit-report-title" tabIndex="0">
                            {RMResx.RM_Report_SectionTitle_Settings}
                        </div>
                        <div className="reco-admin-audit-report-radio-group">
                            <R.Radio.Group
                                block
                                name="dateRangeOfNotCustom"
                                items={this.state.dateRangesNotCustom}
                                onChange={this.onDateRangesNotCustomChange}
                            />
                        </div>
                        {/* <div className="reco-admin-audit-report-custom-radio">
                            <R.Radio
                                name='dateRangeOfCustom'
                                text={RMResx.RM_RC_Audit_Range_Custom}
                                value={this.rangeCustomId}
                                checked={this.state.dateRangeOfCustomChecked}
                                onChange={this.onDateRangeOfCustomChange} />
                        </div> */}
                        <div className='reco-admin-audit-report-custom-datepicker'>
                            {this.renderDateRangDatePicker()}
                        </div>
                    </div>
                    <div className="reco-admin-audit-report-tips">
                        <div className="reco-admin-audit-report-tips-header">
                            <span className="reco-admin-audit-report-tips-icon fia-light">
                            </span>
                            <span className="reco-admin-audit-report-tips-header-title" tabIndex="0">
                                {RMResx.RM_Report_SectionTitle_Introduction}
                            </span>
                        </div>
                        <div className="reco-admin-audit-report-tips-content" tabIndex="0">
                            {RMResx.RM_RC_Audit_AuditDes}
                        </div>
                    </div>
                </section>

                <section className="reco-admin-audit-report-content">
                    <div className="reco-admin-audit-report-content-header">
                        <div className="reco-title" tabIndex="0">
                            {RMResx.RM_RC_AuditReportingDetail}
                        </div>
                        <div className="reco-datarange" tabIndex="0">
                            {this.state.dateRangeText}
                        </div>
                    </div>
                    <R.Tabcontrol
                        active={this.state.tabControlIdx}
                        onChange={this.handleTabControlChanged}
                        destroy={true}
                    >
                        <R.TabPanel tab={RMResx.RM_RC_Audit_ChartTab} key={0}>
                            <Diagram
                                id="auditReportDiagram"
                                setDateRangeText={this.setDateRangeText}
                                onClick={this.chartClick}
                            ></Diagram>
                        </R.TabPanel>
                        <R.TabPanel tab={RMResx.RM_RC_Audit_TableTab} key={1}>
                            <Details id='auditReportDetail'></Details>
                        </R.TabPanel>
                    </R.Tabcontrol>
                </section>
                <div id='downloadDiv' style={{ display: "none" }} />
            </div>
        );
    }
}

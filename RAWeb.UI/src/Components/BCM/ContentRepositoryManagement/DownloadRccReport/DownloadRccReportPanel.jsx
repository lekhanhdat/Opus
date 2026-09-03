import { get } from "lodash";
import { bindEvents, showToast } from "../../../../Utilities/CommonUtil";

const TimeRangeTypes = {
    "Custom": 0,
    "3Months": 1,
    "6Months": 2,
    "1Year": 3,
};

export default class DownloadRccReportPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        bindEvents(this, "onDateRangesChange", "onSelectTime");
        this.state = {
            timeRangeType: TimeRangeTypes["3Months"],
            timeInfo: {},
            dateRangeOptions: this.getDefaultTimeInfo(),
            isDateRangeRequired: false,
        };
    }

    componentReceive(type, args, callback) {
        if (type === "onDownload") {
            this.downloadRccReport(args, callback);
        }
    }

    getDefaultTimeInfo() {
        return [
            { text: RMResx.RM_FS_DateRangeCustom_3M, title: RMResx.RM_FS_DateRangeCustom_3M, value: TimeRangeTypes["3Months"], checked: true },
            { text: RMResx.RM_FS_DateRangeCustom_6M, title: RMResx.RM_FS_DateRangeCustom_6M, value: TimeRangeTypes["6Months"], checked: false },
            { text: RMResx.RM_FS_DateRangeCustom_1Y, title: RMResx.RM_FS_DateRangeCustom_1Y, value: TimeRangeTypes["1Year"], checked: false },
            { text: RMResx.RM_FS_DateRangeCustom_Custom, title: RMResx.RM_FS_DateRangeCustom_Custom, value: TimeRangeTypes["Custom"], checked: false },
        ];
    }

    downloadRccReport(nodeData, callback) {
        const { Id, FullPath, Name, Level, ConnectionId, ConnGroupId } = nodeData;
        if (this.state.timeRangeType === TimeRangeTypes["Custom"] && (!this.state.timeInfo.start || !this.state.timeInfo.end)) {
            this.setState({ isDateRangeRequired: true });
            return;
        }
        $$.loading(true);
        const payload = {
            Nodes: [{
                Id,
                FullPath,
                Name,
            }],
            ConnGroupId,
            JPMCId: ConnectionId,
            Level,
            TimeRange: {
                PresetType: this.state.timeRangeType,
                StartDate: RM.TimeUtil.getCommonDateStr(this.state.timeInfo.start),
                EndDate: RM.TimeUtil.getCommonDateStr(this.state.timeInfo.end)
            }
        }
        const option = {
            url: '/api/BCMAdminSettingApi/DownloadRCCReport',
            method: "POST",
            data: payload,
        };
        fetchUtility(option).then((res) => {
            if (res.MessageType === 0) {
                let content = 
                    <$g.I18NProvider msg={RMResx.RM_JS_Template_ImportSuccess}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                showToast.success(content);
            } else {
                showToast.error(res.ErrorMessage);
            }
            callback();
            $$.loading(false);
        }).catch(() => {
            $$.loading(false);
        });
    }

    onDateRangesChange = (value) => {
        this.setState({ timeRangeType: value, timeInfo: {}, isDateRangeRequired: false });
    }

    onSelectTime(args) {
        if (!args) return;
        this.setState({ timeInfo: args.newValue, isDateRangeRequired: false });
    }

    render() {
        const isCustomRange = this.state.timeRangeType === TimeRangeTypes["Custom"];
        return (
            <div id={this.props.id}>
                <div className="margin-bottom-s font-semibold fontsize-13" tabIndex="0">
                    {RMResx.RM_FS_DateRangeCustom_Title}
                </div>
                <div className="margin-bottom-m">
                    <R.Radio.Group
                        block
                        name="dateRange"
                        items={this.state.dateRangeOptions}
                        onChange={this.onDateRangesChange}
                        tabIndex="0"
                    />
                </div>
                <div>
                    {isCustomRange && (
                        <>
                            <R.Rangepicker
                                selectedDate={this.state.timeInfo}
                                data-part="vtWidget"
                                width={400}
                                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                                onChange={this.onSelectTime}
                                tabIndex="0"
                            />
                            <$g.ValidationMsg show={this.state.isDateRangeRequired}>
                                {RMResx.RM_FS_DateRangeCustom_ValidateMessage}
                            </$g.ValidationMsg>
                        </>
                    )}
                </div>
            </div>
        );
    }
}

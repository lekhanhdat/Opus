import { bindEvents } from "../../../Utilities/CommonUtil";
import LineCharTooltipTemplate from './CharTooltipTemplate';
import { AuditReportFilterType, AuditRangeTypes } from "../Constants";
import "../../../Less/RC/auditReport.less";

export default class Diagram extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        bindEvents(this, "filterTypeChange", "chartDateRangeChange", "showBarChart", "showLineChart",
            "onTabChart", "chartClick");

        this.isRender = false;
        this.originChartData = [];
        this.selectedFilterTypeId = 0;
        this.selectedFilterTypeName = RMResx.RM_JS_RC_Audit_ViewBy_Option_Time;
        this.selectedChartDateRangeVal = 'Daily';
        this.specialTimeReg = /^(\d{4})-(\d{2})-(\d{2})$/; //格式为YYYY-MM-DD的正则
        this.chartInfoParam = {
            Range: 0,
        };
        const pieChartName = RMResx.RM_RC_PieChart_Name;
        const lineChartName = RMResx.RM_RC_LineChart_Name;
        this.state = {
            chartData: [],
            chartTabButtonItems: [
                {
                    icon: "fia-report",
                    tooltip: pieChartName
                },
                {
                    icon: "fia-chart-line",
                    tooltip: lineChartName
                }
            ],
            tabButtonIndex: 0,
            isShowBarChart: true,
            chartColor: 'rgb(46, 199, 201)',
            filterType: this.getFilterType(),
            chartDateRange: this.getChartDateRange(),
            selectedFilterTypeName: this.selectedFilterTypeName,

            //LabelStr 格式为YYYY-MM-DD
            isSpecialTime: false,
            specialTimeData: [],
        };
    }

    componentInit() {
        this.setChartInfo();
    }

    componentReceive(data, resetChart) {
        if (resetChart) {
            this.setState({
                chartData: RM.deepcopy(this.state.chartData)
            });
        } else {
            this.chartInfoParam = data;
            this.initChartTimeRange();
            this.setChartInfo();
        }
    }

    initChartTimeRange() {
        switch (this.chartInfoParam.Range) {
            case AuditRangeTypes["5D"]:
            case AuditRangeTypes["Custom"]:
                this.selectedChartDateRangeVal = 'Daily';
                break;
            case AuditRangeTypes["1M"]:
            case AuditRangeTypes["3M"]:
                this.selectedChartDateRangeVal = 'Weekly';
                break;
            case AuditRangeTypes["6M"]:
                this.selectedChartDateRangeVal = 'Monthly';
        }
    }

    getFilterType() {
        return [
            { Id: AuditReportFilterType.Time, Name: RMResx.RM_JS_RC_Audit_ViewBy_Option_Time, Checked: true },
            { Id: AuditReportFilterType.User, Name: RMResx.RM_JS_RC_Audit_ViewBy_Option_User, Checked: false },
            {
                Id: AuditReportFilterType.DocAveModule,
                Name: RMResx.RM_JS_RC_Audit_ViewBy_Option_Module,
                Checked: false
            },
            { Id: AuditReportFilterType.Action, Name: RMResx.RM_JS_RC_Audit_ViewBy_Option_Action, Checked: false },
            { Id: AuditReportFilterType.Status, Name: RMResx.RM_JS_RC_Audit_ViewBy_Option_Status, Checked: false }
        ];
    }

    getChartDateRange() {
        return [
            { value: 'Daily', Name: RMResx.RM_RC_Audit_Daily, Checked: true },
            { value: 'Weekly', Name: RMResx.RM_RC_Audit_Weekly, Checked: false },
            { value: 'Monthly', Name: RMResx.RM_RC_Audit_Monthly, Checked: false },
        ];
    }

    getDayOfMonthObj() {
        return {
            1: 31,
            3: 31,
            5: 31,
            7: 31,
            8: 31,
            10: 31,
            12: 31,
            2: -1,
            4: 30,
            6: 30,
            9: 30,
            11: 30
        };
    }

    setChartInfo() {
        $$.loading(true);
        let param = {};
        param.Range = this.chartInfoParam.Range;
        param.ViewBy = this.selectedFilterTypeId;
        param.StartTime = this.chartInfoParam.StartTime;
        param.EndTime = this.chartInfoParam.EndTime;
        let option = {
            url: '/api/AuditApi/GetChartInfo',
            data: param
        };
        fetchUtility(option).then((data) => {
            $$.loading(false);
            this.isRender = true;
            this.setDateRangeText(data);
            this.originChartData = RM.deepcopy(data.ChartDatas);
            let chartData = this.getChartData(data.ChartDatas);
            if (this.specialTimeReg.test(chartData[0].LabelStr)) {
                this.setState({ specialTimeData: chartData, isSpecialTime: true });
            } else {
                this.setState({ chartData: chartData, isSpecialTime: false });
            }

        });
    }

    getChartData(data) {
        let chartData = data;
        if (this.selectedFilterTypeId == AuditReportFilterType.Time) {
            chartData = this.getChartDataByTime(data);
        }
        for (let item of chartData) {
            item.filterName = this.selectedFilterTypeName;
        }
        return chartData;
    }

    getChartDataByTime(data) {
        let chartDateRange = RM.deepcopy(this.state.chartDateRange);
        for (let item of chartDateRange) {
            if (this.selectedChartDateRangeVal == item.value) {
                item.Checked = true;
            } else {
                item.Checked = false;
            }
        }
        let chartData = this.formatChartDataByTime(data);
        this.setState({ chartDateRange: chartDateRange });
        return chartData;
    }

    setDateRangeText(data) {
        let dateRangText = data.Start + ' - ' + data.End;
        this.props.setDateRangeText(dateRangText);
    }

    onTabChart(index) {
        let isShowBarChart = true;
        if (index == 1) {
            isShowBarChart = false;
        }
        this.setState({
            isShowBarChart: isShowBarChart
        });
    }

    showBarChart() {
        this.setState({ isShowBarChart: true });
    }

    showLineChart() {
        this.setState({ isShowBarChart: false });
    }

    formatChartDataByTime(data) {
        let chartData = data;
        if (this.selectedChartDateRangeVal == 'Weekly') {
            chartData = this.getChartDataAccordWeek(data);
        }
        if (this.selectedChartDateRangeVal == 'Monthly') {
            chartData = this.getChartDataAccordMon(data);
        }
        return chartData;
    }

    getChartDataAccordMon(data) {
        let newMonData = [];
        if (data == null || data.length == 0) {
            return;
        }
        let current = this.dayOfMonthOffset(data[0].month, data[0].year);
        let count = 0;
        let curIndex = 0;
        for (let i = 0; i < data.length; i++) {
            count += data[i].valueCount;
            if (data[i].day == current || i == data.length - 1) {
                newMonData.push({
                    LabelStr: data[curIndex].LabelStr + "~" + data[i].LabelStr,
                    valueCount: count
                });
                current = i + 1 >= data.length - 1
                    ? this.dayOfMonthOffset(data[data.length - 1].month, data[data.length - 1].year)
                    : this.dayOfMonthOffset(data[i + 1].month, data[i + 1].year);
                count = 0;
                curIndex = i + 1;
            }
        }
        return newMonData;
    }

    dayOfMonthOffset(data, year) {
        let dayOfMonthObj = this.getDayOfMonthObj();
        if (dayOfMonthObj[data] == -1) {
            if ((year % 100 != 0 && year % 4 == 0)
                || (year % 400 == 0)) {
                return 29;
            } else {
                return 28;
            }
        }
        return dayOfMonthObj[data];
    }

    getChartDataAccordWeek(data) {
        let daysArrAccordWeek = [];
        let dayArrAccordWeek = [];
        let newWeekData = [];
        for (let key in data) {
            if (data.hasOwnProperty(key)) {
                dayArrAccordWeek.push(data[key]);
                if (data[key].dateOfWeek == 0 || key == data.length - 1) {
                    daysArrAccordWeek.push(dayArrAccordWeek);
                    dayArrAccordWeek = [];
                }
            }
        }
        for (let item of daysArrAccordWeek) {
            let newWeekDataItem = {};
            let valueCount = 0;
            for (let key in item) {
                if (item.hasOwnProperty(key)) {
                    valueCount += item[key].valueCount;
                }
            }
            newWeekDataItem.valueCount = valueCount;
            newWeekDataItem.LabelStr = `${item[0].LabelStr} ~ ${item[item.length - 1].LabelStr}`;
            newWeekData.push(newWeekDataItem);
        }
        return newWeekData;
    }

    filterTypeChange(args) {
        this.selectedFilterTypeId = args.newValue.Id;
        this.selectedFilterTypeName = args.newValue.Name;
        this.setState({ selectedFilterTypeName: this.selectedFilterTypeName });
        this.setChartInfo();
    }

    chartDateRangeChange(args) {
        this.selectedChartDateRangeVal = args.newValue.value;
        let chartData = this.getChartData(this.originChartData);
        if (this.specialTimeReg.test(chartData[0].LabelStr)) {
            this.setState({ specialTimeData: chartData, isSpecialTime: true });
        } else {
            this.setState({ chartData: chartData, isSpecialTime: false });
        }
    }

    chartClick(item) {
        let labelValue = item.LabelVal;
        if (this.selectedFilterTypeId == 0) {
            labelValue = item.LabelStr;
        }
        this.props.onClick(this.selectedFilterTypeId, labelValue);
    }

    renderFilter() {
        return <div>
            <label className="ra-form-label">{RMResx.RM_Report_ActivitiesBy.replace(':', "")}</label>
            <div className="ra-audit-active-by">
                <R.Combobox
                    textField='Name'
                    valueField='Id'
                    checkedField='Checked'
                    searchable={false}
                    items={this.state.filterType}
                    width={150}
                    height={36}
                    onChange={this.filterTypeChange}
                />
            </div>
        </div>;
    }

    renderDiagramBar() {
        return <div className='diagramBar'>
            <div className='ra-diagram-bar-left'>
                {this.renderFilter()}
            </div>
            <div className='ra-diagram-bar-right'>
                <div className='pull-left'>
                    {this.renderChartDateRange()}
                </div>
                <div className='ra-spliter'></div>
                <div className='pull-left'>
                    <R.TabButton
                        items={this.state.chartTabButtonItems}
                        onChange={this.onTabChart}
                    />
                </div>
            </div>
        </div>;
    }

    renderChartDateRange() {
        if (this.selectedFilterTypeId == 0) {
            return <R.Combobox
                textField='Name'
                valueField='value'
                checkedField='Checked'
                searchable={false}
                items={this.state.chartDateRange}
                width={130}
                height={36}
                onChange={this.chartDateRangeChange}
            />;
        }
    }

    renderBarChart() {
        if (this.state.isShowBarChart && this.isRender) {
            const activeData = this.state.isSpecialTime ? this.state.specialTimeData : this.state.chartData;
            
            const formattedData = activeData.map(item => ({
                ...item,
                name: item.LabelStr,
                value: item.valueCount
            }));
            
            const dynamicHeight = Math.max(formattedData.length * 40 + 80, 280);

            return <div className="ra-audit-barchat" style={{ maxHeight: "400px", overflowY: "auto" }}>
                {formattedData.length > 0 && (
                    <R.Charts onDataClick={this.chartClick} onSeriesClick={this.chartClick} height={dynamicHeight}>
                        <R.Charts.Grid
                            type="bar"
                            orient="v"
                            items={formattedData}
                            color={this.state.chartColor}
                            seriesHeader={RMResx.RM_RC_AuditActivities}
                        />
                    </R.Charts>
                )}
            </div>;
        }
    }

    renderLineChart() {
        if (!this.state.isShowBarChart && this.isRender) {
            const activeData = this.state.isSpecialTime ? this.state.specialTimeData : this.state.chartData;
            
            const formattedData = activeData.map(item => ({
                ...item,
                name: item.LabelStr,
                value: item.valueCount
            }));

            return <div className="ra-audit-linechat">
                {formattedData.length > 0 && (
                     <R.Charts onDataClick={this.chartClick} onSeriesClick={this.chartClick}>
                        <R.Charts.Grid
                            type="line"
                            items={formattedData}
                            color={this.state.chartColor}
                            seriesHeader={RMResx.RM_RC_AuditActivities}
                            showPoint={true}
                        />
                    </R.Charts>
                )}
            </div>;
        }
    }

    render() {
        return (
            <div className="reco-audit-diagram-wrapper" id={this.props.id}>
                <div className="reco-audit-diagram-actions">
                    <div className="reco-datatime-selector">
                        <div className="reco-title" tabIndex="0">
                            {RMResx.RM_Report_ActivitiesBy.replace(':', "")}
                        </div>
                        <div className="reco-active-by">
                            <R.Combobox
                                textField='Name'
                                valueField='Id'
                                checkedField='Checked'
                                searchable={false}
                                items={this.state.filterType}
                                width={150}
                                onChange={this.filterTypeChange}
                            />
                        </div>
                        <div className="reco-time-type">
                            {
                                this.selectedFilterTypeId == 0 &&
                                <R.Combobox
                                    textField='Name'
                                    valueField='value'
                                    checkedField='Checked'
                                    searchable={false}
                                    items={this.state.chartDateRange}
                                    width={130}
                                    height={36}
                                    onChange={this.chartDateRangeChange}
                                />
                            }
                        </div>
                    </div>
                    <div className="reco-chart-selector">
                        <R.TabButton
                            items={this.state.chartTabButtonItems}
                            onChange={this.onTabChart}
                        />
                    </div>
                </div>
                <div className="reco-audit-diagram-chart">
                    {this.renderBarChart()}
                    {this.renderLineChart()}
                </div>
            </div>
        );
    }
}

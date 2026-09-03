import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../../RC/CommonShowReport.jsx";
import {TermShowReportColumnsInfo, TermShowReportColumnsWidth} from "./../Constants";

export default class TermUsageShowReport extends Component {
    constructor(props) {
        super(props);
    }

    getMultiComboboxData() {
        let currentTermShowReportColumnsInfo = this.getColumnsInfo();
        let multiComboboxData = [];
        let multiComboboxDataId = 0;
        for (let value of currentTermShowReportColumnsInfo) {
            let column = {};
            column.isChecked = false;
            if (value == RMResx.RM_JS_RC_ReportColumn_ObjectLevel
                || value == RMResx.RM_JS_RC_ReportColumn_TitleOrName
                || value == RMResx.RM_JS_RC_ReportColumn_Url
                || value == RMResx.RM_JS_RC_ReportColumn_BCSTermName
                || value == RMResx.RM_JS_RC_ReportColumn_TermStatus
            ) {
                column.isChecked = true;
            }
            column.value = value;
            column.id = multiComboboxDataId;
            multiComboboxData.push(column);
            multiComboboxDataId++;
        }
        return multiComboboxData;
    }

    getColumnsInfo(){
        let currentTermShowReportColumnsInfo = RM.deepcopy(TermShowReportColumnsInfo);
        return currentTermShowReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_TermUsageReport, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={2}
                history={this.props.history}
                showReportApiUrl="/api/TermUsageReportApi/ShowReportQueryPager"
                exportUrl="/api/TermUsageReportApi/DownloadFile"
                columnsWidth={TermShowReportColumnsWidth}
                getMultiComboboxData={this.getMultiComboboxData.bind(this)}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
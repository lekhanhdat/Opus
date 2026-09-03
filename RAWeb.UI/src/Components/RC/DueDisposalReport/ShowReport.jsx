import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../../RC/CommonShowReport.jsx";
import {DueShowReportColumnsInfo, DueShowReportColumnsWidth} from "./../Constants";

export default class DueDisposalShowReport extends Component {
    constructor(props) {
        super(props);
        this.reportType = RM.Url.getParam(window.location.href, "type");
    }

    getMultiComboboxData(reportType) {
        let currentDueShowReportColumnsInfo = this.getColumnsInfo(reportType);
        let multiComboboxData = [];
        let multiComboboxDataId = 0;
        for (let value of currentDueShowReportColumnsInfo) {
            let column = {};
            column.isChecked = false;
            if (value == RMResx.RM_JS_RC_ReportColumn_ObjectLevel
                || value == RMResx.RM_JS_RC_ReportColumn_TitleOrName
                || value == RMResx.RM_JS_RC_ReportColumn_SiteCollectionTitle
                || value == RMResx.RM_JS_RC_ReportColumn_Url
                || value == RMResx.RM_JS_RC_ReportColumn_BCSTermName
                || value == RMResx.RM_JS_RC_ReportColumn_AppliedRuleName
                || value == RMResx.RM_JS_Rule_DisposalClass_Title
                || value == RMResx.RM_JS_RC_ReportColumn_DisposalAction
                || value == RMResx.RM_JS_RC_ReportColumn_Status
                || value == RMResx.RM_JS_RC_ReportColumn_Comment
                || value == RMResx.RM_JS_RC_ReportColumn_GroupsTeamsName
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

    getColumnsInfo(reportType){
        let currentDueShowReportColumnsInfo = RM.deepcopy(DueShowReportColumnsInfo);
        //非SP Report去掉SC Title 列
        if (!reportType) {
            reportType = this.reportType;
        }
        if(reportType != 1 && reportType != 6103 && reportType != 5510 && reportType != 6200 && reportType != 10306){
            currentDueShowReportColumnsInfo.splice(3, 1);
        }
        if (reportType == 10306) {
            currentDueShowReportColumnsInfo.splice(3, 1, RMResx.RM_JS_RC_ReportColumn_GroupsTeamsName);
        }
        return currentDueShowReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_DueDisposalReportManagement, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={1}
                history={this.props.history}
                showReportApiUrl="/api/DueDisposalApi/ShowReportQueryPager"
                exportUrl="/api/DueDisposalApi/DownloadFile"
                columnsWidth={DueShowReportColumnsWidth}
                getMultiComboboxData={this.getMultiComboboxData.bind(this)}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
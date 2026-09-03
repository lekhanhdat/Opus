import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../../RC/CommonShowReport.jsx";
import {RestoreShowReportColumnsInfo, RestoreShowReportColumnsWidth, RestoreShowReportSortColumns} from "./../Constants";

export default class RestoreShowReport extends Component {
    constructor(props) {
        super(props);
        this.reportType = RM.Url.getParam(window.location.href, "type");
    }

    getColumnsInfo(){
        let currentDueShowReportColumnsInfo = RM.deepcopy(RestoreShowReportColumnsInfo);
        return currentDueShowReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_RestoreReportManagement, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={21}
                history={this.props.history}
                showReportApiUrl="/api/RestoreReportApi/ShowReportQueryPager"
                exportUrl="/api/RestoreReportApi/DownloadFile"
                columnsWidth={RestoreShowReportColumnsWidth}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={RestoreShowReportSortColumns}
            />
        </div>;
    }
}
import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../../RC/CommonShowReport.jsx";
import {AvaSpaceShowReportReportColumnsInfo, AvaSpaceShowReportColumnsWidth} from "../Constants";

export default class TermUsageShowReport extends Component {
    constructor(props) {
        super(props);
    }

    getColumnsInfo(){
        let currentAvaSpaceShowReportReportColumnsInfo = RM.deepcopy(AvaSpaceShowReportReportColumnsInfo);
        return currentAvaSpaceShowReportReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_AvailableSpaceReport,{text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={14}
                history={this.props.history}
                showReportApiUrl="/api/AvailableSpaceReportApi/ShowReportQueryPager"
                exportUrl="/api/AvailableSpaceReportApi/DownloadFile"
                columnsWidth={AvaSpaceShowReportColumnsWidth}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../../RC/CommonShowReport.jsx";
import {CreateAndDesShowReportColumnsInfo, CreateAndDesShowReportColumnsWidth} from "./../Constants";

export default class CreationAndDestructionShowReport extends Component {
    constructor(props) {
        super(props);
    }

    getColumnsInfo(){
        let currentCreateAndDesShowReportColumnsInfo = RM.deepcopy(CreateAndDesShowReportColumnsInfo);
        return currentCreateAndDesShowReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_CreationAndDestructionReport, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={13}
                history={this.props.history}
                showReportApiUrl="/api/TimeFrameProfileApi/ShowReportQueryPager"
                exportUrl="/api/TimeFrameProfileApi/DownloadFile"
                columnsWidth={CreateAndDesShowReportColumnsWidth}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
import { Component } from "react";
import { JobType } from "../../../Constants/Constants";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonShowReport from "../CommonShowReport";
import { ActionAuditShowReportColumnsInfo, ActionAuditShowReportColumnsWidth } from "../Constants";

export default class CreationAndDestructionShowReport extends Component {
    constructor(props) {
        super(props);
    }

    getColumnsInfo(){
        let currentActionAuditShowReportColumnsInfo = RM.deepcopy(ActionAuditShowReportColumnsInfo);
        return currentActionAuditShowReportColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_ActionAuditReportManagement, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={JobType.SPOActionAuditReport}
                history={this.props.history}
                showReportApiUrl="/api/ActionAuditReportApi/ShowReportQueryPager"
                exportUrl="/api/ActionAuditReportApi/DownloadFile"
                columnsWidth={ActionAuditShowReportColumnsWidth}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
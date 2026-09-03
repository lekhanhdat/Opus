import { Component } from "react";
import { JobType } from "../../../../Constants/Constants";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import CommonShowReport from "../../CommonShowReport";
import { ArchivedSiteColumnsInfo, ArchivedSiteColumnsWidth } from "../../Constants";

export default class ArchivedSiteShowReport extends Component {
    constructor(props) {
        super(props);
    }

    getColumnsInfo(){
        let currentArchivedSiteColumnsInfo = RM.deepcopy(ArchivedSiteColumnsInfo);
        return currentArchivedSiteColumnsInfo;
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap
                data={[SiteMapLinks.RC_StorageOptimizationReportManagement, {text: RMResx.RM_JS_Common_ShowReport}]}/>
            <CommonShowReport
                reportJobType={JobType.ArchivedSiteReport}
                history={this.props.history}
                showReportApiUrl="/api/Dashboard/GetArchivedSiteReportStatus"
                exportUrl="/api/ActionAuditReportApi/DownloadFile"
                columnsWidth={ArchivedSiteColumnsWidth}
                getColumnsInfo={this.getColumnsInfo.bind(this)}
                sortColumns={[]}
            />
        </div>;
    }
}
import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonReportManagement from "../../RC/CommonReportManagement.jsx";
import {ReportType} from "../Constants";
import {JobType} from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";

export default class DueDisposalReportManagement extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_AvailableSpaceReport]}/>
            <CommonReportManagement
                type={ReportType.AvailableSpaceReport}
                getDataUrl="/api/AvailableSpaceReportApi/GetAvailableSpaceReport"
                deleteUrl="/api/AvailableSpaceReportApi/DeleteProfiles"
                generateUrl="/api/AvailableSpaceReportApi/GenerateReport"
                viewDetailsUrl={RouterUrls.RC_AvailableSpaceReportDetail}
                newSPUrl={RouterUrls.RC_AvailableSpaceReportProfile + `/?type=${JobType.AvailableSpaceReport}`}
                editUrl={RouterUrls.RC_AvailableSpaceReportProfile + "/"}
                showReportUrl={RouterUrls.RC_AvailableSpaceReportShowReport}
                isSingleBtn={true}
                // showReportUrl="/RC/AvailableSpaceShowReport"
            />
        </div>;
    }
}



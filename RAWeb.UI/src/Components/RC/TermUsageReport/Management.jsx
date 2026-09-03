import { Component } from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonReportManagement from "../../RC/CommonReportManagement.jsx";
import { ReportType } from "../Constants";
import {JobType} from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";

export default class TermUsageReportManagement extends Component {
    constructor(props) {
        super(props);
    }

    render () {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_TermUsageReport]} />
            <CommonReportManagement
                type={ReportType.BCSTermUsageReport}
                getDataUrl="/api/TermUsageReportApi/GetTermsUsageReport"
                deleteUrl="/api/TermUsageReportApi/DeleteProfiles"
                generateUrl="/api/TermUsageReportApi/GenerateReport"
                viewDetailsUrl={RouterUrls.RC_TermUsageReportViewDetail}
                newSPUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.BCSTermUsageReport}`}
                newEXOUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.EXOTermUsageReport}`}
                newPhysicalUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.PhysicalTermUsageReport}`}
                newFSUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.FSBCSTermUsageReport}`}
                newOneDriveUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.OneDriveTermUsageReport}`}
                newSPOnPremiseUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.SPOnPremiseTermUsageReport}`}
                newBoxUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.BoxBCSTermUsageReport}`}
                newGoogleDriveUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.GoogleBCSTermUsageReport}`}
                newTeamsUrl={RouterUrls.RC_TermUsageReportProfile + `/?type=${JobType.TeamsBCSTermUsageReport}`}
                editUrl={RouterUrls.RC_TermUsageReportProfile + "/"}
                showReportUrl={RouterUrls.RC_TermUsageShowReport}
            />
        </div>;
    }
}

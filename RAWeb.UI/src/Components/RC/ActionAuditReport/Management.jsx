import { Component } from "react";
import { JobType } from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { SourceFlag } from "../../Common/Constants";
import CommonReportManagement from "../CommonReportManagement";
import { ReportType } from "../Constants";

export default class ActionAuditReportManagement extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_ActionAuditReportManagement]}/>
            <CommonReportManagement
                type={ReportType.SPOActionAuditReport}
                getDataUrl="/api/ActionAuditReportApi/GetProfileReport"
                deleteUrl="/api/ActionAuditReportApi/DeleteProfiles"
                generateUrl="/api/ActionAuditReportApi/GenerateReport"
                viewDetailsUrl={RouterUrls.RC_ActionAuditReportDetail}
                newSPUrl={RouterUrls.RC_ActionAuditReportProfile + `/?type=${JobType.SPOActionAuditReport}`}
                newOneDriveUrl={RouterUrls.RC_ActionAuditReportProfile + `/?type=${JobType.OneDriveActionAuditReport}`}
                newTeamsUrl={RouterUrls.RC_ActionAuditReportProfile + `/?type=${JobType.TeamsActionAuditReport}`}
                editUrl={RouterUrls.RC_ActionAuditReportProfile + "/"}
                showReportUrl={RouterUrls.RC_ActionAuditReportShowReport}
                specialSource={[SourceFlag.SharePoint, SourceFlag.OneDrive]}
            />
        </div>;
    }
}
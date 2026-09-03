import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonReportManagement from "../../RC/CommonReportManagement.jsx";
import {ReportType} from "../Constants";
import {JobType} from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import { SourceFlag } from "../../Common/Constants";

export default class RestoreReportManagement extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_RestoreReportManagement]}/>
            <CommonReportManagement
                type={ReportType.RestoreReport}
                getDataUrl="/api/RestoreReportApi/GetProfileReport"
                deleteUrl="/api/RestoreReportApi/DeleteProfiles"
                generateUrl="/api/RestoreReportApi/GenerateReport"
                newSPUrl={RouterUrls.RC_RestoreReportProfile + `/?type=${JobType.RestoreReport}`}
                newOneDriveUrl={RouterUrls.RC_RestoreReportProfile + `/?type=${JobType.OneDriverRestoreReport}`}
                newTeamsUrl={RouterUrls.RC_RestoreReportProfile + `/?type=${JobType.TeamsRestoreReport}`}
                newGoogleUrl={RouterUrls.RC_RestoreReportProfile + `/?type=${JobType.GoogleRestoreReport}`}
                editUrl={RouterUrls.RC_RestoreReportProfile + "/"}
                viewDetailsUrl={RouterUrls.RC_RestoreReportViewDetail}
                showReportUrl={RouterUrls.RC_RestoreShowReport}
                specialSource={[SourceFlag.SharePoint, SourceFlag.OneDrive]}
            />
        </div>;
    }
}



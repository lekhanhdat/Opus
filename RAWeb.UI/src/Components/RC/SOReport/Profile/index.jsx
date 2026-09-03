import {Component} from "react";
import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import CommonReportManagement from "../../../RC/CommonReportManagement.jsx";
import {ReportType} from "../../Constants";
import {JobType} from "../../../../Constants/Constants";
import RouterUrls from "../../../../Constants/RouterUrls";

export default class ArchivedSiteProfiles extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return <div className='ra-common-report'>
            <CommonReportManagement
                type={ReportType.StorageOptimizationReport}
                getDataUrl="/api/Dashboard/GetProfileReport"
                deleteUrl="/api/Dashboard/DeleteProfiles"
                generateUrl="/api/Dashboard/GenerateReport"
                newSPUrl={RouterUrls.RC_StorageOptimizationReportProfile + `/?type=${JobType.ArchivedSiteReportSharePointOnline}`}
                newOneDriveUrl={RouterUrls.RC_StorageOptimizationReportProfile + `/?type=${JobType.ArchivedSiteReportSOneDrive}`}
                newGoogleDriveUrl={RouterUrls.RC_StorageOptimizationReportProfile + `/?type=${JobType.ArchivedSiteReportGoogle}`}
                newTeamsUrl={RouterUrls.RC_StorageOptimizationReportProfile + `/?type=${JobType.ArchivedSiteReportTerm}`}
                editUrl={RouterUrls.RC_StorageOptimizationReportProfile + "/"}
                showReportUrl={RouterUrls.RC_StorageOptimizationReportShowReport}
            />
        </div>;
    }
}   
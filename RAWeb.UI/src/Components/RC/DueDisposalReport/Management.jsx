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
            <$g.SiteMap data={[SiteMapLinks.RC_DueDisposalReportManagement]}/>
            <CommonReportManagement
                type={ReportType.ItemDueForDisposalReport}
                getDataUrl="/api/DueDisposalApi/GetProfileReport"
                deleteUrl="/api/DueDisposalApi/DeleteProfiles"
                generateUrl="/api/DueDisposalApi/GenerateReport"
                newSPUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.ItemsFilesDueDisposal}`}
                newEXOUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.EXOItemsFilesDueDisposalReport}`}
                newPhysicalUrl={RouterUrls.RC_DueDisposalReportProfile +`/?type=${JobType.PhysicalItemsFilesDueDisposalReport}`}
                newFSUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.FSItemsFilesDueDisposal}`}
                newSPOnPremiseUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.SPOnPremiseItemsFilesDueDisposal}`}
                newBoxUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.BoxItemsFilesDueDisposal}`}
                newOneDriveUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.OneDriveItemsFilesDueDisposal}`}
                newGoogleDriveUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.GoogleDriveItemsFilesDueDisposal}`}
                newTeamsUrl={RouterUrls.RC_DueDisposalReportProfile + `/?type=${JobType.TeamsItemsFilesDueDisposalReport}`}
                editUrl={RouterUrls.RC_DueDisposalReportProfile + "/"}
                viewDetailsUrl={RouterUrls.RC_DueDisposalReportViewDetail}
                showReportUrl={RouterUrls.RC_DueDisposalShowReport}
            />
        </div>;
    }
}



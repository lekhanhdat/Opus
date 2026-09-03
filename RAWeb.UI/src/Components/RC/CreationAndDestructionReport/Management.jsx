import {Component} from "react";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import CommonReportManagement from "../CommonReportManagement.jsx";
import {ReportType} from "../Constants";
import {JobType} from "../../../Constants/Constants";
import RouterUrls from "../../../Constants/RouterUrls";
import { showToast } from "../../../Utilities/CommonUtil";
import { checkPermission } from "../../../Utilities/permissionManager";

export default class CreationAndDestructionReportManagement extends Component {
    constructor(props) {
        super(props);
    }

    onClickReportBtn = () => {
        let args = {
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_RC_Confirm_MetricsReportJob,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raGeneralReportDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runJPMCReport.bind(this)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    runJPMCReport() {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/TimeFrameProfileApi/GenerateSiteMetricsReportJob",
            method: "Post",
            // data: 
        };
        fetchUtility(option).then((result) => {
            if (result) {
                showToast.success(
                    <$g.I18NProvider msg={RMResx.RM_MA_HistoryExport_JobStart}>
                        <a className="ra-link-a" href="/Root/JM/Index">
                            {RMResx.RM_JS_JM_Title}
                        </a>
                        <a className="ra-link-a" href="/Root/DC/Download">
                            {RMResx.RM_JS_DC_Title}
                        </a>
                    </$g.I18NProvider>
                )
            }
            $$.loading(false);

        }).catch((e) => {
            $$.loading(false);
        });
    }

    render() {
        return <div className='ra-common-report'>
            <$g.SiteMap data={[SiteMapLinks.RC_CreationAndDestructionReport]}>
                {RM.gData.enableCustomizationApp && checkPermission("RC_ExportSiteMetricsReport_Generate", RM.UserResources) && <div className="ra-flex-justify-end">
                    <R.Button
                        primary={true}
                        classify="theme"
                        title={RMResx.RM_RC_Generate_ExportSiteMetricsReport}
                        text={RMResx.RM_RC_Generate_ExportSiteMetricsReport}
                        onClick={this.onClickReportBtn}
                    />
                </div>}
            </$g.SiteMap>
            <CommonReportManagement
                type={ReportType.CreationAndDestructionReport}
                getDataUrl="/api/TimeFrameProfileApi/GetProfileReport"
                deleteUrl="/api/TimeFrameProfileApi/DeleteProfiles"
                generateUrl="/api/TimeFrameProfileApi/GenerateReport"
                viewDetailsUrl={RouterUrls.RC_CreationAndDestructionViewDetail}
                newSPUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.CreateAndDestroyedFileReport}`}
                newEXOUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.EXOCreateAndDestroyedFileReport}`}
                newPhysicalUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.PhysicalCreateAndDestroyedFileReport}`}
                newFSUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.FSCreateAndDestroyedFileReport}`}
                newOneDriveUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.OneDriveCreateAndDestroyedFileReport}`}
                newSPOnPremiseUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.SPOnPremiseCreateAndDestroyedFileReport}`}
                newBoxUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.BoxCreateAndDestroyedFileReport}`}
                newGoogleDriveUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.GoogleDriveCreateAndDestroyedFileReport}`}
                newTeamsUrl={RouterUrls.RC_CreationAndDestructionProfile + `/?type=${JobType.TeamsCreateAndDestroyedFileReport}`}
                editUrl={RouterUrls.RC_CreationAndDestructionProfile + "/"}
                showReportUrl={RouterUrls.RC_CreationAndDestructionShowReport}
            />
        </div>;
    }
}


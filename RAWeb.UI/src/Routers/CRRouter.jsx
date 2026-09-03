import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import ContentRepositoryManagement4EXO from "../Components/BCM/ContentRepositoryManagement/ContentRepositoryManagementForEXO";
import ContentRepositoryManagement4Phy from "../Components/BCM/ContentRepositoryManagement/ContentRepositoryManagementForPhy";
import CRMForSPO from "../Components/BCM/ContentRepositoryManagement/CRMForSPO";
import CRMForOneDrive from "../Components/BCM/ContentRepositoryManagement/CRMForOneDrive";
import ContentRepositoryManagement4FS from "../Components/BCM/ContentRepositoryManagement/ContentRepositoryManagementForFS";
import ContentRepositoryManagement4LSP from "../Components/BCM/ContentRepositoryManagement/ContentRepositoryManagementForLSP";
import ContentRepositoryManagement4AF from "../Components/BCM/ContentRepositoryManagement/ContentRepositoryManagementForAF";
import FSConnGroupManagement from "../Components/BCM/FSConnGroup/Management";
import AzureFileShareConfigureConnection from "../Components/BCM/AzureFileShareConfigureConnection/index";
import ContentRepositoryManagement4Box from "../Components/BCM/ContentRepositoryManagement/CRMForBox";
import BoxConfigureConnection from "../Components/BCM/BoxConfigureConnection";
import ContentRepositoryManagement4GoogleDrive from "../Components/BCM/ContentRepositoryManagement/CRMForGoogle";
import CRMForTeams from "../Components/BCM/ContentRepositoryManagement/CRMForTeams";
import SwitchForTeamsComponent from "../Components/BCM/ContentRepositoryManagement/SwitchForTeams/SwitchForTeamsComponent";
import FSConnectionDetails from "../Components/BCM/FSConnGroup/ConnectionDetails";
import { EnvironmentHelper } from "../Utilities/CommonUtil";
import FSJobDetails from "../Components/BCM/FSConnGroup/ConnectionDetails/JobDetails";

function getChildren() {
    let children = [];
    children.push(new RouteConfig(
        "CRTeams",
        RouterUrls.BCM_ContentRepositoryManagement_Teams,
        RMResx.RM_JS_SPS_TabLabel_Teams
    ).setComponent(CRMForTeams));
    children.push(new RouteConfig(
        "CRTeams",
        RouterUrls.BCM_ContentRepositoryManagement_Teams_Switch,
        RMResx.RM_JS_SPS_TabLabel_SwitchToTeams
    ).setComponent(SwitchForTeamsComponent)
    .setShowInNav(false));
    children.push(new RouteConfig(
        "CRSPO",
        RouterUrls.BCM_ContentRepositoryManagement_SPO,
        RMResx.RM_JS_SPS_TabLabel_SP
    ).setComponent(CRMForSPO));
    children.push(new RouteConfig(
        "CROD",
        RouterUrls.BCM_ContentRepositoryManagement_OD,
        RMResx.RM_JS_SPS_TabLabel_OneDrive
    ).setComponent(CRMForOneDrive));
    children.push(new RouteConfig(
        "CREXO",
        RouterUrls.BCM_ContentRepositoryManagement_EXO,
        RMResx.RM_JS_SPS_TabLabel_EXO
    ).setComponent(ContentRepositoryManagement4EXO));
    children.push(new RouteConfig(
        "CRGoogleDrive",
        RouterUrls.BCM_ContentRepositoryManagement_GoogleDrive,
        RMResx.RM_JS_SPS_TabLabel_GoogleDrive
    ).setComponent(ContentRepositoryManagement4GoogleDrive));
    children.push(new RouteConfig(
        "CRPHY",
        RouterUrls.BCM_ContentRepositoryManagement_Phy,
        RMResx.RM_JS_SPS_TabLabel_Physical
    ).setComponent(ContentRepositoryManagement4Phy));
    children.push(new RouteConfig(
        "CRFS",
        RouterUrls.BCM_ContentRepositoryManagement_FS,
        RMResx.RM_JS_SPS_TabLabel_FS
    ).setComponent(ContentRepositoryManagement4FS)
    .setShowInNav(!EnvironmentHelper.IsGCPEnvironment));
    children.push(new RouteConfig(
        "CRFS",
        RouterUrls.BCM_FSConnGroup,
        RMResx.RM_FS_Register_PageTitle
    )
        .setComponent(FSConnGroupManagement)
        .setShowInNav(false));
    children.push(new RouteConfig(
        "CRFS",
        RouterUrls.BCM_FSConnection_JobMonitor,
        RMResx.RM_FS_Connection_JobMonitor_PageTitle
    )
        .setComponent(FSConnectionDetails)
        .setShowInNav(false)
    );
    children.push(new RouteConfig(
        "CRFS",
        RouterUrls.BCM_FSConnection_JobDetails,
        RMResx.RM_FS_Connection_JobDetails_PageTitle
    )
        .setComponent(FSJobDetails)
        .setShowInNav(false)
    );
    children.push(new RouteConfig(
        "CRAZ",
        RouterUrls.BCM_AzFileShareConfigureConnection,
        RMResx.RM_AF_Register_PageTitle_Link
    )
        .setComponent(AzureFileShareConfigureConnection)
        .setShowInNav(false));
    children.push(new RouteConfig(
        "CRLSP",
        RouterUrls.BCM_ContentRepositoryManagement_LSP,
        RMResx.RM_JS_SPS_TabLabel_SPLocal
    ).setComponent(ContentRepositoryManagement4LSP)
    .setShowInNav(!EnvironmentHelper.IsGCPEnvironment));
    children.push(new RouteConfig(
        "CRAZ",
        RouterUrls.BCM_ContentRepositoryManagement_AF,
        RMResx.RM_JS_SPS_TabLabel_AF
    ).setComponent(ContentRepositoryManagement4AF));
    children.push(new RouteConfig(
        "CRBox",
        RouterUrls.BCM_ContentRepositoryManagement_Box,
        RMResx.RM_JS_SPS_TabLabel_Box
    ).setComponent(ContentRepositoryManagement4Box));
    children.push(new RouteConfig(
        "CRBox",
        RouterUrls.BCM_BoxConfigureConnection,
        RMResx.RM_AF_Register_PageTitle_Link
    )
        .setComponent(BoxConfigureConnection)
        .setShowInNav(false));
    return children;
}
const CRRouteConfig = new RouteConfig(
    "CR",
    RouterUrls.BCM_ContentRepositoryManagement,
    RMResx.RM_Nav_ContentRepository,
    "--group-3",
    ".fia-content_sources"
).addChildren(...getChildren());

export default CRRouteConfig;

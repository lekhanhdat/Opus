import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import DC from "../Components/DC/DownloadCenter";

const DCRouterConfig = new RouteConfig(
    "DC",
    RouterUrls.DC_Download,
    RMResx.RM_JS_DC_Title,
    RMResx.RM_NavPanel_Group_Activity,
    ".fia-download-center-nav"
)
    .setComponent(DC)

export default DCRouterConfig;
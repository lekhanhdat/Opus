import RestoreCenter from "../Components/ArchiveRC/RestoreCenter";
import RouteConfig from "../Components/Base/RouteConfig";
import RouterUrls from "../Constants/RouterUrls";

const ArchiveRCRouterConfig = new RouteConfig(
    "RestoreCenter",
    RouterUrls.Archiver_RestoreCenter,
    RMResx.RM_AR_RC_Title,
    RMResx.RM_NavPanel_Group_Activity,
    ".fia-restore-center-nav"
)
    .setComponent(RestoreCenter);

export default ArchiveRCRouterConfig;
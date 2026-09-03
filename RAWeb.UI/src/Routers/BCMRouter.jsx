import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import TermManagement from "../Components/BCM/TermManagement";

const BCMRouteConfig = new RouteConfig(
    "BCM",
    RouterUrls.BCM,
    RMResx.RM_Home_Module_BusinessClassification,
    "faui-home"
)
    .setTooltip(RMResx.RM_Home_Module_BusinessClassificationDesc)
    .addChildren(
        new RouteConfig(
            "BCM_TermManagement",
            RouterUrls.BCM_TermManagement,
            RMResx.RM_TM_PageTitle
        ).setComponent(TermManagement),
    );
export default BCMRouteConfig;

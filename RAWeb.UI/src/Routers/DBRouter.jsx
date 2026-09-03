import RouteConfig from "../Components/Base/RouteConfig";
import Dashboard from "../Components/Home/Dashboard";
import RouterUrls from "../Constants/RouterUrls";

const DBRouteConfigs = [
    new RouteConfig("Home", "/", RMResx.RM_DSB_PageTitle, ".fia-dashboard")
        .setComponent(Dashboard)
        .setShowInNav(false),
    new RouteConfig("Home", "/Root/Index", RMResx.RM_DSB_PageTitle, ".fia-dashboard")
        .setComponent(Dashboard)
        .setShowInNav(false),
    new RouteConfig(
        "Home",
        RouterUrls.Home,
        RMResx.RM_DSB_PageTitle,
        "--group-1",
        ".fia-dashboard"
    ).setComponent(Dashboard)
];

export default DBRouteConfigs;

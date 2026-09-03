import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import HybridSearch from "../Components/Common/HybridSearch/HybridSearch";
import ElectronicManageHold from "../Components/PRM/RecordsExplorer/ManageHold/PhyManageHold";

const RERouteConfig = new RouteConfig(
    "HybridSearch",
    RouterUrls.PRM_HybridSearch,
    RMResx.RM_Nav_Search,
    "--group-2",
    ".fia-search",
).setComponent(HybridSearch).addChildren(
    new RouteConfig(
        "HybridSearch",
        RouterUrls.BCM_ManageHold,
        RMResx.RM_JS_RDM_Hold_ManageHoldTitle
    )
        .setComponent(ElectronicManageHold)
        .setShowInNav(false)
        .setExact(false)
);

export default RERouteConfig;

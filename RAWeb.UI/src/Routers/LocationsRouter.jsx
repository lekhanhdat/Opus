import RouterUrls from "../Constants/RouterUrls";
import LocationManagement from "../Components/PRM/LocationManagement";
import LocationSynchronisation from "../Components/PRM/LocationSynchronisation";
import RouteConfig from "../Components/Base/RouteConfig";

const LocationsRouterConfig = new RouteConfig(
    "PRM_LocationManagement",
    RouterUrls.PRM_LocationManagement,
    RMResx.RM_Nav_PR_LocationManager,
    RMResx.RM_Nav_PhysicalRecords,
    ".fia-location"
)
    .setComponent(LocationManagement)
    .addChildren(
        new RouteConfig(
            "PRM_LocationManagement",
            RouterUrls.PRM_LocationSynchronisation,
            RMResx.RM_PRM_LS_PageTitle
        )
            .setComponent(LocationSynchronisation)
            .setShowInNav(false),
    );

export default LocationsRouterConfig;
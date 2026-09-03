import RouterUrls from "../Constants/RouterUrls";
import { 
    SourceConnectorCreateOrEdit,
    SourceConnectorList
} from "../Components/Customization/SourceConnector/index";
import RouteConfig from "../Components/Base/RouteConfig";

const ConnectorRouterConfig = new RouteConfig(
    "Connector",
    RouterUrls.Connector,
    RMResx.RM_Connector_Title,
    ".fia-job-monitor"
)
    .setComponent(SourceConnectorList)
    .addChildren(
        new RouteConfig(
            "Connector",
            RouterUrls.Connector_CreateOrEdit,
            ""
        )
            .setComponent(SourceConnectorCreateOrEdit)
            .setShowInNav(false).setExact(false),
    );

export default ConnectorRouterConfig;
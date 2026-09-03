import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import MachineLearning from "../Components/ML/MachineLearning/Index";
import { EnvironmentHelper } from "../Utilities/CommonUtil";

const MLRouteConfig = new RouteConfig(
    "ML",
    RouterUrls.ML_MachineLearning,
    RMResx.RM_ML_MachineLearning,
    "--group-3",
    ".fia-maestro_ai"
).setComponent(MachineLearning)
.setShowInNav(true); // !EnvironmentHelper.IsGCPEnvironment

export default MLRouteConfig;
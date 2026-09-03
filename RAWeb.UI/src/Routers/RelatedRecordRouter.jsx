import RouteConfig from "../Components/Base/RouteConfig";
import RelatedRecord from "../Components/RelatedRecord/Index";

const  RelatedRecordsRouteConfig = new RouteConfig(
    "RelatedRecords",
    "/RelatedRecords",
).setComponent(RelatedRecord).setShowInNav(false);

export default RelatedRecordsRouteConfig; 
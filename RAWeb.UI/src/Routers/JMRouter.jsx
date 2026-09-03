import RouterUrls from "../Constants/RouterUrls";
import JM from "../Components/JM/JM";
import JobDetail from "../Components/JM/JobDetail";
import PlanDetail from "../Components/JM/PlanDetail";
import RouteConfig from "../Components/Base/RouteConfig";

const JMRouterConfig = new RouteConfig(
    "JM",
    RouterUrls.JM_Index,
    RMResx.RM_JS_JM_Title,
    RMResx.RM_NavPanel_Group_Activity,
    ".fia-job-monitor"
)
    .setComponent(JM)
    .addChildren(
        new RouteConfig(
            "JM",
            RouterUrls.JM_Detail,
            RMResx.RM_JS_JM_PlanDetails,
            "fia-clock-regular"
        )
            .setComponent(JobDetail)
            .setShowInNav(false).setExact(false),
            
        new RouteConfig(
            "JM",
            RouterUrls.JM_PlanDetail,
            RMResx.RM_JS_JM_PlanDetails
        )
            .setComponent(PlanDetail)
            .setShowInNav(false).setExact(false),
    );

export default JMRouterConfig;

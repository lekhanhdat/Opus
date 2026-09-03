import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import RuleManagement from "../Components/RDM/RuleManageMent/RuleManagement";
import MAProcessesManagement from "../Components/RDM/MAProcessesManagement/MAProcessesManagement";
// import CreateRule from "../Components/RDM/RuleManageMent/CreateRule";
import ViewWorkFlow from "../Components/RDM/MAProcessesManagement/WorkFlow/ViewWorkFlow";
import CreateWorkFlow from "../Components/RDM/MAProcessesManagement/WorkFlow/CreateWorkflow";
import TermManagement from "../Components/BCM/TermManagement";

const ManageRouteConfig = new RouteConfig(
    "Manage",
    RouterUrls.BCM,
    "Manage",
    "fia-view"
).addChildren(
    new RouteConfig(
        "BCM_TermManagement",
        RouterUrls.BCM_TermManagement,
        "Terms"
    ).setComponent(TermManagement),
    new RouteConfig(
        "RDM_RuleManagement",
        RouterUrls.RDM_RuleManagement,
        "Rules"
    ).setComponent(RuleManagement),

    // new RouteConfig(
    //     "RDM_RuleManagement",
    //     RouterUrls.RDM_CreateRule,
    //     RMResx.RM_JS_Common_Create
    // )
    //     .setComponent(CreateRule)
    //     .setShowInNav(false),
    // new RouteConfig(
    //     "RDM_RuleManagement",
    //     RouterUrls.RDM_EditRule,
    //     RMResx.RM_JS_Common_Edit
    // )
    //     .setComponent(CreateRule)
    //     .setShowInNav(false),
    new RouteConfig(
        "RDM_WorkFlowManagement",
        RouterUrls.RDM_WorkFlowManagement,
        "Approval Processes"
    ).setComponent(MAProcessesManagement),
    new RouteConfig(
        "RDM_WorkFlowManagement",
        RouterUrls.RDM_ViewWorkFlow,
        RMResx.RM_RDM_WorkFlow_ViewDetail
    )
        .setComponent(ViewWorkFlow)
        .setShowInNav(false),
    new RouteConfig(
        "RDM_WorkFlowManagement",
        RouterUrls.RDM_CreateWorkFlow,
        RMResx.RM_RDM_CreateWorkFlow
    )
        .setComponent(CreateWorkFlow)
        .setShowInNav(false)
);
export default ManageRouteConfig;

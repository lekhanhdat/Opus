import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import MyRequest from "../Components/PRM/Request/MyRequest";
import ManualApproval from "../Components/RDM/ManualApproval/index";
import PickListForLoanRequests from "../Components/MT/PickListForLoanRequests/Index";
import PickListForDestruction from "../Components/MT/PickListForDestruction/Index";
import PickListForMovement from "../Components/MT/PickListForMovement/Index";
import MachineLearningReview from "../Components/MT/MachineLearningReview/Index";
import { EnvironmentHelper } from "../Utilities/CommonUtil";

const MTRouteConfig = new RouteConfig(
    "MT",
    RouterUrls.BCM,
    RMResx.RM_Nav_MyTasks,
    "--group-2",
    ".fia-my-tasks-nav",
).addChildren(
    new RouteConfig(
        "RDM_ManualApprovalReview",
        RouterUrls.RDM_ManualApprovalReview,
        RMResx.RM_DAM_ManualApprovalReview
    ).setComponent(ManualApproval),
    new RouteConfig(
        "PRM_MyRequest",
        RouterUrls.PRM_MyRequest,
        RMResx.RM_Nav_MT_RequestForReview
    ).setComponent(MyRequest),
    new RouteConfig(
        "MT_PickListForLoanRequests",
        RouterUrls.MT_PickListForLoanRequests,
        RMResx.RM_MT_PickList_LoanRequests
    ).setComponent(PickListForLoanRequests),
    new RouteConfig(
        "MT_PickListForDestruction",
        RouterUrls.MT_PickListForDestruction,
        RMResx.RM_MT_PickList_Destruction
    ).setComponent(PickListForDestruction),
    new RouteConfig(
        "MT_PickListForMovement",
        RouterUrls.MT_PickListForMovement, 
        RMResx.RM_MT_PickList_Movement
    ).setComponent(PickListForMovement),
    new RouteConfig(
        "MT_MachineLearningReview",
        RouterUrls.MT_MachineLearningReview,
        RMResx.RM_MT_MachineLearningReview
    ).setComponent(MachineLearningReview)
    .setShowInNav(true), // !EnvironmentHelper.IsGCPEnvironment
);
export default MTRouteConfig;

import RouterUrls from '../Constants/RouterUrls';
// import CreateRule from '../Components/RDM/RuleManageMent/CreateRule';
import ManualApprovalReview from '../Components/RDM/ManualApprovalReview/ManualApprovalReview';
import MAProcessesManagement from '../Components/RDM/MAProcessesManagement/MAProcessesManagement';
import ViewWorkFlow from '../Components/RDM/MAProcessesManagement/WorkFlow/ViewWorkFlow';
import CreateWorkFlow from '../Components/RDM/MAProcessesManagement/WorkFlow/CreateWorkflow';
import RouteConfig from '../Components/Base/RouteConfig';
import RuleManagement from '../Components/RDM/RuleManageMent/RuleManagement';

const RDMRouterConfig = new RouteConfig("RDM", RouterUrls.RDM, RMResx.RM_Home_Module_RetentionDisposal, 'faui-home')
    .setTooltip(RMResx.RM_Home_Module_RetentionDisposalDesc)
    .addChildren(
        new RouteConfig("RDM_RuleManagement", RouterUrls.RDM_RuleManagement, RMResx.RM_RDM_RuleManagement)
            .setComponent(RuleManagement),
        // new RouteConfig("RDM_RuleManagement", RouterUrls.RDM_CreateRule, RMResx.RM_JS_Common_Create)
        //     .setComponent(CreateRule).setShowInNav(false),
        // new RouteConfig("RDM_RuleManagement", RouterUrls.RDM_EditRule, RMResx.RM_JS_Common_Edit)
        //     .setComponent(CreateRule).setShowInNav(false),
        new RouteConfig("RDM_ManualApprovalReview", RouterUrls.RDM_ManualApprovalReview, RMResx.RM_DAM_ManualApprovalReview)
            .setComponent(ManualApprovalReview),
        new RouteConfig("RDM_WorkFlowManagement", RouterUrls.RDM_WorkFlowManagement, RMResx.RM_RDM_WorkFlowManagement)
            .setComponent(MAProcessesManagement),
        new RouteConfig("RDM_WorkFlowManagement", RouterUrls.RDM_ViewWorkFlow, RMResx.RM_RDM_WorkFlow_ViewDetail)
            .setComponent(ViewWorkFlow).setShowInNav(false),
        new RouteConfig("RDM_WorkFlowManagement", RouterUrls.RDM_CreateWorkFlow, RMResx.RM_RDM_CreateWorkFlow)
            .setComponent(CreateWorkFlow).setShowInNav(false)
    );

export default RDMRouterConfig;
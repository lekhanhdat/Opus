﻿import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import DueDisposalReportManagement from "../Components/RC/DueDisposalReport/Management";
import DueDisposalReportProfile from "../Components/RC/DueDisposalReport/Profile";
import DueDisposalShowReport from "../Components/RC/DueDisposalReport/ShowReport";
import DueDisposalReportViewDetail from "../Components/RC/DueDisposalReport/ViewDetail";
import CreationAndDestructionReportManagement from "../Components/RC/CreationAndDestructionReport/Management";
import CreationAndDestructionProfile from "../Components/RC/CreationAndDestructionReport/Profile";
import CreationAndDestructionShowReport from "../Components/RC/CreationAndDestructionReport/ShowReport";
import CreationAndDestructionViewDetail from "../Components/RC/CreationAndDestructionReport/ViewDetail";
import TermUsageReportManagement from "../Components/RC/TermUsageReport/Management";
import TermUsageReportProfile from "../Components/RC/TermUsageReport/Profile";
import TermUsageShowReport from "../Components/RC/TermUsageReport/ShowReport";
import TermUsageReportViewDetail from "../Components/RC/TermUsageReport/ViewDetail";
import AvailableSpaceReportManagement from "../Components/RC/AvailiableSpaceReport/Management";
import AvailableSpaceReportProfile from "../Components/RC/AvailiableSpaceReport/Profile";
import AvailableSpaceReportShowReport from "../Components/RC/AvailiableSpaceReport/ShowReport";
import AvailableSpaceReportViewDetail from "../Components/RC/AvailiableSpaceReport/ViewDetail";
import RuleUsageReportManagement from "../Components/RC/RuleUsageReport/Management";
import AuditReport from "../Components/RC/AuditReport/Management";

import { DisposalReportCreate, DisposalReportEdit } from "../Components/ReportCenter/ContentDueForAction/index";
import { CreateAndDestryoedReportCreate, CreateAndDestryoedReportEdit } from "../Components/ReportCenter/CreateAndDestryoed/index";
import { TermUsageReportCreate, TermUsageReportEdit } from "../Components/ReportCenter/TermUsageReport";
import ActionAuditReportManagement from "../Components/RC/ActionAuditReport/Management";
import ActionAuditReportProfile from "../Components/RC/ActionAuditReport/Profile";
import ActionAuditReportViewDetail from "../Components/RC/ActionAuditReport/ViewDetail";
import ActionAuditReportShowReport from "../Components/RC/ActionAuditReport/ShowReport";
import SOReport from "../Components/RC/SOReport";
import SOReportCreateProfile from "../Components/RC/SOReport/Profile/CreateProfile";
import SOReportShowReport from "../Components/RC/SOReport/Profile/ShowReport";

import RestoreReportManagement from "../Components/RC/RestoreReport/Management";
import RestoreReportProfile from "../Components/RC/RestoreReport/Profile";
import RestoreShowReport from "../Components/RC/RestoreReport/ShowReport";
import RestoreReportViewDetail from "../Components/RC/RestoreReport/ViewDetail";
import { RestoreReportCreate, RestoreReportEdit } from "../Components/ReportCenter/Restore/index";

const RCRouterConfig = new RouteConfig(
    "RC",
    RouterUrls.RC,
    RMResx.RM_Home_Module_ReportCenter,
    "--group-3",
    ".fia-reporting-nav"
)
    .addChildren(
        // new RouteConfig("RC_Dashboard",'/RC/Dashboard',RMResx.RM_DSB_PageTitle)
        //     .setIsInternal(false),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalReportManagement,
            RMResx.RM_Nav_RC_ContentDueforAction
        ).setComponent(DueDisposalReportManagement),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalReportViewDetail,
            RMResx.RM_JM_DetailsTitle
        )
            .setComponent(DueDisposalReportViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalReportProfile
        )
            .setExact(false)
            .setComponent(DueDisposalReportProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalReportCreate
        )
            .setExact(false)
            .setComponent(DisposalReportCreate)
            .setShowInNav(false),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalReportEdit
        )
            .setExact(false)
            .setComponent(DisposalReportEdit)
            .setShowInNav(false),
        new RouteConfig(
            "RC_DueDisposalReportManagement",
            RouterUrls.RC_DueDisposalShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(DueDisposalShowReport)
            .setShowInNav(false),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageReportManagement,
            RMResx.RM_Nav_RC_TermUsage
        ).setComponent(TermUsageReportManagement),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageReportProfile
        )
            .setExact(false)
            .setComponent(TermUsageReportProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(TermUsageShowReport)
            .setShowInNav(false),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageReportCreate
        )
            .setExact(false)
            .setComponent(TermUsageReportCreate)
            .setShowInNav(false),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageReportEdit
        )
            .setExact(false)
            .setComponent(TermUsageReportEdit)
            .setShowInNav(false),
        new RouteConfig(
            "RC_TermUsageReportManagement",
            RouterUrls.RC_TermUsageReportViewDetail,
            RMResx.RM_JM_DetailsTitle
        )
            .setComponent(TermUsageReportViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RuleUsageReportManagement",
            RouterUrls.RC_RuleUsageReportManagement,
            RMResx.RM_Nav_RC_RuleUsage
        ).setComponent(RuleUsageReportManagement),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreationAndDestructionReport,
            RMResx.RM_Nav_RC_CreationandDestruction
        ).setComponent(CreationAndDestructionReportManagement),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreationAndDestructionProfile,
            RMResx.RM_JS_Common_Create
        )
            .setExact(false)
            .setComponent(CreationAndDestructionProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreationAndDestructionShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(CreationAndDestructionShowReport)
            .setShowInNav(false),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreationAndDestructionViewDetail,
            RMResx.RM_JM_DetailsTitle
        )
            .setComponent(CreationAndDestructionViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreateAndDestryoedReportCreate
        )
            .setExact(false)
            .setComponent(CreateAndDestryoedReportCreate)
            .setShowInNav(false),
        new RouteConfig(
            "RC_CreationAndDestructionReport",
            RouterUrls.RC_CreateAndDestryoedReportEdit
        )
            .setExact(false)
            .setComponent(CreateAndDestryoedReportEdit)
            .setShowInNav(false),
        new RouteConfig(
            "RC_AvailableSpaceReportManagement",
            RouterUrls.RC_AvailableSpaceReportProfile
        )
            .setExact(false)
            .setComponent(AvailableSpaceReportProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_AvailableSpaceReportManagement",
            RouterUrls.RC_AvailableSpaceReportShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(AvailableSpaceReportShowReport)
            .setShowInNav(false),
        new RouteConfig(
            "RC_AvailableSpaceReportManagement",
            RouterUrls.RC_AvailableSpaceReportDetail
        )
            .setComponent(AvailableSpaceReportViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_AuditReportManagement",
            RouterUrls.RC_AuditReportManagement,
            RMResx.RM_Nav_RC_AdministratorAudit
        ).setComponent(AuditReport),
        new RouteConfig(
            "RC_ActionAuditReportManagement",
            RouterUrls.RC_ActionAuditReportManagement,
            RMResx.RM_Nav_RC_ActionAudit
        ).setComponent(ActionAuditReportManagement),
        new RouteConfig(
            "RC_ActionAuditReportManagement",
            RouterUrls.RC_ActionAuditReportProfile,
            RMResx.RM_JS_Common_Create
        )
            .setExact(false)
            .setComponent(ActionAuditReportProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_ActionAuditReportManagement",
            RouterUrls.RC_ActionAuditReportDetail,
            RMResx.RM_JM_DetailsTitle
        )
            .setComponent(ActionAuditReportViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_ActionAuditReportManagement",
            RouterUrls.RC_ActionAuditReportShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(ActionAuditReportShowReport)
            .setShowInNav(false),

        new RouteConfig(
            "RC_AvailableSpaceReportManagement",
            RouterUrls.RC_AvailableSpaceReportManagement,
            RMResx.RM_Nav_RC_AvailableSpace
        ).setComponent(AvailableSpaceReportManagement),
        new RouteConfig(
            "RC_StorageOptimizationReportManagement",
            RouterUrls.RC_StorageOptimizationReportManagement,
            RMResx.RM_Nav_RC_SOReport
        ).setComponent(SOReport),
        new RouteConfig(
            "RC_StorageOptimizationReportManagement",
            RouterUrls.RC_StorageOptimizationReportProfile
        )
            .setExact(false)
            .setComponent(SOReportCreateProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_StorageOptimizationReportManagement",
            RouterUrls.RC_StorageOptimizationReportShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(SOReportShowReport)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreReportManagement,
            RMResx.RM_Nav_RC_RestoreReport
        ).setComponent(RestoreReportManagement),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreReportViewDetail,
            RMResx.RM_JM_DetailsTitle
        )
            .setComponent(RestoreReportViewDetail)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreReportProfile
        )
            .setExact(false)
            .setComponent(RestoreReportProfile)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreReportCreate
        )
            .setExact(false)
            .setComponent(RestoreReportCreate)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreReportEdit
        )
            .setExact(false)
            .setComponent(RestoreReportEdit)
            .setShowInNav(false),
        new RouteConfig(
            "RC_RestoreReportManagement",
            RouterUrls.RC_RestoreShowReport,
            RMResx.RM_JS_Common_ShowReport
        )
            .setComponent(RestoreShowReport)
            .setShowInNav(false),
    );

export default RCRouterConfig;

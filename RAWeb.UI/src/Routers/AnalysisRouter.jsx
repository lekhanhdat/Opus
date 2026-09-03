import RouteConfig from "../Components/Base/RouteConfig";
import { AnalysisInactivePage, AnalysisReportPage, AnalysisRotPage, AnalysisPlanProfilePage, AnalysisPlanViewPage } from "../Components/DiscoveryAndAnalysis/Analysis";
import { AnalysisConfigurationEditPage, AnalysisConfigurationFinishPage, AnalysisConfigurationInitializationPage, AnalysisConfigurationRunningPage } from "../Components/DiscoveryAndAnalysis/Discovery/AnalysisConfigurator";
import { FSAnalysisConfigurationConnectionPage } from "../Components/DiscoveryAndAnalysis/Discovery/AnalysisConfigurator/FileSystem";
import RouterUrls from "../Constants/RouterUrls";

function getChildren() {
    let children = [];
    children.push(
        new RouteConfig(
            "Discovery",
            RouterUrls.FA_Discovery,
            RMResx.RM_FA_Discovery
        )
            .setComponent(AnalysisConfigurationInitializationPage)
            .setShowInNav(true),
        new RouteConfig(
            "Discovery",
            RouterUrls.FA_Discovery_Configuration,
            ""
        )
            .setComponent(AnalysisConfigurationEditPage)
            .setShowInNav(false),
        new RouteConfig(
            "Discovery",
            RouterUrls.FA_Discovery_Configuration_FSConfigConnection,
            RMResx.RM_FA_Discovery_ConfigConnection
        )
            .setComponent(FSAnalysisConfigurationConnectionPage)
            .setShowInNav(false),
        new RouteConfig(
            "Discovery",
            RouterUrls.FA_Discovery_RunJob,
            ""
        )
            .setComponent(AnalysisConfigurationRunningPage)
            .setShowInNav(false),
        new RouteConfig(
            "Discovery",
            RouterUrls.FA_Discovery_Finish,
            ""
        )
            .setComponent(AnalysisConfigurationFinishPage)
            .setShowInNav(false),
        new RouteConfig(
            "Inactive",
            RouterUrls.FA_Inactive,
            RMResx.RM_FA_Inactive
        )
            .setComponent(AnalysisInactivePage)
            .setShowInNav(true),
        new RouteConfig(
            "ROT",
            RouterUrls.FA_ROT,
            RMResx.RM_FA_ROT
        )
            .setComponent(AnalysisRotPage)
            .setShowInNav(true),
        new RouteConfig(
            "PlanProfile",
            RouterUrls.FA_Plan_Profile,
            RMResx.RM_FA_Plan_Profile
        )
            .setComponent(AnalysisPlanProfilePage)
            .setShowInNav(true),
       new RouteConfig(
            "PlanView",
            RouterUrls.FA_Plan_PlanView,
            "^View Plan"
        )
            .setComponent(AnalysisPlanViewPage)
            .setShowInNav(false),
        new RouteConfig(
            "Progress",
            RouterUrls.FA_Discovery_Progress,
            RMResx.RM_FA_Progress
        )
            .setComponent(AnalysisReportPage)
            .setShowInNav(true)
    );
    return children;
}

const FileAnalysisRouterConfig = new RouteConfig(
    "FA",
    RouterUrls.FA,
    RMResx.RM_Nav_FileAnalysis,
    "--group-3",
    ".fia-discovery_and_analysis",
).addChildren(...getChildren());

export default FileAnalysisRouterConfig;
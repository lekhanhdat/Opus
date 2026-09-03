import RouterUrls from "../Constants/RouterUrls";
import ContainerSize from "../Components/PRM/ContainerSize";
import PhysicalRecordsBulkImport from "../Components/PRM/PhysicalRecordsBulkImport";
import RecordsExplorer from "../Components/PRM/RecordsExplorer/RecordsExplorer";
import PhyManageHold from "../Components/PRM/RecordsExplorer/ManageHold/PhyManageHold";
import ImportHPTRIM from "../Components/PRM/Import/ImportHPTRIM";
import RouteConfig from "../Components/Base/RouteConfig";

const PRMRouterConfig = new RouteConfig(
    "PRM_RecordsExplorer",
    RouterUrls.PRM_RecordsExplorer,
    RMResx.RM_PRM_RecordsExplorer_PageTitle,
    RMResx.RM_Nav_PhysicalRecords,
    ".fia-explorer"
)
    .setComponent(RecordsExplorer)
    .addChildren(
        new RouteConfig(
            "PRM_RecordsExplorer",
            RouterUrls.PRM_ContainerSize,
            RMResx.RM_CZ_PageTitle
        )
            .setComponent(ContainerSize)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_RecordsExplorer",
            RouterUrls.PRM_PhysicalRecordsBulkImport,
            RMResx.RM_PRM_PhysicalRecordsImport_PageTitle
        )
            .setComponent(PhysicalRecordsBulkImport)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_RecordsExplorer",
            RouterUrls.PRM_ManageHold,
            RMResx.RM_JS_RDM_Hold_ManageHoldTitle
        )
            .setComponent(PhyManageHold)
            .setShowInNav(false)
            .setExact(false),
        new RouteConfig(
            "PRM_RecordsExplorer",
            RouterUrls.PRM_ImportHPRM
        )
            .setComponent(ImportHPTRIM)
            .setShowInNav(false),
    );

export default PRMRouterConfig;

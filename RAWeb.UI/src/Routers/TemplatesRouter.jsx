import RouterUrls from "../Constants/RouterUrls";
import TemplateManagement from "../Components/PRM/TemplateManagement";
import BarcodeManagement from "../Components/PRM/BarcodeManagement";
import CreateBarcodeTemplate from "../Components/PRM/BarcodeManagement/Create";
import EditBarcodeTemplate from "../Components/PRM/BarcodeManagement/EditDefault";
import EditTemplate from "../Components/PRM/EditTemplate";
import CreateTemplateSuite from "../Components/PRM/TemplateSuite/CreateTemplateSuite";
import FolderTemplateManagement from "../Components/PRM/FolderTemplateManagement";
import RecordTemplateManagement from "../Components/PRM/RecordTemplateManagement";
import BarcodeTemplate from "../Components/PRM/BarcodeTemplate/BarcodeTemplate";
import RouteConfig from "../Components/Base/RouteConfig";

const TemplatesRouterConfig = new RouteConfig(
    "PRM",
    RouterUrls.PRM,
    RMResx.RM_Nav_PR_TemplateManager,
    RMResx.RM_Nav_PhysicalRecords,
    ".fia-template"
)
    // .setComponent(TemplateManagement)
    .addChildren(
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_TemplateManagement,
            RMResx.RM_PRM_TM_Records_Template
        )
            .setComponent(TemplateManagement),
        new RouteConfig(
            "PRM_BarcodeManagement",
            RouterUrls.PRM_BarcodeManagement,
            RMResx.RM_PRM_TM_Barcode_Template
        )
            .setComponent(BarcodeManagement),
        new RouteConfig(
            "PRM_BarcodeManagement",
            RouterUrls.PRM_BarcodeManagement_Create,
            RMResx.RM_PRM_TM_Barcode_Template_Create
        )
            .setComponent(CreateBarcodeTemplate)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_BarcodeManagement",
            RouterUrls.PRM_BarcodeManagement_Edit,
            RMResx.RM_PRM_TM_Barcode_Template_Edit
        )
            .setExact(false)
            .setComponent(CreateBarcodeTemplate)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_BarcodeManagement",
            RouterUrls.PRM_BarcodeManagement_EditDefault,
            RMResx.RM_PRM_TM_Barcode_Template_EditDefault
        )
            .setExact(false)
            .setComponent(EditBarcodeTemplate)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_EditTemplate,
            RMResx.RM_PRM_TM_EditTemplate_PageTitle
        )
            .setComponent(EditTemplate)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_CreateTemplate
        )
            .setComponent(EditTemplate)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_CreateTemplateSuite,
            RMResx.RM_PRM_TM_Btn_NewSuite
        )
            .setComponent(CreateTemplateSuite)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_EditTemplateSuite,
            RMResx.RM_PRM_TM_EditTemplateSuite_PageTitle
        )
            .setComponent(CreateTemplateSuite)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_FolderTemplateManagement,
            RMResx.RM_PRM_TM_FolderTemplateManagement_PageTitle
        )
            .setComponent(FolderTemplateManagement)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_RecordTemplateManagement,
            RMResx.RM_PRM_TM_RecordTemplateManagement_PageTitle
        )
            .setComponent(RecordTemplateManagement)
            .setShowInNav(false),
        new RouteConfig(
            "PRM_TemplateManagement",
            RouterUrls.PRM_BarcodeTemplate,
            RMResx.RM_PRM_BarcodeTemplate
        )
            .setComponent(BarcodeTemplate)
            .setShowInNav(false)
    );

export default TemplatesRouterConfig;
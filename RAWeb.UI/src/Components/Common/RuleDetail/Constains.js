import RuleCriterias from "./RDComponent/RDSourceTabRows/RuleCriterias";
import RuleAction from "./RDComponent/RDSourceTabRows/RuleAction";
import ManualApprove from "./RDComponent/RDSourceTabRows/ManualApprove";
import Export from "./RDComponent/RDSourceTabRows/Export";
import ArchiverStorage from "./RDComponent/RDSourceTabRows/ArchiverStorage";

const RuleLevels = {
    0: RMResx.RM_JS_Rule_ObjectLevel_None,
    1: RMResx.RM_JS_Rule_ObjectLevel_WebApplication,
    2: RMResx.RM_JS_Rule_ObjectLevel_SiteCollection,
    4: RMResx.RM_JS_Rule_ObjectLevel_Site,
    8: RMResx.RM_JS_Rule_ObjectLevel_List,
    16: RMResx.RM_JS_Rule_ObjectLevel_Folder,
    32: RMResx.RM_JS_Rule_ObjectLevel_Item,
    64: RMResx.RM_JS_Rule_ObjectLevel_Document,
    128: RMResx.RM_JS_Rule_ObjectLevel_Attachment,
    256: RMResx.RM_JS_Rule_ObjectLevel_DocumentVersion,
    512: RMResx.RM_JS_Rule_ObjectLevel_ItemVersion,
    33554432: RMResx.RM_JS_Rule_ObjectLevel_Teams,
};

const RuleSourceTabIndex = {
    SP: 0,
    OneDrive: 1,
    Exchange: 2, 
    Physical :3,
    FS: 4,
    SPLocal: 5,
    AzureFile: 6,
    Box: 7,
    Connector: 8,
    GoogleDrive: 9,
    Teams: 10,
};

const RDSourceComponentType = {
    RuleCriterias: 1,
    RuleAction: 2,
    ManualApprove: 3,
    Export: 4,
    ArchiverStorage: 5,
};

const RDSourceComponents = {
    [RDSourceComponentType.RuleCriterias]: RuleCriterias,
    [RDSourceComponentType.RuleAction]: RuleAction,
    [RDSourceComponentType.ManualApprove]: ManualApprove,
    [RDSourceComponentType.Export]: Export,
    [RDSourceComponentType.ArchiverStorage]: ArchiverStorage,
};

const ExportType = {
    "-1": RMResx.RM_JS_RDM_CreateRule_ExportType_None,
    "0": RMResx.RM_JS_RDM_CreateRule_ExportType_Autonomy,
    "1": RMResx.RM_JS_RDM_CreateRule_ExportType_Concordance,
    "2": RMResx.RM_JS_RDM_CreateRule_ExportType_EDRM,
    "3": RMResx.RM_JS_RDM_CreateRule_ExportType_VEO,
    "4": RMResx.RM_JS_RDM_CreateRule_ExportType_NAA,
    "5": RMResx.RM_JS_RDM_CreateRule_ExportType_NARA
};

const RuleModules = {
    1: RMResx.RM_AR_SPS_TabControl_Information,
    2: RMResx.RM_AR_SPS_TabControl_Storage
};

const RuleModuleTypes = {
    None: 0,
    Records: 1,
    SOArchiver: 2
};


export {RuleLevels, RuleSourceTabIndex, RDSourceComponentType, RDSourceComponents, ExportType, RuleModuleTypes, RuleModules};
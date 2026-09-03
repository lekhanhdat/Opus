// import React from "react";
// import { Route, Switch } from "react-router-dom";
import RouterUrls from "../Constants/RouterUrls";
import RouteConfig from "../Components/Base/RouteConfig";
import CP from "../Components/CP/CP";
import GeneralSetting from "../Components/CP/GeneralSetting";
import ExportSettings from "../Components/CP/ExportSettings";
import CompliantExports from "../Components/CP/CompliantExports";
import DashboardSettings from "../Components/CP/DashboardSettings";
import TimerJobSettings from "../Components/CP/TimerJobSettings";
import EmailTemplate from "../Components/CP/EmailTemplate/Index";
import EditEmailTemplate from "../Components/CP/EditEmailTemplate";
import AccountManagement from "../Components/CP/AccountManagement/Management";
import AgentManagement from "../Components/CP/AgentManagement/Management";
import CSDApiKeyManagement from "../Components/CP/CSDApiKeyManagement/Management";
import ConfigurationStorageSettings from "../Components/CP/ConfigurationStorageSettings/Management";
import StubSettings from "../Components/CP/StubSettings/Management";
import {
    SourceConnectorCreateOrEdit,
    SourceConnectorList
} from "../Components/Customization/SourceConnector/index";
import EndUserRestoreSetting from "../Components/CP/EndUserRestoreSetting";
import JobNotification from "../Components/CP/JobNotificationManagement/JobNotification";
import MultiGeo from "../Components/CP/MultiGeoSettings/MultiGeo";

const navId = "CP";
const CPRouteConfig = new RouteConfig(
    navId,
    RouterUrls.CP_Index,
    RMResx.RM_Nav_Settings,
    RMResx.RM_NavPanel_Group_System,
    ".fia-settings"
)
    .setComponent(CP)
    .addChildren(
        new RouteConfig(navId, RouterUrls.CP_GeneralSetting, RMResx.RM_GS_Title)
            .setComponent(GeneralSetting)
            .setShowInNav(false),
            
        new RouteConfig(
            navId,
            RouterUrls.CP_StorageSettings,
            RMResx.RM_JS_CP_StorageSetting
        )
            .setComponent(ConfigurationStorageSettings)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.Connector,
            RMResx.RM_CP_Connector
        )
            .setComponent(SourceConnectorList)
            .setShowInNav(false)
            .setExact(true),
        new RouteConfig(
            navId,
            RouterUrls.Connector_CreateOrEdit,
            ""
        )
            .setComponent(SourceConnectorCreateOrEdit)
            .setShowInNav(false)
            .setExact(false),
        new RouteConfig(navId, RouterUrls.CP_ExportSettings, RMResx.RM_ES_Title)
            .setComponent(ExportSettings)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_ExportSettings_CompliantExports,
            RMResx.RM_ES_CompliantExport_Title
        )
            .setComponent(CompliantExports)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_DashboardSettings,
            RMResx.RM_JS_CP_DS_Title
        )
            .setComponent(DashboardSettings)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_TimerJobSettings,
            RMResx.RM_CP_TimerJob
        )
            .setComponent(TimerJobSettings)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_EmailTemplate,
            RMResx.RM_CP_EmailTemplate
        )
            .setComponent(EmailTemplate)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_EditEmailTemplate,
            RMResx.RM_CP_EditEmailTemplate
        )
            .setExact(false)
            .setComponent(EditEmailTemplate)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_CreateEmailTemplate,
            RMResx.RM_JS_CP_EamilTemplate_CreateTemplate
        )
            .setExact(false)
            .setComponent(EditEmailTemplate)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_AccountManagement,
            RMResx.RM_CP_AccountManagement
        )
            .setComponent(AccountManagement)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_AgentManagement,
            RMResx.RM_CP_Agent_Management
        )
            .setComponent(AgentManagement)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_CSDApiKeyManagement,
            RMResx.RM_CP_Agent_Management
        )
            .setComponent(CSDApiKeyManagement)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_StubSettings,
            RMResx.RM_AR_CP_StubSettings
        )
            .setComponent(StubSettings)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_EndUserRestore,
            RMResx.RM_AR_CP_RestoreSetting
        )
            .setComponent(EndUserRestoreSetting)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_JobNotification,
            RMResx.RM_AR_CP_JobNotification
        )
            .setComponent(JobNotification)
            .setShowInNav(false),
        new RouteConfig(
            navId,
            RouterUrls.CP_MultiGeo,
            RMResx.RM_AR_CP_Multi_Geo
        )
            .setComponent(MultiGeo)
            .setShowInNav(false),
    );

export default CPRouteConfig;

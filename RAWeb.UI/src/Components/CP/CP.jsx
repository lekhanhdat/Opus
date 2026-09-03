import { Component } from "react";
import { withRouter } from "react-router-dom";
import RouterUrls from "../../Constants/RouterUrls";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import { checkPermission } from "../../Utilities/permissionManager";
import { checkIsCSDTenant } from "../../Components/Common/Util/CommonApiUtil";
import "../../Less/CP/cp.less";
import { EnvironmentHelper, LicenseHelper } from "../../Utilities/CommonUtil";

class CPCell extends Component {
    constructor(props) {
        super(props);
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl || this.props.routerUrl,
        });
    }

    onCellClick = () => {
        this.routerTo();
    };

    onCellKeyDown = (e) => {
        if (e.keyCode == "13") {
            this.routerTo();
        }
    };

    render() {
        return (
            <a
                className="ra-cp-cell"
                tabIndex="0"
                onClick={this.onCellClick}
                onKeyDown={this.onCellKeyDown}
                role="Link"
            >
                <div className="ra-cp-cell-header-center">
                    <div className="ra-cp-cell-header-icon">
                        <div className={this.props.icon}></div>
                    </div>
                    <div
                        className="ra-cp-cell-header-name"
                        aria-label={this.props.title}
                    >
                        {this.props.title}
                    </div>
                </div>
                <div className="ra-cp-cell-line"></div>
                <div className="ra-cp-cell-body">
                    <p
                        className="ra-cp-cell-body-desc"
                        data-tooltip="ifneed"
                        aria-label={this.props.description}
                    >
                        {this.props.description}
                    </p>
                </div>
            </a>
        );
    }
}

const CellWithRouter = withRouter(CPCell);

export default class CP extends Component {
    currentMainWidth = 0;
    resizeHandler = null;

    constructor(props) {
        super(props);
        this.state = { isCSDTenant: false };
    }

    componentDidMount() {
        this.resizeHandler = setInterval(this.checkMainWidth, 50);
        checkIsCSDTenant()
            .then((res) => {
                this.setState({ isCSDTenant: res });
            })
            .catch((e) => {
                //console.log(e);
            });
    }

    componentDidUpdate() {
        this.checkMainWidth(this.state.isCSDTenant);
    }

    componentWillUnmount() {
        clearInterval(this.resizeHandler);
    }

    checkMainWidth = (force) => {
        let mainWidth = $("#rmCPMain").width() - 30;
        if (force || mainWidth != this.currentMainWidth) {
            this.currentMainWidth = mainWidth;
            this.modulesResize(mainWidth);
        }
    };

    getCurrentWidth(
        controlMainWidth,
        calcByMinWithCountCount,
        minWidth,
        moduleMargin,
        precision
    ) {
        switch (precision) {
            case 0:
                precision = 1;
                break;
            case 1:
                precision = 10;
                break;
            case 2:
                precision = 100;
                break;
        }
        let currentWidth =
            minWidth +
            Math.floor(
                ((controlMainWidth -
                    calcByMinWithCountCount * (minWidth + moduleMargin)) /
                    calcByMinWithCountCount) *
                    precision
            ) /
                precision;
        return currentWidth;
    }

    modulesResize(controlMainWidth) {
        let $modules = $(".ra-cp-cell"),
            totalCount = $modules.length,
            finalWidth = 0,
            minWidth = 240,
            maxWidth = 300,
            moduleMargin = 18,
            calcByMinWithCount = Math.floor(
                controlMainWidth / (minWidth + moduleMargin)
            );
        if (calcByMinWithCount >= totalCount) {
            //all modules in a line.
            finalWidth = Math.min(
                this.getCurrentWidth(
                    controlMainWidth,
                    calcByMinWithCount,
                    minWidth,
                    moduleMargin,
                    2
                ),
                maxWidth
            );
        } else if (calcByMinWithCount < 2) {
            finalWidth = maxWidth;
        } else {
            finalWidth = this.getCurrentWidth(
                controlMainWidth,
                calcByMinWithCount,
                minWidth,
                moduleMargin,
                2
            );
        }

        $(".ra-cp-cell").width(finalWidth);
    }

    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl,
        });
    }

    render() {
        // eslint-disable-next-line no-unused-vars
        let isAllowShowAgentManagementCard =
            ((checkPermission("Source_FS", RM.UserResources) ||
                checkPermission("CP_Schedule_Settings_On_Prem", RM.UserResources))
            || RM.gData.hasDiscoveryFileSystemLicense)
            && !EnvironmentHelper.IsGCPEnvironment;
        let isOnlyHasDiscoveryPermission = LicenseHelper.HasDiscoveryLicenseOnly();
        let isAllowShowRestoreSettingCard = checkPermission(
            RouterUrls.CP_EndUserRestore,
            RM.UserResources
        ) && !EnvironmentHelper.IsGCPEnvironment;
        let isAllowShowConnectorCard = checkPermission(
            RouterUrls.Connector,
            RM.UserResources
        );
        let isAllowShowEmailTemplateCard = checkPermission(
            RouterUrls.CP_EmailTemplate,
            RM.UserResources
        );
        let isAllowShowAccountManagementCard = checkPermission(
            RouterUrls.CP_AccountManagement,
            RM.UserResources
        );
        let isAllowExportSettingsCard = checkPermission(
            RouterUrls.CP_ExportSettings,
            RM.UserResources
        );
        let isAllowStubCard = checkPermission(
            RouterUrls.CP_StubSettings,
            RM.UserResources
        );
        let isAllowStorageCard = checkPermission(
            RouterUrls.CP_StorageSettings,
            RM.UserResources
        );

        let isAllowTimerJob = false;
        if(LicenseHelper.EnableRecordsArchiver()){
            if(LicenseHelper.HasOpusSOLicense() || LicenseHelper.HasOpusGoogleLicense()){
                isAllowTimerJob = true;
            }
        }else{
            if(LicenseHelper.HasOpusILLicense()){
                isAllowTimerJob = true;
            }
        }

        const isJPMCFeatureEnabled = (RM?.gData?.enableJPMCFileSystemFeature && RM?.gData?.isMultiGeoMainDC) ?? false;

        return (
            <div id="rmCP">
                <$g.SiteMap data={[SiteMapLinks.CP]} />
                <div className="ra-page-main">
                    <div id="rmCPMain">
                        <CellWithRouter
                            routerUrl={RouterUrls.CP_GeneralSetting}
                            icon="fia-gear"
                            title={RMResx.RM_GS_Title}
                            description={RMResx.RM_GS_Description}
                        />
                        {!isOnlyHasDiscoveryPermission && (
                            <>
                                {isAllowStorageCard && <CellWithRouter
                                    routerUrl={RouterUrls.CP_StorageSettings}
                                    icon="fia-storage"
                                    title={RMResx.RM_JS_CP_StorageSetting}
                                    description={
                                        RMResx.RM_CP_StorageSettingDesc
                                    }
                                />}
                                {isAllowExportSettingsCard && (
                                    <CellWithRouter
                                        routerUrl={RouterUrls.CP_ExportSettings}
                                        icon="fia-gear"
                                        title={RMResx.RM_ES_Title}
                                        description={RMResx.RM_ES_TitleDescription}
                                    />
                                )}
                                {isAllowTimerJob && <CellWithRouter
                                    routerUrl={RouterUrls.CP_TimerJobSettings}
                                    icon="fia-calendar"
                                    title={RMResx.RM_CP_TimerJob}
                                    description={RMResx.RM_CP_TimerJobDesc}
                                />}
                            </>
                        )}
                        {isAllowShowEmailTemplateCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_EmailTemplate}
                                icon="fia-email"
                                title={RMResx.RM_CP_EmailTemplate}
                                description={RMResx.RM_CP_EmailTemplateDesc}
                            />
                        )}
                        {isAllowShowAccountManagementCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_AccountManagement}
                                icon="fia-user"
                                title={RMResx.RM_CP_AccountManagement}
                                description={
                                    RMResx.RM_CP_AccountManagement_Explain
                                }
                            />
                        )}
                        {isAllowShowConnectorCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.Connector}
                                icon="fia-connecter"
                                title={RMResx.RM_CP_Connector}
                                description={RMResx.RM_CP_Connector_Description}
                            />
                        )}
                        {isAllowShowAgentManagementCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_AgentManagement}
                                icon="fia-agent"
                                title={RMResx.RM_CP_Agent_Management}
                                description={
                                    RMResx.RM_CP_Agent_Management_Introduce
                                }
                            />
                        )}
                        {this.state.isCSDTenant && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_CSDApiKeyManagement}
                                icon="fia-api"
                                title={RMResx.RM_CP_CSDAK_Management}
                                description={RMResx.RM_CP_CSDAK_ManagementDesc}
                            />
                        )}
                        {!isOnlyHasDiscoveryPermission && isAllowStubCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_StubSettings}
                                icon="fia-stub-settings"
                                title={RMResx.RM_AR_CP_StubSettings}
                                description={RMResx.RM_AR_CP_StubSettingsDesc}
                            />
                        )}
                        {isAllowShowRestoreSettingCard && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_EndUserRestore}
                                icon="fia-end-user-restore-settings"
                                title={RMResx.RM_AR_CP_RestoreSetting}
                                description={RMResx.RM_AR_CP_RestoreSettingDesc}
                            />
                        )}
                        <CellWithRouter
                            routerUrl={RouterUrls.CP_JobNotification}
                            icon="fia-notification"
                            title={RMResx.RM_AR_CP_JobNotification}
                            description={RMResx.RM_AR_CP_JobNotificationDesc}
                        />
                        {isJPMCFeatureEnabled && (
                            <CellWithRouter
                                routerUrl={RouterUrls.CP_MultiGeo}
                                icon="fia-multi-geo"
                                title={RMResx.RM_AR_CP_Multi_Geo}
                                description={RMResx.RM_AR_CP_Multi_Geo_Desc}
                            />
                        )}
                    </div>
                </div>
            </div>
        );
    }
}

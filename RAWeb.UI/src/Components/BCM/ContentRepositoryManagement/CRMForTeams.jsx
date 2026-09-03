import { NodeIconClass } from "../../../Constants/DAEnums";
import { checkPermission } from "../../../Utilities/permissionManager";
import ArchiveCRMForTeams from "./CRMForTeams/ArchiveCRMForTeams";
import ContentRepositoryManagementForTeams from "./CRMForTeams/ContentRepositoryManagementForTeams";
import { showToast } from "../../../Utilities/CommonUtil";
import { RAMessageType } from "./Common/CRMCommonUtil";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import "../../../Less/BCM/ContentRepositoryManagement/crmForTeams.less";
import SiteMapLinks from "../../../Constants/SiteMapLinks";
import RouterUrls from "../../../Constants/RouterUrls";
import { StatusCode } from "./SwitchForTeams/Constants";
import { isMoreThanCustomDaysOld } from "../../../Utilities/DateUtil";

export const TabIndex = {
    Records: 0,
    Archive: 1,
};
export default class CRMForTeams extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.isRecordsPermission = checkPermission("RECO_ContentSource_Tab", RM.UserResources);
        this.isArchiverPermission = checkPermission("Archiver_ContentSource_Tab", RM.UserResources);
        this.state = {
            tabIndex: this.isRecordsPermission ? TabIndex.Records : TabIndex.Archive,
            tabs: [
                { head: RMResx.RM_AR_SPS_TabControl_Information },
                { head: RMResx.RM_AR_SPS_TabControl_Storage }
            ],
            isMigrated: false,
            isCheckingMigrate: true,
            moveSCItems: [
                {
                    text: RMResx.RM_AR_Teams_MigratePage_Radio01,
                    value: 1,
                    checked: true,
                },
                {
                    text: RMResx.RM_AR_Teams_MigratePage_Radio02,
                    value: 2,
                    checked: false,
                },
            ],
            moveSCValue: 1,
            showSwitchDialog: false,
        };
    }

    componentInit() {
        this.onCheckHasUpgradeTeams();
    }

    onCheckHasUpgradeTeams = () => {
        $$.loading(true);
        const option = {
            url: "/api/TeamsSettingApi/HasUpgradeTeams",
            method: "GET",
        };
        fetchUtility(option)
            .then((res) => {
                this.setState({
                    isMigrated: res,
                    isCheckingMigrate: false,
                });
            })
            .finally(() => $$.loading(false));
    }

    onTabControl() {
        return <R.Tabcontrol flex active={this.state.tabIndex} onChange={this.handleSelectedTabChanged.bind(this)}>
            {this.state.tabs.map((tab, index) => {
                return (
                    <R.TabPanel key={index} tab={tab.head}></R.TabPanel>
                );
            })}
        </R.Tabcontrol>;
    }

    handleUpgrade = () => {
        const option = {
            url: "/api/TeamsSettingApi/GetTeamsChannelConflictCheckJobInfo",
            method: "GET",
        };
        $$.loading(true);
        fetchUtility(option)
            .then((res) => {
                // !isMoreThanCustomDaysOld(res.StartTime, 2)
                if (res && [StatusCode.InProgress, StatusCode.Waiting, StatusCode.Finished].includes(res.Status)) {
                    this.props.history.push({
                        pathname: RouterUrls.BCM_ContentRepositoryManagement_Teams_Switch,
                    });
                } else {
                    this.setState({ showSwitchDialog: true });
                }
            })
            .finally(() => $$.loading(false));
    }

    onMigrateMessageBox = () => {
        const args = {
            width: "550px",
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_AR_Teams_MigratePage_ConfirmMsg,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_No, onClick: () => $$.messagedialog(false),
                },
                {
                    id: "crmTeamsRunMigrate",
                    text: RMResx.RM_JS_Common_Yes,
                    primary: true,
                    classify: "theme",
                    onClick: this.onMigrate,
                },
            ]
        }
        $$.messagedialog(true, args);
    }

    onMigrate = () => {
        const option = {
            url: "/api/TeamsSettingApi/UpgradeTeams",
            method: "POST",
            data: false,
        };
        $$.loading(true)
        fetchUtility(option)
            .then((res) => {
                if (res.MessageType == RAMessageType.Successful) {
                    const content = (
                        <$g.I18NProvider msg={RMResx.RM_AR_Teams_MigratePage_RunJobSuccess}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>
                    );
                    this.setState({
                        isMigrated: true,
                    });
                    showToast.success(content);
                } else if (res.MessageType == RAMessageType.Failed) {
                    showToast.error(res.ErrorMessage);
                }
            })
            .finally(() => {
                $$.loading(false);
                this.handleCloseSwitchDialog();
            });
    }

    handleSelectedTabChanged(newIndex) {
        this.setState({ tabIndex: newIndex });
    }

    handleCloseSwitchDialog = () => {
        this.setState({ showSwitchDialog: false });
    }

    handleStart = () => {
        if (this.state.moveSCValue === 1) {
            this.onMigrateMessageBox();
            return;
        }
        this.props.history.push({
            pathname: RouterUrls.BCM_ContentRepositoryManagement_Teams_Switch,
        });
    }

    renderSwitchTeamsDialog = () => {
        return (
            <R.Dialog
                id="raCrmSwitchToTeams"
                header={RMResx.RM_AR_Teams_MigratePage_DialogTitle}
                width={500}
                height={346}
                status={{ show: this.state.showSwitchDialog }}
                struct={{ foot: true }}
                destroy={true}
                closeable={true}
                onHide={this.handleCloseSwitchDialog}
            >
                <div className="flex flex-column gap-m">
                    <p style={{ margin: 0 }}>
                        {RMResx.RM_AR_Teams_MigratePage_DialogDesc}
                    </p>
                    <R.Radio.Group
                        block
                        name="moveSCRadio"
                        items={this.state.moveSCItems}
                        onChange={(value) => this.setState({ moveSCValue: value })}
                    />
                </div>
                <R.Button slot="buttons" classify="blank" text={RMResx.RM_JS_Common_Cancel} onClick={this.handleCloseSwitchDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_AR_Teams_MigratePage_StartBtn} onClick={this.handleStart} />
            </R.Dialog>
        );
    }

    render() {
        if (this.state.isCheckingMigrate) {
            return null;
        }

        if (this.state.isMigrated) {
            return <div style={{ height: "100%" }}>
                {this.state.tabIndex == TabIndex.Records && <ContentRepositoryManagementForTeams
                    tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
                ></ContentRepositoryManagementForTeams>}
                {this.state.tabIndex == TabIndex.Archive && <ArchiveCRMForTeams
                    tabControl={this.isRecordsPermission && this.isArchiverPermission ? this.onTabControl() : false}
                ></ArchiveCRMForTeams>}
            </div>;
        }

        return (
            <>
                <section className="crm-header">
                    <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_Teams]} />
                </section>
                <section className="crm-content">
                    <div id="crmForTeams" className="flex flex-column align-center bg-white">
                        <h3 tabIndex={0}>{RMResx.RM_AR_Teams_MigratePage_Title}</h3>
                        <div className="flex justify-center align-center teams-icon-wrapper">
                            <div className={`teams-icon ra-tree-icon ${NodeIconClass.TeamsFarm}`} tabIndex={0} aria-label={RMResx.RM_AR_Teams_MigratePage_Icon}></div>
                        </div>
                        <div className="flex flex-column gap-l">
                            <div className="text-center" tabIndex={0}>{RMResx.RM_AR_Teams_MigratePage_Desc01}</div>
                            <div className="text-center" tabIndex={0}>{RMResx.RM_AR_Teams_MigratePage_Desc02}</div>
                        </div>
                        <div>
                            <img src={`${RM.gData.resCdnURL}/cloud%20records/teams-upgrade.svg`} alt="" />
                        </div>

                        <R.Button
                            id="raMigrateBtn"
                            primary={true}
                            classify="theme"
                            text={RMResx.RM_AR_Teams_MigratePage_MigrateBtn}
                            onClick={this.handleUpgrade}
                        />
                    </div>
                </section>
                {this.renderSwitchTeamsDialog()}
            </>
        );
    }
}
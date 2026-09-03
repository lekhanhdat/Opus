import SiteMapLinks from "../../../Constants/SiteMapLinks";
import { NodeLevel } from "../../../Constants/DAEnums";
import SPCRMTree from "../../Common/Tree/Instances/SPTree/CRMSPTree";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import ColumnSettingComponent from './ColumnSetting/ColumnSettingComponent';
import ManualApprovalSettingComponent from "./ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import GeneralManagementComponent from "./GeneralManagement/GeneralManagementComponent";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import { LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import ValidateMessageBar from "./Common/CommonMessageBar/ValidateMessageBar";
import SharePointOnPremDocumentTerm from "./DocumentTermSetting/Context/SharePointOnPremDocumentTerm";
import CheckLocalNodeMessageBar from "./Common/CommonMessageBar/CheckLocalNodeMessageBar";
import SPOnPremScheduleSetting from "./ScheduleSetting/Context/SPOnPremScheduleSetting";
import SPOnPremManualApprovalSetting from "./ManualApprovalSetting/Context/SPOnPremManualApprovalSetting";
import UniqueIdSettingCommon from "./UniqueIdSetting/UniqueIdSettingCommon";
import SPOnPremColumnSetting from "./ColumnSetting/Context/SPOnPremColumnSetting";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import SPOnPremGeneralManagement from "./GeneralManagement/Context/SPOnPremGeneralManagement";
import SPOnPremUniqueIdSetting from "./UniqueIdSetting/Context/SPOnPremUniqueIdSetting";
import { checkPermission } from "../../../Utilities/permissionManager";
import CheckAgentAvailable from "./Common/CommonMessageBar/CheckAgentAvailable";
import RelatedAppSettingComponent from "./RelatedAppSetting/RelatedAppSettingComponent";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2
};
const RunApplySettingMethod = {
    UpdatedScope: 1,
    AllScope: 2,
    Auto: 3
};

export default class ContentRepositoryManagementForLSP extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            showTip: false,
            tipType: 'success',
            tipMsg: '',
            items: [],
            searchKey: "",
            isShowColumnSettingsPanel: { show: false },
            showRightSetting: false,
            enableClassification: "",
            enableSyncData: "",
            headerName: "",
            nodeLevel: "",
            initUniqueIdPanel: false,
            initRelatedAppSettingPanel: false,
            skipActionInMessageBox: false,
            isCustomSetting: false,
            isUsingExistColumnName: false,
            setDocLevelTermForExistColumn: false,
        };
        this.columnSettingComponent = "columnSettingComponent";
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [
            {
                isGroup: true, id: "raCrmApplyGlobalSettingsBtnGroup", name: RMResx["RM_SPS_ApplyGlobalSettings"], buttons: [
                    { id:"raCrmApplyUpdatedScopeBtn", name: RMResx["RM_JS_SPS_ ApplyUpdatedScope"], onClick: this.applySettingMessageBox.bind(this, false) },
                    { id:"raCrmApplyAllScopeBtn", name: RMResx["RM_JS_SPS_ ApplyAllScope"], onClick: this.applySettingMessageBox.bind(this, true) },
                ]
            }
        ];
        this.inheritButton = { id:"raCrmInheritParentBtn", name: RMResx["RM_SPS_InheritGlobalSettings"], icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { id:"raCrmRunRuleActionBtn", name: RMResx["RM_JS_SPS_DisposalNow"], icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.syncDataButton = { id:"raCrmRunDataSyncBtn", name: RMResx["RM_JS_SPS_CollectNow"], icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.menuBtnItemsInMore = [];
        if (checkPermission("BCM_ContentRepositoryManagement_UniqueId", RM.UserResources)) {
            this.menuBtnItemsInMore.push({ id:"raCrmUniqueIdSettingBtn", name: RMResx.RM_JS_SP_UniqueIdSetting_Btn, icon: "fia-uniqueid", onClick: this.showUniqueIdSettingsClick.bind(this) });
        }
        if (LicenseHelper.EnableRecordsArchiver()) {
            this.menuBtnItemsInMore.push({ id:"raCrmRelatedAppSettingBtn", name: RMResx.RM_JS_LSP_RelatedRecordsAppSetting, icon: "fia-related-records", onClick: this.showRelatedAppSettingsClick.bind(this) });
        }
        this.showSkipAction = false;
        this.checkAgentUrl = "/api/SPOnPremSettingApi/CheckHasAvailableAgent";
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
        this.setState({
            initUniqueIdPanel: true,
            initRelatedAppSettingPanel: true
        });
    }

    applySettingDoAction = (runApplySettingMethod) => {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/SPOnPremSettingApi/ApplySettings",
            method: "Post",
            data: { FromTimerJobPage: false, RunJobMethod: runApplySettingMethod }
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                let showExtensionMessage = false;
                let extensionMessage = "";
                if (runApplySettingMethod == RunApplySettingMethod.UpdatedScope) {
                    let settingsCount = parseInt(resultData.Extension, 10);
                    if (settingsCount > 0) {
                        showExtensionMessage = true;
                        extensionMessage = resultData.Extsion1;
                        let content = <div>
                            {showExtensionMessage && <div>{extensionMessage}
                            </div>}
                            <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        </div>;
                        showToast.success(content);
                    } else {
                        showToast.warn(resultData.Extsion1);
                    }
                } else if (runApplySettingMethod == RunApplySettingMethod.AllScope) {
                    let content = <div>
                        <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                            <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                        </$g.I18NProvider>
                    </div>;
                    showToast.success(content);
                }
            } else {
                let errorMsg = CRMCommonUtil.commonHandelErrorMessage(result);
                if (errorMsg != null && errorMsg != "") {
                    showToast.error(errorMsg);
                } else {
                    showToast.error(RMResx.RM_JS_SPS_FailedRunAppplyJobMsg);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.ApplySettings, [SourceFlags.SPLocal, runApplySettingMethod]);
    }

    applySettingMessageBox = (isScopeAll) => {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_SPLEnsureApply,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmApplySettingDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: () => {
                        if (isScopeAll) {
                            this.applySettingDoAction(RunApplySettingMethod.AllScope);
                        } else {
                            this.applySettingDoAction(RunApplySettingMethod.UpdatedScope);
                        }
                    }
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/SPOnPremSettingApi/InheritParentSettings",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                this.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.FaildType == 10 || resultData.FaildType == 11) {
                    this.refreshNodeSettings();
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    inheritParentMessageBox = () => {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPS_EnsureInherit,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmInheritParentDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.inheritParentDoAction.bind(this)
                },
            ]
        };
        $$.messagedialog(true, args);
    }

    runDataSyncDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/SPOnPremSettingApi/RunCollectionJob",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunCollectionJobSuccess}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.ErrorMessage != "") {
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    runDataSyncMessageBox() {
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_JS_BCM_NSPOPS_EnsureSync,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunDataSyncDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runDataSyncDoAction.bind(this)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    runRuleActionDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        this.settingNode.SkipRemoveContentAndDestroyAction = this.state.skipActionInMessageBox;
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: "/api/SPOnPremSettingApi/RunOnpremiseEnforceRuleActionJob",
            method: "Post",
            data: JSON.stringify(this.settingNode)
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let res = JSON.parse(result);
            if (res.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (res.MessageType == RAMessageType.Failed) {
                if (res.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_NoExportLocation) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_NoExportLocation);
                } else {
                    showToast.error(res.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.SPLocal]);
    }

    runRuleActionMessageBox() {
        this.setState({ skipActionInMessageBox: false });
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div>
                <div className="margin-bottom-m">{RMResx.RM_JS_BCM_NSPS_EnsureRun}</div>
                {this.showSkipAction && <div className="ra-inline-middle margin-bottom-m">
                    <R.Checkbox
                        text={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                        title={RMResx.RM_JS_BCM_EnsureRun_SkipRemoveAction}
                        checked={this.state.skipActionInMessageBox}
                        onChange={this.skipActionInMessageBoxChanged.bind(this)}
                    />
                </div>}
            </div>,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id: "raCrmRunRuleActionDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.runRuleActionDoAction.bind(this)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    skipActionInMessageBoxChanged(args) {
        this.setState({ skipActionInMessageBox: args });
    }

    onTreeChanged = (nodeItem) => {
        this.currNode = nodeItem;
        this.loadNodeSettings();
    }

    loadNodeSettings(reload) {
        let nodeItem = Object.assign({}, this.currNode, { Children: null, ChildrenIds: null });
        let currentItem = nodeItem;
        while (currentItem.Parent) {
            currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
            currentItem = currentItem.Parent;
        }
        $$.loading(true);
        let option = {
            url: "/api/SPOnPremSettingApi/LoadSampleNodeSettings",
            method: "Post",
            data: nodeItem
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let settingNode = JSON.parse(result);
                this.settingNode = settingNode;
                settingNode.ParentId = nodeItem.ParentId;
                settingNode.Parent = nodeItem.Parent;
                if (settingNode.IconStatus == 0) {
                    settingNode.EnableRecordManagement = EnableRecordManagementSetting.Enable;
                }
                let text = settingNode.Name;
                if (text == "." && settingNode.Level == NodeLevel.Site) {
                    text = RMResx.RM_JS_DAM_RootSiteName.format(settingNode.Title);
                }
                if (this.currNode.TeamName) {
                    text = "(" + this.currNode.TeamName + ")" + this.currNode.Name;
                }
                this.setState({
                    showRightSetting: true,
                    headerName: text,
                    nodeLevel: settingNode.Level,
                    enableClassification: settingNode.EnableRecordManagement,
                    enableSyncData: settingNode.IsSyncData,
                    isCustomSetting: settingNode.IsCustomSetting,
                    isUsingExistColumnName: settingNode.IsUsingExistColumnName,
                    setDocLevelTermForExistColumn: settingNode.SetDocLevelTermForExistColumn,

                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
                    this.dispatch(this.columnSettingComponent, 'columnData', settingNode);
                    this.dispatch(this.manualApprovalSettingComponent, 'manualApprovalData', settingNode);
                    this.dispatch(this.scheduleSettingComponent, 'scheduleData', settingNode);
                    this.refGeneralManagementComponent && this.refGeneralManagementComponent.initData(settingNode);
                });
                let updateProps = { IconStatus: settingNode.IconStatus };
                if (nodeItem.IconStatus == 0 && settingNode.IconStatus == 2 || reload) {
                    this.refCRMTree.refreshSelectedNode(updateProps, true);
                } else {
                    this.refCRMTree.refreshSelectedNode(updateProps);
                }
                let menuButtons = [...this.menuBtnItems];
                if (settingNode.IsCustomSetting) {
                    menuButtons.push(this.inheritButton);
                }
                let enableRunJob = settingNode.EnableRecordManagement == EnableRecordManagementSetting.Enable && ((settingNode.ColumnName != null && settingNode.ColumnName != "") || settingNode.IsUsingExistColumnName);
                if (enableRunJob) {
                    menuButtons.push(this.ruleActionButton);
                }
                if (enableRunJob && settingNode.IsSyncData && (CRMCommonUtil.isGroup(settingNode) || CRMCommonUtil.isSiteCollection(settingNode))) {
                    menuButtons.push(this.syncDataButton);
                }
                menuButtons.push(...this.menuBtnItemsInMore);
                this.refTopButtons.updateButtons(menuButtons);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    handleApplySetting = () => {
        this.refreshSelectedNode();
    }

    onSearch = (args) => {
        this.setState({ searchKey: args.value });
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    checkMissingConfig = () => {
        if (!this.settingNode.ColumnName && !this.settingNode.IsUsingExistColumnName) {
            let args = {
                // classify: "warn",
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SPS_ColumnSettingMissing,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => {
                            $$.messagedialog(false);
                        }
                    }]
            };
            $$.messagedialog(true, args);
            return true;
        } else {
            return false;
        }
    }

    showUniqueIdSettingsClick() {
        this.refUniqueIdComponent.showUniqueIdSettingsPanel();
    }

    showRelatedAppSettingsClick() {
        this.refRelatedAppSettingComponent.showPanel();
    }

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_LSP]} />
                <div className="flex flex-column gap-m">
                    <CheckAgentAvailable url={this.checkAgentUrl} />
                    <CheckLocalNodeMessageBar />
                </div>
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                ></TopButtonsComponent>
            </section>
            <section className="crm-content">
                <div className="ra-crm-splitter-container">
                    <R.Splitter minAsize={SplitterSize.minAsize} minBsize={SplitterSize.minBsize} defaultAsize={SplitterSize.defaultAsize}>
                        <div className="ra-splitter-left">
                            <div className="ra-splitter-header-left">
                                <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_LeftTitle}</div>
                            </div>
                            <div className="ra-splitter-tree">
                                <SPCRMTree
                                    ref={r => this.refCRMTree = r}
                                    searchKey={this.state.searchKey}
                                    onSelectedNodeChanged={this.onTreeChanged}
                                    treeSource={SourceFlags.SPLocal}
                                    onNodeRefresh={this.onNodeRefresh}
                                ></SPCRMTree>
                            </div>
                        </div>

                        <div className="ra-splitter-right">
                            <div style={{ fontSize: 0 }}>
                                <div style={{ width: "calc(100% - 156px)", display: "inline-block" }}>
                                    <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_RightTitle}</div>
                                    <div className="ra-splitter-header-name" data-tooltip="diffneed" aria-label={this.state.headerName}>
                                        {this.state.headerName != "" && <span className="fia-folder ra-splitter-folder"></span>}
                                        <span tabIndex="0" style={{ flex: 1 }} className="ra-ellipsis">{this.state.headerName}</span>
                                    </div>
                                </div>
                                {this.state.isCustomSetting && <div className="ra-splitter-unique-container" tabIndex="0" aria-label={RMResx.RM_JS_SPS_HasOwnSettingMessage}>
                                    <div
                                        id="showUniqueBtn"
                                        className="inline-block"
                                        style={{ lineHeight: "26px", marginRight: "8px" }}
                                    >
                                        <span className="fia-asterisk ra-splitter-unique-icon"></span>
                                        <span>{RMResx.RM_JS_SPS_Unique}</span>
                                    </div>
                                    <R.Popup of={'#showUniqueBtn'} arrow={true} triggerEvent="hover:300" position="right">
                                        <div>
                                            <div style={{ margin: "16px", width: "280px", fontSize: "14px" }}>
                                                <span>{RMResx.RM_JS_SPS_HasOwnSettingMessage}</span>
                                            </div>
                                        </div>
                                    </R.Popup>
                                </div>}
                            </div>
                            {!this.state.showRightSetting && <div className="ra-splitter-description" tabIndex="0">
                                <span>{RMResx.RM_JS_SPS_Tips}</span>
                            </div>}

                            {this.settingNode && this.state.showRightSetting && <div>
                                <GeneralManagementComponent
                                    context={SPOnPremGeneralManagement.getContext()}
                                    id="generalManagementComponent"
                                    ref={r => this.refGeneralManagementComponent = r}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    sourceFlag={SourceFlags.SPLocal}
                                ></GeneralManagementComponent>
                            </div>}

                            {this.state.showRightSetting && this.state.enableClassification == EnableRecordManagementSetting.Enable && <div>
                                <ColumnSettingComponent
                                    context={SPOnPremColumnSetting.getContext()}
                                    id={this.columnSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    sourceFlag={SourceFlags.SPLocal}
                                ></ColumnSettingComponent>
                                {((!this.state.isUsingExistColumnName) || (this.state.isUsingExistColumnName && this.state.setDocLevelTermForExistColumn)) &&
                                    <DocumentTermSettingComponent
                                        context={SharePointOnPremDocumentTerm.getContext()}
                                        id={this.documentTermSettingComponent}
                                        refreshNodeSettings={this.refreshNodeSettings}
                                        checkMissingConfig={this.checkMissingConfig}
                                        sourceFlag={SourceFlags.SPLocal}
                                    ></DocumentTermSettingComponent>
                                }
                                <ManualApprovalSettingComponent
                                    context={SPOnPremManualApprovalSetting.getContext()}
                                    id={this.manualApprovalSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ManualApprovalSettingComponent>
                                <ScheduleSettingComponent
                                    context={SPOnPremScheduleSetting.getContext()}
                                    id={this.scheduleSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ScheduleSettingComponent>
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
                {this.state.initUniqueIdPanel &&
                    <UniqueIdSettingCommon
                        id="uniqueId"
                        ref={r => this.refUniqueIdComponent = r}
                        supportCustomColumn={false}
                        context={SPOnPremUniqueIdSetting.getContext()}
                        sourceFlag={SourceFlags.SPLocal}
                    />
                }
                {this.state.initRelatedAppSettingPanel &&
                    <RelatedAppSettingComponent
                        id="relatedAppSetting"
                        ref={r => this.refRelatedAppSettingComponent = r}
                    />
                }
            </section>
        </div>;
    }
}

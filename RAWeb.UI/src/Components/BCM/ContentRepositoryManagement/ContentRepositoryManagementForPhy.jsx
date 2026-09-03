import SiteMapLinks from "../../../Constants/SiteMapLinks";
import PhyCRMTree from "../../Common/Tree/Instances/Physical/PhyCRMTree";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import ManualApprovalSettingComponent from "./ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import PhysicalRecordsDocumentTerm from "./DocumentTermSetting/Context/PhysicalRecordsDocumentTerm";
import { showToast } from "../../../Utilities/CommonUtil";
import ValidateMessageBar from "./Common/CommonMessageBar/ValidateMessageBar";
import PhysicalManualApprovalSetting from "./ManualApprovalSetting/Context/PhysicalManualApprovalSetting";
import PhysicalScheduleSetting from "./ScheduleSetting/Context/PhysicalScheduleSetting";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2
};
export default class ContentRepositoryManagement4Phy extends R.Component {
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
            skipActionInMessageBox: false,
            isCustomSetting: false,
        };
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [];
        this.menuBtnItemsInMore = [];
        this.inheritButton = { id:"raCrmInheritParentBtn", name: RMResx["RM_SPS_InheritGlobalSettings"], icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { id:"raCrmRunRuleActionBtn", name: RMResx["RM_JS_SPS_DisposalNow"], icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.showSkipAction = true;
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/PRSettingApi/InheritParentPRSettings",
            method: "Post",
            data: this.settingNode.UniqueId
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result != 1) {
                this.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else {
                showToast.error(RMResx.RM_JS_SPS_SaveSettingsFailed);
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

    runRuleActionDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        // this.settingNode.SkipRemoveContentAndDestroyAction = this.state.skipActionInMessageBox;
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: "/api/PRSettingApi/RunPhysicalJob",
            method: "Post",
            data: {Id: this.currNode.Id, SkipRemove: this.state.skipActionInMessageBox}
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (resultData.MessageType == RAMessageType.Failed) {
                if (resultData.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_NoExportLocation) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_NoExportLocation);
                } else {
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.Phy]);
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

    onCRMTreeChanged = (nodeItem) => {
        this.currNode = nodeItem;
        this.loadNodeSettings();
    }

    loadNodeSettings(reload) {
        let nodeItem = this.currNode;
        $$.loading(true);
        let option = {
            url: `/api/PRSettingApi/LoadPhysicalRecordSetting?locationUid=${nodeItem.UniqueId}`,
            method: "Get",
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let settingNode = JSON.parse(result);
                this.settingNode = settingNode;
                settingNode.ParentId = nodeItem.ParentId;
                settingNode.Parent = nodeItem.Parent;

                this.setState({
                    showRightSetting: true,
                    headerName: this.currNode.Name,
                    isCustomSetting: settingNode.IsCustomSetting,
                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
                    this.dispatch(this.manualApprovalSettingComponent, 'manualApprovalData', settingNode);
                    this.dispatch(this.scheduleSettingComponent, 'scheduleData', settingNode);
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
                let enableRunJob = !CRMCommonUtil.guidIsEmpty(settingNode.TermSetId);
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

    checkMissingConfig = () => {
        if (CRMCommonUtil.guidIsEmpty(this.settingNode.TermSetId) && !this.settingNode.IsTopLevelSetting) {
            let args = {
                // classify: "warn",
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_PRS_TopLevelSettingMissing,
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

    onSearch = (args) => {
        this.setState({ searchKey: args.value });
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_Phy]} />
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
                                <PhyCRMTree
                                    ref={r => this.refCRMTree = r}
                                    searchKey={this.state.searchKey}
                                    onSelectedNodeChanged={this.onCRMTreeChanged}
                                    onNodeRefresh={this.onNodeRefresh}
                                ></PhyCRMTree>
                            </div>
                        </div>

                        <div className="ra-splitter-right">
                            <div style={{ fontSize: 0 }}>
                                <div style={{ width: this.state.isCustomSetting ? "calc(100% - 156px)" : "calc(100% - 24px)", display: "inline-block" }}>
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
                            {this.state.showRightSetting && <div>
                                <DocumentTermSettingComponent
                                    context={PhysicalRecordsDocumentTerm.getContext()}
                                    id={this.documentTermSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    sourceFlag={SourceFlags.Phy}
                                ></DocumentTermSettingComponent>
                                <ManualApprovalSettingComponent
                                    context={PhysicalManualApprovalSetting.getContext()}
                                    id={this.manualApprovalSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ManualApprovalSettingComponent>
                                <ScheduleSettingComponent
                                    context={PhysicalScheduleSetting.getContext()}
                                    id={this.scheduleSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ScheduleSettingComponent>
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
            </section>
        </div>;
    }
}

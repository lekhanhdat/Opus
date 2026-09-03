import SiteMapLinks from "../../../Constants/SiteMapLinks";
import EXOCRMTree from "../../Common/Tree/Instances/EXO/CRMEXOTree";
import CRMCommonUtil, { RAMessageType, SplitterSize } from "./Common/CRMCommonUtil";
import DocumentTermSettingComponent from "./DocumentTermSetting/DocumentTermSettingComponent";
import ManualApprovalSettingComponent from "./ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "./ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../Common/Util/TopButtonsComponent";
import GeneralManagementComponent from "./GeneralManagement/GeneralManagementComponent";
import "../../../Less/BCM/ContentRepositoryManagement/common.less";
import ExchangeOnlineGeneralManagement from "./GeneralManagement/Context/ExchangeOnlineGeneralManagement";
import ExchangeOnlineDocumentTerm from "./DocumentTermSetting/Context/ExchangeOnlineDocumentTerm";
import EXOManualApprovalSetting from "./ManualApprovalSetting/Context/EXOManualApprovalSetting";
import { EnvironmentHelper, LicenseHelper, showToast } from "../../../Utilities/CommonUtil";
import CheckRemoteNodeMessageBar from "./Common/CommonMessageBar/CheckRemoteNodeMessageBar";
import EXOScheduleSetting from "./ScheduleSetting/Context/EXOScheduleSetting";
import { addTelemetryRecord } from "../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule } from "../../../Constants/Constants";
import { DeployTermMethod } from "./DocumentTermSetting/DocumentTermSettingPanel";
import CustomMetadataPanel from "./CustomMetadataSetting/CustomMetadataPanel";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2
};
export default class ContentRepositoryManagement4EXO extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            showTip: false,
            tipType: 'success',
            tipMsg: '',
            items: [],
            searchKey: "",
            showRightSetting: false,
            enableClassification: "",
            enableSyncData: "",
            headerName: "",
            nodeLevel: "",
            initImportPanel: false,
            skipActionInMessageBox: false,
            isCustomSetting: false,
            nodeSetting: {},
            showTermSettings: true,
            availableRules: [],
            needLoadRules: true,
            showDisposalScheduleSettings: true,
            isShowCustomMetadataPanel: { show: false },
        };
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [
            { isStatic: true, name: RMResx["RM_SPS_ApplyGlobalSettings"], id:"raCrmApplySettinBtn", onClick: this.applySettingMessageBox }
        ];
        this.inheritButton = { name: RMResx["RM_SPS_InheritGlobalSettings"], id:"raCrmInheritParentBtn", icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { name: RMResx["RM_JS_SPS_DisposalNow"], id:"raCrmRunRuleActionBtn", icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.syncDataButton = { name: RMResx["RM_JS_SPS_CollectNow"],  id:"raCrmRunDataSyncBtn", icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.menuBtnItemsInMore = [];
        this.menuBtnItemsInMore.push({ id: "raCrmCustomMetadataBtn", name: RMResx.RM_JS_SP_CustomMetadata_Btn, icon: "fia-search", onClick: this.showCustomMetadataClick });
        this.showSkipAction = true;
    }

    componentInit() {
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.ContentPageLoaded);
        this.setState({ initImportPanel: true });
    }

    showCustomMetadataClick = async () => {
        this.setState({ isShowCustomMetadataPanel: { show: true } });
        const [indexData, manageData, inUsedColumnData] = await Promise.all([this.getCustomIndexMetaData(), this.getCustomMetadataColumns(), this.getInUsedCustomMetadataColumns()]);
        this.dispatch("customMetadataPanel", "isOpenPanel", { indexData, manageData, inUsedColumnData });
    }
    applySettingDoAction = (hasRunningJob) => {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/EXOSettingApi/ApplyEXOSettings",
            method: "Post",
            data: { FromTimerJobPage: false }
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            var resultData = JSON.parse(result);
            if (resultData.MessageType == RAMessageType.Successful) {
                if (hasRunningJob) {
                    showToast.withTitle({
                        id: `toast-source-${SourceFlags.Exo}-apply-setting-is-running`,
                        title: <div className="font-semibold">{RMResx.RM_JS_SPS_RunApplySettingJobSucceedWithConflictJob_Title}</div>,
                        content: (
                            <$g.I18NProvider slot="content" msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceedWithConflictJob}>
                                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                            </$g.I18NProvider>
                        ),
                        type: "success",
                    });
                } else {
                    let content = <$g.I18NProvider msg={RMResx.RM_JS_SPS_RunApplySettingJobSucceed}>
                        <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                    </$g.I18NProvider>;
                    showToast.success(content);
                }
            } else {
                if (resultData.ErrorMessage) {
                    showToast.error(resultData.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.ApplySettings, [SourceFlags.Exo]);
    }

    getApplySettingMessageBoxContent = async () => {
        const defaultResult = {
            hasRunningJob: false,
            messageTitle: RMResx.RM_JS_Common_Confirmation,
            messageContent: RMResx.RM_JS_BCM_EXO_EnsureApply,
        };

        if (LicenseHelper.EnableRecordsArchiver()) {
            const option = {
                url: "/api/EXOSettingApi/CheckRunningEXOSettingJob",
                method: "GET",
            }
            const hasRunningJob = await fetchUtility(option);
            if (!!hasRunningJob) {
                return {
                    hasRunningJob,
                    messageTitle: RMResx.RM_JS_SPS_ApplySettingJobConflictDetected_Title,
                    messageContent: <div tabIndex={0}>{RMResx.RM_JS_SPS_ApplySettingJobConflictDetected}</div>,
                };
            }
            
            return defaultResult;
        }

        return defaultResult;
    }

    applySettingMessageBox = async () => {
        const { hasRunningJob, messageTitle, messageContent } = await this.getApplySettingMessageBoxContent();
        let args = {
            // classify: "warn",
            width: "550px",
            hideActions: false,
            title: messageTitle,
            content: messageContent,
            buttons: [
                {
                    text: RMResx.RM_JS_Common_Cancel, onClick: () => {
                        $$.messagedialog(false);
                    }
                },
                {
                    id:"raCrmApplySettingDoActionBtn",
                    text: RMResx.RM_JS_Common_OK,
                    primary: true,
                    classify: "theme",
                    onClick: this.applySettingDoAction.bind(this, hasRunningJob)
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/EXOSettingApi/InheritParentEXOSettings",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result == "Sucess") {
                this.refreshNodeSettings();
                showToast.success(RMResx.RM_JS_SPS_SaveSettingsSuccess);
            } else if (result == "Failed") {
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
                }
            ]
        };
        $$.messagedialog(true, args);
    }

    runDataSyncDoAction() {
        $$.messagedialog(false);
        $$.loading(true);
        let option = {
            url: "/api/EXOSettingApi/RunEXOCollectionJob",
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
            content: RMResx.RM_JS_BCM_EXO_EnsureSync,
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
                },
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
            url: "/api/EXOSettingApi/RunEXOJob",
            method: "Post",
            data: this.settingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result.MessageType == RAMessageType.Successful) {
                let content = <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>;
                showToast.success(content);
            } else if (result.MessageType == RAMessageType.Failed) {
                if (result.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_NoExportLocation) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_NoExportLocation);
                } else if (result.ErrorMessage.indexOf(RMResx.RM_JS_DAM_FaildRun_FTPExportLocationNotSupported) >= 0) {
                    showToast.error(RMResx.RM_JS_DAM_FaildRun_FTPExportLocationNotSupported);
                } else {
                    showToast.error(result.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.Exo]);
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
        this.loadRules();
    }

    loadRules = async (reload) => {
        if(this.state.needLoadRules || reload) {
            $$.loading(true);
            try {
                const groupNode = CRMCommonUtil.getEXOGroupNode(this.currNode);
                const result = await fetchUtility({url:'/api/EXOSettingApi/GetAvailableRuleList', method: 'post', data: groupNode.Id});
                if(result)
                {
                    this.setState({availableRules: result});
                }
                $$.loading(false);
            } catch (error) {
                $$.loading(false);
            }
        }
    };

    loadNodeSettings(reload) {
        let nodeItem = this.currNode;
        $$.loading(true);
        let option = {
            url: "/api/EXOSettingApi/LoadExchangeNodeSetting",
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
                const enableTermSettings = this.showTermSettings();
                if (settingNode.IconStatus == 0) {
                    settingNode.EnableRecordManagement = EnableRecordManagementSetting.Enable;
                }
                // set apply term by UseAutoClassification as default on GCP env
                if (EnvironmentHelper.IsGCPEnvironment) {
                    settingNode.DeployTermMethod = DeployTermMethod.UseAutoClassification;
                }
                this.setState({
                    showRightSetting: true,
                    headerName: nodeItem.DisplayName,
                    nodeLevel: settingNode.Level,
                    enableClassification: settingNode.EnableRecordManagement,
                    enableSyncData: settingNode.IsSyncData,
                    isCustomSetting: settingNode.IsCustomSetting,
                    nodeSetting: settingNode,
                    showTermSettings: enableTermSettings,
                    showDisposalScheduleSettings: this.showDisposalScheduleSetting()
                }, () => {
                    this.dispatch(this.documentTermSettingComponent, 'init', settingNode);
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

                let enableRunJob = this.isAllowRunJob(settingNode, enableTermSettings);
                if (enableRunJob) {
                    menuButtons.push(this.ruleActionButton);
                }
                if (!settingNode.IsNullClassificationSetting && enableTermSettings && enableRunJob && settingNode.IsSyncData && (CRMCommonUtil.isEXOGroup(settingNode) || CRMCommonUtil.isEXOMailBox(settingNode))) {
                    menuButtons.push(this.syncDataButton);
                }
                menuButtons.push(...this.menuBtnItemsInMore);
                this.refTopButtons.updateButtons(menuButtons);
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    isAllowRunJob = (settingNode, enableTermSettings) => {
        let result = false;
        const enableRuleSettings = settingNode.IsNullClassificationSetting;
        const hasTermSettings = !CRMCommonUtil.guidIsEmpty(settingNode.TermSetId);
        if(settingNode.EnableRecordManagement == EnableRecordManagementSetting.Enable)
        {
            if(CRMCommonUtil.isEXOGroup(settingNode))
            {
                result = enableRuleSettings || hasTermSettings;
            }
            if(CRMCommonUtil.isEXOMailBox(settingNode))
            {
                result = ((enableRuleSettings && !enableTermSettings) || (!enableRuleSettings && hasTermSettings));
            }
        }
        return result;
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
        const  enableTermSettings = !this.settingNode.IsNullClassificationSetting;
        const noTermSettings = CRMCommonUtil.guidIsEmpty(this.settingNode.TermSetId);
        if (enableTermSettings && noTermSettings && !CRMCommonUtil.isEXOGroup(this.settingNode)) {
            let args = {
                // classify: "warn",
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_SPS_EXOGroupSettingMissing,
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

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    checkEditIsDisabled(){
        const enableTermSettings = !this.settingNode.IsNullClassificationSetting;
        const hasCustomTermSetting = this.settingNode.IsCustomTermSetting;
        if (!enableTermSettings &&  CRMCommonUtil.isEXOMailBox(this.settingNode) && !hasCustomTermSetting) {
            return true;
        }
        return false;
    }

    showTermSettings = () => {
        const enableTermSettings = !this.settingNode.IsNullClassificationSetting;
        if(this.isCustomTermSettingMailboxNode()) {
            return true;
        }
        if(enableTermSettings) 
        {
            return true;
        }
        return false;
    }

    showDisposalScheduleSetting = () => {
        const enableTermSettings = !this.settingNode.IsNullClassificationSetting;
        if(!enableTermSettings && this.isCustomTermSettingMailboxNode())
        {
            return false;
        }
        return true;
    }

    isCustomTermSettingMailboxNode = () => {
        return CRMCommonUtil.isEXOMailBox(this.settingNode) && this.settingNode.IsCustomTermSetting;
    }

    // Get manage metadata list
    getCustomMetadataColumns = async () => {
        $$.loading(true);
        const option = {
            url: "/api/EXOSettingApi/GetCustomMetadataColumns",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    getCustomIndexMetaData = async () => {
        $$.loading(true);
        const option = {
            url: "/api/EXOSettingApi/GetCustomIndexMetadatasBySourceFlag",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    getInUsedCustomMetadataColumns = async () => {
        $$.loading(true);
        const option = {
            url: "/api/EXOSettingApi/GetInUsedCustomMetadataColumns",
            method: "GET",
        };
        const res = await fetchUtility(option);
        $$.loading(false);
        return res;
    }

    cancelCustomMetadata = () => {
        this.setState({ isShowCustomMetadataPanel: { show: false } });
    }

    saveCustomMetadata = (e) => {
        this.dispatch("customMetadataPanel", "save");
    }

    renderCustomMetadataPanel() {
        return (
            <R.Panel
                title={RMResx.RM_JS_SP_CustomMetadata_Btn}
                size={670}
                status={this.state.isShowCustomMetadataPanel}
                destroy={true}
            >
                <CustomMetadataPanel id="customMetadataPanel"
                    sourceFlag={SourceFlags.Exo} 
                    getCustomMetadataColumns={this.getCustomMetadataColumns} 
                    onClose={this.cancelCustomMetadata}
                />
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelCustomMetadata} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.saveCustomMetadata} />
                </>
            </R.Panel>
        );
    }

    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_EXO]} />
                <CheckRemoteNodeMessageBar />
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                    showCount={4}
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
                                <EXOCRMTree
                                    ref={r => this.refCRMTree = r}
                                    searchKey={this.state.searchKey}
                                    onSelectedNodeChanged={this.onTreeChanged}
                                    onNodeRefresh={this.onNodeRefresh}
                                ></EXOCRMTree>
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
                                <GeneralManagementComponent
                                    context={ExchangeOnlineGeneralManagement.getContext()}
                                    id="generalManagementComponent"
                                    ref={r => this.refGeneralManagementComponent = r}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    disabled={this.checkEditIsDisabled()}
                                    sourceFlag={SourceFlags.Exo}
                                ></GeneralManagementComponent>
                            </div>}

                            {this.state.showRightSetting && this.state.enableClassification == EnableRecordManagementSetting.Enable && <div>
                                <DocumentTermSettingComponent
                                    context={ExchangeOnlineDocumentTerm.getContext()}
                                    id={this.documentTermSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    disabled={this.checkEditIsDisabled()}
                                    sourceFlag={SourceFlags.Exo}
                                    showTermSettings={this.state.showTermSettings}
                                    availableRules={this.state.availableRules}
                                    refreshRules={this.loadRules}
                                ></DocumentTermSettingComponent>
                                <ManualApprovalSettingComponent
                                    context={EXOManualApprovalSetting.getContext()}
                                    id={this.manualApprovalSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ManualApprovalSettingComponent>
                                {this.state.showDisposalScheduleSettings && 
                                    <ScheduleSettingComponent
                                        context={EXOScheduleSetting.getContext()}
                                        id={this.scheduleSettingComponent}
                                        refreshNodeSettings={this.refreshNodeSettings}
                                        checkMissingConfig={this.checkMissingConfig}
                                    ></ScheduleSettingComponent>
                                }
                            </div>}
                        </div>
                    </R.Splitter>
                </div>
                {this.renderCustomMetadataPanel()}
            </section>
        </div>;
    }
}

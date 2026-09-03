import SiteMapLinks from "../../../../Constants/SiteMapLinks";
import { NodeLevel } from "../../../../Constants/DAEnums";
import SPCRMTree from "../../../Common/Tree/Instances/SPTree/CRMSPTree";
import CRMCommonUtil, { OperationState, RAMessageType, SplitterSize } from "../Common/CRMCommonUtil";
import DocumentTermSettingComponent from "../DocumentTermSetting/DocumentTermSettingComponent";
import ManualApprovalSettingComponent from "../ManualApprovalSetting/ManualApprovalSettingComponent";
import ScheduleSettingComponent from "../ScheduleSetting/ScheduleSettingComponent";
import TopButtonsComponent from "../../../Common/Util/TopButtonsComponent";
import GeneralManagementComponent from "../GeneralManagement/GeneralManagementComponent";
import "../../../../Less/BCM/ContentRepositoryManagement/common.less";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";
import ValidateMessageBar from "../Common/CommonMessageBar/ValidateMessageBar";
import CheckRemoteNodeMessageBar from "../Common/CommonMessageBar/CheckRemoteNodeMessageBar";
import OneDriveDocumentTerm from "../DocumentTermSetting/Context/OneDriveDocumentTerm";
import OneDriveGeneralManagement from "../GeneralManagement/Context/OneDriveGeneralManagement";
import UniqueIdSettingCommon from "../UniqueIdSetting/UniqueIdSettingCommon";
import OneDriveManualApprovalSetting from "../ManualApprovalSetting/Context/OneDriveManualApprovalSetting";
import OneDriveScheduleSetting from "../ScheduleSetting/Context/OneDriveScheduleSetting";
import { addTelemetryRecord } from "../../../../Utilities/TelemetryUtil";
import { SourceFlags, TelemetryEventType, TelemetryModule, BrowseTreeNodeSourceType } from "../../../../Constants/Constants";
import OneDriveUniqueIdSetting from "../UniqueIdSetting/Context/OneDriveUniqueIdSetting";
import { checkPermission } from "../../../../Utilities/permissionManager";
import { TabIndex } from "../CRMForSPO";
import ConvertStubPanel from "../ConvertStubSetting/ConvertStubPanel";
import RouterUrls from "../../../../Constants/RouterUrls";
import MigrateDeclaredRecordsPanel from "../MigrateDeclaredRecordsSetting/MigrateDeclaredRecordsPanel";

export const EnableRecordManagementSetting = {
    Enable: 1,
    Disable: 2
};
const RunApplySettingMethod = {
    UpdatedScope: 1,
    AllScope: 2,
    Auto: 3
};

export default class ContentRepositoryManagementForOD extends R.Component {
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
            isShowConvertStubPanel: { show: false },
            isShowMigrateDeclaredRecordsPanel: { show: false },
            showRightSetting: false,
            enableClassification: "",
            enableSyncData: "",
            headerName: "",
            nodeLevel: "",
            initUniqueIdPanel: false,
            initConvertStubPanel: false,
            initMigrateDeclaredRecordsPanel: false,
            skipActionInMessageBox: false,
            isEnableSuperUserDecryptInMessageBox: false,
            isCustomSetting: false,
            showTermSettings: true,
            needLoadRules: true,
            availableRules: [],
        };
        this.documentTermSettingComponent = "documentTermSettingComponent";
        this.manualApprovalSettingComponent = "manualApprovalSettingComponent";
        this.scheduleSettingComponent = "scheduleSettingComponent";
        this.menuBtnItems = [];
        this.inheritButton = {id:"raCrmInheritParentBtn", name: RMResx["RM_SPS_InheritGlobalSettings"], icon: "fia-arrow-line-up", onClick: this.inheritParentMessageBox };
        this.ruleActionButton = { id:"raCrmRunRuleActionBtn", name: RMResx["RM_JS_SPS_DisposalNow"], icon: "fia-run", onClick: this.runRuleActionMessageBox.bind(this) };
        this.syncDataButton = { id:"raCrmRunDataSyncBtn", name: RMResx["RM_JS_SPS_CollectNow"], icon: "fia-sync", onClick: this.runDataSyncMessageBox.bind(this) };
        this.convertStubButton = { id: "raCrmConvertStubBtn", name: RMResx.RM_JS_SP_ConvertStub_Btn, icon: "fia-convert-stub", onClick: this.showConvertStubClick.bind(this) };
        this.migrateDeclaredRecordsButton = { id: "raCrmMigrateDeclaredRecordsButtonBtn", name: RMResx.RM_JS_SP_MigrateDeclaredRecords_Btn, icon: "fia-change", onClick: this.showMigrateDeclaredRecordsClick.bind(this) };
        this.menuBtnItemsInMore = [];
        if (checkPermission("BCM_ContentRepositoryManagement_UniqueId", RM.UserResources)) {
            this.menuBtnItemsInMore.push({id:"raCrmUniqueIdSettingBtn", name: RMResx.RM_JS_SP_UniqueIdSetting_Btn, icon: "fia-uniqueid", onClick: this.showUniqueIdSettingsClick.bind(this) });
        }
        this.showSkipAction = true;
        this.showUseDecrypt = RM.gData.enableRecordsArchiver;
    }

    componentInit() {
        addTelemetryRecord(
            TelemetryModule.ContentRepositoryManagement,
            TelemetryEventType.ContentPageLoaded
        );
        this.setState({ initUniqueIdPanel: true, initConvertStubPanel: true, initMigrateDeclaredRecordsPanel: true });
    }

    inheritParentDoAction() {
        $$.messagedialog(false);
        let option = {
            url: "/api/OneDriveSettingApi/InheritParentSettings",
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
            url: "/api/OneDriveSettingApi/RunCollectionJob",
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

    async runDataSyncMessageBox() {
        let status = 0;
        let messageContent = RMResx.RM_JS_BCM_NSPS_ODEnsureApply;
        if (RM.gData.hasIntelligentPermission) {
            let option = {
                url: "/api/TrainingScopeApi/GetTrainingModelStatus",
                method: "Post",
                data: this.settingNode
            };
            status = await fetchUtility(option);
            if (status == OperationState.Running) {
                messageContent = <div>
                    <div>{RMResx.RM_MachineLearning_ModelIsTrainingWarn}</div>
                    <div>{RMResx.RM_JS_BCM_NODS_EnsureSync}</div>
                </div>;
            }
        }

        let args = {
            classify: status == OperationState.Running ? "warn" : "",
            width: "550px",
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: messageContent,
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
        this.settingNode.IsEnableSuperUserDecrypt = this.state.isEnableSuperUserDecryptInMessageBox;
        this.settingNode.DisposeScheduleInfo = null;
        let option = {
            url: "/api/OneDriveSettingApi/RunJob",
            method: "Post",
            data: JSON.stringify(this.settingNode)
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
        addTelemetryRecord(TelemetryModule.ContentRepositoryManagement, TelemetryEventType.RunEnforceRuleActions, [SourceFlags.OneDrive]);
    }

    runRuleActionMessageBox() {
        this.setState({ skipActionInMessageBox: false });
        this.setState({ isEnableSuperUserDecryptInMessageBox: false });
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
                {this.showUseDecrypt && <div className="ra-inline-middle margin-bottom-m">
                    <R.Checkbox
                        text={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                        title={RMResx.RM_JS_BCM_EnsureRun_DecryptIRM}
                        checked={this.state.isEnableSuperUserDecryptInMessageBox}
                        onChange={this.isEnableSuperUserDecryptInMessageBoxChanged.bind(this)}
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
                    id: "raRunRuleActionDoActionBtn",
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
    
    isEnableSuperUserDecryptInMessageBoxChanged(args) {
        this.setState({ isEnableSuperUserDecryptInMessageBox: args });
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
                const groupNode = CRMCommonUtil.getGroupNode(this.currNode);
                const result = await fetchUtility({url:'/api/OneDriveSettingApi/GetAvailableRuleList ', method: 'post', data: groupNode.Id});
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
        let nodeItem = Object.assign({}, this.currNode, { Children: null, ChildrenIds: null });
        let currentItem = nodeItem;
        while (currentItem.Parent) {
            currentItem.Parent = Object.assign({}, currentItem.Parent, { Children: null, ChildrenIds: null });
            currentItem = currentItem.Parent;
        }
        $$.loading(true);
        let option = {
            url: "/api/OneDriveSettingApi/LoadSampleNodeSettings",
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
                    showTermSettings: !settingNode.IsNullClassificationSetting,
                    isCustomSetting: settingNode.IsCustomSetting,

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
                let enableRunJob = settingNode.EnableRecordManagement == EnableRecordManagementSetting.Enable && (!CRMCommonUtil.guidIsEmpty(settingNode.TermSetId) || this.settingNode.IsNullClassificationSetting);
                if (enableRunJob) {
                    menuButtons.push(this.ruleActionButton);
                }
                if (!settingNode.IsNullClassificationSetting && enableRunJob && (CRMCommonUtil.isGroup(settingNode) || CRMCommonUtil.isSiteCollection(settingNode))) {
                    menuButtons.push(this.syncDataButton);
                }

                let menuButtonsInMore = [...this.menuBtnItemsInMore];
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && RM.gData.enableRecordsArchiver && (CRMCommonUtil.isGroup(settingNode) || CRMCommonUtil.isSiteCollection(settingNode))) {
                    menuButtonsInMore.push(this.convertStubButton);
                }
                if (checkPermission(RouterUrls.CP_Index, RM.UserResources) && !LicenseHelper.Is21VEnv() && LicenseHelper.EnableRecordsArchiver() && CRMCommonUtil.isGroup(settingNode)) {
                    menuButtonsInMore.push(this.migrateDeclaredRecordsButton);
                }
                menuButtons.push(...menuButtonsInMore);
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
        this.setState({ searchKey: args });
    }

    refreshNodeSettings = (args) => {
        this.loadNodeSettings(args);
    };

    termSettingCheckMissingConfig = () => {
        return !CRMCommonUtil.isGroup(this.settingNode) && this.checkMissingConfig();
    }

    checkMissingConfig = () => {
        if (CRMCommonUtil.isGroup(this.settingNode) || this.settingNode.IsNullClassificationSetting) {
            return false;
        }
        if (CRMCommonUtil.guidIsEmpty(this.settingNode.TermSetId)) {
            let args = {
                // classify: "warn",
                width: '550px',
                hideActions: true,
                title: RMResx.RM_JS_Common_Confirmation,
                content: RMResx.RM_JS_OneDrive_GroupTermSettingMissing,
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

    onNodeRefresh = (exitSelectedNode) => {
        if (!exitSelectedNode) {
            this.setState({ showRightSetting: false, isCustomSetting: false, headerName: "" });
            this.refTopButtons.updateButtons([...this.menuBtnItems, ...this.menuBtnItemsInMore]);
        }
    }

    checkEditIsDisabled = () => {
        return this.settingNode.IsNullClassificationSetting && !this.isAllowSetByRulesForOnedrive();
    }

    isAllowSetByRulesForOnedrive = () => {
        return CRMCommonUtil.isAllowSetByRulesOnedriveLevel(this.settingNode);
    }

    showConvertStubClick() {
        this.setState({ isShowConvertStubPanel: { show: true } });
    }

    saveConvertStub = (e) => {
        this.dispatch("convertStubPanel", 'onSave', (success, data) => {
            if (success) {
                this.setState({ isShowConvertStubPanel: { show: false } });
            }
        });
        return false;
    }

    cancelConvertStub = () => {
        this.setState({ isShowConvertStubPanel: { show: false } });
    }

    showMigrateDeclaredRecordsClick() {
        this.setState({ isShowMigrateDeclaredRecordsPanel: { show: true } });
    }

    saveMigrateDeclaredRecords = (e) => {
        this.dispatch("migrateDeclaredRecordsPanel", 'onValidate', (isValid) => {
            if (isValid) {
                $$.messagedialog(true, {
                    width: "550px",
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: (
                        <div tabIndex={0}>
                            {RMResx.RM_JS_SP_MigrateDeclaredRecords_ConfirmMsg}
                        </div>
                    ),
                    buttons: [
                        {
                            text: RMResx.RM_JS_Common_Cancel,
                            onClick: () => {
                                $$.messagedialog(false);
                            }
                        },
                        {
                            text: RMResx.RM_JS_Common_OK,
                            primary: true,
                            classify: "theme",
                            onClick: this.onSaveMigrateDeclaredRecords.bind(this),
                        },
                    ],
                });
            }
        });
        return false;
    }

    onSaveMigrateDeclaredRecords = () => {
        this.dispatch("migrateDeclaredRecordsPanel", 'onSave', (success, data) => {
            if (success) {
                $$.messagedialog(false);
                this.setState({ isShowMigrateDeclaredRecordsPanel: { show: false } });
            }
        });
        return false;
    }

    cancelMigrateDeclaredRecords = () => {
        this.setState({ isShowMigrateDeclaredRecordsPanel: { show: false } });
    }

    renderConvertStubPanel() {
        return <R.Panel
            header={RMResx.RM_JS_SP_ConvertStub_Btn}
            size={670}
            status={this.state.isShowConvertStubPanel}
            destroy={true}
        >
            <ConvertStubPanel
                id="convertStubPanel"
                source={SourceFlags.OneDrive}
                treeData={this.settingNode}
            ></ConvertStubPanel>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelConvertStub} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_TimerJob_Run} onClick={this.saveConvertStub} />
            </>
        </R.Panel>;
    }

    renderMigrateDeclaredRecordsPanel() {
        return (
            <R.Panel
                header={RMResx.RM_JS_SP_MigrateDeclaredRecords_Btn}
                size={670}
                status={this.state.isShowMigrateDeclaredRecordsPanel}
                destroy={true}
                onClose={this.cancelMigrateDeclaredRecords}
            >
                <MigrateDeclaredRecordsPanel
                    id="migrateDeclaredRecordsPanel"
                    source={SourceFlags.OneDrive}
                    treeData={this.settingNode}
                ></MigrateDeclaredRecordsPanel>
                <>
                    <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.cancelMigrateDeclaredRecords} />
                    <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_SP_MigrateDeclaredRecords_MigrateBtn} onClick={this.saveMigrateDeclaredRecords} />
                </>
            </R.Panel>
        );
    }
    
    render() {
        return <div id="rmCRM">
            <section className="crm-header">
                <$g.SiteMap data={[SiteMapLinks.BCM_ContentRepositoryManagement_OD]} />
                <ValidateMessageBar />
                <div className="margin-top-m"><CheckRemoteNodeMessageBar /></div>
                <TopButtonsComponent
                    ref={r => this.refTopButtons = r}
                    data={{ menuBtnItems: [...this.menuBtnItems, ...this.menuBtnItemsInMore] }}
                ></TopButtonsComponent>
            </section>
            <section className="crm-content">
                <div className="ra-crm-splitter-container">
                    <R.Splitter minAsize={SplitterSize.minAsize} minBsize={SplitterSize.minBsize} defaultAsize={SplitterSize.defaultAsize}>
                        <div className="ra-splitter-left">
                            <div className="ra-splitter-header-leftHight">
                                <div className="ra-splitter-header-title" tabIndex="0">{RMResx.RM_JS_SPS_LeftTitle}</div>
                                {this.props.tabControl && <div className="ra-splitter-header-tabcontrol">{this.props.tabControl}</div>}
                            </div>
                            <div className="ra-splitter-searchbox">
                                <R.Searchbox
                                    width={380}
                                    height={34}
                                    placeholder={RMResx.RM_BCM_SearchTxt}
                                    disabled={false}
                                    onSearch={this.onSearch}
                                />
                            </div>
                            <div className="ra-splitter-tree">
                                <SPCRMTree
                                    ref={r => this.refCRMTree = r}
                                    treeSource={SourceFlags.OneDrive}
                                    mode={TabIndex.Records}
                                    searchKey={this.state.searchKey}
                                    onSelectedNodeChanged={this.onTreeChanged}
                                    onNodeRefresh={this.onNodeRefresh}
                                    dataSource={BrowseTreeNodeSourceType.OneDrive}
                                ></SPCRMTree>
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

                            {this.settingNode && this.state.showRightSetting && <div>
                                <GeneralManagementComponent
                                    context={OneDriveGeneralManagement.getContext()}
                                    id="generalManagementComponent"
                                    ref={r => this.refGeneralManagementComponent = r}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    sourceFlag={SourceFlags.OneDrive}
                                ></GeneralManagementComponent>
                            </div>}

                            {this.state.showRightSetting && this.state.enableClassification == EnableRecordManagementSetting.Enable && <div>
                                <DocumentTermSettingComponent
                                    context={OneDriveDocumentTerm.getContext()}
                                    id={this.documentTermSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                    disabled={this.checkEditIsDisabled()}
                                    sourceFlag={SourceFlags.OneDrive}
                                    showTermSettings={this.state.showTermSettings}
                                    availableRules={this.state.availableRules}
                                    refreshRules={this.loadRules}
                                ></DocumentTermSettingComponent>
                                <ManualApprovalSettingComponent
                                    context={OneDriveManualApprovalSetting.getContext()}
                                    id={this.manualApprovalSettingComponent}
                                    refreshNodeSettings={this.refreshNodeSettings}
                                    checkMissingConfig={this.checkMissingConfig}
                                ></ManualApprovalSettingComponent>
                                <ScheduleSettingComponent
                                    context={OneDriveScheduleSetting.getContext()}
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
                        context={OneDriveUniqueIdSetting.getContext()}
                        sourceFlag={SourceFlags.OneDrive}
                    />}
                {this.state.initConvertStubPanel && this.renderConvertStubPanel()}
                {this.state.initMigrateDeclaredRecordsPanel && this.renderMigrateDeclaredRecordsPanel()}
            </section>
        </div>;
    }
}
